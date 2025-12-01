using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace FastORM;

public class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();
    private readonly Dictionary<string, object> _parameters = new();
    private readonly Func<string, string> _quoteFunc;
    private readonly Func<string, string?> _columnMapper;
    private int _paramIndex;

    public SqlExpressionVisitor(Func<string, string> quoteFunc, Func<string, string?> columnMapper, int startParamIndex = 0)
    {
        _quoteFunc = quoteFunc;
        _columnMapper = columnMapper;
        _paramIndex = startParamIndex;
    }

    public string Sql => _builder.ToString();
    public Dictionary<string, object> Parameters => _parameters;
    public int NextParamIndex => _paramIndex;

    public override Expression? Visit(Expression? node)
    {
        if (node == null) return null;
        return base.Visit(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _builder.Append("(");
        Visit(node.Left);
            
        switch (node.NodeType)
        {
            case ExpressionType.Equal: _builder.Append(" = "); break;
            case ExpressionType.NotEqual: _builder.Append(" <> "); break;
            case ExpressionType.GreaterThan: _builder.Append(" > "); break;
            case ExpressionType.GreaterThanOrEqual: _builder.Append(" >= "); break;
            case ExpressionType.LessThan: _builder.Append(" < "); break;
            case ExpressionType.LessThanOrEqual: _builder.Append(" <= "); break;
            case ExpressionType.AndAlso: _builder.Append(" AND "); break;
            case ExpressionType.OrElse: _builder.Append(" OR "); break;
            default: throw new NotSupportedException($"Operator {node.NodeType} not supported");
        }

        Visit(node.Right);
        _builder.Append(")");
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            // Property access: x.Id
            var columnName = _columnMapper(node.Member.Name) ?? node.Member.Name;
            _builder.Append(_quoteFunc(columnName));
            return node;
        }
            
        // Closure variable or constant property
        var value = GetValue(node);
        AddParameter(value);
        return node;
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        AddParameter(node.Value);
        return node;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name == "Contains" && node.Method.DeclaringType == typeof(Enumerable))
        {
            // Enumerable.Contains(list, value) -> value IN (list)
            // First arg is list, second is value.
            Visit(node.Arguments[1]); // Value (column)
            _builder.Append(" IN (");
            var list = GetValue(node.Arguments[0]) as System.Collections.IEnumerable;
            if (list != null)
            {
                bool first = true;
                foreach (var item in list)
                {
                    if (!first) _builder.Append(", ");
                    AddParameter(item);
                    first = false;
                }
            }
            _builder.Append(")");
            return node;
        }
        else if (node.Method.Name == "Contains" && node.Object != null && typeof(System.Collections.IEnumerable).IsAssignableFrom(node.Object.Type) && node.Object.Type != typeof(string))
        {
            // list.Contains(value) -> value IN (list)
            Visit(node.Arguments[0]);
            _builder.Append(" IN (");
            var list = GetValue(node.Object) as System.Collections.IEnumerable;
            if (list != null)
            {
                bool first = true;
                foreach (var item in list)
                {
                    if (!first) _builder.Append(", ");
                    AddParameter(item);
                    first = false;
                }
            }
            _builder.Append(")");
            return node;
        }
             
        throw new NotSupportedException($"Method {node.Method.Name} not supported");
    }

    private void AddParameter(object? value)
    {
        var name = "@p" + _paramIndex++;
        _parameters[name] = value ?? DBNull.Value;
        _builder.Append(name);
    }

    [UnconditionalSuppressMessage("Aot", "IL3050:RequiresDynamicCode", Justification = "Fallback for local expression evaluation.")]
    private object? GetValue(Expression expression)
    {
        if (expression is ConstantExpression c) return c.Value;
        if (expression is MemberExpression m)
        {
            if (m.Expression == null) // Static field/prop
            {
                if (m.Member is System.Reflection.FieldInfo f1) return f1.GetValue(null);
                if (m.Member is System.Reflection.PropertyInfo p1) return p1.GetValue(null);
            }
            else
            {
                var obj = GetValue(m.Expression);
                if (obj == null) return null;
                if (m.Member is System.Reflection.FieldInfo f) return f.GetValue(obj);
                if (m.Member is System.Reflection.PropertyInfo p) return p.GetValue(obj);
            }
        }
        var lambda = Expression.Lambda(expression);
        var func = lambda.Compile();
        return func.DynamicInvoke();
    }
}
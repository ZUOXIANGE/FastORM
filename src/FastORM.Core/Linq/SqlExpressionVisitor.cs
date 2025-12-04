using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace FastORM;

/// <summary>
/// Visitor that translates expression tree to SQL.
/// </summary>
public class SqlExpressionVisitor : ExpressionVisitor
{
    private readonly StringBuilder _builder = new();
    private readonly Dictionary<string, object> _parameters = new();
    private readonly Func<string, string> _quoteFunc;
    private readonly Func<string, string?> _columnMapper;
    private int _paramIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlExpressionVisitor"/> class.
    /// </summary>
    /// <param name="quoteFunc">The quote function.</param>
    /// <param name="columnMapper">The column mapper.</param>
    /// <param name="startParamIndex">The start parameter index.</param>
    public SqlExpressionVisitor(Func<string, string> quoteFunc, Func<string, string?> columnMapper, int startParamIndex = 0)
    {
        _quoteFunc = quoteFunc;
        _columnMapper = columnMapper;
        _paramIndex = startParamIndex;
    }

    /// <summary>
    /// Gets the generated SQL.
    /// </summary>
    public string Sql => _builder.ToString();
    /// <summary>
    /// Gets the parameters.
    /// </summary>
    public Dictionary<string, object> Parameters => _parameters;
    /// <summary>
    /// Gets the next parameter index.
    /// </summary>
    public int NextParamIndex => _paramIndex;

    /// <inheritdoc/>
    public override Expression? Visit(Expression? node)
    {
        if (node == null) return null;
        return base.Visit(node);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        AddParameter(node.Value);
        return node;
    }

    /// <inheritdoc/>
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
        else if (node.Method.Name == "Contains" && node.Object != null && node.Object.Type == typeof(string))
        {
            // String.Contains(value) -> LIKE '%value%'
            Visit(node.Object);
            _builder.Append(" LIKE ");
            var val = GetValue(node.Arguments[0]);
            AddParameter($"%{val}%");
            return node;
        }
        else if (node.Method.Name == "StartsWith" && node.Object != null && node.Object.Type == typeof(string))
        {
            // String.StartsWith(value) -> LIKE 'value%'
            Visit(node.Object);
            _builder.Append(" LIKE ");
            var val = GetValue(node.Arguments[0]);
            AddParameter($"{val}%");
            return node;
        }
        else if (node.Method.Name == "EndsWith" && node.Object != null && node.Object.Type == typeof(string))
        {
            // String.EndsWith(value) -> LIKE '%value'
            Visit(node.Object);
            _builder.Append(" LIKE ");
            var val = GetValue(node.Arguments[0]);
            AddParameter($"%{val}");
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
        // Try to use ValueExtractor first which is AOT-safe(r)
        var result = FastORM.Internal.ValueExtractor.Evaluate(expression);
        return result;
    }
}
using System.Linq.Expressions;
using System.Text;

namespace FastORM.Internal;

// Suppress IL3050: ValueExtractor.Evaluate is annotated with RequiresDynamicCode,
// but we use it for general expression evaluation which is mostly safe.
// The unsafe paths (array creation for unknown types) are edge cases in AOT.
#pragma warning disable IL3050

public static class ExpressionToSql
{
    public static string Translate(Expression? expr, FastDbContext? context, List<object?> parameters)
    {
        if (expr == null) return "";
        // Unwrap Lambda if present
        if (expr is LambdaExpression lambda) expr = lambda.Body;

        var visitor = new SqlVisitor(context, parameters);
        visitor.Visit(expr);
        return visitor.Sql.ToString();
    }

    public static string TranslateQueryPredicates(Expression? expr, FastDbContext? context, List<object?> parameters)
    {
        if (expr == null) return "";
        var visitor = new QueryVisitor(context, parameters);
        visitor.Visit(expr);
        return visitor.WhereBuilder.ToString();
    }

    private class QueryVisitor : ExpressionVisitor
    {
        public StringBuilder WhereBuilder { get; } = new StringBuilder();
        private readonly FastDbContext? _context;
        private readonly List<object?> _parameters;

        public QueryVisitor(FastDbContext? context, List<object?> parameters)
        {
            _context = context;
            _parameters = parameters;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Traverse down the source first (to maintain order of WHERE clauses if needed, though AND is commutative)
            // Actually, LINQ builds the tree outside-in: Where2(Where1(Source, P1), P2)
            // So visiting node.Arguments[0] first means we visit Where1 then Where2.

            if (node.Arguments.Count > 0)
            {
                Visit(node.Arguments[0]);
            }

            if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
            {
                if (node.Method.Name == "Where" && node.Arguments.Count > 1)
                {
                    if (WhereBuilder.Length > 0) WhereBuilder.Append(" AND ");

                    var predicate = node.Arguments[1];
                    // Translate predicate using the existing SqlVisitor logic
                    // We need to unwrap the lambda
                    if (predicate is UnaryExpression ue && ue.Operand is LambdaExpression le) predicate = le.Body;
                    else if (predicate is LambdaExpression le2) predicate = le2.Body;

                    var sqlVisitor = new SqlVisitor(_context, _parameters);
                    sqlVisitor.Visit(predicate);
                    WhereBuilder.Append(sqlVisitor.Sql);
                }
            }
            return node;
        }
    }

    private class SqlVisitor : ExpressionVisitor
    {
        public StringBuilder Sql { get; } = new StringBuilder();
        private readonly FastDbContext? _context;
        private readonly List<object?> _parameters;

        public SqlVisitor(FastDbContext? context, List<object?> parameters)
        {
            _context = context;
            _parameters = parameters;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Not)
            {
                Sql.Append(" NOT (");
                Visit(node.Operand);
                Sql.Append(")");
                return node;
            }
            if (node.NodeType == ExpressionType.Convert)
            {
                // Ignore cast, just visit operand
                return Visit(node.Operand);
            }
            return base.VisitUnary(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            bool isRightNull = node.Right is ConstantExpression cr && cr.Value == null;
            bool isLeftNull = node.Left is ConstantExpression cl && cl.Value == null;

            if ((node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual) && (isRightNull || isLeftNull))
            {
                Sql.Append("(");
                if (isRightNull) Visit(node.Left); else Visit(node.Right);

                if (node.NodeType == ExpressionType.Equal) Sql.Append(" IS NULL");
                else Sql.Append(" IS NOT NULL");

                Sql.Append(")");
                return node;
            }

            Sql.Append("(");
            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.AndAlso: Sql.Append(" AND "); break;
                case ExpressionType.OrElse: Sql.Append(" OR "); break;
                case ExpressionType.Equal: Sql.Append(" = "); break;
                case ExpressionType.NotEqual: Sql.Append(" <> "); break;
                case ExpressionType.GreaterThan: Sql.Append(" > "); break;
                case ExpressionType.GreaterThanOrEqual: Sql.Append(" >= "); break;
                case ExpressionType.LessThan: Sql.Append(" < "); break;
                case ExpressionType.LessThanOrEqual: Sql.Append(" <= "); break;
                default: throw new NotSupportedException($"Operator {node.NodeType} not supported");
            }

            Visit(node.Right);
            Sql.Append(")");
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // Handle method calls like Contains, StartsWith, etc.
            if (node.Method.Name == "Contains" && node.Arguments.Count == 1 && node.Object != null && node.Object.Type == typeof(string))
            {
                Visit(node.Object);
                Sql.Append(" LIKE ");
                HandleLikePattern(node.Arguments[0], "%", "%");
                return node;
            }
            else if (node.Method.Name == "StartsWith" && node.Arguments.Count == 1 && node.Object != null && node.Object.Type == typeof(string))
            {
                Visit(node.Object);
                Sql.Append(" LIKE ");
                HandleLikePattern(node.Arguments[0], "", "%");
                return node;
            }
            else if (node.Method.Name == "EndsWith" && node.Arguments.Count == 1 && node.Object != null && node.Object.Type == typeof(string))
            {
                Visit(node.Object);
                Sql.Append(" LIKE ");
                HandleLikePattern(node.Arguments[0], "%", "");
                return node;
            }

            // Handle Collection Contains (IN clause)
            if (node.Method.Name == "Contains")
            {
                Expression? collection = null;
                Expression? item = null;

                // Case 1: Extension Method (Enumerable.Contains, MemoryExtensions.Contains, etc.)
                // static bool Contains<T>(this IEnumerable<T> source, T value)
                // static bool Contains<T>(this ReadOnlySpan<T> span, T value)
                if (node.Method.IsStatic && node.Arguments.Count == 2)
                {
                    collection = node.Arguments[0];
                    item = node.Arguments[1];
                }
                // Case 2: Instance Method (List<T>.Contains, HashSet<T>.Contains, etc.)
                // bool Contains(T item)
                else if (!node.Method.IsStatic && node.Arguments.Count == 1 && node.Object != null)
                {
                    collection = node.Object;
                    item = node.Arguments[0];
                }

                if (collection != null && item != null)
                {
                    // Ensure collection is NOT parameter dependent (it's a constant list/array)
                    // And verify it implements IEnumerable (so we can iterate it)
                    // Note: For MemoryExtensions, the argument might be an array but treated as Span.
                    // We need to evaluate the collection expression.

                    bool isParamDep = IsParameterDependent(collection);

                    if (!isParamDep)
                    {
                        // Handle implicit conversion to Span/ReadOnlySpan (which cannot be boxed)
                        // If collection is op_Implicit(array), use array directly.
                        Expression effectiveCollection = collection;
                        if (collection is MethodCallExpression mce && mce.Method.Name == "op_Implicit" && mce.Arguments.Count == 1)
                        {
                            // Check if return type is Span-like
                            if (mce.Type.IsValueType && mce.Type.Name.Contains("Span"))
                            {
                                effectiveCollection = mce.Arguments[0];
                            }
                        }

                        var colVal = ValueExtractor.Evaluate(effectiveCollection);

                        if (colVal is System.Collections.IEnumerable enumerable)
                        {
                            Visit(item);
                            Sql.Append(" IN (");
                            bool first = true;
                            foreach (var obj in enumerable)
                            {
                                if (!first) Sql.Append(", ");
                                AddParameter(obj);
                                first = false;
                            }
                            if (first) Sql.Append("NULL"); // Empty list results in IN (NULL) which is safe (false) or handle differently
                            Sql.Append(")");
                            return node;
                        }
                    }
                }
            }

            // Fallback to evaluating value
            var val = ValueExtractor.Evaluate(node);
            AddParameter(val);
            return node;
        }

        private void HandleLikePattern(Expression arg, string prefix, string suffix)
        {
            if (!IsParameterDependent(arg))
            {
                var val = ValueExtractor.Evaluate(arg);
                if (val != null)
                {
                    AddParameter(prefix + val + suffix);
                    return;
                }
            }

            // Fallback to SQL concatenation
            // Check Dialect
            bool usePipe = _context?.Dialect == SqlDialect.Sqlite || _context?.Dialect == SqlDialect.PostgreSql;
            string concatOp = usePipe ? " || " : " + ";

            if (!string.IsNullOrEmpty(prefix))
            {
                Sql.Append("'").Append(prefix).Append("'").Append(concatOp);
            }

            Visit(arg);

            if (!string.IsNullOrEmpty(suffix))
            {
                Sql.Append(concatOp).Append("'").Append(suffix).Append("'");
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // Check if it is a property of the parameter (column) or a closure (value)
            if (IsParameterDependent(node))
            {
                // Column
                string colName = node.Member.Name;
                if (_context != null) colName = _context.Quote(colName);
                Sql.Append(colName);
                return node;
            }
            else
            {
                // Value (Closure)
                var val = ValueExtractor.Evaluate(node);
                AddParameter(val);
                return node;
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AddParameter(node.Value);
            return node;
        }

        private void AddParameter(object? val)
        {
            int idx = _parameters.Count;
            string pName = "@dyn_" + idx;
            _parameters.Add(val ?? DBNull.Value);
            Sql.Append(pName);
        }

        private bool IsParameterDependent(Expression e)
        {
            if (e is ParameterExpression) return true;
            if (e is MemberExpression me) return me.Expression != null && IsParameterDependent(me.Expression);
            // Note: We treat anything NOT dependent on parameter as a value
            return false;
        }
    }
}

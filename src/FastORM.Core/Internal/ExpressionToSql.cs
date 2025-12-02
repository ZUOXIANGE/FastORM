using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

namespace FastORM.Internal
{
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

            protected override Expression VisitBinary(BinaryExpression node)
            {
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
                        AddParameter(prefix + val.ToString() + suffix);
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
                if (e is MemberExpression me) return IsParameterDependent(me.Expression);
                // Note: We treat anything NOT dependent on parameter as a value
                return false;
            }
        }
    }
}

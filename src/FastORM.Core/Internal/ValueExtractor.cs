using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FastORM.Internal
{
    public static class ValueExtractor
    {
        public static List<object?> GetValues(IQueryable query)
        {
            var values = new List<object?>();
            var expr = query.Expression;
            
            // DEBUG
            // System.Console.WriteLine($"[ValueExtractor] Root Expr: {expr}");

            var current = expr;
            while (current is MethodCallExpression mce)
            {
                // System.Console.WriteLine($"[ValueExtractor] Visiting Method: {mce.Method.Name}");
                
                if (mce.Method.Name == "Where")
                {
                    if (mce.Arguments.Count > 1 && mce.Arguments[1] is UnaryExpression ue && ue.Operand is LambdaExpression le)
                    {
                        // System.Console.WriteLine($"[ValueExtractor] Found Where Lambda: {le.Body}");
                        ExtractFromBody(le.Body, values);
                    }
                    else if (mce.Arguments.Count > 1 && mce.Arguments[1] is LambdaExpression le2)
                    {
                         ExtractFromBody(le2.Body, values);
                    }
                }
                
                if (mce.Arguments.Count > 0)
                {
                    current = mce.Arguments[0];
                }
                else
                {
                    break;
                }
            }
            
            // System.Console.WriteLine($"[ValueExtractor] Extracted {values.Count} values");
             return values;
         }

        private static void ExtractFromBody(Expression body, List<object?> values)
        {
            // System.Console.WriteLine($"[ValueExtractor] Extracting from: {body}");
            
            if (body is BinaryExpression be)
            {
                if (be.NodeType == ExpressionType.AndAlso || be.NodeType == ExpressionType.OrElse)
                {
                    // It's a composite (And/Or). Visit children.
                    // Order: Left then Right
                    ExtractFromBody(be.Left, values);
                    ExtractFromBody(be.Right, values);
                }
                else
                {
                    // It's a leaf (Equality, Comparison, etc.)
                    // Extract value from the side that is NOT parameter dependent
                    if (!IsParameterDependent(be.Right))
                    {
                        values.Add(Evaluate(be.Right));
                    }
                    else if (!IsParameterDependent(be.Left))
                    {
                        values.Add(Evaluate(be.Left));
                    }
                }
            }
            else if (body is MethodCallExpression mce)
            {
                // Handle "Like", "Contains", etc.
                // FastORM supports: Contains, StartsWith, EndsWith -> Like
                if (mce.Method.Name == "Contains" || mce.Method.Name == "StartsWith" || mce.Method.Name == "EndsWith")
                {
                     // System.Console.WriteLine($"[ValueExtractor] Found Like Method: {mce.Method.Name}");
                     
                     if (mce.Object != null && !IsParameterDependent(mce.Object))
                     {
                         // Instance method where Object is the value (unlikely for String.Contains(p.Prop), but possible for List.Contains(p.Prop))
                         // Actually List.Contains is usually extension or instance.
                         values.Add(Evaluate(mce.Object));
                     }
                     else if (mce.Object != null && !IsParameterDependent(mce.Arguments[0]))
                     {
                         // Instance method: Prop.Contains(Value)
                         values.Add(Evaluate(mce.Arguments[0]));
                     }
                     else if (mce.Arguments.Count > 1)
                     {
                         // Extension method
                         // Find which one is the value
                         if (!IsParameterDependent(mce.Arguments[0]))
                             values.Add(Evaluate(mce.Arguments[0]));
                         else if (!IsParameterDependent(mce.Arguments[1]))
                             values.Add(Evaluate(mce.Arguments[1]));
                     }
                }
            }
            // Handle Unary (Not)
            else if (body is UnaryExpression ue && ue.NodeType == ExpressionType.Not)
            {
                ExtractFromBody(ue.Operand, values);
            }
        }

        private static bool IsParameterDependent(Expression? e)
        {
            if (e == null) return false;
            if (e is ParameterExpression) return true;
            if (e is MemberExpression me) return IsParameterDependent(me.Expression);
            if (e is UnaryExpression ue) return IsParameterDependent(ue.Operand);
            if (e is BinaryExpression be) return IsParameterDependent(be.Left) || IsParameterDependent(be.Right);
            if (e is MethodCallExpression mce) 
                return IsParameterDependent(mce.Object) || mce.Arguments.Any(IsParameterDependent);
            if (e is LambdaExpression le) return IsParameterDependent(le.Body);
            return false;
        }

        public static object? Evaluate(Expression? e)
        {
            if (e == null) return null;
            
            if (e is ConstantExpression ce) return ce.Value;
            
            if (e is MemberExpression me)
            {
                // Recursively evaluate the object instance
                // e.g., closureClass.field
                var obj = Evaluate(me.Expression);
                
                if (me.Member is FieldInfo fi)
                {
                    if (fi.IsStatic) return fi.GetValue(null);
                    if (obj == null) return null; 
                    return fi.GetValue(obj);
                }
                else if (me.Member is PropertyInfo pi)
                {
                    if (pi.GetMethod?.IsStatic == true) return pi.GetValue(null);
                    if (obj == null) return null;
                    return pi.GetValue(obj);
                }
            }
            
            // Optimization: We DO NOT support Compile() anymore to maintain AOT compatibility.
            return null;
        }
    }
}

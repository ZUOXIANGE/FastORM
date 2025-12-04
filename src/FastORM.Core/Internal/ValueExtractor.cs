using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace FastORM.Internal;

public static class ValueExtractor
{
    [RequiresDynamicCode("Value extraction may require dynamic code generation for array creation.")]
    public static List<object?> GetValues(IQueryable query)
    {
        var values = new List<object?>();
        var expr = query.Expression;

        var current = expr;
        while (current is MethodCallExpression mce)
        {
            if (mce.Method.Name == "Where" ||
                mce.Method.Name == "Any" ||
                mce.Method.Name == "All" ||
                mce.Method.Name == "Count" ||
                mce.Method.Name == "LongCount" ||
                mce.Method.Name == "First" ||
                mce.Method.Name == "FirstOrDefault" ||
                mce.Method.Name == "Single" ||
                mce.Method.Name == "SingleOrDefault" ||
                mce.Method.Name == "Last" ||
                mce.Method.Name == "LastOrDefault")
            {
                if (mce.Arguments.Count > 1 && mce.Arguments[1] is UnaryExpression ue && ue.Operand is LambdaExpression le)
                {
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

        return values;
    }

    [RequiresDynamicCode("Value extraction may require dynamic code generation for array creation.")]
    public static List<object?> GetValues(Expression expression)
    {
        var values = new List<object?>();
        // If expression is Lambda, use Body
        if (expression is LambdaExpression le)
        {
            ExtractFromBody(le.Body, values);
        }
        else
        {
            ExtractFromBody(expression, values);
        }
        return values;
    }

    [RequiresDynamicCode("Value extraction may require dynamic code generation for array creation.")]
    private static void ExtractFromBody(Expression body, List<object?> values)
    {
        if (body is BlockExpression block)
        {
            foreach (var stmt in block.Expressions)
            {
                ExtractFromBody(stmt, values);
            }
        }
        else if (body.NodeType == ExpressionType.Assign && body is BinaryExpression assign)
        {
            // Assignment: Left = Right
            // We want the value of Right
            if (!IsParameterDependent(assign.Right))
            {
                values.Add(Evaluate(assign.Right));
            }
        }
        else if (body is BinaryExpression be)
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

    [RequiresDynamicCode("Value extraction may require dynamic code generation for array creation.")]
    public static object? Evaluate(Expression? e)
    {
        if (e == null) return null;

        if (e is ConstantExpression ce) return ce.Value;

        if (e is UnaryExpression ue && ue.NodeType == ExpressionType.Convert)
        {
            return Evaluate(ue.Operand);
        }

        if (e is MemberExpression me)
        {
            // Recursively evaluate the object instance
            // e.g., closureClass.field
            var obj = Evaluate(me.Expression);

            if (me.Member is FieldInfo fi)
            {
                if (fi.IsStatic) return fi.GetValue(null);
                if (obj == null)
                {
                    return null;
                }
                return fi.GetValue(obj);
            }
            else if (me.Member is PropertyInfo pi)
            {
                if (pi.GetMethod?.IsStatic == true) return pi.GetValue(null);
                if (obj == null)
                {
                    return null;
                }
                return pi.GetValue(obj);
            }
        }

        if (e is NewArrayExpression nae && nae.NodeType == ExpressionType.NewArrayInit)
        {
            var elemType = nae.Type.GetElementType();
            if (elemType == null) return null;

            Array array;
            int count = nae.Expressions.Count;

            if (elemType == typeof(int)) array = new int[count];
            else if (elemType == typeof(string)) array = new string[count];
            else if (elemType == typeof(long)) array = new long[count];
            else if (elemType == typeof(Guid)) array = new Guid[count];
            else if (elemType == typeof(bool)) array = new bool[count];
            else if (elemType == typeof(double)) array = new double[count];
            else if (elemType == typeof(decimal)) array = new decimal[count];
            else if (elemType == typeof(float)) array = new float[count];
            else if (elemType == typeof(DateTime)) array = new DateTime[count];
            else if (elemType == typeof(byte)) array = new byte[count];
            else
            {
                // Fallback for other types
                array = Array.CreateInstance(elemType, count);
            }

            for (int i = 0; i < nae.Expressions.Count; i++)
            {
                array.SetValue(Evaluate(nae.Expressions[i]), i);
            }
            return array;
        }

        if (e is ListInitExpression lie)
        {
            object? list;
            if (lie.NewExpression.Constructor != null)
            {
                var args = new object?[lie.NewExpression.Arguments.Count];
                for (int i = 0; i < lie.NewExpression.Arguments.Count; i++)
                {
                    args[i] = Evaluate(lie.NewExpression.Arguments[i]);
                }
                list = lie.NewExpression.Constructor.Invoke(args);
            }
            else
            {
#pragma warning disable IL2072
                list = Activator.CreateInstance(lie.NewExpression.Type);
#pragma warning restore IL2072
            }

            if (list != null)
            {
                foreach (var init in lie.Initializers)
                {
                    if (init.Arguments.Count == 1)
                    {
                        init.AddMethod.Invoke(list, new[] { Evaluate(init.Arguments[0]) });
                    }
                }
            }
            return list;
        }

        if (e is MethodCallExpression mce)
        {
            // Evaluate object (if instance method)
            object? obj = null;
            if (mce.Object != null)
            {
                obj = Evaluate(mce.Object);
                if (obj == null && !mce.Method.IsStatic) return null; // Instance is null
            }

            // Evaluate arguments
            var args = new object?[mce.Arguments.Count];
            for (int i = 0; i < mce.Arguments.Count; i++)
            {
                args[i] = Evaluate(mce.Arguments[i]);
            }

            // Invoke
            try
            {
                return mce.Method.Invoke(obj, args);
            }
            catch
            {
                return null;
            }
        }

        // Optimization: We DO NOT support Compile() anymore to maintain AOT compatibility.
        return null;
    }
}

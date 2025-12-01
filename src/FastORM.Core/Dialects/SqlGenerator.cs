using System.Linq.Expressions;
using System.Text;

namespace FastORM;

public class SqlGenerator : ISqlGenerator
{
    public virtual SqlResult GetSelectSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate = null)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ");
        sb.Append(string.Join(", ", metadata.Columns.Select(Quote)));
        sb.Append(" FROM ").Append(Quote(metadata.TableName));
             
        var result = new SqlResult();
        if (predicate != null)
        {
            var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName);
            visitor.Visit(predicate);
            sb.Append(" WHERE ").Append(visitor.Sql);
            result.Parameters = visitor.Parameters;
        }
        result.Sql = sb.ToString();
        return result;
    }

    public virtual SqlResult GetUpdateSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate, Expression<Func<T, object>> updateExpression)
    {
        var sb = new StringBuilder();
        sb.Append("UPDATE ").Append(Quote(metadata.TableName)).Append(" SET ");
            
        var result = new SqlResult();
        int paramIndex = 0;
            
        var body = updateExpression.Body;
        if (body.NodeType == ExpressionType.Convert)
        {
            body = ((UnaryExpression)body).Operand;
        }

        if (body is NewExpression ne && ne.Members != null)
        {
            for (int i = 0; i < ne.Members.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                var propName = ne.Members[i].Name;
                var colName = metadata.GetColumnName(propName) ?? propName;
                sb.Append(Quote(colName)).Append(" = ");
                    
                var arg = ne.Arguments[i];
                var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName, paramIndex);
                visitor.Visit(arg);
                sb.Append(visitor.Sql);
                paramIndex = visitor.NextParamIndex;
                    
                foreach(var kv in visitor.Parameters) result.Parameters[kv.Key] = kv.Value;
            }
        }
        else if (body is MemberInitExpression mie)
        {
            bool first = true;
            foreach (var binding in mie.Bindings)
            {
                if (binding is MemberAssignment ma)
                {
                    if (!first) sb.Append(", ");
                    var propName = ma.Member.Name;
                    var colName = metadata.GetColumnName(propName) ?? propName;
                    sb.Append(Quote(colName)).Append(" = ");
                        
                    var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName, paramIndex);
                    visitor.Visit(ma.Expression);
                    sb.Append(visitor.Sql);
                    paramIndex = visitor.NextParamIndex;

                    foreach(var kv in visitor.Parameters) result.Parameters[kv.Key] = kv.Value;
                    first = false;
                }
            }
        }
        else
        {
            throw new NotSupportedException("Update expression must be a new anonymous object creation or member initialization.");
        }

        if (predicate != null)
        {
            var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName, paramIndex);
            visitor.Visit(predicate);
            sb.Append(" WHERE ").Append(visitor.Sql);
            foreach(var kv in visitor.Parameters) result.Parameters[kv.Key] = kv.Value;
        }
            
        result.Sql = sb.ToString();
        return result;
    }

    public virtual SqlResult GetDeleteSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate)
    {
        var sb = new StringBuilder();
        sb.Append("DELETE FROM ").Append(Quote(metadata.TableName));
            
        var result = new SqlResult();
        if (predicate != null)
        {
            var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName);
            visitor.Visit(predicate);
            sb.Append(" WHERE ").Append(visitor.Sql);
            result.Parameters = visitor.Parameters;
        }
        result.Sql = sb.ToString();
        return result;
    }

    public virtual SqlResult GetInsertSql<T>(IEntityMetadata<T> metadata, IEnumerable<T> entities)
    {
        var entityList = entities as IList<T> ?? entities.ToList();
        if (entityList.Count == 0) return new SqlResult();

        var sb = new StringBuilder();
        var result = new SqlResult();

        var columnNamesStr = string.Join(",", metadata.Columns.Select(Quote));
        sb.Append("INSERT INTO ").Append(Quote(metadata.TableName)).Append(" (").Append(columnNamesStr).Append(") VALUES ");

        int parameterIndex = 0;
        for (int i = 0; i < entityList.Count; i++)
        {
            var entity = entityList[i];
            sb.Append("(");
            for (int j = 0; j < metadata.Columns.Length; j++)
            {
                var parameterName = "@p" + parameterIndex++;
                sb.Append(parameterName);
                    
                result.Parameters[parameterName] = metadata.GetValue(entity, j) ?? DBNull.Value;

                if (j < metadata.Columns.Length - 1) sb.Append(",");
            }
            sb.Append(")");
            if (i < entityList.Count - 1) sb.Append(",");
        }
            
        result.Sql = sb.ToString();
        return result;
    }

    public virtual SqlResult GetExistsSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT 1 FROM ").Append(Quote(metadata.TableName));
             
        var result = new SqlResult();
        if (predicate != null)
        {
            var visitor = new SqlExpressionVisitor(Quote, metadata.GetColumnName);
            visitor.Visit(predicate);
            sb.Append(" WHERE ").Append(visitor.Sql);
            result.Parameters = visitor.Parameters;
        }
        // Usually we want "SELECT CASE WHEN EXISTS (...) THEN 1 ELSE 0 END" or just the query to use with ExecuteScalar
        // But usually Exists checks if any row is returned. 
        // Standard implementation often returns the select query, and caller uses ExecuteScalar or Reader.
        // Let's adhere to a simple SELECT 1 ... WHERE ... LIMIT 1 (if supported) or just the query.
        // Since this is generic, let's just return the query.
        // However, to be efficient, we might want TOP 1 / LIMIT 1. 
        // But base SqlGenerator tries to be standard. Let's just do SELECT 1 ...
             
        result.Sql = sb.ToString();
        return result;
    }

    public virtual string Quote(string identifier) => identifier;
}
using System.Linq.Expressions;

namespace FastORM;

public interface ISqlGenerator
{
    SqlResult GetSelectSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate = null);

    SqlResult GetUpdateSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate, Expression<Func<T, object>> updateExpression);

    SqlResult GetDeleteSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate);

    SqlResult GetInsertSql<T>(IEntityMetadata<T> metadata, IEnumerable<T> entities);

    SqlResult GetExistsSql<T>(IEntityMetadata<T> metadata, Expression<Func<T, bool>>? predicate);
}

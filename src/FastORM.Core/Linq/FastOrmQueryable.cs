using System.Collections;
using System.Linq.Expressions;

namespace FastORM;

public sealed class FastOrmQueryable<T> : IQueryable<T>, IOrderedQueryable<T>
{
    public FastDbContext Context { get; }

    public string TableName { get; }

    public Expression Expression { get; }

    public Type ElementType => typeof(T);

    public IQueryProvider Provider { get; }

    public FastOrmQueryable(FastDbContext context, string tableName)
    {
        Context = context; TableName = tableName; Expression = Expression.Constant(this); Provider = new FastOrmQueryProvider();
    }

    internal FastOrmQueryable(FastDbContext context, string tableName, Expression expression)
    {
        Context = context; TableName = tableName; Expression = expression; Provider = new FastOrmQueryProvider();
    }

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("FastORM: enumerate via AsCompilable()");

    public IEnumerator<T> GetEnumerator() => throw new NotSupportedException("FastORM: enumerate via AsCompilable()");
}

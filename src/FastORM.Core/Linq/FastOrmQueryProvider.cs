using System.Linq.Expressions;

namespace FastORM;

internal sealed class FastOrmQueryProvider : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotSupportedException("FastORM: create root via FastDbContext");
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new FastOrmQueryable<TElement>(null!, null!, expression);
    }

    public object? Execute(Expression expression) => throw new NotSupportedException("FastORM: execute via AsCompilable()");
    
    public TResult Execute<TResult>(Expression expression) => throw new NotSupportedException("FastORM: execute via AsCompilable()");
}

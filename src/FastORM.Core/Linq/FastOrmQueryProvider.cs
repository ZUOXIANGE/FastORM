using System.Linq.Expressions;

namespace FastORM;

internal sealed class FastOrmQueryProvider : IQueryProvider
{
    private readonly FastDbContext _context;
    private readonly string _tableName;

    public FastOrmQueryProvider(FastDbContext context, string tableName)
    {
        _context = context;
        _tableName = tableName;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        throw new NotSupportedException("FastORM: create root via FastDbContext");
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new FastOrmQueryable<TElement>(_context, _tableName, expression);
    }

    public object? Execute(Expression expression) => throw new NotSupportedException("FastORM: execute via AsCompilable()");
    
    public TResult Execute<TResult>(Expression expression) => throw new NotSupportedException("FastORM: execute via AsCompilable()");
}

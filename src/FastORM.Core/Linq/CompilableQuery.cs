namespace FastORM;

public sealed class CompilableQuery<T>
{
    public FastDbContext Context { get; }
    public string TableName { get; }

    public CompilableQuery(FastDbContext context, string tableName)
    {
        Context = context;
        TableName = tableName;
    }
}

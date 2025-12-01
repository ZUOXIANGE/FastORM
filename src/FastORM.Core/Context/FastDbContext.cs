using System.Data.Common;

namespace FastORM;

public partial class FastDbContext
{
    public DbConnection Connection { get; }

    public SqlDialect Dialect { get; }

    public ISqlGenerator SqlGenerator { get; }

    /// <summary>
    /// 自定义 SQL 日志输出委托
    /// </summary>
    public Action<string>? SqlLogger { get; set; }

    /// <summary>
    /// 获取当前活动的事务
    /// </summary>
    public DbTransaction? CurrentTransaction { get; private set; }

    public FastDbContext(DbConnection connection, SqlDialect dialect)
    {
        Connection = connection;
        Dialect = dialect;
        SqlGenerator = dialect switch
        {
            SqlDialect.SqlServer => new SqlServerGenerator(),
            SqlDialect.MySql => new MySqlGenerator(),
            SqlDialect.PostgreSql => new PostgreSqlGenerator(),
            SqlDialect.Sqlite => new SqliteGenerator(),
            _ => new SqlGenerator()
        };
    }

    public DbCommand CreateCommand()
    {
        var cmd = Connection.CreateCommand();
        if (CurrentTransaction != null)
        {
            cmd.Transaction = CurrentTransaction;
        }
        return cmd;
    }

    /// <summary>
    /// 开启一个新事务。
    /// </summary>
    public DbTransaction BeginTransaction()
    {
        if (Connection.State != System.Data.ConnectionState.Open) Connection.Open();
        CurrentTransaction = Connection.BeginTransaction();
        return CurrentTransaction;
    }

    /// <summary>
    /// 异步开启一个新事务。
    /// </summary>
    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Connection.State != System.Data.ConnectionState.Open) await Connection.OpenAsync(cancellationToken);
        CurrentTransaction = await Connection.BeginTransactionAsync(cancellationToken);
        return CurrentTransaction;
    }

    /// <summary>
    /// 提交当前事务。
    /// </summary>
    public void Commit()
    {
        CurrentTransaction?.Commit();
        CurrentTransaction?.Dispose();
        CurrentTransaction = null;
    }

    /// <summary>
    /// 异步提交当前事务。
    /// </summary>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
            await CurrentTransaction.DisposeAsync();
            CurrentTransaction = null;
        }
    }

    /// <summary>
    /// 回滚当前事务。
    /// </summary>
    public void Rollback()
    {
        CurrentTransaction?.Rollback();
        CurrentTransaction?.Dispose();
        CurrentTransaction = null;
    }

    /// <summary>
    /// 异步回滚当前事务。
    /// </summary>
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
            await CurrentTransaction.DisposeAsync();
            CurrentTransaction = null;
        }
    }

    /// <summary>
    /// 根据当前数据库方言对标识符（如表名、列名）进行转义。
    /// </summary>
    /// <param name="identifier">要转义的数据库标识符。</param>
    /// <returns>转义后的标识符字符串。</returns>
    public string Quote(string identifier)
    {
        if (string.IsNullOrEmpty(identifier)) return identifier;
        switch (Dialect)
        {
            case SqlDialect.SqlServer: return "[" + identifier + "]";
            case SqlDialect.MySql: return "`" + identifier + "`";
            case SqlDialect.PostgreSql: return "\"" + identifier.ToLowerInvariant() + "\"";
            default: return "\"" + identifier + "\"";
        }
    }
}

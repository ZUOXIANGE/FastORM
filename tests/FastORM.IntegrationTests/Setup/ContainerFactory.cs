using Testcontainers.MsSql;
using Testcontainers.MySql;
using Testcontainers.PostgreSql;

namespace FastORM.IntegrationTests.Setup;

public static class ContainerFactory
{
    private static PostgreSqlContainer? _postgreSql;
    private static readonly SemaphoreSlim _pgLock = new(1, 1);

    private static MySqlContainer? _mySql;
    private static readonly SemaphoreSlim _myLock = new(1, 1);

    private static MsSqlContainer? _msSql;
    private static readonly SemaphoreSlim _msLock = new(1, 1);

    public static async Task<PostgreSqlContainer> GetPostgreSqlAsync()
    {
        if (_postgreSql != null) return _postgreSql;
        await _pgLock.WaitAsync();
        try
        {
            if (_postgreSql != null) return _postgreSql;
            var c = new PostgreSqlBuilder()
                .WithDatabase("fastorm")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();
            await c.StartAsync();
            _postgreSql = c;
            return c;
        }
        finally { _pgLock.Release(); }
    }

    public static async Task<MySqlContainer> GetMySqlAsync()
    {
        if (_mySql != null) return _mySql;
        await _myLock.WaitAsync();
        try
        {
            if (_mySql != null) return _mySql;
            var c = new MySqlBuilder()
                .WithDatabase("fastorm")
                .WithUsername("mysql")
                .WithPassword("mysql")
                .Build();
            await c.StartAsync();
            _mySql = c;
            return c;
        }
        finally { _myLock.Release(); }
    }

    public static async Task<MsSqlContainer> GetMsSqlAsync()
    {
        if (_msSql != null) return _msSql;
        await _msLock.WaitAsync();
        try
        {
            if (_msSql != null) return _msSql;
            var c = new MsSqlBuilder()
                .WithPassword("yourStrong(!)Password")
                .Build();
            await c.StartAsync();
            _msSql = c;
            return c;
        }
        finally { _msLock.Release(); }
    }
}

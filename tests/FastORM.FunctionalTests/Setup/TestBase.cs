using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;

namespace FastORM.FunctionalTests.Setup;

/// <summary>
/// 功能测试基类
/// 负责初始化 SQLite 内存数据库环境
/// </summary>
public abstract class TestBase
{
    protected SqliteConnection Connection { get; private set; } = null!;
    protected FunctionalTestDbContext Context { get; private set; } = null!;

    [Before(Test)]
    public async Task Setup()
    {
        // 使用内存数据库
        Connection = new SqliteConnection("DataSource=:memory:");
        await Connection.OpenAsync();
        
        Context = new FunctionalTestDbContext(Connection, SqlDialect.Sqlite);
        
        // 初始化表结构
        await CreateTablesAsync();
    }

    [After(Test)]
    public async Task Teardown()
    {
        if (Context != null)
        {
            // Context 不拥有 Connection，所以 Dispose 不会关闭 Connection
            // 但我们需要释放 Context 资源
            // FastDbContext 没有显式的 Dispose 方法释放非托管资源，主要是释放连接引用
        }
        
        if (Connection != null)
        {
            await Connection.CloseAsync();
            await Connection.DisposeAsync();
        }
    }

    private async Task CreateTablesAsync()
    {
        using var cmd = Connection.CreateCommand();
        // 创建 Users, Orders, Items 表
        cmd.CommandText = @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                Name TEXT, 
                Age INTEGER
            );

            CREATE TABLE Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                UserId INTEGER, 
                Amount DECIMAL, 
                OrderDate TEXT
            );

            CREATE TABLE Items (
                Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                OrderId INTEGER, 
                Name TEXT, 
                Price DECIMAL
            );

            CREATE TABLE Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT,
                Price DECIMAL,
                CreatedAt TEXT,
                IsActive INTEGER,
                CategoryId TEXT
            );
        ";
        await cmd.ExecuteNonQueryAsync();
    }
}

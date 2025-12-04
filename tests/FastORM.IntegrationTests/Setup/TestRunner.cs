using System.Data;
using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;

namespace FastORM.IntegrationTests.Setup;

public static class TestRunner
{
    private static readonly SemaphoreSlim _pgLock = new(1, 1);
    private static readonly SemaphoreSlim _mySqlLock = new(1, 1);
    private static readonly SemaphoreSlim _msSqlLock = new(1, 1);

    public static async Task RunOnPostgreSql(Func<IntegrationTestDbContext, Task> testAction)
    {
        await _pgLock.WaitAsync();
        try
        {
            var container = await ContainerFactory.GetPostgreSqlAsync();
            // Do NOT dispose container here, it is shared.
            
            await using var conn = new NpgsqlConnection(container.GetConnectionString());
            await conn.OpenAsync();
            
            // Reset database state
            await DbSetup.ResetDatabaseAsync(conn, SqlDialect.PostgreSql);
            
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            await DbSetup.SeedDataAsync(ctx);
            
            await testAction(ctx);
        }
        finally
        {
            _pgLock.Release();
        }
    }

    public static async Task RunOnMySql(Func<IntegrationTestDbContext, Task> testAction)
    {
        await _mySqlLock.WaitAsync();
        try
        {
            var container = await ContainerFactory.GetMySqlAsync();
            
            await using var conn = new MySqlConnection(container.GetConnectionString());
            await conn.OpenAsync();
            
            await DbSetup.ResetDatabaseAsync(conn, SqlDialect.MySql);
            
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.MySql);
            await DbSetup.SeedDataAsync(ctx);
            
            await testAction(ctx);
        }
        finally
        {
            _mySqlLock.Release();
        }
    }

    public static async Task RunOnSqlServer(Func<IntegrationTestDbContext, Task> testAction)
    {
        await _msSqlLock.WaitAsync();
        try
        {
            var container = await ContainerFactory.GetMsSqlAsync();
            
            await using var conn = new SqlConnection(container.GetConnectionString());
            await conn.OpenAsync();
            
            await DbSetup.ResetDatabaseAsync(conn, SqlDialect.SqlServer);
            
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.SqlServer);
            await DbSetup.SeedDataAsync(ctx);
            
            await testAction(ctx);
        }
        finally
        {
            _msSqlLock.Release();
        }
    }

    public static async Task RunOnSqlite(Func<IntegrationTestDbContext, Task> testAction)
    {
        // Sqlite in-memory is unique per connection. 
        // Shared cache mode could allow sharing but here we want isolation or fresh start.
        // "Data Source=:memory:" creates a fresh DB each time we open a connection?
        // Yes, unless "Mode=Shared" is used.
        // So we just open it, create schema, run test, close it (destroying DB).
        
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        
        await DbSetup.CreateSchemaAsync(conn, SqlDialect.Sqlite);
        
        var ctx = new IntegrationTestDbContext(conn, SqlDialect.Sqlite);
        await DbSetup.SeedDataAsync(ctx);
        
        await testAction(ctx);
    }
}

public static class DbSetup
{
    public static async Task ResetDatabaseAsync(IDbConnection conn, SqlDialect dialect)
    {
        await DropSchemaAsync(conn, dialect);
        await CreateSchemaAsync(conn, dialect);
    }

    public static async Task DropSchemaAsync(IDbConnection conn, SqlDialect dialect)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = GetDropItemsTableScript(dialect);
        await Task.Run(() => cmd.ExecuteNonQuery());

        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = GetDropOrdersTableScript(dialect);
        await Task.Run(() => cmd2.ExecuteNonQuery());
        
        using var cmd3 = conn.CreateCommand();
        cmd3.CommandText = GetDropUsersTableScript(dialect);
        await Task.Run(() => cmd3.ExecuteNonQuery());

        using var cmd4 = conn.CreateCommand();
        cmd4.CommandText = GetDropSupportedTypesTableScript(dialect);
        await Task.Run(() => cmd4.ExecuteNonQuery());
    }

    public static async Task CreateSchemaAsync(IDbConnection conn, SqlDialect dialect)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = GetCreateUsersTableScript(dialect);
        await Task.Run(() => cmd.ExecuteNonQuery());

        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = GetCreateOrdersTableScript(dialect);
        await Task.Run(() => cmd2.ExecuteNonQuery());
        
        using var cmd3 = conn.CreateCommand();
        cmd3.CommandText = GetCreateItemsTableScript(dialect);
        await Task.Run(() => cmd3.ExecuteNonQuery());

        using var cmd4 = conn.CreateCommand();
        cmd4.CommandText = GetCreateSupportedTypesTableScript(dialect);
        await Task.Run(() => cmd4.ExecuteNonQuery());
    }

    public static async Task SeedDataAsync(IntegrationTestDbContext ctx)
    {
        // Seed Users
        await ctx.InsertAsync(new[] {
            new User { Id = 1, Name = "Alice", Age = 30 },
            new User { Id = 2, Name = "Bob", Age = 17 },
            new User { Id = 3, Name = "Carol", Age = 22 },
            new User { Id = 4, Name = "Dave", Age = 40 },
            new User { Id = 5, Name = "Eve", Age = 25 }
        });

        // Seed Orders
        await ctx.InsertAsync(new[] {
            new Order { Id = 101, UserId = 1, Amount = 100.50m },
            new Order { Id = 102, UserId = 1, Amount = 200.00m },
            new Order { Id = 103, UserId = 2, Amount = 50.00m },
            new Order { Id = 104, UserId = 4, Amount = 1000.00m }
        });

        // Seed Items
        await ctx.InsertAsync(new[] {
            new Item { Id = 1, CategoryId = 1 },
            new Item { Id = 2, CategoryId = 1 },
            new Item { Id = 3, CategoryId = 2 },
            new Item { Id = 4, CategoryId = 2 },
            new Item { Id = 5, CategoryId = 2 },
            new Item { Id = 6, CategoryId = 3 }
        });
    }

    private static string GetDropUsersTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "IF OBJECT_ID(N'users', N'U') IS NOT NULL DROP TABLE [users];",
        SqlDialect.MySql => "DROP TABLE IF EXISTS `users`;",
        SqlDialect.PostgreSql => "DROP TABLE IF EXISTS \"users\";",
        SqlDialect.Sqlite => "DROP TABLE IF EXISTS \"users\";",
        _ => throw new NotImplementedException()
    };

    private static string GetDropOrdersTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "IF OBJECT_ID(N'orders', N'U') IS NOT NULL DROP TABLE [orders];",
        SqlDialect.MySql => "DROP TABLE IF EXISTS `orders`;",
        SqlDialect.PostgreSql => "DROP TABLE IF EXISTS \"orders\";",
        SqlDialect.Sqlite => "DROP TABLE IF EXISTS \"orders\";",
        _ => throw new NotImplementedException()
    };

    private static string GetDropItemsTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "IF OBJECT_ID(N'items', N'U') IS NOT NULL DROP TABLE [items];",
        SqlDialect.MySql => "DROP TABLE IF EXISTS `items`;",
        SqlDialect.PostgreSql => "DROP TABLE IF EXISTS \"items\";",
        SqlDialect.Sqlite => "DROP TABLE IF EXISTS \"items\";",
        _ => throw new NotImplementedException()
    };

    private static string GetCreateUsersTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "CREATE TABLE [users]([Id] INT PRIMARY KEY, [Name] NVARCHAR(100), [Age] INT);",
        SqlDialect.MySql => "CREATE TABLE `users`(`Id` INT PRIMARY KEY, `Name` VARCHAR(100), `Age` INT);",
        SqlDialect.PostgreSql => "CREATE TABLE \"users\"(\"id\" INT PRIMARY KEY, \"name\" TEXT, \"age\" INT);",
        SqlDialect.Sqlite => "CREATE TABLE \"users\"(\"Id\" INTEGER PRIMARY KEY, \"Name\" TEXT, \"Age\" INTEGER);",
        _ => throw new NotImplementedException()
    };

    private static string GetCreateOrdersTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "CREATE TABLE [orders]([Id] INT PRIMARY KEY, [UserId] INT, [Amount] DECIMAL(18,2));",
        SqlDialect.MySql => "CREATE TABLE `orders`(`Id` INT PRIMARY KEY, `UserId` INT, `Amount` DECIMAL(18,2));",
        SqlDialect.PostgreSql => "CREATE TABLE \"orders\"(\"id\" INT PRIMARY KEY, \"userid\" INT, \"amount\" DECIMAL(18,2));",
        SqlDialect.Sqlite => "CREATE TABLE \"orders\"(\"Id\" INTEGER PRIMARY KEY, \"UserId\" INTEGER, \"Amount\" REAL);",
        _ => throw new NotImplementedException()
    };

    private static string GetCreateItemsTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "CREATE TABLE [items]([Id] INT PRIMARY KEY, [CategoryId] INT);",
        SqlDialect.MySql => "CREATE TABLE `items`(`Id` INT PRIMARY KEY, `CategoryId` INT);",
        SqlDialect.PostgreSql => "CREATE TABLE \"items\"(\"id\" INT PRIMARY KEY, \"categoryid\" INT);",
        SqlDialect.Sqlite => "CREATE TABLE \"items\"(\"Id\" INTEGER PRIMARY KEY, \"CategoryId\" INTEGER);",
        _ => throw new NotImplementedException()
    };

    private static string GetDropSupportedTypesTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "IF OBJECT_ID(N'supported_types', N'U') IS NOT NULL DROP TABLE [supported_types];",
        SqlDialect.MySql => "DROP TABLE IF EXISTS `supported_types`;",
        SqlDialect.PostgreSql => "DROP TABLE IF EXISTS \"supported_types\";",
        SqlDialect.Sqlite => "DROP TABLE IF EXISTS \"supported_types\";",
        _ => throw new NotImplementedException()
    };

    private static string GetCreateSupportedTypesTableScript(SqlDialect dialect) => dialect switch
    {
        SqlDialect.SqlServer => "CREATE TABLE [supported_types]([Id] INT PRIMARY KEY IDENTITY(1,1), [StringProp] NVARCHAR(100), [IntProp] INT, [LongProp] BIGINT, [DecimalProp] DECIMAL(18,2), [DoubleProp] FLOAT, [BoolProp] BIT, [DateTimeProp] DATETIME2, [GuidProp] UNIQUEIDENTIFIER, [DateOnlyProp] DATE, [TimeOnlyProp] TIME, [DateTimeOffsetProp] DATETIMEOFFSET, [EnumProp] INT);",
            SqlDialect.MySql => "CREATE TABLE `supported_types`(`Id` INT PRIMARY KEY AUTO_INCREMENT, `StringProp` VARCHAR(100), `IntProp` INT, `LongProp` BIGINT, `DecimalProp` DECIMAL(18,2), `DoubleProp` DOUBLE, `BoolProp` BOOLEAN, `DateTimeProp` DATETIME, `GuidProp` CHAR(36), `DateOnlyProp` DATE, `TimeOnlyProp` TIME(6), `DateTimeOffsetProp` VARCHAR(100), `EnumProp` INT);",
            SqlDialect.PostgreSql => "CREATE TABLE \"supported_types\"(\"id\" INT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY, \"stringprop\" TEXT, \"intprop\" INT, \"longprop\" BIGINT, \"decimalprop\" DECIMAL(18,2), \"doubleprop\" DOUBLE PRECISION, \"boolprop\" BOOLEAN, \"datetimeprop\" TIMESTAMP, \"guidprop\" UUID, \"dateonlyprop\" DATE, \"timeonlyprop\" TIME WITHOUT TIME ZONE, \"datetimeoffsetprop\" TIMESTAMPTZ, \"enumprop\" INT);",
            SqlDialect.Sqlite => "CREATE TABLE \"supported_types\"(\"Id\" INTEGER PRIMARY KEY, \"StringProp\" TEXT, \"IntProp\" INTEGER, \"LongProp\" INTEGER, \"DecimalProp\" REAL, \"DoubleProp\" REAL, \"BoolProp\" INTEGER, \"DateTimeProp\" TEXT, \"GuidProp\" TEXT, \"DateOnlyProp\" TEXT, \"TimeOnlyProp\" TEXT, \"DateTimeOffsetProp\" TEXT, \"EnumProp\" INTEGER);",
        _ => throw new NotImplementedException()
    };
}

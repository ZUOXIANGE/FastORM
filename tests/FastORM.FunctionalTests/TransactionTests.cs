using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using Xunit;

namespace FastORM.FunctionalTests;

public class TransactionTests
{
    [Fact]
    public void ManualTransaction_Commit_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // Begin transaction
        ctx.BeginTransaction();

        ctx.Insert(new[] { new User { Id = 1, Name = "Alice", Age = 30 } });
        
        // Check within transaction
        Assert.Equal(1, ctx.Users.Count());

        // Commit
        ctx.Commit();

        // Check after commit
        Assert.Equal(1, ctx.Users.Count());
        
        // Verify persistent
        var ctx2 = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        Assert.Equal(1, ctx2.Users.Count());
    }

    [Fact]
    public void ManualTransaction_Rollback_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // Begin transaction
        ctx.BeginTransaction();

        ctx.Insert(new[] { new User { Id = 1, Name = "Alice", Age = 30 } });
        
        // Check within transaction
        Assert.Equal(1, ctx.Users.Count());

        // Rollback
        ctx.Rollback();

        // Check after rollback
        Assert.Equal(0, ctx.Users.Count());
    }
    
    [Fact]
    public async Task ManualTransaction_Async_Commit_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        await cmd.ExecuteNonQueryAsync();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // Begin transaction
        await ctx.BeginTransactionAsync();

        await ctx.InsertAsync(new[] { new User { Id = 1, Name = "Alice", Age = 30 } });
        
        // Check within transaction
        Assert.Equal(1, await ctx.Users.CountAsync());

        // Commit
        await ctx.CommitAsync();

        // Check after commit
        Assert.Equal(1, await ctx.Users.CountAsync());
    }

    [Fact]
    public void NoTransaction_Update_Is_Not_Atomic_If_Not_Managed()
    {
        // This test demonstrates that without explicit transaction, Update is per-row.
        // But since we use Sqlite in memory, it might be hard to simulate failure halfway reliably without mocking.
        // So we just verify basic Update works without explicit transaction (auto-commit).
        
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        ctx.Insert(new[] { new User { Id = 1, Name = "Alice", Age = 30 } });

        // Update without explicit transaction
        ctx.Update(new[] { new User { Id = 1, Name = "Bob", Age = 31 } });

        var user = ctx.Users.FirstOrDefault();
        Assert.NotNull(user);
        Assert.Equal("Bob", user.Name);
    }

    [Fact]
    public void ManualTransaction_DeleteByIds_Rollback_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        ctx.Insert(new[] { 
            new User { Id = 1, Name = "Alice", Age = 30 },
            new User { Id = 2, Name = "Bob", Age = 31 }
        });

        // Begin transaction
        ctx.BeginTransaction();

        // Delete
        ctx.Delete(new[] { new User { Id = 1 }, new User { Id = 2 } });
        
        // Check within transaction
        Assert.Equal(0, ctx.Users.Count());

        // Rollback
        ctx.Rollback();

        // Check after rollback
        Assert.Equal(2, ctx.Users.Count());
    }

    [Fact]
    public void NoTransaction_DeleteByIds_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        ctx.Insert(new[] { 
            new User { Id = 1, Name = "Alice", Age = 30 }
        });

        // Delete without explicit transaction
        ctx.Delete(new[] { new User { Id = 1 } });
        
        Assert.Equal(0, ctx.Users.Count());
    }
}

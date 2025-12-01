using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncOrderSkipTests
{
    [Fact]
    public async Task OrderSkipTakeAsync_ProducesExpectedResults()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users
            .OrderBy(static p => p.Name)
            .Skip(1)
            .Take(2)
            .ToListAsync();
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "Bob", "Carol" }, list.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task OrderSkipOnlyAsync_ProducesExpectedResults()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users
            .OrderBy(static p => p.Name)
            .Skip(2)
            .ToListAsync();
        Assert.Single(list);
        Assert.Equal("Carol", list[0].Name);
    }
}




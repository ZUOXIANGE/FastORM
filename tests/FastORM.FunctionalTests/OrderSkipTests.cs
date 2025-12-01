using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class OrderSkipTests
{
    [Fact]
    public void OrderSkipTake_ProducesExpectedResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .OrderBy(static p => p.Name)
            .Skip(1)
            .Take(2)
            .ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "Bob", "Carol" }, list.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void OrderSkipOnly_ProducesExpectedResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .OrderBy(static p => p.Name)
            .Skip(2)
            .ToList();
        Assert.Single(list);
        Assert.Equal("Carol", list[0].Name);
    }

    [Fact]
    public void SkipGreaterThanCount_ReturnsEmpty()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .OrderBy(static p => p.Name)
            .Skip(5)
            .ToList();
        Assert.Empty(list);
    }

    [Fact]
    public void TakeZero_ReturnsEmpty()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .OrderBy(static p => p.Name)
            .Take(0)
            .ToList();
        Assert.Empty(list);
    }
}



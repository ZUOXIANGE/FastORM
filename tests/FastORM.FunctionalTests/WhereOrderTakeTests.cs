using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class WhereOrderTakeTests
{
    [Fact]
    public void WhereOrderTake_ProducesExpectedResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.PostgreSql);
        var list = ctx.Users
            .Where(static p => p.Age > 18)
            .OrderBy(static p => p.Name)
            .Take(10)
            .ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "Alice", "Carol" }, list.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void Complex_WhereInNotInLike_OrderSkipTake()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES"+
            "(1,'Alice',30),(2,'Bob',17),(3,'Carol',22),(4,'David',40),(5,'Eve',19),(6,'Frank',18);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .Where(static p => p.Name.Contains("a") || p.Name.StartsWith("C") || p.Name.EndsWith("e"))
            .OrderByDescending(static p => p.Name)
            .Take(3)
            .ToList();
        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { "Frank", "Eve", "David" }, list.Select(x => x.Name).ToArray());
    }
}



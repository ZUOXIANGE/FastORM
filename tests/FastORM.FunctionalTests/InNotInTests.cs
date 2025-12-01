using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class InNotInTests
{
    [Fact]
    public void InQuery_ProducesExpectedResults()
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
            .Where(static p => new[] { 1, 3 }.Contains(p.Id))
            .OrderBy(static p => p.Name)
            .ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { "Alice", "Carol" }, list.Select(x => x.Name).ToArray());
    }

    [Fact]
    public void NotInQuery_ProducesExpectedResults()
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
            .Where(static p => !new[] { 1, 3 }.Contains(p.Id))
            .OrderBy(static p => p.Name)
            .ToList();
        Assert.Single(list);
        Assert.Equal("Bob", list[0].Name);
    }

    [Fact]
    public void InQuery_LargeList_ProducesExpectedCount()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = string.Join(";",
            Enumerable.Range(1, 50).Select(i => $"INSERT INTO Users(Id,Name,Age) VALUES({i},'N{i}',{18 + (i % 5)})"));
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .Where(static p => new[] { 2,4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,34,36,38,40,42,44,46,48,50 }.Contains(p.Id))
            .OrderBy(static p => p.Id)
            .ToList();
        Assert.Equal(25, list.Count);
        Assert.Equal(2, list.First().Id);
        Assert.Equal(50, list.Last().Id);
    }
}



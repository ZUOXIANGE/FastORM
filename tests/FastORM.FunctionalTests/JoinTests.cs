using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class JoinTests
{
    [Fact]
    public void SimpleJoin_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
        cmd.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice'),(2,'Bob');";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.PostgreSql);
        var results = ctx.Users
            .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .ToList();

        Assert.Equal(3, results.Count);
        Assert.Contains(results, r => r.Name == "Alice" && r.Amount == 12.5m);
        Assert.Contains(results, r => r.Name == "Alice" && r.Amount == 20.0m);
        Assert.Contains(results, r => r.Name == "Bob" && r.Amount == 5.0m);
    }

    [Fact]
    public void Join_WithWhereLike_OrderByAmountDesc_Take()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmdU = conn.CreateCommand();
        cmdU.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
        cmdU.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice'),(2,'Bob'),(3,'Carol');";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0),(13,3,7.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var results = ctx.Users
            .Where(static u => u.Name.StartsWith("A") || u.Name.StartsWith("C"))
            .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .OrderByDescending(static r => r.Amount)
            .Take(2)
            .ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal(20.0m, results[0].Amount);
        Assert.Equal(12.5m, results[1].Amount);
        Assert.All(results, r => Assert.Equal("Alice", r.Name));
    }

    [Fact]
    public void JoinProjection_GroupBy_SumOnProjectionProperty_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmdU = conn.CreateCommand();
        cmdU.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
        cmdU.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice'),(2,'Bob');";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var totals = ctx.Users
            .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .GroupBy(static r => r.Name)
            .Select(static g => new UserTotals { Name = g.Key ?? "", Total = g.Sum(static r => r.Amount) })
            .OrderByDescending(static x => x.Total)
            .ToList();

        Assert.Equal(2, totals.Count);
        Assert.Equal("Alice", totals[0].Name);
        Assert.Equal(32.5m, totals[0].Total);
        Assert.Equal("Bob", totals[1].Name);
        Assert.Equal(5.0m, totals[1].Total);
    }
}



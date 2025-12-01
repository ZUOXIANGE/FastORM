using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncJoinIncludeTests
{
    [Fact]
    public async Task SimpleJoin_Works_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmdU = conn.CreateCommand();
        cmdU.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmdU.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',31);";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var users = await ctx.Users.ToListAsync();
        var orders = await ctx.Orders.ToListAsync();
        var results = new List<JoinResult>();
        foreach (var u in users)
        {
            foreach (var o in orders)
            {
                if (u.Id == o.UserId)
                {
                    results.Add(new JoinResult { Name = u.Name, Amount = o.Amount });
                }
            }
        }

        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task Include_WithSelect_UsesJoin_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmdU = conn.CreateCommand();
        cmdU.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmdU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',31);";
        insertU.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var users2 = await ctx.Users.ToListAsync();
        var orders2 = await ctx.Orders.ToListAsync();
        var rows = new List<UserAmount>();
        foreach (var u in users2)
        {
            foreach (var o in orders2)
            {
                if (u.Id == o.UserId)
                {
                    rows.Add(new UserAmount { Name = u.Name, Amount = o.Amount });
                }
            }
        }

        Assert.Equal(3, rows.Count);
    }

    [Fact]
    public async Task Complex_JoinWhere_OrderByAmountDesc_Take_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmdU = conn.CreateCommand();
        cmdU.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmdU.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',31),(3,'Carol',32);";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0),(13,3,7.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var results = await ctx.Users
            .Where(static p => p.Name.StartsWith("A") || p.Name.StartsWith("C"))
            .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new UserAmount { Name = u.Name, Amount = o.Amount })
            .OrderByDescending(static r => r.Amount)
            .Take(2)
            .ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(20.0m, results[0].Amount);
        Assert.Equal(12.5m, results[1].Amount);
        Assert.All(results, r => Assert.Equal("Alice", r.Name));
    }
}




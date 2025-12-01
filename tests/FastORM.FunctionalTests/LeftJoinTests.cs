using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.Linq;

namespace FastORM.FunctionalTests;

public class LeftJoinTests
{
    [Fact]
    public void LeftJoin_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
        cmd.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice'),(2,'Bob'),(3,'Charlie');";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0);";
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        // Alice has orders, Bob has no orders, Charlie has no orders.
        // Left Join Users -> Orders should give Alice (matches) and Bob/Charlie (null matches)
        // Note: FastORM projection logic for joins might need adjustment to handle nulls on the right side.
        
        var results = ctx.Users
            .LeftJoin(ctx.Orders, 
                static u => u.Id, 
                static o => o.UserId, 
                static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .OrderBy(static r => r.Name)
            .ToList();

        // Alice: 2 orders
        // Bob: 1 row (null order -> Amount 0 or default?)
        // Charlie: 1 row
        
        Assert.Equal(4, results.Count); // Alice*2 + Bob + Charlie
        
        var alice = results.Where(static r => r.Name == "Alice").ToArray();
        Assert.Equal(2, alice.Length);
        Assert.Contains(alice, static r => r.Amount == 12.5m);
        Assert.Contains(alice, static r => r.Amount == 20.0m);
        
        var bob = results.Single(static r => r.Name == "Bob");
        Assert.Equal(0m, bob.Amount); // Default for decimal
        
        var charlie = results.Single(static r => r.Name == "Charlie");
        Assert.Equal(0m, charlie.Amount);
    }

    [Fact]
    public void RightJoin_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
        cmd.ExecuteNonQuery();
        using var insertU = conn.CreateCommand();
        insertU.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice');";
        insertU.ExecuteNonQuery();
        using var cmdO = conn.CreateCommand();
        cmdO.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmdO.ExecuteNonQuery();
        using var insertO = conn.CreateCommand();
        insertO.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,10.0),(11,99,50.0);"; // 99 is unknown user
        insertO.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        
        // Right Join Users -> Orders. 
        // Users (Left), Orders (Right).
        // Should return all Orders.
        // Order 10 matches Alice.
        // Order 11 (UserId 99) has no User -> User columns are NULL.
        
        var results = ctx.Users
            .RightJoin(ctx.Orders,
                static u => u.Id,
                static o => o.UserId,
                static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .OrderBy(static r => r.Amount)
            .ToList();

        Assert.Equal(2, results.Count);
        
        var match = results.Single(static r => r.Amount == 10.0m);
        Assert.Equal("Alice", match.Name);
        
        var noMatch = results.Single(static r => r.Amount == 50.0m);
        Assert.Null(noMatch.Name); // Name is string, so null
    }
}

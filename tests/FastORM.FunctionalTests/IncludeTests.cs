using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class IncludeTests
{
    [Fact]
    public void Include_WithSelect_UsesJoin()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);";
            cmd.ExecuteNonQuery();
        }
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
            cmd.ExecuteNonQuery();
        }
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Users(Id,Name) VALUES(1,'Alice'),(2,'Bob');";
            cmd.ExecuteNonQuery();
        }
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var rows = ctx.Users
            .LeftJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new UserAmount { Name = u.Name, Amount = o.Amount })
            .ToList();

        Assert.Equal(3, rows.Count);
        Assert.Contains(rows, r => r.Name == "Alice" && r.Amount == 12.5m);
        Assert.Contains(rows, r => r.Name == "Alice" && r.Amount == 20.0m);
        Assert.Contains(rows, r => r.Name == "Bob" && r.Amount == 5.0m);
    }
}



using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class OrderingThenByTests
{
    [Fact]
    public void OrderBy_ThenBy_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }
        using (var insert = conn.CreateCommand())
        {
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Bob',20),(2,'Alice',20),(3,'Carol',22),(4,'Bob',19);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .OrderBy(static p => p.Age)
            .OrderBy(static p => p.Name)
            .ToList();
        Assert.Equal(4, list.Count);
        Assert.Equal("Bob", list[0].Name);
        Assert.Equal(19, list[0].Age);
        Assert.Equal("Alice", list[1].Name);
        Assert.Equal(20, list[1].Age);
        Assert.Equal("Bob", list[2].Name);
        Assert.Equal(20, list[2].Age);
        Assert.Equal("Carol", list[3].Name);
        Assert.Equal(22, list[3].Age);
    }
}



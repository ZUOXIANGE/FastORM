using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class DistinctTests
{
    [Fact]
    public void Select_Distinct_Works()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Alice',22),(4,'Carol',22);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var people = ctx.Users
            .Distinct()
            .OrderBy(static p => p.Name)
            .ToList();
        Assert.Equal(4, people.Count);
        Assert.Equal("Alice", people[0].Name);
        Assert.Equal("Alice", people[1].Name);
        Assert.Equal("Bob", people[2].Name);
        Assert.Equal("Carol", people[3].Name);
    }
}



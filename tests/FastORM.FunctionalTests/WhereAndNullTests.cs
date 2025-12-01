using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class WhereAndNullTests
{
    [Fact]
    public void Where_With_And_And_NullCompare_Works()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,NULL,17),(3,'Carol',22);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Users
            .Where(static p => p.Name != null && p.Age > 18)
            .OrderBy(static p => p.Name)
            .ToList();
        Assert.Equal(2, list.Count);
        Assert.Equal("Alice", list[0].Name);
        Assert.Equal("Carol", list[1].Name);
    }
}



using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class FirstOrDefaultTests
{
    [Fact]
    public void FirstOrDefault_ProducesExpectedResult()
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
        var User = ctx.Users
            .Where(static p => p.Age > 18)
            .OrderBy(static p => p.Name)
            .FirstOrDefault();
        Assert.NotNull(User);
        Assert.Equal("Alice", User!.Name);
    }

    [Fact]
    public void FirstOrDefault_OnEmpty_ReturnsNull()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.PostgreSql);
        var User = ctx.Users
            .Where(static p => p.Age > 100)
            .OrderBy(static p => p.Name)
            .FirstOrDefault();
        Assert.Null(User);
    }
}



using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AnyAllExistsTests
{
    [Fact]
    public void Any_NoPredicate_UsesExists_ReturnsTrue()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'A',10), (2,'B',20);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var exists = ctx.Users.Where(static p => p.Age > 15).Any();
        Assert.True(exists);
    }

    [Fact]
    public void Any_WithPredicate_UsesExists_ReturnsFalse()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'A',10), (2,'B',20);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var exists = ctx.Users.Any(static p => p.Age > 30);
        Assert.False(exists);
    }

    [Fact]
    public void All_WithPredicate_UsesNotExists_ReturnsTrue()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'A',10), (2,'B',20);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var allAdults = ctx.Users.All(static p => p.Age >= 10);
        Assert.True(allAdults);
    }

    [Fact]
    public void All_WithPredicate_UsesNotExists_ReturnsFalse()
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
            insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'A',10), (2,'B',20);";
            insert.ExecuteNonQuery();
        }
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var allSeniors = ctx.Users.All(static p => p.Age >= 21);
        Assert.False(allSeniors);
    }
}



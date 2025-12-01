using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncFirstOrDefaultTests
{
    [Fact]
    public async Task FirstOrDefaultAsync_ProducesExpectedResult()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var User = await ctx.Users
            .Where(static p => p.Age > 18)
            .OrderBy(static p => p.Name)
            .FirstOrDefaultAsync();
        Assert.NotNull(User);
        Assert.Equal("Alice", User!.Name);
    }
}




using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncLikeTests
{
    [Fact]
    public async Task LikeQueryAsync_ProducesExpectedResults()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Chloe',30),(2,'Chen',25),(3,'Alice',22),(4,'Bob',17);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users
            .Where(static p => p.Name.Contains("Chloe") || p.Name.StartsWith("C") || p.Name.EndsWith("e"))
            .OrderBy(static p => p.Name)
            .ToListAsync();
        Assert.Equal(3, list.Count);
        Assert.Equal(new[] { "Alice", "Chen", "Chloe" }, list.Select(x => x.Name).ToArray());
    }
}




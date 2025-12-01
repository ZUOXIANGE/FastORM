using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FastORM.FunctionalTests;

public class DeleteWhereTests
{
    [Fact]
    public void Delete_Where_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        ctx.Insert(new[] {
            new User { Id = 1, Name = "Alice", Age = 30 },
            new User { Id = 2, Name = "Bob", Age = 17 },
            new User { Id = 3, Name = "Carol", Age = 22 },
            new User { Id = 4, Name = "Dave", Age = 15 }
        });

        // Delete users younger than 18
        var deletedCount = ctx.Users.Where(static u => u.Age < 18).Delete();
        Assert.Equal(2, deletedCount);

        var remaining = ctx.Users.ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Contains(remaining, u => u.Name == "Alice");
        Assert.Contains(remaining, u => u.Name == "Carol");
    }

    [Fact]
    public async Task DeleteAsync_Where_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        ctx.Insert(new[] {
            new User { Id = 1, Name = "Alice", Age = 30 },
            new User { Id = 2, Name = "Bob", Age = 17 },
            new User { Id = 3, Name = "Carol", Age = 22 },
            new User { Id = 4, Name = "Dave", Age = 15 }
        });

        // Delete users younger than 18
        var deletedCount = await ctx.Users.Where(static u => u.Age < 18).DeleteAsync();
        Assert.Equal(2, deletedCount);

        var remaining = await ctx.Users.ToListAsync();
        Assert.Equal(2, remaining.Count);
    }
}

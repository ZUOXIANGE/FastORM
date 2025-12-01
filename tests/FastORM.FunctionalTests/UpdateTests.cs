using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.FunctionalTests;

public class UpdateTests
{
    [Fact]
    public void Update_SpecificColumns_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO Users(Id, Name, Age) VALUES (1, 'Alice', 30);";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO Users(Id, Name, Age) VALUES (2, 'Bob', 25);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // Update Alice's Name and Age
        int affected = ctx.Users.Where(static p => p.Id == 1).Update(static p => { p.Name = "AliceUpdated"; p.Age = 35; });
        Assert.Equal(1, affected);

        // Verify Alice
        var alice = ctx.Users.Where(static p => p.Id == 1).FirstOrDefault();
        Assert.NotNull(alice);
        Assert.Equal("AliceUpdated", alice.Name);
        Assert.Equal(35, alice.Age);

        // Verify Bob is untouched
        var bob = ctx.Users.Where(static p => p.Id == 2).FirstOrDefault();
        Assert.NotNull(bob);
        Assert.Equal("Bob", bob.Name);
        Assert.Equal(25, bob.Age);
    }
    
    [Fact]
    public async Task UpdateAsync_SpecificColumns_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO Users(Id, Name, Age) VALUES (1, 'Alice', 30);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // Update Alice's Age only
        int affected = await ctx.Users.Where(static p => p.Id == 1).UpdateAsync(static p => { p.Age = 40; });
        Assert.Equal(1, affected);

        var alice = await ctx.Users.Where(static p => p.Id == 1).FirstOrDefaultAsync();
        Assert.NotNull(alice);
        Assert.Equal("Alice", alice.Name); // Name unchanged
        Assert.Equal(40, alice.Age);
    }
}

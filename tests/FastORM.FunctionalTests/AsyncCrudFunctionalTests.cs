using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncCrudFunctionalTests
{
    [Fact]
    public async Task SingleRow_Insert_Update_Delete_Work_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        var ins1 = await ctx.InsertAsync(new User { Id = 1, Name = "Alice", Age = 30 });
        Assert.Equal(1, ins1);

        var insMany = await ctx.InsertAsync(new[] {
            new User { Id = 2, Name = "Bob", Age = 17 },
            new User { Id = 3, Name = "Carol", Age = 22 },
        });
        Assert.Equal(2, insMany);

        var upd1 = await ctx.UpdateAsync(new User { Id = 1, Name = "Alice", Age = 31 });
        Assert.Equal(1, upd1);

        var del1 = await ctx.DeleteAsync(new User { Id = 2, Name = "Bob", Age = 17 });
        Assert.Equal(1, del1);

        var delById = await ctx.DeleteAsync(new User { Id = 3 });
        Assert.Equal(1, delById);

        using var countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Users";
        var left = (long)countCmd.ExecuteScalar()!;
        Assert.Equal(1, left);

        var one = await ctx.Users.Where(static u => u.Id == 1).FirstOrDefaultAsync();
        Assert.NotNull(one);
        Assert.Equal(31, one!.Age);

        var all = await ctx.Users.ToListAsync();
        Assert.Single(all);

        var ids = await ctx.Users.Where(static u => new[] { 1 }.Contains(u.Id)).ToListAsync();
        Assert.Single(ids);
    }
}




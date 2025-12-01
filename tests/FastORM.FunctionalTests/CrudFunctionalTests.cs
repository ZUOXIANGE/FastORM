using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class CrudFunctionalTests
{
    [Fact]
    public void SingleRow_Insert_Update_Delete_Work()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        var ins1 = ctx.Insert(new User { Id = 1, Name = "Alice", Age = 30 });
        Assert.Equal(1, ins1);

        var insMany = ctx.Insert(new[] {
            new User { Id = 2, Name = "Bob", Age = 17 },
            new User { Id = 3, Name = "Carol", Age = 22 },
        });
        Assert.Equal(2, insMany);

        var upd1 = ctx.Update(new User { Id = 1, Name = "Alice", Age = 31 });
        Assert.Equal(1, upd1);

        var del1 = ctx.Delete(new User { Id = 2, Name = "Bob", Age = 17 });
        Assert.Equal(1, del1);

        var delById = ctx.Delete(new User { Id = 3 });
        Assert.Equal(1, delById);

        using var countCmd = conn.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM Users";
        var left = (long)countCmd.ExecuteScalar()!;
        Assert.Equal(1, left);

        var one = ctx.Users.Where(static u => u.Id == 1).FirstOrDefault();
        Assert.NotNull(one);
        Assert.Equal(31, one!.Age);

        var all = ctx.Users.ToList();
        Assert.Single(all);

        var ids = ctx.Users.Where(static u => new[] { 1 }.Contains(u.Id)).ToList();
        Assert.Single(ids);
    }
}



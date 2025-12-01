using Xunit;
using Microsoft.Data.Sqlite;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System.Linq;

namespace FastORM.IntegrationTests;

public partial class CrudIntegrationTests
{
    [Fact]
    public void Crud_Sqlite()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }
        var ctx = new IntegrationTestDbContext(conn, SqlDialect.Sqlite);
        var inserted = ctx.Insert(new[] {
            new User{ Id=1, Name="Alice", Age=30 },
            new User{ Id=2, Name="Bob", Age=17 },
            new User{ Id=3, Name="Carol", Age=22 },
        });
        Assert.Equal(3, inserted);
        var list = ctx.Users.Where(static p=>p.Age>18).OrderBy(static p=>p.Name).ToList();
        Assert.Equal(2, list.Count);
        var updated = ctx.Update(new[] { new User{ Id=1, Name="Alice", Age=31 } });
        Assert.Equal(1, updated);
        var del = ctx.Delete(new[] { new User{ Id=2, Name="Bob", Age=17 } });
        Assert.Equal(1, del);
        var final = ctx.Users.OrderBy(static p=>p.Id).ToList();
        Assert.Equal(2, final.Count);

        var one = ctx.Users.Where(static u => u.Id == 1).FirstOrDefault();
        Assert.NotNull(one);
        Assert.Equal(31, one!.Age);
        var allUsers = ctx.Users.ToList();
        Assert.Equal(2, allUsers.Count);
    }
}

using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FastORM.FunctionalTests;

public class GeneratorCrudTests
{
    [Fact]
    public async Task Generated_Insert_Update_Delete_Work()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
            cmd.ExecuteNonQuery();
        }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        // 1. Insert Single (Should be intercepted)
        var u1 = new User { Id = 1, Name = "GenAlice", Age = 30 };
        int ins1 = ctx.Insert(u1);
        Assert.Equal(1, ins1);
        
        // 2. Insert Batch (Should be intercepted)
        var uList = new List<User> {
            new User { Id = 2, Name = "GenBob", Age = 20 },
            new User { Id = 3, Name = "GenCarol", Age = 25 }
        };
        int insMany = ctx.Insert(uList);
        Assert.Equal(2, insMany);

        var users = ctx.Users.ToList();
        Assert.Equal(3, users.Count);
        Assert.Contains(users, u => u.Name == "GenAlice");

        // 3. Update with Where (Generated)
        // Use static lambda for value to ensure generator compatibility
        int upd1 = ctx.Users.Where(static u => u.Id == 1).Update(static u => u.Name = "UpdatedAlice");
        Assert.Equal(1, upd1);

        var updatedUser = ctx.Users.Where(static u => u.Id == 1).FirstOrDefault();
        Assert.Equal("UpdatedAlice", updatedUser.Name);

        // 4. Delete with Where (Generated)
        int del1 = ctx.Users.Where(static u => u.Id == 2).Delete();
        Assert.Equal(1, del1);

        var remaining = ctx.Users.ToList();
        Assert.Equal(2, remaining.Count);
        Assert.DoesNotContain(remaining, u => u.Id == 2);

        // 5. Async versions
        await ctx.InsertAsync(new User { Id = 4, Name = "AsyncDave", Age = 40 });
        
        int updAsync = await ctx.Users.Where(static u => u.Id == 4).UpdateAsync(static u => u.Age = 41);
        Assert.Equal(1, updAsync);
        
        int delAsync = await ctx.Users.Where(static u => u.Id == 4).DeleteAsync();
        Assert.Equal(1, delAsync);

        // 6. Context-based Update(entity) and Delete(entity)
        var u5 = new User { Id = 5, Name = "Charlie", Age = 35 };
        await ctx.InsertAsync(u5);
        
        u5.Age = 36;
        int updEnt = ctx.Update(u5); // Should be intercepted
        Assert.Equal(1, updEnt);
        
        var u5Check = ctx.Users.Where(static u => u.Id == 5).FirstOrDefault();
        Assert.Equal(36, u5Check.Age);
        
        int delEnt = ctx.Delete(u5); // Should be intercepted
        Assert.Equal(1, delEnt);
        
        var u5Gone = ctx.Users.Where(static u => u.Id == 5).FirstOrDefault();
        Assert.Null(u5Gone);
    }
}

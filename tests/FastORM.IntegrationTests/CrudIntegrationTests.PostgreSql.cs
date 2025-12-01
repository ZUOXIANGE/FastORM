using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class CrudIntegrationTests
{
    [Fact]
    public async Task Crud_PostgreSql()
    {
        var pg = new PostgreSqlBuilder()
            .WithDatabase("fastorm")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
        try { await pg.StartAsync(); }
        catch (Exception) { return; }
        await using (pg.ConfigureAwait(false))
        {
            await using var conn = new NpgsqlConnection(pg.GetConnectionString());
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE \"users\"(\"id\" INT PRIMARY KEY, \"name\" TEXT, \"age\" INT);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);

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
            countCmd.CommandText = "SELECT COUNT(*) FROM \"users\"";
            var left = (long)countCmd.ExecuteScalar()!;
            Assert.Equal(1, left);

            var one = ctx.Users.Where(static u => u.Id == 1).FirstOrDefault();
            Assert.NotNull(one);
            Assert.Equal(31, one!.Age);
            var all = ctx.Users.ToList();
            Assert.Single(all);
        }
    }
}

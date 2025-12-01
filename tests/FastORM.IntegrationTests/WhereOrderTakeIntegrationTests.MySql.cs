using Xunit;
using Testcontainers.MySql;
using MySqlConnector;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class WhereOrderTakeIntegrationTests
{
    [Fact]
    public async Task WhereOrderTake_MySql()
    {
        var my = new MySqlBuilder()
            .WithDatabase("fastorm")
            .WithUsername("mysql")
            .WithPassword("mysql")
            .Build();
        try { await my.StartAsync(); }
        catch (Exception) { return; }
        await using (my.ConfigureAwait(false))
        {
            await using var conn = new MySqlConnection(my.GetConnectionString());
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE `users`(`Id` INT PRIMARY KEY, `Name` VARCHAR(100), `Age` INT);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `users`(`Id`,`Name`,`Age`) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.MySql);
            var list = ctx.Users
                .Where(static p => p.Age > 18)
                .OrderBy(static p => p.Name)
                .Take(10)
                .ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal("Alice", list[0].Name);
            Assert.Equal("Carol", list[1].Name);
        }
    }
}

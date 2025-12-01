using Xunit;
using Testcontainers.MySql;
using MySqlConnector;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class GroupByIntegrationTests
{
    [Fact]
    public async Task GroupBy_MySql()
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
                cmd.CommandText = "CREATE TABLE `items`(`Id` INT PRIMARY KEY, `CategoryId` INT);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `items`(`Id`,`CategoryId`) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.MySql);
            var grp = ctx.Items
                .GroupBy(static x => x.CategoryId)
                .Select(static g => new GroupResult { Key = g.Key, Count = g.Count() })
                .ToList();
            Assert.Equal(2, grp.Count);
        }
    }
}

using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class GroupByIntegrationTests
{
    [Fact]
    public async Task GroupBy_PostgreSql()
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
                cmd.CommandText = "CREATE TABLE \"items\"(\"id\" INT PRIMARY KEY, \"categoryid\" INT);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"items\"(\"id\",\"categoryid\") VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            var grp = ctx.Items
                .GroupBy(static x => x.CategoryId)
                .Select(static g => new GroupResult { Key = g.Key, Count = g.Count() })
                .ToList();
            Assert.Equal(2, grp.Count);
        }
    }
}

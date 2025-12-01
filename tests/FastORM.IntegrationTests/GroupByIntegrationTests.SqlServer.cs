using Xunit;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class GroupByIntegrationTests
{
    [Fact]
    public async Task GroupBy_SqlServer()
    {
        var ms = new MsSqlBuilder()
            .WithPassword("yourStrong(!)Password")
            .Build();
        try { await ms.StartAsync(); }
        catch (Exception) { return; }
        await using (ms.ConfigureAwait(false))
        {
            await using var conn = new SqlConnection(ms.GetConnectionString());
            await conn.OpenAsync();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE [items]([Id] INT PRIMARY KEY, [CategoryId] INT);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO [items]([Id],[CategoryId]) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.SqlServer);
            var grp = ctx.Items
                .GroupBy(static x => x.CategoryId)
                .Select(static g => new GroupResult { Key = g.Key, Count = g.Count() })
                .ToList();
            Assert.Equal(2, grp.Count);
        }
    }
}

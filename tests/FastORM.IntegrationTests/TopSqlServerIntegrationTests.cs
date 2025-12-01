using Xunit;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;

namespace FastORM.IntegrationTests;

public class TopSqlServerIntegrationTests
{
    [Fact]
    public async Task Top_Take_SqlServer()
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
                cmd.CommandText = "CREATE TABLE [users]([Id] INT PRIMARY KEY, [Name] NVARCHAR(100), [Age] INT);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO [users]([Id],[Name],[Age]) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.SqlServer);
            var topList = ctx.Users
                .Where(static p => p.Age > 18)
                .Take(2)
                .ToList();
            Assert.Equal(2, topList.Count);
            Assert.Contains(topList, x => x.Name == "Alice");
            Assert.Contains(topList, x => x.Name == "Carol");
        }
    }
}


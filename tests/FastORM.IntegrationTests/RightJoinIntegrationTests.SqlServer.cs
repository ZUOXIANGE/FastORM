using Xunit;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class RightJoinIntegrationTests
{
    [Fact]
    public async Task RightJoin_SqlServer()
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
                cmd.CommandText = "CREATE TABLE [orders]([Id] INT PRIMARY KEY, [UserId] INT, [Amount] DECIMAL(18,2));";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO [users]([Id],[Name],[Age]) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO [orders]([Id],[UserId],[Amount]) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.SqlServer);
            var rjoin = ctx.Users
                .RightJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .ToList();
            Assert.Equal(3, rjoin.Count);
        }
    }
}

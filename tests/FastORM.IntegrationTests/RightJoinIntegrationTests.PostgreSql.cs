using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class RightJoinIntegrationTests
{
    [Fact]
    public async Task RightJoin_PostgreSql()
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
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE \"orders\"(\"id\" INT PRIMARY KEY, \"userid\" INT, \"amount\" NUMERIC);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"users\"(\"id\",\"name\",\"age\") VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"orders\"(\"id\",\"userid\",\"amount\") VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            var rjoin = ctx.Users
                .RightJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .ToList();
            Assert.Equal(3, rjoin.Count);
        }
    }

    [Fact]
    public async Task RightJoin_OrderByAmountDesc_Take1_PostgreSql()
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
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE \"orders\"(\"id\" INT PRIMARY KEY, \"userid\" INT, \"amount\" NUMERIC);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"users\"(\"id\",\"name\",\"age\") VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"orders\"(\"id\",\"userid\",\"amount\") VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0),(13,3,7.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            var rows = ctx.Users
                .RightJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .OrderByDescending(static r => r.Amount)
                .Take(1)
                .ToList();
            Assert.Single(rows);
            Assert.Equal(20.0m, rows[0].Amount);
        }
    }
}

using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class JoinIntegrationTests
{
    [Fact]
    public async Task Join_PostgreSql()
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
            var join = ctx.Users
                .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .ToList();
            Assert.Equal(3, join.Count);

            var inc = ctx.Users
                .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new UserAmount { Name = u.Name, Amount = o.Amount })
                .ToList();
            Assert.Equal(3, inc.Count);
        }
    }

    [Fact]
    public async Task JoinGroupBySum_PostgreSql()
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
                cmd.CommandText = "INSERT INTO \"users\"(\"id\",\"name\",\"age\") VALUES(1,'Alice',30),(2,'Bob',17);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO \"orders\"(\"id\",\"userid\",\"amount\") VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            var totals = ctx.Users
                .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .GroupBy(static r => r.Name)
                .Select(static g => new UserAmount { Name = g.Key, Amount = g.Sum(static r => r.Amount) })
                .OrderByDescending(static x => x.Amount)
                .ToList();
            Assert.Equal(2, totals.Count);
            Assert.Equal("Alice", totals[0].Name);
            Assert.Equal(32.5m, totals[0].Amount);
            Assert.Equal("Bob", totals[1].Name);
            Assert.Equal(5.0m, totals[1].Amount);
        }
    }
}

using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class WhereOrderTakeIntegrationTests
{
    [Fact]
    public async Task WhereOrderTake_PostgreSql()
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
                cmd.CommandText = "INSERT INTO \"users\"(\"id\",\"name\",\"age\") VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
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

    [Fact]
    public async Task WhereInNotInLike_OrderSkipTake_PostgreSql()
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
            using (var insert = conn.CreateCommand())
            {
                insert.CommandText = "INSERT INTO \"users\"(\"id\",\"name\",\"age\") VALUES"+
                    "(1,'Alice',30),(2,'Bob',17),(3,'Carol',22),(4,'David',40),(5,'Eve',19),(6,'Frank',18);";
                insert.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.PostgreSql);
            var list = ctx.Users
                .Where(static p => p.Name.Contains("a") || p.Name.StartsWith("C") || p.Name.EndsWith("e"))
                .OrderByDescending(static p => p.Name)
                .Take(3)
                .ToList();
            Assert.Equal(3, list.Count);
            Assert.Equal("Frank", list[0].Name);
            Assert.Equal("Eve", list[1].Name);
            Assert.Equal("David", list[2].Name);
        }
    }
}

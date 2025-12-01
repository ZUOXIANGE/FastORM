using Xunit;
using Testcontainers.MySql;
using MySqlConnector;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FastORM.IntegrationTests;

public partial class RightJoinIntegrationTests
{
    [Fact]
    public async Task RightJoin_MySql()
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
                cmd.CommandText = "CREATE TABLE `orders`(`Id` INT PRIMARY KEY, `UserId` INT, `Amount` DECIMAL(10,2));";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `users`(`Id`,`Name`,`Age`) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `orders`(`Id`,`UserId`,`Amount`) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.MySql);
            var rjoin = ctx.Users
                .RightJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
                .ToList();
            Assert.Equal(3, rjoin.Count);
        }
    }

    [Fact]
    public async Task RightJoin_OrderByAmountDesc_Take1_MySql()
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
                cmd.CommandText = "CREATE TABLE `orders`(`Id` INT PRIMARY KEY, `UserId` INT, `Amount` DECIMAL(10,2));";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `users`(`Id`,`Name`,`Age`) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO `orders`(`Id`,`UserId`,`Amount`) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0),(13,3,7.0);";
                cmd.ExecuteNonQuery();
            }
            var ctx = new IntegrationTestDbContext(conn, SqlDialect.MySql);
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

using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncAggregationTests
{
    [Fact]
    public async Task CountAsync_ProducesExpectedResult()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var cnt = await ctx.Users
            .Where(static p => p.Age >= 18)
            .CountAsync();
        Assert.Equal(2, cnt);
    }

    [Fact]
    public async Task MaxMinAverageAsync_ProduceExpectedResults()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',23);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var max = await ctx.Users.MaxAsync(static p => p.Age);
        var min = await ctx.Users.MinAsync(static p => p.Age);
        var avg = await ctx.Users.AverageAsync(static p => p.Age);
        Assert.Equal(30, max);
        Assert.Equal(17, min);
        Assert.True(avg > 23 && avg < 24);
    }

    [Fact]
    public async Task SumAsync_ProducesExpectedResult()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',23);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var sum = await ctx.Users.SumAsync(static p => p.Age);
        Assert.Equal(70, sum);
    }

    [Fact]
    public async Task Decimal_SumAsync_ProducesExpectedResult()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(10,1,12.5),(11,1,20.0),(12,2,5.0);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var sum = await ctx.Orders.SumAsync(static o => o.Amount);
        Assert.Equal(37.5m, sum);
    }
}




using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests;

public class AggregationTests
{
    [Fact]
    public void Count_ProducesExpectedResult()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var cnt = ctx.Users
            .Where(static p => p.Age >= 18)
            .Count();
        Assert.Equal(2, cnt);
    }

    [Fact]
    public void MaxMinAverage_ProduceExpectedResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',23);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var max = ctx.Users.Max(static p => p.Age);
        var min = ctx.Users.Min(static p => p.Age);
        var avg = ctx.Users.Average(static p => p.Age);
        Assert.Equal(30, max);
        Assert.Equal(17, min);
        Assert.True(avg > 23 && avg < 24);
    }

    [Fact]
    public void Sum_ProducesExpectedResult()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',23);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var sum = ctx.Users.Sum(static p => p.Age);
        Assert.Equal(70, sum);
    }

    [Fact]
    public void Sum_WithWhereFilter_ProducesExpectedResult()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',23);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var sum = ctx.Users.Where(static p => p.Age >= 20).Sum(static p => p.Age);
        Assert.Equal(53, sum);
    }

    [Fact]
    public void Decimal_SumAndAverage_ProduceExpectedResults()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, Amount REAL);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Orders(Id,Amount) VALUES(1,12.5),(2,20.0),(3,5.0);";
        insert.ExecuteNonQuery();
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var total = ctx.Orders.Sum(static a => a.Amount);
        var avg = ctx.Orders.Average(static a => a.Amount);
        Assert.Equal(37.5m, total);
        Assert.True((double)avg > 12.4 && (double)avg < 12.6);
    }
}



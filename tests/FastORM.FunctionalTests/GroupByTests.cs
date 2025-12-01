using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class GroupByTests
{
    [Fact]
    public void GroupBy_KeyAndCount_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Items(Id,CategoryId) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Items
            .GroupBy(static x => x.CategoryId)
            .Select(static g => new GroupResult { Key = g.Key, Count = g.Count() })
            .ToList();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Key == 1 && x.Count == 2);
        Assert.Contains(list, x => x.Key == 2 && x.Count == 3);
    }

    [Fact]
    public void GroupBy_SumMinMaxAverage_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Items(Id,CategoryId) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = ctx.Items
            .GroupBy(static x => x.CategoryId)
            .Select(static g => new AggResult
            {
                Key = g.Key,
                Sum = g.Sum(static x => x.Id),
                Min = g.Min(static x => x.Id),
                Max = g.Max(static x => x.Id),
                Avg = g.Average(static x => x.Id)
            })
            .OrderBy(static r => r.Key)
            .ToList();

        Assert.Equal(2, list.Count);
        Assert.Equal(1, list[0].Key);
        Assert.Equal(3, list[0].Sum);
        Assert.Equal(1, list[0].Min);
        Assert.Equal(2, list[0].Max);
        Assert.True(list[0].Avg > 1.4 && list[0].Avg < 1.6);

        Assert.Equal(2, list[1].Key);
        Assert.Equal(12, list[1].Sum);
        Assert.Equal(3, list[1].Min);
        Assert.Equal(5, list[1].Max);
        Assert.True(list[1].Avg > 3.9 && list[1].Avg < 4.1);
    }

    [Fact]
    public void GroupBy_Aggregators_OrderBySumDesc_Take1()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Items(Id,CategoryId) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var top = ctx.Items
            .GroupBy(static x => x.CategoryId)
            .Select(static g => new AggResult
            {
                Key = g.Key,
                Sum = g.Sum(static x => x.Id),
                Min = g.Min(static x => x.Id),
                Max = g.Max(static x => x.Id),
                Avg = g.Average(static x => x.Id)
            })
            .OrderByDescending(static r => r.Sum)
            .Take(1)
            .ToList();

        Assert.Single(top);
        Assert.Equal(2, top[0].Key);
        Assert.Equal(12, top[0].Sum);
    }
}



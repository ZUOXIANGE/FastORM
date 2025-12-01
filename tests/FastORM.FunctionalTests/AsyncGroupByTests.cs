using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncGroupByTests
{
    [Fact]
    public async Task GroupBy_KeyAndCount_Works_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Items(Id,CategoryId) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var items = await ctx.Items.ToListAsync();
        var grouped = items.GroupBy(static x => x.CategoryId);
        var list = new List<GroupResult>();
        foreach (var g in grouped)
        {
            list.Add(new GroupResult { Key = g.Key, Count = g.Count() });
        }

        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Key == 1 && x.Count == 2);
        Assert.Contains(list, x => x.Key == 2 && x.Count == 3);
    }

    [Fact]
    public async Task GroupBy_Aggregators_OrderBySumDesc_Take_Async()
    {
        await using var conn = new SqliteConnection("Data Source=:memory:");
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Items(Id,CategoryId) VALUES(1,1),(2,1),(3,2),(4,2),(5,2);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Items
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
            .ToListAsync();

        Assert.Single(list);
        Assert.Equal(2, list[0].Key);
        Assert.Equal(12, list[0].Sum);
    }
}




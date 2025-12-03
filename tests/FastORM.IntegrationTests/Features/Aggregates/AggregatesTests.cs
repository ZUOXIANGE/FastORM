using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Aggregates;

/// <summary>
/// 聚合查询 (Aggregates) 功能测试
/// 目标：验证 ORM 支持 Count 等聚合函数，以及 Group By 分组查询。
/// </summary>
public class AggregatesTests
{
    // --- Count ---

    private async Task Should_Count_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 统计总用户数 (Seed: 5 users)
        var count = await ctx.Users.CountAsync();

        // Assert
        await Assert.That(count).IsEqualTo(5);
    }

    [Test]
    public async Task Should_Count_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Count_Implementation);
    [Test]
    public async Task Should_Count_MySql() => await TestRunner.RunOnMySql(Should_Count_Implementation);
    [Test]
    public async Task Should_Count_SqlServer() => await TestRunner.RunOnSqlServer(Should_Count_Implementation);
    [Test]
    public async Task Should_Count_Sqlite() => await TestRunner.RunOnSqlite(Should_Count_Implementation);


    // --- Count with Filter ---

    private async Task Should_Count_With_Filter_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 统计年龄大于 25 的用户数 (Alice=30, Dave=40 -> 2 users)
        var count = await ctx.Users.Where(u => u.Age > 25).CountAsync();

        // Assert
        await Assert.That(count).IsEqualTo(2);
    }

    [Test]
    public async Task Should_Count_With_Filter_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Count_With_Filter_Implementation);
    [Test]
    public async Task Should_Count_With_Filter_MySql() => await TestRunner.RunOnMySql(Should_Count_With_Filter_Implementation);
    [Test]
    public async Task Should_Count_With_Filter_SqlServer() => await TestRunner.RunOnSqlServer(Should_Count_With_Filter_Implementation);
    [Test]
    public async Task Should_Count_With_Filter_Sqlite() => await TestRunner.RunOnSqlite(Should_Count_With_Filter_Implementation);


    // --- Group By ---

    private async Task Should_GroupBy_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 按 CategoryId 分组统计 Item 数量
        // Items Seed:
        // Cat 1: Id 1, 2 (Count 2)
        // Cat 2: Id 3, 4, 5 (Count 3)
        // Cat 3: Id 6 (Count 1)
        
        var groups = ctx.Items
            .GroupBy(i => i.CategoryId)
            .Select(g => new GroupResult { Key = g.Key, Count = g.Count() })
            .ToList();

        // Assert
        await Assert.That(groups).Count().IsEqualTo(3);
        
        var cat1 = groups.FirstOrDefault(g => g.Key == 1);
        await Assert.That(cat1).IsNotNull();
        await Assert.That(cat1!.Count).IsEqualTo(2);

        var cat2 = groups.FirstOrDefault(g => g.Key == 2);
        await Assert.That(cat2).IsNotNull();
        await Assert.That(cat2!.Count).IsEqualTo(3);

        var cat3 = groups.FirstOrDefault(g => g.Key == 3);
        await Assert.That(cat3).IsNotNull();
        await Assert.That(cat3!.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Should_GroupBy_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_GroupBy_Implementation);
    [Test]
    public async Task Should_GroupBy_MySql() => await TestRunner.RunOnMySql(Should_GroupBy_Implementation);
    [Test]
    public async Task Should_GroupBy_SqlServer() => await TestRunner.RunOnSqlServer(Should_GroupBy_Implementation);
    // Sqlite might have issues with GroupBy depending on provider/ORM implementation, but trying it.
    [Test]
    public async Task Should_GroupBy_Sqlite() => await TestRunner.RunOnSqlite(Should_GroupBy_Implementation);

    // --- Any (Exists) ---

    private async Task Should_Any_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: Check if any users exist
        var exists = await ctx.Users.AnyAsync();

        // Assert
        await Assert.That(exists).IsTrue();
    }

    [Test]
    public async Task Should_Any_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Any_Implementation);
    [Test]
    public async Task Should_Any_MySql() => await TestRunner.RunOnMySql(Should_Any_Implementation);
    [Test]
    public async Task Should_Any_SqlServer() => await TestRunner.RunOnSqlServer(Should_Any_Implementation);
    [Test]
    public async Task Should_Any_Sqlite() => await TestRunner.RunOnSqlite(Should_Any_Implementation);

    private async Task Should_Any_With_Filter_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: Check if any user exists with Age > 100 (Should be false)
        var exists = await ctx.Users.Where(u => u.Age > 100).AnyAsync();

        // Assert
        await Assert.That(exists).IsFalse();

        // Act: Check if any user exists with Age > 20 (Should be true)
        var exists2 = await ctx.Users.Where(u => u.Age > 20).AnyAsync();

        // Assert
        await Assert.That(exists2).IsTrue();
    }

    [Test]
    public async Task Should_Any_With_Filter_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Any_With_Filter_Implementation);
    [Test]
    public async Task Should_Any_With_Filter_MySql() => await TestRunner.RunOnMySql(Should_Any_With_Filter_Implementation);
    [Test]
    public async Task Should_Any_With_Filter_SqlServer() => await TestRunner.RunOnSqlServer(Should_Any_With_Filter_Implementation);
    [Test]
    public async Task Should_Any_With_Filter_Sqlite() => await TestRunner.RunOnSqlite(Should_Any_With_Filter_Implementation);

    // --- All (Not Exists + Negate) ---

    private async Task Should_All_Async_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: Check if all users are older than 10 (Seed: Alice=30, Bob=25, Charlie=35, Dave=40, Eve=28)
        // All > 10 -> True
        var allOlderThan10 = await ctx.Users.AllAsync(u => u.Age > 10);

        // Assert
        await Assert.That(allOlderThan10).IsTrue();

        // Act: Check if all users are older than 30 (False, Bob=25, etc.)
        var allOlderThan30 = await ctx.Users.AllAsync(u => u.Age > 30);

        // Assert
        await Assert.That(allOlderThan30).IsFalse();
    }

    [Test]
    public async Task Should_All_Async_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_All_Async_Implementation);
    [Test]
    public async Task Should_All_Async_MySql() => await TestRunner.RunOnMySql(Should_All_Async_Implementation);
    [Test]
    public async Task Should_All_Async_SqlServer() => await TestRunner.RunOnSqlServer(Should_All_Async_Implementation);
    [Test]
    public async Task Should_All_Async_Sqlite() => await TestRunner.RunOnSqlite(Should_All_Async_Implementation);
}

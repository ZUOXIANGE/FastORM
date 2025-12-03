using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Querying;

/// <summary>
/// 排序与分页 (Sorting and Paging) 功能测试
/// 目标：验证 ORM 能正确生成 Order By, Limit (Take) 和 Offset (Skip) 语句。
/// </summary>
public class SortingAndPagingTests
{
    // --- OrderBy ---

    private async Task Should_OrderBy_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 按年龄升序排序
        var users = await ctx.Users.OrderBy(u => u.Age).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(5);
        await Assert.That(users[0].Name).IsEqualTo("Bob");
        await Assert.That(users[1].Name).IsEqualTo("Carol");
        await Assert.That(users[2].Name).IsEqualTo("Eve");
        await Assert.That(users[3].Name).IsEqualTo("Alice");
        await Assert.That(users[4].Name).IsEqualTo("Dave");
    }

    [Test]
    public async Task Should_OrderBy_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_OrderBy_Implementation);
    [Test]
    public async Task Should_OrderBy_MySql() => await TestRunner.RunOnMySql(Should_OrderBy_Implementation);
    [Test]
    public async Task Should_OrderBy_SqlServer() => await TestRunner.RunOnSqlServer(Should_OrderBy_Implementation);
    [Test]
    public async Task Should_OrderBy_Sqlite() => await TestRunner.RunOnSqlite(Should_OrderBy_Implementation);


    // --- OrderByDescending ---

    private async Task Should_OrderByDescending_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 按年龄降序排序
        var users = await ctx.Users.OrderByDescending(u => u.Age).ToListAsync();

        // Assert
        // Sorted: Dave(40), Alice(30), Eve(25), Carol(22), Bob(17)
        await Assert.That(users[0].Name).IsEqualTo("Dave");
        await Assert.That(users[4].Name).IsEqualTo("Bob");
    }

    [Test]
    public async Task Should_OrderByDescending_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_OrderByDescending_Implementation);
    [Test]
    public async Task Should_OrderByDescending_MySql() => await TestRunner.RunOnMySql(Should_OrderByDescending_Implementation);
    [Test]
    public async Task Should_OrderByDescending_SqlServer() => await TestRunner.RunOnSqlServer(Should_OrderByDescending_Implementation);
    [Test]
    public async Task Should_OrderByDescending_Sqlite() => await TestRunner.RunOnSqlite(Should_OrderByDescending_Implementation);


    // --- Take (Limit) ---

    private async Task Should_Take_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 取前 3 个用户
        var users = await ctx.Users.OrderBy(u => u.Id).Take(3).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(3);
        await Assert.That(users[0].Id).IsEqualTo(1);
        await Assert.That(users[2].Id).IsEqualTo(3);
    }

    [Test]
    public async Task Should_Take_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Take_Implementation);
    [Test]
    public async Task Should_Take_MySql() => await TestRunner.RunOnMySql(Should_Take_Implementation);
    [Test]
    public async Task Should_Take_SqlServer() => await TestRunner.RunOnSqlServer(Should_Take_Implementation);
    [Test]
    public async Task Should_Take_Sqlite() => await TestRunner.RunOnSqlite(Should_Take_Implementation);


    // --- Skip (Offset) ---

    private async Task Should_Skip_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 跳过前 2 个，取剩下的
        // Total 5 users. Skip 2 -> 3 users.
        var users = await ctx.Users.OrderBy(u => u.Id).Skip(2).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(3);
        await Assert.That(users[0].Id).IsEqualTo(3); // 1, 2 skipped
        await Assert.That(users[1].Id).IsEqualTo(4);
        await Assert.That(users[2].Id).IsEqualTo(5);
    }

    [Test]
    public async Task Should_Skip_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Skip_Implementation);
    [Test]
    public async Task Should_Skip_MySql() => await TestRunner.RunOnMySql(Should_Skip_Implementation);
    [Test]
    public async Task Should_Skip_SqlServer() => await TestRunner.RunOnSqlServer(Should_Skip_Implementation);
    [Test]
    public async Task Should_Skip_Sqlite() => await TestRunner.RunOnSqlite(Should_Skip_Implementation);
    
    // --- Paging (Skip + Take) ---
    
    private async Task Should_Page_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 第 2 页，每页 2 条 (Skip 2, Take 2)
        // Total: 1, 2, 3, 4, 5
        // Page 1: 1, 2
        // Page 2: 3, 4
        var users = await ctx.Users.OrderBy(u => u.Id).Skip(2).Take(2).ToListAsync();
        
        // Assert
        await Assert.That(users).Count().IsEqualTo(2);
        await Assert.That(users[0].Id).IsEqualTo(3);
        await Assert.That(users[1].Id).IsEqualTo(4);
    }
    
    [Test]
    public async Task Should_Page_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Page_Implementation);
    [Test]
    public async Task Should_Page_MySql() => await TestRunner.RunOnMySql(Should_Page_Implementation);
    [Test]
    public async Task Should_Page_SqlServer() => await TestRunner.RunOnSqlServer(Should_Page_Implementation);
    [Test]
    public async Task Should_Page_Sqlite() => await TestRunner.RunOnSqlite(Should_Page_Implementation);
}

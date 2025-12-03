using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;
using System.ComponentModel.DataAnnotations.Schema;
using FastORM;

namespace FastORM.IntegrationTests.Features.Relationships;

/// <summary>
/// 关联查询 (Joins) 功能测试
/// 目标：验证 ORM 支持 Inner Join 和 Left Join 等多表关联查询。
/// </summary>
public class JoinTests
{
    [Table("users")]
    public class JoinResultDto
    {
        public string Name { get; set; } = "";
        public decimal Amount { get; set; }
    }

    // --- Inner Join ---

    private async Task Should_Inner_Join_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 连接 Users 和 Orders，查询用户姓名和订单金额
        // Seed Data:
        // Alice (Id 1): Orders 101(100.5), 102(200)
        // Bob (Id 2): Order 103(50)
        // Carol (Id 3): No orders
        // Dave (Id 4): Order 104(1000)
        // Eve (Id 5): No orders
        
        var query = ctx.Users.Join(
            ctx.Orders,
            u => u.Id,
            o => o.UserId,
            (u, o) => new JoinResultDto { Name = u.Name, Amount = o.Amount }
        );

        var results = await query.ToListAsync();

        // Assert
        // Total matches: 2 (Alice) + 1 (Bob) + 1 (Dave) = 4 rows
        await Assert.That(results).Count().IsEqualTo(4);
        
        // Verify Alice's orders
        var aliceOrders = results.Where(r => r.Name == "Alice").ToList();
        await Assert.That(aliceOrders).Count().IsEqualTo(2);
        await Assert.That(aliceOrders.Sum(r => r.Amount)).IsEqualTo(300.50m);

        // Verify Carol (should not be in inner join)
        await Assert.That(results.Any(r => r.Name == "Carol")).IsFalse();
    }

    [Test]
    public async Task Should_Inner_Join_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Inner_Join_Implementation);
    [Test]
    public async Task Should_Inner_Join_MySql() => await TestRunner.RunOnMySql(Should_Inner_Join_Implementation);
    [Test]
    public async Task Should_Inner_Join_SqlServer() => await TestRunner.RunOnSqlServer(Should_Inner_Join_Implementation);
    [Test]
    public async Task Should_Inner_Join_Sqlite() => await TestRunner.RunOnSqlite(Should_Inner_Join_Implementation);


    // --- Left Join ---

    private async Task Should_Left_Join_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 左连接 Users 和 Orders
        // Alice: 2 orders
        // Bob: 1 order
        // Carol: null
        // Dave: 1 order
        // Eve: null
        
        var query = ctx.Users.LeftJoin(
            ctx.Orders,
            u => u.Id,
            o => o.UserId,
            (u, o) => new JoinResultDto { Name = u.Name, Amount = o != null ? o.Amount : 0 }
        );

        var results = await query.ToListAsync();

        // Assert
        // Alice(2) + Bob(1) + Carol(1) + Dave(1) + Eve(1) = 6 rows
        await Assert.That(results).Count().IsEqualTo(6);

        // Verify Carol (should be present with 0 amount)
        var carol = results.FirstOrDefault(r => r.Name == "Carol");
        await Assert.That(carol).IsNotNull();
        await Assert.That(carol!.Amount).IsEqualTo(0);
    }

    [Test]
    public async Task Should_Left_Join_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Left_Join_Implementation);
    [Test]
    public async Task Should_Left_Join_MySql() => await TestRunner.RunOnMySql(Should_Left_Join_Implementation);
    [Test]
    public async Task Should_Left_Join_SqlServer() => await TestRunner.RunOnSqlServer(Should_Left_Join_Implementation);
    [Test]
    public async Task Should_Left_Join_Sqlite() => await TestRunner.RunOnSqlite(Should_Left_Join_Implementation);

    // --- Right Join ---

    private async Task Should_Right_Join_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 右连接 Users 和 Orders (相当于 Orders Left Join Users)
        // All orders have users, so result should be same as Inner Join (4 rows)
        // Unless we add an orphan order.
        
        var query = ctx.Users.RightJoin(
            ctx.Orders,
            u => u.Id,
            o => o.UserId,
            (u, o) => new JoinResultDto { Name = u != null ? u.Name : "Unknown", Amount = o.Amount }
        );

        var results = await query.ToListAsync();

        // Assert
        // Total matches: 4 rows (all orders)
        await Assert.That(results).Count().IsEqualTo(4);
    }

    [Test]
    public async Task Should_Right_Join_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Right_Join_Implementation);
    [Test]
    public async Task Should_Right_Join_MySql() => await TestRunner.RunOnMySql(Should_Right_Join_Implementation);
    [Test]
    public async Task Should_Right_Join_SqlServer() => await TestRunner.RunOnSqlServer(Should_Right_Join_Implementation);
    [Test]
    public async Task Should_Right_Join_Sqlite() => await TestRunner.RunOnSqlite(Should_Right_Join_Implementation);
}

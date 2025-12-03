using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Querying;

/// <summary>
/// 查询过滤 (Filtering) 功能测试
/// 目标：验证 ORM 能正确解析和执行 Where 子句，包括基本比较、字符串操作和组合条件。
/// </summary>
public class FilteringTests
{
    // --- Simple Where ---

    private async Task Should_Filter_By_Id_Implementation(IntegrationTestDbContext ctx)
    {
        // Act
        var user = await ctx.Users.Where(u => u.Id == 1).FirstOrDefaultAsync();

        // Assert
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Should_Filter_By_Id_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_Id_Implementation);
    [Test]
    public async Task Should_Filter_By_Id_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_Id_Implementation);
    [Test]
    public async Task Should_Filter_By_Id_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_Id_Implementation);
    [Test]
    public async Task Should_Filter_By_Id_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_Id_Implementation);


    // --- Comparison Operators ---

    private async Task Should_Filter_By_GreaterThan_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 查找年龄大于 25 的用户 (Alice=30, Dave=40)
        var users = await ctx.Users.Where(u => u.Age > 25).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(2);
        await Assert.That(users.Select(u => u.Name)).Contains("Alice");
        await Assert.That(users.Select(u => u.Name)).Contains("Dave");
    }

    [Test]
    public async Task Should_Filter_By_GreaterThan_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_GreaterThan_Implementation);
    [Test]
    public async Task Should_Filter_By_GreaterThan_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_GreaterThan_Implementation);
    [Test]
    public async Task Should_Filter_By_GreaterThan_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_GreaterThan_Implementation);
    [Test]
    public async Task Should_Filter_By_GreaterThan_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_GreaterThan_Implementation);


    // --- Multiple Conditions ---

    private async Task Should_Filter_By_Multiple_Conditions_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 查找年龄大于 20 且名字叫 "Alice" 的用户
        // 注意：FastORM 需要支持链式 Where 或 && 表达式。此处假设支持链式。
        var users = await ctx.Users.Where(u => u.Age > 20).Where(u => u.Name == "Alice").ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(1);
        await Assert.That(users[0].Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Should_Filter_By_Multiple_Conditions_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_Multiple_Conditions_Implementation);
    [Test]
    public async Task Should_Filter_By_Multiple_Conditions_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_Multiple_Conditions_Implementation);
    [Test]
    public async Task Should_Filter_By_Multiple_Conditions_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_Multiple_Conditions_Implementation);
    [Test]
    public async Task Should_Filter_By_Multiple_Conditions_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_Multiple_Conditions_Implementation);

    // --- String Operations ---

    private async Task Should_Filter_By_Contains_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 查找名字包含 "li" 的用户 (Alice)
        var users = await ctx.Users.Where(u => u.Name.Contains("li")).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(1);
        await Assert.That(users[0].Name).IsEqualTo("Alice");
    }

    [Test]
    public async Task Should_Filter_By_Contains_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_Contains_Implementation);
    [Test]
    public async Task Should_Filter_By_Contains_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_Contains_Implementation);
    [Test]
    public async Task Should_Filter_By_Contains_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_Contains_Implementation);
    [Test]
    public async Task Should_Filter_By_Contains_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_Contains_Implementation);

    private async Task Should_Filter_By_StartsWith_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 查找名字以 "Da" 开头的用户 (Dave)
        var users = await ctx.Users.Where(u => u.Name.StartsWith("Da")).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(1);
        await Assert.That(users[0].Name).IsEqualTo("Dave");
    }

    [Test]
    public async Task Should_Filter_By_StartsWith_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_StartsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_StartsWith_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_StartsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_StartsWith_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_StartsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_StartsWith_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_StartsWith_Implementation);

    private async Task Should_Filter_By_EndsWith_Implementation(IntegrationTestDbContext ctx)
    {
        // Act: 查找名字以 "ob" 结尾的用户 (Bob)
        var users = await ctx.Users.Where(u => u.Name.EndsWith("ob")).ToListAsync();

        // Assert
        await Assert.That(users).Count().IsEqualTo(1);
        await Assert.That(users[0].Name).IsEqualTo("Bob");
    }

    [Test]
    public async Task Should_Filter_By_EndsWith_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Filter_By_EndsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_EndsWith_MySql() => await TestRunner.RunOnMySql(Should_Filter_By_EndsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_EndsWith_SqlServer() => await TestRunner.RunOnSqlServer(Should_Filter_By_EndsWith_Implementation);
    [Test]
    public async Task Should_Filter_By_EndsWith_Sqlite() => await TestRunner.RunOnSqlite(Should_Filter_By_EndsWith_Implementation);
}

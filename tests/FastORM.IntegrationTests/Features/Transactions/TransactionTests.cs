using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Transactions;

/// <summary>
/// 事务功能测试
/// 验证事务的提交与回滚在真实数据库中的行为
/// </summary>
public class TransactionTests
{
    private async Task Should_Commit_Transaction_Successfully_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange
        var newUser = new User { Name = "TransUser1", Age = 20 };

        // Act
        using (await ctx.BeginTransactionAsync())
        {
            await ctx.InsertAsync(newUser);
            // 此时数据在事务中，尚未提交
            
            // 提交事务
            await ctx.CommitAsync();
        }

        // Assert
        // 验证数据已持久化
        var savedUser = await ctx.Users.Where(u => u.Name == "TransUser1").FirstOrDefaultAsync();
        await Assert.That(savedUser).IsNotNull();
        await Assert.That(savedUser!.Age).IsEqualTo(20);
    }

    [Test]
    public async Task Should_Commit_Transaction_Successfully_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Commit_Transaction_Successfully_Implementation);
    [Test]
    public async Task Should_Commit_Transaction_Successfully_MySql() => await TestRunner.RunOnMySql(Should_Commit_Transaction_Successfully_Implementation);
    [Test]
    public async Task Should_Commit_Transaction_Successfully_SqlServer() => await TestRunner.RunOnSqlServer(Should_Commit_Transaction_Successfully_Implementation);
    [Test]
    public async Task Should_Commit_Transaction_Successfully_Sqlite() => await TestRunner.RunOnSqlite(Should_Commit_Transaction_Successfully_Implementation);

    private async Task Should_Rollback_Transaction_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange
        var newUser = new User { Name = "RollbackUser", Age = 30 };

        // Act
        using (await ctx.BeginTransactionAsync())
        {
            await ctx.InsertAsync(newUser);
            
            // 验证在事务内（同一个 Context/Connection）是可以查到的（取决于实现，FastORM通常直接用Conn执行）
            // 如果是同一个连接，且没有设置隔离级别导致阻塞，应该是可见的
            // 但为了测试回滚，我们要么显式回滚，要么不提交
            
            await ctx.RollbackAsync();
        }

        // Assert
        // 验证数据不存在
        var count = await ctx.Users.Where(u => u.Name == "RollbackUser").CountAsync();
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task Should_Rollback_Transaction_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Rollback_Transaction_Implementation);
    [Test]
    public async Task Should_Rollback_Transaction_MySql() => await TestRunner.RunOnMySql(Should_Rollback_Transaction_Implementation);
    [Test]
    public async Task Should_Rollback_Transaction_SqlServer() => await TestRunner.RunOnSqlServer(Should_Rollback_Transaction_Implementation);
    [Test]
    public async Task Should_Rollback_Transaction_Sqlite() => await TestRunner.RunOnSqlite(Should_Rollback_Transaction_Implementation);
}

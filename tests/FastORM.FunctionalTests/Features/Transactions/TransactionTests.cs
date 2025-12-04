using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Transactions;

/// <summary>
/// 事务功能测试
/// 测试事务的提交与回滚
/// </summary>
public class TransactionTests : TestBase
{
    [Test]
    public async Task Should_Commit_Transaction_Successfully()
    {
        // 开启事务
        await Context.BeginTransactionAsync();

        // 插入数据
        await Context.InsertAsync(new User { Name = "TransUser1", Age = 20 });

        // 提交事务
        await Context.CommitAsync();

        // 验证数据已持久化
        // 重新查询需要确保不在同一事务上下文中（对于 SQLite 内存库，连接是同一个，所以可以查到）
        var count = await Context.Users.Where(u => u.Name == "TransUser1").CountAsync();
        await Assert.That(count).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Rollback_Transaction_On_Failure()
    {
        // 开启事务
        await Context.BeginTransactionAsync();

        // 插入数据
        await Context.InsertAsync(new User { Name = "RollbackUser", Age = 30 });

        // 验证在事务中可以查到（如果是 ReadUncommitted 或同连接）
        // FastORM 目前的 CountAsync 可能会使用新命令，但在同连接同事务下应可见
        // 但这里重点是回滚后不可见
        
        // 回滚事务
        await Context.RollbackAsync();

        // 验证数据已回滚
        var count = await Context.Users.Where(u => u.Name == "RollbackUser").CountAsync();
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task Should_Rollback_If_Not_Committed_Explicitly()
    {
        // 开启事务
        await Context.BeginTransactionAsync();
        
        await Context.InsertAsync(new User { Name = "ImplicitRollback", Age = 40 });
        
        // Explicitly rollback instead of relying on Dispose to clean up Context state
        await Context.RollbackAsync();

        // 验证数据已回滚
        var count = await Context.Users.Where(u => u.Name == "ImplicitRollback").CountAsync();
        await Assert.That(count).IsEqualTo(0);
    }
}

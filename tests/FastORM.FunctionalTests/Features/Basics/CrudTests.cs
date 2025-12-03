using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Basics;

/// <summary>
/// 基础 CRUD 功能测试
/// 包含插入、更新、删除和基本查询
/// </summary>
public class CrudTests : TestBase
{
    [Test]
    public async Task Should_Insert_And_GetById()
    {
        // 安排: 创建一个新用户
        var user = new User { Name = "Alice", Age = 25 };
        
        // 行动: 插入用户
        await Context.InsertAsync(user);

        // 断言: Id 应该被自动填充 (SQLite Autoincrement)
        await Assert.That(user.Id).IsGreaterThan(0);

        // 行动: 通过 Id 查询
        var dbUser = await Context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();

        // 断言: 数据应该一致
        await Assert.That(dbUser).IsNotNull();
        await Assert.That(dbUser!.Name).IsEqualTo("Alice");
        await Assert.That(dbUser.Age).IsEqualTo(25);
    }

    [Test]
    public async Task Should_Update_Existing_User()
    {
        // 安排
        var user = new User { Name = "Bob", Age = 30 };
        await Context.InsertAsync(user);
        int id = user.Id;

        // 行动: 修改并保存
        user.Age = 31;
        await Context.UpdateAsync(user);

        // 断言
        var updatedUser = await Context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        await Assert.That(updatedUser!.Age).IsEqualTo(31);
    }

    [Test]
    public async Task Should_Delete_User()
    {
        // 安排
        var user = new User { Name = "Charlie", Age = 40 };
        await Context.InsertAsync(user);
        int id = user.Id;

        // 行动
        await Context.DeleteAsync(user);

        // 断言
        var deletedUser = await Context.Users.Where(u => u.Id == id).FirstOrDefaultAsync();
        await Assert.That(deletedUser).IsNull();
    }
    
    [Test]
    public async Task Should_Batch_Insert()
    {
        // 安排
        var users = new[]
        {
            new User { Name = "User1", Age = 20 },
            new User { Name = "User2", Age = 21 },
            new User { Name = "User3", Age = 22 }
        };

        // 行动
        await Context.InsertAsync(users);

        // 断言
        var count = await Context.Users.CountAsync();
        await Assert.That(count).IsEqualTo(3);
    }
}

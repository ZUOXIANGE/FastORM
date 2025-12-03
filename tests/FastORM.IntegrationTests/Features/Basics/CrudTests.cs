using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Basics;

/// <summary>
/// 基础 CRUD (Create, Read, Update, Delete) 功能测试
/// 目标：验证 ORM 对基本数据库操作的支持，确保数据能正确写入、读取、更新和删除。
/// </summary>
public class CrudTests
{
    // --- Insert & Select (Read) ---

    private async Task Should_Insert_And_Retrieve_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange: 准备一个新的 User 对象
        var newUser = new User { Id = 999, Name = "NewUser", Age = 20 };

        // Act: 执行插入操作
        await ctx.InsertAsync(newUser);

        // Assert: 验证能否通过 ID 查询到该用户，且属性一致
        var user = await ctx.Users.Where(u => u.Id == 999).FirstOrDefaultAsync();
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Name).IsEqualTo("NewUser");
        await Assert.That(user.Age).IsEqualTo(20);
    }

    [Test]
    public async Task Should_Insert_And_Retrieve_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Insert_And_Retrieve_Implementation);

    [Test]
    public async Task Should_Insert_And_Retrieve_MySql() => await TestRunner.RunOnMySql(Should_Insert_And_Retrieve_Implementation);

    [Test]
    public async Task Should_Insert_And_Retrieve_SqlServer() => await TestRunner.RunOnSqlServer(Should_Insert_And_Retrieve_Implementation);

    [Test]
    public async Task Should_Insert_And_Retrieve_Sqlite() => await TestRunner.RunOnSqlite(Should_Insert_And_Retrieve_Implementation);


    // --- Update ---

    private async Task Should_Update_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange: 使用已有的种子数据 (Id=1, Name="Alice", Age=30)
        var user = await ctx.Users.Where(u => u.Id == 1).FirstOrDefaultAsync();
        await Assert.That(user).IsNotNull();
        
        user!.Name = "AliceUpdated";
        user.Age = 31;

        // Act: 执行更新操作
        await ctx.UpdateAsync(user);

        // Assert: 重新从数据库读取，验证更新是否生效
        var updatedUser = await ctx.Users.Where(u => u.Id == 1).FirstOrDefaultAsync();
        await Assert.That(updatedUser).IsNotNull();
        await Assert.That(updatedUser!.Name).IsEqualTo("AliceUpdated");
        await Assert.That(updatedUser.Age).IsEqualTo(31);
    }

    [Test]
    public async Task Should_Update_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Update_Implementation);

    [Test]
    public async Task Should_Update_MySql() => await TestRunner.RunOnMySql(Should_Update_Implementation);

    [Test]
    public async Task Should_Update_SqlServer() => await TestRunner.RunOnSqlServer(Should_Update_Implementation);

    [Test]
    public async Task Should_Update_Sqlite() => await TestRunner.RunOnSqlite(Should_Update_Implementation);


    // --- Delete ---

    private async Task Should_Delete_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange: 使用已有的种子数据 (Id=2, Name="Bob")
        var user = await ctx.Users.Where(u => u.Id == 2).FirstOrDefaultAsync();
        await Assert.That(user).IsNotNull();

        // Act: 执行删除操作
        await ctx.DeleteAsync(user!);

        // Assert: 验证再次查询时返回 null
        var deletedUser = await ctx.Users.Where(u => u.Id == 2).FirstOrDefaultAsync();
        await Assert.That(deletedUser).IsNull();
    }

    [Test]
    public async Task Should_Delete_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Delete_Implementation);

    [Test]
    public async Task Should_Delete_MySql() => await TestRunner.RunOnMySql(Should_Delete_Implementation);

    [Test]
    public async Task Should_Delete_SqlServer() => await TestRunner.RunOnSqlServer(Should_Delete_Implementation);

    [Test]
    public async Task Should_Delete_Sqlite() => await TestRunner.RunOnSqlite(Should_Delete_Implementation);
}

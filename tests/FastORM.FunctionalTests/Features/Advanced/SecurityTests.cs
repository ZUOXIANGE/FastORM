using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Advanced;

/// <summary>
/// 安全性与特殊字符测试
/// 验证 SQL 注入防御和特殊字符处理
/// </summary>
public class SecurityTests : TestBase
{
    [Test]
    public async Task Should_Handle_Single_Quotes_In_String()
    {
        // O'Neil
        var name = "O'Neil";
        var user = new User { Name = name, Age = 30 };
        
        await Context.InsertAsync(user);
        
        var dbUser = await Context.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();
        await Assert.That(dbUser).IsNotNull();
        await Assert.That(dbUser!.Name).IsEqualTo(name);
    }

    [Test]
    public async Task Should_Prevent_Sql_Injection_In_Insert()
    {
        // 尝试注入 SQL
        var maliciousName = "'; DROP TABLE Users; --";
        var user = new User { Name = maliciousName, Age = 666 };
        
        await Context.InsertAsync(user);
        
        // 验证表还在，且数据正确
        var dbUser = await Context.Users.Where(u => u.Name == maliciousName).FirstOrDefaultAsync();
        await Assert.That(dbUser).IsNotNull();
        
        var count = await Context.Users.CountAsync();
        await Assert.That(count).IsGreaterThan(0);
    }

    [Test]
    public async Task Should_Prevent_Sql_Injection_In_Query()
    {
        await Context.InsertAsync(new User { Name = "SafeUser", Age = 20 });

        // 尝试在查询条件中注入
        var maliciousInput = "' OR '1'='1";
        // 如果没有参数化，可能会变成 SELECT * FROM Users WHERE Name = '' OR '1'='1'
        
        var result = await Context.Users.Where(u => u.Name == maliciousInput).ToListAsync();
        
        // 应该查不到任何东西，因为没有叫 "' OR '1'='1" 的用户
        await Assert.That(result).IsEmpty();
    }
}

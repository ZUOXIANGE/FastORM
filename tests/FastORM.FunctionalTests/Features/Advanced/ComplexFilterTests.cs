using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Advanced;

/// <summary>
/// 复杂查询条件测试
/// 验证逻辑运算符优先级、括号组合等
/// </summary>
public class ComplexFilterTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        var users = new[]
        {
            new User { Name = "A_Adult", Age = 20 },
            new User { Name = "A_Child", Age = 10 },
            new User { Name = "B_Adult", Age = 20 },
            new User { Name = "B_Child", Age = 10 },
            new User { Name = "C_Senior", Age = 60 }
        };
        await Context.InsertAsync(users);
    }

    [Test]
    public async Task Should_Respect_Operator_Precedence()
    {
        // (Name contains 'A' OR Name contains 'B') AND Age == 20
        // Should return A_Adult, B_Adult.
        // If precedence is wrong (A OR (B AND Age=20)), it might include A_Child.
        
        var results = await Context.Users
            .Where(u => (u.Name.Contains("A") || u.Name.Contains("B")) && u.Age == 20)
            .ToListAsync();

        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results.Any(u => u.Name == "A_Adult")).IsTrue();
        await Assert.That(results.Any(u => u.Name == "B_Adult")).IsTrue();
        await Assert.That(results.Any(u => u.Name == "A_Child")).IsFalse();
    }

    [Test]
    public async Task Should_Handle_Multiple_And_Or()
    {
        // Age > 50 OR (Age < 15 AND Name Contains "B")
        // Should return C_Senior (60) and B_Child (10, "B")
        
        var results = await Context.Users
            .Where(u => u.Age > 50 || (u.Age < 15 && u.Name.Contains("B")))
            .ToListAsync();

        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results.Any(u => u.Name == "C_Senior")).IsTrue();
        await Assert.That(results.Any(u => u.Name == "B_Child")).IsTrue();
    }
}

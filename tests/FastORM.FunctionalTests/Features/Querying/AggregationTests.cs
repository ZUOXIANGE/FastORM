using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 聚合查询测试
/// 测试 Count, Sum, Min, Max, Average
/// </summary>
public class AggregationTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        var users = new[]
        {
            new User { Name = "A", Age = 10 },
            new User { Name = "B", Age = 20 },
            new User { Name = "C", Age = 30 },
            new User { Name = "D", Age = 40 },
            new User { Name = "E", Age = 50 }
        };
        await Context.InsertAsync(users);
    }

    [Test]
    public async Task Should_Count_All()
    {
        var count = await Context.Users.CountAsync();
        await Assert.That(count).IsEqualTo(5);
    }

    [Test]
    public async Task Should_Count_With_Predicate()
    {
        // Count(u => u.Age > 30) -> 40, 50 -> 2
        var count = await Context.Users.Where(u => u.Age > 30).CountAsync();
        await Assert.That(count).IsEqualTo(2);
    }

    [Test]
    public async Task Should_Calculate_Sum()
    {
        // Sum(Age) = 10+20+30+40+50 = 150
        var sum = await Context.Users.SumAsync(u => u.Age);
        await Assert.That(sum).IsEqualTo(150);
    }

    [Test]
    public async Task Should_Calculate_Max()
    {
        var max = await Context.Users.MaxAsync(u => u.Age);
        await Assert.That(max).IsEqualTo(50);
    }

    [Test]
    public async Task Should_Calculate_Min()
    {
        var min = await Context.Users.MinAsync(u => u.Age);
        await Assert.That(min).IsEqualTo(10);
    }
    
    [Test]
    public async Task Should_Calculate_Average()
    {
        // Avg = 150 / 5 = 30
        var avg = await Context.Users.AverageAsync(u => u.Age);
        await Assert.That(avg).IsEqualTo(30);
    }
}

using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 查询过滤功能测试
/// 测试 Where 子句的各种组合情况
/// </summary>
public class FilteringTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        // 预置数据
        var users = new[]
        {
            new User { Name = "Alice", Age = 25 },
            new User { Name = "Bob", Age = 30 },
            new User { Name = "Charlie", Age = 35 },
            new User { Name = "David", Age = 25 }, // 同龄人
            new User { Name = "Eva", Age = 40 }
        };
        await Context.InsertAsync(users);
    }

    [Test]
    public async Task Should_Filter_By_Simple_Equality()
    {
        // 测试: Where(u => u.Age == 30)
        var users = await Context.Users.Where(u => u.Age == 30).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(1);
        await Assert.That(users[0].Name).IsEqualTo("Bob");
    }

    [Test]
    public async Task Should_Filter_By_String_Contains()
    {
        // 测试: Where(u => u.Name.Contains("li")) -> Alice, Charlie
        var users = await Context.Users.Where(u => u.Name.Contains("li")).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(2);
        // 注意: 顺序不一定保证，除非 OrderBy
        var names = users.Select(u => u.Name).OrderBy(n => n).ToArray();
        await Assert.That(names).Contains("Alice");
        await Assert.That(names).Contains("Charlie");
    }

    [Test]
    public async Task Should_Filter_By_Multiple_Conditions()
    {
        // 测试: Where(u => u.Age > 20 && u.Age < 35) -> Alice(25), Bob(30), David(25)
        var users = await Context.Users.Where(u => u.Age > 20 && u.Age < 35).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Should_Filter_By_List_Contains()
    {
        // 测试: Where(u => list.Contains(u.Name))
        var targetNames = new[] { "Alice", "Eva" };
        var users = await Context.Users.Where(u => targetNames.Contains(u.Name)).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Should_Use_FirstOrDefault_With_Predicate()
    {
        // 测试: FirstOrDefault(u => u.Name == "David")
        var user = await Context.Users.Where(u => u.Name == "David").FirstOrDefaultAsync();
        
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Age).IsEqualTo(25);
    }
    
    [Test]
    public async Task Should_Return_Null_When_NotFound()
    {
        var user = await Context.Users.Where(u => u.Name == "NonExistent").FirstOrDefaultAsync();
        await Assert.That(user).IsNull();
    }
    
    [Test]
    public async Task Should_Handle_Boolean_Logic_Or()
    {
        // 测试: Where(u => u.Name == "Alice" || u.Age == 40) -> Alice, Eva
        var users = await Context.Users.Where(u => u.Name == "Alice" || u.Age == 40).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(2);
    }
}

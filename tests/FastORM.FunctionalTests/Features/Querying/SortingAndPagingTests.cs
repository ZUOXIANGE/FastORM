using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 排序与分页功能测试
/// 测试 OrderBy, Skip, Take
/// </summary>
public class SortingAndPagingTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        // 插入 10 个用户，年龄混杂
        var users = new List<User>();
        for (int i = 1; i <= 10; i++)
        {
            users.Add(new User { Name = $"User{i}", Age = 20 + (i % 5) }); // Age: 21, 22, 23, 24, 20, 21...
        }
        await Context.InsertAsync(users);
    }

    [Test]
    public async Task Should_OrderBy_Ascending()
    {
        // 按年龄升序
        var users = await Context.Users.OrderBy(u => u.Age).ToListAsync();
        
        await Assert.That(users.Count).IsEqualTo(10);
        await Assert.That(users[0].Age).IsEqualTo(20);
        await Assert.That(users[9].Age).IsEqualTo(24);
    }

    [Test]
    public async Task Should_OrderBy_Descending()
    {
        // 按年龄降序
        var users = await Context.Users.OrderByDescending(u => u.Age).ToListAsync();
        
        await Assert.That(users[0].Age).IsEqualTo(24);
        await Assert.That(users[9].Age).IsEqualTo(20);
    }

    [Test]
    public async Task Should_Skip_And_Take()
    {
        // 排序后分页: 取第 2 页，每页 3 条 (Skip 3, Take 3)
        // UserIds: 1..10. Order by Id by default if not specified? No, usually undefined.
        // Let's order by Id explicitly to be deterministic.
        
        var pagedUsers = await Context.Users
            .OrderBy(u => u.Id)
            .Skip(3)
            .Take(3)
            .ToListAsync();

        await Assert.That(pagedUsers.Count).IsEqualTo(3);
        // Expected: User4, User5, User6
        await Assert.That(pagedUsers[0].Name).IsEqualTo("User4");
        await Assert.That(pagedUsers[2].Name).IsEqualTo("User6");
    }
    
    [Test]
    public async Task Should_Combine_Where_Order_Take()
    {
        // 复杂查询: 年龄 > 22, 按 Name 降序, 取前 2 个
        // Ages: 21(1), 22(2), 23(3), 24(4), 20(5), 21(6), 22(7), 23(8), 24(9), 20(10)
        // > 22: User3(23), User4(24), User8(23), User9(24)
        // OrderBy Name Desc: User9, User8, User4, User3
        // Take 2: User9, User8
        
        var result = await Context.Users
            .Where(u => u.Age > 22)
            .OrderByDescending(u => u.Name)
            .Take(2)
            .ToListAsync();
            
        await Assert.That(result.Count).IsEqualTo(2);
        await Assert.That(result[0].Name).IsEqualTo("User9");
        await Assert.That(result[1].Name).IsEqualTo("User8");
    }
}

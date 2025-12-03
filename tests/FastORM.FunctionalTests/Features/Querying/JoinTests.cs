using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 联表查询测试
/// 测试 Inner Join, Left Join 以及投影到 DTO
/// </summary>
public class JoinTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        // Users: Alice(1), Bob(2), Charlie(3)
        var user1 = new User { Name = "Alice", Age = 25 };
        var user2 = new User { Name = "Bob", Age = 30 };
        var user3 = new User { Name = "Charlie", Age = 35 };
        await Context.InsertAsync(user1);
        await Context.InsertAsync(user2);
        await Context.InsertAsync(user3);

        // Orders:
        // Alice -> 100
        // Alice -> 200
        // Bob -> 300
        // Charlie -> No Orders
        var orders = new[]
        {
            new Order { UserId = user1.Id, Amount = 100, OrderDate = DateTime.Now },
            new Order { UserId = user1.Id, Amount = 200, OrderDate = DateTime.Now },
            new Order { UserId = user2.Id, Amount = 300, OrderDate = DateTime.Now }
        };
        await Context.InsertAsync(orders);
    }

    [Test]
    public async Task Should_Inner_Join_Users_And_Orders()
    {
        // Join Users and Orders on Id == UserId
        // Result should be 3 rows: Alice-100, Alice-200, Bob-300. Charlie excluded.
        
        var query = Context.Users
            .Join(
                Context.Orders,
                user => user.Id,
                order => order.UserId,
                (user, order) => new JoinResult { Name = user.Name, Amount = order.Amount }
            );

        var results = await query.ToListAsync();

        await Assert.That(results.Count).IsEqualTo(3);
        
        var aliceOrders = results.Where(r => r.Name == "Alice").ToList();
        await Assert.That(aliceOrders.Count).IsEqualTo(2);
        
        var bobOrders = results.Where(r => r.Name == "Bob").ToList();
        await Assert.That(bobOrders.Count).IsEqualTo(1);
        
        var charlieOrders = results.Where(r => r.Name == "Charlie").ToList();
        await Assert.That(charlieOrders).IsEmpty();
    }

    [Test]
    public async Task Should_Left_Join_Users_And_Orders()
    {
        // Left Join Users and Orders
        // Result should include Charlie with Amount 0 (or default)
        
        var query = Context.Users
            .LeftJoin(
                Context.Orders,
                user => user.Id,
                order => order.UserId,
                (user, order) => new JoinResult { Name = user.Name, Amount = order.Amount }
            );

        var results = await query.ToListAsync();

        // Alice(2) + Bob(1) + Charlie(1) = 4
        await Assert.That(results.Count).IsEqualTo(4);
        
        var charlie = results.FirstOrDefault(r => r.Name == "Charlie");
        await Assert.That(charlie).IsNotNull();
        // Default decimal is 0
        await Assert.That(charlie!.Amount).IsEqualTo(0);
    }
    
    [Test]
    public async Task Should_Join_With_Where_Clause()
    {
        // Join Users and Orders, but filter Amount > 150
        // Should match: Alice-200, Bob-300
        
        var query = Context.Users
            .Join(
                Context.Orders,
                user => user.Id,
                order => order.UserId,
                (user, order) => new JoinResult { Name = user.Name, Amount = order.Amount }
            )
            .Where(r => r.Amount > 150);

        var results = await query.ToListAsync();
        
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results.Any(r => r.Name == "Alice")).IsTrue();
        await Assert.That(results.Any(r => r.Name == "Bob")).IsTrue();
    }
}

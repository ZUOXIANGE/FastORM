using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 复杂查询测试
/// 包含多条件、子查询逻辑、链式调用等
/// </summary>
public class ComplexQueryTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        var users = new List<User>();
        for (int i = 1; i <= 20; i++)
        {
            users.Add(new User { Name = $"User{i}", Age = 10 + i }); // Age: 11..30
        }
        await Context.InsertAsync(users);
    }

    [Test]
    public async Task Should_Execute_Complex_Where_Clause()
    {
        // (Age > 25 AND Name contains "2") OR (Age < 15)
        // Age > 25: 26..30 (User16..User20)
        // Name contains "2": User2, User12, User20
        // Intersection (Age > 25 AND "2"): User20 (Age 30)
        // Age < 15: 11..14 (User1..User4)
        // Total expected: User1, User2, User3, User4, User20 -> 5 users
        
        var users = await Context.Users
            .Where(u => (u.Age > 25 && u.Name.Contains("2")) || u.Age < 15)
            .ToListAsync();

        await Assert.That(users.Count).IsEqualTo(5);
        await Assert.That(users.Any(u => u.Name == "User20")).IsTrue();
        await Assert.That(users.Any(u => u.Name == "User1")).IsTrue();
    }

    [Test]
    public async Task Should_Chained_Where_Clauses()
    {
        // Where(Age > 20).Where(Age < 25) -> 21, 22, 23, 24
        var users = await Context.Users
            .Where(u => u.Age > 20)
            .Where(u => u.Age < 25)
            .ToListAsync();

        await Assert.That(users.Count).IsEqualTo(4);
    }

    [Test]
    public async Task Should_Combine_Where_OrderBy_Skip_Take()
    {
        // Age > 15 (16..30) -> 15 users
        // OrderBy Age Desc -> 30..16
        // Skip 5 -> 25..16
        // Take 3 -> 25, 24, 23
        
        var users = await Context.Users
            .Where(u => u.Age > 15)
            .OrderByDescending(u => u.Age)
            .Skip(5)
            .Take(3)
            .ToListAsync();

        await Assert.That(users.Count).IsEqualTo(3);
        await Assert.That(users[0].Age).IsEqualTo(25);
        await Assert.That(users[1].Age).IsEqualTo(24);
        await Assert.That(users[2].Age).IsEqualTo(23);
    }
}

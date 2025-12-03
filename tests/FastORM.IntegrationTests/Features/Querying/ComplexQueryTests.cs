using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.Querying;

/// <summary>
/// 复杂查询测试
/// 包含多条件、链式调用等复杂场景
/// </summary>
public class ComplexQueryTests
{
    // --- Complex Where ---

    private async Task Should_Execute_Complex_Where_Clause_Implementation(IntegrationTestDbContext ctx)
    {
        // 场景：(Age > 25 AND Name contains "l") OR (Age < 25)
        // Data:
        // 1: Alice, 30 -> Age>25 (True), Name contains "l" (True) -> Match
        // 2: Bob, 17 -> Age>25 (False), Age<25 (True) -> Match
        // 3: Carol, 22 -> Age>25 (False), Age<25 (True) -> Match
        // 4: Dave, 40 -> Age>25 (True), Name contains "l" (False) -> No Match
        // 5: Eve, 25 -> Age>25 (False), Age<25 (False) -> No Match
        
        // Expected: Alice, Bob, Carol

        var users = await ctx.Users
            .Where(u => (u.Age > 25 && u.Name.Contains("l")) || u.Age < 25)
            .OrderBy(u => u.Id)
            .ToListAsync();

        await Assert.That(users).Count().IsEqualTo(3);
        await Assert.That(users[0].Name).IsEqualTo("Alice");
        await Assert.That(users[1].Name).IsEqualTo("Bob");
        await Assert.That(users[2].Name).IsEqualTo("Carol");
    }

    [Test]
    public async Task Should_Execute_Complex_Where_Clause_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Execute_Complex_Where_Clause_Implementation);
    [Test]
    public async Task Should_Execute_Complex_Where_Clause_MySql() => await TestRunner.RunOnMySql(Should_Execute_Complex_Where_Clause_Implementation);
    [Test]
    public async Task Should_Execute_Complex_Where_Clause_SqlServer() => await TestRunner.RunOnSqlServer(Should_Execute_Complex_Where_Clause_Implementation);
    [Test]
    public async Task Should_Execute_Complex_Where_Clause_Sqlite() => await TestRunner.RunOnSqlite(Should_Execute_Complex_Where_Clause_Implementation);

    // --- Chained Query ---

    private async Task Should_Combine_Where_OrderBy_Skip_Take_Implementation(IntegrationTestDbContext ctx)
    {
        // Data:
        // 1: Alice, 30
        // 2: Bob, 17
        // 3: Carol, 22
        // 4: Dave, 40
        // 5: Eve, 25

        // Query: Age > 20 -> Alice(30), Carol(22), Dave(40), Eve(25)
        // OrderBy Age Desc -> Dave(40), Alice(30), Eve(25), Carol(22)
        // Skip 1 -> Alice(30), Eve(25), Carol(22)
        // Take 2 -> Alice(30), Eve(25)

        var users = await ctx.Users
            .Where(u => u.Age > 20)
            .OrderByDescending(u => u.Age)
            .Skip(1)
            .Take(2)
            .ToListAsync();

        await Assert.That(users).Count().IsEqualTo(2);
        await Assert.That(users[0].Name).IsEqualTo("Alice");
        await Assert.That(users[1].Name).IsEqualTo("Eve");
    }

    [Test]
    public async Task Should_Combine_Where_OrderBy_Skip_Take_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Combine_Where_OrderBy_Skip_Take_Implementation);
    [Test]
    public async Task Should_Combine_Where_OrderBy_Skip_Take_MySql() => await TestRunner.RunOnMySql(Should_Combine_Where_OrderBy_Skip_Take_Implementation);
    [Test]
    public async Task Should_Combine_Where_OrderBy_Skip_Take_SqlServer() => await TestRunner.RunOnSqlServer(Should_Combine_Where_OrderBy_Skip_Take_Implementation);
    [Test]
    public async Task Should_Combine_Where_OrderBy_Skip_Take_Sqlite() => await TestRunner.RunOnSqlite(Should_Combine_Where_OrderBy_Skip_Take_Implementation);
}

using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Setup;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.IntegrationTests.Features.Schema;

public class SchemaTests
{
    [Table("SchemaTestTable")]
    public class SchemaTestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    private async Task Should_Create_And_Drop_Implementation(IntegrationTestDbContext ctx)
    {
        // 0. Ensure clean state (Drop if exists)
        try
        {
            await ctx.DropTableAsync<SchemaTestEntity>();
        }
        catch { /* Ignore */ }

        // 1. Create Table
        await ctx.CreateTableAsync<SchemaTestEntity>();

        // 2. Insert Data
        var entity = new SchemaTestEntity { Name = "IntegrationTest", Age = 99 };
        await ctx.InsertAsync(entity);
        await Assert.That(entity.Id).IsGreaterThan(0);

        // 4. Drop Table
        await ctx.DropTableAsync<SchemaTestEntity>();

        // 5. Verify Drop (Insert should fail)
        var entity2 = new SchemaTestEntity { Name = "Fail", Age = 1 };
        try 
        {
            await ctx.InsertAsync(entity2);
            Assert.Fail("Insert should fail after dropping table");
        }
        catch
        {
            // Expected
        }
    }

    [Test]
    public async Task Should_Create_And_Drop_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Create_And_Drop_Implementation);

    [Test]
    public async Task Should_Create_And_Drop_MySql() => await TestRunner.RunOnMySql(Should_Create_And_Drop_Implementation);

    [Test]
    public async Task Should_Create_And_Drop_SqlServer() => await TestRunner.RunOnSqlServer(Should_Create_And_Drop_Implementation);

    [Test]
    public async Task Should_Create_And_Drop_Sqlite() => await TestRunner.RunOnSqlite(Should_Create_And_Drop_Implementation);
}

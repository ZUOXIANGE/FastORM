using FastORM;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;
using TUnit.Assertions;
using TUnit.Core;

namespace FastORM.FunctionalTests.Features.Schema;

public class IndexTests
{
    [Index("Name", IsUnique = true)]
    public class UniqueIndexEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [Index("Category", "Value", Name = "IX_Custom_Composite")]
    public class CompositeIndexEntity
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public int Value { get; set; }
    }

    [Test]
    public async Task CreateTable_WithUniqueIndex_ShouldEnforceUniqueness()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);
        
        // 1. Create Table with Index
        await context.CreateTableAsync<UniqueIndexEntity>();

        // 2. Insert first record
        var e1 = new UniqueIndexEntity { Name = "Unique" };
        await context.InsertAsync(e1);

        // 3. Insert duplicate record - should fail
        var e2 = new UniqueIndexEntity { Name = "Unique" };
        try
        {
            await context.InsertAsync(e2);
            Assert.Fail("Should have thrown exception due to unique index violation");
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // 19 is Constraint Violation
        {
            // Expected
        }
        catch (Exception ex)
        {
             // Fallback for other exceptions, but verify it is related to constraint
             if (!ex.Message.Contains("constraint")) throw;
        }
    }

    [Test]
    public async Task CreateTable_WithCompositeIndex_ShouldSucceed()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        // 1. Create Table with Index
        await context.CreateTableAsync<CompositeIndexEntity>();

        // 2. Insert records
        var e1 = new CompositeIndexEntity { Category = "A", Value = 1 };
        var e2 = new CompositeIndexEntity { Category = "A", Value = 2 };
        await context.InsertAsync(e1);
        await context.InsertAsync(e2);

        // 3. Verify data
        var q = new FastOrmQueryable<CompositeIndexEntity>(context, "CompositeIndexEntity");
        var count = await q.CountAsync();
        await Assert.That(count).IsEqualTo(2);
    }
}

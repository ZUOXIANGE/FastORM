using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Features.Schema;

public class ColumnAttributeTests
{
    [Table("ColumnAttrTestTable")]
    public class ColumnAttrEntity
    {
        public int Id { get; set; }
        
        [Column("custom_name_col")]
        public string? Name { get; set; }
        
        [Column("custom_age_col")]
        public int Age { get; set; }
    }

    [Test]
    public async Task Should_Use_Custom_Column_Name_Sqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        // 1. Create Table
        await context.CreateTableAsync<ColumnAttrEntity>();

        // Verify table structure
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA table_info(ColumnAttrTestTable)";
            using var reader = await cmd.ExecuteReaderAsync();
            var columns = new List<string>();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(1)); // name column
            }
            
            await Assert.That(columns).Contains("custom_name_col");
            await Assert.That(columns).Contains("custom_age_col");
            await Assert.That(columns).DoesNotContain("Name");
            await Assert.That(columns).DoesNotContain("Age");
        }

        // 2. Insert Data
        var entity = new ColumnAttrEntity { Name = "Test", Age = 25 };
        await context.InsertAsync(entity);
        
        // Verify data using raw SQL
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT custom_name_col, custom_age_col FROM ColumnAttrTestTable WHERE Id = 1";
            using var reader = await cmd.ExecuteReaderAsync();
            await Assert.That(await reader.ReadAsync()).IsTrue();
            await Assert.That(reader.GetString(0)).IsEqualTo("Test");
            await Assert.That(reader.GetInt32(1)).IsEqualTo(25);
        }

        // 3. Select Data (ORM)
        // Use FastOrmQueryable directly since we don't have a DbSet property for this test entity
        var queryable = new FastOrmQueryable<ColumnAttrEntity>(context, "ColumnAttrTestTable");
        var result = await queryable.FirstOrDefaultAsync();
        await Assert.That(result).IsNotNull();
        await Assert.That(result!.Name).IsEqualTo("Test");
        await Assert.That(result.Age).IsEqualTo(25);

        // 4. Update Data
        entity.Name = "Updated";
        await context.UpdateAsync(entity);
        
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT custom_name_col FROM ColumnAttrTestTable WHERE Id = 1";
            var val = await cmd.ExecuteScalarAsync();
            await Assert.That(val).IsEqualTo("Updated");
        }
    }
}

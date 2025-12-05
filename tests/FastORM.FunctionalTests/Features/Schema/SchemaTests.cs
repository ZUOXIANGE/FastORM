using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;

namespace FastORM.FunctionalTests.Features.Schema;

public class SchemaTests
{
    public class SchemaTestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
    }

    [Test]
    public async Task Should_Create_And_Drop_Table_Sqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        // 1. Create Table
        await context.CreateTableAsync<SchemaTestEntity>();

        // Verify table exists by checking sqlite_master
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='SchemaTestEntity'";
            var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
            await Assert.That(count).IsEqualTo(1);
        }

        // 2. Insert Data (to ensure schema is correct)
        var entity = new SchemaTestEntity { Name = "Test", Age = 10 };
        await context.InsertAsync(entity);
        await Assert.That(entity.Id).IsGreaterThan(0);

        // 3. Drop Table
        await context.DropTableAsync<SchemaTestEntity>();

        // Verify table is gone
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='SchemaTestEntity'";
            var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
            await Assert.That(count).IsEqualTo(0);
        }
    }
}

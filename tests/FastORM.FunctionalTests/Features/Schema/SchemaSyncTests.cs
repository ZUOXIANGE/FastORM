using FastORM;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;
using TUnit.Assertions;
using TUnit.Core;

namespace FastORM.FunctionalTests.Features.Schema;

public class SchemaSyncTests
{
    [Table("SyncTestTable")]
    public class SyncEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [Test]
    public async Task CreateAndDropTable_Sync_ShouldSucceed()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        // Create
        context.CreateTable<SyncEntity>();

        // Verify Exists
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SyncTestTable'";
            var result = cmd.ExecuteScalar();
            await Assert.That(result).IsNotNull();
        }

        // Drop
        context.DropTable<SyncEntity>();

        // Verify Gone
        using (var cmd = connection.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='SyncTestTable'";
            var result = cmd.ExecuteScalar();
            await Assert.That(result).IsNull();
        }
    }
}

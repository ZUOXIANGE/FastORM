using FastORM;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TUnit.Assertions;
using TUnit.Core;

namespace FastORM.FunctionalTests.Features.Schema;

public class SchemaIndexTests
{
    [Table("IndexTestTable")]
    [Index("Email", IsUnique = true, Name = "IX_Email")]
    [Index("FirstName", "LastName", Name = "IX_FullName")]
    [Index("CreatedAt")] // Default name, non-unique
    public class IndexEntity
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    [Test]
    public async Task CreateTable_WithIndexes_ShouldSucceedOnSqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        await context.CreateTableAsync<IndexEntity>();

        // Verify Indexes
        var indexes = new List<(string Name, bool IsUnique)>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='index' AND tbl_name='IndexTestTable'";
        using (var reader = await cmd.ExecuteReaderAsync())
        {
             while (await reader.ReadAsync())
            {
                var name = reader.GetString(0);
                var sql = reader.IsDBNull(1) ? "" : reader.GetString(1);
                
                // sqlite_autoindex... usually doesn't have SQL or is internal
                if (name.StartsWith("sqlite_autoindex")) continue;

                indexes.Add((name, sql.Contains("UNIQUE")));
            }
        }

        await Assert.That(indexes).Count().IsEqualTo(3);
        await Assert.That(indexes.Any(i => i.Name == "IX_Email" && i.IsUnique)).IsTrue();
        await Assert.That(indexes.Any(i => i.Name == "IX_FullName" && !i.IsUnique)).IsTrue();
        // The third one might have an auto-generated name like IX_IndexTestTable_CreatedAt
        await Assert.That(indexes.Any(i => i.Name.Contains("CreatedAt") && !i.IsUnique)).IsTrue();
    }
}

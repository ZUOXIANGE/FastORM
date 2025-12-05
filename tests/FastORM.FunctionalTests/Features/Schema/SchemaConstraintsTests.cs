using FastORM;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TUnit.Assertions;
using TUnit.Core;

namespace FastORM.FunctionalTests.Features.Schema;

public class SchemaConstraintsTests
{
    [Table("ConstraintTestTable")]
    [Index("CategoryId", Name = "IX_Price", IsUnique = false)]
    public class ConstraintEntity
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Name { get; set; } = "";

        [Column(TypeName = "VARCHAR(100)")]
        public string CustomType { get; set; } = "";

        [DefaultValueSql("'UNKNOWN'")]
        public string Status { get; set; } = "UNKNOWN";

        [Precision(10, 4)]
        public decimal Price { get; set; }
        
        public int CategoryId { get; set; }

        [NotMapped]
        public string IgnoredProperty { get; set; } = "Ignored";
    }

    [Test]
    public async Task CreateTable_WithConstraints_ShouldSucceedOnSqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        // 1. Create Table
        await context.CreateTableAsync<ConstraintEntity>();

        // 2. Verify Schema via PRAGMA
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(ConstraintTestTable)";
        using var reader = await cmd.ExecuteReaderAsync();
        
        var columns = new Dictionary<string, string>();
        while (await reader.ReadAsync())
        {
            var name = reader.GetString(1);
            var type = reader.GetString(2);
            columns[name] = type;
        }
        
        await Assert.That(columns.ContainsKey("Id")).IsTrue();
        await Assert.That(columns.ContainsKey("Name")).IsTrue();
        await Assert.That(columns.ContainsKey("Price")).IsTrue();
        
        // Verify NotMapped
        await Assert.That(columns.ContainsKey("IgnoredProperty")).IsFalse();

        // Verify Type Constraints
        // SQLite stores types as they are declared in CREATE TABLE.
        // SchemaEmitter for SQLite:
        // String + MaxLength(50) -> VARCHAR(50)
        await Assert.That(columns["Name"]).IsEqualTo("VARCHAR(50)");
        
        // Custom Type -> VARCHAR(100)
        await Assert.That(columns["CustomType"]).IsEqualTo("VARCHAR(100)");
        
        // Decimal with Precision -> DECIMAL(10,4)
        await Assert.That(columns["Price"]).IsEqualTo("DECIMAL(10,4)");
    }

    [Test]
    public async Task CreateTable_WithConstraints_ShouldEnforceNotNull()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        await context.CreateTableAsync<ConstraintEntity>();

        // Try insert null for Required column (via raw SQL)
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO ConstraintTestTable (Name, Price, CategoryId) VALUES (NULL, 10, 1)";
        try
        {
            await cmd.ExecuteNonQueryAsync();
            // If no exception, fail
            // SQLite enforces NOT NULL
            // But check if exception is thrown
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19) // Constraint violation
        {
            // Expected
            return;
        }
        
        // If we reach here, it might have succeeded (which is bad) or failed with different error
        // Assert.Fail("Should fail due to NOT NULL constraint");
        // But if it succeeded, we need to fail.
        // However, sometimes SQLite configuration might not enforce constraints if not enabled?
        // By default, NOT NULL is enforced.
        
        // Let's re-verify if it actually failed.
        // If catch block wasn't entered, we are here.
        await Assert.That(true).IsFalse(); // Fail
    }
    
    [Test]
    public async Task CreateTable_WithConstraints_ShouldUseDefaultValue()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        await context.CreateTableAsync<ConstraintEntity>();

        // Insert without Status
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "INSERT INTO ConstraintTestTable (Name, Price, CategoryId) VALUES ('Test', 10.5, 1)";
        await cmd.ExecuteNonQueryAsync();
        
        // Verify default value
        using var cmd2 = connection.CreateCommand();
        cmd2.CommandText = "SELECT Status FROM ConstraintTestTable WHERE Name = 'Test'";
        var status = (string?)await cmd2.ExecuteScalarAsync();
        
        await Assert.That(status).IsEqualTo("UNKNOWN");
    }
}

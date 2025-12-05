using FastORM;
using FastORM.FunctionalTests.Contexts;
using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations.Schema;
using TUnit.Assertions;
using TUnit.Core;

namespace FastORM.FunctionalTests.Features.Schema;

public class SchemaDataTypesTests
{
    [Table("AllTypesTable")]
    public class AllTypesEntity
    {
        public int Id { get; set; }
        public long LongValue { get; set; }
        public short ShortValue { get; set; }
        public byte ByteValue { get; set; }
        public bool BoolValue { get; set; }
        public float FloatValue { get; set; }
        public double DoubleValue { get; set; }
        public decimal DecimalValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public Guid GuidValue { get; set; }
        public byte[] ByteArrayValue { get; set; } = Array.Empty<byte>();
        public DateOnly DateOnlyValue { get; set; }
        public TimeOnly TimeOnlyValue { get; set; }
        public DateTimeOffset DateTimeOffsetValue { get; set; }
        public string StringValue { get; set; } = "";
    }

    [Test]
    public async Task CreateTable_WithAllTypes_ShouldSucceedOnSqlite()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

        await context.CreateTableAsync<AllTypesEntity>();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info(AllTypesTable)";
        using var reader = await cmd.ExecuteReaderAsync();
        
        var columns = new Dictionary<string, string>();
        while (await reader.ReadAsync())
        {
            columns[reader.GetString(1)] = reader.GetString(2);
        }

        // Verify SQLite mappings
        await Assert.That(columns["Id"]).IsEqualTo("INTEGER");
        await Assert.That(columns["LongValue"]).IsEqualTo("INTEGER");
        await Assert.That(columns["ShortValue"]).IsEqualTo("INTEGER");
        await Assert.That(columns["ByteValue"]).IsEqualTo("INTEGER");
        await Assert.That(columns["BoolValue"]).IsEqualTo("INTEGER"); // SQLite has no BOOL
        await Assert.That(columns["FloatValue"]).IsEqualTo("REAL");
        await Assert.That(columns["DoubleValue"]).IsEqualTo("REAL");
        // SQLite treats DECIMAL as NUMERIC or REAL. SchemaEmitter uses NUMERIC for decimal if no precision/scale or generic.
        // In SchemaEmitter for SQLite: if (def.Precision...) DECIMAL(...) else NUMERIC.
        // Here no precision, so NUMERIC.
        await Assert.That(columns["DecimalValue"]).IsEqualTo("NUMERIC");
        
        // DateTime -> TEXT in SchemaEmitter for SQLite
        await Assert.That(columns["DateTimeValue"]).IsEqualTo("TEXT");
        
        await Assert.That(columns["GuidValue"]).IsEqualTo("TEXT");
        await Assert.That(columns["StringValue"]).IsEqualTo("TEXT");
        
        // ByteArray -> BLOB
        await Assert.That(columns["ByteArrayValue"]).IsEqualTo("BLOB");
        
        // DateOnly -> TEXT
        await Assert.That(columns["DateOnlyValue"]).IsEqualTo("TEXT");
        
        // TimeOnly -> TEXT
        await Assert.That(columns["TimeOnlyValue"]).IsEqualTo("TEXT");
        
        // DateTimeOffset -> TEXT
        await Assert.That(columns["DateTimeOffsetValue"]).IsEqualTo("TEXT");
    }
}

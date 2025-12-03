using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;

namespace FastORM.IntegrationTests.Features.DataTypes;

public class DataTypeTests
{
    private async Task Should_Support_All_Data_Types_Implementation(IntegrationTestDbContext ctx)
    {
        // Arrange
        var expected = new SupportedTypes
        {
            // Id will be auto-generated
            StringProp = "Test String",
            IntProp = 42,
            LongProp = 1234567890123456789L,
            DecimalProp = 123.45m,
            DoubleProp = 3.14159,
            BoolProp = true,
            DateTimeProp = new DateTime(2023, 10, 1, 12, 0, 0), // Avoid ms precision issues for cross-db consistency
            GuidProp = Guid.NewGuid(),
            DateOnlyProp = new DateOnly(2023, 10, 1),
            TimeOnlyProp = new TimeOnly(14, 30, 0),
            DateTimeOffsetProp = new DateTimeOffset(2023, 10, 1, 12, 0, 0, TimeSpan.FromHours(2)),
            EnumProp = TestEnum.Second
        };

        // Act
        await ctx.InsertAsync(expected);

        // Assert
        var actual = await ctx.SupportedTypes.Where(x => x.GuidProp == expected.GuidProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.Id).IsGreaterThan(0);
        await Assert.That(actual.StringProp).IsEqualTo(expected.StringProp);
        await Assert.That(actual.IntProp).IsEqualTo(expected.IntProp);
        await Assert.That(actual.LongProp).IsEqualTo(expected.LongProp);
        await Assert.That(actual.DecimalProp).IsEqualTo(expected.DecimalProp);
        // Double comparison with tolerance
        await Assert.That(Math.Abs(actual.DoubleProp - expected.DoubleProp)).IsLessThan(0.0001);
        await Assert.That(actual.BoolProp).IsEqualTo(expected.BoolProp);
        
        // DateTime comparison
        await Assert.That(actual.DateTimeProp).IsEqualTo(expected.DateTimeProp);
        
        // Guid comparison
        await Assert.That(actual.GuidProp).IsEqualTo(expected.GuidProp);

        // New types comparison
        await Assert.That(actual.DateOnlyProp).IsEqualTo(expected.DateOnlyProp);
        await Assert.That(actual.TimeOnlyProp).IsEqualTo(expected.TimeOnlyProp);
        await Assert.That(actual.DateTimeOffsetProp).IsEqualTo(expected.DateTimeOffsetProp);
        await Assert.That(actual.EnumProp).IsEqualTo(expected.EnumProp);
    }

    [Test]
    public async Task Should_Support_All_Data_Types_PostgreSql() => await TestRunner.RunOnPostgreSql(Should_Support_All_Data_Types_Implementation);
    [Test]
    public async Task Should_Support_All_Data_Types_MySql() => await TestRunner.RunOnMySql(Should_Support_All_Data_Types_Implementation);
    [Test]
    public async Task Should_Support_All_Data_Types_SqlServer() => await TestRunner.RunOnSqlServer(Should_Support_All_Data_Types_Implementation);
    [Test]
    public async Task Should_Support_All_Data_Types_Sqlite() => await TestRunner.RunOnSqlite(Should_Support_All_Data_Types_Implementation);
}

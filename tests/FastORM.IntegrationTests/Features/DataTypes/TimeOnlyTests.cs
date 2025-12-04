using FastORM.IntegrationTests.Contexts;
using FastORM.IntegrationTests.Entities;
using FastORM.IntegrationTests.Setup;
using TUnit.Assertions;

namespace FastORM.IntegrationTests.Features.DataTypes;

public class TimeOnlyTests
{
    [Test]
    public async Task Should_Support_Null_TimeOnly()
    {
        await TestRunner.RunOnSqlite(TestNullTimeOnly);
        await TestRunner.RunOnSqlServer(TestNullTimeOnly);
        await TestRunner.RunOnPostgreSql(TestNullTimeOnly);
        await TestRunner.RunOnMySql(TestNullTimeOnly);
    }

    private async Task TestNullTimeOnly(IntegrationTestDbContext context)
    {
        var expected = new SupportedTypes
        {
            StringProp = "Null TimeOnly",
            TimeOnlyProp = null,
            DateTimeProp = new DateTime(2023, 1, 1)
        };

        await context.InsertAsync(expected);
        var actual = await context.SupportedTypes.Where(x => x.StringProp == expected.StringProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.TimeOnlyProp).IsNull();
    }

    [Test]
    public async Task Should_Support_Min_TimeOnly()
    {
        await TestRunner.RunOnSqlite(TestMinTimeOnly);
        await TestRunner.RunOnSqlServer(TestMinTimeOnly);
        await TestRunner.RunOnPostgreSql(TestMinTimeOnly);
        await TestRunner.RunOnMySql(TestMinTimeOnly);
    }

    private async Task TestMinTimeOnly(IntegrationTestDbContext context)
    {
        var expected = new SupportedTypes
        {
            StringProp = "Min TimeOnly",
            TimeOnlyProp = TimeOnly.MinValue,
            DateTimeProp = new DateTime(2023, 1, 1)
        };

        await context.InsertAsync(expected);
        var actual = await context.SupportedTypes.Where(x => x.StringProp == expected.StringProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.TimeOnlyProp).IsNotNull();
        await Assert.That(actual.TimeOnlyProp).IsEqualTo(TimeOnly.MinValue);
    }

    [Test]
    public async Task Should_Support_Max_TimeOnly()
    {
        await TestRunner.RunOnSqlite(TestMaxTimeOnly);
        await TestRunner.RunOnSqlServer(TestMaxTimeOnly);
        await TestRunner.RunOnPostgreSql(TestMaxTimeOnly);
        await TestRunner.RunOnMySql(TestMaxTimeOnly);
    }

    private async Task TestMaxTimeOnly(IntegrationTestDbContext context)
    {
        var expectedTime = TimeOnly.MaxValue;

        // MySql and Postgres have 6 decimal places precision (microseconds).
        // TimeOnly.MaxValue has 7 (ticks).
        // If we insert TimeOnly.MaxValue (23:59:59.9999999), it might round up to 24:00:00 in the DB,
        // which causes issues when reading back (TimeSpan of 1 day is invalid for TimeOnly).
        // So we truncate to microseconds for these dialects.
        if (context.Dialect == SqlDialect.PostgreSql || context.Dialect == SqlDialect.MySql)
        {
             var ticks = expectedTime.Ticks;
             var truncatedTicks = ticks - (ticks % 10); 
             expectedTime = new TimeOnly(truncatedTicks);
        }

        var expected = new SupportedTypes
        {
            StringProp = "Max TimeOnly",
            TimeOnlyProp = expectedTime,
            DateTimeProp = new DateTime(2023, 1, 1)
        };

        await context.InsertAsync(expected);
        var actual = await context.SupportedTypes.Where(x => x.StringProp == expected.StringProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.TimeOnlyProp).IsNotNull();
        await Assert.That(actual.TimeOnlyProp).IsEqualTo(expected.TimeOnlyProp);
    }

    [Test]
    public async Task Should_Support_TimeOnly_With_Milliseconds()
    {
        await TestRunner.RunOnSqlite(TestTimeOnlyWithMilliseconds);
        await TestRunner.RunOnSqlServer(TestTimeOnlyWithMilliseconds);
        await TestRunner.RunOnPostgreSql(TestTimeOnlyWithMilliseconds);
        await TestRunner.RunOnMySql(TestTimeOnlyWithMilliseconds);
    }

    private async Task TestTimeOnlyWithMilliseconds(IntegrationTestDbContext context)
    {
        // Use a value that doesn't suffer from rounding issues between 7 and 6 decimal places.
        // 1234560 ticks = 123456.0 microseconds.
        var expectedTime = new TimeOnly(14, 30, 15).Add(TimeSpan.FromTicks(1234560));
        
        var expected = new SupportedTypes
        {
            StringProp = "TimeOnly Milliseconds",
            TimeOnlyProp = expectedTime,
            DateTimeProp = new DateTime(2023, 1, 1)
        };

        await context.InsertAsync(expected);
        var actual = await context.SupportedTypes.Where(x => x.StringProp == expected.StringProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.TimeOnlyProp).IsEqualTo(expected.TimeOnlyProp);
    }

    [Test]
    public async Task Should_Filter_By_TimeOnly_Sqlite()
    {
        await TestRunner.RunOnSqlite(TestFilterByTimeOnly);
    }

    [Test]
    public async Task Should_Filter_By_TimeOnly_SqlServer() => await TestRunner.RunOnSqlServer(TestFilterByTimeOnly);

    [Test]
    public async Task Should_Filter_By_TimeOnly_PostgreSql() => await TestRunner.RunOnPostgreSql(TestFilterByTimeOnly);

    [Test]
    public async Task Should_Filter_By_TimeOnly_MySql() => await TestRunner.RunOnMySql(TestFilterByTimeOnly);

    private async Task TestFilterByTimeOnly(IntegrationTestDbContext context)
    {
        var time1 = new TimeOnly(10, 0, 0);
        var time2 = new TimeOnly(12, 0, 0);
        var time3 = new TimeOnly(14, 0, 0);

        await context.InsertAsync(new SupportedTypes { StringProp = "Time1", TimeOnlyProp = time1, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Time2", TimeOnlyProp = time2, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Time3", TimeOnlyProp = time3, DateTimeProp = new DateTime(2023, 1, 1) });

        // Debug: Verify insertion and reading
        var all = await context.SupportedTypes.ToListAsync();
        await Assert.That(all).IsNotNull();
        await Assert.That(all.Count).IsEqualTo(3);
        var t2 = all.FirstOrDefault(x => x.StringProp == "Time2");
        await Assert.That(t2).IsNotNull();
        await Assert.That(t2!.TimeOnlyProp).IsEqualTo(time2);

        // Exact match
        var match = await context.SupportedTypes.Where(x => x.TimeOnlyProp == time2).FirstOrDefaultAsync();
        await Assert.That(match).IsNotNull();
        await Assert.That(match!.StringProp).IsEqualTo("Time2");

        // Greater than
        var gt = await context.SupportedTypes.Where(x => x.TimeOnlyProp > time2).FirstOrDefaultAsync();
        await Assert.That(gt).IsNotNull();
        await Assert.That(gt!.StringProp).IsEqualTo("Time3");

        // Less than
        var lt = await context.SupportedTypes.Where(x => x.TimeOnlyProp < time2).FirstOrDefaultAsync();
        await Assert.That(lt).IsNotNull();
        await Assert.That(lt!.StringProp).IsEqualTo("Time1");
    }

    [Test]
    public async Task Should_OrderBy_TimeOnly()
    {
        await TestRunner.RunOnSqlite(TestOrderByTimeOnly);
        await TestRunner.RunOnSqlServer(TestOrderByTimeOnly);
        await TestRunner.RunOnPostgreSql(TestOrderByTimeOnly);
        await TestRunner.RunOnMySql(TestOrderByTimeOnly);
    }

    private async Task TestOrderByTimeOnly(IntegrationTestDbContext context)
    {
        var time1 = new TimeOnly(10, 0, 0);
        var time2 = new TimeOnly(12, 0, 0);
        var time3 = new TimeOnly(14, 0, 0);

        await context.InsertAsync(new SupportedTypes { StringProp = "Sort2", TimeOnlyProp = time2, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Sort1", TimeOnlyProp = time1, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Sort3", TimeOnlyProp = time3, DateTimeProp = new DateTime(2023, 1, 1) });

        var sorted = await context.SupportedTypes
            .OrderBy(x => x.TimeOnlyProp)
            .ToListAsync();

        await Assert.That(sorted.Count).IsEqualTo(3);
        await Assert.That(sorted[0].TimeOnlyProp).IsEqualTo(time1);
        await Assert.That(sorted[1].TimeOnlyProp).IsEqualTo(time2);
        await Assert.That(sorted[2].TimeOnlyProp).IsEqualTo(time3);

        var desc = await context.SupportedTypes
            .OrderByDescending(x => x.TimeOnlyProp)
            .ToListAsync();
            
        await Assert.That(desc[0].TimeOnlyProp).IsEqualTo(time3);
        await Assert.That(desc[1].TimeOnlyProp).IsEqualTo(time2);
        await Assert.That(desc[2].TimeOnlyProp).IsEqualTo(time1);
    }

    [Test]
    public async Task Should_Filter_By_TimeOnly_Range()
    {
        await TestRunner.RunOnSqlite(TestFilterByTimeOnlyRange);
        await TestRunner.RunOnSqlServer(TestFilterByTimeOnlyRange);
        await TestRunner.RunOnPostgreSql(TestFilterByTimeOnlyRange);
        await TestRunner.RunOnMySql(TestFilterByTimeOnlyRange);
    }

    private async Task TestFilterByTimeOnlyRange(IntegrationTestDbContext context)
    {
        var time1 = new TimeOnly(10, 0, 0);
        var time2 = new TimeOnly(12, 0, 0);
        var time3 = new TimeOnly(14, 0, 0);

        await context.InsertAsync(new SupportedTypes { StringProp = "Range1", TimeOnlyProp = time1, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Range2", TimeOnlyProp = time2, DateTimeProp = new DateTime(2023, 1, 1) });
        await context.InsertAsync(new SupportedTypes { StringProp = "Range3", TimeOnlyProp = time3, DateTimeProp = new DateTime(2023, 1, 1) });

        // Range: 11:00 to 13:00 -> should match Time2 (12:00)
        var start = new TimeOnly(11, 0, 0);
        var end = new TimeOnly(13, 0, 0);
        
        var match = await context.SupportedTypes
            .Where(x => x.TimeOnlyProp > start && x.TimeOnlyProp < end)
            .FirstOrDefaultAsync();

        await Assert.That(match).IsNotNull();
        await Assert.That(match!.StringProp).IsEqualTo("Range2");

        // >=
        var ge = await context.SupportedTypes.Where(x => x.TimeOnlyProp >= time2).OrderBy(x => x.TimeOnlyProp).ToListAsync();
        await Assert.That(ge).Count().IsEqualTo(2);
        await Assert.That(ge[0].StringProp).IsEqualTo("Range2");
        await Assert.That(ge[1].StringProp).IsEqualTo("Range3");

        // <=
        var le = await context.SupportedTypes.Where(x => x.TimeOnlyProp <= time2).OrderBy(x => x.TimeOnlyProp).ToListAsync();
        await Assert.That(le).Count().IsEqualTo(2);
        await Assert.That(le[0].StringProp).IsEqualTo("Range1");
        await Assert.That(le[1].StringProp).IsEqualTo("Range2");
    }
}


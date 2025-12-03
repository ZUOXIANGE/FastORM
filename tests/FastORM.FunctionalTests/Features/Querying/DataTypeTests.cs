using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Querying;

/// <summary>
/// 数据类型支持测试
/// 测试 Guid, DateTime, Bool, Decimal 等类型的读写和查询
/// </summary>
public class DataTypeTests : TestBase
{
    private Guid _catId1 = Guid.NewGuid();
    private Guid _catId2 = Guid.NewGuid();

    [Before(Test)]
    public async Task SeedData()
    {
        var products = new[]
        {
            new Product 
            { 
                Name = "Laptop", 
                Price = 999.99m, 
                CreatedAt = new DateTime(2023, 1, 1), 
                IsActive = true, 
                CategoryId = _catId1 
            },
            new Product 
            { 
                Name = "Mouse", 
                Price = 29.50m, 
                CreatedAt = new DateTime(2023, 2, 1), 
                IsActive = true, 
                CategoryId = _catId2 
            },
            new Product 
            { 
                Name = "Old Keyboard", 
                Price = 15.00m, 
                CreatedAt = new DateTime(2020, 1, 1), 
                IsActive = false, 
                CategoryId = _catId1 
            }
        };
        await Context.InsertAsync(products);
    }

    [Test]
    public async Task Should_Filter_By_Guid()
    {
        var products = await Context.Products.Where(p => p.CategoryId == _catId1).ToListAsync();
        
        await Assert.That(products.Count).IsEqualTo(2);
        await Assert.That(products.Any(p => p.Name == "Laptop")).IsTrue();
        await Assert.That(products.Any(p => p.Name == "Old Keyboard")).IsTrue();
    }

    [Test]
    public async Task Should_Filter_By_Boolean()
    {
        var activeProducts = await Context.Products.Where(p => p.IsActive == true).ToListAsync();
        await Assert.That(activeProducts.Count).IsEqualTo(2);

        var inactiveProducts = await Context.Products.Where(p => p.IsActive == false).ToListAsync();
        await Assert.That(inactiveProducts.Count).IsEqualTo(1);
        await Assert.That(inactiveProducts[0].Name).IsEqualTo("Old Keyboard");
    }

    [Test]
    public async Task Should_Filter_By_DateTime()
    {
        var date = new DateTime(2022, 1, 1);
        var newProducts = await Context.Products.Where(p => p.CreatedAt > date).ToListAsync();
        
        await Assert.That(newProducts.Count).IsEqualTo(2); // Laptop (2023), Mouse (2023)
    }

    [Test]
    public async Task Should_Filter_By_Decimal()
    {
        var expensiveProducts = await Context.Products.Where(p => p.Price > 100m).ToListAsync();
        
        await Assert.That(expensiveProducts.Count).IsEqualTo(1);
        await Assert.That(expensiveProducts[0].Name).IsEqualTo("Laptop");
    }

    [Test]
    public async Task Should_Support_Extended_Data_Types()
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
            DateTimeProp = new DateTime(2023, 10, 1, 12, 0, 0),
            GuidProp = Guid.NewGuid(),
            DateOnlyProp = new DateOnly(2023, 10, 1),
            DateTimeOffsetProp = new DateTimeOffset(2023, 10, 1, 12, 0, 0, TimeSpan.FromHours(2)),
            EnumProp = TestEnum.Second
        };

        // Act
        await Context.InsertAsync(expected);

        // Assert
        var actual = await Context.SupportedTypes.Where(x => x.GuidProp == expected.GuidProp).FirstOrDefaultAsync();

        await Assert.That(actual).IsNotNull();
        await Assert.That(actual!.Id).IsGreaterThan(0);
        await Assert.That(actual.StringProp).IsEqualTo(expected.StringProp);
        await Assert.That(actual.IntProp).IsEqualTo(expected.IntProp);
        await Assert.That(actual.LongProp).IsEqualTo(expected.LongProp);
        await Assert.That(actual.DecimalProp).IsEqualTo(expected.DecimalProp);
        await Assert.That(Math.Abs(actual.DoubleProp - expected.DoubleProp)).IsLessThan(0.0001);
        await Assert.That(actual.BoolProp).IsEqualTo(expected.BoolProp);
        await Assert.That(actual.DateTimeProp).IsEqualTo(expected.DateTimeProp);
        await Assert.That(actual.GuidProp).IsEqualTo(expected.GuidProp);
        
        // New types
        await Assert.That(actual.DateOnlyProp).IsEqualTo(expected.DateOnlyProp);
        await Assert.That(actual.DateTimeOffsetProp).IsEqualTo(expected.DateTimeOffsetProp);
        await Assert.That(actual.EnumProp).IsEqualTo(expected.EnumProp);
    }
}

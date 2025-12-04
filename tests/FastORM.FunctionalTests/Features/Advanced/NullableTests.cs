using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Setup;

namespace FastORM.FunctionalTests.Features.Advanced;

/// <summary>
/// 可空类型测试
/// 验证 Nullable<T> 的存储与查询支持
/// </summary>
public class NullableTests : TestBase
{
    [Before(Test)]
    public async Task SeedData()
    {
        var items = new[]
        {
            // 全不为空
            new NullableEntity 
            { 
                IntVal = 100, 
                BoolVal = true, 
                DateVal = new DateTime(2023, 1, 1), 
                StringVal = "NotNull" 
            },
            // 部分为空
            new NullableEntity 
            { 
                IntVal = null, 
                BoolVal = false, 
                DateVal = null, 
                StringVal = null 
            },
            // 全为空 (除了Id)
            new NullableEntity 
            { 
                IntVal = null, 
                BoolVal = null, 
                DateVal = null, 
                StringVal = null 
            }
        };
        await Context.InsertAsync(items);
    }

    [Test]
    public async Task Should_Query_By_Null()
    {
        // 查找 IntVal 为 null 的记录 (应该有 2 条)
        var results = await Context.Nullables.Where(x => x.IntVal == null).ToListAsync();
        
        await Assert.That(results.Count).IsEqualTo(2);
        await Assert.That(results.All(x => x.IntVal == null)).IsTrue();
    }

    [Test]
    public async Task Should_Query_By_NotNull()
    {
        // 查找 IntVal 不为 null 的记录 (应该有 1 条)
        var results = await Context.Nullables.Where(x => x.IntVal != null).ToListAsync();
        
        await Assert.That(results.Count).IsEqualTo(1);
        await Assert.That(results[0].IntVal).IsEqualTo(100);
    }

    [Test]
    public async Task Should_Query_By_Value_On_Nullable_Column()
    {
        // 查找 IntVal == 100
        var results = await Context.Nullables.Where(x => x.IntVal == 100).ToListAsync();
        
        await Assert.That(results.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Handle_Bool_Nulls()
    {
        // True: 1, False: 1, Null: 1
        var trueVal = await Context.Nullables.Where(x => x.BoolVal == true).CountAsync();
        var falseVal = await Context.Nullables.Where(x => x.BoolVal == false).CountAsync();
        var nullVal = await Context.Nullables.Where(x => x.BoolVal == null).CountAsync();

        await Assert.That(trueVal).IsEqualTo(1);
        await Assert.That(falseVal).IsEqualTo(1);
        await Assert.That(nullVal).IsEqualTo(1);
    }

    [Test]
    public async Task Should_Handle_String_Nulls()
    {
        var nullStrings = await Context.Nullables.Where(x => x.StringVal == null).CountAsync();
        await Assert.That(nullStrings).IsEqualTo(2);

        var notNullStrings = await Context.Nullables.Where(x => x.StringVal != null).CountAsync();
        await Assert.That(notNullStrings).IsEqualTo(1);
    }
}

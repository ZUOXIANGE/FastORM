using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.BatchOperations;

/// <summary>
/// 条件批量更新/删除演示
/// 展示如何不先查询出数据，直接根据 Where 条件进行更新或删除
/// </summary>
public static class BatchUpdateDeleteDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 条件批量操作演示 (Batch Update/Delete) ===");

        // 准备测试数据
        await PrepareDataAsync(ctx);

        // 1. 条件批量更新 (Where ... Update)
        // 将所有 "TmpUser" 的年龄修改为 100
        // 这种方式直接生成 UPDATE ... WHERE 语句，性能极高
        var updateCount = await ctx.Person
            .Where(p => p.Name.StartsWith("TmpUser"))
            .UpdateAsync(p => p.Age = 100);

        Console.WriteLine($"[条件更新] 将 TmpUser 的年龄改为 100，受影响行数: {updateCount}");

        // 验证更新
        var checkUpdate = await ctx.Person
            .Where(p => p.Name.StartsWith("TmpUser"))
            .FirstOrDefaultAsync();
        if (checkUpdate != null)
        {
            Console.WriteLine($" - 验证: {checkUpdate.Name} 的年龄现在是 {checkUpdate.Age}");
        }

        // 2. 条件批量删除 (Where ... Delete)
        // 删除所有 "TmpUser"
        // 直接生成 DELETE FROM ... WHERE 语句
        var deleteCount = await ctx.Person
            .Where(p => p.Name.StartsWith("TmpUser"))
            .DeleteAsync();

        Console.WriteLine($"[条件删除] 删除所有 TmpUser，受影响行数: {deleteCount}");

        Console.WriteLine();
    }

    private static async Task PrepareDataAsync(MyDbContext ctx)
    {
        // 插入一些临时数据供测试
        var data = new[]
        {
            new Person { Name = "TmpUser_A", Age = 10 },
            new Person { Name = "TmpUser_B", Age = 20 },
            new Person { Name = "TmpUser_C", Age = 30 }
        };
        
        // 先清理可能存在的旧数据
        // 使用 Name 前缀来识别测试数据
        await ctx.Person.Where(p => p.Name.StartsWith("TmpUser")).DeleteAsync();

        await ctx.InsertAsync(data);
        Console.WriteLine($"[准备数据] 插入了 {data.Length} 条临时记录。");
    }
}

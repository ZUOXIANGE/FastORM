using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FastORM.SampleApp.Demos.BatchOperations;

/// <summary>
/// 批量操作演示
/// 展示 Bulk Insert 和 Bulk Delete 的高效使用
/// </summary>
public static class BulkDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 批量操作演示 (Bulk Operations) ===");

        // 1. 批量插入 (Bulk Insert)
        // 一次性插入多条记录，比循环调用 InsertAsync 效率更高
        var newPeople = new[]
        {
            new Person { Name = "BatchUser1", Age = 20 },
            new Person { Name = "BatchUser2", Age = 22 },
            new Person { Name = "BatchUser3", Age = 24 }
        };

        var insertCount = await ctx.InsertAsync(newPeople);
        Console.WriteLine($"[批量插入] 成功插入 {insertCount} 条记录。");

        // 验证插入结果
        // 由于 ID 是自动生成的，我们使用 Name 来查询
        var names = newPeople.Select(p => p.Name).ToList();
        var fetched = await ctx.Person
            .Where(p => names.Contains(p.Name)) // 使用 IN 查询
            .ToListAsync();

        Console.WriteLine($"[验证] 查询刚才插入的记录:");
        foreach (var p in fetched)
        {
            Console.WriteLine($" - {p.Name} (ID: {p.Id})");
        }

        // 2. 批量删除 (Bulk Delete)
        // 传入对象列表进行批量删除 (通常依据主键)
        // 注意：这里我们需要使用查询出来的 fetched 列表，因为它们包含了数据库生成的 ID
        var deleteCount = await ctx.DeleteAsync(fetched);
        Console.WriteLine($"[批量删除] 成功删除 {deleteCount} 条记录。");

        Console.WriteLine();
    }
}

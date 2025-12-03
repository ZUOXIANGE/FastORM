using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.DynamicQuerying;

/// <summary>
/// 动态查询演示
/// 展示如何根据运行时条件动态构建查询 (IQueryable Chaining)
/// </summary>
public static class DynamicQueryDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 动态查询演示 (Dynamic Querying) ===");

        // 模拟用户输入的筛选条件 (可能为空)
        string? inputName = "Alice"; // 用户想找名字叫 Alice 的人
        int? minAge = 18;            // 用户想找 18 岁以上的人

        Console.WriteLine($"[模拟输入] Name={inputName}, MinAge={minAge}");

        // 1. 开始构建查询
        // 使用 IQueryable 接口来保持查询的延迟执行特性
        IQueryable<Person> query = ctx.Person;

        // 2. 动态添加条件
        // FastORM 支持在运行时根据逻辑拼接 Where 条件
        // 当 Source Generator 检测到这种模式时，会自动切换到运行时翻译模式
        
        if (!string.IsNullOrEmpty(inputName))
        {
            // 添加姓名过滤
            query = query.Where(p => p.Name == inputName);
        }

        if (minAge.HasValue)
        {
            // 添加年龄过滤
            // 注意：这里直接使用了局部变量 minAge，FastORM 支持捕获局部变量
            query = query.Where(p => p.Age >= minAge.Value);
        }

        // 3. 添加排序
        query = query.OrderBy(p => p.Name);

        // 4. 执行查询
        var results = await query.ToListAsync();

        Console.WriteLine($"[结果] 找到 {results.Count} 条符合条件的记录:");
        foreach (var p in results)
        {
            Console.WriteLine($" - {p.Name} ({p.Age}岁)");
        }

        Console.WriteLine();
    }
}

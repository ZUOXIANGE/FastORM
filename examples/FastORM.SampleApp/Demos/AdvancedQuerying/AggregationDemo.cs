using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.AdvancedQuerying;

/// <summary>
/// 聚合查询演示
/// 展示 Count, Sum, Max, Min, Average 等聚合函数的使用
/// </summary>
public static class AggregationDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 聚合查询演示 (Aggregations) ===");

        // 1. 计数 (Count)
        // 统计年龄 >= 18 的人数
        var adultCount = await ctx.Person
            .Where(p => p.Age >= 18)
            .CountAsync();
        Console.WriteLine($"[Count] 成年人数: {adultCount}");

        // 2. 最大值 (Max)
        // 获取最大年龄
        // 注意：如果表为空，某些数据库可能返回 null，需要处理
        var maxAge = await ctx.Person.MaxAsync(p => p.Age);
        Console.WriteLine($"[Max] 最大年龄: {maxAge}");

        // 3. 最小值 (Min)
        var minAge = await ctx.Person.MinAsync(p => p.Age);
        Console.WriteLine($"[Min] 最小年龄: {minAge}");

        // 4. 求和 (Sum)
        // 计算所有订单的总金额
        var totalAmount = await ctx.Orders.SumAsync(o => o.Amount);
        Console.WriteLine($"[Sum] 订单总金额: {totalAmount}");

        // 5. 平均值 (Average)
        // 计算订单平均金额
        var avgAmount = await ctx.Orders.AverageAsync(o => o.Amount);
        Console.WriteLine($"[Average] 订单平均金额: {avgAmount:F2}");

        Console.WriteLine();
    }
}

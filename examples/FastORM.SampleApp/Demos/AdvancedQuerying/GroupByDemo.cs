using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.AdvancedQuerying;

/// <summary>
/// 分组查询演示
/// 展示 GroupBy 及其后的聚合操作
/// </summary>
public static class GroupByDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 分组查询演示 (Group By) ===");

        // 1. 简单分组计数
        // 按年龄分组，并统计每个年龄段的人数
        var ageGroups = await ctx.Person
            .GroupBy(p => p.Age)
            .Select(g => new AgeCount 
            { 
                Key = g.Key,     // 分组键 (Age)
                Count = g.Count() // 组内数量
            })
            .OrderByDescending(x => x.Count) // 按人数降序排列
            .ToListAsync();

        Console.WriteLine($"[GroupBy] 按年龄分组统计人数:");
        foreach (var g in ageGroups)
        {
            Console.WriteLine($" - Age {g.Key}: {g.Count} 人");
        }

        // 2. 联表分组求和
        // 先联表，再分组，再聚合
        // 统计每个人 (按名字) 的订单总金额
        var userTotals = await ctx.Person
            .Join(
                ctx.Orders, 
                p => p.Id, 
                o => o.UserId, 
                (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount }
            )
            .GroupBy(r => r.Name) // 按名字分组
            .Select(g => new UserTotals 
            { 
                Name = g.Key, 
                Total = g.Sum(r => r.Amount) // 计算组内 Amount 总和
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        Console.WriteLine($"[GroupBy + Join] 用户订单总金额排名:");
        foreach (var t in userTotals)
        {
            Console.WriteLine($" - {t.Name}: {t.Total}");
        }

        Console.WriteLine();
    }
}

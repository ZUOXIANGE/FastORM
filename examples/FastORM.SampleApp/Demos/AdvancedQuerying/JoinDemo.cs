using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.AdvancedQuerying;

/// <summary>
/// 联表查询演示
/// 展示 Inner Join, Left Join, Right Join 的使用
/// </summary>
public static class JoinDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 联表查询演示 (Joins) ===");

        // 确保有一些订单数据
        await EnsureOrderDataAsync(ctx);

        // 1. 内连接 (Inner Join)
        // 注意：FastORM 目前对 Join 后的 Where 支持可能有限，建议在 Join 前过滤或在内存中过滤
        var innerResults = await ctx.Person
            .Join(
                ctx.Orders, 
                static p => p.Id, 
                static o => o.UserId, 
                static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount }
            )
            .OrderByDescending(static r => r.Amount)
            .ToListAsync();
        
        Console.WriteLine($"[Inner Join] 订单金额排名:");
        foreach (var r in innerResults)
        {
            Console.WriteLine($" - {r.Name}: {r.Amount}");
        }

        // 2. 左连接 (Left Join)
        var leftResults = await ctx.Person
            .LeftJoin(
                ctx.Orders,
                static p => p.Id,
                static o => o.UserId, 
                static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount }
            )
            .OrderBy(static r => r.Name)
            .ToListAsync();

        Console.WriteLine($"[Left Join] 所有用户及其订单:");
        foreach (var r in leftResults)
        {
            Console.WriteLine($" - {r.Name}: {r.Amount}");
        }

        // 3. 右连接 (Right Join)
        var rightResults = await ctx.Person
            .RightJoin(
                ctx.Orders,
                static p => p.Id,
                static o => o.UserId, 
                static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount }
            )
            .Take(5)
            .ToListAsync();

        Console.WriteLine($"[Right Join] 前 5 个订单及其所属用户:");
        foreach (var r in rightResults)
        {
            Console.WriteLine($" - {r.Name}: {r.Amount}");
        }

        Console.WriteLine();
    }

    private static async Task EnsureOrderDataAsync(MyDbContext ctx)
    {
        var count = await ctx.Orders.CountAsync();
        if (count == 0)
        {
            await ctx.InsertAsync(new[]
            {
                new Order { Id = 1, UserId = 101, Amount = 100 }, // Bob
                new Order { Id = 2, UserId = 101, Amount = 25 },  // Bob
                new Order { Id = 3, UserId = 102, Amount = 200 }, // Charlie
                new Order { Id = 4, UserId = 999, Amount = 500 }  // UserId 999 (Unknown person)
            });
        }
    }
}

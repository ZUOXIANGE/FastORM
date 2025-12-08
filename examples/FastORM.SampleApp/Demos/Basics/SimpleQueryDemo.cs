using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.Basics;

/// <summary>
/// 简单查询演示
/// 展示 Where, OrderBy, Take, ToList 等基本查询功能
/// </summary>
public static class SimpleQueryDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 简单查询演示 (Simple Query) ===");

        // 准备一些测试数据
        await EnsureTestDataAsync(ctx);

        // 1. 基本过滤与排序 (Filtering and Sorting)
        // 查询所有年龄大于 18 岁的人，按姓名排序，取前 5 个
        var query = ctx.Person
            .Where(p => p.Age > 18)      // 过滤条件
            .OrderBy(p => p.Name)        // 排序
            .Take(5);                    // 限制返回数量

        // 执行查询 (ToListAsync 触发数据库访问)
        var results = await query.ToListAsync();

        Console.WriteLine($"[查询] 年龄 > 18 的前 5 人 (按姓名排序):");
        foreach (var p in results)
        {
            Console.WriteLine($" - {p.Name} ({p.Age}岁)");
        }

        // 2. 分页查询 (Paging)
        // 使用 Skip 和 Take 实现分页
        int pageIndex = 1; // 第 2 页 (从 0 开始)
        int pageSize = 2;

        var pagedResults = await ctx.Person
            .OrderBy(p => p.Id)          // 分页通常需要确定的排序
            .Skip(pageIndex * pageSize)  // 跳过前 N 条
            .Take(pageSize)              // 取 M 条
            .ToListAsync();

        Console.WriteLine($"[分页] 第 {pageIndex + 1} 页 (每页 {pageSize} 条):");
        foreach (var p in pagedResults)
        {
            Console.WriteLine($" - ID: {p.Id}, Name: {p.Name}");
        }

        Console.WriteLine();
    }

    private static async Task EnsureTestDataAsync(MyDbContext ctx)
    {
        // 简单的检查和插入数据，确保演示有数据可用
        var count = await ctx.Person.CountAsync();
        if (count < 10)
        {
            var people = new[]
            {
                new Person { Name = "Bob", Age = 20 },
                new Person { Name = "Charlie", Age = 30 },
                new Person { Name = "David", Age = 40 },
                new Person { Name = "Eva", Age = 22 },
                new Person { Name = "Frank", Age = 35 },
                new Person { Name = "Grace", Age = 28 },
                new Person { Name = "Helen", Age = 45 },
                new Person { Name = "Ivan", Age = 19 },
                new Person { Name = "Jack", Age = 50 },
                new Person { Name = "Kelly", Age = 33 }
            };
            
            // 过滤掉已存在的记录 (避免重复)
            foreach(var p in people)
            {
                var exists = await ctx.Person.Where(x => x.Name == p.Name && x.Age == p.Age).CountAsync() > 0;
                if (!exists) await ctx.InsertAsync(p);
            }
        }
    }
}

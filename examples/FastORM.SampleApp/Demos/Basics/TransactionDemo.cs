using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.Basics;

/// <summary>
/// 事务操作演示
/// 展示如何开启、提交和回滚事务
/// </summary>
public static class TransactionDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 事务操作演示 (Transactions) ===");

        // 1. 成功的事务提交
        Console.WriteLine("[事务 1] 开始执行一个成功的事务...");
        
        var person1 = new Person { Name = "TransUser1", Age = 20 };
        var person2 = new Person { Name = "TransUser2", Age = 22 };

        using (var transaction = await ctx.BeginTransactionAsync())
        {
            try
            {
                // 插入一条记录
                await ctx.InsertAsync(person1);
                Console.WriteLine($" - 插入了: {person1.Name}, ID: {person1.Id}");

                // 插入另一条记录
                await ctx.InsertAsync(person2);
                Console.WriteLine($" - 插入了: {person2.Name}, ID: {person2.Id}");

                // 提交事务
                await ctx.CommitAsync();
                Console.WriteLine(" - 事务已提交");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" - 事务出错: {ex.Message}");
                await ctx.RollbackAsync();
            }
        }

        // 验证是否插入成功
        var count1 = await ctx.Person.Where(p => p.Id == person1.Id || p.Id == person2.Id).CountAsync();
        Console.WriteLine($"[验证] 查询 TransUser1 和 TransUser2，找到 {count1} 条记录 (预期 2 条)");


        // 2. 失败的事务回滚
        Console.WriteLine("\n[事务 2] 开始执行一个将会回滚的事务...");
        
        var person3 = new Person { Name = "TransUser3", Age = 30 };

        using (var transaction = await ctx.BeginTransactionAsync())
        {
            try
            {
                // 插入一条记录
                await ctx.InsertAsync(person3);
                Console.WriteLine($" - 插入了: {person3.Name}, ID: {person3.Id}");

                // 模拟一个错误 (例如插入重复主键，或者业务逻辑错误)
                Console.WriteLine(" - 模拟发生异常...");
                throw new Exception("模拟的业务逻辑错误");

                // 下面的代码不会执行
                // await ctx.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" - 捕获异常: {ex.Message}");
                // 回滚事务
                await ctx.RollbackAsync();
                Console.WriteLine(" - 事务已回滚");
            }
        }

        // 验证是否回滚 (TransUser3 不应该存在)
        // 注意：回滚后，AutoIncrement ID 可能不会回滚，但记录本身不在了
        var person3Exists = await ctx.Person.Where(p => p.Id == person3.Id).CountAsync() > 0;
        Console.WriteLine($"[验证] 查询 TransUser3，是否存在: {person3Exists} (预期 False)");

        Console.WriteLine();
    }
}

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastORM;
using FastORM.SampleApp.Models;

namespace FastORM.SampleApp.Demos;

public static class DynamicExpressionDemo
{
    // 这是一个动态构建 Expression Tree 的示例
    // 注意：手动构建 Expression Tree (如 Run 方法所示) 通常不兼容 Native AOT，因为涉及大量动态反射。
    // 但 FastORM 的核心功能 (如 RunIQueryable 所示) 是 AOT 兼容的，因为它依靠 Source Generator 和轻量级运行时提取。
    public static void Run()
    {
        Console.WriteLine("=== Dynamic Expression Tree Demo ===");

        // 模拟用户输入的搜索条件
        string? searchName = "Alice";
        int? minAge = 18;

        var parameter = Expression.Parameter(typeof(Person), "p");
        Expression body = Expression.Constant(true);

        if (!string.IsNullOrEmpty(searchName))
        {
            var property = Expression.Property(parameter, nameof(Person.Name));
            var value = Expression.Constant(searchName);
            var equality = Expression.Equal(property, value);
            body = Expression.AndAlso(body, equality);
        }

        if (minAge.HasValue)
        {
            var property = Expression.Property(parameter, nameof(Person.Age));
            var value = Expression.Constant(minAge.Value);
            var greaterThanOrEqual = Expression.GreaterThanOrEqual(property, value);
            body = Expression.AndAlso(body, greaterThanOrEqual);
        }

        var lambda = Expression.Lambda<Func<Person, bool>>(body, parameter);
        Console.WriteLine($"Generated Expression: {lambda}");
        // context.Person.Where(lambda); // 不支持
    }

    public static async Task RunIQueryable(MyDbContext ctx)
    {
        Console.WriteLine("=== IQueryable Chaining Demo ===");
        Console.WriteLine("Note: FastORM now supports local variables via Runtime Value Extraction.");

        // 模拟设置参数 (局部变量)
        string localSearchName = "Alice";
        int localMinAge = 10;

        // 演示：链式调用 (Fluent API)
        // 现在可以使用普通 Lambda (非 static)，并捕获局部变量
        Console.WriteLine($"Querying for Name={localSearchName} and Age>{localMinAge}...");
        
        // 演示：动态构建查询 (使用 if 拼接)
        // 注意：FastORM 现已支持混合模式执行！
        // 当 Source Generator 检测到查询链被拆分（如使用 IQueryable 变量）时，
        // 会自动切换到“运行时翻译模式”(Runtime Translation Mode)。
        // 在此模式下，FastORM 会在运行时解析 Expression Tree 并生成 SQL，
        // 从而完美支持如下的动态拼接写法，同时保持 AOT 兼容性（不使用 Expression.Compile）。
        
        IQueryable<Person> query = ctx.Person;

        if (localMinAge > 0)
        {
            query = query.Where(p => p.Age > localMinAge);
        }

        if (localSearchName != null)
        {
            query = query.Where(p => p.Name == localSearchName);
        }

        query = query.OrderBy(p => p.Name);

        var results = await query.ToListAsync();

        Console.WriteLine($"Found {results.Count} people.");
        foreach (var p in results)
        {
            Console.WriteLine($" - {p.Name} ({p.Age})");
        }
    }
}

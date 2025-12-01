using FastORM.SampleApp.Models;

namespace FastORM.SampleApp.Demos;

public static class BulkOperationsDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Bulk Operations Demo ===");

        // This part was just listing all, which is covered in BasicQueryDemo, but included in original DemoAsync
        var all = await ctx.Person.ToListAsync();
        foreach (var p in all)
        {
            Console.WriteLine($"All:{p.Name}:{p.Age}");
        }

        var rows = new[] { new Person { Id = 5, Name = "Eve", Age = 28 }, new Person { Id = 6, Name = "Frank", Age = 19 } };
        var i = await ctx.InsertAsync(rows);
        Console.WriteLine($"BulkInserted:{i}");

        var ids = await ctx.Person.Where(static p => new[] { 5, 6 }.Contains(p.Id)).ToListAsync();
        foreach (var p in ids)
        {
            Console.WriteLine($"Fetched:{p.Name}:{p.Age}");
        }

        var d = await ctx.DeleteAsync(new[] { new Person { Id = 5 }, new Person { Id = 6 } });
        Console.WriteLine($"BulkDeleted:{d}");
        
        Console.WriteLine();
    }
}

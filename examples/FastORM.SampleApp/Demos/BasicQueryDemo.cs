namespace FastORM.SampleApp.Demos;

public static class BasicQueryDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Basic Query Demo ===");
        var results = await ctx.Person
            .Where(static p => p.Age > 18)
            .OrderBy(static p => p.Name)
            .Take(10)
            .ToListAsync();

        foreach (var p in results)
        {
            Console.WriteLine($"{p.Name}:{p.Age}");
        }
        Console.WriteLine();
    }
}

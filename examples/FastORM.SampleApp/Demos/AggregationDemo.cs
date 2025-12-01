namespace FastORM.SampleApp.Demos;

public static class AggregationDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Aggregation Demo ===");
        
        var adults = await ctx.Person.Where(static p => p.Age >= 18).CountAsync();
        Console.WriteLine($"Adults:{adults}");

        var maxAge = await ctx.Person.MaxAsync(static p => p.Age);
        Console.WriteLine($"MaxAge:{maxAge}");

        var total = await ctx.Orders.SumAsync(static o => o.Amount);
        Console.WriteLine($"TotalAmount:{total}");

        var avg = await ctx.Orders.AverageAsync(static o => o.Amount);
        Console.WriteLine($"AverageAmount:{avg}");
        
        Console.WriteLine();
    }
}

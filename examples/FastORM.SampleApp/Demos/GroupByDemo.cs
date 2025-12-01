namespace FastORM.SampleApp.Demos;

public static class GroupByDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Group By Demo ===");
        var groups = await ctx.Person
            .GroupBy(static p => p.Age)
            .Select(static g => new AgeCount { Key = g.Key, Count = g.Count() })
            .OrderByDescending(static x => x.Count)
            .ToListAsync();

        foreach (var g in groups)
        {
            Console.WriteLine($"Age {g.Key}:{g.Count}");
        }
        Console.WriteLine();
    }

    public static async Task RunSumAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Group By Sum Demo ===");
        var totals = await ctx.Person
            .Join(ctx.Orders, static p => p.Id, static o => o.UserId, static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount })
            .GroupBy(static r => r.Name)
            .Select(static g => new UserTotals { Name = g.Key, Total = g.Sum(static r => r.Amount) })
            .OrderByDescending(static x => x.Total)
            .ToListAsync();

        foreach (var t in totals)
        {
            Console.WriteLine($"{t.Name}:{t.Total}");
        }
        Console.WriteLine();
    }
}

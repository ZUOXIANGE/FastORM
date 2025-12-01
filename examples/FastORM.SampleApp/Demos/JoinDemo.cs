namespace FastORM.SampleApp.Demos;

public static class JoinDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== Join Demo ===");
        var joinResults = await ctx.Person
            .Join(ctx.Orders, static p => p.Id, static o => o.UserId, static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount })
            .Where(static r => r.Amount > 15)
            .OrderByDescending(static r => r.Amount)
            .ToListAsync();

        foreach (var r in joinResults)
        {
            Console.WriteLine($"{r.Name}:{r.Amount}");
        }

        Console.WriteLine("=== Left Join Demo ===");
        // Person (Left) -> Orders (Right). All people, even if no order.
        var leftJoinResults = await ctx.Person
            .LeftJoin(ctx.Orders, static p => p.Id, static o => o.UserId, static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount })
            .Where(static r => r.Name.StartsWith("A"))
            .OrderBy(static r => r.Name)
            .ToListAsync();

        foreach (var r in leftJoinResults)
        {
            Console.WriteLine($"{r.Name}:{r.Amount}");
        }

        Console.WriteLine("=== Right Join Demo ===");
        // Person (Left) -> Orders (Right). All orders, even if no person.
        var rightJoinResults = await ctx.Person
            .RightJoin(ctx.Orders, static p => p.Id, static o => o.UserId, static (p, o) => new JoinResult { Name = p.Name, Amount = o.Amount })
            .Where(static r => r.Amount < 50)
            .OrderByDescending(static r => r.Amount)
            .ToListAsync();

        foreach (var r in rightJoinResults)
        {
            Console.WriteLine($"{r.Name}:{r.Amount}");
        }
        Console.WriteLine();
    }
}

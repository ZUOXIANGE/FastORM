using FastORM.SampleApp.Models;
using System.Threading.Tasks;

namespace FastORM.SampleApp;

public static class DatabaseSetup
{
    public static async Task InitializeAsync(MyDbContext ctx)
    {
        // 1. Re-create Tables (Schema Generation)
        await ctx.DropTableAsync<Person>();
        await ctx.DropTableAsync<Order>();
        
        await ctx.CreateTableAsync<Person>();
        await ctx.CreateTableAsync<Order>();

        // 2. Insert Data
        // Person
        var alice = new Person { Name = "Alice", Age = 30 };
        var bob = new Person { Name = "Bob", Age = 17 };
        var carol = new Person { Name = "Carol", Age = 22 };

        // Insert individually to populate Ids (AutoIncrement)
        await ctx.InsertAsync(alice);
        await ctx.InsertAsync(bob);
        await ctx.InsertAsync(carol);

        // Orders
        // Alice has orders
        await ctx.InsertAsync(new Order { UserId = alice.Id, Amount = 12.5m });
        await ctx.InsertAsync(new Order { UserId = alice.Id, Amount = 20.0m });
        
        // Carol has order
        await ctx.InsertAsync(new Order { UserId = carol.Id, Amount = 7.0m });
    }
}

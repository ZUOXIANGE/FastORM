using FastORM.SampleApp.Models;

namespace FastORM.SampleApp.Demos;

public static class CrudDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== CRUD Demo ===");
        
        var added = await ctx.InsertAsync(new Person { Id = 4, Name = "Dave", Age = 25 });
        Console.WriteLine($"Inserted:{added}");

        var upd = await ctx.UpdateAsync(new Person { Id = 4, Name = "Dave", Age = 26 });
        Console.WriteLine($"Updated:{upd}");

        var one = await ctx.Person.Where(static p => p.Id == 4).FirstOrDefaultAsync();
        if (one is not null) Console.WriteLine($"{one.Name}:{one.Age}");

        var del = await ctx.DeleteAsync(new Person { Id = 4 });
        Console.WriteLine($"Deleted:{del}");
        
        // Prepare data for Where Update/Delete
        Console.WriteLine("Preparing data for batch operations...");
        var data = new[]
        {
            new Person { Id = 101, Name = "OldGuy1", Age = 105 },
            new Person { Id = 102, Name = "OldGuy2", Age = 110 },
            new Person { Id = 103, Name = "Unknown", Age = 20 },
            new Person { Id = 104, Name = "Unknown", Age = 30 },
            new Person { Id = 105, Name = "SafeGuy", Age = 50 }
        };
        await ctx.InsertAsync(data);
        Console.WriteLine($"Inserted {data.Length} rows for testing.");

        // Where Update Example
        // Update all people older than 100 to be exactly 100
        var whereUpdate = await ctx.Person
            .Where(static p => p.Age > 100)
            .UpdateAsync(static p => p.Age = 100);
        Console.WriteLine($"Where Update (Age > 100 -> 100): {whereUpdate} rows affected");

        // Where Delete Example
        // Delete all people named "Unknown"
        var whereDelete = await ctx.Person
            .Where(static p => p.Name == "Unknown")
            .DeleteAsync();
        Console.WriteLine($"Where Delete (Name == 'Unknown'): {whereDelete} rows affected");
        
        Console.WriteLine();
    }
}

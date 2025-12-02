using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FastORM.FunctionalTests;

public class DynamicExpressionTests
{
    private void SetupDatabase(SqliteConnection conn)
    {
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES" +
            "(1,'Alice',30),(2,'Bob',17),(3,'Carol',22),(4,'David',40),(5,'Eve',19),(6,'Frank',18);";
        insert.ExecuteNonQuery();
    }

    [Fact]
    public void LocalVariable_Where_ShouldWorkWithRuntimeExtraction()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        SetupDatabase(conn);
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        int minAge = 18;
        // This should be handled by the Source Generator + Runtime Value Extraction
        var list = ctx.Users
            .Where(p => p.Age > minAge)
            .OrderBy(p => p.Name)
            .ToList();

        Assert.Equal(4, list.Count); // Alice(30), Carol(22), David(40), Eve(19)
        Assert.Equal("Alice", list[0].Name);
        Assert.Equal("Carol", list[1].Name);
    }

    [Fact]
    public async Task DynamicIfChaining_ShouldWorkWithRuntimeTranslation()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        SetupDatabase(conn);
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        int minAge = 18;
        string? nameFilter = "a"; // Contains 'a'

        IQueryable<User> query = ctx.Users;

        // Split query chain - triggers Runtime Translation Mode
        if (minAge > 0)
        {
            query = query.Where(p => p.Age > minAge);
        }

        if (!string.IsNullOrEmpty(nameFilter))
        {
            query = query.Where(p => p.Name.Contains(nameFilter));
        }

        query = query.OrderBy(p => p.Name);

        var list = await query.ToListAsync();

        // Expected: Age > 18 AND Name contains 'a'
        // Alice (30, has 'a' - NO, 'A' is typically case sensitive in C# Linq to Objects but SQL might vary. 
        // In Sqlite default is case insensitive for ASCII but LIKE is case insensitive.
        // FastORM uses LIKE for Contains.
        // Alice (Contains 'a' -> false if case sensitive, true if insensitive)
        // Carol (Contains 'a' -> true)
        // David (Contains 'a' -> true)
        // Eve (Contains 'a' -> false)
        // Frank (Contains 'a' -> true, but Age 18 not > 18)
        
        // Wait, FastORM's `Contains` maps to `LIKE '%' + @p + '%'`. 
        // In SQLite `LIKE` is case-insensitive by default.
        // So Alice, Carol, David, Frank match name.
        // Age > 18: Alice(30), Carol(22), David(40), Eve(19).
        // Intersection: Alice, Carol, David.
        
        Assert.Equal(3, list.Count);
        Assert.Contains(list, u => u.Name == "Alice");
        Assert.Contains(list, u => u.Name == "Carol");
        Assert.Contains(list, u => u.Name == "David");
    }

    [Fact]
    public void DynamicIfChaining_ComplexLogic()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        SetupDatabase(conn);
        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);

        IQueryable<User> query = ctx.Users;

        bool filterByAge = true;
        bool filterByName = true;
        
        if (filterByAge)
        {
            // Use a local variable inside the block
            int ageLimit = 25; 
            query = query.Where(p => p.Age < ageLimit);
        }
        
        if (filterByName)
        {
            query = query.Where(p => p.Name.StartsWith("B") || p.Name.EndsWith("e"));
        }

        // Age < 25: Bob(17), Carol(22), Eve(19), Frank(18)
        // Name starts with B or ends with e:
        // Bob (Starts B) -> Match
        // Carol (Ends l) -> No
        // Eve (Ends e) -> Match
        // Frank (Starts F, Ends k) -> No
        
        // Intersection: Bob, Eve.
        
        var list = query.ToList();
        
        Assert.Equal(2, list.Count);
        Assert.Contains(list, u => u.Name == "Bob");
        Assert.Contains(list, u => u.Name == "Eve");
    }
}

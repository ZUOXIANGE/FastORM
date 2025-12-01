using FastORM.FunctionalTests.Contexts;
using FastORM.FunctionalTests.Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace FastORM.FunctionalTests;

public class ComplexQueryTests
{
    public static readonly List<int> TargetIds = new List<int> { 1, 2, 3, 4, 6, 9 };
    public static readonly List<int> ExcludeIds = new List<int> { 5, 10 };

    [Fact]
    public async Task ComplexQuery_MixedConditions_ShouldReturnCorrectResults()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);
                INSERT INTO Users (Id, Name, Age) VALUES (1, 'Alice User', 20);
                INSERT INTO Users (Id, Name, Age) VALUES (2, 'Bob User', 30);
                INSERT INTO Users (Id, Name, Age) VALUES (3, 'Charlie User', 40);
                INSERT INTO Users (Id, Name, Age) VALUES (4, 'David User', 25);
                INSERT INTO Users (Id, Name, Age) VALUES (5, 'Eve Admin', 30);
                INSERT INTO Users (Id, Name, Age) VALUES (6, 'Frank User', 35);
                INSERT INTO Users (Id, Name, Age) VALUES (7, 'Grace', 28);
                INSERT INTO Users (Id, Name, Age) VALUES (8, 'Hank', 50);
                INSERT INTO Users (Id, Name, Age) VALUES (9, 'Ian User', 22);
                INSERT INTO Users (Id, Name, Age) VALUES (10, 'Jack User', 32);
            ";
            await cmd.ExecuteNonQueryAsync();

            var context = new FunctionalTestDbContext(connection, SqlDialect.Sqlite);

            // Scenario: 
            // (Name StartsWith "A" OR Name EndsWith "User") 
            // AND (Age >= 25 AND Age <= 40)
            // AND Id NOT IN [5, 10]

            var results = await context.Users
                .Where(static u => u.Name.StartsWith("A") || u.Name.EndsWith("User"))
                .Where(static u => u.Age >= 25 && u.Age <= 40 || (u.Name == "Hank" && u.Id > 5))
                .Where(static u => !global::FastORM.FunctionalTests.ComplexQueryTests.ExcludeIds.Contains(u.Id))
                .OrderBy(static u => u.Id)
                .ToListAsync();

            // Expected matches logic:
            // 1: Alice User (20) -> Name(T), Age(F) -> F
            // 2: Bob User (30) -> Name(T), Age(T), !Ex(T) -> T
            // 3: Charlie User (40) -> Name(T), Age(T), !Ex(T) -> T
            // 4: David User (25) -> Name(T), Age(T), !Ex(T) -> T
            // 5: Eve Admin (30) -> Name(F) -> F
            // 6: Frank User (35) -> Name(T), Age(T), !Ex(T) -> T
            // 7: Grace (28) -> Name(F) -> F
            // 8: Hank (50) -> Name(F) -> F
            // 9: Ian User (22) -> Name(T), Age(F) -> F
            // 10: Jack User (32) -> Name(T), Age(T), !Ex(F) -> F

            // Total: 2, 3, 4, 6 (4 items)

            Assert.Equal(4, results.Count);
            Assert.Equal(2, results[0].Id);
            Assert.Equal(3, results[1].Id);
            Assert.Equal(4, results[2].Id);
            Assert.Equal(6, results[3].Id);
        }
        finally
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
    }
}

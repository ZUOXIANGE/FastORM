using Microsoft.Data.Sqlite;
using Xunit;
using FastORM.FunctionalTests.Entities;
using FastORM.FunctionalTests.Contexts;

namespace FastORM.FunctionalTests;

public class AsyncQueryFunctionalTests
{
    [Fact]
    public async Task ToListAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',25);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users.ToListAsync();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, p => p.Name == "Alice");
        Assert.Contains(list, p => p.Name == "Bob");
    }

    [Fact]
    public async Task FirstOrDefaultAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',25);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var p = await ctx.Users.Where(static p => p.Name == "Bob").FirstOrDefaultAsync();

        Assert.NotNull(p);
        Assert.Equal("Bob", p!.Name);

        var none = await ctx.Users.Where(static p => p.Name == "Charlie").FirstOrDefaultAsync();
        Assert.Null(none);
    }

    [Fact]
    public async Task AnyAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var exists = await ctx.Users.Where(static p => p.Name == "Alice").AnyAsync();
        Assert.True(exists);

        var notExists = await ctx.Users.Where(static p => p.Name == "Bob").AnyAsync();
        Assert.False(notExists);
    }

    [Fact]
    public async Task CountAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        cmd.ExecuteNonQuery();
        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO Users(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',25),(3,'Carol',20);";
        insert.ExecuteNonQuery();

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var count = await ctx.Users.Where(static p => p.Age > 22).CountAsync();
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task JoinAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        // Setup tables
        using (var c = conn.CreateCommand()) { c.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "INSERT INTO Users VALUES(1,'A'),(2,'B');"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "INSERT INTO Orders VALUES(10,1,100),(11,1,50),(12,2,20);"; c.ExecuteNonQuery(); }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users
            .Join(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new JoinResult { Name = u.Name, Amount = o.Amount })
            .ToListAsync();

        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task LeftJoinAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        // Setup tables
        using (var c = conn.CreateCommand()) { c.CommandText = "CREATE TABLE Users(Id INTEGER PRIMARY KEY, Name TEXT);"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "INSERT INTO Users VALUES(1,'A'),(2,'B');"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "INSERT INTO Orders VALUES(10,1,100);"; c.ExecuteNonQuery(); }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Users
            .LeftJoin(ctx.Orders, static u => u.Id, static o => o.UserId, static (u, o) => new UserAmount { Name = u.Name, Amount = o.Amount })
            .ToListAsync();

        Assert.Equal(2, list.Count);
        // A has order
        Assert.Contains(list, x => x.Name == "A" && x.Amount == 100);
        // B has no order => Amount=0 (default)
        Assert.Contains(list, x => x.Name == "B" && x.Amount == 0);
    }

    [Fact]
    public async Task GroupByAsync_Works()
    {
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using (var c = conn.CreateCommand()) { c.CommandText = "CREATE TABLE Items(Id INTEGER PRIMARY KEY, CategoryId INTEGER);"; c.ExecuteNonQuery(); }
        using (var c = conn.CreateCommand()) { c.CommandText = "INSERT INTO Items VALUES(1,1),(2,1),(3,2);"; c.ExecuteNonQuery(); }

        var ctx = new FunctionalTestDbContext(conn, SqlDialect.Sqlite);
        var list = await ctx.Items
            .GroupBy(static x => x.CategoryId)
            .Select(static g => new GroupResult { Key = g.Key, Count = g.Count() })
            .ToListAsync();

        Assert.Equal(2, list.Count);
        Assert.Contains(list, x => x.Key == 1 && x.Count == 2);
        Assert.Contains(list, x => x.Key == 2 && x.Count == 1);
    }
}




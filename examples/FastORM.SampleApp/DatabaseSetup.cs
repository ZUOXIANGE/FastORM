using Microsoft.Data.Sqlite;

namespace FastORM.SampleApp;

public static class DatabaseSetup
{
    public static async Task InitializeAsync(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE People(Id INTEGER PRIMARY KEY, Name TEXT, Age INTEGER);";
        await cmd.ExecuteNonQueryAsync();

        using var insert = conn.CreateCommand();
        insert.CommandText = "INSERT INTO People(Id,Name,Age) VALUES(1,'Alice',30),(2,'Bob',17),(3,'Carol',22);";
        await insert.ExecuteNonQueryAsync();

        using var cmd2 = conn.CreateCommand();
        cmd2.CommandText = "CREATE TABLE Orders(Id INTEGER PRIMARY KEY, UserId INTEGER, Amount REAL);";
        await cmd2.ExecuteNonQueryAsync();

        using var ins2 = conn.CreateCommand();
        ins2.CommandText = "INSERT INTO Orders(Id,UserId,Amount) VALUES(100,1,12.5),(101,1,20.0),(102,3,7.0);";
        await ins2.ExecuteNonQueryAsync();
    }
}

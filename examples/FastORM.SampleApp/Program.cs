using FastORM;
using FastORM.SampleApp;
using FastORM.SampleApp.Demos;
using Microsoft.Data.Sqlite;

await using var conn = new SqliteConnection("Data Source=:memory:");
await conn.OpenAsync();

await DatabaseSetup.InitializeAsync(conn);

var ctx = new MyDbContext(conn, SqlDialect.Sqlite)
{
    SqlLogger = static sql => Console.WriteLine($"SQL: {sql}")
};

await CrudDemo.RunAsync(ctx);
await BasicQueryDemo.RunAsync(ctx);
await JoinDemo.RunAsync(ctx);
await GroupByDemo.RunAsync(ctx);
await GroupByDemo.RunSumAsync(ctx);
await AggregationDemo.RunAsync(ctx);
await BulkOperationsDemo.RunAsync(ctx);
await DynamicExpressionDemo.RunIQueryable(ctx);


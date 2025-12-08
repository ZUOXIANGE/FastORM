using FastORM;
using FastORM.SampleApp;
using FastORM.SampleApp.Demos.Basics;
using FastORM.SampleApp.Demos.AdvancedQuerying;
using FastORM.SampleApp.Demos.BatchOperations;
using FastORM.SampleApp.Demos.DynamicQuerying;
using Microsoft.Data.Sqlite;
using System;

// 设置数据库连接
// 使用 SQLite 内存数据库进行演示
await using var conn = new SqliteConnection("Data Source=:memory:");
await conn.OpenAsync();

// 创建数据库上下文
// 配置 SQL 日志输出到控制台，方便观察生成的 SQL
var ctx = new MyDbContext(conn, SqlDialect.Sqlite)
{
    SqlLogger = static sql => Console.WriteLine($"[SQL]: {sql}")
};

// 初始化数据库表结构
await DatabaseSetup.InitializeAsync(ctx);

Console.WriteLine("FastORM 示例程序启动...\n");

// --- 1. 基础功能 ---
await CrudDemo.RunAsync(ctx);
await SimpleQueryDemo.RunAsync(ctx);
await TransactionDemo.RunAsync(ctx);

// --- 2. 高级查询 ---
await JoinDemo.RunAsync(ctx);
await AggregationDemo.RunAsync(ctx);
await GroupByDemo.RunAsync(ctx);

// --- 3. 批量操作 ---
await BulkDemo.RunAsync(ctx);
await BatchUpdateDeleteDemo.RunAsync(ctx);

// --- 4. 动态查询 ---
await DynamicQueryDemo.RunAsync(ctx);

Console.WriteLine("所有演示完成。");

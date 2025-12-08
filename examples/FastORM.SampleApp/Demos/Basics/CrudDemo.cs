using FastORM;
using FastORM.SampleApp.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace FastORM.SampleApp.Demos.Basics;

/// <summary>
/// 基础 CRUD 演示
/// 展示如何进行基本的增删改查操作
/// </summary>
public static class CrudDemo
{
    public static async Task RunAsync(MyDbContext ctx)
    {
        Console.WriteLine("=== 基础 CRUD 演示 (Basic CRUD) ===");

        // 1. 新增 (Create)
        // 创建一个新的 Person 对象
        var newPerson = new Person 
        { 
            // Id = 1001, // ID 由数据库自动生成
            Name = "Alice", 
            Age = 25 
        };

        // 调用 InsertAsync 将对象插入数据库
        // 返回值通常是受影响的行数，且 newPerson.Id 会被自动填充
        var insertedCount = await ctx.InsertAsync(newPerson);
        Console.WriteLine($"[新增] 插入了 {insertedCount} 条记录: {newPerson.Name}, Age: {newPerson.Age}, ID: {newPerson.Id}");

        // 2. 查询 (Read)
        // 使用 Where 过滤条件，并使用 FirstOrDefaultAsync 获取单条记录
        // 注意：FastORM 支持 Lambda 表达式解析
        var person = await ctx.Person
            .Where(p => p.Id == newPerson.Id)
            .FirstOrDefaultAsync();

        if (person != null)
        {
            Console.WriteLine($"[查询] 找到了记录: ID={person.Id}, Name={person.Name}, Age={person.Age}");

            // 3. 更新 (Update)
            // 修改对象的属性
            person.Age = 26; // Alice 长了一岁
            
            // 调用 UpdateAsync 更新数据库中的记录
            // FastORM 会根据主键 (Id) 更新其他字段
            var updatedCount = await ctx.UpdateAsync(person);
            Console.WriteLine($"[更新] 更新了 {updatedCount} 条记录. 新年龄: {person.Age}");
        }

        // 4. 删除 (Delete)
        // 创建一个只包含主键的对象即可删除，或者使用刚刚查询出来的对象
        // var personToDelete = new Person { Id = 1001 };
        
        // 调用 DeleteAsync 从数据库中删除记录
        var deletedCount = await ctx.DeleteAsync(person);
        Console.WriteLine($"[删除] 删除了 {deletedCount} 条记录 (ID={person?.Id})");

        Console.WriteLine();
    }
}

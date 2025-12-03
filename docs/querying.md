# 查询功能指南

FastORM 支持标准的 LINQ 语法进行查询。查询会被编译为高效的 SQL 语句。

## 基本查询

### 过滤 (Where)

```csharp
var adults = await context.Users
    .Where(u => u.Age >= 18)
    .ToListAsync();
```

### 排序 (OrderBy / OrderByDescending)

```csharp
var sortedUsers = await context.Users
    .OrderBy(u => u.Age)
    .ThenByDescending(u => u.Name) // 支持 ThenBy
    .ToListAsync();
```

### 分页 (Skip / Take)

```csharp
var page = await context.Users
    .Skip(10)
    .Take(20)
    .ToListAsync();
```

### 投影 (Select)

只查询需要的列，提高性能。

```csharp
var names = await context.Users
    .Select(u => u.Name)
    .ToListAsync();

var dtos = await context.Users
    .Select(u => new UserDto { UserName = u.Name, UserAge = u.Age })
    .ToListAsync();
```

## 获取单个元素

```csharp
// 获取第一个匹配项，没有则返回 null
var user = await context.Users.FirstOrDefaultAsync(u => u.Id == 1);

// 获取第一个匹配项 (同步)
var userSync = context.Users.FirstOrDefault(u => u.Id == 1);
```

## 动态查询 (Dynamic Querying)

FastORM 支持使用运行时变量作为查询条件。编译器会智能地将这些变量转换为 SQL 参数。

```csharp
int minAge = 18;
string prefix = "A";

// 运行时变量会自动参数化
var query = await context.Users
    .Where(u => u.Age >= minAge && u.Name.StartsWith(prefix))
    .ToListAsync();
```

## 聚合查询

支持 `Count`, `Sum`, `Min`, `Max`, `Average`。

```csharp
int count = await context.Users.CountAsync();

int adultCount = await context.Users.CountAsync(u => u.Age >= 18);

int maxAge = await context.Users.MaxAsync(u => u.Age);
```

## 高级查询

### 连接 (Join / LeftJoin / RightJoin)

```csharp
// Inner Join
var query = context.Users.Join(
    context.Orders,
    u => u.Id,        // Outer Key
    o => o.UserId,    // Inner Key
    (u, o) => new { u.Name, o.Amount } // Result Selector
);

// Left Join
var queryLeft = context.Users.LeftJoin(
    context.Orders,
    u => u.Id,
    o => o.UserId,
    (u, o) => new { u.Name, o.Amount }
);
```

### 分组 (GroupBy)

```csharp
var group = await context.Users
    .GroupBy(u => u.Age)
    .Select(g => new { Age = g.Key, Count = g.Count() })
    .ToListAsync();
```

### In / Not In

FastORM 智能识别 `Contains` 方法并转换为 SQL 的 `IN` 子句。支持数组、列表以及运行时集合。

```csharp
// 静态数组
var ids = new[] { 1, 2, 3 };
var users = await context.Users
    .Where(u => ids.Contains(u.Id))
    .ToListAsync();

// 运行时列表
List<string> names = GetNamesFromRequest();
var targetUsers = await context.Users
    .Where(u => !names.Contains(u.Name)) // 转换为 NOT IN
    .ToListAsync();
```

### Exists / Not Exists

支持使用 `Any` 和 `All` 来生成 `EXISTS` 和 `NOT EXISTS` 子句，常用于复杂的子查询逻辑。

```csharp
// Exists
bool hasAdults = await context.Users.AnyAsync(u => u.Age >= 18);

// Not Exists (All)
// 检查是否所有用户都大于 0 岁 (即不存在 <= 0 岁的用户)
bool allValid = await context.Users.AllAsync(u => u.Age > 0);
```

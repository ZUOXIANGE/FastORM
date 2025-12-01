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

FastORM 智能识别 `Contains` 方法并转换为 SQL 的 `IN` 子句。

```csharp
var ids = new[] { 1, 2, 3 };
var users = await context.Users
    .Where(u => ids.Contains(u.Id))
    .ToListAsync();
```

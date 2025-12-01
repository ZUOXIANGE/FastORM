namespace FastORM.IntegrationTests.Entities;

public sealed class JoinResult
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class GroupResult
{
    public int Key { get; set; }
    public int Count { get; set; }
}

public sealed class UserAmount
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

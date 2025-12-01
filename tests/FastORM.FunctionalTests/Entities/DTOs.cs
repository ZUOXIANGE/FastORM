namespace FastORM.FunctionalTests.Entities;

public sealed class GroupResult
{
    public int Key { get; set; }
    public int Count { get; set; }
}

public sealed class AggResult
{
    public int Key { get; set; }
    public int Sum { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
    public double Avg { get; set; }
}

public sealed class JoinResult
{
    public string? Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class UserAmount
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class UserTotals
{
    public string Name { get; set; } = "";
    public decimal Total { get; set; }
}

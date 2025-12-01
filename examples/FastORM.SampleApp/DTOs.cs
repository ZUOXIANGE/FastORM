namespace FastORM.SampleApp;

public sealed class JoinResult
{
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class AgeCount
{
    public int Key { get; set; }
    public int Count { get; set; }
}

public sealed class UserTotals
{
    public string Name { get; set; } = "";
    public decimal Total { get; set; }
}

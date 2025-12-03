using System.ComponentModel.DataAnnotations.Schema;

namespace FastORM.FunctionalTests.Entities;

public enum TestEnum
{
    First = 1,
    Second = 2,
    Third = 3
}

[Table("supported_types")]
public sealed class SupportedTypes
{
    public int Id { get; set; }
    public string StringProp { get; set; } = "";
    public int IntProp { get; set; }
    public long LongProp { get; set; }
    public decimal DecimalProp { get; set; }
    public double DoubleProp { get; set; }
    public bool BoolProp { get; set; }
    public DateTime DateTimeProp { get; set; }
    public Guid GuidProp { get; set; }
    public DateOnly DateOnlyProp { get; set; }
    public DateTimeOffset? DateTimeOffsetProp { get; set; }
    public TestEnum EnumProp { get; set; }
}

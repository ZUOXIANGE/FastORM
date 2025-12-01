using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal sealed class AggregationModel
{
    public AggregationKind Kind { get; set; }
    public IPropertySymbol? Property { get; set; }
    public PredicateModel? FilterPredicate { get; set; }
    public bool NegateFilter { get; set; }
}

internal enum AggregationKind
{
    Count,
    Max,
    Min,
    Average,
    Sum,
    Exists,
    NotExists
}

internal enum AggregationReceiverKind
{
    Query,
    Group
}

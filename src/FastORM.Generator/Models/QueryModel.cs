using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal sealed class QueryModel
{
    public readonly List<IncludeModel> Includes = new();
    public readonly List<IPropertySymbol> InsertProperties = new();
    public readonly List<(IPropertySymbol prop, bool desc)> OrderBy = new();
    public readonly List<PredicateModel> Predicates = new();
    public readonly List<(string Column, string ValueExpressionCode)> Updates = new();
    public ITypeSymbol ElementType { get; set; } = null!;
    public string TableName { get; set; } = "";
    public int? TakeCount { get; set; }
    public int? SkipCount { get; set; }
    public bool IsDistinct { get; set; }
    public AggregationModel? Aggregation { get; set; }
    public AggregationReceiverKind AggregationReceiver { get; set; }
    public ProjectionModel? Projection { get; set; }
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }
    public JoinModel? Join { get; set; }
    public GroupByModel? GroupBy { get; set; }
    public bool IsAsync { get; set; }
    public bool IsDelete { get; set; }
    public bool IsUpdate { get; set; }
    public bool UpdateIsEntity { get; set; }
    public bool UpdateIsBatch { get; set; }
    public ITypeSymbol? UpdateParameterType { get; set; }
    public bool IsInsert { get; set; }
    public bool InsertIsBatch { get; set; }
    public ITypeSymbol? InsertParameterType { get; set; }
    public bool DeleteIsEntity { get; set; }
    public bool DeleteIsBatch { get; set; }
    public ITypeSymbol? DeleteParameterType { get; set; }
    public bool IsFirstOrDefault { get; set; }
    public int InterceptVersion { get; set; }
    public string InterceptData { get; set; } = "";
    public ProjectionModel? PreGroupElementProjection { get; set; }
    public bool EndOnIQueryable { get; set; }
    public bool HasError { get; set; }
    public DiagnosticDescriptor? ErrorDescriptor { get; set; }
    public string[]? ErrorArgs { get; set; }
}

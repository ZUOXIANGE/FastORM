using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FastORM.Generator;

internal sealed class ProjectionModel
{
    public readonly List<ProjectionEntry> Entries = new();
    public string TypeName { get; set; } = "";
    public bool IsAnonymous { get; set; }
}

internal sealed class ProjectionEntry
{
    public ProjectionEntryKind Kind { get; set; }
    public IPropertySymbol? Property { get; set; }
    public int Source { get; set; }
    public string? Alias { get; set; }
    public string? Aggregator { get; set; }
    public IPropertySymbol? AggregatorProperty { get; set; }
    public ITypeSymbol? Type { get; set; }
}

internal enum ProjectionEntryKind
{
    Property,
    GroupKey,
    Aggregator
}

using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal sealed class JoinModel
{
    public ITypeSymbol InnerType { get; set; } = null!;
    public string InnerTable { get; set; } = "";
    public IPropertySymbol? OuterKey { get; set; }
    public IPropertySymbol? InnerKey { get; set; }
    public JoinKind Kind { get; set; }
}

internal enum JoinKind
{
    Inner,
    Left,
    Right
}

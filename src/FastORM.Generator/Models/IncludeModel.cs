using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal sealed class IncludeModel
{
    public IPropertySymbol NavigationProperty { get; set; } = null!;
    public ITypeSymbol InnerType { get; set; } = null!;
    public string InnerTable { get; set; } = "";
    public IPropertySymbol OuterKey { get; set; } = null!;
    public IPropertySymbol InnerKey { get; set; } = null!;
}

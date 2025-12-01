using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FastORM.Generator;

internal sealed class GroupByModel
{
    public readonly List<IPropertySymbol> Keys = new();
}

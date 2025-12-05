using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace FastORM.Generator;

internal static class MetadataHelper
{
    public static string GetTableName(ITypeSymbol type)
    {
        foreach (var a in type.GetAttributes())
        {
            if (a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")
            {
                if (a.ConstructorArguments.Length > 0 && a.ConstructorArguments[0].Value is string s)
                {
                    return s;
                }
            }
        }
        return type.Name;
    }

    public static IPropertySymbol? GetPrimaryKey(ITypeSymbol type)
    {
        var props = type.GetMembers().OfType<IPropertySymbol>().ToList();
        // 1. [Key] attribute
        var keyProp = props.FirstOrDefault(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "KeyAttribute" || a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.KeyAttribute"));
        if (keyProp != null) return keyProp;

        // 2. "Id" or "ID"
        keyProp = props.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
        if (keyProp != null) return keyProp;

        // 3. TypeName + "Id"
        keyProp = props.FirstOrDefault(p => p.Name.Equals(type.Name + "Id", StringComparison.OrdinalIgnoreCase));
        return keyProp;
    }

    public static bool IsScalar(ITypeSymbol t)
    {
        if (t.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
        {
            if (t is INamedTypeSymbol nts && nts.TypeArguments.Length > 0)
                return IsScalar(nts.TypeArguments[0]);
        }

        if (t.TypeKind == TypeKind.Enum) return true;
        if (t.TypeKind == TypeKind.Array && t is IArrayTypeSymbol ats)
        {
             if (ats.ElementType.SpecialType == SpecialType.System_Byte) return true;
        }

        switch (t.SpecialType)
        {
            case SpecialType.System_Int16:
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
            case SpecialType.System_Byte:
            case SpecialType.System_String:
            case SpecialType.System_Boolean:
            case SpecialType.System_DateTime:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Single:
                return true;
            default:
                {
                    var name = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (name == "global::System.Guid" || name == "global::System.Byte[]" || name == "global::System.DateOnly" || name == "global::System.TimeOnly" || name == "global::System.DateTimeOffset") return true;
                    return false;
                }
        }
    }
}

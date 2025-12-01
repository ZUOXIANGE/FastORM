using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal static class MetadataEmitter
{
    public static string EmitProvider(List<INamedTypeSymbol> types)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append("#nullable enable\n");
        sb.Append("namespace FastORM.Generated;\n");
        sb.Append("internal sealed class GeneratedMetadataProvider : FastORM.IEntityMetadataProvider\n");
        sb.Append("{\n");
        sb.Append("    public FastORM.IEntityMetadata<T> Get<T>()\n");
        sb.Append("    {\n");
        foreach (var t in types)
        {
            sb.Append("        if (typeof(T) == typeof(").Append(t.ToDisplayString()).Append(") ) return (FastORM.IEntityMetadata<T>)(object)").Append(TypeMetaClassNameFull(t)).Append(".Instance;\n");
        }
        sb.Append("        throw new System.NotSupportedException(\"No metadata for type \" + typeof(T).FullName);\n");
        sb.Append("    }\n");
        sb.Append("}\n");
        sb.Append("static class GeneratedMetadataInit\n");
        sb.Append("{\n");
        sb.Append("    [global::System.Runtime.CompilerServices.ModuleInitializer]\n");
        sb.Append("    internal static void Init() { FastORM.EntityMetadataRegistry.SetProvider(new GeneratedMetadataProvider()); }\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    public static string EmitTypeMeta(INamedTypeSymbol t)
    {
        var tableName = t.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")?.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? t.Name;
        var props = t.GetMembers().OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && p.GetMethod != null && IsScalar(p.Type) && p.GetAttributes().All(a => a.AttributeClass?.ToDisplayString() != "FastORM.NavigationAttribute"))
            .ToList();
        var fields = t.GetMembers().OfType<IFieldSymbol>()
            .Where(f => !f.IsStatic && IsScalar(f.Type) && f.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute"))
            .ToList();

        var cols = new List<(string colName, bool isProperty, ISymbol member, ITypeSymbol type)>();
        foreach (var p in props)
        {
            var cnAttr = p.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute");
            var colName = cnAttr is null ? p.Name : (cnAttr.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? p.Name);
            cols.Add((colName, true, p, p.Type));
        }
        foreach (var f in fields)
        {
            var cnAttr = f.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute");
            var colName = cnAttr is null ? f.Name : (cnAttr!.ConstructorArguments.FirstOrDefault().Value?.ToString() ?? f.Name);
            cols.Add((colName, false, f, f.Type));
        }
        var keyIndex = cols.FindIndex(static x => HasKeyAttribute(x.member));
        if (keyIndex < 0) keyIndex = cols.FindIndex(x => x.member.Name == "Id");
        if (keyIndex < 0) keyIndex = 0;

        var sb = new System.Text.StringBuilder();
        sb.Append("#nullable enable\n");
        sb.Append("namespace FastORM.Generated;\n");
        sb.Append("internal sealed class ").Append(TypeMetaClassName(t)).Append(" : FastORM.IEntityMetadata<").Append(t.ToDisplayString()).Append(">\n");
        sb.Append("{\n");
        sb.Append("    public static readonly ").Append(TypeMetaClassName(t)).Append(" Instance = new ").Append(TypeMetaClassName(t)).Append("();\n");
        sb.Append("    public string TableName => \"").Append(tableName).Append("\";\n");
        sb.Append("    public string[] Columns => new string[] { ");
        for (int i = 0; i < cols.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append("\"").Append(cols[i].colName).Append("\"");
        }
        sb.Append(" };\n");
        sb.Append("    public string[] PropertyNames => new string[] { ");
        for (int i = 0; i < cols.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append("\"").Append(cols[i].member.Name).Append("\"");
        }
        sb.Append(" };\n");
        sb.Append("    public string? GetColumnName(string propertyName)\n");
        sb.Append("    {\n");
        sb.Append("        switch(propertyName)\n");
        sb.Append("        {\n");
        for (int i = 0; i < cols.Count; i++)
        {
            sb.Append("            case \"").Append(cols[i].member.Name).Append("\": return \"").Append(cols[i].colName).Append("\";\n");
        }
        sb.Append("            default: return null;\n");
        sb.Append("        }\n");
        sb.Append("    }\n");
        sb.Append("    public int KeyColumnIndex => ").Append(keyIndex.ToString()).Append(";\n");
        sb.Append("    public object? GetValue(").Append(t.ToDisplayString()).Append(" obj, int columnIndex)\n");
        sb.Append("    {\n");
        sb.Append("        switch(columnIndex)\n");
        sb.Append("        {\n");
        for (int i = 0; i < cols.Count; i++)
        {
            sb.Append("            case ").Append(i.ToString()).Append(": return obj.").Append(cols[i].member.Name).Append(";\n");
        }
        sb.Append("            default: return null;\n");
        sb.Append("        }\n");
        sb.Append("    }\n");

        sb.Append("    public ").Append(t.ToDisplayString()).Append(" ReadRow(System.Data.Common.DbDataReader r)\n");
        sb.Append("    {\n");
        sb.Append("        var o = new ").Append(t.ToDisplayString()).Append("();\n");
        for (int i = 0; i < cols.Count; i++)
        {
            var tname = cols[i].type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var getter = tname switch
            {
                "global::System.Int32" => "GetInt32",
                "global::System.Int64" => "GetInt64",
                "global::System.String" => "GetString",
                "global::System.Boolean" => "GetBoolean",
                "global::System.DateTime" => "GetDateTime",
                "global::System.Decimal" => "GetDecimal",
                "global::System.Double" => "GetDouble",
                "global::System.Single" => "GetFloat",
                "global::System.Guid" => "GetGuid",
                _ => null
            };
            sb.Append("        { int ord; try { ord = r.GetOrdinal(\"").Append(cols[i].colName).Append("\"); } catch { ord = -1; } if (ord >= 0 && !r.IsDBNull(ord)) { ");
            if (getter is not null)
            {
                sb.Append("o.");
                if (cols[i].isProperty) sb.Append(cols[i].member.Name).Append(" = r.").Append(getter).Append("(ord);");
                else sb.Append(cols[i].member.Name).Append(" = r.").Append(getter).Append("(ord);");
            }
            else
            {
                sb.Append("o.");
                if (cols[i].isProperty) sb.Append(cols[i].member.Name).Append(" = r.GetFieldValue<").Append(cols[i].type.ToDisplayString()).Append(">(ord);");
                else sb.Append(cols[i].member.Name).Append(" = r.GetFieldValue<").Append(cols[i].type.ToDisplayString()).Append(">(ord);");
            }
            sb.Append(" } }\n");
        }
        sb.Append("        return o;\n");
        sb.Append("    }\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    public static string Sanitize(string s)
    {
        var chars = s.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            var c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_')) chars[i] = '_';
        }
        return new string(chars);
    }

    static string TypeMetaClassName(INamedTypeSymbol t)
    {
        return Sanitize(t.ToDisplayString()) + "_Metadata";
    }

    static string TypeMetaClassNameFull(INamedTypeSymbol t)
    {
        return "FastORM.Generated." + TypeMetaClassName(t);
    }

    static bool IsScalar(ITypeSymbol t)
    {
        if (t.TypeKind == TypeKind.Enum) return true;
        switch (t.SpecialType)
        {
            case SpecialType.System_Int32:
            case SpecialType.System_Int64:
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
                    if (name == "global::System.Guid" || name == "global::System.Byte[]" || name == "global::System.DateOnly" || name == "global::System.TimeOnly") return true;
                    return false;
                }
        }
    }

    static bool HasKeyAttribute(ISymbol member)
    {
        foreach (var a in member.GetAttributes())
        {
            var name = a.AttributeClass?.ToDisplayString();
            if (name == "System.ComponentModel.DataAnnotations.KeyAttribute") return true;
        }
        return false;
    }
}

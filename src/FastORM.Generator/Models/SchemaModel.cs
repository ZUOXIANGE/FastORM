using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FastORM.Generator;

internal sealed class SchemaModel
{
    public bool IsCreateTable { get; set; }
    public bool IsDropTable { get; set; }
    public string TableName { get; set; } = "";
    public ITypeSymbol ElementType { get; set; } = null!;
    public List<IPropertySymbol> Columns { get; } = new();
    public IPropertySymbol? PrimaryKey { get; set; }
    public bool IsAsync { get; set; }
    
    // Interception info
    public int InterceptVersion { get; set; }
    public string InterceptData { get; set; } = "";

    // Location info
    public string FilePath { get; set; } = "";
    public int Line { get; set; }
    public int Column { get; set; }

    // Diagnostics
    public bool HasError { get; set; }
    public DiagnosticDescriptor? ErrorDescriptor { get; set; }
    public string[]? ErrorArgs { get; set; }
    public List<IndexModel> Indexes { get; } = new();
    public Dictionary<string, ColumnDefinition> ColumnDefinitions { get; } = new();
}

internal sealed class ColumnDefinition
{
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValueSql { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public string? CustomTypeName { get; set; }
}

internal sealed class IndexModel
{
    public string[] Columns { get; set; } = Array.Empty<string>();
    public string? Name { get; set; }
    public bool IsUnique { get; set; }
}

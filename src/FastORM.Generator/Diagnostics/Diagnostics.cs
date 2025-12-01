using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor NonStaticLambda = new(
        id: "FST001",
        title: "Lambda must be static",
        messageFormat: "Only static lambdas are supported for compile-time query analysis",
        category: "FastORM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedExpression = new(
        id: "FST002",
        title: "Unsupported expression",
        messageFormat: "Expression is not supported: {0}",
        category: "FastORM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingTableAttribute = new(
        id: "FST003",
        title: "Missing Table attribute",
        messageFormat: "Type {0} must have [FastORM.Table] attribute",
        category: "FastORM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NavigationMissingAttribute = new(
        id: "FST004",
        title: "Missing Navigation attribute",
        messageFormat: "Property {0} must have [FastORM.Navigation] to be used in Include/ThenInclude",
        category: "FastORM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor PotentialFullScan = new(
        id: "FST100",
        title: "Potential full table scan",
        messageFormat: "Query has no predicate and no limit; may cause full scan",
        category: "FastORM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GeneratorFailure = new(
        id: "FST999",
        title: "Generator failure",
        messageFormat: "FastORM generator failed: {0}",
        category: "FastORM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

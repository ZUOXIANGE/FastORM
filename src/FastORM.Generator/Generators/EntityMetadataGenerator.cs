using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

[Generator]
public sealed class EntityMetadataGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var tableTypes = context.SyntaxProvider.ForAttributeWithMetadataName(
            "System.ComponentModel.DataAnnotations.Schema.TableAttribute",
            static (syntaxNode, _) => true,
            static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
            .Where(static t => t is not null);

        var collected = tableTypes.Collect();
        context.RegisterSourceOutput(collected, static (spc, list) =>
        {
            var unique = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);
            foreach (var t in list)
            {
                if (!unique.ContainsKey(t.ToDisplayString())) unique[t.ToDisplayString()] = t;
            }
            var types = unique.Values.ToList();
            var provider = MetadataEmitter.EmitProvider(types);
            spc.AddSource("FastORM_MetadataProvider.g.cs", provider);
            foreach (var t in types)
            {
                var src = MetadataEmitter.EmitTypeMeta(t);
                var hint = "FastORM_" + MetadataEmitter.Sanitize(t.ToDisplayString()) + "_Meta.g.cs";
                spc.AddSource(hint, src);
            }
        });
    }
}

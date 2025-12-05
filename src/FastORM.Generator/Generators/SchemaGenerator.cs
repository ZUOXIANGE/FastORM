using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using FastORM.Generated;

namespace FastORM.Generator;

[Generator]
public sealed class SchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodCalls = context.SyntaxProvider.CreateSyntaxProvider(IsCandidateNode, SchemaParser.Transform)
            .Where(static m => m is not null);

        var compilationAndCalls = methodCalls.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(compilationAndCalls, Execute);
    }

    private bool IsCandidateNode(SyntaxNode node, CancellationToken _)
    {
        if (node is InvocationExpressionSyntax inv)
        {
            if (inv.Expression is MemberAccessExpressionSyntax ma)
            {
                var name = ma.Name.Identifier.Text;
                return name == "CreateTable" || name == "CreateTableAsync" || 
                       name == "DropTable" || name == "DropTableAsync";
            }
        }
        return false;
    }

    private static void Execute(SourceProductionContext spc, (SchemaModel? Left, Compilation Right) pair)
    {
        try
        {
            var model = pair.Left;
            if (model is null) return;
            if (model.HasError)
            {
                spc.ReportDiagnostic(Diagnostic.Create(model.ErrorDescriptor ?? Diagnostics.GeneratorFailure, Location.None, model.ErrorArgs ?? Array.Empty<string>()));
                return;
            }

            var source = SchemaEmitter.Emit(pair.Right, model);
            var file = Path.GetFileNameWithoutExtension(model.FilePath);
            var hint = $"FastORM_Schema_{file}_L{model.Line}_C{model.Column}.g.cs";
            spc.AddSource(hint, source);
        }
        catch (Exception ex)
        {
            var m = pair.Left;
            var ctxInfo = m is null ? "" : ($" in {Path.GetFileName(m.FilePath)}:{m.Line}:{m.Column}");
            var msg = ex.GetType().FullName + ": " + ex.Message + ctxInfo + "\n" + ex.StackTrace;
            spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.GeneratorFailure, Location.None, msg));
        }
    }
}

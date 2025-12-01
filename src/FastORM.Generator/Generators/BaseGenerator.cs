using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastORM.Generator;

public abstract class BaseGenerator : IIncrementalGenerator
{
    protected abstract bool IsCandidateMethod(string name);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methodCalls = context.SyntaxProvider.CreateSyntaxProvider(IsCandidateNode, QueryParser.Transform)
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
                return IsCandidateMethod(ma.Name.Identifier.Text);
            }
        }
        return false;
    }

    private static void Execute(SourceProductionContext spc, (QueryModel? Left, Compilation Right) pair)
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

            // Warning for full scan
            // Skip for Insert, and Entity/Batch Update/Delete (which operate by ID)
            if (!model.IsInsert && !model.UpdateIsEntity && !model.UpdateIsBatch && !model.DeleteIsEntity && !model.DeleteIsBatch)
            {
                if (model.Predicates.Count == 0 && !model.TakeCount.HasValue && model.Aggregation is null && model.GroupBy is null)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(Diagnostics.PotentialFullScan, Location.None));
                }
            }

            var source = QueryEmitter.Emit(pair.Right, model);
            var file = Path.GetFileNameWithoutExtension(model.FilePath);
            var hint = $"FastORM_{file}_L{model.Line}_C{model.Column}.g.cs";
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
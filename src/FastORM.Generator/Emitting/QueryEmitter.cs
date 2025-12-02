using System.Text;
using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

internal static class QueryEmitter
{
    public static string Emit(Compilation comp, QueryModel model)
    {
        var sb = new StringBuilder();
        var ns = "FastORM.Generated";
        var elementTypeName = model.Projection is null ? model.ElementType.ToDisplayString() : model.Projection.TypeName;
        sb.Append("namespace ").Append(ns).Append(";\n");
        sb.Append("#nullable enable\n");
        sb.Append("#pragma warning disable CS8601\n");
        sb.Append("#pragma warning disable CS8604\n");
        sb.Append("using System;\nusing System.Collections.Generic;\nusing System.Data.Common;\n");
        if (model.Projection is not null && model.Projection.IsAnonymous)
        {
            sb.Append("internal sealed class ").Append(model.Projection.TypeName).Append("\n{\n");
            for (int i = 0; i < model.Projection.Entries.Count; i++)
            {
                var e = model.Projection.Entries[i];
                var typeName = e.Type?.ToDisplayString() ?? "object";
                var name = e.Alias ?? (e.Kind == ProjectionEntryKind.Property || e.Kind == ProjectionEntryKind.GroupKey ? (e.Property?.Name ?? (e.Aggregator ?? "Col")) : (e.AggregatorProperty is null ? (e.Aggregator ?? "Col") : (e.Aggregator + "_" + e.AggregatorProperty.Name)));
                sb.Append("    public ").Append(typeName).Append(" ").Append(name).Append(" { get; set; }\n");
            }
            sb.Append("}\n");
        }
        var baseName = Path.GetFileNameWithoutExtension(model.FilePath);
        var cls = "Generated_" + baseName.Replace('-', '_').Replace('.', '_') + "_L" + model.Line + "_C" + model.Column;
        sb.Append("internal static class ").Append(cls).Append("{\n");
        var interceptAttr = comp.GetTypeByMetadataName("System.Runtime.CompilerServices.InterceptsLocationAttribute");
        bool supportsVersion = false;
        if (interceptAttr != null)
        {
            foreach (var ctor in interceptAttr.Constructors)
            {
                if (ctor.Parameters.Length == 2 &&
                    ctor.Parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
                    ctor.Parameters[1].Type.SpecialType == SpecialType.System_String)
                { supportsVersion = true; break; }
            }
        }
        if (supportsVersion && model.InterceptVersion != 0 && model.InterceptData.Length > 0)
        {
            sb.Append("[global::System.Runtime.CompilerServices.InterceptsLocation(version: ").Append(model.InterceptVersion).Append(", data: \"").Append(Escape(model.InterceptData)).Append("\")]\n");
        }
        else
        {
            sb.Append("#pragma warning disable CS9270\n");
            sb.Append("[global::System.Runtime.CompilerServices.InterceptsLocation(\"").Append(Escape(model.FilePath)).Append("\",").Append(model.Line).Append(",").Append(model.Column).Append(")]\n");
            
            if (model.FilePath.Length > 1 && model.FilePath[1] == ':')
            {
                var drive = model.FilePath[0];
                var otherDrive = char.IsLower(drive) ? char.ToUpper(drive) : char.ToLower(drive);
                var otherPath = otherDrive + model.FilePath.Substring(1);
                sb.Append("[global::System.Runtime.CompilerServices.InterceptsLocation(\"").Append(Escape(otherPath)).Append("\",").Append(model.Line).Append(",").Append(model.Column).Append(")]\n");
            }

            sb.Append("#pragma warning restore CS9270\n");
        }
        if (!model.IsAsync)
        {
            var recv = model.EndOnIQueryable ? "global::System.Linq.IQueryable<" + elementTypeName + ">" : "global::FastORM.CompilableQuery<" + elementTypeName + ">";
            if (model.Aggregation is not null)
            {
                if (model.Aggregation.Kind == AggregationKind.Count)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        sb.Append("public static int Count<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g)\n{\n");
                    }
                    else
                    {
                        sb.Append("public static int Count(this ").Append(recv).Append(" q)\n{\n");
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Exists)
                {
                    if (model.Aggregation.FilterPredicate is not null)
                        sb.Append("public static bool Any(this ").Append(recv).Append(" q, global::System.Linq.Expressions.Expression<global::System.Func<").Append(elementTypeName).Append(",bool>> predicate)\n{\n");
                    else
                        sb.Append("public static bool Any(this ").Append(recv).Append(" q)\n{\n");
                }
                else if (model.Aggregation.Kind == AggregationKind.NotExists)
                {
                    sb.Append("public static bool All(this ").Append(recv).Append(" q, global::System.Linq.Expressions.Expression<global::System.Func<").Append(elementTypeName).Append(",bool>> predicate)\n{\n");
                }
                else if (model.Aggregation.Kind == AggregationKind.Max)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static ").Append(paramType).Append(" Max<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static ").Append(paramType).Append(" Max(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static ").Append(elementTypeName).Append(" Max(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Min)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static ").Append(paramType).Append(" Min<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static ").Append(paramType).Append(" Min(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static ").Append(elementTypeName).Append(" Min(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Average)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        var retType = pt.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                        sb.Append("public static ").Append(retType).Append(" Average<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            var retType = pt.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                            sb.Append("public static ").Append(retType).Append(" Average(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            var retType = model.ElementType.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                            sb.Append("public static ").Append(retType).Append(" Average(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Sum)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static ").Append(paramType).Append(" Sum<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static ").Append(paramType).Append(" Sum(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static ").Append(elementTypeName).Append(" Sum(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
            }
            else if (model.IsFirstOrDefault)
            {
                sb.Append("public static ").Append(elementTypeName).Append("? FirstOrDefault(this ").Append(recv).Append(" q)\n{\n");
            }
            else if (model.IsInsert)
            {
                 sb.Append("public static int Insert(this global::FastORM.FastDbContext context, ");
                 if (model.InsertIsBatch)
                 {
                     var paramType = model.InsertParameterType?.ToDisplayString() ?? ("global::System.Collections.Generic.IEnumerable<" + elementTypeName + ">");
                     sb.Append(paramType).Append(" entities)\n{\n");
                 }
                 else
                 {
                     var paramType = model.InsertParameterType?.ToDisplayString() ?? elementTypeName;
                     sb.Append(paramType).Append(" entity)\n{\n");
                 }
            }
            else if (model.IsDelete)
            {
                if (model.DeleteIsEntity || model.DeleteIsBatch)
                {
                    var paramType = model.DeleteParameterType?.ToDisplayString() ?? elementTypeName;
                    var paramName = model.DeleteIsBatch ? "entities" : "entity";
                    sb.Append("public static int Delete(this global::FastORM.FastDbContext context, ").Append(paramType).Append(" ").Append(paramName).Append(")\n{\n");
                }
                else
                {
                    sb.Append("public static int Delete(this ").Append(recv).Append(" q)\n{\n");
                }
            }
            else if (model.IsUpdate)
            {
                if (model.UpdateIsEntity || model.UpdateIsBatch)
                {
                    var paramType = model.UpdateParameterType?.ToDisplayString() ?? elementTypeName;
                    var paramName = model.UpdateIsBatch ? "entities" : "entity";
                    sb.Append("public static int Update(this global::FastORM.FastDbContext context, ").Append(paramType).Append(" ").Append(paramName).Append(")\n{\n");
                }
                else
                {
                    sb.Append("public static int Update(this ").Append(recv).Append(" q, global::System.Action<").Append(elementTypeName).Append("> updateAction)\n{\n");
                }
            }
            else
            {
                sb.Append("public static System.Collections.Generic.List<").Append(elementTypeName).Append("> ToList(this ").Append(recv).Append(" q)\n{\n");
            }
        }
        else
        {
            var recv = model.EndOnIQueryable ? "global::System.Linq.IQueryable<" + elementTypeName + ">" : "global::FastORM.CompilableQuery<" + elementTypeName + ">";
            if (model.Aggregation is not null)
            {
                 if (model.Aggregation.Kind == AggregationKind.Count)
                {
                     if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        sb.Append("public static async global::System.Threading.Tasks.Task<int> CountAsync<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g)\n{\n");
                    }
                    else
                    {
                        sb.Append("public static async global::System.Threading.Tasks.Task<int> CountAsync(this ").Append(recv).Append(" q)\n{\n");
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Exists)
                {
                    if (model.Aggregation.FilterPredicate is not null)
                         sb.Append("public static async global::System.Threading.Tasks.Task<bool> AnyAsync(this ").Append(recv).Append(" q, global::System.Linq.Expressions.Expression<global::System.Func<").Append(elementTypeName).Append(",bool>> predicate)\n{\n");
                    else
                        sb.Append("public static async global::System.Threading.Tasks.Task<bool> AnyAsync(this ").Append(recv).Append(" q)\n{\n");
                }
                else if (model.Aggregation.Kind == AggregationKind.NotExists)
                {
                    sb.Append("public static async global::System.Threading.Tasks.Task<bool> AllAsync(this ").Append(recv).Append(" q, global::System.Linq.Expressions.Expression<global::System.Func<").Append(elementTypeName).Append(",bool>> predicate)\n{\n");
                }
                else if (model.Aggregation.Kind == AggregationKind.Max)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> MaxAsync<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> MaxAsync(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(elementTypeName).Append("> MaxAsync(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Min)
                {
                     if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> MinAsync<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> MinAsync(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(elementTypeName).Append("> MinAsync(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                 else if (model.Aggregation.Kind == AggregationKind.Average)
                {
                     if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        var retType = pt.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                        sb.Append("public static async global::System.Threading.Tasks.Task<").Append(retType).Append("> AverageAsync<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            var retType = pt.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(retType).Append("> AverageAsync(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            var retType = model.ElementType.SpecialType == SpecialType.System_Decimal ? "decimal" : "double";
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(retType).Append("> AverageAsync(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
                else if (model.Aggregation.Kind == AggregationKind.Sum)
                {
                    if (model.AggregationReceiver == AggregationReceiverKind.Group)
                    {
                        var pt = model.Aggregation.Property!.Type;
                        var paramType = pt.ToDisplayString();
                        sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> SumAsync<TSource,TKey>(this global::FastORM.Group<TSource,TKey> g, global::System.Func<TSource,").Append(paramType).Append("> selector)\n{\n");
                    }
                    else
                    {
                        if (model.Aggregation.Property != null)
                        {
                            var pt = model.Aggregation.Property.Type;
                            var paramType = pt.ToDisplayString();
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(paramType).Append("> SumAsync(this ").Append(recv).Append(" q, global::System.Func<").Append(elementTypeName).Append(",").Append(paramType).Append("> selector)\n{\n");
                        }
                        else
                        {
                            sb.Append("public static async global::System.Threading.Tasks.Task<").Append(elementTypeName).Append("> SumAsync(this ").Append(recv).Append(" q)\n{\n");
                        }
                    }
                }
            }
            else if (model.IsFirstOrDefault)
            {
                sb.Append("public static async global::System.Threading.Tasks.Task<").Append(elementTypeName).Append("?> FirstOrDefaultAsync(this ").Append(recv).Append(" q)\n{\n");
            }
            else if (model.IsInsert)
            {
                 sb.Append("public static async global::System.Threading.Tasks.Task<int> InsertAsync(this global::FastORM.FastDbContext context, ");
                 if (model.InsertIsBatch)
                 {
                     var paramType = model.InsertParameterType?.ToDisplayString() ?? ("global::System.Collections.Generic.IEnumerable<" + elementTypeName + ">");
                     sb.Append(paramType).Append(" entities, global::System.Threading.CancellationToken cancellationToken = default)\n{\n");
                 }
                 else
                 {
                     var paramType = model.InsertParameterType?.ToDisplayString() ?? elementTypeName;
                     sb.Append(paramType).Append(" entity, global::System.Threading.CancellationToken cancellationToken = default)\n{\n");
                 }
            }
            else if (model.IsDelete)
            {
                if (model.DeleteIsEntity || model.DeleteIsBatch)
                {
                    var paramType = model.DeleteParameterType?.ToDisplayString() ?? elementTypeName;
                    var paramName = model.DeleteIsBatch ? "entities" : "entity";
                    sb.Append("public static async global::System.Threading.Tasks.Task<int> DeleteAsync(this global::FastORM.FastDbContext context, ").Append(paramType).Append(" ").Append(paramName).Append(", global::System.Threading.CancellationToken cancellationToken = default)\n{\n");
                }
                else
                {
                    sb.Append("public static async global::System.Threading.Tasks.Task<int> DeleteAsync(this ").Append(recv).Append(" q)\n{\n");
                }
            }
            else if (model.IsUpdate)
            {
                if (model.UpdateIsEntity || model.UpdateIsBatch)
                {
                    var paramType = model.UpdateParameterType?.ToDisplayString() ?? elementTypeName;
                    var paramName = model.UpdateIsBatch ? "entities" : "entity";
                    sb.Append("public static async global::System.Threading.Tasks.Task<int> UpdateAsync(this global::FastORM.FastDbContext context, ").Append(paramType).Append(" ").Append(paramName).Append(", global::System.Threading.CancellationToken cancellationToken = default)\n{\n");
                }
                else
                {
                    sb.Append("public static async global::System.Threading.Tasks.Task<int> UpdateAsync(this ").Append(recv).Append(" q, global::System.Action<").Append(elementTypeName).Append("> updateAction)\n{\n");
                }
            }
            else
            {
                sb.Append("public static async global::System.Threading.Tasks.Task<System.Collections.Generic.List<").Append(elementTypeName).Append(">> ToListAsync(this ").Append(recv).Append(" q)\n{\n");
            }
        }

        if (model.EndOnIQueryable)
        {
             sb.Append("    var context = ((global::FastORM.FastOrmQueryable<").Append(elementTypeName).Append(">)q).Context;\n");
        }
        else
        {
             if (!model.IsInsert && !(model.IsUpdate && (model.UpdateIsEntity || model.UpdateIsBatch)) && !(model.IsDelete && (model.DeleteIsEntity || model.DeleteIsBatch))) sb.Append("    var context = q.Context;\n");
        }
        int paramCounter = 0;
        
        if (!model.IsInsert && !(model.IsUpdate && (model.UpdateIsEntity || model.UpdateIsBatch)) && !(model.IsDelete && (model.DeleteIsEntity || model.DeleteIsBatch)) && !model.EndOnIQueryable) 
        {
            sb.Append("    var runtimeParams_ = global::FastORM.Internal.ValueExtractor.GetValues(q);\n");
        }
        else
        {
            sb.Append("    var runtimeParams_ = new System.Collections.Generic.List<object?>();\n");
        }

        if (model.IsInsert)
        {
             sb.Append("    var sb = new System.Text.StringBuilder();\n");
             sb.Append("    sb.Append(\"INSERT INTO \").Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\" (\");\n");
             var props = model.InsertProperties;
             for(int i=0; i<props.Count; i++)
             {
                 if(i>0) sb.Append("    sb.Append(\", \");\n");
                 sb.Append("    sb.Append(context.Quote(\"").Append(ColumnName(props[i])).Append("\"));\n");
             }
             sb.Append("    sb.Append(\") VALUES \");\n");
             
             if (model.InsertIsBatch)
             {
                 sb.Append("    int i = 0;\n");
                 sb.Append("    var list = entities as System.Collections.Generic.IList<").Append(elementTypeName).Append("> ?? System.Linq.Enumerable.ToList(entities);\n");
                 sb.Append("    if (list.Count == 0) return 0;\n");
                 sb.Append("    foreach(var entity in list) {\n");
                 sb.Append("        if (i > 0) sb.Append(\", \");\n");
                 sb.Append("        sb.Append(\"(\");\n");
                 for(int j=0; j<props.Count; j++)
                 {
                     if(j>0) sb.Append("        sb.Append(\", \");\n");
                     sb.Append("        sb.Append(\"@p\").Append(i).Append(\"_\").Append(").Append(j).Append(");\n");
                 }
                 sb.Append("        sb.Append(\")\");\n");
                 sb.Append("        i++;\n");
                 sb.Append("    }\n");
             }
             else
             {
                  sb.Append("    sb.Append(\"(\");\n");
                  for(int j=0; j<props.Count; j++)
                  {
                      if(j>0) sb.Append("    sb.Append(\", \");\n");
                      sb.Append("    sb.Append(\"@p0_\").Append(").Append(j).Append(");\n");
                  }
                  sb.Append("    sb.Append(\")\");\n");
             }

             sb.Append("    var conn = context.Connection;\n");
             if (model.IsAsync) sb.Append("    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken);\n");
              else sb.Append("    if (conn.State != System.Data.ConnectionState.Open) conn.Open();\n");
             sb.Append("    using var cmd = conn.CreateCommand();\n");
             sb.Append("    cmd.CommandText = sb.ToString();\n");
             
             if (model.InsertIsBatch)
             {
                 sb.Append("    int pIdx = 0;\n");
                 sb.Append("    foreach(var entity in list) {\n");
                 for(int j=0; j<props.Count; j++)
                 {
                     sb.Append("        var p").Append(j).Append(" = cmd.CreateParameter();\n");
                     sb.Append("        p").Append(j).Append(".ParameterName = \"@p\" + pIdx + \"_\" + ").Append(j).Append(";\n");
                     sb.Append("        p").Append(j).Append(".Value = (object)entity.").Append(props[j].Name).Append(" ?? DBNull.Value;\n");
                     sb.Append("        cmd.Parameters.Add(p").Append(j).Append(");\n");
                 }
                 sb.Append("        pIdx++;\n");
                 sb.Append("    }\n");
             }
             else
             {
                 for(int j=0; j<props.Count; j++)
                 {
                     sb.Append("    var p").Append(j).Append(" = cmd.CreateParameter();\n");
                     sb.Append("    p").Append(j).Append(".ParameterName = \"@p0_\" + ").Append(j).Append(";\n");
                     sb.Append("    p").Append(j).Append(".Value = (object)entity.").Append(props[j].Name).Append(" ?? DBNull.Value;\n");
                     sb.Append("    cmd.Parameters.Add(p").Append(j).Append(");\n");
                 }
             }

             if (model.IsAsync) sb.Append("    return await cmd.ExecuteNonQueryAsync(cancellationToken);\n");
             else sb.Append("    return cmd.ExecuteNonQuery();\n");
             sb.Append("}\n");
             sb.Append("}\n");
             return sb.ToString();
        }

        if (model.IsUpdate && (model.UpdateIsEntity || model.UpdateIsBatch))
        {
             var props = GetProperties(comp, model.ElementType).ToList();
             var keyProp = QueryParser.GetPrimaryKey(model.ElementType);
             var keyPropName = keyProp?.Name ?? "Id";
             var keyCol = keyProp != null ? ColumnName(keyProp) : "Id";
             
             sb.Append("    var sb = new System.Text.StringBuilder();\n");
             sb.Append("    var conn = context.Connection;\n");
             if (model.IsAsync) sb.Append("    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken);\n");
             else sb.Append("    if (conn.State != System.Data.ConnectionState.Open) conn.Open();\n");
             
             if (model.UpdateIsBatch)
             {
                 sb.Append("    var list = entities as System.Collections.Generic.IList<").Append(elementTypeName).Append("> ?? System.Linq.Enumerable.ToList(entities);\n");
                 sb.Append("    if (list.Count == 0) return 0;\n");
                 sb.Append("    int affected = 0;\n");
                 sb.Append("    foreach(var entity in list) {\n");
             }
             else
             {
                 sb.Append("    int affected = 0;\n");
                 sb.Append("    {\n");
             }
             
             sb.Append("        using var cmd = conn.CreateCommand();\n");
             sb.Append("        sb.Clear();\n");
             sb.Append("        sb.Append(\"UPDATE \").Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\" SET \");\n");
             
             bool first = true;
             int pIdx = 0;
             var updateProps = props.Where(p => p.Name != keyPropName).ToList();
             
             foreach(var p in updateProps)
             {
                 if (!first) sb.Append("        sb.Append(\", \");\n");
                 sb.Append("        sb.Append(context.Quote(\"").Append(ColumnName(p)).Append("\")).Append(\" = @p").Append(pIdx).Append("\");\n");
                 first = false;
                 pIdx++;
             }
             sb.Append("        sb.Append(\" WHERE \").Append(context.Quote(\"").Append(keyCol).Append("\")).Append(\" = @pk\");\n");
             
             sb.Append("        cmd.CommandText = sb.ToString();\n");
             
             pIdx = 0;
             foreach(var p in updateProps)
             {
                 sb.Append("        var p").Append(pIdx).Append(" = cmd.CreateParameter();\n");
                 sb.Append("        p").Append(pIdx).Append(".ParameterName = \"@p").Append(pIdx).Append("\";\n");
                 sb.Append("        p").Append(pIdx).Append(".Value = (object)entity.").Append(p.Name).Append(" ?? DBNull.Value;\n");
                 sb.Append("        cmd.Parameters.Add(p").Append(pIdx).Append(");\n");
                 pIdx++;
             }
             
             sb.Append("        var pk = cmd.CreateParameter();\n");
             sb.Append("        pk.ParameterName = \"@pk\";\n");
             sb.Append("        pk.Value = (object)entity.").Append(keyPropName).Append(" ?? DBNull.Value;\n");
             sb.Append("        cmd.Parameters.Add(pk);\n");
             
             if (model.IsAsync) sb.Append("        affected += await cmd.ExecuteNonQueryAsync(cancellationToken);\n");
             else sb.Append("        affected += cmd.ExecuteNonQuery();\n");
             
             sb.Append("    }\n");
             
             sb.Append("    return affected;\n");
             sb.Append("}\n");
             sb.Append("}\n");
             return sb.ToString();
        }

        if (model.IsDelete && (model.DeleteIsEntity || model.DeleteIsBatch))
        {
             var keyProp = QueryParser.GetPrimaryKey(model.ElementType);
             var keyPropName = keyProp?.Name ?? "Id";
             var keyCol = keyProp != null ? ColumnName(keyProp) : "Id";
             
             sb.Append("    var sb = new System.Text.StringBuilder();\n");
             sb.Append("    var conn = context.Connection;\n");
             if (model.IsAsync) sb.Append("    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(cancellationToken);\n");
             else sb.Append("    if (conn.State != System.Data.ConnectionState.Open) conn.Open();\n");
             
             if (model.DeleteIsBatch)
             {
                 sb.Append("    var list = entities as System.Collections.Generic.IList<").Append(elementTypeName).Append("> ?? System.Linq.Enumerable.ToList(entities);\n");
                 sb.Append("    if (list.Count == 0) return 0;\n");
                 
                 sb.Append("    sb.Append(\"DELETE FROM \").Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\" WHERE \").Append(context.Quote(\"").Append(keyCol).Append("\")).Append(\" IN (\");\n");
                 sb.Append("    for(int i=0; i<list.Count; i++) {\n");
                 sb.Append("        if (i > 0) sb.Append(\", \");\n");
                 sb.Append("        sb.Append(\"@p\").Append(i);\n");
                 sb.Append("    }\n");
                 sb.Append("    sb.Append(\")\");\n");
                 
                 sb.Append("    using var cmd = conn.CreateCommand();\n");
                 sb.Append("    cmd.CommandText = sb.ToString();\n");
                 
                 sb.Append("    int pIdx = 0;\n");
                 sb.Append("    foreach(var entity in list) {\n");
                 sb.Append("        var p = cmd.CreateParameter();\n");
                 sb.Append("        p.ParameterName = \"@p\" + pIdx;\n");
                 sb.Append("        p.Value = (object)entity.").Append(keyPropName).Append(" ?? DBNull.Value;\n");
                 sb.Append("        cmd.Parameters.Add(p);\n");
                 sb.Append("        pIdx++;\n");
                 sb.Append("    }\n");
                 
                 if (model.IsAsync) sb.Append("    return await cmd.ExecuteNonQueryAsync(cancellationToken);\n");
                 else sb.Append("    return cmd.ExecuteNonQuery();\n");
             }
             else
             {
                 sb.Append("    sb.Append(\"DELETE FROM \").Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\" WHERE \").Append(context.Quote(\"").Append(keyCol).Append("\")).Append(\" = @pk\");\n");
                 sb.Append("    using var cmd = conn.CreateCommand();\n");
                 sb.Append("    cmd.CommandText = sb.ToString();\n");
                 sb.Append("    var pk = cmd.CreateParameter();\n");
                 sb.Append("    pk.ParameterName = \"@pk\";\n");
                 sb.Append("    pk.Value = (object)entity.").Append(keyPropName).Append(" ?? DBNull.Value;\n");
                 sb.Append("    cmd.Parameters.Add(pk);\n");
                 
                 if (model.IsAsync) sb.Append("    return await cmd.ExecuteNonQueryAsync(cancellationToken);\n");
                 else sb.Append("    return cmd.ExecuteNonQuery();\n");
             }
             sb.Append("}\n");
             sb.Append("}\n");
             return sb.ToString();
        }

        int varCounter = 0;
        
        var allPredicates = new System.Collections.Generic.List<PredicateModel>(model.Predicates);
        if (model.Aggregation?.FilterPredicate != null)
        {
            allPredicates.Add(model.Aggregation.FilterPredicate);
        }

        AssignParameterIndices(allPredicates, ref paramCounter, ref varCounter);

        bool isDynamic = CheckIsDynamic(allPredicates) || model.DynamicPredicates.Count > 0 || model.EndOnIQueryable;

        if (isDynamic)
        {
            sb.Append("    var whereBuilder = new System.Text.StringBuilder();\n");

            if (model.EndOnIQueryable)
            {
                 sb.Append("    whereBuilder.Append(global::FastORM.Internal.ExpressionToSql.TranslateQueryPredicates(q.Expression, context, runtimeParams_));\n");
            }
            else
            {
                if (allPredicates.Count > 0)
                {
                     bool isFirst = true;
                     foreach(var p in allPredicates)
                     {
                         bool negate = model.Aggregation?.NegateFilter == true && p == model.Aggregation.FilterPredicate;
                         if (!isFirst) sb.Append("    whereBuilder.Append(\" AND \");\n");
                         else isFirst = false;
                         if (negate) sb.Append("    whereBuilder.Append(\"NOT (\");\n");
                         
                         EmitPredicate(sb, p, model);
                         
                         if (negate) sb.Append("    whereBuilder.Append(\")\");\n");
                     }
                }
                
                if (model.DynamicPredicates.Count > 0)
                {
                     bool isFirst = allPredicates.Count == 0;
                     for(int i=0; i<model.DynamicPredicates.Count; i++)
                     {
                         if (!isFirst) sb.Append("    whereBuilder.Append(\" AND \");\n");
                         else isFirst = false;
                         sb.Append("    whereBuilder.Append(global::FastORM.Internal.ExpressionToSql.Translate(").Append(model.DynamicPredicates[i]).Append(", context, runtimeParams_));\n");
                     }
                }
            }
        }
        
        if (!isDynamic)
        {
             var sqlServer = BuildSql(model, comp, GeneratorDialect.SqlServer);
             var mySql = BuildSql(model, comp, GeneratorDialect.MySql);
             var pgSql = BuildSql(model, comp, GeneratorDialect.PostgreSql);
             var sqlite = BuildSql(model, comp, GeneratorDialect.Sqlite);
             
             var sqlMap = new Dictionary<string, List<string>>();
             void AddSql(string s, string d) {
                 if (!sqlMap.ContainsKey(s)) sqlMap[s] = new List<string>();
                 sqlMap[s].Add(d);
             }
             AddSql(sqlServer, "FastORM.SqlDialect.SqlServer");
             AddSql(mySql, "FastORM.SqlDialect.MySql");
             AddSql(pgSql, "FastORM.SqlDialect.PostgreSql");
             AddSql(sqlite, "FastORM.SqlDialect.Sqlite");
             
             sb.Append("    string sql;\n");
             sb.Append("    switch(context.Dialect) {\n");
             foreach(var kv in sqlMap)
             {
                 foreach(var d in kv.Value)
                 {
                     sb.Append("        case ").Append(d).Append(":\n");
                 }
                 sb.Append("            sql = \"").Append(Escape(kv.Key)).Append("\"; break;\n");
             }
             sb.Append("        default: throw new System.NotSupportedException($\"Dialect {context.Dialect} not supported\");\n");
             sb.Append("    }\n");
        }
        else
        {
            sb.Append("    var sb = new System.Text.StringBuilder();\n");
        if (model.IsDelete)
        {
            sb.Append("    sb.Append(\"DELETE FROM \").Append(context.Quote(\"").Append(model.TableName).Append("\"));\n");
            sb.Append("    if (whereBuilder.Length > 0) sb.Append(\" WHERE \").Append(whereBuilder);\n");
        }
        else if (model.IsUpdate)
        {
            sb.Append("    sb.Append(\"UPDATE \").Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\" SET \");\n");
            for (int i = 0; i < model.Updates.Count; i++)
            {
                if (i > 0) sb.Append("    sb.Append(\", \");\n");
                sb.Append("    sb.Append(context.Quote(\"").Append(model.Updates[i].Column).Append("\")).Append(\" = @p_u_").Append(i).Append("\");\n");
            }
            sb.Append("    if (whereBuilder.Length > 0) sb.Append(\" WHERE \").Append(whereBuilder);\n");
        }
        else if (model.Aggregation is not null && (model.Aggregation.Kind == AggregationKind.Exists || model.Aggregation.Kind == AggregationKind.NotExists))
        {
             sb.Append("    sb.Append(\"SELECT CASE WHEN EXISTS (SELECT 1 FROM \").Append(context.Quote(\"").Append(model.TableName).Append("\"));\n");
             // Joins for Exists
             if (model.Join is not null && model.Join.OuterKey is not null && model.Join.InnerKey is not null)
             {
                 var joinType = model.Join.Kind == JoinKind.Left ? " LEFT JOIN " : (model.Join.Kind == JoinKind.Right ? " RIGHT JOIN " : " INNER JOIN ");
                 sb.Append("    sb.Append(\"").Append(joinType).Append("\").Append(context.Quote(\"").Append(model.Join.InnerTable).Append("\")).Append(\" ON \");\n");
                 sb.Append("    sb.Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\".\").Append(context.Quote(\"").Append(ColumnName(model.Join.OuterKey)).Append("\"));\n");
                 sb.Append("    sb.Append(\" = \");\n");
                 sb.Append("    sb.Append(context.Quote(\"").Append(model.Join.InnerTable).Append("\")).Append(\".\").Append(context.Quote(\"").Append(ColumnName(model.Join.InnerKey)).Append("\"));\n");
             }
             sb.Append("    if (whereBuilder.Length > 0) sb.Append(\" WHERE \").Append(whereBuilder);\n");
             sb.Append("    sb.Append(\") THEN 1 ELSE 0 END\");\n");
        }
        else
        {
             // SELECT
             sb.Append("    sb.Append(\"SELECT \");\n");
             if (model.IsDistinct) sb.Append("    sb.Append(\"DISTINCT \");\n");
             
             // SqlServer TOP
             if (model.TakeCount.HasValue && !model.SkipCount.HasValue)
             {
                 sb.Append("    if (context.Dialect == FastORM.SqlDialect.SqlServer) sb.Append(\"TOP ").Append(model.TakeCount.Value).Append(" \");\n");
             }

             // Columns
             bool firstCol = true;
             void AddCol(string c) {
                 if (!firstCol) sb.Append("    sb.Append(\", \");\n");
                 sb.Append("    sb.Append(").Append(c).Append(");\n");
                 firstCol = false;
             }
             
            if (model.Aggregation is not null)
            {
                 if (model.Aggregation.Kind == AggregationKind.Count)
                {
                    AddCol("\"COUNT(*)\"");
                }
                else if (model.Aggregation.Kind == AggregationKind.Max)
                {
                    string col = model.Aggregation.Property is not null ? "context.Quote(\"" + ColumnName(model.Aggregation.Property) + "\")" : "\"COL_AGG\"";
                    AddCol("\"MAX(\" + " + col + " + \")\"");
                }
                else if (model.Aggregation.Kind == AggregationKind.Min)
                {
                    string col = model.Aggregation.Property is not null ? "context.Quote(\"" + ColumnName(model.Aggregation.Property) + "\")" : "\"COL_AGG\"";
                    AddCol("\"MIN(\" + " + col + " + \")\"");
                }
                else if (model.Aggregation.Kind == AggregationKind.Average)
                {
                    string col = model.Aggregation.Property is not null ? "context.Quote(\"" + ColumnName(model.Aggregation.Property) + "\")" : "\"COL_AGG\"";
                    AddCol("\"AVG(\" + " + col + " + \")\"");
                }
                 else if (model.Aggregation.Kind == AggregationKind.Sum)
                {
                    string col = model.Aggregation.Property is not null ? "context.Quote(\"" + ColumnName(model.Aggregation.Property) + "\")" : "\"COL_AGG\"";
                    AddCol("\"SUM(\" + " + col + " + \")\"");
                }
            }
            else if (model.Projection is not null)
            {
                for (int i = 0; i < model.Projection.Entries.Count; i++)
                {
                    var e = model.Projection.Entries[i];
                    string colExpr = "";
                    if (e.Kind == ProjectionEntryKind.Property)
                    {
                        colExpr = "context.Quote(\"" + ColumnName(e.Property!) + "\")";
                    }
                    else if (e.Kind == ProjectionEntryKind.GroupKey)
                    {
                         if (e.Property?.Name == "Key" && model.GroupBy?.Keys.Count == 1)
                         {
                             colExpr = "context.Quote(\"" + ColumnName(model.GroupBy.Keys[0]) + "\")";
                         }
                         else
                         {
                             colExpr = "context.Quote(\"" + ColumnName(e.Property!) + "\")";
                         }
                    }
                    else
                    {
                        if (e.Aggregator == "COUNT") colExpr = "\"COUNT(*)\"";
                        else if (e.Aggregator == "MAX") colExpr = "\"MAX(\" + context.Quote(\"" + ColumnName(e.AggregatorProperty!) + "\") + \")\"";
                        else if (e.Aggregator == "MIN") colExpr = "\"MIN(\" + context.Quote(\"" + ColumnName(e.AggregatorProperty!) + "\") + \")\"";
                        else if (e.Aggregator == "SUM") colExpr = "\"SUM(\" + context.Quote(\"" + ColumnName(e.AggregatorProperty!) + "\") + \")\"";
                        else if (e.Aggregator == "AVERAGE") colExpr = "\"AVG(\" + context.Quote(\"" + ColumnName(e.AggregatorProperty!) + "\") + \")\"";
                    }
                    
                    if (!string.IsNullOrEmpty(e.Alias))
                    {
                        colExpr += " + \" AS \" + context.Quote(\"" + e.Alias + "\")";
                    }
                    AddCol(colExpr);
                }
            }
            else
            {
                var props = GetProperties(comp, model.ElementType);
                foreach (var p in props)
                {
                    AddCol("context.Quote(\"" + ColumnName(p) + "\")");
                }
            }

             sb.Append("    sb.Append(\" FROM \").Append(context.Quote(\"").Append(model.TableName).Append("\"));\n");

             // Joins
             if (model.Join is not null && model.Join.OuterKey is not null && model.Join.InnerKey is not null)
             {
                 var joinType = model.Join.Kind == JoinKind.Left ? " LEFT JOIN " : (model.Join.Kind == JoinKind.Right ? " RIGHT JOIN " : " INNER JOIN ");
                 sb.Append("    sb.Append(\"").Append(joinType).Append("\").Append(context.Quote(\"").Append(model.Join.InnerTable).Append("\")).Append(\" ON \");\n");
                 sb.Append("    sb.Append(context.Quote(\"").Append(model.TableName).Append("\")).Append(\".\").Append(context.Quote(\"").Append(ColumnName(model.Join.OuterKey)).Append("\"));\n");
                 sb.Append("    sb.Append(\" = \");\n");
                 sb.Append("    sb.Append(context.Quote(\"").Append(model.Join.InnerTable).Append("\")).Append(\".\").Append(context.Quote(\"").Append(ColumnName(model.Join.InnerKey)).Append("\"));\n");
             }

             sb.Append("    if (whereBuilder.Length > 0) sb.Append(\" WHERE \").Append(whereBuilder);\n");
             
             // Group By
              if (model.GroupBy != null && model.GroupBy.Keys.Count > 0)
              {
                  sb.Append("    sb.Append(\" GROUP BY \");\n");
                  for(int i=0; i<model.GroupBy.Keys.Count; i++)
                  {
                      if (i > 0) sb.Append("    sb.Append(\", \");\n");
                      sb.Append("    sb.Append(context.Quote(\"").Append(ColumnName(model.GroupBy.Keys[i])).Append("\"));\n");
                  }
              }
              
              // Order By
              if (model.OrderBy != null && model.OrderBy.Count > 0)
              {
                  sb.Append("    sb.Append(\" ORDER BY \");\n");
                  for(int i=0; i<model.OrderBy.Count; i++)
                  {
                      if (i > 0) sb.Append("    sb.Append(\", \");\n");
                      sb.Append("    sb.Append(context.Quote(\"").Append(ColumnName(model.OrderBy[i].prop)).Append("\"));\n");
                      if (model.OrderBy[i].desc) sb.Append("    sb.Append(\" DESC\");\n");
                  }
              }

             // Limit/Offset
             if (model.TakeCount.HasValue || model.SkipCount.HasValue)
             {
                 string take = model.TakeCount.HasValue ? model.TakeCount.Value.ToString() : "null";
                 string skip = model.SkipCount.HasValue ? model.SkipCount.Value.ToString() : "0";
                 
                 sb.Append("    if (context.Dialect == FastORM.SqlDialect.SqlServer) {\n");
                 if (model.SkipCount.HasValue)
                 {
                      sb.Append("        sb.Append(\" OFFSET ").Append(skip).Append(" ROWS\");\n");
                      if (model.TakeCount.HasValue) sb.Append("        sb.Append(\" FETCH NEXT ").Append(take).Append(" ROWS ONLY\");\n");
                 }
                 sb.Append("    } else {\n");
                  if (model.TakeCount.HasValue) sb.Append("        sb.Append(\" LIMIT ").Append(take).Append("\");\n");
                  else if (model.SkipCount.HasValue)
                  {
                       sb.Append("        if (context.Dialect == FastORM.SqlDialect.Sqlite) sb.Append(\" LIMIT -1\");\n");
                  }
                  if (model.SkipCount.HasValue) sb.Append("        sb.Append(\" OFFSET ").Append(skip).Append("\");\n");
                  sb.Append("    }\n");
             }
        }
        }

        sb.Append("    var conn = context.Connection;\n");
        if (model.IsAsync)
        {
             sb.Append("    if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();\n");
        }
        else
        {
             sb.Append("    if (conn.State != System.Data.ConnectionState.Open) conn.Open();\n");
        }
        sb.Append("    using var cmd = conn.CreateCommand();\n");
        
        // Inject Runtime Parameter Extractor
        if (!isDynamic)
            sb.Append("    cmd.CommandText = sql;\n");
        else
            sb.Append("    cmd.CommandText = sb.ToString();\n");

        // Add parameters
        if (model.IsUpdate)
        {
            for (int i = 0; i < model.Updates.Count; i++)
            {
                 sb.Append("    var p_u_").Append(i).Append(" = cmd.CreateParameter();\n");
                 sb.Append("    p_u_").Append(i).Append(".ParameterName = \"@p_u_").Append(i).Append("\";\n");
                 sb.Append("    p_u_").Append(i).Append(".Value = (object)").Append(model.Updates[i].ValueExpressionCode).Append(" ?? DBNull.Value;\n");
                 sb.Append("    cmd.Parameters.Add(p_u_").Append(i).Append(");\n");
            }
        }
         int paramLocalIndex = 0;
         if (!model.EndOnIQueryable)
         {
             foreach(var p in allPredicates)
             {
                  EmitParameters(sb, p, ref paramLocalIndex);
             }
         }
         
         sb.Append("    for(int i=0; i<runtimeParams_.Count; i++) {\n");
         sb.Append("        var p = cmd.CreateParameter();\n");
         sb.Append("        p.ParameterName = \"@dyn_\" + i;\n");
         sb.Append("        p.Value = runtimeParams_[i] ?? DBNull.Value;\n");
         sb.Append("        cmd.Parameters.Add(p);\n");
         sb.Append("    }\n");

        if (model.IsAsync)
        {
            if (model.IsDelete || model.IsUpdate)
            {
                sb.Append("    return await cmd.ExecuteNonQueryAsync();\n");
            }
            else
            {
            sb.Append("    using var reader = await cmd.ExecuteReaderAsync();\n");
            if (model.Aggregation is not null)
            {
                if (model.Aggregation.Kind == AggregationKind.Count)
                {
                    sb.Append("    if (await reader.ReadAsync()) return reader.GetInt32(0);\n    return 0;\n");
                }
                 else if (model.Aggregation.Kind == AggregationKind.Exists || model.Aggregation.Kind == AggregationKind.NotExists)
                {
                    if (model.Aggregation.Kind == AggregationKind.NotExists)
                         sb.Append("    if (await reader.ReadAsync()) return reader.GetInt32(0) == 0;\n    return true;\n");
                    else
                        sb.Append("    if (await reader.ReadAsync()) return reader.GetInt32(0) == 1;\n    return false;\n");
                }
                else
                {
                     string readerMethod;
                     if (model.Aggregation.Kind == AggregationKind.Average)
                     {
                         var pt = model.Aggregation.Property?.Type ?? model.ElementType;
                         readerMethod = pt.SpecialType == SpecialType.System_Decimal ? "GetDecimal" : "GetDouble";
                     }
                     else
                     {
                         readerMethod = ReaderFor(model.Aggregation.Property!.Type);
                     }
                     sb.Append("    if (await reader.ReadAsync()) return reader.").Append(readerMethod).Append("(0);\n    return default;\n");
                }
            }
            else if (model.IsFirstOrDefault)
            {
                 sb.Append("    if (await reader.ReadAsync()) {\n");
                if (model.Projection is not null)
                {
                     sb.Append("        var item = new ").Append(model.Projection.TypeName).Append("();\n");
                     for(int i=0; i<model.Projection.Entries.Count; i++)
                     {
                         var e = model.Projection.Entries[i];
                         var name = e.Alias ?? (e.Kind == ProjectionEntryKind.Property || e.Kind == ProjectionEntryKind.GroupKey ? (e.Property?.Name ?? (e.Aggregator ?? "Col")) : (e.AggregatorProperty is null ? (e.Aggregator ?? "Col") : (e.Aggregator + "_" + e.AggregatorProperty.Name)));
                         var t = e.Type ?? e.Property!.Type;
                         sb.Append("        item.").Append(name).Append(" = reader.IsDBNull(").Append(i).Append(") ? default : reader.").Append(ReaderFor(t)).Append("(").Append(i).Append(");\n");
                     }
                     sb.Append("        return item;\n    }\n    return default;\n");
                }
                else
                {
                    sb.Append("        var item = new ").Append(elementTypeName).Append("();\n");
                    var props = GetProperties(comp, model.ElementType);
                    int i = 0;
                    foreach (var p in props)
                    {
                        sb.Append("        item.").Append(p.Name).Append(" = reader.").Append(ReaderFor(p.Type)).Append("(").Append(i++).Append(");\n");
                    }
                     sb.Append("        return item;\n    }\n    return default;\n");
                }
            }
            else
            {
                sb.Append("    var list = new System.Collections.Generic.List<").Append(elementTypeName).Append(">();\n");
                sb.Append("    while (await reader.ReadAsync()) {\n");
                if (model.Projection is not null)
                {
                     sb.Append("        var item = new ").Append(model.Projection.TypeName).Append("();\n");
                     for(int i=0; i<model.Projection.Entries.Count; i++)
                     {
                         var e = model.Projection.Entries[i];
                         var name = e.Alias ?? (e.Kind == ProjectionEntryKind.Property || e.Kind == ProjectionEntryKind.GroupKey ? (e.Property?.Name ?? (e.Aggregator ?? "Col")) : (e.AggregatorProperty is null ? (e.Aggregator ?? "Col") : (e.Aggregator + "_" + e.AggregatorProperty.Name)));
                         var t = e.Type ?? e.Property!.Type;
                         sb.Append("        item.").Append(name).Append(" = reader.IsDBNull(").Append(i).Append(") ? default : reader.").Append(ReaderFor(t)).Append("(").Append(i).Append(");\n");
                     }
                     sb.Append("        list.Add(item);\n");
                }
                else
                {
                    sb.Append("        var item = new ").Append(elementTypeName).Append("();\n");
                     var props = GetProperties(comp, model.ElementType);
                     int i = 0;
                    foreach (var p in props)
                    {
                        sb.Append("        item.").Append(p.Name).Append(" = reader.").Append(ReaderFor(p.Type)).Append("(").Append(i++).Append(");\n");
                    }
                    sb.Append("        list.Add(item);\n");
                }
                sb.Append("    }\n    return list;\n");
            }
            }
        }
        else
        {
            if (model.IsDelete || model.IsUpdate)
            {
                sb.Append("    return cmd.ExecuteNonQuery();\n");
            }
            else
            {
            // Sync version
            sb.Append("    using var reader = cmd.ExecuteReader();\n");
             if (model.Aggregation is not null)
            {
                if (model.Aggregation.Kind == AggregationKind.Count)
                {
                    sb.Append("    if (reader.Read()) return reader.GetInt32(0);\n    return 0;\n");
                }
                 else if (model.Aggregation.Kind == AggregationKind.Exists || model.Aggregation.Kind == AggregationKind.NotExists)
                {
                     if (model.Aggregation.Kind == AggregationKind.NotExists)
                         sb.Append("    if (reader.Read()) return reader.GetInt32(0) == 0;\n    return true;\n");
                    else
                        sb.Append("    if (reader.Read()) return reader.GetInt32(0) == 1;\n    return false;\n");
                }
                else
                {
                     string readerMethod;
                     if (model.Aggregation.Kind == AggregationKind.Average)
                     {
                         var pt = model.Aggregation.Property?.Type ?? model.ElementType;
                         readerMethod = pt.SpecialType == SpecialType.System_Decimal ? "GetDecimal" : "GetDouble";
                     }
                     else
                     {
                         readerMethod = ReaderFor(model.Aggregation.Property!.Type);
                     }
                     sb.Append("    if (reader.Read()) return reader.").Append(readerMethod).Append("(0);\n    return default;\n");
                }
            }
            else if (model.IsFirstOrDefault)
            {
                 sb.Append("    if (reader.Read()) {\n");
                if (model.Projection is not null)
                {
                     sb.Append("        var item = new ").Append(model.Projection.TypeName).Append("();\n");
                     for(int i=0; i<model.Projection.Entries.Count; i++)
                     {
                         var e = model.Projection.Entries[i];
                         var name = e.Alias ?? (e.Kind == ProjectionEntryKind.Property || e.Kind == ProjectionEntryKind.GroupKey ? (e.Property?.Name ?? (e.Aggregator ?? "Col")) : (e.AggregatorProperty is null ? (e.Aggregator ?? "Col") : (e.Aggregator + "_" + e.AggregatorProperty.Name)));
                         var t = e.Type ?? e.Property!.Type;
                         sb.Append("        item.").Append(name).Append(" = reader.IsDBNull(").Append(i).Append(") ? default : reader.").Append(ReaderFor(t)).Append("(").Append(i).Append(");\n");
                     }
                     sb.Append("        return item;\n    }\n    return default;\n");
                }
                else
                {
                    sb.Append("        var item = new ").Append(elementTypeName).Append("();\n");
                    var props = GetProperties(comp, model.ElementType);
                    int i = 0;
                    foreach (var p in props)
                    {
                        sb.Append("        item.").Append(p.Name).Append(" = reader.").Append(ReaderFor(p.Type)).Append("(").Append(i++).Append(");\n");
                    }
                     sb.Append("        return item;\n    }\n    return default;\n");
                }
            }
            else
            {
                sb.Append("    var list = new System.Collections.Generic.List<").Append(elementTypeName).Append(">();\n");
                sb.Append("    while (reader.Read()) {\n");
                 if (model.Projection is not null)
                {
                     sb.Append("        var item = new ").Append(model.Projection.TypeName).Append("();\n");
                     for(int i=0; i<model.Projection.Entries.Count; i++)
                     {
                         var e = model.Projection.Entries[i];
                         var name = e.Alias ?? (e.Kind == ProjectionEntryKind.Property || e.Kind == ProjectionEntryKind.GroupKey ? (e.Property?.Name ?? (e.Aggregator ?? "Col")) : (e.AggregatorProperty is null ? (e.Aggregator ?? "Col") : (e.Aggregator + "_" + e.AggregatorProperty.Name)));
                         var t = e.Type ?? e.Property!.Type;
                         sb.Append("        item.").Append(name).Append(" = reader.IsDBNull(").Append(i).Append(") ? default : reader.").Append(ReaderFor(t)).Append("(").Append(i).Append(");\n");
                     }
                     sb.Append("        list.Add(item);\n");
                }
                else
                {
                    sb.Append("        var item = new ").Append(elementTypeName).Append("();\n");
                    var props = GetProperties(comp, model.ElementType);
                    int i = 0;
                    foreach (var p in props)
                    {
                        sb.Append("        item.").Append(p.Name).Append(" = reader.").Append(ReaderFor(p.Type)).Append("(").Append(i++).Append(");\n");
                    }
                    sb.Append("        list.Add(item);\n");
                }
                sb.Append("    }\n    return list;\n");
            }
            }
        }

        sb.Append("}\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    static IEnumerable<IPropertySymbol> GetProperties(Compilation comp, ITypeSymbol t)
    {
        foreach (var m in t.GetMembers())
        {
            if (m is IPropertySymbol p)
            {
                if (IsScalarType(p.Type)) yield return p;
            }
        }
    }

    static string ColumnName(IPropertySymbol p)
    {
        foreach (var a in p.GetAttributes())
        {
            if (a.AttributeClass?.ToDisplayString() == "FastORM.ColumnAttribute")
            {
                if (a.ConstructorArguments.Length == 1)
                {
                    var v = a.ConstructorArguments[0].Value as string;
                    if (v != null) return v;
                }
            }
        }
        return p.Name;
    }

    static string ReaderFor(ITypeSymbol t)
    {
        switch (t.SpecialType)
        {
            case SpecialType.System_Int32: return "GetInt32";
            case SpecialType.System_Int64: return "GetInt64";
            case SpecialType.System_String: return "GetString";
            case SpecialType.System_Boolean: return "GetBoolean";
            case SpecialType.System_DateTime: return "GetDateTime";
            case SpecialType.System_Decimal: return "GetDecimal";
            case SpecialType.System_Double: return "GetDouble";
            case SpecialType.System_Single: return "GetFloat";
        }
        return "GetValue";
    }

    static bool IsScalarType(ITypeSymbol t)
    {
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
                return false;
        }
    }

    private enum GeneratorDialect { SqlServer, MySql, PostgreSql, Sqlite }

    private static string Quote(string id, GeneratorDialect d)
    {
        switch (d)
        {
            case GeneratorDialect.SqlServer: return "[" + id + "]";
            case GeneratorDialect.MySql: return "`" + id + "`";
            case GeneratorDialect.PostgreSql: return "\"" + id.ToLowerInvariant() + "\"";
            default: return "\"" + id + "\"";
        }
    }

    private static string BuildSql(QueryModel model, Compilation comp, GeneratorDialect dialect)
    {
        var sb = new StringBuilder();
        Func<string, string> quote = (s) => Quote(s, dialect);
        
        var whereBuilder = new StringBuilder();
        
        var allPredicates = new List<PredicateModel>(model.Predicates);
        if (model.Aggregation?.FilterPredicate != null)
        {
            allPredicates.Add(model.Aggregation.FilterPredicate);
        }

        if (allPredicates.Count > 0)
        {
             bool isFirst = true;
             foreach(var p in allPredicates)
             {
                 bool negate = model.Aggregation?.NegateFilter == true && p == model.Aggregation.FilterPredicate;
                 if (!isFirst) whereBuilder.Append(" AND ");
                 else isFirst = false;
                 if (negate) whereBuilder.Append("NOT (");
                 
                 if (p.Kind == PredicateKind.Binary)
                 {
                     whereBuilder.Append(quote(ColumnName(p.Left!))).Append(" ");
                     if (p.Operator == "==") whereBuilder.Append("=");
                     else if (p.Operator == "!=") whereBuilder.Append("<>");
                     else if (p.Operator == "&&") whereBuilder.Append("AND");
                     else if (p.Operator == "||") whereBuilder.Append("OR");
                     else whereBuilder.Append(p.Operator);
                     whereBuilder.Append(" ");
                     if (p.RightConstant != null)
                     {
                         if (p.RightConstant is string s) whereBuilder.Append("'").Append(s.Replace("'", "''")).Append("'");
                         else if (p.RightConstant is bool b) whereBuilder.Append(b ? "1" : "0");
                         else whereBuilder.Append(p.RightConstant);
                     }
                     else
                     {
                         whereBuilder.Append("@p").Append(p.ParameterIndex);
                     }
                 }
                 else if (p.Kind == PredicateKind.Like)
                 {
                     whereBuilder.Append(quote(ColumnName(p.Left!))).Append(" LIKE ");
                     if (p.RightConstant is string s) whereBuilder.Append("'").Append(s.Replace("'", "''")).Append("'");
                     else whereBuilder.Append("@p").Append(p.ParameterIndex);
                 }
                 else if (p.Kind == PredicateKind.IsNull)
                 {
                     whereBuilder.Append(quote(ColumnName(p.Left!))).Append(" IS NULL");
                 }
                 else if (p.Kind == PredicateKind.IsNotNull)
                 {
                     whereBuilder.Append(quote(ColumnName(p.Left!))).Append(" IS NOT NULL");
                 }
                 
                 if (negate) whereBuilder.Append(")");
             }
        }

        if (model.IsDelete)
        {
            sb.Append("DELETE FROM ").Append(quote(model.TableName));
            if (whereBuilder.Length > 0) sb.Append(" WHERE ").Append(whereBuilder);
        }
        else if (model.IsUpdate)
        {
            sb.Append("UPDATE ").Append(quote(model.TableName)).Append(" SET ");
            for (int i = 0; i < model.Updates.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(quote(model.Updates[i].Column)).Append(" = @p_u_").Append(i);
            }
            if (whereBuilder.Length > 0) sb.Append(" WHERE ").Append(whereBuilder);
        }
        else if (model.Aggregation is not null && (model.Aggregation.Kind == AggregationKind.Exists || model.Aggregation.Kind == AggregationKind.NotExists))
        {
             sb.Append("SELECT CASE WHEN EXISTS (SELECT 1 FROM ").Append(quote(model.TableName));
             if (model.Join is not null && model.Join.OuterKey is not null && model.Join.InnerKey is not null)
             {
                 var joinType = model.Join.Kind == JoinKind.Left ? " LEFT JOIN " : (model.Join.Kind == JoinKind.Right ? " RIGHT JOIN " : " INNER JOIN ");
                 sb.Append(joinType).Append(quote(model.Join.InnerTable)).Append(" ON ");
                 sb.Append(quote(model.TableName)).Append(".").Append(quote(ColumnName(model.Join.OuterKey)));
                 sb.Append(" = ");
                 sb.Append(quote(model.Join.InnerTable)).Append(".").Append(quote(ColumnName(model.Join.InnerKey)));
             }
             if (whereBuilder.Length > 0) sb.Append(" WHERE ").Append(whereBuilder);
             sb.Append(") THEN 1 ELSE 0 END");
        }
        else
        {
             sb.Append("SELECT ");
             if (model.IsDistinct) sb.Append("DISTINCT ");
             
             if (model.TakeCount.HasValue && !model.SkipCount.HasValue && dialect == GeneratorDialect.SqlServer)
             {
                 sb.Append("TOP ").Append(model.TakeCount.Value).Append(" ");
             }

             bool firstCol = true;
             Action<string> AddCol = (c) => {
                 if (!firstCol) sb.Append(", ");
                 sb.Append(c);
                 firstCol = false;
             };
             
            if (model.Aggregation is not null)
            {
                 if (model.Aggregation.Kind == AggregationKind.Count) AddCol("COUNT(*)");
                else if (model.Aggregation.Kind == AggregationKind.Max)
                {
                    string col = model.Aggregation.Property is not null ? quote(ColumnName(model.Aggregation.Property)) : "COL_AGG";
                    AddCol("MAX(" + col + ")");
                }
                else if (model.Aggregation.Kind == AggregationKind.Min)
                {
                    string col = model.Aggregation.Property is not null ? quote(ColumnName(model.Aggregation.Property)) : "COL_AGG";
                    AddCol("MIN(" + col + ")");
                }
                else if (model.Aggregation.Kind == AggregationKind.Average)
                {
                    string col = model.Aggregation.Property is not null ? quote(ColumnName(model.Aggregation.Property)) : "COL_AGG";
                    AddCol("AVG(" + col + ")");
                }
                 else if (model.Aggregation.Kind == AggregationKind.Sum)
                {
                    string col = model.Aggregation.Property is not null ? quote(ColumnName(model.Aggregation.Property)) : "COL_AGG";
                    AddCol("SUM(" + col + ")");
                }
            }
            else if (model.Projection is not null)
            {
                for (int i = 0; i < model.Projection.Entries.Count; i++)
                {
                    var e = model.Projection.Entries[i];
                    string colExpr = "";
                    if (e.Kind == ProjectionEntryKind.Property) colExpr = quote(ColumnName(e.Property!));
                    else if (e.Kind == ProjectionEntryKind.GroupKey)
                    {
                         if (e.Property?.Name == "Key" && model.GroupBy?.Keys.Count == 1) colExpr = quote(ColumnName(model.GroupBy.Keys[0]));
                         else colExpr = quote(ColumnName(e.Property!));
                    }
                    else
                    {
                        if (e.Aggregator == "COUNT") colExpr = "COUNT(*)";
                        else if (e.Aggregator == "MAX") colExpr = "MAX(" + quote(ColumnName(e.AggregatorProperty!)) + ")";
                        else if (e.Aggregator == "MIN") colExpr = "MIN(" + quote(ColumnName(e.AggregatorProperty!)) + ")";
                        else if (e.Aggregator == "SUM") colExpr = "SUM(" + quote(ColumnName(e.AggregatorProperty!)) + ")";
                        else if (e.Aggregator == "AVERAGE") colExpr = "AVG(" + quote(ColumnName(e.AggregatorProperty!)) + ")";
                    }
                    
                    if (!string.IsNullOrEmpty(e.Alias)) colExpr += " AS " + quote(e.Alias!);
                    AddCol(colExpr);
                }
            }
            else
            {
                var props = GetProperties(comp, model.ElementType);
                foreach (var p in props) AddCol(quote(ColumnName(p)));
            }

             sb.Append(" FROM ").Append(quote(model.TableName));

             if (model.Join is not null && model.Join.OuterKey is not null && model.Join.InnerKey is not null)
             {
                 var joinType = model.Join.Kind == JoinKind.Left ? " LEFT JOIN " : (model.Join.Kind == JoinKind.Right ? " RIGHT JOIN " : " INNER JOIN ");
                 sb.Append(joinType).Append(quote(model.Join.InnerTable)).Append(" ON ");
                 sb.Append(quote(model.TableName)).Append(".").Append(quote(ColumnName(model.Join.OuterKey)));
                 sb.Append(" = ");
                 sb.Append(quote(model.Join.InnerTable)).Append(".").Append(quote(ColumnName(model.Join.InnerKey)));
             }

             if (whereBuilder.Length > 0) sb.Append(" WHERE ").Append(whereBuilder);
             
              if (model.GroupBy != null && model.GroupBy.Keys.Count > 0)
              {
                  sb.Append(" GROUP BY ");
                  for(int i=0; i<model.GroupBy.Keys.Count; i++)
                  {
                      if (i > 0) sb.Append(", ");
                      sb.Append(quote(ColumnName(model.GroupBy.Keys[i])));
                  }
              }
              
              if (model.OrderBy != null && model.OrderBy.Count > 0)
              {
                  sb.Append(" ORDER BY ");
                  for(int i=0; i<model.OrderBy.Count; i++)
                  {
                      if (i > 0) sb.Append(", ");
                      sb.Append(quote(ColumnName(model.OrderBy[i].prop)));
                      if (model.OrderBy[i].desc) sb.Append(" DESC");
                  }
              }

             if (model.TakeCount.HasValue || model.SkipCount.HasValue)
             {
                 string take = model.TakeCount.HasValue ? model.TakeCount.Value.ToString() : "null";
                 string skip = model.SkipCount.HasValue ? model.SkipCount.Value.ToString() : "0";
                 
                 if (dialect == GeneratorDialect.SqlServer)
                 {
                     if (model.SkipCount.HasValue)
                     {
                          sb.Append(" OFFSET ").Append(skip).Append(" ROWS");
                          if (model.TakeCount.HasValue) sb.Append(" FETCH NEXT ").Append(take).Append(" ROWS ONLY");
                     }
                 }
                 else
                 {
                      if (model.TakeCount.HasValue) sb.Append(" LIMIT ").Append(take);
                      else if (model.SkipCount.HasValue)
                      {
                           if (dialect == GeneratorDialect.Sqlite) sb.Append(" LIMIT -1");
                      }
                      if (model.SkipCount.HasValue) sb.Append(" OFFSET ").Append(skip);
                 }
             }
        }
        return sb.ToString();
    }

    static void AssignParameterIndices(List<PredicateModel> preds, ref int paramCounter, ref int varCounter)
    {
        foreach(var p in preds)
        {
            if (p.Kind == PredicateKind.In || p.Kind == PredicateKind.NotIn || p.Kind == PredicateKind.LikeGroup)
            {
                p.VariableIndex = varCounter++;
            }
            else if (p.Kind == PredicateKind.Binary || p.Kind == PredicateKind.Like)
            {
                 p.ParameterIndex = paramCounter++;
            }
            AssignParameterIndices(p.Children, ref paramCounter, ref varCounter);
        }
    }
    
    static bool CheckIsDynamic(List<PredicateModel> preds)
    {
        foreach(var p in preds)
        {
            if (p.Kind == PredicateKind.In || p.Kind == PredicateKind.NotIn || p.Kind == PredicateKind.LikeGroup) return true;
            if (CheckIsDynamic(p.Children)) return true;
        }
        return false;
    }

    static void EmitPredicate(System.Text.StringBuilder sb, PredicateModel p, QueryModel model)
    {
        if (p.Kind == PredicateKind.And)
        {
            sb.Append("    whereBuilder.Append(\"(\");\n");
            bool first = true;
            foreach(var c in p.Children)
            {
                if (!first) sb.Append("    whereBuilder.Append(\" AND \");\n");
                else first = false;
                EmitPredicate(sb, c, model);
            }
            sb.Append("    whereBuilder.Append(\")\");\n");
            return;
        }
        if (p.Kind == PredicateKind.Or)
        {
            sb.Append("    whereBuilder.Append(\"(\");\n");
            bool first = true;
            foreach(var c in p.Children)
            {
                if (!first) sb.Append("    whereBuilder.Append(\" OR \");\n");
                else first = false;
                EmitPredicate(sb, c, model);
            }
            sb.Append("    whereBuilder.Append(\")\");\n");
            return;
        }

        // Leaf nodes
        if (p.Kind == PredicateKind.Binary)
        {
             sb.Append("    whereBuilder.Append(context.Quote(\"").Append(ColumnName(p.Left!)).Append("\")).Append(\" ");
             if (p.Operator == "==") sb.Append("=");
             else if (p.Operator == "!=") sb.Append("<>");
             else if (p.Operator == "&&") sb.Append("AND");
             else if (p.Operator == "||") sb.Append("OR");
             else sb.Append(p.Operator);
             sb.Append(" ");
             if (p.RightConstant != null)
             {
                 if (p.RightConstant is string s) sb.Append("'").Append(Escape(s)).Append("'");
                 else if (p.RightConstant is bool b) sb.Append(b ? "1" : "0");
                 else sb.Append(p.RightConstant);
             }
             else
             {
                 sb.Append("@p").Append(p.ParameterIndex);
             }
             sb.Append("\");\n");
        }
        else if (p.Kind == PredicateKind.Like)
        {
             sb.Append("    whereBuilder.Append(context.Quote(\"").Append(ColumnName(p.Left!)).Append("\")).Append(\" LIKE ");
             if (p.RightConstant is string s) sb.Append("'").Append(Escape(s)).Append("'");
             else sb.Append("@p").Append(p.ParameterIndex);
             sb.Append("\");\n");
        }
        else if (p.Kind == PredicateKind.IsNull)
        {
             sb.Append("    whereBuilder.Append(context.Quote(\"").Append(ColumnName(p.Left!)).Append("\")).Append(\" IS NULL\");\n");
        }
        else if (p.Kind == PredicateKind.IsNotNull)
        {
             sb.Append("    whereBuilder.Append(context.Quote(\"").Append(ColumnName(p.Left!)).Append("\")).Append(\" IS NOT NULL\");\n");
        }
        else if (p.Kind == PredicateKind.In || p.Kind == PredicateKind.NotIn)
        {
             int idx = p.VariableIndex;
             sb.Append("    var inVals_").Append(idx).Append(" = (").Append(p.CollectionExpressionCode).Append(").Cast<object>().ToList();\n");
             sb.Append("    var inParams_").Append(idx).Append(" = new System.Collections.Generic.List<string>();\n");
             sb.Append("    for(int i=0; i<inVals_").Append(idx).Append(".Count; i++) inParams_").Append(idx).Append(".Add(\"@p_in_").Append(idx).Append("_\" + i);\n");
             
             sb.Append("    if (inVals_").Append(idx).Append(".Count > 0) {\n");
             sb.Append("        whereBuilder.Append(context.Quote(\"").Append(ColumnName(p.Left!)).Append("\")).Append(\"").Append(p.Kind == PredicateKind.In ? " IN (" : " NOT IN (").Append("\" + string.Join(\",\", inParams_").Append(idx).Append(") + \")\");\n");
             sb.Append("    } else {\n");
             sb.Append("        whereBuilder.Append(\"").Append(p.Kind == PredicateKind.In ? " 0=1" : " 1=1").Append("\");\n");
             sb.Append("    }\n");
        }
        else if (p.Kind == PredicateKind.LikeGroup)
        {
             int idx = p.VariableIndex;
             sb.Append("    whereBuilder.Append(\"(\");\n");
             for(int i=0; i<p.LikeTerms.Count; i++)
             {
                 if (i > 0) sb.Append("    whereBuilder.Append(\" OR \");\n");
                 var term = p.LikeTerms[i];
                 sb.Append("    whereBuilder.Append(context.Quote(\"").Append(ColumnName(term.Left)).Append("\")).Append(\" LIKE \");\n");
                 
                 sb.Append("    var pat").Append(idx).Append("_").Append(i).Append(" = ").Append(term.PatternCode).Append(";\n");
                 string prefix = term.Kind == LikeKind.Contains || term.Kind == LikeKind.EndsWith ? "%" : "";
                 string suffix = term.Kind == LikeKind.Contains || term.Kind == LikeKind.StartsWith ? "%" : "";
                 sb.Append("    whereBuilder.Append(\"'\").Append(\"").Append(prefix).Append("\").Append(pat").Append(idx).Append("_").Append(i).Append(").Append(\"").Append(suffix).Append("'\");\n");
             }
             sb.Append("    whereBuilder.Append(\")\");\n");
        }
    }

    static void EmitParameters(System.Text.StringBuilder sb, PredicateModel p, ref int paramLocalIndex)
    {
        if (p.Kind == PredicateKind.And || p.Kind == PredicateKind.Or)
        {
            foreach(var c in p.Children) EmitParameters(sb, c, ref paramLocalIndex);
            return;
        }
        
        if (p.Kind == PredicateKind.Binary || p.Kind == PredicateKind.Like)
        {
             if (p.RightConstant == null)
             {
                 int idx = paramLocalIndex++;
                 sb.Append("    var p").Append(idx).Append(" = cmd.CreateParameter();\n");
                 sb.Append("    p").Append(idx).Append(".ParameterName = \"@p").Append(p.ParameterIndex).Append("\";\n");
                 
                 // Use Runtime Parameter if available, else fallback to code
                 // Note: Since we now always extract runtime values, we can use the index.
                 // However, static fields are NOT captured by ValueExtractor (it captures values from the Expression Tree).
                 // If the code is "p.Id == 5", ValueExtractor returns 5.
                 // If "p.Id == id", ValueExtractor returns value of id.
                 // If "p.Id == StaticClass.Field", ValueExtractor returns value of field.
                 // So we can ALWAYS use runtimeParams_[idx].
                 // BUT, we must ensure index alignment.
                 
                 sb.Append("    p").Append(idx).Append(".Value = (object)runtimeParams_[").Append(p.ParameterIndex).Append("] ?? DBNull.Value;\n");
                 sb.Append("    cmd.Parameters.Add(p").Append(idx).Append(");\n");
             }
        }
        else if (p.Kind == PredicateKind.In || p.Kind == PredicateKind.NotIn)
        {
             int idx = p.VariableIndex;
             sb.Append("    if (inVals_").Append(idx).Append(".Count > 0) {\n");
             sb.Append("        for(int i=0; i<inVals_").Append(idx).Append(".Count; i++) {\n");
             sb.Append("            var pVal = cmd.CreateParameter();\n");
             sb.Append("            pVal.ParameterName = \"@p_in_").Append(idx).Append("_\" + i;\n");
             sb.Append("            pVal.Value = inVals_").Append(idx).Append("[i] ?? DBNull.Value;\n");
             sb.Append("            cmd.Parameters.Add(pVal);\n");
             sb.Append("        }\n");
             sb.Append("    }\n");
        }
    }
}

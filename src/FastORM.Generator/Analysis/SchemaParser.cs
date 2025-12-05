using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace FastORM.Generator;

internal static class SchemaParser
{
    public static SchemaModel? Transform(GeneratorSyntaxContext ctx, CancellationToken _)
    {
        if (ctx.Node is not InvocationExpressionSyntax inv) return null;
        if (inv.Expression is not MemberAccessExpressionSyntax ma) return null;
        
        var symbol = ctx.SemanticModel.GetSymbolInfo(ma).Symbol as IMethodSymbol;
        if (symbol is null) return null;

        if (symbol.Name != "CreateTable" && symbol.Name != "CreateTableAsync" && 
            symbol.Name != "DropTable" && symbol.Name != "DropTableAsync") return null;
            
        var model = new SchemaModel();
        model.FilePath = inv.SyntaxTree.FilePath;
        var nameLoc = ma.Name.GetLocation().GetLineSpan();
        model.Line = nameLoc.StartLinePosition.Line + 1;
        model.Column = nameLoc.StartLinePosition.Character + 1;

        if (symbol.Name.EndsWith("Async")) model.IsAsync = true;

        if (symbol.Name.StartsWith("CreateTable"))
        {
            model.IsCreateTable = true;
            if (symbol.TypeArguments.Length > 0)
            {
                model.ElementType = symbol.TypeArguments[0];
                model.TableName = MetadataHelper.GetTableName(model.ElementType);
                
                var props = model.ElementType.GetMembers().OfType<IPropertySymbol>()
                   .Where(p => p.SetMethod != null && p.GetMethod != null && MetadataHelper.IsScalar(p.Type) 
                        && p.GetAttributes().All(a => a.AttributeClass?.ToDisplayString() != "FastORM.NavigationAttribute" 
                            && a.AttributeClass?.ToDisplayString() != "System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute"))
                   .ToList();
                
                model.PrimaryKey = MetadataHelper.GetPrimaryKey(model.ElementType);
                model.Columns.AddRange(props);

                // Parse Column Definitions
                foreach (var p in props)
                {
                    var def = new ColumnDefinition();
                    foreach (var a in p.GetAttributes())
                    {
                        var attrName = a.AttributeClass?.ToDisplayString();
                        if (attrName == "System.ComponentModel.DataAnnotations.StringLengthAttribute" || 
                            attrName == "System.ComponentModel.DataAnnotations.MaxLengthAttribute")
                        {
                            if (a.ConstructorArguments.Length > 0 && a.ConstructorArguments[0].Value is int len)
                                def.MaxLength = len;
                        }
                        else if (attrName == "System.ComponentModel.DataAnnotations.RequiredAttribute")
                        {
                            def.IsRequired = true;
                        }
                        else if (attrName == "FastORM.DefaultValueSqlAttribute")
                {
                     if (a.ConstructorArguments.Length > 0 && a.ConstructorArguments[0].Value is string sql)
                        def.DefaultValueSql = sql;
                }
                else if (attrName == "FastORM.PrecisionAttribute")
                {
                    if (a.ConstructorArguments.Length >= 2 && 
                        a.ConstructorArguments[0].Value is int prec && 
                        a.ConstructorArguments[1].Value is int scale)
                    {
                        def.Precision = prec;
                        def.Scale = scale;
                    }
                }
                else if (attrName == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute")
                        {
                            foreach (var na in a.NamedArguments)
                            {
                                if (na.Key == "TypeName") def.CustomTypeName = na.Value.Value as string;
                            }
                        }
                    }
                    
                    // Infer required for value types
                    if (!def.IsRequired && p.Type.IsValueType && p.Type.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
                    {
                        def.IsRequired = true;
                    }

                    model.ColumnDefinitions[p.Name] = def;
                }

                // Parse Indexes
                foreach (var a in model.ElementType.GetAttributes())
                {
                    if (a.AttributeClass?.ToDisplayString() == "FastORM.IndexAttribute")
                    {
                        var indexModel = new IndexModel();
                        if (a.ConstructorArguments.Length > 0 && !a.ConstructorArguments[0].IsNull)
                        {
                            var args = a.ConstructorArguments[0].Values;
                            indexModel.Columns = args.Select(v => v.Value as string).Where(s => s != null).Cast<string>().ToArray();
                        }
                        
                        foreach (var na in a.NamedArguments)
                        {
                            if (na.Key == "Name") indexModel.Name = na.Value.Value as string;
                            else if (na.Key == "IsUnique") indexModel.IsUnique = (bool)(na.Value.Value ?? false);
                        }

                        if (indexModel.Columns.Length > 0)
                        {
                            model.Indexes.Add(indexModel);
                        }
                    }
                }
            }
        }
        else if (symbol.Name.StartsWith("DropTable"))
        {
            model.IsDropTable = true;
            if (symbol.TypeArguments.Length > 0)
            {
                model.ElementType = symbol.TypeArguments[0];
                model.TableName = MetadataHelper.GetTableName(model.ElementType);
            }
        }

        return model;
    }
}

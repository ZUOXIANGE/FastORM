using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FastORM.Generator;

internal static class QueryParser
{
    public static QueryModel? Transform(GeneratorSyntaxContext ctx, CancellationToken _)
    {
        var inv = (InvocationExpressionSyntax)ctx.Node;
        var ma = (MemberAccessExpressionSyntax)inv.Expression;
        var symbol = ctx.SemanticModel.GetSymbolInfo(ma).Symbol as IMethodSymbol;
        if (symbol is null) 
        {
             var errModel = new QueryModel();
             errModel.FilePath = inv.SyntaxTree.FilePath;
             return Fail(errModel, Diagnostics.GeneratorFailure, "Symbol resolution failed for " + ma.ToString());
        }
        if (symbol.Name != "ToList" && symbol.Name != "ToListAsync" && symbol.Name != "FirstOrDefault" && symbol.Name != "FirstOrDefaultAsync" && symbol.Name != "Count" && symbol.Name != "CountAsync" && symbol.Name != "Max" && symbol.Name != "MaxAsync" && symbol.Name != "Min" && symbol.Name != "MinAsync" && symbol.Name != "Average" && symbol.Name != "AverageAsync" && symbol.Name != "Sum" && symbol.Name != "SumAsync" && symbol.Name != "Any" && symbol.Name != "AnyAsync" && symbol.Name != "All" && symbol.Name != "AllAsync" && symbol.Name != "Delete" && symbol.Name != "DeleteAsync" && symbol.Name != "Update" && symbol.Name != "UpdateAsync" && symbol.Name != "Insert" && symbol.Name != "InsertAsync") return null;
        if ((symbol.Name == "Count" || symbol.Name == "CountAsync" || symbol.Name == "Max" || symbol.Name == "MaxAsync" || symbol.Name == "Min" || symbol.Name == "MinAsync" || symbol.Name == "Average" || symbol.Name == "AverageAsync" || symbol.Name == "Sum" || symbol.Name == "SumAsync") && (symbol.ContainingType?.Name == "GroupExtensions" || symbol.ContainingType?.Name == "GroupExtensionsAsync")) return null;
        
        var qModel = new QueryModel();
        var sm = ctx.SemanticModel;
        ITypeSymbol? paramType = null;
        if (symbol.IsExtensionMethod && symbol.ReducedFrom != null)
        {
             paramType = symbol.ReceiverType;
        }
        else if (symbol.Parameters.Length > 0)
        {
             paramType = symbol.Parameters[0].Type;
        }

        if (paramType != null && (paramType.Name.Contains("IQueryable") || paramType.MetadataName.Contains("IQueryable")))
        {
            qModel.EndOnIQueryable = true;
        }

        // Fix: Only intercept query methods if they are on CompilableQuery, IQueryable or Group
        if (!symbol.Name.StartsWith("Insert") && !symbol.Name.StartsWith("Update") && !symbol.Name.StartsWith("Delete"))
        {
             if (paramType != null)
             {
                  var typeName = paramType.Name;
                  var ns = paramType.ContainingNamespace?.ToDisplayString();
                  
                  bool isValid = typeName.Contains("IQueryable") || 
                                 paramType.AllInterfaces.Any(i => i.Name.Contains("IQueryable")) ||
                                 (typeName == "CompilableQuery" && ns == "FastORM") ||
                                 (typeName == "Group" && ns == "FastORM");
                                 
                  if (!isValid) return null;
             }
        }

        if (symbol.Name.EndsWith("Async"))
        {
            qModel.IsAsync = true;
        }

        if (symbol.Name.StartsWith("FirstOrDefault"))
        {
            qModel.IsFirstOrDefault = true;
        }
        else if (symbol.Name.StartsWith("Delete"))
        {
            qModel.IsDelete = true;
            if (inv.ArgumentList.Arguments.Count > 0)
            {
                 var argExpr = inv.ArgumentList.Arguments[0].Expression;
                 var argType = sm.GetTypeInfo(argExpr).Type;
                 if (argType != null)
                 {
                      // Check if argument is entity or list (not predicate/lambda)
                      if (!argType.Name.Contains("Expression")) 
                      {
                           qModel.DeleteParameterType = symbol.Parameters.Length > 0 ? symbol.Parameters[0].Type : argType;
                           if (argType.Name == "IEnumerable" || argType.AllInterfaces.Any(i => i.Name == "IEnumerable" && i.IsGenericType))
                           {
                               qModel.DeleteIsBatch = true;
                               if (argType is INamedTypeSymbol nts && nts.IsGenericType) qModel.ElementType = nts.TypeArguments[0];
                               else if (argType is IArrayTypeSymbol ats) qModel.ElementType = ats.ElementType;
                               else 
                               {
                                   var enumInterface = argType.AllInterfaces.FirstOrDefault(i => i.Name == "IEnumerable" && i.IsGenericType);
                                   if (enumInterface != null) qModel.ElementType = enumInterface.TypeArguments[0];
                               }
                           }
                           else
                           {
                               qModel.DeleteIsEntity = true;
                               qModel.ElementType = argType;
                           }

                           if (qModel.ElementType != null && string.IsNullOrEmpty(qModel.TableName))
                           {
                               qModel.TableName = GetTableName(qModel.ElementType);
                           }
                      }
                 }
            }
        }
        else if (symbol.Name.StartsWith("Insert"))
        {
            qModel.IsInsert = true;
            
            // Check for batch insert (IEnumerable<T>)
            var parameters = symbol.Parameters;
            if (parameters.Length > 0)
            {
                var insertParamType = parameters[0].Type;
                qModel.InsertParameterType = insertParamType;
                if (insertParamType is INamedTypeSymbol namedParamType)
                {
                     // Check if it's IEnumerable<T>
                     if (namedParamType.Name == "IEnumerable" || namedParamType.AllInterfaces.Any(i => i.Name == "IEnumerable" && i.IsGenericType))
                     {
                         qModel.InsertIsBatch = true;
                         if (namedParamType.IsGenericType)
                         {
                             qModel.ElementType = namedParamType.TypeArguments[0];
                         }
                         else
                         {
                             // Handle derived types implementing IEnumerable<T>
                             var enumInterface = namedParamType.AllInterfaces.FirstOrDefault(i => i.Name == "IEnumerable" && i.IsGenericType);
                             if (enumInterface != null)
                             {
                                 qModel.ElementType = enumInterface.TypeArguments[0];
                             }
                         }
                     }
                     else
                     {
                         // Single entity
                         qModel.ElementType = insertParamType;
                     }
                }
                else if (insertParamType is IArrayTypeSymbol arrayType)
                {
                    qModel.InsertIsBatch = true;
                    qModel.ElementType = arrayType.ElementType;
                }
            }

            if (qModel.ElementType != null)
            {
                qModel.TableName = GetTableName(qModel.ElementType);
                // Get properties
                var props = qModel.ElementType.GetMembers().OfType<IPropertySymbol>()
                   .Where(p => p.SetMethod != null && p.GetMethod != null && IsScalar(p.Type) && p.GetAttributes().All(a => a.AttributeClass?.ToDisplayString() != "FastORM.NavigationAttribute"))
                   .ToList();
                qModel.InsertProperties.AddRange(props);
            }
        }
        else if (symbol.Name.StartsWith("Update"))
        {
            qModel.IsUpdate = true;
            if (inv.ArgumentList.Arguments.Count > 0)
            {
                 var argExpr = inv.ArgumentList.Arguments[0].Expression;
                 var arg = argExpr as LambdaExpressionSyntax;
                 if (arg != null)
                 {
                      string? paramName = null;
                      if (arg is SimpleLambdaExpressionSyntax sl) paramName = sl.Parameter.Identifier.Text;
                      else if (arg is ParenthesizedLambdaExpressionSyntax pl && pl.ParameterList.Parameters.Count > 0) paramName = pl.ParameterList.Parameters[0].Identifier.Text;

                      if (arg.Body is BlockSyntax block)
                      {
                          foreach(var stmt in block.Statements)
                          {
                              if (stmt is ExpressionStatementSyntax exprStmt && exprStmt.Expression is AssignmentExpressionSyntax assign)
                              {
                                  if (assign.Left is MemberAccessExpressionSyntax maProp)
                                  {
                                      var propSym = sm.GetSymbolInfo(maProp).Symbol as IPropertySymbol;
                                      if (propSym != null)
                                      {
                                          qModel.Updates.Add((propSym.Name, assign.Right.ToString(), IsDependent(assign.Right, paramName)));
                                      }
                                  }
                              }
                          }
                      }
                      else if (arg.Body is AssignmentExpressionSyntax assign)
                      {
                           if (assign.Left is MemberAccessExpressionSyntax maProp)
                           {
                               var propSym = sm.GetSymbolInfo(maProp).Symbol as IPropertySymbol;
                               if (propSym != null)
                               {
                                   qModel.Updates.Add((propSym.Name, assign.Right.ToString(), IsDependent(assign.Right, paramName)));
                               }
                           }
                      }
                 }
                 else
                 {
                      // Entity update
                      var argType = sm.GetTypeInfo(argExpr).Type;
                      if (argType != null)
                      {
                           qModel.UpdateParameterType = symbol.Parameters.Length > 0 ? symbol.Parameters[0].Type : argType;
                           if (argType.Name == "IEnumerable" || argType.AllInterfaces.Any(i => i.Name == "IEnumerable" && i.IsGenericType))
                           {
                               qModel.UpdateIsBatch = true;
                               if (argType is INamedTypeSymbol nts && nts.IsGenericType) qModel.ElementType = nts.TypeArguments[0];
                               else if (argType is IArrayTypeSymbol ats) qModel.ElementType = ats.ElementType;
                               else 
                               {
                                   var enumInterface = argType.AllInterfaces.FirstOrDefault(i => i.Name == "IEnumerable" && i.IsGenericType);
                                   if (enumInterface != null) qModel.ElementType = enumInterface.TypeArguments[0];
                               }
                           }
                           else
                           {
                               qModel.UpdateIsEntity = true;
                               qModel.ElementType = argType;
                           }

                           if (qModel.ElementType != null && string.IsNullOrEmpty(qModel.TableName))
                           {
                               qModel.TableName = GetTableName(qModel.ElementType);
                           }
                      }
                 }
            }
        }
        else if (symbol.Name.StartsWith("Count"))
        {
            qModel.Aggregation = new AggregationModel { Kind = AggregationKind.Count };
            if (symbol.ContainingType?.Name == "GroupExtensions" || symbol.ContainingType?.Name == "GroupExtensionsAsync")
            {
                 qModel.AggregationReceiver = AggregationReceiverKind.Group;
            }
        }
        else if (symbol.Name.StartsWith("Any"))
        {
             qModel.Aggregation = new AggregationModel { Kind = AggregationKind.Exists };
             if (inv.ArgumentList.Arguments.Count > 0)
             {
                  var arg = inv.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                  if (arg != null && arg.Body is ExpressionSyntax body)
             {
                  var p = ParsePredicate(body, qModel, sm);
                  if (p != null) qModel.Aggregation.FilterPredicate = p;
             }
             }
        }
        else if (symbol.Name.StartsWith("All"))
        {
             qModel.Aggregation = new AggregationModel { Kind = AggregationKind.NotExists, NegateFilter = true };
             if (inv.ArgumentList.Arguments.Count > 0)
             {
                  var arg = inv.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                  if (arg != null && arg.Body is ExpressionSyntax body)
             {
                  var p = ParsePredicate(body, qModel, sm);
                  if (p != null) qModel.Aggregation.FilterPredicate = p;
             }
             }
        }
        else if (symbol.Name.StartsWith("Max") || symbol.Name.StartsWith("Min") || symbol.Name.StartsWith("Sum") || symbol.Name.StartsWith("Average"))
        {
            AggregationKind kind = AggregationKind.Max;
            if (symbol.Name.StartsWith("Min")) kind = AggregationKind.Min;
            else if (symbol.Name.StartsWith("Sum")) kind = AggregationKind.Sum;
            else if (symbol.Name.StartsWith("Average")) kind = AggregationKind.Average;

            qModel.Aggregation = new AggregationModel { Kind = kind };
             
            if (symbol.ContainingType?.Name == "GroupExtensions" || symbol.ContainingType?.Name == "GroupExtensionsAsync")
            {
                 qModel.AggregationReceiver = AggregationReceiverKind.Group;
            }

            if (inv.ArgumentList.Arguments.Count > 0)
            {
                 var arg = inv.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                 if (arg != null && arg.Body is MemberAccessExpressionSyntax maSel)
                 {
                      var propSym = sm.GetSymbolInfo(maSel).Symbol as IPropertySymbol;
                      if (propSym != null)
                      {
                           qModel.Aggregation.Property = propSym;
                      }
                 }
            }
        }
        
        qModel.FilePath = inv.SyntaxTree.FilePath;
        var nameLoc = ((MemberAccessExpressionSyntax)inv.Expression).Name.GetLocation().GetLineSpan();
        qModel.Line = nameLoc.StartLinePosition.Line + 1;
        qModel.Column = nameLoc.StartLinePosition.Character + 1;
        // var sm = ctx.SemanticModel;
        try
        {
            var loc = sm.GetInterceptableLocation(inv, default);
            if (loc != null)
            {
                var data = loc.Data;
                if (loc.Version != 0 && !string.IsNullOrEmpty(data))
                {
                    qModel.InterceptVersion = loc.Version;
                    qModel.InterceptData = data!;
                }
            }
        }
        catch { }

        var current = inv;
        while (current != null)
        {
            if (current.Expression is MemberAccessExpressionSyntax member)
            {
                var method = sm.GetSymbolInfo(member).Symbol as IMethodSymbol;
                var name = method?.Name;
                if (name == "Where")
                {
                    var argExpr = current.ArgumentList.Arguments[0].Expression;
                    if (argExpr is LambdaExpressionSyntax arg)
                    {
                        if (arg.Modifiers.ToString().IndexOf("static", StringComparison.Ordinal) < 0) 
                        { 
                            // REMOVED LIMITATION: Allow non-static lambdas for runtime parameter extraction
                            // return Fail(qModel, Diagnostics.NonStaticLambda); 
                        }
                        
                        if (arg.Body is ExpressionSyntax bodyExpr)
                        {
                            var p = ParsePredicate(bodyExpr, qModel, sm);
                            if (p != null)
                            {
                                if (p.Kind == PredicateKind.And) qModel.Predicates.AddRange(p.Children);
                                else qModel.Predicates.Add(p);
                            }
                        }
                        else
                        {
                            return Fail(qModel, Diagnostics.UnsupportedExpression, arg.Body.ToString());
                        }
                    }
                    else
                    {
                        qModel.DynamicPredicates.Add(argExpr.ToString());
                    }
                }
                else if (name == "Take")
                {
                     var arg = current.ArgumentList.Arguments[0].Expression;
                     var val = sm.GetConstantValue(arg);
                     if (val.HasValue && val.Value is int i) qModel.TakeCount = i;
                }
                else if (name == "Skip")
                {
                     var arg = current.ArgumentList.Arguments[0].Expression;
                     var val = sm.GetConstantValue(arg);
                     if (val.HasValue && val.Value is int i) qModel.SkipCount = i;
                }
                else if (name == "OrderBy" || name == "OrderByDescending" || name == "ThenBy" || name == "ThenByDescending")
                {
                     var arg = current.ArgumentList.Arguments[0].Expression as SimpleLambdaExpressionSyntax;
                     if (arg is null) { return Fail(qModel, Diagnostics.UnsupportedExpression, current.ToString()); }
                     // if (arg.Modifiers.ToString().IndexOf("static", StringComparison.Ordinal) < 0) { return Fail(qModel, Diagnostics.NonStaticLambda); }
                     if (arg.Body is MemberAccessExpressionSyntax maOrder)
                     {
                         var propSym = sm.GetSymbolInfo(maOrder).Symbol as IPropertySymbol;
                         if (propSym is null) { return Fail(qModel, Diagnostics.UnsupportedExpression, maOrder.ToString()); }
                         qModel.OrderBy.Insert(0, (propSym, name == "OrderByDescending" || name == "ThenByDescending"));
                     }
                }
                 else if (name == "Select")
                {
                    var arg = current.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                    if (arg is null) { return Fail(qModel, Diagnostics.UnsupportedExpression, current.ToString()); }
                    // bool isStatic = false;
                    // foreach(var m in arg.Modifiers) { if (m.IsKind(SyntaxKind.StaticKeyword)) isStatic = true; }
                    // if (!isStatic) { return Fail(qModel, Diagnostics.NonStaticLambda); }
                    
                    ParseProjection(arg, qModel, sm);
                }
                else if (name == "GroupBy")
                {
                     qModel.GroupBy = new GroupByModel();
                     var arg = current.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
                     if (arg != null && arg.Body is ExpressionSyntax bodyExpr)
                     {
                         var body = Unwrap(bodyExpr);
                         if (body is MemberAccessExpressionSyntax maGroup)
                         {
                             var propSym = sm.GetSymbolInfo(maGroup).Symbol as IPropertySymbol;
                             if (propSym != null) qModel.GroupBy.Keys.Add(propSym);
                         }
                         else if (body is AnonymousObjectCreationExpressionSyntax anon)
                         {
                             foreach(var init in anon.Initializers)
                             {
                                 if (init.Expression is MemberAccessExpressionSyntax maInit)
                                 {
                                     var propSym = sm.GetSymbolInfo(maInit).Symbol as IPropertySymbol;
                                     if (propSym != null) qModel.GroupBy.Keys.Add(propSym);
                                 }
                             }
                         }
                     }
                }
                else if (name == "Join" || name == "LeftJoin" || name == "RightJoin")
                {
                    if (current.ArgumentList.Arguments.Count >= 4)
                    {
                        var innerArg = current.ArgumentList.Arguments[0].Expression;
                        var outerKeyArg = current.ArgumentList.Arguments[1].Expression as LambdaExpressionSyntax;
                        var innerKeyArg = current.ArgumentList.Arguments[2].Expression as LambdaExpressionSyntax;
                        var resultSelectorArg = current.ArgumentList.Arguments[3].Expression as LambdaExpressionSyntax;
                        
                        var innerType = sm.GetTypeInfo(innerArg).Type as INamedTypeSymbol;
                        if (innerType != null && innerType.TypeArguments.Length > 0)
                        {
                             var innerEntityType = innerType.TypeArguments[0];
                             var join = new JoinModel 
                             { 
                                 InnerType = innerEntityType,
                                 InnerTable = GetTableName(innerEntityType),
                                 Kind = name == "LeftJoin" ? JoinKind.Left : (name == "RightJoin" ? JoinKind.Right : JoinKind.Inner)
                             };
                             
                             if (outerKeyArg?.Body is MemberAccessExpressionSyntax outerMa)
                             {
                                 join.OuterKey = sm.GetSymbolInfo(outerMa).Symbol as IPropertySymbol;
                             }
                             if (innerKeyArg?.Body is MemberAccessExpressionSyntax innerMa)
                             {
                                 join.InnerKey = sm.GetSymbolInfo(innerMa).Symbol as IPropertySymbol;
                             }
                             qModel.Join = join;
                        }
                        
                        if (resultSelectorArg != null)
                        {
                             if (qModel.Projection == null)
                             {
                                 ParseProjection(resultSelectorArg, qModel, sm);
                             }
                        }
                    }
                }
                 else if (name == "Distinct")
                {
                    qModel.IsDistinct = true;
                }
                
                if (member.Expression is InvocationExpressionSyntax next)
                {
                    current = next;
                }
                else if (member.Expression is IdentifierNameSyntax || member.Expression is GenericNameSyntax || member.Expression is MemberAccessExpressionSyntax)
                {
                     // Base
                     var baseType = sm.GetTypeInfo(member.Expression).Type as INamedTypeSymbol;
                     if (baseType?.TypeArguments.Length == 1)
                     {
                         qModel.ElementType = baseType.TypeArguments[0];
                         qModel.TableName = GetTableName(qModel.ElementType);
                     }
                     break;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
        
        if (qModel.ElementType == null)
        {
             return null;
        }
        if (string.IsNullOrEmpty(qModel.TableName))
        {
             return Fail(qModel, Diagnostics.MissingTableAttribute, qModel.ElementType.ToDisplayString());
        }

        return qModel;
    }

    static PredicateModel? ParsePredicate(ExpressionSyntax expr, QueryModel model, SemanticModel sm)
    {
        if (expr is ParenthesizedExpressionSyntax pex)
        {
            return ParsePredicate(pex.Expression, model, sm);
        }

        if (expr is BinaryExpressionSyntax be)
        {
            if (be.OperatorToken.Text == "&&")
            {
                var l = ParsePredicate(be.Left, model, sm);
                var r = ParsePredicate(be.Right, model, sm);
                if (l != null && r != null)
                {
                    var p = new PredicateModel { Kind = PredicateKind.And };
                    if (l.Kind == PredicateKind.And) p.Children.AddRange(l.Children);
                    else p.Children.Add(l);
                    if (r.Kind == PredicateKind.And) p.Children.AddRange(r.Children);
                    else p.Children.Add(r);
                    return p;
                }
                return null;
            }
            if (be.OperatorToken.Text == "||")
            {
                // Try LikeGroup first (optimization)
                var terms = TryParseLikeTerms(be, sm);
                if (terms != null)
                {
                    var p = new PredicateModel { IsLikeGroup = true, Kind = PredicateKind.LikeGroup };
                    p.LikeTerms.AddRange(terms);
                    return p;
                }
                
                // General OR
                var l = ParsePredicate(be.Left, model, sm);
                var r = ParsePredicate(be.Right, model, sm);
                if (l != null && r != null)
                {
                    var p = new PredicateModel { Kind = PredicateKind.Or };
                    if (l.Kind == PredicateKind.Or) p.Children.AddRange(l.Children);
                    else p.Children.Add(l);
                    if (r.Kind == PredicateKind.Or) p.Children.AddRange(r.Children);
                    else p.Children.Add(r);
                    return p;
                }
                return null;
            }
            
            // Check for IsNull / IsNotNull
            if (be.Right is LiteralExpressionSyntax lit && lit.Kind() == SyntaxKind.NullLiteralExpression)
            {
                if (be.Left is MemberAccessExpressionSyntax ma)
                {
                    var sym = sm.GetSymbolInfo(ma).Symbol as IPropertySymbol;
                    if (sym != null)
                    {
                        var kind = be.OperatorToken.Text == "==" ? PredicateKind.IsNull : PredicateKind.IsNotNull;
                        return new PredicateModel { Left = sym, Kind = kind };
                    }
                }
            }

            // Normal binary
            var leftExpr = Unwrap(be.Left);
            var rightExpr = Unwrap(be.Right);
            
            var leftMa = leftExpr as MemberAccessExpressionSyntax;
            var rightMa = rightExpr as MemberAccessExpressionSyntax;

            IPropertySymbol? propSym = null;
            ExpressionSyntax? otherSide = null;
            string op = be.OperatorToken.Text;
            
            if (leftMa != null)
            {
                propSym = sm.GetSymbolInfo(leftMa).Symbol as IPropertySymbol;
                if (propSym != null) otherSide = be.Right;
            }
            
            if (propSym == null && rightMa != null)
            {
                 propSym = sm.GetSymbolInfo(rightMa).Symbol as IPropertySymbol;
                 if (propSym != null) 
                 {
                     otherSide = be.Left;
                     // Flip operator
                     if (op == ">") op = "<";
                     else if (op == "<") op = ">";
                     else if (op == ">=") op = "<=";
                     else if (op == "<=") op = ">=";
                 }
            }

            if (propSym != null && otherSide != null)
            {
                    object? constVal = null;
                    var constant = sm.GetConstantValue(otherSide);
                    if (constant.HasValue) constVal = constant.Value;
                    
                    return new PredicateModel { Left = propSym, Operator = op, RightExpressionCode = otherSide.ToString(), RightConstant = constVal, Kind = PredicateKind.Binary };
            }
        }
        else if (expr is PrefixUnaryExpressionSyntax pue && pue.OperatorToken.Text == "!")
        {
             if (pue.Operand is InvocationExpressionSyntax inv && IsCollectionContains(inv, sm, out var prop, out var collCode))
             {
                 return new PredicateModel { Left = prop!, CollectionExpressionCode = collCode!, Kind = PredicateKind.NotIn };
             }
        }
        else if (expr is InvocationExpressionSyntax inv)
        {
             if (IsCollectionContains(inv, sm, out var prop, out var collCode))
             {
                 return new PredicateModel { Left = prop!, CollectionExpressionCode = collCode!, Kind = PredicateKind.In };
             }
             var terms = TryParseLikeTerms(inv, sm);
             if (terms != null && terms.Count == 1)
             {
                 var term = terms[0];
                 string code = term.PatternCode;
                 if (term.Kind == LikeKind.Contains) code = "\"%\" + (" + code + ") + \"%\"";
                 else if (term.Kind == LikeKind.StartsWith) code = "(" + code + ") + \"%\"";
                 else if (term.Kind == LikeKind.EndsWith) code = "\"%\" + (" + code + ")";
                 
                 return new PredicateModel { Left = term.Left, Kind = PredicateKind.Like, RightExpressionCode = code };
             }
        }
        Fail(model, Diagnostics.UnsupportedExpression, expr.ToString());
        return null;
    }

    static bool IsCollectionContains(InvocationExpressionSyntax inv, SemanticModel sm, out IPropertySymbol? prop, out string? code)
    {
        prop = null;
        code = null;
        if (inv.Expression is MemberAccessExpressionSyntax ma && ma.Name.Identifier.Text == "Contains")
        {
            // check if argument is property
            if (inv.ArgumentList.Arguments.Count == 1)
            {
                var arg = inv.ArgumentList.Arguments[0].Expression;
                if (arg is MemberAccessExpressionSyntax maArg)
                {
                    prop = sm.GetSymbolInfo(maArg).Symbol as IPropertySymbol;
                    if (prop != null)
                    {
                         code = ma.Expression.ToString();
                         return true;
                    }
                }
            }
        }
        return false;
    }

    static string GetTableName(ITypeSymbol type)
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

    internal static IPropertySymbol? GetPrimaryKey(ITypeSymbol type)
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

    static void Report(GeneratorSyntaxContext ctx, DiagnosticDescriptor desc, params string[] args)
    {
        var _ = Diagnostic.Create(desc, ctx.Node.GetLocation(), args);
    }

    static QueryModel Fail(QueryModel m, DiagnosticDescriptor desc, params string[] args)
    {
        m.HasError = true;
        m.ErrorDescriptor = desc;
        m.ErrorArgs = args;
        return m;
    }

    static List<LikeTerm>? TryParseLikeTerms(ExpressionSyntax expr, SemanticModel sm)
    {
        var terms = new List<LikeTerm>();
        if (Parse(expr, sm, terms)) return terms;
        return null;
        static bool Parse(ExpressionSyntax e, SemanticModel sm, List<LikeTerm> acc)
        {
            if (e is BinaryExpressionSyntax be && be.OperatorToken.Text == "||")
            {
                return Parse(be.Left, sm, acc) && Parse(be.Right, sm, acc);
            }
            if (e is InvocationExpressionSyntax inv && inv.Expression is MemberAccessExpressionSyntax ma)
            {
                var fn = ma.Name.Identifier.Text;
                if (fn == "Contains" || fn == "StartsWith" || fn == "EndsWith")
                {
                    if (inv.ArgumentList.Arguments.Count != 1) return false;
                    var argExpr = inv.ArgumentList.Arguments[0].Expression;
                    var propMa = ma.Expression as MemberAccessExpressionSyntax;
                    if (propMa is null) return false;
                    var propSym = sm.GetSymbolInfo(propMa).Symbol as IPropertySymbol;
                    if (propSym is null) return false;
                    var kind = fn == "Contains" ? LikeKind.Contains : (fn == "StartsWith" ? LikeKind.StartsWith : LikeKind.EndsWith);
                    acc.Add(new LikeTerm { Left = propSym, PatternCode = argExpr.ToString(), Kind = kind });
                    return true;
                }
            }
            return false;
        }
    }

    static bool ParseProjectionSource(ExpressionSyntax expr, ProjectionEntry entry, SemanticModel sm)
    {
        if (expr is ParenthesizedExpressionSyntax pe) return ParseProjectionSource(pe.Expression, entry, sm);
        if (expr is BinaryExpressionSyntax be && be.OperatorToken.Text == "??") return ParseProjectionSource(be.Left, entry, sm);

        if (expr is MemberAccessExpressionSyntax ma)
        {
            var sym = sm.GetSymbolInfo(ma).Symbol;
            if (sym is IPropertySymbol prop)
            {
                if (ma.Expression != null)
                {
                    var typeInfo = sm.GetTypeInfo(ma.Expression);
                    var t = typeInfo.Type;
                    if (t != null && (t.Name.StartsWith("IGrouping") || t.AllInterfaces.Any(i => i.Name.StartsWith("IGrouping"))))
                    {
                        entry.Kind = ProjectionEntryKind.GroupKey;
                        entry.Property = prop;
                        entry.Type = prop.Type;
                        return true;
                    }
                }
                entry.Kind = ProjectionEntryKind.Property;
                entry.Property = prop;
                entry.Type = prop.Type;
                return true;
            }
        }
        else if (expr is InvocationExpressionSyntax inv)
        {
             var method = sm.GetSymbolInfo(inv).Symbol as IMethodSymbol;
             if (method != null)
             {
                 if (method.Name == "Count" || method.Name == "Sum" || method.Name == "Min" || method.Name == "Max" || method.Name == "Average")
                 {
                     entry.Kind = ProjectionEntryKind.Aggregator;
                     entry.Aggregator = method.Name.ToUpper();
                     entry.Type = method.ReturnType;
                     if (inv.ArgumentList.Arguments.Count > 0)
                     {
                         var arg = inv.ArgumentList.Arguments[0].Expression as SimpleLambdaExpressionSyntax;
                         if (arg != null && arg.Body is MemberAccessExpressionSyntax bodyMa)
                         {
                             var prop = sm.GetSymbolInfo(bodyMa).Symbol as IPropertySymbol;
                             if (prop != null)
                             {
                                 entry.AggregatorProperty = prop;
                             }
                         }
                     }
                     return true;
                 }
             }
        }
        return false;
    }

    static ExpressionSyntax Unwrap(ExpressionSyntax expr)
    {
        if (expr is ParenthesizedExpressionSyntax p) return Unwrap(p.Expression);
        return expr;
    }

    static void ParseProjection(LambdaExpressionSyntax arg, QueryModel qModel, SemanticModel sm)
    {
        var bodyNode = arg.Body;
        if (!(bodyNode is ExpressionSyntax bodyExpr)) return;
        var body = Unwrap(bodyExpr);
        if (body is AnonymousObjectCreationExpressionSyntax anon)
        {
            var proj = new ProjectionModel { IsAnonymous = true, TypeName = "Projection_" + Math.Abs(anon.GetHashCode()) };
            foreach(var init in anon.Initializers)
            {
                var alias = (init.NameEquals?.Name.Identifier.Text) ?? null;
                var entry = new ProjectionEntry { Alias = alias };
                var expr = init.Expression as ExpressionSyntax;
                if (expr != null && ParseProjectionSource(expr, entry, sm))
                {
                    if (entry.Alias == null && entry.Property != null) entry.Alias = entry.Property.Name;
                    proj.Entries.Add(entry);
                }
            }
            qModel.Projection = proj;
        }
        else if (body is ObjectCreationExpressionSyntax objCreation)
        {
            var typeSymbol = sm.GetTypeInfo(objCreation).Type;
            var typeName = typeSymbol?.ToDisplayString() ?? "object";
            var proj = new ProjectionModel { IsAnonymous = false, TypeName = typeName };
            if (objCreation.Initializer != null)
            {
                foreach(var initExpr in objCreation.Initializer.Expressions)
                {
                    if (initExpr is AssignmentExpressionSyntax assign)
                    {
                        var left = assign.Left as IdentifierNameSyntax;
                        var alias = left?.Identifier.Text;
                        var entry = new ProjectionEntry { Alias = alias };
                        if (ParseProjectionSource(assign.Right, entry, sm))
                        {
                             proj.Entries.Add(entry);
                        }
                    }
                }
            }
            qModel.Projection = proj;
        }
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

    static bool IsDependent(ExpressionSyntax e, string? pName)
    {
        if (pName == null) return false;
        if (e is IdentifierNameSyntax id && id.Identifier.Text == pName) return true;
        foreach(var child in e.ChildNodes())
            if (child is ExpressionSyntax ce && IsDependent(ce, pName)) return true;
        return false;
    }
}

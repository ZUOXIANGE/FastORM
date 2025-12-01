using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace FastORM.Generator;

internal sealed class PredicateModel
{
    public readonly List<PredicateModel> Children = new();
    public readonly List<LikeTerm> LikeTerms = new();
    public IPropertySymbol Left { get; set; } = null!;
    public string Operator { get; set; } = "";
    public string RightExpressionCode { get; set; } = "";
    public bool IsIn { get; set; }
    public bool IsNotIn { get; set; }
    public string CollectionExpressionCode { get; set; } = "";
    public bool IsLikeGroup { get; set; }
    public PredicateKind Kind { get; set; }
    public object? RightConstant { get; set; }
    public int ParameterIndex { get; set; }
    public int VariableIndex { get; set; }
}

internal sealed class LikeTerm
{
    public IPropertySymbol Left { get; set; } = null!;
    public string PatternCode { get; set; } = "";
    public LikeKind Kind { get; set; }
}

internal enum PredicateKind
{
    Binary,
    Like,
    LikeGroup,
    In,
    NotIn,
    IsNull,
    IsNotNull,
    And,
    Or
}

internal enum LikeKind
{
    Contains,
    StartsWith,
    EndsWith
}

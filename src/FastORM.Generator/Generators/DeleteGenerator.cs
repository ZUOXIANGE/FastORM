using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

[Generator]
public sealed class DeleteGenerator : BaseGenerator
{
    protected override bool IsCandidateMethod(string name)
    {
        return name == "Delete" || name == "DeleteAsync";
    }
}
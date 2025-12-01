using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

[Generator]
public sealed class UpdateGenerator : BaseGenerator
{
    protected override bool IsCandidateMethod(string name)
    {
        return name == "Update" || name == "UpdateAsync";
    }
}
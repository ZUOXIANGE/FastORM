using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

[Generator]
public sealed class InsertGenerator : BaseGenerator
{
    protected override bool IsCandidateMethod(string name)
    {
        return name == "Insert" || name == "InsertAsync";
    }
}
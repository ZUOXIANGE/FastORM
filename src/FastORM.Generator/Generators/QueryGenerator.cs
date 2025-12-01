using Microsoft.CodeAnalysis;

namespace FastORM.Generator;

[Generator]
public sealed class QueryGenerator : BaseGenerator
{
    protected override bool IsCandidateMethod(string name)
    {
        return name == "ToList" || name == "ToListAsync" ||
               name == "FirstOrDefault" || name == "FirstOrDefaultAsync" ||
               name == "Count" || name == "CountAsync" ||
               name == "Max" || name == "MaxAsync" ||
               name == "Min" || name == "MinAsync" ||
               name == "Average" || name == "AverageAsync" ||
               name == "Sum" || name == "SumAsync" ||
               name == "Any" || name == "AnyAsync" ||
               name == "All" || name == "AllAsync";
    }
}
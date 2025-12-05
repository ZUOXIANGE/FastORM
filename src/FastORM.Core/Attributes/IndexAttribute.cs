using System;

namespace FastORM;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class IndexAttribute : Attribute
{
    public string[] PropertyNames { get; }
    public string? Name { get; set; }
    public bool IsUnique { get; set; }

    public IndexAttribute(params string[] propertyNames)
    {
        PropertyNames = propertyNames;
    }
}

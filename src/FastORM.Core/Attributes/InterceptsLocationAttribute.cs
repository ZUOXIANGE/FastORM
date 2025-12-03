namespace System.Runtime.CompilerServices;

/// <summary>
/// Attribute used to indicate that a method intercepts a location.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class InterceptsLocationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptsLocationAttribute"/> class.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="line">The line number.</param>
    /// <param name="column">The column number.</param>
    public InterceptsLocationAttribute(string filePath, int line, int column) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterceptsLocationAttribute"/> class.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="data">The data.</param>
    public InterceptsLocationAttribute(int version, string data) { }
}

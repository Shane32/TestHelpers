using System.Reflection;

namespace System.Linq.Expressions;

/// <summary>
/// Provides extension methods for <see cref="Expression"/> instances.
/// </summary>
public static class DebugViewExpressionExtensions
{
    /// <summary>
    /// Gets the debug view of the specified expression.
    /// </summary>
    public static string GetDebugView(this Expression exp)
    {
        var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (propertyInfo.GetValue(exp) as string)!;
    }
}

namespace Sieve.Plus.QueryBuilder;

/// <summary>
/// Represents a parsed filter with its property name, operator, and value
/// </summary>
public class FilterInfo
{
    /// <summary>
    /// The property name being filtered
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// The filter operator (equals, not equals, contains, starts with, greater than, less than, etc.)
    /// </summary>
    public string Operator { get; set; } = string.Empty;

    /// <summary>
    /// The filter value as a string
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The original filter string
    /// </summary>
    public string OriginalFilter { get; set; } = string.Empty;

    /// <summary>
    /// Returns the original filter string
    /// </summary>
    public override string ToString() => OriginalFilter;
}

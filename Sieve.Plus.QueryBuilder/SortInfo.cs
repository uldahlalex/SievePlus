namespace Sieve.Plus.QueryBuilder;

/// <summary>
/// Represents a parsed sort with its property name and direction
/// </summary>
public class SortInfo
{
    /// <summary>
    /// The property name being sorted
    /// </summary>
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the sort is descending (true) or ascending (false)
    /// </summary>
    public bool IsDescending { get; set; }

    /// <summary>
    /// The original sort string
    /// </summary>
    public string OriginalSort { get; set; } = string.Empty;

    /// <summary>
    /// Returns the original sort string
    /// </summary>
    public override string ToString() => OriginalSort;
}

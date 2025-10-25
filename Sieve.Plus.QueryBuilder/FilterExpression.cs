using System.Collections.Generic;
using System.Linq;

namespace Sieve.Plus.QueryBuilder;

/// <summary>
/// Represents a filter expression that can contain groups and operators
/// </summary>
public abstract class FilterExpression
{
    public abstract string ToQueryString();
}

/// <summary>
/// A simple filter term (e.g., "Name==John")
/// </summary>
public class FilterTerm : FilterExpression
{
    public string Expression { get; set; }

    public FilterTerm(string expression)
    {
        Expression = expression;
    }

    public override string ToQueryString() => Expression;
}

/// <summary>
/// A group of filters combined with an operator (AND or OR)
/// </summary>
public class FilterGroup : FilterExpression
{
    public List<FilterExpression> Filters { get; set; } = new();
    public FilterOperator Operator { get; set; }
    public bool WrapInParentheses { get; set; }

    public override string ToQueryString()
    {
        if (!Filters.Any())
        {
            return string.Empty;
        }

        var separator = Operator == FilterOperator.And ? "," : " || ";
        var result = string.Join(separator, Filters.Select(f => f.ToQueryString()));

        if (WrapInParentheses && Filters.Count > 1)
        {
            return $"({result})";
        }

        return result;
    }
}

/// <summary>
/// Filter operator type
/// </summary>
public enum FilterOperator
{
    And,
    Or
}

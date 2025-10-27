using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sieve.Plus.Models;

namespace Sieve.Plus.QueryBuilder;

/// <summary>
/// Type-safe builder for constructing Sieve.Plus query strings
/// </summary>
/// <typeparam name="TQueryModel">The query model type that defines the filterable/sortable properties</typeparam>
public class SievePlusQueryBuilder<TQueryModel>
    where TQueryModel : class, ISievePlusQueryModel, Sieve.Plus.Models.ISievePlusQueryModel
{
    private readonly List<List<string>> _filterGroups = new() { new List<string>() };
    private int _currentGroupIndex = 0;
    private readonly List<string> _sorts = new();
    private int? _page;
    private int? _pageSize;

    // New fields for parentheses support
    private readonly Stack<FilterSegment> _segmentStack = new();
    private FilterSegment _currentSegment = new FilterSegment();

    /// <summary>
    /// Start a new OR group. Subsequent filters will be OR'd with previous filter groups.
    /// </summary>
    /// <example>
    /// builder.FilterEquals(c => c.CpuModel, "Intel i9")
    ///        .Or()
    ///        .FilterEquals(c => c.CpuModel, "AMD Ryzen 9")
    /// // Produces: "CpuModel==Intel i9 || CpuModel==AMD Ryzen 9"
    /// </example>
    public SievePlusQueryBuilder<TQueryModel> Or()
    {
        // Mark current segment as OR group
        _currentSegment.IsOrGroup = true;

        // Also maintain backward compatibility with old group system
        _filterGroups.Add(new List<string>());
        _currentGroupIndex++;
        return this;
    }

    /// <summary>
    /// Add a filter using equals operator (==)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterEquals<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}=={formattedValue}");
        return this;
    }

    /// <summary>
    /// Helper method to add a filter to both systems (new segment + old groups)
    /// </summary>
    private void AddFilter(string filter)
    {
        _currentSegment.Parts.Add(filter);
        _filterGroups[_currentGroupIndex].Add(filter);
    }

    /// <summary>
    /// Add a filter using not equals operator (!=)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterNotEquals<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}!={formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using contains operator (@=)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterContains<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}@={formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using starts with operator (_=)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterStartsWith<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}_={formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using greater than operator (>)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterGreaterThan<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}>{formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using less than operator
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterLessThan<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}<{formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using greater than or equal operator
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterGreaterThanOrEqual<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}>={formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using less than or equal operator
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterLessThanOrEqual<TProp>(Expression<Func<TQueryModel, TProp>> property, TProp value)
    {
        var propertyName = GetPropertyName(property);
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}<={formattedValue}");
        return this;
    }

    /// <summary>
    /// Add a filter using a custom property name (for mapped properties like BooksCount)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> FilterByName(string propertyName, string operatorSymbol, object value)
    {
        var formattedValue = FormatValue(value);
        AddFilter($"{propertyName}{operatorSymbol}{formattedValue}");
        return this;
    }

    /// <summary>
    /// Add ascending sort for a property
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> SortBy<TProp>(Expression<Func<TQueryModel, TProp>> property)
    {
        var propertyName = GetPropertyName(property);
        _sorts.Add(propertyName);
        return this;
    }

    /// <summary>
    /// Add descending sort for a property
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> SortByDescending<TProp>(Expression<Func<TQueryModel, TProp>> property)
    {
        var propertyName = GetPropertyName(property);
        _sorts.Add($"-{propertyName}");
        return this;
    }

    /// <summary>
    /// Add sort using a custom property name (for mapped properties like BooksCount)
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> SortByName(string propertyName, bool descending = false)
    {
        _sorts.Add(descending ? $"-{propertyName}" : propertyName);
        return this;
    }

    /// <summary>
    /// Set the page number for pagination
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> Page(int page)
    {
        _page = page;
        return this;
    }

    /// <summary>
    /// Set the page size for pagination
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> PageSize(int pageSize)
    {
        _pageSize = pageSize;
        return this;
    }

    /// <summary>
    /// Build the Filters query string component
    /// </summary>
    public string BuildFiltersString()
    {
        // Check if we used the new segment system (BeginGroup/EndGroup)
        if (_segmentStack.Count > 0)
        {
            throw new InvalidOperationException("Unmatched BeginGroup() call - missing EndGroup()");
        }

        // If current segment has parts, use the new segment system
        if (_currentSegment.Parts.Count > 0)
        {
            return _currentSegment.ToQueryString();
        }

        // Fall back to old group system for backward compatibility
        var nonEmptyGroups = _filterGroups.Where(g => g.Any()).ToList();
        if (!nonEmptyGroups.Any())
            return string.Empty;

        var groupStrings = nonEmptyGroups.Select(group => string.Join(",", group));
        return string.Join(" || ", groupStrings);
    }

    /// <summary>
    /// Build the Sorts query string component
    /// </summary>
    public string BuildSortsString()
    {
        return _sorts.Any() ? string.Join(",", _sorts) : string.Empty;
    }

    /// <summary>
    /// Build a complete SievePlusModel object with type information
    /// </summary>
    public SievePlusModel<TQueryModel> BuildSieveModel()
    {
        return new SievePlusModel<TQueryModel>
        {
            Filters = BuildFiltersString(),
            Sorts = BuildSortsString(),
            Page = _page,
            PageSize = _pageSize
        };
    }

    /// <summary>
    /// Build the complete query string for use in HTTP requests
    /// </summary>
    public string BuildQueryString()
    {
        var parts = new List<string>();

        var filtersString = BuildFiltersString();
        if (!string.IsNullOrEmpty(filtersString))
        {
            parts.Add($"filters={Uri.EscapeDataString(filtersString)}");
        }

        if (_sorts.Any())
        {
            parts.Add($"sorts={Uri.EscapeDataString(string.Join(",", _sorts))}");
        }

        if (_page.HasValue)
        {
            parts.Add($"page={_page.Value}");
        }

        if (_pageSize.HasValue)
        {
            parts.Add($"pageSize={_pageSize.Value}");
        }

        return parts.Any() ? string.Join("&", parts) : string.Empty;
    }

    /// <summary>
    /// Format a value for use in a filter, handling DateTime with ISO 8601 format
    /// </summary>
    private static string FormatValue<TProp>(TProp value)
    {
        if (value is DateTime dateTime)
        {
            // Use ISO 8601 format with UTC indicator to preserve timezone information
            // Format: "yyyy-MM-ddTHH:mm:ss.fffZ" for UTC times
            // This ensures compatibility with modern database drivers (PostgreSQL, SQL Server, etc.)
            // that enforce strict timezone handling
            return dateTime.Kind == DateTimeKind.Utc
                ? dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture)
                : dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);
        }

        return value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Extract property name from expression, handling nested properties
    /// </summary>
    private static string GetPropertyName<TProp>(Expression<Func<TQueryModel, TProp>> property)
    {
        if (property.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        if (property.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression operand)
        {
            return operand.Member.Name;
        }

        throw new ArgumentException($"Expression '{property}' does not refer to a property.");
    }

    /// <summary>
    /// Parse a query string into a SieveQueryBuilder instance
    /// </summary>
    /// <param name="queryString">The query string to parse (e.g., filters with Name contains Bob, sorts with descending CreatedAt, page and pageSize)</param>
    public static SievePlusQueryBuilder<TQueryModel> ParseQueryString(string queryString)
    {
        var builder = new SievePlusQueryBuilder<TQueryModel>();

        if (string.IsNullOrWhiteSpace(queryString))
            return builder;

        // Remove leading '?' if present
        queryString = queryString.TrimStart('?');

        var parameters = queryString.Split('&');

        foreach (var param in parameters)
        {
            var keyValue = param.Split(new[] { '=' }, 2);
            if (keyValue.Length != 2) continue;

            var key = keyValue[0].ToLowerInvariant();
            var value = Uri.UnescapeDataString(keyValue[1]);

            switch (key)
            {
                case "filters":
                    ParseFiltersIntoGroups(value, builder);
                    break;
                case "sorts":
                    builder._sorts.AddRange(ParseSorts(value));
                    break;
                case "page":
                    if (int.TryParse(value, out var page))
                        builder._page = page;
                    break;
                case "pagesize":
                    if (int.TryParse(value, out var pageSize))
                        builder._pageSize = pageSize;
                    break;
            }
        }

        return builder;
    }

    /// <summary>
    /// Create a builder from an existing SievePlusModel
    /// </summary>
    public static SievePlusQueryBuilder<TQueryModel> FromSieveModel(SievePlusModel plusModel)
    {
        var builder = new SievePlusQueryBuilder<TQueryModel>();

        if (!string.IsNullOrWhiteSpace(plusModel.Filters))
        {
            ParseFiltersIntoGroups(plusModel.Filters, builder);
        }

        if (!string.IsNullOrWhiteSpace(plusModel.Sorts))
        {
            builder._sorts.AddRange(ParseSorts(plusModel.Sorts));
        }

        builder._page = plusModel.Page;
        builder._pageSize = plusModel.PageSize;

        return builder;
    }

    /// <summary>
    /// Get all filters as structured FilterInfo objects (flattened from all groups)
    /// </summary>
    public IReadOnlyList<FilterInfo> GetFilters()
    {
        var filterInfos = new List<FilterInfo>();

        foreach (var group in _filterGroups)
        {
            foreach (var filter in group)
            {
                var filterInfo = ParseFilterString(filter);
                filterInfos.Add(filterInfo);
            }
        }

        return filterInfos.AsReadOnly();
    }

    /// <summary>
    /// Get all filter groups as structured FilterInfo objects
    /// </summary>
    public IReadOnlyList<IReadOnlyList<FilterInfo>> GetFilterGroups()
    {
        var groups = new List<IReadOnlyList<FilterInfo>>();

        foreach (var group in _filterGroups)
        {
            var filterInfos = group.Select(ParseFilterString).ToList().AsReadOnly();
            groups.Add(filterInfos);
        }

        return groups.AsReadOnly();
    }

    /// <summary>
    /// Get all sorts as structured SortInfo objects
    /// </summary>
    public IReadOnlyList<SortInfo> GetSorts()
    {
        var sortInfos = new List<SortInfo>();

        foreach (var sort in _sorts)
        {
            var isDescending = sort.StartsWith("-");
            var propertyName = isDescending ? sort.Substring(1) : sort;

            sortInfos.Add(new SortInfo
            {
                PropertyName = propertyName,
                IsDescending = isDescending,
                OriginalSort = sort
            });
        }

        return sortInfos.AsReadOnly();
    }

    /// <summary>
    /// Get the current page number
    /// </summary>
    public int? GetPage() => _page;

    /// <summary>
    /// Get the current page size
    /// </summary>
    public int? GetPageSize() => _pageSize;

    /// <summary>
    /// Check if a filter exists for the given property name in any group
    /// </summary>
    public bool HasFilter(string propertyName)
    {
        return _filterGroups.Any(group => group.Any(f => f.StartsWith(propertyName)));
    }

    /// <summary>
    /// Check if a sort exists for the given property name
    /// </summary>
    public bool HasSort(string propertyName)
    {
        return _sorts.Any(s => s == propertyName || s == $"-{propertyName}");
    }

    private static void ParseFiltersIntoGroups(string filtersString, SievePlusQueryBuilder<TQueryModel> builder)
    {
        if (string.IsNullOrWhiteSpace(filtersString))
            return;

        // Clear default empty group
        builder._filterGroups.Clear();
        builder._currentGroupIndex = 0;

        // Split by || for OR groups
        var orGroups = filtersString.Split(new[] { " || " }, StringSplitOptions.None);

        foreach (var orGroup in orGroups)
        {
            var filters = orGroup.Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList();

            if (filters.Any())
            {
                builder._filterGroups.Add(filters);
            }
        }

        // Ensure at least one group exists
        if (!builder._filterGroups.Any())
        {
            builder._filterGroups.Add(new List<string>());
        }

        builder._currentGroupIndex = builder._filterGroups.Count - 1;
    }

    private static List<string> ParseFilters(string filtersString)
    {
        if (string.IsNullOrWhiteSpace(filtersString))
            return new List<string>();

        // For backward compatibility - flatten all groups
        var allFilters = new List<string>();
        var orGroups = filtersString.Split(new[] { " || " }, StringSplitOptions.None);

        foreach (var orGroup in orGroups)
        {
            var filters = orGroup.Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f));
            allFilters.AddRange(filters);
        }

        return allFilters;
    }

    private static List<string> ParseSorts(string sortsString)
    {
        if (string.IsNullOrWhiteSpace(sortsString))
            return new List<string>();

        // Split by comma
        return sortsString.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    private static FilterInfo ParseFilterString(string filter)
    {
        // Try to match operators in order of length (longest first to avoid partial matches)
        var operators = new[] { "==", "!=", ">=", "<=", "@=", "_=", ">", "<" };

        foreach (var op in operators)
        {
            var index = filter.IndexOf(op);
            if (index > 0)
            {
                var propertyName = filter.Substring(0, index);
                var value = filter.Substring(index + op.Length);

                return new FilterInfo
                {
                    PropertyName = propertyName,
                    Operator = op,
                    Value = value,
                    OriginalFilter = filter
                };
            }
        }

        // If no operator found, return the whole thing as property name
        return new FilterInfo
        {
            PropertyName = filter,
            Operator = string.Empty,
            Value = string.Empty,
            OriginalFilter = filter
        };
    }

    /// <summary>
    /// Begin a grouped sub-expression with parentheses
    /// Filters added after BeginGroup will be wrapped in parentheses
    /// Must be paired with EndGroup()
    /// </summary>
    /// <example>
    /// builder.BeginGroup()
    ///        .FilterEquals(c => c.Processor, "Intel")
    ///        .Or()
    ///        .FilterEquals(c => c.Processor, "AMD")
    ///        .EndGroup()
    ///        .FilterGreaterThan(c => c.Price, 1000)
    /// // Produces: (Processor==Intel || Processor==AMD),Price&gt;1000
    /// </example>
    public SievePlusQueryBuilder<TQueryModel> BeginGroup()
    {
        // Push current segment onto stack
        _segmentStack.Push(_currentSegment);

        // Create new segment that will be wrapped in parentheses
        _currentSegment = new FilterSegment { WrapInParentheses = true };

        return this;
    }

    /// <summary>
    /// End a grouped sub-expression
    /// Must be paired with BeginGroup()
    /// </summary>
    public SievePlusQueryBuilder<TQueryModel> EndGroup()
    {
        if (_segmentStack.Count == 0)
        {
            throw new InvalidOperationException("EndGroup() called without matching BeginGroup()");
        }

        // Pop the parent segment and add current segment to it
        var completedSegment = _currentSegment;
        _currentSegment = _segmentStack.Pop();
        _currentSegment.Parts.Add(completedSegment);

        return this;
    }

    /// <summary>
    /// Add shared constraints that will be applied to all OR groups
    /// This is a convenience method to avoid repeating filters
    /// </summary>
    /// <example>
    /// // Instead of repeating constraints:
    /// builder.FilterEquals(c => c.Processor, "Intel")
    ///        .FilterGreaterThan(c => c.Price, 1000)
    ///        .FilterLessThan(c => c.Price, 2000)
    ///        .Or()
    ///        .FilterEquals(c => c.Processor, "AMD")
    ///        .FilterGreaterThan(c => c.Price, 1000)
    ///        .FilterLessThan(c => c.Price, 2000)
    ///
    /// // Use WithSharedConstraints:
    /// builder.BeginGroup()
    ///        .FilterEquals(c => c.Processor, "Intel")
    ///        .Or()
    ///        .FilterEquals(c => c.Processor, "AMD")
    ///        .EndGroup()
    ///        .WithSharedConstraints(b => b
    ///            .FilterGreaterThan(c => c.Price, 1000)
    ///            .FilterLessThan(c => c.Price, 2000))
    /// // Produces: (Processor==Intel || Processor==AMD),Price&gt;1000,Price&lt;2000
    /// </example>
    public SievePlusQueryBuilder<TQueryModel> WithSharedConstraints(Action<SievePlusQueryBuilder<TQueryModel>> constraintsBuilder)
    {
        constraintsBuilder(this);
        return this;
    }

    /// <summary>
    /// Create a filter group with alternative values for a single property
    /// This is a helper for the common case of OR-ing values on one property
    /// </summary>
    /// <example>
    /// builder.FilterWithAlternatives(
    ///     c => c.Processor,
    ///     new[] { "Intel i9", "AMD Ryzen 9" },
    ///     b => b.FilterGreaterThan(c => c.Price, 1000))
    /// // Produces: (Processor==Intel i9 || Processor==AMD Ryzen 9),Price&gt;1000
    /// </example>
    public SievePlusQueryBuilder<TQueryModel> FilterWithAlternatives<TProp>(
        Expression<Func<TQueryModel, TProp>> property,
        TProp[] alternatives,
        Action<SievePlusQueryBuilder<TQueryModel>>? sharedConstraints = null)
    {
        if (alternatives == null || alternatives.Length == 0)
        {
            return this;
        }

        BeginGroup();

        for (int i = 0; i < alternatives.Length; i++)
        {
            if (i > 0)
            {
                Or();
            }
            FilterEquals(property, alternatives[i]);
        }

        EndGroup();

        // Apply shared constraints after the group
        if (sharedConstraints != null)
        {
            sharedConstraints(this);
        }

        return this;
    }

    /// <summary>
    /// Create a new builder instance for fluent API
    /// </summary>
    public static SievePlusQueryBuilder<TQueryModel> Create() => new();
}

/// <summary>
/// Represents a segment in the filter expression tree
/// Can be a simple filter, an AND group, or an OR group
/// </summary>
internal class FilterSegment
{
    public List<object> Parts { get; set; } = new(); // Can contain strings or FilterSegment
    public bool IsOrGroup { get; set; } = false;
    public bool WrapInParentheses { get; set; } = false;

    public string ToQueryString()
    {
        if (Parts.Count == 0)
            return string.Empty;

        var separator = IsOrGroup ? " || " : ",";
        var parts = Parts.Select(p => p is FilterSegment segment ? segment.ToQueryString() : p.ToString()).ToArray();
        var result = string.Join(separator, parts.Where(p => !string.IsNullOrEmpty(p)));

        if (WrapInParentheses && Parts.Count > 1)
        {
            return $"({result})";
        }

        return result;
    }
}

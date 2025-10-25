using System;
using System.Collections.Generic;
using System.Linq;
using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    /// <summary>
    /// Parser for Sieve filter expressions with support for parentheses grouping
    /// </summary>
    /// <typeparam name="TFilterTerm">The filter term type</typeparam>
    public class FilterParser<TFilterTerm> where TFilterTerm : IFilterTerm, new()
    {
        private const char OpenParen = '(';
        private const char CloseParen = ')';
        private const char Comma = ',';
        private const string OrSeparator = " || ";
        private const char EscapeChar = '\\';

        /// <summary>
        /// Parse a filter string into groups respecting parentheses, commas (AND), and || (OR)
        /// Examples:
        /// - "A==1,B==2" -> [[A==1, B==2]]
        /// - "A==1 || B==2" -> [[A==1], [B==2]]
        /// - "(A==1 || B==2),C==3" -> [[A==1, C==3], [B==2, C==3]]
        /// - "A==1,(B==2 || C==3)" -> [[A==1, B==2], [A==1, C==3]]
        /// </summary>
        public List<List<TFilterTerm>> Parse(string filterString)
        {
            if (string.IsNullOrWhiteSpace(filterString))
            {
                return null;
            }

            // First, check if there are parentheses - if not, use simple parsing
            if (!filterString.Contains(OpenParen) && !filterString.Contains(CloseParen))
            {
                return ParseSimpleFilters(filterString);
            }

            // Parse with parentheses support
            var expandedGroups = ExpandParentheses(filterString);
            return ConvertToFilterTerms(expandedGroups);
        }

        /// <summary>
        /// Simple parsing without parentheses (backward compatible)
        /// Splits by || for OR groups, then by comma for AND within groups
        /// </summary>
        private List<List<TFilterTerm>> ParseSimpleFilters(string filterString)
        {
            var result = new List<List<TFilterTerm>>();

            // Check if there's OR logic
            var orGroups = SplitByOrSeparator(filterString);

            foreach (var orGroup in orGroups)
            {
                if (string.IsNullOrWhiteSpace(orGroup))
                {
                    continue;
                }

                var filterTerms = new List<TFilterTerm>();
                var andFilters = SplitByComma(orGroup);

                foreach (var filter in andFilters)
                {
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        continue;
                    }

                    var filterTerm = new TFilterTerm { Filter = filter.Trim() };
                    filterTerms.Add(filterTerm);
                }

                if (filterTerms.Count > 0)
                {
                    result.Add(filterTerms);
                }
            }

            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Expand parentheses into Cartesian product of filter groups
        /// Example: "(A || B),C" becomes [[A,C], [B,C]]
        /// </summary>
        private List<List<string>> ExpandParentheses(string filterString)
        {
            // Parse the filter string into AND segments
            var andSegments = ParseAndSegments(filterString);

            // Each segment is either a simple filter or a group of OR filters
            // We need to compute the Cartesian product
            var result = new List<List<string>> { new List<string>() };

            foreach (var segment in andSegments)
            {
                var segmentOptions = segment; // segment is already a list of options (OR group)
                var newResult = new List<List<string>>();

                foreach (var existingGroup in result)
                {
                    foreach (var option in segmentOptions)
                    {
                        var newGroup = new List<string>(existingGroup) { option };
                        newResult.Add(newGroup);
                    }
                }

                result = newResult;
            }

            return result;
        }

        /// <summary>
        /// Parse the filter string into AND segments
        /// Each segment contains OR options
        /// Example: "(A || B),C,(D || E)" -> [[A, B], [C], [D, E]]
        /// </summary>
        private List<List<string>> ParseAndSegments(string filterString)
        {
            var segments = new List<List<string>>();
            var i = 0;

            while (i < filterString.Length)
            {
                // Skip whitespace
                while (i < filterString.Length && char.IsWhiteSpace(filterString[i]))
                {
                    i++;
                }

                if (i >= filterString.Length)
                {
                    break;
                }

                if (filterString[i] == OpenParen)
                {
                    // Find matching close paren
                    var closeIndex = FindMatchingCloseParen(filterString, i);
                    if (closeIndex == -1)
                    {
                        throw new ArgumentException($"Unmatched opening parenthesis at position {i}");
                    }

                    // Extract content inside parentheses
                    var content = filterString.Substring(i + 1, closeIndex - i - 1);

                    // Parse OR groups within parentheses
                    var orOptions = SplitByOrSeparator(content);
                    segments.Add(orOptions.Select(o => o.Trim()).Where(o => !string.IsNullOrEmpty(o)).ToList());

                    i = closeIndex + 1;

                    // Skip comma after closing paren if present
                    while (i < filterString.Length && (char.IsWhiteSpace(filterString[i]) || filterString[i] == Comma))
                    {
                        i++;
                    }
                }
                else
                {
                    // Regular filter (not in parentheses)
                    var endIndex = FindNextComma(filterString, i);
                    var filter = filterString.Substring(i, endIndex - i).Trim();

                    if (!string.IsNullOrEmpty(filter))
                    {
                        // Check if this filter contains || (should be in parentheses but handle it anyway)
                        if (filter.Contains(OrSeparator))
                        {
                            var orOptions = SplitByOrSeparator(filter);
                            segments.Add(orOptions.Select(o => o.Trim()).Where(o => !string.IsNullOrEmpty(o)).ToList());
                        }
                        else
                        {
                            segments.Add(new List<string> { filter });
                        }
                    }

                    i = endIndex;

                    // Skip comma
                    if (i < filterString.Length && filterString[i] == Comma)
                    {
                        i++;
                    }
                }
            }

            return segments;
        }

        /// <summary>
        /// Find matching closing parenthesis, accounting for nesting
        /// </summary>
        private int FindMatchingCloseParen(string str, int openIndex)
        {
            var depth = 0;

            for (var i = openIndex; i < str.Length; i++)
            {
                if (i > 0 && str[i - 1] == EscapeChar)
                {
                    continue; // Skip escaped characters
                }

                if (str[i] == OpenParen)
                {
                    depth++;
                }
                else if (str[i] == CloseParen)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1; // No matching close paren found
        }

        /// <summary>
        /// Find the next unescaped comma at the current depth
        /// </summary>
        private int FindNextComma(string str, int startIndex)
        {
            var depth = 0;

            for (var i = startIndex; i < str.Length; i++)
            {
                if (i > 0 && str[i - 1] == EscapeChar)
                {
                    continue; // Skip escaped characters
                }

                if (str[i] == OpenParen)
                {
                    depth++;
                }
                else if (str[i] == CloseParen)
                {
                    depth--;
                }
                else if (str[i] == Comma && depth == 0)
                {
                    return i;
                }
            }

            return str.Length; // No comma found, return end of string
        }

        /// <summary>
        /// Split by || operator, respecting escape sequences
        /// </summary>
        private List<string> SplitByOrSeparator(string input)
        {
            var result = new List<string>();
            var current = string.Empty;
            var i = 0;

            while (i < input.Length)
            {
                // Check if we have enough characters for " || "
                if (i + 4 <= input.Length && input.Substring(i, 4) == " || ")
                {
                    // Check if escaped
                    if (i > 0 && input[i - 1] == EscapeChar)
                    {
                        current += " || ";
                        i += 4;
                    }
                    else
                    {
                        // Found unescaped OR separator
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            result.Add(current.Trim());
                        }
                        current = string.Empty;
                        i += 4;
                    }
                }
                // Also check for || without spaces
                else if (i + 2 <= input.Length && input.Substring(i, 2) == "||")
                {
                    // Check if escaped
                    if (i > 0 && input[i - 1] == EscapeChar)
                    {
                        current += "||";
                        i += 2;
                    }
                    else
                    {
                        // Found unescaped OR separator
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            result.Add(current.Trim());
                        }
                        current = string.Empty;
                        i += 2;
                    }
                }
                else
                {
                    current += input[i];
                    i++;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current.Trim());
            }

            return result.Count > 0 ? result : new List<string> { input };
        }

        /// <summary>
        /// Split by comma, respecting escape sequences
        /// </summary>
        private List<string> SplitByComma(string input)
        {
            var result = new List<string>();
            var current = string.Empty;

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == Comma)
                {
                    // Check if escaped
                    if (i > 0 && input[i - 1] == EscapeChar)
                    {
                        current += Comma;
                    }
                    else
                    {
                        // Found unescaped comma
                        if (!string.IsNullOrWhiteSpace(current))
                        {
                            result.Add(current.Trim());
                        }
                        current = string.Empty;
                    }
                }
                else
                {
                    current += input[i];
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
            {
                result.Add(current.Trim());
            }

            return result.Count > 0 ? result : new List<string> { input };
        }

        /// <summary>
        /// Convert expanded filter groups (strings) to FilterTerm objects
        /// </summary>
        private List<List<TFilterTerm>> ConvertToFilterTerms(List<List<string>> filterGroups)
        {
            var result = new List<List<TFilterTerm>>();

            foreach (var group in filterGroups)
            {
                var filterTerms = new List<TFilterTerm>();

                foreach (var filter in group)
                {
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        continue;
                    }

                    var filterTerm = new TFilterTerm { Filter = filter.Trim() };
                    filterTerms.Add(filterTerm);
                }

                if (filterTerms.Count > 0)
                {
                    result.Add(filterTerms);
                }
            }

            return result.Count > 0 ? result : null;
        }
    }
}

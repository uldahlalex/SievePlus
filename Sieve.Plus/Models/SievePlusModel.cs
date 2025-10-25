using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Sieve.Plus.Models
{
    public class SievePlusModel : SievePlusModel<FilterTerm, SortTerm> { }

    [DataContract]
    public class SievePlusModel<TFilterTerm, TSortTerm> : ISievePlusModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {
        /// <summary>
        /// Pattern used to split filters and sorts by comma.
        /// </summary>
        private const string EscapedCommaPattern = @"(?<!($|[^\\])(\\\\)*?\\),\s*";

        /// <summary>
        /// Escaped comma e.g. used in filter filter string.
        /// </summary>
        private const string EscapedComma = @"\,";

        /// <summary>
        /// Pattern used to split OR filter groups by double pipe (||).
        /// Negative lookbehind ensures we don't split on escaped || or single pipes.
        /// </summary>
        private const string OrSeparatorPattern = @"(?<!\\)\|\|(?!\\)";

        [DataMember]
        public string Filters { get; set; }

        [DataMember]
        public string Sorts { get; set; }

        [DataMember, Range(1, int.MaxValue)]
        public int? Page { get; set; }

        [DataMember, Range(1, int.MaxValue)]
        public int? PageSize { get; set; }

        public List<TFilterTerm> GetFiltersParsed()
        {
            if (Filters != null)
            {
                var value = new List<TFilterTerm>();
                foreach (var filter in Regex.Split(Filters, EscapedCommaPattern))
                {
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        continue;
                    }

                    var filterValue = filter.Replace(EscapedComma, ",");

                    if (filter.StartsWith("("))
                    {
                        var lastParenIndex = filterValue.LastIndexOf(")", StringComparison.Ordinal) + 1;
                        var filterOpAndVal = filterValue.Substring(lastParenIndex);
                        var subFilters = filterValue.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
                        var filterTerm = new TFilterTerm
                        {
                            Filter = subFilters + filterOpAndVal
                        };
                        value.Add(filterTerm);
                    }
                    else
                    {
                        var filterTerm = new TFilterTerm
                        {
                            Filter = filterValue
                        };
                        value.Add(filterTerm);
                    }
                }
                return value;
            }
            else
            {
                return null;
            }
        }

        public List<List<TFilterTerm>> GetFiltersWithOrParsed()
        {
            if (Filters == null)
            {
                return null;
            }

            // Check if the filter string contains OR separator
            if (!Regex.IsMatch(Filters, OrSeparatorPattern))
            {
                // No OR logic, return single group with all filters (backward compatible)
                var parsed = GetFiltersParsed();
                return parsed == null ? null : new List<List<TFilterTerm>> { parsed };
            }

            // Split by OR separator (||)
            var orGroups = Regex.Split(Filters, OrSeparatorPattern);
            var result = new List<List<TFilterTerm>>();

            foreach (var orGroup in orGroups)
            {
                if (string.IsNullOrWhiteSpace(orGroup))
                {
                    continue;
                }

                var groupFilters = new List<TFilterTerm>();

                // Each OR group can contain multiple AND filters (comma-separated)
                foreach (var filter in Regex.Split(orGroup.Trim(), EscapedCommaPattern))
                {
                    if (string.IsNullOrWhiteSpace(filter))
                    {
                        continue;
                    }

                    var filterValue = filter.Replace(EscapedComma, ",");

                    if (filter.StartsWith("("))
                    {
                        var lastParenIndex = filterValue.LastIndexOf(")", StringComparison.Ordinal) + 1;
                        var filterOpAndVal = filterValue.Substring(lastParenIndex);
                        var subFilters = filterValue.Replace(filterOpAndVal, "").Replace("(", "").Replace(")", "");
                        var filterTerm = new TFilterTerm
                        {
                            Filter = subFilters + filterOpAndVal
                        };
                        groupFilters.Add(filterTerm);
                    }
                    else
                    {
                        var filterTerm = new TFilterTerm
                        {
                            Filter = filterValue
                        };
                        groupFilters.Add(filterTerm);
                    }
                }

                if (groupFilters.Count > 0)
                {
                    result.Add(groupFilters);
                }
            }

            return result.Count > 0 ? result : null;
        }

        public List<TSortTerm> GetSortsParsed()
        {
            if (Sorts == null)
            {
                return null;
            }

            var value = new List<TSortTerm>();
            foreach (var sort in Regex.Split(Sorts, EscapedCommaPattern))
            {
                if (string.IsNullOrWhiteSpace(sort)) continue;

                var sortTerm = new TSortTerm
                {
                    Sort = sort
                };

                if (value.All(s => s.Name != sortTerm.Name))
                {
                    value.Add(sortTerm);
                }
            }

            return value;
        }
    }
}

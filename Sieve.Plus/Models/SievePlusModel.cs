using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Sieve.Plus.Models
{
    public class SievePlusModel : SievePlusModel<FilterTerm, SortTerm> { }

    /// <summary>
    /// Type-safe SievePlusModel that is aware of the query model it represents.
    /// This provides compile-time safety by ensuring the correct query model is used with the correct service methods.
    /// </summary>
    /// <typeparam name="TQueryModel">The query model type that defines the filterable/sortable properties</typeparam>
    public class SievePlusModel<TQueryModel> : SievePlusModel
        where TQueryModel : ISievePlusQueryModel
    {
    }

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

            // Use the new FilterParser for enhanced parentheses support
            var parser = new Services.FilterParser<TFilterTerm>();
            return parser.Parse(Filters);
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

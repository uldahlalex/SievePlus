using System.Collections.Generic;

namespace Sieve.Plus.Models
{
    public interface ISievePlusModel : ISievePlusModel<IFilterTerm, ISortTerm>
    {

    }

    public interface ISievePlusModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm
        where TSortTerm : ISortTerm
    {
        string Filters { get; set; }

        string Sorts { get; set; }

        int? Page { get; set; }

        int? PageSize { get; set; }

        List<TFilterTerm> GetFiltersParsed();

        List<TSortTerm> GetSortsParsed();

        /// <summary>
        /// Gets filter terms grouped by OR logic.
        /// Each list represents a group of AND conditions, and groups are combined with OR.
        /// Example: "Title@=A || Title@=B, Status==Active" returns [[Title@=A], [Title@=B, Status==Active]]
        /// Result: (Title@=A) OR (Title@=B AND Status==Active)
        /// </summary>
        List<List<TFilterTerm>> GetFiltersWithOrParsed();
    }
}

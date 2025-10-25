using System.Linq;
using Sieve.Plus.Models;

namespace Sieve.Plus.Services
{
    public interface ISievePlusProcessor : ISievePlusProcessor<SievePlusModel, FilterTerm, SortTerm>
    {

    }

    public interface ISievePlusProcessor<TFilterTerm, TSortTerm> : ISievePlusProcessor<SievePlusModel<TFilterTerm, TSortTerm>, TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()
    {

    }

    public interface ISievePlusProcessor<TSieveModel, TFilterTerm, TSortTerm>
        where TSieveModel : class, ISievePlusModel<TFilterTerm, TSortTerm>
        where TFilterTerm : IFilterTerm, new()
        where TSortTerm : ISortTerm, new()

    {
        IQueryable<TEntity> Apply<TEntity>(
            TSieveModel model,
            IQueryable<TEntity> source,
            object[] dataForCustomMethods = null,
            bool applyFiltering = true,
            bool applySorting = true,
            bool applyPagination = true);
    }
}

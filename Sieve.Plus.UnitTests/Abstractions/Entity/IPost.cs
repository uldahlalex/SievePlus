using Sieve.Plus.Attributes;
using Sieve.Plus.UnitTests.Entities;

namespace Sieve.Plus.UnitTests.Abstractions.Entity
{
    public interface IPost: IBaseEntity
    {
        [SievePlus(CanFilter = true, CanSort = true)]
        string Title { get; set; }
        [SievePlus(CanFilter = true, CanSort = true)]
        int LikeCount { get; set; }
        [SievePlus(CanFilter = true, CanSort = true)]
        int CommentCount { get; set; }
        [SievePlus(CanFilter = true, CanSort = true)]
        int? CategoryId { get; set; }
        [SievePlus(CanFilter = true, CanSort = true)]
        bool IsDraft { get; set; }
        string ThisHasNoAttribute { get; set; }
        string ThisHasNoAttributeButIsAccessible { get; set; }
        int OnlySortableViaFluentApi { get; set; }
        Comment TopComment { get; set; }
        Comment FeaturedComment { get; set; }
    }
}

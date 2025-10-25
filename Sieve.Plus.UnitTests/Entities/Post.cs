using System;
using Sieve.Plus.Attributes;
using Sieve.Plus.UnitTests.Abstractions.Entity;

namespace Sieve.Plus.UnitTests.Entities
{
    public class Post : BaseEntity, IPost
    {

        [SievePlus(CanFilter = true, CanSort = true)]
        public string Title { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);

        [SievePlus(CanFilter = true, CanSort = true)]
        public int LikeCount { get; set; } = new Random().Next(0, 1000);

        [SievePlus(CanFilter = true, CanSort = true)]
        public int CommentCount { get; set; } = new Random().Next(0, 1000);

        [SievePlus(CanFilter = true, CanSort = true)]
        public int? CategoryId { get; set; } = new Random().Next(0, 4);

        [SievePlus(CanFilter = true, CanSort = true)]
        public bool IsDraft { get; set; }

        public string ThisHasNoAttribute { get; set; }

        public string ThisHasNoAttributeButIsAccessible { get; set; }

        public int OnlySortableViaFluentApi { get; set; }

        public Comment TopComment { get; set; }
        public Comment FeaturedComment { get; set; }
    }
}

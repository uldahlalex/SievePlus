using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Sieve.Plus.UnitTests.Abstractions.Entity;
using Sieve.Plus.UnitTests.Entities;

namespace Sieve.Plus.UnitTests.Services
{
    public class ApplicationSievePlusProcessor : SievePlusProcessor
    {
        public ApplicationSievePlusProcessor(
            IOptions<SievePlusOptions> options,
            ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomSortMethods, plusCustomFilterMethods)
        {
        }

        protected override SievePlusPropertyMapper MapProperties(SievePlusPropertyMapper mapper)
        {
            mapper.Property<Post>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

            mapper.Property<Post>(p => p.TopComment.Text)
                .CanFilter();

            mapper.Property<Post>(p => p.TopComment.Id)
                .CanSort();

            mapper.Property<Post>(p => p.OnlySortableViaFluentApi)
                .CanSort();

            mapper.Property<Post>(p => p.TopComment.Text)
                .CanFilter()
                .HasName("topc");

            mapper.Property<Post>(p => p.FeaturedComment.Text)
                .CanFilter()
                .HasName("featc");

            mapper
                .Property<Post>(p => p.DateCreated)
                .CanSort()
                .HasName("CreateDate");

            // interfaces
            mapper.Property<IPost>(p => p.ThisHasNoAttributeButIsAccessible)
                .CanSort()
                .CanFilter()
                .HasName("shortname");

            mapper.Property<IPost>(p => p.TopComment.Text)
                .CanFilter();

            mapper.Property<IPost>(p => p.TopComment.Id)
                .CanSort();

            mapper.Property<IPost>(p => p.OnlySortableViaFluentApi)
                .CanSort();

            mapper.Property<IPost>(p => p.TopComment.Text)
                .CanFilter()
                .HasName("topc");

            mapper.Property<IPost>(p => p.FeaturedComment.Text)
                .CanFilter()
                .HasName("featc");

            mapper
                .Property<IPost>(p => p.DateCreated)
                .CanSort()
                .HasName("CreateDate");

            return mapper;
        }
    }
}

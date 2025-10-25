using System.Collections.Generic;
using System.Linq;
using Sieve.Plus.Exceptions;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Sieve.Plus.UnitTests.Entities;
using Sieve.Plus.UnitTests.Services;
using Xunit;

namespace Sieve.Plus.UnitTests
{
    public class Mapper
    {
        private readonly IQueryable<Post> _posts;

        public Mapper()
        {
            _posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    ThisHasNoAttributeButIsAccessible = "A",
                    ThisHasNoAttribute = "A",
                    OnlySortableViaFluentApi = 100
                },
                new Post
                {
                    Id = 2,
                    ThisHasNoAttributeButIsAccessible = "B",
                    ThisHasNoAttribute = "B",
                    OnlySortableViaFluentApi = 50
                },
                new Post
                {
                    Id = 3,
                    ThisHasNoAttributeButIsAccessible = "C",
                    ThisHasNoAttribute = "C",
                    OnlySortableViaFluentApi = 0
                },
            }.AsQueryable();
        }

        /// <summary>
        /// Processors with the same mappings but configured via a different method.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<object[]> GetProcessors()
        {
            yield return new object[] { 
                new ApplicationSievePlusProcessor(
                    new SieveOptionsAccessor(),
                    new SievePlusCustomSortMethods(),
                    new SievePlusCustomFilterMethods())};
            yield return new object[] { 
                new ModularConfigurationSievePlusProcessor(
                    new SieveOptionsAccessor(),
                    new SievePlusCustomSortMethods(),
                    new SievePlusCustomFilterMethods())};
            yield return new object[] { 
                new ModularConfigurationWithScanSievePlusProcessor(
                    new SieveOptionsAccessor(),
                    new SievePlusCustomSortMethods(),
                    new SievePlusCustomFilterMethods())};
        }
        
        
        [Theory]
        [MemberData(nameof(GetProcessors))]
        public void MapperWorks(ISievePlusProcessor plusProcessor)
        {
            var model = new SievePlusModel
            {
                Filters = "shortname@=A",
            };

            var result = plusProcessor.Apply(model, _posts);

            Assert.Equal("A", result.First().ThisHasNoAttributeButIsAccessible);

            Assert.True(result.Count() == 1);
        }

        [Theory]
        [MemberData(nameof(GetProcessors))]
        public void MapperSortOnlyWorks(ISievePlusProcessor plusProcessor)
        {
            var model = new SievePlusModel
            {
                Filters = "OnlySortableViaFluentApi@=50",
                Sorts = "OnlySortableViaFluentApi"
            };

            var result = plusProcessor.Apply(model, _posts, applyFiltering: false, applyPagination: false);

            Assert.Throws<SievePlusMethodNotFoundException>(() => plusProcessor.Apply(model, _posts));

            Assert.Equal(3, result.First().Id);

            Assert.True(result.Count() == 3);
        }
    }
}

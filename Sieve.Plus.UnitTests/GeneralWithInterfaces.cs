using System;
using System.Collections.Generic;
using System.Linq;
using Sieve.Plus.Exceptions;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Sieve.Plus.UnitTests.Abstractions.Entity;
using Sieve.Plus.UnitTests.Entities;
using Sieve.Plus.UnitTests.Services;
using Xunit;
using Xunit.Abstractions;

namespace Sieve.Plus.UnitTests
{
    public class GeneralWithInterfaces
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly SievePlusProcessor _plusProcessor;
        private readonly SievePlusProcessor _nullablePlusProcessor;
        private readonly IQueryable<IPost> _posts;
        private readonly IQueryable<Comment> _comments;

        public GeneralWithInterfaces(ITestOutputHelper testOutputHelper)
        {
            var nullableAccessor = new SieveOptionsAccessor();
            nullableAccessor.Value.IgnoreNullsOnNotEqual = false;

            _testOutputHelper = testOutputHelper;
            _plusProcessor = new ApplicationSievePlusProcessor(new SieveOptionsAccessor(),
                new SievePlusCustomSortMethods(),
                new SievePlusCustomFilterMethods());

            _nullablePlusProcessor = new ApplicationSievePlusProcessor(nullableAccessor,
                new SievePlusCustomSortMethods(),
                new SievePlusCustomFilterMethods());

            _posts = new List<IPost>
            {
                new Post
                {
                    Id = 0,
                    Title = "A",
                    LikeCount = 100,
                    IsDraft = true,
                    CategoryId = null,
                    TopComment = new Comment { Id = 0, Text = "A1" },
                    FeaturedComment = new Comment { Id = 4, Text = "A2" }
                },
                new Post
                {
                    Id = 1,
                    Title = "B",
                    LikeCount = 50,
                    IsDraft = false,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 3, Text = "B1" },
                    FeaturedComment = new Comment { Id = 5, Text = "B2" }
                },
                new Post
                {
                    Id = 2,
                    Title = "C",
                    LikeCount = 0,
                    CategoryId = 1,
                    TopComment = new Comment { Id = 2, Text = "C1" },
                    FeaturedComment = new Comment { Id = 6, Text = "C2" }
                },
                new Post
                {
                    Id = 3,
                    Title = "D",
                    LikeCount = 3,
                    IsDraft = true,
                    CategoryId = 2,
                    TopComment = new Comment { Id = 1, Text = "D1" },
                    FeaturedComment = new Comment { Id = 7, Text = "D2" }
                },
            }.AsQueryable();

            _comments = new List<Comment>
            {
                new Comment
                {
                    Id = 0,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-20),
                    Text = "This is an old comment."
                },
                new Comment
                {
                    Id = 1,
                    DateCreated = DateTimeOffset.UtcNow.AddDays(-1),
                    Text = "This is a fairly new comment."
                },
                new Comment
                {
                    Id = 2,
                    DateCreated = DateTimeOffset.UtcNow,
                    Text = "This is a brand new comment. (Text in braces)"
                },
            }.AsQueryable();
        }

        [Fact]
        public void ContainsCanBeCaseInsensitive()
        {
            var model = new SievePlusModel
            {
                Filters = "Title@=*a"
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.Equal(0, result.First().Id);
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void NotEqualsCanBeCaseInsensitive()
        {
            var model = new SievePlusModel
            {
                Filters = "Title!=*a"
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.Equal(1, result.First().Id);
            Assert.True(result.Count() == 3);
        }

        [Fact]
        public void ContainsIsCaseSensitive()
        {
            var model = new SievePlusModel
            {
                Filters = "Title@=a",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(!result.Any());
        }

        [Fact]
        public void NotContainsWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "Title!@=D",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Count() == 3);
        }

        [Fact]
        public void CanFilterBools()
        {
            var model = new SievePlusModel
            {
                Filters = "IsDraft==false"
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Count() == 2);
        }

        [Fact]
        public void CanSortBools()
        {
            var model = new SievePlusModel
            {
                Sorts = "-IsDraft"
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.Equal(0, result.First().Id);
        }

        [Fact]
        public void CanFilterNullableInts()
        {
            var model = new SievePlusModel
            {
                Filters = "CategoryId==1"
            };

            var result = _plusProcessor.Apply(model, _posts);
            var nullableResult = _nullablePlusProcessor.Apply(model, _posts);

            Assert.True(result.Count() == 2);
            Assert.True(nullableResult.Count() == 2);
        }

        [Fact]
        public void CanFilterNullableIntsWithNotEqual()
        {
            var model = new SievePlusModel()
            {
                Filters = "CategoryId!=1"
            };

            var result = _plusProcessor.Apply(model, _posts);
            var nullableResult = _nullablePlusProcessor.Apply(model, _posts);

            Assert.True(result.Count() == 1);
            Assert.True(nullableResult.Count() == 2);
        }

        [Fact]
        public void EqualsDoesntFailWithNonStringTypes()
        {
            var model = new SievePlusModel
            {
                Filters = "LikeCount==50",
            };

            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Values.ToString());
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].Operator);
            _testOutputHelper.WriteLine(model.GetFiltersParsed()[0].OperatorParsed.ToString());

            var result = _plusProcessor.Apply(model, _posts);

            Assert.Equal(1, result.First().Id);
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersWork()
        {
            var model = new SievePlusModel
            {
                Filters = "Isnew",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.False(result.Any(p => p.Id == 0));
            Assert.True(result.Count() == 3);
        }

        [Fact]
        public void CustomGenericFiltersWork()
        {
            var model = new SievePlusModel
            {
                Filters = "Latest",
            };

            var result = _plusProcessor.Apply(model, _comments);

            Assert.False(result.Any(p => p.Id == 0));
            Assert.True(result.Count() == 2);
        }

        [Fact]
        public void CustomFiltersWithOperatorsWork()
        {
            var model = new SievePlusModel
            {
                Filters = "HasInTitle==A",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Any(p => p.Id == 0));
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersMixedWithUsualWork1()
        {
            var model = new SievePlusModel
            {
                Filters = "Isnew,CategoryId==2",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Any(p => p.Id == 3));
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersMixedWithUsualWork2()
        {
            var model = new SievePlusModel
            {
                Filters = "CategoryId==2,Isnew",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Any(p => p.Id == 3));
            Assert.True(result.Count() == 1);
        }

        [Fact]
        public void CustomFiltersOnDifferentSourcesCanShareName()
        {
            var postModel = new SievePlusModel
            {
                Filters = "CategoryId==2,Isnew",
            };

            var postResult = _plusProcessor.Apply(postModel, _posts);

            Assert.True(postResult.Any(p => p.Id == 3));
            Assert.Equal(1, postResult.Count());

            var commentModel = new SievePlusModel
            {
                Filters = "Isnew",
            };

            var commentResult = _plusProcessor.Apply(commentModel, _comments);

            Assert.True(commentResult.Any(c => c.Id == 2));
            Assert.Equal(2, commentResult.Count());
        }

        [Fact]
        public void CustomSortsWork()
        {
            var model = new SievePlusModel
            {
                Sorts = "Popularity",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.False(result.First().Id == 0);
        }

        [Fact]
        public void CustomGenericSortsWork()
        {
            var model = new SievePlusModel
            {
                Sorts = "Oldest",
            };

            var result = _plusProcessor.Apply(model, _posts);

            Assert.True(result.Last().Id == 0);
        }

        [Fact]
        public void MethodNotFoundExceptionWork()
        {
            var model = new SievePlusModel
            {
                Filters = "does not exist",
            };

            Assert.Throws<SievePlusMethodNotFoundException>(() => _plusProcessor.Apply(model, _posts));
        }

        [Fact]
        public void IncompatibleMethodExceptionsWork()
        {
            var model = new SievePlusModel
            {
                Filters = "TestComment",
            };

            Assert.Throws<SievePlusIncompatibleMethodException>(() => _plusProcessor.Apply(model, _posts));
        }

        [Fact]
        public void OrNameFilteringWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "(Title|LikeCount)==3",
            };

            var result = _plusProcessor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.NotNull(entry);
            Assert.Equal(1, resultCount);
            Assert.Equal(3, entry.Id);
        }

        [Theory]
        [InlineData("CategoryId==1,(CategoryId|LikeCount)==50")]
        [InlineData("(CategoryId|LikeCount)==50,CategoryId==1")]
        public void CombinedAndOrFilterIndependentOfOrder(string filter)
        {
            var model = new SievePlusModel
            {
                Filters = filter,
            };

            var result = _plusProcessor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.NotNull(entry);
            Assert.Equal(1, resultCount);
        }

        [Fact]
        public void CombinedAndOrWithSpaceFilteringWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "Title==D, (Title|LikeCount)==3",
            };

            var result = _plusProcessor.Apply(model, _posts);
            var entry = result.FirstOrDefault();
            var resultCount = result.Count();

            Assert.NotNull(entry);
            Assert.Equal(1, resultCount);
            Assert.Equal(3, entry.Id);
        }

        [Fact]
        public void OrValueFilteringWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "Title==C|D",
            };

            var result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(2, result.Count());
            Assert.True(result.Any(p => p.Id == 2));
            Assert.True(result.Any(p => p.Id == 3));
        }

        [Fact]
        public void OrValueFilteringWorks2()
        {
            var model = new SievePlusModel
            {
                Filters = "Text@=(|)",
            };

            var result = _plusProcessor.Apply(model, _comments);
            Assert.Equal(1, result.Count());
            Assert.Equal(2, result.FirstOrDefault()?.Id);
        }

        [Fact]
        public void NestedFilteringWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "TopComment.Text!@=A",
            };

            var result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(3, result.Count());
            var posts = result.ToList();
            Assert.Contains("B", posts[0].TopComment.Text);
            Assert.Contains("C", posts[1].TopComment.Text);
            Assert.Contains("D", posts[2].TopComment.Text);
        }

        [Fact]
        public void NestedSortingWorks()
        {
            var model = new SievePlusModel
            {
                Sorts = "TopComment.Id",
            };

            var result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(4, result.Count());
            var posts = result.ToList();
            Assert.Equal(0, posts[0].Id);
            Assert.Equal(3, posts[1].Id);
            Assert.Equal(2, posts[2].Id);
            Assert.Equal(1, posts[3].Id);
        }

        [Fact]
        public void NestedFilteringWithIdenticTypesWorks()
        {
            var model = new SievePlusModel
            {
                Filters = "(topc|featc)@=*2",
            };

            var result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(4, result.Count());

            model = new SievePlusModel
            {
                Filters = "(topc|featc)@=*B",
            };

            result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(1, result.Count());
        }

        [Fact]
        public void FilteringNullsWorks()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
            }.AsQueryable();

            var model = new SievePlusModel
            {
                Filters = "FeaturedComment.Text!@=Some value",
            };

            var result = _plusProcessor.Apply(model, posts);
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void SortingNullsWorks()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment =  new Comment { Id = 1 },
                    FeaturedComment = null
                },
                new Post
                {
                    Id = 2,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
            }.AsQueryable();

            var model = new SievePlusModel
            {
                Sorts = "TopComment.Id",
            };

            var result = _plusProcessor.Apply(model, posts);
            Assert.Equal(2, result.Count());
            var sortedPosts = result.ToList();
            Assert.Equal(2, sortedPosts[0].Id);
            Assert.Equal(1, sortedPosts[1].Id);
        }

        [Fact]
        public void FilteringOnNullWorks()
        {
            var posts = new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = null
                },
                new Post
                {
                    Id = 2,
                    Title = null,
                    LikeCount = 0,
                    IsDraft = false,
                    CategoryId = null,
                    TopComment = null,
                    FeaturedComment = new Comment {  Id = 1, Text = null }
                },
            }.AsQueryable();

            var model = new SievePlusModel
            {
                Filters = "FeaturedComment.Text==null",
            };

            var result = _plusProcessor.Apply(model, posts);
            Assert.Equal(1, result.Count());
            var filteredPosts = result.ToList();
            Assert.Equal(2, filteredPosts[0].Id);
        }

        [Fact]
        public void BaseDefinedPropertyMappingSortingWorks_WithCustomName()
        {
            var model = new SievePlusModel
            {
                Sorts = "-CreateDate"
            };

            var result = _plusProcessor.Apply(model, _posts);
            Assert.Equal(4, result.Count());

            var posts = result.ToList();
            Assert.Equal(3,posts[0].Id);
            Assert.Equal(2,posts[1].Id);
            Assert.Equal(1,posts[2].Id);
            Assert.Equal(0,posts[3].Id);
        }
    }
}

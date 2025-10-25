using System.Collections.Generic;
using System.Linq;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Sieve.Plus.UnitTests.Entities;
using Sieve.Plus.UnitTests.Services;
using Xunit;
using Xunit.Abstractions;

namespace Sieve.Plus.UnitTests
{
    public class ParenthesesFilterTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly SievePlusProcessor _processor;
        private readonly IQueryable<Post> _posts;

        public ParenthesesFilterTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _processor = new ApplicationSievePlusProcessor(
                new SieveOptionsAccessor(),
                new SievePlusCustomSortMethods(),
                new SievePlusCustomFilterMethods());

            // Create test data with varying categories and like counts
            _posts = new List<Post>
            {
                new Post { Id = 1, Title = "Post1", CategoryId = 1, LikeCount = 50, IsDraft = false },
                new Post { Id = 2, Title = "Post2", CategoryId = 1, LikeCount = 150, IsDraft = false },
                new Post { Id = 3, Title = "Post3", CategoryId = 2, LikeCount = 75, IsDraft = false },
                new Post { Id = 4, Title = "Post4", CategoryId = 2, LikeCount = 200, IsDraft = false },
                new Post { Id = 5, Title = "Post5", CategoryId = 3, LikeCount = 25, IsDraft = true },
                new Post { Id = 6, Title = "Post6", CategoryId = 3, LikeCount = 300, IsDraft = true },
            }.AsQueryable();
        }

        [Fact]
        public void Parentheses_SimpleOrGroupWithSharedConstraint()
        {
            // (CategoryId==1 || CategoryId==2),LikeCount>100
            // Should return: Posts with category 1 or 2, AND like count > 100
            // Expected: Post2 (cat 1, 150 likes), Post4 (cat 2, 200 likes)
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==2),LikeCount>100"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 2); // Category 1, 150 likes
            Assert.Contains(posts, p => p.Id == 4); // Category 2, 200 likes
        }

        [Fact]
        public void Parentheses_OrGroupBeforeAndConstraint()
        {
            // (CategoryId==1 || CategoryId==3),IsDraft==false
            // Should return: Posts with category 1 or 3, AND not draft
            // Expected: Post1, Post2 (category 1, not draft)
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==3),IsDraft==false"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 1);
            Assert.Contains(posts, p => p.Id == 2);
            Assert.All(posts, p => Assert.False(p.IsDraft));
        }

        [Fact]
        public void Parentheses_MultipleSharedConstraints()
        {
            // (CategoryId==1 || CategoryId==2),LikeCount>100,IsDraft==false
            // Posts with (category 1 or 2) AND like count > 100 AND not draft
            // Expected: Post2, Post4
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==2),LikeCount>100,IsDraft==false"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 2);
            Assert.Contains(posts, p => p.Id == 4);
        }

        [Fact]
        public void Parentheses_ConstraintBeforeOrGroup()
        {
            // LikeCount>100,(CategoryId==1 || CategoryId==2)
            // Same as previous but different order
            var model = new SievePlusModel
            {
                Filters = "LikeCount>100,(CategoryId==1 || CategoryId==2)"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 2);
            Assert.Contains(posts, p => p.Id == 4);
        }

        [Fact]
        public void Parentheses_MultipleOrGroupsInSameFilter()
        {
            // (CategoryId==1 || CategoryId==2),(LikeCount>100 || IsDraft==true)
            // (Category 1 or 2) AND (LikeCount > 100 OR Draft)
            // This should expand to 4 combinations:
            // 1. CategoryId==1,LikeCount>100
            // 2. CategoryId==1,IsDraft==true
            // 3. CategoryId==2,LikeCount>100
            // 4. CategoryId==2,IsDraft==true
            // Expected: Post2 (cat1, 150 likes), Post4 (cat2, 200 likes)
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==2),(LikeCount>100 || IsDraft==true)"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            // Post1: cat1, 50 likes, not draft - NO (doesn't meet either LikeCount>100 or IsDraft)
            // Post2: cat1, 150 likes, not draft - YES (meets LikeCount>100)
            // Post3: cat2, 75 likes, not draft - NO (wrong category)
            // Post4: cat2, 200 likes, not draft - YES (meets LikeCount>100)
            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 2);
            Assert.Contains(posts, p => p.Id == 4);
        }

        [Fact]
        public void Parentheses_ThreeWayOr()
        {
            // (CategoryId==1 || CategoryId==2 || CategoryId==3)
            // All posts with category 1, 2, or 3
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==2 || CategoryId==3)"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(6, posts.Count); // All posts
        }

        [Fact]
        public void Parentheses_ComplexCartesianProduct()
        {
            // (CategoryId==1 || CategoryId==2),(LikeCount>50 || LikeCount<30),IsDraft==false
            // This creates:
            // (cat1, likes>50, not draft) OR (cat1, likes<30, not draft) OR
            // (cat2, likes>50, not draft) OR (cat2, likes<30, not draft)
            // Expected: Post2 (cat1, 150), Post3 (cat2, 75), Post4 (cat2, 200)
            var model = new SievePlusModel
            {
                Filters = "(CategoryId==1 || CategoryId==2),(LikeCount>50 || LikeCount<30),IsDraft==false"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(3, posts.Count);
            Assert.Contains(posts, p => p.Id == 2); // cat1, 150 likes, not draft (meets likes>50)
            Assert.Contains(posts, p => p.Id == 3); // cat2, 75 likes, not draft (meets likes>50)
            Assert.Contains(posts, p => p.Id == 4); // cat2, 200 likes, not draft (meets likes>50)
        }

        [Fact]
        public void BackwardCompatibility_NoParentheses_SimpleOr()
        {
            // CategoryId==1 || CategoryId==2
            // Should work exactly as before
            var model = new SievePlusModel
            {
                Filters = "CategoryId==1 || CategoryId==2"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(4, posts.Count);
            Assert.All(posts, p => Assert.True(p.CategoryId == 1 || p.CategoryId == 2));
        }

        [Fact]
        public void BackwardCompatibility_NoParentheses_OrWithAnd()
        {
            // CategoryId==1,LikeCount>100 || CategoryId==2,LikeCount>100
            // Traditional syntax - should still work
            var model = new SievePlusModel
            {
                Filters = "CategoryId==1,LikeCount>100 || CategoryId==2,LikeCount>100"
            };

            var result = _processor.Apply(model, _posts);
            var posts = result.ToList();

            Assert.Equal(2, posts.Count);
            Assert.Contains(posts, p => p.Id == 2);
            Assert.Contains(posts, p => p.Id == 4);
        }

        [Fact]
        public void FilterParser_ParseSimpleFilter()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("CategoryId==1");

            Assert.NotNull(result);
            Assert.Single(result); // One OR group
            Assert.Single(result[0]); // One filter in the group
            Assert.Equal("CategoryId", result[0][0].Names[0]);
        }

        [Fact]
        public void FilterParser_ParseSimpleOr()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("CategoryId==1 || CategoryId==2");

            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // Two OR groups
            Assert.Single(result[0]); // One filter in each group
            Assert.Single(result[1]);
        }

        [Fact]
        public void FilterParser_ParseParenthesesWithSharedConstraint()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("(CategoryId==1 || CategoryId==2),LikeCount>100");

            Assert.NotNull(result);
            // This should expand to: [[CategoryId==1, LikeCount>100], [CategoryId==2, LikeCount>100]]
            Assert.Equal(2, result.Count); // Two OR groups (Cartesian product)
            Assert.Equal(2, result[0].Count); // Each group has 2 filters
            Assert.Equal(2, result[1].Count);
        }

        [Fact]
        public void FilterParser_ParseMultipleParenthesizedGroups()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("(CategoryId==1 || CategoryId==2),(LikeCount>100 || LikeCount<30)");

            Assert.NotNull(result);
            // Cartesian product: 2 * 2 = 4 groups
            Assert.Equal(4, result.Count);
            Assert.All(result, group => Assert.Equal(2, group.Count)); // Each has 2 filters
        }

        [Fact]
        public void FilterParser_ParseMixedParenthesesAndPlainFilters()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("IsDraft==false,(CategoryId==1 || CategoryId==2),LikeCount>100");

            Assert.NotNull(result);
            // IsDraft AND (Cat1 OR Cat2) AND LikeCount
            // Should expand to: [[IsDraft==false, CategoryId==1, LikeCount>100],
            //                    [IsDraft==false, CategoryId==2, LikeCount>100]]
            Assert.Equal(2, result.Count);
            Assert.All(result, group => Assert.Equal(3, group.Count));
        }

        [Fact]
        public void FilterParser_EmptyFilter_ReturnsNull()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse("");

            Assert.Null(result);
        }

        [Fact]
        public void FilterParser_NullFilter_ReturnsNull()
        {
            var parser = new FilterParser<FilterTerm>();
            var result = parser.Parse(null);

            Assert.Null(result);
        }
    }
}

// using api;
// using api.Services;
// using dataccess;
// using Sieve.Plus.QueryBuilder;
//
// namespace tests;
//
// /// <summary>
// /// Tests for OR filtering functionality using the Sieve.Plus fork with OR support.
// /// These tests use dependency injection with Entity Framework to verify OR operations work end-to-end.
// /// </summary>
// public class OrFilteringTests(ILibraryService libraryService,
//     MyDbContext ctx,
//     ITestOutputHelper outputHelper,
//     ISeeder seeder)
// {
//     [Fact]
//     public async Task FilterAuthors_ByNameOrCondition_SimpleOr()
//     {
//         await seeder.Seed();
//
//         // Get two different authors
//         var authors = ctx.Authors
//             .OrderBy(a => Guid.NewGuid())
//             .Take(2)
//             .ToList();
//
//         var author1 = authors[0];
//         var author2 = authors[1];
//
//         // Build query: Name equals author1 OR Name equals author2
//         var builder = SievePlusQueryBuilder<Author>.Create()
//             .FilterEquals(a => a.Name, author1.Name)
//             .Or()
//             .FilterEquals(a => a.Name, author2.Name);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetAuthors(sieveModel);
//
//         // Should contain both authors
//         Assert.Contains(actual, a => a.Id == author1.Id);
//         Assert.Contains(actual, a => a.Id == author2.Id);
//         Assert.True(actual.Count >= 2, "Should have at least 2 authors");
//
//         outputHelper.WriteLine($"Found {actual.Count} authors");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterAuthors_ByNameContainsOr_MultipleOptions()
//     {
//         await seeder.Seed();
//
//         // Create query: Name contains "Bob" OR Name contains "Alice"
//         var builder = SievePlusQueryBuilder<Author>.Create()
//             .FilterContains(a => a.Name, "Bob")
//             .Or()
//             .FilterContains(a => a.Name, "Alice");
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetAuthors(sieveModel);
//
//         // All results should contain either "Bob" or "Alice" in the name
//         Assert.All(actual, author =>
//             Assert.True(
//                 author.Name.Contains("Bob") || author.Name.Contains("Alice"),
//                 $"Author {author.Name} should contain 'Bob' or 'Alice'"
//             )
//         );
//
//         outputHelper.WriteLine($"Found {actual.Count} authors matching 'Bob' or 'Alice'");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_ByTitleOr_ThreeOptions()
//     {
//         await seeder.Seed();
//
//         // Get three different books
//         var books = ctx.Books
//             .OrderBy(b => Guid.NewGuid())
//             .Take(3)
//             .ToList();
//
//         // Build query: Title equals book1 OR book2 OR book3
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterEquals(b => b.Title, books[0].Title)
//             .Or()
//             .FilterEquals(b => b.Title, books[1].Title)
//             .Or()
//             .FilterEquals(b => b.Title, books[2].Title);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // Should contain all three books
//         Assert.Contains(actual, b => b.Id == books[0].Id);
//         Assert.Contains(actual, b => b.Id == books[1].Id);
//         Assert.Contains(actual, b => b.Id == books[2].Id);
//
//         outputHelper.WriteLine($"Found {actual.Count} books");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_ByPageCountRanges_OrCondition()
//     {
//         await seeder.Seed();
//
//         // Query: (Pages < 100) OR (Pages > 500)
//         // This should get short books OR long books, excluding medium-sized ones
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterLessThan(b => b.Pages, 100)
//             .Or()
//             .FilterGreaterThan(b => b.Pages, 500);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // All results should be < 100 pages OR > 500 pages
//         Assert.All(actual, book =>
//             Assert.True(
//                 book.Pages < 100 || book.Pages > 500,
//                 $"Book {book.Title} has {book.Pages} pages, should be < 100 or > 500"
//             )
//         );
//
//         outputHelper.WriteLine($"Found {actual.Count} books with < 100 or > 500 pages");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_ComplexOrWithAnd_CPUPriceRunnerScenario()
//     {
//         await seeder.Seed();
//
//         // Simulate PriceRunner scenario: Find books matching specific criteria
//         // Query: (Title contains "C#" AND Pages > 300) OR (Title contains "JavaScript" AND Pages > 300)
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterContains(b => b.Title, "C#")
//             .FilterGreaterThan(b => b.Pages, 300)
//             .Or()
//             .FilterContains(b => b.Title, "JavaScript")
//             .FilterGreaterThan(b => b.Pages, 300);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // Each result should match one of the two conditions
//         Assert.All(actual, book =>
//         {
//             var matchesFirstCondition = book.Title.Contains("C#") && book.Pages > 300;
//             var matchesSecondCondition = book.Title.Contains("JavaScript") && book.Pages > 300;
//             Assert.True(
//                 matchesFirstCondition || matchesSecondCondition,
//                 $"Book {book.Title} with {book.Pages} pages should match one of the OR conditions"
//             );
//         });
//
//         outputHelper.WriteLine($"Found {actual.Count} books matching complex OR with AND");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_OrWithSorting_Combined()
//     {
//         await seeder.Seed();
//
//         // Query: (Pages < 100) OR (Pages > 500), sorted by Pages ascending
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterLessThan(b => b.Pages, 100)
//             .Or()
//             .FilterGreaterThan(b => b.Pages, 500)
//             .SortBy(b => b.Pages);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // Verify filtering
//         Assert.All(actual, book =>
//             Assert.True(book.Pages < 100 || book.Pages > 500)
//         );
//
//         // Verify sorting
//         var pagesList = actual.Select(b => b.Pages).ToList();
//         var sortedPages = pagesList.OrderBy(p => p).ToList();
//         Assert.Equal(sortedPages, pagesList);
//
//         outputHelper.WriteLine($"Found {actual.Count} books, sorted by pages");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//         outputHelper.WriteLine($"Sorts: {sieveModel.Sorts}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_OrWithPagination_Combined()
//     {
//         await seeder.Seed();
//
//         // Query: (Title contains "Programming") OR (Title contains "Guide")
//         // With pagination
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterContains(b => b.Title, "Programming")
//             .Or()
//             .FilterContains(b => b.Title, "Guide")
//             .Page(1)
//             .PageSize(5);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // Should return at most 5 books (page size)
//         Assert.True(actual.Count <= 5, "Should respect page size limit");
//
//         // All results should match the OR filter
//         Assert.All(actual, book =>
//             Assert.True(
//                 book.Title.Contains("Programming") || book.Title.Contains("Guide"),
//                 $"Book {book.Title} should contain 'Programming' or 'Guide'"
//             )
//         );
//
//         outputHelper.WriteLine($"Found {actual.Count} books on page 1");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//         outputHelper.WriteLine($"Page: {sieveModel.Page}, PageSize: {sieveModel.PageSize}");
//     }
//
//     [Fact]
//     public async Task FilterGenres_ByNameOr_MultipleGenres()
//     {
//         await seeder.Seed();
//
//         // Get two genres
//         var genres = ctx.Genres
//             .OrderBy(g => Guid.NewGuid())
//             .Take(2)
//             .ToList();
//
//         // Query: Name equals genre1 OR Name equals genre2
//         var builder = SievePlusQueryBuilder<Genre>.Create()
//             .FilterEquals(g => g.Name, genres[0].Name)
//             .Or()
//             .FilterEquals(g => g.Name, genres[1].Name);
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetGenres(sieveModel);
//
//         // Should contain both genres
//         Assert.Contains(actual, g => g.Id == genres[0].Id);
//         Assert.Contains(actual, g => g.Id == genres[1].Id);
//
//         outputHelper.WriteLine($"Found {actual.Count} genres");
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterAuthors_ParseExistingOrQuery_RoundTrip()
//     {
//         await seeder.Seed();
//
//         var authors = ctx.Authors
//             .OrderBy(a => Guid.NewGuid())
//             .Take(2)
//             .ToList();
//
//         // Create OR filter string manually
//         var filtersString = $"Name=={authors[0].Name} || Name=={authors[1].Name}";
//
//         // Parse it back
//         var builder = SievePlusQueryBuilder<Author>.ParseQueryString($"filters={filtersString}");
//         var sieveModel = builder.BuildSieveModel();
//
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetAuthors(sieveModel);
//
//         // Should find both authors
//         Assert.Contains(actual, a => a.Id == authors[0].Id);
//         Assert.Contains(actual, a => a.Id == authors[1].Id);
//
//         // Verify round-trip
//         Assert.Equal(filtersString, sieveModel.Filters);
//
//         outputHelper.WriteLine($"Round-trip successful");
//         outputHelper.WriteLine($"Original: {filtersString}");
//         outputHelper.WriteLine($"Rebuilt: {sieveModel.Filters}");
//     }
//
//     [Fact]
//     public async Task FilterBooks_EmptyOrGroup_IgnoresEmptyGroup()
//     {
//         await seeder.Seed();
//
//         var randomBook = ctx.Books
//             .OrderBy(b => Guid.NewGuid())
//             .First();
//
//         // Create builder with an OR but no filters after it
//         var builder = SievePlusQueryBuilder<Book>.Create()
//             .FilterEquals(b => b.Title, randomBook.Title)
//             .Or(); // Empty OR group
//
//         var sieveModel = builder.BuildSieveModel();
//         sieveModel.PrintAsJson(outputHelper);
//
//         var actual = await libraryService.GetBooks(sieveModel);
//
//         // Should only match the first filter, empty OR group ignored
//         Assert.Contains(actual, b => b.Id == randomBook.Id);
//
//         // Filter string should not contain ||
//         Assert.DoesNotContain("||", sieveModel.Filters);
//
//         outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
//         outputHelper.WriteLine("Empty OR group correctly ignored");
//     }
// }

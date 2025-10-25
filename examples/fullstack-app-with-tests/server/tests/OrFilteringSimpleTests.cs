using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

/// <summary>
/// Simple OR filtering tests that avoid DateTime issues by only testing string and numeric filters.
/// These tests verify OR functionality works end-to-end with the Sieve.Plus fork.
/// </summary>
public class OrFilteringSimpleTests(ILibraryService libraryService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterAuthors_TwoNamesWithOr_FindsBothAuthors()
    {
        await seeder.Seed();

        // Get two different authors
        var authors = ctx.Authors
            .OrderBy(a => a.Id)
            .Take(2)
            .ToList();

        var author1 = authors[0];
        var author2 = authors[1];

        outputHelper.WriteLine($"Looking for: {author1.Name} OR {author2.Name}");

        // Build query: Name equals author1 OR Name equals author2
        var builder = SievePlusQueryBuilder<Author>.Create()
            .FilterEquals(a => a.Name, author1.Name)
            .Or()
            .FilterEquals(a => a.Name, author2.Name);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetAuthors(sieveModel);

        // Should contain both authors
        Assert.Contains(actual, a => a.Id == author1.Id);
        Assert.Contains(actual, a => a.Id == author2.Id);

        outputHelper.WriteLine($"Found {actual.Count} authors");
        outputHelper.WriteLine($"✓ Test passed: OR operator successfully found both authors");
    }

    [Fact]
    public async Task FilterBooks_ThreeTitlesWithOr_FindsAllThreeBooks()
    {
        await seeder.Seed();

        // Get three different books
        var books = ctx.Books
            .OrderBy(b => b.Id)
            .Take(3)
            .ToList();

        outputHelper.WriteLine($"Looking for 3 books:");
        foreach (var book in books)
        {
            outputHelper.WriteLine($"  - {book.Title}");
        }

        // Build query: Title equals book1 OR book2 OR book3
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterEquals(b => b.Title, books[0].Title)
            .Or()
            .FilterEquals(b => b.Title, books[1].Title)
            .Or()
            .FilterEquals(b => b.Title, books[2].Title);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Should contain all three books
        Assert.Contains(actual, b => b.Id == books[0].Id);
        Assert.Contains(actual, b => b.Id == books[1].Id);
        Assert.Contains(actual, b => b.Id == books[2].Id);

        outputHelper.WriteLine($"Found {actual.Count} books");
        outputHelper.WriteLine($"✓ Test passed: Multiple OR operators work correctly");
    }

    [Fact]
    public async Task FilterBooks_PageCountRanges_SmallOrLargeBooks()
    {
        await seeder.Seed();

        // Query: (Pages < 200) OR (Pages > 800)
        // This should get short books OR long books, excluding medium-sized ones
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterLessThan(b => b.Pages, 200)
            .Or()
            .FilterGreaterThan(b => b.Pages, 800);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetBooks(sieveModel);

        // All results should be < 200 pages OR > 800 pages
        Assert.All(actual, book =>
        {
            var isSmall = book.Pages < 200;
            var isLarge = book.Pages > 800;
            Assert.True(
                isSmall || isLarge,
                $"Book '{book.Title}' has {book.Pages} pages, expected < 200 or > 800"
            );
            outputHelper.WriteLine($"  ✓ {book.Title}: {book.Pages} pages");
        });

        outputHelper.WriteLine($"Found {actual.Count} books with < 200 or > 800 pages");
        outputHelper.WriteLine($"✓ Test passed: OR with comparison operators works correctly");
    }

    [Fact]
    public async Task FilterBooks_ComplexAndOr_TwoConditionPairs()
    {
        await seeder.Seed();

        // Get a short and a long book
        var shortBook = ctx.Books.Where(b => b.Pages < 300).OrderBy(b => b.Id).First();
        var longBook = ctx.Books.Where(b => b.Pages > 700).OrderBy(b => b.Id).First();

        outputHelper.WriteLine($"Short book: {shortBook.Title} ({shortBook.Pages} pages)");
        outputHelper.WriteLine($"Long book: {longBook.Title} ({longBook.Pages} pages)");

        // Query: (Title=short AND Pages<300) OR (Title=long AND Pages>700)
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterEquals(b => b.Title, shortBook.Title)
            .FilterLessThan(b => b.Pages, 300)
            .Or()
            .FilterEquals(b => b.Title, longBook.Title)
            .FilterGreaterThan(b => b.Pages, 700);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Should find both books
        Assert.Contains(actual, b => b.Id == shortBook.Id);
        Assert.Contains(actual, b => b.Id == longBook.Id);

        outputHelper.WriteLine($"Found {actual.Count} books");
        outputHelper.WriteLine($"✓ Test passed: Complex (A AND B) OR (C AND D) works correctly");
    }

    [Fact]
    public async Task FilterBooks_OrWithSortByPages_ResultsAreSorted()
    {
        await seeder.Seed();

        // Query: (Pages < 200) OR (Pages > 800), sorted by Pages ascending
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterLessThan(b => b.Pages, 200)
            .Or()
            .FilterGreaterThan(b => b.Pages, 800)
            .SortBy(b => b.Pages);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
        outputHelper.WriteLine($"Sorts: {sieveModel.Sorts}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Verify filtering
        Assert.All(actual, book =>
            Assert.True(book.Pages < 200 || book.Pages > 800)
        );

        // Verify sorting
        var pagesList = actual.Select(b => b.Pages).ToList();
        var sortedPages = pagesList.OrderBy(p => p).ToList();
        Assert.Equal(sortedPages, pagesList);

        outputHelper.WriteLine($"Found {actual.Count} books, sorted by pages:");
        foreach (var book in actual.Take(5))
        {
            outputHelper.WriteLine($"  - {book.Title}: {book.Pages} pages");
        }
        outputHelper.WriteLine($"✓ Test passed: OR with sorting works correctly");
    }

    [Fact]
    public async Task FilterBooks_OrWithPagination_RespectsPageSize()
    {
        await seeder.Seed();

        // Get two books with different page counts
        var book1 = ctx.Books.OrderBy(b => b.Id).First();
        var book2 = ctx.Books.OrderBy(b => b.Id).Skip(1).First();

        // Query with pagination
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterEquals(b => b.Title, book1.Title)
            .Or()
            .FilterEquals(b => b.Title, book2.Title)
            .Page(1)
            .PageSize(1);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");
        outputHelper.WriteLine($"Page: {sieveModel.Page}, PageSize: {sieveModel.PageSize}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Should return exactly 1 book (page size)
        Assert.Equal(1, actual.Count);

        // Should be one of our two books
        Assert.True(
            actual[0].Id == book1.Id || actual[0].Id == book2.Id,
            "Result should be one of the filtered books"
        );

        outputHelper.WriteLine($"Found {actual.Count} book on page 1");
        outputHelper.WriteLine($"✓ Test passed: OR with pagination works correctly");
    }

    [Fact]
    public async Task FilterGenres_TwoGenresWithOr_FindsBothGenres()
    {
        await seeder.Seed();

        // Get two genres
        var genres = ctx.Genres
            .OrderBy(g => g.Id)
            .Take(2)
            .ToList();

        outputHelper.WriteLine($"Looking for genres: {genres[0].Name} OR {genres[1].Name}");

        // Query: Name equals genre1 OR Name equals genre2
        var builder = SievePlusQueryBuilder<Genre>.Create()
            .FilterEquals(g => g.Name, genres[0].Name)
            .Or()
            .FilterEquals(g => g.Name, genres[1].Name);

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetGenres(sieveModel);

        // Should contain both genres
        Assert.Contains(actual, g => g.Id == genres[0].Id);
        Assert.Contains(actual, g => g.Id == genres[1].Id);

        outputHelper.WriteLine($"Found {actual.Count} genres");
        outputHelper.WriteLine($"✓ Test passed: OR filtering works on Genre entity");
    }

    [Fact]
    public async Task FilterAuthors_NameContainsWithOr_MatchesEitherPattern()
    {
        await seeder.Seed();

        // Find authors whose names contain "a" or "e"
        var builder = SievePlusQueryBuilder<Author>.Create()
            .FilterContains(a => a.Name, "a")
            .Or()
            .FilterContains(a => a.Name, "e");

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetAuthors(sieveModel);

        // All results should contain "a" or "e" in the name
        Assert.All(actual, author =>
        {
            var containsA = author.Name.Contains("a", StringComparison.OrdinalIgnoreCase);
            var containsE = author.Name.Contains("e", StringComparison.OrdinalIgnoreCase);
            Assert.True(
                containsA || containsE,
                $"Author '{author.Name}' should contain 'a' or 'e'"
            );
        });

        outputHelper.WriteLine($"Found {actual.Count} authors with 'a' or 'e' in name");
        outputHelper.WriteLine($"✓ Test passed: OR with Contains operator works correctly");
    }

    [Fact]
    public async Task FilterBooks_ParseExistingOrQuery_WorksEndToEnd()
    {
        await seeder.Seed();

        var books = ctx.Books.OrderBy(b => b.Id).Take(2).ToList();

        // Create OR filter string manually
        var filtersString = $"Title=={books[0].Title} || Title=={books[1].Title}";
        outputHelper.WriteLine($"Parsing filter: {filtersString}");

        // Parse it back
        var builder = SievePlusQueryBuilder<Book>.ParseQueryString($"filters={filtersString}");
        var sieveModel = builder.BuildSieveModel();

        outputHelper.WriteLine($"Rebuilt filter: {sieveModel.Filters}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Should find both books
        Assert.Contains(actual, b => b.Id == books[0].Id);
        Assert.Contains(actual, b => b.Id == books[1].Id);

        // Verify round-trip
        Assert.Equal(filtersString, sieveModel.Filters);

        outputHelper.WriteLine($"Found {actual.Count} books");
        outputHelper.WriteLine($"✓ Test passed: Parsing OR queries works end-to-end");
    }

    [Fact]
    public async Task FilterBooks_VerifyOrSemantics_NotAndSemantics()
    {
        await seeder.Seed();

        // Get a book with a unique page count
        var targetBook = ctx.Books
            .OrderBy(b => b.Id)
            .First();

        // Create a query that should match the book with OR but wouldn't with AND
        // Query: (Title = targetTitle) OR (Pages = impossibleValue)
        var builder = SievePlusQueryBuilder<Book>.Create()
            .FilterEquals(b => b.Title, targetBook.Title)
            .Or()
            .FilterEquals(b => b.Pages, 99999); // Impossible page count

        var sieveModel = builder.BuildSieveModel();
        outputHelper.WriteLine($"Filter: {sieveModel.Filters}");

        var actual = await libraryService.GetBooks(sieveModel);

        // Should find the target book because OR allows partial matches
        Assert.Contains(actual, b => b.Id == targetBook.Id);

        // If this were AND, we'd get zero results
        outputHelper.WriteLine($"Found {actual.Count} books");
        outputHelper.WriteLine($"Target book: {targetBook.Title} ({targetBook.Pages} pages)");
        outputHelper.WriteLine($"✓ Test passed: Confirmed OR semantics (not AND)");
    }
}

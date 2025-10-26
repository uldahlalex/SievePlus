using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class SortingTests(
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    // Sorting Tests - Ascending
    [Fact]
    public async Task SortAuthors_ByNameAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortAuthors_ByCreatedDateAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByTitleAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByPageCountAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByCreatedDateAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortGenres_ByNameAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortGenres_ByCreatedDateAscending() { throw new NotImplementedException(); }

    // Sorting Tests - Descending
    [Fact]
    public async Task SortAuthors_ByNameDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortAuthors_ByCreatedDateDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByTitleDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByPageCountDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByCreatedDateDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortGenres_ByNameDescending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortGenres_ByCreatedDateDescending() { throw new NotImplementedException(); }

    // Multi-level Sorting Tests
    [Fact]
    public async Task SortBooks_ByTitleThenPageCount() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByGenreThenTitle() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortAuthors_ByNameThenCreatedDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_ByPageCountDescendingThenTitleAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_WithNullValues() { throw new NotImplementedException(); }
}

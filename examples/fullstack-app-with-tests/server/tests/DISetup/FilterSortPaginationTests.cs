using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class FilterSortPaginationTests(
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    // Combined Filter + Sort Tests
    [Fact]
    public async Task FilterAndSortAuthors_ByNameContainsSortedAscending() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAndSortBooks_ByPageRangeSortedByTitle() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAndSortBooks_ByGenreSortedByPageCount() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAndSortBooks_ByTitleContainsSortedByDate() { throw new NotImplementedException(); }

    // Combined Filter + Pagination Tests
    [Fact]
    public async Task FilterAndPaginateAuthors_ByNameFirstPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAndPaginateBooks_ByPageRangeWithPagination() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAndPaginateBooks_ByGenreWithPagination() { throw new NotImplementedException(); }

    // Combined Sort + Pagination Tests
    [Fact]
    public async Task SortAndPaginateAuthors_ByNameAscendingFirstPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortAndPaginateBooks_ByPageCountDescendingSecondPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortAndPaginateGenres_ByNameAscendingWithPageSize5() { throw new NotImplementedException(); }

    // Combined Filter + Sort + Pagination Tests
    [Fact]
    public async Task FilterSortAndPaginateAuthors_CompleteQuery() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterSortAndPaginateBooks_ByPageRangeSortedPaginated() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterSortAndPaginateBooks_ByGenreSortedByTitlePaginated() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterSortAndPaginateBooks_MultipleFiltersSortedPaginated() { throw new NotImplementedException(); }
}

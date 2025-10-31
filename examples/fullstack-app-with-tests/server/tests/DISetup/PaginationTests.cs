using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class PaginationTests(
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task PaginateAuthors_FirstPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateAuthors_SecondPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateAuthors_LastPage() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateBooks_PageSize10() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateBooks_PageSize25() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateBooks_PageSize50() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateGenres_PageSize5() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateBooks_NavigateThroughAllPages() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateAuthors_PageBeyondRangeReturnsEmpty() { throw new NotImplementedException(); }

    [Fact]
    public async Task PaginateBooks_PageSize0ReturnsError() { throw new NotImplementedException(); }
}

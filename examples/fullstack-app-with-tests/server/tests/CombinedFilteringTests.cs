using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class CombinedFilteringTests(ILibraryService libraryService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterBooks_ByTitleAndPageCount() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByTitleAndGenre() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountAndDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAuthors_ByNameAndDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_MultipleConditionsWithAnd() { throw new NotImplementedException(); }
}

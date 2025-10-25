using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class EdgeCasesTests(ILibraryService libraryService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterAuthors_NoMatchesReturnsEmpty() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_InvalidFilterReturnsError() { throw new NotImplementedException(); }

    [Fact]
    public async Task SortBooks_InvalidPropertyReturnsError() { throw new NotImplementedException(); }
}

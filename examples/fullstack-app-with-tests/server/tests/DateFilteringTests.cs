using api;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class DateFilteringTests(ILibraryService libraryService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterAuthors_ByCreatedAfterDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAuthors_ByCreatedBeforeDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByCreatedAfterDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByCreatedBeforeDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByCreatedDateRange() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterGenres_ByCreatedAfterDate() { throw new NotImplementedException(); }
}

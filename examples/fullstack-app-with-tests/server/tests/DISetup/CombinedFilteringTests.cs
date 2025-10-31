using api;
using api.DTOs.QueryModels;
using api.Services;
using dataccess;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class CombinedFilteringTests(IComputerStoreService computerStoreService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterBooks_ByTitleAndPageCount()
    {

        await seeder.Seed();

        var req = SievePlusQueryBuilder<ComputerQueryModel>.Create()
            .FilterGreaterThan(p => p.Price, 500)
            .BuildSieveModel();
        var actual = await computerStoreService.GetComputers(req);
        
        Assert.All(actual, c => Assert.True(c.Price > 500));

    }

    [Fact]
    public async Task FilterBooks_ByTitleAndGenre() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountAndDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAuthors_ByNameAndDate() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_MultipleConditionsWithAnd() { throw new NotImplementedException(); }
}

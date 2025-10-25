using System.Text.Json;
using System.Text.Json.Serialization;
using api;
using api.Services;
using dataccess;
using Microsoft.EntityFrameworkCore;
using Sieve.Plus.Models;
using Sieve.Plus.QueryBuilder;

namespace tests;

public class BasicFilteringTests(ILibraryService libraryService,
    MyDbContext ctx,
    ITestOutputHelper outputHelper,
    ISeeder seeder)
{
    [Fact]
    public async Task FilterAuthors_ByExactName()
    {
        await seeder.Seed();

        var randomAuthor = ctx.Authors
            .OrderBy(a => Guid.NewGuid())
            .First();

        var builder = SievePlusQueryBuilder<Author>.Create()
            .FilterEquals(a => a.Name, randomAuthor.Name);

        builder.BuildSieveModel().PrintAsJson(outputHelper);

        var actual = await libraryService.GetAuthors(builder.BuildSieveModel());

        randomAuthor.PrintAsJson(outputHelper);

        Assert.Contains(actual, a => a.Id == randomAuthor.Id);
    }

    [Fact]
    public async Task FilterAuthors_ByNameContains()
    {
        await seeder.Seed();
        var randomAuthor = ctx.Authors
            .OrderBy(a => Guid.NewGuid())
            // .AsNoTracking()
            .First();

        var filter = SievePlusQueryBuilder<Author>.Create()
            .FilterContains(a => a.Name, randomAuthor.Name)
            .BuildSieveModel();

        filter.PrintAsJson(outputHelper);
        var actual = await libraryService.GetAuthors(filter);
        randomAuthor.PrintAsJson(outputHelper);
        Assert.Contains(actual, a => a.Id == randomAuthor.Id);
    }

    [Fact]
    public async Task FilterAuthors_ByNameStartsWith() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByExactTitle() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByTitleContains() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountEquals() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountGreaterThan() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountLessThan() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByPageCountRange() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_ByGenreId() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterGenres_ByExactName() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterGenres_ByNameContains() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterAuthors_CaseInsensitiveNameSearch() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_CaseInsensitiveTitleSearch() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterGenres_CaseInsensitiveNameSearch() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_WithNullGenreId() { throw new NotImplementedException(); }

    [Fact]
    public async Task FilterBooks_WithEmptyFilter() { throw new NotImplementedException(); }
}

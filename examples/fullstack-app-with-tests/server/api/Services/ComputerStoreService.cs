using api.DTOs.QueryModels;
using dataccess;
using Microsoft.EntityFrameworkCore;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace api.Services;

public class ComputerStoreService(MyDbContext ctx, SievePlusProcessor sievePlusProcessor) : IComputerStoreService
{
    public Task<List<Computer>> GetComputers(SievePlusModel<ComputerQueryModel> sievePlusModel)
    {
        IQueryable<Computer> query = ctx.Computers;

        query = sievePlusProcessor.Apply<Computer, ComputerQueryModel>(sievePlusModel, query);

        return query
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<List<Brand>> GetBrands(SievePlusModel<BrandQueryModel> sievePlusModel)
    {
        IQueryable<Brand> query = ctx.Brands;

        // Apply Sieve.Plus FIRST with query model
        query = sievePlusProcessor.Apply<Brand, BrandQueryModel>(sievePlusModel, query);

        // Include Computers but DON'T include nested Brand/Category to avoid cycles
        return query
            .Include(b => b.Computers)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<List<Category>> GetCategories(SievePlusModel<CategoryQueryModel> sievePlusModel)
    {
        IQueryable<Category> query = ctx.Categories;

        // Apply Sieve.Plus FIRST with query model
        query = sievePlusProcessor.Apply<Category, CategoryQueryModel>(sievePlusModel, query);

        // Include Computers but DON'T include nested Brand/Category to avoid cycles
        return query
            .Include(c => c.Computers)
            .AsSplitQuery()
            .ToListAsync();
    }
}

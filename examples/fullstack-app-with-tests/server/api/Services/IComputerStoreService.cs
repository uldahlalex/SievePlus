using dataccess;
using Sieve.Plus.Models;

namespace api.Services;

public interface IComputerStoreService
{
    Task<List<Computer>> GetComputers(SievePlusModel sievePlusModel);
    Task<List<Brand>> GetBrands(SievePlusModel sievePlusModel);
    Task<List<Category>> GetCategories(SievePlusModel sievePlusModel);
}

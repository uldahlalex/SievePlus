using api.DTOs.QueryModels;
using dataccess;
using Sieve.Plus.Models;

namespace api.Services;

public interface IComputerStoreService
{
    Task<List<Computer>> GetComputers(SievePlusModel<ComputerQueryModel> sievePlusModel);
    Task<List<Brand>> GetBrands(SievePlusModel sievePlusModel);
    Task<List<Category>> GetCategories(SievePlusModel sievePlusModel);
}

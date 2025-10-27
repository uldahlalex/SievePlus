using api.DTOs.QueryModels;
using api.Services;
using dataccess;
using Microsoft.AspNetCore.Mvc;
using Sieve.Plus.Models;

namespace api;

[ApiController]
[Route("api/[controller]")]
public class ComputerStoreController(IComputerStoreService computerStoreService) : ControllerBase
{
    [HttpPost(nameof(GetComputers))]
    public async Task<List<Computer>> GetComputers([FromBody] SievePlusRequest<ComputerQueryModel> request)
    {
        return await computerStoreService.GetComputers(request);
    }

    [HttpPost(nameof(GetBrands))]
    public async Task<List<Brand>> GetBrands([FromBody] SievePlusRequest<BrandQueryModel> request)
    {
        return await computerStoreService.GetBrands(request);
    }

    [HttpPost(nameof(GetCategories))]
    public async Task<List<Category>> GetCategories([FromBody] SievePlusRequest<CategoryQueryModel> request)
    {
        return await computerStoreService.GetCategories(request);
    }
}

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
    public async Task<List<Computer>> GetComputers([FromBody] SievePlusModel model)
    {
        return await computerStoreService.GetComputers(model);
    }

    [HttpPost(nameof(GetBrands))]
    public async Task<List<Brand>> GetBrands([FromBody] SievePlusModel model)
    {
        return await computerStoreService.GetBrands(model);
    }

    [HttpPost(nameof(GetCategories))]
    public async Task<List<Category>> GetCategories([FromBody] SievePlusModel model)
    {
        return await computerStoreService.GetCategories(model);
    }
}

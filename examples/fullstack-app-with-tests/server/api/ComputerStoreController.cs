using api.Services;
using dataccess;
using Microsoft.AspNetCore.Mvc;
using Sieve.Plus.Models;

namespace api;

[ApiController]
[Route("api/[controller]")]
public class ComputerStoreController(IComputerStoreService computerStoreService) : ControllerBase
{
    [HttpGet(nameof(GetComputers))]
    public async Task<List<Computer>> GetComputers([FromQuery] SievePlusModel model)
    {
        return await computerStoreService.GetComputers(model);
    }

    [HttpGet(nameof(GetBrands))]
    public async Task<List<Brand>> GetBrands([FromQuery] SievePlusModel model)
    {
        return await computerStoreService.GetBrands(model);
    }

    [HttpGet(nameof(GetCategories))]
    public async Task<List<Category>> GetCategories([FromQuery] SievePlusModel model)
    {
        return await computerStoreService.GetCategories(model);
    }
}

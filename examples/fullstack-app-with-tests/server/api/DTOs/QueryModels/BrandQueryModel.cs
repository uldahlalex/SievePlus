using dataccess;
using Sieve.Plus.Models;
using Sieve.Plus.QueryBuilder;

namespace api.DTOs.QueryModels;

public class BrandQueryModel : Brand, Sieve.Plus.Models.ISievePlusQueryModel, Sieve.Plus.QueryBuilder.ISievePlusQueryModel
{

    public int ComputerCount { get; set; }
}

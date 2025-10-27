using dataccess;
using Sieve.Plus.Models;

namespace api.DTOs.QueryModels;

public class BrandQueryModel : Brand, ISievePlusQueryModel
{

    public int ComputerCount { get; set; }
}

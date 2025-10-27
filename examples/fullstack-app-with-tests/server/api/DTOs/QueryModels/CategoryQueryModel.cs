using Sieve.Plus.Models;
using Sieve.Plus.QueryBuilder;

namespace api.DTOs.QueryModels;

public class CategoryQueryModel : Sieve.Plus.Models.ISievePlusQueryModel, Sieve.Plus.QueryBuilder.ISievePlusQueryModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int ComputerCount { get; set; }  // Calculated
}

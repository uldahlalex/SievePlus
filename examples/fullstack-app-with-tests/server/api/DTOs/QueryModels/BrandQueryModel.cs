using Sieve.Plus.Models;

namespace api.DTOs.QueryModels;

public class BrandQueryModel : ISievePlusQueryModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int ComputerCount { get; set; }  // Calculated
}

using Sieve.Plus.Models;

namespace api.DTOs.QueryModels;

/// <summary>
/// Query model for Computer entity - defines exactly what can be filtered and sorted.
/// Perfect for Pricerunner-style filtering.
/// </summary>
public class ComputerQueryModel : ISievePlusQueryModel
{
    // Basic properties
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Processor { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal ScreenSize { get; set; }
    public int Ram { get; set; }
    public int Storage { get; set; }
    public string GraphicsCard { get; set; } = null!;
    public bool InStock { get; set; }
    public double Rating { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public string BrandName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;

    // Custom filters
    public bool? IsPopular { get; set; }  // Sales > 100 && Rating >= 4.0
    public bool? IsPremium { get; set; }  // Price > 2000
    public bool? IsHighPerformance { get; set; }  // Ram >= 16 && Storage >= 512
}

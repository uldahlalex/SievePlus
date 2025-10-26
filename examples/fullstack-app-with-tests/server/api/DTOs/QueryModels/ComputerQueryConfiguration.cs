using dataccess;
using Sieve.Plus.Services;

namespace api.DTOs.QueryModels;

/// <summary>
/// Configuration that maps ComputerQueryModel properties to Computer entity properties.
/// </summary>
public class ComputerQueryConfiguration : ISievePlusQueryConfiguration<ComputerQueryModel, Computer>
{
    public void Configure(SievePlusQueryMapper<ComputerQueryModel, Computer> mapper)
    {
        // Simple 1:1 property mappings
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Name, e => e.Name).CanFilter().CanSort();
        mapper.Property(q => q.Processor, e => e.Processor).CanFilter().CanSort();
        mapper.Property(q => q.Price, e => e.Price).CanFilter().CanSort();
        mapper.Property(q => q.ScreenSize, e => e.ScreenSize).CanFilter().CanSort();
        mapper.Property(q => q.Ram, e => e.Ram).CanFilter().CanSort();
        mapper.Property(q => q.Storage, e => e.Storage).CanFilter().CanSort();
        mapper.Property(q => q.GraphicsCard, e => e.GraphicsCard).CanFilter().CanSort();
        mapper.Property(q => q.InStock, e => e.InStock).CanFilter();
        mapper.Property(q => q.Rating, e => e.Rating).CanFilter().CanSort();
        mapper.Property(q => q.CreatedAt, e => e.CreatedAt).CanFilter().CanSort();

        // Navigation properties
        mapper.Property(q => q.BrandName, e => e.Brand.Name).CanFilter().CanSort();
        mapper.Property(q => q.CategoryName, e => e.Category.Name).CanFilter().CanSort();

        // Custom filters
        mapper.CustomFilter(q => q.IsPopular, e => e.Sales > 100 && e.Rating >= 4.0);
        mapper.CustomFilter(q => q.IsPremium, e => e.Price > 2000);
        mapper.CustomFilter(q => q.IsHighPerformance, e => e.Ram >= 16 && e.Storage >= 512);
    }
}

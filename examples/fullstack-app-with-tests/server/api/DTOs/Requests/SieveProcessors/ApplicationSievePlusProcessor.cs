using api.DTOs.QueryModels;
using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace api.Services;

/// <summary>
/// Custom Sieve.Plus processor with explicit query model configurations
/// for the Pricerunner computer comparison app.
/// </summary>
public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    public ApplicationSievePlusProcessor(IOptions<SievePlusOptions> options) : base(options)
    {
    }

    /// <summary>
    /// Configure explicit query models for type-safe querying.
    /// Query models define exactly what properties can be filtered/sorted.
    /// </summary>
    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        // Register query model configurations for Pricerunner
        registry.AddConfiguration<ComputerQueryConfiguration>();
        registry.AddConfiguration<BrandQueryConfiguration>();
        registry.AddConfiguration<CategoryQueryConfiguration>();

        // Alternative: Scan entire assembly for all configurations
        // registry.AddConfigurationsFromAssembly(typeof(ComputerQueryConfiguration).Assembly);
    }
}

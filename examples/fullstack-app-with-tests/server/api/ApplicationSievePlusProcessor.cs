using api.DTOs.QueryModels;
using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace api;
public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    public ApplicationSievePlusProcessor(
        IOptions<SievePlusOptions> options,
        ISievePlusCustomSortMethods? customSortMethods = null,
        ISievePlusCustomFilterMethods? customFilterMethods = null)
        : base(options, customSortMethods, customFilterMethods)
    {
    }

    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        // Register individual configurations
        // registry.AddConfiguration<BrandQueryConfiguration>();
        // registry.AddConfiguration<ComputerQueryConfiguration>();
        // registry.AddConfiguration<CategoryQueryConfiguration>();

        registry.AddConfigurationsFromAssembly(typeof(BrandQueryConfiguration).Assembly);
    }
}

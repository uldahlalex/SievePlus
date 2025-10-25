using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

namespace Sieve.Plus.UnitTests.Services
{
    public class ModularConfigurationWithScanSievePlusProcessor : SievePlusProcessor
    {
        public ModularConfigurationWithScanSievePlusProcessor(
            IOptions<SievePlusOptions> options,
            ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomSortMethods, plusCustomFilterMethods)
        {
        }

        protected override SievePlusPropertyMapper MapProperties(SievePlusPropertyMapper mapper) => 
            mapper.ApplyConfigurationsFromAssembly(typeof(ModularConfigurationWithScanSievePlusProcessor).Assembly);
    }
}

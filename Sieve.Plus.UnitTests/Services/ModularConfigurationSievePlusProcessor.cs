using Microsoft.Extensions.Options;
using Sieve.Plus.Models;
using Sieve.Plus.Services;
using Sieve.Plus.UnitTests.Abstractions.Entity;
using Sieve.Plus.UnitTests.Entities;

namespace Sieve.Plus.UnitTests.Services
{
    public class ModularConfigurationSievePlusProcessor : SievePlusProcessor
    {
        public ModularConfigurationSievePlusProcessor(
            IOptions<SievePlusOptions> options,
            ISievePlusCustomSortMethods plusCustomSortMethods,
            ISievePlusCustomFilterMethods plusCustomFilterMethods)
            : base(options, plusCustomSortMethods, plusCustomFilterMethods)
        {
        }

        protected override SievePlusPropertyMapper MapProperties(SievePlusPropertyMapper mapper)
        {
            return mapper
                .ApplyConfiguration<SievePlusConfigurationForPost>()
                .ApplyConfiguration<SievePlusConfigurationForIPost>();
        }
    }
}

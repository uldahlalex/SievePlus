using Microsoft.Extensions.Options;
using Sieve.Plus.Models;

namespace Sieve.Plus.UnitTests.Services
{
    public class SieveOptionsAccessor : IOptions<SievePlusOptions>
    {
        public SievePlusOptions Value { get; }

        public SieveOptionsAccessor()
        {
            Value = new SievePlusOptions()
            {
                ThrowExceptions = true
            };
        }
    }
}

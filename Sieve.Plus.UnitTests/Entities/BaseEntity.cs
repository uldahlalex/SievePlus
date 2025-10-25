using System;
using Sieve.Plus.Attributes;
using Sieve.Plus.UnitTests.Abstractions.Entity;

namespace Sieve.Plus.UnitTests.Entities
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }

        [SievePlus(CanFilter = true, CanSort = true)]
        public DateTimeOffset DateCreated { get; set; } = DateTimeOffset.UtcNow;
    }
}

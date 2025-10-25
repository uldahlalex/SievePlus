using Sieve.Plus.Attributes;
using Sieve.Plus.UnitTests.Abstractions.Entity;

namespace Sieve.Plus.UnitTests.Entities
{
    public class Comment : BaseEntity, IComment
    {
        [SievePlus(CanFilter = true)]
        public string Text { get; set; }
        
        [SievePlus(CanFilter = true)]
        public string Author { get; set; }
    }
}

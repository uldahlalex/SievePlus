namespace Sieve.Plus.Models
{
    public class SievePlusPropertyMetadata : ISievePropertyMetadata
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public bool CanFilter { get; set; }
        public bool CanSort { get; set; }
    }
}

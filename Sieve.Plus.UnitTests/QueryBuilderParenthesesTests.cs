using Sieve.Plus.QueryBuilder;
using Xunit;

namespace Sieve.Plus.UnitTests
{
    public class Computer : ISievePlusQueryModel, Sieve.Plus.Models.ISievePlusQueryModel
    {
        public string Processor { get; set; }
        public decimal Price { get; set; }
        public decimal ScreenSize { get; set; }
    }

    public class QueryBuilderParenthesesTests
    {
        [Fact]
        public void BeginGroup_EndGroup_GeneratesParentheses()
        {
            var query = SievePlusQueryBuilder<Computer>.Create()
                .BeginGroup()
                    .FilterEquals(c => c.Processor, "Intel i9")
                    .Or()
                    .FilterEquals(c => c.Processor, "AMD Ryzen 9")
                .EndGroup()
                .FilterGreaterThanOrEqual(c => c.Price, 1000)
                .BuildFiltersString();

            Assert.Equal("(Processor==Intel i9 || Processor==AMD Ryzen 9),Price>=1000", query);
        }

        [Fact]
        public void FilterWithAlternatives_GeneratesParentheses()
        {
            var query = SievePlusQueryBuilder<Computer>.Create()
                .FilterWithAlternatives(
                    c => c.Processor,
                    new[] { "Intel i9", "AMD Ryzen 9" },
                    b => b.FilterGreaterThan(c => c.Price, 1000)
                )
                .BuildFiltersString();

            Assert.Equal("(Processor==Intel i9 || Processor==AMD Ryzen 9),Price>1000", query);
        }

        [Fact]
        public void FilterWithAlternatives_MultipleOptions()
        {
            var query = SievePlusQueryBuilder<Computer>.Create()
                .FilterWithAlternatives(
                    c => c.Processor,
                    new[] { "Intel i9", "AMD Ryzen 9", "Apple M2" },
                    null
                )
                .BuildFiltersString();

            Assert.Equal("(Processor==Intel i9 || Processor==AMD Ryzen 9 || Processor==Apple M2)", query);
        }

        [Fact]
        public void BackwardCompatibility_SimpleOr_StillWorks()
        {
            var query = SievePlusQueryBuilder<Computer>.Create()
                .FilterEquals(c => c.Processor, "Intel")
                .Or()
                .FilterEquals(c => c.Processor, "AMD")
                .BuildFiltersString();

            // Should still generate old-style OR
            Assert.Equal("Processor==Intel || Processor==AMD", query);
        }
    }
}

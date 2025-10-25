using System;
using Sieve.Plus.QueryBuilder;

public class Computer
{
    public string Processor { get; set; }
    public decimal Price { get; set; }
    public decimal ScreenSize { get; set; }
}

class Program
{
    static void Main()
    {
        // Test 1: Simple OR group with shared constraints
        var query1 = SievePlusQueryBuilder<Computer>.Create()
            .BeginGroup()
                .FilterEquals(c => c.Processor, "Intel i9")
                .Or()
                .FilterEquals(c => c.Processor, "AMD Ryzen 9")
            .EndGroup()
            .FilterGreaterThanOrEqual(c => c.Price, 1000)
            .FilterLessThanOrEqual(c => c.Price, 2000)
            .BuildFiltersString();

        Console.WriteLine("Test 1 - OR group with shared constraints:");
        Console.WriteLine(query1);
        Console.WriteLine("Expected: (Processor==Intel i9 || Processor==AMD Ryzen 9),Price>=1000,Price<=2000");
        Console.WriteLine();

        // Test 2: FilterWithAlternatives helper
        var query2 = SievePlusQueryBuilder<Computer>.Create()
            .FilterWithAlternatives(
                c => c.Processor,
                new[] { "Intel i9", "AMD Ryzen 9", "Apple M2" },
                b => b
                    .FilterGreaterThan(c => c.Price, 1000)
                    .FilterLessThan(c => c.ScreenSize, 16)
            )
            .BuildFiltersString();

        Console.WriteLine("Test 2 - FilterWithAlternatives:");
        Console.WriteLine(query2);
        Console.WriteLine("Expected: (Processor==Intel i9 || Processor==AMD Ryzen 9 || Processor==Apple M2),Price>1000,ScreenSize<16");
        Console.WriteLine();
    }
}

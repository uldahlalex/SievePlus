# Sieve.QueryBuilder.Fork

Type-safe query builder for [Sieve.Fork](https://github.com/uldahlalex/Sieve) with full OR query support and round-trip parsing.

## Features

- ✅ **Type-safe query building** - Use lambda expressions instead of magic strings
- ✅ **OR query support** - Full support for `||` operator introduced in Sieve.Fork 2.6.0
- ✅ **Query models** - IntelliSense for custom mapped properties
- ✅ **Round-trip parsing** - Parse query strings and SieveModels back to builders
- ✅ **Inspection API** - Examine filters, filter groups, and sorts programmatically
- ✅ **All operators** - ==, !=, @=, _=, _-=, >, <, >=, <=
- ✅ **Fluent API** - Chain methods for readable construction
- ✅ **Multi-framework** - Supports netstandard2.0 → net9.0

## Installation

```bash
dotnet add package Sieve.QueryBuilder.Fork
dotnet add package Sieve.Fork
```

## Quick Start

### Building Queries with OR Support

```csharp
using Sieve.QueryBuilder;

// Simple OR query
var queryString = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Electronics")
    .Or()
    .FilterEquals(p => p.Category, "Computers")
    .BuildQueryString();

// Result: "filters=Category==Electronics || Category==Computers"
```

### Complex AND/OR Combinations

```csharp
// (Category == Electronics AND Price > 100) OR (Category == Books AND Price > 20)
var query = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Electronics")
    .FilterGreaterThan(p => p.Price, 100)
    .Or()
    .FilterEquals(p => p.Category, "Books")
    .FilterGreaterThan(p => p.Price, 20)
    .BuildQueryString();

// Result: "filters=Category==Electronics,Price>100 || Category==Books,Price>20"
```

## AND/OR Logic: Evaluation Order & Hierarchy

Understanding how Sieve.Plus evaluates AND/OR logic is crucial for building correct queries.

### Key Principle: OR has Higher Priority (Group Separator)

**The `||` (OR) operator acts as a GROUP SEPARATOR, NOT a logical operator with precedence.**

Think of it like this:
- **Comma (`,`)** = AND within a group
- **Double-pipe (`||`)** = Separates independent filter groups
- Groups are combined with OR logic
- Within each group, filters are combined with AND logic

### Evaluation Process

```
Step 1: Split by || to create filter groups
Step 2: Within each group, combine filters with AND
Step 3: Combine all groups with OR
```

### Examples with Evaluation Order

#### Example 1: Simple AND within groups, OR between groups
```csharp
// Query Builder
var query = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Electronics")  // Group 1, Filter 1
    .FilterGreaterThan(p => p.Price, 100)          // Group 1, Filter 2
    .Or()                                           // Start Group 2
    .FilterEquals(p => p.InStock, true)            // Group 2, Filter 1
    .BuildQueryString();

// Output: "filters=Category==Electronics,Price>100 || InStock==true"

// Evaluation (Pseudocode):
// Group 1: (Category == "Electronics" AND Price > 100)
// Group 2: (InStock == true)
// Final: (Group 1) OR (Group 2)
// SQL equivalent: WHERE (Category = 'Electronics' AND Price > 100) OR (InStock = true)
```

#### Example 2: Multiple OR groups
```csharp
var query = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Status, "New")      // Group 1
    .Or()
    .FilterEquals(p => p.Status, "Sale")     // Group 2
    .Or()
    .FilterEquals(p => p.Status, "Clearance") // Group 3
    .BuildQueryString();

// Output: "filters=Status==New || Status==Sale || Status==Clearance"
// Evaluation: (Status == "New") OR (Status == "Sale") OR (Status == "Clearance")
```

#### Example 3: Complex business logic
```csharp
// Find products that are:
// (Premium AND expensive) OR (On sale regardless of price) OR (High rated AND in stock)
var query = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Tier, "Premium")          // Group 1, Filter 1
    .FilterGreaterThan(p => p.Price, 1000)         // Group 1, Filter 2
    .Or()                                           // Start Group 2
    .FilterEquals(p => p.OnSale, true)             // Group 2, Filter 1
    .Or()                                           // Start Group 3
    .FilterGreaterThanOrEqual(p => p.Rating, 4.5)  // Group 3, Filter 1
    .FilterEquals(p => p.InStock, true)            // Group 3, Filter 2
    .BuildQueryString();

// Output: "filters=Tier==Premium,Price>1000 || OnSale==true || Rating>=4.5,InStock==true"

// Evaluation:
// Group 1: (Tier == "Premium" AND Price > 1000)
// Group 2: (OnSale == true)
// Group 3: (Rating >= 4.5 AND InStock == true)
// Final: (Group 1) OR (Group 2) OR (Group 3)
```

#### Example 4: Why order matters within groups (AND is associative, but clarity helps)
```csharp
// These two produce IDENTICAL results (AND is commutative and associative)
var query1 = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Books")
    .FilterGreaterThan(p => p.Price, 20)
    .BuildQueryString();
// Output: "filters=Category==Books,Price>20"
// Evaluation: (Category == "Books" AND Price > 20)

var query2 = SieveQueryBuilder<Product>.Create()
    .FilterGreaterThan(p => p.Price, 20)
    .FilterEquals(p => p.Category, "Books")
    .BuildQueryString();
// Output: "filters=Price>20,Category==Books"
// Evaluation: (Price > 20 AND Category == "Books")
// SAME RESULT - order doesn't affect outcome, only readability
```

### Mental Model: Parentheses Visualization

Think of the query builder like this:

```csharp
builder
    .Filter1()   // (Filter1
    .Filter2()   //  AND Filter2
    .Filter3()   //  AND Filter3)
    .Or()        // OR
    .Filter4()   // (Filter4
    .Filter5()   //  AND Filter5)
    .Or()        // OR
    .Filter6()   // (Filter6)
```

Each `.Or()` call closes the current group and starts a new one.

### Common Pitfalls

❌ **WRONG - Thinking OR has lower precedence than AND:**
```
"A==1,B==2 || C==3"
Incorrect interpretation: A==1 AND (B==2 OR C==3)
```

✅ **CORRECT - OR separates groups:**
```
"A==1,B==2 || C==3"
Correct interpretation: (A==1 AND B==2) OR (C==3)
```

### Rule of Thumb

**Always read from left to right, splitting on `||` first:**

1. Split the filter string by ` || ` to get groups
2. Within each group, split by `,` and combine with AND
3. Combine all groups with OR

### Real-World Example: Pricerunner Computer Filtering

A practical example showing how to filter computers with processor choice while maintaining consistent price and screen size constraints:

```csharp
// Scenario: User wants laptops with EITHER Intel i9 OR AMD Ryzen 9 processor
// BUT all results must be within $1000-$2000 price range
// AND have screen size between 14-16 inches

// ❌ WRONG APPROACH - This will give unexpected results
var wrongQuery = SieveQueryBuilder<Computer>.Create()
    .FilterEquals(c => c.Processor, "Intel i9")
    .Or()
    .FilterEquals(c => c.Processor, "AMD Ryzen 9")
    .FilterGreaterThanOrEqual(c => c.Price, 1000)
    .FilterLessThanOrEqual(c => c.Price, 2000)
    .FilterGreaterThanOrEqual(c => c.ScreenSize, 14)
    .FilterLessThanOrEqual(c => c.ScreenSize, 16)
    .BuildQueryString();

// Output: "filters=Processor==Intel i9 || Processor==AMD Ryzen 9,Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16"
// Problem: This means:
// (Processor == "Intel i9")
// OR
// (Processor == "AMD Ryzen 9" AND Price >= 1000 AND Price <= 2000 AND ScreenSize >= 14 AND ScreenSize <= 16)
//
// This will return Intel i9 laptops at ANY price and ANY screen size! 🐛

// ✅ CORRECT APPROACH - Repeat shared constraints in each OR group
var correctQuery = SieveQueryBuilder<Computer>.Create()
    // Group 1: Intel i9 with all constraints
    .FilterEquals(c => c.Processor, "Intel i9")
    .FilterGreaterThanOrEqual(c => c.Price, 1000)
    .FilterLessThanOrEqual(c => c.Price, 2000)
    .FilterGreaterThanOrEqual(c => c.ScreenSize, 14)
    .FilterLessThanOrEqual(c => c.ScreenSize, 16)
    .Or()
    // Group 2: AMD Ryzen 9 with same constraints
    .FilterEquals(c => c.Processor, "AMD Ryzen 9")
    .FilterGreaterThanOrEqual(c => c.Price, 1000)
    .FilterLessThanOrEqual(c => c.Price, 2000)
    .FilterGreaterThanOrEqual(c => c.ScreenSize, 14)
    .FilterLessThanOrEqual(c => c.ScreenSize, 16)
    .BuildQueryString();

// Output: "filters=Processor==Intel i9,Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16 || Processor==AMD Ryzen 9,Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16"

// This correctly means:
// (Processor == "Intel i9" AND Price >= 1000 AND Price <= 2000 AND ScreenSize >= 14 AND ScreenSize <= 16)
// OR
// (Processor == "AMD Ryzen 9" AND Price >= 1000 AND Price <= 2000 AND ScreenSize >= 14 AND ScreenSize <= 16)
// ✅ Perfect!

// SQL Equivalent:
// WHERE (Processor = 'Intel i9' AND Price >= 1000 AND Price <= 2000 AND ScreenSize >= 14 AND ScreenSize <= 16)
//    OR (Processor = 'AMD Ryzen 9' AND Price >= 1000 AND Price <= 2000 AND ScreenSize >= 14 AND ScreenSize <= 16)
```

#### Building Dynamic Pricerunner Queries

For better maintainability when dealing with multiple OR groups with shared constraints:

```csharp
public class ComputerFilterBuilder
{
    private readonly decimal _minPrice;
    private readonly decimal _maxPrice;
    private readonly decimal _minScreenSize;
    private readonly decimal _maxScreenSize;

    public ComputerFilterBuilder(decimal minPrice, decimal maxPrice,
                                  decimal minScreenSize, decimal maxScreenSize)
    {
        _minPrice = minPrice;
        _maxPrice = maxPrice;
        _minScreenSize = minScreenSize;
        _maxScreenSize = maxScreenSize;
    }

    public SievePlusQueryBuilder<Computer> BuildQuery(string[] processorOptions)
    {
        var builder = SievePlusQueryBuilder<Computer>.Create();

        for (int i = 0; i < processorOptions.Length; i++)
        {
            if (i > 0)
            {
                builder.Or(); // Start new OR group for each processor after the first
            }

            // Add processor filter
            builder.FilterEquals(c => c.Processor, processorOptions[i]);

            // Add shared constraints to this group
            AddSharedConstraints(builder);
        }

        return builder;
    }

    private void AddSharedConstraints(SievePlusQueryBuilder<Computer> builder)
    {
        builder
            .FilterGreaterThanOrEqual(c => c.Price, _minPrice)
            .FilterLessThanOrEqual(c => c.Price, _maxPrice)
            .FilterGreaterThanOrEqual(c => c.ScreenSize, _minScreenSize)
            .FilterLessThanOrEqual(c => c.ScreenSize, _maxScreenSize);
    }
}

// Usage:
var filterBuilder = new ComputerFilterBuilder(
    minPrice: 1000,
    maxPrice: 2000,
    minScreenSize: 14,
    maxScreenSize: 16
);

var query = filterBuilder.BuildQuery(new[] { "Intel i9", "AMD Ryzen 9", "Apple M2" })
    .SortBy(c => c.Price)
    .Page(1)
    .PageSize(20)
    .BuildSieveModel();

// This creates 3 OR groups, each with the same price/screen constraints
// Perfect for Pricerunner-style filtering!
```

#### Key Takeaway

**When using OR for alternative values (like different processors), you must repeat ALL shared constraints in EACH OR group.**

There is no "global AND" that applies across all OR groups - each group is independent.

### Building SieveModel

```csharp
var sieveModel = SieveQueryBuilder<Author>.Create()
    .FilterContains(a => a.Name, "Bob")
    .Or()
    .FilterContains(a => a.Name, "Alice")
    .SortByDescending(a => a.CreatedAt)
    .Page(1)
    .PageSize(25)
    .BuildSieveModel();

// Use with Sieve processor
var results = sieveProcessor.Apply(sieveModel, query);
```

## Query Models - Type-Safe Custom Properties

Query models provide compile-time safety for custom mapped properties:

```csharp
using Sieve.QueryBuilder;

// Define query model matching your SieveProcessor configuration
public class AuthorQueryModel : ISieveQueryModel
{
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? BooksCount { get; set; }  // Custom mapped property
}

// Type-safe queries with IntelliSense
var query = SieveQueryBuilder<AuthorQueryModel>.Create()
    .FilterContains(a => a.Name, "Bob")
    .Or()
    .FilterGreaterThan(a => a.BooksCount, 5)  // IntelliSense!
    .BuildQueryString();
```

## Round-Trip Parsing

### Parse Query Strings

```csharp
// Parse existing query string
var builder = SieveQueryBuilder<Product>.ParseQueryString(
    "filters=Category==Electronics || Price>100&sorts=-CreatedAt&page=1&pageSize=20"
);

// Modify and rebuild
builder.FilterEquals(p => p.InStock, true)
       .BuildQueryString();
```

### Parse from SieveModel

```csharp
var existingModel = new SieveModel
{
    Filters = "Name@=Bob || Age>25",
    Sorts = "-CreatedAt",
    Page = 1,
    PageSize = 20
};

var builder = SieveQueryBuilder<Person>.FromSieveModel(existingModel);
```

## Inspection API

Examine filters and filter groups programmatically:

```csharp
var builder = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Books")
    .FilterGreaterThan(p => p.Price, 20)
    .Or()
    .FilterEquals(p => p.OnSale, true);

// Get filter groups (respects OR logic)
var groups = builder.GetFilterGroups();
// Returns: [[Filter: Category==Books, Filter: Price>20], [Filter: OnSale==true]]

// Get all filters (flattened)
var allFilters = builder.GetFilters();
foreach (var filter in allFilters)
{
    Console.WriteLine($"{filter.PropertyName} {filter.Operator} {filter.Value}");
}

// Get sorts
var sorts = builder.GetSorts();
foreach (var sort in sorts)
{
    Console.WriteLine($"{sort.PropertyName} ({sort.IsDescending ? "DESC" : "ASC"})");
}

// Check for specific filters
bool hasNameFilter = builder.HasFilter("Name");
bool hasCreatedAtSort = builder.HasSort("CreatedAt");
```

## All Filter Operators

```csharp
var builder = SieveQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Id, 42)                              // ==
    .FilterNotEquals(p => p.Status, "Deleted")                // !=
    .FilterContains(p => p.Description, "awesome")            // @=
    .FilterStartsWith(p => p.Name, "Pro")                     // _=
    .FilterGreaterThan(p => p.Price, 99.99m)                  // >
    .FilterLessThan(p => p.Stock, 10)                         // <
    .FilterGreaterThanOrEqual(p => p.Rating, 4.5)             // >=
    .FilterLessThanOrEqual(p => p.Weight, 5.0)                // <=
    .FilterByName("CustomProperty", "==", "value");           // Custom properties
```

## Sorting

```csharp
var query = SieveQueryBuilder<Product>.Create()
    .SortBy(p => p.Category)              // Ascending
    .SortByDescending(p => p.CreatedAt)   // Descending
    .SortByName("CustomSort", true)       // Custom property, descending
    .BuildQueryString();

// Result: "sorts=Category,-CreatedAt,-CustomSort"
```

## Real-World Example

```csharp
public class ProductController : ControllerBase
{
    private readonly ISieveProcessor _sieveProcessor;
    private readonly DbContext _db;

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] bool? onSale,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var builder = SieveQueryBuilder<Product>.Create()
            .Page(page)
            .PageSize(pageSize)
            .SortByDescending(p => p.CreatedAt);

        // Dynamic OR conditions based on parameters
        if (!string.IsNullOrEmpty(category))
        {
            builder.FilterEquals(p => p.Category, category);
        }

        if (minPrice.HasValue)
        {
            if (builder.GetFilters().Any())
                builder.Or();

            builder.FilterGreaterThan(p => p.Price, minPrice.Value);
        }

        if (onSale == true)
        {
            if (builder.GetFilters().Any())
                builder.Or();

            builder.FilterEquals(p => p.OnSale, true);
        }

        var sieveModel = builder.BuildSieveModel();
        var query = _db.Products.AsNoTracking();
        var results = _sieveProcessor.Apply(sieveModel, query);

        return await results.ToListAsync();
    }
}
```

## Output Formats

### Query String

```csharp
string queryString = builder.BuildQueryString();
// "filters=Name@=Bob || Age>25&sorts=-CreatedAt&page=1&pageSize=20"
```

### SieveModel

```csharp
SieveModel model = builder.BuildSieveModel();
// Use directly with Sieve processor
```

### Individual Components

```csharp
string filters = builder.BuildFiltersString();  // "Name@=Bob || Age>25"
string sorts = builder.BuildSortsString();      // "-CreatedAt,Name"
int? page = builder.GetPage();                  // 1
int? pageSize = builder.GetPageSize();          // 20
```

## ASP.NET Core Integration with NSwag

```csharp
using Sieve.QueryBuilder;

public class AuthorQueryModel : ISieveQueryModel
{
    public string? Name { get; set; }
    public int? BooksCount { get; set; }
}

[HttpGet(nameof(GetAuthors))]
public async Task<List<AuthorResponseDto>> GetAuthors(
    [FromQuery] AuthorQueryModel queryModel)  // OpenAPI/Swagger compatible!
{
    var builder = SieveQueryBuilder<AuthorQueryModel>.ParseQueryString(
        Request.QueryString.ToString()
    );

    var sieveModel = builder.BuildSieveModel();
    var results = _sieveProcessor.Apply(sieveModel, _db.Authors);

    return await results.Select(a => new AuthorResponseDto
    {
        Name = a.Name,
        BooksCount = a.Books.Count
    }).ToListAsync();
}
```

## Version Compatibility

| Sieve.QueryBuilder.Fork | Sieve.Fork | Features |
|-------------------------|------------|----------|
| 2.6.0                   | 2.6.0+     | Full OR query support with `\|\|` |
| 2.6.0                   | 2.0.0+     | Basic filtering (no OR groups) |

## Migration from Original

If migrating from `Sieve.Query.Builder`:

```csharp
// Old namespace
using SieveQueryBuilder;

// New namespace
using Sieve.QueryBuilder;

// Old package
dotnet remove package Sieve.Query.Builder

// New package
dotnet add package Sieve.QueryBuilder.Fork
```

## License

MIT License - see LICENSE file for details.

## Contributing

Contributions welcome! This is a companion package to [Sieve.Fork](https://github.com/uldahlalex/Sieve).

## Links

- [Sieve.Fork Documentation](https://github.com/uldahlalex/Sieve)
- [Original Sieve](https://github.com/Biarity/Sieve)
- [Report Issues](https://github.com/uldahlalex/Sieve/issues)

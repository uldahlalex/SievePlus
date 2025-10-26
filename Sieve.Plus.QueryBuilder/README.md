# Sieve.Plus.QueryBuilder

> **Type-safe query builder for Sieve.Plus with powerful OR queries and parentheses grouping**

Build Sieve.Plus queries with compile-time safety, IntelliSense support, and full OR query capabilities including parentheses grouping.

[![NuGet Release](https://img.shields.io/nuget/v/Sieve.Plus.QueryBuilder)](https://www.nuget.org/packages/Sieve.Plus.QueryBuilder)

## Features

- ✅ **Type-safe query building** - Use lambda expressions instead of magic strings
- ✅ **Powerful OR queries** - Full support for `||` operator and parentheses grouping
- ✅ **Query models** - IntelliSense for custom mapped properties
- ✅ **Round-trip parsing** - Parse query strings and SieveModels back to builders
- ✅ **Inspection API** - Examine filters, filter groups, and sorts programmatically
- ✅ **All operators** - `==`, `!=`, `@=`, `_=`, `>`, `<`, `>=`, `<=`
- ✅ **Fluent API** - Chain methods for readable construction
- ✅ **Multi-framework** - Supports netstandard2.0 → net9.0

## Installation

```bash
dotnet add package Sieve.Plus.QueryBuilder
dotnet add package Sieve.Plus
```

## Quick Start

### Basic Query Building

```csharp
using Sieve.Plus.QueryBuilder;

public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}

// Build a type-safe query
var sieveModel = SievePlusQueryBuilder<Product>.Create()
    .FilterContains(p => p.Name, "laptop")
    .FilterGreaterThan(p => p.Price, 500)
    .SortByDescending(p => p.Price)
    .Page(1)
    .PageSize(20)
    .BuildSieveModel();

// Use with Sieve processor
var results = _sieveProcessor.Apply(sieveModel, _db.Products);
```

## Powerful OR Queries

### Simple OR

```csharp
// Products where category is Electronics OR price is greater than $100
var query = SievePlusQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Electronics")
    .Or()
    .FilterGreaterThan(p => p.Price, 100)
    .BuildFiltersString();

// Output: "Category==Electronics || Price>100"
```

### Parentheses Grouping

Use `BeginGroup()` and `EndGroup()` for explicit parentheses:

```csharp
// (Category is Electronics OR Computers) AND Price > $500
var query = SievePlusQueryBuilder<Product>.Create()
    .BeginGroup()
        .FilterEquals(p => p.Category, "Electronics")
        .Or()
        .FilterEquals(p => p.Category, "Computers")
    .EndGroup()
    .FilterGreaterThan(p => p.Price, 500)
    .BuildFiltersString();

// Output: "(Category==Electronics || Category==Computers),Price>500"
```

### Helper Method: FilterWithAlternatives

Convenient method for filtering by multiple values on one property:

```csharp
// Products with processor Intel i9, AMD Ryzen 9, or Apple M2, and price > $1000
var query = SievePlusQueryBuilder<Computer>.Create()
    .FilterWithAlternatives(
        c => c.Processor,
        new[] { "Intel i9", "AMD Ryzen 9", "Apple M2" },
        b => b.FilterGreaterThan(c => c.Price, 1000)
    )
    .BuildFiltersString();

// Output: "(Processor==Intel i9 || Processor==AMD Ryzen 9 || Processor==Apple M2),Price>1000"
```

### Complex Nested Groups

```csharp
// ((Title A OR Title B) AND Pages > 100) AND Price < 50
var query = SievePlusQueryBuilder<Book>.Create()
    .BeginGroup()
        .BeginGroup()
            .FilterEquals(b => b.Title, "Book A")
            .Or()
            .FilterEquals(b => b.Title, "Book B")
        .EndGroup()
        .FilterGreaterThan(b => b.Pages, 100)
    .EndGroup()
    .FilterLessThan(b => b.Price, 50)
    .BuildFiltersString();

// Output: "((Title==Book A || Title==Book B),Pages>100),Price<50"
```

## Real-World Example: Pricerunner-Style Filtering

A common pattern for e-commerce filtering where users select from options with shared constraints:

```csharp
public class Computer
{
    public string Processor { get; set; }
    public decimal Price { get; set; }
    public decimal ScreenSize { get; set; }
}

// User wants: (Intel i9 OR AMD Ryzen 9) with price $1000-$2000 and screen 14-16"
var query = SievePlusQueryBuilder<Computer>.Create()
    .BeginGroup()
        .FilterEquals(c => c.Processor, "Intel i9")
        .Or()
        .FilterEquals(c => c.Processor, "AMD Ryzen 9")
    .EndGroup()
    .FilterGreaterThanOrEqual(c => c.Price, 1000)
    .FilterLessThanOrEqual(c => c.Price, 2000)
    .FilterGreaterThanOrEqual(c => c.ScreenSize, 14)
    .FilterLessThanOrEqual(c => c.ScreenSize, 16)
    .BuildFiltersString();

// Output: "(Processor==Intel i9 || Processor==AMD Ryzen 9),Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16"
```

This expands to a cartesian product:
- `(Processor==Intel i9),Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16`
- `(Processor==AMD Ryzen 9),Price>=1000,Price<=2000,ScreenSize>=14,ScreenSize<=16`

### Dynamic Builder for Multiple Options

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
                builder.Or(); // Start new OR group for each processor after the first

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

## All Filter Operators

```csharp
var builder = SievePlusQueryBuilder<Product>.Create()
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
var query = SievePlusQueryBuilder<Product>.Create()
    .SortBy(p => p.Category)              // Ascending
    .SortByDescending(p => p.CreatedAt)   // Descending
    .SortByName("CustomSort", true)       // Custom property, descending
    .BuildSortsString();

// Output: "Category,-CreatedAt,-CustomSort"
```

## Query Models - Type-Safe Custom Properties

Query models provide compile-time safety for custom mapped properties:

```csharp
using Sieve.Plus.QueryBuilder;

// Define query model matching your SieveProcessor configuration
public class AuthorQueryModel : ISieveQueryModel
{
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? BooksCount { get; set; }  // Custom mapped property
}

// In your SieveProcessor
public class ApplicationSieveProcessor : SievePlusProcessor
{
    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        mapper.Property<Author>(a => a.Books.Count)
            .CanFilter()
            .CanSort()
            .HasName("BooksCount");  // Maps to query model property

        return mapper;
    }
}

// Type-safe queries with IntelliSense for custom properties
var query = SievePlusQueryBuilder<AuthorQueryModel>.Create()
    .FilterContains(a => a.Name, "Bob")
    .Or()
    .FilterGreaterThan(a => a.BooksCount, 5)  // IntelliSense works!
    .SortByDescending(a => a.BooksCount)
    .BuildSieveModel();
```

## Round-Trip Parsing

### Parse Query Strings

```csharp
// Parse existing query string
var builder = SievePlusQueryBuilder<Product>.ParseQueryString(
    "filters=(Category==Electronics || Category==Computers),Price>500&sorts=-CreatedAt&page=1&pageSize=20"
);

// Modify and rebuild
builder.FilterEquals(p => p.InStock, true)
       .BuildQueryString();
```

### Parse from SieveModel

```csharp
var existingModel = new SievePlusModel
{
    Filters = "Name@=Bob || Age>25",
    Sorts = "-CreatedAt",
    Page = 1,
    PageSize = 20
};

var builder = SievePlusQueryBuilder<Person>.FromSieveModel(existingModel);

// Continue building
builder.FilterEquals(p => p.Active, true)
       .SortBy(p => p.Name);

var updatedModel = builder.BuildSieveModel();
```

## Inspection API

Examine filters and filter groups programmatically:

```csharp
var builder = SievePlusQueryBuilder<Product>.Create()
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

// Get pagination
int? page = builder.GetPage();
int? pageSize = builder.GetPageSize();
```

## Output Formats

### SieveModel Object

```csharp
var model = builder.BuildSieveModel();
// Use directly with Sieve processor
var results = _sieveProcessor.Apply(model, query);
```

### Query String

```csharp
string queryString = builder.BuildQueryString();
// "filters=(Name@=Bob || Age>25)&sorts=-CreatedAt&page=1&pageSize=20"
```

### Individual Components

```csharp
string filters = builder.BuildFiltersString();
// "(Name@=Bob || Age>25)"

string sorts = builder.BuildSortsString();
// "-CreatedAt,Name"

int? page = builder.GetPage();         // 1
int? pageSize = builder.GetPageSize();  // 20
```

## ASP.NET Core Integration

### Basic Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISievePlusProcessor _sieve;

    public ProductsController(ApplicationDbContext db, ISievePlusProcessor sieve)
    {
        _db = db;
        _sieve = sieve;
    }

    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetProducts(
        [FromQuery] string? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] bool? onSale,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var builder = SievePlusQueryBuilder<Product>.Create()
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
        var results = _sieve.Apply(sieveModel, query);

        return await results.ToListAsync();
    }
}
```

### With Query Models

```csharp
public class AuthorQueryModel : ISieveQueryModel
{
    public string? Name { get; set; }
    public int? BooksCount { get; set; }
}

[HttpGet(nameof(GetAuthors))]
public async Task<List<AuthorResponseDto>> GetAuthors(
    [FromQuery] string? searchTerm,
    [FromQuery] int? minBooks,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var builder = SievePlusQueryBuilder<AuthorQueryModel>.Create()
        .Page(page)
        .PageSize(pageSize);

    if (!string.IsNullOrEmpty(searchTerm))
    {
        builder.FilterContains(a => a.Name, searchTerm);
    }

    if (minBooks.HasValue)
    {
        builder.FilterGreaterThanOrEqual(a => a.BooksCount, minBooks.Value);
    }

    var sieveModel = builder.BuildSieveModel();
    var results = _sieveProcessor.Apply(sieveModel, _db.Authors);

    return await results.Select(a => new AuthorResponseDto
    {
        Name = a.Name,
        BooksCount = a.Books.Count
    }).ToListAsync();
}
```

## Date Handling

Dates are automatically formatted to ISO 8601 with UTC:

```csharp
var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

var query = SievePlusQueryBuilder<Post>.Create()
    .FilterGreaterThanOrEqual(p => p.CreatedAt, thirtyDaysAgo)
    .BuildFiltersString();

// Output: "CreatedAt>=2024-10-25T12:00:00.000Z"
```

## Error Handling

Mismatched `BeginGroup()` and `EndGroup()` calls throw exceptions:

```csharp
// ❌ Throws InvalidOperationException: "Unmatched BeginGroup() call - missing EndGroup()"
var query = SievePlusQueryBuilder<Product>.Create()
    .BeginGroup()
        .FilterEquals(p => p.Name, "Test")
    .BuildFiltersString();

// ❌ Throws InvalidOperationException: "EndGroup() called without matching BeginGroup()"
var query2 = SievePlusQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Name, "Test")
    .EndGroup()
    .BuildFiltersString();
```

## Best Practices

### 1. Use Query Models for Large Projects

```csharp
// Define query models that match your SieveProcessor configuration
public class ProductQueryModel : ISieveQueryModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? CategoryName { get; set; }  // Mapped from nested property
}

// Get IntelliSense and compile-time safety
var query = SievePlusQueryBuilder<ProductQueryModel>.Create()
    .FilterContains(p => p.CategoryName, "Electronics")  // Type-safe!
    .BuildSieveModel();
```

### 2. Encapsulate Complex Queries

```csharp
public static class ProductQueries
{
    public static SievePlusQueryBuilder<Product> PopularProducts()
    {
        return SievePlusQueryBuilder<Product>.Create()
            .FilterGreaterThan(p => p.Rating, 4.0)
            .FilterGreaterThan(p => p.Sales, 1000)
            .SortByDescending(p => p.Sales);
    }

    public static SievePlusQueryBuilder<Product> InPriceRange(decimal min, decimal max)
    {
        return SievePlusQueryBuilder<Product>.Create()
            .FilterGreaterThanOrEqual(p => p.Price, min)
            .FilterLessThanOrEqual(p => p.Price, max);
    }
}

// Usage
var popular = ProductQueries.PopularProducts()
    .Page(1)
    .PageSize(20)
    .BuildSieveModel();
```

### 3. When Using OR, Repeat Shared Constraints

When filtering with OR groups, shared constraints must be repeated in each group:

```csharp
// ❌ WRONG - Constraints only apply to second group
var wrong = SievePlusQueryBuilder<Product>.Create()
    .FilterEquals(p => p.Category, "Electronics")
    .Or()
    .FilterEquals(p => p.Category, "Computers")
    .FilterGreaterThan(p => p.Price, 500)  // Only applies to Computers!
    .BuildFiltersString();
// Output: "Category==Electronics || Category==Computers,Price>500"

// ✅ CORRECT - Use parentheses and repeat constraints
var correct = SievePlusQueryBuilder<Product>.Create()
    .BeginGroup()
        .FilterEquals(p => p.Category, "Electronics")
        .Or()
        .FilterEquals(p => p.Category, "Computers")
    .EndGroup()
    .FilterGreaterThan(p => p.Price, 500)  // Applies to both!
    .BuildFiltersString();
// Output: "(Category==Electronics || Category==Computers),Price>500"
```

## Framework Support

Supports the following target frameworks:
- netstandard2.0
- netstandard2.1
- net6.0
- net8.0
- net9.0

## Examples

See the [unit tests](../Sieve.Plus.UnitTests/QueryBuilderParenthesesTests.cs) for comprehensive examples of all features.

## Related Packages

| Package | Description |
|---------|-------------|
| [Sieve.Plus](https://www.nuget.org/packages/Sieve.Plus) | Core filtering/sorting/pagination library |
| [ts-sieve-plus-query-builder](https://www.npmjs.com/package/ts-sieve-plus-query-builder) | TypeScript equivalent for frontend |

## License

Apache License 2.0 - See [LICENSE](../LICENSE) for details.

## Contributing

Contributions welcome!

**Repository**: [https://github.com/uldahlalex/sieveplus](https://github.com/uldahlalex/sieveplus)

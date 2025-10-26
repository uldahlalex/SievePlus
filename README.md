# Sieve.Plus ⚗️➕

> **Modern .NET filtering, sorting, and pagination with powerful OR queries**

Sieve.Plus is a simple, clean, and extensible framework for .NET that adds **sorting, filtering, and pagination** functionality to your ASP.NET Core APIs. Built as an enhanced fork of the original Sieve library with full backward compatibility.

[![NuGet Release](https://img.shields.io/nuget/v/Sieve.Plus?style=for-the-badge)](https://www.nuget.org/packages/Sieve.Plus)

## What's New in Sieve.Plus?

- ✨ **Powerful OR Queries** - Use `||` operator and parentheses for complex filtering logic
- 🎯 **Explicit Query Models** - Define exactly what's queryable, separate from your entities
- 🔒 **Type-Safe Query Builder** - Build queries with compile-time safety (separate `Sieve.Plus.QueryBuilder` package)
- 🌐 **TypeScript Support** - Matching query builder for TypeScript/JavaScript frontends
- 🔄 **Full Backward Compatibility** - Drop-in replacement for original Sieve
- 📦 **Multi-Framework Support** - netstandard2.0, netstandard2.1, net6.0, net8.0, net9.0

## Quick Start

### 1. Installation

```bash
dotnet add package Sieve.Plus
```

### 2. Configure Services

In `Program.cs` (or `Startup.cs` for older projects):

```csharp
using Sieve.Plus.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Sieve.Plus
builder.Services.AddScoped<SievePlusProcessor>();

// Optional: Configure options
builder.Services.Configure<SieveOptions>(builder.Configuration.GetSection("Sieve"));
```

Optional `appsettings.json` configuration:

```json
{
  "Sieve": {
    "DefaultPageSize": 10,
    "MaxPageSize": 100,
    "ThrowExceptions": true,
    "CaseSensitive": false
  }
}
```

### 3. Define Query Models

Query models define **exactly** what properties can be filtered and sorted. This separates your queryable API from your database schema.

```csharp
using Sieve.Plus.Models;

// Your entity (database model)
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool InStock { get; set; }
    public int Sales { get; set; }
}

// Query model - defines what CAN be queried
public class ProductQueryModel : ISievePlusQueryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool InStock { get; set; }

    // Custom filter (doesn't exist on entity)
    public bool? IsPopular { get; set; }  // Sales > 1000
}
```

### 4. Configure Query Model Mapping

```csharp
using Sieve.Plus.Services;

public class ProductQueryConfiguration : ISievePlusQueryConfiguration<ProductQueryModel, Product>
{
    public void Configure(SievePlusQueryMapper<ProductQueryModel, Product> mapper)
    {
        // Map queryable properties
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Name, e => e.Name).CanFilter().CanSort();
        mapper.Property(q => q.Price, e => e.Price).CanFilter().CanSort();
        mapper.Property(q => q.Category, e => e.Category).CanFilter().CanSort();
        mapper.Property(q => q.CreatedAt, e => e.CreatedAt).CanFilter().CanSort();
        mapper.Property(q => q.InStock, e => e.InStock).CanFilter();

        // Custom filter with expression
        mapper.CustomFilter(q => q.IsPopular, e => e.Sales > 1000);
    }
}
```

### 5. Register in Your Processor

```csharp
using Sieve.Plus.Services;
using Microsoft.Extensions.Options;

public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    public ApplicationSievePlusProcessor(
        IOptions<SieveOptions> options,
        ISieveCustomSortMethods? customSortMethods = null,
        ISieveCustomFilterMethods? customFilterMethods = null)
        : base(options, customSortMethods, customFilterMethods)
    {
    }

    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        // Register individual configurations
        registry.AddConfiguration<ProductQueryConfiguration>();

        // Or scan entire assembly
        // registry.AddConfigurationsFromAssembly(typeof(ProductQueryConfiguration).Assembly);
    }
}

// Register in Program.cs
builder.Services.AddScoped<ISievePlusProcessor, ApplicationSievePlusProcessor>();
```

### 6. Create Your Controller

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sieve.Plus.Models;
using Sieve.Plus.Services;

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
        [FromQuery] SievePlusModel model)
    {
        var query = _db.Products.AsNoTracking();

        // Apply with explicit query model
        var result = _sieve.Apply<Product, ProductQueryModel>(model, query);

        return await result.ToListAsync();
    }
}
```

### 7. Make API Requests

```bash
# Simple filtering
GET /api/products?filters=Price>100&sorts=-CreatedAt&page=1&pageSize=20

# OR queries - Products that are Electronics OR cost more than $100
GET /api/products?filters=Category==Electronics || Price>100

# Parentheses for complex logic - (Electronics OR Computers) AND expensive
GET /api/products?filters=(Category==Electronics || Category==Computers),Price>500

# Custom filter
GET /api/products?filters=IsPopular==true

# Search across multiple fields
GET /api/products?filters=Name@=laptop || Category@=computer
```

## Why Query Models?

Query models solve the problem where your **entity** (database schema) doesn't perfectly match what should be **queryable** (API contract).

### Benefits

1. **Perfect Correspondence** - The query model IS exactly what can be queried (no confusion)
2. **Self-Documenting** - Look at the query model to see what's queryable
3. **Type-Safe Query Building** - Query builders use the model for IntelliSense
4. **Custom Properties** - Custom filters appear as regular properties
5. **TypeScript Generation** - Easily generate TypeScript types from query models
6. **Separation of Concerns** - Query API is separate from database schema

### Example: Complex Query Model

```csharp
public class BookQueryModel : ISievePlusQueryModel
{
    // Basic properties
    public string Id { get; set; }
    public string Title { get; set; }
    public int Pages { get; set; }

    // Navigation properties (flattened)
    public string GenreName { get; set; }      // From book.Genre.Name
    public string AuthorName { get; set; }     // From book.Author.Name

    // Calculated properties
    public int PublishedYear { get; set; }     // From book.PublishedDate.Year

    // Custom filters
    public bool? IsLongBook { get; set; }      // Pages > 500
    public bool? IsRecent { get; set; }        // Published in last year
}

public class BookQueryConfiguration : ISievePlusQueryConfiguration<BookQueryModel, Book>
{
    public void Configure(SievePlusQueryMapper<BookQueryModel, Book> mapper)
    {
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Title, e => e.Title).CanFilter().CanSort();
        mapper.Property(q => q.Pages, e => e.Pages).CanFilter().CanSort();

        // Navigation properties
        mapper.Property(q => q.GenreName, e => e.Genre.Name).CanFilter().CanSort();
        mapper.Property(q => q.AuthorName, e => e.Author.Name).CanFilter().CanSort();

        // Calculated properties
        mapper.Property(q => q.PublishedYear, e => e.PublishedDate.Year).CanFilter().CanSort();

        // Custom filters
        mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);
        mapper.CustomFilter(q => q.IsRecent, e => e.PublishedDate > DateTime.UtcNow.AddYears(-1));
    }
}
```

## Powerful OR Queries

Sieve.Plus introduces advanced filtering with OR logic and parentheses:

### Simple OR

```bash
# Products where category is Electronics OR price is greater than $100
GET /api/products?filters=Category==Electronics || Price>100
```

### Parentheses Grouping

```bash
# (Electronics OR Computers) AND Price > $500
GET /api/products?filters=(Category==Electronics || Category==Computers),Price>500

# Complex: High-value OR premium products
GET /api/products?filters=(Price>1000,InStock==true) || (Category==Premium)
```

### Multiple OR Groups (Cartesian Product)

```bash
# (Processor A OR B) AND (Price range) AND (Screen size range)
GET /api/products?filters=(Processor==Intel || Processor==AMD),(Price>=1000,Price<=2000),(ScreenSize>=14,ScreenSize<=16)
```

This expands to all combinations:
- Intel AND Price 1000-2000 AND Screen 14-16
- AMD AND Price 1000-2000 AND Screen 14-16

## Type-Safe Query Building

Install the companion package for compile-time safety:

```bash
dotnet add package Sieve.Plus.QueryBuilder
```

Build queries with IntelliSense and type checking using your query models:

```csharp
using Sieve.Plus.QueryBuilder;

// Type-safe query building with query model
var sieveModel = SievePlusQueryBuilder<ProductQueryModel>.Create()
    .BeginGroup()
        .FilterEquals(p => p.Category, "Electronics")
        .Or()
        .FilterEquals(p => p.Category, "Computers")
    .EndGroup()
    .FilterGreaterThan(p => p.Price, 500)
    .FilterEquals(p => p.IsPopular, true)  // Custom filter with IntelliSense!
    .SortByDescending(p => p.CreatedAt)
    .Page(1)
    .PageSize(20)
    .BuildSieveModel();

// Use with Sieve processor
var query = _db.Products.AsNoTracking();
var results = _sieve.Apply<Product, ProductQueryModel>(sieveModel, query);
```

**See the [Sieve.Plus.QueryBuilder README](Sieve.Plus.QueryBuilder/README.md) for complete documentation.**

## TypeScript Frontend Support

Build matching queries on the frontend with full type safety:

```bash
npm install ts-sieve-plus-query-builder
```

```typescript
import { SieveQueryBuilder } from 'ts-sieve-plus-query-builder';

// Define interface matching your query model
interface ProductQueryModel {
  id: number;
  name: string;
  price: number;
  category: string;
  isPopular?: boolean;  // Custom filter
}

const queryString = SieveQueryBuilder.create<ProductQueryModel>()
  .beginGroup()
    .filterEquals('category', 'Electronics')
    .or()
    .filterEquals('category', 'Computers')
  .endGroup()
  .filterGreaterThan('price', 500)
  .filterEquals('isPopular', true)  // Type-safe custom filter!
  .buildQueryString();

// Use with fetch, axios, etc.
const response = await fetch(`/api/products?${queryString}`);
```

**See the [ts-sieve-plus-query-builder README](ts-sieve-plus-query-builder/README.md) for complete documentation.**

## Filter Operators

| Operator | Meaning                              | Example              |
|----------|--------------------------------------|----------------------|
| `==`     | Equals                               | `Price==100`         |
| `!=`     | Not equals                           | `Category!=Books`    |
| `>`      | Greater than                         | `Price>50`           |
| `<`      | Less than                            | `Stock<10`           |
| `>=`     | Greater than or equal                | `Rating>=4.5`        |
| `<=`     | Less than or equal                   | `Weight<=5`          |
| `@=`     | Contains                             | `Name@=laptop`       |
| `_=`     | Starts with                          | `Name_=Pro`          |
| `_-=`    | Ends with                            | `Name_-=Max`         |
| `!@=`    | Does not contain                     | `Name!@=refurb`      |
| `!_=`    | Does not start with                  | `Name!_=Old`         |
| `@=*`    | Case-insensitive contains            | `Name@=*laptop`      |
| `==*`    | Case-insensitive equals              | `Category==*books`   |

## Advanced Features

### Deferred Pagination (for Total Count)

Get total count before pagination:

```csharp
[HttpGet]
public async Task<ActionResult<ProductListResponse>> GetProducts(
    [FromQuery] SievePlusModel model)
{
    var query = _db.Products.AsNoTracking();

    // Apply only filtering and sorting
    var filtered = _sieve.Apply<Product, ProductQueryModel>(model, query,
        applyFiltering: true,
        applySorting: true,
        applyPagination: false);

    var totalCount = await filtered.CountAsync();

    // Now apply pagination
    var paginated = _sieve.Apply<Product, ProductQueryModel>(model, filtered,
        applyFiltering: false,
        applySorting: false,
        applyPagination: true);

    var items = await paginated.ToListAsync();

    return new ProductListResponse
    {
        Items = items,
        TotalCount = totalCount,
        Page = model.Page ?? 1,
        PageSize = model.PageSize ?? 10
    };
}
```

### Custom Filter Methods

For complex filters that can't be expressed with simple expressions:

```csharp
public class ProductQueryModel : ISievePlusQueryModel
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    // Custom filter as method
    public bool? InPriceRange { get; set; }
}

public class CustomFilterMethods : ISieveCustomFilterMethods
{
    // Custom filter method: filters=InPriceRange==100|500
    public IQueryable<Product> InPriceRange(IQueryable<Product> source, string op, string[] values)
    {
        if (values.Length < 2) return source;

        var min = decimal.Parse(values[0]);
        var max = decimal.Parse(values[1]);

        return source.Where(p => p.Price >= min && p.Price <= max);
    }
}

// Register in Program.cs
builder.Services.AddScoped<ISieveCustomFilterMethods, CustomFilterMethods>();
```

## Query Syntax Reference

```bash
# Basic query structure
?filters={filter_expression}&sorts={sort_expression}&page={number}&pageSize={number}

# Filter syntax
{PropertyName}{Operator}{Value}

# Multiple filters (AND)
filters=Name@=laptop,Price>500,InStock==true

# OR between groups
filters=Category==Electronics || Category==Computers

# Parentheses for grouping
filters=(Category==Electronics || Category==Computers),Price>500

# Multiple values for same property (OR within property)
filters=Category@=laptop|desktop|tablet

# Escaping special characters
filters=Name@=some\,title     # Match "some,title"
filters=Name@=some\|value     # Match "some|value"

# Sorting
sorts=Price              # Ascending by price
sorts=-Price             # Descending by price
sorts=-CreatedAt,Name    # Descending by created, then ascending by name

# Pagination
page=1&pageSize=20
```

## Error Handling

Enable exception throwing for better error messages:

```json
{
  "Sieve": {
    "ThrowExceptions": true
  }
}
```

Handle Sieve exceptions in your middleware:

```csharp
public class SieveExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SieveExceptionMiddleware> _logger;

    public SieveExceptionMiddleware(RequestDelegate next, ILogger<SieveExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SieveException ex)
        {
            _logger.LogWarning(ex, "Sieve query error");

            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid query",
                message = ex.Message
            });
        }
    }
}

// Register in Program.cs
app.UseMiddleware<SieveExceptionMiddleware>();
```

## Real-World Example: E-Commerce API

Complete example with query models, custom filters, and OR queries:

```csharp
// Entity
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Category Category { get; set; }
    public bool InStock { get; set; }
    public double Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int Sales { get; set; }
    public List<ProductTag> Tags { get; set; }
}

// Query Model
public class ProductQueryModel : ISievePlusQueryModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; }  // From navigation
    public bool InStock { get; set; }
    public double Rating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int TagCount { get; set; }          // Calculated

    // Custom filters
    public bool? IsPopular { get; set; }       // Sales > 1000 && Rating >= 4.0
    public bool? IsPremium { get; set; }       // Price > 1000
}

// Configuration
public class ProductQueryConfiguration : ISievePlusQueryConfiguration<ProductQueryModel, Product>
{
    public void Configure(SievePlusQueryMapper<ProductQueryModel, Product> mapper)
    {
        mapper.Property(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property(q => q.Name, e => e.Name).CanFilter().CanSort();
        mapper.Property(q => q.Price, e => e.Price).CanFilter().CanSort();
        mapper.Property(q => q.CategoryName, e => e.Category.Name).CanFilter().CanSort();
        mapper.Property(q => q.InStock, e => e.InStock).CanFilter();
        mapper.Property(q => q.Rating, e => e.Rating).CanFilter().CanSort();
        mapper.Property(q => q.CreatedAt, e => e.CreatedAt).CanFilter().CanSort();
        mapper.Property(q => q.TagCount, e => e.Tags.Count).CanFilter().CanSort();

        // Custom filters
        mapper.CustomFilter(q => q.IsPopular, e => e.Sales > 1000 && e.Rating >= 4.0);
        mapper.CustomFilter(q => q.IsPremium, e => e.Price > 1000);
    }
}

// Processor
public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    public ApplicationSievePlusProcessor(IOptions<SieveOptions> options)
        : base(options)
    {
    }

    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        registry.AddConfiguration<ProductQueryConfiguration>();
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ISievePlusProcessor _sieve;

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] SievePlusModel model)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .AsNoTracking();

        // Get total before pagination
        var filtered = _sieve.Apply<Product, ProductQueryModel>(model, query,
            applyPagination: false);
        var total = await filtered.CountAsync();

        // Apply pagination
        var results = _sieve.Apply<Product, ProductQueryModel>(model, filtered);
        var items = await results.ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = items.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category.Name,
                Rating = p.Rating
            }).ToList(),
            TotalCount = total,
            Page = model.Page ?? 1,
            PageSize = model.PageSize ?? 20
        };
    }
}
```

Example queries:

```bash
# Products in Electronics or Computers category, over $500
GET /api/products?filters=(CategoryName==Electronics || CategoryName==Computers),Price>500&sorts=-CreatedAt

# High-rated OR premium products
GET /api/products?filters=IsPopular==true || IsPremium==true&pageSize=50

# Search by name OR category
GET /api/products?filters=Name@=laptop || CategoryName@=computer

# Complex: Premium products OR (budget products in stock)
GET /api/products?filters=(IsPremium==true) || (Price<100,InStock==true)
```

## Alternative: Attribute-Based Configuration (Legacy)

If you prefer, you can still use the older attribute-based approach:

```csharp
public class Product
{
    public int Id { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Price { get; set; }
}

// Use without query model
var results = _sieve.Apply(sieveModel, _db.Products);
```

However, we **strongly recommend using Query Models** for new projects as they provide better separation of concerns, type safety, and clarity.

## Migration from Original Sieve

Sieve.Plus is a drop-in replacement:

```bash
# 1. Update package reference
dotnet remove package Sieve
dotnet add package Sieve.Plus

# 2. Update using statements
# Old:
using Sieve.Services;
using Sieve.Models;

# New:
using Sieve.Plus.Services;
using Sieve.Plus.Models;

# 3. Update service registration
# Old:
services.AddScoped<SieveProcessor>();

# New:
services.AddScoped<SievePlusProcessor>();

# That's it! All existing queries continue to work.
# Gradually migrate to Query Models for better type safety.
```

## Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| **Sieve.Plus** | Core filtering/sorting/pagination | [![NuGet](https://img.shields.io/nuget/v/Sieve.Plus)](https://www.nuget.org/packages/Sieve.Plus) |
| **Sieve.Plus.QueryBuilder** | .NET type-safe query builder | [![NuGet](https://img.shields.io/nuget/v/Sieve.Plus.QueryBuilder)](https://www.nuget.org/packages/Sieve.Plus.QueryBuilder) |
| **ts-sieve-plus-query-builder** | TypeScript query builder | [![npm](https://img.shields.io/npm/v/ts-sieve-plus-query-builder)](https://www.npmjs.com/package/ts-sieve-plus-query-builder) |

## Attribution

This project is a fork of the original [Sieve](https://github.com/Biarity/Sieve) library by Biarity.

**Original Project**: [Sieve by Biarity](https://github.com/Biarity/Sieve)
**Original Copyright**: 2018 Biarity, 2021 Ashish Patel and Kevin Sommer
**License**: Apache License 2.0

Sieve.Plus maintains full backward compatibility while adding powerful OR query support, parentheses grouping, explicit query models, and type-safe query building capabilities. See the [NOTICE](NOTICE) file for complete attribution details.

## License

Apache License 2.0 - See [LICENSE](LICENSE) for details.

## Contributing

Contributions are highly appreciated!

**Repository**: [https://github.com/uldahlalex/sieveplus](https://github.com/uldahlalex/sieveplus)

**Original Sieve Project**: [https://github.com/Biarity/Sieve](https://github.com/Biarity/Sieve)

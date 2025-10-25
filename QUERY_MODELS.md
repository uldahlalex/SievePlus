# Explicit Query Models in SievePlus

## Overview

The new **Explicit Query Model** feature solves the problem where the entity model doesn't perfectly correspond to what can be filtered/sorted. With explicit query models, you define exactly what properties are queryable, separate from your entity structure.

## The Problem

Previously, SievePlus mixed concerns:
- Your **entity** represents the database schema
- Your **queryable surface** is configured separately via attributes or fluent API
- Result: The entity type parameter in queries is "misleading" because not all properties are actually queryable

## The Solution

**Explicit Query Models** separate these concerns:

```csharp
// 1. Define what CAN be queried (Query Model)
public class BookQueryModel : ISievePlusQueryModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public int Pages { get; set; }

    // Custom properties that don't exist on the entity
    public bool? IsLongBook { get; set; }
    public string GenreName { get; set; }  // From navigation
}

// 2. Map query model to entity (Configuration)
public class BookQueryConfiguration : ISievePlusQueryConfiguration<BookQueryModel, Book>
{
    public void Configure(SievePlusQueryMapper<BookQueryModel, Book> mapper)
    {
        mapper.Property<string>(q => q.Id, e => e.Id).CanFilter().CanSort();
        mapper.Property<string>(q => q.Title, e => e.Title).CanFilter().CanSort();
        mapper.Property<int>(q => q.Pages, e => e.Pages).CanFilter().CanSort();

        // Navigation properties
        mapper.Property<string>(q => q.GenreName, e => e.Genre.Name).CanFilter().CanSort();

        // Custom filters
        mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);
    }
}

// 3. Register in your processor
public class ApplicationSievePlusProcessor : SievePlusProcessor
{
    protected override void ConfigureQueryModels(SievePlusQueryModelRegistry registry)
    {
        registry.AddConfiguration<BookQueryConfiguration>();
        // Or scan assembly: registry.AddConfigurationsFromAssembly(assembly);
    }
}

// 4. Use in queries with explicit query model type
var sieveModel = new SievePlusModel { Filters = "IsLongBook==true,Title@=*Harry" };
var books = processor.Apply<Book, BookQueryModel>(sieveModel, dbContext.Books);
```

## Benefits

1. **Perfect Correspondence** - `BookQueryModel` IS exactly what can be queried (no confusion)
2. **Self-Documenting** - Look at the query model to see what's queryable
3. **Type-Safe Query Building** - Future query builders can use the query model for intellisense
4. **Custom Properties** - Custom filters appear as regular properties in the query model
5. **TypeScript Generation** - Query models can be easily generated as TypeScript types
6. **Separation of Concerns** - Query API is separate from database schema

## Key Design Decisions

### Property Types Must Match

Query model property types should match entity property types exactly:

```csharp
// Entity
public class Book
{
    public string Id { get; set; }      // Non-nullable
    public int Pages { get; set; }       // Non-nullable
}

// Query Model - types match entity
public class BookQueryModel : ISievePlusQueryModel
{
    public string Id { get; set; }      // Same type
    public int Pages { get; set; }       // Same type
}
```

**Why?** The query model defines what's *queryable*, not what's *required* in a request. Users don't need to provide all properties - Sieve filters are always optional.

### Custom Filters as Properties

Custom boolean filters are defined as nullable bool properties:

```csharp
public class BookQueryModel : ISievePlusQueryModel
{
    public bool? IsLongBook { get; set; }  // Custom filter
}

// Configuration
mapper.CustomFilter(q => q.IsLongBook, e => e.Pages > 500);

// Usage: ?Filters=IsLongBook==true
```

## Backward Compatibility

The old approach still works! You can use both simultaneously:

```csharp
// Old way (still works)
var books1 = processor.Apply(sieveModel, dbContext.Books);

// New way (with query model)
var books2 = processor.Apply<Book, BookQueryModel>(sieveModel, dbContext.Books);
```

## Migration Path

1. Keep using the old `MapProperties` fluent API for existing code
2. Create query models for new entities
3. Gradually migrate existing entities to query models as needed

## Example: Complex Query Model

```csharp
public class BookQueryModel : ISievePlusQueryModel
{
    // Basic properties
    public string Id { get; set; }
    public string Title { get; set; }
    public int Pages { get; set; }
    public DateTime Createdat { get; set; }

    // Navigation properties
    public string GenreName { get; set; }
    public string AuthorName { get; set; }

    // Date parts
    public int PublishedYear { get; set; }
    public int PublishedMonth { get; set; }

    // Calculated properties
    public int PageRangeStart { get; set; }  // Pages / 100 * 100

    // Custom filters
    public bool? IsLongBook { get; set; }     // Pages > 500
    public bool? IsRecent { get; set; }       // Published in last year
}
```

This query model makes it crystal clear what can be filtered/sorted when querying books!

## Future Enhancements

1. **Source Generator** - Auto-generate query models from entity + configuration
2. **TypeScript Generation** - Export query models as TypeScript types
3. **Query Builder** - Strongly-typed query builder using query models
4. **Validation** - Validate incoming filter/sort requests against query model

## Summary

Explicit Query Models provide a clean, type-safe way to define your queryable surface area. The query model IS the contract - what you see is exactly what you can query.

# ts-sieve-plus-query-builder

> **Type-safe query builder for Sieve.Plus APIs with powerful OR queries and parentheses grouping**

Build type-safe Sieve query strings for your TypeScript/JavaScript frontend with full IntelliSense support and powerful OR query capabilities including parentheses grouping.

[![npm version](https://img.shields.io/npm/v/ts-sieve-plus-query-builder)](https://www.npmjs.com/package/ts-sieve-plus-query-builder)

## Features

- ✅ **Type-safe** - Full TypeScript support with compile-time property name checking
- ✅ **Powerful OR queries** - Full support for `||` operator and parentheses grouping
- ✅ **Fluent API** - Chain methods for readable query building
- ✅ **Multiple output formats** - Query strings, SievePlusModel objects, or query parameter objects
- ✅ **All Sieve operators** - `==`, `!=`, `@=`, `_=`, `>`, `<`, `>=`, `<=`
- ✅ **Custom property support** - Handle mapped properties from your Sieve processor
- ✅ **Date handling** - Automatic ISO string conversion for Date objects
- ✅ **Round-trip parsing** - Parse query strings and SievePlusModels back to builders
- ✅ **Zero dependencies** - Lightweight and standalone

## Installation

```bash
npm install ts-sieve-plus-query-builder
```

## Quick Start

### Basic Usage

```typescript
import { SievePlusQueryBuilder } from 'ts-sieve-plus-query-builder';

// Define your query model interface (matches your C# query model)
interface ProductQueryModel {
  id: number;
  name: string;
  price: number;
  category: string;
  createdat: Date;
}

// Build a type-safe query
const queryString = SievePlusQueryBuilder.create<ProductQueryModel>()
  .filterContains('name', 'laptop')
  .filterGreaterThan('price', 500)
  .sortByDescending('price')
  .page(1)
  .pageSize(20)
  .buildQueryString();

// Use with fetch, axios, etc.
const response = await fetch(`/api/products?${queryString}`);
const products = await response.json();
```

### Integration with Generated TypeScript Client

When using NSwag to generate your TypeScript client from .NET's `SievePlusRequest<T>`, the query builder integrates seamlessly:

```typescript
import { SievePlusQueryBuilder } from 'ts-sieve-plus-query-builder';
import { ProductClient, ProductQueryModel } from './generated-client';

const client = new ProductClient();

// Create query using the query model type
const builder = SievePlusQueryBuilder.create<ProductQueryModel>()
  .filterContains('name', 'laptop')
  .filterGreaterThan('price', 500)
  .sortByDescending('price')
  .page(1)
  .pageSize(20);

// Build the model - fully type-safe with generated types
const model = builder.buildSievePlusModel();

// Use with generated client - TypeScript validates compatibility
const products = await client.getProducts(model);
```

## Powerful OR Queries

### Simple OR

```typescript
// Products where category is Electronics OR price is greater than $100
const query = SievePlusQueryBuilder.create<Product>()
  .filterEquals('category', 'Electronics')
  .or()
  .filterGreaterThan('price', 100)
  .buildFiltersString();

// Output: "category==Electronics || price>100"
```

### Parentheses Grouping

Use `beginGroup()` and `endGroup()` for explicit parentheses:

```typescript
// (Category is Electronics OR Computers) AND Price > $500
const query = SievePlusQueryBuilder.create<Product>()
  .beginGroup()
    .filterEquals('category', 'Electronics')
    .or()
    .filterEquals('category', 'Computers')
  .endGroup()
  .filterGreaterThan('price', 500)
  .buildFiltersString();

// Output: "(category==Electronics || category==Computers),price>500"
```

### Helper Method: filterWithAlternatives

Convenient method for filtering by multiple values on one property:

```typescript
interface Computer {
  processor: string;
  price: number;
  screenSize: number;
}

// Computers with processor Intel i9, AMD Ryzen 9, or Apple M2, and price > $1000
const query = SievePlusQueryBuilder.create<Computer>()
  .filterWithAlternatives(
    'processor',
    ['Intel i9', 'AMD Ryzen 9', 'Apple M2'],
    (b) => b.filterGreaterThan('price', 1000)
  )
  .buildFiltersString();

// Output: "(processor==Intel i9 || processor==AMD Ryzen 9 || processor==Apple M2),price>1000"
```

### Complex Nested Groups

```typescript
// ((title A OR title B) AND pages > 100) AND price < 50
const query = SievePlusQueryBuilder.create<Book>()
  .beginGroup()
    .beginGroup()
      .filterEquals('title', 'Book A')
      .or()
      .filterEquals('title', 'Book B')
    .endGroup()
    .filterGreaterThan('pages', 100)
  .endGroup()
  .filterLessThan('price', 50)
  .buildFiltersString();

// Output: "((title==Book A || title==Book B),pages>100),price<50"
```

## All Filter Operators

```typescript
const builder = SievePlusQueryBuilder.create<Product>()
  .filterEquals('id', 42)                          // ==
  .filterNotEquals('status', 'Deleted')            // !=
  .filterContains('description', 'awesome')        // @=
  .filterStartsWith('name', 'Pro')                 // _=
  .filterGreaterThan('price', 99.99)               // >
  .filterLessThan('stock', 10)                     // <
  .filterGreaterThanOrEqual('rating', 4.5)         // >=
  .filterLessThanOrEqual('weight', 5.0)            // <=
  .filterByName('CustomProperty', '==', 'value');  // Custom properties
```

## Replacing Filters (Preventing Duplicates)

By default, filter methods append to existing filters. Use the `replace` parameter to replace existing filters for the same property:

```typescript
const builder = SievePlusQueryBuilder.create<Product>();

// Without replace - appends filters (can create duplicates)
builder
  .filterContains('name', 'a')
  .filterContains('name', 'as')
  .filterContains('name', 'ass');
// Result: "name@=a,name@=as,name@=ass"

// With replace=true - replaces existing filters for that property
builder
  .filterContains('name', 'a', true)
  .filterContains('name', 'as', true)
  .filterContains('name', 'ass', true);
// Result: "name@=ass"

// Real-world example: Search input that updates on every keystroke
const handleSearchInput = (e: React.ChangeEvent<HTMLInputElement>) => {
  // Replace the name filter each time - prevents duplicates
  queryBuilder.filterContains('name', e.target.value, true);
  // Other filters are preserved
};

// Manual filter removal
builder.removeFilters('name');              // Remove all filters for 'name' property
builder.removeFiltersByName('CustomProp');  // For custom mapped properties
```

All filter methods support the `replace` parameter:
- `filterEquals(property, value, replace?)`
- `filterNotEquals(property, value, replace?)`
- `filterContains(property, value, replace?)`
- `filterStartsWith(property, value, replace?)`
- `filterGreaterThan(property, value, replace?)`
- `filterLessThan(property, value, replace?)`
- `filterGreaterThanOrEqual(property, value, replace?)`
- `filterLessThanOrEqual(property, value, replace?)`
- `filterByName(propertyName, operator, value, replace?)`

## Date Filtering

```typescript
const thirtyDaysAgo = new Date();
thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

const query = SievePlusQueryBuilder.create<Product>()
  .filterGreaterThanOrEqual('createdat', thirtyDaysAgo)
  .buildFiltersString();

// Output: "createdat>=2024-09-25T12:00:00.000Z" (automatically converted to ISO string)
```

## Custom Mapped Properties

For properties mapped in your C# `ApplicationSieveProcessor`:

```typescript
// C# Sieve Processor maps a.Books.Count to "BooksCount"
const query = SievePlusQueryBuilder.create<Author>()
  .filterByName('BooksCount', '>=', 5)
  .sortByName('BooksCount', true) // descending
  .buildSievePlusModel();

// Result: { filters: "BooksCount>=5", sorts: "-BooksCount" }
```

**Note**: `filterByName` and `sortByName` intentionally bypass type-safety for flexibility with custom mapped properties.

## Sorting

```typescript
// Ascending sort
builder.sortBy('name');
// Result: "name"

// Descending sort
builder.sortByDescending('createdat');
// Result: "-createdat"

// Multiple sorts
builder
  .sortByDescending('createdat')
  .sortBy('name');
// Result: "-createdat,name"
```

## Pagination

```typescript
const query = SievePlusQueryBuilder.create<Product>()
  .page(2)
  .pageSize(25)
  .buildSievePlusModel();

// Result: { filters: "", sorts: "", page: 2, pageSize: 25 }
```

## Round-Trip Parsing

### Parse Query Strings

```typescript
// Parse from a complete query string
const queryString = 'filters=name@=Bob&sorts=-createdat&page=2&pageSize=20';
const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);

// Works with leading '?' too
const urlSearch = '?filters=name@=Bob&page=1';
const builder2 = SievePlusQueryBuilder.parseQueryString<Author>(urlSearch);

// Continue building on top of the parsed query
builder
  .filterEquals('status', 'active')
  .sortBy('name');

const model = builder.buildSievePlusModel();
```

### Parse from SievePlusModel

Useful with React Router or URL search params:

```typescript
import { useSearchParams } from 'react-router-dom';

// In your React component
const [searchParams] = useSearchParams();
const filters = searchParams.get('filters') ?? "";
const sorts = searchParams.get('sorts') ?? "";
const pageSize = Number.parseInt(searchParams.get('pageSize') ?? "10");
const page = Number.parseInt(searchParams.get('page') ?? "1");

const queryBuilder = SievePlusQueryBuilder.fromSievePlusModel<Product>({
  pageSize: pageSize,
  page: page,
  sorts: sorts,
  filters: filters
});

// Continue building on top of the parsed query
queryBuilder
  .filterEquals('status', 'active')
  .sortByDescending('createdat');

// Or just use it as-is
const model = queryBuilder.buildSievePlusModel();
```

### Round-Trip Support

Both parsing methods support round-trip conversion:

```typescript
// Build a query
const original = SievePlusQueryBuilder.create<Product>()
  .filterContains('name', 'laptop')
  .sortBy('name')
  .page(1)
  .pageSize(10);

// Round-trip via query string
const queryString = original.buildQueryString();
const fromQuery = SievePlusQueryBuilder.parseQueryString<Product>(queryString);
const rebuilt1 = fromQuery.buildQueryString();
// rebuilt1 === queryString ✅

// Round-trip via SievePlusModel
const model = original.buildSievePlusModel();
const fromModel = SievePlusQueryBuilder.fromSievePlusModel<Product>(model);
const rebuilt2 = fromModel.buildSievePlusModel();
// rebuilt2 equals model ✅

// Add more filters/sorts after parsing
const modified = SievePlusQueryBuilder.parseQueryString<Product>(queryString)
  .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
  .page(2);
// Result includes both original and new filters/sorts
```

## Generic Type Support

The query builder uses TypeScript generics to maintain type safety with your query models:

```typescript
// The builder is generic over your query model type
const builder = SievePlusQueryBuilder.create<ProductQueryModel>();

// The output model is also generic
const model: SievePlusModel<ProductQueryModel> = builder.buildSievePlusModel();
```

The `SievePlusModel<T>` interface includes:
- `filters: string` - Filter query string
- `sorts: string` - Sort query string
- `page: number` - Page number
- `pageSize: number` - Page size
- `queryModel: T | undefined` - Query model type (for type safety only, always `undefined` at runtime)

This matches the generated TypeScript interface from .NET's `SievePlusRequest<TQueryModel>`, ensuring seamless integration.

## Output Formats

### 1. SievePlusModel Object

```typescript
const model = builder.buildSievePlusModel();
// {
//   filters: "name@=laptop",
//   sorts: "-price",
//   page: 1,
//   pageSize: 10,
//   queryModel: undefined  // Type marker only
// }
```

### 2. Query String

```typescript
const queryString = builder.buildQueryString();
// "filters=name%40%3Dlaptop&sorts=-price&page=1&pageSize=10"
```

### 3. Query Parameters Object

```typescript
const params = builder.buildQueryParams();
// { filters: "name@=laptop", sorts: "-price", page: 1, pageSize: 10 }

// Use with URLSearchParams
const url = `/api/products?${new URLSearchParams(params)}`;

// Or with fetch
const response = await fetch('/api/products?' + new URLSearchParams(params));
```

### 4. Individual Components

```typescript
const filtersString = builder.buildFiltersString();
// "name@=laptop,price>500"

const sortsString = builder.buildSortsString();
// "-createdat,name"
```

## React Integration Examples

### Simple Search and Filter

```typescript
import { useState, useEffect } from 'react';
import { SievePlusQueryBuilder } from 'ts-sieve-plus-query-builder';

interface Product {
  id: number;
  name: string;
  price: number;
  category: string;
}

function ProductSearch() {
  const [products, setProducts] = useState<Product[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [minPrice, setMinPrice] = useState<number>();
  const [page, setPage] = useState(1);

  useEffect(() => {
    const fetchProducts = async () => {
      const builder = SievePlusQueryBuilder.create<Product>()
        .page(page)
        .pageSize(20)
        .sortByDescending('createdat');

      if (searchTerm) {
        builder.filterContains('name', searchTerm);
      }

      if (minPrice) {
        if (searchTerm) builder.or();
        builder.filterGreaterThan('price', minPrice);
      }

      const queryString = builder.buildQueryString();
      const response = await fetch(`/api/products?${queryString}`);
      const data = await response.json();
      setProducts(data);
    };

    fetchProducts();
  }, [searchTerm, minPrice, page]);

  return (
    <div>
      <input
        type="text"
        placeholder="Search products..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
      />
      <input
        type="number"
        placeholder="Min price"
        value={minPrice ?? ''}
        onChange={(e) => setMinPrice(Number(e.target.value) || undefined)}
      />
      {/* Render products */}
    </div>
  );
}
```

### Advanced: URL-Synced Filters with React Router

```typescript
import { useSearchParams } from 'react-router-dom';
import { SievePlusQueryBuilder } from 'ts-sieve-plus-query-builder';

function ProductList() {
  const [searchParams, setSearchParams] = useSearchParams();

  // Parse current query from URL
  const builder = SievePlusQueryBuilder.parseQueryString<Product>(
    searchParams.toString()
  );

  const handleFilterChange = (category: string) => {
    // Update builder with new filter (replace existing)
    builder.filterEquals('category', category, true);

    // Update URL
    const newParams = new URLSearchParams(builder.buildQueryString());
    setSearchParams(newParams);
  };

  const handleSearch = (term: string) => {
    builder.filterContains('name', term, true);
    setSearchParams(new URLSearchParams(builder.buildQueryString()));
  };

  // Fetch products using current query
  useEffect(() => {
    const fetchProducts = async () => {
      const response = await fetch(`/api/products?${searchParams.toString()}`);
      const data = await response.json();
      setProducts(data);
    };

    fetchProducts();
  }, [searchParams]);

  // ...
}
```

### Real-World Example: E-Commerce Filtering

```typescript
import { SievePlusQueryBuilder } from 'ts-sieve-plus-query-builder';

interface Computer {
  processor: string;
  price: number;
  screenSize: number;
  ram: number;
  inStock: boolean;
}

function ComputerFilter() {
  const [filters, setFilters] = useState({
    processors: ['Intel i9', 'AMD Ryzen 9'] as string[],
    minPrice: 1000,
    maxPrice: 2000,
    minScreen: 14,
    maxScreen: 16,
    minRam: 16,
  });

  const buildQuery = () => {
    const builder = SievePlusQueryBuilder.create<Computer>();

    // Processor alternatives with shared constraints
    builder.filterWithAlternatives(
      'processor',
      filters.processors,
      (b) => b
        .filterGreaterThanOrEqual('price', filters.minPrice)
        .filterLessThanOrEqual('price', filters.maxPrice)
        .filterGreaterThanOrEqual('screenSize', filters.minScreen)
        .filterLessThanOrEqual('screenSize', filters.maxScreen)
        .filterGreaterThanOrEqual('ram', filters.minRam)
        .filterEquals('inStock', true)
    );

    return builder.buildQueryString();
  };

  const fetchComputers = async () => {
    const queryString = buildQuery();
    const response = await fetch(`/api/computers?${queryString}`);
    const data = await response.json();
    return data;
  };

  // ...
}
```

## Inspection API

Examine filters and sorts programmatically:

```typescript
const builder = SievePlusQueryBuilder.create<Product>()
  .filterEquals('category', 'Books')
  .filterGreaterThan('price', 20)
  .or()
  .filterEquals('onSale', true);

// Get filter groups (respects OR logic)
const groups = builder.getFilterGroups();
// Returns: [[{propertyName: 'category', operator: '==', value: 'Books'}, ...], [...]]

// Get all filters (flattened)
const allFilters = builder.getFilters();
allFilters.forEach(filter => {
  console.log(`${filter.propertyName} ${filter.operator} ${filter.value}`);
});

// Get sorts
const sorts = builder.getSorts();
sorts.forEach(sort => {
  console.log(`${sort.propertyName} (${sort.isDescending ? 'DESC' : 'ASC'})`);
});

// Check for specific filters
const hasNameFilter = builder.hasFilter('name');
const hasCreatedAtSort = builder.hasSort('createdat');

// Get pagination
const page = builder.getPage();        // number | undefined
const pageSize = builder.getPageSize(); // number | undefined
```

## Error Handling

Mismatched `beginGroup()` and `endGroup()` calls throw errors:

```typescript
// ❌ Throws Error: "Unmatched beginGroup() call - missing endGroup()"
const query = SievePlusQueryBuilder.create<Product>()
  .beginGroup()
    .filterEquals('name', 'Test')
  .buildFiltersString();

// ❌ Throws Error: "endGroup() called without matching beginGroup()"
const query2 = SievePlusQueryBuilder.create<Product>()
  .filterEquals('name', 'Test')
  .endGroup()
  .buildFiltersString();
```

## Best Practices

### 1. Define Type Interfaces

```typescript
// Define interfaces that match your C# models
// Consider generating these from your OpenAPI/Swagger spec with NSwag

interface ProductDto {
  id: number;
  name: string;
  price: number;
  category: string;
  createdat: Date;
}

// Use generated types for type safety
const query = SievePlusQueryBuilder.create<ProductDto>()
  .filterContains('name', 'laptop')  // IntelliSense works!
  .buildSievePlusModel();
```

### 2. Encapsulate Complex Queries

```typescript
class ProductQueries {
  static popularProducts() {
    return SievePlusQueryBuilder.create<Product>()
      .filterGreaterThan('rating', 4.0)
      .filterGreaterThan('sales', 1000)
      .sortByDescending('sales');
  }

  static inPriceRange(min: number, max: number) {
    return SievePlusQueryBuilder.create<Product>()
      .filterGreaterThanOrEqual('price', min)
      .filterLessThanOrEqual('price', max);
  }
}

// Usage
const popular = ProductQueries.popularProducts()
  .page(1)
  .pageSize(20)
  .buildQueryString();
```

### 3. When Using OR, Repeat Shared Constraints

```typescript
// ❌ WRONG - Constraints only apply to second group
const wrong = SievePlusQueryBuilder.create<Product>()
  .filterEquals('category', 'Electronics')
  .or()
  .filterEquals('category', 'Computers')
  .filterGreaterThan('price', 500)  // Only applies to Computers!
  .buildFiltersString();
// Output: "category==Electronics || category==Computers,price>500"

// ✅ CORRECT - Use parentheses and apply constraints after group
const correct = SievePlusQueryBuilder.create<Product>()
  .beginGroup()
    .filterEquals('category', 'Electronics')
    .or()
    .filterEquals('category', 'Computers')
  .endGroup()
  .filterGreaterThan('price', 500)  // Applies to both!
  .buildFiltersString();
// Output: "(category==Electronics || category==Computers),price>500"
```

## TypeScript Type Safety

The query builder provides full TypeScript support:

```typescript
interface Product {
  id: number;
  name: string;
  price: number;
}

const builder = SievePlusQueryBuilder.create<Product>();

// ✅ These work - properties exist
builder.filterEquals('name', 'Test');
builder.sortBy('price');

// ❌ TypeScript error - property doesn't exist
builder.filterEquals('invalidProperty', 'value');
//                    ^^^^^^^^^^^^^^^^^
// Argument of type '"invalidProperty"' is not assignable to parameter of type 'keyof Product'
```

## Browser Compatibility

Works in all modern browsers and Node.js environments that support ES2020.

## Testing

The package includes comprehensive tests (1,252 lines of test coverage). Run them with:

```bash
npm test
```

## Related Packages

| Package | Description |
|---------|-------------|
| [Sieve.Plus](https://www.nuget.org/packages/Sieve.Plus) | .NET Core filtering/sorting/pagination library |
| [Sieve.Plus.QueryBuilder](https://www.nuget.org/packages/Sieve.Plus.QueryBuilder) | .NET equivalent query builder |

## License

MIT License - See [LICENSE](LICENSE) for details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

**Repository**: [https://github.com/uldahlalex/sieveplus](https://github.com/uldahlalex/sieveplus)

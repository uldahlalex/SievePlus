import { describe, it, expect } from 'vitest';
import { SieveQueryBuilder } from './SieveQueryBuilder';

// Sample interfaces matching your C# entities
interface Author {
  id: string;
  name: string;
  createdat: Date;
  books: Book[];
}

interface Book {
  id: string;
  title: string;
  pages: number;
  createdat: Date;
  authors: Author[];
}

describe('SieveQueryBuilder', () => {
  describe('Filter operations', () => {
    it('should build equals filter', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterEquals('name', 'Bob_5')
        .buildFiltersString();

      expect(query).toBe('name==Bob_5');
    });

    it('should build contains filter', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterContains('name', '_5')
        .buildFiltersString();

      expect(query).toBe('name@=_5');
    });

    it('should build not equals filter', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterNotEquals('name', 'Bob_0')
        .buildFiltersString();

      expect(query).toBe('name!=Bob_0');
    });

    it('should build starts with filter', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterStartsWith('name', 'Bob')
        .buildFiltersString();

      expect(query).toBe('name_=Bob');
    });

    it('should build greater than filter', () => {
      const query = SieveQueryBuilder.create<Book>()
        .filterGreaterThan('pages', 200)
        .buildFiltersString();

      expect(query).toBe('pages>200');
    });

    it('should build less than filter', () => {
      const query = SieveQueryBuilder.create<Book>()
        .filterLessThan('pages', 500)
        .buildFiltersString();

      expect(query).toBe('pages<500');
    });

    it('should build greater than or equal filter', () => {
      const query = SieveQueryBuilder.create<Book>()
        .filterGreaterThanOrEqual('pages', 200)
        .buildFiltersString();

      expect(query).toBe('pages>=200');
    });

    it('should build less than or equal filter', () => {
      const query = SieveQueryBuilder.create<Book>()
        .filterLessThanOrEqual('pages', 500)
        .buildFiltersString();

      expect(query).toBe('pages<=500');
    });

    it('should combine multiple filters', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('name', 'Bob_0')
        .buildFiltersString();

      expect(query).toBe('name@=Bob,name!=Bob_0');
    });

    it('should build custom property filter', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .buildFiltersString();

      expect(query).toBe('BooksCount>=5');
    });

    it('should handle Date filters', () => {
      const date = new Date('2024-01-01T00:00:00Z');
      const query = SieveQueryBuilder.create<Author>()
        .filterGreaterThanOrEqual('createdat', date)
        .buildFiltersString();

      expect(query).toBe('createdat>=2024-01-01T00:00:00.000Z');
    });
  });

  describe('Sort operations', () => {
    it('should build ascending sort', () => {
      const query = SieveQueryBuilder.create<Author>()
        .sortBy('name')
        .buildSortsString();

      expect(query).toBe('name');
    });

    it('should build descending sort', () => {
      const query = SieveQueryBuilder.create<Author>()
        .sortByDescending('name')
        .buildSortsString();

      expect(query).toBe('-name');
    });

    it('should combine multiple sorts', () => {
      const query = SieveQueryBuilder.create<Author>()
        .sortByDescending('createdat')
        .sortBy('name')
        .buildSortsString();

      expect(query).toBe('-createdat,name');
    });

    it('should build custom property sort', () => {
      const query = SieveQueryBuilder.create<Author>()
        .sortByName('BooksCount', true)
        .buildSortsString();

      expect(query).toBe('-BooksCount');
    });
  });

  describe('Pagination', () => {
    it('should set page number', () => {
      const model = SieveQueryBuilder.create<Author>()
        .page(2)
        .buildSieveModel();

      expect(model.page).toBe(2);
    });

    it('should set page size', () => {
      const model = SieveQueryBuilder.create<Author>()
        .pageSize(10)
        .buildSieveModel();

      expect(model.pageSize).toBe(10);
    });
  });

  describe('SieveModel building', () => {
    it('should build complete SieveModel', () => {
      const model = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .sortBy('name')
        .page(2)
        .pageSize(10)
        .buildSieveModel();

      expect(model).toEqual({
        filters: 'name@=Bob',
        sorts: 'name',
        page: 2,
        pageSize: 10,
      });
    });

    it('should only include non-empty values in SieveModel', () => {
      const model = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .buildSieveModel();

      expect(model).toEqual({
        filters: 'name@=Bob',
        sorts: '',
        page: 1,
        pageSize: 10,
      });
    });
  });

  describe('Query string building', () => {
    it('should build complete query string', () => {
      const queryString = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('name', 'Bob_0')
        .sortBy('name')
        .page(2)
        .pageSize(10)
        .buildQueryString();

      expect(queryString).toBe(
        'filters=name%40%3DBob%2Cname!%3DBob_0&sorts=name&page=2&pageSize=10'
      );
    });

    it('should handle empty query string', () => {
      const queryString = SieveQueryBuilder.create<Author>().buildQueryString();

      expect(queryString).toBe('');
    });
  });

  describe('Query params building', () => {
    it('should build query params object', () => {
      const params = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .sortBy('name')
        .page(1)
        .pageSize(20)
        .buildQueryParams();

      expect(params).toEqual({
        filters: 'name@=Bob',
        sorts: 'name',
        page: 1,
        pageSize: 20,
      });
    });

    it('should return empty object when no params', () => {
      const params = SieveQueryBuilder.create<Author>().buildQueryParams();

      expect(params).toEqual({});
    });
  });

  describe('Fluent API chaining', () => {
    it('should support method chaining', () => {
      const query = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .filterByName('BooksCount', '>=', 3)
        .sortByDescending('createdat')
        .sortBy('name')
        .page(1)
        .pageSize(20);

      const model = query.buildSieveModel();

      expect(model.filters).toContain('name@=Bob');
      expect(model.filters).toContain('createdat>=');
      expect(model.filters).toContain('BooksCount>=3');
      expect(model.sorts).toBe('-createdat,name');
      expect(model.page).toBe(1);
      expect(model.pageSize).toBe(20);
    });
  });

  describe('Type safety', () => {
    it('should only allow valid property names', () => {
      const builder = SieveQueryBuilder.create<Author>();

      // These should compile without errors
      builder.filterEquals('name', 'test');
      builder.filterEquals('id', 'test');
      builder.sortBy('createdat');

      // @ts-expect-error - 'invalidProperty' does not exist on Author
      builder.filterEquals('invalidProperty', 'test');
    });
  });

  describe('Real-world usage examples', () => {
    it('should build query for filtering authors by name contains', () => {
      const queryString = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob_0')
        .pageSize(10)
        .buildQueryString();

      expect(queryString).toBe('filters=name%40%3DBob_0&pageSize=10');
    });

    it('should build query for complex search scenario', () => {
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

      const model = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterGreaterThanOrEqual('createdat', thirtyDaysAgo)
        .filterByName('BooksCount', '>=', 3)
        .sortByDescending('createdat')
        .sortBy('name')
        .page(1)
        .pageSize(20)
        .buildSieveModel();

      expect(model.filters).toBeDefined();
      expect(model.filters).toContain('name@=Bob');
      expect(model.filters).toContain('BooksCount>=3');
      expect(model.sorts).toBe('-createdat,name');
      expect(model.page).toBe(1);
      expect(model.pageSize).toBe(20);
    });

    it('should build query for books with page count filtering', () => {
      const params = SieveQueryBuilder.create<Book>()
        .filterGreaterThan('pages', 200)
        .filterLessThan('pages', 500)
        .sortBy('title')
        .buildQueryParams();

      expect(params.filters).toBe('pages>200,pages<500');
      expect(params.sorts).toBe('title');
    });
  });

  describe('fromSieveModel', () => {
    it('should parse filters correctly', () => {
      const model = {
        filters: 'name@=Bob,id==123',
        sorts: '',
        page: 1,
        pageSize: 10
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob,id==123');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should parse sorts correctly', () => {
      const model = {
        filters: '',
        sorts: '-createdat,name',
        page: 1,
        pageSize: 20
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(20);
    });

    it('should parse pagination correctly', () => {
      const model = {
        filters: '',
        sorts: '',
        page: 5,
        pageSize: 25
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.page).toBe(5);
      expect(result.pageSize).toBe(25);
      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
    });

    it('should handle empty model', () => {
      const model = {
        filters: '',
        sorts: '',
        page: 1,
        pageSize: 10
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should allow chaining after parsing', () => {
      const model = {
        filters: 'name@=Bob',
        sorts: 'name',
        page: 1,
        pageSize: 10
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model)
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .sortByDescending('createdat')
        .page(2);

      const result = builder.buildSieveModel();

      expect(result.filters).toContain('name@=Bob');
      expect(result.filters).toContain('createdat>=');
      expect(result.sorts).toContain('name');
      expect(result.sorts).toContain('-createdat');
      expect(result.page).toBe(2);
      expect(result.pageSize).toBe(10);
    });

    it('should handle URL search params use case', () => {
      // Simulating: const [searchParams] = useSearchParams()
      const filters = 'name@=Bob,id!=123';
      const sorts = '-createdat,name';
      const pageSize = 3;
      const page = 1;

      const builder = SieveQueryBuilder.fromSieveModel<Author>({
        pageSize: pageSize,
        page: page,
        sorts: sorts,
        filters: filters
      });

      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob,id!=123');
      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(3);
    });

    it('should handle empty strings', () => {
      const model = {
        filters: '',
        sorts: '',
        page: 1,
        pageSize: 10
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should handle undefined values in model', () => {
      const model = {
        filters: 'name@=Bob',
        sorts: '',
        page: 1,
        pageSize: 10
      };

      const builder = SieveQueryBuilder.fromSieveModel<Author>(model);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should support round-trip parsing', () => {
      const original = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('id', '123')
        .sortByDescending('createdat')
        .sortBy('name')
        .page(2)
        .pageSize(15);

      const model = original.buildSieveModel();
      const parsed = SieveQueryBuilder.fromSieveModel<Author>(model);
      const rebuilt = parsed.buildSieveModel();

      expect(rebuilt).toEqual(model);
    });
  });

  describe('Remove filters', () => {
    it('should remove filters for a specific property', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('name', 'Alice')
        .filterEquals('id', '123');

      builder.removeFilters('name');
      const result = builder.buildFiltersString();

      expect(result).toBe('id==123');
    });

    it('should remove filters by name for mapped properties', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .filterByName('BooksCount', '<=', 10)
        .filterEquals('name', 'Bob');

      builder.removeFiltersByName('BooksCount');
      const result = builder.buildFiltersString();

      expect(result).toBe('name==Bob');
    });

    it('should handle removing non-existent filters', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterEquals('name', 'Bob');

      builder.removeFilters('id');
      const result = builder.buildFiltersString();

      expect(result).toBe('name==Bob');
    });
  });

  describe('Replace filter functionality', () => {
    it('should replace filter when replace=true on filterContains', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterContains('name', 'Alice', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=Alice');
    });

    it('should append filter when replace=false (default)', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterContains('name', 'Alice');

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=Bob,name@=Alice');
    });

    it('should replace multiple existing filters for same property', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('name', 'Alice')
        .filterStartsWith('name', 'Charlie')
        .filterContains('name', 'David', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=David');
    });

    it('should only replace filters for the specified property', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('id', '123')
        .filterContains('name', 'Alice', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('id==123,name@=Alice');
    });

    it('should work with filterEquals replace', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterEquals('id', '123')
        .filterEquals('id', '456', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('id==456');
    });

    it('should work with filterByName replace', () => {
      const builder = SieveQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .filterByName('BooksCount', '<=', 10)
        .filterByName('BooksCount', '==', 7, true);

      const result = builder.buildFiltersString();
      expect(result).toBe('BooksCount==7');
    });

    it('should handle real-world scenario with input changes', () => {
      // Simulating multiple button clicks that should replace the filter
      const builder = SieveQueryBuilder.create<Book>();

      // User types "a"
      builder.filterContains('title', 'a', true);
      expect(builder.buildFiltersString()).toBe('title@=a');

      // User types "as"
      builder.filterContains('title', 'as', true);
      expect(builder.buildFiltersString()).toBe('title@=as');

      // User types "ass"
      builder.filterContains('title', 'ass', true);
      expect(builder.buildFiltersString()).toBe('title@=ass');

      // User deletes and types "book"
      builder.filterContains('title', 'book', true);
      expect(builder.buildFiltersString()).toBe('title@=book');
    });

    it('should preserve other filters when replacing', () => {
      const builder = SieveQueryBuilder.create<Book>()
        .filterGreaterThan('pages', 100)
        .filterContains('title', 'a')
        .filterContains('title', 'book', true)
        .filterLessThan('pages', 500);

      const result = builder.buildFiltersString();
      expect(result).toContain('pages>100');
      expect(result).toContain('pages<500');
      expect(result).toContain('title@=book');
      expect(result).not.toContain('title@=a');
    });
  });

  describe('parseQueryString', () => {
    it('should parse filters from query string', () => {
      const queryString = 'filters=name@=Bob,id==123&page=1&pageSize=10';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob,id==123');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should parse sorts from query string', () => {
      const queryString = 'sorts=-createdat,name&page=1&pageSize=20';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(20);
    });

    it('should parse pagination from query string', () => {
      const queryString = 'page=5&pageSize=25';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.page).toBe(5);
      expect(result.pageSize).toBe(25);
      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
    });

    it('should handle URL-encoded query string', () => {
      const queryString = 'filters=name%40%3DBob%2Cid%21%3D123&sorts=-createdat';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob,id!=123');
      expect(result.sorts).toBe('-createdat');
    });

    it('should handle query string with leading question mark', () => {
      const queryString = '?filters=name@=Bob&page=1';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.page).toBe(1);
    });

    it('should handle empty query string', () => {
      const builder = SieveQueryBuilder.parseQueryString<Author>('');
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should allow chaining after parsing query string', () => {
      const queryString = 'filters=name@=Bob&sorts=name&page=1&pageSize=10';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString)
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .sortByDescending('createdat')
        .page(2);

      const result = builder.buildSieveModel();

      expect(result.filters).toContain('name@=Bob');
      expect(result.filters).toContain('createdat>=');
      expect(result.sorts).toContain('name');
      expect(result.sorts).toContain('-createdat');
      expect(result.page).toBe(2);
      expect(result.pageSize).toBe(10);
    });

    it('should support round-trip with query string', () => {
      const original = SieveQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('id', '123')
        .sortByDescending('createdat')
        .sortBy('name')
        .page(2)
        .pageSize(15);

      const queryString = original.buildQueryString();
      const parsed = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const rebuilt = parsed.buildQueryString();

      expect(rebuilt).toBe(queryString);
    });

    it('should handle complex real-world query string', () => {
      const queryString = 'filters=name@=Bob,createdat>=2024-01-01T00:00:00.000Z,BooksCount>=3&sorts=-createdat,name&page=2&pageSize=20';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.filters).toContain('name@=Bob');
      expect(result.filters).toContain('createdat>=2024-01-01T00:00:00.000Z');
      expect(result.filters).toContain('BooksCount>=3');
      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(2);
      expect(result.pageSize).toBe(20);
    });

    it('should handle case-insensitive parameter names', () => {
      const queryString = 'Filters=name@=Bob&Sorts=name&Page=1&PageSize=10';

      const builder = SieveQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSieveModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.sorts).toBe('name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });
  });
});

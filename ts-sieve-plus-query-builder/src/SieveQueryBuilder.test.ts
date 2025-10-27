import { describe, it, expect } from 'vitest';
import { SievePlusQueryBuilder } from './SieveQueryBuilder';

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

describe('SievePlusQueryBuilder', () => {
  describe('Filter operations', () => {
    it('should build equals filter', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterEquals('name', 'Bob_5')
        .buildFiltersString();

      expect(query).toBe('name==Bob_5');
    });

    it('should build contains filter', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', '_5')
        .buildFiltersString();

      expect(query).toBe('name@=_5');
    });

    it('should build not equals filter', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterNotEquals('name', 'Bob_0')
        .buildFiltersString();

      expect(query).toBe('name!=Bob_0');
    });

    it('should build starts with filter', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterStartsWith('name', 'Bob')
        .buildFiltersString();

      expect(query).toBe('name_=Bob');
    });

    it('should build greater than filter', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterGreaterThan('pages', 200)
        .buildFiltersString();

      expect(query).toBe('pages>200');
    });

    it('should build less than filter', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterLessThan('pages', 500)
        .buildFiltersString();

      expect(query).toBe('pages<500');
    });

    it('should build greater than or equal filter', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterGreaterThanOrEqual('pages', 200)
        .buildFiltersString();

      expect(query).toBe('pages>=200');
    });

    it('should build less than or equal filter', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterLessThanOrEqual('pages', 500)
        .buildFiltersString();

      expect(query).toBe('pages<=500');
    });

    it('should combine multiple filters', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('name', 'Bob_0')
        .buildFiltersString();

      expect(query).toBe('name@=Bob,name!=Bob_0');
    });

    it('should build custom property filter', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .buildFiltersString();

      expect(query).toBe('BooksCount>=5');
    });

    it('should handle Date filters', () => {
      const date = new Date('2024-01-01T00:00:00Z');
      const query = SievePlusQueryBuilder.create<Author>()
        .filterGreaterThanOrEqual('createdat', date)
        .buildFiltersString();

      expect(query).toBe('createdat>=2024-01-01T00:00:00.000Z');
    });
  });

  describe('Sort operations', () => {
    it('should build ascending sort', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .sortBy('name')
        .buildSortsString();

      expect(query).toBe('name');
    });

    it('should build descending sort', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .sortByDescending('name')
        .buildSortsString();

      expect(query).toBe('-name');
    });

    it('should combine multiple sorts', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .sortByDescending('createdat')
        .sortBy('name')
        .buildSortsString();

      expect(query).toBe('-createdat,name');
    });

    it('should build custom property sort', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .sortByName('BooksCount', true)
        .buildSortsString();

      expect(query).toBe('-BooksCount');
    });
  });

  describe('Pagination', () => {
    it('should set page number', () => {
      const model = SievePlusQueryBuilder.create<Author>()
        .page(2)
        .buildSievePlusModel();

      expect(model.page).toBe(2);
    });

    it('should set page size', () => {
      const model = SievePlusQueryBuilder.create<Author>()
        .pageSize(10)
        .buildSievePlusModel();

      expect(model.pageSize).toBe(10);
    });
  });

  describe('SieveModel building', () => {
    it('should build complete SieveModel', () => {
      const model = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .sortBy('name')
        .page(2)
        .pageSize(10)
        .buildSievePlusModel();

      expect(model).toEqual({
        filters: 'name@=Bob',
        sorts: 'name',
        page: 2,
        pageSize: 10,
      });
    });

    it('should only include non-empty values in SieveModel', () => {
      const model = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .buildSievePlusModel();

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
      const queryString = SievePlusQueryBuilder.create<Author>()
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
      const queryString = SievePlusQueryBuilder.create<Author>().buildQueryString();

      expect(queryString).toBe('');
    });
  });

  describe('Query params building', () => {
    it('should build query params object', () => {
      const params = SievePlusQueryBuilder.create<Author>()
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
      const params = SievePlusQueryBuilder.create<Author>().buildQueryParams();

      expect(params).toEqual({});
    });
  });

  describe('Fluent API chaining', () => {
    it('should support method chaining', () => {
      const query = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .filterByName('BooksCount', '>=', 3)
        .sortByDescending('createdat')
        .sortBy('name')
        .page(1)
        .pageSize(20);

      const model = query.buildSievePlusModel();

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
      const builder = SievePlusQueryBuilder.create<Author>();

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
      const queryString = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob_0')
        .pageSize(10)
        .buildQueryString();

      expect(queryString).toBe('filters=name%40%3DBob_0&pageSize=10');
    });

    it('should build query for complex search scenario', () => {
      const thirtyDaysAgo = new Date();
      thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);

      const model = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterGreaterThanOrEqual('createdat', thirtyDaysAgo)
        .filterByName('BooksCount', '>=', 3)
        .sortByDescending('createdat')
        .sortBy('name')
        .page(1)
        .pageSize(20)
        .buildSievePlusModel();

      expect(model.filters).toBeDefined();
      expect(model.filters).toContain('name@=Bob');
      expect(model.filters).toContain('BooksCount>=3');
      expect(model.sorts).toBe('-createdat,name');
      expect(model.page).toBe(1);
      expect(model.pageSize).toBe(20);
    });

    it('should build query for books with page count filtering', () => {
      const params = SievePlusQueryBuilder.create<Book>()
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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model)
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .sortByDescending('createdat')
        .page(2);

      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>({
        pageSize: pageSize,
        page: page,
        sorts: sorts,
        filters: filters
      });

      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

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

      const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should support round-trip parsing', () => {
      const original = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('id', '123')
        .sortByDescending('createdat')
        .sortBy('name')
        .page(2)
        .pageSize(15);

      const model = original.buildSievePlusModel();
      const parsed = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);
      const rebuilt = parsed.buildSievePlusModel();

      expect(rebuilt).toEqual(model);
    });
  });

  describe('Remove filters', () => {
    it('should remove filters for a specific property', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('name', 'Alice')
        .filterEquals('id', '123');

      builder.removeFilters('name');
      const result = builder.buildFiltersString();

      expect(result).toBe('id==123');
    });

    it('should remove filters by name for mapped properties', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .filterByName('BooksCount', '<=', 10)
        .filterEquals('name', 'Bob');

      builder.removeFiltersByName('BooksCount');
      const result = builder.buildFiltersString();

      expect(result).toBe('name==Bob');
    });

    it('should handle removing non-existent filters', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterEquals('name', 'Bob');

      builder.removeFilters('id');
      const result = builder.buildFiltersString();

      expect(result).toBe('name==Bob');
    });
  });

  describe('Replace filter functionality', () => {
    it('should replace filter when replace=true on filterContains', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterContains('name', 'Alice', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=Alice');
    });

    it('should append filter when replace=false (default)', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterContains('name', 'Alice');

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=Bob,name@=Alice');
    });

    it('should replace multiple existing filters for same property', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('name', 'Alice')
        .filterStartsWith('name', 'Charlie')
        .filterContains('name', 'David', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('name@=David');
    });

    it('should only replace filters for the specified property', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterEquals('id', '123')
        .filterContains('name', 'Alice', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('id==123,name@=Alice');
    });

    it('should work with filterEquals replace', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterEquals('id', '123')
        .filterEquals('id', '456', true);

      const result = builder.buildFiltersString();
      expect(result).toBe('id==456');
    });

    it('should work with filterByName replace', () => {
      const builder = SievePlusQueryBuilder.create<Author>()
        .filterByName('BooksCount', '>=', 5)
        .filterByName('BooksCount', '<=', 10)
        .filterByName('BooksCount', '==', 7, true);

      const result = builder.buildFiltersString();
      expect(result).toBe('BooksCount==7');
    });

    it('should handle real-world scenario with input changes', () => {
      // Simulating multiple button clicks that should replace the filter
      const builder = SievePlusQueryBuilder.create<Book>();

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
      const builder = SievePlusQueryBuilder.create<Book>()
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

  describe('Parentheses grouping', () => {
    it('should generate parentheses with beginGroup/endGroup', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .beginGroup()
          .filterEquals('title', 'Book A')
          .or()
          .filterEquals('title', 'Book B')
        .endGroup()
        .filterGreaterThan('pages', 100)
        .buildFiltersString();

      expect(query).toBe('(title==Book A || title==Book B),pages>100');
    });

    it('should generate parentheses with filterWithAlternatives', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterWithAlternatives(
          'title',
          ['Book A', 'Book B', 'Book C'],
          (b) => b.filterGreaterThan('pages', 200)
        )
        .buildFiltersString();

      expect(query).toBe('(title==Book A || title==Book B || title==Book C),pages>200');
    });

    it('should handle filterWithAlternatives without shared constraints', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterWithAlternatives('title', ['Book A', 'Book B'])
        .buildFiltersString();

      expect(query).toBe('(title==Book A || title==Book B)');
    });

    it('should support simple or() method', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterEquals('title', 'Book A')
        .or()
        .filterEquals('title', 'Book B')
        .buildFiltersString();

      expect(query).toBe('title==Book A || title==Book B');
    });

    it('should maintain backward compatibility - no parentheses without groups', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .filterEquals('title', 'Book A')
        .filterGreaterThan('pages', 100)
        .buildFiltersString();

      expect(query).toBe('title==Book A,pages>100');
    });

    it('should throw error on unmatched beginGroup', () => {
      const builder = SievePlusQueryBuilder.create<Book>()
        .beginGroup()
          .filterEquals('title', 'Book A');

      expect(() => builder.buildFiltersString()).toThrow('Unmatched beginGroup()');
    });

    it('should throw error on unmatched endGroup', () => {
      const builder = SievePlusQueryBuilder.create<Book>();

      expect(() => builder.endGroup()).toThrow('endGroup() called without matching beginGroup()');
    });

    it('should handle nested groups', () => {
      const query = SievePlusQueryBuilder.create<Book>()
        .beginGroup()
          .beginGroup()
            .filterEquals('title', 'A')
            .or()
            .filterEquals('title', 'B')
          .endGroup()
          .filterGreaterThan('pages', 100)
        .endGroup()
        .filterLessThan('pages', 500)
        .buildFiltersString();

      expect(query).toBe('((title==A || title==B),pages>100),pages<500');
    });

    it('should handle complex pricerunner-style query', () => {
      interface Computer {
        processor: string;
        price: number;
        screenSize: number;
      }

      const query = SievePlusQueryBuilder.create<Computer>()
        .beginGroup()
          .filterEquals('processor', 'Intel i9')
          .or()
          .filterEquals('processor', 'AMD Ryzen 9')
        .endGroup()
        .filterGreaterThanOrEqual('price', 1000)
        .filterLessThanOrEqual('price', 2000)
        .filterGreaterThanOrEqual('screenSize', 14)
        .filterLessThanOrEqual('screenSize', 16)
        .buildFiltersString();

      expect(query).toBe('(processor==Intel i9 || processor==AMD Ryzen 9),price>=1000,price<=2000,screenSize>=14,screenSize<=16');
    });
  });

  describe('parseQueryString', () => {
    it('should parse filters from query string', () => {
      const queryString = 'filters=name@=Bob,id==123&page=1&pageSize=10';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('name@=Bob,id==123');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should parse sorts from query string', () => {
      const queryString = 'sorts=-createdat,name&page=1&pageSize=20';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(20);
    });

    it('should parse pagination from query string', () => {
      const queryString = 'page=5&pageSize=25';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.page).toBe(5);
      expect(result.pageSize).toBe(25);
      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
    });

    it('should handle URL-encoded query string', () => {
      const queryString = 'filters=name%40%3DBob%2Cid%21%3D123&sorts=-createdat';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('name@=Bob,id!=123');
      expect(result.sorts).toBe('-createdat');
    });

    it('should handle query string with leading question mark', () => {
      const queryString = '?filters=name@=Bob&page=1';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.page).toBe(1);
    });

    it('should handle empty query string', () => {
      const builder = SievePlusQueryBuilder.parseQueryString<Author>('');
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('');
      expect(result.sorts).toBe('');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });

    it('should allow chaining after parsing query string', () => {
      const queryString = 'filters=name@=Bob&sorts=name&page=1&pageSize=10';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString)
        .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
        .sortByDescending('createdat')
        .page(2);

      const result = builder.buildSievePlusModel();

      expect(result.filters).toContain('name@=Bob');
      expect(result.filters).toContain('createdat>=');
      expect(result.sorts).toContain('name');
      expect(result.sorts).toContain('-createdat');
      expect(result.page).toBe(2);
      expect(result.pageSize).toBe(10);
    });

    it('should support round-trip with query string', () => {
      const original = SievePlusQueryBuilder.create<Author>()
        .filterContains('name', 'Bob')
        .filterNotEquals('id', '123')
        .sortByDescending('createdat')
        .sortBy('name')
        .page(2)
        .pageSize(15);

      const queryString = original.buildQueryString();
      const parsed = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const rebuilt = parsed.buildQueryString();

      expect(rebuilt).toBe(queryString);
    });

    it('should handle complex real-world query string', () => {
      const queryString = 'filters=name@=Bob,createdat>=2024-01-01T00:00:00.000Z,BooksCount>=3&sorts=-createdat,name&page=2&pageSize=20';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toContain('name@=Bob');
      expect(result.filters).toContain('createdat>=2024-01-01T00:00:00.000Z');
      expect(result.filters).toContain('BooksCount>=3');
      expect(result.sorts).toBe('-createdat,name');
      expect(result.page).toBe(2);
      expect(result.pageSize).toBe(20);
    });

    it('should handle case-insensitive parameter names', () => {
      const queryString = 'Filters=name@=Bob&Sorts=name&Page=1&PageSize=10';

      const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);
      const result = builder.buildSievePlusModel();

      expect(result.filters).toBe('name@=Bob');
      expect(result.sorts).toBe('name');
      expect(result.page).toBe(1);
      expect(result.pageSize).toBe(10);
    });
  });

  describe('Introspection methods', () => {
    describe('getFilters', () => {
      it('should return empty array when no filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>();
        const filters = builder.getFilters();

        expect(filters).toEqual([]);
      });

      it('should parse single filter into FilterInfo', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob');

        const filters = builder.getFilters();

        expect(filters).toHaveLength(1);
        expect(filters[0].propertyName).toBe('name');
        expect(filters[0].operator).toBe('==');
        expect(filters[0].value).toBe('Bob');
        expect(filters[0].originalFilter).toBe('name==Bob');
      });

      it('should parse multiple filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterContains('name', 'Bob')
          .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'));

        const filters = builder.getFilters();

        expect(filters).toHaveLength(2);
        expect(filters[0].propertyName).toBe('name');
        expect(filters[0].operator).toBe('@=');
        expect(filters[0].value).toBe('Bob');
        expect(filters[1].propertyName).toBe('createdat');
        expect(filters[1].operator).toBe('>=');
      });

      it('should parse all filter operators correctly', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('id', '1')
          .filterNotEquals('id', '2')
          .filterContains('name', 'Bob')
          .filterStartsWith('name', 'Al')
          .filterGreaterThan('createdat', new Date('2024-01-01'))
          .filterLessThan('createdat', new Date('2024-12-31'))
          .filterGreaterThanOrEqual('createdat', new Date('2024-01-01'))
          .filterLessThanOrEqual('createdat', new Date('2024-12-31'));

        const filters = builder.getFilters();

        expect(filters).toHaveLength(8);
        expect(filters[0].operator).toBe('==');
        expect(filters[1].operator).toBe('!=');
        expect(filters[2].operator).toBe('@=');
        expect(filters[3].operator).toBe('_=');
        expect(filters[4].operator).toBe('>');
        expect(filters[5].operator).toBe('<');
        expect(filters[6].operator).toBe('>=');
        expect(filters[7].operator).toBe('<=');
      });

      it('should parse custom property filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterByName('BooksCount', '>=', 5);

        const filters = builder.getFilters();

        expect(filters).toHaveLength(1);
        expect(filters[0].propertyName).toBe('BooksCount');
        expect(filters[0].operator).toBe('>=');
        expect(filters[0].value).toBe('5');
      });

      it('should flatten filters from multiple OR groups', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob')
          .or()
          .filterEquals('name', 'Alice')
          .or()
          .filterEquals('name', 'Charlie');

        const filters = builder.getFilters();

        expect(filters).toHaveLength(3);
        expect(filters[0].value).toBe('Bob');
        expect(filters[1].value).toBe('Alice');
        expect(filters[2].value).toBe('Charlie');
      });
    });

    describe('getFilterGroups', () => {
      it('should return empty groups when no filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>();
        const groups = builder.getFilterGroups();

        expect(groups).toHaveLength(1);
        expect(groups[0]).toEqual([]);
      });

      it('should return single group with filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob')
          .filterGreaterThan('createdat', new Date('2024-01-01'));

        const groups = builder.getFilterGroups();

        expect(groups).toHaveLength(1);
        expect(groups[0]).toHaveLength(2);
        expect(groups[0][0].propertyName).toBe('name');
        expect(groups[0][1].propertyName).toBe('createdat');
      });

      it('should separate OR groups correctly', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob')
          .filterGreaterThan('createdat', new Date('2024-01-01'))
          .or()
          .filterEquals('name', 'Alice')
          .filterLessThan('createdat', new Date('2024-06-01'));

        const groups = builder.getFilterGroups();

        expect(groups).toHaveLength(2);
        expect(groups[0]).toHaveLength(2);
        expect(groups[0][0].value).toBe('Bob');
        expect(groups[0][1].operator).toBe('>');
        expect(groups[1]).toHaveLength(2);
        expect(groups[1][0].value).toBe('Alice');
        expect(groups[1][1].operator).toBe('<');
      });

      it('should handle multiple OR groups', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob')
          .or()
          .filterEquals('name', 'Alice')
          .or()
          .filterEquals('name', 'Charlie');

        const groups = builder.getFilterGroups();

        expect(groups).toHaveLength(3);
        expect(groups[0][0].value).toBe('Bob');
        expect(groups[1][0].value).toBe('Alice');
        expect(groups[2][0].value).toBe('Charlie');
      });
    });

    describe('getSorts', () => {
      it('should return empty array when no sorts', () => {
        const builder = SievePlusQueryBuilder.create<Author>();
        const sorts = builder.getSorts();

        expect(sorts).toEqual([]);
      });

      it('should parse ascending sort', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortBy('name');

        const sorts = builder.getSorts();

        expect(sorts).toHaveLength(1);
        expect(sorts[0].propertyName).toBe('name');
        expect(sorts[0].isDescending).toBe(false);
        expect(sorts[0].originalSort).toBe('name');
      });

      it('should parse descending sort', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortByDescending('createdat');

        const sorts = builder.getSorts();

        expect(sorts).toHaveLength(1);
        expect(sorts[0].propertyName).toBe('createdat');
        expect(sorts[0].isDescending).toBe(true);
        expect(sorts[0].originalSort).toBe('-createdat');
      });

      it('should parse multiple sorts', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortByDescending('createdat')
          .sortBy('name')
          .sortByName('BooksCount', true);

        const sorts = builder.getSorts();

        expect(sorts).toHaveLength(3);
        expect(sorts[0].propertyName).toBe('createdat');
        expect(sorts[0].isDescending).toBe(true);
        expect(sorts[1].propertyName).toBe('name');
        expect(sorts[1].isDescending).toBe(false);
        expect(sorts[2].propertyName).toBe('BooksCount');
        expect(sorts[2].isDescending).toBe(true);
      });
    });

    describe('getPage and getPageSize', () => {
      it('should return undefined when not set', () => {
        const builder = SievePlusQueryBuilder.create<Author>();

        expect(builder.getPage()).toBeUndefined();
        expect(builder.getPageSize()).toBeUndefined();
      });

      it('should return page when set', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .page(5);

        expect(builder.getPage()).toBe(5);
      });

      it('should return pageSize when set', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .pageSize(25);

        expect(builder.getPageSize()).toBe(25);
      });

      it('should return both when set', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .page(3)
          .pageSize(50);

        expect(builder.getPage()).toBe(3);
        expect(builder.getPageSize()).toBe(50);
      });
    });

    describe('hasFilter', () => {
      it('should return false when no filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>();

        expect(builder.hasFilter('name')).toBe(false);
      });

      it('should return true when filter exists', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob');

        expect(builder.hasFilter('name')).toBe(true);
      });

      it('should return false when filter does not exist', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob');

        expect(builder.hasFilter('id')).toBe(false);
      });

      it('should find filters across multiple groups', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterEquals('name', 'Bob')
          .or()
          .filterEquals('id', '123');

        expect(builder.hasFilter('name')).toBe(true);
        expect(builder.hasFilter('id')).toBe(true);
      });

      it('should detect custom property filters', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterByName('BooksCount', '>=', 5);

        expect(builder.hasFilter('BooksCount')).toBe(true);
      });

      it('should work with all filter operators', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .filterContains('name', 'Bob')
          .filterGreaterThan('createdat', new Date('2024-01-01'));

        expect(builder.hasFilter('name')).toBe(true);
        expect(builder.hasFilter('createdat')).toBe(true);
      });
    });

    describe('hasSort', () => {
      it('should return false when no sorts', () => {
        const builder = SievePlusQueryBuilder.create<Author>();

        expect(builder.hasSort('name')).toBe(false);
      });

      it('should return true for ascending sort', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortBy('name');

        expect(builder.hasSort('name')).toBe(true);
      });

      it('should return true for descending sort', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortByDescending('createdat');

        expect(builder.hasSort('createdat')).toBe(true);
      });

      it('should return false when sort does not exist', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortBy('name');

        expect(builder.hasSort('createdat')).toBe(false);
      });

      it('should detect custom property sorts', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortByName('BooksCount', true);

        expect(builder.hasSort('BooksCount')).toBe(true);
      });

      it('should work with multiple sorts', () => {
        const builder = SievePlusQueryBuilder.create<Author>()
          .sortByDescending('createdat')
          .sortBy('name')
          .sortByName('BooksCount');

        expect(builder.hasSort('createdat')).toBe(true);
        expect(builder.hasSort('name')).toBe(true);
        expect(builder.hasSort('BooksCount')).toBe(true);
        expect(builder.hasSort('id')).toBe(false);
      });
    });

    describe('Integration - parsing and introspection', () => {
      it('should introspect parsed query string', () => {
        const queryString = 'filters=name@=Bob,createdat>=2024-01-01T00:00:00.000Z&sorts=-createdat,name&page=2&pageSize=20';
        const builder = SievePlusQueryBuilder.parseQueryString<Author>(queryString);

        // Check filters
        const filters = builder.getFilters();
        expect(filters).toHaveLength(2);
        expect(filters[0].propertyName).toBe('name');
        expect(filters[0].operator).toBe('@=');
        expect(filters[1].propertyName).toBe('createdat');
        expect(filters[1].operator).toBe('>=');

        // Check sorts
        const sorts = builder.getSorts();
        expect(sorts).toHaveLength(2);
        expect(sorts[0].propertyName).toBe('createdat');
        expect(sorts[0].isDescending).toBe(true);
        expect(sorts[1].propertyName).toBe('name');
        expect(sorts[1].isDescending).toBe(false);

        // Check pagination
        expect(builder.getPage()).toBe(2);
        expect(builder.getPageSize()).toBe(20);

        // Check has methods
        expect(builder.hasFilter('name')).toBe(true);
        expect(builder.hasFilter('createdat')).toBe(true);
        expect(builder.hasSort('createdat')).toBe(true);
        expect(builder.hasSort('name')).toBe(true);
      });

      it('should introspect parsed SieveModel', () => {
        const model = {
          filters: 'name@=Bob,id!=123',
          sorts: '-createdat,name',
          page: 1,
          pageSize: 10
        };
        const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);

        // Check filter groups
        const groups = builder.getFilterGroups();
        expect(groups).toHaveLength(1);
        expect(groups[0]).toHaveLength(2);

        // Check has methods
        expect(builder.hasFilter('name')).toBe(true);
        expect(builder.hasFilter('id')).toBe(true);
        expect(builder.hasSort('createdat')).toBe(true);
        expect(builder.hasSort('name')).toBe(true);
      });

      it('should introspect OR groups', () => {
        const model = {
          filters: 'name==Bob || name==Alice || name==Charlie',
          sorts: '',
          page: 1,
          pageSize: 10
        };
        const builder = SievePlusQueryBuilder.fromSievePlusModel<Author>(model);

        const groups = builder.getFilterGroups();
        // When parsed, "name==Bob || name==Alice || name==Charlie" creates 3 OR groups
        expect(groups.length).toBeGreaterThanOrEqual(1);

        // Check that all three values are present across groups
        const allFilters = builder.getFilters();
        expect(allFilters).toHaveLength(3);
        expect(allFilters[0].value).toBe('Bob');
        expect(allFilters[1].value).toBe('Alice');
        expect(allFilters[2].value).toBe('Charlie');

        expect(builder.hasFilter('name')).toBe(true);
      });
    });
  });
});

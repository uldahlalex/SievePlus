/**
 * Type-safe Sieve query string builder for TypeScript
 *
 * Supports building filter, sort, and pagination parameters for Sieve-compatible APIs
 */

/**
 * Represents the Sieve model structure
 */
export interface SieveModel {
  filters: string;
  sorts: string;
  page: number;
  pageSize: number;
}

/**
 * Extract property keys from a type (excluding functions and symbols)
 */
type PropertyKeys<T> = {
  [K in keyof T]: T[K] extends Function ? never : K;
}[keyof T];

/**
 * Type-safe Sieve query builder
 * @template T The entity type to build queries for
 */
export class SieveQueryBuilder<T extends object> {
  private filters: string[] = [];
  private sorts: string[] = [];
  private pageValue?: number;
  private pageSizeValue?: number;

  /**
   * Create a new SieveQueryBuilder instance
   */
  static create<T extends object>(): SieveQueryBuilder<T> {
    return new SieveQueryBuilder<T>();
  }

  /**
   * Parse a SieveModel object into a SieveQueryBuilder instance
   * @param model The SieveModel object with filters, sorts, page, and pageSize
   */
  static fromSieveModel<T extends object>(model: SieveModel): SieveQueryBuilder<T> {
    const builder = new SieveQueryBuilder<T>();

    if (model.filters) {
      builder.filters = this.parseFilters(model.filters);
    }

    if (model.sorts) {
      builder.sorts = this.parseSorts(model.sorts);
    }

    if (model.page !== undefined && model.page !== null) {
      builder.pageValue = model.page;
    }

    if (model.pageSize !== undefined && model.pageSize !== null) {
      builder.pageSizeValue = model.pageSize;
    }

    return builder;
  }

  /**
   * Parse a query string into a SieveQueryBuilder instance
   * @param queryString The query string to parse (e.g., "filters=name@=Bob&sorts=-createdat&page=1&pageSize=10")
   */
  static parseQueryString<T extends object>(queryString: string): SieveQueryBuilder<T> {
    const builder = new SieveQueryBuilder<T>();

    if (!queryString || queryString.trim() === '') {
      return builder;
    }

    // Remove leading '?' if present
    queryString = queryString.trim().replace(/^\?/, '');

    const parameters = queryString.split('&');

    for (const param of parameters) {
      const equalIndex = param.indexOf('=');
      if (equalIndex === -1) continue;

      const key = param.substring(0, equalIndex).toLowerCase();
      const value = decodeURIComponent(param.substring(equalIndex + 1));

      switch (key) {
        case 'filters':
          builder.filters = this.parseFilters(value);
          break;
        case 'sorts':
          builder.sorts = this.parseSorts(value);
          break;
        case 'page':
          const page = parseInt(value, 10);
          if (!isNaN(page)) {
            builder.pageValue = page;
          }
          break;
        case 'pagesize':
          const pageSize = parseInt(value, 10);
          if (!isNaN(pageSize)) {
            builder.pageSizeValue = pageSize;
          }
          break;
      }
    }

    return builder;
  }

  /**
   * Parse filters string into individual filter components
   */
  private static parseFilters(filtersString: string): string[] {
    if (!filtersString || filtersString.trim() === '') {
      return [];
    }

    // Split by comma and trim
    return filtersString.split(',').map(f => f.trim()).filter(f => f.length > 0);
  }

  /**
   * Parse sorts string into individual sort components
   */
  private static parseSorts(sortsString: string): string[] {
    if (!sortsString || sortsString.trim() === '') {
      return [];
    }

    // Split by comma and trim
    return sortsString.split(',').map(s => s.trim()).filter(s => s.length > 0);
  }

  /**
   * Remove all filters for a specific property
   */
  removeFilters<K extends PropertyKeys<T>>(property: K): this {
    const propertyName = String(property);
    this.filters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));
    return this;
  }

  /**
   * Remove all filters for a specific property name (for mapped properties)
   */
  removeFiltersByName(propertyName: string): this {
    this.filters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));
    return this;
  }

  /**
   * Check if a filter string is for the given property name
   */
  private isFilterForProperty(filter: string, propertyName: string): boolean {
    const operators = ['==', '!=', '>=', '<=', '@=', '_=', '>', '<'];
    for (const op of operators) {
      const index = filter.indexOf(op);
      if (index > 0) {
        const filterProp = filter.substring(0, index);
        return filterProp === propertyName;
      }
    }
    return false;
  }

  /**
   * Add a filter using equals operator (==)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterEquals<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | boolean,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    this.filters.push(`${String(property)}==${value}`);
    return this;
  }

  /**
   * Add a filter using not equals operator (!=)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterNotEquals<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | boolean,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    this.filters.push(`${String(property)}!=${value}`);
    return this;
  }

  /**
   * Add a filter using contains operator (@=)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterContains<K extends PropertyKeys<T>>(
    property: K,
    value: string,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    this.filters.push(`${String(property)}@=${value}`);
    return this;
  }

  /**
   * Add a filter using starts with operator (_=)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterStartsWith<K extends PropertyKeys<T>>(
    property: K,
    value: string,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    this.filters.push(`${String(property)}_=${value}`);
    return this;
  }

  /**
   * Add a filter using greater than operator (>)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterGreaterThan<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    this.filters.push(`${String(property)}>${formattedValue}`);
    return this;
  }

  /**
   * Add a filter using less than operator (<)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterLessThan<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    this.filters.push(`${String(property)}<${formattedValue}`);
    return this;
  }

  /**
   * Add a filter using greater than or equal operator (>=)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterGreaterThanOrEqual<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    this.filters.push(`${String(property)}>=${formattedValue}`);
    return this;
  }

  /**
   * Add a filter using less than or equal operator (<=)
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterLessThanOrEqual<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    this.filters.push(`${String(property)}<=${formattedValue}`);
    return this;
  }

  /**
   * Add a filter using a custom property name (for mapped properties)
   * @param propertyName The custom property name (e.g., "BooksCount")
   * @param operator The operator symbol (e.g., ">=", "==", "@=")
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterByName(
    propertyName: string,
    operator: string,
    value: string | number | boolean | Date,
    replace: boolean = false
  ): this {
    if (replace) {
      this.removeFiltersByName(propertyName);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    this.filters.push(`${propertyName}${operator}${formattedValue}`);
    return this;
  }

  /**
   * Add ascending sort for a property
   */
  sortBy<K extends PropertyKeys<T>>(property: K): this {
    this.sorts.push(String(property));
    return this;
  }

  /**
   * Add descending sort for a property
   */
  sortByDescending<K extends PropertyKeys<T>>(property: K): this {
    this.sorts.push(`-${String(property)}`);
    return this;
  }

  /**
   * Add sort using a custom property name (for mapped properties)
   * @param propertyName The custom property name (e.g., "BooksCount")
   * @param descending Whether to sort descending (default: false)
   */
  sortByName(propertyName: string, descending: boolean = false): this {
    this.sorts.push(descending ? `-${propertyName}` : propertyName);
    return this;
  }

  /**
   * Set the page number for pagination
   */
  page(page: number): this {
    this.pageValue = page;
    return this;
  }

  /**
   * Set the page size for pagination
   */
  pageSize(pageSize: number): this {
    this.pageSizeValue = pageSize;
    return this;
  }

  /**
   * Build the Filters query string component
   */
  buildFiltersString(): string {
    return this.filters.join(',');
  }

  /**
   * Build the Sorts query string component
   */
  buildSortsString(): string {
    return this.sorts.join(',');
  }

  /**
   * Build a complete SieveModel object
   */
  buildSieveModel(): SieveModel {
    return {
      filters: this.buildFiltersString(),
      sorts: this.buildSortsString(),
      page: this.pageValue ?? 1,
      pageSize: this.pageSizeValue ?? 10
    };
  }

  /**
   * Build the complete query string for use in HTTP requests
   */
  buildQueryString(): string {
    const parts: string[] = [];

    if (this.filters.length > 0) {
      parts.push(`filters=${encodeURIComponent(this.filters.join(','))}`);
    }

    if (this.sorts.length > 0) {
      parts.push(`sorts=${encodeURIComponent(this.sorts.join(','))}`);
    }

    if (this.pageValue !== undefined) {
      parts.push(`page=${this.pageValue}`);
    }

    if (this.pageSizeValue !== undefined) {
      parts.push(`pageSize=${this.pageSizeValue}`);
    }

    return parts.join('&');
  }

  /**
   * Build an object suitable for use as query parameters in fetch/axios
   */
  buildQueryParams(): Record<string, string | number> {
    const params: Record<string, string | number> = {};

    if (this.filters.length > 0) {
      params.filters = this.filters.join(',');
    }

    if (this.sorts.length > 0) {
      params.sorts = this.sorts.join(',');
    }

    if (this.pageValue !== undefined) {
      params.page = this.pageValue;
    }

    if (this.pageSizeValue !== undefined) {
      params.pageSize = this.pageSizeValue;
    }

    return params;
  }
}

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
 * Represents a parsed filter with its property name, operator, and value
 */
export interface FilterInfo {
  propertyName: string;
  operator: string;
  value: string;
  originalFilter: string;
}

/**
 * Represents a parsed sort with its property name and direction
 */
export interface SortInfo {
  propertyName: string;
  isDescending: boolean;
  originalSort: string;
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
/**
 * Represents a segment in the filter expression tree
 * Can be a simple filter, an AND group, or an OR group
 */
class FilterSegment {
  parts: (string | FilterSegment)[] = [];
  isOrGroup: boolean = false;
  wrapInParentheses: boolean = false;

  toQueryString(): string {
    if (this.parts.length === 0) {
      return '';
    }

    const separator = this.isOrGroup ? ' || ' : ',';
    const parts = this.parts
      .map(p => typeof p === 'string' ? p : p.toQueryString())
      .filter(p => p.length > 0);
    const result = parts.join(separator);

    if (this.wrapInParentheses && this.parts.length > 1) {
      return `(${result})`;
    }

    return result;
  }
}

export class SieveQueryBuilder<T extends object> {
  private filters: string[] = [];
  private filterGroups: string[][] = [[]]; // Support for OR groups (backward compat)
  private currentGroupIndex: number = 0;
  private sorts: string[] = [];
  private pageValue?: number;
  private pageSizeValue?: number;

  // New fields for parentheses support
  private segmentStack: FilterSegment[] = [];
  private currentSegment: FilterSegment = new FilterSegment();

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
      // Parse filters into groups (handles OR with " || " separator)
      this.parseFiltersIntoGroups(model.filters, builder);
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
          this.parseFiltersIntoGroups(value, builder);
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
   * Parse filters string and populate builder's filter groups
   * Handles both comma-separated AND filters and " || " separated OR groups
   */
  private static parseFiltersIntoGroups<T extends object>(
    filtersString: string,
    builder: SieveQueryBuilder<T>
  ): void {
    if (!filtersString || filtersString.trim() === '') {
      return;
    }

    // Clear default empty group
    builder.filterGroups = [];
    builder.currentGroupIndex = 0;

    // Split by " || " for OR groups
    const orGroups = filtersString.split(' || ');

    for (const orGroup of orGroups) {
      // Split each OR group by comma for AND filters
      const filters = orGroup
        .split(',')
        .map(f => f.trim())
        .filter(f => f.length > 0);

      if (filters.length > 0) {
        builder.filterGroups.push(filters);
        // Also add to current segment for consistency
        filters.forEach(filter => {
          builder.currentSegment.parts.push(filter);
        });
      }
    }

    // Ensure at least one group exists
    if (builder.filterGroups.length === 0) {
      builder.filterGroups.push([]);
    }

    builder.currentGroupIndex = builder.filterGroups.length - 1;
  }

  /**
   * Helper method to add a filter to both systems (new segment + old groups)
   */
  private addFilter(filter: string): void {
    this.currentSegment.parts.push(filter);
    this.filterGroups[this.currentGroupIndex].push(filter);
  }

  /**
   * Start a new OR group. Subsequent filters will be OR'd with previous filter groups.
   * Example:
   * ```typescript
   * builder.filterEquals('processor', 'Intel i9')
   *        .or()
   *        .filterEquals('processor', 'AMD Ryzen 9')
   * // Produces: "processor==Intel i9 || processor==AMD Ryzen 9"
   * ```
   */
  or(): this {
    // Mark current segment as OR group
    this.currentSegment.isOrGroup = true;

    // Also maintain backward compatibility with old group system
    this.filterGroups.push([]);
    this.currentGroupIndex++;

    return this;
  }

  /**
   * Begin a grouped sub-expression with parentheses
   * Filters added after beginGroup will be wrapped in parentheses
   * Must be paired with endGroup()
   *
   * Example:
   * ```typescript
   * builder.beginGroup()
   *        .filterEquals('processor', 'Intel')
   *        .or()
   *        .filterEquals('processor', 'AMD')
   *        .endGroup()
   *        .filterGreaterThan('price', 1000)
   * // Produces: (processor==Intel || processor==AMD),price>1000
   * ```
   */
  beginGroup(): this {
    // Push current segment onto stack
    this.segmentStack.push(this.currentSegment);

    // Create new segment that will be wrapped in parentheses
    this.currentSegment = new FilterSegment();
    this.currentSegment.wrapInParentheses = true;

    return this;
  }

  /**
   * End a grouped sub-expression
   * Must be paired with beginGroup()
   */
  endGroup(): this {
    if (this.segmentStack.length === 0) {
      throw new Error('endGroup() called without matching beginGroup()');
    }

    // Pop the parent segment and add current segment to it
    const completedSegment = this.currentSegment;
    this.currentSegment = this.segmentStack.pop()!;
    this.currentSegment.parts.push(completedSegment);

    return this;
  }

  /**
   * Create a filter group with alternative values for a single property
   * This is a helper for the common case of OR-ing values on one property
   *
   * Example:
   * ```typescript
   * builder.filterWithAlternatives(
   *     'processor',
   *     ['Intel i9', 'AMD Ryzen 9'],
   *     b => b.filterGreaterThan('price', 1000)
   * )
   * // Produces: (processor==Intel i9 || processor==AMD Ryzen 9),price>1000
   * ```
   */
  filterWithAlternatives<K extends PropertyKeys<T>>(
    property: K,
    alternatives: (T[K] | string | number | boolean)[],
    sharedConstraints?: (builder: SieveQueryBuilder<T>) => void
  ): this {
    if (!alternatives || alternatives.length === 0) {
      return this;
    }

    this.beginGroup();

    for (let i = 0; i < alternatives.length; i++) {
      if (i > 0) {
        this.or();
      }
      this.filterEquals(property, alternatives[i]);
    }

    this.endGroup();

    // Apply shared constraints after the group
    if (sharedConstraints) {
      sharedConstraints(this);
    }

    return this;
  }

  /**
   * Remove all filters for a specific property
   */
  removeFilters<K extends PropertyKeys<T>>(property: K): this {
    const propertyName = String(property);

    // Remove from both segment system and old filters array
    this.currentSegment.parts = this.currentSegment.parts.filter(p => {
      if (typeof p === 'string') {
        return !this.isFilterForProperty(p, propertyName);
      }
      return true; // Keep FilterSegment objects
    });

    this.filters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));
    this.filterGroups[this.currentGroupIndex] = this.filterGroups[this.currentGroupIndex]
      .filter(f => !this.isFilterForProperty(f, propertyName));

    return this;
  }

  /**
   * Remove all filters for a specific property name (for mapped properties)
   */
  removeFiltersByName(propertyName: string): this {
    // Remove from both segment system and old filters array
    this.currentSegment.parts = this.currentSegment.parts.filter(p => {
      if (typeof p === 'string') {
        return !this.isFilterForProperty(p, propertyName);
      }
      return true; // Keep FilterSegment objects
    });

    this.filters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));
    this.filterGroups[this.currentGroupIndex] = this.filterGroups[this.currentGroupIndex]
      .filter(f => !this.isFilterForProperty(f, propertyName));

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
    this.addFilter(`${String(property)}==${value}`);
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
    this.addFilter(`${String(property)}!=${value}`);
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
    this.addFilter(`${String(property)}@=${value}`);
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
    this.addFilter(`${String(property)}_=${value}`);
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
    this.addFilter(`${String(property)}>${formattedValue}`);
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
    this.addFilter(`${String(property)}<${formattedValue}`);
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
    this.addFilter(`${String(property)}>=${formattedValue}`);
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
    this.addFilter(`${String(property)}<=${formattedValue}`);
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
    this.addFilter(`${propertyName}${operator}${formattedValue}`);
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
    // Check if we used the new segment system (beginGroup/endGroup)
    if (this.segmentStack.length > 0) {
      throw new Error('Unmatched beginGroup() call - missing endGroup()');
    }

    // If current segment has parts, use the new segment system
    if (this.currentSegment.parts.length > 0) {
      return this.currentSegment.toQueryString();
    }

    // Fall back to old system for backward compatibility
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

    const filterString = this.buildFiltersString();
    if (filterString.length > 0) {
      parts.push(`filters=${encodeURIComponent(filterString)}`);
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

    const filterString = this.buildFiltersString();
    if (filterString.length > 0) {
      params.filters = filterString;
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

  /**
   * Parse a filter string into a FilterInfo object
   */
  private static parseFilterString(filter: string): FilterInfo {
    // Try to match operators in order of length (longest first to avoid partial matches)
    const operators = ['==', '!=', '>=', '<=', '@=', '_=', '>', '<'];

    for (const op of operators) {
      const index = filter.indexOf(op);
      if (index > 0) {
        const propertyName = filter.substring(0, index);
        const value = filter.substring(index + op.length);

        return {
          propertyName,
          operator: op,
          value,
          originalFilter: filter
        };
      }
    }

    // If no operator found, return the whole thing as property name
    return {
      propertyName: filter,
      operator: '',
      value: '',
      originalFilter: filter
    };
  }

  /**
   * Get all filters from all groups flattened into FilterInfo objects
   */
  getFilters(): FilterInfo[] {
    const filterInfos: FilterInfo[] = [];

    for (const group of this.filterGroups) {
      for (const filter of group) {
        const filterInfo = SieveQueryBuilder.parseFilterString(filter);
        filterInfos.push(filterInfo);
      }
    }

    return filterInfos;
  }

  /**
   * Get all filter groups as structured FilterInfo objects
   */
  getFilterGroups(): FilterInfo[][] {
    const groups: FilterInfo[][] = [];

    for (const group of this.filterGroups) {
      const filterInfos = group.map(filter =>
        SieveQueryBuilder.parseFilterString(filter)
      );
      groups.push(filterInfos);
    }

    return groups;
  }

  /**
   * Get all sorts as structured SortInfo objects
   */
  getSorts(): SortInfo[] {
    const sortInfos: SortInfo[] = [];

    for (const sort of this.sorts) {
      const isDescending = sort.startsWith('-');
      const propertyName = isDescending ? sort.substring(1) : sort;

      sortInfos.push({
        propertyName,
        isDescending,
        originalSort: sort
      });
    }

    return sortInfos;
  }

  /**
   * Get the current page number
   */
  getPage(): number | undefined {
    return this.pageValue;
  }

  /**
   * Get the current page size
   */
  getPageSize(): number | undefined {
    return this.pageSizeValue;
  }

  /**
   * Check if a filter exists for the given property name in any group
   */
  hasFilter(propertyName: string): boolean {
    return this.filterGroups.some(group =>
      group.some(f => this.isFilterForProperty(f, propertyName))
    );
  }

  /**
   * Check if a sort exists for the given property name
   */
  hasSort(propertyName: string): boolean {
    return this.sorts.some(s => s === propertyName || s === `-${propertyName}`);
  }
}

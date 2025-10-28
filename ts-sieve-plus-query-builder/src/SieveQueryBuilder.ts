/**
 * Type-safe SievePlus query string builder for TypeScript
 *
 * Supports building filter, sort, and pagination parameters for SievePlus-compatible APIs
 */

/**
 * Represents the SievePlus model structure with query model type
 * @template TQueryModel The query model type that defines available filter and sort properties
 */
export interface SievePlusModel<TQueryModel = any> {
  filters: string;
  sorts: string;
  page: number;
  pageSize: number;
  /** Query model type for OpenAPI/TypeScript type safety - always undefined at runtime */
  queryModel: TQueryModel | undefined;
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
 * Type-safe SievePlus query builder
 * @template T The query model type to build queries for
 */
/**
 * Represents a segment in the filter expression tree
 * Can be a simple filter, an AND group, or an OR group
 */
class FilterSegment {
  readonly parts: readonly (string | FilterSegment)[];
  readonly isOrGroup: boolean;
  readonly wrapInParentheses: boolean;

  constructor(
    parts: readonly (string | FilterSegment)[] = [],
    isOrGroup: boolean = false,
    wrapInParentheses: boolean = false
  ) {
    this.parts = parts;
    this.isOrGroup = isOrGroup;
    this.wrapInParentheses = wrapInParentheses;
  }

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

  /**
   * Returns a new FilterSegment with the added part
   */
  addPart(part: string | FilterSegment): FilterSegment {
    return new FilterSegment([...this.parts, part], this.isOrGroup, this.wrapInParentheses);
  }

  /**
   * Returns a new FilterSegment with filtered parts
   */
  filterParts(predicate: (part: string | FilterSegment) => boolean): FilterSegment {
    return new FilterSegment(this.parts.filter(predicate), this.isOrGroup, this.wrapInParentheses);
  }
}

export class SievePlusQueryBuilder<T extends object> {
  private readonly filters: readonly string[];
  private readonly filterGroups: readonly (readonly string[])[]; // Support for OR groups (backward compat)
  private readonly currentGroupIndex: number;
  private readonly sorts: readonly string[];
  private readonly pageValue?: number;
  private readonly pageSizeValue?: number;

  // New fields for parentheses support
  private readonly segmentStack: readonly FilterSegment[];
  private readonly currentSegment: FilterSegment;

  /**
   * Private constructor - use static factory methods to create instances
   */
  private constructor(
    filters: readonly string[] = [],
    filterGroups: readonly (readonly string[])[] = [[]],
    currentGroupIndex: number = 0,
    sorts: readonly string[] = [],
    pageValue?: number,
    pageSizeValue?: number,
    segmentStack: readonly FilterSegment[] = [],
    currentSegment: FilterSegment = new FilterSegment()
  ) {
    this.filters = filters;
    this.filterGroups = filterGroups;
    this.currentGroupIndex = currentGroupIndex;
    this.sorts = sorts;
    this.pageValue = pageValue;
    this.pageSizeValue = pageSizeValue;
    this.segmentStack = segmentStack;
    this.currentSegment = currentSegment;
  }

  /**
   * Clone this builder with optional property overrides
   */
  private clone(overrides: Partial<{
    filters: readonly string[];
    filterGroups: readonly (readonly string[])[];
    currentGroupIndex: number;
    sorts: readonly string[];
    pageValue: number | undefined;
    pageSizeValue: number | undefined;
    segmentStack: readonly FilterSegment[];
    currentSegment: FilterSegment;
  }> = {}): SievePlusQueryBuilder<T> {
    return new SievePlusQueryBuilder<T>(
      overrides.filters ?? this.filters,
      overrides.filterGroups ?? this.filterGroups,
      overrides.currentGroupIndex ?? this.currentGroupIndex,
      overrides.sorts ?? this.sorts,
      overrides.pageValue !== undefined ? overrides.pageValue : this.pageValue,
      overrides.pageSizeValue !== undefined ? overrides.pageSizeValue : this.pageSizeValue,
      overrides.segmentStack ?? this.segmentStack,
      overrides.currentSegment ?? this.currentSegment
    );
  }

  /**
   * Create a new SievePlusQueryBuilder instance
   */
  static create<T extends object>(): SievePlusQueryBuilder<T> {
    return new SievePlusQueryBuilder<T>();
  }

  /**
   * Parse a SievePlusModel object into a SievePlusQueryBuilder instance
   * @param model The SievePlusModel object with filters, sorts, page, and pageSize
   */
  static fromSievePlusModel<T extends object>(model: SievePlusModel<T>): SievePlusQueryBuilder<T> {
    let builder = new SievePlusQueryBuilder<T>();

    if (model.filters) {
      // Parse filters into groups (handles OR with " || " separator)
      const { filterGroups, currentGroupIndex, currentSegment } = this.parseFiltersIntoGroupsImmutable(model.filters);
      builder = builder.clone({ filterGroups, currentGroupIndex, currentSegment });
    }

    if (model.sorts) {
      builder = builder.clone({ sorts: this.parseSorts(model.sorts) });
    }

    if (model.page !== undefined && model.page !== null) {
      builder = builder.clone({ pageValue: model.page });
    }

    if (model.pageSize !== undefined && model.pageSize !== null) {
      builder = builder.clone({ pageSizeValue: model.pageSize });
    }

    return builder;
  }

  /**
   * Parse a query string into a SievePlusQueryBuilder instance
   * @param queryString The query string to parse (e.g., "filters=name@=Bob&sorts=-createdat&page=1&pageSize=10")
   */
  static parseQueryString<T extends object>(queryString: string): SievePlusQueryBuilder<T> {
    let builder = new SievePlusQueryBuilder<T>();

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
          const { filterGroups, currentGroupIndex, currentSegment } = this.parseFiltersIntoGroupsImmutable(value);
          builder = builder.clone({ filterGroups, currentGroupIndex, currentSegment });
          break;
        case 'sorts':
          builder = builder.clone({ sorts: this.parseSorts(value) });
          break;
        case 'page':
          const page = parseInt(value, 10);
          if (!isNaN(page)) {
            builder = builder.clone({ pageValue: page });
          }
          break;
        case 'pagesize':
          const pageSize = parseInt(value, 10);
          if (!isNaN(pageSize)) {
            builder = builder.clone({ pageSizeValue: pageSize });
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
   * Parse filters string and return filter groups (immutable version)
   * Handles both comma-separated AND filters and " || " separated OR groups
   */
  private static parseFiltersIntoGroupsImmutable(
    filtersString: string
  ): {
    filterGroups: readonly (readonly string[])[];
    currentGroupIndex: number;
    currentSegment: FilterSegment;
  } {
    if (!filtersString || filtersString.trim() === '') {
      return {
        filterGroups: [[]],
        currentGroupIndex: 0,
        currentSegment: new FilterSegment()
      };
    }

    const filterGroups: string[][] = [];
    let currentSegment = new FilterSegment();

    // Split by " || " for OR groups
    const orGroups = filtersString.split(' || ');

    for (const orGroup of orGroups) {
      // Split each OR group by comma for AND filters
      const filters = orGroup
        .split(',')
        .map(f => f.trim())
        .filter(f => f.length > 0);

      if (filters.length > 0) {
        filterGroups.push(filters);
        // Also add to current segment for consistency
        filters.forEach(filter => {
          currentSegment = currentSegment.addPart(filter);
        });
      }
    }

    // Ensure at least one group exists
    if (filterGroups.length === 0) {
      filterGroups.push([]);
    }

    return {
      filterGroups,
      currentGroupIndex: filterGroups.length - 1,
      currentSegment
    };
  }

  /**
   * Helper method to add a filter to both systems (new segment + old groups)
   * Returns a new builder instance with the filter added
   */
  private addFilter(filter: string): SievePlusQueryBuilder<T> {
    const newSegment = this.currentSegment.addPart(filter);
    const newFilterGroups = this.filterGroups.map((group, index) =>
      index === this.currentGroupIndex ? [...group, filter] : group
    );

    return this.clone({
      currentSegment: newSegment,
      filterGroups: newFilterGroups
    });
  }

  /**
   * Start a new OR group. Subsequent filters will be OR'd with previous filter groups.
   * Returns a new builder instance.
   * Example:
   * ```typescript
   * const query = builder
   *   .filterEquals('processor', 'Intel i9')
   *   .or()
   *   .filterEquals('processor', 'AMD Ryzen 9');
   * // Produces: "processor==Intel i9 || processor==AMD Ryzen 9"
   * ```
   */
  or(): SievePlusQueryBuilder<T> {
    // Create new segment marked as OR group
    const newSegment = new FilterSegment(this.currentSegment.parts, true, this.currentSegment.wrapInParentheses);

    // Also maintain backward compatibility with old group system
    const newFilterGroups = [...this.filterGroups, []];
    const newGroupIndex = this.currentGroupIndex + 1;

    return this.clone({
      currentSegment: newSegment,
      filterGroups: newFilterGroups,
      currentGroupIndex: newGroupIndex
    });
  }

  /**
   * Begin a grouped sub-expression with parentheses
   * Filters added after beginGroup will be wrapped in parentheses
   * Must be paired with endGroup()
   * Returns a new builder instance.
   *
   * Example:
   * ```typescript
   * const query = builder
   *   .beginGroup()
   *   .filterEquals('processor', 'Intel')
   *   .or()
   *   .filterEquals('processor', 'AMD')
   *   .endGroup()
   *   .filterGreaterThan('price', 1000);
   * // Produces: (processor==Intel || processor==AMD),price>1000
   * ```
   */
  beginGroup(): SievePlusQueryBuilder<T> {
    // Push current segment onto stack
    const newStack = [...this.segmentStack, this.currentSegment];

    // Create new segment that will be wrapped in parentheses
    const newSegment = new FilterSegment([], false, true);

    return this.clone({
      segmentStack: newStack,
      currentSegment: newSegment
    });
  }

  /**
   * End a grouped sub-expression
   * Must be paired with beginGroup()
   * Returns a new builder instance.
   */
  endGroup(): SievePlusQueryBuilder<T> {
    if (this.segmentStack.length === 0) {
      throw new Error('endGroup() called without matching beginGroup()');
    }

    // Pop the parent segment and add current segment to it
    const completedSegment = this.currentSegment;
    const newStack = this.segmentStack.slice(0, -1);
    const parentSegment = this.segmentStack[this.segmentStack.length - 1];
    const newSegment = parentSegment.addPart(completedSegment);

    return this.clone({
      segmentStack: newStack,
      currentSegment: newSegment
    });
  }

  /**
   * Create a filter group with alternative values for a single property
   * This is a helper for the common case of OR-ing values on one property
   * Returns a new builder instance.
   *
   * Example:
   * ```typescript
   * const query = builder.filterWithAlternatives(
   *   'processor',
   *   ['Intel i9', 'AMD Ryzen 9'],
   *   b => b.filterGreaterThan('price', 1000)
   * );
   * // Produces: (processor==Intel i9 || processor==AMD Ryzen 9),price>1000
   * ```
   */
  filterWithAlternatives<K extends PropertyKeys<T>>(
    property: K,
    alternatives: (T[K] | string | number | boolean)[],
    sharedConstraints?: (builder: SievePlusQueryBuilder<T>) => SievePlusQueryBuilder<T>
  ): SievePlusQueryBuilder<T> {
    if (!alternatives || alternatives.length === 0) {
      return this;
    }

    let result = this.beginGroup();

    for (let i = 0; i < alternatives.length; i++) {
      if (i > 0) {
        result = result.or();
      }
      result = result.filterEquals(property, alternatives[i]);
    }

    result = result.endGroup();

    // Apply shared constraints after the group
    if (sharedConstraints) {
      result = sharedConstraints(result);
    }

    return result;
  }

  /**
   * Remove all filters for a specific property
   * Returns a new builder instance.
   */
  removeFilters<K extends PropertyKeys<T>>(property: K): SievePlusQueryBuilder<T> {
    const propertyName = String(property);

    // Remove from segment system
    const newSegment = this.currentSegment.filterParts(p => {
      if (typeof p === 'string') {
        return !this.isFilterForProperty(p, propertyName);
      }
      return true; // Keep FilterSegment objects
    });

    // Remove from old filters array
    const newFilters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));

    // Remove from filter groups
    const newFilterGroups = this.filterGroups.map((group, index) =>
      index === this.currentGroupIndex
        ? group.filter(f => !this.isFilterForProperty(f, propertyName))
        : group
    );

    return this.clone({
      currentSegment: newSegment,
      filters: newFilters,
      filterGroups: newFilterGroups
    });
  }

  /**
   * Remove all filters for a specific property name (for mapped properties)
   * Returns a new builder instance.
   */
  removeFiltersByName(propertyName: string): SievePlusQueryBuilder<T> {
    // Remove from segment system
    const newSegment = this.currentSegment.filterParts(p => {
      if (typeof p === 'string') {
        return !this.isFilterForProperty(p, propertyName);
      }
      return true; // Keep FilterSegment objects
    });

    // Remove from old filters array
    const newFilters = this.filters.filter(f => !this.isFilterForProperty(f, propertyName));

    // Remove from filter groups
    const newFilterGroups = this.filterGroups.map((group, index) =>
      index === this.currentGroupIndex
        ? group.filter(f => !this.isFilterForProperty(f, propertyName))
        : group
    );

    return this.clone({
      currentSegment: newSegment,
      filters: newFilters,
      filterGroups: newFilterGroups
    });
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
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterEquals<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | boolean,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    return builder.addFilter(`${String(property)}==${value}`);
  }

  /**
   * Add a filter using not equals operator (!=)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterNotEquals<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | boolean,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    return builder.addFilter(`${String(property)}!=${value}`);
  }

  /**
   * Add a filter using contains operator (@=)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterContains<K extends PropertyKeys<T>>(
    property: K,
    value: string,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    return builder.addFilter(`${String(property)}@=${value}`);
  }

  /**
   * Add a filter using starts with operator (_=)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterStartsWith<K extends PropertyKeys<T>>(
    property: K,
    value: string,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    return builder.addFilter(`${String(property)}_=${value}`);
  }

  /**
   * Add a filter using greater than operator (>)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterGreaterThan<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    return builder.addFilter(`${String(property)}>${formattedValue}`);
  }

  /**
   * Add a filter using less than operator (<)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterLessThan<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    return builder.addFilter(`${String(property)}<${formattedValue}`);
  }

  /**
   * Add a filter using greater than or equal operator (>=)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterGreaterThanOrEqual<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    return builder.addFilter(`${String(property)}>=${formattedValue}`);
  }

  /**
   * Add a filter using less than or equal operator (<=)
   * Returns a new builder instance.
   * @param property The property to filter on
   * @param value The value to filter by
   * @param replace If true, removes existing filters for this property first (default: false)
   */
  filterLessThanOrEqual<K extends PropertyKeys<T>>(
    property: K,
    value: T[K] | string | number | Date,
    replace: boolean = false
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFilters(property);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    return builder.addFilter(`${String(property)}<=${formattedValue}`);
  }

  /**
   * Add a filter using a custom property name (for mapped properties)
   * Returns a new builder instance.
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
  ): SievePlusQueryBuilder<T> {
    let builder = this as SievePlusQueryBuilder<T>;
    if (replace) {
      builder = builder.removeFiltersByName(propertyName);
    }
    const formattedValue = value instanceof Date ? value.toISOString() : value;
    return builder.addFilter(`${propertyName}${operator}${formattedValue}`);
  }

  /**
   * Add ascending sort for a property
   * Returns a new builder instance.
   */
  sortBy<K extends PropertyKeys<T>>(property: K): SievePlusQueryBuilder<T> {
    return this.clone({
      sorts: [...this.sorts, String(property)]
    });
  }

  /**
   * Add descending sort for a property
   * Returns a new builder instance.
   */
  sortByDescending<K extends PropertyKeys<T>>(property: K): SievePlusQueryBuilder<T> {
    return this.clone({
      sorts: [...this.sorts, `-${String(property)}`]
    });
  }

  /**
   * Add sort using a custom property name (for mapped properties)
   * Returns a new builder instance.
   * @param propertyName The custom property name (e.g., "BooksCount")
   * @param descending Whether to sort descending (default: false)
   */
  sortByName(propertyName: string, descending: boolean = false): SievePlusQueryBuilder<T> {
    return this.clone({
      sorts: [...this.sorts, descending ? `-${propertyName}` : propertyName]
    });
  }

  /**
   * Set the page number for pagination
   * Returns a new builder instance.
   */
  page(page: number): SievePlusQueryBuilder<T> {
    return this.clone({ pageValue: page });
  }

  /**
   * Set the page size for pagination
   * Returns a new builder instance.
   */
  pageSize(pageSize: number): SievePlusQueryBuilder<T> {
    return this.clone({ pageSizeValue: pageSize });
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
   * Build a complete SievePlusModel object with the query model type
   * The queryModel property is always undefined - it only exists for TypeScript type checking
   */
  buildSievePlusModel(): SievePlusModel<T> {
    return {
      filters: this.buildFiltersString(),
      sorts: this.buildSortsString(),
      page: this.pageValue ?? 1,
      pageSize: this.pageSizeValue ?? 10,
      queryModel: undefined
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
        const filterInfo = SievePlusQueryBuilder.parseFilterString(filter);
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
        SievePlusQueryBuilder.parseFilterString(filter)
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

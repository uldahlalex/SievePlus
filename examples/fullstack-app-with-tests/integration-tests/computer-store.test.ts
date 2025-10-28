import { describe, test, expect } from 'vitest';

const API_URL = process.env.API_URL || 'http://localhost:5555';

interface Computer {
  Id: number;
  Name: string;
  Price: number;
  ReleaseDate: string;
  BrandId: number;
  Brand?: Brand;
  CategoryId: number;
  Category?: Category;
}

interface Brand {
  Id: number;
  Name: string;
  Country: string;
}

interface Category {
  Id: number;
  Name: string;
}

interface SievePlusRequest {
  Filters?: string;
  Sorts?: string;
  Page?: number;
  PageSize?: number;
}

async function apiPost<T>(endpoint: string, body: SievePlusRequest): Promise<T> {
  const response = await fetch(`${API_URL}/api/ComputerStore/${endpoint}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(`API request failed: ${response.status} ${response.statusText}`);
  }

  const json = await response.json();

  // Handle JSON.NET ReferenceHandler.Preserve format
  if (json && typeof json === 'object' && '$values' in json) {
    return json.$values as T;
  }

  return json;
}

describe('Computer Store API - Basic Filtering', () => {
  test('should get all computers without filters', async () => {
    const computers = await apiPost<Computer[]>('GetComputers', {});

    expect(computers).toBeDefined();
    expect(Array.isArray(computers)).toBe(true);
    expect(computers.length).toBeGreaterThan(0);
  });

  test('should filter computers by exact name', async () => {
    const allComputers = await apiPost<Computer[]>('GetComputers', {});
    const targetComputer = allComputers[0];

    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: `Name==${targetComputer.Name}`,
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c => c.Name === targetComputer.Name)).toBe(true);
  });

  test('should filter computers by name contains', async () => {
    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: 'Name@=*Dell*',
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c => c.Name.toLowerCase().includes('dell'))).toBe(true);
  });

  test('should filter computers by price greater than', async () => {
    const minPrice = 1000;
    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: `Price>${minPrice}`,
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c => c.Price > minPrice)).toBe(true);
  });

  test('should filter computers by price less than', async () => {
    const maxPrice = 2000;
    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: `Price<${maxPrice}`,
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c => c.Price < maxPrice)).toBe(true);
  });

  test('should filter computers by price range', async () => {
    const minPrice = 1000;
    const maxPrice = 3000;
    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: `Price>${minPrice},Price<${maxPrice}`,
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c => c.Price > minPrice && c.Price < maxPrice)).toBe(true);
  });
});

describe('Computer Store API - Sorting', () => {
  test('should sort computers by price ascending', async () => {
    const computers = await apiPost<Computer[]>('GetComputers', {
      Sorts: 'Price',
      PageSize: 50,
    });

    expect(computers.length).toBeGreaterThan(1);

    for (let i = 1; i < computers.length; i++) {
      expect(computers[i].Price).toBeGreaterThanOrEqual(computers[i - 1].Price);
    }
  });

  test('should sort computers by price descending', async () => {
    const computers = await apiPost<Computer[]>('GetComputers', {
      Sorts: '-Price',
      PageSize: 50,
    });

    expect(computers.length).toBeGreaterThan(1);

    for (let i = 1; i < computers.length; i++) {
      expect(computers[i].Price).toBeLessThanOrEqual(computers[i - 1].Price);
    }
  });

  test('should sort computers by name', async () => {
    const computers = await apiPost<Computer[]>('GetComputers', {
      Sorts: 'Name',
      PageSize: 50,
    });

    expect(computers.length).toBeGreaterThan(1);

    for (let i = 1; i < computers.length; i++) {
      expect(computers[i].Name.localeCompare(computers[i - 1].Name)).toBeGreaterThanOrEqual(0);
    }
  });
});

describe('Computer Store API - Pagination', () => {
  test('should paginate computers', async () => {
    const page1 = await apiPost<Computer[]>('GetComputers', {
      Page: 1,
      PageSize: 5,
    });

    const page2 = await apiPost<Computer[]>('GetComputers', {
      Page: 2,
      PageSize: 5,
    });

    expect(page1.length).toBeLessThanOrEqual(5);
    expect(page2.length).toBeLessThanOrEqual(5);

    // Ensure different results on different pages
    if (page1.length > 0 && page2.length > 0) {
      expect(page1[0].Id).not.toBe(page2[0].Id);
    }
  });

  test('should respect custom page size', async () => {
    const pageSize = 3;
    const computers = await apiPost<Computer[]>('GetComputers', {
      Page: 1,
      PageSize: pageSize,
    });

    expect(computers.length).toBeLessThanOrEqual(pageSize);
  });
});

describe('Computer Store API - Combined Filters', () => {
  test('should combine filter, sort, and pagination', async () => {
    const minPrice = 1000;
    const computers = await apiPost<Computer[]>('GetComputers', {
      Filters: `Price>${minPrice}`,
      Sorts: '-Price',
      Page: 1,
      PageSize: 5,
    });

    expect(computers.length).toBeGreaterThan(0);
    expect(computers.length).toBeLessThanOrEqual(5);
    expect(computers.every(c => c.Price > minPrice)).toBe(true);

    // Verify sorting
    for (let i = 1; i < computers.length; i++) {
      expect(computers[i].Price).toBeLessThanOrEqual(computers[i - 1].Price);
    }
  });

  test('should handle multiple filters with OR operator', async () => {
    const filtered = await apiPost<Computer[]>('GetComputers', {
      Filters: 'Name@=*Dell*|Name@=*HP*',
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(c =>
      c.Name.toLowerCase().includes('dell') ||
      c.Name.toLowerCase().includes('hp')
    )).toBe(true);
  });
});

describe('Computer Store API - Brands', () => {
  test('should get all brands', async () => {
    const brands = await apiPost<Brand[]>('GetBrands', {});

    expect(brands).toBeDefined();
    expect(Array.isArray(brands)).toBe(true);
    expect(brands.length).toBeGreaterThan(0);
  });

  test('should filter brands by name', async () => {
    const allBrands = await apiPost<Brand[]>('GetBrands', {});
    const targetBrand = allBrands[0];

    const filtered = await apiPost<Brand[]>('GetBrands', {
      Filters: `Name==${targetBrand.Name}`,
    });

    expect(filtered.length).toBeGreaterThan(0);
    expect(filtered.every(b => b.Name === targetBrand.Name)).toBe(true);
  });
});

describe('Computer Store API - Categories', () => {
  test('should get all categories', async () => {
    const categories = await apiPost<Category[]>('GetCategories', {});

    expect(categories).toBeDefined();
    expect(Array.isArray(categories)).toBe(true);
    expect(categories.length).toBeGreaterThan(0);
  });

  test('should filter categories by name contains', async () => {
    const allCategories = await apiPost<Category[]>('GetCategories', {});
    const targetName = allCategories[0].Name.substring(0, 3);

    const filtered = await apiPost<Category[]>('GetCategories', {
      Filters: `Name@=*${targetName}*`,
    });

    expect(filtered.length).toBeGreaterThan(0);
  });
});

describe('Computer Store API - Error Handling', () => {
  test('should handle invalid filter syntax gracefully', async () => {
    await expect(
      apiPost<Computer[]>('GetComputers', {
        Filters: 'InvalidSyntax!!!',
      })
    ).rejects.toThrow();
  });

  test('should handle invalid sort field', async () => {
    // This might not throw depending on server implementation
    // but should at least return results without error
    const computers = await apiPost<Computer[]>('GetComputers', {
      Sorts: 'NonExistentField',
    });

    expect(computers).toBeDefined();
  });
});

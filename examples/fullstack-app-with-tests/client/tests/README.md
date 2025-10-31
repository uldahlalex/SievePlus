# Integration Tests

TypeScript integration tests for the Computer Store API.

## Prerequisites

1. **Start the API server** in a separate terminal:
   ```bash
   cd ../server/api
   dotnet run
   ```

   The API should be running at `http://localhost:5284` (or configure via `API_BASE_URL` environment variable).

## Running Tests

From the `client` directory:

```bash
# Run all tests once
npm test

# Run tests in watch mode (re-runs on file changes)
npm run test:watch

# Run tests with UI (interactive browser interface)
npm run test:ui
```

## Writing Tests

Tests are located in `client/tests/*.test.ts` and use:
- **Vitest** as the test runner
- **Generated TypeScript client** from `src/generated-client.ts` (same client used by the React app)

Example test structure:

```typescript
import { describe, it, expect, beforeAll } from 'vitest';
import { ComputerStoreClient, SievePlusRequestOfComputerQueryModel } from '../src/generated-client';

describe('My Test Suite', () => {
  let client: ComputerStoreClient;
  const baseUrl = process.env.API_BASE_URL || 'http://localhost:5284';

  beforeAll(() => {
    const http = { fetch: (url: RequestInfo, init?: RequestInit) => fetch(url, init) };
    client = new ComputerStoreClient(baseUrl, http);
  });

  it('should test something', async () => {
    const request: SievePlusRequestOfComputerQueryModel = {
      filters: {},
      sorts: [],
      page: 1,
      pageSize: 10
    };

    const result = await client.getComputers(request);
    expect(result).toBeDefined();
  });
});
```

## Configuration

- **vitest.config.ts**: Test runner configuration
- **API_BASE_URL**: Set environment variable to override default API URL
  ```bash
  API_BASE_URL=http://localhost:8080 npm test
  ```

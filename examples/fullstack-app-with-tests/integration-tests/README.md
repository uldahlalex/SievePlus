# Integration Tests

This directory contains TypeScript-based integration tests that run against a real Kestrel server.

## Architecture

The test setup consists of two parts:

1. **.NET Fixture** (`fixture/` directory): A console application that uses `WebApplicationFactory` to start a real Kestrel server programmatically.

2. **TypeScript Tests**: Vitest-based tests that send HTTP requests to the running API server.

## How It Works

1. When you run the tests, Vitest executes the global setup script (`global-setup.ts`)
2. The setup script spawns the .NET fixture process which starts Kestrel on port 5555
3. The setup script waits for the server to be ready (by polling the Swagger endpoint)
4. All tests run against the live server at `http://localhost:5555`
5. After tests complete, the teardown script stops the server gracefully

## Benefits

- **Real server testing**: Tests routing, model binding, middleware, and the full request pipeline
- **Type-safe tests**: Write tests in TypeScript with full type checking
- **Fast feedback**: Integration tests run quickly with a real in-memory database (Testcontainers)
- **Isolated environment**: Each test run gets a fresh database via Testcontainers

## Setup

Install dependencies:

```bash
npm install
```

## Running Tests

Run all tests once:

```bash
npm test
```

Run tests in watch mode (re-runs on file changes):

```bash
npm run test:watch
```

Run tests with Vitest UI:

```bash
npm run test:ui
```

## Writing Tests

Tests are located in `*.test.ts` files. Example:

```typescript
import { describe, test, expect } from 'vitest';

const API_URL = process.env.API_URL || 'http://localhost:5555';

describe('My API Tests', () => {
  test('should do something', async () => {
    const response = await fetch(`${API_URL}/api/MyEndpoint`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ /* request */ }),
    });

    const data = await response.json();
    expect(data).toBeDefined();
  });
});
```

## Configuration

- **Port**: The API server runs on port 5555 by default (configurable in `global-setup.ts`)
- **Timeouts**: Test timeout is 30s, hook timeout is 60s (configurable in `vitest.config.ts`)
- **Environment**: The fixture runs in Development mode, using Testcontainers for PostgreSQL

## Troubleshooting

**Server won't start:**
- Check if port 5555 is already in use
- Ensure .NET 9 SDK is installed
- Check the fixture project builds: `cd fixture && dotnet build`

**Tests timeout:**
- Increase timeout in `vitest.config.ts`
- Check server logs in the test output

**Database issues:**
- The server uses Testcontainers which requires Docker
- Ensure Docker is running and accessible

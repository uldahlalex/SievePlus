import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    globalSetup: './global-setup.ts',
    testTimeout: 30000,
    hookTimeout: 60000,
  },
});

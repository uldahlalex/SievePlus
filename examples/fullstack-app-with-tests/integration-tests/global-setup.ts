import { spawn, ChildProcess } from 'child_process';
import { resolve } from 'path';

let serverProcess: ChildProcess | null = null;
const API_PORT = 5555;
const API_URL = `http://localhost:${API_PORT}`;

async function waitForServer(url: string, maxAttempts = 60, delay = 1000): Promise<void> {
  console.log('Waiting for server to be ready...');
  for (let i = 0; i < maxAttempts; i++) {
    try {
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), delay);

      const response = await fetch(`${url}/swagger/v1/swagger.json`, {
        signal: controller.signal,
      });
      clearTimeout(timeoutId);

      if (response.ok) {
        console.log('✓ API server is ready');
        return;
      }
    } catch (error) {
      // Server not ready yet, continue waiting
    }
    await new Promise(resolve => setTimeout(resolve, delay));
  }
  throw new Error(`Server did not start within ${maxAttempts * delay}ms`);
}

export async function setup() {
  console.log('Starting API server for integration tests...');

  const fixturePath = resolve(import.meta.dirname, './fixture');

  // Start the .NET fixture which hosts the Kestrel server
  serverProcess = spawn('dotnet', ['run', '--', API_PORT.toString()], {
    cwd: fixturePath,
    stdio: ['ignore', 'pipe', 'pipe'],
  });

  // Log server output for debugging
  serverProcess.stdout?.on('data', (data) => {
    console.log(`[API Server] ${data.toString().trim()}`);
  });

  serverProcess.stderr?.on('data', (data) => {
    console.error(`[API Server Error] ${data.toString().trim()}`);
  });

  serverProcess.on('error', (error) => {
    console.error('Failed to start API server:', error);
  });

  // Wait for the server to be ready
  await waitForServer(API_URL);

  // Make the API URL available to tests
  process.env.API_URL = API_URL;
}

export async function teardown() {
  console.log('Stopping API server...');

  if (serverProcess) {
    serverProcess.kill('SIGINT');

    // Wait for graceful shutdown
    await new Promise<void>((resolve) => {
      serverProcess!.on('exit', () => {
        console.log('✓ API server stopped');
        resolve();
      });

      // Force kill after 5 seconds if not stopped
      setTimeout(() => {
        if (serverProcess && !serverProcess.killed) {
          serverProcess.kill('SIGKILL');
          resolve();
        }
      }, 5000);
    });

    serverProcess = null;
  }
}

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { FullConfig } from '@playwright/test';

/**
 * Fails the run at once with a clear message when the E2E stack is not running,
 * instead of letting every test time out on its own.
 *
 * The API is checked on its own host port (E2E_API_URL, the API origin without /api):
 * /health/ready answers 200 only when the API reaches its database, and the client's nginx
 * forwards only /api/. The tests themselves still call the API through the client.
 */
export default async function globalSetup(config: FullConfig): Promise<void> {
  const clientUrl = config.projects[0]?.use.baseURL ?? 'http://localhost:4200';
  const apiUrl = process.env['E2E_API_URL'] ?? 'http://localhost:5000';

  for (const url of [clientUrl, new URL('/health/ready', apiUrl).href]) {
    try {
      const response = await fetch(url, { signal: AbortSignal.timeout(5000) });
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }
    } catch (error) {
      throw new Error(
        `E2E stack not reachable at ${url} (${(error as Error).message}). ` +
          'Start it from e2e/ with "npm run stack:up" (stop the dev stack first: same ports).',
      );
    }
  }
}

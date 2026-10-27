// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { FullConfig } from '@playwright/test';

/**
 * Fails the run at once with a clear message when the E2E stack is not running,
 * instead of letting every test time out on its own.
 *
 * The client bundle calls the API on its own origin (/api, forwarded by the client's
 * nginx), so the API is checked through the client by default.
 */
export default async function globalSetup(config: FullConfig): Promise<void> {
  const clientUrl = config.projects[0]?.use.baseURL ?? 'http://localhost:4200';
  const apiUrl = process.env['E2E_API_URL'] ?? `${clientUrl}/api`;

  for (const url of [clientUrl, `${apiUrl}/Health`]) {
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

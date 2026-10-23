// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright runs against the docker-compose E2E stack (docker-compose.e2e.yml, TESTING §3.3).
 * Start it with `npm run stack:up`; `global-setup.ts` stops the run early if it is not reachable.
 *
 * Browsers: Chromium, Firefox and WebKit on desktop plus one Android and one iOS viewport
 * (SPEC §7.4, CLAUDE.md §9).
 */
export default defineConfig({
  testDir: '.',
  testMatch: '**/*.spec.ts',
  testIgnore: '**/node_modules/**',
  globalSetup: './support/global-setup.ts',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200',
    locale: 'de-DE',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
    { name: 'mobile-chrome', use: { ...devices['Pixel 7'] } },
    { name: 'mobile-safari', use: { ...devices['iPhone 15'] } },
  ],
});

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
 *
 * Firefox runs the organisation specs one at a time in the project `firefox-organization`, which
 * `firefox` depends on, so `--project=firefox` runs them first and then the rest in parallel: two
 * S21 tests at once crashed Firefox's page process in axe (#337, TESTING V228).
 */
const ORGANIZATION_SPECS = '**/organization/*.spec.ts';
const NODE_MODULES = '**/node_modules/**';

export default defineConfig({
  testDir: '.',
  testMatch: '**/*.spec.ts',
  testIgnore: NODE_MODULES,
  globalSetup: './support/global-setup.ts',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: 0,
  // In CI (GitHub Actions) the `github` reporter adds failures as annotations to the pull request.
  reporter: process.env['CI']
    ? [['github'], ['list'], ['html', { open: 'never' }]]
    : [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200',
    locale: 'de-DE',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox-organization', testMatch: ORGANIZATION_SPECS, workers: 1, use: { ...devices['Desktop Firefox'] } },
    {
      name: 'firefox',
      testIgnore: [NODE_MODULES, ORGANIZATION_SPECS],
      dependencies: ['firefox-organization'],
      use: { ...devices['Desktop Firefox'] },
    },
    { name: 'webkit', use: { ...devices['Desktop Safari'] } },
    { name: 'mobile-chrome', use: { ...devices['Pixel 7'] } },
    { name: 'mobile-safari', use: { ...devices['iPhone 15'] } },
  ],
});

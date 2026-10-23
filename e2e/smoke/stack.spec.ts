// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';

/**
 * Smoke test of the E2E stack: the client is served, its German texts load, and data comes
 * from the API and the seeded database. Journeys (J1 from S1-19 on) live in e2e/journeys/.
 */
test.describe('E2E stack', () => {
  test('serves the client with its German texts', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveTitle('ProcureAccess');
    await expect(page.getByText('Willkommen zurück.')).toBeVisible();
  });

  test('lists the seeded products from the API', async ({ page }) => {
    await page.goto('/products');

    await expect(page.getByRole('cell', { name: 'CoreFlow', exact: true })).toBeVisible();
  });
});

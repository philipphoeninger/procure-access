// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';

/**
 * Smoke test of the test catalog release (S4-08, #241): the API seeds and activates the MiniCatalog
 * release T.1 when the stack starts (docker-compose.e2e.yml), so both catalog endpoints answer 200
 * through the client's nginx. The configurator (S10) and the export build on this.
 */
test.describe('E2E stack catalog', () => {
  test('answers the filter options of the active test release', async ({ request }) => {
    const response = await request.get('/api/v1/catalog/filter-options');

    expect(response.status()).toBe(200);
    const body = (await response.json()) as { release: string; layers: { useCases: unknown[] } };
    expect(body.release).toBe('T.1');
    expect(body.layers.useCases.length).toBeGreaterThan(0);
  });

  test('answers the criteria of a selection on the active test release', async ({ request }) => {
    // A complete selection of T.1 that no exclusion rule of T.1 refuses; base is always selected.
    const query = new URLSearchParams([
      ['useCase', 'hr'],
      ['appTypeIds[]', 'web'],
      ['productPartIds[]', 'base'],
      ['testTypeIds[]', 'softwareTest'],
    ]);

    const response = await request.get(`/api/v1/catalog/criteria?${query.toString()}`);

    expect(response.status()).toBe(200);
    const body = (await response.json()) as { release: string; criteria: { key: string }[] };
    expect(body.release).toBe('T.1');
    expect(body.criteria.map((c) => c.key)).toContain('PROC-softwareTest');
  });
});

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { expect, test, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { recordThirdPartyRequests, thirdPartyLinksInDocument } from '../support/third-party-requests';

/**
 * J1 in its current form (TESTING §5.4, §9 step 0): characterisation of today's criteria filter
 * on `/filters`, with axe on every state (A11Y-01). Step 2 replaces it with the anonymous part of
 * J1 (rationale, priority change, exclusion of a criterion, XLSX export); the export is left out
 * here because today's button downloads the page, not data (S1-06 K2).
 *
 * The expected criteria are the CAT-10 API fixture, case `j1-like-selection`, so API and E2E
 * characterisation share one recording. J1 adds a second app type (Website / Webanwendung): in the
 * seed, app types carry no criteria (S1-15 F3), so the recorded set stays the same.
 */

interface CriteriaFilterCase {
  readonly name: string;
  readonly criteriaFilterIds: readonly number[];
  readonly expected: readonly { readonly id: number; readonly name: string; readonly criteriaFilterId: number }[];
}

const cat10Fixture = JSON.parse(
  readFileSync(
    join(__dirname, '..', '..', 'tests', 'ProcureAccess.Tests.Integration', 'Catalog', 'Fixtures', 'cat-10-criteria-filter.json'),
    'utf-8',
  ),
) as { readonly cases: readonly CriteriaFilterCase[] };

const recorded = cat10Fixture.cases.find((entry) => entry.name === 'j1-like-selection');
if (!recorded) {
  throw new Error('CAT-10 fixture has no case "j1-like-selection"');
}

/** Seed ids (migration AddSeedData) of the options J1 selects. */
const PRODUCT_TYPE = { id: 17, name: 'HR-Anwendung' };
const APP_TYPES = [
  { id: 8, name: 'Website / Webanwendung' },
  { id: 10, name: 'Software (offen)' },
];
const PRODUCT_PARTS = [
  { id: 1, name: 'Basisfunktionalität' },
  { id: 3, name: 'Video' },
  { id: 4, name: 'Zweiwege-Sprachkommunikation' },
];
const TEST_TYPE = { id: 23, name: 'BIT inklusiv Software-Test' };

/** Options the seed exclusions switch off once HR-Anwendung, then Website / Webanwendung is chosen. */
const EXCLUDED_BY_PRODUCT_TYPE = [
  'Dokument',
  'Mobile Anwendung (offen)',
  'Mobile Anwendung (geschlossen)',
  'Hardware',
  'Vermittlungs-/Notrufdienst',
  'BIT inklusiv PDF-Test',
];
const EXCLUDED_BY_WEBSITE = ['BIT inklusiv App-Test', 'BIK BITV-Test (App)'];

const recordedNames = recorded.expected.map((criterion) => criterion.name).sort();

const byNumber = (a: number, b: number) => a - b;

function option(page: Page, name: string) {
  return page.locator('pa-filters-selection').getByRole('option', { name, exact: true });
}

test.describe('J1 configurator without account (current form)', () => {
  test('INV:CAT-10 INV:A11Y-01 anonymous visitor filters criteria and gets the recorded list', async ({ page, baseURL }, testInfo) => {
    // Seven axe runs take 1–3 s each; under parallel load (Firefox) the default 30 s is too short.
    test.slow();
    // CLAUDE §8: fonts, icons and the API all come from the app's own origin (#43).
    const appOrigin = new URL(baseURL!).origin;
    const thirdPartyRequests = recordThirdPartyRequests(page, appOrigin);

    await test.step('open the criteria filter', async () => {
      await page.goto('/filters');
      await expect(page.getByRole('button', { name: 'Produkt-Typen' })).toBeVisible();
      // The header icons are drawn with the self-hosted Material Symbols font (design handoff §1.8).
      const iconFontLoaded = () =>
        page.evaluate(() =>
          [...document.fonts].some(
            (face) => face.family.replaceAll('"', '') === 'Material Symbols Outlined' && face.status === 'loaded',
          ),
        );
      await expect.poll(iconFontLoaded).toBe(true);
      await expectNoAxeViolations(page, testInfo, 'filters loaded');
    });

    await test.step('expand all filter layers', async () => {
      await page.getByRole('button', { name: 'Alle ausklappen' }).click();
      for (const layer of ['Produkt-Typen', 'App-Typen', 'Produkt-Teile', 'Test-Typen']) {
        await expect(page.getByRole('button', { name: layer })).toHaveAttribute('aria-expanded', 'true');
      }
      await expect(option(page, TEST_TYPE.name)).toBeVisible();
      await expectNoAxeViolations(page, testInfo, 'all layers expanded');
    });

    await test.step('choose the product type in the autocomplete', async () => {
      // The product type replaces the whole selection, so it is chosen first. Focus opens the
      // panel; a tap on mobile lands on the floating label above the input.
      await page.getByRole('combobox').focus();
      const productType = page.getByRole('listbox').getByRole('option', { name: PRODUCT_TYPE.name, exact: true });
      await expect(productType).toBeVisible();
      await expectNoAxeViolations(page, testInfo, 'product type autocomplete open');

      await productType.click();
      await expect(page.getByRole('combobox')).toHaveValue(PRODUCT_TYPE.name);
      await expect(page.getByText('1 ausgewählt')).toBeVisible();
      for (const name of EXCLUDED_BY_PRODUCT_TYPE) {
        await expect(option(page, name), `${name} is excluded by ${PRODUCT_TYPE.name}`).toBeDisabled();
      }
      await expectNoAxeViolations(page, testInfo, 'product type chosen, excluded options disabled');
    });

    await test.step('choose two app types, three product parts and one test type', async () => {
      for (const { name } of [...APP_TYPES, ...PRODUCT_PARTS, TEST_TYPE]) {
        await option(page, name).click();
        await expect(option(page, name)).toHaveAttribute('aria-selected', 'true');
      }
      await expect(page.getByText('7 ausgewählt')).toBeVisible();
      for (const name of [...EXCLUDED_BY_PRODUCT_TYPE, ...EXCLUDED_BY_WEBSITE]) {
        await expect(option(page, name), `${name} is excluded`).toBeDisabled();
      }
      await expectNoAxeViolations(page, testInfo, 'selection complete');
    });

    await test.step('confirm the selection and get the recorded criteria', async () => {
      const request = page.waitForRequest((candidate) => candidate.url().includes('/Criteria/byCriteriaFilterIds'));
      await page.getByRole('button', { name: 'Enter' }).click();

      // The client sends every marked filter id of all four layers (S1-15 F2).
      const sentIds = new URL((await request).url()).searchParams.getAll('criteriaFilterIds').map(Number);
      expect([...sentIds].sort(byNumber)).toEqual(
        [PRODUCT_TYPE, ...APP_TYPES, ...PRODUCT_PARTS, TEST_TYPE].map((entry) => entry.id).sort(byNumber),
      );

      const titles = page.locator('pa-criteria-list mat-expansion-panel-header mat-panel-title');
      await expect(titles).toHaveCount(recorded.expected.length);
      const shown = (await titles.allTextContents()).map((title) => title.trim());
      expect([...shown].sort()).toEqual(recordedNames);
      await expectNoAxeViolations(page, testInfo, 'criteria list');
    });

    await test.step('open one criterion', async () => {
      const first = recordedNames[0];
      const header = page.locator('pa-criteria-list').getByRole('button', { name: first });
      await header.click();
      await expect(header).toHaveAttribute('aria-expanded', 'true');
      // Today every criterion shows the same fixed priority; step 2 replaces it (CAT-02).
      await expect(header).toContainText('Priorität: 1');
      await expectNoAxeViolations(page, testInfo, 'criterion expanded');
    });

    await test.step('no request and no link went to a third-party origin', async () => {
      expect(thirdPartyRequests).toEqual([]);
      expect(await thirdPartyLinksInDocument(page, appOrigin)).toEqual([]);
    });
  });

  test('INV:A11Y-01 confirming without a selection shows the empty-list message', async ({ page }, testInfo) => {
    // S1-15 P3: the API returns [] for an empty selection; the client shows its empty-list text.
    await page.goto('/filters');
    await expect(page.getByRole('button', { name: 'Produkt-Typen' })).toBeVisible();
    await expect(page.getByText('0 ausgewählt')).toBeVisible();

    await page.getByRole('button', { name: 'Enter' }).click();

    await expect(page.getByText('Keine Kriterien gefunden. Bitte versuchen Sie es erneut...')).toBeVisible();
    await expectNoAxeViolations(page, testInfo, 'empty criteria list');
  });
});

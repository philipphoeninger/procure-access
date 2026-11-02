// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * Route change (S2-16, R-SHL-04): page title most-specific-first, focus on the H1 and the title
 * announced through `A11yAnnouncerService` (A11Y-03); `<html lang>` follows the UI language
 * (A11Y-05). Starts on a legal page: they have an H1 and fit 360 px (see shell.spec.ts).
 */
const START_PAGE = '/barrierefreiheit';

test.describe('Route change', () => {
  test('INV:A11Y-03 title, H1 focus and announcement after following a link', async ({ page }, testInfo) => {
    await page.goto(START_PAGE);
    await expect(page).toHaveTitle('Erklärung zur Barrierefreiheit – ProcureAccess');
    // The first page load moves no focus: the skip link stays the first stop of the Tab key.
    await expect(page.getByRole('heading', { level: 1 })).not.toBeFocused();

    await page.getByRole('contentinfo').getByRole('link', { name: 'Impressum' }).click();

    await expect(page).toHaveURL(/\/impressum$/);
    await expect(page).toHaveTitle('Impressum – ProcureAccess');
    await expect(page.getByRole('heading', { level: 1, name: 'Impressum' })).toBeFocused();
    // CDK LiveAnnouncer's live region, the only one A11yAnnouncerService writes to.
    await expect(page.locator('.cdk-live-announcer-element')).toHaveText('Impressum – ProcureAccess');
    await expectNoAxeViolations(page, testInfo, 'imprint after route change, H1 focused');
  });

  test('INV:A11Y-05 html lang follows the UI language', async ({ page }) => {
    await page.goto(START_PAGE);
    await expect(page.locator('html')).toHaveAttribute('lang', 'de');

    // The legacy settings page (S60 rebuilt in step 3) is the only language switch so far.
    await page.goto('/settings');
    await expect(page).toHaveTitle('Einstellungen – ProcureAccess');
    await page.locator('#language-select').click();
    await page.getByRole('option', { name: 'English' }).click();

    await expect(page.locator('html')).toHaveAttribute('lang', 'en');
  });
});

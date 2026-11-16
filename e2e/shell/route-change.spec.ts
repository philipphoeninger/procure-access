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
  test('INV:A11Y-03 INV:A11Y-08 title, H1 focus and announcement after following a link', async ({ page }, testInfo) => {
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

  // German is the only UI language of v1 (SPEC R-SET-01, SD12): the legacy language switch went
  // with its settings page in S5-25b (owner decision S5-25 2a). That <html lang> follows a change of
  // the language is checked in document-language.spec.ts.
  test('INV:A11Y-05 html lang is the UI language', async ({ page }) => {
    await page.goto(START_PAGE);
    await expect(page.locator('html')).toHaveAttribute('lang', 'de');
  });
});

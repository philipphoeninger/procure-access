// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * S10 reset notice, „Bisherige Auswahl“ and the hand-over to S11 (S4-12; R-CON-07, R-CON-08,
 * R-CON-10) on the active test release T.1 of the E2E stack, by keyboard, with axe in both
 * appearances (A11Y-01, A11Y-08). Also the switch of O-S4-3: without an active release the
 * criteria lead to the legacy filter.
 */
const WIDE = { width: 1280, height: 800 };
const FILTER_OPTIONS = '**/api/v1/catalog/filter-options*';

const checkbox = (page: Page, name: string) => page.getByRole('checkbox', { name, exact: true });
const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });
const next = (page: Page) => page.getByRole('button', { name: 'Weiter' });
const change = (page: Page, description: string) =>
  page.getByRole('button', { name: 'Ändern', description, exact: true });

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Types into the combobox and takes the first matching use case by keyboard. */
async function chooseUseCase(page: Page, typed: string, name: string): Promise<void> {
  await combobox(page).focus();
  await page.keyboard.type(typed);
  // ArrowDown takes the first option: wait until the list is filtered.
  await expect(page.getByRole('listbox').getByRole('option').first()).toHaveAccessibleName(name);
  await page.keyboard.press('ArrowDown');
  await page.keyboard.press('Enter');
  await expect(combobox(page)).toHaveValue(name);
}

async function pressSpace(box: Locator): Promise<void> {
  await box.focus();
  await box.press('Space');
  await expect(box).toBeChecked();
}

/** Steps 1–3 with HR-Anwendung, Website oder Webanwendung and Audio; ends on step 4. */
async function toStep4(page: Page): Promise<void> {
  await page.setViewportSize(WIDE);
  await page.goto('/kriterien');
  await chooseUseCase(page, 'HR', 'HR-Anwendung');
  await next(page).click();
  await pressSpace(checkbox(page, 'Website oder Webanwendung'));
  await next(page).click();
  await pressSpace(checkbox(page, 'Audio'));
  await next(page).click();
  await expect(page.getByRole('heading', { level: 2, name: /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/ })).toBeFocused();
}

test.describe('S10 reset, „Bisherige Auswahl“ and „Kriterienliste anzeigen“', () => {
  test('INV:CFG-01 INV:A11Y-01 INV:A11Y-08 changing the use case via „Ändern“ clears steps 2–4 and says so in a status region', async ({ page }, testInfo) => {
    await toStep4(page);
    await pressSpace(checkbox(page, 'BIK BITV-Test (Web)'));
    await expect(page).toHaveTitle('Kriterien zusammenstellen – Schritt 4 von 4 – ProcureAccess');

    // „Bisherige Auswahl“ names the three earlier steps, each with its „Ändern“ (R-CON-08).
    await expect(page.getByRole('button', { name: 'Ändern' })).toHaveCount(3);
    await expect(change(page, 'Funktionsbereiche: Basisfunktionalität und Audio')).toBeVisible();
    await expectNoAxeViolations(page, testInfo, 's10 summary');

    await change(page, 'Verwendungszweck: HR-Anwendung').click();
    await expect(page.getByRole('heading', { level: 2, name: 'Wofür setzen Sie das Produkt ein?' })).toBeFocused();
    await combobox(page).fill('');
    await chooseUseCase(page, 'Report', 'Reporting-Anwendung');

    const status = page.getByRole('main').getByRole('status').filter({ hasText: 'Auswahl zurückgesetzt' });
    await expect(status).toContainText(
      'Sie haben den Verwendungszweck geändert. Ihre Auswahl bei Anwendungstyp, Funktionsbereichen und Prüfverfahren wurde deshalb zurückgesetzt.',
    );
    // The focus stays where the user is (R-CON-07).
    await expect(combobox(page)).toBeFocused();
    await settled(page.getByRole('main'));
    await expectNoAxeViolations(page, testInfo, 's10 reset');

    await next(page).click();
    await expect(status).toBeVisible();
    await expect(checkbox(page, 'Website oder Webanwendung')).not.toBeChecked();
    await pressSpace(checkbox(page, 'Website oder Webanwendung'));
    await expect(status).toBeHidden();
    await next(page).click();
    await expect(checkbox(page, 'Basisfunktionalität')).toBeChecked();
    await expect(checkbox(page, 'Audio')).not.toBeChecked();
  });

  test('INV:CFG-04 INV:A11Y-01 INV:A11Y-08 „Kriterienliste anzeigen“ works without a test type and opens S11', async ({ page }, testInfo) => {
    await toStep4(page);

    const submit = page.getByRole('button', { name: 'Kriterienliste anzeigen' });
    await expect(submit).not.toHaveAttribute('aria-disabled');
    await submit.focus();
    await page.keyboard.press('Enter');

    await expect(page).toHaveURL(/\/kriterien\/ergebnis$/);
    await expect(page).toHaveTitle('Ihre Kriterienliste – ProcureAccess');
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 's11 placeholder');

    await page.getByRole('link', { name: 'Auswahl ändern' }).click();
    await expect(page).toHaveURL(/\/kriterien$/);
    await expect(combobox(page)).toHaveValue('HR-Anwendung');
  });

  test('S11 without a selection, e.g. after a reload, leads back to S10 (R-CON-09)', async ({ page }) => {
    await page.goto('/kriterien/ergebnis');

    await expect(page).toHaveURL(/\/kriterien$/);
    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeVisible();
  });
});

test.describe('S10 or the legacy filter (O-S4-3)', () => {
  test('start page and header lead to S10 while a release is active', async ({ page }) => {
    await page.setViewportSize(WIDE);
    await page.goto('/');

    await page.getByRole('main').getByRole('link', { name: 'Kriterien zusammenstellen' }).click();
    await expect(page).toHaveURL(/\/kriterien$/);
    const criteria = page.getByRole('navigation', { name: 'Hauptnavigation' }).getByRole('link', { name: 'Kriterien' });
    await expect(criteria).toHaveAttribute('aria-current', 'page');
  });

  test('without an active release they lead to the legacy filter, without an error message', async ({ page }) => {
    await page.setViewportSize(WIDE);
    await page.route(FILTER_OPTIONS, (route) =>
      route.fulfill({
        status: 404,
        contentType: 'application/problem+json',
        body: JSON.stringify({ type: 'https://procureaccess.de/problems/not-found', status: 404 }),
      }),
    );
    await page.goto('/');

    await page.getByRole('main').getByRole('link', { name: 'Kriterien zusammenstellen' }).click();
    await expect(page).toHaveURL(/\/filters$/);
    await expect(page.getByRole('navigation', { name: 'Hauptnavigation' }).getByRole('link', { name: 'Kriterien' }))
      .toHaveAttribute('aria-current', 'page');
    // The 404 is the switch, not an error: no snackbar.
    await expect(page.locator('mat-snack-bar-container')).toHaveCount(0);

    await page.goto('/kriterien');
    await expect(page).toHaveURL(/\/filters$/);
  });
});

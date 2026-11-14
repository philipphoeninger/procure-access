// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { chooseFirstOption } from '../support/combobox';
import { expireSessionOnce, fakeSession } from '../support/session';

/**
 * Sign-in S01, the login overlay and sign-out (S5-09; R-AUTH-03, R-AUTH-07 base, R-AUTH-08) on
 * the E2E stack with the test release T.1, by keyboard. The session is faked
 * (`support/session.ts`) until the E2E users of S5-14 exist; everything else is the real stack.
 * Axe in both appearances on each new state (A11Y-01, A11Y-08).
 */
const WIDE = { width: 1280, height: 800 };

const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });
const header = (page: Page) => page.getByRole('banner');
const main = (page: Page) => page.getByRole('main');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Focuses a control and presses a key, as a keyboard user does after reaching it. */
async function press(control: Locator, key: string): Promise<void> {
  await control.focus();
  await control.press(key);
}

/** Types the address and the password and submits with Enter in the password field. */
async function signIn(scope: Locator): Promise<void> {
  await scope.getByLabel('E-Mail-Adresse').fill('e2e@example.test');
  await scope.getByLabel('Passwort', { exact: true }).fill('ein-passwort');
  await scope.getByLabel('Passwort', { exact: true }).press('Enter');
}

/** „Weiter“, then waits for the focus on the heading of the next step. */
async function next(page: Page, heading: string | RegExp): Promise<void> {
  await press(main(page).getByRole('button', { name: 'Weiter' }), 'Enter');
  await expect(main(page).getByRole('heading', { level: 2, name: heading })).toBeFocused();
}

/** S10 with HR-Anwendung and Website oder Webanwendung, on step 4. */
async function toStep4(page: Page): Promise<void> {
  await chooseHr(page);
  await next(page, 'Was möchten Sie beschaffen?');
  await press(main(page).getByRole('checkbox', { name: 'Website oder Webanwendung', exact: true }), 'Space');
  await next(page, 'Welche Funktionen hat das Produkt?');
  await next(page, /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/);
}

/** S10 step 1 with HR-Anwendung chosen. */
async function chooseHr(page: Page): Promise<void> {
  await page.goto('/kriterien');
  await combobox(page).focus();
  await page.keyboard.type('HR');
  await chooseFirstOption(page, combobox(page), 'HR-Anwendung');
}

test.describe('S01 „Anmelden“, login overlay and sign-out', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 INV:A11Y-08 „Anmelden“ opens S01 over the page; an empty submit shows the error summary', async ({ page }, testInfo) => {
    await fakeSession(page);
    await chooseHr(page);

    await press(header(page).getByRole('button', { name: 'Anmelden' }), 'Enter');

    await expect(page).toHaveURL(/\/kriterien\(login:anmelden\)$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Anmelden' })).toBeFocused();
    await expect(page).toHaveTitle('Anmelden – ProcureAccess');
    // S10 stays alive underneath, hidden (R-AUTH-03).
    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeHidden();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's01');

    await press(main(page).getByRole('button', { name: 'Anmelden' }), 'Enter');

    const summary = main(page).locator('pa-error-summary pa-notice');
    await expect(summary).toBeFocused();
    await expect(summary).toContainText('2 Angaben müssen noch korrigiert werden');
    await expect(page).toHaveTitle('Fehler: Anmelden – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's01 error summary');

    await press(summary.getByRole('link', { name: 'E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an.' }), 'Enter');
    await expect(main(page).getByLabel('E-Mail-Adresse')).toBeFocused();
  });

  test('a refused sign-in names no field', async ({ page }) => {
    await fakeSession(page);
    await page.route('**/api/v1/auth/login', (route) =>
      route.fulfill({
        status: 400,
        contentType: 'application/problem+json',
        body: JSON.stringify({ type: 'https://procureaccess.de/problems/validation', status: 400, errors: { $: ['Invalid sign-in.'] } }),
      }),
    );
    await page.goto('/(login:anmelden)');
    await expect(page.getByRole('heading', { level: 1, name: 'Anmelden' })).toBeVisible();

    await signIn(main(page));

    const summary = main(page).locator('pa-error-summary pa-notice');
    await expect(summary).toBeFocused();
    await expect(summary).toContainText('Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht.');
    await expect(summary.getByRole('link')).toHaveCount(0);
    await expect(page.locator('mat-snack-bar-container')).toHaveCount(0);
  });

  test('after the sign-in S01 closes and the page underneath keeps its input', async ({ page }) => {
    await fakeSession(page);
    await chooseHr(page);
    await press(header(page).getByRole('button', { name: 'Anmelden' }), 'Enter');
    await expect(main(page).getByRole('heading', { level: 1, name: 'Anmelden' })).toBeFocused();

    await signIn(main(page));

    await expect(page).toHaveURL(/\/kriterien$/);
    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeFocused();
    await expect(combobox(page)).toHaveValue('HR-Anwendung');
    await expect(header(page).getByRole('button', { name: 'Abmelden' })).toBeVisible();
  });

  test('INV:A11Y-01 INV:A11Y-08 the overlay after a 401 keeps the selection and sends the request again', async ({ page }, testInfo) => {
    await fakeSession(page);
    await expireSessionOnce(page, '**/api/v1/catalog/criteria?**');
    await toStep4(page);

    await press(main(page).getByRole('button', { name: 'Kriterienliste anzeigen' }), 'Enter');

    const overlay = page.getByRole('dialog', { name: 'Sie wurden abgemeldet' });
    await expect(overlay.getByLabel('E-Mail-Adresse')).toBeFocused();
    await expect(page).toHaveURL(/\/kriterien$/);
    await settled(overlay);
    await expectNoAxeViolations(page, testInfo, 'login overlay');

    await signIn(overlay);

    await expect(overlay).toHaveCount(0);
    await expect(page).toHaveURL(/\/kriterien\/ergebnis$/);
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
    await expect(page.getByRole('main').getByRole('definition').first()).toHaveText('HR-Anwendung');
  });

  test('„Schließen“ in the overlay leaves the user signed out on the page', async ({ page }) => {
    await fakeSession(page);
    await expireSessionOnce(page, '**/api/v1/catalog/criteria?**');
    await toStep4(page);
    const submit = main(page).getByRole('button', { name: 'Kriterienliste anzeigen' });
    await press(submit, 'Enter');
    const overlay = page.getByRole('dialog', { name: 'Sie wurden abgemeldet' });
    await expect(overlay.getByLabel('E-Mail-Adresse')).toBeFocused();

    await press(overlay.getByRole('button', { name: 'Schließen' }), 'Enter');

    await expect(overlay).toHaveCount(0);
    await expect(page).toHaveURL(/\/kriterien$/);
    await expect(submit).toBeFocused();
    await expect(header(page).getByRole('button', { name: 'Anmelden' })).toBeVisible();
  });

  test('„Abmelden“ clears the selection and moves the focus to the H1 of the start page', async ({ page }) => {
    await fakeSession(page, true);
    await chooseHr(page);
    const signOut = header(page).getByRole('button', { name: 'Abmelden' });
    await expect(signOut).toBeVisible();

    await press(signOut, 'Enter');

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
    await expect(header(page).getByRole('button', { name: 'Anmelden' })).toBeVisible();

    await press(header(page).getByRole('link', { name: 'Kriterien' }), 'Enter');
    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeFocused();
    await expect(combobox(page)).toHaveValue('');
  });
});

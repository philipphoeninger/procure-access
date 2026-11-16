// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type APIRequestContext, type Locator, type Page, type TestInfo } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { linksOf, waitForMail } from '../support/mailpit';
import { accountMenuButton } from '../support/session';

/**
 * Password reset S03 and the lockout note of S01 (S5-13b; R-AUTH-03, R-AUTH-04) on the real E2E
 * stack, by keyboard: the account is registered through the API, the reset mail is read from
 * Mailpit (`support/mailpit.ts`) and its link opened, the new password signs in and the used link is
 * refused. Axe in both appearances on each new state (A11Y-01, A11Y-08).
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';
const NEW_PASSWORD = 'e2e-neues-Passwort-2026';

const main = (page: Page) => page.getByRole('main');

/** A new address per test and browser project: projects share one stack when run locally. */
function newAddress(testInfo: TestInfo): string {
  return `reset-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
}

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Registers through the API with the probe's antiforgery token; S02 itself is tested in register.spec.ts. */
async function registerAccount(request: APIRequestContext, address: string): Promise<void> {
  expect((await request.get('/api/v1/auth/session')).ok()).toBe(true);
  const token = (await request.storageState()).cookies.find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
  expect(token, 'the probe issues XSRF-TOKEN').toBeTruthy();
  const response = await request.post('/api/v1/auth/register', {
    headers: { 'X-XSRF-TOKEN': token! },
    data: { email: address, password: PASSWORD, privacyAcknowledged: true },
  });
  expect(response.status()).toBe(204);
  await waitForMail(request, address);
}

/** Moves the focus to `locator` and presses `key` there, as Tab and a key would. */
async function press(locator: Locator, key = 'Enter'): Promise<void> {
  await locator.focus();
  await locator.press(key);
}

test.describe('S03 „Passwort zurücksetzen“', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 INV:A11Y-08 resets the password through the mailed link, signs in with the new one and refuses the used link', async ({ page, request }, testInfo) => {
    const address = newAddress(testInfo);
    await registerAccount(request, address);

    // From S01: „Passwort vergessen?“ opens S03 in place of the page
    await page.goto('/(login:anmelden)');
    await press(main(page).getByRole('link', { name: 'Passwort vergessen?' }));
    await expect(page).toHaveURL(/\/passwort$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Passwort zurücksetzen' })).toBeFocused();
    await expect(page).toHaveTitle('Passwort zurücksetzen – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's03-request');

    await main(page).getByLabel('E-Mail-Adresse').fill(address);
    await press(main(page).getByRole('button', { name: 'Link anfordern' }));
    await expect(main(page).getByText(`Falls ein Konto zu ${address} existiert, ist der Link unterwegs. Er ist 2 Stunden gültig.`)).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 's03-sent');

    const mail = await waitForMail(request, address, 2);
    expect(mail.subject).toBe('Passwort zurücksetzen');
    const links = linksOf(mail.html);
    expect(links).toHaveLength(1);
    const link = new URL(links[0]);
    expect(link.origin).toBe(new URL(page.url()).origin);
    expect(link.pathname).toBe('/passwort/neu');
    expect(link.search).not.toContain(encodeURIComponent(address));

    await page.goto(link.href);
    // Waits by id, not by label: a name computation of the field before axe crashes Firefox now and
    // then (like TESTING V129, V165); the label is checked by the component test and used after axe
    await expect(main(page).locator('#pa-new-password')).toBeVisible();
    expect(page.url()).not.toContain('token');
    await expect(page).toHaveTitle('Passwort zurücksetzen – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's03-new-password');

    await main(page).getByLabel('Neues Passwort').fill(NEW_PASSWORD);
    await press(main(page).getByRole('button', { name: 'Passwort speichern' }));
    await expect(main(page).getByRole('heading', { level: 2, name: 'Passwort geändert' })).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 's03-done');

    // „Zur Anmeldung“ opens S01; the new password signs in
    await press(main(page).getByRole('link', { name: 'Zur Anmeldung' }));
    await expect(main(page).getByRole('heading', { level: 1, name: 'Anmelden' })).toBeFocused();
    await main(page).getByLabel('E-Mail-Adresse').fill(address);
    await main(page).getByLabel('Passwort', { exact: true }).fill(NEW_PASSWORD);
    await press(main(page).getByRole('button', { name: 'Anmelden' }));
    await expect(accountMenuButton(page)).toBeVisible();

    // The link is single use: opened again it shows the form, and saving shows „Link nicht mehr gültig“
    await page.goto(link.href);
    await main(page).getByLabel('Neues Passwort').fill('noch-ein-Passwort-2026');
    await press(main(page).getByRole('button', { name: 'Passwort speichern' }));
    await expect(main(page).getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeFocused();
    await expect(page).toHaveTitle('Link nicht mehr gültig – ProcureAccess');
    await expect(main(page).getByRole('link', { name: 'Neuen Link anfordern' })).toHaveAttribute('href', '/passwort');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's03-invalid');
  });

  test('an unknown address gets the same confirmation (no account enumeration)', async ({ page }, testInfo) => {
    const address = newAddress(testInfo);
    await page.goto('/passwort');

    await main(page).getByLabel('E-Mail-Adresse').fill(address);
    await press(main(page).getByRole('button', { name: 'Link anfordern' }));

    await expect(main(page).getByText(`Falls ein Konto zu ${address} existiert, ist der Link unterwegs.`, { exact: false })).toBeFocused();
  });

  test('INV:A11Y-01 INV:A11Y-08 an empty request shows the error summary', async ({ page }, testInfo) => {
    await page.goto('/passwort');
    // Material adds `mat-form-field-animations-enabled` 300 ms after a field starts (see register.spec.ts)
    await expect(main(page).locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(1);
    await press(main(page).getByRole('button', { name: 'Link anfordern' }));

    const summary = main(page).locator('pa-error-summary pa-notice');
    await expect(summary).toBeFocused();
    await expect(summary.getByRole('link', { name: 'E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an.' })).toBeVisible();
    await expect(page).toHaveTitle('Fehler: Passwort zurücksetzen – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's03-errors');
  });

  test('S01 shows the lockout note with a refused sign-in (R-AUTH-03, #319)', async ({ page }, testInfo) => {
    await page.goto('/(login:anmelden)');
    await main(page).getByLabel('E-Mail-Adresse').fill(newAddress(testInfo));
    await main(page).getByLabel('Passwort', { exact: true }).fill('falsches-Passwort');
    await press(main(page).getByRole('button', { name: 'Anmelden' }));

    const summary = main(page).locator('pa-error-summary pa-notice');
    await expect(summary).toBeFocused();
    await expect(summary).toContainText('Anmeldung nicht möglich.');
    await expect(summary).toContainText('Nach 5 Fehlversuchen ist die Anmeldung aus Sicherheitsgründen für 15 Minuten gesperrt.');
  });
});

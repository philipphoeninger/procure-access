// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { linksOf, waitForMail } from '../support/mailpit';

/**
 * Registration S02 and the confirmation page (S5-12b; R-AUTH-01, R-AUTH-02) on the real E2E stack,
 * by keyboard: the edge cases — a known address, an unknown link, the empty submit, the rate limit.
 * The way through registration, the mail from Mailpit (PRV-11, E2E part), confirmation, sign-in and
 * sign-out is journey J8 (`journeys/j8-account.spec.ts`, S5-14). Answers the stack cannot produce on
 * demand (the rate limit) are faked. Axe in both appearances on each new state (A11Y-01, A11Y-08).
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');

/** A new address per test and browser project: projects share one stack when run locally. */
function newAddress(testInfo: TestInfo): string {
  return `register-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
}

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Fills S02 by keyboard and submits with Enter on „Konto erstellen“. */
async function register(page: Page, address: string): Promise<void> {
  await main(page).getByLabel('E-Mail-Adresse').fill(address);
  await main(page).getByLabel('Passwort', { exact: true }).fill(PASSWORD);
  const consent = main(page).getByRole('checkbox', { name: /^Ich habe die Datenschutzhinweise/ });
  await consent.focus();
  await consent.press('Space');
  await expect(consent).toBeChecked();
  const submit = main(page).getByRole('button', { name: 'Konto erstellen' });
  await submit.focus();
  await submit.press('Enter');
}

test.describe('S02 „Konto erstellen“ and the confirmation page', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('a known address gets the same success and the „already registered“ mail instead (no account enumeration)', async ({ page, request }, testInfo) => {
    const address = newAddress(testInfo);
    await page.goto('/registrieren');
    await register(page, address);
    await waitForMail(request, address);

    await page.goto('/registrieren');
    await register(page, address);

    await expect(main(page).getByRole('heading', { level: 2, name: 'E-Mail-Adresse bestätigen' })).toBeFocused();
    const mail = await waitForMail(request, address, 2);
    expect(mail.subject).toBe('Anmeldung bei ProcureAccess');
    expect(new URL(linksOf(mail.html)[0]).pathname).toBe('/(login:anmelden)');
  });

  test('INV:A11Y-01 INV:A11Y-08 an unknown or used-up link shows „Link nicht mehr gültig“', async ({ page }, testInfo) => {
    await page.goto('/registrieren/bestaetigen?token=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA');

    await expect(main(page).getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeVisible();
    expect(page.url()).not.toContain('token');
    await expect(page).toHaveTitle('Link nicht mehr gültig – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'confirm-invalid');
  });

  test('„Jetzt registrieren“ on S01 opens S02 in place of the page', async ({ page }) => {
    await page.goto('/(login:anmelden)');
    const link = main(page).getByRole('link', { name: 'Noch kein Konto? Jetzt registrieren' });
    await link.focus();
    await link.press('Enter');

    await expect(page).toHaveURL(/\/registrieren$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Konto erstellen' })).toBeFocused();
  });

  test('INV:A11Y-01 INV:A11Y-08 an empty submit shows the error summary with every field', async ({ page }, testInfo) => {
    await page.goto('/registrieren');
    // Material adds `mat-form-field-animations-enabled` 300 ms after a field starts; errors shown
    // before that fade in again once the class arrives, and axe measured them mid-fade (CI)
    await expect(main(page).locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(2);
    const submit = main(page).getByRole('button', { name: 'Konto erstellen' });
    await submit.focus();
    await submit.press('Enter');

    const summary = main(page).locator('pa-error-summary pa-notice');
    await expect(summary).toBeFocused();
    await expect(summary.getByRole('link')).toHaveCount(3);
    await expect(page).toHaveTitle('Fehler: Konto erstellen – ProcureAccess');
    await expect(main(page).locator('.mat-mdc-form-field-error-wrapper')).toHaveCount(2);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's02-errors');
  });

  test('the rate limit shows in the error summary with its minutes (API-05)', async ({ page }, testInfo) => {
    await page.route('**/api/v1/auth/register', (route) =>
      route.fulfill({
        status: 429,
        contentType: 'application/problem+json',
        json: { type: 'https://procureaccess.de/problems/rate-limit', status: 429, retryAfterMinutes: 60 },
      }),
    );
    await page.goto('/registrieren');

    await register(page, newAddress(testInfo));

    await expect(main(page).locator('pa-error-summary pa-notice')).toBeFocused();
    await expect(main(page).getByText(/Bitte warten Sie 60 Minuten/)).toBeVisible();
  });
});

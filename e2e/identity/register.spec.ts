// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { linksOf, waitForMail } from '../support/mailpit';

/**
 * Registration S02 and the confirmation page (S5-12b; R-AUTH-01, R-AUTH-02) on the real E2E stack,
 * by keyboard: the account is created by the API, the mail is read from Mailpit
 * (`support/mailpit.ts`) and its link opened (owner decision S5-12 2 (a)). PRV-11 (E2E part) checks
 * that mail. Answers the stack cannot produce on demand (the rate limit) are faked. S5-14 builds
 * the whole account journey with sign-in and sign-out on this. Axe in both appearances on each new
 * state (A11Y-01, A11Y-08).
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
  // By role alone, the only checkbox of S02: a name check of a mat-checkbox together with axe crashes
  // Firefox (TESTING V129, V165); the component test and axe check its name.
  const consent = main(page).getByRole('checkbox');
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

  test('INV:PRV-11 INV:A11Y-01 INV:A11Y-08 registers; the mail links to the confirmation page, which takes the token out of the address and confirms', async ({ page, request }, testInfo) => {
    const address = newAddress(testInfo);
    await page.goto('/registrieren');
    await expect(main(page).getByRole('heading', { level: 1, name: 'Konto erstellen' })).toBeVisible();
    await expect(page).toHaveTitle('Konto erstellen – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's02-form');

    await register(page, address);

    await expect(main(page).getByRole('heading', { level: 2, name: 'E-Mail-Adresse bestätigen' })).toBeFocused();
    await expect(main(page).getByText(`Wir haben Ihnen eine E-Mail an ${address} geschickt.`, { exact: false })).toBeVisible();
    await expectNoAxeViolations(page, testInfo, 's02-success');

    // PRV-11 (E2E part): only the purpose of the message — no image, no tracking, one link to the client itself
    const mail = await waitForMail(request, address);
    expect(mail.to).toEqual([address]);
    expect(mail.subject).toBe('Bitte bestätigen Sie Ihre E-Mail-Adresse');
    expect(mail.html).not.toMatch(/<img|src=|style|<script/i);
    const links = linksOf(mail.html);
    expect(links).toHaveLength(1);
    const link = new URL(links[0]);
    expect(link.origin).toBe(new URL(page.url()).origin);
    expect(link.pathname).toBe('/registrieren/bestaetigen');
    expect(mail.text).toContain(link.href);
    expect(`${mail.text}${mail.html}`).not.toContain(PASSWORD);

    // No request of the page carries the token in its Referer (the referrer policy of index.html).
    // `ng serve` injects its /@vite/client script before that meta tag; the release build has none.
    const referers: string[] = [];
    page.on('request', (request) => {
      if (!new URL(request.url()).pathname.startsWith('/@vite/')) referers.push(request.headers()['referer'] ?? '');
    });
    await page.goto(link.href);

    await expect(main(page).getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeVisible();
    await expect(page).toHaveURL(/\/registrieren\/bestaetigen$/);
    expect(page.url()).not.toContain('token');
    expect(referers.length).toBeGreaterThan(1);
    expect(referers.filter((referer) => referer.includes('token'))).toEqual([]);
    await expect(page).toHaveTitle('E-Mail-Adresse bestätigt – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'confirm-done');

    // A second click (or a mail scanner first) still says „bestätigt“ (decision S5-12 4 (b))
    await page.goto(link.href);
    await expect(main(page).getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeVisible();
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

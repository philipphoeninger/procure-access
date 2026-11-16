// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { linksOf, waitForMail } from '../support/mailpit';
import { accountMenuButton, signOutThroughHeader } from '../support/session';

/**
 * J8, account (TESTING §5.4; S5-14), on the real E2E stack by keyboard: register on S02 → read the
 * confirmation mail from Mailpit (`support/mailpit.ts`) → open its link → sign in on S01 → signed in,
 * with nothing of the session in browser storage (SEC-01, E2E part) → sign out. PRV-11 (E2E part)
 * checks the mail; S5-12b began this test in `identity/register.spec.ts`, which keeps the edge cases
 * of S02 and the confirmation page. Axe in both appearances on every state (A11Y-01, A11Y-08).
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

/** The Identity session cookie (S5-07); `HttpOnly`, so the page never sees it. */
const SESSION_COOKIE = '.ProcureAccess.Session';

const main = (page: Page) => page.getByRole('main');
const header = (page: Page) => page.getByRole('banner');

/** A new address per run and browser project: projects share one stack when run locally. */
function newAddress(testInfo: TestInfo): string {
  return `j8-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
}

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Moves the focus to `locator` and presses `key` there, as Tab and a key would. */
async function press(locator: Locator, key = 'Enter'): Promise<void> {
  await locator.focus();
  await locator.press(key);
}

/** Fills S02 by keyboard and submits with Enter on „Konto erstellen“. */
async function register(page: Page, address: string): Promise<void> {
  await main(page).getByLabel('E-Mail-Adresse').fill(address);
  await main(page).getByLabel('Passwort', { exact: true }).fill(PASSWORD);
  // By role alone, the only checkbox of S02: a name check of a mat-checkbox together with axe crashes
  // Firefox (TESTING V129, V165); the component test and axe check its name.
  const consent = main(page).getByRole('checkbox');
  await press(consent, 'Space');
  await expect(consent).toBeChecked();
  await press(main(page).getByRole('button', { name: 'Konto erstellen' }));
}

/** The signed-in address of the session probe, or null (SPEC §8.8). */
async function sessionEmail(page: Page): Promise<string | null> {
  const response = await page.request.get('/api/v1/auth/session');
  expect(response.ok()).toBe(true);
  return ((await response.json()) as { user: { email: string } | null }).user?.email ?? null;
}

test.describe('J8 account: register, confirm, sign in, sign out', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:SEC-01 INV:PRV-11 INV:A11Y-01 INV:A11Y-08 registers, confirms through the mailed link, signs in without a token in browser storage and signs out', async ({ page, request }, testInfo) => {
    const address = newAddress(testInfo);

    // S02
    await page.goto('/registrieren');
    await expect(main(page).getByRole('heading', { level: 1, name: 'Konto erstellen' })).toBeVisible();
    await expect(page).toHaveTitle('Konto erstellen – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j8-s02-form');

    await register(page, address);

    await expect(main(page).getByRole('heading', { level: 2, name: 'E-Mail-Adresse bestätigen' })).toBeFocused();
    await expect(main(page).getByText(`Wir haben Ihnen eine E-Mail an ${address} geschickt.`, { exact: false })).toBeVisible();
    await expectNoAxeViolations(page, testInfo, 'j8-s02-success');

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

    // The confirmation page. No request of the page carries the token in its Referer (the referrer
    // policy of index.html); `ng serve` injects its /@vite/client script before that meta tag.
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
    await expectNoAxeViolations(page, testInfo, 'j8-confirm-done');

    // A second click (or a mail scanner first) still says „bestätigt“ (decision S5-12 4 (b))
    await page.goto(link.href);
    await expect(main(page).getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeVisible();

    // S01 over the confirmation page
    await press(header(page).getByRole('button', { name: 'Anmelden' }));
    await expect(main(page).getByRole('heading', { level: 1, name: 'Anmelden' })).toBeFocused();
    await expect(page).toHaveTitle('Anmelden – ProcureAccess');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j8-s01');

    await main(page).getByLabel('E-Mail-Adresse').fill(address);
    await main(page).getByLabel('Passwort', { exact: true }).fill(PASSWORD);
    await main(page).getByLabel('Passwort', { exact: true }).press('Enter');

    // Signed in: S01 closes, the page underneath is back with the focus on its H1
    await expect(main(page).getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeFocused();
    await expect(page).toHaveURL(/\/registrieren\/bestaetigen$/);
    await expect(accountMenuButton(page)).toBeVisible();
    expect(await sessionEmail(page)).toBe(address);
    await settled(page.locator('body'));
    await expectNoAxeViolations(page, testInfo, 'j8-signed-in');

    // SEC-01 (E2E part): the session lives only in the HttpOnly cookie. Browser storage is empty
    // (since S5-25b nothing writes to it, R-SET-02, TD-25), and the only cookie the page can read
    // is XSRF-TOKEN. `Secure` is not set on the plain-HTTP E2E stack
    // (S5-07 decision 3a); CookieSessionTests check it on HTTPS.
    const cookies = await page.context().cookies();
    const session = cookies.find((cookie) => cookie.name === SESSION_COOKIE);
    expect(session, `the ${SESSION_COOKIE} cookie`).toBeDefined();
    expect(session!.httpOnly).toBe(true);
    expect(session!.sameSite).toBe('Strict');
    expect(cookies.filter((cookie) => !cookie.httpOnly).map((cookie) => cookie.name)).toEqual(['XSRF-TOKEN']);
    const storage = await page.evaluate(() => {
      const entries = (store: Storage) => Object.keys(store).map((key) => [key, store.getItem(key) ?? '']);
      return {
        entries: [...entries(window.localStorage), ...entries(window.sessionStorage)],
        cookieNames: document.cookie.split(';').map((cookie) => cookie.split('=')[0].trim()).filter(Boolean),
      };
    });
    expect(storage.cookieNames).toEqual(['XSRF-TOKEN']);
    expect(storage.entries, 'local and session storage').toEqual([]);

    // „Abmelden“ in the menu „Konto“ (S5-18): the start page with the focus on its H1, the session gone
    await signOutThroughHeader(page);

    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
    await expect(header(page).getByRole('button', { name: 'Anmelden' })).toBeVisible();
    expect(await sessionEmail(page)).toBeNull();
    expect((await page.context().cookies()).map((cookie) => cookie.name)).not.toContain(SESSION_COOKIE);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j8-signed-out');
  });
});

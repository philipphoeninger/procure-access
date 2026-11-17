// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page, type TestInfo } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { registerAccount } from '../support/accounts';
import { SessionClock } from '../support/clock';
import { accountMenuButton, signInThroughApi } from '../support/session';
import { CONFIRMED_USER, TEST_USER_PASSWORD } from '../support/users';

/**
 * J5 — the session (TESTING §5.4; SPEC R-AUTH-06, R-AUTH-07; A11Y-04, SEC-03, SEC-07; S5-30) on a
 * controlled clock: the browser's `page.clock` and the API's clock of this browser context
 * (`support/clock.ts`), moved together. The idle limit is 30 minutes, the absolute limit 8 hours,
 * the warning 5 minutes before the earlier one (🔧 defaults of the E2E stack).
 *
 * Each test registers its own account, so it may let the session expire and change the settings;
 * the seeded user is only signed in (owner decision S5-30 D3 (b)). The page with input is S60: a
 * choice is applied at once and saved with a signed-in request.
 */

const PASSWORD = 'e2e-Sitzung-Passwort-1';
const WIDE = { width: 1280, height: 800 };

const warning = (page: Page) => page.getByRole('alertdialog', { name: 'Ihre Sitzung endet bald' });
const overlay = (page: Page) => page.getByRole('dialog', { name: 'Sie wurden abgemeldet' });
const announcement = (page: Page) => page.locator('.cdk-live-announcer-element');
const dark = (page: Page) => page.getByRole('radio', { name: 'Dunkel' });
const RESTORED = 'Sie sind wieder angemeldet. Ihre Eingaben sind erhalten.';

/**
 * Records every text of the live announcer from now on: after the sign-in in the overlay the
 * settings are read again, and the loading announcement „Wird geladen …“ follows `restored` (S5-30
 * open point), so the last text alone does not show what was announced.
 */
async function recordAnnouncements(page: Page): Promise<void> {
  await page.evaluate(() => {
    const seen: string[] = [];
    (window as unknown as { paAnnouncements: string[] }).paAnnouncements = seen;
    new MutationObserver(() => {
      const text = document.querySelector('.cdk-live-announcer-element')?.textContent?.trim();
      if (text && seen.at(-1) !== text) {
        seen.push(text);
      }
    }).observe(document.body, { subtree: true, childList: true, characterData: true });
  });
}

const announcements = (page: Page) =>
  page.evaluate(() => (window as unknown as { paAnnouncements: string[] }).paAnnouncements);

/** Registers an account of this test and signs the page in with it, on S60 with a controlled clock. */
async function signedInOnSettings(page: Page, testInfo: TestInfo, startMinutes = 0): Promise<{ clock: SessionClock; address: string }> {
  const address = `j5-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.invalid`;
  await page.setViewportSize(WIDE);
  await registerAccount(page.request, address, PASSWORD);
  const clock = await SessionClock.install(page, startMinutes);
  await signInThroughApi(page, address, PASSWORD);
  return { clock, address };
}

async function openSettings(page: Page): Promise<void> {
  await page.goto('/einstellungen');
  await expect(page.getByRole('heading', { level: 1, name: 'Einstellungen' })).toBeVisible();
}

/** Chooses „Dunkel“ and waits until it is saved. */
async function chooseDark(page: Page): Promise<void> {
  await dark(page).check();
  await expect(announcement(page)).toHaveText('Einstellungen gespeichert.');
}

/** The account's stored appearance, read through the API in this context (and its clock). */
async function storedAppearance(page: Page): Promise<string> {
  const response = await page.request.get('/api/v1/account/preferences');
  expect(response.ok()).toBe(true);
  return ((await response.json()) as { appearance: string }).appearance;
}

async function signInInOverlay(page: Page, address: string, password: string): Promise<void> {
  const dialog = overlay(page);
  await expect(dialog.getByLabel('E-Mail-Adresse')).toBeFocused();
  await dialog.getByLabel('E-Mail-Adresse').fill(address);
  await dialog.getByLabel('Passwort', { exact: true }).fill(password);
  await dialog.getByLabel('Passwort', { exact: true }).press('Enter');
}

test.describe('J5 session (R-AUTH-06, R-AUTH-07)', () => {
  test('INV:A11Y-04 INV:A11Y-08 INV:SEC-07 the warning before the idle limit is announced; „Angemeldet bleiben“ keeps the user working', async ({ page }, testInfo) => {
    const { clock } = await signedInOnSettings(page, testInfo);
    await openSettings(page);

    await clock.move(25);

    const dialog = warning(page);
    await expect(dialog).toBeVisible();
    await expect(dialog).toContainText('Aus Sicherheitsgründen werden Sie in 5 Minuten abgemeldet.');
    await expect(dialog.getByRole('button', { name: 'Angemeldet bleiben' })).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 'session warning before the idle limit');

    await dialog.getByRole('button', { name: 'Angemeldet bleiben' }).press('Enter');

    await expect(dialog).toHaveCount(0);
    await expect(announcement(page)).toHaveText('Ihre Sitzung wurde verlängert.');

    // 45 minutes after the sign-in, past the first idle limit: still signed in, the work goes on.
    await clock.move(20);
    await expect(warning(page)).toHaveCount(0);
    await chooseDark(page);
    await expect(accountMenuButton(page)).toBeVisible();
    expect(await storedAppearance(page)).toBe('dark');
  });

  test('INV:A11Y-04 INV:SEC-07 at the idle limit the login overlay replaces the warning; the page and its input stay', async ({ page }, testInfo) => {
    const { clock, address } = await signedInOnSettings(page, testInfo);
    await openSettings(page);
    await chooseDark(page);

    await clock.move(25);
    await expect(warning(page)).toBeVisible();
    await clock.move(5);

    await expect(warning(page)).toHaveCount(0);
    await expect(overlay(page)).toContainText('Ihre Eingaben auf dieser Seite bleiben erhalten.');
    await recordAnnouncements(page);
    await signInInOverlay(page, address, PASSWORD);

    await expect(overlay(page)).toHaveCount(0);
    await expect.poll(() => announcements(page)).toContain(RESTORED);
    await expect(page).toHaveURL(/\/einstellungen$/);
    await expect(dark(page)).toBeChecked();
    await expect(accountMenuButton(page)).toBeVisible();
  });

  test('INV:A11Y-04 INV:SEC-03 a request after the idle limit opens the login overlay and is sent again after the sign-in', async ({ page }, testInfo) => {
    const { clock, address } = await signedInOnSettings(page, testInfo);
    await openSettings(page);

    // The page's timers did not run (a laptop that slept): only the API's time passed the limit.
    await clock.moveApi(31);
    await dark(page).check();

    await expect(overlay(page)).toBeVisible();
    await signInInOverlay(page, address, PASSWORD);

    await expect(overlay(page)).toHaveCount(0);
    await expect(dark(page)).toBeChecked();
    await expect.poll(() => storedAppearance(page)).toBe('dark');
  });

  test('INV:A11Y-04 INV:A11Y-08 before the absolute limit the warning asks to save and sign in again, without „Angemeldet bleiben“', async ({ page }, testInfo) => {
    const address = `j5-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.invalid`;
    await page.setViewportSize(WIDE);
    await registerAccount(page.request, address, PASSWORD);
    // Signed in at the real time, then kept alive for 7 hours 55 minutes by a request every 25 minutes.
    const clock = await SessionClock.install(page);
    await signInThroughApi(page, address, PASSWORD);
    const token = (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')!.value;
    for (let step = 0; step < 19; step++) {
      await clock.moveApi(25);
      const response = await page.request.post('/api/v1/auth/session/extend', { headers: { 'X-XSRF-TOKEN': token } });
      expect(response.status(), `extension ${step + 1}`).toBe(204);
    }
    await page.clock.setSystemTime(Date.now() + 475 * 60_000);

    // The warning is due at once: it opens over S60 as the page loads and hides the page behind it.
    await page.goto('/einstellungen');

    const dialog = warning(page);
    await expect(dialog).toContainText(
      'Ihre Sitzung endet in 5 Minuten und kann nicht mehr verlängert werden. Speichern Sie Ihre Eingaben und melden Sie sich danach erneut an.',
    );
    await expect(dialog.getByRole('button', { name: 'Angemeldet bleiben' })).toHaveCount(0);
    await expect(dialog.getByRole('button', { name: 'Schließen' })).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 'session warning before the absolute limit');

    await dialog.getByRole('button', { name: 'Schließen' }).press('Enter');
    await expect(dialog).toHaveCount(0);
  });

  test('INV:SEC-03 another account signing in through the overlay gets the start page, not the previous user\'s page', async ({ page }, testInfo) => {
    const { clock } = await signedInOnSettings(page, testInfo);
    await openSettings(page);

    await clock.move(30);
    await expect(overlay(page)).toBeVisible();
    await recordAnnouncements(page);
    await signInInOverlay(page, CONFIRMED_USER, TEST_USER_PASSWORD);

    await expect(overlay(page)).toHaveCount(0);
    await expect(page).toHaveURL(/\/$/);
    await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
    await expect(accountMenuButton(page)).toHaveAccessibleName(`Konto, angemeldet als ${CONFIRMED_USER}`);
    expect(await announcements(page)).not.toContain(RESTORED);
  });

  test('INV:SEC-03 „Schließen“ in the overlay leaves the user signed out on the same page', async ({ page }, testInfo) => {
    const { clock } = await signedInOnSettings(page, testInfo);
    await openSettings(page);

    await clock.move(30);
    await expect(overlay(page)).toBeVisible();
    await overlay(page).getByRole('button', { name: 'Schließen' }).press('Enter');

    await expect(overlay(page)).toHaveCount(0);
    await expect(page).toHaveURL(/\/einstellungen$/);
    await expect(page.getByRole('banner').getByRole('button', { name: 'Anmelden' })).toBeVisible();
    await expect(accountMenuButton(page)).toHaveCount(0);
  });
});

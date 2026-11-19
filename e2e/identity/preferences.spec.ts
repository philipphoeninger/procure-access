// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { registerConfirmedAccount } from '../support/accounts';
import { accountMenuButton, signInThroughApi, signOutThroughHeader } from '../support/session';

/**
 * A signed-in user's settings (S5-25b; SPEC R-SET-01, R-SET-02, §8.12; TESTING A11Y-06, A11Y-08):
 * the stored choice overrides the system after the sign-in, on every device, and ends with the
 * sign-out; nothing is kept in browser storage, and blocked storage breaks nothing (TD-25, #234).
 * The first test saves them through the API; since S5-26 the second changes them on the settings
 * page S60: applied at once without reload, saved, and shown again on the next visit. Each test
 * builds its own account, so the seeded users stay unchanged.
 */

const PASSWORD = 'e2e-Einstellungen-Passwort-1';

const htmlClasses = (page: Page) =>
  page.evaluate(() =>
    [...document.documentElement.classList].filter((name) => /^(theme-|motion-|contrast-)/.test(name)).sort(),
  );

const storageEntries = (page: Page) =>
  page.evaluate(() => window.localStorage.length + window.sessionStorage.length);

/** Saves the settings of the user `page` is signed in as (`PUT /api/v1/account/preferences`). */
async function savePreferences(page: Page, preferences: Record<string, string>): Promise<void> {
  const token = (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
  expect(token, 'the sign-in issued XSRF-TOKEN').toBeTruthy();
  const response = await page.request.put('/api/v1/account/preferences', {
    headers: { 'X-XSRF-TOKEN': token! },
    data: preferences,
  });
  expect(response.status(), 'saving the settings').toBe(200);
}

/** „Abmelden“ in the menu „Konto“ of the full header, or in the compact menu on a narrow screen. */
async function signOut(page: Page): Promise<void> {
  if (await accountMenuButton(page).isVisible()) {
    await signOutThroughHeader(page);
    return;
  }
  await page.getByRole('button', { name: 'Menü', exact: true }).click();
  await page.getByRole('dialog', { name: 'Menü' }).getByRole('button', { name: 'Abmelden' }).click();
}

/** Opens S60 through „Einstellungen“ in the menu „Konto“, or in the compact menu on a narrow screen. */
async function openSettings(page: Page): Promise<void> {
  if (await accountMenuButton(page).isVisible()) {
    await accountMenuButton(page).click();
    await page.getByRole('menuitem', { name: 'Einstellungen' }).click();
  } else {
    await page.getByRole('button', { name: 'Menü', exact: true }).click();
    await page.getByRole('dialog', { name: 'Menü' }).getByRole('link', { name: 'Einstellungen' }).click();
  }
  await expect(page.getByRole('heading', { level: 1, name: 'Einstellungen' })).toBeVisible();
}

test.describe('User preferences (R-SET-01, R-SET-02)', () => {
  test('INV:A11Y-06 INV:A11Y-08 INV:A11Y-01 the saved dark appearance and reduced motion override the light system, on every sign-in, until the sign-out', async ({
    page,
    request,
    browser,
  }, testInfo) => {
    const address = `settings-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await page.emulateMedia({ colorScheme: 'light', reducedMotion: 'no-preference' });
    await signInThroughApi(page, address, PASSWORD);
    await savePreferences(page, { language: 'de', appearance: 'dark', motion: 'reduced', contrast: 'system' });

    await test.step('signed in: dark and reduced motion over the light system, checked by axe', async () => {
      await page.goto('/');
      await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
      await expect.poll(() => htmlClasses(page)).toEqual(['motion-reduced', 'theme-dark']);
      await expectNoAxeViolations(page, testInfo, 'signed in with the explicit dark appearance', { explicitAppearance: true });
      expect(await storageEntries(page), 'local and session storage').toBe(0);
    });

    await test.step('another browser: the same settings after its sign-in', async () => {
      const other = await browser.newContext({ colorScheme: 'light', reducedMotion: 'no-preference' });
      try {
        const otherPage = await other.newPage();
        await otherPage.goto('/');
        await expect.poll(() => htmlClasses(otherPage)).toEqual(['theme-light']);
        await signInThroughApi(otherPage, address, PASSWORD);
        await otherPage.reload();
        await expect.poll(() => htmlClasses(otherPage)).toEqual(['motion-reduced', 'theme-dark']);
      } finally {
        await other.close();
      }
    });

    await test.step('signed out: the system again, without a reload', async () => {
      await signOut(page);
      await expect.poll(() => htmlClasses(page)).toEqual(['theme-light']);
      expect(await storageEntries(page), 'local and session storage').toBe(0);
    });
  });

  test('INV:A11Y-08 INV:A11Y-06 INV:A11Y-01 S60 applies a change at once without reload, saves it and shows it on the next visit', async ({
    page,
    request,
  }, testInfo) => {
    const address = `s60-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await page.emulateMedia({ colorScheme: 'light', reducedMotion: 'no-preference' });
    await signInThroughApi(page, address, PASSWORD);
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    await test.step('S60 through the menu: the stored defaults, checked by axe', async () => {
      await openSettings(page);
      await expect(page).toHaveURL(/\/einstellungen$/);
      await expect(page.getByRole('radio', { name: 'Deutsch' })).toBeChecked();
      await expect(page.getByRole('group', { name: 'Erscheinungsbild' }).getByRole('radio', { name: 'Wie im Betriebssystem eingestellt' })).toBeChecked();
      await expect(page.getByRole('radio', { name: 'Hoher Kontrast' })).toHaveCount(0);
      await expectNoAxeViolations(page, testInfo, 'S60 with the system settings');
    });

    await test.step('„Dunkel“ and „Bewegungen reduzieren“ apply at once, without reload, and are announced', async () => {
      // A reload would drop this marker.
      await page.evaluate(() => ((window as unknown as { paNoReload: boolean }).paNoReload = true));
      await page.getByRole('radio', { name: 'Dunkel' }).check();
      await expect.poll(() => htmlClasses(page)).toEqual(['theme-dark']);
      await page.getByRole('radio', { name: 'Bewegungen reduzieren' }).check();
      await expect.poll(() => htmlClasses(page)).toEqual(['motion-reduced', 'theme-dark']);
      await expect(page.locator('.cdk-live-announcer-element')).toHaveText('Einstellungen gespeichert.');
      expect(await page.evaluate(() => (window as unknown as { paNoReload?: boolean }).paNoReload)).toBe(true);
      await expectNoAxeViolations(page, testInfo, 'S60 with the explicit dark appearance', { explicitAppearance: true });
    });

    await test.step('saved: the server answers them, and S60 shows them after a reload', async () => {
      const stored = await page.request.get('/api/v1/account/preferences');
      expect(await stored.json()).toEqual({ language: 'de', appearance: 'dark', motion: 'reduced', contrast: 'system', assessmentLegend: 'auto' });
      await page.reload();
      await expect(page.getByRole('heading', { level: 1, name: 'Einstellungen' })).toBeVisible();
      await expect(page.getByRole('radio', { name: 'Dunkel' })).toBeChecked();
      await expect(page.getByRole('radio', { name: 'Bewegungen reduzieren' })).toBeChecked();
      await expect.poll(() => htmlClasses(page)).toEqual(['motion-reduced', 'theme-dark']);
      expect(await storageEntries(page), 'local and session storage').toBe(0);
    });
  });

  test('without an account the page follows the system and keeps nothing in browser storage', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark', reducedMotion: 'reduce' });

    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await page.goto('/kriterien');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    await expect.poll(() => htmlClasses(page)).toEqual(['motion-reduced', 'theme-dark']);
    expect(await storageEntries(page), 'local and session storage').toBe(0);
  });

  // TD-25: the legacy settings store threw when storage was blocked, so the products never loaded.
  test('with browser storage blocked the pages load and follow the system', async ({ page }) => {
    await page.addInitScript(() => {
      for (const name of ['localStorage', 'sessionStorage']) {
        Object.defineProperty(window, name, {
          get() {
            throw new DOMException('Storage is blocked', 'SecurityError');
          },
        });
      }
    });
    const errors: string[] = [];
    page.on('pageerror', (error) => errors.push(error.message));
    await page.emulateMedia({ colorScheme: 'dark' });

    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await page.goto('/kriterien');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    await expect.poll(() => htmlClasses(page)).toEqual(['theme-dark']);
    expect(errors).toEqual([]);
  });
});

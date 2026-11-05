// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type Route } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * Start page S04 (R-SHL-05), error pages S80 and the offline notice (R-SHL-08), and the loading
 * state (R-SHL-09), with axe on every state (A11Y-01). Server errors, a missing connection and a
 * slow answer are simulated in the browser (`page.route`), so the stack itself stays untouched.
 * They use the legacy criteria screen: it loads its data on every visit (the products are loaded
 * once at start-up).
 */
const WIDE = { width: 1280, height: 800 };
const COMPACT = { width: 360, height: 780 };
const API = '**/api/**';

const mainNav = (page: Page) => page.getByRole('navigation', { name: 'Hauptnavigation' });

/**
 * Waits until the transitions of a clicked element are over, so axe sees the settled colours.
 * A transition that is cancelled (e.g. the focus moved on) rejects `finished`: it is over too.
 */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/**
 * Records every text of CDK LiveAnnouncer's live region (the only one A11yAnnouncerService
 * writes to): queued messages follow each other within 100 ms, too fast to poll.
 */
async function recordAnnouncements(page: Page): Promise<() => Promise<string[]>> {
  await page.addInitScript(() => {
    const seen: string[] = [];
    (window as unknown as { paAnnouncements: string[] }).paAnnouncements = seen;
    new MutationObserver(() => {
      const text = document.querySelector('.cdk-live-announcer-element')?.textContent?.trim();
      if (text && seen.at(-1) !== text) seen.push(text);
    }).observe(document, { subtree: true, childList: true, characterData: true });
  });
  return () => page.evaluate(() => (window as unknown as { paAnnouncements: string[] }).paAnnouncements);
}

test.describe('Start page', () => {
  for (const [layout, viewport] of [['wide', WIDE], ['360 px', COMPACT]] as const) {
    test(`INV:A11Y-01 shows H1, entry cards and disclaimer (${layout})`, async ({ page }, testInfo) => {
      await page.setViewportSize(viewport);
      await page.goto('/');

      await expect(page).toHaveTitle('ProcureAccess – Barrierefreie Software beschaffen');
      await expect(page.getByRole('heading', { level: 1, name: 'Barrierefreie Software beschaffen – Schritt für Schritt' })).toBeVisible();
      await expect(page.getByRole('heading', { level: 2 })).toHaveText(['Kriterien zusammenstellen', 'Produkte nachschlagen']);
      await expect(page.getByRole('note')).toContainText('ProcureAccess ersetzt keine professionelle Barrierefreiheitsprüfung.');
      await expectNoAxeViolations(page, testInfo, `start page, ${layout}`);
    });
  }

  test('INV:A11Y-01 the cards open their screens without an account', async ({ page }) => {
    await page.setViewportSize(WIDE);
    await page.goto('/');

    await page.getByRole('main').getByRole('link', { name: 'Produktverzeichnis öffnen' }).click();
    await expect(page).toHaveURL(/\/products$/);

    await page.goto('/');
    await page.getByRole('main').getByRole('link', { name: 'Kriterien zusammenstellen' }).click();
    await expect(page).toHaveURL(/\/filters$/);
  });

  test('the legacy address /home leads to the start page', async ({ page }) => {
    await page.goto('/home');

    await expect(page).toHaveURL(/:\d+\/$/);
    await expect(page.getByRole('heading', { level: 1, name: 'Barrierefreie Software beschaffen – Schritt für Schritt' })).toBeVisible();
  });
});

test.describe('Error pages S80', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 an unknown address shows „Seite nicht gefunden“ and keeps the URL', async ({ page }, testInfo) => {
    await page.goto('/gibt-es-nicht');

    await expect(page).toHaveTitle('Seite nicht gefunden – ProcureAccess');
    await expect(page.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeVisible();
    await expect(page).toHaveURL(/\/gibt-es-nicht$/);
    await expectNoAxeViolations(page, testInfo, '404 page');

    await page.getByRole('link', { name: 'Zur Startseite' }).click();
    await expect(page).toHaveTitle('ProcureAccess – Barrierefreie Software beschaffen');
  });

  test('INV:A11Y-01 a page without the permission shows „Kein Zugriff“', async ({ page }, testInfo) => {
    // The legacy approver list: the permission guard sends visitors without the role here.
    await page.goto('/proposals');

    await expect(page).toHaveURL(/\/unauthorized$/);
    await expect(page).toHaveTitle('Kein Zugriff – ProcureAccess');
    await expect(page.getByRole('heading', { level: 1, name: 'Kein Zugriff' })).toBeVisible();
    await expectNoAxeViolations(page, testInfo, '403 page');
  });

  test('INV:A11Y-01 a server error shows „Etwas ist schiefgelaufen“ in place, with the focus on the H1', async ({ page }, testInfo) => {
    await page.goto('/');
    await page.route(API, (route) => route.fulfill({ status: 500, contentType: 'application/problem+json', body: '{"status":500}' }));

    const criteria = mainNav(page).getByRole('link', { name: 'Kriterien' });
    await criteria.click();

    const heading = page.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' });
    await expect(heading).toBeFocused();
    await expect(page).toHaveTitle('Etwas ist schiefgelaufen – ProcureAccess');
    await expect(page).toHaveURL(/\/filters$/);
    await settled(criteria);
    await expectNoAxeViolations(page, testInfo, '500 page');
  });
});

test.describe('System states', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 without an answer from the server the offline notice appears until it answers again', async ({ page }, testInfo) => {
    await page.goto('/');
    await page.route(API, (route) => route.abort('internetdisconnected'));

    const criteria = mainNav(page).getByRole('link', { name: 'Kriterien' });
    await criteria.click();

    const notice = page.getByRole('main').getByRole('status');
    await expect(notice).toContainText('Keine Verbindung zum Server. Bitte prüfen Sie Ihre Internetverbindung.');
    // A failed load ends the loading state (#168).
    await expect(page.getByRole('progressbar')).toBeHidden();
    await settled(criteria);
    await expectNoAxeViolations(page, testInfo, 'offline notice');

    await page.unroute(API);
    await mainNav(page).getByRole('link', { name: 'Produkte' }).click();
    await mainNav(page).getByRole('link', { name: 'Kriterien' }).click();
    await expect(notice).toBeHidden();
  });

  test('on a first page load without API answers the offline notice stays when the translations answer last (#171)', async ({ page }) => {
    await page.route(API, (route) => route.abort('internetdisconnected'));
    // The web server still serves the static files; de.json answers after the failed API calls.
    await page.route('**/i18n/de.json', async (route) => {
      await new Promise((resolve) => setTimeout(resolve, 800));
      await route.continue();
    });

    await page.goto('/filters');

    // The text exists only once de.json has answered, so this checks the state after that answer.
    await expect(page.getByRole('main').getByRole('status')).toContainText(
      'Keine Verbindung zum Server. Bitte prüfen Sie Ihre Internetverbindung.',
    );
    await expect(page.getByRole('progressbar')).toBeHidden();
  });

  test('INV:A11Y-01 a slow answer shows the progress bar and announces „Wird geladen …“', async ({ page }, testInfo) => {
    const announcements = await recordAnnouncements(page);
    await page.goto('/');
    let release!: () => void;
    const released = new Promise<void>((resolve) => (release = resolve));
    await page.route(API, async (route: Route) => {
      await released;
      await route.continue();
    });

    const criteria = mainNav(page).getByRole('link', { name: 'Kriterien' });
    await criteria.click();

    await expect(page.getByRole('main').getByRole('progressbar', { name: 'Wird geladen …' })).toBeVisible();
    // Both reach the live region: the page title of the route change does not swallow the loading text.
    await expect.poll(announcements).toEqual(expect.arrayContaining(['Wird geladen …', 'ProcureAccess']));
    await settled(criteria);
    await expectNoAxeViolations(page, testInfo, 'loading');

    release();
    await expect(page.getByRole('progressbar')).toBeHidden();
  });
});

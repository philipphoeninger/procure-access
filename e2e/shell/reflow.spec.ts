// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page } from '@playwright/test';

/**
 * Reflow of the shell pages (A11Y-09; S2-18, R-SHL-10, WCAG 1.4.10): at 320 × 256 CSS px, which is a
 * 1280 × 1024 window at 400 % zoom, no page scrolls sideways and every link and button lies
 * inside the viewport. Covers the pages of the new shell and the rebuilt screens: S10 and the S11
 * placeholder since S4-13. The legacy screens are checked by hand until they are rebuilt (protocol
 * S2-18). A rebuilt screen joins this list in its step (TESTING_v26 V87).
 *
 * The width is compared with the set viewport, not with the document: mobile browsers widen the
 * layout viewport to fit content that is too wide, so the document alone would hide the overflow.
 */
const ZOOM_400 = { width: 320, height: 256 };

async function expectReflow(page: Page): Promise<void> {
  const tooWide = () =>
    page.evaluate((width) => {
      const pageWidth = Math.max(document.documentElement.scrollWidth, document.body.scrollWidth);
      const outside = [...document.querySelectorAll<HTMLElement>('a[href], button')]
        // The closed compact menu is hidden off-screen (`visibility: hidden`), so it does not count.
        .filter((element) => element.checkVisibility() && getComputedStyle(element).visibility === 'visible')
        .filter((element) => {
          const box = element.getBoundingClientRect();
          return box.left < 0 || box.right > width;
        })
        .map((element) => element.textContent?.trim() || element.tagName);
      return { pageWidth: pageWidth > width ? pageWidth : 0, outside };
    }, ZOOM_400.width);
  await expect.poll(tooWide).toEqual({ pageWidth: 0, outside: [] });
}

const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });

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

async function next(page: Page, heading: string | RegExp): Promise<void> {
  await page.getByRole('button', { name: 'Weiter' }).click();
  await expect(page.getByRole('heading', { level: 2, name: heading })).toBeFocused();
}

/** S10 steps 1–3 on the test release T.1 with HR-Anwendung, Website oder Webanwendung and Audio; ends on step 4. */
async function toStep4(page: Page): Promise<void> {
  await page.goto('/kriterien');
  await chooseUseCase(page, 'HR', 'HR-Anwendung');
  await next(page, 'Was möchten Sie beschaffen?');
  await page.getByRole('checkbox', { name: 'Website oder Webanwendung', exact: true }).press('Space');
  await next(page, 'Welche Funktionen hat das Produkt?');
  await page.getByRole('checkbox', { name: 'Audio', exact: true }).press('Space');
  await next(page, /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/);
}

test.describe('INV:A11Y-09 Reflow at 320 px / 400 % zoom', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(ZOOM_400);
  });

  for (const [path, heading] of [
    ['/', 'Barrierefreie Software beschaffen – Schritt für Schritt'],
    ['/gibt-es-nicht', 'Seite nicht gefunden'],
    ['/unauthorized', 'Kein Zugriff'],
    ['/error', 'Etwas ist schiefgelaufen'],
    ['/impressum', 'Impressum'],
    // Both H1s carry a soft hyphen (U94): match the part before it (Playwright matches a substring).
    ['/datenschutz', 'Datenschutz'],
    ['/barrierefreiheit', 'Erklärung zur Barriere'],
  ] as const) {
    test(`${path} („${heading}“) needs no horizontal scrolling`, async ({ page }) => {
      await page.goto(path);
      await expect(page.getByRole('heading', { level: 1, name: heading })).toBeVisible();

      await expectReflow(page);
    });
  }

  test('the open compact menu needs no horizontal scrolling and reaches every item', async ({ page }) => {
    await page.goto('/');
    await page.getByRole('button', { name: 'Menü', exact: true }).click();

    const nav = page.getByRole('navigation', { name: 'Hauptnavigation' });
    await expect(nav.getByRole('link', { name: 'Kriterien' })).toBeFocused();
    await expectReflow(page);
    // 256 px are too low for all items: the menu scrolls vertically, so each one can be reached.
    for (const item of [
      nav.getByRole('link', { name: 'Kriterien' }),
      nav.getByRole('link', { name: 'Produkte' }),
      nav.getByRole('link', { name: 'Beitragen' }),
      // „Anmelden“ follows the navigation in the menu, outside the nav landmark.
      page.getByRole('button', { name: 'Anmelden' }),
    ]) {
      await item.scrollIntoViewIfNeeded();
      await expect(item).toBeInViewport();
    }
  });

  test('the offline notice needs no horizontal scrolling', async ({ page }) => {
    // As in start-and-error-pages.spec.ts: the app is loaded first, then the server stops answering.
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    await page.route('**/api/**', (route) => route.abort('internetdisconnected'));
    await page.getByRole('button', { name: 'Menü', exact: true }).click();
    await page.getByRole('navigation', { name: 'Hauptnavigation' }).getByRole('link', { name: 'Kriterien' }).click();
    // S10 has a status region of its own (reset notice), so the notice is found by its text.
    await expect(page.getByRole('main').getByRole('status').filter({ hasText: 'Keine Verbindung zum Server' })).toBeVisible();

    await expectReflow(page);
  });

  test('S10 step 1 with the use-case list open needs no horizontal scrolling', async ({ page }) => {
    await page.goto('/kriterien');
    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeVisible();
    await combobox(page).focus();
    await expect(page.getByRole('listbox').getByRole('option', { name: 'HR-Anwendung' })).toBeVisible();

    await expectReflow(page);
  });

  test('S10 step 4 with „Bisherige Auswahl“ and the reset notice on step 1 need no horizontal scrolling', async ({ page }) => {
    await toStep4(page);
    await expect(page.getByRole('button', { name: 'Ändern' })).toHaveCount(3);
    await expectReflow(page);

    await page.getByRole('button', { name: 'Ändern', description: 'Verwendungszweck: HR-Anwendung', exact: true }).click();
    await combobox(page).fill('');
    await chooseUseCase(page, 'Report', 'Reporting-Anwendung');
    await expect(page.getByRole('main').getByRole('status').filter({ hasText: 'Auswahl zurückgesetzt' })).toBeVisible();

    await expectReflow(page);
  });

  test('the S11 placeholder needs no horizontal scrolling', async ({ page }) => {
    await toStep4(page);
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();

    await expectReflow(page);
  });
});

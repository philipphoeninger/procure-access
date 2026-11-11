// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page } from '@playwright/test';

/**
 * Reflow of the shell pages (A11Y-09; S2-18, R-SHL-10, WCAG 1.4.10): at 320 × 256 CSS px, which is a
 * 1280 × 1024 window at 400 % zoom, no page scrolls sideways and every link and button lies
 * inside the viewport. Covers the pages of the new shell only; the legacy screens are rebuilt
 * from step 2 on and are checked by hand until then (protocol S2-18). A rebuilt screen joins this
 * list in its step (TESTING_v26 V87).
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
    await expect(page.getByRole('main').getByRole('status')).toContainText('Keine Verbindung zum Server.');

    await expectReflow(page);
  });
});

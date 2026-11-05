// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page, type Route } from '@playwright/test';
import { recordThirdPartyRequests } from '../support/third-party-requests';

/**
 * Theme foundation (S3-09): light and dark appearance from the system setting (R-SET-02,
 * SPEC §7.5), strong focus indicators on Material components (Handoff §1.1, §1.6, #184), the
 * self-hosted Roboto font (Handoff §1.3) and reduced motion (Handoff §1.7). The axe run in the
 * dark appearance is A11Y-08 (S3-10).
 */
const COMPACT = { width: 360, height: 780 };
const API = '**/api/**';

const htmlState = (page: Page) =>
  page.evaluate(() => ({
    classes: [...document.documentElement.classList].filter((name) => name.startsWith('theme-')),
    colorScheme: getComputedStyle(document.documentElement).colorScheme,
  }));

const pageBackground = (page: Page) =>
  page.locator('mat-sidenav-container').evaluate((element) => getComputedStyle(element).backgroundColor);

test.describe('Appearance', () => {
  test('follows the system appearance and its changes without a reload', async ({ page, baseURL }) => {
    const thirdParty = recordThirdPartyRequests(page, new URL(baseURL!).origin);
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

    await expect.poll(() => htmlState(page)).toEqual({ classes: ['theme-dark'], colorScheme: 'dark' });
    const dark = await pageBackground(page);

    await page.emulateMedia({ colorScheme: 'light' });

    await expect.poll(() => htmlState(page)).toEqual({ classes: ['theme-light'], colorScheme: 'light' });
    await expect.poll(() => pageBackground(page)).not.toBe(dark);
    expect(thirdParty).toEqual([]);
  });

  test('shows the 3 px focus ring on a Material button reached with the keyboard', async ({ page }) => {
    await page.setViewportSize(COMPACT);
    await page.goto('/');

    const menu = page.getByRole('button', { name: 'Menü', exact: true });
    await menu.focus();
    await page.keyboard.press('Shift+Tab');
    await page.keyboard.press('Tab');
    await expect(menu).toBeFocused();

    // Material draws the ring on a child span.mat-focus-indicator; the global outline of plain
    // elements (main.scss) must not add a second one.
    const ring = await menu.evaluate((element) => {
      const indicator = getComputedStyle(element.querySelector('.mat-focus-indicator')!, '::before');
      return {
        display: indicator.display,
        width: indicator.borderTopWidth,
        style: indicator.borderTopStyle,
        outline: getComputedStyle(element).outlineStyle,
      };
    });
    expect(ring).toEqual({ display: 'block', width: '3px', style: 'solid', outline: 'none' });
  });

  test('sets the text in the self-hosted Roboto', async ({ page }) => {
    await page.goto('/');
    const heading = page.getByRole('heading', { level: 1 });
    await expect(heading).toBeVisible();

    expect(await heading.evaluate((element) => getComputedStyle(element).fontFamily)).toMatch(/^"?Roboto"?,/);
    await expect
      .poll(() =>
        page.evaluate(async () => {
          await document.fonts.ready;
          return [...document.fonts].some((face) => face.family.replace(/"/g, '') === 'Roboto' && face.status === 'loaded');
        }),
      )
      .toBe(true);
  });

  for (const reducedMotion of ['reduce', 'no-preference'] as const) {
    test(`loading bar ${reducedMotion === 'reduce' ? 'stands still' : 'moves'} with reduced motion „${reducedMotion}“`, async ({ page }) => {
      await page.emulateMedia({ reducedMotion });
      let release!: () => void;
      const released = new Promise<void>((resolve) => (release = resolve));
      await page.route(API, async (route: Route) => {
        await released;
        await route.continue();
      });

      await page.goto('/filters');
      const bar = page.getByRole('main').getByRole('progressbar', { name: 'Wird geladen …' });
      await expect(bar).toBeVisible();

      const running = () =>
        bar.evaluate((element) => element.getAnimations({ subtree: true }).filter((a) => a.playState === 'running').length);
      if (reducedMotion === 'reduce') {
        expect(await running()).toBe(0);
      } else {
        await expect.poll(running).toBeGreaterThan(0);
      }

      release();
      await expect(page.getByRole('progressbar')).toBeHidden();
    });
  }
});

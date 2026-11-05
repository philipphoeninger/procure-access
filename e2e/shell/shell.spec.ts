// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * App shell of step 1 (S2-15): landmarks and skip link (R-SHL-01), header in the Anonymous state
 * with its compact layout below 600 px (R-SHL-02) and the footer (R-SHL-06), with axe on every
 * state (A11Y-01). Each test sets its own viewport, so the wide and the compact header run in
 * every browser project.
 */
const WIDE = { width: 1280, height: 800 };
const COMPACT = { width: 360, height: 780 };

test.describe('App shell', () => {
  test('INV:A11Y-01 INV:A11Y-08 skip link, landmarks and footer on the start page', async ({ page }, testInfo) => {
    await page.setViewportSize(WIDE);
    await page.goto('/');

    await expect(page.getByRole('banner')).toBeVisible();
    await expect(page.getByRole('navigation', { name: 'Hauptnavigation' })).toBeVisible();
    await expect(page.getByRole('main')).toHaveAttribute('id', 'main');
    await expect(page.getByRole('contentinfo')).toBeVisible();

    // The skip link is the first stop of the Tab key and is visible while it has the focus.
    const skipLink = page.getByRole('link', { name: 'Zum Inhalt springen' });
    await page.keyboard.press('Tab');
    await expect(skipLink).toBeFocused();
    await expect(skipLink).toBeInViewport();
    await expectNoAxeViolations(page, testInfo, 'start page, skip link focused');

    await page.keyboard.press('Enter');
    await expect(page.getByRole('main')).toBeFocused();
    await expect(page).toHaveURL(/:\d+\/$/);
  });

  test('INV:A11Y-01 INV:A11Y-08 marks the current page in the main navigation', async ({ page }, testInfo) => {
    await page.setViewportSize(WIDE);
    await page.goto('/products');

    const nav = page.getByRole('navigation', { name: 'Hauptnavigation' });
    await expect(nav.getByRole('link', { name: 'Produkte' })).toHaveAttribute('aria-current', 'page');
    await expect(nav.getByRole('link', { name: 'Kriterien' })).not.toHaveAttribute('aria-current');
    await expectNoAxeViolations(page, testInfo, 'products, nav item current');
  });

  test('INV:A11Y-01 INV:A11Y-08 compact menu opens on the first item and Esc returns to „Menü“', async ({ page }, testInfo) => {
    await page.setViewportSize(COMPACT);
    await page.goto('/');

    await page.keyboard.press('Tab');
    await expect(page.getByRole('link', { name: 'Zum Inhalt springen' })).toBeFocused();

    const menu = page.getByRole('button', { name: 'Menü', exact: true });
    await expect(menu).toHaveAttribute('aria-expanded', 'false');
    await expect(page.getByRole('navigation', { name: 'Hauptnavigation' })).toBeHidden();

    await menu.click();
    await expect(menu).toHaveAttribute('aria-expanded', 'true');
    await expect(page.getByRole('link', { name: 'Kriterien', exact: true })).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 'compact menu open');

    await page.keyboard.press('Escape');
    await expect(menu).toHaveAttribute('aria-expanded', 'false');
    await expect(menu).toBeFocused();

    await menu.click();
    await page.getByRole('button', { name: 'Menü schließen' }).click();
    await expect(menu).toHaveAttribute('aria-expanded', 'false');
    await expect(menu).toBeFocused();
  });

  test('INV:A11Y-01 INV:A11Y-08 legal pages are placeholders reachable from the footer', async ({ page }, testInfo) => {
    await page.setViewportSize(COMPACT);
    await page.goto('/');

    for (const [link, title] of [
      ['Impressum', 'Impressum'],
      ['Datenschutz', 'Datenschutzhinweise'],
      ['Erklärung zur Barrierefreiheit', 'Erklärung zur Barrierefreiheit'],
    ]) {
      await page.getByRole('contentinfo').getByRole('link', { name: link }).click();
      await expect(page.getByRole('heading', { level: 1, name: title })).toBeVisible();
      await expect(page.getByText('Platzhalter: Der Inhalt dieser Seite fehlt noch.')).toBeVisible();
    }
    await expectNoAxeViolations(page, testInfo, 'legal page placeholder, 360 px');
  });
});

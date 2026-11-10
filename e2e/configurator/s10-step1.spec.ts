// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * S10 frame and step 1 (S4-10; R-CON-01, R-CON-02) on the active test release T.1 of the E2E
 * stack, with axe in both appearances on every state (A11Y-01, A11Y-08). The page is not linked
 * until S4-12, so the tests open `/kriterien` directly; J1 moves onto it in S4-13.
 */
const WIDE = { width: 1280, height: 800 };
const COMPACT = { width: 360, height: 780 };

const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });
const options = (page: Page) => page.getByRole('listbox').getByRole('option');
const NONE = 'Keiner davon passt Es gelten die allgemeinen Empfehlungen.';

/** Focus opens the list (a click at the middle of a narrow field lands on its label). */
async function openList(page: Page): Promise<void> {
  await combobox(page).focus();
  await expect(page.getByRole('listbox')).toBeVisible();
}

/** Waits until Material's enter animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

async function openPage(page: Page, viewport: { width: number; height: number }): Promise<void> {
  await page.setViewportSize(viewport);
  await page.goto('/kriterien');
  await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeVisible();
}

test.describe('S10 Kriterien zusammenstellen, step 1', () => {
  test('INV:A11Y-01 INV:A11Y-08 shows the four steps and the combobox with „Keiner davon passt“ last (wide)', async ({ page }, testInfo) => {
    await openPage(page, WIDE);

    const steps = page.getByRole('main').getByRole('list').first().getByRole('listitem');
    await expect(steps).toHaveText([/Verwendungszweck/, /Anwendungstyp/, /Funktionsbereiche/, /Prüfverfahren \(optional\)/]);
    await expect(steps.first()).toHaveAttribute('aria-current', 'step');
    await expect(page.getByRole('tab')).toHaveCount(0);
    await expectNoAxeViolations(page, testInfo, 's10 step 1, wide');

    await openList(page);
    await expect(options(page).last()).toHaveAccessibleName(NONE);
    await expect(page.getByRole('option', { name: 'HR-Anwendung' })).toBeVisible();
    await expectNoAxeViolations(page, testInfo, 's10 step 1, list open');

    await combobox(page).fill('Reporting');
    await expect(options(page)).toHaveCount(2);
    await expect(options(page).first()).toHaveAccessibleName('Reporting-Anwendung');
    await expect(options(page).last()).toHaveAccessibleName(NONE);
  });

  test('INV:A11Y-01 INV:A11Y-08 „Weiter“ without an answer shows the field error (S10-E)', async ({ page }, testInfo) => {
    await openPage(page, WIDE);

    await page.getByRole('button', { name: 'Weiter' }).click();

    await expect(page.getByText('Bitte wählen Sie einen Verwendungszweck oder „Keiner davon passt“.')).toBeVisible();
    await expect(combobox(page)).toBeFocused();
    await expect(combobox(page)).toHaveAttribute('aria-invalid', 'true');
    await settled(page.getByRole('main'));
    await expectNoAxeViolations(page, testInfo, 's10 step 1, error');
  });

  test('INV:A11Y-01 INV:A11Y-08 a use case chosen by keyboard leads to step 2', async ({ page }, testInfo) => {
    await openPage(page, WIDE);

    await openList(page);
    await page.keyboard.type('HR');
    // Wait for the filtered list: an arrow key before it would mark an option that is replaced.
    await expect(options(page)).toHaveCount(2);
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    await expect(combobox(page)).toHaveValue('HR-Anwendung');

    await page.keyboard.press('Tab');
    await expect(page.getByRole('button', { name: 'Weiter' })).toBeFocused();
    await page.keyboard.press('Enter');

    await expect(page.getByRole('heading', { level: 2, name: 'Was möchten Sie beschaffen?' })).toBeFocused();
    const steps = page.getByRole('main').getByRole('list').first().getByRole('listitem');
    await expect(steps.nth(1)).toHaveAttribute('aria-current', 'step');
    await expect(steps.first()).toContainText('erledigt');
    await expectNoAxeViolations(page, testInfo, 's10 step 2 frame');
  });

  test('INV:A11Y-01 INV:A11Y-08 shows the step as a text line at 360 px (S10-M)', async ({ page }, testInfo) => {
    await openPage(page, COMPACT);

    await expect(page.getByText('Schritt 1 von 4: Verwendungszweck')).toBeVisible();
    await expect(page.getByRole('main').getByRole('listitem')).toHaveCount(0);
    await expectNoAxeViolations(page, testInfo, 's10 step 1, 360 px');

    await openList(page);
    await expect(options(page).last()).toHaveAccessibleName(NONE);
    await expectNoAxeViolations(page, testInfo, 's10 step 1, 360 px, list open');
  });
});

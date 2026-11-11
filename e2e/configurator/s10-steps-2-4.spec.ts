// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { chooseFirstOption } from '../support/combobox';

/**
 * S10 steps 2–4 (S4-11; R-CON-03 – R-CON-06) on the active test release T.1 of the E2E stack,
 * by keyboard, with axe in both appearances on every step (A11Y-01, A11Y-08). T.1 excludes
 * Hardware and Video for HR-Anwendung, the App-Test for Website oder Webanwendung and the
 * Software-Test for Audio, so one path meets an exclusion by each higher layer (CFG-02).
 */
const WIDE = { width: 1280, height: 800 };
const COMPACT = { width: 360, height: 780 };

const checkbox = (page: Page, name: string) => page.getByRole('checkbox', { name, exact: true });
const next = (page: Page) => page.getByRole('button', { name: 'Weiter' });

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Opens S10 and answers step 1 with HR-Anwendung by keyboard. */
async function toStep2(page: Page, viewport: { width: number; height: number }): Promise<void> {
  await page.setViewportSize(viewport);
  await page.goto('/kriterien');
  const combobox = page.getByRole('combobox', { name: 'Verwendungszweck' });
  await combobox.focus();
  await page.keyboard.type('HR');
  await expect(page.getByRole('listbox').getByRole('option')).toHaveCount(2);
  await chooseFirstOption(page, combobox, 'HR-Anwendung');
  await next(page).click();
  await expect(page.getByRole('heading', { level: 2, name: 'Was möchten Sie beschaffen?' })).toBeFocused();
}

/** Space on a focused checkbox, as a keyboard user activates it. */
async function pressSpace(box: Locator): Promise<void> {
  await box.focus();
  await box.press('Space');
}

/** An excluded option stays focusable, is aria-disabled with its reason, and Space changes nothing. */
async function expectExcluded(box: Locator, reason: string): Promise<void> {
  await expect(box).toHaveAttribute('aria-disabled', 'true');
  // Not `disabled`: Playwright's toBeEnabled() would count aria-disabled as disabled too.
  await expect(box).not.toHaveAttribute('disabled');
  await expect(box).toHaveAccessibleDescription(reason);
  await pressSpace(box);
  await expect(box).toBeFocused();
  await expect(box).not.toBeChecked();
}

test.describe('S10 Kriterien zusammenstellen, steps 2–4', () => {
  test('INV:CFG-02 INV:A11Y-01 INV:A11Y-08 excluded options stay focusable with their reason in steps 2–4', async ({ page }, testInfo) => {
    await toStep2(page, WIDE);

    // Step 2: Hardware is excluded by the use case; the hint names exclusions.
    await expect(page.getByText(/Einige passen nicht zum gewählten Verwendungszweck/)).toBeVisible();
    await expect(checkbox(page, 'Software (geschlossen)')).toHaveAccessibleDescription(
      'Eigene Hilfsmittel wie Screenreader lassen sich nicht verwenden, etwa bei Kiosk-Terminals.',
    );
    await expectExcluded(checkbox(page, 'Hardware'), 'Passt nicht zum Verwendungszweck HR-Anwendung');
    await expectNoAxeViolations(page, testInfo, 's10 step 2');

    await pressSpace(checkbox(page, 'Website oder Webanwendung'));
    await expect(checkbox(page, 'Website oder Webanwendung')).toBeChecked();
    await next(page).click();

    // Step 3: Basisfunktionalität is locked, Video is excluded by the use case.
    await expect(page.getByRole('heading', { level: 2, name: 'Welche Funktionen hat das Produkt?' })).toBeFocused();
    const base = checkbox(page, 'Basisfunktionalität');
    await expect(base).toBeChecked();
    await expect(base).toHaveAttribute('aria-disabled', 'true');
    await expect(base).toHaveAccessibleDescription('Immer ausgewählt.');
    await expectExcluded(checkbox(page, 'Video'), 'Passt nicht zum Verwendungszweck HR-Anwendung');
    // Unnamed on purpose: in Firefox 159 (Playwright 1.65) any name check of this mat-slide-toggle
    // followed by an axe run crashes the page. It is the only switch; the unit test checks its name.
    await page.getByRole('switch').click();
    await expect(checkbox(page, 'Audio')).toHaveAccessibleDescription(/akustische/);
    await settled(page.getByRole('main'));
    await expectNoAxeViolations(page, testInfo, 's10 step 3, explanations shown');

    await pressSpace(checkbox(page, 'Audio'));
    await expect(checkbox(page, 'Audio')).toBeChecked();
    await next(page).click();

    // Step 4: the App-Test is excluded by the app type, the Software-Test by the product part.
    await expect(page.getByRole('heading', { level: 2, name: /Soll das Produkt nach einem bestimmten Verfahren/ })).toBeFocused();
    await expectExcluded(checkbox(page, 'BIT inklusiv App-Test'), 'Passt nicht zum Anwendungstyp Website oder Webanwendung');
    await expectExcluded(checkbox(page, 'BIT inklusiv Software-Test'), 'Passt nicht zum Funktionsbereich Audio');
    await expect(checkbox(page, 'BIK BITV-Test (Web)')).toHaveAccessibleDescription('Für Websites und Webanwendungen.');
    const link = page.getByRole('link', { name: 'Verfahren ansehen (bitvtest.de) (öffnet in neuem Tab)' }).first();
    await expect(link).toHaveAttribute('target', '_blank');
    await expectNoAxeViolations(page, testInfo, 's10 step 4');
  });

  test('INV:A11Y-01 INV:A11Y-08 „Weiter“ without an app type shows the error and focuses the first option', async ({ page }, testInfo) => {
    await toStep2(page, WIDE);

    await next(page).click();

    await expect(page.getByText('Bitte wählen Sie mindestens einen Anwendungstyp.')).toBeVisible();
    await expect(checkbox(page, 'Website oder Webanwendung')).toBeFocused();
    await expectNoAxeViolations(page, testInfo, 's10 step 2, error');
  });

  test('INV:A11Y-01 INV:A11Y-08 step 2 at 360 px', async ({ page }, testInfo) => {
    await toStep2(page, COMPACT);

    await expect(page.getByText('Schritt 2 von 4: Anwendungstyp')).toBeVisible();
    await expect(checkbox(page, 'Hardware')).toHaveAttribute('aria-disabled', 'true');
    await expectNoAxeViolations(page, testInfo, 's10 step 2, 360 px');
  });
});

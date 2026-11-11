// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { chooseFirstOption } from '../support/combobox';

/**
 * S11 around the list (S4-18a; R-LST-09, R-LST-10, R-LST-13) on the active test release T.1 of the
 * E2E stack: summary, notices, counts, „Kriterium vorschlagen“ and the action bar, which is in the
 * flow below 600 px width or height and sticky from there without covering the focused element
 * (WCAG 2.4.11). Axe in both appearances (A11Y-01, A11Y-08). Since S4-18b also the usage event
 * `CriteriaListShown`: one report per new selection.
 *
 * Axe runs before any name-based locator on S11: in Firefox such locators followed by axe crashed
 * the page process (S4-16 P5, the class of TESTING_v43 V129).
 */
const WIDE = { width: 1280, height: 800 };

const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });
const actionBar = (page: Page) => page.locator('.pa-criteria-list__actions');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

async function pressSpace(box: Locator): Promise<void> {
  await box.focus();
  await box.press('Space');
  await expect(box).toBeChecked();
}

/** S10 with HR-Anwendung, Website oder Webanwendung and Audio, then „Kriterienliste anzeigen“. */
async function toList(page: Page): Promise<void> {
  await page.setViewportSize(WIDE);
  await page.goto('/kriterien');
  await combobox(page).focus();
  await page.keyboard.type('HR');
  await chooseFirstOption(page, combobox(page), 'HR-Anwendung');
  await page.getByRole('button', { name: 'Weiter' }).click();
  await pressSpace(page.getByRole('checkbox', { name: 'Website oder Webanwendung', exact: true }));
  await page.getByRole('button', { name: 'Weiter' }).click();
  await pressSpace(page.getByRole('checkbox', { name: 'Audio', exact: true }));
  await page.getByRole('button', { name: 'Weiter' }).click();
  await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
  await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
  await expect(page.getByRole('main').getByRole('list')).toBeVisible();
}

/** The position of the action bar and how far its top is from the bottom of the viewport. */
function barPlacement(page: Page) {
  return actionBar(page).evaluate((bar) => ({
    position: getComputedStyle(bar).position,
    gapToViewportBottom: Math.round(window.innerHeight - bar.getBoundingClientRect().bottom),
  }));
}

test.describe('S11 summary, notices, suggestion link and action bar', () => {
  test('INV:A11Y-01 INV:A11Y-08 shows summary, notices and counts above the list and the link and action bar after it', async ({ page }, testInfo) => {
    await toList(page);
    await settled(page.getByRole('main'));
    await expectNoAxeViolations(page, testInfo, 's11 list');

    await expect(page).toHaveTitle('Ihre Kriterienliste – ProcureAccess');
    const main = page.getByRole('main');
    await expect(main.getByText(/^\d+ Kriterien? für Ihre Auswahl$/)).toBeVisible();
    await expect(main.getByRole('term')).toHaveText(['Verwendungszweck', 'Anwendungstyp', 'Funktionsbereiche']);
    await expect(main.getByRole('definition')).toHaveText(['HR-Anwendung', 'Website oder Webanwendung', 'Basisfunktionalität und Audio']);
    await expect(main.getByText(/^Die Prioritäten sind eine Empfehlung des Projektteams/)).toBeVisible();
    await expect(main.getByText(/^Ihre Auswahl wird nicht gespeichert, solange Sie nicht angemeldet sind/)).toBeVisible();
    await expect(main.getByText(/\d+ Mindestanforderungen?, \d+ Bewertungskriteri(um|en), \d+ Hinweise?/)).toBeVisible();

    const suggest = main.getByRole('link', { name: /^Fehlt ein Kriterium oder ist eine Formulierung unklar\?/ });
    await expect(suggest).toHaveAttribute('href', /^(mailto:[^?]+\?subject=Vorschlag%20f%C3%BCr%20den%20Kriterienkatalog|https:)/);
    await expect(actionBar(page)).toContainText(/^\s*(\d+) von \1 Kriterien in Ihrer Liste/);
    await expect(main.getByRole('button', { name: 'Liste speichern' })).toHaveCount(0);
  });

  test('the action bar is sticky from 600 px width and height and in the flow below', async ({ page }) => {
    await toList(page);
    await page.evaluate(() => window.scrollTo(0, 0));
    expect(await barPlacement(page)).toEqual({ position: 'sticky', gapToViewportBottom: 0 });

    await page.setViewportSize({ width: 1280, height: 560 });
    await page.evaluate(() => window.scrollTo(0, 0));
    expect((await barPlacement(page)).position).toBe('static');

    await page.setViewportSize({ width: 560, height: 800 });
    await page.evaluate(() => window.scrollTo(0, 0));
    expect((await barPlacement(page)).position).toBe('static');
  });

  test('the sticky action bar never covers the element with the keyboard focus (WCAG 2.4.11)', async ({ page }) => {
    await toList(page);
    const exportButton = page.getByRole('button', { name: 'Liste exportieren' });
    let steps = 0;
    while (!(await exportButton.evaluate((button) => button === document.activeElement))) {
      await page.keyboard.press('Tab');
      expect(++steps, 'Tab reaches „Liste exportieren“').toBeLessThan(200);
      const covered = await page.evaluate(() => {
        const focused = document.activeElement;
        const bar = document.querySelector('.pa-criteria-list__actions');
        if (!(focused instanceof HTMLElement) || !bar || bar.contains(focused)) return null;
        const box = focused.getBoundingClientRect();
        const barBox = bar.getBoundingClientRect();
        return box.bottom > barBox.top + 1 && box.top < barBox.bottom ? focused.outerHTML.slice(0, 120) : null;
      });
      expect(covered, 'focused element hidden behind the action bar').toBeNull();
    }
  });

  test('reports the shown list once (CriteriaListShown) and not again after a return without a change', async ({ page }) => {
    // S4-18b, R-MET-01, R-CON-10, owner decision O-S4-55 (a).
    const isReport = (request: { url(): string; method(): string }) =>
      request.url().endsWith('/api/v1/configurator/criteria-list-shown') && request.method() === 'POST';
    // Counted when sent, not when answered, so a second report shows up at once.
    let sent = 0;
    page.on('request', (request) => {
      if (isReport(request)) sent++;
    });
    const answered = page.waitForResponse((response) => isReport(response.request()));

    await toList(page);
    expect((await answered).status()).toBe(204);

    await page.getByRole('link', { name: 'Auswahl ändern' }).click();
    await expect(combobox(page)).toHaveValue('HR-Anwendung');
    for (let step = 0; step < 3; step++) {
      await page.getByRole('button', { name: 'Weiter' }).click();
    }
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    await expect(page.getByRole('heading', { level: 1 })).toBeFocused();
    await expect(page.getByRole('main').getByRole('list')).toBeVisible();
    // A wrong report would be sent in the task that applies the list; a short bounded wait covers it.
    await page.waitForTimeout(500);

    expect(sent).toBe(1);
  });

  test('„Liste exportieren“ opens the placeholder dialog by keyboard; Escape closes it and returns the focus', async ({ page }) => {
    await toList(page);
    const exportButton = page.getByRole('button', { name: 'Liste exportieren' });
    await exportButton.focus();
    await page.keyboard.press('Enter');

    const dialog = page.getByRole('dialog', { name: 'Kriterienliste exportieren' });
    await expect(dialog).toBeVisible();
    await expect(dialog.getByRole('button', { name: 'Schließen' })).toBeFocused();
    await page.keyboard.press('Escape');

    await expect(dialog).toHaveCount(0);
    await expect(exportButton).toBeFocused();
  });
});

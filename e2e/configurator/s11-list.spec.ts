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
 * `CriteriaListShown`: one report per new selection. Since S5-31 the rating legend above the list;
 * since S5-32 the title row and the sticky bar across the viewport (Figma `2020:102`, `2034:1193`).
 *
 * Axe runs before any name-based locator on S11: in Firefox such locators followed by axe crashed
 * the page process (S4-16 P5, the class of TESTING_v43 V129).
 */
const WIDE = { width: 1280, height: 800 };

const combobox = (page: Page) => page.getByRole('combobox', { name: 'Verwendungszweck' });
const actionBar = (page: Page) => page.locator('pa-criteria-action-bar');

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

/**
 * The colours at the given x of a one-pixel-high screenshot, as `[r, g, b]`. The PNG is decoded in
 * the page through a canvas, so the spec needs no image library.
 */
function pixelsAt(page: Page, png: Buffer, xs: number[]): Promise<number[][]> {
  return page.evaluate(
    async ({ data, xs }) => {
      const image = await createImageBitmap(await (await fetch(`data:image/png;base64,${data}`)).blob());
      const canvas = new OffscreenCanvas(image.width, image.height);
      const context = canvas.getContext('2d');
      if (!context) throw new Error('no 2d context');
      context.drawImage(image, 0, 0);
      return xs.map((x) => [...context.getImageData(x, 0, 1, 1).data.slice(0, 3)]);
    },
    { data: png.toString('base64'), xs },
  );
}

/** `rgb(r, g, b)` as `[r, g, b]`. */
const channels = (colour: string): number[] => (colour.match(/\d+/g) ?? []).slice(0, 3).map(Number);

/** Whether two colours differ by at most 2 per channel (colour management of the screenshot). */
const sameColour = (a: number[], b: number[]): boolean => a.length === 3 && a.every((value, i) => Math.abs(value - b[i]) <= 2);

test.describe('S11 summary, notices, suggestion link and action bar', () => {
  test('„Auswahl ändern“ is on the H1 line from 600 px and below it on narrow screens, after it in the DOM (S5-32, #278)', async ({ page }) => {
    await toList(page);
    const place = () =>
      page.evaluate(() => {
        const h1 = document.querySelector('main h1');
        const link = h1?.parentElement?.querySelector('a');
        if (!h1 || !link) return null;
        const heading = h1.getBoundingClientRect();
        const button = link.getBoundingClientRect();
        const row = h1.parentElement!.getBoundingClientRect();
        return {
          domOrder: !!(h1.compareDocumentPosition(link) & Node.DOCUMENT_POSITION_FOLLOWING),
          sameLine: button.top < heading.bottom && button.bottom > heading.top,
          below: button.top >= heading.bottom,
          atStart: Math.round(button.left) === Math.round(heading.left),
          atEnd: Math.round(button.right) === Math.round(row.right),
        };
      });

    expect(await place()).toEqual({ domOrder: true, sameLine: true, below: false, atStart: false, atEnd: true });

    await page.setViewportSize({ width: 360, height: 800 });
    expect(await place()).toEqual({ domOrder: true, sameLine: false, below: true, atStart: true, atEnd: false });
  });

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
    // The draft of S11 is never saved itself (S5-22, owner decision S5-22 4).
    await expect(actionBar(page).getByText('Noch nicht gespeichert', { exact: true })).toBeVisible();
    // „Liste speichern“ since S5-20b, before „Liste exportieren“; signed out it opens S01 (R-SAV-01).
    await expect(actionBar(page).getByRole('button')).toHaveText(['Liste speichern', 'Liste exportieren']);
  });

  test('INV:A11Y-01 INV:A11Y-08 explains the rating levels once, between the filter row and the cards (S5-31)', async ({ page }, testInfo) => {
    // Owner decisions #279 (b), O-S5-31-1: one collapsed legend per page with the levels the list allows.
    await toList(page);
    const legend = page.locator('pa-rating-legend');
    await expect(legend).toHaveCount(1);
    const toggle = legend.locator('button');
    await expect(toggle).toHaveAttribute('aria-expanded', 'false');
    await toggle.click();
    await expect(toggle).toHaveAttribute('aria-expanded', 'true');
    await expect(legend.locator('dl')).toBeVisible();
    await settled(page.getByRole('main'));
    await expectNoAxeViolations(page, testInfo, 's11 rating legend open');

    await expect(toggle).toHaveText('Erklärung der Bewertungsstufen ausblenden');
    await expect(legend.getByRole('term').first()).toHaveText('Erfüllt');
    // Nothing on S11 is assessed: „Nicht eingeschätzt“ is never a level a criterion allows.
    await expect(legend.getByRole('term').filter({ hasText: 'Nicht eingeschätzt' })).toHaveCount(0);
    await expect(legend.getByText(/^Welche Stufen bei einem Kriterium möglich sind/)).toBeVisible();
    const order = await page.evaluate(() => {
      const filter = document.querySelector('pa-criteria-list-filter');
      const legendHost = document.querySelector('pa-rating-legend');
      const cards = document.querySelector('main ul');
      return (
        !!filter && !!legendHost && !!cards &&
        !!(filter.compareDocumentPosition(legendHost) & Node.DOCUMENT_POSITION_FOLLOWING) &&
        !!(legendHost.compareDocumentPosition(cards) & Node.DOCUMENT_POSITION_FOLLOWING)
      );
    });
    expect(order, 'filter row, legend, cards').toBe(true);
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

  test('while sticky the action bar spans the viewport without corners, its content in the column (S5-32, #281)', async ({ page }) => {
    await toList(page);
    await page.evaluate(() => window.scrollTo(0, 0));
    const bar = await actionBar(page).evaluate((element) => {
      const box = element.getBoundingClientRect();
      const main = document.querySelector('main')?.getBoundingClientRect();
      return {
        radius: getComputedStyle(element).borderTopLeftRadius,
        colour: getComputedStyle(element).backgroundColor,
        left: Math.round(box.left),
        right: Math.round(box.right),
        mainLeft: Math.round(main?.left ?? 0),
        y: Math.round(box.top + box.height / 2),
        width: document.documentElement.clientWidth,
      };
    });
    expect(bar.radius).toBe('0px');
    // The box stays in the content column, away from both edges of the 1280 px viewport.
    expect(bar.left).toBeGreaterThan(bar.mainLeft);
    expect(bar.right).toBeLessThan(bar.width);
    // Its colour reaches both edges of the viewport, on the bar's line.
    // In CSS pixels, so x is the same on the mobile projects (device scale > 1).
    const shot = await page.screenshot({ clip: { x: 0, y: bar.y, width: bar.width, height: 1 }, scale: 'css' });
    const edges = await pixelsAt(page, shot, [2, bar.width - 3]);
    for (const edge of edges) {
      expect(sameColour(edge, channels(bar.colour)), `rgb(${edge.join(', ')}) at the edge, bar ${bar.colour}`).toBe(true);
    }
    // Nothing to scroll to sideways.
    expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBe(bar.width);

    // In the flow (below 600 px height) it keeps its corners.
    await page.setViewportSize({ width: 1280, height: 560 });
    await expect(actionBar(page)).not.toHaveCSS('border-top-left-radius', '0px');
  });

  test('in forced colours the sticky action bar shows a top edge towards the list (WCAG 1.4.11, #378)', async ({ page, browserName }) => {
    test.skip(browserName !== 'chromium', 'forced-colors emulation is Chromium only');
    await toList(page);
    await page.emulateMedia({ forcedColors: 'active' });
    const edge = (): Promise<number> => actionBar(page).evaluate((bar) => parseFloat(getComputedStyle(bar).borderTopWidth));

    expect(await edge()).toBeGreaterThan(0);
    // In the flow nothing changes, and neither does the bar outside forced colours.
    await page.setViewportSize({ width: 1280, height: 560 });
    expect(await edge()).toBe(0);
    await page.setViewportSize(WIDE);
    await page.emulateMedia({ forcedColors: 'none' });
    expect(await edge()).toBe(0);
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
        const bar = document.querySelector('pa-criteria-action-bar');
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
});

/**
 * The export dialog S13 (S4-21b; R-LST-11), by keyboard on T.1. Axe in both appearances on the
 * dialog and its error state S13-E (A11Y-01, A11Y-08). The dialog is found by role before axe runs,
 * its texts only afterwards (Firefox, see above).
 */
test.describe('S13 export dialog', () => {
  const XLSX = 'Excel-Datei (.xlsx) – für Vergabeunterlagen';
  const CSV = 'CSV-Datei – für andere Programme';

  /** Opens S13 from the action bar by keyboard and returns the button that opened it. */
  async function openExport(page: Page): Promise<Locator> {
    await toList(page);
    const exportButton = actionBar(page).getByRole('button', { name: 'Liste exportieren' });
    await exportButton.focus();
    await page.keyboard.press('Enter');
    // The dialog moves the focus to the selected format once it is open; keys go there only then.
    await expect(page.getByRole('dialog').locator('input[type="radio"]:checked')).toBeFocused();
    return exportButton;
  }

  test('INV:A11Y-01 INV:A11Y-08 opens by keyboard on the preselected XLSX format; Escape closes it and returns the focus', async ({ page }, testInfo) => {
    const exportButton = await openExport(page);
    const dialog = page.getByRole('dialog');
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's13 export dialog');

    await expect(dialog).toHaveAccessibleName('Kriterienliste exportieren');
    const xlsx = dialog.getByRole('radio', { name: XLSX });
    await expect(xlsx).toBeChecked();
    await expect(xlsx).toBeFocused();
    await expect(dialog.getByRole('group', { name: 'Dateiformat' })).toBeVisible();
    await expect(dialog).toContainText('Die Datei nennt die Katalogversion T.1.');
    await expect(dialog).toContainText('ProcureAccess speichert davon nichts.');
    await page.keyboard.press('Escape');

    await expect(dialog).toHaveCount(0);
    await expect(exportButton).toBeFocused();
  });

  test('downloads the CSV chosen by keyboard, then closes and returns the focus', async ({ page }) => {
    const exportButton = await openExport(page);
    const dialog = page.getByRole('dialog');
    await page.keyboard.press('ArrowDown');
    await expect(dialog.getByRole('radio', { name: CSV })).toBeChecked();
    await page.keyboard.press('Tab');
    await expect(dialog.getByRole('button', { name: 'Abbrechen' })).toBeFocused();
    await page.keyboard.press('Tab');
    await expect(dialog.getByRole('button', { name: 'Datei herunterladen' })).toBeFocused();

    const download = page.waitForEvent('download');
    await page.keyboard.press('Enter');

    expect((await download).suggestedFilename()).toMatch(/^ProcureAccess_Kriterien_T\.1_\d{4}-\d{2}-\d{2}\.csv$/);
    await expect(dialog).toHaveCount(0);
    await expect(exportButton).toBeFocused();
  });

  test('INV:A11Y-01 INV:A11Y-08 shows a rate limit as an alert above the actions, without a snackbar, and keeps the focus on „Erneut versuchen“', async ({ page }, testInfo) => {
    await page.route('**/api/v1/exports', (route) =>
      route.fulfill({
        status: 429,
        contentType: 'application/problem+json',
        body: JSON.stringify({ type: 'https://procureaccess.de/problems/rate-limit', status: 429, retryAfterMinutes: 1 }),
      }),
    );
    await openExport(page);
    const dialog = page.getByRole('dialog');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Tab');
    await page.keyboard.press('Enter');
    const alert = dialog.getByRole('alert');
    await expect(alert).toBeVisible();
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's13 export error');

    await expect(alert).toContainText('Export fehlgeschlagen');
    await expect(alert).toContainText('Bitte warten Sie 1 Minute und versuchen Sie es dann erneut.');
    await expect(dialog.getByRole('button', { name: 'Erneut versuchen' })).toBeFocused();
    await expect(page.locator('mat-snack-bar-container')).toHaveCount(0);
  });
});

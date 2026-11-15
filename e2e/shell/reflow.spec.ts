// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page } from '@playwright/test';
import { chooseFirstOption } from '../support/combobox';
import { expireSessionOnce, signInThroughApi } from '../support/session';
import { CONFIRMED_USER, E2E_ORGANIZATION, ORG_OWNER, TEST_USER_PASSWORD } from '../support/users';

/**
 * Reflow of the shell pages (A11Y-09; S2-18, R-SHL-10, WCAG 1.4.10): at 320 × 256 CSS px, which is a
 * 1280 × 1024 window at 400 % zoom, no page scrolls sideways and every link and button lies
 * inside the viewport. Covers the pages of the new shell and the rebuilt screens: S10 since S4-13,
 * S11 with its summary, notices, detail panel and action bar since S4-18a, the export dialog S13
 * (full screen below 600 px, S13-M) since S4-21b, the sign-in page S01 (#170 L1) and the login
 * overlay since S5-09, the registration S02 and its confirmation pages since S5-12b, the password reset
 * S03 with both steps since S5-13b, the organisation pages S20 and S21 (with the error summary, the
 * last-Owner notice and the confirmation dialog) since S5-16b, S21's open invitations and invite form
 * and the page of an invitation link since S5-17b. The legacy screens are
 * checked by hand until they are rebuilt (protocol S2-18). A rebuilt screen joins this list in its
 * step (TESTING_v26 V87).
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
  await chooseFirstOption(page, combobox(page), name);
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
    ['/registrieren', 'Konto erstellen'],
    // Without a token the page shows its „invalid“ result and sends nothing (S5-12b)
    ['/registrieren/bestaetigen', 'Link nicht mehr gültig'],
    ['/passwort', 'Passwort zurücksetzen'],
    // Without a token in memory the new-password step shows „Link nicht mehr gültig“ (S5-13b)
    ['/passwort/neu', 'Link nicht mehr gültig'],
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

  test('S11 with summary, notices, an open detail panel and the action bar needs no horizontal scrolling', async ({ page }) => {
    await toStep4(page);
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
    await expect(page.getByRole('button', { name: 'Liste exportieren' })).toBeVisible();
    await expectReflow(page);

    await page.getByRole('button', { name: 'Details anzeigen' }).first().click();
    await expect(page.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' }).first()).toBeVisible();
    await expectReflow(page);
  });

  test('S01 with the error summary needs no horizontal scrolling (#170 L1)', async ({ page }) => {
    await page.goto('/(login:anmelden)');
    await expect(page.getByRole('heading', { level: 1, name: 'Anmelden' })).toBeVisible();
    await expectReflow(page);

    await page.getByRole('main').getByRole('button', { name: 'Anmelden' }).click();
    await expect(page.locator('pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);
  });

  test('S02 with the error summary and after the submit needs no horizontal scrolling', async ({ page }) => {
    // The registration is answered here: the real one is tested in identity/register.spec.ts
    await page.route('**/api/v1/auth/register', (route) => route.fulfill({ status: 204 }));
    await page.goto('/registrieren');
    const submit = page.getByRole('main').getByRole('button', { name: 'Konto erstellen' });
    await submit.click();
    await expect(page.locator('pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);

    await page.getByLabel('E-Mail-Adresse').fill('reflow@example.test');
    await page.getByLabel('Passwort', { exact: true }).fill('lang-genug-2026');
    await page.getByRole('checkbox', { name: /^Ich habe die Datenschutzhinweise/ }).check();
    await submit.click();
    await expect(page.getByRole('heading', { level: 2, name: 'E-Mail-Adresse bestätigen' })).toBeFocused();
    await expectReflow(page);
  });

  test('the confirmation page with „E-Mail-Adresse bestätigt“ needs no horizontal scrolling', async ({ page }) => {
    await page.route('**/api/v1/auth/confirm-email', (route) => route.fulfill({ status: 204 }));
    await page.goto('/registrieren/bestaetigen?token=reflow');
    await expect(page.getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeVisible();
    await expectReflow(page);
  });

  test('S03 with the error summary and after the request needs no horizontal scrolling', async ({ page }) => {
    // The request is answered here: the real one is tested in identity/password-reset.spec.ts
    await page.route('**/api/v1/auth/password-reset', (route) => route.fulfill({ status: 200, json: { validHours: 2 } }));
    await page.goto('/passwort');
    const submit = page.getByRole('main').getByRole('button', { name: 'Link anfordern' });
    await submit.click();
    await expect(page.locator('pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);

    await page.getByLabel('E-Mail-Adresse').fill('reflow@example.test');
    await submit.click();
    await expect(page.getByText(/Falls ein Konto zu reflow@example.test existiert/)).toBeFocused();
    await expectReflow(page);
  });

  test('the new-password step of S03 with the error summary and after saving needs no horizontal scrolling', async ({ page }) => {
    await page.route('**/api/v1/auth/password-reset/complete', (route) => route.fulfill({ status: 204 }));
    await page.goto('/passwort/neu?token=reflow');
    const submit = page.getByRole('main').getByRole('button', { name: 'Passwort speichern' });
    await submit.click();
    await expect(page.locator('pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);

    await page.getByLabel('Neues Passwort').fill('lang-genug-2026');
    await submit.click();
    await expect(page.getByRole('heading', { level: 2, name: 'Passwort geändert' })).toBeFocused();
    await expectReflow(page);
  });

  test('S20 with the error summary needs no horizontal scrolling', async ({ page }) => {
    await signInThroughApi(page, CONFIRMED_USER, TEST_USER_PASSWORD);
    await page.goto('/organisation');
    await expect(page.getByRole('heading', { level: 1, name: 'Organisation anlegen oder beitreten' })).toBeVisible();
    await expectReflow(page);

    await page.getByRole('main').getByRole('button', { name: 'Organisation anlegen' }).click();
    await expect(page.locator('pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);
  });

  test('S21 as a list with the last-Owner notice and the confirmation dialog needs no horizontal scrolling', async ({ page }) => {
    // The seeded organisation is only read: the refusal changes nothing, the dialog is cancelled
    await signInThroughApi(page, ORG_OWNER, TEST_USER_PASSWORD);
    await page.goto(`/organisation/${E2E_ORGANIZATION.id}/mitglieder`);
    const main = page.getByRole('main');
    await expect(main.getByRole('heading', { level: 1, name: `Mitglieder von ${E2E_ORGANIZATION.name}` })).toBeVisible();
    await expect(main.getByRole('listitem')).toHaveCount(3);
    await expectReflow(page);

    await main.getByRole('combobox', { name: `Rolle von ${ORG_OWNER}` }).click();
    await page.getByRole('option', { name: /^Bearbeiten/ }).click();
    await expect(main.getByRole('alert')).toContainText('Änderung nicht möglich');
    await expectReflow(page);

    await main.getByRole('button', { name: 'Aus der Organisation entfernen' }).first().click();
    await expect(page.getByRole('dialog').getByRole('heading', { name: 'Mitglied entfernen?' })).toBeFocused();
    await expectReflow(page);
    await page.getByRole('dialog').getByRole('button', { name: 'Abbrechen' }).click();
  });

  test('S21 with open invitations and the invite form with its error summary needs no horizontal scrolling', async ({ page }) => {
    // The open invitation is answered here and the form is only submitted empty: the seeded
    // organisation stays unchanged (owner decision S5-16 7a); the real flow is organization/invitations.spec.ts
    await page.route(`**/api/v1/organizations/${E2E_ORGANIZATION.id}/invitations`, (route) =>
      route.fulfill({
        status: 200,
        json: [{ id: 'reflow', email: 'eine-recht-lange-adresse-zum-umbrechen@example.test', role: 'editor', expiresAt: '2026-12-01T00:00:00Z' }],
      }),
    );
    await signInThroughApi(page, ORG_OWNER, TEST_USER_PASSWORD);
    await page.goto(`/organisation/${E2E_ORGANIZATION.id}/mitglieder`);
    const main = page.getByRole('main');
    await expect(main.getByRole('region', { name: 'Offene Einladungen' })).toBeVisible();
    await expectReflow(page);

    await main.getByRole('button', { name: 'Einladung senden' }).click();
    await expect(page.locator('pa-invite-form pa-error-summary pa-notice')).toBeFocused();
    await expectReflow(page);
  });

  test('the page of an invitation link, signed out and „nicht mehr gültig“, needs no horizontal scrolling', async ({ page }) => {
    await page.route('**/api/v1/invitations/view', (route) =>
      route.fulfill({
        status: 200,
        json: {
          organizationName: 'Bezirksamt Hamburg-Mitte',
          role: 'editor',
          addressMatches: null,
          invitedEmail: null,
          alreadyMember: false,
          organizationId: null,
        },
      }),
    );
    await page.goto('/organisation/einladung?token=reflow');
    await expect(page.getByRole('heading', { level: 1, name: 'Einladung in eine Organisation' })).toBeVisible();
    await expect(page.getByRole('main').getByRole('button', { name: 'Konto erstellen' })).toBeVisible();
    await expectReflow(page);

    await page.unroute('**/api/v1/invitations/view');
    await page.goto('/organisation/einladung');
    await expect(page.getByRole('heading', { level: 1, name: 'Einladung nicht mehr gültig' })).toBeVisible();
    await expectReflow(page);
  });

  test('the login overlay after a 401 needs no horizontal scrolling', async ({ page }) => {
    await expireSessionOnce(page, '**/api/v1/catalog/criteria?**');
    await toStep4(page);
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    const overlay = page.getByRole('dialog', { name: 'Sie wurden abgemeldet' });
    await expect(overlay.getByLabel('E-Mail-Adresse')).toBeFocused();

    await expectReflow(page);
    await overlay.getByRole('button', { name: 'Anmelden' }).scrollIntoViewIfNeeded();
    await expect(overlay.getByRole('button', { name: 'Anmelden' })).toBeInViewport();
  });

  test('S11 with a card of two procedures (tabs) needs no horizontal scrolling and keeps both tabs reachable', async ({ page }) => {
    // T.1: „Mobile Anwendung (offen)“ is mapped to the Software-Test and the App-Test, so 99.1 (texts for both) gets two tabs.
    await page.goto('/kriterien');
    await chooseUseCase(page, 'HR', 'HR-Anwendung');
    await next(page, 'Was möchten Sie beschaffen?');
    await page.getByRole('checkbox', { name: 'Mobile Anwendung (offen)', exact: true }).press('Space');
    await next(page, 'Welche Funktionen hat das Produkt?');
    await next(page, /Soll das Produkt nach einem bestimmten Verfahren geprüft sein/);
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();

    const card = page.getByRole('listitem').filter({
      has: page.getByRole('heading', { level: 2, name: 'Testkriterium mit Regeln auf allen vier Ebenen' }),
    });
    await card.getByRole('button', { name: 'Details anzeigen' }).click();
    await expect(card.getByRole('tab')).toHaveCount(2);
    await expectReflow(page);
    // The first label is selected, and both labels fit the header without Material's pagination buttons (#312, TD-54).
    await expect(card.getByRole('tab', { name: 'Software-Test' })).toHaveAttribute('aria-selected', 'true');
    await expect(card.locator('.mat-mdc-tab-header')).not.toHaveClass(/mat-mdc-tab-header-pagination-controls-enabled/);

    // Both labels are reachable by keyboard (also where a wider font would bring the pagination back), and the header follows the selected tab into the viewport.
    for (const name of ['Software-Test', 'App-Test', 'Software-Test']) {
      const tab = card.getByRole('tab', { name });
      await card.getByRole('tab').first().focus();
      await page.keyboard.press('Home');
      if (name === 'App-Test') {
        await page.keyboard.press('ArrowRight');
      }
      // Arrow keys only move the focus between the tabs; Enter selects.
      await page.keyboard.press('Enter');
      await expect(tab).toHaveAttribute('aria-selected', 'true');
      // The header scrolls to the selected tab (animated, sub-pixel rounding at the edge), so allow 1 %.
      await expect(tab).toBeInViewport({ ratio: 0.99 });
      // And it lies inside the visible part of the tab header, not behind its edge or a pagination button.
      await expect
        .poll(() =>
          tab.evaluate((element) => {
            const label = element.getBoundingClientRect();
            const header = element.closest('.mat-mdc-tab-header')!.getBoundingClientRect();
            return label.left >= header.left - 1 && label.right <= header.right + 1;
          }),
        )
        .toBe(true);
      await expectReflow(page);
    }
  });

  test('S13 fills the screen with its actions at the bottom and needs no horizontal scrolling', async ({ page }) => {
    await toStep4(page);
    await page.getByRole('button', { name: 'Kriterienliste anzeigen' }).click();
    await expect(page.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeFocused();
    await page.getByRole('button', { name: 'Liste exportieren' }).click();
    const dialog = page.getByRole('dialog', { name: 'Kriterienliste exportieren' });
    await expect(dialog).toBeVisible();
    await expectReflow(page);

    // Full screen (S13-M): the dialog covers the viewport, and „Datei herunterladen“ is inside it.
    const box = await dialog.boundingBox();
    expect(box).toMatchObject({ x: 0, y: 0, width: ZOOM_400.width, height: ZOOM_400.height });
    await expect(dialog.getByRole('button', { name: 'Datei herunterladen' })).toBeInViewport();
  });
});

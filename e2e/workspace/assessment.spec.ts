// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page } from '@playwright/test';
import {
  createCandidateThroughApi,
  createOrganizationThroughApi,
  createProjectThroughApi,
  registerConfirmedAccount,
  saveListThroughApi,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';
import { settled, tabTo } from '../support/keyboard';

/**
 * The assessment tab S34b (S6-19; R-CAN-06, Handoff §4.6): an Owner assesses a product with the
 * keyboard only — no click, tap or pointer move —: Tab into the first criterion's radio group, an
 * arrow key chooses a level, which is saved by itself after 800 ms and announced
 * „Einschätzung gespeichert.“; Tab into its note, typing is saved the same way. After a reload both
 * are still there, and Überblick counts the criterion. Below 600 px K6 is the compact form.
 * S6-19b (R-CAN-08): the progress „0 von n“ → „1 von n“ and the list line; the rating legend
 * expanded until the first assessment, which stores „collapsed“ in the user's settings while the
 * legend stays open on that visit (owner decision S6-19b (a)), collapsed after the reload; the
 * filter „Nur noch nicht eingeschätzte anzeigen“ with Space.
 * Axe in both appearances on every state (A11Y-01, A11Y-08).
 */
const WIDE = { width: 1280, height: 800 };
const NARROW = { width: 360, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');

/** The PUT of one row of the candidate's assessment. */
function assessmentSaved(page: Page, candidateId: string) {
  return page.waitForResponse(
    (response) => response.request().method() === 'PUT' && new URL(response.url()).pathname.startsWith(`/api/v1/candidates/${candidateId}/assessments/`),
  );
}

test.describe('Assessment tab S34b', () => {
  test('INV:A11Y-01 INV:A11Y-08 INV:CAT-15 an Owner assesses a criterion and its note with the keyboard only; both are autosaved and stay', async ({ page, request }, testInfo) => {
    test.setTimeout(90_000);
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `assessment-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const listId = await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    const project = `Bewerbermanagement ${suffix}`;
    const projectId = await createProjectThroughApi(page.request, organizationId, project, listId);
    const candidateId = await createCandidateThroughApi(page.request, projectId, 'BewerberPro');

    await page.goto(`/projekte/${projectId}/produkte/${candidateId}/einschaetzung`);
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeVisible();
    await expect(page).toHaveTitle(`Einschätzung nach Kriterien – BewerberPro – ${project} – ProcureAccess`);
    await expect(main(page)).toContainText('Schätzen Sie für jedes Kriterium ein, wie gut das Produkt es erfüllt.');
    const cards = main(page).locator('pa-assessment-input');
    await expect(cards.first()).toBeVisible();
    const first = cards.first();
    const radios = first.getByRole('radio');
    const note = first.getByRole('textbox', { name: 'Worauf stützt sich Ihre Einschätzung? (optional)' });
    // Only the allowed levels and „Nicht eingeschätzt“, which is last and preset (CAT-15)
    await expect(radios.last()).toHaveAccessibleName('Nicht eingeschätzt');
    await expect(radios.last()).toBeChecked();
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    // Progress, list line and the legend, expanded for a user who never assessed (R-CAN-08)
    const total = await cards.count();
    const count = main(page).locator('.pa-assessment__count');
    const legend = main(page).locator('pa-rating-legend button');
    await expect(count).toHaveText(`0 von ${total} Kriterien eingeschätzt`);
    await expect(main(page)).toContainText('Kriterienliste: HR-Software 2027 (Katalogversion T.1)');
    await expect(legend).toHaveAttribute('aria-expanded', 'true');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b');

    // Keyboard only: from the top of the page, Tab reaches the checked radio of the first criterion
    await tabTo(page, first.getByRole('radio', { checked: true }));
    const saved = assessmentSaved(page, candidateId);
    const legendStored = page.waitForResponse(
      (response) => response.request().method() === 'PUT' && new URL(response.url()).pathname === '/api/v1/account/preferences',
    );
    // ArrowUp moves from „Nicht eingeschätzt“ to the level before it and checks it (an arrow key past
    // the end of the group wraps in Chromium and Firefox, but not in WebKit)
    const chosen = (await radios.count()) - 2;
    await page.keyboard.press('ArrowUp');
    await expect(radios.nth(chosen)).toBeChecked();
    const level = (await first.locator('mat-radio-button').nth(chosen).textContent())?.trim() ?? '';
    const valueSaved = await saved;
    expect(valueSaved.status()).toBe(200);
    expect(valueSaved.request().postDataJSON()).toMatchObject({ note: null });
    await expect(page.locator('.cdk-live-announcer-element')).toHaveText('Einschätzung gespeichert.');
    await expect(count).toHaveText(`1 von ${total} Kriterien eingeschätzt`);
    // The first assessment stores „collapsed“; on this visit the legend stays open
    expect((await legendStored).request().postDataJSON()).toMatchObject({ assessmentLegend: 'collapsed' });
    await expect(legend).toHaveAttribute('aria-expanded', 'true');

    // Tab on into its note and type: saved by itself once typing pauses
    await tabTo(page, note);
    const noteSaved = assessmentSaved(page, candidateId);
    await page.keyboard.type('Eigener Test mit der Tastatur.');
    const withNote = await noteSaved;
    expect(withNote.status()).toBe(200);
    expect(withNote.request().postDataJSON()).toMatchObject({ note: 'Eigener Test mit der Tastatur.' });
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b-assessed');

    // Both stay after a reload (stored on the server, R-CAN-06)
    await page.reload();
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeVisible();
    await expect(radios.nth(chosen)).toBeChecked();
    await expect(note).toHaveValue('Eigener Test mit der Tastatur.');
    await expect(legend).toHaveAttribute('aria-expanded', 'false');

    // The filter with Space: only the criteria not assessed yet, then all again
    await tabTo(page, main(page).getByRole('checkbox', { name: 'Nur noch nicht eingeschätzte anzeigen' }));
    await page.keyboard.press('Space');
    await expect(cards).toHaveCount(total - 1);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b-filtered');
    await page.keyboard.press('Space');
    await expect(cards).toHaveCount(total);

    // Überblick counts the assessed criterion (the server's counts, read after leaving the tab)
    // Only the selected tab is in the tab order; Home moves to the first one (ARIA tabs)
    // The filter is below the tabs: back with Shift+Tab
    await tabTo(page, main(page).getByRole('tab', { selected: true }), 'Shift+Tab');
    await page.keyboard.press('Home');
    await page.keyboard.press('Enter');
    await expect(page).toHaveTitle(`Überblick – BewerberPro – ${project} – ProcureAccess`);
    await expect(main(page)).toContainText(/1 von \d+ Kriterien eingeschätzt/);
    const row = main(page).locator('.pa-fulfillment__row', { has: page.locator(`dt:text-is("${level}")`) });
    await expect(row.locator('dd')).toHaveText('1 Kriterium');

    // Below 600 px K6 is the compact form: a select „Einschätzung“ and the note behind a button (Handoff §2.2)
    await page.setViewportSize(NARROW);
    await page.goto(`/projekte/${projectId}/produkte/${candidateId}/einschaetzung`);
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeVisible();
    await expect(first.getByRole('combobox', { name: 'Einschätzung' })).toContainText(level);
    await expect(first.getByRole('button', { name: 'Begründung bearbeiten' })).toBeVisible();
    await expect(cards.first().locator('textarea')).toBeHidden();
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b-compact');
  });
});

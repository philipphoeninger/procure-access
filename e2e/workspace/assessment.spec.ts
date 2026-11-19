// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import {
  createCandidateThroughApi,
  createOrganizationThroughApi,
  createProjectThroughApi,
  registerConfirmedAccount,
  saveListThroughApi,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';

/**
 * The assessment tab S34b (S6-19; R-CAN-06, Handoff §4.6): an Owner assesses a product with the
 * keyboard only — no click, tap or pointer move —: Tab into the first criterion's radio group, an
 * arrow key chooses a level, which is saved by itself after 800 ms and announced
 * „Einschätzung gespeichert.“; Tab into its note, typing is saved the same way. After a reload both
 * are still there, and Überblick counts the criterion. Below 600 px K6 is the compact form.
 * Axe in both appearances on every state (A11Y-01, A11Y-08).
 *
 * The K6 cards are found by CSS: Playwright's name lookups of form fields followed by axe crash
 * Firefox's page process now and then (TESTING V226, V367); the names are checked by the
 * component tests and by axe.
 */
const WIDE = { width: 1280, height: 800 };
const NARROW = { width: 360, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';
const MAX_TABS = 30;

const main = (page: Page) => page.getByRole('main');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** Presses Tab until the focused element matches the CSS `selector` (keyboard only). */
async function tabTo(page: Page, selector: string): Promise<void> {
  for (let presses = 0; presses <= MAX_TABS; presses++) {
    if (await page.evaluate((css) => document.activeElement?.matches(css) ?? false, selector)) return;
    await page.keyboard.press('Tab');
  }
  throw new Error(`${selector} was not reached with ${MAX_TABS} Tab presses`);
}

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
    const radios = first.locator('input[type="radio"]');
    // Only the allowed levels and „Nicht eingeschätzt“, which is last and preset (CAT-15)
    await expect(first.locator('mat-radio-button').last()).toHaveText('Nicht eingeschätzt');
    await expect(radios.last()).toBeChecked();
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b');

    // Keyboard only: from the top of the page, Tab reaches the checked radio of the first criterion
    await tabTo(page, 'pa-assessment-input:first-of-type input[type="radio"]:checked');
    const saved = assessmentSaved(page, candidateId);
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

    // Tab on into its note and type: saved by itself once typing pauses
    await tabTo(page, 'pa-assessment-input:first-of-type textarea');
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
    await expect(cards.first().locator('input[type="radio"]').nth(chosen)).toBeChecked();
    await expect(cards.first().locator('textarea')).toHaveValue('Eigener Test mit der Tastatur.');

    // Überblick counts the assessed criterion (the server's counts, read after leaving the tab)
    // Only the selected tab is in the tab order; Home moves to the first one (ARIA tabs)
    await tabTo(page, '[role="tab"][aria-selected="true"]');
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
    await expect(cards.first().locator('mat-select')).toContainText(level);
    await expect(cards.first().locator('.pa-assessment-input__note-toggle')).toHaveText('Begründung bearbeiten');
    await expect(cards.first().locator('textarea')).toBeHidden();
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34b-compact');
  });
});

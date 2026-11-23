// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import {
  acceptInvitationThroughApi,
  createOrganizationThroughApi,
  inviteThroughApi,
  registerConfirmedAccount,
  saveListThroughApi,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';

/**
 * J2, workspace, up to the assessment (TESTING §5.4; S6-22b — the exit check of migration step 5),
 * with the keyboard only (A11Y-02): the test never clicks, taps or moves the pointer, and it
 * reaches every control with Tab from where the page put the focus, as J1. Axe runs on the states
 * in both appearances (A11Y-01, A11Y-08). Every browser project runs it at its own viewport, so
 * the two mobile projects go through the layouts below 600 px (the compact K6, the short tab label).
 *
 * An Owner of an organisation of the test's own (account, organisation and its saved list built
 * through the API: registering and saving a list are J8 and J1) creates a project with a list in
 * S31, takes a criterion out of the project's copy on S11, adds a product in S33, records that it
 * was tested with one evidence in S34a and assesses the first criterion in S34b; Überblick and S32
 * then show the evidence and count the assessment. A Reader of the same organisation, in a browser
 * context of their own, then walks S32 and the three tabs of S34 and finds the stored values
 * without any input (R-CAN-10). The saved list has no test procedure, so no „Geprüft nach …“ row
 * appears (those are in `workspace/assessment.spec.ts` and the component tests). The publication and
 * the directory follow with step 6.
 *
 * Controls are found by role and accessible name, so the journey fails when a name goes missing.
 */
const PASSWORD = 'e2e-Passwort-lang-genug';
const LIST = 'HR-Software 2027';
const PRODUCT = 'BewerberPro';
const NOTE = 'Eigener Test mit der Tastatur.';

// S11 with its cards and S34b with the legend have many tab stops before the target.
const MAX_TABS = 80;

const main = (page: Page) => page.locator('main');
const heading = (page: Page) => main(page).getByRole('heading', { level: 1 });
const announcement = (page: Page) => page.locator('.cdk-live-announcer-element');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/**
 * Presses Tab (or Shift+Tab) until the target has the focus, as a keyboard user moves through the
 * page. Fails when the target is not reached, i.e. when it is not in the tab order from the focus on.
 */
async function tabTo(page: Page, target: Locator, key: 'Tab' | 'Shift+Tab' = 'Tab'): Promise<void> {
  for (let presses = 0; presses <= MAX_TABS; presses++) {
    if (await target.evaluate((element) => element === document.activeElement)) {
      return;
    }
    await page.keyboard.press(key);
  }
  throw new Error(`${target.toString()} was not reached with ${MAX_TABS} presses of ${key}`);
}

/** The option of the open list of a `mat-select` that the arrow keys have reached. */
const activeOption = (page: Page) => page.locator('mat-option.mat-mdc-option-active');

/**
 * Presses ArrowDown in the open list of a `mat-select` until the option with the text is active.
 * The select marks its first (or selected) option active when the list opens, but the class is
 * rendered a moment later; a press before that skipped the first option, and the list does not
 * wrap, so the target was never reached. Hence wait for the active option before the first
 * check and for each press to move it before the next one.
 */
async function arrowToOption(page: Page, text: string): Promise<void> {
  await expect(activeOption(page)).toHaveCount(1);
  for (let presses = 0; presses <= 10; presses++) {
    const current = ((await activeOption(page).textContent()) ?? '').trim();
    if (current.startsWith(text)) {
      return;
    }
    await page.keyboard.press('ArrowDown');
    await expect(activeOption(page), `ArrowDown did not move past „${current}“ towards „${text}“`).not.toHaveText(current);
  }
  throw new Error(`The option „${text}“ was not reached with the arrow keys`);
}

/**
 * From anywhere below the tabs of S34: Shift+Tab back to the selected tab — the only one in the tab
 * order (ARIA tabs) —, the arrow key to the tab at `index`, Enter. The tab's page takes the focus
 * to the H1.
 */
async function openTab(page: Page, index: number, from: 'above' | 'below'): Promise<void> {
  const tabs = main(page).getByRole('tab');
  await tabTo(page, main(page).getByRole('tab', { selected: true }), from === 'below' ? 'Shift+Tab' : 'Tab');
  for (let presses = 0; presses <= 3; presses++) {
    if (await tabs.nth(index).evaluate((element) => element === document.activeElement)) break;
    await page.keyboard.press('ArrowRight');
  }
  await expect(tabs.nth(index)).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(tabs.nth(index)).toHaveAttribute('aria-selected', 'true');
  await expect(heading(page)).toBeFocused();
}

/** The row of a `<dl>` inside `scope` whose term is `term`: its values. */
const valuesOf = (scope: Locator, term: string) => scope.locator(`dt:text-is("${term}") ~ dd`);

test.describe('J2 workspace up to the assessment', () => {
  test('INV:A11Y-02 INV:A11Y-01 INV:A11Y-08 an Owner creates a project with a list, adjusts the list, adds a product, records its evidence and assesses a criterion by keyboard; a Reader finds all of it read only', async ({ page, browser, request }, testInfo) => {
    // Eight screens with axe in two appearances each and a second browser context (as J1)
    test.setTimeout(240_000);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const owner = `j2-owner-${suffix}@example.test`;
    const reader = `j2-reader-${suffix}@example.test`;
    const project = `Bewerbermanagement ${suffix}`;
    await registerConfirmedAccount(request, owner, PASSWORD);
    await registerConfirmedAccount(request, reader, PASSWORD);
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    await saveListThroughApi(page.request, organizationId, LIST);
    await inviteThroughApi(page, organizationId, reader, 'reader');

    // S30 → S31: the name and the list of the organisation; the project gets its own copy (R-PRJ-01, R-PRJ-02)
    await page.goto('/projekte');
    await expect(main(page)).toContainText('Noch keine Beschaffungsprojekte.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s30');
    await tabTo(page, main(page).getByRole('button', { name: 'Projekt anlegen' }));
    await page.keyboard.press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Beschaffungsprojekt anlegen' });
    // MatDialog moves the focus when it has opened (S6-01)
    await expect(dialog.getByRole('textbox', { name: 'Name des Projekts' })).toBeFocused();
    await page.keyboard.type(project);
    const listChoice = dialog.getByRole('combobox', { name: 'Kriterienliste (optional)' });
    await tabTo(page, listChoice);
    await page.keyboard.press('Enter');
    await arrowToOption(page, LIST);
    await page.keyboard.press('Enter');
    await expect(listChoice).toContainText(LIST);
    await expect(page.getByRole('option')).toHaveCount(0);
    await expect(dialog.locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 'j2-s31');
    const assigned = page.waitForResponse(
      (response) => response.request().method() === 'PUT' && new URL(response.url()).pathname.endsWith('/criteria-configuration'),
    );
    await tabTo(page, dialog.getByRole('button', { name: 'Projekt anlegen' }));
    await page.keyboard.press('Enter');
    expect((await assigned).status()).toBe(200);

    // S32 with the copy and no product
    await expect(page).toHaveURL(/\/projekte\/[0-9a-f-]{36}$/);
    const projectUrl = page.url();
    await expect(heading(page)).toHaveText(project);
    await expect(heading(page)).toBeFocused();
    await expect(main(page)).toContainText('Noch keine Produkte in diesem Projekt.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s32-new');

    // S11 of the copy: one criterion is taken out; it is saved by itself (R-PRJ-06)
    await tabTo(page, main(page).getByRole('link', { name: 'Kriterienliste anpassen' }));
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/\/kriterienliste$/);
    await expect(heading(page)).toHaveText(LIST);
    await expect(heading(page)).toBeFocused();
    const cards = main(page).locator('pa-criterion-card');
    const listed = await cards.count();
    expect(listed).toBeGreaterThan(2);
    const excluded = page.waitForResponse(
      (response) => response.request().method() === 'PATCH' && new URL(response.url()).pathname.includes('/criteria/'),
    );
    const include = cards.first().getByRole('checkbox', { name: /^In Liste aufnehmen / });
    await tabTo(page, include);
    await page.keyboard.press('Space');
    expect((await excluded).ok()).toBe(true);
    await expect(include).not.toBeChecked();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s11-copy');

    // Back to the project as the browser goes back (Alt+←): S11 of a copy has no link to its project
    await page.goBack();
    await expect(page).toHaveURL(projectUrl);
    await expect(heading(page)).toHaveText(project);
    await expect(heading(page)).toBeFocused();

    // S32's menu „Produkt hinzufügen“ › „Neu erfassen“ → S33 (R-CAN-02)
    await tabTo(page, main(page).getByRole('button', { name: 'Produkt hinzufügen' }));
    await page.keyboard.press('Enter');
    const entry = page.getByRole('menuitem', { name: 'Neu erfassen' });
    await expect(entry).toBeFocused();
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(/\/produkte\/neu$/);
    await expect(heading(page)).toHaveText('Produkt hinzufügen');
    await expect(heading(page)).toBeFocused();
    await tabTo(page, main(page).getByRole('textbox', { name: 'Produktname' }));
    await page.keyboard.type(PRODUCT);
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s33');
    const created = page.waitForResponse(
      (response) => response.request().method() === 'POST' && new URL(response.url()).pathname.endsWith('/candidates'),
    );
    await tabTo(page, main(page).getByRole('button', { name: 'Speichern' }));
    await page.keyboard.press('Enter');
    expect((await created).status()).toBe(201);

    // S34 Überblick: „Nicht bekannt“ and nothing assessed (SIG-03)
    await expect(page).toHaveURL(/\/produkte\/[0-9a-f-]{36}$/);
    const productUrl = page.url();
    await expect(heading(page)).toHaveText(PRODUCT);
    await expect(heading(page)).toBeFocused();
    await expect(main(page)).toContainText('Auf Barrierefreiheit geprüft: Nicht bekannt');
    // The criterion taken out on S11 is not assessed (WSP-03)
    const total = listed - 1;
    await expect(main(page)).toContainText(`0 von ${total} Kriterien eingeschätzt`);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s34-overview');

    // S34a: „Ja“, then one evidence of the first procedure (R-CAN-05, WSP-01)
    await openTab(page, 1, 'above');
    await expect(page).toHaveURL(/\/pruefnachweis$/);
    const status = (name: string) => main(page).getByRole('radio', { name, exact: true });
    await tabTo(page, status('Nicht bekannt'));
    await expect(status('Nicht bekannt')).toBeChecked();
    // „Ja“ is the first of the three: two steps up, without wrapping past the end (WebKit does not wrap)
    await page.keyboard.press('ArrowUp');
    await page.keyboard.press('ArrowUp');
    await expect(status('Ja')).toBeChecked();
    const card = main(page).locator('.pa-evidence__card:has(.pa-evidence__card-title)');
    const cardHeading = card.getByRole('heading', { level: 3 });
    await expect(cardHeading).toHaveText('Prüfnachweis 1');
    await tabTo(page, card.getByRole('combobox', { name: 'Art der Prüfung' }));
    await page.keyboard.press('Enter');
    await arrowToOption(page, 'BIT inklusiv Software-Test');
    await page.keyboard.press('Enter');
    await expect(cardHeading).toHaveText('Prüfnachweis 1: BIT inklusiv Software-Test');
    await tabTo(page, card.getByRole('textbox', { name: 'Wer hat geprüft?' }));
    await page.keyboard.type('Prüfstelle Beispiel');
    await tabTo(page, card.getByRole('textbox', { name: 'Datum der Prüfung' }));
    await page.keyboard.type('14.03.2026');
    const evidenceSaved = page.waitForResponse(
      (response) => response.request().method() === 'POST' && new URL(response.url()).pathname.endsWith('/test-evidences'),
    );
    await tabTo(page, main(page).getByRole('button', { name: 'Speichern' }));
    await page.keyboard.press('Enter');
    expect((await evidenceSaved).status()).toBe(201);
    await expect(announcement(page)).toHaveText('Prüfnachweis gespeichert.');
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s34a');

    // S34b: the first criterion gets a level and a note, both saved by themselves (R-CAN-06)
    await openTab(page, 2, 'below');
    await expect(page).toHaveURL(/\/einschaetzung$/);
    const inputs = main(page).locator('pa-assessment-input');
    await expect(inputs).toHaveCount(total);
    await expect(main(page).locator('.pa-assessment__count')).toHaveText(`0 von ${total} Kriterien eingeschätzt`);
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    const first = inputs.first();
    const saved = () =>
      page.waitForResponse(
        (response) => response.request().method() === 'PUT' && new URL(response.url()).pathname.includes('/assessments/'),
      );
    const valueSaved = saved();
    const levelSelect = first.getByRole('combobox', { name: 'Einschätzung' });
    const compact = (await levelSelect.count()) > 0;
    if (compact) {
      // Below 600 px: the select „Einschätzung“; its list opens on the chosen „Nicht eingeschätzt“, the last option
      await tabTo(page, levelSelect);
      await page.keyboard.press('Enter');
      await expect(activeOption(page)).toHaveText('Nicht eingeschätzt');
      await page.keyboard.press('ArrowUp');
      await page.keyboard.press('Enter');
    } else {
      // From 600 px: the radio group; ArrowUp checks the level before „Nicht eingeschätzt“
      await tabTo(page, first.getByRole('radio', { checked: true }));
      await page.keyboard.press('ArrowUp');
    }
    const stored = await valueSaved;
    expect(stored.status()).toBe(200);
    const level = (stored.request().postDataJSON() as { value: string }).value;
    expect(level).not.toBe('unknown');
    await expect(main(page).locator('.pa-assessment__count')).toHaveText(`1 von ${total} Kriterien eingeschätzt`);
    const noteSaved = saved();
    const noteField = first.getByRole('textbox', { name: 'Worauf stützt sich Ihre Einschätzung? (optional)' });
    if (compact) {
      await tabTo(page, first.getByRole('button', { name: 'Begründung hinzufügen' }));
      await page.keyboard.press('Enter');
      await expect(noteField).toBeFocused();
    } else {
      await tabTo(page, noteField);
    }
    await page.keyboard.type(NOTE);
    expect((await noteSaved).request().postDataJSON()).toMatchObject({ value: level, note: NOTE });
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s34b');

    // Überblick counts the assessment and names the evidence; S32 lists the product with both
    await openTab(page, 0, 'below');
    await expect(page).toHaveURL(productUrl);
    await expect(main(page)).toContainText('Auf Barrierefreiheit geprüft: Ja – 1 Prüfnachweis: BIT inklusiv Software-Test');
    await expect(main(page)).toContainText(`1 von ${total} Kriterien eingeschätzt`);
    await tabTo(page, main(page).getByRole('navigation', { name: 'Sie sind hier' }).getByRole('link', { name: project }), 'Shift+Tab');
    await page.keyboard.press('Enter');
    await expect(page).toHaveURL(projectUrl);
    await expect(heading(page)).toHaveText(project);
    await expect(heading(page)).toBeFocused();
    const item = main(page).locator('.pa-project__product');
    await expect(item.getByRole('heading', { level: 3 })).toHaveText(PRODUCT);
    await expect(item).toContainText(`1 von ${total} Kriterien eingeschätzt`);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 'j2-s32-product');

    // The Reader view (R-CAN-10): the same project in a context of its own, by keyboard, without any input
    const context = await browser.newContext({ viewport: page.viewportSize() });
    try {
      const other = await context.newPage();
      await signInThroughApi(other, reader, PASSWORD);
      await acceptInvitationThroughApi(other, request, reader, 2);
      const controls = main(other).locator('input, textarea, select, mat-select, mat-radio-group, form');

      await other.goto(projectUrl);
      await expect(heading(other)).toHaveText(project);
      await expect(main(other).getByRole('button')).toHaveCount(0);
      await expect(controls).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'j2-reader-s32');

      await tabTo(other, main(other).getByRole('link', { name: PRODUCT, exact: true }));
      await other.keyboard.press('Enter');
      await expect(other).toHaveURL(productUrl);
      await expect(heading(other)).toHaveText(PRODUCT);
      await expect(heading(other)).toBeFocused();
      await expect(main(other).getByRole('link', { name: 'Bearbeiten' })).toHaveCount(0);
      await expect(main(other).getByRole('button')).toHaveCount(1); // the legend
      await expect(controls).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'j2-reader-s34');

      await openTab(other, 1, 'above');
      await expect(valuesOf(main(other), 'Wurde das Produkt auf Barrierefreiheit geprüft?')).toHaveText('Ja');
      const evidence = main(other).locator('.pa-evidence__card:has(.pa-evidence__card-title)');
      await expect(evidence.getByRole('heading', { level: 3 })).toHaveText('Prüfnachweis 1: BIT inklusiv Software-Test');
      await expect(valuesOf(evidence, 'Wer hat geprüft?')).toHaveText('Prüfstelle Beispiel');
      await expect(valuesOf(evidence, 'Datum der Prüfung')).toHaveText('14.03.2026');
      await expect(main(other).getByRole('button')).toHaveCount(0);
      await expect(controls).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'j2-reader-s34a');

      // No control below the tabs: the selected tab is the last tab stop before the footer
      await openTab(other, 2, 'above');
      const assessed = main(other).locator('.pa-assessment__card:has(h2)').first();
      await expect(valuesOf(assessed, 'Begründung')).toHaveText(NOTE);
      await expect(valuesOf(assessed, 'Einschätzung')).not.toHaveText('Nicht eingeschätzt');
      await expect(main(other).locator('.pa-assessment__count')).toHaveText(`1 von ${total} Kriterien eingeschätzt`);
      await expect(main(other).locator('pa-assessment-input')).toHaveCount(0);
      await expect(controls).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'j2-reader-s34b');
    } finally {
      await context.close();
    }
  });
});

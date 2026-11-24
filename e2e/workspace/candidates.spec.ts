// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import {
  acceptInvitationThroughApi,
  createCandidateThroughApi,
  createOrganizationThroughApi,
  createProjectThroughApi,
  inviteThroughApi,
  registerConfirmedAccount,
  saveListThroughApi,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';

/**
 * Products of a procurement project (S6-16; R-CAN-02 UI, R-CAN-03, R-CAN-04): an Owner of an
 * organisation of its own adds a product in S33 — first submitted empty, with the error summary —,
 * which opens S34 Überblick: the facts, the test status „Nicht bekannt“, and the server's progress
 * „0 von n“ and K1 with seven rows, n × „Nicht eingeschätzt“ for a new product (S6-18; owner
 * decision S6-16 D3 (b)). The tabs are routes with
 * their own page titles; „Bearbeiten“ leads to S33 with the facts filled, and a change shows on
 * S34. S33 is reached from S32's menu „Produkt hinzufügen“ › „Neu erfassen“, and the path above S34
 * leads back to S32, which lists the product with its test status, progress and seven counts
 * (S6-14; R-PRJ-03, SIG-13). On the tab Prüfnachweis S34a an evidence is saved, a second one of the
 * same procedure is refused, and leaving „Ja“ asks first and leaves the product at „Nein“ (S6-17;
 * R-CAN-05, WSP-01). Axe in both appearances on every new state (A11Y-01, A11Y-08).
 *
 * Removing a product (S6-22; R-CAN-09, WSP-04): „Produkt aus dem Projekt entfernen“ on Überblick
 * asks first; Escape keeps the product, the confirmation removes it, S32 opens with the focus on
 * its H1 and the removal is announced.
 *
 * The Reader view (S6-22; R-CAN-10): a Reader of the Owner's organisation, in a browser context of
 * their own, finds S32 and the three tabs of S34 without any input or action — the evidence and
 * the assessment the Owner stored are shown as text.
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

test.describe('Products of a project S33, S34', () => {
  test('INV:A11Y-01 INV:A11Y-08 INV:SIG-13 INV:WSP-01 an Owner adds a product from S32 in S33, sees S34 Überblick, records its evidence in S34a, edits it and finds it on S32', async ({ page, request }, testInfo) => {
    // S32, S33, S34 and S34a with ten states, each checked by axe in two appearances, take longer
    // than the default 30 s in WebKit (as J1)
    test.setTimeout(90_000);
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `candidates-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const listId = await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    const project = `Bewerbermanagement ${suffix}`;
    const projectId = await createProjectThroughApi(page.request, organizationId, project, listId);

    // S32 without products, then its menu „Produkt hinzufügen“ › „Neu erfassen“ to S33 (S6-14)
    await page.goto(`/projekte/${projectId}`);
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeVisible();
    await expect(main(page)).toContainText('Noch keine Produkte in diesem Projekt.');
    await expect(main(page).getByRole('link', { name: 'HR-Software 2027' })).toBeVisible();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-empty');
    const add = main(page).getByRole('button', { name: 'Produkt hinzufügen' });
    await expect(add).toHaveAttribute('aria-haspopup', 'menu');
    await add.press('Enter');
    const menu = page.getByRole('menu');
    await expect(menu.getByRole('menuitem')).toHaveText(['Neu erfassen']);
    await expect(menu.getByRole('menuitem').first()).toBeFocused();
    await menu.getByRole('menuitem', { name: 'Neu erfassen' }).press('Enter');
    await expect(page).toHaveURL(new RegExp(`/projekte/${projectId}/produkte/neu$`));
    await expect(main(page).getByRole('heading', { level: 1, name: 'Produkt hinzufügen' })).toBeFocused();
    await expect(page).toHaveTitle(`Produkt hinzufügen – ${project} – Beschaffungsprojekte – ProcureAccess`);
    await expect(main(page)).toContainText('Diese Angaben werden nie veröffentlicht');
    // Material adds `mat-form-field-animations-enabled` 300 ms after a field starts; an error shown
    // before that fades in again once the class arrives, and axe measured it mid-fade (as S31)
    await expect(main(page).locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(6);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's33-new');

    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    await expect(main(page).locator('pa-error-summary pa-notice')).toBeFocused();
    await expect(main(page).locator('mat-error')).toHaveText('Bitte geben Sie Produktname an.');
    await expect(page).toHaveTitle(/^Fehler: Produkt hinzufügen/);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's33-error-summary');

    await main(page).getByRole('textbox', { name: 'Produktname' }).fill('BewerberPro');
    await main(page).getByRole('textbox', { name: 'Anbieter (optional)' }).fill('Beispiel Software GmbH');
    await main(page).getByRole('textbox', { name: 'Webseite des Anbieters (optional)' }).fill('https://www.beispiel-software.de');
    await main(page).getByRole('textbox', { name: 'Interne Notizen (optional)' }).fill('Demo vereinbart.');
    const created = page.waitForResponse(
      (response) => response.request().method() === 'POST' && new URL(response.url()).pathname.endsWith('/candidates'),
    );
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await created).status()).toBe(201);

    // S34 Überblick: facts only, no verdict (R-CAN-04)
    await expect(page).toHaveURL(new RegExp(`/projekte/${projectId}/produkte/[0-9a-f-]{36}$`));
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeFocused();
    await expect(page).toHaveTitle(`Überblick – BewerberPro – ${project} – ProcureAccess`);
    await expect(main(page)).toContainText('Anbieter: Beispiel Software GmbH');
    const tabs = main(page).getByRole('tab');
    await expect(tabs).toHaveText(['Überblick', 'Prüfnachweis', 'Einschätzung nach Kriterien']);
    await expect(tabs.first()).toHaveAttribute('aria-selected', 'true');
    await expect(main(page)).toContainText('Auf Barrierefreiheit geprüft: Nicht bekannt');
    await expect(main(page)).toContainText(/0 von (\d+) Kriterien eingeschätzt · Kriterienliste „HR-Software 2027“ \(Katalogversion T\.1\)/);
    const total = /0 von (\d+) Kriterien/.exec((await main(page).textContent()) ?? '')?.[1];
    const breakdown = main(page).locator('pa-fulfillment-breakdown');
    await expect(breakdown.locator('dt')).toHaveText([
      'Erfüllt',
      'Leichte Einschränkung',
      'Einschränkung',
      'Barriere',
      'Blockade',
      'Nicht anwendbar',
      'Nicht eingeschätzt',
    ]);
    await expect(breakdown.locator('dd').last()).toHaveText(`${total} Kriterien`);
    await expect(main(page)).toContainText('Noch nicht im Produktverzeichnis veröffentlicht.');
    await expect(main(page)).toContainText('Demo vereinbart.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34-overview');

    // The tabs are routes with their own page titles (R-CAN-03)
    await tabs.nth(1).press('Enter');
    await expect(page).toHaveURL(/\/pruefnachweis$/);
    await expect(page).toHaveTitle(`Prüfnachweis – BewerberPro – ${project} – ProcureAccess`);
    await expect(tabs.nth(1)).toHaveAttribute('aria-selected', 'true');
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeFocused();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34-evidence-tab');

    // S34a (S6-17, R-CAN-05, WSP-01): „Ja“ shows the first card; one evidence of a procedure is saved.
    const radio = (name: string) => main(page).getByRole('radio', { name, exact: true });
    const card = (index: number) => main(page).locator('.pa-evidence__card:has(.pa-evidence__card-title)').nth(index);
    const cardHeading = (index: number) => card(index).getByRole('heading', { level: 3 });
    const kind = (index: number) => card(index).getByRole('combobox', { name: 'Art der Prüfung' });
    const field = (index: number, name: string) => card(index).getByRole('textbox', { name });
    const option = (name: string) => page.getByRole('option', { name });
    await expect(radio('Nicht bekannt')).toBeChecked();
    await radio('Ja').check();
    await expect(cardHeading(0)).toHaveText('Prüfnachweis 1');
    await kind(0).click();
    await option('BIT inklusiv Software-Test').click();
    await expect(cardHeading(0)).toHaveText('Prüfnachweis 1: BIT inklusiv Software-Test');
    await field(0, 'Wer hat geprüft?').fill('Prüfstelle Beispiel');
    await field(0, 'Datum der Prüfung').fill('14.03.2026');
    await field(0, 'Geprüfte Version (optional)').fill('4.2');
    const savedEvidence = page.waitForResponse(
      (response) => response.request().method() === 'POST' && new URL(response.url()).pathname.endsWith('/test-evidences'),
    );
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await savedEvidence).status()).toBe(201);
    await expect(page.locator('.cdk-live-announcer-element')).toHaveText('Prüfnachweis gespeichert.');
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34a-evidence');

    // A second card with the same procedure is refused before sending (`candidate.evidence.list.duplicate`)
    await main(page).getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' }).press('Enter');
    await expect(kind(1)).toBeFocused();
    await kind(1).click();
    await option('BIT inklusiv Software-Test').click();
    await field(1, 'Wer hat geprüft?').fill('Eigene Prüfung');
    await field(1, 'Datum der Prüfung').fill('01.04.2026');
    await expect(main(page).locator('mat-form-field:not(.mat-form-field-animations-enabled)')).toHaveCount(0);
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    await expect(main(page).locator('pa-error-summary pa-notice')).toBeFocused();
    await expect(main(page).locator('pa-error-summary')).toContainText(
      'Prüfnachweis 2 – Art der Prüfung: Für dieses Prüfverfahren gibt es schon einen Prüfnachweis.',
    );
    await expect(page).toHaveTitle(/^Fehler: Prüfnachweis – BewerberPro/);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's34a-duplicate');
    await main(page).getByRole('button', { name: 'Prüfnachweis 2 entfernen' }).press('Enter');
    await expect(cardHeading(0)).toBeFocused();

    // Leaving „Ja“ with an evidence asks first (`candidate.evidence.list.statusChange`)
    await radio('Nein').check();
    const dialog = page.getByRole('dialog', { name: 'Prüfnachweise entfernen?' });
    await expect(dialog.getByRole('heading', { level: 2, name: 'Prüfnachweise entfernen?' })).toBeFocused();
    await expect(dialog).toContainText('Ihr Prüfnachweis wird dabei entfernt. Möchten Sie fortfahren?');
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's34a-status-change');
    await dialog.getByRole('button', { name: 'Nachweise entfernen' }).press('Enter');
    await expect(dialog).toBeHidden();
    await expect(main(page).locator('.pa-evidence__title')).toHaveCount(0);
    // One request: leaving „Ja“ removes the evidence with the status (the candidate's tag, S6-20)
    const statusSaved = page.waitForResponse((response) => new URL(response.url()).pathname.endsWith('/test-status'));
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await statusSaved).status()).toBe(200);
    await expect(page.locator('.cdk-live-announcer-element')).toHaveText('Prüfnachweis gespeichert.');

    await tabs.first().press('Enter');
    await expect(page).toHaveTitle(`Überblick – BewerberPro – ${project} – ProcureAccess`);
    await expect(main(page)).toContainText('Auf Barrierefreiheit geprüft: Nein');

    // „Bearbeiten“ → S33 with the facts filled and Überblick selected
    await main(page).getByRole('link', { name: 'Bearbeiten' }).first().press('Enter');
    await expect(page).toHaveURL(/\/bearbeiten$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Produkt bearbeiten' })).toBeFocused();
    await expect(page).toHaveTitle(`Produkt bearbeiten – BewerberPro – ${project} – ProcureAccess`);
    const vendor = main(page).getByRole('textbox', { name: 'Anbieter (optional)' });
    await expect(vendor).toHaveValue('Beispiel Software GmbH');
    await expect(main(page).locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(6);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's33-edit');

    await vendor.fill('Muster AG');
    const changed = page.waitForResponse((response) => response.request().method() === 'PATCH');
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await changed).status()).toBe(200);
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeFocused();
    await expect(main(page)).toContainText('Anbieter: Muster AG');

    // The path leads back to S32, which lists the product; its name is the only link to S34 (R-PRJ-03)
    const path = page.getByRole('navigation', { name: 'Sie sind hier' });
    await expect(path.getByText('BewerberPro')).toHaveAttribute('aria-current', 'page');
    await path.getByRole('link', { name: project }).press('Enter');
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    const item = main(page).getByRole('listitem').filter({ has: page.getByRole('heading', { level: 3, name: 'BewerberPro' }) });
    await expect(item.getByRole('link')).toHaveText(['BewerberPro']);
    await expect(item).toContainText('Anbieter: Muster AG');
    // S34a above left the product at „Nein“ (S6-17)
    await expect(item.locator('dt:text-is("Auf Barrierefreiheit geprüft") + dd')).toHaveText('Nein');
    await expect(item).toContainText(`0 von ${total} Kriterien eingeschätzt`);
    // SIG-13: all seven counts, label first, in the order of the scale
    await expect(item.locator('.pa-project__counts li')).toHaveText([
      'Erfüllt: 0',
      'Leichte Einschränkung: 0',
      'Einschränkung: 0',
      'Barriere: 0',
      'Blockade: 0',
      'Nicht anwendbar: 0',
      `Nicht eingeschätzt: ${total}`,
    ]);
    await expect(item).toContainText('Nicht veröffentlicht');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-products');
  });

  test('INV:A11Y-01 INV:A11Y-08 an Owner removes a product from S34 Überblick after a confirmation; S32 opens without it and the removal is announced', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `remove-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const project = `Bewerbermanagement ${suffix}`;
    const projectId = await createProjectThroughApi(page.request, organizationId, project);
    const candidateId = await createCandidateThroughApi(page.request, projectId, 'BewerberPro');

    await page.goto(`/projekte/${projectId}/produkte/${candidateId}`);
    await expect(main(page).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeVisible();
    const remove = main(page).getByRole('button', { name: 'Produkt aus dem Projekt entfernen' });
    await remove.press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Produkt entfernen?' });
    // MatDialog moves the focus when it has opened (S6-01): the title, so the question is read first
    await expect(dialog.getByRole('heading', { name: 'Produkt entfernen?' })).toBeFocused();
    await expect(dialog).toContainText(
      '„BewerberPro“ wird mit Prüfnachweisen und Einschätzungen aus diesem Projekt entfernt. Das lässt sich nicht rückgängig machen.',
    );
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's34-remove');

    // Escape keeps the product and returns the focus to the button
    await page.keyboard.press('Escape');
    await expect(dialog).toBeHidden();
    await expect(remove).toBeFocused();

    // The removal is announced before the page title of S32: record what the live region says from now on
    await page.evaluate(() => {
      const seen: string[] = [];
      (window as unknown as { paAnnouncements: string[] }).paAnnouncements = seen;
      new MutationObserver(() => {
        const text = document.querySelector('.cdk-live-announcer-element')?.textContent?.trim();
        if (text && seen.at(-1) !== text) seen.push(text);
      }).observe(document.body, { subtree: true, childList: true, characterData: true });
    });
    await remove.press('Enter');
    await expect(dialog.getByRole('heading', { name: 'Produkt entfernen?' })).toBeFocused();
    const removed = page.waitForResponse((response) => response.request().method() === 'DELETE');
    await dialog.getByRole('button', { name: 'Produkt entfernen' }).press('Enter');
    expect((await removed).status()).toBe(204);
    await expect(page).toHaveURL(new RegExp(`/projekte/${projectId}$`));
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    await expect
      .poll(() => page.evaluate(() => (window as unknown as { paAnnouncements: string[] }).paAnnouncements))
      .toContain('„BewerberPro“ wurde aus dem Projekt entfernt.');
    await expect(main(page)).toContainText('Noch keine Produkte in diesem Projekt.');
  });

  test('INV:A11Y-01 INV:A11Y-08 a Reader finds S32 and the three tabs of S34 read only: the stored evidence and assessment as text, no input, no action', async ({ page, browser, request }, testInfo) => {
    test.setTimeout(90_000);
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const owner = `readonly-owner-${suffix}@example.test`;
    const reader = `readonly-reader-${suffix}@example.test`;
    await registerConfirmedAccount(request, owner, PASSWORD);
    await registerConfirmedAccount(request, reader, PASSWORD);
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const listId = await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    const project = `Bewerbermanagement ${suffix}`;
    const projectId = await createProjectThroughApi(page.request, organizationId, project, listId);
    const candidateId = await createCandidateThroughApi(page.request, projectId, 'BewerberPro');
    const criterion = await storeEvidenceAndAssessment(page, candidateId);
    await inviteThroughApi(page, organizationId, reader, 'reader');

    const context = await browser.newContext({ viewport: WIDE });
    try {
      const other = await context.newPage();
      await signInThroughApi(other, reader, PASSWORD);
      await acceptInvitationThroughApi(other, request, reader, 2);
      const inputs = main(other).locator('input, textarea, select, mat-select, mat-radio-group, form');

      // S32: no „Projekt bearbeiten“, no „Produkt hinzufügen“, no „Kriterienliste anpassen“
      await other.goto(`/projekte/${projectId}`);
      await expect(main(other).getByRole('heading', { level: 1, name: project })).toBeVisible();
      await expect(main(other).getByRole('link', { name: 'BewerberPro' })).toBeVisible();
      await expect(main(other).getByRole('button')).toHaveCount(0);
      await expect(inputs).toHaveCount(0);

      // Überblick: the facts and the ways to the other tabs, no „Bearbeiten“, no removal
      await main(other).getByRole('link', { name: 'BewerberPro' }).press('Enter');
      await expect(main(other).getByRole('heading', { level: 1, name: 'BewerberPro' })).toBeFocused();
      await expect(main(other)).toContainText('Auf Barrierefreiheit geprüft: Ja – 1 Prüfnachweis: BIT inklusiv Software-Test');
      await expect(main(other).getByRole('link', { name: 'Bearbeiten' })).toHaveCount(0);
      await expect(main(other).getByRole('button', { name: 'Produkt aus dem Projekt entfernen' })).toHaveCount(0);
      await expect(inputs).toHaveCount(0);

      // S34a: the stored answer and the evidence as text
      await main(other).getByRole('link', { name: 'Zum Prüfnachweis' }).press('Enter');
      await expect(other).toHaveURL(/\/pruefnachweis$/);
      await expect(main(other).locator('dt:text-is("Wurde das Produkt auf Barrierefreiheit geprüft?") + dd')).toHaveText('Ja');
      const evidence = main(other).locator('.pa-evidence__card').filter({ has: other.getByRole('heading', { level: 3 }) });
      await expect(evidence.getByRole('heading', { level: 3 })).toHaveText('Prüfnachweis 1: BIT inklusiv Software-Test');
      await expect(evidence.locator('dt')).toHaveText([
        'Art der Prüfung',
        'Wer hat geprüft?',
        'Datum der Prüfung',
        'Geprüfte Version',
        'Notiz zur Qualifikation der prüfenden Stelle',
      ]);
      await expect(evidence).toContainText('15.01.2026');
      await expect(evidence).toContainText('Nur für Ihre Organisation:');
      await expect(inputs).toHaveCount(0);
      await expect(main(other).getByRole('button')).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 's34a-readonly');

      // S34b: progress, legend and the stored level and note as text
      await main(other).getByRole('tab').nth(2).press('Enter');
      await expect(other).toHaveURL(/\/einschaetzung$/);
      const card = main(other).locator('.pa-assessment__card').filter({ has: other.getByRole('heading', { level: 2, name: criterion }) });
      await expect(card.locator('dt')).toHaveText(['Einschätzung', 'Begründung']);
      await expect(card.locator('dd')).toHaveText(['Leichte Einschränkung', 'Eigener Test mit der Tastatur.']);
      await expect(main(other).locator('.pa-assessment__count')).toHaveText(/^1 von \d+ Kriterien eingeschätzt$/);
      await expect(main(other)).not.toContainText('Schätzen Sie für jedes Kriterium ein');
      await expect(main(other).locator('pa-assessment-input')).toHaveCount(0);
      await expect(inputs).toHaveCount(0);
      // The only button opens and closes the legend
      await expect(main(other).getByRole('button')).toHaveCount(1);
      await expect(main(other).locator('pa-rating-legend button')).toHaveCount(1);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 's34b-readonly');
    } finally {
      await context.close();
    }
  });
});

/**
 * As the Owner signed in on `page`: sets the product to „Ja“ with one evidence and assesses the
 * first criterion of the project's list with a note. Returns the title S34b shows for that criterion.
 */
async function storeEvidenceAndAssessment(page: Page, candidateId: string): Promise<string> {
  const token = async () => {
    expect((await page.request.get('/api/v1/auth/session')).ok()).toBe(true);
    return (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')!.value;
  };
  const candidate = await page.request.get(`/api/v1/candidates/${candidateId}`);
  const status = await page.request.put(`/api/v1/candidates/${candidateId}/test-status`, {
    headers: { 'X-XSRF-TOKEN': await token(), 'If-Match': candidate.headers()['etag'] },
    data: { testStatus: 'tested' },
  });
  expect(status.status(), 'the test status „Ja“').toBe(200);
  const evidence = await page.request.post(`/api/v1/candidates/${candidateId}/test-evidences`, {
    headers: { 'X-XSRF-TOKEN': await token() },
    data: {
      testKind: 'procedure',
      testTypeId: 'softwareTest',
      testedBy: 'Prüfstelle Beispiel',
      testDate: '2026-01-15',
      testedVersion: '4.2',
      qualificationNote: 'Akkreditierte Prüfstelle',
    },
  });
  expect(evidence.status(), 'the evidence').toBe(201);

  const assessments = await page.request.get(`/api/v1/candidates/${candidateId}/assessments`);
  expect(assessments.ok(), 'the assessments of the product').toBe(true);
  const first = ((await assessments.json()) as { criteria: { criterionRevisionId: number; title: string; plainLanguageTitle: string | null }[] })
    .criteria[0];
  const assessed = await page.request.put(`/api/v1/candidates/${candidateId}/assessments/${first.criterionRevisionId}`, {
    headers: { 'X-XSRF-TOKEN': await token() },
    data: { value: 'slightRestriction', note: 'Eigener Test mit der Tastatur.' },
  });
  expect(assessed.status(), 'the assessment').toBe(200);
  return first.plainLanguageTitle ?? first.title;
}

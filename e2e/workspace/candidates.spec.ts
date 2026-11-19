// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { createOrganizationThroughApi, createProjectThroughApi, registerConfirmedAccount, saveListThroughApi } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';

/**
 * Products of a procurement project (S6-16; R-CAN-02 UI, R-CAN-03, R-CAN-04): an Owner of an
 * organisation of its own adds a product in S33 — first submitted empty, with the error summary —,
 * which opens S34 Überblick: the facts, the test status „Nicht bekannt“, and the server's progress
 * „0 von n“ and K1 with seven rows, n × „Nicht eingeschätzt“ for a new product (S6-18; owner
 * decision S6-16 D3 (b)). The tabs are routes with
 * their own page titles; „Bearbeiten“ leads to S33 with the facts filled, and a change shows on
 * S34. S33 is reached by its address until S6-14 builds the menu of S32 (D4 (a)). Axe in both
 * appearances on every new state (A11Y-01, A11Y-08).
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
  test('INV:A11Y-01 INV:A11Y-08 INV:WSP-01 an Owner adds a product in S33, sees S34 Überblick, records its evidence in S34a and edits it', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `candidates-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const listId = await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    const project = `Bewerbermanagement ${suffix}`;
    const projectId = await createProjectThroughApi(page.request, organizationId, project, listId);

    // S33 „Produkt hinzufügen“, by its address until S6-14 (D4 (a))
    await page.goto(`/projekte/${projectId}/produkte/neu`);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Produkt hinzufügen' })).toBeVisible();
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

    // S34a (S6-17, R-CAN-05, WSP-01): „Ja“ shows the first card; one evidence of a procedure is saved
    const status = main(page).getByRole('radiogroup');
    await expect(main(page).getByRole('radio', { name: 'Nicht bekannt' })).toBeChecked();
    await main(page).getByRole('radio', { name: 'Ja' }).check();
    await expect(main(page).getByRole('heading', { level: 3, name: 'Prüfnachweis 1' })).toBeVisible();
    await main(page).getByRole('combobox', { name: 'Art der Prüfung' }).click();
    await page.getByRole('option', { name: 'BIT inklusiv Software-Test' }).click();
    await expect(main(page).getByRole('heading', { level: 3, name: 'Prüfnachweis 1: BIT inklusiv Software-Test' })).toBeVisible();
    await main(page).getByRole('textbox', { name: 'Wer hat geprüft?' }).fill('Prüfstelle Beispiel');
    await main(page).getByRole('textbox', { name: 'Datum der Prüfung' }).fill('14.03.2026');
    await main(page).getByRole('textbox', { name: 'Geprüfte Version (optional)' }).fill('4.2');
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
    const secondKind = main(page).getByRole('combobox', { name: 'Art der Prüfung' }).nth(1);
    await expect(secondKind).toBeFocused();
    await secondKind.click();
    await page.getByRole('option', { name: 'BIT inklusiv Software-Test' }).click();
    await main(page).getByRole('textbox', { name: 'Wer hat geprüft?' }).nth(1).fill('Eigene Prüfung');
    await main(page).getByRole('textbox', { name: 'Datum der Prüfung' }).nth(1).fill('01.04.2026');
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
    await expect(main(page).getByRole('heading', { level: 3, name: 'Prüfnachweis 1: BIT inklusiv Software-Test' })).toBeFocused();

    // Leaving „Ja“ with an evidence asks first (`candidate.evidence.list.statusChange`)
    await status.getByRole('radio', { name: 'Nein' }).check();
    const dialog = page.getByRole('dialog');
    await expect(dialog.getByRole('heading', { name: 'Prüfnachweise entfernen?' })).toBeFocused();
    await expect(dialog).toContainText('Ihr Prüfnachweis wird dabei entfernt. Möchten Sie fortfahren?');
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's34a-status-change');
    await dialog.getByRole('button', { name: 'Nachweise entfernen' }).press('Enter');
    await expect(dialog).toBeHidden();
    await expect(main(page).getByRole('heading', { level: 2, name: 'Prüfnachweise' })).toBeHidden();
    const removed = page.waitForResponse((response) => response.request().method() === 'DELETE');
    const statusSaved = page.waitForResponse((response) => new URL(response.url()).pathname.endsWith('/test-status'));
    await main(page).getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await removed).status()).toBe(204);
    expect((await statusSaved).status()).toBe(200);

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
  });
});

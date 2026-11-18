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
 * S34. S33 is reached from S32's menu „Produkt hinzufügen“ › „Neu erfassen“, and the path above S34
 * leads back to S32, which lists the product with its test status, progress and seven counts
 * (S6-14; R-PRJ-03, SIG-13). Axe in both appearances on every new state (A11Y-01, A11Y-08).
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
  test('INV:A11Y-01 INV:A11Y-08 INV:SIG-13 an Owner adds a product from S32 in S33, sees S34 Überblick, switches tabs, edits it and finds it on S32', async ({ page, request }, testInfo) => {
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

    await tabs.first().press('Enter');
    await expect(page).toHaveTitle(`Überblick – BewerberPro – ${project} – ProcureAccess`);

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
    await expect(item).toContainText('Nicht bekannt');
    await expect(item).toContainText(`0 von ${total} Kriterien eingeschätzt`);
    // INV:SIG-13: all seven counts, label first, in the order of the scale
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
});

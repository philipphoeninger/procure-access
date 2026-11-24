// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { createOrganizationThroughApi, createProjectThroughApi, registerConfirmedAccount, saveListThroughApi } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';
import { ORG_READER, TEST_USER_PASSWORD } from '../support/users';

/**
 * Procurement projects (S6-13; R-PRJ-01, R-PRJ-02, R-SHL-02): „Projekte“ in the header leads to
 * S30; a Reader of the seeded organisation sees its list without „Projekt anlegen“ (only read,
 * owner decision S5-16 7a); an Owner of an organisation of its own creates a project in S31, which
 * opens S32 and is then listed on S30. Axe in both appearances on every new state (A11Y-01, A11Y-08).
 *
 * S32's dialog „Projekt bearbeiten“ (S6-14; R-PRJ-05): an Owner renames the project and archives it;
 * archived, S32 is read-only — only the status can change, back to „Laufend“.
 *
 * The project's own copy of a list (S6-12; R-PRJ-02, R-PRJ-06, WSP-03): S31 copies a saved list of
 * the organisation into the new project; S11 of the copy fixes the selection and changes the
 * criteria; back on S32 `projects.detail.criteriaAdded` reports the criteria taken in; the copy's id
 * at the address of saved lists leads to the project's address.
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');
const mainNav = (page: Page) => page.getByRole('banner').getByRole('navigation', { name: 'Hauptnavigation' });

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

test.describe('Procurement projects S30, S31, S32', () => {
  test('INV:A11Y-01 INV:A11Y-08 „Projekte“ leads a Reader to S30, current there, without „Projekt anlegen“', async ({ page }, testInfo) => {
    await page.setViewportSize(WIDE);
    await signInThroughApi(page, ORG_READER, TEST_USER_PASSWORD);
    await page.goto('/');

    await expect(mainNav(page).getByRole('link')).toHaveText(['Kriterien', 'Projekte', 'Produkte']);
    await mainNav(page).getByRole('link', { name: 'Projekte' }).press('Enter');

    await expect(page).toHaveURL(/\/projekte$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Beschaffungsprojekte' })).toBeFocused();
    await expect(page).toHaveTitle('Beschaffungsprojekte – ProcureAccess');
    await expect(mainNav(page).getByRole('link', { name: 'Projekte' })).toHaveAttribute('aria-current', 'page');
    await expect(main(page)).toContainText('Halten Sie hier fest, welche Produkte für eine Beschaffung infrage kommen');
    await expect(main(page).getByRole('button', { name: 'Projekt anlegen' })).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's30-reader');
  });

  test('INV:A11Y-01 INV:A11Y-08 an Owner creates a project in S31; S32 opens and S30 lists it', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `projects-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    await createOrganizationThroughApi(page, `Amt ${suffix}`);
    await page.goto('/projekte');

    await expect(main(page)).toContainText('Noch keine Beschaffungsprojekte.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's30-empty');

    await main(page).getByRole('button', { name: 'Projekt anlegen' }).press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Beschaffungsprojekt anlegen' });
    const name = dialog.getByRole('textbox', { name: 'Name des Projekts' });
    await expect(name).toBeFocused();
    // Material adds `mat-form-field-animations-enabled` 300 ms after a field starts; an error shown
    // before that fades in again once the class arrives, and axe measured it mid-fade (CI; as S02)
    await expect(dialog.locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(1);
    await dialog.getByRole('button', { name: 'Projekt anlegen' }).press('Enter');
    await expect(dialog.locator('pa-error-summary pa-notice')).toBeFocused();
    // The field's own error, not only the summary's line with the same text, before axe runs
    await expect(dialog.locator('mat-error')).toHaveText('Bitte geben Sie Name des Projekts an.');
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's31-error-summary');

    const project = `Bewerbermanagement ${suffix}`;
    const created = page.waitForResponse((response) => response.request().method() === 'POST' && new URL(response.url()).pathname.endsWith('/projects'));
    await name.fill(project);
    await name.press('Enter');
    expect((await created).status()).toBe(201);

    await expect(page).toHaveURL(/\/projekte\/[0-9a-f-]{36}$/);
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    await expect(page).toHaveTitle(`${project} – Beschaffungsprojekte – ProcureAccess`);
    await expect(main(page)).toContainText('Stand: Laufend · noch keine Produkte · nur für Ihre Organisation sichtbar');
    await expect(mainNav(page).getByRole('link', { name: 'Projekte' })).toHaveAttribute('aria-current', 'page');
    await expect(main(page)).toContainText('Diesem Projekt ist noch keine Kriterienliste zugeordnet.');
    await expect(main(page)).toContainText('Noch keine Produkte in diesem Projekt.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-new');

    await mainNav(page).getByRole('link', { name: 'Projekte' }).press('Enter');
    await expect(main(page).getByRole('heading', { level: 1, name: 'Beschaffungsprojekte' })).toBeFocused();
    const table = main(page).getByRole('table', { name: 'Beschaffungsprojekte' });
    await expect(table.getByRole('row')).toHaveCount(2);
    await expect(table.getByRole('row').nth(1)).toContainText(project);
    await expect(table.getByRole('row').nth(1)).toContainText('Laufend');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's30-list');
  });

  test('INV:A11Y-01 INV:A11Y-08 S31 offers „Keine Liste“ and the saved lists of the organisation', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `project-choice-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    await page.goto('/projekte');

    await main(page).getByRole('button', { name: 'Projekt anlegen' }).press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Beschaffungsprojekt anlegen' });
    await expect(dialog.getByRole('textbox', { name: 'Name des Projekts' })).toBeFocused();
    await expect(dialog).toContainText('Kriterienliste (optional)');
    await expect(dialog).toContainText('Das Projekt erhält eine eigene Kopie der Liste');
    const choice = dialog.getByRole('combobox', { name: 'Kriterienliste (optional)' });
    await expect(choice).toContainText('Keine Liste');
    await choice.click();
    await expect(page.getByRole('option')).toHaveText(['Keine Liste', 'HR-Software 2027']);
    await page.getByRole('option', { name: 'HR-Software 2027' }).click();
    await expect(page.getByRole('option')).toHaveCount(0);
    await expect(choice).toContainText('HR-Software 2027');
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's31-list-choice');
  });

  test('INV:WSP-03 INV:A11Y-01 INV:A11Y-08 a project created with a list adjusts its own copy in S11; S32 reports the criteria taken in', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `project-copy-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    await saveListThroughApi(page.request, organizationId, 'HR-Software 2027');
    await page.goto('/projekte');

    await main(page).getByRole('button', { name: 'Projekt anlegen' }).press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Beschaffungsprojekt anlegen' });
    await expect(dialog.getByRole('textbox', { name: 'Name des Projekts' })).toBeFocused();
    const project = `Mit Liste ${suffix}`;
    await dialog.getByRole('textbox', { name: 'Name des Projekts' }).fill(project);
    const choice = dialog.getByRole('combobox', { name: 'Kriterienliste (optional)' });
    await expect(choice).toContainText('Keine Liste');
    await choice.click();
    await page.getByRole('option', { name: 'HR-Software 2027' }).click();
    const assigned = page.waitForResponse(
      (response) => response.request().method() === 'PUT' && new URL(response.url()).pathname.endsWith('/criteria-configuration'),
    );
    await dialog.getByRole('button', { name: 'Projekt anlegen' }).click();
    const assignment = await assigned;
    expect(assignment.status()).toBe(200);
    const copyId = ((await assignment.json()) as { id: string }).id;

    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    await expect(main(page).getByRole('link', { name: 'HR-Software 2027' })).toBeVisible();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-with-list');

    // S11 of the copy: the selection is fixed, the criteria change (WSP-03)
    await main(page).getByRole('link', { name: 'Kriterienliste anpassen' }).press('Enter');
    await expect(page).toHaveURL(/\/projekte\/[0-9a-f-]{36}\/kriterienliste$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'HR-Software 2027' })).toBeFocused();
    await expect(page).toHaveTitle(`Kriterienliste – ${project} – Beschaffungsprojekte – ProcureAccess`);
    await expect(main(page)).toContainText(`Diese Liste gehört zum Beschaffungsprojekt „${project}“.`);
    await expect(main(page).getByRole('link', { name: 'Auswahl ändern' })).toHaveCount(0);
    await expect(main(page).getByRole('button', { name: 'Freigabelink erstellen' })).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's11-project-copy');

    const first = main(page).locator('pa-criterion-card').first().getByRole('checkbox', { name: /^In Liste aufnehmen / });
    const decide = (included: boolean) =>
      page.waitForResponse(
        (response) =>
          response.request().method() === 'PATCH' &&
          new URL(response.url()).pathname.includes(`/configurations/${copyId}/criteria/`) &&
          (response.request().postDataJSON() as { isIncluded: boolean }).isIncluded === included,
      );
    const excluded = decide(false);
    await first.uncheck();
    expect((await excluded).ok()).toBe(true);

    const heading = main(page).getByRole('heading', { level: 1 });

    // Leaving out is no addition: S32 shows no report
    await page.goBack();
    await expect(heading).toHaveText(project);
    await expect(heading).toBeFocused();
    await expect(main(page)).not.toContainText('neu in der Liste');

    await main(page).getByRole('link', { name: 'Kriterienliste anpassen' }).press('Enter');
    await expect(heading).toHaveText('HR-Software 2027');
    await expect(heading).toBeFocused();
    const included = decide(true);
    await first.check();
    expect((await included).ok()).toBe(true);

    await page.goBack();
    await expect(heading).toHaveText(project);
    await expect(heading).toBeFocused();
    await expect(main(page)).toContainText('1 Kriterium neu in der Liste. Es ist bei allen Produkten noch nicht eingeschätzt.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-criteria-added');

    // The copy has one address, its project's (owner decision S6-12 D7 (a))
    await page.goto(`/kriterienlisten/${copyId}`);
    await expect(page).toHaveURL(/\/projekte\/[0-9a-f-]{36}\/kriterienliste$/);
    await expect(heading).toHaveText('HR-Software 2027');
  });

  test('INV:A11Y-01 INV:A11Y-08 S32 renames a project and archives it; archived, only its status can change', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
    const address = `projects-edit-${suffix}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${suffix}`);
    const project = await createProjectThroughApi(page.request, organizationId, `Projekt ${suffix}`);
    await page.goto(`/projekte/${project}`);
    await expect(main(page).getByRole('heading', { level: 1, name: `Projekt ${suffix}` })).toBeVisible();

    const edit = main(page).getByRole('button', { name: 'Projekt bearbeiten' });
    await edit.press('Enter');
    const dialog = page.getByRole('dialog', { name: 'Projekt bearbeiten' });
    const name = dialog.getByRole('textbox', { name: 'Name des Projekts' });
    // MatDialog moves the focus when it has opened (S6-01)
    await expect(name).toBeFocused();
    await expect(dialog.getByRole('radio', { name: 'Laufend' })).toBeChecked();
    await expect(dialog.locator('mat-form-field.mat-form-field-animations-enabled')).toHaveCount(1);
    await settled(dialog);
    await expectNoAxeViolations(page, testInfo, 's32-edit-dialog');

    const renamed = `Umbenannt ${suffix}`;
    await name.fill(renamed);
    await dialog.getByRole('radio', { name: 'Archiviert' }).check();
    const changed = page.waitForResponse((response) => response.request().method() === 'PATCH');
    await dialog.getByRole('button', { name: 'Speichern' }).press('Enter');
    expect((await changed).status()).toBe(200);
    await expect(dialog).toBeHidden();
    await expect(edit).toBeFocused();
    await expect(main(page).getByRole('heading', { level: 1, name: renamed })).toBeVisible();
    await expect(page).toHaveTitle(`${renamed} – Beschaffungsprojekte – ProcureAccess`);
    await expect(main(page)).toContainText('Stand: Archiviert');
    await expect(main(page)).toContainText('Dieses Projekt ist archiviert.');
    // Read-only (R-PRJ-05): no menu; „Projekt bearbeiten“ stays for the status
    await expect(main(page).getByRole('button', { name: 'Produkt hinzufügen' })).toHaveCount(0);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-archived');

    await edit.press('Enter');
    await expect(dialog.getByRole('textbox')).toHaveCount(0);
    await expect(dialog.getByRole('radio', { name: 'Archiviert' })).toBeFocused();
    await dialog.getByRole('radio', { name: 'Laufend' }).check();
    await dialog.getByRole('button', { name: 'Speichern' }).press('Enter');
    await expect(dialog).toBeHidden();
    await expect(main(page)).toContainText('Stand: Laufend');
    await expect(main(page).getByRole('button', { name: 'Produkt hinzufügen' })).toBeVisible();
  });
});

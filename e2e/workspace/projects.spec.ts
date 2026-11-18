// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { createOrganizationThroughApi, registerConfirmedAccount, saveListThroughApi } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';
import { ORG_READER, TEST_USER_PASSWORD } from '../support/users';

/**
 * Procurement projects (S6-13; R-PRJ-01, R-PRJ-02, R-SHL-02): „Projekte“ in the header leads to
 * S30; a Reader of the seeded organisation sees its list without „Projekt anlegen“ (only read,
 * owner decision S5-16 7a); an Owner of an organisation of its own creates a project in S31, which
 * opens S32 (the placeholder until S6-14) and is then listed on S30. Axe in both appearances on
 * every new state (A11Y-01, A11Y-08).
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

test.describe('Procurement projects S30, S31', () => {
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
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-placeholder');

    await mainNav(page).getByRole('link', { name: 'Projekte' }).press('Enter');
    await expect(main(page).getByRole('heading', { level: 1, name: 'Beschaffungsprojekte' })).toBeFocused();
    const table = main(page).getByRole('table', { name: 'Beschaffungsprojekte' });
    await expect(table.getByRole('row')).toHaveCount(2);
    await expect(table.getByRole('row').nth(1)).toContainText(project);
    await expect(table.getByRole('row').nth(1)).toContainText('Laufend');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's30-list');
  });

  // Axe on S31 with the list choice in a test of its own (TESTING V341): in Firefox, axe on the dialog
  // with its select crashes the page process when the select or its options were found by name before
  // (as V129) or when the dialog closes after it. So this test finds them by role or CSS and ends with
  // axe; the journey below checks S31 without axe. The names are checked by the unit test.
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
    const dialog = page.locator('mat-dialog-container');
    await expect(dialog.locator('input')).toBeFocused();
    await expect(dialog).toContainText('Kriterienliste (optional)');
    await expect(dialog).toContainText('Das Projekt erhält eine eigene Kopie der Liste');
    const choice = dialog.locator('mat-select');
    await expect(choice).toContainText('Keine Liste');
    await choice.click();
    await expect(page.locator('mat-option')).toHaveText(['Keine Liste', 'HR-Software 2027']);
    await page.locator('mat-option', { hasText: 'HR-Software 2027' }).click();
    await expect(page.locator('mat-option')).toHaveCount(0);
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
    // S31's axe run is a test of its own (below): with the select in the dialog, axe followed by the
    // dialog closing crashes Firefox's page process (TESTING V341)
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

    // The include checkbox by role inside its card, without its name: after axe on S11 the name
    // computation of that checkbox crashes Firefox (TESTING V165, as J1)
    const first = main(page).locator('pa-criterion-card').first().getByRole('checkbox');
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

    // Leaving out is no addition: S32 shows no report
    await page.goBack();
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    await expect(main(page)).not.toContainText('neu in der Liste');

    await main(page).getByRole('link', { name: 'Kriterienliste anpassen' }).press('Enter');
    await expect(main(page).getByRole('heading', { level: 1, name: 'HR-Software 2027' })).toBeFocused();
    const included = decide(true);
    await main(page).locator('pa-criterion-card').first().getByRole('checkbox').check();
    expect((await included).ok()).toBe(true);

    await page.goBack();
    await expect(main(page).getByRole('heading', { level: 1, name: project })).toBeFocused();
    await expect(main(page)).toContainText('1 Kriterium neu in der Liste. Es ist bei allen Produkten noch nicht eingeschätzt.');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's32-criteria-added');

    // The copy has one address, its project's (owner decision S6-12 D7 (a))
    await page.goto(`/kriterienlisten/${copyId}`);
    await expect(page).toHaveURL(/\/projekte\/[0-9a-f-]{36}\/kriterienliste$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'HR-Software 2027' })).toBeVisible();
  });
});

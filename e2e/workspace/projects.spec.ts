// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page } from '@playwright/test';
import { createOrganizationThroughApi, registerConfirmedAccount } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';
import { ORG_READER, TEST_USER_PASSWORD } from '../support/users';

/**
 * Procurement projects (S6-13; R-PRJ-01, R-PRJ-02, R-SHL-02): „Projekte“ in the header leads to
 * S30; a Reader of the seeded organisation sees its list without „Projekt anlegen“ (only read,
 * owner decision S5-16 7a); an Owner of an organisation of its own creates a project in S31, which
 * opens S32 (the placeholder until S6-14) and is then listed on S30. Axe in both appearances on
 * every new state (A11Y-01, A11Y-08).
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
    await dialog.getByRole('button', { name: 'Projekt anlegen' }).press('Enter');
    await expect(dialog.locator('pa-error-summary pa-notice')).toBeFocused();
    await expect(dialog).toContainText('Bitte geben Sie Name des Projekts an.');
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
});

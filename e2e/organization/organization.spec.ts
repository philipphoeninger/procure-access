// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import { registerAccount } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { linksOf, waitForMail } from '../support/mailpit';
import { signInThroughApi } from '../support/session';
import { E2E_ORGANIZATION, ORG_OWNER, ORG_READER, TEST_USER_PASSWORD } from '../support/users';
import { settled } from '../support/keyboard';

/**
 * Organisations S20 and S21 (S5-16b; R-ORG-01, R-ORG-03, R-ORG-04) on the real E2E stack. S20 with
 * a freshly registered account runs SEC-10's E2E part: the gate refuses the unconfirmed address, the
 * notice sends the confirmation again, the link from Mailpit confirms it, and the same form then
 * creates the organisation and opens S21. S21 is read on the seeded organisation, which no test
 * changes (owner decision S5-16 7a): the views of the Owner and a Reader, the last-Owner refusal
 * (nothing changes) and the dialogs cancelled. Axe in both appearances on each new state.
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');
const membersPath = (id: string) => `/organisation/${id}/mitglieder`;

/** A new address per test and browser project: projects share one stack when run locally. */
function newAddress(testInfo: TestInfo): string {
  return `org-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
}

async function openMembers(page: Page, email: string): Promise<void> {
  await signInThroughApi(page, email, TEST_USER_PASSWORD);
  await page.goto(membersPath(E2E_ORGANIZATION.id));
  await expect(main(page).getByRole('heading', { level: 1, name: `Mitglieder von ${E2E_ORGANIZATION.name}` })).toBeVisible();
}

test.describe('S20 „Organisation anlegen oder beitreten“', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:SEC-10 INV:A11Y-01 INV:A11Y-08 an unconfirmed address gets the notice, confirms through the mail and then creates the organisation', async ({
    page,
    request,
  }, testInfo) => {
    const address = newAddress(testInfo);
    const name = `Stadt ${testInfo.project.name} ${Date.now()}`;
    await registerAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);

    await page.goto('/organisation');
    // The first page load moves no focus (R-SHL-04); the route change to S21 below does
    await expect(main(page).getByRole('heading', { level: 1, name: 'Organisation anlegen oder beitreten' })).toBeVisible();
    await expect(page).toHaveTitle(/^Organisation anlegen oder beitreten/);
    await main(page).getByLabel('Name der Organisation').fill(name);
    await main(page).getByRole('radio', { name: 'Öffentliche Stelle' }).check();
    await main(page).getByRole('button', { name: 'Organisation anlegen' }).click();

    // The gate (SEC-10): the notice takes the focus, the input stays
    await expect(page.locator('.pa-org-start__confirm')).toBeFocused();
    await expect(main(page).getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeVisible();
    await expect(main(page).getByLabel('Name der Organisation')).toHaveValue(name);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's20-confirm-required');

    await main(page).getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' }).click();
    await expect(main(page).getByText(new RegExp(`an ${address.replaceAll('.', '\\.')}`))).toBeVisible();
    const mail = await waitForMail(request, address, 2);
    const link = linksOf(mail.html).find((href) => href.includes('/registrieren/bestaetigen?token='));
    expect(link, 'the confirmation link').toBeTruthy();

    // Confirmed in a second tab; the form keeps its input
    const other = await page.context().newPage();
    await other.goto(new URL(link!).pathname + new URL(link!).search);
    await expect(other.getByRole('heading', { level: 1, name: 'E-Mail-Adresse bestätigt' })).toBeVisible();
    await other.close();

    await main(page).getByRole('button', { name: 'Organisation anlegen' }).click();
    await expect(main(page).getByRole('heading', { level: 1, name: `Mitglieder von ${name}` })).toBeFocused();
    await expect(page).toHaveURL(/\/organisation\/[0-9a-f-]{36}\/mitglieder$/);
    const row = main(page).getByRole('row', { name: new RegExp(address.replaceAll('.', '\\.')) });
    await expect(row.getByText('(Sie)')).toBeVisible();
    await expect(row.getByRole('combobox', { name: `Rolle von ${address}` })).toHaveText(/Verwalten/);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's21-new-organization');
  });
});

test.describe('S21 „Mitglieder“ of the seeded organisation', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 INV:A11Y-08 the Owner sees every member with a role select and is refused as the last Owner (TEN-11)', async ({
    page,
  }, testInfo) => {
    await openMembers(page, ORG_OWNER);
    await expect(main(page).getByRole('columnheader')).toHaveText(['Mitglied', 'Rolle', 'Aktionen']);
    await expect(main(page).getByRole('combobox')).toHaveCount(3);
    await expect(main(page).getByRole('button', { name: 'Aus der Organisation entfernen' })).toHaveCount(2);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's21-owner');

    // The only Owner gives up the role: 409 last-owner, the notice above the list, nothing changes
    await main(page).getByRole('combobox', { name: `Rolle von ${ORG_OWNER}` }).click();
    await page.getByRole('option', { name: /^Bearbeiten/ }).click();
    const alert = main(page).getByRole('alert');
    await expect(alert).toContainText('Änderung nicht möglich');
    await expect(alert).toContainText('Jede Organisation braucht mindestens ein Mitglied mit der Rolle „Verwalten“.');
    await expect(main(page).getByRole('combobox', { name: `Rolle von ${ORG_OWNER}` })).toHaveText(/Verwalten/);
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's21-last-owner');
  });

  test('cancelling „entfernen“ and „Organisation verlassen“ changes nothing and returns the focus', async ({ page }) => {
    await openMembers(page, ORG_OWNER);

    const remove = main(page).getByRole('button', { name: 'Aus der Organisation entfernen' }).first();
    await remove.click();
    const dialog = page.getByRole('dialog');
    await expect(dialog.getByRole('heading', { name: 'Mitglied entfernen?' })).toBeFocused();
    await dialog.getByRole('button', { name: 'Abbrechen' }).click();
    await expect(dialog).toBeHidden();
    await expect(remove).toBeFocused();

    const leave = main(page).getByRole('button', { name: 'Organisation verlassen' });
    await leave.click();
    await expect(page.getByRole('dialog').getByRole('heading', { name: 'Organisation verlassen' })).toBeFocused();
    await page.keyboard.press('Escape');
    await expect(page.getByRole('dialog')).toBeHidden();
    await expect(leave).toBeFocused();
    await expect(main(page).getByRole('combobox')).toHaveCount(3);
  });

  test('INV:A11Y-01 INV:A11Y-08 a Reader sees the roles as text and only „Organisation verlassen“', async ({ page }, testInfo) => {
    await openMembers(page, ORG_READER);

    await expect(main(page).getByRole('combobox')).toHaveCount(0);
    await expect(main(page).getByRole('button', { name: 'Aus der Organisation entfernen' })).toHaveCount(0);
    await expect(main(page).getByRole('row', { name: new RegExp(ORG_OWNER.replaceAll('.', '\\.')) })).toContainText('Verwalten');
    await expect(main(page).getByRole('button', { name: 'Organisation verlassen' })).toBeVisible();
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's21-reader');
  });

  test('an organisation the user is no member of shows „Seite nicht gefunden“ (TEN-05)', async ({ page }) => {
    await signInThroughApi(page, ORG_OWNER, TEST_USER_PASSWORD);

    await page.goto(membersPath('00000000-0000-4000-8000-000000000000'));

    await expect(main(page).getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeVisible();
    await expect(page).toHaveTitle(/^Seite nicht gefunden/);
  });
});

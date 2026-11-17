// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { readFile } from 'node:fs/promises';
import { expect, test, type Page } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { createOrganizationThroughApi, registerConfirmedAccount, saveListThroughApi } from '../support/accounts';
import { accountMenuButton, signInThroughApi } from '../support/session';
import { E2E_ORGANIZATION, ORG_OWNER, TEST_USER_PASSWORD } from '../support/users';

/**
 * The account page S61 (S5-28b; SPEC R-AUTH-09, R-AUTH-10, §8.13; TESTING TEN-11): the download of
 * the personal data export and the account deletion through the dialog „Konto endgültig löschen?“.
 * The first test builds its own account with an organisation and a list and deletes it. The second
 * only reads: the seeded Owner is the only member with „Verwalten“ of the seeded organisation, so
 * the deletion is refused and nothing changes.
 */

const PASSWORD = 'e2e-Konto-Passwort-1';
const REASON = 'Das Löschen ist erst möglich, wenn Sie Ihre E-Mail-Adresse genau so eingeben, wie sie in Ihrem Konto steht.';

/** Opens S61 through „Konto“ in the menu „Konto“, or in the compact menu on a narrow screen. */
async function openAccount(page: Page): Promise<void> {
  if (await accountMenuButton(page).isVisible()) {
    await accountMenuButton(page).click();
    await page.getByRole('menuitem', { name: 'Konto', exact: true }).click();
  } else {
    await page.getByRole('button', { name: 'Menü', exact: true }).click();
    await page.getByRole('dialog', { name: 'Menü' }).getByRole('link', { name: 'Konto', exact: true }).click();
  }
  await expect(page.getByRole('heading', { level: 1, name: 'Konto' })).toBeVisible();
}

test.describe('Account page S61 (R-AUTH-09, R-AUTH-10)', () => {
  test('INV:TEN-11 INV:A11Y-01 INV:A11Y-08 a user downloads the data, deletes the account with its organisation and cannot sign in again', async ({
    page,
    request,
  }, testInfo) => {
    const address = `account-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
    await registerConfirmedAccount(request, address, PASSWORD);
    await signInThroughApi(page, address, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, 'Konto-Test');
    await saveListThroughApi(page.request, organizationId, 'Liste des Kontos');
    await page.goto('/');
    await expect(page.getByRole('heading', { level: 1 })).toBeVisible();
    const main = page.getByRole('main');

    await test.step('S61 opens from the menu „Konto“ and passes axe', async () => {
      await openAccount(page);
      await expect(page).toHaveURL(/\/konto$/);
      await expect(page).toHaveTitle(/^Konto/);
      await expect(main.getByRole('heading', { level: 2 })).toHaveText(['Ihre Daten herunterladen', 'Konto löschen']);
      // A confirmed address: no confirmation notice
      await expect(main.getByText('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse')).toHaveCount(0);
      await expectNoAxeViolations(page, testInfo, 's61');
    });

    await test.step('„Daten herunterladen“ hands over the JSON file with the user’s own data', async () => {
      const downloading = page.waitForEvent('download');
      await main.getByRole('button', { name: 'Daten herunterladen' }).click();
      const download = await downloading;
      expect(download.suggestedFilename()).toMatch(/^ProcureAccess_Daten_\d{4}-\d{2}-\d{2}\.json$/);
      const file = JSON.parse(await readFile(await download.path(), 'utf8')) as {
        account: { email: string };
        memberships: { organization: string }[];
        createdByMe: { criteriaLists: { name: string }[] };
      };
      expect(file.account.email).toBe(address);
      expect(file.memberships.map((m) => m.organization)).toEqual(['Konto-Test']);
      expect(file.createdByMe.criteriaLists.map((l) => l.name)).toEqual(['Liste des Kontos']);
    });

    const dialog = page.getByRole('dialog');
    const confirm = dialog.getByRole('button', { name: 'Konto endgültig löschen' });

    await test.step('the dialog keeps the action unavailable with its reason until the address is typed', async () => {
      await main.getByRole('button', { name: 'Konto löschen' }).click();
      await expect(dialog.getByRole('heading', { name: 'Konto endgültig löschen?' })).toBeFocused();
      await expect(confirm).toHaveAttribute('aria-disabled', 'true');
      // Not natively disabled: it stays focusable
      await expect(confirm).toHaveJSProperty('disabled', false);
      await expect(confirm).toHaveAccessibleDescription(REASON);
      await expect(dialog.getByText(REASON)).toBeVisible();
      await expectNoAxeViolations(page, testInfo, 's61-delete-dialog');

      // Activating the unavailable action by keyboard does nothing
      await confirm.focus();
      await expect(confirm).toBeFocused();
      await page.keyboard.press('Enter');
      await expect(dialog).toBeVisible();
      await expect(page).toHaveURL(/\/konto$/);

      await dialog.getByRole('textbox', { name: 'E-Mail-Adresse' }).fill(address);
      await expect(confirm).toHaveAttribute('aria-disabled', 'false');
      await expect(dialog.getByText(REASON)).toHaveCount(0);
    });

    await test.step('deleting ends the session and shows the start page with the focus on its H1', async () => {
      await confirm.click();
      await expect(page).toHaveURL(/\/$/);
      await expect(main.getByRole('heading', { level: 1 })).toBeFocused();
      await expect(accountMenuButton(page)).toHaveCount(0);
      const session = (await (await page.request.get('/api/v1/auth/session')).json()) as { user: unknown };
      expect(session.user).toBeNull();
      // Nothing of the user is kept in the browser (SEC-01)
      expect(await page.evaluate(() => window.localStorage.length + window.sessionStorage.length)).toBe(0);
    });

    await test.step('the address and the password open nothing any more', async () => {
      const token = (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
      const login = await page.request.post('/api/v1/auth/login', {
        headers: { 'X-XSRF-TOKEN': token! },
        data: { email: address, password: PASSWORD },
      });
      expect(login.status()).toBe(400);
    });
  });

  test('INV:TEN-11 INV:A11Y-01 INV:A11Y-08 the only member with „Verwalten“ of an organisation with other members cannot delete the account', async ({
    page,
  }, testInfo) => {
    // The seeded Owner and organisation are only read: the refusal changes nothing
    await signInThroughApi(page, ORG_OWNER, TEST_USER_PASSWORD);
    await page.goto('/konto');
    const main = page.getByRole('main');
    await expect(main.getByRole('heading', { level: 1, name: 'Konto' })).toBeVisible();

    await main.getByRole('button', { name: 'Konto löschen' }).click();
    const dialog = page.getByRole('dialog');
    await dialog.getByRole('textbox', { name: 'E-Mail-Adresse' }).fill(ORG_OWNER);
    await dialog.getByRole('button', { name: 'Konto endgültig löschen' }).click();

    const alert = main.getByRole('alert');
    await expect(alert).toContainText('Konto nicht gelöscht');
    await expect(alert).toContainText(`Sie sind das einzige Mitglied mit der Rolle „Verwalten“ in ${E2E_ORGANIZATION.name}.`);
    await expect(page).toHaveURL(/\/konto$/);
    await expectNoAxeViolations(page, testInfo, 's61-sole-owner');

    // Still signed in, and the organisation is still the user's
    const session = (await (await page.request.get('/api/v1/auth/session')).json()) as { user: { email: string } | null };
    expect(session.user?.email).toBe(ORG_OWNER);
    const organizations = (await (await page.request.get('/api/v1/organizations')).json()) as { id: string }[];
    expect(organizations.map((o) => o.id)).toContain(E2E_ORGANIZATION.id);
  });
});

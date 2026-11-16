// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';
import { organizationOfAccount, registerConfirmedAccount, saveListThroughApi } from '../support/accounts';
import { accountMenuButton, signInThroughApi } from '../support/session';

/**
 * S15 „Freigabelink erstellen“ and S16 „Geteilte Kriterienliste“ (S5-24; SPEC R-SAV-04, R-SAV-05;
 * TESTING TEN-09): an Owner shares a saved list, someone without an account reads it read only,
 * and after the revocation the link shows „Link nicht mehr gültig“. The test builds its own
 * account, organisation and list, so the seeded ones stay unchanged.
 */

const PASSWORD = 'e2e-Teilen-Passwort-1';

test.describe('S15 and S16 (R-SAV-04, R-SAV-05)', () => {
  test('INV:TEN-09 INV:A11Y-01 INV:A11Y-08 a link of S15 opens the list read only without an account, and not after its revocation', async ({
    page,
    request,
    browser,
  }, testInfo) => {
    const address = `share-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
    const name = `Geteilte Liste ${testInfo.project.name} ${Date.now()}`;
    await registerConfirmedAccount(request, address, PASSWORD);
    const organizationId = await organizationOfAccount(request, address, PASSWORD, `Amt ${testInfo.project.name} ${Date.now()}`);
    const listId = await saveListThroughApi(request, organizationId, name);
    await signInThroughApi(page, address, PASSWORD);

    const link = await test.step('S15: the Owner creates a link with the default date and sees it once', async () => {
      await page.goto(`/kriterienlisten/${listId}`);
      const main = page.getByRole('main');
      await expect(main.getByRole('heading', { level: 1, name })).toBeVisible();
      const share = main.getByRole('button', { name: 'Freigabelink erstellen' });
      await share.click();
      const dialog = page.getByRole('dialog', { name: 'Freigabelink erstellen' });
      const date = dialog.getByLabel(/^Gültig bis/);
      await expect(date).toBeFocused();
      await expect(date).not.toHaveValue('');
      await expect(dialog.getByText('Für diese Liste gibt es keinen gültigen Link.')).toBeVisible();
      await expectNoAxeViolations(page, testInfo, 's15-open');

      await dialog.getByRole('button', { name: 'Link erstellen' }).click();
      const field = dialog.getByRole('textbox', { name: 'Freigabelink' });
      await expect(field).toBeFocused();
      await expect(dialog.getByRole('button', { name: 'Link ungültig machen' })).toHaveCount(1);
      await expectNoAxeViolations(page, testInfo, 's15-created');
      const value = await field.inputValue();
      expect(value).toMatch(/\/geteilt\/[A-Za-z0-9_-]{43}$/);
      return new URL(value).pathname;
    });

    const reader = await browser.newContext();
    try {
      const readerPage = await reader.newPage();
      await test.step('S16: without any cookie the list shows read only', async () => {
        await readerPage.goto(link);
        const main = readerPage.getByRole('main');
        await expect(main.getByRole('heading', { level: 1, name: `Geteilte Kriterienliste: ${name}` })).toBeVisible();
        await expect(readerPage).toHaveTitle(new RegExp(`^Geteilte Kriterienliste: ${name}`));
        await expect(main.getByRole('heading', { level: 2 }).first()).toBeVisible();
        await expect(main.getByRole('checkbox')).toHaveCount(0);
        await expect(main.getByRole('button', { name: /^Priorität/ })).toHaveCount(0);
        await expect(main.getByRole('button', { name: 'Liste exportieren' })).toHaveCount(0);
        await expect(accountMenuButton(readerPage)).toHaveCount(0);
        await expectNoAxeViolations(readerPage, testInfo, 's16');
      });

      await test.step('S15: the Owner revokes the link; S16 says it is no longer valid', async () => {
        const dialog = page.getByRole('dialog', { name: 'Freigabelink erstellen' });
        await dialog.getByRole('button', { name: 'Link ungültig machen' }).click();
        await expect(dialog.getByRole('heading', { level: 3, name: 'Gültige Links' })).toBeFocused();
        await expect(dialog.getByRole('textbox', { name: 'Freigabelink' })).toHaveCount(0);

        await readerPage.reload();
        await expect(readerPage.getByRole('main').getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeVisible();
        await expectNoAxeViolations(readerPage, testInfo, 's16-invalid');
      });
    } finally {
      await reader.close();
    }
  });
});

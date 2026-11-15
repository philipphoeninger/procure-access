// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import {
  createOrganizationThroughApi,
  invitationLinkOf,
  inviteThroughApi,
  registerAccount,
  registerConfirmedAccount,
} from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { signInThroughApi } from '../support/session';
import { CONFIRMED_USER, TEST_USER_PASSWORD } from '../support/users';

/**
 * Invitations (S5-17b; R-ORG-02, R-ORG-03) on the real E2E stack, each test on an organisation of
 * its own with freshly registered accounts, so it may change it (never the seeded one, owner
 * decision S5-16 7a). The journey: the Owner invites on S21, the invitee opens the link from
 * Mailpit signed out, signs in on S01 over the page, accepts and lands on S21; the Owner then
 * changes the new member's role and removes them (S5-16b open point 2). SEC-10's E2E accept case:
 * an unconfirmed invitee gets the notice. Axe in both appearances on each new state.
 */
const WIDE = { width: 1280, height: 800 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');
const membersPath = (id: string) => `/organisation/${id}/mitglieder`;
const escape = (text: string) => text.replaceAll('.', '\\.');

/** A new address per test, role and browser project: projects share one stack when run locally. */
function newAddress(testInfo: TestInfo, role: string): string {
  return `inv-${role}-${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}@example.test`;
}

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

test.describe('Invitations', () => {
  test.beforeEach(async ({ page }) => {
    await page.setViewportSize(WIDE);
  });

  test('INV:A11Y-01 INV:A11Y-08 the Owner invites on S21, the invitee signs in on the link page and accepts, the Owner changes the role and removes the member', async ({
    page,
    browser,
    request,
  }, testInfo) => {
    const owner = newAddress(testInfo, 'owner');
    const invitee = newAddress(testInfo, 'invitee');
    const name = `Verein ${testInfo.project.name} ${Date.now()}`;
    await registerConfirmedAccount(request, owner, PASSWORD);
    await registerConfirmedAccount(request, invitee, PASSWORD);
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, name);

    // S21: the Owner invites with the role „Bearbeiten“
    await page.goto(membersPath(organizationId));
    await expect(main(page).getByRole('heading', { level: 1, name: `Mitglieder von ${name}` })).toBeVisible();
    // CSS locators only until the axe run below: Playwright's name and label lookups on this form
    // followed by its reset crash Firefox in the next axe run (TESTING V226, like V129 and V165);
    // the roles and names are checked by the component tests and by axe
    const address = page.locator('#pa-invite-email');
    await address.fill(invitee);
    await page.locator('#pa-invite-role-editor-input').check();
    await page.locator('pa-invite-form button[type="submit"]').click();
    const pending = page.locator('.pa-members__pending li');
    await expect(pending).toHaveCount(1);
    await expect(pending).toContainText(invitee);
    await expect(pending).toContainText('Eingeladen, noch nicht angenommen');
    await expect(address).toHaveValue('');
    await settled(main(page));
    await expectNoAxeViolations(page, testInfo, 's21-invitation-pending');

    // The invitee opens the link signed out: organisation and role, the token gone from the address
    const link = await invitationLinkOf(request, invitee, 2);
    const inviteeContext = await browser.newContext({ viewport: WIDE });
    const other = await inviteeContext.newPage();
    try {
      await other.goto(link);
      await expect(other).toHaveURL(/\/organisation\/einladung$/);
      await expect(main(other).getByRole('heading', { level: 1, name: 'Einladung in eine Organisation' })).toBeVisible();
      await expect(other).toHaveTitle(/^Einladung in eine Organisation/);
      await expect(main(other).getByText(`Sie wurden in die Organisation ${name} eingeladen. Ihre Rolle: Bearbeiten.`)).toBeVisible();
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'invitation-signed-out');

      // S01 over the page; after the sign-in the page offers „Einladung annehmen“
      await main(other).getByRole('button', { name: 'Anmelden' }).click();
      await main(other).getByLabel('E-Mail-Adresse').fill(invitee);
      await main(other).getByLabel('Passwort', { exact: true }).fill(PASSWORD);
      await main(other).getByLabel('Passwort', { exact: true }).press('Enter');
      const accept = main(other).getByRole('button', { name: 'Einladung annehmen' });
      await expect(accept).toBeVisible();
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'invitation-signed-in');

      await accept.click();
      await expect(main(other).getByRole('heading', { level: 1, name: `Mitglieder von ${name}` })).toBeFocused();
      await expect(other).toHaveURL(new RegExp(`${membersPath(organizationId)}$`));
      await expect(main(other).getByRole('row', { name: new RegExp(escape(invitee)) })).toContainText('(Sie)');
      await expect(main(other).getByRole('row', { name: new RegExp(escape(invitee)) })).toContainText('Bearbeiten');
    } finally {
      await inviteeContext.close();
    }

    // The Owner: the new member is listed, the invitation is gone; role change and removal
    await page.reload();
    await expect(main(page).getByRole('region', { name: 'Offene Einladungen' })).toHaveCount(0);
    const select = main(page).getByRole('combobox', { name: `Rolle von ${invitee}` });
    await expect(select).toHaveText(/Bearbeiten/);
    await select.click();
    await page.getByRole('option', { name: /^Lesen/ }).click();
    await expect(main(page).getByRole('combobox', { name: `Rolle von ${invitee}` })).toHaveText(/Lesen/);

    await main(page).getByRole('row', { name: new RegExp(escape(invitee)) }).getByRole('button', { name: 'Aus der Organisation entfernen' }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Aus der Organisation entfernen' }).click();
    await expect(main(page).getByRole('row', { name: new RegExp(escape(invitee)) })).toHaveCount(0);
    await expect(main(page).getByRole('row')).toHaveCount(2); // header and the Owner
  });

  test('INV:SEC-10 an invitee with an unconfirmed address gets the notice instead of a membership', async ({ page, browser, request }, testInfo) => {
    const owner = newAddress(testInfo, 'owner');
    const invitee = newAddress(testInfo, 'unconfirmed');
    await registerConfirmedAccount(request, owner, PASSWORD);
    await registerAccount(request, invitee, PASSWORD);
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Amt ${testInfo.project.name} ${Date.now()}`);
    await inviteThroughApi(page, organizationId, invitee, 'reader');

    const link = await invitationLinkOf(request, invitee, 2);
    const inviteeContext = await browser.newContext({ viewport: WIDE });
    const other = await inviteeContext.newPage();
    try {
      await signInThroughApi(other, invitee, PASSWORD);
      await other.goto(link);
      await main(other).getByRole('button', { name: 'Einladung annehmen' }).click();

      await expect(other.locator('.pa-invitation__notice').first()).toBeFocused();
      await expect(main(other).getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse/)).toBeVisible();
      await expect(main(other).getByRole('button', { name: 'Einladung annehmen' })).toBeVisible();
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'invitation-confirm-required');
    } finally {
      await inviteeContext.close();
    }

    // Still open: the Owner sees the invitation, nobody joined
    await page.goto(membersPath(organizationId));
    await expect(main(page).getByRole('region', { name: 'Offene Einladungen' }).getByRole('listitem')).toContainText(invitee);
  });

  test('another signed-in address gets the invited address and no button; an unknown link „nicht mehr gültig“', async ({
    page,
    browser,
    request,
  }, testInfo) => {
    const owner = newAddress(testInfo, 'owner');
    const invitee = newAddress(testInfo, 'other');
    await registerConfirmedAccount(request, owner, PASSWORD);
    await signInThroughApi(page, owner, PASSWORD);
    const organizationId = await createOrganizationThroughApi(page, `Firma ${testInfo.project.name} ${Date.now()}`);
    await inviteThroughApi(page, organizationId, invitee, 'reader');
    const link = await invitationLinkOf(request, invitee, 1);

    const otherContext = await browser.newContext({ viewport: WIDE });
    const other = await otherContext.newPage();
    try {
      await signInThroughApi(other, CONFIRMED_USER, TEST_USER_PASSWORD);
      await other.goto(link);
      await expect(main(other).getByText(new RegExp(`Melden Sie sich mit ${escape(invitee)} an`))).toBeVisible();
      await expect(main(other).getByRole('button', { name: 'Einladung annehmen' })).toHaveCount(0);
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'invitation-mismatch');

      await other.goto('/organisation/einladung?token=unbekannt');
      await expect(main(other).getByRole('heading', { level: 1, name: 'Einladung nicht mehr gültig' })).toBeVisible();
      await expect(other).toHaveTitle(/^Einladung nicht mehr gültig/);
      await expect(main(other).getByRole('link', { name: 'Zur Startseite' })).toBeVisible();
      await settled(main(other));
      await expectNoAxeViolations(other, testInfo, 'invitation-invalid');
    } finally {
      await otherContext.close();
    }
  });
});

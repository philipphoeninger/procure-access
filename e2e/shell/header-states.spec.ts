// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Locator, type Page, type TestInfo } from '@playwright/test';
import { createOrganizationThroughApi, registerConfirmedAccount } from '../support/accounts';
import { expectNoAxeViolations } from '../support/axe';
import { accountMenuButton, openAccountMenu, signInThroughApi } from '../support/session';
import { CONFIRMED_USER, E2E_ORGANIZATION, ORG_READER, TEST_USER_PASSWORD } from '../support/users';

/**
 * The signed-in header (S5-18; R-SHL-02, R-SHL-03): SignedInNoOrg with the seeded confirmed user
 * (no membership), SignedIn with the seeded Reader of „E2E-Organisation“ (both only read, owner
 * decision S5-16 7a), and the organisation switch with an account of its own and two
 * organisations, in the full header (menu button) and in the compact menu (list). Axe in both
 * appearances on each new state (A11Y-01, A11Y-08), on the start page: the legacy product
 * pages have axe findings of their own until they are rebuilt.
 */
const WIDE = { width: 1280, height: 800 };
const COMPACT = { width: 360, height: 780 };
const PASSWORD = 'e2e-Passwort-lang-genug';

const main = (page: Page) => page.getByRole('main');
const banner = (page: Page) => page.getByRole('banner');

/** Waits until Material's animations are over, so axe sees the final colours. */
async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

/** A confirmed account of its own with two organisations; returns their names and ids, by name. */
async function userWithTwoOrganizations(page: Page, request: Page['request'], testInfo: TestInfo) {
  const suffix = `${testInfo.project.name}-${testInfo.workerIndex}-${Date.now()}`;
  const address = `header-${suffix}@example.test`;
  await registerConfirmedAccount(request, address, PASSWORD);
  await signInThroughApi(page, address, PASSWORD);
  const verein = { name: `Verein ${suffix}`, id: '' };
  const amt = { name: `Amt ${suffix}`, id: '' };
  // Created in the other order than the header lists them: the list is by name.
  verein.id = await createOrganizationThroughApi(page, verein.name);
  amt.id = await createOrganizationThroughApi(page, amt.name);
  return { amt, verein };
}

test.describe('Signed-in header', () => {
  test('INV:A11Y-01 INV:A11Y-08 SignedInNoOrg: „Konto“ without organisation display; „Organisation“ opens S20', async ({ page }, testInfo) => {
    await page.setViewportSize(WIDE);
    await signInThroughApi(page, CONFIRMED_USER, TEST_USER_PASSWORD);
    await page.goto('/');

    await expect(accountMenuButton(page)).toHaveText('Konto');
    await expect(banner(page).getByText(/^Organisation:/)).toHaveCount(0);
    await expect(banner(page).getByRole('link', { name: 'Projekte' })).toHaveCount(0);

    const menu = await openAccountMenu(page);
    // „Einstellungen“ opens S60 (S5-26); „Kriterienlisten“ only with a membership (S5-20b 4a)
    await expect(menu.getByRole('menuitem')).toHaveText(['Konto', 'Organisation', 'Einstellungen', 'Abmelden']);
    await settled(menu);
    await expectNoAxeViolations(page, testInfo, 'header-signed-in-no-org-menu');

    await menu.getByRole('menuitem', { name: 'Organisation' }).press('Enter');
    await expect(page).toHaveURL(/\/organisation$/);
    await expect(main(page).getByRole('heading', { level: 1, name: 'Organisation anlegen oder beitreten' })).toBeFocused();
  });

  test('INV:A11Y-01 INV:A11Y-08 SignedIn: the organisation display; „Organisation“ opens S21 and is marked current there', async ({ page }, testInfo) => {
    await page.setViewportSize(WIDE);
    await signInThroughApi(page, ORG_READER, TEST_USER_PASSWORD);
    await page.goto('/');

    await expect(banner(page).getByText(`Organisation: ${E2E_ORGANIZATION.name}`)).toBeVisible();
    // One membership: the display is text, not the switch
    await expect(banner(page).getByRole('button', { name: /^Organisation:/ })).toHaveCount(0);
    await settled(banner(page));
    await expectNoAxeViolations(page, testInfo, 'header-signed-in');

    const menu = await openAccountMenu(page);
    await menu.getByRole('menuitem', { name: 'Organisation' }).press('Enter');
    await expect(page).toHaveURL(new RegExp(`/organisation/${E2E_ORGANIZATION.id}/mitglieder$`));
    await expect(main(page).getByRole('heading', { level: 1, name: `Mitglieder von ${E2E_ORGANIZATION.name}` })).toBeFocused();

    const again = await openAccountMenu(page);
    await expect(again.getByRole('menuitem', { name: 'Organisation' })).toHaveAttribute('aria-current', 'page');
    await expect(again.getByRole('menuitem', { name: 'Konto' })).not.toHaveAttribute('aria-current');
  });

  test('INV:A11Y-01 INV:A11Y-08 switch: a menu of the organisations by name; choosing one on S21 opens its S21', async ({ page, request }, testInfo) => {
    await page.setViewportSize(WIDE);
    const { amt, verein } = await userWithTwoOrganizations(page, request, testInfo);
    await page.goto(`/organisation/${amt.id}/mitglieder`);

    // The first by name is active after the sign-in (owner decision S5-18 3a)
    const switchButton = banner(page).getByRole('button', { name: `Organisation: ${amt.name}` });
    await switchButton.press('Enter');
    const menu = page.getByRole('menu', { name: 'Organisation wechseln' });
    const options = menu.getByRole('menuitemradio');
    await expect(options).toHaveCount(2);
    await expect(options.nth(0)).toContainText(amt.name);
    await expect(options.nth(0)).toHaveAttribute('aria-checked', 'true');
    await expect(options.nth(1)).toContainText(verein.name);
    await expect(options.nth(1)).toHaveAttribute('aria-checked', 'false');
    await settled(menu);
    await expectNoAxeViolations(page, testInfo, 'header-switch-menu');

    await options.nth(1).press('Enter');

    await expect(page).toHaveURL(new RegExp(`/organisation/${verein.id}/mitglieder$`));
    await expect(main(page).getByRole('heading', { level: 1, name: `Mitglieder von ${verein.name}` })).toBeFocused();
    await expect(banner(page).getByRole('button', { name: `Organisation: ${verein.name}` })).toBeVisible();

    // The choice stays while the page changes (in memory, for the session; 3a)
    await banner(page).getByRole('link', { name: /^ProcureAccess/ }).press('Enter');
    await expect(page).toHaveURL(/:\d+\/$/);
    await expect(banner(page).getByRole('button', { name: `Organisation: ${verein.name}` })).toBeVisible();
    const account = await openAccountMenu(page);
    await expect(account.getByRole('menuitem', { name: 'Organisation' })).toHaveAttribute('href', `/organisation/${verein.id}/mitglieder`);
  });

  test('INV:A11Y-01 INV:A11Y-08 switch in the compact menu: a list of the organisations, the active one current', async ({ page, request }, testInfo) => {
    await page.setViewportSize(COMPACT);
    const { amt, verein } = await userWithTwoOrganizations(page, request, testInfo);
    await page.goto('/');

    const menuButton = page.getByRole('button', { name: 'Menü', exact: true });
    await menuButton.click();
    const drawer = page.getByRole('dialog', { name: 'Menü' });
    await expect(drawer.getByText(`Organisation: ${amt.name}`)).toBeVisible();
    const list = drawer.getByRole('list', { name: 'Organisation wechseln' });
    await expect(list.getByRole('button', { name: amt.name })).toHaveAttribute('aria-current', 'true');
    await expect(list.getByRole('button', { name: verein.name })).not.toHaveAttribute('aria-current');
    await expect(drawer.getByRole('link', { name: 'Organisation', exact: true })).toHaveAttribute('href', `/organisation/${amt.id}/mitglieder`);
    await settled(drawer);
    await expectNoAxeViolations(page, testInfo, 'compact-menu-switch');

    await list.getByRole('button', { name: verein.name }).press('Enter');

    // The menu closes and the page stays
    await expect(menuButton).toHaveAttribute('aria-expanded', 'false');
    await expect(page).toHaveURL(/:\d+\/$/);
    await menuButton.click();
    await expect(drawer.getByText(`Organisation: ${verein.name}`)).toBeVisible();
    await expect(list.getByRole('button', { name: verein.name })).toHaveAttribute('aria-current', 'true');
  });
});

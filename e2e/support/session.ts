// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Answers the next request to `url` with 401, as after an expired session (R-AUTH-07); later ones
 * reach the stack. Sessions themselves are real since S5-14: tests sign in as the seeded E2E users
 * (`support/users.ts`).
 */
export async function expireSessionOnce(page: Page, url: string): Promise<void> {
  let expired = false;
  await page.route(url, async (route) => {
    if (expired) {
      await route.continue();
      return;
    }
    expired = true;
    await route.fulfill({ status: 401 });
  });
}

/**
 * Signs `page` in through the API with the probe's antiforgery token (S5-16b): `page.request`
 * shares the browser context's cookies, so the page is signed in from its next navigation. For
 * tests whose subject is not the sign-in (S01 is tested in identity/login.spec.ts).
 */
export async function signInThroughApi(page: Page, email: string, password: string): Promise<void> {
  expect((await page.request.get('/api/v1/auth/session')).ok()).toBe(true);
  const token = (await page.context().cookies()).find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
  expect(token, 'the probe issues XSRF-TOKEN').toBeTruthy();
  const response = await page.request.post('/api/v1/auth/login', {
    headers: { 'X-XSRF-TOKEN': token! },
    data: { email, password },
  });
  expect(response.status(), `sign-in of ${email}`).toBe(204);
}

/**
 * „Konto“ of the full header (S5-18): a menu button whose name names the signed-in address
 * (`shell.userMenu.label`); it is there exactly while somebody is signed in.
 */
export function accountMenuButton(page: Page): Locator {
  return page.getByRole('banner').getByRole('button', { name: /^Konto, angemeldet als / });
}

/** Opens the menu „Konto“ of the full header with the keyboard and returns the menu. */
export async function openAccountMenu(page: Page): Promise<Locator> {
  await accountMenuButton(page).press('Enter');
  const menu = page.getByRole('menu');
  await expect(menu.getByRole('menuitem').first()).toBeFocused();
  return menu;
}

/** „Abmelden“ in the menu „Konto“ of the full header, by keyboard (R-AUTH-08). */
export async function signOutThroughHeader(page: Page): Promise<void> {
  const menu = await openAccountMenu(page);
  await menu.getByRole('menuitem', { name: 'Abmelden' }).press('Enter');
}

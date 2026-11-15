// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type Page } from '@playwright/test';

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

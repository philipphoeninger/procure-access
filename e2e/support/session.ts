// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { Page } from '@playwright/test';

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

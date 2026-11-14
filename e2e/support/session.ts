// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { Page } from '@playwright/test';

/** The signed-in answer of the session probe (SPEC §8.8). */
const SIGNED_IN = {
  user: { id: 'e2e-user', email: 'e2e@example.test', roles: [], permissions: [] },
  expiresAt: '2099-01-01T00:30:00+00:00',
  absoluteExpiresAt: '2099-01-01T08:00:00+00:00',
};

/** A session the test switches; `signedIn` follows the mocked sign-in and sign-out. */
export interface FakeSession {
  signedIn: boolean;
}

/**
 * Stands in for the cookie session until the E2E users of S5-14 exist (owner decision S5-08): the
 * probe answers from {@link FakeSession} — through the real API, so its antiforgery cookie is still
 * set —, `POST /auth/login` and `/auth/logout` answer 204 and switch it, and the legacy requests a
 * signed-in client makes (`/api/me`, `/api/Proposals`) answer without user data, so the server never
 * sees a signed-in request it would refuse. Everything else reaches the stack.
 */
export async function fakeSession(page: Page, signedIn = false): Promise<FakeSession> {
  const session: FakeSession = { signedIn };
  await page.route('**/api/v1/auth/session', async (route) => {
    const response = await route.fetch();
    await route.fulfill({ response, status: 200, json: session.signedIn ? SIGNED_IN : { user: null } });
  });
  await page.route('**/api/v1/auth/login', async (route) => {
    session.signedIn = true;
    await route.fulfill({ status: 204 });
  });
  await page.route('**/api/v1/auth/logout', async (route) => {
    session.signedIn = false;
    await route.fulfill({ status: 204 });
  });
  await page.route('**/api/me', (route) => route.fulfill({ json: null }));
  await page.route('**/api/Proposals', (route) => route.fulfill({ json: { value: [] } }));
  return session;
}

/** Answers the next request to `url` with 401, as after an expired session (R-AUTH-07); later ones reach the stack. */
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

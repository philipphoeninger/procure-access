// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type APIRequestContext, type Page } from '@playwright/test';
import { linksOf, waitForMail } from './mailpit';

/**
 * Accounts and organisations built through the API for tests whose subject is not S02 or S20
 * (S5-17b): each test makes its own, so it may change them, unlike the seeded users and the seeded
 * organisation (TESTING §6.3). Every state-changing call sends the probe's antiforgery token.
 */

/** The probe's antiforgery token of the request context (SEC-09). */
async function xsrfToken(request: APIRequestContext): Promise<string> {
  expect((await request.get('/api/v1/auth/session')).ok()).toBe(true);
  const token = (await request.storageState()).cookies.find((cookie) => cookie.name === 'XSRF-TOKEN')?.value;
  expect(token, 'the probe issues XSRF-TOKEN').toBeTruthy();
  return token!;
}

/** Registers `address` and waits for its confirmation mail. */
export async function registerAccount(request: APIRequestContext, address: string, password: string): Promise<void> {
  const response = await request.post('/api/v1/auth/register', {
    headers: { 'X-XSRF-TOKEN': await xsrfToken(request) },
    data: { email: address, password, privacyAcknowledged: true },
  });
  expect(response.status()).toBe(204);
  await waitForMail(request, address);
}

/** Registers `address` and confirms it with the token of the newest confirmation mail. */
export async function registerConfirmedAccount(request: APIRequestContext, address: string, password: string): Promise<void> {
  await registerAccount(request, address, password);
  const link = linksOf((await waitForMail(request, address)).html).find((href) => href.includes('/registrieren/bestaetigen?token='));
  expect(link, 'the confirmation link').toBeTruthy();
  const response = await request.post('/api/v1/auth/confirm-email', {
    headers: { 'X-XSRF-TOKEN': await xsrfToken(request) },
    data: { token: new URL(link!).searchParams.get('token') },
  });
  expect(response.status(), `confirmation of ${address}`).toBe(204);
}

/** Creates an organisation for the user `page` is signed in as (its Owner) and returns its id. */
export async function createOrganizationThroughApi(page: Page, name: string): Promise<string> {
  const response = await page.request.post('/api/v1/organizations', {
    headers: { 'X-XSRF-TOKEN': await xsrfToken(page.request) },
    data: { name, type: 'publicBody' },
  });
  expect(response.status(), `creating ${name}`).toBe(201);
  return ((await response.json()) as { id: string }).id;
}

/** Invites `email` into the organisation as the Owner `page` is signed in as. */
export async function inviteThroughApi(page: Page, organizationId: string, email: string, role: string): Promise<void> {
  const response = await page.request.post(`/api/v1/organizations/${organizationId}/invitations`, {
    headers: { 'X-XSRF-TOKEN': await xsrfToken(page.request) },
    data: { email, role },
  });
  expect(response.status(), `inviting ${email}`).toBe(201);
}

/** The path and query of the invitation link in the newest mail to `address`. */
export async function invitationLinkOf(request: APIRequestContext, address: string, count: number): Promise<string> {
  const link = linksOf((await waitForMail(request, address, count)).html).find((href) => href.includes('/organisation/einladung?token='));
  expect(link, 'the invitation link').toBeTruthy();
  const url = new URL(link!);
  return url.pathname + url.search;
}

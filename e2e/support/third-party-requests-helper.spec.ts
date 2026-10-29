// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { recordThirdPartyRequests, thirdPartyLinksInDocument } from './third-party-requests';

/**
 * Self-test of the request recorder and the link check on a fixed page (no stack needed): both origins are answered
 * by `page.route`, so nothing leaves the machine. Without it, a recorder that never fires would
 * let every "no third-party request" check pass.
 */
const appOrigin = 'http://app.test';
const pixel = Buffer.from('R0lGODlhAQABAAAAACw=', 'base64');

const fixturePage = `
  <!doctype html>
  <html lang="de">
    <head>
      <title>Request recorder fixture</title>
      <link rel="preconnect" href="https://fonts.example" crossorigin>
      <link rel="icon" href="/favicon.ico">
    </head>
    <body>
      <img src="/own.gif" alt="">
      <img src="data:image/gif;base64,R0lGODlhAQABAAAAACw=" alt="">
      <img src="https://cdn.example/icon.gif" alt="">
    </body>
  </html>`;

test.describe('third-party request checks', () => {
  test('records only requests to another origin', async ({ page }) => {
    await page.route(`${appOrigin}/`, (route) => route.fulfill({ contentType: 'text/html', body: fixturePage }));
    await page.route(`${appOrigin}/own.gif`, (route) => route.fulfill({ contentType: 'image/gif', body: pixel }));
    await page.route('https://cdn.example/**', (route) => route.fulfill({ contentType: 'image/gif', body: pixel }));

    const thirdParty = recordThirdPartyRequests(page, appOrigin);
    await page.goto(`${appOrigin}/`, { waitUntil: 'load' });

    expect(thirdParty).toEqual(['https://cdn.example/icon.gif']);
  });

  test('finds links to another origin that send no request, e.g. preconnect', async ({ page }) => {
    await page.route(`${appOrigin}/**`, (route) => route.fulfill({ contentType: 'text/html', body: fixturePage }));
    await page.route('https://cdn.example/**', (route) => route.fulfill({ contentType: 'image/gif', body: pixel }));
    await page.goto(`${appOrigin}/`, { waitUntil: 'load' });

    expect(await thirdPartyLinksInDocument(page, appOrigin)).toEqual(['https://fonts.example/']);
  });
});

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test, type Page, type Request } from '@playwright/test';
import { expectNoAxeViolations } from '../support/axe';

/**
 * A lazy route chunk that cannot be loaded (#296, owner decision O-S6-5 (a)): a tab opened before a
 * deployment asks for a chunk file the new image no longer has. The router's navigation error
 * handler loads the target once in full; if the chunk fails again after that reload, S80 „Etwas ist
 * schiefgelaufen“ shows in place instead of a second reload. Simulated with `page.route`: every
 * chunk file the start page did not load (i.e. a lazy page's) answers 404.
 */
const WIDE = { width: 1280, height: 800 };
const CHUNK = /\/chunk-[^/]+\.js$/;

const mainNav = (page: Page) => page.getByRole('navigation', { name: 'Hauptnavigation' });

/**
 * Opens the start page and answers 404 for every chunk it did not load, while `failing()` holds.
 * Returns the paths of the documents the tab loads after that, i.e. its full reloads.
 */
async function startWithLostChunks(page: Page, failing: () => boolean): Promise<string[]> {
  const initial = new Set<string>();
  const collect = (request: Request) => {
    if (CHUNK.test(request.url())) initial.add(request.url());
  };
  page.on('request', collect);
  await page.setViewportSize(WIDE);
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1, name: 'Barrierefreie Software beschaffen – Schritt für Schritt' })).toBeVisible();
  // The start page renders only after every chunk of the first download has run.
  page.off('request', collect);

  const documents: string[] = [];
  page.on('request', (request) => {
    if (request.isNavigationRequest() && request.frame() === page.mainFrame()) {
      documents.push(new URL(request.url()).pathname);
    }
  });
  await page.route(CHUNK, (route) =>
    failing() && !initial.has(route.request().url()) ? route.fulfill({ status: 404, body: '' }) : route.continue(),
  );
  return documents;
}

test.describe('Lazy page after a deployment (#296)', () => {
  test('a lazy chunk that is gone reloads the target once and shows the page', async ({ page }) => {
    let reloaded = false;
    const documents = await startWithLostChunks(page, () => !reloaded);
    page.on('request', (request) => {
      // The new document gets the new chunks: from its request on, nothing fails any more.
      if (request.isNavigationRequest() && request.frame() === page.mainFrame()) reloaded = true;
    });

    await mainNav(page).getByRole('link', { name: 'Kriterien' }).click();

    await expect(page.getByRole('heading', { level: 1, name: 'Kriterien zusammenstellen' })).toBeVisible();
    await expect(page).toHaveURL(/\/kriterien$/);
    expect(documents).toEqual(['/kriterien']);
  });

  test('INV:A11Y-01 INV:A11Y-08 a chunk that fails again after the reload shows „Etwas ist schiefgelaufen“, never a second reload', async ({ page }, testInfo) => {
    const documents = await startWithLostChunks(page, () => true);

    await mainNav(page).getByRole('link', { name: 'Kriterien' }).click();

    await expect(page.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeVisible();
    await expect(page).toHaveTitle('Etwas ist schiefgelaufen – ProcureAccess');
    // In place: the address names the page the user asked for.
    await expect(page).toHaveURL(/\/kriterien$/);
    expect(documents).toEqual(['/kriterien']);
    await expectNoAxeViolations(page, testInfo, 'error page after a lost chunk');
  });
});

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Dev server smoke (S6-07, TD-73, #388; regression of S5-02, #264). CI stage 1, after `ng serve` has
// started in the client folder:
//   node scripts/smoke-dev-server.mjs [http://localhost:4200] [/ /filters]
// Opens each path in a headless Chrome and fails on a 5xx answer of the dev server, an uncaught page
// error or a page without a visible <h1> (what the app does when its first script does not load).
// #264 was exactly that: `ng serve` built, but the browser got a 500 for a pre-bundled dependency and
// nothing rendered. The default paths are `/` (the eager bundle) and `/filters` (a lazy route whose
// chunk imports ngx-markdown through the dev server's pre-bundling); when the legacy filter goes
// (activation of 2026.1) `/filters` is replaced by another lazy route that imports a package.
// No API runs in stage 1: the dev server's proxy answers 5xx for `/api` and `/health`, which is not
// a failure of the dev server, so those paths are not counted.
// Chrome: the runner's Google Chrome (Playwright `channel: 'chrome'`); `CHROME_PATH` names another one.
// Playwright comes from e2e/node_modules (`npm ci` in e2e/), this script has no dependencies of its own.
import { createRequire } from 'node:module';
import { dirname, join } from 'node:path';
import { fileURLToPath, URL } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const PROXIED = /^\/(api|health)(\/|$|\?)/;

/** Whether a response of the dev server counts as a failure: 5xx, except answers of the API proxy. */
export function isServerFailure(status, pathname) {
  return status >= 500 && !PROXIED.test(pathname);
}

async function smoke(base, paths) {
  const require = createRequire(join(ROOT, 'e2e', 'package.json'));
  const { chromium } = require('@playwright/test');
  const executablePath = process.env.CHROME_PATH;
  const browser = await chromium.launch(executablePath ? { executablePath } : { channel: 'chrome' });
  const failures = [];
  try {
    for (const path of paths) {
      const page = await browser.newPage();
      page.on('pageerror', (error) => failures.push(`${path}: page error: ${error.message}`));
      page.on('response', (response) => {
        const { pathname } = new URL(response.url());
        if (isServerFailure(response.status(), pathname)) {
          failures.push(`${path}: HTTP ${response.status()} for ${pathname}`);
        }
      });
      await page.goto(new URL(path, base).href, { waitUntil: 'load', timeout: 120_000 });
      try {
        await page.locator('h1').first().waitFor({ state: 'visible', timeout: 60_000 });
      } catch {
        failures.push(`${path}: no visible <h1> after 60 s, the app did not render`);
      }
      await page.close();
    }
  } finally {
    await browser.close();
  }
  return failures;
}

if (process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1]) {
  const base = process.argv[2] ?? 'http://localhost:4200';
  const paths = process.argv.length > 3 ? process.argv.slice(3) : ['/', '/filters'];
  const failures = await smoke(base, paths);
  if (failures.length > 0) {
    for (const failure of failures) console.error(`smoke-dev-server: ${failure}`);
    process.exit(1);
  }
  console.log(`smoke-dev-server: ${paths.join(', ')} of ${base} rendered without a 5xx or a page error`);
}

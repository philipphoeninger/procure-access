// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { Page } from '@playwright/test';

/**
 * Records every request the page sends to an origin other than the app's own, from now until the
 * page closes (CLAUDE §8: no third-party requests from the browser; issue #43). The release build
 * calls the API on its own origin (nginx forwards `/api/`), so only the app origin is allowed.
 * `data:` and `blob:` URLs never leave the browser and are ignored.
 *
 * Returns the list, which fills while the page runs; assert on it at the end of the test.
 */
export function recordThirdPartyRequests(page: Page, appOrigin: string): readonly string[] {
  const thirdParty: string[] = [];
  page.on('request', (request) => {
    const url = new URL(request.url());
    if (url.protocol === 'data:' || url.protocol === 'blob:') {
      return;
    }
    if (url.origin !== appOrigin) {
      thirdParty.push(request.url());
    }
  });
  return thirdParty;
}

/**
 * Returns the `href`/`src` of every `<link>` and `<script>` in the current document that points
 * at another origin. The request log misses some of them: `preconnect` and `dns-prefetch` open a
 * connection without a request, and a font face is only fetched once a glyph needs it. Angular's
 * release build, for example, turns a Google Fonts stylesheet link into inline CSS plus a
 * `preconnect` to fonts.gstatic.com (S2-03).
 */
export function thirdPartyLinksInDocument(page: Page, appOrigin: string): Promise<string[]> {
  return page.evaluate((origin) => {
    const urls = [...document.querySelectorAll<HTMLLinkElement | HTMLScriptElement>('link[href], script[src]')].map(
      (element) => (element instanceof HTMLLinkElement ? element.href : element.src),
    );
    return urls.filter((url) => url.startsWith('http') && new URL(url).origin !== origin);
  }, appOrigin);
}

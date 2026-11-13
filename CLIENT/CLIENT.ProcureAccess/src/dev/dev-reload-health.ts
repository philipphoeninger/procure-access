// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { environment } from '../environments/environment';

/**
 * Liveness URL of the API that the build calls. The health endpoints sit at the API root, not
 * under `/api`: `apiUrl` is `/api` on the page's origin since S5-08 (#317), and the dev server
 * proxies `/health` next to `/api` to the local API (proxy.conf.json), so the watcher polls
 * `<origin>/health/live`.
 */
function apiHealthUrl(apiUrl: string): string {
  return new URL('/health/live', new URL(apiUrl, window.location.origin)).href;
}

// call this from main.ts only in development
function startApiHealthWatcher() {
  const checkUrl = apiHealthUrl(environment.apiUrl);
  setInterval(async () => {
    try {
      const res = await fetch(checkUrl, { method: 'GET', cache: 'no-cache' });
      if (!res.ok) {
        console.debug('API not healthy (status ' + res.status + ')');
      }
    } catch {
      // if offline, reload to let dev server re-connect / show error
      console.debug('API unreachable ...'); //— reloading page to reconnect
      // window.location.reload();
    }
  }, 5000); // every 5s
}

export { apiHealthUrl, startApiHealthWatcher };

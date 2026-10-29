// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { environment } from '../environments/environment';

/**
 * Liveness URL of the API that the build calls. The health endpoints sit at the API root,
 * not under `/api`, so `http://localhost:5000/api` becomes `http://localhost:5000/health/live`.
 */
function apiHealthUrl(apiUrl: string): string {
  return new URL('/health/live', apiUrl).href;
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

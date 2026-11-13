// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { environment } from '../environments/environment';
import { apiHealthUrl } from './dev-reload-health';

// Regression test for #113: the dev-only health watcher polled the production API. It must
// poll the API of the dev configuration (environment.ts, used by `npm start`): since S5-08 the
// local API behind the dev server's proxy, on the page's own origin (#317).
describe('dev health watcher', () => {
  it('polls the liveness endpoint of the local API in the dev configuration', () => {
    expect(environment.production).toBe(false);
    expect(environment.apiUrl).toBe('/api');
    expect(apiHealthUrl(environment.apiUrl)).toBe(`${window.location.origin}/health/live`);
  });
});

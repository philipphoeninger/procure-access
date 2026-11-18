// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of smoke-dev-server.mjs: which answers count as a failure of the dev server. The browser
// part runs in CI stage 1 against a real `ng serve`.
// Node's built-in test runner, no dependencies: node --test scripts/smoke-dev-server.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';

import { isServerFailure } from './smoke-dev-server.mjs';

test('a 5xx of the dev server is a failure, as for the pre-bundled dependency of #264', () => {
  assert.equal(isServerFailure(500, '/.angular/cache/22.0.0/procure-access/vite/deps/ngx-markdown.js'), true);
  assert.equal(isServerFailure(503, '/main.js'), true);
});

test('a 5xx of the API proxy is not, as no API runs in CI stage 1', () => {
  assert.equal(isServerFailure(504, '/api/v1/auth/session'), false);
  assert.equal(isServerFailure(500, '/health/ready'), false);
});

test('other answers are not failures; a path that only starts like the proxy is still checked', () => {
  assert.equal(isServerFailure(200, '/main.js'), false);
  assert.equal(isServerFailure(404, '/nothing'), false);
  assert.equal(isServerFailure(500, '/apidocs.js'), true);
});

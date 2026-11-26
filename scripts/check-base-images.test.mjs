// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import assert from 'node:assert/strict';
import { test } from 'node:test';
import { check, imagesOf, parseStatement, run } from './check-base-images.mjs';

const STATEMENT = `
## Shipped images

| Image | Used for | Info |
|---|---|---|
| \`nginx:1.30-alpine\` | client | https://hub.docker.com/_/nginx |

## Not shipped

| Image | Used for | Note |
|---|---|---|
| \`node:24-alpine\` | build | none |
| \`axllent/mailpit:v1\` | dev | MIT |
`;

const refs = (...list) => list.map((ref) => ({ file: 'f', line: 1, ref }));
const MATCHING = ['nginx:1.30-alpine', 'node:24-alpine', 'axllent/mailpit:v1'];

test('imagesOf reads FROM lines and image keys and drops digests', () => {
  assert.deepEqual(
    imagesOf('a/Dockerfile', 'FROM --platform=x node:24-alpine AS build\nFROM scratch\nFROM nginx:1.30-alpine AS final').map((r) => r.ref),
    ['node:24-alpine', 'nginx:1.30-alpine'],
  );
  assert.deepEqual(imagesOf('docker-compose.yml', '  image: axllent/mailpit:v1@sha256:abc\n  # image: x:1').map((r) => r.ref), ['axllent/mailpit:v1']);
});

test('parseStatement reads both tables', () => {
  const s = parseStatement(STATEMENT);
  assert.deepEqual(s.shipped.map((e) => e.ref), ['nginx:1.30-alpine']);
  assert.deepEqual(s.other.map((e) => e.ref), ['node:24-alpine', 'axllent/mailpit:v1']);
});

test('check accepts matching images', () => {
  assert.deepEqual(check(refs(...MATCHING), parseStatement(STATEMENT)), []);
});

test('check fails on a changed tag and names the statement tag', () => {
  const problems = check(refs('nginx:1.31-alpine', 'node:24-alpine', 'axllent/mailpit:v1'), parseStatement(STATEMENT));
  assert.equal(problems.length, 2);
  assert.match(problems[0], /nginx:1\.31-alpine is not named.*a tag changed/);
  assert.match(problems[1], /nginx:1\.30-alpine is no longer used/);
});

test('check fails on a new image, an unused entry and a shipped image without a link', () => {
  assert.match(check(refs(...MATCHING, 'redis:7'), parseStatement(STATEMENT)).join(), /redis:7 is not named/);
  assert.match(check(refs('nginx:1.30-alpine', 'node:24-alpine'), parseStatement(STATEMENT)).join(), /mailpit.*no longer used/);
  const noLink = parseStatement(STATEMENT.replace('https://hub.docker.com/_/nginx', 'see Docker Hub'));
  assert.match(check(refs(...MATCHING), noLink).join(), /names no place for its licence information/);
});

test('check fails on an image without a fixed tag', () => {
  assert.match(check(refs('nginx', 'x:${TAG}'), { shipped: [], other: [] }).join('\n'), /nginx has no fixed tag[\s\S]*TAG.*no fixed tag/);
});

test('the repository statement matches the Dockerfiles and compose files', () => {
  assert.deepEqual(run().problems, []);
});

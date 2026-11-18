// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-chunk-cache.mjs: parsing, the header rule and the whole check against a local server.
// Node's built-in test runner, no dependencies: node --test scripts/check-chunk-cache.test.mjs
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { test } from 'node:test';

import { checkClient, checkImmutable, findEntryFiles, findLazyChunk, IMMUTABLE } from './check-chunk-cache.mjs';

const INDEX =
  '<link rel="stylesheet" href="styles-GVKI6HQB.css" media="print"><link rel="stylesheet" href="styles-GVKI6HQB.css">' +
  '<script src="main-DRINI6T5.js" type="module"></script>';

test('finds the main bundle and the stylesheet in index.html', () => {
  assert.deepEqual(findEntryFiles(INDEX), { main: 'main-DRINI6T5.js', styles: 'styles-GVKI6HQB.css' });
  assert.deepEqual(findEntryFiles('<html></html>'), { main: null, styles: null });
});

test('finds a lazy chunk by its dynamic import, not a static one', () => {
  assert.equal(findLazyChunk('x=()=>import(`./chunk-BkVFE9XS.js`)'), 'chunk-BkVFE9XS.js');
  assert.equal(findLazyChunk('y=()=>import("./chunk-CpH5Yic-.js")'), 'chunk-CpH5Yic-.js');
  assert.equal(findLazyChunk('import{a}from"./chunk-5Z3zbkON.js";'), null);
});

test('accepts only 200 with the immutable header', () => {
  assert.equal(checkImmutable('a.js', 200, IMMUTABLE), null);
  assert.match(checkImmutable('a.js', 200, 'no-cache'), /Cache-Control is "no-cache"/);
  assert.match(checkImmutable('a.js', 200, undefined), /Cache-Control is ""/);
  assert.match(checkImmutable('a.js', 404, IMMUTABLE), /HTTP 404/);
});

/** A client that answers like nginx; `cacheFor` decides the header of each file. */
async function withClient(cacheFor, run) {
  const files = {
    '/': INDEX,
    '/main-DRINI6T5.js': 'go=()=>import(`./chunk-BkVFE9XS.js`)',
    '/chunk-BkVFE9XS.js': '//',
    '/styles-GVKI6HQB.css': '',
  };
  const server = createServer((request, response) => {
    const body = files[request.url];
    if (body === undefined) return response.writeHead(404).end();
    return response.writeHead(200, { 'Cache-Control': cacheFor(request.url) }).end(body);
  });
  await new Promise((resolve) => server.listen(0, '127.0.0.1', resolve));
  try {
    return await run(`http://127.0.0.1:${server.address().port}`);
  } finally {
    server.close();
  }
}

test('passes a client whose hashed files are immutable', async () => {
  const errors = await withClient((url) => (url === '/' ? 'no-cache' : IMMUTABLE), checkClient);
  assert.deepEqual(errors, []);
});

test('fails for each of main, styles and the lazy chunk without the header', async () => {
  for (const [file, name] of [
    ['/main-DRINI6T5.js', 'main-DRINI6T5.js'],
    ['/styles-GVKI6HQB.css', 'styles-GVKI6HQB.css'],
    ['/chunk-BkVFE9XS.js', 'chunk-BkVFE9XS.js (lazy)'],
  ]) {
    const errors = await withClient((url) => (url === file ? 'no-cache' : IMMUTABLE), checkClient);
    assert.equal(errors.length, 1, file);
    assert.ok(errors[0].startsWith(name), errors[0]);
  }
});

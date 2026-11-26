// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-client-proxy.mjs: the three rules and the whole check against local servers.
// Node's built-in test runner, no dependencies: node --test scripts/check-client-proxy.test.mjs
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { test } from 'node:test';
import { gzipSync } from 'node:zlib';

import { API_SAMPLE_PATHS, checkEtag, checkHealth, checkNotForwarded, checkProxy, MIN_PROOF_BYTES } from './check-client-proxy.mjs';

const ETAG = '"0a1b2c"';
const JSON_BODY = JSON.stringify({ release: 'T.1', text: 'x'.repeat(MIN_PROOF_BYTES) });

test('accepts a health answer only as 200 with the status word alone', () => {
  assert.equal(checkHealth('/health/live', 200, 'Healthy'), null);
  assert.match(checkHealth('/health/ready', 503, 'Unhealthy'), /HTTP 503/);
  assert.match(checkHealth('/health/ready', 429, ''), /HTTP 429/);
  assert.match(checkHealth('/health/ready', 200, '{"status":"Healthy","entries":{"database":{}}}'), /not the status word/);
});

test('accepts another path under /health/ only when the client answers it', () => {
  assert.equal(checkNotForwarded('/health/details', 'text/html'), null);
  assert.match(checkNotForwarded('/health/details', 'text/plain'), /forwarded to the API/);
  assert.match(checkNotForwarded('/health/details', undefined), /forwarded to the API/);
});

test('accepts only an uncompressed answer with the strong ETag of the API', () => {
  assert.deepEqual(checkEtag('/api/x', { etag: ETAG }, ETAG), []);
  assert.deepEqual(checkEtag('/api/x', { etag: ETAG }, undefined), []);
  assert.match(checkEtag('/api/x', { etag: `W/${ETAG}`, contentEncoding: 'gzip' }, ETAG).join('\n'), /Content-Encoding "gzip"[\s\S]*weak ETag/);
  assert.match(checkEtag('/api/x', { etag: '"other"' }, ETAG)[0], /differs from the API's/);
  assert.match(checkEtag('/api/x', {}, undefined)[0], /no ETag/);
});

/** A local API, and a client that answers like the image's nginx; `client` overrides parts of it. */
async function withServers(client, run) {
  const api = createServer((request, response) => {
    if (request.url === API_SAMPLE_PATHS[0]) return response.writeHead(200, { 'Content-Type': 'application/json', ETag: ETAG }).end('{}');
    if (request.url === API_SAMPLE_PATHS[1]) return response.writeHead(200, { 'Content-Type': 'application/json', ETag: ETAG }).end(JSON_BODY);
    return response.writeHead(404).end();
  });
  const proxy = createServer((request, response) => {
    if (request.url === '/health/live' || request.url === '/health/ready') {
      return response.writeHead(200, { 'Content-Type': 'text/plain' }).end(client.health ?? 'Healthy');
    }
    if (request.url.startsWith('/health/')) {
      return response.writeHead(client.otherHealthStatus ?? 200, { 'Content-Type': client.otherHealthType ?? 'text/html' }).end('<html></html>');
    }
    if (request.url === API_SAMPLE_PATHS[0]) return response.writeHead(200, { 'Content-Type': 'application/json', ETag: ETAG }).end('{}');
    if (request.url === API_SAMPLE_PATHS[1] && client.compress && /gzip/.test(request.headers['accept-encoding'] ?? '')) {
      // What nginx does when it compresses a proxied answer: the strong tag becomes weak.
      return response.writeHead(200, { 'Content-Type': 'application/json', 'Content-Encoding': 'gzip', ETag: `W/${ETAG}` }).end(gzipSync(JSON_BODY));
    }
    if (request.url === API_SAMPLE_PATHS[1]) return response.writeHead(200, { 'Content-Type': 'application/json', ETag: ETAG }).end(JSON_BODY);
    return response.writeHead(404).end();
  });
  const listen = (server) => new Promise((resolve) => server.listen(0, '127.0.0.1', () => resolve(`http://127.0.0.1:${server.address().port}`)));
  const [apiUrl, clientUrl] = [await listen(api), await listen(proxy)];
  try {
    await run(clientUrl, apiUrl);
  } finally {
    api.close();
    proxy.close();
  }
}

test('passes for a client that forwards the two health paths and leaves /api/ answers alone', async () => {
  await withServers({}, async (client, api) => {
    assert.deepEqual(await checkProxy(client, api), []);
    assert.deepEqual(await checkProxy(client, undefined), []);
  });
});

test('fails when the proxy compresses an /api/ answer, with and without the API to compare', async () => {
  await withServers({ compress: true }, async (client, api) => {
    for (const errors of [await checkProxy(client, api), await checkProxy(client, undefined)]) {
      assert.equal(errors.length, 2);
      assert.match(errors[0], /Content-Encoding "gzip"/);
      assert.match(errors[1], /weak ETag/);
    }
  });
});

test('fails when the health answer carries details or another health path reaches the API', async () => {
  await withServers({ health: '{"status":"Healthy","entries":{}}', otherHealthStatus: 404, otherHealthType: 'text/plain' }, async (client, api) => {
    const errors = await checkProxy(client, api);
    assert.equal(errors.length, 3);
    assert.match(errors[0], /\/health\/live: the body is not the status word/);
    assert.match(errors[1], /\/health\/ready: the body is not the status word/);
    assert.match(errors[2], /\/health\/details: forwarded to the API/);
  });
});

test('fails when no /api/ answer is large enough to prove anything', async () => {
  const empty = createServer((request, response) => {
    if (request.url === '/health/live' || request.url === '/health/ready') return response.writeHead(200).end('Healthy');
    if (request.url.startsWith('/health/')) return response.writeHead(200, { 'Content-Type': 'text/html' }).end('');
    return response.writeHead(200, { 'Content-Type': 'application/json', ETag: ETAG }).end('{}');
  });
  await new Promise((resolve) => empty.listen(0, '127.0.0.1', resolve));
  try {
    const errors = await checkProxy(`http://127.0.0.1:${empty.address().port}`, undefined);
    assert.equal(errors.length, 1);
    assert.match(errors[0], /no \/api\/ answer of at least 1024 bytes/);
  } finally {
    empty.close();
  }
});

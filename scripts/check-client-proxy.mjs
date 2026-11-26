// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Proxy smoke of the client image (S7-09; #448, TECH_DEBT TD-28 point 15, S6-20a R6). Node, no
// dependencies, CI stage 6 (once, on the E2E stack's client):
//   node scripts/check-client-proxy.mjs [http://localhost:4200] [http://localhost:5000]
// The first address is the client (its nginx, or a deployment's public address behind the reverse
// proxy), the second the API asked directly; pass only the first where the API is not reachable.
//
// 1. Health route: /health/live and /health/ready answer 200 through the client with nothing but
//    the status word, and no other path under /health/ reaches the API.
// 2. ETags: an /api/ answer asked for with `Accept-Encoding: gzip` comes back uncompressed and with
//    a strong ETag — the same one the API sent, when the API address is given. A proxy that
//    compresses the answer turns the strong tag into a weak one (`W/"…"`), the API accepts only the
//    strong tag in If-Match, and every change would answer 409 (API-03, ARCHITECTURE §7.1).
import { Buffer } from 'node:buffer';
import { realpathSync } from 'node:fs';
import { request as httpRequest } from 'node:http';
import { request as httpsRequest } from 'node:https';
import { fileURLToPath } from 'node:url';

export const HEALTH_PATHS = ['/health/live', '/health/ready'];

/** A path under /health/ that the API does not have: the client must answer it itself. */
export const OTHER_HEALTH_PATH = '/health/details';

/** The words of ASP.NET Core's `HealthStatus`; a healthy stack answers the first. */
const HEALTHY = 'Healthy';

/**
 * Anonymous catalog reads with an ETag, tried in this order: the first that answers 200 with a body
 * of at least `MIN_PROOF_BYTES` is the sample. The second is a complete selection of the test
 * release T.1 (as e2e/smoke/catalog.spec.ts uses it).
 */
export const API_SAMPLE_PATHS = [
  '/api/v1/catalog/filter-options',
  '/api/v1/catalog/criteria?useCase=hr&appTypeIds%5B%5D=web&productPartIds%5B%5D=base&testTypeIds%5B%5D=softwareTest',
];

/** nginx compresses from `gzip_min_length 1024` on: a smaller answer proves nothing. */
export const MIN_PROOF_BYTES = 1024;

/** An error message for a health answer that is not `200 Healthy` and nothing else, or null. */
export function checkHealth(path, status, body) {
  if (status !== 200) return `${path}: HTTP ${status}, expected 200`;
  if (body.trim() !== HEALTHY) return `${path}: the body is not the status word "${HEALTHY}" alone (${body.length} characters)`;
  return null;
}

/** An error message when a path under /health/ other than the two endpoints reached the API, or null. */
export function checkNotForwarded(path, contentType) {
  // The client answers an unknown path with index.html (Angular routing); the API would answer without HTML.
  return (contentType ?? '').startsWith('text/html') ? null : `${path}: forwarded to the API (Content-Type "${contentType ?? ''}"), only ${HEALTH_PATHS.join(' and ')} may be`;
}

/**
 * Error messages for an /api/ answer through the client that was compressed or whose ETag is not
 * the strong tag of the API (`apiEtag` undefined = the API was not asked).
 */
export function checkEtag(path, { contentEncoding, etag }, apiEtag) {
  const errors = [];
  if (contentEncoding) errors.push(`${path}: Content-Encoding "${contentEncoding}", /api/ answers must stay uncompressed`);
  if (!etag) errors.push(`${path}: no ETag`);
  else if (etag.startsWith('W/')) errors.push(`${path}: weak ETag ${etag}, If-Match needs the strong tag`);
  else if (apiEtag !== undefined && etag !== apiEtag) errors.push(`${path}: ETag ${etag} differs from the API's ${apiEtag ?? '(none)'}`);
  return errors;
}

function request(base, path) {
  const url = new URL(path, base);
  const send = url.protocol === 'https:' ? httpsRequest : httpRequest;
  return new Promise((resolve, reject) => {
    // Node's request sends no Accept-Encoding on its own and never decompresses: the headers are those of the wire.
    send(url, { headers: { 'Accept-Encoding': 'gzip, br' } }, (response) => {
      const chunks = [];
      response.on('data', (chunk) => chunks.push(chunk));
      response.on('end', () => {
        const body = Buffer.concat(chunks);
        resolve({
          status: response.statusCode,
          contentType: response.headers['content-type'],
          contentEncoding: response.headers['content-encoding'],
          etag: response.headers.etag,
          bytes: body.length,
          body: body.toString('utf8'),
        });
      });
    })
      .on('error', reject)
      .end();
  });
}

/** Runs the check; returns the error messages (empty = passed). `api` may be undefined. */
export async function checkProxy(client, api) {
  const errors = [];
  for (const path of HEALTH_PATHS) {
    const response = await request(client, path);
    errors.push(checkHealth(path, response.status, response.body));
  }
  const other = await request(client, OTHER_HEALTH_PATH);
  errors.push(checkNotForwarded(OTHER_HEALTH_PATH, other.contentType));

  // The sample is chosen by the uncompressed answer: the API's when it is reachable.
  let sample = null;
  for (const path of API_SAMPLE_PATHS) {
    const direct = api ? await request(api, path) : null;
    const proxied = await request(client, path);
    const size = direct ?? proxied;
    if (proxied.status === 200 && size.status === 200 && (size.bytes >= MIN_PROOF_BYTES || proxied.contentEncoding)) {
      sample = { path, proxied, apiEtag: direct ? (direct.etag ?? null) : undefined };
      break;
    }
  }
  if (!sample) {
    errors.push(`no /api/ answer of at least ${MIN_PROOF_BYTES} bytes to check (tried ${API_SAMPLE_PATHS.join(', ')}): is a catalog release active?`);
  } else {
    errors.push(...checkEtag(sample.path, sample.proxied, sample.apiEtag));
  }
  return errors.filter(Boolean);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const client = process.argv[2] ?? 'http://localhost:4200';
  const api = process.argv[3];
  const errors = await checkProxy(client, api);
  if (errors.length > 0) {
    for (const error of errors) console.error(`check-client-proxy: ${error}`);
    console.error('See CLIENT/CLIENT.ProcureAccess/nginx.conf (/api/, /health/) and docs/TECHNICAL.md §4.3 for a proxy in front.');
    process.exit(1);
  }
  console.log(`check-client-proxy: health route and /api/ ETags of ${client} are as required`);
}

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Cache header smoke of the client image (S6-07, TD-73, #388; regression of S5-01, #308). Node, no
// dependencies, CI stage 6 (once, on the E2E stack's client):
//   node scripts/check-chunk-cache.mjs [http://localhost:4200]
// The nginx of the client image gives every file with a content hash in its name
// `Cache-Control: public, max-age=31536000, immutable`. The rule matches the hash alphabet of the
// build, so a new alphabet or length of the Angular/esbuild hashes would silently drop a file out of
// it. The check reads index.html, takes the main bundle and the stylesheet from it and one lazy chunk
// (a dynamic `import("./chunk-<hash>.js")`) from the main bundle, and requires the header on all three.
import { Buffer } from 'node:buffer';
import { realpathSync } from 'node:fs';
import { get } from 'node:http';
import { fileURLToPath } from 'node:url';

export const IMMUTABLE = 'public, max-age=31536000, immutable';

const MAIN = /<script[^>]*\bsrc="(main-[A-Za-z0-9_-]+\.js)"/;
const STYLES = /<link[^>]*\bhref="(styles-[A-Za-z0-9_-]+\.css)"/;
const LAZY_CHUNK = /import\(\s*[`'"]\.\/(chunk-[A-Za-z0-9_-]+\.js)[`'"]\s*\)/;

/** The names of the main bundle and the stylesheet in index.html, or null for each one that is missing. */
export function findEntryFiles(indexHtml) {
  return { main: MAIN.exec(indexHtml)?.[1] ?? null, styles: STYLES.exec(indexHtml)?.[1] ?? null };
}

/** The name of a lazily loaded chunk in the main bundle, or null. */
export function findLazyChunk(mainJs) {
  return LAZY_CHUNK.exec(mainJs)?.[1] ?? null;
}

/** An error message for a response that is not immutable, or null. */
export function checkImmutable(name, status, cacheControl) {
  if (status !== 200) return `${name}: HTTP ${status}, expected 200`;
  if (cacheControl !== IMMUTABLE) return `${name}: Cache-Control is "${cacheControl ?? ''}", expected "${IMMUTABLE}"`;
  return null;
}

function request(base, path) {
  return new Promise((resolve, reject) => {
    get(new URL(path, base), (response) => {
      const chunks = [];
      response.on('data', (chunk) => chunks.push(chunk));
      response.on('end', () =>
        resolve({
          status: response.statusCode,
          cacheControl: response.headers['cache-control'],
          body: Buffer.concat(chunks).toString('utf8'),
        }),
      );
    }).on('error', reject);
  });
}

/** Runs the check against a client; returns the error messages (empty = passed). */
export async function checkClient(base) {
  const index = await request(base, '/');
  const { main, styles } = findEntryFiles(index.body);
  if (!main || !styles) return [`index.html names no ${main ? 'stylesheet' : 'main bundle'} with a content hash`];

  const mainResponse = await request(base, `/${main}`);
  const chunk = findLazyChunk(mainResponse.body);
  const errors = [checkImmutable(main, mainResponse.status, mainResponse.cacheControl)];
  if (!chunk) {
    errors.push(`${main} imports no lazy chunk with a content hash`);
  } else {
    const response = await request(base, `/${chunk}`);
    errors.push(checkImmutable(`${chunk} (lazy)`, response.status, response.cacheControl));
  }
  const stylesResponse = await request(base, `/${styles}`);
  errors.push(checkImmutable(styles, stylesResponse.status, stylesResponse.cacheControl));
  return errors.filter(Boolean);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const base = process.argv[2] ?? 'http://localhost:4200';
  const errors = await checkClient(base);
  if (errors.length > 0) {
    for (const error of errors) console.error(`check-chunk-cache: ${error}`);
    console.error('The nginx rule for hashed files (CLIENT/CLIENT.ProcureAccess/nginx.conf) no longer matches the build.');
    process.exit(1);
  }
  console.log(`check-chunk-cache: main, styles and a lazy chunk of ${base} are immutable`);
}

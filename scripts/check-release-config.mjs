// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Release check (SPEC R-SUG-01; ARCHITECTURE C85, C110). Node, no dependencies:
//   node scripts/check-release-config.mjs
// Run it before a production release, not on every pull request: while a 🔧 placeholder is
// configured it fails, and placeholders stay until their owner decision (owner, S3-21 1a).
// Checks the client's release environment (environment.prod.ts):
//   1. `criteriaSuggestion` is set once, as `mailto:<address>` or an https URL (the forms the
//      client accepts, core/contact/criteria-suggestion.ts);
//   2. it is not the placeholder: no address or host on the reserved domain `.invalid` (O-6).
// GP.3 adds the legal pages (R-SHL-07) here.
import { readFileSync, realpathSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const ENVIRONMENT_PROD = 'CLIENT/CLIENT.ProcureAccess/src/environments/environment.prod.ts';

const SETTING = /^\s*criteriaSuggestion\s*:\s*(['"])(.*?)\1\s*,?\s*$/gm;
const MAILTO = /^mailto:([^\s?#@/]+@[^\s?#@/]+)$/;

/** The domain of the configured channel, or null if the value has neither accepted form. */
export function channelDomain(value) {
  const address = MAILTO.exec(value)?.[1];
  if (address) return address.slice(address.lastIndexOf('@') + 1);
  if (URL.canParse(value)) {
    const url = new URL(value);
    if (url.protocol === 'https:') return url.hostname;
  }
  return null;
}

/** Returns every problem of the release environment's source text; empty when it is ready. */
export function checkReleaseConfig(source) {
  const values = [...source.matchAll(SETTING)].map((m) => m[2]);
  if (values.length !== 1) {
    return [`criteriaSuggestion: expected one string value, found ${values.length}`];
  }
  const [value] = values;
  const domain = channelDomain(value);
  if (domain === null) return [`criteriaSuggestion: "${value}" is neither "mailto:<address>" nor an https URL`];
  if (/(^|\.)invalid\.?$/i.test(domain)) {
    return [`criteriaSuggestion: "${value}" is the placeholder (domain .invalid); set the channel decided in O-6`];
  }
  return [];
}

function main() {
  const errors = checkReleaseConfig(readFileSync(join(ROOT, ENVIRONMENT_PROD), 'utf8'));
  for (const error of errors) console.error(`::error file=${ENVIRONMENT_PROD}::${error}`);
  console.log(`Release check (${ENVIRONMENT_PROD}): ${errors.length} error(s).`);
  process.exit(errors.length ? 1 : 0);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();

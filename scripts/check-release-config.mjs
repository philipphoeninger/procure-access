// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Release check (SPEC R-SUG-01, R-SHL-07; ARCHITECTURE C85, C110, C167). Node, no dependencies:
//   node scripts/check-release-config.mjs
// Run it before a production release, not on every pull request: while a 🔧 placeholder is
// configured it fails, and placeholders stay until their owner decision (owner, S3-21 1a) or
// until WERTE.IT delivers the legal texts (O-S6-6).
// Checks the client's release environment (environment.prod.ts):
//   1. `criteriaSuggestion` is set once, as `mailto:<address>` or an https URL (the forms the
//      client accepts, core/contact/criteria-suggestion.ts);
//   2. it is not the placeholder: no address or host on the reserved domain `.invalid` (O-6).
// And the client's legal pages (content/legal/*.de.md, S6-28):
//   3. each file exists, has content outside comments and no longer carries the placeholder
//      marker `<!-- PLATZHALTER -->` (the client shows `legal.placeholder` on the same rule,
//      src/app/pages/legal/legal-markdown.ts; keep marker and comment rule in step).
import { existsSync, readFileSync, realpathSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const ENVIRONMENT_PROD = 'CLIENT/CLIENT.ProcureAccess/src/environments/environment.prod.ts';
export const LEGAL_PAGES = ['impressum', 'datenschutz', 'barrierefreiheit'].map(
  (name) => `CLIENT/CLIENT.ProcureAccess/content/legal/${name}.de.md`,
);

const LEGAL_PLACEHOLDER = /<!--\s*PLATZHALTER\s*-->/i;

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

/** Returns the problems of one legal page's Markdown source (null: the file is missing). */
export function checkLegalPage(source) {
  if (source === null) return ['the legal page is missing'];
  if (LEGAL_PLACEHOLDER.test(source)) {
    return ['the legal page is still the placeholder (<!-- PLATZHALTER -->); insert the text from WERTE.IT (O-S6-6)'];
  }
  if (withoutComments(source).trim() === '') return ['the legal page has no content'];
  return [];
}

/** The source without its `<!-- … -->` comments; a comment without its end hides the rest (as in HTML). */
export function withoutComments(source) {
  let result = '';
  let from = 0;
  for (;;) {
    const start = source.indexOf('<!--', from);
    if (start === -1) return result + source.slice(from);
    result += source.slice(from, start);
    const end = source.indexOf('-->', start + 4);
    if (end === -1) return result;
    from = end + 3;
  }
}

/** Every problem of the repository below `root`, as [file, message] pairs. */
export function checkRelease(root = ROOT) {
  const read = (file) => (existsSync(join(root, file)) ? readFileSync(join(root, file), 'utf8') : null);
  const environment = read(ENVIRONMENT_PROD);
  return [
    [ENVIRONMENT_PROD, environment === null ? ['the release environment is missing'] : checkReleaseConfig(environment)],
    ...LEGAL_PAGES.map((file) => [file, checkLegalPage(read(file))]),
  ].flatMap(([file, errors]) => errors.map((error) => [file, error]));
}

function main() {
  const errors = checkRelease();
  for (const [file, error] of errors) console.error(`::error file=${file}::${error}`);
  console.log(`Release check (${ENVIRONMENT_PROD}, ${LEGAL_PAGES.length} legal pages): ${errors.length} error(s).`);
  process.exit(errors.length ? 1 : 0);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();

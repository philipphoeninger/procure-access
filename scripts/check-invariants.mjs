// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Invariant gate (TESTING §8.3, CI stage 7). Node, no dependencies: node scripts/check-invariants.mjs
//
// 1. Reads the invariant catalogue from the highest docs/TESTING_v<n>.md (risk R4 of sprint 01:
//    the documents are versioned, there is no docs/TESTING.md).
// 2. Searches the tests for INV:<ID> tags (TESTING §8.1).
// 3. Fails if an active invariant has no tagged test; warns on tags with an unknown or
//    non-active ID (typo protection).
// 4. Prints the active invariants with the number of tagged tests.
//
// Manual invariants (level M) are satisfied by a protocol reference in the status column,
// e.g. "active — docs/a11y-reports/2027-02-10_step-5.md"; the referenced file must exist.

import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { dirname, join, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const DOCS = join(ROOT, 'docs');

// Only these tables are catalogue tables (TESTING §4); the journey table and others are ignored.
const CATALOGUE_HEADER = /^\|\s*ID\s*\|\s*Invariant\s*\|\s*Level\s*\|\s*Step\s*\|\s*Status\s*\|\s*$/;
const ID_PATTERN = /^[A-Z][A-Z0-9]*-\d+$/;
const STATUSES = new Set(['planned', 'active', 'conditional']);
const ACTIVE_WITH_PROTOCOL = /^active\s+—\s+(\S+)$/;
const TAG_PATTERN = /INV:([A-Z][A-Z0-9]*-\d+)/g;

// Where tests live (TESTING §8.3 step 2). Generated output and dependencies are skipped.
const SEARCH = [
  { dir: 'tests', accept: (path) => path.endsWith('.cs') },
  { dir: 'CLIENT', accept: (path) => path.endsWith('.spec.ts') && path.split(sep).includes('src') },
  { dir: 'e2e', accept: (path) => path.endsWith('.ts') },
];
const SKIP_DIRS = new Set([
  'node_modules', 'bin', 'obj', 'dist', 'coverage', 'test-results', 'playwright-report', 'blob-report',
]);

const inActions = process.env.GITHUB_ACTIONS === 'true';
let failed = false;

function error(message) {
  console.error(inActions ? `::error title=Invariant gate::${message}` : `✗ ${message}`);
  failed = true;
}

function warn(message) {
  console.warn(inActions ? `::warning title=Invariant gate::${message}` : `! ${message}`);
}

function findTestingDocument() {
  const versions = readdirSync(DOCS)
    .map((name) => ({ name, match: /^TESTING_v(\d+)\.md$/.exec(name) }))
    .filter(({ match }) => match)
    .map(({ name, match }) => ({ name, version: Number(match[1]) }))
    .sort((a, b) => b.version - a.version); // numeric: v10 before v9
  return versions.length > 0 ? join(DOCS, versions[0].name) : null;
}

// Splits a Markdown table row; "\|" inside a cell is part of the text, not a column border.
function cells(line) {
  return line.trim().replace(/^\|/, '').replace(/\|$/, '').split(/(?<!\\)\|/).map((cell) => cell.trim());
}

function parseCatalogue(file) {
  const invariants = new Map();
  let inTable = false;
  readFileSync(file, 'utf8').split(/\r?\n/).forEach((line, index) => {
    if (CATALOGUE_HEADER.test(line)) {
      inTable = true;
      return;
    }
    if (!inTable) return;
    if (!line.trim().startsWith('|')) {
      inTable = false;
      return;
    }
    if (/^\|[\s|:-]+\|$/.test(line.trim())) return; // separator row

    const row = cells(line);
    const where = `${relative(ROOT, file)}:${index + 1}`;
    const [id, , level, , status] = row;
    if (row.length !== 5 || !ID_PATTERN.test(id)) {
      error(`${where}: catalogue row does not have the form | ID | Invariant | Level | Step | Status |`);
      return;
    }
    if (invariants.has(id)) {
      error(`${where}: ${id} appears twice in the catalogue`);
      return;
    }
    const protocol = ACTIVE_WITH_PROTOCOL.exec(status)?.[1] ?? null;
    if (!STATUSES.has(status) && !protocol) {
      error(`${where}: ${id} has the unknown status "${status}"`);
      return;
    }
    invariants.set(id, { level, active: status === 'active' || protocol !== null, protocol });
  });
  return invariants;
}

function* walk(dir) {
  if (!existsSync(dir)) return;
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!SKIP_DIRS.has(entry.name) && !entry.name.startsWith('.')) yield* walk(join(dir, entry.name));
    } else if (entry.isFile()) {
      yield join(dir, entry.name);
    }
  }
}

function collectTags() {
  const tags = new Map(); // ID → list of "file:line"
  for (const { dir, accept } of SEARCH) {
    for (const file of walk(join(ROOT, dir))) {
      const path = relative(ROOT, file);
      if (!accept(path)) continue;
      readFileSync(file, 'utf8').split(/\r?\n/).forEach((line, index) => {
        for (const [, id] of line.matchAll(TAG_PATTERN)) {
          if (!tags.has(id)) tags.set(id, []);
          tags.get(id).push(`${path}:${index + 1}`);
        }
      });
    }
  }
  return tags;
}

const testingFile = findTestingDocument();
if (!testingFile) {
  error('no docs/TESTING_v<n>.md found');
  process.exit(1);
}
console.log(`Invariant catalogue: ${relative(ROOT, testingFile)}`);

const invariants = parseCatalogue(testingFile);
if (invariants.size === 0) {
  error(`${relative(ROOT, testingFile)} has no catalogue rows (header | ID | Invariant | Level | Step | Status |)`);
  process.exit(1);
}
const tags = collectTags();

const rows = [];
for (const [id, { level, active, protocol }] of invariants) {
  if (!active) continue;
  const tests = tags.get(id)?.length ?? 0;
  if (protocol) {
    if (!level.split(/,\s*/).includes('M')) {
      error(`${id}: only manual (M) invariants may be satisfied by a protocol reference`);
    } else if (!existsSync(join(ROOT, protocol))) {
      error(`${id}: protocol file ${protocol} does not exist`);
    }
    rows.push({ id, level, tests, note: protocol });
  } else {
    if (tests === 0) error(`${id} is active but no test is tagged INV:${id}`);
    rows.push({ id, level, tests, note: tests === 0 ? 'MISSING' : '' });
  }
}

for (const [id, places] of tags) {
  const invariant = invariants.get(id);
  if (!invariant) warn(`INV:${id} does not exist in the catalogue (${places.join(', ')})`);
  else if (!invariant.active) warn(`INV:${id} is not active in the catalogue (${places.join(', ')})`);
}

console.log(`\n${invariants.size} invariants in the catalogue, ${rows.length} active:\n`);
const widths = [7, Math.max(5, ...rows.map((r) => r.level.length)), 5];
const line = (a, b, c, d) => `${a.padEnd(widths[0])}  ${b.padEnd(widths[1])}  ${c.padStart(widths[2])}  ${d}`.trimEnd();
console.log(line('ID', 'Level', 'Tests', ''));
for (const { id, level, tests, note } of rows) console.log(line(id, level, String(tests), note));

console.log(failed ? '\nInvariant gate failed.' : '\nInvariant gate passed.');
process.exit(failed ? 1 : 0);

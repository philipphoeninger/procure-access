// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-release-config.mjs: a ready configuration and ready legal pages pass, each rule
// fails on its case, and the script itself exits with 1 on the repository while a placeholder is left.
// Node's built-in test runner, no dependencies: node --test scripts/check-release-config.test.mjs
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

import {
  checkLegalPage,
  checkRelease,
  checkReleaseConfig,
  ENVIRONMENT_PROD,
  LEGAL_PAGES,
} from './check-release-config.mjs';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const SCRIPT = fileURLToPath(new URL('./check-release-config.mjs', import.meta.url));

const environment = (...lines) =>
  ['export const environment = {', '  production: true,', "  apiUrl: '/api',", ...lines, '};'].join('\n');

test('passes a mailto: address and an https URL', () => {
  assert.deepEqual(checkReleaseConfig(environment("  criteriaSuggestion: 'mailto:vorschlag@werte-it.de'")), []);
  assert.deepEqual(checkReleaseConfig(environment('  criteriaSuggestion: "https://werte-it.de/vorschlag",')), []);
});

test('fails on the placeholder, as address and as host, in any case', () => {
  for (const value of [
    'mailto:kriterium-vorschlagen@example.invalid',
    'mailto:x@INVALID',
    'https://vorschlag.example.Invalid/form',
  ]) {
    const [error] = checkReleaseConfig(environment(`  criteriaSuggestion: '${value}'`));
    assert.match(error, /is the placeholder/, value);
  }
});

test('a domain that only contains "invalid" is not the placeholder', () => {
  assert.deepEqual(checkReleaseConfig(environment("  criteriaSuggestion: 'mailto:a@invalid-domains.de'")), []);
});

test('fails on a value the client does not accept', () => {
  for (const value of ['vorschlag@werte-it.de', 'http://werte-it.de', 'mailto:a@b.de?subject=x', 'javascript:alert(1)', '']) {
    const [error] = checkReleaseConfig(environment(`  criteriaSuggestion: '${value}'`));
    assert.match(error, /neither "mailto:<address>" nor an https URL/, value);
  }
});

test('fails when the setting is missing, not a string literal, or set twice', () => {
  assert.match(checkReleaseConfig(environment())[0], /found 0/);
  assert.match(checkReleaseConfig(environment('  criteriaSuggestion: SUGGESTION,'))[0], /found 0/);
  const twice = environment("  criteriaSuggestion: 'mailto:a@b.de',", "  criteriaSuggestion: 'mailto:c@d.de'");
  assert.match(checkReleaseConfig(twice)[0], /found 2/);
});

test('reads the release environment of the repository (ready, or only the placeholder)', () => {
  const errors = checkReleaseConfig(readFileSync(join(ROOT, ENVIRONMENT_PROD), 'utf8'));
  assert.ok(errors.every((error) => /is the placeholder/.test(error)), errors.join('\n'));
});

// Legal pages (R-SHL-07, S6-28): the release check fails while a page is a placeholder.

const LEGAL_TEXT = '## Angaben zum Anbieter\nVerein\n';

test('passes a legal page with content, also with comments around it', () => {
  assert.deepEqual(checkLegalPage(LEGAL_TEXT), []);
  assert.deepEqual(checkLegalPage(`<!--\nStand und Quelle\n-->\n${LEGAL_TEXT}<!-- Stand -->`), []);
});

test('fails on the placeholder marker, in any spacing and case and next to content', () => {
  for (const source of ['<!-- PLATZHALTER -->\n', '<!--platzhalter-->', `${LEGAL_TEXT}\n<!--  PLATZHALTER  -->\n`]) {
    assert.match(checkLegalPage(source)[0], /still the placeholder/, source);
  }
});

test('fails on a legal page that is missing, empty or only comments', () => {
  assert.match(checkLegalPage(null)[0], /missing/);
  for (const source of ['', ' \n\n', '<!-- Inhalt folgt -->\n', '<!-- Kommentar ohne Ende\n## Titel\nText\n']) {
    assert.match(checkLegalPage(source)[0], /no content/, JSON.stringify(source));
  }
});

test('the word "Platzhalter" in the text is not the marker', () => {
  assert.deepEqual(checkLegalPage(`${LEGAL_TEXT}Platzhalter in Formularen ersetzen keine Beschriftung.\n`), []);
});

/** Runs `check` on a repository root that is ready for a release, changed by `files` (null deletes). */
function withRoot(files, check) {
  const root = mkdtempSync(join(tmpdir(), 'release-check-'));
  try {
    const ready = {
      [ENVIRONMENT_PROD]: environment("  criteriaSuggestion: 'mailto:vorschlag@werte-it.de'"),
      ...Object.fromEntries(LEGAL_PAGES.map((file) => [file, LEGAL_TEXT])),
    };
    for (const [file, content] of Object.entries({ ...ready, ...files })) {
      if (content === null) continue;
      mkdirSync(dirname(join(root, file)), { recursive: true });
      writeFileSync(join(root, file), content);
    }
    return check(root);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

test('the whole release passes when the setting and every legal page are ready', () => {
  assert.deepEqual(withRoot({}, checkRelease), []);
});

test('the whole release fails on each legal page that is a placeholder or missing, naming the file', () => {
  for (const file of LEGAL_PAGES) {
    const placeholder = withRoot({ [file]: '<!-- PLATZHALTER -->\n' }, checkRelease);
    assert.deepEqual(placeholder.map(([name]) => name), [file]);
    const missing = withRoot({ [file]: null }, checkRelease);
    assert.deepEqual(missing, [[file, 'the legal page is missing']]);
  }
});

test('the script exits with 1 on the repository and names every legal page that is a placeholder', () => {
  const placeholders = LEGAL_PAGES.filter((file) => checkLegalPage(readFileSync(join(ROOT, file), 'utf8')).length > 0);
  const run = spawnSync(process.execPath, [SCRIPT], { encoding: 'utf8' });

  for (const file of placeholders) {
    assert.ok(run.stderr.includes(`::error file=${file}::`), run.stderr);
  }
  // The script passes only when nothing is left: the O-6 channel and every legal page.
  assert.equal(run.status, checkRelease(ROOT).length === 0 ? 0 : 1, run.stdout + run.stderr);
});

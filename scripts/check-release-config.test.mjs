// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-release-config.mjs: a ready configuration passes, each rule fails on its case.
// Node's built-in test runner, no dependencies: node --test scripts/check-release-config.test.mjs
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

import { checkReleaseConfig, ENVIRONMENT_PROD } from './check-release-config.mjs';

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
  const root = join(dirname(fileURLToPath(import.meta.url)), '..');
  const errors = checkReleaseConfig(readFileSync(join(root, ENVIRONMENT_PROD), 'utf8'));
  assert.ok(errors.every((error) => /is the placeholder/.test(error)), errors.join('\n'));
});

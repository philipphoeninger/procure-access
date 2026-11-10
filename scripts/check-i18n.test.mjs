// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-i18n.mjs: each rule once with sample data, and each check shown to fail.
// Node's built-in test runner, no dependencies: node --test scripts/check-i18n.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';

import { check, flatten, keysOfCell, lookup, parseUxCopy, setText, syncAreas, templateKeys } from './check-i18n.mjs';

const ux = (rows) =>
  ['## 4. Before', '| Schlüssel | Element | Text | Hinweis |', '|---|---|---|---|', '| `shell.before` | x | not read | |',
    '', '## 5. Texte', '', '| Schlüssel | Element | Text | Hinweis |', '|---|---|---|---|', ...rows, '', '## 7. After', ''].join('\n');

const run = (texts, de, templates = []) =>
  check({ ...parseUxCopy(ux(texts)), de, templates, areas: ['shell'] }).errors;

test('reads key rows of §5–§6 only, by the Text column', () => {
  const { texts, problems } = parseUxCopy(ux(['| `shell.a` | Button | Eins | Hinweis |']));
  assert.deepEqual([...texts], [['shell.a', 'Eins']]);
  assert.equal(problems.size, 0);
  const other = ux(['', '| Schlüssel | Screen | Text |', '|---|---|---|', '| `pageTitle.x` | S1 | Zwei |']);
  assert.equal(parseUxCopy(other).texts.get('pageTitle.x'), 'Zwei');
});

test('expands cells with several keys, absolute and relative', () => {
  assert.deepEqual(keysOfCell('`common.yes` / `common.no`'), ['common.yes', 'common.no']);
  assert.deepEqual(keysOfCell('`a.status.open` / `.done`'), ['a.status.open', 'a.status.done']);
  assert.deepEqual(keysOfCell('`a.col.one` / `.col.two`'), ['a.col.one', 'a.col.two']);
  assert.equal(keysOfCell('**Heading** (`a.b.`)'), null);
  const { texts } = parseUxCopy(ux(['| `shell.yes` / `shell.no` | Wert | Ja / Nein | |']));
  assert.deepEqual([...texts], [['shell.yes', 'Ja'], ['shell.no', 'Nein']]);
});

test('skips headings and removed keys, reports rows it cannot use', () => {
  const { texts, problems } = parseUxCopy(ux([
    '| **Gruppe** (Dialog mit `role="x"`) | | | |',
    '| ~~`shell.gone`~~ | – | entfällt | |',
    '| `shell.md` | x | Text mit `code` | |',
    '| `shell.two` / `.three` | x | nur ein Text | |',
    '| `shell.dup` | x | A | |',
    '| `shell.dup` | x | B | |',
    '| `shell.empty` | x | | |',
    '| `shell.typo` (alt) | x | T | |',
  ]));
  assert.equal(texts.size, 0);
  assert.deepEqual([...problems.keys()].sort(), ['shell.dup', 'shell.empty', 'shell.md', 'shell.three', 'shell.two', 'shell.typo']);
});

test('reports key rows of a table without the columns "Schlüssel" and "Text"', () => {
  const { texts, problems } = parseUxCopy(ux(['', '| Key | Wording |', '|---|---|', '| `shell.lost` | Weg |']));
  assert.equal(texts.size, 0);
  assert.match(problems.get('shell.lost'), /table has no columns "Schlüssel" and "Text"/);
});

test('lookup finds a property name with dots next to a text, like ngx-translate', () => {
  const de = { shell: { nav: { menu: 'Menü', 'menu.close': 'Menü schließen' } }, a: { 'b.c': { d: 'x' } } };
  assert.equal(lookup(de, 'shell.nav.menu'), 'Menü');
  assert.equal(lookup(de, 'shell.nav.menu.close'), 'Menü schließen');
  assert.equal(lookup(de, 'a.b.c.d'), 'x');
  assert.equal(lookup(de, 'shell.nav'), undefined); // a group is not a text
  assert.equal(lookup(de, 'shell.nav.menu.open'), undefined);
});

test('--write stores a key that extends a text as a dotted name, in any order', () => {
  const de = { shell: { nav: { menu: 'Menü' } } };
  const texts = new Map([['shell.x.long.key', 'L'], ['shell.x.long', 'S'], ['shell.nav.menu.close', 'Z'], ['other.y', 'no']]);
  assert.deepEqual(syncAreas(de, texts, ['shell']), ['shell.x.long.key', 'shell.x.long', 'shell.nav.menu.close']);
  assert.deepEqual(de, { shell: { nav: { menu: 'Menü', 'menu.close': 'Z' }, x: { 'long.key': 'L', long: 'S' } } });
  assert.deepEqual(syncAreas(de, texts, ['shell']), []); // nothing left to do
  assert.equal(run(['| `shell.x.long` | x | S | |', '| `shell.x.long.key` | x | L | |', '| `shell.nav.menu` | x | Menü | |',
    '| `shell.nav.menu.close` | x | Z | |'], de).length, 0);
});

test('--write rejects the key segments __proto__, constructor and prototype', () => {
  for (const key of ['shell.__proto__.polluted', 'shell.constructor.prototype.polluted', 'shell.a.prototype']) {
    const de = { shell: {} };
    assert.throws(() => setText(de, key, 'x', new Set()), /is not allowed in a key/, key);
    assert.deepEqual(de, { shell: {} }, key);
  }
  // Also where the rest of the key would become one dotted name next to a text (file header).
  assert.throws(() => setText({ shell: { nav: 'N' } }, 'shell.nav.constructor', 'x', new Set(['shell.nav'])), /"constructor"/);
  // Through --write: nothing written, Object.prototype untouched.
  assert.throws(() => syncAreas({ shell: {} }, new Map([['shell.__proto__.polluted', 'x']]), ['shell']), /"__proto__"/);
  assert.equal({}.polluted, undefined);
});

test('--write rejects an unsafe segment at the start, in the middle and at the end of a key', () => {
  for (const unsafe of ['__proto__', 'constructor', 'prototype']) {
    // `shell.b` does not exist yet: a guard that ran only during the walk would leave it behind.
    for (const key of [`${unsafe}.a`, `shell.b.${unsafe}.c`, `shell.b.${unsafe}`]) {
      const de = { shell: {} };
      assert.throws(() => setText(de, key, 'x', new Set()), (e) => e.message === `${key}: the segment "${unsafe}" is not allowed in a key`, key);
      assert.deepEqual(de, { shell: {} }, key);
    }
  }
  assert.equal({}.a, undefined);
  assert.equal({}.c, undefined);
});

test('check 1 fails on a missing or different text of an area', () => {
  assert.deepEqual(run(['| `shell.a` | x | Eins | |'], { shell: {} }), ['shell.a: missing in de.json (run with --write)']);
  assert.match(run(['| `shell.a` | x | Eins | |'], { shell: { a: 'Uno' } })[0], /differs from the UX copy/);
  assert.match(run(['| `shell.a` / `.b` | x | Eins | |'], { shell: {} })[0], /^UX copy: shell\.a: .*2 keys but 1 texts/);
});

test('check 2 fails on a lowercase key the UX copy lacks; uppercase legacy keys are free', () => {
  assert.deepEqual(run([], { auth: { x: 'X' }, LEGACY: { Y: 'Y' } }), ['auth.x: lowercase key in de.json that the UX copy does not define']);
  assert.match(run(['| `auth.x` | x | Ix | |'], { auth: { x: 'X' } })[0], /differs from the UX copy "Ix"/);
});

test('check 3 fails on a text that another key hides', () => {
  const de = { LEGACY: { 'A.B': 'hidden', A: { B: 'wins' } } };
  assert.deepEqual(run([], de), ['LEGACY.A.B: hidden by another key in de.json, the lookup does not reach it']);
  assert.equal(flatten(de).length, 2);
});

test('check 4 fails on a template key that does not resolve; computed keys are notices', () => {
  const source = [
    `<p>{{ 'LEGACY.OK' | translate }}</p>`,
    `<p [attr.aria-label]="'LEGACY.MISSING' | translate"></p>`,
    `<p>{{ "shell.count" | translatePlural: n }}</p>`,
    `<p>{{ prefixKey() | translate }}</p>`,
  ].join('\n');
  assert.deepEqual(templateKeys(source).dynamic, [4]);
  const { errors, notices } = check({
    texts: new Map(), problems: new Map(), areas: [],
    de: { LEGACY: { OK: 'ok' }, shell: { count: 'n', 'count.one': '1' } },
    templates: [{ file: 'x.html', source }],
  });
  assert.deepEqual(errors, ['shell.count: lowercase key in de.json that the UX copy does not define',
    'shell.count.one: lowercase key in de.json that the UX copy does not define', 'x.html:2: key LEGACY.MISSING is not in de.json']);
  assert.deepEqual(notices, ['x.html:4: key computed at runtime, not checked']);
});

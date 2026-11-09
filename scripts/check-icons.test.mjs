// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-icons.mjs: a fitting icon list passes, each rule fails on its case, and the
// repository's own list, manifest and font pass.
// Node's built-in test runner, no dependencies: node --test scripts/check-icons.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';

import { checkIcons, iconsInScssMap, iconsInTemplate, MAIN_SCSS, repositoryInput } from './check-icons.mjs';

const SCSS = "$easymde-toolbar-icons: (\n  'fa-bold': 'format_bold',\n  'fa-link': \"link\",\n);";

/** A small client that fits its list; each test changes one thing. */
function input() {
  const icons = ['add', 'contrast', 'contrast_rtl_off', 'done', 'format_bold', 'info', 'link', 'warning'];
  return {
    list: {
      icons,
      computed: { 'src/app/notice/notice.ts': ['info', 'warning'] },
      material: { done: 'mat-stepper: done state' },
    },
    manifest: { package: 'material-symbols', version: '0.48.2', icons: [...icons], sha256: 'abc' },
    fontSha256: 'abc',
    pinnedVersion: '0.48.2',
    sources: {
      'src/app/list/list.html': '<button><mat-icon matListItemIcon>add</mat-icon></button>\n<!-- <mat-icon>lock</mat-icon> -->',
      'src/app/settings/settings.html': "<mat-icon>\n  {{ on() ? 'contrast_rtl_off' : \"contrast\" }}\n</mat-icon>",
      'src/app/notice/notice.html': '<mat-icon aria-hidden="true">{{ icon() }}</mat-icon>',
      'src/app/notice/notice.ts': "const ICONS = { info: 'info', attention: 'warning' };",
      [MAIN_SCSS]: SCSS,
    },
  };
}

const errorsOf = (change) => {
  const value = input();
  change(value);
  return checkIcons(value);
};

test('passes a list that fits the client', () => {
  assert.deepEqual(checkIcons(input()), []);
});

test('reads literal names, quoted names in interpolations, and skips HTML comments', () => {
  const { names, computed, errors } = iconsInTemplate(
    "<mat-icon>add</mat-icon><!-- <mat-icon>lock</mat-icon> --><mat-icon>{{ a ? 'x_1' : \"y\" }}</mat-icon><mat-icon>{{ name() }}</mat-icon>",
  );
  assert.deepEqual(names, ['add', 'x_1', 'y']);
  assert.equal(computed, true);
  assert.deepEqual(errors, []);
});

test('a comment ends at its first -->: an icon after it counts, one inside does not', () => {
  const { names } = iconsInTemplate('<!-- <mat-icon>lock</mat-icon> --><mat-icon>add</mat-icon><!-- a --> <mat-icon>edit</mat-icon>');
  assert.deepEqual(names, ['add', 'edit']);
});

test('reads the glyphs of the EasyMDE map', () => {
  assert.deepEqual(iconsInScssMap(SCSS), ['format_bold', 'link']);
  assert.equal(iconsInScssMap('$other: (a: b);'), null);
});

test('fails on a used icon that is not in the list', () => {
  for (const [file, source] of [
    ['src/app/list/list.html', '<mat-icon>add</mat-icon><mat-icon>lock</mat-icon>'],
    ['src/app/inline.ts', "@Component({ template: '<mat-icon>lock</mat-icon>' })"],
  ]) {
    const errors = errorsOf((v) => (v.sources[file] = source));
    assert.equal(errors.length, 1, file);
    assert.match(errors[0], /icon "lock" is not in .*material-symbols-icons\.json; add it and run scripts\/subset-material-symbols\.sh/);
  }
  const fromScss = errorsOf((v) => (v.sources[MAIN_SCSS] = SCSS.replace('"link"', '"image"')));
  assert.ok(fromScss.some((e) => /main\.scss: icon "image" is not in/.test(e)), fromScss.join('\n'));
});

test('fails on a listed icon nobody uses', () => {
  const errors = errorsOf((v) => {
    v.list.icons = [...v.list.icons, 'zoom_in'];
    v.manifest.icons = [...v.list.icons];
  });
  assert.deepEqual(errors.length, 1);
  assert.match(errors[0], /"zoom_in" is not used any more/);
});

test('fails on an unsorted or duplicated list', () => {
  for (const icons of [['link', 'add'], ['add', 'add']]) {
    const errors = errorsOf((v) => (v.list.icons = icons));
    assert.ok(errors.some((e) => /must be sorted and without duplicates/.test(e)), icons.join());
  }
});

test('fails on a computed name whose component is not under "computed"', () => {
  const errors = errorsOf((v) => delete v.list.computed['src/app/notice/notice.ts']);
  assert.ok(
    errors.some((e) => /notice\.html: a <mat-icon> computes its name; list its names under "computed" for src\/app\/notice\/notice\.ts/.test(e)),
    errors.join('\n'),
  );
});

test('fails on a "computed" name its file does not write, and on an unknown file', () => {
  const missing = errorsOf((v) => (v.sources['src/app/notice/notice.ts'] = "const ICONS = { info: 'info' };"));
  assert.ok(missing.some((e) => /"computed" lists "warning" for src\/app\/notice\/notice\.ts, which does not write it/.test(e)), missing.join('\n'));
  const unknown = errorsOf((v) => (v.list.computed['src/app/gone.ts'] = ['add']));
  assert.ok(unknown.some((e) => /"computed" names src\/app\/gone\.ts, which is not a client source file/.test(e)), unknown.join('\n'));
});

test('fails on <mat-icon> content that is not an icon name', () => {
  const errors = errorsOf((v) => (v.sources['src/app/list/list.html'] += '<mat-icon>Add item</mat-icon>'));
  assert.ok(errors.some((e) => /<mat-icon> content "Add item" is not an icon name/.test(e)), errors.join('\n'));
});

test('fails when the EasyMDE map is gone', () => {
  const errors = errorsOf((v) => (v.sources[MAIN_SCSS] = ''));
  assert.ok(errors.some((e) => /\$easymde-toolbar-icons not found/.test(e)), errors.join('\n'));
});

test('fails on a stale subset: other package version, other list, other font, no manifest', () => {
  const cases = [
    [(v) => (v.pinnedVersion = '0.49.0'), /subset of material-symbols 0\.48\.2, package\.json pins 0\.49\.0/],
    [(v) => (v.manifest.icons = ['add']), /subset built from another icon list/],
    [(v) => (v.fontSha256 = 'def'), /is not the font the manifest describes/],
    [(v) => (v.fontSha256 = null), /is not the font the manifest describes/],
    [(v) => (v.manifest = null), /manifest\.json is missing/],
  ];
  for (const [change, pattern] of cases) {
    const errors = errorsOf(change);
    assert.equal(errors.length, 1, String(pattern));
    assert.match(errors[0], pattern);
    assert.match(errors[0], /run scripts\/subset-material-symbols\.sh/);
  }
});

test("the repository's icon list, manifest and font pass", () => {
  assert.deepEqual(checkIcons(repositoryInput()), []);
});

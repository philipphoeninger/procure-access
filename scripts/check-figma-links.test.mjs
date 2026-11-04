// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-figma-links.mjs: each rule once with sample data, and each check shown to fail.
// Node's built-in test runner, no dependencies: node --test scripts/check-figma-links.test.mjs
import assert from 'node:assert/strict';
import { test } from 'node:test';

import { check, figmaTags, FILE_KEY } from './check-figma-links.mjs';

const tag = (node, label = '') => ` * @figma https://www.figma.com/design/${FILE_KEY}/ProcureAccess?node-id=${node}${label}`;
const component = (selector, ...tags) => ['/**', ...tags, ' */', `@Component({ selector: '${selector}' })`].join('\n');

const components = {
  'a/notice.ts': { nodes: ['2032-486'], selector: 'pa-notice' },
  'a/header.ts': { nodes: ['2017-650', '2039-2131'], selector: 'pa-header' },
};
const pages = { 'p/search.ts': '2134-2905' };
const run = (files, pending = {}) => check({ components, pages, pending, files: new Map(Object.entries(files)) });

test('reads the node of every @figma line in order; foreign links give null', () => {
  const source = [tag('2134-2905'), tag('2053-2051', ' (desktop)'), ' * @figma https://www.figma.com/design/other/X?node-id=1-2', tag('1-2&t=x')].join('\n');
  assert.deepEqual(figmaTags(source), ['2134-2905', '2053-2051', null, '1-2']);
});

test('passes mapped files with their tags and counts files not built yet', () => {
  const result = run({
    'a/notice.ts': component('pa-notice', tag('2032-486')),
    'a/header.ts': component('pa-header', tag('2017-650'), tag('2039-2131', ' (compact)')),
  });
  assert.deepEqual(result.errors, []);
  assert.deepEqual([result.checked, result.missing, result.pending], [2, 1, 0]);
});

test('a component fails without each of its nodes, also on a longer node id', () => {
  const { errors } = run({
    'a/notice.ts': component('pa-notice', tag('2032-4860')),
    'a/header.ts': component('pa-header', tag('2017-650')),
  });
  assert.deepEqual(errors, [
    'a/notice.ts: missing or wrong @figma tag (expected node 2032-486)',
    'a/header.ts: missing or wrong @figma tag (expected node 2017-650 and node 2039-2131)',
  ]);
});

test('a page needs its base frame as the first @figma line; further lines are free', () => {
  const ok = run({ 'p/search.ts': [tag('2134-2905'), tag('2053-2051')].join('\n') });
  assert.deepEqual(ok.errors, []);
  const { errors } = run({ 'p/search.ts': [tag('2053-2051'), tag('2134-2905')].join('\n') });
  assert.deepEqual(errors, ['p/search.ts: missing or wrong @figma tag (expected first @figma line node 2134-2905)']);
});

test('a mapped component declared at another path fails, also next to the mapped file', () => {
  const moved = run({ 'b/notice.component.ts': component('pa-notice') });
  assert.deepEqual(moved.errors, [
    'b/notice.component.ts: declares pa-notice, which the mapping places at a/notice.ts; move it there or update the mapping §2 and COMPONENTS',
  ]);
  assert.equal(moved.missing, 3);
  const twice = run({ 'a/notice.ts': component('pa-notice', tag('2032-486')), 'b/copy.ts': component('pa-notice') });
  assert.equal(twice.errors.length, 1);
  assert.deepEqual(run({ 'b/other.ts': component('pa-notice-list') }).errors, []);
});

test('a PENDING file without a tag of this Figma file gets a notice', () => {
  const result = run({ 'a/header.ts': component('pa-header') }, { 'a/header.ts': 'legacy header' });
  assert.deepEqual(result.errors, []);
  assert.deepEqual(result.notices, ['a/header.ts skipped: legacy header']);
  assert.deepEqual([result.checked, result.pending], [0, 1]);
});

test('a PENDING file that carries its tag gets a notice to remove the entry', () => {
  const files = { 'a/header.ts': component('pa-header', tag('2017-650'), tag('2039-2131')) };
  const result = run(files, { 'a/header.ts': 'legacy header' });
  assert.deepEqual(result.errors, []);
  assert.deepEqual(result.notices, ['a/header.ts now carries its @figma tag; remove it from PENDING']);
});

test('a PENDING file with a wrong node fails', () => {
  const { errors } = run({ 'a/header.ts': component('pa-header', tag('2017-651')) }, { 'a/header.ts': 'legacy header' });
  assert.deepEqual(errors, ['a/header.ts: on the PENDING list, but its @figma tag is wrong (expected node 2017-650 and node 2039-2131)']);
});

test('a PENDING entry fails when its file is gone or it is not mapped', () => {
  const { errors } = run({}, { 'a/header.ts': 'legacy header', 'x/unmapped.ts': 'reason' });
  assert.deepEqual(errors, [
    'PENDING entry x/unmapped.ts is neither in COMPONENTS nor in PAGES',
    'a/header.ts: on the PENDING list, but the file does not exist; remove the entry',
  ]);
});

test('base is put in front of every path in a message', () => {
  const { errors } = check({ components, pages, pending: {}, files: new Map([['b/x.ts', component('pa-notice')]]), base: 'C/' });
  assert.match(errors[0], /^C\/b\/x\.ts: .* at C\/a\/notice\.ts;/);
});

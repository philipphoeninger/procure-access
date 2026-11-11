// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// i18n check (CLAUDE §6.5, CI stage 1). Node, no dependencies:
//   node scripts/check-i18n.mjs           check only
//   node scripts/check-i18n.mjs --write   first copy the texts of AREAS from the UX copy into de.json
// Source of the texts: the tables of §5 and §6 in the highest docs/ux_copy_procureaccess_v*.md.
// Checks:
//   1. every UX copy key of AREAS is in de.json with exactly the UX copy text;
//   2. every lowercase key in de.json is a UX copy key with the same text (uppercase = legacy);
//   3. every text in de.json is found under its own path (no two keys hide each other);
//   4. every literal key before `| translate` / `| translatePlural` in the client resolves.
// A key with text can also be the start of a longer key (`shell.nav.menu`, `shell.nav.menu.close`;
// SPEC SD25 `.one`/`.zero`). Nested JSON cannot hold both, so the longer key is stored as one
// property name with dots next to the text: "menu": "Menü", "menu.close": "Menü schließen".
// ngx-translate finds both (its getValue, mirrored in lookup() below; pinned by S2-12's test).
import { readdirSync, readFileSync, realpathSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

// UX copy areas that de.json carries in full: a key root (`shell`) or a deeper prefix
// (`criteria.select`), when only part of a root has its screen yet. Add an area with the step that
// builds its screens.
export const AREAS = ['common', 'shell', 'errors', 'legal', 'home', 'suggest', 'criteria.select', 'criteria.list', 'criteria.item', 'criteria.priority', 'testType'];

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const CLIENT = 'CLIENT/CLIENT.ProcureAccess';
const DE_JSON = `${CLIENT}/public/i18n/de.json`;
const KEY = /^[a-z][A-Za-z0-9]*(\.[A-Za-z0-9]+)+$/;

/** Splits a Markdown table row into trimmed cells; `\|` stays inside a cell. */
function cells(line) {
  return line
    .trim()
    .replace(/^\||\|$/g, '')
    .split(/(?<!\\)\|/)
    .map((c) => c.trim());
}

/**
 * Expands a key cell: "`a.b.c`", "`a.b.c` / `a.b.d`" or "`a.b.c` / `.d` / `.e`" (a relative key
 * with n segments replaces the last n segments of the first key). Returns null if the cell is
 * not a key cell.
 */
export function keysOfCell(cell) {
  const parts = cell.split(' / ');
  const tokens = parts.map((p) => /^`([^`]+)`$/.exec(p)?.[1]);
  if (tokens.some((t) => t === undefined)) return null;
  const [first, ...more] = tokens;
  if (!KEY.test(first)) return null;
  const segs = first.split('.');
  const keys = [first];
  for (const rel of more) {
    if (KEY.test(rel)) {
      keys.push(rel);
      continue;
    }
    if (!/^(\.[A-Za-z0-9]+)+$/.test(rel)) return null;
    const relSegs = rel.slice(1).split('.');
    if (relSegs.length >= segs.length) return null;
    keys.push([...segs.slice(0, segs.length - relSegs.length), ...relSegs].join('.'));
  }
  return keys;
}

/**
 * Reads the text tables of §5 and §6. Returns `texts` (key → text, in document order) and
 * `problems` (key → why its row cannot be used). A problem only fails the check for a key
 * that de.json needs, so rows of later steps never block it.
 */
export function parseUxCopy(markdown) {
  const lines = markdown.split('\n');
  const start = lines.findIndex((l) => /^## 5\. /.test(l));
  const end = lines.findIndex((l) => /^## 7\. /.test(l));
  if (start < 0 || end < start) throw new Error('UX copy: sections "## 5." and "## 7." not found');

  const texts = new Map();
  const problems = new Map();
  const problem = (key, why) => {
    if (!problems.has(key)) problems.set(key, why);
  };
  let textCol = -1; // column "Text" of the current table; -1 outside a table "| Schlüssel | … | Text |"
  let inTable = false;

  for (let i = start; i < end; i++) {
    const line = lines[i];
    if (!line.startsWith('|')) {
      inTable = false; // a table ends at the first line that is not a row
      continue;
    }
    const row = cells(line);
    if (!inTable) {
      inTable = true;
      textCol = row[0] === 'Schlüssel' ? row.indexOf('Text') : -1;
      continue;
    }
    if (/^:?-+:?$/.test(row[0])) continue;

    const where = `line ${i + 1}`;
    const keyCell = row[0];
    if (textCol < 0) {
      // A key row in a table without the columns "Schlüssel" and "Text" would be dropped silently.
      for (const key of keysOfCell(keyCell) ?? []) problem(key, `${where}: table has no columns "Schlüssel" and "Text"`);
      continue;
    }
    if (/^~~.*~~$/.test(keyCell)) continue; // removed key, kept in the copy for history
    const keys = keysOfCell(keyCell);
    if (!keys) {
      // Headings like "**Sitzungswarnung** (…)" are not key rows. A key-like token in a cell
      // that is not a plain key cell is: report it, so a typo cannot drop a key silently.
      for (const [, token] of keyCell.matchAll(/`([^`]+)`/g)) {
        if (KEY.test(token)) problem(token, `${where}: key cell "${keyCell}" is not "\`key\`" or "\`key\` / \`.rel\`"`);
      }
      continue;
    }
    const text = row[textCol] ?? '';
    const values = keys.length > 1 ? text.split(' / ') : [text];
    for (const [n, key] of keys.entries()) {
      const value = values[n];
      if (values.length !== keys.length) problem(key, `${where}: ${keys.length} keys but ${values.length} texts`);
      else if (!value) problem(key, `${where}: no text`);
      else if (/`|\*\*|~~|<|\]\(|\\\|/.test(value)) problem(key, `${where}: Markdown in the text "${value}"`);
      else if (texts.has(key) && texts.get(key) !== value) problem(key, `${where}: defined twice with different texts`);
      else texts.set(key, value);
    }
  }
  for (const key of problems.keys()) texts.delete(key);
  return { texts, problems };
}

/** ngx-translate 18 `getValue` for string keys: a property name may itself contain dots. */
export function lookup(dict, key) {
  const segs = key.split('.');
  let target = dict;
  let k = '';
  while (segs.length) {
    k += segs.shift();
    const last = segs.length === 0;
    const value = target !== null && typeof target === 'object' ? target[k] : undefined;
    if (value !== undefined && value !== null && (typeof value === 'object' || last)) {
      target = value;
      k = '';
    } else if (last) {
      return undefined;
    } else {
      k += '.';
    }
  }
  return typeof target === 'string' ? target : undefined;
}

/** All texts of a nested dictionary as [key, text]; dotted property names join like any other. */
export function flatten(dict, prefix = '') {
  return Object.entries(dict).flatMap(([name, value]) => {
    const key = prefix ? `${prefix}.${name}` : name;
    return value !== null && typeof value === 'object' ? flatten(value, key) : [[key, value]];
  });
}

/**
 * Sets `key` to `text`. Where a shorter key in `textKeys` holds a text, the rest of the key
 * goes into one property name with dots next to it (file header).
 * A segment `__proto__`, `constructor` or `prototype` is rejected: it would write into the
 * prototype of every object instead of into de.json (#182). All segments are checked before
 * anything is written, so a rejected key leaves `dict` unchanged.
 */
export function setText(dict, key, text, textKeys) {
  const segs = key.split('.');
  const unsafe = segs.find((name) => name === '__proto__' || name === 'constructor' || name === 'prototype');
  if (unsafe !== undefined) throw new Error(`${key}: the segment "${unsafe}" is not allowed in a key`);
  let cur = dict;
  for (let i = 0; i < segs.length; i++) {
    const name = segs[i];
    // Unreachable after the check above; CodeQL counts only a check on `name` itself (#233).
    if (name === '__proto__' || name === 'constructor' || name === 'prototype') {
      throw new Error(`${key}: the segment "${name}" is not allowed in a key`);
    }
    if (i === segs.length - 1) {
      if (cur[name] !== null && typeof cur[name] === 'object') throw new Error(`${key}: is a group of keys in de.json, cannot hold a text`);
      cur[name] = text;
      return;
    }
    if (textKeys.has(segs.slice(0, i + 1).join('.'))) {
      cur[segs.slice(i).join('.')] = text;
      return;
    }
    if (cur[name] === undefined) cur[name] = {};
    else if (typeof cur[name] !== 'object') throw new Error(`${key}: "${segs.slice(0, i + 1).join('.')}" holds a text`);
    cur = cur[name];
  }
}

/** True if `key` is an area or lies below one (`criteria.select.title` is in `criteria.select`). */
export function inAreas(key, areas) {
  return areas.some((area) => key === area || key.startsWith(`${area}.`));
}

/** Copies the texts of `areas` from the UX copy into `de`. Returns the keys it added or changed. */
export function syncAreas(de, texts, areas) {
  const wanted = [...texts.keys()].filter((k) => inAreas(k, areas));
  const textKeys = new Set([...flatten(de).map(([k]) => k), ...wanted]);
  const changed = [];
  for (const key of wanted) {
    if (lookup(de, key) === texts.get(key)) continue;
    setText(de, key, texts.get(key), textKeys);
    changed.push(key);
  }
  return changed;
}

/**
 * Finds the `| translate` and `| translatePlural` pipes in a template or component source.
 * `keys`: literal keys with their line; `dynamic`: lines whose key is computed at runtime.
 */
export function templateKeys(source) {
  const keys = [];
  const dynamic = [];
  for (const match of source.matchAll(/\|\s*translate(?:Plural)?\b/g)) {
    const line = source.slice(0, match.index).split('\n').length;
    const literal = /(['"])([A-Za-z][\w.-]*)\1\s*$/.exec(source.slice(Math.max(0, match.index - 300), match.index));
    if (literal) keys.push({ key: literal[2], line });
    else dynamic.push(line);
  }
  return { keys, dynamic };
}

/** Runs checks 1–4. `templates`: [{ file, source }]. Returns error and notice messages. */
export function check({ texts, problems, de, templates, areas }) {
  const errors = [];
  const notices = [];
  for (const [key, why] of problems) {
    if (inAreas(key, areas) || lookup(de, key) !== undefined) errors.push(`UX copy: ${key}: ${why}`);
  }
  for (const [key, text] of texts) {
    if (!inAreas(key, areas)) continue;
    const actual = lookup(de, key);
    if (actual === undefined) errors.push(`${key}: missing in de.json (run with --write)`);
    else if (actual !== text) errors.push(`${key}: de.json "${actual}" differs from the UX copy "${text}" (run with --write)`);
  }
  for (const [key, value] of flatten(de)) {
    if (typeof value !== 'string') errors.push(`${key}: not a text in de.json`);
    else if (lookup(de, key) !== value) errors.push(`${key}: hidden by another key in de.json, the lookup does not reach it`);
    else if (/^[a-z]/.test(key) && !problems.has(key)) {
      if (!texts.has(key)) errors.push(`${key}: lowercase key in de.json that the UX copy does not define`);
      else if (texts.get(key) !== value) errors.push(`${key}: de.json "${value}" differs from the UX copy "${texts.get(key)}"`);
    }
  }
  for (const { file, source } of templates) {
    const { keys, dynamic } = templateKeys(source);
    for (const { key, line } of keys) {
      if (lookup(de, key) === undefined) errors.push(`${file}:${line}: key ${key} is not in de.json`);
    }
    for (const line of dynamic) notices.push(`${file}:${line}: key computed at runtime, not checked`);
  }
  return { errors, notices };
}

function highestUxCopy() {
  const versions = readdirSync(join(ROOT, 'docs'))
    .map((f) => /^ux_copy_procureaccess_v(\d+)\.md$/.exec(f))
    .filter(Boolean)
    .sort((a, b) => Number(b[1]) - Number(a[1]));
  if (!versions.length) throw new Error('no docs/ux_copy_procureaccess_v*.md');
  return `docs/${versions[0][0]}`;
}

function clientSources(dir) {
  return readdirSync(join(ROOT, dir), { withFileTypes: true }).flatMap((e) => {
    const rel = `${dir}/${e.name}`;
    if (e.isDirectory()) return clientSources(rel);
    return /\.(html|ts)$/.test(e.name) && !/\.spec\.ts$/.test(e.name) ? [rel] : [];
  });
}

function main() {
  const write = process.argv.includes('--write');
  const inActions = process.env.GITHUB_ACTIONS === 'true';
  const uxFile = highestUxCopy();
  const { texts, problems } = parseUxCopy(readFileSync(join(ROOT, uxFile), 'utf8'));
  const de = JSON.parse(readFileSync(join(ROOT, DE_JSON), 'utf8'));

  if (write) {
    const changed = syncAreas(de, texts, AREAS);
    if (changed.length) writeFileSync(join(ROOT, DE_JSON), JSON.stringify(de, null, 2) + '\n');
    console.log(`${DE_JSON}: ${changed.length} keys added or changed from ${uxFile}`);
    for (const key of changed) console.log(`  ${key}`);
  }

  const templates = clientSources(`${CLIENT}/src`).map((file) => ({ file, source: readFileSync(join(ROOT, file), 'utf8') }));
  const { errors, notices } = check({ texts, problems, de, templates, areas: AREAS });
  for (const n of notices) console.log(inActions ? `::notice title=i18n check::${n}` : `- ${n}`);
  for (const e of errors) console.error(`✗ ${e}`);
  const areaKeys = [...texts.keys()].filter((k) => inAreas(k, AREAS)).length;
  console.log(
    `i18n check (${uxFile}): ${areaKeys} keys of ${AREAS.join(', ')}, ${templates.length} client files, ` +
      `${errors.length} errors, ${notices.length} keys not checked.`,
  );
  process.exit(errors.length ? 1 : 0);
}

// Real paths on both sides: through a symlinked path the check would otherwise skip itself and pass.
if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();

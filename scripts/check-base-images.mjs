// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Base images and their licence statement (S7-15, TD-44, owner decision O-S7-15). Node, no
// dependencies, CI stage 1:
//   node scripts/check-base-images.mjs
// Every image a Dockerfile (`FROM`) or a compose file (`image:`) names must appear, with the same tag,
// in the tables of docs/BASE_IMAGES.md (a digest after `@` is ignored), and every image of the tables
// must still be used. A shipped image also needs the place of its licence information. A changed tag
// therefore stays red until the statement is changed, which is the moment to look at the new tag.
import { readdirSync, readFileSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const STATEMENT = 'docs/BASE_IMAGES.md';
const SKIPPED_FOLDERS = new Set(['node_modules', '.git', '.claude', 'bin', 'obj', 'dist', '.angular']);
const SOURCE_FILE = /^(Dockerfile(\..+)?|docker-compose(\..+)?\.ya?ml)$/;
const FIXED_TAG = /^[^:$@\s]+:[^:$@\s]+$/;

/** The image references of one Dockerfile or compose file, without digest. */
export function imagesOf(file, text) {
  const refs = [];
  const dockerfile = file.split('/').pop().startsWith('Dockerfile');
  text.split(/\r?\n/).forEach((line, index) => {
    const m = dockerfile ? /^\s*FROM\s+(?:--\S+\s+)*(\S+)/i.exec(line) : /^\s*image:\s*["']?([^\s"'#]+)/.exec(line);
    const ref = m?.[1];
    if (ref && ref.toLowerCase() !== 'scratch') refs.push({ file, line: index + 1, ref: ref.split('@')[0] });
  });
  return refs;
}

/** The two tables of the statement: `shipped` and `other`, each a list of { ref, info }. */
export function parseStatement(text) {
  const result = { shipped: [], other: [] };
  let section = null;
  for (const line of text.split(/\r?\n/)) {
    if (/^##\s+Shipped images/i.test(line)) section = 'shipped';
    else if (/^##\s+Not shipped/i.test(line)) section = 'other';
    else if (/^##\s/.test(line)) section = null;
    const m = section && /^\|\s*`([^`\s]+)`\s*\|(.*)\|\s*$/.exec(line);
    if (m) {
      const cells = m[2].split('|').map((cell) => cell.trim());
      result[section].push({ ref: m[1], info: cells[cells.length - 1] ?? '' });
    }
  }
  return result;
}

/** The problems between the references found in the sources and the statement. */
export function check(refs, statement) {
  const problems = [];
  const entries = [...statement.shipped, ...statement.other];
  const known = new Set(entries.map((entry) => entry.ref));
  const used = new Set();
  for (const { file, line, ref } of refs) {
    used.add(ref);
    if (!FIXED_TAG.test(ref)) {
      problems.push(`${file}:${line}: image ${ref} has no fixed tag (or uses a variable); pin a tag so the statement can name it`);
    } else if (!known.has(ref)) {
      const name = ref.split(':')[0];
      const same = entries.filter((entry) => entry.ref.split(':')[0] === name).map((entry) => entry.ref);
      problems.push(
        `${file}:${line}: image ${ref} is not named in ${STATEMENT}` +
          (same.length ? ` (the statement has ${same.join(', ')}: a tag changed, review its licence information and update the statement)` : ' (add it to the shipped or the not shipped table)'),
      );
    }
  }
  for (const entry of entries) {
    if (!used.has(entry.ref)) problems.push(`${STATEMENT}: ${entry.ref} is no longer used by a Dockerfile or compose file; remove it`);
  }
  for (const entry of statement.shipped) {
    if (!/https?:\/\//.test(entry.info)) problems.push(`${STATEMENT}: shipped image ${entry.ref} names no place for its licence information (a link)`);
  }
  const shippedRefs = new Set(statement.shipped.map((entry) => entry.ref));
  for (const entry of statement.other) {
    if (shippedRefs.has(entry.ref)) problems.push(`${STATEMENT}: ${entry.ref} is in both tables`);
  }
  return problems;
}

function sourceFiles(dir) {
  const files = [];
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!SKIPPED_FOLDERS.has(entry.name)) files.push(...sourceFiles(join(dir, entry.name)));
    } else if (SOURCE_FILE.test(entry.name)) {
      files.push(join(dir, entry.name));
    }
  }
  return files;
}

export function run(root = ROOT) {
  const refs = sourceFiles(root).flatMap((path) => imagesOf(relative(root, path).split('\\').join('/'), readFileSync(path, 'utf8')));
  const statement = parseStatement(readFileSync(join(root, STATEMENT), 'utf8'));
  return { refs, problems: check(refs, statement) };
}

if (process.argv[1] === fileURLToPath(import.meta.url)) {
  const { refs, problems } = run();
  if (problems.length) {
    console.error(problems.join('\n'));
    process.exit(1);
  }
  console.log(`Base images: ${new Set(refs.map((r) => r.ref)).size} images in ${STATEMENT} match the Dockerfiles and compose files.`);
}

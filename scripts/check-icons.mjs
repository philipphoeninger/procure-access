// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Icon check (#135, S3-25; TESTING V117). Node, no dependencies, CI stage 1:
//   node scripts/check-icons.mjs
// The client ships a subset of the Material Symbols font with only the icons of the icon list
// (src/assets/fonts/material-symbols-icons.json); an icon that is not in the subset stays blank.
// Checks:
//   1. the icon list holds exactly the names the client uses: the literal names in `<mat-icon>`
//      (templates and inline templates, HTML comments left out; quoted names inside `{{ }}`), the
//      glyphs of the EasyMDE toolbar map in main.scss, the names of `computed` (a `<mat-icon>` whose
//      name has no literal needs its component's .ts file there, and each name there must be
//      written in that file) and of `material` (icons that Angular Material renders itself);
//   2. the manifest that scripts/subset-material-symbols.py writes matches the list, the version of
//      material-symbols pinned in package.json and the font file — otherwise the subset is stale
//      and scripts/subset-material-symbols.sh regenerates it (e.g. after a Dependabot update).
// Names that `fontIcon`, `svgIcon` or a CSS class choose are not seen; the client uses none.
import { createHash } from 'node:crypto';
import { readdirSync, readFileSync, realpathSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const CLIENT = 'CLIENT/CLIENT.ProcureAccess';
export const ICON_LIST = `${CLIENT}/src/assets/fonts/material-symbols-icons.json`;
export const MANIFEST = `${CLIENT}/src/assets/fonts/material-symbols-outlined.manifest.json`;
export const FONT = `${CLIENT}/src/assets/fonts/material-symbols-outlined.woff2`;
/** Relative to the client, like the file names in the icon list. */
export const MAIN_SCSS = 'src/styles/main.scss';
const PACKAGE_JSON = `${CLIENT}/package.json`;
const REGENERATE = 'run scripts/subset-material-symbols.sh';

const MAT_ICON = /<mat-icon\b[^>]*>([\s\S]*?)<\/mat-icon>/g;
const QUOTED = /'([^'\\]*)'|"([^"\\]*)"/g;
const NAME = /^[a-z0-9_]+$/;

const quoted = (text) => [...text.matchAll(QUOTED)].map((m) => m[1] ?? m[2]);

/** The icon names of one template or .ts file; `computed` is true if a name has no literal. */
export function iconsInTemplate(source) {
  const names = [];
  const errors = [];
  let computed = false;
  for (const [, raw] of source.replace(/<!--[\s\S]*?-->/g, '').matchAll(MAT_ICON)) {
    const content = raw.trim();
    if (content.includes('{{')) {
      const literals = quoted(content);
      if (literals.length) names.push(...literals);
      else computed = true;
    } else if (NAME.test(content)) {
      names.push(content);
    } else {
      errors.push(`<mat-icon> content "${content}" is not an icon name`);
    }
  }
  return { names, computed, errors };
}

/** The glyph names of the `$easymde-toolbar-icons` map in main.scss. */
export function iconsInScssMap(source) {
  const map = /\$easymde-toolbar-icons:\s*\(([\s\S]*?)\);/.exec(source);
  if (!map) return null;
  return [...map[1].matchAll(/['"][^'"]+['"]\s*:\s*['"]([^'"]+)['"]/g)].map((m) => m[1]);
}

/**
 * Every problem of the icon list and the subset; empty when they fit the client.
 * `sources` maps each client file (path relative to the client) to its text: templates, .ts files,
 * main.scss.
 */
export function checkIcons({ list, manifest, fontSha256, pinnedVersion, sources }) {
  const errors = [];
  const found = new Map(); // name → first place it was found
  const add = (name, where) => found.has(name) || found.set(name, where);
  const at = (file) => `${CLIENT}/${file}`;

  const icons = list.icons ?? [];
  const sorted = [...new Set(icons)].sort();
  if (sorted.join() !== icons.join()) errors.push(`${ICON_LIST}: "icons" must be sorted and without duplicates`);

  const computedFiles = list.computed ?? {};
  for (const [file, source] of Object.entries(sources)) {
    if (!/\.(html|ts)$/.test(file)) continue;
    const result = iconsInTemplate(source);
    for (const error of result.errors) errors.push(`${at(file)}: ${error}`);
    for (const name of result.names) add(name, at(file));
    const component = file.replace(/\.html$/, '.ts');
    if (result.computed && !(component in computedFiles)) {
      errors.push(`${at(file)}: a <mat-icon> computes its name; list its names under "computed" for ${component}`);
    }
  }

  const scss = iconsInScssMap(sources[MAIN_SCSS] ?? '');
  if (scss === null) errors.push(`${at(MAIN_SCSS)}: $easymde-toolbar-icons not found`);
  for (const name of scss ?? []) add(name, at(MAIN_SCSS));

  for (const [file, names] of Object.entries(computedFiles)) {
    const source = sources[file];
    if (source === undefined) {
      errors.push(`${ICON_LIST}: "computed" names ${file}, which is not a client source file`);
      continue;
    }
    const literals = new Set(quoted(source));
    for (const name of names) {
      if (literals.has(name)) add(name, at(file));
      else errors.push(`${ICON_LIST}: "computed" lists "${name}" for ${file}, which does not write it`);
    }
  }
  for (const name of Object.keys(list.material ?? {})) add(name, 'Angular Material');

  for (const [name, where] of found) {
    if (!icons.includes(name)) errors.push(`${where}: icon "${name}" is not in ${ICON_LIST}; add it and ${REGENERATE}`);
  }
  for (const name of icons) {
    if (!found.has(name)) errors.push(`${ICON_LIST}: "${name}" is not used any more; remove it and ${REGENERATE}`);
  }

  if (!manifest) {
    errors.push(`${MANIFEST} is missing: ${REGENERATE}`);
  } else {
    if (manifest.version !== pinnedVersion) {
      errors.push(`${MANIFEST}: subset of material-symbols ${manifest.version}, package.json pins ${pinnedVersion}: ${REGENERATE}`);
    }
    if ((manifest.icons ?? []).join() !== icons.join()) {
      errors.push(`${MANIFEST}: subset built from another icon list: ${REGENERATE}`);
    }
    if (manifest.sha256 !== fontSha256) {
      errors.push(`${FONT} is not the font the manifest describes: ${REGENERATE}`);
    }
  }
  return errors;
}

/** Templates and .ts files below `dir` (relative to the client), without specs. */
function clientSources(dir) {
  return readdirSync(join(ROOT, CLIENT, dir), { withFileTypes: true }).flatMap((e) => {
    const rel = `${dir}/${e.name}`;
    if (e.isDirectory()) return clientSources(rel);
    return /\.(html|ts)$/.test(e.name) && !/\.spec\.ts$/.test(e.name) ? [rel] : [];
  });
}

const read = (file) => readFileSync(join(ROOT, file), 'utf8');
const readJson = (file) => {
  try {
    return JSON.parse(read(file));
  } catch {
    return null;
  }
};
const sha256 = (file) => {
  try {
    return createHash('sha256').update(readFileSync(join(ROOT, file))).digest('hex');
  } catch {
    return null;
  }
};

/** The check's input as read from the repository. */
export function repositoryInput() {
  const files = [...clientSources('src/app'), MAIN_SCSS];
  return {
    list: JSON.parse(read(ICON_LIST)),
    manifest: readJson(MANIFEST),
    fontSha256: sha256(FONT),
    pinnedVersion: JSON.parse(read(PACKAGE_JSON)).dependencies?.['material-symbols'],
    sources: Object.fromEntries(files.map((file) => [file, read(`${CLIENT}/${file}`)])),
  };
}

function main() {
  const input = repositoryInput();
  const errors = checkIcons(input);
  for (const error of errors) console.error(`::error::${error}`);
  console.log(`Icon check (${input.list.icons.length} icons in ${ICON_LIST}): ${errors.length} error(s).`);
  process.exit(errors.length ? 1 : 0);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();

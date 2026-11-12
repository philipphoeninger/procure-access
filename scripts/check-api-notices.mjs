// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Licence notices of the API image (#187, S4-27, owner decisions O-S4-90 – O-S4-93). Node, no
// dependencies, CI stage 2 after the .NET build:
//   node scripts/check-api-notices.mjs           check API/API.ProcureAccess/THIRD-PARTY-NOTICES.txt
//   node scripts/check-api-notices.mjs --write   regenerate it (after a NuGet update)
//   … --deps <file>                              use this deps.json, e.g. the one of a built image
// Without --deps both run `dotnet publish --no-build -c Debug` on the API (after `dotnet build`,
// which builds Debug; publish alone would default to Release) and read the published deps.json,
// so the file lists exactly the NuGet packages the image ships — not the design-time packages a
// plain build lists. Debug and Release publish the same packages. Which packages ship depends on the .NET SDK (newer
// SDKs prune packages the shared framework already has), so generate the file with the SDK that
// CI and the image's sdk:10.0 use (the latest 10.0), or from the image's deps.json:
//   docker run --rm --entrypoint cat <api image> /app/API.ProcureAccess.deps.json > deps.json
// Per package it reads the .nuspec and the licence and notice files in the NuGet package folder
// (NUGET_PACKAGES or ~/.nuget/packages).
// Rules:
//   1. a package's licence expression must be on the allowlist below (CLAUDE §10, ARCHITECTURE
//      D12) or the package must have its own reviewed text in PACKAGE_TEXTS; anything else fails
//      until someone checks the licence and adds it here;
//   2. third-party code a package embeds without its notices is a reviewed entry in EMBEDDED,
//      pinned to the package version: a new version fails until its entry is checked again;
//   3. the committed file must equal the generated one.
// The file lists each package with its copyright line and each distinct licence or notice text
// once. A package without its own licence file gets the standard text of its licence.
import { execFileSync } from 'node:child_process';
import { existsSync, mkdtempSync, readdirSync, readFileSync, realpathSync, rmSync, writeFileSync } from 'node:fs';
import { homedir, tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
export const NOTICES = 'API/API.ProcureAccess/THIRD-PARTY-NOTICES.txt';
const API_PROJECT = 'API/API.ProcureAccess/API.ProcureAccess.csproj';
const DEPS_FILE = 'API.ProcureAccess.deps.json';
const TEXTS = 'scripts/api-notices';
const REGENERATE =
  'run `dotnet build ProcureAccess.sln`, then `node scripts/check-api-notices.mjs --write` with the SDK of CI (or --deps with the deps.json of the image)';

/** Licences any package may carry, with their standard text (MIT's copyright placeholder is removed). */
export const STANDARD_TEXTS = {
  MIT: 'LICENSES/MIT.txt',
  'Apache-2.0': 'LICENSES/Apache-2.0.txt',
};
const MIT_PLACEHOLDER = /^Copyright \(c\) <year> <copyright holders>\n\n/m;

/** Packages whose licence text names its holder, so no standard text fits: the project's own text. */
export const PACKAGE_TEXTS = {
  Npgsql: { licence: 'PostgreSQL', file: `${TEXTS}/npgsql.txt` },
  'Npgsql.EntityFrameworkCore.PostgreSQL': { licence: 'PostgreSQL', file: `${TEXTS}/npgsql-efcore.txt` },
};

/** Third-party code a package embeds without its notices, reviewed for one package version. */
export const EMBEDDED = {
  'Swashbuckle.AspNetCore.SwaggerUI': {
    version: '10.3.1',
    name: 'swagger-ui-dist 5.33.3',
    licence: 'Apache-2.0',
    file: `${TEXTS}/swagger-ui-dist.txt`,
  },
};

/** Licence and notice files in the root of a package folder; the first kind is the package's licence. */
const LICENCE_FILE = /^licen[cs]e(\.(txt|md))?$/i;
const NOTICE_FILE = /^(licen[cs]e|notice|third-party-notices)(\.(txt|md))?$/i;

const normalise = (text) => `${text.replace(/\r\n?/g, '\n').replace(/\n+$/, '')}\n`;
const byId = (a, b) => (a.id.toLowerCase() < b.id.toLowerCase() ? -1 : a.id.toLowerCase() > b.id.toLowerCase() ? 1 : 0);

/** The NuGet packages of a deps.json, sorted by id. */
export function packagesOf(deps) {
  return Object.entries(deps.libraries ?? {})
    .filter(([, library]) => library.type === 'package')
    .map(([key]) => {
      const slash = key.lastIndexOf('/');
      return { id: key.slice(0, slash), version: key.slice(slash + 1) };
    })
    .sort(byId);
}

const ENTITIES = { amp: '&', lt: '<', gt: '>', quot: '"', apos: "'" };
const decode = (text) =>
  text.replace(/&(#x[0-9a-f]+|#\d+|[a-z]+);/gi, (match, name) => {
    if (name[0] !== '#') return ENTITIES[name.toLowerCase()] ?? match;
    return String.fromCodePoint(name[1] === 'x' || name[1] === 'X' ? parseInt(name.slice(2), 16) : Number(name.slice(1)));
  });
const element = (xml, name) => {
  const match = xml.match(new RegExp(`<${name}(\\s[^>]*)?>([\\s\\S]*?)</${name}>`));
  return match ? decode(match[2].trim()) : null;
};

/** What a .nuspec says about licence and authorship. */
export function readNuspec(xml) {
  const licence = xml.match(/<license\s+type="(expression|file)"[^>]*>([^<]*)<\/license>/);
  const repository = xml.match(/<repository\s[^>]*url="([^"]+)"/);
  return {
    licenceExpression: licence?.[1] === 'expression' ? decode(licence[2].trim()) : null,
    licenceFile: licence?.[1] === 'file' ? decode(licence[2].trim()) : null,
    copyright: element(xml, 'copyright'),
    authors: element(xml, 'authors'),
    project: element(xml, 'projectUrl') ?? (repository ? decode(repository[1]) : null),
  };
}

/**
 * Reads one package from the NuGet package folder: { nuspec, files } or null when it is missing.
 * `files` are its licence and notice files as { name, text, isLicence }, the nuspec's licence file first.
 */
export function readPackage(packagesDir, { id, version }) {
  const folder = join(packagesDir, id.toLowerCase(), version.toLowerCase());
  const nuspecFile = join(folder, `${id.toLowerCase()}.nuspec`);
  if (!existsSync(nuspecFile)) return null;
  const nuspec = readNuspec(readFileSync(nuspecFile, 'utf8'));
  const names = readdirSync(folder)
    .filter((name) => NOTICE_FILE.test(name) && name !== nuspec.licenceFile)
    .sort();
  if (nuspec.licenceFile && existsSync(join(folder, nuspec.licenceFile))) names.unshift(nuspec.licenceFile);
  const files = names.map((name) => ({
    name,
    text: readFileSync(join(folder, name), 'utf8'),
    isLicence: name === nuspec.licenceFile || LICENCE_FILE.test(name),
  }));
  return { nuspec, files };
}

/**
 * Builds the notices file. `packages` are { id, version, nuspec, files } with nuspec and files as
 * readPackage gives them (nuspec null when the package is missing from the package folder);
 * `readText(file)` reads a repository file. The self-test passes its own PACKAGE_TEXTS and
 * EMBEDDED. Returns { text, errors }.
 */
export function buildNotices(packages, readText, { packageTexts = PACKAGE_TEXTS, embeddedCode = EMBEDDED } = {}) {
  const errors = [];
  const texts = []; // { title, body, users: [] }
  const textIndex = new Map(); // body → index in texts
  const useText = (title, body, user) => {
    const key = normalise(body);
    if (!textIndex.has(key)) {
      textIndex.set(key, texts.length);
      texts.push({ title, body: key, users: [] });
    }
    const entry = texts[textIndex.get(key)];
    if (!entry.users.includes(user)) entry.users.push(user);
    return textIndex.get(key) + 1;
  };
  const standardText = (licence) => {
    const text = normalise(readText(STANDARD_TEXTS[licence]));
    if (licence !== 'MIT') return text;
    if (!MIT_PLACEHOLDER.test(text)) errors.push(`${STANDARD_TEXTS.MIT}: the copyright placeholder line was not found`);
    return text.replace(MIT_PLACEHOLDER, '');
  };

  const shipped = new Set(packages.map((p) => p.id));
  for (const id of Object.keys(packageTexts)) {
    if (!shipped.has(id)) errors.push(`PACKAGE_TEXTS names ${id}, which the API no longer ships: remove the entry`);
  }
  for (const id of Object.keys(embeddedCode)) {
    if (!shipped.has(id)) errors.push(`EMBEDDED names ${id}, which the API no longer ships: remove the entry`);
  }

  const blocks = [];
  for (const pkg of [...packages].sort(byId)) {
    const name = `${pkg.id} ${pkg.version}`;
    if (!pkg.nuspec) {
      errors.push(`${name}: not in the NuGet package folder (restore the API first)`);
      continue;
    }
    const { licenceExpression, copyright, authors, project } = pkg.nuspec;
    const own = packageTexts[pkg.id];
    const refs = [];
    let licence = licenceExpression;
    if (own) {
      if (licenceExpression !== own.licence) {
        errors.push(
          `${name}: licence is now "${licenceExpression ?? 'none'}", PACKAGE_TEXTS says "${own.licence}": check it (CLAUDE §10) and update the entry`,
        );
      }
      refs.push(useText(`${own.licence} licence of ${pkg.id}`, readText(own.file), name));
    } else if (licenceExpression && STANDARD_TEXTS[licenceExpression]) {
      // A package without a licence file of its own gets the standard text of its licence.
      if (!pkg.files.some((file) => file.isLicence)) {
        refs.push(useText(`${licenceExpression} licence`, standardText(licenceExpression), name));
      }
    } else {
      licence = licenceExpression ?? 'none';
      errors.push(
        licenceExpression
          ? `${name}: licence "${licenceExpression}" is not on the allowlist: check it against CLAUDE §10, then add its text to scripts/check-api-notices.mjs`
          : `${name}: no licence expression in its .nuspec: check its licence against CLAUDE §10, then add it to PACKAGE_TEXTS`,
      );
    }
    for (const file of pkg.files) refs.push(useText(`${file.name} of ${pkg.id}`, file.text, name));

    const lines = [name, `  Licence: ${licence}`];
    if (copyright) lines.push(`  Copyright: ${copyright}`);
    else lines.push(`  Authors: ${authors ?? 'not stated'}`);
    if (project) lines.push(`  Project: ${project}`);

    const embedded = embeddedCode[pkg.id];
    if (embedded) {
      if (embedded.version !== pkg.version) {
        errors.push(
          `${name}: the entry for the code it embeds (${embedded.name}) was checked for ${embedded.version}: check what ${pkg.version} embeds, update ${embedded.file} and the version in EMBEDDED`,
        );
      }
      const notice = useText(`Notices of ${embedded.name}`, readText(embedded.file), name);
      const licenceText = useText(`${embedded.licence} licence`, standardText(embedded.licence), name);
      lines.push(`  Embeds: ${embedded.name} (${embedded.licence}), texts [${notice}], [${licenceText}]`);
    }
    lines.push(`  Texts: ${refs.map((n) => `[${n}]`).join(', ')}`);
    blocks.push(lines.join('\n'));
  }

  const header = [
    'Third-party notices of the ProcureAccess API image',
    '',
    'The API image (API/API.ProcureAccess/Dockerfile) ships the NuGet packages listed below. Their',
    'licences ask for these notices to travel with them. The .NET runtime of the base image carries',
    'its own notices in /usr/share/dotnet/LICENSE.txt and /usr/share/dotnet/ThirdPartyNotices.txt.',
    '',
    'Generated by scripts/check-api-notices.mjs --write from the package metadata; do not edit.',
    'Each package names the texts that apply to it; each text is printed once, under "Texts".',
  ].join('\n');
  const textBlocks = texts.map(
    (t, i) => `[${i + 1}] ${t.title}\n    Applies to: ${t.users.join(', ')}\n\n${t.body}`,
  );
  const rule = '='.repeat(100);
  const text = [header, `${rule}\nPackages (${blocks.length})\n${rule}`, ...blocks, `${rule}\nTexts\n${rule}`, ...textBlocks]
    .join('\n\n')
    .concat('\n');
  return { text, errors };
}

/** Publishes the built API into a temporary folder and returns its deps.json. */
function publishedDeps() {
  const out = mkdtempSync(join(tmpdir(), 'api-notices-'));
  try {
    const args = ['publish', API_PROJECT, '--no-build', '-c', 'Debug', '-o', out, '-p:UseAppHost=false', '-v', 'q', '-nologo'];
    execFileSync('dotnet', args, { cwd: ROOT, encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] });
    return JSON.parse(readFileSync(join(out, DEPS_FILE), 'utf8'));
  } catch (error) {
    // MSBuild writes its errors to stdout.
    const output = `${error.stdout ?? ''}${error.stderr ?? ''}`.trim();
    throw new Error(`dotnet publish --no-build of the API failed (run \`dotnet build ProcureAccess.sln\` first)${output ? `:\n${output}` : ''}`, {
      cause: error,
    });
  } finally {
    rmSync(out, { recursive: true, force: true });
  }
}

const sdkVersion = () => {
  try {
    return execFileSync('dotnet', ['--version'], { cwd: ROOT, encoding: 'utf8' }).trim();
  } catch {
    return 'unknown';
  }
};

function main() {
  const args = process.argv.slice(2);
  const write = args.includes('--write');
  const depsAt = args.indexOf('--deps');
  if (depsAt >= 0 && !args[depsAt + 1]) {
    console.error('usage: node scripts/check-api-notices.mjs [--write] [--deps <deps.json>]');
    process.exit(2);
  }
  const depsFile = depsAt >= 0 ? args[depsAt + 1] : null;
  const deps = depsFile ? JSON.parse(readFileSync(depsFile, 'utf8')) : publishedDeps();
  const source = depsFile ?? `dotnet publish with .NET SDK ${sdkVersion()}`;
  const packagesDir = process.env.NUGET_PACKAGES || join(homedir(), '.nuget', 'packages');
  const packages = packagesOf(deps).map((p) => {
    const read = readPackage(packagesDir, p);
    return { ...p, nuspec: read?.nuspec ?? null, files: read?.files ?? [] };
  });
  const { text, errors } = buildNotices(packages, (file) => readFileSync(join(ROOT, file), 'utf8'));
  const target = join(ROOT, NOTICES);
  if (!errors.length) {
    if (write) writeFileSync(target, text);
    else if (!existsSync(target) || readFileSync(target, 'utf8') !== text) {
      errors.push(`${NOTICES} does not match the shipped packages (${source}): ${REGENERATE}`);
    }
  }
  for (const error of errors) console.error(`::error::${error}`);
  console.log(`API notices (${packages.length} packages${write && !errors.length ? ', written' : ''}): ${errors.length} error(s).`);
  process.exit(errors.length ? 1 : 0);
}

if (process.argv[1] && realpathSync(process.argv[1]) === fileURLToPath(import.meta.url)) main();

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-api-notices.mjs: the notices list every package with the texts that apply to
// it, each rule fails on its case, and the repository's own texts load. Whether the committed file
// matches the shipped packages needs a .NET build: CI stage 2 runs the check itself.
// Node's built-in test runner, no dependencies: node --test scripts/check-api-notices.test.mjs
import assert from 'node:assert/strict';
import { mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

import {
  buildNotices,
  EMBEDDED,
  PACKAGE_TEXTS,
  packagesOf,
  readNuspec,
  readPackage,
  STANDARD_TEXTS,
} from './check-api-notices.mjs';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');

const MIT = 'MIT License\n\nCopyright (c) <year> <copyright holders>\n\nPermission is hereby granted.\n';
const APACHE = 'Apache License\nVersion 2.0\n';
const TEXTS = {
  'LICENSES/MIT.txt': MIT,
  'LICENSES/Apache-2.0.txt': APACHE,
  'own/pg.txt': 'Copyright (c) Pg Team\n\nPostgreSQL-style permission.\n',
  'own/embedded.txt': 'widget.js NOTICE\n',
};
const readText = (file) => {
  if (!(file in TEXTS)) throw new Error(`unknown text ${file}`);
  return TEXTS[file];
};
const CONFIG = {
  packageTexts: { Pg: { licence: 'PostgreSQL', file: 'own/pg.txt' } },
  embeddedCode: { Ui: { version: '2.0.0', name: 'widget 1.0', licence: 'Apache-2.0', file: 'own/embedded.txt' } },
};

const nuspec = (licenceExpression, copyright = `© ${licenceExpression} people`) => ({
  licenceExpression,
  licenceFile: null,
  copyright,
  authors: 'Someone',
  project: 'https://example.org',
});

/** A small set of packages that passes; each test changes one thing. */
function packages() {
  return [
    { id: 'Zeta', version: '1.0.0', nuspec: nuspec('MIT', '© Zeta'), files: [] },
    { id: 'alpha', version: '3.1.0', nuspec: nuspec('MIT', '© Alpha'), files: [] },
    {
      id: 'Own.Licence',
      version: '1.0.0',
      nuspec: nuspec('MIT'),
      files: [{ name: 'LICENSE.txt', text: 'MIT License\nCopyright (c) Own\n', isLicence: true }],
    },
    {
      id: 'Ms.Package',
      version: '10.0.0',
      nuspec: nuspec('MIT', '© Microsoft'),
      files: [{ name: 'THIRD-PARTY-NOTICES.TXT', text: 'Notices of embedded code\n', isLicence: false }],
    },
    { id: 'Logs', version: '4.0.0', nuspec: nuspec('Apache-2.0', null), files: [] },
    { id: 'Pg', version: '10.0.3', nuspec: nuspec('PostgreSQL'), files: [] },
    { id: 'Ui', version: '2.0.0', nuspec: nuspec('MIT'), files: [] },
  ];
}

const build = (change = () => {}, config = CONFIG) => {
  const value = packages();
  change(value);
  return buildNotices(value, readText, config);
};
const find = (value, id) => value.find((p) => p.id === id);
/** The lines of one package block of the notices. */
const block = (text, name) => text.split('\n\n').find((part) => part.startsWith(`${name}\n`));
/** Number and body of the text titled `title`. */
const textNamed = (text, title) => {
  const match = text.match(new RegExp(`\\[(\\d+)\\] ${title.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')}\\n`));
  assert.ok(match, `text "${title}" is missing`);
  return Number(match[1]);
};

test('passes a fitting set and lists the packages by id, ignoring case', () => {
  const { text, errors } = build();
  assert.deepEqual(errors, []);
  const order = ['alpha 3.1.0', 'Logs 4.0.0', 'Ms.Package 10.0.0', 'Own.Licence 1.0.0', 'Pg 10.0.3', 'Ui 2.0.0', 'Zeta 1.0.0'];
  const positions = order.map((name) => text.indexOf(`\n${name}\n`));
  assert.ok(positions.every((p) => p > 0), 'every package is listed');
  assert.deepEqual([...positions].sort((a, b) => a - b), positions);
  assert.match(text, /Packages \(7\)/);
});

test('prints the standard MIT text once, without its placeholder, and the copyright in each block', () => {
  const { text } = build();
  const mit = textNamed(text, 'MIT licence');
  assert.equal(text.split('Permission is hereby granted.').length, 2, 'the MIT text is printed once');
  assert.ok(!text.includes('<copyright holders>'));
  assert.match(block(text, 'alpha 3.1.0'), new RegExp(`Copyright: © Alpha\\n.*\\n  Texts: \\[${mit}\\]`));
  assert.match(block(text, 'Zeta 1.0.0'), new RegExp(`Copyright: © Zeta\\n.*\\n  Texts: \\[${mit}\\]`));
  assert.match(text, /Applies to: alpha 3\.1\.0, Ms\.Package 10\.0\.0, Ui 2\.0\.0, Zeta 1\.0\.0\n/);
});

test('uses a licence file of the package instead of the standard text', () => {
  const { text } = build();
  const own = textNamed(text, 'LICENSE.txt of Own.Licence');
  assert.match(block(text, 'Own.Licence 1.0.0'), new RegExp(`Texts: \\[${own}\\]$`));
});

test('adds a notices file to the standard text when the package has no licence file', () => {
  const { text } = build();
  const mit = textNamed(text, 'MIT licence');
  const notices = textNamed(text, 'THIRD-PARTY-NOTICES.TXT of Ms.Package');
  assert.match(block(text, 'Ms.Package 10.0.0'), new RegExp(`Texts: \\[${mit}\\], \\[${notices}\\]$`));
});

test('prints a text shared by several packages once', () => {
  const same = { name: 'THIRD-PARTY-NOTICES.TXT', text: 'Notices of embedded code\r\n\r\n', isLicence: false };
  const { text, errors } = build((value) => find(value, 'Zeta').files.push(same));
  assert.deepEqual(errors, []);
  assert.equal(text.split('Notices of embedded code').length, 2);
  assert.match(text, /Applies to: Ms\.Package 10\.0\.0, Zeta 1\.0\.0\n/);
});

test('names the authors when the package states no copyright', () => {
  const { text } = build();
  assert.match(block(text, 'Logs 4.0.0'), /Licence: Apache-2\.0\n {2}Authors: Someone\n/);
});

test('uses the own text of a package in PACKAGE_TEXTS', () => {
  const { text } = build();
  const pg = textNamed(text, 'PostgreSQL licence of Pg');
  assert.match(block(text, 'Pg 10.0.3'), new RegExp(`Licence: PostgreSQL\\n.*\\n.*\\n  Texts: \\[${pg}\\]$`));
});

test('adds the reviewed entry and its licence for embedded code', () => {
  const { text } = build();
  const notice = textNamed(text, 'Notices of widget 1.0');
  const apache = textNamed(text, 'Apache-2.0 licence');
  assert.match(block(text, 'Ui 2.0.0'), new RegExp(`Embeds: widget 1\\.0 \\(Apache-2\\.0\\), texts \\[${notice}\\], \\[${apache}\\]\\n`));
});

test('fails on a licence outside the allowlist', () => {
  const { errors } = build((value) => (find(value, 'Zeta').nuspec.licenceExpression = 'GPL-3.0-only'));
  assert.equal(errors.length, 1);
  assert.match(errors[0], /^Zeta 1\.0\.0: licence "GPL-3\.0-only" is not on the allowlist/);
});

test('fails on a compound expression, even of allowed licences', () => {
  const { errors } = build((value) => (find(value, 'Zeta').nuspec.licenceExpression = 'MIT OR Apache-2.0'));
  assert.match(errors.join('\n'), /Zeta 1\.0\.0: licence "MIT OR Apache-2\.0" is not on the allowlist/);
});

test('fails on a package without a licence expression', () => {
  const { errors } = build((value) => (find(value, 'Zeta').nuspec.licenceExpression = null));
  assert.match(errors.join('\n'), /Zeta 1\.0\.0: no licence expression in its \.nuspec/);
});

test('fails when the licence of a package in PACKAGE_TEXTS changes', () => {
  const { errors } = build((value) => (find(value, 'Pg').nuspec.licenceExpression = 'MIT'));
  assert.match(errors.join('\n'), /Pg 10\.0\.3: licence is now "MIT", PACKAGE_TEXTS says "PostgreSQL"/);
});

test('fails on a new version of a package with embedded code until its entry is checked', () => {
  const { errors } = build((value) => (find(value, 'Ui').version = '3.0.0'));
  assert.equal(errors.length, 1);
  assert.match(errors[0], /Ui 3\.0\.0: the entry for the code it embeds \(widget 1\.0\) was checked for 2\.0\.0/);
});

test('fails on entries for packages the API no longer ships', () => {
  const { errors } = build((value) => value.splice(value.indexOf(find(value, 'Pg')), 1));
  assert.deepEqual(errors, ['PACKAGE_TEXTS names Pg, which the API no longer ships: remove the entry']);
  const { errors: embedded } = build((value) => value.splice(value.indexOf(find(value, 'Ui')), 1));
  assert.deepEqual(embedded, ['EMBEDDED names Ui, which the API no longer ships: remove the entry']);
});

test('fails on a package missing from the package folder', () => {
  const { errors } = build((value) => (find(value, 'Zeta').nuspec = null));
  assert.deepEqual(errors, ['Zeta 1.0.0: not in the NuGet package folder (restore the API first)']);
});

test('fails when the MIT text loses its copyright placeholder', () => {
  const texts = { ...TEXTS, 'LICENSES/MIT.txt': 'MIT License\n\nPermission is hereby granted.\n' };
  const { errors } = buildNotices(packages(), (file) => texts[file], CONFIG);
  assert.match(errors.join('\n'), /LICENSES\/MIT\.txt: the copyright placeholder line was not found/);
});

test('reads the packages of a deps.json, without projects', () => {
  const deps = {
    libraries: {
      'Serilog/4.4.0': { type: 'package' },
      'API.ProcureAccess/1.0.0': { type: 'project' },
      'Asp.Versioning.Http/10.2.1': { type: 'package' },
    },
  };
  assert.deepEqual(packagesOf(deps), [
    { id: 'Asp.Versioning.Http', version: '10.2.1' },
    { id: 'Serilog', version: '4.4.0' },
  ]);
});

test('reads licence, copyright, authors and project of a .nuspec', () => {
  const xml = `<?xml version="1.0"?><package><metadata>
    <id>X</id><authors>A &amp; B</authors>
    <license type="expression">MIT</license><licenseUrl>https://licenses.nuget.org/MIT</licenseUrl>
    <copyright>&#169; X &lt;team&gt;</copyright>
    <repository type="git" url="https://github.com/x/x" commit="abc" />
  </metadata></package>`;
  assert.deepEqual(readNuspec(xml), {
    licenceExpression: 'MIT',
    licenceFile: null,
    copyright: '© X <team>',
    authors: 'A & B',
    project: 'https://github.com/x/x',
  });
  const file = readNuspec('<license type="file">docs/LICENSE.md</license><projectUrl>https://x.org</projectUrl>');
  assert.equal(file.licenceExpression, null);
  assert.equal(file.licenceFile, 'docs/LICENSE.md');
  assert.equal(file.project, 'https://x.org');
});

test('reads the licence and notice files of a package folder, the nuspec licence file first', () => {
  const dir = mkdtempSync(join(tmpdir(), 'api-notices-test-'));
  try {
    const folder = join(dir, 'some.package', '1.2.3');
    mkdirSync(join(folder, 'docs'), { recursive: true });
    writeFileSync(join(folder, 'some.package.nuspec'), '<license type="file">docs/COPYING.txt</license>');
    writeFileSync(join(folder, 'docs', 'COPYING.txt'), 'own licence');
    writeFileSync(join(folder, 'THIRD-PARTY-NOTICES.TXT'), 'notices');
    writeFileSync(join(folder, 'NOTICE'), 'notice');
    writeFileSync(join(folder, 'README.md'), 'readme');
    const read = readPackage(dir, { id: 'Some.Package', version: '1.2.3' });
    assert.deepEqual(read.files, [
      { name: 'docs/COPYING.txt', text: 'own licence', isLicence: true },
      { name: 'NOTICE', text: 'notice', isLicence: false },
      { name: 'THIRD-PARTY-NOTICES.TXT', text: 'notices', isLicence: false },
    ]);
    assert.equal(readPackage(dir, { id: 'Missing', version: '1.0.0' }), null);
  } finally {
    rmSync(dir, { recursive: true, force: true });
  }
});

test("the repository's licence texts and reviewed entries load", () => {
  const readRepo = (file) => readFileSync(join(ROOT, file), 'utf8');
  const configured = [
    ...Object.entries(PACKAGE_TEXTS).map(([id, own]) => ({ id, version: '1.0.0', nuspec: nuspec(own.licence), files: [] })),
    ...Object.entries(EMBEDDED).map(([id, entry]) => ({ id, version: entry.version, nuspec: nuspec('MIT'), files: [] })),
    ...Object.keys(STANDARD_TEXTS).map((licence) => ({ id: `Std.${licence}`, version: '1.0.0', nuspec: nuspec(licence), files: [] })),
  ];
  const { errors } = buildNotices(configured, readRepo);
  assert.deepEqual(errors, []);
});

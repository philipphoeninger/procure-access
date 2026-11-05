// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Self-test of check-invariants.mjs (#185): each rule of TESTING §8.3 shown to fail on its case
// and to pass on a clean tree, with the cases of S2-09 (docs/sprints/sprint_02/S2-09_gate_tightening.md §4.1).
// The gate reads the repository it lives in, so every case copies it into a small fake repository
// in a temp folder and runs it there; the gate itself stays unchanged.
// Node's built-in test runner, no dependencies: node --test scripts/check-invariants.test.mjs
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { copyFileSync, mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';

const GATE = join(dirname(fileURLToPath(import.meta.url)), 'check-invariants.mjs');

const catalogue = (rows, header = '| ID | Invariant | Level | Step | Status |') =>
  ['# Testing', '', '## 4. Invariant catalogue', '', header, '|---|---|---|---|---|', ...rows, '', '## 5. Next', ''].join('\n');

const ROWS = [
  '| ARC-06 | A .NET rule | U | 0 | active |',
  '| A11Y-01 | A Playwright rule | E2E | 0 | active |',
  '| CAT-10 | A Vitest rule | FE | 0 | active |',
  '| A11Y-07 | A manual check | M | 5 | planned |',
  '| SIG-01 | A later rule | U | 4 | planned |',
];

const UNIT = 'tests/ProcureAccess.Tests.Unit/RuleTests.cs';
const SHARED = 'tests/ProcureAccess.Tests.Shared';
const J1 = 'e2e/journeys/j1.spec.ts';
const CLIENT_SPEC = 'CLIENT/CLIENT.ProcureAccess/src/app/rule.spec.ts';

const csTest = (attributes, { classAttributes = '' } = {}) =>
  ['namespace ProcureAccess.Tests.Unit;', '', `${classAttributes}public class RuleTests`, '{',
    ...attributes.map((a) => `    ${a}`), '    public void Rule_Holds_Passes() { }', '}', ''].join('\n');

const UNIT_OK = csTest(['[Fact(DisplayName = "Rule_Holds_Passes INV:ARC-06")]', '[Trait("Invariant", "ARC-06")]']);

const j1 = (...body) => ["import { test } from '@playwright/test';", '', ...body, ''].join('\n');
const J1_OK = j1("test.describe('J1 configurator', () => {", "  test('all states pass axe INV:A11Y-01', async () => {});", '});');

// The clean tree: one counted test per active invariant, a test project and a non-test project.
const BASE = {
  'docs/TESTING_v1.md': catalogue(ROWS),
  'tests/ProcureAccess.Tests.Unit/ProcureAccess.Tests.Unit.csproj': '<Project Sdk="Microsoft.NET.Sdk"></Project>\n',
  [UNIT]: UNIT_OK,
  [`${SHARED}/ProcureAccess.Tests.Shared.csproj`]:
    '<Project Sdk="Microsoft.NET.Sdk">\n  <PropertyGroup>\n    <IsTestProject>false</IsTestProject>\n  </PropertyGroup>\n</Project>\n',
  [`${SHARED}/TestData.cs`]: 'namespace ProcureAccess.Tests.Shared;\n\npublic static class TestData { }\n',
  [J1]: J1_OK,
  [CLIENT_SPEC]: "describe('criteria filter', () => {\n  it('keeps the expected set INV:CAT-10', () => {});\n});\n",
};

/** Runs the gate on BASE with `changes` (path → content; null deletes the file). */
function gate(changes = {}) {
  const root = mkdtempSync(join(tmpdir(), 'invariant-gate-'));
  try {
    const files = { ...BASE, ...changes };
    for (const [path, content] of Object.entries(files)) {
      if (content === null) continue;
      mkdirSync(dirname(join(root, path)), { recursive: true });
      writeFileSync(join(root, path), content);
    }
    mkdirSync(join(root, 'docs'), { recursive: true });
    mkdirSync(join(root, 'scripts'));
    copyFileSync(GATE, join(root, 'scripts', 'check-invariants.mjs'));
    // Plain messages also in CI: GITHUB_ACTIONS=true would turn them into annotations.
    const run = spawnSync(process.execPath, [join(root, 'scripts', 'check-invariants.mjs')], {
      encoding: 'utf8',
      env: { ...process.env, GITHUB_ACTIONS: 'false' },
    });
    return { passed: run.status === 0, status: run.status, output: `${run.stdout}${run.stderr}` };
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

/** Number of counted tests of `id` in the gate's table, or undefined if it is not listed. */
function counted(output, id) {
  const match = new RegExp(`^${id}\\s+\\S+\\s+(\\d+)`, 'm').exec(output);
  return match ? Number(match[1]) : undefined;
}

const fails = (result, message) => {
  assert.equal(result.status, 1, result.output);
  assert.match(result.output, message);
};

const missing = (id) => new RegExp(`${id} is active but no test that runs is tagged INV:${id}`);

// ---------------------------------------------------------------------------------------------
// Control

test('passes the clean tree and counts one test per active invariant', () => {
  const result = gate();
  assert.equal(result.status, 0, result.output);
  assert.match(result.output, /Invariant catalogue: docs[/\\]TESTING_v1\.md/);
  assert.match(result.output, /5 invariants in the catalogue, 3 active/);
  assert.deepEqual(['ARC-06', 'A11Y-01', 'CAT-10'].map((id) => counted(result.output, id)), [1, 1, 1]);
  assert.equal(counted(result.output, 'SIG-01'), undefined);
  assert.doesNotMatch(result.output, /^[!✗]/m);
  assert.match(result.output, /Invariant gate passed\./);
});

// ---------------------------------------------------------------------------------------------
// Step 1: the catalogue

test('reads the highest TESTING version, compared as a number', () => {
  const result = gate({ 'docs/TESTING_v9.md': catalogue(ROWS), 'docs/TESTING_v10.md': catalogue([...ROWS, '| OPS-01 | New | I | 1 | active |']) });
  assert.match(result.output, /Invariant catalogue: docs[/\\]TESTING_v10\.md/);
  fails(result, missing('OPS-01'));
});

test('fails without a TESTING document or without catalogue rows', () => {
  fails(gate({ 'docs/TESTING_v1.md': null }), /no docs\/TESTING_v<n>\.md found/);
  fails(gate({ 'docs/TESTING_v1.md': '# Testing\n\nNo catalogue here.\n' }), /has no catalogue rows/);
});

test('H1, H2: fails on a catalogue table whose header drifted', () => {
  const extraColumn = catalogue(ROWS, '| ID | Invariant | Level | Step | Status | Note |');
  const renamed = catalogue(ROWS, '| ID | Invariant | Level | Step | State |');
  for (const document of [extraColumn, renamed]) {
    const result = gate({ 'docs/TESTING_v1.md': `${catalogue(ROWS.slice(0, 1))}\n${document}` });
    fails(result, /docs[/\\]TESTING_v1\.md:\d+: catalogue table header is not \| ID \| Invariant \| Level \| Step \| Status \|/);
  }
});

test('fails on a malformed row, a duplicate ID or an unknown status', () => {
  fails(gate({ 'docs/TESTING_v1.md': catalogue([...ROWS, '| OPS-01 | Missing columns | I |']) }), /catalogue row does not have the form/);
  fails(gate({ 'docs/TESTING_v1.md': catalogue([...ROWS, '| ARC-06 | Again | U | 0 | active |']) }), /ARC-06 appears twice in the catalogue/);
  fails(gate({ 'docs/TESTING_v1.md': catalogue([...ROWS, '| OPS-01 | Done? | I | 1 | done |']) }), /OPS-01 has the unknown status "done"/);
});

// ---------------------------------------------------------------------------------------------
// Step 2: test files only

test('F1: a tag outside a test file does not count (e2e support file, client source, node_modules)', () => {
  const tagged = "test('looks like a test INV:A11Y-01', async () => {});\n";
  const result = gate({
    [J1]: j1("test('no tag', async () => {});"),
    'e2e/support/axe-suppressions.ts': tagged,
    'CLIENT/CLIENT.ProcureAccess/src/app/rule.ts': "it('not a spec INV:CAT-10', () => {});\n",
    'CLIENT/CLIENT.ProcureAccess/node_modules/pkg/src/a.spec.ts': "it('dependency INV:CAT-10', () => {});\n",
    [CLIENT_SPEC]: "it('no tag', () => {});\n",
  });
  fails(result, missing('A11Y-01'));
  assert.match(result.output, missing('CAT-10'));
});

test('F2: a tagged test in a project with <IsTestProject>false does not count', () => {
  const result = gate({
    [UNIT]: csTest(['[Fact]']),
    [`${SHARED}/RuleTests.cs`]: UNIT_OK,
  });
  fails(result, missing('ARC-06'));
});

// ---------------------------------------------------------------------------------------------
// Step 2: TypeScript tests that run

const TS_CASES = [
  ['R5: test.describe.skip around the test', j1("test.describe.skip('J1', () => {", "  test('x INV:A11Y-01', async () => {});", '});'), /the test or its group is skipped/],
  ['R6: test.fixme', j1("test.fixme('x INV:A11Y-01', async () => {});"), /the test or its group is skipped/],
  ['test.todo and it.skip', j1("test.todo('x INV:A11Y-01');", "it.skip('y INV:A11Y-01', () => {});"), /the test or its group is skipped/],
  ['R7: unconditional test.skip() in the group', j1("test.describe('J1', () => {", '  test.skip();', "  test('x INV:A11Y-01', async () => {});", '});'), /the test or its group is skipped/],
  ['test.skip(true, …) in the group', j1("test.describe('J1', () => {", "  test.skip(true, 'later');", "  test('x INV:A11Y-01', async () => {});", '});'), /the test or its group is skipped/],
  ['R10: test.skip() at file level', j1('test.skip();', "test('x INV:A11Y-01', async () => {});"), /the test or its group is skipped/],
  ['R8: xtest', j1("xtest('x INV:A11Y-01', async () => {});"), /the test or its group is skipped/],
  ['R8: a comment', j1('// INV:A11Y-01', "test('x', async () => {});"), /INV:A11Y-01 is not counted \(comment\)/],
  ['R8: a test.step title', j1("test('x', async () => { await test.step('INV:A11Y-01', async () => {}); });"), /not in the title of test\(\), it\(\) or describe\(\)/],
  ['test.skipIf (conditional modifiers count as skipped)', j1("test.skipIf(process.env.CI)('x INV:A11Y-01', async () => {});"), /the test or its group is skipped/],
];

for (const [name, source, reason] of TS_CASES) {
  test(`TypeScript: ${name} does not count`, () => {
    const result = gate({ [J1]: source });
    fails(result, missing('A11Y-01'));
    assert.match(result.output, reason);
  });
}

test('TypeScript: R9 control: a conditional test.skip(…) inside the group, .only and .serial still count', () => {
  const result = gate({
    [J1]: j1(
      "test.describe.serial('J1', () => {",
      "  test.skip(browserName === 'webkit', 'known limit: decided at runtime');",
      "  test('x INV:A11Y-01', async () => {});",
      "  test.only('y INV:A11Y-01', async () => {});",
      '});',
    ),
  });
  assert.equal(result.status, 0, result.output);
  assert.equal(counted(result.output, 'A11Y-01'), 2);
});

// ---------------------------------------------------------------------------------------------
// Step 2: .NET tests that run, with the Invariant trait

const CS_CASES = [
  ['R1: tag only in a comment', csTest(['// INV:ARC-06', '[Fact]']), /INV:ARC-06 is not counted \(comment\)/],
  ['R2: Skip', csTest(['[Fact(Skip = "flaky", DisplayName = "x INV:ARC-06")]', '[Trait("Invariant", "ARC-06")]']), /the test is skipped or explicit/],
  ['R3: Explicit = true', csTest(['[Fact(Explicit = true, DisplayName = "x INV:ARC-06")]', '[Trait("Invariant", "ARC-06")]']), /the test is skipped or explicit/],
  ['SkipWhen', csTest(['[Theory(DisplayName = "x INV:ARC-06", SkipWhen = nameof(Ci))]', '[Trait("Invariant", "ARC-06")]']), /the test is skipped or explicit/],
  ['R4: tag in [InlineData] instead of the DisplayName', csTest(['[Theory]', '[InlineData("INV:ARC-06")]', '[Trait("Invariant", "ARC-06")]']), /not in the DisplayName of a \[Fact\] or \[Theory\]/],
];

for (const [name, source, reason] of CS_CASES) {
  test(`.NET: ${name} does not count`, () => {
    const result = gate({ [UNIT]: source });
    fails(result, missing('ARC-06'));
    assert.match(result.output, reason);
  });
}

test('.NET: control: Explicit = false and an attribute in one section still count', () => {
  const result = gate({ [UNIT]: csTest(['[Fact(Explicit = false, DisplayName = $"x INV:ARC-06"), Trait("Invariant", "ARC-06")]']) });
  assert.equal(result.status, 0, result.output);
  assert.equal(counted(result.output, 'ARC-06'), 1);
});

test('.NET: T1: the tag needs the trait on the same method', () => {
  fails(gate({ [UNIT]: csTest(['[Fact(DisplayName = "x INV:ARC-06")]']) }),
    /INV:ARC-06 needs \[Trait\("Invariant", "ARC-06"\)\] on the same test method/);
});

test('.NET: T2: a trait for another ID fails both ways', () => {
  const result = gate({ [UNIT]: csTest(['[Fact(DisplayName = "x INV:ARC-06")]', '[Trait("Invariant", "ARC-05")]']) });
  fails(result, /INV:ARC-06 needs \[Trait\("Invariant", "ARC-06"\)\]/);
  assert.match(result.output, /\[Trait\("Invariant", "ARC-05"\)\] needs a \[Fact\] or \[Theory\] on the same method with INV:ARC-05/);
});

test('.NET: T3: the trait on the class does not count for the method', () => {
  const result = gate({ [UNIT]: csTest(['[Fact(DisplayName = "x INV:ARC-06")]'], { classAttributes: '[Trait("Invariant", "ARC-06")]\n' }) });
  fails(result, /INV:ARC-06 needs \[Trait\("Invariant", "ARC-06"\)\]/);
  assert.match(result.output, /\[Trait\("Invariant", "ARC-06"\)\] needs a \[Fact\] or \[Theory\] on the same method/);
});

test('.NET: T4: the trait value must be a string literal', () => {
  fails(gate({ [UNIT]: csTest(['[Fact(DisplayName = "x INV:ARC-06")]', '[Trait("Invariant", Ids.Arc06)]']) }),
    /the value of \[Trait\("Invariant", …\)\] must be a string literal/);
});

// ---------------------------------------------------------------------------------------------
// Manual invariants: protocol files

const withProtocol = (reference, level = 'M') =>
  catalogue([...ROWS.filter((row) => !row.startsWith('| A11Y-07')), `| A11Y-07 | A manual check | ${level} | 5 | active — ${reference} |`]);

const PROTOCOL_CASES = [
  ['P1: the folder itself', 'docs/a11y-reports/', /is not a Markdown file in docs[/\\]a11y-reports\//],
  ['P2: a Markdown file outside the folder', 'docs/sprints/S2-08_x.md', /is not a Markdown file in docs[/\\]a11y-reports\//],
  ['P3: a path that leaves the folder with ..', 'docs/a11y-reports/../sprints/S2-08_x.md', /is not a Markdown file in docs[/\\]a11y-reports\//],
  ['P4: a file that is not Markdown', 'docs/a11y-reports/.gitkeep', /is not a Markdown file in docs[/\\]a11y-reports\//],
  ['P5: a Markdown file that does not exist', 'docs/a11y-reports/2027-01-01_missing.md', /does not exist/],
];

for (const [name, reference, message] of PROTOCOL_CASES) {
  test(`protocol: ${name} fails`, () => {
    const result = gate({
      'docs/TESTING_v1.md': withProtocol(reference),
      'docs/sprints/S2-08_x.md': '# Not a protocol\n',
      'docs/a11y-reports/.gitkeep': '',
    });
    assert.ok(result.output.includes(`A11Y-07: protocol ${reference} `), result.output);
    fails(result, message);
  });
}

test('protocol: only a manual (M) invariant may use one', () => {
  const result = gate({ 'docs/TESTING_v1.md': withProtocol('docs/a11y-reports/2027-01-01_step-5.md', 'E2E'), 'docs/a11y-reports/2027-01-01_step-5.md': '# Protocol\n' });
  fails(result, /A11Y-07: only manual \(M\) invariants may be satisfied by a protocol reference/);
});

test('protocol: P6 control: an existing Markdown file in docs/a11y-reports/ passes', () => {
  const result = gate({ 'docs/TESTING_v1.md': withProtocol('docs/a11y-reports/2027-01-01_step-5.md'), 'docs/a11y-reports/2027-01-01_step-5.md': '# Protocol\n' });
  assert.equal(result.status, 0, result.output);
  assert.match(result.output, /^A11Y-07\s+M\s+0\s+docs\/a11y-reports\/2027-01-01_step-5\.md$/m);
});

// ---------------------------------------------------------------------------------------------
// Step 3: typo protection

test('warns, but passes, on a tag with an unknown or a non-active ID', () => {
  const result = gate({
    [CLIENT_SPEC]: "describe('criteria filter', () => {\n  it('keeps the expected set INV:CAT-10', () => {});\n  it('typo INV:CAT-99', () => {});\n  it('later INV:SIG-01', () => {});\n});\n",
  });
  assert.equal(result.status, 0, result.output);
  assert.match(result.output, /INV:SIG-01 is not active in the catalogue/);
  assert.match(result.output, /INV:CAT-99 does not exist in the catalogue/);
});

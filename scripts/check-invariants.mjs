// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Invariant gate (TESTING §8.3, CI stage 7). Node, no dependencies: node scripts/check-invariants.mjs
//
// 1. Reads the invariant catalogue from the highest docs/TESTING_v<n>.md (risk R4 of sprint 01:
//    the documents are versioned, there is no docs/TESTING.md). A table that starts like a
//    catalogue table but has another header fails the gate instead of being skipped.
// 2. Searches the test files for INV:<ID> tags (TESTING §8.1) and counts a tag only where it names
//    a test that runs:
//    - .NET: in the DisplayName of a [Fact] or [Theory] without Skip or Explicit; the same method
//      must carry [Trait("Invariant", "<ID>")], and every such trait needs the matching tag.
//    - TypeScript: in the title of test(), it() or describe(), not skipped (.skip, .fixme, .todo,
//      .fails, x…, …If) and not inside a skipped group or a group with an unconditional test.skip().
//    Tags in comments, in other strings or in skipped tests are reported, not counted.
// 3. Fails if an active invariant has no counted test; warns on tags with an unknown or
//    non-active ID (typo protection).
// 4. Prints the active invariants with the number of counted tests.
//
// Manual invariants (level M) are satisfied by a protocol reference in the status column,
// e.g. "active — docs/a11y-reports/2027-02-10_step-5.md"; it must be a Markdown file in that folder.
//
// The checks read the source text; they cannot see skips decided at runtime (Assert.Skip,
// test.skip(condition) inside a test) or tests a CI stage filters out (TESTING §8.3, known limits).

import { existsSync, readFileSync, readdirSync, statSync } from 'node:fs';
import { dirname, join, relative, resolve, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = join(dirname(fileURLToPath(import.meta.url)), '..');
const DOCS = join(ROOT, 'docs');
const PROTOCOL_DIR = join(DOCS, 'a11y-reports');

// Catalogue tables (TESTING §4) have exactly this header. A table whose header starts with
// "| ID | Invariant |" is meant to be one, so a different header is an error, not a skip.
const CATALOGUE_HEADER = /^\|\s*ID\s*\|\s*Invariant\s*\|\s*Level\s*\|\s*Step\s*\|\s*Status\s*\|\s*$/;
const CATALOGUE_START = /^\|\s*ID\s*\|\s*Invariant\s*\|/i;
const ID_PATTERN = /^[A-Z][A-Z0-9]*-\d+$/;
const STATUSES = new Set(['planned', 'active', 'conditional']);
const ACTIVE_WITH_PROTOCOL = /^active\s+—\s+(\S+)$/;
const TAG_PATTERN = /INV:([A-Z][A-Z0-9]*-\d+)/g;

// Where tests live (TESTING §8.3 step 2): test files only. Generated output and dependencies are
// skipped, and so are .NET projects that declare <IsTestProject>false (ProcureAccess.Tests.Shared).
const SEARCH = [
  { dir: 'tests', language: 'cs', accept: (path) => path.endsWith('.cs') && !inNonTestProject(path) },
  { dir: 'CLIENT', language: 'ts', accept: (path) => path.endsWith('.spec.ts') && path.split(sep).includes('src') },
  { dir: 'e2e', language: 'ts', accept: (path) => path.endsWith('.spec.ts') }, // Playwright's testMatch
];
const SKIP_DIRS = new Set([
  'node_modules', 'bin', 'obj', 'dist', 'coverage', 'test-results', 'playwright-report', 'blob-report',
]);

// xUnit attributes that declare a test, and named arguments that keep it from running by default.
const CS_TEST_ATTRIBUTES = new Set(['Fact', 'Theory']);
const CS_NOT_RUN_ARGUMENTS = new Set(['Skip', 'SkipWhen', 'SkipUnless', 'Explicit']);

// Vitest and Playwright: test functions, modifiers that keep the test, and modifiers that skip it.
// Unknown modifiers (test.step, test.use, …) mean the string is not a test title.
const TS_TEST_ROOTS = new Set(['test', 'it', 'describe', 'fit', 'fdescribe']);
const TS_SKIPPED_ROOTS = new Set(['xit', 'xtest', 'xdescribe']);
const TS_KEEP = new Set(['describe', 'only', 'serial', 'parallel', 'concurrent', 'sequential', 'shuffle', 'each', 'for']);
const TS_SKIP = new Set(['skip', 'fixme', 'todo', 'fails', 'fail', 'skipIf', 'runIf']);

const inActions = process.env.GITHUB_ACTIONS === 'true';
let failed = false;

function error(message) {
  console.error(inActions ? `::error title=Invariant gate::${message}` : `✗ ${message}`);
  failed = true;
}

function warn(message) {
  console.warn(inActions ? `::warning title=Invariant gate::${message}` : `! ${message}`);
}

function findTestingDocument() {
  const versions = readdirSync(DOCS)
    .map((name) => ({ name, match: /^TESTING_v(\d+)\.md$/.exec(name) }))
    .filter(({ match }) => match)
    .map(({ name, match }) => ({ name, version: Number(match[1]) }))
    .sort((a, b) => b.version - a.version); // numeric: v10 before v9
  return versions.length > 0 ? join(DOCS, versions[0].name) : null;
}

// Splits a Markdown table row; "\|" inside a cell is part of the text, not a column border.
function cells(line) {
  return line.trim().replace(/^\|/, '').replace(/\|$/, '').split(/(?<!\\)\|/).map((cell) => cell.trim());
}

function parseCatalogue(file) {
  const invariants = new Map();
  let inTable = false;
  readFileSync(file, 'utf8').split(/\r?\n/).forEach((line, index) => {
    const where = `${relative(ROOT, file)}:${index + 1}`;
    if (CATALOGUE_HEADER.test(line)) {
      inTable = true;
      return;
    }
    if (CATALOGUE_START.test(line)) {
      error(`${where}: catalogue table header is not | ID | Invariant | Level | Step | Status |; its rows would not be checked`);
      inTable = false;
      return;
    }
    if (!inTable) return;
    if (!line.trim().startsWith('|')) {
      inTable = false;
      return;
    }
    if (/^\|[\s|:-]+\|$/.test(line.trim())) return; // separator row

    const row = cells(line);
    const [id, , level, , status] = row;
    if (row.length !== 5 || !ID_PATTERN.test(id)) {
      error(`${where}: catalogue row does not have the form | ID | Invariant | Level | Step | Status |`);
      return;
    }
    if (invariants.has(id)) {
      error(`${where}: ${id} appears twice in the catalogue`);
      return;
    }
    const protocol = ACTIVE_WITH_PROTOCOL.exec(status)?.[1] ?? null;
    if (!STATUSES.has(status) && !protocol) {
      error(`${where}: ${id} has the unknown status "${status}"`);
      return;
    }
    invariants.set(id, { level, active: status === 'active' || protocol !== null, protocol });
  });
  return invariants;
}

// A protocol must be a Markdown file inside docs/a11y-reports/ (TESTING §7.2, §8.3).
function protocolProblem(protocol) {
  const path = resolve(ROOT, protocol);
  if (!path.startsWith(PROTOCOL_DIR + sep) || !path.endsWith('.md')) {
    return `is not a Markdown file in ${relative(ROOT, PROTOCOL_DIR)}/`;
  }
  if (!existsSync(path) || !statSync(path).isFile()) return 'does not exist';
  return null;
}

function* walk(dir) {
  if (!existsSync(dir)) return;
  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    if (entry.isDirectory()) {
      if (!SKIP_DIRS.has(entry.name) && !entry.name.startsWith('.')) yield* walk(join(dir, entry.name));
    } else if (entry.isFile()) {
      yield join(dir, entry.name);
    }
  }
}

// Folders under tests/ whose project file says it holds no tests (builders and fixtures only).
const nonTestProjects = new Set(
  [...walk(join(ROOT, 'tests'))]
    .filter((file) => file.endsWith('.csproj'))
    .filter((file) => /<IsTestProject>\s*false\s*<\/IsTestProject>/i.test(readFileSync(file, 'utf8')))
    .map((file) => relative(ROOT, dirname(file)) + sep),
);

function inNonTestProject(path) {
  return [...nonTestProjects].some((project) => path.startsWith(project));
}

// ---------------------------------------------------------------------------------------------
// Lexer: just enough C# and TypeScript to tell code, strings and comments apart. Tokens carry the
// line they start on; strings keep their raw content (interpolations included), so a tag in
// `INV:X ${name}` or $"INV:X {name}" is still seen.

const TS_REGEX_AFTER_WORD = new Set([
  'return', 'typeof', 'instanceof', 'in', 'of', 'new', 'delete', 'void', 'throw', 'case', 'do', 'else',
  'yield', 'await',
]);

function lex(source, language) {
  const tokens = [];
  const comments = [];
  let i = 0;
  let line = 1;

  const advanceTo = (end) => {
    for (let k = i; k < end; k++) if (source[k] === '\n') line++;
    i = end;
  };
  const push = (kind, start, end, value, startLine) => tokens.push({ kind, value, line: startLine, start, end });

  // Scans a quoted string from `start` (at the opening quote); returns the index after it.
  const quoted = (start, quote, { escapes = true, doubled = false, holes = false } = {}) => {
    let k = start + 1;
    while (k < source.length) {
      const c = source[k];
      if (escapes && c === '\\') k += 2;
      else if (doubled && c === quote && source[k + 1] === quote) k += 2;
      else if (c === quote) return k + 1;
      else if (holes && c === '{' && source[k + 1] === '{') k += 2;
      else if (holes && c === '{') k = balanced(k);
      else k++;
    }
    return k;
  };
  // Skips a {...} or ${...} hole including nested strings; returns the index after the closing brace.
  const balanced = (open) => {
    let depth = 0;
    let k = open;
    while (k < source.length) {
      const c = source[k];
      if (c === '{') depth++;
      else if (c === '}' && --depth === 0) return k + 1;
      else if (c === '"' || c === "'") { k = quoted(k, c); continue; }
      else if (c === '`' && language === 'ts') { k = template(k); continue; }
      k++;
    }
    return k;
  };
  const template = (start) => {
    let k = start + 1;
    while (k < source.length) {
      const c = source[k];
      if (c === '\\') k += 2;
      else if (c === '`') return k + 1;
      else if (c === '$' && source[k + 1] === '{') k = balanced(k + 1);
      else k++;
    }
    return k;
  };
  const regexAllowed = () => {
    const previous = tokens.at(-1);
    if (!previous) return true;
    if (previous.kind === 'name') return TS_REGEX_AFTER_WORD.has(previous.value);
    if (previous.kind === 'punct') return ![')', ']', '}'].includes(previous.value);
    return false;
  };

  while (i < source.length) {
    const c = source[i];
    const next = source[i + 1];
    const startLine = line;
    const start = i;

    if (c === '\n' || c === ' ' || c === '\t' || c === '\r') { advanceTo(i + 1); continue; }

    if (c === '/' && next === '/') {
      let end = source.indexOf('\n', i);
      if (end === -1) end = source.length;
      comments.push({ value: source.slice(i, end), line: startLine });
      advanceTo(end);
      continue;
    }
    if (c === '/' && next === '*') {
      let end = source.indexOf('*/', i + 2);
      end = end === -1 ? source.length : end + 2;
      comments.push({ value: source.slice(i, end), line: startLine });
      advanceTo(end);
      continue;
    }

    if (language === 'cs') {
      // Raw strings ("""…"""), verbatim (@"…"), interpolated ($"…", $@"…", @$"…") and regular strings.
      const prefix = /^(\$+@?|@\$*)?(?=")/.exec(source.slice(i, i + 4))?.[0];
      if (prefix !== undefined) {
        const quoteStart = i + prefix.length;
        const raw = /^"{3,}/.exec(source.slice(quoteStart))?.[0];
        let end;
        if (raw) {
          const close = source.indexOf(raw, quoteStart + raw.length);
          end = close === -1 ? source.length : close + raw.length;
        } else {
          end = quoted(quoteStart, '"', {
            escapes: !prefix.includes('@'),
            doubled: prefix.includes('@'),
            holes: prefix.includes('$'),
          });
        }
        push('string', start, end, source.slice(start, end), startLine);
        advanceTo(end);
        continue;
      }
      if (c === "'") {
        const end = quoted(i, "'");
        push('char', start, end, source.slice(start, end), startLine);
        advanceTo(end);
        continue;
      }
    } else {
      if (c === '"' || c === "'") {
        const end = quoted(i, c);
        push('string', start, end, source.slice(start, end), startLine);
        advanceTo(end);
        continue;
      }
      if (c === '`') {
        const end = template(i);
        push('string', start, end, source.slice(start, end), startLine);
        advanceTo(end);
        continue;
      }
      if (c === '/' && regexAllowed()) {
        let k = i + 1;
        let inClass = false;
        while (k < source.length && source[k] !== '\n') {
          if (source[k] === '\\') k++;
          else if (source[k] === '[') inClass = true;
          else if (source[k] === ']') inClass = false;
          else if (source[k] === '/' && !inClass) break;
          k++;
        }
        k++;
        while (/[a-z]/i.test(source[k] ?? '')) k++;
        push('regex', start, k, source.slice(start, k), startLine);
        advanceTo(k);
        continue;
      }
    }

    const word = /^[A-Za-z_$@][\w$]*/.exec(source.slice(i, i + 200))?.[0];
    if (word) {
      push('name', start, i + word.length, word.replace(/^@/, ''), startLine);
      advanceTo(i + word.length);
      continue;
    }
    const number = /^\d[\w.]*/.exec(source.slice(i, i + 50))?.[0];
    if (number) {
      push('number', start, i + number.length, number, startLine);
      advanceTo(i + number.length);
      continue;
    }
    push('punct', start, i + 1, c, startLine);
    advanceTo(i + 1);
  }
  return { tokens, comments };
}

// Index of the matching bracket for every (, [, { and ), ], }.
function matchBrackets(tokens) {
  const pairs = { ')': '(', ']': '[', '}': '{' };
  const match = new Map();
  const stack = [];
  tokens.forEach((token, index) => {
    if (token.kind !== 'punct') return;
    if ('([{'.includes(token.value)) stack.push(index);
    else if (pairs[token.value]) {
      const top = stack.at(-1);
      if (top !== undefined && tokens[top].value === pairs[token.value]) {
        stack.pop();
        match.set(top, index);
        match.set(index, top);
      }
    }
  });
  return match;
}

// Splits the tokens between two indexes (exclusive) at top-level commas.
function splitTopLevel(tokens, match, from, to) {
  const parts = [];
  let part = [];
  for (let k = from; k < to; k++) {
    const token = tokens[k];
    if (token.kind === 'punct' && token.value === ',') {
      parts.push(part);
      part = [];
      continue;
    }
    part.push(k);
    if (token.kind === 'punct' && '([{'.includes(token.value) && match.has(k)) {
      for (let inner = k + 1; inner <= match.get(k); inner++) part.push(inner);
      k = match.get(k);
    }
  }
  if (part.length > 0) parts.push(part);
  return parts;
}

function tagsIn(text) {
  return [...text.matchAll(TAG_PATTERN)].map(([, id]) => id);
}

// ---------------------------------------------------------------------------------------------
// C#: attributes. An attribute section [A(…), B(…)] is parsed into { name, args } where args are
// { name (for "Name = value"), tokens }. A method's attributes are the sections that follow one
// another directly.

function csAttributes(tokens, match, open) {
  const close = match.get(open);
  if (close === undefined) return [];
  return splitTopLevel(tokens, match, open + 1, close).map((part) => {
    let k = 0;
    const nameParts = [];
    // Optional target "method:" and a dotted name such as Xunit.Fact or FactAttribute.
    if (tokens[part[1]]?.value === ':') k = 2;
    while (k < part.length && tokens[part[k]].kind === 'name') {
      nameParts.push(tokens[part[k]].value);
      if (tokens[part[k + 1]]?.value === '.') k += 2;
      else { k++; break; }
    }
    const name = (nameParts.at(-1) ?? '').replace(/Attribute$/, '');
    const parenthesis = part[k];
    const args = [];
    if (parenthesis !== undefined && tokens[parenthesis].value === '(' && match.has(parenthesis)) {
      for (const arg of splitTopLevel(tokens, match, parenthesis + 1, match.get(parenthesis))) {
        const named = tokens[arg[0]]?.kind === 'name' && tokens[arg[1]]?.value === '=' && tokens[arg[2]]?.value !== '=';
        args.push({ name: named ? tokens[arg[0]].value : null, tokens: named ? arg.slice(2) : arg });
      }
    }
    return { name, args, open, close };
  });
}

// All attribute sections directly before and after the section at `open` (one method's attributes).
function csAttributeRun(tokens, match, open) {
  let first = open;
  while (tokens[first - 1]?.value === ']' && tokens[match.get(first - 1)]?.value === '[') first = match.get(first - 1);
  const sections = [];
  for (let k = first; tokens[k]?.value === '[' && match.has(k); k = match.get(k) + 1) sections.push(k);
  return sections.flatMap((section) => csAttributes(tokens, match, section));
}

function stringValue(tokens, indexes) {
  if (indexes.length !== 1 || tokens[indexes[0]].kind !== 'string') return null;
  return /^"(.*)"$/s.exec(tokens[indexes[0]].value)?.[1] ?? null;
}

function analyseCSharp(tokens, match, report) {
  const isSection = (k) => tokens[k].value === '[' && match.has(k);
  const sections = tokens.map((_, k) => k).filter(isSection);
  const seenRuns = new Set();
  const counted = new Set(); // token indexes of strings counted as test names

  for (const section of sections) {
    const attributes = csAttributeRun(tokens, match, section);
    const runKey = attributes[0]?.open;
    if (runKey === undefined || seenRuns.has(runKey)) continue;
    seenRuns.add(runKey);

    const traits = new Map(); // ID → line
    const named = new Map(); // ID → line, from the DisplayName of a test attribute
    for (const attribute of attributes) {
      if (attribute.name === 'Trait' && attribute.args.length === 2 && stringValue(tokens, attribute.args[0].tokens) === 'Invariant') {
        const id = stringValue(tokens, attribute.args[1].tokens);
        if (id === null) report.error(tokens[attribute.open].line, 'the value of [Trait("Invariant", …)] must be a string literal such as "ARC-01"');
        else traits.set(id, tokens[attribute.open].line);
      }
      if (!CS_TEST_ATTRIBUTES.has(attribute.name)) continue;
      const notRun = attribute.args.some((arg) => CS_NOT_RUN_ARGUMENTS.has(arg.name)
        && !(arg.name === 'Explicit' && arg.tokens.length === 1 && tokens[arg.tokens[0]].value === 'false'));
      for (const arg of attribute.args) {
        if (arg.name !== 'DisplayName') continue;
        for (const k of arg.tokens.filter((t) => tokens[t].kind === 'string')) {
          for (const id of tagsIn(tokens[k].value)) {
            named.set(id, tokens[k].line);
            if (notRun) report.ignored(id, tokens[k].line, 'the test is skipped or explicit');
            else report.counted(id, tokens[k].line);
          }
          counted.add(k);
        }
      }
    }
    for (const [id, line] of named) {
      if (!traits.has(id)) report.error(line, `INV:${id} needs [Trait("Invariant", "${id}")] on the same test method (TESTING §8.1)`);
    }
    for (const [id, line] of traits) {
      if (!named.has(id)) report.error(line, `[Trait("Invariant", "${id}")] needs a [Fact] or [Theory] on the same method with INV:${id} in its DisplayName (TESTING §8.1)`);
    }
  }

  tokens.forEach((token, k) => {
    if (token.kind !== 'string' || counted.has(k)) return;
    for (const id of tagsIn(token.value)) report.ignored(id, token.line, 'not in the DisplayName of a [Fact] or [Theory]');
  });
}

// ---------------------------------------------------------------------------------------------
// TypeScript: test(), it() and describe() calls of Playwright and Vitest.

// The callee chain of the call whose "(" is at `open`, e.g. test.describe.skip → [test, describe, skip];
// .each([...]) and similar calls inside the chain are stepped over.
function tsCallee(tokens, match, open) {
  const chain = [];
  let k = open - 1;
  while (k >= 0) {
    const token = tokens[k];
    if (token.kind === 'punct' && token.value === ')' && match.has(k)) {
      k = match.get(k) - 1;
      continue;
    }
    if (token.kind !== 'name') break;
    chain.unshift(token.value);
    if (tokens[k - 1]?.value === '.') k -= 2;
    else break;
  }
  return chain;
}

// "test", "skipped" or null (not a test function).
function tsKind(chain) {
  const [root, ...modifiers] = chain;
  if (TS_SKIPPED_ROOTS.has(root)) return 'skipped';
  if (!TS_TEST_ROOTS.has(root)) return null;
  if (modifiers.some((modifier) => TS_SKIP.has(modifier))) return 'skipped';
  if (modifiers.every((modifier) => TS_KEEP.has(modifier))) return 'test';
  return null;
}

function analyseTypeScript(tokens, match, report) {
  const calls = []; // { open, close, kind, titled }
  tokens.forEach((token, k) => {
    if (token.kind !== 'punct' || token.value !== '(' || !match.has(k)) return;
    const kind = tsKind(tsCallee(tokens, match, k));
    if (kind) calls.push({ open: k, close: match.get(k), kind, titled: tokens[k + 1]?.kind === 'string' });
  });
  const enclosing = (k) => calls.filter((call) => call.titled && call.open < k && k < call.close);

  // test.skip() / test.fixme() without a condition (or with `true`) skips the test or group it sits
  // in, or the whole file at the top level (Playwright).
  const skippedRanges = calls.filter((call) => call.kind === 'skipped' && call.titled);
  for (const call of calls.filter((c) => c.kind === 'skipped' && !c.titled)) {
    const first = tokens[call.open + 1];
    const unconditional = first.value === ')' || (first.value === 'true' && [',', ')'].includes(tokens[call.open + 2]?.value));
    if (unconditional) skippedRanges.push(enclosing(call.open).at(-1) ?? { open: -1, close: tokens.length });
  }

  const titles = new Set(calls.filter((call) => call.titled).map((call) => call.open + 1));
  tokens.forEach((token, k) => {
    if (token.kind !== 'string') return;
    for (const id of tagsIn(token.value)) {
      if (!titles.has(k)) {
        report.ignored(id, token.line, 'not in the title of test(), it() or describe()');
        continue;
      }
      const own = calls.find((call) => call.open === k - 1);
      const skipped = own.kind === 'skipped' || skippedRanges.some((range) => range.open < k && k < range.close);
      if (skipped) report.ignored(id, token.line, 'the test or its group is skipped');
      else report.counted(id, token.line);
    }
  });
}

function collectTags() {
  const tags = new Map(); // ID → list of "file:line" of counted tests
  for (const { dir, language, accept } of SEARCH) {
    for (const file of walk(join(ROOT, dir))) {
      const path = relative(ROOT, file);
      if (!accept(path)) continue;
      const { tokens, comments } = lex(readFileSync(file, 'utf8'), language);
      const report = {
        counted(id, line) {
          if (!tags.has(id)) tags.set(id, []);
          tags.get(id).push(`${path}:${line}`);
        },
        ignored(id, line, reason) {
          warn(`${path}:${line}: INV:${id} is not counted (${reason})`);
        },
        error(line, message) {
          error(`${path}:${line}: ${message}`);
        },
      };
      for (const comment of comments) {
        for (const id of tagsIn(comment.value)) report.ignored(id, comment.line, 'comment');
      }
      const match = matchBrackets(tokens);
      if (language === 'cs') analyseCSharp(tokens, match, report);
      else analyseTypeScript(tokens, match, report);
    }
  }
  return tags;
}

const testingFile = findTestingDocument();
if (!testingFile) {
  error('no docs/TESTING_v<n>.md found');
  process.exit(1);
}
console.log(`Invariant catalogue: ${relative(ROOT, testingFile)}`);

const invariants = parseCatalogue(testingFile);
if (invariants.size === 0) {
  error(`${relative(ROOT, testingFile)} has no catalogue rows (header | ID | Invariant | Level | Step | Status |)`);
  process.exit(1);
}
const tags = collectTags();

const rows = [];
for (const [id, { level, active, protocol }] of invariants) {
  if (!active) continue;
  const tests = tags.get(id)?.length ?? 0;
  if (protocol) {
    const problem = protocolProblem(protocol);
    if (!level.split(/,\s*/).includes('M')) {
      error(`${id}: only manual (M) invariants may be satisfied by a protocol reference`);
    } else if (problem) {
      error(`${id}: protocol ${protocol} ${problem}`);
    }
    rows.push({ id, level, tests, note: protocol });
  } else {
    if (tests === 0) error(`${id} is active but no test that runs is tagged INV:${id}`);
    rows.push({ id, level, tests, note: tests === 0 ? 'MISSING' : '' });
  }
}

for (const [id, places] of tags) {
  const invariant = invariants.get(id);
  if (!invariant) warn(`INV:${id} does not exist in the catalogue (${places.join(', ')})`);
  else if (!invariant.active) warn(`INV:${id} is not active in the catalogue (${places.join(', ')})`);
}

console.log(`\n${invariants.size} invariants in the catalogue, ${rows.length} active:\n`);
const widths = [7, Math.max(5, ...rows.map((r) => r.level.length)), 5];
const line = (a, b, c, d) => `${a.padEnd(widths[0])}  ${b.padEnd(widths[1])}  ${c.padStart(widths[2])}  ${d}`.trimEnd();
console.log(line('ID', 'Level', 'Tests', ''));
for (const { id, level, tests, note } of rows) console.log(line(id, level, String(tests), note));

console.log(failed ? '\nInvariant gate failed.' : '\nInvariant gate passed.');
process.exit(failed ? 1 : 0);

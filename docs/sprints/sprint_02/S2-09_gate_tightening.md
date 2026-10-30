# S2-09 — Invariant gate tightening

> **Sprint:** 02, P0 task S2-09 (roadmap M1.1 ride-along debt, 1 point; TECH_DEBT_v1 #109, #111; closes S1-21 open point P3). **Issues:** #109, #111. **Requirements / invariants:** the gate of TESTING §8.3; no invariant changes status (`TESTING_v17.md`, V76, V77). **Status:** done by Claude Code (2026-10-30), pull request open.
> **Scope:** `scripts/check-invariants.mjs`: fail on catalogue header drift, read test files only, require a Markdown protocol in `docs/a11y-reports/` (#109); count a tag only in a test that runs, require the .NET `Invariant` trait (#111); the matching text in TESTING §8.1, §8.3 and CLAUDE.md. Not in scope: a `[Theory]` counted per case, reading test result reports, CI changes.
> **Owner decisions 2026-10-30:** **1A** "tests that run" is checked on the source text inside the script, not by reading the TRX/Playwright/Vitest reports of stages 3–6 (the gate stays dependency-free, runs locally, stage 7 keeps no `needs`). **2a** a protocol reference must be a file under `docs/a11y-reports/`, as TESTING §7.2 and §8.3 say; the prompt's "protocols now live in `docs/sprints/sprint_NN/`" refers to task protocols, which are not invariant protocols. **3** the owner's local files (`TASK_PROMPTS.md`, `.ai/`, the deleted `ARCHITECTURE_v13.md` and `TASK_PROMPT.md`) stay out of this pull request. Also, not asked: the trait must sit on the test method itself, as on every test today.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The gate parsed only tables with the exact header `\| ID \| Invariant \| Level \| Step \| Status \|`; any other table was skipped silently (#109.1) | A table starting with `\| ID \| Invariant \|` now has to match exactly, otherwise the gate fails |
| F2 | `e2e/` accepted every `.ts` file, `tests/` included `ProcureAccess.Tests.Shared` (#109.2). Playwright's `testMatch` is `**/*.spec.ts`; `Tests.Shared.csproj` says `<IsTestProject>false</IsTestProject>` | `e2e/**/*.spec.ts` only; .NET projects with `IsTestProject` false are skipped (read from the project file, not by name) |
| F3 | A protocol reference passed if anything existed at the path, folder or outside `docs/` (#109.3) | Must resolve to a `.md` file inside `docs/a11y-reports/` |
| F4 | Any `INV:` text in a scanned file counted, also in comments and skipped tests; the .NET trait was not checked (#111) | Small lexer for C# and TypeScript; tags count only in test names, with the trait checked per method (decision 1A) |
| F5 | Today every active invariant already meets the stricter rules: every .NET test carries `[Trait("Invariant", …)]` on the method, the only TypeScript tags are in the two J1 titles | The gate passes on `main`'s tests with the same counts as before (check 2) |
| F6 | The only manual invariant is A11Y-07 (`planned`); `docs/a11y-reports/` does not exist yet | The protocol rule has no effect today; cases P1–P6 show it |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `scripts/check-invariants.mjs` | Header drift is an error (F1); search limited to test files (F2); protocol must be a Markdown file in `docs/a11y-reports/` (F3). A lexer separates code, strings and comments (C# raw, verbatim and interpolated strings, TypeScript templates and regex literals). **.NET:** a tag counts in the `DisplayName` of `[Fact]`/`[Theory]` without `Skip`, `SkipWhen`, `SkipUnless`, `Explicit = true`; the attributes of the same method must include `[Trait("Invariant", "<ID>")]` with a string literal, and every such trait needs the tag. **TypeScript:** a tag counts in the title of `test`/`it`/`describe` (with `.only`, `.serial`, `.each` …), not with `.skip`, `.fixme`, `.todo`, `.fails`, `.skipIf`, `.runIf` or `x…`, not inside a skipped group, and not in a group or file with an unconditional `test.skip()`/`test.fixme()`. Tags that do not count are printed as warnings with the reason | #109, #111 |
| `docs/TESTING_v17.md` (new) | Copy of v16: status line; "Changes from v16" V76 (#109) and V77 (#111, decision 1A; replaces the limits of V72); §4 intro, §8.1 (trait on the method, `--filter-trait`), §8.3 steps 1–4, known limits, protocol rule | Prompt: text changes in the next TESTING version |
| `CLAUDE.md` | Header; §3 (what the gate checks since S2-09); §5 comment of the gate command | DoD |
| `docs/sprints/sprint_01/S1-21_invariant_gate.md` | P3 marked done | Traceability |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; plus the question whether the gate can now pass without checking. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: the script reads repository files and runs nothing; protocol paths are resolved before the folder check, so `..` cannot leave `docs/a11y-reports/` (case P3). N+1, a11y: not applicable | – |
| R2 | Fail-open risk: a lexer slip could hide a tag. It fails closed instead: a tag the lexer does not see in a test title is not counted, so the invariant shows as MISSING. Checked on all 335 C# and TypeScript files of the repository (migrations excluded): every bracket matched | – |
| R3 | Simplified own code: the skipped-group check had a redundant clause; a file-level `test.skip()` was not handled (Playwright skips the file); a trait value that is not a string literal gave a misleading message | Fixed before the case runs (R10, T4) |
| R4 | A `describe('INV:X …')` counts even if all of its tests are skipped or it has none | Won't fix: rare, and §8.1 allows the tag on a group title; added to the known limits in TESTING_v17 §8.3 (P1) |
| R5 | For each C# string the raw-string check slices the rest of the file (quadratic in file size) | Won't fix: test files are small; the gate runs in 0.05 s |
| R6 | GitHub annotation messages are not escaped (`%`, line breaks) | Won't fix: unchanged behaviour, all messages are one line |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` on the branch | ✅ passed, 11 active invariants, 0.05 s |
| 2 | The same counts as the old gate on `main`'s tests (old script run beside the new one) | ✅ identical: CAT-10 2, PRV-05 3, SEC-06 1, ARC-01 1, ARC-02 1, ARC-03 7, ARC-06 1, OPS-01 2, OPS-02 2, OPS-03 5, A11Y-01 2; no warning |
| 3 | New cases on a scratch branch (§4.1) | ✅ all 22 cases fail the new gate (the old gate failed only P5); the two controls and the unchanged tree pass |
| 4 | Lexer on every C# and TypeScript file of the repository (R2) | ✅ 335 files, no unmatched bracket |
| 5 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 6 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 587/587 |
| 7 | Build, .NET tests, Vitest, ESLint, E2E | – no code of these touched |

### 4.1 Cases

Run by a script in the session scratchpad on the local branch `scratch/S2-09-gate-cases` (deleted afterwards): one change at a time, then the new and the old gate, then revert. Nothing of it is committed. Line references point at the changed files.

| Case | Change | New gate | Old gate | New gate's message |
|---|---|---|---|---|
| H1 | Catalogue header of §4.10 gets an extra column | ✗ | ✓ | `docs/TESTING_v17.md:503: catalogue table header is not \| ID \| Invariant \| Level \| Step \| Status \|` (the old gate dropped ARC-01 – ARC-03, ARC-06, OPS-01 – OPS-03 and passed) |
| H2 | A header cell renamed (`Status` → `State`) | ✗ | ✓ | same |
| F1 | A11Y-01 tags removed from J1; the tag in `e2e/support/axe-suppressions.ts` | ✗ | ✓ | `A11Y-01 is active but no test that runs is tagged INV:A11Y-01` |
| F2 | ARC-06 tag and trait removed from the unit test; a tagged test in `ProcureAccess.Tests.Shared` | ✗ | ✓ | `ARC-06 is active but no test …` |
| P1 | A11Y-07 `active — docs/a11y-reports/` (the folder) | ✗ | ✓ | `protocol docs/a11y-reports/ is not a Markdown file in docs/a11y-reports/` |
| P2 | A11Y-07 with an existing file outside the folder (`docs/sprints/…/S2-08_….md`) | ✗ | ✓ | same form |
| P3 | A11Y-07 with `docs/a11y-reports/../sprints/…` | ✗ | ✓ | same form |
| P4 | A11Y-07 with `docs/a11y-reports/.gitkeep` | ✗ | ✓ | same form |
| P5 | A11Y-07 with a missing `.md` in the folder | ✗ | ✗ | `… does not exist` |
| P6 | Control: A11Y-07 with an existing `.md` in the folder | ✓ | ✓ | – |
| R1 | ARC-06 tag only in a comment above a plain `[Fact]` | ✗ | ✓ | warning `(comment)` + MISSING |
| R2 | `[Fact(Skip = "flaky", DisplayName = …)]` | ✗ | ✓ | warning `(the test is skipped or explicit)` + MISSING |
| R3 | `[Fact(Explicit = true, …)]` | ✗ | ✓ | same |
| R4 | Tag moved into `[InlineData("INV:ARC-06")]` | ✗ | ✓ | trait without tag + warning `(not in the DisplayName …)` + MISSING |
| R5 | `test.describe.skip` around J1 | ✗ | ✓ | warnings `(the test or its group is skipped)` + MISSING A11Y-01 |
| R6 | Both J1 tests `test.fixme` | ✗ | ✓ | same |
| R7 | `test.skip();` as the first statement of the J1 group | ✗ | ✓ | same |
| R8 | A11Y-01 only in an `xtest`, a comment and a `test.step` title | ✗ | ✓ | one warning each + MISSING |
| R10 | `test.skip();` at file level in J1 | ✗ | ✓ | warnings + MISSING |
| R9 | Control: `test.skip(browserName === 'webkit', …)` in the group | ✓ | ✓ | – (runtime condition, known limit) |
| T1 | ARC-06 trait removed | ✗ | ✓ | `INV:ARC-06 needs [Trait("Invariant", "ARC-06")] on the same test method` |
| T2 | Trait names `ARC-05` | ✗ | ✓ | the message of T1 and `[Trait("Invariant", "ARC-05")] needs a [Fact] or [Theory] … with INV:ARC-05` |
| T3 | Trait moved to the class | ✗ | ✓ | both messages of T2 for ARC-06 |
| T4 | Trait value `Ids.Arc06` instead of a literal | ✗ | ✓ | `the value of [Trait("Invariant", …)] must be a string literal` |
| M0 | No change | ✓ | ✓ | – |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Known limits that stay (TESTING_v17 §8.3): skips decided at runtime, tests a CI stage filters out, code removed by preprocessor directives, a `[Theory]` counted once, a group title counted even if its tests are all skipped (R4). Reading the result reports (option 1B) would close most of them if one of them ever bites | Owner, if needed |
| P2 | The case script lives only in the session scratchpad. A permanent self-test of the gate (Node's built-in `node:test`, no dependency) would protect the lexer against regressions, but needs a CI step | Proposal for a later CI task (e.g. with S2-20) |
| P3 | TECH_DEBT_v1 still lists #109 and #111 as open | Sprint 02 close-out, TECH_DEBT_v2 |

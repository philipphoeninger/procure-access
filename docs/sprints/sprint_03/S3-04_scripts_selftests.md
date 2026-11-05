# S3-04 — Scripts: CodeQL alert 3 and a permanent self-test of the invariant gate

> **Sprint:** 03, P0 task S3-04 (debt, 0.5 points). **Issues:** #182 (TD-16, CodeQL alert 3), #185 (TD-20; S2-09 P2, S2-20 P3). **Requirements / invariants:** none (CI stage 1); no invariant activated. New `TESTING_v25.md` (V86) for the stage-1 and §8.3 text only. **Status:** done by Claude Code (2026-11-05), pull request #198.
> **Scope:** exactly the sprint row: `setText` of `scripts/check-i18n.mjs` rejects prototype keys, with a self-test case; a `node:test` self-test of `scripts/check-invariants.mjs` that runs in CI stage 1 like the Figma and i18n self-tests. What the gate accepts is unchanged.
> **Owner decisions** (asked before the start, 2026-11-05):
> 1. **(1a) Test the gate from outside:** each case copies the unchanged `check-invariants.mjs` into a small fake repository in a temp folder and runs it there; the gate gets no exports and no refactor.
> 2. **(2a) TESTING_v25 in this PR:** stage 1 of §8.2 names the self-tests, so the new one goes into a new TESTING version; no other PR was open, and S3-05 starts after this one is merged.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | CodeQL alert 3 (`js/prototype-pollution-utility`, medium) points at `cur[name] = text` in `setText` (`check-i18n.mjs:170`). With the key `shell.__proto__.polluted`, `cur` becomes `Object.prototype` and the text is written there (shown by the new case on `main`'s `setText`, §4 check 1) | Reject the three segments before anything is written |
| F2 | The `KEY` pattern of the UX copy parser already excludes `__proto__` (no `_` in a segment), but not `constructor` or `prototype`; `setText` is exported and takes any key | The guard sits in `setText` itself, not in the parser |
| F3 | The other functions that split keys (`keysOfCell`, `lookup`, `flatten`, `templateKeys`, `check`) only read; `syncAreas` writes only through `setText` | No other function needs the guard |
| F4 | `check-invariants.mjs` exports nothing and runs on load against its own repository (`ROOT` from `import.meta.url`) | Decision 1a: run it as a process on a fake repository; the script's location makes the temp folder its root |
| F5 | The S2-09 cases (`S2-09_gate_tightening.md` §4.1: H1–H2, F1–F2, P1–P6, R1–R10, T1–T4, M0) existed only as a scratch script | They become the self-test, plus cases for rules S2-09 did not touch: highest version, missing document, row errors, typo warnings |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `scripts/check-i18n.mjs` | `setText` throws `<key>: the segment "<name>" is not allowed in a key` when any segment is `__proto__`, `constructor` or `prototype`, before it writes anything; also where the rest of the key would become one dotted property name | #182 |
| `scripts/check-i18n.test.mjs` | Case: the three segments throw and leave `de.json` unchanged, also in the dotted-name branch and through `syncAreas` (`--write`); `Object.prototype` stays clean | #182 |
| `scripts/check-invariants.test.mjs` (new) | 36 cases on `node:test`. `gate(changes)` writes a clean fake repository (catalogue with three active invariants, a .NET test project and a non-test project, a Playwright and a Vitest spec) with the case's changes into a temp folder, copies the gate in, runs it with `GITHUB_ACTIONS=false` (same messages locally and in CI) and removes the folder. Cases: the clean tree passes with one test per active invariant; catalogue (highest version as a number, no document, no rows, H1/H2 header drift, malformed row, duplicate ID, unknown status); test files only (F1: e2e support file, client source, `node_modules`; F2: `IsTestProject` false); TypeScript (R5–R8, R10, `.todo`, `it.skip`, `test.skip(true, …)`, `.skipIf`; control R9 with a conditional skip, `.only`, `.serial`); .NET (R1–R4, `SkipWhen`, T1–T4; control `Explicit = false` with the trait in the same section); protocols (P1–P5, a non-M invariant with a protocol; control P6); warnings for an unknown and a non-active ID | #185 |
| `.github/workflows/ci.yml` | Stage 1: step "Invariant gate self-test" after the i18n check; the gate itself stays in stage 7 | #185 |
| `docs/TESTING_v25.md` (new) | Copy of v24: V86; §8.2 stage 1 row; §3.4 layout; §8.3 paragraph "Self-test"; status line with v25 (and the missing v24 clause, P2) | Decision 2a |
| `CLAUDE.md` | Header; §3 (gate self-test, `setText` guard); §4 (`scripts/`); §5 (command) | DoD |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; plus whether the self-test can pass while the gate is broken (§4 check 3). N+1 and a11y: not applicable. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: the self-test writes only into `mkdtemp` folders and removes them in `finally`; it starts Node with an argument array, no shell; no input from outside the file | – |
| R2 | The guard checks all segments before the write loop. CodeQL may not count a check on the split array as a barrier for the variable it writes with, so alert 3 could stay open even though the code is safe | Check on the PR's CodeQL run (§4 check 7); if the alert stays, guard `name` inside the loop as well (P1) |
| R3 | `spawnSync` has no timeout: a lexer regression that loops forever would hold stage 1 until the job's 20-minute timeout | Won't fix: the stage still fails, and the job timeout bounds it |
| R4 | The fake repository tests the gate's rules, not the shape of the real repository (e.g. a moved `tests/` folder) | Won't fix: stage 7 runs the gate on the real repository |
| R6 | CI on the PR: CodeQL alert 4 (`js/incomplete-sanitization`, high) on the self-test: the protocol cases built a regular expression from the reference and escaped only dots. Harmless with the fixed references, but it fails the `CodeQL` check | Fixed: the reference is compared as plain text (`includes`), the message pattern stays a literal; the protocol mutations of §4 check 3 are still caught |
| R5 | Scope: the status line of TESTING lacked the v24 clause (S3-03); added with v25 so "Supersedes v24" makes sense. Its "v15 —" start stays for S3-05 (#180) | – (P2) |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node --test scripts/check-i18n.test.mjs` | ✅ 11/11; with `main`'s `check-i18n.mjs` the new case fails (10/11, "Missing expected exception: shell.__proto__.polluted") |
| 2 | `node --test scripts/check-invariants.test.mjs` | ✅ 36/36 in about 1 s, on Node 22 and Node 24 (CI's version), also with `GITHUB_ACTIONS=true` in the environment |
| 3 | Mutations of the gate (scratchpad, one at a time, reverted): header drift ignored, version sorted as text, any `.ts` in `e2e/`, non-test projects read, `Skip`/`Explicit` ignored, TypeScript skip modifiers ignored, unconditional `test.skip()` ignored, comments counted, trait not required, orphan trait allowed, trait literal not checked, protocol outside the folder, missing protocol, protocol on a non-M invariant, missing test passes, row form unchecked, typo warning off | ✅ 17/17 caught, each by the cases of its rule; the gate is byte-identical to `main` afterwards |
| 4 | `node scripts/check-invariants.mjs` | ✅ passed, reads `docs/TESTING_v25.md` |
| 5 | `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs`, `node --test scripts/check-figma-links.test.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending; 0 errors; 10/10) |
| 6 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 686/686 |
| 7 | CI on the pull request (stage 1 with the new step; CodeQL `Analyze (javascript-typescript)` and alert 3 on the PR ref) | First run: stages 1–4 and 7 and the three `Analyze` jobs green, the new step ran in stage 1; alert 3 no longer on `refs/pull/198/merge`, so the guard counts as a fix (R2); the `CodeQL` check failed on the new alert 4 (R6), fixed in the second commit |
| 8 | Build, .NET tests, Vitest, ESLint, E2E | – no code of these touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Alert 3 closes when the analysis of `main` no longer finds it, i.e. after the merge; the PR's analysis no longer reports it (§4 check 7) | Owner, check after the merge |
| P2 | The TESTING status line still starts with "v15 —" in every version since v15 | S3-05 (#180) |
| P3 | `scripts/*.mjs`, including the new self-test, is not linted | S3-26 (#161) |
| P4 | TD-16 and TD-20 are closed by this PR; the register records it in its next version | Sprint 03 close-out (TECH_DEBT_v3) |

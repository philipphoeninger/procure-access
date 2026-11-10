# S4-04 — Scripts and licence: CodeQL alert 3 and the CAT-10 fixture

> **Sprint:** 04, P0 task S4-04 (debt, 0.5 points). **Issues:** #233 (TD-24), #242 (TD-36). **Requirements / invariants:** none (CI stage 1); no invariant activated, no new TESTING version. **Status:** done by Claude Code (2026-11-10), pull request open for review; alert 3 is checked after the merge (P1).
> **Scope:** exactly the sprint row: the loop variable of `setText` in `scripts/check-i18n.mjs` checked inside the loop as well, with a self-test case; the CAT-10 fixture annotated as BIT inklusiv in `REUSE.toml`. No other behaviour change.
> **Owner decisions:** none needed (task prompt: "none"; confirmed before the start). The plan was approved without changes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | CodeQL alert 3 (`js/prototype-pollution-utility`) is open on `main` at 9934952, at `scripts/check-i18n.mjs:174` (`cur[name] = text` in `setText`); it is the only open alert. The guard of #182 checks the split array `segs` before the loop. The analysis does not count that as a barrier for the loop variable `name` (S3-04 R2, P1) | Check `name` itself inside the loop, before `cur[name]` is read or written (#233) |
| F2 | The check before the loop is what keeps `de.json` unchanged for a rejected key: a check only during the walk would already have created the groups before the unsafe segment (`shell.b` in `shell.b.__proto__`) | Both checks stay. The inner one cannot be reached while the outer one exists; it is there for the analysis |
| F3 | `cat-10-criteria-filter.json` compares `id`, `name` and `criteriaFilterId` of the legacy seed; the names are BIT inklusiv criterion titles. `REUSE.toml` covers the file as MIT through the `**/*.json` block (S1-15 R3, S3-11 P3) | Add it to the existing BIT inklusiv block, which already stands below the JSON block (the last matching block wins) (#242) |
| F4 | The fixture's own `description` says the descriptions are left out because "this MIT-annotated file" does not copy BIT inklusiv texts | The sentence would become wrong; reworded. Nothing reads it (the integration test and J1 read `cases` and `comparedFields`) |
| F5 | No other test file copies BIT inklusiv texts: `ExistingCatalogSeed.cs` reads the seed arrays of `MODELS`, it does not copy them; the MiniCatalog and the test release folders `T.1`, `T.2` hold made-up texts | Nothing else to annotate |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `scripts/check-i18n.mjs` | `setText`: at the top of the loop, `name === '__proto__' \|\| name === 'constructor' \|\| name === 'prototype'` throws the same error as the check before the loop (`<key>: the segment "<name>" is not allowed in a key`), with a comment that it is unreachable and why it is there; the JSDoc says that all segments are checked before anything is written | #233, F1, F2 |
| `scripts/check-i18n.test.mjs` | New case "--write rejects an unsafe segment at the start, in the middle and at the end of a key": each of the three names in `<name>.a`, `shell.b.<name>.c` and `shell.b.<name>`, on a dictionary without `shell.b`; checks the exact message, that the dictionary stays `{ shell: {} }`, and that `Object.prototype` gains neither `a` nor `c`. The message is compared as a string, not through a `RegExp` built from the name (S3-04 R6) | Task prompt: self-test case |
| `REUSE.toml` | `tests/ProcureAccess.Tests.Integration/Catalog/Fixtures/cat-10-criteria-filter.json` added to the BIT inklusiv block (copyright `BIT inklusiv` and BSVH, `MIT AND LicenseRef-AllRightsReserved`); the block's comment names it | #242, F3 |
| `tests/ProcureAccess.Tests.Integration/Catalog/Fixtures/cat-10-criteria-filter.json` | `description`: "Descriptions are left out (S1-15 F6). The names are BIT inklusiv texts, so REUSE.toml annotates this file like the seed (#242)." | F4 |
| `CLAUDE.md` | Header entry; §3: the `--write` guard also checks each segment inside the write loop. §4 and §5 unchanged (no new path or command) | CLAUDE §11 |
| `docs/sprints/sprint_04/S4-04_scripts_licence.md` | This protocol | – |

ARCHITECTURE, SPEC, TESTING, the Figma mapping and `LEGACY_EXCEPTIONS` are not touched. TD-24 and TD-36 are closed by this pull request; `TECH_DEBT` records it in its next version at the sprint close-out (as S3-04 P4).

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | The write path of `--write` cannot reach `Object.prototype` (both checks; mutation C3). The script runs only in CI and locally on files of the repository |
| Correctness | Same message at both checks; a rejected key changes nothing (new case, mutation C3). Valid keys behave as before (all 12 cases, `check-i18n.mjs` on the real `de.json`) |
| Edge cases | Unsafe segment first (on the root), in the middle with a missing group before it, last; the dotted-name branch keeps its case from S3-04 |
| Performance | Three string comparisons per segment; no change worth measuring |
| a11y, UI texts | Not touched; `de.json` unchanged |
| Licence | The fixture now shows `MIT AND LicenseRef-AllRightsReserved` with both copyright holders in `reuse spdx` (C5) |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The three names are listed twice. A shared constant with `includes` would be shorter, but the analysis may not count it as a barrier, which is the point of #233 | Won't fix |
| R2 | `cur[segs.slice(i).join('.')] = text` (dotted-name branch) also writes with a computed name. It always contains a dot, so it can never be one of the three names; the analysis has not flagged it | No change; the CodeQL run of the pull request confirms it (C7) |
| R3 | No test can reach the check inside the loop while the outer check exists (F2). The new case fails when the outer check is removed (C3); the inner one is guarded by the analysis | Won't fix (by design) |
| R4 | `REUSE.toml` names the fixture by its exact path. A CAT-10 fixture under a new name would fall back to MIT | Follow-up: P2 |

No correctness or security bug left that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `node --test scripts/check-i18n.test.mjs` | 12 of 12 pass (11 before) |
| C2 | Self-tests `check-figma-links`, `check-icons`, `check-invariants`, `check-release-config` | 10, 13, 36, 6 pass |
| C3 | Mutations (reverted): (a) the check before the loop removed; (b) both checks removed | (a) the new case and the S3-04 case fail; (b) both fail, and the next case fails too because `Object.prototype` was polluted. Unchanged code: all pass |
| C4 | `node scripts/check-i18n.mjs`, `check-invariants.mjs`, `check-figma-links.mjs`, `check-icons.mjs` | Exit 0 each |
| C5 | `reuse lint` (Docker `fsfe/reuse`); `reuse spdx` for the fixture | Compliant, 1069 / 1069 files (with this protocol); fixture: `LicenseRef-AllRightsReserved`, `MIT`, copyright BSVH and `BIT inklusiv` |
| C6 | `npm run typecheck && npm run lint` in `e2e/` (lints `scripts/*.mjs`) | Green |
| C7 | `dotnet test --project tests/ProcureAccess.Tests.Integration --filter-class "*CriteriaFilterCharacterisationTests"` (reads the fixture) | 6 of 6 pass. No .NET or client code changed, so no further build; J1 (also reads the fixture) runs in CI |
| C8 | CodeQL on the pull request | See the pull request; result recorded in P1 |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | Alert 3 closes when the analysis of `main` no longer finds it, i.e. after the merge | Check after the merge (`gh api repos/philipphoeninger/procure-access/code-scanning/alerts/3`) and record the result here in a follow-up commit; #233 is closed by the merge |
| P2 | If the CAT-10 rewrite (S4-13, O-S4-5) adds or renames a fixture that quotes BIT inklusiv texts, it needs the same entry in `REUSE.toml` (R4) | In S4-13; #242 already names it |

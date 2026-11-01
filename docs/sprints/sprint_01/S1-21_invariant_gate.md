# S1-21 — Invariant gate, Figma link check, step-0 invariants active

> **Sprint:** 01, task S1-21 (milestone M0.5, TESTING §4, §8.2 stages 1 and 7, §8.3, §9 step 0; `figma_code_mapping_procureaccess_v1.md` §4; risk R4). **Status:** done by Claude Code (2026-10-24).
> **Scope:** `scripts/check-invariants.mjs` (reads the highest `docs/TESTING_v*.md`), `scripts/check-figma-links.mjs`, both wired into CI in place of the S1-20 placeholders; ARC-06, OPS-03, CAT-10 and A11Y-01 set to `active`. Not in scope: the other "next TESTING version" items of S1-12 P2, S1-13 P2, S1-16 P1, S1-17 P3, S1-18 P4/P5 and S1-20 D1–D6 (separate documentation session); stage 9; required checks on `main`.
> **Owner decisions 2026-10-24:** **A1** a new `TESTING_v11.md` with a "Changes from v10" table that covers only this task. **B1** the legacy `layout/header/header.ts` stays in the Figma `MAP`, but on a separate `PENDING` list with its reason, until the step-1 shell replaces it.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | All four step-0 invariants already have tagged tests: ARC-06 (1, Unit), OPS-03 (2, Integration), CAT-10 (1 Integration + J1), A11Y-01 (2 in J1); no other `INV:` tag in the repository | Only the status flip and the gate were missing |
| F2 | TESTING §8.3 names `docs/TESTING.md`; the documents are versioned, and `TESTING_v9.md` and `TESTING_v10.md` both exist. Sorting by text would pick v9 | The gate compares the version numbers (risk R4) |
| F3 | The catalogue has 14 tables with the header `ID \| Invariant \| Level \| Step \| Status`; the journey table has another header. Some cells contain `\|` (SIG-01, SIG-02, MOD-01) | The gate parses only catalogue tables and does not split on escaped pipes |
| F4 | The Figma `MAP` contains `src/app/layout/header/header.ts`. That file exists, but it is the legacy header, not `pa-app-header`, so the documented script would fail on `main` | Decision B1 |
| F5 | The documented Figma check uses `includes`, so a tag for node `2032-4860` would also satisfy node `2032-486` | The node ID must end after the expected tag |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `scripts/check-invariants.mjs` (new) | Node, no dependencies, repository root from the script location (runs from any directory). Picks the highest `docs/TESTING_v<n>.md`; parses catalogue tables only; fails on no file, no rows, a malformed row, a duplicate ID or an unknown status. Searches `tests/**/*.cs`, `CLIENT/**/src/**/*.spec.ts` and `e2e/**/*.ts` (skips `node_modules`, `bin`, `obj`, `dist`, `coverage`, Playwright output, dot folders) for `INV:<ID>`. Fails if an active invariant has no tag; warns on tags with an unknown or non-active ID. `active — <file>` is accepted only for level `M`, and the file must exist (§8.3). Prints the file it read and a table with the number of tagged tests; GitHub annotations in Actions | TESTING §8.3; R4 |
| `scripts/check-figma-links.mjs` (new) | The script of `figma_code_mapping_procureaccess_v1.md` §4 with the same `MAP`, plus: `PENDING` list (header, decision B1; an entry not in `MAP` fails, a pending file that already has its tag gets a notice to remove the entry), exact node-ID match (F5), root from the script location, summary line | Mapping §4; B1 |
| `docs/TESTING_v11.md` (new) | Copy of v10 with: status line, "Changes from v10" V58–V60, the four rows `active`, §3.4 layout `TESTING_v<n>.md`, §8.2 stage 1 (`PENDING` notice), §8.3 step 1 (highest version, catalogue header, failure on no file/rows) | A1 |
| `.github/workflows/ci.yml` | Stage 1: `node scripts/check-figma-links.mjs` (Node is already set up in the job). Stage 7: checkout, `setup-node` (same pinned SHA, `CLIENT/…/.nvmrc`), `node scripts/check-invariants.mjs` | S1-20 D4, decision 2 |
| `CLAUDE.md` §3, §4, §5 | Current state (S1-21), `scripts/` in the layout, the two commands; the "From S1-21 on" sentence replaced | CLAUDE §11 |

No production code, UI text or dependency changed.

## 3. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` | ✅ reads `TESTING_v11.md`, 136 invariants, 4 active: CAT-10 2, ARC-06 1, OPS-03 2, A11Y-01 2; same result when run from another directory |
| 2 | **Done when:** `INV:ARC-06` removed from `MappingProfileTests.cs` | ✅ exit 1, `ARC-06 is active but no test is tagged INV:ARC-06`; after restoring the tag exit 0 |
| 3 | OPS-01 set to `active` without a test | ✅ exit 1 |
| 4 | Probe file with `INV:ARC-99 INV:OPS-01` | ✅ two warnings (unknown, not active), exit 0 |
| 5 | Status `activ` | ✅ exit 1, unknown status with file and line |
| 6 | `active — <file>`: missing file on A11Y-07 (M); reference on OPS-01 (I); existing file on A11Y-07 | ✅ exit 1 / exit 1 / exit 0 |
| 7 | Without `TESTING_v11.md` (only v9 and v10) | ✅ reads `TESTING_v10.md` (numeric order), 0 active, four "not active" warnings |
| 8 | `TESTING_v99.md` without a catalogue | ✅ exit 1, "has no catalogue rows" |
| 9 | `node scripts/check-figma-links.mjs` | ✅ exit 0: 0 checked, 12 not built yet, 1 pending (header notice) |
| 10 | Temporary `shared/components/notice/notice.ts`: no tag / tag for node `2032-4860` / correct tag | ✅ exit 1 / exit 1 / exit 0 |
| 11 | Header with its correct tag | ✅ exit 0 with the notice "remove it from PENDING" |
| 12 | `reuse lint` 6.2.0, `actionlint` 1.7.12 | ✅ compliant (498/498) / ✅ |
| 13 | Local, final: `dotnet build` / `dotnet test --solution` / `dotnet format` whitespace + style / `npm run lint` / `npx ng test` / `npm run build:release` / Playwright on chromium, firefox, mobile-chrome | ✅ 0 errors, 136 warnings (unchanged) / ✅ 22/22 / ✅ / ✅ / ✅ 5/5 / ✅ 1.81 MB / ✅ 39/39 |

All temporary edits were reverted; `git status` showed only the intended files afterwards.

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The other "next TESTING version" items (S1-12 P2, S1-13 P2, S1-16 P1, S1-17 P3, S1-18 P4/P5, S1-20 D1–D6) are still open; v11 covers only this task (A1) | Philipp / documentation session |
| P2 | Older documents still name `TESTING_v10.md` (SPEC_v2, ARCHITECTURE_v12, handoff, UX copy v14, SPRINT_01). Left as they are: they name the version they were aligned with, and CLAUDE §2 says to use the highest version | Next version of each document |
| P3 | The gate counts `INV:` tags, not tests: a tag in a comment counts, a `[Theory]` counts once, and a .NET test with `INV:` but without the `Invariant` trait passes (§8.1 asks for both). A trait check could follow when the Architecture project gets its first tests | **Done 2026-10-30** in S2-09 (#109, #111; TESTING_v17 §8.3, V76, V77); a `[Theory]` still counts once |
| P4 | The `PENDING` entry for the header must be removed by the step-1 shell PR; the check prints a notice once the tag is there | Step 1 |
| P5 | `figma_code_mapping_procureaccess_v1.md` §4 still shows the script without `PENDING` and the exact node match; it should point to `scripts/check-figma-links.mjs` as the source in its next version | Next mapping version |
| P6 | CAT-10 is `active` with step `0→2`: step 2 replaces the filter and has to keep or rewrite the characterisation tests, never drop the tag | Step 2 |
| P7 | Branch protection with stages 1–7 as required checks (S1-20 P1) makes the gate binding on GitHub; without it a red gate only shows on the PR | Philipp |

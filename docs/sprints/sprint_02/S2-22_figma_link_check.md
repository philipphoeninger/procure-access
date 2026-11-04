# S2-22 — Figma link check improvements

> **Sprint:** 02, stretch task S2-22 (1 point; TECH_DEBT_v1 §3.4, §9.2 "M1.4 Shell"). **Issues:** #110. **Requirements / invariants:** none (CI stage 1); no invariant changes status. TESTING_v23 records the wider stage-1 check (V84). **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** the three points of #110 in `scripts/check-figma-links.mjs`: map the pages, find mapped components at another path, validate `PENDING` files. Also a self-test on `node:test` in CI stage 1, and the second `@figma` line on `layout/header/header.ts`. Not in scope: the stale references in the header of the Figma mapping (§5 P5) and TECH_DEBT_v1 (§5 P4).
> **Owner decisions 2026-11-04 (before the task, asked by Claude):** **O-S2-22-1 = A** add the pages of mapping §2.2 to the check at their planned paths; only the first `@figma` line (base frame) is checked. **O-S2-22-2 = A** `header.ts` gets a second `@figma` line for `pa-app-header-compact` (2039-2131); mapping §3 allows one line per Figma component a class maps. **O-S2-22-3 = A** the selector search covers only the components of mapping §2; pages are found by path.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `MAP` held 13 component files with one node each; no pages. TESTING_v22 §8.2 stage 1 promised "every domain component and page" | `PAGES` with the 13 rows of §2.2 (O-S2-22-1) |
| F2 | `header.ts` named 2039:2131 only in its prose, so the compact header node was never checked | Second tag (O-S2-22-2); the check now fails without it (seen before the tag was added) |
| F3 | A mapped file missing at its exact path counted as "not built yet", wherever the component really was | Selector search over the production `.ts` files |
| F4 | The legacy `features/criteria/list/criteria-list.ts` declares `pa-criteria-list`, the natural selector of the S11 page | Pages by path only (O-S2-22-3) |
| F5 | A `PENDING` entry skipped every state of its file, and a `PENDING` entry without a file was only counted as missing. The list has been empty since S2-15 | Wrong node and missing file fail; the notice to remove a tagged entry stays (S1-21 P4) |
| F6 | The mapping was already at v1.13 (S2-11, S2-15), and v1.11 had closed S1-21 P5. My question before the task named "1.11" | Mapping v1.14; P5 of S1-21 was already done |
| F7 | §2.2 marks the 360 px base frame in bold; S11, S35, S37, S41, S53 and S55 have no 360 px frame; S34 and S51 have two bold nodes | Base frame = first bold node, else the first node listed; written down in §2.2 |

## 2. Changes

| File | Change |
|---|---|
| `scripts/check-figma-links.mjs` | Split into a pure `check({ components, pages, pending, files, base })`, plus `main()` that reads the files, as in `check-i18n.mjs`. `COMPONENTS`: path → nodes + selector; every node must be a `@figma` line. `PAGES`: path → base frame, which must be the first `@figma` line. `figmaTags()` reads the node of each `@figma` line (exact node, so `2014-341` ≠ `2014-3410`; a foreign link gives `null`). Selector search: a production `.ts` file under `src/app` (without `*.spec.ts` and `src/app/testing/`) that declares a mapped selector at another path fails. `PENDING`: an entry in neither list fails, an entry whose file is gone fails, a pending file with a tag of the wrong node fails, a tagged pending file gets the notice to remove the entry, an untagged one gets the notice with its reason |
| `scripts/check-figma-links.test.mjs` (new) | Self-test on `node:test`, 10 cases: tag parsing, pass case, each failure (missing node, longer node id, page order, moved and duplicate component, every `PENDING` case), path prefix |
| `.github/workflows/ci.yml` | Stage 1 step "Figma link check" runs the self-test first |
| `CLIENT/…/layout/header/header.ts` | `@figma … node-id=2039-2131 (compact)` after the 2017-650 line |
| `docs/TESTING_v23.md` (new) | V84; §8.2 stage 1 and §3.4 name the self-test and say what the check covers; input document figma mapping 1.14 |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.14: §2 status of the compact header, §2.2 base-frame rule and expected file paths, §3 one line per mapped Figma component, §4 rewritten for `COMPONENTS`/`PAGES`/selector search/`PENDING`, §6 row for new pages |
| `CLAUDE.md` | Version line; §3 (link check, header tags), §4 (`scripts/`), §5 (commands) |

Before (main) → after: `2 checked, 11 not built yet, 0 pending` → `2 checked, 24 not built yet, 0 pending` (13 pages added).

## 3. Self-review

Checklist of CLAUDE §6.7 (with *engineering:code-review*):

| Area | Result |
|---|---|
| Security | Reads only files under `CLIENT/…/src/app`; no input from outside the repository; no new dependency |
| Correctness | Each rule and each failure is shown in the self-test. On the real tree: a probe file `shared/probe-notice.ts` with `selector: 'pa-notice'` failed the check, and the same file in `src/app/testing/` did not (both removed again). Without the second header tag the check failed |
| N+1, performance | 13 selectors × ~150 files, one regular expression each; well under a second |
| Accessibility | No UI change (comment line in `header.ts` only) |

Findings, sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `figmaTags()` reads every `@figma ` in the file, also in prose: a page whose JSDoc mentions "@figma lines" before its tag fails | *Won't fix*: it fails loudly, not silently; no page exists yet, and JSDoc tags start the line |
| R2 | The selector search also matches `selector: 'pa-…'` in a comment or a string | *Won't fix*: also fails loudly, and the message names the file |
| R3 | A test double for a mapped component outside `src/app/testing/` and `*.spec.ts` fails the check | *Won't fix*: that is where the conventions put test doubles (CLAUDE §3, `shell-test-doubles.ts`) |
| R4 | Selectors go into a `RegExp` unescaped | *Won't fix*: they are constants in the script, `[a-z-]` only |

## 4. Checks

| Check | Result |
|---|---|
| `node --test scripts/check-figma-links.test.mjs` | 10 passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 24 not built yet, 0 pending |
| `node --test scripts/check-i18n.test.mjs`, `node scripts/check-i18n.mjs` | 10 passed; 0 errors |
| `node scripts/check-invariants.mjs` (reads TESTING_v23) | passed |
| `npx eslint src/app/layout/header/header.ts` | clean |
| Vitest `layout/header/header.spec.ts` | 5 passed |
| `reuse lint` | compliant (666 / 666) |
| .NET build and tests, E2E | not run locally: no .NET or rendered client change; CI stages 2–6 |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | S34 Einschätzung has its own 360 px frame (`2136:3127`), but §2.2 names no component path for the child routes `pruefnachweis` and `einschaetzung`, so `PAGES` has only the S34 Überblick | Step 4 (workspace), when the child routes are built: add their paths to §2.2 and `PAGES` |
| P2 | The pages built so far (S04 start page, S80 error pages, legal pages) have no Figma frame in §2.2, so they stay unmapped | Philipp (design), if frames are drawn for them |
| P3 | For screens without a 360 px frame the base frame is the first node listed; for S35 that is the "gesperrt" state (`2023:633`) | Step 4/5 or design: change §2.2 and `PAGES` together if another frame is meant |
| P4 | TECH_DEBT_v1 lists #110 as open; like the other items closed in this sprint, it is marked done in TECH_DEBT_v2 at the sprint end (process step 14) | Sprint-02 close-out |
| P5 | The header of the Figma mapping still names `ux_copy_procureaccess_v12.md`, `ARCHITECTURE_v10.md`, `TESTING_v8.md` and handoff 1.12 as related documents | Next documentation session |

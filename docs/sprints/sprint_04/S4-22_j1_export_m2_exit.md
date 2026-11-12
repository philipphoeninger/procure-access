# S4-22 — J1 complete to the export; M2 exit check; bundle budget

> **Sprint:** 04, P0 task S4-22 (M2.7d, the end of M2). **Issues:** #42. **Requirements / invariants:** A11Y-02 (J1 anonymous part, complete); A11Y-01, A11Y-08, A11Y-09 stay green; no invariant changes status.
> **Status:** done — pull request opened, not merged.
> **Scope:** J1 extended from the hand-over through S11 and S13 to the downloaded XLSX, keyboard only, axe in both appearances; an XLSX reader for E2E; lazy-loaded feature routes and the tightened initial budget (#42); the M2 exit criteria with their state; SPEC v2.13 §11, TESTING_v52. Not here: the J1 save part (step 3), the reload flow after `release-mismatch` (O-S4-72), the 2026.1 priorities (O-1/K-1).
> **Owner decisions (2026-11-12, before the task, all (a)):** **O-S4-75** — load the feature pages with their route, then set the budget to the measured size plus a margin. **O-S4-76** — J1 exports XLSX and reads it with a small reader on Node's `zlib`, no dependency; it also checks the rows J1 changed (override, excluded criterion, note). **O-S4-77** — SPEC §11 step 2 lists A11Y-02 (SPEC v2.13).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | J1 ended at the S11 heading (S4-13); TESTING §5.4 wants the list, a priority change, an exclusion and the export with the downloaded file opened | J1 extended (§2) |
| F2 | Every route was imported eagerly: the release build had one initial chunk of **1.94 MB** (383.5 kB transferred) under the raised budget of 2 MB / 2.5 MB (S1-11); EasyMDE/CodeMirror of the legacy proposal pages were in it | O-S4-75: `loadComponent` for every feature route |
| F3 | `e2e/` has no XLSX reader; an XLSX file is a ZIP archive of XML parts | O-S4-76: `e2e/support/xlsx.ts` on `node:zlib` |
| F4 | ClosedXML writes every element with a namespace prefix (`<x:sheet>`, `<x:c>`); the first version of the reader expected none and found no sheet | The reader accepts any prefix; the self-test has a prefixed sheet |
| F5 | Firefox: once axe has run on S11, Playwright's name computation of a card's include checkbox (`aria-labelledby` on the input of `mat-checkbox`) crashes the page process — reproduced every time; buttons on S11 and the same name check without a prior axe run pass (probe test, not kept) | J1 finds the checkbox by role inside its card and compares its `aria-labelledby` texts (TESTING_v52 V165) — the class of V129 and S4-16 P5 |
| F6 | SPEC §11 step 2 lacked A11Y-02, which TESTING §9 lists (S4-07 P4) | O-S4-77: SPEC v2.13 |
| F7 | The release build warns „Could not parse 1 selector … 50%“ | Pre-existing (CodeMirror's `@-moz-keyframes`, S4-21b F8), not this task |

## 2. Changes

| File | Change | Reason |
|---|---|---|
| `CLIENT/CLIENT.ProcureAccess/src/app/app.routes.ts` | Every feature page (configurator S10/S11, legacy identity, settings, products, favorites, proposals, password reset, e-mail confirmation, login/register outlet) is `loadComponent`; the start page, the error pages and the legal pages stay eager; guards stay as they were | O-S4-75, #42 |
| `CLIENT/CLIENT.ProcureAccess/angular.json` | Initial budget **850 kB** warning / **1 MB** error (was 2 MB / 2.5 MB) | #42: measured 762.9 kB plus ≈ 11 % (warning) and ≈ 34 % (error) |
| `e2e/support/xlsx.ts` (new) | `readXlsxSheet(file, sheetName)` and `unzip(archive)`: stored and deflated ZIP entries from the central directory, the sheet found through `workbook.xml` and its relationships (relative or absolute target), shared/inline/formula strings, numbers, booleans, gaps by cell reference, namespace prefixes, XML entities | O-S4-76 |
| `e2e/support/xlsx-helper.spec.ts` (new) | Self-test on archives built in memory (no stack, no browser): every cell kind, a gap up to column AB, rich text, entities, a prefixed sheet with an absolute target and a stored entry, a missing sheet, a non-ZIP file | A reader that gets a cell wrong would let J1 pass |
| `e2e/journeys/j1-configurator.spec.ts` | After the hand-over: K5 menu of 99.1 A → „Hinweis“ (axe on the open menu; override line, „Empfehlung wiederherstellen“, focus back on the level button), detail panel of 99.1 („Warum diese Priorität?“, rationale, rule, source; axe), a note on 99.2, 99.7 left out (card kept, „9 von 10 Kriterien in Ihrer Liste“; axe), the „Geprüft nach BIK BITV-Test (Web)“ card, S13 on the preselected XLSX (scope text; axe), „Datei herunterladen“ → dialog closed, focus back on „Liste exportieren“; the file: name `ProcureAccess_Kriterien_T.1_<date>.xlsx`, header row of „Kriterien“, the keys in catalog order without 99.7, recommended „A – Mindestanforderung“ / chosen „Hinweis“ / rationale of 99.1, the note of 99.2, the title of `PROC-bikWeb`. Every control reached with Tab; timeout 240 s for 12 axe states in two appearances; `MAX_TABS` 80 | TESTING §5.4, O-S4-76 |
| `docs/TESTING_v52.md` (new), `docs/TESTING_v50.md` (deleted) | V163 – V166: J1 anonymous part, the reader, the Firefox crash class, the A11Y-02 row and the input documents | CLAUDE §2 (two newest versions) |
| `docs/SPEC_v2.md` | v2.13: §11 step 2 lists A11Y-02 (J1, anonymous part) | O-S4-77 |
| `docs/sprints/sprint_04/SPRINT_04.md`, `TASK_PROMPTS.md` | O-S4-75 – O-S4-77; the S4-22 prompt names them | Decisions |
| `CLAUDE.md` | §3 J1 to the export and the reader; lazy routes and budget; §4 layout | CLAUDE §11 |

ARCHITECTURE needs no new version: §8 already says „Routes become lazy-loaded per feature“. The Figma mapping, the UX copy and `de.json` are unchanged. `TECH_DEBT_v3` (#42, „end of M2“) is updated with the sprint's register review (process step 14).

**Bundle sizes (`ng build --configuration=production`):**

| | Initial raw | Initial transferred | Note |
|---|---|---|---|
| Before | 1.94 MB (`main` 1.89 MB + styles 51 kB) | 391.1 kB | all routes eager |
| After | **762.9 kB** (`main` 709.6 kB + styles 51.2 kB) | **173.7 kB** | lazy: `criterion-proposal` 342 kB (EasyMDE/CodeMirror), `products-container` 114 kB, `criteria-list` 82 kB, `criteria-select` 34 kB, 29 smaller chunks |

Left in the initial chunk (esbuild metafile): `@angular/core` 186 kB, `@angular/material` 175 kB, `@angular/cdk` 117 kB, `@angular/router` 104 kB, `marked` 59 kB + `ngx-markdown` 15 kB (legacy `provideMarkdown()` in `app.config.ts`, P4), `@angular/common` 36 kB, `rxjs` 31 kB, `@ngx-translate/core` 23 kB, app code of the shell and the root store.

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | Lazy routes: a tab left open across a deployment asks for chunk files the new image no longer has, so its next navigation to a page it has not loaded yet fails (Angular logs the navigation error; the click does nothing until a reload). Going offline before opening a page has the same effect | **Follow-up issue** (proposed, P3) | Today only staging deploys, and the page is reloaded after a deploy anyway; the remedy (a navigation error handler that loads the target URL in full) is a mechanism of its own |
| R2 | `marked` + `ngx-markdown` (≈ 75 kB) stay in the first download through the legacy `provideMarkdown()` | **Open point** (P4) | Moves with the legacy criteria list (`features/criteria/list/`), its only user |
| R3 | The XLSX reader parses XML with regular expressions and has no ZIP64 | **Won't fix** | A test helper for the API's own files, documented in its JSDoc, with a self-test; a file it cannot read fails the test loudly |
| R4 | J1 reads the include checkbox name through `aria-labelledby` instead of Playwright's accessible-name computation | **Won't fix** | Same name, no crash (F5); the component tests check the name with Testing Library; recorded as V165 to recheck with the next Playwright update |

Security: no production code path changes beyond route loading; the guards run before a lazy chunk is fetched, as before. A11y: route change, H1 focus and the page title are unchanged — `e2e/shell/route-change.spec.ts`, the reflow check and J1 pass with the lazy routes.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest) | 504 of 504 pass (incl. `app.routes.spec.ts` on the lazy table) |
| C2 | `npm run lint` (client); `npm run typecheck && npm run lint` (e2e) | Pass |
| C3 | `npm run build:release` | Pass without a budget warning (762.9 kB initial); F7 only |
| C4 | E2E on the E2E stack, Chromium, Firefox, mobile Chrome: the whole suite | 228 of 228 pass; J1 in Firefox 3 more times (`--repeat-each=3`): pass. The first run found F4 and F5. WebKit and mobile Safari are left to CI (no local WebKit) |
| C5 | `check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs`, `reuse lint` | Pass each / compliant |
| C6 | Backend | No backend change |

### M2 exit check

ROADMAP_v4 §M2 exit criteria: „journey J1 (anonymous part) passes keyboard-only and with axe; an exported file passes the screen-reader check (TESTING §7.2)“.

| # | Criterion / package | State |
|---|---|---|
| X1 | J1 anonymous part, keyboard only, axe in both appearances | **Met** with this task (C4; CI runs all five browser projects) |
| X2 | An exported XLSX passes the screen-reader check (TESTING §7.2) | **Open — the accessibility tester's item** (outside the sprint, SPRINT_04 „Outside the sprint“); EXP-02 checks the structure automatically (table, header row, no colour) |
| X3 | A11Y-07: manual screen-reader pass at the end of step 2 (TESTING §9, §7.2: the journeys of the step plus J1) | **Open — the tester**, together with X2 |
| X4 | M2.1, M2.2, M2.4, M2.5, M2.5d, M2.6, M2.6d, M2.7, M2.8 | Done (S3-11 – S3-21, S4-08 – S4-21b) |
| X5 | M2.7d: #42 bundle budget | Done with this task; the rest done in S4-19, S4-21a |
| X6 | M2.3: priorities, rationales and plain-language titles of 2026.1 | **Blocked on O-1/K-1** (project team); `catalog activate 2026.1` refuses the release until then, so dev and E2E run on `T.1` and staging on the legacy filter (O-S4-3) |
| X7 | M2.4d: API-06 (`Accept-Language`) | **Open** — S4-23 (stretch) not done yet |
| X8 | Step-2 invariants of TESTING §9 | Active: CAT-01, -02, -05 – -07, -09, -11 – -14, -16, -18 – -21, -23, CFG-01 – -04, EXP-01 – -05, -07, TEN-04 (catalog and export parts), API-05 (anonymous export), A11Y-02 (J1). Still `planned`: API-06 (X7), CAT-08 (the stored `ConfigurationCriterion` of step 3, S4-16), CAT-17 (rows 2/5/6 — the „Geprüft nach …“ card is shown and never assessed; the workspace and directory parts come with steps 5 and 6) |

M2 can be closed as a development milestone after X7 (or with API-06 moved), but not as the „first pilot-ready milestone“ (SPEC §12) before X2, X3 and X6.

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | Pull request opened; not merged | Owner |
| P2 | Screen-reader check of an exported XLSX and the A11Y-07 pass of step 2 (X2, X3) | Accessibility tester |
| P3 | Proposed issue: after a deployment, an open tab cannot load a lazy page it has not opened yet (R1) — a navigation error handler (`withNavigationErrorHandler`) that loads the target URL in full when a chunk fails to load | Owner: create the issue; before the first production release |
| P4 | `provideMarkdown()` (≈ 75 kB of the first download) moves to the route of the legacy criteria list, or goes with it | With the legacy filter (activation of 2026.1) |
| P5 | Firefox crash on the include checkbox name after axe (F5, V165); recheck with the next Playwright update, and consider a minimal report upstream | Next Playwright update |
| P6 | The tab labels of a card with two procedures at 320 px (S4-15b P3, S4-18a P6) are still not in the reflow check: J1's selection has one procedure per criterion | A test selection whose first card has two procedures, or with 2026.1 |
| P7 | API-06 (X7) and the 2026.1 priorities (X6) | S4-23; project team (O-1/K-1) |
| P8 | `TECH_DEBT_v3`: #42 resolved; ROADMAP M2 status | Sprint review (process step 14) |

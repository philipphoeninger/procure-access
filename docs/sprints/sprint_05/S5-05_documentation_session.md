# S5-05 — Documentation session: sprint-04 findings

> **Sprint:** 05 · **Task:** S5-05 (debt, 1 point) · **Issues:** #313 (`TECH_DEBT_v4.md` TD-50)
> **Requirements / invariants:** none (documents only); no invariant changes status. New versions `ARCHITECTURE_v49.md` (C135; v47 deleted), `TESTING_v57.md` (V178 – V180; v55 deleted), SPEC v2.17 in `SPEC_v2.md`, Handoff 1.25, Figma mapping 1.24
> **Status:** pull request #329 open (2026-11-13)
> **Scope:** the TD-50 points of #313 for SPEC (current-versions line), ARCHITECTURE (status line, input list), TESTING (§4.2a, §6.2), the Design handoff (`pa-stepper` for S10, S35, S55, the other sprint-04 changes of a screen, the body references) and the Figma mapping, with every header naming the current versions of the others; the `@spec` tag of `criteria-list-summary.ts`. ROADMAP_v5 and the SPRINT_04 close-out are not touched. PRD, Kriterienkatalog and UX copy are the owner's documents: their points are in §5. No rule or requirement changes beyond the listed points.
> **Owner decisions (2026-11-13, before the start):** **D1 (a)** — references without version: only a document's header names the versions of the others; text about the current state cites them by section, change or invariant number; applied where #313 points and where this task touches such text, no sweep of whole documents; change rows keep their versions; the rule goes into CLAUDE §2. **D2 (a)** — the rows O-S4-78 – O-S4-80 go into the decisions table of `SPRINT_04.md` (not the close-out). **D3 (a)** — S5-03 P2 (TESTING §8.2 stage 1 lacks „Check the SDK pin“) is included, because TESTING gets a version anyway. Plan stated before the change; one session, no EF migration, no screen built (so no Figma comparison).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Highest versions on `main` at `06793c9`: ARCHITECTURE v48, TESTING v56, SPEC v2.16, UX copy v26, Handoff 1.24, Figma mapping 1.23 | New files `ARCHITECTURE_v49.md`, `TESTING_v57.md` (third-newest deleted, CLAUDE §2); SPEC v2.17, Handoff 1.25, mapping 1.24 |
| F2 | SPEC's current-versions line is that of v2.7 (ARCHITECTURE v28, TESTING v40, UX copy v16, Handoff and mapping 1.16) | v2.17-1 |
| F3 | ARCHITECTURE's status line starts with „v33 —“ since v34 (v33 of S4-14 was the last correct one); the input list names `brainstorm_validierung_procureaccess_werteit.md` and `anforderungen_beschaffungstool_procureaccess_werteit_v2.md`, the files are `1_2_…_v2.md` and `1_1_…_v2.md` (S4-07 P3); the header names Handoff/mapping 1.19, UX copy v19 (language note), SPEC v2.12, TESTING v55; §13 names `TESTING_v10.md` | C135 |
| F4 | TESTING: input documents name ARCHITECTURE v47, UX copy v25, Handoff 1.18, mapping 1.19; the language note UX copy v18; §4.2a „UX copy v12 §5.4“; §6.2 lacks the short description of the MiniCatalog (S4-24 R4); §8.2 stage 1 lacks the step „Check the SDK pin“ (S5-03 P2, D3); PRV-07 cites „SPEC_v1 §8.4“ | V178 – V180 |
| F5 | Handoff: §2.2 and §5.1 name `mat-stepper` for S10, S35 and S55 (S4-10 P7, ARCHITECTURE C117); the sources name ARCHITECTURE v28, TESTING v40, SPEC v2.7, mapping 1.16, UX copy v16; the body names ARCHITECTURE_v11 (16 places, 3 of them in done rows of §9), TESTING_v9/v10, SPEC_v1 and UX copy v13 | Handoff 1.25 |
| F6 | Handoff vs. S10 as built in S4-10 – S4-12 (protocols F5, F7, F8 and the owner decisions O-S4-12, O-S4-14 – O-S4-20): the step list from 600 px with the check icon, focus and announcement on a step change, „Bisherige Auswahl“ as one entry per earlier step with its own „Ändern“ (SPEC R-CON-08; the Handoff had one row with one button), „Weiter“ waiting for current filter options, the server check of „Kriterienliste anzeigen“, app-type hints from the UX copy only, excluded options also in step 3 with the reason in place of hint and link, the product-part reason in its own line (U98), the step-2 hint `criteria.select.step2.hint.excluded` (U99), the reset notice only when a lower layer held a selection (O-S4-20), the data flow through `withConfigurator()` instead of the legacy `filters-selection` | Handoff §5.1 S10, §5.4 |
| F7 | Handoff S13: no word on `release-mismatch` (O-S4-72: `criteria.export.outdated` with „Schließen“ only); the initial focus named `cdkFocusInitial`, the dialog uses its `autoFocus` selector (S4-21b) | Handoff §5.1 S13 |
| F8 | A script (scratchpad, not committed; as S4-07 F7) compared every backticked key and key prefix of the Handoff with the key tables of UX copy v26 §5–§6: 245 checked, one missing — S41 named `directory.product.entryCount`, which UX copy v17 U89 dropped (S4-02) in favour of the three count keys of S40 | Handoff §5.1 S41; the sources line says the keys were checked against v26 |
| F9 | Handoff §3.4 describes `A11yAnnouncerService` without the shared throttle name (`throttleKey`, S4-17), the newest-text rule (S4-10) and the queue in call order, and says „UX copy v13“ | Handoff §3.4 |
| F10 | Figma mapping: the related line names Handoff 1.22, UX copy v22, SPEC v2.10, ARCHITECTURE v37, TESTING v48; its body already names no versions (S4-07) | Mapping 1.24 |
| F11 | `SPRINT_04.md` has no rows for O-S4-78 – O-S4-80 (S4-25 P5); the S4-24 protocol records only the chosen option (a) of each | Three rows that say so (D2) |
| F12 | `criteria-list-summary.ts` names the deleted `ux_copy_procureaccess_v22.md` in its `@spec` tag (S4-20a P5); no other code comment names a versioned document that is gone (`grep` over `CLIENT/…/src`, `src`, `API`, `e2e`, `scripts`, `tests`) | Tag names the file pattern |
| F13 | S5-07 left two version records out: TESTING's status line has no clause for v56, and SPEC has no „Changes in v2.16“ table (§8.8 and the `X-Forwarded-Proto` sentence of §7.3 came with it). ARCHITECTURE C133 and C134 sit in the table headed „Changes from v45“ | Not in scope (a sprint-05 finding); §5 P2 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docs/ARCHITECTURE_v49.md` (new, copy of v48) | Status line „v49 —“, „Supersedes v48“ and a v49 clause; input list with the real file names; header Handoff 1.25, mapping 1.24, SPEC v2.17 (change rows „from v2.1“), `TESTING_v57.md`; language note without UX copy version; §13 names the testing strategy as `TESTING_v*.md`. New section „Changes from v48“ with **C135** | TD-50 (F3); D1 |
| `docs/ARCHITECTURE_v47.md` | Deleted (third-newest; in the git history) | CLAUDE §2 |
| `docs/TESTING_v57.md` (new, copy of v56) | Status line „v57 —“ with a v57 clause; input documents ARCHITECTURE v49, UX copy v26, Handoff 1.25, mapping 1.24, SPEC v2.17; language note and §4.2a heading without UX copy version; PRV-07 cites SPEC §8.4 (wording otherwise unchanged); §6.2 MiniCatalog: only `99.1` has a short description, in both releases; §8.2 stage 1 lists „Check the SDK pin“. „Changes from v56“: **V178** upkeep, **V179** §6.2, **V180** stage 1 | TD-50 (F4); S4-24 R4; S5-03 P2 (D3) |
| `docs/TESTING_v55.md` | Deleted (third-newest) | CLAUDE §2 |
| `docs/SPEC_v2.md` | Status v2.17; current-versions line (v2.17); „Changes in v2.17“ with **v2.17-1** | TD-50 (F2) |
| `docs/design_handoff_procureaccess_v1.md` | v1.25: sources (UX copy v26 with the key check, ARCHITECTURE v49, TESTING v57, SPEC v2.17, mapping 1.24); language line without UX copy version; the body references of F5 without version (rows of §9 that record when an item was done keep theirs); `pa-stepper` in §2.2, S10, S35, S55; S10 row and §5.4 as built (F6); S13 (F7); S41 count keys (F8); §3.4 (F9) | TD-50; S4-10 P7; F6 – F9; D1 |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.24: related line Handoff 1.25, UX copy v26, SPEC v2.17, ARCHITECTURE v49, TESTING v57 | TD-50 (F10) |
| `docs/sprints/sprint_04/SPRINT_04.md` | Rows O-S4-78 – O-S4-80 in the owner-decisions table, marked as added in S5-05 with only the chosen option recorded; the close-out is not touched | D2 (F11); S4-25 P5 |
| `CLAUDE.md` | §2: the rule for references without version (D1); version line | D1; DoD |
| `CLIENT/…/criteria-list-summary/criteria-list-summary.ts` | `@spec` names the UX copy by its file pattern instead of the deleted v22 | S4-20a P5 (F12) |

What the rule of D1 changes in practice: the Handoff and the mapping already cited by section since S4-07; with this PR the rule is written down in CLAUDE §2, the remaining places #313 names follow it, and the headers carry the versions. Section, change and invariant numbers were checked against the current files before a version was dropped (every ARCHITECTURE section and change number the Handoff cites exists in v49; TESTING §7.2 and §8.2, SPEC §8 and §8.4 too).

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y. For a documentation PR: does every new statement match the code, the protocols and the owner decisions, and does the PR stay in scope? N+1 and a11y: not applicable (no behaviour changes). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The Handoff's S10 and §5.4 statements were checked against the code (`pa-stepper` template and class comment, `criteria-select.html`, `pa-filter-option`) and the S4-10 – S4-12 protocols, not only against CLAUDE §3 | Kept |
| R2 | The first SPEC draft added „only this line names their versions“ to the current-versions line; that is not true for the SPEC body (e.g. §7.6 still says the e-mail dependency „is not yet in `ARCHITECTURE_v10`“), and D1 (a) does not sweep it | Fixed before the commit: the line only lists the versions; the stale sentence is §5 P3 |
| R3 | The S41 fix (F8) is a sprint-04 change of a screen (UX copy v17 U89, S4-02), found by the key check, not named in #313 | Kept: the prompt asks for „every other sprint-04 change of a screen“ in the Handoff |
| R4 | Handoff §3.4 now also describes the queue in call order, which came with S2-17, not sprint 04 | Kept: one sentence that makes the row describe the service as built; recorded in the 1.25 clause |
| R5 | The missing version records of S5-07 (F13) would be cheap to add, but they record another task's changes | Won't fix here: §5 P2 |
| R6 | Version numbers may collide with M3 pull requests that write ARCHITECTURE, TESTING or SPEC versions in parallel | Follow-up: whichever merges second renumbers (prompt step 3); §5 P5 |
| R7 | Security: no secrets, no configuration, no behaviour changed; the code change is a comment | – |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` | ✅ passed, reads `docs/TESTING_v57.md` (137 invariants, 53 active — as in v56; the catalogue differs from v56 only in the reference of PRV-07) |
| 2 | `node scripts/check-figma-links.mjs` | ✅ 6 checked, 20 not built yet, 0 pending |
| 3 | `node scripts/check-i18n.mjs` | ✅ 0 errors (client 317 keys, export texts 35 keys, UX copy v26) |
| 4 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 1301/1301 |
| 5 | Handoff keys against UX copy v26 §5–§6 (scratchpad script, F8) | ✅ 245 keys and prefixes; the only hit left is the dropped key named in the 1.25 clause on purpose |
| 6 | ESLint and Prettier on `criteria-list-summary.ts` | ✅ |
| 7 | Build, .NET tests, Vitest, E2E | – no behaviour touched (one comment); CI runs them on the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | **Owner documents (German), not edited here** — for #259 or the next version of each: (a) UX copy: `criteria.priority.change` is unused, and the hint of `criteria.priority.changeLabel` says „Label Select“ while K5 is a menu — drop `change` or give it a use, hint „Name des Menüs“ (S4-16 P4); §5.4 describes „Bisherige Auswahl“ with one „Ändern“, while SPEC R-CON-08 and the code have one per earlier step (S4-12 F7); the header's current versions are SPEC v2.15, ARCHITECTURE v43, TESTING v53, Handoff 1.24, mapping 1.23. (b) PRD v5 header: ARCHITECTURE_v10, SPEC_v1, TESTING_v8, Handoff 1.12, UX copy v12. (c) Kriterienkatalog header: ARCHITECTURE_v10, TESTING_v8, UX copy v12; §3.1, the table of §7 and K-3 in §8 still name `SampleData.cs` for the HR → Mobile exclusions, removed since S3-18 (migration `RemoveHrMobileExclusions`). With D1 the bodies of these documents could also cite without version | Owner (#259) |
| P2 | S5-07 left out two version records (F13): a v56 clause in TESTING's status line and a „Changes in v2.16“ table in SPEC (§8.8 session probe, sign-in and sign-out; §7.3 `X-Forwarded-Proto`). ARCHITECTURE C133 and C134 stand in the table headed „Changes from v45“ | Next document change of SPEC and TESTING (e.g. S5-08), or the next documentation session |
| P3 | Versioned references about the current state that D1 (a) left as they are (no sweep): SPEC §7.6 says the e-mail dependency „is not yet in `ARCHITECTURE_v10` (§14)“, while ARCHITECTURE has it since v12; the „Before“ column of Handoff §9 names `SPEC_v1` for the open item H13; the example tag in Figma mapping §3 names `docs/design/design_handoff_procureaccess_v1.md`, the file is `docs/design_handoff_procureaccess_v1.md` | Next content version of each document |
| P4 | TD-50 (#313) is resolved by this PR for Claude's part; the register records it in its next version | Sprint 05 close-out (TECH_DEBT_v5) |
| P5 | ARCHITECTURE v49, TESTING v57 and SPEC v2.17 may collide with an M3 pull request that writes the same versions: whichever merges second renumbers its versions and the header lines that name them (Handoff, mapping, SPEC, CLAUDE §2 needs no change) | The second pull request to merge |

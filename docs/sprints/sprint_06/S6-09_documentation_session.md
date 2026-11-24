<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-09 — Documentation session: sprint-05 and sprint-06 findings

> **Sprint:** 06, task S6-09 (debt, 1 point). **Issues:** #389 (`TECH_DEBT_v5.md` TD-78).
> **Requirements / invariants:** none (documents and comments only); no invariant changes status. New versions `ARCHITECTURE_v107.md` (C195; v105 deleted), `TESTING_v114.md` (V416, V417; v112 deleted), SPEC v2.69 in `SPEC_v2.md`, Handoff 1.46, Figma mapping 1.36; `CODEBASE_v1.md` §1 and new §2.0.
> **Status:** done by Claude Code (2026-11-24); pull request open. One session. No EF migration, no new dependency, no screen built (so no Figma comparison). Build, .NET tests, Vitest and E2E were not run: only comments changed in code; CI runs them.
> **Scope:** the TD-78 points of #389 and the open points of the sprint-05 and sprint-06 protocols that name a TESTING, SPEC, ARCHITECTURE, Handoff or Figma-mapping change or the documentation session: the headers name the current versions; the code comments that name deleted documents cite by section or change number. Not in scope (in the owner's control chat): ROADMAP v7, the SPRINT_06 close-out, and `TECH_DEBT_v6.md` (process step 14 at the sprint end; §5 P2 lists what it should record). PRD, Kriterienkatalog and UX copy are owner documents (TD-79): §5 P1 lists the proposed changes.
> **Owner decisions (2026-11-24, with the plan):** **1 (a)** — delete `ROADMAP_v2.md` – `ROADMAP_v4.md`; v5 and v6 stay for v7. **2 (a)** — record the six open questions of the protocols in SPEC §13 (as ARCHITECTURE §16.5 O-25 – O-29, and the restore of a deleted account in O-4). **3 (a)** — annotate the S5-28 row of `SPRINT_05.md` and its task prompt.

## 1. Starting point

`main` at `163418e5` (#438 merged).

| # | Finding | Consequence |
|---|---|---|
| F1 | Highest versions: ARCHITECTURE v106, TESTING v113, SPEC v2.68, UX copy v48, Handoff 1.45, mapping 1.35, CODEBASE v1, TECH_DEBT v5, ROADMAP v6 | New files ARCHITECTURE v107, TESTING v114 (third-newest deleted); SPEC v2.69, Handoff 1.46, mapping 1.36 |
| F2 | ARCHITECTURE: status line „v67 — … Supersedes v67“; inputs name UX copy v41, Handoff 1.30, mapping 1.25, SPEC v2.25, `TESTING_v109.md`; C136 (S5-08) under „Changes from v48“, C134 (S5-03) and C133 (S5-07) under „Changes from v45“ (S5-05 P2) | C195 |
| F3 | TESTING: status line „v74 —“, no v56 clause (S5-05 P2), no clause since v93 (later versions record themselves only in „Changes from“); inputs ARCHITECTURE v105, Handoff 1.45, mapping 1.35, SPEC v2.67; §3.3 says the clock and the relaxed rate limits do not exist (S5-11 P7); §8.2 names neither the dev-server smoke nor the cache-header check (S6-07 P1) nor the backup self-test (S6-29) | V416, V417 |
| F4 | SPEC: current-versions line of v2.67 (ARCHITECTURE v105, TESTING v110, S6-03 P3, S6-27 P4); no „Changes in v2.16“ (S5-07); §7.6 „not yet in `ARCHITECTURE_v10`“ (S5-05 P3); R-SHL-07 names `content/legal/*.de.md` without its folder (S6-28 P3); §13 holds none of the questions the sprint-05/06 protocols left for the owner | v2.69-1 – 4 |
| F5 | Handoff: sources UX copy v26, ARCHITECTURE v49, TESTING v57, SPEC v2.17, mapping 1.27; §5.1 S34a says the tabs are links with `aria-current` (S6-16 P3); §5.3 rows say „until S5-21“ (saved list), „until S6-14“ (S33, S34) and „until S6-17/S6-19/S6-22“ (S34, S34a) for what was built since; §9 H13 „Before: SPEC_v1“ (S5-05 P3) | Handoff 1.46 |
| F6 | Handoff keys against UX copy v48 and `de.json` (scratchpad script, as S5-05 F8): 486 keys and key prefixes, 4 hits — all wildcards in the middle of a key (`signals.source.*.explain`, `legal.*.title`, `criteria.select.step2.*.hint`), which resolve | No change |
| F7 | Figma mapping: related line Handoff 1.36, UX copy v41, SPEC v2.52, ARCHITECTURE v90, TESTING v95; the §3 example names `docs/design/design_handoff_procureaccess_v1.md`, and the `@spec` rule says the documents „move to `docs/design/`“ — they never did, and every tag names `docs/design_handoff_procureaccess_v1.md` (S5-05 P3) | Mapping 1.36 |
| F8 | Code naming deleted versions: `reflow.spec.ts` (TESTING_v26), `MappingCompletenessTests.cs` (TESTING_v27), `ProblemDetailsTests.cs` (TECH_DEBT_v1), `cat-10-criteria-filter.json` (ARCHITECTURE_v18), `FilterExclusion.cs` (ARCHITECTURE v7), `criterion-card-rules.ts` (UX copy v18), `criterion-detail-rules.ts` (UX copy v19), `edit-project-dialog.ts` (UX copy v44). `s11-list.spec.ts:18` and `IdentityAccessModule.cs:34` (TD-78) no longer name one. SPEC change-row IDs (`SPEC v2.5-3`, `v2.56-1`) are stable numbers, like C- and V-numbers, and stay | Eight comments |
| F9 | `CLAUDE.md`: 39 957 characters (40 185 bytes); its §3 paragraph „State of the code“ was 3 270 characters and ended with „Next: migration step 1“; the document table did not name `TECHNICAL.md` (S6-29 P6) | Paragraph moved to CODEBASE §2.0 |
| F10 | `docs/` held ROADMAP v2 – v6; the S5-28 row of `SPRINT_05.md` and its task prompt still asked for „change e-mail and password“ (S5-28a P5) | Decisions 1 (a), 3 (a) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docs/ARCHITECTURE_v107.md` (new, copy of v106) | Status line „v107 — … Supersedes v106“ with a v107 clause; inputs UX copy v48, Handoff 1.46, mapping 1.36, SPEC v2.69, `TESTING_v114.md`, `CODEBASE_v1.md`; headings „Changes from v49“ (C136), „from v47“ (C134), „from v46“ (C133), each with its task line; §16.5 O-25 – O-32 and the restore of a deleted account in O-4; „Changes from v106“ with **C195** | F2; decision 2 (a) |
| `docs/ARCHITECTURE_v105.md` | Deleted (third-newest) | CLAUDE §2 |
| `docs/TESTING_v114.md` (new, copy of v113) | Status line „v114 (…) —“ with the note that versions since v93 are recorded in their sections only; the v56 clause; inputs ARCHITECTURE v107, Handoff 1.46, mapping 1.36, SPEC v2.69, `CODEBASE_v1.md`; §3.3 state note; §8.2 stage 1 (backup self-test, dev-server smoke) and stage 6 (cache-header check); „Changes from v113“ with **V416**, **V417** | F3 |
| `docs/TESTING_v112.md` | Deleted (third-newest) | CLAUDE §2 |
| `docs/SPEC_v2.md` | Status v2.69; „Combines“ marked as the inputs of v1; current-versions line (v2.69); „Changes in v2.16“ (v2.16-1, v2.16-2, recorded afterwards); „Changes in v2.69“ (v2.69-1 – 4); §7.6 sentence; R-SHL-07 folder; §13 O-25 – O-29 and O-4 | F4; decision 2 (a) |
| `docs/design_handoff_procureaccess_v1.md` | v1.46: sources; §5.1 S34a tabs as built; §5.3 rows „Saved list“ (the editing of S5-21b, decisions S5-21b 1a, 2a, 5b, 6a), „S33 as built“, „S34 as built“, „S34a as built“ without the „until …“ of later tasks; §9 H13 | F5, F6 |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.36: related line; §3 example tag and `@spec` rule | F7 |
| Eight code comments (F8) | Cite by section, change or U-number: TESTING V87, TESTING §4.10, ARCHITECTURE C102, ARCHITECTURE C49 (review finding Y5), UX copy U100, U101, U177; the former ad-hoc error object by its register and roadmap item (M1.3) | TD-78; CLAUDE §2 |
| `CLAUDE.md` | §3 „State of the code“ shortened to the step status and the pointer to CODEBASE §2.0 (now 37 491 characters); §2 document table names `TECHNICAL.md`; version line | TD-78 (size); S6-29 P6 |
| `docs/CODEBASE_v1.md` | New §2.0 with the paragraph moved verbatim (without its last two sentences, which the new CLAUDE paragraph keeps or which were stale) and the sprint-06 tasks it did not name; §1 records the change of `CLAUDE.md` | TD-78 |
| `docs/ROADMAP_v2.md` – `ROADMAP_v4.md` | Deleted | Decision 1 (a) |
| `docs/sprints/sprint_05/SPRINT_05.md`, `TASK_PROMPTS.md` | A bracketed note at „change e-mail and password“ of S5-28: dropped by decision S5-28 1a, see ARCHITECTURE O-25 | Decision 3 (a) |

What C195 / V416 / v2.69 / 1.46 do not change: no rule, requirement, acceptance criterion, invariant or design. The §16.5 rows record questions; none is decided.

## 3. Self-review (CLAUDE §6.7)

For a documentation pull request: does every new statement match the code, the protocols and the owner decisions, and does it stay in scope? N+1 and a11y: not applicable.

| # | Finding | Sorting |
|---|---|---|
| R1 | The TESTING §3.3 note was checked against `docker-compose.e2e.yml` (the relaxed limits, `TestClock__Enabled`, the API still in `Development`) and §8.2 against `ci.yml` (step names, `scripts/backup-restore.test.mjs`, the cache-header step only for `chromium`) | Kept |
| R2 | The Handoff's saved-list row was checked against the S5-21b protocol and its decisions; S02, S03, S20, S21, the invitation page and the header states were built from the §5.3 patterns the Handoff already names, so they get no „as built“ row here | Kept; no further sweep |
| R3 | ARCHITECTURE O-31 (lock and copy not atomic) was not in the plan; the plan named only S6-12a P2 and S6-28 Q1 for §16.5. It is the S6-18 open point 1 that S6-20a left as „won't fix“ (R2 there) without a decision, so it belongs with the other open points | Kept; named as a deviation |
| R4 | ARCHITECTURE has no heading „Changes from v41“ either (C128 and C129 stand under „from v42“, sprint 04) | Won't fix: sprint-04 drift, not in #389; P4 |
| R5 | The new CLAUDE state paragraph says „steps 0 and 4 are done“; the old one said step 0 done and step 4 done (S5-30), parts of steps 1–3 built | Kept: same facts, plus step 5 in progress |
| R6 | Security: no secrets, no configuration, no behaviour; the JSON fixture's description changed, its data did not (parsed after the edit) | – |

No correctness or security finding that affects behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` | ✅ passed, reads `docs/TESTING_v114.md` (catalogue unchanged from v113) |
| 2 | `node scripts/check-figma-links.mjs` | ✅ 12 checked, 14 not built yet, 0 pending |
| 3 | `node scripts/check-i18n.mjs` | ✅ 0 errors (client 721 keys, export 35, e-mail 19; UX copy v48) |
| 4 | `node scripts/check-icons.mjs` | ✅ 0 errors |
| 5 | `reuse lint` | ✅ compliant, 1758/1758 |
| 6 | ESLint on the three client files; `npm run lint` in `e2e/` | ✅ |
| 7 | `dotnet format whitespace --verify-no-changes` on the three C# files (SDK 10.0.404) | ✅ |
| 8 | Handoff keys against UX copy v48 and `de.json` (scratchpad script, F6) | ✅ 486, none missing |
| 9 | References to the deleted files outside history (`git grep`) | ✅ none |
| 10 | Build, .NET tests, Vitest, E2E | – comments only; CI runs them |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | **Owner documents (TD-79), not edited here.** **UX copy:** (a) still from TD-79: v31 U133 – U137 and v33 U140 – U144 are marked „Entwurf“; U24 names „S11 (Details)“; U130 says S20 for `org.left`; no `errors.generic` (legacy `ERROR.GENERIC`); `criteria.share.expiry.hint` writes the 🔧 values; S16's excluded card „Nicht in Ihrer Liste“; the organisation-switch announcement; `criteria.priority.change` unused and the `changeLabel` hint; §5.4 „Ändern“. (b) New from sprint 06: §6.3 has no row for `test-status-conflict` (S6-15 P1); the S32 scope row reads „1 Mindestanforderungen (A)“ with a count of one (S6-14 P2); German texts for the date picker's calendar (`MatDatepickerIntl`, S6-17 P2); no text for a failed autosave (S6-19a P2); `assessment.procedure.derived` is unused (S6-19a P3); confirm `candidate.assessment.list` (S6-19b P1); an hours variant of `errors.rateLimit` for day windows (S6-26 P2); `criteria.save.outdated.reload` for S12 (S6-27 P1); texts for ARCHITECTURE O-29 (S14 delete after a conflict) if wanted; the header's current versions. **PRD v5** header: ARCHITECTURE_v10, SPEC_v1, TESTING_v8, Handoff 1.12, UX copy v12. **Kriterienkatalog** header (ARCHITECTURE_v10, TESTING_v8, UX copy v12) and §3.1, §7, K-3 still naming `SampleData.cs` | Owner (#259), next version of each |
| P2 | **For `TECH_DEBT_v6.md` (process step 14):** TD-78 done by this pull request; TD-65 (S6-03 P2), TD-57 – TD-59 (S6-05 P1), TD-60 and #318 (S6-26 P1), TD-76, TD-17 and TD-51's handling (S6-08 P3), TD-61 and the §5 rows S6-23 P7 names, the §5 rows of S6-24 P2 (`LEGACY_EXCEPTIONS` 16 → 7), the step-5 part of „Account deletion parts of later steps“ (S6-25 P4), TD-29's legal-page part (S6-28 P2), the backup part of #4 (S6-29 P7); new watch row picomatch GHSA-fc5q-39xc-vjpf (S6-08 P1). Several protocols pointed these at S6-09; the register is the sprint-end task | Sprint close-out |
| P3 | #389 closes with this pull request. The sprint-05 points of TD-78 and the S6 documentation points are done; the owner documents are P1 | – |
| P4 | Drift found outside #389, left as it is: ARCHITECTURE has no heading „Changes from v41“ (sprint 04, R4); the TESTING and SPEC status lines no longer list each version since v93 / v2.16 in prose (V416 says so for TESTING) | Next documentation session, if wanted |
| P5 | Version numbers may collide with a pull request merged first that writes ARCHITECTURE v107, TESTING v114, SPEC v2.69 or Handoff 1.46: whichever merges second renumbers its versions and the header lines that name them | The second pull request to merge |
| P6 | The task fitted one session | – |

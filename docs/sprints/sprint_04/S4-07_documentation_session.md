# S4-07 — Documentation session: sprint-03 findings

> **Sprint:** 04, P0 task S4-07 (documentation, 1 point). **Issues:** #243 (`TECH_DEBT_v3.md` TD-37). **Requirements / invariants:** none (documents only); no invariant changes status. **Status:** done by Claude Code (2026-11-10), pull request #258 open, not merged.
> **Scope:** the TD-37 points the task prompt lists for ARCHITECTURE (status line, `Category` in §5.1), TESTING (input line, §8.2 stage 9 and V85 against the ruleset), SPEC (R-SHL-10, §11 step 1, R-SET-03, current-versions line), the Design handoff (UX copy version, §1.3 subsets, sources line) and the Figma mapping (body and related line). ROADMAP_v4 and the SPRINT_03 close-out were already done and are not touched. PRD, Kriterienkatalog and UX copy are the owner's documents: their points are listed in §5. No rule or requirement changes beyond the listed points.
> **Owner decisions:** none for this task. Before the start, the owner had no objection to four defaults Claude named: SPEC as v2.7 in `SPEC_v2.md` (renumbered if the draft PR #248, which also writes a v2.7, merges first); criterion splits (TD-31, #238) left out; #243 closed by this PR with the owner-document points in §5 and a proposed follow-up issue; the third-newest versions deleted (CLAUDE §2).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Highest versions on `main` at `e35e22d`: ARCHITECTURE v27, TESTING v39, SPEC v2.6 (`SPEC_v2.md`), UX copy v16, Handoff 1.15, Figma mapping 1.15. Draft PR #248 (S4-02) writes UX copy v17 and SPEC v2.7 | New files `ARCHITECTURE_v28.md`, `TESTING_v40.md` (`ARCHITECTURE_v26.md` and `TESTING_v38.md` deleted, CLAUDE §2); in-file versions SPEC 2.7, Handoff and mapping 1.16; this PR names UX copy v16, the newest on `main` |
| F2 | ARCHITECTURE v27's status line starts with „v26 —“ and says „Supersedes v26“; its header names Handoff/mapping 1.15, UX copy v15 (language note), SPEC v2.5 and TESTING v37 | C113 |
| F3 | `Criterion.Category` in §5.1 has no meaning (S3-12 P5). Code: `Criterion.Category`, `string?`, ≤ 128 (`Criterion.CategoryMaxLength`); the release format calls `category` the chapter (S3-15 §2.1); release 2026.1 sets it from the „Kapitel“ headings of Kriterienkatalog Teil A (S3-27 §2.1), 122 of 188 criteria; the import refuses a changed category for a stored criterion (§5.1 „Import“, C105); no endpoint sends it (C109) | C114 and the §5.1 row: describes the state as built |
| F4 | The ruleset „Main protection“ (read through the GitHub API on 2026-11-10) has a `code_scanning` rule: tool CodeQL, `alerts_threshold` `errors`, `security_alerts_threshold` `high_or_higher`, next to the required checks of stages 1–7 and the three `Analyze (…)` jobs. TESTING v39 §8.2 stage 9 („findings: not yet“) and the ruleset note still describe the state before S3-03 P1 | V120; CLAUDE §3 |
| F5 | SPEC: R-SHL-10 names only the manual check; R-SET-03 says „TESTING follow-up, §14“, which A11Y-08 now covers; §11 step 1 lacks API-05 (registration, password reset), A11Y-08 and A11Y-09 against TESTING §9 step 1; the current-versions line is that of v2.4 | v2.7-1 – v2.7-4 |
| F6 | Handoff: „UX copy v14“ in §0, §5.3 and §7; §1.3 says „latin + latin-ext“, while `angular.json` loads `@fontsource/roboto/400.css` and `500.css` with every subset (S3-09 F8, R2, P5); the sources name ARCHITECTURE v16, TESTING v26, SPEC v2.3, mapping 1.15, UX copy v15 with keys last checked against v14 | Handoff 1.16 |
| F7 | A script (scratchpad, not committed) compared every backticked key in the Handoff with the key tables of UX copy v16 §5–§6: all 205 keys and key prefixes are there | The sources line says the keys were checked against v16 |
| F8 | Figma mapping: the body names `ARCHITECTURE_v10` (§1, §2.2, K6 row), `TESTING_v8` (§3, twice) and `TESTING_v23` (§4); the related line names Handoff 1.15, UX copy v15, SPEC v2.3, ARCHITECTURE v16, TESTING v26 | Mapping 1.16 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docs/ARCHITECTURE_v28.md` (new, copy of v27) | Status line starts with „v28 —“, „Supersedes v27“, v28 clause; header: Handoff and mapping 1.16, UX copy v16 in the language note, SPEC v2.7, TESTING v40. „Changes from v27“: **C113** document upkeep, **C114** `Category`. §5.1 row `Criterion`: `Category?` with its meaning | TD-37 (F2, F3) |
| `docs/ARCHITECTURE_v26.md` | Deleted (third-newest; in the git history) | CLAUDE §2 |
| `docs/TESTING_v40.md` (new, copy of v39) | Status line starts with „v40 —“ with a v40 clause; input documents ARCHITECTURE v28, SPEC v2.7, Handoff and mapping 1.16; language note UX copy v16. „Changes from v39“: **V119** document upkeep, **V120** CodeQL findings block. §8.2 stage 9 „Blocking“ and the ruleset note under „State after step 0“ follow the ruleset. V85 stays as written (history); V120 says it replaces its „findings: not yet“ | TD-37 (F4) |
| `docs/TESTING_v38.md` | Deleted (third-newest) | CLAUDE §2 |
| `docs/SPEC_v2.md` | Status v2.7; „Changes in v2.7“ (v2.7-1 – v2.7-4); current-versions line (v2.7); R-SHL-10 first criterion „A11Y-09 for the pages of the new shell and every rebuilt screen; legacy screens: manual check per TESTING §7.2“; R-SET-03 „(A11Y-08)“ instead of „(TESTING follow-up, §14)“; §11 step 1 adds API-05 (registration, password reset), A11Y-08 (system appearance), A11Y-09 (shell pages), as TESTING §9 lists them | TD-37 (F5); S3-05 P4, S3-10 P3 |
| `docs/design_handoff_procureaccess_v1.md` | v1.16: sources UX copy v16 (keys checked, F7), ARCHITECTURE v28, TESTING v40, SPEC v2.7, mapping 1.16; language line UX copy v16; §0, §5.3, §7 refer to „the UX copy“ without a version; §1.3 names the per-weight files with every subset | TD-37 (F6, F7); S3-09 P5 |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.16: related line Handoff 1.16, UX copy v16, SPEC v2.7, ARCHITECTURE v28, TESTING v40; the body cites ARCHITECTURE §12, §8.1, C46 and TESTING §4, §8.1, §8.2 without a version | TD-37 (F8) |
| `CLAUDE.md` | Version line; §3 CodeQL sentence: the code-scanning rule of the ruleset blocks a pull request that introduces an alert of the named severities | DoD (CLAUDE §3) |

The body references of the Handoff (§0, §5.3, §7) and of the mapping now name the document without a version, so only the header pins the version. This is why the same drift came back after S3-05 (S3-05 P2): every new version of ARCHITECTURE, TESTING or the UX copy made the body references stale. Section and change numbers (`§8.1`, `C46`) stay stable across versions.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y. For a documentation PR this means: does each new statement match the code, the ruleset and the source protocols, and does the PR stay in scope? N+1 and a11y: not applicable (no code, no UI). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | SPEC §11 step 1 now also lists API-05 (registration, password reset). The prompt names „§11 step-1 invariants incl. A11Y-08/09“; API-05 was missing in the same way (TESTING §9 step 1 lists it), so the row now matches TESTING §9 in full. The mapping table is not a rule, and API-05 itself is unchanged (`planned`) | Kept; recorded in v2.7-3 |
| R2 | V120 says an alert that already exists on `main` blocks no other pull request. This is GitHub's documented behaviour of the code-scanning rule (it evaluates the alerts a pull request introduces), as S3-03 P1 already noted for alert 3. No pull request was opened to prove it | Kept; no proof needed for a documentation statement that names the rule's settings |
| R3 | ARCHITECTURE C109 (history row, v24) says „the revision's own `RationaleDe` and `Category` are not sent“, but `Category` is a property of `Criterion`, not of the revision (C101 lists it with the revision's optional fields as well). C114 and §5.1 state it correctly; change rows are history and are not rewritten | Won't fix (history); C114 is the current statement |
| R4 | SPEC v2.7 collides with the draft PR #248, which also writes „v2.7“ | Follow-up: whichever PR merges second renumbers (prompt step 3, sprint risk R5); §5 P1 |
| R5 | Security: no secrets, no code, no configuration changed. The ruleset was only read | – |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` | ✅ passed, reads `docs/TESTING_v40.md` (catalogue unchanged) |
| 2 | `node scripts/check-figma-links.mjs` | ✅ 2 checked, 24 not built yet, 0 pending |
| 3 | `node scripts/check-i18n.mjs` | ✅ 0 errors (109 keys, UX copy v16) |
| 4 | Self-tests `check-invariants` / `check-figma-links` / `check-i18n` | ✅ 36/36, 10/10, 12/12 |
| 5 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 1072/1072 (`docs/**` covered by `REUSE.toml`) |
| 6 | Handoff keys against UX copy v16 §5–§6 (script in the scratchpad, F7) | ✅ 205/205 |
| 7 | Ruleset read through `gh api repos/philipphoeninger/procure-access/rulesets/<id>` (F4) | ✅ code-scanning rule present |
| 8 | Build, .NET tests, Vitest, ESLint, E2E | – no code touched; CI runs them on the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | SPEC v2.7 here and in the draft PR #248 (S4-02): whichever merges second renumbers its change table to v2.8, and if #248 merges first, this PR's current-versions line names UX copy v17 (and the Handoff/mapping sources move with it) | The second PR to merge |
| P2 | **Owner documents (German), not edited here:** (a) `prd_procureaccess_werteit_v5.md` header names ARCHITECTURE_v10, SPEC_v1 and UX copy v12; (b) `kriterienkatalog_bit_inklusiv_v1.md` header names ARCHITECTURE_v10, TESTING_v8, Handoff 1.12 and UX copy v12, and §3.1 („Entschieden …“) and row K-3 still name `SampleData.cs` for the two HR → Mobile exclusions, which were in `CriteriaFilterExclusionSeed.cs` and are removed since S3-18 (migration `RemoveHrMobileExclusions`; S3-11 P5, S3-18 P1); (c) the UX copy header names ARCHITECTURE v11/v12, SPEC v1/v2 and TESTING v10 (with #181 / S4-02) | Owner, next version of each document. Issue #259 („Owner documents: headers and Kriterienkatalog §3.1 after sprint 03“, milestone M2), so #243 can close with this PR |
| P3 | Body references with old versions outside the listed points: the Handoff body names ARCHITECTURE_v11, TESTING_v9/v10 and SPEC_v1 (§4, §5, §7, §8, §9); TESTING §4.2a names „UX copy v12 §5.4“; the ARCHITECTURE input list names `brainstorm_validierung_procureaccess_werteit.md` and `anforderungen_beschaffungstool_procureaccess_werteit_v2.md`, while the files are `1_2_…_v2.md` and `1_1_…_v2.md` | Next content version of each document; versionless references as in the mapping (§2) would stop the drift |
| P4 | SPEC §11 step 2 lists `CAT-*, CFG-*, EXP-01–05, -07, TEN-04, API-05/06`, but TESTING §9 step 2 also lists A11Y-02 (J1) | Next SPEC version (owner), or with the M2 exit check (S4-22) |
| P5 | Criterion splits vs one `SupersededByKey` (TD-31, #238): #243 names it for ARCHITECTURE; left out on purpose (owner decision before step 6, CAT-03/04) | Owner, before step 6 |
| P6 | TD-37 (#243) is closed by this PR; the register records it in its next version | Sprint 04 close-out (TECH_DEBT_v4) |

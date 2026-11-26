<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S7-05 — Assessments in the personal data export

- **Sprint:** 07 · **Task:** S7-05 · **Issues:** #414
- **Requirements / invariants:** R-AUTH-09, SPEC §8.4; **PRV-07** (stays `active`, workspace part complete); TEN-11 unchanged and green
- **Status:** done, in review (pull request open; E2E left to CI)
- **Scope:** exactly the row of S7-05 in `SPRINT_07.md`: a candidate of `createdByMe.projects` in `GET /api/v1/account/export` gains its stored assessments. Server only. No EF migration, no client, UX copy, Handoff or Figma change, no new dependency or project.
- **Owner decisions (2026-11-26, with the plan):**
  - **O-S7-1 — as proposed:** `assessments [ { number, title, value, note } ]` per candidate; stored rows only, in the order of the project's list; the criterion by number and title of the list's release; `value` in the spelling of the assessment API; no ids, no counts.
  - **Excluded criteria — export them:** the stored assessment of a criterion the list excludes by now (the row stays, WSP-03) is in the file, without a marker. Whether a criterion is included says the list in `createdByMe.criteriaLists` of the user who assigned it.

## Plan (given to the owner before the code)

Starting point, files, invariants, tests and documents as in §1 and §2. No invariant flips from `planned`; no migration; one session, no split. Two questions were asked and answered (above): the task prompt carried a placeholder for O-S7-1, and „stored rows only“ did not say what happens to the row of an excluded criterion.

## 1 Starting point

- `createdByMe.projects` holds projects, candidates, private sections and test evidences since S6-25 (`IProjectDataExport`, `Workspace/Accounts/ProjectDataExportService`, `AccountExportEndpoints`). The `CandidateAssessment` rows of S6-18 were missing (S6-25 open point 1, TESTING V350, #414); the deletion was done by the cascading foreign key.
- A row is stored with the first value or note and removed when it would say nothing, so „stored rows only“ needs no filter. A row is keyed by the criterion revision of the release the project's list is pinned to.
- Every stored row can be named through the list: a project's copy cannot change its selection (409 `project-copy`), is never the source of a carry-over to a newer release, and cannot be replaced once a value exists (WSP-02); replacing removes the rows without a value. An excluded criterion stays in the selection, only its decision changes.
- `AssessmentReader.ScopeOfAsync` returns only the included criteria, which is right for the tab and wrong for the export after the owner's second decision.

## 2 Changes

1. **Contract** (`IdentityAccess.Contracts/Accounts/IProjectDataExport.cs`): `ExportedCandidate` ends with `Assessments`; new record `ExportedAssessment(Number, Title, Value, Note)`.
2. **Reader** (`Workspace/Assessments/AssessmentReader.cs`): the reads of the list and of the catalog moved unchanged into the private `ResolveAsync`; `ScopeOfAsync` uses it as before; new `RequirementsOfSelectionAsync` returns every requirement criterion of the selection in list order, also an excluded one.
3. **Service** (`Workspace/Accounts/ProjectDataExportService.cs`): one query for the assessments of all exported candidates (through the organisation filter, TEN-01); the catalog is asked once per project that has a row, never for a project without one; `AssessmentsOf` walks the criteria and writes an entry where a row exists. Title as the exported lists name a criterion (`PlainLanguageTitle ?? Title`), value through `AssessmentProjection.NameOf`.
4. **Endpoint** (`IdentityAccess/Accounts/AccountExportEndpoints.cs`): `AccountExportCandidate` ends with `assessments` (`AccountExportAssessment`); `formatVersion` stays 1, as for the earlier additions.
5. **Test** (`AccountExportTests.Export_UserWhoCreatedProjects_…`, `INV:PRV-07`): see TESTING V419. The test now imports T.1 and uses a resolved list, because a project with an assessment resolves its list.
6. **Documents:** SPEC v2.70 (§8.4 text and example); TESTING v116 (V419, the PRV-07 row; `TESTING_v114.md` deleted); CODEBASE §2.120, §2.102 and the two layout entries.

Not changed: ARCHITECTURE (no module, dependency or flow changes — Workspace already reads the catalog and the Configurator through their contracts; C183 describes the export), TECHNICAL.md (nothing for an operator), CLAUDE.md (no new rule), TECH_DEBT (next version at the end of the sprint: the rows of #414 in §3.2, §5 and §6 are done then), ROADMAP, the sprint file.

## 3 Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` on the diff.

- **Security / privacy:** the rows are read through the organisation filter and only for candidates of projects the user created; the file gains no id (no revision id) and names no user. The assessments of colleagues in the user's own project are included, as everything else in such a project (S6-25 D1 (a)). No count, sum or percentage (SIG-01).
- **Correctness:** list order although stored in another order; a note without a value is `unknown`; a criterion without a row and a procedure criterion have no entry; no list → empty array.
- **Queries:** one more query for all candidates; one list read and one catalog resolve more per project with assessments (the list read per project existed before). No N+1 per candidate or row.
- **Findings:**
  - *fix* — the sentinel `ShouldNotContain("barrier")` would have matched a criterion title such as „Barrierefreiheit“ in a later test release; it checks the quoted value now.
  - *won't fix* — a stored list whose selection does not resolve under its release makes the export fail with 500 instead of leaving the assessments out. The assessment tab does the same; a stored list was checked when it was saved and its release never changes (ADR-002). Hiding it would export an incomplete file without a sign.
  - *won't fix* — a second read of the saved list per assessed project (`FindOfProjectAsync`, then `FindAsync` inside the reader). A user creates few projects; a combined read would change the Configurator contract.

## 4 Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format --verify-no-changes`: clean.
- `dotnet test --solution ProcureAccess.sln`: 2261 passed, 0 failed (unit, architecture, integration with Testcontainers); after the last change of the test the class `AccountExportTests` again: 8 passed.
- `node scripts/check-invariants.mjs` (reads `TESTING_v116.md`), `check-figma-links.mjs`, `check-i18n.mjs`: green.
- reuse: `lint-file` on the new protocol (the full lint reports false positives in a worktree; CI runs it).
- Client: not touched, not run. **E2E: not run locally** — another session used the E2E stack (owner's instruction); the change has no E2E case of its own, and CI runs the suites.

## 5 Open points

1. **TD-62 stays open** (legal review O-4): `ModifiedByUserId` in the export and on deletion, and the lists of organisations the user has left. Not touched.
2. **PRV-07** now only waits for step 6: `contributions` and purged content.
3. **TECH_DEBT** at the end of the sprint: #414 leaves §3.2, §5, §6 and §9.
4. **For the legal review, no action now:** the note of an assessment is free text and can name a person; it is exported like the other private notes of the user's projects (S6-25 D1 (a)).

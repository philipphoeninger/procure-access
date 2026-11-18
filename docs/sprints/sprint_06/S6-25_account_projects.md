<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-25 — Account deletion and data export: projects

> **Sprint:** 06, task S6-25. **Issues:** none.
> **Requirements / invariants:** R-AUTH-09, R-AUTH-10 (step-5 parts). **PRV-07** (workspace part) is `active`; TEN-11 stays `active` and gains its project cases.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **No EF migration.**
> **Scope:** exactly the row of S6-25 in `SPRINT_06.md`: the projects of an organisation whose last member deletes the account go with candidates, evidences and project copies; `createdByMe.projects` in `GET /api/v1/account/export`. No client change, no UX copy change. The assessments are not built yet (S6-18); the step-6 parts (contributions, profiles) stay deferred.
> **Owner decisions (2026-11-18, with the plan):** **D1 (a)** — a project entry of the export carries the full content of the project without ids, in the spelling of the workspace API: `name, organization, status, createdAt, criteriaList, candidates[]`; a candidate with its private section and `testEvidences[]`; an evidence with its private note; also what colleagues entered (candidates and evidences store no creator) · **D2 (a)** — the project names its criteria list as `{ name, release }` or `null`; the copy with its decisions stays in `createdByMe.criteriaLists` of the user who assigned it · **D3 (a)** — PRV-07 (workspace part) is flipped now; S6-18 adds the assessments to the deletion and the export.

## 0. Plan (written before the code)

- **Starting point:** `main` at e8fa463 (S6-12b merged). `DELETE /api/v1/account` calls every `IOrganizationDataDeletion`; only Configurator implements it. `createdByMe.projects` is the constant `[]`.
- **Code:** a Workspace implementation of `IOrganizationDataDeletion`; a new contract `IProjectDataExport` in `IdentityAccess.Contracts` with its Workspace implementation; the export endpoint fills the section.
- **Migration:** none — the cascading foreign keys (S6-15) and the index on `CreatedByUserId` (S6-11) exist.
- **Tests:** `AccountDeletionTests` (`INV:TEN-11`) and `AccountExportTests` (`INV:PRV-07`) extended.
- **Invariants:** PRV-07 (workspace part) `active` in TESTING v99.
- **Size:** one session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The projects of an organisation that ends stayed as rows no request sees (CODEBASE §2.93) | `ProjectDeletionService` |
| F2 | A project's copy of a list is a `CriteriaConfiguration` with the organisation's id (S6-12a), so `CriteriaListDeletionService` deletes it already | No code; the deletion test checks it |
| F3 | Candidates, private sections, evidences and private notes hang on the project by cascading foreign keys (S6-15) | One `ExecuteDeleteAsync` on the projects is enough |
| F4 | SPEC §8.4 gives the shape of a project entry only as `{ name, candidates: [ … ] }` | Decision D1 (a); SPEC v2.56-1 |
| F5 | `CriteriaListDataExportService` exports every list the user created, a project copy among them (the user who assigned it), looking like an ordinary list | Decision D2 (a): unchanged; the project entry names its list |
| F6 | The assessments of the row do not exist: S6-18 is not merged and has no open pull request | Decision D3 (a); open point 1 |
| F7 | `ICatalogTestTypes` serves only the keys of the test procedures, no names | `testType` of an evidence is the catalog key (e.g. `softwareTest`), as the API's `testTypeId` |
| F8 | `account.export.text` already names „Beschaffungsprojekte“ | No UX copy or client change |

## 2. Changes

1. **Contract** `IdentityAccess.Contracts/Accounts/IProjectDataExport.cs`: `GetCreatedByAsync(userId)` → `ExportedProject` (`ExportedProjectCriteriaList`, `ExportedCandidate`, `ExportedTestEvidence`). Declared by IdentityAccess and implemented by Workspace, as `ICriteriaListDataExport` is by Configurator (IdentityAccess depends on no module).
2. **Workspace** `Accounts/` (new folder):
   - `ProjectDeletionService : IOrganizationDataDeletion` — deletes the projects of the given organisations through the organisation filter of the request; the database cascades to candidates, private sections, evidences and private notes. Repeatable.
   - `ProjectDataExportService : IProjectDataExport` — the projects with `CreatedByUserId` = the user through the filter (the organisations the user still belongs to), oldest first; candidates with evidences in one query, the private rows in one query each; organisation names from `IOrganizationMembers`; the list's name and release from `IProjectCriteriaLists.FindOfProjectAsync`. Status, test status and kind are spelled by `ProjectService.NameOf` and `CandidateNames.Of`.
   - Both registered in `WorkspaceModule`.
3. **IdentityAccess** `AccountExportEndpoints`: takes `IProjectDataExport`, maps to the new records `AccountExportProject`, `AccountExportProjectCriteriaList`, `AccountExportCandidate`, `AccountExportTestEvidence`; `AccountExportCreatedByMe.Projects` is typed. `AccountDeletionEndpoints`: comment only — the loop over the registered implementations did not change.
4. **Tests** (integration, PostgreSQL through Testcontainers):
   - `AccountExportTests.Export_UserWhoCreatedProjects_ContainsTheirContentAndNoProjectOfOthersOrOfALeftOrganization` (`INV:PRV-07`, new): two own projects (A; B archived), the property order of project, list, candidate and evidence, every value, a candidate with only a name; not exported: a colleague's project with its candidate and contact note, a project in an organisation the user left; no id and no other user's address in the raw body.
   - `AccountDeletionTests` (`INV:TEN-11`): the fixture stores a project with candidate, private section, evidence, private note and copy in A and in „Einzelbüro“; `Rows` counts the five Workspace tables; the last-member case expects only rows of A in every Workspace table and the list of A with its copy in the Configurator; the 409 case and both 204 cases of an organisation that lives on expect every row unchanged.
   - Checked once that the last-member case fails without the registration of `ProjectDeletionService`.
5. **Documents:** TESTING v99 (V342 – V344; PRV-07 and TEN-11 rows, §9 step 5), ARCHITECTURE v94 (C182; §7.2 listing, §12 step 5), SPEC v2.56 (four rows; §8.4, §8.13, R-AUTH-10), CODEBASE §2.101, the notes of §2.93 and §2.98, §3; CLAUDE.md (the state line only; 39 921 characters). `TESTING_v97.md` and `ARCHITECTURE_v92.md` deleted (the two newest stay).

Not changed: client, e2e, UX copy, Handoff, Figma mapping, `Dockerfile` (no new project), `ModuleMap` (no new dependency), compose files, TECH_DEBT (next version at the end of the sprint; its §5 row „Account deletion parts of later steps“ and §9 row are done for step 5 except the assessments), the sprint file.

## 3. Self-review

Checklist of CLAUDE §6.7, with `/engineering:code-review` on the code diff; findings sorted per §6.8.

- **fix:** none found.
- **follow-up:** R1 — the assessments (open point 1).
- **won't fix:** R2 — `FindOfProjectAsync` is called once per exported project (N reads for N projects). The contract answers one project, the export is a rare request of one user, and a user creates few projects; a batch method would be a mechanism this task does not need. R3 — the test method `Delete_LastMember_DeletesTheOrganizationWithItsListsLinksAndInvitationsOnly` keeps its name although it checks the projects too; TESTING V286 names it.
- **Security and tenancy:** both services read and delete through the organisation filter of the request; no `IgnoreQueryFilters()`, no raw SQL, so no allow-list entry. An id passed for an organisation the user is no member of deletes nothing. The export leaves the module without ids and without a user; the test searches the raw body for every id and address.
- **Privacy (PRV-07):** only projects with the user's `CreatedByUserId`. Their content includes what colleagues typed (candidate facts, `testedBy`, notes) — decision D1 (a), SPEC §8.4 „current content“; `testedBy` is free text of the project, not an account.
- **Correctness:** a project of an organisation the user has left is not exported (the filter, and the name lookup for a membership removed during the request). A candidate without a private section and an evidence without a note export `null`. The deletion is one statement per module and can run again after a failure in a later step (ADR-014).
- **Queries:** the export reads projects, candidates with evidences, private sections, private notes and memberships in one query each, plus R2. The deletion is one `DELETE`; the foreign keys' indexes exist.
- **Product rules:** no count, score or verdict; private fields leave the workspace only towards the user who can read them anyway (SPEC §8.4), never towards the directory.

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format ProcureAccess.sln --verify-no-changes`: green.
- `dotnet test --solution ProcureAccess.sln` (unit, architecture, integration with Testcontainers / PostgreSQL 17): 2 012 passed, 0 failed.
- `dotnet ef migrations has-pending-model-changes` for `WorkspaceDbContext`: no changes since the last migration.
- `node scripts/check-invariants.mjs`: gate passed (TESTING v99). `check-figma-links.mjs`, `check-i18n.mjs`: green, unchanged.
- `reuse lint-file` on the new files: compliant. The full `reuse lint` runs in CI (in a worktree the container cannot read `.gitignore`).
- **E2E: not run locally** — another session used the local E2E stack at the same time (owner instruction). No client or e2e file changed; `e2e/identity/account.spec.ts` (export download and account deletion with the real cookie session, `INV:TEN-11`) runs in CI.
- Client lint and tests: not run — no client file changed.

## 5. Open points

1. **Assessments (S6-18, decision D3 (a); TESTING V344).** When S6-18 adds its assessment rows: a cascading foreign key onto the candidate (or a delete of its own in `ProjectDeletionService`), the assessments in the candidate entry of the export (SPEC §8.4), and the cases of `AccountDeletionTests` and `AccountExportTests` extended. The task prompt of S6-18 should name it.
2. **Copy in the export of the user who assigned it (D2 (a)).** In `createdByMe.criteriaLists` a project copy cannot be told from the list it was copied from (same name). If that confuses users, an entry could name its project; not needed for PRV-07.
3. **Step 6 – 7** (unchanged, S5-28a decision 3, S5-27 1a): contributor profiles detached, entries withdrawn, experience reports purged; `contributions` in the export.
4. **TECH_DEBT** (end of the sprint): §5 „Account deletion parts of later steps“ and the §9 row lose their step-5 part, except open point 1.

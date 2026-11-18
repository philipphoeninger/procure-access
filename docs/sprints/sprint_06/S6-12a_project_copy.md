<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-12 — Project copy of a criteria list (part a: server)

> **Sprint:** 06, task S6-12, **split** into S6-12a (server, this pull request) and S6-12b (client, after S6-13 is merged). **Issues:** none.
> **Requirements / invariants:** R-PRJ-02 (copy), R-PRJ-04 (assign/replace without assessments), R-PRJ-06 (server part). **WSP-03** `active` (without its two candidate clauses, deferred to S6-18); **TEN-08** „project copy → 409“ covered; **WSP-02** copy part tested, status `planned` until S6-18 adds the lock on real assessments; TEN-05, TEN-04, SEC-10 and OPS-03 gain cases.
> **Status:** S6-12a done by Claude Code (2026-11-18) — pull request open. **One EF migration:** Configurator `ProjectCopies`. **No Workspace migration.**
> **Scope (a):** the server part of the row of S6-12 in `SPRINT_06.md`: `PUT /api/v1/projects/{id}/criteria-configuration`, the copy with `ProcurementProjectId`, the source unchanged, a copy never offered as a source (saved-list list, the PUT) and never shared (409), replacing while no assessment exists with a hook for the lock of S6-18. **Scope (b):** S11 in project-copy mode (`criteria.list.projectCopy`, the four selection layers fixed, „Auswahl ändern“ hidden), `projects.detail.criteriaAdded` on return to S32, the list choice of S31 (S6-13 owner decision D1 (a)).
> **Owner decisions (2026-11-18, with the plan):** **split** a/b as proposed · **D1 (a)** — the link between project and copy is stored only on the copy (`CriteriaConfiguration.ProcurementProjectId`), not as `ProcurementProject.CriteriaConfigurationId` · **D2 (a)** — a new problem type `project-copy` (409) for the actions a copy refuses, without a client text · **D3 (a)** — the copy of an archived project is read-only in the client (S6-12b); the server's refusal is an open point · **D4 (a)** — S11 of a project copy at `/projekte/:id/kriterienliste` (S6-12b) · **D5** open — the text for „project created, list not copied“ (S31, S6-12b).

## 0. Plan (written before the code)

- **Starting point:** `main` at d377018 (S6-26 merged). S6-13 (#407, client of the projects) and S6-15 (#409, candidates, with a Workspace migration) open.
- **Split:** the client needs S6-13's routes, store and S32 placeholder; the server part can start now. One session each.
- **Migration:** Configurator only (`ProcurementProjectId` with a unique index); with D1 (a) none in Workspace, so no conflict with S6-15's open migration.
- **Code:** a contract `IProjectCriteriaLists` in `Configurator.Contracts`, the PUT in Workspace, the lock hook `ICriteriaListLock`, the guards of the saved-list and share-link endpoints for a copy.
- **Tests:** unit tests of the copy; integration tests of the PUT, the guards, the share link; the matrix row.
- **Invariants:** WSP-03 `active` (candidate clauses deferred); TEN-08's deferred part; WSP-02 stays `planned`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | ARCHITECTURE §5.4 lists `ProcurementProject.CriteriaConfigurationId` and §5.3 `CriteriaConfiguration.ProcurementProjectId` — a pointer on both sides, in two contexts that cannot share a transaction | D1 (a): only the copy carries the link; replacing is one save of `ConfiguratorDbContext`; §5.4 records that `CriteriaConfigurationId` is not built (C180) |
| F2 | §5.3 says „the column **and its foreign key** are added in step 5“, CLAUDE §7 / ADR-014: no foreign key crosses two contexts | No foreign key; a unique index where set; §5.3 corrected |
| F3 | The module map allows Workspace → Configurator.Contracts, not the other way | Workspace calls `IProjectCriteriaLists`; Configurator cannot see a project's status (→ D3, open point 2) |
| F4 | SPEC R-SAV-04 and TEN-08 ask for a 409 on a share link of a copy; no problem type fits (`conflict` is S37's with `editorDisplayName`) | D2 (a): `project-copy`, also for the selection change, the deletion and the copy of a copy |
| F5 | TESTING WSP-03 speaks of „after assessments exist“ and of candidates; assessments come with S6-18 | WSP-03 `active` with the two candidate clauses deferred (V335) |
| F6 | S6-15 (#409) changes `ProjectService`, `ProjectContracts`, `ProjectEndpoints` and `ProblemTypes` | The PUT lives in files of its own; the remaining overlaps are small (the answer record, four helpers made `internal`, one problem-type line) |
| F7 | S6-13 (#407) says S6-12 adds the list choice to S31 and needs a text for „project created, list not copied“; UX copy v40 (and S6-13's v41) has none | D5 open; blocks only S6-12b |
| F8 | The gate warns on tags of a `planned` invariant; earlier tasks (V191, V277) left such tests untagged until the flip | WSP-02's tests are named in TESTING V336 and tagged by S6-18 |

## 2. Changes

1. **Domain (Configurator):** `CriteriaConfiguration.ProcurementProjectId`, `IsProjectCopy`, `CopyForProject(project, user, now)` (same organisation, name, release, selection keys and rows with their stored recommendation and decision; new id, the assigning user, the assignment time; throws for a copy as source); `ChangeSelection` throws on a copy.
2. **Persistence and migration `20261118150252_ProjectCopies`** (`internal`): the nullable column and the unique index `IX_CriteriaConfigurations_ProcurementProjectId` where not null; no foreign key.
3. **Contract** `IProjectCriteriaLists` (`Configurator.Contracts/Configurations/`) with `ProjectCopyResult`, `ProjectCopyOutcome`, `ProjectCriteriaList`; implemented by `Configurations/ProjectCriteriaLists`: the source through the filter and compared with the project's organisation, the old copy removed and the new one added in one save; a unique violation (two first assignments at once) retried once, as `UserPreferencesService` does.
4. **Saved-list endpoints and a copy:** `GET /configurations/{id}` answers `procurementProjectId`; the S14 list leaves copies out; `PUT …/selection`, `DELETE`, `POST …/copies` → 409 `project-copy` (`EditOutcome.ProjectCopy`; `DeleteAsync` returns an `EditResult<bool>`); `POST …/share-links` → 409 `project-copy` after the role check (`OwnerOfAsync` → `ListOwner`). `PATCH …/criteria/{rev}`, the read and the saved export work as for any list.
5. **Platform contracts:** `ProblemTypes.ProjectCopy` (`project-copy`), in `All`; no client key (D2 (a)).
6. **Workspace:** `Projects/ProjectCriteriaListEndpoint` (`PUT /projects/{id}/criteria-configuration`) and `ProjectCriteriaListService`; role (Editor, from the stored project), confirmed address, antiforgery; 400 at `sourceConfigurationId` (missing, unknown, foreign, copy — one answer); 409 `project-archived`; 409 `locked-criteria-list` with an `errors` extension at `sourceConfigurationId`. `ICriteriaListLock` with `NoAssessmentsYet` (the hook for S6-18). `ProjectResponse.CriteriaList` on read and PATCH; `ProjectService` takes `IProjectCriteriaLists`. The helpers of `ProjectEndpoints` are `internal` for the new endpoint.
7. **Tests.** New: `Integration/Workspace/ProjectCriteriaListTests.cs` (13). Changed: `Unit/Configurator/CriteriaConfigurationTests.cs` (+3), `Integration/Tenancy/AuthorizationMatrixTests.cs` (row `assign-project-list`, six users).
8. **Documents.** TESTING v97 (V335 – V338; WSP-03 `active`, rows WSP-02, TEN-08), ARCHITECTURE v92 (C180; §5.3, §5.4, §7.2, §12 step 5), SPEC v2.54 (§8.6, §8.10, five rows), CODEBASE §2.99 and §3, CLAUDE.md §3 (state line; 39 589 characters). `TESTING_v93.md` and `ARCHITECTURE_v88.md` deleted (two newest stay).

Not changed: client, e2e, UX copy, Handoff, Figma mapping, `Dockerfile` (no new project), compose files, `appsettings.json`, TECH_DEBT (next version at the end of the sprint), the sprint file.

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` on the whole diff (security, performance, correctness, maintainability); findings sorted per §6.8.

- **fix:** none — no correctness or security bug that affects current behaviour.
- **won't fix:** R1 — assigning the same list again replaces the copy by a fresh one and drops the copy's adjustments: that is „replace“ in R-PRJ-04, and S6-12b asks before it replaces. R2 — a missing `sourceConfigurationId` answers 400 before an archived project's 409: both refuse, the order changes nothing for a client.
- **follow-up:** open points 1 – 4.
- **Security and tenancy:** the project's organisation comes from the stored project through the filter; the source is read through the filter **and** compared with the project's organisation, so a member of two organisations cannot copy across them (tested). An unknown, foreign or copy source gets one identical 400, so the request learns nothing about lists it cannot use; the share-link 409 comes after the role check. No `IgnoreQueryFilters()`, no raw SQL: the TEN-02 allow-list is unchanged. No log line names a user, a list or a project.
- **Correctness:** replacing is one save in one context (atomic); the unique index holds one copy per project, and the race of two first assignments ends with one copy (tested). The source is never tracked (`AsNoTracking`), so it cannot change.
- **Queries:** reading a project adds one indexed query for its copy; the PUT reads the project, the source with its rows (split query) and the old copy. No N+1.
- **Product rules:** no score or aggregate; the copy keeps the stored recommendations (CAT-08).

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format ProcureAccess.sln --verify-no-changes`: green.
- `dotnet test --solution ProcureAccess.sln` (unit, architecture, integration with Testcontainers, PostgreSQL 17): **1 842 passed, 0 failed** (unit 819). `ProjectCriteriaListTests` passed three runs in a row on its own (13 of 13).
- OPS-03: `ConfiguratorMigrationTests` (all migrations applied with the own history table; no pending model changes).
- `check-invariants` (73 active, WSP-03 with 3 tests; no warning), `check-figma-links`, `check-i18n`, `check-icons`: green. REUSE: `reuse lint-file` on the new files (worktree; CI runs the full `reuse lint`).
- Client tests and lint: not run, the client is unchanged (the new answer members are additive). E2E: left to CI; no screen and no seeded data change.
- Figma: no screen in part a.

## 5. Open points

1. **Lock and copy are two steps in two contexts (S6-18).** `ICriteriaListLock` is asked in Workspace, the copy is made in Configurator: an assessment saved between the two is not caught. S6-18 decides whether the window matters (a re-check after the copy, or accepted with API-03 in S6-20).
2. **Archived project's copy (D3 (a)).** `PATCH /configurations/{copyId}/criteria/{rev}` still changes the copy of an archived project; Configurator cannot see the project's status. S6-12b shows the copy read-only. A server refusal needs Workspace to tell Configurator (e.g. through the contract on a status change) — proposed for S6-20 or a follow-up issue.
3. **Personal data export.** A copy is a list the assigning user „created“, so `ICriteriaListDataExport` lists it like any list, without its project. Whether the export should name the project or leave copies out is for S6-25 (the workspace part of the export).
4. **„Last change“ of a project.** An assignment does not move `ProcurementProject.ModifiedAt`. S6-15 decided that candidates and evidences move it (owner decision S6-15 5 (a)); whether an assignment should as well stays open (S6-11 open point 3).
5. **S6-12b (client):** S11 at `/projekte/:id/kriterienliste` (D4 (a)) with `criteria.list.projectCopy`, „Auswahl ändern“, share, delete and copy hidden, read-only for a Reader and an archived project; `projects.detail.criteriaAdded` on return to S32 (the placeholder of S6-13); the list choice of S31 (`projects.create.criteria`, `.hint`, only organisation lists). Needs S6-13 merged and **D5**: the wording for „project created, list not copied“, or the decision that this case shows the general error text.
6. **Merge order with S6-13 and S6-15:** both merged first, so main was merged into this branch (2026-11-18) and S6-12a renumbered to TESTING v97 (V335 – V338), ARCHITECTURE v92 (C180), SPEC v2.54 and CODEBASE §2.99; this protocol was renamed from `S6-12_project_copy.md`. S6-15's `WorkspaceAccess` replaced the role-check helpers this task had opened up in `ProjectEndpoints`; `ProjectCriteriaListEndpoint` uses it.
7. **CLAUDE §3 rule „a new problem type gets its UX key“** has an owner-decided exception (`project-copy`, D2 (a)); SPEC §8.6 records it.

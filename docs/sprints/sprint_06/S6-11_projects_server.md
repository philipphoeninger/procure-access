<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-11 — Workspace server: projects

> **Sprint:** 06, task S6-11. **Issues:** none.
> **Requirements / invariants:** R-PRJ-01, R-PRJ-05 (server part). TEN-01 (new entity), TEN-04 (workspace part), TEN-05 (project rows), SEC-10, API-01 and OPS-03 gain cases; no invariant changes its status.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **One EF migration:** Workspace `InitialWorkspaceSchema` (the module's first, own history table).
> **Scope:** exactly the row of S6-11 in `SPRINT_06.md`: `ProcurementProject` in the Workspace module with its own context, the endpoints list, create, read, rename and status, the rule of an archived project, no deletion. No client change, no project copy (S6-12), no candidates (S6-15).
> **Owner decisions (2026-11-18, with the plan):** **1 (a)** — a change of an archived project answers 409 with the new problem type `project-archived`, mapped to the existing text `projects.archived.note` · **2 (a)** — on an archived project only the status changes, to either other value; a change that names a name is refused, also together with a status · **3 (a)** — the list answers every project of the organisation with its status, the most recently changed first; S30 sorts and filters, as S14 does · **4 (a)** — the creator is stored but not served.

## 0. Plan (written before the code)

- **Starting point:** `main` at c66f601 (S6-10b and S6-03 merged). The Workspace module is the empty skeleton of S2-06.
- **Migration:** one, in the Workspace module: schema `workspace`, table `ProcurementProjects`, own history table (ADR-014).
- **Code:** entity, context with the organisation filter, service, four minimal-API endpoints on the routes of ARCHITECTURE §7.2, host wiring.
- **Tests:** unit tests of the entity; endpoint tests; the context and a sample in the isolation test; the matrix rows; OPS-03 for the new context.
- **Invariants:** none activated; WSP-01 – WSP-04, SIG-13, API-03 and API-04 stay `planned` for their tasks.
- **Size:** one session, no split.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `WorkspaceModule.AddWorkspaceModule()` registered nothing; the project had no EF reference. `Dockerfile`, `ModuleMap` and ARCHITECTURE §4.3 already know the module | The project references EF Core 10.0.15 and Npgsql 10.0.3 like the other modules (no new dependency, `THIRD-PARTY-NOTICES.txt` unchanged); no `Dockerfile` or `ModuleMap` change |
| F2 | SPEC §8.6 had no problem type for a change of an archived project, and `conflict` belongs to the S37 dialog | Decision 1 (a): `project-archived` |
| F3 | `projects.archived.note` tells the user to set the status back to „Laufend“; SPEC says only „read-only for everyone“ | Decision 2 (a): the status is the one thing that changes |
| F4 | The row reads „sort by name/date“ as part of the list endpoint; S14 as built gets every list and sorts in the browser | Decision 3 (a): no `sort` parameter, no paging |
| F5 | ARCHITECTURE §7.2 names the organisation of list and create in the route (`/organizations/{orgId}/projects`), while the saved lists name it in the query or body | The routes of §7.2 are built |
| F6 | The UX copy area `projects` is not in `AREAS` of `check-i18n.mjs` yet, so `projects.archived.note` is not in the client's `de.json` | The client mapping of `project-archived` waits for the task that adds the area (open point 1); the client shows the general error text until then |
| F7 | TEN-04's workspace part is the Platform default `no-store` (S5-22), tested on every row of the matrix | No header code in the module; the new matrix rows carry `INV:TEN-04` |

## 2. Changes

1. **Entity** (`Domain/`): `ProcurementProject` (`Create`, `Rename`, `ChangeStatus`, `IsArchived`; name ≤ 120; `CreatedByUserId`, `CreatedAt`, `ModifiedAt`) and `ProjectStatus` (Open, Done, Archived). `ModifiedAt` moves only when the name or the status really changes. `Rename` throws on an archived project.
2. **Persistence** (`Persistence/`): `WorkspaceDbContext` (`internal`, schema `workspace`, history `workspace."__EFMigrationsHistory"`, the organisation filter bound to `IOrganizationContext`), `ProcurementProjectConfiguration` (status by name, indexes on organisation and creator, no foreign key), the design-time factory and `WorkspaceDatabaseMigrator`.
3. **Migration `20261118123127_InitialWorkspaceSchema`** (`internal`): the schema, the table and its two indexes; `Down` drops the table.
4. **Service and endpoints** (`Projects/`):
   - `GET /api/v1/organizations/{organizationId}/projects` — Reader; every project of that organisation, also archived, the most recently changed first: `id`, `name`, `status`, `productCount` (0 until S6-15), `modifiedAt`.
   - `POST /api/v1/organizations/{organizationId}/projects` `{ name }` — Editor; 201 with the project and `Location`.
   - `GET /api/v1/projects/{id}` — every member; the organisation filter is the check.
   - `PATCH /api/v1/projects/{id}` `{ name?, status? }` — Editor on the stored project's organisation; 200 with the project; 409 `project-archived` when the body names a name and the project is archived.
   - `status` travels as `open`, `done`, `archived`. The role check comes before validation; create and change need a confirmed address and antiforgery. No DELETE route.
   - **Seam for S6-20:** every change of a stored project runs through `ProjectService.ChangeAsync` (load, change, one save). Nothing is mapped for the concurrency token yet.
5. **Platform contracts:** `ProblemTypes.ProjectArchived` (`project-archived`), also in `ProblemTypes.All`.
6. **Host:** `AddWorkspaceModule(builder.Configuration)` and `MapWorkspaceEndpoints()` in `Program.cs`, before the Swagger set-up.
7. **Tests.** New: `Unit/Workspace/ProcurementProjectTests.cs`, `Integration/Workspace/ProjectEndpointsTests.cs` (3 × `INV:TEN-01`, 2 × `INV:SEC-10`, 2 × `INV:API-01`, the archived rule and „no DELETE“ without a tag), `Integration/Persistence/WorkspaceMigrationTests.cs` (2 × `INV:OPS-03`). Changed: `OrganizationIsolationTests` (context and sample), `AuthorizationMatrixTests` (five actions × six users, each as `INV:TEN-05` and `INV:TEN-04`), `ApiFixture` (migrates the context, keeps its history out of Respawn, stores its entities).
8. **Documents.** TESTING v92 (V313 – V316; TEN-05 row), ARCHITECTURE v86 (C172; §4.3, §5.4, §7.2, §12 step 5), SPEC v2.49 (§8.6 and three rows on R-PRJ-01, R-PRJ-05), CODEBASE §2.93, §3, §4, CLAUDE.md (§3 state, §5 context list; 39 724 characters). `TESTING_v90.md` and `ARCHITECTURE_v84.md` deleted (two newest stay).

Not changed: client, e2e, UX copy, Figma mapping, `Dockerfile`, compose files, `appsettings.json`, TECH_DEBT (next version at the end of the sprint), the sprint file.

## 3. Self-review

Checklist of CLAUDE §6.7; findings sorted per §6.8.

- Reviewed with `/engineering:code-review` (security, performance, correctness, maintainability) on the whole diff; no critical issue.
- **fix (done):** R1 — the read endpoint first asked for the organisation and then for the project (two queries) and checked the Reader role, which every member has; it now reads once through the filter, as the saved lists do. R2 — two validation tests carried `INV:API-02`, which is the client's invariant; they carry `INV:API-01`.
- **won't fix:** R3 — a PATCH that names the current name of an archived project answers 409 although nothing would change: the rule of decision 2 (a) is „names a name“, and a second rule for equal values would only help a client that sends unchanged fields. R4 — `"name": null` in a PATCH counts as „not named“; JSON merge semantics would need a way to clear the name, which a project cannot do.
- **follow-up:** open points 1 – 3.
- **Correctness:** the name is trimmed and checked in the endpoint before the entity sees it, so the entity's exceptions are guards, not answers. A change is one load and one save; two racing changes end with the later one (API-03 with S6-20). An empty PATCH and a PATCH with the stored values answer 200 and keep `ModifiedAt`.
- **Security and tenancy:** the organisation of every request comes from the route or from the stored project through the filter; no header is read. A non-member cannot tell an unknown project from a foreign one (404 both), and the role check runs before validation, so a non-member learns nothing from field errors. No `IgnoreQueryFilters()` and no raw SQL: the TEN-02 allow-list is unchanged. No log line names a user or a project.
- **Queries:** list and read are one query each, a change two (organisation, then the tracked row); the list uses the index on `OrganizationId`. No N+1.
- **Product rules:** `productCount` is a count of rows, not a statement about a product (SIG-01 untouched); the seven counts of SIG-13 come with S6-18.

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format ProcureAccess.sln --verify-no-changes`: green.
- Unit tests: 814 passed. Architecture tests: 65 passed. Integration tests (Testcontainers, PostgreSQL 17): 918 passed; after the last change (R1) the five affected classes again, 345 passed.
- OPS-03 for `WorkspaceDbContext`: `WorkspaceMigrationTests` (empty database, no pending model changes, applied at startup).
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` (38 packages, unchanged): green. `reuse lint`: compliant.
- Client tests and lint: not run, the client is unchanged. E2E: left to CI; no screen or seeded data changes.
- Figma: no screen in this task.

## 5. Open points

1. **Client mapping of `project-archived`.** `PROBLEM_TYPE_KEYS` gets `'project-archived': 'projects.archived.note'` with the task that adds the area `projects` to `AREAS` (S6-13 or S6-14); the UX copy §6.3 table gets its row with the next UX copy version. Until then the client would show the general error text; no screen sends the request yet.
2. **Projects of an organisation that ends** (account deletion of its only member, the Development rebuild) stay as rows no request can see until S6-25 implements `IOrganizationDataDeletion` for the Workspace. The same holds for `createdByMe.projects` of the personal data export.
3. **„Last change“** is the project row's `ModifiedAt`. Whether a change to a candidate, an evidence or an assessment moves it is to be decided with S6-15 and S6-18 (S30 shows it as „last change“ of the project).
4. **`productCount`** is the constant 0 until S6-15 stores candidates; the field is in both answers so the client of S6-13 can be built against the final shape.
5. **Paging:** ARCHITECTURE §7.1 says „offset-based everywhere“; the project list, like the list of saved lists, has none (decision 3 (a)). If an organisation ever holds hundreds of projects, both lists need it together.
6. Unchanged from S6-10b: the header of `SPEC_v2.md` named older ARCHITECTURE and TESTING versions; v2.49 sets it to the current ones.

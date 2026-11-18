<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-15 — Candidates server: candidate, test status and evidence

> **Sprint:** 06, task S6-15. **Issues:** none.
> **Requirements / invariants:** R-CAN-02, R-CAN-05 (server part). **WSP-01** and **SIG-03** (workspace part) become `active`; TEN-01, TEN-04, TEN-05 (candidate rows), SEC-10, API-01 and OPS-03 gain cases.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **One EF migration:** Workspace `CandidateProducts` (the second migration of the module).
> **Scope:** exactly the row of S6-15 in `SPRINT_06.md`: `CandidateProduct` with its private section stored apart, the data for the name-match hint, `TestStatus` with default `Unknown`, `TestEvidence` 0..n with at most one per test type, „Andere Prüfung“ with a name, a new product version updating the row, the endpoints and the TEN-05 rows. No client change, no candidate removal (S6-22), no ETags (S6-20), no assessments (S6-18), no „Merken“ (M6).
> **Owner decisions (2026-11-18, with the plan):** **1 (a)** — no name-match data yet: `SoftwareProductId` is stored and stays `null`; the match and the link come with „Merken“ (R-CAN-01, M6) · **2 (a)** — a candidate needs only its name; an evidence needs kind, procedure (catalog) or name („Andere Prüfung“), tester and date, the date not after today in Europe/Berlin; version, report link and qualification note optional · **3 (a)** — new problem type `test-status-conflict` (409, `errors.testStatus`, `evidenceCount`); `PUT …/test-status` takes `removeEvidences: true` to remove the evidences in the same save · **4 (a)** — the qualification note of an evidence is a row of its own too, ≤ 2 000 · **5 (a)** — a change of a candidate, its test status or an evidence moves the project's `ModifiedAt`. Plan defaults the owner approved with them: only catalog procedures are unique per candidate (several „Andere Prüfung“ / „Eigene Prüfung“ allowed); the candidate's `ModifiedAt` moves with its evidences.

## 0. Plan (written before the code)

- **Starting point:** `main` at b16d6e5 (S6-11 and S6-23 merged). The Workspace module holds `ProcurementProject`; `productCount` was the constant 0.
- **Migration:** one, in the Workspace module: four tables in schema `workspace`, foreign keys inside the schema only.
- **Code:** entities with the WSP-01 rules in the candidate aggregate, private rows apart, a Catalog.Contracts lookup of the test-type keys, seven endpoints on the routes of ARCHITECTURE §7.2, the project read with its candidates.
- **Tests:** unit tests of the aggregate; endpoint tests; samples in the isolation test; the matrix rows; OPS-03 and the SIG-03 schema check.
- **Invariants:** WSP-01 and SIG-03 (workspace part) `active` in TESTING v94.
- **Size:** one session; fallback split a (candidate, private section, status) / b (evidences) not needed.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No module holds the directory's product identities yet (Directory is an empty skeleton; the legacy `Products` table is out of reach of a module, ARC-01), so the name-match hint has nothing to match against | Decision 1 (a): `SoftwareProductId` stored, `null`; the hint comes with M6 (open point 3) |
| F2 | The UX copy marks none of the S33 fields and not `candidate.evidence.qualification` with „(optional)“; by `common.requiredNote` every field, the private notes included, would be required | Decision 2 (a); the UX copy needs the markers (open point 2) |
| F3 | WSP-01 asks for „409 with field error“; SPEC §8.6 had no type for it, and `conflict` belongs to the S37 dialog | Decision 3 (a): `test-status-conflict` |
| F4 | ARCHITECTURE §5.4 lists `ContactNotes`, `InternalNotes` and `TesterQualificationNote` as columns of `CandidateProduct` and `TestEvidence`; the sprint row asks for the private section to be stored apart | Rows of their own (decision 4 (a) for the evidence note); ARCHITECTURE C177 records the deviation |
| F5 | SPEC §7.2 has no limit for the qualification note | 2 000, like the other private notes (decision 4 (a), SPEC v2.51-2) |
| F6 | The catalog's test procedures are filter values that belong to no release (C104); the Workspace module had no way to check a key | `ICatalogTestTypes` in Catalog.Contracts (Workspace may reference it, §4.3; no `ModuleMap` change) |
| F7 | „Not after today“ needs a calendar day; Platform's `ShareLinks:TimeZone` belongs to another module | 🔧 `Workspace:TimeZone` (`Europe/Berlin`) in `appsettings.json`, checked at the start |
| F8 | The UX copy area `candidate` is not in `AREAS` of `check-i18n.mjs`, so the client has no key to map `test-status-conflict` to | The client mapping waits for S6-16/S6-17 (open point 1), as `project-archived` waits for S6-13/S6-14 |

## 2. Changes

1. **Entities** (`Workspace/Domain/`):
   - `CandidateProduct` — the aggregate: `Create(project, facts, now)` (refuses an archived project), `Facts`/`ChangeFacts`, `ChangeTestStatus(status, removeEvidences, now)`, `AddEvidence`, `ChangeEvidence`, `RemoveEvidence`, `MarkChanged`. The rule cases answer outcomes (`EvidenceChangeOutcome` NotTested / DuplicateTestType, `TestStatusChangeOutcome` EvidencesExist); invalid input throws (the endpoint checks it first). `CandidateFacts` (name ≤ 200, vendor ≤ 200, website ≤ 2 048, summary ≤ 2 000).
   - `TestEvidence` with `TestEvidenceFacts` (kind, `TestTypeKey` only with a procedure, `OtherTestTypeName` ≤ 120 only with „Andere Prüfung“, tester ≤ 200, date, version ≤ 200, report link ≤ 2 048); a change replaces the fields of the same row (C65).
   - `CandidatePrivateSection` (contacts, internal notes ≤ 2 000; key = candidate) and `TestEvidencePrivateNote` (qualification note ≤ 2 000; key = evidence). Neither copied type leads to them.
   - `TestStatus` (Unknown, Tested, NotTested) and `TestKind` (Procedure, Other, Own).
   - `ProcurementProject.MarkChanged(now)` — a change below the project (throws on an archived one).
2. **Persistence:** `CandidateConfigurations.cs` (four configurations; `TestStatus` by name with the default `Unknown` and a sentinel that makes EF always send it; foreign keys with cascade; partial unique index on candidate and `TestTypeKey`); every entity with the organisation filter in `WorkspaceDbContext`.
3. **Migration `20261118142038_CandidateProducts`** (`internal`): the four tables, their foreign keys and indexes; `Down` drops them.
4. **Endpoints** (`Candidates/`):
   - `POST /api/v1/projects/{id}/candidates` `{ name, vendor?, website?, functionalSummary?, contactNotes?, internalNotes? }` — Editor; 201 with the candidate and `Location`; `softwareProductId` refused until M6.
   - `GET /api/v1/candidates/{id}` — every member; `privateSection` and `testEvidences[]` (with `qualificationNote`) in the answer.
   - `PATCH /api/v1/candidates/{id}` — Editor; what is named changes, `""` clears an optional field, the name cannot be cleared.
   - `PUT /api/v1/candidates/{id}/test-status` `{ testStatus: tested | notTested | unknown, removeEvidences? }` — Editor; 409 `test-status-conflict` with `evidenceCount` when it leaves `tested` with evidences and without `removeEvidences`.
   - `POST /api/v1/candidates/{id}/test-evidences`, `PUT` and `DELETE /api/v1/test-evidences/{id}` — Editor; 409 `test-status-conflict` for an evidence while the status is not `tested`; a second evidence of a catalog procedure 400 at `testTypeId`.
   - Every change: role check first (the organisation of the stored row, through the filter), then validation; `ConfirmedEmail` and antiforgery; 409 `project-archived` for a candidate of an archived project. The last change of the candidate and of its project moves with it (decision 5 (a)).
   - `CandidateService` loads the candidate with its evidences and its project, changes and saves once — the seam for the ETags of S6-20.
   - `WorkspaceAccess` holds the role check and the shared answers; `ProjectEndpoints` uses it instead of its private copies.
5. **Projects:** `ProjectResponse` gains `candidates[]` (id, name, vendor, test status, in the order they were added); `productCount` counts the candidates (list: one query with a count subquery).
6. **Other modules:** `ICatalogTestTypes` (Catalog.Contracts) with `CatalogTestTypes` on the cached filter values; `ProblemTypes.TestStatusConflict` in Platform.Contracts (also in `All`).
7. **Configuration:** 🔧 `Workspace:TimeZone` = `Europe/Berlin` in `API/API.ProcureAccess/appsettings.json`, read by `WorkspaceSettings` at the start.
8. **Tests.** New: `Unit/Workspace/CandidateProductTests.cs` (7 × `INV:WSP-01`, 1 × `INV:SIG-03`, the private-field check of the copied types), `Integration/Workspace/CandidateEndpointsTests.cs` (8 × `INV:WSP-01`, 1 × `INV:SIG-03`, 1 × `INV:TEN-01`, 1 × `INV:SEC-10`, 5 × `INV:API-01`, archived project, date edge in Berlin), `WorkspaceMigrationTests.CandidateTestStatus_Schema_NotNullWithDefaultUnknown` (`INV:SIG-03`). Changed: `OrganizationIsolationTests` (four samples; a sample is a list of rows), `AuthorizationMatrixTests` (seven actions × six users), `ProjectEndpointsTests` (`ShouldBeEquivalentTo`, the empty `candidates`).
9. **Documents.** TESTING v94 (V323 – V328; WSP-01, SIG-03, TEN-05 rows, §9 step 5), ARCHITECTURE v89 (C177; §4.3, §5.4, §7.2, §12 step 5), SPEC v2.51 (§7.2, §8.6, five rows), CODEBASE §2.96 and §3, CLAUDE.md (the state line and „Last tasks reflected“ only; 39 839 characters). `TESTING_v92.md` and `ARCHITECTURE_v87.md` deleted (two newest stay). Renumbered after S6-05 (#406) merged first with ARCHITECTURE v88 (C174 – C176), SPEC v2.50 and CODEBASE §2.95: this task's parts are ARCHITECTURE v89 (C177), SPEC v2.51, CODEBASE §2.96; TESTING was not touched by S6-05.

Not changed: client, e2e, UX copy, Figma mapping, `Dockerfile` (no new project), compose files, `ModuleMap`, TECH_DEBT (next version at the end of the sprint), the sprint file.

## 3. Self-review

Checklist of CLAUDE §6.7, with `/engineering:code-review` on the whole diff; findings sorted per §6.8.

- **fix (done):** R1 — `ChangeTestStatus` returned the removed evidences and `RemoveEvidence` the removed one, „so their private notes go too“, but the database cascade removes the notes and nothing used the values; both return what is needed only (§6.8: no mechanism the fix does not need). R2 — the test `ChangeEvidence_ToAProcedureAnotherEvidenceHas_Returns400AtTestTypeId` added two evidences at the same fake-clock time, so their order fell to the random ids and the full run failed once; the clock now advances between them.
- **follow-up:** R3 — two concurrent `POST …/test-evidences` of the same procedure both pass the aggregate check; the unique index keeps the data right, but the second answers 500 instead of 400 (open point 5). The other follow-ups are open points 1 – 4.
- **won't fix:** R4 — a PATCH of a candidate in an archived project that names unchanged values answers 409, like the projects of S6-11 (R3 there). R5 — `removeEvidences: true` with `testStatus: tested` is accepted and removes nothing; refusing it would only help a client that sends a meaningless flag.
- **Correctness:** text is trimmed and checked in the endpoint before the domain sees it; the domain's exceptions are guards. Every change is one load of the aggregate and one save; an empty or unchanged change answers 200 and keeps both `ModifiedAt`. Removing evidences with the status is one save, so no half state remains.
- **Security and tenancy:** the organisation of every request comes from the stored project, candidate or evidence through the filter, never a header; a non-member cannot tell an unknown row from a foreign one (404 both) and learns nothing from field errors (role check first). No `IgnoreQueryFilters()` and no raw SQL in production code: the TEN-02 allow-list is unchanged. Private fields are served only to members of the organisation and never logged.
- **Queries:** a candidate read is three queries (candidate with evidences, private section, the evidences' notes in one `IN`); a change adds the project; the project list counts in the same query. Indexes: candidate by project, evidence by candidate and procedure. No N+1.
- **Product rules:** no score, count or verdict about a product; `productCount` is a number of rows. „Nicht bekannt“ stays a workspace state (SIG-03); the publication of step 6 refuses it (SIG-10).

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format ProcureAccess.sln --verify-no-changes`: green.
- `dotnet test --solution ProcureAccess.sln` (unit, architecture, integration with Testcontainers / PostgreSQL 17): 1 961 passed, 0 failed, after the fixes R1 and R2 (the run before them: 1 960 passed, R2 failed once); after merging `main` with S6-05: 1 975 passed, 0 failed.
- OPS-03: `WorkspaceMigrationTests` (both migrations on an empty database, no pending model changes, applied at the start).
- `check-invariants` (WSP-01, SIG-03 active and tagged), `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` (38 packages, unchanged): green. `reuse lint` (full, through the `fsfe/reuse` image): compliant.
- Client tests and lint: not run, the client is unchanged. E2E: left to CI; no screen or seeded data changes.
- Figma: no screen in this task.

## 5. Open points

1. **Client mapping of `test-status-conflict`.** With the area `candidate` in `AREAS` (S6-16/S6-17), `PROBLEM_TYPE_KEYS` and S34a handle it: with `evidenceCount` > 0 the dialog `candidate.evidence.list.statusChange` (plural by count), then the repeat with `removeEvidences: true`. The UX copy §6.3 table gets its row with the next UX copy version.
2. **UX copy „(optional)“** on the S33 labels `candidate.form.vendor`, `.website`, `.summary`, `.contacts`, `.internalNotes`, and on `candidate.evidence.qualification` (decision 2 (a)); the S34a date field needs `errors.field.dateFuture` there as in S55. For the owner before S6-16/S6-17 (CLAUDE §6.5: no invented wording).
3. **Name-match hint** (`candidate.form.name.match`) and „Merken“ with the directory (R-CAN-01, M6.7); `SoftwareProductId` is ready.
4. **Candidates of an organisation that ends** stay as rows no request sees until S6-25 implements `IOrganizationDataDeletion` and `createdByMe.projects` for the Workspace (as S6-11 open point 2).
5. **Racing duplicate evidences** (R3) answer 500 from the unique index; map the unique violation to 400 at `testTypeId` with the concurrency work of S6-20, or as a small follow-up issue.
6. **CLAUDE.md is at 39 839 characters**, 161 below the warning limit; the next task that adds to its state line should shorten it (e.g. move the S5 list into CODEBASE).

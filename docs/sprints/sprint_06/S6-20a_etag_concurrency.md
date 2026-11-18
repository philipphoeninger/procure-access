<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-20a — Optimistic locking: ETag / If-Match for the workspace resources

> **Sprint:** 06, task S6-20, first part (split by owner decision D4 (a)). **Issues:** none.
> **Requirements / invariants:** R-PRJ-07, R-CAN-11 (server part, workspace resources). **API-03** (workspace part) and **API-04** become `active`; OPS-03 gains a case; TEN-05, WSP-01 – WSP-03 and SEC-10 are unchanged, their tests send `If-Match`.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **One EF migration:** Workspace `ResourceVersions` (the fourth migration of the module). No Configurator migration in this part; S6-20b adds one.
> **Scope:** the shared parts of the row of S6-20 in `SPRINT_06.md` — the 🔧 `ETagEpoch`, the tag, 428 and 409 — and the Workspace module: project, candidate, test evidence and the assignment of a list, with the client calls that change them. **Not in this part (S6-20b):** saved lists and the project's copy in Configurator, the race of S5-21a R1, the S11 editor. The dialog S37 is S6-21.
> **Owner decisions (2026-11-18, with the plan; all as recommended):** **D1 (a)** — the ETag comes from an integer `Version` column per resource, counted only for changes of the resource's own fields, not from `xmin` (ARCHITECTURE §7.1 reworded) · **D2 (a)** — `If-Match` on `PATCH /projects/{id}`, `PUT /projects/{id}/criteria-configuration` (the project's tag), `PATCH /candidates/{id}`, `PUT …/test-status`, `PUT` and `DELETE /test-evidences/{id}`, and on the three saved-list changes (S6-20b); creating requests and the assessment `PUT` need none · **D3 (a)** — 428 is the new problem type `precondition-required` without a UX key · **D4 (a)** — split by module into S6-20a and S6-20b.

## 0. Plan (written before the code)

- **Starting point:** `main` at aefaf4d (S6-18 and S6-16 merged). No concurrency anywhere; no column says who changed a row last; child changes stamp `ModifiedAt` of their parents; `ETagEpoch` does not exist.
- **Migration:** Workspace, `Version` and `ModifiedByUserId` on three tables.
- **Code:** the tag and the epoch in Platform; version, editor and `If-Match` in the Workspace services and endpoints; the client's two API services.
- **Tests:** per resource 428, 409 with the editor's name and success with a new tag; assessments without `If-Match`; the epoch.
- **Invariants:** API-03 (workspace part), API-04 `active` in TESTING v102.
- **Size:** one session for this part.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | ARCHITECTURE §7.1 derived the ETag from `xmin`. Every candidate, evidence and assessment change writes `ModifiedAt` of its candidate and project (S6-15 5 (a), S6-18 3 (a)), so `xmin` of those rows changes although nothing of them is overwritten | D1 (a): a `Version` column, counted by the service only for a change of the resource's own fields |
| F2 | No entity stored who changed it last; the 409 has to name the competing editor | `ModifiedByUserId` beside the version; existing rows have none (`editorDisplayName` is `null` for them) |
| F3 | With the version as EF concurrency token, a save that only stamps a parent fails when the parent got a new version in between — an assessment or a candidate change would answer 500 | `ConcurrentChange.RunAsync`: the change runs again on a fresh load (three attempts); the assessment `PUT` already had one retry |
| F4 | The candidate holds the rule between test status and evidences (WSP-01); a status change racing an added evidence could leave evidences on a candidate that is not „Ja“ | An evidence added or removed counts a version of the candidate; a changed field of an evidence counts on the evidence only |
| F5 | An evidence has no `GET` of its own | Its tag is the body member `etag` |
| F6 | SPEC §8.6 had no problem type for 428 | D3 (a) |
| F7 | The client changes only the candidate (S33) and assigns a list (S31) so far; rename and status come with S6-14, evidences with S6-17 | Two services changed; S6-14 and S6-17 have to send `If-Match` (open point 3) |
| F8 | The client image's nginx compresses JSON, and nginx turns a strong ETag into a weak one when it does; `/api/` is excluded already (`gzip off`) | Kept, with a comment that it must stay (open point 5) |

## 2. Changes

**Platform**
- `Platform.Contracts/Concurrency/EntityTags.cs`: `Of(version)` = `"<epoch>-<version>"`, `Matches(ifMatch, version)` — exactly one strong tag.
- `Platform/Concurrency/ConcurrencySettings.cs`, `PlatformModule`, `appsettings.json`: 🔧 `Concurrency:ETagEpoch` = 1, checked at the start.
- `ProblemTypes.PreconditionRequired` (`precondition-required`).

**Workspace**
- `ProcurementProject`, `CandidateProduct`, `TestEvidence`: `Version`, `ModifiedByUserId`, `MarkEdited(userId)`; `CandidateProduct.Create` and `AddEvidence` take the creator as an optional argument. Mappings: `IsConcurrencyToken()`, default 1. Migration `20261118223426_ResourceVersions`.
- `WorkspaceConcurrency.cs`: `EditPrecondition`, `ConcurrentChange.RunAsync`.
- `ProjectService`, `CandidateService`, `ProjectCriteriaListService`: the `If-Match` comparison after the load, the outcome `Conflict` with the editor, `MarkEdited` for a real change, every changing method inside `RunAsync`.
- `ProjectEndpoints`, `CandidateEndpoints`, `ProjectCriteriaListEndpoint`: the `If-Match` header, 428 after the role check, the `ETag` header on every answer with a project or a candidate; `WorkspaceAccess.PreconditionRequired()` and `ConflictAsync(…)` with `editorDisplayName` from `IUserDirectory`.
- `ProjectResponse`, `CandidateResponse`: `Etag` for the header (`[JsonIgnore]`); `TestEvidenceResponse`: the member `etag`.
- `CandidateService.ChangeEvidenceAsync`: a changed evidence is recognised by `evidence.Holds(facts)` (R1).

**Client**
- `core/concurrency/etag.ts` (`withEtag`, `ifMatch`) with its spec.
- `Project`, `Candidate`: `etag`; `TestEvidence`: `etag` of the body.
- `ProjectsApiService` (`create`, `get` keep the tag; `assignCriteriaList(…, projectEtag)`), `CandidatesApiService` (`get`, `create` keep it; `change(…, etag)`), `withWorkspace().assignCriteriaList`, `create-project-dialog`, `candidate-form`.
- `nginx.conf`: a comment at `gzip off` of `/api/`.

**Tests**
- New: `Workspace/WorkspaceConcurrencyTests.cs`, `Unit/Concurrency/EntityTagsTests.cs`, `Infrastructure/IfMatchRequests.cs`; cases in `AssessmentEndpointsTests`, `ProjectCriteriaListTests`, `WorkspaceMigrationTests`, `ProcurementProjectTests`.
- Adapted to send `If-Match`: `ProjectEndpointsTests`, `CandidateEndpointsTests`, `ProjectCriteriaListTests`, `ProjectLists`, `AuthorizationMatrixTests`; the client specs of the two services, the store, S31 and S33; `e2e/support/accounts.ts`.
- `Assign_TwoFirstAssignmentsAtOnce_…`: before, both assignments succeeded; now one answers 409.

**Documents**
- ARCHITECTURE v97 (C185; §7.1, §7.2, §9.2; v95 deleted), SPEC v2.59 (four rows, §8.6), TESTING v102 (V357 – V362; API-03, API-04 `active`; v100 deleted), `CODEBASE_v1.md` §2.104 and §3.
- `CLAUDE.md`: the rule „Concurrency (S6-20a)“ and the state line. The file was 34 characters below its limit of 40 000, so three clauses of the state line whose detail is in CODEBASE §2 are shorter now (preferences S5-25/26, Figma match S5-32, session hardening S5-29) and the step-5 clause is regrouped; no rule changed. 39 993 characters.
- `SPRINT_06.md` (the split in the row and the dependency of S6-21), `TASK_PROMPTS.md` (the prompt of S6-20b).
- No UX copy, Handoff, Figma mapping or `LEGACY_EXCEPTIONS` change; no screen, so no Figma frame to compare.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review` on the staged diff), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `ChangeEvidenceAsync` recognised a changed evidence by the candidate's `ModifiedAt`; two changes in the same instant looked like none, so the second counted no version (and before S6-20 stamped no last change) | **fixed**: asks `evidence.Holds(facts)` before the change; found by the stale-tag test |
| R2 | The assignment of a list stores the copy (Configurator context) before the project counts its version (Workspace context). A rename or status change of the project that lands in between answers 409 although the copy is replaced | **won't fix**: two contexts have no common transaction (as S6-18 open point 1); the other order would make the sender's own tag stale after a refused source. Documented in the service and C185; the reload the conflict asks for shows the new list |
| R3 | After three saves that each met a new concurrent change, the exception leaves as 500 | **won't fix**: needs three other changes of the same rows within milliseconds |
| R4 | `editorDisplayName` is the address of whoever made the stored version — also of someone who has left the organisation since; `null` for a deleted account or a row without an editor | **kept**: members saw that address on S21 while the person was a member; the text for „nobody“ is open (open point 2) |
| R5 | `ModifiedByUserId` is a user id that stays after the account is deleted, as `CreatedByUserId` does, and is not part of the personal data export | **follow-up (owner)**: open point 4 |
| R6 | A proxy in front of the client image that compresses API answers the way nginx does would weaken the tag, and every change would answer 409 | **follow-up**: open point 5; the image itself is safe (`gzip off`) |
| R7 | Security: the role check (404 non-member, 403 Reader) comes before 428 and 409, so neither says anything about a resource its sender may not change (tested); the editor's name is looked up only for a resource the sender may edit and goes into no log; antiforgery and `ConfirmedEmail` are unchanged; no raw SQL, no `IgnoreQueryFilters()`. N+1: the name is one query, only on a conflict; a change makes no query more than before. a11y: no screen | — |
| R8 | The helper `PatchCurrentAsync` of the tests reads the tag with a `GET` before each change | **kept**: it is what a client does; the tests of the precondition send fixed tags |

## 4. Checks

- **Backend:** build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution`: **2 141 passed, 0 failed** (before the migration case was added; `WorkspaceMigrationTests` then run alone: 5 passed).
- **Client:** `npm run lint` clean; `npx ng test`: **1 008 passed**; `tsc` on the specs clean.
- **E2E:** `npm run typecheck` and `npm run lint` clean. The suite was not run locally — another session (worktree `s6-14`) may use the stack — and runs in CI.
- **CI, first run (f88cd10):** every stage green except E2E — in all five browsers the one test of `e2e/shell/reflow.spec.ts` that answers the project and the candidate itself (S33, S34) failed, because its answers had no `ETag` and the client refuses such an answer (`withEtag`). **Fixed** in the test: both mocked answers carry `ETag: "1-1"`. A spec that answers a project or a candidate itself has to send the header.
- **Scripts:** `check-invariants` (gate passed with API-03 and API-04 active), `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` passed; `reuse lint` compliant.

## 5. Open points

1. **S6-20b:** `CriteriaConfiguration` with version and editor (one Configurator migration), `If-Match` on `PATCH …/criteria/{id}`, `PUT …/selection` and `DELETE`, the race of S5-21a R1 as a test, `ConfiguratorApiService`, and `SavedListEditor` with one save chain per list. It completes API-03 (TESTING V262).
2. **S6-21 (S37):** `conflict.body` needs `{{name}}`; the UX copy has no wording for a conflict whose editor is unknown (`editorDisplayName` `null`). Until S37 exists, a 409 `conflict` shows the general error text.
3. **S6-14 and S6-17** build rename, status change, test status and evidences on the client: their requests need `If-Match` (`Project.etag`, `Candidate.etag`, `TestEvidence.etag`; `ifMatch()`), and after an evidence is added or removed the candidate has a new tag, so S34a reads the candidate again before it changes the test status. S6-14 runs in a worktree now: whichever of the two pull requests merges second adapts.
4. **Owner — `ModifiedByUserId` and privacy:** should the personal data export name the resources a user changed last, and should deleting an account clear the id? Today it is handled like `CreatedByUserId` (kept, not exported beyond the creator's projects).
5. **Operations:** a reverse proxy in front of the client must not compress `/api/` answers in a way that weakens ETags; the restore runbook (`TECHNICAL.md`, not written yet) has to name `Concurrency:ETagEpoch`.
6. **Deviations from the plan:** the epoch test uses one host — a unit test and a request with the stored version under another epoch — instead of a second host with a raised epoch (TESTING V360); `CLAUDE.md` was shortened to stay under its limit (§2 above).

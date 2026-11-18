<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-18 — Assessment server: rows, allowed values, derived rows, counts, lock

> **Sprint:** 06, task S6-18. **Issues:** none.
> **Requirements / invariants:** R-CAN-06 – R-CAN-08 (server part), R-PRJ-04 (lock). **CAT-15**, **CAT-17** (workspace part), **SIG-13** and **WSP-02** become `active`; WSP-03 gains its two candidate clauses and stays green; TEN-01, TEN-04, TEN-05, SEC-10 and API-01 gain cases.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **One EF migration:** Workspace `CandidateAssessments` (the third migration of the module).
> **Scope:** exactly the row of S6-18 in `SPRINT_06.md`: `CandidateAssessment` rows created lazily, the value checked against the union of allowed ratings of the list's app types, the note ≤ 1 000, last-write-wins per row, procedure criteria derived and never stored or counted, progress and the seven counts on the project and candidate views pinned to the copy's release, the lock of the list, excluded criteria keeping their assessments. No client change (S6-19, S6-14), no candidate removal (S6-22), no ETags (S6-20), nothing published (step 6).
> **Owner decisions (2026-11-18, with the plan; all as recommended):** **1 (a)** — the rows are read by their own `GET /candidates/{id}/assessments`; progress, the seven counts and the procedure answers ride on `GET /projects/{id}` per candidate, progress and counts on `GET /candidates/{id}` · **2 (a)** — a write to anything but an included requirement criterion of the project's list (a procedure criterion, an excluded one, one outside the list, a project without a list) answers 400 `validation` at `criterionRevisionId` · **3 (a)** — a changed assessment moves `ModifiedAt` of the candidate and of its project · **4 (a)** — replacing the list deletes the project's rows without a value (notes on criteria of the replaced copy) · **5 (a)** — the assessments read says whether the list's app types are tested by more than one procedure (`ratingsFromSeveralProcedures`, for `candidate.assessment.unionNote`).

## 0. Plan (written before the code)

- **Starting point:** `main` at e8fa463 (S6-15, S6-12a and S6-12b merged). Workspace holds projects, candidates and evidences; the lock of the list is the hook `ICriteriaListLock` → `NoAssessmentsYet`.
- **Migration:** one, in the Workspace module: table `CandidateAssessments`.
- **Code:** the entity; a reader that resolves the project's copy under its release through the existing contracts; a pure projection for union check, counts and derived answers; two endpoints; counts on the project and candidate views; the lock on the rows. `Catalog.Contracts` exposes the allowed-ratings union it already computes.
- **Tests:** unit tests of the projection and the row; endpoint tests on the stored test release; the lock and the WSP-03 clauses on real rows; matrix rows and the isolation sample.
- **Invariants:** CAT-15, CAT-17 (workspace part), SIG-13, WSP-02 `active` in TESTING v99.
- **Size:** one session, no split.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | SPEC had no contract for the assessment endpoints; ARCHITECTURE §7.2 named only the `PUT` and C34 put the counts and `procedureAnswers[]` on `GET /projects/{id}` | Decisions 1 (a), 2 (a); the contract is SPEC v2.56 |
| F2 | The union of allowed ratings (C31) and the test type of a procedure criterion were computed inside the Catalog module (`CriteriaResolver`) but not part of `ICatalogSelectionResolver` | `ResolvedCatalogCriterion` gains `AllowedRatings` and `TestTypeKey`, `ResolvedCatalogSelection` gains `MappedTestTypeKeys`; no new module dependency |
| F3 | The project's copy lives in the Configurator context, the assessments in the Workspace context (ADR-014): lock check and copy cannot share a transaction | Recorded as a known gap, not built around (open point 1) |
| F4 | A row may hold a note without a value. By C53 that is no assessment, so it does not lock — and after a replace it would point at a criterion of the old copy | Decision 4 (a): such rows are deleted with the replace |
| F5 | S6-15 5 (a) moves `ModifiedAt` of candidate and project with every change of a candidate; S6-20 will add ETags on both | Decision 3 (a), with the note for S6-20 (open point 2) |
| F6 | V336 and V340 left the copy tests of WSP-02 untagged „until S6-18 flips the row“, one of them in the client (`create-project-dialog.spec.ts`) | Tagged here, including the one client spec title |
| F7 | `CLAUDE.md` on `main` has 40 073 characters, above its own limit of 40 000 | Not changed by this task (no cross-cutting rule); open point 5 |

## 2. Changes

**Catalog.Contracts, Catalog**
- `ResolvedCatalogCriterion.AllowedRatings` (the union of C31; none for a procedure criterion) and `.TestTypeKey`; `ResolvedCatalogSelection.MappedTestTypeKeys` (init property). `CatalogSelectionResolver` fills them from what `CriteriaResolver` already resolves.

**Workspace**
- `Domain/CandidateAssessment` (key: candidate and criterion revision; `OrganizationId`, `Fulfillment`, `EvidenceNote` ≤ 1 000, `ModifiedAt`), `Domain/Fulfillment`; `CandidateAssessmentConfiguration`; the set and the organisation filter in `WorkspaceDbContext`; migration `20261118203404_CandidateAssessments` (`internal`).
- `Assessments/AssessmentProjection` — pure: wire names, `IsAllowed` (CAT-15), `AnswerOf` (CAT-17), `SummaryOf` (progress, seven counts, procedure answers); `ProjectCriteriaScope`.
- `Assessments/AssessmentReader` — the scope of a project's list (copy → saved list → resolved under its release), the summaries of the candidates of a project in one query, the assessment tab.
- `Assessments/AssessmentService` — the write: archive, list membership, union, lazy row, last change; one repetition when two writes meet at the key.
- `Assessments/AssessmentEndpoints`, `AssessmentContracts` — `GET /candidates/{id}/assessments`, `PUT /candidates/{id}/assessments/{criterionRevisionId}`.
- `CandidateSummaryResponse` (+ `Progress`, `Fulfillment`, `ProcedureAnswers`), `CandidateResponse` (+ `Progress`, `Fulfillment`); `ProjectService` and `CandidateService` ask the reader.
- `ProjectCriteriaListService`: the lock on the assessment rows (`IsLockedAsync`), the rows without a value deleted after a replace. `Projects/ICriteriaListLock.cs` (with `NoAssessmentsYet`) is deleted.
- `WorkspaceModule`: registrations and the endpoint map.

**Tests**
- `Unit/Workspace/AssessmentProjectionTests` (new): CAT-15 table, CAT-17 answers and „never counted“, SIG-13 counts and contract, the row's `Change`.
- `Integration/Workspace/AssessmentEndpointsTests` (new, 26 cases) and the helper `ProjectLists`.
- `Integration/Workspace/ProjectCriteriaListTests`: `Replace_LockedList_…` with its test double replaced by `Replace_OnceACandidateHasAnAssessment_…` and `Replace_WhileOnlyRowsWithoutAValueExist_…`; new `ExcludeAndInclude_CriteriaOfAnAssessedProject_…` (WSP-03); three copy tests tagged `INV:WSP-02`.
- `AuthorizationMatrixTests` (`read-assessments`, `set-assessment`), `OrganizationIsolationTests` (sample), `CandidateEndpointsTests` (the summary's new members).
- `Unit/Configurator/CriteriaConfigurationTests`: two tests tagged `INV:WSP-02`; three unit tests adapted to the two new members of `ResolvedCatalogCriterion`.
- Client: the title of one spec in `create-project-dialog.spec.ts` carries `INV:WSP-02` (V340); no other client change.

**Documents**
- `TESTING_v99.md` (V342 – V347; CAT-15, CAT-17, SIG-13, WSP-02 `active`), `ARCHITECTURE_v94.md` (C182; §4.3, §5.4, §7.2, §7.3, §12 step 5), `SPEC_v2.md` v2.56, `CODEBASE_v1.md` §2.101 and §3 (and the lock note of §2.99). `TESTING_v97.md` and `ARCHITECTURE_v92.md` deleted as third-newest. `CLAUDE.md` unchanged.

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` on the staged diff. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | With a `:int` route constraint, a malformed `criterionRevisionId` answered 404 instead of the 400 Problem Details every other route value gives (found by `ProblemDetailsTests`, API-01) | **Fixed:** no constraint; the binding failure is a 400 `validation` |
| R2 | A copy replaced between the reader's two reads made `ScopeOfAsync` throw (500 on a project or candidate read) | **Fixed:** reads as „no list“; the next read sees the new copy |
| R3 | Lock check and copy are not one transaction (F3): an assessment saved in between survives against the old list and locks the new one | **Follow-up** (open point 1). Not a regression: the same window existed for two assignments at once since S6-12a |
| R4 | The write resolves the whole list on every autosave (two Configurator reads, the catalog snapshot from the cache) | **Won't fix:** two small reads per save of one user; copying the scope into the workspace would be a second source of truth beside the copy, which can be adjusted at any time (C51) |
| R5 | The repeated write logs EF's first `DbUpdateException` as an error | **Won't fix:** expected and rare (two first writes of the same row within milliseconds); an `INSERT … ON CONFLICT` would be raw SQL with a TEN-02 allow-list entry |
| R6 | `SetAsync` repeats after any `DbUpdateException`, not only a key violation | **Won't fix:** the repetition reads fresh state and fails again for any other cause, which then surfaces |
| R7 | A value on an excluded criterion locks the list although it is hidden | **Intended** (WSP-02: „any candidate has an assessment“); the hidden assessment would be lost by a replace |

Security: both endpoints sit in the authenticated group; the organisation comes from the stored candidate through the query filter (404 for another organisation), the write needs Editor, `ConfirmedEmail` and antiforgery (middleware; the exemption list is unchanged). Bodies refuse unknown members. No raw SQL and no `IgnoreQueryFilters()`; `ExecuteDeleteAsync` runs through the filter. No N+1: one grouped read of the rows per project view. No private field is in an answer that did not have it; no answer has a score, a percentage or `notCovered` (contract test).

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` | 2 086 passed (Unit 859, Architecture 67, Integration 1 160) |
| `dotnet ef migrations has-pending-model-changes` (WorkspaceDbContext) | no changes |
| `npx ng test` for `create-project-dialog.spec.ts` (title changed) | 11 passed |
| `node scripts/check-invariants.mjs` | passed; CAT-15 2, CAT-17 4, SIG-13 3, WSP-02 8, WSP-03 8 counted |
| `node scripts/check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | passed (nothing of theirs changed) |
| `reuse lint` | compliant (1 667 files) |
| E2E | not run locally: no client or E2E change; CI runs it |

## 5. Open points

1. **Lock and copy are not atomic (for S6-20).** The copy is stored by Configurator, the assessments by Workspace. An assessment saved between the lock check and the copy keeps its value, locks the new list and may belong to a criterion the new list does not have (invisible, so the user cannot unlock). Options: a row lock on the project in both paths (raw SQL, allow-list entry), or the `If-Match` S6-20 gives the project copy together with a version bump of the project on every assessment. Recommendation: decide with S6-20.
2. **ETag of the candidate (for S6-20).** Decision 3 (a) moves the candidate's `ModifiedAt` with every autosaved assessment. If the candidate's `ETag` is derived from `ModifiedAt`, an open candidate form (S33) becomes stale by the user's own assessment. Recommendation: a version of the candidate's own fields, or the assessments excluded from it.
3. **Client contract (for S6-19, S6-14).** The models of `withWorkspace()` do not know `progress`, `fulfillment`, `procedureAnswers` and the assessments read yet; SPEC v2.56-1 – 3 are their contract.
4. **Organisation and account deletion.** Workspace still implements no `IOrganizationDataDeletion` and no section of the personal data export (as after S6-15). Assessment rows hold no user id and go with their candidate by cascade once the projects are deleted. Belongs to the task that adds the deletion for the module (S6-25 names it for candidates).
5. **`CLAUDE.md` is 73 characters above its limit of 40 000** on `main` (not from this task). Proposed: shorten the state paragraph of §3 with the next task that changes it.
6. **Published assessments (step 6).** CAT-15 and CAT-17 are active for the workspace part; `PublishedAssessment` and the directory's `procedureAnswers` follow with Flow B, D and E.

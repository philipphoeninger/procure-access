# S7-08 — Workspace correctness: archived copy on the server, lock and copy atomic

> **Sprint:** 07 · **Task:** S7-08 (2 points) · **Issues:** none (TECH_DEBT_v6 TD-86, TD-87; ARCHITECTURE O-30, O-31)
> **Requirements / invariants:** R-PRJ-04, R-PRJ-05, R-PRJ-07, R-CAN-06, R-SAV-03; WSP-02, API-03 and TEN-02 gain cases; WSP-03, TEN-05, ARC-01 – ARC-03 stay green; no invariant flipped. New versions `ARCHITECTURE_v109.md` (C197; v107 deleted), `TESTING_v116.md` (V419, V420; v114 deleted), SPEC v2.70
> **Status:** done — pull request open (2026-11-26)
> **Scope:** exactly the row of S7-08. Backend only: no EF migration, no new dependency, no client change, no new UX key.
> **Owner decisions (2026-11-26, with the plan):** **O-S7-4** — O-30: the server refuses a change to the copy of an archived project, Configurator asks Workspace through a contract; O-31: a row lock on the project in both paths (assignment and assessment) · **answer of the refusal:** 409 `project-archived` (the existing type and client text), not `project-copy` · **strictness of the archived check:** a plain read before the save, not under the project's row lock.

## 0. Plan (written before the code)

- **Starting point:** `main` at 2d20941c. `ConfigurationService.DecideAsync` changes any copy it finds (TD-86). `ProjectCriteriaListService.AssignOnceAsync` checks the lock, lets Configurator store the copy in its own context and then counts the project's version, with nothing holding the project in between; `AssessmentService` takes no lock (TD-87). `OrganizationRowLock` (IdentityAccess, S5-16a) is the existing pattern for a row lock with a deterministic test.
- **TD-86:** a contract in `Configurator.Contracts`, implemented by Workspace, asked by `DecideAsync` for a project's copy. Against ARCHITECTURE §4.3: Workspace → Configurator.Contracts is in the table, Configurator gets no new reference, the table and `ModuleMap.cs` stay; no cycle (the inversion of `IEntrySourceAuthorizer`). Order of the checks: 404, stale `If-Match`, archived, then criterion and fields.
- **TD-87:** the assignment locks the project's row before it loads the project; the assessment locks the candidate's row, then the project's, before it reads the list. A rename needs no code: its `UPDATE` waits and then meets the new version.
- **Deadlock order:** candidate row → project row → assessment rows; workspace before Configurator. The candidate first, because the saves of a candidate and an evidence write the candidate and then stamp the project (to be confirmed from the logged SQL). The TD-86 read takes no lock and adds no edge.
- **Not covered:** the copy still commits in Configurator's context; a process that ends before the workspace's commit leaves the copy replaced and the version uncounted.
- **Invariants:** none activated. **Tests:** two races through the API with the test holding the row lock and `pg_stat_activity` deciding the order, the rename race, the archived copy, the allow-list entry. **Migration:** none. **One session:** yes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | O-S7-4 was not recorded as decided: the task prompt had a placeholder, `SPRINT_07.md` only the proposal | Asked with the plan; decided as proposed (header) |
| F2 | The saves of the workspace write `CandidateAssessments`, then `CandidateProducts`, then `ProcurementProjects` in one batch (EF orders by table; confirmed from the logged SQL of a scratch test) | The assessment path locks the candidate before the project; a project-first order could wait in a circle with every candidate and evidence save |
| F3 | The removal of a candidate deletes its assessment rows before it stamps the project; an assignment holds the project and deletes the notes without a value | One circle is left. PostgreSQL ends it (`40P01`); `PostgresException.IsTransient` is `true` for it (checked), so the execution strategy of the context runs the ended change again |
| F4 | With both locks held, nothing the assessment's repeat after `DbUpdateException` was for can happen any more: two first writes of a row, and a new version of the candidate or project it stamps, wait for the locks | The repeat is removed |
| F5 | My first rename test (the test holds the lock, assignment then rename wait) would have passed against the old services too: the old assignment also waited, at its last `UPDATE`, after the copy | Rewritten: the rename is sent between the copy and the version (`CopyThen`). All three race tests fail against the services before this task (checked by stashing the two services) |
| F6 | No invariant covers „archived is read-only“ (R-PRJ-05); `Assign_ArchivedProject_…` has no tag either | The archived-copy test carries no `INV:` tag |

## 2. Changes

| File | Change |
|---|---|
| `src/ProcureAccess.Modules.Configurator.Contracts/Configurations/IProjectStatusReader.cs` (new) | `IsArchivedAsync(projectId)`; declared here, implemented by Workspace |
| `src/ProcureAccess.Modules.Workspace/Projects/ProjectStatusReader.cs` (new), `WorkspaceModule.cs` | The filtered read and its registration |
| `src/ProcureAccess.Modules.Configurator/Configurations/ConfigurationService.cs`, `ConfigurationEndpoints.cs` | `DecideAsync` refuses the copy of an archived project after the `If-Match` check: `EditOutcome.ProjectArchived` → 409 `project-archived`; the endpoint's description names it |
| `src/ProcureAccess.Modules.Workspace/WorkspaceRowLock.cs` (new) | `InTransactionAsync` (transaction inside the execution strategy), `LockProjectAsync`, `LockCandidateAsync` (raw `SELECT … FOR UPDATE`, need a transaction); the lock order in its remarks |
| `src/ProcureAccess.Modules.Workspace/Projects/ProjectCriteriaListService.cs` | The assignment runs in a transaction, locks the project after a filtered read found it, then loads and checks it |
| `src/ProcureAccess.Modules.Workspace/Assessments/AssessmentService.cs` | The save runs in a transaction and locks candidate, then project, before it reads the list; the repeat after `DbUpdateException` is gone (F4) |
| `tests/…/Workspace/ProjectCriteriaListTests.cs` | „INV:WSP-02 `Replace_WhileAnAssessmentIsBeingSaved_…`“, „INV:WSP-02 `Assess_WhileTheListIsBeingReplaced_…`“, „INV:API-03 `Rename_BetweenTheCopyAndTheVersionOfAnAssignment_…`“, `Decide_CopyOfAnArchivedProject_Returns409ProjectArchivedAndKeepsTheCopy`; helpers `HoldProjectLockAsync`, `WaitForLockWaitersAsync`, `CopyThen` |
| `tests/…/Architecture/Tenancy/IgnoreQueryFiltersAllowListTests.cs` | `WorkspaceRowLock.cs` = 2 calls, with the reason (TEN-02) |
| `tests/…/Configurator/SavedListConcurrencyTests.cs` | The hand-built `ConfigurationService` gets the host's `IProjectStatusReader` |
| `docs/ARCHITECTURE_v109.md` (v107 deleted) | C197; §4.3 rows of Configurator and Workspace and the contracts paragraph; §5.4 `ProcurementProject`; §7.2 the `PATCH`; §16.5 O-30 and O-31 closed |
| `docs/TESTING_v116.md` (v114 deleted) | V419, V420; the notes of WSP-02 and API-03 |
| `docs/SPEC_v2.md` | v2.70-1 (replaces v2.54-5), v2.70-2; header |
| `docs/CODEBASE_v1.md` | §2.121, the sentence in §1, the layout lines of Configurator.Contracts and Workspace |
| `CLAUDE.md` | Concurrency rule: the row lock and its order (candidate, then project) |

**Tests without sleeps.** The two API races hold the project's row lock on a connection of the test, start the first request, wait until `pg_stat_activity` shows one session waiting for a lock, start the second, wait for two and release; PostgreSQL serves the waiters of a row in the order they arrived. The wait polls a database condition and fails the test after 15 s; no delay decides an outcome.

## 3. Self-review (CLAUDE §6.7, §6.8)

`/engineering:code-review` on the diff; verdict: approve.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: the raw SQL is an interpolated `ExecuteSqlAsync` (parameters, no string building); the locks are taken only after a filtered read found the row (TEN-01); the archived read runs through the organisation filter | No finding |
| R2 | The copy commits in Configurator's context before the workspace's commit (crash between the two) | **Follow-up** (open point 1): not a race, and closing it needs one transaction over two contexts |
| R3 | When the execution strategy repeats an assignment whose commit did reach the database (connection lost during the commit), the repeat answers 409 `conflict` for a stored assignment | **Won't fix**: needs a lost connection in the commit; the reload the conflict asks for shows the list. The assessment's repeat is harmless (last write wins) |
| R4 | A request that waits for the row lock holds a database connection (and the assessment a second and third for Configurator and Catalog once it runs) | **Won't fix**: the waits are the length of one assignment; the pool (100) is far from the autosave of a few members |
| R5 | The remaining circle with the removal of a candidate (F3) costs PostgreSQL's deadlock timeout (1 s) before the retry | **Won't fix**: needs a removal and a replacement of the same project in the same moment, with a note without a value on that candidate; making every candidate path lock the project first would be a larger change than O-S7-4 decided |
| R6 | `WaitForLockWaitersAsync` exists twice (here and in `OrganizationEndpointsTests`) | **Follow-up** (open point 3): moving the older one is outside this row |
| R7 | N+1, edge cases: one extra read per assignment and per assessment, one per `PATCH` of a copy; a candidate or project deleted between the filtered read and the lock answers 404 | No finding |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 2 | `dotnet build ProcureAccess.sln -warnaserror …` | ✅ 0 warnings |
| 3 | `dotnet test --solution ProcureAccess.sln` (unit, architecture, integration) | ✅ 2265 passed; run before the document changes, which touch no code |
| 4 | The three race tests against the services before this task (stash) | ✅ all three fail at the wait for the row lock, as they must |
| 5 | `node scripts/check-invariants.mjs` (reads `TESTING_v116.md`), `check-figma-links`, `check-i18n`, `check-icons`, `check-base-images`, `check-api-notices` | ✅ |
| 6 | `reuse lint` | ✅ compliant |
| 7 | Client (lint, Vitest) and E2E | not run: no client or E2E file changes; left to CI |

## 5. Open points

1. **Crash between the two commits (R2).** The copy is replaced, the notes without a value and the project's version are as before; a new assignment repairs it. Closing it needs both contexts on one connection and transaction — an architecture decision (ADR-014 keeps the contexts apart). Proposed as a new TECH_DEBT item with the next version; no owner decision needed now.
2. **TECH_DEBT.** TD-86 and TD-87 are resolved; `TECH_DEBT_v6.md` is unchanged, because its new version comes at the end of the sprint (process step 14).
3. **`WaitForLockWaitersAsync` twice (R6):** a shared helper in the integration test infrastructure when either test is touched next.
4. **Stale headers, as S7-09 open point 5:** the Handoff and the Figma mapping still name `ARCHITECTURE_v107.md`, `TESTING_v114.md` and SPEC v2.69. Left for the next document upkeep.
5. **`SPRINT_07.md` has no column for a decision:** O-S7-4 is recorded here and in ARCHITECTURE C197 and §16.5, not in the sprint's table.
6. **Parallel sessions (worktrees s7-05, s7-10):** if one of them merges `ARCHITECTURE_v109`, `TESTING_v116` or SPEC v2.70 first, this branch is rebased and renumbered.

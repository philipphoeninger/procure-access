<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-20b — Optimistic locking: ETag / If-Match for saved lists and the project copy

> **Sprint:** 06, task S6-20, second part (split by owner decision S6-20 D4 (a)). **Issues:** none.
> **Requirements / invariants:** R-SAV-03, R-PRJ-07 (the project's copy), R-CAN-11 (server part, list edits). **API-03** is complete; API-04 and OPS-03 gain cases; TEN-05, WSP-03 and CAT-08 are unchanged, their tests send `If-Match`.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **One EF migration:** Configurator `ListVersions` (the third migration of the module). No Workspace migration.
> **Scope:** the second part of the row of S6-20 in `SPRINT_06.md`: version, editor and `If-Match` on the saved lists and the project's copy, the race of S5-21a R1 as a test, the client calls that change a list and the S11 editor. The dialog S37 is S6-21.
> **Owner decisions:** none new. S6-20 **D1 (a)** (a version column, not `xmin`), **D2 (a)** (`PATCH …/criteria/{id}`, `PUT …/selection` and `DELETE` of a list need `If-Match`), **D3 (a)** (428 `precondition-required` without a UX key) and **D4 (a)** (the split) of 2026-11-18 apply as taken with S6-20a.

## 0. Plan (written before the code)

- **Starting point:** `main` at c92bfb4 (S6-20a merged): `EntityTags`, the epoch, `precondition-required`, `core/concurrency/etag.ts` and `IfMatchRequests` exist; the saved lists have no concurrency, and a decision racing a selection change answers 500.
- **Migration:** Configurator, `Version` and `ModifiedByUserId` on `CriteriaConfigurations`.
- **Code:** the entity, the three changing paths of `ConfigurationService`, their endpoints; `ConfiguratorApiService`, `SavedListEditor`, S10's selection change, S14's deletion.
- **Tests:** 428, 409 and success per change; the race made deterministic; API-03 complete in TESTING v104.
- **Two details chosen within the decisions:** every stored decision counts a version; the lists of S14 name their tag in the body.
- **Size:** one session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `Decide` stamps the list's last change with every call, also for the same values; the client sends a decision only when it differs from the saved one | Every stored decision counts a version; no comparison added on the server |
| F2 | S14 deletes a list from the answer of `GET /configurations`, which had nothing to send in `If-Match` | `ConfigurationSummaryResponse` gets the body member `etag` (as the evidences in S6-20a) |
| F3 | Nothing writes a list's row from below (no stamps as in the Workspace) | No retry: a save that meets a concurrent change is the conflict itself (`SaveAsync`) |
| F4 | Replacing a project's copy deletes the old copy; with the version as token that delete fails when the copy was changed on S11 since it was loaded | `ProjectCriteriaLists` repeats the replacement once, as it did for a lost insert race |
| F5 | `SavedListEditor` kept one save chain per criterion: saves of two criteria ran side by side and would now send the same tag | One chain per list |
| F6 | Three answers of `e2e/shell/reflow.spec.ts` stand for a saved list (the lesson of S6-20a's first CI run) | They carry an `ETag` |

## 2. Changes

**Configurator**
- `CriteriaConfiguration`: `Version`, `ModifiedByUserId`, `MarkEdited(userId)`; `Create` starts at version 1 with the creator. Mapping: `IsConcurrencyToken()`, default 1. Migration `20261119105213_ListVersions`.
- `ConfigurationService`: `DecideAsync`, `ChangeSelectionAsync` and `DeleteAsync` take a `ListEdit`, refuse a stale `If-Match` (`EditOutcome.Conflict`) and save through `SaveAsync`, which answers a concurrent change as the conflict (or 404 for a list that is gone). `FindAsync` and `ListAsync` name the tag.
- `ConfigurationEndpoints`: the `If-Match` header on the three changes, 428 after the role check, 409 `conflict` with `editorDisplayName`, the `ETag` header on `GET`, `PATCH` (204) and `PUT`.
- `ProjectCriteriaLists.CopyIntoProjectAsync`: the second attempt also after a `DbUpdateConcurrencyException`.

**Client**
- `SavedList.etag`, `SavedListSummary.etag`; `ConfiguratorApiService.get`, `decide`, `changeSelection`, `delete`.
- `SavedListEditor`: the tag, one save chain per list.
- `SavedListSelection` and `criteria-select` (S10 of a saved list), `saved-lists-page` (S14), `saved-list-page`.

**Tests**
- New: `Configurator/SavedListConcurrencyTests.cs`; cases in `ConfiguratorMigrationTests`, `configurator-api.service.spec.ts`, `saved-list-editor.spec.ts`.
- Adapted to send `If-Match`: `SavedListChangeTests`, `ShareLinkTests`, `SavedExportEndpointTests`, `ProjectCriteriaListTests`, `AuthorizationMatrixTests` (through `PatchListAsync`, `PutListAsync`, `DeleteListAsync`, new in `IfMatchRequests`); the client specs of S10, S14 and the saved list; `e2e/shell/reflow.spec.ts`.

**Documents**
- ARCHITECTURE v99 (C187; §7.1, §7.2; v97 deleted), SPEC v2.61 (three rows), TESTING v104 (V369 – V373; API-03 complete; v102 deleted), `CODEBASE_v1.md` §2.106 and §3.
- `CLAUDE.md`: the concurrency rule names S6-20 as a whole, the state line the lists (39 999 characters).
- No UX copy, Handoff or Figma mapping change; no screen, so no Figma frame to compare.

## 3. Self-review

Code review per CLAUDE §6.7 (the checklist of `/engineering:code-review` on the diff), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | After a 409 the editor keeps its stale tag, so every further change on S11 fails too and shows the general error each time, until the page is loaded again | **kept until S6-21**: that is the behaviour S37 describes („saving stays blocked until reload“, Handoff §5.1); the dialog that says so is S6-21 (open point 1) |
| R2 | A stale `If-Match` is answered before `project-copy` and before the validation, so a request that would be refused anyway learns of the conflict first | **kept**: the same order as in the Workspace (conflict before `project-archived`); no screen sends such a request |
| R3 | The 428 and 409 answers exist twice, in `WorkspaceAccess` and in `ConfigurationEndpoints` (about 25 lines) | **won't fix**: `Platform.Contracts` has no ASP.NET reference, and a shared project for two helpers is more than they need |
| R4 | A repeated decision with the stored values counts a version | **kept** (F1): the client does not send one; the list already stamped its last change for it |
| R5 | Security: role check before 428 and 409 (tested with a Reader and another organisation); the editor's name only for a list the sender may edit, in no log; antiforgery and `ConfirmedEmail` unchanged; no raw SQL, no `IgnoreQueryFilters()`. N+1: the list of S14 is still one query; the name is one query on a conflict. a11y: no screen | — |

## 4. Checks

- **Backend:** build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution`: **2 157 passed, 0 failed** (the migration case added afterwards: `ConfiguratorMigrationTests` 4 passed).
- **Client:** `npm run lint` clean; `npx ng test`: **1 012 passed**. In two earlier runs, while the backend suite ran beside them, one test of `export-dialog.spec.ts` timed out; alone and in the final run it passes.
- **E2E:** `npm run typecheck` and `npm run lint` clean; the suite runs in CI.
- **Scripts:** `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` passed; `reuse lint` compliant.

## 5. Open points

1. **S6-21 (S37):** the dialog for a 409 `conflict` on the workspace forms and on S11; `conflict.body` needs a wording for an unknown editor (`editorDisplayName` `null`, S6-20a open point 2). Until then S11 shows the general error for every change after a conflict (R1).
2. **Owner — `ModifiedByUserId` and privacy** (S6-20a open point 4) now also concerns the saved lists: export and account deletion treat it like `CreatedByUserId`.
3. **S6-14 and S6-17** still have to send `If-Match` for the project and evidence changes they build (S6-20a open point 3).
4. **Operations** (S6-20a open point 5): no compression of `/api/` by a proxy that weakens ETags; `Concurrency:ETagEpoch` in the restore runbook.

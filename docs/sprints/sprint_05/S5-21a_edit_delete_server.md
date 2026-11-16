# S5-21a — Changing, deleting, copying and exporting a saved list (server)

> **Sprint:** 05 · **Task:** S5-21a (server part of S5-21, 4.5 points for S5-21 in all) · **Issues:** none
> **Requirements / invariants:** SPEC R-SAV-03, R-SAV-06, R-LST-12, R-LST-14, §7.3, §8.3, §8.10 (v2.37); **CAT-22** active (TESTING_v77 V255); **EXP-01, EXP-03** persisted parts active (V256); EXP-06 endpoint (V257); TEN-05 rows, TEN-01 (V258); SEC-10 cases (V259); API-05 `SavedExport` (V260); CAT-08 reworded (V261); API-03 for saved lists stays with step 5 (V262); ARCHITECTURE C155
> **Status:** done — pull request open (2026-11-16)
> **Scope:** `PATCH /api/v1/configurations/{id}/criteria/{criterionRevisionId}`, `PUT …/{id}/selection`, `DELETE …/{id}`, `POST …/{id}/copies` and `POST …/{id}/exports`, with the catalog carry-over to the active release and the slug of the list name in the file name. No screen: editing on S11, „Auswahl ändern“, the delete dialog, the older-release notice and J1's file comparison are S5-21b.
> **Owner decisions (2026-11-16, before the task):** S5-21 **1a** — split: S5-21a server, S5-21b client · **2a** — no ETag/If-Match on saved lists until API-03 in migration step 5; until then the last change wins · **3a** — „Auswahl ändern“ as a new `PUT /configurations/{id}/selection`; the server keeps the decisions of criteria that stay · **4a** — the decisions a selection change drops are reported (the UX text is drafted in S5-21b) · **5a** — on a list of an older release the criteria stay editable, „Auswahl ändern“ is hidden (the server refuses it with `release-mismatch`) · **6a** — the copy keeps the name · **7a** — „Liste löschen“ only on S14 (S5-21b) · **8b** — a neutral text instead of „Von Ihnen geändert“ on saved lists, drafted in S5-21b · **a** (during planning) — a weight without a chosen level is carried as „B chosen“ when the new recommendation is not B · **defaults of the plan, confirmed:** every change of a list needs a confirmed address (SEC-10), Readers may export, the slug rule of SPEC §8.3 v2.37

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-19/S5-20a built the list (`Create`, `Decide`), save, read, list and `PersistedConfigurationSource` without an endpoint | The changes and the export build on them; no EF migration |
| 2 | R-SAV-03 asks for ETag/If-Match with S37, API-03 is planned for step 5, the sprint row did not name it, and S37 needs the other person's name (a new column) | Asked before planning; decision 2a |
| 3 | ARCHITECTURE §7.2 had no endpoint for „Auswahl ändern“ | Decision 3a; new row in §7.2 |
| 4 | `Criterion.SupersededByKey` existed, but `Catalog.Contracts` exposed neither it nor the active release; it is set on the old criterion when a later release is imported, so the release cache cannot hold it | New `ICatalogReleaseCarryOver`, which reads successors from the database |
| 5 | The recommendation of a row depends on the selection; CAT-08 said only that a decision never changes it | A selection change sets it anew (`Reset`), CAT-08 reworded (V261) |
| 6 | A carried weight can lose its B when the new recommendation differs; the domain allows a weight only while B applies | Asked in the plan; decision a |
| 7 | The test releases merge 99.10a and 99.10b into 99.10 | Of merged criteria, the one with its own key wins, then one with a decision; the others with a decision are reported |
| 8 | SPEC §7.3 sets „Saved export: 60 per 10 minutes per user“ | New per-user policy `SavedExport` |

## 2. Changes

### 2.1 Server

- **`Catalog.Contracts/ICatalogReleaseCarryOver.cs`**: `CarryOverAsync(fromReleaseVersion, selection)` → `CatalogCarryOverResult` (outcome, active release, the selection under both releases, dropped values by name, successor revision per old revision, errors). **`Catalog/Criteria/CatalogReleaseCarryOver`** (registered scoped): `Trim` drops an inactive use case, then, going down the layers, every value a kept higher-layer value excludes; successors by key or followed `SupersededByKey`.
- **`Configurator/Domain`**: `CriteriaConfiguration.ChangeSelection` (rows that stay are kept as the same objects); `ConfigurationCriterion.Reset` (the only path that sets `ResolvedLevel` anew) and `HasDecision` (ignored by EF).
- **`Configurator/Configurations`**:
  - `ConfigurationEndpoints`: the four changes, each with `ConfirmedEmail` and `RefusedForListAsync` (organisation from the stored list, Editor).
  - `ConfigurationService`: `OrganizationOfAsync`, `DecideAsync`, `ChangeSelectionAsync`, `DeleteAsync`, `CopyToActiveReleaseAsync`, and `EditResult<T>`/`EditOutcome`.
  - `DecisionCarryOver`.
  - Body and answer records in `ConfigurationContracts`.
  - `ConfigurationDraftCheck.LevelOf` and `SelectionKey` are now `internal` for reuse.
- **`Export`**: `Http/SavedExportEndpoint` (any member, `no-store`, `ExportCreated`, policy `SavedExport`, body ≤ 4 KB); `ExportSourceResult.ListName`; `ExportFileWriter.SlugOf` and the slug after „Kriterien“; `SavedExportRequest`.
- **`Platform`**: `RateLimitPolicies.SavedExport` in `UserPolicies`; `appsettings.json` 60 / 10 min 🔧; relaxed in `docker-compose.e2e.yml` and the integration host.
- No EF migration, no new package.

### 2.2 Tests

- **Unit:**
  - `Configurator/DecisionCarryOverTests.cs` (CAT-22);
  - `Configurator/CriteriaConfigurationTests.cs` (`ChangeSelection_*`, `HasDecision_*`);
  - `Catalog/Criteria/CatalogReleaseCarryOverTests.cs` (CAT-22);
  - `Export/ExportFileNameTests.cs`.
- **Integration:**
  - `Configurator/SavedListChangeTests.cs`: decide, selection, delete, copy on `T.1` → `T.2` (CAT-22), TEN-01 and SEC-10 for each change.
  - `Export/SavedExportEndpointTests.cs`: EXP-01 and EXP-03 persisted, the usage event, format, 404.
  - Five rows in `Tenancy/AuthorizationMatrixTests` (TEN-05).
  - `Api/RateLimitTests.SavedExport_*` (API-05).
  - `ExportEndpointTests.Export_Endpoint_LimitsTheBodyToOneMegabyte` finds the anonymous export by its name now (R6).

### 2.3 Documents

- TESTING_v77 (V255 – V262; CAT-22 active; EXP-01, EXP-03, EXP-06, TEN-05, API-03, API-05, SEC-10 and CAT-08 rows; v75 deleted).
- ARCHITECTURE_v69 (C155; §4.3 module rows, §7.2, §12 step 3; header references; v67 deleted).
- SPEC v2.37 (§8.3 slug, §8.10 the five endpoints, change rows).
- `CODEBASE_v1.md` §2.70 and §3.
- `CLAUDE.md` (state line, last task).
- SPRINT_05 and TASK_PROMPTS (the split).
- No UX copy, Figma mapping or `LEGACY_EXCEPTIONS` change. No screen, so no Figma frame to compare.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | A `PATCH` on a criterion that a selection change removes at the same moment updates 0 rows and answers 500 (`DbUpdateConcurrencyException`) | **follow-up (step 5, API-03)**: no ETag by decision 2a; the window is milliseconds |
| R2 | A carried-over weight is not checked again against the 🔧 weight range | **won't fix**: stored rows keep their weights anyway; the range is checked on input |
| R3 | Of two merged criteria with decisions, only one decision moves | **kept**: documented (C155) and tested; the other is reported |
| R4 | `criteria.list.copy.notCarried` gives the reason „gibt es nicht mehr“ also for a criterion that only a dropped value brought (99.4 with audio) | **follow-up (UX review, S5-21b)** |
| R5 | The saved export checks the format before the list, so a foreign list with a wrong format answers 400, not 404 | **won't fix**: says nothing about the list |
| R6 | `ExportEndpointTests` found the anonymous export by „route ends in `/exports`“, which now matches two endpoints | **fixed**: by endpoint name |
| R7 | The copy looked up `rows[old]` without a guard | **fixed** during the build (`GetValueOrDefault`) |
| R8 | Security: <ul><li>the organisation always comes from the stored list through the filter (404), then Editor (403);</li><li>`ConfirmedEmail` and antiforgery on every change;</li><li>the export needs a sign-in, is rate-limited per user and answers `no-store`;</li><li>no `IgnoreQueryFilters()`, no raw SQL (TEN-02 unchanged);</li><li>unknown body members are refused and bodies are bounded;</li><li>no note or user id in a log or exception text;</li><li>the slug is ASCII.</li></ul> N+1: the successor map is one query bounded by the catalog; release snapshots are cached. a11y: no screen | — |

## 4. Checks

- Backend:
  - Build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean.
  - `dotnet test --solution`: **1 476 passed, 0 failed** (unit 792, architecture, integration); the first full run had 1 failure, R6, fixed before this run.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` passed; `reuse lint` compliant.
- Client and E2E not run: no client file changed. `docker-compose.e2e.yml` only relaxes the new limit.

## 5. Open points

1. **S5-21b**:
   - S11 editable for Editor and Owner (Readers read only);
   - „Auswahl ändern“ to S10 with the saved selection, hidden on a list of an older release (5a);
   - the delete dialog on S14 (7a);
   - the older-release notice with „Neue Liste mit derselben Auswahl“ and the report on the new list;
   - the saved export in S13;
   - UX copy v34 drafts: the dropped decisions of a selection change (4a), a neutral text instead of „Von Ihnen geändert“ (8b), R4;
   - J1's file comparison and with it the M3 exit criterion.
2. **Step 5 (API-03)**: ETag/If-Match on saved lists with S37 (2a); R1 goes away with it.
3. **S5-23**: deleting a list ends its share links.
4. **S5-22**: `no-store` on the saved-list answers, the weight range served with the API (TD-45).

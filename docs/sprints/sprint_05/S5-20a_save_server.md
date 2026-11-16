# S5-20a — Saved-list endpoints and the shared draft check

> **Sprint:** 05 · **Task:** S5-20a (server part of S5-20, 4 points for S5-20 in all) · **Issues:** none
> **Requirements / invariants:** SPEC R-SAV-01, R-SAV-02 (server part), §8.10 (v2.35); TEN-05 saved-list rows, TEN-01 on read and list (TESTING_v75 V244); SEC-10 saving a list (V245); CAT-18 configurator part (V246); ARC-03 narrowed to the draft check (V247); EXP-07 on saving, J1 split (V248); ARCHITECTURE C153. No invariant changes its status
> **Status:** done — pull request open (2026-11-16)
> **Scope:** `POST /api/v1/configurations`, `GET /api/v1/configurations/{id}` and `GET /api/v1/configurations?organizationId=` on `CriteriaConfiguration.Create`, with the draft check of the anonymous export moved into the Configurator module. No screen: S12, S14, the draft across sign-in and J1 are S5-20b; edit, delete and the saved export S5-21.
> **Owner decisions (2026-11-16, before and during the task):** S5-20 **1a** — split: S5-20a server, S5-20b client · **2a** — S14 gets a new entry in the menu „Konto“ (wording by the owner, S5-20b) · **3a** — the missing UX copy of S12 and S14 is drafted as U140 ff. in UX copy v33 (S5-20b); a list in S14 shows name, save date and catalog version, not its creator · **4a** — after saving, the page becomes the saved list (`/kriterienlisten/:id`) and the draft is dropped (S5-20b) · **5a** — a saved list opens read-only on S11 until S5-21 · **6a** — J1 builds its own account and organisation through `e2e/support/accounts.ts` · **7a** — J1 runs through the save in S5-20; the file comparison and the M3 exit criterion move to S5-21 · **A (a′)** — one draft check for the export and saving, in Configurator behind `Configurator.Contracts`, with the ARC-03 rules of the ad-hoc source narrowed to it (the first proposal A (a) contradicted ARCHITECTURE §4.4 and two ARC-03 tests; asked again, see §1 row 3) · **B (a)** — saving and listing name their organisation in the body or the query, checked as a resource

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-19 built the tables and `CriteriaConfiguration.Create`, no endpoint (decision S5-19 3a); its open point R7 left the weight range and the catalog check of values and criteria to the save path | The save path runs the full catalog check |
| 2 | That check existed once, `internal` to Export in `AdHocSelectionSource` (with `PriorityWeightRange`, a `Configurator:` setting); Configurator cannot reference Export | Decision A: move it into Configurator behind a contract |
| 3 | ARCHITECTURE §4.4 (C123) says the ad-hoc source „depends on Catalog.Contracts only“, enforced by `ModuleBoundaryTests.AdHocSelectionSource_Types_UseOfOtherModulesOnlyCatalogContracts` and `Export_SavedLists_AreReadOnlyThroughIConfigurationReader`; the plan had named only §4.3 | Stopped before coding and asked; decision A (a′): the rules keep their point (no saved configuration in the anonymous export) and allow the draft check (C153, V247) |
| 4 | ARCHITECTURE §7.2 has flat routes (`POST /configurations`); the organisation of a new list or of the list query has no resource yet | Decision B (a): body / query, checked with `OrganizationRoleRequirement` on `OrganizationReference` |
| 5 | CAT-18 left „rejection for new selections with the configurator (step 3)“; the catalog contract did not say whether a resolved use case is active | `ResolvedCatalogSelection.UseCaseIsActive`; the save refuses, the export still accepts (owner, S3-20) |
| 6 | The S14 list endpoint was in no document; `no-store` on workspace answers is S5-22's (TEN-04 workspace part) | List endpoint in SPEC §8.10 and ARCHITECTURE §7.2; `no-store` left to S5-22 |
| 7 | TESTING §5.4 J1 ends with „export again → both files identical“, but the saved export is S5-21 | Decision 7a; SPRINT_05 rows S5-20/S5-21, TASK_PROMPTS, TESTING V248 |

## 2. Changes

### 2.1 Server

- **`Catalog.Contracts`**: `ResolvedCatalogSelection` gets `UseCaseIsActive` (default `true`); `CatalogSelectionResolver` fills it from the use-case template.
- **`Configurator.Contracts/Drafts/IConfigurationDraftCheck.cs`**: `CheckAsync(CriteriaDraft, DraftPurpose)`; records `CriteriaDraft`, `DraftSelection`, `DraftCriterion` (both with unknown JSON members refused), `DraftCheckResult` (`DraftCheckOutcome`: `Checked`, `Invalid`, `SelectionExcluded`, `ReleaseNotFound`, `ReleaseNotActive`; detail; field errors), `CheckedDraft`, `CheckedCriterion`. No problem type names: a contracts project references no `Platform.Contracts` (ADR-017).
- **`Configurator/Drafts/`**: `ConfigurationDraftCheck` — the rules and messages of the ad-hoc source moved unchanged, plus `DraftPurpose.Save` refusing an inactive use case; `PriorityWeightRange` moved from Export (namespace only), registered by `AddConfiguratorModule`.
- **`Configurator/Configurations/`**: `ConfigurationEndpoints` (group `/configurations`, `RequireAuthorization()`): `POST` with `ConfirmedEmail` and a 1 MB `RequestSizeLimit` — role check Editor first, then name and draft check, one answer with both kinds of field errors, 201 with `Location`; `GET /{id}` through the filter (404 for another organisation's list); `GET ?organizationId=` with Reader. `ConfigurationService` (check, create with the catalog's recommendation per row, read with decisions by revision id, list without rows), `ConfigurationContracts` (body and answers). `MapConfiguratorEndpoints` maps the group.
- **`Export`**: `AdHocSelectionSource` takes `IConfigurationDraftCheck` and maps its outcome to the export's Problem Details and the checked draft to `ExportContent`; `ExportModule` no longer registers the weight range.
- No EF migration, no new package, no rate limit (SPEC §7.3 lists none).

### 2.2 Client

None (S5-20b).

### 2.3 Tests

- Unit: `Configurator/ConfigurationDraftCheckTests.cs` (2 × `INV:CAT-18`, `INV:EXP-07` for the checked draft, the weight range test moved here); `Export/AdHocSelectionSourceTests.cs` and `PersistedConfigurationSourceTests.cs` run on the moved check, otherwise unchanged.
- Integration: `Configurator/ConfigurationEndpointsTests.cs` (21 cases: save by Editor and Owner, names, organisation, a recommended level in the body, weight range, missing criterion, release-mismatch, selection-excluded, `INV:CAT-18`, 2 × `INV:SEC-10`, 2 × `INV:TEN-01`, read and list); `Tenancy/AuthorizationMatrixTests.cs` 18 new rows (`INV:TEN-05`); `Infrastructure/LegacyAccounts.cs`.
- Architecture: `ModuleBoundaryTests.AdHocSelectionSource_Types_UseOfOtherModulesOnlyCatalogContractsAndTheDraftCheck` (renamed) and `Export_SavedLists_AreReadOnlyThroughIConfigurationReader` (narrowed); `ModuleRules` with name-based overloads, which the assembly forms call.

### 2.4 Documents

ARCHITECTURE_v67 (C153; §4.3 module rows, §4.4 Flow A, §7.2, §12 step 3; v65 deleted), TESTING_v75 (V244 – V248; CAT-18, ARC-03 and J1 rows; v73 deleted), SPEC v2.35 (§8.10, change rows), SPRINT_05 (rows S5-20, S5-21), TASK_PROMPTS (S5-20 split, S5-21 read-only and M3 exit), `CODEBASE_v1.md` §2.68 and §3, `CLAUDE.md` (state, last task). No UX copy, Figma mapping or `LEGACY_EXCEPTIONS` change; no screen, so no Figma frame to compare.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `ConfigurationDraftCheck.NoteMaxLength` and `ConfigurationCriterion.NoteMaxLength` were two equal constants in one module | **fixed** — the check takes the domain's |
| R2 | With an invalid name and a draft refused as `selection-excluded`, `release-mismatch` or unknown release, the answer names only the draft's problem | **won't fix** — two problem types never mix (the export does the same with its format); the name error comes on the next try |
| R3 | `GET /configurations?organizationId=` has no paging | **won't fix** — SPEC §8.10 sets none; an organisation's lists are few; recorded for S14 |
| R4 | The decisions of a list come by revision id, not in list order | **kept** — S11 takes texts and order from the criteria endpoint of the pinned release; documented in SPEC §8.10 |
| R5 | The new answers carry no `Cache-Control: no-store` | **follow-up S5-22** — its row (TEN-04 workspace part); without validators or freshness the answers are not stored by a browser heuristically |
| R6 | Three test files keep their own copy of the legacy-account helper that `LegacyAccounts` now offers | **follow-up issue** (proposed) — move `OrganizationEndpointsTests`, `InvitationEndpointsTests` to it; not this PR's scope |
| R7 | The anonymous matrix row imports the test catalog it does not need | **won't fix** — one import per row, the body stays the same for every row |
| R8 | Security: every endpoint needs a sign-in; the organisation is checked as a resource before anything else (404 non-member, 403 lesser role) and never read from a header; another organisation's list is 404 through the filter; no `IgnoreQueryFilters()`, no raw SQL (TEN-02 unchanged); saving needs a confirmed address and antiforgery (`AntiforgeryValidationTests` covers the new POST); unknown body members refused; name, note, weight, entry count and body size bounded; no note or user id in a log or exception text. N+1: a read loads one list with four split queries, the list one query, the catalog is cached per release. a11y: no screen | — |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 383 passed, 0 failed** (unit, architecture, integration) before R1, unit tests (765) again after it.
- `check-invariants` passed, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` passed (0 errors); `reuse lint` compliant.
- Client and E2E not run: no client file changed, no migration; CI is the authority.

## 5. Open points

1. **S5-20b** builds S12, S14 with its menu entry (2a), the draft across sign-in, the page of a saved list read-only (4a, 5a), the UX copy v33 drafts (3a: list fields, sort, S12 errors incl. `release-mismatch` on saving and the inactive use case) and J1 with its own account and organisation (6a); the switch test of R-SHL-03 for the lists (V232).
2. **S5-21**: `PATCH`, delete and the saved export; J1 file comparison and the M3 exit criterion (7a); a saved list editable on S11.
3. **S5-22**: `no-store` on these answers (R5); the weight range served with the saved-list API (TD-45).
4. **Proposed issue:** one legacy-account helper for the integration tests (R6).

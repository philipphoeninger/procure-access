# S5-19 — Configurator persistence and PersistedConfigurationSource

> **Sprint:** 05 · **Task:** S5-19 (M3.5 part, 4.5 points) · **Issues:** none
> **Requirements / invariants:** **CAT-08, EXP-06 `active`** (TESTING_v74 V239, V240); ARC-03 restated with its `IConfigurationReader` part written (V241); CAT-06 stored part, TEN-01 sample, OPS-03 for the new context (V242); PRV-07 criteria lists (V243); ARCHITECTURE C152, ADR-017; SPEC §8.4 (v2.34); groundwork for R-SAV-01 – R-SAV-03, R-LST-12
> **Status:** done — pull request #353 open (2026-11-16)
> **Scope:** the saved criteria lists as tables of the Configurator module, `IConfigurationReader`, `PersistedConfigurationSource` in Export, the criteria lists of the personal data export. No endpoint and no screen: save, read and list are S5-20, edit, delete and the saved export S5-21.
> **Owner decisions (2026-11-16, before the task):** S5-19 **1a** (= O-S5-5, O-22) — a `Contracts` project may reference the contracts of the modules its row of ARCHITECTURE §4.3 lists, without cycles, with an ADR and ARC-03 adjusted · **2a** — the lists store the release version, the value keys and `CriterionRevisionId`, not database ids of the catalog; §5.3 follows · **3a** — no endpoint in S5-19; S5-20 builds save, read and list · **4** — `createdByMe.criteriaLists`: name, organisation name, release, creation time, selection by name, per criterion number, title, included, recommended level, effective level, weight, note; read through the organisation filter (only organisations the user still belongs to) · **note** — `CreatedAt` and `ModifiedAt` on the list now

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The Configurator module had one endpoint (`criteria-list-shown`), no context, no services; Export had only `AdHocSelectionSource`, whose private `RowOf` built the rows | A first `ConfiguratorDbContext` with its migration; the row builder moved to `ExportRow.Of`, used by both sources |
| 2 | `IConfigurationReader` hands out levels, and `PriorityLevel` lives in `Catalog.Contracts`, while a contracts project could reference nothing (O-22, C95) | Decision 1a: ADR-017, ARC-03 restated |
| 3 | ARCHITECTURE §5.3 named `CatalogReleaseId`, `UseCaseTemplateId`, `AppTypeId` …; `Catalog.Contracts` exposes only the version, the value keys and `CriterionRevisionId`, and no foreign key may cross two contexts | Decision 2a: version and keys |
| 4 | The row names no endpoint; `POST`/`GET /configurations` and the list for S14 were in no row | Decision 3a; the S5-20 and S5-21 prompts got a line each |
| 5 | SPEC §8.4 showed `criteriaLists` only as `[ … ]` | Decision 4; SPEC v2.34 |
| 6 | S14 sorts by date and S5-21 needs an ETag | `CreatedAt` and `ModifiedAt` now (note); `xmin` needs no migration and comes with S5-21 |

## 2. Changes

### 2.1 Server

- **`Configurator.Contracts`** (references `Catalog.Contracts`): `Configurations/IConfigurationReader.cs` — `FindAsync(id)` → `SavedConfiguration` (id, name, release version, keys of the four layers, `SavedCriterion`s with `ResolvedLevel`, `OverrideLevel`, `OverrideWeight`, note), `null` for a list that does not exist for the request; `Accounts/ICriteriaListDataExport.cs` — `GetCreatedByAsync(userId)` with records without ids.
- **`Configurator/Domain`**: `CriteriaConfiguration` (aggregate; `Create` from a resolved selection and one `ConfigurationCriterionDraft` per criterion; `Decide` for one criterion, stamps `ModifiedAt`), `ConfigurationCriterion` (`ResolvedLevel` set once — CAT-08; a level equal to the recommendation is no choice; a weight needs B and ≥ 1; note ≤ 1 000), `ConfigurationAppType`, `ConfigurationProductPart`, `ConfigurationTestType` (one key each).
- **`Configurator/Persistence`**: `ConfiguratorDbContext` (schema `configurator`, own history table, the organisation filter on `CriteriaConfiguration`), `CriteriaConfigurationConfiguration` (the four row tables as owned collections: key list + value, cascade delete, no set of their own; indexes on `OrganizationId` and `CreatedByUserId`), `ConfiguratorDatabaseMigrator`, the design-time factory, **migration `InitialConfiguratorSchema`** (`internal`). No foreign key leaves the schema.
- **`Configurator`**: `Configurations/ConfigurationReader`, `Accounts/CriteriaListDataExportService` (texts from `ICatalogSelectionResolver` under the pinned release, organisation names from `IOrganizationMembers.ListOfAsync`); `AddConfiguratorModule(configuration)` registers the context with split queries, the migrator and both services; `Program.cs` passes the configuration. The project references EF Core and Npgsql in the versions the other modules use (no new package for the product; `THIRD-PARTY-NOTICES.txt` unchanged).
- **`Export`**: `Sources/PersistedConfigurationSource` (`IExportSource<Guid>`): the list through the reader, the pinned release through the catalog also when it is no longer active, 404 `not-found` for an unknown list, an exception when the list does not hold exactly the criteria of its selection. `ExportRow.Of` builds a row for both sources. Registered; its endpoint is S5-21.
- **Host**: `AccountExportEndpoints` fills `createdByMe.criteriaLists` (`AccountExportCriteriaList`, `…ListSelection`, `…ListCriterion`; levels `A`, `B`, `Info`).

### 2.2 Client

None.

### 2.3 Tests

- Unit: `Configurator/CriteriaConfigurationTests.cs` (2 × `INV:CAT-08`, the rules of a list and a decision); `Export/PersistedConfigurationSourceTests.cs` (`INV:EXP-06` on doubles, pinned release, 404, mismatching list).
- Integration: `Configurator/CriteriaConfigurationPersistenceTests.cs` (`INV:CAT-08` across save and reload; the reader behind the filter; rows deleted with the list), `Export/ExportSourceParityTests.cs` (`INV:EXP-06`: content, CSV records and XLSX cells of both sources on `T.1`; `INV:CAT-06`: the list keeps its release after `T.2` is activated; a foreign list is 404), `Persistence/ConfiguratorMigrationTests.cs` (2 × `INV:OPS-03`, start applies the migrations), `Tenancy/OrganizationIsolationTests.cs` (sample and context, `INV:TEN-01`), `IdentityAccess/AccountExportTests.cs` (new `INV:PRV-07` case). `Configurator/SavedLists.cs` builds a list from the host's resolved selection; `ApiFixture` migrates and stores through the new context.
- Architecture: `ModuleReferenceTests` — contracts reference only listed contracts (projects and types), the table has no cycle; `ModuleBoundaryTests.Export_SavedLists_AreReadOnlyThroughIConfigurationReader` (all `INV:ARC-03`).

### 2.4 Documents

TESTING_v74 (V239 – V243; v72 deleted), ARCHITECTURE_v66 (C152, ADR-017, O-22 closed, §4.3, §5.3, §12 step 3; v64 deleted), SPEC v2.34 (§8.4 shape of a criteria list; change rows), `CODEBASE_v1.md` §2.67, §3, §4, `CLAUDE.md` (state, context list of §5, the contracts rule of §7, last task), `TASK_PROMPTS.md` (S5-20 builds the endpoints, S5-21 the saved export endpoint). No UX copy, Figma mapping or `LEGACY_EXCEPTIONS` change.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | The data export took the organisation name with an indexer; a membership removed while the request runs would have answered 500 | **fix** — such a list is left out (it is no longer the user's to read) |
| R2 | A list loads four row tables; in one query they multiply each other's rows, and the explicit `AsSplitQuery()` is easy to forget on the next query | **fix** — the module registers the context with split queries |
| R3 | The row tables are owned collections, while ARCHITECTURE §5.3 lists them as entities | **kept** — same tables; they cannot be queried without their list, so the filter on the list covers them; recorded in §5.3 (C152 (4)) |
| R4 | `PersistedConfigurationSource` takes recommendation, weight and rationale from the catalog, not the stored `ResolvedLevel` | **kept** — releases are immutable, so both are equal, and one row builder keeps EXP-06 true by construction; the stored level is the audit trail and what the data export shows |
| R5 | A saved selection that no longer resolves (or a list that does not match it) throws → 500 | **kept** — cannot happen while stored releases never change (CAT-05); a partial export would be worse |
| R6 | The data export resolves the catalog once per list | **kept** — release content is cached per release; the number is the user's own lists |
| R7 | The weight range (🔧) and whether keys and revisions belong to the release are not checked in the domain | **follow-up in S5-20** — the save path checks them against the catalog, as the ad-hoc source does (TASK_PROMPTS) |
| R8 | Security: no new endpoint; every read through the organisation filter, no `IgnoreQueryFilters()`, no raw SQL (TEN-02 unchanged); the export carries no ids; the exception texts name a list id, never a user. a11y: no screen | — |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1340 passed (unit, architecture, integration) before the two review fixes, the touched classes again after them; the Configurator model has no pending changes (`ConfiguratorMigrationTests`).
- `check-invariants` passed (CAT-08 and EXP-06 `active` with their tagged tests), `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` passed (0 errors); `reuse lint` compliant.
- Client and E2E not run: no client file changed; the E2E stack applies the new migration at startup — CI is the authority.

## 5. Open points

1. **S5-20** builds `POST /api/v1/configurations`, the reads and the list on `CriteriaConfiguration.Create` (decision 3a), with the checks of R7, the ConfirmedEmail gate and the rows in `AuthorizationMatrixTests`; probably S5-20a/b.
2. **S5-21**: the endpoint of the saved export, the slug of the list name in the file name (`ExportFileWriter`), the J1 file comparison (deferred part of EXP-06), `xmin` as ETag.
3. **Deleting an organisation or an account** does not exist yet; when it comes, it has to decide what happens to the lists (`OrganizationId` and `CreatedByUserId` have no foreign key, ADR-014).
4. **Lists of a left organisation** are not in the personal data export (decision 4); if the legal review (O-4) wants them, the read needs an allow-listed filter bypass.
5. **`ProcurementProjectId`** (C53) comes with step 5; **share links** (S5-23) add `IShareableResourceProvider` on these tables — the next Configurator migration, one open branch at a time.

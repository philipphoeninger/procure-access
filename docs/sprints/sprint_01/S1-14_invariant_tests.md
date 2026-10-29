# S1-14 — ARC-06 and OPS-03 as tagged tests

> **Sprint:** 01, task S1-14 (milestone M0.3, TESTING §4.10, §5.2, §8.1, §9 step 0). **Status:** done by Claude Code (2026-10-23).
> **Scope:** tagged tests for **ARC-06** (AutoMapper configuration valid) and **OPS-03** (migrations apply to an empty PostgreSQL 17 database, no pending model changes). Not in scope: setting both to `active` in TESTING §4 and the gate script (S1-21), CAT-10 (S1-15), formatting the production projects (S1-13 P3).
> **Owner decisions 2026-10-23:** **A** ARC-06 lives in the **Unit** project, as TESTING §4.10 lists it with level U; this corrects S1-12 (table row of the Architecture project and P3). **B** Fix `MappingProfile` with explicit `Ignore()` instead of leaving ARC-06 red.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `AssertConfigurationIsValid` fails on `main`: `CreateProductDto → Product` leaves `Id`, `CreatedAt`, `IsDeleted`, `xmin`, `Proposal` unmapped; `CreateCriterionDto → Criterion` the same plus `CriteriaFilter` | Decision B. These members are set by the server or EF; no DTO member matches them, so AutoMapper never wrote them. `Ignore()` declares this and changes no behaviour |
| F2 | AutoMapper 14 has `new MapperConfiguration(Action<…>)` only; the logger-factory argument came with 15 | The test uses the one-argument constructor |
| F3 | The fixture database is migrated once and then emptied by Respawn; it is never "empty, then migrated" inside a test | OPS-03 creates its own database with a unique name on the shared container and drops it afterwards |
| F4 | TESTING §4.10 (level U) and S1-12 (first Architecture test) disagree on where ARC-06 goes | Decision A; the Architecture project stays empty until step 1 and keeps `--ignore-exit-code 8` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `DAL/MODELS.ProcureAccess/Entities/Mapping/MappingProfile.cs` | Both create maps: `ForMember(…, Ignore())` for key, audit fields, concurrency token and navigations; comment. The re-indented block uses spaces | F1, decision B |
| `tests/ProcureAccess.Tests.Unit/Mapping/MappingProfileTests.cs` (new) | `INV:ARC-06 MappingConfiguration_AllProfilesOfTheApi_IsValid`: same profile scan as `AddAutoMapper` in `Program.cs` (`AddMaps(typeof(MappingProfile).Assembly)`), then `AssertConfigurationIsValid()` | ARC-06 |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/ApiFixture.cs` | `ConnectionStringForNewDatabase()`: connection string for a not-yet-existing database `test_<guid>` on the shared container | F3 |
| `tests/ProcureAccess.Tests.Integration/Persistence/MigrationTests.cs` (new) | `INV:OPS-03 Migrate_EmptyDatabase_AppliesAllMigrations` (database does not exist → `MigrateAsync` → applied = all migrations of the assembly, none pending); `INV:OPS-03 Model_ComparedWithLastMigration_HasNoPendingChanges` (`HasPendingModelChanges()`, same check as `dotnet ef migrations has-pending-model-changes`). Not derived from `IntegrationTest`, so no Respawn reset | OPS-03 |
| `tests/ProcureAccess.Tests.Architecture/*.csproj` | Comment on `--ignore-exit-code 8` now points to step 1 (ARC-01–03) | F4 |
| `CLAUDE.md` §3 | Current state: ARC-06 and OPS-03 tests exist | CLAUDE §11 |

Tagging follows TESTING §8.1: `[Trait("Invariant", "<ID>")]` and `INV:<ID>` in the display name (`DisplayName = "INV:<ID> " + nameof(<method>)`, so the name cannot drift from the method). No new packages.

## 3. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; 136 warnings, unchanged from S1-13, none from `tests/` |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ exit 0; 16/16: Unit 3, Integration 13, Architecture 0 (still allowed to be empty) |
| 3 | xUnit `-list full -trait "Invariant=…"` | ✅ ARC-06: 1 test, OPS-03: 2 tests, each with trait and `INV:` display name |
| 4 | Negative: one `Ignore()` removed (`Criterion.CriteriaFilter`) | ✅ ARC-06 fails with "Unmapped properties: CriteriaFilter"; restored |
| 5 | Negative: property added to `FilterType` without a migration | ✅ run fails with EF's `PendingModelChangesWarning` — but already in `ApiFixture.InitializeAsync` (its `MigrateAsync` throws on EF Core 10), so every integration test fails, not only OPS-03. See P1; restored |
| 6 | `dotnet format --verify-no-changes`, the four test projects | ✅ |
| 7 | `dotnet format --verify-no-changes`, `MappingProfile.cs` | ⚠️ findings left only on the untouched tab-indented lines (11–16, 45–48); fewer than on `main`. See S1-13 P3 |
| 8 | `reuse lint` | ✅ compliant (488/488 files) |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | On model drift the shared fixture fails first (EF Core 10 `Migrate` throws), so the report shows "fixture threw" on all integration tests instead of a single red OPS-03 test. The cause is still named clearly (`PendingModelChangesWarning … Add a new migration`). Accepted for now; if the noise matters, the fixture could suppress that warning and leave the verdict to OPS-03 | Revisit in S1-20 (CI) if needed |
| P2 | S1-12 said ARC-06 would be the Architecture project's first test (table and P3); corrected by decision A. The `--ignore-exit-code 8` there goes with ARC-01–03 in step 1 | Step 1 |
| P3 | ARC-06 tests AutoMapper 14.0.0, which is to be replaced in M1 (issue #31). TESTING must then say what replaces ARC-06 (S1-03 §4) | M1 |
| P4 | ARC-06 and OPS-03 stay `planned` in TESTING §4 until the gate exists; S1-21 sets the step-0 invariants to `active` | S1-21 |

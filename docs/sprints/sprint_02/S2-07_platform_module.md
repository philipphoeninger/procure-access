# S2-07 — Move the platform code built ahead into the Platform module

> **Sprint:** 02, P0 task S2-07 (roadmap M1.1, 2 points; ARCHITECTURE_v15 §4.3, §10.5; closes S1-23 open point P6 and S2-06 open points P1, P3, P5). **Issues:** none. **Requirements / invariants:** OPS-01, OPS-02, PRV-05, SEC-06 stay green (no behaviour change); OPS-03 gets tests for the new context. No invariant changes status, so no new TESTING version. **Status:** done by Claude Code (2026-10-30), pull request open.
> **Scope:** the Serilog set-up and `LogRetentionJob`, the health checks, `IUsageEventWriter` with its entity, and the `TimeProvider` registration move into `src/ProcureAccess.Modules.Platform`; only `PlatformModule` (registration) stays public outside `Contracts`. Not in scope: `/api/Health` (removed with #122, S2-19), the legacy logging wrapper `IAppLogging<T>` (goes with the legacy controllers), Problem Details (S2-10), architecture tests (S2-08).
> **Owner decisions 2026-10-30:** **O-S2-1 for S2-07:** option A, a `DbContext` of the Platform module with its own migration history, and the ADR for S2-06 P5 written in this task (ADR-014). (Rejected: option B, keeping both entities in `DAL` until step 2, which leaves the writer, the retention job and the ready check in legacy code.) **D1** (asked during the task): S2-06 open point P1 (module reference rules in ARCHITECTURE §4.3) goes into the same `ARCHITECTURE_v15.md`. (Rejected: only ADR-014 and S2-07 in v15.) **D2** (asked during the task): free disk space with `docker image prune -f` (dangling images only), as in S1-23.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Serilog set-up, settings and retention in `SERVICES/Logging`; writer in `SERVICES/Analytics`; `SeriLogEntry`, `UsageEvent`, `UsageEventType` and their configurations in `MODELS`; health checks in `API/Extensions`; `TimeProvider` registered in `Program.cs`. All of it uses the legacy `ApplicationDBContext` | Platform must not reference `DAL` (O-S2-1): own context (option A) |
| F2 | Both tables exist in every deployed database, created by the legacy migrations (`InitialCreate`, `ReworkLogTableForPostgreSqlSink`, `AddUsageEventTable`, `UsageEventRandomKey`) | The Platform baseline must adopt them, not create them (ADR-014 rule 4) |
| F3 | Removing the entities from the legacy model makes EF scaffold `DropTable` for both tables | Legacy migration emptied by hand |
| F4 | `ConfigureSerilog` worked on `WebApplicationBuilder`; the module registration is an `IServiceCollection` extension | Serilog registered through `services.AddLogging(…)` (same `LoggingBuilder` underneath); `AddPlatformModule` takes the configuration |
| F5 | The migrations ran from `Program.cs` on the legacy context only; the Platform context is `internal` | Contract `IModuleDatabaseMigrator` in `Platform.Contracts`, run by the host after the legacy context |
| F6 | The tests use the moved types directly (`LogRetentionJob`, `UsageEventWriter`, `SeriLogEntry`, settings) | `InternalsVisibleTo` for the three test projects (and `DynamicProxyGenAssembly2` for NSubstitute's `ILogger<UsageEventWriter>`) |
| F7 | The S2-06 design note lists `HealthController` (`/api/Health`) under Platform for S2-07; S2-19 removes it | Kept in `API` so it is not moved twice (CLAUDE §6.2) |
| F8 | `IAppLogging<T>` / `AppLogging<T>` (legacy controllers' wrapper on Serilog's `LogContext`) is not platform code built ahead | Stays in `SERVICES` with `Serilog.AspNetCore` |
| F9 | The root file system was full (0 bytes free): Testcontainers could not initialise PostgreSQL | D2: `docker image prune -f` freed 4.5 GB |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Platform.Contracts/Analytics/IUsageEventWriter.cs` (new) | `IUsageEventWriter` and `UsageExportFormat`, unchanged from `SERVICES` | Other modules (S11, export in step 2) call the writer |
| `src/ProcureAccess.Modules.Platform.Contracts/Persistence/IModuleDatabaseMigrator.cs` (new) | `ModuleName`, `MigrateAsync`; one per module with tables | F5, ADR-014 rule 3 |
| `src/ProcureAccess.Modules.Platform/PlatformModule.cs` | `AddPlatformModule(services, configuration)`: `TimeProvider.System`, Serilog, `PlatformDbContext` (same Npgsql options as the legacy context: retry on failure, 60 s command timeout), the migrator, log retention, the writer (singleton), health checks. `MapPlatformEndpoints()` maps `/health/live` and `/health/ready` | Sprint row |
| `src/ProcureAccess.Modules.Platform/Logging/` (new) | `LoggingRegistration` (was `LoggingConfiguration.ConfigureSerilog` / `AddLogRetention`, same validation and sink set-up), `AppLoggingSettings`, `LogRetentionJob`, `LogRetentionService`, `SeriLogEntry`; all `internal` | Sprint row |
| `src/ProcureAccess.Modules.Platform/Analytics/` (new) | `UsageEvent`, `UsageEventType`, `UsageEventWriter`; `internal`; the writer resolves `PlatformDbContext` in its own scope | Sprint row |
| `src/ProcureAccess.Modules.Platform/Health/HealthChecks.cs` (new) | Was `API/Extensions/HealthCheckExtensions.cs`; same paths, tag, 5 s timeout, anonymous, body only `Healthy`/`Unhealthy`; the check uses `PlatformDbContext` | Sprint row |
| `src/ProcureAccess.Modules.Platform/Persistence/` (new) | `PlatformDbContext` (history table `platform."__EFMigrationsHistory"` set in `OnConfiguring`, so every instance uses it), both configurations (unchanged), design-time factory, `PlatformDatabaseMigrator`, migration `InitialPlatformSchema` (schemas via `EnsureSchema`, tables and indexes `IF NOT EXISTS` with the statements EF generates, empty `Down`, class made `internal`) and the model snapshot | ADR-014 rules 1, 2, 4 |
| `src/ProcureAccess.Modules.Platform/ProcureAccess.Modules.Platform.csproj` | Packages moved with the code, same versions: EF Core 10.0.14, Npgsql EF 10.0.3, `Serilog.AspNetCore`, `Serilog.Enrichers.Environment`, `Serilog.Sinks.Console`, `Serilog.Sinks.Postgresql.Alternative`; `InternalsVisibleTo` (F6). No new dependency, licences unchanged (MIT, Apache-2.0, PostgreSQL) | – |
| `DAL/…/Migrations/20261030134406_HandOverPlatformTables` (new) | Empty `Up` and `Down`; the snapshot no longer contains the two entities | F3, ADR-014 rule 4 |
| `DAL/…/ApplicationDBContext.cs`; `DAL/MODELS…/Entities/` | The two `DbSet`s and configurations removed; entity and configuration files deleted | – |
| `SERVICES/…` | `Analytics/`, `Logging/Retention/`, `Logging/Settings/` deleted; `LoggingConfiguration` keeps only `RegisterLoggingInterfaces`; three Serilog packages and two global usings removed | F8 |
| `API/…/Program.cs`, `GlobalUsings.cs`, `API.ProcureAccess.xml` | `AddPlatformModule(builder.Configuration)` replaces the five registrations; after the legacy `Migrate()` every `IModuleDatabaseMigrator` runs; `MapPlatformEndpoints()`; `HealthCheckExtensions.cs` deleted; XML documentation regenerated | F5 |
| `tests/…Unit/…` | Logging tests call `AddPlatformLogging` on a `ServiceCollection` (same 8 cases); writer tests use the moved types; project references Platform | – |
| `tests/…Architecture/…` | PRV-05 reads the design-time model of `PlatformDbContext` (same 3 tests); project references Platform | PRV-05 |
| `tests/…Integration/Infrastructure/ApiFixture.cs` | Migrates both contexts before Respawn; Respawn keeps both history tables; `StoreAsync` stores each entity through the context that maps it | ADR-014 rule 3 |
| `tests/…Integration/Operations/LogRetentionTests.cs`, `LogSinkTests.cs`, `MigrationStartupTests.cs`, `Analytics/UsageEventWriterTests.cs` | Read and write through `PlatformDbContext`; the caller with unsaved changes is now a request-scoped `PlatformDbContext`, the same type the writer uses | OPS-02 |
| `tests/…Integration/Persistence/PlatformMigrationTests.cs` (new) | **INV:OPS-03** ×3: legacy then Platform on an empty database, all applied, separate histories; Platform alone on an empty database creates the same columns, types, defaults, identity and indexes as the legacy migrations; no pending model changes. Plus: the API's startup applies the Platform migrations | OPS-03, ADR-014 rules 3, 4, 6 |
| `REUSE.toml` | `src/ProcureAccess.Modules.*/Persistence/Migrations/**` in the block for generated files (MIT; no BIT inklusiv texts) | `reuse lint` |
| `docs/ARCHITECTURE_v15.md` (new) | Copy of v14: status line, "Changes from v14" C92 (ADR-014), C93 (Platform as built), C94 (module references, S2-06 P1); §4.3 table and contracts paragraph, §5.7, §9.5 as-built notes, §10.5 ADR-014 text, §14 ADR-014 row, §16.5 O-22 | D1; owner decision for P5 |
| `CLAUDE.md` | Header; §3 state (S2-07); §4 Platform layout; §5 `--context` on the legacy commands and the Platform migration commands; §7 rule "one `DbContext` per module" | DoD |
| `docs/sprints/sprint_02/S2-06_module_skeleton.md`, `docs/sprints/sprint_01/S1-23_usage_event_table.md` | P1, P3, P5 of S2-06 and P6 of S1-23 marked done | Traceability |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; plus breaking the new tests on purpose (check 3) and a database in today's state upgraded with real rows (check 5). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security and privacy: no endpoint, auth rule, UI or secret changed; health endpoints keep path, anonymity and body; the migration SQL is constant, the test SQL parameterised; PRV-05 unchanged | – |
| R2 | Behaviour beyond the four invariants: a usage-event insert no longer prints the legacy context's "Saving changes" console lines (S1-23 R7), and startup logs one `Information` line per module migrator (below the default `Warning`, so not stored) | Won't fix: console noise only |
| R3 | EF generates migration classes `public`; every future Platform (and module) migration needs the hand edit to `internal` | Follow-up: S2-08's ARC-03 check catches a forgotten one (P1); the step is in CLAUDE §5 and ADR-014 rule 2 |
| R4 | `CREATE TABLE IF NOT EXISTS` does not compare an existing table with the model | Won't fix: the legacy history always runs first and creates exactly this layout; the module-only path is covered by the layout test |
| R5 | `InternalsVisibleTo DynamicProxyGenAssembly2` lets any Castle proxy assembly see Platform's internals | Won't fix: only test doubles create such proxies; the usual NSubstitute set-up |
| R6 | The connection-string check exists twice (`EFCoreExtensions` in the host and `PlatformModule`) | Won't fix: Platform must not depend on legacy code; the legacy one goes with `ApplicationDBContext` |
| R7 | `SERVICES` still references `Serilog.Settings.Configuration`, which nothing uses (already before S2-07) | Follow-up: with #118 or an issue (P3) |
| R8 | `ApiFixture.StoreAsync` saves two contexts one after the other, without a transaction | Won't fix: test helper; a failure fails the test anyway |
| R9 | N+1, unbounded queries: none added; retention is one `DELETE` statement as before | – |
| R10 | a11y: no UI change | – |

No correctness or security finding that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors, 112 warnings (unchanged), none in `src/` or `tests/` |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ 66/66 (62 before + 4 new), incl. OPS-01 2, OPS-02 2, OPS-03 5, PRV-05 3, SEC-06 1 |
| 3 | Broken on purpose, one at a time: a column type in the baseline SQL / the history table setting in `OnConfiguring` / the migrator registration | ✅ layout test fails / ✅ separate-histories test and startup test fail / ✅ startup test fails; all restored, 4/4 green |
| 4 | Public types (reflection over both Platform assemblies, throwaway test) | ✅ `PlatformModule` only; `Contracts`: `IUsageEventWriter`, `UsageExportFormat`, `IModuleDatabaseMigrator` |
| 5 | Upgrade path on a throwaway PostgreSQL 17: legacy migrations up to `UsageEventRandomKey` (main today), one log row and one usage event inserted, then `database update` for the legacy context and for `PlatformDbContext` (design-time factory) | ✅ both rows kept; legacy history ends with `HandOverPlatformTables`; `platform."__EFMigrationsHistory"` holds `InitialPlatformSchema` |
| 6 | `dotnet ef migrations has-pending-model-changes` for both contexts | ✅ no changes |
| 7 | `dotnet format` whitespace / style on the solution, analyzers on each test project | ✅ no changes |
| 8 | E2E stack (`npm run stack:up`), API image from the Dockerfile | ✅ healthy; `/health/live` and `/health/ready` 200; Platform history filled at startup; the sink writes to `logging."SeriLogs"` |
| 9 | `npx playwright test` on Chromium, Firefox, mobile Chrome | ✅ 45/45 (all listed tests on these projects; the "51" in S2-06 check 6 does not match the suite, which has 45 here and 75 on all five projects). ⚠️ WebKit and mobile Safari not run locally (host libraries missing, as before); CI runs them |
| 10 | `node scripts/check-invariants.mjs` | ✅ passed (OPS-03 now 5 tags) |
| 11 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 12 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 575/575 |
| 13 | Client build, Vitest, ESLint | – not touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | S2-08 (ARC-03): count `PlatformModule.MapPlatformEndpoints` as part of the module registration (the class `PlatformModule` is the registration; ARC-03 checks public *types*); a generated `public` migration must fail the check (R3); plus S2-06 P2 | S2-08 |
| P2 | ADR-014: the owner accepted the principle (option A); rules 1–6 are this task's proposal and are confirmed with the review of this pull request (status in ARCHITECTURE §14) | **Done 2026-10-30:** the owner confirmed rules 1–6 with the review of PR #148; ADR-014 is **accepted** (ARCHITECTURE_v15 §10.5, §14) |
| P3 | `Serilog.Settings.Configuration` in `SERVICES` is unused (R7) | #118 or a new issue |
| P4 | `TECH_DEBT_v1.md` §9.2 lists "move `UsageEvent` and its writer to the Platform module (S1-23 P6)" for M1.1: done here; record it in `TECH_DEBT_v2.md` | Sprint 02 close-out |
| P5 | Deployment: the first start after the merge creates the schema `platform`. The database role already created `logging`, `analytics` and `identity`, so it has the right; a role without `CREATE` on the database would stop the API at startup (logged as before) | Note for the deployment |
| P6 | S2-10 (Problem Details) can now be built in Platform on this registration | S2-10 |
| P7 | WebKit / mobile Safari only in CI (check 9) | CI of this PR |

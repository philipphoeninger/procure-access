# S1-22 — Operations basics: log table with retention, health endpoints, rebuild guard

> **Sprint:** 01, stretch task S1-22 (roadmap M1.2; SPEC R-ADM-04 part; ARCHITECTURE §9.5, §9.7, §12 step 1; TESTING §4.7, §4.10). **Status:** done by Claude Code (2026-10-25).
> **Scope:** Serilog writes to the `logging` schema, with a retention job; `/health/live` and `/health/ready`; `RebuildDatabase` only in `Development`; OPS-01, OPS-02 and SEC-06 set to `active`. Not in scope: the correlation ID and the log privacy rules of §9.5 (second bullet), the file-store check in `/health/ready` (the file store comes in step 7), retiring `/api/Health`, the other step-1 invariants.
> **Owner decisions 2026-10-25:** **D1** use the package `Serilog.Sinks.Postgresql.Alternative` (MIT) instead of a sink of our own (ARCHITECTURE §9.5 names "the PostgreSQL sink"). **D2** a new `TESTING_v12.md` with a "Changes from v11" table, as in S1-21. **D3** `docker-compose.yml` takes `ASPNETCORE_ENVIRONMENT` from `.env`, default `Development` (*reversed 2026-10-28, review of #61, PR #114: default `Production`; `.env.example` sets `Development` for the local and CI stacks*). **D4** retention default 90 days (the example of ARCHITECTURE §9.5), read from configuration.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Serilog wrote to the console and a file only. Outside `Development` the file path was built from `Drive:\FilePath\FileName`, and `AppLoggingSettings` existed only in `appsettings.Development.json`, so the API crashed on startup in `Production` (issue #38, S1-06 K6) | Settings moved to `appsettings.json`; the file sink is gone |
| F2 | The table `logging."SeriLogs"` already existed (initial migration) with an MSSQL-era layout: `Properties` of type `xml`, every column `varchar(200)` and `NOT NULL`. Nothing ever wrote to it. `Serilog.Sinks.MSSqlServer` was referenced but unused | Table reworked by a migration; MSSQL sink removed |
| F3 | The `SeriLogEntry` entity used data annotations (`[Table]`, `[Key]`, `[MaxLength]`) | Now Fluent API only (CLAUDE §3) |
| F4 | `Program.cs` checked only the config flag before deleting all users and reseeding; the `IsDevelopment()` check was commented out. `docker-compose.yml` always set `Development` (S1-06 K8) | Guard on `IHostEnvironment` (SEC-06); environment from `.env` (D3) |
| F5 | Health: `HealthController` at `/api/Health` (used by the E2E healthcheck, `e2e/support/global-setup.ts` and `ClockTests`), plus a `Development`-only `/health` with `DateTime.UtcNow` | New endpoints; the `Development`-only one removed; `/api/Health` kept (P2) |
| F6 | The DbContext uses `EnableRetryOnFailure()`: a database that is unreachable from the start makes startup retry for more than a minute | The OPS-01 test stops a container of its own after startup instead |
| F7 | The sink offers its own `retentionTime`, but it runs on the system clock and its own timer, so OPS-02 could not be tested with `FakeTimeProvider` (TESTING §3.1) | Own `LogRetentionJob` on `TimeProvider` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `SERVICES/…/SERVICES.ProcureAccess.csproj` | `+ Serilog.Sinks.Postgresql.Alternative 4.3.0` (MIT, serilog-contrib; targets `net10.0`, Npgsql 10.0.3, Serilog 4.4); `− Serilog.Sinks.MSSqlServer`, `− Serilog.Sinks.File`; the `System.Security.Cryptography.Pkcs` pin stays (MimeKit still needs it), its comment no longer names the MSSQL sink | D1, F1, F2 |
| `SERVICES/…/Logging/Configuration/LoggingConfiguration.cs` | Console + PostgreSQL sink in batches, `needAutoCreateTable: false` (the migrations own the table), column mapping from the entity's names, queue limited by `QueueLimit` (R1); a missing section, an unknown level (R2) or a missing queue limit fail startup with a clear message instead of a `NullReferenceException` or a silent `Debug` fallback; the logger is disposed with the host, so the last batch is flushed. `AddLogRetention()` binds and validates the retention settings (`ValidateOnStart`) and registers the job and the hosted service | ARCH §9.5; #38 |
| `SERVICES/…/Logging/Settings/AppLoggingSettings.cs` | `General`, `PostgreSQL` (connection-string name, `QueueLimit`), `Retention` (`RetentionDays`, `RunInterval`); file settings removed | F1, R1 |
| `SERVICES/…/Logging/Retention/LogRetentionJob.cs` (new) | Deletes rows with `TimeStamp < now − RetentionDays` (`ExecuteDeleteAsync`, `now` from `TimeProvider`) and logs the count | OPS-02, F7 |
| `SERVICES/…/Logging/Retention/LogRetentionService.cs` (new) | `BackgroundService`: runs the job once at startup, then on a `PeriodicTimer` with `TimeProvider`; a failed run is logged and never stops the host | OPS-02 |
| `API/…/appsettings.json`, `appsettings.Development.json` | `AppLoggingSettings` in the base file (level `Warning` as before, queue limit 10 000, retention 90 days, run interval 1 day); the outdated block in the Development file removed | #38, D4 |
| `DAL/MODELS…/Entities/SeriLogEntry.cs`, `Configuration/SeriLogEntryConfiguration.cs` | Columns of the sink: `Id` bigint, `TimeStamp` timestamptz (indexed for retention), `Level` varchar(32), texts as `text`, `Properties` jsonb, optional columns nullable; `Schema`/`Table` constants for the sink | F2, F3 |
| `DAL/…/Migrations/20261025125738_ReworkLogTableForPostgreSqlSink` (new) | Drops and recreates `logging."SeriLogs"`: PostgreSQL has no cast from `xml` to `jsonb`, and the table was never written to. `Down` restores the old table. The snapshot also changes the format of the JSON-owned `UICustomization` (EF 10 output, no schema change) | F2 |
| `API/…/Extensions/HealthCheckExtensions.cs` (new) | `AddHealthChecks()` with a database check (`CanConnectAsync`, tag `ready`, 5 s timeout); `/health/live` runs no checks, `/health/ready` the `ready` checks; both anonymous; body only `Healthy`/`Unhealthy`. No new package | ARCH §9.5, OPS-01 |
| `API/…/Extensions/DatabaseRebuildExtensions.cs` (new) | Rebuild only if `IsDevelopment()` **and** `RebuildDatabase`; a set flag outside `Development` logs a warning and changes nothing | SEC-06, F4 |
| `API/…/Program.cs` | Wires the three parts; the inline rebuild block and the `Development`-only `/health` removed | – |
| `API/…/API.ProcureAccess.xml` | Regenerated XML documentation of the new public types | – |
| `CLIENT/…/src/dev/dev-reload-health.ts` | Dev watcher polls `/health/live` (the old `/health` is gone) | F5 |
| `docker-compose.yml`, `.env.example` | `ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Production}` (since #114; `Development` until then); the template explains the two values | D3, F4 |
| `tests/…/Infrastructure/ProcureAccessApiFactory.cs`, `ApiFixture.cs` | The factory takes an environment name and extra host settings; the file-log workaround is gone; the image name is public for tests with a container of their own | SEC-06, OPS-01 |
| `tests/…/Operations/HealthCheckTests.cs` (new) | **INV:OPS-01** ×2: both endpoints healthy against the shared database; against a container of its own that the test stops after startup, `/live` 200 `Healthy`, `/ready` 503 `Unhealthy` | OPS-01, F6 |
| `tests/…/Operations/LogRetentionTests.cs` (new) | **INV:OPS-02** ×2: the job keeps rows at the limit, one minute newer and now, and removes one minute older; advancing the fake clock by the run interval makes the hosted service remove a row that aged out. Rows are marked per test, because the sink writes into the same table | OPS-02 |
| `tests/…/Operations/DatabaseRebuildGuardTests.cs` (new) | **INV:SEC-06** theory over `Testing`, `Staging`, `Production` with `RebuildDatabase=true`: the existing user is still the only user. Control in `Development`: the user is replaced by sample data, so the theory cannot pass because the flag never arrived. Each case on its own database | SEC-06 |
| `tests/…/Unit/Logging/LoggingConfigurationTests.cs` (new) | Valid settings build the logger; a missing section, an unknown level (`Verbos`, empty, `42`) and a missing or non-positive queue limit throw (8 cases, no database) | R1, R2 |
| `tests/…/Operations/LogSinkTests.cs` (new) | A warning through `ILogger<T>` arrives in `logging."SeriLogs"` with level, template, source context, time stamp and JSON properties (no invariant) | Sink and table fit together |
| `docs/TESTING_v12.md` (new) | Copy of v11: status line; V61 (OPS-01, OPS-02, SEC-06 `active`, other step-1 invariants stay `planned`); V62 (§5.2: own host for startup tests) | D2 |
| `CLAUDE.md` §3, §5 | Current state (S1-22), health probe command, the guard and the single-instance note | CLAUDE §11 |

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Action |
|---|---|---|
| R1 | The sink's default `queueLimit` is `int.MaxValue` (read by reflection from 4.3.0): while the database is down, buffered events would grow memory without limit | Fixed: `AppLoggingSettings:PostgreSQL:QueueLimit` (10 000), required |
| R2 | An unknown `RestrictedToMinimumLevel` fell back to `Debug` without a word; with the database sink that would fill the log table | Fixed: startup fails with the key in the message |
| R3 | `LogRetentionTests` advances the shared fake clock by one day; with parallel test classes this could race with `ClockTests` | No change: the integration assembly runs with `ParallelMode.None` |
| R4 | Health endpoints are anonymous and outside `/api/`: the body is only `Healthy`/`Unhealthy`, the failure reason goes to the log, not the response | No change |
| R5 | The DbContext uses `EnableRetryOnFailure()`. Whether `CanConnectAsync` in `/health/ready` retries was not measured on its own; the check has a 5 s timeout, so an unreachable database answers 503 after 5 s at most | Unsure; the 5 s are a code constant (P10) |
| R6 | No UI change; the only client change is the URL of the dev-only watcher (a11y not affected) | – |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | New tests: `…Operations` (9), `…Unit.Logging` (8) | ✅ 9/9, ✅ 8/8 |
| 2 | Guard broken on purpose (`IsDevelopment()` removed) / cutoff `<` changed to `<=` | ✅ 3 SEC-06 cases fail / ✅ OPS-02 job test fails; both restored |
| 3 | `dotnet build --no-incremental` / `dotnet test --solution ProcureAccess.sln` | ✅ 0 errors, 112 warnings, none in the changed files / ✅ 39/39 (Unit 11, Architecture, Integration) |
| 4 | `dotnet format` whitespace + style on the solution, analyzers on each test project | ✅ |
| 5 | Compose stack, `Development`: `/health/live`, `/health/ready`, `/api/Health`, `/api/Criteria`, client | ✅ all 200; warnings (incl. the rebuild warning) in `logging."SeriLogs"`; 4 migrations applied |
| 6 | API in `Production` with `RebuildDatabase=true` (compose override) | ✅ starts (issue #38 fixed), logs "the database is left unchanged", user count unchanged, health 200 |
| 7 | E2E stack, Playwright chromium, firefox, mobile-chrome | ✅ 39/39. webkit and mobile-safari cannot start on this machine (host library `libicu74` missing, not a test failure); CI runs them on Ubuntu |
| 8 | `npm run lint` / `npx ng test` / `npm run build:release` | ✅ / ✅ 5/5 / ✅ 1.81 MB |
| 9 | `node scripts/check-invariants.mjs` | ✅ reads `TESTING_v12.md`, 7 active: CAT-10 2, SEC-06 1, ARC-06 1, OPS-01 2, OPS-02 2, OPS-03 2, A11Y-01 2 |
| 10 | `node scripts/check-figma-links.mjs` / `reuse lint` 6.2.0 | ✅ unchanged (0 checked, 12 not built, 1 pending) / ✅ 512/512 |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | **Deployment:** the compose stack still defaults to `Development`, where `appsettings.Development.json` sets `RebuildDatabase: true`. A deployment that uses this compose file without `ASPNETCORE_ENVIRONMENT=Production` in its environment still deletes all users on every restart. Set the variable in Dokploy; then `Production` starts since this task (#38) | **Done 2026-10-28:** Dokploy sets `Production`; since #114 the compose stack also falls back to `Production` |
| P2 | `/api/Health` stays because the E2E healthcheck (through nginx, which proxies only `/api/`), `global-setup.ts` and `ClockTests` use it. Moving them to `/health/ready` needs an nginx location or an API-container healthcheck (the aspnet image has no `curl`/`wget`) | Step 1 shell or later |
| P3 | `/health/ready` checks only the database; the file-store check joins in step 7 | Step 7 |
| P4 | Record in the next ARCHITECTURE version: sink package (D1), table owned by the migrations, own retention job instead of the sink's `retentionTime` (F7). The 90 days are not a 🔧 value in SPEC §7; add them to the confirmed defaults if wanted | Next ARCHITECTURE / SPEC version |
| P5 | `ApplicationName` in every log row is `Unknown`: the enricher reads the key `ApplicationName`, the appsettings set `AppName` (unchanged behaviour) | Step 1 or later |
| P6 | Serilog stamps events with the system clock, not `TimeProvider`; in production both are the same clock, and the tests seed rows with explicit times | – |
| P7 | Logs are now part of the database and its backups. The privacy rules of §9.5 (correlation ID, no request bodies of the anonymous export, PRV-04, EXP-05) come with their own tasks | Steps 2/3 |
| P8 | The EF probe error on an empty database (S1-06 K4) now also lands in the log table once, on the first startup | – |
| P9 | The health endpoints are not rate-limited yet (SPEC §7.3 comes later) | Rate-limit task |
| P10 | Health-check timeout (5 s) is a code constant, not configuration; whether the ready check retries through `EnableRetryOnFailure()` before the timeout was not measured | Rate-limit / ops task |

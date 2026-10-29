# S2-02 — Logs: `ApplicationName` is always `Unknown`

> **Sprint:** 02, P0 task S2-02 (debt; TECH_DEBT_v1 TD-09, S1-22 open point P5). **Issues:** #121. **Requirements / invariants:** none activated; OPS-02 stays green. **Status:** done by Claude Code (2026-10-29), pull request open.
> **Scope:** fix the mismatch between the enricher (`ApplicationName`) and the setting (`AppName`) in one place, without renaming other settings, and cover it with a logging test. Not in scope: other logging open points of S1-22 (P6 clock, P7 privacy rules …).
> **Owner decisions 2026-10-29:** **D1** option A: the enricher reads `AppName`; `appsettings.json` sets the default `API.ProcureAccess`; Development keeps `API.ProcureAccess_Dev`. (Rejected: B, renaming the setting to `ApplicationName`, which is also the host's own key; C, using `IHostEnvironment.ApplicationName`, which deletes the setting and loses the Development name.)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `LoggingConfiguration.ConfigureSerilog` enriched every event with `config.GetValue<string>("ApplicationName") ?? "Unknown"` | One key string changes (D1) |
| F2 | The only setting was `"AppName": "API.ProcureAccess_Dev"` in `appsettings.Development.json`; `appsettings.json` and `appsettings.Production.json` had neither key; nothing else read `AppName` | Default in `appsettings.json` so every environment carries a name |
| F3 | `ApplicationName` is case-insensitively the same key as the host setting `applicationName` (`HostDefaults.ApplicationKey`). `WebApplicationFactory` sets that host setting to the assembly name, so the integration host already logged `API.ProcureAccess` while the deployed API (started without it) logged `Unknown` | The bug was invisible in tests; a test must use a name that differs from the assembly name |
| F4 | `LogSinkTests` did not assert `ApplicationName`; an assertion for `API.ProcureAccess` passed **before** the fix (because of F3) | New test with its own host and a distinct `AppName` |
| F5 | No document (ARCHITECTURE, SPEC, TESTING, CLAUDE.md) names the key | No document change |

## 2. Changes

| File | Change |
|---|---|
| `SERVICES/…/Logging/Configuration/LoggingConfiguration.cs` | The enricher reads `AppName`; a comment says why not `ApplicationName` (F3). Fallback `Unknown` unchanged |
| `API/API.ProcureAccess/appsettings.json` | `"AppName": "API.ProcureAccess"` (default for every environment; Development overrides it with `API.ProcureAccess_Dev`) |
| `tests/…Integration/Operations/LogSinkTests.cs` | Existing test also checks that `AppName` is configured and that the stored row carries it. New test `LogWarning_AppNameConfigured_StoresAppNameAsApplicationName` starts its own host on a new database with `AppName = log-sink-test-app` and checks the stored row. The polling loop moved into `WaitForEntryAsync`, shared by both tests |

Deviation from the plan: the planned unit test in `LoggingConfigurationTests` was replaced by the second integration test. A unit test would have had to capture the global console output (the only sink without a database) and could not run in parallel safely; the integration test checks the same wiring end to end, up to the table.

Documents: none changed (F5). No invariant activated, so no new TESTING version; no structure or command changed (CLAUDE.md §3–§5); no UI text. `TECH_DEBT_v1.md` stays; the sprint-end review writes v2. The behaviour of the logging section described in CLAUDE.md §3 is unchanged.

## 3. Self-review (CLAUDE §6.7, `/engineering:code-review`)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: no secret, no endpoint, no auth change; the application name is no personal data (PRV-04 unaffected) | – |
| R2 | Performance: one value read once at startup | – |
| R3 | If someone removes `AppName` from `appsettings.json`, rows fall back to `Unknown` silently | Won't fix: the existing `LogSinkTests` case now fails in that case; a startup check would be a new mechanism the fix does not need |
| R4 | Development logs now show `API.ProcureAccess_Dev` instead of `Unknown` — the value that was always intended | – (intended) |
| R5 | The new test migrates its own database (≈ 10 s), like `MigrationStartupTests` and `HealthCheckTests` | Won't fix: needed to set a host setting; the container is dropped at the end of the run |
| R6 | An environment variable `AppName` (e.g. in the compose stack) overrides the value like any other setting | – (standard configuration precedence) |

No finding is a correctness or security bug in the change; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; 112 existing warnings (#118), none in the touched files |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ 62/62 (incl. both OPS-02 tests in `LogRetentionTests`) |
| 3 | New and changed `LogSinkTests` against the old code | ✅ both fail (`appName` null; row `API.ProcureAccess` instead of `log-sink-test-app`); both pass with the fix |
| 4 | `dotnet format` whitespace + style on the solution, analyzers on the integration test project | ✅ |
| 5 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 6 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 7 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 8 | Client, E2E | – not touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | F3 is a general trap: any setting whose key equals a host setting (`applicationName`, `environment`, `contentRoot`) reads differently under `WebApplicationFactory` than in the deployed API. Nothing else is affected today | Note only; no issue proposed |
| P2 | Logs carry no environment name; Development is told apart only by `AppName`. Each environment has its own database, so this is enough today | – |

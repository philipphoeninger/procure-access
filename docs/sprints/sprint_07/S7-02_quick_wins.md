# S7-02 — Quick wins: evidence race, TimeSpan reader, legacy remnants, log sink, page title

> **Sprint:** 07 · **Task:** S7-02 (1 point) · **Issues:** #453 (TECH_DEBT_v6 TD-88), #456 (TD-94), #437; TD-95 and TD-68 (no issue)
> **Requirements / invariants:** R-CAN-05 (WSP-01 stays green), R-AUTH-02, R-AUTH-03, R-AUTH-05 (SEC-10, SEC-11 stay green), ARCHITECTURE §9.5 (OPS-02 stays green), R-SHL-04 (A11Y-03 stays green); no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-26)
> **Scope:** exactly the row of S7-02; one commit per item. No EF migration, no new dependency, no lock-file change.
> **Owner decisions:** none

## 0. Plan (written before the code)

The owner cannot be asked in this batch, so the plan stands here instead of waiting for approval.

- **Starting point:** `main` at f54bee9 (sprint 07 prepared, ROADMAP_v5 removed).
- **#453 (TD-88):** `CandidateService.AddEvidenceAsync` and `ChangeEvidenceAsync` catch the violation of the unique index over the procedure rows (`23505` with its constraint name, now a constant of `TestEvidenceConfiguration`, same name as before: no migration) and answer `DuplicateTestType` (400 at `testTypeId`). Test: two integration tests in `CandidateEndpointsTests` that run the service with a save interceptor which lets a real `POST …/test-evidences` of the same procedure commit between the check and the save (the pattern of `SavedListConcurrencyTests`) — deterministic, no sleep.
- **#456 (TD-94):** one reader `DurationSetting` in `Platform.Contracts/Configuration` (beside `EntityTags`, no package needed: it takes the raw string) that accepts only `hh:mm:ss` or `d.hh:mm:ss` (optional fraction of a second) and fails the start with the setting's name otherwise; every 🔧 `TimeSpan` setting goes through it (list in §2). Unit tests: `"15"` refused, `"00:00:15"` accepted, and one case per settings class.
- **TD-95:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and the Identity global usings leave SERVICES; `Permissions` and `CustomClaimTypes` leave MODELS. Only tests read them, so the two values still used move to `IdentityAccess.Contracts` as `PlatformPermissions` (as S6-10a P5 proposed), the four unused `objects:*` constants are deleted, `SessionProbeEndpoint` uses the moved claim type. Test: build and the existing tests; `THIRD-PARTY-NOTICES.txt` checked.
- **#437:** first measure on an empty database whether the first batch is lost (console against `logging."SeriLogs"`), then let the PostgreSQL sink hold its events in memory until the migrations have run: a gate around the sink (`WriteTo.Wrap`), opened by `Program.cs` after the migrations (and before the critical entry of a failed start) and by the two command hosts after their build. Unit tests of the gate; the measurement repeated after the change.
- **TD-68:** `ConfirmEmailPage` calls `PageTitleStrategy.changePageInPlace` when a second link brings a new result at the same address. Test: the existing "second link" case of `confirm-email.routing.spec.ts` also checks the title.
- **Invariants:** none activated; WSP-01, SEC-10, SEC-11, OPS-02, A11Y-03 stay green. **Migration:** none. **Figma:** no screen changes (the confirmation page has no frame and keeps its look). **E2E:** not run locally (batch rule); left to CI.
- **One session:** yes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Two concurrent adds of one procedure both pass `CandidateProduct.AddEvidence`; the second save fails with `23505` on the unique index `IX_TestEvidences_CandidateProductId_TestTypeKey` — not with the concurrency exception of the candidate's version, which `ConcurrentChange` would have run again — and the exception leaves as 500. Reproduced with the new tests on `main` (both failed with `DbUpdateException`); the same holds for a `PATCH` that moves an evidence to a procedure another request has just added | Catch that violation by name in both paths |
| F2 | Fifteen 🔧 duration settings in nine places; four classes parse with `TimeSpan.TryParse`, two with `GetValue<TimeSpan?>`, two bind through the options binder. All accept `"15"` as 15 days | One reader, used everywhere (§2.2) |
| F3 | No duration setting is set in `.env.example`, the compose files, `appsettings.Development.json` or `appsettings.Production.json`; the owner's `.env` names none either (checked by key name only, no value read). The integration and unit tests set only `hh:mm:ss` values | **No existing value fails** with the reader |
| F4 | SERVICES uses no Identity type; its Identity package pinned `Microsoft.EntityFrameworkCore.Relational` 10.0.15 by accident: without it Npgsql brings 10.0.4 and the build fails with MSB3277 against `DAL.dll` (built with 10.0.15 through `EntityFrameworkCore.Design`) | Pin Relational 10.0.15 in SERVICES instead (§2.3) |
| F5 | `Permissions` and `CustomClaimTypes` are read only by tests (`TestUsers`, `TestAuthHandler`, `PlatformRoleMigrationTests`) and by the global usings of API, DAL and SERVICES; `ObjectsCreate/Read/Update/Delete` by nothing | Move the two used values, delete the rest |
| F6 | **#437 measured** (API from source, `Development`, level `Information`, PostgreSQL 17 in a throwaway container): on a database that exists but is empty, PostgreSQL logged one `schema "logging" does not exist` with the sink's `COPY`; on a missing database, the sink's first connection failed with `database "ProcureAccessDb" does not exist`. In both runs **every console line reached `logging."SeriLogs"`** (79 of 79, 274 of 274), the first one („Applying pending migrations...“) included: Serilog's batching kept the failed batch and wrote it again after its pause | Nothing was lost; the fix removes the failed write (§2.4) |
| F7 | `PageTitleStrategy.updateTitle` returns early for the same path, and a second confirmation link ends at the same path, so only H1 and text changed | The page calls `changePageInPlace` (§2.5) |

## 2. Changes

1. **#453 (commit 1).** `TestEvidenceConfiguration.ProcedureIndex` names the unique index by EF's default name (`HasDatabaseName`, no migration: `has-pending-model-changes` for `WorkspaceDbContext` reports none). `CandidateService.RefusingRacedDuplicatesAsync` wraps `AddEvidenceAsync` and `ChangeEvidenceAsync` outside `ConcurrentChange.RunAsync`; a `DbUpdateException` whose inner `PostgresException` is `23505` on that index clears the tracker and answers `DuplicateTestType`, which the endpoint maps to 400 `validation` at `testTypeId` (`candidate.evidence.list.duplicate` on S34a, unchanged). Tests in `CandidateEndpointsTests`: `AddEvidence_SameProcedureSavedMeanwhile_IsRefusedAsADuplicate` and `ChangeEvidence_ToAProcedureSavedMeanwhile_IsRefusedAsADuplicate` (both `INV:WSP-01`) run the service as Editor A with a save interceptor that lets Owner A's real `POST` of the same procedure commit between the check and the save — deterministic, no sleep, no parallel timing.
2. **#456 (commit 2).** `Platform.Contracts/Configuration/DurationSetting.Read(value, setting, zeroAllowed = false)`: `TimeSpan.TryParseExact` with `hh:mm:ss`, `d.hh:mm:ss` and both with a fraction; a missing, malformed or zero value (unless allowed) throws `InvalidOperationException` naming the setting and the expected form. It takes the raw text, so `Platform.Contracts` stays without packages. **Every duration setting reads through it:**

   | Setting | Read in |
   |---|---|
   | `Session:IdleTimeout`, `AbsoluteTimeout`, `WarningLead`, `SecurityStampValidationInterval` (zero allowed) | `SessionSettings.From` |
   | `Account:Tokens:EmailConfirmationLifetime`, `PasswordResetLifetime`, `CleanupInterval` | `AccountTokenSettings.From` |
   | `Account:Lockout:Duration` | `LockoutPolicy.From` |
   | `Organizations:Invitations:Lifetime`, `CleanupInterval` | `InvitationSettings.From` |
   | `Email:Smtp:Timeout` (plus its MailKit bounds) | `EmailSettings.From` |
   | `Health:ReadyTimeout` | `HealthSettings.From` |
   | `RateLimits:<policy>:Window`, eleven policies | `RateLimitingRegistration.FixedWindowLimit.From` |
   | `AppLoggingSettings:Retention:RunInterval` | `AddLogRetention`: bound as before, then `Configure<IConfiguration>` reads it again through the reader (the binder alone would take `"15"`); `ValidateOnStart` makes it fail the start |
   | `UsageEvents:RetryDelay` | `AddUsageEvents`, the same way |

   `TestClock`'s offset is a cookie in seconds, not a setting, and stays as it is. **Would an existing `.env` value now fail?** No (F3). A deployment that sets one of these through an environment variable in another form — a bare number, `00:15` without seconds, a one-digit hour — now stops at the start with the setting's name instead of running with a wrong value. Tests: `DurationSettingTests` (accepted and refused forms; zero; `"15"` refused through each settings class for each of the fifteen setting groups; the shipped `appsettings.json` read through all of them). `TECHNICAL.md` §3.2 tells operators the form; `CLAUDE.md` §3 (🔧 settings) names the rule.
3. **TD-95 (commit 3).** SERVICES: the package `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and the global usings `Microsoft.AspNetCore.Identity`, `.Identity.Data` and `MODELS…Authorization` removed; `Microsoft.EntityFrameworkCore.Relational` 10.0.15 pinned with the reason (F4). This is not a new dependency: the API image ships that package at that version already, and `check-api-notices` is unchanged (38 packages, 0 errors). MODELS: `Entities/Authorization/` deleted. `IdentityAccess.Contracts/Accounts/PlatformPermissions` holds `ClaimType` (`permission`) and `ModerationReview` (`moderation:review`) beside `PlatformRoles`; `SessionProbeEndpoint` uses it instead of its own constant; tests updated; the global usings of API and DAL dropped.
4. **#437 (commit 4).** `Platform/Logging/DatabaseLogGate` sits in front of the PostgreSQL sink (`LoggerSinkConfiguration.Wrap`; the gate is not disposable, so Serilog still disposes and flushes the wrapped sink). Closed, it queues events up to `AppLoggingSettings:PostgreSQL:QueueLimit` and drops newer ones (counted in Serilog's self log); `Open` passes the queue in order, then every event goes straight through. `PlatformModule.StartDatabaseLogSink(this IHost)` opens it: `Program.cs` after the migrations and in the start-failure catch before the critical entry (so `MigrationStartupTests` still finds it), `CatalogCommandHost` and `RoleCommandHost` after their build. The console sink is not gated. Measured again after the change (same set-up as F6): 79 of 79 and 274 of 274 lines stored, no `schema "logging"` error and no failed sink connection in PostgreSQL's log; what remains there are EF Core's own probes (`relation "…__EFMigrationsHistory" does not exist`, and on a missing database the `FATAL` of EF's existence check), which are expected and now named in `TECHNICAL.md` §6.3. Tests: `DatabaseLogGateTests` (held until open, order, limit, open twice, one sink only, the registration with and without a connection string) and `LogSinkTests.Start_EmptyDatabase_StoresTheEventsWrittenBeforeTheMigrations` (a new database at level `Information`: the first stored entry is „Applying pending migrations...“).
5. **TD-68 (commit 5).** `ConfirmEmailPage` subscribes to its route data, skips the first result and calls `PageTitleStrategy.changePageInPlace(result)` for every new one (the router emits data only when it changed, so S01 opening over the page changes nothing). The spec's „second link“ case now also checks title, focus on the H1 and the announcement; it fails without the change (checked).
6. **Docs (commit 6).** With it one more refused case in `DurationSettingTests` (`8:00:00`, a one-digit hour), added in the self-review. CODEBASE §1 (the `CLAUDE.md` change), §2.0, new §2.118, §3 (Platform.Contracts, IdentityAccess.Contracts, Platform lines); `TECHNICAL.md` §3.2 and §6.3; `CLAUDE.md` §3; this protocol. ARCHITECTURE, SPEC, TESTING, UX copy, Handoff and Figma mapping are unchanged: no section describes the parsing of durations, the start order of the log sink or the confirmation page's title at this level, and no invariant changes status. TECH_DEBT stays at v6 (new version at the sprint end); TD-68, TD-88, TD-94, TD-95 and #437 are closed by this PR.

## 3. Self-review (CLAUDE §6.7, §6.8)

Security and privacy: no endpoint, no new log content; the gate holds the same events the sink would write, in memory, for seconds. The duration error quotes the configured value, as the replaced messages did; none of these values is a secret. Correctness: the evidence catch matches only the one index by name, so any other unique violation still surfaces; it runs outside `ConcurrentChange`, so a stale `If-Match` still answers 409 first. No N+1 (no query added). Accessibility: the confirmation page now behaves like a route change (A11Y-03).

| Finding | Sorting | Reason / fix |
|---|---|---|
| `dotnet format` refused the multi-line property pattern of the first catch | fix | Rewritten as a plain condition (amended into commit 1) |
| The three-argument `WriteTo.Wrap` is obsolete in Serilog 4.4 (CS0618 with warnings as errors) | fix | Two-argument `LoggerSinkConfiguration.Wrap` inside `WriteTo.Sink` |
| `LockoutPolicy` names its key constant `DurationSetting`, the same as the reader | won't fix | The reader is called by its full name there; renaming the public constant would touch its tests for no gain |
| A one-digit hour (`8:00:00`) is refused now, though it was accepted before | won't fix | The owner's rule is `hh:mm:ss`; no shipped or configured value uses one digit (F3); the error names the form |
| The integration test of #437 would also pass without the gate (Serilog retried) | won't fix | It guards the no-loss property the issue asked about; the gate's behaviour is unit-tested, the removed failed write was measured (§2.4) |
| The client spec does not cover S01 opening over the page after a second link | won't fix | The router emits route data only on change; the existing S01 case is unchanged and green |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` (unit, architecture, integration with Testcontainers) | 2228 passed, 0 failed, 0 skipped; afterwards `DurationSettingTests` again with the one-digit-hour case: 36 passed |
| `dotnet ef migrations has-pending-model-changes` (`WorkspaceDbContext`) | no changes |
| `node scripts/check-api-notices.mjs` | 38 packages, 0 errors |
| Client `npm run lint` | all files pass |
| Client `npx ng test` | 108 files, 1155 tests passed |
| `node scripts/check-invariants.mjs` | gate passed |
| `node scripts/check-figma-links.mjs` | 12 checked, 14 not built yet, 0 pending |
| `node scripts/check-i18n.mjs`, `check-icons.mjs` | 0 errors |
| REUSE | `fsfe/reuse:6.2.0 lint-file` on the five new source and test files and this protocol: no finding. The full `reuse lint` reports false positives in a worktree (no git inside the container), so it was not run; CI runs it |
| #437 measurement | §1 F6 (before) and §2.4 (after) |
| E2E | not run locally (batch rule: one local E2E stack, other sessions in parallel); left to CI |
| Figma | no frame touched: the confirmation page keeps its look (Handoff §5.3) |

## 5. Open points

1. **API project:** `API.ProcureAccess.csproj` still references `Microsoft.AspNetCore.Identity.EntityFrameworkCore` directly and its global usings import Identity namespaces. TD-95 names SERVICES only; whether the host still needs the package now that IdentityAccess owns Identity is a check of its own (proposed for the next change of the host or the legacy removal).
2. **EF Core version skew in the legacy projects:** DAL and MODELS reference EF Core 10.0.15 with Npgsql 10.0.3, which brings `EntityFrameworkCore.Relational` 10.0.4; only `EntityFrameworkCore.Design` in DAL and now the pin in SERVICES lift it to 10.0.15. The modules have the same pair. A Dependabot update of Npgsql settles it; nothing to do here.
3. **TECH_DEBT:** TD-68, TD-88, TD-94, TD-95 and #437 close with this PR; the register records it in its next version (sprint end).

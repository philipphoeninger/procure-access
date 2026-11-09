# S3-23 — Usage events: fire-and-forget writes

> **Sprint:** 03, stretch task S3-23 (ROADMAP_v3 M2.7d, 1 point). **Issues:** #115. **Requirements / invariants:** SPEC R-MET-01 („writing an event never fails the user's request (fire-and-forget with error log)“); PRV-05 stays green; no invariant activated, so no new TESTING version. **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: the writer no longer awaits the insert on the request path; a short retry delay; failures logged and swallowed; tests on the fake clock. **Not in scope:** the first callers (S11 `CriteriaListShown` in M2.6, the export `ExportCreated` in M2.7); the indicator queries of R-MET-02 (S1-23 P5).
> **Owner decisions** (2026-11-09, asked before the task, all as recommended):
> 1. **D1 A — bounded queue and background service.** The writer puts the event into a bounded in-memory `Channel` (part of .NET, no new package); a hosted service of the Platform module stores it. Not B (direct write without retries: the request still waits up to the timeout) and not C (unawaited task: no back-pressure, silent loss on shutdown).
> 2. **D2 — retry only what was never sent.** The background service writes without EF's retrying execution strategy; it retries only when the connection could not be opened (the insert was never sent, so it cannot be doubled) and keeps the batch meanwhile. An insert that was sent and failed is logged as lost and not retried: an event may be lost, never counted twice.
> 3. **D3 — limits.** Section `UsageEvents` in `appsettings.json`: `QueueCapacity` 1000, `RetryDelay` 5 s; retry for as long as the queue holds the events; when the queue is full new events are dropped, logged once per outage; on shutdown the queue is emptied until the host's shutdown timeout and the rest is logged as lost; single API instance as for migrations and log retention.
> 4. **D4 — `void` methods.** `IUsageEventWriter.CriteriaListShown` / `ExportCreated` return nothing (a `Task` that is always complete would mislead callers); nothing calls them yet.
> 5. **Test package** (asked with the plan): `Microsoft.Extensions.TimeProvider.Testing` 10.11.0 (MIT, Microsoft) added to `ProcureAccess.Tests.Unit`; the integration tests already used it; a test tool, not shipped (CLAUDE §10).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `UsageEventWriter` (Platform, singleton since S2-07) awaited `SaveChangesAsync` in a scope of its own; `PlatformDbContext` is registered with `EnableRetryOnFailure().CommandTimeout(60)` | With the database down, the caller waited through up to six retries (#115, S1-23 R1/P4) |
| F2 | A retry after a lost commit acknowledgement stores the event a second time; nothing removes duplicates, so the SPEC §9.2 counts would be inflated | D2: the store's own options have no retrying strategy; only a failed connection open is retried |
| F3 | Issue #115 left the approach open, while the task prompt said „no owner decisions“ | Asked before the task: D1–D4 |
| F4 | ARCHITECTURE_v25 C91 and §5.7: „awaits the insert … there is no queue or background service“ | ARCHITECTURE_v26 C111 |
| F5 | Platform already runs a hosted service on `TimeProvider` (`LogRetentionService`) and binds settings with validation at start (`AddLogRetention`) | Same patterns for the background writer and `UsageEventSettings` |
| F6 | The `catalog` command host (S3-16) calls `AddPlatformModule` but never starts the host | No hosted service runs there; nothing in it records usage events |

## 2. Changes

| Area | Change |
|---|---|
| `Platform.Contracts/Analytics/IUsageEventWriter.cs` | `void CriteriaListShown(bool signedIn)`, `void ExportCreated(UsageExportFormat, bool signedIn)` (D4); the remarks describe fire-and-forget |
| `Analytics/UsageEventWriter.cs` | Builds the event with the UTC day of the call and queues it without waiting; never touches the database; a full or completed queue drops the event and logs one warning per outage; any other failure (e.g. an unknown export format) is logged as an error, as before |
| `Analytics/UsageEventQueue.cs` (new) | Bounded `Channel<UsageEvent>` (`Wait` mode, so `TryWrite` refuses instead of dropping an older event), counts dropped events, `Complete()` for shutdown |
| `Analytics/UsageEventStore.cs` (new) | `IUsageEventStore` with the outcomes `Written`, `NotConnected` (open failed, nothing sent) and `Failed` (sent and failed); own `PlatformDbContext` options without the retrying strategy, Npgsql's default timeouts; opens the connection first, then one `SaveChanges` for the batch |
| `Analytics/UsageEventBackgroundWriter.cs` (new) | `BackgroundService`: reads batches of at most 100; `NotConnected` → wait `RetryDelay` on `TimeProvider` and try the same batch again; `Failed` → error log, no retry; an unexpected exception drops only that batch (an unhandled exception would stop the host); logs the outage once, the recovery once and the number dropped after the next write; `StopAsync` completes the queue and lets the loop store it until the shutdown timeout, then logs what is lost |
| `Analytics/UsageEventSettings.cs`, `AnalyticsRegistration.cs` (new); `PlatformModule.cs` | Section `UsageEvents` bound and validated at start (both values > 0); queue, store, writer and hosted service registered by `AddUsageEvents(connectionString)` |
| `API/appsettings.json` | `"UsageEvents": { "QueueCapacity": 1000, "RetryDelay": "00:00:05" }` (D3) |
| Unit tests (`tests/ProcureAccess.Tests.Unit/Analytics/`) | `UsageEventWriterTests` rewritten (5) and `UsageEventBackgroundWriterTests` new (8) on `TimerAwareClock` (a `FakeTimeProvider` that reports each timer, so a test advances the clock only after the wait has begun) and `FakeUsageEventStore`; test package added (decision 5) |
| Integration tests (`tests/ProcureAccess.Tests.Integration/Analytics/UsageEventWriterTests.cs`) | The five existing tests call the `void` methods and wait (real time, at most 10 s) until the rows are stored; new: an unreachable database (port 1) gives `NotConnected`, and the store's context has no retrying strategy while the module's has |
| Documents | `ARCHITECTURE_v26.md` (C111, §5.7, note on C91; v24 deleted, two newest kept); `CLAUDE.md` §3 and version line; this protocol. SPEC unchanged: R-MET-01 already asks for fire-and-forget |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff. N+1 and a11y: not applicable (one batched insert, no UI). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | A cancellation that is not the host's (e.g. an internal timeout surfacing as `TaskCanceledException`) passed the store's `ex is not OperationCanceledException` filter and the loop's `when (stoppingToken…)` filter; an unhandled exception in a `BackgroundService` stops the whole host by default | **Fixed:** the store classifies by „was our token cancelled“, and the loop drops only the batch on any unexpected exception; test `Write_StoreThrowsUnexpectedly_DropsThatBatchAndKeepsRunning` (fails without the guard) |
| R2 | Events in the queue are lost when the process ends abruptly (crash, kill) | Won't fix: D1 accepts it; a normal stop empties the queue |
| R3 | During a long outage the service retries every 5 s without limit | Won't fix: D3 („retry as long as the queue holds the events“); each attempt is one connection attempt, logged once per outage |
| R4 | The first call after a full queue logs one warning; drops during shutdown use the same message („full or the API is stopping“) | Won't fix: one message for both cases is enough for an operator |
| R5 | Privacy: the logs carry counts, event types and Npgsql exceptions (host and database names, no people); the entity and its columns are unchanged, PRV-05 green | – |
| R6 | Security: no endpoint, no input from users reaches the queue except the two booleans and the format enum; no secrets in code (the store's connection string comes from configuration as before) | – |
| R7 | Mutation checks: treating `Failed` like `NotConnected` fails `Write_InsertSentAndFailed_DoesNotStoreTheBatchAgain`; a real-time `Task.Delay` does not compile (`timeProvider` unread, warnings are errors); removing the loop guard fails the R1 test | – |
| R8 | The integration tests wait in real time for the background service (at most 10 s, polling every 50 ms) | Won't fix: the only way to observe the hosted service of the shared host; the unit tests cover timing on the fake clock |

No correctness or security finding left that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ exit 0 |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ 634 tests, 0 failed (PRV-05, ARC-01 – ARC-03 included) |
| 4 | Usage-event unit tests 20 times in a row (background thread, flakiness) | ✅ 20/20 green |
| 5 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v37.md`) |
| 6 | `node scripts/check-figma-links.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending) |
| 7 | `node scripts/check-i18n.mjs` | ✅ 0 errors (no UI text touched) |
| 8 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 9 | Client, E2E | – not touched |
| 10 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | First callers: S11 calls `CriteriaListShown` once per complete selection shown (R-CON-10), the export calls `ExportCreated` after the file is generated (M2.7). Both just call the `void` method; no `await`, no `try` | M2.6, M2.7 |
| P2 | `TECH_DEBT_v3` (sprint close-out, step 14): #115 resolved; S1-23 P4 is done with it | Sprint 03 close-out |
| P3 | `TESTING_v37` still names `ARCHITECTURE_v25.md` as its input; no TESTING version was written because no invariant or test rule changed | Next TESTING version |
| P4 | `CLAUDE.md` version line: S3-22 (PR #222, not merged yet) appends to the same line, so whichever PR merges second needs a one-line merge | Merge of #222 / this PR |

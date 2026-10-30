# S1-23 — `UsageEvent` table and writer (no events yet)

> **Sprint:** 01, stretch task S1-23 (roadmap M1.7; SPEC R-MET-01 table part, §9.1, §11 step 1; ARCHITECTURE §5.7; TESTING §4.3 PRV-05). **Status:** done by Claude Code (2026-10-25).
> **Scope:** table `analytics."UsageEvents"`, the writer `IUsageEventWriter`, PRV-05 set to `active`. Not in scope: calling the writer (S11 and the export come in step 2), the indicator queries in `TECHNICAL.md` (R-MET-02), moving the code into the Platform module (M1.1).
> **Owner decisions 2026-10-25:** **D1** PRV-05 `active` now, in a new `TESTING_v13.md` (as in S1-22). **D2** the writer awaits the insert in a scope of its own and logs and swallows every failure; no queue or background service. **D3** `OccurredOn` is the **UTC** day. **D4** one typed method per event of SPEC §9.1 instead of a generic `Write(type, dim1, dim2)`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No `analytics` schema and no usage-event code existed; only `identity` and `logging` (S1-22) besides `public` | Built from scratch in the S1-22 pattern (entity in `MODELS`, Fluent API configuration, service in `SERVICES`) |
| F2 | `ApplicationDBContext` caps every string at 200 characters by convention, and its error wrapping covers only the synchronous `SaveChanges` | Column lengths set explicitly; the writer handles its own errors |
| F3 | The DbContext is scoped and uses `EnableRetryOnFailure()` | Writing through the caller's context would also save the caller's unsaved changes and leave a failed insert tracked: the writer opens a scope of its own |
| F4 | PRV-05 was `planned` (level A, step 1); S1-22 had set other step-1 invariants `active` ahead of time | D1 |
| F5 | The architecture test project was empty and allowed "no tests ran" (`--ignore-exit-code 8`, S1-12) | PRV-05 is its first test; the allowance is removed |
| F6 | Respawn empties every schema except the migration history | The new schema needs no fixture change |
| F7 | SPEC writes `analytics.UsageEvent`; the repository's tables are plural (`SeriLogs`, `Criteria`) | Table `analytics."UsageEvents"`, entity `UsageEvent` (P2) |
| F8 | No document says which time zone decides the day | D3 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `DAL/MODELS…/Entities/UsageEvent.cs`, `UsageEventType.cs` (new) | `Id`, `EventType`, `OccurredOn` (`DateOnly`), `Dimension1?`, `Dimension2?`; the enum has exactly `CriteriaListShown` and `ExportCreated` | ARCH §5.7, SPEC §9.1 |
| `DAL/MODELS…/Entities/Configuration/UsageEventConfiguration.cs` (new) | Schema `analytics`, table `UsageEvents`; `Id` identity; `EventType` as text (`varchar(64)`) so the R-MET-02 queries stay readable; `OccurredOn` `date`; dimensions `varchar(32)`; index (`EventType`, `OccurredOn`) for the monthly counts | ARCH §5.7, SPEC §9.2 |
| `DAL/…/EFStructures/ApplicationDBContext.cs` | `DbSet<UsageEvent>` and its configuration | – |
| `DAL/…/Migrations/20261025141241_AddUsageEventTable` (new) | Creates schema, table and index; `Down` drops the table and the schema. The snapshot only gains the new entity | – |
| `SERVICES/…/Analytics/IUsageEventWriter.cs`, `UsageEventWriter.cs`, `AnalyticsConfiguration.cs` (new) | `CriteriaListShownAsync(signedIn)`, `ExportCreatedAsync(format, signedIn)`; dimension values `anonymous`/`signedIn`, `xlsx`/`csv` as constants; UTC day from `TimeProvider`; own `AsyncServiceScope` and `ApplicationDBContext`; every exception (also an unknown format) is logged as an error with the event type only and not rethrown; registered as a singleton by `AddUsageEvents()` | R-MET-01, D2–D4 |
| `API/…/Program.cs`, `GlobalUsings.cs` | `AddUsageEvents()` | – |
| `tests/…Architecture/Privacy/UsageEventPrivacyTests.cs` (new) | **INV:PRV-05** ×3: entity properties are exactly the allow-list; mapped columns of the design-time EF model (incl. shadow properties) are exactly the allow-list and none looks like a personal reference; `OccurredOn` is `DateOnly` / `date` and no property has a time type. Needs no database | PRV-05 |
| `tests/…Architecture/….csproj` | `--ignore-exit-code 8` removed | F5 |
| `tests/…Integration/Analytics/UsageEventWriterTests.cs` (new) | Each event and dimension combination stores the fixed values and the fake clock's day (4 cases); 23:59:59 and 00:00:01 UTC land on two UTC days; unsaved changes of a caller's context stay unsaved and tracked; an unknown format throws nothing and stores nothing | R-MET-01, D3 |
| `tests/…Unit/Analytics/UsageEventWriterTests.cs` (new) | Database unavailable (scope creation throws): both methods return normally and log one error | R-MET-01 |
| `docs/TESTING_v13.md` (new) | Copy of v12: status line; V63 (PRV-05 `active`, other step-1 invariants stay `planned`) | D1 |
| `CLAUDE.md` §3 | Current state (S1-23) | CLAUDE §11 |

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Action |
|---|---|---|
| R1 | **Delay, not failure:** the insert is awaited and the DbContext retries on transient errors (`EnableRetryOnFailure`, Npgsql default up to 6 retries). With a database that is down, the caller waits for the retries before the writer gives up. S11 and the export read the catalog from the same database, so such a request fails anyway; on a short blip the retry helps | Unsure; revisit when step 2 calls the writer (option: no retry or a short timeout for this insert) |
| R2 | **Sequential `Id`:** the identity column orders events within a day. Together with request logs at the same level of detail, the order could hint at which request produced which event. The default log level is `Warning`, so requests are not logged today. ARCH §5.7 lists no key; EF needs one for inserts | Unsure; recorded as P3 |
| R3 | The writer passes `CancellationToken.None`: the event describes something that already happened, so a client disconnect does not drop it. During shutdown one pending insert can delay the stop by its duration | No change |
| R4 | The error log carries the exception (Npgsql messages name host and database, not people) and the event type; the event itself has no personal data | No change |
| R5 | `UsageExportFormat` lives in `SERVICES.Analytics`; the Export module of step 2 will have its own format type and has to map it | No change; noted for step 2 |
| R6 | The name-fragment check in the PRV-05 test (`user`, `org`, `ip`, `session`, …) can produce false positives for future columns; the allow-list is the real guard and makes the fragments a second line | No change |
| R7 | The constructor of `ApplicationDBContext` writes "Saving changes" to the console on every save, so every event prints two console lines (existing behaviour) | No change |
| R8 | No UI change; no endpoint added; no rate limit or antiforgery concern | – |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors, 112 warnings (unchanged), none in the new files |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ 51/51 (Unit 13, Architecture 3, Integration 35) |
| 3 | Code broken on purpose: an extra `SessionKey` property / `OccurredOn` as `timestamp with time zone` / a shadow column `ClientIp` / `throw;` in the catch | ✅ PRV-05 2 of 3 fail / ✅ 2 of 3 fail / ✅ 1 of 3 fails / ✅ both unit tests fail; all restored |
| 4 | `dotnet ef migrations has-pending-model-changes` | ✅ no changes |
| 5 | Throwaway PostgreSQL 17: `database update` → columns, types, nullability and index as configured; back to `ReworkLogTableForPostgreSqlSink` → schema gone; update again → table back | ✅ |
| 6 | Compose stack (`db`, `api`, `Development`, existing volume): `/health/ready` 200, `/api/Criteria` 200, last migration `20261025141241_AddUsageEventTable`, table empty | ✅ |
| 7 | `dotnet format` whitespace + style on the solution, analyzers on each test project | ✅ |
| 8 | `npx ng test` / `npm run lint` (Node 24.22.1) | ✅ 5/5 / ✅ (no client change) |
| 9 | E2E stack, Playwright chromium, firefox, mobile-chrome | ✅ 39/39; webkit and mobile-safari not run locally (host library missing, S1-22 check 7); CI runs them |
| 10 | `node scripts/check-invariants.mjs` | ✅ reads `TESTING_v13.md`, 8 active: CAT-10 2, PRV-05 3, SEC-06 1, ARC-06 1, OPS-01 2, OPS-02 2, OPS-03 2, A11Y-01 2 |
| 11 | `node scripts/check-figma-links.mjs` / `reuse lint` 6.2.0 | ✅ unchanged (0 checked, 12 not built, 1 pending) / ✅ compliant |

During check 6 the root file system was full (0 bytes free; the database container could not write its lock file). `docker image prune -f` (dangling images only, owner's choice) freed 4.5 GB.

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Record in the next ARCHITECTURE version (§5.7): table name `UsageEvents` with an `Id` key, UTC day (D3), awaited write in its own scope instead of literal fire-and-forget (D2); SPEC R-MET-01 says "fire-and-forget with error log" | Next ARCHITECTURE / SPEC version |
| P2 | SPEC R-MET-01 names `analytics.UsageEvent`; the table is `analytics."UsageEvents"` (repository convention). Align the SPEC wording or rename the table | Philipp |
| P3 | Sequential `Id` orders events within a day (R2). If that matters, a random key (`uuid`) removes the order; decide before the first pilot data | Philipp / step 2 |
| P4 | Delay with an unreachable database through the retry policy (R1) | Step 2, when S11 and the export call the writer |
| P5 | The indicator queries (R-MET-02) go into `TECHNICAL.md`, which does not exist yet | R-MET-02 task |
| P6 | M1.1 moves `UsageEvent` and the writer into the Platform module | **Done 2026-10-30** in S2-07 (`PlatformDbContext`, ADR-014) |

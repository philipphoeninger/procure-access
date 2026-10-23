# S1-13 — Integration fixture

> **Sprint:** 01, task S1-13 (milestone M0.3, TESTING §3.1, §5.2, §6.1, §8.1, §9 step 0). **Status:** done by Claude Code (2026-10-23).
> **Scope:** Testcontainers PostgreSQL 17, Respawn reset, `WebApplicationFactory` host, test authentication handler, `TimeProvider` registered in the app and replaced by `FakeTimeProvider` in tests, test `HttpClient` with the antiforgery hook. Not in scope: invariant tests (ARC-06/OPS-03 in S1-14, CAT-10 in S1-15), replacing the other `DateTime.UtcNow` calls (S1-03 §3: out of scope for M0), the Tenants/MiniCatalog fixtures (steps 2–3).
> **Owner decisions 2026-10-23:** **1A** Respawn empties every table except the migration history, including the seed data of migration `AddSeedData`; each test stores its own data. **2A** `HealthController` reads `TimeProvider`, so a test proves the fake clock reaches the app. **3** Auth handler and fixture users in `Shared` (TESTING §3.4); container, factory and antiforgery hook in `Integration`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `Program.cs` reads configuration while the builder is still being set up: the connection string (`AddPostgreSQLConnection` throws without it) and `AppLoggingSettings` (`ConfigureSerilog`, only in `appsettings.Development.json`) | The factory passes them with `UseSetting`; checked that these win over `appsettings.json` |
| F2 | Outside `Development` the Serilog file sink writes to `<Drive><volume separator><dir separator><FilePath>/<FileName>` | Test settings point it at `<temp>/procureaccess-tests`. The file only appears when something is logged at Warning or above |
| F3 | `JWTSettings:Key` is empty without `.env` | Random key per test run; no key in the repository |
| F4 | The default authorisation policy is tied to the JwtBearer scheme | The factory makes the test scheme the default and rebuilds the default policy on it |
| F5 | Migration `AddSeedData` inserts reference data with `InsertData` | Removed by the first reset (decision 1A) |
| F6 | xUnit v3 4.0.2 marks `CollectionBehavior(DisableTestParallelization)` as an error (CS0619) | `[assembly: Parallelization(Mode = ParallelMode.None)]` instead |
| F7 | .NET 10 makes the top-level `Program` public | No `public partial class Program` needed |
| F8 | The developer machine's root file system was full (0 bytes free); the first run's `postgres:17` pull ran out of space during `initdb` | Fixture uses `postgres:17-alpine` (already present locally, same image as `docker-compose.yml`); the pulled `postgres:17` image was removed again. See open point P1 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `API/.../Program.cs` | `AddSingleton(TimeProvider.System)` | CLAUDE §7, TESTING §3.1. No change in behaviour: ASP.NET Core authentication and Identity already default to the system clock |
| `API/.../Controllers/HealthController.cs` | `time` from `TimeProvider` instead of `DateTime.UtcNow`; same JSON shape (UTC `DateTime`) | Decision 2A. The development-only `/health` in `Program.cs` is unchanged (health endpoints are M1.2) |
| `tests/.../Shared/Auth/TestAuthHandler.cs` | Scheme `Test`; signs in the fixture user named in the `X-Test-User` header; no header = anonymous; unknown name = authentication fails (401) | TESTING §5.2. A typo cannot pass as anonymous |
| `tests/.../Shared/Auth/TestUsers.cs` | `TestUser` record; `TestUsers.Member`, `TestUsers.Approver` with fixed ids, `@example.test` addresses, roles and permission claims of the current model | Covers S1-10 O7 for integration tests. Replaced by the Tenants fixture (TESTING §6.2) in step 3 |
| `tests/.../Shared/Builders/FilterTypeBuilder.cs`, `TestData.AFilterType()` | Builder for the existing `FilterType` | Data for the sample test (TESTING §6.1) |
| `tests/.../Shared/*.csproj` | `FrameworkReference Microsoft.AspNetCore.App` | Authentication types for the handler |
| `tests/.../Integration/Infrastructure/ApiFixture.cs` | Assembly fixture: container, `MigrateAsync()` once, Respawn checkpoint (ignores `public.__EFMigrationsHistory`), factory, `FakeTimeProvider` (start 2026-03-02 09:00 UTC), `StoreAsync` through the real `ApplicationDBContext` | TESTING §5.2, §6.1. Migrates itself because `Program.cs` swallows migration errors |
| `tests/.../Integration/Infrastructure/ProcureAccessApiFactory.cs` | `WebApplicationFactory<Program>`, environment `Testing`, settings F1–F3, test auth (F4), fake clock | TESTING §3.1, §5.2 |
| `tests/.../Integration/Infrastructure/AntiforgeryHandler.cs` | Before the first state-changing request: `GET /api/v1/auth/session` once, as the same test user; 404 is tolerated, any other failure throws. Every state-changing request: `XSRF-TOKEN` cookie → `X-XSRF-TOKEN` header, read fresh each time (token rotation on sign-in/out, ARCH C60) | TESTING §5.2: no POST test has to change in step 3 |
| `tests/.../Integration/Infrastructure/IntegrationTest.cs` | Base class: `CreateClient(TestUser?)` (own cookie jar, base `https://localhost` so `Secure` cookies are kept), `StoreAsync`, `Clock`; class fixture `DatabaseReset` runs Respawn once per test class | TESTING §5.2 ("between test classes") |
| `tests/.../Integration/AssemblyInfo.cs` | Assembly fixture; parallel runs off (F6) | One shared database |
| `tests/.../Integration/FixtureTests/*` | 11 tests, see §3 | Done when |
| `tests/.../Integration/*.csproj` | Mvc.Testing, TimeProvider.Testing, Respawn, Testcontainers.PostgreSql; `--ignore-exit-code 8` removed | S1-12 P3 |
| `CLAUDE.md` §3, §5 | Current state; integration tests need Docker | CLAUDE §11 |

**Packages** (latest stable on nuget.org, 2026-10-23; licences from the nuspec; no `NU1xxx` audit warning on restore):

| Package | Version | Licence |
|---|---|---|
| Microsoft.AspNetCore.Mvc.Testing | 10.0.13 | MIT ✅ |
| Microsoft.Extensions.TimeProvider.Testing | 10.11.0 | MIT ✅ |
| Respawn | 7.0.0 | Apache-2.0 ✅ |
| Testcontainers.PostgreSql (+ Testcontainers, Docker.DotNet.Enhanced, SSH.NET, SharpZipLib) | 4.16.0 | MIT ✅ |

**Tests** (names `Scenario_Condition_ExpectedOutcome`; no `INV:` tags because no invariant is activated in this task):

| Class | Tests | Shows |
|---|---|---|
| `DatabaseFixtureTests` | `FilterTypes_GetAll_ReturnsOnlyFilterTypeStoredByTheTest` | **Done when:** existing endpoint against the real database; only the stored row comes back, so the reset ran |
| `TestAuthenticationTests` | anonymous → 401, fixture user → 200, unknown test user → 401 on `GET /api/Proposals` | Test authentication handler |
| `ClockTests` | `Health_AfterAdvancingFakeClock_ReturnsFakeClockTime` | Fake clock reaches the app |
| `AntiforgeryHandlerTests` (stub server) | token issued → header sent; probe missing → probed once, no header; GET → no probe; token rotated → new token sent; probe fails → throws | Antiforgery hook |
| `AntiforgeryHookApiTests` | `SignIn_BeforeSessionProbeExists_ReachesEndpoint` | Hook tolerates the missing probe against the real API |

## 3. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; 136 warnings, unchanged from S1-12; none from `tests/` |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ exit 0; 13/13: Unit 2, Integration 11, Architecture 0 (still allowed to be empty until S1-14). Integration ≈ 9 s incl. container start |
| 3 | Same run with a temporary failing integration test | ✅ exit 2; test removed again |
| 4 | Effective test-host configuration (temporary probe test, removed) | ✅ environment `Testing`, `RebuildDatabase=false`, random 88-character JWT key, container connection string, `FakeTimeProvider` resolved |
| 5 | Container clean-up | ✅ no test container left after the run (Ryuk) |
| 6 | `dotnet format --verify-no-changes`, the four test projects | ✅ |
| 7 | `dotnet format --verify-no-changes`, API project, changed files | ⚠️ fails, but with the same findings on `main` (whitespace in `Program.cs`, 3-space indent in `HealthController.cs`); this task adds none. See P3 |
| 8 | `reuse lint` | ✅ compliant (484/484 files) |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | **Disk space:** the developer machine's root file system is at 99 % (≈ 300 MB free after removing `postgres:17`). Docker images and builds will fail again soon | Philipp, now |
| P2 | TESTING §3.1 names the image `postgres:17`; the fixture uses `postgres:17-alpine` (same as `docker-compose.yml` and the deployment). Record in the next TESTING version together with S1-12 P2 and R4 | Next TESTING version |
| P3 | The API project is not `dotnet format` clean on `main`. CI stage 1 (S1-20) runs `dotnet format --verify-no-changes`: either format the production projects in one separate PR or limit stage 1 to `tests/` until then | S1-20 |
| P4 | The fake clock is shared by all test classes and only moves forward (`FakeTimeProvider` cannot go back), so tests use times relative to `Clock.GetUtcNow()`. Per-class reset of the database also means tests **within** one class can see each other's rows; keep them independent (TESTING §6.4) or reset per test if that becomes a problem | Whoever writes the first time-dependent test (SEC-07/08, step 4) |
| P5 | `TestUsers` exist only as principals, not as rows in `identity.Users`. Endpoints that look the user up (`GET /api/me`) need the row stored by the test | Step 3 (Tenants fixture) |
| P6 | Six `DateTime.UtcNow` calls remain (`UserRepo` ×1, `UserService` ×4, `ProposalService` ×1) plus the development-only `/health`; they ignore the fake clock | Replaced with the code they belong to (M1/M3) |

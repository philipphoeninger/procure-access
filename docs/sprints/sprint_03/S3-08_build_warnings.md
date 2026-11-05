# S3-08 — Build warnings and analyzer check on all projects

> **Sprint:** 03, P0 task S3-08 (debt, 1.5 points). **Issues:** #118 (`TECH_DEBT_v2.md` §3.3; S2-06 P4, S2-07 P3). **Requirements / invariants:** none activated; CI stages 1 and 2 of TESTING §8.2 (`TESTING_v28.md` V90). **Status:** done by Claude Code (2026-11-05), pull request open.
> **Scope:** exactly the sprint row: fix every warning of a Release build of `ProcureAccess.sln`, `TreatWarningsAsErrors` in `src/Directory.Build.props`, analyzer check of CI stage 1 on every project, drop the unused `Serilog.Settings.Configuration`. No behaviour change. Not in scope: AV0027 itself (#176, only suppressed), the missing `[Authorize]` on `UsersController` (P2).
> **Owner decisions** (2026-11-05, asked before the start and after the plan):
> 1. **Guard for the legacy projects: option A.** `TreatWarningsAsErrors` in `src/Directory.Build.props` as planned, plus `-warnaserror` on the solution build of CI stage 2, so a new warning in a legacy project also fails the pull request; local builds of the legacy projects stay lenient.
> 2. **NuGet audit warnings stay warnings (option A).** `NU1901`–`NU1904` are exempt from both (`WarningsNotAsErrors`, `-warnnotaserror`): a newly published advisory must not block unrelated pull requests; Dependabot reports it.
> 3. **Nullable convention** for the legacy fixes: `= null!` where the value is always set (entities, settings, DTOs), `?` only where the code already handles null; never `required` or `= string.Empty`, which would change model binding or the JSON the API returns.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | A Release build of `main` (6b591ac) shows **104** distinct warnings, not the 55 locations of `TECH_DEBT_v2.md` §3.3: 53 × `CS8618`, 22 other nullable warnings (`CS8603` 9, `CS8604` 4, `CS8600` 4, `CS8625` 3, `CS8622` 2, `CS8601`, `CS8602`), **22 × `VSTHRD200`** (not 2), `AV0027`, `CS0168`, `CS0105`, `CS1572`, `CS1573`. By project: `MODELS` 48, `API` 29, `SERVICES` 22, `DAL` 5; `src/` and `tests/` none. The S3-07 protocol (R5) had already counted 104 | About twice the documented size; the renames are mechanical, so the estimate held (P1 corrects the register) |
| F2 | `dotnet format analyzers` on each production project reports only `VSTHRD200` (15 in `API`, 7 in `SERVICES`) and `AV0027`; it does not report compiler warnings such as `CS8618` | The analyzer check alone cannot keep the build clean; stage 2 needs `-warnaserror` (decision 1) |
| F3 | The controllers use explicit route templates; no `[action]` token, `CreatedAtAction`, `nameof` or `ActionName` on an action; no test calls a renamed method | Adding `Async` to action names changes no URL (MVC also strips the suffix from action names) |
| F4 | MVC treats a non-nullable reference property of a request DTO as implicitly required (nullable is enabled in every project); EF treats it as a `NOT NULL` column or a required navigation | `string?` would drop the 400 for a missing field and change the EF model; `= null!` changes neither (decision 3) |
| F5 | `BaseService.ReadAsync`, `ProposalService` (`ReadAsync`, `ReadAllAsync`, `UpsertAsync`, `ReviewAsync`) return `null` as a gate, and `BaseCrudController.GetOneAsync` answers 204 on null | The honest fix is a nullable return type; it changes only annotations |
| F6 | `UsersController` has a stale doc comment of a commented-out `GetOneAsync` above `ChangeEmail` (`CS1572`, `CS1573`): the compiler attaches it to `ChangeEmail`, so the OpenAPI document shows "Gets a single record" as the summary of `POST /api/Users/change-email` | Removing the dead block removes the wrong summary; the only change in the OpenAPI document (check 5) |
| F7 | `UsersController` has no `[Authorize]`; `User.FindFirstValue(NameIdentifier)` can be null. A throwaway integration test (not committed): anonymous `DELETE /api/Users` and `POST /api/Users/revoke-sessions` answer 400 and change nothing | Kept as it is (`!`, no behaviour change); P2 |
| F8 | `NuGetAudit` is on; both `TreatWarningsAsErrors` and `-warnaserror` turn `NU1901`–`NU1904` into errors (checked on a scratch project with a vulnerable package); `WarningsNotAsErrors` and `-warnnotaserror` keep them warnings | Decision 2 |
| F9 | `Serilog.Settings.Configuration` in `SERVICES.csproj` is unused: no `ReadFrom.Configuration` anywhere (Platform reads `AppLoggingSettings` itself, S2-07) | Removed |

## 2. Changes

| File(s) | Change | Why |
|---|---|---|
| 25 files in `DAL/MODELS.ProcureAccess` (entities, DTOs, identity requests, `EmailSettings`, `AppSettings`, `TemporalViewModel`), 2 in `API/…/Swagger/Models` | `= null!` on the 53 `CS8618` properties | Decision 3, F4 |
| `SERVICES/…/DataServices/Interfaces/IBaseService.cs`, `Base/BaseService.cs`, `CriteriaFilterService.cs`, `CriterionService.cs`, `ProductService.cs`, `ProposalService.cs`, `IProposalService.cs`; `API/…/Controllers/Base/BaseCrudController.cs` | Nullable return types where the code returns null (F5); `Task<T>` is invariant, so every `ReadAllAsync` override follows the base. `ProposalService`: `string? userId`, `ProposerId = userId!` and `user!` in `ApproveAsync` keep today's behaviour | `CS8603`, `CS8600`, `CS8601`, `CS8602`, `CS8609`, `CS8613` |
| `DAL/…/Repos/Base/BaseRepo.cs`, `Repos/Interfaces/IBaseRepo.cs` | `FindAsNoTracking` / `FindIgnoreQueryFilters` return `T?` (they use `FirstOrDefault`); unused `ex` removed | `CS8603`, `CS0168` |
| `DAL/…/EFStructures/ApplicationDBContext.cs` | Event handlers take `object? sender` | `CS8622` |
| `API/…/ApiVersionSupport/ApiVersionConfiguration.cs`, `Controllers/UICustomizationController.cs` | `ApiVersion? defaultVersion = null`, `UICustomizationDto? dto = null` | `CS8625`, `CS8600` |
| `API/…/Controllers/Identity/UsersController.cs` | `FindFirstValue(…)!` (F7); dead doc block and commented-out method removed (F6) | `CS8604`, `CS1572`, `CS1573` |
| `SERVICES/…/Logging/AppLogging.cs` | The log actions get `[]` instead of `null` as their argument array: `FormattedLogValues` treats both alike (no formatting without values) | `CS8625` |
| `SERVICES/…/GlobalUsings.cs` | Duplicate `global using Microsoft.Extensions.Options` removed | `CS0105` |
| 22 methods in `API` and `SERVICES` | `Async` suffix: controller actions `GetOne`, `GetAll`, `ByCriteriaFilterIds` (2), `UpdateOne`, `Upsert`, `Review`, `Approve`, `ChangeEmail`, `ChangePassword`, `ResetPassword`, `RevokeSessions`, `DeleteUser`, `UICustomizationController.GetOne`; `AddRolesAndPermissions`; services `Read`, `ReadAll`, `GetByCriteriaFilterIds` (2), `GetCurrentUser`, `GenerateAuthResponse`, `ApplyApprovalSideEffects`, with their callers | `VSTHRD200`; F3 |
| `API/…/Extensions/SwaggerExtensions.cs` | `#pragma warning disable/restore AV0027` around the provider lookup, with a pointer to #176 | Row: AV0027 stays with #176 |
| `SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj` | `Serilog.Settings.Configuration` removed | F9 (S2-07 P3) |
| `src/Directory.Build.props` | `TreatWarningsAsErrors`; `WarningsNotAsErrors` `NU1901`–`NU1904` | Row (S2-06 P4); decision 2 |
| `.github/workflows/ci.yml` | Stage 1: `dotnet format whitespace`, `style` and `analyzers` on `ProcureAccess.sln` (the per-test-project loop is gone, the solution includes the tests); stage 2: `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | Row; decisions 1 and 2 |
| `docs/TESTING_v28.md` | V90: §8.2 stages 1 and 2 and the state after step 0 (replaces the analyzer part of V71); no invariant changes status | §8.2 described the old scope. The sprint plan lists S3-08 among the tasks without a TESTING version; S3-03 and S3-04 set the precedent of a version for a stage change without an invariant, and no other branch was writing one |
| `CLAUDE.md` | Header; §3 (no warnings, the guards, the nullable convention, AV0027); §5 (format and build commands as in CI) | DoD |

Not changed on purpose: `TECH_DEBT_v2.md` (count of F1, #118 done) and `ROADMAP_v3.md` M1.1d are updated in their next versions at the close-out (P1). The CI build stays a Debug build; the Release build was checked locally (check 1) and is the one of the API image.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff against `main`. N+1 and a11y: not applicable (no query and no UI changed). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Behaviour: the OpenAPI document of `Development` is identical before and after except the wrong summary of `change-email` (check 5); both EF models match their last migration (check 6); every backend test passes, including the Problem Details test that sends `{}` to `change-email` and expects `newEmail` in `errors`, which shows the implicit required check is kept | – |
| R2 | The `!` on the user id (`UsersController`, `ProposalService`) and on `user` in `ApproveAsync` silence the compiler instead of handling null | Won't fix: handling null would change behaviour (row: no behaviour change); the code goes with steps 3 (identity) and M5.6 (proposals). The missing `[Authorize]` behind it is P2 |
| R3 | `-warnaserror` also fails CI when an SDK or analyzer update (e.g. a Dependabot pull request) brings a new warning | Intended: the pull request shows it and fixes it; NuGet advisories are exempt (decision 2) |
| R4 | `AppLogging` passes `[]` instead of `null`: equal for `FormattedLogValues` (values without length → no formatting, message as given) | – |
| R5 | Guards verified with throwaway probes (removed): a `CS8618` in `API` fails the CI build command; a `CS8618` in `Modules.Platform` fails a plain local `dotnet build`; a `VSTHRD200` in `SERVICES` fails `dotnet format analyzers` (exit 2) | – |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -c Release --no-incremental` and the Debug build | ✅ 0 warnings, 0 errors (before: 104 distinct warnings) |
| 2 | CI stage 2 command `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings; fails with a probe warning (R5) |
| 3 | `dotnet format ProcureAccess.sln --verify-no-changes` (whitespace, style, analyzers on every project) | ✅ |
| 4 | `dotnet test --solution ProcureAccess.sln` (Docker running) | ✅ 184/184 |
| 5 | OpenAPI document `/swagger/v1/swagger.json` in `Development`, before (on `main`) and after, through a throwaway integration test (not committed), compared after sorting the keys | ✅ one line: the summary "Gets a single record" of `POST /api/Users/change-email` is gone (F6); paths, operations, parameters, schemas and `required` lists unchanged |
| 6 | `dotnet ef migrations has-pending-model-changes` for `ApplicationDBContext` and `PlatformDbContext` | ✅ no changes since the last migration (OPS-03) |
| 7 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v28.md`) |
| 8 | `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | ✅ 2 checked, 24 not built, 0 pending; 0 errors |
| 9 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 10 | Vitest, ESLint, E2E | – no client or E2E code touched; routes unchanged (check 5); E2E runs in CI |
| 11 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | `TECH_DEBT_v2.md` §3.3 counts 55 warning locations and 2 `VSTHRD200`; the build showed 104 and 22 (F1). #118 is closed by this PR; the register and `ROADMAP_v3.md` M1.1d record it in their next versions | Sprint 03 close-out (TECH_DEBT_v3) |
| P2 | **Proposed issue:** `UsersController` (`change-email`, `change-password`, `revoke-sessions`, `DELETE /api/Users`) has no `[Authorize]`; anonymous calls answer 400 instead of 401 and change nothing (F7). Add `[Authorize]` and a test, or leave it to step 3, which rebuilds the identity endpoints with cookie sessions | Owner: issue now or step 3 |
| P3 | Remove the `AV0027` suppression in `SwaggerExtensions.cs` with #176 (the issue's "done when: no AV0027 warning" now means: no suppression) | #176 |
| P4 | The legacy projects fail on a warning only in CI, not locally (decision 1). Once a project moves into `src/` (O-S2-1) it inherits `TreatWarningsAsErrors` | With each migration step |
| P5 | WebKit / mobile Safari only in CI | CI of this PR |

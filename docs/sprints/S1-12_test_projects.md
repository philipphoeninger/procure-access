# S1-12 — Root solution with backend test projects

> **Sprint:** 01, task S1-12 (milestone M0.3, TESTING §3.1, §3.4, §6.1, §9 step 0). **Status:** done by Claude Code (2026-10-23).
> **Scope:** the four test projects `tests/ProcureAccess.Tests.{Unit,Architecture,Integration,Shared}` in the root solution `ProcureAccess.sln`; packages xUnit v3, Shouldly, NSubstitute, NetArchTest; builders skeleton in `Shared`. Not in scope: Testcontainers, Respawn, `WebApplicationFactory`, test auth, `FakeTimeProvider` (S1-13); invariant tests (S1-14, S1-15); coverage (S1-20).
> **Owner decisions 2026-10-23:** **1** use Microsoft.Testing.Platform through `global.json`; empty test projects may exit with code 8 until their first test; **2** delete the leftover solution files (S1-03 §1) instead of keeping them until step 1 (TESTING §12.2); **3** builders skeleton as Claude Code sees fit (see §2).

## 1. Starting point

| # | Finding | Evidence |
|---|---|---|
| F1 | `ProcureAccess.sln` already holds the four production projects; there is no `tests/` folder | `dotnet sln list` |
| F2 | xUnit v3 (4.0.2) runs on Microsoft.Testing.Platform (MTP). The .NET 10 SDK refuses the VSTest mode with an error ("Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later"), also with `UseMicrosoftTestingPlatformRunner=false` | Probe project in the scratchpad |
| F3 | With MTP, `dotnet test` takes the solution only as `--solution ProcureAccess.sln`; the form `dotnet test ProcureAccess.sln` in the sprint's "Done when" and in CLAUDE §5 fails ("Specifying a solution for 'dotnet test' should be via '--solution'") | Probe project |
| F4 | MTP ends a run without tests with **exit code 8**, so an empty test project fails the run | Probe project |
| F5 | Two leftover solution files (`DAL/DAL.ProcureAccess.sln`, `SERVICES/SERVICES.ProcureAccess.sln`); nothing in the repository refers to them (the API Dockerfile restores the `.csproj` files directly) | `grep` |
| F6 | `bin/` and `obj/` are only ignored by the `.gitignore` of each production project | `git check-ignore` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `global.json` (new) | Only `"test": { "runner": "Microsoft.Testing.Platform" }`; **no SDK pin** | F2, decision 1. The installed SDK is still chosen as before |
| `tests/ProcureAccess.Tests.Unit/` | Test project: xunit.v3, Shouldly, NSubstitute → Shared, MODELS, SERVICES | TESTING §3.4: no I/O |
| `tests/ProcureAccess.Tests.Architecture/` | Test project: xunit.v3, Shouldly, NetArchTest.Rules → API, DAL, MODELS, SERVICES (the assemblies it inspects); `--ignore-exit-code 8` | F4; remove the setting with the first test (S1-14, ARC-06) |
| `tests/ProcureAccess.Tests.Integration/` | Test project: xunit.v3, Shouldly, NSubstitute → Shared, API; `--ignore-exit-code 8` | F4; remove the setting with the first test (S1-13) |
| `tests/ProcureAccess.Tests.Shared/` | Class library (`IsTestProject=false`) → MODELS. `Builders/Builder<T>` (base with `Build()` and an implicit conversion), `Builders/TestData` (entry point, used with `using static`, method names with article as in TESTING §6.1, fixed `DefaultCreatedAt`), `Builders/CriterionBuilder` (example for the existing `Criterion`) | Decision 3, see below |
| `tests/ProcureAccess.Tests.Unit/Builders/CriterionBuilderTests.cs` | Two tests: defaults are valid and deterministic; overrides change only the given values | Documents the builder convention; the Unit project needs no exit-code exception |
| `tests/.gitignore` (new) | `bin/`, `obj/`, `TestResults/` | F6; same convention as the production projects |
| `ProcureAccess.sln` | Four projects in a `tests` solution folder; only `Any CPU` configurations, CRLF kept | `dotnet sln add` rewrote the whole file (line endings, extra x64/x86 platforms), so the entries were added by hand: diff = 32 added lines |
| `DAL/DAL.ProcureAccess.sln`, `SERVICES/SERVICES.ProcureAccess.sln` | Deleted | F5, decision 2 |
| `CLAUDE.md` §3, §4, §5 | Current state, layout, command `dotnet test --solution ProcureAccess.sln` | CLAUDE §11 |

**Builders skeleton (decision 3).** Builders create objects in memory with fixed values (no clock, no random data, TESTING §3.1). Persisting them through the real `DbContext` (TESTING §6.1) needs the database fixture and is added in S1-13. `CriterionBuilder` is the only builder for now, because S1-15 (CAT-10, criteria filter) is the next task that needs criteria; builders for the catalog-release model replace it in step 2.

**Packages** (latest stable on nuget.org, 2026-10-23; licences from the nuspec):

| Package | Version | Licence | Note |
|---|---|---|---|
| xunit.v3 | 4.0.2 | Apache-2.0 ✅ | Brings Microsoft.Testing.Platform; no `Microsoft.NET.Test.Sdk` or `xunit.runner.visualstudio` needed (both only serve VSTest) |
| Shouldly | 4.3.0 | BSD-3-Clause ✅ | 5.0 is still a preview |
| NSubstitute | 6.2.0 | BSD-3-Clause ✅ | |
| NetArchTest.Rules | 1.3.2 | MIT ✅ (GitHub repository; the nuspec has no licence field) | Last release 2021-05; see open point P1 |

## 3. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; 136 warnings, unchanged from S1-11 and all in production projects; none from `tests/` |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ exit 0; Unit 2/2 passed, Architecture and Integration ran with 0 tests |
| 3 | Same run with a temporary failing test in the Integration project | ✅ exit 2 (the run fails; test removed again) |
| 4 | `dotnet format <project> --verify-no-changes` for the four test projects | ✅ no changes |
| 5 | `reuse lint` | ✅ compliant (REUSE 3.3). Note: while `tests/` was still untracked, `reuse` listed the ignored `bin/`/`obj/` files; after `git add` they are skipped |
| 6 | `npx ng test` (Karma, Node 24 from `.nvmrc`) | ❌ does not compile (`save-search-dialog.spec.ts` imports a missing `./update-user-dialog`): same as on `main` (S1-10, S1-11), client not touched; specs are replaced in S1-16 |
| 7 | `dotnet restore --force`: NuGet audit | ✅ no new `NU1xxx` warnings from the test packages |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | NetArchTest.Rules has had no release since 2021. If it cannot read the .NET 10 assemblies, that shows in S1-14 at the earliest, and in step 1 (module rules) for certain. Alternative: the maintained fork `NetArchTest.eNhancedEdition` (needs an architecture decision, CLAUDE §11) | Step 1 |
| P2 | TESTING §3.1 and the sprint's "Done when" say `dotnet test`; with xUnit v3 on the .NET 10 SDK this means Microsoft.Testing.Platform and `dotnet test --solution ProcureAccess.sln`. Record this in the next TESTING version, together with risk R4 (S1-21) | Next TESTING version |
| P3 | Remove `--ignore-exit-code 8` from the Architecture (S1-14) and Integration (S1-13) projects together with their first test | S1-13, S1-14 |
| P4 | S1-20 (CI): call `dotnet test --solution ProcureAccess.sln`; coverage with MTP needs the MTP coverage extension or coverlet's MTP integration instead of `coverlet.collector` (VSTest only) — check when S1-20 starts | S1-20 |

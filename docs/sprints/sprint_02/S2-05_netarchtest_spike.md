# S2-05 — NetArchTest spike on .NET 10

> **Sprint:** 02, P0 task S2-05 (debt, 0.5 points; TECH_DEBT_v1 TD-05, S1-12 open point P1). **Issues:** #117. **Requirements / invariants:** prepares ARC-01 – ARC-03 (TESTING_v15 §4.10); they stay `planned`. **Status:** done by Claude Code (2026-10-30), pull request open; spike code thrown away, this protocol is the only file of the PR.
> **Scope:** run real NetArchTest rules against the .NET 10 assemblies of the solution and record whether they load and evaluate correctly. The result decides how S2-08 is built. Not in scope: any production or test code change, the ARC-01 – ARC-03 tests themselves (S2-08), a new TESTING version.
> **Owner decisions:** none for this task. O-S2-2 (alternative tool) is **not needed**: the spike passed (§2).

**Result in one line:** `NetArchTest.Rules` 1.3.2 loads the .NET 10 assemblies and evaluates dependency, allow-list, inheritance and visibility rules correctly, framework assemblies included. S2-08 can build on it as planned. Four library limits (§2.3) shape how the rules must be written.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `tests/ProcureAccess.Tests.Architecture` references `NetArchTest.Rules` 1.3.2 (MIT, `netstandard2.0`, last release 2021-05) and transitively `Mono.Cecil` 0.11.3 since S1-12 | Nothing to add; the spike uses the existing reference |
| F2 | The project's only test is PRV-05 (S1-23), which uses reflection and the EF model, not NetArchTest. NetArchTest had never run against the solution | The open question of #117 was still open |
| F3 | ARC-01 – ARC-03 need four kinds of rule: forbidden dependencies (ARC-01, ARC-02), an allow-list of referenced `Contracts` (ARC-03), "only controllers and the module registration are public" (ARC-03, needs `NotBePublic` and `Inherit(ControllerBase)` across into the ASP.NET Core framework), and type-level checks such as "Export reads configurations only through `IConfigurationReader`" | The spike tests each kind on today's assemblies |
| F4 | Today's dependency graph: `MODELS` → nothing; `DAL` → `MODELS`; `SERVICES` → `DAL`, `MODELS`; `API` → all three. All four assemblies target `.NETCoreApp,Version=v10.0` | Real rules with known outcomes, both passing and failing |
| F5 | `dotnet list package --vulnerable --include-transitive` and `--deprecated` on the Architecture project: neither NetArchTest nor Mono.Cecil is flagged (only the known AutoMapper advisory, #31) | No security reason to replace the library |

## 2. Changes

None in the repository besides this protocol. The spike was a temporary file `tests/ProcureAccess.Tests.Architecture/Spike/` with 12 xUnit v3 tests (the probe types of §2.2 included), run locally with `dotnet test --project tests/ProcureAccess.Tests.Architecture/ProcureAccess.Tests.Architecture.csproj` (SDK 10.0.113, Debug build, as CI stage 4 builds), then deleted. A second, smaller probe checked how namespace names are matched (§2.2). The whole run took about 2 s.

### 2.1 Rules against the solution's assemblies

| # | Rule | Expected | Result |
|---|---|---|---|
| A1 | `MODELS` should not have a dependency on any of `DAL.ProcureAccess`, `SERVICES.ProcureAccess`, `API.ProcureAccess` (shape of ARC-01) | pass | ✅ pass |
| A2 | `SERVICES` should not have a dependency on `API.ProcureAccess` (shape of ARC-02) | pass | ✅ pass |
| A3 | `SERVICES` should not have a dependency on `DAL.ProcureAccess` (deliberately wrong) | fail, with types | ✅ fails, 11 types (services, `BaseService<,>`, `IBaseService<,>`, `LogRetentionJob`, `DataServiceConfiguration`) |
| A4 | `API` should not have a dependency on `SERVICES.ProcureAccess` (deliberately wrong) | fail, with types | ✅ fails, 10 types, among them `Program` (top-level statements) and the static `IdentityUserEndpoints`, so references that appear only inside method bodies are found too |
| B1 | Classes in `API.ProcureAccess.Controllers` whose name ends in `Controller` should inherit `ControllerBase` | pass | ✅ pass, 8 controllers, including the 5 that inherit through the generic `BaseCrudController<,,>` and `HealthController` with a primary constructor |
| B2 | Types inheriting `ControllerBase`: API vs SERVICES | 9 / 0 | ✅ 9 (8 controllers + `BaseCrudController<,,>`) / 0 |
| B3 | Types in `SERVICES` implementing `IHostedService`, compared with reflection | `LogRetentionService` | ❌ **none found**; reflection finds `LogRetentionService : BackgroundService`. Explained in §2.2 (L1): a limit of the library, not of .NET 10 |
| C1 | Types in `SERVICES` outside `DataServices.Interfaces` should not be public (shape of ARC-03 visibility) | fail, with types | ✅ fails, 25 public types listed |
| C2a | `MODELS` should only have dependencies on `MODELS.ProcureAccess`, `System`, `Microsoft` (allow-list shape of ARC-03) | not known beforehand | Fails on 3 types (`MappingProfile` and two converters) because they use `AutoMapper`. Correct: AutoMapper is not on the list (L3) |
| C2b | `SERVICES` allow-list without `DAL.ProcureAccess` | fail, with types | ✅ fails, 14 types |
| D1/D2 | Types NetArchTest reads per assembly, compared with reflection | nothing missing | ✅ nothing missing. NetArchTest also lists compiler-generated types (`<>f__AnonymousType…`, `<PrivateImplementationDetails>`, `<>z__ReadOnlySingleElementList`1`) (L4) |

### 2.2 Probe types (in the test assembly, to separate library behaviour from .NET 10 behaviour)

| # | Probe | Result | Reading |
|---|---|---|---|
| P-a | `Inherit(ControllerBase)` on `class X : Controller` (`Controller` lives in a different framework assembly than `ControllerBase`) | ✅ found | Mono.Cecil resolves base types across the ASP.NET Core shared framework on .NET 10 |
| P-b | `Inherit(BackgroundService)` on `class X : BackgroundService` | ✅ found | Same |
| P-c | `ImplementInterface(IHostedService)` on a class that declares it directly / on a `BackgroundService` subclass | found / **not found** | L1 |
| P-d | `ImplementInterface(ILocalMarker)` on `LocalBase : ILocalMarker` and `ViaLocalBase : LocalBase`, all in one assembly | `LocalBase` found, `ViaLocalBase` **not found** | L1 holds without any framework type, so it is not a .NET 10 resolution problem |
| P-e | `ImplementInterface(IBaseService<,>)` (open generic) on `SERVICES` | ✅ finds the services that declare `IXService : IBaseService<,>` and `BaseService<,>` | Open generics and interfaces inherited from interfaces work |
| P-f | `HaveDependencyOn("MODELS.ProcureAccess")` on a `record` with a positional parameter and on a class with a primary constructor | ✅ both found | Records (C# 9) and primary constructors (C# 12) are read |
| P-g | Namespaces `Probe.Mod`, `Probe.Mod.Contracts`, `Probe.ModX`: `HaveDependencyOn("Probe.Mod")` | matches users of `Probe.Mod` **and** `Probe.Mod.Contracts`, not `Probe.ModX` | L2: whole-segment prefix match |
| P-h | Same namespaces: `ResideInNamespace("Probe.Mod")` | selects types of `Probe.Mod`, `Probe.Mod.Contracts` **and `Probe.ModX`** | L2: plain string prefix, not segment-aware |
| P-i | `OnlyHaveDependenciesOn("System", "Probe.Users", "Probe.Mod.Contracts")` on users of all three | fails exactly on the users of `Probe.Mod` and `Probe.ModX` | Allow-list works for "Contracts yes, internals no" |

### 2.3 Limits S2-08 must design around

| # | Limit | How S2-08 handles it |
|---|---|---|
| L1 | `ImplementInterface` / `NotImplementInterface` see only interfaces the type declares itself (plus their base interfaces), not those inherited from a base class | Do not use it where the interface can come through a base class; use `Inherit`, a `MeetCustomRule` with `Type.IsAssignableFrom`, or plain reflection. ARC-01 – ARC-03 are dependency and visibility rules and do not need it |
| L2 | `HaveDependencyOn("X")` also matches `X.Contracts`, so a deny rule on a module namespace also forbids its allowed `Contracts`. `ResideInNamespace("X")` also selects `XY` | ARC-03: write "references only the listed `Contracts`" as an `OnlyHaveDependenciesOn` allow-list (P-i), or deny the module's internal sub-namespaces; select modules with `ResideInNamespaceMatching(@"^ProcureAccess\.Modules\.Catalog(\.|$)")` or an exact `Contracts` exclusion. Each rule keeps a deliberate-violation check (SPRINT_02 S2-08 acceptance) so a too-wide match shows up |
| L3 | An allow-list must name every third-party namespace a module uses (`AutoMapper`, `Serilog`, `Npgsql` …), see C2a | Keep a shared list of allowed framework and package namespaces in one place of the test project; a new package then fails the rule until it is added, which is the intended review point |
| L4 | NetArchTest lists compiler-generated types (anonymous types, `<PrivateImplementationDetails>`) | Harmless for dependency rules today; for visibility rules filter with `DoNotHaveNameStartingWith("<")` if one ever shows up |
| L5 | `NetArchTest.Rules.TestResult` clashes with `Xunit.TestResult` (xUnit v3), which is visible in every test file | `using TestResult = NetArchTest.Rules.TestResult;` or `var` |
| L6 | No `Slice()` / "no dependencies between slices" in 1.3.2 (that API is only in forks) | Not needed: S2-08 writes one rule per row of ARCHITECTURE §4.3 anyway |

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security, privacy, a11y, N+1: not touched (no code in the PR) | – |
| R2 | The spike ran locally only, not in CI stage 4 as #117 proposed. CI builds the same Debug configuration on the same SDK major on Linux, so the result carries over; S2-08 puts the first NetArchTest rules into stage 4 | Won't fix: the task asked to throw the spike code away |
| R3 | The rules ran against legacy assemblies, not module projects. The rule kinds and the matching behaviour (§2.2) do not depend on the project layout | – |
| R4 | B3 "failed" in the spike run; it is the expected behaviour of L1, not a defect to fix | – |
| R5 | `--ignore-exit-code 8`, which SPRINT_02 S2-08 and the success criteria want removed, was already removed in S1-23 (first PRV-05 test; S1-23 F5) | Follow-up (P2) |

No correctness or security finding; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | Spike: 12 tests against `API`, `SERVICES`, `DAL`, `MODELS` (.NET 10) | ✅ loaded and evaluated; 11 as expected, B3 explained by L1 |
| 2 | Probe: namespace matching (P-g – P-i) | ✅ 1 probe test + 3 PRV-05 tests passed |
| 3 | Architecture test project after deleting the spike: `dotnet test --project tests/ProcureAccess.Tests.Architecture/ProcureAccess.Tests.Architecture.csproj` | ✅ 3/3 (PRV-05), `git status` without spike files |
| 4 | `dotnet list package --vulnerable --include-transitive` / `--deprecated` (Architecture project) | ✅ NetArchTest and Mono.Cecil not flagged (AutoMapper #31 known) |
| 5 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 6 | `node scripts/check-figma-links.mjs` | ✅ unchanged |
| 7 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant (`docs/**` covered by `REUSE.toml`) |
| 8 | Build, other test levels, client, E2E | – not touched (documentation only) |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | #117 asks to note the result "in the next TESTING version". This PR adds no TESTING version (documentation-only spike); S2-08 creates the next one when it flips ARC-01 – ARC-03 and should add to §3.1 (NetArchTest row): "1.3.2 verified on .NET 10 (S2-05); limits L1, L2" | S2-08 |
| P2 | SPRINT_02 (S2-08 row, success criteria), TASK_PROMPTS (S2-08) and TECH_DEBT_v1 (§5 row, M1.1 row) still say `--ignore-exit-code 8` has to be removed; it is gone since S1-23. S2-08 only needs to confirm it; the sprint close-out and TECH_DEBT_v2 should drop the item | S2-08; sprint 02 close-out |
| P3 | NetArchTest stays unmaintained (no release since 2021). Today that costs nothing; if a future SDK changes the metadata format, the fork `NetArchTest.eNhancedEdition` is the fallback (needs an ADR, CLAUDE §11). The ARC tests themselves would show it, because each rule has a deliberate-violation check | Watch; no issue proposed |

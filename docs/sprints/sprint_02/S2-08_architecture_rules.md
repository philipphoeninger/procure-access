# S2-08 — Architecture tests ARC-01 – ARC-03

> **Sprint:** 02, P0 task S2-08 (roadmap M1.1, 3 points; ARCHITECTURE_v15 §4.3; closes S2-05 open point P1, S2-06 open point P2 and S2-07 open point P1). **Issues:** none. **Requirements / invariants:** ARC-01, ARC-02, ARC-03 → `active` in `TESTING_v16.md` (V73 – V75). **Status:** done by Claude Code (2026-10-30), pull request open.
> **Scope:** one rule per row of the module table, checked on the project references and with NetArchTest, plus the visibility rule of ARC-03; deliberate-violation runs; confirmation that the architecture test project runs without `--ignore-exit-code 8`. Not in scope: the `AdHocSelectionSource` part of ARC-03 (step 2, decision 2), ARC-04 – ARC-07 (later steps), analyzers on production projects (#118).
> **Owner decisions 2026-10-30:** **S2-05 result:** NetArchTest works on .NET 10, so O-S2-2 is not needed. **D1:** a `Contracts` project may not reference another module's `Contracts` project while O-22 is open; the test forbids it, with a pointer to O-22. **D2:** the `AdHocSelectionSource` rule is written with that type in step 2; ARC-03 is `active` now and TESTING records the deferred part (rejected: a placeholder test that passes without checking anything). **D3:** transitive references are handled by the test (it follows the `.csproj` graph), not by `DisableTransitiveProjectReferences` (rejected: changes how every project builds). **D4:** contracts projects reference no project at all, `Platform.Contracts` included (O-22 covers it too). **D5:** the reverse rule (only the host `API` and the test projects reference a module project, C94) is part of ARC-03. Also: the owner's local files (`TASK_PROMPT.md` → `TASK_PROMPTS.md`, `.ai/`, the deleted `ARCHITECTURE_v13.md`) stay out of this pull request.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The architecture test project already references NetArchTest.Rules 1.3.2 (S1-23) and holds only the PRV-05 tests | No new dependency |
| F2 | The 16 module projects under `src/` match the table of ARCHITECTURE_v15 §4.3 exactly; only Platform has code, the other modules hold just `<Name>Module`, all contracts except `Platform.Contracts` are empty | The type rules pass today without anything to find; the project-reference check is the real check, and self-tests show the type checks can fail |
| F3 | The compiler drops references to projects whose types are not used (S2-06 R2) | Project references are read from the `.csproj` files, not from the assembly metadata |
| F4 | NetArchTest matches dependencies by name prefix (S2-05 L2): `ProcureAccess.Modules.Directory` also matches `ProcureAccess.Modules.Directory.Contracts` | The type rule names the forbidden types (all top-level types of each forbidden assembly in its own namespace), not a namespace |
| F5 | Every assembly carries compiler-embedded types with the same names (`NullableAttribute`, `NullableContextAttribute`, `RefSafetyRulesAttribute`, `<PrivateImplementationDetails>`) | Forbidden types are taken from the assembly's own namespace only |
| F6 | CI stage 4 runs with coverlet, which instruments the module assemblies on disk | Checked with the exact CI command (check 3): the rules see the instrumented assemblies and pass; coverlet's tracker types are not public, so the visibility rule needs no exception |
| F7 | `--ignore-exit-code 8` is not in `ci.yml` (removed in S1-23) | Confirmed only (check 4) |
| F8 | The prompt referenced ARCHITECTURE_v14; v15 is the highest | Worked from v15; the owner fixed `TASK_PROMPTS.md` (local file) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `tests/…Architecture/Modules/ModuleMap.cs` (new) | The module table of ARCHITECTURE §4.3 in one place: per module the allowed contracts; own contracts and `Platform.Contracts` added for every row; host, test-project prefix and legacy projects | One rule per row; one place to change with the table |
| `tests/…Architecture/Modules/ProjectGraph.cs` (new) | Reads the projects of `ProcureAccess.sln` and the `ProjectReference` items of their `.csproj` files (`System.Xml.Linq`); direct references, all references (transitive) and referrers | F3; D3 |
| `tests/…Architecture/Modules/ModuleRules.cs` (new) | `TypesUsingAny` (NetArchTest `ShouldNot().HaveDependencyOnAny(<forbidden type names>)`) and `UnexpectedPublicTypes` (reflection on exported types: only the static `<Name>Module` and `ControllerBase` subclasses) | F4, F5 |
| `tests/…Architecture/Modules/ModuleReferenceTests.cs` (new) | **INV:ARC-03**: the projects under `src/` are exactly the table; per row: project references (transitive) only the allowed ones; contracts reference nothing (D1, D4); only host and tests reference the module project (D5); types use no type of a forbidden assembly; contracts types use no other project | ARC-03 |
| `tests/…Architecture/Modules/ModuleVisibilityTests.cs` (new) | **INV:ARC-03** per row: the registration class exists, nothing else is public except controllers; a generated `public` migration fails (S2-07 P1) | ARC-03 |
| `tests/…Architecture/Modules/ModuleBoundaryTests.cs` (new) | **INV:ARC-01** (Catalog and its contracts reference no other module except `Platform.Contracts`; types likewise) and **INV:ARC-02** (Workspace never sees Directory internals, Directory never sees Workspace or its contracts; projects and types). Both name their projects themselves, so a wrong edit of `ModuleMap` cannot weaken them | ARC-01, ARC-02 |
| `tests/…Architecture/Modules/RuleSelfTests.cs`, `Probes/RuleProbes.cs` (new) | Five self-tests on probe types and a made-up graph: a use of Platform internals is reported, a use of `Platform.Contracts` only when it is forbidden; only the stray public type is reported (registration class, controller and a public type nested in an internal one are not); a non-static class with the registration name is reported; a wrong reference through a contracts project shows in the transitive references | F2; S2-05 P3 |
| `docs/TESTING_v16.md` (new) | Copy of v15: status line; input documents (module rules follow ARCHITECTURE_v15 §4.3); "Changes from v15" V73 – V75; ARC-01 – ARC-03 `active`; ARC-03 text as built (`Platform.Contracts`, transitive, contracts reference nothing, reverse rule, `AdHocSelectionSource` part from step 2); §3.1 NetArchTest row (S2-05 result, L1, L2); §9 step 2 names the `AdHocSelectionSource` rule | DoD; S2-05 P1, S2-06 P2 |
| `CLAUDE.md` | Header; §3 state (S2-08); §4 layout (`Architecture/Modules/`); §7 contracts reference no other project, reverse rule, change `ModuleMap.cs` with the table | DoD |
| `docs/sprints/sprint_02/S2-05_netarchtest_spike.md`, `S2-06_module_skeleton.md`, `S2-07_platform_module.md` | S2-05 P1, S2-06 P2 and S2-07 P1 marked done | Traceability |

**Deviation from the plan.** The plan named a namespace allow-list (`OnlyHaveDependenciesOn`) with a shared list of framework and package namespaces (S2-05 L3). The rules instead deny the types of every forbidden assembly. This needs no package list to maintain, cannot be broken by namespaces a tool adds (F6), and is exact where the namespace prefix is not (F4). What the allow-list would have added, a review point for new packages, is not part of ARC-03; the project-reference check already rejects every project outside the table.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; plus the question whether a rule can pass without checking anything. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security, privacy, N+1, a11y: test code only; no endpoint, query, UI or secret touched | – |
| R2 | ARC-01 compared Catalog's references with the exact set {`Catalog.Contracts`, `Platform.Contracts`}: Catalog dropping its `Platform.Contracts` reference would have failed ARC-01 without a violation | Fixed: "nothing beyond these two" |
| R3 | Vacuous pass: if the solution file moves to `.slnx` or is not found, the graph would be empty. The `src/` test then fails (no projects vs. 16 expected) and every per-row test throws on the unknown project, so nothing passes silently | – |
| R4 | A `ProjectReference` put into a `Directory.Build.props`/`.targets` instead of a `.csproj` is not read by `ProjectGraph`. The type rules would still catch any use of the referenced types | Won't fix: no project file does this; the type rule covers it |
| R5 | The type rule selects the module's types by namespace prefix; a type declared in a foreign namespace inside a module project would escape it. The project-reference check still blocks the reference itself | Won't fix: such a namespace would be a review finding on its own |
| R6 | The forbidden type names are matched by prefix, so a forbidden `…Platform.PlatformModule` would also match an allowed type `…Platform.PlatformModuleX` in another assembly. Module types and contracts types live in different namespaces (`<Name>` vs. `<Name>.Contracts`) | Won't fix: cannot occur with the layout of §4.3 |

No other correctness or security finding; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build` of the architecture test project (pulls in the whole solution) | ✅ 0 errors; the warnings are the legacy ones (#118) |
| 2 | `dotnet test --project tests/ProcureAccess.Tests.Architecture` | ✅ 59/59 (PRV-05 3, ARC-01 1, ARC-02 1, ARC-03 49 cases, self-tests 5) |
| 3 | The same with the CI stage-4 command (`--coverlet` with all `--coverlet-include` filters of `ci.yml`) | ✅ 59/59; cobertura report lists all 20 production assemblies, so the rules ran on instrumented assemblies (F6) |
| 4 | `--ignore-exit-code 8` | ✅ not in `ci.yml`; the project exits 0 (S1-23; S2-05 P2 confirmed) |
| 5 | Deliberate violations (§4.1) | ✅ every case fails the expected tests; after the reverts 59/59 and no stray file |
| 6 | `dotnet format whitespace` / `style` on the solution; `dotnet format` on the architecture test project (analyzers) | ✅ no changes (one AV0014 on the probe controller fixed with `[ApiController]`) |
| 7 | `dotnet test --solution ProcureAccess.sln` | ✅ 122/122 (Unit, Architecture, Integration incl. OPS-01/02/03, SEC-06, CAT-10) |
| 8 | `node scripts/check-invariants.mjs` | ✅ passed; ARC-01 1, ARC-02 1, ARC-03 7 tagged tests |
| 9 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 10 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 586/586 |
| 11 | Client build, Vitest, ESLint, E2E | – not touched |

### 4.1 Deliberate violations

Run by a script in the session scratchpad, one case at a time: change, `dotnet test --project tests/ProcureAccess.Tests.Architecture`, revert (`git checkout -- src SERVICES ProcureAccess.sln`, `git clean -fd src`). "Type used" means the case also adds a class that uses a type of the wrongly referenced project, so the NetArchTest rule has something to find; for empty contracts the case first adds a public interface there. Nothing of it is committed. The runs were made before the fix of R2, which only widens the Catalog check from "exactly" to "at most" these references; C1, C9 and C10 still fail ARC-01 with it (both checks reject the wrong reference).

| Case | Change | Failing tests (of 59) |
|---|---|---|
| C1 | `Catalog` → `Workspace` (module project), type used | 4: ARC-01; ARC-03 references (Catalog), referrers (Workspace), types (Catalog) |
| C2 | `Configurator` → `MODELS.ProcureAccess` (legacy), type used | 2: ARC-03 references, types (Configurator) |
| C3 | `Workspace` → `Directory` (module project), type used | 4: ARC-02; ARC-03 references (Workspace), referrers (Directory), types (Workspace) |
| C4 | `Directory` → `Workspace.Contracts` (not listed), type used | 3: ARC-02; ARC-03 references, types (Directory) |
| C5 | `Moderation` → `Catalog.Contracts` (not listed), type used | 2: ARC-03 references, types (Moderation) |
| C6 | `Export` → `Directory.Contracts` (not listed), type used | 2: ARC-03 references, types (Export) |
| C7 | `IdentityAccess` → `Platform` (module project instead of `Platform.Contracts`), type used | 3: ARC-03 references (IdentityAccess), referrers (Platform), types (IdentityAccess) |
| C8 | `Platform` → `IdentityAccess.Contracts` (not listed), type used | 2: ARC-03 references, types (Platform) |
| C9 | `Catalog.Contracts` → `Platform.Contracts` (D4), type used | 3: ARC-01; ARC-03 contracts references, contracts types (Catalog) |
| C10 | `Catalog.Contracts` → `Workspace.Contracts`, reference only | 7: ARC-01; ARC-02 (Directory now sees `Workspace.Contracts` transitively); ARC-03 contracts references (Catalog), references of Catalog, Configurator, Directory, Export (all reach it through `Catalog.Contracts`); no type rule fails, because no type is used |
| C11 | `SERVICES` (legacy) → `Platform` (module project), reference only | 1: ARC-03 referrers (Platform) |
| C12 | `Catalog`: a public class that is neither registration nor controller | 1: ARC-03 visibility (Catalog) |
| C13 | `Platform`: `InitialPlatformSchema` made `public`, as EF generates it | 1: ARC-03 visibility (Platform) |
| C14 | New project `src/ProcureAccess.Modules.Probe` added to the solution | 1: ARC-03 projects under `src/` |
| – | After all reverts | 0 failed, 59/59; `git status` shows only this task's files and the owner's local files |

Each row of the table therefore fails on a wrong reference (C1 – C8), the contracts rule on its own (C9), a reference that only arrives transitively (C10), the reverse rule (C11), the visibility rule for a stray type and for a generated migration (C12, C13) and the table itself (C14). NetArchTest fails together with the project check in every case that uses a type; that it can fail on its own is shown by the self-tests.

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | ARCHITECTURE §16.5 O-22 could record the interim rule (D1, D4: contracts reference nothing until decided) with the next ARCHITECTURE version; today it is in TESTING_v16 (V74) and CLAUDE §7 | Next ARCHITECTURE version |
| P2 | The `AdHocSelectionSource` rule of ARC-03 (D2) | Step 2 (export pipeline), TESTING §9 |
| P3 | SPRINT_02 (S2-08 row, success criteria), TASK_PROMPTS (S2-08) and TECH_DEBT_v1 still mention removing `--ignore-exit-code 8`; it is gone since S1-23 (check 4) | Sprint 02 close-out, TECH_DEBT_v2 (S2-05 P2) |
| P4 | When a module gets its first contracts type that needs another module's type, O-22 has to be decided before the test can allow it; the test message points there | Owner, step 2 or 3 |

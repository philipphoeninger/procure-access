# S3-07 — AutoMapper replacement with the generic CRUD maps and the ARC-06 successor

> **Sprint:** 03, P0 task S3-07 (debt, 3 points, two runs). **Issues:** #31 (`TECH_DEBT_v2.md` §3.2, Dependabot alerts 1, 8, 9, 10), #90 (`TECH_DEBT_v2.md` §3.1, S2-10 P4). **Requirements / invariants:** ARC-06 is replaced by its successor in the next TESTING version (run 2). **Status:** run 1 done by Claude Code (2026-11-05): options as a draft ADR, no code changed; O-S3-4 decided the same day. Run 2 follows the separate pull request of decision (d).
> **Scope:** run 1 = options only (sprint row, `TASK_PROMPTS.md`). Run 2 = replace every AutoMapper map, fix #90 with tests first, remove the package and the advisory suppression, ADR in the next ARCHITECTURE version, ARC-06 successor in the next TESTING version.
> **Owner decisions (2026-11-05, after run 1; all as proposed in §2.5):** **O-S3-4 (a)** option A, hand-written mapping, no new package. **(b)** ARC-06 keeps its ID with the S1 wording; the #90 round trips are regular tests. **(c)** option (ii): the generic `POST`/`PUT`/`DELETE` of the four catalog controllers and of `ProposalsController` are removed. **(d)** the removal goes into a separate small pull request before run 2.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `AutoMapper` 14.0.0 is referenced directly by all four legacy projects (`API`, `DAL`, `MODELS`, `SERVICES`); `Directory.Build.props` suppresses GHSA-rvv3-g6hj-g44x with a link to #31. The module projects under `src/` and the test projects except `Tests.Unit` don't use it | Run 2 removes four `PackageReference`s, three `global using`s and the suppression |
| F2 | One `MappingProfile` (`DAL/MODELS.ProcureAccess/Entities/Mapping/`): 10 `CreateMap` (5 entity→DTO, `CriteriaFilter→LookupDto`, `User→UserDto`, `CreateProductDto→Product`, `CreateCriterionDto→Criterion`, `UICustomization↔UICustomizationDto` with `ReverseMap`), two `IValueConverter`s for `Language` ↔ language code, two `Select` over join entities (`ProductCriteriaFilters → CriteriaFilters`, `Exclusions → Exclusions`) | Every option must cover the two join-entity maps and the language converters; they are the only non-trivial parts |
| F3 | Call sites: `Mapper.Map` in `BaseService` (4×), `ProposalService` (3×), `UserService` (2×), `UICustomizationRepo` (2×); `ProjectTo` in `CriterionService` (2×), `CriteriaFilterService` (1×), `ProductService` (2×); `AddAutoMapper` in `Program.cs`; `IMapper` in the constructors of 7 services, 1 repository and `IBaseService` | 11 `Map` + 5 `ProjectTo` call sites, about 15 maps in all once #90's maps are added |
| F4 | Mapped types are small: 15 DTOs with about 45 properties in all | Hand-written mapping is about 250 lines; no option has a large edit |
| F5 | **#90, create:** `BaseService.Create` maps `TDto→TEntity`, but no such map exists for `CriterionDto`, `ProductDto`, `FilterTypeDto`, `CriteriaFilterDto`, `ProposalDto`, so every generic `POST` answers 400 (logged at Error). Also: `AddOne` answers `CreatedAtAction` with the *posted* `dto.Id` (0), not the new id | Run 2 fixes the location too, if the endpoints stay (decision c) |
| F6 | **#90, update and delete:** `BaseService.Update` saves the loaded entity unchanged; `UpdateOne` returns 200 with the posted DTO even for an unknown id; `DeleteOne` returns 200 with body `0` for an unknown id | Target per the task: 404 Problem Details `not-found` |
| F7 | **The generic write endpoints accept anonymous requests.** `CriteriaController`, `ProductsController`, `FilterTypesController` and `CriteriaFiltersController` have no `[Authorize]`, `BaseCrudController` has none, `FallbackPolicy = null` (`IdentityExtensions.cs`) and `MapControllers()` adds no policy. Checked in a throwaway integration test (not committed) on the real PostgreSQL fixture: an anonymous `DELETE /api/FilterTypes/{id}` answered **200, body `1`, and removed the row**. `BaseRepo.Delete` is a hard delete. Once #90 makes create and update work, anonymous users could also create and change catalog records. `ProposalsController` has `[Authorize]`, but any signed-in user can then use its generic `PUT`/`DELETE` on any proposal | **Security bug that affects behaviour today** (CLAUDE §6.8 *fix*). It is outside this row's scope, but it decides how #90 is fixed: decisions (c) and (d) |
| F8 | The client calls none of the generic `POST`/`PUT`/`DELETE` endpoints (only `GET`s, `Proposals/upsert`, `review`, `approve`). SPEC SD20 / R-ADM-01: the catalog changes only through data files and the CLI import, **no catalog-editor UI**. D11 keeps generic CRUD "only for catalog admin", but SD20 leaves catalog administration without any HTTP writes | Making the generic writes work (the task's run-2 instruction) and removing them (#90's own suggestion: "decide per controller whether create/update is needed at all") both fit a document; that's a conflict for the owner (CLAUDE §2), decision (c) |
| F9 | ARC-06 (`MappingProfileTests`, U, step 0) calls `AssertConfigurationIsValid`; it only checks maps that exist (#90). TESTING_v26 §5.5 still says "Individual AutoMapper maps (covered by ARC-06 plus the contract tests)" | The successor must also catch a *missing* map or a forgotten member, and §5.5 is reworded in run 2 |
| F10 | Lifetime of the code: the legacy catalog entities (`Criterion`, `CriteriaFilter`, `FilterType`) are replaced by the catalog module in M2 (S3-11 – S3-20); the proposal workflow retires in M5.6 (`withProposal()`, ARCHITECTURE §8.1); `Product` with the directory (step 6); `User`/`UICustomization` with identity and settings (steps 3/4). New module endpoints are hand-written with their invariants (D11) | The replacement mostly serves code that goes away within the next steps; what the new modules use matters more than the legacy maps |

## 2. Draft ADR-015 — Replace AutoMapper

*Draft for O-S3-4. Goes into the next ARCHITECTURE version (§14 row, D-table row, a `C` entry) in run 2, with the owner's choice.*

### 2.1 Context

AutoMapper 15+ is RPL-1.5 or commercial and fails D12; 14.0.0, the last MIT version, carries the unpatched High advisory GHSA-rvv3-g6hj-g44x (#31). The profile is small (F2–F4) and the generic CRUD maps are incomplete (#90). ARC-06 can't see missing maps (F9).

### 2.2 Options

| | A — Hand-written mapping | B — Mapperly (source generator) | C — Mapster | D — Facet |
|---|---|---|---|---|
| **Package** | none | `Riok.Mapperly` 4.3.1 (2025-12-22; repository active, last commit 2026-10-29) | `Mapster` 10.0.14 (2026-11-02) | `Facet` 6.7.0 (2026-10-30; first release 2025-04) |
| **Licence vs. D12** | n/a | **Apache-2.0** ✅. Generator runs at build time; shipped is only `Riok.Mapperly.Abstractions.dll` (attributes, Apache-2.0) | **MIT** ✅ | **MIT** ✅ |
| **How it works** | Static extension methods per entity in `MODELS`: `ToDto()`, `ToEntity()`, `ApplyTo(entity)` for updates, and an `Expression<Func<TEntity, TDto>>` for queries (`.Select(ProductMapping.ToDto)` replaces `ProjectTo`) | `[Mapper] partial class`; the generator writes the code at compile time; `IQueryable` projections, mapping onto an existing object, user methods for special members | Runtime mapping like AutoMapper (`Adapt`, `ProjectToType`), configuration validated at runtime (`Compile()`) | Generates DTO types *from* entities |
| **Existing profile (F2)** | 1:1, the two `Select`s and the language switch are plain C# | Spike (scratchpad, not committed) on copies of `Product`, `ProductCriteriaFilter`, `UICustomization`: the `Select` through the join entity is inlined into the query expression, `ApplyTo` maps onto a loaded entity and keeps `CreatedAt`, the language helpers work in both directions | Closest to today's code (map names change, configuration similar) | Doesn't fit: the DTOs exist with their validation attributes and different shapes |
| **#90 maps (5× DTO→entity + update)** | 5 `ToEntity` + 5 `ApplyTo` written out | 5 + 5 partial methods with ignore attributes | 5 configurations | — |
| **Forgotten member** | Not visible to the compiler → needs the test of §2.3 S1 | **Compile error** with `RMG012` (target member without source) and `RMG020` (source member not mapped) set to `error` in `.editorconfig`; checked in the spike: a new DTO property fails the build | Only at runtime/in a validation test, like today | — |
| **Effort run 2** (incl. #90 tests first, docs) | ≈ 2.5 points | ≈ 2.5 points (+ licence approval, `.editorconfig` severities, attributes for ignored members) | ≈ 2 points | not viable |
| **For handover** | Plain C#, nothing to learn; more lines | One more tool with its own attributes and diagnostics; generated code in `obj/` | Same model as AutoMapper | — |
| **Risk** | Forgotten member caught only by the test | Mainly one maintainer (`latonz`); a new dependency for code that mostly retires (F10) | Same class of tool and risk as AutoMapper (runtime reflection; licence of a popular mapper can change, as AutoMapper's did) | young, one maintainer |

Also rejected: **stay on AutoMapper 14** (High advisory without a fix), **buy the commercial AutoMapper licence** (D12: a publicly funded open-source release must not depend on a commercial licence).

### 2.3 What replaces ARC-06

| | Successor | Fits | Catches a missing map / forgotten member? |
|---|---|---|---|
| **S1** | **ARC-06 reworded:** "Every mapping between an entity and its DTO sets every member of its target or names it as ignored." Unit test: for each map, fill the source with non-default values for every settable member (one element in collections), map, and assert every target member is set, except an explicit ignore list per map (`Id`, `CreatedAt`, `xmin`, `IsDeleted`, navigations) | A (needed), B (redundant with the compiler, but cheap) | Yes, for a forgotten member; a missing map can't exist because every call site calls a method |
| **S2** | Compiler only: `RMG012`/`RMG020` as `error`; ARC-06 is retired. The gate needs a test per active invariant, so the invariant would be dropped, not replaced | B only | Yes, at build time |
| **S3** | Endpoint level: per generic CRUD controller an integration round trip `POST → GET → PUT → GET → DELETE → GET` and unknown ids → 404 `not-found` | A, B — only if the endpoints stay (decision c) | Yes for the maps these endpoints use, not for `User`/`UICustomization`/`Proposal` |

S3 is what #90 needs in any case (tests first); the question is only whether it is an invariant or a regular test. Because the generic endpoints are legacy (ARC-05 from step 5, D11), Claude recommends regular tests for S3 and keeping the ID **ARC-06 with the S1 wording**, so the gate keeps one invariant for mapping that the new modules also use.

### 2.4 Recommendation

**Option A (hand-written) with S1 as ARC-06; the #90 round trips (S3) as regular tests.** Reasons: no new dependency (CLAUDE §11) for code that mostly retires within M2–M6 (F10); the new modules write their projections by hand anyway (D11) and S1 covers them; plain C# is the easiest to hand over. Option B is a close second, and it would be the choice if the owner expects a lot of DTO mapping in the new modules: its compile-time check is stronger than S1, and its licence passes D12.

### 2.5 Decisions needed (O-S3-4)

| # | Question | Options | Claude's proposal |
|---|---|---|---|
| a | Replacement | A hand-written · B Mapperly 4.3.1 · C Mapster 10 | **A** |
| b | ARC-06 successor | S1 (ID ARC-06, new wording) · S2 (retire ARC-06, compiler only; B only) · S3 as the invariant | **S1**, S3 as regular tests |
| c | #90 scope: which generic write endpoints stay (F7, F8) | (i) make `POST`/`PUT`/`DELETE` of all five controllers work as the run-2 instruction says, **plus** an authorization requirement (no catalog role exists yet; legacy JWT only) · (ii) **remove** the generic `POST`/`PUT`/`DELETE` of the four catalog controllers and of `ProposalsController` (keep the `GET`s and `Proposals/upsert`, `review`, `approve`); SD20 = catalog only through the CLI; the 5 DTO→entity maps and `Update` go away · (iii) make them work and limit them to the legacy `Approver` role until step 2 | **(ii)**: closes F7 without inventing a role, needs no maps that nobody calls, follows #90's own suggestion and SD20. It needs the owner because the run-2 instruction says "create and update work" |
| d | When F7 is closed | Separate small PR **now**, before run 2 (remove or lock the write actions, with an anonymous-write test) · in S3-07 run 2 | **Separate PR now**: every deployed API is open to anonymous deletes until then. Public repository: no public issue with reproduction steps before the fix |

## 3. Self-review (CLAUDE §6.7)

Run 1 changes no code. The only test written (F7) was a throwaway probe and is deleted; the spike in §2.2 lives in the session scratchpad. Licences were read from the NuGet `nuspec` files (`<license type="expression">`); the shipped file list of Mapperly from its `.nupkg` and from the spike's build output.

## 4. Checks

| Check | Result |
|---|---|
| Throwaway integration test (F7), Testcontainers PostgreSQL 17 | Anonymous `DELETE /api/FilterTypes/{id}` → 200, body `1`, row removed; unknown id → 200, body `0` |
| Mapperly 4.3.1 spike, .NET SDK 10.0.113 | Projection, `ApplyTo` and language helpers as expected; `RMG012`/`RMG020` as errors fail the build on an unmapped new DTO property |
| `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `reuse lint` | Not needed for run 1 (one Markdown file under `docs/`); run in run 2 |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | F7: anonymous writes on the generic catalog endpoints | Decision (d); fix before or in run 2 |
| P2 | F5: `AddOne` answers with the posted id | Goes with #90 in run 2, if the endpoint stays (decision c) |
| P3 | TESTING_v26 §5.5 says ARC-06 covers individual maps | Reworded in run 2's TESTING version |
| P4 | The CVE exposure note of S1-03 §4 stays true until run 2 removes the package | — |

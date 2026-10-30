# S2-06 — Module projects: design note and skeleton (M1.1)

> **Sprint:** 02, P0 task S2-06 (roadmap M1.1, 3 points; ARCHITECTURE_v14 §4.3, §12 step 1). **Issues:** none. **Requirements / invariants:** prepares ARC-01 – ARC-03 (TESTING_v15 §4.10); they stay `planned` until S2-08, so no new TESTING version. **Status:** done by Claude Code (2026-10-30), pull request open.
> **Scope:** the design note (§1.2) and the skeleton: `src/ProcureAccess.Modules.<Name>` and `.Contracts` for the eight modules, project references per ARCHITECTURE §4.3, `Add<Name>Module` called from `Program.cs`, SPDX headers, CLAUDE.md §3/§4. Not in scope: moving any code (platform code: S2-07), architecture tests (S2-08), analyzer settings (#118).
> **Owner decisions 2026-10-30:** **O-S2-1:** create the module and contracts projects and move only the platform code; `API`, `DAL`, `MODELS`, `SERVICES` stay legacy until the step that replaces them. **D1** (asked during the task, F3): module projects reference Platform **only through `Platform.Contracts`**; only the host references the `Platform` project. (Rejected: modules reference `Platform` and `Platform.Contracts`, the literal reading of §4.3, which gives them nothing but `AddPlatformModule`; `Platform` without contracts, which contradicts the sprint row and ARC-03's visibility rule.)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Four legacy projects: `MODELS` → nothing; `DAL` → `MODELS`; `SERVICES` → `DAL`, `MODELS`; `API` → all three. One `ApplicationDBContext` maps every entity (incl. `logging."SeriLogs"` and `analytics."UsageEvents"`); one migration history | Design note §1.2 |
| F2 | No `src/` folder, no module project; the target layout exists only in CLAUDE.md §4 and ARCHITECTURE §4.3 | Built from scratch |
| F3 | ARCHITECTURE §4.3 and TESTING ARC-03 say a module references "the listed `Contracts` projects and Platform"; the SPRINT_02 row also creates `Platform.Contracts`, and ARC-03 allows only controllers and `Add<Name>Module` to be public outside `Contracts` | Ambiguous, asked: D1 |
| F4 | §4.3's table names `Identity.Contracts`; CLAUDE.md §4 and the sprint row name the module `IdentityAccess` | Projects named `ProcureAccess.Modules.IdentityAccess(.Contracts)`; wording in ARCHITECTURE is P1 |
| F5 | S2-05 confirmed NetArchTest 1.3.2 on .NET 10 | No influence on the skeleton; S2-08 builds on it |
| F6 | The API image copies `DAL/`, `SERVICES/`, `API/` explicitly; CI coverage includes the four production assemblies by name | Both need the new projects, otherwise the image does not build and the modules are not measured |

### 1.2 Design note — legacy projects and the target layout

**Rule (O-S2-1):** a module project never references a legacy project (`API`, `DAL`, `MODELS`, `SERVICES`). The host `API` references the modules and calls their `Add<Name>Module()`; later, legacy code may also use a module's contracts while it waits for its step. Code moves into a module only when the migration step that replaces it starts, so nothing is moved twice (CLAUDE §6.2, risk R1). `API` stays the host for good (`Program.cs`, Swagger, versioning, configuration); it loses its controllers step by step.

| Legacy part | Target | When |
|---|---|---|
| `API`: `Program.cs`, `ApiVersionSupport`, `Swagger`, `AppConfigExtensions`, `EFCoreExtensions` | stays: host | – |
| `API`: `HealthController` (`/api/Health`), `HealthCheckExtensions` | Platform | S2-07 (health checks); `/api/Health` removed with #122 (S2-19) |
| `API`: `CriteriaController`, `CriteriaFiltersController`, `FilterTypesController`, `BaseCrudController` (catalog administration, D11) | Catalog | step 2 (catalog versioning); ad-hoc error objects replaced before, in S2-10 |
| `API`: `UsersController`, `IdentityUserEndpoints`, `AuthorizationExtensions`, `IdentityExtensions`, `UICustomizationController` | IdentityAccess (preferences C73) | step 3 (cookie session replaces the JWT) |
| `API`: `ProductsController`, `FavoritesController` | Directory / Workspace | steps 5 – 6 |
| `API`: `ProposalsController` | retired | step 5 |
| `API`: `DatabaseRebuildExtensions` (SEC-06 guard) | stays with the sample data | until the sample data is replaced (catalog import step 2, E2E seeder steps 2 – 4) |
| `DAL`: `ApplicationDBContext`, `Migrations` | split into module-owned contexts as entities move | from step 2 (how the one migration history splits: P5) |
| `DAL`: repos | with their entities (Criterion, CriteriaFilter, FilterType → step 2; User, UICustomization → step 3; Product, Favorite → steps 5 – 6; Proposal → retired step 5) | as listed |
| `DAL`: `SampleData`, `SampleDataInitializer` | replaced by the catalog release import and the E2E seeder | steps 2 – 4 |
| `MODELS`: `Criterion`, `CriteriaFilter*`, `FilterType`, `Language(s)` | Catalog (`CatalogRelease`, `CriterionRevision` …) | step 2 |
| `MODELS`: `User`, `Identity/`, `Authorization/`, `RefreshToken`, `UICustomization`, `Settings/AppSettings` (JWT) | IdentityAccess; `RefreshToken` and the JWT settings go away | step 3 |
| `MODELS`: `EmailSettings`, `EmailTemplates/` | Platform (`IEmailSender`, C71) | step 3 |
| `MODELS`: `Product`, `ProductCriteriaFilter`, `Favorite` | Directory / Workspace | steps 5 – 6 |
| `MODELS`: `Proposal*`, `Requests/` | retired | step 5 |
| `MODELS`: `SeriLogEntry`, `UsageEvent`, `UsageEventType` | Platform | S2-07 |
| `MODELS`: `Mapping/` (AutoMapper profile) | replaced with #31 | ADR before sprint 03 |
| `SERVICES`: `Logging/`, `Analytics/` | Platform | S2-07 |
| `SERVICES`: `CriterionService`, `CriteriaFilterService`, `FilterTypeService` | Catalog | step 2 |
| `SERVICES`: `UserService`; `EmailService`, `EmailTemplateService` | IdentityAccess; Platform | step 3 |
| `SERVICES`: `ProductService`; `ProposalService` | Directory / Workspace; retired | steps 5 – 6; step 5 |
| `SERVICES`: `Utilities/StringExtensions` | with its first module user | – |

Configurator (step 3, saved lists), Export (step 2, export pipeline) and Moderation (step 7) have no legacy code; they start empty and grow with their step.

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.<Name>/` ×8 (new) | `.csproj` with `FrameworkReference` `Microsoft.AspNetCore.App` and exactly the references of §4.3: own `Contracts`, the listed `Contracts` of other modules, `Platform.Contracts` (D1). One public static class `<Name>Module` with `Add<Name>Module(this IServiceCollection)`, which registers nothing yet; its XML doc names what the module owns and the step its code arrives with | Sprint row; ARCHITECTURE §4.3; ARC-03 visibility |
| `src/ProcureAccess.Modules.<Name>.Contracts/` ×8 (new) | `.csproj` only, no references, no code yet | Sprint row |
| `src/Directory.Build.props` (new) | Imports the root `Directory.Build.props` (NuGet audit suppression) and sets `net10.0`, nullable, implicit usings once for all 16 projects | One place instead of 16 |
| `src/.gitignore` (new) | `bin/`, `obj/` | As `tests/.gitignore` |
| `ProcureAccess.sln` | 16 projects in a `src` solution folder; CRLF kept (`dotnet sln` writes LF) | Root solution builds everything |
| `API/…/API.ProcureAccess.csproj`, `GlobalUsings.cs`, `Program.cs` | The host references the eight module projects; `Program.cs` calls `AddPlatformModule()` … `AddModerationModule()` after the legacy registrations, Platform first, then in dependency order | Sprint row |
| `API/…/Dockerfile` | `COPY src/ src/`; restoring the API restores the modules | The image would not build otherwise (F6) |
| `.github/workflows/ci.yml` | `--coverlet-include "[ProcureAccess.Modules.*]*"` in the backend test stage | Coverage of the new production assemblies (F6) |
| `CLAUDE.md` | Header; §3 current state (module skeleton, O-S2-1, D1); §4 current layout with `src/`; §7 module rule: Platform through `Platform.Contracts`, modules never reference legacy projects | DoD; rule for later sessions |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; plus a script check that every module `.csproj` references exactly the projects of §4.3 (it does, see check 2). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security, privacy, N+1, a11y: no endpoint, query, UI or secret touched; the modules register nothing | – |
| R2 | The compiler drops references to assemblies a project does not use, so with empty contracts the built module assemblies carry no reference to them yet. An ARC-03 check on assembly metadata alone would pass vacuously; it must read the `.csproj` references | Follow-up: S2-08 (P2) |
| R3 | Transitive project references: today no `Contracts` project references another, so nothing leaks. If one ever does, a module would see the second contracts project without listing it | Follow-up: S2-08 decides between `DisableTransitiveProjectReferences` and the ARC-03 reference check (P2) |
| R4 | The new projects carry neither `Microsoft.VisualStudio.Threading.Analyzers` (`API` and `SERVICES` do) nor warnings-as-errors (no project does) | Follow-up: #118, put the settings into `src/Directory.Build.props` there (P4) |
| R5 | Inside the Directory module, `Directory` binds to the namespace `ProcureAccess.Modules.Directory`, not `System.IO.Directory` | Won't fix: file storage belongs to Platform; write `System.IO.Directory` if ever needed |
| R6 | `Add<Name>Module` does not null-check `services` | Won't fix: extension called once from `Program.cs`; no behaviour |

No correctness or security finding that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln` | ✅ 0 errors; no warning from the new projects (the legacy warnings are #118) |
| 2 | References of every module `.csproj` compared with §4.3 (script over `src/*/*.csproj`) | ✅ exact match; contracts projects reference nothing |
| 3 | `dotnet format whitespace` / `style` on the solution, `dotnet format` on each test project | ✅ no changes |
| 4 | `dotnet test --solution ProcureAccess.sln` | ✅ 62/62 (Unit, Architecture incl. PRV-05, Integration incl. OPS-01/02, OPS-03, SEC-06, CAT-10 on the real `Program.cs`) |
| 5 | E2E stack (`npm run stack:up`): API image built through the changed Dockerfile | ✅ healthy; image contains the 16 module assemblies; `/health/ready` 200 |
| 6 | `npx playwright test`: smoke, helper self-tests, J1 (CAT-10 E2E, A11Y-01, third-party requests) | ✅ 51/51 on Chromium, Firefox, mobile Chrome. ⚠️ WebKit and mobile Safari not run locally (host lacks WebKit's system libraries, as in S1-18, S2-03); CI runs them |
| 7 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 8 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 9 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant. While `src/` was still untracked, reuse did not apply `src/.gitignore` and listed `bin/`/`obj/`; once tracked, as in CI, it is clean |
| 10 | Client build, Vitest, ESLint | – not touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | ARCHITECTURE §4.3 should state what this task decided: modules use Platform through `Platform.Contracts` (D1); module projects never reference legacy projects (O-S2-1); `Identity.Contracts` is `IdentityAccess.Contracts`; whether a `Contracts` project may reference another module's `Contracts` (e.g. a Workspace DTO using a Catalog type) is still open. Also closes risk R1 | **Done 2026-10-30** in ARCHITECTURE_v15 (C94, S2-07, owner's choice); the open question is O-22 |
| P2 | S2-08: ARC-03 reads the `.csproj` project references (R2), TESTING's ARC-03 text says `Platform.Contracts` instead of "Platform", and transitive references are handled (R3) | S2-08 |
| P3 | S2-07: `UsageEvent` and `SeriLogEntry` are mapped in the legacy `ApplicationDBContext`. Platform cannot reference `DAL` (rule above), and an entity is no contract. Options: a Platform-owned `DbContext` for the `logging` and `analytics` schemas (the tables already exist; the migration must not recreate them), or keep the mapping in `DAL` until step 2 (deviates from the S2-07 row). To be put to the owner in the S2-07 plan | **Decided 2026-10-30:** option A, a Platform-owned `DbContext` with its own migration history (O-S2-1 for S2-07, ADR-014); done in S2-07 |
| P4 | #118 (analyzers and warnings on all projects): the new projects are the cheapest place to start, through `src/Directory.Build.props` | #118 |
| P5 | How the single migration history splits when modules get their own `DbContext`s (one history per context, schema per module) is not written down | **Decided 2026-10-30:** ADR-014 (ARCHITECTURE_v15 §10.5), written in S2-07 with Platform as the first module with entities |
| P6 | WebKit / mobile Safari only in CI (check 6) | CI of this PR |

# CLAUDE.md

This file guides Claude Code (and any other AI assistant) when working in this repository. It is the project's AI onboarding document: read it at the start of every session.

> **Version:** v4 (2026-11-16) — v3 (2026-10-19, step 9 of `docs/ai_development_process.md`) grew with every task to 206 000 characters, far above the 40 000 at which Claude Code warns. v4 keeps the rules, the short layout and the everyday commands; the state of the code task by task, the annotated layout, the further commands and the change history of v3 moved verbatim to `docs/CODEBASE_v1.md` (owner request, 2026-11-16). Last task reflected: S5-31. Keep this file under 40 000 characters: a task records what it built in `CODEBASE_v*.md` and changes this file only when a rule, command or convention that applies across the codebase changes.

---

## 1. What this project is

**ProcureAccess** is a web tool of the publicly funded research project **WERTE.IT**. It helps public bodies and companies in the DACH region procure accessible software:

1. **Kriterien-Konfigurator** — choose use case, app types, product parts and optional test procedures → prioritised, plain-language accessibility criteria (based on BIT inklusiv texts) → adjust → export as XLSX/CSV. Works **without an account**.
2. **Workspace** — organisation-scoped procurement projects: candidate products, test evidence, per-criterion assessments, optional publication.
3. **Product directory** — published statements in two strictly separated channels: *Angaben der Anbieterseite* (vendors/auditors, Kanal 1) and *Unabhängige Angaben* (organisations, experience reports of people with disabilities, Kanal 2).

The tool must itself meet WCAG 2.1 AA / EN 301 549 / BITV 2.0, mobile-first, in light and dark appearance. It is **open source** (GitHub, public), maintained by **one developer**, and must be handed over to a successor: maintainability and documentation beat cleverness and performance.

## 2. Documents — where the truth lives

All in `docs/`. File names carry versions; **always use the highest version** of each document. Only the two newest versions of a document stay in `docs/`; a task that writes a new version deletes the third-newest. Older versions are in the git history, so a protocol that names one (e.g. `ARCHITECTURE_v18` C101) is read there: `git log --oneline -- docs/<file>` lists its commits, `git show <commit>:docs/<file>` prints it (use the commit before the deletion) (owner, 2026-11-09). Only a document's header names the versions of the other documents; text that describes the current state cites them without a version, by section, change or invariant number (`ARCHITECTURE §7.2`, `C117`, `TESTING §8.2`, `SPEC R-CON-08`), because these numbers stay stable while every new version made a versioned reference stale; a code comment names a versioned file by its pattern (`docs/ux_copy_procureaccess_v*.md`). Change rows and protocols are history and keep the versions they name (owner decision D1 (a), S5-05).

| Topic | Document | Authoritative for |
|---|---|---|
| Process | `ai_development_process.md` | Which step comes next |
| Product scope | `prd_procureaccess_werteit_v*.md` (German) | What and why, P0/P1/P2, non-goals |
| Feature spec | `SPEC_v*.md` | Requirements `R-<AREA>-nn`, acceptance criteria, limits, rate limits, contracts §8, open questions §13 |
| Architecture | `ARCHITECTURE_v*.md` | Modules, flows A–E, domain model, API, ADRs, migration steps §12, open points §16.5 |
| Tests | `TESTING_v*.md` | Invariant catalogue (CI gate), tooling, test levels |
| Tech debt | `TECH_DEBT_v*.md` | Known debt with scores and when it is resolved; check its *When* column when a migration step starts; new version at the end of each sprint (process step 14) |
| Sprints | `docs/sprints/sprint_NN/SPRINT_NN.md` and the task protocols `docs/sprints/sprint_NN/S<N>-nn_<topic>.md` | Sprint goal, task list with IDs (`S2-nn` …), owner decisions `O-S<N>-n`, Definition of done; what each task found, changed and left open |
| UI texts | `ux_copy_procureaccess_v*.md` (German) | Every UI string and its key; terminology (§2) |
| UI behaviour | `design_handoff_procureaccess_v1.md` (internal version 1.x) | Tokens, layout, components K1–K6, screens |
| Figma ↔ code | `figma_code_mapping_procureaccess_v1.md` | Component paths, `@figma` tags, CI link check |
| Figma files | Design file `rPeSyK8NNe3d8AnUEXW9mt` (pages „Komponenten“, „Screens“; node IDs per frame in the Figma mapping) and FigJam board `4JmdtppBCOiAKzYPzzCLkv` (architecture diagrams, flows A–E, decision trees, persona flow map) | **Design file:** visual reference for screens and components — when the Figma MCP connection is available, compare a screen or component with its frame (node ID from the mapping) before and after building it; the Handoff and the UX copy stay authoritative for behaviour and wording. **FigJam board:** illustration only, not maintained per version (boards 01–09 and 17 show ARCHITECTURE v7, boards 10–16 v4); on any difference the current ARCHITECTURE wins — do not build from the board |
| Criteria content | `kriterienkatalog_bit_inklusiv_v1*.md` (German) | Filter layers, exclusions, rating scale, BIT inklusiv texts |
| Release gaps | `kriterienkatalog_<version>_luecken_v*.md` (German) | What the project team still has to supply per criterion of a release folder (plain-language title, priorities, product parts to confirm) |
| Codebase state | `CODEBASE_v*.md` | What each task built, by area (§2), the annotated repository layout (§3), further commands (§4), the change history of this file (§1); read the section of an area before working on it |

**Precedence on conflict:** PRD (scope) → SPEC (behaviour, acceptance) → ARCHITECTURE (technical shape) → Handoff (visual detail) → UX copy (wording only). Never resolve a conflict silently: stop, name both places, and ask.

**Language:** code, comments, commit messages and technical docs (CLAUDE.md, ARCHITECTURE, SPEC, TESTING, TECHNICAL) in **English**. PRD, user handbook and UI texts in **German**. Domain terms stay German where they are project vocabulary (Kriterium, Workspace, Kanal 1/2, Vergabeunterlagen).

## 3. Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core 10 (.NET 10 LTS), C#, EF Core 10 with Fluent API only (`IEntityTypeConfiguration<T>`, no data annotations), Npgsql 10 |
| Database | PostgreSQL 17 (`pg_trgm` for product search) |
| Frontend | Angular 22 (zoneless, standalone components, signal inputs), NgRx Signals, Angular Material 22 (M3), ngx-translate (`de.json`) |
| Auth | ASP.NET Core Identity cookie authentication + antiforgery (`X-XSRF-TOKEN`); no JWT, no token in JavaScript |
| E-mail | `IEmailSender` (MailKit, SMTP/TLS); Lettermint in staging/production, Mailpit locally |
| Excel | ClosedXML (MIT) — never EPPlus |
| Tests | xUnit v3, Shouldly, NSubstitute, Testcontainers (PostgreSQL 17), Respawn, NetArchTest, `FakeTimeProvider`; Vitest + Angular Testing Library; Playwright + axe-core |
| Packaging | Docker / docker-compose; GitHub Actions |

**State of the code:** migration step 0 is done (sprint 01), parts of steps 1–3 are built: the module projects under `src/` (Platform, Catalog, IdentityAccess and Configurator with their own contexts; the Configurator and Export endpoints), the catalog with releases, the configurator S10/S11 and the anonymous export, the cookie session with registration, password reset and lockout, organisations with members and invitations (S20, S21, the invitation page), the signed-in header states with the organisation switch; the legacy account screens are gone (S5-10; the settings page in S5-25b, replaced by S60 in S5-26; the profile in S5-28b, replaced by S61); the personal data export (S5-27); the persistence of saved lists with `IConfigurationReader` and the export source of saved lists (S5-19); saving, reading and listing them, with the draft check the anonymous export shares (S5-20a); on the client „Liste speichern“ with the draft kept across the sign-in, S12, S14 and a saved list read only (S5-20b); changing a decision or the selection, deleting a list, its copy to the active release and its export, on the server (S5-21a) and on the client with J1 complete (S5-21b); the weight range served by the API, the `no-store` default, S13 leading to a too-long note and „Noch nicht gespeichert“ (S5-22); share links of saved lists on the server: Platform's `ShareLink`, the management endpoints and the anonymous read `GET /api/v1/shared/{token}` (S5-23); the share dialog S15 with the expiry dates the server serves and the shared list S16 at `/geteilt/:token` without an account (S5-24); the user preferences, `GET`/`PUT /api/v1/account/preferences` and the export's settings (S5-25a), applied on the client by `withSettings` with the legacy settings store, its page `/settings` and the legacy `UICustomization` removed (S5-25b); the settings page S60 at `/einstellungen`, each change applied at once and saved (S5-26); the account deletion on the server, `DELETE /api/v1/account` with the organisation rules (S5-28a); the account page S61 at `/konto` with the export download and the deletion dialog (S5-28b); the session hardening of step 4: renewal on every request, expiry headers, `POST /api/v1/auth/session/extend`, the security stamp checked on every request (S5-29); the session warning and the completed login overlay, step 4 done (S5-30); the rating legend `pa-rating-legend` once above the list on S11, a saved list and S16 (S5-31). The legacy projects `API`, `DAL`, `MODELS`, `SERVICES` and their screens remain until their step replaces them. **Every built area is described in `docs/CODEBASE_v*.md` §2** (search for the task ID, file or type) — read it before changing that area. Next: migration step 1 (roadmap M1, without the packages already done in S1-22 and S1-23).

**Rules of the built code** (details and reasons in `CODEBASE` §2):

- **Backend modules:** endpoints are `internal` minimal APIs; a module makes public only `<Name>Module` with `Add<Name>Module(…)` and `Map<Name>Endpoints()`, mapped in `Program.cs` on the versioned group `/api/v1` **before** `app.ConfigureSwaggerExplorer()`; legacy controllers stay at `/api/<controller>`. Every `/api/v1` answer without its own `Cache-Control` gets `no-store` from the Platform middleware (TEN-04); an endpoint that may be cached sets its header itself, as the catalog does. Endpoints that need the legacy users (`UserManager<User>`) live in the host (`API/Account/`, `API/Auth/`, `API/Organizations/`) until M3.1. EF generates migrations `public`: make them `internal`. A new .NET project needs its `COPY` line in the API `Dockerfile`.
- **Errors:** set `ProblemDetails.Type` to a name of `ProblemTypes` (`Platform.Contracts`), Platform writes the URI; `errors` keys are field paths as the client sends them. A new problem type gets its UX key in the client's `PROBLEM_TYPE_KEYS`.
- **🔧 settings** live in `appsettings.json` (or the client's `environment*.ts`) and are validated at startup; client copies of server values stay in step (`passwordMinLength` ↔ `Account:Password:MinLength`, `lockout` ↔ `Account:Lockout:*`); better, the API serves the value, as `GET /api/v1/configurator/settings` the weight range (S5-22).
- **Rate limits:** a new policy gets its name in `RateLimitPolicies`, its settings in `appsettings.json` and an `INV:API-05` test; per-user policies go into `UserPolicies`, body-keyed ones into `KeyedPolicies` (`IKeyedRateLimiter`); the integration host and `docker-compose.e2e.yml` relax limits.
- **Session (S5-29):** `SessionCookieEvents.ValidatePrincipal` alone decides limits, stamp check and renewal; every authenticated request except the probe renews the idle limit and its answer is `no-store` (also a signed-in catalog read), so never rely on a cached answer for a signed-in user; a test that expects a session ended by a changed security stamp advances the fake clock first. The client's limits live in `SessionLimits`, written by `authInterceptor`, which must not inject the root store. The warning (`SessionWarningService`, S5-30) asks the probe before it warns and at the limit. E2E moves time only per browser context through `e2e/support/clock.ts` (cookie `pa-test-clock`; the API honours it in Development/E2E with `TestClock__Enabled`). On the dev server the dev health watcher renews the session every 5 s: check the warning on the E2E stack.
- **Identity and tokens:** the Identity cookie is the only scheme; antiforgery on every state-changing endpoint (exemption list in `AntiforgeryValidationTests`, only the anonymous export). Gated actions use `IdentityAccessPolicies.ConfirmedEmail`, each with its own `INV:SEC-10` case. Link tokens are 32 random bytes, stored only as SHA-256, travel only in POST bodies, never in logs; link pages keep them in memory and drop them from the address. Exception: a share link is meant to be passed on, so its token is in the path (`/geteilt/<token>`, `GET /api/v1/shared/{token}`); `ShareTokenMaskingEnricher` and the client's nginx log format mask it (S5-23). Never look up an address in the request path of the password-reset request (background mailer).
- **Tenancy as built:** see §7; a new scoped entity also gets a sample in `OrganizationIsolationTests`, a new organisation endpoint its rows in `AuthorizationMatrixTests`, a new raw-SQL or `IgnoreQueryFilters()` site an allow-list entry in `IgnoreQueryFiltersAllowListTests`.
- **Texts outside the client:** e-mails (`IEmailSender` message classes in `Platform.Contracts/Email`) and export files read the UX copy through `check-i18n.mjs` (`EMAIL_AREAS`, `EXPORT_AREAS`); a new key a renderer reads goes into `EmailTexts.Required` / `ExportTextKeys.Required`. ClosedXML: never `AdjustToContents` (no fonts in the image).
- **Catalog:** stored releases never change; never cache which release is active; never import 2026.1 into a kept database before its priorities exist. Usage events only through `IUsageEventWriter` (`SPEC §9.1`).
- **Tests (.NET):** real PostgreSQL through the integration fixture (`ProcureAccessApiFactory`, Respawn, `FakeTimeProvider`, `CapturingEmailSender`); a new entity–DTO map gets a case in `MapCases` (ARC-06); after a NuGet or SDK update regenerate `THIRD-PARTY-NOTICES.txt` (`check-api-notices.mjs --write`).
- **Client routing:** every page is lazy (`loadComponent`/`loadChildren`); `app.routes.ts` imports no store or service and route paths live in import-free model files (cycle through `app.config.ts`); the resolvers of one route run side by side, so never read a value another resolver of the same route writes; never destroy the primary outlet for the `login` outlet. Page titles through `data[PAGE_TITLE_KEY]`, `PageTitleStrategy.setPageTitleState`, `changePageInPlace`, `focusPageHeading`.
- **Client requests:** mark a request with `CALLER_HANDLES_PROBLEMS` (problem types the caller shows), `CALLER_SHOWS_FAILURES` (a requested action whose screen shows every failure), `BACKGROUND_REQUEST` (only requests the user did not ask for) or `FORM_SHOWS_FIELD_ERRORS`; session, drafts and the user's settings live in memory, never in browser storage (R-SET-02).
- **Client details:** a new icon goes into `src/assets/fonts/material-symbols-icons.json`, then `scripts/subset-material-symbols.sh`; a new UX copy area joins `AREAS` of `check-i18n.mjs` with the step that builds its screens (the table shorthand `/ .x` replaces the last key segment); never bind `id` on a component host; Material sets no `aria-invalid` on an empty required field (test `aria-describedby`); a Material button that can start unavailable gets `[aria-disabled]` (its own input), because its host binding overwrites `[attr.aria-disabled]="'true'"` on the first render (S5-28b); tests that switch a `mat-tab` provide `animationsDisabled`.
- **E2E:** axe through `e2e/support/axe.ts` checks both appearances (tag `INV:A11Y-08` too); a rebuilt screen joins `e2e/shell/reflow.spec.ts`; S10's combobox only through `chooseFirstOption`. Signed in, „Abmelden“ and „Organisation“ sit in the menu „Konto“: use `accountMenuButton`/`signOutThroughHeader` of `e2e/support/session.ts`. The seeded E2E users and organisation are shared: never send them a wrong password, never change them — a test that changes something builds its own through `e2e/support/accounts.ts`. Firefox crashes its page process in known axe patterns (TESTING V129, V165, V226, V229): a `<section>` gets no accessible name (`aria-labelledby`) for now, and the specs use CSS locators where they say so — check those rows before working around a new crash.


## 4. Repository layout

Current, in short:

```
ProcureAccess.sln, global.json        Root solution incl. tests; global.json pins the .NET SDK and the test runner
API/API.ProcureAccess/                Host (Program.cs, command modes `catalog …`/`roles …`), legacy controllers, Account/, Auth/ and Organizations/ endpoints until M3.1
DAL/, SERVICES/                       Legacy EF context, repositories, migrations, entities, DTOs, services (retired step by step)
src/ProcureAccess.Modules.<Name>/     One project per module (+ .Contracts): Catalog, Configurator, Workspace, Directory,
                                      Moderation, Export, IdentityAccess, Platform; src/Directory.Build.props
catalog/releases/<version>/           Catalog release folders for `catalog import` (2026.1 texts only, not activated until O-1)
CLIENT/CLIENT.ProcureAccess/          Angular SPA: src/app/{core,layout,pages,shared,features/<name>,testing}
tests/ProcureAccess.Tests.{Unit,Architecture,Integration,Shared}/   Backend tests; Shared = builders and fixtures (MiniCatalog, Tenants)
e2e/                                  Playwright + axe (own package.json): support/, smoke/, journeys/, identity/, organization/, configurator/, shell/
docker-compose.yml (+ .dev.yml, .e2e.yml)   Local stack (API 5000, client 4200, PostgreSQL 5432); dev: test release T.1 and Mailpit; E2E: in-memory DB, seeded users
.github/workflows/                    ci.yml (stages 1–8 of TESTING §8.2), codeql.yml (stage 9)
scripts/                              CI scripts without dependencies, each with a node:test self-test
docs/                                 Project documents (§2)
```

Every folder with its files and the task that built it: `docs/CODEBASE_v*.md` §3.

Target (built along the migration steps, `ARCHITECTURE §4.3`, §12; `TESTING §3.4`):

```
ProcureAccess.sln                               root solution incl. tests
src/ProcureAccess.Modules.<Name>/               one project per module:
src/ProcureAccess.Modules.<Name>.Contracts/     Catalog, Configurator, Workspace, Directory,
                                                Moderation, Export, IdentityAccess (+ Platform)
tests/ProcureAccess.Tests.{Unit,Architecture,Integration,Shared}/
e2e/                                            Playwright journeys J1–J7
catalog/releases/<version>/                     catalog release data (JSON), imported by CLI (SPEC SD20; no editor UI)
content/legal/*.de.md                           imprint, privacy, accessibility statement
scripts/check-invariants.mjs, scripts/check-figma-links.mjs
```

Angular features follow `src/app/features/<name>/{models,services/api,state,pages,components,dialogs}`; store slices `withCatalog`, `withConfigurator`, `withWorkspace`, `withDirectory`, `withIdentity`, `withSettings`, `withLoading`. `withProposal()` and `withProducts()`/`withFavorites()` are legacy and will be retired or reworked; the `Approver` role is `Moderator` since S4-25 (`ARCHITECTURE §8.1`).

## 5. Commands

```bash
cp .env.example .env                      # once; fill in the secrets (never commit .env)
docker network create dokploy-network     # once; the compose file uses it as an external network
docker compose up --build                 # full stack (DB + API + client), reads .env; client http://localhost:4200 calls the API through its nginx (/api/)
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build   # plus the active test release T.1 and Mailpit (http://localhost:8025); `down -v` after a change to T.1
curl http://localhost:5000/health/ready   # 200 when the API reaches the database; Swagger at /swagger only in Development
# Manual API checks (session, registration, organisations, export …) and the operations commands (key ring, `catalog import|activate`, `roles`): CODEBASE §4

# API (run from API/API.ProcureAccess). Secrets only from environment variables:
export ConnectionStrings__ProcureAccess="Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres;Password=<from .env>"
export Email__Smtp__Host=localhost Email__Smtp__Port=1025 Email__Smtp__Security=None   # Development with the dev stack's Mailpit; other environments need SMTP credentials
dotnet build && dotnet run
dotnet ef migrations add <Name> --project ../../DAL/DAL.ProcureAccess --startup-project . --context ApplicationDBContext
dotnet ef migrations has-pending-model-changes --project ../../DAL/DAL.ProcureAccess --startup-project . --context ApplicationDBContext   # exit 0 = model matches (OPS-03)
# Module contexts (ADR-014): the same with --project ../../src/ProcureAccess.Modules.<Name> --context <Name>DbContext --output-dir Persistence/Migrations
#   (PlatformDbContext, CatalogDbContext, IdentityAccessDbContext, ConfiguratorDbContext); make the generated class `internal`; moving a legacy table: ADR-014 rule 4

# Client (run from CLIENT/CLIENT.ProcureAccess; Node from .nvmrc)
npm install && npm start                  # http://localhost:4200 (proxy to the local API)
npm run build:release
npx ng test                               # Vitest (single run without a terminal); npm run test:coverage for the lcov report
npm run lint                              # ESLint incl. template accessibility and type-aware rules (CI stage 1)

# Backend (repository root). global.json pins the SDK (10.0.404): curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 10.0.404 --install-dir ~/.dotnet
dotnet test --solution ProcureAccess.sln  # integration tests need Docker (Testcontainers)
dotnet format ProcureAccess.sln --verify-no-changes   # CI stage 1
dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904   # CI stage 2

# E2E + axe (run from e2e/). Needs .env; stop the dev stack first (same ports)
npm install && npx playwright install chromium firefox webkit   # once
npm run stack:up                          # E2E stack: in-memory DB, T.1 active, seeded E2E users and organisation, Mailpit
npx playwright test                       # all browsers; --project=chromium for one
npm run stack:down
npm run typecheck && npm run lint         # CI stage 1 for e2e/ and scripts/*.mjs

# Checks (Node, no dependencies; each has a self-test: node --test scripts/<name>.test.mjs)
node scripts/check-invariants.mjs         # CI stage 7: every `active` invariant of the highest TESTING_v*.md has a test tagged INV:<ID>
node scripts/check-figma-links.mjs        # CI stage 1: @figma tags of built mapped components and pages
node scripts/check-i18n.mjs [--write]     # CI stage 1: client, export and e-mail texts match the UX copy; --write copies them first
node scripts/check-icons.mjs              # CI stage 1: icon list and font subset match the used icons
node scripts/check-api-notices.mjs [--write]   # CI stage 2 after a build: THIRD-PARTY-NOTICES.txt matches the shipped NuGet packages
node scripts/check-release-config.mjs     # before a production release only (🔧 placeholders)
```

CI (`.github/workflows/ci.yml`) runs the commands above on every pull request, and `.github/workflows/codeql.yml` runs CodeQL; every stage except coverage (stage 8) must be green before a merge (sprint Definition of done). When a migration step starts, flip its invariants in the highest `TESTING_v*.md` (CLAUDE §6.4); the gate reads only that file.

Licence check (REUSE, CLAUDE §10): `reuse lint` (install with `pipx install reuse`; without it, from the repository root: `docker run --rm --user $(id -u):$(id -g) -v "$PWD":/data fsfe/reuse:latest lint`). New files get a header with `reuse annotate --copyright "Blinden- und Sehbehindertenverein Hamburg e.V." --year <year> --license MIT <file>`; files without comment syntax (JSON, binaries, generated files) are covered in `REUSE.toml`.

A new .NET project under `DAL/`, `SERVICES/` or `src/` needs its own `COPY` line in `API/API.ProcureAccess/Dockerfile` (restore layer); without it the image build stops with NETSDK1004.

`RebuildDatabase` / `SampleDataInitializer` must never run outside `Development` (SEC-06); `DatabaseRebuildExtensions` checks `IHostEnvironment`, not only the flag. Migrations apply on startup (single instance only); so does the log-retention job, which assumes one API instance.

## 6. How to work here

1. **Find the requirement first.** Every change maps to an `R-…` ID in SPEC (or an architecture/test task). Name the IDs in the PR description. If no requirement covers the change, ask.
2. **Work in migration-step order** (`ARCHITECTURE §12`, mapping in `SPEC §11`). Do not build features of a later step on top of structures an earlier step will replace.
3. **Small slices.** One slice should be reviewable in one session: backend + tests + frontend + texts for one requirement or a coherent part of one.
4. **Tests come with the code.** Starting a migration step flips its invariants in `TESTING §4` from `planned` to `active`; the CI gate then requires a test tagged `INV:<ID>` for each. Name tests `Scenario_Condition_ExpectedOutcome` and add `[Trait("Invariant","<ID>")]` plus `INV:<ID>` in the name (`TESTING §8.1`).
5. **UI strings only by key.** Never hard-code German text in templates or code; add missing keys to `de.json` with the exact wording from the UX copy. If a needed key is missing in the UX copy, stop and report it — do not invent wording.
6. **Open decisions stay open.** Open points (SPEC §13, ARCHITECTURE §16.5) and ⚖️ texts may be implemented as written but must not be decided by you. 🔧 values are confirmed defaults (SPEC SD23): always read them from configuration, never hard-code them, and do not change them on your own.
7. **Before each push:** run the code-review checklist (security, correctness, N+1 queries, missing edge cases, a11y), the invariant, Figma-link and i18n scripts, and `reuse lint`.
8. **Review findings: sort them, don't fix everything.** Put every finding of a code review into *fix* (correctness or security, affects behaviour today), *won't fix* (reason in the PR) or *follow-up issue*, and list that sorting in the PR — also when asked to "fix all points". Keep fixes minimal, add no mechanism the fix does not need, and check the side effects of your own proposals before making them. A review round is enough when it finds no correctness or security bug that affects current behaviour, only style, matters of opinion or "what if later" cases: then recommend merging, not another round.

## 7. Architecture rules (enforced by tests — do not work around them)

- **Modular monolith.** A module references other modules only through their `*.Contracts` project, as listed in `ARCHITECTURE §4.3`; Platform may be used by all, through `Platform.Contracts`. A module project never references a legacy project (`API`, `DAL`, `MODELS`, `SERVICES`); only the host `API` (and the test projects) references the module projects (O-S2-1); a `Contracts` project references only the `Contracts` projects of the modules its row lists, never `Platform.Contracts` or a module project (ADR-017, S5-19). A new module or a changed dependency changes ARCHITECTURE §4.3 and `ModuleMap.cs` together. Outside `Contracts`, only controllers and the module registration are public. Catalog depends on nothing (ARC-01–03).
- **One `DbContext` per module.** A module that owns tables has its own `internal` `DbContext`, migrations in its project and its own history table `<schema>."__EFMigrationsHistory"`; no foreign key or join crosses two contexts; tables leave the legacy `ApplicationDBContext` only as ADR-014 rule 4 describes (`ARCHITECTURE §10.5`).
- **Workspace and Directory never reference each other's entities.** Flow B goes through `IPublicationGateway`; withdrawal rights through `IEntrySourceAuthorizer`.
- **Exactly three write paths into the directory** (`PublicationService` Flow B, `ContributionService` Flow D, `VendorPublicationService` Flow E), all through **`PublishedEntryFactory`**; only the factory constructs `PublishedEntry`/`PublishedEntryVersion` (PUB-01, ARC-07). Kanal 1 only via Flow E; Flow B is always Kanal 2.
- **Published versions are immutable.** A `SaveChangesInterceptor` rejects changes, except `ContentPurge` on consent withdrawal (PUB-13, PRV-10). Republishing creates a new version of the same entry.
- **Catalog releases are immutable**; configurations and entries are pinned to a release (ADR-002). Matching across releases uses `Criterion.Key`; `NotCoveredByThisRelease` is its own value.
- **Tenant isolation** through EF global query filters on `OrganizationId`; `IgnoreQueryFilters()` only at allow-listed call sites (TEN-01, TEN-02). Resolve the organisation from the resource, never from a client header. Foreign resources → 404, not 403. As built (S5-15, ADR-007): the filter is bound to `IOrganizationContext` (`Platform.Contracts`), the memberships of the signed-in user loaded once per request by IdentityAccess; a new scoped entity gets `HasQueryFilter(e => _organizations.OrganizationIds.Contains(e.OrganizationId))` in its module context and a sample in `OrganizationIsolationTests`; a new bypass goes into the allow-list of `IgnoreQueryFiltersAllowListTests` with its reason; a role check is `OrganizationRoleRequirement` on the resource, answered through `OrganizationAccess.StatusOf` (404 non-member, 403 lesser role); raw SQL bypasses the filter too and is allow-listed like `IgnoreQueryFilters()` (S5-16a).
- **Generic CRUD controllers** (`BaseCrudController`) only for catalog administration; workspace and directory endpoints are hand-written with their invariants (D11).
- **Errors** are RFC 9457 Problem Details with a field-keyed `errors` map and the problem types of `SPEC §8.6`.
- **Time** only through `TimeProvider`; never `DateTime.Now`/`UtcNow`.

## 8. Product rules that must never break

- **No overall verdict.** No score, percentage, average, rating, stars, traffic light or bar for a product — not in the database, not in any DTO, not in the UI (SIG-01, SIG-02, SIG-05, SIG-08). Fulfilment counts are always shown complete and in fixed order: eight in the directory, seven in the workspace, none for experience reports. Never sort or filter the directory by fulfilment (D3).
- **Two signals stay separate:** documentation (K2) and fulfilment (K1) never share a container or a DTO branch (SIG-04, SIG-09).
- **„Auf Barrierefreiheit geprüft: Ja/Nein“** is always shown for every test-result entry, also „Nein“; never for experience reports; „Nicht bekannt“ exists only inside the workspace and blocks publication (SIG-03, SIG-10–12).
- **Both channels look identical** apart from their label; never colour-code products, channels or counts.
- **„Geprüft nach …“** criteria are derived from test evidences, never assessed and never counted (CAT-17).
- **Priorities explain themselves:** every resolved priority carries rationale, matched rule and source „Empfehlung des Projektteams“ (CAT-02).
- **Pseudonymity:** `PublishedEntry` has no `OrganizationId`; `UserContributorLink` is never exposed; never log a user id together with a contributor-profile or entry id above `Debug` (PRV-03, PRV-04).
- **Private fields never leave the workspace** (`InternalNotes`, `ContactNotes`, `TesterQualificationNote`, consent records) (PRV-01, PRV-02, PRV-08).
- **Anonymous export persists nothing** (no rows, no files, no notes in logs) (EXP-05).
- **No third-party requests from the browser:** fonts and icons self-hosted, no analytics, no CDN. Usage analytics only via the two `UsageEvent`s in `SPEC §9.1`.

## 9. Frontend conventions

- **Zoneless:** every async result goes into a signal; no `fakeAsync`, no reliance on Zone.js.
- **Mobile-first:** base styles for 360 px, enhancements with `min-width` media queries (600 px) and container queries (840 px). DOM order = visual order; no CSS `order`. Touch targets ≥ 48 px. Works at 320 px / 400 % zoom without horizontal page scrolling.
- **Supported browsers** (SPEC §7.4): two latest major versions of Chrome, Edge, Firefox, Safari on desktop; Safari on iOS and Chrome on Android (current and previous). Playwright runs Chromium, Firefox and WebKit plus mobile viewports in CI. No polyfills for older browsers.
- **Server field errors** (API-02): mark the request with `FORM_SHOWS_FIELD_ERRORS`, catch the failure in the form, pass it to `applyServerErrors` with a resolver from server path to control, input id and label key, and show the result in `pa-error-summary` at the top of the form; each field shows `hasError('server')` with `errors.field.server`. Example: `product-proposal`.
- **Texts from the UX copy:** keys of the areas in `AREAS` of `scripts/check-i18n.mjs` are never typed into `de.json` by hand: run `node scripts/check-i18n.mjs --write` after a new UX copy version; a key that starts with another key's text goes next to it with a dot in its name. Legacy uppercase keys stay until their screen is rebuilt.
- **Lint exceptions only shrink:** never add a file or a rule to `LEGACY_EXCEPTIONS` in `eslint.config.js` and never use `eslint-disable` in new code; fix the finding (e.g. `void` or `.catch()` for a promise nobody awaits, `inject()` instead of constructor injection). When you replace or fix a listed file, delete its entry (#158). `e2e/` and `scripts/` have no exception list at all (S3-26).
- **Plural forms** (SPEC SD25): a text with a count uses `translatePlural: count`, never `if (count === 1)`. Keys: `<key>` = plural, `<key>.one` = singular, optional `<key>.zero`; in `de.json` they sit side by side as `"title": …, "title.one": …`.
- **Tokens only:** colours `--mat-sys-*`, spacing `--pa-space-*`; never hex values or px font sizes. Both appearances (light/dark) must work (A11Y-08).
- **Domain components K1–K6** and the shared components of the Handoff carry the product rules; never re-implement their markup ad hoc. Every mapped component and page has a `@figma` JSDoc tag (+ `@spec`, `@invariants`) as in `figma_code_mapping_procureaccess_v1.md`.
- **Accessibility:** use the patterns of Handoff §6–§7: error summary gets focus after submit, field errors linked with `aria-describedby`, live announcements only through `A11yAnnouncerService`, focus to H1 on route change, `aria-disabled` (not `disabled`) for unavailable options with a visible reason, no tooltips as the only source of information, no skeleton screens, no truncation.
- **Component tests** query by role and accessible name (Angular Testing Library) with the real German texts (`provideTestTranslations()`); if something cannot be found that way, fix the accessibility, not the test. Trigger actions without change detection, then `await fixture.whenStable()`: clicks with `element.click()`, other events with `fireEvent` from `@testing-library/dom` — never `fireEvent` from `@testing-library/angular`. Example: `delete-account-dialog.spec.ts`.

## 10. Security and privacy

- Cookie session (`HttpOnly`, `Secure`, `SameSite=Strict`), 30 min idle / 8 h absolute, warning 5 min before; 401 opens the login overlay and retries the request (SEC-01–03, SEC-07).
- Antiforgery on every non-GET endpoint except the anonymous export (SEC-02, SEC-09).
- E-mail confirmation gates saving, organisations, invitations, contributor profiles and publishing (SEC-10); lockout and token rules per SEC-11.
- Rate limits per `SPEC §7.3`; field limits per `SPEC §7.2` (server authoritative).
- Secrets only from environment variables — the repository is public. Licence headers (REUSE/SPDX) on new files.
- Dependency licences (`ARCHITECTURE` D12): **shipped dependencies** (API or client image, browser bundle) must be OSI-approved, without commercial-use restriction and **without strong copyleft or reciprocal licence** (GPL, AGPL, RPL, OSL …). Weak, file-level copyleft (MPL-2.0, LGPL as a separate library) is allowed if used unmodified. A dual licence counts by the option the project would use. **Development and test tools that are not shipped:** OSI-approved, no commercial-use restriction. **Services of the hosting platform** (GitHub Actions runners, CodeQL code scanning, Dependabot) are not dependencies; actions a workflow references (`uses:`) are development tools and follow the rule (ARCHITECTURE_v16 C98). **Notices:** every image ships the licence notices of what it bundles — the client image `/3rdpartylicenses.txt`, the API image `/app/THIRD-PARTY-NOTICES.txt` (generated, checked in CI stage 2; a NuGet licence outside its allowlist fails until it is checked and added to `scripts/check-api-notices.mjs`, ARCHITECTURE_v46 C132).

## 11. Do / don't

**Do**
- Read the relevant SPEC requirement, architecture section and invariants before coding.
- Keep changes small, tested and traceable (`R-…` and `INV:` IDs).
- Update `de.json`, tests and — if a structure changes — this file in the same PR.
- Ask when a document is ambiguous, contradictory or silent.

**Don't**
- Add a score, ranking, aggregate or colour coding for products.
- Introduce new dependencies, services, message brokers or caches without an architecture decision.
- Bypass `PublishedEntryFactory`, the query filters or the module contracts.
- Write UI text that is not in the UX copy, or change ⚖️ texts.
- Use the EF Core InMemory provider or SQLite in tests (real PostgreSQL only).
- Commit secrets, real personal data or test data from real users.

# CLAUDE.md

This file guides Claude Code (and any other AI assistant) when working in this repository. It is the project's AI onboarding document: read it at the start of every session.

> **Version:** v3 (2026-10-19) — step 9 of `docs/ai_development_process.md`, generated from `SPEC_v1.md` and `ARCHITECTURE_v11.md` (v2, 2026-10-15) and aligned with `SPEC_v2.md` / `ARCHITECTURE_v12.md` (v3: the former proposals O-S1–O-S4 and O-S6 are decided; supported browsers added). v1 only described the existing codebase. Updated during sprint 01 (S1-01 – S1-23) with the current state of the code, layout and commands; merged on 2026-10-26 from the repository-root `CLAUDE.md` and `docs/CLAUDE_v3.md`. Keep it updated whenever a migration step changes structure, commands or conventions.

---

## 1. What this project is

**ProcureAccess** is a web tool of the publicly funded research project **WERTE.IT**. It helps public bodies and companies in the DACH region procure accessible software:

1. **Kriterien-Konfigurator** — choose use case, app types, product parts and optional test procedures → prioritised, plain-language accessibility criteria (based on BIT inklusiv texts) → adjust → export as XLSX/CSV. Works **without an account**.
2. **Workspace** — organisation-scoped procurement projects: candidate products, test evidence, per-criterion assessments, optional publication.
3. **Product directory** — published statements in two strictly separated channels: *Angaben der Anbieterseite* (vendors/auditors, Kanal 1) and *Unabhängige Angaben* (organisations, experience reports of people with disabilities, Kanal 2).

The tool must itself meet WCAG 2.1 AA / EN 301 549 / BITV 2.0, mobile-first, in light and dark appearance. It is **open source** (GitHub, public), maintained by **one developer**, and must be handed over to a successor: maintainability and documentation beat cleverness and performance.

## 2. Documents — where the truth lives

All in `docs/`. File names carry versions; **always use the highest version** of each document.

| Topic | Document | Authoritative for |
|---|---|---|
| Process | `ai_development_process.md` | Which step comes next |
| Product scope | `prd_procureaccess_werteit_v*.md` (German) | What and why, P0/P1/P2, non-goals |
| Feature spec | `SPEC_v*.md` | Requirements `R-<AREA>-nn`, acceptance criteria, limits, rate limits, contracts §8, open questions §13 |
| Architecture | `ARCHITECTURE_v*.md` | Modules, flows A–E, domain model, API, ADRs, migration steps §12, open points §16.5 |
| Tests | `TESTING_v*.md` | Invariant catalogue (CI gate), tooling, test levels |
| UI texts | `ux_copy_procureaccess_v*.md` (German) | Every UI string and its key; terminology (§2) |
| UI behaviour | `design_handoff_procureaccess_v1.md` (internal version 1.x) | Tokens, layout, components K1–K6, screens |
| Figma ↔ code | `figma_code_mapping_procureaccess_v1.md` | Component paths, `@figma` tags, CI link check |
| Criteria content | `kriterienkatalog_bit_inklusiv_v1*.md` (German) | Filter layers, exclusions, rating scale, BIT inklusiv texts |

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

**Current state of the code (migration step 0 completed in sprint 01; parts of step 1 built ahead):** backend projects on .NET 10 / EF Core 10 / Npgsql 10 (S1-04) and the API Docker image on the 10.0 base images (S1-06; AutoMapper pinned to 14.0.0 until issue #31), Angular 22 with TypeScript 6.0 on Node 24 LTS / npm 11 (S1-09; `.nvmrc` and the client `Dockerfile` carry the same Node version), zoneless with every component `OnPush` (S1-10), client Docker image serves the release build through nginx (S1-11; `CLIENT_BUILD_CONFIGURATION` in `.env` picks the API: `production` or `production,local`), root solution `ProcureAccess.sln` with the four backend test projects under `tests/` on xUnit v3 and Microsoft.Testing.Platform (S1-12); integration fixture with Testcontainers PostgreSQL 17, Respawn, `WebApplicationFactory`, test authentication handler (`X-Test-User` header) and `FakeTimeProvider`; the API registers `TimeProvider.System`, read so far by `HealthController`, the log retention and the usage-event writer (S1-13, S1-22, S1-23); tagged tests for ARC-06 (Unit) and OPS-03 (Integration) (S1-14; `active` since the gate, S1-21); CAT-10 API characterisation of the criteria filter on the `AddSeedData` seed, expected sets in `tests/ProcureAccess.Tests.Integration/Catalog/Fixtures/cat-10-criteria-filter.json` (S1-15), frontend tests on Vitest (`@angular/build:unit-test`, jsdom) with Angular Testing Library; every `TestBed` gets zoneless change detection from `src/test-providers.ts` (`providersFile`); the generated Karma specs are deleted (S1-16); the first component test `delete-account-dialog.spec.ts` is the example of the convention in §9, with the real German texts from `src/app/testing/provide-test-translations.ts`, and `npm run test:coverage` writes a coverage report (`@vitest/coverage-v8`, text summary + lcov in `coverage/`, not gated) (S1-17); Playwright with `@axe-core/playwright` in `e2e/` (own `package.json`), axe helper `e2e/support/axe.ts` with the central suppression list `e2e/support/axe-suppressions.ts` (every entry links an issue), E2E stack = `docker-compose.yml` + `docker-compose.e2e.yml` (database in memory, API still `Development`, same host ports as the dev stack), smoke test and helper self-test (S1-18); journey J1 in its current form `e2e/journeys/j1-configurator.spec.ts` (CAT-10 E2E part, axe on every state for A11Y-01; expected criteria read from the CAT-10 fixture), one suppression (#52, stepper names), three axe findings fixed (#53–#55) (S1-19); CI in `.github/workflows/ci.yml` runs stages 1–8 of TESTING §8.2 on every pull request to `main` (one job per stage, actions pinned by SHA; `dotnet format` checks whitespace and style on the whole solution and analyzers only on `tests/`; ESLint with the Angular template rules incl. accessibility, no TypeScript rules yet; backend coverage with `coverlet.MTP`, merged with the Vitest lcov by ReportGenerator into the job summary; E2E on all five browser projects with a generated `.env`; stage 7 and the Figma-link check were placeholders until S1-21; npm install scripts of `esbuild`, `lmdb`, `msgpackr-extract`, `@parcel/watcher` are denied in `allowScripts`) (S1-20); invariant gate `scripts/check-invariants.mjs` (reads the catalogue from the highest `docs/TESTING_v*.md`, CI stage 7) and Figma link check `scripts/check-figma-links.mjs` (CI stage 1; the legacy `layout/header/header.ts` is on its `PENDING` list until the step-1 shell); ARC-06, OPS-03, CAT-10 and A11Y-01 are `active` in TESTING_v11 §4 (S1-21); operations basics ahead of step 1 (S1-22): Serilog writes to the console and, through `Serilog.Sinks.Postgresql.Alternative`, to `logging."SeriLogs"` (the migrations own the table, the sink never creates it; settings in `AppLoggingSettings` of `appsettings.json`, so every environment starts, issue #38), `LogRetentionJob` deletes rows older than `AppLoggingSettings:Retention:RetentionDays` (default 90) once at startup and every `RunInterval` (`LogRetentionService`, on `TimeProvider`), `/health/live` (no checks) and `/health/ready` (database) beside the legacy `/api/Health` that the E2E stack still waits on, `RebuildDatabase` runs only in `Development` (`DatabaseRebuildExtensions`), `ASPNETCORE_ENVIRONMENT` of the compose stack comes from `.env` (default `Development`); OPS-01, OPS-02 and SEC-06 are `active` in TESTING_v12 §4; usage events ahead of step 1 (S1-23): table `analytics."UsageEvents"` (date only, no personal reference) and `IUsageEventWriter` with one method per event of SPEC §9.1 (UTC day from `TimeProvider`, own DbContext scope, failures logged and swallowed); nothing calls it yet (S11 and the export come in step 2); PRV-05 is `active` in TESTING_v13 §4 and is the first test of the architecture project. Still legacy until migration step 3: JWT in `localStorage`. Next: migration step 1 (roadmap M1, without the packages already done in S1-22 and S1-23).

## 4. Repository layout

Current:

```
ProcureAccess.sln, global.json      Root solution (all projects incl. tests); global.json only selects the test runner
API/API.ProcureAccess/              ASP.NET Core Web API (controllers, Program.cs, Identity endpoints)
DAL/DAL.ProcureAccess/              EF Core DbContext, repositories, migrations
DAL/MODELS.ProcureAccess/           Entities, DTOs, AutoMapper profile
SERVICES/SERVICES.ProcureAccess/    Data services
CLIENT/CLIENT.ProcureAccess/        Angular SPA
docker-compose.yml, .env            Local full stack (API 5000, client 4200, PostgreSQL 5432)
tests/ProcureAccess.Tests.{Unit,Architecture,Integration,Shared}/   Backend tests; Shared = builders (`TestData.ACriterion()` …), not a test project
e2e/                                Playwright + axe (own package.json): support/ (axe helper, suppressions, global setup), smoke/, journeys/ (J1 current form)
docker-compose.e2e.yml              E2E override of docker-compose.yml (used by e2e/ npm scripts)
.github/workflows/ci.yml            CI pipeline, stages 1–8 of TESTING §8.2
scripts/                            CI scripts without dependencies: check-invariants.mjs (gate), check-figma-links.mjs
docs/                               Project documents (§2)
```

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

Angular features follow `src/app/features/<name>/{models,services/api,state,pages,components,dialogs}`; store slices `withCatalog`, `withConfigurator`, `withWorkspace`, `withDirectory`, `withIdentity`, `withSettings`, `withLoading`. `withProposal()`, `withProducts()`/`withFavorites()` and the `Approver` role are legacy and will be retired or reworked (`ARCHITECTURE §8.1`).

## 5. Commands

```bash
cp .env.example .env                      # once; fill in the secrets (never commit .env)
docker network create dokploy-network     # once; the compose file uses it as an external network
docker-compose up --build                 # full stack (DB + API + client), reads .env; locally CLIENT_BUILD_CONFIGURATION=production,local
curl http://localhost:5000/health/ready   # 200 Healthy when the API reaches the database; /health/live checks nothing else

# API (run from API/API.ProcureAccess). Secrets come only from environment variables:
export ConnectionStrings__ProcureAccess="Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres;Password=<from .env>"
export JWTSettings__Key="<from .env>"     # dotnet run also needs Email__Username / Email__Password to send mail
dotnet build && dotnet run
dotnet ef migrations add <Name> --project ../../DAL/DAL.ProcureAccess --startup-project .
dotnet ef database update     --project ../../DAL/DAL.ProcureAccess --startup-project .
dotnet ef migrations has-pending-model-changes --project ../../DAL/DAL.ProcureAccess --startup-project .   # exit 0 = model matches the last migration (OPS-03)

# Client (run from CLIENT/CLIENT.ProcureAccess)
npm install && npm start                  # http://localhost:4200
npm run build:release
npx ng test                               # Vitest; watch mode in a terminal, single run without one (CI)
npm run test:coverage                     # same, plus coverage summary and lcov report in coverage/procure-access/
npm run lint                              # ESLint: Angular template rules incl. accessibility (CI stage 1)

# Backend tests (run from the repository root). Microsoft.Testing.Platform (global.json) needs --solution
dotnet test --solution ProcureAccess.sln   # integration tests need a running Docker (Testcontainers starts postgres:17-alpine)
#   coverage as in CI: add --coverlet --coverlet-output-format cobertura --coverlet-include "[API.ProcureAccess]*" (one per production assembly, see ci.yml)

# Formatting as checked in CI stage 1 (run from the repository root)
dotnet format whitespace ProcureAccess.sln --verify-no-changes && dotnet format style ProcureAccess.sln --verify-no-changes
dotnet format tests/ProcureAccess.Tests.Unit/ProcureAccess.Tests.Unit.csproj --verify-no-changes   # analyzers too; likewise for each test project

# E2E + axe (run from e2e/; Node from .nvmrc). Needs .env like the dev stack; stop the dev stack first (same ports)
npm install && npx playwright install chromium firefox webkit   # once
npm run stack:up                          # E2E stack: fresh in-memory database, waits until client and API answer
npx playwright test                       # all browsers; e.g. --project=chromium for one
npm run stack:down                        # stop and remove the E2E stack

# Invariant gate and Figma link check (Node, no dependencies; run from anywhere)
node scripts/check-invariants.mjs         # CI stage 7: every `active` invariant of the highest docs/TESTING_v*.md needs an INV:<ID> test
node scripts/check-figma-links.mjs        # CI stage 1: mapped components carry their @figma tag; missing files are skipped
```

CI (`.github/workflows/ci.yml`) runs the commands above on every pull request; every stage except coverage (stage 8) must be green before a merge (sprint Definition of done). When a migration step starts, flip its invariants in the highest `TESTING_v*.md` (CLAUDE §6.4); the gate reads only that file.

Licence check (REUSE, CLAUDE §10): `reuse lint` (install with `pipx install reuse`). New files get a header with `reuse annotate --copyright "Blinden- und Sehbehindertenverein Hamburg e.V." --year <year> --license MIT <file>`; files without comment syntax (JSON, binaries, generated files) are covered in `REUSE.toml`.

`RebuildDatabase` / `SampleDataInitializer` must never run outside `Development` (SEC-06); `DatabaseRebuildExtensions` checks `IHostEnvironment`, not only the flag. Migrations apply on startup (single instance only); so does the log-retention job, which assumes one API instance.

## 6. How to work here

1. **Find the requirement first.** Every change maps to an `R-…` ID in SPEC (or an architecture/test task). Name the IDs in the PR description. If no requirement covers the change, ask.
2. **Work in migration-step order** (`ARCHITECTURE §12`, mapping in `SPEC §11`). Do not build features of a later step on top of structures an earlier step will replace.
3. **Small slices.** One slice should be reviewable in one session: backend + tests + frontend + texts for one requirement or a coherent part of one.
4. **Tests come with the code.** Starting a migration step flips its invariants in `TESTING §4` from `planned` to `active`; the CI gate then requires a test tagged `INV:<ID>` for each. Name tests `Scenario_Condition_ExpectedOutcome` and add `[Trait("Invariant","<ID>")]` plus `INV:<ID>` in the name (`TESTING §8.1`).
5. **UI strings only by key.** Never hard-code German text in templates or code; add missing keys to `de.json` with the exact wording from the UX copy. If a needed key is missing in the UX copy, stop and report it — do not invent wording.
6. **Open decisions stay open.** Open points (SPEC §13, ARCHITECTURE §16.5) and ⚖️ texts may be implemented as written but must not be decided by you. 🔧 values are confirmed defaults (SPEC SD23): always read them from configuration, never hard-code them, and do not change them on your own.
7. **Before each push:** run the code-review checklist (security, correctness, N+1 queries, missing edge cases, a11y), the invariant and Figma-link scripts, and `reuse lint`.

## 7. Architecture rules (enforced by tests — do not work around them)

- **Modular monolith.** A module references other modules only through their `*.Contracts` project, as listed in `ARCHITECTURE §4.3`; Platform may be used by all. Outside `Contracts`, only controllers and the module registration are public. Catalog depends on nothing (ARC-01–03).
- **Workspace and Directory never reference each other's entities.** Flow B goes through `IPublicationGateway`; withdrawal rights through `IEntrySourceAuthorizer`.
- **Exactly three write paths into the directory** (`PublicationService` Flow B, `ContributionService` Flow D, `VendorPublicationService` Flow E), all through **`PublishedEntryFactory`**; only the factory constructs `PublishedEntry`/`PublishedEntryVersion` (PUB-01, ARC-07). Kanal 1 only via Flow E; Flow B is always Kanal 2.
- **Published versions are immutable.** A `SaveChangesInterceptor` rejects changes, except `ContentPurge` on consent withdrawal (PUB-13, PRV-10). Republishing creates a new version of the same entry.
- **Catalog releases are immutable**; configurations and entries are pinned to a release (ADR-002). Matching across releases uses `Criterion.Key`; `NotCoveredByThisRelease` is its own value.
- **Tenant isolation** through EF global query filters on `OrganizationId`; `IgnoreQueryFilters()` only at allow-listed call sites (TEN-01, TEN-02). Resolve the organisation from the resource, never from a client header. Foreign resources → 404, not 403.
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
- **Tokens only:** colours `--mat-sys-*`, spacing `--pa-space-*`; never hex values or px font sizes. Both appearances (light/dark) must work (A11Y-08).
- **Domain components K1–K6** and the shared components of the Handoff carry the product rules; never re-implement their markup ad hoc. Every mapped component and page has a `@figma` JSDoc tag (+ `@spec`, `@invariants`) as in `figma_code_mapping_procureaccess_v1.md`.
- **Accessibility:** use the patterns of Handoff §6–§7: error summary gets focus after submit, field errors linked with `aria-describedby`, live announcements only through `A11yAnnouncerService`, focus to H1 on route change, `aria-disabled` (not `disabled`) for unavailable options with a visible reason, no tooltips as the only source of information, no skeleton screens, no truncation.
- **Component tests** query by role and accessible name (Angular Testing Library) with the real German texts (`provideTestTranslations()`); if something cannot be found that way, fix the accessibility, not the test. Trigger actions without change detection, then `await fixture.whenStable()`: clicks with `element.click()`, other events with `fireEvent` from `@testing-library/dom` — never `fireEvent` from `@testing-library/angular`. Example: `delete-account-dialog.spec.ts`.

## 10. Security and privacy

- Cookie session (`HttpOnly`, `Secure`, `SameSite=Strict`), 30 min idle / 8 h absolute, warning 5 min before; 401 opens the login overlay and retries the request (SEC-01–03, SEC-07).
- Antiforgery on every non-GET endpoint except the anonymous export (SEC-02, SEC-09).
- E-mail confirmation gates saving, organisations, invitations, contributor profiles and publishing (SEC-10); lockout and token rules per SEC-11.
- Rate limits per `SPEC §7.3`; field limits per `SPEC §7.2` (server authoritative).
- Secrets only from environment variables — the repository is public. Licence headers (REUSE/SPDX) on new files; only OSI-licensed dependencies without commercial-use restrictions.

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

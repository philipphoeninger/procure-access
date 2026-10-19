# CLAUDE.md

This file guides Claude Code (and any other AI assistant) when working in this repository. It is the project's AI onboarding document: read it at the start of every session.

> **Version:** v3 (2026-10-19) — step 9 of `docs/ai_development_process.md`, generated from `SPEC_v1.md` and `ARCHITECTURE_v11.md` (v2, 2026-10-15) and aligned with `SPEC_v2.md` / `ARCHITECTURE_v12.md` (v3: the former proposals O-S1–O-S4 and O-S6 are decided; supported browsers added). v1 only described the existing codebase. Keep it updated whenever a migration step changes structure, commands or conventions.

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

**Current state of the code (before migration step 0):** .NET 8, Angular 20, JWT in `localStorage`, Karma/Jasmine, no tests. The upgrade to .NET 10 and Angular 22 is migration step 0 and comes before any new feature code.

## 4. Repository layout

Current:

```
API/API.ProcureAccess/              ASP.NET Core Web API (controllers, Program.cs, Identity endpoints)
DAL/DAL.ProcureAccess/              EF Core DbContext, repositories, migrations
DAL/MODELS.ProcureAccess/           Entities, DTOs, AutoMapper profile
SERVICES/SERVICES.ProcureAccess/    Data services
CLIENT/CLIENT.ProcureAccess/        Angular SPA
docker-compose.yml, .env            Local full stack (API 5000, client 4200, PostgreSQL 5432)
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
docker-compose up --build                 # full stack (DB + API + client), reads .env

# API (run from API/API.ProcureAccess). Secrets come only from environment variables:
export ConnectionStrings__ProcureAccess="Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres;Password=<from .env>"
export JWTSettings__Key="<from .env>"     # dotnet run also needs Email__Username / Email__Password to send mail
dotnet build && dotnet run
dotnet ef migrations add <Name> --project ../../DAL/DAL.ProcureAccess --startup-project .
dotnet ef database update     --project ../../DAL/DAL.ProcureAccess --startup-project .

# Client (run from CLIENT/CLIENT.ProcureAccess)
npm install && npm start                  # http://localhost:4200
npm run build:release
```

From migration step 0 on (update this section when they exist): `dotnet test ProcureAccess.sln`, `npx ng test` (Vitest), `npx playwright test` (in `e2e/`), `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`.

Licence check (REUSE, CLAUDE §10): `reuse lint` (install with `pipx install reuse`). New files get a header with `reuse annotate --copyright "Blinden- und Sehbehindertenverein Hamburg e.V." --year <year> --license MIT <file>`; files without comment syntax (JSON, binaries, generated files) are covered in `REUSE.toml`.

`RebuildDatabase` / `SampleDataInitializer` must never run outside `Development` (SEC-06). Migrations apply on startup (single instance only).

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
- **Component tests** query by role and accessible name (Angular Testing Library); if something cannot be found that way, fix the accessibility, not the test.

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

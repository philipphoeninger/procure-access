# ProcureAccess — Tech-debt register

> **Status:** v1 (2026-10-29) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 01. The skill `Engineering/tech-debt` was not installed in Claude Code; the register follows the same method by hand (inventory → categorise → score → plan).
> **Sources:** open GitHub issues (2026-10-29), the "Open points" and review sections of `docs/sprints/S1-*.md`, a scan of the code (TODOs, suppressed warnings, `DateTime.UtcNow`), `dotnet build` warnings, `dotnet list package --vulnerable/--outdated`, `npm audit`, Dependabot alerts and the repository settings.
> **Next review:** end of sprint 02. Each review writes a new version (`TECH_DEBT_v2.md` …) and moves done items out.

---

## 1. Summary

- **55 items:** the 42 open issues (33 active, 9 superseded, §7) plus 13 items that were only written down in sprint protocols or found in this scan (marked **TD-nn**, §3 and §6). §5 adds the legacy code that a migration step replaces.
- **Most debt is planned.** About half of it lives in legacy code that a migration step replaces anyway: JWT, AutoMapper, generic controllers, legacy client features. That debt gets no extra work; it is listed in §5 so that nobody "fixes" code which is about to go.
- **Live defects come first.** Five items affect the running application or break a project rule today: #37 (product filter 500), #43 (fonts from Google, CLAUDE §8), #78 (Font Awesome licence, D12), #95 (account deletion enabled with an empty e-mail), #113 (dev builds poll the production API).
- **The CI gate was not binding.** `main` had no branch protection (TD-01), so a red invariant gate only showed on the PR. The owner added a ruleset on 2026-10-29.
- **Documentation is drifting.** Twelve "record in the next TESTING / ARCHITECTURE / SPEC version" points from sprint 01 have not reached the documents (TD-11). Claude Code and a successor treat those documents as the truth, so this drift costs more than it looks.
- **Issue hygiene:** nine issues from before the current process (#3–#23) were superseded by SPEC/ROADMAP or done, and the old milestones did not match the roadmap. Both fixed on 2026-10-29 (§7); the new items have issues #116–#124 (§8).

## 2. Method

**Categories:** Code · Architecture · Test · Dependency · Security/Privacy · Accessibility · CI/Ops · Documentation.

**Scores** (1 = low, 5 = high):

- *Impact* — how much it hurts users, the product rules or development speed today.
- *Risk* — what happens if it stays (security, legal, data, handover).
- *Effort* — size of the fix (1 ≈ under an hour, 3 ≈ one session, 5 ≈ several sessions or a migration step).

**Priority = (Impact + Risk) × (6 − Effort)**, from 2 to 50. Above 25 → fix in the next sprint; 15–25 → schedule with the named step; below 15 → fix when the code is touched anyway.

**When** says where the item is resolved: *now* (next sprint), a migration step (`ARCHITECTURE §12`, roadmap milestone), or *touch* (when the code is changed anyway).

## 3. Register

### 3.1 Live defects and rule violations

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #37 | Products filter returns 500 (missing `await` in `ProductsController.ByCriteriaFilterIds`); the client spinner then stays (S1-08 N1) | Code | 4 | 3 | 1 | **35** | now |
| #43 | Icon fonts load from `fonts.googleapis.com` — third-party request from the browser (CLAUDE §8, privacy) | Security/Privacy | 4 | 4 | 2 | **32** | now, with #78 |
| #95 | Delete-account dialog: an empty account e-mail enables "Löschen" at once | Code | 3 | 3 | 1 | **30** | now |
| #78 | Font Awesome icons (CC-BY-4.0) in the bundle conflict with D12 | Dependency | 3 | 4 | 2 | **28** | now, with #43 |
| TD-08 (#120) | Legacy header icon buttons have English accessible names ("Search icon-button" …), `COMMON.ENTER` is English — WCAG 3.1.2, not found by axe (S1-19 P3) | Accessibility | 3 | 2 | 1 | 25 | step 1 shell (needs UX copy wording) |
| #113 | Dev health watcher polls the production API instead of the local one | CI/Ops | 2 | 2 | 1 | 20 | now |
| #90 | Generic CRUD create fails: missing DTO→entity maps, not caught by ARC-06 | Code | 3 | 2 | 2 | 20 | step 1 (or with #31) |
| #52 | Stepper steps on `/filters` have no accessible name (suppressed in axe) | Accessibility | 3 | 2 | 2 | 20 | step 2 rebuilds the screen; earlier only if the owner rates it `blocker` |

### 3.2 Security and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-01 | `main` had no branch protection, so the invariant gate was not binding (S1-20 P1, S1-21 P7). **Done 2026-10-29:** ruleset "Main protection" requires stages 1–8 and CodeQL. Stage 8 (coverage) is required although CLAUDE §5 does not gate it — fine while the job only reports | CI/Ops | 3 | 4 | 1 | ~~35~~ | ✅ done (owner) |
| TD-02 (#116) | No `.github/dependabot.yml` for version updates (NuGet, npm ×2, Actions, Docker); only security alerts are on (S1-20 P2) | Dependency | 2 | 3 | 1 | 25 | now |
| TD-03 (#116) | .NET packages on 10.0.13; 10.0.14 is out (ASP.NET Core, EF Core, Identity, Mvc.Testing); base images likewise | Dependency | 2 | 2 | 1 | 20 | now (Dependabot afterwards) |
| TD-04 (#116) | `vite` 8.3.0 high advisory (dev server only, not shipped); fixed by `@angular/build` 22.2.5, which now exists (S1-16 P2) | Dependency | 1 | 2 | 1 | 15 | now |
| TD-05 (#117) | `NetArchTest.Rules` has had no release since 2021; ARC-01–03 of step 1 depend on it (S1-12 P1). Spike: run one rule against the .NET 10 assemblies; if it fails, the fork needs an architecture decision (CLAUDE §11) | Architecture | 3 | 3 | 2 | 24 | before step 1 |
| #77 | API Dockerfile: Debug build, root user, no restore caching | Security | 2 | 3 | 2 | 20 | step 1 |
| #31 | AutoMapper: v14 has CVE-2026-32933 (High, advisory suppressed in `Directory.Build.props`, 4 open Dependabot alerts), v15+ is RPL/commercial | Dependency | 4 | 4 | 4 | 16 | step 1 (roadmap M1); ARC-06 must then be replaced in TESTING |
| #112 | `/health/ready` is public and opens a DB connection per request; timeout is a code constant (S1-22 P9, P10) | Security | 2 | 2 | 2 | 16 | rate-limit task |
| TD-12 | The E2E job's `compose.log` artefact could contain per-run secrets; harmless while they are random and per run (S1-20 P9) | Security | 1 | 2 | 1 | 15 | watch at step 3 (Mailpit, real secrets) |

### 3.3 Code and architecture

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-07 (#119) | ESLint checks templates only: `typescript-eslint` is installed, but `.ts` files load only its parser (`configs.base`), no rules (S1-20) | Code | 2 | 2 | 2 | 16 | step 1 |
| #115 | Usage events: write fire-and-forget, without retries on the request path (R-MET-01) | Code | 2 | 2 | 2 | 16 | step 2 (first caller) |
| #101 | UX copy key `shell.nav.menu` cannot coexist with `shell.nav.menu.close` in `de.json` | Documentation | 1 | 2 | 1 | 15 | step 1 shell |
| TD-06 (#118) | Production build warnings: about 80 nullable (`CS86xx`, 53 of them `CS8618`), 25 `VSTHRD200`, `AV0027`, a few `CS0168`/`CS0105`/`CS157x`; the CI analyzer check covers only `tests/` (S1-20 P3, P8) | Code | 2 | 2 | 3 | 12 | step 1 (most warnings sit in legacy entities that step 2 replaces) |
| #42 | Bundle budget raised to 2 MB / 2.5 MB; tighten after the rebuild | Code | 2 | 1 | 2 | 12 | M1/M2 |
| TD-09 (#121) | Every log row has `ApplicationName = Unknown`: the enricher reads `ApplicationName`, appsettings set `AppName` (S1-22 P5) | CI/Ops | 1 | 1 | 1 | 10 | now (quick win) or touch |
| #74 | Generated `API.ProcureAccess.xml` is tracked; unused `Microsoft.AspNetCore.OpenApi` | Code | 1 | 1 | 1 | 10 | touch |
| TD-10 (#122) | Legacy `/api/Health` stays because the E2E healthcheck, `global-setup.ts` and `ClockTests` use it (S1-22 P2) | CI/Ops | 1 | 1 | 2 | 8 | step 1 shell |
| #73 | Swagger: decide on `AV0027` and harden `SwaggerDefaultValues` | Code | 1 | 1 | 2 | 8 | step 1 |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #109 | Invariant gate: fail on catalogue header drift, count only test files, require a protocol file | Test | 2 | 2 | 2 | 16 | step 1 |
| #111 | Invariant gate: count only tests that run, require the .NET `Invariant` trait (S1-21 P3) | Test | 2 | 2 | 2 | 16 | step 1 |
| #87 | Session probe must answer 200 for anonymous and unknown test users | Test | 2 | 3 | 3 | 15 | step 3 |
| #88 | Integration host option for the real Identity cookie flow (SEC-01/02/09) | Test | 2 | 3 | 3 | 15 | step 3 |
| #89 | Test that the antiforgery helper's session probe carries the caller's headers | Test | 1 | 2 | 1 | 15 | step 3 |
| #98 | Keep `vitest` and `@vitest/coverage-v8` on the same version | Test | 1 | 2 | 1 | 15 | now (with TD-02) |
| #82 | Regression tests for the S1-10 zoneless fixes (Z1–Z5, R1–R5) | Test | 2 | 2 | 3 | 12 | touch (screens are rebuilt in M1–M3) |
| #92 | CAT-10: `ExistingCatalogSeed` can drift from the migration seed | Test | 1 | 2 | 2 | 12 | step 2 replaces the seed |
| #91 | CAT-10: comparison key ignores JSON value types | Test | 1 | 1 | 1 | 10 | touch |
| #96 | `delete-account-dialog.spec`: cancel from the start, near-miss e-mails | Test | 1 | 1 | 1 | 10 | with #95 |
| #97 | Client coverage: exclude models, DTOs and environment files | Test | 1 | 1 | 1 | 10 | M1 (target structure) |
| #110 | Figma link check: map pages, find moved components, validate `PENDING` files | Test | 1 | 2 | 3 | 9 | step 1 shell |

### 3.5 CI

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #107 | Stage 1: type-check `e2e/` with the Node version of `e2e/.nvmrc` | CI/Ops | 1 | 2 | 1 | 15 | touch |
| #104 | Stage 8: coverage download pattern also matches its own artifact | CI/Ops | 1 | 1 | 1 | 10 | touch |
| #105 | Stage 6: save the Playwright browser cache also when E2E fails | CI/Ops | 1 | 1 | 1 | 10 | touch |
| #106 | Stage 8: skip coverage when the build failed | CI/Ops | 1 | 1 | 1 | 10 | touch |

### 3.6 Documentation

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-11 (#123) | Sprint-01 findings not yet carried into the documents. **TESTING:** `dotnet test --solution` on MTP (S1-12 P2), `postgres:17-alpine` (S1-13 P2), zoneless via `providersFile` (S1-16 P1), component-test convention (S1-17 P3), step-0 E2E deviations and suppression semantics (S1-18 P4/P5), CI decisions D1–D6 (S1-20), gate counts tags not tests (S1-21 P3). **ARCHITECTURE:** Serilog sink package, table owned by migrations, own retention job (S1-22 P4); `UsageEvents` table, UTC day, awaited write (S1-23 P1). **SPEC:** table name `analytics."UsageEvents"` (S1-23 P2), 90-day log retention as a 🔧 value (S1-22 P4). **Figma mapping §4:** point to `scripts/check-figma-links.mjs` (S1-21 P5) | Documentation | 3 | 3 | 2 | 24 | now (one documentation session) |
| #4 | README is 18 lines; a successor needs setup, links to `docs/` and the process | Documentation | 2 | 3 | 2 | 20 | step 16 (`TECHNICAL.md`) — keep open |

## 4. Recommended for sprint 02

Reserve about **15–20 % of the capacity (≈ 6 points)** for debt. §9 has the full plan; the order for sprint 02 was:

| # | Work | Items | Pts |
|---|---|---|---:|
| 1 | ~~Turn on branch protection~~ — done by the owner on 2026-10-29 | TD-01 | 0 |
| 2 | Bug fixes users hit today | #37, #95 (+ #96), #113 | 1 |
| 3 | Self-host the icon fonts and settle the Font Awesome licence in one PR | #43, #78 | 2 |
| 4 | Dependency PR: `@angular/build` 22.2.5, .NET 10.0.14, `dependabot.yml`, vitest pairing | #116, #98 | 1 |
| 5 | NetArchTest spike before the module rules | #117 | 0.5 |
| 6 | Documentation session: TESTING v15, ARCHITECTURE v14, SPEC v3 wording | #123 | 1.5 |

Everything else rides with its migration step: when a step starts, look up its items here (column *When*) next to the step's invariants.

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design. Do not fix it on its own; replace it with the step.

| Debt | Retires with |
|---|---|
| JWT in `localStorage`, `jwtDecode` with `any` in `auth.service.ts` (SEC-01) | step 3 (M3) |
| `DateTime.UtcNow` ×6 (`UserRepo` ×1, `UserService` ×4 — token expiry, `ProposalService` ×1) against CLAUDE §7 | `UserService`/`UserRepo` with step 3, `ProposalService` with the proposal retirement (step 5) |
| `Microsoft.AspNetCore.Identity.UI` only for the `IEmailSender` interface (S1-03) | step 3 |
| Ad-hoc error objects in `BaseCrudController` and `UICustomizationController` ("TODO: handle more gracefully") instead of Problem Details | step 1 (M1.3) |
| ~25 TODOs in legacy client features (favorites, proposal, settings, register, profile, filters) and DAL repos | features retired or rebuilt in M1–M3 (`ARCHITECTURE §8.1`) |
| Architecture test project empty, `--ignore-exit-code 8` (S1-12, S1-20 P7) | step 1 (ARC-01–03) |
| `vitest-axe` not installed (S1-16 P5) | first K-component (M1/M2) |
| E2E without `E2E` environment, seeder, Mailpit, clock endpoint, relaxed rate limit (S1-18 P4) | steps 2–4 |
| CAT-10 on `ExistingCatalogSeed` and the old filter (S1-15 P1) | step 2 (catalog release import) |
| `Approver` role, `withProposal()`, `withProducts()`/`withFavorites()` | per `ARCHITECTURE §8.1` |

Not tracked on purpose: CI has no NuGet/Docker cache (S1-20 P4, add when run times hurt); Serilog stamps with the system clock (S1-22 P6, same clock in production).

## 6. Decisions waiting for the owner

These are not debt to fix but open choices; CLAUDE §6.6 says Claude Code must not decide them.

| Decision | Source | Needed by |
|---|---|---|
| TD-13 (#124): `UsageEvents` uses a sequential `Id`, which orders events within a day; switch to `uuid`? | S1-23 P3 | before the first pilot data (step 2) |
| Severity (`blocker`/`major`/`minor`) on the a11y issues, especially #52 | S1-19 P1 | sprint 02 planning |
| Does tapping the product-type field open the autocomplete on phones? | #102 | manual pass (TESTING §7.2) |
| UX copy wording for the English header names and `COMMON.ENTER` | TD-08 | step 1 shell |

## 7. Issue hygiene

**Superseded issues from before the current process** — closed on 2026-10-29, each with a comment that names its replacement:

| Issue | Replaced by |
|---|---|
| #3 Create DB-Schema | `ARCHITECTURE §5` domain model |
| #5 App / Code Documentation | step 16 `TECHNICAL.md` (the Google Doc link stays in the closing comment) |
| #6 Identity management frontend pages | SPEC R-ACC / roadmap M3 (account deletion included) |
| #7 Identity management sample data | TESTING §6 fixtures; demo users are out of scope per SEC-06 |
| #8 Milestone naming and descriptions | `ROADMAP_v1.md` |
| #11 Implement Docker setup | done (S1-06, S1-11, S1-22: compose, release image, environments) |
| #15 Add accessibility controls | SPEC R-SET (appearance, M1.5 / M3) |
| #16 Settings page with accessibility customisations | SPEC R-SET (M3) |
| #23 Authorization concept | `ARCHITECTURE` roles and tenant isolation (TEN-01/02, step 3) |

**Milestones:** the eight old GitHub milestones ("Identity Management", "Software-Components Filter" …) predated the roadmap. Done 2026-10-29: they are closed, and the milestones **M1 … M7, G-P, G-R** of `ROADMAP_v1.md` exist. Every open issue has the milestone of its *When* column ("now" and "touch" → M1, the current milestone; #4 → G-P), except the question #102. A step's debt now shows up when the step starts.

**Labels:** most open issues carry only `enhancement`, which does not separate debt from features; #78, #112, #113, #115 have no label. Done 2026-10-29: the label `tech-debt` marks the §3 items that are not plain bugs, next to `bug`, `a11y`, `.NET`, `dependencies`.

## 8. New issues

Created on 2026-10-29 for the **TD-** items that need one (owner actions and §5 items do not):

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-02 – TD-04 | #116 Dependency updates: `@angular/build` 22.2.5 (vite advisory), .NET 10.0.14, `dependabot.yml` | dependencies, tech-debt | M1 |
| TD-05 | #117 Check NetArchTest.Rules on .NET 10 before the module rules (ARC-01–03) | tech-debt, .NET | M1 |
| TD-06 | #118 Clean up production build warnings and extend the CI analyzer check to all projects | tech-debt, .NET | M1 |
| TD-07 | #119 ESLint: turn on the TypeScript rules (`typescript-eslint` recommended) | tech-debt | M1 |
| TD-08 | #120 a11y: English accessible names on the header icon buttons and `COMMON.ENTER` | a11y, bug | M1 |
| TD-09 | #121 Logs: `ApplicationName` is always `Unknown` (`AppName` vs `ApplicationName`) | bug | M1 |
| TD-10 | #122 Move the E2E readiness checks to `/health/ready` and remove `/api/Health` | tech-debt | M1 |
| TD-11 | #123 Carry the sprint-01 findings into TESTING, ARCHITECTURE, SPEC and the Figma mapping | documentation, tech-debt | M1 |
| TD-13 (§6) | #124 Decide: random key for `UsageEvents` before pilot data | question | M2 |

## 9. Fix plan — when to fix what

Every open finding of sprint 01, by the moment it should be fixed: the issues of §3, the planned debt of §5, the decisions of §6, and the open points of `docs/sprints/S1-*.md` that have no issue (named by protocol and point, e.g. S1-13 P4). The GitHub milestones carry the same plan for the issues: at the start of a step, the milestone shows what belongs to it.

Already resolved by the pull requests of this review (once merged): #116 and #98 (dependency PR #126), #123 (documentation PR #127, incl. SPEC v2.1).

### 9.1 Sprint 02 — first M1 sprint, debt budget ≈ 6 points

| Do | Items | Pts |
|---|---|---:|
| Fix the live bugs | #37 (+ spinner S1-08 N1), #95 with #96, #113 | 1 |
| Self-host the icon fonts and settle Font Awesome in one PR | #43, #78 | 2 |
| Quick fix in any logging PR | #121 | 0.5 |
| NetArchTest spike **before** the module rules | #117 | 0.5 |
| Owner: set the severity of the a11y issues; plan the phone check | §6: #52, #102 | 0 |
| Merge the PRs of this review; review the first Dependabot PRs (licence check, CLAUDE §10) | #125 – #127 | 0.5 |

### 9.2 M1 — step 1, together with the work package that touches the code

| Work package | Fix with it | Why then |
|---|---|---|
| M1.1 Module projects | #31 AutoMapper (and say in TESTING what replaces ARC-06), #90 with it; #118 build warnings + analyzer check on all projects; #109, #111 gate tightening and removing `--ignore-exit-code 8` when ARC-01–03 arrive (§5); move `UsageEvent` and its writer to the Platform module (S1-23 P6) | The code moves anyway; the gate gets its first architecture tests |
| M1.2 / ops | #77 Dockerfile hardening; #74; #73 | Small, same files |
| M1.3 Problem Details | Ad-hoc error objects in `BaseCrudController` / `UICustomizationController` (§5) | Replaced by the middleware |
| M1.4 Shell | #120 (needs UX copy wording first), #101, #110 and the `PENDING` header entry (S1-21 P4), #122 (`/api/Health` → `/health/ready`) | The legacy header and its E2E checks are replaced |
| First new client code | #119 TypeScript lint rules; #97 coverage exclusions; `vitest-axe` with the first K-component (§5); decide on `@testing-library/user-event` with the first keyboard tests (S1-17 P2) | New code should start clean |
| Any CI pull request | #104, #105, #106, #107 | One-liners in `ci.yml` |

### 9.3 M2 — step 2 (catalog, configurator, export)

| Fix | Items | Why then |
|---|---|---|
| Before the writer gets its first caller | #124 decision (key type), then #115 (fire-and-forget, retry delay S1-23 P4) | No pilot data exists yet |
| With the configurator rebuild | #52 (stepper), J1 rewrite (S1-19 P5), CAT-10 rewrite incl. #91, #92 and the optional empty-selection case (S1-15 P1, P3; S1-21 P6) | The screens and the filter are replaced |
| With the first rate limit (export, M2.7) | #112 incl. the health timeout (S1-22 P9, P10) | Same mechanism |
| With the export | No request bodies of the anonymous export in the logs (S1-22 P7, EXP-05); relaxed E2E rate limit and E2E seeder (S1-18 P4) | Needed by the export and its journey |
| End of M2 | #42 bundle budget | After the screen rebuild |

### 9.4 M3 – M7

| Milestone | Fix | Items |
|---|---|---|
| M3 | Cookie session replaces the JWT; `Identity.UI`, the `DateTime.UtcNow` calls in `UserService`/`UserRepo` and the legacy account pages go with it (§5) | §5 |
| M3 | Integration host for the real cookie flow; session probe; `TestUsers` as stored rows (S1-13 P5) | #87, #88, #89 |
| M3 | Mailpit and real secrets in the E2E stack: check the `compose.log` artefact (TD-12); dev and E2E stacks share host ports (S1-18 P6) | TD-12 |
| M3 | Correlation ID and the logging rules of PRV-04 (S1-22 P7) | — |
| M1 – M3 | Regression tests for the zoneless fixes, screen by screen as it is rebuilt | #82 |
| M4 | First time-dependent tests: the fake clock is shared and only moves forward (S1-13 P4); E2E clock endpoint for J5 (S1-18 P4) | — |
| M5 | Proposal retirement: `ProposalService` (`DateTime.UtcNow`, `VSTHRD101`), `withProposal()`, `Approver` (§5) | §5 |
| M7 | File-store check in `/health/ready` (S1-22 P3) | — |

### 9.5 Before the pilot (G-P)

| Fix | Items |
|---|---|
| README and `TECHNICAL.md` (process step 16), incl. the indicator queries of R-MET-02 (S1-23 P5) | #4 |
| The axe suppression list is empty (TESTING §12.4) | #52 and any later entry |
| Manual accessibility pass (TESTING §7.2) | #102 |

### 9.6 Watch items — act only when the trigger happens

| Trigger | Then | Source |
|---|---|---|
| E2E timeouts show up in CI | Raise `workers` or timeouts, not before | S1-19 P7, S1-20 P6 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |
| A test mixes seed rows with fixed ids and generated ids in one table | Advance the identity sequence in the test | S1-15 P4 |
| Model drift makes all integration tests red | Accepted: the log names the cause; suppress the warning in the fixture only if the noise matters | S1-14 P1 |
| Docker builds fail on the developer machine | Free disk space (E2E images ≈ 2 GB) | S1-13 P1, S1-18 P7 |
| WebKit is needed locally on Arch | Run it in the Playwright container image | S1-18 P1 |
| The next version of SPEC, ARCHITECTURE, handoff or UX copy is written | Update its reference to the current TESTING version | S1-21 P2 |

### 9.7 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`: move done items out, add new findings from the sprint protocols and reviews, re-score.
- **Start of every migration step:** open its GitHub milestone and plan its debt together with the step's invariants (§9.2 – §9.4).
- **End of every migration step:** review the axe suppression list (TESTING §12.4).
- **Weekly:** Dependabot pull requests — check the licence of new versions before merging (CLAUDE §10).

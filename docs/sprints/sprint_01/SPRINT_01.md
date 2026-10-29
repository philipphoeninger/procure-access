# Sprint 01 — Platform upgrade and test foundation

> **Status:** plan (2026-10-19) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v1.md` (milestone M0, packages M0.1–M0.6), `TESTING_v10.md` (§3 tooling, §8 CI, §9 step 0), `ARCHITECTURE_v12.md` (§12 step 0, §10.3 pipeline), `CLAUDE_v3.md`.
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.

---

## Sprint goal

**The existing application runs unchanged on .NET 10 and Angular 22 (zoneless), and every test level plus the CI pipeline exists, with the four step-0 invariants active and green.**

This is roadmap milestone **M0** in full. It is also the calibration sprint: its measured velocity is the first basis for the roadmap forecast (`ROADMAP_v1.md` §6).

### Success criteria (= M0 exit criteria)

- [ ] Backend builds and runs on .NET 10 / EF Core 10 / Npgsql 10; frontend builds and runs on Angular 22 without Zone.js.
- [ ] The application behaves as before: sign-in, criteria filter and existing export work in a manual smoke test (the JWT login stays until M3).
- [ ] Test projects for unit, architecture and integration tests, Vitest + Angular Testing Library, and Playwright with axe exist and each contains at least one real test.
- [ ] CI stages 1–8 (TESTING §8.2) run on every pull request; the invariant gate is active.
- [ ] Invariants **ARC-06, OPS-03, CAT-10, A11Y-01** have status `active` and tagged, passing tests. Existing axe violations are recorded as issues and either fixed or suppressed with an issue link (TESTING §9, step 0).

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | ~35 points | Documentation, WERTE.IT coordination and reviews are outside these days |
| **Total** | **8 days** | **~35 points** | |

**Assumption, not a measurement:** ~35 points per sprint (≈ 4–5 points per development day with Claude Code) is a starting guess. The real value is recorded at the end of this sprint (see "Close-out").

**Planned load:** 28 points committed (**80 %** of the assumed capacity) + 4 points stretch.

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimate. One task ≈ one Claude Code session and, as a rule, one pull request. Order = recommended working order.

### P0 — committed (28 points)

#### M0.6 Repository hygiene — 2 points (first, because the repository is public)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-01 | Secrets audit: no credentials in `appsettings*.json`, `.env` or history-relevant files; all secrets from environment variables; `.env.example` with placeholders; `.env` in `.gitignore` | 1 | A search for keys/passwords in the repository finds nothing; app starts from `.env` |
| S1-02 | `LICENSE`, REUSE/SPDX headers on existing source files (`reuse lint` passes); `CLAUDE.md` (= `CLAUDE_v3.md`) in the repository root; project documents in `docs/` | 1 | `reuse lint` green; Claude Code picks up `CLAUDE.md` |

#### M0.1 Backend to .NET 10 — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-03 | Inventory: target frameworks, NuGet packages and their licences; upgrade notes for .NET 9 and 10 breaking changes that touch the code; **check the licence of the installed AutoMapper version** (newer major versions changed their licensing; CLAUDE §10 requires OSI licences without commercial-use restrictions) | 1 | Short upgrade note in the PR; AutoMapper decision: keep the current version pinned, or open an issue to replace it |
| S1-04 | Retarget all projects to `net10.0`; ASP.NET Core, EF Core 10, Npgsql 10 and other packages updated; fix compiler errors and new warnings | 2 | `dotnet build` without errors; no new warnings left unexplained |
| S1-05 | Migrations: apply all migrations to an empty PostgreSQL 17 database; `dotnet ef migrations has-pending-model-changes` reports none (prepares OPS-03) | 1 | Both checks pass locally |
| S1-06 | Docker: SDK/runtime base images 10.0; `docker-compose up --build` works; manual smoke test (sign-in, criteria filter, export) | 1 | Smoke test protocol in the PR |

#### M0.2 Frontend to Angular 22 — 8 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-07 | Pre-check: Node version required by Angular 22 (local and Dockerfile); list of third-party packages and their Angular 22 support (Material, NgRx Signals, ngx-translate, others) | 1 | List in the PR; blockers raised before S1-08 |
| S1-08 | `ng update` 20 → 21 (core, CLI, Material, CDK); fix breaking changes; build and smoke test | 2 | App builds and runs on 21 |
| S1-09 | `ng update` 21 → 22; update NgRx Signals and ngx-translate; build and smoke test | 2 | App builds and runs on 22 |
| S1-10 | Zoneless: `provideZonelessChangeDetection()`, remove `zone.js` from polyfills; fix places that relied on Zone.js (async results into signals, `markForCheck` where needed); run the standalone migration schematic for any remaining NgModules | 2 | No `zone.js` in the bundle; all existing screens update correctly in the smoke test |
| S1-11 | `npm run build:release` and the client Docker image work; smoke test via docker-compose | 1 | Smoke test protocol in the PR |

#### M0.3 Backend test foundation — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-12 | Root solution `ProcureAccess.sln` including `tests/ProcureAccess.Tests.{Unit,Architecture,Integration,Shared}`; packages xUnit v3, Shouldly, NSubstitute, NetArchTest; builders skeleton in `Shared` | 1 | `dotnet test ProcureAccess.sln` runs (empty suites allowed) |
| S1-13 | Integration fixture: Testcontainers PostgreSQL 17, Respawn reset, `WebApplicationFactory` host, test authentication handler, `TimeProvider` registered in the app and replaced by `FakeTimeProvider` in tests, shared `HttpClient` with an antiforgery helper hook (used from M3) | 2 | A sample integration test calls an existing endpoint against a real database |
| S1-14 | **ARC-06** (AutoMapper configuration valid) and **OPS-03** (migrations apply to an empty database, no pending model changes) as tagged tests | 1 | Both green with `INV:` tag and trait |
| S1-15 | **CAT-10** characterisation test, API part: the existing criteria filter returns a fixed set of criteria for a fixed seed (subset comparison on stable fields) | 1 | Green; the expected set is stored as a fixture file |

#### M0.4 Frontend test foundation — 3 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-16 | Replace Karma/Jasmine with Vitest (Angular builder) and Angular Testing Library; delete obsolete generated specs; `npx ng test` runs in CI mode | 2 | No Karma/Jasmine dependency left; test run green |
| S1-17 | First real component test that queries by role and accessible name; Vitest coverage output | 1 | Example test documents the convention (CLAUDE §9) |

#### M0.5 E2E, a11y and CI — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S1-18 | `e2e/` with Playwright and `@axe-core/playwright`; axe helper with a central suppression list (each entry links an issue); E2E stack in docker-compose | 2 | `npx playwright test` runs against the compose stack |
| S1-19 | Journey J1 in its **current** form (CAT-10, E2E part) with axe on every visited state (**A11Y-01**); existing violations recorded as issues and fixed or suppressed | 1 | J1 green; issues created for all suppressions |
| S1-20 | GitHub Actions: stages 1–8 of TESTING §8.2 (lint and licences incl. `dotnet format`, ESLint with template a11y rules and `reuse lint`; build; unit; architecture; integration; E2E + a11y; invariant gate; coverage report) | 1 | Pipeline green on a pull request |
| S1-21 | `scripts/check-invariants.mjs` (reads the **highest** `docs/TESTING_v*.md`, see risk R4) and `scripts/check-figma-links.mjs` (passes while no mapped components exist); step-0 invariants set to `active` | 1 | Gate fails on a deliberately untagged active invariant and passes after tagging |

### P2 — stretch (4 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S1-22 | Operations basics: Serilog in its own schema with retention, `/health` endpoints, `RebuildDatabase` guard on `IHostEnvironment` | 3 | M1.2 |
| S1-23 | `UsageEvent` table and writer (no events yet) | 1 | M1.7 |

Stretch items are pulled in order; if one is started but not finished, it goes back to the M1 backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | S1-01, S1-02, S1-03, S1-07 | Inventories done; blockers in third-party packages known |
| First half | D1–D4 | S1-04 – S1-06, S1-08 – S1-11 | **Mid-sprint check (end of D4):** both upgrades merged (15 points done). If not: drop the stretch and decide whether S1-19/S1-20 move to sprint 02 |
| Second half | D5–D8 | S1-12 – S1-21 | Gate active, all step-0 invariants green |
| Buffer | remaining | stretch S1-22, S1-23 | – |

Backend (S1-03 – S1-06) and frontend (S1-07 – S1-11) upgrades are independent; if one is blocked, switch to the other instead of waiting.

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| S1-12 – S1-15 | backend upgrade S1-04 (tests are built once on the target version, ARCH §12 step 0) | internal |
| S1-16, S1-17 | frontend upgrade S1-09 | internal |
| S1-18 – S1-20 | both upgrades and S1-06/S1-11 (compose stack) | internal |
| S1-20 | GitHub repository with Actions enabled; CodeQL and Dependabot switched on in the repository settings | Philipp (settings) |
| — | No external dependency on the project team or WERTE.IT in this sprint | – |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | A third-party Angular package has no Angular 22 version yet | S1-09 blocked | S1-07 finds it on D1; options: pin and patch, replace, or stop at 21 for this sprint and finish 22 in sprint 02 |
| R2 | Zoneless switch breaks screens that relied on Zone.js | S1-10 takes longer | Manual smoke test per screen; the screens are replaced from M1/M2 on anyway — fix only what is needed for "behaves as before" |
| R3 | AutoMapper licence of the installed version does not meet the licence rule | ARC-06 may test a library that has to go | Keep the version that is compliant pinned; if none fits, open an issue to replace AutoMapper in M1 and keep ARC-06 until then |
| R4 | TESTING §8.3 assumes `docs/TESTING.md`, but the documents are versioned | Gate parses the wrong or no file | S1-21 reads the highest `TESTING_v*.md`; record the change in the next TESTING version |
| R5 | Existing axe violations on the current screens are numerous | S1-19 grows | Suppress with issue link as TESTING §9 allows; the screens are rebuilt in M1/M2 |
| R6 | The velocity guess (35) is wrong | Over- or under-commitment | 20 % buffer plus stretch; measure and record actual velocity at close-out |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6) done.
- [ ] CI pipeline green (once S1-20 exists; before that, build and tests locally).
- [ ] Tests for invariants carry `INV:<ID>` and the trait (TESTING §8.1).
- [ ] New files carry SPDX headers; no secrets committed.
- [ ] Documentation touched by the change updated (e.g. commands in `CLAUDE.md` §5 once `dotnet test`, `npx ng test`, `npx playwright test` exist).

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; inventories (S1-03, S1-07) |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: success criteria checked, smoke test on the compose stack |
| D8 | Retro (short): what slowed the upgrade, how well the task size fit one session |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Velocity for forecasting | |
| Carried over to sprint 02 (with reason) | |
| Issues created (axe suppressions, AutoMapper, others) | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** M1 Foundations (33 points), minus any stretch items finished here.

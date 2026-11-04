# S2-19 — E2E readiness on `/health/ready`, remove `/api/Health`

> **Sprint:** 02, stretch task S2-19 (0.5 points; roadmap M1.4d, TECH_DEBT TD-10). **Issues:** #122. **Requirements / invariants:** OPS-01 stays green; no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** the E2E client healthcheck, `e2e/support/global-setup.ts` and `ClockTests` no longer use `/api/Health`; the endpoint and its controller are removed (S1-22 open point P2). Not in scope: protecting or rate-limiting `/health/ready` (#112), the file-store check of `/health/ready` (step 7).
> **Owner decisions 2026-11-04 (before the task, asked by Claude):** **1** nothing outside the repository (Dokploy, uptime monitoring) calls `/api/Health`, so it can go. **2 = A** no nginx change: the client container's healthcheck asks `http://api:8080/health/ready` over the stack network, `global-setup.ts` asks the API's host port; `/health/ready` stays off the client origin (#112). **3 = default** `ClockTests` checks the clock without HTTP, on the `TimeProvider` the API resolves.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `HealthController` (`GET /api/Health`, anonymous, `{ status, time }`) was the only reader of `TimeProvider` outside Platform and had three users: the E2E client healthcheck (`http://127.0.0.1/api/Health` through nginx), `global-setup.ts` (`${clientUrl}/api/Health`) and `ClockTests` (the `time` field) | All three moved; controller deleted |
| F2 | The client's nginx forwards only `/api/`; `/health/ready` on the client origin returns the SPA's `index.html` (200, `text/html`), so a check there would always pass | The checks ask the API directly (decision 2 = A) |
| F3 | The aspnet image has no `wget`/`curl` (S1-22 P2), so the API container cannot check itself; the client image (Alpine) has BusyBox `wget` and resolves `api` through Docker's DNS, as nginx already does | Healthcheck stays on the client container |
| F4 | `/health/ready` returns no time, so `ClockTests` could not simply switch endpoints (the task's extra instruction "point ClockTests at /health/ready") | Owner decision 3: the test resolves `TimeProvider` from the API's services |
| F5 | No document outside the sprint protocols, `TECH_DEBT_v1.md`, `ROADMAP_v2.md` and ARCHITECTURE_v15 C93 names `/api/Health`; TESTING, SPEC and the Figma mapping are not affected | CLAUDE.md §3/§5 and `e2e/README.md` updated; the rest see §5 |

## 2. Changes

| File | Change |
|---|---|
| `API/API.ProcureAccess/Controllers/HealthController.cs` | Deleted: `/api/Health` now answers 404 like every unknown route (Problem Details, S2-10) |
| `docker-compose.e2e.yml` | Client healthcheck: `wget http://127.0.0.1/` (nginx) **and** `wget http://api:8080/health/ready` (API with database); header comment updated |
| `e2e/support/global-setup.ts` | Checks the client URL and `new URL('/health/ready', E2E_API_URL)`; `E2E_API_URL` is now the API origin, default `http://localhost:5000` (a value that still ends in `/api` works too, because the absolute path replaces it) |
| `tests/ProcureAccess.Tests.Integration/FixtureTests/ClockTests.cs` | `TimeProvider_AfterAdvancingFakeClock_ReturnsFakeClockTime`: resolves `TimeProvider` in a scope of the test API, checks it is the fixture's `FakeTimeProvider` and returns the advanced time |
| `CLAUDE.md` | §3: `TimeProvider` readers, health endpoints and E2E readiness, legacy wrapper sentence; §5: `stack:up` comment; version line |
| `e2e/README.md` | What `--wait` and `global-setup.ts` check |

## 3. Self-review

Checklist of CLAUDE §6.7 (with *Engineering/code-review*):

| Area | Result |
|---|---|
| Security | One anonymous endpoint less. No new public path: `/health/ready` stays reachable only on the API's own port, as before; the client origin does not forward it (#112 unchanged) |
| Correctness | The healthcheck fails when nginx does not answer, when the API does not answer, and when the database is down (checked by hand: 404 → `wget` exit 1, database stopped → 503 → exit 1). `global-setup.ts` names `http://localhost:5000/health/ready` when the API is down (checked with only the client running) |
| Trade-off (decision 2 = A) | The readiness checks no longer go through the `/api/` proxy. J1 and the shell tests call the API through it on every run, so a broken proxy still fails E2E |
| N+1, performance | Not affected |
| Accessibility | No UI change |

Findings, sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `ShouldBeSameAs(Clock)` makes the time check of `ClockTests` partly redundant | *Won't fix*: the time check gives the clearer failure message if the fixture ever wraps the clock |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln` | 0 errors, 112 warnings (#118; this change adds none) |
| `dotnet test --solution ProcureAccess.sln` | 135 passed (unit, architecture, integration incl. OPS-01 and `ClockTests`) |
| `dotnet format` whitespace + style (solution), analyzers (integration tests) | clean |
| `npm run stack:up` (E2E) | client `healthy` with the new check |
| `npx playwright test` chromium, firefox, mobile-chrome | 117 passed; WebKit and mobile Safari only in CI |
| `e2e`: `tsc -p .` | clean |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors |
| `reuse lint` | compliant (660 / 660) |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | #112 (`/health/ready` public on the API port, one database connection per request) stays open; this task neither widens nor fixes it | With the rate-limit task (SPEC §7.3) or the deployment hardening |
| P2 | ARCHITECTURE_v15 C93 says "`/api/Health` is removed with #122"; the next ARCHITECTURE version can record it as done | Next ARCHITECTURE version |
| P3 | `TECH_DEBT_v1.md` TD-10 and `ROADMAP_v2.md` M1.4d still list #122 as open | Sprint close-out (process step 14, `TECH_DEBT_v2.md`) |

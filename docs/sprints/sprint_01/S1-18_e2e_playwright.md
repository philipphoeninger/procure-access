# S1-18 — Playwright with axe, central suppression list, E2E stack in docker-compose

> **Sprint:** 01, task S1-18 (milestone M0.5, TESTING §3.3, §3.4, §5.4, §7.1, §9 step 0, §12.4). **Status:** done by Claude Code (2026-10-23).
> **Scope:** `e2e/` with Playwright and `@axe-core/playwright`; axe helper with a central suppression list (each entry links an issue); E2E stack as a docker-compose override; a smoke test of the stack and a self-test of the helper. Not in scope: journey J1 and axe on the real screens, filling the suppression list (S1-19); CI (S1-20); A11Y-01 to `active` (S1-21); the full E2E environment of TESTING §3.3/§6.3 (seeder, Mailpit, clock endpoint, relaxed rate limit: steps 2–4).
> **Owner decisions 2026-10-23:** **a** the API keeps running as `Development` in the E2E stack; **b** no Mailpit yet, the SMTP credentials are only cleared; **c** the first tests are a stack smoke test and the helper self-test, no axe check on real screens.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No `e2e/` folder, no Playwright | Built from scratch as TESTING §3.4 lays out (own `package.json`, `support/`, later `journeys/`) |
| F2 | `docker-compose.yml` has a fixed `container_name` for the database, a persistent volume, and the external network `dokploy-network` (absent in CI) | Override with `!reset`: no container name, no volume (tmpfs instead), no published DB port, stack-local network |
| F3 | The client bundle calls `http://localhost:5000/api` (configuration `production,local`) | The E2E stack uses the same host ports 5000/4200 as the dev stack; only one of them can run |
| F4 | Roles are created only in the `RebuildDatabase` branch (`Development`); the criteria seed comes from the migrations | Decision a. An `E2E` environment needs its own seeder (TESTING §6.3), later |
| F5 | SMTP credentials from `.env` reach the API | Override sets `Email__Username`/`Email__Password` to empty (decision b) |
| F6 | The API image has no `curl`/`wget`; the nginx client image has `wget` and proxies `/api/` to the API | Healthcheck on the client through its proxy covers client and API, so `up --wait` returns when both answer |
| F7 | In the Alpine client image `localhost` resolves to `::1` first; nginx listens on IPv4 only | Healthcheck uses `127.0.0.1` (the first attempt with `localhost` stayed unhealthy) |
| F8 | The home page's product cards are hard-coded in `home.html`, not loaded from the API | The smoke test checks `/products` for data from the API |
| F9 | Arch Linux is not supported by Playwright; Chromium and Firefox run, WebKit lacks Ubuntu libraries (`libicu74`, `libxml2`, `libflite1`, `libmanette-0.2-0`, `libenchant-2-2`) | WebKit and mobile Safari cannot run on this machine; they run in CI (S1-20). See P1 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docker-compose.e2e.yml` (new) | Project name `procureaccess-e2e`; db on tmpfs, not published, no fixed container name; stack-local network; SMTP credentials cleared; client healthcheck `/` + `/api/Health` | F2–F7 |
| `e2e/package.json`, `package-lock.json`, `.nvmrc` (24.22.1), `tsconfig.json`, `.gitignore` | `@playwright/test` 1.64.1 (Apache-2.0), `@axe-core/playwright` 4.14.0 (MPL-2.0, brings `axe-core` 4.14.0), `typescript` ~6.0.3, `@types/node` ^24. Scripts `stack:up`, `stack:down`, `test`, `typecheck`, `report` | TESTING §3.3, §3.4 |
| `e2e/playwright.config.ts` | Projects `chromium`, `firefox`, `webkit`, `mobile-chrome` (Pixel 7), `mobile-safari` (iPhone 15); `baseURL` from `E2E_BASE_URL` (default `http://localhost:4200`); locale `de-DE`; trace and screenshot on failure; `retries: 0` | SPEC §7.4, CLAUDE §9 |
| `e2e/support/global-setup.ts` | Checks client and `E2E_API_URL`/Health (default `http://localhost:5000/api`) once; stops the run with "npm run stack:up" if either is down | One clear error instead of every test timing out |
| `e2e/support/axe.ts` | `expectNoAxeViolations(page, testInfo, state)`: runs axe with `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa` (blocking) and `wcag22aa` (reported only); applies the suppressions; attaches `axe-<state>.json` (blocking, suppressed, reported); annotations for suppressed and WCAG 2.2 findings; the failure message names rule, impact, target, HTML and help link | TESTING §3.3, §5.4, A11Y-01 |
| `e2e/support/axe-suppressions.ts` | Central list (empty). Entry = rule + selector + issue link + reason; a node is suppressed if the rule matches and its element matches the selector or lies inside a match. `validateAxeSuppressions` rejects missing fields, links that are not issues of this repository, and duplicates; the helper validates before every run | TESTING §9 step 0, §12.4 |
| `e2e/support/axe-helper.spec.ts` | 9 tests on a fixed HTML page with two violations: clean page passes; unsuppressed violations fail with rule and element; suppression applies only to its rule and selector (also inside a container); all suppressed passes; central list valid; invalid entries rejected; helper refuses an invalid list | Proves the helper before S1-19 relies on it |
| `e2e/smoke/stack.spec.ts` | 2 tests: client served with German texts; `/products` lists the seeded product "CoreFlow" from the API | "Done when" |
| `e2e/README.md` | How to run, browser note, layout | Handover |
| `.dockerignore` (root) | `e2e/` | Keeps test reports out of the API build context |
| `CLAUDE.md` §3, §4, §5 | Current state, layout, E2E commands | CLAUDE §11 |

No production code, UI text or backend/client dependency changed. No test carries `INV:A11Y-01` yet: the invariant is covered by J1 with axe on every state (S1-19) and activated in S1-21.

## 3. Checks

Node 24.22.1 / npm 11.19.1 from `.nvmrc`; Docker 29.9.1 / Compose 5.6.0.

| # | Check | Result |
|---|---|---|
| 1 | `npm install` in `e2e/` | ✅ 8 packages; `npm audit`: 0 vulnerabilities |
| 2 | `npm run typecheck` | ✅ |
| 3 | `npm run stack:up` (first build ≈ 5 min) | ✅ after F7; db, api, client healthy; later starts ≈ 7 s |
| 4 | `npx playwright test` — `chromium`, `firefox`, `mobile-chrome` | ✅ 33/33 (11 tests × 3) |
| 5 | `npx playwright test` — all five projects | 39 passed, 16 failed: every failure is `browserType.launch` on `webkit`/`mobile-safari` ("Host system is missing dependencies", F9); the 3 browser-free list tests pass there too |
| 6 | Negative: API container stopped | ✅ run stops in global setup: "E2E stack not reachable at http://localhost:5000/api/Health … npm run stack:up" |
| 7 | Negative: stack down | ✅ same message for `http://localhost:4200` |
| 8 | Probe (temporary spec, deleted): helper on the real home page | Helper works on the app: 1 blocking violation, `label-content-name-mismatch` (serious) — input for S1-19 |
| 9 | `npm run stack:down` | ✅ containers, tmpfs data and network removed; no volume created |
| 10 | `docker compose config --quiet` (dev stack, without the override) | ✅ unchanged |
| 11 | `dotnet build ProcureAccess.sln --no-incremental` / `dotnet test --solution ProcureAccess.sln` | ✅ 0 errors, 136 warnings (unchanged) / ✅ 22/22 |
| 12 | `npx ng test` / `npm run build:release` | ✅ 2 files, 5 tests / ✅ 1.81 MB initial, only the known EasyMDE selector warning |
| 13 | `reuse lint` (6.2.0) | ✅ compliant, 489/489 (before this note) |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | WebKit/mobile Safari run only where Playwright's Ubuntu libraries exist. Locally on Arch: `--project=chromium --project=firefox --project=mobile-chrome`. Fallback if needed: the official image `mcr.microsoft.com/playwright:v1.64.1-noble` with `--network host` (≈ 2 GB; the root file system has 2 GB free after the image builds) | S1-20 (CI runs all five) |
| P2 | CI needs a `.env`: `docker-compose.yml` requires `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `JWT_KEY`, `CLIENT_BUILD_CONFIGURATION` (`${VAR:?}`). Generate it in the workflow from `.env.example` with random secrets. `!reset` needs Compose ≥ 2.24 | S1-20 |
| P3 | `npx playwright install --with-deps` in CI; cache `~/.cache/ms-playwright` per Playwright version; upload `playwright-report/` on failure. `retries` stays 0 until flakiness is seen | S1-20 |
| P4 | Deviations from TESTING §3.3/§6.3/§6.4 for step 0: no `E2E` environment, no E2E seeder (the API runs `RebuildDatabase` and `SampleDataInitializer` as in development; no test relies on its users), no Mailpit, no clock endpoint, no relaxed rate limit. Record in the next TESTING version and build with steps 2–4 | Next TESTING version; steps 2–4 |
| P5 | Suppression semantics ("element matches the selector or lies inside a match") should be written into TESTING §12.4; a container selector like `body` would suppress a rule almost globally, so review entries for narrow selectors | Next TESTING version; review in S1-19 |
| P6 | The E2E and dev stacks share host ports 5000/4200 (F3). A same-origin API (`/api` behind nginx, S1-11 P4) would remove the API port and let the browser tests run inside a container | M3 |
| P7 | Disk: the E2E images (`procureaccess-e2e-api`, `-client`) take space on the root file system (2.0 GB free after the build). Remove with `docker image rm` if needed; `stack:up` rebuilds them | Philipp |
| P8 | Home page: `label-content-name-mismatch` (check 8); S1-09 N5 expected more. S1-19 records the real list | S1-19 |

## 5. Review follow-up (2026-10-28)

Code review of PR #51 (10 findings), sorted per CLAUDE §6.8. None affects the product: #51 changed no production code. Fixed in `sprint01/S1-18-review-fixes`:

| # | Finding | Fix |
|---|---|---|
| R1 | Suppression matching looked the reported element up again (`document.querySelector(target)` in a separate `page.evaluate`) after `analyze()` had finished. A re-render in between finds no element, so a suppressed violation blocks intermittently, or a positional selector finds a different element and the wrong node is suppressed | `e2e/support/axe.ts`: for a blocking violation whose rule has entries, axe runs that one rule again with the entries' selectors excluded (`AxeBuilder.withRules([rule]).exclude(…)`). axe resolves the selectors in the same run in which it checks the elements; the nodes of that run block. Semantics unchanged: excluded elements' descendants are excluded too; selectors apply to the top document only. The extra run happens only when a suppressed rule fires (today `aria-tab-name`, #52). Nodes of the first run that the rerun no longer reports are listed as suppressed (report only); with several entries for one rule, their issues are listed together |

Already fixed after the merge (S1-11 review fixes, same-origin API):
- R2 — the E2E client build took its API address from `CLIENT_BUILD_CONFIGURATION` in `.env`, so a production value would point the tests at the live API. The variable no longer exists; the bundle calls `/api` on its own origin, and `global-setup.ts` checks the API through the client.
- R3 — `E2E_BASE_URL` other than `http://localhost:4200` failed CORS. Same cause, same fix.

Won't fix:
- R4 — `Email:Host` stays `smtp.lettermint.co` with cleared credentials. No E2E test sends mail yet (J1 needs no account), and without credentials nothing can be sent. Mailpit for the E2E stack is planned (P4, TESTING §6.3).
- R5 — a selector like `body` would suppress a rule on the whole page. Already P5: the rule "never a rule globally" is documented, and entries are reviewed.
- R6 — an invalid selector fails with a raw `SyntaxError` instead of the list error. Still a loud failure.
- R7 — entries that match nothing are not reported. The list is reviewed at the end of every migration step.
- R8 — one browser round-trip per violating node (now: one axe run per suppressed rule that fires). Only rules with entries are affected, today one rule on two elements.
- R9 — `e2e/.nvmrc` repeats the client's Node version (both `24.22.1`, CI uses each on purpose). Drift risk at the next Node bump, not a bug.
- R10 — the helper self-test needs the running stack (global setup) and runs once per browser project. Convenience and run time only.

Checks: `npx tsc --noEmit` in `e2e/` ✅; full suite on the E2E stack: chromium, firefox, mobile-chrome 45/45 ✅ (webkit and mobile-safari cannot start on this Arch host, P1; CI runs them); J1 still lists both stepper headers as suppressed (#52) ✅. J1 with `--repeat-each` on branch and `main` (52 runs each, up to 6 workers): all passed.

Observed once, not caused by this change: in one J1 run the first axe run reported `label-content-name-mismatch` on the toolbar button `.filters-btn` (`aria-label="Filters icon-button"`, content the Material Symbols ligature `action_key`). axe ignores an icon ligature only when the icon font has rendered. The icon fonts come from Google Fonts (`src/index.html`, #43); when that request fails, the ligature stays visible text and the rule fires. Reproduced by blocking `fonts.gstatic.com` (then `.products-btn` fires too); delaying it does not reproduce it, because `page.goto` waits for the fonts. Waiting in the helper would therefore not help; the fix is #43 (self-hosted icon fonts, M1.5). Not reproduced in 104 further runs.

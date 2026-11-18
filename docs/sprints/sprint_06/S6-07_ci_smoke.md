<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-07 — CI smoke: `ng serve` and the immutable header of lazy chunks

> **Sprint:** 06, task S6-07. **Issues:** #388 (TD-73). **Requirements / invariants:** none; no invariant activated, no new TESTING version.
> **Status:** done by Claude Code (2026-11-18) — pull request open; E2E left to CI. No EF migration.
> **Scope:** exactly the sprint row: a CI step that starts `ng serve` and checks that the app renders (#264), and a check against the client image that main, styles and a lazy chunk carry `Cache-Control: immutable` (#308).
> **Owner decisions:** none. The run could not ask the owner; the plan below was followed without a stop.

## Plan (written before the code)

- **Starting point:** `main` at bcca089. CI never starts `ng serve` (#264 returned unnoticed in principle) and never looks at the cache header (#308).
- **Files:** `.github/workflows/ci.yml` (two steps), `scripts/smoke-dev-server.mjs` and `scripts/check-chunk-cache.mjs` with a `node:test` self-test each, `docs/CODEBASE_v1.md` (§2.85, §3, §4), this protocol.
- **Design:** the dev-server smoke goes into stage 1 (the job already has `npm ci` for the client and for `e2e/`): `ng serve`, then a headless Chrome from the runner opens `/` and `/filters` and fails on a 5xx, a page error or no `<h1>`; Playwright comes from `e2e/node_modules`, so no dependency. The cache-header check goes into the E2E job once (project `chromium`), because the E2E stack already builds the client image: no extra image build.
- **Invariants / tests / migration:** none activated; the scripts have self-tests; no EF migration; no new dependency.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | #264 (S5-02): `ng serve` built, but the browser got a 500 for a pre-bundled dependency and nothing rendered; release build, Vitest and E2E never use the dev server | Needs a real browser against a real `ng serve` |
| F2 | #308 (S5-01): the nginx rule `-[A-Za-z0-9_-]{8}\.(js\|css)$` had to be widened by hand once; lazy chunks are named `chunk-<hash>.js`, referenced in `main-*.js` as `import(\`./chunk-<hash>.js\`)` | The check finds a lazy chunk the same way, from the main bundle |
| F3 | The E2E job builds and starts the client image (nginx + release build) per browser project | The header check runs there, once |
| F4 | `ng serve` in stage 1 has no API: the dev server's proxy answers 502/504 for `/api` and `/health`, and the app calls them on load | The smoke must not count those paths; checked locally: `/` and `/filters` render without an API |
| F5 | ESLint globals for `scripts/*.mjs` are `console` and `process` only | `node:http`, `Buffer` from `node:buffer`, `URL` from `node:url`; no config change |

## 2. Changes

| File | Change |
|---|---|
| `.github/workflows/ci.yml` | Stage 1, last step of `lint`: „Dev server smoke (ng serve renders)“ — runs both self-tests, starts `ng serve` in the background, waits up to 180 s for `/`, runs the smoke, prints the `ng serve` log on failure, stops the server. Stage 6, after „Start E2E stack“: „Cache headers of the client image“ (`if: matrix.project == 'chromium'`) |
| `scripts/smoke-dev-server.mjs` (+ `.test.mjs`) | Opens `/` and `/filters` in Chrome (`channel: 'chrome'`, or `CHROME_PATH`); failure = 5xx outside `/api` and `/health`, an uncaught page error, or no visible `<h1>` within 60 s. Self-test: the 5xx rule (the browser part is exercised by the CI step) |
| `scripts/check-chunk-cache.mjs` (+ `.test.mjs`) | Reads `index.html`, takes `main-*.js`, `styles-*.css` and one lazy chunk from the main bundle, requires HTTP 200 and the exact header on all three. Self-test: parsing, the header rule, the whole check against a local server (passes; each of the three files fails on its own) |
| `docs/CODEBASE_v1.md` | §2.85, the `scripts/` line of §3, three command lines in §4 |

Not changed: CLAUDE.md (no rule or command that applies across the codebase), TESTING (no invariant; §8.2 stage text is listed in P1), ARCHITECTURE, SPEC, Figma mapping, `LEGACY_EXCEPTIONS`, `nginx.conf`, the client.

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | `/filters` is a legacy route and will be removed with the legacy filter | **Won't fix** | It is the route that reproduces #264 today; the script header and CODEBASE §2.85 say to name another lazy route with a package import then (P2) |
| R2 | The dev-server smoke depends on the runner's Google Chrome (`channel: 'chrome'`) | **Won't fix** | Preinstalled on `ubuntu-24.04`; installing Playwright's Chromium would add ≈ 150 MB of download to every run. If the image drops Chrome the step fails loudly, not silently |
| R3 | The header check runs on the E2E stack's client, which is the production nginx image with the release build | **Fix applied** | It is the image the rule lives in; run for `chromium` only so 4 other jobs add nothing |
| R4 | The smoke would pass if a page error were thrown before the listener attached | **Won't fix** | Listeners attach before `goto`; a first-script failure also shows as a 5xx or a missing `<h1>` |
| R5 | A page error from the missing API (unhandled rejection of an HTTP call) could fail the smoke | **Checked** | Locally the dev server logged such rejections (`/api/Products` 502) yet no `pageerror` reached the page; `/` and `/filters` passed. CI has the same condition (no API) |

Security: the CI step only starts a local dev server on the runner; no secret, no new permission, no new action. Licences: no new dependency (Playwright is the existing e2e dev dependency, Chrome is the runner's).

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `node --test` of both self-tests | 8 tests pass |
| C2 | Dev-server smoke against a real local `ng serve` (Node 24, chromium at `/usr/bin/chromium`, no API) | `/` and `/filters` rendered; about 2.7 s for the script itself |
| C3 | Negative: dev-server smoke against a server that answers 500 for its script | Exit 1: „HTTP 500 for /main.js“ and „no visible `<h1>`“ |
| C4 | Negative of #264 itself: removing the `externalDependencies` entry from `angular.json` | Not reproduced: the dev server of Angular 22.2.6 renders `/filters` even then (the pre-bundle exists). So the smoke guards the class of failure (5xx, no render), not that one setting; the entry was restored |
| C5 | `check-chunk-cache.mjs` against the local release build served with the nginx pattern | Passed (main, styles, one lazy chunk); the self-test covers each file without the header |
| C6 | `npm run typecheck` and `npm run lint` in `e2e/` (covers `scripts/*.mjs`) | Green |
| C7 | `node scripts/check-invariants.mjs` | Green |
| C8 | Figma, i18n, icons, `reuse lint` | Not affected beyond SPDX headers on the new files (present); CI stage 1 runs them |
| C9 | CI | See the pull request; added time in P3 |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | TESTING §8.2 describes the stages; it does not yet name the two smokes | Next TESTING version (sprint-end documentation session S6-09; no version was written here to avoid a clash with parallel tasks) |
| P2 | Replace `/filters` in the default paths of `smoke-dev-server.mjs` when the legacy filter is removed | Legacy filter removal |
| P3 | Added CI time | Measured on the pull request's run: see the report |

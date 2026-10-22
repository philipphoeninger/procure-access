# S1-11 — Release build and client Docker image

> **Sprint:** 01, task S1-11 (milestone M0.2, ARCH C37 / §10.1 / §12 step 0). **Status:** done by Claude Code (2026-10-22); manual smoke test by the owner passed (2026-10-22, §5).
> **Scope:** `npm run build:release` passes; the client Docker image builds and serves the release build; smoke test on the compose stack. Items handed over by earlier tasks: production budget (S1-08 – S1-10), Dockerfile build command (S1-09 N3), global CLI install (S1-07), nginx proxy port (S1-06 K7), stale `main.js` after a redeploy (S1-10 O6).
> **Owner decisions 2026-10-22:** **1a** raise the bundle budget to the measured size instead of lazy-loading routes; **2a** separate `local` build configuration for the compose stack, chosen through a `.env` variable because Dokploy builds from `docker-compose.yml`; **3** npm install scripts (S1-09 N1) stay open for S1-20.

## 1. Starting point

| # | Finding | Evidence |
|---|---|---|
| F1 | `npm run build:release` fails: initial bundle 1.81 MB (373 kB transferred) > 1 MB error budget. Every route is eager, so the start bundle holds all of it: Angular Material 608 kB, CodeMirror + EasyMDE ≈ 325 kB, CDK 153 kB, app code 120 kB (esbuild metafile) | Build log; `--stats-json` of a budget-free copy in the scratchpad |
| F2 | `environment.prod.ts` points at `https://api.procureaccess.tech/api`. Switching the image to the production build unchanged would make the local compose stack call the production API | `src/environments/` |
| F3 | Dockerfile: unpinned global `@angular/cli`; `npm run build --configuration=development` passes the flag to npm, not to `ng` (N3); `nginx:stable-alpine` floating tag | `CLIENT/CLIENT.ProcureAccess/Dockerfile` |
| F4 | nginx proxies `/api/` to `api:5000`, the API listens on 8080 (K7, unused); no cache headers, so `index.html` can be served from the browser cache after a redeploy (O6) | `nginx.conf` |
| F5 | The client build context does not exclude `.angular/` (build cache) | `CLIENT/CLIENT.ProcureAccess/.dockerignore` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `angular.json` | Initial budget warning 500 kB → **2 MB**, error 1 MB → **2.5 MB** | Decision 1a. Fits today's 1.81 MB with ~10 % headroom for warnings. No SPEC/TESTING requirement sets a budget; the screens are rebuilt in M1/M2, which is when the budget should be tightened (open point P1) |
| `angular.json` | `allowedCommonJsDependencies: ["easymde", "reflect-metadata"]` | Silences the two known CommonJS warnings only for these two packages; any new CommonJS dependency still warns |
| `angular.json`, `src/environments/environment.local.ts` (new) | Build configuration `local` that only replaces the environment file: `production: true`, `apiUrl: http://localhost:5000/api`. Used as `--configuration=production,local` | Decision 2a: optimised, hashed release build for the local stack without touching the production environment |
| `Dockerfile` | No global CLI; `ARG NG_CONFIGURATION=production`; `RUN npm run ng -- build --configuration="${NG_CONFIGURATION}"`; `nginx:1.30-alpine` | F3; the default is the production build, so an image built without the argument is a production image |
| `docker-compose.yml` | Client build argument `NG_CONFIGURATION: ${CLIENT_BUILD_CONFIGURATION:?…}` | Dokploy builds from this file, so the value must come from the environment. **Required** variable: a missing value stops compose with an error instead of silently building against the wrong API |
| `.env.example` | `CLIENT_BUILD_CONFIGURATION=production,local` with both options explained | Local default |
| `nginx.conf` | `index.html` and unhashed files: `Cache-Control: no-cache` (revalidate with ETag); `*-XXXXXXXX.js/css` and `/media/` (content-hashed): `max-age=31536000, immutable`, missing ones → 404 instead of `index.html`; proxy to `api:8080`; `^~` on `/api/` and `/media/` | F4 (O6, K7) |
| `.dockerignore` (client) | `.angular` | F5 |
| `CLAUDE.md` §3, §5 | Current state and compose command mention the new variable | CLAUDE §11 |

No application code, template, UI text or dependency changed.

## 3. Build, tests and checks (Claude Code, 2026-10-22, Node 24.22.1 / npm 11.19.1)

| # | Check | `main` (S1-10) | Branch |
|---|---|---|---|
| 1 | `npm run build` (development) | ✅ | ✅ 0 errors, 0 warnings |
| 2 | `npm run build:release` | ❌ budget error, 1.81 MB | ✅ 1.81 MB initial (373.37 kB transferred); one warning left: an unparsable `50%` selector in the EasyMDE stylesheet (third-party, only affects critical-CSS inlining) |
| 3 | `ng build --configuration=production,local` | – | ✅ same output size; `localhost:5000` in `main`, no `api.procureaccess.tech` outside the dev-only health-watcher chunk (never loaded when `production: true`) |
| 4 | API address in the release build (`build:release`) | – | ✅ only `api.procureaccess.tech`, no `localhost:5000` |
| 5 | `docker compose build --no-cache client` | ✅ (development build) | ✅ release build; npm warning N3 gone. Remaining npm output: deprecations (`@angular/animations` S1-09 N4; `glob`/`inflight`/`rimraf` from Karma, S1-16) and the install-scripts notice (N1, S1-20) |
| 6 | Image built without the argument (Dockerfile default) | – | ✅ only `api.procureaccess.tech` in the bundle; temporary image removed |
| 7 | `docker compose config` without `CLIENT_BUILD_CONFIGURATION` | – | ✅ stops: "required variable CLIENT_BUILD_CONFIGURATION is missing a value" |
| 8 | Images | node `24.22-alpine` (24.22.1 = `.nvmrc`), nginx `stable-alpine` (1.30.6) | node unchanged; nginx `1.30-alpine` (1.30.6) |
| 9 | `nginx -t` inside the compose stack | – | ✅. Outside the compose network it fails with "host not found in upstream api" — same on `main` (existing behaviour) |
| 10 | HTTP checks on `http://localhost:4200` | – | ✅ `/` and deep link `/filters`: 200, `text/html`, `no-cache`; `main-*.js`, `styles-*.css`, `/media/*.woff2`: `immutable`; unknown `main-AAAAAAAA.js`: 404; `/i18n/de.json`: JSON, `no-cache`; `/api/CriteriaFilters` through the nginx proxy: 200 |
| 11 | `npx ng test` (Karma) | ❌ does not compile (`save-search-dialog.spec.ts` imports a missing `./update-user-dialog`) | ❌ same compile error; specs are replaced in S1-16 |
| 12 | `dotnet build ProcureAccess.sln` / `dotnet test` | ✅ / no test projects | ✅ 0 errors, 136 warnings (unchanged, backend not touched) / no test projects (S1-12) |
| 13 | `reuse lint` (reuse 6.2.0) | ✅ | ✅ compliant, 463/463 files (incl. this protocol) |

## 4. Browser check (Claude in Chrome, compose stack, release build)

Throwaway account `smoke-s111@example.test` on the local stack only (created with `POST /api/signUp`).

| # | Step | Result |
|---|---|---|
| 1 | `/home` loads `main-TI2MVM2K.js`; `window.ng` (dev-mode debug API) and `window.Zone` undefined | ✅ production mode |
| 2 | Sign in with a wrong password | ✅ error snackbar, not signed in |
| 3 | Sign in with the correct password | ✅ header shows favourites, profile, sign-out; user appearance applied |
| 4 | Full reload on `/filters` | ✅ still signed in |
| 5 | Filter step 1: expand all, tick app type, product part, test type | ✅ counter follows; app-only test types disabled by the exclusions |
| 6 | "Enter" → criteria list; expand 5.2 | ✅ 87 criteria with priority; markdown rendered (paragraphs, list) — `typedjson` deserialisation works after minification |
| 7 | By URL: `/products`, `/product/1`, `/profile`, `/settings` | ✅ 17 rows; CoreFlow with website link; e-mail shown; language "Deutsch" |
| 8 | Settings: dark mode on and off (not saved) | ✅ |
| 9 | `/criterion-proposal/0`: type into the markdown editor | ✅ CodeMirror holds the typed text |
| 10 | Network on a reload of `/home` (12 s) | ✅ API calls only to `localhost:5000`; no request to `api.procureaccess.tech`; hashed files from the browser cache, `/home` revalidated. ⚠️ two font requests to `fonts.gstatic.com` (O1) |
| 11 | Console | ✅ no errors or warnings (the 36 NgRx dev-mode warnings of S1-09 are gone in production mode) |
| 12 | Sign out | ✅ header shows the sign-in icon |

## 5. Manual smoke test (owner)

Start the stack (once: `docker network create dokploy-network`; your `.env` needs `CLIENT_BUILD_CONFIGURATION=production,local`, see `.env.example`):

```bash
docker compose up --build -d
docker compose ps                        # db healthy, api + client up
# client http://localhost:4200 · API http://localhost:5000 · Swagger http://localhost:5000/swagger
docker compose down                      # stop; add -v to drop the database volume
```

The API re-seeds the database on every start (`Development`), so accounts from earlier runs are gone.

| # | Step | Expected | Result (owner, 2026-10-22) |
|---|---|---|---|
| 1 | `curl -sI http://localhost:4200/filters` | 200, `Cache-Control: no-cache` | ✅ |
| 2 | Open `http://localhost:4200`, DevTools → Network and Console open | Home page loads `main-<HASH>.js` (hashed name); no errors in the console | ✅ |
| 3 | Register a new account; sign in with a wrong password; sign in correctly; reload | Error message for the wrong password; signed in after the reload | ✅ |
| 4 | `/filters`: tick filters of each type, "Enter", expand a criterion | Counter follows; criteria list with priorities; markdown text formatted | ✅ |
| 5 | `/products` and a product's details by URL | Table filled; details show name and website | ✅ |
| 6 | `/settings`: dark mode, language | Theme switches; language "Deutsch" | ✅ |
| 7 | Network tab, filter `procureaccess.tech` | No requests (all API calls go to `localhost:5000`) | ✅ |
| 8 | *Optional (redeploy, S1-10 O6):* change a visible text locally (e.g. a value in `public/i18n/de.json` or a template), `docker compose up --build -d client`, then a **normal** reload (no hard reload); revert the change afterwards | The change is visible; the page loads the new `main-<HASH>.js` if a template changed (compare with `curl -s http://localhost:4200/ \| grep -o 'main-[A-Z0-9]*\.js'`) | – optional, not reported |
| 9 | Sign out | Header shows "Anmelden" | ✅ |

## 6. Observations — existing behaviour, not changed

| # | Observation | Evidence | Proposed follow-up |
|---|---|---|---|
| O1 | The browser loads Material Icons and Material Symbols from `fonts.googleapis.com` / `fonts.gstatic.com`, which CLAUDE §8 ("no third-party requests from the browser") does not allow | `src/index.html` lines 15–16 (since commit `b6b5134`) | Issue #43; self-host the icon fonts with the theme foundation (M1.5) |
| O2 | The dev-only health watcher polls `https://api.procureaccess.tech/health`, also in local development | `src/dev/dev-reload-health.ts` | Point it at `environment.apiUrl` or remove it in M1. With the `local` configuration the local stack no longer runs it (`production: true`) |
| O3 | nginx resolves `api` at startup, so the client container only starts where an `api` host exists | `nginx -t` outside compose | None while the client runs next to the API; revisit with the same-origin setup in M3 |

## 7. Open points

| # | Point | When |
|---|---|---|
| P1 | Tighten the bundle budget once the screens are rebuilt (lazy routes, fewer Material modules) | M1/M2 — issue #42 |
| P2 | **Dokploy:** set `CLIENT_BUILD_CONFIGURATION=production` in the Dokploy environment **before** this branch is deployed; without it the compose build stops (by design). Until now Dokploy built the development image, which called `http://localhost:5000/api` | ✅ Done by the owner, 2026-10-22 |
| P3 | npm install scripts (S1-09 N1): approve or deny `esbuild`, `lmdb`, `msgpackr-extract`, `@parcel/watcher` | S1-20 (decision 3) |
| P4 | Same-origin API (`/api` behind nginx) instead of a separate API host | M3 (cookie auth, ARCH §8.3, §10.1) |

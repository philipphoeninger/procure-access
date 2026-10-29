# S1-08 — Angular 20 → 21 and smoke test

> **Sprint:** 01, task S1-08 (milestone M0.2, ARCH C37 / §8.2, migration step 0). **Status:** done (2026-10-22) — builds, migrations and browser smoke test on the compose stack passed.
> **Scope:** `ng update` 20 → 21 for core, CLI, Material and CDK, the third-party packages that have to move with it, and removal of the unused packages listed in S1-07. No source code changed. Angular 22, TypeScript 6 and Node 24 follow in S1-09.

## 1. Versions

| Package | Before | After | Why |
|---|---|---|---|
| `@angular/*` (core, common, compiler, forms, router, platform-browser, animations, compiler-cli) | 20.3.4 | 21.2.25 | Task |
| `@angular/build`, `@angular/cli`, `@angular-devkit/core` | 20.3.5 | 21.2.27 | Task |
| `@angular/material`, `@angular/cdk` | 20.2.14 | 21.2.14 | Task (Material 20 requires Angular `^20`) |
| `@ngrx/signals` | 20.0.1 | 21.1.1 | Peer `@angular/core ^20` → install fails on 21 |
| `ngx-markdown` | 20.1.0 | 21.3.0 | Peer `@angular/* ^20` → install fails on 21 |
| `marked` (new direct dependency) | 16.4.2 (transitive) | 17.0.6 | Peer of `ngx-markdown` 21 (`^17 \|\| ^18`), S1-07 risk 4 |
| Node / npm / TypeScript | 22.20.0 / 10.9.3 / 5.9.3 | unchanged | Angular 21 accepts Node `^22.12` and TypeScript 5.9 |

**Removed (unused, S1-07 §2, owner OK 2026-10-22):** `@ngrx/store`, `@ngrx/router-store`, `@ngrx/store-devtools`, `@angular/platform-browser-dynamic`, `flatted`, `jsonpath`, `comment-json`. None is referenced in `src/` or `angular.json`; the three `@ngrx/store*` packages would also have blocked the install (peer `^20`).

**Kept, correcting S1-07:**
- `@angular/animations` is **not** unused. `app.config.ts` calls `provideAnimationsAsync()` from `@angular/platform-browser/animations/async`, which needs the package. Updated to 21.
- `ngx-color-picker` is **not** used. The import in `settings.ts` and the markup in `settings.html` are commented out, so it is not in the bundle and S1-07 risk 3 does not apply. Kept in `package.json` (owner decision in S1-07); removing it is a one-line change whenever wanted. *Removed on 2026-10-27 (owner decision after the review of PR #39, §7).*

## 2. Breaking changes and how they were fixed

No breaking change needed a change to application code. All Angular, Material, CDK and NgRx migration schematics ran; only one of them changed a file.

| # | Breaking change | Fix |
|---|---|---|
| B1 | `ng update @angular/core@21 @angular/cli@21` updates `package.json` but the install fails: Material/CDK 20, `@ngrx/signals` 20, `@ngrx/store*` 20 and `ngx-markdown` 20 declare peer `@angular/core ^20` | Raised Material/CDK, `@ngrx/signals`, `ngx-markdown` + `marked` in the same step; removed the unused `@ngrx/store*` packages |
| B2 | npm 10.9.3 (also the latest 10.9.9) crashes while resolving the new tree: `Cannot read properties of null (reading 'edgesOut')` in arborist `#loadPeerSet`, on the optional `vitest` peer group of `@angular/build` 21. Also happens without a lockfile | Lockfile generated with npm 11 (`npx npm@11 install --package-lock-only`, lockfile version 3 as before). npm 10.9.3 can `npm ci` and `npm install` from this lockfile without changing it, locally and in the `node:22-alpine` image. Only *adding or changing* packages with npm 10 still crashes → use `npx npm@11 install …` until Node 24 (npm 11) arrives in S1-09 |
| B3 | Regenerating the lockfile also moved unrelated direct packages (Font Awesome 7.3.1, easymde 2.21.0, prettier 3.9.10) | Pinned back to the versions on `main` (7.2.0, 2.20.0, 3.6.2) in the lockfile; `package.json` ranges unchanged. *That pin did not last (the next lockfile regeneration would undo it); since 2026-10-27 `package.json` pins the three exactly (§7).* Transitive packages are re-resolved within their ranges (that is what fixes most of the audit findings, §3). `@types/jasmine` moved 5.1.9 → 5.1.15 (patch, test-only, replaced in S1-16) |
| B4 | `ng update` runs the *latest* CLI (22) to perform the update when the local CLI is not the latest; CLI 22 refuses Node 22.20 (`^22.22.3 \|\| ^24.15`) | Migrations run with the installed CLI 21: `NG_DISABLE_VERSION_CHECK=1 npx ng update <pkg> --migrate-only --from=20.0.0 --to=21.2.27 --allow-dirty` (`--allow-dirty` because `.ai/` is untracked) |
| B5 | Migration "Updates the 'lib' property in tsconfig files" removed `"lib": ["ES2022", "dom"]` from `tsconfig.json` | Accepted: without `lib`, TypeScript uses the default library of `target: ES2022` (`lib.es2022.full.d.ts`): ES2022, DOM, DOM.Iterable, DOM.AsyncIterable, ScriptHost and WebWorker.ImportScripts. That is a superset of the old `["ES2022", "dom"]`, so the code is now type-checked against a few more APIs (e.g. iterating a `NodeList` with `for…of`). Emitted JavaScript and build output unchanged. *(Corrected 2026-10-27; the first version called the default equivalent.)* |

### Migrations that ran

| Package | Migration | Result |
|---|---|---|
| `@angular/cli` | remove default karma config; `moduleResolution: bundler`; update `lib` | only `lib` changed (B5) |
| `@angular/core` | `BootstrapContext` for SSR; `ApplicationConfig` import; bootstrap options → providers; block control flow; `Router.lastSuccessfulNavigation` | no changes |
| `@angular/material`, `@angular/cdk` | update to v21 | no changes |
| `@ngrx/signals` | rename `withEffects` → `withEventHandlers` | no changes (not used) |

**Not run (optional, would be refactoring):** `@angular/cli` `use-application-builder` (the app already uses `@angular/build:application`), `@angular/core` `router-current-navigation` (`getCurrentNavigation` is not used in `src/`).

## 3. Build, tests and checks (Claude Code, 2026-10-22)

| # | Check | `main` (Angular 20) | Branch (Angular 21) |
|---|---|---|---|
| 1 | `npm run build` (development; used by `npm start` and the Dockerfile) | ✅ | ✅ 0 errors, 0 warnings |
| 2 | `npm run build:release` (production) | ❌ initial bundle 1.72 MB > 1 MB budget error | ❌ same error, 1.76 MB (366.6 kB transferred, +6 kB); same warnings as `main` (budget, CommonJS `reflect-metadata` and `easymde`, one CSS selector). S1-11 |
| 3 | `npx ng test` (Karma, all 22 specs) | ❌ does not compile: `save-search-dialog.spec.ts` imports a missing `./update-user-dialog` | ❌ same compile error |
| 4 | Karma, the other 21 spec files (broken spec excluded through a temporary tsconfig outside the repository) | 22 of 22 tests fail | 22 of 22 tests fail, **identical set**. Cause: generated "should create" specs without providers (`NG0201` for `API_URL`, `JWT_NAME`, `MatDialogRef`). Specs are replaced in S1-16 |
| 5 | `npm audit` | 73 (3 low, 16 moderate, 51 high, 3 critical); 22 in runtime dependencies | 9 high, all in dev tooling (`@angular/cli` → MCP SDK, `karma` → `braces`, `vite`); **0** in runtime dependencies |
| 6 | `docker compose build --no-cache client` (`npm ci` with npm 10 in `node:22-alpine`) | – | ✅ |
| 7 | Markdown output: all 127 criterion descriptions rendered with `marked` 16.4.2 and 17.0.6 | – | ✅ identical HTML for 127 of 127 |
| 8 | `reuse lint` | ✅ | ✅ compliant |

Run in the browser: `ng-version` = 21.2.25, `window.Zone` undefined (zoneless, as before).

## 4. Smoke test (browser, Claude in Chrome, compose stack)

Stack: `docker compose up --build -d` (client image rebuilt without cache), `RebuildDatabase` seed. Throwaway account `smoke-s108@example.test`; the sign-up confirmation mail went to Lettermint's test sink `ok@testing.lettermint.co` (K5, owner OK).

| # | Step | Expected | Result |
|---|---|---|---|
| 1 | Home page | Loads with texts from `de.json` and three product cards from the API | ✅ |
| 2 | Login dialog (`(login:auth)`), "Hier registrieren" | Material form fields render; register form opens | ✅ |
| 3 | Register | Snackbar "Registrierungsbestätigung gesendet", back to login | ✅ |
| 4 | Sign in with a wrong password | Error snackbar, `POST /api/signIn` 400, not signed in | ✅ |
| 5 | Sign in with the correct password | Header shows favourites, profile, sign-out; user appearance applied | ✅ |
| 6 | Full page reload (`/filters`) | Still signed in | ✅ |
| 7 | Criteria filter: expand all, select app type, product part, test type | Counter "3 ausgewählt"; the existing exclusions disable app-only test types | ✅ |
| 8 | "Enter" → criteria list | `GET /api/Criteria/byCriteriaFilterIds` 200; expanded criterion shows rendered markdown (86 blocks: paragraphs, lists, bold, links) | ✅ |
| 9 | "Produkte anzeigen" | As before: 500 from the API (K1, #37), error snackbar | ✅ unchanged behaviour (see N1) |
| 10 | Products page via the header | Material table with all products | ✅ (see N2 for a direct URL) |
| 11 | Export button (step 2 of the filter) | As before: link to `./` with `download="products.csv"` (K2). The click was intercepted in the page and logged instead of downloading | ✅ unchanged behaviour |
| 12 | Settings: dark mode toggle (not saved) | Theme switches immediately, and back | ✅ |
| 13 | Sign out | Header shows "Anmelden"; home page loads | ✅ |
| 14 | `docker compose restart api`, reload `/filters` | App works again | ✅ |
| 15 | Final image (after B3 pin) rebuilt, home page | Angular 21.2.25, cards and icons render, no console error | ✅ |

The only failed API requests in the session were the expected 400 (step 4) and 500 (step 9). The console showed three `HttpErrorResponse` entries; two belong to these requests, the third could not be tied to a request (the console tool repeated entries with shifted timestamps; it also listed the single `NG04002` from N5 several times).

## 5. Observations — existing behaviour, not caused by the upgrade

| # | Observation | Evidence | Proposed follow-up |
|---|---|---|---|
| N1 | After the 500 in step 9 the loading spinner stays: `lastValueFrom` throws, so `decrementLoadingCount()` is never reached | `features/products/state/with-products.ts` (both load methods) | Goes away with #37; error handling of the store slices is reworked in M1/M2 |
| N2 | `/products` opened by URL shows an empty table: `MatTableDataSource(this.store.products())` takes a one-time snapshot before the products have loaded | `features/products/list/products-list.ts:44` | Zoneless/signal fix candidate for S1-10 ("existing screens update correctly") |
| N3 | `@ngrx/signals` logs "SignalStore members cannot be overridden" ~36 times on start (`loadingCount`, `isLoading`, …): several slices each include `withLoading()` | Same warning exists in `@ngrx/signals` 20.0.1 | Store slice restructuring (ARCH §8.1) |
| N4 | The dev-only health watcher polls `https://api.procureaccess.tech/health` (the production host) every 5 s from the local browser and logs "API unreachable" | `src/dev/dev-reload-health.ts` | Point it at `environment.apiUrl` or remove it (CLAUDE §8: no third-party requests from the browser) |
| N5 | `/register` typed as a URL logs `NG04002` (the route exists only in the named `login` outlet); the in-app link works | `app.routes.ts` | None needed now |
| N6 | `npm ci` warns: `@angular/animations` is deprecated in 21 (use `animate.enter` / `animate.leave`) | `app.config.ts` uses `provideAnimationsAsync()` | Decide in S1-09/S1-10 whether Material still needs it; replacing it is a change of its own |

## 6. Open points for S1-09 and later

- **npm:** until Node 24 / npm 11 (S1-09), add or update packages with `npx npm@11 install …`; npm 10 crashes (B2). `npm ci` and `npm install` with an unchanged lockfile work.
- **`ng update` to 22** needs Node 24 first (S1-07 blocker 1); B4 does not occur then.
- **Production build** still fails on the bundle budget (S1-11).
- **Karma specs** do not compile and all fail (S1-16).

## 7. Review follow-ups (2026-10-27)

The review of PR #39 found no security or correctness bug. Three points were fixed afterwards (owner decision):

| # | Point | Change |
|---|---|---|
| F1 | B3 pinned Font Awesome, easymde and prettier only in the lockfile; the `^` ranges in `package.json` let the next regeneration move them again (it happened again in S1-09, B2) | `package.json` pins `@fortawesome/fontawesome-free` 7.2.0, `easymde` 2.20.0 and `prettier` 3.6.2 exactly; installed versions unchanged |
| F2 | `ngx-color-picker` was unused but shipped as a runtime dependency | Removed from `package.json` and the lockfile. The commented-out colour pickers in `features/settings/settings.ts` and `settings.html` still name it; re-enabling them needs the package (or a replacement) again |
| F3 | B5 called the TypeScript default `lib` equivalent to `["ES2022", "dom"]` | B5 corrected |

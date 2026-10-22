# S1-09 — Angular 21 → 22, Node 24, TypeScript 6 and smoke test

> **Sprint:** 01, task S1-09 (milestone M0.2, ARCH C37 / §12 step 0). **Status:** done (2026-10-22) — builds, checks and browser smoke test on the compose stack passed.
> **Scope:** `ng update` 21 → 22 for core, CLI, Material and CDK; NgRx Signals and ngx-translate updated; Node 24 LTS and TypeScript 6.0, which Angular 22 requires (S1-07 blockers 1 and 2). Only the changes the upgrade forces; no refactoring. Zoneless/OnPush work follows in S1-10, the release build and Dockerfile clean-up in S1-11, Vitest in S1-16.

## 1. Versions

| Package / tool | Before | After | Why |
|---|---|---|---|
| `@angular/*` (core, common, compiler, forms, router, platform-browser, animations, compiler-cli) | 21.2.25 | 22.2.3 | Task |
| `@angular/build`, `@angular/cli`, `@angular-devkit/core` | 21.2.27 | 22.2.3 | Task |
| `@angular/material`, `@angular/cdk` | 21.2.14 | 22.2.4 | Task (Material 21 requires Angular `^21`) |
| `@ngrx/signals` | 21.1.1 | 22.0.1 | Task; 21 declares peer `@angular/core ^21` |
| `@ngx-translate/core`, `@ngx-translate/http-loader` | 17.0.0 | 18.0.0 | Task; owner decision 2026-10-22 (17 would still work, peer `>=16`) |
| `ngx-markdown` | 21.3.0 | 22.1.1 | Peer `@angular/* ^21` blocks the install |
| `typescript` | 5.9.3 | 6.0.3 (`~6.0.3`) | `@angular/build` 22 needs `>=6.0 <6.1`; **not** 7.x |
| `istanbul-lib-instrument` (new dev dependency) | – | 6.0.3 | Added by the CLI migration because Karma is still configured; goes with Karma in S1-16 |
| Node (`.nvmrc`) | 22.20.0 | **24.22.1** | Angular 22 needs `^22.22.3 \|\| ^24.15`; owner decision S1-07 (Node 24 LTS) and 2026-10-22 (24.22.1) |
| npm | 10.9.3 | 11.19.1 | Bundled with Node 24.22.1 |
| Client `Dockerfile` build stage | `node:22-alpine` (floating) | `node:24.22-alpine` (resolves to 24.22.1 / npm 11.19.1 today) | Same minor as `.nvmrc` (S1-07) |

Unchanged on purpose: `marked` 17.0.6, `ngx-color-picker` 20.1.1 (peer `>=19`; not imported, S1-08), `@auth0/angular-jwt` 5.2.0, `typedjson`/`reflect-metadata`, Karma/Jasmine. Font Awesome 7.2.0, easymde 2.20.0 and prettier 3.6.2 were pinned back in the lockfile after regeneration (B2).

## 2. Breaking changes and how they were fixed

| # | Breaking change | Fix |
|---|---|---|
| B1 | `ng update @angular/core@22 @angular/cli@22 @angular/material@22 @angular/cdk@22 @ngrx/signals@22` stops: `ngx-markdown` 21 has peer `@angular/common ^21` | All target versions set in `package.json` in one step and installed, then each package's migrations run with `ng update <pkg> --migrate-only --from=<old> --to=<new> --allow-dirty` (same route as S1-08) |
| B2 | npm 11 `ERESOLVE` when installing on top of the existing lockfile: arborist keeps the 21 tree (e.g. `@angular/animations` 21.2.25) and cannot satisfy the exact peer `@angular/core 22.2.3` | Lockfile regenerated from scratch with npm 11 (lockfile version 3 as before). Unrelated direct packages that moved (Font Awesome 7.3.1, easymde 2.21.0, prettier 3.9.10) pinned back to the versions on `main`; `package.json` ranges unchanged |
| B3 | Angular CLI 22 runs Prettier over every file a migration touches; that reformatted unrelated lines in 15 files | Migrations re-run with a temporary `.prettierignore` containing `**` (the CLI calls Prettier with `--ignore-unknown`, which honours it); file removed afterwards. Diff now contains only the migration changes |
| B4 | Components without `changeDetection` are **OnPush by default** in Angular 22 | Migration `change-detection-eager` added `changeDetection: ChangeDetectionStrategy.Eager` to the 14 components that had none (the other 10 were already OnPush). Behaviour unchanged; moving them to OnPush is S1-10 |
| B5 | The compiler rejects duplicate outputs: `MarkdownEditor` had `value = model('')` (implicit output `valueChange`) **and** `@Output() valueChange` | Migration `model-output` replaced the model with `valueInput = input('', {alias: 'value'})` + `value = linkedSignal(this.valueInput)`. The component never called `value.set()`, so it only ever emitted through the hand-written output; `[(value)]` in `criterion-proposal.html` binds unchanged |
| B6 | `?.` in templates now returns `undefined` instead of `null` | Migration `safe-optional-chaining` wrapped one expression in `$safeNavigationMigration(...)` (`product-details.html`, link href) to keep the old result |
| B7 | Extended diagnostics `nullishCoalescingNotNullable` / `optionalChainNotNullable` now fire on existing code | Migration `strict-safe-navigation-narrow` set both to `suppress` in `tsconfig.app.json` and `tsconfig.spec.json` |
| B8 | TypeScript 6: `baseUrl` is deprecated and fails the build (`TS5101`) | `baseUrl` removed from `tsconfig.json`; `paths` targets made relative (`./src/app/*`, …). The unused alias `@tests` keeps its old target (project root) as `./`. `ignoreDeprecations` not needed |
| B9 | ngx-translate 18: `TranslateService.getCurrentLang()` returns `Language \| null`; `EnLanguage[currentLang]` fails with `TS2538` | `LanguageService.get()` checks for `null` first; the fallback to `DEFAULT_LANGUAGE` is unchanged |
| B10 | Angular 22 refuses Node 22.20 | Node 24.22.1 in `.nvmrc` and `node:24.22-alpine` in the Dockerfile |

No other breaking change applied: the NgRx Signals 22 `DeepSignal` change for union state (`user: UserDto \| null`) compiles unchanged; the other Angular 22 breaking changes (`ComponentFactoryResolver`, `provideRoutes`, `CanMatch`, Hammer.js, `data-` bindings, `[min]`/`[max]` strings, router `paramsInheritanceStrategy` — no nested parameterised routes) have no use in `src/`. The ngx-translate 18 changes to `TranslateModule`, `setDefaultLang`, `currentLang`, the `[translate]` directive and `setTranslation` do not apply (not used).

### Migrations that ran

| Package | Migration | Result |
|---|---|---|
| `@angular/cli` | `add-istanbul-instrumenter` | added `istanbul-lib-instrument` (dev) |
| | `trust-proxy-headers`, `update-workspace-config` | no changes |
| `@angular/core` | `change-detection-eager` | 14 components (B4) |
| | `model-output` | `markdown-editor.ts` (B5) |
| | `safe-optional-chaining` | `product-details.html` (B6) |
| | `strict-safe-navigation-narrow` | `tsconfig.app.json`, `tsconfig.spec.json` (B7) |
| | `can-match-snapshot-required`, `http-xhr-backend`, `incremental-hydration`, `strict-templates-default` | no changes |
| `@angular/material`, `@angular/cdk` | update to v22 | no changes |
| `@ngrx/signals`, `ngx-markdown`, `@ngx-translate/core` | – | no migrations shipped |

**Not run (optional):** `migrate-karma-to-vitest` (S1-16), `use-application-builder` (already on `@angular/build:application`).

## 3. Build, tests and checks (Claude Code, 2026-10-22, Node 24.22.1)

| # | Check | `main` (Angular 21, S1-08) | Branch (Angular 22) |
|---|---|---|---|
| 1 | `npm run build` (development) | ✅ 0 errors, 0 warnings | ✅ 0 errors, 0 warnings |
| 2 | `npm run build:release` (production) | ❌ budget error, 1.76 MB initial (366.6 kB transferred) | ❌ same budget error, 1.81 MB (373.4 kB transferred); same warnings (budget, CommonJS `reflect-metadata` and `easymde`, one CSS selector). S1-11 |
| 3 | `npx ng test` (Karma, all 22 specs) | ❌ does not compile (`save-search-dialog.spec.ts` imports a missing `./update-user-dialog`) | ❌ same compile error |
| 4 | Karma, other 21 spec files (broken spec excluded through a temporary tsconfig outside the repository) | 22 of 22 fail, `NG0201` | 22 of 22 fail, `NG0201` (88×, missing providers `API_URL`, `JWT_NAME`, `MatDialogRef`). Specs are replaced in S1-16 |
| 5 | `npm audit` | 9 high, all dev tooling; 0 runtime | 7 high, all dev tooling (`@angular/build` → `vite`, `karma` → `braces`/`chokidar`); **0** runtime (`--omit=dev`) |
| 6 | `docker compose build --no-cache client` (`npm ci` with npm 11 in `node:24.22-alpine`) | ✅ | ✅ |
| 7 | All components carry an explicit `changeDetection` | – | ✅ 24 of 24 (14 Eager, 10 OnPush) |
| 8 | `reuse lint` (reuse 6.2.0) | ✅ | ✅ compliant, 459/459 files |

## 4. Smoke test (browser, Claude in Chrome, compose stack)

Stack: `docker compose up --build -d` after the no-cache client build. Throwaway account `smoke-s109@example.test` on the local stack; the sign-up mail goes to the fixed Lettermint test address (S1-06 K5). Browser: `ng-version` = 22.2.3, `window.Zone` undefined.

| # | Step | Expected | Result |
|---|---|---|---|
| 1 | Home page | Loads with texts from `de.json` and three product cards | ✅ |
| 2 | Login dialog (`(login:auth)`), "Hier registrieren" | Material form fields render; register form opens | ✅ |
| 3 | Register | Snackbar "Registrierungsbestätigung gesendet", back to login | ✅ |
| 4 | Sign in with a wrong password | Error snackbar, not signed in | ✅ |
| 5 | Sign in with the correct password | Header shows favourites, profile, sign-out; user appearance applied | ✅ |
| 6 | Full page reload (`/filters`) | Still signed in | ✅ |
| 7 | Criteria filter: expand all, select app type, product part, test type | Counter "3 ausgewählt"; app-only test types disabled by the exclusions | ✅ |
| 8 | "Enter" → criteria list, expand a criterion | Criteria with priority; rendered markdown (paragraphs, list) | ✅ |
| 9 | "Produkte anzeigen" | As before: `GET /api/Products/byCriteriaFilterIds` 500 (#37), error snackbar, spinner stays (S1-08 N1) | ✅ unchanged behaviour |
| 10 | Products page via the header | Material table with all products | ✅ |
| 10a | Product details (`/product/1`, template touched by B6) | Name and website link `https://www.coreflow.org` | ✅ |
| 11 | Export button in step 2 | As before: link `./` with `download="products.csv"` (K2); `HTMLAnchorElement.click` stubbed in the page, so nothing was downloaded | ✅ unchanged behaviour |
| 12 | Settings: dark mode toggle (not saved), language field | Theme switches immediately, and back; language shows "Deutsch" (goes through `LanguageService.get()`, B9) | ✅ |
| 13 | Sign out | Header shows "Anmelden"; home page loads | ✅ |
| 14 | `docker compose restart api`, reload `/filters` | App works again | ✅ |
| 15 | Console on a fresh load of `/home` | No errors | ✅ no errors; 36 NgRx "members cannot be overridden" warnings and the dev health watcher's "API unreachable", both as on 21 (S1-08 N3, N4) |
| 16 | Markdown editor (B5): sign in again (account re-registered, see note), `/criterion-proposal/0`, type into the editor, do not submit | Editor renders; `[(value)]` updates the parent | ✅ CodeMirror, `valueInput`, `value` and the parent's `proposal().criterion.description` all hold the typed text (read through Angular's dev-mode `ng.getComponent`); no console error |

Note on step 16: the API restart in step 14 rebuilt the database (`RebuildDatabase`, Development), which removed the throwaway account, so it was registered again. Existing behaviour, not related to the upgrade.

## 5. Observations

| # | Observation | Evidence | Proposed follow-up |
|---|---|---|---|
| N1 | npm 11.19 no longer runs install scripts that are not approved (`allowScripts`): `esbuild`, `lmdb`, `msgpackr-extract`, `@parcel/watcher`. Builds work locally and in Docker because the prebuilt platform binaries come as optional dependencies | `npm warn install-scripts …` on every install | Decide in S1-11/S1-20 whether to approve them (`npm install-scripts approve`) or deny them explicitly, so CI output is clean |
| N2 | `.npmrc` contains a stray line `10.9.3`; npm 11 warns "Unknown project config" and says it will stop working in the next major | `CLIENT/CLIENT.ProcureAccess/.npmrc` line 5 (existing) | **Fixed in this task (owner request 2026-10-22):** line deleted; the warning is gone, `npm ci` and the build are unchanged |
| N3 | npm 11 warns `Unknown cli config "--configuration"` for `RUN npm run build --configuration=development` in the Dockerfile: the flag goes to npm, not to `ng` (the `build` script already uses the development configuration) | Docker build log | S1-11 (release build in the image) |
| N4 | Deprecations (no action needed for 22): `@angular/animations` (S1-08 N6) and `withFetch()` (fetch is the default backend in 22) | `npm ci` warning; Angular 22 changelog | Remove when `app.config.ts` is reworked (M1) |
| N5 | On the home page only the icon button next to "Anmelden / Registrieren" opens the login; the text is a plain paragraph, and the button's `aria-label` is the English "Login icon-button" | `pages/home/home.html` (unchanged by this task) | Screens are rebuilt in M1/M2; axe in S1-19 will likely report it |
| N6 | ARCHITECTURE §8.2 says the per-major `ng update` runs "in migration step 1"; §12 and the sprint plan say step 0 | `ARCHITECTURE_v12.md` §8.2 vs §12 | Correct §8.2 in the next ARCHITECTURE version |
| N7 | `ng test --ts-config` with a tsconfig outside the project needs `typeRoots` pointing at the project's `node_modules/@types` (`types` are looked up relative to the tsconfig's folder) | Only affects the temporary workaround in check 4 | None |

## 6. Open points for S1-10 and later

- **S1-10:** 14 components now carry `ChangeDetectionStrategy.Eager`. Moving them to OnPush (and checking each screen) belongs to the zoneless task, together with S1-08 N2 (`/products` by URL).
- **S1-10 or later:** remove the `$safeNavigationMigration()` wrapper and the two suppressed diagnostics once the template is reworked.
- **S1-11:** production budget, Dockerfile build command (N3), global CLI install; npm install scripts (N1).
- **S1-16:** Karma, the broken spec and `istanbul-lib-instrument` go away with Vitest.

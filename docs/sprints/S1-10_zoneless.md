# S1-10 — Zoneless: per-screen check, OnPush everywhere, async results into signals

> **Sprint:** 01, task S1-10 (milestone M0.2, ARCH C37 / §8.2 / §12 step 0). **Status:** done (2026-10-22) — build, checks and browser smoke test on the compose stack passed.
> **Scope:** run the client without Zone.js and fix only what "behaves as before" needs (sprint risk R2). Owner decisions 2026-10-22: **A** move the 14 `Eager` components to `OnPush`; **B** fix the "async result never reaches the screen" places that the baseline smoke test confirms (Z1–Z4), later extended to **Z5**; **C** give the throwaway smoke-test account the Approver role in the local dev database (granted by the owner).

## 1. Starting point

| Item from the task | Finding | Action |
|---|---|---|
| `provideZonelessChangeDetection()` | Already in `app.config.ts`; the client has never had Zone.js (first commit of `CLIENT/`, confirmed in S1-07 §4) | None |
| Remove `zone.js` from polyfills | No `polyfills` entry in `angular.json`, no `zone.js` in `package.json`, lockfile or `node_modules` | None |
| Standalone migration schematic | No NgModules; all 24 components are standalone | Ran `ng g @angular/core:standalone --mode=convert-to-standalone --path=src`: "Nothing to be done", no file changed |
| Places that relied on Zone.js | 14 components still `Eager` (added by the S1-09 migration); five screens copy async store data once (Z1–Z5) | §2 |

## 2. Changes

### 2.1 Eager → OnPush (decision A)

All 14 components that S1-09 marked `ChangeDetectionStrategy.Eager` are now `OnPush`, so **all 24 components are OnPush**. Each one was read before the switch: none shows a plain (non-signal) field in its template that changes after async work. The one plain field that does change, `App.showHeader`, is set from the router outlet's `(activate)`/`(deactivate)` outputs; template listeners mark the view dirty, and the smoke test confirms the header hides and returns around the login outlet.

### 2.2 Async results into signals (decision B)

All five were reproduced on `main` before the change.

| # | Screen | Cause | Fix |
|---|---|---|---|
| Z1 | `/products` | `new MatTableDataSource(this.store.products())` copies the list once; products load later in the store's `init()` (S1-08 N2). By URL: empty table | `effect()` in `ProductsList` writes `store.products()` into `dataSource.data` |
| Z2 | `/proposals` | Same pattern with `store.proposals()` | `effect()` in `ProposalList` as in Z1 |
| Z3 | `/product/:id` | `ngOnInit` read `getProductById` once. By URL: "Kein Produkt mit der ID: 1 gefunden" | `product` is a `computed()` over the store |
| Z4 | `/profile` | The e-mail row was built once from `store.user()` when the page was created. By URL: empty e-mail cell | `userInformation` is a `computed()`; an `effect()` writes it into `dataSource.data` |
| Z5 | `/settings` | `updateUICustomization = model(this.store.uiCustomization())` copies the settings before a signed-in user's settings have loaded. By URL while signed in: empty language field | `linkedSignal(() => this.store.uiCustomization())`. Like the `model()` before, it holds the store's settings object, so the `[(ngModel)]` binding and "Speichern" work as before |

Files: `features/products/list/products-list.ts`, `features/proposal/list/proposal-list.ts`, `features/products/details/product-details.ts`, `features/identity/profile/profile.ts`, `features/settings/settings.ts`, plus the 14 files of §2.1. No template, UI text or dependency changed.

## 3. Build, tests and checks (Claude Code, 2026-10-22, Node 24.22.1)

| # | Check | `main` (S1-09) | Branch |
|---|---|---|---|
| 1 | `npm run build` (development) | ✅ 0 errors, 0 warnings | ✅ 0 errors, 0 warnings |
| 2 | `npm run build:release` | ❌ budget error, 1.81 MB initial | ❌ same budget error, 1.81 MB (373.25 kB transferred); same three warnings. S1-11 |
| 3 | `npx ng test` (Karma, all 22 spec files) | ❌ does not compile (`save-search-dialog.spec.ts` imports a missing `./update-user-dialog`) | ❌ same compile error |
| 4 | Karma, other 21 spec files (broken spec excluded through a temporary tsconfig outside the repository, `CHROME_BIN=/usr/bin/chromium`) | 22 of 22 fail, `NG0201` | 22 of 22 fail, `NG0201` (44×: `API_URL` 36, `MatDialogRef` 6, `JWT_NAME` 2). Specs are replaced in S1-16 |
| 5 | Backend tests | none exist (S1-12) | none exist; `dotnet build` of the API: 0 errors (backend not touched) |
| 6 | `zone.js` in the bundle | – | ✅ none: no `node_modules/zone.js` module in any output chunk; the only match for "zone.js" is an error-message string inside `@angular/core` (`whenStable` testability) |
| 7 | `window.Zone` in the browser | `undefined` | ✅ `undefined` |
| 8 | Components on `Eager` | 14 of 24 | ✅ 0 of 24; `ɵcmp.onPush` is `true` at runtime (checked on `App` and `Header`) |
| 9 | Standalone schematic | – | ✅ "Nothing to be done" |
| 10 | `reuse lint` (reuse 6.2.0) | ✅ | ✅ compliant |

## 4. Smoke test per screen (browser, Claude in Chrome, compose stack)

Every screen was tested **before** the change (baseline, branch = `main`) and **after** it. "By URL" means a full page load on that address, where async store data arrives after the page is created. Throwaway account `smoke-s110@example.test` on the local stack only (Member; Approver role added by the owner for rows 19–20).

| # | Screen | Component(s) changed | Before (`main`) | After | Why |
|---|---|---|---|---|---|
| 1 | App shell (header hide/show around the login outlet) | `App` → OnPush | ✅ | ✅ | A |
| 2 | Header (sign-in state, language switch) | `Header` → OnPush | ✅ | ✅ follows sign-in, sign-out and language switch | A |
| 3 | `/home` | `Home` → OnPush | ✅ cards, sign-in links | ✅ | A |
| 4 | Login (`(login:auth)`): validation, wrong password, sign-in, reload stays signed in | – (already OnPush) | ✅ | ✅ | – |
| 5 | Register (`(login:register)`): e-mail error, password match enables the button, sign-up | `Register` → OnPush | ✅ (sign-up done once, before) | ✅ (validation; no second sign-up) | A |
| 6 | Forgot-password dialog (open, e-mail prefilled, cancel) | `ForgotPasswordDialog` → OnPush | ✅ | ✅ | A |
| 7 | `/filters` step 1: expand, tick app type / product part / test type, counter, exclusions, product-type autocomplete | `ProductsContainer` → OnPush (`FiltersSelection` already OnPush) | ✅ "3 ausgewählt", app-only tests disabled | ✅ identical, incl. the autocomplete quirk (O2) | A |
| 8 | `/filters` step 2: 86 criteria, expand (markdown), "Nächste", back keeps selection, link snackbar, save-search dialog, "Produkte anzeigen" | `ProductsContainer`, `SaveSearchDialog` → OnPush | ✅; "Produkte anzeigen" → 500 (#37), spinner stays (S1-08 N1) | ✅ identical | A |
| 9 | `/products` by URL | `ProductsList` → OnPush + Z1 | ❌ empty table | ✅ 17 rows; sorting by name works | A, B |
| 10 | `/products` in-app, favourite "+", details link | `ProductsList` | ✅ | ✅ | A |
| 11 | `/product/:id` by URL | `ProductDetails` → OnPush + Z3 | ❌ "Kein Produkt mit der ID: 1 gefunden" | ✅ CoreFlow; unknown id still shows the not-found text | A, B |
| 12 | `/favorites` (product tab, remove) | `Favorites` → OnPush | ✅ (lost on reload, O4) | ✅ same | A |
| 13 | `/profile` by URL | `Profile` (already OnPush) + Z4 | ❌ empty e-mail cell | ✅ e-mail shown | B |
| 14 | Change-user and delete-account dialogs (open, typing enables "Löschen", cancel) | `UpdateUserDialog`, `DeleteAccountDialog` → OnPush | ✅ | ✅ | A |
| 15 | `/settings`: dark mode, contrast mode, language EN ↔ DE (not saved) | `Settings` (already OnPush) + Z5 | ✅ in-app; ❌ by URL while signed in the language field is empty | ✅ "Deutsch" also by URL; switching updates page and header | B |
| 16 | `/criterion-proposal/0`: markdown editor writes through to the parent | `MarkdownEditor` → OnPush | ✅ | ✅ CodeMirror, editor and parent hold the typed text | A |
| 17 | `/product-proposal/0` | – (already OnPush) | ✅ | ✅ | – |
| 18 | `/proposals` as Member | – | ✅ redirects to `/unauthorized` | ✅ | – |
| 19 | `/proposals` as Approver (header button after sign-in): list, sort | `ProposalList` + Z2 | – | ✅ new proposal listed, sort works | B |
| 20 | Approver: send a criterion proposal, open it (name and editor filled), "Genehmigen", list shows "approved" | `MarkdownEditor` → OnPush | – | ✅ | A |
| 21 | `/reset-password` (button follows validity and match) | – | ✅ | ✅ | – |
| 22 | `/confirm-email` with an invalid token | – | ✅ error snackbar; spinner stays (O3) | ✅ same | – |
| 23 | `/unauthorized` | `Unauthorized` → OnPush | ✅ | ✅ | A |
| 24 | Console | – | – | ✅ only the expected `HttpErrorResponse` of row 8 | – |

Approver screens (rows 19–20) were only tested after the change: the role was granted after the baseline. The code path of Z2 is the same as Z1, which was tested before and after.

## 5. Observations — existing behaviour, not changed

| # | Observation | Evidence | Proposed follow-up |
|---|---|---|---|
| O1 | *(fixed as Z5)* | – | – |
| O2 | Choosing a product type replaces the whole selection (counter "1 ausgewählt" while three boxes stay ticked); typing in the field afterwards sets the counter to 0. `FiltersSelection` also changes the selection array in place (`splice`) and sets the same array again, which a signal does not notice | `features/filters/selection/filters-selection.ts` | Filter screen is rebuilt in M2 (configurator) |
| O3 | `/confirm-email`: on an HTTP error the page keeps "E-Mail wird bestätigt…" and the spinner; only the error snackbar appears (`map` never runs on error) | `email-confirmation.ts` | Identity flows are rebuilt in M3 |
| O4 | Product favourites live only in memory; a reload empties `/favorites` | `with-favorites.ts` has no load or save | `withFavorites()` is legacy (ARCH §8.1) |
| O5 | The delete-account dialog shows the raw key `AUTH.CANNOT_UNDO_WARNING` | Missing in `de.json` | Screens rebuilt in M3; texts from the UX copy then |
| O6 | After a redeploy the browser kept the old `main.js` until a hard reload: the development build has no hashed file names | `curl -I` shows only `Last-Modified`/`ETag` | S1-11 (release build in the image has hashed names) |
| O7 | Seeded users have no password, so no Approver can sign in out of the box | `SampleDataInitializer.cs` | Test fixtures in S1-13/S1-18 |
| O8 | `/reset-password` and register use `name="password"` for both password fields, so they share one form control | `reset-password.html`, `register.html` | Rebuilt in M3 |
| O9 | `npm run build` refuses the shell's default Node 22.20; `nvm use` (reads `.nvmrc`, 24.22.1) is needed | Local shell | None (documented in S1-09) |
| O10 | Permissions are read from the token only at sign-in (`Login.onSubmit` → `loadUserFromToken`). After a page reload an Approver loses them: `/proposals` by URL redirects to `/unauthorized`, the header button is gone | `auth.service.ts`, `login.ts` | Goes away with cookie auth and the session endpoint in M3 |
| O11 | The empty-state text of `/proposals` ("PROPOSALS.NON_OPEN_FOUND") is never shown: the `<p>` sits inside `<table mat-table>`, which does not project it | `proposal-list.html:104` | `ProposalList` is retired (ARCH §8.1) |

## 6. Open points

- **Later:** the `$safeNavigationMigration()` wrapper and the two suppressed diagnostics from S1-09 stay until the templates are reworked; `provideAnimationsAsync()` stays (S1-09 N4).
- **S1-16:** the Karma specs (check 3/4) are replaced by Vitest; the TESTING §3.2 rule "every `TestBed` configuration includes `provideZonelessChangeDetection()`" applies from then on.

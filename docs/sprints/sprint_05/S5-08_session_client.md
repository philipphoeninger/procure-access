# S5-08 — Client: session state, interceptor, JWT removed

> **Sprint:** 05 · **Task:** S5-08 (3 points) · **Issues:** #317, #320 (closed by this task); none assigned by the row
> **Requirements / invariants:** R-AUTH-05, R-AUTH-07 (overlay and retry; the S01 form card follows in S5-09), R-AUTH-08 (base); SEC-01 (FE part) and SEC-03 (interceptor part) → complete (`TESTING_v58.md` V181); SEC-02 with a shorter exemption list (V182); the E2E storage part of SEC-01 deferred to S5-14 (owner decision)
> **Status:** done — pull request #330 open (2026-11-13)
> **Scope:** `withIdentity()` holds the session from `GET /api/v1/auth/session`; the interceptor turns a 401 into the login overlay and retries the request once; a minimal overlay dialog; the legacy screens sign in and out through the cookie session; JWT, `jwt-decode`, `@auth0/angular-jwt` and every token in browser storage gone on the client; the JWT handler, `/api/signIn`, `/api/refresh`, the Bearer antiforgery exemption and the CORS policy gone on the server; the probe's answer as a contract record with `permissions`; the dev server behind a proxy. No EF migration (the `RefreshToken` table stays until S5-10).
> **Owner decisions (control chat, 2026-11-13):** 1 (a) S5-08 also removes the server-side JWT (handler, selector, Bearer exemption, `JWTSettings`, `signIn`, `refresh`) · 2 (a) the legacy login page switches to `POST /api/v1/auth/login`, S5-09 stays separate · 3 (a) the probe's `user` carries `permissions`, with the contract record of #320 · 4 (a) `proxy.conf.json`, `apiUrl` `/api` everywhere, CORS removed · 5 (a) stale tokens in existing browsers are left alone · the E2E storage inspection of SEC-01 follows with the E2E users of S5-14

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The client signed in through `/api/signIn`, kept access and refresh token in `localStorage`, sent `Authorization: Bearer`, decoded permissions with `jwt-decode`, and read „signed in“ through `AuthService.isAuthenticated()` (token expiry) in the nav, S11, S13 and the settings store; the probe of S5-07 was called at start and its answer thrown away | The whole sign-in state moves into `withIdentity()`; every reader changes |
| F2 | The server ran the cookie next to the JWT (`CookieOrBearer`, Bearer skip in the antiforgery middleware, `/api/signIn` and `/api/refresh` exempt) | Decision 1 (a): all of it goes now; SEC-02's list shrinks |
| F3 | The probe's `user` had `roles` only; the client's guard, two directives and the moderation route check the permission `moderation:review` | Decision 3 (a): `permissions` from the role claims, which Identity's claims factory puts into the cookie principal |
| F4 | `npm start` ran cross-origin (`apiUrl` absolute): the Strict cookie never crosses origins and Angular's XSRF interceptor adds the header only to a relative URL | Decision 4 (a): dev-server proxy, `apiUrl` `/api`, CORS policy removed (#317) |
| F5 | `JwtKeyStartupTests`, the legacy-JWT case of `CookieSessionTests` and three stub paths of the antiforgery-hook tests named the JWT | Deleted or renamed |
| F6 | The dev-only health watcher built its URL from the absolute `apiUrl` (`new URL('/health/live', apiUrl)`) | Resolved against the page origin; the proxy also forwards `/health` |

## 2. Changes

### 2.1 Server

| File | Change |
|---|---|
| `src/ProcureAccess.Modules.IdentityAccess.Contracts/Session/SessionResponse.cs` | New: `SessionResponse` and `SessionUser(Id, Email, Roles, Permissions)`, the first types of the contracts project (#320) |
| `src/…IdentityAccess/Session/SessionProbeEndpoint.cs` | Answers the contract record; `permissions` = the claims of type `permission` (constant `PermissionClaimType`), distinct and sorted |
| `src/…IdentityAccess/Session/AntiforgeryValidationMiddleware.cs` | The Bearer skip is gone: every endpoint without `DisableAntiforgery()` is validated |
| `API/…/Extensions/IdentityExtensions.cs` | `AddIdentityAuth()` without configuration: default scheme `Identity.Application`; JWT handler, `CookieOrBearer` and the key check removed |
| `API/…/Extensions/AppConfigExtensions.cs` | `AddFrontendCors`, `ConfigureCORS` and `AddAppConfig` (`JWTSettings`) removed; `Program.cs` no longer calls them |
| `API/…/Controllers/Identity/IdentityUserEndpoints.cs` | `/signIn` and `/refresh` removed; `signUp`, `forgot-password`, `reset-password`, `confirm-email`, `me` stay until S5-10 |
| `API/…/Swagger/SwaggerConfiguration.cs`, `SwaggerExtensions.cs` | No Bearer security scheme (the session is the cookie) |
| `SERVICES/…/UserService.cs`, `IUserService.cs` | `SignInAsync`, `RefreshTokenAsync` and the token generation removed |
| `DAL/MODELS…/Dto/Read/AuthResponseDto.cs`, `Settings/AppSettings.cs` | Deleted (`JWTSettings`); the `MODELS.ProcureAccess.Settings` global usings with them |
| `API.ProcureAccess.csproj`, `SERVICES.ProcureAccess.csproj` | `Microsoft.AspNetCore.Authentication.JwtBearer` removed; `THIRD-PARTY-NOTICES.txt` regenerated (48 → 39 packages: the IdentityModel family left) |
| `appsettings.json`, `appsettings.Development.json`, `docker-compose.yml`, `.env.example`, `.github/workflows/ci.yml` | `JWTSettings`, `Cors:AllowedOrigins`, `JWTSettings__Key` and `JWT_KEY` removed; the E2E `.env` step fills only `DB_PASSWORD` |
| `API/…/Commands/RoleCommandLine.cs` | Comment: a role change reaches the user with the next sign-in |

### 2.2 Client

| File | Change |
|---|---|
| `features/identity/models/session.ts` | `SessionUser`, `SessionResponse` as the probe answers |
| `features/identity/state/with-identity.ts` | State `session`, `sessionExpiresAt`, `sessionAbsoluteExpiresAt`, `sessionStatus`, legacy `user` (now typed `User`, with the UI settings); computed `signedIn`, `permissions`; `loadSession()`, `sessionReady()`, `signIn()`, `signOut()`, `hasPermission()`; `deleteAccount()` ends the session and opens the start page |
| `features/identity/services/auth.service.ts` | Rewritten with `inject()`: `session()` (`BACKGROUND_REQUEST`), `login()` (`CALLER_HANDLES_PROBLEMS` for `validation`, `antiforgery` — `LOGIN_PROBLEMS`), `logout()`, `me()`; the legacy `register`, `forgotPassword`, `resetPassword`, `confirmEmail`, `showLogin`, `showRegister` typed without `any`; `isAuthenticated`, `loadUserFromToken`, `hasPermission`, `refreshToken` gone |
| `core/interceptors/auth.interceptor.ts` | 401 → `LoginOverlayService.requestSignIn()` → one retry with `RETRIED_AFTER_SIGN_IN`; not for the probe, a background request or a retried request |
| `core/session/login-overlay.service.ts` | One pending dialog for any number of 401s; loads the component on first use (#42) |
| `features/identity/dialogs/login-overlay/` | `pa-login-overlay`: `shell.session.expired.title|body`, e-mail and password fields with `errors.field.required`, error notice (`auth.login.error.invalid` or `errors.antiforgery`, `role="alert"`, focus), „Schließen“, „Anmelden“ with spinner; announces `shell.session.restored`, closes with `true`; panel class `pa-dialog` |
| `core/interceptors/error.interceptor.ts` | A 401 gets neither snackbar nor server page |
| `app.config.ts` | `withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })`; `headersInterceptor`, `provideSessionProbe()`, `JWT_NAME`, `REFRESH_TOKEN_NAME` removed |
| `core/state/app.store.ts`, `core/models/appState.interface.ts` | `init()` waits for `sessionReady()`; the identity slice starts from `initialIdentityState` |
| `layout/main-nav/main-nav.ts` | `signedIn` is the store's signal; „Abmelden“ → `signOut()`, legacy proposals and settings reset, start page |
| `features/identity/login/login.ts`, `.html` | Signs in through `store.signIn()`, sets the legacy UI settings from the returned user, shows `auth.login.error.invalid` (`role="alert"`) on a refusal; `inject()`, no floating promises |
| `core/guards/auth.guard.ts`, `features/identity/guards/permission.guard.ts` | Functional guards (`authGuard`, `permissionGuard`, route data key `PERMISSION_KEY`) that await `sessionReady()`; `app.routes.ts` uses them |
| `features/identity/directives/has-permission.directive.ts`, `has-not-permission.directive.ts` | Signal input + `effect` on `store.hasPermission()` |
| `features/configurator/pages/criteria-list/criteria-list.ts`, `dialogs/export-dialog/export-dialog.ts`, `features/settings/state/with-settings.ts`, `features/products/**/*.html` | Read `signedIn()` instead of `isAuthenticated()` |
| `environments/environment.ts`, `proxy.conf.json`, `angular.json` | `apiUrl` `/api`; dev-server proxy of `/api` and `/health` to `http://localhost:5000` |
| `src/dev/dev-reload-health.ts` | Health URL resolved against the page origin |
| `package.json`, `package-lock.json` | `jwt-decode`, `@auth0/angular-jwt` removed |
| `eslint.config.js` | Entries of `auth.guard.ts`, both directives, `permission.guard.ts`, `login.ts` and `auth.service.ts` removed from `LEGACY_EXCEPTIONS` (#158) |
| Deleted | `core/session/session-probe.service.ts` (+ spec), `core/interceptors/headers.interceptor.ts`, `models/auth-response.model.ts`, `models/custom-claim-types.enum.ts`, `models/user.dto.ts` |
| `scripts/check-i18n.mjs`, `public/i18n/de.json` | `auth.login` is an `AREAS` entry; `--write` copied its 15 texts |

### 2.3 Behaviour that changes

| Request | Before | After |
|---|---|---|
| Any state-changing request from `npm start` | 400 `antiforgery` (absolute URL, no header) | the header is sent (relative URL through the proxy) |
| `POST /api/v1/configurator/criteria-list-shown` from the release build | 400 while no probe had run before S5-07's client change; 204 since | 204 (probe at start, header on the relative URL) |
| A request with `Authorization: Bearer …` | authenticated by the JWT handler, antiforgery skipped | the header is ignored; antiforgery validated like any other request |
| `POST /api/signIn`, `POST /api/refresh` | 200 with tokens | 404 (gone) |
| Cross-origin browser request | allowed for `Cors:AllowedOrigins` | no CORS headers (every client is on the API's origin) |
| Swagger UI | Bearer „Authorize“ button | none; sign in through `POST /api/v1/auth/login` in the UI |

### 2.4 Tests

- `with-identity.spec.ts` — probe with and without user, failed probe, `sessionReady()` once, sign-in and refused sign-in, sign-out also on failure, account deletion; `INV:SEC-01`: every `Storage` method blocked, no `Authorization` header on any request.
- `auth.interceptor.spec.ts` — `INV:SEC-03`: overlay and one retry with the original body, failure when the overlay is closed, no second overlay when the retry answers 401 (no loop), the probe's 401 ignored; no overlay for a background request or for 403/404/500.
- `login-overlay.spec.ts` — texts and fields, required check before any request, `INV:SEC-03` announcement and close with `true`, the two error notices, axe.
- `error.interceptor.spec.ts` — a 401 shows neither snackbar nor server page.
- `main-nav.spec.ts`, `shell-test-doubles.ts`, `criteria-list.spec.ts`, `export-dialog.spec.ts`, `with-settings.spec.ts` (`protectedState: false`, session patched in), `profile.spec.ts`, `dev-reload-health.spec.ts` — doubles follow the session state.
- .NET: `AntiforgeryValidationTests` (list without `signIn`/`refresh`; a Bearer header is validated → 400 `antiforgery`), `CookieSessionTests` (`permissions` asserted; legacy-JWT case gone), `AntiforgeryHandlerTests`/`AntiforgeryHookApiTests` (paths → `/api/v1/auth/login`), `JwtKeyStartupTests` deleted, `ProcureAccessApiFactory` without the key.

### 2.5 Documents

`TESTING_v58.md` (V181–V183; SEC-01, SEC-02, SEC-03 rows; §5.2), `ARCHITECTURE_v50.md` (C136; §8.3 and §9.1 „as built“), `SPEC_v2.md` v2.18 (§8.8), `CLAUDE.md` §3/§4/§5, this protocol. `TESTING_v56.md` and `ARCHITECTURE_v48.md` deleted (two newest versions stay).

## 3. Self-review (CLAUDE §6.7, §6.8)

| Finding | Sorting | Note |
|---|---|---|
| Removing the CORS policy makes the API stricter: a browser on another origin gets no CORS headers at all | no change | wanted (decision 4 (a)); the API port is not public anyway (§9.2) |
| The overlay shows `auth.login.error.invalid` for every failure of `store.signIn()` that is not `antiforgery`, including a failed `/api/me` after a successful login | won't fix | such a failure is a 5xx or no answer, for which the error interceptor already shows the server page or the offline notice; S5-09 builds the full form with the error summary |
| A 401 while another dialog is open (later: the save dialog S12) stacks the overlay on top of it | no change | MatDialog handles stacked dialogs; the retried request returns to the dialog underneath |
| `headersInterceptor` set `Content-Type: application/json` on every request, also without a body; now Angular sets it only for object bodies | fix (removed) | every endpoint posted to without a body has no body parameter; integration and unit tests cover them |
| `UserService` keeps `_db` and `_config` fields the remaining methods still use; `RefreshTokens` of the legacy context stay mapped but unused | follow-up | S5-10 drops the table with a legacy migration |
| Stale JWTs in `localStorage` of existing browsers are never read again | no change | decision 5 (a) |
| The Swagger UI has no „Authorize“ any more | no change | Development only; signing in through the login endpoint in the UI sets the cookie, and the UI sends it |
| No correctness or security bug that affects current behaviour was found | — | merge recommended |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | Build succeeded |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `node scripts/check-api-notices.mjs` (after `--write`) | 39 packages, 0 errors |
| `dotnet test` Architecture / Unit / Integration | 61 / 597 / 289 passed, 0 failed |
| `npx ng build --configuration production` | initial total 726.80 kB (762.9 kB before: the JWT packages left the first download) |
| `npx ng lint` | all files pass |
| `npx ng test --watch=false` | 58 files, 529 tests passed |
| `node scripts/check-invariants.mjs` | passed (TESTING_v58) |
| `node scripts/check-i18n.mjs` (+ self-test) | 0 errors, 15/15 self-test |
| `node scripts/check-figma-links.mjs`, `check-icons.mjs` | 0 errors |
| `reuse lint` (Docker) | compliant |
| `ng serve --port 4299` with the proxy | `/` 200 from the dev server, `/health/live` 200 from the API on 5000 through the proxy, `/api/v1/auth/session` answered by the API (404 from the 27-hour-old dev image that predates S5-07, same answer direct and through the proxy) |
| E2E | not run locally (another session owns the stack); CI |

## 5. Open points

1. **E2E storage inspection of SEC-01 (deferred, owner decision).** Needs a real sign-in on the stack, so it follows with the E2E users of S5-14; the TESTING row says so.
2. **`RefreshToken` table and entity (S5-10).** Mapped in the legacy context, no longer written or read; a legacy migration drops it with the other cleanup.
3. **Lockout hint (#319)** is a question for S5-09, which builds the S01 form.
4. **Sign-in rate limit (#318)** is server work; proposed for S5-13 with the lockout.
5. **English snackbar texts of `deleteAccount` („Account deleted“)** stay until S5-28 rebuilds the account page.
6. **`docs/CLAUDE_v3.md`** (the historical copy) still shows `JWTSettings__Key`; it is history and was not changed.
7. **S5-09:** the overlay and the S01 page should share one form component; the overlay's required check and error notice are the minimal version of the Handoff's error summary.

# S5-07 — Server: Identity cookie session with antiforgery

> **Sprint:** 05 · **Task:** S5-07 · **Issues:** #87, #88, #89
> **Requirements / invariants:** R-AUTH-03 (server part), R-AUTH-05, R-AUTH-08; SEC-01 (server part), SEC-02, SEC-03 (API part), SEC-09, OPS-04 → `active` (TESTING_v56 V175); API-01 stays green; SEC-07/SEC-08 stay `planned` (S5-29)
> **Status:** pull request #316 open (2026-11-13)
> **Scope:** ASP.NET Core Identity cookie session (`HttpOnly; Secure; SameSite=Strict`, 🔧 30 min idle / 8 h absolute from configuration) with the Data Protection keys of S4-26; antiforgery (`X-XSRF-TOKEN`, cookie `XSRF-TOKEN`) on every state-changing endpoint except the anonymous export; `GET /api/v1/auth/session`, `POST /api/v1/auth/login`, `POST /api/v1/auth/logout`; 401 never 302; the integration-host options of #87 – #89. The legacy JWT keeps working next to the cookie for this one PR (S5-08 switches the client and removes it). No EF migration.
> **Owner decisions (control chat, 2026-11-13):** Q1 (A) server switch now, legacy client untouched, two asserted legacy exemptions · Q2 (a) sign-in failures answer 400 `validation` with `errors["$"]`, never 401 · Q3 (a) the stored sign-in time and the absolute limit are built now and tested without an invariant tag · after the first CI run: 1a trust `X-Forwarded-Proto` from the proxies and have nginx send it · 2a the client calls the probe once at start · 3a Secure cookies only on HTTPS requests in `Development` and `E2E`

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The API had one authentication scheme, the legacy JWT (`AddJwtBearer` as default, `DefaultPolicy` bound to it); no antiforgery, no `/auth/session`. The client stores the JWT in `localStorage` and sends `Authorization: Bearer`. | The cookie must come in next to the JWT, and `[Authorize]` must accept both for one PR. |
| 2 | The Data Protection key ring is in the database and encrypted (S3-24, S4-26); `ProblemTypes.Antiforgery` and the UX key `errors.antiforgery` exist; the test client already carries `AntiforgeryHandler` (probes `/api/v1/auth/session`, tolerates 404) and `TestAuthHandler` (`X-Test-User`). | Only the server side was missing; the test infrastructure needed the probe and the `realAuthentication` switch (#88). |
| 3 | The IdentityAccess module may not reference legacy projects (ARC-01); users are still the legacy `User : IdentityUser` in MODELS/DAL. | Sign-in and sign-out, which need `SignInManager<User>`, live in the host (`API/Auth/`) until the users move into the module (M3.1); cookie set-up, antiforgery and the probe live in the module. |
| 4 | Neither SPEC nor ARCHITECTURE named the answer for wrong credentials or the shape of the probe's `user`. | Owner decision Q2 (a); the `user` shape `{ id, email, roles }` is recorded in ARCHITECTURE C133 and SPEC §8.8. |
| 5 | The anonymous `POST /api/v1/configurator/criteria-list-shown` is the row's explicit antiforgery target; the legacy client never calls the probe, so it has no token cookie. | After this PR that request answers 400 until S5-08 calls the probe; the client marks it `BACKGROUND_REQUEST`, so nothing is shown, and the E2E test counts the request, not its status. |
| 6 | Every integration host uses the test scheme as default, replacing the API's scheme host-wide. | A host that tests the real flow needs the API's own schemes: factory flag `realAuthentication`, own database, own clock (#88, "the smaller one"). |

## 2. Changes

### 2.1 IdentityAccess module (`src/ProcureAccess.Modules.IdentityAccess/Session/`, all `internal`)

- `SessionSettings` — 🔧 `Session:IdleTimeout` (00:30:00) and `Session:AbsoluteTimeout` (08:00:00) from `appsettings.json`; a missing, non-positive or unparsable value, or an absolute limit shorter than the idle limit, stops the start.
- `SessionRegistration.AddSession` — `AddIdentityCookies()` (the three Identity schemes; `SignInManager.SignOutAsync` needs them), application cookie `.ProcureAccess.Session` (`HttpOnly`, `SecurePolicy = Always`, `SameSite = Strict`, `IsEssential`, not persistent, `ExpireTimeSpan` = idle limit, sliding, `EventsType = SessionCookieEvents`); cookie handler and `SecurityStampValidatorOptions` take the API's `TimeProvider`; `AddAntiforgery` with header `X-XSRF-TOKEN`, cookie `.ProcureAccess.Antiforgery` (`HttpOnly; Secure; Strict`).
- `SessionCookieEvents` — 401/403 instead of redirects; `SigningIn` stores `Items["pa:signedInUtc"]` (only once, `TryAdd`); `ValidatePrincipal` chains `SecurityStampValidator.ValidatePrincipalAsync`, then rejects and signs out a ticket past the absolute limit; `CheckSlidingExpiration` refuses renewal on the probe (`SessionProbeMetadata`); otherwise the handler's default renewal (after half the window) stays until S5-29.
- `AntiforgeryTokenCookie` — issues the readable `XSRF-TOKEN` (`Secure; SameSite=Strict; Path=/`) from `IAntiforgery.GetAndStoreTokens`, bound to `HttpContext.User`.
- `AntiforgeryValidationMiddleware` — validates every request whose method is not GET/HEAD/OPTIONS/TRACE and that reaches an endpoint without `DisableAntiforgery()` metadata; a request with a Bearer header is skipped (legacy until S5-08); failure → 400 Problem Details `antiforgery`.
- `SessionProbeEndpoint` — `GET /auth/session`: always 200, `{ user: null }` or `{ user: { id, email, roles }, expiresAt, absoluteExpiresAt }` (the times are omitted for a sign-in without limits: JWT, test scheme), (re)issues the token, `Cache-Control: no-cache, no-store`.
- `IdentityAccessModule` — `AddIdentityAccessModule` calls `AddSession`; new public members `UseIdentityAccessAntiforgery()`, `MapIdentityAccessEndpoints()` (group `/auth`) and `IssueAntiforgeryToken(HttpContext)` for the host's sign-in/sign-out (ARC-03 allows public members on the registration class).

### 2.2 Host (`API/API.ProcureAccess`)

- `Auth/AuthEndpoints.cs` — `POST /api/v1/auth/login` (`{ email, password }`; missing fields → 400 `validation` keyed `email`/`password`; unknown address, wrong password, lockout, deleted user → 400 `validation` with `errors["$"]`; success → `SignInAsync(isPersistent: false)`, `HttpContext.User` set to the new principal, new token, 204) and `POST /api/v1/auth/logout` (`SignOutAsync`, anonymous principal, new token, 204). Both are validated by antiforgery like every other endpoint.
- `Extensions/IdentityExtensions.cs` — default scheme is the policy scheme `CookieOrBearer`: Bearer header → JWT handler, otherwise `Identity.Application`; `DefaultPolicy` without scheme names.
- `Program.cs` — `.MapIdentityAccessEndpoints().MapAuthEndpoints()` on the versioned group (before Swagger); `app.UseIdentityAccessAntiforgery()` after the rate limiter.
- `Controllers/Identity/IdentityUserEndpoints.cs` — `DisableAntiforgery()` on `/api/signIn`, `signUp`, `forgot-password`, `reset-password`, `refresh` (legacy until S5-10).
- `appsettings.json` — section `Session`.
- `src/ProcureAccess.Modules.Export/Http/ExportEndpoint.cs` — `DisableAntiforgery()` (the exemption the architecture names).
- `src/ProcureAccess.Modules.Platform/RateLimiting/ClientAddress.cs` (1a) — the forwarded-headers middleware also takes `X-Forwarded-Proto` from the trusted proxies; `RequireHeaderSymmetry` stays off, so a `For` header without a `Proto` header still sets the address. `CLIENT/CLIENT.ProcureAccess/nginx.conf` sends `X-Forwarded-Proto` (Traefik's value when present, else its own scheme).
- `Session/SessionCookiePolicy.cs` (3a) — `CookieSecurePolicy.SameAsRequest` for the session cookie, the antiforgery cookie and `XSRF-TOKEN` in `Development` and `E2E` (guarded on `IHostEnvironment`, the same two environments that may run without a key-ring certificate), `Always` everywhere else; `AddIdentityAccessModule` passes the environment on.
- `CLIENT/CLIENT.ProcureAccess/src/app/core/session/session-probe.service.ts` (2a) — `SessionProbeService.probe()` (GET probe, `BACKGROUND_REQUEST`, never rejects) and `provideSessionProbe()`, registered in `app.config.ts`: fire-and-forget at application start. Angular's default XSRF interceptor (cookie `XSRF-TOKEN` → header `X-XSRF-TOKEN`, relative URLs only) then covers every state-changing request of the release build and the E2E stack; `npm start` (absolute `apiUrl`) sends no header (§5).

### 2.3 Endpoints whose behaviour changes

| Endpoint | Before | After |
|---|---|---|
| `GET /api/v1/auth/session`, `POST /api/v1/auth/login`, `POST /api/v1/auth/logout` | 404 | new (above) |
| `POST /api/v1/configurator/criteria-list-shown` | 204 for everyone | 400 `antiforgery` without `X-XSRF-TOKEN` (the legacy client, until S5-08); 204 with it; `signedIn` also counts cookie sessions |
| `POST /api/Proposals/upsert`, `/review`, `/approve`; `POST`/`PUT` of `UICustomizationController`; `POST /api/Users/change-email`, `/change-password`, `/revoke-sessions`, `DELETE /api/Users` | JWT only | cookie or JWT; with the cookie the token is required, with a Bearer header it is not |
| every `[Authorize]` endpoint and the `ModerationReview` policy | 401 with `WWW-Authenticate: Bearer` | 401 Problem Details from the cookie scheme (no redirect, no `WWW-Authenticate`) unless a Bearer header was sent |
| `POST /api/signIn`, `signUp`, `forgot-password`, `reset-password`, `refresh`; `POST /api/v1/exports` | – | unchanged, exempt by metadata (asserted) |
| unknown `POST` route | 404 | 404 (the middleware validates only requests that reach an endpoint) |
| GET endpoints, `/health/*` | – | unchanged |

### 2.4 Tests

- `tests/ProcureAccess.Tests.Integration/Infrastructure/ProcureAccessApiFactory.cs` — parameter `realAuthentication` (#88).
- `tests/ProcureAccess.Tests.Integration/IdentityAccess/SessionProbeTests.cs` — #87: anonymous and unknown `X-Test-User` → 200 `{"user":null}` + token cookie (`INV:SEC-09`), fixture user → `user` without expiry times.
- `…/IdentityAccess/AntiforgeryValidationTests.cs` — `INV:SEC-02`: missing and wrong token → 400 `antiforgery`, probe token → 204, export exempt (400 `validation`), exact exemption list from `EndpointDataSource`, Bearer header skips validation; unknown POST route → 404.
- `…/IdentityAccess/CookieSessionTests.cs` — real hosts on an own database and clock: `INV:SEC-01` cookie flags and the only readable cookie, `INV:SEC-03` 401 Problem Details without `Location` and the cookie accepted by `/api/me`, `INV:SEC-09` token rotation on login and logout with the old token rejected and the cookie deleted, probe with both expiry times, `INV:OPS-04` second host accepts the cookie; untagged: wrong password and missing fields, idle limit (31 min), absolute limit (23 × 20 min active, ends at 8 h), legacy JWT sign-in and Bearer write without token.
- `…/FixtureTests/AntiforgeryHandlerTests.cs` — #89 `StateChangingRequest_CallerHeaders_ProbeCarriesSameHeaders`; comments updated; `AntiforgeryHookApiTests` renamed to the legacy exemption.
- `…/Api/ProblemDetailsHostTests.cs` — its client got the antiforgery hook (its POST now needs the token).
- `tests/ProcureAccess.Tests.Unit/IdentityAccess/SessionSettingsTests.cs` — defaults and every refused value.
- `tests/ProcureAccess.Tests.Integration/Api/ForwardedProtoTests.cs` (1a) — probe over HTTP from a trusted proxy forwarding `https` → 200 with the Secure token; without the header → 500; from an untrusted address → ignored (500).
- `tests/ProcureAccess.Tests.Unit/IdentityAccess/SessionCookiePolicyTests.cs` and `ForwardedProtoTests.Probe_OverHttpInDevelopment_IssuesTheTokenWithoutSecure` (3a).
- `CLIENT/…/core/session/session-probe.service.spec.ts` (2a) — GET with the background marker, resolves on failure, called once by the initializer.

### 2.5 Documents

TESTING_v56 (V175–V177; v54 deleted), ARCHITECTURE_v47 (C133, §7.1, §9.1 "as built", §12 step 3; v45 deleted), SPEC_v2 v2.16 (§8.8), CLAUDE.md §3/§4/§5, this protocol. Figma mapping, LEGACY_EXCEPTIONS and TECH_DEBT untouched (no client file; tech debt at sprint end).

## 3. Self-review (CLAUDE §6.7, §6.8)

| Finding | Sorting | Note |
|---|---|---|
| The validation middleware validated POSTs that reach no endpoint, so an unknown route answered 400 `antiforgery` instead of 404 | fix | skipped when `GetEndpoint()` is null; test `StateChangingRequest_UnknownRoute_Returns404NotAntiforgery` |
| The probe, login and logout set `Cache-Control: no-store` themselves, which antiforgery then overrode with a warning per request | fix | removed; antiforgery sets `no-cache, no-store` |
| The export endpoint had no `DisableAntiforgery()` yet (the plan assumed it) | fix | added, asserted by SEC-02 |
| A request with a bogus Bearer header and a valid session cookie: the cookie is never consumed (the policy scheme forwards to JWT), so skipping antiforgery there cannot be used for CSRF | no change | documented in the middleware |
| The user's own session survives their password change until the security-stamp interval (30 min) passes | follow-up | SEC-08, S5-29 |
| Sliding renewal is the handler's default (after half the window), not on every request | won't fix here | ARCHITECTURE puts it in step 4 (S5-29); the probe never renews, as the row requires |
| `.ProcureAccess.Session` is not persistent (gone when the browser closes) | no change | matches "no token survives in the browser"; a "remember me" is not in scope |
| **CI, first run:** every E2E job red — `s11-list.spec.ts` asserts the 204 of `criteria-list-shown` (my plan said it only counted the request) and received 500: antiforgery with `SecurePolicy = Always` throws on a non-HTTPS request, and the API sees HTTP behind every proxy, also behind Traefik in staging and production | fix (1a, 2a) | `X-Forwarded-Proto` trusted and sent; the client calls the probe at start |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | exit 0 |
| Unit (592), Architecture (61), Integration (288 + the new ones, incl. API-01) | all passed locally (Docker) |
| `node scripts/check-invariants.mjs` | passed (SEC-01/02/03/09, OPS-04 counted) |
| `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | passed, nothing touched |
| `reuse lint` (container) | only the owner's untracked `.worktreeinclude` reported, not part of this PR |
| Client: `npm run lint`, `ng test` (56 files, 507 tests) | passed |
| E2E | first CI run red on all five browsers (see §3); fixed by 1a, 2a and 3a, confirmed by the second CI run (see the PR) |

## 5. Open points

1. **Secure cookies on the plain-HTTP dev and E2E stacks — decided (3a).** `SessionCookiePolicy` relaxes the three cookies to `SameAsRequest` in `Development` and `E2E` only; production and staging keep `Always`, and behind Traefik the scheme comes from `X-Forwarded-Proto` (1a). TLS in the stacks (the alternative) was not chosen.
2. **`npm start` origin (S5-08).** The dev client on `localhost:4200` calls `http://localhost:5000/api` cross-origin; cookies would need `AllowCredentials` in the CORS policy and `withCredentials` in the client, and `SameSite=Strict` still blocks a cross-site cookie. The release build and the E2E stack call `/api` on their own origin and are unaffected. Recommendation for S5-08: a dev-server proxy to the API (`proxy.conf.json`), so `apiUrl` is `/api` everywhere and CORS can shrink.
3. **Legacy client until S5-08.** With 2a the release build and the E2E stack send the token for `criteria-list-shown`; `npm start` (absolute `apiUrl`, no XSRF header) answers 400 silently until point 2 is settled.
4. **Sign-in rate limit.** SPEC §7.3 lists limits per endpoint; the new `/auth/login` has none yet (lockout after 5 attempts applies). Proposed issue for S5-08 or S5-12.
5. **Lockout message.** R-AUTH-03 wants a lockout hint next to `auth.login.error.invalid`; the server answers the same `$` error for both (Q2 (a)), so the client cannot tell them apart. If the hint is wanted, the server needs a second problem type or an extension member — owner decision for S5-08.
6. **`SessionResponse` is `internal` and serialised by reflection;** when the client needs it typed (S5-08), a contract record in `IdentityAccess.Contracts` can replace it.
7. Deferred to S5-29 (step 4): renewal on every request, `X-Session-Expires-At` headers, `POST /auth/session/extend`, `Session:WarningLead`, SEC-07/SEC-08.

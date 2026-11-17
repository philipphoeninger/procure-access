# S5-29 — Session hardening (M4.1)

> **Sprint:** 05 · **Task:** S5-29 (stretch, 5 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-AUTH-05 (completion), §8.8 (v2.47); **SEC-07**, **SEC-08** → `active` (TESTING_v87 V293 – V296); ARCHITECTURE C165; ROADMAP M4.1. R-AUTH-06, A11Y-04 and J5 are S5-30
> **Status:** done — pull request #PR open (2026-11-17)
> **Scope:** the server session of migration step 4: renewal of the idle limit on every request, capped at the absolute limit; the expiry headers; `POST /api/v1/auth/session/extend`; the security stamp checked on every request, so a password reset and an account deletion end the other sessions at once; the 🔧 warning lead time served by the probe. On the client only the reading of the expiry headers into the session state — no screen.
> **Owner decisions (2026-11-17, before the task):** **1a** — the client part of the expiry headers (ARCHITECTURE §8.3 job 2) belongs to this task; S5-30 is only the dialog · **2a** — `Session:WarningLead` is a 🔧 server setting, validated at start and served by the probe (`warningLeadSeconds`), no client copy · **3a** — a signed-in read of the catalog is answered `no-store`, because the answer renews the cookie; anonymous reads unchanged · **4a** — Identity's security-stamp validator with a 🔧 interval of zero (`Session:SecurityStampValidationInterval`), no check of our own · **Assumptions confirmed:** „password change“ is the reset of S03 (S61 has none, S5-28 1a); the extend endpoint answers 204 with the two headers, needs the antiforgery token, has no rate limit and is never refused near the absolute limit; J5 and A11Y-04 stay with S5-30.

## 1. Starting point

- S5-07 had built the cookie session with the stored sign-in time (`Items["pa:signedInUtc"]`) and the absolute limit in `OnValidatePrincipal`. Renewal was the cookie handler's rule (only after half of the idle window, so the effective idle limit varied between 15 and 30 minutes); the probe never renewed.
- No expiry headers and no extend endpoint; `Session:WarningLead` did not exist. The client knew the two limits only from the probe at start and after a sign-in (S5-08).
- The password reset (S5-13a) and the account deletion (S5-28a) changed the security stamp, but Identity checked it every 30 minutes — as long as the idle limit — so the other sessions did not end with their next request, and no test could show that they did (TESTING V199). SEC-07 and SEC-08 were `planned`; the idle and absolute limits had two tests without a tag.

## 2. Changes

### 2.1 Server (`src/ProcureAccess.Modules.IdentityAccess/Session/`)

- **`SessionCookieEvents.ValidatePrincipal`** decides everything, in this order: (1) the limits — a ticket whose idle or absolute limit is reached is rejected and the cookie deleted; (2) Identity's security-stamp validator; (3) the renewal: every request except the probe sets the issue time to now and the expiry to min(now + idle limit, sign-in time + absolute limit) and asks the handler to renew. On the probe the renewal the stamp validator asks for is switched off again. `CheckSlidingExpiration` always answers no, so the handler's own rule never applies.
- **Expiry headers** `X-Session-Expires-At` and `X-Session-Absolute-Expires-At` (ISO 8601 UTC, whole seconds): written at the end of the validation, so on every answer to an authenticated request (module endpoints, legacy controllers, error answers, the probe with its unrenewed limits); `SigningIn` writes them on the answer of the sign-in, `SigningOut` removes them.
- **`SessionExtendEndpoint`**: `POST /api/v1/auth/session/extend`, signed in, 204, an empty handler — being an authenticated request is what renews.
- **`SessionSettings`**: `WarningLead` (positive, shorter than the idle limit) and `SecurityStampValidationInterval` (zero or more); `appsettings.json` gets `00:05:00` and `00:00:00`. `SessionRegistration` hands the interval to `SecurityStampValidatorOptions`.
- **Probe**: `SessionResponse.WarningLeadSeconds` (contracts), left out without a session.
- No change in the host: the reset (`ResetPasswordAsync`) and the deletion (`UpdateSecurityStampAsync`) already change the stamp. No migration, no new dependency.

### 2.2 Client

- `core/session/session-limits.ts` — `SessionLimits`, a root service without imports that holds the two limits, and the header names.
- `authInterceptor` writes the headers of every answer, also a failed one, into it. It does not inject the root store: the store's first request (the probe) runs while the store is built.
- `withIdentity()`: `sessionExpiresAt` and `sessionAbsoluteExpiresAt` are computed from `SessionLimits` and null while nobody is signed in; the probe sets the limits; new state `sessionWarningLeadSeconds`.

### 2.3 Tests

- `CookieSessionTests` (its own host with the real cookie scheme, fake clock): six `INV:SEC-07` tests (renewal at minutes 10, 20, 49 and the end at 79; 401 at exactly 30 idle minutes; the probe does not renew and serves the lead time; extend; extend without session or token; the headers on every kind of answer; the cap and the end at the absolute limit) and two `INV:SEC-08` tests (two sessions; after a completed reset and after a deletion the other session answers 401). Without a tag: a role taken away is gone with the next request; a signed-in catalog read is `no-store`, an anonymous one `public`.
- `SessionSettingsTests`: the two new values and their limits.
- Client: `auth.interceptor.spec.ts` (two `INV:SEC-07` tests and one without), `with-identity.spec.ts`.

### 2.4 Documents

TESTING_v87 (V293 – V296; v85 deleted), ARCHITECTURE_v79 (C165, §7.1, §8.3, §9.1, §9.6; v77 deleted), SPEC v2.47 (§8.8, §8.13), CODEBASE §2.80, §3, §4, CLAUDE.md §3 (state and a rule „Session“).

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | The first run failed three tests: at exactly the expiry instant the cookie handler still accepts the ticket (it refuses only a ticket *past* its expiry), while the client is told that instant as the end | **fixed** — the validation rejects a session whose idle limit is reached, as it did for the absolute limit; the limits are now checked first, before any database read |
| R2 | C60 describes the extend endpoint as „sets `ExpiresUtc` explicitly and signs in again with the existing `Items`“; built as an empty handler, because every authenticated request renews | **deviation named in the plan**, same behaviour (tested: idle limit renewed, sign-in time kept); recorded in C165 (3) |
| R3 | The stamp validator rebuilds the principal on every signed-in request: by Identity's code one read of the user, one of its claims, one of its roles and two per role — not measured here | **won't fix** — owner decision 4a, the expected load of ARCHITECTURE §11.1; the interval is a 🔧 setting if it ever matters |
| R4 | Identity checks the stamp only when *more* than the interval passed since the ticket was issued; a request in the very instant of the last renewal is not checked | **won't fix** — ticket times are stored in whole seconds, so a real request is practically always later than the stored instant, and the next request is checked; the tests advance the fake clock by one second and TESTING V294 says why |
| R5 | Two answers of parallel requests may arrive in another order than they were sent, so the client may keep the limit of the earlier one | **won't fix** — the difference is the milliseconds between the requests; the warning of S5-30 works in minutes |
| R6 | A late answer after the sign-out writes limits into `SessionLimits` | **no finding** — the store shows them only while somebody is signed in, and the next probe sets them anew (tested) |
| R7 | Security: the extend endpoint needs the session and the antiforgery token (tested); the headers carry only two times; the probe still never renews, so a shell that only asks keeps nothing alive; the client has no timer or polling that would renew a session without the user; the antiforgery token stays valid across the rebuilt principal (the extend test posts 25 minutes after the sign-in with the token of the sign-in) | **no finding** |
| R8 | Caching: an answer with the session cookie or the expiry headers never leaves with `public` (tested for the catalog) | **no finding** (decision 3a) |
| R9 | Accessibility: no screen | **n/a** (S5-30) |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 671 passed, 0 failed** (unit, architecture, integration).
- Client: `npm run lint` clean; `npx ng test` **780 passed** (80 files).
- `check-invariants` passed (SEC-07, SEC-08 `active`); `check-figma-links`, `check-i18n`, `check-icons` 0 errors; `reuse lint` compliant.
- E2E not run locally (0.8 GB free on the disk, no room for the stack images); no E2E file changed. CI is the authority — it is the first run of the real sign-in flows with the stamp check and the renewal on every request.

## 5. Open points

1. **S5-30 (M4.2):** the warning dialog with `shell.session.warning.*`, „Angemeldet bleiben“ on `POST /auth/session/extend`, the completed login overlay, J5 with the controlled clock, A11Y-04. Everything it needs is served: `sessionExpiresAt`, `sessionAbsoluteExpiresAt`, `sessionWarningLeadSeconds`.
2. **Cost per request (R3):** not measured. If the load ever asks for it, raise `Session:SecurityStampValidationInterval`; SEC-08 then holds only after that interval.
3. **Signed-in catalog reads are not cached** (decision 3a): S10/S11 of a signed-in user load the catalog on every visit. If that becomes noticeable, the alternative of the decision (catalog requests neither renew nor carry headers) is still open.
4. **Staging and production** take the two new values from `appsettings.json`; nothing to set in the environment.

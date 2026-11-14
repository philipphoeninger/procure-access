# S5-13a — Password reset and sign-in lockout, server part

> **Sprint:** 05 · **Task:** S5-13a (first half of S5-13, 2.5 points together with S5-13b) · **Issues:** #319 (decided here, closed with S5-13b, which builds the lockout note)
> **Requirements / invariants:** SPEC R-AUTH-03, R-AUTH-04, §7.3, §8.8 (v2.23); ARCHITECTURE C72, §9.1, §9.2 (C141); **SEC-11 → `active`** (TESTING_v62 V196; invitation tokens with S5-17); **API-05** covers both reset limits (V197); A11Y-09 untouched (S03 and its reflow check come with S5-13b)
> **Status:** done — pull request #335 open (2026-11-14)
> **Scope:** `POST /api/v1/auth/password-reset` and `/password-reset/complete`; the reset token as a second purpose of `IAccountTokens`; the background mailer that keeps known and unknown addresses equally fast; the rate limits `PasswordReset` (IP) and `PasswordResetAddress` (address) with Platform's new `IKeyedRateLimiter`; the 🔧 lockout from configuration and on `TimeProvider`. **Not in this half (S5-13b):** S03 at `/passwort` and `/passwort/neu`, the fixed lockout note on S01, UX copy v29 with the confirmed wording, S03 in `reflow.spec.ts`, the E2E reset with Mailpit, and the removal of the legacy reset (`forgot-password`, `reset-password`, the dialog and page, the SEC-02 exemptions).
> **Owner decisions (2026-11-14):** 1 (a) — the server keeps one answer for a wrong password and a locked account; S01 shows a fixed lockout note with every refused sign-in; `auth.login.locked` is dropped (#319) · 2 (a) — every non-deleted account gets a reset link, confirmed or not; a completed reset confirms the address and ends a lockout · 3 (a) — the reset changes the security stamp, other sessions end at its next check; the immediate end is SEC-08 with S5-29 · 4 (a) — the legacy reset goes in S5-13 (here: in S5-13b, see §1 #6) · 5 (a) — split into S5-13a (server) and S5-13b (client) · wording of the S03 keys and the lockout note confirmed (UX copy v29 in S5-13b) · S5-13a (a) — the reset request queues the address for a background mailer, so the answer takes as long for a known as for an unknown address

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | Sign-in already counted failed attempts (`CheckPasswordSignInAsync(…, lockoutOnFailure: true)`), but 5 and 15 minutes were hard-coded in `IdentityExtensions` (CLAUDE §6.6) | 🔧 `Account:Lockout:*` in `appsettings.json` (`LockoutPolicy`) |
| 2 | Identity 10 reads the system clock for the lock (no `TimeProvider` in `Microsoft.Extensions.Identity.Core` 10.0.15), so SEC-11's "controlled clock" was impossible | `ClockedUserManager` overrides `IsLockedOutAsync` and `AccessFailedAsync` |
| 3 | The legacy reset uses Identity's built-in token (not stored, not single use), puts the address into the link and sends nothing to an unconfirmed address | Purpose `PasswordReset` of `IAccountTokens` (C139), token only in the link, decision 2 (a) |
| 4 | Only a known address triggers an SMTP send; answering after it would let the response time tell known from unknown addresses, although the bodies are identical | Decision S5-13a (a): background mailer |
| 5 | SPEC §7.3 limits the reset request per address, but the rate-limit middleware runs before binding and cannot read the body | `IKeyedRateLimiter` in Platform, called by the endpoint |
| 6 | Deleting the legacy `forgot-password` / `reset-password` now would break „Passwort vergessen?“ (S01 opens the legacy dialog until S03) between the two pull requests | The legacy reset goes with S5-13b, together with the client switch |
| 7 | `AccountTokens.Purpose` is stored by name | A new purpose needs no migration |
| 8 | Identity's security-stamp interval (30 minutes) equals the idle limit, so a test cannot show "the other session ends after a reset" apart from "the session expired" | No such test; V199 records it for S5-29 |

## 2. Changes

### 2.1 Endpoints (host, `API/Auth/PasswordResetEndpoints.cs`)

In the host next to sign-in and registration (legacy `UserManager<User>`, until M3.1); both antiforgery-validated.

- **`POST /api/v1/auth/password-reset`** `{ email }`: 400 `validation` at `email` for a missing or malformed address (the same check as the registration, `RegistrationEndpoints.IsEmailAddress`, now `internal`). Otherwise the per-address limit (`IKeyedRateLimiter.Acquire(PasswordResetAddress, NormalizeEmail(address))`), the address into `PasswordResetMailQueue`, and **200 `{ validHours }`** — the 🔧 lifetime in whole hours, the same for every address, so S03 fills `{{hours}}` of `auth.reset.sent` without a client setting. No account lookup in the request path.
- **`POST /api/v1/auth/password-reset/complete`** `{ token, newPassword }`: the new password goes through Identity's password validators (🔧 minimum and the 128 maximum) **before** the token is redeemed, so a refused password keeps the link (400 at `newPassword`). Then the token is redeemed (unknown, expired, used, or a deleted account → 400 at `token`). On success `EmailConfirmed = true`, `AccessFailedCount = 0`, `LockoutEnd = null` and the new password are saved in Identity's one update of `ResetPasswordAsync` (with an Identity token generated in place), which also changes the security stamp; then `IAccountTokens.RevokeAsync` ends the user's other unused reset links. 204, no sign-in.

### 2.2 Background mailer (host, `API/Auth/PasswordResetMailer.cs`)

`PasswordResetMailQueue` (singleton, bounded channel of 100, `TryWrite` without waiting; a full queue counts the drop) and `PasswordResetMailer` (`BackgroundService`, one reader): per address a new scope, `FindByEmailAsync`, nothing for an unknown or deleted account, otherwise `IssueAsync(userId, PasswordReset)` and `PasswordResetEmail` to the **stored** address with `<App:BaseUrl>/passwort/neu?token=…` (`ClientLinks.PasswordResetPagePath`). A failed send logs „A password reset e-mail could not be sent.“, any other failure „… could not be processed.“, drops are logged with their count when the mailer takes its next request — never the address. Queued requests are lost when the host stops. Registered in `Program.cs` only, not in the command hosts.

### 2.3 Account tokens (IdentityAccess)

`AccountTokenPurpose.PasswordReset`; `IAccountTokens.RevokeAsync(userId, purpose)` (one `ExecuteUpdate` setting `UsedAt` on the unused, unexpired tokens) and `LifetimeOf(purpose)`; `IssuedAccountToken.HoursOf(lifetime)` shared with `ValidHours`; 🔧 `Account:Tokens:PasswordResetLifetime` (`02:00:00`), a bad value stops the start. No migration.

### 2.4 Keyed rate limit (Platform)

- `Platform.Contracts/RateLimiting`: `RateLimitPolicies.PasswordReset` and `.PasswordResetAddress`; `IKeyedRateLimiter.Acquire(policy, key)`; `RateLimitExceededException` (policy, `RetryAfter`).
- `Platform/RateLimiting`: `PasswordReset` joins the client-IP policies; `KeyedPolicies` holds `PasswordResetAddress`; `KeyedRateLimiter` (one `PartitionedRateLimiter` per keyed policy, fixed window, keyed by the SHA-256 hex of the key; registered through a factory so the container disposes it); `RateLimitExceededExceptionHandler` writes the same 429 as the middleware through the shared `WriteRejectionAsync(context, retryAfter)` (now sets the status itself).
- 🔧 `RateLimits:PasswordReset` 5 / `01:00:00`, `RateLimits:PasswordResetAddress` 3 / `01:00:00`; every integration host and `docker-compose.e2e.yml` relax both.

### 2.5 Lockout (host)

`LockoutPolicy.From(configuration)` reads 🔧 `Account:Lockout:MaxFailedAttempts` (5) and `:Duration` (`00:15:00`) into Identity's options (with `AllowedForNewUsers`); `ClockedUserManager` (`AddUserManager<ClockedUserManager>()`) follows Identity's own two methods but reads `TimeProvider`. The sign-in endpoint is unchanged: a locked account answers 400 `validation` at `$` like a wrong password (decision 1 (a)).

### 2.6 Tests

- `tests/ProcureAccess.Tests.Integration/IdentityAccess/PasswordResetTests.cs` (own database, API authentication, own clock, per-address limit 2): `INV:SEC-11` — identical answers and only the known address gets a link; link used twice; refused after 2 h, accepted 1 s before; stored as SHA-256; 11 and 129 characters refused with the link kept; lockout after 5 failures until 15:00 on the clock (the right password answers like a wrong one meanwhile); 4 failures do not lock. `INV:API-05` — the third request for one address (also upper case) is 429 with `retryAfterMinutes` 60 and `Retry-After`, another address stays free. Untagged: unconfirmed account gets a link and the reset confirms it; address in other case and with spaces goes to the stored address; deleted account gets nothing; field errors; antiforgery; old password refused and new one works; other links revoked; a reset ends a lockout; a success restarts the count.
- `RateLimitTests`: `PasswordReset` joins the IP theories; the defaults check covers both new policies.
- `RegistrationTests`: three existing tests carry `INV:SEC-11` (V191).
- Unit: `KeyedRateLimiterTests`, `LockoutPolicyTests`, `AccountTokenTests` (reset lifetime), `PasswordPolicyTests` (helper with the lockout settings).
- `CapturingEmailSender.WaitForAsync<T>` for e-mails sent in the background.

### 2.7 Documents

SPEC v2.23 (R-AUTH-03, §8.8, §13 #319 closed), ARCHITECTURE_v55 C141 (v53 deleted), TESTING_v62 V196–V199 (v60 deleted), CLAUDE.md §3, §4, §5. No change to the UX copy, the Figma mapping or `LEGACY_EXCEPTIONS` in this half (no client file).

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `KeyedRateLimiter` was registered as a ready-made instance, which the container never disposes; its partition timers would outlive every test host | **fix** — registered through a factory |
| R2 | Dropped queue entries are logged only when the mailer takes its next request; a burst that fills the queue with nothing after it is never logged | **won't fix** — the queue holds 100 against 5 requests per hour per IP; the documents say when the count is logged |
| R3 | A failure after the token is redeemed (e.g. a concurrency conflict in `ResetPasswordAsync`) uses up the link and answers 500 | **won't fix** — the password passed the same validators first; the user asks for a new link |
| R4 | The SHA-256 of an address can be reversed by guessing, so the limiter's memory is not anonymous | **won't fix** — memory only, for at most the window; the point is that no plain address sits in a dictionary that a dump would show |
| R5 | `/password-reset/complete` has no rate limit | **won't fix** — SPEC §7.3 lists none; the token has 256 random bits and nothing is hashed before it is redeemed |
| R6 | No N+1: one lookup and one insert per queued address, one conditional update and one read per redemption, one update for revocation | — |
| R7 | Logs never name the address or the token (`IssuedAccountToken.ToString`); the 429 exception names only the policy | — |

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` — no warning
- `dotnet format ProcureAccess.sln --verify-no-changes` — clean
- `dotnet test --solution ProcureAccess.sln` — 1101 passed (unit 687, architecture 61, integration); after the R1 fix unit, architecture, `PasswordResetTests` and `RateLimitTests` again green
- `node scripts/check-invariants.mjs` — passed (SEC-11 `active`)
- `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` — passed (no client or text change)
- `reuse lint` — compliant
- No NuGet change, so `check-api-notices.mjs` is unaffected; E2E unaffected (no client change; `docker-compose.e2e.yml` only relaxes the two new limits) — left to CI

## 5. Open points

1. **S5-13b** (next): S03 at `/passwort` and `/passwort/neu` (token out of the address like the confirmation page; request → `auth.reset.sent` with `validHours`; new password with `auth.register.password.hint`; done heading; invalid page), the fixed lockout note on S01 (its numbers as client settings kept in step with `Account:Lockout:*`), UX copy v29 with the confirmed keys (`pageTitle.reset`, `pageTitle.reset.invalid`, `auth.reset.newPassword.submit`, `auth.reset.done.title`, `auth.reset.done.login`, `auth.reset.invalid.title`, `auth.reset.invalid.requestNew`, the lockout note; `auth.login.locked` removed), S03 in `reflow.spec.ts`, an E2E reset through Mailpit, and the removal of the legacy reset with the SEC-02 exemptions (decision 4 (a)). #319 closes with it.
2. **SEC-08 (S5-29):** the immediate end of the other sessions after a reset, and its test (V199).
3. **SEC-11 invitation tokens (S5-17)** with the invitation endpoint.
4. **Legacy `UserService.RequestPasswordResetAsync` / `ResetPasswordAsync`** still use Identity's own token and the address in the link until S5-13b removes them.

# S6-26 — Rate limits for sign-in and organisation creation

> **Sprint:** 06 · **Task:** S6-26 (1 point) · **Issues:** #318 (TECH_DEBT_v5 TD-60; S5-07 P4, S5-16a P3)
> **Requirements / invariants:** SPEC §7.3, §8.8 (R-AUTH-03), §8.9 (R-ORG-01); API-05 gains three policies; SEC-11 stays green; no invariant flipped
> **Status:** done — pull request open (2026-11-18)
> **Scope:** exactly the row of S6-26. Platform (policy names and registration), IdentityAccess (sign-in and organisation endpoints), the client's sign-in form and S20, the relaxed limits of the integration host and the E2E stack. No EF migration, no new dependency, no new UX copy key, no screen layout change. E2E was not run locally, because another session used the local E2E stack at the same time; it runs in CI.
> **Owner decisions:** O-S6-2 (2026-11-18): sign-in 30 per 10 minutes per client IP and 10 per hour per e-mail address (not like the reset: 5 per hour per IP would lock out an office behind one shared address; the account lockout keeps protecting each account), organisation creation 5 per day per user; all 🔧 and in SPEC §7.3. Task decisions of the session (2026-11-18): **Q1 (a)** the tests count requests, as every API-05 test does — the limiters take no `TimeProvider`, so the passing of a window is not tested on the fake clock; **Q2 (a)** S20 shows the existing `errors.rateLimit`, which reads „1440 Minuten“ for the day window; an hours variant is an open point (P2).

## 0. Plan (written before the code)

- **Starting point:** `main` at ed7ba2b (after S6-05). Branch `sprint06/S6-26-rate-limits`.
- **Server:** `RateLimitPolicies.SignIn` (client IP), `SignInAddress` (keyed, through `IKeyedRateLimiter` like the reset's address), `OrganizationCreation` (per user); 🔧 values in `appsettings.json`; `RequireRateLimiting` on `POST /auth/login` and `POST /organizations`; the address counted before the lookup. Relaxed in `ProcureAccessApiFactory` and `docker-compose.e2e.yml`; `RateLimits:Invitation` unchanged.
- **Client:** `rate-limit` in `LOGIN_PROBLEMS` and `CREATE_PROBLEMS`; `errors.rateLimit` in the error summary of the sign-in form and S20.
- **Tests:** `INV:API-05` per policy (count, partition, 429 shape); Vitest for both forms.
- **Docs:** SPEC v2.51, ARCHITECTURE v89 (C177), TESTING v94 (V323 – V325), CODEBASE §2.96. **Invariants:** none activated. **Migration:** none. One session.
- **Asked before the code:** the row's "tests on the fake clock" against the limiters' own clock (Q1), and the day window's „1440 Minuten“ (Q2).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `POST /api/v1/auth/login` had no rate limit; only the lockout (5 failed attempts, 15 minutes) per account (#318) | Per IP through the middleware, per address through `IKeyedRateLimiter` |
| F2 | `POST /api/v1/organizations` had no rate limit; one confirmed account could create any number of organisations (TD-60) | A per-user policy; authorization runs before the limiter (`Program.cs`), so a 403 does not count |
| F3 | The limiters of `System.Threading.RateLimiting` keep their own clock; .NET 10 gives them no `TimeProvider` (checked in the shipped assembly). `RateLimitingRegistration` documents this, and every API-05 test checks the count, not the passing of a window | The row's "tests on the fake clock" cannot be met without a limiter of our own → Q1 |
| F4 | `errors.rateLimit` takes `{{minutes}}`; a day window gives `retryAfterMinutes` 1440 | Q2 |
| F5 | The login form and S20 did not list `rate-limit`, so a 429 would have shown the interceptor's general snackbar | Both forms show `errors.rateLimit` in their summary, as S02, S03 and the invite form do |

## 2. Changes

1. **Server.** `RateLimitPolicies` (`Platform.Contracts`): `SignIn`, `SignInAddress`, `OrganizationCreation`; `RateLimitingRegistration` puts them into its client-IP, keyed and per-user groups; `appsettings.json` holds `30 / 00:10:00`, `10 / 01:00:00`, `5 / 1.00:00:00`. `AuthEndpoints`: `RequireRateLimiting(SignIn)`; `LoginAsync` asks `SignInAddress` with `NormalizeEmail(email.Trim())` after the empty-field check and before `FindByEmailAsync` — every attempt with both fields counts, also a successful one; an address without an account gets the same 429; a refused attempt never reaches `CheckPasswordSignInAsync` and so never counts towards the lockout. `OrganizationEndpoints`: `RequireRateLimiting(OrganizationCreation)` behind `ConfirmedEmail`. Both endpoints declare the 429 in their OpenAPI description.
2. **Relaxed limits.** `ProcureAccessApiFactory` and `docker-compose.e2e.yml` relax the three new policies like the others (TESTING §12.5; the seeded E2E users sign in far more than ten times an hour).
3. **Client.** `LOGIN_PROBLEMS` and `CREATE_PROBLEMS` list `rate-limit`; `pa-login-form` (S01 and the login overlay) and `organization-start-page` (S20) show `errors.rateLimit` / `.one` with the server's `retryAfterMinutes` in their error summary; the sign-in shows no lockout note with it.
4. **Tests.** `RateLimitTests`: the login path in the per-IP theories; `CreateOrganization_OneMoreThanTheLimitForOneUser_Returns429AndLeavesOtherUsersFree INV:API-05` (two confirmed accounts stored first); untagged `CreateOrganization_UnconfirmedUser_Returns403AndDoesNotCount`; the defaults test with the new values. New `IdentityAccess/SignInRateLimitTests.cs` (own host with real authentication and antiforgery, address limit 2): two `INV:API-05` cases (limit + 1 → 429 with 60 minutes for the address in another case and with spaces, also with the right password, another address free; the same 429 with and without an account) and two untagged ones (a 429 does not count towards the lockout; an attempt without a password does not count for the address). Vitest: one case each in `login-form.spec.ts` and `organization-start-page.spec.ts`.
5. **Docs.** SPEC v2.51 (§7.3 rows, §8.8, §8.9), ARCHITECTURE v89 (C177, §9.2; v87 deleted), TESTING v94 (V323 – V325, API-05 row, §12.5; v92 deleted), CODEBASE §2.96 and the rate-limit note of §4; §3 unchanged (no new folder). TECH_DEBT stays at v5 (new version at the sprint end, S6-09); CLAUDE.md unchanged (no new rule: the rate-limit rule of §3 already covers new policies).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: the address limit is asked before the lookup, so the 429 tells nothing about an account (tested); the address is kept only as its SHA-256 hash in memory, never logged (`KeyedRateLimiter`); antiforgery still runs before the endpoint, so the keyed count needs a valid token, and the IP count happens before it (a forged request still counts per IP). Correctness: authorization before the limiter keeps an unconfirmed account's 403 out of the organisation count (tested). Privacy: nothing stored. N+1 / performance: no query added; one hash per sign-in attempt. Accessibility: the 429 line goes into the existing error summary, which takes the focus (tested on the sign-in form).

| Finding | Sorting | Reason / fix |
|---|---|---|
| Anyone who knows an address can block its sign-in for up to an hour with ten requests (the lockout already allowed 15 minutes with five) | won't fix | Consequence of the decided value O-S6-2; recorded in ARCHITECTURE C177. Changing it is an owner decision (P3) |
| Many different addresses create many partitions of the keyed limiter | won't fix | Bounded by the IP limit (30 per 10 minutes); `PartitionedRateLimiter` removes idle partitions — as for the reset's address |
| The sign-in has no length check of the address before hashing it | won't fix | As before the task (the address went to `FindByEmailAsync` unchecked); bounded by the request size limit and the IP limit; no behaviour change wanted in this row |
| The dev stack keeps the production limits: a developer who signs in more than ten times an hour with one address gets 429 | follow-up | P4; the E2E stack and the integration host are relaxed |

No correctness or security bug affecting current behaviour was found.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | no changes |
| `dotnet test --solution ProcureAccess.sln` | 1814 tests passed (unit, architecture, integration with Testcontainers) |
| Touched classes (`RateLimitTests`, `SignInRateLimitTests`, `PasswordResetTests`, `CookieSessionTests`, `OrganizationEndpointsTests`) | 104 passed; the six new tests on their own: 6 passed |
| `npx ng test` | 894 tests in 87 files passed |
| `npm run lint` | all files pass |
| `node scripts/check-invariants.mjs` | Invariant gate passed |
| `node scripts/check-figma-links.mjs` | 7 checked, 19 not built yet, 0 pending (no mapped component changed its layout) |
| `node scripts/check-i18n.mjs` | 0 errors (no key added) |
| `node scripts/check-icons.mjs` | 0 errors |
| `reuse lint-file` on the new files (`fsfe/reuse`) | compliant; the full `reuse lint` runs in CI (the container sees no git in a worktree) |
| Figma | no frame compared: no layout changed, only a line of an existing error summary (S01 has a frame; S20 has none, Handoff §5.3) |
| E2E | not run locally — another session used the local E2E stack; left to CI. No E2E test changed; the stack relaxes the new limits |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | #318 and TD-60 are resolved; TECH_DEBT_v6 records it at the sprint end | S6-09 |
| P2 | A day window shows „1440 Minuten“ (`errors.rateLimit`, Q2 (a)); the experience report (20 per day) and the product-identity request (5 per day) will have the same | UX copy: an hours variant (e.g. `errors.rateLimit.hours` / `.hours.one` with `{{hours}}`) in the next UX copy version, owner wording; the client then switches above 120 minutes |
| P3 | The per-address limit lets a third party block an address's sign-in for up to an hour (§3) | Owner: keep (O-S6-2), or later count only failed attempts per address — which still blocks after ten failures |
| P4 | The dev stack (`docker-compose.dev.yml`) keeps the production limits | Follow-up issue if it bothers: relax `SignInAddress` there like in E2E |
| P5 | The passing of a rate-limit window is not tested anywhere (Q1 (a)), for every policy | Only with a limiter of our own on `TimeProvider`; not proposed unless a window bug appears |

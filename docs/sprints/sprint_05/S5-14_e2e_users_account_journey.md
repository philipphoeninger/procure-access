# S5-14 — E2E users and the account journey J8

> **Sprint:** 05 · **Task:** S5-14 (1 point) · **Issues:** none
> **Requirements / invariants:** SPEC R-AUTH-01, R-AUTH-02, R-AUTH-03, R-AUTH-08 (covered end to end, unchanged); TESTING §6.3 (user part of the E2E seeder, V203), §5.4 (new journey J8, V204); **SEC-01 complete** with its E2E part (V205, deferred by S5-08); PRV-11 (E2E part moved into J8), A11Y-01, A11Y-08 stay green; prepares the J1 second part (S5-20); no invariant changes its status
> **Status:** done — pull request #338 open (2026-11-15)
> **Scope:** the E2E users in the seeder; journey J8 register → mail in Mailpit → confirm → sign in → sign out with axe in both appearances; SEC-01's browser storage inspection after a real sign-in; the S01 tests on real sessions instead of the faked one.
> **Owner decisions (2026-11-14, before the task):** S5-14 1 (a) — the seeder runs only in `Development` or `E2E` and only when a setting is set that only `docker-compose.e2e.yml` sets; no `E2E` environment yet · 2 (a) — one confirmed and one unconfirmed user; organisation users with S5-15/S5-16 · 3 (a) — fixed test credentials in the repository (`example.invalid`, fixed password in the compose override and `e2e/support/users.ts`) · 4 (a) — SEC-01's E2E storage inspection is part of this task · defaults accepted: the journey in its own file under `journeys/`, `login.spec.ts` on the seeded user, the seeder in the host, no EF migration · **2026-11-15:** the journey is J8

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The task prompt said "Owner decisions: none", but the control chat had taken S5-14 1–4 the day before | Applied as recorded in the header |
| 2 | `register.spec.ts` (S5-12b) already ran register → Mailpit → confirm with the PRV-11 checks; `password-reset.spec.ts` (S5-13b) reset → sign-in | J8 takes over the first test of `register.spec.ts` and extends it with S01, the signed-in state and „Abmelden“, so a registration with its mail checks runs once |
| 3 | `login.spec.ts` and one reflow case faked the session (`fakeSession`) "until the E2E users of S5-14" (S5-09) | Real sessions with the seeded user; `fakeSession` removed |
| 4 | The E2E stack runs as `Development` with `RebuildDatabase: true`, which deletes every user at each start | The seeder runs after the rebuild (and after the test catalog release); idempotent for a start without the rebuild |
| 5 | The legacy settings store still writes UI settings to `localStorage` (`with-local-storage.ts`, until S5-25) | SEC-01's inspection cannot require empty storage: it requires no key for a token or the session and no value holding the session cookie or a JWT |
| 6 | The E2E stack runs plain HTTP, so the session cookie is not `Secure` there (S5-07 decision 3a) | J8 checks `HttpOnly` and `SameSite=Strict`; `CookieSessionTests` keep the `Secure` check on HTTPS |
| 7 | The S01 tests share one seeded account; five wrong passwords lock it (SEC-11) | A refused sign-in uses an unknown address; `users.ts` says why |
| 8 | The root disk had 1.6 GB free: too little to build the E2E stack images | E2E against the branch's API (`dotnet run --no-launch-profile` on 5100), a throwaway PostgreSQL and Mailpit, and `ng serve` on 4300 (§4) |

## 2. Changes

### 2.1 User part of the E2E seeder (ARCHITECTURE C143, TESTING V203)

- `API/API.ProcureAccess/Extensions/TestUserSeedExtensions.cs`: `SeedTestUsersIfAllowedAsync` creates `e2e-confirmed@example.invalid` (address confirmed) and `e2e-unconfirmed@example.invalid` (not confirmed) through the legacy `UserManager<User>` — user name = address, no role, the password of `TestUsers:Password`. An existing user is left as it is. It runs only in `Development` and `E2E` (`IsTestUserSeedAllowed`, the same check as the catalog part) and only when the password is set; a password set in another environment is logged as a warning and ignored. A refused password stops the start with the Identity error codes only. It moves into IdentityAccess with the users (M3.1).
- `Program.cs` calls it after `SeedTestCatalogIfAllowedAsync`.
- `docker-compose.e2e.yml`: `TestUsers__Password` with a comment that names `e2e/support/users.ts`; the header lists the users and no longer counts them as missing.
- `e2e/support/users.ts`: `CONFIRMED_USER`, `UNCONFIRMED_USER`, `TEST_USER_PASSWORD`, with the rules for shared accounts (no wrong password, no change of password or address).

### 2.2 Journey J8 (TESTING V204, V205)

`e2e/journeys/j8-account.spec.ts`, by keyboard, on a new address per run: S02 → success → confirmation mail from Mailpit (PRV-11: one recipient, no image/style/script, one link to the own origin and the confirmation path, the same link in the text, no password) → confirmation page (token not in the address, not in any Referer; a second click still „bestätigt“) → header „Anmelden“ → S01 → signed in (S01 closes, the H1 of the page underneath focused, header „Abmelden“, the probe names the address) → SEC-01 inspection → „Abmelden“ (start page, H1 focused, probe `user: null`, session cookie gone). Axe in both appearances on six states (`j8-s02-form`, `j8-s02-success`, `j8-confirm-done`, `j8-s01`, `j8-signed-in`, `j8-signed-out`). Tags `INV:SEC-01 INV:PRV-11 INV:A11Y-01 INV:A11Y-08`.

SEC-01 (E2E part): `.ProcureAccess.Session` is `HttpOnly` and `SameSite=Strict`; the only cookie without `HttpOnly` in the browser's jar, and the only name in `document.cookie`, is `XSRF-TOKEN`; no `localStorage`/`sessionStorage` key matches `token|jwt|session|auth`, and no value holds the session cookie's value or a JWT.

### 2.3 Other E2E tests

- `register.spec.ts`: its first test moved into J8; the header says so. The edge cases stay (known address, unknown link, „Jetzt registrieren“, empty submit, faked 429).
- `login.spec.ts`: no faked session. S01, the "keeps its input" case and the overlay after a 401 sign in as `e2e-confirmed` through the form; „Abmelden“ starts signed in through the API (`signInThroughApi`, probe token + `POST /auth/login`); the refused sign-in sends an unknown address to the real API. `expireSessionOnce` stays for the 401.
- `reflow.spec.ts`: the overlay case needs no session, so `fakeSession` is gone there too.
- `support/session.ts`: only `expireSessionOnce` remains.

### 2.4 Tests (backend)

`tests/ProcureAccess.Tests.Integration/Operations/TestUserSeedGuardTests.cs` (8 cases, each on its own database): nothing in Testing, Staging, Production even with the password; both users with that password, user name = address, the right confirmation state and no role in Development and E2E (the control of the first theory); nothing without the password; a second start leaves id, password hash and security stamp unchanged; with `RebuildDatabase` in Development both users exist after the start (order of the startup steps). Not an invariant (like `TestCatalogSeedGuardTests`).

### 2.5 Documents

- TESTING_v64 (V203–V206): §6.3 users, §3.3 state note, §5.4 row J8, A11Y-01 names J1–J8, SEC-01 row complete, PRV-11 row names J8; TESTING_v62 deleted.
- ARCHITECTURE_v57 C143 and §10.2; ARCHITECTURE_v55 deleted.
- SPEC v2.25: the current-versions line only.
- CLAUDE.md: version line, §3 (S5-14 paragraph; the S5-09 and S5-12b sentences point to it), §4 (host extension, e2e support and journeys, compose override), §5 (`stack:up` names the users).
- No Figma mapping, UX copy, `de.json` or `LEGACY_EXCEPTIONS` change: no screen or text changed.

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y) over the whole diff; findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Could the known password ever exist outside an E2E stack? Two conditions must both hold (environment `Development`/`E2E` and the setting); only `docker-compose.e2e.yml` sets it, `appsettings*.json` do not; Staging/Production with the setting are covered by the guard theory | — (no change) |
| R2 | `FindByEmailAsync` does not see a soft-deleted user (the legacy query filter on `IsDeleted`); if a test ever deleted a seeded account, the next start without the rebuild would fail with `DuplicateUserName` | **won't fix** — `users.ts` forbids changing the shared accounts, and every E2E start rebuilds the database; S5-28 (account deletion) registers its own account |
| R3 | The log line "E2E test users are seeded" is written also when both existed | **won't fix** — informational, the guard tests check the state |
| R4 | The `request` listener of J8 stays attached after the confirmation step | **won't fix** — the array is read once; later requests carry no token |
| R5 | No new dependency, no secret (test data by owner decision 3 (a)), no N+1 (two lookups at startup) | — |
| R6 | A11y: J8 runs by keyboard, queries by role and name, axe in both appearances on every state; the mat-checkbox of S02 is found by role alone (TESTING V129, V165) | — |

No correctness or security finding that affects current behaviour.

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1107 passed (8 new).
- `check-invariants` (TESTING_v64), `check-figma-links`, `check-i18n` passed; e2e `typecheck` and `lint` clean; `reuse lint` compliant.
- E2E locally against the branch (API `dotnet run --no-launch-profile` on 5100 with the E2E settings, throwaway PostgreSQL 17 and Mailpit, `ng serve` on 4300; the dev stack untouched): J8, `identity/` and `reflow.spec.ts` in Chromium, Firefox and mobile Chrome 120 of 120; the whole suite in Chromium 103 of 103. WebKit and mobile Safari only in CI.
- Firefox, repeated: with two workers the content process crashed now and then at the axe run of S02's success state (J8 2 of 4 with `login.spec.ts` in parallel; 0 of 6 and 0 of 8 in other runs). The first test of `register.spec.ts` from `main` crashed the same way in the same set-up (2 of 8 in one run with J8, which passed 8 of 8 there); with one worker J8 and `login.spec.ts` passed 28 of 28. So it is the local Firefox problem of S5-13b under load on the dev build, not this change; CI (release build, one job per browser project) is the authority.
- CI of #338: every stage green; J8 passed on all five browser projects (Firefox included) at the first run. Only `6. E2E + a11y (mobile-safari)` failed once, in `configurator/s10-step1.spec.ts` (untouched here): axe found a contrast of 3.89 (`#807c82` on `#fef7ff`, 12 px) in the state „s10 step 1, wide“, a colour between the field's text and the background — Material's form field measured while it fades in, as `register.spec.ts` notes for S02. The re-run of that job passed.

## 5. Open points

1. If CI shows the Firefox crash in J8, split the journey at the confirmation (registration and confirmation / sign-in and sign-out) — fewer axe runs per page.
2. S5-15/S5-16: organisation users in the seeder (Owner, Editor, Reader) with the steps that need them; S5-16 uses `e2e-unconfirmed` for SEC-10's E2E part.
3. S5-20: the J1 second part signs in as `e2e-confirmed` (or registers, as TESTING §5.4 J1 describes — decide there).
4. Proposed issue: `s10-step1.spec.ts` runs axe on „s10 step 1, wide“ right after the H1 is visible, before the form field's fade-in ends (mobile Safari flake in #338, §4); wait as `register.spec.ts` does (`mat-form-field-animations-enabled` and `settled`) before that axe run.
5. When the stack gets its own `E2E` environment (TESTING §3.3), the seeder's check stays as it is; `RebuildDatabase` then no longer runs, and the idempotent path is used.

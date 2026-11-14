# S5-12a — Registration and e-mail confirmation, server part

> **Sprint:** 05 · **Task:** S5-12a (first half of S5-12, 3.5 points together with S5-12b) · **Issues:** #301 (closed by this task)
> **Requirements / invariants:** SPEC R-AUTH-01, R-AUTH-02, §7.3, §7.8, §8.6, §8.8 (v2.21); ARCHITECTURE C72, §9.1 (C139); **SEC-10 → `active`** for its integration part (TESTING_v60 V188), client and E2E part with S5-12b, each gated action with its endpoint; **API-05** covers the registration and the confirmation resend (V189); A11Y-09 untouched (no screen in this half)
> **Status:** done — pull request #333 open (2026-11-14)
> **Scope:** `POST /api/v1/auth/register`, `/confirm-email`, `/confirm-email/resend`; hashed single-use account tokens in IdentityAccess (one migration); the policy `ConfirmedEmail` with its 403 `email-unconfirmed`; the password rule; the rate limits `Registration` (IP) and `ConfirmationResend` (user); the problem type `email-delivery`; UX copy v28; #301 by removing the global `Member` role. **Not here (S5-12b):** S02 at `/registrieren`, the confirmation pages at `/registrieren/bestaetigen`, the notice `auth.confirm.required` with „erneut senden“, the switch away from the legacy register page, S02 in `reflow.spec.ts`, the E2E registration with the link from Mailpit (also the E2E part of PRV-11).
> **Owner decisions (2026-11-14):** O-S5-3 (a) — registration without the global `Member` role, legacy uses of it removed · split into S5-12a (server) and S5-12b (client) · 2 — UX copy v28 with the proposed wording, confirmed (U113–U118) · 3 (a) — SEC-10 `active` with the policy on a test endpoint; each gated action adds its case; the legacy product proposal is not gated · 4 (b) — a used token of a confirmed account answers „bestätigt“ · 5 (a) — the privacy acknowledgement is checked, not stored · e-mail failure (a) — the registration answers 204 even when the mail fails; the resend answers 503 with the new problem type `email-delivery` · defaults accepted without a question: no sign-in rate limit (#318 stays open), the 🔧 values of SPEC, the token table shared with S5-13, the E2E registration in S5-12b.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The legacy `UserService.SignUpAsync` added every user to `Member`, whose row only the Development rebuild created: on a database that was only migrated the sign-up answered 500 after storing the user (#301). Nothing read `Member` (no policy, no role check, the client enum value unused) | O-S5-3 (a): no global role at all |
| 2 | Identity's built-in confirmation tokens are neither stored nor single use, while C72 and SPEC §7.8 want single-use tokens stored hashed | A token table in IdentityAccess behind `IAccountTokens`, reused by S5-13 |
| 3 | Legacy password rule: 6 characters with upper, lower, digit and special character | SPEC's rule: 12 🔧 to 128, no composition |
| 4 | `RequireConfirmedEmail` was already off, so unconfirmed accounts could sign in; nothing checked a confirmed address | The policy `ConfirmedEmail` |
| 5 | None of the gated actions exists yet (organisations S5-15/16, invitations S5-17, saved lists S5-19/20, contributor profiles and publication later) | Decision 3 (a): the policy on a test endpoint |
| 6 | SPEC §7.6 wants a notice with retry after a failed send, but §8.6 had no type for "done, but the mail failed"; a retry of the registration would take the known-address path | Decision (a): 204 for the registration, 503 `email-delivery` for the resend |
| 7 | `email.confirm.body` wrote „24 Stunden“ while the lifetime is 🔧 (S5-11 P5); S02's success heading, the confirmation pages' H1 and page titles, a consent error and a minimum-length error had no wording | UX copy v28, wording confirmed by the owner |
| 8 | The rate limiter knew only partitions per client IP; SPEC §7.3 limits the resend per user | A per-user partition in Platform |
| 9 | Identity's default user-name characters refuse valid addresses such as `o'brien@…` (the user name is the address) | Any character allowed; the address check is the endpoint's |
| 10 | The disk had 1.6 GB free: too little for the E2E stack | E2E left to CI; no E2E file or client screen changed |

## 2. Changes

### 2.1 Endpoints (host, `API/Auth/RegistrationEndpoints.cs`)

In the host next to sign-in, because they need the legacy `UserManager<User>`; they move into IdentityAccess with the users (M3.1). All three are antiforgery-validated.

- **`POST /api/v1/auth/register`** `{ email, password, privacyAcknowledged }`: field errors 400 `validation` at `email` (missing, not one plain address by `MailAddress`, longer than 256), `password` (below 🔧 12, above 128), `privacyAcknowledged` (not `true`; never stored). A **known address** — found by e-mail, or a duplicate `CreateAsync` reports (a registration in between, or an address taken only as a user name) — gets 204 and `AlreadyRegisteredEmail` with a link to `/(login:anmelden)`; its password is hashed anyway, so the answer takes about as long. A deleted account gets 204 and no mail. A **new address** gets an unconfirmed account without a role, a token and `AddressConfirmationEmail` linking `<App:BaseUrl>/registrieren/bestaetigen?token=…`. A failed send is logged with the message type only and the answer stays 204 (decision (a)). Rate limit `Registration`.
- **`POST /api/v1/auth/confirm-email`** `{ token }`: redeems the token; 204 when the address is confirmed now or when the token was used before for a confirmed account (decision 4 (b) — a second click or a mail scanner's visit first); otherwise 400 `validation` at `token`. Needs no sign-in.
- **`POST /api/v1/auth/confirm-email/resend`**: signed in only; 204 without mail for a confirmed address; a new token otherwise (earlier ones stay valid until they expire); 503 `email-delivery` after a failed send. Rate limit `ConfirmationResend` per user.
- `ClientLinks` builds the links from `App:BaseUrl` (the legacy `UserService.AppLink` stays until S5-10).

### 2.2 Account tokens (IdentityAccess)

- `IdentityAccess.Contracts/Accounts/IAccountTokens.cs`: `IssueAsync(userId, purpose)` → `IssuedAccountToken(Token, ExpiresAt, Lifetime)` with `ValidHours` (rounded up) and a `ToString` that never shows the value; `RedeemAsync(token, purpose)` → `Redeemed` / `AlreadyUsed` (both with the user id) / `Invalid`; purpose `EmailConfirmation` (S5-13 adds the reset).
- `Domain/AccountToken`, table `identity."AccountTokens"` (migration `AddAccountTokens`: `Id`, `UserId` without foreign key, `Purpose` by name, `TokenHash` bytea unique, `CreatedAt`, `ExpiresAt`, `UsedAt`).
- `Accounts/AccountTokenService`: 32 random bytes, Base64url in the link, SHA-256 stored (a plain hash suffices for a random value); redemption is one `ExecuteUpdate … WHERE UsedAt IS NULL AND ExpiresAt > now` with the time from `TimeProvider`, then one read to tell used from expired. `Accounts/AccountTokenSettings`: 🔧 `Account:Tokens:EmailConfirmationLifetime` (`1.00:00:00`), a bad value stops the start.

### 2.3 The gate

- `IdentityAccessPolicies.ConfirmedEmail` in `IdentityAccess.Contracts` (Configurator, Workspace and Directory may reference it, ARCHITECTURE §4.3).
- `API/Auth/ConfirmedEmailAuthorization.cs`: `ConfirmedEmailRequirement`; `ConfirmedEmailHandler` reads the user through `UserManager.GetUserAsync` on every gated request (not deleted, `EmailConfirmed`), so a confirmation in another browser counts at once; `ConfirmedEmailResultHandler` answers a request refused **only** by this requirement with 403 Problem Details `email-unconfirmed` and leaves every other result to the default handler (401 without a sign-in).
- Registered in `AddAuthorizationPolicies`. No real endpoint carries it yet.

### 2.4 Password rule, rate limits, problem type, e-mail

- `API/Auth/PasswordPolicy.cs`: 🔧 `Account:Password:MinLength` (12; 1–128, else the start stops), `MaximumLengthPasswordValidator` (128). `ConfigureIdentityOptions(configuration)` drops the composition rules and allows any user-name character. Global: the legacy password change and reset follow until S5-13, and the dev `SAMPLE_USER_PASSWORD` needs 12 characters (`.env.example` says so).
- Platform: `RateLimitPolicies.Registration` and `.ConfirmationResend`; `RateLimitingRegistration.UserPolicies` partitions by `user:<NameIdentifier>`, `ip:<address>` without a sign-in. `appsettings.json` 5 per hour each; the integration hosts and `docker-compose.e2e.yml` relax them.
- `ProblemTypes.EmailDelivery` (`email-delivery`, in `All`).
- `AddressConfirmationEmail(recipient, link, validHours)`; the composer fills `{{hours}}`; the legacy sign-up passes the legacy Identity token's lifetime.

### 2.5 `Member` removed (#301)

`UserService.SignUpAsync` and `SampleDataInitializer` add no role; `AddMemberRoleAsync` and its call in the Development rebuild are gone; `Roles.Member` and the client's `EnUserRole.member` are deleted. Tests: `TestUsers.Member` keeps its name without a role, `Moderator` has only `Moderator`; `LegacyAccountEmailTests` no longer creates the role (so the legacy sign-up is checked on an only migrated database too); `DatabaseRebuildGuardTests` expects a sample user without a role; `RoleCommandTests` and `PlatformRoleMigrationTests` keep a literal `"Member"` row, because old databases still hold it. No migration deletes the rows: they are unused and go with the pre-pilot reset (O-S5-4).

### 2.6 Documents

UX copy v28 (U113–U119; v26 deleted), TESTING v60 (V188–V191; v58 deleted), ARCHITECTURE v53 (C139; v51 deleted), SPEC v2.21 (§8.6, §8.8), CLAUDE.md (§3, §4, §5). No Figma mapping change (no screen), no `LEGACY_EXCEPTIONS` change. `TECH_DEBT` gets #301 closed at the sprint close-out (process step 14).

## 3. Self-review

Checklist of CLAUDE §6.7, then sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | A `DuplicateUserName` from `CreateAsync` for an address `FindByEmailAsync` does not find (a user name without the same e-mail) would have answered 400 with Identity's "already taken" — an account enumeration | **Fixed:** every duplicate answers 204 and, when the account is found by e-mail or name, sends „already registered“ to its own address; test `Register_AddressTakenOnlyAsAUserName_AnswersAsForANewOne` |
| R2 | Timing: the new-address path does a few more database round trips (user validators, insert, token insert) than the known path; the password hash is equalised, the mail dominates both | **Won't fix:** milliseconds against an SMTP round trip; a fixed delay would add a mechanism for a theoretical gain |
| R3 | A `Redeemed` token whose `UpdateAsync` fails is used up and the address stays unconfirmed | **Won't fix:** the user asks for a new link; one transaction across two contexts is what ADR-014 forbids |
| R4 | The token sits in the query string of the confirmation page: browser history and, on an external link, the `Referer` | **Follow-up (S5-12b):** the page reads the token, removes it from the URL (`history.replaceState`) before posting, and links nothing external before that |
| R5 | `identity."AccountTokens"` grows by one row per registration and resend; nothing deletes expired rows | **Follow-up issue (proposed):** a cleanup in Platform's job style, or a delete on redemption; with S5-13, which adds the reset tokens |
| R6 | The legacy `POST /api/signUp` still creates accounts without the privacy acknowledgement, the rate limit and antiforgery, and answers 400 for a taken address | **Follow-up:** S5-12b moves the client to the new endpoint, S5-10 deletes the legacy one (O-S5-4) |
| R7 | 5 registrations per hour per client IP: an office behind one NAT address shares the counter | **Won't fix here:** SPEC §7.3 🔧 value, UX-O11 stays open until the pilot |
| R8 | `ClientLinks` repeats the legacy `UserService.AppLink` | **Won't fix:** the legacy method goes with S5-10 |

Security: no secret in code; tokens and passwords never logged (the send-failure log names the message type only; `IssuedAccountToken.ToString` is its type); the token compare is an indexed hash lookup; the gate reads the resource owner's state from the database. Correctness: concurrency of redemption tested; the expiry boundary tested on the fake clock. N+1: one user read per gated request, two statements per redemption. Accessibility: no UI in this half.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror` (SDK 10.0.404) | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` (Testcontainers PostgreSQL 17 and Mailpit) | 1061 passed before R1; after R1 `RegistrationTests` 26 passed |
| OPS-03 (pending model changes, every context) | green in the integration run |
| ARC-01 – ARC-03 | green (the host references `IdentityAccess.Contracts` transitively; the module exposes nothing new beyond its registration) |
| Client: ESLint and `tsc` of the changed enum, `de-json.spec.ts` | clean, 2 passed |
| `node scripts/check-i18n.mjs` (`--write` copied `email.confirm.body`, `errors.field.minLength`, `pageTitle.register`, `pageTitle.confirm.*`) | 0 errors in all three targets |
| `node scripts/check-invariants.mjs` (TESTING_v60) | passed; SEC-10 counts `RegistrationTests` |
| `node scripts/check-figma-links.mjs` | 6 checked, 0 pending |
| Self-tests of the i18n, invariant and Figma scripts | 15, 36, 10 passed |
| `reuse lint` (Docker) | compliant |
| `check-api-notices.mjs` | not run: no NuGet package changed |
| E2E | not run locally (disk); no client screen or E2E file changed — CI runs the five projects |

## 5. Open points

1. **S5-12b (next):** S02 at `/registrieren` (form card of Handoff §5.3, no Figma frame), the success state with `auth.register.success.title`, the confirmation page at `/registrieren/bestaetigen` with `auth.confirm.done|invalid` as S80 (R4: remove the token from the URL first), the notice `auth.confirm.required` with `auth.confirm.resend` and `auth.confirm.sent` (`email-delivery` → `errors.email.sendFailed`, mark the requests `CALLER_HANDLES_PROBLEMS`), S01's „Jetzt registrieren“ to `/registrieren`, the legacy register and confirm pages off their routes, S02 and the confirmation pages in `reflow.spec.ts`, the client and E2E parts of SEC-10, and the E2E part of PRV-11 (link from Mailpit).
2. **Owner, staging (#301):** nothing to check any more under O-S5-3 (a) — a `Member` row is unused, and users stored without a role by an earlier failed sign-up are ordinary unconfirmed accounts. #301 can be closed.
3. **Owner, dev `.env`:** `SAMPLE_USER_PASSWORD` must have at least 12 characters, otherwise the Development rebuild stops with „Creating the sample user failed: PasswordTooShort“.
4. **Proposed issue — token cleanup (R5):** delete expired and used account tokens (e.g. older than 7 days); with S5-13.
5. **Proposed issue — rate limit on `POST /auth/login` (#318)** stays open as decided; the lockout covers the account, not the address space.
6. **Each gated action** attaches `IdentityAccessPolicies.ConfirmedEmail` and adds its own `INV:SEC-10` case: S5-16 (create organisation), S5-17 (accept invitation), S5-20 (save list), contributor profiles and publication later. When the first real one exists, `ConfirmedEmailProbeController` can go.
7. **SEC-11 (S5-13)** tags the parts built here: the password rule, the identical answer for known addresses and the hashed token storage (TESTING V191).

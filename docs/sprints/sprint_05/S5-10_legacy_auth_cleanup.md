# S5-10 — Legacy auth cleanup

> **Sprint:** 05 · **Task:** S5-10 (M3.1d, 2 points) · **Issues:** #158 (auth entries; stays open for the profile, settings and M5 entries)
> **Requirements / invariants:** API-01 stays green for the identity endpoints (TESTING_v72 V235); TEN-02 allow-list one entry shorter (V236); ARCHITECTURE C150; SPEC v2.32 (current-versions line only); no invariant changes its status
> **Status:** done — pull request open (2026-11-16)
> **Scope:** the remaining legacy auth code after S5-09, S5-12 and S5-13: `Microsoft.AspNetCore.Identity.UI`, the `{ code, message }` failures of `UsersController`, `DateTime.UtcNow` in `UserRepo`, `ERROR_TRANSLATIONS` and the dead legacy client keys, the unused `RefreshTokens` table.
> **Owner decisions (2026-11-16):** O-S5-4 — yes: S01 – S03, S60 and S61 replace the legacy account pages, which go once their replacement is merged; existing users sign in again · S5-10 2a — delete the three legacy account actions no screen calls (`change-email`, `change-password`, `revoke-sessions`) with their repository methods, turn `DELETE /api/Users` into Problem Details · S5-10 3a — drop the `RefreshTokens` table in this task (legacy migration)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The legacy login, register, reset and confirm pages, their routes and `LEGACY_EXCEPTIONS` entries were already gone (S5-09, S5-12b, S5-13b); `register`, `reset-password` and `auth` redirect to S02, S03 and the login outlet | Nothing left to delete there; the redirects stay for old links |
| 2 | Of the legacy identity minimal APIs only `GET /api/me` was left, without a failure body; `UserService` had no `DateTime.UtcNow` left | Nothing to change there |
| 3 | `UsersController` answered `{ code, message }` for four actions; the client called only `DELETE /api/Users` (profile page, delete-account dialog) | Decision 2a |
| 4 | `Microsoft.AspNetCore.Identity.UI` was referenced only for a global `using` of its `IEmailSender`, which nothing used since S5-11 | Removed |
| 5 | `RefreshToken` mapped in the legacy context, unused since S5-08 (S5-08 open point 2) | Decision 3a; not named in the sprint row, assigned by S5-08 |
| 6 | `UserRepo.GetAsync` and `RestoreUserAsync` were in no interface and called nowhere; `RestoreUserAsync` held the TEN-02 allow-listed `IgnoreQueryFilters()` of `UserRepo.cs` | Removed with the other repository methods |
| 7 | `ERROR_TRANSLATIONS` mapped legacy codes to `ERROR.*` keys; its fallback `ERROR.GENERIC` is the general notice of every error without a mapped problem type (UX copy §6.3), and the UX copy names no lowercase key for it | `ERROR.GENERIC` stays as the general notice (open point 1) |

## 2. Changes

### 2.1 Server

- `UsersController`: only `DELETE /api/Users` (primary constructor with `IUserRepo`; the unused logger, `HttpContext` accessor and `UserManager` are gone; without `[Produces("application/json")]`, so its problems are `application/problem+json` — the TECH_DEBT §5 note of S4-03 R2). A principal without user id → 401; a missing user → 404 Problem Details `not-found`; any other failed `IdentityResult` throws (the message names the error codes, not their texts), which Platform's exception handler answers with 500 Problem Details. `change-email`, `change-password`, `revoke-sessions` removed (decision 2a) — they answer 404 like every unknown route.
- `IUserRepo` only `DeleteUserAsync`; `UserRepo` takes `TimeProvider` for `DeletedAt` and names the not-found code (`UserNotFoundCode`); `ChangeEmailAsync`, `ChangePasswordAsync`, `RevokeSessionsAsync`, `GetAsync`, `RestoreUserAsync` and a commented-out block removed. `ChangeEmailRequest`, `ChangePasswordRequest` and their namespace `MODELS.ProcureAccess.Entities.Identity` removed.
- `SERVICES.ProcureAccess.csproj` without `Microsoft.AspNetCore.Identity.UI`; its global `using` gone. `THIRD-PARTY-NOTICES.txt` regenerated (§4).
- `RefreshToken`, `RefreshTokenConfiguration`, `User.RefreshTokens` and the `DbSet` removed; legacy migration `DropRefreshTokens` (`ApplicationDBContext`, drops `public."RefreshTokens"`; `Down` recreates the empty table).

### 2.2 Client

- `errorInterceptor`: the key is `PROBLEM_TYPE_KEYS[type]`, otherwise `GENERAL_ERROR_KEY` (`ERROR.GENERIC`); the `LegacyErrorBody` path and `core/models/error-translation.map.ts` are gone.
- `features/identity/models/login.model.ts` deleted (no importer since S5-09).
- `de.json`, `en.json`, `es.json`, `fr.json`: the 28 (en/es/fr: 30) dead `AUTH.*` keys and 15 dead `ERROR.*` keys removed. Left: `AUTH.PROFILE`, `.EMAIL`, `.DELETE_ACCOUNT`, `.DELETE_ACCOUNT_WARNING`, `.DELETE_ACCOUNT_VERIFICATION_MSG`, `.YOUR_EMAIL` (legacy profile page and delete-account dialog) and `ERROR.GENERIC`.
- `LEGACY_EXCEPTIONS` unchanged: no listed file was edited or deleted (`identity-api.service.ts`, `with-identity.ts`, profile and settings stay until S5-26/S5-28).

### 2.3 Tests

- `UsersControllerTests` (unit): without user id → 401 and no repository call; another failure throws with the codes, not the texts.
- `UsersControllerAuthorizationTests` (integration): anonymous `DELETE` → 401 Problem Details; the controller has exactly one endpoint, authorised, no anonymous exception; signed-in `DELETE` → 200, soft-deleted, `DeletedAt` = the fake clock's time, new security stamp, address anonymised; a signed-in user the database does not hold → 404 `not-found` without `code`; the three removed actions → 404 `not-found`. The `revoke-sessions` test went with its endpoint.
- `IgnoreQueryFiltersAllowListTests`: the entry of `UserRepo.cs` removed (V236).
- Two tests used a removed action as their sample: API-01's field-path check now posts `{"product":{}}` to `POST /api/Proposals/upsert` (key `product.name`), SEC-03's anonymous 401 now calls `DELETE /api/Users`.
- `error.interceptor.spec.ts`: a legacy `{ code, message }` body shows the general notice.

### 2.4 Documents

ARCHITECTURE_v64 C150 (v62 deleted), TESTING_v72 V235–V236 (v70 deleted), SPEC v2.32 (current-versions line), CODEBASE §2.27 and the new §2.65, CLAUDE.md (version line, state of the code). Not changed: the Figma mapping (no screen), the UX copy (no new key), TECH_DEBT (new version at the end of the sprint: of §5, the rows `Microsoft.AspNetCore.Identity.UI` and `UsersController … { code, message }` are done; `signUp` / `reset-password` 500 and the key `UICustomizationNotFound` went earlier or now — the `ERROR.UICUSTOMIZATION_NOT_FOUND` key is deleted; the in-action 401 of `UsersController` is now an empty 401 that `UseStatusCodePages` turns into Problem Details).

## 3. Self-review

| # | Finding | Sorting |
|---|---|---|
| R1 | The error interceptor spec still described the legacy code path | **fix** — the case now checks the general notice |
| R2 | `UserRepo.RestoreUserAsync` removed although the row does not name it | **kept** — dead code in a file the task changes, and it was the only TEN-02 bypass of the legacy user repository |
| R3 | A failed deletion other than a missing user was a 400 with the Identity texts; now it is a 500 without them | **kept** — such a failure (e.g. a concurrency failure of `UpdateAsync`) is a server fault, not a client error, and the texts must not reach the client (API-01) |
| R4 | The delete-account dialog on the legacy profile page keeps its legacy keys and lint exceptions | **won't fix here** — S5-26/S5-28 replace profile and settings |
| R5 | The first full test run failed in `ProblemDetailsTests` and `CookieSessionTests`, which used `change-email` and `revoke-sessions` as samples | **fix** — new samples (§2.3) |
| R6 | Code review (`/engineering:code-review`): a failed deletion now shows the general notice (404) or S80 (500) in the legacy dialog instead of the specific `ERROR.USER_DELETE` | **won't fix** — only when the account vanished meanwhile or the server fails; the specific text came from the `{ code, message }` body API-01 forbids, and S5-26/S5-28 replace the dialog |
| R7 | Code review: the deletion anonymises the address and ends sessions, but memberships of the deleted user stay | **follow-up** — unchanged behaviour; open point 4 |
| R8 | Security: nothing new is reachable; three authenticated actions fewer; the client never relied on the deleted codes. N+1: none (removals only). a11y: no screen change | — |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1311 passed (unit, architecture, integration); `has-pending-model-changes` for `ApplicationDBContext` exit 0; `check-api-notices.mjs` regenerated (38 packages, only `Microsoft.AspNetCore.Identity.UI` left the file) and checked, 0 errors.
- Client: `npm run lint` clean; `npx ng test` 654 passed (69 files); `npm run build:release` within budget (initial 763.3 kB).
- `check-invariants`, `check-figma-links`, `check-i18n` passed (0 i18n errors); `reuse lint` compliant (1434 files).
- E2E not run locally (no screen changed; the E2E stack ships the migration at startup) — CI is the authority.

## 5. Open points

1. **General notice key:** the UX copy (§6.3) speaks of „dem allgemeinen Hinweis“ for problem types without a text of their own but names no key; the client uses the legacy `ERROR.GENERIC` („Ein Fehler ist aufgetreten. …“). Proposal for the next UX copy version: a key `errors.generic` with that or a revised wording, then the interceptor switches and `ERROR.GENERIC` goes.
2. **#158** stays open: the M3 entries of profile, settings, `identity-api.service.ts` and `with-identity.ts` go with S5-26/S5-28; M2 and M5 entries with their steps.
3. **TECH_DEBT §5** (end of sprint 05): mark `Microsoft.AspNetCore.Identity.UI` and the `{ code, message }` failures done (S5-10).
4. **Account deletion:** the legacy deletion leaves the deleted user's organisation memberships (and a last-Owner organisation) in place; the account deletion of S61 (S5-28, R-AUTH) has to decide what happens to them.

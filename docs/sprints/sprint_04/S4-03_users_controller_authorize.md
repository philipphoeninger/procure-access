# S4-03 — `UsersController` requires authentication

> **Sprint:** 04, P0 task S4-03 (debt, 0.5 points). **Issues:** #205. **Requirements / invariants:** SEC-01 (legacy JWT) unchanged; API-01 stays green; no invariant activated, no new TESTING version. **Status:** done by Claude Code (2026-11-10), pull request open for review. **Deviation from the row:** no `[AllowAnonymous]` on `ResetPassword`. The action is removed instead (owner decision 1).
> **Scope:** exactly the sprint row: `[Authorize]` on `UsersController`, 401 tests, and the null-forgiving `!` on the user id replaced by a real check. No other behaviour change; the controller is replaced in migration step 3 (ARCHITECTURE §7.1).
> **Owner decisions** (2026-11-10, before the start):
> 1. **O-S4-7 (a):** remove `POST /api/Users/reset-password` instead of marking it `[AllowAnonymous]` (F2). Options (b) `[Authorize]` (any signed-in user could still set another user's password) and (c) `[AllowAnonymous]` as the row said (keeps the hole) were rejected. Recorded in `SPRINT_04.md`, owner decisions.
> 2. A principal without a user id answers 401 (plan, no objection): it counts as not signed in.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `UsersController` has no `[Authorize]`, and the fallback policy is `null` (`IdentityExtensions.cs`), so all five actions accepted anonymous requests. Four of them read the user id with `User.FindFirstValue(ClaimTypes.NameIdentifier)!` (S3-08 F7, R2) | `[Authorize]` on the class; a helper `CurrentUserId()` replaces the `!` |
| F2 | **`POST /api/Users/reset-password` took only `email` and `newPassword`. `UserRepo.ResetPasswordAsync` generated the reset token itself and set the password**. So anyone who knew a user's e-mail address could set that user's password without signing in and without a mail token. This is reachable today on every deployed instance that runs the current code | Owner decision 1: remove the action, `IUserRepo.ResetPasswordAsync`, `UserRepo.ResetPasswordAsync` and `ResetPWRequest` (no other users) |
| F3 | Legacy client (`CLIENT/…/src/app`): the only `UsersController` action it calls is `DELETE /api/Users` (`identity-api.service.ts`, account deletion). `change-email`, `change-password` and `revoke-sessions` have no caller. The client's password reset posts to the token-based minimal API `POST /api/reset-password` (`auth.service.ts` → `IdentityUserEndpoints` → `UserService.ResetPasswordAsync` with the mail token) | `[Authorize]` breaks no client flow, and removing the token-less action leaves the reset flow unchanged |
| F4 | API-01 (`ProblemDetailsTests`) sends signed-in test users (Approver, Member) to every write endpoint and to `change-email` | Stays green with `[Authorize]`; one write endpoint fewer in its discovery |
| F5 | An anonymous request to an `[Authorize]` action is answered by the authentication challenge (JWT; the test scheme in integration tests): an empty 401, which Platform's `UseStatusCodePages` turns into Problem Details (`application/problem+json`, `status` 401). SPEC §8.6 has no problem type for 401, so the document keeps the framework's type | The tests check status, media type and the `status` member, not a `type` |
| F6 | The in-action 401 (`Unauthorized()` when the user id is missing) also becomes Problem Details, but as `application/json`, because of the legacy `[Produces("application/json")]` on every action (S2-10, open points) | A missing `[Authorize]` would still give 401 through the in-action check. So a separate test pins the attribute through the endpoint metadata (§3, R1) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `API/API.ProcureAccess/Controllers/Identity/UsersController.cs` | `[Authorize]` on the class (default policy: signed-in user, JWT scheme); no `[AllowAnonymous]`; `ResetPasswordAsync` action removed; `CurrentUserId()` (null for a missing or empty `NameIdentifier`) → `Unauthorized()` before the repository is called in the four actions; class summary | F1, F2, decisions 1 and 2 |
| `DAL/DAL.ProcureAccess/Repos/Interfaces/IUserRepo.cs`, `Repos/UserRepo.cs` | `ResetPasswordAsync(email, newPassword)` removed | F2: the token-less reset is gone, not only unreachable |
| `DAL/MODELS.ProcureAccess/Entities/Identity/ResetPWRequest.cs` | Deleted | Only the removed action used it |
| `tests/ProcureAccess.Tests.Integration/Api/UsersControllerAuthorizationTests.cs` (new) | Anonymous call of each of the four actions → 401 Problem Details; endpoint metadata: every `Users` endpoint carries `IAuthorizeData` and none `IAllowAnonymous`, and there are exactly four; signed-in Member with a stored user row → `revoke-sessions` 200 and the security stamp changes; `POST /api/Users/reset-password` → 404 `not-found`, anonymous and signed in | Row: "anonymous calls answer 401" (#205 "Done when"); decision 1 |
| `tests/ProcureAccess.Tests.Unit/Identity/UsersControllerTests.cs` (new) | Each of the four actions with a signed-in principal without `NameIdentifier` → `UnauthorizedResult`, repository not called | Row: "the `!` replaced by a real check" |
| `CLAUDE.md` | §3: account actions need sign-in, token-less reset removed; header entry | CLAUDE §11 |
| `docs/sprints/sprint_04/SPRINT_04.md` | Owner decisions: O-S4-7 with the decision | Decision 1 |
| `docs/sprints/sprint_04/S4-03_users_controller_authorize.md` | This protocol | – |

No invariant tag on the new tests. SEC-03 ("API endpoints answer an unauthenticated request with 401") describes the cookie session of step 3 and stays `planned`. ARCHITECTURE, SPEC and TESTING do not name these legacy endpoints, so they are unchanged.

**Behaviour change on the client side (intended):** for a signed-in user whose JWT has expired, `DELETE /api/Users` now answers 401 instead of 400. The legacy `auth.interceptor.ts` then refreshes the token and retries; before, the deletion failed with 400.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | Every remaining action needs a signed-in user; the token-less password reset is gone (F2). The user id comes only from the principal, never from the request. The token-based reset (`POST /api/reset-password`) is untouched. No secret or personal data in the tests (`example.test` addresses) |
| Correctness | `CurrentUserId()` treats a missing and an empty claim alike; the model-state check stays first, as before. Probes (reverted): without `[Authorize]`, the metadata test and the four anonymous tests fail; with it, all pass |
| Edge cases | Unknown test user → authentication fails → 401 (existing `TestAuthenticationTests`). Signed in, but no user row → the repository answers "User not found" as before (unchanged 400) |
| Performance, N+1 | No query added; the test reads the security stamp once |
| a11y, UI texts | Not touched |
| Maintainability | One helper instead of four `!`; dead repository method and DTO removed |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The anonymous 401 test alone cannot tell the attribute from the in-action check (F6). A probe without `[Authorize]` failed only on the media type | **Fixed:** the metadata test pins the attribute directly |
| R2 | The in-action 401 carries `application/json`, the challenge 401 `application/problem+json` (F6) | Won't fix: legacy `[Produces]`, known since S2-10; the controller goes with step 3 |
| R3 | `UICustomizationController` has no `[Authorize]` either, but it already answers 401 itself when the user id is missing | Won't fix in this task (row scope); same outcome for the client; P3 |
| R4 | `revoke-sessions` renews the security stamp, but the legacy JWT does not check it, so issued tokens stay valid until they expire | Unchanged behaviour, out of scope; step 3 (SEC-08); P2 |

No correctness or security bug left that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | 0 warnings, 0 errors |
| C2 | `dotnet format ProcureAccess.sln --verify-no-changes` | Exit 0 |
| C3 | `dotnet test --solution ProcureAccess.sln` | 651 passed (Unit 422, Architecture 59, Integration 170), 0 failed; API-01 green |
| C4 | Probe: `[Authorize]` removed, `UsersControllerAuthorizationTests` run, attribute restored | 5 of 8 fail (metadata test and the four anonymous calls); with the attribute 8 of 8 pass |
| C5 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` | Green |
| C6 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1064 / 1064 files |
| C7 | Client, E2E | Not touched (no client file changed); CI runs them |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | F2 was reachable on every deployed instance until this merge | Owner: deploy staging from `main` after the merge (together with TD-28, #236) and look in the logs (`logging."SeriLogs"`, proxy logs) for `POST /api/Users/reset-password`. Any call there may have set a password. If accounts there are real, consider a forced password reset |
| P2 | `revoke-sessions` and the password change renew the security stamp, but the legacy JWT does not validate it (R4) | No change; SEC-08 covers it with the cookie session in step 3 |
| P3 | `UICustomizationController` and the generic read controllers have no `[Authorize]` (`FavoritesController.cs` is commented out entirely); UI customisation checks the user id itself (R3) | No issue proposed: the read controllers are public by design, UI customisation already answers 401, and both are rebuilt in step 3 (preferences) or retired |
| P4 | `POST /api/forgot-password` sends the reset mail to the fixed address `ok@testing.lettermint.co`, not to the requested one (`IdentityUserEndpoints.cs`; known test sink, S1-08 K5) | Unchanged; the reset flow is rebuilt in step 3. A real user cannot reset their password on staging until then |

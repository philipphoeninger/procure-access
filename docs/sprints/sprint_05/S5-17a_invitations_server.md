# S5-17a — Invitations by e-mail (server)

> **Sprint:** 05 · **Task:** S5-17a (part 1 of S5-17, 3 points together) · **Issues:** none
> **Requirements / invariants:** SPEC R-ORG-02, R-ORG-03 (revoking), server part; **SEC-11 complete** with the invitation tokens (TESTING_v68 V219); SEC-10 accept case, integration part (V220); TEN-05 rows of the invitation endpoints (V221); TEN-01 sample and TEN-02 allow-list for invitations (V222); API-05 `Invitation` (V223); ARCHITECTURE_v61 C147; SPEC v2.29 §8.6, §8.9
> **Status:** done — pull request #343 open (2026-11-15)
> **Scope:** `OrganizationInvitation` with its migration, the contract `IOrganizationInvitations` and its service, five endpoints in the host (invite, pending list, revoke, read by token, accept), the problem type `invitation-mismatch`, the rate limit `Invitation`, the clean-up job and the tests. No screen, UI text or E2E change (S5-17b).
> **Owner decisions (2026-11-15, before the task):** D1 (a) — S5-17 is split: S5-17a server, S5-17b client · D2 (a) — S5-17b drafts the missing UX copy keys as v31 for review · D3 (a) — a new invitation replaces the open one of the same address; a member's address is refused (400 at `email`) · D4 (a) — the pending list for Owners only · D5 (a) — the page of the link shows organisation and role without sign-in; the invited address only in the mismatch answer to a signed-in user · D6 (a) — SPEC as written: the address is confirmed first, accepting never confirms it · D7 (a) — revoking deletes the row; an accepted row stays until it expires so a second click leads to the organisation; a clean-up job deletes expired rows · D8 (a) — the rate limit „invitation“ of SPEC §7.3 is its own policy, 5 per hour per user 🔧. **After the plan:** the mismatch answer (a) — a new problem type `invitation-mismatch` (403) with the extension `invitedEmail`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `OrganisationInvitationEmail` and the texts `email.invitation.*` exist since S5-11; the organisation endpoints, the row lock and the `ConfirmedEmail` gate since S5-16a / S5-12a | Reused as they are |
| 2 | `OrganizationInvitation` existed only in ARCHITECTURE §5.2 (`Email`, `Token`, `ExpiresAt`) | New entity and migration in IdentityAccess |
| 3 | `IAccountTokens` is keyed by a user id; an invitee may have no account yet | The invitation stores its own token hash, made the same way (`AccountTokenService.TokenBytes`/`TryHash`) |
| 4 | SPEC R-ORG-02 asks for „a clear message“ on another address, but §8.6 had no fitting type | Owner decision after the plan: `invitation-mismatch` |
| 5 | ARCHITECTURE §7.1 sketched `POST /invitations/{token}/accept`, with the token in the path | Token in the body of `POST /invitations/view` and `/invitations/accept`, so it never reaches a request log (C147 (4)) |
| 6 | S5-16a open point 2 (R5): accepting must not wait on the organisation lock longer than a role change | Accepting takes the same lock for one membership insert and one update; inviting and revoking take it too |
| 7 | No open pull request touched IdentityAccess (one migration branch per module) | Migration `AddOrganizationInvitations` |

## 2. Changes

### 2.1 Contracts

- `src/ProcureAccess.Modules.IdentityAccess.Contracts/Organizations/IOrganizationInvitations.cs`: `Lifetime`, `InviteAsync`, `GetPendingAsync`, `RevokeAsync`, `FindAsync`, `AcceptAsync`; records `IssuedInvitation` (with `ValidDays`, rounded up), `PendingInvitation`, `InvitationDetails`, `InvitationAcceptance`; enum `InvitationAcceptanceOutcome` (`Accepted`, `AlreadyMember`, `Invalid`, `AddressMismatch`). Every record that holds a token or an address has a `ToString` without it (PRV-11).
- `src/ProcureAccess.Modules.Platform.Contracts/Errors/ProblemTypes.cs`: `InvitationMismatch = "invitation-mismatch"`, also in `All`.
- `src/ProcureAccess.Modules.Platform.Contracts/RateLimiting/RateLimitPolicies.cs`: `Invitation`.

### 2.2 IdentityAccess module

- `Domain/OrganizationInvitation.cs`: `Create` (organisation, address as typed and trimmed, `NormalizedEmail` = upper invariant like Identity's normaliser, role, 32-byte hash, 🔧 lifetime), `IsOpenAt`, `IsFor`, `Accept`.
- `Persistence/Configurations/OrganizationInvitationConfiguration.cs`: table `identity."OrganizationInvitations"`, unique token hash, partial unique index (organisation, normalised address) where `AcceptedAt IS NULL`, index on `ExpiresAt`, cascade with the organisation. `IdentityAccessDbContext` binds the organisation filter.
- `Persistence/Migrations/…_AddOrganizationInvitations.cs` (made `internal`, ARC-03).
- `Organizations/OrganizationRowLock.cs`: the raw-SQL row lock, moved out of `OrganizationMemberService`; it refuses to run outside a transaction.
- `Organizations/OrganizationInvitationService.cs`: inviting, revoking and accepting in a transaction inside the execution strategy, under the row lock; accepting reloads the invitation after the lock (an acceptance, revocation or replacement committed meanwhile counts), then checks: accepted (member → `AlreadyMember`, else `Invalid`), expired, address, and creates the membership with `InvitedAt` = the invitation's creation. The read by token joins the organisation's name past the filter in one query (TEN-02).
- `Organizations/InvitationSettings.cs`: 🔧 `Organizations:Invitations:Lifetime` (14 days) and `:CleanupInterval` (1 h); a bad value stops the start.
- `Organizations/InvitationCleanupService.cs`: deletes expired invitations at start and every interval, on `TimeProvider`.
- `IdentityAccessModule.cs`: registers settings, service and hosted job (the role command host never starts its host).

### 2.3 Host

- `API/Organizations/InvitationEndpoints.cs`: the five endpoints (SPEC §8.9 v2.29). Inviting checks the Owner role, the address (as registration does), a member's address (one query over the legacy users), then invites and sends the e-mail; a failed send answers 503 `email-delivery` and keeps the invitation. `view` is anonymous; `accept` carries `ConfirmedEmail`.
- `API/Organizations/OrganizationEndpoints.cs`: role map, `RefusedAsync`, `UserIdOf`, `NameOf` are `internal` for the invitation endpoints.
- `API/Auth/ClientLinks.cs`: `InvitationPagePath` `/organisation/einladung`.
- `Program.cs`: `MapInvitationEndpoints()` after the organisation endpoints.
- `appsettings.json`: `RateLimits:Invitation` (5 per hour), `Organizations:Invitations`.
- `docker-compose.e2e.yml`: relaxes `RateLimits:Invitation`.

### 2.4 Tests

- `tests/ProcureAccess.Tests.Unit/IdentityAccess/OrganizationInvitationTests.cs`: creation, expiry boundary and single acceptance (`INV:SEC-11`), address comparison, refusals, `DaysOf`, settings.
- `tests/ProcureAccess.Tests.Integration/Organizations/InvitationEndpointsTests.cs` (37 cases): e-mail and link, hash only, replace, member's address, invalid bodies, failed send, pending list, revoke, the read by token (anonymous, matching, other address, unknown), accepting (role and times, second click, used by someone else, two at once, 14 days minus 1 s / 14 days on the shared fake clock), unconfirmed and then confirmed (`INV:SEC-10`), mismatch, anonymous, the clean-up query and the job on the clock. A mutation check confirmed that the concurrency case fails without the reload after the lock.
- `Tenancy/AuthorizationMatrixTests.cs`: 18 rows for invite, list and revoke (`INV:TEN-05`).
- `Tenancy/OrganizationIsolationTests.cs`: sample for `OrganizationInvitation` (`INV:TEN-01`).
- `tests/ProcureAccess.Tests.Architecture/Tenancy/IgnoreQueryFiltersAllowListTests.cs`: `OrganizationRowLock.cs` replaces the member service; the read by token and the clean-up added, each with its reason (`INV:TEN-02`).
- `Api/RateLimitTests.cs`: `Invitation` per user (`INV:API-05`) and its default in `Configuration_Defaults_AreTheValuesOfSpec73`.
- `Infrastructure/ProcureAccessApiFactory.cs`: relaxes `RateLimits:Invitation` for the shared host.

### 2.5 Documents

- ARCHITECTURE_v61 (C147; §5.2 row, §7.1 endpoint note, §12 step 3); ARCHITECTURE_v59 deleted.
- TESTING_v68 (V219–V223): SEC-11 complete, SEC-10 accept case, TEN-05 rows, TEN-01/TEN-02, API-05; TESTING_v66 deleted.
- SPEC v2.29: §8.6 `invitation-mismatch`, §8.9 invitations, current versions. No acceptance criterion changes; nothing in §13 opened or closed.
- CLAUDE.md: version line, §3 (S5-17a paragraph), §4 (host `Organizations/`, IdentityAccess `Organizations/` and `Domain/`, Contracts, `Integration/Organizations/`), §5 (curl examples, rate limit line).
- No UX copy, `de.json`, Figma mapping or `LEGACY_EXCEPTIONS` change: no screen.

## 3. Self-review

Checklist of CLAUDE §6.7 over the whole diff (`/engineering:code-review`); findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | A revocation between an acceptance's reload and its save made that save find no row (`DbUpdateConcurrencyException` → 500) | **fixed** — revoking takes the organisation lock too |
| R2 | The integration test's check that no column holds the token in text compared a Base64url token with a `bytea` row text, so it could never fail | **fixed** — removed; the hash comparison stays |
| R3 | The clean-up job deletes without the lock; an acceptance in the same milliseconds in which the invitation expires could find its row gone at save (500) | **won't fix** — only at the expiry instant; the user opens the link again and gets „nicht mehr gültig“, which is the right answer |
| R4 | Owner re-invites while the invitee accepts: if the acceptance commits first, the new invitation is for a member now | **won't fix** — harmless: accepting it answers `AlreadyMember`, and it expires or is revoked |
| R5 | Security: the token is 256 random bits, only in a POST body, compared by hash; the read by token reveals no address to an anonymous caller; the mismatch answer names the invited address only to a signed-in, confirmed holder of the link (D5 (a), SPEC R-ORG-02); antiforgery on every POST/DELETE; the Owner endpoints check the route's organisation first | — |
| R6 | Filter bypass only at the three allow-listed sites; the lock is taken after a filtered read (invite, revoke) or after the token found the invitation (accept) | — |
| R7 | N+1: inviting runs the members read, one address query and the invite transaction; the read by token is one query | — |
| R8 | The integration tests advance the shared fake clock (14 days); time only moves forward, and the classes run one after another | — |
| R9 | No new package or secret; logs name the organisation id, never an address or a token (PRV-11) | — |

No correctness or security finding that affects current behaviour remains.

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1301 passed (Unit 753, Architecture 63, Integration).
- `dotnet ef migrations has-pending-model-changes` for `IdentityAccessDbContext`: no changes (OPS-03).
- `node scripts/check-invariants.mjs` passed (TESTING_v68); `check-figma-links`, `check-i18n` passed; `reuse lint` compliant.
- No E2E run: no client change (S5-17b).

## 5. Open points

1. **S5-17b:** the invite form and the pending list with revoking on S21, the page `/organisation/einladung` (signed out, unconfirmed with `pa-confirm-required-notice`, mismatch, invalid, accepted → S21), `PROBLEM_TYPE_KEYS` for `invitation-mismatch`, the UX copy v31 drafts (D2 (a)), SEC-10's E2E accept case, an E2E test that builds its own organisation (role change, removal and leaving end to end, S5-16b open point 2) and the reflow check.
2. **UX copy keys still missing** for S5-17b (to be drafted): the page's H1 and page titles, the signed-out prompt with the hint to open the link again after registering, headings of the invite form and the pending list, the announcements after revoking and accepting, and „already a member“ for the 400 at `email`.
3. **Rate limit:** 5 invitations per hour per Owner 🔧 means onboarding ten colleagues takes two hours; the value stays as confirmed (SD23) unless the owner changes it.
4. From S5-16a, still open: no rate limit on creating organisations, a sole member cannot leave.

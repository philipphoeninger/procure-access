# S5-27 — Personal data export

> **Sprint:** 05 · **Task:** S5-27 (M3.8 part, 2 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-AUTH-09, §8.4, SD14 (v2.33); **PRV-07 account part `active`** (TESTING_v73 V237); ARCHITECTURE C79, C151
> **Status:** done — pull request #352 open (2026-11-16)
> **Scope:** `GET /api/v1/account/export` returns the signed-in user's own data as JSON per SPEC §8.4, nothing of other users, as an attachment with `no-store`. No screen: the download on S61 is S5-28.
> **Owner decisions (2026-11-16):** S5-27 **1a** — the full shape of §8.4 now (`formatVersion` 1); sections whose data later tasks build are empty arrays, `preferences` is `null`; each later task fills its section and its PRV-07 case, recorded as deferred parts in TESTING; the S5-19 and S5-25 prompts get one line · **2a** — the legacy `User` gets `CreatedAt`, set at creation from `TimeProvider`; existing accounts `null` · **3a** — exactly what §8.4 lists; legacy proposals and open invitations stay out (open point) · **4a** — no rate limit (§7.3 names none) · **PRV-04** (after the review, R1) — the export is PRV-04's one exception

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | Nothing served `/api/v1/account/export`; ARCHITECTURE §7.2 listed it, SPEC §8.4 defined the file | Built in the host |
| 2 | Memberships (`OrganizationMembership` with `AcceptedAt`) and contributor profiles (`ContributorProfile`, `UserContributorLink`, S4-25) live in IdentityAccess; address and confirmation in the legacy `User` | A contract of IdentityAccess for memberships and profiles; the endpoint in the host next to `API/Auth/` (until M3.1) |
| 3 | The legacy `User` had no creation time, §8.4 has `account.createdAt` | Decision 2a: `User.CreatedAt` with a legacy migration |
| 4 | `UserPreferences` (S5-25), criteria lists (S5-19), projects (step 5) and contributions (step 6) do not exist yet, while the task depends only on S5-15 and its extra instructions name preferences and saved lists | Decision 1a |
| 5 | The legacy user also has proposals (`Proposals.ProposerId`) and possibly open invitations to the address; §8.4 names neither | Decision 3a: not exported (open point 2) |

## 2. Changes

### 2.1 Server

- `IdentityAccess.Contracts/Accounts/IAccountDataExport.cs`: `GetAsync(userId)` → `AccountDataPart` with `ExportedMembership(Organization, Role, Since)` and `ExportedContributorProfile(DisplayName, Kind, CreatedAt)`; no ids. `Kind` is already spelled as §8.4 names it, so `ContributorKind` stays internal.
- `IdentityAccess/Accounts/AccountDataExportService.cs`: memberships joined with their organisation through the organisation query filter of the request (it holds exactly the signed-in user's memberships, so no `IgnoreQueryFilters()`), `Since` = `AcceptedAt ?? InvitedAt`, ordered by name (German collation); profiles through `UserContributorLink` by `UserId`, ordered by creation time. Registered in `IdentityAccessModule`.
- `API/Account/AccountExportEndpoints.cs`: `GET /api/v1/account/export`, mapped on the versioned group; sign-in required (401), no confirmation gate, GET without antiforgery, no rate limit. `Cache-Control: no-store` on every answer of the handler; 404 for a soft-deleted account; 200 with the indented JSON of §8.4 (records `AccountExport*` in its order, camelCase) as `application/json` attachment `ProcureAccess_Daten_<YYYY-MM-DD>.json` (`FileNameOf`, UTC day of `generatedAt` like the criteria export). Roles through `OrganizationEndpoints.NameOf`. `preferences` `null`, `contributions`, `createdByMe.criteriaLists` and `createdByMe.projects` empty (decision 1a). Global using `API.ProcureAccess.Account`.
- `User.CreatedAt` (`DateTimeOffset?`, `timestamptz`); legacy migration `AddUserCreatedAt` (`ApplicationDBContext`, `identity."Users"`); `ClockedUserManager.CreateAsync(User)` sets it from `TimeProvider` unless set, so registration and the E2E seeder stamp it (decision 2a).

### 2.2 Client

None — S61 with the download button is S5-28.

### 2.3 Tests

- `tests/ProcureAccess.Tests.Integration/IdentityAccess/AccountExportTests.cs` on the Tenants fixture with the legacy accounts stored: `INV:PRV-07` — Editor A (Editor of A, Reader of B from a day later, one profile; Owner B has another) gets exactly the own address, confirmation, `createdAt`, both memberships and the own profile, and the raw body contains no address or id of any other user, not the own user id, no organisation or profile id and not Owner B's profile name; `INV:PRV-07` — a user without membership or profile gets every key of §8.4 in its order, `formatVersion` 1, `generatedAt` from the fake clock, `createdAt` and `preferences` `null`, every array empty. Untagged: attachment name, JSON type and `no-store`; anonymous 401; soft-deleted account 404 with `no-store`.
- `RegistrationTests`: the registered account has `CreatedAt` = the fake clock.
- `PlatformRoleMigrationTests` (`INV:TEN-06`) migrated to `RemoveHrMobileExclusions` and stored its two legacy users through the current `User` model, which now has `CreatedAt` — the first full run failed with `column "CreatedAt" of relation "Users" does not exist`. It now stores them with SQL in the schema of that migration, so later `User` columns cannot break it again.

### 2.4 Documents

TESTING_v73 (PRV-07 account part `active` with its deferred parts, V237; v71 deleted), ARCHITECTURE_v65 (C151, the endpoint map; v63 deleted), SPEC v2.33 (change rows v2.33-1 – 3 and a note in §8.4), `CODEBASE_v1.md` §2.66, §3 and §4 (a `curl` line), `CLAUDE.md` (state line, last task), `TASK_PROMPTS.md` (S5-19 and S5-25 fill their export section, decision 1a). No Figma mapping, UX copy or `LEGACY_EXCEPTIONS` change.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | **PRV-04** (planned, identity part step 3) reads „no endpoint returns both a user identifier and a contributor profile“; the export returns the address with the user's own profiles, as SPEC §8.4 and R-AUTH-09 („never … `UserContributorLink` beyond the user's own profiles“) require | **decided by the owner** (2026-11-16): the export is PRV-04's one exception — PRV-04 in TESTING_v73 (V238) and ARCHITECTURE §9.3 / C151 say so |
| R2 | `since` falls back to `InvitedAt` for a membership without acceptance | **kept** — invitations live in their own table since S5-17a, so every membership row is accepted; the fallback only keeps the field non-null |
| R3 | `IAccountDataExport` reads memberships through the request's organisation filter, so for another user than the signed-in one it would return no memberships | **kept** — the only caller passes the signed-in user; the contract remarks say so |
| R4 | Accounts created before S5-27 export `createdAt: null` | **kept** — decision 2a; pre-pilot data |
| R6 | The first full test run failed in `PlatformRoleMigrationTests` (a user row through the current model on an old schema) | **fix** — SQL in the old schema (§2.3) |
| R5 | Security: user id only from the principal, no route or body input, nothing logged, `no-store` before every branch. N+1: two queries bounded by the user's rows on indexed `UserId` columns. a11y: no screen | — |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1316 passed (unit, architecture, integration; the first run had the one failure of R6); `has-pending-model-changes` for `ApplicationDBContext` exit 0. No NuGet change, so `THIRD-PARTY-NOTICES.txt` unchanged.
- `check-invariants` passed (PRV-07 `active` with its two tagged tests), `check-figma-links` and `check-i18n` passed (0 errors); `reuse lint` compliant.
- Client and E2E not run: no client file changed; the E2E stack applies the migration at startup — CI is the authority.

## 5. Open points

1. **PRV-04** (R1): decided — the export is its one exception (TESTING V238); the endpoint scan of PRV-04's identity part has to skip it by name when it is activated.
2. **Legacy proposals** (decision 3a): the proposals a user made (`Proposals.ProposerId`) are personal data outside §8.4; they go with the legacy product proposal of their step. If they outlive the pilot start, the export or the deletion has to cover them.
3. **Sections of later tasks** (decision 1a): S5-19 (`createdByMe.criteriaLists`), S5-25 (`account.preferences`), step 5 (`createdByMe.projects`, PRV-07 workspace part), step 6 (`contributions`, purged content as null); the S5-19 and S5-25 prompts name it.
4. **S5-28** builds the download on S61 (`account.export.*` of the UX copy) on this endpoint.

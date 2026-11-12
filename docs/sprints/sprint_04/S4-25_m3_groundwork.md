# S4-25 — M3.4 groundwork: contributor profiles and the Moderator role

> **Sprint:** 04, P2 stretch task S4-25 (2 points). **Issues:** none. **Requirements / invariants:** ROADMAP M3.4, ARCHITECTURE §5.2, §9.1, §12 step 3; SPEC R-ADM-03; **TEN-06** `active` for the part that exists before step 3 (TESTING_v54 V169); prepares PRV-04, PUB-* (step 6).
> **Status:** done — pull request #300 open.
> **Scope:** the entities `ContributorProfile` and `UserContributorLink` in the IdentityAccess module with their migration (no endpoints); the legacy roles renamed to the platform roles of §9.1 by a data migration, with the legacy client's checks; a claim policy for the approve endpoint; the `roles` command of the API host to grant, revoke and list platform roles. Not here: users, memberships and the JWT (M3.1), organisations (M3.3), contributor-profile endpoints and screens (S50, step 6), PRV-04.
> **Owner decisions (2026-11-12, all (a)):** **O-S4-81** — the legacy `Admin` is renamed to `PlatformAdmin` in the same migration and keeps `moderation:review` until the proposals go. **O-S4-82** — the migration makes sure `PlatformAdmin`, `CatalogEditor` and `Moderator` exist in every environment. **O-S4-83** — `Proposal.ApproverId` keeps its name until step 5. **O-S4-84** — TEN-06 `active` for the rename and the legacy approve endpoint; the endpoints of `CatalogEditor` and `PlatformAdmin` deferred. **O-S4-85** — a claim policy `ModerationReview` replaces `AuthApproverAttribute` (403/401 instead of 500). **O-S4-86** — asked before the JWT or the legacy account pages change: the token carries the new names from its next refresh, and the profile page and three other client checks use `moderation:review`. Defaults stated before the task and not objected to: the tables in `IdentityAccessDbContext`, display name ≤ 60 and unique regardless of case, at most one user per profile, no foreign key from `UserId` to the legacy users; the command as a host command mode `roles list|grant|revoke`, on the legacy `UserManager`, no JWT change.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Users, roles and the JWT are in the legacy context; IdentityAccess has its own context with the Data Protection keys only (S3-24) | The profile tables go into `IdentityAccessDbContext`; `UserContributorLink.UserId` gets no foreign key (ADR-014: no key across contexts) |
| F2 | Role rows were created only by `AddRolesAndPermissionsAsync` inside the Development rebuild (SEC-06): `Admin` and `Approver` with `objects:approve`, `Member` | Other environments may have no role rows at all; the platform roles must come from a migration to exist everywhere (O-S4-82) |
| F3 | `AuthApproverAttribute` threw `UnauthorizedAccessException`, which nothing maps, so `POST /api/Proposals/approve` answered 500 to a user without the permission (and to an anonymous request the class-level `[Authorize]` answered 401 first) | TEN-06 needs a proper rejection: claim policy (O-S4-85) |
| F4 | The legacy client checks `'objects:approve'` in the proposal-list route, both proposal pages and the legacy profile page (hides the user's own proposals from approvers) | The task asks before the account pages change: O-S4-86 |
| F5 | The JWT is built from the role and permission names in the database at sign-in and refresh (`UserService.GenerateAuthResponseAsync`), lifetime 30 minutes | No JWT code change; a token issued before the deployment keeps the old names for at most 30 minutes |
| F6 | EF Npgsql writes a column's collation without schema and quotes a schema-qualified name as one identifier; PostgreSQL finds an unqualified collation only on the search path (checked on `postgres:17-alpine`) | The case-insensitive ICU collation lives in `public`, named `identity_case_insensitive` |
| F7 | Respawn deleted every row except a list of reference tables | The role tables are now reference data of a migration: Respawn keeps `identity."Roles"` and `"RoleClaims"` |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `src/ProcureAccess.Modules.IdentityAccess/Domain/ContributorProfile.cs`, `UserContributorLink.cs`, `ContributorKind.cs` (new, `internal`) | Profile with random `Guid` id, display name ≤ 60 (non-empty), kind, `CreatedAt` passed in from `TimeProvider`; link keyed by `ContributorProfileId` | ARCHITECTURE §5.2; SPEC §7.2, R-CTR-01 |
| `src/ProcureAccess.Modules.IdentityAccess/Persistence/Configurations/ContributorProfileConfiguration.cs`, `UserContributorLinkConfiguration.cs` (new); `IdentityAccessDbContext.cs` | Tables `identity."ContributorProfiles"`, `identity."UserContributorLinks"`; unique index on the display name with the ICU collation `public.identity_case_insensitive` (`und-u-ks-level2`, non-deterministic); index on `UserId`; link deleted with its profile | ADR-014 |
| `src/ProcureAccess.Modules.IdentityAccess/Persistence/Migrations/*_AddContributorProfiles.cs` (new, `internal`), snapshot | Creates the collation and both tables | ADR-014 |
| `DAL/DAL.ProcureAccess/Migrations/*_RenamePlatformRoles.cs` (new), snapshot (product version 10.0.15 only) | SQL: rename `Approver` → `Moderator`, `Admin` → `PlatformAdmin` (same ids), `objects:approve` → `moderation:review`; insert missing platform roles; `moderation:review` once on Moderator and PlatformAdmin. Down restores the legacy names and drops `CatalogEditor` | TEN-06; O-S4-81, O-S4-82 |
| `DAL/MODELS.ProcureAccess/Entities/Authorization/Roles.cs`, `Permissions.cs` | `PlatformAdmin`, `CatalogEditor`, `Moderator`, `PlatformRoles`; `ModerationReview` replaces `ObjectsApprove` | §9.1 |
| `SERVICES/.../ProposalService.cs` | Reads `Permissions.ModerationReview` | O-S4-81 |
| `API/API.ProcureAccess/Extensions/AuthorizationExtensions.cs` | `AuthorizationPolicies.ModerationReview` and `AddAuthorizationPolicies()`; `AddRolesAndPermissionsAsync` became `AddMemberRoleAsync` (rebuild creates only `Member`) | O-S4-85; SEC-06 unchanged |
| `API/API.ProcureAccess/Authorization/AuthApproverAttribute.cs` (deleted), `Controllers/ProposalsController.cs`, `Program.cs`, `GlobalUsings.cs`, `Extensions/DatabaseRebuildExtensions.cs` | Approve endpoint uses the policy; policies registered; `roles` command mode before the web host | O-S4-85; R-ADM-03 |
| `API/API.ProcureAccess/Commands/RoleCommandLine.cs`, `Extensions/RoleCommandHost.cs` (new) | `roles list [<email>]`, `roles grant <email> <role>`, `roles revoke <email> <role>`; host with Platform, IdentityAccess, legacy context and Identity (no JWT, no migration); refuses with pending migrations; only platform roles (case-insensitive); logs user id and role | R-ADM-03 |
| `CLIENT/.../app.routes.ts`, `product-proposal.html`, `criterion-proposal.html`, `identity/profile/profile.html`, `identity/models/user-roles.enum.ts` | `'moderation:review'`; role enum with the platform roles | O-S4-86 |
| `e2e/shell/start-and-error-pages.spec.ts` | Comment only („proposal list for moderators“) | — |
| `tests/ProcureAccess.Tests.Shared/Auth/TestUsers.cs`; `tests/.../Api/ProblemDetailsTests.cs`, `GenericCrudWriteTests.cs` | `TestUsers.Approver` → `TestUsers.Moderator` with `moderation:review` | — |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/ApiFixture.cs` | Respawn keeps the role tables; `StoreAsync` also stores IdentityAccess entities | TESTING_v54 V170 |
| `tests/ProcureAccess.Tests.Integration/Persistence/PlatformRoleMigrationTests.cs` (new) | Legacy roles with users and claims migrated (`INV:TEN-06`); empty database gets the three roles (`INV:TEN-06`); down migration restores the legacy names | TEN-06 |
| `tests/ProcureAccess.Tests.Integration/Api/ModerationPolicyTests.cs` (new) | Approve: anonymous 401; member 403 Problem Details, proposal unchanged (`INV:TEN-06`); moderator rejects the proposal, 200 (`INV:TEN-06`) | TEN-06 |
| `tests/ProcureAccess.Tests.Unit/Commands/RoleCommandLineTests.cs`, `tests/ProcureAccess.Tests.Integration/Operations/RoleCommandTests.cs` (new) | Usage errors before any database access; grant, revoke, already/not held, unknown address, list all and one | R-ADM-03 |
| `tests/ProcureAccess.Tests.Unit/IdentityAccess/ContributorProfileTests.cs`, `tests/ProcureAccess.Tests.Integration/IdentityAccess/ContributorProfilePersistenceTests.cs` (new) | Factory checks; round trip; „leselupe“/„LESELUPE“ refused next to „Leselupe“, „Bär“ next to „Bar“ kept; second link of a profile refused; several profiles per user; link deleted with its profile | SPEC §7.2 |
| `docs/TESTING_v54.md` (v52 deleted) | V169 – V172: TEN-06 `active` (partly), Respawn, untagged tests, inputs | O-S4-84 |
| `docs/ARCHITECTURE_v44.md` (v42 deleted) | C130; §4.3, §5.2, §9.1, §12 step 3 as built | — |
| `CLAUDE.md` | Header, §3 (M3.4 groundwork), §4 (IdentityAccess `Domain/`, host commands), §5 (`roles` commands), legacy role note | — |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-81 – O-S4-86 | — |

SPEC needs no change: R-ADM-03 and TEN-06 already describe what was built, and no acceptance criterion or open question moves. No UX copy key, Figma mapping or `LEGACY_EXCEPTIONS` entry is touched; no new dependency or project.

## 3. Self-review (CLAUDE §6.7)

Findings sorted per CLAUDE §6.8. No correctness or security bug that affects current behaviour remained after the checks.

| # | Finding | Sorting |
|---|---|---|
| R1 | **Security:** the command needs shell access to the host and its secrets; it accepts only the three platform roles, so it cannot grant `Member` or any other role; it never prints secrets and logs the user id, not the address | No change |
| R2 | **Transition:** a JWT issued before the deployment carries `objects:approve` for at most 30 minutes; during that time a moderator gets 403 on approve and the client hides the buttons until the next refresh, which reads the new names | Won't fix: 30 minutes on a legacy path with no real moderators before the pilot |
| R3 | **Correctness:** the down migration leaves `Approver`/`Admin` rows that the up migration created where none existed (named in its comment); it deletes `CatalogEditor` with its grants | Won't fix: rollback only, documented |
| R4 | **Correctness:** `public.identity_case_insensitive` needs the right to create objects in `public`. The compose stacks connect as the image's superuser (`POSTGRES_USER`); a database owner also may (PostgreSQL 15+: `pg_database_owner`) | Open point P3 for other hosting |
| R5 | **N+1 / performance:** `roles list` is one query with two joins over the role tables; grant and revoke are single-user Identity calls | No change |
| R6 | **Edge cases:** display names with surrounding white space or in another Unicode normal form are distinct today; there is no endpoint that writes them yet | Open point P2 (with S50) |
| R7 | **A11y:** no UI change except permission strings | — |
| R8 | **Logging:** grants and revokes are logged at Information; the database sink keeps Warning and above, so they appear only in the container's console log | Open point P4 |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test`: Unit, Architecture, Integration | 571 + 61 + 263 = 895 of 895 pass (two of the new approve tests failed first on test data — a proposal needs a snapshot — fixed in the test) |
| C3 | `has-pending-model-changes` for the legacy and the IdentityAccess context | No changes (OPS-03 tests `MigrationTests`, `IdentityAccessMigrationTests`) |
| C4 | The generated collation SQL run on `postgres:17-alpine` by hand: an unqualified collation in `identity` fails („collation does not exist“), in `public` it works and refuses „ANNA“ next to „Anna“ | As in F6 |
| C5 | The real `roles` host (`dotnet run -- roles …`) on a throwaway database: pending migrations refused with exit 1; after migrating: grant (role name in other case), list, list one, revoke, unknown address (exit 1), `Member` (exit 2) | As expected; the throwaway container was removed |
| C6 | Client: `npm run lint`, `ng test` | Pass; 504 of 504 |
| C7 | `e2e`: `npm run typecheck`, `npm run lint` | Pass |
| C8 | `check-invariants.mjs` (TEN-06 counted with 4 tests), `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Exit 0 each |
| C9 | `reuse lint` | Compliant |
| C10 | E2E | Left to CI: 2.7 GB free on the disk (an E2E build leaves about 2 GB of images); the only E2E change is a comment |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | **Registration outside Development may fail:** `UserService` adds every new user to `Member`, but only the Development rebuild creates that role (F2). On a database without it, `AddToRoleAsync` throws. Whether staging has the row is not known here | Proposed issue: check staging; if missing, add `Member` to a migration like the platform roles, or drop the global role with step 3 (M3.3 makes it an organisation role) |
| P2 | Display-name normalisation (trim, Unicode NFC) before the uniqueness check | With the S50 endpoint (step 6); the factory then normalises or refuses |
| P3 | The collation in `public` assumes the application's database user may create objects there (R4) | Note for TECHNICAL.md when another hosting is set up |
| P4 | Role grants are logged only to the console (R8) | Decide with R-ADM-02 (administration actions logged with user and timestamp) whether platform-role changes need a stored trail |
| P5 | `SPRINT_04.md` has no rows for O-S4-78 – O-S4-80 of S4-24 (they are in its protocol) | Control chat |
| P6 | PRV-04 stays `planned`: nothing maps or returns the link yet | Identity part with step 3, flows with step 6 (TESTING §9) |

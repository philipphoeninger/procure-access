<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-10b — Foreign keys onto the users and the clean-up (part b of the users' move)

> **Sprint:** 06, task S6-10, part b (S6-10b). **Issues:** #387 (TD-72) — closed by this part.
> **Requirements / invariants:** no requirement changes. OPS-03, TEN-01, TEN-02, TEN-05, TEN-11, PRV-07, SEC-01 – SEC-03, SEC-07 – SEC-11 and ARC-01 – ARC-03 stay green; no invariant changes its status. New OPS-03 case for the migration (TESTING V306).
> **Status:** done by Claude Code (2026-11-18) — pull request open. **One EF migration:** IdentityAccess `AddUserForeignKeys`. No legacy migration.
> **Scope:** as agreed in the plan of S6-10a (`S6-10a_users_identityaccess.md`, section 5, point 1): the foreign keys onto `identity."Users"` with the orphan handling and the delete rules, the clean-up of soft-deleted accounts, the order of the Development rebuild, the comments "no foreign key while users are legacy". Routes and answers are unchanged; no client change.
> **Owner decisions (2026-11-18, with the plan of S6-10a):** **3** — also `UserPreferences.UserId` (the fifth key); the migration deletes orphan rows; cascade for tokens, contributor links, memberships and settings, restrict for `Organization.CreatedByUserId` · **4** — a deleted account loses its role, claim, login and token rows and its lockout, once for the existing soft-deleted rows too. No decision was needed in this part.

## 0. Plan (written before the code)

- **Starting point:** `main` at cc3f45d (S6-10a merged, PR #400). The users belong to `IdentityAccessDbContext`; five columns of the module name a user without a key.
- **Migration:** one in IdentityAccess: orphans first, then the soft-deleted accounts, then the keys.
- **Code:** the account deletion, the Development rebuild, the comments.
- **Tests:** an OPS-03 case on a database with orphans; the deletion and the rebuild; fixtures that store rows of a user without its account get the account.
- **Invariants:** none activated.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The five columns are `text`, `Users.Id` is `character varying(200)`; EF aligns a foreign key column with its principal | The migration alters the five columns to `character varying(200)`; every id is a GUID of 36 characters, and the orphans are deleted before |
| F2 | The Development rebuild deleted every user on each start and left tokens, memberships and organisations behind | Orphans exist only in Development databases; the migration deletes them before it adds the keys (decision 3) |
| F3 | With restrict on the creator, the rebuild could no longer delete a user who created an organisation | The rebuild deletes every organisation first |
| F4 | Identity and the deletion endpoint share one scoped context since S6-10a | The role, claim, login and token rows are marked on that context and go in the one save of the soft delete |
| F5 | The Tenants fixture and four test classes stored organisations, memberships, tokens, settings and contributor links for users without an account | 424 integration tests failed on the keys; the tests store the accounts first (TESTING V309) |
| F6 | The part-a protocol was named `S6-10_…`; the prompt for this part expects `S6-10a_…` | Renamed, references in ARCHITECTURE v81 and TESTING v89 updated |

## 2. Changes

1. **Model.** `HasOne<User>().WithMany()` (for the settings `WithOne()`) without navigation in `AccountTokenConfiguration`, `UserContributorLinkConfiguration`, `OrganizationMembershipConfiguration`, `UserPreferencesConfiguration` (cascade) and `OrganizationConfiguration` (restrict, with the reason in a comment).
2. **Migration `20261118023759_AddUserForeignKeys`** (`internal`), in this order:
   - deletes tokens, contributor links, settings without their user, organisations whose creator row is gone (memberships and invitations cascade), then memberships without their user;
   - for soft-deleted accounts deletes the rows of `UserRoles`, `UserClaims`, `UserLogins`, `UserTokens` and resets `LockoutEnd` and `AccessFailedCount`;
   - alters the five columns to `character varying(200)`, creates `IX_Organizations_CreatedByUserId`, adds the five keys.
   - `Down` (generated) drops the keys and the index and widens the columns; deleted rows do not come back.
   - **Order on a database:** the legacy context migrates first, then Platform, then IdentityAccess with `AdoptIdentityTables` before this migration, so `Users` always exists when the keys are added — on a fresh database, on an existing one and on a module-only one.
3. **Account deletion** (`AccountDeletionEndpoints`): removes the account's role, claim, login and token rows and resets the lockout in the save of the soft delete.
4. **Development rebuild** (`SeedUsers.RebuildAsync`): every organisation first, then the users; one new entry in the TEN-02 allow-list with its reason.
5. **Comments** in the five entities and configurations and in `OrganizationInvitation` no longer speak of legacy users.
6. **Tests.** New `Persistence/UserForeignKeysMigrationTests.cs` (1 × `INV:OPS-03`, 1 without tag); `AccountDeletionTests` (member case extended); `DatabaseRebuildGuardTests` (organisations before users); `TestAccounts.WithId`; accounts before the Tenants fixture in eleven classes; accounts for `OrganizationIsolationTests`, `UserPreferencesTests`, `ContributorProfilePersistenceTests` and the token race of `RegistrationTests`.
7. **Documents.** ARCHITECTURE v82 (C168; §5.2, §12 step 3), TESTING v90 (V306 – V309), CODEBASE §2.88, CLAUDE.md (§3: state and the test rule), SPRINT_06 (both parts done), the part-a protocol renamed. `ARCHITECTURE_v80.md` and `TESTING_v88.md` deleted (two newest stay).

Not changed: SPEC, UX copy, client, e2e, the legacy projects, Dockerfile and compose files, TECH_DEBT (next version at the end of the sprint).

## 3. Self-review

Checklist of CLAUDE §6.7; findings sorted per §6.8.

- **fix (done):** R1 — the first run of the suite showed the fixtures without accounts (F5): fixed in the tests, not by weakening a key.
- **won't fix:** R2 — an organisation the migration or the rebuild deletes leaves its criteria lists and share links in the Configurator and Platform contexts. Only Development databases are affected, and a statement across contexts would break the module rule (open point 2). R3 — a contributor profile whose link is deleted as an orphan stays without owner; profiles have no endpoints yet (step 6) and only Development databases can hold such a link.
- **follow-up:** none new.
- **Correctness:** every production write of a row that names a user happens for the signed-in user or a user `UserManager` found, and a soft-deleted row still satisfies the key, so no request path can now fail on a key. The deletion's extra rows go in the same save as the account; a failed save throws as before.
- **Security and tenancy:** no endpoint, policy or filter changes; the one new filter bypass is the rebuild's delete, Development only and allow-listed. The restrict rule keeps a tenant from ending through a user row.
- **Data:** on staging the migration deletes nothing unless a soft-deleted account still holds roles, claims, logins or tokens, which is its purpose. The column change rewrites five small tables once.

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format` whitespace, style, analyzers: green.
- Unit tests: 799 passed. Architecture tests: 65 passed. Integration tests (Testcontainers, PostgreSQL 17): 826 passed.
- `dotnet ef migrations has-pending-model-changes` for `IdentityAccessDbContext`: none.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` (38 packages, unchanged): green. `reuse lint`: compliant.
- Client tests and lint: not run, the client is unchanged. E2E: left to CI (the E2E stack rebuilds on every start, so it exercises the new rebuild order and the seeder there).

## 5. Open points

1. **Local databases:** the first start after this change deletes the orphan rows of earlier rebuilds. Nothing to do by hand; with `docker compose … down -v` the question does not arise.
2. Criteria lists and share links of an organisation that the rebuild or the migration deleted stay behind (Development only, R2). A clean rebuild would ask each module to delete its organisation data, as the account deletion does through `IOrganizationDataDeletion`; proposed as a small follow-up when the Workspace joins that contract (S6-25).
3. Contributor profiles: deleting an account does not yet remove its link (owner decision S5-28 3, step 6); the cascade only applies to a hard-deleted user row.
4. Unchanged from part a: registration and its token in one transaction; the Identity package reference in SERVICES; `Permissions` and `CustomClaimTypes` in MODELS until S6-23; the header of `SPEC_v2.md` names the older ARCHITECTURE and TESTING versions.
5. `TASK_PROMPTS.md` still names `S6-10_users_identityaccess.md` in the S6-10 prompt block; it is the owner's template and was left as it is.

<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-10 — Move the users into IdentityAccess (part a: the move)

> **Sprint:** 06, task S6-10, part a (S6-10a). **Issues:** #387 (TD-72; stays open until part b), TD-75 (done here).
> **Requirements / invariants:** no requirement changes. ARC-01 – ARC-03, OPS-03, SEC-01 – SEC-03, SEC-07 – SEC-11, TEN-01, TEN-02, TEN-05, TEN-11, PRV-07 stay green; no invariant changes its status. New OPS-03 cases for the hand-over (TESTING V302).
> **Status:** part a done by Claude Code (2026-11-18) — pull request open. **Two EF migrations:** legacy context `HandOverIdentityTables`, IdentityAccess `AdoptIdentityTables`. Part b (S6-10b) is not started.
> **Scope of part a:** the `User` entity and the seven ASP.NET Core Identity tables into `IdentityAccessDbContext` with their rows; the Identity registration; the host endpoints of `API/Account`, `API/Auth`, `API/Organizations` and `GET /api/me` as module endpoints; `UserService.GetCurrentUserAsync`; the `roles` command; the E2E seeder and the Development rebuild; the display-name contract for S6-11; TD-75; the JWT comment in `IdentityAccessModule.cs`. Routes and answers are unchanged; no client change.
> **Scope of part b (S6-10b):** the foreign keys onto `Users` with the orphan handling, the role, claim and lockout columns of soft-deleted accounts, the order of the Development rebuild, closing #387.
> **Owner decisions:** O-S6-1 (2026-11-18): yes, at the start of M5; (a) the migration keeps the rows. In this session (2026-11-18), on the plan: **split** confirmed as below · **1 (b)** the other modules implement IdentityAccess's contracts for the account export and deletion · **2 (a)** the legacy hand-over migration drops the two proposal foreign keys · **3** (part b) the fifth key `UserPreferences.UserId`, the migration deletes orphan rows, cascade for tokens, contributor links, memberships and preferences, restrict for `Organization.CreatedByUserId` · **4** the deletion also removes the role, claim, login and token rows and resets the lockout columns, once for existing soft-deleted rows too (part b); the display name of a user is the e-mail address · the package `Microsoft.AspNetCore.Identity.EntityFrameworkCore` in the IdentityAccess project.

## 0. Plan (written before the code)

- **Starting point:** `main` at f5f303e. The Identity tables are mapped by the legacy `ApplicationDBContext`, `User` lives in MODELS, and every account endpoint sits in the host on `UserManager<User>`.
- **Contradiction found:** the row expects "a: tables and keys, b: endpoints and seeder". A module must not reference MODELS and `User` must be `internal` (ARC-03), so the entity and every user of `UserManager<User>` move in one PR. Proposed and confirmed: **a = the whole move**, **b = keys and clean-up**. S6-11 can start after a.
- **Migrations:** legacy hand-over (no table dropped) and a module baseline with `IF NOT EXISTS`, as ADR-014 rule 4 and the Platform hand-over of S2-07.
- **Invariants:** none activated; new OPS-03 cases. **Tests:** the existing ones move with their types; a layout comparison and an existing-database case are new.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | A module project may not reference a legacy project, and outside `Contracts` only the registration is public | The row's a/b split does not compile; split a = move, b = keys (owner) |
| F2 | The account export and deletion call `Configurator.Contracts`, but IdentityAccess may depend on no module (ARCHITECTURE §4.3) | Decision 1 (b): the contracts move to `IdentityAccess.Contracts`, the Configurator implements them |
| F3 | Only `Proposal` relates to `User` in the legacy model (`ProposerId` with cascade, `ApproverId`); `ProposalService` uses the ids only | Decision 2 (a): the two constraints go, the ids and their indexes stay |
| F4 | The legacy context's string convention gave every Identity text column without a length 200 characters | `IdentityTablesConfiguration` repeats it; a test compares both layouts |
| F5 | The platform roles are rows of the legacy data migration `RenamePlatformRoles` | The module baseline inserts them where they are missing, so a module-only database is complete |
| F6 | `ExportedListCriterion` carried `PriorityLevel` of `Catalog.Contracts` | In `IdentityAccess.Contracts` the levels are the names of SPEC §8.2; the Configurator maps them |
| F7 | `GET /api/me` is still read by the client (`auth.service.ts`) | It stays at `/api/me`, mapped by the module on the unversioned group |
| F8 | The legacy rebuild deleted the users the soft-delete filter shows | Kept as it was (no new filter bypass); the order of the rebuild is part b |
| F9 | After the move the host project holds no unversioned minimal API, and the Asp.Versioning analyzer reports AV0016 for `AssumeDefaultVersionWhenUnspecified`, which fails `-warnaserror` and `dotnet format analyzers` | The option is still needed (legacy controllers without the version segment, `GET /api/me`): suppressed at the line with its reason |

## 2. Changes

1. **Entity and tables.** `Domain/User.cs`; `IdentityAccessDbContext` derives from `IdentityDbContext<User>`; `Persistence/Configurations/IdentityTablesConfiguration.cs` (table names, 200-character columns, soft-delete filter). Package reference `Microsoft.AspNetCore.Identity.EntityFrameworkCore` in the module, removed from DAL and MODELS (`THIRD-PARTY-NOTICES.txt` unchanged).
2. **Migrations.**
   - Legacy `20261118015734_HandOverIdentityTables`: the scaffolded seven `DropTable` calls deleted; `Up` drops `FK_Proposals_Users_ApproverId` and `FK_Proposals_Users_ProposerId`; `Down` adds them again and creates no table.
   - IdentityAccess `20261118015819_AdoptIdentityTables` (`internal`): `CREATE TABLE IF NOT EXISTS` for the seven tables and `CREATE [UNIQUE] INDEX IF NOT EXISTS` for the seven indexes, copied from the generated statements; the platform roles and the reviewers' permission where missing; `Down` empty.
   - **Order.** Startup migrates the legacy context first, then Platform, then IdentityAccess (ADR-014 rule 3, unchanged). *Fresh database:* the legacy chain creates the tables, the module migration only records itself. *Existing database:* tables and rows untouched, the module migration is a no-op apart from its history row. *Module-only database:* the baseline creates the tables and the roles.
3. **Legacy projects.** `ApplicationDBContext` is a plain `DbContext`; `Proposal` loses its two navigations, `ProposalConfiguration` keeps the two indexes; removed: `User`, `UserConfiguration`, `UserDto`, `Roles`, the user map, `SampleData`, `SampleDataInitializer`, `UserService`, `IUserService`.
4. **Module.** `Auth/` (`IdentityRegistration`, sign-in and sign-out, registration, password reset with its mailer, the confirmation gate, password and lockout policy, `ClockedUserManager`, `ClientLinks`), `Accounts/` (account, export, deletion, `LegacyAccountEndpoint`, `UserDirectory`, `SeedUsers`), `Organizations/` (organisation and invitation endpoints), `Commands/RoleCommandLine`. The moved files changed namespace, visibility (`internal`) and usings; their logic is unchanged except: the deletion calls every `IOrganizationDataDeletion`, the export takes the level names as they come, the `roles` command checks the module's pending migrations. `IdentityAccessModule`: one `MapIdentityAccessEndpoints`, `MapIdentityAccessLegacyEndpoints`, `IsRolesCommand` / `RunRolesCommandAsync`, `RebuildSampleUsersAsync`, `SeedTestUsersAsync` (replaces `SeedTestOrganizationAsync` and `IssueAntiforgeryToken`); the JWT comment is corrected.
5. **Contracts.** `IdentityAccess.Contracts/Accounts`: `ICriteriaListDataExport` with its records and `IOrganizationDataDeletion` (both from `Configurator.Contracts/Accounts`, which is gone), `PlatformRoles` (from MODELS `Roles`), new `IUserDirectory`. The Configurator's two services implement the moved contracts.
6. **Host.** `Program.cs` (one mapping call, the Identity registration and the mailer gone), `IdentityExtensions` (scheme, policy and middleware order only), `AuthorizationExtensions` (`ModerationReview` only), `RoleCommandHost`, `TestUserSeedExtensions` and `DatabaseRebuildExtensions` (guards and test data stay, the work is the module's), `ApiVersionConfiguration` (F9).
7. **Tests.** New: `Persistence/IdentityTablesHandOverTests.cs` (2 × `INV:OPS-03`), `IdentityAccess/UserDirectoryTests.cs`. Adapted: users through the module's context and types; `PlatformRoleMigrationTests` on both contexts; `Infrastructure/TestAccounts.cs` replaces `LegacyAccounts` and its three copies (TD-75); the ARC-06 case of the removed user map is gone.
8. **Documents.** ARCHITECTURE v81 (C167; §4.3, §5.2, §10.5, §12 step 3), TESTING v89 (V302 – V305), CODEBASE §2.86 and §3, CLAUDE.md (§3 state and module rule, §4 layout), SPRINT_06 (the split in the row). `ARCHITECTURE_v79.md` and `TESTING_v87.md` deleted (two newest stay).

Not changed: SPEC (no route, answer or limit changes), UX copy, client, e2e, `Dockerfile` (no new project), compose files, TECH_DEBT (next version at the end of the sprint), Figma mapping, `LEGACY_EXCEPTIONS`.

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` on the diff; findings sorted per §6.8.

- **fix (done):** R1 — the first version of `SeedUsers.RebuildAsync` read past the soft-delete filter, a new filter bypass and a change of the rebuild's behaviour: removed (F8). R2 — AV0016 after the move (F9).
- **won't fix:** R3 — Identity and the module's services now share one scoped context, so a failed Identity update would leave its entity modified in the tracker for a later save of the same request. Checked on every write path (registration, confirmation, password reset, sign-in, deletion): each failed result ends the request with an exception or a return, and the services that open a transaction clear the tracker first. No path saves after a failed result. R4 — the moved files keep their own request and answer records and helper names; no renaming in a move.
- **follow-up:** R5 — registration creates the user and its token in two saves, as before; one transaction is possible now (open point 3). R6 — the SERVICES project still references the Identity package it no longer needs (open point 4).
- **Security:** authorisation, antiforgery, rate-limit and confirmation attributes moved with their endpoints; the antiforgery exemption list, the authorisation matrix and the `no-store` default are unchanged and green. No token or address is logged anew. `IUserDirectory` answers addresses: the contract says that the caller decides who may see them.
- **Queries:** no new query in a loop; `UserDirectory` is one query; the members read stays one query.
- **Migrations:** neither changes a row of `Users`; the layout comparison covers columns, indexes and constraints.

## 4. Checks

- `dotnet build ProcureAccess.sln --no-incremental -warnaserror …`: 0 warnings, 0 errors.
- `dotnet format` whitespace, style, analyzers `--verify-no-changes`: green.
- Unit tests: 799 passed. Architecture tests: 65 passed (ARC-01 – ARC-03, TEN-02 allow-list unchanged).
- Integration tests (Testcontainers, PostgreSQL 17) on the final code: 823 passed (821 before, plus the two `UserDirectoryTests`; the two hand-over cases are among them).
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` (38 packages, unchanged): green. `reuse lint`: compliant.
- Client tests and lint: not run, the client is unchanged. E2E: left to CI (no local stack in this session; WebKit runs in CI only).

## 5. Open points

1. **S6-10b:** the five foreign keys with the orphan handling and the delete rules (decision 3), the clean-up of soft-deleted accounts (decision 4), the Development rebuild deleting organisations before users, the comments "no foreign key while users are legacy" in the module, closing #387.
2. **Development rebuild and proposals:** without the cascade a proposal of a deleted user stays after the rebuild (Development only). It ends with the proposal tables in S6-23.
3. Registration and its confirmation token could share one transaction now that both are in one context (R5). Behaviour is unchanged; proposed for S6-05 (token hygiene) or as an issue.
4. `SERVICES.ProcureAccess.csproj` still references `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, and its global usings name Identity namespaces (R6). Remove with S6-23 or the next change of that project.
5. `Permissions` and `CustomClaimTypes` stay in MODELS, because the legacy proposal service reads them; they move to IdentityAccess's contracts when the proposals retire (S6-23).
6. The header of `SPEC_v2.md` names `ARCHITECTURE_v80.md` and `TESTING_v88.md` as the versions of v2.48. SPEC did not change in this task, so the line is left; the next SPEC change row updates it.
7. `AssumeDefaultVersionWhenUnspecified` and its AV0016 suppression go with the last legacy controller.
8. Staging: nothing to do by hand for this task. The first start after the deployment applies both migrations; TD-28's list is unchanged.

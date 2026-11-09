# S3-24 — Persist the Data Protection keys outside the container

> **Sprint:** 03, stretch task S3-24 (TECH_DEBT_v2 TD-21, 1 point). **Issues:** #186. **Requirements / invariants:** ARCHITECTURE C60 and §9.1 (key ring in the database); prepares OPS-04 (step 3), which stays `planned`; OPS-03 tests added for the new context; no invariant activated, so no new TESTING version. **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: the Data Protection key ring moves from the container's file system into the database, with the module's context and migration. **Not in scope:** the cookie session, antiforgery and the OPS-04 test (step 3); encrypting the keys at rest (owner decision 3a); `TECHNICAL.md` (G-P, #4).
> **Owner decisions** (2026-11-09, asked before the task, all as recommended):
> 1. **1a — IdentityAccess owns the key store**, as ARCHITECTURE §4.3 and §9.1 say (table `DataProtectionKeys` in schema `identity`). The task prompt's additional instruction „in the Platform module“ contradicted both; the decision replaces it.
> 2. **2 — new package** `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (MIT, Microsoft; shipped in the API image, allowed by CLAUDE §10 / D12). Its dependencies (`Microsoft.AspNetCore.DataProtection`, `Microsoft.EntityFrameworkCore` 10.0.14) are already in the solution.
> 3. **3a — no encryption at rest.** The key XML is stored as written; the database and its backups must be protected instead. Revisit with `TECHNICAL.md` before the pilot.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No `AddDataProtection` configuration anywhere: ASP.NET Core used its default file-system key ring, in the container `/home/app/.aspnet/DataProtection-Keys` (S2-21 P3), lost with every new container | #186, TD-21 |
| F2 | The legacy Identity setup uses `AddDefaultTokenProviders`; `GenerateEmailConfirmationTokenAsync`, `GeneratePasswordResetTokenAsync` and `GenerateChangeEmailTokenAsync` are called, and these tokens are protected with Data Protection. No antiforgery runs in the API yet | Effect today: e-mail confirmation, password reset and e-mail change links broke on every redeploy; this change fixes that as a side effect |
| F3 | `IdentityAccess` was an empty module (`AddIdentityAccessModule()` without configuration) | First context of the module, same pattern as Catalog (ADR-014): `internal` context, own schema and history table, migrator, design-time factory |
| F4 | The task prompt said „Platform“, ARCHITECTURE §4.3 / §9.1 „IdentityAccess, schema `identity`“ | Asked: owner decision 1a |
| F5 | The default Data Protection application name is the content root path | A local `dotnet run` and the container would use different key rings on the same database; fixed name `ProcureAccess` |
| F6 | The `catalog …` command host registers only Platform and Catalog | Unchanged: it neither migrates nor uses Data Protection |

## 2. Changes

| Area | Change |
|---|---|
| `src/ProcureAccess.Modules.IdentityAccess/*.csproj` | Packages `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` 10.0.14 (decision 2), `Microsoft.EntityFrameworkCore` 10.0.14 and `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 (same versions as the other modules); `InternalsVisibleTo` for the three test projects |
| `Persistence/IdentityAccessDbContext.cs` (new) | `internal`, implements `IDataProtectionKeyContext`; schema `identity`, history `identity."__EFMigrationsHistory"` (set in `OnConfiguring`, as in the other module contexts) |
| `Persistence/Configurations/DataProtectionKeyConfiguration.cs` (new) | Table `DataProtectionKeys`; the library's entity shape unchanged (Fluent API only) |
| `Persistence/IdentityAccessDatabaseMigrator.cs`, `IdentityAccessDbContextFactory.cs` (new) | Migrator for startup; design-time factory for `dotnet ef` |
| `Persistence/Migrations/…_InitialIdentityAccessSchema` (new) | Creates schema `identity` and the table; class made `internal` (ARC-03) |
| `IdentityAccessModule.cs` | `AddIdentityAccessModule(configuration)`: context with `EnableRetryOnFailure().CommandTimeout(60)` like the others, the migrator, and `AddDataProtection().SetApplicationName("ProcureAccess").PersistKeysToDbContext<IdentityAccessDbContext>()` |
| `API/…/Program.cs` | Passes the configuration; the module's migrator runs after Platform and before Catalog (registration order) |
| `tests/…Integration/Infrastructure/ApiFixture.cs` | Migrates the new context; Respawn keeps `identity."DataProtectionKeys"` (and the new history table), because the shared API host caches the key ring and deleting rows under it would only make cache and database disagree |
| `tests/…Integration/Persistence/IdentityAccessMigrationTests.cs` (new) | OPS-03 for the context: applies after legacy and Platform with its own history; model matches the last migration; startup applies it |
| `tests/…Integration/Operations/DataProtectionKeyStoreTests.cs` (new, no invariant tag) | A host protects a value and stops; one key row is in the database; a second host on the same database unprotects it. A host on another database cannot (`CryptographicException`) — this is what shows the key comes from the database, because hosts of one test run otherwise share `~/.aspnet/DataProtection-Keys` |
| `docs/ARCHITECTURE_v27.md` | C112; §4.3 IdentityAccess „as built“; §9.1 key store as built (application name, unencrypted); §12 step 3 notes the store is built ahead. v25 deleted (two newest kept) |
| `CLAUDE.md` | Version line; §3 key store; §4 module folder; §5 `dotnet ef` line for the context |
| Dockerfile | Unchanged: its restore layer already copies the IdentityAccess project file (S2-06) |

**How OPS-04 will be tested (step 3).** With the cookie session in place: create a database of its own (`ConnectionStringForNewDatabase`), start host 1 (`ProcureAccessApiFactory` without the test authentication scheme, or with the real cookie scheme added), sign in through the real endpoint and keep the `Set-Cookie`; dispose host 1; start host 2 on the same database and call an authenticated endpoint with that cookie → 200 (not 401). The fake clock must stay inside the idle limit between the two hosts. `DataProtectionKeyStoreTests` already proves the mechanism underneath; OPS-04 adds the cookie and its tag.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff. N+1 and a11y: not applicable (Data Protection reads the key ring once and caches it, refreshing about every 24 h; no UI). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The keys are readable for anyone with read access to the database or a `pg_dump`; ASP.NET Core logs „No XML encryptor configured“ when it creates a key | Won't fix: owner decision 3a; recorded in ARCHITECTURE §9.1 and C112 |
| R2 | Keys of the old file-system ring are not imported: an e-mail confirmation or password-reset link sent before the deployment fails once | Won't fix: links are short-lived (legacy defaults: 1 day), the project has no production users yet |
| R3 | Expired keys stay in the table (Data Protection never deletes them; one row about every 90 days) | Won't fix: negligible size; old keys are still needed to read old payloads |
| R4 | Two instances starting on an empty table could each create a key | Won't fix: both keys end up in the ring at the next refresh; single API instance assumed as for migrations and log retention (CLAUDE §5) |
| R5 | Startup order: Data Protection's hosted service loads the key ring when the host starts, i.e. after `Program.cs` has migrated every context | – checked: in the E2E stack the key was created right after the migrations and stored in `identity."DataProtectionKeys"` |
| R6 | Mutation check: without `PersistKeysToDbContext` both `DataProtectionKeyStoreTests` fail (no row; the other-database host reads the payload through the shared file-system ring) | – |
| R7 | Security: no endpoint, no user input, no secret in code; the connection string comes from configuration as in the other modules | – |

No correctness or security finding left that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ exit 0 |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ 639 tests, 0 failed (OPS-03, ARC-01 – ARC-03 included) |
| 4 | E2E stack (`npm run stack:up`, image rebuilt) | ✅ API healthy; `identity."__EFMigrationsHistory"` holds `InitialIdentityAccessSchema`, one key row; the expected „No XML encryptor configured“ warning; the EF „Failed executing DbCommand … __EFMigrationsHistory“ errors on an empty database are the known probe (S1-06 K4), now one more for `identity` |
| 5 | `npx playwright test --project=chromium` | ✅ 47 passed (other browser projects in CI) |
| 6 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v37.md`) |
| 7 | `node scripts/check-figma-links.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending) |
| 8 | `node scripts/check-i18n.mjs` | ✅ 0 errors (no UI text touched) |
| 9 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant (migrations covered by `REUSE.toml`) |
| 10 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | OPS-04: write the test described in §2 with the cookie session and flip it to `active` | Migration step 3 |
| P2 | Encryption at rest of the key ring (certificate or another `IXmlEncryptor`) and the documentation of the key store, its backup and restore (ARCHITECTURE §9.1, §10.4) | `TECHNICAL.md` before the pilot (#4) |
| P3 | `TECH_DEBT_v3` (sprint close-out, step 14): TD-21 / #186 resolved | Sprint 03 close-out |
| P4 | `TESTING_v37` and `SPEC_v2` still name `ARCHITECTURE_v25.md` as their input (also after S3-23); no TESTING version was written because no invariant or test rule changed | Next TESTING / SPEC version |
| P5 | Step 3 adds the Identity entities to `IdentityAccessDbContext`; moving `AspNetUsers` etc. out of the legacy context follows ADR-014 rule 4 | Migration step 3 |

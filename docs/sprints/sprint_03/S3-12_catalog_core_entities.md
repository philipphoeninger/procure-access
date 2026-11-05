# S3-12 — Catalog core entities and first migration

> **Sprint:** 03, P0 task S3-12 (M2.1, 3 points). **Issues:** none. **Requirements / invariants:** CAT-05 → `active` (unit part, plus a first integration test) in `TESTING_v30.md` §4.2 (V92); OPS-03 (`active`) covers the new context; ARCHITECTURE_v18 §4.3, §5.1, §10.5 (ADR-014), §12 step 2; SPEC_v2 R-ADM-01, SD20 (no change). CAT-10, ARC-01 – ARC-03 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: `CatalogRelease`, `Criterion` (with `Key`, `Kind`), `CriterionRevision`, `CatalogReleaseCriterion`, `StandardReference` in the Catalog module with `CatalogDbContext` (schema `catalog`, own migration history) and its first migration; a published release and its revisions cannot be changed (domain error). Also, from O-S3-5 (e): ARCHITECTURE §12 step 2, S1-15 decision 1A and the CAT-10 fixture description reworded. Not in scope: filter dimensions and rules, the D1 seed and its Respawn entries (S3-13), builders and MiniCatalog (S3-14), release format, import and activation (S3-15 – S3-17).
> **Owner decisions:** O-S3-5 (2026-11-06; S3-11 §2.9): **C** no copy of the existing criteria, **D1** filter values seeded in the catalog migration (S3-13), **Q2 (iii)**, proposals kept, documents reworded, `Catalog.Contracts` as in S3-11 §2.2. Before the task (2026-11-06): **1** a release counts as published as soon as it is stored — `catalog import` is the only way in, `PublishedAt` is the import time, `IsActive` is the only field that still changes, through activate/deactivate; every other change fails with a domain error. **2** a `StandardReference` linked to a revision of a published release is locked. **3** confirmed: `Criterion.TestTypeId`, the D1 seed and its Respawn entries come with S3-13. **4** confirmed: immutability in the entities only; an interceptor is decided in S3-16 with CAT-05's import part. **P8** (S3-11): extend the BIT inklusiv block of `REUSE.toml` to the catalog migrations — done with S3-13, the migration that carries the quoted texts (§5 P1).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `ProcureAccess.Modules.Catalog` held only `AddCatalogModule()` without parameters; `Catalog.Contracts` was empty. The project files and their Dockerfile `COPY` lines existed since S2-06 | No new project; the Dockerfile needs no change |
| F2 | Platform is the template of ADR-014: `internal` context with the history table set in `OnConfiguring`, `IModuleDatabaseMigrator`, a design-time factory that needs no secret, `PlatformMigrationTests` for OPS-03 | Same shape for Catalog; nothing moves from the legacy context (O-S3-5 C), so plain `CreateTable` (no rule-4 baseline) |
| F3 | ARCHITECTURE §5 says `BaseEntity` (`Id`, `CreatedAt`, `xmin`) applies throughout; the legacy `BaseEntity` sets `CreatedAt` by a database default (`CURRENT_TIMESTAMP`), which `FakeTimeProvider` cannot control | Catalog entities get `Id`, `CreatedAt` and `xmin`; `CatalogDbContext` sets `CreatedAt` from `TimeProvider` (CLAUDE §7) |
| F4 | CAT-05 needs a rule for what „published“ means. CAT-23 makes an imported release immutable and `catalog activate` (S3-17) changes `IsActive`; no document has a draft state | Owner decision 1: stored = published; `PublishedAt` `NOT NULL` |
| F5 | Release 2026.1 is imported before the plain-language titles exist (O-1, K-1; S3-27, Q2 (iii)); §5.1 does not say which revision fields are required | `TitleDe` required; `PlainLanguageDe`, `RationaleDe` and `Criterion.Category` optional (ARCHITECTURE_v18 C101) |
| F6 | SPEC §8.2 returns a `number` (EN 301 549 / BIT inklusiv) per criterion; §5.1 has no such field on `CriterionRevision` | Not decided here (§5 P3) |
| F7 | The integration fixture migrates every context once and Respawn ignores the `public` and `platform` histories (S3-11 F10, P6) | Fixture migrates `CatalogDbContext`; Respawn also ignores `catalog."__EFMigrationsHistory"` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog.Contracts/CriterionKind.cs` (new) | `public enum CriterionKind { Requirement, TestProcedure }` | S3-11 §2.2 (f): other modules will need it |
| `src/ProcureAccess.Modules.Catalog/Domain/` (new) | `CatalogEntity` (base: `Id`, `CreatedAt`), `CatalogRelease`, `CatalogReleaseCriterion`, `Criterion`, `CriterionRevision`, `StandardReference`, `Standard` (enum), `PublishedCatalogChangeException`; all `internal`, no public setters, created by static `Create` methods | ARCHITECTURE §5.1; ARC-03 |
| — `CatalogRelease` | `Create(version, notes)` → draft; `Include(revision)` (one revision per criterion key; an unchanged revision of an earlier release can be shared, CAT-07); `Publish(at)` sets `PublishedAt` and seals every included revision; `Activate`/`Deactivate` only on a published release. After `Publish`, `Include` and `Publish` throw `PublishedCatalogChangeException` | Decision 1, CAT-05 |
| — `CriterionRevision` | Draft only when built with `Create`; the flag is not mapped, so every instance EF creates is sealed. `AddStandardReference` throws on a sealed revision. Fields of §5.1; `Criterion` navigation required | Decision 1 (stored = published), CAT-05 |
| — `StandardReference` | `Standard`, `Clause`, `Url`; no method that changes it; unique per standard and clause | Decision 2 |
| — `Criterion` | `Key`, `Kind`, `Category?`, `SupersededByKey?`; no method that changes it. `TestTypeId` with S3-13 | Decision 3; key and kind never change (ADR-002) |
| `src/ProcureAccess.Modules.Catalog/Persistence/` (new) | `CatalogDbContext` (schema `catalog`, history `catalog."__EFMigrationsHistory"` in `OnConfiguring`, `CreatedAt` of added rows from `TimeProvider`), `Configurations/` (Fluent API only; `xmin` as in the legacy `BaseEntityConfiguration`; enums as strings; unique `Version`, `Key`, (`CriterionId`, `RevisionNumber`), (`Standard`, `Clause`); `PublishedAt` `NOT NULL`; every foreign key `Restrict`), `CatalogDatabaseMigrator`, `CatalogDbContextFactory` | ADR-014 rules 1–3, 5 |
| `…/Persistence/Migrations/*_InitialCatalogSchema*` (generated) | Six tables: `CatalogReleases`, `CatalogReleaseCriteria`, `Criteria`, `CriterionRevisions`, `StandardReferences`, `CriterionRevisionStandardReferences`; classes made `internal` | ADR-014 rule 2; ARC-03 |
| `src/ProcureAccess.Modules.Catalog/CatalogModule.cs`, `.csproj` | `AddCatalogModule(configuration)` registers the context (same connection string, retry and timeout as Platform) and the migrator; packages `Microsoft.EntityFrameworkCore` 10.0.14 and `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.3 (the versions Platform uses — no new dependency); `InternalsVisibleTo` the three test projects | ADR-014 rule 3 |
| `API/API.ProcureAccess/Program.cs` | `.AddCatalogModule(builder.Configuration)`; comment | Registration after Platform (rule 3) |
| `tests/ProcureAccess.Tests.Unit/Catalog/CatalogReleaseImmutabilityTests.cs` (new), `.csproj` | 9 tests, 5 tagged `INV:CAT-05`: `Include`, `Publish` and `AddStandardReference` after publishing throw; a shared revision stays sealed in the next release; `Activate`/`Deactivate` change only `IsActive`; drafts change until published; one revision per criterion; activating a draft and citing a reference twice throw | CAT-05 unit part |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogPersistenceTests.cs` (new) | Round trip of a published release with both kinds of criteria and a reference (`CreatedAt` = fake clock); `INV:CAT-05`: a loaded release and its revisions are sealed and only the activation is saved; an unpublished release is rejected by the database | Decision 1; EF mapping exercised (TESTING §6.1) |
| `tests/ProcureAccess.Tests.Integration/Persistence/CatalogMigrationTests.cs` (new) | `INV:OPS-03`: an empty database after legacy and Platform, and on its own; own history; no pending model changes; startup applies the migrations | ADR-014 rule 6 |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/ApiFixture.cs` | Migrates `CatalogDbContext`; Respawn ignores its history; `StoreAsync` routes entities to the module context that maps them | F7; S3-11 P6 |
| `docs/TESTING_v30.md` (new) | V92: CAT-05 `active` (unit part; import part with S3-16), OPS-03 covers `CatalogDbContext`; input document `ARCHITECTURE_v18.md` | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v18.md` (new) | C101 Catalog core as built and „published“ (§4.3, §5.1, §10.5); C102 §12 step 2 reworded after O-S3-5; C103 the seed exclusions 17 → 12 and 17 → 13 named by their entries (S3-11 P7) | O-S3-5 (e); decisions 1, 2 |
| `docs/sprints/sprint_01/S1-15_cat10_characterisation.md` | Dated note under decision 1A; the protocol itself stays | O-S3-5 (e) |
| `tests/…/Catalog/Fixtures/cat-10-criteria-filter.json` | Description: the sets stay true while the legacy filter reads the legacy tables, until the CAT-10 rewrite (M2.5d); descriptions left out because they are BIT inklusiv texts. Expected sets unchanged | O-S3-5 (e); S3-11 F1, F6 |
| `CLAUDE.md` | Header; §3 Catalog core; §4 module folder; §5 catalog migration commands | CLAUDE §11 |

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | No endpoint, no input from outside; connection string only from configuration; design-time factory without secret |
| 2 | Correctness — immutability | Every mutating member of the five entities is guarded or absent; loaded entities are sealed (integration test). Not covered by the domain: changes through the EF change tracker (`Entry(…).Property(…).CurrentValue`) and `Remove` — open point P2 for S3-16 (decision 4) |
| 3 | Correctness — ADR-014 | Own history in `catalog`; no foreign key or query to another schema; migrator registered after Platform; ARC-01 – ARC-03 green (only `CatalogModule` public; `CriterionKind` in `Contracts`) |
| 4 | N+1 / queries | No queries in production code yet |
| 5 | Edge cases | Empty key, version, title, clause; unknown enum values; revision number < 1; the same criterion or reference twice; activation of a draft |
| 6 | Time | `CreatedAt` from `TimeProvider`; `PublishedAt` is passed in by the caller (the import) |
| 7 | A11y, UI strings | No UI change |
| 8 | Independent review (agent) sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

The review agent (code-reviewer, read-only, against CLAUDE.md, ADR-014 and the owner decisions) found no correctness or security bug: no sequence of public calls changes a published release or a sealed revision; EF uses the private parameterless constructors, so loaded revisions are sealed; the backing-field navigations and the m:n join match migration and snapshot; only `CatalogModule` is public outside `Contracts`. Its five remarks, sorted per CLAUDE §6.8:

| # | Remark | Sorting |
|---|---|---|
| R1 | `Criteria` and `StandardReferences` return the backing `List<>` as `IReadOnlyCollection<>`; code inside the module could cast it back | **Won't fix:** the types are `internal`, and a cast-back is no API anyone uses; `AsReadOnly()` adds an allocation per access for a theoretical case |
| R2 | `Include` reads `Criterion.Key` of the entries already in the release, which throws if their criterion is not loaded | **Won't fix:** every entry came through `Include`, which rejects a revision without its criterion; a loaded release is published, so `Include` throws before the comparison |
| R3 | `Publish` accepts an empty release and any `publishedAt` | **Follow-up (P7):** the import validates the release folder (S3-15) and sets the time (S3-16) |
| R4 | Nothing enforces a single active release | **Follow-up:** already P4 (S3-17) |
| R5 | Immutability only in the entities; a never-published release fails only at the database | **Won't fix here:** decision 4; P2 (S3-16); covered by `Store_UnpublishedRelease_IsRejectedByTheDatabase` |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format whitespace / style / analyzers --verify-no-changes` | ✅ all three |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ 200 passed |
| 4 | `node scripts/check-invariants.mjs` | ✅ gate passed; CAT-05 `U, I` 7 tests, OPS-03 8 tests |
| 5 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 6 | `reuse lint` (Docker image) | ✅ compliant, 715 / 715 files |
| 7 | API Docker image build (`API/API.ProcureAccess/Dockerfile`) | ✅ built; restore layer with the two packages of the Catalog project |
| 8 | E2E stack (`npm run stack:up`, fresh database), `npx playwright test --project=chromium` (WebKit only in CI) | ✅ 47 passed, J1 (CAT-10) included; the API log shows the migration into `catalog."__EFMigrationsHistory"` at startup |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | P8 of S3-11 decided (extend the BIT inklusiv block of `REUSE.toml` to the catalog migrations): S3-13 adds the D1 seed migration with the product-part explanations and names it, its `Designer.cs` and the catalog snapshot in that block, as the legacy block does for `AddSeedData` | S3-13 |
| P2 | CAT-05 import part: the domain rule does not stop changes through the EF change tracker or a `Remove`; whether `catalog import` gets a `SaveChangesInterceptor` that rejects modified or deleted catalog rows (except `IsActive`) is decided with S3-16 (decision 4) | S3-16 |
| P3 | SPEC §8.2 returns `number` per criterion; §5.1 has no field for it (from the key, a `StandardReference`, or a new revision field) | S3-15 (folder format) or S3-20 |
| P4 | One active release at a time: `Activate` does not deactivate the others; a partial unique index on `IsActive` and the switch belong to `catalog activate` | S3-17 |
| P5 | `Criterion.Category` has no definition in the documents; mapped as optional text (≤ 128) | S3-15, with the folder format |
| P7 | `Publish` checks neither that the release has criteria nor the time it gets (review R3); the folder validation and the import set both | S3-15, S3-16 |
| P6 | A draft revision stored on its own (not through a published release) is sealed when loaded; only the import creates revisions, so it is a rule for S3-16 | S3-16 |

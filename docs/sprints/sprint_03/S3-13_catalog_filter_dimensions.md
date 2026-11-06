# S3-13 — Catalog filter dimensions, applicability, priorities and procedure texts

> **Sprint:** 03, P0 task S3-13 (M2.1, 4 points). **Issues:** none. **Requirements / invariants:** CAT-13 → `active` in `TESTING_v31.md` §4.2 (V93); CAT-05 (`active`) extended to the new rows; OPS-03 (`active`) covers the new migration; ARCHITECTURE_v19 §4.3, §5.1 (C104); SPEC_v2 §8.1, R-ADM-01, SD20 (no change). CAT-10, ARC-01 – ARC-03 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: `AppType`, `ProductPart` (`IsAlwaysSelected`), `UseCaseTemplate` (`IsActive`), `TestType`, `FilterExclusion`, `CatalogReleaseFilterExclusion`, `CatalogReleaseAppTypeProcedure`, `CriterionApplicability`, `PriorityRecommendation`, `CriterionProcedureText` with a migration; a `FilterExclusion` whose target layer is not lower than its source layer cannot be created (CAT-13). Also, from S3-12: `Criterion.TestTypeId`, the D1 seed and its Respawn entries (S3-12 decision 3), the REUSE annotation of the catalog migrations (S3-11 P8, S3-12 P1). Not in scope: priority resolution and selection validation (S3-20), builders and MiniCatalog (S3-14), release format, import and activation (S3-15 – S3-17).
> **Owner decisions:** O-S3-5 (2026-11-06): **D1** the four filter-value tables are seeded in the catalog migration (`HasData`, values from Kriterienkatalog §2), Respawn ignores them, release folders refer to them by key; `Catalog.Contracts` as in S3-11 §2.2. Before the task (2026-11-06, on the questions of this session): **keys as proposed** (§2, C104); **`AppType.IsInScope` dropped** (no document defined it); **defaults fine**: `UseCaseTemplate.Description` optional and empty in the seed, names and order as in Kriterienkatalog §2, CAT-13 in the domain and as a CHECK constraint, exclusions and procedure mapping not seeded (release folder, S3-15), the new rows locked like S3-12, `PriorityRecommendation.Weight` an optional integer. P8 of S3-11 was decided earlier (extend the BIT inklusiv block of `REUSE.toml`).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S3-12 left `Criterion.TestTypeId`, the filter values and their Respawn entries to this task; `CatalogRelease` and `CriterionRevision` already mention „the rows of S3-13 follow the same rule“ | Same pattern: rows are added through the release or revision before `Publish`, never changed after |
| F2 | The prompt names `ARCHITECTURE_v17` and `TESTING_v29`; the highest versions are v18 and v30 (CLAUDE §2) | Worked from v18/v30; this task writes v19/v31 |
| F3 | ARCHITECTURE §5.1 lists `AppType.IsInScope` without a meaning; no document, no legacy code defines it | Owner: drop it (C104) |
| F4 | `UseCaseTemplate.Description` has no text anywhere (legacy seed: empty strings; Kriterienkatalog §2.1: none) | Optional and empty; no wording invented (CLAUDE §6.5) |
| F5 | SPEC §8.1 shows only example keys (`hr`, `web`, `document`, `base`, `pdfTest` …); release folders (S3-15) and the API need all of them | Owner: the key list of C104 |
| F6 | The legacy seed spells „Interne Kommunikations- und Datenaustausch-Plattform“ and „Website / Webanwendung“; PRD and Kriterienkatalog use „…Datenaustauschplattform“ and the display form without slash (UX copy U17) | Seed uses the Kriterienkatalog display names |
| F7 | §5.1 gives `UseCaseTemplate` no `SortOrder` (the UI shows a searchable list) | Not added; open point P3 |
| F8 | The product-part explanations are quoted from the Standardanforderungskatalog; EF repeats seed data in the migration, its designer and the snapshot | Header of `FilterValueSeed.cs` and the BIT inklusiv block of `REUSE.toml` (S3-11 P8) |
| F9 | Seeded rows with an identity column would leave the sequence behind the fixed ids | The four tables generate no ids (`ValueGeneratedNever`): only migrations write them |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog.Contracts/PriorityLevel.cs`, `Rating.cs` (new) | `PriorityLevel` (A, B, Info); `[Flags] Rating` with the six BIT inklusiv levels and `All` (no `Unknown`: always allowed, SPEC §8.2) | S3-11 §2.2: Configurator, Export, Workspace, Directory need them |
| `src/ProcureAccess.Modules.Catalog/Domain/FilterValue.cs`, `FilterLayer.cs` (new) | Unmapped base (`Key`, `NameDe`, `Layer`) of the four value types; layers 1–4 in their fixed order | CAT-13 compares layers |
| `…/Domain/UseCaseTemplate.cs`, `AppType.cs` (+ `AppTypeFunctionality.cs`), `ProductPart.cs`, `TestType.cs` (new) | Fields of §5.1 (without `IsInScope`); static `Create`, no method that changes them | §5.1; C104 |
| `…/Domain/FilterExclusion.cs` (new) | Typed source (use case, app type, product part) and target (app type, product part, test type), `ReasonDe`; `Create` throws `ArgumentException` unless the target layer is lower; `Source`/`Target` helpers; no mutators | CAT-13; v7 Y5 |
| `…/Domain/CatalogReleaseFilterExclusion.cs`, `CatalogReleaseAppTypeProcedure.cs` (new); `CatalogRelease.cs` | `AddExclusion` (shared rules, CAT-19) and `MapProcedure` (several procedures per app type, CAT-21), both refused after `Publish`; duplicates refused | §5.1; CAT-05 |
| `…/Domain/CriterionApplicability.cs`, `PriorityRecommendation.cs` (+ `PrioritySource.cs`), `CriterionProcedureText.cs` (+ `ProcedureTextContent`) (new); `CriterionRevision.cs` | `AddApplicability`, `AddPriority`, `AddProcedureText`, refused on a sealed revision; one row per key pair; rationale required (CAT-02); allowed ratings non-empty and only the six levels; a procedure criterion gets no applicability and no procedure text (C30) | §5.1; CAT-05 |
| `…/Domain/Criterion.cs` | `TestTypeId`/`TestType`, required exactly for `TestProcedure` | §5.1 (C30); S3-12 decision 3 |
| `…/Persistence/Seed/FilterValueSeed.cs` (new) | The 7 use cases (`databaseApplication` inactive), 8 app types (hints for open/closed), 7 product parts (explanations verbatim, only `base` always selected), 5 test types (links); fixed ids, sort order = Kriterienkatalog order; MIT + BIT inklusiv header | O-S3-5 D1; keys of C104 |
| `…/Persistence/Configurations/` (11 new, 4 changed) | Fluent API only; `ConfigureCatalogEntity(generateIds: false)` for the seeded tables; CHECKs `CK_FilterExclusions_OneSourceOneTarget`, `CK_FilterExclusions_TargetLayerLower`, `CK_Criteria_TestTypeOnlyForProcedure`, `CK_CriterionProcedureTexts_AllowedRatings`; unique indexes (nulls not distinct for applicability and priority rules); enums as strings, `AllowedRatings` as integer; every foreign key `Restrict` | C104; CAT-13 |
| `…/Persistence/CatalogDbContext.cs` | Five new `DbSet`s, ten new configurations | ADR-014 |
| `…/Persistence/Migrations/*_AddFilterDimensions*` (generated), snapshot | Ten tables, `Criteria.TestTypeId`, seed rows; migration class made `internal` | ADR-014 rule 2; ARC-03 |
| `REUSE.toml` | BIT inklusiv block also covers `*_AddFilterDimensions.cs`, the catalog `*.Designer.cs` files and `CatalogDbContextModelSnapshot.cs` | S3-11 P8 |
| `tests/ProcureAccess.Tests.Unit/Catalog/FilterExclusionTests.cs` (new) | `INV:CAT-13` theory over all 16 layer pairs; the two rules of Kriterienkatalog §3.1 set their typed columns; empty reason | CAT-13 domain part |
| `tests/ProcureAccess.Tests.Unit/Catalog/CatalogRulesImmutabilityTests.cs` (new) | `INV:CAT-05`: `AddExclusion`, `MapProcedure` and the three revision methods after `Publish`; drafts keep their rows; duplicates; ratings; rationale; procedure criteria; the test-type rule of `Criterion` | CAT-05 for the new rows |
| `tests/ProcureAccess.Tests.Integration/Catalog/FilterValueSeedTests.cs` (new) | The seeded values written out (keys, names, flags, hints, links, one explanation); a Respawn reset keeps them | D1 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogRulesPersistenceTests.cs` (new) | Round trip of a published release with every new row on seeded values; `INV:CAT-05` a loaded revision refuses new rows; `INV:CAT-13` the database refuses the three higher-or-equal pairs and accepts the six lower ones; CHECKs for two sources, the test-type rule and allowed ratings; nulls-not-distinct duplicates | CAT-13 database part; C104 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogPersistenceTests.cs` | The procedure criterion of the round trip names the seeded `pdfTest` and is stored through the context that loaded it | `Criterion` test-type rule |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/ApiFixture.cs` | Respawn ignores `catalog."UseCaseTemplates"`, `"AppTypes"`, `"ProductParts"`, `"TestTypes"` | S3-11 P6, D1 |
| `docs/TESTING_v31.md` (new) | V93: CAT-13 `active` with new wording (domain and CHECK, no editor), CAT-05 covers the new rows; input document `ARCHITECTURE_v19.md` | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v19.md` (new) | C104 (as built, keys, `IsInScope` removed, `Description` optional, database rules, immutability); §4.3 Catalog row; §5.1 table and paragraph „Filter values“ | Owner decisions |
| `CLAUDE.md` | Header; §3 filter dimensions and rules; §4 Catalog folders | CLAUDE §11 |

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | No endpoint, no outside input; raw SQL only in tests, from constants and `InlineData` |
| 2 | Correctness — CAT-13 | Domain: every pair of the four layers (unit theory). Database: the CHECK refuses exactly app type → app type and product part → app type / product part; `num_nonnulls` enforces one source and one target |
| 3 | Correctness — CAT-05 | Every new mutating member checks the seal or the publication first; exclusions and filter values have no mutators; a loaded revision refuses new rows (integration test) |
| 4 | Correctness — ADR-014, ARC-01 – ARC-03 | Everything in the `catalog` schema; only the two enums are public, in `Contracts`; architecture tests green |
| 5 | N+1 / queries | No queries in production code; every foreign key has an index |
| 6 | Edge cases | Empty texts; ratings `0`, unknown bit; the same rule, pair or key twice; procedure criteria; test type with a requirement |
| 7 | Seed vs. Kriterienkatalog §2 | Compared by script: all names, hints and the seven explanations match character for character |
| 8 | Time | `CreatedAt` from `TimeProvider`; seeded rows a fixed date |
| 9 | A11y, UI strings | No UI change |
| 10 | Independent review (agent) sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

The review agent (code-reviewer, read-only, against CLAUDE.md, ARCHITECTURE_v19 §5.1 and TESTING_v31) found no correctness or security bug: no public call sequence changes a published release or a sealed revision, the CHECK SQL is right, the mapping matches migration and snapshot, the seed matches Kriterienkatalog §2. Its remarks, sorted per CLAUDE §6.8:

| # | Remark | Sorting |
|---|---|---|
| R1 | The allowed-ratings CHECK and the nulls-not-distinct indexes had no database test (the test class comment claimed the ratings one) | **Fixed:** two raw-SQL theories in `CatalogRulesPersistenceTests` |
| R2 | The CAT-13 acceptance theory missed lower pairs | **Fixed:** all six lower pairs (use case → product part and app type → test type added) |
| R3 | A test comment named a class that does not exist | **Fixed** |
| R4 | No database test for a criterion pointing at a real test type; no loaded-release test for `AddExclusion` / `AddProcedureText` | **Won't fix:** same `ThrowIfPublished` / `ThrowIfSealed` path as the tested members; the round trip stores a procedure criterion with `pdfTest` |
| R5 | No unique index over the six columns of `FilterExclusions`; `AddExclusion` compares references only | **Follow-up (P1):** the import removes duplicate rules |
| R6 | `Weight` has no range, `Level`/`Source` no CHECK | **Won't fix:** no document defines a range; only the domain writes the rows |
| R7 | Argument checks run before the seal check, so a sealed revision with a bad argument throws `ArgumentException` | **Won't fix:** both refuse the change |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (248 before the review fixes; the catalog integration tests again after them, 38 passed) |
| 4 | `node scripts/check-invariants.mjs` (+ self-test) | ✅ gate passed; self-test 36 passed |
| 5 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 6 | `reuse lint` (Docker image) | ✅ compliant, 752 / 752 files; `reuse spdx` gives the new migration and the snapshot `LicenseRef-AllRightsReserved` next to MIT |
| 7 | E2E | Not run locally (the owner's dev stack was running; backend-only change). The startup migration is covered by `Startup_EmptyDatabase_AppliesTheCatalogMigrations`; CI runs E2E on all five browser projects |
| 8 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | `FilterExclusions` has no unique index over source and target, and `CatalogRelease.AddExclusion` refuses only the same object twice: the import removes duplicate rules, or a unique index (nulls not distinct) comes with it (review R5) | S3-16 |
| P2 | `Rating.All` is a named flag value: a JSON enum-to-string converter would write `"all"`; SPEC §8.2 wants `allowedRatings` as an array of levels, so the endpoint maps flags to a list | S3-20 |
| P3 | `UseCaseTemplate` has no `SortOrder` in §5.1; the order of the searchable list in S10 (name or a fixed order) is decided with the endpoint | S3-19 |
| P4 | The procedure-criterion key `PROC-<testTypeKey>` (§5.1, SPEC §8.3) is not checked by the domain; the folder validation checks it with the key scheme (S3-11 Q4) | S3-15 |
| P5 | Integration tests store a release that refers to seeded values through the context that loaded them (the values have fixed ids and `Add` would insert them again); the MiniCatalog builders of S3-14 need the same rule for integration use | S3-14 |
| — | Carried from S3-12 and still open: P2 (interceptor, S3-16), P3 (`number`, S3-15/S3-20), P4 (one active release, S3-17), P5 (`Category`), P6, P7 | as in S3-12 |

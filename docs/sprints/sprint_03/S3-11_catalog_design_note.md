# S3-11 — Catalog module: design note on persistence and the existing criteria

> **Sprint:** 03, P0 task S3-11 (M2.1, 1 point, two runs: note → owner decision). **Issues:** none. **Requirements / invariants:** prepares CAT-05, CAT-07, CAT-13, CAT-23; CAT-10 stays green; SPEC_v2 R-ADM-01, SD20; ARCHITECTURE_v17 §4.3, §5.1, §12 step 2, ADR-014. No invariant changes status; no TESTING version. **Status:** design note written by Claude Code (2026-11-05); owner decision **O-S3-5** taken on 2026-11-06 (below and §2.9); decisions e and f of §2.9 still open.
> **Scope:** exactly the sprint row: `CatalogDbContext` and schema per ADR-014; what goes into `Catalog.Contracts`; how the existing criteria, the `CriteriaFilter` types 1–4 and their exclusions reach the versioned model; what that means for the order of S3-12 – S3-18; what the legacy endpoints read afterwards; how CAT-10 stays green with unchanged expected sets; dropping seed rows 2 and 3. Options with a recommendation, no code. Also in this branch, as a separate commit: the task prompts S3-11 – S3-27 cite `ARCHITECTURE_v17` and `TESTING_v29` (owner, 2026-11-05; `TESTING_v29` comes with S3-10, PR #207, which every later invariant task waits for).
> **Owner decisions (2026-11-05, before the note):** **1** the conflict between ARCHITECTURE §12 („Migrate existing criteria into release 2026.1“), SPEC R-ADM-01 (2026.1 from Kriterienkatalog Teil A/B plus the priority document) and CAT-10 (unchanged sets) is handled inside this note as part of O-S3-5 (F1).
> **O-S3-5 (owner, 2026-11-06):** **a** option **C**: no copy of the seed's criteria; the legacy endpoints stay on the legacy tables until S10 (sprint 04) and steps 5/6 retire them; S3-18 is reduced to seed rows 2 and 3 plus J1 (§2.7). **b** **D1**: the filter values are seeded in the catalog migration. **c** **Q2 (iii)**: `catalog import` accepts a release without priorities, `catalog activate` refuses it. **d** criterion proposals are **kept** as they are.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | **Two places describe release 2026.1 differently.** ARCHITECTURE §12 step 2 says „Migrate existing criteria into release `2026.1`“; the S1-15 decision 1A and the CAT-10 fixture description („step 2 must keep them true for catalog release 2026.1“) say the same. SPEC R-ADM-01 says 2026.1 „is produced from Kriterienkatalog Teil A/B plus the priority document (O-1)“, and so does §12 itself a few sentences later (folder `catalog/releases/2026.1/`, C74), as do ROADMAP M2.3 and S3-27. Teil A/B differ from the seed whatever the project team decides: Teil A has four Basisfunktionalität criteria the seed lacks (F3) and five titles with App-Test variants, Teil B adds 61 PDF criteria. Kriterienkatalog §7 also **proposes** (marked „Vorschlag“) 11.1.4.2 → Audio, 11.8.x → Autorenwerkzeug, 12.x → Basisfunktionalität; only „HR no longer excludes mobile apps“ is decided | The old filter can't answer from 2026.1 and still return the CAT-10 sets (`product-part-basis-only` gains at least the four new Basisfunktionalität criteria; names change). Either the seed's criteria get their own interim release, or they are not copied at all (§2.4) |
| F2 | **Seed** (migration `AddSeedData`, `DAL/MODELS.ProcureAccess/SeedData/`): 4 `FilterType`s, 27 `CriteriaFilter`s (use cases 16–22, app types 8–15, product parts 1–7, test types 23–27), 9 `CriteriaFilterExclusion`s, 127 `Criterion`s. Every criterion has exactly **one** `CriteriaFilterId`: product parts (Basisfunktionalität 85, Video 15, Zweiwege 8, RTT 8, Autorenwerkzeug 5, Biometrie 1, Audio 0) or a test type (one each, named like the test type) | The seed itself has **no app-type applicability and no priorities**, both of which the new model expects (`CriterionApplicability.AppTypeId` is required; CAT-02). App types per criterion are stated in Teil A („Gilt für Anwendungstypen“: 118 criteria for the five Software/App types, 4 for mobile apps only), so a copy can take them from there. Priorities can't be taken from anywhere before O-1 and must not be invented (CLAUDE §6.6) |
| F3 | The 122 seed criteria that hang on product parts carry the EN 301 549 numbers of Kriterienkatalog Teil A: all 122 numbers appear there. Teil A has four numbers the seed lacks (11.1.3.1d, 11.1.3.1e, 11.2.1.1a, 11.2.1.1b, all Basisfunktionalität); six titles differ (five App-Test variants in parentheses, „11.7.“ with a dot). The five test-type criteria correspond to the procedure criteria `PROC-<testTypeKey>` (ARCHITECTURE §5.1, C30) | A key scheme based on the EN 301 549 number fits seed and 2026.1 alike. The seed's `Description` is only the Software-Test „Anforderung“ (Kriterienkatalog §7) |
| F4 | **The legacy tables can't be dropped in step 2.** `ProductCriteriaFilters` (products page, `GET /api/Products/byCriteriaFilterIds`, until step 6) has a foreign key to `CriteriaFilters`; `Proposals` (until step 5) has one to `Criteria`, and approving a criterion proposal still inserts into `Criteria` (`ProposalService`, route `criterion-proposal/:id`). The legacy client reads `/api/FilterTypes`, `/api/CriteriaFilters` (with exclusions) and `/api/Criteria/byCriteriaFilterIds` | In every option `FilterTypes`, `CriteriaFilters` and `CriteriaFilterExclusions` stay in `ApplicationDBContext`. ADR-014 rule 4 (moving a table) does not apply: the new tables have other shapes, nothing is moved |
| F5 | **The legacy screen lives one more sprint.** The configurator S10 replaces `/filters` in sprint 04 (ROADMAP M2.5), together with the J1 and CAT-10 rewrite (M2.5d, TECH_DEBT_v2 #91, #92) | Code that only keeps the legacy screen on the new model is thrown away after about one sprint |
| F6 | **CAT-10 can't hold beyond 2026.1 in any option.** Its wording („same criteria for a fixed seed before and after migration step 2“) holds as long as the old filter answers from the seed's content. Once 2026.1 replaces it, the sets change (F1). S1-15 R2 („subset comparison“ vs. exact sets) is still open | The CAT-10 rewrite in M2.5d needs a new wording in any case; this sprint only has to keep today's sets |
| F7 | **J1 depends on seed rows 2 and 3.** `e2e/journeys/j1-configurator.spec.ts` expects both mobile app types to be disabled once HR-Anwendung is chosen (`EXCLUDED_BY_PRODUCT_TYPE`). `CriteriaFilterExclusion` has a composite key; „rows 2 and 3“ are the second and third entries of `CriteriaFilterExclusionSeed` (17 → 12, 17 → 13). Kriterienkatalog §3.1 names `SampleData.cs`, but the rows are in `CriteriaFilterExclusionSeed.cs` | Dropping them changes J1's expectations (two names out of the list); the CAT-10 sets don't contain exclusions (S1-15 P2) |
| F8 | **The release format lists no filter values.** R-ADM-01 and the S3-15 row name criteria revisions, procedure texts, applicability, priorities, exclusions and procedure mapping — not `AppType`, `ProductPart`, `UseCaseTemplate`, `TestType`. ARCHITECTURE §5.1 pins only exclusions and the procedure mapping to a release; the value rows themselves are global | Where these rows come from is part of „how the `CriteriaFilter` types 1–4 reach the model“ (§2.3) |
| F9 | **Public types in a module:** outside `Contracts` only controllers and the registration class are public (ARC-03). A public **controller action** can't return an `internal` DTO type (CS0050), also not as `ActionResult<T>`. Minimal-API handlers are not public members, so they can return `Ok<T>` with an internal `T`; Platform maps its endpoints that way (`MapPlatformEndpoints`) | Where the HTTP DTOs of `/api/v1/catalog/*` live follows from the endpoint style, O-S3-6 (§2.2) |
| F10 | **Tests and references:** Respawn deletes every row except the two migration histories it ignores (`public`, `platform`), including reference data; CAT-10 stores `ExistingCatalogSeed` per test (S1-15 F4), and so do `LegacyCatalogReadTests` and `ProductsByCriteriaFilterTests`. ARC-03 (`ModuleProject_ReferencedBy_OnlyHostAndTests`) forbids references to a module project from anything but the host and the tests; it does not cover the `Contracts` projects | S3-12 adds `catalog."__EFMigrationsHistory"` to the ignore list. No test stops `SERVICES` from referencing `Catalog.Contracts`, but O-S2-1 keeps legacy code out of the modules until its step: an adapter that reads the catalog belongs in the host `API` |

## 2. Changes

### Design note (options for O-S3-5)

#### 2.1 `CatalogDbContext` and schema (ADR-014 applied; no options)

| Topic | Design |
|---|---|
| Context | `CatalogDbContext` in `src/ProcureAccess.Modules.Catalog/Persistence/`, `internal`, like its entities and `IEntityTypeConfiguration<T>` classes (Fluent API only). Registered by `AddCatalogModule` (today without parameters; it takes the configuration in S3-12, as `AddPlatformModule` does) on the same connection string |
| Schema | All tables in schema `catalog`; history `catalog."__EFMigrationsHistory"`, set in `OnConfiguring` as in `PlatformDbContext` (ADR-014 rule 2) |
| Migrations | `Persistence/Migrations`, made `internal` after `migrations add` (ARC-03). Plain `CreateTable` — rule 4's `IF NOT EXISTS` baseline is only for moved tables, and nothing is moved (F4) |
| Startup | `CatalogDatabaseMigrator : IModuleDatabaseMigrator`, registered after Platform (rule 3: Platform first, then modules in dependency order; Catalog is a leaf) |
| Keys | `int` identity keys for every table; every filter value and `Criterion` also has a unique, stable `Key` (string) that release folders and the API use. Release folders never carry database ids |
| Boundaries | No foreign key, join or query from `catalog` to `public` or back (rule 5). The legacy tables stay where they are (F4) |
| Immutability | CAT-05 in S3-12 (domain error on change of a published release and its revisions); how (interceptor or domain methods only) is S3-12's choice |
| Tests | OPS-03 covers the new context (rule 6); Respawn ignores its history (F10); builders persist through `CatalogDbContext` (TESTING §6.1) |

#### 2.2 What goes into `Catalog.Contracts` (O-22 interim rule)

`Catalog.Contracts` references no project until O-22 is decided (C95), so it can only hold types made of BCL types. Interface names below are proposals; no document names them yet.

| Goes in | When | Why |
|---|---|---|
| Enums used across module borders: `CriterionKind`, `PriorityLevel`, `Rating` (flags) | S3-12 / S3-13 | Configurator, Export, Workspace and Directory will need them; under the interim rule they get them by referencing `Catalog.Contracts` from their module project (allowed, §4.3) |
| The HTTP response DTOs of `GET /api/v1/catalog/filter-options` and `/criteria` (SPEC §8.1, §8.2) as `record`s — **only if O-S3-6 chooses controllers** (F9) | S3-19, S3-20 | With minimal APIs they can stay `internal` in the module. Export's `AdHocSelectionSource` re-resolves the same data (ARCHITECTURE §4.4), but it can use its own contract type |
| A read interface for other modules (e.g. `ICatalogReader`: active release, criteria for a selection, a release by id) and the selection DTO (keys, not ids) | first caller: Export in step 2, later Configurator | ARCHITECTURE §4.3 (modules read each other only through contracts); not needed by S3-12 – S3-18 unless option A or B is chosen (§2.4) |
| Import and activate as an interface (e.g. `ICatalogReleaseAdministration`) — **or** a public command entry on `CatalogModule`, which keeps them out of `Contracts` | S3-16, S3-17 | Where the CLI runs (host `API` or its own entry) is S3-16's choice; either way the host must not reach into `internal` types |

Never in `Contracts`: entities, `CatalogDbContext`, configurations, the import loader or validator. If a contracts type needs a type of another module's contracts, stop and ask (R8, O-22).

#### 2.3 Where the filter values (`CriteriaFilter` types 1–4) come from

| | D1 — seed in the catalog migration | D2 — file in each release folder |
|---|---|---|
| How | `HasData` in `CatalogDbContext` for `UseCaseTemplate`, `AppType`, `ProductPart`, `TestType` with every field of §5.1 (keys, names, hints and explanations, `Functionality`, `IsInScope`, `IsActive`, `IsAlwaysSelected`, `Url`, `SortOrder`; values from Kriterienkatalog §2). Release folders reference them by key | `filters.json` per release; the import inserts or updates the rows by key in the same transaction |
| Matches the documents | Yes: R-ADM-01 and S3-15 list no filter values (F8); §5.1 pins only exclusions and the procedure mapping | Extends R-ADM-01 (SPEC change) |
| Changing a name or hint | New migration (developer) | In the release PR (project team reviews it) |
| Side effect | None on the active release | Importing a release that is not yet active already changes names of the active one (the rows are global, §5.1) |
| Validation of a folder | Against the seeded keys in the database | Self-contained |
| Tests | Respawn removes the rows (F10). Either Respawn also ignores the four tables (static reference data, no second copy), or builders recreate them — another copy that can drift like #92 | Importing the test folder creates them |
| Licence | The product-part explanations are quoted from the Standardanforderungskatalog (Kriterienkatalog §2.3); module migrations are annotated MIT in `REUSE.toml` (open point P8) | Covered by the existing `catalog/releases/**` annotation |

**Recommendation: D1, with Respawn ignoring the four tables.** It is what the documents describe, and importing a release never changes what the active release shows. The few text changes per year don't justify a second import path. Exclusions (`FilterExclusion` + `CatalogReleaseFilterExclusion`) and the procedure mapping come from the release folder in both variants (R-ADM-01).

#### 2.4 The path of the existing criteria — three options

All three keep `FilterTypes`, `CriteriaFilters`, `CriteriaFilterExclusions` and the products and proposals on the legacy tables (F4). They differ in whether the seed's **criteria** get a copy in the catalog and whether the old criteria filter reads it.

**A — generated interim release folder, imported by the CLI.** A one-off script (in S3-18, under `scripts/`, Node without dependencies) writes `catalog/releases/2026.0/` from the seed: 122 requirement criteria with keys from their EN 301 549 numbers, 5 procedure criteria `PROC-<testTypeKey>`, one revision each (title = seed name, the seed `Description` as Software-Test `RequirementDe`), applicability per seed product part × the app types Teil A states for the criterion (F2), the exclusions of Kriterienkatalog §3.1 (without rows 2 and 3), the procedure mapping of §4. The folder is committed and imported with `catalog import 2026.0` in **every** environment that serves `/filters`, production included; it is **not activated**, so it never reaches `/api/v1/catalog/*`. `GET /api/Criteria/byCriteriaFilterIds` becomes an adapter in `API`: it maps the legacy filter ids to keys and asks the catalog through a legacy-only read method for the criteria of release `2026.0` by version whose product part or test type is selected, **ignoring app types**, because the legacy filter is a plain union by filter id (`CriterionService.GetByCriteriaFilterIdsAsync`) and two CAT-10 cases select no app type, which the new selection logic rejects (CAT-11). It maps the result back to `CriterionDto` with the legacy criterion ids (a static map file next to the adapter, deleted with it). The other legacy endpoints stay on the legacy tables.
- Needs: import (not activation) of a release without priorities (§2.6 Q2 (iii)); an import step in the dev, E2E, staging and production stacks (S3-16/S3-17 decide how); CAT-10 imports the folder instead of storing `ExistingCatalogSeed` (the seed copy stays for `LegacyCatalogReadTests` and `ProductsByCriteriaFilterTests`, so #92 stays open).
- The applicability rows are written but unused by the adapter; they would only matter if `2026.0` were ever activated.
- 2026.1 keeps its own content from Teil A/B (F1) and reuses the keys; `2026.0` is never pinned by anything (configurations come in step 3) and is ignored once S10 is built.
- Cost: generator, id map, legacy read method, adapter, stack import — about 3 points as planned; all except the stack import is thrown away in sprint 04 (F5).

**B — EF data migration.** A catalog migration inserts the same release `2026.0` (content as in A). Either it reads `public."Criteria"` etc. with SQL — a join across two contexts, which ADR-014 rule 5 forbids — or it carries the values as C# constants, a third copy of the seed next to `HasData` and `ExistingCatalogSeed` (#92), with BIT inklusiv texts in a module migration that `REUSE.toml` annotates as MIT. The release skips the import validation (CAT-23) and the review folder (R-ADM-01), and it stays in every database's history: removing it later means a migration that deletes an immutable release (CAT-05). The adapter, legacy read method and id map are the same as in A. Advantage: every database has it without a CLI step, and S3-18 can come right after S3-13.

**C — no copy; the old screen stays on the legacy tables until sprint 04.** `GET /api/Criteria/byCriteriaFilterIds` and every other legacy endpoint keep reading `public` unchanged until S10 replaces `/filters` (F5) and steps 5/6 retire products and proposals. The catalog gets content only from release folders: the MiniCatalog for tests (S3-14) and 2026.1 from Kriterienkatalog Teil A/B (S3-27 for the texts, O-1 for priorities). The existing criteria reach the catalog as part of 2026.1 — by their EN 301 549 numbers (F3), with whatever the project team decides on the §7 proposals — not by copying rows. S3-18 shrinks to seed rows 2 and 3 plus J1 (§2.7), about 1 point.
- Needs rewording, with the decision: ARCHITECTURE §12 step 2 („Map the repository's `CriteriaFilter` … onto them“, „Migrate existing criteria into release 2026.1“, „The existing filter UI keeps working against the extended contract“), the same phrase in the S3-18 row, the sprint goal („the existing criteria live in the catalog model“), ROADMAP M2.1 („migration of existing criteria“), S1-15 decision 1A and the CAT-10 fixture description (F1).
- With Q2 (iii) the S3-27 text folder can be imported this sprint (CLI and validation on ≈ 188 real criteria) but not activated: until O-1 the dev, E2E and staging stacks have no active real release, and the new endpoints are tested on the MiniCatalog. If S10 in sprint 04 needs real data before O-1 arrives, Q2 (iv) is the decision to take then.
- CAT-10 stays green trivially and says nothing about the catalog; it never could after 2026.1 (F6).

| | A — interim folder by CLI | B — EF data migration | C — no copy |
|---|---|---|---|
| Follows ARCH §12 / sprint goal as written | Yes (interim release instead of 2026.1, F1) | Yes (same) | No — needs the rewording listed above |
| Follows SD20 / R-ADM-01 (content only by import, folder reviewed) | Yes | No | Yes |
| ADR-014 rule 5 | Yes | Only with a C# copy of the seed | Yes |
| CAT-10 green, sets unchanged | Yes, on the catalog (legacy-only read method) | Yes, on the catalog (same) | Yes, on the legacy tables |
| Needs a release without priorities (§2.6 Q2) | Import only | Inserted by the migration, no check | Import only, for S3-27 before O-1 |
| Code thrown away in sprint 04 | Generator, id map, legacy read method, adapter | Migration content, id map, legacy read method, adapter | None |
| Points of S3-18 | ≈ 3 | ≈ 3 | ≈ 1 |
| Exercises the CLI on real-sized data this sprint | Yes (127 criteria) | No | With S3-27 under Q2 (iii) (≈ 188 criteria) |

#### 2.5 What the legacy endpoints read afterwards

| Endpoint | A | B | C |
|---|---|---|---|
| `GET /api/Criteria/byCriteriaFilterIds` | Catalog, release `2026.0` (imported, not active), through the adapter | as A | legacy `Criteria` |
| `GET /api/Criteria`, `/api/Criteria/{id}` (criterion proposals) | legacy `Criteria` | legacy `Criteria` | legacy `Criteria` |
| `GET /api/CriteriaFilters` (with exclusions), `/api/FilterTypes` | legacy tables, rows 2 and 3 removed | as A | as A |
| `GET /api/Products/byCriteriaFilterIds`, proposals | legacy tables (until steps 5/6) | as A | as A |
| `GET /api/v1/catalog/*` (S3-19, S3-20) | active release | active release | active release |

With A and B the old screen shows criteria from the catalog but filter options from the legacy tables; an approved criterion proposal lands in `public.Criteria` and no longer appears in `/filters` (§2.6 Q3).

#### 2.6 Further questions the decision needs

| # | Question | Options | Claude's proposal |
|---|---|---|---|
| Q1 | Filter values (§2.3) | D1 seed in the catalog migration · D2 file per release folder | **D1**, Respawn ignores the four tables |
| Q2 | May a release without priorities be imported or activated? (A imports one; S3-27 is one until O-1; CAT-02 forbids a resolved priority without rationale, and the UX copy has no text for „no recommendation“) | (i) import requires a catalog-default priority for every requirement criterion · (ii) priorities optional everywhere; the criteria endpoint returns no `priority` (needs a UX copy key and a CAT-02 change) · (iii) import accepts a release without priorities, `activate` refuses it · (iv) as (iii), plus an explicit `activate` option that only the development and E2E stacks use | **(iii)** for every option: nothing without priorities ever reaches `/api/v1/catalog/*`, and the S3-27 texts can be validated now. (iv) only if sprint 04 needs it (§2.4 C). Decide before S3-15 |
| Q3 | Criterion proposals (legacy approval inserts into `public.Criteria`, F4) | keep as is · stop approving criterion proposals from S3-18 (R-SUG-01 moves suggestions to an external channel, S3-21) | Keep with C (nothing changes); stop approving with A or B |
| Q4 | Key scheme of `Criterion.Key` | EN 301 549 number (`11.1.3.1c`) · prefixed (`BITV-SW-11.1.3.1c`, the example in §5.1) | Decide in S3-15 with the folder format; it must be the same for `2026.0` (A) and 2026.1 |

Licence of release folders: `REUSE.toml` already annotates `catalog/releases/**` as `MIT AND LicenseRef-AllRightsReserved` with BIT inklusiv (`precedence = "override"`), so no decision is needed for them.

#### 2.7 Seed rows 2 and 3 (all options)

A legacy migration removes the two entries from `CriteriaFilterExclusionSeed` (EF generates `DeleteData`; `has-pending-model-changes` stays clean, OPS-03), and J1 drops „Mobile Anwendung (offen)“ and „Mobile Anwendung (geschlossen)“ from `EXCLUDED_BY_PRODUCT_TYPE`. Neither a generated folder (A), a migration release (B) nor 2026.1 contains them (Kriterienkatalog §3.1). CAT-10 is not affected (F7). The change does not depend on the catalog: it can stay in S3-18 or go into a separate small PR at any time.

#### 2.8 Order of S3-12 – S3-18

| Option | Order | Change against the plan |
|---|---|---|
| A | S3-12 → S3-13 → S3-14 → S3-15 → S3-16 → S3-17 → S3-18 | None; S3-18 also adds the stack import and the generator |
| B | S3-12 → S3-13 → S3-18 → S3-14 → S3-15 → S3-16 → S3-17 | S3-18 moves up (dependency table: „S3-13 (EF data migration)“) |
| C | S3-12 → S3-13 → S3-14 → S3-15 → S3-16 → S3-17; S3-18 reduced to §2.7, any time | S3-18 shrinks by ≈ 2 points; they can go to S3-27 (release 2026.1 texts, then P0 rather than stretch) |

#### 2.9 Recommendation and decisions needed (O-S3-5)

**Recommendation: C, with D1 and Q2 (iii).** The legacy tables can't go before steps 5/6 anyway (F4), the screen that reads them is replaced in sprint 04 (F5), and 2026.1 differs from the seed whatever the project team decides on the §7 proposals (F1). A copy of the seed would be a third version of the criteria that nobody reviews; it needs a legacy-only read path because the new selection rules can't express the old filter, and it only serves CAT-10 for about one sprint, whose wording has to change with 2026.1 in any case (F6). The ≈ 2 points saved are better spent on the 2026.1 text folder (S3-27), which under Q2 (iii) also tests the CLI on real-sized data this sprint. **A is the second choice**, if the owner wants the sprint goal as written; then with Q3 „stop approving“. B is not recommended (it bypasses SD20 and leaves an immutable release in every migration history).

*Changed from the plan:* the plan for this note leaned towards A. Writing out the consequences (the legacy filter can't be expressed with the new selection rules, legacy tables pinned by products and proposals, CAT-10 ending with 2026.1 anyway) moved the recommendation to C.

| # | Decision | Options | Proposal | Owner, 2026-11-06 |
|---|---|---|---|---|
| a | Path of the existing criteria (§2.4) | A · B · C | **C** | **C** |
| b | Filter values (§2.3, Q1) | D1 · D2 | **D1**, Respawn ignores the four tables | **D1** |
| c | Release without priorities (Q2) | (i) · (ii) · (iii) · (iv) | **(iii)** | **(iii)** |
| d | Criterion proposals (Q3) | keep · stop approving | keep with C; stop approving with A/B | **keep** |
| e | If C: the documents of §2.4 C reworded; S3-27 moves to P0 with the freed points | yes · no | yes; ARCHITECTURE with S3-12 (first catalog task that changes ARCHITECTURE), the sprint plan and ROADMAP right after the decision | open |
| f | `Catalog.Contracts` content (§2.2) | as proposed · other | as proposed; HTTP DTO placement follows O-S3-6 | open |

Q4 (key scheme) is not part of O-S3-5; it is an open point for S3-15 (§5).

## 3. Self-review

| # | Check | Result |
|---|---|---|
| 1 | Seed counts (F2) and the product-part distribution | Counted from `CriteronSeed.cs`; same as S1-15 F3 |
| 2 | Seed numbers vs. Kriterienkatalog Teil A (F3) | Compared by script (scratchpad, not committed): 122/122 numbers present, 4 only in Teil A (all Basisfunktionalität), 6 titles differ |
| 3 | Every option against ADR-014 rules 1–6, ARC-03 and O-22 | §2.1, §2.2, §2.4; B is the only option in conflict (rule 5 or a seed copy) |
| 4 | Nothing decided that belongs to the owner (CLAUDE §6.6) | O-S3-5 and Q1–Q4 are open; no priority or text invented; A's applicability is taken from Teil A, not invented |
| 5 | Scope | Only this note and the task-prompt update; no code, no TESTING or ARCHITECTURE version |
| 6 | Independent fact-check of the first draft (review agent against code and documents), sorted per CLAUDE §6.8 | 12 findings, all **fixed** in this version (no won't-fix, no follow-up issue): licence of release folders already settled in `REUSE.toml` (Q5 removed); A's adapter can't use the new selection rules (legacy-only read method added); C with Q2 (i) contradicted itself (Q2 reworked, (iii) recommended, (iv) added); §7 moves are proposals, not decisions (F1); app types per criterion are in Teil A (F2); F9 holds for controllers only; F10 is a project rule, not an enforced one, for `Contracts`; #92 stays open under A; C's rewording list completed; A imports in production too and does not activate; D1 field list, Respawn variant and licence point; wording of §2.2 and F3 |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `reuse lint` | ✅ compliant (687/687) |
| 2 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` | ✅ all three pass (no change to code or catalogue) |
| 3 | Build and tests | not run: no code changed |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | O-S3-5 a–d decided (2026-11-06); e (rewording of the documents of §2.4 C, S3-27 to P0) and f (`Catalog.Contracts` content) open | Owner, before S3-12 |
| P2 | Q4: key scheme of `Criterion.Key` | S3-15 |
| P3 | S1-15 R3: the CAT-10 fixture copies BIT inklusiv titles but is annotated MIT through `**/*.json` (release folders, `CriteronSeed.cs` and the seed migrations are already dual-annotated) | Owner, with the CAT-10 rewrite (M2.5d) at the latest |
| P4 | S1-15 R2: CAT-10 „subset comparison“ vs. exact sets; with F6 the wording changes in the M2.5d rewrite anyway | Owner, with M2.5d |
| P5 | Kriterienkatalog §3.1 names `SampleData.cs` for rows 2 and 3; they are in `CriteriaFilterExclusionSeed.cs` (German document) | Owner, next Kriterienkatalog version |
| P6 | Respawn ignore list gets `catalog."__EFMigrationsHistory"` (and, with D1, the four filter-value tables) | S3-12 / S3-13 |
| P7 | ARCHITECTURE §5.1 / §16.3.2 say „seed rows `CriteriaFilterExclusion` Id 2 and 3“; the table has a composite key (F7). Reword when §12 step 2 is amended | With decision e |
| P8 | With D1, the product-part explanations (quoted from the Standardanforderungskatalog) land in a catalog migration that `REUSE.toml` annotates as MIT: extend the BIT inklusiv block to the catalog migrations, or keep the explanations out of the seed | Owner, before S3-13 |

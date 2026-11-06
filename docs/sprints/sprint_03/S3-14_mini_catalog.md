# S3-14 — MiniCatalog fixture and builders for step 2

> **Sprint:** 03, P0 task S3-14 (M2.1, 2 points). **Issues:** none. **Requirements / invariants:** none activated (test data); TESTING_v31 §6.1, §6.2, §9 step 2; ARCHITECTURE_v19 §5.1. CAT-05 and CAT-13 (`active`) keep their tests; two CAT-05 test classes now build their data with the new builders. CAT-10, OPS-03, ARC-01 – ARC-03 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: a `MiniCatalog` fixture and builders in `Tests.Shared` with use cases, app types (incl. one without criteria), product parts, test types, exclusions and priority rules on all four levels, used by at least one test of S3-12/S3-13. Not in scope: priority resolution and selection validation (S3-20), the release folder format and its test folder (S3-15), import and activation (S3-16, S3-17), the E2E seeder (TESTING §6.3), a new TESTING version (none is written by another task right now, so §6.2 is an open point, P1).
> **Owner decisions** (2026-11-06, before and during the task): **1A** the MiniCatalog has both releases `T.1` and `T.2` now, as TESTING §6.2 describes; **2A** the MiniCatalog is defined once in C# in `Tests.Shared`, S3-15 writes the same content as its test release folder and S3-16 checks that importing it gives exactly this fixture; **split A** the non-1:1 `SupersededByKey` case is a merge (two `T.1` keys → one `T.2` key), because `Criterion.SupersededByKey` holds one key and cannot store a split (P2). Defaults stated in the plan and not objected to: the fixture uses the seeded filter values, made-up texts, builders behind their own entry point `CatalogData`, `Tests.Shared` references the Catalog module.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `TESTING_v29` and `ARCHITECTURE_v17`; the highest versions are v31 and v19 (CLAUDE §2) | Worked from v31/v19; no new version written |
| F2 | TESTING §6.2 describes the MiniCatalog as two releases with cross-release changes; the sprint row lists the content of one release only | Owner: 1A, both releases |
| F3 | TESTING §6.2 asks for „one split (non-1:1 `SupersededByKey`)“; the model has one successor key per criterion, so only a merge is non-1:1 in the data | Owner: model a merge; P2 |
| F4 | The catalog migration seeds every value the row asks for (inactive `databaseApplication`, `hardware`, `base`, five test types); Respawn keeps those tables, so values a test adds would stay for the next class | The MiniCatalog uses the seeded values; integration tests load them and store through the same context (S3-13 P5) |
| F5 | The Catalog types are `internal`; `Tests.Shared` referenced only `MODELS` | Project reference + `InternalsVisibleTo`; the catalog builders are `internal` and visible to Unit and Integration |
| F6 | `TestData.ACriterion()` already builds a legacy criterion and is used by the CAT-10 tests | Catalog builders get their own entry point `CatalogData` |
| F7 | `Criterion` cannot change after `Create`, so `SupersededByKey` is known when the old criterion is created | The fixture builds both releases at once; P3 for the import |
| F8 | `CatalogRelease.Version` and `Criterion.Key` have unique indexes | Release builder default `T.0` (never a MiniCatalog version); the default criterion key is documented |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/ProcureAccess.Modules.Catalog.csproj` | `InternalsVisibleTo` `ProcureAccess.Tests.Shared` | F5 |
| `tests/ProcureAccess.Tests.Shared/ProcureAccess.Tests.Shared.csproj` | Reference to the Catalog module; `InternalsVisibleTo` Unit and Integration. No new package | F5; ARC-01 allows test-side projects (`ProcureAccess.Tests.` prefix) |
| `tests/ProcureAccess.Tests.Shared/Builders/Catalog/CatalogData.cs` (new) | Entry point: `ACatalogRelease()`, `ACriterion()`, `ARevision()`; `DefaultPublishedAt` | TESTING §6.1; F6 |
| `…/Builders/Catalog/CatalogReleaseBuilder.cs` (new) | Version (default `T.0`), notes, `Including` (shared revisions), `WithExclusion`, `WithProcedure`; published by default, `Draft()`, `Active()` | The database holds published releases only (C101); F8 |
| `…/Builders/Catalog/CatalogCriterionBuilder.cs` (new) | Requirement criterion, or `ProcedureFor(testType)` with key `PROC-<testTypeKey>`; `SupersededBy` | §5.1 (C30) |
| `…/Builders/Catalog/CriterionRevisionBuilder.cs` (new) | `Of(criterion)` or a new criterion by key; title, plain language, rationale, sort order; `Citing`, `AppliesTo`, `WithPriority`, `WithDefaultPriority`, `WithProcedureText` (made-up wording, given allowed ratings); every rule goes through the domain method | A rule the domain refuses fails in the builder |
| `tests/ProcureAccess.Tests.Shared/Fixtures/MiniCatalogValues.cs` (new) | The eleven filter values the fixture uses: `InMemory()` (seeded keys, names, flags, sort orders; ids 0; placeholder explanations, because the seeded ones quote BIT inklusiv) and `LoadAsync(db)` (the seeded rows) | F4 |
| `tests/ProcureAccess.Tests.Shared/Fixtures/MiniCatalog.cs` (new) | `Build(values)` → `T.1` (12 criteria) and `T.2` (11), both published and inactive; keys in `MiniCatalog.Keys`; `RevisionOf(release, key)`. Content: see §2.1 | Row; 1A; split A |
| `tests/ProcureAccess.Tests.Unit/Catalog/MiniCatalogTests.cs` (new) | 14 tests: every case of §2.1 is present (counts, cross-release differences, shared revisions, 1:1 vs. non-1:1, four priority levels, several app types, values, product-part applicability, Hardware, exclusion pairs, procedure mapping and the allowed-ratings union per release, procedure criteria, ratings per test type) | The fixture cannot lose a case silently |
| `tests/ProcureAccess.Tests.Integration/Catalog/MiniCatalogPersistenceTests.cs` (new) | Both releases stored on the seeded values: counts per release, 16 revisions / 15 criteria / 7 exclusions / 1 standard reference, 7 shared revision rows, no filter value added; `InMemory()` equals the seeded rows | 2A; S3-13 P5 |
| `tests/ProcureAccess.Tests.Unit/Catalog/CatalogReleaseImmutabilityTests.cs`, `CatalogRulesImmutabilityTests.cs` | Helpers `Revision`/`PublishedRelease` and the filter values now come from the builders and `MiniCatalogValues.InMemory()` (Dokument → Hardware in two exclusion tests); `INV:CAT-05` tags unchanged | Row: used by tests of S3-12/S3-13 |
| `CLAUDE.md` | Header; §3 MiniCatalog and catalog builders; §4 `Tests.Shared` | CLAUDE §11 |

### 2.1 Content of the MiniCatalog

| Case | In the fixture | Serves |
|---|---|---|
| Use cases | `hr` (active), `databaseApplication` (inactive, with a level-3 rule) | CAT-11, CAT-18 |
| App types | `web` (1), `softwareOpen` (2), `mobileOpen` (4), `hardware` (7) — no requirement criterion and no procedure for Hardware; HR excludes it | CAT-14, CAT-20 |
| Product parts | `base` (always selected; `T-DEFAULT` applies with it), `audio` (`T-AUDIO`), `video` (`T-VIDEO`) | CAT-11 |
| Test types | `softwareTest`, `appTest`, each with its procedure criterion `PROC-…`, default B, no applicability, no texts | CAT-16 |
| Priorities, four levels | `T-PRIO-LEVELS`: (web, hr) A; (softwareOpen, –) Info; (–, hr) B; (–, databaseApplication) A; default B | CAT-01, CAT-02 |
| Several app types | `T-PRIO-APPTYPES`: (web, hr) and (softwareOpen, hr) B weight 2 → sort order decides; (web, –) B 1, (softwareOpen, –) B 3 → weight decides; (mobileOpen, –) A → level decides; default Info | CAT-14 |
| Exclusions | One per pair of layers: hr → hardware, hr → video, databaseApplication → appTest, mobileOpen → audio, web → appTest, audio → softwareTest; `T.2` replaces hr → video by hr → audio (Audio then has two reasons) | CAT-12, CAT-13, CAT-19 |
| Procedure mapping | `T.1`: web, softwareOpen → softwareTest; mobileOpen → appTest **and** softwareTest. `T.2` drops mobileOpen → softwareTest | CAT-21 |
| Allowed ratings | `T-PRIO-LEVELS` (shared): Software-Test without „nicht anwendbar“, App-Test with it — Mobile gets the union in `T.1`, the App-Test text only in `T.2`; `T-VIDEO` has no text (all six levels); `T-CHANGED` changes its ratings with revision 2 | CAT-15, CAT-21 |
| Between releases | changed `T-CHANGED` (revision 2), added `T-ADDED`, removed `T-REMOVED`, renamed 1:1 `T-RENAMED-OLD` → `T-RENAMED-NEW`, merged `T-MERGE-A` + `T-MERGE-B` → `T-MERGED`; seven revisions shared, four new | CAT-03, CAT-06, CAT-07 |

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | Test code only; no endpoint, no secret, no real personal data; texts made up, URLs `example.org` or the seeded links |
| 2 | Correctness — fixture vs. ARCHITECTURE §5.1 | Every priority case resolves to a different rule (checked by hand for each selection, and by the reviewer); every row satisfies the unique indexes and CHECK constraints (integration test stores both releases) |
| 3 | Architecture rules | Only test-side projects reference the module; production code unchanged apart from one `InternalsVisibleTo` line; ARC-01 – ARC-03 green |
| 4 | N+1 / queries | `LoadAsync` makes four queries; no production query |
| 5 | Edge cases | Inactive use case, app type without criteria, criterion without procedure text, procedure criteria, shared exclusion objects, default builder values against unique indexes |
| 6 | Licence | No new dependency; the in-memory values carry no BIT inklusiv explanation, so nothing new for `REUSE.toml` |
| 7 | Independent review (agent) sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

My own pass and the review agent (code-reviewer, read-only, against CLAUDE.md, ARCHITECTURE_v19 §5.1 and TESTING_v31) found no correctness or architecture bug: counts, priority cases, exclusion pairs, procedure mapping and constraints were checked. Sorted per CLAUDE §6.8:

| # | Remark | Sorting |
|---|---|---|
| R1 | (own) The release builder defaulted to `T.1`, the MiniCatalog's version; `Version` is unique | **Fixed:** default `T.0` |
| R2 | (own) `T-AUDIO` had an App-Test text that no app type of its applicability maps to | **Fixed:** text removed |
| R3 | (own) HR excludes Hardware; a Hardware test must not select HR | **Fixed:** stated in both docs |
| R4 | `InMemory()` values have id 0; a later test that compares foreign-key ids sees them as equal | **Fixed:** documented (compare by navigation or key). Real ids would need reflection on a private setter: more mechanism than the fix needs |
| R5 | Audio/Video applicability and the per-release allowed-ratings difference of CAT-21 were described but not asserted | **Fixed:** one new test and assertions in the procedure-mapping test |
| R6 | The default criterion key `T-CRITERION` collides when a test stores two default revisions | **Fixed:** documented in the builder; a counter would make defaults depend on test order |
| R7 | The round trip compares counts, not every column of priorities, applicability and texts | **Won't fix:** the column mapping of every row type is already round-tripped in `CatalogRulesPersistenceTests` and `CatalogPersistenceTests` (S3-12, S3-13); this test is about storing the fixture |
| R8 | `CatalogData.ACriterion()` and `TestData.ACriterion()` share a name | **Won't fix:** separate entry points as stated in the plan; no file needs both |
| R9 | TESTING §6.2 still says „split“ | **Follow-up:** P1 |
| R10 | A merge is non-1:1 only when the predecessors of the new key are counted | **Follow-up:** P2 |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (271) |
| 4 | `node scripts/check-invariants.mjs` | ✅ gate passed |
| 5 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 6 | `reuse lint` (Docker image) | ✅ compliant |
| 7 | E2E | Not run: test code only, no client or API change; CI runs E2E on all five browser projects |
| 8 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | TESTING §6.2 describes the MiniCatalog with „one split“ and names no builders' entry point; the next TESTING version should describe it as built: `T.1`/`T.2` with 12/11 criteria, the non-1:1 case as a merge, `CatalogData` next to `TestData`, `MiniCatalogValues` and the rule for seeded values (no other task writes a TESTING version now) | next task that writes a TESTING version (S3-16) |
| P2 | `Criterion.SupersededByKey` holds one key, so a split cannot be stored and a merge is non-1:1 only when the predecessors of the new key are counted. A real split needs a multi-valued successor or a rule of its own; decide before CAT-03/04 | ARCHITECTURE, before step 6 |
| P3 | `Criterion` cannot change after `Create`. Importing `T.2` when `T.1` is already stored needs a way to set `SupersededByKey` on the existing criterion | S3-15 / S3-16 |
| P4 | Decision 2A: S3-15 writes the MiniCatalog as its test release folder, S3-16 checks that importing it gives exactly `MiniCatalog.Build` | S3-15, S3-16 |
| P5 | The criterion keys `T-…` follow no scheme; if S3-15 decides the key scheme (S3-11 Q4) and the validation enforces it, the keys of the fixture follow | S3-15 |
| P6 | TESTING §6.3: the E2E seeder creates the MiniCatalog in the stack; with 2A it can import the test release folder | with the first rebuilt screen that needs it (S10, sprint 04) |

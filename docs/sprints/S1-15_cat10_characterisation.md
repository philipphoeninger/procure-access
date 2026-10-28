# S1-15 — CAT-10 characterisation test, API part

> **Sprint:** 01, task S1-15 (milestone M0.3, TESTING §4.2, §5.2, §6, §8.1, §9 step 0). **Status:** done by Claude Code (2026-10-23).
> **Scope:** a tagged integration test that fixes the current behaviour of the existing criteria filter `GET /api/Criteria/byCriteriaFilterIds` for a fixed seed and fixed selections; expected sets in a fixture file. Not in scope: the E2E part of CAT-10 (J1, S1-19), setting CAT-10 to `active` in TESTING §4 (S1-21), any change to the filter itself.
> **Owner decisions 2026-10-23:** **1A** the seed is the real reference data of migration `AddSeedData` (the data step 2 moves into release 2026.1), not a test-owned seed. **2A** compared fields: `id`, `name`, `criteriaFilterId`; `description` is left out. **3** six selections as proposed (see §2).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `CriteriaController.ByCriteriaFilterIds` → `CriterionService.GetByCriteriaFilterIds`: `Where(CriteriaFilterId in ids).OrderBy(Id)`, projected to `CriterionDto {id, name, description, criteriaFilterId}`; plain JSON array, anonymous access | The test calls it without a principal (configurator without account, J1) |
| F2 | The client sends every marked filter id of all four layers (`with-criteria.ts`) | Selections mix use case, app type, product part and test type ids |
| F3 | Seed: 4 filter types, 27 criteria filters, 127 criteria. Criteria hang only on product parts (Basisfunktionalität 85, Video 15, Zweiwege 8, RTT 8, Autorenwerkzeug 5, Biometrie 1, **Audio 0**) and test types (1 each); use cases and app types have none | Cases cover a part with criteria, a part without, layers without criteria, and all criteria |
| F4 | Respawn removes the seed before each class (S1-13 decision 1A); the seed rows carry fixed ids | The test class resets and stores the seed **per test** (S1-13 P4), so theory rows cannot collide |
| F5 | The seed arrays are the static instances used by `HasData` | `ExistingCatalogSeed` copies scalar values into new entities, so EF never fixes up the shared instances |
| F6 | Descriptions are BIT inklusiv–style texts that step 2 replaces with the imported BIT inklusiv texts (REUSE.toml: not MIT) | Not stable and not copied into the MIT-annotated fixture file |
| F7 | Like `ReadAll`, the filter drops deleted criteria (query filter `!IsDeleted` on `Criterion`). Unlike `ReadAll`, it ignores proposal status. It never reads `CriteriaFilter`, so criteria of a deleted criteria filter or filter type are still returned (and `CriteriaFilter`'s own query filter checks only `FilterType.IsDeleted`, not its own `IsDeleted`). The server does not enforce exclusions *(corrected 2026-10-28, §5)* | **Not** pinned: step 2 changes these on purpose (CAT-12). The seed has no deleted rows and no proposal criteria |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `tests/ProcureAccess.Tests.Shared/Fixtures/ExistingCatalogSeed.cs` (new) | Fresh copies of `FilterTypeSeed`, `CriteriaFilterSeed`, `CriterionSeed` with fixed ids and `CreatedAt = TestData.DefaultCreatedAt`; exclusions and products left out (the filter does not read them) | Decision 1A, F4, F5 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CriteriaFilterCharacterisationTests.cs` (new) | `INV:CAT-10 CriteriaByFilterIds_FixedSeedAndSelection_ReturnsRecordedCriteria`, `[Trait("Invariant", "CAT-10")]`, one theory row per fixture case. Compares the raw JSON (TESTING §5.2) as an unordered set on `comparedFields`; reports unexpected and missing criteria separately, plus a count check for duplicates | CAT-10, TESTING §8.1 |
| `tests/ProcureAccess.Tests.Integration/Catalog/Fixtures/cat-10-criteria-filter.json` (new) | Seed description, `comparedFields`, six cases with their recorded criteria (one per line) | "Done when": expected set as a fixture file. Covered by `**/*.json` in REUSE.toml |
| `tests/ProcureAccess.Tests.Integration/*.csproj` | Copy `Catalog/Fixtures/*.json` to the output folder | Read at run time |
| `CLAUDE.md` §3 | Current state: CAT-10 API test exists | CLAUDE §11 |

**Cases** (ids of the seed):

| Case | Selection | Criteria |
|---|---|---:|
| `product-part-basis-only` | 1 | 85 |
| `j1-like-selection` | 17, 10, 1, 3, 4, 23 | 109 |
| `all-product-parts-and-test-types` | 1–7, 23–27 | 127 |
| `use-case-and-app-type-only` | 17, 10 | 0 |
| `product-part-without-criteria` | 2 (Audio) | 0 |
| `unknown-filter-id` | 999 | 0 |

**How the expected sets were made.** A temporary test (deleted, not committed) stored the seed, called the endpoint for each case and wrote the results. A separate script then computed the same sets directly from `CriteronSeed.cs`: all six cases matched exactly, and no case had duplicates. No new packages; no production code changed.

## 3. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; 136 warnings, unchanged from S1-14, none from `tests/` |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ exit 0; 22/22: Unit 3, Integration 19 (13 + 6 CAT-10 rows), Architecture 0 (still allowed to be empty) |
| 3 | xUnit `-list full -trait "Invariant=CAT-10"` | ✅ the theory with trait and `INV:CAT-10` display name; it runs as 6 rows |
| 4 | Negative: one name changed in the fixture copy in `bin/` | ✅ the 3 cases containing that criterion fail ("criteria returned that the recording does not contain"); restored |
| 5 | Negative: criterion 2 (Biometrie) moved to filter 1 in `ExistingCatalogSeed` | ✅ 3 cases fail and name `id=2 \| name=5.3 Biometrie \| criteriaFilterId=1`; restored |
| 6 | `dotnet format --verify-no-changes`, the four test projects | ✅ |
| 7 | `reuse lint` | ✅ compliant (491/491). `reuse` was no longer installed; run from a temporary venv |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Step 2 replaces `CriteriaFilter`/`Criterion` with the catalog-release model and the seed with `catalog import`. Then `ExistingCatalogSeed` goes, the test imports release 2026.1 instead and calls the new criteria endpoint; the fixture file stays the reference. If ids or filter ids are not carried over, narrow `comparedFields` (e.g. to `name`) in that PR, with a reason | Step 2 |
| P2 | The removal of the exclusion seed rows 2 and 3 (§16.3.2) does not touch CAT-10: exclusions are not part of the seed copy, and no case relies on them | Step 2 |
| P3 | An empty selection (no `criteriaFilterIds`) is not a case (decision 3). It currently returns `[]` | If wanted, add it in S1-19 or step 2 |
| P4 | Storing rows with fixed ids does not advance PostgreSQL identity sequences. This is harmless as long as no test mixes seed rows and generated ids in one table within one class | Whoever does that first |
| P5 | CAT-10 stays `planned` in TESTING §4 until the gate exists (S1-21) | S1-21 |
| P6 | Disk space: still 305 MB free on the root file system (S1-13 P1) | Philipp |

## 5. Review follow-up (2026-10-28)

Code review of PR #48 (8 findings), sorted per CLAUDE §6.8. None changes current behaviour: #48 added only a test, a fixture and this note. Fixed in `sprint01/S1-15-review-fixes`:

| # | Finding | Fix |
|---|---|---|
| R1 | F7 said the filter ignores `IsDeleted`. `CriterionConfiguration` has `HasQueryFilter(x => !x.IsDeleted)`, so deleted criteria are already dropped, as in `ReadAll`. The gap is elsewhere: the filter never reads `CriteriaFilter`, so deleted criteria filters and filter types are not checked. Step 2 (CAT-12) could otherwise add a filter that exists and miss the gap | F7 corrected (§1). The PR body of #48 still has the old wording |

Open, owner decision:
- R2 — TESTING §4 calls CAT-10 a "subset comparison on stable fields"; the test compares the sets exactly (fails on extra and on missing criteria), and this note gives no reason. Either "subset" means a subset of the *fields* (then the test fits), or the response only has to *contain* the recorded criteria (then the test is too strict and fails when release 2026.1 adds criteria in step 2). TESTING or the test changes accordingly.
- R3 — F6 cites REUSE.toml for "not MIT", but REUSE.toml marks only `docs/kriterienkatalog_bit_inklusiv_v1*.md` as non-MIT. `CriteronSeed.cs` and the fixture (which copies the BIT inklusiv criterion titles) are annotated MIT. Either the reason in F6 or the annotations are wrong.

Follow-up issues: R4 — the comparison key uses `JsonElement.ToString()` and ignores the JSON type (`1` = `"1"`, `null` = `""`): #91. R5 — `ExistingCatalogSeed` copies properties by hand and can drift from the `HasData` seed: #92.

Won't fix (test-only, cheap):
- R6 — the fixture loader throws a bare `NullReferenceException` when `comparedFields`, `cases` or a case's `expected` is missing, and duplicate case names are not checked: the fixture is edited rarely and by hand; the stack trace points at the loader.
- R7 — the duplicate-count message always blames the response, also when the recording contains a duplicate: the recording was checked for duplicates when it was made (§2).
- R8 — one Respawn reset and full seed per theory row: six rows, and the per-test reset is the documented choice of F4 (S1-13 P4).

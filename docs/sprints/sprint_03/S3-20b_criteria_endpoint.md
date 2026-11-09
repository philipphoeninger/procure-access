# S3-20b — `GET /api/v1/catalog/criteria`

> **Sprint:** 03, P0 task S3-20 (M2.4, 3 points), second of two parts (owner decision 4; part a: `S3-20a_priority_resolution.md`, PR #217). **Issues:** none. **Requirements / invariants:** SPEC_v2 §8.2, §8.6 (now v2.4); **CAT-06**, **CAT-16**, **CAT-20**, **CAT-21** → `active` with their catalog-endpoint parts; contract parts of **CAT-02** and **CAT-11** and the HTTP parts of **CAT-12**, **CAT-18**, **CAT-19** done — all in `TESTING_v36.md` (V111–V115).
> **Status:** done, pull request open.
> **Scope:** the rest of the sprint row: the endpoint per SPEC §8.2 on the resolver and selection check of S3-20a, with allowed ratings, procedure texts, procedure criteria from test types, `appTypesWithoutCriteria[]`, release pinning and 404 for an incomplete release. **Not in scope:** configurations (step 3), export (step 2, later task), the screens S10/S11 (sprint 04).
> **Owner decisions** (2026-11-09, before S3-20, all as recommended; numbered as in S3-20a): **2** `productPart` singular — the selected part with the lowest sort order of the matching applicability rows, `null` when none names a part; **3** `allowedRatings: []` for a procedure criterion; **7** 404 `not-found` for a release `catalog activate` would refuse. **Defaults** stated before the task and not objected to: a request without `base` gets 400; `number` = key, empty (`null`) for `PROC-…`; `appTypesWithoutCriteria[]` for the whole selection incl. product parts; order `sortOrder`, then key; caching like filter-options. Decisions 1, 5, 6 were applied in S3-20a.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17` and `TESTING_v29`; the highest versions are v23 and v35 (CLAUDE §2) | Worked from v23 / v35, wrote v24 / v36 |
| F2 | `CatalogReleaseSnapshot` held only criterion keys and exclusions; the resolver of S3-20a works on `CriterionRules` | Snapshot holds every revision's content, the procedure mapping and the gaps; one factory `From(release, testTypeKeys)` for the reader and the unit tests |
| F3 | „Incomplete“ (decision 7) needs the same rules as `catalog activate`, which had them inline as a database query | Moved to `ReleaseCompleteness.FindGaps`, used by both; the activator's messages are unchanged |
| F4 | SPEC §8.2 says „enums are serialised as camelCase strings“, but its example sends `"level": "A"`, and the UX keys are `criteria.priority.A` / `.B` / `.Info`; minimal APIs have no enum converter configured | Enum values are mapped to strings in the module; `level` as `A` / `B` / `Info` (SPEC example and UX keys agree), the rest camelCase; recorded in SPEC v2.4-4 |
| F5 | SPEC §8.2 left `standardReferences[]` and `procedureTexts[]` open („…“) and used a key `BITV-SW-11.2.1.1` from before the key scheme of S3-15 | Shapes as built recorded in SPEC v2.4-4 and the example updated |
| F6 | The revision's own `RationaleDe` and `Category` exist in the model, but SPEC §8.2 does not list them and no screen or export column (§8.3) uses them | Not sent (SPEC wins); C109 (8) |
| F7 | C31 says the allowed ratings come from the procedures mapped to the *selected* app types, not only those the criterion applies to | Implemented as written; the validator of S3-15 already ties every text to an app type the criterion applies to |
| F8 | filter-options and criteria built the same ETag / `Cache-Control` answer | Shared `Http/CatalogJsonAnswer`; filter-options unchanged in behaviour (its integration tests pass) |
| F9 | The MiniCatalog had every case needed: `T.2` drops Mobile offen → Software-Test (CAT-21), Hardware has no criteria (CAT-20), 99.6 changes and 99.7 disappears between releases (CAT-06) | No fixture change; an incomplete release is a copy of `T.1` without the priorities of 99.3 in a temporary folder |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Criteria/CriteriaEndpoint.cs` (new, `internal`) | `MapGet("/criteria")` with `release`, `useCase`, `appTypeIds[]`, `productPartIds[]`, `testTypeIds[]`; 404 for an unknown, missing or incomplete release; 400 `validation` / `selection-excluded` from `CriteriaSelectionReader` | SPEC §8.2; decisions 5, 7 |
| `…/Criteria/CriteriaResolver.cs` (new, `internal`) | The answer: requirement criteria that apply and one `PROC-…` per selected test type, in list order; priority from `PriorityResolver`; `productPart` (decision 2); `number`; texts of the procedures mapped to the selected app types and the union of their ratings (all six without a text, `[]` for `PROC-…`, decision 3); `appTypesWithoutCriteria[]`; enum values as strings | CAT-02, CAT-16, CAT-20, CAT-21; C31 |
| `…/Criteria/CriteriaResponse.cs` (new, `internal`) | Response records in the shape of SPEC §8.2 | C77 |
| `…/Reading/CatalogReleaseSnapshot.cs` (new) | `CatalogReleaseSnapshot` (moved from `CatalogReader.cs`) with `Criteria`, `Exclusions`, `Procedures`, `Gaps`, `IsComplete`, derived `CriterionKeys`, and `From(release, testTypeKeys)`; `CriterionSnapshot`, `StandardReferenceSnapshot`, `ProcedureTextSnapshot`, `ProcedureMapping` | F2 |
| `…/Reading/CatalogReader.cs` | Loads the release graph once with `AsSplitQuery` (no projection query for exclusions any more) and builds the snapshot with `From` | F2; §9.6 |
| `…/Releases/ReleaseCompleteness.cs` (new) | The two completeness rules as one pure function | F3; decision 7 |
| `…/Releases/CatalogReleaseActivator.cs` | Uses `ReleaseCompleteness` | F3 |
| `…/Http/CatalogJsonAnswer.cs` (new) | ETag, 304, `Cache-Control` for named and active releases | F8 |
| `…/FilterOptions/FilterOptionsEndpoint.cs` | Uses `CatalogJsonAnswer`; its two constants moved there | F8 |
| `…/CatalogModule.cs` | Maps `CriteriaEndpoint` | — |
| `tests/ProcureAccess.Tests.Unit/Catalog/Criteria/CriteriaResolverTests.cs` (new) | 13 tests: criteria for Web in list order; Audio / Video parts; procedure criteria per test type (CAT-16); `PROC-…` without ratings; texts of the mapped procedures (CAT-16); same criterion in `T.1` / `T.2` (CAT-21); no text → six levels; Hardware and an app type whose criteria need an unselected part (CAT-20); `productPart` (decision 2); content and explained priority (CAT-02); incomplete release refused; MiniCatalog complete | Tests first for the rules |
| `tests/ProcureAccess.Tests.Integration/Catalog/CriteriaEndpointTests.cs` (new) | 12 test methods: JSON shape and explained priorities (CAT-02, contract); texts with source URLs and one `PROC-…` (CAT-16, contract); 4 incomplete selections → `validation` (CAT-11); HR + Hardware → `selection-excluded` (CAT-12); `release=T.1` rules with `T.2` active (CAT-19); inactive use case (CAT-18); `T.1` revisions with `T.2` active, checked against the database (CAT-06); mapping of each release (CAT-21); Hardware (CAT-20, contract); incomplete release → 404; unknown / no active release → 404; caching headers | HTTP parts |
| `tests/ProcureAccess.Tests.Unit/Catalog/FilterOptionsResolverTests.cs` | Snapshot helper uses `CatalogReleaseSnapshot.From` | F2 |
| `docs/TESTING_v36.md` (new) | V111 CAT-06, V112 CAT-16, V113 CAT-20, V114 CAT-21 `active` with their deferred parts; V115 contract and HTTP parts of CAT-02, CAT-11, CAT-12, CAT-18, CAT-19 | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v24.md` (new) | C109; §7.2 criteria as built; §9.6 names both endpoints | — |
| `docs/SPEC_v2.md` | v2.4: change rows v2.4-1 – v2.4-4, §8.2 example and notes, §8.6 `selection-excluded` | Decisions 2, 3, 5, 7; F4, F5 |
| `CLAUDE.md` | §3 state of S3-20b, §4 `Criteria/`, `Http/`, §5 curl example, version line | — |

### 2.1 How to call it

```bash
# active release (404 until one is activated); base must always be sent
curl 'http://localhost:5000/api/v1/catalog/criteria?useCase=hr&appTypeIds[]=web&productPartIds[]=base&testTypeIds[]=softwareTest'
# a named release, e.g. one a saved list is pinned to
curl 'http://localhost:5000/api/v1/catalog/criteria?release=2026.1&appTypeIds[]=web&productPartIds[]=base'
```

## 3. Self-review

Checklist of CLAUDE §6.7, plus `/engineering:code-review` on the diff.

| # | Area | Result |
|---|---|---|
| 1 | Security | Public, read-only, like filter-options; no input reaches a query (keys are checked against the cached filter values). Error texts echo the sent keys, JSON-encoded. No new dependency |
| 2 | Correctness | Every rule of C109 and SPEC v2.4 has a unit test, every invariant an HTTP test; an incomplete release can never reach the resolver |
| 3 | N+1 / performance | One release graph load per release (split queries), then memory only; per request a few loops over the criteria of the release |
| 4 | Edge cases | Hardware alone, Hardware with a test type, an app type whose criteria all need an unselected part, a criterion without any text, the same revision in two releases with different mappings |
| 5 | Mutation check | `CriteriaResolver` ignored the release mapping: the CAT-16 and CAT-21 unit tests fail; restored from a backup copy |
| 6 | Architecture | All new types `internal`; no new project reference; ARC-01 – ARC-03 green |
| 7 | a11y / UI texts | No UI change |

### 3.1 Review findings

| # | Finding | Sorting |
|---|---|---|
| R1 | filter-options still serves a release with gaps, while criteria answers 404 for it | **Won't fix:** filter-options needs no priority; such a release can only be named, never activated (open point P2) |
| R2 | The 404 detail of an incomplete release lists every gap, which can be long | **Won't fix:** the catalog is public and the list helps whoever imported the release |
| R3 | Swagger shows `useCase` as an array, though SPEC allows one | **Won't fix:** the array is how the endpoint sees a second value and answers 400 (CAT-11); the operation description says „at most one“ |
| R4 | Import and activation helpers are repeated in `FilterOptionsEndpointTests` and `CriteriaEndpointTests` | **Won't fix** in this PR: a third catalog endpoint test would be the time to share them |
| R5 | The first filter-options request for a release now loads its whole content, not only its exclusions | **Won't fix:** once per release and process, then cached; the criteria endpoint needs it anyway |

No correctness or security finding affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (619) |
| 4 | Mutation check (§3 row 5) | ✅ two tests fail, restored |
| 5 | `node scripts/check-invariants.mjs` | ✅ gate passed with TESTING_v36 |
| 6 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 7 | `reuse lint` (Docker image) | ✅ compliant |
| 8 | E2E | Not run locally: no client change, and no stack has an active release; CI runs it |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | No stack has an active release, so both catalog endpoints answer 404 there; the E2E stack needs one when S10 calls them (S3-19 P1) | S10 (sprint 04) / S3-27 |
| P2 | A release with gaps stays stored; filter-options serves it by name, criteria does not (R1). If that matters, a release with gaps could be refused by the import instead (O-S3-5 Q2 (iii) chose otherwise) | Owner, when release 2026.1 is imported (S3-27) |
| P3 | UX key for `selection-excluded` on the client (S2-11 P1) | UX copy session #181 / S10 |
| P4 | `matchedRule.kind` `appTypeAndUseCase` maps to the UX key `criteria.priority.rule.full`; the client needs that one mapping (SPEC §8.2 „maps to `criteria.priority.rule.*`“) | S11 |
| P5 | Kriterienkatalog K-4 (one product part per criterion) is still open; decision 2 covers several parts until it is decided | Project team |
| P6 | Deferred invariant parts: CAT-06 and CAT-21 for stored configurations (step 3), CAT-16 deselection and export, CAT-20 FE part (S11) | Their steps (TESTING_v36) |

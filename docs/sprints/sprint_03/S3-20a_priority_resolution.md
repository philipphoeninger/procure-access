# S3-20a — Priority resolution and criteria selection check

> **Sprint:** 03, P0 task S3-20 (M2.4, 3 points), first of two parts (owner decision 4). **Issues:** none. **Requirements / invariants:** SPEC_v2 §8.2 (`matchedRule.kind`), §8.6 (`selection-excluded`); **CAT-01**, **CAT-14** → `active`; **CAT-02**, **CAT-11** → `active` with their unit parts; selection-check unit parts added to **CAT-12**, **CAT-18**, **CAT-19** — all in `TESTING_v35.md` (V106–V110).
> **Status:** done, pull request open.
> **Scope:** the pure, `internal` logic of `GET /api/v1/catalog/criteria`: the selection check with field errors (CAT-11) and the priority resolution (CAT-01, CAT-02, CAT-14), table-driven over the MiniCatalog. **Not in scope (S3-20b):** the endpoint itself, the criteria content in the release snapshot, allowed ratings and procedure texts (CAT-16, CAT-21), procedure criteria from test types (CAT-16), `appTypesWithoutCriteria[]` (CAT-20), release pinning of the answer (CAT-06), the contract parts of CAT-02 and CAT-11, SPEC §8.2 changes (decisions 2, 3).
> **Owner decisions** (2026-11-09, before the task, all as recommended):
> 1. **Resolution per app type:** each selected app type the criterion applies to runs the full chain 1 → 2 → 3 → 4; the strictest result wins. Example: 99.1, Web + Software (offen), no use case → B (default, decided by Web), not Info.
> 2. **`productPart`** stays singular (SPEC §8.2): the selected part with the lowest sort order the criterion applies to, `null` if its applicability names no part — S3-20b.
> 3. **`allowedRatings` of a `PROC-…` criterion** is an empty list — S3-20b.
> 4. **Split** S3-20 into S3-20a (this task) and S3-20b (endpoint), each with its own TESTING version.
> 5. **Excluded value → 400 `selection-excluded`** with field-keyed `errors`; an incomplete selection stays 400 `validation`.
> 6. **Inactive use case accepted** by the criteria selection, as by filter-options; V102's „rejection for new selections“ moves to the configurator.
> 7. **Incomplete named release** (a criterion without matching rule) → 404 `not-found` — S3-20b; here the resolver returns null for it.
>
> Defaults stated before the task and accepted: a request without `base` gets 400; a rule without weight ranks below one with; `number` = key (empty for `PROC-…`); `appTypesWithoutCriteria[]` for the whole selection incl. product parts; order `sortOrder`, then key; caching like filter-options (the last four for S3-20b).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17` and `TESTING_v29`; the highest versions are v22 and v34 (CLAUDE §2) | Worked from v22 / v34, wrote v23 / v35 |
| F2 | ARCHITECTURE §5.1 „Rules 1 and 2 are evaluated per selected app type; the strictest result wins“ can be read as „per app type, whole chain“ or „rule level first“; the two give different results for 99.1 (B vs. Info) | Asked before coding: decision 1 |
| F3 | SPEC §8.2 sends one `productPart`; the release format (S3-15) allows several parts or none per criterion (Kriterienkatalog K-4 still open) | Asked: decision 2 (S3-20b) |
| F4 | C31 would give a procedure criterion all six ratings, though it is never rated (C30) | Asked: decision 3 (S3-20b) |
| F5 | Risk R3: validation, resolution, ratings, texts, endpoint and integration tests do not fit one session | Asked: decision 4, split |
| F6 | SPEC §8.6 lists `selection-excluded` without saying where it is used (S2-11 P1); CAT-11 and ARCHITECTURE §7.2 say only „400 with field errors“ | Asked: decision 5 |
| F7 | TESTING_v34 V102 deferred „rejection for new selections“ of inactive use cases to this endpoint, against S3-19 decision 4 (accepted at filter-options) | Asked: decision 6 |
| F8 | The import accepts a release without priorities, only `catalog activate` refuses it (O-S3-5 Q2 (iii)); `release=` can still name such a release, and CAT-02 forbids an unexplained priority | Asked: decision 7 (S3-20b); the resolver returns null |
| F9 | The MiniCatalog already holds the rule cases: 99.1 has rules on all four levels incl. an inactive use case, 99.2 the several-app-type cases (level, weight, sort order); `T.1`/`T.2` have one exclusion per pair of layers | No fixture change; three edge cases (no weight vs. weight, app type the criterion does not apply to, part-limited applicability) use small revisions of their own |
| F10 | `FilterOptionsResolver` had a private key reader that the criteria selection needs too, but with the position of each key for the error key of an exclusion | Moved to `Selection/SelectionKeys`, returning key and position; same behaviour for filter-options |
| F11 | The prompt names the branch `sprint03/S3-20-criteria-endpoint` | After the split: `sprint03/S3-20a-priority-resolution`; S3-20b gets `sprint03/S3-20b-criteria-endpoint` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Criteria/CriteriaSelection.cs` (new, `internal`) | `CriteriaSelection` (four layers by key), `SelectionProblem` (`Invalid` → `validation`, `Excluded` → `selection-excluded`), `CriteriaSelectionResult`, `CriteriaSelectionReader.Read`: empty values = not given, a repeated key once; errors `useCase` (unknown, more than one), `appTypeIds` (none), `productPartIds` (an `IsAlwaysSelected` part missing), `<field>[i]` (unknown key); only a complete selection is checked against the release's exclusion rules, each excluded value gets `<field>[i]` naming every selected value that excludes it | CAT-11, CAT-12, CAT-19; decisions 5, 6 |
| `…/Criteria/CriterionRules.cs` (new, `internal`) | `ApplicabilityRule`, `PriorityRule`, `CriterionRules` (key, kind, applicability, priorities by key; `From(CriterionRevision)`); `AppTypesApplyingTo(selection)`: selected app types with an applicability row without part or with a selected part | Entity-free input the release cache can hold (S3-20b) |
| `…/Criteria/PriorityResolver.cs` (new, `internal`) | `MatchedRuleKind`, `MatchedRule`, `ResolvedPriority`; `PriorityResolver.Resolve`: chain per applicable app type (rules 3/4 only for a procedure criterion), strictest of the results by level, weight (none lowest), sort order, key; null if a chain finds no rule; throws for a requirement criterion that does not apply | CAT-01, CAT-02, CAT-14; decisions 1, 7 |
| `…/Selection/SelectionKeys.cs` (new, `internal`) | Shared reader of list parameters (`SentKey` = key + position) | F10 |
| `…/FilterOptions/FilterOptionsResolver.cs` | Uses `SelectionKeys`; private `ReadKeys` removed | F10 |
| `…/Domain/PriorityRecommendation.cs`, `CriterionApplicability.cs` | Comments point to the resolver instead of „S3-20“ | Review |
| `tests/ProcureAccess.Tests.Unit/Catalog/Criteria/PriorityResolverTests.cs` (new) | 9 test methods: one app type, 8 rows (CAT-01); several app types, 7 rows incl. decision 1's example and request order (CAT-14); weight vs. none, app type the criterion does not apply to, part-limited applicability (CAT-14); procedure criterion (CAT-01); missing rule → null; not applying → throws; every criterion of `T.1`/`T.2` × 5 selections explains itself with its rule's values (CAT-02) | Tests first |
| `tests/ProcureAccess.Tests.Unit/Catalog/Criteria/CriteriaSelectionReaderTests.cs` (new) | 10 test methods: complete selection, no use case and no test type, empty/repeated values, inactive use case (CAT-18); 8 broken selections (CAT-11); one row per `T.1` exclusion (CAT-19); `T.1` vs. `T.2` rules (CAT-19); several excluding values named; one error per excluded value | Tests first |
| `docs/TESTING_v35.md` (new) | V106 CAT-01, V107 CAT-02 (unit), V108 CAT-11 (unit, new wording), V109 CAT-14 (new wording) `active`; V110 selection-check parts of CAT-12, CAT-18, CAT-19 | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v23.md` (new) | C108; §5.1 „Several app types“ as built; §7.2 the two 400 answers of `/criteria` | Decisions 1, 5–7 |
| `CLAUDE.md` | §3 state of S3-20a, §4 `Criteria/`, `Selection/`, version line | — |
| `docs/sprints/sprint_03/SPRINT_03.md` | S3-20 row: split into S3-20a / S3-20b | Decision 4 |

## 3. Self-review

Checklist of CLAUDE §6.7, plus `/engineering:code-review` on the diff.

| # | Area | Result |
|---|---|---|
| 1 | Security | No endpoint yet; error messages echo the sent keys, as filter-options does (JSON-encoded by the serializer). No new dependency |
| 2 | Correctness | Every rule of ARCHITECTURE §5.1 and C108 has a test row; the owner's example (decision 1) is a row of its own |
| 3 | N+1 / performance | Pure in-memory code over a handful of rules per criterion; key lists are small (≤ 8 known values per layer) |
| 4 | Edge cases | Empty, repeated and unknown keys; all values of a layer empty; unknown and excluded together (only `Invalid` reported); missing default (null); a criterion that does not apply (throws) |
| 5 | Mutation check | Weight order reversed in `PriorityResolver`: two CAT-14 tests fail; reverted |
| 6 | Architecture | All new types `internal`; no new project reference (ARC-01 – ARC-03 green) |
| 7 | a11y / UI texts | No UI change |

### 3.1 Review findings

| # | Finding | Sorting |
|---|---|---|
| R1 | Two domain comments still said resolution and matching „come with S3-20“ | **Fixed** |
| R2 | `Resolve_EveryCriterion_ExplainsItself` returns early for a requirement criterion that does not apply to a selection; every criterion applies to at least two of the five selections, so each is checked | **Won't fix:** the data covers every criterion |
| R3 | `CriteriaSelectionReader` and `FilterOptionsResolver` each have a small layer-name switch (words for messages vs. SPEC names for `excludedBy[].layer`) | **Won't fix:** different purposes |
| R4 | A rule weight of `int.MinValue` would rank like no weight | **Won't fix:** weights are small whole numbers from release folders |

No correctness or security finding affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (590) |
| 4 | Mutation check (§3 row 5) | ✅ two tests fail, reverted |
| 5 | `node scripts/check-invariants.mjs` | ✅ gate passed with TESTING_v35 |
| 6 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 7 | `reuse lint` (Docker image) | ✅ compliant |
| 8 | E2E | Not run locally: no client change; CI runs it |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | S3-20b: endpoint `GET /api/v1/catalog/criteria`, criteria content in `CatalogReleaseSnapshot`, allowed ratings and procedure texts, procedure criteria from test types, `appTypesWithoutCriteria[]`, 404 for an incomplete release, caching; contract parts of CAT-02, CAT-11 and the HTTP parts of CAT-12, CAT-18, CAT-19; CAT-06, CAT-16, CAT-20, CAT-21 → `active` in TESTING_v36 | S3-20b, next |
| P2 | SPEC §8.2 / §8.6 to record decisions 2, 3 and 5 (`productPart`, `allowedRatings` of `PROC-…`, use of `selection-excluded`) — with the endpoint | S3-20b |
| P3 | UX key for `selection-excluded` on the client (S2-11 P1): the configurator prevents excluded values, so the answer should not reach a user; a key is still needed for the mapping | UX copy session #181 / S10 |
| P4 | Kriterienkatalog K-4 (one product part per criterion) is still open; decision 2 covers several parts in the answer until it is decided | Project team |

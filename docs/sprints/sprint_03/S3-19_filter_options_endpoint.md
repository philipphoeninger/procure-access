# S3-19 — `GET /api/v1/catalog/filter-options`

> **Sprint:** 03, P0 task S3-19 (M2.4, M2.4d, 3 points). **Issues:** #153 (versioned routes, duplicate Swagger parameters), #176 (Swagger `DescribeApiVersions`, milestone M2). **Requirements / invariants:** SPEC_v2 §8.1 (C77), §8.6; **CAT-12** (catalog-endpoint part), **CAT-18** (endpoint part), **CAT-19** (endpoint part) → `active`, **CAT-09** HTTP part completed, in `TESTING_v34.md` (V101–V105); ARCHITECTURE_v22 §4.3, §7.1, §7.2, §9.6 (C107). CAT-05, CAT-07, CAT-10, CAT-23, API-01, ARC-01 – ARC-03, OPS-03 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: the filter-options endpoint per SPEC §8.1 in the Catalog module, pinned to the active release or a named one, inactive use cases not offered but still resolvable, exclusions release-pinned; versioned routes and the duplicate Swagger parameters (#153); endpoint style and #176 per O-S3-6. Not in scope: `GET /api/v1/catalog/criteria` with `appTypesWithoutCriteria[]` and the selection check (S3-20), any client screen (S10, sprint 04), an active release in the dev, E2E or staging stacks (S3-27).
> **Owner decisions** (2026-11-06, before the task, all as recommended): **O-S3-6** minimal APIs, `internal` in the module, mapped by the host on its versioned group; #176 done here, milestone M2. **#153** only the new module endpoints under `/api/v1`, legacy controllers stay at `/api/<controller>`; the URL segment is the only version reader. **Caching** `Cache-Control: public, max-age=3600, must-revalidate` with `release`, `public, no-cache` + `ETag` without; ARCHITECTURE §9.6 reworded. **Errors** unknown release 404 `not-found`; unknown key 400 `validation` keyed by parameter; an inactive use case is accepted as input but not listed. **Order** use cases by German name. **E2E** no active release in the E2E stack (open point). After the plan: **Q1** `appTypesWithoutCriteria[]` stays with `/criteria` (SPEC §8.2, S3-20), as SPEC §8.1 has it; **Q2** no active release and no `release` → 404 `not-found`; **Q3** a product part with `IsAlwaysSelected` counts as selected only when the request names it.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17` and `TESTING_v29`; the highest versions are v21 and v33 (CLAUDE §2) | Worked from v21 / v33, wrote v22 / v34 |
| F2 | The sprint row lists `appTypesWithoutCriteria[]` for filter-options; SPEC §8.1 has no such field, it belongs to `/criteria` (§8.2, CAT-20) | Asked before coding; Q1: SPEC wins, S3-20 |
| F3 | The prompt says „a given release id“; SPEC §8.1 names the release by its version (`release=2026.1`) | SPEC wins (CLAUDE §2): `release` is the version string |
| F4 | TESTING CAT-12 said the endpoint returns excluded values „with their reason(s)“; SPEC §8.1 / C77 send no reason text | CAT-12 reworded in v34 (V101) |
| F5 | ARCHITECTURE §9.6 asked for `max-age=3600` on every catalog answer; with CAT-09 that would hide an activation for up to an hour | Owner decision „caching“; §9.6 reworded (C107) |
| F6 | `BaseCrudController` declared `api/v{version}/[controller]`, but every derived controller declares its own route, so the versioned route never existed (#153). Seven version readers (URL, two query, two header, two media type) made Swagger list `api-version` and `v` twice | Dead route removed; URL segment only |
| F7 | `SwaggerExtensions` resolved `IApiVersionDescriptionProvider` from a scope with `AV0027` suppressed until the first versioned minimal API (#176); this endpoint is that API | Switched to `app.DescribeApiVersions()`; every `Map…` call moved before the Swagger set-up |
| F8 | Filter values are global (seeded), exclusion rules belong to a release (C104); `CatalogReleaseSnapshot` held only the criterion keys | Snapshot gets the release's rules; filter values get a cache entry of their own |
| F9 | The MiniCatalog has one exclusion per pair of layers, and `T.2` replaces HR → Video by HR → Audio | Table-driven CAT-12 tests and the CAT-19 test need no new fixture |
| F10 | First unit test expected one target per selected source; HR has two rules in `T.1` (Hardware, Video) | Test expects every target of the source |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/FilterOptions/FilterOptionsEndpoint.cs` (new, `internal`) | `MapGet("/filter-options")` with `release`, `useCase`, `appTypeIds[]`, `productPartIds[]`; 404 Problem Details for an unknown or missing active release, 400 validation problem for unknown keys; serialises the answer once, `ETag` = first 16 bytes of its SHA-256, 304 on a matching `If-None-Match`, `Cache-Control` per decision; OpenAPI metadata (summary, 200/304/400/404) | SPEC §8.1; decisions |
| `…/FilterOptions/FilterOptionsResolver.cs` (new, `internal`) | `ReadSelection` (empty values = not given, repeated values once, one error per unknown key as sent); `Resolve` (lists in order, inactive use cases left out, `excludedBy[]` from the rules whose source is selected, in layer and list order) — pure, so unit tests drive it | CAT-12, CAT-18, CAT-19 |
| `…/FilterOptions/FilterOptionsResponse.cs` (new, `internal`) | Response records in the shape of SPEC §8.1 | C77 |
| `…/Reading/CatalogReader.cs` | `GetReleaseByVersionAsync`; `CatalogReleaseSnapshot.Exclusions` (rules by layer and key, one more projection query per release, cached); `GetFilterValuesAsync` (four lists, cached without expiry under its own key type) | CAT-09, CAT-19 |
| `…/CatalogModule.cs` | `MapCatalogEndpoints(this IEndpointRouteBuilder)`: group `/catalog`, tag `Catalog` — the module's only new public member | O-S3-6; ARC-03 |
| `API/API.ProcureAccess/Program.cs` | All `Map…` calls before `ConfigureSwaggerExplorer()`; `app.NewVersionedApi().MapGroup("/api/v{version:apiVersion}").HasApiVersion(1, 0).MapCatalogEndpoints()` | #176; O-S3-6 |
| `…/ApiVersionSupport/ApiVersionConfiguration.cs` | `UrlSegmentApiVersionReader` only | #153 |
| `…/Controllers/Base/BaseCrudController.cs` | Dead `api/v{version:apiVersion}/[controller]` route removed | #153 |
| `…/Extensions/SwaggerExtensions.cs` | `app.DescribeApiVersions()`; `AV0027` suppression removed | #176 |
| `tests/ProcureAccess.Tests.Unit/Catalog/FilterOptionsResolverTests.cs` (new) | 11 test methods (the theory runs once per rule): the six rules of `T.1` cover every pair of layers; each rule's source excludes its targets and nothing else (theory); no selection; several excluding values in layer order; always-selected part only when sent; inactive use case not listed but accepted with its rules; list order (German names, sort order); fields; query reading | CAT-12, CAT-18 |
| `tests/ProcureAccess.Tests.Integration/Catalog/FilterOptionsEndpointTests.cs` (new) | 11 tests: JSON shape of SPEC §8.1 (contract); every seeded value without selection (contract); inactive use case; `release=T.1` vs `T.2` vs active (CAT-19); request after `catalog activate`, also with the old ETag (CAT-09); `max-age` and 304; `no-cache` for the active release; 404 for an unknown or no active release; 400 keyed as sent; no unversioned route | CAT-09, CAT-12, CAT-18, CAT-19 |
| `tests/ProcureAccess.Tests.Integration/Operations/SwaggerDocumentTests.cs` | No operation lists `api-version`/`v` or a parameter twice (replaces the `api-version` default assertion, #153); the filter-options operation with its four query parameters, tag and responses; the UI lists exactly the generated documents (#176, from the test of #71) | #153, #176 |
| `docs/TESTING_v34.md` (new) | V101 CAT-12, V102 CAT-18, V103 CAT-19 `active` with their deferred parts; V104 CAT-09 HTTP part; V105 §5.2 Swagger tests; input `ARCHITECTURE_v22.md` | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v22.md` (new) | C107; §4.3 Catalog row; §7.1 version reader; §7.2 filter-options as built; §9.6 caching for named and active releases | Decisions |
| `docs/sprints/sprint_03/SPRINT_03.md` | O-S3-6 decided | — |
| `CLAUDE.md` | §3 the endpoint, minimal-API pattern, versioning and Swagger; `AV0027` note removed; §4 `FilterOptions/`; §5 `curl` example | Prompt |

SPEC is not changed: the response shape of §8.1 is built as written, and the error and caching behaviour is recorded in ARCHITECTURE C107. SPEC §13 has no question this task decides. No UI, so neither `de.json` nor the Figma mapping changes. TECH_DEBT gets its next version at the end of the sprint (process step 14); #153 and #176 are closed by the pull request.

### 2.1 How to call it

```bash
curl -i 'http://localhost:5000/api/v1/catalog/filter-options?useCase=hr&appTypeIds[]=web&productPartIds[]=base'
curl -i 'http://localhost:5000/api/v1/catalog/filter-options?release=2026.1'
```

Until a release is imported and activated (S3-27) the first call answers 404 „No catalog release is active.“

## 3. Self-review

| # | Point | Result |
|---|---|---|
| 1 | Security | Public, read-only, anonymous by design (catalog, ARCHITECTURE §7.2); no antiforgery needed for GET; every input goes through EF parameters or set lookups; error messages echo the sent key or version into a JSON body only (see R1) |
| 2 | Correctness — exclusions | `excludedBy[]` comes only from the rules of the named or active release (CAT-19) and only from values the request names (Q3); a rule whose source is unknown would throw, but foreign keys rule that out |
| 3 | Correctness — caching | The ETag is a hash of the exact body, so it changes with release, selection and any seed change; the active release is revalidated on every use, so CAT-09 holds through HTTP. A 304 keeps `ETag` and `Cache-Control`; `UseStatusCodePages` leaves 304 alone (empty body checked) |
| 4 | Versioning | `/api/v1.0/…` works, `/api/v2/…` and `/api/vx/…` answer 404 `not-found` as Problem Details (checked with a throwaway probe test); legacy `/api/<controller>` unchanged (CAT-10, API-01 and every legacy test green); no client or E2E file sends `api-version` or `v` |
| 5 | Swagger | Mutation check: a v2 minimal API mapped after `ConfigureSwaggerExplorer()` makes `SwaggerUi_Urls_MatchTheGeneratedDocuments` fail; reverted |
| 6 | N+1 / queries | Per request one indexed query for the active id (or the id of the version); per release once two projection queries; filter values once per API instance (four queries) |
| 7 | Architecture rules | One new public member on `CatalogModule`; everything else `internal`; no new project, package or reference (`Asp.Versioning.Http` comes with the host's `Asp.Versioning.Mvc`, the module uses only the shared framework); ARC-01 – ARC-03 green |
| 8 | Licence | No new dependency |
| 9 | a11y | No UI |
| 10 | Review sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

Own pass over the diff with the code-review checklist (security, performance, correctness, maintainability). No correctness or security bug affecting current behaviour:

| # | Remark | Sorting |
|---|---|---|
| R1 | The 404 detail and the validation messages repeat the sent version or key | **Won't fix:** JSON only, length bounded by Kestrel's request-line limit, no HTML context; it helps the caller |
| R2 | Two first requests at the same moment may both load a release or the filter values (`GetOrCreateAsync` has no lock) | **Won't fix:** the loads are read-only and give equal results; same as S3-17 |
| R3 | The cached lists are `List<T>` behind `IReadOnlyList<T>` | **Won't fix:** internal types, no caller casts; same as S3-17 R4 |
| R4 | ARCHITECTURE §7.1 says list endpoints honour `Accept-Language` (de/en); filter values have German names only | **Follow-up** P2: only when English names exist |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings (no `AV0027` any more) |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (402) |
| 4 | Mutation check (§3 row 5) | ✅ the expected test fails, reverted |
| 5 | `node scripts/check-invariants.mjs` | ✅ gate passed with TESTING_v34 |
| 6 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 7 | `reuse lint` (Docker image) | ✅ compliant |
| 8 | E2E | Not run locally: no client change, and no stack has an active release; CI runs E2E on all five browser projects |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The dev, E2E and staging stacks have no active release, so the endpoint answers 404 there; the E2E stack needs one (test folders or release 2026.1) when S10 calls the endpoint (decision 6, S3-17 P4) | S10 (sprint 04) / S3-27 |
| P2 | `Accept-Language` (ARCHITECTURE §7.1) for catalog names: only German names exist (R4) | when English names exist |
| P3 | The other indicative catalog endpoints of §7.2 (`releases/active`, `app-types`, …) are not built; S10 may not need them, because filter-options carries every value | S3-20 / S10 |
| P4 | The legacy controllers keep their `[ApiVersion("1.0")]` attributes; they are harmless with the default version and go with their controllers | steps 5/6 |

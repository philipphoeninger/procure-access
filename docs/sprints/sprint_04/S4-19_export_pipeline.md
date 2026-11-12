# S4-19 — Export pipeline and `POST /api/v1/exports`

> **Sprint:** 04, P0 task S4-19 (M2.4; 4 points). **Issues:** none. **Requirements / invariants:** R-LST-12 (anonymous export, file format per SPEC §8.3); EXP-05, EXP-07, TEN-04 → `active`; the export part of CAT-12; the `AdHocSelectionSource` part of ARC-03 (V74); in passing the ad-hoc source part of CAT-16 (deferred to S4-19/S4-20 by V141).
> **Status:** done — pull request #277 open.
> **Scope:** the export pipeline in the Export module — `IExportSource` with `AdHocSelectionSource`, the file model, the formatter interface with a placeholder per format, `POST /api/v1/exports` — and `ICatalogSelectionResolver` in `Catalog.Contracts`; the "persists nothing" helper and a test logger; TESTING_v48, ARCHITECTURE_v37. Not here: the XLSX and CSV renderers and the short-description column (S4-20, O-S4-6), the export dialog S13, the rate limit and `ExportCreated` (S4-21), the persisted source (step 3).
> **Owner decisions (2026-11-12):** **O-S4-57 (a)** — the request names every criterion of the selection exactly once; a missing, repeated or foreign one is a 400 `validation`. **O-S4-58 (a)** — the weight range is a server setting `Configurator:PriorityWeight` (1–10 🔧), a weight only while B applies. **O-S4-59 (a)** — a hand-written test logger instead of `Serilog.Sinks.InMemory`. **O-S4-60 (a)** — only the active release is exported; a stored, inactive one is 400 `release-mismatch` (`criteria.export.outdated`), an unknown one 404. **TEN-04 reworded** to ARCHITECTURE §9.6 (`no-cache` for the active release). Plan approved without changes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The Export module was an empty skeleton that already references `Catalog.Contracts` and `Configurator.Contracts`; `Catalog.Contracts` held only three enums | A first contract interface in `Catalog.Contracts`, implemented by Catalog |
| F2 | The criteria check and the priority resolution live inside Catalog (`CriteriaSelectionReader`, `CriteriaResolver`), and `CriteriaResolver.Resolve` produced the JSON records of the endpoint directly | `ResolveCriteria` yields typed results; the endpoint and the new resolver both map them, so export and S11 share one resolution path |
| F3 | ARCHITECTURE §7.2 gives the body but not what a criterion left out of it means | O-S4-57 |
| F4 | The weight range 1–10 existed only as the client setting `priorityWeight` | O-S4-58 |
| F5 | SPEC §8.6 maps `release-mismatch` to `criteria.export.outdated` („inzwischen gilt eine neue Katalogversion“) — it contradicted the planned default of exporting any stored release | O-S4-60 |
| F6 | TESTING TEN-04 asked for `max-age=3600` on every catalog answer; ARCHITECTURE §9.6 and the code (S3-19) give `no-cache` for the active release | TEN-04 reworded (owner) |
| F7 | TESTING §3 named `Serilog.Sinks.InMemory` for log assertions, not a dependency; the Serilog PostgreSQL sink writes Warning and above in the background | O-S4-59; EXP-05 searches `logging` instead of counting it |
| F8 | The test server (`WebApplicationFactory`) has no `IHttpMaxRequestBodySizeFeature`, so a body-size limit from endpoint metadata does nothing there | The test checks the metadata; 413 checked by hand on Kestrel (C7) |

## 2. Changes

| File | Change | Requirement / decision |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog.Contracts/ICatalogSelectionResolver.cs` (new) | `ICatalogSelectionResolver.ResolveAsync(releaseVersion, CatalogSelectionQuery)` and its records: outcome, release summary (version, active, criterion count), selection names, resolved criteria (revision, key, kind, number, titles, original title, product part name, recommended priority, test-step link) | ARCHITECTURE §4.4; ARC-03 |
| `src/ProcureAccess.Modules.Catalog/Criteria/CatalogSelectionResolver.cs` (new), `CatalogModule.cs` | The implementation on `CatalogReader`, `CriteriaSelectionReader` and `CriteriaResolver.ResolveCriteria`; registered scoped | EXP-07; CAT-02 |
| `src/ProcureAccess.Modules.Catalog/Criteria/CriteriaResolver.cs` | `ResolveCriteria` (typed `ResolvedCriterion`) split out of `Resolve`; the endpoint answer is unchanged (its tests pass unchanged) | F2 |
| `src/ProcureAccess.Modules.Export/Requests/ExportRequest.cs` (new) | Body records, every member nullable, unknown members refused | EXP-07 |
| `src/ProcureAccess.Modules.Export/Sources/` (new) | `IExportSource<TInput>`, `ExportSourceResult`/`ExportProblem`, `PriorityWeightRange` (🔧 `Configurator:PriorityWeight`, start fails on an impossible range), `AdHocSelectionSource` (request check, re-resolution, rows of the included criteria in list order; weight rule of O-S4-27) | EXP-07; O-S4-57, O-S4-58, O-S4-60 |
| `src/ProcureAccess.Modules.Export/Documents/ExportContent.cs` (new) | `ExportContent`, `ExportRow` (recommended and chosen level, weight that applies, rationale, note, test-step link), `ExportDocument` | SPEC §8.3 |
| `src/ProcureAccess.Modules.Export/Formatting/` (new) | `ExportFormat`, `IExportFormatter`, `StubExportFormatter` (plain text under the format's content type, until S4-20), `ExportFileWriter` (UTC date, file name of SPEC §8.3) | R-LST-12 |
| `src/ProcureAccess.Modules.Export/Http/ExportEndpoint.cs` (new), `ExportModule.cs`, `.csproj` | `POST /api/v1/exports`, anonymous, `RequestSizeLimit` 1 MB, `Cache-Control: no-store`, format error merged with the other field errors; `AddExportModule(configuration)`, `MapExportEndpoints()`; `InternalsVisibleTo` for the three test projects | R-LST-12; TEN-04 |
| `API/API.ProcureAccess/Program.cs`, `appsettings.json` | Registration with the configuration, mapping after the Configurator endpoints; `Configurator:PriorityWeight` Min 1, Max 10 | O-S4-58 |
| `tests/ProcureAccess.Tests.Unit/Export/AdHocSelectionSourceTests.cs` (new) | 22 cases on a catalog double: priorities from the catalog, choices next to them, order, every request rule, 404, `release-mismatch`, selection keys, sentinel never repeated, the weight range setting | EXP-07, CAT-12, CAT-16 |
| `tests/ProcureAccess.Tests.Integration/Export/` (new) | `ExportTestCatalog` (imports `T.1`/`T.2`, builds bodies from the criteria answer); `ExportEndpointTests` (file, content type, file name; resolved level refused; foreign revision; keys by body path; `release-mismatch`; 404; the six rules of `T.1`; body-size metadata; unreadable body); `AnonymousExportPersistsNothingTests` (EXP-05) | EXP-05, EXP-07, CAT-12 |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/DatabaseRowCounts.cs`, `CapturingLoggerProvider.cs` (new) | The "persists nothing" helper (every table through `information_schema`) and the test logger (every level, message, values, exception) | TESTING §9 step 2; O-S4-59 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogSelectionResolverTests.cs` (new) | The resolver agrees with `GET /api/v1/catalog/criteria` field by field and names, reports an inactive release, `ReleaseNotFound`, excluded and invalid selections | CAT-02, CAT-12 |
| `tests/ProcureAccess.Tests.Integration/Api/CacheHeaderTests.cs` (new) | Both catalog endpoints (named and active release) and every export answer | TEN-04 |
| `tests/ProcureAccess.Tests.Architecture/Modules/` | `ModuleRules.TypeUsingAny` (one type and its nested types); the `AdHocSelectionSource` rule; its self-test on the probe types | ARC-03 (V74) |
| `docs/TESTING_v48.md` (v46 deleted) | V143–V148: EXP-05, EXP-07, TEN-04 `active`; CAT-12 export part, CAT-16 ad-hoc source part; ARC-03 part written; §3 log assertions | — |
| `docs/ARCHITECTURE_v37.md` (v35 deleted) | C123; §4.3 rows of Catalog and Export, §4.4 Flow A1, §7.2 | — |
| `docs/sprints/sprint_04/SPRINT_04.md`, `CLAUDE.md` | Owner decisions O-S4-57 – O-S4-60 and the TEN-04 wording; CLAUDE §3, §4, §5 | — |

SPEC unchanged: R-LST-12, §7.2, §8.3 and §8.6 already say what was built; the request contract is ARCHITECTURE §7.2.

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases) over the diff against `main`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | Swagger listed the file answer as `application/octet-stream` | **Fix** | Lists the XLSX and CSV content types |
| R2 | An unused `using` in `ExportModule.cs` | **Fix** | Removed |
| R3 | Kestrel's 413 carries no `Cache-Control: no-store` (C7): it is written before the handler and outside the exception handler | **Won't fix** | It holds no user data, and a cache does not store a POST answer without freshness information (RFC 9111 §3); recorded in TESTING V145 and C123 |
| R4 | A 413 is a Problem Details with the RFC default `type`, not one of SPEC §8.6 | **Follow-up** | Platform's behaviour for every non-400 bad request since S2-10; S13 (S4-21) shows its generic export error for it — P3 |
| R5 | A 1 MB body of empty entries gives one field error per entry, so the error answer can be large | **Won't fix** | Bounded by the body cap; the per-entry check is linear; the rate limit of S4-21 bounds repeats |
| R6 | `releaseVersion` and an unknown member's name are echoed in the error answer (and the latter in the exception log) | **Won't fix** | JSON, no markup; neither is a note (EXP-05) |
| R7 | With an invalid format and a 404, `release-mismatch` or `selection-excluded` from the source, the format error is not reported | **Won't fix** | The other problem decides first; the format comes from a radio group of S13 |

Checked without finding: two small queries per request (release id, active id), the release content and filter values come from the cache — no N+1; nothing is written (EXP-05 proves it on all tables); no body or note is logged at any level, also for refused and unreadable bodies (a mutation that logs a note at Debug fails the test, C3); the server never takes a level from the client; module boundaries hold (ARC-01 – ARC-03); no new dependency, no UI text, no client change.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 720 of 720 pass; after the CAT-16 test the unit project again: 447 of 447; after R1 Swagger, export and cache-header tests: 25 of 25 |
| C3 | Mutation: the endpoint logs the first note at Debug | `AnonymousExportPersistsNothingTests` fails on the sentinel; reverted |
| C4 | `check-invariants.mjs` (TESTING_v48), `check-figma-links.mjs`, `check-i18n.mjs` | Exit 0 each |
| C5 | `reuse lint` | Compliant, 1198 / 1198 files |
| C6 | Client, E2E | No client or E2E change; left to CI (the dev stack was running) |
| C7 | By hand on Kestrel: this branch's API on port 5099 (`Development`, `RebuildDatabase=false`) against a throwaway PostgreSQL container, `T.1` seeded | A body of 1.2 MB → 413 Problem Details (no `Cache-Control`, R3); a valid CSV export → 200, `text/csv; charset=utf-8`, `Cache-Control: no-store`, `Content-Disposition: attachment; filename=ProcureAccess_Kriterien_T.1_2026-11-12.csv`; an unreadable body → 400 with `no-cache,no-store`. Both stopped afterwards; the running dev stack was not touched |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | The file body is a placeholder; the short-description column waits for O-S4-6 | S4-20 |
| P2 | No rate limit and no `ExportCreated` yet; the endpoint is not called by the client | S4-21 |
| P3 | 413 answers with the RFC default problem type (R4); S13 must treat it like any other failed export | S4-21 (S13 error state) |
| P4 | After `release-mismatch`, S13 has to load the criteria of the active release (drop the pinned release) — the client's part of O-S4-60 | S4-21 |
| P5 | The file name date is the UTC day (as for the usage events); between 0:00 and 1:00/2:00 German time it is the day before | Owner, if it matters before the pilot |
| P6 | EXP-05's file-store part waits for the file store; TEN-04's workspace and directory parts for steps 3 and 6 | With those steps |
| P7 | `Configurator:PriorityWeight` (server) and `priorityWeight` (client) are kept in step by hand | Step 3 may serve the range to the client |

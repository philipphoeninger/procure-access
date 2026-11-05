# S3-06 — Swagger UI and OpenAPI documents only in Development

> **Sprint:** 03, P0 task S3-06 (debt, 0.5 points). **Issues:** #183 (`TECH_DEBT_v2.md` TD-18; S2-21 P1). **Requirements / invariants:** none activated; SEC-06 stays green. No new TESTING version. **Status:** done by Claude Code (2026-11-05), pull request open.
> **Scope:** exactly the sprint row: `ConfigureSwaggerExplorer()` serves the Swagger UI and the OpenAPI documents in `Development` only, and a test shows the behaviour per environment. Not in scope: the duplicate `api-version`/`v` parameters (#153), AV0027 (#176), the deployment's environment settings (P1).
> **Owner decisions** (asked before the start, 2026-11-05):
> 1. **O-S3-2: `Development` only.** Every other environment, staging included (it runs as `Production`: the repository has only `appsettings.Development.json` and `appsettings.Production.json`), answers 404 on `/swagger`. If the API is ever meant for third parties, the decision is revisited; until then the OpenAPI JSON can be exported from a `Development` instance as a file.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `ConfigureSwaggerExplorer()` (`API/API.ProcureAccess/Extensions/SwaggerExtensions.cs`) calls `UseSwagger()` and `UseSwaggerUI()` without an environment check; `Program.cs` calls it in every environment | The check goes into `ConfigureSwaggerExplorer()` itself, so `Program.cs` stays unchanged |
| F2 | `AddSwaggerExplorer()` registers `AddEndpointsApiExplorer()` and SwaggerGen; the API explorer is also used by API versioning (`IApiVersionDescriptionProvider`) | Registration stays in every environment; only the middleware is gated (R1) |
| F3 | The only Swagger integration test, `SwaggerDocumentTests`, read the document through the shared fixture, which runs in `Testing` | With the change it would get 404; it needs its own API in `Development` |
| F4 | `Development` turns `RebuildDatabase` on in `appsettings.Development.json` (SEC-06) | Every test of the class starts its API on a database of its own and drops it afterwards, as `DatabaseRebuildGuardTests` does |
| F5 | nginx of the client image forwards only `/api/`, so `/swagger` was never on the client origin; the E2E stack runs the API in `Development` and does not use Swagger; no script, workflow or E2E test reads `/swagger` | No other file depends on the old behaviour |
| F6 | SPEC, ARCHITECTURE and TESTING do not mention Swagger | Nothing to change there; CLAUDE §3/§5 and the compose comment name it |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `API/API.ProcureAccess/Extensions/SwaggerExtensions.cs` | `ConfigureSwaggerExplorer()` returns at once unless `app.Environment.IsDevelopment()`; XML comment names O-S3-2 and #183 | O-S3-2 |
| `tests/ProcureAccess.Tests.Integration/Operations/SwaggerDocumentTests.cs` | No longer on the shared fixture: each test starts its own `ProcureAccessApiFactory` in the environment it needs, on a new database that `DisposeAsync` drops. New theory `Swagger_OutsideDevelopment_AnswersNotFound` (`Testing`, `Staging`, `Production`: `/swagger/index.html` and `/swagger/v1/swagger.json` answer 404); new control `Swagger_InDevelopment_ServesUiAndDocument` (both 200, `text/html` and `application/json`); `SwaggerDocument_V1_CarriesXmlCommentsAndParameterDefaults` (#73, #74) runs in `Development` unchanged | Shows the behaviour per environment; the control keeps the theory from passing on a wrong path |
| `CLAUDE.md` | Header; §3 (Swagger only in `Development`, next to the API image); §5 (Swagger URL and its condition) | DoD |
| `docker-compose.yml` | Comment of the API port: "Swagger (Development only)" | The stack falls back to `Production` when `.env` sets no environment |

Not changed on purpose: `TECH_DEBT_v2.md` still lists TD-18; the register is versioned and v3 at the end of the sprint moves it to "Done" (P2). `SPRINT_03.md` keeps O-S3-2 in its decision table; the decision is recorded here.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff. N+1 and a11y: not applicable (no query, no UI). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | SwaggerGen and the API explorer stay registered outside `Development`. Without the middleware nothing serves a document; removing the registration would also need a check that API versioning does not lose its explorer | Won't fix: no endpoint reaches it, and the smaller change is enough for O-S3-2 |
| R2 | Security: the guard reads `IHostEnvironment`, as `DatabaseRebuildExtensions` does for SEC-06. Both depend on `ASPNETCORE_ENVIRONMENT`, which the compose stack takes from `.env`; `.env.example` sets `Development`, so a server set up by copying it unchanged would serve Swagger and allow the rebuild | Follow-up: P1 (deployment, outside this row; not new with this change) |
| R3 | Edge cases: `/swagger` without `index.html` (the UI's redirect) also answers 404 outside `Development`, because no Swagger middleware runs; the 404 is the Problem Details answer of `UseStatusCodePages` (S2-10) | – |
| R4 | The class starts five API instances, each on a new database with all migrations; about 13 s for the class locally | Won't fix: same pattern as `DatabaseRebuildGuardTests`; a shared database would need a rebuild guard the test cannot rely on |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet test --solution ProcureAccess.sln --filter-class "*SwaggerDocumentTests"` | ✅ 5/5; with `main`'s `SwaggerExtensions.cs` the theory fails in all three environments (3/5), the two `Development` tests pass |
| 2 | `dotnet test --solution ProcureAccess.sln` (Docker running) | ✅ 146/146, including `DatabaseRebuildGuardTests` (SEC-06) |
| 3 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; no new warning (AV0027 in `SwaggerExtensions.cs` stays by decision, #176, and only moved down two lines) |
| 4 | `dotnet format whitespace` / `style` on the solution, `dotnet format` on the integration test project, each `--verify-no-changes` | ✅ |
| 5 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v26.md`) |
| 6 | `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending; 0 errors) |
| 7 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant (`docs/**` is covered in `REUSE.toml`) |
| 8 | Vitest, ESLint, E2E | – no client or E2E code touched; the E2E stack runs the API in `Development`, where nothing changes |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The Swagger guard and SEC-06 rely on `ASPNETCORE_ENVIRONMENT`. `.env.example` sets `Development` for the local stack; the deployment (Dokploy, not in the repository) must set `Production` or `Staging`, or leave it unset (the compose default is `Production`). Worth a line in the deployment documentation when it is written | Owner / deployment documentation (TECHNICAL or handbook) |
| P2 | TD-18 (#183) is closed by this PR; the register records it in its next version | Sprint 03 close-out (TECH_DEBT_v3) |
| P3 | Revisit O-S3-2 when the API is meant for third parties (then e.g. a configuration switch or a published OpenAPI file) | Owner, with the public API |

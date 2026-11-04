# S2-21 — API Dockerfile hardening and Swagger clean-up

> **Sprint:** 02, stretch task S2-21 (1 point; TECH_DEBT §9.2 "M1.2 / ops"). **Issues:** #77, #74, #73. **Requirements / invariants:** SEC-06 stays green; no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** the API image builds in Release, runs as the base image's non-root user and restores from the project files alone (#77); the generated `API.ProcureAccess.xml` is no longer tracked and the unused `Microsoft.AspNetCore.OpenApi` is gone (#74); `SwaggerDefaultValues` no longer throws on missing entries and no longer skips cases silently (#73 part 2). Not in scope: pinning base images by digest (#77 "Not in scope"), a container healthcheck, Swagger in Production (§5 P1).
> **Owner decisions 2026-11-04 (before the task, asked by Claude):** **#73** only part 2 (harden `SwaggerDefaultValues`); AV0027 stays as it is and moves to a follow-up issue for the step that adds the first versioned minimal API (created: #176); #73 is closed. **#74** `Microsoft.OpenApi` resolves to Swashbuckle's version 2.7.5, without a direct reference; check it for known vulnerabilities and note it in the csproj.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The image ran `dotnet publish -c Debug`, as root (`uid=0`), with `ENV ASPNETCORE_URLS=http://+:8080`, which logged "Overriding HTTP_PORTS '8080' … Binding to values defined by URLS instead" on every start | Release, `USER $APP_UID`, variable removed (the base image sets `ASPNETCORE_HTTP_PORTS=8080`) |
| F2 | All sources were copied before four `dotnet restore` runs, so any source change re-ran every restore; `NuGet.config` and `src/Directory.Build.props` were not copied before the restore | One restore from `NuGet.config`, both `Directory.Build.props` and the 20 project files; sources after it |
| F3 | The local Docker (29.9.2) has no buildx plugin, so `docker compose build` uses the classic builder; BuildKit-only syntax such as `COPY --parents **/*.csproj` is not available there | Each project file has its own `COPY` line; a forgotten one stops the build (checked, §3) |
| F4 | `<DocumentationFile>API.ProcureAccess.xml</DocumentationFile>` wrote the file into the source tree; without it `GenerateDocumentationFile` writes it to the build output, where Swagger reads it (`AppContext.BaseDirectory`), and publish includes it | Property removed, file untracked, two `REUSE.toml` entries removed |
| F5 | Without `Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi` drops from 2.12.2 to 2.7.5. GitHub's advisory database has one advisory for the package: GHSA-v5pm-xwqc-g5wc / CVE-2026-49451 (high; circular schema references can end parsing), affected `>= 2.0.0-preview.11, <= 2.7.4`, **first patched 2.7.5**. The API only writes documents; it parses none | Decision #74 as given; note in the csproj |
| F6 | `SwaggerDefaultValues` used `Responses![key]` and `.First(...)`, which throw on a missing entry, and silently skipped reference parameters and schemas. The history of PR #71 (S1-04, commits f7cb236, f7a427e, 0d771d3) has a version that was reviewed twice and kept the Swagger document byte-identical | That version is taken over unchanged |
| F7 | Swagger document `v1` of `main` saved from the dev stack before the change (83,888 bytes) | Compared after the change, §4 |

## 2. Changes

| File | Change |
|---|---|
| `API/API.ProcureAccess/Dockerfile` | Restore layer from `NuGet.config`, `Directory.Build.props`, `src/Directory.Build.props` and every `*.csproj` of `API`, `DAL`, `SERVICES` and `src/`; one `dotnet restore` of the API project; sources copied afterwards; `dotnet publish -c Release --no-restore`; runtime stage without `ASPNETCORE_URLS`, `USER $APP_UID` (the published files stay owned by root, so the app user can read but not change them) |
| `API/API.ProcureAccess/API.ProcureAccess.csproj` | `DocumentationFile` and `Microsoft.AspNetCore.OpenApi` removed; comment on `Microsoft.OpenApi` 2.7.5 (decision, advisory) |
| `API/API.ProcureAccess/API.ProcureAccess.xml` | No longer tracked (generated into `bin/` and the publish output) |
| `REUSE.toml` | Both entries of the XML file removed |
| `API/API.ProcureAccess/Swagger/SwaggerDefaultValues.cs` | Version of #71: `TryGetValue` for responses, `FirstOrDefault` for parameter descriptions, content keys copied before removing; an undescribed parameter, a parameter reference and a schema reference are left unchanged and logged as a warning once per host; cached `JsonSerializerOptions` |
| `tests/ProcureAccess.Tests.Unit/Swagger/SwaggerDefaultValuesTests.cs` | New: six unit tests (missing response, foreign content types, default and required flag, undescribed parameter with one warning per filter, parameter reference, schema reference). Four of them fail on the filter of `main` |
| `tests/ProcureAccess.Tests.Unit/ProcureAccess.Tests.Unit.csproj` | References `API` (the filter lives there; no I/O, so a unit test per TESTING §3.4) |
| `tests/ProcureAccess.Tests.Integration/Operations/SwaggerDocumentTests.cs` | New: `/swagger/v1/swagger.json` of the test API carries the XML summary, a parameter description and the `api-version` default |
| `CLAUDE.md` | §3: state of the API image; §5: a new .NET project needs its `COPY` line in the API Dockerfile; version line |
| GitHub | #176 created (AV0027 with the first versioned minimal API, contents of #73 part 1) |

## 3. Self-review

Checklist of CLAUDE §6.7 (with *Engineering/code-review*):

| Area | Result |
|---|---|
| Security | The container runs as `app` (`uid=1654`) instead of root; it cannot write the application files. Nothing in the API writes to the file system except ASP.NET Data Protection, whose keys go to `/home/app/.aspnet/DataProtection-Keys` (writable; before: `/root/.aspnet/…`, same "may not be persisted" warning, §5 P3). No secret enters the image (`.dockerignore` unchanged). One package less in the image |
| Correctness | Migrations apply as `app`: the E2E stack starts on an empty in-memory database and reached `healthy`; the dev database holds 7 legacy and 1 Platform migration and 127 criteria. A missing `COPY` line was probed (Workspace.Contracts left out): the build stops with `NETSDK1004 Assets file … not found`, as the Dockerfile comment says. `--no-restore` with `-c Release` is safe: no package or project reference depends on the configuration |
| Swagger | Document `v1` byte-identical before and after (§4); the filter logs no warning for today's API |
| N+1, performance | Not affected. Restore layer cached after a source change (checked: step `RUN dotnet restore` "Using cache") |
| Accessibility | No UI change |

Findings, sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | 20 hand-written `COPY` lines must follow every new project | *Won't fix*: `COPY --parents` needs BuildKit, which the local classic builder lacks (F3); the list changes only with ARCHITECTURE §4.3, a forgotten line fails loudly, and CLAUDE §5 names the rule |
| R2 | The content-type unit test also passes on the old filter | *Won't fix*: .NET's `Dictionary` allows removing during enumeration, so the old code was not wrong there; the test guards the behaviour |
| R3 | Swagger is served in every environment, Production included; every operation lists the `api-version` and `v` parameters twice; Data Protection keys live only in the container | *Follow-up*, outside this row: §5 P1–P3 |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln` | 0 errors, 112 warnings (#118; AV0027 stays by decision; this change adds none) |
| `dotnet test --solution ProcureAccess.sln` | 142 passed (unit 19, architecture, integration 64 incl. SEC-06 and `SwaggerDocumentTests`) |
| `dotnet format` whitespace + style (solution), analyzers (unit and integration tests) | clean |
| `docker compose up --build` (dev stack) | `/health/ready` 200 `Healthy`; client `/` 200 and `/api/CriteriaFilters` through nginx 200; `docker exec … id` → `uid=1654(app)`; no URLS override line in the log |
| Swagger | `/swagger/index.html` 200; `/swagger/v1/swagger.json` byte-identical to `main` (83,888 bytes) |
| Restore cache | After a change in `Program.cs` the restore step comes from the cache, only publish runs again |
| `npm run stack:up` (E2E, empty in-memory database) | all containers `healthy`; API as `app` |
| `npx playwright test` chromium, firefox, mobile-chrome | 117 passed; WebKit and mobile Safari only in CI |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors |
| `reuse lint` | compliant (663 / 663) |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | Swagger UI and documents are served in every environment, Production included (`ConfigureSwaggerExplorer()` has no environment check). Owner decision whether that is wanted for the public API; proposed issue | Deployment hardening or the step that rebuilds the API surface |
| P2 | Every operation lists `api-version` (query) and `v` twice in the Swagger document; already so on `main`, probably related to #153 (versioned routes) | With #153 |
| P3 | Data Protection keys are stored only in the container and are lost on every new container. No effect today (JWT); the cookie session and antiforgery of step 3 depend on them | Migration step 3 |
| P4 | #176 has no milestone yet: the step of the first versioned minimal API is not fixed in ARCHITECTURE | Owner, when the endpoints of M2 are planned |
| P5 | `TECH_DEBT_v1.md` still lists #77, #74 and #73 as open; #176 is new | Sprint close-out (process step 14, `TECH_DEBT_v2.md`) |

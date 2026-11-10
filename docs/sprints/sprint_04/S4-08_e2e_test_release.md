# S4-08 — An active test release in the E2E stack (and in development)

> **Sprint:** 04, P0 task S4-08 (debt, 1 point). **Issues:** #241 (TD-35, from S1-18 P4, S3-14 P6, S3-17 P4, S3-19 P1, S3-20b P1, S3-27 P6). **Requirements / invariants:** CAT-23 unchanged; no invariant changes status; prepares the E2E parts of CFG-* and EXP-*. New versions `TESTING_v41.md` (V121, §6.3 and §3.3 text) and `ARCHITECTURE_v29.md` (C115, §9.7 and §10.2 text). **Status:** done by Claude Code (2026-11-10), pull request open, not merged.
> **Scope:** exactly the sprint row: the catalog part of the E2E seeder of TESTING §6.3 imports and activates the test folder `T.1` in the E2E stack and in development, so `/api/v1/catalog/*` answers 200 there; an E2E smoke test shows it. The users and products of the seeder, the switch between S10 and the legacy filter (S4-12) and the `E2E` environment itself are not part of it.
> **Owner decisions (2026-11-10):** **O-S4-3 (a) + (b)** — S10 only where a release is active, the legacy filter elsewhere (S4-12); `T.1` also active in Development, staging stays on the legacy filter. **Seeder (A)** — a startup seeder in the API host, limited by environment like the `RebuildDatabase` guard (SEC-06), the test folder mounted read-only, never in the image. **Environment (i)** — the E2E stack keeps running as `Development`; the seeder runs in `Development` and `E2E`, never elsewhere. **Dev stack (x)** — a new `docker-compose.dev.yml` mounts the folder; staging (deployed from `docker-compose.yml` alone) never gets it. Default confirmed with the plan: the seeder imports `T.1` only if it is not stored and activates it only if no release is active.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No stack has an active release: dev, E2E and staging answer 404 on `/api/v1/catalog/filter-options` and `/criteria` (#241). 2026.1 cannot be activated before O-1 and must not be imported into a kept database before then | A test release for dev and E2E only |
| F2 | `catalog import` and `catalog activate` exist (S3-16, S3-17). Activating the active release is a no-op; importing a stored version is refused (exit 1) | The seeder checks what is stored before it imports, so it can run on every start |
| F3 | `T.1` lives in `tests/ProcureAccess.Tests.Shared/Fixtures/Releases/` and can be activated (one `PROC-…` per seeded test type since S3-17). The API image copies only `catalog/releases/` | The folder reaches a container only through a compose mount |
| F4 | The E2E stack runs as `Development` (`.env.example`), with an in-memory database; `RebuildDatabase` clears only users, not the catalog | The seeder runs after `RebuildDatabase`; nothing clears the release between starts of one stack |
| F5 | Staging is deployed from `docker-compose.yml`; whether its `ASPNETCORE_ENVIRONMENT` is right is unchecked (#236, TD-28) | The mount must not be in `docker-compose.yml` (decision (x)) |
| F6 | The integration host (`ProcureAccessApiFactory`) runs as `Testing`, but several tests start hosts in `Development` (SEC-06, Swagger), which load `appsettings.Development.json` | The factory turns the seeder off by default, as it does `RebuildDatabase` |
| F7 | T.2 cannot be imported before T.1: its `supersedes` keys must be criteria of the previous release | The "another release is active" test uses a copy of T.1 named `T.0` (as `CatalogActivationTests` does for T.3) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Releases/TestReleaseSeeder.cs` (new, `internal`) | Imports the version with `CatalogReleaseImporter` if it is not stored (missing folder → warning, nothing written; refused import → `InvalidOperationException` with every error), then activates it with `CatalogReleaseActivator` if no release is active (refused activation → exception) | F2, decision default |
| `src/ProcureAccess.Modules.Catalog/CatalogModule.cs` | `SeedTestReleaseAsync(services, releasesPath, version)`: runs the seeder in its own scope. The registration class is the module's only public type (ARC-03) | Host access without a new public type |
| `API/API.ProcureAccess/Extensions/TestCatalogSeedExtensions.cs` (new) | `Catalog:TestRelease:Path`, `Catalog:TestRelease:Version`; `IsTestCatalogSeedAllowed` = `Development` or `E2E` on `IHostEnvironment`; outside them a configured release only logs a warning. Relative paths start at the working directory, like `Catalog:ReleasesPath` | O-S4-3 (b), environment (i) |
| `API/API.ProcureAccess/Program.cs` | Calls `SeedTestCatalogIfAllowedAsync()` after the migrations and `RebuildDatabaseIfAllowedAsync()`; a thrown error stops the start through the existing catch | Seeder (A) |
| `API/API.ProcureAccess/appsettings.Development.json` | `Catalog:TestRelease` = `../../tests/ProcureAccess.Tests.Shared/Fixtures/Releases`, `T.1`, for `dotnet run` from `API/API.ProcureAccess`. In a container the path points nowhere (warning only) | O-S4-3 (b) |
| `docker-compose.e2e.yml` | Mounts the test folders read-only at `/app/catalog/test-releases` and sets both settings; header comment updated | #241 |
| `docker-compose.dev.yml` (new) | The same for the dev stack: `docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build` | Decision (x) |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/ProcureAccessApiFactory.cs` | Sets `Catalog:TestRelease:Path` to empty; tests of the seeder override it | F6 |
| `tests/ProcureAccess.Tests.Integration/Catalog/TestReleaseSeederTests.cs` (new) | Empty catalog → T.1 imported and active; second run changes nothing; another active release (T.0) stays active and T.1 is stored inactive; stored but inactive T.1 is activated; missing folder writes nothing; a folder the import refuses throws and writes nothing | F2, F7 |
| `tests/ProcureAccess.Tests.Integration/Operations/TestCatalogSeedGuardTests.cs` (new) | Starts the API on a database of its own with both settings: nothing seeded in `Testing`, `Staging`, `Production`; T.1 active in `Development` and `E2E` (the control, as in `DatabaseRebuildGuardTests`) | Never in Staging or Production |
| `e2e/smoke/catalog.spec.ts` (new) | Through the client's nginx: `filter-options` answers 200 with release `T.1` and use cases; `criteria` for `hr` / `web` / `base` / `softwareTest` answers 200 with `T.1` and `PROC-softwareTest` | Sprint row; #241 "done when" |
| `docs/TESTING_v41.md` (new; `v39` deleted) | V121: §6.3 "built so far" (catalog part), §3.3 state note; header names `ARCHITECTURE_v29.md` | CLAUDE §2 (two newest versions) |
| `docs/ARCHITECTURE_v29.md` (new; `v27` deleted) | C115: §9.7 (the test release follows the same rule as `RebuildDatabase`), §10.2 (Development with the dev override); header names `TESTING_v41.md` | – |
| `CLAUDE.md` | Header entry; §3: test catalog release; the filter-options paragraph no longer says "no environment has an active release"; §4: `docker-compose.e2e.yml`, `docker-compose.dev.yml`; §5: the dev command with the override, the `curl` note, `npm run stack:up` | CLAUDE §11 |

SPEC, the Figma mapping, the UX copy, `de.json`, `LEGACY_EXCEPTIONS` and `.github/workflows/ci.yml` are not touched (the E2E jobs already run `npm run stack:up` with both compose files). TD-35 is closed by this pull request; `TECH_DEBT` records it in its next version at the sprint close-out. ROADMAP D15 still says that no release is active outside the tests; it is updated at the close-out too.

### 2.1 Using it

```bash
# Dev stack with the active test release (Development only):
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
curl 'http://localhost:5000/api/v1/catalog/filter-options'          # 200, "release":"T.1"
# After a change to the T.1 folder the stored T.1 is the old one (a stored release never changes):
docker compose -f docker-compose.yml -f docker-compose.dev.yml down -v
```

The E2E stack (`npm run stack:up` in `e2e/`) does it on every start; its database is in memory.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | Two independent conditions: the environment (`Development`/`E2E` on `IHostEnvironment`, tested in five environments) and a folder that only the dev and E2E overrides mount. A staging host started as `Development` by mistake (#236) would resolve the `appsettings.Development.json` path to `/tests/…`, which does not exist in the image: warning, nothing written. The version comes from configuration only; the importer refuses a path-like version |
| Correctness | Every combination of stored/active is covered by `TestReleaseSeederTests`; on the real E2E stack a restart of the API container found T.1 stored and active, wrote nothing and served T.1 again (C6) |
| Edge cases | A dev database with T.1 that later gets 2026.1 imported: both share the five `PROC-…` criteria by key; kind, test type and category are equal in both folders, and 2026.1 replaces nothing, so the import accepts it (checked on the files, R3). Activation of 2026.1 by hand deactivates T.1, and the seeder then leaves it inactive |
| Performance | One query (`AnyAsync` on the version) and one (`AnyAsync` on `IsActive`) per start once T.1 is stored |
| a11y, UI texts | Not touched |
| Licence | No new dependency; SPDX headers on all new files |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | A folder the import refuses stops the start; in the dev stack `restart: on-failure` then restarts the API in a loop | Won't fix: a loud failure is the point (the E2E stack must not start without its data), and the T.1 folder is validated by `ReleaseFolderReaderTests` in CI |
| R2 | The API-only smoke test runs in all five browser projects (about 50 ms each) | Won't fix: same as `smoke/stack.spec.ts`; a project filter would be a new mechanism for no gain |
| R3 | A kept dev database mixes the test release and, later, real releases (shared `PROC-…` criteria rows; T.1 is the "previous release" of the first real import) | No change today (no conflict, see Edge cases); P2 |
| R4 | The seeder logs its success at `Information`, which the console sink drops (minimum `Warning`) | Won't fix: as the migrations; the endpoints show the result |

No correctness or security bug that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | 0 warnings, 0 errors |
| C2 | `dotnet test --solution ProcureAccess.sln` (Testcontainers) | 664 of 664 pass, the 11 new ones included |
| C3 | `dotnet format ProcureAccess.sln --verify-no-changes` | Exit 0 |
| C4 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` and the five `node --test scripts/*.test.mjs` | Exit 0 each |
| C5 | `npm run typecheck && npm run lint` in `e2e/` | Exit 0 |
| C6 | E2E stack locally (`npm run stack:up`), `npx playwright test --project=chromium --project=firefox --project=mobile-chrome` | 147 of 147 pass, the two new smoke tests included; after `docker restart` of the API container `/health/ready` 200 and filter-options `"release":"T.1"` again. WebKit runs in CI only |
| C7 | `docker compose … config` with both overrides | Both resolve the mount (`read_only: true`) and the two settings |
| C8 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1079 / 1079 files |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | The users (tenants with known test credentials) and the products of the E2E seeder (TESTING §6.3), and the `E2E` environment itself (§3.3), are not built | With the steps that need them: users with step 3 (sign-in journeys), products with step 6; the `E2E` environment with Mailpit / the clock endpoint (steps 3–4). `TestCatalogSeedExtensions` already accepts `E2E` |
| P2 | A kept dev database holds T.1 next to real releases (R3). When 2026.1 gets its priorities, a developer who wants to check it on the dev stack may prefer a database without T.1 | Document then (start without `docker-compose.dev.yml` on a fresh volume); no code |
| P3 | ROADMAP_v4 D15 and TECH_DEBT_v3 TD-35 still describe "no active release outside the tests" | Sprint-04 close-out (new versions) |
| P4 | Staging still answers 404 on `/api/v1/catalog/*` by design (O-S4-3); S4-12 switches the start page and header by release availability | S4-12 |

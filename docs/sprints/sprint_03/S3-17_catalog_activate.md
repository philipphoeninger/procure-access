# S3-17 — CLI `catalog activate`, active release and cache

> **Sprint:** 03, P0 task S3-17 (M2.2, 2 points). **Issues:** none. **Requirements / invariants:** SPEC_v2 R-ADM-01, SD20; **CAT-23** (activate part) and **CAT-09** (read part) → `active` in `TESTING_v33.md` (V98–V100); ARCHITECTURE_v21 §5.1 „Activation“, §9.6, §10.2 (C106). OPS-03, ARC-01 – ARC-03, CAT-05, CAT-07, CAT-10 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: `catalog activate <version>` switches the active release, the next catalog read returns it, and the catalog cache is keyed by `CatalogReleaseId`. Also the completeness rules that S3-15 P2 and S3-16 P1 left for `activate`, and one active release at a time (S3-12 P4). Not in scope: the catalog endpoints and their HTTP caching (S3-19), an activation step in the dev, E2E or staging stacks (no real release before S3-27), release 2026.1 (S3-27).
> **Owner decisions** (2026-11-06, before the task, all as recommended): **1a** a release needs a procedure criterion for **every seeded test type** (ARCHITECTURE C30), so the MiniCatalog gets the three it lacked. **1b** every criterion, procedure criteria included, needs its own **catalog-default priority** (rule 4); app-type rules do not replace it. **2** which release is active is **not cached**: it is read from the database on every call; release content is cached by `CatalogReleaseId` without expiry. **3** the read side is built now as an internal reader and tested through the API's services; the HTTP part of CAT-09 goes to S3-19. **4** at most one active release (partial unique index); activating an older release is allowed (rollback); activating the active release exits 0 and writes nothing; an unknown or incomplete release exits 1 with every gap; no `catalog deactivate`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `TESTING_v29` and `ARCHITECTURE_v17`; the highest versions are v32 and v20 (CLAUDE §2) | Worked from v32 / v20, wrote v33 / v21 |
| F2 | The prompt depends on S3-16 being merged; its pull request #213 was still open, and its first CI run failed in the Firefox and WebKit E2E jobs: „Install Playwright system dependencies“ timed out after 10 minutes, no test ran | Failed jobs rerun as instructed (WebKit timed out once more and passed on the second rerun); #213 is green but not merged from this session (the merge was not allowed), so this branch is based on the S3-16 branch and the pull request showed the S3-16 commit until #213 was merged. **Update 2026-11-06:** the owner merged #213 (`bfc9617`, 08:19 UTC) before this pull request #214 (`89f8cd3`, 08:26 UTC) |
| F3 | `CatalogRelease.Activate`/`Deactivate` existed, and `CatalogImmutabilityInterceptor` already allowed `IsActive` to change, but nothing kept two releases from being active (S3-12 P4) | Partial unique index (decision 4) |
| F4 | ARCHITECTURE §9.6 said the cache is „invalidated when a release is published“, but `catalog activate` runs in another process than the API and cannot reach its memory cache | Decision 2; §9.6 reworded (C106) |
| F5 | There is no catalog endpoint yet (S3-19, O-S3-6 open), so „the next catalog request“ has nothing to call | Decision 3 |
| F6 | The MiniCatalog had procedure criteria for two of the five seeded test types, so under decision 1a neither test release could be activated | Three procedure criteria added to the C# fixture and both test folders |
| F7 | My first refactor of `CatalogCommandLine` read `Catalog:ReleasesPath` from the scope's provider instead of the provider passed in; the S3-16 test that overrides the setting caught it | Read from the passed provider again, before the scope, as in S3-16 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Releases/CatalogReleaseActivator.cs` (new) | `ActivateAsync(version)`: unknown version → error; already active → no write; otherwise every gap (criterion without catalog-default priority, seeded test type without procedure criterion) → error; else, in one transaction inside the execution strategy, deactivate the active release and save, then activate the target and save (the unique index is checked per row). `CatalogActivationResult` holds the active and the previous version or the errors | R-ADM-01; decisions 1, 4 |
| `…/Reading/CatalogReader.cs` (new) | `GetActiveReleaseAsync` reads the active id from the database on every call; `GetReleaseAsync(id)` returns a `CatalogReleaseSnapshot` (id, version, `PublishedAt`, criterion keys in list order) from `IMemoryCache` under a private key type per `CatalogReleaseId`, without expiry | CAT-09; decisions 2, 3 |
| `…/CatalogModule.cs` | `AddMemoryCache()` and the scoped `CatalogReader`; comments name `activate` | D10; decision 3 |
| `…/Commands/CatalogCommandLine.cs` | `catalog activate <version>` next to `import`; shared usage, pending-migration check and exit codes; messages `Activated release … ; active before: …`, `… is already active; nothing changed.`, `… was not activated; the database is unchanged.` with every error, and for an incomplete release the hint to import a fixed folder as a new version | R-ADM-01 |
| `…/Persistence/Configurations/CatalogReleaseConfiguration.cs`, `Migrations/20261106014851_AddSingleActiveReleaseIndex.cs` (new, `internal`) | Unique index `IX_CatalogReleases_SingleActive` on `IsActive` with the filter `"IsActive"` | Decision 4; OPS-03 |
| `API/API.ProcureAccess/Program.cs`, `Extensions/CatalogCommandHost.cs` | Comments name both commands | — |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogActivationTests.cs` (new) | 7 tests: switch and rollback with the messages; the active release again; an incomplete release (T.1 without the default of 99.3 and without `PROC-bikApp`) names both gaps and leaves T.1 active; an unknown version; the index refuses two active releases; after each activation the next read returns the new release; the same release is read from the cache (same object), another release is another entry | CAT-23, CAT-09 |
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/CatalogCommandLineTests.cs` | `catalog activate <v>` is a command; `activate` without or with two versions and `deactivate` are usage errors; the usage names both commands | — |
| `tests/ProcureAccess.Tests.Shared/Fixtures/MiniCatalogValues.cs`, `MiniCatalog.cs`, `Releases/T.1`, `T.2` (`PROC-pdfTest.json`, `PROC-bikWeb.json`, `PROC-bikApp.json`, new) | All five test types; three procedure criteria (default B, no mapping, no texts) in both releases, shared: T.1 15, T.2 14 criteria, ten shared | Decision 1a |
| `tests/ProcureAccess.Tests.Unit/Catalog/MiniCatalogTests.cs`, `tests/ProcureAccess.Tests.Integration/Catalog/MiniCatalogPersistenceTests.cs`, `CatalogImportTests.cs` | New counts; new case „every criterion has a catalog default“ in `MiniCatalogTests` | — |
| `docs/TESTING_v33.md` (new) | V98 CAT-23 activate part `active`, V99 CAT-09 `active` (read part; HTTP part S3-19), V100 §6.2 MiniCatalog; input `ARCHITECTURE_v21.md` | CLAUDE §6.4 |
| `docs/ARCHITECTURE_v21.md` (new) | C106; §5.1 „Activation“ paragraph; §9.6 cache without invalidation; §10.2 the activate step | F4 |
| `CLAUDE.md` | §3 activation and cache, §4 `Reading/` and the activator, §5 both commands | Prompt |

SPEC §13 and the Figma mapping are not touched: no open question is decided and no UI changes.

### 2.1 How to run it

```bash
# local, from API/API.ProcureAccess (after the API has started once and applied the migrations)
export ConnectionStrings__ProcureAccess="Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres;Password=<from .env>"
export Catalog__ReleasesPath=../../catalog/releases
dotnet run -- catalog import 2026.1
dotnet run -- catalog activate 2026.1

# compose stack
docker compose run --rm api catalog activate 2026.1
```

Exit code 0: `Activated release 2026.1; active before: <version or none>.` or `Release 2026.1 is already active; nothing changed.` Exit code 1: `Release … was not activated; the database is unchanged. <n> error(s):` with one line per gap. Exit code 2: usage. The running API serves the new release from its next request; no restart.

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | The version is only a query parameter; no file is read. No endpoint; the command runs only from a shell on the host or in the container |
| 2 | Correctness — one active release | Deactivate and activate run in one transaction with two saves; the partial unique index refuses a second active release also around the command (test). A retry of the execution strategy clears the change tracker and reloads |
| 3 | Correctness — completeness | Checked before the transaction: a stored release and the seeded test types change only through import and migrations. Mutation check: the gap check turned off fails the incomplete-release test |
| 4 | Cache | Content per release id without expiry is safe because releases never change (CAT-05); the active id is never cached (decision 2). Respawn deletes rows without resetting the id sequences, so the fixture-wide cache never serves a release of another test. Mutation check: an entry removed before each read fails the cache test |
| 5 | N+1 / queries | Activation: 1 lookup, 2 projection queries for the gaps, then 2 loads and 2 updates. Read: 1 indexed query for the active id, plus 1 projection query per release the first time |
| 6 | Architecture rules | No new public member, project, package or reference; `IMemoryCache` is part of the ASP.NET Core shared framework the module already references (D10, no new dependency); ARC-01 – ARC-03 green; the migration is `internal` |
| 7 | Licence | No new dependency; the new JSON files are covered by `REUSE.toml` (`**/*.json`) |
| 8 | Review sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

Own pass over the diff with the code-review checklist (security, performance, correctness, maintainability). No correctness or security bug affecting current behaviour:

| # | Remark | Sorting |
|---|---|---|
| R1 | The host comments in `Program.cs` and `CatalogCommandHost.cs` named only `catalog import` | **Fixed** |
| R2 | Another process activates the target between the check and the transaction | **Won't fix:** the transaction then loads it as both current and target (identity resolution), and the result is „already active“, which is true |
| R3 | Two operators activate two releases at the same moment: the second transaction fails on the unique index and the host prints its generic „The catalog command failed: …“ with exit code 1; the database is unchanged | **Follow-up** P1, only if it ever confuses someone |
| R4 | `CatalogReleaseSnapshot.CriterionKeys` is a `List<T>` behind `IReadOnlyList<T>`; a caller could cast it and change the cached object | **Won't fix:** internal type, no caller does; S3-19 shapes the snapshot |
| R5 | Cache entries have no `Size`; if `AddMemoryCache` ever gets a `SizeLimit`, every entry must set one | **Follow-up** P2 (D10 note) |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (373) |
| 4 | Mutation checks (§3 rows 3, 4) | ✅ the two expected tests fail, reverted |
| 5 | Smoke test of the real command on a throwaway PostgreSQL 17 (Platform and Catalog migrated with `dotnet ef database update`, `ASPNETCORE_ENVIRONMENT=Production`, the test release folders): `activate T.1` before import → exit 1; `import T.1` → 15 criteria; `activate T.1` → „active before: none“; again → „already active“; `import T.2` → „4 new revision(s), 10 shared with T.1“; `activate T.2` → „active before: T.1“; `activate T.1` → rollback; `activate` → usage, exit 2; the table then holds T.1 active, T.2 inactive | ✅ |
| 6 | `node scripts/check-invariants.mjs` | ✅ gate passed (CAT-09 2, CAT-23 10 tests) |
| 7 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 8 | `reuse lint` (Docker image) | ✅ compliant |
| 9 | E2E | Not run locally: no client change; CI runs E2E on all five browser projects |
| 10 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Concurrent activations end with the host's generic error message (R3); a specific message would need to catch the unique violation | follow-up if it confuses someone |
| P2 | `IMemoryCache` has no size limit; whoever adds one (D10, scale-out) must give the catalog entries a size (R5) | with D10 |
| P3 | CAT-09 HTTP part: the catalog endpoints read through `CatalogReader`, extend `CatalogReleaseSnapshot` with what they return, and a test shows that the next request after `catalog activate` returns the new release; ETag / `Cache-Control` of §9.6 | S3-19 |
| P4 | The dev, E2E and staging stacks have no active release until a real one exists (O-1); S3-19 decides how E2E gets one (MiniCatalog folders or the E2E seeder of TESTING §6.3) | S3-19 / S3-27 |
| P5 | ~~Merge #213 (S3-16) before this pull request~~ — **done 2026-11-06**: #213 merged before #214 (F2). Still open: the remote branches of S3-14 and S3-15 are still on GitHub after their merges (S3-16 P6), now also those of S3-16 and S3-17 | owner |

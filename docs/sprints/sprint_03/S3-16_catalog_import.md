# S3-16 — CLI `catalog import`

> **Sprint:** 03, P0 task S3-16 (M2.2, 3 points). **Issues:** none. **Requirements / invariants:** SPEC_v2 R-ADM-01, SD20; **CAT-23** (import part), **CAT-07** → `active`, **CAT-05** import part, all in `TESTING_v32.md` (V94–V97); ARCHITECTURE_v20 §5.1 „Import“, §10.2 (C105). OPS-03, ARC-01 – ARC-03, CAT-10 stay green. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: `catalog import <version>` validates the whole release folder (S3-15) and imports it atomically as an immutable release; unchanged criteria share their revision with the previous release. Not in scope: `catalog activate`, the active release and the catalog cache (S3-17), completeness rules (S3-17), release 2026.1 (S3-27), a `--dry-run` or a CI check of release folders (P5).
> **Owner decisions** (2026-11-06, before the task, all as recommended): **1** the CLI is a command mode of the API host, no new project; the public entry is a method of `CatalogModule`. **2** the API image copies `catalog/releases/`, the setting `Catalog:ReleasesPath` points to it. **3** importing a stored version is refused. **4** `Criterion.MarkSupersededBy` sets a successor once, during the import; each `supersedes` key must be a criterion of the previous release. **5** the URL becomes part of the unique index of `StandardReference`, so a corrected URL is a new row. **6** a `SaveChangesInterceptor` refuses changes and deletes of stored catalog rows. **Defaults** stated before the task and not objected to: no new dependency (arguments parsed by hand); the command never migrates and refuses while catalog migrations are pending; CAT-07 compares with the revision in the previous release; a stored criterion's kind, test type and category must match; exclusion rules are reused, with a unique index; importing `T.1` then `T.2` gives exactly `MiniCatalog.Build`; one TESTING version with CAT-23 (import part), CAT-05 (import part), CAT-07 and §6.2; CLAUDE §5 names the commands.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `TESTING_v29`; the highest version is v31 (CLAUDE §2) | Worked from v31, wrote v32 |
| F2 | The previous task's pull request #212 (S3-15) had one failed check: the Firefox E2E job timed out after 10 minutes in „Install Playwright system dependencies“, not in a test | Failed jobs rerun as instructed, all green, merged; this branch is based on the merge |
| F3 | `Criterion` had no way to set `SupersededByKey` after `Create` (S3-14 P3, S3-15 P1) | `MarkSupersededBy` (decision 4) |
| F4 | `StandardReference` was unique per standard and clause, which contradicts „a corrected URL is a new row“ (S3-15 P9) | Index with the URL (decision 5) |
| F5 | `FilterExclusion` had no unique index (S3-13 P1), and the domain says „a changed reason is a new rule“ | Unique per source, target **and reason**, so a reworded rule is a new row and an unchanged one is reused |
| F6 | `catalog/releases/` did not exist yet; `COPY` needs it | `catalog/releases/.gitkeep` (outside every version folder, so the validator never sees it; covered by the `catalog/releases/**` block of `REUSE.toml`) |
| F7 | The integration fixture resets the database per test class; a release can be imported once | `CatalogImportTests` resets before every test |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `API/API.ProcureAccess/Program.cs` | `catalog …` arguments run `CatalogCommandHost` and return its exit code; otherwise the API starts as before (`return 0` after `app.Run()`) | Decision 1 |
| `API/API.ProcureAccess/Extensions/CatalogCommandHost.cs` (new) | Builds a `WebApplication` builder (same configuration and environment variables as the API) with only Platform and Catalog, runs `CatalogModule.RunCommandAsync`, logs an unexpected exception and returns 1. No web server, no migration, no hosted service | Decision 1 |
| `API/API.ProcureAccess/appsettings.json` | `Catalog:ReleasesPath` = `catalog/releases` (relative to the working directory, `/app` in the image) | Decision 2 |
| `API/API.ProcureAccess/Dockerfile` | Runtime stage copies `catalog/releases/` to `/app/catalog/releases` | Decision 2 |
| `catalog/releases/.gitkeep` (new) | The folder exists before S3-27 adds 2026.1 | F6 |
| `src/ProcureAccess.Modules.Catalog/CatalogModule.cs` | `IsCommand(args)`, `RunCommandAsync(services, args, output, error)` — the module's only new public members | Decision 1; ARC-03 |
| `…/Commands/CatalogCommandLine.cs` (new) | `catalog import <version>`; usage → 2; missing setting or pending catalog migrations → 1; prints every error with file and JSON path, or a summary (criteria, new criteria, new and shared revisions) | R-ADM-01 |
| `…/Releases/CatalogReleaseImporter.cs` (new) | Refuses a path as version; loads the seeded filter values; reads and validates the folder (S3-15); refuses a stored version; loads the previous release (stored last) with everything it holds, the stored criteria of the folder's keys, their last revision numbers, the standard references of the cited clauses and all exclusion rules; builds the release and collects every error (kind / test type / category of a stored criterion, `supersedes` key not in the previous release, a second successor); only without errors: `Publish(now)`, one `SaveChanges` | CAT-23, CAT-07; decisions 3, 4 |
| `…/Releases/RevisionContent.cs` (new) | `Matches(definition, revision)`: title, plain language, rationale, sort order, and the four lists as sets | CAT-07 |
| `…/Domain/Criterion.cs` | `MarkSupersededBy(key)`: empty → key; same key again → nothing; another key → `PublishedCatalogChangeException`; its own key → `ArgumentException` | Decision 4 |
| `…/Domain/StandardReference.cs` | Comment: one row per standard, clause and URL | Decision 5 |
| `…/Persistence/CatalogImmutabilityInterceptor.cs` (new) | Refuses `Deleted` entries and `Modified` entries except `CatalogRelease.IsActive` and `Criterion.SupersededByKey` from null | Decision 6; CAT-05 |
| `…/Persistence/CatalogDbContext.cs` | Adds the interceptor in `OnConfiguring` (every instance: application, tools, tests) | Decision 6 |
| `…/Persistence/Configurations/StandardReferenceConfiguration.cs`, `FilterExclusionConfiguration.cs` | Unique `(Standard, Clause, Url)` and `IX_FilterExclusions_Rule` over the six value columns and the reason, both `NULLS NOT DISTINCT` | Decision 5; F5 |
| `…/Persistence/Migrations/20261106012117_AddImportUniqueIndexes.cs` (new, `internal`) | The two indexes; drops the old reference index and the foreign-key index on `SourceUseCaseId`, which the rule index now leads with | OPS-03 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogImportTests.cs` (new) | 9 tests: T.1 + T.2 equal the MiniCatalog (incl. successors, `PublishedAt`, inactive); T.2 adds 4 revisions and shares 7, 1 reference, 7 rules; invalid folder, successor without predecessor, stored version and changed category leave every catalog table unchanged; corrected URL → new reference of a new revision; the guard; the command's exit codes and messages | CAT-23, CAT-07, CAT-05 |
| `tests/ProcureAccess.Tests.Unit/Catalog/CriterionSuccessorTests.cs`, `Releases/RevisionContentTests.cs`, `Releases/CatalogCommandLineTests.cs` (new) | `MarkSupersededBy`; `Matches` for equal content, other order and each changed field; `IsCommand` and the usage errors | Decisions 1, 4; CAT-07 |
| `tests/ProcureAccess.Tests.Shared/Fixtures/ReleaseDefinitions.cs` (new) | `Of(release, previous)` (moved from `ReleaseFolderReaderTests`), `Json`, `SortedJson` (a loaded release has no order) | Shared by unit and integration tests |
| `tests/…/ReleaseFolderReaderTests.cs`, `Fixtures/MiniCatalog.cs`, `Fixtures/TestReleaseFolders.cs` | Use the shared helper; comments on `MarkSupersededBy` and the import test | — |
| `docs/TESTING_v32.md` (new) | V94 CAT-23 import part `active` (activate part S3-17), V95 CAT-07 `active`, V96 CAT-05 import part, V97 §6.2 MiniCatalog as built; input `ARCHITECTURE_v20.md` | CLAUDE §6.4; S3-14 P1, S3-15 P4 |
| `docs/ARCHITECTURE_v20.md` (new) | C105; §5.1 „Import“ paragraph, key example after S3-15 decision 1a; §10.2 how a release reaches an environment | S3-15 P3 |
| `CLAUDE.md` | §3 catalog import, §4 layout (`Commands/`, interceptor, `catalog/releases/`, command host), §5 commands | Prompt |

### 2.1 How to run it

```bash
# local, from API/API.ProcureAccess (after the API has started once and applied the migrations)
export ConnectionStrings__ProcureAccess="Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres;Password=<from .env>"
export Catalog__ReleasesPath=../../catalog/releases
dotnet run -- catalog import 2026.1

# compose stack: the folders ship with the API image (rebuild it after adding one)
docker compose run --rm api catalog import 2026.1
```

Exit code 0: `Imported release 2026.1 (inactive): <n> criteria, <n> new; <n> new revision(s), <n> shared with <previous>.` Exit code 1: `Release … was not imported; the database is unchanged.` and every error as `<file> <JSON path>: <message>`. Exit code 2: usage.

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | The version is a folder name: `.`, `..` and any `/` or `\` are refused before a path is built; the folder is read only below `Catalog:ReleasesPath`. No endpoint; the command runs only from a shell on the host or in the container. The test-only raw SQL uses table names from the EF model |
| 2 | Correctness — atomicity | Every check runs before the write; on an error the change tracker is cleared and nothing is saved; the release is stored by one `SaveChanges` (one PostgreSQL transaction). A concurrent second import of another version would fail on the unique indexes (revision number, criterion key) and write nothing |
| 3 | Correctness — sharing | A revision is shared only with the previous release and only when all content is equal; lists compare as sets, so reordering a file creates no revision. Mutation check: `Matches` always false fails both CAT-07 integration tests |
| 4 | Immutability | Domain rules unchanged; the interceptor adds the EF side. Mutation check: interceptor turned off fails the CAT-05 test |
| 5 | N+1 / queries | Fixed number of queries per import (filter values, previous release as split query, criteria, revision numbers, references, rules), independent of the number of criteria |
| 6 | Architecture rules | Only `CatalogModule` methods are new public members; no new project, package or reference; ARC-01 – ARC-03 green; the migration is `internal` |
| 7 | Licence | No new dependency; `catalog/releases/**` already annotated in `REUSE.toml` |
| 8 | Independent review (agent) sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

My own pass and the review agent (code-reviewer, read-only, against CLAUDE.md, the importer, `RevisionContent`, the interceptor, `MarkSupersededBy`, the command line and host, and the index configurations) found no correctness or security bug at the reporting bar. Sorted per CLAUDE §6.8:

| # | Remark | Sorting |
|---|---|---|
| R1 | (agent, low confidence) Two imports of different versions started at the same moment can pick the same previous release; if neither creates a revision or criterion, both succeed, and a `SupersededByKey` could be written twice | **Won't fix:** every catalog row carries `xmin` as concurrency token, so the second save of the same criterion fails with a concurrency error and writes nothing; one maintainer runs the command by hand |
| R2 | (agent, low confidence) „Stored last“ is ordered by `PublishedAt`, then `Id`; a clock that goes backwards between two imports picks another release | **Won't fix:** the times come from `TimeProvider` on one host; the rule itself is P2 |
| R3 | (own) The unused `using …Domain` in `ReleaseFolderReaderTests` after moving the helper | **Fixed** |
| R4 | (own) A registration error before the host is built ends without exit code 1 and a one-line message | **Follow-up:** P4 (the exit code is non-zero either way) |
| R5 | (own) The generated name of the rule index was truncated by PostgreSQL's 63-character limit (`…SourceProd~`) | **Fixed:** named `IX_FilterExclusions_Rule` |

One review round is enough: no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ (after `dotnet format` fixed the indentation of the index expression) |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (362) |
| 4 | Mutation checks (§3 rows 3, 4) | ✅ 3 tests fail as expected, reverted |
| 5 | Smoke test of the real command on a throwaway PostgreSQL 17 (Platform and Catalog migrated with `dotnet ef database update`): `T.2` first → 3 errors, exit 1; `T.1` → exit 0; `T.2` → „4 new revision(s), 7 shared with T.1“, exit 0; `T.2` again → exit 1; no version → exit 2; `../T.1` → exit 1. No log noise on the console in Production | ✅ |
| 6 | API image built; `docker run … catalog import 2026.1` → „/app/catalog/releases/2026.1 $: The release folder does not exist.“, exit 1 | ✅ (Npgsql prints „Cannot load library libgssapi_krb5.so.2“ on its first connection in the image; that happens without this change too, P7) |
| 7 | `node scripts/check-invariants.mjs` | ✅ gate passed (CAT-05 14, CAT-07 5, CAT-23 5 tests) |
| 8 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 9 | `reuse lint` (Docker image) | ✅ compliant |
| 10 | E2E | Not run locally: no client change; `Program.cs` is exercised by every integration test and the image smoke test; CI runs E2E on all five browser projects |
| 11 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | CAT-23 activate part, CAT-09 and the active release; `catalog activate` refuses an incomplete release (S3-15 P2). Its command goes next to `import` in `CatalogCommandLine` | S3-17 |
| P2 | „Previous release“ is the release stored last. If an older version is imported after a newer one (a hotfix release), it compares with and supersedes against the newer one. Fine for the expected one release at a time; a version order would be a new rule | owner, if it ever happens |
| P3 | A database failure during the save is not provoked in a test: one `SaveChanges` is one transaction, and the checks before it are tested | — |
| P4 | Registration errors before the host is built (e.g. a missing connection string) end the command with the runtime's exception and a non-zero exit code, not with exit code 1 and a one-line message | follow-up if it confuses someone |
| P5 | A `--dry-run` (validate and compare without writing) or a CI step that validates every folder under `catalog/releases/` would let the project team see errors in the release pull request itself | proposed issue, owner |
| P6 | The remote branch `sprint03/S3-15-release-data-format` is still on GitHub after the merge of #212 (deleting it was not allowed from this session); so is `sprint03/S3-14-mini-catalog` (S3-15 P8) | owner |
| P7 | Npgsql logs „Cannot load library libgssapi_krb5.so.2“ in the API image (no Kerberos library in `aspnet:10.0`); harmless, as no GSSAPI authentication is used | follow-up issue, optional |

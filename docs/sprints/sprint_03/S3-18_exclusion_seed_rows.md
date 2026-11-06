# S3-18 — Seed rows 2 and 3 dropped, J1 updated

> **Sprint:** 03, P0 task S3-18 (M2.1, 1 point). **Issues:** none. **Requirements / invariants:** ARCHITECTURE_v21 §16.3.2 (C29, C103), §5.1, §12 step 2; **CAT-10** stays green (API and E2E part, expected sets unchanged), **OPS-03** stays green; J1 green. No invariant changes, so no new TESTING version. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: a legacy migration of `ApplicationDBContext` removes the seed exclusions 17 → 12 and 17 → 13 (HR-Anwendung → Mobile Anwendung (offen) / (geschlossen)), and J1 no longer expects the two mobile app types to be excluded. Per O-S3-5 (C) nothing is copied into the catalog; the legacy endpoints keep reading the legacy tables.
> **Owner decisions:** none new for the task. O-S3-5 (2026-11-06, S3-11 §2.9) applies. Before the task (2026-11-06, both as recommended): **1** the S3-17 protocol update (F2, P5: #213 merged) is the first commit of this pull request, not its own pull request; **2** J1 also checks that both mobile app types stay **selectable** after HR-Anwendung, so it fails if the rows ever come back.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17`; the highest version is v21 (CLAUDE §2). v21 already describes the result (§5.1, §12 step 2, §16.3.2, C103) | Worked from v21; no new ARCHITECTURE version, because nothing in it changes |
| F2 | The two rows are the second and third entries of `CriteriaFilterExclusionSeed` (`DAL/MODELS.ProcureAccess/SeedData/`), applied through `HasData` in `CriteriaFilterExclusionConfiguration`. „Id 2 and 3“ comes from the migration `UpdateAssociationEntitiesNotUsingBase`, where the table still had an `Id` column (`{ 2, 17, 12 }`, `{ 3, 17, 13 }`) before the composite key | Matches C103 |
| F3 | The client disables only the exclusions of the selected filters (`unavailableOptionIds` in `filters-selection.ts`), in one direction; nothing else in the client, the backend or the tests names the two rows (`LegacyCatalogReadTests` builds its own rows) | Only the seed, the migration and J1 change |
| F4 | The CAT-10 fixture contains no exclusions, and the filter endpoint does not read them (S1-15 F7, P2) | CAT-10 expected sets unchanged, as required |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `DAL/MODELS.ProcureAccess/SeedData/CriteriaFilterExclusionSeed.cs` | The entries 17 → 12 and 17 → 13 removed; the comment names §16.3.2 and the migration | §16.3.2 |
| `DAL/DAL.ProcureAccess/Migrations/20261106083424_RemoveHrMobileExclusions.cs` (+ `.Designer.cs`, model snapshot; new, generated) | `Up` deletes the two rows with `DeleteData`, `Down` inserts them again; a summary instead of `<inheritdoc />`, as in `HandOverPlatformTables` | OPS-03 |
| `e2e/journeys/j1-configurator.spec.ts` | The two mobile app types moved from `EXCLUDED_BY_PRODUCT_TYPE` to the new `NOT_EXCLUDED_BY_PRODUCT_TYPE`, which the step „choose the product type“ expects to be enabled | Prompt; owner decision 2 |
| `docs/sprints/sprint_03/S3-17_catalog_activate.md` | F2 and P5: #213 was merged before #214 (separate first commit) | Owner decision 1 |
| `CLAUDE.md` | §3 one sentence on the legacy seed, change log in the header | Prompt |

SPEC §13, TESTING and the Figma mapping are not touched: no open question is decided, no invariant changes, no UI changes.

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | Data migration with fixed keys; no endpoint, no input |
| 2 | Correctness — existing databases | `DeleteData` deletes by the composite key; a database where a row is already missing is not an error (the `DELETE` affects 0 rows). `Down` restores both rows |
| 3 | Correctness — the check in J1 | Before the change, J1 expected both options to be disabled after HR-Anwendung and passed on the old seed; the new `toBeEnabled` check on the same locators therefore fails as long as the rows exist |
| 4 | N+1 / queries | No query changes; two deletes once at startup |
| 5 | Architecture rules | Legacy context only; no module, project, package or reference touched; ARC-01 – ARC-03 green |
| 6 | a11y | No UI change; J1 keeps axe on every state in both appearances (A11Y-01, A11Y-08) |
| 7 | Licence | No new dependency; migrations are covered by `REUSE.toml` |
| 8 | Review sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

Pass over the diff with `/engineering:code-review` (security, performance, correctness, maintainability). It found no correctness or security bug that affects current behaviour:

| # | Remark | Sorting |
|---|---|---|
| R1 | No backend test checks the seeded exclusions of the legacy tables; only J1 does, end to end | **Won't fix:** the legacy tables go with steps 5/6, and CAT-12 tests the exclusions of the new model (S3-19); J1 covers the behaviour the visitor sees |

One review round is enough (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings, 0 errors |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet ef migrations has-pending-model-changes … --context ApplicationDBContext` (OPS-03) | ✅ no changes since the last migration |
| 4 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers; CAT-10 API part) | ✅ all passed (373) |
| 5 | `npm run typecheck` (e2e) | ✅ |
| 6 | E2E stack (fresh database, all migrations) + `npx playwright test --project=chromium --project=firefox --project=mobile-chrome` (J1, CAT-10 E2E part) | ✅ 141 passed; WebKit and mobile Safari only in CI |
| 7 | `node scripts/check-invariants.mjs` | ✅ gate passed |
| 8 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 9 | `reuse lint` (Docker image) | ✅ compliant |
| 10 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Kriterienkatalog §3.1 (German document) still names `SampleData.cs` for the two rows; they were in `CriteriaFilterExclusionSeed.cs` (S3-11 P5). After this pull request §3.1 can simply say the rows are removed | Owner, next Kriterienkatalog version |
| P2 | The remote branches of merged pull requests (S3-14 – S3-17, now also this one after its merge) are still on GitHub (S3-17 P5) | Owner |

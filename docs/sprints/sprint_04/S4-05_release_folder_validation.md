# S4-05 — Validate every catalog release folder in CI

> **Sprint:** 04, P0 task S4-05 (debt, 0.5 points). **Issues:** #239 (TD-33, from S3-16 P5). **Requirements / invariants:** CAT-23 unchanged (wording and status); the new tests carry `INV:CAT-23`; no new TESTING version. **Status:** done by Claude Code (2026-11-10), pull request #257 open, not merged.
> **Scope:** exactly the sprint row: every folder under `catalog/releases/` is validated in CI with the validator of `catalog import`, and a broken folder fails with the validator's message (file and JSON path). No change to the validator, the import, the CLI or the CI workflow.
> **Owner decisions:** **O-S4-2 (a)** — a CI check without a new command: a .NET unit test finds every folder and runs the validator on it; no `catalog validate` command. **CI proof (i)** — a throwaway draft pull request with a deliberately broken folder, closed after the result is recorded. The plan was approved without changes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Only `Release2026_1Tests` (integration, needs Docker) validates a real folder, and it names `2026.1`. A new version — or the project team's first pull request with 2026.1 priorities — is checked only by a local `catalog import` into a throwaway database (#239) | A test that finds the folders itself |
| F2 | `ReleaseFolderValidator` checks consistency only and needs no database: the folder's files and the keys of the seeded filter values (`FilterValueKeys`). Completeness (priorities, a procedure criterion per test type) is for `catalog activate` (O-S3-5 Q2 (iii)) | 2026.1 passes as it is; the check fits the unit tests (CI stage 3, no Docker) |
| F3 | The seeded keys come from `FilterValueSeed` (anonymous rows for `HasData`); the catalog migration writes the same rows, and OPS-03 (no pending model changes) holds that | Read the keys from `FilterValueSeed` in the test, no production code change |
| F4 | `MiniCatalogValues.InMemory()` holds only the MiniCatalog's values (e.g. no `document`), so it cannot be used for the real folders | A separate helper with every seeded key |
| F5 | The integration project copies `catalog/releases/**` into its output (`PreserveNewest`). A copy keeps files that were deleted or renamed in the repository until the output is cleaned | The new test reads the repository's folder directly |
| F6 | The import's database checks (version not yet stored; `supersedes` keys are criteria of the release stored last; a stored criterion keeps kind, test type and category) cannot run without a database | Out of scope; P1 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/CatalogReleaseFoldersTests.cs` (new) | Theory `INV:CAT-23 Validate_ReleaseFolder_HasNoErrors` with one case per folder under `catalog/releases/` (found from the test output by walking up to `ProcureAccess.sln`); a failing case lists every error as `<file> <JSON path>: <message>` (`Assert.Fail`, so the list is not repeated by Shouldly). Fact `INV:CAT-23 Find_ReleaseFolders_IncludesRelease2026_1` keeps the theory from passing because it found nothing. Both with `[Trait("Invariant", "CAT-23")]` | #239, F1, F2, F5 |
| `tests/ProcureAccess.Tests.Shared/Fixtures/SeededFilterValueKeys.cs` (new) | `Create()` returns the `FilterValueKeys` of every row of `FilterValueSeed` (key read by reflection; names and other fields are placeholders, only the keys count); throws when a seed row has no string `Key` | F3, F4 |
| `CLAUDE.md` | Header entry; §3: every release folder is validated in CI and what it does not check; §4: the line of `catalog/releases/`; §5: the local command | CLAUDE §11 |
| `docs/sprints/sprint_04/S4-05_release_folder_validation.md` | This protocol | – |

ARCHITECTURE, SPEC, TESTING (CAT-23 unchanged), the Figma mapping, `LEGACY_EXCEPTIONS` and `.github/workflows/ci.yml` (the test runs in the existing job "3. Unit (.NET)") are not touched. TD-33 is closed by this pull request; `TECH_DEBT` records it in its next version at the sprint close-out (as S4-04).

### 2.1 Running the check locally (project team)

With the .NET SDK of `global.json` (no Docker, no database), from the repository root:

```bash
dotnet test --project tests/ProcureAccess.Tests.Unit --filter-class "ProcureAccess.Tests.Unit.Catalog.Releases.CatalogReleaseFoldersTests"
```

It validates every folder under `catalog/releases/`, also one not yet committed. A broken folder fails with, for example:

```text
failed INV:CAT-23 Validate_ReleaseFolder_HasNoErrors(version: "9999.1")
  catalog/releases/9999.1/ has 2 validation error(s), each as <file> <JSON path>: <message>:
  exclusions.json $.exclusions[0].source.layer: Unknown layer 'useCaze'; allowed: useCase, appType, productPart, testType.
  criteria/10.1.1.1.json $.applicability[0].appTypes[0]: Unknown AppType 'documnet'. Filter values are seeded by the catalog migration; use one of their keys.
```

Without the SDK, the release pull request shows the same message in the CI job "3. Unit (.NET)". What the check does not cover is still checked by `catalog import` (F6): before a release goes to a kept database, import it once into a throwaway one (CLAUDE §5).

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | No user input, no database, no network; reads files of the repository only |
| Correctness | All 2026.1 files pass; a broken copy fails with each error (C3); zero folders cannot pass (R2) |
| Edge cases | A file at the top of `catalog/releases/` is not a folder and is ignored; a folder whose name differs from its `version` is refused by the validator (existing rule) |
| Performance | One run reads the 191 files of 2026.1 (188 criteria plus three release files), about 250 ms |
| a11y, UI texts | Not touched |
| Licence | No new dependency; SPDX headers on both new files |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The folder is found by walking up to `ProcureAccess.sln`; renaming the solution (e.g. to `.slnx`) breaks the test discovery, loudly | No change |
| R2 | A theory without data could pass by checking nothing | Covered by `Find_ReleaseFolders_IncludesRelease2026_1` |
| R3 | `SeededFilterValueKeys` reads `Key` by reflection from the anonymous seed rows. A changed row shape throws; a fifth layer would need a new `FilterLayer` value and a change here | Won't fix: keeps the production code unchanged |
| R4 | The csproj comment of the unit project says "no I/O", but this test reads repository files | Won't fix: `ReleaseFolderReaderTests` already reads fixture folders; the files are part of the source |
| R5 | `Release2026_1Tests` reads a copy in the build output, the new test the repository (F5) | Won't fix: the two check different things (import and activation vs. validation); see P2 |

No correctness or security bug that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | 0 warnings, 0 errors |
| C2 | `dotnet test --project tests/ProcureAccess.Tests.Unit` | 424 of 424 pass (the two new ones included) |
| C3 | Local negative check: `catalog/releases/9999.1/` = 2026.1 with `version` 9999.1, an unknown layer in `exclusions.json` and an unknown app type in `criteria/10.1.1.1.json` (removed afterwards) | The 9999.1 case fails with both errors (§2.1); 2026.1 and the guard pass |
| C4 | `dotnet format ProcureAccess.sln --verify-no-changes` | Exit 0 |
| C5 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` | Exit 0 each; the gate counts 14 CAT-23 tests |
| C6 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1071 / 1071 files |
| C7 | CI proof (i): draft pull request #256 (branch `scratch/S4-05-broken-release-folder`, the folder of C3 on top of this branch's first commit) | Job "3. Unit (.NET)" failed (run 42859888861): 425 tests, 1 failed — `INV:CAT-23 Validate_ReleaseFolder_HasNoErrors(version: "9999.1")` with exactly the two lines of §2.1 (file and JSON path). Every other job that had finished was green (lint and licences, build, Angular unit, architecture, invariant gate, CodeQL); the pull request was then closed without merging and its branch deleted |
| C8 | Integration and E2E tests | Not touched by the change; left to CI on the pull request |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | The import's database checks (F6) — above all that each `supersedes` key is a criterion of the previous release — are not part of the CI check. Without a database the previous release could only be taken as the previous folder, which is not always the release stored last | Follow-up issue only if the project team's first release PR shows the need; until then `catalog import` into a throwaway database (§2.1) |
| P2 | `Release2026_1Tests` could drop its own folder copy and read the repository as well (F5) | Optional, with the next change to that test |

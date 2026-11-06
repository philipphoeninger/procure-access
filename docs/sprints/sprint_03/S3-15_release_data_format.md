# S3-15 — Release data format and validation

> **Sprint:** 03, P0 task S3-15 (M2.2, 3 points). **Issues:** none. **Requirements / invariants:** SPEC_v2 R-ADM-01, SD20; ARCHITECTURE_v19 §5.1, C74, C104; prepares **CAT-23** (`planned`, flipped with S3-16); one more test for **CAT-13** (`active`, the import part its wording names). No TESTING version written. **Status:** done by Claude Code (2026-11-06); one pull request.
> **Scope:** exactly the sprint row: folder layout and JSON of `catalog/releases/<version>/`, a loader and a validator for the whole folder (references, exclusion layers, procedure mapping, allowed ratings), errors with file and JSON path, a test release folder. Not in scope: storing a release, revision sharing and `SupersededByKey` on stored criteria (S3-16), activation and completeness (S3-17), release 2026.1 (S3-27), a CLI.
> **Owner decisions** (2026-11-06, before the task): **1a** criterion keys are the EN 301 549 number as Kriterienkatalog Teil A/B writes it (`11.1.3.1c`), procedure criteria `PROC-<testTypeKey>`; the validator enforces it and the MiniCatalog keys follow; during the plan: **PDF/UA-1 clauses** (Teil B, seven criteria written „UA1 6.2“) as `UA1-6.2`. **2A** one file per criterion plus `release.json`, `exclusions.json`, `procedure-mapping.json`. **3a** a new criterion names the keys it replaces in `supersedes`; a split is refused until S3-14 P2 is decided. **4** the format is described in this protocol only (no `catalog/README.md` now). Defaults stated before the task and not objected to: every folder is a complete snapshot without revision numbers or ids; System.Text.Json only, no JSON Schema file; loader and validator `internal` in the Catalog module; test releases under `tests/`, covered by the existing `**/*.json` MIT rule; priorities optional (O-S3-5 Q2 (iii)); ratings by the names of `Rating`; no TESTING version.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17` and `TESTING_v29`; the highest versions are v19 and v31 (CLAUDE §2) | Worked from v19/v31 |
| F2 | The prompt lists „filter dimensions“ among the folder's JSON files; under O-S3-5 D1 the four filter-value tables are seeded by the catalog migration and release folders refer to them by key (C104, §5.1 „Filter values“) | The folder holds no filter values, only their keys (stated in the plan) |
| F3 | Kriterienkatalog Teil B has seven PDF/UA-1 criteria without an EN 301 549 number („UA1 6.2“ … „UA1 7.21“); keys cannot hold a space | Owner: `UA1-6.2` |
| F4 | The BIT inklusiv texts (Teil A/B) hold paragraphs and bullet lists, not only the examples | Every long text is an array of Markdown lines, not only the examples as announced before the task (§2.1) |
| F5 | `Criterion` cannot change after `Create` (S3-14 P3); a release folder knows only its own content | `supersedes` is read and checked against the folder; whether the old keys exist in the previous release and how `SupersededByKey` is stored is S3-16's (P1) |
| F6 | ARCHITECTURE §5.1 wants one procedure criterion per test type; the MiniCatalog has two of five, and O-S3-5 Q2 (iii) moves the other completeness rule (priorities) to `activate` | Import checks consistency, `activate` completeness (stated in the plan; P2) |
| F7 | S3-14 decision 2A: S3-15 writes the MiniCatalog as its test release folder | `T.1` and `T.2` under `tests/ProcureAccess.Tests.Shared/Fixtures/Releases/`, compared with `MiniCatalog.Build` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Releases/ReleaseFolderReader.cs` (new) | `ReadDirectory(dir, keys)` reads every file of a folder (folder name = version); `Read(name, files, keys)` validates a set of files (used by tests) | Row; nothing is stored |
| `…/Releases/ReleaseFolderJson.cs` (new) | JSON shape of the four file kinds; every property nullable (a missing value is a validation error with its path), unknown and duplicate properties refused, no comments, no trailing commas | Errors name file and path; typos are caught |
| `…/Releases/ReleaseFolderValidator.cs` (new) | All rules of §2.2; collects every error; returns a `ReleaseDefinition` only for a valid folder | CAT-23 (validation part) |
| `…/Releases/ReleaseDefinition.cs` (new) | The validated content for the import: criteria (sorted by key) with revision content, references, applicability pairs, priorities, procedure texts (`ProcedureTextContent` of the domain); exclusions; procedure mapping; `ReleaseFolderError(File, Path, Message)`; `ReleaseFolderResult` | Input of S3-16 |
| `…/Releases/FilterValueKeys.cs` (new) | Known keys per layer, built from filter values (the seeded rows in S3-16, `MiniCatalogValues` in tests) | D1 |
| `src/ProcureAccess.Modules.Catalog/Domain/Criterion.cs` | Comment of `Key`: the scheme of decision 1a | Was „decided in S3-15“ |
| `tests/ProcureAccess.Tests.Shared/Fixtures/MiniCatalog.cs` | Keys in chapter 99 (`99.1` … `99.11`, merge `99.10a` + `99.10b` → `99.10`); remark on the test release folders | 1a; S3-14 P5 |
| `tests/ProcureAccess.Tests.Shared/Fixtures/Releases/T.1/**`, `T.2/**` (new) | The MiniCatalog as release folders, made-up texts | 2A of S3-14; F7 |
| `tests/ProcureAccess.Tests.Shared/Fixtures/TestReleaseFolders.cs` (new), `ProcureAccess.Tests.Shared.csproj` | Path and a mutable copy of a test release; the JSON files are copied into the output of the test projects | Tests change one file and validate |
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/ReleaseFolderReaderTests.cs` (new) | Both folders are valid and equal `MiniCatalog.Build` (compared as JSON of the whole definition, supersedes derived from `SupersededByKey` of `T.1`); missing folder; version ≠ folder name | 2A |
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/ReleaseFolderValidationTests.cs` (new) | One test per rule of §2.2 on a copy of `T.1`/`T.2` with one change; the layer rule tagged `INV:CAT-13` | Every rule can fail |
| `CLAUDE.md` | Header; §3 release folder format and validation; §4 `Releases/`, test release folders | CLAUDE §11 |

`REUSE.toml` is unchanged: `catalog/releases/**` is already `MIT AND LicenseRef-AllRightsReserved` with BIT inklusiv (S3-11), and the test releases, whose texts are made up, fall under `**/*.json` (MIT). `reuse lint` is green.

### 2.1 The format (for the project team)

A release is a folder `catalog/releases/<version>/`, e.g. `catalog/releases/2026.1/`. It always holds the **whole** release: an unchanged criterion is simply the same file again, and the import (S3-16) finds out by comparing which criteria changed (CAT-07). The folder never holds database ids or revision numbers. Its pull request is the review of the release (R-ADM-01).

```
catalog/releases/2026.1/
  release.json             version and release notes
  exclusions.json          which value excludes which value of a lower layer
  procedure-mapping.json   which procedures' texts apply to which app type
  criteria/
    5.2.json               one file per criterion, named by its key
    11.1.3.1c.json
    UA1-6.2.json
    PROC-softwareTest.json
    …
```

Nothing else may be in the folder. All files are JSON in UTF-8, without comments.

**Filter values** (use cases, app types, product parts, test types) are not part of a release. A catalog migration creates them; the folder names them by key (ARCHITECTURE C104):

| Layer | `layer` | Keys |
|---|---|---|
| Verwendungszweck | `useCase` | `hr`, `reporting`, `trainingManagement`, `travelRequests`, `payroll`, `internalCommunication`, `databaseApplication` |
| Anwendungstyp | `appType` | `web`, `softwareOpen`, `softwareClosed`, `mobileOpen`, `mobileClosed`, `document`, `hardware`, `relayEmergencyService` |
| Funktionsbereich | `productPart` | `base`, `audio`, `video`, `twoWayVoice`, `rtt`, `biometrics`, `authoringTool` |
| Prüfverfahren | `testType` | `softwareTest`, `appTest`, `pdfTest`, `bikWeb`, `bikApp` |

**Criterion keys** (file names): the EN 301 549 number as Kriterienkatalog Teil A/B writes it (`5.2`, `11.1.3.1c`, `10.1.3.1a`), a PDF/UA-1 clause with a hyphen (`UA1-6.2`, `UA1-7.10`), or `PROC-<testType>` for the „Geprüft nach …“ criterion of a test type (`PROC-pdfTest`). Lower-case letter suffixes only. A key never changes once a release with it is imported.

**Long texts** are arrays of lines: one element per paragraph or list item, an empty string between paragraphs, Markdown for lists (`- …`). A pull request then shows a changed sentence as one changed line. Short texts (titles, reasons, rationales of priorities) are plain strings. Leave out what is not known; never write an empty string.

`release.json`:

```json
{
  "version": "2026.1",
  "releaseNotes": "Erste Fassung aus Kriterienkatalog Teil A/B."
}
```

`version` must equal the folder name. `releaseNotes` is optional.

`exclusions.json` — the rules of Kriterienkatalog §3.1; the target must lie in a lower layer than the source (CAT-13). Write `"exclusions": []` for none.

```json
{
  "exclusions": [
    {
      "source": { "layer": "useCase", "key": "hr" },
      "target": { "layer": "appType", "key": "document" },
      "reasonDe": "Eine HR-Anwendung ist kein Dokument."
    }
  ]
}
```

`procedure-mapping.json` — one entry per app type; app types without a procedure (today Hardware, Vermittlungs- oder Notrufdienst) are left out (§16.3.3). Write `"procedures": []` for none.

```json
{
  "procedures": [
    { "appType": "web", "testTypes": ["softwareTest"] },
    { "appType": "mobileOpen", "testTypes": ["appTest"] },
    { "appType": "document", "testTypes": ["pdfTest"] }
  ]
}
```

`criteria/<key>.json` — a requirement criterion:

```json
{
  "kind": "requirement",
  "category": "Allgemeine Anforderungen",
  "titleDe": "Aktivierung von Barrierefreiheitsfunktionen",
  "plainLanguageDe": "…",
  "rationaleDe": ["…"],
  "sortOrder": 10,
  "standardReferences": [
    { "standard": "en301549", "clause": "5.2", "url": "https://…" }
  ],
  "applicability": [
    { "appTypes": ["web", "softwareOpen", "mobileOpen"], "productParts": ["base"] }
  ],
  "priorities": [
    { "level": "B", "rationaleDe": "…" },
    { "appType": "web", "useCase": "hr", "level": "A", "weight": 2, "rationaleDe": "…" }
  ],
  "procedureTexts": [
    {
      "testType": "softwareTest",
      "originalNumber": "5.2",
      "originalTitleDe": "Aktivierung von Barrierefreiheitsfunktionen",
      "applicabilityDe": ["Der Prüfschritt ist anwendbar, wenn …"],
      "requirementDe": ["Bietet die Software …", "", "Beispiele für individuelle Funktionen:", "", "- Die Software bietet eine Vorlesefunktion an."],
      "rationaleWhyDe": ["Barrierefreiheitsfunktionen können …"],
      "allowedRatings": ["fulfilled", "restriction", "blockade", "notApplicable"],
      "positiveExamplesDe": ["- Die Funktion zum Aktivieren von leichter Sprache ist einfach identifizierbar und aktivierbar."],
      "negativeExamplesDe": ["- Die Bedienelemente sind versteckt und nur schwer auffindbar."],
      "sourceUrl": "https://softwaretest.bit-inklusiv.de/#aktivierung-von-barrierefreiheitsfunktionen"
    }
  ]
}
```

| Property | Required | Meaning |
|---|---|---|
| `kind` | yes | `requirement` or `testProcedure` |
| `testType` | only for `testProcedure` | the test type; the file must then be `PROC-<testType>.json` |
| `category` | no | chapter, e.g. „Allgemeine Anforderungen“ (≤ 128 characters) |
| `supersedes` | no | keys of earlier criteria this one replaces (rename: one key; merge: several). An old key has one successor; it must not be a key of this release |
| `titleDe` | yes | title (until K-1: the BIT inklusiv title) |
| `plainLanguageDe` | no | plain-language title (K-1, O-1) |
| `rationaleDe` | no | lines; the project team's explanation |
| `sortOrder` | yes | order in lists (a whole number) |
| `standardReferences` | no | `standard` = `en301549`, `wcag`, `bitInklusiv` or `iso9241`; `clause` (≤ 64); optional `url` — the same clause must have the same URL in every file |
| `applicability` | yes for `requirement`, never for `testProcedure` | each entry: the criterion applies to every listed app type, for each listed product part; without `productParts` for any part. No pair twice |
| `priorities` | no (until O-1) | `level` `A`, `B` or `Info`; optional `appType` and `useCase` (both left out = catalog default); optional `weight`; `rationaleDe` (≤ 2000). One rule per pair |
| `procedureTexts` | no; never for `testProcedure` | one per test type: `originalNumber` (≤ 32), `originalTitleDe` (≤ 500), `sourceUrl` (the BIT inklusiv anchor), `allowedRatings` (at least one of `fulfilled`, `slightRestriction`, `restriction`, `barrier`, `blockade`, `notApplicable`; „Nicht eingeschätzt“ is always allowed and never listed), and the optional line texts `applicabilityDe`, `requirementDe`, `rationaleWhyDe`, `positiveExamplesDe`, `negativeExamplesDe`. The test type must be mapped to one of the criterion's app types in `procedure-mapping.json`, or the text would never be shown |

A procedure criterion (`PROC-pdfTest.json`):

```json
{
  "kind": "testProcedure",
  "testType": "pdfTest",
  "titleDe": "Geprüft nach BIT inklusiv PDF-Test",
  "sortOrder": 900,
  "priorities": [{ "level": "B", "rationaleDe": "…" }]
}
```

### 2.2 What the validation checks

It reads the whole folder and reports **every** error, each as `<file> <JSON path>: <message>`, e.g. `criteria/11.1.3.1c.json $.procedureTexts[0].allowedRatings[2]: Unknown rating 'erfüllt'; allowed: …`. A JSON syntax error also names the line. If there is any error, no release definition is returned, so nothing half-valid reaches the import (R-ADM-01).

| Area | Rule |
|---|---|
| Layout | only the three files and `criteria/<key>.json`; the three files exist; at least one criterion |
| JSON | valid syntax; no unknown or duplicate property; required values present; texts not empty, without surrounding white space, within the database limits; line texts without line breaks inside an element, not starting or ending with an empty line |
| Release | `version` equals the folder name (≤ 32) |
| Keys | file names follow decision 1a; `supersedes` names valid keys outside the release, each old key once in the whole release (no split) |
| References | every filter-value key exists in its layer; one standard clause has one URL in the whole release (the database keeps one row per standard and clause) |
| Exclusions | the target layer is lower than the source layer (CAT-13), a test type excludes nothing, no pair twice |
| Procedure mapping | one entry per app type, at least one test type, no pair twice |
| Procedure criteria | kind `testProcedure` with a known `testType`, named `PROC-<testType>`, no applicability, no procedure texts; a requirement has no `testType` and no `PROC-` key |
| Applicability | at least one app type for a requirement; no app type / product part pair twice |
| Priorities | known level and filter values, a rationale, one rule per app type / use case pair |
| Procedure texts | one per test type; its test type is mapped to an app type the criterion applies to (not checked when the mapping file or the criterion's applicability already has an error, so a typo gives one error); required fields; absolute http(s) URL without surrounding white space |
| Allowed ratings | at least one, only the six names, each once |

Not checked here, because a folder alone cannot tell: whether a `supersedes` key existed in the previous release (S3-16), and whether the release is complete — a priority for every requirement criterion, a procedure criterion for every test type (`catalog activate`, S3-17; O-S3-5 Q2 (iii)).

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | No endpoint, no database write. JSON read with System.Text.Json into plain classes (no polymorphic types); the reader reads only below the folder it is given; URLs only checked, never fetched |
| 2 | Correctness — rules vs. domain and database | Every domain refusal (`FilterExclusion.Create`, `AddApplicability`, `AddPriority`, `AddProcedureText`, `Criterion.Create`) and every `HasMaxLength` of the catalog configurations has a validation rule, so the import never meets an exception it cannot point to a file for |
| 3 | Architecture rules | All new types `internal`; no new project, package or reference; ARC-01 – ARC-03 green |
| 4 | N+1 / queries | No query |
| 5 | Edge cases | Missing files and folder, syntax errors, `null` entries, empty arrays, duplicates on every list, unknown keys per layer, cascades (an invalid mapping file does not also fail every procedure text) |
| 6 | Licence | No new dependency; release folders already annotated (§2) |
| 7 | Independent review (agent) sorted per CLAUDE §6.8 | see §3.1 |

### 3.1 Review findings

My own pass and the review agent (code-reviewer, read-only, against CLAUDE.md, ARCHITECTURE_v19 §5.1, the domain methods and the EF configurations) found no security bug and no rule that lets the import meet a refusal it cannot point to a file for, except R2. Sorted per CLAUDE §6.8:

| # | Remark | Sorting |
|---|---|---|
| R1 | An unknown app type in `applicability` also failed the criterion's procedure texts („no app type … is mapped“), a second, misleading error | **Fixed:** the mapping check of the texts is skipped when the applicability has an error, as already for an invalid mapping file; test `ProcedureText_WhenAnAppTypeOfTheCriterionIsUnknown_IsNotReportedAsWell` |
| R2 | `StandardReference` has a unique index on (standard, clause), so two files citing one clause with different URLs could not both be stored | **Fixed:** refused within the release with both file names; test `StandardReference_SameClauseWithAnotherUrl_IsRefused`. Across releases see P9 |
| R3 | A URL with surrounding white space passed (`Uri.TryCreate` trims) while every other text refuses it | **Fixed:** refused; test `Url_WithSurroundingWhiteSpace_IsRefused` |
| R4 | `.DS_Store`, `.gitkeep` and similar files are reported as unexpected | **Won't fix:** intended — the folder is the review artefact and holds nothing else; P7 |
| R5 | `ReadDirectory_FolderNameIsTheVersion` did not follow `Scenario_Condition_ExpectedOutcome` | **Fixed:** renamed `ReadDirectory_FolderNameDiffersFromVersion_IsRefused` |
| R6 | (own) An invalid `procedure-mapping.json` would have failed every procedure text as well | **Fixed** before the review (`mappingIsValid`); test `ProcedureText_WhenTheMappingFileIsInvalid_IsNotReportedAsWell` |

One review round is enough: after the fixes no remark affects current behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (336) |
| 4 | Mutation check: one changed title in `T.1` fails `ReleaseFolderReaderTests` | ✅ fails as expected, reverted |
| 5 | `node scripts/check-invariants.mjs` | ✅ gate passed |
| 6 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 7 | `reuse lint` (Docker image) | ✅ compliant |
| 8 | E2E | Not run: no client or API change; CI runs E2E on all five browser projects |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | `supersedes` is checked within the folder only. The import must check that each old key is in the previous release and store the successor on the existing `Criterion`, which has no method for it yet (S3-14 P3) | S3-16 |
| P2 | Completeness rules for `catalog activate`: a catalog-default priority (or the rules that make one unnecessary) for every requirement criterion (Q2 (iii)) and one procedure criterion per seeded test type (ARCHITECTURE §5.1, C30) | S3-17 |
| P3 | ARCHITECTURE §5.1 still gives `BITV-SW-2.1.1` as the key example, and its „Release composition“ section does not describe the folder; the next ARCHITECTURE version should name the key scheme of 1a and point to §2.1 here (no task writes one now) | next ARCHITECTURE version |
| P4 | TESTING §6.2 should name the MiniCatalog's keys (`99.x`) and the test release folders next to S3-14 P1 | next TESTING version (S3-16) |
| P5 | Kriterienkatalog Teil B cites PDF/UA-1 (ISO 14289-1), which `Standard` does not have (EN 301 549, WCAG, BIT inklusiv, ISO 9241). S3-27 either cites them as `bitInklusiv` or needs a new enum value (a domain change and a migration) | S3-27 |
| P6 | The texts are stored as Markdown lines joined with `\n`; the criteria list and the detail view (S11) must render paragraphs and lists | S11 (sprint 04) |
| P7 | Hidden files (e.g. `.DS_Store`) in a release folder are reported as unexpected files; the format description says so, a `.gitignore` entry may be wanted | S3-27, when the first real folder is written |
| P9 | ARCHITECTURE §5.1 (C101) says „a corrected clause or URL is a new row cited by a new revision“, but `StandardReference` has a unique index on (standard, clause), so a corrected URL for the same clause cannot be a new row. The import (S3-16) must decide what a folder whose clause URL differs from the stored row means — refuse it, or change the index to include the URL (ARCHITECTURE and a migration) | S3-16, with the owner if the index changes |
| P8 | The remote branch `sprint03/S3-14-mini-catalog` is still on GitHub after the merge of #211 | owner |

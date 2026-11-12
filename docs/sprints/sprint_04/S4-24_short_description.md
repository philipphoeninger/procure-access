# S4-24 — Short description in the catalog model

> **Sprint:** 04, P2 stretch task S4-24 (2 points). **Issues:** #237 (`TECH_DEBT_v3.md` TD-30). **Requirements / invariants:** Kriterienkatalog §6.1, SPEC §8.3 (export column „Kurzbeschreibung“), ARCHITECTURE §5.1; CAT-23 (validation of the new field) and CAT-07 (a changed short description is a new revision) — both already `active`, no invariant changes status.
> **Status:** done — pull request #298 open.
> **Scope:** the optional field `CriterionRevision.ShortDescriptionDe` with its catalog migration, the release folder property `shortDescriptionDe` (reader, validator, import, revision comparison), the release snapshot and `ICatalogSelectionResolver`, and the export column „Kurzbeschreibung“ that now reads it; ADR-016 in ARCHITECTURE_v43 (C129), SPEC v2.15, UX copy v26 (note only), Handoff 1.24, gap list v3. Not here: the criteria answer (SPEC §8.2) and the S11 card (O-S4-78 (a)), the S11 search over it, the texts themselves (project team, K-1).
> **Owner decisions (2026-11-12, all (a)):** **O-S4-6** — the short description is a field of the revision and of the release format, with an ADR; the column was written empty until now (S4-20a). **O-S4-78** — scope: catalog model, release format, import and the export column only; the S11 card and the criteria answer follow as their own task. **O-S4-79** — format: an optional plain string `shortDescriptionDe`, no Markdown, no length limit, checked like `plainLanguageDe`. **O-S4-80** — the gap list gets a v3 that names the field and lists it as missing for every criterion in one general line, no column per criterion. Defaults stated before the task and not objected to: name `ShortDescriptionDe`; `PlainLanguageDe` stays the plain-language title (no rename, its comment and Handoff §5.5 corrected); the test release `T.1`/`T.2` and the MiniCatalog give `99.1` a short description; 2026.1 stays without one; UX copy v26 changes only the note of `exportFile.column.shortDescription`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `CriterionRevision` held `TitleDe`, `PlainLanguageDe` and `RationaleDe`; the comment of `PlainLanguageDe` said „title or summary“, while the code (S11 H2, export „Titel“, O-S4-32) uses it as a title only | New field next to it; the comment now says „title“ |
| F2 | Handoff §5.5 mapped the card's short description to `PlainLanguageDe`, the same field as the title row | Handoff 1.24 names `ShortDescriptionDe` and that the card leaves it out until its follow-up |
| F3 | `CriteriaColumns` wrote the column „Kurzbeschreibung“ as `null` (S4-20a, O-S4-6) | The column reads `ExportRow.ShortDescription` |
| F4 | The release folder reader refuses unknown properties (`JsonUnmappedMemberHandling.Disallow`) | A folder with `shortDescriptionDe` failed before this task; `CriterionJson` gets the property |
| F5 | `RevisionContent.Matches` decides whether the import shares a stored revision (CAT-07) | The field is part of the comparison, so a changed short description gives a new revision |
| F6 | `T.1` and `T.2` share the revision of `99.1`, and `ReleaseFolderReaderTests` checks both folders against `MiniCatalog.Build` | The value goes into both folders and the MiniCatalog in one change |
| F7 | The criteria endpoint (`CriteriaResponse`) does not carry the field | Its answer and ETag are unchanged; the field reaches only the export (O-S4-78 (a)) |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Domain/CriterionRevision.cs` | `ShortDescriptionDe` (nullable), last optional parameter of `Create`; comment of `PlainLanguageDe` corrected | ADR-016 |
| `src/ProcureAccess.Modules.Catalog/Persistence/Migrations/*_AddShortDescription.cs` (new, `internal`), model snapshot | Nullable `text` column `catalog."CriterionRevisions"."ShortDescriptionDe"`; the snapshot's product version follows the package (10.0.15) | ADR-014 |
| `src/ProcureAccess.Modules.Catalog/Releases/ReleaseFolderJson.cs`, `ReleaseFolderValidator.cs`, `ReleaseDefinition.cs` | Optional `shortDescriptionDe`, validated with `OptionalText` like `plainLanguageDe` (empty or surrounding white space refused, no limit) | CAT-23, O-S4-79 |
| `src/ProcureAccess.Modules.Catalog/Releases/RevisionContent.cs`, `CatalogReleaseImporter.cs` | Compared for revision sharing; written on a new revision | CAT-07 |
| `src/ProcureAccess.Modules.Catalog/Reading/CatalogReleaseSnapshot.cs`, `Criteria/CatalogSelectionResolver.cs`, `src/ProcureAccess.Modules.Catalog.Contracts/ICatalogSelectionResolver.cs` | The snapshot holds it; `ResolvedCatalogCriterion.ShortDescription` | O-S4-78 |
| `src/ProcureAccess.Modules.Export/Documents/ExportContent.cs`, `Sources/AdHocSelectionSource.cs`, `Formatting/CriteriaColumns.cs` | `ExportRow.ShortDescription`; the column „Kurzbeschreibung“ writes it (empty without a value), in CSV (with the formula guard of every text cell) and XLSX | SPEC §8.3 v2.15 |
| `tests/ProcureAccess.Tests.Shared/Builders/Catalog/CriterionRevisionBuilder.cs`, `Fixtures/MiniCatalog.cs`, `Fixtures/ReleaseDefinitions.cs`, `Fixtures/Releases/T.1/criteria/99.1.json`, `T.2/criteria/99.1.json` | `WithShortDescription`; `99.1` has one in the MiniCatalog and both folders | TESTING §6.2 |
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/ReleaseFolderValidationTests.cs` | `INV:CAT-23`: empty, blank and padded values refused at `$.shortDescriptionDe`; a given value read as written | CAT-23 |
| `tests/ProcureAccess.Tests.Unit/Catalog/Releases/RevisionContentTests.cs` | `INV:CAT-07`: a changed and a removed short description do not match | CAT-07 |
| `tests/ProcureAccess.Tests.Unit/Catalog/MiniCatalogTests.cs` | The fixture promises exactly one criterion with a short description | — |
| `tests/ProcureAccess.Tests.Unit/Export/CsvExportFormatterTests.cs`, `XlsxExportFormatterTests.cs`, `AdHocSelectionSourceTests.cs` | Filled and empty cell; the source copies the value | EXP-01, EXP-04 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogPersistenceTests.cs` | The value survives a round trip through the database | — |
| `tests/ProcureAccess.Tests.Integration/Export/ExportEndpointTests.cs` | `Export_Csv_ShortDescription_IsTheImportedTextOrEmpty`: imported `T.1`, CSV over HTTP — `99.1` has its text, every other row an empty cell | SPEC §8.3 |
| `e2e/journeys/j1-configurator.spec.ts` | The downloaded XLSX has the short description of `99.1` and an empty cell for `99.2` | J1 |
| `docs/ARCHITECTURE_v43.md` (new; v41 deleted) | C129, ADR-016, §5.1 row of `CriterionRevision` | — |
| `docs/SPEC_v2.md` | v2.15: §8.3 cell content, §13 O-1 names the gap list v3 | — |
| `docs/kriterienkatalog_2026_1_luecken_v3.md` (new; v1 deleted) | The short description bullet names `shortDescriptionDe` and what it is used for | O-S4-80 |
| `docs/ux_copy_procureaccess_v26.md` (new; v24 deleted) | U111: only the note of `exportFile.column.shortDescription`; no text changes, so `de.json` and `Texts/de.json` are unchanged | — |
| `docs/design_handoff_procureaccess_v1.md` | 1.24: §5.5 short description = `ShortDescriptionDe` | — |
| `docs/sprints/sprint_03/S3-15_release_data_format.md` | Row `shortDescriptionDe` in the property table of §2.1 (CLAUDE.md names it as the format reference), marked as added by S4-24 | — |
| `CLAUDE.md` | §3 state, header log | — |

No new dependency, no new project, no client change outside the E2E test.

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases, a11y) over the diff against `main` (skill `engineering:code-review`). Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | `CriterionDefinition`, `ResolvedCatalogCriterion` and `ExportRow` are positional records with several `string?` parameters in a row; a swapped argument would compile | **Won't fix** | Existing pattern of these records; every call site was checked, and the formatter and endpoint tests compare the cells by column, so a swap fails them |
| R2 | The model snapshot's `ProductVersion` moved from 10.0.14 to 10.0.15 | **No change needed** | It is the version of the referenced EF Core package (10.0.15); the Platform and IdentityAccess snapshots follow with their next migration |
| R3 | A short description could start with `=` and reach a spreadsheet | **No change needed** | The CSV guards every text cell (O-S4-67); XLSX writes texts as texts (C125); the text comes from a reviewed release folder |
| R4 | TESTING §6.2 describes the MiniCatalog without the short description | **Won't fix** | No invariant changes, so no TESTING version for one sentence; `MiniCatalogTests` lists the case (`ShortDescription_OnlyPriorityAllLevelsHasOne`). The next TESTING version can add it |
| R5 | `TECH_DEBT_v3.md` (TD-30, line 169) and `ROADMAP_v4.md` still name `kriterienkatalog_2026_1_luecken_v1.md` | **Follow-up** | Both get a new version at the end of the sprint (process step 14): TD-30 resolved, gap list v3 |

Checked without finding: no new query (the snapshot already loads the revision; one more column); the immutability interceptor is untouched — stored rows are never changed, an old revision keeps no value; the criteria endpoint's body and ETag are unchanged (F7); ARC-01 – ARC-03 green (the contract gains one record member, no new type).

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass (one whitespace finding in a test fixed first) |
| C2 | `dotnet test`: Unit, Architecture, Integration | 548 + 61 + 240 = 849 of 849 pass |
| C2a | Mutation: the field left out of `RevisionContent.Matches` | `INV:CAT-07 Matches_AnyFieldChanged_IsFalse` fails; reverted |
| C3 | `dotnet ef migrations has-pending-model-changes` for `CatalogDbContext` | No changes since the last migration |
| C4 | `check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` (reads UX copy v26) | Exit 0 each |
| C5 | `e2e`: `npm run typecheck`, `npm run lint` | Pass |
| C6 | `reuse lint` | Compliant |
| C7 | E2E (J1) | Left to CI: the user's dev stack runs on the same ports and the disk had 2.7 GB free (an E2E build leaves about 2 GB of images) |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request #298 opened; not merged | Owner |
| P2 | The criteria answer (SPEC §8.2) and the S11 card (Handoff §5.5, Kriterienkatalog §6.1: under the source title) do not show the short description; the S11 search (O-S4-26) does not look at it | Issue #299 „S11: show the short description (Kurzbeschreibung) on the criterion card“ — `shortDescription` in the criteria answer (SPEC §8.2, contract test), the card line, and an owner decision on the search |
| P3 | The project team writes the texts (K-1); release 2026.1 has none | Gap list v3; with the priorities (O-1) in a later 2026.x folder |
| P4 | A kept dev database stores `T.1` without the short description of `99.1` (a stored release never changes) | `docker compose down -v` before using the dev stack with this branch; CI and the E2E stack start empty |
| P5 | TD-30 is resolved; TECH_DEBT and ROADMAP still name gap list v1 (R5) | End of sprint 04 (process step 14) |

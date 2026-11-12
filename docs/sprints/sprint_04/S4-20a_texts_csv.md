# S4-20a — Texts of the export files and the CSV renderer

> **Sprint:** 04, P0 task S4-20a (first part of S4-20, M2.7; split by O-S4-68). **Issues:** none. **Requirements / invariants:** R-LST-12 (file format per SPEC §8.3); EXP-04 → `active`, EXP-03 → `active` with its CSV part; the CSV file row of CAT-16.
> **Status:** done — pull request open (see §5 P1 for the number).
> **Scope:** the texts of the export files as UX copy keys (UX copy v24 `exportFile.*`), embedded in the Export module and checked by `check-i18n.mjs` as its second target; the column model shared by every format; the CSV renderer; TESTING_v49, ARCHITECTURE_v38, SPEC v2.11. Not here: the XLSX renderer, the sheet „Info“, `Export:ToolUrl` and ClosedXML (S4-20b); the short-description field in the catalog (S4-24); the export dialog S13, rate limit and `ExportCreated` (S4-21).
> **Owner decisions (2026-11-12, all (a)):** **O-S4-6** — the short description becomes a catalog field (S4-24, ADR); the column is written empty until then. **O-S4-62** — Claude drafts the missing file texts in UX copy v24, the owner confirms the wording in this pull request. **O-S4-63** — the texts are an embedded `de.json` in the Export module, checked by `check-i18n.mjs`. **O-S4-64** — the tool URL is the 🔧 server setting `Export:ToolUrl` = `https://procureaccess.de` (S4-20b). **O-S4-65** — „Priorität (Ihre Wahl)“ holds the level that applies. **O-S4-66** — the „Kriterien“ data range is an Excel table `Kriterien` (S4-20b). **O-S4-67** — the CSV guards text cells against formulas. **O-S4-68** — S4-20 is split into S4-20a and S4-20b. ClosedXML 0.105.1 approved for S4-20b.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-19 left one `StubExportFormatter` per format; `ExportRow` already held every column of SPEC §8.3 except the short description | No change to the pipeline; renderers replace the stubs |
| F2 | The short description has no field in the catalog (TD-30, #237) | O-S4-6 |
| F3 | The UX copy had no keys for the file texts: „Info“ labels, the BIT inklusiv source line, sheet names and column headers (SPEC §8.3 lists the headers, but not as keys) — CLAUDE §6.5 | O-S4-62; UX copy v24 U109 |
| F4 | Only the client has texts by key; the server had no place for UI texts | O-S4-63 |
| F5 | The client's `de.json` carries the area `criteria.export` in full, so file texts under `criteria.export.file.*` would have to be in the client too | New root `exportFile` (U109) |
| F6 | SPEC §8.3 did not say what „Priorität (Ihre Wahl)“ holds (`ExportRow.ChosenLevel` is null when the choice equals the recommendation), nor the „Titel“ fallback | O-S4-65; „Titel“ as the H2 of S11 (O-S4-32); SPEC v2.11 |
| F7 | A note starting with `=` runs as a formula when the CSV is opened in a spreadsheet | O-S4-67 |
| F8 | ClosedXML 0.105.1 accepts SixLabors.Fonts `[1.0.0, 3.0.0)`; NuGet resolves 1.0.0 (Apache-2.0), but 2.x and later carry the Six Labors Split License, which fails D12. The other transitive packages (ClosedXML.Parser, DocumentFormat.OpenXml, ExcelNumberFormat, RBush.Signed) are MIT; no advisory for these versions | ClosedXML approved (O-S4-68); the Dependabot rule comes with the package in S4-20b |
| F9 | The plan was more than one session (texts, check script, two renderers, five documents) | O-S4-68: S4-20a and S4-20b |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `docs/ux_copy_procureaccess_v24.md` (new; v22 deleted) | U109: table „Exportdatei“ in §5.6 with `exportFile.sheet.*`, `exportFile.column.*` (13 headers of SPEC §8.3 plus „Katalogversion“) and `exportFile.info.*` (labels, source line) | O-S4-62 |
| `src/ProcureAccess.Modules.Export/Texts/` (new) | `de.json` (written by `check-i18n.mjs --write`, embedded resource), `ExportTexts` (reads it, refuses to start without a key of `ExportTextKeys.Required`), `ExportTextKeys` | O-S4-63 |
| `src/ProcureAccess.Modules.Export/Formatting/CriteriaColumns.cs` (new) | The columns of SPEC §8.3 with header key and cell value; „Titel“ fallback, empty „Kurzbeschreibung“, „Ihre Wahl“ = level that applies | SPEC §8.3; O-S4-6, O-S4-65 |
| `src/ProcureAccess.Modules.Export/Formatting/CsvExportFormatter.cs` (new) | UTF-8 with BOM, `;`, CRLF, RFC 4180, „Katalogversion“ first, formula guard | EXP-03, EXP-04; O-S4-67 |
| `src/ProcureAccess.Modules.Export/ExportModule.cs`, `StubExportFormatter.cs`, `.csproj` | Registers `ExportTexts` and the CSV renderer; the stub stays for XLSX; embeds `Texts/de.json` | — |
| `scripts/check-i18n.mjs`, `check-i18n.test.mjs` | Two targets (`AREAS` client, `EXPORT_AREAS` Export); `--write` fills both; error lines name the file; two self-test cases (one-key area, `exportFile` outside the client areas) | V152 |
| `tests/ProcureAccess.Tests.Shared/Export/CsvReader.cs` (new) | Strict reader: BOM, valid UTF-8, CRLF records, RFC 4180 quoting; a malformed file fails | EXP-04 |
| `tests/ProcureAccess.Tests.Unit/Export/CsvExportFormatterTests.cs`, `ExportTextsTests.cs` (new) | Encoding, line ends, header order, release column, cell content, quoting, formula guard, „Geprüft nach …“ row, empty list; texts complete | EXP-03, EXP-04, CAT-16 |
| `tests/ProcureAccess.Tests.Integration/Export/ExportEndpointTests.cs` | The CSV of `POST /api/v1/exports` on `T.1` (included criteria in list order, release, user's level and note); the attachment test no longer reads the stub body | EXP-03 |
| `docs/TESTING_v49.md` (new; v47 deleted) | V149 EXP-04, V150 EXP-03 (CSV part), V151 CAT-16 CSV row, V152 i18n check, V153 upkeep | — |
| `docs/ARCHITECTURE_v38.md` (new; v36 deleted) | C124: server texts, column model, CSV, decisions for S4-20b | — |
| `docs/SPEC_v2.md` | v2.11: §8.3 text keys, cell content, Excel table, `Export:ToolUrl`, formula guard | — |
| `docs/sprints/sprint_04/SPRINT_04.md`, `TASK_PROMPTS.md`, `CLAUDE.md` | S4-20a/S4-20b rows, O-S4-6 decided, O-S4-62 – O-S4-68; CLAUDE §3, §4, §5 | — |

No client, E2E, Figma-mapping or `LEGACY_EXCEPTIONS` change: the client's `de.json` is unchanged.

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases) over the diff against `main`, with `/engineering:code-review`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | The CSV integration test read cells by column number (9, 12) | **Fix** | Reads them by header text |
| R2 | The formula guard also prefixes a catalog text that starts with `-` (a Markdown list line) | **Won't fix** | O-S4-67 covers every text cell; no exported catalog field of 2026.1 or the test releases starts that way today (only a procedure „Warum“ text, which the file does not hold) |
| R3 | The one-key area `criteria.select.step1.none` also copies `criteria.select.step1.none.hint` into the Export texts | **Won't fix** | `inAreas` takes longer keys by design (shared with the client); one unused text, checked like the others; documented at `EXPORT_AREAS` |
| R4 | Full-width look-alikes (`＝`) are not guarded | **Won't fix** | Spreadsheets do not start a formula with them; OWASP lists the six characters guarded |
| R5 | `ExportTexts` is German only, while SPEC says „export language = UI language“ | **Won't fix** | The UI is German only; `Accept-Language` (API-06) adds a resource per language |

Checked without finding: no new endpoint, query or log line; the texts are read once at start (a missing key stops the start, also in `dotnet ef`, which runs `Program.cs`; the `catalog` command host does not register Export); the CSV of the largest release (183 criteria) is a few hundred kB in memory and never stored (EXP-05 unchanged and green); `ExportTexts` is immutable after construction.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 743 of 743 pass; after R1 the Export integration tests again: 17 of 17 |
| C3 | Mutation: the guard checks only `=`, `+`, `@` | 3 of 17 CSV tests fail (`-`, tab, CR); reverted |
| C4 | `check-invariants.mjs` (TESTING_v49), `check-figma-links.mjs`, `check-i18n.mjs` (both targets), `check-icons.mjs`; self-tests of i18n check and gate | Exit 0 each |
| C5 | `npm run lint` in `e2e/` (ESLint on `scripts/*.mjs`) | Pass |
| C6 | `reuse lint` | Compliant, 1208 / 1208 files (`Texts/de.json` through `**/*.json`) |
| C7 | Client, E2E | No client or E2E change; left to CI |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request number to be filled in after it is opened | This protocol, in the pull request |
| P2 | Owner: confirm the wording of U109 (UX copy v24 §5.6 „Exportdatei“), especially „Nummer“ (SPEC writes „Nummer (EN 301 549 / BIT inklusiv)“; the parenthesis read as an explanation like „(running)“) and the source line, which should be checked against the BIT inklusiv permission | Owner, in this pull request (O-S4-62) |
| P3 | XLSX renderer, sheet „Info“, `Export:ToolUrl`, ClosedXML with the Dependabot rule for SixLabors.Fonts ≥ 2; EXP-01, EXP-02, the XLSX part of EXP-03 and of CAT-16 | S4-20b |
| P4 | The short-description column stays empty | S4-24 (O-S4-6) |
| P5 | The client component `criteria-list-summary.ts` names `ux_copy_procureaccess_v22.md` in its `@spec` tag, which this task deleted (keep two versions, CLAUDE §2) | Next touch of that file |
| P6 | Under EXP-06 (step 3) the formula guard applies to both sources alike, because both write through `CsvExportFormatter` | Step 3 |

# S4-20b — XLSX renderer

> **Sprint:** 04, P0 task S4-20b (second part of S4-20, M2.7; split by O-S4-68). **Issues:** none. **Requirements / invariants:** R-LST-12 (file format per SPEC §8.3); EXP-01, EXP-02 → `active`; EXP-03 complete for the ad-hoc source; the XLSX file row of CAT-16.
> **Status:** done — pull request #287 open.
> **Scope:** `XlsxExportFormatter` with ClosedXML 0.105.1 in place of the placeholder of S4-19 — sheet „Kriterien“ as an Excel table, sheet „Info“ —, the 🔧 setting `Export:ToolUrl`, the Dependabot rule and a test guard for the licence of SixLabors.Fonts; TESTING_v50, ARCHITECTURE_v39. Not here: the persisted source (step 3, EXP-06), the short-description field (S4-24), the export dialog S13, rate limit and `ExportCreated` (S4-21), J1 to the downloaded file (S4-22).
> **Owner decisions (2026-11-12, all (a), taken in the planning of S4-20):** **O-S4-64** — the tool URL is the 🔧 server setting `Export:ToolUrl` = `https://procureaccess.de`. **O-S4-66** — the data range of „Kriterien“ is an Excel table `Kriterien` without a colour style. **O-S4-68** — S4-20 split; ClosedXML 0.105.1 approved. The wording of UX copy v24 U109 was confirmed unchanged in pull request #286 (recorded in the UX copy).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-20a left `StubExportFormatter` for XLSX and put every text the „Info“ sheet needs into `Texts/de.json` | The XLSX renderer reads only existing keys; `ExportTextKeys.Required` gets them |
| F2 | ClosedXML stamps a new workbook with the machine clock (created/modified) | The formatter sets both to the generation time from `TimeProvider` (CLAUDE §7) and the author to the tool name |
| F3 | ClosedXML reads the stored UTC time back as local time | The test compares instants in UTC |
| F4 | The API image (`aspnet` base) has no system fonts and no fontconfig | Nothing measures text (fixed widths, no `AdjustToContents`); checked in the image (C6) |
| F5 | An Excel table needs at least one data row | A list without criteria gets one empty data row |
| F6 | SixLabors.Fonts resolves to 1.0.0 (Apache-2.0), but ClosedXML accepts up to 3.0, and 2.0+ carries the Six Labors Split License | Dependabot ignore rule and `ClosedXmlLicenceTests` |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `src/ProcureAccess.Modules.Export/Formatting/XlsxExportFormatter.cs` (new) | Sheet „Kriterien“ (Excel table `Kriterien`, no style/stripes, bold frozen header row, fixed widths, wrapped text, test-step hyperlink), sheet „Info“ (label and value lines), document properties from the export | EXP-01 – EXP-03; O-S4-66 |
| `src/ProcureAccess.Modules.Export/Formatting/ExportToolUrl.cs` (new), `API/API.ProcureAccess/appsettings.json` | 🔧 `Export:ToolUrl` = `https://procureaccess.de`; the start fails unless it is an absolute http(s) URL | O-S4-64 |
| `src/ProcureAccess.Modules.Export/Formatting/StubExportFormatter.cs` | Deleted | — |
| `src/ProcureAccess.Modules.Export/ExportModule.cs`, `.csproj` | Registers `ExportToolUrl` and `XlsxExportFormatter`; ClosedXML 0.105.1 with a licence comment | D12 |
| `src/ProcureAccess.Modules.Export/Texts/ExportTextKeys.cs` | Sheet, „Info“ and reused screen keys added to `Required` | — |
| `.github/dependabot.yml` | Ignore SixLabors.Fonts ≥ 2.0.0 | D12 |
| `tests/ProcureAccess.Tests.Unit/Export/XlsxExportFormatterTests.cs`, `ExportToolUrlTests.cs`, `ClosedXmlLicenceTests.cs` (new) | Re-opened XLSX against its content; table, header, freeze, no colour, empty list, „Info“ lines, properties; the setting; the font package's major version | EXP-01 – EXP-03, CAT-16 |
| `tests/ProcureAccess.Tests.Integration/Export/ExportEndpointTests.cs` | The XLSX of `POST /api/v1/exports` on `T.1` re-opened and compared with the criteria answer and the user's decisions | EXP-01 |
| `docs/TESTING_v50.md` (new; v48 deleted) | V154 EXP-01, V155 EXP-02, V156 EXP-03, V157 CAT-16, V158 upkeep | — |
| `docs/ARCHITECTURE_v39.md` (new; v37 deleted) | C125; D12 names SixLabors.Fonts 2.0+ among the rejected | — |
| `docs/ux_copy_procureaccess_v24.md` | Records the owner's confirmation of U109 (#286); current document versions | — |
| `CLAUDE.md` | §3, §4, §5 | — |

SPEC unchanged: v2.11 (S4-20a) already describes the table, the tool URL and the cell content.

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases) over the diff against `main`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | The „Info“ lookup in the integration test walked the column twice | **Fix** | One `RowsUsed().Single(…)` |
| R2 | The first draft set column A of „Info“ bold and then reset the source line | **Fix** | Bold per label cell |
| R3 | The API image now ships ClosedXML and its packages (MIT, one Apache-2.0) without their licence notices | **Follow-up** | #187 (S4-27, stretch) covers the notices of every NuGet package in the API image |
| R4 | The date on „Info“ and in the file name is the UTC day | **Won't fix** | As S4-19 P5 (owner, if it matters before the pilot) |
| R5 | The hyperlink style of ClosedXML colours the link text | **Won't fix** | It is also underlined, so colour is not the only sign (EXP-02); the hyperlink itself is the meaning |
| R6 | A cell text above 32 767 characters would make ClosedXML throw | **Won't fix** | Notes are capped at 1 000 (EXP-07); catalog titles and rationales are short |
| R7 | The second note line on „Info“ has an empty label cell | **Won't fix** | „Hinweise“ labels both lines; a repeated label would be read twice |

Checked without finding: no new endpoint, query or log line; the workbook lives in memory per request and is disposed (EXP-05 green); every text is a text value, so a note starting with `=` stays text (unit and integration test); the table name comes from the UX copy text, which the tests use.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 761 of 761 pass |
| C3 | Mutation: no frozen header row and a coloured table style | Both EXP-02 tests fail; reverted |
| C4 | `check-invariants.mjs` (TESTING_v50), `check-figma-links.mjs`, `check-i18n.mjs` + self-test, `check-icons.mjs` | Exit 0 each |
| C5 | `reuse lint`; `dotnet list … package --vulnerable --include-transitive` on the Export module | Compliant; no vulnerable package |
| C6 | By hand in the API image (built from this branch, `Development`, throwaway PostgreSQL, `T.1` mounted, port 5099; the image has no `/usr/share/fonts` and no fontconfig) | XLSX and CSV → 200, `no-store`, file names as SPEC §8.3; the XLSX package: sheets „Kriterien“, „Info“, table `Kriterien` A1:M10 with all stripes off, pane frozen below row 1, no formula, no solid fill, creator and last modified by „ProcureAccess“, created = the export time; the CSV starts with the BOM. The log shows only EF's history queries against the new database. Containers, network and image removed |
| C7 | `.github/dependabot.yml` parsed | The nuget ignore list holds the SixLabors.Fonts rule |
| C8 | Client, E2E | No client or E2E change; left to CI (the dev stack was running) |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request #287 opened; not merged | Owner |
| P2 | Licence notices of ClosedXML and its packages in the API image | #187 (S4-27) |
| P3 | The accessibility tester's screen-reader check of an exported file (O-S4-66 assumes the table is announced well) | Outside the sprint (SPRINT_04) |
| P4 | EXP-01/EXP-03 for the persisted source, EXP-06 | Step 3 |
| P5 | J1 downloads the XLSX and checks it | S4-22 |
| P6 | The short-description column stays empty | S4-24 (O-S4-6) |

# S3-27 — Release 2026.1, part 1: texts without priorities

> **Sprint:** 03, P0 task S3-27 (M2.3, 3 points; P0 since O-S3-5). **Issues:** none. **Requirements / invariants:** SPEC_v2 R-ADM-01 (now v2.5), §13 O-1; Kriterienkatalog K-1, K-6; **CAT-23** (`active`, unchanged): the import validation passes on the real folder, and `catalog activate` refuses it. No TESTING version written: no invariant changes status.
> **Status:** done, pull request open.
> **Scope:** exactly the sprint row: `catalog/releases/2026.1/` in the format of S3-15 from the BIT inklusiv texts of Kriterienkatalog Teil A/B, with the exclusions (§3.1) and procedure mapping (§4) and one procedure criterion per test type; no priorities, rationales or plain-language titles (O-1, K-1); a list of what is missing per criterion; a test that `catalog import` accepts the folder. **Not in scope:** activating the release (O-S3-5 Q2 (iii)), an active release in any stack (S3-20b P1), a field for the short description (P2).
> **Owner decisions** (2026-11-09, before the task): **1** product parts marked „Vorschlag“ as written in Teil A/B; **2** no standard reference for the seven `UA1-…` criteria; **3** a one-off converter that is not committed; **4** the gap list as a German Markdown table in `docs/`. During the plan, all as recommended: **5** exclusion reasons as in Kriterienkatalog §3.2 („Passt nicht zum Verwendungszweck HR-Anwendung“, „Passt nicht zum Anwendungstyp Website oder Webanwendung“ — the UX copy text `criteria.select.excluded.*` with the name filled in); **6** the PDF test's examples per rating split into positive („erfüllt: …“) and negative examples (every other rating, with its prefix), its remarks after the negative examples, the intro line „Beispiele für Umsetzungen …“ dropped; **7** the three „nicht anwendbar“ PDF steps stay in, with `notApplicable` as their only rating. **Defaults** stated in the plan and not objected to: 2026.1 is imported only into throwaway test databases; 11.1.2.1 Video only (K-4); `titleDe` = the Software-Test title, an App-Test variant becomes the App-Test `originalTitleDe`; procedure mapping web/software → softwareTest, mobile → appTest, document → pdfTest (BIK Web/App have no texts and no mapping); the five `PROC-…` criteria titled „Geprüft nach <name>“ without priority; sort order = document order; EN reference without the BIT letter suffix and without URL; „Nicht getestet“ dropped (Kriterienkatalog §5).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The prompt names `ARCHITECTURE_v17`; the highest versions are ARCHITECTURE_v24, TESTING_v36, SPEC_v2 (v2.4), UX copy v15 (CLAUDE §2). PR #219 (S3-21) is open and also changes `CLAUDE.md` and `SPEC_v2.md` | Worked from the highest versions on main; whichever of #219 and this PR merges second gets a small conflict in the two headers (SPEC v2.5 is numbered against main) |
| F2 | Teil A has 126 `####` headings; four are parent steps (11.1.1.1, 11.1.3.1, 11.3.3.8, 11.5.2.10: „Übergeordneter Prüfschritt. Die Inhalte stehen in den folgenden Unterschritten.“) | 122 criteria from Teil A; the parent steps are no criteria |
| F3 | Teil A writes each text in one of three blocks: „Gleichlautend in beiden Verfahren“, „BIT inklusiv Software-Test“, „BIT inklusiv App-Test“ (23 different combinations); four criteria are App-Test only; 11.2.1.1 App-Test says „Alle Texte gleichlautend, siehe oben.“; five headings carry an App-Test title („(App-Test: „Ausrichtung“)“) | A shared field goes into both procedure texts; 11.2.1.1 App-Test gets the Software texts; the variant title is the App-Test `originalTitleDe` |
| F4 | Teil B (PDF test) has no positive/negative examples but a block „Bewertungsalternativen und Beispiele“: the allowed ratings, examples per rating, remarks | Owner decision 6 |
| F5 | Three PDF steps (10.2.5.1, 10.2.5.4, 10.4.1.3) only say „Dieser Prüfschritt ist nicht anwendbar.“; the format needs at least one rating | Owner decision 7 |
| F6 | `exclusions.json` requires `reasonDe`; Kriterienkatalog §3.1 has none | Owner decision 5 |
| F7 | The data model has no field for the short description (Kurzbeschreibung) of Kriterienkatalog §6.1 and SPEC §8.4 (export column) | Open point P2; named in the gap list |
| F8 | Kriterienkatalog §2.4 decided „Empfohlene Priorität: B“ for the procedure criteria, but gives no rationale, and the format requires one per priority | No priority in the `PROC-…` files; the gap list names B as decided and the rationale as missing |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `catalog/releases/2026.1/` (new) | `release.json` (version only, no release notes: no wording given), `exclusions.json` (7 rules), `procedure-mapping.json` (6 app types), `criteria/*.json` (183 requirement + 5 procedure criteria, 1.2 MB) | Row |
| `docs/kriterienkatalog_2026_1_luecken_v1.md` (new, German) | What is missing for every criterion (plain-language title, catalog-default priority with rationale, short description, optional explanation), what was taken from a proposal (Z-1 – Z-6), one row per criterion with empty columns to fill in | Decision 4; SPEC O-1 |
| `tests/ProcureAccess.Tests.Integration/Catalog/Release2026_1Tests.cs` (new) | `INV:CAT-23` import of the real folder: the summary (188 new criteria and revisions), inactive, kinds, the five `PROC-…` keys, no priority and no plain-language title, 301 procedure texts, 7 exclusions, 6 mappings; `INV:CAT-23` activation refused with one gap per criterion and nothing active | Row („show in a test that catalog import accepts the folder; do not activate it“) |
| `tests/ProcureAccess.Tests.Integration/ProcureAccess.Tests.Integration.csproj` | Copies every file of `catalog/releases/**` into `CatalogReleases/` of the test output | The test reads the folder the API image ships |
| `REUSE.toml` | The gap list (`docs/kriterienkatalog_*_luecken_v*.md`) gets the BIT inklusiv annotation: it quotes the titles | CLAUDE §10 |
| `docs/SPEC_v2.md` | v2.5: §13 O-1 names the folder and the gap list; change row v2.5-1 (incl. F7) | Prompt step 5 |
| `CLAUDE.md` | Header; §2 row „Release gaps“; §3 release 2026.1; §4 `catalog/releases/`; §5 import warning | CLAUDE §11 |

Not changed: ARCHITECTURE (no technical change; P2 and S3-15 P3 are for its next version), TESTING (no status change; CAT-23 gets two more tests on real data), ROADMAP (M2.3 says „imported, not activated“; the folder is importable, but no environment imports it — P5).

### 2.1 Conversion rules (the converter is not kept, decision 3)

From now on the folder is the source and is changed by pull request. The converter (Python, run once on 2026-11-09) applied these rules; it reported every line it could not place (none), and a second script checked that every text line of Teil A/B (2,367) is in the procedure texts of its criterion.

| Source | Folder |
|---|---|
| `#### <number> <title>[ (App-Test: „<title>“)]` | file `criteria/<number>.json`; `UA1 7.21` → `UA1-7.21`; `titleDe` = title; `originalNumber` as written (`UA1 7.21`) |
| `### Kapitel: <name>` (Teil A) | `category`; Teil B has no chapters → no category |
| Funktionsbereich | `applicability[0].productParts` (one part; suffixes „(Vorschlag …)“, „(vom übergeordneten Prüfschritt übernommen)“ dropped) |
| Gilt für Anwendungstypen | `applicability[0].appTypes` |
| Number | `standardReferences`: `en301549`, clause without the BIT letter suffix (`11.1.3.1c` → `11.1.3.1`), no URL; none for `UA1-…` |
| Link „Prüfschritt im … öffnen“ of each procedure | `sourceUrl` |
| Anwendbarkeit, Anforderung, Warum wird das geprüft?, Beispiele für positive / negative Bewertung | `applicabilityDe`, `requirementDe`, `rationaleWhyDe`, `positiveExamplesDe`, `negativeExamplesDe` — one element per line, anchors removed, one empty element between paragraphs, nested list indentation and bold sub-headings („**1. Überschriften**“) kept |
| Bewertungsalternativen | `allowedRatings` by the names of `Rating`; „Nicht getestet“ dropped |
| PDF: „Der Prüfschritt kann wie folgt bewertet werden:“ + list | `allowedRatings` |
| PDF: examples per rating | „- erfüllt: …“ → positive; every other rating prefix (also „leichte Einschränkung bzw. Einschränkung:“, 10.2.4.6) → negative; indented lines stay with their item; remark paragraphs → after the negative examples in their order; the intro „Beispiele für Umsetzungen mit …“ dropped |
| Order | `sortOrder` 10, 20, … in document order: Teil A, Teil B, then the `PROC-…` criteria |

## 3. Self-review (CLAUDE §6.7)

| # | Check | Result |
|---|---|---|
| 1 | Security | No production code; data only. No secret, no personal data |
| 2 | Correctness | Validator and importer accept the folder unchanged (test); content check: no source line missing; spot checks of an App-Test-only, a title-variant, a „nicht anwendbar“, a UA1 and a „Nicht getestet“ criterion |
| 3 | Architecture rules | No new project, package or reference |
| 4 | N+1 / queries | None in production code |
| 5 | Edge cases | Parent steps skipped; App-Test „siehe oben“; fields both shared and per procedure (none found, checked); combined rating prefix |
| 6 | Licence | No new dependency; release folder already annotated, gap list annotated; `reuse lint` green |
| 7 | Review (`/engineering:code-review`) sorted per CLAUDE §6.8 | §3.1 |

### 3.1 Review findings

| # | Remark | Sorting |
|---|---|---|
| R1 | The test copied only `**/*.json`, while the API image ships the whole folder and the validator refuses any other file — the test could pass on a folder the real import refuses | **Fixed:** the csproj copies `**/*` |
| R2 | 10.1.3.1c: „Dieser Prüfschritt ist erfüllt, wenn … Andernfalls handelt es sich um eine Einschränkung.“ is a remark, so it stands under the negative examples | **Won't fix:** decision 6; the team can move it in the folder (gap list Z-3) |
| R3 | The expected count of procedure texts is a fixed number | **Won't fix:** it documents the source structure; a changed folder should fail the test |
| R4 | (own) The first run put the combined prefix of 10.2.4.6 under the positive examples | **Fixed** in the converter before the commit |

One review round is enough: no remaining remark affects behaviour (CLAUDE §6.8).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ all passed (621) |
| 4 | Mutation checks: an unknown rating in `criteria/5.2.json` and a stray `.DS_Store` in the folder (R1) each fail both tests of `Release2026_1Tests` with the validator's message | ✅ fail as expected, reverted; the folder equals the converter output again (`diff -r`) |
| 5 | `node scripts/check-invariants.mjs` | ✅ gate passed |
| 6 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 0 errors |
| 7 | `reuse lint` (Docker image) | ✅ compliant |
| 8 | E2E | Not run: no client or API change; no stack has an active release |
| 9 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Priorities with rationales and plain-language titles for every criterion; then 2026.1 can be activated. The gap list is the work list; the result is a pull request on the folder | Project team (O-1, K-1, K-6) |
| P2 | The short description (Kurzbeschreibung) of Kriterienkatalog §6.1 and the export column of SPEC §8.4 have no field in `CriterionRevision` or the folder | Next ARCHITECTURE version / owner, before the texts arrive |
| P3 | Rating examples of the PDF test: if the split of decision 6 reads badly in S11, a field `ratingExamplesDe` (format, domain, migration, SPEC §8.2) is the alternative named in the plan | S11 (sprint 04) |
| P4 | The three „nicht anwendbar“ PDF steps: keep them (with a priority) or drop them in the next state of the folder (gap list Z-4) | Project team |
| P5 | ROADMAP_v3 M2.3 says „imported, not activated“; no environment imports 2026.1 before O-1 (a stored version never changes) | Next ROADMAP version |
| P6 | S3-20b P1: the E2E stack still has no active release; 2026.1 cannot be it before O-1 | S10 (sprint 04) |
| P7 | S3-15 P7 (hidden files): `catalog/releases/` has no `.gitignore` for `.DS_Store`; a stray file makes the import fail with a clear error, and `Release2026_1Tests` now catches it in CI | Won't fix now |

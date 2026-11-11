# S4-15b — Catalog sections of the detail panel

> **Sprint:** 04, P0 task S4-15b (M2.6; second half of S4-15, 2 points). **Issues:** none. **Requirements / invariants:** R-LST-01 (detail panel with H3 sections), Handoff §5.5; no invariant changes (TESTING stays at v46; CAT-02 keeps its S4-15a parts).
> **Status:** done — pull request #272 open.
> **Scope:** the row of S4-15b: the Markdown lines of the catalog texts as paragraphs and lists (S3-15 P6), the sections „Was wird verlangt?“ … „Wie wird bewertet?“, „So wird es geprüft“, tabs for several procedures, examples, „Gilt für“ with `appliesTo[]` in the criteria answer, the source line. Not here: the checkbox, notes and the K5 menu (S4-16); E2E of the card list (S4-18, S4-22).
> **Owner decisions (2026-11-11, all (a)):** before the task **O-S4-29** (`appliesTo[]` in the answer) and **O-S4-30** (own renderer, no `marked`); with the plan **O-S4-36** — http(s) links in catalog texts open in a new tab; **O-S4-37** — three examples per group, each group with its own „Alle {{count}} Beispiele anzeigen“, notes always visible; **O-S4-38** — short tab labels as three keys (UX copy v19 U101, a proposal the owner confirms in the pull request); **O-S4-39** — „#todo#“ in 11.1.3.2 shown verbatim and listed in the gap list. Defaults accepted with the plan: nested lists and continuation lines; HTML in a text stays text; `appliesTo[]` as `{ id, name }`; ratings through `assessment.value.*`; „So wird es geprüft“ is the link only; the source line per procedure; sections with the same text above the tabs; after „Alle … anzeigen“ the button goes and the focus moves to the fourth example; no short description (no data).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The criteria answer had no data for „Gilt für“ (Handoff §5.5) | O-S4-29: `appliesTo[]` from the rule that already decides whether a criterion is listed |
| F2 | The texts of 2026.1 use more than the subset of O-S4-30: four `[url](url)` links (12.1.2, 12.2.4), nested lists (10.1.3.2 PDF), 21 lines that continue a list item with a leading space (11.2.5.7 and others), `**bold**` as a pseudo-heading (10.2.2.2, 10.2.4.6), one `<H2> … </H2>` in an example, one `#todo#` | O-S4-36 (links), nested lists and continuation lines as default, HTML as text, O-S4-39 (`#todo#`) |
| F3 | No text line follows a list item without a blank line (checked over all 2026.1 texts), so a text line always starts a paragraph — no Markdown „lazy continuation“ needed | Simpler parser |
| F4 | 194 of the 301 procedure texts have more than three examples (up to 32); the Handoff did not say whether three per group or in total | O-S4-37 |
| F5 | `criteria.item.detail.tabs` held „Software-Test / App-Test / PDF-Test“ — a description, not a label; the full names (`testType.*`) need tab arrows at 360 px | O-S4-38, UX copy v19 U101 |
| F6 | 118 criteria of 2026.1 have two procedure texts (Software-Test and App-Test) | Tabs are the normal case for software criteria |

## 2. Changes

| Where | What | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Criteria/CriteriaResolver.cs`, `CriteriaResponse.cs` | `CriterionItem.AppliesTo` (`AppTypeItem` list) after `productPart`: the selected app types of `CriterionRules.AppTypesApplyingTo`, in app-type sort order; `[]` for a procedure criterion | O-S4-29, SPEC v2.9 §8.2 |
| `tests/ProcureAccess.Tests.Unit/Catalog/Criteria/CriteriaResolverTests.cs` | Two tests: product-part rows and order over three app types; empty for `PROC-…` | Row |
| `tests/ProcureAccess.Tests.Integration/Catalog/CriteriaEndpointTests.cs` | Contract test: field list with `appliesTo`, its value for one requirement and `[]` for the procedure criterion | Row („the endpoint test for `appliesTo[]`“) |
| `shared/components/catalog-text/catalog-markdown.ts` (new) | `parseCatalogText` (paragraphs, lists nested by two spaces per level, continuation lines), `parseInline` (`**bold**`, `[text](url)` only for http(s), else text), `isWebUrl`, `countTopLevelItems`, `limitTopLevelItems` | O-S4-30, O-S4-36, O-S4-37 |
| `shared/components/catalog-text/catalog-text.*` (new) | `pa-catalog-text`: renders the blocks with Angular nodes (recursive template for nested lists), never `innerHTML`; links new tab + `common.newTab`; `maxItems`; `focusItem(index)` focuses a top-level item after the next render | Handoff §5.5 |
| `features/configurator/state/criterion-detail-rules.ts` (new) | `detailLayout` (sections in Handoff order; a section with the same content in every text is shared, otherwise per procedure; missing everywhere → left out), `ratingsOf` (scale order), `RATING_KEYS`, `tabLabelKey` (U101, full name for a test type without a short label) | Handoff §5.5 |
| `features/configurator/components/procedure-sections/*` (new) | `pa-procedure-sections`: the H3 sections of one procedure text for the given kinds, ratings as a list with „ · “ from CSS, „So wird es geprüft“ with the test step link (only an http(s) `sourceUrl`) | Handoff §5.5 |
| `features/configurator/components/example-group/*` (new) | `pa-example-group`: H4 „So ist es gut“ / „So ist es nicht gut“, the first three examples, „Alle {{count}} Beispiele anzeigen“ described by the H4 and the card title; after it the focus goes to the fourth example | O-S4-37 |
| `features/configurator/components/criterion-card/*` | After „Warum diese Priorität?“: shared sections, `mat-tab-group` (two or more texts) with the other sections, the link and the source line per tab, „Gilt für“ (`Intl.ListFormat`, conjunction), with one text the source line last; „Geprüft nach …“ unchanged | Handoff 1.19 §5.5 |
| `features/configurator/models/criteria.ts`, `testing/criteria-fixture.ts` | `appliesTo: AppTypeName[]` (the type of `appTypesWithoutCriteria`, renamed from `AppTypeWithoutCriteria`) | SPEC §8.2 |
| `public/i18n/de.json`, `scripts/check-i18n.mjs` | `assessment.value` added to `AREAS`; `--write` copied its texts and the three U101 keys; the old `criteria.item.detail.tabs` removed by hand (`--write` only adds and changes) | U101 |
| `docs/ux_copy_procureaccess_v19.md` (new; v17 deleted) | U101: `criteria.item.detail.tabs.softwareTest` „Software-Test“, `.appTest` „App-Test“, `.pdfTest` „PDF-Test“ replace `criteria.item.detail.tabs` — **proposal, the owner confirms it in this pull request** | O-S4-38 |
| `docs/SPEC_v2.md` v2.9 | §8.2 example and text: `appliesTo[]`; change row v2.9-1 | O-S4-29 |
| `docs/ARCHITECTURE_v34.md` (new; v32 deleted) | C120: the contract, the parser and renderer, the panel layout; document upkeep of the input documents (UX copy v19, Handoff and mapping 1.19, SPEC v2.9, TESTING v46) | O-S4-29, O-S4-30, O-S4-36 – O-S4-38 |
| `docs/design_handoff_procureaccess_v1.md` 1.19 | §5.5: tab labels, per-procedure link and source line, „Gilt für“ after the tabs, examples per group with focus, ratings, Markdown and links | O-S4-36 – O-S4-38 |
| `docs/kriterienkatalog_2026_1_luecken_v2.md` (new; v1 stays) | Z-7: „#todo#“ in the Software-Test requirement of 11.1.3.2 | O-S4-39 |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-36 – O-S4-39 | — |
| `CLAUDE.md` | Header; §3 the catalog sections (S4-15b); §4 the new folders | — |

## 3. Self-review (CLAUDE §6.7)

Review with the CLAUDE §6.7 checklist by an independent reviewer (subagent) over the whole diff, plus my own pass. Sorted per CLAUDE §6.8:

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | Every plain-text piece of a catalog text was rendered with a space on each side (the multi-line `@default` block of `catalog-text.html`), so „werden ([https://…](…)).“ read „werden ( https://… ).“ — the text is no longer verbatim (12.1.2, 12.2.4 in 2026.1); the specs trimmed the text and missed it | **Fix** | The text piece sits in its own `<ng-container>`; `catalog-text.spec.ts` compares the whole paragraph exactly (failed before the fix) |
| R2 | Content of an inactive `mat-tab` is detached, so an example group expanded in one tab is collapsed again after switching tabs and back | **Won't fix** | `preserveContent` would render every tab of every card while collapsed; the lost state is one click; shared sections above the tabs keep theirs (P4) |

Checked without finding: links only for http(s) (`isWebUrl`, and Angular sanitises `href`), no `innerHTML`, `<H2>`/`#todo#` stay text; the depth rules of the parser (at most one level deeper, an indented first item is top-level); `firstItem` counts across several lists of one group, so the focus reaches the fourth example also in 10.4.1.2 (lists of 4 and 10 with a note between); `afterNextRender` with the component's injector, `tabindex="-1"` only on the target item; ids unique within a card and across cards (`k-<key>-<testType>-positive` above the tabs, `k-<key>-tab-<testType>-…` in them), `aria-describedby` of the examples button points at existing ids; headings H2 → H3 → H4; a shared section is never empty; „So wird es geprüft“ only with an http(s) `sourceUrl`; the tab header of the hidden panel updates through Material's resize observer when the panel opens; backend: no new query (the release snapshot already holds the rules), `appliesTo` from the same rule that lists the criterion, order by `SortOrder` then key, `[]` for `PROC-…`; the response record, the contract test and SPEC §8.2 agree. No correctness or security bug that affects current behaviour remains, so no second round.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 406 of 406 pass (373 before); 33 new: `catalog-markdown.spec.ts` (16), `catalog-text.spec.ts` (4, one with the exact-text check of R1), `criterion-detail-rules.spec.ts` (7), `criterion-card.spec.ts` (6 new, 17 in all) |
| C2 | `npm run lint` (client) | All files pass |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the same as on `main` |
| C4 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format --verify-no-changes` | 0 errors, 0 warnings; format clean |
| C5 | .NET tests: unit (426, 2 new), architecture (59), `CriteriaEndpointTests` (16, one extended) | All pass |
| C6 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` (264 keys, `assessment.value` new), `check-icons.mjs`; their self-tests | Exit 0 each; self-tests 78 of 78 |
| C7 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1157 / 1157 files |
| C8 | Parser over every text of `catalog/releases/2026.1` (1 746 texts, throwaway script) | No list mark or `**` left in any paragraph or item; exactly the 4 known links; lists up to four levels deep |
| C9 | In Chromium on the dev stack's API (`T.1`, branch client as a development build on port 4300; `appliesTo` added in the browser, because that API was built from `main`) | 99.1 with Software-Test and App-Test: tabs, sections, ratings, link, source line, „Gilt für“ as specified at 1280 px; at 320 px no horizontal page scrolling, but the tab labels paginate (P3) |
| C10 | E2E | Left to CI; no E2E test opens a detail panel yet (S4-18, S4-22) |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | U101 (`criteria.item.detail.tabs.*`) is a proposal | Owner confirms or edits the wording in this pull request; the code reads the keys only |
| P2 | „#todo#“ in 11.1.3.2 is shown verbatim (O-S4-39) | Project team (gap list v2 Z-7); fixed with the next release, as a stored release never changes |
| P3 | At 320 px (400 % zoom) two tab labels do not fit: Material shows its scroll arrows, the second label is cut and the header does not scroll to the selected tab on resize; both tabs stay reachable with the arrow keys, the page does not scroll sideways | Check when S11 with open cards joins the reflow E2E (S4-18/S4-22); if the reflow rule counts the cut tab, reduce the tab padding below 600 px |
| P4 | Content of an inactive tab is detached by Material, so an expanded example group of that tab collapses again after switching tabs (R2, won't fix) | Accept unless usability tests say otherwise |
| P5 | `ng serve` (`npm start`) fails to load the legacy `ngx-markdown` (Vite cannot resolve its optional `marked-katex-extension` import); the release build is not affected. Not caused by this task (no dependency change) | Follow-up issue, or gone when `ngx-markdown` leaves with the legacy screens |
| P6 | A client newer than its API (an answer without `appliesTo`) fails while rendering the cards (the hidden panel is in the DOM); client and API ship together, so only the moment of a deployment could show it | None; noted for the release procedure |

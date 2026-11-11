# S4-15a — Criterion card, K5 and the priority section of the detail panel

> **Sprint:** 04, P0 task S4-15a (M2.6; first half of S4-15, 4 points split 2 + 2). **Issues:** none. **Requirements / invariants:** R-LST-01 (compact card, H2/H3, anchor), R-LST-02 (K5 with rationale, matched rule and source), R-LST-07 (the „Geprüft nach …“ card, display part); CAT-02 gets its frontend part (`TESTING_v46.md` V139, level FE; v44 deleted). No invariant changes status. New UX copy `ux_copy_procureaccess_v18.md` (U100, a proposal; v16 deleted), Handoff 1.18, Figma mapping 1.19.
> **Status:** done — pull request #271 open.
> **Scope:** the row of S4-15 as split by O-S4-28: K5 `pa-priority-indicator` (display), `pa-priority-rationale`, the compact criterion card with its detail panel holding the first section „Warum diese Priorität?“, the „Geprüft nach …“ variant, and S11 loading and listing the cards. Not here: the catalog text sections with Markdown, tabs, examples, „So wird es geprüft“, „Gilt für“ (S4-15b); the K5 menu, the checkbox and notes (S4-16); summary, notices, search/filter UI, empty states, action bar (S4-17, S4-18).
> **Owner decisions (2026-11-11, before the task, all (a)):** **O-S4-28** — split S4-15 into S4-15a and S4-15b; with the plan the priority section moved into S4-15a, so no card shows a priority without its rationale. **O-S4-29** — „Gilt für“ gets `appliesTo[]` in the criteria answer (S4-15b). **O-S4-30** — a small Markdown renderer into Angular nodes, no raw HTML (S4-15b). **O-S4-31** — the meta line takes the clause of the EN 301 549 reference and shows only the parts that exist (two fragment keys, U100). **O-S4-32** — the H2 is the catalog `title` while `plainLanguage` is null. **O-S4-33** — the „Geprüft nach …“ panel starts with „Warum diese Priorität?“ too. **O-S4-34** — a failed S11 load gets no new text: only `not-found` is handled by the page, a 400 gets the generic snackbar. **O-S4-35** — the Figma line „Prüfverfahren · aus Ihrer Auswahl in Schritt 4“ (no UX copy key) is left out. **Defaults of the plan accepted:** K5 without a button until the menu of S4-16; rule text names from the filter options in the store; tab labels and `{{testType}}` from `testType.<key>`; anchor `k-<key with dots as hyphens>`; axe through the `axe-core` helper (O-S4-11), not `vitest-axe` as the row says; `criteria.list`, `criteria.item`, `criteria.priority` as `AREAS`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S11 (`pages/criteria-list/`) was a placeholder with H1 and „Auswahl ändern“; S4-14 had built `loadCriteriaList()`, `visibleCriteria` and the decisions, but nothing read them | The page loads the list on entry and renders the cards |
| F2 | The criteria answer names catalog values only by key (`productPart`, `matchedRule.appType`/`useCase`), and has no applicability per criterion (SPEC §8.2) | Names come from the filter options of the draft (`catalogNames`); „Gilt für“ needs `appliesTo[]` (O-S4-29, S4-15b) |
| F3 | `plainLanguage` is null for every criterion of `T.1` and 2026.1 (O-1, K-1); the short description `PlainLanguageDe` does not exist in the answer (O-S4-6, #237) | H2 falls back to the catalog title (O-S4-32); the compact card has no short description for requirements |
| F4 | `criteria.item.meta` „{{productPart}} · EN 301 549 {{number}}“ fits neither a criterion without product part nor the PDF/UA-1 criteria, which have no EN 301 549 reference | O-S4-31: two fragment keys proposed in UX copy v18 (U100) |
| F5 | Handoff §5.5 gives „Geprüft nach …“ cards a panel with text and note only, R-LST-02/CAT-02 a rationale for every priority | O-S4-33 (SPEC before Handoff); Handoff 1.18 follows |
| F6 | The criteria request carried `CALLER_HANDLES_PROBLEMS` for `validation`, `selection-excluded`, `not-found` for S10 and S11 alike (S4-14 R1/P1): a 400 on S11 showed nothing | O-S4-34: S11 passes only `not-found` |
| F7 | The Figma K5 component has a „Begründung ausblenden“ state for its own toggle; the UX copy has no key for it. S11 does not use the toggle (`showRationaleToggle=false`, U28) | The toggle is not built; open point P3 |
| F8 | `PriorityLevel` was a type of the configurator feature; K5 lives in `shared/` (Figma mapping §2: used by configurator and workspace) | The type moved to `shared/components/priority-indicator/priority-level.ts`; `models/criteria.ts` re-exports it |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `shared/components/priority-indicator/priority-level.ts` (new) | `PriorityLevel`, `PRIORITY_LEVELS`, `PriorityRuleText` | F8 |
| `shared/components/priority-indicator/priority-indicator.*` (new) | K5 `pa-priority-indicator`, `@figma 2011-203`: visible level `criteria.priority.<level>`, hidden name `criteria.priority.button` („Priorität: …“), weight `criteria.priority.weight` only for B, `criteria.priority.overridden` while the level differs from the recommendation. No button (menu with S4-16), no toggle (F7). Tokens of Figma 2011:203 | R-LST-02; CAT-08 display |
| `shared/components/priority-indicator/priority-rationale.*` (new) | `pa-priority-rationale`: rationale (`criteria.priority.rationale`), rule text, source `criteria.priority.source.expert` — all three required, the source fixed | CAT-02, UX copy G8 |
| `features/configurator/state/catalog-names.ts` (new) | `CatalogNames`, `catalogNames(options)`, `ExternalLink`, `linkTo` (moved from the S10 page, which now imports it; `OptionLink` is an alias) | F2; one place for the domain of a catalog URL |
| `features/configurator/state/criterion-card-rules.ts` (new) | Pure: `cardAnchor`, `procedureTestType`, `requirementTitle`, `sourceTitle`, `metaLine`, `ruleText` | R-LST-01, O-S4-31, O-S4-32, CAT-02 |
| `features/configurator/components/criterion-card/*` (new) | `pa-criterion-card`: H2 (id = anchor), source title, meta line, K5, „Details anzeigen/ausblenden“ (`aria-expanded`, `aria-controls`, `aria-describedby` = title), panel on `surface-container-low` in the DOM with `hidden`, H3 „Warum diese Priorität?“ + `pa-priority-rationale`; „Geprüft nach …“: `testedBy.title`/`.text`, `.meta` with the test type's domain as new-tab link (the text is split around a private-use marker, so the link sits inside the UX copy sentence), `.note` in the panel | R-LST-01, R-LST-02, R-LST-07, O-S4-33 |
| `features/configurator/pages/criteria-list/*` | Loads the list on every entry; `<ul>` of cards when `loaded`; `noRelease` → `/filters` unless the user already left the page; failure keeps H1 and „Auswahl ändern“ | R-LST-01, O-S4-3, O-S4-34 |
| `features/configurator/services/api/catalog-api.service.ts` | `getCriteria(selection, release, callerHandles = SELECTION_CHECK_PROBLEMS)`; `CRITERIA_LIST_PROBLEMS = ['not-found']` | O-S4-34 |
| `features/configurator/state/with-configurator.ts` | `loadCriteriaList()` passes `CRITERIA_LIST_PROBLEMS` | O-S4-34 |
| `features/configurator/models/criteria.ts` | Re-exports `PriorityLevel`/`PRIORITY_LEVELS` from K5 | F8 |
| `features/configurator/testing/criteria-fixture.ts` | `procedureText()` exported | Tests |
| `public/i18n/de.json`, `scripts/check-i18n.mjs` | `criteria.list`, `criteria.item`, `criteria.priority` added to `AREAS`; `--write` copied their texts (with U100) | Row |
| `docs/ux_copy_procureaccess_v18.md` (new; v16 deleted) | U100: `criteria.item.meta.part` „{{productPart}}“, `criteria.item.meta.standard` „EN 301 549 {{number}}“ — proposal, confirmed unchanged by the owner in this pull request (2026-11-11) | O-S4-31 |
| `docs/TESTING_v46.md` (new; v44 deleted) | V139: CAT-02 frontend part, level FE; input documents updated | CAT-02 |
| `docs/design_handoff_procureaccess_v1.md` 1.18 | §5.5: „Geprüft nach …“ panel with the priority section; anchor is the title id | O-S4-33 |
| `docs/figma_code_mapping_procureaccess_v1.md` 1.19 | K5 built (display only), §3 notes for `level`, `Show why button`, rationale; related line | Mapping upkeep |
| `docs/sprints/sprint_04/SPRINT_04.md` | Rows S4-15a/S4-15b; O-S4-28 – O-S4-35 | Split, decisions |
| `CLAUDE.md` | §3 paragraph S4-15a, §4 paths, header; `criteria.list.title`/`.changeSelection` are no longer typed by hand | Doc upkeep |

ARCHITECTURE and SPEC are unchanged: the card and K5 follow ARCHITECTURE §8 and SPEC R-LST-01/02 as written; the contract change for `appliesTo[]` comes with S4-15b.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the staged diff.

| Area | Result |
|---|---|
| Security | No `innerHTML`; catalog text (rationale, titles) only through text interpolation; the test-type link is an Angular `[href]` binding (sanitised) with `rel="noopener noreferrer"` |
| Correctness | The source line cannot be left out (fixed in the template, test `INV:CAT-02`); a rule key missing from the options is shown as it is; the redirect to `/filters` is skipped when the page is gone (mutation-checked) |
| Edge cases | No procedure text → no source title; no product part / no EN 301 549 reference → fragment key or no line; test type without URL → no meta line; weight given for a level other than B is not shown |
| Performance | One request per S11 entry (browser cache for a named release); cards are `OnPush` with computed signals |
| a11y | H1 → H2 per card → H3 in the panel; disclosure with `aria-expanded`/`aria-controls`/`aria-describedby`; the panel stays in the DOM (`hidden`) so `aria-controls` always resolves; focus stays on the toggle; K5 level read as „Priorität: …“; new-tab link with hidden `common.newTab`; axe on every component state (jsdom, contrast in E2E) |
| Licence | No new dependency; SPDX headers on all new files |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | A 404 that answers after the user left S11 still sent them to the legacy filter | **Fixed** (`DestroyRef` flag, test „stays where the user went …“) |
| R2 | `linkTo` keeps a URL with a non-http scheme (`javascript:`) and would show an empty domain | Won't fix: catalog URLs are reviewed repository content, and Angular's URL sanitiser already neutralises such an `href` |
| R3 | A test type without a `testType.<key>` text (a new seeded type) would show the raw key in „Geprüft nach …“ | Won't fix: test types are seeded with fixed keys (`FilterValueSeed`), and the UX copy has all five; a new type comes with its UX copy key |
| R4 | `ngx-translate` interpolates the rationale through `criteria.priority.rationale` („{{rationale}}“) | Won't fix: parameter values are not parsed again, and the output is text interpolation |

No correctness or security bug left that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 373 of 373 pass; 43 new: `priority-indicator.spec.ts` (8), `criterion-card-rules.spec.ts` (18), `criterion-card.spec.ts` (11), `criteria-list.spec.ts` (6, was 1), `catalog-api.service.spec.ts` (1); `with-configurator.spec.ts` checks the S11 marker in an existing test |
| C2 | `npm run lint` (client) | All files pass |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the same as on `main` (S4-14 C3) |
| C4 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs` (5 checked — K5 new), `check-i18n.mjs`, `check-icons.mjs` | Exit 0 each; self-tests of the i18n, Figma and invariant scripts 59 of 59 |
| C5 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1141 / 1141 files |
| C6 | E2E (J1, `s10-reset-submit`, `reflow` reach S11 and now see cards) | Left to CI: a dev stack started outside this session holds the E2E ports locally |
| C7 | Backend | Not touched |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | U100 (`criteria.item.meta.part`, `.standard`) was a proposal | Closed: the owner confirmed the wording unchanged in this pull request (2026-11-11) |
| P2 | S4-15b: catalog sections (Markdown renderer O-S4-30, tabs, examples, „So wird es geprüft“, source line) and `appliesTo[]` in SPEC §8.2 / the endpoint (O-S4-29) | Next task |
| P3 | The K5 toggle „Warum diese Priorität?“ / „Begründung ausblenden“ (Figma 2011:92) has no UX copy key for the hide state | With the first screen that shows K5 outside a detail panel (workspace); a UX copy key then |
| P4 | Before S4-16: no UX copy key for the „Notiz hinzufügen“ button and none for a weight input label (S4-14 P4) | One UX copy proposal before S4-16 |
| P5 | The headers of SPEC (v2.7 line), Handoff and ARCHITECTURE_v32 still name `ux_copy_procureaccess_v16.md`, which this task deletes | Next documentation session (as S4-07) |
| P6 | E2E of the card list on `T.1` (axe in both appearances, reflow at 320 px with an open panel) | S4-18 (reflow list) and J1 to the export (S4-22) |

# S4-17 — Search, priority filter, „Geprüft nach …“, empty state

> **Sprint:** 04, P0 task S4-17 (M2.6, 2.5 points). **Issues:** none. **Requirements / invariants:** R-LST-06 (search with debounce and priority filter, `common.results.shown`, `criteria.list.emptyFilter`), R-LST-07 („Geprüft nach …“ cards: priority B, deselectable, short detail panel), R-LST-08 (empty state for app types without criteria); CAT-20 FE part → complete, CAT-16 S11 deselection part (TESTING_v47 V140, V141).
> **Status:** done — pull request open (see the PR of branch `sprint04/S4-17-search-filter-tested-empty`).
> **Scope:** the row of S4-17: the filter row of S11 (search, priority levels, „Nur aufgenommene Kriterien anzeigen“ from R-LST-04), the result announcement, the empty state of the filter, the notice and empty states for app types without criteria, and tests for the „Geprüft nach …“ cards. Not here: the summary of the selection and the counts (moved to S4-18 by the owner), notices, action bar, live counter and „Kriterium vorschlagen“ (S4-18); the export rows of „Geprüft nach …“ (S4-19/S4-20); E2E of the S11 list (S4-18 reflow list, S4-22 J1).
> **Owner decisions (2026-11-11, all (a), before the plan):** **O-S4-44** — the priority filter is a fieldset „Priorität“ with one checkbox per level, none checked = all; **O-S4-45** — „Filter zurücksetzen“ becomes its own button key `criteria.list.emptyFilter.reset` (UX copy v21 U105, wording unchanged); **O-S4-46** — search and filter are cleared when S11 shows the list of a changed selection and kept for the same one (S4-14 P3); **O-S4-47** — „Auswahl ändern“ in `criteria.list.noCriteriaFor` opens S10 at step 2 through the router state, never the URL; the S11 summary (`criteria.list.summary.*`, `criteria.list.counts`), which no task row named, moves to S4-18. **Defaults accepted with the plan:** R-LST-07 needs tests only; the inclusion filter sits in the filter row; `common.results.shown` only after a change by the user; the notice is the empty state when no criterion is left, the filter row is then left out; `criteria.list.empty` only without app types without criteria; no clear button in the search field (no text for one).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `searchCriteria()` (300 ms debounce), `filterCriteriaByPriority()`, `showOnlyIncludedCriteria()`, `clearCriteriaFilter()` and `visibleCriteria` exist in `withConfigurator()` since S4-14; S11 had no filter UI | A filter row that only emits; the page wires it to the store |
| F2 | The „Geprüft nach …“ cards exist since S4-15a (title, short text, meta link, priority section and `criteria.item.testedBy.note` in the panel), their checkbox and K5 menu since S4-16; the priority B comes from the release. The search already finds them: the catalog title of a `PROC-…` criterion is „Geprüft nach …“ (`titleDe` in the release folders) | R-LST-07 needs tests only; no code change |
| F3 | S11 ignored `appTypesWithoutCriteria[]` and an empty answer | Notice and empty states in the page |
| F4 | The Figma filter row (S11, `2020:70`) shows count text, search field and the inclusion checkbox, no priority filter; UX copy v20 has only the label `criteria.list.filter.priority` (S4-14 F5) | → O-S4-44 |
| F5 | `criteria.list.emptyFilter` held the sentence and the button text „Filter zurücksetzen“ in one string | → O-S4-45, UX copy v21 U105 |
| F6 | UX copy: the link „Auswahl ändern“ of `criteria.list.noCriteriaFor` leads to S10 step 2, but S10 always opened at step 1 and the selection never goes into the URL | → O-S4-47; the step goes into the router state |
| F7 | `common.results.shown` has three plural forms (`.one`, `.zero`), i.e. three keys, and `A11yAnnouncerService` throttles per key: an older count in one form could be read after a newer one in another | A `throttleKey` option, so the forms share one throttle (ARCHITECTURE_v35 C121 (2)) |
| F8 | The summary of the selection (`criteria.list.summary.*`) and `criteria.list.counts` were pushed to „S4-17, S4-18“ by S4-15a, but neither row named them | → moved to S4-18 (owner); SPRINT_04 and TASK_PROMPTS updated |

## 2. Changes

| File | Change | Requirement / decision |
|---|---|---|
| `features/configurator/components/criteria-list-filter/` (new: `.ts`, `.html`, `.scss`, `.spec.ts`) | `pa-criteria-list-filter` (`CriteriaListFilterRow`): group `criteria.list.filter.label` with the search field (`type="search"`, `criteria.list.search`), fieldset `criteria.list.filter.priority` with one `mat-checkbox` per level (`criteria.priority.A|B|Info`, emitted in K5 order), checkbox `criteria.list.filter.included`; shows the state it gets and emits; `focusSearch()`. Stacked below 600 px, wrapping row from 600 px | R-LST-04, R-LST-06; O-S4-44 |
| `features/configurator/pages/criteria-list/criteria-list.{ts,html,scss,spec.ts}` | Filter row above the cards (only when the answer has criteria); `common.results.shown` once per filter change by the user (plural key through `pluralKey`, 1 s throttle under the base key; not on the first load, not when the filter is cleared for a new selection); `criteria.list.emptyFilter` + button `criteria.list.emptyFilter.reset` (clears the filter, focus into the search field); info notice `criteria.list.noCriteriaFor` (names joined by `Intl.ListFormat`, conjunction) with „Auswahl ändern“ → `/kriterien` with `APP_TYPE_STEP_STATE`; `criteria.list.empty` for an empty answer without such app types | R-LST-06 – R-LST-08; O-S4-45, O-S4-47 |
| `features/configurator/state/with-configurator.ts` (+ spec) | `configurator.list.selection`; `loadCriteriaList()` clears search and filter (also a pending search) for another selection; `clearCriteriaFilter` shares the new `clearFilter()` | O-S4-46 |
| `features/configurator/state/criteria-list-rules.ts` (+ spec) | `sameSelection` (use case and each layer as a set) | O-S4-46 |
| `features/configurator/guards/configurator-guards.ts` | `ConfiguratorNavigationState`, `APP_TYPE_STEP_STATE` | O-S4-47 |
| `features/configurator/state/selection-rules.ts` (+ spec) | `openingStep(requested, draft)`: the requested step, never past the first unanswered one; step 1 without a request or for an unknown value | O-S4-47 |
| `features/configurator/pages/criteria-select/criteria-select.{ts,html,spec.ts}` | `firstStep` from `Router.currentNavigation()` state, bound to `pa-stepper` `[selectedIndex]` before it has its steps (no step change, no step announcement; the route change focuses the H1); `currentStep` starts there, so the page title says „Schritt 2 von 4“ | O-S4-47 |
| `core/a11y/a11y-announcer.service.ts` (+ spec) | `AnnounceOptions.throttleKey` | F7 |
| `public/i18n/de.json` | `criteria.list.emptyFilter` without the button text, new `criteria.list.emptyFilter.reset` (through `check-i18n.mjs --write`) | U105 |
| `docs/ux_copy_procureaccess_v21.md` (new; v19 deleted) | U105 | O-S4-45 |
| `docs/TESTING_v47.md` (new; v45 deleted) | V140 CAT-20 FE part, V141 CAT-16 S11 deselection part (level FE), V142 upkeep | Invariants |
| `docs/ARCHITECTURE_v35.md` (new; v33 deleted) | C121; §8.1 row of `withConfigurator()` | O-S4-44 – O-S4-47 |
| `docs/design_handoff_procureaccess_v1.md` (1.21) | §5.1 S11: filter row, empty filter, „Auswahl ändern“ to step 2, empty states | O-S4-44 – O-S4-47 |
| `docs/figma_code_mapping_procureaccess_v1.md` (1.21) | The filter row is part of the S11 frames (no §2 row); the frames lack the priority group | F4 |
| `docs/sprints/sprint_04/SPRINT_04.md`, `TASK_PROMPTS.md` | O-S4-44 – O-S4-47; the summary in the S4-18 row and prompt | F8 |
| `CLAUDE.md` | §3 (S4-17 paragraph, header line), §4 (`components/criteria-list-filter/`) | — |

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (`/engineering:code-review` over the diff against `main`: security, correctness, performance, edge cases, a11y). Sorted per CLAUDE §6.8:

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | Typing the same search text again within 300 ms (e.g. delete and retype a letter) applies an equal query and announces the unchanged count once more | **Won't fix** | The user acted; one extra polite announcement, throttled; comparing filters by value would add code for no visible gain |
| R2 | With the notice shown, the page has two links „Auswahl ändern“ with different targets (title row → step 1, notice → step 2) | **Won't fix** | UX copy names the link so; its purpose is clear from the notice sentence around it (WCAG 2.4.4, in context) |
| R3 | The router state is read once while S10 is created; a crafted history state could ask for any step | **No change needed** | `openingStep` clamps to the first unanswered step and ignores unknown values; the state carries no data, only a step |
| R4 | The Figma S11 frames have no priority filter | **Follow-up** | Figma update by the design owner (P1) |

Checked without finding: no `innerHTML`, no new dependency; every text by key, `de.json` only through `--write`; the announcement effect reads the count untracked, so a change of decisions (include/exclude) does not announce (the live counter is S4-18); the filter cleared for a new selection happens while the list loads, so it is not announced; `[value]` on the search field writes the same string back while typing, which keeps the caret; the filter row is not rendered while loading, so it never shows a stale search text; tokens only, DOM order = visual order.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 450 of 450 pass (423 before); new: filter row (6), S11 page (9), S10 opening step (3), `openingStep` (4), `sameSelection` (2), store (2), announcer (1) |
| C2 | `npm run lint` (client) | All files pass (a first run found three typing findings in the new specs; fixed) |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the known one from `main` |
| C4 | `node scripts/check-invariants.mjs` (TESTING_v47), `check-figma-links.mjs` (5 checked), `check-i18n.mjs` (269 keys, UX copy v21), `check-icons.mjs` (36 icons, no new icon) | Exit 0 each |
| C5 | `reuse lint` | Compliant, 1163 / 1163 files |
| C6 | `e2e/`: `npm run typecheck`, `npm run lint` | Pass (no change there) |
| C7 | E2E on the local E2E stack (dev stack stopped and started again): `configurator/`, `shell/reflow.spec.ts`, `journeys/` in Chromium and Firefox | 50 of 50 pass — J1 runs axe on S11 (now with the filter row) in both appearances, and the reflow check covers S11 at 320 px; WebKit and mobile projects are left to CI |
| C8 | .NET build and tests | Not run: no backend file changed |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | The Figma S11 frames (`2020:70`, `2039:2546`) show no priority filter (R4) | Design owner: add the checkbox group „Priorität“ to the filter row |
| P2 | U105 (`criteria.list.emptyFilter.reset`) is a proposal of Claude | Owner confirms the wording in this pull request |
| P3 | E2E of the filter, the empty filter and the notice on `T.1` (Hardware has no criteria there) | S4-18 (reflow list) and J1 to the export (S4-22) |
| P4 | CAT-16 export part (both export sources) | S4-19/S4-20 |
| P5 | The summary of the selection and the counts above the list | S4-18 (moved by the owner, O-S4-47) |

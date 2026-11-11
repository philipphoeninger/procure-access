# S4-14 — S11 state: criteria client, list draft, search and filter logic

> **Sprint:** 04, P0 task S4-14 (M2.6, 3 points). **Issues:** none. **Requirements / invariants:** prepares R-LST-01 – R-LST-06; R-CON-09 (memory only) extended to the list. No invariant changes status — CAT-08 stays `planned` (step 3, the stored `ConfigurationCriterion`), so there is no new TESTING version. New version `ARCHITECTURE_v33.md` (C119; v31 deleted).
> **Status:** done — pull request #268 open.
> **Scope:** exactly the sprint row plus the „Nur aufgenommene Kriterien anzeigen“ filter of R-LST-04 (owner, 2026-11-11, with the plan): the typed answer of `GET /api/v1/catalog/criteria`, the list state in `withConfigurator()` (include/exclude, override next to the recommendation, weight, note), search with 300 ms debounce and the priority filter as pure functions with tests. No screen, no route change, no UI text, no export request (S4-19 – S4-21).
> **Owner decisions (2026-11-11, before the task):** **O-S4-24 (a)** — decisions are kept for the session by criterion key, also for criteria that leave the list, and apply again when they return. **O-S4-25 (a)** — the priority filter compares the level that applies now (override, else recommendation). **O-S4-26 (a)** — the search looks at what the compact card shows: key/number, title, plain-language title, original titles of the procedure texts; case-insensitive substring, umlauts not folded. **O-S4-27 (a)** — an override from A or Info to B has no weight until the user sets one; the range 1–10 (🔧) is a client setting. **Defaults of the plan accepted:** state under `configurator.list`, keyed by criterion key; S11 loads the list again on entry (C118 (3)); choosing the recommended level counts as reset; note length in UTF-16 code units; 1 000 as a constant (SPEC §7.2, not 🔧).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-12 (#265) built `CatalogApiService.getCriteria()` for the hand-over; its answer was typed `criteria: unknown[]`, and `submitSelection()` keeps no answer (C118 (3): a named release is cached by the browser) | The client call exists; S4-14 types the answer and adds a separate `loadCriteriaList()` for S11 |
| F2 | The answer as built (`CriteriaResponse.cs`, `CriteriaResolver`) differs from the SPEC §8.2 sketch in details: `kind` is `requirement`/`testProcedure`, the procedure texts are strings (Markdown lines joined), `plainLanguage` is null until O-1/K-1 are filled | The client types follow the built answer |
| F3 | The root store still has the legacy `withCriteria()` with the state key `criteria` and the methods `setCriteria`, `getCriterionById` | Everything new sits under `configurator.list`; every method name is new (checked with `grep`) |
| F4 | R-LST-03 asks for the weight of B „default from `PriorityRecommendation.Weight`, editable 1–10“ 🔧; SD23 adopted the value but names no default for an override from A/Info to B | O-S4-27 (a); the range is read from the environment like the criteria-suggestion channel (C110) |
| F5 | UX copy v17 has `criteria.list.filter.priority` „Priorität“ and `criteria.list.filter.included`, but the Handoff S11 filter row lists only count, search field and checkbox | The filter logic takes a set of levels (empty = all); whether the UI offers one or several is left to S4-17 and Figma |
| F6 | The anonymous export body carries per criterion `criterionRevisionId`, `isIncluded`, `overrideLevel?`, `overrideWeight?`, `note?` (ARCHITECTURE §4.4) | The decision fields map one to one onto that body; the mapping comes with the export dialog (S4-21) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `features/configurator/models/criteria.ts` | Full types of the answer: `CriteriaResponse`, `Criterion`, `ProcedureText`, `StandardReference`, `ResolvedPriority`, `MatchedRule`, `PriorityLevel` (+ `PRIORITY_LEVELS`), `CriterionKind`, `RatingName` | Row: typed client |
| `features/configurator/models/criteria-list.ts` (new) | `CriterionDecision` (`included`, `overrideLevel`, `overrideWeight`, `note`), `DEFAULT_DECISION`, `CriterionDecisions` (by key), `NOTE_MAX_LENGTH` = 1 000, `NoteLengthError`, `WeightRange`, `CriteriaListFilter` (`query`, `levels`, `onlyIncluded`), `EMPTY_LIST_FILTER` | R-LST-03 – R-LST-06 |
| `features/configurator/state/criteria-list-rules.ts` (new) | Pure functions: `decisionOf`, `effectiveLevel`, `effectiveWeight`, `hasPriorityOverride`, `setIncluded`, `overridePriority`, `isWeightInRange`, `setWeight`, `resetPriority`, `setNote`, `noteLengthError`, `matchesSearch`, `isFilterActive`, `filterCriteria`. A change returns the same object when nothing changes; a decision equal to the default is removed | Row: logic as pure functions |
| `features/configurator/state/priority-weight-range.ts` (new) | `PRIORITY_WEIGHT_RANGE` injection token, read from `environment.priorityWeight` | 🔧 value from configuration (CLAUDE §6.6) |
| `src/environments/environment.ts`, `environment.prod.ts` | `priorityWeight: { min: 1, max: 10 }` | O-S4-27, SD23 |
| `features/configurator/state/with-configurator.ts` | `configurator.list` (`status`, `answer`, `decisions`, `searchInput`, `filter`); methods `loadCriteriaList`, `setCriterionIncluded`, `overrideCriterionPriority`, `setCriterionWeight`, `resetCriterionPriority`, `setCriterionNote`, `searchCriteria` (300 ms timer), `filterCriteriaByPriority`, `showOnlyIncludedCriteria`, `clearCriteriaFilter`; computed `visibleCriteria`. A decision for a key the loaded list does not hold changes nothing | Row: list state in the draft |
| `features/configurator/testing/criteria-fixture.ts` (new) | `criteriaAnswer()` with A, B (weight 3), Info without plain-language title and a „Geprüft nach …“ criterion; `criterion()` builder | Tests |
| `*.spec.ts` | See §4 | Tests come with the code |
| `docs/ARCHITECTURE_v33.md` (v31 deleted) | C119; §8.1 store table; the header names UX copy v17, Handoff v1.17, TESTING_v45 | Documents follow the code |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-24 – O-S4-27 recorded as decided (a) | Owner decisions |
| `CLAUDE.md` | §3 (S11 list state), §4 (new files), version line | CLAUDE §11 |

### 2.1 Using it (for S4-15 – S4-18)

```ts
const store = inject(ProcureAccessStore);
await store.loadCriteriaList();                   // on entering S11: 'loaded' | 'noRelease' (→ legacy filter) | 'failed'
store.visibleCriteria();                          // criteria under the filter, in catalog order
const decision = decisionOf(store.configurator.list.decisions(), criterion.key);
effectiveLevel(criterion, decision);              // K5 level; criterion.resolvedPriority stays the recommendation
effectiveWeight(criterion, decision);             // `criteria.priority.weight` only when not null
store.overrideCriterionPriority(key, 'B');        // then announce criteria.priority.changed
store.setCriterionWeight(key, 7);                 // ignored outside PRIORITY_WEIGHT_RANGE or when the level is not B
store.resetCriterionPriority(key);                // „Empfehlung wiederherstellen“, shown while hasPriorityOverride(decision)
store.setCriterionIncluded(key, false);           // card stays, „Nicht in Ihrer Liste“
store.setCriterionNote(key, text);                // noteLengthError(text) → errors.field.maxLength { max, current } on submit
store.searchCriteria(text);                       // field value: store.configurator.list.searchInput()
store.filterCriteriaByPriority(['A']); store.showOnlyIncludedCriteria(true); store.clearCriteriaFilter();
```

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the uncommitted diff.

| Area | Result |
|---|---|
| Security | No storage, no HTML; ids only through `HttpParams`; no secrets. The note is kept as plain text — S4-15/S4-16 render it with interpolation only |
| Correctness | Stale list answers and stale errors are ignored (mutation check: dropping the latest-request guard fails „applies only the answer of the latest request“); the recommendation is never written (tests compare `resolvedPriority` after an override); `setWeight` refuses NaN, fractions and values outside the range |
| Edge cases | Empty or whitespace query shows everything; umlauts are not folded (test); a criterion without plain-language title and a „Geprüft nach …“ criterion are searchable; a note of 501 emoji counts 1 002 (UTF-16); decisions survive two selection changes and apply again when a criterion returns |
| Performance | One request per S11 entry, served from the browser cache after the hand-over (`max-age=3600` for a named release); filtering is linear over ≤ 200 criteria and runs once per applied query or decision |
| a11y, UI texts | No UI; `noteLengthError` returns exactly the parameters of `errors.field.maxLength`; the announcement `common.results.shown` and the counter `criteria.list.status.count` come with the screens |
| Licence | No new dependency; SPDX headers on all new files |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The criteria request carries `CALLER_HANDLES_PROBLEMS` for `validation`, `selection-excluded` and `not-found`, so a 400 on S11 (a selection S10 had sent successfully, e.g. a release changed in between — practically impossible with the pinned release) gets no snackbar; `loadCriteriaList()` returns `failed` | Follow-up P1 (S4-15/S4-18): S11 must show `failed` itself |
| R2 | `answer` still holds the list of the previous selection while the user is back on S10; `visibleCriteria` would show it if a page read it before `loadCriteriaList()` | Won't fix: S11 calls `loadCriteriaList()` on every entry, which clears `answer` first; documented in §2.1 |
| R3 | The filter (search, levels, inclusion) is kept when the selection changes | Won't fix: the filter is visible on S11 and the user set it; S4-17 may clear it on a new selection if the screen needs it (P3) |
| R4 | The overrides allow a level change for „Geprüft nach …“ criteria; the Handoff says only „priority B, deselectable“ | Follow-up P2 (S4-15/S4-16): the rules accept it, the UI decides whether K5 offers the menu there |
| R5 | `matchesSearch` also matches the key `PROC-<testType>`, which no card shows | Won't fix: O-S4-26 names the key; a search for „proc“ is unlikely |

No correctness or security bug that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 330 of 330 pass; 35 new: `criteria-list-rules.spec.ts` (24), `with-configurator.spec.ts` (11 new, among them the storage-blocked test now also covering list decisions); `catalog-api.service.spec.ts` uses the typed answer |
| C2 | `npm run lint` (client); `npm run lint` in `e2e/` | All files pass |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the same on `main` (checked with the changes stashed) |
| C4 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Exit 0 each |
| C5 | `node scripts/check-release-config.mjs` and its self-test | The check still fails only on the O-6 placeholder (expected before a production release); the new `priorityWeight` line does not disturb it; self-test 6 of 6 |
| C6 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1123 / 1123 files |
| C7 | Backend, E2E | Not touched; left to CI |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | A failed S11 load (`failed`, R1) has no text yet; UX copy has `criteria.list.empty` only for an empty answer | S4-15/S4-18: show the failure with existing texts (as O-S4-18 for S10) or propose a key |
| P2 | Whether K5 on a „Geprüft nach …“ card offers the priority menu (R4) | S4-15/S4-16, from Handoff §5.1 S11 and the Figma frame |
| P3 | Whether the filter is cleared when S11 shows a list for a new selection (R3) | S4-17 with the filter UI |
| P4 | UX copy v17 has no label for a weight input (`criteria.priority.weight` „Gewichtung {{weight}}“ is display text) | S4-16 needs a key; a UX copy proposal before S4-16 |
| P5 | The export server (S4-19) must count the note as `string.Length` (UTF-16 code units) and check the same weight range as `priorityWeight` | S4-19/S4-20: one 🔧 setting on the server, the same value as the client |

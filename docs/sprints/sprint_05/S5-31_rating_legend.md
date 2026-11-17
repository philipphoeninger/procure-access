# S5-31 — Rating legend „Was bedeuten die Bewertungsstufen?“

> **Sprint:** 05 · **Task:** S5-31 (stretch, 1 point) · **Issues:** #279
> **Requirements / invariants:** SPEC R-LST-01 (detail panel per Handoff); A11Y-01 stays green (no invariant flipped, no new TESTING version); UX copy U24; Handoff 1.31 §4.11, §5.5; Figma mapping v1.26
> **Status:** done — pull request #374 open (2026-11-17)
> **Scope:** the component `pa-rating-legend` (Handoff §4.11, Figma `2099:760`) and its one collapsed instance above the list on S11, a saved list and S16; the levels it explains; the texts of `assessment.legend`. Client only: no server change, no EF migration, no new dependency.
> **Owner decisions:** **#279 (b)** (2026-11-12, in the issue) — the legend appears once per page, above the list, not in each detail panel; „Wie wird bewertet?“ keeps listing the card's levels and gets no legend of its own · **O-S5-31-1** (2026-11-17) — the legend explains every level at least one criterion of the *whole* list allows, in scale order, so the filters never change it; „Nicht eingeschätzt“ never appears there · **O-S5-31-2** (2026-11-17) — the same legend on S11, a saved list (`/kriterienlisten/:id`) and S16 · confirmed with the plan (2026-11-17): a list whose criteria allow no level („Geprüft nach …“ only) shows no legend; „Wie wird bewertet?“ does not link to the legend; the open state is not kept; the U24 texts are built as written (proposals, K-8 / O19).
>
> The task prompt still described the legend inside „Wie wird bewertet?“ and named no owner decision; the decisions above replace that (owner, 2026-11-17). In the question before the task the saved list was called „S14“ by mistake: S14 is the overview of saved lists, which has no criterion cards and stays unchanged.

## 1. Starting point

- `pa-rating-legend` did not exist. `scripts/check-figma-links.mjs` already mapped `shared/components/rating-legend/rating-legend.ts` to `2099-760` and checked nothing while the file was missing.
- Figma: the S11 frame `2020:70` has the instance `2099:3165` „pa-rating-legend (einmal je Seite, über der Liste)“ between the filter row and the list, 24 px apart, full width, collapsed (moved there with #283). The component `2099:760` matches Handoff §4.11: text button, `<dl>` with a 168 px term column, footnote.
- Handoff §4.11 still placed the S11 legend in the detail section „Wie wird bewertet?“, and its `values` row spoke of one criterion's levels.
- The `assessment.value.*.explain` texts were in `de.json` (area `assessment.value`); `assessment.legend.show|hide|footnote` were in UX copy v39 but their area was not in `AREAS`.
- Each criterion of `GET /api/v1/catalog/criteria` carries `allowedRatings`, the server's union over the procedure texts of the selection (`CriteriaResolver`): none for „Geprüft nach …“, all six for a requirement without a procedure text of the selection. `RatingName` on the client has the six levels without `unknown`.

## 2. Changes

### 2.1 Client

- **`shared/components/rating-legend/`** — `RatingLegend` (`pa-rating-legend`, `@figma 2099-760`). A `matButton` text button `assessment.legend.show|hide` with `aria-expanded` and `aria-controls`; the panel follows it directly and stays in the DOM while `hidden` (as the card's detail panel). Panel: `surface-container-low`, `corner-small`, padding md; `<dl>` of `assessment.value.<level>` (label/medium) and `assessment.value.<level>.explain` (body/medium), stacked below 600 px and in a 10.5rem term column from 600 px; then `assessment.legend.footnote` (body/small, on-surface-variant). Inputs `expanded` (`model(false)`) and `values` (all seven by default). The component exports `ASSESSMENT_VALUES` and `AssessmentValue` (the seven values of `assessment.value.*`), so `shared/` does not import the configurator's `RatingName`; levels always show in scale order.
- **`ratingsOfList(criteria)`** in `features/configurator/state/criterion-detail-rules.ts` — the union of the criteria's `allowedRatings` in scale order (O-S5-31-1).
- **S11** (`criteria-list`) — one `<pa-rating-legend [values]="ratings()">` between the filter row and the cards, from the whole answer; shown also when the filter leaves no card, left out when `ratings()` is empty.
- **Saved list** (`saved-list-page`) — the same after the filter row, for every role.
- **S16** (`shared-list-page`) — the same after the counts.
- **Texts** — `assessment.legend` joined `AREAS` of `scripts/check-i18n.mjs`; `--write` added the three keys to `de.json`.

### 2.2 Tests

- `rating-legend.spec.ts` (4): closed state with button name, `aria-expanded`, `aria-controls` → hidden panel, axe; open with all seven levels in order, seven explanations, the footnote and the panel directly after the button, axe; closing again; `values` limited and shown in scale order.
- `criterion-detail-rules.spec.ts` (+2): the union in scale order without duplicates; empty for no criteria and for „Geprüft nach …“ only.
- `criteria-list.spec.ts` (+3): one collapsed legend between the filter row and the list with the union, axe when open; kept with its levels when the search leaves no card; left out for a list of „Geprüft nach …“ only.
- `saved-list-page.spec.ts` (+2, Reader and Editor) and `shared-list-page.spec.ts` (+1): one collapsed legend in place with the union, axe when open.
- E2E: `configurator/s11-list.spec.ts` gets `INV:A11Y-01 INV:A11Y-08 explains the rating levels once, between the filter row and the cards (S5-31)` — opened through CSS locators, axe in both appearances before any name-based locator (Firefox, TESTING V129), then the texts and the order filter row → legend → cards. `shell/reflow.spec.ts` opens the legend on S11, the editable saved list and S16 at 320 px.

### 2.3 Documents

- **Handoff 1.31** — §4.11: the `values` row and the placement (S11, a saved list and S16 above the list, after the filter row where there is one; the union of the list; left out without levels); §5.5: „Wie wird bewertet?“ has no legend of its own; the sources name Figma mapping 1.26.
- **Figma mapping v1.26** — §2 status of `pa-rating-legend` „built (S5-31)“; the related line names the current versions.
- **`CODEBASE_v1.md`** — §2.82 and the `rating-legend/` folder in §3.
- **`CLAUDE.md`** — §3 state of the code and „Last task reflected: S5-31“ (38 046 characters).
- Not changed: the UX copy row U24 still names „S11 (Details)“ as a place of the legend. It is a change-history row of the owner's German document, so it stays as written (open point 2). SPEC, ARCHITECTURE and TESTING need no change: R-LST-01 points to the Handoff, and no invariant is flipped.

## 3. Self-review

CLAUDE §6.7 checklist and `/engineering:code-review` over the diff; findings sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | On a saved list the page stays when only the list id changes (S5-21b); an open legend then stays open on the next list | **won't fix** — harmless, the levels follow the new list; nothing is stored |
| R2 | S16 shows `criteria.list.empty` for a list of only „Geprüft nach …“ criteria, because its list sits inside `counts().total > 0`, and the counts leave those criteria out (O-S4-51). Existed before S5-31; the legend inherits the gate, which is right here (no level) | **follow-up issue** #375 — open point 1 |
| R3 | The union uses the criterion-level `allowedRatings`, which the server sets to all six for a requirement without a procedure text of the selection; such a card's „Wie wird bewertet?“ lists nothing, the legend all six | **won't fix** — `allowedRatings` is the SPEC §8.2 contract and the base of K6 later; with T.1 and 2026.1 every requirement has a text for the app types that select it |
| R4 | Security: new input, injection, data | **ok** — static texts by key, no request, no storage |
| R5 | Performance | **ok** — one `computed` per page over the list (≤ a few hundred criteria × six levels) |
| R6 | Accessibility | **ok** — disclosure with `aria-expanded`/`aria-controls`, panel right after the button, `<dl>`, no tooltip, button ≥ 48 px through `matButton`, axe in both appearances (component, page and E2E), reflow at 320 px on all three pages |

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test` | 822 passed (12 new: component 4, `ratingsOfList` 2, S11 3, saved list 2, S16 1) |
| `npm run lint` (client), `npm run typecheck && npm run lint` (e2e) | clean |
| `npm run build:release` | built; the known critical-CSS warning „Could not parse 1 selector“ only (S2-03) |
| E2E, touched specs (`s11-list.spec.ts`, the reflow tests of S11, the saved list and S16), Chromium and Firefox | 18 / 18 passed |
| E2E, full suite in Chromium | 140 passed, 3 failed for the environment only (below); WebKit and mobile Safari in CI only |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | passed, 7 checked — `rating-legend.ts` now among them (a wrong node made it fail, checked) |
| `node scripts/check-i18n.mjs`, `check-icons.mjs`; `node --test scripts/*.test.mjs` | passed; 101 / 101 |
| `reuse lint` | compliant |
| Figma | compared after building: the open legend at 1280 px and 360 px and in the dark appearance matches `2099:760` (button, 168 px term column, explanations, footnote) and its place in `2020:70` |

**How E2E ran locally:** the disk had 2.1 GB free, too little for a rebuild of both images without build cache, so the E2E stack ran from its images of the same day (API = `main`, this task changes no server code) and the client of this branch was served with `ng serve --port 4300` (`E2E_BASE_URL`). The three failures belong to that setup: S03 and J8 compare the mailed link's origin (the API's 4200) with the page (4300), and J5's idle limit cannot pass while the dev server's health watcher renews the session (CLAUDE §3). CI runs them against the built client image.

## 5. Open points

1. **S16 with „Geprüft nach …“ criteria only (R2)** — issue #375 (M3): S16 shows `criteria.list.empty` although the list has cards, because its list sits inside `counts().total > 0`; S11 and the saved list show them. Gate the S16 list by the number of criteria instead.
2. **UX copy U24** names „S11 (Details)“ among the places of the legend; with #279 (b) it is above the list on S11, a saved list and S16. For the next UX copy version, if the owner wants the row to follow (Handoff 1.31 already does).
3. The task fitted one session.

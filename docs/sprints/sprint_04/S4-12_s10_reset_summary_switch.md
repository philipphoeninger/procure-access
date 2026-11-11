# S4-12 — S10 reset notice, „Bisherige Auswahl“, hand-over to S11, legacy switch

> **Sprint:** 04, P0 task S4-12 (M2.5, 3 points). **Issues:** none. **Requirements / invariants:** R-CON-07, R-CON-08, R-CON-10; **CFG-01 → `active`**, **CFG-04** complete with its submit part (`TESTING_v44.md` V130 – V133; v42 deleted); A11Y-03 extended to the page-title state; A11Y-01 and A11Y-08 on the new states. Also S4-09 P1 (catalog 404 without snackbar), S4-10 P1/P5 and S4-11 P1 (explanation of `noRelease`/`failed`, page title and „Fehler:“ prefix).
> **Status:** done — pull request open.
> **Scope:** the reset notice in a status region; „Bisherige Auswahl“ with „Ändern“ per layer; „Kriterienliste anzeigen“ with the server check, the button spinner and a placeholder S11 at `/kriterien/ergebnis`; the per-step page title and the „Fehler:“ prefix; the switch of O-S4-3 in one place; the configurator's legacy `LiveAnnouncer`. Not in scope: J1 on S10 and S10 in `reflow.spec.ts` (S4-13), the S11 list (S4-14 – S4-18).
> **Owner decisions (2026-11-11, before the start):** **O-S4-17 (a)** — start page and header always link `/kriterien`; a guard of S10 opens the legacy filter when the filter options answer 404; **O-S4-18 (a)** — no new text for failed filter options (5xx → server page, no answer → offline notice); **O-S4-19 (a)** — „Kriterienliste anzeigen“ calls the criteria endpoint with a button spinner, accepted → placeholder S11, refused → error at the step; **O-S4-20 (a)** — the reset notice only when a lower layer held a selection. Defaults accepted with them: S11 placeholder with H1 and „Auswahl ändern“, selection never in the URL; one „Ändern“ per layer (SPEC over the UX copy's one-button hint), empty layers left out, values joined by `Intl.ListFormat`; the step number in the title without a second announcement; no S10 empty states; only the legacy filter's `LiveAnnouncer`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-11 (#263) is merged. The store already reports what a change cleared (`reset`, only when a lower layer held a selection) and drops it with the next selection (S4-09) | The notice reads the store; O-S4-20 (a) needs no rule change |
| F2 | Start page and header linked `/filters`; `/kriterien` was not linked; the error interceptor gave every 404 the generic snackbar (S4-09 P1) | → O-S4-17; request marker `CALLER_HANDLES_PROBLEMS` |
| F3 | No UX copy text for failed filter options on S10 (S4-10 P1, S4-11 P1) | → O-S4-18 (a): the system states explain it |
| F4 | `PageTitleStrategy` took only a fixed key per route: no `{{n}}` for `pageTitle.criteria.step`, no „Fehler:“ prefix (O-S4-13) | Page-title state (§2) |
| F5 | The app provided the strategy with `useClass`, so a page injecting `PageTitleStrategy` would get a second instance | `useExisting` |
| F6 | The legacy filter injected `LiveAnnouncer` but never called it; the two real legacy calls (products, proposals) belong to steps 5 and 3 | The unused injection goes; TECH_DEBT_v3 §5 counts two calls from now on |
| F7 | UX copy §5.4 describes „Bisherige Auswahl“ as one line with one „Ändern“; SPEC R-CON-08 wants „Ändern“ per layer; Figma S10-R shows one layer (text + button), no visible heading | SPEC wins on behaviour (CLAUDE §2); one entry per layer with the same wording, no missing key |
| F8 | The loading bar never showed for a configurator load (found by the E2E slow-answer test once S10's guard made the request): each feature that adds `withLoading()` gets a new `loadingCount` signal in NgRx Signals 22, and the counter methods read their own, out-of-date signal — after the legacy loads at start the shared counter stood at −1, so the configurator's +1 gave 0 | Fixed in `withLoading()` (§3 R1); it existed since S4-09 |
| F9 | S11's planned path is `features/configurator/pages/criteria-list/` (Figma mapping §2.2); the legacy criteria list holds the selector `pa-criteria-list` | Placeholder at the planned path, selector `pa-criteria-list-page` |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `features/configurator/guards/configurator-guards.ts` (new) | `configuratorReleaseGuard`: waits for `filterOptionsReady()`; `noRelease` → `/filters`, else S10. `completeSelectionGuard`: S11 only with a complete draft, else `/kriterien`. Path constants `LEGACY_FILTER_PATH`, `CONFIGURATOR_PATH`, `CRITERIA_LIST_PATH` | O-S4-17, R-CON-09, R-CON-10 |
| `app.routes.ts` | `/kriterien` with the guard and `pageTitle.criteria.step`; `/kriterien/ergebnis` → S11 placeholder with `pageTitle.criteria.list` | O-S4-17, O-S4-19 |
| `pages/home/home.{ts,html}`, `layout/main-nav/main-nav.{ts,html}` | The criteria card and „Kriterien“ link `/kriterien`; the nav item is current on `/kriterien…` and on `/filters` | O-S4-17 |
| `core/errors/problem-details.ts`, `core/interceptors/error.interceptor.ts` | `CALLER_HANDLES_PROBLEMS` (`HttpContextToken<readonly string[]>`): no snackbar for the problem types named; `problemFieldErrors()` | S4-09 P1, O-S4-19 |
| `features/configurator/services/api/catalog-api.service.ts`, `models/criteria.ts` (new), `models/configurator-draft.ts` | Filter options marked `not-found`; `getCriteria(selection, release)` with `testTypeIds[]`, marked `validation`, `selection-excluded`, `not-found`; `CriteriaSelection`; `CriteriaResponse` with `criteria: unknown[]` (typed with S4-14) | R-CON-10 |
| `features/configurator/state/selection-rules.ts` | `toCriteriaSelection`, `selectedNames` (values of a layer in option order), `layerOfField` (server path → layer) | R-CON-08, R-CON-10 |
| `features/configurator/state/with-configurator.ts` | `filterOptionsReady()` (loads when not loaded, failed or `noRelease`; waits for a running load); `submitSelection()` → `accepted` / `rejected` (problem, layer, message) / `noRelease` / `failed` | O-S4-17, O-S4-19 |
| `features/configurator/pages/criteria-select/criteria-select.{ts,html,scss}` | Each step renders a `role="status"` region with the reset notice (`pa-notice` attention, title + text of the changed layer); from step 2 the list „Bisherige Auswahl“ (`criteria.select.summary` per earlier step, „Ändern“ described by it, jumps to the step); step 4 „Kriterienliste anzeigen“ with 20 px spinner and `aria-disabled` while checking, then S11 / error at the step (focus into the field or the first available option, as S10-E) / legacy filter; the title state follows the open step and the errors; `@figma` line of S10-R | R-CON-07, R-CON-08, R-CON-10, Handoff §5.4, §6.2 |
| `features/configurator/pages/criteria-list/criteria-list.{ts,html,scss}` (new) | S11 placeholder: H1 `criteria.list.title`, „Auswahl ändern“ → `/kriterien`; `@figma` `2020-70`, `2039-2546` | O-S4-19 |
| `core/a11y/page-title.strategy.ts`, `app.config.ts` | `setPageTitleState({ params, error })`; cleared on a route change; the route change announces the title the page filled while rendering (`settledTitle`); provided with `useExisting` | UX copy §5.16, §6.1; O-S4-13 |
| `shared/state/with-loading.ts` | Counter methods use the updater form of `patchState` | F8 |
| `features/filters/selection/filters-selection.ts` | Unused `LiveAnnouncer` removed (the file stays in `LEGACY_EXCEPTIONS`: not deleted) | Row, F6 |
| `public/i18n/de.json` | `pageTitle.criteria.step`, `pageTitle.criteria.list`, `criteria.list.title`, `criteria.list.changeSelection` by hand (not `AREAS` roots; check 2 compares them with UX copy v17) | Texts |
| `testing/shell-test-doubles.ts` | Test routes `kriterien`, `kriterien/ergebnis` | Tests |
| `*.spec.ts` | 53 new unit tests (295 in all): guards, interceptor marker, API client, rules, store, page-title state, S10 reset/summary/submit/title, S11 placeholder, nav, `withLoading` | Tests with the code |
| `e2e/configurator/s10-reset-submit.spec.ts` (new) | T.1 by keyboard: reset via „Ändern“ (status region, focus kept, notice goes with the next selection), „Bisherige Auswahl“, submit without test type → S11 → „Auswahl ändern“, S11 without selection → S10, the switch (404 faked) without snackbar; axe in both appearances | CFG-01, CFG-04 (E), A11Y-01, A11Y-08 |
| `e2e/shell/start-and-error-pages.spec.ts`, `e2e/shell/reflow.spec.ts` | Start card → `/kriterien`; the offline notice found by its text (S10 has status regions too); server error and slow answer happen before S10 opens (URL stays `/`; the S10 title is announced after the answer) | V133 |
| `docs/TESTING_v44.md` (new; v42 deleted) | V130 CFG-01 `active`; V131 CFG-04 complete; V132 A11Y-03 page-title state; V133 E2E notes | Invariant flip |
| `docs/ARCHITECTURE_v32.md` (new; v30 deleted) | C118, §8.6 | Structure |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.18: S10-R line, S11 placeholder checked | Mapping |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-17 – O-S4-20 decided | Decisions |
| `CLAUDE.md` | Header; §3 S4-12 paragraph and the old `/filters` / „not linked“ wording; §4 `guards/`, `pages/criteria-list/` | DoD |

SPEC is unchanged: no requirement, contract or open question moves.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff against `main`.

| Area | Result |
|---|---|
| Security | No new endpoint or storage; the selection stays in memory and is never put in the URL; the server message of a `validation` problem is shown through text interpolation (escaped) |
| Correctness | One switch (the guard); the submit sends once while checking; a refused selection opens its step and the error goes with the next change; the title state is cleared by every route change; every `incrementLoadingCount` has its `decrementLoadingCount` in `finally`/`finalize`, so the corrected counter cannot stay above zero |
| Edge cases | Reset with nothing to clear → no notice; „Keiner davon passt“ in the summary; S11 after a reload → S10; 404 on submit → legacy filter; 5xx/offline → system state, page stays; a path the server names without a layer → error on step 4 |
| Performance | One extra request per submit (the criteria, cached by the browser for the named release, §9.6); the guard reuses a running filter-options request |
| a11y | Status region exists before the notice; no focus move on reset; „Ändern“ described by its entry; spinner `aria-hidden`, button `aria-disabled` with its label kept; field errors as S10-E (focus into the field); route change announces the filled title; axe clean in unit tests and E2E in both appearances |
| UI texts | Only UX copy keys; no new wording |
| Licence | No new dependency (`MatProgressSpinner` is part of Angular Material); SPDX headers on the new files; `reuse lint` compliant |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The loading bar never showed for configurator loads (F8) | **Fixed** (affects behaviour today, R-SHL-09): updater form in `withLoading()`; `with-loading.spec.ts` fails without it. Outside the row's files, but the E2E test of the S10 loading state needs it |
| R2 | My first edit inserted the two page effects twice (also into `fitPanelToVisualViewport`) | **Fixed** before the tests went green |
| R3 | A `validation` problem without any field error would show an empty error on step 4 | **Won't fix:** SPEC §8.2 always sends field errors for both 400 types, and the client only sends complete selections |
| R4 | The page's title parameters rely on the router calling `updateTitle` before the page's first effect | **Won't fix:** that is the router's order; covered by `page-title.strategy.spec.ts` (real router) and the E2E announcement „… Schritt 1 von 4 …“ |
| R5 | After a failed guard load S10's constructor asks for the options once more | **Won't fix:** the same retry-on-open as since S4-10; one request |
| R6 | `layout/main-nav` imports path constants from `features/configurator/guards/` | **Won't fix:** constants only; they go with the legacy filter |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test --watch=false` (Vitest) | 295 of 295 pass (242 before) |
| C2 | `npm run lint` (client); `npm run typecheck && npm run lint` (`e2e/`) | Pass |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the same on `main` |
| C4 | E2E on the E2E stack (dev stack stopped and started again): `configurator/ shell/ smoke/ journeys/` in Chromium and Firefox; `configurator/` three times in Chromium, Firefox and mobile-chrome | Last full run 94 of 96: the two failures were timing in the new spec (ArrowDown before the list was filtered; axe during a colour transition), fixed; then `configurator/` 36 of 36 three times. The shell, smoke and journey tests passed in that full run. WebKit and mobile-safari left to CI |
| C5 | `node scripts/check-invariants.mjs` | Passed (CFG-01 in unit and E2E tests) |
| C6 | `check-figma-links.mjs` (4 checked, S11 now among them), `check-i18n.mjs` (0 errors), `check-icons.mjs` | Pass |
| C7 | `reuse lint` | Compliant |
| C8 | Backend | Not touched; left to CI |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | S10 (with reset notice and „Bisherige Auswahl“) and the S11 placeholder are not in `reflow.spec.ts` yet | S4-13 (row) |
| P2 | No visual comparison of the reset state and „Bisherige Auswahl“ with the Figma frame S10-R in this task (the E2E runs axe on both, not a pixel check) | With the S10 check before M2 exit |
| P3 | Screen-reader check of the reset notice (status region inside the fieldset) and of „Ändern“ with its description | Accessibility tester, with S4-11 P2 |
| P4 | TECH_DEBT_v3 §5 counts three legacy `LiveAnnouncer` calls; two remain (products list, proposal list) | `TECH_DEBT_v4.md` at the sprint end |
| P5 | The `validation` text on S10 is the server's English message (`errors.field.server`); the API sends no codes yet (TECH_DEBT_v3 §5) | Each step's module validation |
| P6 | „Auswahl ändern“ on the S11 placeholder opens S10 on step 1 with the selection kept, not on the last step | S4-18 (action bar of S11) |

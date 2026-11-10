# S4-09 — Configurator state: in-memory draft, catalog client, selection rules

> **Sprint:** 04, P0 task S4-09 (M2.5, 3 points). **Issues:** none. **Requirements / invariants:** R-CON-09; prepares R-CON-01 – R-CON-08, R-CON-10, CFG-01 – CFG-04 and the configurator part of CAT-12. No invariant changes status — CFG-01 – CFG-04 check the page (`role="status"`, `aria-disabled`, combobox, submit button) and flip with S4-10 – S4-12 — so there is no new TESTING version. New version `ARCHITECTURE_v30.md` (C116; v28 deleted).
> **Status:** done — pull request open.
> **Scope:** exactly the sprint row: `withConfigurator()` with the four-layer draft in memory, the typed client of `GET /api/v1/catalog/filter-options`, and the selection rules as pure functions with tests, under `features/configurator/`. No screen, no route, no UI text, no criteria call (S4-14), no switch between S10 and the legacy filter (S4-12).
> **Owner decisions (2026-11-10):** **O-S4-8 (a)** — `withConfigurator()` is a slice of the root store and holds the draft and the filter options answered for it; `withCatalog()` comes when the legacy filter goes. **Defaults of the plan accepted:** the draft pins the release of the first answer (ADR-002); the options load again after each change of steps 1–3, the latest answer wins; any change of a layer clears every lower layer except Basisfunktionalität (CFG-01) and reports the changed layer and the cleared layers that held a selection; „Keiner davon passt“ is its own state; the always-selected parts come from `alwaysSelected`; no new dependency.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-08 (#260) is merged: dev and E2E stacks serve the active release `T.1`; staging answers 404 (O-S4-3) | The client must keep 404 apart from other failures, so S4-12 can switch to the legacy filter |
| F2 | The root store `ProcureAccessStore` (`core/state/app.store.ts`) already has the legacy members `appTypes`, `productParts`, `testTypes` (computed in `withFilters()`) | All configurator state sits under one key `configurator`; method names do not overlap with legacy ones |
| F3 | The server reads `useCase`, `appTypeIds[]`, `productPartIds[]`, `release` (`FilterOptionsResolver` constants) and counts an always-selected part as selected only when it is sent (SPEC §8.1) | The draft carries Basisfunktionalität explicitly; the client tests check the parameter names |
| F4 | `excludedBy[]` is computed by the server for the selection it receives; a request without a selection gives empty `excludedBy` everywhere | The options load again after every change of a layer that can exclude (steps 1–3) |
| F5 | The new endpoints answer plain JSON, not the legacy `TResult` wrapper; the legacy API services use constructor injection | The new client uses `inject()` and types the plain answer |
| F6 | UX copy v16 has one reset text per **changed** layer (`criteria.select.reset.text.useCase|appType|productPart`) and reason texts only for exclusions by use case and app type (`criteria.select.excluded.*`) | The reset report names the changed layer; the reason helper groups by all three layers (see P3) |
| F7 | Release 2026.1 excludes only use case → app type, app type → test type and use case → test type; `T.1` also has product-part exclusions (MiniCatalog: all six pairs) | No product-part reason text is needed for the real release yet (P3) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `features/configurator/models/filter-options.ts` (new) | Types of the §8.1 answer: `FilterOptionsResponse`, `FilterOptionsLayers`, the four option types, `ExcludingValue` with `ExcludingLayer` | Typed client |
| `features/configurator/models/configurator-draft.ts` (new) | `ConfiguratorDraft` (use case as `unanswered` / `none` / `useCase`, three id lists), `EMPTY_DRAFT`, `CONFIGURATOR_LAYERS`, `ResetReport`, `StepError` (`noUseCase`, `noAppType`), `CatalogSelection` | R-CON-02, R-CON-07 |
| `features/configurator/services/api/catalog-api.service.ts` (new) | `CatalogApiService.getFilterOptions(selection, release?)` → `GET {API_URL}/v1/catalog/filter-options`; S4-14 adds the criteria call here | Row: typed client |
| `features/configurator/state/selection-rules.ts` (new) | Pure functions: `chooseUseCase`, `toggleAppType`, `toggleProductPart`, `toggleTestType` (each returns the new draft and a `ResetReport` or null; an unknown or excluded option, an always-selected part and the same use case again return the same draft object), `withAlwaysSelectedParts`, `alwaysSelectedPartIds`, `isExcluded`, `exclusionReasons`, `stepError`, `isComplete`, `toCatalogSelection`. Ids keep the order of the options, so equal selections send equal requests | Row: selection rules |
| `features/configurator/state/with-configurator.ts` (new) | Slice with state `configurator` = `release`, `draft`, `options`, `optionsStatus` (`notLoaded`, `loading`, `loaded`, `noRelease`, `failed`), `changedSinceAnswer`, `reset`; methods `loadFilterOptions`, `chooseUseCase`, `toggleAppType`, `toggleProductPart`, `toggleTestType`; computed `configuratorComplete`. Pins the release of the first answer; reloads after each change of steps 1–3; applies only the latest answer; refuses choices below a changed layer until its answer arrived (their `excludedBy` would be out of date); keeps `reset` until the next selection; uses the `withLoading()` counter | R-CON-07, R-CON-09, O-S4-8 (a) |
| `features/configurator/testing/filter-options-fixture.ts` (new) | Small option set and `excluded()` for the tests | Tests |
| `*.spec.ts` (three new) | See §4 | Tests come with the code |
| `core/state/app.store.ts` | `withConfigurator()` added to the root store | O-S4-8 (a) |
| `docs/ARCHITECTURE_v30.md` (new; v28 deleted) | C116; §8.1 rows of `withConfigurator()` and `withCatalog()` | Decision and behaviour recorded |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-8 in the owner-decision table | Decision recorded |
| `CLAUDE.md` | Header, §3 (configurator state), §4 (`features/configurator/`) | Structure changed |

### 2.1 Using it (for S4-10 – S4-12)

```ts
const store = inject(ProcureAccessStore);
void store.loadFilterOptions();                       // on entering S10; noRelease → legacy filter (S4-12)
store.chooseUseCase({ kind: 'useCase', id: 'hr' });   // or { kind: 'none' }
store.toggleAppType('web');
stepError('appType', store.configurator.draft());     // 'noAppType' → criteria.select.error.noAppType
store.configurator.reset();                           // { changedLayer, clearedLayers } → criteria.select.reset.text.<changedLayer>
exclusionReasons(option);                             // { useCase: [...], appType: [...], productPart: [...] } → criteria.select.excluded.*
```

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the staged diff.

| Area | Result |
|---|---|
| Security | No storage, no user input in URLs except ids through `HttpParams` (encoded); no secrets |
| Correctness | Stale answers and stale errors are ignored; the applied answer always belongs to the current draft, because every draft change that alters the request sends a newer one (test-type toggles do not change the request). Mutation check: dropping the latest-request guard fails one test, dropping the clearing of step 4 fails nine |
| Edge cases | Choices before the first answer and after a failed reload are refused; retry through `loadFilterOptions()`; a removed last app type still resets the lower layers; „Keiner davon passt“ sends no `useCase` |
| Performance | One request per change of steps 1–3; the lists hold ≤ 10 options, so the linear lookups do not matter. With the pinned release the answers are cacheable (`max-age=3600`, ETag, S3-19) |
| a11y, UI texts | No UI; the codes `noUseCase`/`noAppType` and the reset layers map to existing UX keys |
| Licence | No new dependency; SPDX headers on all new files |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The error interceptor gives a 404 of the filter options the generic error snackbar. When S4-12 uses `noRelease` to choose the legacy filter, staging would show an error on every visit | Follow-up P1 (S4-12); nothing calls the client today |
| R2 | Every reload runs the `withLoading()` counter, and the loading bar announces `common.loading` each time loading starts — on S10 that would be one announcement per checkbox in steps 2–3 | Follow-up P2 (S4-10/S4-11); no screen today |
| R3 | A choice below a changed layer is refused while its reload runs; the screen must make that visible (e.g. „Weiter“ waits for the options), or the click seems lost | Follow-up P2 (S4-10/S4-11) |
| R4 | The test helper `excluded()` builds the layer under a computed key, so TypeScript loses its exact type | Won't fix: test-only, values unchanged at runtime |
| R5 | `withConfigurator()` brings its own `withLoading()` | Won't fix: same pattern as every legacy slice; the root store merges them into one counter |

No correctness or security bug that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test --watch=false` (Vitest) | 195 of 195 pass, 46 new: `selection-rules.spec.ts` (26), `catalog-api.service.spec.ts` (3), `with-configurator.spec.ts` (17) — the latter also runs a full flow with `Storage.setItem/getItem/removeItem/clear` throwing and asserts none was called |
| C2 | `npm run lint` (client) | All files pass |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the same on `main` |
| C4 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Exit 0 each |
| C5 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1089 / 1089 files |
| C6 | Backend, E2E | Not touched; left to CI |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | A filter-options 404 opens the generic error snackbar (R1) | S4-12: mark the request (an `HttpContext` token like `FORM_SHOWS_FIELD_ERRORS`) so the interceptor leaves a catalog 404 to the caller |
| P2 | Loading bar and announcement on every reload, and refused choices while a reload runs (R2, R3) | S4-10/S4-11: decide with the screen — e.g. the loading bar only for the first load, „Weiter“ waits for the options of the step |
| P3 | UX copy has no reason text for an exclusion by a product part (`criteria.select.excluded.*` knows use case and app type); `T.1` has such exclusions, 2026.1 has none | S4-11: a UX copy key for the product-part case, or the owner confirms that the screen never needs it for real releases |
| P4 | `TESTING_v41.md` names `ARCHITECTURE_v29.md`; SPEC v2.7, the Handoff and TESTING_v40 name v28 | Next documentation session (as S4-07) |
| P5 | TESTING §5.3 asks that the configurator draft survives the login outlet and can be saved in one call | The draft is in the root store (`providedIn: 'root'`), so the login outlet does not drop it; the saving part comes with step 3 |

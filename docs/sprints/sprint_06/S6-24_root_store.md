# S6-24 — Root store and the M5 entries of `LEGACY_EXCEPTIONS`

> **Sprint:** 06 · **Task:** S6-24 (1.5 points) · **Issues:** #158 (M5 entries); TECH_DEBT_v5 §5 (`withLoading()`, callers of the legacy store methods, `<strong>` in `mat-error`, `LEGACY_EXCEPTIONS`); S5-02 P3, S3-02 R1, S3-09 R1
> **Requirements / invariants:** none; every client test stays green; no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-18)
> **Scope:** exactly the row of S6-24. Client only: no EF migration, no backend change, no new dependency, no UX copy key, no screen change (no Figma comparison needed).
> **Owner decisions (2026-11-18, in the session):** (1) the legacy `<strong>` in `mat-error` stays in `product-proposal.html`, which S6-23 deletes (option (a)); (2) `filters-selection.ts` (M2 group) catches the rejection of `loadFilters()` here, so its findings are fixed and its entry deleted (option (a))

## 0. Plan (written before the code)

- **Starting point:** `main` at f5f303e.
- **Files:** `core/state/app.store.ts`, `core/models/appState.interface.ts` (deleted), `shared/state/with-loading.ts`, the slices `with-identity`, `with-configurator`, `with-filters`, `with-criteria`, `with-products`, `with-favorites`, `with-proposal` and their test stores; the six non-proposal M5 files of `LEGACY_EXCEPTIONS`; `filters-selection.ts`; `eslint.config.js`.
- **Invariants:** none activated. Tests: a new spec with the real root store (no override warning, one counter, start without an unhandled rejection); the slice specs adjusted.
- **Migration:** none. One session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `ProcureAccessStore` added `withLoading()`, and so did `withIdentity`, `withConfigurator`, `withFilters`, `withCriteria`, `withProducts`, `withFavorites` and `withProposal`; `withFavorites` also nested a second `withIdentity()`; the root `withState<AppState>` declared `loadingCount`, `products`, `productSaves`, `filterSets`, `criteria` and `proposals`, which the slices declared again | NgRx's `assertUniqueStoreMembers` warned „SignalStore members cannot be overridden“ for every repeated member on every page in dev mode (S5-02 R4/P3) |
| F2 | `AppState.identity` and `AppState.filters` were never read (`store.identity()`/`store.filters()` nowhere); the slices took their initial values from `initialAppState` | The root `withState` and the interface can go; each slice holds its own initial values |
| F3 | Callers of the store methods that reject since S3-02 still did not catch: `onInit` (`void store.init().finally(…)` — a failed product read at start was an unhandled rejection), `startSession` (`void state.loadProposals()`), `products-container.enterFilters` (floating), `filters-selection.ngOnInit` (`async` lifecycle hook); the proposal screens | Each caller catches; the error interceptor already reports an unmarked failure (offline notice, S80 page or snackbar). The proposal screens are deleted by S6-23 |
| F4 | `<strong>` in `mat-error` is only left in `products/proposal/product-proposal.html` | Owner decision (1): stays for S6-23, which deletes the file |
| F5 | The M5 group held 10 files. With their entries removed, the six non-proposal files had 15 findings (`prefer-inject`, four floating promises, `any` from `translate.get`, unbound `Validators.required`, `HttpErrorResponse` as rejection reason in the spec) | Fixed here; the four proposal files stay for S6-23 (table in §2) |
| F6 | `filters-selection.ts` (M2) had 3 findings; `with-filters.ts` (M2), changed here for F1, had 4 (`no-unsafe-enum-comparison`) | Both fixed and their entries deleted (#158: a listed file that is edited is fixed) |

## 2. Changes

1. **One loading counter** — `with-loading.ts` exports the type `LoadingCounter` and `LOADING_INPUT = { methods: type<LoadingCounter>() }`. `LoadingCounter` is a type literal: an interface has no implicit index signature and does not fit NgRx's `MethodsDictionary` (found by `tsc`). Every slice that counts loads starts with `LOADING_INPUT` instead of adding `withLoading()`. A store that forgets `withLoading()` now fails to compile, so the ten test stores of slices add `withLoading()` first. The S4-12 comment about stale `loadingCount` signals is replaced.
2. **`withFavorites`** declares `{ state: type<{ user: User | null }>() }` instead of nesting `withIdentity()`. `withIdentity` has no hooks, so the nested copy only replaced the closures of the first; the store now has one identity instance.
3. **Root store** — the `withState<AppState>` and `core/models/appState.interface.ts` are deleted; the slices start from `[]`. A doc comment states the rule.
4. **Rejections** — `onInit`: `store.init().catch(() => undefined)` (the console line „store initialized“ went with the old `finally`); `startSession`: `loadProposals().catch(…)`; `products-container`: `enterFilters` catches, `showProductsByCriteriaFilters` stays on the page when the read fails (it did not navigate before either, but rejected unhandled); `filters-selection`: synchronous `ngOnInit` with `loadFilters().catch(…)`.
5. **Lint fixes** — `inject()` in `favorites.ts`, `products-list.ts`, `products-container.ts`, `products-api.service.ts`; `product-details.ts` loses its unused `SnackbarService`; `void` for the two legacy `LiveAnnouncer` calls in `products-list.ts` (their replacement is the M6 directory rebuild, TECH_DEBT_v5 §5) and for `navigateByUrl`; `(message: string)` for the `translate.get` result; `Validators.required` through an arrow function; the spec rejects with an `Error`. `filters-selection.ts`: the field `listOptions = ViewChildren(MatListOption)` (a decorator called as a function, never read) is removed, `event.option.value as string`. `with-filters.ts`: `ofType(type)` compares `filterTypeId === Number(type)` (the API sends numbers, so `==` → `===` changes nothing).
6. **`LEGACY_EXCEPTIONS`** — 15 → 7 files:

   | Entry | Task |
   |---|---|
   | `favorites/list/favorites.ts` | S6-24 (deleted) |
   | `products/details/product-details.ts` | S6-24 (deleted) |
   | `products/list/products-list.ts` | S6-24 (deleted) |
   | `products/pages/products-container/products-container.ts` | S6-24 (deleted) |
   | `products/services/api/products-api.service.ts` | S6-24 (deleted) |
   | `products/state/with-products.spec.ts` | S6-24 (deleted) |
   | `filters/selection/filters-selection.ts` (M2) | S6-24 (deleted, owner decision (2)) |
   | `filters/state/with-filters.ts` (M2) | S6-24 (deleted, file changed for F1) |
   | `criteria/proposal/criterion-proposal.ts` | S6-23 |
   | `products/proposal/product-proposal.ts` | S6-23 |
   | `proposal/list/proposal-list.ts` | S6-23 |
   | `proposal/services/api/proposal-api.service.ts` | S6-23 |
   | `criteria/list/criteria-list.ts`, `criteria/services/api/criteria-api.service.ts`, `filters/services/api/filters-api.service.ts` (M2) | legacy removal (activation of 2026.1) |

   The M5 comment now reads „proposal workflow retired (M5.6, S6-23)“. With S6-23 the M5 group is empty.
7. **Tests** — `core/state/app.store.spec.ts` (new) builds the real root store with only the backend replaced: no „cannot be overridden“ warning (`console.warn` spy); a load of `withFilters` shows in the store's `isLoading()`; a start whose product read answers 500 leaves `isLoading()` false without an unhandled rejection (Vitest fails a run on one). Checked red: with a second `withLoading()` in the root store the first test fails (three warnings), with the old `void store.init()` Vitest reports the unhandled rejection. The counter test is a regression guard (it also passed before: NgRx keeps one state key).
8. **Docs** — CODEBASE §2.86 (new), §2.10 (the counter sentence), §3 (layout line), §1 (history); CLAUDE §4 (the root-store rule, last task reflected). ARCHITECTURE §8.1 is unchanged (`withLoading()` "keep"); the S4-12 wording in ARCHITECTURE C118 is a change row and stays.

## 3. Self-review

Code review per CLAUDE §6.7 (`engineering:code-review` on the staged diff):

| Finding | Sorting |
|---|---|
| Security: no request, header, guard or token handling changed | — |
| Correctness: removing the nested `withIdentity()` from `withFavorites` — checked that `withIdentity` has no hooks or effects, so nothing ran twice before and nothing is lost now | no change needed |
| Correctness: `==` → `===` in `with-filters` — `CriteriaFilter.filterTypeId` is a number from JSON | no change needed |
| `LOADING_INPUT` is `{ methods: undefined }` at runtime; NgRx drops a non-function first argument | no change needed |
| A11y: no markup change; the legacy `LiveAnnouncer` calls stay (M6) | won't fix (scope; TECH_DEBT_v5 §5 names M6) |
| Catches swallow the rejection after the interceptor reported it; no caller needs the reason | by design |

No *fix* finding remained open.

## 4. Checks

| Check | Result |
|---|---|
| `npx tsc -p tsconfig.app.json` / `tsconfig.spec.json` | Green (first run found the interface/`MethodsDictionary` mismatch, fixed) |
| `npm run lint` (client) | All files pass |
| `npx ng test` (full) | 85 files, 827 tests: 826–827 green per run; in two full runs under a host load average of ~21 (other sessions) one to three different component tests hit the 5 s timeout (S12, S13, saved list); each passes alone (`save-list-dialog` 11/11, the three configurator groups 64/64). Not related to the change; CI is the authority |
| `npx ng test --include src/app/core/state/app.store.spec.ts` | 3/3 green; red check as in §2.7 |
| `npm run build:release` | Green; the known CodeMirror selector warning only |
| `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Green |
| `reuse lint` (Docker) | No finding in a repository file. In a worktree the container cannot see git, so it also lists ignored files (`node_modules/`, `dist/`, `.env`); those were filtered against `git ls-files` |
| E2E | Not run locally: another session uses the local E2E stack (shared ports). Left to CI |
| Dev server warning | Checked through the unit test, which runs in dev mode (`ngDevMode`): with the old composition it records the warnings, with the new none |

## 5. Open points

1. **S6-23** deletes the four proposal files, their `LEGACY_EXCEPTIONS` entries, `withProposal()` (and the `loadProposals()` call in `startSession`, `setProposals([])` in `clearUserData`) and the last `<strong>` in `mat-error` (`product-proposal.html`). Then the M5 group is empty and #158 keeps only the three M2 entries of the legacy filter. `with-proposal.ts` and its spec changed here only so that they compile (`LOADING_INPUT`, `withLoading()` in the test store).
2. **#158** stays open (M2 entries until the activation of 2026.1). TECH_DEBT_v5 §5 rows "`withLoading()` … (S5-02 P3)" and "Callers of the fixed legacy store methods … `<strong>` in `mat-error`" are done except the `<strong>` (S6-23); they are struck in the next TECH_DEBT version (sprint-end task), with "`LEGACY_EXCEPTIONS` 16 files" → 7.
3. The two legacy `LiveAnnouncer` calls with English text in `products-list.ts` stay until the M6 directory rebuild (TECH_DEBT_v5 §5).
4. Component tests near the 5 s Vitest timeout under heavy host load (S12, S13, saved list) — seen only locally with a load average of ~21; watch CI before proposing a longer timeout.

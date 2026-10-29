# S2-01 — Live bugs: product filter 500, delete-account dialog, dev health watcher

> **Sprint:** 02, P0 task S2-01 (debt; TECH_DEBT_v1 §3.1, §9.1 row 2). **Issues:** #37, #95, #96, #113. **Requirements / invariants:** none (bug fixes). **Status:** done by Claude Code (2026-10-29), pull request open.
> **Scope:** #37 with the spinner that stays after a failed request (S1-08 N1), #95 with the missing spec cases of #96, #113. One PR, one commit per issue, a regression test per bug. Not in scope: the `aria-disabled` pattern for the dialog (identity rework), other differences of the legacy product endpoint (§5).
> **Owner decisions 2026-10-29:** **D1** #113: keep the watcher and derive its URL from `environment.apiUrl` (not delete it). **D2** #95: keep the native `disabled` on "Löschen"; the CLAUDE §9 pattern (`aria-disabled` with a visible reason) comes with the rework of the identity dialogs, because the UX copy has no reason text for it yet.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `ProductsController.ByCriteriaFilterIds` returned `Ok(task)` without `await`; System.Text.Json failed on the task's `ExecutionContext` (exactly the exception in #37) | `async` + `await` |
| F2 | `ProductService.GetByCriteriaFilterIds` returns `Result<IEnumerable<ProductDto>>`; the client reads `TResult<Product[]>.value`, like `GET /api/Products` (`BaseCrudController.GetAll` also returns the wrapper) | The wrapper is the expected shape; the action now declares it instead of `List<ProductDto>` |
| F3 | Both load methods of `with-products.ts` call `decrementLoadingCount()` only after a successful `await` (S1-08 N1) | `try/finally` |
| F4 | `deletionDisabled = inputEmail() !== email()`: with `email: ''` the empty input matches at once (#95) | Empty input never confirms |
| F5 | `dev-reload-health.ts` had `https://api.procureaccess.tech/health/live` hard-coded; Development CORS of the API already allows `http://localhost:4200`, and `UseCors` covers the health endpoints | URL from `environment.apiUrl`; the health endpoints sit at the API root |
| F6 | `GetByCriteriaFilterIds` does not apply the proposal filter of `ReadAll` (`Proposal == null || Approved`). Products are only created when a proposal is approved (`ProposalService.ApplyApprovalSideEffects`), so no product with a pending proposal exists today | No change; §5 P1 |

## 2. Changes

| File | Change | Issue |
|---|---|---|
| `API/…/Controllers/ProductsController.cs` | `ByCriteriaFilterIds` is `async Task<ActionResult<Result<IEnumerable<ProductDto>>>>` and awaits the service | #37 |
| `tests/…Integration/Catalog/ProductsByCriteriaFilterTests.cs` (new) | Seed filter types and criteria filters plus three products built in the test (filters 1+2, filter 1, none). One filter → 200, `succeeded`, the two products with filter 1; two filters → only the product with both (records today's "every selected filter" rule) | #37 |
| `CLIENT/…/features/products/state/with-products.ts` | `loadProducts` and `getProductsBySelectedCriteriaFilterIds` lower the loading count in `finally`; the error still reaches the caller (the error interceptor shows the snackbar as before; `showProductsByCriteriaFilters` still does not navigate after an error) | #37 (N1) |
| `CLIENT/…/features/products/state/with-products.spec.ts` (new) | Store with only `withProducts()`, API service replaced: both failed requests leave `isLoading()` `false` and reject with the error; a successful filtered request stores the products | #37 (N1) |
| `CLIENT/…/identity/dialogs/delete-account-dialog/delete-account-dialog.ts` | `deletionDisabled` is also `true` while the input is empty | #95 |
| `…/delete-account-dialog.spec.ts` | New cases: empty account e-mail (also after typing and clearing); "Abbrechen" with nothing typed closes with `false`; near misses (other case, trailing space, prefix) keep "Löschen" unavailable. "Löschen" and "Abbrechen" are tested in separate dialogs; the click check that could never fail on a disabled button is removed | #95, #96 |
| `CLIENT/…/src/dev/dev-reload-health.ts` | `apiHealthUrl(apiUrl)` = `new URL('/health/live', apiUrl)`; the watcher uses `environment.apiUrl` | #113 |
| `CLIENT/…/src/dev/dev-reload-health.spec.ts` (new) | The dev configuration (`environment.ts`, `production: false`) resolves to `http://localhost:5000/health/live` | #113 |

No document needed a change: no structure, command or convention changed (CLAUDE.md §3/§4), no invariant was activated (no new TESTING version), no UI text or `@figma` component was added. `TECH_DEBT_v1.md` stays as it is; the sprint-end review writes v2 and moves the closed items out.

## 3. Self-review (CLAUDE §6.7, `/engineering:code-review`)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: no new endpoint, no auth change, no antiforgery concern (GET); the dev watcher only requests the API of the build (no third-party request, CLAUDE §8) | – |
| R2 | Performance: the product query is unchanged (one query, `ProjectTo`); no N+1 | – |
| R3 | The legacy endpoint does not apply the proposal filter of `ReadAll` (F6). No effect today, because products only exist after approval | Won't fix: no behaviour today; the endpoint is replaced by the directory. §5 P1 |
| R4 | Neither `ReadAll` nor `GetByCriteriaFilterIds` filters `Product.IsDeleted`; both behave the same | Won't fix: existing behaviour, legacy code replaced later. §5 P2 |
| R5 | New analyzer warning `VSTHRD200` ("Async" suffix) on the now asynchronous action; `CriteriaController.ByCriteriaFilterIds` has the same one. The route comes from the attribute, so renaming would be safe but is cosmetic | Won't fix: build warnings are #118 (sprint 03) |
| R6 | Swagger attributes of the action announce 204 and 400, which it never returns | Won't fix: legacy controller; Problem Details (S2-10) and the directory API redo the contracts |
| R7 | `apiHealthUrl('/api')` (release configuration) would throw, because the base is relative. The watcher only starts when `environment.production` is `false` | Won't fix: not reachable; JSDoc names the dev case |
| R8 | A11y: no markup change. The dialog keeps native `disabled` (D2) | Follow-up: already in #95's "When" (identity rework); §5 P3 |
| R9 | The near-miss cases pin down the exact match. If the rework trims or folds case on purpose, these tests change with it | – (intended, #96) |

No finding is a correctness or security bug in the change; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln --no-incremental` | ✅ 0 errors; in the touched files only R5 |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ 61/61 |
| 3 | New integration test against the old controller | ✅ both cases fail with the #37 exception (`ExecutionContext&` … invalid for serialization); pass with the fix |
| 4 | `npx ng test` (Node 24.22.1) | ✅ 17/17 in 5 files |
| 5 | New client tests against the old code | ✅ store: the two failure cases fail; dialog: only the empty-e-mail case fails; all pass with the fixes |
| 6 | `npm run lint` / `npm run build:release` / `ng build --configuration development` | ✅ / ✅ / ✅ |
| 7 | `dotnet format` whitespace + style on the solution, analyzers on the integration test project | ✅ |
| 8 | E2E stack: `GET /api/Products/byCriteriaFilterIds?criteriaFilterIds=1` | ✅ 200 with `value` |
| 9 | E2E stack, Playwright chromium | ✅ 13/13; other browser projects run in CI |
| 10 | Manual flow on the E2E stack (Playwright script outside the repository): `/filters` → "Audio" → "Enter" → "Produkte anzeigen" | ✅ `byCriteriaFilterIds` 200, `/products` shows the filtered table, no progress bar left |
| 11 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 12 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 13 | `reuse lint` (image `fsfe/reuse:latest`; `reuse` is not installed in Claude's shell) | ✅ compliant, 531/531 files |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The legacy `GetByCriteriaFilterIds` lacks the proposal filter of `ReadAll` (R3). Harmless today; matters only if products with a pending proposal become possible before the directory replaces the endpoint | Directory step; no issue proposed |
| P2 | Legacy product reads ignore `Product.IsDeleted` (R4) | Directory step; no issue proposed |
| P3 | "Löschen" uses native `disabled`; CLAUDE §9 wants `aria-disabled` with a visible reason and the gate checked in the click handler. Needs a UX copy text | Identity rework (step 1/3), noted in #95 |
| P4 | `reuse` is not installed in Claude's shell; the FSFE Docker image works. Optional: name it in CLAUDE.md §5 | Philipp |

# S3-02 — Legacy client fixes: loading counter, offline notice, icon spans

> **Sprint:** 03, P0 task S3-02 (debt, 1 point). **Issues:** #168, #171, #137. **Requirements / invariants:** R-SHL-08 (offline notice), R-SHL-09 (loading state), Handoff §1.8 (icons `aria-hidden`); no invariant activated, no new TESTING version; A11Y-01 stays green. **Status:** done by Claude Code (2026-11-05); one pull request, one commit per issue.
> **Scope:** exactly the sprint row: #168 decrement the loading counter when a load fails, in every legacy store and in `approveProposal` (S2-17 P1, S2-11 P6); #171 the offline notice counts only answers to `/api/` requests (S2-18 P2); #137 the legacy icon `<span>`s in settings and profile become `<mat-icon>`. Nothing else in the legacy screens is touched; they are rebuilt from sprint 04 on.
> **Owner decisions:** none needed for the task. Asked before the start (2026-11-05): #171's "Done when" asks for an E2E case besides the unit test, the task prompt for "a unit or component test per issue" — the owner chose to add the E2E case, so #171 can be closed.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Seven store methods increment the `withLoading()` counter, `await` the API and decrement only after it: `withFilters.loadFilters`, `withCriteria.getCriteriaBySelectedCriteriaFilterIds`, `withSettings.reloadUICustomization` (signed-in branch) and `updateUICustomization`, `withIdentity.deleteAccount`, `withProposal.loadProposals` and `approveProposal`. Every API service behind them returns `lastValueFrom(http…)`, so a failed request rejects and the decrement never runs | `try … finally` in each, as `with-products.ts` does; callers see the same rejection as before |
| F2 | The other call sites are fine: `withProducts`, `upsertProposal`, `reviewProposal` (`try/finally`), login and register (`finalize`); `criterion-proposal.ts` and `product-proposal.ts` only have commented-out calls | Not changed |
| F3 | `errorInterceptor` calls `ConnectionStatus.answered()` for every answer, also for `i18n/de.json`, which the web server serves while the API is down (S2-18 P2) | Count only requests under `API_URL` (issue option 1); `API_URL` is `/api` in the release build and `http://localhost:5000/api` for `npm start`, and every API service builds its URLs from it |
| F4 | Three `<span class="material-symbols-outlined">` are left: profile (×2, product icon `box` of the open and closed proposal lists, read as list item text) and settings (×1, contrast button, named by its `<label for>`). The header ones went with the shell (S2-15) | `<mat-icon>`: same self-hosted font (default font set since S2-03), `aria-hidden="true"` by default |

## 2. Changes

| File | Change |
|---|---|
| `features/{filters,criteria,settings,identity,proposal}/state/with-*.ts` | #168: the bodies of the seven methods of F1 in `try { … } finally { state.decrementLoadingCount(); }`; no other change |
| `features/{filters,criteria,settings,identity,proposal}/state/with-*.spec.ts` (new) | #168: one spec per store, the slice in a store of its own with only the API service (and the services outside the slice) replaced, as `with-products.spec.ts` does: a rejected call rejects the method and leaves `isLoading()` false. Settings: both methods for a signed-in user; proposal: `loadProposals` and `approveProposal`. The fakes reject with an `Error`, not an `HttpErrorResponse` (rule `prefer-promise-reject-errors`, no new lint exception; the stores do not look at the reason) |
| `core/interceptors/error.interceptor.ts` | #171: `answered()` only when `req.url` is `API_URL` or starts with `API_URL/`; a request without answer (status 0) still turns the notice on, whatever its URL. Doc comment says why |
| `core/errors/system-errors.ts` | #171: doc comments of `ConnectionStatus` and `answered()` say "API" instead of "server" |
| `core/interceptors/error.interceptor.spec.ts` | #171: `API_URL` provided as `/api`; two tests: a static file answering after a failed API request keeps the notice; `/apidocs` (starts like the API address, but is not under it) does not count |
| `e2e/shell/start-and-error-pages.spec.ts` | #171: new case — first page load of `/filters` with the API cut and `de.json` delayed by 800 ms (the reproduction of the issue): the notice shows its text, which exists only after `de.json` has answered. #168: that case and the existing offline case check that the progress bar is gone after the failed load |
| `features/identity/profile/profile.html`, `features/settings/settings.html` | #137: the three spans → `<mat-icon>` (profile keeps `matListItemIcon`) |
| `features/identity/profile/profile.spec.ts`, `features/settings/settings.spec.ts` (new) | #137: component tests with a stub store (and the permission check for profile): the text exposed to assistive technology of both proposal list items holds the product name but not "box"; the icon of the contrast button is `aria-hidden` with both glyphs, and the button is still named „Kontrastmodus:“ |
| `CLAUDE.md` | §3: loading counter decremented in `finally`; `ConnectionStatus` goes online only with an answer of the API; version line |
| `docs/sprints/sprint_03/S3-02_legacy_client_fixes.md` (new) | This protocol |

Every new test failed against the code of `main` and passes with the change (checked per issue by putting the old file back for one run; §4 check 2). Not changed on purpose: `TECH_DEBT_v2.md` still lists #168, #171 and #137 as open; the register is versioned and v3 at the end of the sprint moves them to "Done" (P3).

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The callers of the changed methods still do not catch the rejection (e.g. `approveProposal` in the proposal screens is called without `await`, `deleteAccount` from a dialog callback), so a failed call ends as an unhandled rejection in the console, as before | Won't fix: behaviour unchanged, the screens are legacy and rebuilt or retired (M3, M5); their files are in `LEGACY_EXCEPTIONS` for `no-floating-promises` |
| R2 | A 5xx answer of a non-API request (e.g. the web server failing `de.json`) still opens the S80 page in place; status 0 of such a request still shows the offline notice | Won't fix: the row and #171 are about hiding the notice; a web server that cannot serve the app's own files is out of reach anyway. Changing the 5xx rule would be a new behaviour without a requirement |
| R3 | `isApiRequest` compares the URL string; a request with a URL built another way (e.g. an absolute URL to the API in the release build) would not count | Won't fix: every API service builds its URLs from `API_URL` (F3); the unit test pins the rule |
| R4 | `withSettings.reloadUICustomization` reads `uiCustomization.language` before the `?? initial…` fallback, so a `null` answer throws (pre-existing; now the counter is decremented anyway) | Won't fix: legacy, settings S60 are rebuilt with step 3 |
| R5 | Security, N+1, data: no API, query, storage or logging change; no new dependency | – |
| R6 | Accessibility: the three icons now leave the accessibility tree; names of the buttons unchanged (component test); the notice and the progress bar behave as R-SHL-08/09 describe also after a failed load | – |

No correctness or security bug that affects current behaviour was found in the diff.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npx ng test` (Vitest, single run) | ✅ 31 files / 122 tests passed (12 new) |
| 2 | New tests against the old code (old file put back for one run per issue) | ✅ all fail for the expected reason: #168 7 tests `expected true to be false`; #171 both new interceptor tests; #137 3 tests (`aria-hidden` undefined, "box" in the item text) |
| 3 | `npm run lint` | ✅ all files pass; `LEGACY_EXCEPTIONS` unchanged |
| 4 | `npm run build:release` | ✅ passes; initial total 1.80 MB raw / 369 KB transfer; the known critical-CSS selector warning (S2-03 R9) is unchanged |
| 5 | `e2e`: `npm run typecheck` | ✅ |
| 6 | E2E, Chromium (`npm run stack:up`, `npx playwright test --project=chromium`) | ✅ 40 / 40 passed, incl. the new #171 case and A11Y-01 on every shell state; Firefox and mobile Chrome: 80 / 80 passed. WebKit and mobile Safari run only in CI (P2). The new #171 case was not run against an image of the old client; its reproduction is the one of the issue, and the unit test of §2 fails on the old interceptor |
| 7 | `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | ✅ gate passed; Figma 2 checked, 24 not built, 0 pending; i18n 0 errors (6 keys computed at runtime, as before) |
| 8 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 680 / 680 files |
| 9 | Backend build and tests | – not touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Close #168, #171 and #137 with the merge (the PR says "Closes") | Owner, merge |
| P2 | Firefox, WebKit and the mobile projects of the E2E run are left to CI (no local WebKit) | CI of this PR |
| P3 | `TECH_DEBT_v3.md` (end of sprint): move #168, #171 and #137 to "Done" (S3-02) | End of sprint 03 (process step 14) |

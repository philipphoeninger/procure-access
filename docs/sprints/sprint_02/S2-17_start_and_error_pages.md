# S2-17 — Start page, error pages, loading/empty/system states

> **Sprint:** 02, P0 task S2-17 (2 points; roadmap M1 shell). **Issues:** none. **Requirements / invariants:** R-SHL-05, R-SHL-08 (without the TEN-03 part, step 6), R-SHL-09; A11Y-01 stays green; no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** start page S04; error pages S80 for 404, 403 and 500; the offline notice; the loading state (`mat-progress-bar`, one `common.loading` announcement, no skeleton screens). Not in scope: TEN-03 (foreign resources → 404, step 6), empty states and the button spinner of Handoff §6.2 (their first screen), the e-mail confirmation result pages „as S80“ (step 3), the start page of the compact E2E tests (S2-18), the 320 px / 400 % check (S2-18).
> **Owner decisions 2026-11-03 (before the task, asked by Claude):** **1A** the start page shows only the cards whose target exists (Kriterien, Produkte); projects (S30), contributions (S53) and the vendor link (S55) come with their steps. **2** the legacy start page content goes, including the only link to `/favorites` (reachable by URL until `withFavorites()` is retired). **3A** every 5xx answer shows the S80 page „Etwas ist schiefgelaufen“ in place, without changing the URL and without a snackbar. **4** the offline notice appears when a request gets no answer or the browser goes offline, and disappears with the next answer or when the browser is back online; no snackbar. Defaults accepted with them: start page at `/` with `/home` redirecting, `**` shows 404 in place, `/unauthorized` becomes the S80 403 page, the centered spinner becomes the progress bar, `common.loaded` waits for a load with a count, `home` becomes an `AREAS` root, S04/S80 have no Figma frame and get no `@figma` tag.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `/` redirected to `/home`, the legacy page: hard-coded „Kürzlich hinzugefügt“ demo products with image files that do not exist, English button names („Filters icon-button“ …), the only link to `/favorites`, wider than 360 px (S2-15 P3) | Replaced with S04 (decisions 1A, 2) |
| F2 | `/unauthorized` was a legacy card with the H1 „403“, an H2 „Zugriff verweigert“ and a login button; the title already was „Kein Zugriff“ (S2-16 R6) | S80 `forbidden` |
| F3 | No wildcard route: an unknown address ended in a router error and an empty `main` | `**` → S80 `notFound` in place |
| F4 | Every HTTP error showed a snackbar, also 5xx and „no answer“ | Interceptor: S80 `server` for 5xx, offline notice for status 0 (3A, 4) |
| F5 | A `mat-spinner` in the middle of the window while the shared `withLoading()` counter was above zero; nothing was announced | `pa-loading-bar` at the top of `main` |
| F6 | `home.*`, `errors.*` and `common.loading` are in UX copy v15; `pageTitle.notFound` / `.server` were not in `de.json` yet | `home` added to `AREAS` and written with `--write`; the two page titles by hand (`pageTitle` is no `AREAS` root, S2-16 F7) |
| F7 | `pa-notice` already has `live="polite"` → `role="status"` (S2-11), which is what Handoff §6.5 asks for the offline notice | Used as it is; no second live region |
| F8 | The legacy products are loaded once at start-up; the legacy criteria screen loads its data on every visit | The E2E tests of the system states use the criteria screen |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/app/pages/home/` (rewritten, `pa-home`) | H1 `home.title`, `home.intro`, a list of two `mat-card`s (H2, text, link with the look of a filled button: Kriterien → `/filters`, Produkte → `/products`), `home.disclaimer` as `pa-notice` info. One column at 360 px, two from 600 px; tokens only | R-SHL-05, Handoff §5.3; 1A, 2 |
| `src/app/pages/error/` (new, `pa-error-page`) | One component for `notFound` / `forbidden` / `server` (route data `errorPage`): H1 `errors.<kind>.title`, text `errors.<kind>.body`, link `errors.home` → `/`. The UX copy calls `errors.home` a button; it navigates, so it is a link with the look of a filled button | R-SHL-08, Handoff §5.3 |
| `src/app/pages/unauthorized/` | Deleted | F2 |
| `src/app/app.routes.ts` | `''` = start page (`pageTitle.home`), `home` redirects to `''`; `unauthorized` = 403 (`pageTitle.forbidden`), `error` = 500 (`pageTitle.server`), last `**` = 404 (`pageTitle.notFound`) | Page title = H1 (R-SHL-08) |
| `src/app/core/errors/system-errors.ts` (new) | `SERVER_ERROR_PATH`; `ConnectionStatus` with the signal `offline`: `noAnswer()`, `answered()` and the browser's `offline` / `online` events; starts from `navigator.onLine` | Decision 4 |
| `src/app/core/interceptors/error.interceptor.ts` | Every answer (also an error answer) → `answered()`; status 0 → `noAnswer()`, no snackbar; ≥ 500 → `router.navigate(['error'], { skipLocationChange: true })`, no snackbar. Other errors as before | Decisions 3A, 4; Handoff §6.5 |
| `src/app/layout/loading-bar/` (new, `pa-loading-bar`) | `mat-progress-bar mode="indeterminate"` named „Wird geladen …“ while `isLoading()`; an effect announces `common.loading` each time loading starts (`isLoading` is computed, so it changes only at start and end) | R-SHL-09, Handoff §6.2 |
| `src/app/app.html`, `app.ts`, `app.scss` | At the top of `main`: `pa-loading-bar`, then the offline notice (`pa-notice` attention, `live="polite"`, full width). The spinner and the store in `App` are gone. While the legacy login outlet replaces the shell, a `pa-loading-bar` stands at its place, so the login requests keep a loading state | Handoff §6.2, §6.5 |
| `src/app/core/a11y/a11y-announcer.service.ts` | Messages are queued in call order: CDK `LiveAnnouncer` sets a message after 100 ms and drops it when the next one comes earlier, so the page title of a route change swallowed `common.loading` (found by the E2E test, see R1) | R-SHL-09, A11Y-03 unchanged |
| `src/app/layout/header/header.html` | Brand link → `/` | New start page address |
| `public/i18n/de.json` | `home.*` written by `check-i18n.mjs --write`; `pageTitle.notFound`, `pageTitle.server` by hand; unused legacy keys `HOME.*`, `AUTH.ACCESS_DENIED_LABEL`, `AUTH.ACCESS_DENIED_MSG`, `COMMON.GO_TO_HOME`, `COMMON.SEE_MORE` removed (`en/es/fr.json` untouched, as in S2-15) | F6 |
| `scripts/check-i18n.mjs` | `home` added to `AREAS` | S2-13 P2 |
| Tests (Vitest) | `pages/home/home.spec.ts`, `pages/error/error-page.spec.ts`, `app.routes.spec.ts` (real route table: `/`, `/home`, 404/403/500 with title = H1, unknown URL kept), `layout/loading-bar/loading-bar.spec.ts`, `core/errors/system-errors.spec.ts`, new cases in `error.interceptor.spec.ts` (5xx, status 0, next answer), `app.spec.ts` (bar and notice in `main`), `a11y-announcer.service.spec.ts` (`Announce_WhileAnotherIsPending_WaitsInsteadOfReplacingIt`); `header.spec.ts`, `main-nav.spec.ts`, `shell-test-doubles.ts` follow the new start page address | Component tests by role and name |
| Tests (E2E) | `e2e/shell/start-and-error-pages.spec.ts` (new): start page wide and 360 px, its cards, `/home`; 404, 403 (through the legacy approver list), 500 in place with the focus on the H1; offline notice and its end; progress bar and announcement — axe on every state (`INV:A11Y-01`). Server errors, no connection and slow answers come from `page.route`. `e2e/smoke/stack.spec.ts` checks the new H1; `e2e/shell/shell.spec.ts` starts at `/` | A11Y-01 |
| `CLAUDE.md` | §3 current state, §4 layout, §5 comment of the i18n check | Document upkeep |

`figma_code_mapping_procureaccess_v1.md` is unchanged: S04 and S80 have no Figma frame (Handoff §5.3), like S71, which is not in the mapping either.

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y) with the skill *engineering:code-review* on the whole diff. Findings sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | On every route change that starts a load, `LiveAnnouncer` dropped „Wird geladen …“ in favour of the page title (second message within 100 ms cancels the first) | **Fixed:** queue in `A11yAnnouncerService`, in call order; unit test and E2E check that both texts reach the live region |
| R2 | The first axe run of the offline state reported `color-contrast` on the active nav item (4.18 : 1): the colour transition after the click was still running; the settled state passes (probe in Chromium; the fixed test is green in all three local browsers) | **Fixed in the test:** axe runs after the clicked item's animations are finished (`settled()`); no change to the shell |
| R3 | With the queue, a burst of loads could queue several „Wird geladen …“ | **Won't fix:** probe of `/`, `/filters`, `/products`, `/settings` shows one per page visit; the per-key throttle of Handoff §3.4 comes with the first counter (step 2) |
| R4 | A translation that never resolves would hold the announcement queue | **Won't fix:** then no text of the app is shown and no announcement was made before either |
| R5 | A 5xx answer of a background request (e.g. the products loaded at start-up) replaces the page the user opened, also a legal page; a 5xx during the legacy login closes the login outlet | **Won't fix:** this is decision 3A (every 5xx); recorded here so it is not a surprise |
| R6 | Several legacy stores (`withFilters`, `withCriteria`, `withSettings`, `withIdentity`, `withProposal`) do not decrement the loading counter when a load fails, so after a failed load the progress bar stays (the spinner did the same) | **Follow-up issue #168 (P1):** the stores are rebuilt with their steps; the bar only makes the existing defect visible |
| R7 | `role="status"` on a notice that is inserted with its text is not announced by every screen reader | **Follow-up:** manual screen-reader check (A11Y-07, P2); the pattern is the one Handoff §6.5 and `pa-notice` prescribe |
| — | Security: no new input, no new endpoint, no secrets; N+1: no backend change | – |

## 4. Checks

| Check | Result |
|---|---|
| `npx ng build` | green |
| `npx ng test` (Vitest) | 24 files, 110 tests green |
| `npm run lint` | green, no new `LEGACY_EXCEPTIONS` entry |
| E2E (`npx playwright test`, Chromium, Firefox, mobile Chrome; WebKit and mobile Safari only in CI) | all green; the new spec also three times in a row (`--repeat-each=3`) |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 0 pending |
| `node scripts/check-i18n.mjs` / `node --test scripts/check-i18n.test.mjs` | 0 errors (103 keys incl. `home`) / 10 passed |
| `reuse lint` | compliant (657 / 657 files) |
| Backend (`dotnet test`) | not run: no backend change |

## 5. Open points

| # | Point | Where / when |
|---|---|---|
| P1 | Legacy loading counter is not decremented on a failed load, so the progress bar stays after an error (R6), #168 | Rebuild of each store (M2/M3) or a small legacy fix |
| P2 | Manual screen-reader check of the offline notice (`role="status"` inserted with its text) and of the queued announcements (R7) | A11Y-07, before the pilot |
| P3 | Start page cards for projects (S30), contributions (S53) and the vendor link (S55) (decision 1A); „no card needs an account“ then needs S30's anonymous state | Steps 5 – 7 |
| P4 | `/favorites` has no link any more (decision 2) | Retirement of `withFavorites()` (ARCHITECTURE §8.1) |
| P5 | Empty states (text + one action) and the button spinner of Handoff §6.2; `common.loaded` with a count | Their first screen (step 2) |
| P6 | The compact E2E tests still start on `/barrierefreiheit` (S2-15 P3); the start page fits 360 px now | S2-18 |
| P7 | TEN-03: foreign resources → 404 | Step 6 |
| P8 | The e-mail confirmation result pages „as S80“ (Handoff §5.3) | Step 3 |

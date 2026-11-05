# S3-10 — A11Y-08: axe in the dark appearance

> **Sprint:** 03, P0 task S3-10 (M1.5, 2 points). **Issues:** none. **Requirements / invariants:** A11Y-08 → `active` (development part) in `TESTING_v29.md` §4.11 (V91); R-SET-02, R-SET-03 (development part: "A11Y-01 runs on each journey state in both appearances"), SPEC §7.5; A11Y-01, A11Y-03 and A11Y-09 stay green. **Status:** done by Claude Code (2026-11-05); one pull request.
> **Scope:** exactly the sprint row: the axe run of A11Y-01 repeated in the dark appearance on every journey state and shell page, in all five browser projects; contrast findings on shell pages fixed with tokens; findings on legacy screens suppressed with one issue link (TESTING §12.4); A11Y-08 flipped, the persisted setting recorded as deferred to step 3.
> **Owner decisions (2026-11-05):** asked before the start: **1 (a)** run axe with the emulated browser preference only (`colorScheme: 'dark'`); the explicit setting and its persistence in `UserPreferences` are deferred to step 3 (S60) and recorded in the TESTING version. **2 (a)** a contrast fix on a shell page only switches to another `--mat-sys-*` role; pinning palette values (`mat.theme-overrides`) stays with O-S5. Approach after the plan: the axe helper runs both appearances itself (instead of five extra Playwright projects). Asked during the task (F4): **drop the snackbar** that the legacy settings store opened on a change of the system appearance.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | 13 tests call `expectNoAxeViolations` (`e2e/support/axe.ts`): J1 (2 tests: 7 states of the journey and the empty list, on `/filters`), `shell.spec.ts` (4), `route-change.spec.ts` (1), `start-and-error-pages.spec.ts` (6; the start-page test runs for two layouts). All ran in light only: Playwright emulates `light` by default | Repeat every call in dark; one place to change is the helper |
| F2 | Since S3-09 `AppearanceService` follows `prefers-color-scheme` without reload (class `theme-light` / `theme-dark` on `<html>`), so the appearance can be switched in the middle of a journey state | The helper switches with `page.emulateMedia`, checks, and switches back; journeys stay as they are |
| F3 | Alternative: five more Playwright projects with `colorScheme: 'dark'`. It would run every test twice, also those without axe, and change the CI jobs and the required check `6. E2E + a11y` | Not taken (plan, owner OK) |
| F4 | First run (Chromium): two failures, no contrast finding. A change of the system appearance made the legacy settings store (`with-settings.ts`, `activateMediaEventListeners`) open an English snackbar („New color scheme saved as cookie.“; signed-in users got another English text) **without duration**. It covered the „Menü“ button (the compact-menu test timed out) and once gave an axe `aria-hidden-focus` finding on the snackbar while it was being announced. Every visitor who changes the system appearance saw it (S3-09 R6/P3) | Owner: drop the two snackbar calls; the storage writes stay until S60 |
| F5 | After F4: no dark-mode violation on any state, also not on the legacy screen `/filters` of J1 (sprint risk R6 did not occur). A probe with only `color-contrast` in dark on `/`, `/filters`, `/products`, `/impressum` and a 404 page gave no `incomplete` result either, so axe could decide every text node | No issue and no suppression needed; `axe-suppressions.ts` unchanged |

## 2. Changes

| File | Change |
|---|---|
| `e2e/support/axe.ts` | `expectNoAxeViolations` runs axe in both appearances: first the one the page shows (`matchMedia` in the page), then it emulates the other `prefers-color-scheme`, waits until `<html>` carries the matching `theme-*` class (pages without such a class, like the helper's fixtures, only follow the media query) and until the CSS transitions it started have ended, runs axe again with the same suppressions, and switches back. Failures of both runs are reported together, each naming the appearance; the report of each run is attached as `axe-<state>-<appearance>.json` and annotations name the appearance. New exported type `Appearance` |
| `e2e/support/axe-helper.spec.ts` | 2 new self-tests (`INV:A11Y-08`): a page whose text has enough contrast in light and about 1.4:1 in dark fails, naming „dark appearance“, `color-contrast` and the element, and not „light appearance“; a page that starts in dark is checked dark first, then light, and is dark again afterwards |
| `e2e/journeys/j1-configurator.spec.ts`, `e2e/shell/shell.spec.ts`, `route-change.spec.ts`, `start-and-error-pages.spec.ts` | `INV:A11Y-08` added to the title of every test that calls the helper (13 tests); `the cards open their screens without an account` runs no axe and keeps only its old tag |
| `CLIENT/…/src/app/core/a11y/appearance.spec.ts` | `INV:A11Y-08` on `SystemSettingChanges_ClassFollowsWithoutReload` (the FE level of the invariant: the page follows the system without reload) |
| `CLIENT/…/src/app/features/settings/state/with-settings.ts` | F4: the colour-scheme listener no longer calls `snackbarService.showInfo` (both branches); everything else, including the `localStorage` write for signed-out users, is unchanged |
| `CLIENT/…/src/app/features/settings/state/with-settings.spec.ts` | `SystemChange_SignedOut_FollowsWithoutSnackbar` / `SystemChange_SignedIn_FollowsWithoutSnackbar`: a system change while the page is open switches to dark and opens no snackbar. Both fail against the old store (checked by restoring it) |
| `docs/TESTING_v29.md` (new, from v28) | Status line; V91; A11Y-08 `active` with the reworded row (explicit setting and persistence: step 3); §5.4 names the two runs of the helper; §9 step 3 lists the remaining part |
| `CLAUDE.md` | Version line, §3 (after the theme foundation), §4 (`e2e/`) |
| `docs/sprints/sprint_03/S3-10_dark_appearance_axe.md` (new) | This protocol |

Not changed on purpose: `ci.yml` (same jobs, one more axe run per state); `axe-suppressions.ts` (F5); ARCHITECTURE (C73 is unchanged; the deferred part is step 3 as written in §12); SPEC (P3); the theme files (no finding).

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8. Fixed during the task: the snackbar (F4, owner decision); one `unbound-method` lint finding in the new unit test.

| # | Finding | Sorting |
|---|---|---|
| R1 | After the check the helper emulates `light` explicitly instead of returning to "no emulation" | Won't fix: Playwright's default is `light`, so the page sees the same preference; tests that set their own scheme get it back |
| R2 | A page whose appearance is pinned by an explicit choice (a signed-in legacy user with a saved `darkModeOn`) cannot follow the emulation; the helper then fails with „the page did not switch to the dark appearance“ | Follow-up, no issue yet: no E2E journey signs in today; the explicit setting is the deferred step-3 part of A11Y-08 (P1) |
| R3 | The contrast-mode listener of the same store still opens an English snackbar on a change of `prefers-contrast` | Won't fix here: outside the row, no test switches contrast; replaced with S60 (S3-09 P3, P2 below) |
| R4 | The wait for transitions covers CSS transitions only; the indeterminate loading bar is a CSS animation that never ends and is left out on purpose | – (144 runs in the repeat check, §4 check 6, without a flaky result) |
| R5 | Run time: one more axe run and two appearance switches per state; a local run of three projects took 2.2 min | – (CI runs the projects in parallel jobs) |
| R6 | Security, N+1, data: no API, query or logging change; no new dependency; no UI text added (the removed snackbar texts were hard-coded English) | – |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | Playwright, E2E stack (client image rebuilt, i.e. `build:release`), `chromium`, `firefox`, `mobile-chrome` (WebKit only in CI) | ✅ 141 passed: J1, the shell specs, reflow, appearance and the helper self-tests, every axe call in light and dark |
| 2 | The same before the store change (Chromium) | ❌ 2 failed (F4): compact-menu test (snackbar over „Menü“), loading state (`aria-hidden-focus` on the snackbar in dark) |
| 3 | `npx ng test` (Vitest) | ✅ 32 files, 133 tests (2 new); the 2 new ones fail against the old store |
| 4 | `npm run lint` | ✅ All files pass |
| 5 | `npx tsc --noEmit -p e2e` | ✅ |
| 6 | Repeat: `-g "INV:A11Y-08" --repeat-each=3` on `chromium`, `firefox`, `mobile-chrome` | ✅ 144 passed, none flaky |
| 7 | Probe: only `color-contrast`, dark, `/`, `/filters`, `/products`, `/impressum`, 404 (Chromium) | ✅ 0 violations, 0 incomplete; `<html class="theme-dark">` on each |
| 8 | `node scripts/check-invariants.mjs` | ✅ gate passed; A11Y-08 `E, FE`, 16 tests |
| 9 | `node scripts/check-figma-links.mjs` / `check-i18n.mjs` | ✅ 2 checked, 0 pending / 0 errors |
| 10 | `node --test scripts/*.test.mjs` | ✅ 57 passed |
| 11 | `reuse lint` (Docker image) | ✅ compliant, 687 / 687 files |
| 12 | CI on the pull request (all five projects, incl. `webkit` and `mobile-safari`) | see the pull request |

## 5. Open points

| # | Point | When / who |
|---|---|---|
| P1 | A11Y-08, deferred part: axe with the explicit setting, its persistence in `UserPreferences` and "applies without reload" for it (TESTING_v29 V91, §9 step 3). A signed-in journey then needs the helper to switch the explicit setting instead of the media query (R2) | Step 3, with S60 (R-SET-01/02) |
| P2 | The legacy settings store still opens an English snackbar on a change of `prefers-contrast` and still writes the system appearance to `localStorage` for signed-out users (S3-09 P3) | Step 3 (S60) |
| P3 | SPEC: §11 lists the step-1 invariants without A11Y-08, and the R-SET-03 criterion "A11Y-01 runs on each journey state in both appearances (TESTING follow-up, §14)" is now met by A11Y-08. No SPEC version for this alone | Next SPEC version (owner / documentation session) |
| P4 | The dark values are still the generated ones; the clean axe result does not replace the Figma dark mode and the accessibility review (S3-09 P6) | O-S5, before release |
| P5 | `webkit` and `mobile-safari` run only in CI | CI of this pull request |

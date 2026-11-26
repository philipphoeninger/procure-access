# S7-01 — The causes of the E2E failures on `main`

> **Sprint:** 07 · **Task:** S7-01 (1 point) · **Issues:** #451, #452, #436 (TECH_DEBT_v6 TD-84, TD-85, TD-99)
> **Requirements / invariants:** A11Y-01, A11Y-02, A11Y-03, A11Y-08 stay green; no invariant flipped
> **Status:** done — pull request open, not merged (2026-11-26); the three green runs on `main` follow the merge
> **Scope:** exactly the row of S7-01: the four failures, the shared E2E helpers of TD-99, documents. One client change (the focus return of the compact menu), no server change, no EF migration, no new dependency, no UX key. No Figma comparison: no screen looks different.
> **Owner decisions (2026-11-26, in the session):** (1) the silent repeated 5xx on S80 becomes a proposed issue, not part of this PR; (2) TD-99 replaces all 18 copies of `settled`, not only the three files the register names; (3) #452 (b) is fixed in the shell, not by a test wait; (4) #452 (a) gets the test wait, and the colour ripple under reduced motion becomes a proposed issue.

## 0. Plan (written before the code, confirmed by the owner on 2026-11-26)

- **Starting point:** `main` at f54bee9f. Branch `sprint07/S7-01-e2e-causes`. The Playwright reports of the six failed runs named on the issues were still downloadable, each with its trace.
- **Per failure:** read the trace, decide product or test, fix that one; a product defect gets a unit test that fails first.
- **TD-99:** one support module for the keyboard helpers, bounds for the two download waits.
- **Verification:** repeated runs in CI on throwaway draft pull requests (the S6-06 method), because WebKit does not run locally and another session (S7-09) had containers up on a disk with 0.9 GB free.
- **Invariants:** none activated. **Migration:** none. One session.

## 1. Starting point — the causes

| # | Failure | What the trace shows | Who was wrong |
|---|---|---|---|
| C1 | **#451** Firefox: S80's H1 not focused after a 5xx (`start-and-error-pages.spec.ts:110`; runs 44315856184, 44801181547) | `page.route('**/api/**')` came right after `goto`, while the legacy start-up read `GET /api/Products` (it follows the session probe, `app.store.ts` `init`) was still pending. It got the 500, so S80 showed **before** the click. The click moved the focus to „Kriterien“; S10's `filter-options` 500 asked the router for `error` again, and the router dropped it as the same address (`skipLocationChange`), so nothing moved the focus. The H1 carried `tabindex="-1"` from the first focus. Same order in both runs. Not a late re-render | Test |
| C2 | **#452 (b)** mobile Safari: „Menü“ not focused after Esc (`shell.spec.ts:49`; run 44380257487) | The sidenav container turns its transitions on 200 ms after it is created. The test clicks „Menü“ right after load: the drawer opened without a transition (simulated „opened“: focus on „Kriterien“), then the class arrived and the 400 ms transform ran (the axe helper waited 400 ms for it). Its `transitionend` made the drawer report „opened“ a second time, and the drawer records `document.activeElement` — now „Kriterien“ — as the element to return to. Esc returned the focus there | Product (through Material) |
| C3 | **#452 (a)** mobile Safari: axe found the cards' text `#1c1b1f` on `#141317` in dark (`preferences.spec.ts:63`; runs 44110586418, 44232836143) | The video frame during the axe run shows the cards' text nearly invisible; 130 ms later it is right. The H1 had switched. The text colour is `--mat-sys-on-background` from `mat-drawer-container` (not pinned, light half = neutral 10). Diagnostic run (#463, 60 repeats): under reduced motion `html.motion-reduced * { transition-duration: 0.01ms }` gives every element a transition of every property (`transition-property` defaults to `all`); 18 – 43 `color` transitions were listed after the switch. An inherited colour reaches one DOM level per frame, so the deep levels' transitions start only after their parents'. **In 38 of 60 runs the card's colour was still light after the helper's wait**, which only saw the transitions running at its start | Test (the wait); the ripple itself is a product question (P2) |
| C4 | **#436** WebKit: sole-Owner test timed out (`account.spec.ts:120`; run 45184273467) | `fill` ended 65 ms after the dialog opened; MatDialog's first focus (`autoFocus: 'first-heading'`) then took the focus, the typed text went nowhere, the field stayed empty and „Konto endgültig löschen“ unavailable | Test (CLAUDE §3: wait for the first focus) |
| C5 | TD-99 | `settled(locator)` identical in 18 specs; `tabTo` in J1, J2, `assessment.spec.ts` (bounds 80, 80, 30); `arrowTo` in J1, `arrowToOption` in J2; `waitForEvent('download')` without a bound in `account.spec.ts:60` and `s11-list.spec.ts:324` | Hygiene |

## 2. Changes

1. **#451 — `e2e/shell/start-and-error-pages.spec.ts`.** The S80 test routes only `CATALOG_API` (`**/api/v1/catalog/**`, S10's own reads) to the 500, so a start-up read cannot show S80 before the click. The 5xx check of the error interceptor comes before `CALLER_HANDLES_PROBLEMS`, so S10's read still shows S80.
2. **#452 (b) — `app.ts`, `app.html`, `layout/header/header.{ts,html}`.** The drawer's `closedStart` calls `App.menuClosing()`: it closes the menu and, while the focus is still in the menu or nowhere, moves it to „Menü“ through `Header.focusMenuButton()`. A chosen item that opens a page keeps the page's move to its H1 (A11Y-03): by then the focus has left the menu, or the H1 takes it after this. **`app.spec.ts`:** „returns the focus to „Menü“ on Esc, also after a late transitionend of the drawer“ dispatches the drawer's `transitionend` after it opened; it failed without the fix (focus on `body`) and passes with it. A second test covers „Menü schließen“ (passed before and after). Both wait for the drawer's own `setTimeout` (`drawerSettled`), which `whenStable` does not cover.
3. **#452 (a) — `e2e/support/axe.ts`.** `settled` waits frame by frame until a whole frame passes without a running finite animation (at most 200 frames, then it fails with a message); `showAppearance` uses it instead of its own one-round wait for transitions. No fixed sleep.
4. **#436 — `e2e/identity/account.spec.ts`.** The sole-Owner test waits for the heading „Konto endgültig löschen?“ to be focused before it types, as the other deletion test does.
5. **TD-99 — `e2e/support/keyboard.ts`** (new): `tabTo` (Tab or Shift+Tab, at most 80 presses), `arrowTo`, `arrowToOption` with `activeOption`, `settled(locator)` and `DOWNLOAD_TIMEOUT` (30 s, with the #380 reason from J1). The 18 specs import it; the two download waits have the bound.
6. **Docs.** CODEBASE §2.0, §2.118, §3 (`support/`) and §1 (change history of `CLAUDE.md`); `CLAUDE.md` §3 E2E: the helpers come from `support/keyboard.ts`, every download wait gets `DOWNLOAD_TIMEOUT`. TESTING unchanged (no invariant, no new verification row needed beyond the protocol); TECH_DEBT stays at v6 (new version at the end of the sprint: TD-84, TD-85, TD-99 closed by this PR).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: one client focus change and test code; no secret, no request, no new input. Correctness: the focus moves only when it is in the closing menu or nowhere; checked against the page change after a chosen item (H1 first or later — either way the H1 keeps it), the backdrop click (focus on `body`: back to „Menü“, as Handoff §2.2 wants) and the switch to the wide layout (the drawer goes; `focusMenuButton` does nothing without the button). N+1: not applicable. Accessibility: A11Y-03 unchanged; Handoff §2.2's focus return now holds in the race too.

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | `support/keyboard.ts` also holds `settled` and `DOWNLOAD_TIMEOUT`, which are not keyboard helpers | won't fix | TD-99 asks for one shared module; the file's comment names both |
| R2 | `assessment.spec.ts` gave up after 30 Tab presses, now after 80 | won't fix | The bound only decides how soon a failing test stops; passing runs are unchanged |
| R3 | `menuClosing` relies on Material emitting `closedStart` after its own restore | won't fix | That order is Material's code (`_setOpen` restores, `closedStart` follows in a task); the unit test fails if it changes |
| R4 | A repeated 5xx while S80 shows in place is dropped by the router: no focus move, title or announcement after the user's click | follow-up issue (P1) | Owner decision (1) |
| R5 | Under reduced motion an appearance switch ripples down the DOM for many frames | follow-up issue (P2) | Owner decision (4) |

## 4. Checks

- Client: `ng test` 108 files, 1157 tests passed; `npm run lint` clean (after typing the two view queries, which lint first flagged as `any`).
- E2E package: `npm run typecheck` and `eslint .` clean.
- `node scripts/check-invariants.mjs` passed; `check-figma-links.mjs` 12 checked, 0 pending; `check-i18n.mjs` 0 errors; `check-icons.mjs` 0 errors; `reuse lint` (Docker) compliant, 1764 / 1764 files.
- Server: not touched; CI runs its stages.
- **Diagnosis in CI** (draft #463, closed without merging): mobile Safari, the dark-appearance test ×60 with the colour log, all passed; the log named C3 (§1).
- **Repeat run in CI** (draft #465, closed without merging; run 45840858451): RESULTS_PLACEHOLDER
- E2E of the whole suite: CI of the pull request. Not run locally (S7-09's containers up, 0.9 GB free, TD-74).
- **After the merge:** three consecutive green runs on `main` — links in the pull request when they exist.

## 5. Open points

1. **Proposed issue — S80 after a second 5xx (R4).** While S80 shows in place, a click on a link whose page answers 5xx again leaves everything as it was: the router drops `navigate(['error'])` as the same address, so neither the title, the focus nor an announcement tells the user that the attempt failed. Fix candidate: the error interceptor calls `PageTitleStrategy.changePageInPlace('pageTitle.server')` when S80 already shows. R-SHL-04, R-SHL-08, A11Y-03.
2. **Proposed issue — the colour ripple under reduced motion (R5, Handoff §1.7).** `html.motion-reduced * { transition-duration: 0.01ms }` gives every element a transition of every property; an inherited colour change (appearance, contrast) then runs down the DOM one level per frame, visible as a wave of several frames — motion for the users who asked for none. Candidates: suspend transitions while `AppearanceService` switches its classes, or narrow the rule. Needs a design decision.
3. **`mat-sidenav` records its restore target on every `transitionend` of its own** (C2). The shell no longer depends on it; reporting it to Angular Components is optional.
4. **The legacy start-up read `GET /api/Products`** runs after the session probe on every page; it goes with `withProducts()` in sprint 08 (M6.6d). Until then a test that routes every API request after `goto` can catch it (the offline and slow-answer tests of the same spec do, harmlessly: their outcome is the same).

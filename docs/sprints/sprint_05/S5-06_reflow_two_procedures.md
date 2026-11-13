# S5-06 — Reflow check with a two-procedure card

> **Sprint:** 05, task S5-06 (0.5 points). **Issues:** #312 (`TECH_DEBT_v4.md` TD-54). **Requirements / invariants:** A11Y-09 stays `active`; R-SHL-10, R-LST-01.
> **Status:** done — pull request open.
> **Scope:** `e2e/shell/reflow.spec.ts` opens a criterion card with two procedure tabs at 320 × 256 px; the tab padding below 600 px.
> **Owner decisions:** none.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The reflow check opened only the first card's panel; J1's selection (web, audio) gives one procedure per criterion | No test saw a `mat-tab-group` at 320 px |
| F2 | `T.1` already has such a card: `99.1` has texts for Software-Test and App-Test, and the procedure mapping ties „Mobile Anwendung (offen)“ to both | No change to `T.1`, `T.2` or the MiniCatalog |
| F3 | „Mobile Anwendung (offen)“ excludes Audio (`T.1` rules); HR-Anwendung does not exclude it | Selection: HR-Anwendung, Mobile Anwendung (offen), Basisfunktionalität only, no test type |
| F4 | Measured before the fix (Chromium, Firefox): header 222 px wide, labels 139 + 107 px with Material's 24 px label padding, pagination buttons shown, second label cut | The task's fix applies |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `e2e/shell/reflow.spec.ts` | New test `S11 with a card of two procedures (tabs) …`: selection of F3, opens the details of `99.1`, expects two tabs, no sideways scrolling, no pagination buttons, then selects each tab by keyboard (Home, ArrowRight, Enter) and checks it is selected, in the viewport (≥ 99 %) and inside the tab header | INV:A11Y-09 (the describe title carries it) |
| `CLIENT/CLIENT.ProcureAccess/src/styles/main.scss` | `.pa-criterion-card__tabs .mat-mdc-tab.mdc-tab` gets `padding-inline: var(--pa-space-md)` below 600 px (24 px from 600 px, Material's default) | Handoff §2.2 |
| `CLAUDE.md` §3 | One sentence about the new reflow state | — |

After the fix the labels are 127 + 95 px and fit the 222 px header, so Material shows no pagination. The header following the selected tab was already correct in Chromium and Firefox (the keyboard part of the test passed against the old padding), so only the padding changed.

## 3. Self-review

- Fix: no correctness or security finding. The fit is tight: a wider font or a third procedure brings Material's pagination back, which stays reachable by keyboard and is what Handoff §2.2 allows (the bar scrolls inside itself, never the page). Only the test's „no pagination“ assertion would then fail, which signals that the labels need a new look.
- Won't fix: wrapping the labels (Material tab labels are single-line; no token for it).
- The selector is global in `main.scss`, because Material's tab markup is outside the component's encapsulation; `ng-deep` is used nowhere else in the code base.

## 4. Checks

| Check | Result |
|---|---|
| `npx playwright test shell/reflow.spec.ts` on the E2E stack, projects chromium, firefox, mobile-chrome | 42 passed (before the padding change the new test failed on the pagination assertion) |
| `npm run typecheck`, `npm run lint` in `e2e/` | green |
| WebKit, mobile Safari | left to CI (no local WebKit) |
| `check-invariants`, `check-figma-links`, `check-i18n`, `reuse lint` | see the pull request |

## 5. Open points

- `TECH_DEBT_v4.md` TD-54 / #312 is done; the register changes with the next version at the end of the sprint (process step 14).
- A tab header with three procedures or a font wider than Roboto was not checked; no such state exists on `T.1`.

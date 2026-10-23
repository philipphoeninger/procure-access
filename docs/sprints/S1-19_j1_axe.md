# S1-19 — Journey J1 in its current form, axe on every state

> **Sprint:** 01, task S1-19 (milestone M0.5, TESTING §5.4, §7.1, §9 step 0, §12.4; invariants CAT-10 E2E part, A11Y-01). **Status:** done by Claude Code (2026-10-23).
> **Scope:** journey J1 as today's screens allow it: criteria filter on `/filters` without an account, with axe after every significant state change. Every axe violation found is recorded as a GitHub issue and either fixed or suppressed by rule and selector with the issue link. Not in scope: J1 anonymous part with rationale, priority change, exclusion of a criterion and XLSX export (step 2); J1 save part (step 3); A11Y-01 and CAT-10 to `active` (S1-21); CI (S1-20); keyboard-only variant (A11Y-02, step 2); dark appearance (A11Y-08, step 1).
> **Owner decisions 2026-10-23:** **D1** suppress with an issue; fix only what needs no new wording; findings rated `blocker` by the owner or tester are fixed with wording from them. **D2** Claude creates the issues with `gh` after review, with a new label `a11y`; severity is left to the tester (TESTING §7.2). **D3** no export step in J1 (today's button downloads the page, S1-06 K2). **D4** a second test for the empty selection (S1-15 P3). **D5** E2E images built locally.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `/filters` is a two-step `mat-stepper`. Step 1 has an accordion with four layers: Produkt-Typen (autocomplete), App-Typen, Produkt-Teile and Test-Typen (each split into two `mat-selection-list`s). Step 2 has one expansion panel per criterion | J1 walks exactly these states |
| F2 | Choosing a product type in the autocomplete *replaces* the whole selection | J1 chooses the product type first |
| F3 | Excluded options are only `disabled` (`aria-disabled="true"`), with no reason shown; every criterion shows the fixed text "Priorität: 1" | J1 checks the disabled options and the fixed priority as today's behaviour; reasons and rationale come in step 2 |
| F4 | In the E2E stack (API `Development`) the criteria come from migration `AddSeedData`; `SampleDataInitializer` only adds users | Same seed as the CAT-10 API fixture, so J1 reads its expected criteria from that fixture |
| F5 | On the Pixel 7 viewport, a tap on the autocomplete lands on the floating `mat-label` (Playwright: "intercepts pointer events"); in a real browser that tap still focuses the input | J1 opens the panel with `focus()`. Not an a11y finding |
| F6 | J1 in Firefox needs about 30 s under parallel load (7 axe runs of 1–3 s each) and hit the default timeout in 3 of 30 repeated runs | `test.slow()` on the long test (90 s). After that, 60 of 60 repeated runs passed |

## 2. Axe findings and what happened to them

A temporary probe (deleted, not committed) ran axe with the helper's tags on every J1 state and on the empty list, in Chromium, Firefox and mobile Chrome. It found four rules. No WCAG 2.2 findings.

| Issue | Rule (impact) | WCAG | Where | Browsers | Outcome |
|---|---|---|---|---|---|
| [#52](https://github.com/philipphoeninger/procure-access/issues/52) | `aria-tab-name` (serious) | 4.1.2 | both stepper headers on `/filters` (`pa-products-container`) | all three | **Suppressed**: rule `aria-tab-name`, selector `pa-products-container mat-step-header`. A name needs wording for today's two steps, and the UX copy has none; S10 replaces the stepper in step 2 |
| [#53](https://github.com/philipphoeninger/procure-access/issues/53) | `label-content-name-mismatch` (serious) | 2.5.3 | header `.home-btn` ("Home button" vs "ProcureAccess"), `.products-page-btn` ("Products page button" vs "Produkte anzeigen") | all three | **Fixed**: English `aria-label`s removed; the name comes from the visible content. This also fixes the home-page finding from S1-18 (check 8, P8) |
| [#54](https://github.com/philipphoeninger/procure-access/issues/54) | `aria-input-field-name` (serious) | 4.1.2 | the six `mat-selection-list`s in `pa-filters-selection` | all three | **Fixed**: `[attr.aria-label]="filterType.name"` (catalog data, no new UI text); both halves of a layer carry the same name |
| [#55](https://github.com/philipphoeninger/procure-access/issues/55) | `button-name` (critical) | 4.1.2 | header `#menu-btn` (icon only, narrow layout) | mobile Chrome | **Fixed**: `aria-label` from the existing UX copy key `shell.nav.menu` „Menü“ (ux_copy v14), added to `de.json` |

All four issues carry the labels `a11y` and `bug`; the label `a11y` is new. #53–#55 are closed by this pull request, and #52 stays open until the suppression goes.

## 3. Changes

| File | Change | Why |
|---|---|---|
| `e2e/journeys/j1-configurator.spec.ts` (new) | `INV:CAT-10 INV:A11Y-01` J1: open `/filters` → expand all layers → autocomplete open → HR-Anwendung chosen (6 options disabled) → Website / Webanwendung, Software (offen), Basisfunktionalität, Video, Zweiwege-Sprachkommunikation, BIT inklusiv Software-Test (8 options disabled, "7 ausgewählt") → "Enter". The test checks the request ids, then that the criteria titles equal the CAT-10 fixture case `j1-like-selection` (109 names, unordered), then opens one criterion. Axe runs on 7 states. Second test, `INV:A11Y-01`: "Enter" without a selection shows the empty-list text, with axe on that state | Done when; D3, D4. A second app type does not change the set, because app types carry no criteria in the seed (S1-15 F3) |
| `e2e/support/axe-suppressions.ts` | First entry: `aria-tab-name` on `pa-products-container mat-step-header`, #52 | TESTING §12.4 |
| `CLIENT/…/layout/header/header.html` | `#menu-btn` gets `aria-label` „Menü“; `aria-label="Home button"` removed | #55, #53 |
| `CLIENT/…/products-container.html` | `aria-label="Products page button"` removed | #53 |
| `CLIENT/…/filters-selection.html` | Both selection lists get `[attr.aria-label]="filterType.name"` | #54 |
| `CLIENT/…/public/i18n/de.json` | New key `shell.nav.menu` = „Menü“ (exact UX copy wording) | CLAUDE §6.5; the other languages fall back to German (`setFallbackLang`) |
| `CLAUDE.md` §3, §4; `e2e/README.md` | Current state and layout: J1 exists | CLAUDE §11 |

No backend code, no dependency, and no ⚖️ text changed.

## 4. Checks

Node 24.22.1 / npm 11.19.1; Docker 29.9.1 / Compose 5.6.0; Playwright 1.64.1, axe-core 4.14.0.

| # | Check | Result |
|---|---|---|
| 1 | `npm run stack:up` (first build, then rebuild with the fixes) | ✅ db, api, client healthy |
| 2 | Before the fixes, with J1 and an empty suppression list | ❌ as expected: `aria-tab-name` and the other findings in §2 |
| 3 | After the fixes, without the suppression | ❌ as expected: only `aria-tab-name` (2 nodes; 1 on the empty list) |
| 4 | `npx playwright test --project=chromium --project=firefox --project=mobile-chrome` | ✅ 39/39 (13 tests × 3) |
| 5 | J1 `--repeat-each=5` on the same three projects, twice | ✅ 30/30 and 30/30 (after F6; before: 28/30 and 29/30, Firefox timeouts) |
| 6 | All five projects | 45 passed, 20 failed: every failure is a `browserType.launch` on `webkit`/`mobile-safari` (S1-18 F9). The first full run reported 21 failures; the extra one was not captured, and two reruns showed only the 20 launch failures |
| 7 | Negative: one criterion name changed in the CAT-10 fixture (all three cases) | ✅ J1 fails and shows the changed name in the diff; file restored |
| 8 | `npm run typecheck` (e2e) | ✅ |
| 9 | `dotnet build ProcureAccess.sln --no-incremental` / `dotnet test --solution ProcureAccess.sln` | ✅ 0 errors, 136 warnings (unchanged) / ✅ 22/22 |
| 10 | `npx ng test` / `npm run build:release` | ✅ 2 files, 5 tests / ✅ 1.81 MB initial, only the known EasyMDE selector warning |
| 11 | `reuse lint` | ✅ compliant |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Set the severity (`blocker` / `major` / `minor`) on #52–#55; a `blocker` on #52 would mean fixing it now, with step labels from the owner | Philipp / tester |
| P2 | Suppression #52 is checked only on Chromium, Firefox and mobile Chrome; WebKit and mobile Safari run first in CI | S1-20 |
| P3 | Axe does not see everything: the header icon buttons still have English names ("Search icon-button", "Filters icon-button", …), and the "Enter" button text is English (`COMMON.ENTER`). These are WCAG 3.1.2 (language of parts) or plain-language matters for the manual pass, not axe rules. They need UX copy wording | Manual pass (TESTING §7.2) / shell rebuild, step 1 |
| P4 | The home page is not part of J1. Its known `label-content-name-mismatch` was the header button and is fixed; other states (login overlay, products, settings) are covered only when a journey visits them | Later journeys |
| P5 | J1 relies on Material markup for the criteria titles (`mat-expansion-panel-header mat-panel-title`) and on `/filters`; step 2 rewrites J1 for S10 anyway | Step 2 |
| P6 | The new key `shell.nav.menu` is a leaf in nested `de.json`; the UX copy also has `shell.nav.menu.close`. ngx-translate can read that as a flat key, but the step-1 shell should settle the key layout | Step 1 |
| P7 | 3 of 30 runs timed out before `test.slow()` (F6). With 5 projects in CI on small runners, timeouts may come back. Watch the first CI runs before raising `workers` or the timeout further | S1-20 |
| P8 | The E2E images take about 2 GB; 4.5 GB were free before this task | Philipp |

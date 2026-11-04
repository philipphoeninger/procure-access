# S2-18 — 320 px / 400 % zoom check and E2E on the new shell

> **Sprint:** 02, P0 task S2-18 (1 point; roadmap M1 shell). **Issues:** none. **Requirements / invariants:** R-SHL-10; A11Y-01, A11Y-03 and CAT-10 (E2E part) stay green; no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** reflow check at 320 px / 400 % zoom of every page inside the shell (new shell pages and legacy screens); J1 and the CAT-10 E2E part on the new shell (locators only, expected sets unchanged); start page of the compact E2E tests (S2-15 P3, S2-17 P6). Not in scope: fixing legacy screens (rebuilt from step 2 on), the manual screen-reader pass (A11Y-07), the dark appearance (sprint 03, A11Y-08).
> **Owner decisions 2026-11-04 (before the task, asked by Claude):** **1b** check the new shell pages and the legacy screens inside the shell; fix only shell pages, legacy findings go into one follow-up issue. **2** this protocol lives in the sprint folder and the PR, marked as a developer check, not as a tester pass in `docs/a11y-reports/` (that folder is the A11Y-07 evidence of the gate). **3** a permanent reflow E2E test without an invariant ID; a reflow invariant is proposed at the sprint close-out. **4** small CSS findings on shell pages are fixed in this PR with tokens; anything bigger becomes an issue whose severity the owner sets. Defaults accepted with them: J1 keeps `page.goto('/filters')`, light appearance only, DOM order = visual order is checked through the Tab order, WebKit and mobile Safari only in CI.

**This is a developer check, not a tester pass (TESTING §7.2).** It emulates 400 % zoom with a 320 × 256 CSS px viewport (a 1280 × 1024 window at 400 %, `deviceScaleFactor` 4) in Playwright, not with the zoom of a real browser and assistive technology. The A11Y-07 pass of the project team's tester stays open (P6).

## 1. Starting point and check

| # | Finding | Consequence |
|---|---|---|
| F1 | J1 and the CAT-10 E2E part already run on the new shell: S2-15 changed the only locator the shell touched (`Enter` → `Weiter`, twice); the CI run of PR #167 had J1 green on all five browser projects | No change to J1; it stays green locally (Chromium, Firefox, mobile Chrome) |
| F2 | The compact tests of `e2e/shell/shell.spec.ts` started on `/barrierefreiheit` because the legacy start page was wider than 360 px; S2-17 rebuilt it | They start at `/` |
| F3 | At `/`, `getByRole('link', { name: 'Kriterien' })` also matches the start-page card „Kriterien zusammenstellen“ | Locator `exact: true` |

**Method.** A probe script (not in the repository) loaded each route in Chromium and Firefox at 320 × 256 and at 360 × 780 and recorded the document width against the viewport, every element that crossed the viewport edge outside a scroll container, clipped text, the H1, the Tab order (each stop's position against the previous one) and a full-page screenshot. The screenshots were looked at. The open compact menu, the offline notice and the criteria list after a selection were checked as states.

**Results at 320 × 256 (Chromium and Firefox agree; 360 × 780 gives the same findings):**

| Page / state | Sideways scrolling | Tab order = visual order | Result |
|---|---|---|---|
| `/` start page | none | yes | pass |
| `/gibt-es-nicht` (404), `/unauthorized` (403, also via `/proposals`), `/error` (500) | none | yes | pass |
| `/impressum`, `/barrierefreiheit` | none | yes | pass |
| `/datenschutz` | **371 px** (H1 „Datenschutzhinweise“ in the browser's default 2em bold) | yes | **F4, fixed** (§2) |
| Compact menu open | none; the menu scrolls vertically, „Anmelden“ is reached with Tab and scrolled into view | yes (focus trap) | pass |
| Offline notice | none | yes | pass |
| Header | not fixed (`position: static`), so it does not take a quarter of the 256 px | – | pass |
| `/filters` (step 1, all layers open; step 2, criteria list) | none | yes | pass (legacy) |
| `/products` | none for the page; the table is 714 px wide in a 288 px scroll box whose `max-height: calc(100vh - 156px)` leaves one row visible | the position check reported jumps, caused by the box scrolling between stops; recorded by row and cell index, the order follows the rows (sort headers, then per row details button → website link) | **L2** (legacy) |
| `/product/1` | none | yes | pass for reflow; English `alt` text (L3) |
| `/settings`, `/reset-password`, `/confirm-email` | none | yes | pass (legacy) |
| Login outlet `/(login:auth)` (from „Anmelden“, `/favorites`, `/profile`, `/product-proposal/0`, `/criterion-proposal/0`) | **411 px** (431 px at 360); fields and „Passwort vergessen“ cut off on the left | – | **L1** (legacy) |
| `/auth`, `/register` | none (404 page: they exist only in the `login` outlet) | yes | pass |

No style of the shell reorders content visually (no CSS `order`, no `*-reverse`, no grid areas; the only `position: absolute` is the skip link).

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/pages/legal/legal-page.scss` (new), `legal-page.html`, `legal-page.ts` | H1 class `pa-legal-page__title`: `--mat-sys-headline-medium` (Handoff §1: „Doc pages only“), `margin: 0`, `hyphens: auto` (`<html lang="de">`) and `overflow-wrap: break-word`; the host takes the full width of `main` (`main` aligns its children to the start, so without it the H1 sized the column and still reached into the page padding) and spaces H1 and notice with `--pa-space-md` | F4; decision 4 |
| `e2e/shell/reflow.spec.ts` (new) | At 320 × 256: the seven pages of the new shell (each waits for its H1), the open compact menu (every item can be scrolled into view) and the offline notice (app loaded first, then the API cut, as in `start-and-error-pages.spec.ts`). Fails when the page is wider than the set viewport or a visible link or button lies outside it. Compared with the set width, not with the document: mobile browsers widen the layout viewport to fit content. No `INV:` tag | Decision 3 |
| `e2e/shell/shell.spec.ts` | Compact tests start at `/` (constant `NARROW_PAGE` removed); `Kriterien` matched exactly | F2, F3 |
| `CLAUDE.md` | §3 current state, §4 layout | Document upkeep |

J1 (`e2e/journeys/j1-configurator.spec.ts`) and the CAT-10 fixture are unchanged (F1). `route-change.spec.ts` keeps its start on `/barrierefreiheit`: it needs a page with an H1 and a footer link to a second one, which is still true. `figma_code_mapping_procureaccess_v1.md` is unchanged: S71 has no Figma frame. No TESTING version: no invariant changes status, and the reflow test has no ID (decision 3).

**The test can fail:** a temporary copy of the spec with injected CSS (an H1 of 80 px without wrapping; footer links pushed out of a clipped footer) failed in Chromium and mobile Chrome with the page width (469 px) and with the names of the links outside the viewport; the copy was deleted.

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y) with the skill *engineering:code-review* on the whole diff. Findings sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | The first fix (headline token + `overflow-wrap`) removed the sideways scrolling but did not wrap the H1: `main` has `align-items: start`, so the legal page took the width of its longest word and the H1 still ran 10 px into the right padding | **Fixed:** host `inline-size: 100%` (as `pa-home`) and `hyphens: auto`; measured in Chromium and Firefox: the H1 ends at the column edge (304 px) on two lines |
| R2 | `checkVisibility({ visibilityProperty: true })` is newer than the rest of the API; where WebKit ignores the option, the links of the closed compact menu (`visibility: hidden`, moved off-screen to the right) would count as outside and fail the test only in CI | **Fixed:** plain `checkVisibility()` plus an explicit `getComputedStyle(…).visibility === 'visible'` |
| R3 | An icon-only button would be listed with an empty name in a failure message | **Fixed:** falls back to the tag name |
| R4 | On a first page load without API answers the offline notice can stay hidden: the translation file `de.json` is loaded through the same `HttpClient` and the error interceptor, and when its answer comes after the failed API calls, `ConnectionStatus.answered()` switches the notice off (reproduced by delaying `de.json` by 800 ms; this is why the first version of the offline reflow test failed) | **Follow-up issue #171 (P2):** it is S2-17 behaviour, not reflow; the test loads the app first, as `start-and-error-pages.spec.ts` does |
| R5 | The browsers hyphenate „Datenschutzhin-weise“ (Chromium) and „Datenschutzhinwei-se“ (Firefox), not at the word joint | **Won't fix here:** a soft hyphen in the text is a UX copy change (P3) |
| R6 | `pa-home` and `pa-error-page` use `headline-medium` for their H1 on every width; Handoff §1 says `headline-small` below 600 px and `headline-large` from 600 px, `headline-medium` only for doc pages | **Follow-up (P4):** not a reflow finding (both fit 320 px); S2-17's choice, to be settled with the theme task |
| — | Security: no new input, no endpoint, no secrets, no dependency; N+1: no backend change | – |

## 4. Checks

| Check | Result |
|---|---|
| `npm run build:release` | green (the critical-CSS warning about one selector exists on `main` too) |
| `npx ng test` (Vitest) | 24 files, 110 tests green |
| `npm run lint` | green, no new `LEGACY_EXCEPTIONS` entry |
| E2E (`npx playwright test`, Chromium, Firefox, mobile Chrome; WebKit and mobile Safari only in CI) | 117 passed, incl. J1 / CAT-10; `reflow.spec.ts` three times in a row (`--repeat-each=3`): 81 passed |
| `npx tsc -p .` in `e2e/` | green |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors |
| `reuse lint` | compliant (660 / 660 files) |
| Backend (`dotnet test`) | not run: no backend change |

## 5. Open points

| # | Point | Where / when |
|---|---|---|
| P1 | **#170** (decision 1b): „a11y: legacy login outlet and product list do not reflow at 320 px / 400 % zoom“, labels `a11y`, `tech-debt`, milestone M3, severity `minor` (owner, 2026-11-04). **L1** login outlet 411 px wide (fields cut off), replaced by S01 in step 3; **L2** `/products` table in a one-row scroll box, 714 px wide (Handoff §2.2: `<ul>` below 600 px), English icon-button names (pattern #120), 20 px high links (CLAUDE §9: ≥ 48 px), replaced by S40 in step 6; **L3** `/product/1` English `alt` „Stock image of an accessible document“ on an image that does not load, S41 in step 6. No legacy fix (severity `minor`); each rebuilt screen joins `reflow.spec.ts` | Rebuild of each screen (steps 3 and 6) |
| P2 | **#171** (R4): the offline notice can be switched off by the answer of a static file (`de.json`) on the first page load. Fix options: count only answers of `/api/` requests in the error interceptor, or load translations without the interceptor; plus a unit test „static answer after a failed API call keeps the notice“ | S2-17 follow-up, before step 2 |
| P3 | Soft hyphen in the legal titles (e.g. „Datenschutz­hinweise“ with U+00AD) so the browsers break at the word joint (R5) | UX copy, any version |
| P4 | H1 type scale of S04 and S80 against Handoff §1 (R6) | Theme task (M1.5, sprint 03) |
| P5 | Reflow invariant (decision 3), e.g. „A11Y-09: every page of the shell and every rebuilt screen needs no horizontal scrolling at 320 × 256 CSS px“, tagged on `reflow.spec.ts` | Sprint 02 close-out / next TESTING version |
| P6 | Manual 320 px / 400 % pass with real browser zoom and the tester's screen reader (TESTING §7.2 mobile-first checks) | A11Y-07, end of step 2 or before the pilot |
| P7 | WebKit / mobile Safari only in CI | CI of this PR |

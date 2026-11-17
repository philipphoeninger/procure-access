# S5-32 — Design fidelity of S10 and S11

> **Sprint:** 05 · **Task:** S5-32 (stretch, 1 point) · **Issues:** #278, #281, #282
> **Requirements / invariants:** SPEC R-LST-01 (S11 per Handoff), R-LST-10 (action bar), R-SHL-10; A11Y-01 and A11Y-09 stay green (no invariant flipped, no new TESTING version); Handoff 1.32 §2.2, §5.1; Figma mapping v1.27
> **Status:** done — pull request open (2026-11-17)
> **Scope:** styles of the S10 stepper and of S11 — the title row, the sticky action bar, the summary terms — compared with their Figma frames before and after; no behaviour change. Client only: no server change, no EF migration, no new dependency, no new UX copy key.
> **Owner decisions (2026-11-17):** **1 (a)** — the full-width sticky bar applies to S11 and a saved list, which share `pa-criteria-action-bar` · **2 (a)** — the title row on S11 only; a saved list keeps its title and actions stacked for now (open point 1) · **3 (a)** — the bar's background spans the viewport while its status and buttons stay in the content column, although the frame places them 32 px from the viewport edges (open point 2).

## 1. Starting point

Compared with the frames of the design file through the Figma MCP connection (`get_metadata`, `get_design_context`):

| Point | Figma | Code before |
|---|---|---|
| #278 title row | `2020:102`: H1 left, „Auswahl ändern“ right on one 48 px line, centred | H1, then the button in a `<div>` below it on every width; no title-row rule |
| #281 action bar | `2034:1193`: 1440 wide in the 1440 frame, no corners, padding 12 / 32 px, status `label/large` | as wide as the 1040 px column, `corner-medium`, padding 16 px, status `body-medium` |
| #282.1 stepper | `2019:457`: steps 32 px high, circles 32 px, number `label/medium`, label `label/large`, gap 8 px | circles 24 px (`1.5rem`) |
| #282.2 summary | `2101:3160`: `dt` `label/medium` (12/16, 500) in `on-surface-variant`, 160 px column, gap 16 px; `dd` `body/medium` | `dt` `label-large` (14/20, 500); column and colour already as in the frame |
| #282.3 filter row | settled with #283 (comment on #282): the frame shows the layout as built | no change; the screenshots of S11 at 1440 px match it |

The stepper frame of step 3 (`2088:2200`) shows the number in a finished step; the code shows the check icon that Handoff §5.1 asks for (open point 3).

## 2. Changes

### 2.1 Client (styles and one wrapper)

- **S11 title row** — `criteria-list.html` wraps H1 and „Auswahl ändern“ in `.pa-criteria-list__title-row` (instead of the button's own `<div>`). `criteria-list.scss`: stacked and left-aligned with the old 24 px gap below 600 px; from 600 px one row, H1 at the start (`min-inline-size: 0`, so a long title wraps), the button at the end (`flex: none`), centred on the cross axis, 16 px apart. DOM order H1 → button; `focusPageHeading` still finds the H1 in `main`.
- **Action bar** — `criteria-action-bar.scss`, inside the existing sticky query: `border-radius: 0`, `box-shadow: 0 0 0 100vmax var(--mat-sys-surface-container-high)` and `clip-path: inset(0 -100vmax)`. The shadow paints the bar's colour to both viewport edges and the clip keeps it to the bar's height. Only the paint overflows, so nothing scrolls sideways, and the box stays in the column: status and buttons stay aligned with the list (decision 3 (a)) and the `scroll-padding-bottom` and `focusin` rule of S4-18a measure the same box. In the flow (below 600 px width or height) the bar keeps its corners. A saved list gets the same, since it uses the same component (decision 1 (a)). `app.scss` notes that the shell's `overflow: visible` is what lets the background reach the edges.
- **Stepper** — `stepper.scss`: circles `2rem` (32 px). The number already used `label-medium`; the check icon stays 18 px.
- **Summary** — `criteria-list-summary.scss`: `dt` in `label-medium`. The component is also used on a saved list and S16.

### 2.2 Tests

- `e2e/configurator/s11-list.spec.ts`, two new tests:
  - „Auswahl ändern“ is on the H1 line from 600 px and below it on narrow screens, after it in the DOM (#278): at 1280 px on the H1's line and at the row's end; at 360 px below the H1 at its start; H1 before the link in the DOM.
  - While sticky the action bar spans the viewport without corners, its content in the column (#281): square corners; its box inside the column; the pixels of a one-line screenshot (`scale: 'css'`, so the mobile projects sample the same x) at both viewport edges have the bar's colour (±2 per channel); `scrollWidth` equals the viewport width; at 560 px height (in the flow) the corners are back. The screenshot is decoded in the page through `createImageBitmap` and an `OffscreenCanvas` (`pixelsAt`), so no image library is needed.
  - Both failed against the client of `main` (button below the H1; radius 12 px) and pass on the branch.
- No new component tests: the change is CSS, which jsdom does not lay out. The existing specs of the page, stepper and summary stay green.

### 2.3 Documents

- **Handoff 1.32** — §2.2: the stepper row (32 px circle with number or check), the „S11 action bar“ row (background across the viewport while sticky, no corners, content in the column); §5.1 S11: the title row and the summary term style in the order above the list; the sources name Figma mapping 1.27.
- **Figma mapping v1.27** — the comparison and the two kept differences (content in the column, the check of a finished step); the related line names Handoff 1.32. No node ID changed.
- **`CODEBASE_v1.md`** — §2.83.
- **`CLAUDE.md`** — §3 state of the code and „Last task reflected: S5-32“ (37 960 characters).
- Not changed: SPEC, ARCHITECTURE, TESTING (no requirement, structure or invariant changes); UX copy (no text); `LEGACY_EXCEPTIONS` (no listed file touched); `TECH_DEBT_v4.md` — its row #278/#281/#282 is closed with the register version at the end of the sprint (process step 14).

## 3. Self-review

CLAUDE §6.7 checklist and `/engineering:code-review` over the diff; findings sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The mobile projects in CI (Pixel 7, iPhone 15) take screenshots at their device scale, so the right-edge sample would have landed in the middle of the screen and tested less | **fix** — `scale: 'css'`; the S11 spec passes on Chromium, Firefox and mobile Chrome (30 / 30) |
| R2 | In forced-colours mode (Windows contrast themes) the browser drops the `box-shadow`, so the bar is only as wide as the column there; it has no border in either mode, so the content scrolling under it has no visible edge. The missing edge existed before S5-32 | **follow-up issue** (proposed, open point 4) |
| R3 | `pixelsAt` loads the screenshot from a `data:` URL in the page; a later Content-Security-Policy with a strict `connect-src` would block it. The client sends no CSP today (checked against the nginx image) | **won't fix** — if a CSP comes, the test adds `data:` to its exceptions or decodes in Node |
| R4 | Prettier reports `criteria-list.html` and `s11-list.spec.ts`; the versions on `main` are reported too, as are 69 other files, and CI does not run Prettier | **won't fix** — no formatting churn in a design slice |
| R5 | Security: input, injection, data | **ok** — styles, one wrapper element and tests; no request, no text |
| R6 | Performance | **ok** — one shadow on one sticky, composited element; no script change |
| R7 | Accessibility | **ok** — DOM order = visual order on both layouts; no `order`/`row-reverse`; the 12 px terms in `on-surface-variant` pass axe in both appearances; the focus rule of the bar is untouched (its E2E test passes); reflow at 320 px passes for S11, a saved list and S16 |

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test` | 822 passed (no new unit tests, see 2.2) |
| `npm run lint` (client), `npm run typecheck && npm run lint` (e2e) | clean |
| `npm run build:release` | built; the known critical-CSS warning „Could not parse 1 selector“ only, the same on `main` |
| E2E `configurator/s11-list.spec.ts`, `configurator/s10-steps-2-4.spec.ts`, `shell/reflow.spec.ts`, Chromium and Firefox | 100 / 100 passed |
| E2E `configurator/s11-list.spec.ts` on mobile Chrome too | 30 / 30 passed (with Chromium and Firefox) |
| E2E `configurator/s15-s16-share.spec.ts`, `journeys/j1-configurator.spec.ts`, Chromium and Firefox | 5 passed, 1 skipped (J1 part 2 on Firefox, owner decision S5-20b) |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | passed, 7 checked |
| `node scripts/check-i18n.mjs`, `check-icons.mjs`; `node --test scripts/*.test.mjs` | passed; 101 / 101 |
| `reuse lint` | compliant |
| Figma, before and after | screenshots of S10 (1440 px) and S11 (1440 and 360 px, light and dark) from the client of `main` and of this branch, compared with `2019:457`, `2088:2200`, `2020:102`, `2101:3160`, `2034:1193` and the filter row of `2020:70`: title row, bar width and corners, circle size and term style as in the frames; the kept differences are open points 2 and 3 |

**How E2E ran locally:** 2.1 GB of disk were free, too little for a rebuild of the images. The E2E stack ran from its images of the same day (API = `main`; this task changes no server code), and the client of this branch was served with `ng serve --port 4300` (`E2E_BASE_URL`), as in S5-31. WebKit and mobile Safari run in CI only.

## 5. Open points

1. **Title row on a saved list** (decision 2 (a)) — `/kriterienlisten/:id` still shows its H1 above „Auswahl ändern“ and „Teilen“ on every width. Handoff §2.2 names the title-row pattern for S11 and S34 only. Proposed follow-up issue: decide whether a saved list takes the title row (then as a shared style with S34), and add it to the Handoff.
2. **Figma action bar** (decision 3 (a)) — `2034:1193` places status and button 32 px from the viewport edges; the code keeps them in the content column. Proposed Figma update: the bar's content on the column grid. The same frame shows the status in `label/large` and 12 px vertical padding, the code `body-medium` (Handoff §1.3: counts are body/medium) and 16 px. Both are outside this row and unchanged; for the owner to decide, with the frame update.
3. **Figma stepper** — the S10 frames show the number in a finished step; Handoff §5.1 and the code show the `check` icon with `common.stepper.done`. Proposed Figma update.
4. **Sticky bar in forced-colours mode (R2)** — proposed follow-up issue: give the sticky bar a visible top edge that survives forced colours (e.g. a `border-block-start` in `CanvasText` under `@media (forced-colors: active)`), so the content scrolling under it stays apart from it.
5. The task fitted one session.

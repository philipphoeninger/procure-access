# S4-18a — S11 summary, notices, action bar, suggestion link and page titles

> **Sprint:** 04, P0 task S4-18a (M2.6, M2.6d, M2.8; 1.5 points, split from S4-18 by O-S4-52). **Issues:** none. **Requirements / invariants:** R-LST-09 (notices, not dismissible), R-LST-10 (action bar, sticky from 600 px width and height, `scroll-padding-bottom`, WCAG 2.4.11), R-LST-13 („Kriterium vorschlagen“ at the end of the list, R-SUG-01); A11Y-09 stays green (S11 with its new parts in `reflow.spec.ts`). No invariant changes status, so no new TESTING version.
> **Status:** done — pull request #275 open.
> **Scope:** the part of S4-18 without the event: the summary of the selection and the counts above the list (moved here from S4-17 by O-S4-47), the priority and draft notes, the action bar with the live counter and „Liste exportieren“ (placeholder until S4-21), „Kriterium vorschlagen“ after the list, `pageTitle` as an `AREAS` root (M2.6d). Not here: the `CriteriaListShown` caller (S4-18b, O-S4-48 (a)), the export dialog S13 (S4-21), „Liste speichern“ (step 3).
> **Owner decisions (2026-11-11, all (a), before the plan):** **O-S4-48** — `CriteriaListShown` through `POST /api/v1/configurator/criteria-list-shown` in the Configurator module, called once per new selection (built in S4-18b); **O-S4-49** — „Liste speichern“ and `criteria.list.unsaved` wait for step 3; **O-S4-50** — `criteria.list.counts` as a frame with one plural key per level (UX copy v22 U106, wording approved as proposed); **O-S4-51** — the counts cover the requirement criteria by the level that applies now, without „Geprüft nach …“, inclusion only in the live counter; **O-S4-52** — split into S4-18a (this task) and S4-18b; **O-S4-53** — the draft note at the top below the priority note (Handoff and Figma over the UX copy's „oberhalb der Aktionen“; UX copy v22 U107). **Defaults accepted with the plan:** the draft note follows the legacy sign-in; the export placeholder has only the title and „Schließen“; the end-of-list link has the mail subject only (no criterion); S11 gets no „Fehler:“ title prefix (no form to submit there).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S11 showed H1, „Auswahl ändern“, the filter row, the cards and the empty states; the texts of `criteria.list` were already in `de.json` (area since S4-14) | Page work and tests only, no new text in that area |
| F2 | SPEC §9.1 names no trigger for `CriteriaListShown`; the Catalog module may not depend on Platform's writer through the criteria endpoint, and the browser caches the criteria answer of a pinned release | O-S4-48 (a): an endpoint of its own in S4-18b; split O-S4-52 |
| F3 | „Liste speichern“ leads an anonymous user to S01 with `auth.login.reasonSave` (UX copy), but S01 and saving come with step 3 | O-S4-49 (a) |
| F4 | `criteria.list.counts` held three counts in one key, so „1 Mindestanforderungen“ (SD25 needs one count per plural key) | O-S4-50 (a), U106 |
| F5 | The UX copy put the draft note „oberhalb der Aktionen“; Handoff §5.1 and Figma `2020:70` show both notices at the top | O-S4-53 (a), U107 |
| F6 | `criteria.export.title` was not in `de.json` (no area yet) | `criteria.export` is an `AREAS` entry now; S4-21 needs the rest of it anyway |
| F7 | The S11 route already had its title `pageTitle.criteria.list`; S10 already uses `shell.pageTitle.error`; `pageTitle` was typed into `de.json` by hand | `pageTitle` becomes an `AREAS` root (M2.6d); S11 needs no error prefix |
| F8 | The page scrolls, not `mat-sidenav-content` (`app.scss`) | `scroll-padding-bottom` goes on `<html>` |
| F9 | Firefox ignores `scroll-padding` when the focus moves: it scrolls only an element outside the viewport, so a control just above the bottom edge stayed partly behind the sticky bar (found by the new E2E test: 12 px of a 40 px checkbox) | A `focusin` handler scrolls such an element above the bar |

## 2. Changes

| File | Change | Requirement / decision |
|---|---|---|
| `features/configurator/components/criteria-list-summary/` (new: `.ts`, `.html`, `.scss`, `.spec.ts`) | `pa-criteria-list-summary`: `criteria.list.summary.count` (plural), `<dl>` of use case („Keiner davon passt“ = `criteria.select.step1.none`), app types, product parts and — only with a selection — test types; names joined by `Intl.ListFormat` | UX copy §5.5, U28; O-S4-51 |
| `features/configurator/state/selection-names.ts` (new, + spec) | `selectionNames(selection, options)`: names by layer in option order; an unlisted id is shown as it is; null use case = „Keiner davon passt“ | Summary |
| `features/configurator/state/criteria-list-rules.ts` (+ spec) | `levelCounts` (requirement criteria by `effectiveLevel`, included or not) and `includedCount` (every card) | O-S4-51 |
| `features/configurator/dialogs/export-placeholder/export-placeholder-dialog.ts` (new) | `mat-dialog` with `criteria.export.title` and „Schließen“ | R-LST-10, until S4-21 |
| `features/configurator/pages/criteria-list/criteria-list.{ts,html,scss,spec.ts}` | Summary, priority note, draft note (legacy sign-in through `store.user()` + `isAuthenticated()`), counts before the filter row, suggestion link after the list, action bar (only with a card) with the counter and „Liste exportieren“; live counter announced through `A11yAnnouncerService` (≤ 1/s) only after an include/exclude by the user; sticky from `STICKY_ACTION_BAR_QUERY`; while sticky its height is `scroll-padding-bottom` on `<html>` (`ResizeObserver`), and a `focusin` handler scrolls an overlapped focused element above the bar, keeping its top in view; tests with a sign-in flag in the test store | R-LST-09, R-LST-10, R-LST-13; O-S4-49 – O-S4-51, O-S4-53; F8, F9 |
| `core/contact/criteria-suggestion.ts`, `layout/footer/footer.{ts,html}` | `injectSuggestionLink()` (channel + href with the mail subject), used by the footer and S11 | R-SUG-01, R-LST-13 |
| `scripts/check-i18n.mjs` | `AREAS` + `criteria.export`, `pageTitle` | M2.6d, F6 |
| `public/i18n/de.json` | Through `check-i18n.mjs --write`: U106 keys, `criteria.export.*`, every `pageTitle.*` of §5.16 | U106 |
| `e2e/configurator/s11-list.spec.ts` (new) | Order of summary, notices, counts, link and action bar with axe in both appearances (axe first, S4-16 P5); sticky at 1280 × 800, in the flow at 1280 × 560 and 560 × 800; Tab from the H1 to „Liste exportieren“ never ends on an element the bar overlaps; the placeholder opens with Enter and Escape returns the focus | R-LST-09, R-LST-10, R-LST-13; A11Y-01, A11Y-08 |
| `e2e/shell/reflow.spec.ts` | S11 with summary, notices, action bar and an open detail panel at 320 px | A11Y-09 |
| `docs/ux_copy_procureaccess_v22.md` (new; v20 deleted) | U106, U107 | O-S4-50, O-S4-53 |
| `docs/design_handoff_procureaccess_v1.md` (1.22) | §5.1 S11: order above the list, action bar as built, focus fallback | O-S4-49 – O-S4-53 |
| `docs/figma_code_mapping_procureaccess_v1.md` (1.22) | The new parts belong to the S11 frames (no §2 row); Figma still shows „Liste speichern“ and the link as a button | — |
| `docs/sprints/sprint_04/SPRINT_04.md`, `TASK_PROMPTS.md` | Split into S4-18a/S4-18b; O-S4-48 – O-S4-53; dependencies of S4-21, S4-22 | O-S4-52 |
| `CLAUDE.md` | §3 (S4-18a paragraph, `pageTitle` area, suggestion helper, header line), §4 (new folders) | — |

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (`/engineering:code-review` over the diff against `main`: security, correctness, performance, edge cases, a11y). Sorted per CLAUDE §6.8:

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | The focus fallback scrolled by the whole overlap; a focused element taller than the space above the bar (a long note) would lose its top | **Fix** | Scroll at most to the element's top (`Math.min`); E2E rerun |
| R2 | Until step 3 the approved draft note says „melden Sie sich an, um sie zu speichern“, though signing in saves nothing yet | **Follow-up** | Wording is the owner's (P2) |
| R3 | The export placeholder is not full screen below 600 px (Handoff §2.1 dialogs) | **Won't fix** | Replaced by S13 in S4-21, which brings the dialog layout; the placeholder fits at 320 px |

Checked without finding: no `innerHTML`, no new dependency; the link target is the validated configured channel; texts by key, `de.json` only through `--write`; overlays (K5 menu, weight select, dialog) live outside the page host, so the `focusin` handler never moves them; the counter's visible text is not a live region (announcements only through the announcer); the scroll padding and listeners are removed when S11 is left; tokens only, DOM order = visual order.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 464 of 464 pass (450 before); new: summary (2), `selectionNames` (3), `levelCounts`/`includedCount` (3), S11 page (6); three S11 tests find the no-criteria notice by its text, as the priority note is a note too |
| C2 | `npm run lint` (client) | All files pass (a first run found three needless non-null assertions in the new tests; fixed) |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the known one, checked again on `main` |
| C4 | `node scripts/check-invariants.mjs` (TESTING_v47), `check-figma-links.mjs` (5 checked), `check-i18n.mjs` (315 keys, UX copy v22), `node --test scripts/check-i18n.test.mjs` (13 pass), `check-icons.mjs` (36 icons, no new icon) | Exit 0 each |
| C5 | `reuse lint` | Compliant, 1172 / 1172 files |
| C6 | `e2e/`: `npm run typecheck`, `npm run lint` | Pass |
| C7 | E2E on the local E2E stack (dev stack stopped and started again): the whole suite in Chromium, Firefox and mobile Chrome | 204 of 204 pass; after R1 `s11-list`, `reflow` and `journeys` again in Chromium and Firefox: 34 of 34; `s11-list` in Firefox repeated 4 times: 16 of 16. WebKit and mobile Safari only in CI |
| C8 | .NET build and tests | Not run: no backend file changed |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | `CriteriaListShown` has no caller yet (R-MET-01, R-CON-10) | S4-18b (O-S4-48 (a)) |
| P2 | Until step 3 the draft note invites anonymous users to sign in „um sie zu speichern“, but saving does not exist yet (R2) | Owner: keep the approved wording until step 3, or a temporary text (UX copy) |
| P3 | Figma S11 (`2020:70`, `2039:2546`) still shows „Liste speichern“, „Noch nicht gespeichert“ and the suggestion link as a button | Design owner, with the next Figma update (as S4-17 P1) |
| P4 | The first E2E run saw one Firefox page crash on S11 („Page crashed“, the class of S4-16 P5) in a test that ran name-based locators after axe; not seen again in the full suite or in 4 repeats | Watch in CI; S4-22 (J1 to the export) |
| P5 | Firefox ignores `scroll-padding` for focus moves (F9); the `focusin` fallback covers S11 | Reuse for any later sticky bar (S16 has none) |
| P6 | The reflow check opens only the first card's detail panel, so the tab labels of a card with two procedures at 320 px (S4-15b P3) are still not checked | S4-22 or a test selection whose first card has two procedures |

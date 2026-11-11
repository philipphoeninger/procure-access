# S4-16 — Priority override, include/exclude and notes

> **Sprint:** 04, P0 task S4-16 (M2.6, 3 points). **Issues:** none. **Requirements / invariants:** R-LST-03 (override through the K5 menu, recommendation visible, reset, weight of B), R-LST-04 (include/exclude, excluded cards stay in the list), R-LST-05 (note with the length error of SPEC §7.1, §7.2); no invariant changes status (TESTING stays at v46; CAT-08 stays `planned`, P3).
> **Status:** done — pull request #273 open.
> **Scope:** the row of S4-16: the K5 level menu with „Empfehlung wiederherstellen“, the weight of B, the include checkbox with excluded cards kept in the list, the note with its length error — everything in the in-memory draft of `withConfigurator()` (store methods of S4-14). Not here: the live counter `criteria.list.status.count` (action bar, S4-18) and the filter „Nur aufgenommene Kriterien anzeigen“ (S4-17), both named in R-LST-04's acceptance; E2E of S11 (S4-18 reflow, S4-22 J1).
> **Owner decisions (2026-11-11, all (a), before the plan):** **O-S4-40** — a `mat-select` „Gewichtung“ next to K5 while B applies (U102); **O-S4-41** — a change of only the weight of a recommended B shows `criteria.priority.overridden.weight` (U103) and the reset; **O-S4-42** — „Notiz hinzufügen/bearbeiten“ (U104) as a disclosure with the focus into the field; **O-S4-43** — the length error shows once the field was left, no `maxlength`, the export dialog checks again. **Defaults accepted with the question and the plan:** the K5 menu per Handoff §4.5 (not the select the UX copy row `criteria.priority.change` names), `criteria.priority.changeLabel` names the menu; an excluded card keeps every control; „Empfehlung wiederherstellen“ focuses the level button and announces the recommended level; the counter and the „Nur aufgenommene“ filter stay in S4-18/S4-17; no invariant flip; no visible „In Liste aufnehmen“ next to the checkbox (Figma, Handoff title row); in S11 the weight shows in the field only; the detail panel of an excluded card on `surface-container`; axe through the `axe-core` helper (O-S4-11), not `vitest-axe` as the row says.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The store methods `setCriterionIncluded`, `overrideCriterionPriority`, `setCriterionWeight`, `resetCriterionPriority`, `setCriterionNote` and `noteLengthError` exist since S4-14 | UI, texts and documents only; no backend, no store change |
| F2 | R-LST-03 asks for an editable weight of B; Handoff §4.5 has only the level menu, the UX copy no label (S4-15a P4) | O-S4-40, U102 |
| F3 | K5 showed an override only when the level differed, so a changed weight of a recommended B was invisible and could not be reset | O-S4-41, U103 |
| F4 | „Notiz hinzufügen“ is named by Handoff §5.5 and in the note of `criteria.priority.why`, but no key had the text (S4-15a P4) | O-S4-42, U104 |
| F5 | §6.4 and UX copy §6.1 check on submit; S11 has no submit for the note | O-S4-43 |
| F6 | UX copy `criteria.priority.change` „Priorität ändern — öffnet Select“ and `changeLabel` „Label Select“ vs. Handoff §4.5 `mat-menu` with `menuitemradio` | Handoff wins on visual detail (CLAUDE §2): menu; `changeLabel` names it; `criteria.priority.change` unused (P4) |
| F7 | Figma K5 (2011:203): the level button with `arrow_drop_down` and the weight inside it; Figma S11 (2020:70): checkbox before the H2 without visible text, „Nicht in Ihrer Liste“ top right of an excluded card on `surface-container-low`, „Empfehlung wiederherstellen“ below the override line. No frame for the menu, the weight control or an open note | Built as listed; menu and note per Handoff text |
| F8 | Handoff §4.5 names the K5 output `(reset)`; Angular's lint rule `no-output-native` refuses it (a DOM event) | Output `restoreRecommendation`; Handoff 1.20 says so |

## 2. Changes

| Where | What | Why |
|---|---|---|
| `shared/components/priority-indicator/priority-indicator.*` | K5: level button (`matMenuTriggerFor`, `arrow_drop_down`, name `criteria.priority.button` + visible text, `aria-describedby` = criterion title); `mat-menu` `criteria.priority.changeLabel` with three `menuitemradio` (`aria-checked`, `check` mark); a choice sets the `level` model and announces `criteria.priority.changed` (the same level again: nothing); with `weightRange` an outlined `mat-select` `criteria.priority.weightLabel` while B applies (`weight` model), else the weight in the button; override line `overridden` / `overridden.weight`; „Empfehlung wiederherstellen“ emits `restoreRecommendation`, focuses the level button and announces the recommended level. New inputs `recommendedWeight`, `criterionTitleId`, `weightRange` | R-LST-03, O-S4-40, O-S4-41, Handoff §4.5 |
| `shared/components/priority-indicator/priority-indicator.spec.ts` | Rewritten around a host that keeps level and weight like the store: name and popup of the button, menu items and `aria-checked`, choice with focus return and announcement, same level, reset with focus and announcement, the weight field (range, value, choice, empty after an override to B), both override lines, no field for A, weight in the button without range; axe in each state | Row |
| `features/configurator/components/criterion-card/*` | Title row: `mat-checkbox` (`aria-labelledby` = hidden `criteria.item.include` + title) and, when excluded, `criteria.item.excluded`; host class `--excluded` (`surface-container-low`, panel `surface-container`); K5 with title id, recommended weight and `PRIORITY_WEIGHT_RANGE`; „Notiz hinzufügen/bearbeiten“ disclosure (`aria-expanded`, `aria-controls`, `aria-describedby`) for an outlined `textarea` `criteria.item.note` with `cdkTextareaAutosize`, focus into it, open from the start when a note exists; the note as a `FormControl` whose validator wraps `noteLengthError`, so `errors.field.maxLength` shows after the field was left and follows the text until it fits; outputs `includedChange`, `levelChange`, `weightChange`, `priorityReset`, `noteChange` | R-LST-03 – R-LST-05, O-S4-42, O-S4-43, Handoff §5.5 |
| `features/configurator/components/criterion-card/criterion-card.spec.ts` | 9 new tests: checkbox name and change, excluded card with every control and axe, „Geprüft nach …“ checkbox, level and weight and reset reach the outputs, note disclosure with focus and axe, note as typed, existing note open behind „Notiz bearbeiten“, length error only after leaving, kept text, no `maxlength`, cleared once it fits | Row |
| `features/configurator/pages/criteria-list/*` | The page hands each card output to the store method of S4-14; two tests with the real `withConfigurator()` slice: exclude + override + note land in the draft, the excluded card stays in the list, reset; weight of a recommended B | R-LST-03 – R-LST-05 |
| `src/assets/fonts/material-symbols-icons.json`, `material-symbols-outlined.woff2`, `.manifest.json` | `arrow_drop_down` added, subset regenerated (36 icons, 24 208 bytes) | Icon check V117 |
| `public/i18n/de.json` | `criteria.priority.weightLabel`, `criteria.priority.overridden.weight`, `criteria.item.note.add`, `criteria.item.note.edit` through `check-i18n.mjs --write` | U102 – U104 |
| `docs/ux_copy_procureaccess_v20.md` (new; v18 deleted) | U102 `criteria.priority.weightLabel` „Gewichtung“, U103 `criteria.priority.overridden.weight` „Von Ihnen geändert. Empfohlen war: {{recommended}}, Gewichtung {{weight}}.“, U104 `criteria.item.note.add` „Notiz hinzufügen“ / `.edit` „Notiz bearbeiten“ — proposals, confirmed unchanged by the owner in this pull request (2026-11-11) | O-S4-40 – O-S4-42 |
| `docs/design_handoff_procureaccess_v1.md` 1.20 | §4.5: weight field, weight-only override line, inputs `recommendedWeight`/`weightRange`, output `restoreRecommendation`, menu details; §5.5: title row and excluded card, note disclosure and length error | Handoff upkeep |
| `docs/figma_code_mapping_procureaccess_v1.md` 1.20 | K5 row and §3 property notes; related line names the current versions | Mapping upkeep |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-40 – O-S4-43 | — |
| `CLAUDE.md` | Header; §3 the K5 menu, include/exclude and note (S4-16); §4 the folders' new parts | — |

ARCHITECTURE and SPEC are unchanged: no structure, contract or open question changes (the state of C119 is used as built).

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (`/engineering:code-review` over the diff against `main`: security, correctness, performance, edge cases, a11y). Sorted per CLAUDE §6.8:

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | The weight field truncated its label („Gewichtu…“) at the fixed width of 9 rem (seen on the E2E probe screenshot, C8); CLAUDE §9: no truncation | **Fix** | Fixed width removed; the field keeps Material's default width |
| R2 | A note that is too long stays too long when its field is closed; „Notiz bearbeiten“ does not say so | **Follow-up** (S4-21) | O-S4-43 (a): the export dialog checks again; S4-21 must refuse the export and lead to the note (P2) |
| R3 | `criteria.priority.change` („öffnet Select“) is unused, and the hint of `changeLabel` still says „Label Select“ | **Won't fix** here | Owner document; wording only, the menu follows the Handoff (F6); listed for the next UX copy round (P4) |

Checked without finding: no `innerHTML`, no new dependency; every text by key, `de.json` only through `--write`; the K5 models stay in step with the store (a choice goes through the store and comes back as input; the store accepts every level and every weight in range); a level other than B drops the weight, the recommended level again removes the override, a weight equal to the recommended one removes the weight override (S4-14 rules); `aria-checked` as strings, `aria-haspopup="menu"` from the trigger, focus returns to the button after the menu and after the reset (the reset button goes away after the focus moved); each keystroke in a note changes one decision, other cards keep their `decision` object and are not re-rendered (`track criterion.key`); the length counts UTF-16 code units like the server; Material's form-field subscript is `aria-live="polite"`, so the error is read once it appears on leaving the field (no announcer call added).

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 423 of 423 pass (406 before); new or rewritten: K5 (14, was 6), card (26, 9 new), S11 page (8, 2 new) |
| C2 | `npm run lint` (client) | All files pass (a first run found `no-output-native` on `reset`, F8, and three typing findings; fixed) |
| C3 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“ in the critical CSS) is the known one from `main` |
| C4 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs` (5 checked), `check-i18n.mjs` (268 keys, UX copy v20), `check-icons.mjs` (36 icons) | Exit 0 each |
| C5 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1158 / 1158 files |
| C6 | `e2e/`: `npm run typecheck`, `npm run lint` | Pass (no change there) |
| C7 | E2E on the local E2E stack (dev stack stopped and started again), Chromium, Firefox, mobile Chrome | 192 of 192 pass, J1 with axe on S11 in both appearances included; WebKit is left to CI |
| C8 | Throwaway E2E probe (not committed) on `T.1`: exclude the first card, open the K5 menu (axe), choose B (weight field), open the note, 1 001 characters, leave the field (error), open the details (axe) | Chromium and mobile Chrome: no axe violation in either appearance. Firefox: the page process crashed at the first axe run after name-based locators (`getByRole(…, { name })`); the same steps with CSS locators pass axe in both appearances, twice (P5). The screenshot showed R1 |
| C9 | .NET build and tests | Not run: no backend file changed |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | U102 – U104 were proposals of Claude | Closed: the owner confirmed the wording unchanged in this pull request (2026-11-11) |
| P2 | A too-long note in a closed field shows no error until the export (R2, O-S4-43 (a)) | S4-21: the export dialog refuses while a note is too long and leads to the field (open it, focus, error) |
| P3 | CAT-08 („`ConfigurationCriterion` keeps `ResolvedLevel` unchanged when an override is set“) describes the stored entity of step 3; the client keeps `resolvedPriority` unchanged since S4-14 and K5 shows both | Stays `planned`; flipped with the saved lists of step 3 (and the export's „recommended vs. decided“ columns in S4-19/S4-20 cover the ad-hoc case) |
| P4 | UX copy `criteria.priority.change` is unused and the hint of `changeLabel` says „Label Select“, while the Handoff has a menu (F6, R3) | Next UX copy round: drop `change` or give it a use, reword the hint to „Name des Menüs“ |
| P5 | Firefox 159 (Playwright 1.65) crashes the page process when axe runs after name-based locators on S11 with the new controls — the class of TESTING_v46 V129, which so far named only `mat-slide-toggle` | S4-18 / S4-22, when E2E tests drive S11: find the triggering locator; until then use role-only or CSS locators before axe in Firefox, and recheck with the next Playwright update |
| P6 | The live counter `criteria.list.status.count` and the filter „Nur aufgenommene Kriterien anzeigen“ are acceptance criteria of R-LST-04 but tasks of S4-18 and S4-17 | As planned |

# S4-30 — S10 step 1: error icon and „Eingabe löschen“

> **Sprint:** 04, added 2026-11-12 from the Figma review of sprints 1–4. **Issues:** #280. **Requirements / invariants:** R-CON-02; CFG-03 unchanged; A11Y-01, A11Y-08 (E2E); no TESTING version.
> **Owner decision:** O-S4-61 (a), 2026-11-12: build the clear button; the wording is Claude's proposal.
> **Status:** done — pull request open.
> **Scope:** the trailing icons of the S10 use-case combobox; UX copy v23, Handoff 1.23.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Figma S10-E `2139:3332` shows the `error` icon at the end of the field after „Weiter“ without an answer; Handoff §1.8 lists `error` for „text field in error state (Material)“. The page showed the outline and `mat-error` only | Missing icon; the meaning is in the linked `mat-error`, so the icon is decorative (`aria-hidden`) |
| F2 | Figma step 1 `2019:407` and S10-M1 `2134:3174` show a clear button (circled ×) while the field holds text. Neither the Handoff nor the UX copy named it, and no key existed for its name | Needs a UX copy key first (CLAUDE §6.5); the owner asked Claude to propose the wording (O-S4-61) |
| F3 | `cancel` was not in the self-hosted icon subset (S3-25) | Icon list + regenerated subset |

## 2. Decisions taken in this task

| # | Question | Taken | Why |
|---|---|---|---|
| D1 | Wording of the name | `common.actions.clearInput` „Eingabe löschen“ (UX copy v23 U108) | Says what happens, fits every field; a common key because S40 „Verwendungszweck“ uses the same field pattern (Handoff §2.2) |
| D2 | What clearing does with the answer | Empties the **text** only; the answer stays until another option is chosen. Leaving the field shows it again (the existing rule of S4-10) | Clearing the answer would change step 1 and reset steps 2–4 (CFG-01) from one icon press. The comment on #280 had proposed clearing the answer too; this is safer and keeps one rule for text that chose nothing |
| D3 | After clearing | Focus back in the field, list open with every option, match count announced like typing (`criteria.select.step1.matches`) | Keyboard users continue where they were; screen-reader users hear the new count |
| D4 | Both icons at once | Possible (typed text while the error shows): error icon first, then the button | Figma shows each state alone; nothing forbids both, and hiding either would hide information or a control |

## 3. Changes

| File | Change |
|---|---|
| `features/configurator/pages/criteria-select/criteria-select.html` | `matSuffix`: `mat-icon` `error` (`aria-hidden`) while the field error shows; `matIconButton` with `cancel` named `common.actions.clearInput` while the field holds text |
| `…/criteria-select.ts` | `useCaseErrorIconShown` (same rule as the validator), `useCaseFieldHasText` (from the control's `valueChanges`), `clearUseCase()`; imports `MatIconModule` |
| `…/criteria-select.spec.ts` | Clear button absent/present, clearing (empty field, focus, full list, announcement), answer kept and no reset, error icon with the error; axe with the button |
| `e2e/configurator/s10-step1.spec.ts` | Keyboard test: choose, Tab to „Eingabe löschen“, Enter, full list, Escape shows the answer; axe in both appearances. The test „a use case chosen by keyboard …“ tabs over the new stop |
| `src/assets/fonts/material-symbols-icons.json`, `material-symbols-outlined.woff2`, `.manifest.json` | `cancel` added; subset regenerated (37 icons, 24.8 kB) |
| `public/i18n/de.json` | `common.actions.clearInput` through `check-i18n.mjs --write` |
| `docs/ux_copy_procureaccess_v23.md` | New version: U108 and the §5.0 row; v21 deleted (two newest kept) |
| `docs/design_handoff_procureaccess_v1.md` | 1.23: §1.8 lists `cancel`; §5.4 layer 1 describes both trailing icons |
| `docs/sprints/sprint_04/SPRINT_04.md` | S4-30 in backlog and dependencies; O-S4-61 |
| `CLAUDE.md` | §3 S10 step 1 names both icons |

## 4. Self-review

| Area | Result |
|---|---|
| Correctness | The icon follows the validator's rule, so it appears and goes with `mat-error`; clearing never changes the draft (test „keeps the answer … resets no lower step“) |
| Keyboard | The button is one more tab stop after the field while it holds text; J1's `tabTo` and the reflow test are unaffected, `s10-step1` expects the stop |
| Accessibility | Button named by `aria-label` from the UX copy, 48 px target (Material icon button); icons `aria-hidden`; axe clean in light and dark |
| Tokens | No new styles: Material colours the suffix icon with the error role in the error state |

## 5. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npm run lint`, `npx ng test` (client) | Pass, 480 tests |
| C2 | E2E stack: `s10-step1`, `s10-steps-2-4`, `s10-reset-submit`, J1, reflow on chromium, firefox, mobile-chrome | 78 of 78 pass |
| C3 | WebKit projects | Left to CI (no local WebKit) |
| C4 | `check-icons`, `check-i18n` (+ self-tests), `check-figma-links`, `check-invariants`, e2e typecheck and lint, `reuse lint` | Pass |

## 6. Open points

1. S40 „Verwendungszweck“ reuses `common.actions.clearInput` when it is built (step 6).
2. Figma draws the `error` icon filled, the code uses Outlined (Handoff §1.8); the same open point as the `pa-notice` icons (#283).

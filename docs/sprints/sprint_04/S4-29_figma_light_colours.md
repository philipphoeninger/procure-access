# S4-29 — Figma light colours pinned in the theme

> **Sprint:** 04, P0 task S4-29 (general debt, 1 point; added 2026-11-11). **Issues:** #240 (light part; the issue stays open for the dark part). **Requirements / invariants:** R-SET-03 (development part), SPEC §7.5; A11Y-01 and A11Y-08 stay `active` and green; no invariant flipped, no new TESTING version.
> **Status:** done — pull request #267 open.
> **Scope:** the light value of every `--mat-sys-*` role of Handoff §1.2 pinned in `theme.scss` with `mat.theme-overrides`; an E2E test that compares the resolved light values with the Handoff table; the sprint documents of the S4-29 amendment (renumbered decision, order after S10). Not in scope: the dark values (O-S5), roles §1.2 does not list, the high-contrast design (Handoff H2).
> **Owner decisions (2026-11-11):** **O-S4-22** (added as O-S4-7) — pin the light values of Handoff §1.2 now, the dark appearance stays generated until the Figma dark mode and its review (O-S5); with the planning defaults „as recommended“: (1) renumber to O-S4-22, because S4-03 already used O-S4-7; (2) S4-29 runs after S4-13 and before S4-14, because S10 was already merged; (3) the pinned values give way to the high-contrast override; (4) the P0 heading of SPRINT_04 says 50 points.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `theme.scss` emitted only the generated palettes through `mat.theme` with `theme-type: color-scheme`, so every `--mat-sys-*` colour is `light-dark(<light>, <dark>)`. S3-09 (check 5, P1) found the light neutral roles 1–2 steps per channel off the Figma values and `error-container` / `on-error-container` visibly different | 15 of the 21 roles of §1.2 had to change (table in §2) |
| F2 | `mat.theme-overrides` writes each value as given. A plain hex value would replace both halves of `light-dark()` and change the dark appearance | Each override is `light-dark(<Figma>, <generated dark>)`; the dark half comes from `mat.get-theme-color` on a dark `mat.define-theme` of the same palettes (public Material API, no copied tone table) |
| F3 | High contrast is the legacy setting class `.contrast-more`, which `with-settings.ts` puts on `<html>`, not `prefers-contrast: more` (the planning note said „when the system asks for more contrast“; the mechanism is the setting) | The overrides stand in the `html { }` rule; `.contrast-more` has the higher specificity and still replaces them, as decision (3) wants |
| F4 | The page area is painted by `mat-sidenav-content` with `--mat-sys-background`, a role §1.2 does not list; §1.2 says Surface is the page background | `background` keeps its generated `#FDF8FD` per the task; owner decision O-S4-23 proposed (§5 P1) |
| F5 | The legacy helper classes `.secondary`, `.tertiary`, `.error` (legacy account screens) call `mat.theme` again with one palette, which re-emits every colour role with generated values inside their subtree | Not changed (legacy, outside the row); §5 P2 |
| F6 | The release build already warns „Could not parse 1 selector … 50%“ in the critical CSS on `main` | Not caused by this change (same warning without it); not in scope |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/CLIENT.ProcureAccess/src/styles/theme.scss` | `$figma-light` (the 21 roles of §1.2), `$_generated-dark` (`mat.define-theme`, dark, same palettes), function `_light-only` → `mat.theme-overrides(...)` in `html { }` after `mat.theme` | O-S4-22, SPEC §7.5, Handoff §1.1 „pin it with `mat.theme-overrides`“ |
| `e2e/shell/appearance.spec.ts` | Test „uses the Figma values of Handoff §1.2 in the light appearance“: reads the §1.2 table of the highest `docs/design_handoff_procureaccess_v*.md` (rows `Schemes/…`, 21 expected), emulates the light appearance, resolves each role on a probe element (`background-color: var(--mat-sys-…)`, so `light-dark()` resolves as on the page) and compares it as `#rrggbb`; a value that is not opaque stays as the browser wrote it, so an undefined role cannot pass as `#000000` | The task's „test that reads the computed custom properties“; a changed Figma value in the Handoff fails the test until `$figma-light` follows |
| `CLAUDE.md` | §3 theme foundation: what is pinned and how; version line | CLAUDE §11 |
| `docs/sprints/sprint_04/SPRINT_04.md` | The owner's amendment (S4-29 row, goal, load) plus: P0 heading 50 points; O-S4-7 of S4-29 renumbered to O-S4-22 and moved after O-S4-21; O-S4-23 proposed; S4-29 after S4-13 and before S4-14 in the sequence and dependency tables | Decisions (1), (2), (4) |
| `docs/sprints/sprint_04/TASK_PROMPTS.md` | The owner's S4-29 prompt (v1.1) with O-S4-22, „Depends on“ and the working order after S4-13 | Decisions (1), (2) |
| `docs/ROADMAP_v4.md` | The owner's amendment (M1.5 row, sprint 04 load) — unchanged by Claude | Included as asked |

**Light values (Chromium, from the compiled `theme.scss`; contrast by the WCAG formula against the background each role is used on).** Dark values: all 49 colour roles in the compiled `html` rule have the same dark half before and after (script comparison of the `light-dark()` pairs) — the dark appearance is unchanged.

| Role | Generated | Figma (§1.2, now) | Measured against | Contrast generated → Figma | Rule (SPEC §7.5, Handoff §1.2) |
|---|---|---|---|---|---|
| `surface` | `#FDF8FD` | `#FEF7FF` | `on-surface` | 16.33 → 16.23 | text ≥ 4.5 |
| `surface-container-lowest` | `#FFFFFF` | `#FFFFFF` (=) | `on-surface` | 17.13 → 17.07 | text ≥ 4.5 |
| `surface-container-low` | `#F8F2F8` | `#F7F2FA` | `on-surface` | 15.54 → 15.48 | text ≥ 4.5 |
| `surface-container` | `#F2ECF2` | `#F3EDF7` | `on-surface` | 14.73 → 14.85 | text ≥ 4.5 |
| `surface-container-high` | `#ECE6EC` | `#ECE6F0` | `on-surface` | 13.95 → 13.94 | text ≥ 4.5 |
| `surface-container-highest` | `#E6E1E7` | `#E6E0E9` | `on-surface` | 13.29 → 13.17 | text ≥ 4.5 |
| `on-surface` | `#1C1B1F` | `#1D1B20` | `surface` | 16.33 → 16.23 | text ≥ 4.5 |
| `on-surface-variant` | `#48454E` | `#49454F` | `surface` / `surface-container-high` | 8.95 / 7.64 → 8.88 / 7.63 | text ≥ 4.5 |
| `primary` | `#6750A4` | `#6750A4` (=) | `surface` / `surface-container` (header) | 6.14 / 5.54 → 6.12 / 5.60 | text ≥ 4.5 (links) |
| `on-primary` | `#FFFFFF` | `#FFFFFF` (=) | `primary` | 6.44 → 6.44 | text ≥ 4.5 |
| `primary-container` | `#E9DDFF` | `#EADDFF` | `on-primary-container` | 7.24 → 7.25 | text ≥ 4.5 |
| `on-primary-container` | `#4F378A` | `#4F378A` (=) | `primary-container` | 7.24 → 7.25 | text ≥ 4.5 |
| `secondary` | `#625B71` | `#625B71` (=) | `surface` / `surface-container` | 6.15 / 5.55 → 6.13 / 5.61 | focus ring ≥ 3 |
| `secondary-container` | `#E8DEF9` | `#E8DEF8` | `on-secondary-container` | 7.27 → 7.18 | text ≥ 4.5 |
| `on-secondary-container` | `#4A4358` | `#4A4459` | `secondary-container` | 7.27 → 7.18 | text ≥ 4.5 |
| `outline` | `#79757F` | `#79747E` | `surface` | 4.29 → 4.33 | non-text ≥ 3 (field borders) |
| `outline-variant` | `#C9C4CF` | `#CAC4D0` | `surface` | 1.63 → 1.62 | decorative only |
| `error` | `#B4271F` | `#B3261E` | `surface` / `surface-container-lowest` | 6.16 / 6.46 → 6.21 / 6.54 | text ≥ 4.5 |
| `error-container` | `#FFDAD5` | `#F9DEDC` | `on-error-container` | 7.24 → 7.34 | text ≥ 4.5 |
| `on-error-container` | `#910809` | `#852221` | `error-container` | 7.24 → 7.34 | text ≥ 4.5 |
| `scrim` | `#000000` | `#000000` (=) | – | – | backdrop (Material applies the 32 %) |

No Figma value breaks a contrast rule of SPEC §7.5, and axe finds nothing in either appearance (C5), so no value had to be reported.

**Roles §1.2 does not list keep their generated light value:** `background` (see F4), `on-background`, `surface-dim`, `surface-bright`, `surface-variant`, `surface-tint`, `inverse-surface`, `inverse-on-surface`, `inverse-primary`, `on-secondary`, `on-error`, `shadow`, the `tertiary` family (`tertiary`, `on-tertiary`, `tertiary-container`, `on-tertiary-container`), the `*-fixed`, `*-fixed-dim`, `on-*-fixed`, `on-*-fixed-variant` roles of primary, secondary and tertiary, and the palette values `neutral10`, `neutral-variant20`.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff against `main`.

| Area | Result |
|---|---|
| Security | Styles and test code only; no endpoint, no secret |
| Correctness | The dark half of every override is read from Material's own dark theme of the same palettes, and the compiled dark values are identical before and after; the overrides come after `mat.theme` in the same rule, so they win; `.contrast-more` still wins over them |
| Edge cases | The test fails when the Handoff table loses or gains a row (21 expected), when a role is undefined (transparent is not read as black), and when the light appearance is not active (`theme-light` checked first) |
| Performance | 21 extra declarations in the global stylesheet; the test is one page load |
| a11y | All roles meet SPEC §7.5 (table in §2); full E2E suite with axe in both appearances green |
| Licence | No new dependency; `reuse lint` compliant (the protocol has no header, like the earlier ones, covered by `REUSE.toml`) |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | The probe turned every colour into `#rrggbb` and dropped the alpha channel, so an undefined role (`transparent`) would have passed the `scrim` row as `#000000` | **Fixed**: only an opaque `rgb(…)` is converted |
| R2 | The test expects exactly 21 rows; adding a row to §1.2 (e.g. O-S4-23 (a)) means changing the number | **Won't fix**: the number guards the parser; a new role in the Handoff also needs its line in `$figma-light`, so the same PR touches both |
| R3 | The legacy `.secondary` / `.tertiary` / `.error` classes still emit generated values in their subtree | **Follow-up** (§5 P2): legacy screens, replaced in their migration step |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | Compiled `theme.scss` (sass) before and after: `light-dark()` pairs of the `html` rule | 49 roles; 15 light values changed, 6 already equal; 0 dark values changed |
| C2 | `npm run build:release` (client) | Pass; the existing critical-CSS warning (F6) is the same on `main` |
| C3 | `npm run lint`, `npx ng test` (client) | Pass; 295 of 295 tests |
| C4 | `npm run typecheck && npm run lint` (`e2e/`) | Pass |
| C5 | E2E on the E2E stack (dev stack stopped and started again): the whole suite in Chromium, Firefox and mobile-chrome (A11Y-01, A11Y-08 in both appearances); after R1 `shell/appearance.spec.ts` again | 192 of 192; then 18 of 18. WebKit and mobile-safari left to CI |
| C6 | `node scripts/check-invariants.mjs` | Passed |
| C7 | `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Pass (0 errors) |
| C8 | `reuse lint` | Compliant |
| C9 | Backend build and tests | Not touched |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | **Owner decision O-S4-23:** the page background is `--mat-sys-background` (generated `#FDF8FD`), not Surface `#FEF7FF` as §1.2 says (F4). (a) pin `background` to the Surface value and add the row Schemes/Background to §1.2 (and raise the expected row count of the test to 22); (b) leave it generated until O-S5 | (a) — one line, an invisible difference, and §1.2 then covers every colour the page shows |
| P2 | Legacy `.secondary` / `.tertiary` / `.error` re-emit generated colours in their subtree (F5, R3) | Goes with the legacy account screens (migration step 3); no issue needed unless a rebuilt screen uses them |
| P3 | #240 stays open for the dark part: dark values generated, review with the Figma dark mode (O-S5) | Comment on #240 names the light part as done (S4-29) |
| P4 | TD-34 (`TECH_DEBT_v3.md`) still describes the light values as differing | `TECH_DEBT_v4.md` at the sprint end: TD-34 keeps only the dark part (score may drop) |
| P5 | Handoff §9 H1 („verify the generated palette against §1.2; pin deviations“) is done for the light values | Mark H1 done in the next Handoff revision (documentation session); §1.2 itself needed no change — no value was ambiguous |

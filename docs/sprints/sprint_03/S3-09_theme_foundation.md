# S3-09 — Theme foundation: light and dark appearance, focus indicators

> **Sprint:** 03, P0 task S3-09 (M1.5, 3 points). **Issues:** #184 (TD-19), #192 (owner decision D4). **Requirements / invariants:** R-SET-02, R-SET-03 (development part), SPEC §7.5, Handoff §1.1, §1.3, §1.6, §1.7; no invariant activated, no new TESTING version (A11Y-08 is S3-10); A11Y-01, A11Y-03 and A11Y-09 stay green. **Status:** done by Claude Code (2026-11-05); one pull request.
> **Scope:** exactly the sprint row: `color-scheme: light dark`, `mat.theme` in light and a generated dark variant, the class `theme-light` / `theme-dark` on `html` from `prefers-color-scheme` without browser storage, `prefers-reduced-motion`, strong focus indicators (#184), the `Poppins` TODO, the H1 type scale of S04/S80 (S2-18 P4); plus #192 (D4).
> **Owner decisions (2026-11-05, asked before the start):** **D1 (a)** regenerate the palettes from the Handoff colours (M3 baseline `#6750A4` …), compare the light values with Handoff §1.2, pin nothing. **D2 (a)** Roboto through `@fontsource/roboto` (OFL-1.1, new dependency approved), Poppins removed. **D3 (a)** the legacy dark-mode toggle stays and sets the class instead of the inline style; detail asked during the task: **only an explicit action overrides the system** — the toggle and a signed-in user's saved setting; a value the legacy store keeps in `localStorage` is no longer applied on load. **D4 (a)** #192 (remove `@angular/animations` and `provideAnimationsAsync()`) is part of this task.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `styles/theme.scss` already had `color-scheme: light dark` and `theme-type: color-scheme`, so Material emitted every `--mat-sys-*` colour as `light-dark()`. Missing: the class, strong focus indicators, Roboto; `font-family: 'Poppins'` with a TODO, commented-out typography blocks, a list of old brand colours | Rewrite `theme.scss` along Handoff §1.1 |
| F2 | `_theme-colors.scss` was generated from the old brand colours (`#1500ff` / `#db6fd8` / `#172c3f`); `--mat-sys-secondary`, the focus ring colour, was a magenta tone. Two palettes (`$secondary-palette`, `$error-palette`) had been added by hand for the legacy classes `.secondary` and `.error` | D1: regenerate; add the two palettes back, marked |
| F3 | The legacy settings store (`with-settings.ts`) set an inline `color-scheme` style on `<html>` from `uiCustomization.darkModeOn`; an inline style beats any class. For signed-out users it saves the **system** setting to `localStorage` on the first visit and on every system change, so after the first visit a stale value overrode the system on the next load (against R-SET-02) | D3: new `AppearanceService`, the store calls its override only for explicit choices |
| F4 | Material 22 reads `prefers-reduced-motion` itself, but only slows some animations (the indeterminate progress bar runs at half speed); Handoff §1.7 asks for no motion (`MATERIAL_ANIMATIONS` with `animationsDisabled: true`). No component of ours has its own animation or transition | Provide `MATERIAL_ANIMATIONS` from the media query |
| F5 | `@angular/animations` is only used by `provideAnimationsAsync()`; Material and CDK 22 do not depend on it (#192) | Remove both |
| F6 | `pa-home` and `pa-error-page` use `headline-medium` for the H1 on every width; Handoff §1.3: `headline-small` below 600 px, `headline-large` from 600 px (S2-18 P4). The legal pages use `headline-medium` as doc pages, which is right | Change S04 and S80 only |
| F7 | Material buttons draw the strong focus ring on a child `span.mat-focus-indicator`, not on the button (found in the E2E run) | The global outline rule of Handoff §1.6 must leave such elements out, or they get two rings |
| F8 | `@fontsource/roboto`'s single-subset files (`latin-400.css`) carry no `unicode-range`, so loading `latin` and `latin-ext` side by side would let the later face hide the earlier one | Use the per-weight files `400.css` and `500.css`, which declare every subset with its range |

## 2. Changes

| File | Change |
|---|---|
| `CLIENT/…/src/styles/_theme-colors.scss` | Regenerated with `ng generate @angular/material:theme-color --primary-color=#6750A4 --secondary-color=#625B71 --tertiary-color=#7D5260 --neutral-color=#605D62 --neutral-variant-color=#605D66 --error-color=#B3261E --include-high-contrast` (Handoff §1.1); SPDX header and the two hand-added palettes for the legacy classes put back, with a comment |
| `CLIENT/…/src/styles/theme.scss` | `mat.theme` with Roboto as plain and brand family and density 0; `mat.strong-focus-indicators((border-style: solid, border-width: 3px))` (colour `--mat-sys-secondary` by default); `html { font-family: var(--mat-sys-body-medium-font) }`; `html.theme-light` / `html.theme-dark` → `color-scheme: light` / `dark`. Old comment blocks, the brand colour list and the empty `body` rule removed. Legacy classes `.tertiary`, `.secondary`, `.error`, `.text-error`, `.contrast-more` kept |
| `CLIENT/…/src/styles/main.scss` | The 18 Poppins `@font-face` rules removed. New: `:where(a, button, [tabindex]):not(.mat-focus-indicator, :has(> .mat-focus-indicator)):focus-visible { outline: 3px solid var(--mat-sys-secondary); outline-offset: 2px }` (Handoff §1.6, F7) |
| `CLIENT/…/src/app/core/a11y/appearance.ts` (new) | `AppearanceService`: `appearance` = explicit choice, otherwise `prefers-color-scheme` (CDK `MediaMatcher`, `change` listener); an effect sets `theme-light` / `theme-dark` on `<html>`; `setExplicit(appearance \| null)`. No browser storage. `provideAppearance()` starts it and provides `MATERIAL_ANIMATIONS` with `animationsDisabled` = `prefers-reduced-motion: reduce` |
| `CLIENT/…/src/app/app.config.ts` | `provideAnimationsAsync()` → `provideAppearance()` (#192) |
| `CLIENT/…/package.json`, `package-lock.json` | `@angular/animations` removed (#192); `@fontsource/roboto` `5.3.0` added, pinned like `material-symbols` (D2) |
| `CLIENT/…/angular.json` | Styles `@fontsource/roboto/400.css` and `500.css` (F8); the asset entry `src/assets/` removed — the folder held only Poppins |
| `CLIENT/…/src/index.html` | Preload of `Poppins-Regular.ttf` removed |
| `CLIENT/…/src/assets/fonts/poppins/` (18 files, 3 MB), `LICENSES/OFL-1.1.txt`, `REUSE.toml` (Poppins block), `README.md` (Poppins line) | Removed: no font file is in the repository any more; the Roboto licence ships in the build's `3rdpartylicenses.txt`, as the Material Symbols one does (S2-03 D1). `reuse lint` would report the unused OFL text |
| `CLIENT/…/src/app/features/settings/state/with-settings.ts` | D3: the inline `color-scheme` style is gone. `setUICustomization` sets the explicit appearance only for a signed-in user; `toggleDarkMode` always sets it; a signed-out (re)load and a system change for a signed-out user clear it, and `darkModeOn` in the state then mirrors the system, so the toggle icon matches. The contrast class and the `localStorage` handling are unchanged |
| `CLIENT/…/src/app/pages/home/home.scss`, `pages/error/error-page.scss` | H1 `headline-small`, from 600 px (`37.5em`) `headline-large` (F6) |
| `CLIENT/…/src/app/testing/fake-media-matcher.ts` (new) | Test double for CDK `MediaMatcher`: queries match as set, `set()` fires `change` like a system change |
| `CLIENT/…/src/app/core/a11y/appearance.spec.ts` (new) | 6 tests: system dark → `theme-dark`; a system change switches the class without reload; an explicit choice wins until cleared; works with `localStorage` throwing; reduced motion on/off → `animationsDisabled` true/false |
| `CLIENT/…/src/app/features/settings/state/with-settings.spec.ts` | 3 tests (D3): after sign-out a stored `darkModeOn: true` is ignored and the system applies; the toggle of a signed-out user applies; a signed-in user's saved setting wins over the system. The first one fails without the change (checked by removing `setExplicit(null)`) |
| `e2e/shell/appearance.spec.ts` (new) | Follows `prefers-color-scheme` and its change without reload (class, computed `color-scheme`, page background) and sends no third-party request; the Material „Menü“ button reached by keyboard shows the 3 px solid ring and no second outline; text is set in the loaded Roboto; the loading bar has no running animation with `reducedMotion: 'reduce'` and has one without |
| `CLAUDE.md` | §3 theme foundation, §4 `core/a11y/` and `e2e/shell/`, version line |
| `docs/sprints/sprint_03/S3-09_theme_foundation.md` (new) | This protocol |

Not changed on purpose: `TECH_DEBT_v2.md` (v3 at the end of the sprint moves TD-19 and #192 to "Done", P8); Handoff §1.2/§1.3 (P1, P5); the shell's own identical `:focus-visible` rules on the skip link, header and footer links (R3).

### Licence check (CLAUDE §10, D2)

`@fontsource/roboto` 5.3.0: package licence `OFL-1.1` (`package.json`, `LICENSE`: "Copyright 2011 The Roboto Project Authors"). OSI-approved, no commercial-use restriction; the OFL is a font-level licence that allows bundling with software under any licence, the font is used unmodified — the same licence as the Poppins files shipped until now. The CSS files are part of the package. The build bundles the woff2/woff files into `media/` and lists the licence in `3rdpartylicenses.txt` (checked in the release build).

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8. Fixed during the task (before the review): the second ring on Material buttons (F7, found by the E2E test).

| # | Finding | Sorting |
|---|---|---|
| R1 | Only Roboto 400 and 500 ship (Handoff §1.3); legacy `<strong>` in `mat-error` texts (register, reset password, product proposal) is drawn as synthetic bold | Won't fix: Handoff weights; the forms are rebuilt in step 3 / M5 |
| R2 | The image now also carries the cyrillic, greek, vietnamese, math and symbols subsets of Roboto (≈ 0.4 MB); browsers download only the subsets a page needs (`unicode-range`), in practice latin | Won't fix: Handoff's "latin + latin-ext" is about what is loaded; writing our own `@font-face` rules against `node_modules` paths would be more to maintain (F8) |
| R3 | Skip link, header and footer links keep their own `:focus-visible` outline, now the same as the global rule | Won't fix: identical result, no behaviour change; removing them is cleanup outside the row |
| R4 | `nginx.conf` still has `location ^~ /assets/`, which now serves nothing (404, as for any missing file) | Won't fix: harmless; legacy screens may still request assets until they are rebuilt |
| R5 | Material reads `prefers-reduced-motion` once at start, so a change of the system setting applies after a reload (the appearance follows without reload) | Won't fix: Material's own behaviour; the explicit motion setting comes with S60 (R-SET-01). Recorded in the code comment |
| R6 | The legacy store still writes the system appearance to `localStorage` for signed-out users and shows the English snackbar „New color scheme saved as cookie.“ on a system change; with storage blocked its `reloadUICustomization` throws (the store's init then never loads the products) | Won't fix here: pre-existing legacy behaviour, D3 keeps the storage until S60; the appearance itself works with storage blocked (unit test). Open point P3 |
| R7 | Security, N+1, data: no API, query or logging change; one new runtime dependency (fonts, licence checked); no third-party request (E2E) | – |
| R8 | Accessibility: text pairs and the focus ring checked in both appearances (§4 check 6); the global ring also reaches legacy links and buttons that had none or `outline: none` | – |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npm run build:release` | ✅ Builds; the warning "Could not parse 1 selector … 50%" also appears on `main` (critical CSS, not from this change). `media/` holds Roboto and Material Symbols, no Poppins; `3rdpartylicenses.txt` names `@fontsource/roboto` |
| 2 | `npx ng test` (Vitest) | ✅ 32 files, 131 tests (9 new) |
| 3 | `npm run lint` | ✅ All files pass |
| 4 | Playwright, E2E stack, `chromium`, `firefox`, `mobile-chrome` (WebKit only in CI) | ✅ 135 passed, incl. J1 with axe on every state, the shell specs, reflow (A11Y-09) and the new `appearance.spec.ts` (5 tests per project) |
| 5 | Resolved light `--mat-sys-*` values against Handoff §1.2 (Chromium) | ⚠️ `primary`, `on-primary`, `on-primary-container`, `secondary`, `surface-container-lowest` match; the neutral roles differ by 1–2 steps per channel (e.g. `surface` `#FDF8FD` vs `#FEF7FF`, `on-surface` `#1C1B1F` vs `#1D1B20`); visible: `error-container` `#FFDAD5` vs `#F9DEDC`, `on-error-container` `#910809` vs `#852221`. Angular's generator uses a newer Material colour algorithm than the Figma kit. Not pinned (prompt: pin nothing) → P1 |
| 6 | Contrast of the generated values (WCAG formula) | ✅ Light / dark: `on-surface`/`surface` 16.3 / 14.4; `on-surface-variant`/`surface` 9.0 / 14.3; `primary`/`surface` 6.1 / 10.8; `error`/`surface` 6.2 / 10.9; `on-error-container`/`error-container` 7.2 / 7.2; `on-secondary-container`/`secondary-container` 7.3 / 7.3; focus ring `secondary` against `surface` 6.2 / 10.9 and `surface-container` 5.6 / 9.6 |
| 7 | #192 step 2, scripted by hand in Chromium on the E2E stack, light/dark × reduced motion on/off | ✅ All 4 combinations: compact menu (`mat-sidenav`) opens, Esc closes and returns the focus to „Menü“; `mat-select` on `/settings` opens with Enter, Esc closes and returns the focus; snackbar after „Speichern“; expansion panel on `/filters` toggles; the „Passwort vergessen“ dialog opens, Esc closes and returns the focus; progress bar (check 4); stepper (J1). Not reachable without an account: `mat-menu` (profile) and tooltips (no trigger on the anonymous screens) → P4 |
| 8 | Screenshots of `/` and `/filters` in dark (Chromium) | ✅ Surfaces, text and the focus ring on the app name render in the dark scheme |
| 9 | `node scripts/check-invariants.mjs` / `check-figma-links.mjs` / `check-i18n.mjs` | ✅ gate passed (no flip) / 2 checked, 0 pending / 0 errors |
| 10 | `node --test scripts/*.test.mjs` | ✅ 36 + 10 + 11 passed |
| 11 | `npx tsc --noEmit -p e2e` | ✅ |
| 12 | `reuse lint` (Docker image) | ✅ compliant, 685 / 685 files |

## 5. Open points

| # | Point | When / who |
|---|---|---|
| P1 | Light values differ from Handoff §1.2 (check 5); all pass the contrast rules (check 6). Handoff §1.1 says to pin differing text and state colours with `mat.theme-overrides`, the task prompt says pin nothing. Options: (a) pin the light values of §1.2 now; (b) update the table in §1.2 to the generated values; (c) leave both until the O-S5 review, which pins light and dark together. Recommendation: (c) | Owner (O-S5 or earlier) |
| P2 | `.text-error` (legacy delete-account dialog title) uses `--mat-sys-error-container` as text colour, which is a background role: low contrast in both appearances. Not on a journey state, so axe does not see it | Account screens of step 3, or S3-10 if it shows up |
| P3 | Legacy settings store: storage still written for signed-out users, English snackbar on a system change, init throws with storage blocked (R6). Replaced by S60 (R-SET-01) and `UserPreferences` | Step 3 (S60) |
| P4 | `mat-menu` (profile) and `matTooltip` were not checked by hand for #192 (signed-in screens / no trigger for anonymous users) | CI E2E has no signed-in journey yet; check with J5 or the first signed-in screen |
| P5 | Handoff §1.3 says "`@fontsource/roboto` 400 and 500, latin + latin-ext"; the code loads the per-weight files with all subsets (F8, R2). Handoff wording could say "per-weight CSS files" | Owner, next Handoff version |
| P6 | Dark values are generated (e.g. `on-surface-variant` `#E6E0EB` is nearly `on-surface`); review and pinning with the Figma dark mode | O-S5 (before release) |
| P7 | Close #184 and #192 with the merge (the PR says "Closes") | Owner, merge |
| P8 | `TECH_DEBT_v3.md` (end of sprint): TD-19 (#184) and #192 to "Done" (S3-09) | End of sprint 03 (process step 14) |
| P9 | `webkit` and `mobile-safari` run only in CI (no local WebKit) | CI of this PR |

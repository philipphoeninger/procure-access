# S3-25 — Material Symbols font subsetting

> **Sprint:** 03, stretch task S3-25 (TECH_DEBT_v2 §3.3, 2 points). **Issues:** #135 (S2-03 open point P1). **Requirements / invariants:** A11Y-01 stays green; CLAUDE §8 (no third-party requests); no invariant activated. New CI check in stage 1, so a new TESTING version: `TESTING_v38.md` V117 (`TESTING_v36.md` deleted, two-version rule). **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: ship a subset of the Material Symbols font with only the icons the client uses, keep it in step with the code, measure before and after. **Not in scope:** the legacy icon markup and its English accessible names (S2-03 P3, rebuilt with the screens); the bundle budget #42 (end of M2); `font-display` (kept as `block`, see F7).
> **Owner decisions** (2026-11-09):
> 1. **O-S3-7 — option 1 of #135** (build-time subset), with the choices proposed before the task: fontTools `pyftsubset`/API as a development tool that is never shipped; the subset **committed** in `src/assets/fonts/`, with the icon list and a CI check without dependencies; fall back to option 3 (measure only) if the subsetting would not stay small. It stayed small (F4).
> 2. **Tool OK:** fontTools 4.68.0 (MIT) and brotli 1.2.1 (MIT, for WOFF2), run in the official `python:3.13-slim` Docker image (licence check §1 F6).
> 3. **2a — Dependabot:** an update of `material-symbols` fails the icon check until the subset is regenerated; no ignore rule.
> 4. **Licence entry in the client image** (asked during the task, F8): the client `Dockerfile` appends the font's entry to the served `3rdpartylicenses.txt`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `angular.json` loaded `node_modules/material-symbols/outlined.css`; the build hashed the full variable font into `media/` (4,006,044 bytes, ~3,900 icons, axes FILL, GRAD, opsz, wght). `font-display: block` from the package CSS | #135 |
| F2 | The client uses **34 icon names** from four places: 18 literal names in templates (two picked in code by the settings toggles: `contrast`/`contrast_rtl_off`, `light_mode`/`dark_mode`), `pa-notice` computes `info`/`warning`/`error` from its `ICONS` map, the EasyMDE toolbar map in `main.scss` (12 glyphs), and Angular Material's stepper (legacy `/products`) draws `create`, `done` and `warning` itself. `account_circle` in `criteria-list.html` is inside an HTML comment | The list needs all four sources; a template scan alone misses two of them |
| F3 | Each icon is a ligature: the font replaces the letters of the name with the glyph. A plain subset that keeps the letters a–z keeps every ligature, i.e. every icon | The generator removes the ligature rules of the other icons first |
| F4 | Glyph names are the icon names, the ligatures sit in one extension lookup (type 7 → 4); two names are **aliases** whose ligature produces a glyph with another name (`create` → `edit`, `draft` → `note`) | Rules are matched by the name they spell, not by glyph name. The whole pruning is ~20 lines of fontTools API: option 1 stayed small |
| F5 | The subset kept `head.modified` = time of saving, so two runs gave different bytes | Font loaded with `recalcTimestamp=False`: the same input gives the same bytes (checked twice), so the manifest hash is stable |
| F6 | **Licence check** (CLAUDE §10, development tools): fontTools 4.68.0 MIT (PyPI, repository `fonttools/fonttools`), brotli 1.2.1 MIT (Google); both OSI-approved, no commercial-use restriction, not shipped. Installed only into the throwaway container (`pip --target /tmp/py`), pinned in `scripts/subset-material-symbols.sh`. Not the `woff` extra (it also pulls `zopfli`, not needed for WOFF2) | Approved (decision 2) |
| F7 | `font-display: block` hides the icon for up to 3 s, then shows the ligature text until the font arrives (before: Slow 4G, §4). With a 24 kB font the block period is not reached in the measured profiles | Kept; `swap` would show the icon names as text at once |
| F8 | The build lists only npm packages it bundles in `3rdpartylicenses.txt`: with the font from `src/assets`, `material-symbols` dropped out (diff of both builds). Apache-2.0 §4 asks for the licence and a notice that the file was changed | Asked; decision 4: the client `Dockerfile` appends the entry (package `LICENSE` + „cut down by ProcureAccess“) |
| F9 | Angular rebases a `url()` relative to the file it is written in: the package mixin's `url()` lives in `node_modules/material-symbols/_core.scss`, so `../assets/fonts/` failed („Could not resolve“) | Path relative to the package folder (`../../src/assets/fonts/`), with a comment; a wrong path stops the build, it cannot fail silently |
| F10 | `src/assets/` is not an `angular.json` asset folder (only `public/` is) | The list and the manifest are not shipped; the font reaches the build only through the stylesheet, hashed into `media/` and cached for a year (`nginx.conf`) as before |

## 2. Changes

| Area | Change |
|---|---|
| `CLIENT/…/src/assets/fonts/material-symbols-icons.json` | The icon list (hand-kept): `icons` (34, sorted), `computed` (`notice.ts`: `error`, `info`, `warning`), `material` (stepper: `create`, `done`, `warning`, each with its reason) |
| `CLIENT/…/src/assets/fonts/material-symbols-outlined.woff2`, `…manifest.json` | Generated, committed: the subset (24,108 bytes) and its manifest (package version, icons, SHA-256) |
| `scripts/subset-material-symbols.py` | Generator (fontTools): checks that `node_modules` holds the pinned version, matches the ligature rules by spelled name, removes the others, fails on a name that is no icon of the package, subsets (letters/digits/underscore of the names + the kept glyphs, all layout features, variable axes kept), writes WOFF2 and the manifest |
| `scripts/subset-material-symbols.sh` | Runs the generator in `python:3.13-slim` with pinned fontTools and brotli, as the current user, repository mounted |
| `scripts/check-icons.mjs` (+ `check-icons.test.mjs`, 12 cases) | Icon check (V117): list ↔ names in templates (also inline templates, HTML comments skipped, quoted names in `{{ }}`), EasyMDE map, `computed` (a `<mat-icon>` without a literal needs its component there; each name must be written in that file), `material`; manifest ↔ list, pinned version, font hash. Every message names the fix |
| `CLIENT/…/src/styles/material-symbols.scss`, `angular.json` | Replaces `node_modules/material-symbols/outlined.css`: the package's own mixin (`@font-face` with `font-display: block` and the `.material-symbols-outlined` class, unchanged) with the subset's path |
| `CLIENT/…/Dockerfile` | Build stage appends the font's licence entry to `3rdpartylicenses.txt` (decision 4) |
| `.github/workflows/ci.yml` | Stage 1: step „Icon check“ (self-test, then the check) |
| `REUSE.toml`, `LICENSES/Apache-2.0.txt` | Font: `2026 Google LLC`, `Apache-2.0`, marked as changed (the copyright record of the font says „Copyright 2026 Google LLC“). The two JSON files fall under the existing `**/*.json` block |
| `docs/TESTING_v38.md` | V117 (§8.2 stage 1, §3.4); `TESTING_v36.md` deleted |
| `CLAUDE.md` | §3 icon paragraph (it said „no font file in the repository“), §4 layout and scripts, §5 commands, version line |

Unchanged: `material-symbols` stays in `dependencies` (its SCSS mixin is used by the build, and Dependabot keeps watching it); `app.config.ts` (`setDefaultFontSetClass`); Handoff §1.8 („self-hosted, `material-symbols` package“ is still true).

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y), with `/engineering:code-review`. Sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | The `material` entry named `error` for the stepper's error state; Material's `_getDefaultTextForState` returns `warning` (and the state name for `done`) | **Fixed.** The list and the font did not change: `error` and `warning` are both used by `pa-notice` |
| R2 | A `material` entry is always counted as used, so it is not reported when the Material component goes (the stepper leaves with the legacy `/products` screen in sprint 04) | **Won't fix:** a stale entry costs a few hundred bytes and nothing visible; checking which Material components are used would need its own rules. Open point P2 |
| R3 | `pip install` in the generator container pins versions but not hashes | **Won't fix:** development tool on the developer's machine, its output (the font) is reviewed through the manifest hash and the visual check; same level as the `reuse` container |
| R4 | Names chosen by `fontIcon`, `svgIcon` or a CSS class are not seen by the check | **Won't fix:** the client uses none (stated in the script header); a new use fails visibly (blank icon) and the check can learn it then |
| R5 | A `<mat-icon>` with control flow (`@if …`) or a `>` inside an attribute would not be parsed as a name | No change: it fails loudly („is not an icon name“), never silently |
| R6 | Security: no input from outside the repository in the CI step, the Dockerfile `RUN` or the scripts; no third-party request added (J1 and the shell tests check this) | — |
| R7 | a11y: `mat-icon` stays `aria-hidden`; icons render identically (§4 row 6); A11Y-01 green | — |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npm run build:release` before / after | Font in `media/`: **4,006,044 → 24,108 bytes** (−99.4 %); whole `browser/` output 6,326,100 → 2,344,164 bytes. Initial bundle unchanged (main 1.75 MB / 359 kB transfer; styles 49.95 kB / 6.7 kB) — the font was never part of it. No new build warning |
| 2 | First visit, release build served with gzip, Chromium, empty cache, 360 × 740, `/(login:auth)` (its heading shows the `lock` icon without API); **Slow 4G** = Lighthouse mobile (1.6 Mbit/s, 750 kbit/s up, 150 ms RTT) | **Before:** icon in the page after 3.0 s, font loaded after **22.4–22.8 s** (4,006,216 bytes on the wire). Screenshots (CDP, which does not wait for fonts): blank at +0.3 s; at +8 s the clipped ligature text „loc“ instead of the icon; icon only after the font. **After:** icon in the page after 3.0 s, font loaded after **3.3 s** (24,278 bytes), no text phase |
| 3 | Same, **Fast 4G** (9 Mbit/s, 1.5 Mbit/s up, 60 ms RTT) | Before: font 3.6 s after the icon appeared (0.9 s → 4.5 s), i.e. past the 3 s block period. After: 0.01 s after (0.92 s → 0.93 s) |
| 4 | Generator twice | Same bytes (SHA-256), F5 |
| 5 | All 34 icons rendered from the release build (light and dark appearance, `theme-light`/`theme-dark`), compared with the same page on the full font | **Pixel-identical** screenshots in both appearances; the control `search` (not listed) falls back to letters, so the subset holds only the listed icons |
| 6 | E2E stack (client image rebuilt), `npx playwright test` on chromium, firefox, mobile-chrome | ✅ 141 passed (J1 with axe on every state in both appearances = A11Y-01/A11Y-08, shell, reflow, third-party request checks). WebKit and mobile Safari run in CI only |
| 7 | Served by nginx from the image | `media/material-symbols-outlined-ITFOKGAT.woff2`: 24,108 bytes, `font/woff2`, `public, max-age=31536000, immutable`; `/3rdpartylicenses.txt` holds the entry „material-symbols (Material Symbols Outlined, cut down by ProcureAccess …)“, Apache-2.0 |
| 8 | `npm run lint`; `npx ng test` | ✅ All files pass; 34 files, 149 tests |
| 9 | `node scripts/check-icons.mjs`; `node --test scripts/check-icons.test.mjs` | ✅ 0 errors; 12/12 |
| 10 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`; the other four self-tests | ✅ all green |
| 11 | `reuse lint` (fsfe/reuse 6.2.0); actionlint 1.7.12 | ✅ compliant (1,055 files; Apache-2.0 now a used licence); ✅ no findings |
| 12 | .NET build and tests | Not run: no backend file changed |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | Sprint 04 rebuilds the screens: every new icon goes into the list and the subset is regenerated (the check says so). The Handoff §1.8 icons (`arrow_drop_down`, `calendar_today`, `check`) are drawn by Material as SVG, not by the font | Each screen task |
| P2 | The stepper entries of `material` stay after the legacy `/products` screen is gone (R2); remove them with that screen | S10/S11 (sprint 04) |
| P3 | A Dependabot update of `material-symbols` is red in stage 1 until someone runs `scripts/subset-material-symbols.sh` on its branch and pushes the result (decision 2a) | Dependabot PRs |
| P4 | #42 bundle budget: the font no longer weighs anything there | End of M2 |
| P5 | TECH_DEBT: #135 moves to „resolved“ in the next version (end of the sprint) | TECH_DEBT_v3 |

Fitted one session.

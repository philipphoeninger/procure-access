# S2-03 — Self-hosted icon fonts, Font Awesome removed

> **Sprint:** 02, P0 task S2-03 (debt; TECH_DEBT_v1 #43 score 32, #78 score 28). **Issues:** #43, #78. **Requirements / invariants:** none activated; A11Y-01 and CAT-10 stay green; CLAUDE §8 (no third-party requests from the browser), ARCHITECTURE_v14 D12 (licence rule, C89). **Status:** done by Claude Code (2026-10-29), pull request open.
> **Scope:** serve the icon font from the client bundle, remove both Google Fonts links and every Font Awesome package and asset, prove it with J1. Not in scope: the legacy icon markup itself (P3), the font size (P1, #135), the header's English accessible names (#120, S2-15).
> **Owner decisions 2026-10-29:** **O-S2-5:** Material Symbols Outlined (Apache-2.0), self-hosted via the `material-symbols` npm package; classic Material Icons and Font Awesome removed; full font for now, subsetting as a follow-up issue (→ #135). **D1** (asked during the task, F6): the font is bundled by the build from `node_modules`; `LICENSES/` and `REUSE.toml` stay unchanged. (Rejected: copying the woff2 into `src/assets/` like Poppins, with `LICENSES/Apache-2.0.txt` and a `REUSE.toml` block: the package would then only be a manual source, every update would add a 4 MB binary to git, and Dependabot would no longer cover it.)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `src/index.html` linked two Google stylesheets: classic **Material Icons** (default font set of all 25 `mat-icon` uses) and a **Material Symbols Outlined** subset of five icons (`<span class="material-symbols-outlined">` in header, settings, profile) | Both links go; one icon set |
| F2 | `add_2`, `draft` (`mat-icon`) exist only in Material Symbols, not in the classic font, which showed them as text | Material Symbols as the default font set of `mat-icon` fixes them as a side effect |
| F3 | `@fortawesome/fontawesome-free` 7.2.0 (`CC-BY-4.0 AND OFL-1.1 AND MIT`) was in the global `styles` of `angular.json`. Its only user: the EasyMDE toolbar (`<i class="fa fa-bold">` …) of the legacy criterion proposal. No template uses `fa-*`; the repository holds no Font Awesome file | Remove package and style entry; give the toolbar other glyphs |
| F4 | EasyMDE downloads Font Awesome from a CDN unless `autoDownloadFontAwesome: false`, which the editor already sets. Without the Font Awesome CSS, that flag is now the only thing preventing a CDN request | Comment on the flag |
| F5 | **Licence check `material-symbols` 0.48.2** (CLAUDE §10): Apache-2.0, OSI-approved, permissive → meets D12 for a shipped dependency. Community repackaging of Google's font files (maintainer marella, published 2026-10-27, repository `marella/material-symbols`). No install scripts. Ships three variable fonts; only `outlined.css` is used, which references `material-symbols-outlined.woff2` (4,006,044 bytes). Every icon name in use and every toolbar glyph exists in its `index.d.ts` | New dependency approved with O-S2-5 |
| F6 | The task asked to update `LICENSES/` and `REUSE.toml` for the font files. Bundled from `node_modules`, they never enter the repository; `reuse lint` checks only repository files and reports a licence in `LICENSES/` that no file uses | Owner decision D1: no change to `LICENSES/` or `REUSE.toml` |
| F7 | `ng build` writes the licence texts of all bundled packages to `dist/procure-access/3rdpartylicenses.txt` (incl. `material-symbols`, Apache-2.0), but the client `Dockerfile` copied only `dist/…/browser`. So **no** dependency's licence notice (MIT, Apache-2.0 …) reached users; `/3rdpartylicenses.txt` answered with `index.html` | Found in the self-review (R2). D1 relies on the notice being shipped → one `COPY` line (deviation, §2) |
| F8 | **Angular's release build inlines Google Fonts CSS** into `index.html` at build time and adds `<link rel="preconnect" href="https://fonts.gstatic.com">`. A font file is fetched only when a glyph needs it. With a Google link put back for a negative check, J1's request log stayed empty although the page connected to Google | A request log alone cannot prove #43; J1 also checks `<link>`/`<script>` origins (R1) |
| F9 | Handoff §1.8 already specifies Material Symbols Outlined, weight 400, self-hosted from the `material-symbols` package, via `mat-icon`. No other document names an icon set (SPEC, TESTING, figma mapping) | No document change besides CLAUDE.md |

## 2. Changes

| File | Change |
|---|---|
| `CLIENT/…/package.json`, `package-lock.json` | `@fortawesome/fontawesome-free` removed; `material-symbols` `0.48.2` (exact, like `easymde`) added. Lockfile written with Node 24 / npm 11 (`.nvmrc`); a first attempt with the system npm 10 rewrote unrelated `libc` fields and was discarded |
| `CLIENT/…/angular.json` | Global style `node_modules/@fortawesome/fontawesome-free/css/all.min.css` → `node_modules/material-symbols/outlined.css`; the build copies the woff2 to `media/material-symbols-outlined-<hash>.woff2` (served `immutable` for a year by the existing `/media/` rule of `nginx.conf`) |
| `CLIENT/…/src/index.html` | Both `fonts.googleapis.com` links removed |
| `CLIENT/…/src/app/app.config.ts` | `provideAppInitializer` sets `MatIconRegistry.setDefaultFontSetClass('material-symbols-outlined')`: every `mat-icon` uses Material Symbols (F1, F2) |
| `CLIENT/…/src/styles/main.scss` | Map of the twelve default EasyMDE toolbar classes (`fa-bold` → `format_bold` …) to Material Symbols glyphs via `::before` ligatures, next to the existing EasyMDE rules; the buttons keep EasyMDE's `aria-label` (no UI text added). Size in `rem` |
| `CLIENT/…/markdown-editor/markdown-editor.ts` | Comment: `autoDownloadFontAwesome` must stay `false` (F4) |
| `CLIENT/…/Dockerfile` | **Deviation from the plan:** also copies `3rdpartylicenses.txt` into the nginx root, so the licence texts of the bundled packages are served at `/3rdpartylicenses.txt` (F7) |
| `e2e/support/third-party-requests.ts` (new) | `recordThirdPartyRequests(page, appOrigin)` collects every request to another origin (ignores `data:`/`blob:`); `thirdPartyLinksInDocument(page, appOrigin)` lists `<link href>`/`<script src>` of other origins (preconnect, dns-prefetch, unused font CSS, F8) |
| `e2e/support/third-party-requests-helper.spec.ts` (new) | Self-test on a routed fixture page (nothing leaves the machine): the recorder reports only the foreign image; the link check reports only the foreign `preconnect` |
| `e2e/journeys/j1-configurator.spec.ts` | Main J1 test records requests from the start, checks that the "Material Symbols Outlined" `FontFace` is `loaded` after `/filters` opens, and ends with the step "no request and no link went to a third-party origin" |
| `CLAUDE.md` | §3 current state (icons, licence file, J1 check); §4 `e2e/support/` content |

Deviations from the plan: the Dockerfile line (F7, R2) and the link check (F8, R1) were not in the plan; both came out of the self-review. `document.fonts.check()` from the first draft was replaced by the `FontFace` status, because `check()` also returns `true` when no matching font face exists.

Documents: TESTING unchanged (no invariant activated, so no new version). Handoff §1.8 already describes the result (F9). `TECH_DEBT_v1.md` stays; the sprint-end review writes v2 (#43, #78 closed by this PR; #135 new). No UI text, no SPEC §13 or figma mapping change.

## 3. Self-review (CLAUDE §6.7, `/engineering:code-review`)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Privacy: the J1 request log misses a `preconnect` and declared-but-unused fonts (F8); the check would not have caught the exact #43 regression | **Fix** (done): `thirdPartyLinksInDocument` in J1 + self-test. Negative check in §4 |
| R2 | Licence: the Apache-2.0 notice (and all other notices) never reached users (F7); D1 relies on it | **Fix** (done): one `COPY` line. Affects all bundled packages, so it also fixes an older gap |
| R3 | Performance: the full font is 4.0 MB; `font-display: block` from the package CSS hides icons for up to 3 s on the first visit, then shows ligature names as text until it arrives; cached for a year afterwards | **Follow-up issue** #135 (O-S2-5: full font for now) |
| R4 | A11y: the legacy `<span class="material-symbols-outlined">` icons are not `aria-hidden`. In the header and settings their buttons have their own name (`aria-label`, `<label for>`), but in the profile list „box“ is read as text. Unchanged by this PR (the Google font rendered the same text) | **Follow-up issue** #137 (P3) |
| R5 | The toolbar map covers only EasyMDE's default buttons; an added button would show no glyph | Won't fix: the comment says so; the editor belongs to the legacy proposal, which ARCHITECTURE §8.1 retires |
| R6 | EasyMDE's „guide“ button opens markdownguide.org in a new tab on click | Won't fix: user-initiated navigation, not a request of the page; legacy editor |
| R7 | Component tests do not use `appConfig`, so `mat-icon` keeps the `material-icons` class there | Won't fix: no test queries icons; they are `aria-hidden` and found by role/name only |
| R8 | `material-symbols` has one community maintainer | Accepted (O-S2-5): Apache-2.0 font files from Google; fallback is the same files from `google/material-design-icons` |
| R9 | Build warning „Could not parse 1 selector … 50%“ | – exists on `main` too (checked), not from this change |
| R10 | Security: no secret, no endpoint, no auth change; no new script or `innerHTML` (the glyphs are CSS `content`) | – |

No finding left is a correctness or security bug in the change; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npm run build:release` | ✅ font in `media/` (4,006,044 bytes); `3rdpartylicenses.txt` lists `material-symbols` with the Apache-2.0 text; no `fontawesome`/`googleapis`/`gstatic` in the output except EasyMDE's own disabled auto-download code (F4). Budgets unchanged (initial 1.74 MB) |
| 2 | `npx ng test` (Vitest) | ✅ 17/17 |
| 3 | `npm run lint` | ✅ |
| 4 | E2E on the E2E stack, Chromium + Firefox + mobile Chrome | ✅ 45/45 (smoke, J1 incl. A11Y-01/CAT-10 and the new checks, axe helper, new self-test) |
| 5 | E2E WebKit + mobile Safari | ⚠️ not run locally: the host lacks WebKit's system libraries (`libicu74`, `libflite1` …), all WebKit tests fail at browser launch, also untouched ones. CI (Ubuntu) runs them |
| 6 | Negative check: a Google Fonts link put back into `index.html`, stack rebuilt | Request log alone: J1 **passed** (F8). With the link check: J1 **fails** with `https://fonts.gstatic.com/`. File restored, stack rebuilt, 45/45 |
| 7 | Network log of `/filters` (light and dark, Chromium) | ✅ 14 requests, all `http://localhost:4200`; fonts: `Poppins-*.ttf`, `media/material-symbols-outlined-EDG2WWZ5.woff2` |
| 8 | Visual check, light and dark | ✅ header icons and all twelve EasyMDE toolbar glyphs (EasyMDE's markup injected into `/filters` with the real global styles; the proposal page needs a sample user whose password is in `.env`, not read) |
| 9 | `curl` against the client image | ✅ `/3rdpartylicenses.txt` `text/plain`; woff2 `font/woff2`, `Cache-Control: public, max-age=31536000, immutable` |
| 10 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 11 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 12 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 13 | Backend | – not touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Font size 4.0 MB (R3) | #135, owner decision on the options there |
| P2 | WebKit/mobile Safari only in CI (check 5) | CI of this PR |
| P3 | #137: legacy icons as `<span class="material-symbols-outlined">` (header ×2, settings, profile ×2) are not `aria-hidden` (Handoff §1.8: icons always `aria-hidden`); „box“ is read as text in the profile list. Fix: use `<mat-icon>` (aria-hidden by default). The header part goes away with the shell (S2-15) | #137 (created 2026-10-29 on the owner's OK); settings/profile with their rebuild |
| P4 | The API image: whether the licence notices of its NuGet packages reach users was not checked (outside this task) | Note for the next licence review |

# S2-15 — Shell frame: landmarks, skip link, anonymous header, footer

> **Sprint:** 02, P0 task S2-15 (3 points; roadmap M1 shell). **Issues:** #120 (TECH_DEBT_v1 TD-08); the header part of #137. **Requirements / invariants:** R-SHL-01, R-SHL-02 (state Anonymous and compact layout), R-SHL-06; A11Y-01 stays green and now covers the shell. No invariant changes status (A11Y-03/05 come with S2-16), so there is no new TESTING version. **Status:** done by Claude Code (2026-11-02), pull request open.
> **Scope:** page structure skip link → header → `main#main` → footer; `pa-header` in the Anonymous state with its compact layout below 600 px; footer; legal routes as placeholders; #120. Not in scope: SignedIn / SignedInNoOrg (step 3), page titles and H1 focus on route change (S2-16), start page and error pages (S2-17), the 320 px / 400 % check (S2-18), legal page content (GP.3), R-SUG-01 (step 2).
> **Owner decisions 2026-11-02 (before the task):** **O-S2-4:** `COMMON.ENTER` → the existing `common.actions.next` „Weiter“ (1a); the header's English names go with the legacy header, the new one uses the `shell.nav.*` texts of UX copy v14. Signed-in legacy users get „Konto“, „Einstellungen“ and „Abmelden“ instead of „Anmelden“ until step 3 (2a). The rename `TASK_PROMPT.md` → `TASK_PROMPTS.md` is committed separately first.
> **Owner decisions 2026-11-02 (after the plan):** **D1** „Beitragen“ links to the legacy product proposal `/product-proposal/0` until the contribution screens of step 6. **D2** the footer shows only links with a target: the three legal pages and „Quellcode auf GitHub“; „Über ProcureAccess“ (no page), „Projekt WERTE.IT“ (no URL) and „Kriterium vorschlagen“ (R-SUG-01, step 2; O-6) follow when their target exists. **D3** no „Einstellungen“ for anonymous visitors (Handoff §3.1 as written). **D4** legacy pages without an `h1` are accepted as an open point.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The legacy `pa-header` was a fixed `mat-toolbar` with icon buttons named in English („Search icon-button“ …, #120), icons not `aria-hidden` (#137), an unused search field, the WERTE.IT logo and an `<h1>` with the app name; `app.html` put it *inside* `<main id="main">`; no skip link, no footer; `app.scss` padded the content 64 px for the fixed header | Header rebuilt, shell restructured |
| F2 | Figma `2017:650` / `2039:2131`: app name + claim, nav Kriterien / Produkte / Beitragen, filled „Anmelden“; compact: app name + „Menü“. No logo | Logo dropped from the header (still used by login and register) |
| F3 | All `shell.*` and `legal.*` texts are in UX copy v14; `shell.*` was already in `de.json` (S2-13) | No missing wording; `legal` added to `AREAS` of the i18n check |
| F4 | R-SHL-06 lists six links; three have no target in any document (F-D2) | Asked; D2 |
| F5 | The legacy app name `<h1>` was the only `h1` on most legacy pages (own `h1` only on `/unauthorized` and the e-mail confirmation) | Asked; D4 — R-SHL-01 „one h1 per page“ is not met on legacy pages yet |
| F6 | `/settings` (language, contrast, dark mode) was linked for everyone; Handoff's Anonymous state has no settings | Asked; D3 |
| F7 | The legacy `products-container` stepper button used `COMMON.ENTER` = „Enter“ in `de.json` | O-S2-4 (1a) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/app/app.html`, `app.ts`, `app.scss` | Shell: skip link `shell.skipLink` (first focusable element; a click handler focuses `main` without putting `#main` into the URL, where the router would see it) → `pa-header` → `<main id="main" tabindex="-1">` (content column of Handoff §2.1, 16/24/32 px paddings, max 65 rem from 1072 px) → `pa-footer`, all in a `mat-sidenav-container`. Below 600 px a `mat-sidenav` (`mode="over"`, `fixedInViewport`, end) holds „Menü schließen“ and `pa-main-nav`; `menuOpen` is a `linkedSignal` that resets when the layout switches; the menu closes on Esc, backdrop, „Menü schließen“ and after a chosen item. The legacy login outlet still replaces the whole shell | R-SHL-01, R-SHL-02, Handoff §2.1, §2.2, §3.2 |
| `layout/header/` (`pa-header`) | Rebuilt: `<header>` with the app name (+ claim from 600 px) as link to `/home`, from 600 px `pa-main-nav layout="bar"`, below it „Menü“ (`aria-expanded`, `aria-controls="main-nav"`). `@figma` 2017-650. No inputs for data; `menuOpen` input and `menuToggle` output connect it to the shell's sidenav | R-SHL-02, Handoff §3.1; #120, header part of #137 |
| `layout/main-nav/` (`pa-main-nav`, new) | `nav` „Hauptnavigation“ with Kriterien → `/filters`, Produkte → `/products`, Beitragen → `/product-proposal/0` (D1), 48 px text buttons, active item with `aria-current="page"`, `secondary-container` background and 3 px `primary` bottom border; account area: „Anmelden“ (legacy login outlet) or, signed in, „Konto“ → `/profile`, „Einstellungen“ → `/settings`, „Abmelden“ (same legacy calls as before). Layout `drawer` puts `cdkFocusInitial` on the first item. The signed-in state is re-read on user changes and after every navigation (R6) | Handoff §3.1; O-S2-4 (2a) |
| `layout/shell/wide-layout.ts` (new) | `injectWideLayout()`: `BreakpointObserver` on `(min-width: 37.5em)` as a signal | Handoff §3.1, §2.2 |
| `layout/footer/` (`pa-footer`, new) | `<footer>` on `surface-container`, wrapping link list (48 px targets): Erklärung zur Barrierefreiheit, Datenschutz, Impressum, Quellcode auf GitHub (same tab, so no `common.newTab` suffix needed) | R-SHL-06, Handoff §5.3; D2 |
| `pages/legal/` (`pa-legal-page`, new) + `app.routes.ts` | `/impressum`, `/datenschutz`, `/barrierefreiheit`: H1 `legal.*.title` + `pa-notice` Info `legal.placeholder`; literal keys per page, so the i18n check resolves them | R-SHL-07 placeholder until GP.3 |
| `features/products/pages/products-container/products-container.html`, `public/i18n/de.json` | `COMMON.ENTER` → `common.actions.next`; unused `COMMON.ENTER` removed from `de.json` (`en/es/fr.json` untouched); `legal.*` written by `check-i18n.mjs --write` | O-S2-4 (1a), #120 |
| `scripts/check-i18n.mjs` | `AREAS` + `legal` | CLAUDE §9 (add a root with its screens) |
| `scripts/check-figma-links.mjs` | `PENDING` emptied | S1-21 P4 |
| `eslint.config.js` | M1 entry `layout/header/header.ts` removed from `LEGACY_EXCEPTIONS` | #158, S2-14 P1 |
| Tests: `app.spec.ts`, `layout/header/header.spec.ts`, `layout/main-nav/main-nav.spec.ts`, `layout/footer/footer.spec.ts`, `pages/legal/legal-page.spec.ts`, helper `testing/shell-test-doubles.ts` | 23 new component tests (landmark order, skip link first and focusing `main`, names and targets, `aria-current`, login / logout, signed-in interim, compact menu open/close/Esc/after a choice, `cdkFocusInitial` only in the drawer, no heading and no icon-only control in the header, legal H1 + placeholder) | Tests with the code |
| `e2e/shell/shell.spec.ts` (new) | Four tests tagged `INV:A11Y-01`, each with its own viewport so both header layouts run in every browser project: Tab → skip link visible and focused, Enter → `main` focused (URL unchanged), landmarks; `aria-current` on `/products`; compact menu: focus on „Kriterien“ when open, axe on the open menu, Esc and „Menü schließen“ return the focus to „Menü“; legal placeholders through the footer at 360 px | A11Y-01 on the shell (DoD of the row) |
| `e2e/journeys/j1-configurator.spec.ts` | Locator `Enter` → `Weiter` (twice); comment on the icon font no longer names the header | SPRINT_02 R3: locators adjusted, expected sets unchanged |
| `CLAUDE.md` §3, §4; `figma_code_mapping_procureaccess_v1.md` v1.13 | Shell as built; header rows „built (S2-15)“ | DoD |

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` over the whole diff: no security finding (no `innerHTML`, no new endpoint, the only external URL is the public repository, no secret). No query. No new dependency (`@angular/cdk/layout` and `MatSidenavModule` are part of the installed packages). UI strings only by key.

| # | Finding | Sorting |
|---|---|---|
| R1 | axe `aria-hidden-focus` (serious) on the open compact menu: the CDK focus-trap anchors of `mat-sidenav` are `aria-hidden` with `tabindex="0"` (found by the E2E test on mobile Chrome) | **Fixed:** the open menu traps the focus like a modal, so the sidenav is announced as one: `role="dialog"`, `aria-modal="true"`, name „Menü“ (`shell.nav.menu`); axe recognises the open modal |
| R2 | The brand link's accessible name joined the two spans without a space („ProcureAccessBarrierefreie …“; found by the component test) | **Fixed:** `&ngsp;` between name and claim |
| R3 | `openedChange` of the sidenav arrived after a programmatic close and reopened the state (`aria-expanded` stayed `true`; component test) | **Fixed:** the shell listens to `closedStart` only |
| R4 | The skip link with a plain `href="#main"` would put `#main` into the URL and the router would navigate | **Fixed:** click handler focuses `main` and prevents the navigation; `href` stays for semantics |
| R5 | Test doubles first lived under `layout/shell/`, which `tsconfig.app.json` compiles (`vi` unknown there) | **Fixed:** moved to `src/app/testing/` |
| R6 | `signedIn` re-read the token only when `user` changed; account deletion and a failed token refresh remove the token without `setUser(null)`, so „Abmelden“ would stay | **Fixed:** also re-read after every `NavigationEnd` (both paths navigate to `/home`); test added. A logout while already on `/home` is not a navigation and keeps the stale state until the next one — legacy until step 3, won't fix |
| R7 | The open compact menu shows Material's state layer on the focused item but no 3 px ring; Material buttons have no strong focus indicator in this theme yet (Handoff §1.1/§1.6 `strong-focus-indicators`) | Follow-up: theme-wide, not shell-specific (P5) |
| R8 | Handoff §3.1 names the SignedIn account button by `shell.userMenu.label` „Menü für {{displayName}}“ while Figma shows the visible text „Konto“: a visible label that is not part of the name fails WCAG 2.5.3 (label in name) | Not built here (step 3); open point P4 |

## 4. Checks

| Check | Result |
|---|---|
| `npm run lint` (Node 24.22.1) | All files pass |
| `npx ng test` (Vitest, all specs) | 69 tests in 16 files passed |
| `npx ng build` | Passes |
| `npm run typecheck` in `e2e/` | Passes |
| Playwright on the E2E stack: chromium, firefox, mobile-chrome | 57 passed (shell, J1, smoke, helper self-tests) |
| Playwright WebKit + mobile Safari | ⚠️ not run locally: the host lacks WebKit's system libraries (as in S1-18, S2-03); a run in the image `mcr.microsoft.com/playwright:v1.64.1-noble` stopped because the root partition ran full (image removed again). CI runs both projects; the shell tests use the real Tab key, so WebKit's link focus is the point to watch there |
| `node scripts/check-invariants.mjs` | Passed |
| `node scripts/check-figma-links.mjs` | Passed: 2 checked, 11 not built yet, 0 pending |
| `node --test scripts/check-i18n.test.mjs`, `node scripts/check-i18n.mjs` | 10 passed; 86 keys of common, shell, errors, legal, 0 errors |
| `reuse lint` (image `fsfe/reuse:latest`) | Compliant |
| Visual check (Chromium screenshots) | Full header at 1280 px with the active item; compact header at 360 px with the focused skip link; open compact menu |
| Backend tests | Not run: no backend change |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | R-SHL-01 „one h1 per page“: legacy pages have no `h1` since the header's went (D4); only `/unauthorized`, the e-mail confirmation and the legal pages have one | S2-16 (H1 focus) / S2-17 (start and error pages) and the rebuild of each screen |
| P2 | R-SHL-06 partly open: „Über ProcureAccess“ needs a target page or text, „Projekt WERTE.IT“ a URL, „Kriterium vorschlagen“ R-SUG-01 with `Contact:CriteriaSuggestion` (O-6 / UX copy O4). Should go to SPEC §13 as open questions | Philipp / WERTE.IT; R-SUG-01 in step 2 |
| P3 | The legacy start page is wider than 360 px (row of product cards), which zooms the mobile viewports out; it still has English icon-button names („Filters icon-button“, „Login icon-button“, „Contact icon-button“, the same pattern as #120). The compact E2E tests start on a legal page for that reason | S2-17 rebuilds the start page; S2-18 checks 320 px |
| P4 | Handoff §3.1: account button named „Menü für {{displayName}}“ with the visible text „Konto“ fails WCAG 2.5.3 (R8). Proposal: name = visible text, display name as description, or the visible text contains the name | Before step 3 (Handoff / UX copy) |
| P5 | No strong focus indicator on Material components in the theme (R7, Handoff §1.1 `mat.strong-focus-indicators`) | Proposed issue; theme task (A11Y-08 scaffolding, step 1) |
| P6 | Favourites and proposals (approver) have no header link any more (O-S2-4 2a); reachable by URL until step 5 retires or reworks them | Step 5 |
| P7 | #120: closed by this PR; #137: the header part is done, settings and profile remain; both are recorded in `TECH_DEBT_v2.md` at the close-out | Sprint 02 close-out |
| P8 | `TASK_PROMPTS.md` rename is the first commit of this branch (committed on `main` locally, not pushed separately) | Philipp: merge with the PR |
| P9 | WebKit / mobile Safari only in CI (check table) | CI of this PR |

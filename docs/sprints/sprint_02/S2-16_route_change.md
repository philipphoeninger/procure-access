# S2-16 — Route change: page title, H1 focus, announcement, lang

> **Sprint:** 02, P0 task S2-16 (2 points; roadmap M1 shell). **Issues:** none. **Requirements / invariants:** R-SHL-04; **A11Y-03** and **A11Y-05** → `active` in `TESTING_v22.md` §4.11. **Status:** done by Claude Code (2026-11-02), pull request open.
> **Scope:** `<title>` from `pageTitle.*` through `shell.pageTitle`, most specific first; focus on the H1 and announcement through `A11yAnnouncerService` on every route change; dialogs do not change the title; `<html lang>` follows the UI language. Not in scope: the error prefix `shell.pageTitle.error` (S2-11 P3, see D3), start page and error pages (S2-17), H1s on legacy pages (rebuild of each screen), the per-key throttle of the announcer (its first user).
> **Owner decisions 2026-11-02 (before the task):** UX copy v15 writes out `pageTitle.notFound` / `.forbidden` / `.server` as the H1 texts of §6.2 (S2-13 P3); Claude may write them because they follow from the UX copy.
> **Owner decisions 2026-11-02 (after the plan):** **D1** (1A) the start page gets its title now: `pageTitle.home` = „Barrierefreie Software beschaffen“ and the new key `shell.pageTitle.home` = „ProcureAccess – {{page}}“ (UX copy v15 U84). **D2** (2A) legacy routes get a key only where the legacy screen is the target screen; the others keep the app name as title, the focus goes to `main` and „ProcureAccess“ is announced until their screen is rebuilt. **D3** (3A) the error prefix `shell.pageTitle.error` waits for the first rebuilt form.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No `TitleStrategy`: `<title>` was always „ProcureAccess“ from `index.html` | `PageTitleStrategy` |
| F2 | No `A11yAnnouncerService`; three legacy screens (`filters-selection`, `products-list`, `proposal-list`) call CDK `LiveAnnouncer` directly | Service built; the legacy calls stay until their screens are rebuilt (P4) |
| F3 | `index.html` had no `lang`; only the legacy settings store set it from its saved `uiCustomization.language`, not from ngx-translate | `provideDocumentLanguage()`; the store's two `setAttribute('lang', …)` calls removed |
| F4 | UX copy v14 §5.16: `pageTitle.notFound` / `.forbidden` / `.server` said „wie die jeweilige H1 (§6.2)“; `pageTitle.home` held its prefix-less variant in parentheses; no key for the start page's order „ProcureAccess – …“ | UX copy v15 (U83, U84; D1) |
| F5 | Slash rows such as `` `pageTitle.settings` / `.account` `` are already read by `check-i18n.mjs` (one text per key, split at „ / “) | No split needed (corrected before the plan) |
| F6 | Only `/unauthorized` (H1 „403“), the e-mail confirmation and the legal pages have an H1 (S2-15 P1) | Focus falls back to `main#main` |
| F7 | `pageTitle` cannot become an `AREAS` root yet: `criteria.list` and `publish` still hold variants in parentheses, which `--write` would copy as text | Keys added to `de.json` by hand; check 2 compares them with the UX copy (P2) |
| F8 | Importing `APP_NAME` from `app.config.ts` into the strategy would close an import cycle `app.config` → `app.routes` → strategy → `app.config` | The fallback title is the `<title>` of `index.html`, read when the strategy is created |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docs/ux_copy_procureaccess_v15.md` (new) | U83: the three S80 page titles written out (Seite nicht gefunden / Kein Zugriff / Etwas ist schiefgelaufen = `errors.*.title`). U84: `pageTitle.home` = „Barrierefreie Software beschaffen“; new `shell.pageTitle.home` „ProcureAccess – {{page}}“ | S2-13 P3; D1 |
| `src/app/core/a11y/page-title.strategy.ts` (new) | `TitleStrategy`: the deepest primary route's `data[PAGE_TITLE_KEY]` → `shell.pageTitle` (start page `shell.pageTitle.home`); no key → app name. Acts only when the primary path changes, so the login outlet, query parameters and dialogs change nothing. The first page load sets only the title (the skip link stays the first Tab stop). Later changes: after the next render, focus on `main#main h1` (gets `tabindex="-1"` if it has none) or on `main`, then the title is announced. Re-translates on a language change without announcing; a counter keeps a late translation from overwriting a newer title; waits for the first active language instead of showing raw keys | R-SHL-04, A11Y-03, Handoff §3.4, §7 |
| `src/app/core/a11y/a11y-announcer.service.ts` (new) | `announce(key, params, { politeness })` (UX copy key → CDK `LiveAnnouncer`, polite by default) and `announcePageTitle(title)` for the title built by the strategy | Handoff §3.4, CLAUDE §9 |
| `src/app/core/a11y/document-language.ts` (new) | `provideDocumentLanguage()`: environment initializer that sets `<html lang>` to the current ngx-translate language and on every `onLangChange` | A11Y-05, Handoff §7 |
| `src/index.html` | `<html lang="de">` until the texts are loaded | A11Y-05 |
| `src/app/app.config.ts` | `TitleStrategy` → `PageTitleStrategy`; `provideDocumentLanguage()` | — |
| `src/app/app.routes.ts` | `data[PAGE_TITLE_KEY]`: `/home` → `pageTitle.home`, `/settings` → `.settings`, `/profile` → `.account`, `/unauthorized` → `.forbidden`, legal pages → `.legal.*`; all other legacy routes keep the app name | D2 |
| `features/settings/state/with-settings.ts` | The two `setAttribute('lang', …)` calls removed: one source for `lang` | A11Y-05 |
| `public/i18n/de.json` | `shell.pageTitle.home` (by `check-i18n.mjs --write`); `pageTitle.home`, `.settings`, `.account`, `.legal.*`, `.forbidden` by hand | F7 |
| Tests: `core/a11y/page-title.strategy.spec.ts`, `a11y-announcer.service.spec.ts`, `document-language.spec.ts` (new) | 14 tests: title page-first, start page frame, deepest key wins, fallback; `INV:A11Y-03` focus on H1 / on `main` + announcement; no focus or announcement on the first load, the login outlet, query parameters; dialog keeps the title; language change re-translates without announcing; announcer politeness; `INV:A11Y-05` `lang` follows `use('de')` / `use('en')`. A mutation check (focus call removed) fails both A11Y-03 tests | Tests with the code |
| `e2e/shell/route-change.spec.ts` (new) | `INV:A11Y-03`: from `/barrierefreiheit` (title checked, H1 not focused) via the footer link „Impressum“: title, focused H1, text of CDK's live region, axe. `INV:A11Y-05`: `lang="de"`, then English on the legacy settings page → `lang="en"` | DoD of the row (Vitest + E2E) |
| `e2e/smoke/stack.spec.ts` | Expected start page title „ProcureAccess“ → „ProcureAccess – Barrierefreie Software beschaffen“ | D1 |
| `docs/TESTING_v22.md` (new) | V83: A11Y-03 and A11Y-05 `active`; input document UX copy v15 | Sprint convention |
| `CLAUDE.md` §3, §4 | Route change and document language as built; `core/a11y/` in the layout | DoD |

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` over the whole diff: no security finding (no `innerHTML`, no new endpoint, no secret; the title is set through `Title.setTitle`, the announcement through `LiveAnnouncer`, both as text). No query. No new dependency (`@angular/cdk/a11y`, `Title` and `TitleStrategy` are part of the installed packages). UI strings only by key; the only literal is the app name taken from `index.html`.

| # | Finding | Sorting |
|---|---|---|
| R1 | Focus timing in zoneless Angular: when the router reuses a component (`/product/1` → `/product/2`), would `afterNextRender` wait for an unrelated render? | No change needed: registering an `afterNextRender` hook notifies the change detection scheduler itself (`AfterRenderImpl.register` → `scheduler.notify`) |
| R2 | If the translation file fails to load, `applyTitle` rejects; the title keeps its old value and the rejection shows in the console | Won't fix: without texts the whole UI is broken; the console error is the signal, and a fallback would hide it |
| R3 | Focus on the H1 and the announcement of the title may make a screen reader read the page name twice | Won't fix here: Handoff §3.4 and §7 ask for both; judged in the manual screen-reader pass (A11Y-07, P3) |
| R4 | A language change while a route change is still translating cancels that route change's announcement (the newer update wins) | Won't fix: rare, the title is still correct, and the H1 focus still happens |
| R5 | `announce(key, …)` has no production caller yet | Won't fix: it is the service interface of Handoff §3.4 and is tested; the next users (S2-17 states, step 2 counters) need it. The per-key throttle is left out until a user needs it |
| R6 | `/unauthorized` shows the H1 „403“ but the title „Kein Zugriff“ | Accepted: the title follows UX copy S80; S2-17 replaces the page (P1) |

## 4. Checks

| Check | Result |
|---|---|
| `npm run lint` (Node 24.22.1) | All files pass |
| `npx ng test` (Vitest, all specs) | 83 tests in 19 files passed |
| `npm run build:release` | Passes |
| `npm run typecheck` in `e2e/` | Passes |
| Playwright on the E2E stack: chromium, firefox, mobile-chrome | 63 passed (after the smoke test's title expectation was updated for D1) |
| Playwright WebKit + mobile Safari | ⚠️ not run locally (host lacks WebKit's libraries, as in S2-15); CI runs both |
| `node scripts/check-invariants.mjs` | Passed; A11Y-03 with 3 tests (FE, E), A11Y-05 with 2 |
| `node scripts/check-figma-links.mjs` | Passed: 2 checked, 11 not built yet, 0 pending (no mapped component changed) |
| `node --test scripts/check-i18n.test.mjs`, `node scripts/check-i18n.mjs` | 10 passed; 87 keys of common, shell, errors, legal, 0 errors (the hand-added `pageTitle.*` keys pass check 2) |
| `reuse lint` (image `fsfe/reuse:latest`) | Compliant |
| Backend tests | Not run: no backend change |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Legacy routes without a title key (D2): `/filters`, `/products`, `/product/:id`, `/favorites`, `/product-proposal/:id`, `/criterion-proposal/:id`, `/proposals`, `/reset-password`, `/confirm-email`; most also lack an H1 (S2-15 P1). Each gets its key with its rebuild; `/unauthorized` with S80 in S2-17 | Rebuild of each screen; S2-17 |
| P2 | `pageTitle` as an `AREAS` root of `check-i18n.mjs`: needs the rows `pageTitle.criteria.list` (saved-list variant) and `pageTitle.publish` (stepper variant) in UX copy §5.16 split into one key per variant | Philipp (UX copy), before step 2 builds S11/S35 |
| P3 | Manual check whether H1 focus plus title announcement is read twice (R3) with NVDA, JAWS and VoiceOver | Next A11Y-07 pass |
| P4 | The three legacy `LiveAnnouncer` calls (F2) move to `A11yAnnouncerService` with UX copy keys when their screens are rebuilt | Steps 2 and 5 |
| P5 | `shell.pageTitle.error` (S2-11 P3) with the first rebuilt form, including the reset once all errors are fixed (D3) | First form of step 2 or later |
| P6 | Handoff (`design_handoff_procureaccess_v1.md`), SPEC_v2 and TESTING still name UX copy v14 in their headers (TESTING_v22 now v15); no content depends on the change | Next version of each document |
| P7 | WebKit / mobile Safari only in CI | CI of this PR |

# S5-26 — Settings page S60

> **Sprint:** 05 · **Task:** S5-26 (3.5 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-SET-01 (met), R-SET-02, R-SET-04, §8.12 (v2.44); **A11Y-08** complete — „applied without reload“ through S60 (TESTING_v84 V283); A11Y-06 stays green (V284); **A11Y-09** S60 in `reflow.spec.ts` (V285); ARCHITECTURE C162; UX copy v37 U157
> **Status:** done — pull request #369 open (2026-11-17)
> **Scope:** S60 at `/einstellungen` per Handoff §5.3 with its menu entry; a change applies without reload and is saved; S60 in `reflow.spec.ts`. No server change, no EF migration. The legacy settings page and its `LEGACY_EXCEPTIONS` entries had already gone with S5-25b (decision S5-25 2a), so the prompt's line about deleting them had nothing left to do.
> **Owner decisions (2026-11-17, before the task):** **D1 (a)** — „Deutsch“ as the one chosen option of „Sprache“, new UX key `settings.language.de` in UX copy v37 · **D2 (a)** — each change is saved at once; a failed save shows the existing system states of the error interceptor, announces nothing, and the choice stays applied for this session and is saved with the next change (instead of the prompt's „error summary on a failed save“: S60 has no field that can be wrong, and the error summary of Handoff §6.4 lists field errors) · **defaults** — the entry last in the menu „Konto“ before „Abmelden“ (U138, U140), also in the compact menu; sign-in required (UX copy §4); no redirect from the old `/settings`; S60 reads the settings again before it shows; several quick changes are saved one after another, each with all four settings.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `main` at bf40002 (S5-25b merged): `withSettings` reads and applies the settings with the session (`BACKGROUND_REQUEST`; a failed read follows the system), but has no save method; no route `/einstellungen`, no menu entry; `PUT /api/v1/account/preferences` exists since S5-25a | Save method, page, route and entry added |
| 2 | UX copy §5.14 had `settings.language` and its hint, but no text for the option „Deutsch“ that R-SET-01 and the Handoff ask for | Stopped and asked: D1 (a), UX copy v37 U157 |
| 3 | The prompt asked for an error summary on a failed save, the Handoff for changes that apply immediately without a save button; no field of S60 can be invalid (the page never offers `high` contrast, which the server refuses) | Stopped and asked: D2 (a) |
| 4 | A failed read after the sign-in leaves `preferences` at `null`; a change on S60 replaces all four settings, so it would have saved the defaults over the stored values | S60 reads the settings again in its resolver; a failed read shows S80 in place |
| 5 | S60 has no Figma frame (sprint risk R5); built from Handoff §5.3 („form card as in S51“) like S20 | No Figma comparison; no `@figma` tag (S60 is not in the Figma mapping) |
| 6 | `header-states.spec.ts`, `reflow.spec.ts` (compact menu) and `main-nav.spec.ts` expected no „Einstellungen“ since S5-25b | Expect it again |

## 2. Changes

### 2.1 Client

- **`features/settings/models/user-preferences.ts`:** `SETTINGS_PATH = 'einstellungen'` (import-free, for `app.routes.ts`) and `SYSTEM_PREFERENCES`.
- **`services/api/preferences-api.service.ts`:** `read()` (`GET`, `CALLER_SHOWS_FAILURES`: the page shows a failure in place; no answer still turns the offline notice on) and `save()` (`PUT`, unmarked: the error interceptor shows a failure, D2 (a)).
- **`state/with-settings.ts`:** `readPreferences()` reads and applies, rejects on failure; `changePreferences(next)` applies at once (without reload), then saves after the save still in flight and resolves `true` only for the newest change; a failed save rejects and keeps the choice. One counter `latest` replaces `requestedFor`: `loadPreferences`, `readPreferences` and `changePreferences` each take a number, and only the newest answer is applied (see R1).
- **`pages/settings/`** (new): `settings-page.{ts,html,scss}` — H1 `settings.title`, intro, one outlined card with four `fieldset`s of `mat-radio-group` in the order of the Handoff: „Sprache“ (one chosen option, hint linked with `aria-describedby`), „Erscheinungsbild“, „Bewegung und Animationen“, „Kontrast“ (only the system option until H2); no button; `settings.saved` through `A11yAnnouncerService`. `settings.routing.ts` (`authGuard`, resolver with the loading bar, title `pageTitle.settings` or `pageTitle.server`), `settings-load.ts` (route-level holder, as `SavedListLoad`).
- **`app.routes.ts`:** lazy route `einstellungen`. **`layout/main-nav`:** „Einstellungen“ (`shell.nav.settings`) after „Kriterienlisten“ in the menu „Konto“ and in the compact menu, `routerLinkActive` with `aria-current="page"`.
- **i18n:** area `settings` joins `AREAS` of `check-i18n.mjs`; `de.json` written by `--write` (16 keys; `settings.contrast.high` stays unused until H2).
- **`testing/shell-test-doubles.ts`:** routes `kriterienlisten` and `einstellungen` instead of the legacy `settings`.

### 2.2 Tests

- FE: `with-settings.spec.ts` (+7: the page's read applies and rejects; `INV:A11Y-08` a change applies before the answer and saves all four; two quick changes in order, only the last counts; a failed save keeps the choice and the next change saves; a read before a change does not undo it; a slower failing read after the sign-in does not replace the page's read), `settings-page.spec.ts` (new, 9, `INV:A11Y-06`, axe), `settings.routing.spec.ts` (new, 2), `preferences-api.service.spec.ts` (new, 3), `main-nav.spec.ts` (entry in both menus, current on S60).
- E2E: `identity/preferences.spec.ts` new test `INV:A11Y-08 INV:A11Y-06 INV:A11Y-01` (own account; S60 through the menu, axe in both appearances; „Dunkel“ and „Bewegungen reduzieren“ applied with a `window` marker proving no reload, `settings.saved` in the live region, axe in the explicit dark appearance; stored values from the API, shown again after a reload, storage empty); `shell/reflow.spec.ts` S60 (read only, seeded confirmed user) and „Einstellungen“ in the compact menu; `shell/header-states.spec.ts` menu items.

### 2.3 Documents

UX copy v37 (U157, `settings.language.de`; v35 deleted), TESTING_v84 (V283 – V285; A11Y-06 and A11Y-08 rows; v82 deleted), ARCHITECTURE_v76 (C162; v74 deleted), SPEC v2.44 (v2.44-1, -2; §8.12 note; header versions), Handoff 1.28 (S60 as built), `CODEBASE_v1.md` §2.77 and §3, `CLAUDE.md` (state line, last task). Figma mapping unchanged (S60 has no frame); `LEGACY_EXCEPTIONS` unchanged (nothing left of the legacy settings); TECH_DEBT waits for the sprint end (process step 14): its §9.2 rows „S60 replaces the legacy settings store“ (settings part) and „A11Y-08 with the explicit setting and its persistence“ are done.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | When the background read after the sign-in fails *after* S60's own read succeeded (the user opens S60 right after signing in), the store fell back to `null`: S60 showed the defaults, and the next change would have saved them over the stored values (`requestedFor` and the change counter both still matched) | **fixed** — one counter `latest` for reads, sign-outs and changes; test `SlowerReadAfterTheSignInFails_DoesNotReplaceTheReadOfThePage` |
| R2 | If the session ends on the server while S60 is open, a change's `PUT` gets 401, the login overlay opens and retries the request after the sign-in; a *different* user signing in there would get the page's values (the first user's settings plus the change) | **won't fix** — the overlay's retry is generic (SEC-03, S5-08) and carries this for every request; signing in as someone else in the overlay of another user's page is not a flow of the product. Sign-out through the header navigates to `/` |
| R3 | A change that is followed by a sign-out or a new visit of S60 before its save answers is not announced | **won't fix** — the user has left the change; the next change announces |
| R4 | „Sprache“ and „Kontrast“ are radio groups with one option | **kept** — D1 (a) and Handoff §5.3 („only the system option until H2“); arrow keys do nothing, the option is read as chosen |
| R5 | Security and privacy: the settings of the session's own user only; `PUT` through Angular's XSRF header (antiforgery, SEC-02); nothing in browser storage (tested); no user id logged | **no finding** |
| R6 | Performance: one `GET` when S60 opens, one `PUT` per change, saves in sequence | **no finding** |
| R7 | Accessibility: legends name the groups, the hint is linked, no button with `disabled`, the announcement through the announcer service, H1 focus through the route change; axe in both appearances (FE and E2E), reflow at 320 px | **no finding** |

## 4. Checks

- Client: `ng test` **768 passed**; `npm run lint` clean; `npm run build:release` complete (its CSS warning about the selector `50%` exists on `main` too).
- E2E, local: the disk had 0.9 GB free, too little to build the E2E images, so the stack ran from its images of `main` (the API is unchanged by this task) and the branch's client through `ng serve --port 4300` (`E2E_BASE_URL`). Chromium and Firefox: `identity/preferences.spec.ts` and `shell/reflow.spec.ts` **78 passed**; with mobile-chrome also `shell/header-states.spec.ts`, the compact-menu reflow and `journeys/`: all passed except J8 in all three browsers, which compares the origin of the mailed confirmation link (`http://localhost:4200`, the API's configured client) with the page (`:4300`) — an effect of the local set-up only; CI runs the client image on 4200. WebKit and mobile Safari in CI only. E2E `typecheck` and `lint` clean.
- Backend: not run — no server file changed; CI is the authority.
- `check-invariants` passed; `check-figma-links`, `check-i18n` (3 × 0 errors), `check-icons` 0 errors; `reuse lint` compliant.

## 5. Open points

1. **S5-28**: the account page S61 replaces the legacy profile (`/profile`, still the target of „Konto“ in the menu); the account deletion removes the user's `UserPreferences` row (S5-25a R2).
2. **H2** (R-SET-04): „Hoher Kontrast“ (`settings.contrast.high`) on S60 and its colours once the design and the review exist; the API refuses `high` until then.
3. **TECH_DEBT** at the sprint end: the two S60 rows of §9.2 (see §2.3).
4. **Issue #368** (from S5-25b) stays: the legacy languages `en/es/fr.json`; they do not carry the `settings.*` keys.

# S5-09 — Sign-in S01, login overlay and sign-out

> **Sprint:** 05 · **Task:** S5-09 (2 points) · **Issues:** #170 (L1, the login outlet; L2 and L3 stay open for steps 6), #319 (deferred to S5-13 by decision 1 (a))
> **Requirements / invariants:** R-AUTH-03 (without the lockout hint, #319), R-AUTH-07 (base: overlay with the S01 form), R-AUTH-08; A11Y-01, A11Y-03, A11Y-08, A11Y-09 stay green and now cover S01 and the overlay; SEC-03 keeps its overlay tests on the shared form. No invariant changes its status, so no new TESTING version
> **Status:** done — pull request #331 open (2026-11-14)
> **Scope:** S01 in the named `login` outlet as the form card of Handoff §5.3, inside the shell over the page it opened on; one sign-in form for S01 and the login overlay; the page title of the outlet; sign-out clearing the in-memory state with the focus on the start page's H1; S01 and the overlay in the reflow check. Client only, no EF migration, no new dependency.
> **Owner decisions (control chat, 2026-11-14):** 1 (a) the lockout hint (#319) is decided with the lockout in S5-13; S01 shows `auth.login.error.invalid` for every refused sign-in · 2 (a) S01 shows inside the shell, in `main` in place of the page · 3 `pageTitle.login` „Anmelden“ confirmed (UX copy v27 U112) · defaults confirmed: route `/(login:anmelden)` with a redirect from `auth`, „Jetzt registrieren“ to the legacy registration, `reasonSave`/`draftKept` built now and wired to „Liste speichern“ in S5-20, after the sign-in S01 closes over the page, sign-out clears the configurator too, one form for S01 and the overlay · „Passwort vergessen?“ (a): a button that opens the legacy reset dialog until S03 (S5-13)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The legacy login sat in the outlet `/(login:auth)` beside the shell and replaced it while open (no skip link, header or footer); it scrolled sideways at 320 px (#170 L1) | S01 in `main` of the shell (decision 2 (a)) |
| 2 | „Anmelden“ in the header opened the outlet together with the start page (`['', { outlets }]`), so the page the user was on was gone | `showLogin()` opens only the outlet |
| 3 | `PageTitleStrategy` ignored the `login` outlet: no title, no focus, no announcement when it opened or closed | The outlet counts as the page |
| 4 | The S5-08 overlay was minimal: a notice instead of the error summary, no password toggle, no links (S5-08 open point 7) | One form `pa-login-form` for both |
| 5 | Sign-out forgot the session and the proposals only and moved no focus | `endSession()` and the H1 of the start page |
| 6 | UX copy had no page-title key for S01 | Owner confirmed `pageTitle.login` (decision 3) |
| 7 | The legacy request for a reset link is a dialog, not a page | Decision „Passwort vergessen?“ (a) |
| 8 | The server answers a locked account like a wrong password (S5-07 Q2 (a)), so the lockout hint of R-AUTH-03 cannot be shown (#319) | Decision 1 (a): with S5-13 |

## 2. Changes

### 2.1 Shell, routes and page title

- `app.html`: the `login` outlet moved into `main#main`. The primary outlet sits in `.pa-main__page`, `hidden` while the `login` outlet is open and `display: contents` otherwise, so the pages stay items of the column layout and nothing else changes for them. `showShell` is gone; `loginOpen` follows the outlet's `activate`/`deactivate`.
- `app.routes.ts`: S01 `LoginPage` at `anmelden` in the `login` outlet with `pageTitle.login`; `auth` redirects there (legacy `register` and `reset-password` still navigate to it until S5-10).
- `features/identity/models/login-navigation.ts`: `LOGIN_PATH`, `LoginReason`, `LoginNavigationState`, without imports. The first version exported them from `auth.service.ts`; that closed a cycle `app.routes.ts` → `auth.service.ts` → `app.config.ts` → `app.routes.ts`, and the route path was `undefined` when the routes were built (NG04014, blank page). The unit tests could not see it; the E2E run on the dev server did.
- `PageTitleStrategy`: the page is the open `login` outlet, else the primary page. Opening or closing the outlet sets the title from the outlet's key, focuses the first H1 in `main` that is not inside `[hidden]` (else `main`) and announces the title. The hidden page's title state (S10's step number) is kept and given back when S01 closes over the same page (found in the self-review, §3). `focusPageHeading()` focuses the H1 after the next render, for a change the router does not see.
- `authGuard` sends to `/(login:anmelden)` over the start page (unchanged behaviour otherwise).

### 2.2 Sign-in form, S01 and overlay

- `pa-login-form` (`features/identity/components/login-form/`): `common.requiredNote`, outlined „E-Mail-Adresse“ and „Passwort“, „Passwort anzeigen“ (text button with `aria-pressed` and `aria-controls`, no icon, so the icon subset stays), „Passwort vergessen?“ (legacy dialog, then `AuthService.forgotPassword` as before), „Anmelden“ with the 20 px spinner and `aria-disabled` while busy. A projected `paLoginSecondaryAction` stands before „Anmelden“ (the overlay's „Schließen“). Layout `card` (S01: outlined card, actions below it, stacked below 600 px with the primary last) or `dialog`.
- Validation on submit only (Handoff §6.4.1) through an `ErrorStateMatcher` that shows a field error after the first submit and a server error at once. A failed submit fills `pa-error-summary`, which takes the focus: one linked line per empty field (`errors.field.required`), `auth.login.error.invalid` without a field for the server's error on `$`, a field's own server error through `applyServerErrors`, or `errors.antiforgery`. Other failures (5xx, no answer) stay with the error interceptor. Ids carry a prefix, so S01 and the overlay never share one.
- S01 `LoginPage` (`features/identity/pages/login/`): H1 `auth.login.title`, the info notice `auth.login.reasonSave` when the router state says `save`, the form, and the link `auth.login.noAccount` (the whole text, to the legacy `/(login:register)` over the same page). `errorsShown` sets the „Fehler:“ prefix of the page title. After the sign-in the outlet closes; with the `save` reason and a loaded list `auth.login.draftKept` is announced (the visible hint on S11 belongs to S5-20).
- `LoginOverlay` uses the form (`formLayout="dialog"`, ids `pa-login-overlay-…`) with `shell.session.expired.*` and „Schließen“ before „Anmelden“; success still announces `shell.session.restored` and closes with `true`.
- `AuthService.showLogin(reason?)` opens the outlet over the current page.

### 2.3 Session start and end

- Root store: `startSession(email, password)` signs in and applies the legacy settings of the signed-in user and loads the proposals (moved from the legacy login page, so the overlay does it too). `endSession()` signs out and, also when the request fails, calls `resetConfigurator()` (new in `withConfigurator()`: back to the initial state, running filter-option and list requests dropped, search timer cancelled), empties the proposals and the legacy favourites and reloads the anonymous settings.
- „Abmelden“ (`pa-main-nav`): `endSession()` (a failure is ignored there, the session is forgotten anyway and the error interceptor shows the failure), then `/` and `focusPageHeading()`, so the focus lands on the start page's H1 also when it was already open.

### 2.4 Tests

- Vitest: `login-form.spec.ts` (11: required note and fields, no error before submit, summary with focus and links, sign-in, `$` without a field, a field's server error, antiforgery, other failures, busy, password toggle, legacy reset dialog), `login-page.spec.ts` (5: over the page, reason notice, closing, `draftKept`, „Fehler:“ title), `login-overlay.spec.ts` (3, two with `INV:SEC-03`), `page-title.strategy.spec.ts` (+5 with the outlet, three `INV:A11Y-03`), `main-nav.spec.ts` (sign-out with focus, sign-out after a failed request), `with-configurator.spec.ts` (`resetConfigurator`). Client: 60 files, 550 tests.
- E2E: `e2e/identity/login.spec.ts` (6, keyboard: S01 over S10 with axe in both appearances and the error summary, a refused sign-in, S10's choice kept after the sign-in, the overlay after a 401 on „Kriterienliste anzeigen“ with axe and the retry to S11, „Schließen“ in the overlay, sign-out clearing the draft). `e2e/support/session.ts` fakes the session — the probe goes through the API so its antiforgery cookie is set, `login`/`logout` answer 204, `/api/me` and `/api/Proposals` answer without user data — until the E2E users of S5-14. `reflow.spec.ts`: S01 with the error summary and the overlay at 320 px.
- `endSession()` has no unit test of its own: the root store starts settings, media queries and the product load on creation, so a unit test would test doubles; the E2E sign-out test checks that the S10 draft is gone.

### 2.5 Documents

UX copy v27 (`pageTitle.login`, U112; v25 deleted), `de.json` through `check-i18n.mjs --write`, SPEC v2.19 (R-AUTH-03 points to §13 #319, §8.8 screens paragraph, §13 row #319), ARCHITECTURE v51 C137 and the as-built note in §8.3, CLAUDE.md §3 and §4. No change to TESTING (no status change), the Figma mapping (S01 has no frame) or `LEGACY_EXCEPTIONS` (no listed file was replaced; the legacy `login/login.ts` is unrouted now and goes with S5-10).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: nothing reads or writes browser storage; the form sends only the two fields; the fake session exists only in `e2e/`. N+1: none (client only). Accessibility: checked with axe in both appearances (S01, error summary, overlay), keyboard only, reflow at 320 px.

| Finding | Sorting | Reason / fix |
|---|---|---|
| Closing S01 over S10 cleared S10's page-title parameters; S10 does not set them again, so the title would read „Schritt {{n}} von 4“ | fix | The strategy keeps the hidden page's title state and restores it (test `LoginOutletClosedOverPageWithParams_…`) |
| A failed logout request rejected the click handler unhandled, and the start page did not open | fix | `endSession()` clears state in `finally`; „Abmelden“ ignores the rejection and still opens `/` (test) |
| Material leaves `aria-invalid` off an empty required field (it exposes `required` instead) and links the error with `aria-describedby` | won't fix | Material's choice; WCAG 3.3.1 is met through the linked error text and the summary. The tests check the link |
| S01 can be opened while signed in (deep link) and signs in again | won't fix | Harmless; the header offers „Abmelden“ instead of „Anmelden“ then |
| `authGuard` sends a guarded deep link to S01 over the start page, not over the guarded page, so after the sign-in the user is on the start page | follow-up | Pre-existing; the guarded legacy pages (`/profile`, `/favorites`) are replaced in S5-28 — open point 3 |
| The content link „Noch kein Konto? Jetzt registrieren“ has the browser's link colour, as the suggestion link on S11 | follow-up | No global content-link style yet; open point 4 |

No correctness or security bug that affects current behaviour is left: merge recommended after CI.

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test --watch=false` | 60 files, 550 tests passed |
| `npx ng lint` | all files pass |
| `npx ng build --configuration production` | initial total 727.46 kB (726.80 kB before); the critical-CSS warning about `50%` is the same on `main` |
| `node scripts/check-invariants.mjs` | passed (TESTING_v58) |
| `node scripts/check-figma-links.mjs` | 6 checked, 0 errors |
| `node scripts/check-i18n.mjs` | 0 errors (UX copy v27) |
| `node scripts/check-icons.mjs` | 0 errors |
| e2e `npm run typecheck`, `npm run lint` | clean |
| `reuse lint` (Docker) | compliant |
| E2E | The local disk had 0.4 GB free, too little to build the E2E stack, so the suite ran against `ng serve --port 4300` (this branch) and the API of the running dev stack (an image from before S5-07, so the probe answers 404 there; the fake session forces 200): Chromium and Firefox 168 passed, 2 failed — both the J1 cell „Kurzbeschreibung“, because the kept dev database holds a `T.1` from before S4-24 (CLAUDE §3: `docker compose down -v`), unrelated; mobile Chrome `identity/` and `shell/` 44 passed. WebKit runs in CI only. The full matrix on the real stack runs in CI |
| Figma | S01 has no frame: compared with the S51 card (`2136:4005`): H1 above, required note and fields in the outlined card, actions below it, stacked below 600 px with the primary last |

## 5. Open points

1. **Lockout hint (#319)** — decided with the lockout in S5-13 (decision 1 (a)); SPEC §13 row #319.
2. **„Liste speichern“ → S01** — S5-20 calls `AuthService.showLogin('save')` and shows the visible `auth.login.draftKept` hint on S11; S01 and the announcement are ready.
3. **Guarded deep links** — `authGuard` opens S01 over the start page; a return to the guarded page after the sign-in is not built (legacy pages only, replaced in S5-28).
4. **Content-link colour** — content links outside the footer (S01's registration link, S11's suggestion link) use the browser's link colour; a shared style in `main.scss` would be a small follow-up issue (proposed).
5. **Legacy pages in the outlet** — the legacy registration now shows inside the shell too, until S02 (S5-12) moves it to `/registrieren`; the legacy `login/login.ts` is unrouted and goes with S5-10.
6. **#170** stays open for L2 and L3 (product list and page, step 6); L1 is done.

# S5-12b — Registration and e-mail confirmation, client part

> **Sprint:** 05 · **Task:** S5-12b (second half of S5-12, 3.5 points together with S5-12a) · **Issues:** — (#301 closed by S5-12a)
> **Requirements / invariants:** SPEC R-AUTH-01, R-AUTH-02, §8.8 (v2.22); ARCHITECTURE C72, §8.3 (C140); **SEC-10** client part (TESTING_v61 V192), E2E part moved to S5-16; **PRV-11** E2E part → `active` (V193); SEC-02 list shorter (V194); **A11Y-09** stays green with S02 and both confirmation results (V195)
> **Status:** done — pull request #334 open (2026-11-14)
> **Scope:** S02 at `/registrieren`; the confirmation page at `/registrieren/bestaetigen` (token out of the URL first, S5-12a R4); the reusable notice `auth.confirm.required` with „Bestätigungs-E-Mail erneut senden“; S01's „Jetzt registrieren“ to S02; the legacy sign-up and confirmation removed; S02 and the confirmation page in `reflow.spec.ts`; the E2E registration with the mail from Mailpit.
> **Owner decisions (2026-11-14):** 1 (a) — the notice is built now as a reusable piece with component tests; S5-16 places it and brings SEC-10's E2E part · 2 (a) — S5-12b tests the real registration with the mail from Mailpit (PRV-11 E2E part), error cases with faked answers; S5-14 adds the seeded users and sign-in/sign-out · 3 (a) — the legacy `POST /api/signUp`, `GET /api/confirm-email` and the legacy register and confirm pages go now · defaults accepted without a question: S02 in place of the page, the privacy link in a new tab, the minimum from the environment like `priorityWeight`, the confirmation sent on load without a button, `/register` → `/registrieren`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | No client screen shows a gated action yet (the first is S5-16), and S61 does not exist | Decision 1 (a): the notice as a component, tested alone |
| 2 | S5-14 plans the account journey register → Mailpit → confirm → sign in → sign out | Decision 2 (a): this task covers register → Mailpit → confirm, S5-14 extends it |
| 3 | After S02 nothing calls the legacy `POST /api/signUp`, which has no rate limit, no privacy acknowledgement and answers 400 for a taken address (S5-12a R6) | Decision 3 (a): removed with its confirmation endpoint and pages |
| 4 | `app.routes.ts` must not import the store or a service: they import `app.config.ts`, which imports the routes (the cycle of S5-09) | The confirmation page's guard and resolver live in lazy child routes |
| 5 | The confirmation link carries the token in its query (S5-12a R4) | A guard moves it into memory and redirects without it before anything is sent |
| 6 | The confirmation request needs the antiforgery token of the session probe, which the app starts at init | The resolver awaits `sessionReady()` first |
| 7 | The client's nginx log drops the query string but writes `$http_referer`; the JS and CSS of the confirmation page are requested with the full link as Referer | Self-review R1: referrer policy `strict-origin` |
| 8 | The disk had 1.6 GB free: too little to build the E2E stack images | E2E run against the branch's API (`dotnet run`, throwaway PostgreSQL and Mailpit) and `ng serve`; CI runs the stack |

## 2. Changes

### 2.1 S02 „Konto erstellen“ (`features/identity/pages/register/`)

- Route `/registrieren` (`REGISTER_PATH` in `models/login-navigation.ts`, which stays without imports), page title `pageTitle.register`, in place of the page; `/register` redirects there. S01's „Noch kein Konto? Jetzt registrieren“ closes the `login` outlet and opens S02 (route change: focus on its H1).
- Form card like S01: `common.requiredNote`, „E-Mail-Adresse“ (`autocomplete="email"`), „Passwort“ (`new-password`) with the hint „Mindestens {{min}} Zeichen.“ and „Passwort anzeigen“ (`aria-pressed`), `auth.register.privacyNote`, the acknowledgement checkbox, „Konto erstellen“ with the button spinner.
- The link „Datenschutzhinweise“ inside the checkbox label: its text is `legal.privacy.title` without its soft hyphen, found in `auth.register.consent` (no German text in code; without a match the label shows without link); `routerLink="/datenschutz"`, `target="_blank"`, hidden `common.newTab`.
- Checked on submit: required fields, `errors.field.email`, `errors.field.minLength` / `.maxLength` with the current count, `auth.register.consent.required` under the checkbox (linked by `aria-describedby`; ticking the box clears it). The error summary lists the same texts with links to the fields (the checkbox's input `pa-register-consent-input`) and takes the focus; „Fehler:“ prefixes the title.
- 🔧 minimum: `passwordMinLength: 12` in both environment files, read through `PASSWORD_RULE` (`models/password-rule.ts`, maximum 128 fixed by SPEC).
- `AuthService.register` (`REGISTER_PROBLEMS`: `validation`, `antiforgery`, `rate-limit`): server field errors to their fields (API-02), `errors.antiforgery` and `errors.rateLimit` (plural, minutes) into the summary; other failures keep the interceptor's states.
- Success replaces intro and form with the H2 `auth.register.success.title` (focused) and `auth.register.success` with the trimmed address — the same for a known address.

### 2.2 Confirmation page (`features/identity/pages/confirm-email/`)

- `/registrieren/bestaetigen` loads `CONFIRM_EMAIL_ROUTES` (`confirm-email.routing.ts`).
- `confirmEmailTokenGuard`: a `token` query → `PendingConfirmationToken` (root service, memory only) and `RedirectCommand` to the path without it (`replaceUrl`, `onSameUrlNavigation: 'reload'`).
- `confirmEmailResultResolver` resolves `data[PAGE_TITLE_KEY]`: loading counter on, `sessionReady()`, `AuthService.confirmEmail` (`CALLER_SHOWS_FAILURES`); 204 → `pageTitle.confirm.done`, 400 `validation` or no token → `pageTitle.confirm.invalid`, anything else → `pageTitle.server`. So `PageTitleStrategy` sets the title, focuses the H1 and announces it like on any route change.
- `runGuardsAndResolvers: 'always'`: a second link opened on the page is redeemed; without a new token the resolver returns the last result (S01 opening over the page keeps „bestätigt“). A reload after the guard took the token shows „nicht mehr gültig“.
- `ConfirmEmailPage`: H1, text and „Zur Startseite“ like S80 (`auth.confirm.done.title`/`done`, `auth.confirm.invalid.title`/`invalid`, `errors.server.title`/`body`).

### 2.3 Notice (`features/identity/components/confirm-required-notice/`)

`pa-confirm-required-notice`: `pa-notice` attention with `auth.confirm.required` and the outlined button „Bestätigungs-E-Mail erneut senden“ (spinner, `aria-disabled`, one request at a time; the label wraps at 320 px). `withIdentity().resendConfirmation()` → `AuthService.resendConfirmation` (`RESEND_PROBLEMS`: `email-delivery`, `rate-limit`). The result is a line in the notice, announced through `A11yAnnouncerService`: `auth.confirm.sent` with the session's address, `errors.email.sendFailed`, or `errors.rateLimit`; other failures (a 401 opens the overlay and repeats the request) keep the interceptor's states. Not on a screen yet.

### 2.4 Legacy removed (decision 3 (a))

Server: `POST /api/signUp`, `GET /api/confirm-email`, `UserService.SignUpAsync`, `ConfirmEmailAsync`, `SendConfirmationEmailAsync`, `RegistrationRequest`; SEC-02's exemption list is the export, `forgot-password`, `reset-password`. `LegacyAccountEmailTests` keeps its two reset cases (the confirmed account is stored directly). Client: `features/identity/register/` and `pages/email-confirmed/` with their `LEGACY_EXCEPTIONS` entries, the routes (login outlet `register`, `confirm-email`), `AuthService.register(LoginModel)` and `confirmEmail(userId, token)`; `showRegister()` (unrouted legacy login page) opens S02. The icon `app_registration` was only used by the legacy register page: removed from the icon list, subset regenerated (36 icons).

### 2.5 Texts and checks

`auth.register` and `auth.confirm` are `AREAS` entries of `check-i18n.mjs`; `--write` copied their texts (UX copy v28, no new wording). `index.html` has `<meta name="referrer" content="strict-origin">` (R1).

### 2.6 Tests

- Component: `register-page.spec.ts` (11), `confirm-email.routing.spec.ts` (8: token out of the URL before the request, order loading → probe → confirm, the three results with titles, a reload, S01 over the page, a second link), `confirm-required-notice.spec.ts` (6, two `INV:SEC-10`), `with-identity.spec.ts` (+1 resend), `login-page.spec.ts` (link to `/registrieren`).
- E2E `e2e/identity/register.spec.ts` (6, keyboard): the real registration with the mail from Mailpit (`INV:PRV-11`: one recipient, no image/style/script, one link to the own origin and path, same link in the text, no password), the link opened (token not in the address, no request with the token in its Referer, „bestätigt“, a second click the same), a known address (same success, „already registered“ mail with the S01 link), an unknown token, „Jetzt registrieren“, the empty submit, the faked 429. `e2e/support/mailpit.ts` reads Mailpit's API (`E2E_MAILPIT_URL`).
- Reflow: `/registrieren`, `/registrieren/bestaetigen` (no token) in the page list; S02 with the summary and after a faked submit; „bestätigt“ with a faked answer.

### 2.7 Documents

TESTING v61 (V192–V195; v59 deleted), ARCHITECTURE v54 (C140; v52 deleted), SPEC v2.22 (§8.8 antiforgery and screens), CLAUDE.md (header, §3, §4, §5). No UX copy change (all texts were in v28), no Figma mapping change (S02 has no frame).

## 3. Self-review

Checklist of CLAUDE §6.7 (`/engineering:code-review` on the working tree), then sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The confirmation page's own JS and CSS requests sent the full link as `Referer`; the client's nginx log keeps `$http_referer` (it strips only the query of the request line), so the token reached the container log, and a proxy log likewise | **Fixed:** `<meta name="referrer" content="strict-origin">` first in `index.html` — requests of the page send the origin only; the E2E test asserts no request carries the token. `ng serve` injects `/@vite/client` before the meta tag (dev only; the test skips it) |
| R2 | `mat-checkbox` gets `aria-describedby=""` while no error shows, and no `aria-invalid` with the error (MatCheckbox exposes neither as optional) | **Won't fix:** the error is linked while shown and is in the focused summary; axe finds nothing |
| R3 | A reload of the confirmation page after the guard took the token shows „Link nicht mehr gültig“ | **Won't fix:** keeping it needs browser storage; the address is confirmed already, sign-in works (noted in SPEC §8.8 v2.22) |
| R4 | A second link opened on the confirmation page updates H1 and text but not the page title (same path for `PageTitleStrategy`) | **Won't fix:** a mail link opens a new page load; the in-page case is an edge |
| R5 | When the session probe failed, the confirmation answers 400 `antiforgery` and the page shows the S80 server text | **Won't fix:** correct („try again later“); the link stays valid |
| R6 | The client minimum (environment) can drift from the server's `Account:Password:MinLength` | **Won't fix:** same pattern as `priorityWeight` (O-S4-58); the server decides and its message shows at the field |
| R8 | CI (first run of the PR): no mail reached Mailpit in the E2E stack — `docker-compose.e2e.yml` left `mailpit` on the default network while the API is only on `dokploy-network` (since S5-11; nothing sent mail in E2E before), so every send failed with a `SocketException` and the registration still answered 204 (decision S5-12 4 (a)). The local run had its own Mailpit | **Fixed:** `mailpit` joins `dokploy-network` in the E2E override, as in the dev override |
| R7 | Legacy uppercase `AUTH.*` keys of the deleted pages and the now unused `models/login.model.ts` stay | **Follow-up:** S5-10 (legacy auth cleanup) |

Security: no secret in code; the token never leaves memory after the guard (not in storage, router state, history or Referer); the confirmation request is antiforgery-validated; the removed legacy endpoints shrink the anonymous surface and SEC-02's list. Correctness: concurrent submits are guarded by `busy`; the resolver decrements the loading counter in `finally`. Accessibility: role/name queries in every test, axe in component tests and in both appearances in E2E, focus to the success heading and the summary, announcements only through `A11yAnnouncerService`.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror` (SDK 10.0.404) | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` | 1060 passed (two legacy sign-up cases removed) |
| Client `npx ng test` | 63 files, 576 passed (before R1; R1 changed only `index.html`) |
| Client `npm run lint`, `tsc`, `npm run build:release` | clean; initial 728.5 kB (budget 850 kB) |
| e2e `npm run typecheck`, `npm run lint` | clean |
| E2E against the branch (`dotnet run` API + throwaway PostgreSQL/Mailpit, `ng serve`) | `register.spec.ts`, `reflow.spec.ts`, `login.spec.ts` in Chromium, Firefox, mobile Chrome: 96 passed; whole suite in Chromium: 95 passed (after R1 again: register 18, whole Chromium 95). WebKit only in CI |
| `node scripts/check-i18n.mjs` | 0 errors in all three targets |
| `node scripts/check-invariants.mjs` (TESTING_v61) | passed |
| `node scripts/check-figma-links.mjs` | 6 checked, 0 pending |
| `node scripts/check-icons.mjs` (after removing `app_registration`, subset regenerated) | 0 errors |
| Self-tests of all six scripts | passed |
| `reuse lint` (Docker) | compliant |
| `check-api-notices.mjs` | not run: no NuGet package changed |

## 5. Open points

1. **S5-16 (first gated screen):** attach `ConfirmedEmail` to „Organisation anlegen“, place `pa-confirm-required-notice` where the 403 `email-unconfirmed` arrives (mark the request so the interceptor's snackbar `auth.confirm.required` does not show as well), and add SEC-10's E2E part (unconfirmed → notice → resend → confirm from Mailpit → action succeeds).
2. **S5-14:** extend `register.spec.ts` into the account journey with sign-in and sign-out and the seeded users; `support/mailpit.ts` is ready.
3. **S5-10:** the legacy uppercase `AUTH.*` keys, `models/login.model.ts`, the unrouted legacy login page and `showRegister()`; the remaining legacy `forgot-password`/`reset-password` with S5-13.
4. **From S5-12a, still open:** token cleanup (R5 there, with S5-13); #318 sign-in rate limit; the owner's dev `.env` `SAMPLE_USER_PASSWORD` needs 12 characters.

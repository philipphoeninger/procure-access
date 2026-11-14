# S5-13b — Password reset S03 and the lockout note, client part

> **Sprint:** 05 · **Task:** S5-13b (second half of S5-13, 2.5 points together with S5-13a) · **Issues:** #319 (closed by this task)
> **Requirements / invariants:** SPEC R-AUTH-03, R-AUTH-04, §8.8 (v2.24); ARCHITECTURE C72, §8.3 (C142); SEC-02 (exemption list now only the export, TESTING_v63 V200), A11Y-09 (S03 joins the reflow check, V201), A11Y-01, A11Y-03, A11Y-08 on the new screens (V202); no invariant changes its status
> **Status:** done — pull request #336 open (2026-11-14), stacked on #335 (S5-13a)
> **Scope:** S03 at `/passwort` (request) and `/passwort/neu` (new password from the mail's link); „Passwort vergessen?“ as a link; the fixed lockout note in S01's error summary; UX copy v29; the legacy reset removed on server and client; S03 in `reflow.spec.ts`; an E2E reset through Mailpit.
> **Owner decisions (2026-11-14):** S5-13 1 (a) — one server answer for a wrong password and a locked account, a fixed lockout note on S01 · 4 (a) — the legacy reset goes with S5-13 · wording of the S03 keys and the lockout note confirmed (UX copy v29, U120–U124) · S5-13b 1 (a) — „Passwort vergessen?“ opens S03 in a new tab from the login overlay, in place on S01 · S5-13b 2 (a) — the lockout note is an optional note of the error summary, after its list, not counted

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | „Passwort vergessen?“ opened the legacy dialog, which posted to the legacy `/api/forgot-password`; its link led to the legacy reset page with the address in the URL | S03 and the new endpoints of S5-13a; the legacy code removed (decision 4 (a)) |
| 2 | The UX copy had no page title, no button for the second step, no heading after the change, no H1 and action for an invalid link, and `auth.login.locked` assumed the server tells a lockout apart | UX copy v29 with the confirmed wording; `auth.login.locked` removed |
| 3 | `pa-error-summary` counts every line in its title; the lockout note is not an error | Optional `note` input (decision S5-13b 2 (a)) |
| 4 | `PageTitleStrategy.updateTitle` returns early for a navigation to the same path, so reloading `/passwort/neu` after a refused token would change neither title nor focus nor announcement | `changePageInPlace(key)` instead of a reload |
| 5 | The unrouted legacy login page (`features/identity/login/`) still opened the forgot-password dialog, which this task deletes | Deleted with it (it was due with S5-10, O-S5-4) |
| 6 | The removed legacy files were the last users of the icon `lock` | Icon list and subset regenerated (`scripts/subset-material-symbols.sh`, 35 icons) |
| 7 | The root disk had 1.6 GB free: too little for the E2E stack | E2E run against a throwaway Postgres and Mailpit, the branch's API on port 5100 and `ng serve` on 4300 (§4) |

## 2. Changes

### 2.1 S03

- **Request step** `ResetRequestPage` (`features/identity/pages/reset-password/`) at `/passwort` (`RESET_PASSWORD_PATH`), in place of the page; form card like S02 (no Figma frame, Handoff §5.3): H1, intro, `common.requiredNote`, the address labelled `auth.login.email` (the same text and key as S01), „Link anfordern“ with the button spinner. Checked on submit; server field errors at the field, `antiforgery` and `rate-limit` in the error summary (`RESET_REQUEST_PROBLEMS`). Success replaces the form with `auth.reset.sent` (address and the answer's `validHours`), focused.
- **New-password step** at `/passwort/neu` (`NEW_PASSWORD_PATH`), lazy child routes `NEW_PASSWORD_ROUTES` like the confirmation page: `newPasswordTokenGuard` puts the token into `NewPasswordState` (memory only) and redirects to the path without it (`replaceUrl`); `newPasswordViewResolver` and `newPasswordTitleResolver` give the view (form with token / done / invalid) and `pageTitle.reset` / `.invalid`. `NewPasswordPage`: H1, „Neues Passwort“ with the hint `auth.register.password.hint` and „Passwort anzeigen“, „Passwort speichern“; checked on submit (required, 🔧 minimum, 128). 204 → focused H2 `auth.reset.done.title`, `auth.reset.done`, „Zur Anmeldung“ (S01 over the start page, a parsed `UrlTree`, because `routerLink` would encode the outlet string); 400 at `newPassword` → the field; 400 at `token` → „Link nicht mehr gültig“ with „Neuen Link anfordern“ (`/passwort`) through `changePageInPlace`; no token in memory (a reload) → the same. A second link opened on the page empties the field and gives the new view its title, focus and announcement.
- Both steps share `pages/reset-password-pages.scss`. `AuthService.requestPasswordReset` and `completePasswordReset` (`CALLER_HANDLES_PROBLEMS`).

### 2.2 S01 and the overlay

- `pa-login-form`: „Passwort vergessen?“ is a link — in place on S01 (`['/', { outlets: { primary: ['passwort'], login: null } }]`), in a new tab with `common.newTab` from the overlay (`forgotInNewTab`, set by `pa-login-overlay`).
- Every refusal at `$` adds `auth.login.error.lockout` with `attempts`/`minutes` from `LOCKOUT_RULE` (`models/lockout-rule.ts`, client setting `lockout: { maxFailedAttempts: 5, minutes: 15 }` 🔧 in both environment files, kept in step with `Account:Lockout:*`).
- `pa-error-summary` has an optional `note` (`TextKey`), shown after the list inside the notice.

### 2.3 Shared

- `PageTitleStrategy.changePageInPlace(pageKey)`: title of the key without the page's title state, H1 focus after the next render, announcement — for a page that shows another page at the same address.
- Routes: `/passwort`, `/passwort/neu` (lazy), `/reset-password` → `/passwort`.
- UX copy v29 (v27 deleted): U120–U124; `auth.reset` an `AREAS` entry of `check-i18n.mjs`; `de.json` through `--write`, `auth.login.locked` deleted.

### 2.4 Legacy removed (decision 4 (a))

Server: `POST /api/forgot-password` and `/api/reset-password` (`IdentityUserEndpoints` keeps only `GET /api/me`), `UserService.RequestPasswordResetAsync`/`ResetPasswordAsync`/`AppLink` and the dependencies only they used, `IUserService` entries, `ForgotPWRequest`, `ResetPasswordDto`, the `SERVICES` project reference to `Platform.Contracts`, `LegacyAccountEmailTests`. SEC-02's exemption list holds only the export (`ExemptEndpoints_AreExactlyTheAnonymousExport`). Client: the forgot-password dialog, the legacy reset page and model and its `LEGACY_EXCEPTIONS` entry, `AuthService.forgotPassword`/`resetPassword`, the unrouted legacy login page, the icon `lock`. The legacy uppercase `de.json` keys of these screens stay for S5-10 (dead legacy error keys).

### 2.5 Tests

- Component: `reset-request-page.spec.ts` (7), `new-password.routing.spec.ts` (9: token out of the address, form, checks, save, server field error, refused token with title/focus/announcement, antiforgery, no token, a second link), `login-form.spec.ts` (lockout note in the summary, count 1; both link variants), `login-overlay.spec.ts`, `page-title.strategy.spec.ts` (`changePageInPlace`). 594 client tests.
- E2E: `e2e/identity/password-reset.spec.ts` (journey through the Mailpit mail with axe on five states, unknown address, empty request, lockout note); `reflow.spec.ts` adds S03 (four states plus „Link nicht mehr gültig“).
- Backend: `AntiforgeryValidationTests` list; `LegacyAccountEmailTests` deleted. 1099 tests.

### 2.6 Documents

UX copy v29, SPEC v2.24, ARCHITECTURE_v56 C142 (v54 deleted), TESTING_v63 V200–V202 (v61 deleted — not planned: the SEC-02 row named the removed endpoints), CLAUDE.md §3, §4, §5. Figma mapping unchanged (S03 has no frame).

## 3. Self-review

| # | Finding | Sorting |
|---|---|---|
| R1 | After „Link nicht mehr gültig“ shown in place, a second mail link opened on the page showed the form under the old title and with the old input | **fix** — the page reacts to a new view from the resolver |
| R2 | A reload of `/passwort/neu` shows „Link nicht mehr gültig“ although the mail's link may still work | **won't fix** — the token is in memory only by design, as on the confirmation page; the text asks for a new link, and the old one still works from the mail |
| R3 | The legacy uppercase `de.json` keys of the removed screens are left | **won't fix here** — S5-10 removes the dead legacy keys |
| R4 | Deleting the unrouted legacy login page is outside the row | **kept** — it imported the deleted dialog; O-S5-4 had it due anyway |
| R5 | No N+1 or security change on the server beyond removals; the client never stores the token outside memory and never sends it before „Passwort speichern“ | — |

## 4. Checks

- Client: `npm run lint` clean; `npx ng test` 594 passed; `npm run build:release` within budget (initial 729.2 kB).
- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1099 passed.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons` passed; e2e typecheck and lint clean; `reuse lint` compliant.
- E2E locally (Chromium, Firefox, mobile Chrome; throwaway Postgres and Mailpit, the branch's API on 5100, `ng serve` on 4300): `identity/` and `reflow.spec.ts` 119 of 120. Firefox's content process crashed now and then in the reset journey with four workers (3–4 of 24 repeats, at different steps after its fourth axe run), never with one worker (6 of 6); `register.spec.ts` crashed the same way once with two workers. So the crashes come from local Firefox under load on the dev build, not from S03; CI (release build, one job per browser project) is the authority. WebKit and mobile Safari only in CI.

## 5. Open points

1. If Firefox in CI shows the crash of §4, split the reset journey into two tests with fewer axe runs per page.
2. S5-10: the legacy uppercase keys of the removed screens, the remaining legacy account endpoints.
3. S5-14: the account journey (registration, confirmation, reset, sign-in, sign-out) on the E2E users.

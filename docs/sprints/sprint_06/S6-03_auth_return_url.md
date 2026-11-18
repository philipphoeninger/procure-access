# S6-03 — Return to the target page after the sign-in

> **Sprint:** 06 · **Task:** S6-03 (0.5 points) · **Issues:** #386 (TECH_DEBT_v5 TD-65; S5-09 P3, S5-28b P3)
> **Requirements / invariants:** R-AUTH-03; SEC-03 stays green (the login overlay after a 401 is unchanged); no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-18)
> **Scope:** exactly the row of S6-03. Client only: no EF migration, no backend change, no new dependency, no UX copy key, no screen change (S01 has no Figma frame and looks the same). E2E was not run locally, because another session used the local E2E stack at the same time; the new E2E case runs in CI.
> **Owner decisions:** none needed. The plan below was approved in the session (2026-11-18, „Go ahead“).

## 0. Plan (written before the code)

- **Starting point:** `main` at 84bfac1 (after S6-27, S6-28).
- **Approach:** `authGuard` keeps sending a signed-out user to S01 over the start page (S01 cannot open over the guarded page: the guard would run again and the page would load signed out), but as a `RedirectCommand` whose navigation state carries `returnUrl` — memory only, never the URL or browser storage (R-SET-02). S01 reads it like the `save` reason, checks it with `safeReturnUrl` and goes there after the sign-in; without a valid target it closes the outlet as before, so „Liste speichern“ (R-SAV-01) is unchanged.
- **Files:** `core/guards/auth.guard.ts` (+ new spec), `features/identity/models/login-navigation.ts`, new `features/identity/models/return-url.ts` (+ spec), `features/identity/pages/login/login-page.ts` (+ spec), `e2e/identity/login.spec.ts`; ARCHITECTURE v84 (C170), CODEBASE §2.90 and §3 (renumbered to v85 C171 and §2.92 at the merge, §2 item 6).
- **Tests:** a unit test per refused form (absolute URL, `//host`, backslashes, `javascript:`, control characters, the `login` outlet, non-strings), the guard's redirect and state, S01's return and its refusal; one E2E case for `/konto`.
- **Invariants:** none activated. **Migration:** none. One session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `authGuard` returned `createUrlTree(['', { outlets: { login: [LOGIN_PATH] } }])`; the page the user opened was forgotten, and after the sign-in S01 only closed the outlet, so the user stayed on `/` (#386) | Keep the target in the router state of the redirect |
| F2 | S01 already reads its reason `save` from `currentNavigation().extras.state` (S5-09), never from the URL | The target travels the same way (`LoginNavigationState.returnUrl`) |
| F3 | Opening S01 over the guarded page itself would loop: the guard runs for the primary route of every navigation, and the page would load its data signed out | S01 stays over the start page; S01 navigates to the target after the sign-in |
| F4 | Found by the guard's unit test: when the redirect activates the `login` route before the outlet exists (the first navigation of a deep link, before the shell is rendered), the outlet creates S01 after the navigation has ended and `currentNavigation()` is `null` — the state, and with it `save` too, would be lost | S01 reads `currentNavigation() ?? lastSuccessfulNavigation()`; a unit test with the real page fails without the fallback (checked by removing it) |

## 2. Changes

1. **Guard** (`core/guards/auth.guard.ts`): for a signed-out user `new RedirectCommand(<'/(login:anmelden)'>, { state: { returnUrl: state.url } })`. A signed-in user passes as before.
2. **Router state** (`login-navigation.ts`): `LoginNavigationState.returnUrl?: string`, documented as untrusted until `safeReturnUrl` accepts it.
3. **Open-redirect check** (`features/identity/models/return-url.ts`, without imports): `safeReturnUrl(value, origin)` returns the value only when it is a string that starts with `/`, contains no `//`, no backslash (`/\host` is `//host` to a browser), no control character or whitespace (browsers drop tab and newline, so `/\t/host` would become `//host`), no `(login:` outlet (a sign-in loop), and `new URL(value, origin).origin === origin`; otherwise `null`. Absolute URLs, `//host` and `javascript:` fail the first rule. The router cannot leave the origin with `navigateByUrl` anyway; the check is defence in depth against a state that does not come from the guard (e.g. one restored from the history entry).
4. **S01** (`login-page.ts`): reads the navigation state once (F4), keeps `reasonSave`, computes `returnUrl` through `safeReturnUrl` with `DOCUMENT.location.origin`. After the sign-in a valid target is opened with `navigateByUrl` — its URL has no `login` outlet, so the outlet closes; page title, H1 focus and the announcement follow from the route change (`PageTitleStrategy`, A11Y-03). Without a target the old path runs unchanged (draft kept, `auth.login.draftKept`).
5. **Tests:**
   - `return-url.spec.ts` (28 cases): 4 accepted paths; refused: absolute `https`/`http` URLs (also to this site), `//host`, `///host`, `/\host`, `\\host`, a backslash in the path, `javascript:` (also mixed case and behind spaces), `data:`, tab/newline/space between the slashes, `//` later in the path, `/(login:anmelden)`, `/konto(login:anmelden)`, `konto`, `''`, `undefined`, `null`, a number, an object.
   - `auth.guard.spec.ts` (4): a signed-in user passes; the `RedirectCommand` to `/(login:anmelden)` with `{ returnUrl: '/konto?tab=daten' }`; the state at the `login` outlet with the shell open and on a deep link (outlet created after the navigation).
   - `login-page.spec.ts` (+6): back to `/konto?ansicht=daten` after the sign-in, no draft handling; four refused targets (`https://evil.example/konto`, `//evil.example`, `/\evil.example`, `javascript:alert(1)`) only close the outlet; the deep-link case with the real S01.
   - E2E `e2e/identity/login.spec.ts`: signed out, `/konto` → S01 over the start page (`/(login:anmelden)`, the target not in the address) → sign-in by keyboard as the seeded confirmed user (correct password, nothing changed) → `/konto`, H1 „Konto“ focused, title „Konto – ProcureAccess“, the menu „Konto“ in the header.
6. **Merge with `main` (twice):** S6-10b merged first and took ARCHITECTURE v84 C170 and CODEBASE §2.90, then S6-08 took CODEBASE §2.91, so `main` was merged into the branch each time (no rebase, no force-push): this task's change is ARCHITECTURE v85 C171 and CODEBASE §2.92; only ARCHITECTURE and CODEBASE conflicted.
7. **Documents:** ARCHITECTURE v85 (C171 and the as-built sentence in §8.3; v83 deleted), CODEBASE §2.92 and the §3 layout line of `features/identity`. No change to SPEC (R-AUTH-03 is met as written), TESTING (no status change), UX copy, Handoff, Figma mapping, `LEGACY_EXCEPTIONS` or `CLAUDE.md` (no rule that applies across the codebase).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: the target is never read from the URL; it comes from the guard's own `state.url` and is checked before use (open redirect: absolute URLs, `//host`, backslashes, `javascript:`, control characters). Nothing is written to browser storage (R-SET-02); Angular keeps the navigation state in the history entry, as for the existing `save` reason — it holds only a path the user opened. N+1: none (client only). Accessibility: the return is a route change, so title, H1 focus and announcement work as on every page (E2E checks focus and title); no new text.

| Finding | Sorting | Reason / fix |
|---|---|---|
| The state was lost when S01 was created after the navigation (deep link before the shell exists, F4) | fix | `?? lastSuccessfulNavigation()`; test with the real page |
| Lint: unnecessary type assertion in the guard spec | fix | removed |
| `safeReturnUrl` refuses a target whose fragment contains `//` | won't fix | The router encodes `/` in query values; a fragment with `//` is not used by any guarded page. The user then stays on the start page, as before S6-03 |
| S01's link „Noch kein Konto? Jetzt registrieren“ and the registration with its e-mail confirmation drop the target | follow-up | Carrying it through the confirmation link would be a feature of its own (open point P1) |
| The `lastSuccessfulNavigation()` fallback could in theory hand S01 the state of an unrelated earlier navigation | won't fix | S01 is only created by a navigation to the `login` outlet, so the last successful navigation is the one that opened it; a stale state would only hold a checked same-origin path |

No correctness or security bug that affects current behaviour is left: merge recommended after CI.

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test --watch=false` | 89 files, 896 tests passed (before the lint fix; the guard spec again after it: 4 passed) |
| `npm run lint` (client) | all files pass |
| `npm run build:release` | builds; initial total 756.88 kB; the critical-CSS warning is the same on `main` |
| `npm run typecheck`, `npm run lint` (e2e/, scripts/) | pass |
| `node scripts/check-invariants.mjs` | Invariant gate passed |
| `node scripts/check-figma-links.mjs` | 7 checked, 19 not built yet, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors (client, export and e-mail texts; UX copy v40) |
| `node scripts/check-icons.mjs` | 0 errors |
| `reuse lint` (`fsfe/reuse:6.2.0`, the CI version) | no finding in a repository file; the container sees no git (worktree), so it also lists the untracked `node_modules/`, `dist/` and `.env`, which CI does not have |
| E2E | not run locally — another session used the local E2E stack; left to CI |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | S01's link to S02 and the registration (with the e-mail confirmation in another tab) drop the return target | Follow-up issue if the pilot asks for it; the confirmed user signs in from the e-mail link's page |
| P2 | TD-65 is resolved; TECH_DEBT_v6 records it at the sprint end | S6-09 |
| P3 | The headers of TESTING_v91 and SPEC_v2 still name ARCHITECTURE_v84 | S6-09 (as S6-27 P4) |

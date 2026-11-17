# S5-30 — Session warning dialog and the completed login overlay (M4.2)

> **Sprint:** 05 · **Task:** S5-30 (stretch, 3 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-AUTH-06, R-AUTH-07 (completion), §8.8 (v2.48); **A11Y-04** → `active` (TESTING_v88 V297 – V301); J5 built; ARCHITECTURE C166; UX copy v39 U160; Handoff 1.30; ROADMAP M4.2
> **Status:** done — pull request #373 open (2026-11-17)
> **Scope:** the warning `Session:WarningLead` 🔧 before the earlier of the idle and the absolute limit (alert dialog, „Angemeldet bleiben“ on `POST /auth/session/extend`; before the absolute limit the request to save and sign in again); the login overlay at the limit and its completion after a 401; J5 on a movable clock of the E2E stack. No EF migration, no new dependency.
> **Owner decisions (2026-11-17):** before the task — **D1 (b)** at the limit the client asks the probe; an ended session opens the login overlay in place of the warning, a session another tab extended is planned again · **D2 (a)** before the absolute limit „Schließen“ (focus) and „Jetzt abmelden“ · **D3 (b)** another account signing in through the overlay: no retry, the in-memory data of the previous user goes as after a sign-out, the start page with the focus on its H1 · **D4 (a)** an E2E clock of our own (real time plus an offset), Development/E2E only with a switch, no shipped `FakeTimeProvider`; at the plan — **Q1 (a)** UX copy v39 adds the singular texts `shell.session.warning.body.one` and `.absolute.one` · **Q2 (a)** the offset applies only to requests that carry the cookie `pa-test-clock`, so J5 runs in parallel with the other tests and no `POST /e2e/clock/advance` exists. **Assumptions confirmed:** the client compares the server's times with the browser's clock (no correction); Esc closes the warning without extending; the minutes change at most once a minute without a new announcement.

## 1. Starting point

- S5-29 served everything the warning needs: the two limits from the probe and the expiry headers (`sessionExpiresAt`, `sessionAbsoluteExpiresAt` of `withIdentity()`), `sessionWarningLeadSeconds`, and `POST /auth/session/extend`. Nothing showed them.
- The login overlay (S5-08, S5-09) showed `shell.session.expired.*`, retried the held request and announced `shell.session.restored`. Its E2E tests faked the 401 for an *anonymous* user. Closed by a signed-in user, the store still held the session, so the header kept „Konto“.
- All `shell.session.*` keys were in `de.json`; the singular of the two texts with `{{minutes}}` was missing (Q1).
- TESTING planned J5 with a `FakeTimeProvider` moved by `POST /e2e/clock/advance`. The E2E stack runs in `Development` (from `.env`), and locally five browser projects share one stack (Q2).

## 2. Changes

### 2.1 Client

- **`core/session/session-warning.service.ts`** — `SessionWarningService`, started once by `App`. An effect plans, from the two limits and the lead time, a timer at the warning time and one at the earlier limit. At the warning time it calls `refreshSession()` and opens the dialog for a session that is still there; an answer that moves the limit out of the warning time closes it. At the limit it asks again: an ended session closes the warning and opens the login overlay (D1 (b)), once per ended session. A limit already passed when planned shows nothing (no probe loop with a wrong clock).
- **`features/identity/dialogs/session-warning-dialog/`** — `SessionWarningDialog`: `MatDialog`, `role: 'alertdialog'`, described by its text; `.body` with „Jetzt abmelden“ and „Angemeldet bleiben“ (focus), or `.absolute` with „Jetzt abmelden“ and „Schließen“ (focus) (D2 (a)); `translatePlural` with the minutes; „Angemeldet bleiben“ extends, announces `shell.session.extended`, closes; a failed extension keeps it. Loaded with its first use.
- **`withIdentity().refreshSession()`** — the probe again; unlike `loadSession()` a failed probe leaves the state and answers `unknown` (offline is not signed out). `AuthService.extendSession()`.
- **Login overlay** — `LoginOverlay.open()` is async: it takes the previous user, calls the probe for a new antiforgery token *without applying the answer* and opens the dialog with `{ previousUserId }`. Same account (or nobody before): `restored` and the retry; another account (D3 (b)): no retry, `SignOutService.anotherUserSignedIn()` → `clearUserData()` and the start page. Closed without a sign-in: `refreshSession()`, so the header shows the user signed out. `LoginOverlayService.requestSignIn(previousUserId?)`.
- **`features/identity/services/sign-out.service.ts`** — `SignOutService`: „Abmelden“ of `MainNav` and „Jetzt abmelden“ of the warning end the session and show the start page with the focus on its H1; the root store's `forgetUser()` is split into `forgetSession()` + `clearUserData()`.
- `testing/shell-test-doubles.ts`: the store double has no limits, so the warning plans nothing in shell tests.

### 2.2 Server (host only)

- **`API/API.ProcureAccess/Extensions/TestClock.cs`, `TestClockExtensions.cs`** — with `TestClock:Enabled` in Development or E2E `TimeProvider` is a `TestClock` over the real clock that adds the offset of the request's cookie `pa-test-clock` (whole seconds, 0 – 172 800; anything else counts as 0). The first middleware stores it in `HttpContext.Items`; the clock reads it through `IHttpContextAccessor`, so the cookie handler's callbacks at the start of the response read the same time. Staging and Production never register it, whatever the configuration says. `docker-compose.e2e.yml` sets `TestClock__Enabled`.

### 2.3 E2E

- **`e2e/support/clock.ts`** — `SessionClock`: `install(page)` before the first `goto`, `move(minutes)` moves the browser's `page.clock` and the cookie together, `moveApi(minutes)` only the API's time.
- **`e2e/journeys/j5-session.spec.ts`** — J5 with an account per test on S60: the warning with axe in both appearances and „Angemeldet bleiben“; the overlay replacing the warning at the idle limit, the choice kept; a save after the API's time passed the limit (401 → overlay → retry); the absolute warning after 19 extensions; another account; „Schließen“ in the overlay.
- `shell/reflow.spec.ts` — the session warning at 320 px.

### 2.4 Documents

UX copy v39 (U160; v37 deleted), TESTING v88 (A11Y-04 `active`, V297 – V301; v86 deleted), ARCHITECTURE v80 (C166, §8.3, step 4; v78 deleted), SPEC v2.48 (§8.8, rows v2.48-1 – 3), Handoff 1.30 (§5.3 two rows), CODEBASE §2.81 and §3, CLAUDE.md §3 (state, session rule). The Figma mapping is unchanged: the warning has no frame (Handoff H4).

## 3. Self-review

CLAUDE §6.7 checklist and `/engineering:code-review` over the diff; findings sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | J5 found it: after a real expiry the sign-in in the overlay was refused with `antiforgery` — the token cookie belonged to the ended session, and the earlier overlay test faked the 401 for an anonymous user | **fixed** — the overlay calls the probe for a new token before it opens; unit test in `login-overlay.spec.ts` |
| R2 | The first version of that fix applied the probe's answer: the store became signed out under the overlay, the settings fell back to the system, and the sign-in read the stored settings over the user's unsaved choice | **fixed** — the probe's answer is not applied; the state changes only with the sign-in or when the overlay is closed |
| R3 | Firefox: when the page's clock jumps past both times, the warning and the limit timer both find the session ended, and the slower one opened a second overlay after the first was closed | **fixed** — one overlay request per ended session; unit test with both probes in flight |
| R4 | The dev-only API watcher `src/dev/dev-reload-health.ts` fetches `/health/live` every 5 s with the session cookie; the dev server forwards `/health`, so the session renews itself and the warning never shows under `npm start` (nginx forwards only `/api/`, so the E2E stack and production are not affected). Found while J5 ran against the dev server | **follow-up issue** — proposed: `credentials: 'omit'` in the watcher's `fetch` (outside this row) |
| R5 | After the sign-in in the overlay the settings read announces „Wird geladen …“ right after `shell.session.restored`; a screen reader may not finish the first. Existed before (S5-25b reads the settings with each sign-in) | **follow-up issue** — J5 records every announcement and checks that `restored` was made |
| R6 | The client compares the server's times with the browser's clock; a clock several minutes off moves the warning | **won't fix** (owner, assumption confirmed) — open point 2; correcting it needs the server's time in the contract |
| R7 | Another tab signing in as a *different* user: the warning's probe takes that user's session into this tab's store without the D3 handling | **follow-up issue** (low; an edge case of shared devices) — open point 3 |
| R8 | D3: the held request's 401 is passed back to its caller before the start page opens; a caller may show its failure for a moment | **won't fix** — the page is left at once; the error interceptor stays silent on 401 |
| R9 | Security of the test clock: a cookie that moves the server's time | **ok** — registered only in Development and E2E with the switch, which only `docker-compose.e2e.yml` sets; `TestClockGuardTests` prove the absence in Testing, Staging and Production with the switch on; the offset is bounded and parsed without sign |
| R10 | N+1, unbounded work | **ok** — one probe before the warning and one at the limit; no query changed |
| R11 | Accessibility | **ok** — alert dialog described by its text, first focus on the action, focus back after closing, the countdown not re-announced, actions in DOM order and stacked below 600 px, axe in both appearances, reflow at 320 px |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings |
| `dotnet format --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` | 1682 passed (11 new in `TestClockGuardTests`) |
| `npx ng test` | 810 passed (new: `session-warning.service.spec.ts` 12, `session-warning-dialog.spec.ts` 6, `sign-out.service.spec.ts` 2, overlay +7, identity store +3) |
| `npm run lint` (client), `npm run typecheck && npm run lint` (e2e) | clean |
| E2E on the local stack (Chromium, Firefox, mobile Chrome) | full suite 425 passed, 1 skipped; J5 90 / 90 with `--repeat-each 5`; WebKit and mobile Safari in CI only |
| `node scripts/check-invariants.mjs` | passed (A11Y-04 active) |
| `node scripts/check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | passed |
| `reuse lint` | compliant |

## 5. Open points

1. **Dev watcher renews the session (R4)** — proposed issue: `credentials: 'omit'` in `src/dev/dev-reload-health.ts`; until then the warning can only be seen on the E2E stack (CLAUDE §3 says so).
2. **Clock skew (R6)** — the warning follows the browser's clock. If it matters later: the server sends its time with the expiry headers and the client keeps the difference.
3. **Another user signed in from another tab (R7)** — the warning's probe could apply D3 there too.
4. **`restored` followed by „Wird geladen …“ (R5)** — e.g. no loading announcement for a read the user did not start, or the announcement after the settings read.
5. The task fitted one session, including the two bugs J5 found in the 401 path (R1, R3).

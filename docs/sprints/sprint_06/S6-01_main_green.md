# S6-01 — Make `main` green: J5 session warnings and the J1 download flake

> **Sprint:** 06 · **Task:** S6-01 (1 point) · **Issues:** #379 (TD-55), #380 (TD-56)
> **Requirements / invariants:** SPEC R-AUTH-05, R-AUTH-06 (§8.8 probe contract unchanged); SEC-07, SEC-08, A11Y-01, A11Y-02 stay green; no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-18); "done" of the sprint row (three consecutive green CI runs on `main`) follows after the merge
> **Scope:** the cause of each red E2E run on `main` since S5-30 and the J1 download timeout; a real double-open of the session warning fixed on the server with tests that failed before the fix; the two E2E races fixed in the tests. No EF migration, no new dependency, no UI text, no client code.
> **Owner decisions (2026-11-18, at the plan):** **(1)** server fix only, no second guard in `SessionWarningService` · **(2)** bound J1's download waits to 30 s · **(3)** the mobile-Safari axe flake of run 44110586418 is only proposed as an issue (§5).

## 1. Starting point

Four CI runs on `main` failed in E2E; the issues named three of them.

| Run | Job | Test | Cause (§2) |
|---|---|---|---|
| [44091357421](https://github.com/philipphoeninger/procure-access/actions/runs/44091357421) (merge of #373) | WebKit | `j5-session.spec.ts:152` — strict-mode violation, **two** `alertdialog` „Ihre Sitzung endet bald“ | **real defect**: the probe and the headers named the absolute limit differently (2.1) |
| [44097092697](https://github.com/philipphoeninger/procure-access/actions/runs/44097092697) (merge of #374) | Chromium | `j5-session.spec.ts:198` — the overlay still open after „Schließen“ | **test race**: Enter before the overlay's first focus (2.2) |
| [43937117139](https://github.com/philipphoeninger/procure-access/actions/runs/43937117139) (merge of #359) | mobile Chrome | `j1-configurator.spec.ts:415` — `waitForEvent('download')` for 5 minutes | **test race** of the same class, fixed since S5-25b (2.3) |
| [44110586418](https://github.com/philipphoeninger/procure-access/actions/runs/44110586418) (merge of #376) | mobile Safari | `identity/preferences.spec.ts:63` — axe contrast 1.07 on the home cards in dark | not in this row (§5, proposed issue) |

The analysis used the Playwright traces of the failed jobs (network log with bodies, action log, screenshot, page snapshot).

## 2. Changes

### 2.1 #379 WebKit: the warning opened, closed and opened again (fixed in the code)

**What the trace shows.** The first dialog has the classes `mdc-dialog--opening mdc-dialog--open mdc-dialog--closing`: it was closed within the frame it opened in, and a second one opened. This is neither two timers nor two subscriptions nor a second tab: it is `showWarning` closing the dialog because the warning's kind changed.

**Why the kind changed.** The probe answered

```json
"expiresAt": "2026-11-17T21:22:49+00:00", "absoluteExpiresAt": "2026-11-17T21:22:49.8209408+00:00"
```

while every expiry header named both limits as `21:22:49Z`. The cookie ticket stores its idle limit in whole seconds, the headers are formatted in whole seconds, but `SessionCookieEvents.AbsoluteExpiry` returned the sign-in time with its fraction (stored in format `O`) plus 8 hours. In the last 30 minutes the server caps the idle limit at the absolute limit, so:

1. the probe's body set `absolute (…49.82) > idle (…49)` → kind `idle` → the warning opened with „Angemeldet bleiben“;
2. the next answer (`/api/me`, 80 ms later) set both from the headers, equal → kind `absolute` → `showWarning` closed the dialog and opened the absolute one.

Users would see and hear it: the alert announced twice, the focus moved twice, for a moment an „Angemeldet bleiben“ that cannot extend. It came back with every probe in the last 30 minutes (`beforeWarning`, `atLimit`). The race in the test depended on whether the dialog's lazy chunk loaded before the next answer, which is why S5-30's 90 / 90 local runs passed.

**Fix (decision 1).** `AbsoluteExpiry` cuts the limit to the whole second, so the probe, the headers and the server's own check name the same instant; the capped idle limit now equals it. A session ends at most one second earlier than before, at the instant the client was told. The client needs no change: equal limits already mean kind `absolute`. `SessionResponse` documents the whole seconds.

**Tests (both failed before the fix):**
- `Tests.Unit/IdentityAccess/SessionCookieEventsTests.cs` (new): `AbsoluteExpiry_SignInWithAFractionOfASecond_IsTheWholeSecondOfTheLimit` (INV:SEC-07) and the null case.
- `Tests.Integration/IdentityAccess/CookieSessionTests.cs`: `SessionProbe_SignInWithinASecond_ReportsTheAbsoluteLimitAsTheHeaderDoes` (INV:SEC-07) — sign-in at `…:00.820` on the fake clock, extended every 25 minutes to 7 h 55 min as J5 does; the probe's `absoluteExpiresAt` equals the probe's and the extension's header, and `expiresAt` equals `absoluteExpiresAt`. Before the fix: `17:00:00.82` against `17:00:00`. The fixture clock starts on a whole second, which is why no earlier test saw the fraction.

Checked on the E2E stack: the traced J5 `:152` probe now answers `"expiresAt":"…07:36:00+00:00","absoluteExpiresAt":"…07:36:00+00:00"`.

### 2.2 #379 Chromium: „Schließen“ in the overlay sent the empty form (test fixed)

The screenshot shows the overlay with the error summary „2 Angaben müssen noch korrigiert werden“, and the trace has no fourth probe (the overlay's `onDestroy` reads the session again when it closes), so it never closed. `locator.press('Enter')` focuses „Schließen“ and then presses; `MatDialog` moves the first focus to the e-mail field when the open animation ends, and that happened in between — Enter submitted the form. A keyboard user cannot reach the button before the dialog has opened, so this is not a product defect.

`j5-session.spec.ts:198` now waits for the e-mail field's focus before pressing „Schließen“, as `signInInOverlay` does. The other dialog steps of J5 already wait for the first focus.

### 2.3 #380 J1 on mobile Chrome: no export request at all (cause named; wait bounded)

The trace has no `POST /api/v1/configurations/{id}/exports`: S13 never sent the export. The run's commit (197d9b3) still had the old `exportThroughS13`, which tabbed to „Datei herunterladen“ right after `toBeChecked()` on the radio; S13's first focus on the selected format came afterwards and took the focus back, so Enter landed on the radio. S5-25b (0065abf, PR #367, merged three hours after the failed run) added `await expect(xlsx).toBeFocused()` for exactly this ("CI of #362"); no run on `main` has failed there since. **The runner is not the cause**, so no retry and no longer timeout.

Decision 2: the two download waits of J1 get `DOWNLOAD_TIMEOUT` (30 s; the export answers in about a second, 0.6 s in CI), so a download that never starts fails in seconds with its own message instead of holding the test for 5 minutes.

### 2.4 Documents

- `CLAUDE.md` §3 E2E: in a dialog, wait for its first focus before the first key (the second occurrence of the race, after S13 in #362). Size 38 362 characters.
- `docs/CODEBASE_v1.md`: §2.80 (S5-29) the absolute limit in whole seconds and why; §2.81 (S5-30) the E2E point; §1 the change of `CLAUDE.md`. §3 unchanged (it does not list unit test files; no new folder).
- SPEC §8.8 names no precision, so the probe's contract is unchanged; ARCHITECTURE C165 describes the limit as "sign-in time plus the absolute limit", which stays true at whole-second precision: no new version of either.

## 3. Self-review

Checklist of CLAUDE §6.7 with the skill `engineering:code-review` on the diff:

| # | Finding | Sorting |
|---|---|---|
| R1 | The absolute limit ends up to 1 s earlier | **no change** — stricter, never longer, and the instant the client was told |
| R2 | The new integration test leaves the last extension response undisposed when an assertion inside the loop fails | **won't fix** — the test fails anyway; the host is disposed with the class |
| R3 | A real change of the next limit from idle to absolute while the warning is open still closes and reopens the dialog (by design of S5-30), so two `alertdialog`s exist for the close animation | **open point** (§5) — intended, J5 no longer reaches it |
| R4 | `account.spec.ts` and `s11-list.spec.ts` keep an unbounded download wait | **won't fix (scope)** — #380 is J1 |

No security or correctness finding that affects current behaviour; no N+1 or performance concern.

## 4. Checks

- `dotnet format ProcureAccess.sln --verify-no-changes` — clean.
- `dotnet build ProcureAccess.sln -warnaserror …` — 0 warnings, 0 errors.
- `dotnet test --solution ProcureAccess.sln` — **1685 / 1685 passed** (unit, architecture, integration on Testcontainers PostgreSQL 17).
- New tests before the fix: unit 1 of 2 failed (the fraction), integration failed on `absolute` (`…:00.82` vs `…:00`); after the fix green.
- E2E on the local E2E stack (branch build), WebKit and mobile Safari only in CI:
  - J5 `--repeat-each 5` on Chromium, Firefox, mobile Chrome: **90 / 90 passed**;
  - J1 `--repeat-each 2` on the same three: 10 passed, 2 skipped (Firefox `fixme` of the second part, #355);
  - the whole suite on Chromium: **145 / 145 passed**.
- `e2e`: `npm run typecheck`, `npm run lint` — clean. Client code unchanged (no Vitest or ESLint run needed).
- `node scripts/check-invariants.mjs` (gate passed), `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` — clean; `reuse lint` — compliant (1583 / 1583).
- Disk: the E2E build took the root partition to 97 %; dangling images pruned afterwards (back to 2.1 GB free, TD-74).

## 5. Open points

1. **Done criterion of the row:** three consecutive green CI runs on `main` after the merge; the run links go into the PR when they exist. #379 closes with the PR; #380 closes with it as well (cause named, wait bounded).
2. **Proposed issue — mobile-Safari axe flake on the home page in dark** (run 44110586418, `identity/preferences.spec.ts:63`, step „signed in: dark and reduced motion over the light system, checked by axe“): the home cards' text `#1c1b1f` (light scheme) on `#141317` (dark), contrast 1.07 — axe ran while the cards still had the light text colour after the switch to dark. Same class as #339 (axe during a transition on mobile Safari), different place. Proposal: wait in the test until the card text has the dark scheme's colour (or the shared "settled" wait of #339) before axe; check whether `mat-card` text colour transitions on appearance change with reduced motion. It may break the three green runs of item 1.
3. **R3:** when the next limit really changes from idle to absolute while the warning is open, the dialog closes and opens again (S5-30 design: the focus moves to the other action). Two alert dialogs exist for ~75 ms. Acceptable now; revisit with the manual A11Y-07 pass (#311) if the double announcement disturbs.
4. **#380 neighbours:** `e2e/identity/account.spec.ts:60` and `e2e/configurator/s11-list.spec.ts:313` wait for a download without a bound; touch them when those specs change.

# S3-22 — Snackbar buttons in German

> **Sprint:** 03, stretch task S3-22 (TECH_DEBT_v2 §3.1, 0.5 points). **Issues:** #160 (TD-15 / #181 snackbar row). **Requirements / invariants:** none (CLAUDE §6.5 „UI strings only by key“, WCAG 3.1.2); no invariant activated, no new TESTING version. **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: the buttons of `SnackbarService` read their text from `de.json`. **Not in scope:** whether a snackbar is still the right pattern for error messages (UX copy §4.1: „nie für Fehler“), the commented-out `dialog.service.ts`, the different positions of info and error snackbars.
> **Owner decisions** (2026-11-09, asked before the task):
> 1. **O-S3-9 (a) — one existing key for both buttons.** `showInfo` ('Close') and `showError` ('Dismiss') both use `common.actions.close` („Schließen“), which UX copy v16 §5.0 already defines; the English distinction has no meaning for users and no wording is invented. The snackbar row of the UX copy session #181 is struck as done, so S3-22 does not wait for that session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `core/services/snackbar.service.ts` passes `'Close'` (`showInfo`) and `'Dismiss'` (`showError`) as literals to `MatSnackBar.open`, so the German UI shows English buttons (#160) | Both read `common.actions.close` (O-S3-9 (a)) |
| F2 | The UX copy session #181 has not taken place; UX copy v16 has no snackbar-specific key, but `common.actions.close` = „Schließen“ (§5.0) is in `de.json`, and `common` is an `AREAS` root of `check-i18n.mjs` | No UX copy or `de.json` change; `check-i18n --write` changes nothing |
| F3 | The app does not wait for `de.json` at startup (no app initializer for ngx-translate) | The service uses `TranslateService.get` (like `A11yAnnouncerService`), not `instant`, so an early snackbar never shows the key |
| F4 | 18 production files call the service; the error interceptor (S2-11) passes its message already translated through `translate.get` | Public methods unchanged (`showInfo`, `showError`, `void`); callers and their tests (which mock the service) untouched |
| F5 | `core/services/dialog.service.ts` is entirely commented out and still contains `'Close'` / `'Dismiss'` | Dead code, not shipped; open point P1 |
| F6 | UX copy v16 §4.1 lists „Snackbar (nur für nicht kritische Bestätigungen, nie für Fehler)“, but `showError` is a snackbar for the error interceptor's generic texts; owner decision S2-17 (3A/4) keeps that until each screen is rebuilt | Not changed here; open point P2 (the issue asked to check this) |

## 2. Changes

| Area | Change |
|---|---|
| `core/services/snackbar.service.ts` | Injects `TranslateService`; `showInfo` and `showError` go through a private `open()` that fetches `common.actions.close` with `translate.get` and then opens the snackbar with that text (info keeps its top-centre config, error keeps Material's default). Once `de.json` is loaded, `get` emits synchronously, so the timing for callers is unchanged |
| `core/services/snackbar.service.spec.ts` (new) | Real `MatSnackBar` with `provideTestTranslations()`: for both methods the message is shown with a button named „Schließen“, no button named „Close“/„Dismiss“, and the button closes the snackbar. The test failed before the change (no „Schließen“ button) |
| Docs | This protocol; O-S3-9 in `SPRINT_03.md` (owner decisions) and `TASK_PROMPTS.md` (placeholders); CLAUDE.md header and §3 (snackbar button text) |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff. N+1: not applicable. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: the button text comes from the bundled `de.json`, rendered by Material as text (no HTML) | – |
| R2 | If `de.json` cannot be loaded, `translate.get` may not emit and the snackbar does not open. The error interceptor already fetches its message through `translate.get`, so the message has the same dependency; nothing new | Won't fix |
| R3 | `translate.get` completes after one value, so the subscription in the service does not leak | – |
| R4 | Two snackbars requested while `de.json` is still loading open in call order (both wait for the same load); Material shows one at a time either way, as before | – |
| R5 | a11y: the button name now matches the page language (WCAG 3.1.2); the snackbar does not close by itself (no `duration`), as Handoff §1.7 asks for important content | – |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npx ng test` (Node 24) | ✅ 34 files, 149 tests (new spec: 2 tests, red before the change) |
| 2 | `npm run lint` | ✅ all files pass (`snackbar.service.ts` is not in `LEGACY_EXCEPTIONS`) |
| 3 | `npm run build:release` | ✅ |
| 4 | `node scripts/check-i18n.mjs` and `--write` | ✅ 109 keys of `common, shell, errors, legal, home, suggest`, 0 errors; `--write` changes nothing |
| 5 | `node scripts/check-figma-links.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending) |
| 6 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v37.md`, unchanged) |
| 7 | `reuse lint` | ✅ compliant (1063 / 1063) |
| 8 | E2E | – not run locally: no E2E test opens a snackbar or names its button, and the shell is unchanged; CI runs all five browser projects |
| 9 | Backend | – not touched |
| 10 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | `core/services/dialog.service.ts` is entirely commented out (with the old English literals); delete it with the next change in `core/services/` or in the M3 rebuild | Follow-up, any time |
| P2 | UX copy §4.1: snackbars „nie für Fehler“ — `showError` is still the error interceptor's channel for non-5xx errors (S2-17, 3A/4). Each rebuilt screen shows its errors in place (Handoff §6.4); when the last legacy caller is gone, `showError` goes too | Screen rebuilds, steps 2–6 |
| P3 | The snackbar row of #181 is struck as done (O-S3-9); the remaining rows of the UX copy session stay open | Owner, #181 |

<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-27 — Reload flows: failed lazy chunk and release mismatch

> **Sprint:** 06, task S6-27. **Issues:** #296, #310 (TD-39). **Requirements / invariants:** none; no invariant activated, no new TESTING version; A11Y-01 stays green (axe on the new states).
> **Status:** done by Claude Code (2026-11-18) — pull request open; E2E left to CI (another session used the local E2E stack). No EF migration.
> **Scope:** exactly the sprint row: one guarded reload when a lazy route chunk fails after a deployment, then the error page (#296); „Kriterien neu laden“ in S13 after `release-mismatch` (#310, TD-39).
> **Owner decisions:** O-S6-5 (2026-11-18): (a) for #296 — one automatic reload per failed chunk, guarded against loops, then the error page; the S10 draft may be lost on the reload, no warning. #310 follows the confirmed UX copy `criteria.export.outdated` (selection kept, per-criterion changes dropped), not #310's proposal; new button `criteria.export.outdated.reload` „Kriterien neu laden“ (owner-confirmed wording). The run could not ask the owner; the plan below was followed without a stop.

## Plan (written before the code)

- **Starting point:** `main` at f5f303e. Since S4-22 every page is lazy; a failed `import()` cancels the navigation without a message. S13 shows `criteria.export.outdated` („Bitte laden Sie die Kriterien neu“) with „Schließen“ only, and the client keeps the old release pinned.
- **#296:** `provideRouter(routes, withNavigationErrorHandler(handleChunkLoadError))`. The handler (new `core/errors/chunk-reload.ts`) recognises a lost chunk by the `TypeError` messages of the three engines (one list, unit-tested); offline (`ConnectionStatus`) → nothing; else reload the target once. Guard: the target's **history entry state** (`{ paChunkReload: <router URL> }`, survives a reload) — no browser storage, nothing in the URL (owner rule: no persistent storage for signed-out users). The same target failing again in the marked entry → `RedirectCommand` to S80 `server` with `skipLocationChange`. The guard is a pure function `chunkFailureAction` plus an injectable `DocumentReload`, so both are unit-tested without a real reload.
- **#310:** S13 for the draft gets „Schließen“ (text) + „Kriterien neu laden“ (filled, focused) in the outdated state; it closes with `{ reload: true }`. New store method `reloadCriteriaListForActiveRelease()` in `withConfigurator()`: drop pinned release, options and decisions, keep the four-layer draft, load options without release (pins the active one), then the list. S11 focuses its H1 afterwards. A saved list keeps „Schließen“ only (its export never gets `release-mismatch`).
- **Texts:** `criteria.export.outdated.reload` is not in UX copy v39 → UX copy v40 with U161 (wording confirmed by the owner in O-S6-5), `de.json` through `check-i18n.mjs --write`. No other text needed (S80 and the offline notice exist).
- **Figma:** S13-E `2039:1944` checked through the Figma MCP before building: secondary text button left of the filled primary; the outdated state follows that pattern. No frame of its own (Handoff §5.3 dialog rules).
- **Tests:** unit — both guards (`chunk-reload.spec.ts`: messages, `chunkFailureAction`, `DocumentReload`, the handler in a router with a failing `loadComponent`; `with-configurator.spec.ts`: four reload cases), `export-dialog.spec.ts` (both buttons, the reload on S11). E2E (CI only) — `e2e/shell/chunk-reload.spec.ts` (404 once → one reload and the page; always → one reload and S80) and an S13 case in `e2e/configurator/s11-list.spec.ts`.
- **Documents:** CODEBASE §2.86 and §3, CLAUDE.md §3 (client routing names the handler, as #296 asks), Handoff 1.34 (S13 row), ARCHITECTURE v81 C167 (v79 deleted), UX copy v40 (v38 deleted).
- **Invariants / migration / dependencies:** none activated; no EF migration; no new dependency. One session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Angular 22.2 `withNavigationErrorHandler`: a returned `RedirectCommand` turns the error into a redirect; anything else keeps the error (logged, navigation cancelled) | The handler returns a redirect only for S80; for the reload and for offline it returns nothing |
| F2 | With the default `urlUpdateStrategy: 'deferred'` the address bar and `history.state` are not touched before the lazy chunk has loaded; but after the handler returns nothing, the router emits `NavigationError` and its `restoreHistory` replaces the current entry with the previous address and state **in the same call stack** (found in the review, R1) | In the reloaded document the failing navigation still sees the marked state; the mark and the reload are written in the next task, after the restore; a successful navigation copies the key into `extras.state` (harmless, read nowhere) |
| F3 | `nginx.conf`: hashed files `try_files $uri =404`, `index.html` `no-cache` (S6-07) | A lost chunk is a clean 404 and the reload fetches the new `index.html` — the E2E simulation (`page.route` → 404) matches production |
| F4 | `criteria.export.outdated` (v39) promises: selection kept, per-criterion changes redone; no key for the action | UX copy v40 U161 with the owner-confirmed wording |
| F5 | The options answer pins the release (`load()`), `loadCriteriaList` did not; S11's summary names come from the options | Reload loads the options first (pinning the active release and the names), then the list; `loadCriteriaList` pins the answer's release when none is pinned (options failed) |
| F6 | `mat-dialog-close` without a value closes with `''`, not `undefined` (found by the new dialog test: `'toNote' in ''` threw) | Both pages test `typeof result === 'object'` before reading the result |
| F7 | The saved-list export (`POST /configurations/{id}/exports`) uses the stored release; only `AdHocSelectionSource` answers `release-mismatch` | „Kriterien neu laden“ only for the draft of S11 |

## 2. Changes

| File | Change |
|---|---|
| `CLIENT/…/src/app/core/errors/chunk-reload.ts` (new, + `.spec.ts`) | `CHUNK_LOAD_MESSAGES`, `isChunkLoadError`, `chunkFailureAction`, `DocumentReload` (history-state guard through Angular's `Location`: `go`/`replaceState` with the mark, then `location.reload()`, both in the next task), `handleChunkLoadError`. The router tests run on `provideLocationMocks()`, the history the router itself writes to |
| `CLIENT/…/src/app/app.config.ts` | `provideRouter(routes, withNavigationErrorHandler(handleChunkLoadError))` |
| `CLIENT/…/features/configurator/state/with-configurator.ts` (+ spec) | `reloadCriteriaListForActiveRelease()`; `loadCriteriaList` pins the answer's release when none is pinned |
| `CLIENT/…/features/configurator/dialogs/export-dialog/export-dialog.{ts,html}` (+ spec) | Outdated state: „Schließen“ + „Kriterien neu laden“ for the draft, „Schließen“ only for a saved list; `ExportDialogResult = { toNote } \| { reload: true }` |
| `CLIENT/…/features/configurator/pages/criteria-list/criteria-list.ts` | Handles `{ reload: true }`: store reload, then `focusPageHeading()`, or the legacy filter for `noRelease`; result checked as an object |
| `CLIENT/…/features/configurator/pages/saved-list/saved-list-page.ts` | Result checked as an object before `toNote` |
| `CLIENT/…/public/i18n/de.json` | `criteria.export.outdated.reload` (by `check-i18n.mjs --write`) |
| `e2e/shell/chunk-reload.spec.ts` (new), `e2e/configurator/s11-list.spec.ts` | E2E as in the plan (run in CI only) |
| `docs/ux_copy_procureaccess_v40.md` (new; v38 deleted) | U161, the key row in §5.6 |
| `docs/ARCHITECTURE_v81.md` (new; v79 deleted) | C167 |
| `docs/design_handoff_procureaccess_v1.md` | 1.34: S13 row |
| `docs/CODEBASE_v1.md` | §1 change history, §2.44 pointer, §2.86, §3 layout line |
| `CLAUDE.md` | §3 client routing: the navigation error handler and where a new engine message goes |

Not changed: SPEC (no requirement changes; its header still names ARCHITECTURE v80 and UX copy v39, P4), TESTING (no invariant), Figma mapping (S13 frames unchanged), `LEGACY_EXCEPTIONS`, `nginx.conf`, the server.

## 3. Self-review (CLAUDE §6.7)

An independent review (a reviewer agent given the diff and the goal, not the design) read the router source of Angular 22.2.5 besides the code.

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | The mark and the target written in the handler are overwritten at once: the router's `NavigationError` handling (`restoreHistory` → `resetUrlToCurrentUrlTree`, default `canceledNavigationResolution: 'replace'`) replaces the entry with the previous address and state synchronously, and `location.reload()` runs later — the tab would reload the previous page without a mark, and the S80 branch would never be reached. The first unit test stubbed `DocumentReload` and could not see it | **Fixed** | Confirmed in `_router-chunk.mjs`. `DocumentReload.reload` now writes through Angular's `Location` and does so in the next task (`setTimeout`), after the restore. The router tests run the real guard on `provideLocationMocks()` (the history the router writes to); with the write made immediate again, three of them fail, so they guard the order |
| R2 | A chunk that fails because the server is out of reach while the browser reports online leads to a reload that ends on the browser's own error page | **Won't fix** | `ConnectionStatus` knows only `navigator.onLine` and a request without answer; the owner decided on the reload (O-S6-5 (a)); a dropped connection also fails a manual reload. P6 |
| R3 | While the list reloads, the focus is on `<body>`: the button MatDialog returns it to goes with the list | **Won't fix** | Short (one options and one list request; the loading bar announces `common.loading`), and the focus ends on the H1; keeping the action bar during the load would show the old release's counts |
| R4 | `'toNote' in result` would throw for `null` | **Won't fix** | No path closes S13 with `null` (`mat-dialog-close` gives `''`, Esc `undefined`); the `''` case was fixed with F6 |
| R5 | After a back/forward navigation the router has already rewritten the entry, so the reload adds an entry and „Zurück“ may lead to the page before | **Won't fix** | Rare (a lost chunk on back/forward); the target loads, nothing loops |

Own checks: the store method drops answers of the old release still in flight (both request counters), its `finally` blocks still decrement the loading counter, and `optionsCurrent()` ends because `changedSinceAnswer` is reset; the fallback pin in `loadCriteriaList` only applies when no options answer arrived. Security and privacy: no browser storage, no token in the address (a link page's token leaves the address before any lazy chunk of its page is needed); the handler writes only the router URL into the history state. No N+1 (client only). Accessibility: the new button is a real button in DOM order, takes the focus, axe in both appearances in the unit test and in E2E; S80 is eager, so the error page itself cannot fail on a chunk.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (all client unit tests) | 843 pass. Two earlier full runs had 3–4 first tests of heavy page specs time out at 5 s (load average ≈ 20 from parallel sessions; one in `members-page.spec.ts`, untouched); those files pass alone and the third full run was green |
| C2 | `npm run lint` (client) | Green |
| C3 | `npm run build:release` | Green; initial total 756.3 kB (budget 850 kB warning) |
| C4 | `npm run typecheck`, `npm run lint` in `e2e/` | Green |
| C5 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Green |
| C6 | `reuse lint` (local `fsfe/reuse` image, no pull) | Only `node_modules/` and the ignored `.env` listed — the container cannot read the worktree's git ignore rules; every repository file, the new ones included, carries its header |
| C7 | E2E | Not run locally (another session used the E2E stack). CI (#399, run 44221236610): the new specs `shell/chunk-reload.spec.ts` (both cases) and the S13 case of `s11-list.spec.ts` pass on Chromium, Firefox, WebKit and both mobile projects. The first Firefox job failed in an existing, unchanged test (`start-and-error-pages.spec.ts:109`, S80 after an API 5xx: H1 not focused, also on its retry); the re-run of the failed jobs was green, all other branches pass it — a Firefox flake on that path (P7) |
| C8 | Figma | S13-E `2039:1944` compared through the Figma MCP before building; the outdated state uses its action layout |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | S12 („Liste speichern“) shows `criteria.save.outdated` — the same „Bitte laden Sie die Kriterien neu“ — with „Schließen“ only; it could offer the same reload (needs a UX key such as `criteria.save.outdated.reload`, owner) | Proposed follow-up issue; before a second release is activated |
| P2 | A selection the new release excludes ends on S11's failure state with the generic snackbar (and, if the options also refuse it, a second one); #310's proposal „back to S10 at the right step“ was not chosen (O-S6-5) | Watch when 2026.x is activated; an issue if the pilot meets it |
| P3 | The engine messages are matched as text; a browser that changes its message falls back to today's behaviour (the click does nothing). The E2E spec on three engines detects it in CI | Watch item |
| P4 | SPEC_v2 and TESTING_v88 headers still name ARCHITECTURE v80 and UX copy v39 | Their next version (sprint-end documentation, S6-09) |
| P5 | #296 asked for an E2E of the offline case; it is covered by unit tests only (`setOffline` and the browsers' `offline` event differ between engines) | If an offline regression appears |
| P6 | A lost chunk while the server is out of reach but the browser reports online reloads into the browser's own error page (R2) | Watch item; a probe of `/health` before the reload would need an owner decision |
| P7 | Firefox flake: `start-and-error-pages.spec.ts:109` (focus on the S80 H1 after an API 5xx) failed twice in one job and passed on the re-run; it does not reach the chunk handler (the failure is an HTTP 5xx) | Proposed issue if it recurs |

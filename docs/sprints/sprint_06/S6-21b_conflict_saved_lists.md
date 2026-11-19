<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-21b — Conflict dialog S37 for the saved lists (S11, S10)

> **Sprint:** 06, task S6-21 (2.5 points), part b of the split the owner confirmed on 2026-11-19 (D5 (a)); after S6-21a (#425). Started on `main` at `eee42d02` (S6-21a merged). Documents: TESTING v109, ARCHITECTURE v104, SPEC v2.66, UX copy v47 (unchanged), Handoff 1.44, Figma mapping 1.35, CODEBASE §2.111. **Issues:** none.
> **Requirements / invariants:** R-CAN-11 (UI) for the saved lists — R-SAV-03 and the project's copy (R-PRJ-07); API-03 stays green and gains the client cases of the saved lists; A11Y-01, A11Y-08 gain S37 on S11. No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **No EF migration, no server change, no new UX key**, no new dependency. E2E run locally in Chromium, Firefox and mobile Chrome; WebKit and mobile Safari left to CI.
> **Scope:** S37 for the autosaved decisions on S11 — a saved list of the organisation and a project's copy, one page and one editor — and for S10's selection change of a saved list. Not in scope: S14's delete (S6-21a open point 2).
> **Owner decisions:** with the plan of S6-21a (2026-11-19), no new one: **D2 (a)** — the copied text has „label: value“ per changed field, on S11 each criterion's title with its changed parts · **D3 (a)** — S11 keeps the shown decisions after a conflict, stops saving, and every further change opens S37 again with all unsaved changes · **D4 (a)** — S10's selection change of a saved list is included; S14's delete keeps the general error.

## 0. Plan (agreed with S6-21a)

- **S11:** `SavedListEditor` keeps the shown decisions after a `conflict`, stops saving and reports each blocked save; the page opens S37 and reloads the list in place.
- **S10:** S37 for `PUT …/selection`; reload opens the list.
- **Tests:** component specs of editor, page, resolvers, S10 and the API; the E2E with two contexts on a saved list. **Invariants:** none flipped. **Migration:** none.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `SavedListEditor` put a refused decision back and kept the stale tag, so every later change failed with the general error (S6-20b R1) | A `conflict` keeps the decision (D3 (a)) and blocks; any other failure still puts it back |
| F2 | The editor is a route-level service without access to the dialog or the translations | It reports on an observable `conflicts` and lists `unsaved()`; the page builds the text and opens S37 |
| F3 | S11 has two routes with different loaders (a list of the organisation, a project and its copy); the resolver alone knew how to read | Each resolver stores its loader as `SavedListLoad.reload`; the page reads again in place — no navigation, so the page, its filter and its editor stay |
| F4 | A note is saved 800 ms after the last keystroke | While blocked, S37 opens after that pause, also while the user is still in the note; Escape returns the focus to the note (as D3 (a) says: every further change) |
| F5 | S10's „Kriterienliste anzeigen“ first asks the server to check the selection, then saves | After a conflict, S37 opens before the check: no request at all until the list is read again |
| F6 | The labels of a card's controls exist: `criteria.item.include` (the checkbox's name without the title), `criteria.priority.changeLabel`, `criteria.priority.weightLabel`, `criteria.item.note`; S10's layers have the S11 summary labels | No new UX key; „Ja“/„Nein“ are `common.yes`/`common.no` |

## 2. Changes

- **`SavedListEditor`:** `EditConflict`, `conflicts`, `conflictEditor`, `unsaved()` (`UnsavedDecision`); a `conflict` keeps the shown decision; while blocked nothing is sent; `start()` clears.
- **`SavedListLoad.reload`**, set by `savedListResolver` and `projectCriteriaListResolver`.
- **S11** (`SavedListPage`): S37 once at a time, never after the page is gone; the copied text per criterion (`changedGroup`, `cardTitle`); `reload()` with the loading bar, `show({ fromRoute: false })`, focus to the H1.
- **S10** (`CriteriaSelect`): `EditConflict`; `SavedListSelectionContext` with the stored selection (`savedListSelectionGuard`); S37 after the spinner; the changed layers; „Neue Fassung laden“ → the list's page.
- **API:** `decide` and `changeSelection` leave `conflict` to the caller (`conflictContext()`).
- **Tests:** `saved-list-editor.spec.ts` (+2), `saved-list-page.spec.ts` (+3), `saved-list.routing.spec.ts` (+1), `project-criteria-list.routing.spec.ts` (+1), `criteria-select.spec.ts` (+2; `SAVED` with its selection), `configurator-api.service.spec.ts` (request contexts); E2E `e2e/workspace/conflict.spec.ts` (+1 test, shared setup `ownerAndEditor`, `countPatches`).
- **Documents:** TESTING v109 (V392 – V396; v107 deleted), ARCHITECTURE v104 (C192; v102 deleted), SPEC v2.66 (v2.66-1, -2), Handoff 1.44 (row „S37 as built (part b)“), Figma mapping 1.35, CODEBASE §2.111 (and the pointer in §2.110, §3), SPRINT_06 (the protocol in the row). `CLAUDE.md` unchanged (size limit, S6-21a open point 4).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security / privacy:** no request, header or log changed; the copied text holds the user's own decisions and notes, only in their clipboard. Nothing in browser storage.
- **Correctness / edge cases:** a change back to the saved decision is no unsaved change and opens nothing (unit); a reload that finds the list gone shows „Seite nicht gefunden“ in place; the project-copy resolver's `reload` reads the project first, so a copy replaced meanwhile is read under its new id (unit); `flush()` on leaving the page while blocked sends nothing, and the destroyed page opens no dialog (`takeUntilDestroyed`, `destroyed`); S37 is never stacked twice (`conflictShown`). S10 resolves the spinner before S37, so its button is not busy behind the dialog.
- **Requests:** one reload per „Neue Fassung laden“ (the same reads as the resolver); none for a blocked change (unit and E2E).
- **A11y:** as S6-21a — alert dialog, first focus on the copy button, Escape returns the focus to the control that was changed; while S37 is open the page is hidden from assistive technology (the unit test reads the cards only after Escape); axe in both appearances on `s37-saved-list`.
- **Found while testing and fixed:** a unit test queried a card while S37 was open; the page behind the dialog is `aria-hidden`, so the check moved after Escape.
- **Won't fix:** (1) after a conflict, „Liste exportieren“ counts the shown decisions in S13 while the file holds the stored list — the user has been told by S37 that the changes are not saved, and reloading aligns both. (2) On a project's copy, criteria taken in and saved before the conflict are not reported to S32 after „Neue Fassung laden“ (the report compares the list as read with the list as left; the reload starts a new baseline). (3) S37 may open while the user is still typing a note (F4) — the behaviour D3 (a) asks for.
- **Follow-up:** open points 1 – 3.

## 4. Checks

- Client: `npx ng test` **1 143 passed** (108 files); `npm run lint` clean; `npm run build:release` clean; `tsc` of the app clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** (throwaway PostgreSQL and Mailpit, the API with `dotnet run` and the env of `docker-compose.e2e.yml`, `ng serve --port 4300`): `workspace/conflict.spec.ts` 6 passed, then `configurator/`, `journeys/`, `workspace/` and `shell/reflow.spec.ts` **256 passed, 2 skipped** (the known Chromium-only forced-colours test and the J1 Firefox fixme, TESTING V254) in Chromium, Firefox and mobile Chrome. WebKit and mobile Safari: CI.
- No server change: the .NET build and tests are unaffected (CI runs them).
- `check-invariants` (gate passed), `check-figma-links`, `check-i18n` (0 errors), `check-icons` (0 errors); `reuse lint-file` on every new file: compliant.
- **Figma:** the dialog is S6-21a's, compared there with `2056:2519`; S6-21b changes nothing visual.

## 5. Open points

1. **S14's delete** answers a `conflict` with the general error (S6-21a open point 2) — a design question for the owner (proposed issue).
2. **Leaving S11 after a conflict** drops the unsaved decisions without a question (SPEC v2.66-1 says so). A „leave without saving?“ prompt would need UX copy — only if users report losing input.
3. **`CLAUDE.md` size limit** (S6-21a open point 4) — unchanged.

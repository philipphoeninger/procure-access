<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-22b — Journey J2 up to the assessment

> **Sprint:** 06, task S6-22 (3 points), part b of the split the owner confirmed on 2026-11-19 (S6-22 D1 (a)); after S6-22a (#427). Started on `main` at `4dc19727` (S6-22a merged). Documents: TESTING v111, CODEBASE §2.113; ARCHITECTURE v105, SPEC v2.67, UX copy v48, Handoff 1.45 and Figma mapping 1.35 unchanged. **Issues:** #429 (opened from open point 1).
> **Requirements / invariants:** the E2E part of S6-22 — J2 (TESTING §5.4) up to the assessment, with R-CAN-10's Reader view; **A11Y-02** gains J2; A11Y-01, A11Y-08 gain its states. No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **A test only:** no server change, no client change, no EF migration, no UX key, no dependency. Run locally in Chromium, Firefox and mobile Chrome; WebKit and mobile Safari left to CI.
> **Scope:** `e2e/journeys/j2-workspace.spec.ts` — create project → assign a list → adjust it → add a product → evidence → assessment → Reader view, keyboard only, axe in both appearances, on all five browser projects. This is the M5 exit check. Not in scope: the publication half of J2 (step 6).
> **Owner decisions:** none new; D1 (a) of S6-22 (the split).

## 0. Plan (given at the start of the session)

- One journey file, keyboard only as J1 defines it, at each project's own viewport; an Owner's path through S30, S31, S32, S11 of the copy, S33, S34a and S34b, then a Reader in a second browser context.
- CSS locators only and no axe on S31, because of the known Firefox crashes.
- No invariant flips; A11Y-02 gains J2. No migration.
- If a screen cannot be operated by keyboard: report it, do not work around it.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Every screen of J2 up to the assessment exists and has its own E2E test with pointer or mixed input; no test went through all of them, and only S34b had a keyboard-only test | One journey, as J1 for the configurator |
| F2 | J1 runs at each project's own viewport; the workspace specs fix 1 280 px | J2 follows J1, so the mobile projects take the layouts below 600 px: K6 as a select with the note behind a button, the short tab label. The test branches once (`compact`) |
| F3 | Firefox's page process crashes when Playwright looks an element up by its accessible name and axe runs on S11, S32, S34a or S34b, and when axe runs on a dialog with a `mat-select` (TESTING V165, V226, V341, V368) | CSS locators throughout; S31 without axe (it has its own test) |
| F4 | `saveListThroughApi` saves a list without a test procedure | No „Geprüft nach …“ row in J2; the first draft's assertion of one was wrong and is gone |
| F5 | S11 of a project's copy has no link to its project: no path above the H1, no link in `criteria.list.projectCopy` | The journey goes back as the browser does (`page.goBack()`); open point 1 |
| F6 | **Every step worked with the keyboard at the first try** — the S31 select, the menu of S32, the ARIA tabs with the arrow keys, the radio group of S34a, both forms of K6 | No product change needed |

## 2. Changes

- **`e2e/journeys/j2-workspace.spec.ts`** (new): the test „INV:A11Y-02 INV:A11Y-01 INV:A11Y-08 an Owner creates a project with a list, adjusts the list, adds a product, records its evidence and assesses a criterion by keyboard; a Reader finds all of it read only“. Helpers in the file: `tabTo` (Tab or Shift+Tab until the target has the focus), `arrowToOption` (an open `mat-select`), `openTab` (the selected tab, ArrowRight, Enter, the H1 focused), `valuesOf` (a `<dl>` row). Axe on twelve states.
- **Documents:** TESTING v111 (V404 – V406; the A11Y-02 row, the J2 row of §5.4, §9 step 5; v109 deleted), CODEBASE §2.113 (and the pointers in §2.112, §3), SPRINT_06 (the protocol in the row). SPEC, ARCHITECTURE, UX copy, Handoff, Figma mapping and `CLAUDE.md` unchanged: nothing they describe changed. The header of SPEC still names TESTING v110; it is renewed with the next SPEC version.

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), for a test-only change:

- **Does it test what it says?** No `click`, `tap`, `hover`, `check` or `fill` in the file — only `page.keyboard`, `page.goto` at the start of each actor, and `page.goBack()` once (F5). Every target is reached through `tabTo`, which fails when the element is not in the tab order from the current focus on. Each save is awaited by its response and checked by status; the Reader's assertions read the values the Owner typed.
- **Stability:** the dialog's first focus is awaited before typing (S6-01); autosaves are awaited by their `PUT`/`PATCH`; axe runs after `settled` and after the form fields carry `mat-form-field-animations-enabled` (memory of S02/S31); the radio group is left with ArrowUp only (WebKit does not wrap); own accounts and organisation per run, nothing shared is changed.
- **Security / data:** test addresses at `example.test`, a test password in the file as in the other specs (no secret).
- **Won't fix:** (1) the helpers `tabTo`, `settled` and `arrowTo…` exist in several spec files (J1, `assessment.spec.ts`, here) — the specs have kept their helpers local so far; moving them to `e2e/support/` is a refactoring of files this task does not touch (open point 3). (2) The Reader's membership and the saved list come through the API — their screens have their own journeys.
- **Follow-up:** open points 1 – 3.

## 4. Checks

- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** (throwaway PostgreSQL and Mailpit containers, the API with `dotnet run` and the environment of `docker-compose.e2e.yml`, `ng serve --port 4300`): J2 in Chromium, Firefox and mobile Chrome, three and then two repetitions per project — **15 of 15 passed**. WebKit and mobile Safari cannot run on this machine: CI.
- No server or client change: the .NET and Angular builds and tests are those of `main` (CI runs them).
- `check-invariants` (gate passed), `check-figma-links`, `check-i18n` (0 errors), `check-icons` (0 errors), `reuse lint` (compliant).
- **Figma:** no screen or component built or changed.

## 5. Open points

1. **S11 of a project's copy has no way back to its project** but the browser's „back“ and „Projekte“ in the header (F5). S34 has the path `pa-breadcrumb`; the same path on the copy's page would close the gap. Needs no new text (project name, list name). Issue **#429** (opened on 2026-11-19 at the owner's request) — a small UI change, not part of this task.
2. **The rest of J2** with step 6: the publication blocked while the status is „Nicht bekannt“, the second evidence with the derived „Geprüft nach …“ rows (a list with a test procedure), publishing, the directory.
3. **Shared keyboard helpers** (`tabTo`, `arrowTo`, `settled`) for `e2e/support/` when the next journey is written.
4. **M5 exit:** with J2 green on all five projects in CI, the step-5 success criterion „Remove and read-only (M5.5)“ of SPRINT_06 is met; ticking it is the owner's at the sprint review.

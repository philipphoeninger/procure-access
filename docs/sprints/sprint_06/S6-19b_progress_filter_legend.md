<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-19b — Assessment tab S34b, part b: progress, filter and legend

> **Sprint:** 06, task S6-19 (5 points), part b (≈ 2 points) of the split confirmed on 2026-11-19; after S6-19a (#423). Started on `main` with S6-19a merged. Documents: TESTING v107, ARCHITECTURE v102, SPEC v2.64, UX copy v46, Handoff 1.42, Figma mapping 1.33, CODEBASE §2.109. **Issues:** none.
> **Requirements / invariants:** R-CAN-08 (progress, filter, legend); §8.4 and §8.12 (the fifth setting). A11Y-06 and OPS-03 gain cases; A11Y-01, A11Y-08 and A11Y-09 too. No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **One EF migration** (IdentityAccess, `AssessmentLegendPreference`); no new dependency. E2E run locally in Chromium, Firefox and mobile Chrome; WebKit and mobile Safari left to CI.
> **Scope:** the progress row of S34b with the list line, the filter „Nur noch nicht eingeschätzte anzeigen“, `pa-rating-legend` expanded until the first assessment with its state as a stored user setting. Not in scope: the Reader view (S6-22).
> **Owner decisions:** with the plan of S6-19a (2026-11-19): **Q1 (b)** — the legend state as a fifth user setting in IdentityAccess · **Q2 (a)** — new key `candidate.assessment.list`, wording from the frames (to be confirmed in UX copy v46). With the plan of S6-19b (2026-11-19): **(a)** — the first assessment stores „collapsed“, but the legend stays open on that visit (no jump of the list under the focus). The defaults of the S6-19a plan were kept: the filter is a snapshot, `criteria.list.emptyFilter` for an empty filter, the legend's levels include „Nicht eingeschätzt“, the progress announcement with its own 5 s throttle.

## 0. Plan (written before the code)

- **Server:** `assessmentLegend` (`auto` default, `expanded`, `collapsed`) in IdentityAccess with a migration; optional in the `PUT` (missing keeps); in the export (format 1).
- **Client:** the member in the settings model, `withSettings()` saves it quietly; UX copy v46 with `candidate.assessment.list`; S34b with progress row, filter and legend.
- **Tests:** integration, component, E2E, reflow. **Invariants:** none flipped.
- **Session:** one; it fitted.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `UserPreferencesValues` (Contracts) is a positional record built with four arguments in a dozen tests | The fifth member gets the default `"auto"`: every existing construction stays valid and means „never chose“ |
| F2 | The `PUT` replaces all four settings; S60 sends the whole object it read | `assessmentLegend` is optional in the `PUT`, missing keeps the stored one — a client without the member (or a tab opened before the deployment) cannot reset it; S60 now sends it along unchanged |
| F3 | `UserForeignKeysMigrationTests` migrates to an older target and then inserts settings through EF with today's model | With the new column that insert would fail before the migration that adds it: the test inserts its two settings rows with SQL in the four columns of that time |
| F4 | Handoff §2.2: the legend on S34b is expanded until the first assessment from 600 px, collapsed below | The initial state depends on the setting and the width; an explicit choice wins on every width |
| F5 | The seeded E2E users are shared and must not change (CLAUDE §3) | The reflow test opens the legend as the seeded owner: it answers the settings `PUT` itself |

## 2. Changes

- **Server (IdentityAccess):** `AssessmentLegendPreference` and `UserPreferences.AssessmentLegend` / `ChangeAssessmentLegend`; `ParsedPreferences.AssessmentLegend` (null keeps); spelling `auto` / `expanded` / `collapsed`; `UserPreferencesBody.AssessmentLegend` optional with its 400; `UserPreferencesValues.AssessmentLegend = "auto"`; `AccountExportPreferences` lists it; configuration (string, 16); migration `AssessmentLegendPreference` (internal, default `Auto` for existing rows). `has-pending-model-changes`: none.
- **Client settings:** `AssessmentLegendSetting`, the member in `UserPreferences` / `SYSTEM_PREFERENCES`; `PreferencesApiService.save(…, { background })`; `withSettings().rememberAssessmentLegend`.
- **UX copy v46:** U186 `candidate.assessment.list`, U187 the empty filter text at `candidate.assessment.filter.unassessed` (v44 deleted); `de.json` by `check-i18n --write`.
- **S34b:** progress row (progress, list line, filter), legend, union note moved directly above the list, the filter's snapshot and empty text, the derived rows hidden while filtering, the progress announcement, the legend's state and storage (ARCHITECTURE C190).
- **Tests:** `UserPreferencesTests` (+1 fact, +2 invalid spellings, the five members), `AccountExportTests`, `UserForeignKeysMigrationTests` (+1 OPS-03, SQL inserts); client `with-settings.spec.ts` (+4), `preferences-api.service.spec.ts` (+1), `candidate-assessment.spec.ts` (+9); E2E `assessment.spec.ts` (progress, legend over a reload, filter with Space, axe on `s34b-filtered`), `reflow.spec.ts` (legend open, filter on), `preferences.spec.ts` (the stored settings with the member).
- **Documents:** TESTING v107 (V383 – V386; v105 deleted), ARCHITECTURE v102 (C190; v100 deleted), SPEC v2.64 (v2.64-1 – -4), UX copy v46, Handoff 1.42 (the S34b row's part b), Figma mapping 1.33, CODEBASE §2.109 and §3, SPRINT_06 (both parts done). `CLAUDE.md` unchanged (no rule across the codebase; size limit, S6-17 open point 6).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security / privacy:** the setting is per user, keyed by the signed-in user's id as the other four; antiforgery on the `PUT` unchanged; nothing in browser storage (R-SET-02); the export carries the user's own value only (test with another user's settings).
- **Correctness / edge cases:** a `PUT` without the member keeps it (test); an unknown spelling or a wrong case is 400 at the field (tests); rows from before the migration start at `Auto` (OPS-03 test); the legend save runs after any save in flight, so it never overtakes an S60 change; an unchanged state sends nothing; without settings (read failed) nothing is sent and the page keeps its own state; the first assessment stores „collapsed“ once and the legend stays (test); the filter keeps a row assessed meanwhile (test) and shows the empty text when none is left (test).
- **Requests:** at most one settings `PUT` per legend change and one at the first assessment.
- **A11y:** the filter is a labelled checkbox; its count and the progress are announced through the announcer with throttles; the legend keeps `aria-expanded` / `aria-controls`; no layout jump under the focus (S6-19b (a)); axe in both appearances on the new states; reflow at 320 px with legend and filter.
- **Found while testing and fixed:** (1) the keyboard E2E could not reach the tab bar from the filter with Tab (the filter is below it): Shift+Tab. (2) The reflow test would have stored the legend choice for the shared seeded user: it answers the `PUT` itself (F5).
- **Won't fix:** (1) a user whose settings could not be read gets the legend by width only, and nothing is stored in that visit — the next sign-in reads them again. (2) The progress is announced on the change, before „Einschätzung gespeichert.“; both are throttled and short.
- **Follow-up:** open points 1 – 3.

## 4. Checks

- Server: `dotnet build ProcureAccess.sln -warnaserror …` clean; `dotnet format --verify-no-changes` clean; `dotnet ef migrations has-pending-model-changes` (IdentityAccess) — no changes; the affected integration classes 31 passed; the full `dotnet test --solution ProcureAccess.sln` **2 161 passed** (unit, architecture, integration).
- Client: `npx ng test` **1 115 passed** (107 files); `npm run lint` clean; `npm run build:release` clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** (throwaway PostgreSQL and Mailpit, the API with `dotnet run` — which applied the new migration —, `ng serve --port 4300`): `workspace/`, `shell/reflow.spec.ts`, `identity/preferences.spec.ts`, `identity/account.spec.ts` (filtered to the affected tests) — 45 passed, then the changed keyboard test 3 passed in Chromium, Firefox and mobile Chrome. WebKit and mobile Safari: CI.
- `check-invariants` (gate passed), `check-figma-links` (11 checked), `check-i18n` (0 errors), `check-icons` (0 errors).
- `reuse lint-file` on every new file: compliant (worktree; CI runs the full `reuse lint`).
- **Figma:** S34b compared side by side in a browser at 1440 px and 360 px with `2022:841` and `2136:3127`: the progress row (progress, list line, filter on the right from 600 px, stacked below), the legend expanded on desktop for a user who never assessed and collapsed at 360 px, as the frames show.

## 5. Open points

1. **The wording of `candidate.assessment.list`** (UX copy v46 U186) is the frames' text; the owner confirms it.
2. **S60 does not show the legend setting.** A user who closed the legend reopens it on S34b itself; if users ask for a reset of such remembered states, S60 could get a section — a design decision.
3. From S6-19a: no text of its own for a failed autosave; `assessment.procedure.derived` unused; the Reader view with S6-22.

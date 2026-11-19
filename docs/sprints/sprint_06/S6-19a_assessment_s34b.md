<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-19a — Assessment tab S34b, part a: K6 rows and autosave

> **Sprint:** 06, task S6-19 (5 points), part a (≈ 3 points) of the split confirmed by the owner on 2026-11-19; after S6-18 (#413) and S6-16 (#415). Started on `main` after S6-17 (#418), rebased onto `main` after S6-17b (#422) was merged during the task. Documents: TESTING v106, ARCHITECTURE v101, SPEC v2.63, Handoff 1.41, Figma mapping 1.32, CODEBASE §2.108. **Issues:** none.
> **Requirements / invariants:** R-CAN-06 (UI), R-CAN-07 (UI); CAT-15 and CAT-17 (workspace part) gain client cases; A11Y-01, A11Y-08 and A11Y-09 gain cases; the keyboard-only autosave is tested (the S34b part of A11Y-02, whose J2 follows with S6-22). No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **No EF migration, no server change, no new dependency.** E2E run locally in Chromium, Firefox and mobile Chrome (no other session used the stack); WebKit and mobile Safari left to CI.
> **Scope:** the tab „Einschätzung nach Kriterien“ of S34: K6 per requirement criterion with the allowed levels only (radios from 600 px, compact form below), autosave with an 800 ms debounce, `candidate.assessment.saved` at most every 5 s, the note per criterion, the union note, the derived „Geprüft nach …“ rows with the link to S34a. **Not in scope (S6-19b):** progress with the list line, the filter „Nur noch nicht eingeschätzte“, the rating legend and its stored state. Not in scope at all: the Reader view (S6-22), `publish.backToPublish` (step 6).
> **Owner decisions (with the plan, 2026-11-19):** **Q1 (b)** — the legend's state becomes a fifth stored user setting (IdentityAccess, SPEC §8.12; S6-19b) · **Q2 (a)** — new key `candidate.assessment.list` „Kriterienliste: {{list}} (Katalogversion {{release}})“ for the line of both frames (S6-19b; the owner confirms it in the next UX copy version) · **Q3 (a)** — Readers and archived projects see the empty tab until S6-22, as S6-17 D5 (a) · the split a/b confirmed. The defaults stated with the plan were not objected to: the filter applies when it is changed (a row assessed meanwhile stays until then), `criteria.list.emptyFilter` for an empty filter, the legend's levels on S34b include „Nicht eingeschätzt“, a failed autosave shows the interceptor's text and keeps the change, the progress announcement has its own 5 s throttle (the last four concern S6-19b).

## 0. Plan (written before the code)

- **Starting point:** the server of S6-18 (`GET /candidates/{id}/assessments`, `PUT …/assessments/{criterionRevisionId}`, last write wins). On the client the tab was `CandidateTabPlaceholder` (S6-16); K6 did not exist; `pa-rating-legend`, the announcer's throttle and `injectWideLayout()` did.
- **Frames:** desktop `2022:841` (radios, the note field always shown), 360 `2136:3127` (select, the note behind „Begründung hinzufügen/bearbeiten“, the legend collapsed). Neither shows K5, so the missing hide key of „Warum diese Priorität?“ (TECH_DEBT_v5 §5) does not block S34b.
- **Files:** models, API, K6 in `shared/`, the store slice, the tab, routing, specs, E2E, reflow, documents.
- **Invariants:** none flipped. **EF migration:** none in a; b adds one in IdentityAccess (Q1 (b)).
- **Session:** split a/b (≈ 3 + 2 points). Part a fitted one session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | ARCHITECTURE C184 (2) foresaw that the autosaved assessments bring candidate state into `withWorkspace()`; the Handoff wants „Last saved state survives navigation (store)“ | The rows and the save queue live in the root store, not in the tab, so a save outlives the tab |
| F2 | The server answers `PUT` with the stored (trimmed) row | The client does not write the answer back: it would replace what the user is still typing (a trailing space would vanish) |
| F3 | An assessment never changes the candidate's tag (SPEC v2.59-3), but it changes progress and K1 on Überblick, which read the candidate once per candidate id | After leaving the tab, only `progress` and `fulfillment` of a new read are taken over; the tag and the fields stay as the other tabs read them |
| F4 | `assessment.procedure.*` was not in `AREAS` of `check-i18n.mjs` | Added; the keys came from UX copy v45 with `--write` |
| F5 | The frames show the derived row's value on its own line under the title; the key `assessment.procedure.derived` („Geprüft nach {{testType}}: {{value}}“) joins both | The row shows the server's title („Geprüft nach …“) and `.derived.yes|no|notStated` on its own line, as the frames and U69; `assessment.procedure.derived` itself is not used |
| F6 | S6-17b (#422) was merged during the task: long questions as a visible label above the field (Handoff §2.2 „Forms“, D7 (a)) | K6's note question follows the same rule (§2) |

## 2. Changes

- **K6** `shared/components/assessment-input/` (`AssessmentInput`, `@figma 2012-2430`; Handoff §4.6): fieldset with the criterion title as legend; `radios` — the question linked to the radio group, one radio per allowed level in scale order and „Nicht eingeschätzt“ last, in a row that wraps; `compact` — a select „Einschätzung“ with the same options, the note behind a disclosure (`aria-expanded`, `aria-controls`, described by the title; opening moves the focus into the field; a too-long note keeps it open). The note: an outlined text area with the question as a visible `<label for>` above it (Handoff §2.2 „Forms“) — the desktop frame shows it as a floating label, which was cut off at 320 px as on S34a (§4); its length error `errors.field.maxLength` once the field was left. `value` and `note` are models; no colour per level.
- **Store** `withWorkspace()`: `assessments { candidateId, status, view }`; `loadAssessments`, `changeAssessment`, `flushAssessments`, `resetWorkspace` extended (ARCHITECTURE C189 (3)). One timer per row (800 ms); the saves of a row chained so the client's last change is the last request; a read keeps rows waiting or in flight; a row with a too-long note is not sent but kept as unsaved; each save announces `candidate.assessment.saved` with the 5 s throttle. API and announcer read through the store's injector on first use.
- **API** `candidates-api.service.ts`: `assessments`, `saveAssessment`.
- **Tab** `pages/candidate-detail/assessment/` (`CandidateAssessment`, route `einschaetzung`, `@figma 2136-3127`, `2022-841`): intro, union note, K6 list (layout from `injectWideLayout()`), the derived rows with „Prüfnachweis bearbeiten“; without a list `projects.detail.noCriteria`; a failed read the S80 texts as an error notice; empty for Readers and archived projects (Q3 (a)); on destroy: flush, then progress and counts for Überblick (F3).
- **Removed:** `tab-placeholder/candidate-tab-placeholder.ts`; `candidate-detail.spec.ts` has a local tab stub.
- **i18n:** `assessment.procedure` in `AREAS`; `de.json` written by `check-i18n.mjs --write`. No new key.
- **E2E:** `e2e/workspace/assessment.spec.ts` (keyboard only, axe in both appearances on three states), helper `createCandidateThroughApi`; `reflow.spec.ts` gains the S34b case, `routeCandidate` answers the assessments.
- **Documents:** TESTING v106 (V379 – V382; v104 deleted), ARCHITECTURE v101 (C189; v99 deleted), SPEC v2.63 (v2.63-1 – -4, the decisions for b in -4), Handoff 1.41 (§5.3 „S34b as built (part a)“), Figma mapping 1.32 (K6 built), CODEBASE §2.108 and §3, SPRINT_06 (the split as built). `CLAUDE.md` unchanged: no rule across the codebase changed, and it is at its size limit (S6-17 open point 6).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security:** no new endpoint; the `PUT` goes through S6-18's checks (role, organisation from the resource, confirmed address, antiforgery, archived project, allowed levels). The tab hides its content from Readers and archived projects, but the server decides. Nothing is kept in browser storage (R-SET-02); `resetWorkspace` drops the rows on sign-out.
- **Correctness / edge cases:** debounce per row, not per tab — changing two criteria quickly saves both; a change during a save of the same row waits for it, so the last change is stored last (test); a read while a save is on its way keeps the local value (test); a failed save keeps the change on screen and is not announced (test); the tab left within 800 ms sends at once (test); switching to another candidate in the same tab keeps the keys apart (`candidateId/criterionRevisionId`).
- **Requests:** one `PUT` per row and pause, one read per tab visit, one candidate read after leaving when something changed. No N+1: the list comes in one answer.
- **A11y:** fieldset/legend per criterion; the question by `aria-describedby`; the derived rows' link described by its title; the compact disclosure moves the focus; announcements only through `A11yAnnouncerService` with the throttle; no spinner (Handoff §6.2); axe in both appearances in the component tests and E2E; reflow at 320 px.
- **Fix (found in the review, fixed):** a row whose note was too long was dropped from the waiting rows when its timer fired, so leaving and coming back to the tab read the server again and lost the typed note. It now stays as unsaved without a timer (test „does not save a row while its note is too long, and keeps it on a new read“).
- **Found while testing and fixed:** (1) reflow at 320 px: the page was 341 px wide with the note open — the floating label „Worauf stützt sich Ihre Einschätzung? (optional)“; first clipped like S34a's, then, after #422, moved above the field as Handoff §2.2 asks. (2) At 360 px the floated label „Einschätzung“ touched the criterion title: 8 px above the select. (3) On a direct load the H1 is not focused (only an in-app route change focuses it): the E2E waits for it to be visible. (4) Running Prettier without a project config reformatted whole files; restored and reapplied (no formatter change in this PR).
- **Won't fix:** (1) a change made less than 800 ms before closing the browser tab is lost — the tab's destroy does not run on unload; a `beforeunload` save would need `keepalive` requests and is out of proportion for 800 ms. (2) Below 600 px the note of a criterion that has one stays collapsed, as the 360 frame shows; „Begründung bearbeiten“ says that one exists.
- **Follow-up:** open points 1 – 4.

## 4. Checks

- Client: `npx ng test` **1 101 passed** (107 files); `npm run lint` clean; `npm run build:release` clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** against the worktree (throwaway PostgreSQL 17 and Mailpit containers, the API with `dotnet run` in Development with the environment of `docker-compose.e2e.yml`, the client with `ng serve --port 4300`; the disk had 2 GB free, too little for the E2E images): `workspace/`, `shell/reflow.spec.ts` (S33, S34, S34a, S34b) and the products journey — **18 passed** in Chromium, Firefox and mobile Chrome after the last change (36 before it, with S30 – S32). WebKit and mobile Safari cannot run on this host: left to CI.
- `check-invariants` (gate passed), `check-figma-links` (11 checked: K6 is checked now), `check-i18n` (0 errors), `check-icons` (0 errors), the script self-tests (62 passed).
- Backend unchanged (no `dotnet` change in this PR).
- `reuse lint-file` on every new file: compliant (worktree; CI runs the full `reuse lint`).
- **First CI run of #423:** every stage green except E2E in three browsers. (1) WebKit and mobile Safari: the keyboard test pressed ArrowDown on the last radio („Nicht eingeschätzt“) and expected the first one checked — an arrow key past the end of a radio group wraps in Chromium and Firefox but not in WebKit. The test now presses ArrowUp, which checks the level before „Nicht eingeschätzt“, and checks Überblick's count of that level; run locally twice per project in Chromium, Firefox and mobile Chrome (66 passed with the error-page spec). (2) Firefox: `start-and-error-pages.spec.ts` „a server error shows „Etwas ist schiefgelaufen“ in place, with the focus on the H1“ found the H1 not focused — a test this PR does not touch, green in every earlier run on `main` and the sprint-06 branches and twice in a row locally in Firefox; treated as a flake and left to the next run.
- **Figma:** both frames (`2022:841`, `2136:3127`) looked at through the Figma MCP before building; the result compared side by side in a browser at 1440 px and 360 px (screenshots of the running app). The same structure and order; the differences: the note's question above its field (Handoff §2.2), and the progress row, filter and legend that S6-19b adds. The test release T.1 has no „Geprüft nach …“ criterion in the list the tests save, so the derived rows were compared through the component test and the reflow case.

## 5. Open points

1. **S6-19b** (next): progress „{{done}} von {{total}} Kriterien eingeschätzt“ with `candidate.assessment.list` (Q2 (a); the owner confirms the wording in the next UX copy version), the filter „Nur noch nicht eingeschätzte“ with its count, `pa-rating-legend` above the list — from 600 px expanded until the first assessment, collapsed below — and its state as a fifth member of the user's settings (Q1 (b): IdentityAccess column and migration, SPEC §8.12, `PUT /account/preferences` with the member optional, S60 keeps it).
2. **No text for a failed autosave:** the interceptor's general text names no criterion and the screen keeps the unsaved choice. If the pilot shows that users miss it, the UX copy needs a key (for example a notice at the row) — a decision for the owner.
3. **`assessment.procedure.derived`** („Geprüft nach {{testType}}: {{value}}“) is unused: the frames split title and value (F5). The UX copy could drop it or describe it as the accessible text of a row.
4. **The Reader view** of S34b with S6-22 (Q3 (a)); `publish.backToPublish` with S35 in step 6.

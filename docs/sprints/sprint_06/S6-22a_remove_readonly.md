<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-22a — Remove a candidate, Readers read-only

> **Sprint:** 06, task S6-22 (3 points), part a of the split the owner confirmed on 2026-11-19 (D1 (a)). Started on `main` at `4f17851d` (S6-21b merged, #426). Documents: TESTING v110, ARCHITECTURE v105, SPEC v2.67, UX copy v48, Handoff 1.45, Figma mapping 1.35 (unchanged), CODEBASE §2.112. **Issues:** none.
> **Requirements / invariants:** R-CAN-09, R-CAN-10. **WSP-04** `active` for its workspace part (the published-entry part deferred to step 6); **TEN-05** complete for the workspace endpoints of step 5. API-03, TEN-01, SEC-10, A11Y-01, A11Y-08, A11Y-09 gain cases. A11Y-02 for J2 follows with S6-22b.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **No EF migration** (the cascades of S6-15 and S6-18 do the work), no new dependency, no new project. Four new UX copy keys (v48). E2E run locally in Chromium, Firefox and mobile Chrome; WebKit and mobile Safari left to CI.
> **Scope:** `DELETE /api/v1/candidates/{id}` with its tests and the matrix rows; the removal on S34 Überblick with its confirmation; the read-only view of S34a and S34b for Readers and archived projects. Not in scope: the keyboard-only journey J2 up to the assessment (S6-22b, the M5 exit check); everything about published entries (step 6).
> **Owner decisions (2026-11-19, „go on with your recommendations“):** **D1 (a)** — split: S6-22a server, WSP-04, TEN-05, the dialog and the Reader views; S6-22b the journey J2 with A11Y-02 · **D2 (a)** — new display keys for the read-only tabs with the proposed wording: `candidate.evidence.version.heading` „Geprüfte Version“, `candidate.evidence.url.heading` „Link zum Prüfbericht“, `candidate.evidence.qualification.heading` „Notiz zur Qualifikation der prüfenden Stelle“, `candidate.assessment.note.heading` „Begründung“; intro and input hints left out for Readers · **D3 (a)** — `If-Match` with the candidate's tag on the `DELETE`; a 409 shows the general error, as S14's delete · **D4 (a)** — „Produkt aus dem Projekt entfernen“ in its own last row below the four cards of Überblick, outlined.

## 0. Plan (as given to the owner)

- **Server:** the `DELETE` for Editor and Owner; evidences, private rows and assessments go by the database's cascades.
- **Client:** `CandidatesApiService.remove`, the button and `ConfirmDialog` on Überblick, S32 and the announcement afterwards; read-only branches in the tabs Prüfnachweis and Einschätzung.
- **Tests:** WSP-04 and the removal's cases in `CandidateEndpointsTests`, six matrix rows, the removal in the API-03 theories; component specs; E2E for the removal and the Reader view with axe, a reflow case.
- **Invariants:** WSP-04 `active` (workspace part), TEN-05 complete. **Migration:** none.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | No `DELETE /candidates/{id}`. The foreign keys of the private section, the evidences (with their notes) and the assessments cascade from the candidate (S6-15, S6-18) | One `Remove` and one save; no migration |
| F2 | `AuthorizationMatrixTests` had rows for all 14 workspace endpoints | Only the six rows of the new endpoint were missing for „TEN-05 complete“ |
| F3 | For a Reader S30, S32, S33 (refused), S11 of the copy and S34 Überblick were read-only already; S34a and S34b showed an **empty tab**, also in an archived project (S6-17 D5 (a), S6-19 Q3 (a)) | The task builds the two read-only views; they also serve archived projects |
| F4 | The UX copy has `candidate.remove.*` and `candidate.removed`, but no text for a stored optional evidence fact or a stored note: the labels describe inputs („… (optional)“, „Ihre Notiz …“, „Worauf stützt sich Ihre Einschätzung? (optional)“) | Stopped and asked (CLAUDE §6.5): D2 (a), UX copy v48 U190, U191 |
| F5 | Neither the removal nor the Reader view has a Figma frame; the frame of S34 Überblick (`2097:2627`, looked at through the Figma connection) shows no remove button | Handoff §5.3 patterns; D4 (a) puts the button below the cards; open point 2 |
| F6 | SPEC and ARCHITECTURE did not say whether the `DELETE` needs `If-Match`; `DELETE /test-evidences/{id}` and `DELETE /configurations/{id}` do | D3 (a): required, as every change of a shared resource (API-03) |
| F7 | No candidate can be published before step 6 | The published-entry clauses of WSP-04 and of R-CAN-09 are recorded as deferred (TESTING WSP-04, SPEC v2.67-2); `candidate.remove.publishedNote` stays unused |
| F8 | An announcement made right after a route change is followed in the live region by the page title (the same holds for `projects.created` since S6-13) | Both are read in order; the E2E test records the live region instead of reading its last text |

## 2. Changes

- **Server (Workspace):** `CandidateEndpoints` maps `DELETE /candidates/{id}` (`DeleteCandidate`): role check through `WorkspaceAccess` with the organisation of the stored candidate, `ConfirmedEmail`, 428 without `If-Match`, then `CandidateService.RemoveAsync` (`ConcurrentChange.RunAsync`): 404, 409 `conflict` with the editor, 409 `project-archived`, 204; the project is stamped (`MarkChanged`).
- **Client, removal:** `CandidatesApiService.remove(id, etag)`; `CandidateOverview` — the button after the last card (same `canEdit()` as „Bearbeiten“), `remove()` with `ConfirmDialog`, navigation to S32 and `candidate.removed`; `not-found` counts as removed; after another failure the candidate is read again into `CandidateLoad`.
- **Client, read only:** `CandidateEvidence` gets a second template branch (`<dl>` lists, `storedHeading`, `storedKindKey`, `date`); `CandidateAssessment` shows its content to every member and switches intro, filter, union note, K6 / read-only card and the link of the derived rows on `editable()`.
- **Texts:** UX copy v48 (U190, U191), the four keys in `de.json` through `check-i18n.mjs --write`.
- **Tests (.NET):** `CandidateEndpointsTests` — `Remove_EditorOfA_Returns204AndRemovesEvidencesNotesAndAssessments` and `Remove_Twice_Returns404TheSecondTime` (`INV:WSP-04`), the removal in the archived-project, unconfirmed-address (`INV:SEC-10`) and other-organisation (`INV:TEN-01`) cases; `AuthorizationMatrixTests` `remove-candidate` (six rows, `INV:TEN-05`); `WorkspaceConcurrencyTests` `Change.RemoveCandidate` (three `INV:API-03` theories).
- **Tests (client):** `candidate-overview.spec.ts` (+6), `candidate-evidence.spec.ts` (+5, replacing the two „no form“ cases), `candidate-assessment.spec.ts` (+4, replacing the two „shows nothing“ cases), `candidates-api.service.spec.ts` (+1).
- **E2E:** `e2e/workspace/candidates.spec.ts` (+2: the removal; a Reader in a second browser context, axe in both appearances on `s34-remove`, `s34a-readonly`, `s34b-readonly`), `e2e/shell/reflow.spec.ts` (+1), `e2e/support/accounts.ts` `acceptInvitationThroughApi` (taken out of `conflict.spec.ts`, which uses it now).
- **Documents:** TESTING v110 (V397 – V403, WSP-04 and TEN-05 rows, §9; v108 deleted), ARCHITECTURE v105 (C193; v103 deleted), SPEC v2.67 (v2.67-1 – -4), UX copy v48 (v46 deleted), Handoff 1.45 (new row in §5.3, the S34b row), CODEBASE §2.112 (pointers in §2.98, §2.101, §2.103, §2.107, §2.108, §3), SPRINT_06 (the split in the row). Figma mapping unchanged (no mapped node or component changed). `CLAUDE.md` unchanged: no rule, command or convention across the codebase changed.

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security / tenancy:** the organisation comes from the stored candidate through the query filter (404 for another organisation, the header changes nothing — tested); the role is checked before the tag and before the archive state (428 never tells a Reader anything — tested); antiforgery by the middleware, `ConfirmedEmail` on the endpoint. The Reader view shows nothing a Reader could not read before: `GET /candidates/{id}` and `…/assessments` already answered every member, private fields included (they are the organisation's, PRV-01 is about publication).
- **Correctness / concurrency:** the removal loads the candidate with its evidences, so a concurrent change of the candidate or an evidence fails the save's concurrency token and the retry answers 409 with the new editor; a second removal answers 404; an assessment written in the same moment is refused by its foreign key (the row cannot outlive the candidate). The lock of WSP-02 is released because it reads the rows that are gone (tested).
- **Requests / N+1:** one read and one save on the server. On the client one `DELETE`; one `GET` only after a refused removal.
- **Edge cases:** double click (guard while the dialog or the request is open); the candidate removed by someone else (counts as removed); a stale tag (general error, product read again, the next try works — unit); archived project and Reader (no button; the server refuses anyway).
- **A11y:** the dialog opens with the focus on its title and returns it to the button on Escape; the button is last in DOM and tab order; after the removal the focus is on S32's H1 and the removal is announced. Read-only views: headings (H2/H3 in S34a, H2 per criterion in S34b), `<dl>` terms, no disabled controls, reflow at 320 px, axe in both appearances and in the component tests.
- **Found while testing and fixed:** (1) the E2E test read the live region's last text and found the page title of S32 — the removal is announced before it (F8); the test records the live region now. (2) A component test expected the collapsed legend's button text although the legend starts expanded from 600 px.
- **Won't fix:** (1) S37 on a stale removal — decided (D3 (a)). (2) A Reader's opening or closing of the legend on S34b stores the choice in their own settings, as for an Editor: it is their setting. (3) The Reader view of S34b has no filter „Nur noch nicht eingeschätzte“ — R-CAN-10 says no inputs; if Readers miss it, it is a small follow-up. (4) `CandidateEvidence` still fills its unused form controls for a Reader (as before this task): no effect on what is shown.
- **Follow-up:** open points 1 – 4.

## 4. Checks

- .NET: `dotnet format --verify-no-changes` clean; `dotnet build -warnaserror` 0 warnings; `dotnet test --solution ProcureAccess.sln`: **2 178 passed**, 0 failed (unit, architecture, integration). The candidate, matrix and concurrency classes alone before that: 524 passed.
- Client: `npx ng test` **1 155 passed** (108 files); `npm run lint` clean; `npm run build:release` clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** (throwaway PostgreSQL and Mailpit containers, the API with `dotnet run` and the environment of `docker-compose.e2e.yml`, `ng serve --port 4300`): the three new tests, the first test of `candidates.spec.ts`, `conflict.spec.ts` and the S32 – S34 cases of `reflow.spec.ts` in Chromium, Firefox and mobile Chrome — **30 passed**. WebKit and mobile Safari cannot run on this machine: CI.
- `check-invariants` (gate passed; WSP-04 has its tests), `check-figma-links` (12 checked, 0 pending), `check-i18n` (0 errors, UX copy v48), `check-icons` (0 errors); `reuse lint` (Docker image): compliant, 1 750 files.
- **Figma:** the frame of S34 Überblick (`2097:2627`) looked at before building: it has no remove button and no Reader variant; the built button sits below its last card and changes nothing above. No frame to compare the read-only tabs with (open point 2).

## 5. Open points

1. **S6-22b:** the journey J2 up to the assessment, keyboard only, with axe in both appearances on five browser projects (A11Y-02; the M5 exit check). It can use `acceptInvitationThroughApi` and the Reader assertions of `candidates.spec.ts`.
2. **No Figma frame** for „Produkt aus dem Projekt entfernen“ and for the read-only tabs (Handoff 1.45 describes them as built). Proposed: add them to the frames the owner reviews with TD-80.
3. **Step 6:** the published entry of a removed candidate (WSP-04's second half, R-CAN-09's second criterion, `candidate.remove.publishedNote`), recorded in TESTING WSP-04 and SPEC v2.67-2.
4. **An announcement after a route change** is followed at once by the page title (F8; also `projects.created`). Both are read by a screen reader that queues polite announcements; whether every screen reader does belongs to the tester's pass (#311, TD-49). No change proposed now.
5. **`CLAUDE.md` size limit** (S6-21a open point 4) — unchanged; this task did not touch it.

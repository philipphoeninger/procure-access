<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-17 — Evidence tab S34a

> **Sprint:** 06, task S6-17 (1.5 points), after S6-16 (#415) and S6-15 (#409); started on `main` after S6-16, rebased onto `main` after S6-20a (#416) was merged during the task — documents renumbered (TESTING v103, ARCHITECTURE v98, SPEC v2.60, CODEBASE §2.105) and the requests send `If-Match` (F10). **Issues:** none.
> **Requirements / invariants:** R-CAN-05 (UI); WSP-01 stays green and gains its client cases; A11Y-01, A11Y-08, A11Y-09, API-02 and SEC-10 gain cases. No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **No EF migration, no server change, no new dependency.** E2E not run locally: another session used the local E2E stack (owner instruction); left to CI.
> **Scope:** the tab „Prüfnachweis“ of S34: test status radio group, evidence cards with H3 and fields in fixed order, „Andere Prüfung“ with its required name, the duplicate procedure, the question when leaving „Ja“, one „Speichern“. Not in scope: the publish button and its hint (step 6), the Reader view (S6-22).
> **Owner decisions (with the plan, 2026-11-18; D6 2026-11-19):** **D1 (a)** — one „Speichern“ for the tab, as the frames; the client sequences the endpoints of S6-15 · **D2 (a)** — new keys `candidate.evidence.list.statusChange.title` „Prüfnachweise entfernen?“ and `.statusChange.confirm` „Nachweise entfernen“ · **D3 (a)** — new key `candidate.evidence.saved` „Prüfnachweis gespeichert.“, announced, the focus stays · **D4 (b)** — „Ja“ without evidences shows one empty card, skipped on „Speichern“ while blank · **D5 (a)** — Readers and archived projects see the empty tab until S6-22 · **D6 (a)** — „Datum der Prüfung“ as a TT.MM.JJJJ text field without Material's calendar, whose texts the UX copy does not have. The defaults stated with the plan were accepted: the procedure list as a client copy of the seeded keys, the private note with „(optional)“ and the hint „Nur für Ihre Organisation: …“, summary lines named with their card. UX copy v44 U177 – U180.

## 0. Plan (written before the code)

- **Starting point:** the server of S6-15 (`PUT /candidates/{id}/test-status` with `removeEvidences`, 409 `test-status-conflict` with `evidenceCount`; `POST /candidates/{id}/test-evidences`, `PUT`/`DELETE /test-evidences/{id}`; a second evidence of a procedure is 400 at `testTypeId`; „Ja“ with no evidence allowed). On the client the tab was `CandidateTabPlaceholder` (S6-16).
- **Files:** models, API service, the tab `pages/candidate-detail/evidence/`, the routing, specs, E2E journey and reflow, documents.
- **Invariants:** none flipped; WSP-01 gains client cases.
- **EF migration:** none.
- **Session:** one (fallback split: a = radio group, dialog and API; b = the cards). It fitted one, with a pause in between.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The frames S34a (`2022:267`) and S34a-L (`2144:3508`) have one „Speichern“ for status and cards; the server has one endpoint per change | Asked: D1 (a) — the client sequences the requests (ARCHITECTURE C186 (2)) |
| F2 | UX copy v43 had the text of the question when leaving „Ja“ but no title, and its button „Nachweise entfernen“ only in the note column; `ConfirmDialog` needs both | Asked: D2 (a); U177, U178 |
| F3 | No text confirms a saved evidence; S34a stays on the page after saving | Asked: D3 (a); U180 |
| F4 | „Prüfnachweis {{n}}“ (H3 without a kind) stood only in the note column of `candidate.evidence.list.item` | Key `candidate.evidence.list.item.untyped` as S6-16 D2 (a) did for such texts; U179 — **not in the plan**, reported here |
| F5 | WSP-01 allows „Ja“ with no evidence, while the frames show a card as soon as „Ja“ is chosen and the button says „*Weiteren* Prüfnachweis hinzufügen“ | Asked: D4 (b) |
| F6 | The Reader view of every tab is S6-22's | Asked: D5 (a) |
| F7 | `GET /catalog/filter-options` answers 404 without an active release — the normal state outside tests (SPRINT_06 R9) — so it cannot feed „Art der Prüfung“ | The five keys the catalog seeds (`FilterValueSeed.TestTypes`), identical to `testType.*`, as `TEST_PROCEDURES` on the client; open point 1 |
| F8 | Material's calendar popup has about ten texts of its own (`MatDatepickerIntl`: „Previous month“ …); the UX copy has only `common.date.openCalendar` | Asked during the task: D6 (a) — a text field; open point 2 |
| F9 | The UX copy asks for `common.requiredNote` above forms with required fields; the frames do not show it | Shown under the H2 „Prüfnachweise“, as the rule of U176 says; Handoff 1.39 records it |
| F10 | S6-20a (#416) was merged during the task: `If-Match` on `PUT …/test-status` (the candidate's tag) and on `PUT`/`DELETE /test-evidences/{id}` (the evidence's own tag, a body member); adding or removing an evidence changes the candidate's tag, and neither answer carries it; its open point 3 leaves the evidence requests to S6-17 | Rebased; the save sequence changed from „removals → status → cards“ to **status → removals → cards**: leaving „Ja“ sends the status with `removeEvidences: true` instead of separate `DELETE`s, which would make the candidate's tag stale; an ETag `conflict` shows the general text until S37 (S6-21). No owner decision needed: the plan's D1 (a) stays, only the order follows the server (C186 (2)) |

## 2. Changes

1. **Models** `features/workspace/models/candidates.ts`: `TEST_PROCEDURES`, `EVIDENCE_LIMITS`, `TestEvidenceRequest`.
2. **API** `services/api/candidates-api.service.ts`: `changeTestStatus` (`SAVE_TEST_STATUS_PROBLEMS`, `If-Match` with the candidate's tag, keeps the new tag), `addEvidence` (no tag), `changeEvidence` and `removeEvidence` (`If-Match` with the evidence's own tag; `removeEvidence` also handles `not-found`); `ProblemDetails.evidenceCount`.
3. **Tab** `pages/candidate-detail/evidence/candidate-evidence.{ts,html,scss}` and `evidence-card.ts` (C186): the radio group in a card, the details hint, H2, `common.requiredNote`, the cards (focusable H3; Art der Prüfung with the five procedures, „Andere Prüfung“ with its hint line and a top border instead of a divider element, „Eigene Prüfung“; the required name after the select for „Andere Prüfung“; Wer hat geprüft? | Datum, Version | Link in pairs from 600 px; the private note with „(optional)“ and „Nur für Ihre Organisation: …“; „Prüfnachweis n entfernen“), „Weiteren Prüfnachweis hinzufügen“, „Speichern“. The save sequence (D1 (a), F10): the status with the candidate's tag when it changed (leaving „Ja“ with `removeEvidences` when saved evidences were removed; 409 `test-status-conflict` → dialog with the server's count → again with `removeEvidences`) → removals while „Ja“ stays, each with its tag (404 counts as removed) → new and changed cards in card order; a refused card keeps its input; the candidate is read again for the other tabs; `candidate.evidence.saved`. Errors with the S20 pattern; the page title gets „Fehler:“.
4. **Shared:** `shared/date/german-date.ts` (TT.MM.JJJJ ↔ `yyyy-MM-dd`, today); `ErrorSummaryItem.field.group` and the summary's line „Prüfnachweis n – Feld: …“; `candidateTabTitle(tab, error?)`.
5. **Routing:** `pruefnachweis` → `CandidateEvidence`; the placeholder stays for `einschaetzung`.
6. **Texts:** UX copy v44 (U177 – U180), `de.json` through `check-i18n.mjs --write`; v42 deleted.
7. **Tests:** `candidate-evidence.spec.ts` (16 cases), `german-date.spec.ts` (3 groups), two new cases in `candidates-api.service.spec.ts` (with the `If-Match` of each request). E2E: the journey of `e2e/workspace/candidates.spec.ts` goes on in S34a (save an evidence, a refused duplicate, leaving „Ja“ through the dialog; axe on three new states); `e2e/shell/reflow.spec.ts` gets S34a with two long cards, the open list and the error summary, and shares the answers of the S33/S34 case (with S6-20a's ETag headers) through `routeCandidate`.
8. **Documents:** TESTING v103 (V363 – V367; v101 deleted), ARCHITECTURE v98 (C186; v96 deleted), SPEC v2.60 (v2.60-1), Handoff 1.39 (§5.3 „S34a as built“), Figma mapping 1.30, CODEBASE §2.105 and §3. **`CLAUDE.md` unchanged:** after S6-20a it has 39 993 characters, and naming S34a in its state line would pass the 40 000 at which Claude Code warns (open point 6).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security:** no new endpoint; every request goes through S6-15's checks (role, organisation from the resource, antiforgery, confirmed address, archived project). The form is hidden for Readers and archived projects, but the server decides. Private note shown only here (PRV-01). No HTML from user input; report links are only input values here.
- **Correctness / edge cases:** a blank new card is never sent, a partly filled one is checked (D4 (b)); duplicates are found across cards before sending; the status goes first with the tag the tab read, so it never removes an evidence the user has not seen (the tag changes with every evidence added or removed, S6-20a), and leaving „Ja“ removes the user's removals with it; a `test-status-conflict` asks again with the server's count (evidences another session added); a refused card keeps its input and the rest stays saved; the form is filled once per candidate, so the refresh after saving does not drop what failed.
- **Requests:** one per removed, new or changed card plus the status and one read; bounded by the few cards of a product.
- **A11y:** fieldset/legend for the radio group with its hint linked; H2 → H3 per card, focusable for the focus after a removal; the add button moves the focus to the new select; the summary takes the focus and names each line's card; announcements through `A11yAnnouncerService`; axe in component tests and three new E2E states in both appearances.
- **Fix (found in the review, fixed):** a `DELETE` of an evidence another session had removed answered 404 and kept its id queued, so every further „Speichern“ would have failed at the same place — a 404 now counts as removed (test „takes an evidence another session removed meanwhile …“).
- **Found while testing and fixed:** `#addButton` on a Material button is the `MatButton`, not the element (`read: ElementRef`); the two lines of „Andere Prüfung“ have no space in `textContent` (test reads the spans); a test of the radio group reset needed a dialog that answers after a change detection, as a real one does.
- **Won't fix:** (1) a server field message (e.g. a date that is after today in Berlin but not yet in the browser around midnight) shows as the server's text, as on S33 (`errors.field.server`) — the client checks the same rules first. (2) Swapping the procedures of two saved cards in one „Speichern“ gives a duplicate at the first card, because the cards are sent one by one; very unlikely, open point 4. (3) After removing a card, the error summary keeps its lines until the next „Speichern“ — the summary does not change while the user edits (`ErrorSummary`). (4) Leaving the tab drops unsaved input without a warning, as on S33 (no text in the UX copy).
- **Follow-up:** open points 1 – 5.

## 4. Checks

- Client (after the rebase onto S6-20a): `npx ng test` **1 040 passed** (103 files); `npm run lint` clean; `npx ng build --configuration development` clean. Before the rebase a full run had three configurator specs time out at a load average of 19 (another session ran E2E); they passed alone and in every later run.
- e2e: `npm run typecheck`, `npm run lint` clean. **E2E not run locally** — another session used the local E2E stack (owner instruction); left to CI.
- `check-invariants` (gate passed), `check-figma-links` (9 checked, S34a tagged), `check-i18n` (0 errors), `check-icons` (0 errors): green.
- Backend unchanged (no `dotnet` change in this PR).
- `reuse lint-file` on every new file: compliant (worktree; CI runs the full `reuse lint`).
- **First CI run of #418:** every stage green except E2E, two tests in all five browsers. (1) The journey still waited for a `DELETE` that the order of F10 no longer sends — a test left from before the rebase; it waits for the status request now. (2) The S34a reflow test: 341 px at 320 px — the floated label of the private note (436 px of text) is cut off by Material but its text counted as page overflow; found by rebuilding the DOM snapshot of the trace and measuring it in a local headless Chromium (no stack started); the evidence fields now clip their horizontal overflow (V367). (3) Firefox only: the axe run `s34a-evidence` crashed the page process after name lookups of the S34a form fields — the known class of V226; the S34a part of the journey uses CSS locators up to its last axe run (V367).
- **Figma:** S34a (`2022:267`), S34a-J (`2094:2525`) and S34a-L (`2144:3508`) compared before building through the Figma MCP (screenshots). The rendered result was compared by structure, not side by side in a browser: no stack was started in this session (owner instruction). Differences kept on purpose: no publish button and hint (step 6), no breadcrumb (S6-14), `common.requiredNote` under the H2 (F9), no calendar toggle (D6 (a)), the procedure list's divider as a border; S34a-J's heading „Angaben zur Prüfung“ is the older one-evidence layout that S34a-L replaced (H2 „Prüfnachweise“, U56).

## 5. Open points

1. **The procedures without an active release:** „Art der Prüfung“ reads the five seeded keys from `TEST_PROCEDURES`; the server could serve the procedures (filter values, which belong to no release, C104) without an active release, e.g. `GET /catalog/test-types` — then the client copy goes. Proposed as a small follow-up issue.
2. **The calendar of the date field (D6 (a)):** the UX copy needs German texts for Material's `MatDatepickerIntl` (about ten: previous/next month, choose month and year, close calendar …), then a `MatDatepickerIntl` provider and the toggle `common.date.openCalendar` with `calendar_today` (Handoff §1.5) join S34a and S55.
3. **The publish button and `candidate.publish.cta.hint`** of S34a come with S35 in step 6; **the Reader view** with S6-22 (D5 (a)).
4. **Sending the cards one by one** is not atomic (C185 (2)); swapping two procedures in one save needs two saves. If that turns out to matter, a batch endpoint with the ETag rules of S6-20 would solve it.
5. **Rendered comparison with the frames** (§4): to be done in the owner's review or by the next session that may start a local stack.
6. **`CLAUDE.md` is full** (39 993 characters after S6-20a): S34a is not named in its state line, which would pass 40 000. The line „State of the code“ should move to CODEBASE (as S6-16's open point 5 proposed), with the next task that changes `CLAUDE.md`.
7. **Long floated labels are cut off at 320 px** (400 % zoom): „Ihre Notiz zur Qualifikation der prüfenden Stelle (optional)“ shows as „Ihre Notiz zur Qualifikation der prüfend…“ inside its field (Material's ellipsis; S33's longest labels are just short enough). The accessible name stays complete, but sighted users at 400 % lose part of the label. Options for the Handoff: the label above the field for long labels, or a shorter wording in the UX copy — a design decision, not taken here.

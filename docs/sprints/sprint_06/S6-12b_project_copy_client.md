<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-12 — Project copy of a criteria list (part b: client)

> **Sprint:** 06, task S6-12, part b (client) after S6-12a (server, #410). **Issues:** none.
> **Requirements / invariants:** R-PRJ-02 (the list choice in S31), R-PRJ-06 (UI: S11 of the project's copy, `projects.detail.criteriaAdded`). No invariant changes status: **WSP-03** (active since S6-12a) gains its client and E2E cases; WSP-02's client case stays untagged until S6-18 flips it; TEN-01, A11Y-01, A11Y-08 gain cases.
> **Status:** S6-12b done by Claude Code (2026-11-18) — pull request open; started stacked on #410 (S6-12a was not merged yet) and rebased onto main after #410 was merged. **No EF migration, no new dependency.**
> **Scope (b):** S11 in project-copy mode at `/projekte/:id/kriterienliste`, the list choice of S31 with the copy after creating the project, `projects.detail.criteriaAdded` on return to S32, and the way from S32 to S11 until S6-14.
> **Owner decisions (2026-11-18, with the plan):** **D5 (b)** — a list that cannot be copied after the project was created: S32 of the new project opens with the general notice, the project has no list · **D6 (a)** — S11 of a copy gets its own page title, wording „Kriterienliste – {{project}} – Beschaffungsprojekte“ (UX copy v42 U169) · **D7 (a)** — `/kriterienlisten/<id of a copy>` leads to `/projekte/:id/kriterienliste` · **D8 (a)** — the S32 placeholder gets a short section „Kriterienliste“ until S6-14 · **„Keine Liste“** as the first option of S31's select (`projects.create.criteria.none`, U170; asked during the work). From S6-12a: D3 (a) — the copy of an archived project is read-only in the client; D4 (a) — the address of S11 of a copy.

## 0. Plan (written before the code)

- **Starting point:** S6-13 (#407) and S6-15 (#409) merged; S6-12a (#410) open and in conflict with main, so its CI never started. Main merged into the S6-12a branch first (renumbering, see S6-12a open point 6), pushed; this branch starts on that commit.
- **Code:** a project-copy mode of the configurator's `SavedListPage`, reached through a new lazy route under S32; `withWorkspace()` assigns lists and keeps the report of added criteria; S31 gets the select; the S32 placeholder a section.
- **Tests:** component tests of S11 in copy mode, S31, S32, the store, the API service and both resolvers; one E2E journey. E2E left to CI.
- **Session:** one.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The S6-12a server answers `criteriaList` on `GET /projects/{id}` and `procurementProjectId` on `GET /configurations/{id}`; the client models had neither | Both fields in the models |
| F2 | S11 of a saved list (`SavedListPage`, S5-21b) already decides each criterion right away and hides editing for a Reader | The copy is a mode of that page, not a new page; the configurator's loader and editor are reused |
| F3 | The UX copy had no page title for S11 of a copy (`pageTitle.criteria.list.saved` names S14) and no text for „no list“ in S31's select | Asked: D6 (a) and „Keine Liste“; UX copy v42 U169, U170 |
| F4 | MatSelect shows an option with the value `null` as no choice at all | „Keine Liste“ has the value `''` (`NO_LIST`); the lists have UUIDs |
| F5 | The general notice of the error interceptor is `ERROR.GENERIC` in a snackbar; a 5xx would send the user to S80 („Ihre Eingaben wurden nicht gespeichert“), wrong after the project was created | The PUT is `CALLER_SHOWS_FAILURES`; S31 shows the general notice itself after opening S32 (D5 (b)) |
| F6 | S32 is a placeholder until S6-14 and had no way to S11 | D8 (a): a short section with the existing keys |
| F7 | Angular 22 resolvers may return a `RedirectCommand` | D7 (a) without a guard of its own |
| F8 | The root partition had about 2 GB free | E2E not run locally (memory: the E2E stack needs ~2 GB per build); CI runs it |

## 2. Changes

1. **Models.** `SavedList.procurementProjectId`; `Project.criteriaList` (`ProjectCriteriaList`); `PROJECT_CRITERIA_LIST_SEGMENT`, `projectCriteriaListPath`.
2. **API and store.** `ProjectsApiService.assignCriteriaList` (PUT, `CALLER_SHOWS_FAILURES`). `withWorkspace()`: `assignCriteriaList`; `workspace.criteriaAdded` with `noteCriteriaAdded` and `takeCriteriaAdded` (one waiting report, for one project, taken once; `resetWorkspace` clears it).
3. **S11 of a copy.** `features/workspace/pages/project-criteria-list/project-criteria-list.routing.ts`, lazy child `kriterienliste` of `PROJECT_ROUTES`: the resolver reads the project (its id from `pathFromRoot`), then the copy through `loadSavedListView`, waits for the configurator settings and adds `project` (id, name, archived) to the view; a project without a list is „not found“; title `pageTitle.project.criteria`. `SavedListPage`: `project()`; `canEdit` false for an archived project; `listActions` (change selection, share, older release) only for lists of the organisation; notices `criteria.list.projectCopy` and, archived, `projects.archived.note`; title parameter `project`; `reportAddedCriteria` on leaving (and before showing another list): the criteria left out when the page showed and included now.
4. **Old address (D7 (a)).** `savedListResolver` answers a copy's id with `RedirectCommand` to the project's address, `replaceUrl`.
5. **S31.** `mat-select` „Kriterienliste (optional)“ with „Keine Liste“ (preselected) and the organisation's saved lists by name (`localeCompare('de')`); left out when there are none or they cannot be read. After the create, the chosen list is assigned while the button keeps its spinner; a failed copy → S32, `projects.created`, then the snackbar with `ERROR.GENERIC`.
6. **S32 placeholder.** Section „Kriterienliste“: H2, `<dl>` with the list name (link to S11) and its release, `projects.detail.criteriaAdded` (notice and announcement, `pluralKey`), „Kriterienliste anpassen“ for Editor and Owner of a project that is not archived; without a list `projects.detail.noCriteria` without its button. Styles with tokens; dt above dd below 600 px.
7. **Texts.** UX copy v42 (U169 `pageTitle.project.criteria`, U170 `projects.create.criteria.none`), `de.json` through `check-i18n.mjs --write`; v40 deleted.
8. **Tests.** `saved-list-page.spec.ts` (+5: copy mode, inclusion reported, net count, archived, Reader), `saved-list.routing.spec.ts` (+1: redirect), `project-criteria-list.routing.spec.ts` (new, 5), `create-project-dialog.spec.ts` (+5), `project-detail.spec.ts` (+6), `with-workspace.spec.ts` (+3), `projects-api.service.spec.ts` (+1). E2E `e2e/workspace/projects.spec.ts` (+1 journey).
9. **Documents.** TESTING v98 (V339 – V341; v96 deleted), ARCHITECTURE v93 (C181; v91 deleted), SPEC v2.55 (four rows), Handoff 1.37 (S31 as built, S32 placeholder, new row „S11 of a project copy as built“), CODEBASE §2.100 and §3, CLAUDE.md §3 state line, the `PENDING` note of `check-figma-links.mjs`.

## 3. Self-review (CLAUDE §6.7, §6.8)

- **Security:** no new endpoint; the copy is read through the existing endpoints with their checks (TEN-01: another organisation's project answers 404 → „Seite nicht gefunden“). Role and archived state only hide controls; the server decides (S6-12a, and its open point 2 for an archived copy).
- **Correctness:** the count of added criteria compares the decisions shown when the page opened with those shown when it is left; a save the server refused is undone by the editor before that, so only saved inclusions count. The report is taken before S32 renders: the router deactivates S11 before it activates S32 (the E2E checks it with the browser's back, in CI).
- **N+1:** S11 of a copy reads the project and then the list (two requests, the second needs the first); S31 reads the lists once per opening.
- **A11y:** the select has its label and hint; the S32 section has a heading and a description list; the report is announced once; the E2E runs axe in both appearances on four new states.
- **Found and fixed during the work:** „Keine Liste“ as `null` showed an empty select (F4); the doc comment of `SAVED_LIST_ROUTES` was lost in the resolver change and restored.
- **Found by CI and fixed:** (1) the two reflow tests of a saved list (`e2e/shell/reflow.spec.ts`) answered `GET /configurations/{id}` without `procurementProjectId`, so the new redirect sent them to `/projekte/undefined/kriterienliste`; their answers now carry `null`, as the API does. (2) Firefox crashed its page process in axe in the new journey — the known class of TESTING V129/V165 (a name check of a Material control labelled by `aria-labelledby`): the journey finds S31's select as the dialog's only combobox and the include checkbox by role inside its card, as J1 does. A second CI run still crashed Firefox after the axe run on S31 (the trace: name lookups of the dialog and its button after axe), which did not help: the third run crashed at the same place. Reproduced locally (Firefox, the API and client from source with the E2E settings) and narrowed down with throwaway variants (TESTING V341): axe on S31 with its `mat-select` crashes Firefox when the dialog closes afterwards, or during axe when the select was found by name before. S31's axe run is now a test of its own that finds the select by CSS and ends with axe; the journey checks S31 without axe — 24 of 24 local Firefox runs green.
- **Won't fix:** two links on S32 lead to S11 (the list name and „Kriterienliste anpassen“), as the Handoff's criteria card has them. A user who includes and excludes the same criterion within one visit sees no report (net count), which matches „neu in der Liste“.
- **Follow-up:** open points 1–4.

## 4. Checks

- Client: `npx ng test` **962 passed** (94 files); `npm run lint` clean; `npm run build:release` complete.
- e2e: `npm run typecheck`, `npm run lint` clean. **E2E not run locally** (disk, F8): left to CI.
- `check-invariants` (no warning; the WSP-02 client case untagged), `check-figma-links`, `check-i18n`, `check-icons`: green. `check-figma-links` self-test green.
- Backend unchanged since S6-12a's merge commit (2 011 passed there).
- `reuse lint-file` on the new files (routing, its spec, the protocol, ARCHITECTURE v93, TESTING v98, UX copy v42): compliant (worktree; CI runs the full `reuse lint`).

## 5. Open points

1. **Assign or replace the list from S32** (`projects.detail.noCriteria.cta` „Kriterienliste zuordnen“, R-PRJ-04 in the UI, `projects.detail.criteriaLocked` once assessments exist): needs a choose dialog that neither the UX copy nor the Handoff describes. Proposed for S6-14 (it builds the criteria card) or a follow-up issue with a design decision.
2. ~~**Rebase after #410.**~~ Done: #410 was merged and this branch was rebased onto main; the PR holds only S6-12b.
3. **S11 of a copy in the reflow run** (`e2e/shell/reflow.spec.ts`): S11 is already there as a saved list; the copy mode adds two notices only. S6-14 can add it with S32.
4. **The general notice is a legacy key** (`ERROR.GENERIC`, D5 (b)); when the interceptor's general text moves to the UX copy, this case follows it.

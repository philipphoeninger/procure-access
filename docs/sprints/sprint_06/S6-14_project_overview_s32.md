<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-14 — Project overview S32

> **Sprint:** 06, task S6-14 (3 points), after S6-12 (#410, #411), S6-13 (#407), S6-18 (#413) and S6-16 (#415); started on `main` at `aefaf4d`; `main` merged in after S6-20a (#416) was merged during the task — documents renumbered (TESTING v103, ARCHITECTURE v98, SPEC v2.60, CODEBASE §2.105) and the dialog sends `If-Match`. **Issues:** none.
> **Requirements / invariants:** R-PRJ-03, R-PRJ-05 (UI); R-PRJ-04 (UI) in part — the lock notice; SIG-13 (UI part); A11Y-01, A11Y-09. No invariant changes status: SIG-13 (active since S6-18) gains its S32 cases; WSP-01, WSP-02, API-03, A11Y-01, A11Y-08, A11Y-09, TEN-01, API-02 and SEC-10 gain cases.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **No EF migration, no new dependency.** A small server change in the Workspace module (two members of the project answer). **E2E not run locally:** another session used the local E2E stack (owner instruction); the specs are written, type-checked and linted, and run in CI. S32 was compared with its frames in a browser on a private stack of its own (own PostgreSQL container on port 55432, API on 5100, `ng serve` on 4300), not on the shared E2E stack.
> **Scope:** exactly the row of S6-14 in `SPRINT_06.md`: S32 with the criteria card (scope row with „Geprüft nach …“ named apart, the link to S11 of the copy), the product list (the name the only link to S34, the counts row with seven values, label first), the add-product menu, renaming and the status change, archived projects read-only.
> **Owner decisions (2026-11-18, with the plan, all as recommended):** **D1 (a)** — dialog „Projekt bearbeiten“ with new keys `projects.edit.title`, `.error.title`, `.error`, `.saved` (wording as proposed), reusing the S31 name label, `projects.status.*` and `common.actions.*` · **D2 (a)** — assigning or replacing the list from S32 is not built here; S32 without a list shows `projects.detail.noCriteria` without its button; a follow-up issue · **D3 (a)** — „Aus dem Produktverzeichnis übernehmen“ hidden until M6; the menu holds „Neu erfassen“ only · **D4 (a)** — the project answer carries the evidence kinds per candidate and `criteriaListLocked`, no migration · **D5 (a)** — `projects.detail.row.publication.none` „Nicht veröffentlicht“; Ja/Nein/Nicht bekannt reuse `candidate.evidence.tested.*`, the evidence kinds `testType.*` · **D6 (a)** — `pa-breadcrumb` on S32 and S34 with `common.breadcrumb` „Sie sind hier“. UX copy v44 U177 – U181.

## 0. Plan (written before the code)

- **Starting point:** the S32 placeholder of S6-13 with the list section of S6-12b; S33/S34 (S6-16) reached only by address; the project answer with `candidates[]`, progress and the seven counts (S6-18), but without the evidence kinds and without the lock.
- **Files:** Workspace contracts and `ProjectService`; the client's project model, API service, store, S32 page with its loader, a new edit dialog, a shared breadcrumb, S34's path; specs; E2E (journeys, reflow); documents.
- **Invariants:** none flipped; SIG-13 gains its S32 client cases (TESTING v102 in the plan; v103 after the merge).
- **EF migration:** none.
- **Session:** one (fallback split: a = server and S32 without the dialog, b = dialog and breadcrumb). It fitted one.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | UX copy §5.8 has the button „Projekt bearbeiten“ („Name und Stand“) but no texts for its dialog | Asked: D1 (a); UX copy v44 U177 |
| F2 | R-PRJ-04 in the UI (`noCriteria.cta`, replacing the list) has no dialog in the Handoff or the UX copy (open point 1 of S6-12b, SPEC v2.55-4) and is not in the task row | Asked: D2 (a) — not built; the lock notice is shown |
| F3 | The Handoff does not say whether „Aus dem Produktverzeichnis übernehmen“ (M6) is hidden or disabled; a disabled option needs a visible reason (CLAUDE §9) | Asked: D3 (a) — hidden |
| F4 | „Ja – 2 Prüfnachweise: …“ needs the evidence kinds per candidate, and `criteriaLocked` needs the lock — neither is in the project answer; deriving the lock from `progress` misses assessments on excluded criteria (SPEC v2.56-4) | Asked: D4 (a); ARCHITECTURE C186 (1), SPEC v2.60-2 |
| F5 | „Nicht veröffentlicht“ and „Keiner davon passt“ stood only in note columns; the second has a key (`criteria.select.step1.none`) | Asked: D5 (a); UX copy U178 |
| F6 | Both frames (and S34's) show a path above the H1; no key names its navigation (S6-16 D6 (a) left it to S6-14) | Asked: D6 (a); UX copy U179 |
| F7 | The counts row is one sentence in the note column of `projects.detail.row.counts`, but must be a list of seven items; U54 names the short forms `directory.entry.row.*` for the same row | The seven `directory.entry.row.<value>` keys, added to `de.json` by hand (`directory` is not an area of `check-i18n.mjs` yet; texts equal to the UX copy); recorded as U180, no new text |
| F8 | The server lists the products as added; `projects.detail.orderNote` says „zuletzt hinzugefügt zuerst“ | Reversed on the client |
| F9 | S6-13 open point 2: S32 kept showing the project after an organisation switch | S30 opens on a switch to another organisation (SPEC v2.60-5); not on sign-out (self-review R1) |

## 2. Changes

1. **Server (Workspace, no migration):** `CandidateSummaryResponse.TestEvidences` (`TestEvidenceTypeResponse`: `testKind`, `testTypeId`, `otherTestTypeName`), read in the candidates' query in the order of the candidate answer; `ProjectResponse.CriteriaListLocked` from `ProjectCriteriaListService.IsLockedAsync` (now a dependency of `ProjectService`). Tests: `CandidateEndpointsTests` (evidence kinds on the project answer, an empty list on a new candidate), `ProjectCriteriaListTests` (the flag through the lock's lifecycle, per project).
2. **Models** `features/workspace/models/projects.ts`: `ProjectCandidate`, `ProjectEvidenceType`, `Project.candidates`, `Project.criteriaListLocked`, `ProjectChange`, `PROJECT_STATUSES`.
3. **API and store:** `ProjectsApiService.change` (PATCH), `withWorkspace().changeProject` (updates the loaded S30 list).
4. **Loader** `pages/project-detail/project-detail-load.ts`: `loadProjectView` — the project and, with a list, its copy (`loadSavedListView`); `ProjectCriteriaFacts` with the scope of the included criteria (requirement criteria by level, „Geprüft nach …“ apart) and the selection by name. The resolver uses it.
5. **Page** `project-detail.{ts,html,scss}`: rebuilt from the frames (Handoff 1.39 „S32 as built“) — path, title row, meta, archived note, criteria card, product card with menu and list, read-only when archived, S30 on an organisation switch; `@figma` `2148:4697` (first), `2148:3877`.
6. **Dialog** `dialogs/edit-project-dialog/`: name and status (radio group), archived → status only, only what changed is sent, error summary, confirmation notice, error notice, `projects.edit.saved`.
7. **Shared** `shared/components/breadcrumb/` `pa-breadcrumb`; on S32 and S34 (`candidate-detail`).
8. **Texts:** UX copy v44 (U177 – U181), `de.json` through `check-i18n.mjs --write` plus the seven `directory.entry.row.*` keys (F7).
9. **Checks:** `check-figma-links.mjs` `PENDING` empty again.
10. **Tests:** Vitest — `project-detail.spec.ts` (26 cases, rewritten), `project-detail.routing.spec.ts` (copy facts, failed copy), `edit-project-dialog.spec.ts` (9), `breadcrumb.spec.ts`, cases in `projects-api.service.spec.ts`, `with-workspace.spec.ts`, `candidate-detail.spec.ts`; fixtures gain the new members. E2E — `projects.spec.ts` (`s32-new`; new test: rename, archive, back to „Laufend“ with axe on the dialog and the archived page), `candidates.spec.ts` (S33 through the S32 menu, back through the path, the product row with seven counts), `reflow.spec.ts` (S32 with the open menu and the dialog's error summary).
11. **Documents:** TESTING v103 (V363 – V368; v101 deleted), ARCHITECTURE v98 (C186; v96 deleted), SPEC v2.60, UX copy v44 (v42 deleted), Handoff 1.39, Figma mapping 1.30, CODEBASE §2.105 and §3, CLAUDE.md state line (kept under 40 000 characters by shortening the note of S6-25 next to it).

12. **Merge of `main` after S6-20a (#416):** the first CI run had one Firefox failure — „Page crashed“ in `projects.spec.ts` „INV:WSP-03 … S32 reports the criteria taken in“, at the name lookup of S32's H1 after axe had run on S11 (the crash class of TESTING V165); the other new S32 tests passed in Firefox. Meanwhile S6-20a was merged, which requires `If-Match` on `PATCH /projects/{id}` (its open point 3): `ProjectsApiService.change(id, change, etag)` sends `ifMatch` and keeps the answer's tag (`withEtag`), `withWorkspace().changeProject` and the dialog pass `Project.etag`; a 409 `conflict` shows `projects.edit.error` until S37 (S6-21), as S6-20a decided for the other dialogs. The server merge keeps both `CriteriaListLocked` and S6-20a's `Etag` in `ProjectResponse`. S32's reflow mock answers with an `ETag`. Documents renumbered as above.

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y), with `/engineering:code-review` on the diff. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The organisation-switch subscription of S32 also fired when the active organisation became `null` (sign-out, account deletion) and navigated to `/projekte`, racing the sign-out's own navigation | **Fixed:** only a switch to another organisation opens S30; test „leaves the navigation to the sign-out …“ |
| R2 | Security: no new endpoint; the new members expose no private field (`qualificationNote`, `testedBy`, the private section stay on the candidate); tenancy through the query filters as before (the lock query runs in the same filtered context) | No action |
| R3 | N+1: the evidence kinds are read in the candidates' projection (one query); the lock is one `EXISTS` query per project read; the client reads the copy once per S32 load (four requests, as S34) | No action; the four requests are open point 4 |
| R4 | A catalog procedure key without a `testType.*` text would show the key | Won't fix: the server accepts only the five catalog procedures, each with its UX copy key |
| R5 | The dialog focuses the chosen status of an archived project through Material's class `.mat-mdc-radio-checked` (an internal class; `first-tabbable` finds nothing in jsdom and Material's own focus trap checks visibility) | Won't fix: covered by a unit test and the E2E test, which fail if Material renames the class |
| R6 | SIG-08/SIG-13: no total, share, colour or sort by counts; counts as a list in the fixed order; zeros shown | No action |
| R8 | After the merge: the dialog's `PATCH` sent no `If-Match` (S6-20a requires it: 428) | **Fixed:** the project's tag in `If-Match`, the new tag kept; specs check the header |
| R9 | CI (Firefox): „Page crashed“ in the WSP-03 journey after axe on S11, at the name lookup of S32's H1 (V165 class) — twice in two runs, so not a flake; the other S32 tests pass in Firefox | **Fixed (test):** after the axe run on S11 the journey finds the H1s and the link by CSS, as V165 does for the checkbox; TESTING V368 |
| R7 | Visual comparison (desktop light/dark, 360 dark, 320 light, open menu, dialog): links first rendered in the browser's blue; the „·“ separator started a wrapped line instead of ending the previous one (frame: „Nicht anwendbar: 2 ·“ ↵) | **Fixed** before pushing: links in the primary colour; separator `::after` on every item but the last |

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | green |
| `dotnet format ProcureAccess.sln --verify-no-changes` | green |
| `dotnet test --solution ProcureAccess.sln` | 2087 / 2087; after the merge of S6-20a 2142 / 2142 |
| Integration tests of the Workspace namespace | 125 / 125 |
| `npx ng test` (client, all) | 1031 / 1031 before R1; after the merge of S6-20a 1035 / 1035 |
| `npm run lint` (client), `npm run build:release` | green; S32's lazy chunk 26 kB |
| `npm run typecheck && npm run lint` (e2e, scripts) | green |
| E2E (Playwright + axe) | **not run locally** — another session uses the local E2E stack (owner instruction); left to CI |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` (+ self-test) | 10 checked, 0 pending; self-test 10 / 10 |
| `node scripts/check-i18n.mjs` | 0 errors |
| REUSE | `reuse lint-file` on every new file: compliant (the full `reuse lint` scans build output in a worktree; CI runs it on a clean checkout) |
| Figma | `2148:3877` and `2148:4697` compared with the page in a browser before and after building (desktop light and dark, 360 px, 320 px; no horizontal scrolling) |

## 5. Open points

1. **Assign or replace the list from S32** (`projects.detail.noCriteria.cta`, R-PRJ-04 in the UI; D2 (a)): needs a dialog design and its UX copy keys (title, the list select — S31's could be reused —, the 409 `locked-criteria-list` answer). Follow-up issue #420 „S32: assign or replace the project's criteria list (R-PRJ-04 UI)“ (milestone M5).
2. **Singular forms in the scope row:** `projects.detail.criteria.scope.value` has several counts in one sentence, so SD25's `.one` form cannot apply; with one criterion of a level it reads „1 Mindestanforderungen (A)“, „1 Hinweise“, „1 × „Geprüft nach …““ (seen with the test release T.1). A UX copy question for the owner (e.g. counts as „A: 1“ or a form per level).
3. **Figma S32 frames:** the last item of the path is a link in `2148:3877`/`2148:4697` but text in S34's frame; built as text with `aria-current="page"` for both. The frame's example products are listed oldest first, the code follows `orderNote` (last added first). Proposed for the next Figma pass (TD-80 batch).
4. **Four requests for the copy's facts** (also S6-16 open point 1): S32 and S34 read the whole copy, its criteria and filter values to show a name, a release, counts and names. The project answer could serve them; a small follow-up when the workspace answers are revised with S6-20 (ETags).
5. **`directory.entry.row.*` in `de.json` by hand** (F7): step 6 adds the area `directory` to `check-i18n.mjs`, which then owns these keys.
6. **S11 of a copy in the reflow run** (S6-12b open point 3) stays open; not in this row.
7. **E2E** runs first in CI (see §4).
8. **Firefox crash after axe on S11** (R9, TESTING V368): the WSP-03 journey uses CSS locators after that axe run; recheck with the next Playwright update, together with V129, V165, V226, V229 and V341.

<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-16 — Candidate form S33 and S34 with tabs and overview

> **Sprint:** 06, task S6-16 (2.5 points), after S6-15 (#409) and S6-13 (#407); started on `main` after S6-12b (#411), rebased onto `main` after S6-18 (#413) and S6-25 (#412) were merged during the task — documents renumbered (TESTING v101, ARCHITECTURE v96, SPEC v2.58, CODEBASE §2.103). **Issues:** none.
> **Requirements / invariants:** R-CAN-02 (UI), R-CAN-03, R-CAN-04 (the published states of step 6 excluded); A11Y-01, A11Y-09. No invariant changes status: SIG-13 (active since S6-18), A11Y-01, A11Y-08, A11Y-09, TEN-01, API-02 and SEC-10 gain cases; the client parts of SIG-08 and PRV-02 are tested and stay untagged until step 6 flips them.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **No EF migration, no server change, no new dependency.** E2E not run locally: another session used the local E2E stack (owner instruction); left to CI.
> **Scope:** S33 („Produkt hinzufügen“ / „Produkt bearbeiten“), S34 with its tabs as routes and a page title per tab, the tab Überblick with K1 in the workspace scope and the publication card `candidate.overview.publication.none`; the tabs Prüfnachweis and Einschätzung stay empty until S6-17 and S6-19.
> **Owner decisions (2026-11-18, with the plan):** **D1 (a)** — S33 gets two page titles `pageTitle.candidate.new` „Produkt hinzufügen – {{project}} – Beschaffungsprojekte“ and `pageTitle.candidate.edit` „Produkt bearbeiten – {{product}} – {{project}}“ · **D2 (a)** — keys for the texts that stood only in the UX copy's note column: `candidate.overview.appType.none` „Noch keine Kriterienliste“, `candidate.overview.directory.none` „Nicht verknüpft“ · **D3 (a), then (b)** — (a): progress and K1 counted on the client from the project's copy until S6-18; when S6-18 was merged during the task (its candidate answer carries `progress` and `fulfillment`), asked again: **(b)** S34 shows the server's values now · **D4 (a)** — S33 and S34 are reached by their address until S6-14 builds S32's menu and product list · **D5 (a)** — K1 and the overview cards are `<section>`s without accessible names (TESTING V229) · **D6 (a)** — vendor line `candidate.vendorLine` „Anbieter: {{vendor}}“; the frame's breadcrumb comes with S6-14 · **D7 (a)** — K1 rows: term `assessment.value.*`, value `signals.fulfillment.count` „{{count}} Kriterien“ / „1 Kriterium“. UX copy v43 U171 – U176.

## 0. Plan (written before the code)

- **Starting point:** server of S6-15 (`POST /projects/{id}/candidates`, `GET`/`PATCH /candidates/{id}`); the client's `features/workspace/` with S30, S31, the S32 placeholder and S11 of a copy. K1 not built. S6-14 and S6-18 not built.
- **Files:** models, API service, S33 page, S34 shell with tabs as child routes, the overview tab, K1 in `shared/components/signals/`, routes below S32; specs; E2E spec and reflow; documents.
- **Invariants:** none flipped; A11Y-01 and A11Y-09 gain cases.
- **EF migration:** none.
- **Session:** one (fallback split: a = S33 and the S34 frame, b = overview and K1). It fitted one.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | UX copy v42 had no page title for S33, and „Noch keine Kriterienliste“ / „Nicht verknüpft“ only in its note column | Asked: D1 (a), D2 (a); UX copy v43 U171 – U173 |
| F2 | S34's progress and K1 need assessments, which the server serves with S6-18 (not merged at the start); the overview's „Anwendungstyp“ needs the copy's app types, which the project answer does not carry | Asked: D3 (a) — client counts from the copy; after S6-18's merge asked again: D3 (b) — the server's `progress` and `fulfillment`. The copy is still read for its name, release and app types |
| F3 | S32 is a placeholder; its add-product menu and the product links come with S6-14 | Asked: D4 (a) — reached by address; E2E and reflow navigate there |
| F4 | Handoff §4.1 makes K1 a named region; CLAUDE §3 and TESTING V226/V229 forbid named sections while Firefox crashes axe on them, and V229 says K1/K2 decide it again | Asked: D5 (a) |
| F5 | The Figma frame shows a vendor line and a breadcrumb that neither the Handoff row nor the UX copy has | Asked: D6 (a); UX copy U174 |
| F6 | K1 in Figma (`2101:3180`) splits each row into term and value; the UX copy has one combined text per row (`signals.fulfillment.fulfilled` „Erfüllt: {{count}} Kriterien“) | Asked: D7 (a); UX copy U175 |
| F7 | SPEC v2.53-3 left „(optional)“ on S33's labels open | The UX copy's own rule (`common.optional`: optional fields are marked) and the server's (only the name is required) settle it without a new text; U176 records it |
| F8 | Angular Material 22's `mat-tab-nav-bar` throws without `[tabPanel]` („A mat-tab-nav-panel must be specified“) | Not as Handoff §5.1 (links with `aria-current="page"`): the tabs are ARIA tabs with the page in their panel — Material's own accessible pattern; recorded in Handoff 1.38, TESTING V355, ARCHITECTURE C184 (5) |
| F9 | At 320 px the three tab labels do not fit even with 16 px padding; Material scrolls the bar (the Figma 360 note asks for exactly that) | The reflow check exempts links clipped inside a self-scrolling tab bar and checks that the selected tab is in view instead (TESTING V356) |

## 2. Changes

1. **Models** `features/workspace/models/candidates.ts`: route segments and paths (`produkte`, `neu`, `bearbeiten`, tab segments `''` / `pruefnachweis` / `einschaetzung`), `CANDIDATE_LIMITS` (the server's), `TestStatus` with `testStatusKey`, `Candidate` (with S6-18's `progress` and `fulfillment`), `TestEvidence`, `CandidateRequest`.
2. **API** `services/api/candidates-api.service.ts`: `get` (`CALLER_SHOWS_FAILURES`), `create`, `change` (PATCH with every field; `SAVE_CANDIDATE_PROBLEMS` = validation, antiforgery, email-unconfirmed as on S20).
3. **Routes and loading** `pages/candidate-detail/candidate-detail.routing.ts` (lazy child `produkte` of `PROJECT_ROUTES`) and `candidate-load.ts`: `CandidateLoad` with a signal of the view, `loadCandidateView` (project and candidate side by side; a candidate of another project is „not found“; for S34 the copy through `loadSavedListView`), `isProjectEditor`. Resolvers: `candidateResolver` (`pageTitle.candidate`), `candidateFormResolver` (`.new` / `.edit`, `pageTitle.forbidden` for a Reader). **Deviation from the plan:** no candidate state in `withWorkspace()` — nothing outside these pages reads a candidate; S6-19's autosave brings the state that survives navigation (ARCHITECTURE C184 (2)).
4. **S34** `candidate-detail.ts`: H1, `candidate.vendorLine`, archived note, `pa-candidate-tabs` with the outlet in its panel, S80 in place; `candidateTabTitle(tab)` fills `pageTitle.candidate` per tab from a translation stream (so a direct load gets the right title).
5. **Tabs** `components/candidate-tabs/`: `mat-tab-nav-bar` not stretched, its panel with projected content, the short label below 600 px, 16 px label padding below 600 px (Handoff §2.2, as S11), the divider in `outline`; `selected` for S33.
6. **Überblick** `overview/candidate-overview.ts`: four cards; Produktangaben (empty optional facts left out, app types from the copy, „Nicht verknüpft“), Stand der Bewertung (test status with evidences as S32 names them, the server's progress and K1 as H4 (D3 (b)), legend, the two text buttons; without a list `projects.detail.noCriteria`), Veröffentlichung (`.none`, `.explain`), Nur für Ihre Organisation (`surface-container-low`); „Bearbeiten“ for Editor/Owner of a project that is not archived, described by its card's heading.
7. **S33** `pages/candidate-form/`: H1, the tabs with Überblick selected when editing, `common.requiredNote`, the fields (all but the name with `common.optional`), the private section, the S20 error pattern, „Abbrechen“ / „Speichern“; archived note, „Kein Zugriff“, S80 in place.
8. **K1** `shared/components/signals/fulfillment-breakdown/`: scope workspace only; seven rows in the scale's order, no input that could form a total.
9. **Texts.** UX copy v43 (U171 – U176); areas `candidate` and `signals.fulfillment` in `AREAS`; `de.json` through `check-i18n.mjs --write`; v41 deleted.
10. **Tests.** New specs: `candidates-api.service.spec.ts` (3), `candidate-detail.routing.spec.ts` (9), `candidate-detail.spec.ts` (5), `candidate-overview.spec.ts` (12), `candidate-form.spec.ts` (10), `fulfillment-breakdown.spec.ts` (4); fixtures `testing/candidate-fixtures.ts`. E2E `e2e/workspace/candidates.spec.ts` (one journey, five axe states), the reflow test of S33 and S34, `createProjectThroughApi` in `e2e/support/accounts.ts`, `expectReflow(page, { scrollingTabBars })`.
11. **Documents.** TESTING v101 (V351 – V356; v99 deleted), ARCHITECTURE v96 (C184; v94 deleted), SPEC v2.58 (three rows), Handoff 1.38 (§4.1 K1 as built, §5.3 „S33 as built“, „S34 as built“), Figma mapping 1.29 (K1 built, S34 built), CODEBASE §2.103 and §3, CLAUDE.md §3 state line (shortened: the file is at 39 966 characters).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security:** no new endpoint; reads go through S6-15's endpoints and their checks (TEN-01: a foreign project or candidate is 404 → „Seite nicht gefunden“, a candidate of another project too). Role and archived state only hide controls; the server decides (S6-15). The vendor website is bound to `href` (Angular sanitises it; the server accepts only `https`). Private fields are shown only in the private card and the form; nothing is published in this step.
- **Correctness / edge cases:** empty optional facts are left out rather than shown empty; a project without a list shows `projects.detail.noCriteria` instead of progress and K1 (the server answers both `null` then); a list with no requirement criterion shows „0 von 0“ and seven zeros (K1 never hides a row). The form trims and sends every field, so an emptied field is cleared (S6-15's PATCH).
- **N+1 / requests:** opening S34 costs six requests (project, candidate, the copy, its criteria, its filter values, the active release); four of them only for the copy's name, release and app type names (open point 1).
- **A11y:** headings H1 → H2 per card → H3 per column → H4 for K1; labels with „(optional)“ in the accessible name; the error summary takes the focus; „Bearbeiten“ links are described by their card headings; axe in component tests and in the E2E journey in both appearances.
- **Fix (found in the review, fixed):** (1) at 320 px the tab bar scrolls inside itself, which the reflow check would have counted as a link outside the viewport — CI would have failed A11Y-09 (F9); (2) on a direct load the tab name in the page title and the test-status line could keep the raw key when the texts arrived after the page opened — both now follow the loaded texts; (3) a second divider under the tabs (Material draws its own; its token now carries the frame's colour).
- **Found while testing and fixed:** Material 22 requires the tab panel (F8); the input alias `mat-stretch-tabs`; tags of `planned` invariants (SIG-08, PRV-02) removed again — the gate warns, and the project leaves them untagged until the flip (as V336); SIG-13 became `active` with S6-18, so its two client cases carry `INV:SIG-13`.
- **Won't fix:** leaving S33 through a tab drops unsaved input without a warning, as „Abbrechen“ does — the UX copy has no text for such a warning; the six requests of S34 (above) for now (open point 1).
- **Follow-up:** open points 1 – 6.

## 4. Checks

- Client: `npx ng test` **1 005 passed** (100 files); `npm run lint` clean; `npx ng build --configuration development` clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **E2E not run locally** — another session used the local E2E stack (owner instruction); left to CI.
- `check-invariants` (no warning), `check-figma-links` (K1 and S34 tagged), `check-i18n` (0 errors), and the self-tests of the three scripts (62 passed): green.
- Backend unchanged (no `dotnet` change in this PR).
- `reuse lint-file` on every new file: compliant (worktree; CI runs the full `reuse lint`).
- **Figma:** S34 (`2134:3184`, `2097:2627`) and K1 (`2101:3180`) compared before building through the Figma MCP (screenshots and node structure). The rendered result was **not** compared side by side: starting a throwaway database or the dev server for it was refused in this session (the owner's instruction not to start a stack), so the comparison after building is by structure — order, texts, headings, the 360 px rules of the frame note. Differences kept on purpose: no publish button (step 6), no breadcrumb (D6 (a)), dt texts are S33's labels (the frame shortens „Was kann das Produkt?“ and „Ansprechpersonen“), the tabs are ARIA tabs (F8).

## 5. Open points

1. **The copy is read only for its name, release and app type names** (four of S34's six requests): the server could serve the app type names with the project's `criteriaList` (name and release are there already). Proposed as a small follow-up with S6-19 or S6-14, which show the same facts.
2. **S6-14 leads here:** the menu „Produkt hinzufügen“ (`.new` → `newCandidatePath`), the product names as links to S34, and a shared breadcrumb for S32 and S34 (the frames show one; no key for its navigation label yet — UX copy, D6 (a)).
3. **Handoff §5.1 still says „links with `aria-current`“ for the tabs of S34/S34a**; 1.38 records the built ARIA tabs in §5.3. If the owner wants plain links, Material's bar cannot provide them (F8); a hand-built tab row would be needed.
4. **The reflow exemption for self-scrolling tab bars** (`scrollingTabBars`, V356) applies to S34's tabs only; S11's card tabs still fit without pagination.
5. **CLAUDE.md is at 39 966 characters** (limit 40 000; this task shortened its own line): the „State of the code“ list should move to CODEBASE with the next task that touches it.
6. **Rendered comparison with the frames** (§4): to be done in the owner's review or by the next session that may start a local stack.

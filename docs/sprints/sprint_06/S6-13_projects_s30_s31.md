<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-13 — Project list S30 and create dialog S31

> **Sprint:** 06, task S6-13. **Issues:** none.
> **Requirements / invariants:** R-PRJ-01, R-PRJ-02 (UI part), R-SHL-02 („Projekte“). A11Y-01, A11Y-08, A11Y-09 gain the new screens; TEN-01 (client part), SEC-10 and API-02 gain cases; no invariant changes its status.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **No EF migration, no server change.**
> **Scope:** exactly the row of S6-13 in `SPRINT_06.md`: the store slice `withWorkspace()`, routes under `/projekte`, S30, S31, a placeholder of S32 that S31 opens, and „Projekte“ in the header. No project copy (S6-12), no S32 screen (S6-14), no candidates (S6-15).
> **Owner decisions (2026-11-18, with the plan):** **D1 (a)** — S31 asks only for the name; the list choice `projects.create.criteria` with its hint, and what S31 shows when the project was created but the copy failed, come with S6-12, which builds `PUT /projects/{id}/criteria-configuration` · **D2 (a)** — the missing texts are new `projects.*` keys in UX copy v41 (U162 – U168) with the wording of the plan · **D3 (a)** — `withWorkspace()` holds the projects only; `withProducts()` / `withFavorites()` stay with the legacy product screens until step 6, and ARCHITECTURE §8.1 says so.

## 0. Plan (written before the code)

- **Starting point:** `main` at b16d6e5 (S6-11, S6-23 and S6-24 merged). S6-12 is not built, and no branch for it exists.
- **Files:** `features/workspace/` (models, API service, slice, S30, S31, S32 placeholder), the root store, `app.routes.ts`, `main-nav`, `PROBLEM_TYPE_KEYS`, `check-i18n.mjs` (area `projects`).
- **Invariants:** none activated; A11Y-01 and A11Y-09 are active already and gain the screens.
- **Tests:** Vitest for service, slice, S30, S31, header; E2E reflow and axe, left to CI (another session uses the local E2E stack).
- **Migration:** none. **Size:** one session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The server creates a project from `{ name }` only (S6-11, C172); the copy of a list is S6-12's `PUT …/criteria-configuration`, in the Configurator context, so create and copy can never be one transaction | Decision D1 (a): S31 without the list choice; it joins with S6-12 |
| F2 | UX copy v40 had no texts for S30's sorting, its column heads, the announcement after the filter, the case „every project archived“, S30 without a membership, and S31's error notice and success announcement | Decision D2 (a): UX copy v41 U162 – U168 |
| F3 | ARCHITECTURE C173 (6) moved the rework of `withProducts()` / `withFavorites()` into S6-13, but both only serve the legacy product screens `/filters`, `/products`, `/favorites`, which step 6 replaces | Decision D3 (a); ARCHITECTURE v90 C178 (2) |
| F4 | S6-24 is merged: a slice declares `LOADING_INPUT` and the root store adds `withLoading()` once | `withWorkspace()` follows that pattern |
| F5 | S30 and S31 have no Figma frame; the header frame `2017:565` (Auth = SignedIn, Active = Projekte) places „Projekte“ between „Kriterien“ and „Produkte“ | Built from Handoff §5.3 like S14 and S12; the header as the frame shows |
| F6 | The Figma mapping places S32 at `features/workspace/pages/project-detail/project-detail.ts`, and `check-figma-links.mjs` checks the tag of every mapped file that exists | The placeholder sits at that path and is on the check's `PENDING` list until S6-14 (no frame-less tag) |
| F7 | The project area `projects` was not in `AREAS` of `check-i18n.mjs`, so `project-archived` had no client text (S6-11 open point 1) | The area joins; `PROBLEM_TYPE_KEYS` maps `project-archived` → `projects.archived.note` |

## 2. Changes

1. **UX copy v41** (`docs/ux_copy_procureaccess_v41.md`, v39 deleted): U162 `projects.sort*`, U163 `projects.list.name|products|modified`, U164 `projects.shown` (+ `.one`, `.zero`), U165 `projects.empty.archivedOnly`, U166 `projects.needOrg` (+ `.cta`), U167 `projects.create.error.title`, `projects.create.error`, `projects.created`; U168 the row `project-archived` → `projects.archived.note` in §6.3 (the mapping of decision S6-11 1 (a), no new wording). `check-i18n.mjs` gets the area `projects`; `de.json` written by `--write`.
2. **Models and API** (`features/workspace/models/projects.ts`, `services/api/projects-api.service.ts`): route paths, types, `statusKey`, the 120-character limit; list, create, read, all `CALLER_SHOWS_FAILURES`.
3. **Store** (`state/with-workspace.ts`): `withWorkspace()` after `withConfigurator()`; `workspace.projects` = `{ organizationId, status, items }`; `loadProjects` (an answer that a later load or a reset replaced is dropped and reported as not shown), `createProject` (adds the project to the loaded list of its organisation), `resetWorkspace` (from `clearUserData()`).
4. **S30** (`pages/projects/`, `/projekte`, lazy, `authGuard`): H1, intro, „Projekt anlegen“ for Editor and Owner, „Sortieren nach“, „Archivierte Projekte anzeigen“, cards below 600 px, table from 600 px, the empty states, S20 link without a membership, S80 in place on a failed load; announcements `common.loaded`, `projects.sort.updated`, `projects.shown`.
5. **S31** (`dialogs/create-project-dialog/`): the S12 form pattern with the name, its hint and checks, the error summary, the confirmation notice, the error notice; success navigates to S32 and announces `projects.created`.
6. **S32 placeholder** (`pages/project-detail/`, `/projekte/:id`): resolver (`pageTitle.project`, not found, server), H1 = name, `projects.detail.meta`, `projects.archived.note` for an archived project. `PENDING` entry in `check-figma-links.mjs`.
7. **Header** (`layout/main-nav/`): „Projekte“ between „Kriterien“ and „Produkte“ with an active organisation, `aria-current` on S30 and below, in bar and drawer. `SHELL_TEST_ROUTES` know the two routes.
8. **Root store and errors:** `withWorkspace()` and `resetWorkspace()` in `app.store.ts`; `project-archived` in `PROBLEM_TYPE_KEYS`.
9. **Tests.** New: `projects-api.service.spec.ts`, `with-workspace.spec.ts`, `projects-page.spec.ts` (`INV:TEN-01`), `create-project-dialog.spec.ts` (`INV:SEC-10`, `INV:API-02`), `project-detail.spec.ts`, `project-detail.routing.spec.ts` (`INV:TEN-01`), `testing/workspace-test-store.ts`; `e2e/workspace/projects.spec.ts` (`INV:A11Y-01`, `INV:A11Y-08`). Changed: `main-nav.spec.ts`, `app.store.spec.ts`, `e2e/shell/reflow.spec.ts` (S30, S31; „Projekte“ in the compact menu).
10. **Documents.** ARCHITECTURE v90 (C178; v88 deleted), TESTING v95 (V326 – V328; v93 deleted), SPEC v2.52 (three rows), Handoff 1.36 (§5.3 S30, S31, S32 placeholder; §3.1), Figma mapping 1.28, CODEBASE §2.97 and §3, CLAUDE.md (§3 state, §4 the two legacy slices; 39 849 bytes). S6-05 merged first with ARCHITECTURE v88 (C174 – C176), SPEC v2.50 and CODEBASE §2.95; after merging `main` this task's parts were renumbered to v89 / C177, v2.51 and §2.96 (TESTING was not touched by S6-05). S6-26 then merged with ARCHITECTURE v89 (C177), TESTING v94 (V323 – V325), SPEC v2.51 and CODEBASE §2.96; after merging `main` again they are ARCHITECTURE v90 / C178, TESTING v95 / V326 – V328, SPEC v2.52 and CODEBASE §2.97.

Not changed: server, migrations, `appsettings.json`, compose files, TECH_DEBT (next version at the end of the sprint), the sprint file, `LEGACY_EXCEPTIONS` (no listed file touched).

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` on the whole diff; findings sorted per §6.8.

- **Security:** no secrets; ids are URL-encoded in every request; names only as interpolated text. The client hides „Projekt anlegen“ from Readers, the server decides (403 → `projects.create.notAllowed`). No browser storage (R-SET-02).
- **Tenancy:** S30 reads the active organisation; an answer for an organisation switched away from is dropped by the slice (tested). S32 reads through the server's filter (404 → „Seite nicht gefunden“).
- **Correctness:** one request per page open or switch; the loading counter is decremented on every path; `loadProjects` never rejects, so no unhandled rejection; S31 ignores a second submit while busy.
- **Queries:** no N+1 — the list is one request; S32 one request.
- **A11y:** H1 focus on every route change (S30, S32 after S31), the table named by the H1, the sort and filter changes announced, error summary focused, `aria-disabled` on the busy button, touch targets from Material; axe in the component tests and in E2E in both appearances.
- **fix:** none found.
- **won't fix:** R1 — S30 loads again each time it opens (a short loading bar, also after S31): fresh data like S14, and the server orders by change. R2 — when another account signs in through the login overlay into an organisation with the same id, S30 would not load again; owner decision S5-30 D3 (b) sends that user to the start page, so S30 is never shown in that state.
- **follow-up:** open points 1 – 3.

## 4. Checks

- Client: `npx ng test` 93 files, 934 tests passed (the new and changed specs: 8 files, 70 tests, then the renamed S32 files again); `npm run lint`: all files pass; `npm run build:release`: no warning, no budget error. A last full run after the renaming had two 5-second timeouts in untouched configurator specs (`export-dialog.spec.ts`, `criteria-list.spec.ts`; another test in each of two runs) at a load average of 18 from another session; both files passed alone (39 tests), and every workspace spec passed in that run.
- `e2e/`: `npm run typecheck` and `npm run lint` green. **E2E not run locally:** another session uses the local E2E stack (owner instruction), so `e2e/workspace/projects.spec.ts` and the reflow cases run in CI only. **CI, first run:** `projects.spec.ts` failed on Chromium and mobile Chrome with an axe colour-contrast finding on S31's `mat-error` (2.91 and 3.57: the error colour half-faded). Cause, found after a first wrong fix (waiting two frames, second run failed the same way): Material adds `mat-form-field-animations-enabled` to a field 300 ms after it starts, and an error shown before that fades in a second time when the class arrives — after every settle. The test submitted S31 right after the dialog opened. Fixed in the test as `identity/register.spec.ts` does: it waits for that class before submitting, and for the field's own `mat-error` (not the summary's line with the same text) before axe. Firefox, WebKit, mobile Safari and every other stage passed.
- `node scripts/check-invariants.mjs`: gate passed. `node scripts/check-figma-links.mjs`: 7 checked, 18 not built yet, 1 pending (the S32 placeholder). `node scripts/check-i18n.mjs`: 580 keys, 0 errors. `node scripts/check-icons.mjs`: 0 errors. Self-tests of `check-i18n` and `check-figma-links` green. `reuse lint-file` on the new and changed files: compliant (full `reuse lint` scans build output in a worktree).
- Backend: unchanged; not built or tested in this task.
- Figma: the header frame `2017:565` looked at before building; S30 and S31 have no frame. The screens were not seen in a browser (no API stack was running, and none was started beside the other session's E2E stack); their layout is covered by the reflow runs in CI.

## 5. Open points

1. **List choice in S31 (D1 (a)).** S6-12 adds `projects.create.criteria` (saved lists of the active organisation only, no project copies) with `projects.create.criteria.hint`, calls its `PUT …/criteria-configuration` after the create, and needs a text for „project created, list not copied“ (not in the UX copy yet). Until then R-PRJ-02's criterion „project copies of other projects are not offered“ has nothing to check.
2. **Organisation switch on S32.** The placeholder keeps showing the project after the header switches to another organisation (no leak: the user is a member of both). S6-14 decides what S32 does on a switch (e.g. back to S30), as S21 opens the chosen organisation's page.
3. **S32 in the reflow run and its Figma tag** come with S6-14, which removes the `PENDING` entry.
4. The legacy slices `withProducts()` / `withFavorites()` and the legacy screens go with step 6 (D3 (a), C178 (2)).

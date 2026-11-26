<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S7-06 — Path back from S11 of a project copy

- **Sprint:** 07 · **Task:** S7-06 · **Issues:** #429 (also TD-91)
- **Requirements / invariants:** R-PRJ-06; A11Y-01, A11Y-09 (both stay active, new cases)
- **Status:** done, in review (pull request open; E2E left to CI)
- **Scope:** `pa-breadcrumb` above the H1 of `/projekte/:id/kriterienliste`; spec, J2, reflow; documents. No server change, no EF migration.
- **Owner decisions:** O-S7-2 (a), 2026-11-26: the last path item is the page's H1 (no new UX copy key).

## Plan (written before the code; the owner could not be asked in this batch)

Starting point: S11 in project-copy mode names the project only as text. Files: `saved-list-page.{ts,html,spec.ts}`, `e2e/journeys/j2-workspace.spec.ts`, `e2e/shell/reflow.spec.ts`, Handoff, CODEBASE, TESTING. Invariants: none flip (A11Y-01, A11Y-09 gain cases). No migration. One session. No stop condition applied (the text is decided, every key exists: `shell.nav.projects`, `common.breadcrumb`).

## 1 Starting point

`saved-list-page` already holds `SavedListProject` (id, name, archived) and reports `noteCriteriaAdded` in its `onDestroy`; `Breadcrumb` and `PROJECTS_PATH`/`projectPath` exist (S6-14).

## 2 Changes

- `saved-list-page.ts/.html`: computed `breadcrumb` (null unless `shown.project !== null`): „Projekte“ → `/projekte`, project name → `projectPath(id)`, `shown.list.name` as text; `<pa-breadcrumb>` above the H1. The organisation's lists and S16 are unchanged. Leaving through the path destroys the page like any navigation, so `noteCriteriaAdded` still reports (issue point 3) without new code.
- `saved-list-page.spec.ts`: three cases (Owner, Reader, Editor of an archived project: items, links, last item as text with `aria-current`, path before the H1) and one for a list of the organisation (no path).
- `j2-workspace.spec.ts`: returns to S32 by Shift+Tab to the project link and Enter, instead of `page.goBack()`. Axe `j2-s11-copy` already runs in both appearances.
- `reflow.spec.ts`: S11 of a copy at 320 px with long project and list names (TD-91), through `routeCandidate`.
- Documents: Handoff 1.47 (S11 copy row), CODEBASE §2.100, TESTING v115 (V418; v113 deleted).

## 3 Self-review

- Correctness: the path is shown only with a project; for an archived project and a Reader it is shown too (navigation, not an edit).
- A11y: `nav` „Sie sind hier“, `aria-current="page"`, DOM order = visual order, wraps (component CSS, no truncation).
- Finding, won't fix: a component spec that clicks the link starts a real navigation without routes (NG0205 in the test), so „leaving through the path reports“ has no extra spec; the `onDestroy` path is covered by V339 and the unchanged code.

## 4 Checks

Client: `ng test` of `saved-list-page.spec.ts` (29 passed), `ng lint` clean. E2E: `npm run typecheck` and `npm run lint` clean; the J2 and reflow runs are left to CI (the stack was not started, by the owner's rule). `check-invariants`, `check-figma-links`, `check-i18n` green. reuse: `lint-file` on the new files (the full lint reports false positives in a worktree).

## 5 Open points

- The Figma frame of S11 has no project-copy variant; noted in the Handoff row, the Figma mapping is unchanged (no new component).
- Prettier reports style findings in untouched neighbouring files (not part of CI); not changed.

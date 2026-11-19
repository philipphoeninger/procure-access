<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-21a — Conflict dialog S37 for the workspace forms

> **Sprint:** 06, task S6-21 (2.5 points), part a of the split the owner confirmed on 2026-11-19 (D5 (a)). Started on `main` at `0ea901ca` (S6-20a, S6-20b, S6-19b merged). Documents: TESTING v108, ARCHITECTURE v103, SPEC v2.65, UX copy v47, Handoff 1.43, Figma mapping 1.34, CODEBASE §2.110. **Issues:** none.
> **Requirements / invariants:** R-CAN-11 (UI) for „Projekt bearbeiten“, S33 and S34a; API-03 stays green and gains its client cases; A11Y-01, A11Y-08, A11Y-09 gain S37. No invariant changes status.
> **Status:** done by Claude Code (2026-11-19) — pull request open. **No EF migration, no server change**, no new dependency. E2E run locally in Chromium, Firefox and mobile Chrome; WebKit and mobile Safari left to CI.
> **Scope:** the shared dialog S37 (`shared/components/conflict-dialog/`, Figma `2056:2519`) with the behaviour of Handoff §5.1, used by the dialog „Projekt bearbeiten“ (S32), S33 (editing a product) and S34a (test status and evidences); the E2E with two browser contexts. **Not in this part (S6-21b):** the saved-list edits on S11 (a saved list and a project's copy) and the selection change on S10.
> **Owner decisions (2026-11-19, with the plan):** **D1 (a)** — new key `conflict.body.unknown` „Eine andere Person hat diesen Inhalt geändert, während Sie ihn bearbeitet haben. Ihre Änderungen sind noch nicht gespeichert.“ for a conflict without an editor (wording as proposed) · **D2 (a)** — „Meine Änderungen kopieren“ copies one line „label: value“ per changed field with the existing labels; on S11 each criterion's title with its changed parts (S6-21b) · **D3 (a)** — S11 keeps the shown decisions after a conflict, stops saving, every further change opens S37 (S6-21b) · **D4 (a)** — S10's selection change of a saved list is included (S6-21b); S14's delete keeps the general error · **D5 (a)** — split a/b. The clipboard fallback (the general error when the browser refuses it) was named in the plan without an objection.

## 0. Plan (written before the code)

- **Shared:** `ConflictDialog` (alert dialog, first focus on „Meine Änderungen kopieren“, S13 rules) and a small state class for „saving blocked until reload“.
- **Callers:** „Projekt bearbeiten“ (S37 over it; reload of S32), S33, S34a; `conflict` handled by the caller instead of the snackbar.
- **Tests:** component specs of the dialog and of each caller; E2E with two contexts; S37 in `reflow.spec.ts`. **Invariants:** none flipped. **Migration:** none.
- **Session:** the plan proposed the split a/b; part a fitted one session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The server answers every stale `If-Match` with 409 `conflict` and `editorDisplayName` (S6-20a/b); the client showed the general error | No server change; the client needs only the field in `ProblemDetails` |
| F2 | `editorDisplayName` is `null` for a deleted account or a version stored before S6-20; `conflict.body` needs `{{name}}` | New key `conflict.body.unknown` (D1 (a), UX copy v47 U188) |
| F3 | After a conflict, S34a's refresh reads the candidate again (its tag is then fresh), so a second „Speichern“ would succeed and overwrite the other change silently | Handoff §5.1 „saving stays blocked until reload“ is enforced by the form (`EditConflict.blocked`), not by the stale tag |
| F4 | `ProjectsApiService.change` is `CALLER_SHOWS_FAILURES`; the candidate requests name their problem types | The project dialog needed no request change; `SAVE_CANDIDATE_PROBLEMS` gains `conflict` |
| F5 | MatDialog in this codebase sets `aria-modal="false"` on every dialog | S37 follows the other dialogs; no override |
| F6 | `CLAUDE.md` stands at 39 991 of its 40 000 characters | The convention „a form's `conflict` opens S37 through `ConflictDialog`/`EditConflict`“ is recorded in CODEBASE §2.110 only (open point 4) |

## 2. Changes

- **Shared** `shared/components/conflict-dialog/`: `conflict-dialog.ts` (`ConflictDialog`, `ConflictDialogData`; `@figma 2056-2519`), `edit-conflict.ts` (`EditConflict`, `changesText`, `ChangedField`, `ChangedGroup`), `conflict-dialog.spec.ts`.
- **Core:** `ProblemDetails.editorDisplayName`.
- **„Projekt bearbeiten“:** S37 over the dialog; Escape returns to it, „Speichern“ then opens S37 without a request; „Neue Fassung laden“ closes both with `RELOAD_PROJECT` (`EditProjectResult`). **S32:** `reload` reads the project and its list facts (`loadProjectView`, loading bar) and focuses the H1.
- **S33:** S37 when editing; the copied fields are those that differ from the product as read (`valuesOf`); „Neue Fassung laden“ reads the product, the form is filled from it, the focus goes to the H1.
- **S34a:** a `conflict` stops the sequence of requests; the refresh still runs; S37 copies the changed status and each unsaved card under its heading (`EvidenceCard.changedFields()`, `shownValues`); „Neue Fassung laden“ refills the tab and focuses the H1.
- **API:** `SAVE_CANDIDATE_PROBLEMS` with `conflict`; doc comments of both API services.
- **UX copy v47** (U188 `conflict.body.unknown`, U189 what is copied; v45 deleted); `conflict` joins `AREAS` of `check-i18n.mjs`; `de.json` by `--write`.
- **Tests:** `conflict-dialog.spec.ts` (9), new cases in `edit-project-dialog.spec.ts` (+3), `project-detail.spec.ts` (+1), `candidate-form.spec.ts` (+3), `candidate-evidence.spec.ts` (+3), `candidates-api.service.spec.ts` (expected problem types); E2E `e2e/workspace/conflict.spec.ts` (new), `e2e/shell/reflow.spec.ts` (S37 over „Projekt bearbeiten“ at 320 px).
- **Documents:** TESTING v108 (V387 – V391; v106 deleted), ARCHITECTURE v103 (C191; v101 deleted), SPEC v2.65 (v2.65-1, -2), Handoff 1.43 (row „S37 as built (part a)“), Figma mapping 1.34, CODEBASE §2.110 and §3, SPRINT_06 (the split in the row of S6-21). `CLAUDE.md` unchanged (F6).

## 3. Self-review (CLAUDE §6.7, §6.8)

Review with the checklist of CLAUDE §6.7 (skill *engineering:code-review*), findings sorted:

- **Security / privacy:** no request and no header changed; the 409's `editorDisplayName` is the address members already see on S21 (S6-20a R4). „Meine Änderungen kopieren“ also copies private fields (contacts, internal notes, the private note of an evidence) — only into the user's own clipboard, on their request; nothing is sent or logged (PRV-01, PRV-02 concern publication and answers, not the user's clipboard). Nothing in browser storage.
- **Correctness / edge cases:** a second „Speichern“ after Escape sends nothing (unit and E2E: one `PATCH`); S34a's refresh after the conflict does not unblock saving (F3, test); a reload that fails shows „Seite nicht gefunden“ or S80 in place, as the resolvers do; the S34a sequence keeps what was stored before the conflict and copies only the rest; a `null` editor shows `conflict.body.unknown` (unit); a refused clipboard shows the general error and keeps the dialog (unit). The S37 → „Projekt bearbeiten“ → S32 chain: S32 reloads after `afterClosed`, i.e. after MatDialog restored the focus, so the H1 gets it last (E2E).
- **Requests:** one `GET` per „Neue Fassung laden“ (S32 also the copy's facts, as its resolver); none for a blocked save.
- **A11y:** alert dialog named by its title and described by its body, first focus on the primary action as the frame says, Escape returns the focus to „Speichern“; the copy is announced through `A11yAnnouncerService`; axe in both appearances on `s37-conflict`; reflow at 320 px.
- **Found while testing and fixed:** (1) the S33 copy expected „Anbieter (optional): …“ — the labels come from the label keys, without the „(optional)“ suffix the template adds; the shorter label is the better copy text, the test was adjusted. (2) A lint finding in the S34a spec (a promise in a mock).
- **Won't fix:** (1) `busy` is set to false twice on the conflict path of „Projekt bearbeiten“ and S33 (in `catch` so the spinner is gone while S37 is open, and in `finally`); harmless. (2) In S34a the copied text of a new „Eigene Prüfung“ card names the kind in its heading and as its first field; both come from the form as shown.
- **Follow-up:** open points 1 – 4.

## 4. Checks

- Client: `npx ng test` **1 134 passed** (108 files); `npm run lint` clean; `npm run build:release` clean; `tsc` of the app clean.
- e2e: `npm run typecheck`, `npm run lint` clean. **Run locally** (throwaway PostgreSQL and Mailpit, the API with `dotnet run` and the env of `docker-compose.e2e.yml`, `ng serve --port 4300`): `workspace/conflict.spec.ts` 3 passed, then `workspace/` and `shell/reflow.spec.ts` **150 passed** in Chromium, Firefox and mobile Chrome. WebKit and mobile Safari: CI.
- No server change: the .NET build and tests are unaffected (CI runs them).
- `check-invariants` (gate passed), `check-figma-links` (12 checked — S37 is new), `check-i18n` (0 errors), `check-icons` (0 errors); `reuse lint-file` on every new file: compliant.
- **Figma:** S37 compared side by side in a browser with `2056:2519` at 1440 px and 360 px, light and dark: title, body with the name, the attention notice, „Neue Fassung laden“ (text) and „Meine Änderungen kopieren“ (filled) on the right of a 600 px dialog; below 600 px full screen with the actions stacked, the primary action last. Difference kept: on „Projekt bearbeiten“ S37 opens over that dialog; the frame shows it straight over the page (S34b).

## 5. Open points

1. **S6-21b:** S37 for the saved-list editor on S11 (a saved list and a project's copy; D3 (a)) and S10's selection change of a saved list (D4 (a)); until then those show the general error (S6-20b R1).
2. **S14's delete** answers a `conflict` with the general error: there is nothing to copy, and the UX copy has no text for „deleted although changed meanwhile“. A design question for the owner (proposed issue).
3. **A refused clipboard** (an insecure origin, a browser that forbids it) shows the general error. A fallback that shows the text to select needs a label key (UX copy) — only if users report it.
4. **`CLAUDE.md` is at its size limit** (39 991 of 40 000 characters; also S6-17 open point 6, S6-19b): the convention for a form's `conflict` lives in CODEBASE §2.110. A shortening pass of `CLAUDE.md` is the owner's call.

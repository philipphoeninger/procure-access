# S4-21b — Export dialog S13

> **Sprint:** 04, P0 task S4-21b (client part of S4-21, M2.7; split by O-S4-73). **Issues:** none. **Requirements / invariants:** R-LST-11 (export dialog S13), R-LST-12 (the download); SPEC §7.3 (429 on the client); A11Y-01, A11Y-08, A11Y-09 stay green; no invariant changes.
> **Status:** done — pull request #294 open.
> **Scope:** `ExportDialog` (S13, S13-E, S13-M) in place of the placeholder of S4-18a, the export request and the download, the request marker `CALLER_SHOWS_FAILURES`, Problem Details read from a `Blob` error body, UX copy v25 U110 (`errors.rateLimit.one`), the panel class `pa-dialog`; ARCHITECTURE_v41 (C127). Not here: the reload flow after `release-mismatch` (follow-up, O-S4-72), J1 to the export (S4-22).
> **Owner decisions (2026-11-12, all (a)):** **O-S4-71** — UX copy v25 U110 `errors.rateLimit.one` („… Bitte warten Sie 1 Minute …“, wording confirmed). **O-S4-72** — `criteria.export.outdated` with „Schließen“ only; the reload flow is a follow-up. **O-S4-73** — S4-21 split; this is the client part, after S4-21a (#293, merged). **O-S4-74** — the interceptor reads Problem Details from a `Blob` body, and a new marker turns off snackbar and server page for a request whose caller shows every failure (the offline notice stays).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | „Liste exportieren“ opened `ExportPlaceholderDialog` (S4-18a); the Figma mapping and the link check already expected `dialogs/export-dialog/export-dialog.ts` with `2134-3825` first | The placeholder is deleted; the dialog takes the mapped path |
| F2 | The error interceptor shows the server page for 5xx and a snackbar for other problems; S13 must show every failure itself (Handoff §5.1) | O-S4-74: `CALLER_SHOWS_FAILURES` |
| F3 | A request with `responseType: 'blob'` gets its error body as a `Blob`, so the problem type of a 429 or `release-mismatch` could not be read | `withProblemBody` reads a JSON `Blob` before the interceptor's checks |
| F4 | MatDialog's default first focus is the first tabbable element; on iOS the CDK counts fewer elements as tabbable (S4-18a) | `autoFocus: 'input[type="radio"]:checked'` |
| F5 | `#closeButton` on a `matButton` refers to Material's button component, not the element | `viewChild(…, { read: ElementRef })` |
| F6 | In E2E the dialog moves the focus to the radio a moment after it is visible; an arrow key sent earlier is lost | The E2E helper waits for the focused radio |
| F7 | The 360 px frame's note (Figma) puts the error notice at the top of the content with the focus on it; the Handoff says above the actions, `role="alert"` | Built as the Handoff says (it is authoritative for behaviour); listed in §3 |
| F8 | The release build warns „Could not parse 1 selector … 50%“ | CodeMirror's legacy `@-moz-keyframes`; the same warning without this change (checked), not this task |

## 2. Changes

| File | Change | Requirement |
|---|---|---|
| `features/configurator/dialogs/export-dialog/export-dialog.{ts,html,scss}` (new) | S13: format fieldset (XLSX preselected), scope, release and anonymous note, ⚖️ legal note; button spinner + `aria-disabled`; success → download, `criteria.export.success`, close; failure → error notice above the actions (`role="alert"`), „Erneut versuchen“; 429 → `errors.rateLimit` with minutes; `release-mismatch` → `criteria.export.outdated`, „Schließen“ only with the focus | R-LST-11 |
| `features/configurator/dialogs/export-placeholder/` | Deleted | — |
| `features/configurator/models/export.ts`, `state/export-request.ts` (new) | Body of `POST /api/v1/exports` from the shown list (`toExportRequest`); `fileNameOf` for `Content-Disposition` | R-LST-12, EXP-07 |
| `features/configurator/services/api/export-api.service.ts` (new) | Posts the body as `blob`, marked `CALLER_SHOWS_FAILURES`; returns content and file name | R-LST-12 |
| `core/files/file-saver.ts` (new) | Download through a temporary object URL | R-LST-12 |
| `core/errors/problem-details.ts` | `CALLER_SHOWS_FAILURES`; `problemDetails` exported, ignores a `Blob`; `withProblemBody`; `retryAfterMinutes` | O-S4-74 |
| `core/interceptors/error.interceptor.ts` | Reads a JSON `Blob` body first; no snackbar or server page for `CALLER_SHOWS_FAILURES` | O-S4-74 |
| `features/configurator/pages/criteria-list/criteria-list.ts` | „Liste exportieren“ → `ExportDialog.open` | R-LST-10 |
| `styles/main.scss` | Panel class `pa-dialog`: full screen below 600 px, 600 px from there | Handoff §2.3 |
| `docs/ux_copy_procureaccess_v25.md` (v23 deleted), `public/i18n/de.json` | U110 `errors.rateLimit.one` | O-S4-71 |
| Tests: `export-dialog.spec.ts`, `export-request.spec.ts`, `export-api.service.spec.ts` (new), `error.interceptor.spec.ts`, `criteria-list.spec.ts`; `e2e/configurator/s11-list.spec.ts`, `e2e/shell/reflow.spec.ts` | Dialog states, body, file name, marker and `Blob` body; E2E by keyboard with download name, 429 alert, axe in both appearances; S13 at 320 px | R-LST-11; A11Y-01, A11Y-08, A11Y-09 |
| `docs/ARCHITECTURE_v41.md` (C127; v39 deleted), `CLAUDE.md` §3/§4 | Documentation | — |

## 3. Self-review (CLAUDE §6.7) and Figma comparison

Pass with the checklist (security, correctness, edge cases, a11y) over the diff against `main`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | `#closeButton` read the component, so the focus never moved to „Schließen“ (found by the component test) | **Fix** | `read: ElementRef` |
| R2 | A note above 1 000 characters makes the export a 400 `validation`, which S13 shows as the generic `criteria.export.error` | **Won't fix** | S11 already shows `errors.field.maxLength` at that note (S4-16); the server stays authoritative (SPEC §7.1) |
| R3 | Esc while the file is requested closes the dialog; the request continues | **Won't fix** | Its answer is dropped (no download, no announcement); the server stores nothing (EXP-05) |
| R4 | Every failed request now passes one more `catchError` with a microtask (`withProblemBody`) | **Won't fix** | Only a `Blob` body is read; all 504 client tests and the E2E runs unchanged |

**Figma comparison** (S13 `2021:225`, S13-E `2039:1944`, S13-M `2134:3825`; screenshots of the built dialog on `T.1` at 1440 × 1000 and 360 × 949). Order, texts, notices, button order and the error state match. Visible differences (the Handoff and the UX copy stay authoritative):

| # | Difference | Built as |
|---|---|---|
| D1 | The radio labels are larger in Figma (body-large) | Angular Material's radio label size |
| D2 | S13-M has a tinted title bar and a divider above the actions | Title and actions on the dialog surface; the Handoff names no colour or divider |
| D3 | S13-M shows „Datei herunterladen“ as a centred button, not full width | Full width, as Handoff §2.3 says for stacked actions |
| D4 | The release and anonymous notes are one paragraph in Figma | Two paragraphs in one info notice (two UX keys) |
| D5 | The note of S13-M puts the error notice at the top of the content with the focus on it | Above the actions with `role="alert"`, as Handoff §5.1 says (F7) |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest) | 504 of 504 pass |
| C2 | `npm run lint` (client), `npm run typecheck && npm run lint` (e2e) | Pass |
| C3 | `npm run build:release` | Pass; one pre-existing CSS warning (F8) |
| C4 | E2E locally on the E2E stack: `s11-list.spec.ts` and `reflow.spec.ts` in Chromium, Firefox, mobile Chrome | First run 54 of 60 (the download and 429 tests failed on F6); after the fix the S13 tests again: 9 of 9 pass. WebKit left to CI |
| C5 | `check-figma-links.mjs` (S13 now checked), `check-i18n.mjs`, `check-icons.mjs`, `check-invariants.mjs`, `reuse lint` | Exit 0 each / compliant |
| C6 | Backend | No backend change |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request #294 opened; not merged | Owner |
| P2 | Reload flow after `release-mismatch`: unpin the release, reload S11 for the same selection, drop the per-criterion changes as the text says, S10 if the selection is excluded now; needs a UX copy key for the action | Proposed issue (O-S4-72) |
| P3 | Figma differences D1–D5 | Owner / designer: adjust Figma or ask for a change |
| P4 | J1 to the export with the downloaded file | S4-22 |
| P5 | The deployment check of S4-21a P2 (no published API port, no direct Traefik route) | Owner |

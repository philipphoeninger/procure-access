# S5-21b — Edit, delete and export saved lists; older release (client)

> **Sprint:** 05 · **Task:** S5-21b (client part of S5-21, 4.5 points for S5-21 in all) · **Issues:** none
> **Requirements / invariants:** SPEC R-SAV-03, R-SAV-06, R-LST-12, R-LST-14 (client parts, v2.38); J1 second part complete with the file comparison (EXP-06, TESTING_v78 V263) and with it the M3 exit criterion; component tests V264; A11Y-09 V265; UX copy v34 V266; ARCHITECTURE C156. No invariant changes its status
> **Status:** done — pull request #358 open (2026-11-16)
> **Scope:**
> - a saved list editable for Editor and Owner, read only with export for Readers;
> - „Auswahl ändern“ on S10 apart from the draft;
> - the older-release notice with „Neue Liste mit derselben Auswahl“ and the report on the copy;
> - S13 for a saved list;
> - „Liste löschen“ on S14;
> - UX copy v34;
> - J1's file comparison.
>
> No server change (S5-21a, #357).
> **Owner decisions (2026-11-16, before the task):**
> - **1a** — every change saved at once, a note after a pause in typing; a failed save is undone and the general error shown.
> - **2a** — „Auswahl ändern“ on S10 at `/kriterienlisten/:id/auswahl`, apart from the draft; „Abbrechen“ (common text) back to the list.
> - **3a** — `criteria.list.selectionChanged.dropped` and `.one` confirmed as worded.
> - **4a** — „Geändert. Empfohlen war: …“ everywhere, plus the R4 wording of `criteria.list.copy.notCarried`.
> - **5b** — Readers don't see the older-release notice.
> - **6a** — Readers get the filter row and an action bar with only „Liste exportieren“.
>
> Earlier decisions for this task: S5-21 4a, 5a, 7a, 8b (S5-21a).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S10 and S11 work on the one draft of `withConfigurator()`; a second S10 state would mean a second store or a refactor of S10 | „Auswahl ändern“ puts the draft aside in the store and brings it back when S10 is left (`startSavedListSelection`, `endSavedListSelection`); one store, one S10 |
| 2 | The list may belong to another organisation than the one the header shows | The role comes from the user's membership in the list's organisation |
| 3 | The client knew which release is active only on S10 | The loader of a saved list also asks the filter values without a release |
| 4 | S11's action bar with its focus rule (WCAG 2.4.11) was code in the S11 page | Moved into `pa-criteria-action-bar`, used by S11 and the saved list |
| 5 | Figma S11 (`2020:70`) puts „Auswahl ändern“ at the top, beside the H1; the plan had it in the action bar | Placed below the H1, as on S11 (deviation from the plan) |
| 6 | Export and copy read the stored list; a note waits 800 ms before it is saved | `SavedListEditor.settled()` before both (self-review R2) |

## 2. Changes

### 2.1 Client

- **`pages/saved-list/`**
  - `saved-list-page` (edit or read, filter row, counts, announcements, older-release notice and copy, arrival report, „Auswahl ändern“, export, `@figma` S11).
  - `saved-list-editor.ts`: `SavedListEditor`, `toDecisionRequest`, `NOTE_SAVE_DELAY_MS`.
  - `saved-list-load.ts`: `activeRelease`, `SavedListArrival`, `SavedListNavigationState`.
  - `saved-list-selection.ts`: `SavedListSelection`, `savedListSelectionGuard`.
  - `saved-list.routing.ts`: the list route is `pathMatch: 'full'`; new child route `auswahl` with lazy S10.
- **`pages/criteria-select/`**: S10 in saved-list mode — „Abbrechen“, saving the selection with `PUT …/selection`, then the list with the dropped decisions; the draft brought back on destroy.
- **`state/with-configurator.ts`**: `startSavedListSelection`, `endSavedListSelection`, `draftOf`; `resetConfigurator` also forgets a draft put aside.
- **`dialogs/export-dialog/`**: optional `SavedListExport`; `ExportApiService.exportSavedList`.
- **`pages/saved-lists/`**: „Liste löschen“ for Editor and Owner, `ConfirmDialog`, announcement, reload, focus on the H1.
- **`components/criteria-action-bar/`**: new; S11 uses it.
- **`services/api/configurator-api.service.ts`**: `decide`, `changeSelection`, `delete`, `copyToActiveRelease`. **`models/saved-lists.ts`**: the request and answer types, `savedListSelectionPath`.
- **`de.json`**: from UX copy v34 through `check-i18n.mjs --write`.

### 2.2 Tests

- **Component tests:**
  - `saved-list-page.spec.ts` (rewritten for the editable page);
  - `saved-list-editor.spec.ts` (new);
  - `saved-lists-page.spec.ts` (delete);
  - `export-dialog.spec.ts` (saved list);
  - `criteria-select.spec.ts` (saved-list mode);
  - `with-configurator.spec.ts` (the draft put aside);
  - the specs that asserted „Von Ihnen geändert“.
- **E2E:**
  - `journeys/j1-configurator.spec.ts`: second test with the draft export, the saved export and its comparison, the deletion through S14, and `exportThroughS13`;
  - `shell/reflow.spec.ts`: the editable list and the delete dialog;
  - `configurator/s11-list.spec.ts`: locates `pa-criteria-action-bar`.

### 2.3 Documents

- UX copy v34 (U145 – U147; v32 deleted).
- TESTING_v78 (V263 – V266; v76 deleted).
- ARCHITECTURE_v70 (C156; header references; v68 deleted).
- SPEC v2.38.
- `CODEBASE_v1.md` §2.71 and §3.
- `CLAUDE.md` (state, last task).
- No Figma mapping or `LEGACY_EXCEPTIONS` change; the saved list names the S11 frame in its `@figma` tag.

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | A save still running when „Auswahl ändern“ is followed can reach the server after the selection change. For a criterion that stays, the newest decision still wins; for one that leaves, the save answers 404 on a page already gone. | **won't fix**: the window is milliseconds and nothing is lost; API-03 in step 5 |
| R2 | Export and copy could miss a change still being saved | **fixed**: `settled()`, tested |
| R3 | The report after a copy or a selection change shows again on „Zurück“ (history state) | **won't fix**: it is the same report and still true |
| R4 | A note over 1 000 characters isn't saved and is lost when the page is left | **kept**: S11's rule; the card shows the length error |
| R5 | E2E not run locally: the disk had 2.1 GB free and building the E2E stack needs about 2 GB (it filled the disk once, S2-15) | **CI is the authority** |
| R6 | S11's action bar moved into a component | **checked**: S11 specs unchanged and green |
| R7 | Security: <ul><li>every request is checked by the server (#357); the client role checks only decide what is shown;</li><li>no storage: the draft put aside and the reports live in memory;</li><li>sign-out forgets the draft put aside (R-AUTH-08).</li></ul> a11y: <ul><li>„Liste löschen“ is described by the list's name;</li><li>the focus goes to the H1 after a delete;</li><li>reports are shown and announced;</li><li>„Abbrechen“ is a link;</li><li>axe runs on every new component state and in J1.</li></ul> | — |

## 4. Checks

- **Client:** `ng build` clean, `npm run lint` clean, `ng test` **707 passed**.
- **E2E:** `npm run typecheck` and `npm run lint` clean. The tests themselves were not run locally (R5); CI runs all browsers.
- **Scripts:** `check-invariants`, `check-figma-links`, `check-i18n` (with self-test), `check-icons` passed; `reuse lint` compliant.
- **Backend:** unchanged.
- **Figma:** S11 frame `2020:70` compared; „Auswahl ändern“ placed as there.

## 5. Open points

1. **#355** (Firefox page crash after saving): J1's second test, now with the file comparison, stays `test.fixme` on Firefox.
2. **Step 5:** ETag/If-Match with S37 for saved lists (S5-21 2a; R1).
3. **S5-22:** `criteria.list.unsaved`, the weight range from the API, `no-store`.
4. **S5-23:** deleting a list ends its share links.
5. **UX copy v33** (U140 – U144) still waits for the owner's confirmation; v34 (U145 – U147) is confirmed.

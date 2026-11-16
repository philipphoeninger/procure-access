# S5-22 — Saved-list debt: weight range, too-long note, unsaved state, no-store

> **Sprint:** 05 · **Task:** S5-22 (M3.5d, 1.5 points) · **Issues:** none
> **Requirements / invariants:**
> - **TEN-04 workspace part → `active`** (TESTING_v79 V267);
> - SPEC R-LST-03, R-LST-10, R-LST-11 and the new §8.11 (v2.39);
> - TECH_DEBT TD-40 and TD-45 settled;
> - UX copy v35 U148, U149; ARCHITECTURE C157; client tests V268.
>
> **Status:** done — pull request open (2026-11-16)
> **Scope:**
> - the weight range served by the API instead of the client's copy (TD-45);
> - S13 leading to a note over 1 000 characters (TD-40);
> - `criteria.list.unsaved` on S11;
> - `Cache-Control: no-store` on the workspace answers (TEN-04 workspace part).
>
> No EF migration.
> **Owner decisions (2026-11-16, before the task):**
> - **1a** — the weight range from an anonymous Configurator endpoint `GET /api/v1/configurator/settings`, because the anonymous draft needs it too; the client constant goes.
> - **2a** — new UX copy keys `criteria.export.noteTooLong` and `criteria.export.toNote` with the proposed wording; a too-long note stays open on its card.
> - **3b** — `no-store` as the default of every `/api/v1` answer without a header of its own, not only of the saved-list endpoints.
> - **4** — confirmed reading: `criteria.list.draftNote` stays for signed-out users only, its wording unchanged (O-S4-54); `criteria.list.unsaved` on S11 for everyone, not on a saved list.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The row says „the weight range served with the saved-list API“, but the anonymous S11 draft uses the range too (`withConfigurator`, the card), and an anonymous user never calls `/configurations` | Owner decision 1a: an anonymous settings endpoint of the Configurator |
| 2 | UX copy v34 had no text for a too-long note in S13 | Owner decision 2a: U148, U149 in UX copy v35 |
| 3 | `no-store` was set by hand on the export, the account export and the session probe only | Owner decision 3b: a Platform middleware for every `/api/v1` answer |
| 4 | Figma S11 (`2020:70`) has a hidden layer „Unsaved status (ab Migrationsschritt 3, ausgeblendet)“ under the counter of the action bar | The second line goes into the status block of `pa-criteria-action-bar`, as there |
| 5 | A saved list never stores a too-long note (S5-21b R4), so its export succeeded without it | S13 checks the editor's notes too |
| 6 | The server refuses a too-long note on any criterion of the draft, excluded ones too | `firstTooLongNote` checks every criterion in list order; „Zur Notiz“ clears a filter that hides the card |
| 7 | PR #358 (S5-21b) was still open at the start | The owner merged it first; the branch starts from main after it |

## 2. Changes

### 2.1 Server

- `Configurator/Settings/ConfiguratorSettingsEndpoint.cs`:
  - `GET /api/v1/configurator/settings`, anonymous, no rate limit;
  - answers `{ priorityWeight: { min, max } }` from the singleton `PriorityWeightRange`;
  - mapped in `MapConfiguratorEndpoints`.
- `Platform/Caching/NoStoreDefault.cs`:
  - `UseNoStoreDefault`, first in `UsePlatformMiddleware`;
  - sets `Cache-Control: no-store` in `Response.OnStarting` when an answer below `/api/v1` has none.
  - It also covers 401, 403, 404, 429 and the antiforgery 400. The catalog keeps its own header, and the legacy controllers are untouched.

### 2.2 Client

- **Weight range:**
  - `state/configurator-settings.ts` (`ConfiguratorSettings`): `ready()` loads the range once; a failure resolves false and is not kept. `weightRange()` reads the range.
  - `ConfiguratorApiService.settings()`, not marked, so a failure shows the system state.
  - `completeSelectionGuard` (S11) waits for it; `savedListResolver` loads it beside the list, and a failure becomes the server view.
  - `withConfigurator`, `CriterionCard` and `SavedListEditor` read the range there.
  - `PRIORITY_WEIGHT_RANGE` and `environment*.priorityWeight` are deleted.
  - Test helper: `testing/configurator-settings-stub.ts`.
- **Too-long note:**
  - `firstTooLongNote` in `criteria-list-rules.ts`.
  - `ExportDialog` checks before the request, shows status `noteTooLong` and turns the primary button into „Zur Notiz“. The dialog closes with `ExportDialogResult { toNote }`.
  - `SavedListExport.tooLongNote` comes from the editor.
  - S11 and the saved-list page clear a filter that hides the card, then call `CriterionCard.showNote()`, which opens the note, marks it touched and moves the focus into it.
  - `toggleNote()` keeps a too-long note open.
- **Unsaved state:** `CriteriaActionBar.saveState`, a second line in the status block. S11 passes `criteria.list.unsaved`; the saved list passes nothing.
- **Texts:** `de.json` with `criteria.export.noteTooLong` and `criteria.export.toNote` (`check-i18n.mjs --write`).

### 2.3 Tests

- **Integration, `INV:TEN-04`:**
  - every row of the TEN-05 matrix carries `no-store`;
  - every GET of the versioned API except the catalog, signed in and anonymous, enumerated from the endpoint data source;
  - an unknown path;
  - the settings endpoint answers the configured range.
  - Without the middleware, 94 of the 102 TEN-04 tests fail.
- **Vitest:**
  - `configurator-settings.spec.ts` and `saved-list.routing.spec.ts` (new);
  - the S11 guard when the settings fail;
  - S13's too-long note on the S11 page, including the filter that hides an excluded card;
  - the card keeps a too-long note open;
  - the saved list hands its note to S13 and opens it;
  - „Noch nicht gespeichert“ on S11 and not on the saved list;
  - `firstTooLongNote`.
- **E2E:** `s11-list.spec.ts` checks „Noch nicht gespeichert“ in the action bar.

### 2.4 Documents

- **SPEC v2.39:**
  - §8.11;
  - R-LST-03, R-LST-10, R-LST-11.
- **ARCHITECTURE v71:**
  - C157;
  - §7.2, §9.6, §9.7.
- **TESTING v79:**
  - V267, V268;
  - TEN-04 row.
- **UX copy v35:** U148, U149.
- **CLAUDE.md:**
  - §3 state;
  - the rule on client copies of server values;
  - the `no-store` default.
- **CODEBASE:**
  - §1;
  - §2.72;
  - §3.

Removed: ARCHITECTURE_v69, TESTING_v77, UX copy v33. U140 – U144 of v33 stay in v35's change history.

## 3. Self-review

Checklist of CLAUDE §6.7 and `/engineering:code-review` on the diff.

| # | Finding | Sorting |
|---|---|---|
| R1 | If the settings fail while a saved list opens, the interceptor shows the server page (5xx) or the offline notice (status 0), and the resolver also yields the server view. The result is the same server page, or the offline notice over the S80 text | **won't fix**: the same state the other unmarked requests show; the settings request has no caller that could show it better |
| R2 | A Reader of a saved list needs no weight range, but a failure of the settings blocks the page for them too | **won't fix**: one rule for the page; the endpoint is a constant from memory |
| R3 | No test provokes a 429 for `no-store` | **won't fix**: the middleware runs before the rate limiter, so the 429 passes through it; recorded as deferred in V267 |
| R4 | The S13 check uses the client's `NOTE_MAX_LENGTH` (1 000, SPEC §7.2), a copy of the server's limit | **kept**: a field limit of SPEC §7.2, not a 🔧 value; the server still checks, and its 400 shows the generic error. Mapping that 400 to the note (named in the plan as a backup) was left out: it could only name the client's limit, which is the wrong number in exactly the case it would catch |
| R5 | A JSDoc line of `criteria-list.ts` over the line width | **fixed** |
| R6 | Security and a11y | **checked:** <ul><li>the new endpoint exposes a configured range only, without data of any user;</li><li>`no-store` adds a header and removes none;</li><li>the too-long-note error is `role="alert"` with the criterion named;</li><li>„Zur Notiz“ is the same button element, so the focus stays on it;</li><li>after closing, the focus goes into the note field with its error linked by `aria-describedby`;</li><li>axe runs on the new dialog state.</li></ul> |

## 4. Checks

- **Backend:**
  - `dotnet format --verify-no-changes` clean;
  - `dotnet build -warnaserror` clean;
  - `dotnet test --solution` **1 575 passed** (unit, architecture, integration);
  - the 102 TEN-04 tests pass.
- **Client:**
  - `ng build` clean;
  - `npm run lint` clean;
  - `ng test` 719 passed.
- **E2E:**
  - `npm run typecheck` and `npm run lint` clean;
  - the suite itself was not run locally; CI runs all browsers.
- **Scripts:**
  - `check-invariants`;
  - `check-figma-links`;
  - `check-i18n`;
  - `check-icons`;
  - `reuse lint`.
- **Figma:**
  - S11 `2020:70`: the unsaved line under the counter, as the hidden layer `2034:1196`.
  - S13-E `2039:1944`: the error notice above the actions with „Abbrechen“ and the primary button.

## 5. Open points

1. **Figma:** the layer `2034:1196` „Unsaved status (ab Migrationsschritt 3, ausgeblendet)“ of S11 can now be shown. I changed nothing in Figma.
2. **TECH_DEBT:** TD-40 and TD-45 are settled. The new TECH_DEBT version at the end of the sprint (process step 14) records it.
3. **TEN-04 directory part** with step 6.
4. **UX copy v33** (U140 – U144) still waits for the owner's confirmation; v34 and v35 are confirmed.
5. **Field limits** of SPEC §7.2 (`NOTE_MAX_LENGTH`, `LIST_NAME_MAX_LENGTH`) are still client copies. They could be served like the weight range if the owner wants it; not in this row.

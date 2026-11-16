# S5-20b — Save dialog S12, saved lists S14, J1 second part (client)

> **Sprint:** 05 · **Task:** S5-20b (client part of S5-20, 4 points for S5-20 in all) · **Issues:** #355 (opened: Firefox crash, see §5)
> **Requirements / invariants:** SPEC R-SAV-01, R-SAV-02 (client part), R-SHL-03 (the saved lists read the active organisation), R-AUTH-03 (`auth.login.reasonSave`, `draftKept`); J1 second part up to the save (V249); SEC-10 client part for saving (V250); TEN-01 switch test of V232 (V251); A11Y-01, A11Y-02, A11Y-08, A11Y-09 stay green with new states (V249, V252); Firefox crash class (V253, V254); ARCHITECTURE C154; SPEC v2.36; UX copy v33 U140 – U144 (drafts). No invariant changes its status
> **Status:** done — pull request open (2026-11-16)
> **Scope:** „Liste speichern“ on S11 with the draft kept across the sign-in, S12, a saved list as S11 read only, S14 with its menu entry, J1's second part, the reflow checks. No server change (S5-20a). Edit, delete and the saved export are S5-21; `criteria.list.unsaved` and `no-store` S5-22.
> **Owner decisions (2026-11-16):** S5-20 **2a** — S14 gets an entry in the menu „Konto“ · **3a** — the missing UX copy is drafted for review (U140 ff.); S14 shows name, date and catalog version, not the creator · **4a** — after saving, the page becomes the saved list and the draft is dropped · **5a** — a saved list opens read only until S5-21 · **6a** — J1 builds its own account and organisation through the API · **7a** — J1 runs through the save; the file comparison and the M3 exit criterion move to S5-21 (recorded by S5-20a) · S5-20b **1a** — after the sign-in from „Liste speichern“ S11 shows `auth.login.draftKept`; S12 does not open by itself · **2a** — one „Sortieren nach“ select in every width, cards below 600 px, a plain table from 600 px · **3a** — without a membership `criteria.save.needOrg` leads to S20, no special way back · **4a** — the entry is „Kriterienlisten“, between „Organisation“ and „Einstellungen“, only with a membership · **during the task:** skip J1's second test on Firefox and open an issue for the page crash (§3 R6, §5)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-20a built save, read and list; S01 already showed `reasonSave` and announced `draftKept` (S5-09) | S11 calls `showLogin('save')`; S01 also sets a store flag for the visible hint |
| 2 | S12 and S14 have no Figma frame; Handoff §5.3: S12 as S13, S14 as a list with sorting by name/date and announced results | Built from S13's dialog and S21's card/table pattern; recorded in Handoff 1.26 |
| 3 | The UX copy had no keys for the menu entry, the sort, the list fields and S12's errors; on a read-only card the note label „Ihre Notiz zu diesem Kriterium (optional)“ read wrong as a heading | Drafted as U140 – U144 in UX copy v33 (decision 3a) |
| 4 | The criterion card and K5 had no read-only mode; R-SAV-05 (S16) will need the same | `readonly` input on both; S16 can reuse it |
| 5 | The session has no confirmation flag | S12 learns an unconfirmed address from the 403, as S20 |
| 6 | V232 named S5-21 for the switch test of the saved lists, because S14 was planned there | The test comes with S14 now (V251) |

## 2. Changes

### 2.1 Client

- **S11** (`pages/criteria-list/`): „Liste speichern“ (outlined, before „Liste exportieren“) by `saveAction` — signed out `AuthService.showLogin('save')`; no membership `ConfirmDialog` with `criteria.save.needOrg` → `/organisation`; Reader none; Editor/Owner S12. The info notice `auth.login.draftKept` while `withConfigurator().draftKept` (set by S01, cleared when S11 is destroyed). From 600 px the counter keeps the start of the action bar, the buttons share its end.
- **S12** `SaveListDialog` (`dialogs/save-list-dialog/`): as in ARCHITECTURE C154 (2).
- **A saved list** (`pages/saved-list/`): route `/kriterienlisten/:id` (lazy, `authGuard`, resolver), `SavedListLoad`, `loadSavedListView`, `decisionsOf`, `selectionOf`, `SavedListPage`.
- **S14** `SavedListsPage` (`pages/saved-lists/`): route `/kriterienlisten` (`authGuard`), `sortSavedLists`.
- **Read only:** `pa-criterion-card` and `pa-priority-indicator` `readonly`.
- **Menu:** „Kriterienlisten“ in `main-nav` (account menu and compact list).
- **Plumbing:** `models/saved-lists.ts` (types, `SAVED_LISTS_PATH`), `ConfiguratorApiService.save|get|list` (`CALLER_SHOWS_FAILURES`), `toSaveRequest`, `keepDraftAfterSignIn`/`forgetDraftKept` in `withConfigurator()`, `app.routes.ts`, `de.json` (`check-i18n.mjs --write`; `AREAS` + `criteria.save`, `criteria.saved`).

### 2.2 Tests

- Component: `save-list-dialog.spec.ts` (11: S11 states, the hint, S12 focus, name rules, success with request, route, dropped draft and announcement, `INV:SEC-10`, `release-mismatch`, 403, server field errors; axe), `saved-lists-page.spec.ts` (9: cards and table, sort and announcement, `INV:TEN-01` switch, empty, no membership, failure, `sortSavedLists`), `saved-list-page.spec.ts` (6: H1 and title, read-only decisions, detail panel, not found, `decisionsOf`, `selectionOf`); extended: `criteria-list.spec.ts`, `export-dialog.spec.ts` (doubles for `AuthService`, `activeOrganization`), `main-nav.spec.ts` (the entry, only with a membership), `login-page.spec.ts` (`keepDraftAfterSignIn`).
- E2E: J1's second test (V249; CSS locators of V253; `test.fixme` on Firefox, V254), `reflow.spec.ts` S12, S14, a saved list (V252), `configurator/s11-list.spec.ts` (two buttons in the action bar), `support/accounts.ts` `organizationOfAccount`.

### 2.3 Documents

UX copy v33 (U140 – U144, drafts; v31 deleted), TESTING_v76 (V249 – V254; J1 and A11Y-02 rows; v74 deleted), ARCHITECTURE_v68 (C154; v66 deleted), SPEC v2.36, Handoff 1.26 (§5.3), `CODEBASE_v1.md` §2.69 and §3, `CLAUDE.md` (state, last task). No Figma mapping change (no frame for S12, S14), no `LEGACY_EXCEPTIONS` change.

## 3. Self-review

Code review per CLAUDE §6.7, sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `saveListThroughApi` (E2E) ended up unused: the reflow checks answer by `page.route` | **fixed** — removed |
| R2 | Could „Liste speichern“ show the organisation question before the memberships are loaded? | **no** — the sign-in awaits the session with the organisations before S01 closes; a reload loses the draft anyway (R-CON-09) |
| R3 | After saving, the draft is dropped also when the route change is cancelled | **won't fix** — the list is saved and reachable from S14; cancelling needs a second navigation in that moment |
| R4 | A read-only card says „Von Ihnen geändert“ also to a member who did not save the list | **follow-up (UX review)** — the text is the UX copy's (`criteria.priority.overridden`); noted for the review of v33 and for S5-21, which edits saved lists |
| R5 | S14 loads all lists without paging | **won't fix** — the server has none (SPEC §8.10) |
| R6 | Firefox page crash after saving in J1 (V254) | **owner decision:** `test.fixme` on Firefox, issue #355 |
| R7 | Security: no token or draft in storage; requests marked `CALLER_SHOWS_FAILURES`; the organisation of a save is the one the header shows, checked by the server (TEN-05); a saved list is read through the server's filter (404). a11y: dialog rules of S13, error summary and field errors, the confirmation notice takes the focus, the table named by the H1, cards with H2, every change of order and load announced; axe in both appearances on every new state | — |

## 4. Checks

- Client: `ng build` clean, `npm run lint` clean, `ng test` **680 passed**.
- E2E (local stack built from the branch): Chromium, Firefox and mobile Chrome **371 passed** before the `test.fixme`, the one failure being V254; J1 file again after it (Firefox: first test passed, second skipped; Chromium: both passed). WebKit and mobile Safari run only in CI.
- `check-invariants`, `check-figma-links`, `check-i18n` (+ self-test), `check-icons` passed; `reuse lint` compliant. Backend unchanged.

## 5. Open points

1. **#355** — Firefox page crash after saving (V254): check by hand in a real Firefox, then narrow it down in CI; remove the `test.fixme`.
2. **UX copy v33** (U140 – U144) awaits the owner's confirmation; R4 belongs to that review.
3. **S5-21**: edit on the saved list (the read-only mode stays for S16), delete, the saved export, J1's file comparison and the M3 exit criterion.
4. **S5-22**: `criteria.list.unsaved`, the weight range from the API, `no-store`.

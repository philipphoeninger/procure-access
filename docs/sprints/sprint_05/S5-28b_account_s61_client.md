# S5-28b — Account page S61, client part

> **Sprint:** 05 · **Task:** S5-28b (client part of S5-28, 3 points for S5-28 in all) · **Issues:** #234 (TD-26 — closed with this task; TD-25 went in S5-25b)
> **Requirements / invariants:** SPEC R-AUTH-09, R-AUTH-10 (met for what is built), §8.13 (v2.46); **TEN-11** client part (TESTING_v86 V289), **A11Y-09** with S61 (V290), A11Y-01, A11Y-08; ARCHITECTURE C164. No invariant changes its status
> **Status:** done — pull request #371 open (2026-11-17)
> **Scope:** S61 at `/konto` with the download of the personal data export and the account deletion through the dialog „Konto endgültig löschen?“; `GET /api/v1/account` for the notice of an unconfirmed address; the legacy profile page, its two dialogs, `identity-api.service.ts`, `DELETE /api/Users` with `UsersController` and `UserRepo`, their texts, icons, tests and `LEGACY_EXCEPTIONS` entries removed.
> **Owner decisions (2026-11-17):** from S5-28: **1a** — no change of e-mail address or password on S61 · **5a** — the proposal lists of the legacy profile go with the page · **P2 (a)** — the deletion request has no body; confirmed again after ARCHITECTURE §7.2's planned `{ emailConfirmation }` was named (S5-28a open point 1) · **P3 (a)** — no message after the deletion, as the sign-out. In this task: **S5-28b 1** — a new `GET /api/v1/account` (`{ email, emailConfirmed }`) tells S61 whether the address is confirmed; the session probe stays as it is · **2** — new UX key `account.delete.error.title` „Konto nicht gelöscht“ for the title of the refusal notice · **3** — the dialog's address field is labelled with `auth.login.email`

## 1. Starting point

- S5-28a (PR #370, merged) built `DELETE /api/v1/account` with 409 `sole-owner`; the export `GET /api/v1/account/export` exists since S5-27. Neither had a screen.
- `/profile` was the legacy page: a table with a „change“ dialog that ended in a `console.log`, the user's proposals, and the delete dialog with native `disabled` and the low-contrast title of TD-26. It called `DELETE /api/Users`, which left memberships and organisations behind.
- **Three things the documents did not answer**, asked before building (decisions S5-28b 1 – 3): the client could not know whether the address is confirmed (Handoff §5.3 wants `auth.confirm.required` on S61); an error notice needs a title and §5.14 had none; the dialog's field had no label key.
- The Figma mapping has no frame for S61; the page follows Handoff §5.3 (form card, as S60).

## 2. Changes

### 2.1 Server

- `API/Account/AccountEndpoints.cs`: `GET /api/v1/account` → `{ email, emailConfirmed }`; signed in (401), 404 for a deleted account, `no-store` by Platform's default.
- Removed: `UsersController` (`DELETE /api/Users`), `IUserRepo`, `UserRepo` and their registration.

### 2.2 Client

- **Route** `/konto` (`ACCOUNT_PATH`), lazy `account.routing.ts` with `authGuard`, `AccountLoad` and `accountResolver`; „Konto“ in the menu „Konto“ and the compact menu points there.
- **Page** `features/identity/pages/account/`: H1, the confirmation notice for an unconfirmed address, two outlined cards. „Daten herunterladen“ saves the export under the server's file name. „Konto löschen“ opens the dialog; after the deletion `store.forgetUser()` clears every in-memory state and the start page shows with the focus on its H1. A 409 `sole-owner` shows the error notice „Konto nicht gelöscht“ with one line per organisation; other failures keep the interceptor's system states; a failed read of the account shows S80 in place.
- **Dialog** `features/identity/dialogs/delete-account-dialog/` rebuilt: one address field, „Konto behalten“, and „Konto endgültig löschen“ with `aria-disabled` (focusable, does nothing) and its visible reason linked by `aria-describedby` until the address matches exactly. Through the Material button's own `aria-disabled` input: its host binding overwrites an attribute binding that starts as `true` (new line in CLAUDE §3).
- **API service** `services/api/account-api.service.ts`; `fileNameOf` moved to `core/files/content-disposition.ts`, so the configurator's export and the account export share it.
- **Store:** `withIdentity().forgetSession()`, `ProcureAccessStore.forgetUser()`; the legacy `deleteAccount` with its English snackbar is gone. `PROBLEM_TYPE_KEYS['sole-owner']`.
- **Removed:** `features/identity/profile/`, `dialogs/update-user-dialog/`, `services/identity-api.service.ts`, the route `/profile`; three `LEGACY_EXCEPTIONS` entries (profile, `identity-api.service.ts`, `with-identity.ts`); 16 legacy text keys in each of the four language files (the whole `AUTH` group); five icons only the profile used (font subset regenerated: 26 icons).
- **Texts:** UX copy v38 (U158, U159); `account` joins `AREAS` of `check-i18n.mjs`. The script now drops the marker „⚖️“ in front of a text: `account.delete.text` is the first ⚖️ text of a built area, and the marker is a note for the reader (UX copy §5.13), not wording. The text itself is as written.

### 2.3 Tests

- FE: `account-page.spec.ts` (10, one `INV:TEN-11`), `delete-account-dialog.spec.ts` (7, still the example of CLAUDE §9), `account.routing.spec.ts` (2), `with-identity.spec.ts` (`forgetSession` replaces the two `deleteAccount` cases), `main-nav.spec.ts` (`/konto`).
- E2E: `identity/account.spec.ts` — an account of its own downloads the export, deletes itself with its organisation and list, lands signed out on the start page and cannot sign in again (`INV:TEN-11`, `INV:A11Y-01`, `INV:A11Y-08`); the seeded Owner is refused and stays (read only). `shell/reflow.spec.ts`: S61, its dialog and the refusal notice (A11Y-09).
- Server: three cases of `GET /api/v1/account` in `AccountDeletionTests`; `CookieSessionTests` asks it as its protected sample. Removed: `UsersControllerAuthorizationTests`, `UsersControllerTests`.
- `check-i18n.test.mjs`: the marker case.

### 2.4 Documents

UX copy v38 (v36 deleted), TESTING_v86 (V289 – V292; v84 deleted), ARCHITECTURE_v78 (C164, §7.2; v76 deleted), SPEC v2.46 (§8.13, R-AUTH-10 note), Handoff 1.29 (§5.3 „S61 as built“), CODEBASE §2.79, §3, CLAUDE.md §3 (state of the code, the `aria-disabled` line). `TECH_DEBT` gets its new version at the end of the sprint (process step 14): TD-26 is done, `LEGACY_EXCEPTIONS` is three files shorter.

## 3. Self-review

Code review per CLAUDE §6.7, sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `[attr.aria-disabled]="matches() ? null : 'true'"` on the Material button rendered no attribute: the button's host binding wrote `null` over it on the first pass, so the unavailable action was not announced as such | **fixed** — the button's own `[aria-disabled]` input; component and E2E tests check the attribute |
| R2 | An e-mail field drops surrounding spaces itself, so „genau so“ of `account.delete.confirm.unavailable` does not cover a trailing space | **won't fix** — the browser's behaviour for `type="email"`; case and every other character are compared exactly, and a space cannot be part of the address |
| R3 | After a 204 a failed navigation to the start page would leave the user on S61 with an empty store | **won't fix** — the navigation to `/` has no guard that refuses; S61's own guard would open S01 on the next action |
| R4 | The refusal notice appears while the focus returns from the dialog to „Konto löschen“; the focus is not moved to the notice | **won't fix** — it is `role="alert"` and announced, as the last-Owner notice on S21; moving the focus would take it from the button the user returns to |
| R5 | The page keeps the address read when it opened; an address confirmed in another tab still shows the notice until S61 is opened again | **won't fix** — the notice only informs here; export and deletion do not depend on it |
| R6 | `ProcureAccess_Daten.json` as the fallback file name is never expected (the server always names the file) and has no test | **won't fix** — same as the configurator's export |
| R7 | The seeded Owner is the subject of two read-only E2E tests that ask for the deletion; if the refusal ever broke, they would delete a shared account | **won't fix** — every E2E start rebuilds the database, and `AccountDeletionTests` guards the refusal on the server; an own organisation with a second member would need the whole invitation flow in two tests |
| R8 | Security: nothing of the user in browser storage (E2E checks it after the deletion); the address is compared in the client only, by decision P2 (a); `GET /api/v1/account` answers only the caller's own address | **no finding** |
| R9 | Accessibility: axe in both appearances on S61, the dialog and the notice; reflow at 320 px; H1 focus after the deletion; touch targets are Material buttons; no named `section` (Firefox rule) | **no finding** |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 653 passed, 0 failed**.
- Client: `npm run lint` clean; `npx ng test` **776 passed** (one focus test of the export dialog failed once in a run beside the E2E suite and passed three times alone); `npm run build:release` within its budgets.
- `e2e/`: `npm run typecheck` and `npm run lint` clean. Local run without the stack's images (the root disk had under 1 GB free): PostgreSQL and Mailpit as containers, the API with `dotnet run` in the environment `E2E`, the client with `ng serve`. Chromium and mobile Chrome, all specs: 259 of 270 passed in a first run that shared the machine with the unit tests and the release build; the 11 others (focus and visibility waits on S10 – S14, S20, S01, S03 and S61) passed when run again alone. Firefox, `shell/` and `identity/`: 83 passed. WebKit and mobile Safari run in CI only.
- `check-invariants` passed; `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` 0 errors; the scripts' self-tests pass; `reuse lint` compliant.

## 5. Open points

1. **Later steps** (unchanged from S5-28a): contributor profiles, entries, experience reports, projects in the deletion; the immediate end of other sessions (SEC-08, S5-29).
2. **Changing the e-mail address** has no requirement (decision 1a); a SPEC question for a later sprint if users need it. The sprint row and the task prompt of S5-28 still name it.
3. **Guarded deep links** (S5-09 open point 3): `authGuard` still opens S01 over the start page, so a signed-out `/konto` does not return to S61 after the sign-in — as for `/einstellungen`.
4. **`TECH_DEBT`:** TD-26 and the three `LEGACY_EXCEPTIONS` entries are done; the register records it with its next version at the end of the sprint.
5. **Proposals:** the legacy list of a user's own proposals is gone with the profile (decision 5a); the proposal pages stay until M5.

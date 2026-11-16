# S5-25a — User preferences, server part

> **Sprint:** 05 · **Task:** S5-25a (server part of S5-25, 3 points for S5-25 in all) · **Issues:** #234 (TD-25 part; stays open — the client part is S5-25b, TD-26 is S5-28)
> **Requirements / invariants:** SPEC R-SET-01, R-SET-02 (server part), §8.4, §8.12 (v2.42); PRV-07 preferences part (TESTING_v82 V276); A11Y-06 server part, still `planned` (V277); ARCHITECTURE C160. No invariant changes its status
> **Status:** done — pull request open (2026-11-16)
> **Scope:** `UserPreferences` stored per user in IdentityAccess with `GET` / `PUT /api/v1/account/preferences`, and the settings in the personal data export. No client change: the client applies the settings, the legacy settings store and page and the legacy `UICustomization` go in S5-25b.
> **Owner decisions (2026-11-16, before the task):** S5-25 **1a** — `UserPreferences` gets its own table in IdentityAccess (keyed by `UserId`, no foreign key, ADR-014); the legacy `UICustomization` with its column, controller, repo, DTO, mapping and `MapCases` entry is removed (legacy migration, no data migrated, O-S5-4) · **2a** — the legacy settings page `/settings`, its route, nav entries and `LEGACY_EXCEPTIONS` entries go with the store; S5-26 adds the nav entry of `/einstellungen` · **3** — contrast stored as `System` / `High`, `High` refused by the API until H2; `.contrast-more` keeps following `prefers-contrast: more`, without the snackbar (i) · **4** — the export shows the effective values (the defaults when never saved), lower case · **split (a)** — S5-25a server, S5-25b client and the legacy removal

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | ARCHITECTURE §5.2 said `UserPreferences` „extends existing `UICustomization`“ (the legacy JSON column of `identity."Users"` with colours, `DarkModeOn`, `HighContrastOn`, `Language`), SPRINT_05 and TASK_PROMPTS planned an IdentityAccess migration | Named before the task; decision 1a |
| 2 | The legacy client store (`with-settings.ts`) runs the legacy page `/settings`, reads `User.uiCustomization` from `me()` and writes `localStorage`; removing the store breaks the page | Decision 2a; with the client in S5-25b |
| 3 | R-SET-01 offers only „Wie im Betriebssystem eingestellt“ for contrast until H2, UX copy v36 already has `settings.contrast.high`; the legacy store sets the unreviewed `.contrast-more` from the system | Decision 3 |
| 4 | SPEC §8.4 left the shape of `account.preferences` open (`{ … }`, `null` until S5-25) | Decision 4; SPEC v2.42 §8.4, §8.12 |
| 5 | The `INV:A11Y-05` E2E check switches to English on the legacy page, the only language switch | S5-25b keeps only `lang="de"`; `document-language.spec.ts` keeps covering „follows the UI language“ (recorded in TASK_PROMPTS) |
| 6 | Server, client, two migrations, the legacy removal on both sides and E2E did not fit one session | Split (a); the server's legacy `UICustomization` stays until S5-25b, because the legacy client still reads it |

## 2. Changes

### 2.1 Server

- **`IdentityAccess/Domain/UserPreferences.cs`**: `UserPreferences` (`UserId`, `Language`, `Appearance`, `Motion`, `Contrast`) with `Defaults(userId)` (German, `System` × 3) and `Change(…)`, which replaces all four and throws for a language other than `de` or an undefined value; enums `AppearancePreference` (`System`, `Light`, `Dark`), `MotionPreference` (`System`, `Reduced`), `ContrastPreference` (`System`, `High`).
- **Persistence**: `UserPreferencesConfiguration` (table `identity."UserPreferences"`, key `UserId`, enums as text ≤ 16, language ≤ 8), a `DbSet` in `IdentityAccessDbContext` without query filter (not organisation-scoped; every read names the signed-in user's id); **migration `AddUserPreferences`** (internal).
- **`Accounts/UserPreferencesService.cs`**: reads without writing (the defaults for a user without a row); saves by update or insert, and retries once as an update when two first saves of one user collide on the key (the last save wins). `UserPreferenceSpelling` is the one place of the API spellings.
- **`Accounts/UserPreferencesEndpoints.cs`**: `GET` and `PUT /api/v1/account/preferences`, mapped by `MapIdentityAccessEndpoints` on the versioned group; `RequireAuthorization()` without the confirmed-address policy; the PUT checks each field and answers `validation` at `language`, `appearance`, `motion`, `contrast` (exact lower case; `high` refused, decision 3), else 200 with the stored values. Antiforgery through the module's middleware, `no-store` through Platform's default, no rate limit (SPEC §7.3 sets none).
- **`IdentityAccess.Contracts/Accounts/IUserPreferences.cs`**: `GetAsync(userId)` → `UserPreferencesValues(Language, Appearance, Motion, Contrast)`, registered as the same scoped service.
- **Host `Account/AccountExportEndpoints.cs`**: `account.preferences` is `AccountExportPreferences` from `IUserPreferences` (decision 4), never `null`.
- No new package, no new project (no `Dockerfile` line), no rate-limit policy, no UX copy key.

### 2.2 Client

None (S5-25b).

### 2.3 Tests

- Integration: `IdentityAccess/UserPreferencesTests.cs` (15 cases: defaults with `no-store` and nothing stored; PUT stores and the next GET answers it; a second PUT replaces every setting; one user's PUT leaves the others' settings alone; two concurrent first saves both 200; `validation` at the field for `en`, `Dark`, an unknown motion or contrast, `high`, a missing field, a number instead of a string, and at all four fields for `{}`, storing nothing; anonymous 401; PUT without the antiforgery token 400 `antiforgery`). No `INV:A11Y-06` tag yet: the invariant is `FE, E` and becomes `active` with S5-25b.
- `AccountExportTests`: the empty-account case expects the defaults instead of `null`; new `Export_UserWhoSavedSettings_ContainsTheUsersOwnSettings` (`INV:PRV-07`).

### 2.4 Documents

ARCHITECTURE_v74 (C160; §5.2 row, the API block; v72 deleted), TESTING_v82 (V276, V277; PRV-07 and A11Y-06 rows; v80 deleted), SPEC v2.42 (change rows v2.42-1, -2; §8.4 note; new §8.12), `CODEBASE_v1.md` §2.75, §3 (IdentityAccess line), §4 (a `curl` line), `CLAUDE.md` (state line, last task), SPRINT_05 (row S5-25 split), TASK_PROMPTS (S5-25 split and S5-25b's scope). No Figma mapping, UX copy or `LEGACY_EXCEPTIONS` change; TECH_DEBT changes at the end of the sprint (TD-25 is S5-25b's).

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | A number instead of a string (`"appearance": 5`) was not covered; it goes through Platform's `BadRequestExceptionHandler` | **fixed** — a case in the theory (400 `validation` at `appearance`); it also replaces the legacy `UICustomization` example of `ProblemDetailsTests` when S5-25b removes that endpoint |
| R2 | A soft-deleted legacy account keeps reading and writing its settings until the security-stamp validation ends its session; its row stays after the deletion | **follow-up S5-28** — the account deletion with its rules (TEN-11 account part) deletes the row; until then a row of a deleted account holds only four settings |
| R3 | The concurrency test does not force the unique-violation path; it passes whether or not the race happens | **won't fix** — the retry is a three-line catch; a deterministic test would need a hook into `SaveChanges` only for this |
| R4 | The PUT ignores unknown JSON members, while the saved-list bodies refuse them | **won't fix** — the four fields are all required and checked; an extra member changes nothing stored |
| R5 | No explicit request-size limit on the PUT | **won't fix** — Kestrel's default applies as on the other small bodies (sign-in, organisations); SPEC §7.2 sets none for settings |
| R6 | Security: the user is taken only from the cookie principal, never from the route or the body, so no user can read or change another's settings; antiforgery on the PUT (tested); no user id in logs or answers (the export test checks the raw body); no `IgnoreQueryFilters()`, no raw SQL | **no finding** |
| R7 | Performance: GET, PUT and the export each add one primary-key lookup; no N+1 | **no finding** |
| R8 | Accessibility: no screen in this part | **n/a** (S5-25b, S5-26) |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 660 passed, 0 failed** (unit, architecture, integration) before R1, `UserPreferencesTests` (15) again after it; `dotnet ef migrations has-pending-model-changes` for `IdentityAccessDbContext`: no changes.
- `check-invariants` passed; `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` 0 errors; `reuse lint` compliant.
- Client and E2E not run: no client file changed; CI is the authority.

## 5. Open points

1. **S5-25b** (decisions 2a, 3 (i)): the client's `withSettings` slice on the new API (loaded with the session, a background request; sign-out back to the system; nothing in browser storage), explicit appearance through `AppearanceService`, explicit reduced motion without reload (a class on `<html>`), `.contrast-more` following `prefers-contrast`; the legacy store, `with-local-storage.ts`, the page `/settings` with route, nav entries and `LEGACY_EXCEPTIONS` entries removed; the server's legacy `UICustomization` removed with a legacy migration (and its `ProblemDetailsTests` examples); A11Y-06 and the explicit-setting part of A11Y-08 `active`; the `INV:A11Y-05` E2E check reduced to `lang="de"`; J8's storage check made strict.
2. **S5-26**: S60 changes the settings (applied without reload, `settings.saved`), the nav entry of `/einstellungen`, the A11Y-08 part „applied without reload“.
3. **S5-28**: the account deletion removes the user's `UserPreferences` row (R2).

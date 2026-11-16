# S5-25b — User preferences, client part and the end of the legacy settings store

> **Sprint:** 05 · **Task:** S5-25b (client part of S5-25, 3 points for S5-25 in all) · **Issues:** #234 (TD-25 part done; the issue stays open for TD-26, S5-28)
> **Requirements / invariants:** SPEC R-SET-01 (persistence), R-SET-02, §7.5, §8.12 (v2.43); **A11Y-06** `active` (TESTING_v83 V278); **A11Y-08** explicit-setting part `active` (V279); SEC-01 E2E storage inspection strict (V280); A11Y-05 E2E check (V282); ARCHITECTURE C161
> **Status:** done — pull request open (2026-11-17)
> **Scope:** the client applies a signed-in user's settings and follows the system otherwise, without browser storage; the legacy settings store, its page `/settings` and the legacy `UICustomization` go; the fixes of the two CI findings of #362 (axe helper, J1). No S60: it comes with S5-26.
> **Owner decisions (2026-11-16):** S5-25 **1a** — the legacy `UICustomization` removed (legacy migration, no data migrated, O-S5-4) · **2a** — the legacy settings page `/settings` with route, menu entries and `LEGACY_EXCEPTIONS` entries goes now; S5-26 adds the menu entry of `/einstellungen` · **3** — contrast `System` / `High`, `High` refused until H2 (S5-25a); **(i)** `.contrast-more` follows `prefers-contrast: more`, without the snackbar · **4** — the export shows the effective values (S5-25a) · **split (a)** · the axe-helper fix and the J1 download fix go into this task (both found in the CI of #362)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `with-settings.ts` (legacy) wrote the appearance to `localStorage` for signed-out users, opened an English snackbar on a `prefers-contrast` change and threw when storage was blocked (TD-25); `app.store.ts` called it in `init`, `startSession`, `endSession`; `User.uiCustomization` came from `GET /api/me` | Replaced by a new `withSettings` that follows the session |
| 2 | `AppearanceService` handled only the appearance; Material read `prefers-reduced-motion` once at start (`MATERIAL_ANIMATIONS`), so an explicit „reduziert“ could not apply without reload | Motion and contrast added; `MATERIAL_ANIMATIONS` through a getter; a CSS rule for transitions (Handoff §1.7) |
| 3 | Material asks `_getAnimationsState()` when it creates a component | A getter is enough for components created after a change; components already shown keep their state, `motion-reduced` shortens their transitions |
| 4 | The legacy page used `LANGUAGES` (`core/models/languages.map.ts`) and four icons nothing else uses | Deleted; `check-icons` asked for the icon list and the font subset |
| 5 | `header-states.spec.ts` and `reflow.spec.ts` expected „Einstellungen“ in the menus; `route-change.spec.ts` switched the language on `/settings` | Adjusted (decision 2a) |
| 6 | J1's download timeout (CI of #362, chromium; `main`, mobile-chrome): the screenshot of the failure shows S13 idle with the focus ring on the XLSX radio. S13's first focus (`autoFocus: 'input[type="radio"]:checked'`) comes after the open animation; `exportThroughS13` tabbed before it, reached „Datei herunterladen“, then the focus jumped back to the radio and Enter did nothing | Wait for the first focus before tabbing |
| 7 | S15's axe failure (CI of #362, Firefox, mobile Safari): `color-contrast` 3.04 on the hint of a dialog still fading in; the axe helper waited for transitions only after switching the appearance and never for CSS animations | The helper waits before every run |

## 2. Changes

### 2.1 Client

- **`features/settings/`**: `models/user-preferences.ts`; `services/api/preferences-api.service.ts` (`get()`, `BACKGROUND_REQUEST`); `state/with-settings.ts` rewritten — `signalStoreFeature({ state: type<{ session }>() }, …)`, `preferences` in memory, `loadPreferences(userId)` (drops an answer for an earlier user; a failure follows the system), `onInit`: German through `LanguageService.init`, an effect on the session's user id (a `computed`, so a renewed session of the same user reads nothing). No save method (S5-26).
- **`core/a11y/appearance.ts`**: `AppearanceService` follows `prefers-color-scheme`, `prefers-reduced-motion: reduce` and `prefers-contrast: more` live, with `setExplicit`, `setExplicitReducedMotion`, `setExplicitHighContrast` and the classes `theme-*`, `motion-reduced`, `contrast-more`; `provideAppearance()` answers `MATERIAL_ANIMATIONS` through a getter.
- **`styles/theme.scss`**: `html.motion-reduced` shortens transitions to 0.01 ms (so `transitionend` still fires) and `scroll-behavior`; animations stay, so progress indicators keep running; `.contrast-more` comment (decision 3 (i)).
- **`core/state/app.store.ts`**: `startSession`, `endSession`, `init` without settings calls; `void` on `init()` — its `LEGACY_EXCEPTIONS` entry went. `appState` without `settings`; `User` without `uiCustomization`.
- **Removed:** `features/settings/settings.{ts,html,scss,spec.ts}`, `models/uiCustomization.model.ts`, `services/settings-api.service.ts`, `shared/state/with-local-storage.ts`, `core/models/languages.map.ts`, the route `settings`, both menu entries in `main-nav.html`, `SETTINGS.*` in `public/i18n/{de,en,es,fr}.json`, the icons `contrast`, `contrast_rtl_off`, `dark_mode`, `light_mode` (subset regenerated: 31 icons), the `LEGACY_EXCEPTIONS` entries of `settings-api.service.ts`, `settings.ts`, `with-settings.ts` and `app.store.ts`.

### 2.2 Server

- Removed: `UICustomization`, `UICustomizationDto`, `Language`, `Languages`, `AccountMappings.ToDto(UICustomization)`, `ToLanguageCode`, `ToLanguage`, `UserDto.UICustomization`, `User.UICustomization`, the owned JSON configuration in `UserConfiguration`, `SampleData`'s line, `UICustomizationRepo` and `IUICustomizationRepo` with their registration, `UICustomizationController`. **Legacy migration `DropUserUICustomization`** (`ApplicationDBContext`) drops `identity."Users"."UICustomization"`. `GET /api/me` answers id and address.

### 2.3 Tests

- Vitest: `with-settings.spec.ts` (3 × `INV:A11Y-06`; session renewal, failed read, late answer, another user, anonymous, high contrast; TD-25: no `Storage` call, blocked storage, no snackbar); `appearance.spec.ts` (+5: motion and contrast); `main-nav.spec.ts` without „Einstellungen“; `with-identity.spec.ts` fixture without `uiCustomization`. 746 tests.
- .NET: `ProblemDetailsTests` — the wrong-type example of API-01 posts a number as `product.name` to the legacy proposal endpoint (MVC), the legacy-failure case of `UICustomization` is gone; `MappingCompletenessTests` without the language theory; `MapCases` without `UICustomization`. 1 652 tests.
- E2E: new `identity/preferences.spec.ts` (`INV:A11Y-06`, `INV:A11Y-08`, `INV:A11Y-01`: explicit dark and reduced motion over a light system, axe in the explicit appearance, a second browser context, the sign-out back to the system — through the compact menu on narrow screens; storage empty signed in, signed out and anonymous; storage blocked); J8 requires empty storage; A11Y-05 keeps `lang="de"`; `header-states`, `reflow` without „Einstellungen“; `support/axe.ts` `settled()` before every run and `explicitAppearance`; J1 waits for S13's first focus.

### 2.4 Documents

ARCHITECTURE_v75 (C161, §5.2; v73 deleted), TESTING_v83 (V278 – V282; A11Y-05, A11Y-06, A11Y-08, SEC-01 rows; v81 deleted), SPEC v2.43 (v2.43-1, -2; §8.12), `CODEBASE_v1.md` §2.75 (note), §2.76, §3, `CLAUDE.md` (state line, last task, the memory rule names the settings). No UX copy, Figma mapping or Handoff change; TECH_DEBT at the end of the sprint (TD-25 done).

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | `languages.map.ts` became dead with the legacy page | **fixed** — deleted |
| R2 | A signed-in user with an explicit appearance sees the system appearance for a moment after a page load, until the probe and `GET /api/v1/account/preferences` answered | **won't fix** — R-SET-02 forbids browser storage, which is the only way to know earlier; the sign-in itself switches without reload |
| R3 | `en.json`, `es.json`, `fr.json` and `EnLanguage.en/es/fr` are unreachable now (German only, R-SET-01; the language switch is gone) | **follow-up issue** (proposed) — remove the legacy languages with `LanguageService`'s lint debt; not this row's scope |
| R4 | `.contrast-more` now applies the legacy high-contrast colours for every visitor whose system asks for more contrast, as the legacy store did after its first start | **kept** — decision 3 (i); the colours wait for H2 |
| R5 | `pageTitle.settings` and `shell.nav.settings` stay in `de.json` without a user until S5-26 | **won't fix** — UX copy keys of S60, which S5-26 uses |
| R6 | Components shown before an explicit „reduziert“ keep Material's animation state until they are created again; only their CSS transitions are shortened | **won't fix** — Handoff §1.7 allows either mechanism; every dialog, menu and page created after the change is without animation |
| R7 | Security and privacy: the settings are read only for the session's own user; nothing in storage (tested); no user id logged; the server's legacy endpoint that wrote settings is gone | **no finding** |
| R8 | Accessibility: no new screen; the explicit dark appearance passes axe (E2E); motion and contrast follow the system live | **no finding** |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 652 passed, 0 failed**.
- Client: `ng test` **746 passed**; `npm run lint` clean (four exceptions fewer); `npm run build:release` complete (its CSS warning about the selector `50%` exists on `main` too).
- E2E (local stack): Chromium **130 passed** (after adjusting `header-states` and `reflow`), Firefox **129 passed, 1 skipped**, mobile-chrome: the touched specs (preferences, journeys, shell, S15/S16) passed after the compact-menu sign-out; WebKit and mobile Safari in CI only. E2E `typecheck` and `lint` clean.
- `check-invariants` passed (A11Y-06 active), `check-figma-links`, `check-i18n` 0 errors, `check-icons` 0 errors (after the subset), `reuse lint` compliant.

## 5. Open points

1. **S5-26**: S60 at `/einstellungen` with the menu entry, a save method in `withSettings` (applied at once, `settings.saved`), the A11Y-08 part „applied without reload“ through S60.
2. **S5-28**: the account deletion removes the user's `UserPreferences` row (S5-25a R2); TD-26 closes #234.
3. **Proposed issue (R3):** remove the legacy languages (`en/es/fr.json`, `EnLanguage` values) and fix `language.service.ts`'s lint exception.

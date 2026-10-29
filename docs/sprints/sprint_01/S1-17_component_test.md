# S1-17 — First component test and Vitest coverage

> **Sprint:** 01, task S1-17 (milestone M0.4, TESTING §3.2, §5.3, §8.4). **Status:** done by Claude Code (2026-10-23).
> **Scope:** one test of a real application component that queries by role and accessible name and serves as the example of the convention (CLAUDE §9); coverage output with `@vitest/coverage-v8`. Follow-ups from S1-16: P3 (how tests trigger user actions) and P4 (coverage package). Not in scope: `vitest-axe` (first K component), ESLint template rules and the CI coverage merge (S1-20), changes to the component under test.
> **Owner decisions 2026-10-23:** **1A** user actions without a new dependency (no `@testing-library/user-event`). **2** plain `expect` matchers (no `@testing-library/jest-dom`). **3** tests use the real German texts from `de.json`, not translation keys. **4** the component is `DeleteAccountDialog`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `DeleteAccountDialog` has no HTTP or store dependency, a heading, a labelled text field and two buttons; "Löschen" is enabled only when the typed e-mail matches the account e-mail (`computed` over an `ngModel`-bound `model`) | Covers role/name queries, typing, a signal-driven view update and the dialog result in one small component (decision 4) |
| F2 | The dialog uses native `disabled`; CLAUDE §9 asks for `aria-disabled` with a visible reason | The test describes today's behaviour and says so in a comment; the component is not changed (screens are rebuilt in M1/M2) |
| F3 | The app loads `public/i18n/de.json` over HTTP; tests had no translation setup | Test helper with a static loader (decision 3) |
| F4 | `@angular/build:unit-test` 22.2.3 has `coverage`, `coverageInclude`, `coverageExclude`, `coverageReporters`, `coverageThresholds`; only the provider package was missing. `@vitest/coverage-v8` 5.0.5 (MIT) needs exactly `vitest` 5.0.5, which is installed | Configuration in `angular.json`, no Vitest config file |
| F5 | **jsdom pitfall:** `fireEvent.click` (from `@testing-library/dom`) on a **disabled** button still runs its click handler — the probe closed the dialog with `true`. `HTMLElement.click()` does nothing on a disabled button, as in a browser | Convention: clicks with `element.click()`, other events with `fireEvent` from `@testing-library/dom` (see §3) |
| F6 | `fireEvent` from `@testing-library/angular` runs change detection itself: with it, the test passes even without `await fixture.whenStable()` (check N2) | Confirms S1-16 P3: that import is not used |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/package.json`, `package-lock.json` | Dev dependency `@vitest/coverage-v8` ^5.0.5; script `test:coverage` (`ng test --coverage`) | S1-16 P4; TESTING §3.2 |
| `CLIENT/…/angular.json` → `test` | `coverageInclude: ["src/app/**/*.ts"]` (untested files count as 0 % instead of being missing), `coverageExclude` for specs and `src/app/testing/**`, reporters `text-summary` + `lcov` (lcov for the merge in S1-20). No thresholds | TESTING §8.4: reported, not gated |
| `CLIENT/…/src/app/testing/provide-test-translations.ts` (new) | `provideTestTranslations()`: `provideTranslateService` with language `de` and a loader that returns the imported `de.json` | Decision 3; a missing key renders as the key and the role query fails (check N4) |
| `CLIENT/…/tsconfig.spec.json` | `resolveJsonModule: true` | JSON import in the helper |
| `CLIENT/…/tsconfig.app.json` | `src/app/testing/**` excluded | Test-only code stays out of the app compilation (release bundle unchanged) |
| `CLIENT/…/delete-account-dialog/delete-account-dialog.spec.ts` (new) | Three tests: title, account e-mail and labelled field found by role/name; "Löschen" unavailable (and a click on it does nothing) until the account e-mail is typed; "Löschen" closes with `true`, "Abbrechen" with `false`. The header comment states the convention | "Done when" |
| `CLAUDE.md` §3, §5, §9 | Current state; `npm run test:coverage`; the action convention and the pointer to the example | CLAUDE §11 |
| `CLIENT/…/README.md` | Coverage command and pointer to the example test | |

Lockfile: 12 packages added, all MIT (`@vitest/coverage-v8`, `@vitest/istanbul-lib-coverage`, `@vitest/istanbul-lib-report`, `@bcoe/v8-coverage`, `ast-v8-to-istanbul`, `magicast`, `obug`, `tinyrainbow`, `@babel/parser`, `@babel/types`, `@babel/helper-string-parser`, `@babel/helper-validator-identifier`); no existing package changed version.

## 3. Convention (as documented in the example test and CLAUDE §9)

1. Find elements by role and accessible name (`getByRole`); `getByText` only for plain text. If something cannot be found that way, fix the accessibility, not the query.
2. Names are the German texts users see (`provideTestTranslations()`).
3. Trigger actions without running change detection, then `await fixture.whenStable()`: clicks with `element.click()`, other events (`input`, `keydown`, …) with `fireEvent` from `@testing-library/dom`. Never `fireEvent` from `@testing-library/angular`.
4. Replace only what is outside the component (here `MatDialogRef` and `MAT_DIALOG_DATA`).

## 4. Checks

Node 24.22.1 / npm 11.19.1 from `.nvmrc`.

| # | Check | Result |
|---|---|---|
| 1 | `npx ng test` and `CI=true npx ng test` | ✅ exit 0; 2 files, 5 tests (3 new + 2 toolchain) |
| 2 | `npm run test:coverage` | ✅ exit 0; summary printed; `coverage/procure-access/lcov.info` + `lcov-report/` (ignored by git). 100 of 100 non-test source files listed with `.ts` paths; helpers and specs excluded; `delete-account-dialog.ts` 6/6 lines, 10/10 branches. Overall: statements 0.91 %, branches 3.63 %, functions 0.57 %, lines 0.52 % |
| N1 | `await fixture.whenStable()` removed from the typing helper | ✅ 2 tests fail (button stays disabled; the click then does nothing); restored |
| N2 | As N1, but `fireEvent` imported from `@testing-library/angular` (first version of the test, before F5) | Passes — F6; restored |
| N3 | `deletionDisabled` always `false` in the component | ✅ fails; restored |
| N4 | Test loader returns `{}` instead of `de.json` | ✅ 3 tests fail (names are keys); restored |
| N5 | `[disabled]` binding removed from the template | ✅ fails; restored |
| 3 | `npm run build:release` / `npm run build` | ✅ both; release 1.81 MB initial (373.37 kB transferred), unchanged; only the known EasyMDE selector warning |
| 4 | `npm audit` | 2 (1 high, 1 low), dev only, unchanged from S1-16 P2; `--omit=dev`: 0 |
| 5 | `dotnet test --solution ProcureAccess.sln` | ✅ 22/22. A first run failed all 19 integration tests because PostgreSQL could not start in Docker (`initdb: … No space left on device`, root file system 100 % full, S1-13 P1); after the owner freed space (4.5 GB free) all passed |
| 6 | `reuse lint` (6.2.0, temporary venv) | ✅ compliant |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | **Disk space:** the root file system ran full during this task and blocked the integration tests; with 4.5 GB free they pass again. Keep an eye on it (S1-13 P1, S1-15 P6) | ✅ Freed by the owner, 2026-10-23 |
| P2 | `@testing-library/user-event` would make interactions closer to a browser (refuses disabled elements, real keyboard sequences, focus). Reconsider with the first keyboard-interaction tests (K-components, A11Y-04); it is a new dependency → owner decision | M1/M2 |
| P3 | The next TESTING version should take over the convention of §3 (TESTING §5.3 currently only names the role queries) and S1-16 P1 (zoneless via `providersFile`) | Next TESTING version |
| P4 | S1-20: stage 8 runs `npm run test:coverage` (or `npx ng test --coverage`) and merges `coverage/procure-access/lcov.info` with the backend coverage | S1-20 |
| P5 | Excluding generated code and DTOs from measurement (TESTING §8.4) is not configured: model and DTO files are part of `src/app/**/*.ts` and show 0 %. Narrow `coverageExclude` once the target structure (`features/<name>/models`) exists | M1 |

## 6. Review follow-up (2026-10-28)

Code review of PR #50 (9 findings), sorted per CLAUDE §6.8. None affects the product: #50 changed no production code. Fixed in `sprint01/S1-17-review-fixes`:

| # | Finding | Fix |
|---|---|---|
| R1 | `npm run test:coverage` (`ng test --coverage`) stays in watch mode in a terminal and never exits, although it is documented as the report command; only CI (no terminal) ran it once | Script is now `ng test --coverage --watch=false`; CLAUDE §5 and the client README say it always runs once. CI stage 8 is unchanged (it ran once before as well) |

Follow-up issues:
- R2 — an empty account e-mail makes the input match at once, so "Löschen" starts enabled (legacy dialog code, not from #50): #95.
- R3 — two findings: the example spec doesn't test "Abbrechen" from the start, nor near-miss e-mails (case, trailing space, prefix): #96.
- R4 — `coverageInclude` counts models, DTOs and environment files as 0 % (= P5, now tracked): #97.
- R5 — `vitest` and `@vitest/coverage-v8` use separate caret ranges but must stay on the same version: #98.

Won't fix:
- R6 — `deleteButton().click()` on the disabled button can never close the dialog, so that assertion can't fail; the `disabled === true` assertion before it carries the check (N5). Harmless; removing it is noted in #96. The switch to `aria-disabled` (CLAUDE §9) is part of #95.
- R7 — `provideTestTranslations()` sets no fallback language, unlike `LanguageService`. With only `de.json` loaded the output is identical; revisit when a test needs the fallback.
- R8 — the spec contains German UI strings, said to conflict with CLAUDE §6.5. Intended: §9 asks tests to query by the real German texts; §6.5 is about templates and app code.

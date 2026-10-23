# S1-16 — Karma/Jasmine replaced by Vitest and Angular Testing Library

> **Sprint:** 01, task S1-16 (milestone M0.4, TESTING §3.2, §9 step 0, §12.1). **Status:** done by Claude Code (2026-10-23).
> **Scope:** the `test` target runs Vitest through the Angular CLI builder `@angular/build:unit-test` (jsdom); Angular Testing Library installed; Karma, Jasmine and the 22 generated specs removed; `npx ng test` runs once without a terminal (CI). Not in scope: the first real component test and coverage output (S1-17), `vitest-axe` and ESLint template rules (S1-20 / later), the CI workflow (S1-20).
> **Owner decisions 2026-10-23:** **1A** one toolchain smoke test with its own host component keeps the run green (the builder refuses to run without spec files); no application component is tested before S1-17. **2** zoneless change detection for every `TestBed` comes from one `providersFile` instead of being repeated in every spec. **3** current package versions.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | All 22 `*.spec.ts` were CLI-generated "should create" stubs; one did not compile (`save-search-dialog.spec.ts` imports a missing file), the others failed with `NG0201` (S1-08 – S1-12) | Deleted, nothing worth keeping |
| F2 | `@angular/build` 22.2.3 contains `@angular/build:unit-test` (default runner Vitest, peer `vitest ^4.0.8 \|\| ^5.0.0`, jsdom when no browser is configured) | No separate Vitest config file needed |
| F3 | The builder stops with "No tests found matching …" when no spec file exists | Decision 1A |
| F4 | The builder's `providersFile` is added to the TestBed initialisation, so it applies to every `TestBed` and to ATL's `render()` | Decision 2 |
| F5 | The builder sets Vitest `globals: true` (`describe`, `it`, `expect` without import) — confirmed on the installed version as TESTING §12.1 asks | `tsconfig.spec.json` uses `vitest/globals` types |
| F6 | `zone.js` is not installed, so the builder loads no Zone (strategy `none`). Angular 22's `TestBed` is zoneless by default in that case: the smoke test also passes with an empty providers list | The provider makes the TESTING §3.2 rule explicit and keeps it if Zone.js ever returns as a dependency |
| F7 | Watch mode defaults to on in a terminal and off without one; also off with `CI=true` | No CI-only option or script needed; the Karma relic `test:headless` is removed |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/package.json`, `package-lock.json` | Removed `karma`, `karma-chrome-launcher`, `karma-coverage`, `karma-jasmine`, `karma-jasmine-html-reporter`, `jasmine-core`, `@types/jasmine`, `istanbul-lib-instrument` (added only for Karma, S1-09). Added (dev) `vitest` ^5.0.5, `jsdom` ^30.1.3, `@testing-library/angular` ^19.6.0, `@testing-library/dom` ^10.4.2 (all MIT). Script `test:headless` removed | "Done when"; TESTING §3.2 |
| `CLIENT/…/angular.json` | `test` → `@angular/build:unit-test`, `runner: vitest`, `tsConfig`, `providersFile`; Karma-only assets and styles dropped (the builder takes the build target's options) | F2, F4 |
| `CLIENT/…/tsconfig.spec.json` | `types: ["vitest/globals"]` | F5 |
| `CLIENT/…/tsconfig.app.json` | `src/test-providers.ts` excluded | Test-only file stays out of the app compilation |
| `CLIENT/…/src/test-providers.ts` (new) | Default export `[provideZonelessChangeDetection()]` | Decision 2, TESTING §3.2 |
| `CLIENT/…/src/app/testing/toolchain.spec.ts` (new) | Two tests with an OnPush host component: no Zone loaded; a plain DOM click on a signal-driven button updates the view after `fixture.whenStable()` (queried by role and name) | Decision 1A |
| 22 × `CLIENT/…/src/**/*.spec.ts` | Deleted | F1 |
| `CLIENT/…/README.md`, `.vscode/launch.json` | Karma text replaced; the Karma debug launch entry (port 9876) removed | No Karma traces left |
| `CLAUDE.md` §3, §5 | Current state (S1-16); `npx ng test` in the command list | CLAUDE §11 |

Lockfile: 213 packages removed, 62 added; the only version change of an existing package is `punycode` 1.4.1 → 2.3.1 (1.x came from Karma's `ent`, 2.x is needed by jsdom's `whatwg-url`). Direct dependencies unchanged (the S1-08 B3 effect did not occur). The two `karma` lines left in the lockfile are `@angular/build`'s optional peer declarations; nothing is installed.

## 3. Checks

Node 24.22.1 / npm 11.19.1 from `.nvmrc`.

| # | Check | Result |
|---|---|---|
| 1 | `npx ng test` (no terminal) | ✅ exit 0; 1 file, 2 tests; single run, ~3 s |
| 2 | `CI=true npx ng test` | ✅ exit 0, single run |
| 3 | Negative: a temporary `expect(1).toBe(2)` | ✅ exit 1; removed |
| 4 | Negative: `test-providers.ts` throws on load | ✅ exit 1 with that error, so the file is loaded; restored |
| 5 | Negative: `await fixture.whenStable()` removed from the smoke test | ✅ fails ("Unable to find … 'Clicked 1'"), so the test does not pass by accident; restored |
| 6 | `npm ls karma jasmine-core @types/jasmine istanbul-lib-instrument`; `git grep -i "karma\|jasmine"` outside `docs/` and the lockfile | ✅ empty; only the old CLAUDE.md sentence, now replaced |
| 7 | `npm run build:release` / `npm run build` | ✅ both; release 1.81 MB initial (373.37 kB transferred), unchanged from S1-11; only the known EasyMDE selector warning |
| 8 | `docker compose build --no-cache client` | ✅; the Karma deprecation warnings (`glob`/`inflight`/`rimraf`, S1-11) are gone, only `@angular/animations` (S1-09 N4) remains |
| 9 | `npm audit` | Before 7 high, after 2 (1 high, 1 low), all dev tooling; `--omit=dev`: 0. See P2 |
| 10 | `dotnet test --solution ProcureAccess.sln` | ✅ 22/22 (backend not touched) |
| 11 | `reuse lint` (6.2.0, temporary venv) | ✅ compliant, 472/472 |

## 4. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | TESTING §3.2 says "every `TestBed` configuration includes `provideZonelessChangeDetection()`". It is now done once through `providersFile` (decision 2); the next TESTING version should say so, so that specs do not repeat it | Next TESTING version |
| P2 | Remaining high audit finding: `vite` 8.3.0 (dev server only), pulled in by `@angular/build` 22.2.3 and shared with Vitest; `npm audit fix --force` would downgrade `@angular/build` to 22.1.9. Wait for a patch release of `@angular/build`; Dependabot (S1-20) will raise it | S1-20 / next Angular patch |
| P3 | ATL's `fireEvent` helpers call `detectChanges()` themselves and can hide a component that only updates through Zone-triggered change detection (TESTING §3.2). The smoke test uses a plain DOM event plus `whenStable()` for that reason. S1-17 should fix the convention: plain events + `whenStable()`, or `@testing-library/user-event` (a new dependency → owner decision) | S1-17 |
| P4 | `@vitest/coverage-v8` is not installed yet | S1-17 |
| P5 | `vitest-axe` (TESTING §3.2) is not installed; it is needed with the first component that gets a unit-level axe check (always for K1–K6) | First K-component, M1/M2 |
| P6 | The smoke test's host component uses an English button text; it is test-only markup, not UI text, so CLAUDE §9 (UI strings by key) does not apply | – |

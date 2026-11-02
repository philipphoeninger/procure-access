# S2-14 — TypeScript lint rules

> **Sprint:** 02, P0 task S2-14 (TECH_DEBT_v1 TD-07, 1 point; debt budget). **Issues:** #119; new tracking issue #158. **Requirements / invariants:** none (CI stage 1); no invariant changes status. **Status:** done by Claude Code (2026-11-02), pull request open.
> **Scope:** the TypeScript rules of ESLint for the client's `.ts` files, the trivial findings fixed, the rest of the legacy findings turned off per file with an issue link, no rule turned off for new code. Not in scope: `e2e/` and `scripts/` (not linted by the client config), the `inject()` migration of legacy code, any change to legacy files beyond the mechanical fixes.
> **Owner decisions 2026-11-02 (before the task):** **Rule set:** `tseslint.configs.recommendedTypeChecked` (type-aware, `projectService`) — on purpose beyond the row's "recommended", because #119 names unhandled promises, which only the type-aware set reports. **Angular:** `angular.configs.tsRecommended` is on as well. **Legacy code:** fix only mechanical findings (prefer-const, unused variables and imports, lifecycle interfaces, empty lifecycle methods); turn off the remaining rules per legacy file. **Exceptions:** one central list in `eslint.config.js` (file list + rules), linked to one new tracking issue that Claude opens; no new path in it; new code (`core/errors`, `shared/`, `testing/` …) is fixed, never turned off.
> **Owner decision 2026-11-02 (during the task, F4):** the legacy files that step 5 retires or reworks stay in the list; the list covers legacy code that **any** migration step replaces, reworks or retires, grouped by that step, and #158 belongs to milestone M5.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `eslint.config.js` (S1-20) gave `.ts` files only `tseslint.configs.base` (parser, no rules) to reach their inline templates. `typescript-eslint` 8.72 and `angular-eslint` 22.6 were already installed | No new dependency |
| F2 | Dry run of the decided rules on `main` (S2-13 merged): **262 findings in 74 of 122 files** — 57 `no-unused-vars`, 54 `prefer-inject`, 45 `prefer-const`, 26 `no-floating-promises`, 14 `no-unsafe-assignment`, 12 `use-lifecycle-interface`, 12 `no-explicit-any`, 8 `no-empty-lifecycle-method`, 30 others. Plain `recommended` would have reported 194 | Rule set as decided; the type-aware run takes about 4 s |
| F3 | Almost all findings are in legacy screens. Code written since sprint 02 had 2 (`translate-plural.pipe.ts`, `de-json.spec.ts`); older code that no step replaces had 13 (`main.ts`, `shared/` markdown editor and local-storage feature, `core/services/snackbar.service.ts`, `core/interceptors/error.interceptor.ts`) | Those files are fixed completely (decision "new code is fixed"; a file no step replaces is not legacy in the sense of the decision) |
| F4 | The decision and SPRINT_02 R5 say "legacy code that M1–M3 replace". But `features/products`, `features/proposal`, `criteria/proposal` and `favorites` are retired or reworked only in step 5 (ROADMAP M5.6, ARCHITECTURE §8.1), and the store root `core/state/app.store.ts` loads them | Stopped and asked; owner: keep them in the list, grouped by step (header) |
| F5 | Adding `implements OnInit` to the three `async ngOnInit()` (criterion proposal, product proposal, filters selection) makes the type-aware rule report `no-misused-promises` there: Angular does not wait for the returned promise. True, but not mechanical to fix | Turned off for those three legacy files; the finding is real and goes with their step |
| F6 | The shell's default Node (22.20) is below the Angular CLI's minimum; `npm run lint` and `ng` need Node 24 from `.nvmrc` | Local runs on Node 24.22.1; CI uses `.nvmrc` already |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/eslint.config.js` | `.ts` files: `recommendedTypeChecked` + `tsRecommended`, `parserOptions.projectService` with `tsconfigRootDir`; inline templates still go through `processInlineTemplates`. `LEGACY_EXCEPTIONS`: 33 exact file paths (no globs), each with the rules turned off, grouped by step — M1 shell (1 file), M2 catalog/configurator (5), M3 identity, auth guard and settings (16), M5 proposal workflow, products, favorites and the store root (11). The config throws when an entry names a file that no longer exists, so a step that deletes a screen has to delete its entry | Decisions; #158 |
| 55 legacy `.ts` files and 1 template under `features/`, `layout/header/`, `core/models/`, `core/state/` | Mechanical only (122 findings): `let` → `const` (by `eslint --fix` limited to that rule), unused imports removed (mostly `TypedJSON` from `typedjson`; TypeScript drops unused imports anyway, so nothing changes at runtime), the unused parameters of `onSubmit(form, event)` in the criterion proposal (template call adjusted) and of an empty `withComputed`, 8 empty `ngOnInit`/`ngAfterViewInit` removed, `implements OnInit` / `AfterViewInit` added where the method has a body, one unnecessary `!` | Decision "mechanical only" |
| `src/main.ts` | The dev-only `import()` of the API health watcher gets a `.catch` like `bootstrapApplication` | `no-floating-promises` |
| `shared/pipes/translate-plural.pipe.ts` | `instant(...) as string` (ngx-translate types it `any`) | `no-unsafe-return` |
| `shared/components/markdown-editor/markdown-editor.ts` | Input `value` without alias; the `linkedSignal` copy is gone (nothing wrote to it, so it always equalled the input); `ElementRef<HTMLTextAreaElement>` | `no-input-rename`, `no-unsafe-assignment`; `[(value)]` binding unchanged |
| `shared/state/with-local-storage.ts` | `any` → `unknown` for the value read from the store | `no-explicit-any` |
| `core/services/snackbar.service.ts` | `inject(MatSnackBar)` instead of constructor injection | `prefer-inject` |
| `core/interceptors/error.interceptor.ts` | The legacy error body (`code`, `errors[0].code`) gets a type `LegacyErrorBody`; the translated message is typed `string` | `no-unsafe-*` (5) |
| `src/app/testing/de-json.spec.ts` | Unnecessary `as Texts` removed | `no-unnecessary-type-assertion` |
| `docs/TESTING_v21.md` (new) | V82: stage 1 runs the TypeScript rules; tools table §3; stage-1 note in §8.2. No invariant changes status | Stage 1 is described there |
| `CLAUDE.md` | Header; §3 (current state, lint as built); §5 (`npm run lint` comment); §9 rule "Lint exceptions only shrink" | DoD |
| Issue #158 (new, label `tech-debt`, milestone M5) | Tracks the list: delete an entry when its step replaces the file, close the issue when the list is empty | Decision "one new tracking issue" |

130 findings stay turned off in the 33 listed files: 53 `prefer-inject`, 25 `no-floating-promises`, 12 `no-unsafe-assignment`, 11 `no-explicit-any`, 6 `no-unsafe-argument`, 5 `no-non-null-asserted-optional-chain`, 4 `no-unsafe-enum-comparison`, 3 `no-misused-promises` (F5), 3 `no-unsafe-member-access`, 3 `unbound-method`, 2 `no-unsafe-return`, 2 `prefer-promise-reject-errors`, 1 `only-throw-error`.

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` over the whole diff: no security finding (lint configuration and type annotations; the `error.interceptor.ts` change only types the body it already read). No query, no new dependency, no UI text.

| # | Finding | Sorting |
|---|---|---|
| R1 | My import-removal script reformatted one untouched `@angular/core` import (`criteria-list.ts`) | Fixed: restored, the file now only loses its empty `ngOnInit` |
| R2 | The markdown editor is used only by the legacy criterion proposal, which no test or journey opens; the two-way binding `[(value)]` → `value` + `valueChange` is the same public API as before, and the release build's strict template check compiles it | Won't fix: no test for a screen step 5 retires; noted as P3 |
| R3 | `instant(...) as string` trusts ngx-translate to return a string for a leaf key; for a key with children it returns an object, as before | Won't fix: same behaviour as before, the pipe only feeds templates |
| R4 | `SnackbarService` has hard-coded English button texts ('Close', 'Dismiss') | Out of scope (UI text, not lint); open point P2 |
| R5 | `no-misused-promises` on three `async ngOnInit()` comes from the added lifecycle interfaces (F5) | Won't fix here: legacy, listed with its step |

Probes (each shows that a check can fail): a temporary `shared/probe.ts` with constructor injection, an `any` parameter, an unused `let` and an unawaited `router.navigate()` failed with `prefer-inject`, `no-explicit-any`, `prefer-const`, `no-unused-vars` and `no-floating-promises`; a probe component with an inline `<img>` without `alt` still failed `alt-text` (inline templates are linted as before); a copy of the config with an entry for a missing file stopped with "legacy exception for a missing file". Probes deleted.

## 4. Checks

| Check | Result |
|---|---|
| `npm run lint` (Node 24.22.1) | All files pass; about 4 s |
| `npx ng test` (Vitest, all specs) | 46 tests in 11 files passed |
| `npm run build:release` | Passes; the known warning about one unparsable selector in the critical CSS (S2-03) is unrelated |
| Playwright on the E2E stack: chromium, firefox, mobile-chrome | 45 passed (J1, smoke, helper self-tests). WebKit could not start on this machine (missing system libraries, `playwright install-deps`); CI installs them and runs all five projects |
| `node scripts/check-invariants.mjs` | Passed |
| `node scripts/check-figma-links.mjs` | Passed (1 checked, 11 not built yet, 1 pending); no mapped component touched |
| `node --test scripts/check-i18n.test.mjs`, `node scripts/check-i18n.mjs` | 10 passed; 0 errors |
| `reuse lint` (image `fsfe/reuse:latest`) | 619 / 619 files compliant |
| Backend tests | Not run: no backend change |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | Work off `LEGACY_EXCEPTIONS` with the steps: delete an entry when a step replaces or fixes its file; close #158 when the list is empty. S2-15 – S2-17 should remove the M1 entry (`layout/header/header.ts`) | #158, each migration step |
| P2 | `SnackbarService` buttons 'Close' / 'Dismiss' are hard-coded English UI texts (CLAUDE §6.5); the snackbar stays after M3 (S2-11 uses it for the generic error text). Needs keys from the UX copy | Proposed issue; owner (UX copy) |
| P3 | No test opens the legacy criterion proposal (markdown editor, R2) | Retired in step 5 (M5.6); no action unless the editor is reused |
| P4 | `e2e/` (Playwright, own `package.json`) and `scripts/*.mjs` have no ESLint; stage 1 only type-checks `e2e/` | Proposed issue, low priority |
| P5 | TECH_DEBT_v1 TD-07 / #119: closed by this PR; recorded in `TECH_DEBT_v2.md` at the sprint close-out (process step 14) | Sprint 02 close-out |
| P6 | SPRINT_02 R5 and the S2-14 row still say "M1–M3"; the list now covers M1–M5 (owner, F4) | Sprint 02 close-out |

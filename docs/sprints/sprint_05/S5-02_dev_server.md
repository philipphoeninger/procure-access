# S5-02 — Local dev server: npm start renders again

> **Sprint:** 05 · **Task:** S5-02 · **Issues:** #264
> **Requirements / invariants:** none (developer tooling; no invariant changes)
> **Status:** pull request #324 open (2026-11-13)
> **Scope:** make `npm start` (`ng serve`) render the app again; `provideMarkdown()` only where the legacy Markdown is rendered, which also takes `marked` and `ngx-markdown` out of the first download (S4-22 P4)
> **Owner decisions:** none for this task. Plan not approved in advance: the task ran unattended, so the plan below was followed without a stop (coordinator's instruction)

## 1. Starting point

Plan (as followed): reproduce the failure on a dev server at port 4300 (4200 is taken by the owner's stack), measure the initial bundle of the release build, move `provideMarkdown()` from `app.config.ts` into the one legacy component that renders `<markdown>`, check whether the dev server then also serves `/filters`, and if not make the optional import resolvable for the dev server only; one component test; no EF migration, no invariant, no new dependency, no E2E change.

| # | Finding | Consequence |
|---|---|---|
| F1 | Reproduced #264 on `main` (7b6e774): `ng serve` builds, but the browser gets a 500 for `.angular/cache/…/vite/deps/ngx-markdown.js`; nothing renders on `/` or `/filters` | The fix must make both work |
| F2 | `ngx-markdown` is used by one component only: the legacy `CriteriaList` (`features/criteria/list/`, `<markdown [data]>`), which sits inside `ProductsContainer` at `/filters`. `MarkdownEditor` (criterion proposal) uses EasyMDE, not ngx-markdown; no `markdown` pipe anywhere; S11 renders catalog texts with its own `pa-catalog-text` (S4-15b) | `provideMarkdown()` belongs to that component |
| F3 | `provideMarkdown()` returns a plain provider array (`MarkdownService` + the optional config tokens), not `EnvironmentProviders` | It can sit in a component's `providers`. A route's `providers` in `app.routes.ts` would import `ngx-markdown` eagerly and keep it in the first download, so the component is the lazier place |
| F4 | `ngx-markdown` loads its optional peer `marked-katex-extension` (not installed) with `await import('marked-katex-extension').…catch(() => null)`, only when a `<markdown katex>` asks for it — nobody does. Its other optional peers (prism, mermaid, clipboard, emoji) are read from globals, not imported | Only this one specifier has to be handled |
| F5 | With `provideMarkdown()` moved, `/` renders, but `/filters` still fails: its chunk imports the pre-bundled `ngx-markdown.js`, which still 500s | A dev-server setting is needed besides the move |
| F6 | `prebundle: { exclude: ["ngx-markdown"] }` in the serve options (option (a) of #264) does not work: the dev server's `angular-external-packages` plugin then fails the whole build with „Could not resolve "marked-katex-extension"“ | Not taken |
| F7 | `externalDependencies: ["marked-katex-extension"]` in the `development` build configuration works: the pre-bundling leaves the import alone, `/` and `/filters` render, and the Markdown of the legacy list renders | Taken; production configuration untouched (its build never failed) |

Initial bundle of the release build (`npm run build:release`):

| | Raw | Estimated transfer |
|---|---|---|
| Before (main 7b6e774) | 763.21 kB | 173.84 kB |
| After | **700.81 kB** (−62.4 kB) | **157.63 kB** (−16.2 kB) |

The lazy chunk `products-container` (route `/filters`) grows from 113.69 kB to 176.59 kB. The S4-22 estimate of ≈ 75 kB was the esbuild metafile input size; minified it is ≈ 62 kB.

## 2. Changes

| File | Change | Reason |
|---|---|---|
| `CLIENT/CLIENT.ProcureAccess/src/app/app.config.ts` | `provideMarkdown()` and its import removed | F2, F3 |
| `CLIENT/CLIENT.ProcureAccess/src/app/features/criteria/list/criteria-list.ts` | `provideMarkdown()` in the component's `providers`, with a comment why | F2, F3; `marked` and `ngx-markdown` now load with `/filters` |
| `CLIENT/CLIENT.ProcureAccess/src/app/features/criteria/list/criteria-list.spec.ts` (new) | One component test: the legacy list renders `**Fettdruck**` as `<strong>` with no `provideMarkdown()` from the test — it fails with `NullInjectorError` when the component's provider is removed (checked) | Pins the move |
| `CLIENT/CLIENT.ProcureAccess/angular.json` | `build.configurations.development.externalDependencies: ["marked-katex-extension"]` | F5–F7; JSON has no comments, so the reason is in CLAUDE §3 and here |
| `CLAUDE.md` | §3: the S4-22 sentence on `provideMarkdown()` replaced by the S5-02 state (component provider, sizes, the dev-server setting and why); header line | Documents the change |

Not changed: ARCHITECTURE (C120 „`marked`/`ngx-markdown` stay legacy-only“ still holds), SPEC, TESTING (no invariant), Figma mapping, `LEGACY_EXCEPTIONS` (the edited `criteria-list.ts` keeps its entry: its remaining findings — constructor injection — are untouched), the `initial` budget (see P1). TECH_DEBT is versioned at the end of the sprint (process step 14): TECH_DEBT_v5 should list #264 as resolved and, in the „Legacy filter“ row, replace „`provideMarkdown()`, `marked` and `ngx-markdown`, unless #264 moves them earlier“ by „`marked`, `ngx-markdown`, their provider in `features/criteria/list/` and the `externalDependencies` entry of the `development` configuration“.

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | A component-level `provideMarkdown()` creates one `MarkdownService` per `CriteriaList` instance | **Won't fix** | There is one instance on `/filters`; no shared Markdown configuration exists |
| R2 | A development build keeps the bare `import('marked-katex-extension')` in its output | **Won't fix** | Reached only by `<markdown katex>`, which nothing uses; it is caught (`.catch(() => null)`) and ngx-markdown then throws its own „katex extension not loaded“ — the same as in production. Checked in the `ng build --configuration development` output |
| R3 | CI never runs `ng serve`, so a broken dev server is not caught | **Follow-up issue** (proposed, P2) | Not this row's scope; the release build, Vitest and E2E do not exercise the dev server's pre-bundling |
| R4 | The dev server logs „@ngrx/signals: SignalStore members cannot be overridden“ warnings (`loadingCount`, `isLoading` …) on every page | **Open point** (P3) | Pre-existing: every feature that adds `withLoading()` to the root store redefines its members (CLAUDE §3, S4-12); dev mode only |

Security: no change to any request, header or guard; the provider moves into a lazily loaded component. A11y: no markup change. Licences: no new dependency (`externalDependencies` only names a package that is *not* bundled).

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | Dev server before the fix (`ng serve --port 4300`, same configuration as `npm start`), headless Chromium | `/` and `/filters` blank; HTTP 500 for `vite/deps/ngx-markdown.js` |
| C2 | Dev server after the fix, headless Chromium | `/`: H1 „Barrierefreie Software beschaffen – Schritt für Schritt“, shell rendered, no import error. `/filters`: renders; after choosing a product type and „Basisfunktionalität“ and „Weiter“ (CORS switched off in the throwaway browser, because the API allows only origin 4200) the legacy list shows 85 `<markdown>` elements with rendered `<p>` content, no Markdown/katex error. The API at `localhost:5000` was another session's stack on an older build (404 for `/api/v1/auth/session`), not related |
| C3 | `npm run build:release` | Green; initial 700.81 kB / 157.63 kB (table in §1); the known CodeMirror selector warning only |
| C4 | `ng build --configuration development` | Green |
| C5 | `npm run test:coverage` | 57 files, 508 tests passed (one new) |
| C6 | `npm run lint` | All files pass |
| C7 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | All green |
| C8 | `reuse lint` (Docker image) | Green (1296/1296), run on a copy of the tracked and new files: in a worktree the container cannot read the repository metadata, so it would also scan the ignored `node_modules/`, `coverage/` and `.env` |
| C9 | E2E | Left to CI (another session runs the local E2E stack); `e2e/` untouched |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | The `initial` budget (850 kB warning / 1 MB error, S4-22) was not tightened to the new 700.8 kB; by the S4-22 rule (measured + ≈ 11 % / + ≈ 34 %) it would be ≈ 780 kB / ≈ 940 kB | Owner's choice; any later PR |
| P2 | Proposed issue: a CI smoke of the dev server (start `ng serve`, load `/`, fail on a 5xx or a page error), so #264 cannot come back unnoticed | Proposed issue |
| P3 | Dev-mode warnings of NgRx Signals about overridden store members (R4) | With the root store's rework (`withCatalog()`, legacy slices retired) |
| P4 | `marked`, `ngx-markdown`, the component provider and the `externalDependencies` entry go with the legacy criteria list | Legacy filter removal (activation of 2026.1) |

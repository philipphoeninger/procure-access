# S3-26 — ESLint for e2e/ and scripts

> **Sprint:** 03, stretch task S3-26 (TECH_DEBT_v2 §3.5, 1 point). **Issues:** #161 (S2-14 open point P4). **Requirements / invariants:** none (CI stage 1); no invariant changes status. New lint step in stage 1, so a new TESTING version: `TESTING_v39.md` V118 (`TESTING_v37.md` deleted, two-version rule). **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: ESLint with type-aware TypeScript rules for `e2e/` and for `scripts/*.mjs`, run in CI stage 1, so that a missing `await` before a Playwright `expect` is reported; findings fixed or listed. **Not in scope:** the Python and shell generator of S3-25 (`scripts/subset-material-symbols.*`), the client's ESLint config, Dependabot rules.
> **Owner decisions** (2026-11-09, before the task):
> 1. **O-S3-8 — no `eslint-plugin-playwright`.** The plugin was licence-checked (MIT, one dependency `globals`, MIT); the type-aware `no-floating-promises` already reports the missing `await`.
> 2. **Dev dependencies OK:** `eslint`, `@eslint/js`, `typescript-eslint` in `e2e/` (licence check §1 F5).
> 3. **Scripts: rule set (c)** — `@eslint/js` recommended plus the type-aware promise rules `no-floating-promises` and `no-misused-promises`; not the full `recommendedTypeChecked`, whose `no-unsafe-*` rules would mostly report the untyped `JSON.parse` results of plain JavaScript. Where: linted with the packages of `e2e/`, no own `package.json` for `scripts/`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | CI stage 1 only type-checked `e2e/` (`npm run typecheck`); `scripts/*.mjs` (10 files: five checks, five self-tests on `node:test`) had no static check. The client's `eslint.config.js` covers only `CLIENT/CLIENT.ProcureAccess` | #161 |
| F2 | ESLint 10 lints no file outside the folder it starts from: from `e2e/`, `eslint ../scripts` answers „all of the files … are ignored“ (also with an absolute `basePath` in the config object); with no `eslint.config.*` in `scripts/` or the root, file-based config lookup finds nothing | `npm run lint` in `e2e/` runs `cd .. && eslint -c e2e/eslint.config.mjs e2e scripts`: with `-c` the base path is the repository root, `files` patterns are written from there. No root `package.json`, no third npm project |
| F3 | `node:test`'s `test()` returns a promise that the runner handles itself; the self-tests never await it. Dry run: `no-floating-promises` reported about 40 of these calls | `allowForKnownSafeCalls` for `test`, `describe`, `it`, `suite` from the package `node:test`; any other unawaited promise in those files is still reported (probe 5) |
| F4 | `@eslint/js` recommended turns on `no-undef`; the scripts use the Node globals `console` and `process` only. The usual source of a globals list is the `globals` package, a further dependency | The two globals are named in the config; a script that needs another one adds it there (comment, CLAUDE §3) |
| F5 | **Licence check** (CLAUDE §10, development tools, not shipped — `e2e/` is in no image): `eslint` 10.14.0, `@eslint/js` 10.0.1, `typescript-eslint` 8.72.2 are MIT; the 95 new transitive packages are MIT (70), Apache-2.0 (12), BSD-2-Clause (6), BSD-3-Clause (1), ISC (5), BlueOak-1.0.0 (1, `minimatch`) — all OSI-approved, no copyleft, the same set the client already has | Approved (decision 2) |
| F6 | `npm audit` in `e2e/` now reports the moderate advisory GHSA-g3hv-75wg-m249 (`minimatch` 10.2.6 through `eslint` 10 and `typescript-eslint`); the only offered fix is a downgrade to ESLint 9. The client has the same chain and the open Dependabot alert #12 for it | Not fixed here: development tool on our own glob patterns; GitHub will likely open a second alert for `e2e/package-lock.json`. Open point P2 |
| F7 | Dry run of the decided config: **0 findings** in `e2e/` (15 `.ts` files, `recommendedTypeChecked`) and in `scripts/` (10 `.mjs` files) once F3 and F4 were in the config | Nothing to fix; no exception list |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `e2e/eslint.config.mjs` (new) | `e2e/**/*.ts`: `tseslint.configs.recommendedTypeChecked` with `projectService` (e2e's `tsconfig.json`). `scripts/*.mjs` and `e2e/*.mjs` (the config itself): `js.configs.recommended`, the typescript-eslint parser with `e2e/tsconfig.scripts.json`, `no-floating-promises` (with the `node:test` safe calls, F3) and `no-misused-promises`, globals `console` and `process` (F4). Ignores `node_modules/`, `test-results/`, `playwright-report/`, `blob-report/` | Decisions 1 and 3 |
| `e2e/tsconfig.scripts.json` (new) | `allowJs` without `checkJs`, `NodeNext`, `types: ["node"]` (resolved from `e2e/node_modules`); includes `../scripts/*.mjs` and `*.mjs`. Only ESLint reads it; `npm run typecheck` still uses `tsconfig.json` | Type information for the two promise rules |
| `e2e/package.json`, `package-lock.json` | Dev dependencies `@eslint/js` ^10.0.1, `eslint` ^10.14.0, `typescript-eslint` ^8.72.2; script `lint` (F2) | Decision 2 |
| `.github/workflows/ci.yml` | Stage 1, after the e2e TypeScript check (same Node of `e2e/.nvmrc`, `npm ci` already done): step „ESLint (e2e and scripts)“ = `npm run lint` in `e2e/` | #161 |
| `e2e/README.md` | `npm run typecheck && npm run lint` under „Run“, a paragraph on the config | Handover |
| `docs/TESTING_v39.md` (new; `TESTING_v37.md` deleted) | V118; tools tables §3.2 and §3.3, layout §3.4, stage 1 in §8.2 and its note. No invariant changes status | Stage 1 is described there |
| `CLAUDE.md` | Header; §3 (current state after the S2-14 lint sentence); §4 (`e2e/` line); §5 (command); §9 (no exception list for `e2e/` and `scripts/`) | DoD |

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review` over the whole diff. Security: the CI step takes no input from outside the repository; the new packages are development tools that no image ships. No query, no UI text, no production code; a11y not touched.

| # | Finding | Sorting |
|---|---|---|
| R1 | `scripts/` is linted with the packages of `e2e/`, so `npm install` in `e2e/` is needed before linting a script locally | **Won't fix:** decided („where“ of decision 3); stated in CLAUDE §5 and the README. CI installs `e2e/` in stage 1 anyway |
| R2 | The scripts block uses `parserOptions.project` (a fixed tsconfig) instead of `projectService`: a `.mjs` file outside `scripts/*.mjs` / `e2e/*.mjs` would not be linted, one in a subfolder of `scripts/` neither | **Won't fix:** `scripts/` has no subfolders; the `files` pattern and the tsconfig `include` name the same files, so a new top-level script is linted at once. Recorded in V118 |
| R3 | `typescript-eslint` is `^8.72.2` in `e2e/` and `^8.72.1` in the client; Dependabot updates both folders separately, so the two lint setups can drift by a version | **Won't fix:** both resolve the newest 8.x; no rule ties them (stated in the plan). Open point P3 |
| R4 | `no-undef` knows only `console` and `process`: a script that starts using e.g. `URL` or `setTimeout` as a global fails lint | Intended: the comment in the config and CLAUDE §3 say to add the global there |

Probes (each shows that a check can fail; files deleted afterwards). Run from the repository root with the committed config:

| # | Probe | Result |
|---|---|---|
| 1 | `e2e/smoke/probe.spec.ts`: `expect(page.locator('h1')).toBeVisible();` without `await` | `no-floating-promises` ✅ |
| 2 | same file: `expect.soft(page).toHaveTitle('x');` and `test.step('s', () => {});` without `await` | `no-floating-promises` (2×) ✅ |
| 3 | `scripts/probe.mjs`: unawaited `readFile('x')`, an unused `let`, a call of an undefined name, `forEach(async …)` | `no-floating-promises`, `no-unused-vars`, `no-undef`, `no-misused-promises` ✅ |
| 4 | `scripts/probe.test.mjs`: `test('fine', () => {})` from `node:test` | not reported (F3) ✅ |
| 5 | same file: unawaited `readFile('y')` | `no-floating-promises` ✅ |

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npm run lint` in `e2e/` (Node 24.22.1) | ✅ 26 files (15 `.ts` in `e2e/`, the config, 10 `.mjs` in `scripts/`), 0 findings, about 3 s |
| 2 | `npm run typecheck` in `e2e/` | ✅ |
| 3 | `node scripts/check-invariants.mjs` (reads `TESTING_v39.md`), `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | ✅ all green |
| 4 | The five script self-tests (`node --test scripts/*.test.mjs`) | ✅ 76 passed (10 + 11 + 13 + 36 + 6) — their files are linted now and unchanged |
| 5 | `reuse lint` (fsfe/reuse); actionlint 1.7.12 (Docker, not added) | ✅ compliant (1,058 files; `tsconfig.scripts.json` covered by `**/*.json` in `REUSE.toml`); ✅ no findings |
| 6 | Playwright, client build and tests, .NET build and tests | Not run: no test, client or backend file changed; the new dev dependencies do not reach Playwright. CI stage 6 runs all five browser projects |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | TECH_DEBT: #161 moves to „resolved“ in the next version, and its open question (§6, `eslint-plugin-playwright`) is answered by O-S3-8 | TECH_DEBT_v3 (end of the sprint) |
| P2 | `minimatch` advisory GHSA-g3hv-75wg-m249 in the ESLint 10 chain (F6): Dependabot alert #12 for the client, probably a second one for `e2e/`; fixed when ESLint / typescript-eslint take a fixed `minimatch` | Dependabot |
| P3 | `eslint` and `typescript-eslint` are updated separately in `e2e/` (group `e2e`) and the client (R3); if the two drift and one breaks, a Dependabot rule like the TypeScript one (O-S3-1) could tie them | Only if it hurts |

Fitted one session.

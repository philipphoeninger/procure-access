# S2-23 — Client coverage exclusions

> **Sprint:** 02, stretch task S2-23 (0.5 points; TECH_DEBT_v1 §9.2 "first new client code"). **Issue:** #97. **Requirements / invariants:** none (coverage is reported, not gated, TESTING_v23 §8.4); no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-05), pull request open.
> **Scope:** leave the `models/` folders under `src/app` out of the client coverage report; environment files checked. Not in scope: other files without logic (routes, `app.config.ts`), a coverage gate.
> **Owner decisions 2026-11-05 (before the task, asked by Claude):** exclude by folder (`src/app/**/models/**`), not by file name (`*.dto.ts`, `*.enum.ts` …), because the target layout `features/<name>/{models,…}` keeps types in `models/`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `angular.json` (`test` target): `coverageInclude: ["src/app/**/*.ts"]`, `coverageExclude` only specs and `src/app/testing/**` | One more exclude entry |
| F2 | All model, DTO and enum files of the client lie in `models/` folders (`core/models`, `features/{criteria,favorites,filters,identity,products,proposal,settings}/models`). None has a method: plain fields, `typedjson` classes with a constructor, enums and constant maps (`ERROR_TRANSLATIONS`, `LANGUAGES`) | The folder exclude hides no logic |
| F3 | The environment files lie in `src/environments/`, outside `coverageInclude`; the baseline lcov holds none of them | Nothing to change for them; noted here instead of a redundant exclude |
| F4 | The issue's 0.52 % lines dates from S1-17. Before this change: lines 35.76 % (465/1300), 114 files, 38 of them in `models/` with 124 of 282 lines "covered" — importing a model runs its field declarations and decorators | Removing them lowers the figure a little (§4); the figure now leaves out files with nothing to test, which is the point of #97 |
| F5 | CI stage 3 runs `npm run test:coverage`, which reads these options; stage 8 merges its `lcov.info` | No change in `ci.yml` |

## 2. Changes

| File | Change |
|---|---|
| `CLIENT/CLIENT.ProcureAccess/angular.json` | `coverageExclude` gets `"src/app/**/models/**"` |
| `CLAUDE.md` | Version line; §3 (coverage note of S1-17: `models/` left out, environment files never counted); §5 (`npm run test:coverage` comment) |

TESTING_v23 §8.4 already says DTOs are excluded from measurement, so it does not change. TECH_DEBT_v1 keeps #97 until `TECH_DEBT_v2` at the sprint close-out, as for the other items closed in this sprint.

## 3. Self-review

Checklist of CLAUDE §6.7 (with *Engineering/code-review*):

| Area | Result |
|---|---|
| Security | Not touched: only the coverage report changes |
| Correctness | The report lost exactly the 38 `models/` files; the list of other files is identical before and after. Test run unchanged (24 files, 110 tests) |
| N+1, performance | Not touched |
| Accessibility | No UI change |

Findings, sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | My first CLAUDE.md wording ("the figure counts only files with code to test") claimed too much: routes and configs without logic still count | Fixed |
| R2 | A future file with logic placed in a `models/` folder would drop out of the report | Won't fix: folder exclusion is the owner's choice; the target layout keeps only types in `models/` |

## 4. Checks

| Check | Result |
|---|---|
| `npm run test:coverage` before | Statements 42.2 % (663/1571), branches 58.28 % (415/712), functions 32.33 % (140/433), lines 35.76 % (465/1300); 114 files |
| `npm run test:coverage` after | Statements 41.13 % (524/1274), branches 58.17 % (402/691), functions 32.24 % (128/397), lines 33.49 % (341/1018); 76 files, none in `models/` or `environments/` |
| `node scripts/check-invariants.mjs` | Passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 24 not built yet, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors |
| `reuse lint` | 667/667 files compliant |
| CI | On the pull request |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | Other client files without logic (`*.routes.ts`, `app.config.ts`) still count | Leave them: they are few and partly run by tests; revisit only if they distort the figure |

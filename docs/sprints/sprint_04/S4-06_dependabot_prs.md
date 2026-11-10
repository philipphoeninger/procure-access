# S4-06 — Review of the Dependabot pull requests #226 – #231 (and #252)

> **Sprint:** 04, P0 task S4-06 (debt, 0.5 points). **Pull requests:** #226 (`@playwright/test` 1.65 in `e2e/`), #227 (Angular group), #228 (Vitest group), #229 (client `other` group), #230 (jsdom 31), #231 (xunit.v3 4.0.3); #252 (`dotnet` group, EF Core 10.0.15) opened during the review and was taken in. **Requirements / invariants:** none activated; CLAUDE §10 and ARCHITECTURE D12 (licence rule); A11Y-01 and the component tests stay green. **Status:** review by Claude Code (2026-11-10). **All seven pull requests are merged by the owner** (2026-11-10, 14:17 – 14:37 UTC); this protocol records the review afterwards.
> **Scope:** for each pull request: the licence of every new version, the changelog for breaking changes, the CI result, and the reason for the merge. Not in scope: changes beyond what a pull request needed to pass CI (→ §5).
> **Deviations from the task prompt** (owner, 2026-11-10, in the session of S4-03):
> 1. **The owner merged the pull requests**, instead of only receiving a recommendation (prompt: "Do not merge any of the Dependabot PRs"). Claude Code could not merge them: the permission check of Claude Code blocked `gh pr merge`.
> 2. **Claude Code pushed fixes onto three Dependabot branches** at the owner's request ("Fix #226 yourself", "fix #229 and #252"): #226, #229 and #252 did not pass CI as Dependabot opened them (§1).
> 3. **#252 was added** to the review: it was opened on 2026-11-10 and the owner asked for it to be fixed together with the others.
>
> **Order of the review — not as the prompt asks** (Claude Code, stated at the owner's question on 2026-11-10):
> 4. **The changelogs came after the merge recommendation.** When the owner asked to merge the green pull requests, Claude Code recommended #226 – #228, #230 and #231 on the licence check (new versions and new transitive packages) and the CI result alone. The changelogs in §1.1 – §1.7, including the breaking changes of jsdom 31, were read only while writing this protocol, after the merges. They contain nothing that affects this project, so no recommendation would have changed; but for a major the changelog belongs before the recommendation.
> 5. **No local run of the affected suites for jsdom 31.** The prompt asks for it, and S3-01 (check 5) ran the client's lint, unit tests and release build on the merged pull requests in a scratch worktree. Here the evidence is CI: the run of #230 and the run on `main` after #227, #228 and #230 together (F5), which covers the same commands. That was not a plan, it followed from the merges.
> 6. **No plan before the work** (step 2 of the way of working), and the reading list was taken up late: the S3-01 protocol and TECH_DEBT_v3 §7 and §9.6 only after the merges. The alert check of §9.6 is in §1.8, the summary table of S3-01 in §1.9.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | #226 failed at *Check the Playwright image* after S4-01b (#250): the image pin `e2e/playwright-image/Dockerfile` still named 1.64.1 | Expected (S4-01b P1). Image bump pushed onto #226 (§1.1) |
| F2 | #226, after the image bump: WebKit and mobile Safari failed two self-tests of the axe helper (`support/axe-helper.spec.ts`, the "clean page" in the dark appearance): `color-contrast` on an unstyled `<button>Weiter</button>`. The screenshot shows WebKit 1.65 drawing the whole fixture page dark although it declares no `color-scheme`. All app pages passed in WebKit (45 of 47 tests), because the app styles its buttons through Material | A browser default on a test fixture, not an app finding. The fixture's button got its own colours (§1.1) |
| F3 | #229 failed at the icon check: the font subset was cut from `material-symbols` 0.48.2, the pull request pins 0.48.3 | By design (S3-25, owner 2a): a `material-symbols` update stays red until the subset is regenerated (§1.4) |
| F4 | #252 failed in stages 1 and 2. It bumped EF Core only in the three module projects, so `API`, `DAL`, `MODELS`, `SERVICES` stayed on 10.0.14 and NuGet reported a package downgrade (`NU1605`). It was also opened while Microsoft was still publishing 10.0.15: `Microsoft.EntityFrameworkCore.Abstractions` 10.0.15 was not yet listed (`NU1603`), later `Microsoft.AspNetCore.TestHost` and `…Authentication.JwtBearer` were missing. By 14:27 UTC all were listed | Every Microsoft 10.0.x package moved to 10.0.15 together, after NuGet listed them all (§1.7) |
| F5 | #227, #228 and #230 all change the client's `package.json` and `package-lock.json`. They were merged within 33 seconds of each other, so the combination never ran in CI on a pull request | The CI run on `main` after the three merges (`4bc6c75`, run 42832704070) is green: the combination is tested there (§4) |
| F6 | The sprint plan calls #231 "xunit.v3 4, a major". The pull request moves 4.0.2 → 4.0.3, a patch; the major (3 → 4) was taken earlier | No major review needed for #231 |
| F7 | #230's green CI run (42712322924) is from 2026-11-09, before S4-01b; it passed on the old apt path | Its merge commit was tested on `main` (F5) |
| F8 | None of the seven pull requests is a security update: no Dependabot alert was open for their packages, and none was closed by the merges. The two open alerts (#12, #13) concern `minimatch` (§1.8) | TECH_DEBT_v3 §9.6 check: nothing closed that should have been; TD-17 unchanged |

### 1.1 #226 — `@playwright/test` 1.64.1 → 1.65.0 (`e2e/`)

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `@playwright/test`, `playwright`, `playwright-core` 1.65.0 | Apache-2.0 | No (E2E tests) | Node 20 no longer tested (we run 24 and 26). Docker images now bring **Node 26** instead of 24. `.css`/`.less`/`.scss` imports in test files load as empty modules (we have none). The blob reporter clears its output folder again (we do not use it). New: the runner can run under Bun (not used) |
| `mcr.microsoft.com/playwright:v1.65.0-noble@sha256:6e3fb709…` | Apache-2.0 (image contents as in S4-01b §2) | No (CI) | Pin bumped in `e2e/playwright-image/Dockerfile` (commit `a56f5d6`). The image's Node is 26, `e2e/.nvmrc` stays 24 for `npm ci` and local runs; `e2e/` has no native module and no install script, so its `node_modules` run on both. Noted in the Dockerfile and the `ci.yml` comment |

No new package in the lockfile. **Fixture** (commit `58fce3d`): the "clean page" of the axe helper self-test gives its button explicit colours (F2), with a comment why.

**CI:** all 18 checks green on `58fce3d` (run 42832076543), 47 tests in each of the five browser projects. **Merged** (`c729ba2`): CI on `main` green (run 42833253035).

### 1.2 #227 — Angular group: 12 packages

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `@angular/common`, `compiler`, `core`, `forms`, `platform-browser`, `router` 22.2.4 → 22.2.5 | MIT | **Yes** (browser bundle) | Patch release, fixes only |
| `@angular/cdk`, `@angular/material` 22.2.5 → 22.2.6 | MIT | **Yes** | Patch release |
| `@angular/build`, `@angular/cli`, `@angular-devkit/core` 22.2.5 → 22.2.6; `@angular/compiler-cli` 22.2.4 → 22.2.5 | MIT | No (build) | Patch release; Node `^22.22.3 \|\| ^24.15.0 \|\| >=26` (we run 24.22.1) |

No new package in the lockfile. **CI:** green on `9e8d0a3` (run 42830131538). **Merged** (`706d6ac`).

### 1.3 #228 — Vitest group: `vitest`, `@vitest/coverage-v8` 5.0.5 → 5.1.0

MIT, not shipped (client unit tests). Minor release without a breaking change in its notes; Node `^22.12.0 || ^24.0.0 || >=26`. No new package. **CI:** green on `5ec7dd3` (run 42830028239); 3. Unit (Angular) green. **Merged** (`ef0e401`).

### 1.4 #229 — client `other` group: `marked` 18.1.2 → 18.1.3, `material-symbols` 0.48.2 → 0.48.3

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `marked` 18.1.3 | MIT | **Yes** (Markdown rendering through `ngx-markdown`) | Patch release |
| `material-symbols` 0.48.3 | Apache-2.0 | **Yes** (as the subset font) | Glyph update. The subset was regenerated with `scripts/subset-material-symbols.sh` (commit `b8c5c44`): same 34 icons, 24 108 → 24 144 bytes; `check-icons.mjs` 0 errors; `reuse lint` green |

No new package. **CI:** green on `b8c5c44` (run 42834246715). **Merged** (`9690a4b`).

### 1.5 #230 — `jsdom` 30.1.3 → 31.0.0 (major, client dev dependency)

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `jsdom` 31.0.0 | MIT | No (Vitest environment) | (1) Canvas backend is now `@napi-rs/canvas` — we use no canvas. (2) New CSS parser and computed-style engine; serialisation and computed values can differ — the component tests query by role and name, not by computed style, and all passed. (3) Parsing pauses at blocking scripts; `JSDOM` can return a partly parsed document — Vitest builds the environment itself, not through the `JSDOM` constructor. (4) Page `load` fires only on `window`; focus leaves elements that become non-focusable — no test listens for `load` on `document`. Node `^22.22.2 \|\| ^24.15.0 \|\| >=26` |
| New: `bcp-47`, `bcp-47-match`, `is-alphabetical`, `is-alphanumerical`, `is-decimal`, `parse5-parser-stream` | MIT | No | Dependencies of the new CSS and parser code |
| New: `parse5/node_modules/entities` | BSD-2-Clause | No | Nested copy for `parse5` |

All new packages are OSI-approved without commercial restriction (dev-tool rule, CLAUDE §10). **CI:** green on `52775cb` (run 42712322924, F7). **Merged** (`4bc6c75`); CI on `main` green (run 42832704070, F5).

### 1.6 #231 — `xunit.v3` 4.0.2 → 4.0.3

Apache-2.0, not shipped (backend tests). Patch (F6): crash detection no longer claims a test process crashed too early; interface methods used as test templates are no longer reported as invalid overloads; `DateOnly` shown properly in assertion messages. **CI:** green on `5bfb511` (run 42830697715). **Merged** (`9ee9585`).

### 1.7 #252 — `dotnet` group: EF Core and Data Protection 10.0.14 → 10.0.15

| Package | Licence | Shipped? | Change |
|---|---|---|---|
| `Microsoft.EntityFrameworkCore`, `.Design`, `.Tools`; `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `.Identity.UI`, `.DataProtection.EntityFrameworkCore`, `.Authentication.JwtBearer`; `Microsoft.AspNetCore.Mvc.Testing` | MIT | **Yes** (API image), except `Design`, `Tools`, `Mvc.Testing` | .NET servicing release of 2026-11-10. Its entry was not yet in `dotnet/core` `releases.json` during the review, so security fixes are not confirmed here; 10.0.14 (2026-10-25) carried four CVE fixes |

Dependabot changed only the three module projects (F4). Commit `809088b` moves the same packages in `API`, `DAL`, `MODELS`, `SERVICES` and `Tests.Integration` to 10.0.15, `JwtBearer` included, so every Microsoft 10.0.x package has one version (`grep 'Version="10.0.14"'` is empty). Local checks after NuGet listed all packages: restore without HTTP cache, build with `-warnaserror` 0 warnings, `dotnet test --solution` 639 passed, `dotnet format --verify-no-changes` clean. `Npgsql.EntityFrameworkCore.PostgreSQL` stays 10.0.3. **CI:** green on `809088b` (run 42834127879). **Merged** (`31ed54a`).

### 1.8 Dependabot alerts (TECH_DEBT_v3 §9.6)

Read on 2026-11-10 after the merges (`gh api …/dependabot/alerts`):

| Alert | Package | Where | Advisory | State | Effect of the seven pull requests |
|---|---|---|---|---|---|
| #12 | `minimatch` 10.2.6 | `CLIENT/CLIENT.ProcureAccess/package-lock.json` | GHSA-g3hv-75wg-m249 (moderate), fixed in 11.0.1 | open, scope development | None: only `eslint` 10, `@eslint/config-array` and `@typescript-eslint/typescript-estree` pull it in (`^10.2.x`), none of them was updated |
| #13 | `minimatch` 10.2.6 | `e2e/package-lock.json` | same | open, scope development | None, same dependents |

No other alert is open. The last alerts closed by an update were #1, #8 – #10 (AutoMapper, 2026-11-05, S3-07). This is TD-17: it stays open until an ESLint release moves to `minimatch` 11 (P5).

### 1.9 Summary

| PR | Recommendation | Reason in one line | Decision |
|---|---|---|---|
| #226 | Merge, with the image bump and the fixture fix | Apache-2.0, no new package; Playwright 1.65 breaks nothing we use; image pin must match (S4-01b), the WebKit fixture needed own colours | **Merged** by the owner (`c729ba2`) |
| #227 | Merge | Angular patch releases, MIT, no new package | **Merged** (`706d6ac`) |
| #228 | Merge | Vitest minor without breaking change, MIT, not shipped | **Merged** (`ef0e401`) |
| #229 | Merge, with the regenerated icon subset | MIT/Apache-2.0 patch releases; subset must follow the pinned `material-symbols` (S3-25) | **Merged** (`9690a4b`) |
| #230 | Merge | jsdom 31 (major): its breaking changes (canvas backend, CSS engine, parser, `load` on `window`) touch nothing in our component tests; new packages MIT/BSD-2-Clause, not shipped | **Merged** (`4bc6c75`) |
| #231 | Merge | xunit.v3 patch (not a major, F6), Apache-2.0 | **Merged** (`9ee9585`) |
| #252 | Merge, with every Microsoft 10.0.x package on 10.0.15 | .NET servicing release, MIT; all projects on one version | **Merged** (`31ed54a`) |

The recommendations for #226 – #228, #230 and #231 were given before the changelogs were read (deviation 4); the reasons above include them.

## 2. Changes

This pull request: only this protocol. The changes of the review went in with the Dependabot pull requests themselves:

| Pull request | Commit | Change |
|---|---|---|
| #226 | `a56f5d6` | `e2e/playwright-image/Dockerfile` → `v1.65.0-noble@sha256:6e3fb709…`, note on the image's Node 26; `ci.yml` comment: `e2e/node_modules` runs on the image's Node |
| #226 | `58fce3d` | `e2e/support/axe-helper.spec.ts`: explicit colours for the fixture's button (F2) |
| #229 | `b8c5c44` | Regenerated `material-symbols-outlined.woff2` and its manifest |
| #252 | `809088b` | Every Microsoft 10.0.x `PackageReference` → 10.0.15 |

## 3. Self-review (CLAUDE §6.7)

| Area | Result |
|---|---|
| Licences | Every new version and every new transitive package checked (§1): MIT, Apache-2.0, BSD-2-Clause. Shipped: Angular, CDK/Material, `marked`, `material-symbols`, the Microsoft packages — no strong copyleft |
| Correctness | The fixture change (F2) changes what the helper is tested on, not the helper: the dark-only fixture still fails as before, and both appearances are still checked on every app page. #252 moves only the version numbers |
| Security | .NET servicing update taken the day it came out; no package with a known advisory left behind |
| a11y | A11Y-01 and A11Y-08 green on all five browser projects after the Playwright update; no app page needed a change |

**Findings sorted (CLAUDE §6.8):** fix — F1, F2, F3, F4 (done on the pull requests, §2); the order of the review (deviations 4 – 6) is stated, not repeated: the evidence is in §1 and §4. Won't fix — none. Follow-up — §5.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | CI of each pull request on its last head | All green, runs in §1 |
| 2 | CI on `main` after the merges | `4bc6c75` (#227, #228, #230 together): green, run 42832704070. `c729ba2` (#226): green, run 42833253035. `9690a4b` (#229): green, run 42835842089. `31ed54a` (#252, all seven merged): green, run 42835880308 |
| 3 | Licence of each new version (`npm view … license`, NuGet `.nuspec`, lockfile `license` fields of new packages) | §1 |
| 4 | #252 locally after NuGet listed every 10.0.15 package | Build 0 warnings; 639 tests passed; format clean |
| 5 | #229 locally | `check-icons.mjs` 0 errors; `reuse lint` compliant |
| 6 | This pull request: `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `reuse lint` | Green |
| 7 | Dependabot alerts, open and closed since 2026-11-01 (`gh api …/dependabot/alerts`); `minimatch` and its dependents in both lockfiles | §1.8: two open (`minimatch`, development), none closed by these merges |
| 8 | Release notes: Playwright 1.65.0, Vitest 5.1.0, jsdom 31.0.0, xunit.v3 4.0.3; `dotnet/core` `releases.json` (10.0.15 not yet listed) | Read after the merges (deviation 4); findings in §1.1 – §1.7 |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | Pull requests that change the same lockfile were merged without waiting for the next one's rebase (F5). It worked because Git merged the lockfiles cleanly and CI on `main` was green, but a broken combination would only show on `main` | Merge one lockfile pull request at a time and let Dependabot rebase the next; or turn on "Require branches to be up to date" in the ruleset (slower: every merge needs a new CI run). Owner's call |
| P2 | Dependabot's `nuget` group can open a pull request in the middle of a .NET servicing release (F4) and bump only part of the projects | No rule change: the build catches both. If it repeats, a `cooldown` of one or two days for the `nuget` ecosystem in `dependabot.yml` would let the release finish first |
| P3 | Running the E2E tests on the image's Node 26 while `npm ci`, local runs and the client stay on 24 (F1) | Fine while `e2e/` has no native module. When the project moves to Node 26 (CLAUDE §3: `.nvmrc` and the client Dockerfile together), the two meet again |
| P4 | S4-01b protocol: P1 (#226) is done by this review; F3 there names Node 24 for the image, true only for 1.64.1 | No change to the S4-01b protocol (it records the state of its PR); this protocol carries the update |
| P5 | TD-17: `minimatch` alerts #12 and #13 stay open (§1.8) | Unchanged: take the ESLint release that moves to `minimatch` 11 when Dependabot offers it |
| P6 | For later Dependabot reviews: read the changelog of every major before recommending a merge, and run the affected suites locally on the merged combination (as S3-01 check 5), also when the owner asks for a quick merge | Lesson of this task (deviations 4, 5) |

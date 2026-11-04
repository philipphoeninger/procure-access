# S2-20 — CI one-liners

> **Sprint:** 02, stretch task S2-20 (0.5 points; TECH_DEBT_v1 §3.5, §9.2 "any CI pull request"). **Issues:** #104, #105, #106, #107, plus #140 and #142 (owner decision D1). **Requirements / invariants:** none (CI); no invariant flipped, so no new TESTING version. **Status:** done by Claude Code (2026-11-04), pull request open.
> **Scope:** six small changes, one commit each: stage 8 downloads only its inputs (#104), the Playwright browser cache is saved right after the install (#105), stage 8 is skipped without a build (#106), stage 1 type-checks `e2e/` on the Node of `e2e/.nvmrc` (#107), Dependabot ignores `@types/node` majors in `e2e/` (#140), J1 no longer measures a fading tooltip (#142). Not in scope: a permanent self-test of the invariant gate (S2-09 P2), the open Dependabot PR #129 (§5).
> **Owner decisions 2026-11-04 (before the task, asked by Claude):** **D1** take #140 and #142 into this PR (both protocols S2-04 P1/P6 pointed them here). **D2 = A** show the effect of #104 – #106 in a throwaway draft PR (#173) that breaks things on purpose; it is closed without merging and its branch deleted. Taken without asking (named before the task): #106 through `needs.build.result`, #104 by narrowing the download pattern and keeping the name `coverage-report`, #105 with `actions/cache/restore` + `actions/cache/save`, #107 with a second `setup-node`.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | All six issues still open; `ci.yml` and `dependabot.yml` unchanged since they were written (main at e9323ed) | Fixes as proposed in the issues |
| F2 | Stage 8 downloads `pattern: coverage-*`; the inputs are `coverage-ProcureAccess.Tests.{Unit,Architecture,Integration}` and `coverage-client`, its own output is `coverage-report` | Pattern `coverage-{ProcureAccess.Tests.*,client}` (minimatch with braces, as `download-artifact` v8 uses it; checked locally against all six artifact names of a run) |
| F3 | Stage 1 sets up Node from the client's `.nvmrc` and runs the `e2e/` type check in the middle of the job; a second `setup-node` there would also move the later `node scripts/*.mjs` steps to the e2e Node | The two e2e steps move to the end of the job, after their own `setup-node` |
| F4 | Dependabot PR #129 (opened 2026-10-29, updated 2026-11-02) still bumps `@types/node` 24 → 26: the ignore given by comment in S2-04 did not reach this group PR | The rule in `dependabot.yml` is the place where it is visible and kept |
| F5 | The J1 step that clicks „Weiter“ is called "confirm the selection …"; the issue's "Enter" is the older button name | Comment names „Weiter“ |
| F6 | Stage 8 is a required check of the `main` ruleset (TESTING §8.2). A job skipped by its `if` reports success for required checks; the failed build blocks the merge itself | #106 needs no ruleset change |

## 2. Changes

| Commit | File | Change |
|---|---|---|
| #107 | `.github/workflows/ci.yml` stage 1 | `setup-node` with `node-version-file: e2e/.nvmrc` (no own cache: the first `setup-node` already restores the npm cache for both lock files), then the e2e `npm ci` + `typecheck`, last in the job |
| #104 | `ci.yml` stage 8 | `download-artifact` pattern `coverage-{ProcureAccess.Tests.*,client}` |
| #105 | `ci.yml` stage 6 | `actions/cache` → `actions/cache/restore` (same SHA, v6.1.0) before the install and `actions/cache/save` (key `cache-primary-key`) right after it, only on a cache miss |
| #106 | `ci.yml` stage 8 | `needs: [build, dotnet-tests, client-tests]`, `if: !cancelled() && needs.build.result == 'success'` (still runs after failed tests, with a partial report) |
| #140 | `.github/dependabot.yml` `/e2e` | `ignore` `@types/node` `version-update:semver-major`, with the comment that it follows the Node major of the `.nvmrc` files |
| #142 | `e2e/journeys/j1-configurator.spec.ts` | Before the axe check "criteria list": `page.mouse.move(0, 0)` and wait until no `.mat-mdc-tooltip-surface` is in the page; no suppression |

No document describes these details (TESTING §8.2 names the stages only, CLAUDE.md §3 not the cache or the stage-8 condition), so nothing else changes.

## 3. Self-review

Checklist of CLAUDE §6.7 (with *Engineering/code-review*):

| Area | Result |
|---|---|
| Security | No new action, permission or secret; the new `cache/restore` and `cache/save` are sub-actions of the pinned `actions/cache` SHA |
| Correctness | `actionlint` 1.7.12 clean. Each change shown in CI (§4). The e2e type check stays in stage 1, only later in the job; a failure in an earlier step still stops the job before it, as before |
| N+1, performance | A cache miss now costs one more step (save, ≈ 5 s); a hit skips it |
| Accessibility | No UI change; the J1 axe checks keep their scope (the fully shown tooltip has enough contrast, #142) |

Findings, sorted per CLAUDE §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | Re-running stage 8 uploads `coverage-report` again in the same run; immutable artifacts could refuse the name | Checked in the probe run (§4) |
| R2 | J1 finds the tooltip by its CSS class, not by role (CLAUDE §9 prefers roles) | *Won't fix*: the tooltip surface has no role; the legacy tooltip goes with the configurator rebuild (step 2) |

## 4. Checks

| Check | Result |
|---|---|
| `actionlint` 1.7.12 (Docker) on `ci.yml`; YAML parse of `dependabot.yml` | clean |
| `e2e`: `npm run typecheck` | clean |
| J1 locally (dev stack, read-only), chromium + firefox, `--repeat-each=2` | 8 passed |
| `node scripts/check-invariants.mjs` | passed |
| `node scripts/check-figma-links.mjs` | 2 checked, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors |
| `reuse lint` | compliant (661 / 661) |
| .NET build and tests, Vitest | not run locally: no .NET or client code changed; CI stages 2–5 |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | #129 (e2e group) still holds `@types/node` 26 and `typescript` 6.0 → 7.0. With the new rule Dependabot should drop `@types/node` from it on its next run. TypeScript in `e2e/` has no ignore rule, unlike the client, where the Angular release sets it | Next Dependabot review; owner: should `e2e/` follow the client's TypeScript version (rule like the client's)? |
| P2 | Stage 5 and the other test jobs upload `coverage-<project>` too; a re-run of only those jobs in the same run hits the same immutable-name question as R1 | Only if a re-run fails on it |
| P3 | `TECH_DEBT_v1.md` §3.5 and §9.2 still list #104 – #107 | Sprint close-out (process step 14, `TECH_DEBT_v2.md`) |
| P4 | S2-09 P2 (permanent self-test of the invariant gate on `node:test`) stays open; not a one-liner | Proposal for a later CI task |

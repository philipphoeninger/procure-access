# S6-08 — Dependabot batch and `npm audit fix`

> **Sprint:** 06 · **Task:** S6-08 (0.5 points) · **Pull requests:** #366, #364, #365, #363 (TD-76); TD-17 (Dependabot alerts 12 and 13), TD-51
> **Requirements / invariants:** none; every test level stays green; no invariant flipped, no new TESTING version
> **Status:** the four Dependabot pull requests merged by the owner (2026-11-18); this pull request (`npm audit fix`, protocol, CODEBASE) open
> **Scope:** exactly the row of S6-08. No EF migration, no backend code, no new package, no screen change
> **Owner decisions (2026-11-18, in the session):** the fixes of #365 (font subset) and #363 (Playwright image) go onto the Dependabot branches themselves (option (a)), so each pull request is green on its own and can be merged one at a time; the owner merges, Claude reports per pull request

## 0. Plan (written before the change)

- **Starting point:** `main` at f5f303e; the four Dependabot pull requests on base d856880. Since then `main` had changed no `package*.json`, no `*.csproj` and no Playwright Dockerfile, but the icon list and the font subset (S6-02, S6-04).
- **Order:** #366 → #364 → #365 → #363, each after `@dependabot rebase` on the previous merge (TD-51); fixes as plain commits on the Dependabot branch; local checks; a report; the owner merges. Then this branch with `npm audit fix` in its own commit(s), the protocol and CODEBASE.
- **Invariants:** none. **Migration:** none. One session, paced by the owner's merges.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | CI on the old base: #366 and #364 green, #365 red in stage 1 (icon check: manifest pins 0.48.3), #363 red in every E2E job (image tag 1.65.0 ≠ `@playwright/test` 1.65.1) | As TD-76 says |
| F2 | The row asks for the image tag "in CI and compose". The pin lives only in `e2e/playwright-image/Dockerfile`; `ci.yml` reads it from there and checks it against the lock file; `docker-compose.e2e.yml` names no Playwright image | Only the Dockerfile changes (said in the plan) |
| F3 | `docker buildx` is not installed on the development machine | The digest comes from the registry API (§2.4) |
| F4 | `npm audit` on `main`: client 25 findings, `e2e/` 13 — more than TD-17 lists. `npm audit --omit=dev`: 0 in both. New since TD-17: the high picomatch ReDoS GHSA-fc5q-39xc-vjpf (affects every version) under vite, vitest, `@angular/build`, `@parcel/watcher`, fdir/tinyglobby | minimatch is fixed here; picomatch has no patched release (§5) |
| F5 | Before #363, the minimatch fix in `e2e/` was breaking (it downgraded eslint to 9); with eslint 10.15 it is not | Both alerts can close |

## 2. Changes

### 2.1 #366 (dotnet) — merged 02:42

coverlet.MTP 10.1.0 → 10.1.1 (unit, architecture, integration tests), Testcontainers.PostgreSql 4.16.0 → 4.17.0 (integration). Both MIT; Testcontainers 4.17.0 has the same dependencies as 4.16.0. Not shipped, so `THIRD-PARTY-NOTICES.txt` stays (check: 38 packages, 0 errors). Rebased by Dependabot onto f5f303e; S6-24 (#398, client only) merged meanwhile, so no second rebase. CI 18/18 green.

### 2.2 #364 (angular) — merged 02:44

Angular 22.2.5 → 22.2.6 (core, common, compiler, compiler-cli, forms, router, platform-browser) and 22.2.6 → 22.2.7 (cdk, material, cli, build, devkit); MIT. **Deviation:** merged by the owner before the requested rebase arrived and before the report — at the old head 6990411, CI green on base d856880. The merges in between (S6-24, S6-10a, #366) touched no client lock file, and the combined state was checked afterwards: locally on `main` 288f497 lint, 827/827 tests and `build:release` green; the CI run of `main` 288f497 green on every job including the five E2E jobs.

### 2.3 #365 (other) — merged 03:47

material-symbols 0.48.3 → 0.49.0 (shipped as the font subset; Apache-2.0 unchanged), eslint 10.15.0, jsdom 31.0.1, prettier 3.10.1 (development tools, MIT). Rebased by Dependabot onto 288f497 (after #364, same lock file). **Commit a152180 on the branch:** `scripts/subset-material-symbols.sh` on the 26 icons of the current icon list → `material-symbols-outlined.woff2` 15 048 bytes, manifest `version` 0.49.0 with the new sha256. `REUSE.toml` names no font version. CI 18/18 green.

### 2.4 #363 (e2e) — merged 11:20

@playwright/test 1.65.0 → 1.65.1 (Apache-2.0), eslint 10.15.0 (MIT), both development tools. Rebased by Dependabot onto 84bfac1 (after #365). **Commit 2e32fed on the branch:** `FROM mcr.microsoft.com/playwright:v1.65.1-noble@sha256:1507cbfc550a63a6b7396791ca4f00df17beb56342d2447b3f106c306f525579`. The digest is the `Docker-Content-Digest` of the OCI image index (`curl -sI -H "Accept: application/vnd.oci.image.index.v1+json" https://mcr.microsoft.com/v2/playwright/manifests/v1.65.1-noble`), the same value `docker buildx imagetools inspect` prints; the index holds linux/amd64 and linux/arm64. The same request for v1.65.0 now answers another digest than the one pinned before: Microsoft re-pushes a tag, which is why the digest is the pin. CI's image check reproduced locally (green); CI 18/18 green, all five E2E jobs in the new image.

### 2.5 This branch

| Commit | Change |
|---|---|
| `npm audit fix` in the client | Dev only: minimatch 10.2.6/11.0.8 → **11.0.9** (GHSA-g3hv-75wg-m249, alert 12), typescript-eslint 8.72.2 → 8.73.0, angular-eslint 22.6.0 → 22.7.0, `ignore` 7.0.14, a nested `semver` 7.8.6. Licences unchanged (minimatch BlueOak-1.0.0, OSI-approved, already so; the others MIT/ISC). `package.json` unchanged |
| `npm audit fix` in `e2e/` | Dev only: minimatch → **11.0.9** (alert 13), typescript-eslint 8.73.0, `ignore` 7.0.16 |
| Docs | This protocol; CODEBASE §2.91 (the batch, the digest without buildx, the audit state) |

The row asked for `npm audit fix` "in its own commit": it is two commits, one per lock file, each with nothing else in it.

## 3. Self-review

| Finding | Sorting |
|---|---|
| Security: nothing shipped changes on this branch (`npm audit --omit=dev` 0 in client and `e2e/`); the shipped changes of the batch are Angular patches and the font | — |
| Licences (CLAUDE §10): no new package; all changed packages keep their licence | — |
| #364 merged before its report and rebase (§2.2) | No change needed: combined state checked locally and by CI on `main` |
| picomatch ReDoS (high) remains in both trees, dev only, no patched release | Follow-up (§5 P1) |
| The Dockerfile's comment names only `docker buildx imagetools inspect` for the digest | Won't fix: correct; the curl variant is in CODEBASE §2.91 |

No correctness or security finding that affects current behaviour.

## 4. Checks

| Check | Result |
|---|---|
| #366: `dotnet build -warnaserror …`, `dotnet format --verify-no-changes`, `dotnet test --solution` (Testcontainers 4.17) | 0 errors; clean; 1685 passed |
| #366: `check-api-notices.mjs` | 38 packages, 0 errors |
| `main` 288f497 (after #364): `npm ci`, lint, `ng test`, `build:release`; CI run 44225510651 | Green; 827/827; CI success |
| #365: icon check, lint, `ng test`, `build:release`, Figma link and i18n checks | Green; 827/827 |
| #363: e2e `npm ci`, `typecheck`, `lint`; CI's image check | Green |
| CI of each Dependabot pull request after the fixes | 18/18 green each |
| This branch, client: `npm ci` from the new lock, lint, `ng test`, `build:release` | Green; 858/858 |
| This branch, `e2e/`: `npm ci`, `typecheck`, `lint` | Green |
| `npm audit --omit=dev` (client, `e2e/`) | 0, 0 |
| `npm audit` (dev included) | client 22 high, `e2e/` 9 high — all the picomatch chain (§5 P1); minimatch gone |
| `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons` | Green |
| `reuse lint` (Docker) | No finding in a repository file (in a worktree the container cannot see git and also lists ignored files; filtered against `git ls-files`) |
| E2E | Not run locally: another session uses the local E2E stack; CI runs it (and ran it on #363 and on `main`) |

## 5. Open points

1. **picomatch GHSA-fc5q-39xc-vjpf (high, ReDoS)** — every version is affected; it sits under vite, vitest, `@angular/build`, `@parcel/watcher` (client) and typescript-eslint's tinyglobby/fdir (both). Dev tools only, nothing shipped. Watch for a patched picomatch or new upstream versions; Dependabot will likely open an alert. Proposed TECH_DEBT row (watch) at the sprint close-out.
2. **Alerts 12 and 13** close when this pull request is merged into `main` (Dependabot rescans the lock files); check after the merge.
3. **TECH_DEBT_v5:** TD-76 done, TD-17 done (minimatch), TD-51 handled by merging one at a time — struck in the next TECH_DEBT version (sprint close-out).
4. **TD-51 itself:** the ruleset still does not require up-to-date branches; #364 showed that a merge can overtake the requested rebase. Merging one at a time worked here; the setting stays the owner's decision.

# S4-01b — E2E tests in the pinned Playwright image

> **Sprint:** 04, follow-up to P0 task S4-01 (debt; not in the sprint plan, asked for by the owner on 2026-11-10 after the apt step failed again). **Issues:** #203 (closed by #247; it failed again). **Requirements / invariants:** none (CI, TESTING §8.2 stage 6); no invariant activated, no new TESTING version. **Status:** done by Claude Code (2026-11-10); pull request #250; proof in the throwaway draft #251 (closed, not merged, branch deleted).
> **Scope:** the E2E jobs of `.github/workflows/ci.yml`: where Playwright and its browsers come from. The stack, the tests and the required checks do not change.
> **Owner decisions:**
> 1. **O-S4-1 (1)** (sprint start): the pinned Playwright container image, "if `main` still goes red" after option (2) of S4-01. On 2026-11-10 it went red again, and the owner asked for the fix.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | After S4-01 (#247) the apt step failed three more times on 2026-11-10: `main` run 42811668839 (firefox, webkit) and twice on PR #249 (mobile-safari, run 42823865035, first run and re-run). Each time both 3-minute attempts timed out (exit 124) | The retry of S4-01 does not hold |
| F2 | Log of the failed mobile-safari job (129860361780): `apt-get update` fetched 13 MB at 10 MB/s, then `install-deps webkit` needed **181 new packages, 114 MB**. From `azure.archive.ubuntu.com` they came at about 100–300 kB/s: attempt 1 got packages 2–35 in 3 minutes (one 9 MB package took 53 s), attempt 2 another 17 | The time goes into the volume of the download. On a slow mirror two attempts of 3 minutes cannot fetch 114 MB, and a longer limit only makes the job slower. S4-01 F1 had seen the same at 40 kB/s |
| F3 | `mcr.microsoft.com/playwright:v1.64.1-noble` exists for the locked `@playwright/test` 1.64.1 (index digest `sha256:2f05f8a7…`). Its Dockerfile (Playwright repository, tag v1.64.1): Ubuntu 24.04, Node 24 (as `e2e/.nvmrc`), all browsers in `/ms-playwright` with their system packages | No browser and no apt package needs to be downloaded in the job; the image comes from MCR's CDN |
| F4 | The tests reach the stack only through `localhost:4200` and `localhost:5000` (`playwright.config.ts`, `global-setup.ts`); J1 reads a fixture from `tests/`; no test starts a process | `docker run --network=host` with the repository mounted runs them unchanged |
| F5 | Dependabot updates `@playwright/test` in `e2e/` (#226: 1.65). A `docker run` line in a workflow is not tracked by Dependabot | The pin needs a file and a check that it matches the lockfile; the update of both goes into one pull request |

## 2. Changes

| File | Change |
|---|---|
| `e2e/playwright-image/Dockerfile` (new) | One `FROM` line: `mcr.microsoft.com/playwright:v1.64.1-noble@sha256:2f05…`, with how to update it. CI only reads the line; nothing builds the file |
| `.github/workflows/ci.yml` | E2E job: new step *Check the Playwright image* (the `FROM` must be `…:v<lockfile version>-noble@sha256:…`; otherwise `::error` on the Dockerfile with the command that prints the digest), *Pull the Playwright image* (10-minute limit). The step *Playwright (<project>)* runs `npx playwright test` in the image: `--network=host`, `--ipc=host` (Chromium's shared memory), `--init`, the runner's uid and gid with `HOME=/tmp`, the repository at `/work`, `CI`, `GITHUB_ACTIONS`, `GITHUB_WORKSPACE=/work` (annotation paths of the `github` reporter) and `PROJECT`. Removed: browser cache restore and save, browser install, apt step with retry. The matrix lists only `project` (the `browser` key had no other use); job names and required checks are unchanged |
| `.github/dependabot.yml` | Note at the `e2e/` entry: a `@playwright/test` update fails CI until the Dockerfile names the same version, so push that change onto the pull request |
| `CLAUDE.md` | §3: the CI sentence of S4-01 replaced; §4: `e2e/playwright-image/`; header entry |
| `docs/sprints/sprint_04/S4-01b_playwright_image.md` | This protocol |

**Licence (CLAUDE §10):** a development tool, not shipped. Playwright and its Docker image are Apache-2.0. The image contains Ubuntu 24.04, Node.js (MIT), the browsers (Chromium BSD, Firefox MPL-2.0, WebKit LGPL/BSD) and the Ubuntu packages that `playwright install-deps` installed on the runner until now. The software is the same as before; it now comes from MCR instead of the apt mirror.

## 3. Self-review (CLAUDE §6.7)

| Area | Result |
|---|---|
| Security | The image is pinned by digest, so a re-pushed tag cannot change it. No secret goes into the container: only `CI`, `GITHUB_ACTIONS`, `GITHUB_WORKSPACE` and the project name. The `.env` with the run's random secrets lies in the mounted repository but is read by no test. The container runs as the runner's user, not as root. The host network is the runner's own, which the tests used before as well |
| Correctness | The version check compares the full tag, so `1.64.10` does not pass for `1.64.1`. `e2e/node_modules` comes from `npm ci` on the host's Node 24; the image has Node 24, and `e2e/` has no native module. Reports land in `e2e/playwright-report` and `e2e/test-results`, owned by the runner, so the artifact upload and `stack:down` work as before (proof §4, check 3) |
| Edge cases | A slow MCR pull is bounded by the step's 10 minutes; in the proof it took 25–33 s. A Playwright update without the image bump fails at the check step with its message, before the stack starts |
| Performance | E2E jobs 3–5 minutes as before (§4). Per job: no browser cache restore and no apt install; instead a pull of 990 MB (amd64, compressed, 7 layers) from MCR in 25–33 s |
| Maintainability | Fewer steps (three removed, two added); the apt retry and its root `timeout` (S4-01 F7, F8, P5) are gone |

**Findings sorted (CLAUDE §6.8):** fix — none beyond the change. Won't fix — none. Follow-up — §5.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | CI of #250 (run 42826941636) | All 18 checks green. E2E: 47 passed in each of the five projects, the same count as the last green run before the change (42727668061). Jobs 178–284 s (before: 222–283 s). Pull 25–33 s; stack start 70–104 s; tests 34–97 s |
| 2 | Version check in bash, version 1.64.1 and 1.65.0 | Match → `image=…` output; mismatch → `::error` with the message, exit 1 |
| 3 | Proof of the failure path (draft #251, run 42827912007: the smoke test expects a wrong title) | All five projects failed on exactly that test; the `github` reporter's annotation points at `e2e/smoke/stack.spec.ts:16` (right path through `GITHUB_WORKSPACE=/work`); each job uploaded `playwright-report-<project>` (1.4–4.5 MB); *Collect stack logs* and *Stop E2E stack* ran. #251 closed without merge, branch deleted |
| 4 | actionlint (Docker `rhysd/actionlint`, with shellcheck) | Clean |
| 5 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `reuse lint` | Green (CI stage 1 and 7) |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | Dependabot #226 (`@playwright/test` 1.65) now fails at *Check the Playwright image* until the Dockerfile names `v1.65.x-noble` with its digest | In S4-06: push the Dockerfile change onto #226 (command in the Dockerfile) |
| P2 | Local E2E runs still use the browsers of `npx playwright install` on the host; CI uses the image. Both run the same Playwright version, and the browsers come with it | None. Running locally in the image is possible but needs about 2 GB of disk (memory note of S2-15) |
| P3 | #203 was closed by #247 although the step still failed | Reopen #203 and close it with #250, or mention #250 in it — owner's call |
| P4 | MCR itself can be slow or down; then the pull step fails after 10 minutes | No retry for now: MCR is a CDN, unlike the single apt mirror. Watch the runs on `main` until the sprint close-out |

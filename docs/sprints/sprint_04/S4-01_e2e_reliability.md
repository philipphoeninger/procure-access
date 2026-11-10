# S4-01 — E2E reliability: the Playwright apt step no longer fails `main`

> **Sprint:** 04, P0 task S4-01 (debt, 1 point). **Issues:** #203. **Requirements / invariants:** none (CI, TESTING §8.2 stage 6); no invariant activated, no new TESTING version. **Status:** done by Claude Code (2026-11-10); pull request __PR__, proof in the throwaway draft #246 (closed, not merged).
> **Scope:** exactly the sprint row: the step that installs the browsers' apt packages in the E2E jobs of `.github/workflows/ci.yml`. Nothing else in the workflow changes.
> **Owner decisions** (2026-11-10, before the start):
> 1. **O-S4-1 (2):** a shorter limit plus one retry of the apt step. Option (1) (Playwright container image) only if `main` still goes red afterwards.
> 2. **Proof:** ten green runs in a row prove little while the flake hits about one run in twelve, so a test branch cuts the first attempt short on purpose and shows that the retry recovers; the normal duration comes from the real runs.
> 3. **Scope:** runner failures that no workflow change can fix (`abandoned` jobs, a status shown as `queued` for hours) are open points, not part of this task.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The failed step's log (run 42200002675, firefox, 2026-11-06): `apt-get update` took 5 s; then `apt-get install` fetched 48.8 MB from `azure.archive.ubuntu.com` at about 40 kB/s (one package of 5.6 MB took 2:23) until the 10-minute limit stopped it at package 14 of 40. Nothing hung inside `dpkg` | The time goes into the download. Stopping apt there is safe: it releases its lock on exit and keeps finished packages in `/var/cache/apt/archives` (partial ones in `partial/`), so a second attempt does not start from zero and opens new connections to the mirror |
| F2 | Duration of the successful apt steps in the last 40 CI runs (186 jobs, `gh api …/jobs`): median 17 s, p90 37 s, 5 over 60 s, 3 over 120 s (103, 115, 154, 172 s), the slowest 558 s | 3 minutes per attempt is about five times the p90; only 3 of 186 runs needed more, and the 558 s run would have gone to the retry with about a third of its packages already in the cache |
| F3 | The uncached path (`npx playwright install --with-deps`, 15 minutes, only after a Playwright version change) fetches the same apt packages from the same mirror | `install` without `--with-deps` fetches only the browser (Playwright's CDN); the retried apt step then runs on both paths |
| F4 | Runs on `main` since 2026-11-06: 1 of 12 failed (42674440407), and not in this step: the WebKit and mobile-Safari jobs ended `abandoned` (runner lost), so the aggregate check saw `needs.e2e.result = abandoned`. The merge of #219 failed the same way. The merge of #245 (42723147023) showed its aggregate job as `queued` for hours in the API, but it had passed at 23:50 | Not an apt failure; a re-run is the only fix → §5 P1 |
| F6 | **First proof run (42724589474, plain `timeout 3m npx …`): the retry failed in all four jobs that took it.** Attempt 2 stopped at once with `E: Could not get lock /var/lib/apt/lists/lock. It is held by process 2349 (apt-get)` or `dpkg frontend lock was locked by another process`; the old `apt-get` kept downloading after the stop. A local Ubuntu 24.04 container did not show it (the leftovers there were zombies of a container without init) | Root cause found on the runner (F7), not guessed |
| F7 | Instrumented proof runs (42725953007, 42726307356): every process sat in `timeout`'s process group, and the group signal ended npm, Node, `sudo` and `sh`, but `apt-get update` (pid 2626) lived on. Its signal state (`/proc/<pid>/status`: `SigIgn 0x1000` = only SIGPIPE, `SigCgt 0x8000000` = SIGWINCH, nothing blocked) shows it does not ignore SIGTERM; even `--kill-after` (SIGKILL) did not end it. Cause: `timeout` runs as `runner`, `apt-get` as root under `sudo`, and the kernel does not let an ordinary user signal a root process — the group kill skips it silently | `timeout` must run as root: `sudo env "PATH=$PATH" timeout …`. Playwright then runs `sh -c "apt-get …"` itself (it uses `sudo` only when it is not root), so apt stays in `timeout`'s group. Proof run 42726684084: no `apt-get` left after any stop, retry green in four jobs, no root-owned file in the workspace or `$HOME` |
| F8 | Same run, mobile-Safari (stopped at 15 s, during the install): `dpkg` (pid 2970) outlived the stop in its own session (apt starts it in a new session for its pseudo-terminal) and finished just before `dpkg --configure -a` | Before `dpkg --configure -a` the retry waits until no `dpkg` runs; otherwise the configure step could fail on the lock |
| F5 | No document except the comment in `ci.yml` describes the step (CLAUDE.md, TESTING, ARCHITECTURE checked) | #203 "Done when": CLAUDE.md §3 and the comment in `ci.yml` describe the chosen way |

## 2. Changes

| File | Change |
|---|---|
| `.github/workflows/ci.yml` | *Install Playwright browser* (cache miss only): `npx playwright install "$BROWSER"` without `--with-deps`, limit 15 → 10 minutes (it took 20–41 s including apt). *Install Playwright system dependencies*: runs on every job (no `if` on the cache hit any more); up to two attempts of `sudo env "PATH=$PATH" timeout --kill-after=10s 3m npx playwright install-deps "$BROWSER"` (root, F7; `PATH` keeps the Node of `setup-node`; SIGKILL 10 s after SIGTERM if something ignores it); a failed attempt writes a `::warning::` with its exit code (124 or 137 = stopped), waits while a `dpkg` runs (F8) and runs `sudo dpkg --configure -a` (no network, a no-op unless the stop interrupted an install); the step fails after the second attempt. Step limit 10 → 8 minutes (2 × 3 minutes plus `npx` start and `dpkg`). The comment explains why, including the root `timeout` |
| `CLAUDE.md` | §3, CI sentence: the apt step, its 3-minute limit and the retry; header entry for S4-01 |
| `docs/sprints/sprint_04/S4-01_e2e_reliability.md` | This protocol |

`timeout` (coreutils) puts itself and `npx` into their own process group and signals the whole group when the limit expires. As root it reaches every process of the group, `apt-get` and its download methods included (F7); only `dpkg`, which apt starts in its own session, can outlive the stop, and the retry waits for it (F8). The proof (§4) covers the stop in every phase of apt.

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | No event data in an expression or a `run:` line; `BROWSER` comes from the matrix. `install-deps` already ran its apt commands as root through `sudo`; now `npm exec` and Playwright's CLI run as root too, from the lockfile-pinned `e2e/node_modules` that `npm ci` installed in the step before (the same code the job runs anyway, on a throwaway runner). `env "PATH=$PATH"` only keeps the runner's own `PATH`. No new action, no new permission. Checked that the root run leaves no root-owned file in the workspace or `$HOME` (proof run 42726684084), so later steps as `runner` are unaffected |
| Correctness | `status` is set per attempt (`|| status=$?` keeps `bash -e` from ending the loop); exit 0 leaves the step at once; after attempt 2 the step fails with exit 1, so a mirror that is down still shows as a failed step, not as a later browser error. If `dpkg --configure -a` itself fails, `bash -e` fails the step with its message — a broken package state is a real failure. The browser cache is saved before the apt step, as before (#105) |
| Edge cases | A stop during `apt-get update` or the download ends `apt-get` and frees its locks (F7); a stop during the install leaves `dpkg` running until it finishes, then `dpkg --configure -a` repairs what the stop interrupted (F8). Partial downloads resume when the mirror supports ranges, otherwise they are fetched again. The `pgrep` wait has no limit of its own; the step's 8 minutes bound it |
| Performance | Normal runs unchanged (one attempt, median 17 s). Cache-miss runs now run `install-deps` as its own step — the same apt work `--with-deps` did |
| Maintainability | One loop of nine lines, a comment with the reason (including why `timeout` runs as root); actionlint (with shellcheck) clean |
| a11y, N+1 | Not touched |

**Findings sorted (CLAUDE §6.8):** fix — (1) the retry did not work at all (F6, F7): found by the proof, not by the review; `timeout` now runs as root; (2) a `dpkg` that outlives the stop could fail `dpkg --configure -a` (F8): wait for it; (3) the comment said "more than 10 minutes on a third of the runs", which overstated it (a third of the runs had *one* job hit the limit); reworded. Won't fix — none. Follow-up — §5.

## 4. Checks

__CHECKS__

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | Runner failures outside the workflow (F4): `abandoned` jobs (42674440407, merge of #219) turn the required check red although no test failed; once the API showed a finished job as `queued` for hours | Re-run the job; no workflow change can prevent a lost runner. If it happens more than once per sprint, the aggregate check could name `abandoned` in its message so it is not mistaken for a test failure — owner's call, no issue proposed yet |
| P2 | If `main` still goes red in the apt step after this change (both attempts slow), O-S4-1 option (1) is next: run the Playwright test step in the pinned `mcr.microsoft.com/playwright` image (`docker run --network host` next to the stack, digest pin, licence check) | Watch the runs on `main` until the sprint-04 close-out; count apt failures and `::warning::` retries there |
| P3 | GitHub reported "2 vulnerabilities on the default branch (2 moderate)" on the push of the proof branch (Dependabot alerts) | Belongs to S4-06 (Dependabot review), not to this task |

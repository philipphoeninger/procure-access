# S3-03 — CodeQL advanced setup

> **Sprint:** 03, P0 task S3-03 (debt, 0.5 points). **Issues:** #144. **Requirements / invariants:** none (CI, TESTING §8.2 stage 9); no invariant activated. New `TESTING_v24.md` (V85) for the stage-9 text only. **Status:** done by Claude Code (2026-11-05); one pull request. The Dependabot proof (§4 check 6) follows after the merge in a small docs PR.
> **Scope:** exactly the sprint row: CodeQL moves from GitHub's default setup to a workflow file that also runs on Dependabot pull requests, with the same three languages; actions pinned by SHA like `ci.yml`. The ruleset is the owner's.
> **Owner decisions** (asked before the start, 2026-11-05):
> 1. **Order (1A):** Claude Code switches default setup off through the API as soon as the pull request is open, so the PR shows the `Analyze` jobs running. Reason: GitHub rejects results of a workflow while default setup is on.
> 2. **Dependabot proof after the merge:** a Dependabot PR picks up the workflow only once it is on `main`. After the merge, `@dependabot rebase` on #193; Claude Code adds the result to §4 in a follow-up docs PR; then the owner adds the three checks to the ruleset.
> 3. **Licence (D12):** GitHub-hosted CI services (Actions, CodeQL, Dependabot) count as services of the hosting platform, not as dependencies. Wording for ARCHITECTURE v16 in §5 P3; it goes in with the documentation session S3-05.
> 4. **TESTING:** the stage-9 text goes into a new TESTING version in this PR (option 1), as #144 "Done when" asks.
>
> The sprint row is taken as the owner's OK for the new workflow (#144: "a new workflow needs the owner's OK").

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | CodeQL runs as default setup (`gh api …/code-scanning/default-setup`): `state: configured`, languages `actions`, `csharp`, `javascript-typescript` (the API also lists the aliases `javascript`, `typescript`), query suite `default`, threat model `remote`, weekly. No workflow file | The workflow takes over exactly these three languages and the `default` suite; threat model `remote` is the default of the action |
| F2 | Default setup did not start its `Analyze` jobs on Dependabot PRs (#129, #131, #138, S2-04), so the owner removed the three checks from the ruleset (2026-10-30). The ruleset "Main protection" (active, no bypass) now requires `1. Lint & licences`, `2. Build`, `3. Unit (.NET)`, `3. Unit (Angular)`, `4. Architecture`, `5. Integration`, `6. E2E + a11y`, `7. Invariant gate` | After the merge the owner adds `Analyze (actions)`, `Analyze (csharp)`, `Analyze (javascript-typescript)` back |
| F3 | The analyses of default setup carry the categories `/language:actions`, `/language:csharp`, `/language:javascript-typescript` (`…/code-scanning/analyses`); alert 3 (`js/prototype-pollution-utility`, `scripts/check-i18n.mjs`) is open under `/language:javascript-typescript` | The workflow sets the same categories, so the alerts continue on the same key: no duplicate, no stale analysis. Alert 3 stays open until S3-04 |
| F4 | GitHub docs ("Troubleshooting Dependabot on GitHub Actions"): workflows triggered by Dependabot get a read-only `GITHUB_TOKEN`; "you can use the `permissions` key in your workflow to increase the access for the token". The page does not name `security-events` explicitly (#144 step 3) | The job sets `security-events: write`; the Dependabot run on #193 (§4 check 6) is the proof. A `push` to a `dependabot/**` branch would also run with a read-only token, so `push` is limited to `main` |
| F5 | Licences (CLAUDE §10, development tools): `github/codeql-action` MIT, the query packs `github/codeql` MIT; the CodeQL CLI that the action downloads runs under GitHub's CodeQL terms (free for open-source code, not an OSI licence). Default setup ran the same CLI | Owner decision 3: a service of the hosting platform, not a dependency (§5 P3) |
| F6 | `.github/dependabot.yml` already updates `github-actions` weekly in one group | The new SHA pins get Dependabot updates like those of `ci.yml`; nothing to add |
| F7 | The ruleset requires status checks only; it has no "require code scanning results" rule. The separate `CodeQL` check (results against the repository's failure threshold) is not required either. TESTING v23 §8.2 said "CodeQL high findings: yes" | A finding does not block a merge today, before and after this task. Out of the row's scope → §5 P1 |

## 2. Changes

| File | Change |
|---|---|
| `.github/workflows/codeql.yml` (new) | Advanced setup: `pull_request` and `push` to `main`, weekly `schedule` (Monday 04:27 UTC); no path filter, so a required check never waits for a run that does not come. Workflow token `contents: read`, the job adds `security-events: write`. Matrix `language: [actions, csharp, javascript-typescript]`, job name `Analyze (${{ matrix.language }})` = the former check names. `actions/checkout` v7.0.1 (same SHA as `ci.yml`, `persist-credentials: false`), `github/codeql-action/init` and `/analyze` v4.39.3 (`33d7b85…`), `build-mode: none` (as default setup; no .NET or Node set-up needed), no `queries` input (= the default suite), `category: /language:<language>` (F3). Concurrency as in `ci.yml`: only superseded PR runs are cancelled. No `run:` step, no event data in an expression |
| `.github/workflows/ci.yml` | Header comment only: stage 9 is `codeql.yml` and `dependabot.yml`, no longer "the repository settings" |
| `docs/TESTING_v24.md` (new) | V85: §8.2 stage 9 names the workflow and its jobs, blocking "`Analyze (…)` jobs: yes; findings: not yet"; the ruleset note says stages 1–7 plus the three `Analyze` jobs and that a finding does not block yet (P1). Otherwise identical to v23 |
| `CLAUDE.md` | §3: CodeQL workflow, default setup off, job names are required checks; §4: `codeql.yml` next to `ci.yml`; §5: CI paragraph names `codeql.yml`; version line |
| `docs/sprints/sprint_03/S3-03_codeql_advanced.md` (new) | This protocol |

Repository setting (not in the diff): default setup switched off with `gh api -X PATCH repos/philipphoeninger/procure-access/code-scanning/default-setup -f state=not-configured` right after the PR was opened (owner decision 1).

Not changed on purpose: `TECH_DEBT_v2.md` still lists #144 (v3 at the end of the sprint moves it to "Done"); ARCHITECTURE (D12 wording with S3-05, P3); the ruleset (owner).

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security of the workflow: no `run:` step, no untrusted event data (titles, branch names) in an expression; `pull_request` (not `pull_request_target`), so a PR from a fork runs with a read-only token and no secrets; checkout without persisted credentials; least privilege (`contents: read` + `security-events: write` on the job) | – |
| R2 | C# with `build-mode: none` sees no generated code and resolves types without a build, so some data-flow results can be missing compared with a manual build | Won't fix: same mode as default setup (F1), no regression; a manual build would add the .NET set-up and about two minutes per run. Can be revisited with the first real C# finding |
| R3 | The `CodeQL` check of a PR stays neutral or red when an analysis is missing (e.g. a run cancelled by a newer push) | Won't fix: it is not a required check; the required `Analyze` jobs of the newest commit always run |
| R4 | GitHub turns off scheduled workflows after 60 days without repository activity | Won't fix: PR and push runs keep the scans current while the project is active; a dormant repository gets the warning e-mail |
| R5 | The workflow and `ci.yml` pin the same `actions/checkout` SHA; Dependabot's `actions` group updates both in one PR (F6) | – |
| R6 | The first draft set `queries: default`. That input takes extra suites (`security-extended`, `security-and-quality`, `security-experimental`) or query paths; `default` is none of them and would be read as a path | Fixed: input removed, the action then runs the default suite |
| R7 | Correctness, N+1, a11y, data: no application code changed | – |

No correctness or security bug that affects current behaviour was found in the diff.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `actionlint` 1.7.12 (Docker image `rhysd/actionlint`, run once, not added to the repository) on `codeql.yml` and `ci.yml` | ✅ no finding (incl. shellcheck of `ci.yml`) |
| 2 | `node scripts/check-invariants.mjs` | ✅ gate passed, reads `docs/TESTING_v24.md` |
| 3 | `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | ✅ Figma 2 checked, 24 not built, 0 pending; i18n 0 errors (6 keys computed at runtime, as before) |
| 4 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 684 / 684 files (with this protocol) |
| 5 | `Analyze` on a normal PR: this pull request (#195), run 42068998755 after default setup was switched off | ✅ `Analyze (actions)`, `Analyze (csharp)`, `Analyze (javascript-typescript)` passed (39 s, 2 min 10 s, 47 s); three analyses uploaded for `refs/pull/195/merge` with the categories `/language:actions`, `/language:csharp`, `/language:javascript-typescript`, 0 new results, no upload error; the `CodeQL` check passed; alert 3 is still the only open alert (same key, no duplicate). CI stages 1–8 green |
| 6 | `Analyze` on a Dependabot PR: #193 after `@dependabot rebase` | follows after the merge (owner decision 2) |
| 7 | Build and test levels | – not touched (no application code); CI stages 2–6 green on #195 |

## 5. Open points

| # | Point | Who, when |
|---|---|---|
| P1 | **A CodeQL finding does not block a merge** (F7): the `Analyze` jobs pass also when they find something, and the ruleset has no code-scanning rule. TESTING v23 claimed "CodeQL high findings: yes". Options: **(a, recommended)** add the ruleset rule "Require code scanning results" (tool CodeQL, alerts "Errors", security alerts "High or higher"), which matches the old claim; (b) leave it and keep TESTING v24's "findings: not yet". Alert 3 (S3-04) is not a new alert of a PR, so it would not block other PRs | Owner, with the ruleset change after the merge |
| P2 | Dependabot proof (§4 check 6) and the ruleset: after the merge, `@dependabot rebase` on #193 (the earlier rebase came before the workflow was on `main`, so Dependabot answered "already up to date"); Claude Code records the run in a docs PR; then the owner adds `Analyze (actions)`, `Analyze (csharp)`, `Analyze (javascript-typescript)` to "Main protection" | Owner and Claude Code, after the merge |
| P3 | D12 wording for ARCHITECTURE v16 (owner decision 3), proposal: "**Services of the hosting platform** (GitHub Actions runners, CodeQL code scanning, Dependabot) are not dependencies: they are neither shipped nor kept in the repository, so the rule above does not apply to them. Actions that a workflow references (`uses:`) are development tools and follow the rule (e.g. `github/codeql-action`, MIT)." | S3-05 (documentation session) |
| P4 | `TECH_DEBT_v2.md` keeps #144 open; v3 at the end of the sprint moves it to "Done" together with TD-01's note | Sprint close-out |

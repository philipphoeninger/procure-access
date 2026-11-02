# Sprint 02 — task prompts for Claude Code

> **Status:** v2 (2026-10-29). Companion to `SPRINT_02.md`: one ready-to-paste prompt per task. Claude Code implements; the owner's control chat for sprint 02 takes the decisions and reviews the reports. v1 (`TASK_PROMPT.md`) held only the generic template; it is kept at the end of this file for later sprints.
> **How to use:** copy the block of the task into a new Claude Code session. Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>` or `<Name>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_02.md`, `TECH_DEBT_v1.md` and the GitHub issues as of 2026-10-29.

---

## Overview

| Task | Title | Issues | Can start |
|---|---|---|---|
| S2-01 | Live bugs: product filter 500, delete-account dialog, dev health watcher | #37, #95, #96, #113 | ready |
| S2-02 | Logs: ApplicationName is always Unknown | #121 | ready |
| S2-03 | Self-hosted icon fonts, Font Awesome removed | #43, #78 | after: O-S2-5 |
| S2-04 | Review Dependabot pull requests #129 – #131 | PR #129 (e2e group), PR #130 (client 'other' group), PR #131 (marked 17.0.6 → 18.1.1) | ready |
| S2-05 | NetArchTest spike on .NET 10 | #117 | ready |
| S2-06 | Module projects: design note and skeleton (M1.1) | none | after: S2-05 reported; O-S2-1 |
| S2-07 | Move the platform code built ahead into the Platform module | none (closes S1-23 open point P6) | after: S2-06 merged |
| S2-08 | Architecture tests ARC-01 – ARC-03 | none | after: S2-05 result; S2-06 merged; no other open branch that creates a TESTING version |
| S2-09 | Invariant gate tightening | #109, #111 | ready (if it changes TESTING §8.3: no other open branch that creates a TESTING version) |
| S2-10 | Problem Details middleware | none (removes the ad-hoc error objects of TECH_DEBT_v1 §5) | after: S2-07 merged; no other open branch that creates a TESTING version |
| S2-11 | Client mapping of Problem Details and field errors | none | after: S2-10 merged; no other open branch that creates a TESTING version |
| S2-12 | Pluralisation approach (UX-O9) | none | ready (run 1); run 2 after O-S2-3 |
| S2-13 | de.json from the UX copy | #101 | after: S2-12 merged; O-S2-3 |
| S2-14 | TypeScript lint rules | #119 | ready (must be merged before S2-15) |
| S2-15 | Shell frame: landmarks, skip link, anonymous header, footer | #120 | after: S2-13 and S2-14 merged; O-S2-4 |
| S2-16 | Route change: page title, H1 focus, announcement, lang | none | after: S2-15 merged; no other open branch that creates a TESTING version |
| S2-17 | Start page, error pages, loading/empty/system states | none | after: S2-15 merged |
| S2-18 | 320 px / 400 % zoom check and E2E on the new shell | none | after: S2-15 – S2-17 merged |
| S2-19 | E2E readiness on /health/ready, remove /api/Health | #122 | stretch, after S2-15 |
| S2-20 | CI one-liners | #104, #105, #106, #107 | stretch, ready |
| S2-21 | API Dockerfile hardening and Swagger clean-up | #77, #74, #73 | stretch, ready |
| S2-22 | Figma link check improvements | #110 | stretch, after S2-15 |
| S2-23 | Client coverage exclusions | #97 | stretch, ready |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S2-1 (scope of the module move) | S2-06 run 2, S2-07 | the design note of S2-06 run 1 |
| O-S2-2 (only if the spike fails) | S2-08 | the result of S2-05 |
| O-S2-3 (pluralisation) | S2-12 run 2, S2-13 | the options of S2-12 run 1 |
| Legacy en/es/fr.json | S2-13 | owner choice (any time) |
| O-S2-4 (wording for #120) | S2-15 | the next UX copy version |
| O-S2-5 (icon set) | S2-03 | owner choice (any time) |

## Rules for running tasks

- **One new TESTING version at a time.** S2-08, S2-10, S2-11 and S2-16 flip invariants to `active` (S2-09 may change TESTING §8.3). If two of them run on parallel branches, both would create the same `TESTING_v16.md`. Start such a task only after the previous one is merged.
- **Order:** as in `SPRINT_02.md` (Sequence). Backend (S2-06 – S2-11) and frontend (S2-12 – S2-18) may alternate when one side waits.
- **Stretch tasks** (S2-19 – S2-23) only after all P0 tasks are done.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_02.md` and `TECH_DEBT_v2.md` (process step 14), prepared in the control chat.

---

## P0 — committed

### S2-01 — Live bugs: product filter 500, delete-account dialog, dev health watcher

```text
Task S2-01 from docs/sprints/sprint_02/SPRINT_02.md — Live bugs: product filter 500, delete-account dialog, dev health watcher
Issues: #37, #95, #96, #113
Requirements / invariants: none (bug fixes)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 37, gh issue view 95, gh issue view 96, gh issue view 113) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-08_angular21_upgrade.md (finding N1: the spinner that stays after the 500)

Owner decisions for this task: none
Additional instructions: One PR for the three bugs, one commit per issue. Every bug gets a regression test: an integration test for #37 (ProductsController.ByCriteriaFilterIds), the missing cases of #96 in delete-account-dialog.spec.ts for #95, and a check for #113 that the dev configuration points at the local API.

Way of working:
1. Branch sprint02/S2-01-live-bugs from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-01_live_bugs.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-02 — Logs: ApplicationName is always Unknown

```text
Task S2-02 from docs/sprints/sprint_02/SPRINT_02.md — Logs: ApplicationName is always Unknown
Issues: #121
Requirements / invariants: OPS-02 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 121) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-22_operations_basics.md (open point P5)

Owner decisions for this task: none
Additional instructions: Fix the mismatch between the enricher (ApplicationName) and appsettings (AppName) in one place; do not rename other settings. Cover it with an existing or a new logging test.

Way of working:
1. Branch sprint02/S2-02-log-application-name from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-02_log_application_name.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-03 — Self-hosted icon fonts, Font Awesome removed

```text
Task S2-03 from docs/sprints/sprint_02/SPRINT_02.md — Self-hosted icon fonts, Font Awesome removed
Issues: #43, #78
Requirements / invariants: A11Y-01 stays green; CLAUDE §8 (no third-party requests from the browser)
Depends on: O-S2-5

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 43, gh issue view 78) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 D12 (licence rule, C89); design_handoff_procureaccess_v1.md (icons); LICENSES/ and REUSE.toml

Owner decisions for this task: O-S2-5: <icon set replacing Font Awesome — fill in once decided (proposal: Material Symbols, Apache-2.0, self-hosted)>
Additional instructions: Prove the result with the network log of J1: no request to any third-party origin. Remove every Font Awesome package and asset; update LICENSES/ and REUSE.toml for the self-hosted font files.

Way of working:
1. Branch sprint02/S2-03-self-hosted-icons from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-03_self_hosted_icons.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-04 — Review Dependabot pull requests #129 – #131

```text
Task S2-04 from docs/sprints/sprint_02/SPRINT_02.md — Review Dependabot pull requests #129 – #131
Issues: PR #129 (e2e group), PR #130 (client 'other' group), PR #131 (marked 17.0.6 → 18.1.1)
Requirements / invariants: CLAUDE §10 (licences)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the three pull requests (gh pr view 129, gh pr view 130, gh pr view 131); always the highest version of each document in docs/.
Also read: the three pull requests (gh pr view / gh pr diff), their CI runs, and where `marked` is used in the client

Owner decisions for this task: none
Additional instructions: This task reviews existing pull requests instead of building something: steps 1 and 3 of the way of working do not apply. For every PR check the licence of each new version (CLAUDE §10), the changelog for breaking changes (marked is a major bump), and the CI result. The protocol recommends merge or close per PR with the reason; open a small PR that contains only the protocol. Do not merge any of the Dependabot PRs.

Way of working:
1. Branch sprint02/S2-04-dependabot-prs from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-04_dependabot_prs.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-05 — NetArchTest spike on .NET 10

```text
Task S2-05 from docs/sprints/sprint_02/SPRINT_02.md — NetArchTest spike on .NET 10
Issues: #117
Requirements / invariants: prepares ARC-01 – ARC-03 (they stay planned here)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 117) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-12_test_projects.md (open point P1); TESTING_v15 §4.10 (ARC-01 – ARC-03)

Owner decisions for this task: none
Additional instructions: Spike only, no production change. Run one real NetArchTest rule against the .NET 10 assemblies of the solution (for example a dependency rule between two existing projects) and record whether it loads and evaluates correctly. Throw the spike code away; the PR contains only the protocol. Report the result and stop: it decides how S2-08 is built.

Way of working:
1. Branch sprint02/S2-05-netarchtest-spike from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-05_netarchtest_spike.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-06 — Module projects: design note and skeleton (M1.1)

```text
Task S2-06 from docs/sprints/sprint_02/SPRINT_02.md — Module projects: design note and skeleton (M1.1)
Issues: none
Requirements / invariants: prepares ARC-01 – ARC-03
Depends on: S2-05 reported; O-S2-1

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 §4.3 (modules, contracts, dependency rules) and §12 step 1; CLAUDE.md §4 (target layout); TECH_DEBT_v1 §5 and §9.2

Owner decisions for this task: O-S2-1: <scope of the module move — fill in once decided (proposal: create the module and contracts projects and move only the platform code; API, DAL, MODELS, SERVICES stay legacy until the step that replaces them)>
Additional instructions: Run 1 (O-S2-1 not yet decided): write only the design note — how today's projects API, DAL, MODELS and SERVICES map onto the target layout, what moves now, what stays legacy until which migration step, with a recommendation — as the draft of the protocol, and stop. Run 2 (with O-S2-1): build the skeleton per the SPRINT_02 row (src/ProcureAccess.Modules.<Name> and .Contracts for the eight modules, references exactly per ARCHITECTURE §4.3, Add<Name>Module called from Program.cs, SPDX headers), update CLAUDE.md §4 and smoke-test that the app behaves as before.

Way of working:
1. Branch sprint02/S2-06-module-skeleton from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-06_module_skeleton.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-07 — Move the platform code built ahead into the Platform module

```text
Task S2-07 from docs/sprints/sprint_02/SPRINT_02.md — Move the platform code built ahead into the Platform module
Issues: none (closes S1-23 open point P6)
Requirements / invariants: OPS-01, OPS-02, PRV-05, SEC-06 stay green
Depends on: S2-06 merged

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-22_operations_basics.md, docs/sprints/sprint_01/S1-23_usage_event_table.md; ARCHITECTURE_v15 §4.3 (Platform)

Owner decisions for this task: O-S2-1: <copy the decision from S2-06>
Additional instructions: Move the Serilog set-up and LogRetentionJob, the health checks, IUsageEventWriter with its entity, and the TimeProvider registration into Platform. Only controllers and the registration extension stay public outside Contracts. No behaviour change: the four invariants above are the proof.

Way of working:
1. Branch sprint02/S2-07-platform-module from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-07_platform_module.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-08 — Architecture tests ARC-01 – ARC-03

```text
Task S2-08 from docs/sprints/sprint_02/SPRINT_02.md — Architecture tests ARC-01 – ARC-03
Issues: none
Requirements / invariants: ARC-01, ARC-02, ARC-03 → active
Depends on: S2-05 result; S2-06 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 §4.3 (one rule per row of the table); TESTING_v15 §4.10, §8.1; .github/workflows/ci.yml (architecture stage)

Owner decisions for this task: S2-05 result: <NetArchTest works on .NET 10 | failed → O-S2-2: <tool decided>>
Additional instructions: One rule per row of the module table, as project-reference checks plus NetArchTest (or the tool of O-S2-2). Show in the protocol that each rule fails on a deliberate wrong reference and passes without it. Confirm the architecture test project still runs without --ignore-exit-code 8 (removed in S1-23). Flip ARC-01 – ARC-03 in the next TESTING version.

Way of working:
1. Branch sprint02/S2-08-architecture-rules from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-08_architecture_rules.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-09 — Invariant gate tightening

```text
Task S2-09 from docs/sprints/sprint_02/SPRINT_02.md — Invariant gate tightening
Issues: #109, #111
Requirements / invariants: the gate of TESTING §8.3 (no invariant changes status)
Depends on: nothing; no other open branch that creates a TESTING version if §8.3 text changes

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 109, gh issue view 111) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-21_invariant_gate.md (open point P3); TESTING_v15 §8.1, §8.3, V72; scripts/check-invariants.mjs

Owner decisions for this task: none
Additional instructions: Implement both issues in scripts/check-invariants.mjs: fail on catalogue header drift, count only test files, count only tests that run, require the .NET Invariant trait, and require a protocol file. Protocols now live in docs/sprints/sprint_NN/. Show in the protocol that the gate fails on each new case in a scratch branch and passes on main. If TESTING §8.3 or V72 need a text change, put it in the next TESTING version.

Way of working:
1. Branch sprint02/S2-09-gate-tightening from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-09_gate_tightening.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-10 — Problem Details middleware

```text
Task S2-10 from docs/sprints/sprint_02/SPRINT_02.md — Problem Details middleware
Issues: none (removes the ad-hoc error objects of TECH_DEBT_v1 §5)
Requirements / invariants: API-01 → active; R-… per SPEC §7.1, §8.6
Depends on: S2-07 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 §7.1, §8.6 (problem types); ARCHITECTURE_v15 §7.1, C78; BaseCrudController and UICustomizationController

Owner decisions for this task: none
Additional instructions: RFC 9457 Problem Details in Platform with the field-keyed errors map and the problem types of SPEC §8.6 under a configurable base URI. Replace the ad-hoc error objects in BaseCrudController and UICustomizationController. API-01 as one generic test over all write endpoints with an invalid payload; flip it in the next TESTING version.

Way of working:
1. Branch sprint02/S2-10-problem-details from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-10_problem_details.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-11 — Client mapping of Problem Details and field errors

```text
Task S2-11 from docs/sprints/sprint_02/SPRINT_02.md — Client mapping of Problem Details and field errors
Issues: none
Requirements / invariants: API-02 → active
Depends on: S2-10 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 §8.6; design_handoff_procureaccess_v1.md §6 – §7 (error summary, field errors); ux_copy_procureaccess_v14.md (error keys); CLAUDE.md §9

Owner decisions for this task: none
Additional instructions: Map problem types to UX keys; bind server field errors to the matching form control with aria-describedby and announce them through A11yAnnouncerService. Propose one existing form as the example in the plan. Component test per the CLAUDE §9 convention; flip API-02 in the next TESTING version.

Way of working:
1. Branch sprint02/S2-11-client-error-mapping from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-11_client_error_mapping.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-12 — Pluralisation approach (UX-O9)

```text
Task S2-12 from docs/sprints/sprint_02/SPRINT_02.md — Pluralisation approach (UX-O9)
Issues: none
Requirements / invariants: none
Depends on: nothing for run 1; O-S2-3 for run 2

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 §13 (UX-O9); ux_copy_procureaccess_v14.md §8; CLIENT package.json (ngx-translate 18)

Owner decisions for this task: O-S2-3: <pluralisation approach — fill in for run 2>
Additional instructions: Run 1: options only, then stop — for each option the licence (CLAUDE §10), bundle size, compatibility with ngx-translate 18 and zoneless Angular 22, and how the UX copy writes plural forms; with a recommendation. Run 2 (with O-S2-3): set up the chosen approach with one example key that renders singular and plural, and record the decision in SPEC §13 or the next SPEC version.

Way of working:
1. Branch sprint02/S2-12-pluralisation from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-12_pluralisation.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-13 — de.json from the UX copy

```text
Task S2-13 from docs/sprints/sprint_02/SPRINT_02.md — de.json from the UX copy
Issues: #101
Requirements / invariants: CLAUDE §6.5 (UI strings only by key)
Depends on: S2-12 merged; O-S2-3

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 101) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: ux_copy_procureaccess_v14.md §5 (keys) and §8; CLIENT src/assets/i18n/

Owner decisions for this task: O-S2-3: <copy the decision from S2-12>; legacy en.json / es.json / fr.json: <keep untouched | remove until the English UI texts come>
Additional instructions: Build de.json from the UX copy for all keys of §5 (shell, errors, common), by script or by a checked conversion, plus a check that every key used in templates exists. Resolve #101 (shell.nav.menu vs shell.nav.menu.close) together with the UX copy: propose the key change, do not invent wording. List every key or wording the UX copy lacks and stop on gaps the shell needs.

Way of working:
1. Branch sprint02/S2-13-de-json from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-13_de_json.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-14 — TypeScript lint rules

```text
Task S2-14 from docs/sprints/sprint_02/SPRINT_02.md — TypeScript lint rules
Issues: #119
Requirements / invariants: none (CI stage 1)
Depends on: nothing; must be merged before S2-15

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 119) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-20_ci.md; CLIENT eslint configuration

Owner decisions for this task: none
Additional instructions: Turn on typescript-eslint recommended for .ts files. Fix trivial findings; for legacy code that M1 – M3 replaces, disable rules per file with an issue link. No rule may be disabled for new paths.

Way of working:
1. Branch sprint02/S2-14-typescript-lint from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-14_typescript_lint.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-15 — Shell frame: landmarks, skip link, anonymous header, footer

```text
Task S2-15 from docs/sprints/sprint_02/SPRINT_02.md — Shell frame: landmarks, skip link, anonymous header, footer
Issues: #120
Requirements / invariants: R-SHL-01, R-SHL-02 (Anonymous state and compact layout), R-SHL-06; A11Y-01 stays green
Depends on: S2-13 and S2-14 merged; O-S2-4

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 120) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: SPEC_v2 R-SHL-01, -02, -06; design_handoff_procureaccess_v1.md §2.1, §3.1, §7; ux_copy_procureaccess_v14.md (shell.*); figma_code_mapping_procureaccess_v1.md; docs/sprints/sprint_01/S1-21_invariant_gate.md (open point P4: header on the PENDING list)

Owner decisions for this task: O-S2-4: <German wording for the header icon buttons and COMMON.ENTER (#120) — fill in once the UX copy has it>
Additional instructions: Header only in the Anonymous state (SignedIn and SignedInNoOrg come with M3); legal routes as placeholders until GP.3. The legacy feature screens must keep working inside the new shell. New components carry @figma tags; take the legacy header off the PENDING list of check-figma-links.mjs.

Way of working:
1. Branch sprint02/S2-15-shell-frame from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-15_shell_frame.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-16 — Route change: page title, H1 focus, announcement, lang

```text
Task S2-16 from docs/sprints/sprint_02/SPRINT_02.md — Route change: page title, H1 focus, announcement, lang
Issues: none
Requirements / invariants: R-SHL-04; A11Y-03, A11Y-05 → active
Depends on: S2-15 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-SHL-04; ux_copy_procureaccess_v14.md §5.16 (pageTitle.*); TESTING_v15 §4.11; CLAUDE.md §9

Owner decisions for this task: none
Additional instructions: Page title most-specific-first via shell.pageTitle, focus on the H1 and announcement through A11yAnnouncerService on every route change; dialogs do not change the title; document lang follows the UI language. Vitest and E2E tests; flip A11Y-03 and A11Y-05 in the next TESTING version.

Way of working:
1. Branch sprint02/S2-16-route-change from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-16_route_change.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-17 — Start page, error pages, loading/empty/system states

```text
Task S2-17 from docs/sprints/sprint_02/SPRINT_02.md — Start page, error pages, loading/empty/system states
Issues: none
Requirements / invariants: R-SHL-05, R-SHL-08, R-SHL-09; A11Y-01 stays green
Depends on: S2-15 merged

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-SHL-05, -08, -09; design_handoff_procureaccess_v1.md §5.3, §6.2 – §6.5; ux_copy_procureaccess_v14.md (home.*, errors.*, common.*)

Owner decisions for this task: none
Additional instructions: Start page S04, error pages S80 for 404/403/500 and the offline notice, loading/empty/system states (mat-progress-bar with one common.loading announcement, no skeleton screens). The TEN-03 part of R-SHL-08 (foreign resources → 404) comes with step 6 and is not in scope. Component tests by role and name; axe on every page.

Way of working:
1. Branch sprint02/S2-17-start-and-error-pages from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-17_start_and_error_pages.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-18 — 320 px / 400 % zoom check and E2E on the new shell

```text
Task S2-18 from docs/sprints/sprint_02/SPRINT_02.md — 320 px / 400 % zoom check and E2E on the new shell
Issues: none
Requirements / invariants: R-SHL-10; A11Y-01, CAT-10 (E2E part) stay green
Depends on: S2-15 – S2-17 merged

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-SHL-10; TESTING_v15 §7.2 (manual check); e2e/journeys/j1-configurator.spec.ts

Owner decisions for this task: none
Additional instructions: Short protocol of the 320 px / 400 % zoom check of every shell page. Adjust J1 and the CAT-10 E2E part to the new shell: locators only, never the expected sets. J1 green on all five browser projects (WebKit and mobile Safari in CI).

Way of working:
1. Branch sprint02/S2-18-zoom-and-e2e from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-18_zoom_and_e2e.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

## P2 — stretch

### S2-19 — E2E readiness on /health/ready, remove /api/Health *(stretch)*

```text
Task S2-19 from docs/sprints/sprint_02/SPRINT_02.md — E2E readiness on /health/ready, remove /api/Health
Issues: #122
Requirements / invariants: OPS-01 stays green
Depends on: S2-15 merged (stretch: only after all P0 tasks)

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 122) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-22_operations_basics.md (open point P2); e2e/global-setup.ts; docker-compose.e2e.yml

Owner decisions for this task: none
Additional instructions: Stretch task. Point the E2E healthcheck, global-setup.ts and ClockTests at /health/ready, then remove the legacy /api/Health endpoint and its controller.

Way of working:
1. Branch sprint02/S2-19-health-ready-for-e2e from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-19_health_ready_for_e2e.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-20 — CI one-liners *(stretch)*

```text
Task S2-20 from docs/sprints/sprint_02/SPRINT_02.md — CI one-liners
Issues: #104, #105, #106, #107
Requirements / invariants: none (CI)
Depends on: nothing (stretch: only after all P0 tasks)

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 104, gh issue view 105, gh issue view 106, gh issue view 107) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_01/S1-20_ci.md; .github/workflows/ci.yml

Owner decisions for this task: none
Additional instructions: Stretch task. One commit per issue in one PR; show the effect of each change in a CI run.

Way of working:
1. Branch sprint02/S2-20-ci-one-liners from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-20_ci_one_liners.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-21 — API Dockerfile hardening and Swagger clean-up *(stretch)*

```text
Task S2-21 from docs/sprints/sprint_02/SPRINT_02.md — API Dockerfile hardening and Swagger clean-up
Issues: #77, #74, #73
Requirements / invariants: SEC-06 stays green
Depends on: nothing (stretch: only after all P0 tasks)

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 77, gh issue view 74, gh issue view 73) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: API Dockerfile; docker-compose.yml; docs/sprints/sprint_01/S1-06_docker_smoke_test.md

Owner decisions for this task: none
Additional instructions: Stretch task. Release build, non-root user and restore caching in the API image (#77); untrack the generated API.ProcureAccess.xml and drop the unused Microsoft.AspNetCore.OpenApi (#74); the AV0027 decision and SwaggerDefaultValues hardening (#73) — if #73 needs a decision, present the options and stop. Smoke-test the compose stack.

Way of working:
1. Branch sprint02/S2-21-api-dockerfile-swagger from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-21_api_dockerfile_swagger.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-22 — Figma link check improvements *(stretch)*

```text
Task S2-22 from docs/sprints/sprint_02/SPRINT_02.md — Figma link check improvements
Issues: #110
Requirements / invariants: none (CI stage 1)
Depends on: S2-15 merged (stretch: only after all P0 tasks)

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 110) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: scripts/check-figma-links.mjs; figma_code_mapping_procureaccess_v1.md §4; docs/sprints/sprint_01/S1-21_invariant_gate.md (open point P4)

Owner decisions for this task: none
Additional instructions: Stretch task. Map pages, find moved components and validate the PENDING list, as described in #110.

Way of working:
1. Branch sprint02/S2-22-figma-link-check from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-22_figma_link_check.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S2-23 — Client coverage exclusions *(stretch)*

```text
Task S2-23 from docs/sprints/sprint_02/SPRINT_02.md — Client coverage exclusions
Issues: #97
Requirements / invariants: none (coverage is reported, not gated)
Depends on: nothing (stretch: only after all P0 tasks)

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 97) and their entries in TECH_DEBT_v1.md; always the highest version of each document in docs/.
Also read: TESTING_v15 §8.4; CLIENT test configuration

Owner decisions for this task: none
Additional instructions: Stretch task. Exclude models, DTOs and environment files from the client coverage report.

Way of working:
1. Branch sprint02/S2-23-coverage-exclusions from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-23_coverage_exclusions.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

```text
Task S2-<NN> from docs/sprints/sprint_02/SPRINT_02.md — <title of the row>

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-<NN> plus the sections Dependencies, Owner decisions, Risks and Definition of done; the documents and sections the row names (always the highest version in docs/); the issues named in the row (gh issue view <n>) and their entries in TECH_DEBT_v1.md; sprint-01 protocols in docs/sprints/sprint_01/ only where the row or an issue points to them.

Owner decisions for this task: <none | O-S2-n: <decision> (decided <date>)>
Additional instructions: <none | …>

Way of working:
1. Branch sprint02/S2-<NN>-<topic> from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-<NN>_<topic>.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

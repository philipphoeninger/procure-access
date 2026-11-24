# Sprint 07 — task prompts for Claude Code

> **Status:** v1 (2026-11-24). Companion to `SPRINT_07.md`: one ready-to-paste prompt per task, built from the generic template at the end of `docs/sprints/sprint_06/TASK_PROMPTS.md` (repeated, updated, at the end of this file). Claude Code implements; the owner's control chat for sprint 07 takes the decisions and reviews the reports. The model per task is the owner's choice and is not part of the prompts.
> **How to use:** copy the block of the task into a new Claude Code session (or a worktree session for parallel work). Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>`, `<Name>`, `<area>`, `<topic>` or `<date>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_07.md`, `TECH_DEBT_v6.md` and the GitHub issues and pull requests as of 2026-11-24.
> **Changes to the template since sprint 06:** reads `TECH_DEBT_v6.md`; step 5 names `TECHNICAL.md` for anything an operator must know; "Also read" names documents without their file version (`TECH_DEBT §…` means the highest version). The running order in waves and the extra prompts for parallel work are part of this file from the start. Decision IDs `O-S7-n` are this sprint's; O-S5 (dark mode) and O-25 – O-32 (ARCHITECTURE §16.5) keep their names.

---

## Overview

Prompts are listed in task-ID order; the working order is in "Running order (waves)" below.

| Task | Title | Issues | Can start |
|---|---|---|---|
| S7-01 | Name the causes of the E2E failures on main | #451, #452, #436 | ready (first) |
| S7-02 | Quick wins: evidence race, TimeSpan reader, legacy remnants, log sink, page title | #453, #456, #437 | ready (subagent batch) |
| S7-03 | Dependabot batch | PR #439, #440, #441, #442, #443, #444 | after O-S7-11 |
| S7-04 | Documentation session and the CODEBASE split | none (TD-100, TD-101) | last |
| S7-05 | Assessments in the personal data export | #414 | after O-S7-1 |
| S7-06 | Path back from S11 of a project copy | #429 | after O-S7-2 (subagent batch) |
| S7-07 | Assign or replace a project's list on S32 | #420 | after O-S7-3 and S7-06 |
| S7-08 | Workspace correctness: archived copy on the server, lock and copy atomic | none (TD-86, TD-87; ARCHITECTURE O-30, O-31) | after O-S7-4 |
| S7-09 | Deployment hardening: ports, public-address settings, health route | #449, #447, #448 | after O-S7-5 |
| S7-10 | Staging from CI and the first deployment | #236 | after S7-09 and O-S7-6 (with the owner) |
| S7-11 | Backup completion: empty-dump check, alert, off-host copy, staging rehearsal | #455 | after S7-09 and O-S7-7 |
| S7-12 | Security small items: role changes at once, another user in another tab | #450 | ready |
| S7-13 | S12 reloads after release-mismatch | #454 | after O-S7-8 (subagent batch) |
| S7-14 | Release pipeline with check-release-config | none (TD-29) | ready (subagent batch) |
| S7-15 | Licence notices for the base images | none (TD-44, TD-53) | ready (subagent batch) |
| S7-16 | Pre-pilot polish: one time zone, .contrast-more, content links | none (TD-47, TD-69, TD-67) | after O-S7-9 (subagent batch) |
| S7-17 | Guide for the manual accessibility pass | #311, #102 | ready |
| S7-18 | Fixes after the tester's pass | #311 and the issues the pass creates | gated: after the tester's pass |
| S7-19 | Dark theme release | #240 | gated: after O-S5 |
| S7-20 | Criterion splits: several successors | #238 | after O-S7-10 |
| S7-21 | Directory domain: products, published entries, immutable versions | none | after S7-01 |
| S7-22 | PublishedEntryFactory with redaction | none | after S7-21 |
| S7-23 | Product-identity resolver and cross-release matching | none | after S7-21 and S7-20 |
| S7-24 | Directory entries in the development and E2E seeder | none | after S7-22, S7-23 |
| S7-25 | Directory search: server | none | after S7-23 |
| S7-26 | Directory search S40 | #170 (L2, product list), #82 (Z1) | after S7-25, S7-24 |
| S7-27 | Read projections for the product page and the entry detail | none | after S7-22, S7-23, S7-25 |
| S7-28 | Product page S41 | #170 (L3), #82 (Z3, R5) | stretch, after S7-27, S7-26 |
| S7-29 | Entry detail S42 | none | stretch, after S7-28 |
| S7-30 | Release 2026.1 with the project team's priorities | #299 | on arrival (O-1/K-1) |
| S7-31 | Legacy removal after the activation of 2026.1 | #52, #158 | on arrival, after S7-30 |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S7-1 (shape of the assessments in the export) | S7-05 | owner choice (D1) |
| O-S7-2 (last path item on S11 of a copy) | S7-06 | owner choice (D1) |
| O-S7-3 (assign/replace dialog, O-28) | S7-07 | owner choice |
| O-S7-4 (O-30, O-31) | S7-08 | owner choice |
| O-S7-5 (ports, public-address values, health route) | S7-09 | owner choice (D1) |
| O-S7-6 (deployment trigger, staging reset) | S7-10 | owner choice |
| O-S7-7 (off-host copy, alert) | S7-11 | owner choice |
| O-S7-8 (UX key of the S12 reload) | S7-13 | owner choice |
| O-S7-9 (time zone, `.contrast-more`) | S7-16 | owner choice |
| O-S7-10 (criterion splits) | S7-20 | owner choice |
| O-S7-11 (up-to-date branches, Serilog 11) | S7-03 | owner choice |
| O-S7-12 (CODEBASE split) | S7-04 | owner choice (before the last task) |

O-S6-8 (2026.1 as a G-P criterion) needs no prompt.

## Rules for running tasks

- **S7-01 first:** it runs alone on the local E2E stack. Free disk space before parallel sessions (TD-74).
- **Directory line (one session, in order):** S7-21 → S7-22 → S7-23 → S7-25 → S7-27. The Directory module allows one open migration branch at a time; these tasks carry the two-signal and privacy invariants.
- **Deployment line (in order, same files):** S7-09 → S7-10 → S7-11. S7-10 and the staging rehearsal of S7-11 need the owner at the server.
- **Gated tasks:** S7-18 only after the tester's pass, S7-19 only after O-S5, S7-30 and S7-31 only after O-1/K-1. If their input does not arrive, they move to sprint 08.
- **EF migrations:** one open branch per context — Directory (S7-21, S7-23, S7-25, S7-27), Catalog (S7-20), Workspace/Configurator (S7-08).
- **Files in common:** `docker-compose.yml` and `TECHNICAL.md` (S7-09 → S7-10 → S7-11), the workflows (S7-10 before S7-14, or rebase), `package-lock.json` (S7-03 alone, one PR at a time), S32 and the breadcrumb (S7-06 before S7-07), the main navigation (S7-26 before S7-31).
- **TESTING versions:** almost every M6 task flips an invariant; the later PR rebases and renumbers.
- **Local E2E:** one session at a time (shared host ports); the others leave E2E to CI.
- **Stretch tasks** (S7-28, S7-29) only after all P0 tasks that are not gated.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_07.md` and `TECH_DEBT_v7.md` (process step 14), prepared in the control chat.

---

## Running order (waves)

At most three windows at a time: **A** is the attended line (first the workspace fix, then the Directory line) and the only window that runs the local E2E stack; **B** and **C** run beside it. Every task gets a new session in its own worktree (`claude --worktree s7-nn`, or `/clear` before the next task). A wave starts only when the PRs in the last column are merged. If B or C finishes early, it waits; never pull a task forward whose dependency is still open.

Before wave 1: free disk space (TD-74); the owner decisions O-S7-1 – O-S7-12 filled in above (wave 1 needs only O-S7-2 and O-S7-5).

| Wave | Window A (attended) | Window B | Window C | Next wave once merged |
|---|---|---|---|---|
| 1 | S7-01 (E2E causes) | Subagent batch S7-02, S7-06, S7-15 (prompt 2) | S7-09 (deployment hardening) | S7-01, S7-09, S7-06 |
| 2 | S7-08 (workspace correctness) | S7-05, then S7-07 | S7-10 (staging from CI, **with the owner at the server**) | S7-08, S7-10 |
| 3 | S7-21 (directory domain) | S7-12, then S7-03 | S7-11 (backup completion, staging rehearsal with the owner) | S7-21 |
| 4 | S7-22 (factory) | S7-20 (criterion splits) | Subagent batch S7-13, S7-14, S7-16 (prompt 3) | S7-22, S7-20 |
| 5 | S7-23 (identity resolver, matching) | S7-17 (guide for the tester) | S7-19 if O-S5 is there | S7-23 |
| 6 | S7-25 (search, server) | S7-24 (seeder) | S7-18 if the tester's findings are there | S7-25, S7-24 |
| 7 | S7-27 (read projections; split a/b likely, prompt 4) | S7-26 (S40) | – | S7-27, S7-26 |
| 8 | stretch S7-28 (S41) | – | S7-30, then S7-31 if O-1/K-1 arrived | S7-28 |
| 9 | stretch S7-29 (S42) | S7-04 (documentation, last) | – | sprint end |

After each report: the result goes to the control chat, the owner merges, the table goes on. The gated tasks S7-18 and S7-19 can take any free window B or C once their input is there.

### Prompt 1 — put in front of every task prompt in window B or C

```text
Another session uses the local E2E stack at the same time. Do not start it; leave E2E to CI and say so in the protocol.
```

### Prompt 2 — subagent batch of wave 1 (window B)

```text
Run the tasks S7-02, S7-06 and S7-15 from docs/sprints/sprint_07/TASK_PROMPTS.md in parallel: one subagent per task, each in its own git worktree (isolation: worktree), model Sonnet 5.5.

Give each subagent the complete prompt block of its task (the text inside its ```text block) unchanged, plus these rules:
- You cannot ask the owner. Write the plan of step 2 at the top of your protocol instead of waiting for approval, then implement it.
- If a stop condition of step 2 applies (contradiction, missing UX key, owner decision, new dependency) or the plan needs more than one session: stop before pushing and report the question.
- Do not start the local E2E stack; leave E2E to CI.
- Push the branch and open the PR. Do not merge.

Wait for all three. Then give me one combined report: per task the PR link, protocol path, CI status, open questions and deviations. Check whether the PRs touch the same files; if they do, give me a merge order.
```

The model in this prompt is the owner's choice and may be changed. Fill in O-S7-2 in the block of S7-06 first. S7-15 asks for a choice in its plan: as a subagent it stops and reports the options.

### Prompt 3 — subagent batch of wave 4 (window C)

Prompt 2 with the first line changed to `Run the tasks S7-13, S7-14 and S7-16 … model Opus 5.5.` Fill in O-S7-8 and O-S7-9 in their prompt blocks first.

### Prompt 4 — continue after a split (e.g. S7-27a merged)

```text
Continue with S7-27b as agreed in the plan of S7-27a (see docs/sprints/sprint_07/S7-27a_*.md, section 5). Use the prompt block of S7-27 in docs/sprints/sprint_07/TASK_PROMPTS.md with branch sprint07/S7-27b-<topic> and protocol S7-27b_<topic>.md. Start from an up-to-date main that contains S7-27a.
```

The same for any other split, with its ID.

### Prompt 5 — start a gated task

```text
The input for <S7-18 | S7-19 | S7-30 | S7-31> has arrived: <what arrived and where it is — issue numbers, the Figma mode name, the path of the filled gap list>. Run that task from docs/sprints/sprint_07/TASK_PROMPTS.md: read its prompt block and follow it exactly. Begin by checking that the input is complete, and tell me what is missing before you plan.
```

---

## P0 — committed

### S7-01 — Name the causes of the E2E failures on main

```text
Task S7-01 from docs/sprints/sprint_07/SPRINT_07.md — Name the causes of the E2E failures on main
Issues: #451, #452, #436
Requirements / invariants: A11Y-01, A11Y-02, A11Y-03, A11Y-08 stay green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 451, gh issue view 452, gh issue view 436) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.4 (TD-84, TD-85, TD-99) and §1; CLAUDE §3 (E2E rules); docs/sprints/sprint_06/S6-01_main_green.md (P2 – P4), S6-06_firefox_crash_s10_flake.md, S6-27_reload_flows.md (P7), S6-29_backup_restore.md (P8); the failed CI runs named on the issues; e2e/shell/start-and-error-pages.spec.ts, e2e/identity/preferences.spec.ts, e2e/identity/account.spec.ts

Owner decisions for this task: none
Additional instructions: First task of the sprint, alone on the local E2E stack. For each failure decide whether the product or the test is wrong: (1) #451 Firefox loses the H1 focus on S80 after an API 5xx — check the focus move against a late re-render; (2) #452 (a) axe runs on mobile Safari while the text still has its light colour after switching to dark — wait for the applied appearance, never a fixed sleep; (b) the compact menu focus after Esc; (3) #436 the test types before the dialog has its first focus. A product defect gets a unit test that fails before the fix. With it TD-99: move the keyboard helpers tabTo, arrowTo… and settled into one shared e2e support module, and give the two unbounded download waits a bound. Done means three consecutive green CI runs on main after the merge (report the run links).

Way of working:
1. Branch sprint07/S7-01-e2e-causes from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-01_e2e_causes.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-02 — Quick wins: evidence race, TimeSpan reader, legacy remnants, log sink, page title

```text
Task S7-02 from docs/sprints/sprint_07/SPRINT_07.md — Quick wins: evidence race, TimeSpan reader, legacy remnants, log sink, page title
Issues: #453, #456, #437
Requirements / invariants: WSP-01 stays green; SEC-10, SEC-11 stay green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 453, gh issue view 456, gh issue view 437) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.2 (TD-94), §3.3 (TD-88, TD-95), §3.5 (#437), §3.1 (TD-68); docs/sprints/sprint_06/S6-15_candidates_server.md (P5, R3), S6-05_mail_token_hygiene.md (P2), S6-10a_users_identityaccess.md (P4), S6-23_retire_proposals.md (P3), S6-29_backup_restore.md (P9)

Owner decisions for this task: none
Additional instructions: One PR, one commit per item. #453: the second of two concurrent POST …/test-evidences for one procedure answers 400 with the duplicate field error, not 500 (catch the unique violation; a deterministic test). #456: one shared reader for TimeSpan settings that requires hh:mm:ss and fails the start on a bare number, used by every TimeSpan setting (list them in the protocol; say whether an existing .env value would now fail). TD-95: remove the Identity package and usings from SERVICES and move or delete Permissions and CustomClaimTypes in MODELS if nothing else reads them. #437: find out whether the first log batch is lost on an empty database and make the sink start after its schema exists. TD-68: the page title follows a second confirmation link (PageTitleStrategy.changePageInPlace).

Way of working:
1. Branch sprint07/S7-02-quick-wins from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-02_quick_wins.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-03 — Dependabot batch

```text
Task S7-03 from docs/sprints/sprint_07/SPRINT_07.md — Dependabot batch
Issues: PR #439, #440, #441, #442, #443, #444
Requirements / invariants: none; every test level stays green
Depends on: O-S7-11

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh pr view 439, gh pr view 440, gh pr view 441, gh pr view 442, gh pr view 443, gh pr view 444) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.5 (TD-51, TD-102) and §9.5; CLAUDE §10 (licences); docs/sprints/sprint_06/S6-08_dependabot_batch.md

Owner decisions for this task: O-S7-11: <up-to-date branches required? Serilog.AspNetCore 11: take or ignore — decision and date>
Additional instructions: One PR at a time, in this order: #439 (actions), #443 (dotnet), #441 (vitest), #442 (other), #440 (Angular 22.3.0). Rebase each on the previous merge. #444 Serilog.AspNetCore 11.0.0: check its target frameworks against the rule "stay on .NET 10 LTS" and report before touching it. Regenerate THIRD-PARTY-NOTICES.txt where packages change; run the material-symbols subset script if that package moves. Do not merge: report per PR what changed and whether I can merge.

Way of working:
1. Branch sprint07/S7-03-dependabot-batch from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-03_dependabot_batch.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-04 — Documentation session and the CODEBASE split

```text
Task S7-04 from docs/sprints/sprint_07/SPRINT_07.md — Documentation session and the CODEBASE split
Issues: none (TD-100, TD-101)
Requirements / invariants: none
Depends on: every other P0 task merged; O-S7-12

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.6 (TD-100, TD-101, and TD-79/TD-80 as the owner's lists); every protocol in docs/sprints/sprint_06/ and docs/sprints/sprint_07/ (section 5, open points marked "documentation"); docs/sprints/sprint_06/S6-09_documentation_session.md

Owner decisions for this task: O-S7-12: <split CODEBASE into docs/codebase/<area>.md — decision and date>
Additional instructions: Carry the findings of sprints 06 and 07 into TESTING, SPEC (§13 included), ARCHITECTURE (§9.1 key store, §16.5 closed questions), the Design handoff (S34 tabs as built, S6-16 P3), the Figma mapping and the code comments; headers name current versions. If decided: split CODEBASE into one file per area with a short index, keep every task ID findable, and update CLAUDE.md §2 and the generic template at the end of this sprint's TASK_PROMPTS.md so "CODEBASE §2" points to the new place. ROADMAP and the SPRINT_07 close-out are done in my control chat; PRD, Kriterienkatalog and UX copy are owner documents — list proposed changes in the protocol.

Way of working:
1. Branch sprint07/S7-04-documentation-session from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-04_documentation_session.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-05 — Assessments in the personal data export

```text
Task S7-05 from docs/sprints/sprint_07/SPRINT_07.md — Assessments in the personal data export
Issues: #414
Requirements / invariants: PRV-07 (workspace part complete); TEN-11 stays green
Depends on: O-S7-1

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 414) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC §8.4, R-AUTH-09; TESTING PRV-07; TECH_DEBT §3.2 (#414, TD-62 for what stays open); docs/sprints/sprint_06/S6-25_account_projects.md (P1), docs/sprints/sprint_05/S5-27_personal_data_export.md

Owner decisions for this task: O-S7-1: <shape of the assessments — decision and date>
Additional instructions: createdByMe.projects gains the assessments of each candidate in the decided shape (stored rows only; procedure criteria have no rows and are not invented). Only projects the user created; no other user's identity (sentinel test). Update SPEC §8.4 and its example, and flip PRV-07's workspace part to complete. TD-62 (ModifiedByUserId, lists of left organisations) stays with the legal review: do not widen the scope.

Way of working:
1. Branch sprint07/S7-05-export-assessments from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-05_export_assessments.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-06 — Path back from S11 of a project copy

```text
Task S7-06 from docs/sprints/sprint_07/SPRINT_07.md — Path back from S11 of a project copy
Issues: #429
Requirements / invariants: R-PRJ-06; A11Y-01, A11Y-09
Depends on: O-S7-2

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 429) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.1 (#429), §3.4 (TD-91); Design handoff §5.1; docs/sprints/sprint_06/S6-22b_j2_journey.md (P1), S6-12b_project_copy_client.md (P3), S6-14_project_overview_s32.md (P6); e2e/shell/reflow.spec.ts

Owner decisions for this task: O-S7-2: <text of the last path item — decision and date>
Additional instructions: pa-breadcrumb above the H1 of /projekte/:id/kriterienliste as on S32 and S34 (Projekte → project name → last item as decided). Add the page to reflow.spec.ts in the same PR (TD-91) and to the axe runs in both appearances. J2 may use the breadcrumb instead of the browser's back.

Way of working:
1. Branch sprint07/S7-06-copy-breadcrumb from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-06_copy_breadcrumb.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-07 — Assign or replace a project's list on S32

```text
Task S7-07 from docs/sprints/sprint_07/SPRINT_07.md — Assign or replace a project's list on S32
Issues: #420
Requirements / invariants: R-PRJ-04 (UI); WSP-02, API-03 stay green; A11Y-01
Depends on: O-S7-3; S7-06 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 420) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-02, R-PRJ-04; TESTING WSP-02; Design handoff §5.1 (S32), §5.3 (dialogs); Figma mapping §2 (S32); UX copy projects.detail.*; ARCHITECTURE §16.5 O-28; docs/sprints/sprint_06/S6-12a_project_copy.md, S6-12b_project_copy_client.md (P1), S6-14_project_overview_s32.md (P1, D2), S6-18_assessment_server.md, S6-20a_etag_concurrency.md, S6-21a_conflict_dialog_s37.md

Owner decisions for this task: O-S7-3: <dialog design, place on S32, UX keys, confirm before replacing, O-28 — decision and date>
Additional instructions: The server part exists (PUT …/criteria-configuration, the lock, If-Match). Build the UI as decided: projects.detail.noCriteria.cta when the project has no list, a replace action otherwise; the dialog reuses S31's select (saved lists of the active organisation, never project copies) with the S13 dialog rules; confirm before replacing; the lock's 409 shows its text; a stale ETag opens S37. If O-28 is "yes", assigning moves the project's last change (server change with a test). E2E: assign, replace, replace refused after an assessment. Close milestone M5 in the report if #414, #429 and #453 are done.

Way of working:
1. Branch sprint07/S7-07-assign-replace-list from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-07_assign_replace_list.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-08 — Workspace correctness: archived copy on the server, lock and copy atomic

```text
Task S7-08 from docs/sprints/sprint_07/SPRINT_07.md — Workspace correctness: archived copy on the server, lock and copy atomic
Issues: none (TD-86, TD-87; ARCHITECTURE O-30, O-31)
Requirements / invariants: WSP-02, WSP-03, API-03, TEN-05 stay green; ARC-01 – ARC-03
Depends on: O-S7-4

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §16.5 (O-30, O-31), §4.3 (module dependencies), ADR-014; TECH_DEBT §3.2 (TD-86), §3.3 (TD-87); TESTING WSP-02, WSP-03, API-03; docs/sprints/sprint_06/S6-12a_project_copy.md (P1, P2, D3), S6-18_assessment_server.md (P1, R3), S6-20a_etag_concurrency.md (R2)

Owner decisions for this task: O-S7-4: <O-30 and O-31 — decisions and date>
Additional instructions: Two fixes, planned together because both cross the Workspace and Configurator contexts. TD-86: a change to the copy of an archived project is refused on the server (Configurator asks Workspace through a contract along the allowed dependency direction — show it against the table of ARCHITECTURE §4.3; no cycle). TD-87: the lock check and the storing of the copy cannot be separated by an assessment or a rename (the decided approach; if it is a row lock with raw SQL, add the allow-list entry and its test). Deterministic race tests (no sleeps, no timing), and a note in the plan on deadlock order. Close O-30 and O-31 in ARCHITECTURE §16.5.

Way of working:
1. Branch sprint07/S7-08-workspace-correctness from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-08_workspace_correctness.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-09 — Deployment hardening: ports, public-address settings, health route

```text
Task S7-09 from docs/sprints/sprint_07/SPRINT_07.md — Deployment hardening: ports, public-address settings, health route
Issues: #449, #447, #448
Requirements / invariants: OPS-01, OPS-02, SEC-06 stay green
Depends on: O-S7-5

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 449, gh issue view 447, gh issue view 448) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECHNICAL.md §3.1, §4.3, §6.1; TECH_DEBT §3.2 (#449, #447, #448, TD-28 points 11 – 13 and 15); docker-compose.yml, docker-compose.dev.yml, docker-compose.e2e.yml, the client nginx.conf; docs/sprints/sprint_06/S6-30_technical_readme.md (P1 – P3), S6-20a_etag_concurrency.md (R6)

Owner decisions for this task: O-S7-5: <#449 base file or override; #447 values; #448 nginx.conf or proxy — decisions and date>
Additional instructions: #449: bind the API and client ports to 127.0.0.1 as decided; the dev and E2E stacks must keep working on localhost. #447: compose passes App__BaseUrl, Email__From, Email__DisplayName, Export__ToolUrl, ProblemTypes__BaseUri; outside Development the API refuses to start with the appsettings default of a public address (a start-up check with a test, like the key-ring certificate). #448: /health/live and /health/ready reachable through the client's nginx, answering only the status (no details to the public). Make sure nginx does not compress or rewrite /api/ answers in a way that weakens ETags (TD-28 point 15) and add a check for it. Update TECHNICAL.md and .env.example. No secrets in the repository.

Way of working:
1. Branch sprint07/S7-09-deployment-hardening from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-09_deployment_hardening.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-10 — Staging from CI and the first deployment

```text
Task S7-10 from docs/sprints/sprint_07/SPRINT_07.md — Staging from CI and the first deployment
Issues: #236
Requirements / invariants: OPS-01 – OPS-04, SEC-06 stay green
Depends on: S7-09 merged; O-S7-6; the owner at the server

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 236) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECHNICAL.md §3, §4, §5, §7.6; ARCHITECTURE §10.2; TECH_DEBT §3.2 (TD-28 points 1 – 15) and the comments on #236 of 2026-11-13, 2026-11-17 and 2026-11-24; .github/workflows/ci.yml; docs/sprints/sprint_04/S4-26 (key ring), docs/sprints/sprint_05/S5-11_email_sender.md, S5-07_cookie_session_server.md

Owner decisions for this task: O-S7-6: <how CI triggers the deployment; staging reset before the pilot — decision and date>
Additional instructions: You cannot reach the server: prepare, and I do the steps there. (1) A workflow that deploys main to staging by manual dispatch in the decided way, with the hook URL or key only as a repository secret, never echoed. (2) A smoke check after the deployment against the public address: /health/ready 200, the sign-in page loads, the catalog answers with the active release. (3) A step-by-step checklist for me from TD-28 points (1) – (15) with the exact variable names and commands (key-ring certificate, ASPNETCORE_ENVIRONMENT never Development, SMTP credentials and tracking off, public addresses, X-Forwarded-Proto, proxy log masking for share tokens, no compression on /api/, the backup profile with BACKUP_DIR outside the checkout, JWT_KEY removed), each with how to verify it. (4) After my deployment: check a registration mail carries a staging link, tick #236 and update TECHNICAL.md §4. Stop and ask whenever a value or a step on the server is needed.

Way of working:
1. Branch sprint07/S7-10-staging-from-ci from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-10_staging_from_ci.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-11 — Backup completion: empty-dump check, alert, off-host copy, staging rehearsal

```text
Task S7-11 from docs/sprints/sprint_07/SPRINT_07.md — Backup completion: empty-dump check, alert, off-host copy, staging rehearsal
Issues: #455
Requirements / invariants: R-ADM-04; API-04 stays green
Depends on: S7-09 merged; O-S7-7; the staging rehearsal after S7-10

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 455) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECHNICAL.md §6.3, §7; TECH_DEBT §3.2 (TD-92, TD-93 for what stays with the legal review); scripts/backup/; docs/sprints/sprint_06/S6-29_backup_restore.md (P1 – P5, D3)

Owner decisions for this task: O-S7-7: <target of the off-host copy and who gets the alert — decision and date>
Additional instructions: A dump of an emptied database must fail the run instead of counting as a success (a plausibility check, e.g. minimum row counts or size against the previous dump — propose it). An alert when the backup or the API is unhealthy, in the decided way, without a new paid service. The off-host copy as decided, credentials only from the environment, the copy encrypted in transit; retention on both sides. Then, with me, the restore rehearsal on staging per TECHNICAL.md §7.5 (ETagEpoch bumped) and its record in this folder. Never touch my local dev database or volumes.

Way of working:
1. Branch sprint07/S7-11-backup-completion from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-11_backup_completion.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-12 — Security small items: role changes at once, another user in another tab

```text
Task S7-12 from docs/sprints/sprint_07/SPRINT_07.md — Security small items: role changes at once, another user in another tab
Issues: #450
Requirements / invariants: SEC-07, SEC-08 stay green; TEN-06
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 450) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.2 (#450), §3.1 (TD-63); ARCHITECTURE §9.1; TECHNICAL.md §5.2; docs/sprints/sprint_05/S5-29_session_hardening.md, S5-30_session_warning.md (D3, R7); docs/sprints/sprint_06/S6-30_technical_readme.md (P4)

Owner decisions for this task: none
Additional instructions: #450: roles grant and roles revoke update the user's security stamp, so the check of S5-29 ends the old claims at the next request (the user signs in again; say so in TECHNICAL.md §5.2); tests on the fake clock. TD-63: when the session probe shows a different user than the one this tab holds, clear the in-memory data and treat it like the S5-30 D3 case (another account in the overlay) — no data of the previous user stays visible; a unit test and one E2E check with two contexts.

Way of working:
1. Branch sprint07/S7-12-role-stamp-other-user from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-12_role_stamp_other_user.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-13 — S12 reloads after release-mismatch

```text
Task S7-13 from docs/sprints/sprint_07/SPRINT_07.md — S12 reloads after release-mismatch
Issues: #454
Requirements / invariants: CAT-09 stays green; A11Y-01
Depends on: O-S7-8

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 454) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.1 (TD-89); docs/sprints/sprint_06/S6-27_reload_flows.md (P1); the S13 reload as built (CODEBASE, search S6-27)

Owner decisions for this task: O-S7-8: <UX key and wording — decision and date>
Additional instructions: S12 offers „Kriterien neu laden“ after release-mismatch like S13, with the decided key; the draft's decisions are carried as S13 does. Unit tests. This must be merged before a second release is activated anywhere (S7-30).

Way of working:
1. Branch sprint07/S7-13-s12-reload from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-13_s12_reload.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-14 — Release pipeline with check-release-config

```text
Task S7-14 from docs/sprints/sprint_07/SPRINT_07.md — Release pipeline with check-release-config
Issues: none (TD-29)
Requirements / invariants: R-SHL-07, R-SUG-01 (release checks)
Depends on: nothing (after S7-10 if both touch the workflows)

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECHNICAL.md §3.3, §4.2; SPEC §11 (release prerequisites); TECH_DEBT §3.2 (TD-29); scripts/check-release-config.mjs; docs/sprints/sprint_06/S6-28_legal_pages.md (P1)

Owner decisions for this task: none
Additional instructions: A manually dispatched workflow that builds the production images and runs check-release-config.mjs, so a placeholder legal text, the example.invalid suggestion channel or a default public address stops a release and names each reason. It is expected to fail today; show that run. A test of the script per reason. No deployment to production in this task.

Way of working:
1. Branch sprint07/S7-14-release-pipeline from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-14_release_pipeline.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-15 — Licence notices for the base images

```text
Task S7-15 from docs/sprints/sprint_07/SPRINT_07.md — Licence notices for the base images
Issues: none (TD-44, TD-53)
Requirements / invariants: none; the notices check stays green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: CLAUDE §10; TECH_DEBT §3.2 (TD-44), §3.6 (TD-53); docs/sprints/sprint_04/S4-27 (notices, P4), S4-20a (P2); the Dockerfiles

Owner decisions for this task: none
Additional instructions: Cover the OS packages of the shipped base images (aspnet:10.0, nginx, PostgreSQL; Mailpit is development only): propose the smallest sound option — a generated list per image tag in the notices, or a documented statement pointing to the images' own licence information — and stop for my choice. The check must notice an image tag change. TD-53 (BIT inklusiv permission) is mine: only add the place where it is recorded.

Way of working:
1. Branch sprint07/S7-15-base-image-licences from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-15_base_image_licences.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-16 — Pre-pilot polish: one time zone, .contrast-more, content links

```text
Task S7-16 from docs/sprints/sprint_07/SPRINT_07.md — Pre-pilot polish: one time zone, .contrast-more, content links
Issues: none (TD-47, TD-69, TD-67)
Requirements / invariants: EXP-01 – EXP-03 stay green; A11Y-01, A11Y-06, A11Y-08
Depends on: O-S7-9

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §3.3 (TD-47), §3.1 (TD-69, TD-67); SPEC R-SET-04; docs/sprints/sprint_05/S5-23_share_links_api.md, S5-25b_user_preferences_client.md (R4), S5-26_settings_s60.md (P2), S5-09_login_s01_overlay.md (P4); docs/sprints/sprint_06/S6-15_candidates_server.md

Owner decisions for this task: O-S7-9: <one time zone and the German day; what .contrast-more shows — decisions and date>
Additional instructions: TD-47: one 🔧 time-zone setting replaces ShareLinks:TimeZone and Workspace:TimeZone and is used for the export file names and „Info“ dates and the account export (a fake-clock test at 0:30 German time); keep the old keys working or fail the start with a clear message — propose which. TD-69: .contrast-more as decided; axe with forced prefers-contrast in both appearances. TD-67: one shared content-link style in main.scss with the token colours, applied to links outside the footer.

Way of working:
1. Branch sprint07/S7-16-prepilot-polish from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-16_prepilot_polish.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-17 — Guide for the manual accessibility pass

```text
Task S7-17 from docs/sprints/sprint_07/SPRINT_07.md — Guide for the manual accessibility pass
Issues: #311, #102
Requirements / invariants: A11Y-07
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 311, gh issue view 102) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §7.2; TECH_DEBT §3.4 (TD-49), §3.1 (TD-41, TD-66); accessibility_review_procureaccess; the journeys J1, J2, J5 as E2E specs (for the steps)

Owner decisions for this task: none
Additional instructions: A German document for the accessibility tester (a professional who uses a screen reader): per screen of M2 – M5 (S10, S11, S13, the exported XLSX, S01 – S03, S12 – S16, S20, S21, S60, S61, session warning and login overlay, S30 – S34b, S37, the remove dialog) the steps and what to check with keyboard and screen reader, the known open points to judge (TD-41 announcements, the double alertdialog of S6-01 P3, #102), test accounts and how to reach the application, and how a finding is reported (issue template with labels a11y and blocker or not). No code change. Ask me where it should live and in which format before writing (project rule on output formats).

Way of working:
1. Branch sprint07/S7-17-a11y-pass-guide from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-17_a11y_pass_guide.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-18 — Fixes after the tester's pass

```text
Task S7-18 from docs/sprints/sprint_07/SPRINT_07.md — Fixes after the tester's pass
Issues: #311 and the issues the pass creates
Requirements / invariants: A11Y-01 – A11Y-09
Depends on: the tester's pass is done

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 311) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: the issues labelled a11y created by the pass; TESTING §7.2, §12.4 (the suppression list stays empty); TECH_DEBT §3.1 (TD-41, TD-66); UX copy §4 (announcements)

Owner decisions for this task: none
Additional instructions: Gated task: start only when the findings exist. List them first, sorted blocker / other, with a plan per finding and an estimate; propose a split if they do not fit one session. Blockers first. Every fix gets a regression test where a machine can check it (axe, keyboard journey, unit). TD-41 and TD-66 need UX keys: ask.

Way of working:
1. Branch sprint07/S7-18-a11y-fixes from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-18_a11y_fixes.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-19 — Dark theme release

```text
Task S7-19 from docs/sprints/sprint_07/SPRINT_07.md — Dark theme release
Issues: #240
Requirements / invariants: R-SET-03; A11Y-08 complete
Depends on: O-S5 (Figma dark mode and its review)

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 240) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SET-03, §13 O-S5; Design handoff §1.2; TECH_DEBT §3.3 (TD-34); CLAUDE §2 (Figma files); docs/sprints/sprint_03/S3-09 and S3-10 (theme foundation, dark in axe), docs/sprints/sprint_04/S4-29 (light values pinned, P3)

Owner decisions for this task: none
Additional instructions: Gated task: start only when the Figma file has the dark variable mode and I confirm its review. Read the dark values through the Figma MCP connection (variable modes), pin them as tokens as S4-29 did for light, remove what marked dark as "development", and run axe plus the contrast checks in dark on every page of reflow.spec.ts. Report every pair whose contrast fails instead of adjusting a value yourself.

Way of working:
1. Branch sprint07/S7-19-dark-theme-release from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-19_dark_theme_release.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-20 — Criterion splits: several successors

```text
Task S7-20 from docs/sprints/sprint_07/SPRINT_07.md — Criterion splits: several successors
Issues: #238
Requirements / invariants: CAT-22, CAT-23 stay green; prepares CAT-03, CAT-04
Depends on: O-S7-10

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 238) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §5.1; TECH_DEBT §3.3 (TD-31); TESTING CAT-03, CAT-04, CAT-22; docs/sprints/sprint_03/S3-14 (P2), S3-15 (release format), S3-16 (import); docs/sprints/sprint_05/S5-21a_edit_delete_server.md (carry-over)

Owner decisions for this task: O-S7-10: <splits as several successors — decision and date>
Additional instructions: A criterion can name several successors (Catalog migration; stored releases are never changed — show how existing rows are carried). The release folder format, ReleaseFolderValidator and the importer follow; the MiniCatalog gets a split case. The carry-over of saved lists to a newer release handles a split as decided. Record in TESTING what CAT-03/04 (cross-release matching, S7-23) rely on.

Way of working:
1. Branch sprint07/S7-20-criterion-splits from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-20_criterion_splits.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-21 — Directory domain: products, published entries, immutable versions

```text
Task S7-21 from docs/sprints/sprint_07/SPRINT_07.md — Directory domain: products, published entries, immutable versions
Issues: none
Requirements / invariants: ARC-07 active; the immutability and PUB invariants of the model; TEN-01/TEN-02 allow-list for public data; OPS-03
Depends on: nothing (first of the Directory line)

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §4.4 (flows B, D, E), §5.5, §7.3, §12 step 6, ADR-003, ADR-014; TESTING §4.1, §4.3, §9 step 6 (PUB-*, ARC-07, SIG-*, PRV-*), §6.2 (canonical fixtures); SPEC §6 (directory model); architektur_review (the three critical findings); CODEBASE (the Directory module skeleton, search S2-06; the contributor profiles of S4-25)

Owner decisions for this task: none
Additional instructions: Entities, context, migration and builders only — no endpoint, no factory logic yet (S7-22). SoftwareProduct; PublishedEntry with its channel and origin kind; PublishedEntryVersion immutable once stored; published assessments and procedure answers as ARCHITECTURE §5.5 has them; the relation to contributor profiles without a key across contexts (ADR-014). The immutability interceptor: a stored version cannot be changed or deleted except by the purge rules the architecture names — tests for every blocked path. Directory data is public: record how TEN-01/TEN-02 treat entities without OrganizationId (allow-list with reasons). In the plan, list which step-6 invariants this task activates and which stay for S7-22, S7-23, S7-27.

Way of working:
1. Branch sprint07/S7-21-directory-domain from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-21_directory_domain.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-22 — PublishedEntryFactory with redaction

```text
Task S7-22 from docs/sprints/sprint_07/SPRINT_07.md — PublishedEntryFactory with redaction
Issues: none
Requirements / invariants: PRV-01, PRV-02, PRV-03, SIG-03 (published side), CAT-15 (published), the PUB invariants of the factory
Depends on: S7-21 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §4.4 – §4.6, §5.5, §9.3, §12 step 6 ("all three paths at once"); TESTING §4.1 – §4.3, §9 step 6 ("Factory with all three paths at once; redaction with sentinels; consent records"); SPEC R-PUB-*, R-CTR-*, R-VEN-* (only for the inputs); docs/sprints/sprint_06/S6-15_candidates_server.md (private section), S6-18_assessment_server.md (P6)

Owner decisions for this task: none
Additional instructions: The factory is the only way to create an entry or a version: one input contract that the three later write paths (workspace publication, contribution, vendor publication) will fill — define it in Directory.Contracts, with no dependency from Directory on Workspace. Redaction: private candidate fields (contacts, internal notes), organisation, user and project data never reach an entry — sentinel tests with marked values through every input. A test-result version without a definite WasTested is rejected; ratings are validated per path (CAT-15). Consent records where the architecture requires them. An architecture test that nothing outside the factory constructs entries or versions. No endpoint and no screen: the write paths are sprint 08.

Way of working:
1. Branch sprint07/S7-22-entry-factory from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-22_entry_factory.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-23 — Product-identity resolver and cross-release matching

```text
Task S7-23 from docs/sprints/sprint_07/SPRINT_07.md — Product-identity resolver and cross-release matching
Issues: none
Requirements / invariants: CAT-03, CAT-04 active; OPS-03
Depends on: S7-21 merged; S7-20 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §5.5 (identity at publication time, resolver, admin merge), §12 step 6; TESTING CAT-03, CAT-04, §6.2; SPEC R-ADM-02 (S81 comes later), R-CAN-02 (name-match hint); docs/sprints/sprint_06/S6-15_candidates_server.md (P3)

Owner decisions for this task: none
Additional instructions: One shared resolver for all paths: trigram similarity on name and vendor with pg_trgm (a Directory migration for the extension and index; state what the database user must be allowed, TECHNICAL.md §4.3), thresholds 🔧, never an automatic merge of two existing products. Cross-release matching of assessments across catalog releases, using the successors of S7-20. Tests on the canonical fixtures: same product in other spelling, near names of different vendors, empty vendor. The licence of pg_trgm (PostgreSQL contrib) noted per CLAUDE §10.

Way of working:
1. Branch sprint07/S7-23-identity-resolver from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-23_identity_resolver.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-24 — Directory entries in the development and E2E seeder

```text
Task S7-24 from docs/sprints/sprint_07/SPRINT_07.md — Directory entries in the development and E2E seeder
Issues: none
Requirements / invariants: the factory invariants stay green; SEC-06 (seeders never outside Development/E2E)
Depends on: S7-22 and S7-23 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-24 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §6.2, §6.3; TECH_DEBT §5 ("E2E: no E2E environment", S5-14 P5); CODEBASE (search S4-08 test release seeder, S5-14 E2E users); TECHNICAL.md §2

Owner decisions for this task: none
Additional instructions: Products and entries of both channels and every origin kind, created through the factory only, for the development and E2E stacks: enough to show S40 with paging (more than 20 products), filters and an empty search. Invented products and vendors only, clearly fictional. Decide with me in the plan whether the E2E stack now needs its own environment name (the soft-deleted seeded user problem of S5-14 P5).

Way of working:
1. Branch sprint07/S7-24-directory-seeder from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-24_directory_seeder.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-25 — Directory search: server

```text
Task S7-25 from docs/sprints/sprint_07/SPRINT_07.md — Directory search: server
Issues: none
Requirements / invariants: DIR-01, DIR-02, DIR-03 active; TEN-04 and API-06 directory parts
Depends on: S7-23 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-25 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-DIR-01 – R-DIR-07, §8 (directory endpoints); ARCHITECTURE §5.5, §7.1 (offset paging), §7.2, D3, C32; TESTING §4.9a; Design handoff §4.9

Owner decisions for this task: none
Additional instructions: GET /api/v1/directory/products: search by name or vendor, the filters in layer order (use case, app types, test procedures incl. „Andere Prüfung“, origin, the conformance-report filter only as far as P0 defines it), sort by name (default), last published, number of entries — nothing else and never by matching (DIR-02) — offset paging 20. The result item holds facts only: no tested ratio, no fulfilment number. Public: no organisation or user data, cache headers as ARCHITECTURE says for the directory (TEN-04 directory part), Accept-Language (API-06). Say whether the search needs an index (Directory migration).

Way of working:
1. Branch sprint07/S7-25-directory-search-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-25_directory_search_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-26 — Directory search S40

```text
Task S7-26 from docs/sprints/sprint_07/SPRINT_07.md — Directory search S40
Issues: #170 (L2, product list), #82 (Z1)
Requirements / invariants: R-DIR-01 – R-DIR-07; A11Y-01, A11Y-09
Depends on: S7-25 and S7-24 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-26 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 170, gh issue view 82) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-DIR-*; Design handoff §4.9 (pa-product-result), §5.2; Figma mapping §2 (S40: 360 2134:2905, desktop 2053:2051, 2142:3850 use case selected); UX copy directory.*; ARCHITECTURE §8.1 (withDirectory)

Owner decisions for this task: none
Additional instructions: Compare with the frames before building and before pushing. withDirectory() slice; search field with 300 ms debounce and the previous request cancelled; filters; filters and sort in the URL query (reload and back restore the results); directory.results visible and announced (polite, throttled); directory.results.orderNote, combined with the use-case note when one is selected; mat-paginator only above 20 results; the empty state with directory.empty.suggest (S52 comes in sprint 08: link disabled or hidden — ask). „Produkte“ in the main navigation leads to S40. The legacy product list is no longer linked but its code stays (it goes in sprint 08, M6.6d); a product result links to the legacy detail until S41 exists — or to nothing: ask in the plan. Record on #170 and #82 what is left.

Way of working:
1. Branch sprint07/S7-26-directory-search-s40 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-26_directory_search_s40.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-27 — Read projections for the product page and the entry detail

```text
Task S7-27 from docs/sprints/sprint_07/SPRINT_07.md — Read projections for the product page and the entry detail
Issues: none
Requirements / invariants: SIG-01 – SIG-11 for the built parts, PRV-04, PUB-11, PUB-14, CAT-17 (directory)
Depends on: S7-22, S7-23 and S7-25 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-27 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §4.4, §5.4, §5.5, §7.3, ADR-003; TESTING §4.1 (two-signal principle), §4.3, §9 step 6; SPEC R-ENT-01 – R-ENT-10, §8 (directory endpoints); Design handoff §4 (K1 – K4); architektur_review (critical findings 1 and 2: score display, the private configurationId on a public endpoint)

Owner decisions for this task: none
Additional instructions: The endpoints S41 and S42 read: two channel sections that never merge; per test-result entry wasTested (never null), K4 data, the seven counts with notCovered against the entry's own scope; a vendor entry without assessments; experience reports apart with no K1, no K4 and no counts; staleness from PublishedAt (18 months 🔧); the context line with its source; previous versions. The comparison with a list of the caller's organisation is its own authorised, non-cacheable request — a private id never appears in a public, cacheable URL — with a test of the IDOR case (another organisation's list → 404). No fulfilment score anywhere: contract tests assert the projections hold the two signals separately and nothing that could be shown as a ranking. Split if needed (a: product page; b: entry detail and comparison).

Way of working:
1. Branch sprint07/S7-27-read-projections from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-27_read_projections.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## P2 — stretch

### S7-28 — Product page S41 *(stretch)*

```text
Task S7-28 from docs/sprints/sprint_07/SPRINT_07.md — Product page S41
Issues: #170 (L3), #82 (Z3, R5)
Requirements / invariants: R-ENT-01 – R-ENT-08, R-ENT-10; SIG-04, SIG-07 – SIG-09, SIG-11 (UI); A11Y-01, A11Y-09
Depends on: S7-27 and S7-26 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-28 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 170, gh issue view 82) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-ENT-*; Design handoff §4 (K1 – K4), §5.2 (S41); Figma mapping §2 (S41: 2025:676, 320 px 2039:2139); UX copy directory.product.*, signals.*

Owner decisions for this task: none
Additional instructions: Stretch. Compare with the frames. Header facts and directory.product.noVerdict; two channel sections (H2) that keep their heading with the empty text; entry rows and expandable cards with K4 always for test-result entries and K2 and K1 in separate regions; the notCovered tag always shown, also at 0; sort of entries (last published, name, number); the compare select only signed in with an organisation; experience reports only under their heading; staleness note; copy link. „Zu einem Projekt hinzufügen“ (R-ENT-11) is sprint 08. Results of S40 link here; the legacy detail is no longer linked.

Way of working:
1. Branch sprint07/S7-28-product-page-s41 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-28_product_page_s41.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-29 — Entry detail S42 *(stretch)*

```text
Task S7-29 from docs/sprints/sprint_07/SPRINT_07.md — Entry detail S42
Issues: none
Requirements / invariants: R-ENT-09; SIG-* (UI); A11Y-01, A11Y-09
Depends on: S7-27 merged; S7-28 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-29 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-ENT-09; Design handoff §2.6 (tables as lists below 600 px), §5.2 (S42); Figma mapping §2 (S42: 360 2132:3382, desktop 2054:2542); UX copy directory.entry.*

Owner decisions for this task: none
Additional instructions: Stretch. Compare with the frames. The full card, the table „Einschätzung je Kriterium“ in catalog order with its filter (a list of items below 600 px as the Handoff says), the rating legend, previous versions; the public view projects against the entry's own scope, with a comparison list the comparison is added and labelled.

Way of working:
1. Branch sprint07/S7-29-entry-detail-s42 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-29_entry_detail_s42.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## On arrival — only if O-1/K-1 arrive

### S7-30 — Release 2026.1 with the project team's priorities *(on arrival)*

```text
Task S7-30 from docs/sprints/sprint_07/SPRINT_07.md — Release 2026.1 with the project team's priorities
Issues: #299
Requirements / invariants: CAT-02, CAT-09, CAT-23; R-ADM-01
Depends on: O-1/K-1 delivered; S7-13 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-30 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 299) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: kriterienkatalog_2026_1_luecken (the filled gap list) and kriterienkatalog_bit_inklusiv; SPEC R-ADM-01, §13 (O-1, K-1, K-6); TECHNICAL.md §5.1; CODEBASE (search S3-15, S3-16, S3-17, S3-27, S4-24); docs/sprints/sprint_03/S3-27

Owner decisions for this task: none
Additional instructions: On arrival only. Carry the delivered priorities, rationales, plain-language titles and short descriptions into the release folder exactly as delivered — never invent or complete a priority or a text; list every criterion that still lacks its catalog-default priority and stop. A stored release never changes: if 2026.1 is already imported in any environment I name, the folder becomes the next version. Validate (the folder test), import and activate in development; give me the commands for staging. Decide #299 with me (short description in the criteria answer, on S11 and in the search).

Way of working:
1. Branch sprint07/S7-30-release-2026-1 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-30_release_2026_1.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S7-31 — Legacy removal after the activation of 2026.1 *(on arrival)*

```text
Task S7-31 from docs/sprints/sprint_07/SPRINT_07.md — Legacy removal after the activation of 2026.1
Issues: #52, #158
Requirements / invariants: CAT-10 retired; A11Y-02 (J1 on the active release)
Depends on: S7-30 active where intended; S7-13 and S7-26 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_07.md the row of S7-31 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 52, gh issue view 158) and their entries in TECH_DEBT_v6.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT §5 (first row, "Legacy filter") and §9.3; ARCHITECTURE §12; ARCHITECTURE §16.5 O-32 (legal-page renderer); eslint.config.js (LEGACY_EXCEPTIONS)

Owner decisions for this task: none
Additional instructions: On arrival only. Remove the legacy filter /filters with configuratorReleaseGuard's switch, LEGACY_FILTER_PATH and the nav constants; CAT-10 with ExistingCatalogSeed, LegacyCatalogReadTests and ProductsByCriteriaFilterTests (retired in TESTING with the reason); marked and ngx-markdown with their provider (O-32: the legal pages keep the project's parser unless I say otherwise); the M2 group of LEGACY_EXCEPTIONS; the test release T.1 stays for E2E. List everything of §5's first row and tick it in the protocol.

Way of working:
1. Branch sprint07/S7-31-legacy-removal from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S6-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:PUB-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_07/S7-31_legacy_removal.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

Replace `<S>` with the sprint number (e.g. 8), `<SS>` with the two-digit sprint folder number (e.g. 08) and `<v>` with the current tech-debt register version. Name documents without version numbers. Check that new decision IDs are not used yet.

```text
Task S<S>-<NN> from docs/sprints/sprint_<SS>/SPRINT_<SS>.md — <title of the row>
Issues: <#n, … | none>
Requirements / invariants: <R-… / INV IDs the task activates or must keep green>
Depends on: <tasks, owner decisions | nothing>

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_<SS>.md the row of S<S>-<NN> plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view <n>) and their entries in TECH_DEBT_v<v>.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: <documents and sections without version numbers, and earlier task protocols in docs/sprints/ that the row or an issue points to>

Owner decisions for this task: <none | O-S<S>-n: <decision> (decided <date>)>
Additional instructions: <none | …>

Way of working:
1. Branch sprint<SS>/S<S>-<NN>-<topic> from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_<SS>/S<S>-<NN>_<topic>.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; TECHNICAL.md for anything an operator must know; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

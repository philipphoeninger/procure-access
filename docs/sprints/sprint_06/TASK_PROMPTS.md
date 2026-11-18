# Sprint 06 — task prompts for Claude Code

> **Status:** v1.2 (2026-11-18; v1.2 fills in the owner decisions O-S6-1 – O-S6-6; v1.1 added the section "Running order (waves)" with the extra prompts for parallel work). Companion to `SPRINT_06.md`: one ready-to-paste prompt per task, built from the generic template at the end of `docs/sprints/sprint_05/TASK_PROMPTS.md` (repeated, updated, at the end of this file). Claude Code implements; the owner's control chat for sprint 06 takes the decisions and reviews the reports. The model per task is the owner's choice and is not part of the prompts.
> **How to use:** copy the block of the task into a new Claude Code session (or a worktree session for parallel work). Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>`, `<Name>` or `<date>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_06.md`, `TECH_DEBT_v5.md` and the GitHub issues and pull requests as of 2026-11-17.
> **Changes to the template since sprint 05:** reads `TECH_DEBT_v5.md`; "Read first" adds the `CODEBASE` §2 sections of the touched areas (the code state moved out of `CLAUDE.md`, #346); step 5 updates `CODEBASE` §2/§3 and touches `CLAUDE.md` only for rules that apply across the codebase. Decision IDs `O-S6-n` are this sprint's; the SPEC open question O-S5 (dark mode) keeps its name.

---

## Overview

Prompts are listed in task-ID order; the working order and the parallel lines are in `SPRINT_06.md` (Sequence) and below.

| Task | Title | Issues | Can start |
|---|---|---|---|
| S6-01 | Make main green: J5 session warnings and the J1 download flake | #379, #380 | ready (first) |
| S6-02 | Quick wins: S16 gate, forced-colours edge, aria-label, health watcher, dead code | #375, #378, #350, #385 | ready (subagent batch) |
| S6-03 | Return to the target page after the sign-in | #386 | ready |
| S6-04 | Remove the legacy UI languages | #368, #158 (M3 entry) | ready (subagent batch) |
| S6-05 | Mail and token hygiene: SMTP timeout, token cleanup, nginx error_log | #382, #383, #381 | after S6-10 and O-S6-3 |
| S6-06 | Firefox crash after saving; S10 step-1 axe flake | #355, #339 | when the E2E stack is free |
| S6-07 | CI smoke: ng serve and the immutable header of lazy chunks | #388 | ready (subagent batch) |
| S6-08 | Dependabot batch and npm audit fix | PR #363, #364, #365, #366 | after S6-02 |
| S6-09 | Documentation session: sprint-05 and sprint-06 findings | #389 | last |
| S6-10 | Move the users into IdentityAccess | #387 | after S6-01 and O-S6-1 |
| S6-11 | Workspace server: projects | none | after S6-10 |
| S6-12 | Project copy of a criteria list and S11 in project-copy mode | none | after S6-11 |
| S6-13 | Project list S30 and create dialog S31 | none | after S6-11 |
| S6-14 | Project overview S32 | none | after S6-12, S6-13, S6-18 |
| S6-15 | Candidates server: candidate, test status and evidence | none | after S6-11 |
| S6-16 | Candidate form S33 and S34 with tabs and overview | none | after S6-15, S6-13 |
| S6-17 | Evidence tab S34a | none | after S6-16 |
| S6-18 | Assessment server: rows, allowed values, derived rows, counts, lock | none | after S6-15, S6-12 |
| S6-19 | Assessment tab S34b | none | after S6-18, S6-16 |
| S6-20 | Optimistic locking: ETag / If-Match for workspace resources and saved lists | none | after S6-18 |
| S6-21 | Conflict dialog S37 | none | after S6-20 |
| S6-22 | Remove a candidate, Readers read-only, J2 up to the assessment | none | after S6-19, S6-21 |
| S6-23 | Retire the proposal workflow | #384, #158 (proposal entries), #82 (check) | after S6-10 and O-S6-4 |
| S6-24 | Root store and the M5 entries of LEGACY_EXCEPTIONS | #158 (M5 entries) | ready |
| S6-25 | Account deletion and data export: projects | none | after S6-15, S6-10 |
| S6-26 | Rate limits for sign-in and organisation creation | #318 | after S6-10 and O-S6-2 |
| S6-27 | Reload flows: failed lazy chunk and release-mismatch | #296, #310 | after O-S6-5 |
| S6-28 | Legal pages from Markdown with the placeholder check | none | after O-S6-6 |
| S6-29 | Nightly backup and restore rehearsal | none | stretch, after P0 |
| S6-30 | TECHNICAL.md and README | #4 | stretch, after P0 |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S6-1 (users into IdentityAccess, data) | S6-10 | decided 2026-11-18 |
| O-S6-2 (rate-limit values) | S6-26 | decided 2026-11-18 |
| O-S6-3 (SMTP timeout) | S6-05 | decided 2026-11-18 |
| O-S6-4 (legacy proposals) | S6-23 | decided 2026-11-18 |
| O-S6-5 (reload flows) | S6-27 | decided 2026-11-18 |
| O-S6-6 (legal pages) | S6-28 | decided 2026-11-18 |

O-S6-7 (milestone hygiene) and O-S6-8 (2026.1 as a G-P criterion) need no prompt.

## Rules for running tasks

- **S6-01 first:** `main` must be green before M5 work merges; it runs alone on the local E2E stack. Free disk space before parallel sessions (TD-74).
- **Server line (one session, in order):** S6-10 → S6-11 → S6-15 → S6-18 → S6-20 → S6-22. It is the critical path; the Workspace module allows one open migration branch at a time.
- **Parallel lines beside it**, in separate worktrees (`claude --worktree s6-nn`), at most three sessions at once: client (S6-13 → S6-16 → S6-17 → S6-19 → S6-14 → S6-21), project copy (S6-12), legacy (S6-24, then S6-23 after S6-10), account and auth (S6-03; S6-05, S6-26, S6-25 after S6-10), G-P (S6-27, S6-28).
- **Subagent batch (low risk, no migration, no local E2E):** S6-02, S6-04, S6-07 together on D1; later S6-27 and S6-28 together.
- **EF migrations:** one open branch per context — Workspace (S6-11, S6-15, S6-18, S6-20), IdentityAccess (S6-10, S6-05 if it adds an index), legacy (S6-10, then S6-23), Configurator (S6-12, S6-20). The later branch rebases and regenerates.
- **Lock file:** S6-08 after S6-02; Dependabot PRs one at a time.
- **TESTING versions:** almost every M5 task flips an invariant; the later PR rebases and renumbers.
- **Local E2E:** one session at a time (shared host ports); the others leave E2E to CI. S6-06 only when the stack is free.
- **Stretch tasks** (S6-29, S6-30) only after all P0 tasks are done.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_06.md` and `TECH_DEBT_v6.md` (process step 14), prepared in the control chat.

## Running order (waves)

At most three windows at a time: **A** is the server line, attended by the owner, and the only window that runs the local E2E stack; **B** and **C** run beside it. Every task gets a new session in its own worktree (`claude --worktree s6-nn`, or `/clear` before the next task). A wave starts only when the PRs in the last column are merged. If B or C finishes early, it waits; never pull a task forward whose dependency is still open.

Before wave 1: free disk space (TD-74); before wave 2: owner decisions O-S6-1 – O-S6-6 filled in above.

| Wave | Window A (server, attended) | Window B | Window C | Next wave once merged |
|---|---|---|---|---|
| 1 | S6-01 (`main` green) | Subagent batch S6-02, S6-04, S6-07 (prompt 2) | – | S6-01, S6-02 |
| 2 | S6-10 (users; split a/b likely, prompt 4) | S6-24, then S6-08 | Subagent batch S6-27, S6-28 (prompt 3), then S6-03 | S6-10 |
| 3 | S6-11 | S6-23 | S6-05, then S6-26 | S6-11 |
| 4 | S6-15 | S6-13 | S6-12 | S6-15, S6-12, S6-13 |
| 5 | S6-18 | S6-16, then S6-17 | S6-25 | S6-18, S6-16 |
| 6 | S6-20 | S6-19 | S6-14 | S6-20, S6-19 |
| 7 | S6-21, then S6-22 (J2) | – | – | S6-22 |
| 8 | S6-06 (needs the E2E stack alone) | stretch S6-29 | – | all P0 |
| 9 | S6-09 (documentation, last) | stretch S6-30 | – | sprint end |

After each report: the result goes to the control chat, the owner merges, the table goes on.

### Prompt 1 — put in front of every task prompt in window B or C

```text
Another session uses the local E2E stack at the same time. Do not start it; leave E2E to CI and say so in the protocol.
```

### Prompt 2 — subagent batch of wave 1 (window B)

```text
Run the tasks S6-02, S6-04 and S6-07 from docs/sprints/sprint_06/TASK_PROMPTS.md in parallel: one subagent per task, each in its own git worktree (isolation: worktree), model Sonnet 5.5.

Give each subagent the complete prompt block of its task (the text inside its ```text block) unchanged, plus these rules:
- You cannot ask the owner. Write the plan of step 2 at the top of your protocol instead of waiting for approval, then implement it.
- If a stop condition of step 2 applies (contradiction, missing UX key, owner decision, new dependency) or the plan needs more than one session: stop before pushing and report the question.
- Do not start the local E2E stack; leave E2E to CI.
- Push the branch and open the PR. Do not merge.

Wait for all three. Then give me one combined report: per task the PR link, protocol path, CI status, open questions and deviations. Check whether the PRs touch the same files; if they do, give me a merge order.
```

The model in this prompt is the owner's choice and may be changed.

### Prompt 3 — subagent batch of wave 2 (window C)

Prompt 2 with the first line changed to `Run the tasks S6-27 and S6-28 … model Opus 5.5.` O-S6-5 and O-S6-6 are filled in (2026-11-18).

### Prompt 4 — continue after a split (e.g. S6-10a merged)

```text
Continue with S6-10b as agreed in the plan of S6-10a (see docs/sprints/sprint_06/S6-10a_*.md, section 5). Use the prompt block of S6-10 in docs/sprints/sprint_06/TASK_PROMPTS.md with branch sprint06/S6-10b-<topic> and protocol S6-10b_<topic>.md. Start from an up-to-date main that contains S6-10a.
```

The same for S6-19 a/b or any other split, with its ID.

---

## P0 — committed

### S6-01 — Make main green: J5 session warnings and the J1 download flake

```text
Task S6-01 from docs/sprints/sprint_06/SPRINT_06.md — Make main green: J5 session warnings and the J1 download flake
Issues: #379, #380
Requirements / invariants: SEC-07, SEC-08, A11Y-01, A11Y-02 stay green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 379, gh issue view 380) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §1 and §3.4 (TD-55, TD-56); e2e/journeys/j5-session.spec.ts and the J1 journey; the session-warning code of S5-30 (CODEBASE §2, search S5-30); docs/sprints/sprint_05/S5-30_session_warning.md and S5-29_session_hardening.md; the failed CI runs named on #379 and #380

Owner decisions for this task: none
Additional instructions: First task of the sprint, alone on the E2E stack. Decide first whether the two alert dialogs on WebKit are a real double-open (two timers, two subscriptions, a second tab) or a test problem; a real double-open is fixed in the code with a unit test that fails before the fix. Then the Chromium count check after „Schließen“. For #380 find the cause of the download timeout on mobile Chrome; if it is the runner, propose a scoped retry or a longer timeout with the reason instead of hiding it. Done means three consecutive green CI runs on main after the merge (report the run links).

Way of working:
1. Branch sprint06/S6-01-main-green from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-01_main_green.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-02 — Quick wins: S16 gate, forced-colours edge, aria-label, health watcher, dead code

```text
Task S6-02 from docs/sprints/sprint_06/SPRINT_06.md — Quick wins: S16 gate, forced-colours edge, aria-label, health watcher, dead code
Issues: #375, #378, #350, #385
Requirements / invariants: A11Y-01, A11Y-09 stay green; TEN-09 stays green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 375, gh issue view 378, gh issue view 350, gh issue view 385) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.1, §3.3 (TD-64, TD-71); docs/sprints/sprint_05/S5-31_rating_legend.md (P on #375), S5-32_design_fidelity.md (#378), S5-30_session_warning.md (R4, TD-64)

Owner decisions for this task: none
Additional instructions: One PR, one commit per item. #375: a shared list that holds only „Geprüft nach …“ criteria must not show an empty S16; follow the issue. #378: no visible top edge of the sticky action bar in forced colours (check with the forced-colors emulation in Playwright). #350: German accessible name by key. TD-64: the health watcher fetches with credentials: 'omit' so it no longer renews the session. TD-71: delete only what TECH_DEBT_v5 lists as dead; list each deletion in the protocol. Do not run npm audit fix here (S6-08 owns the lock file).

Way of working:
1. Branch sprint06/S6-02-quick-wins from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-02_quick_wins.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-03 — Return to the target page after the sign-in

```text
Task S6-03 from docs/sprints/sprint_06/SPRINT_06.md — Return to the target page after the sign-in
Issues: #386
Requirements / invariants: SEC-03 stays green; R-AUTH-03
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 386) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.1 (TD-65); the auth guard, the login outlet and overlay of S5-09 and S5-30 (CODEBASE §2, search S5-09, S5-30); docs/sprints/sprint_05/S5-09_login_s01_overlay.md

Owner decisions for this task: none
Additional instructions: A signed-out user who opens a guarded deep link (e.g. /konto, /einstellungen, later /projekte/…) lands there after the sign-in. Accept only same-origin relative paths; reject absolute URLs, protocol-relative //host, backslashes and javascript: (open redirect). Unit tests for each rejected form and an E2E check for one guarded page. Keep the draft behaviour of „Liste speichern“ (R-SAV-01) unchanged.

Way of working:
1. Branch sprint06/S6-03-auth-return-url from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-03_auth_return_url.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-04 — Remove the legacy UI languages

```text
Task S6-04 from docs/sprints/sprint_06/SPRINT_06.md — Remove the legacy UI languages
Issues: #368, #158 (M3 entry)
Requirements / invariants: none; check-i18n stays green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 368, gh issue view 158) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.3 (#368) and §5 (LEGACY_EXCEPTIONS); docs/sprints/sprint_05/S5-25b_user_preferences_client.md (where #368 came from); eslint.config.js (LEGACY_EXCEPTIONS)

Owner decisions for this task: none
Additional instructions: Remove en.json, es.json, fr.json, the legacy language.service and every reference; German is the only UI language (SPEC). The M3 group of LEGACY_EXCEPTIONS must be empty afterwards; state the remaining counts per group in the protocol.

Way of working:
1. Branch sprint06/S6-04-legacy-languages from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-04_legacy_languages.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-05 — Mail and token hygiene: SMTP timeout, token cleanup, nginx error_log

```text
Task S6-05 from docs/sprints/sprint_06/SPRINT_06.md — Mail and token hygiene: SMTP timeout, token cleanup, nginx error_log
Issues: #382, #383, #381
Requirements / invariants: SEC-10, SEC-11, PRV-11, TEN-08 stay green
Depends on: S6-10 merged; O-S6-3

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 382, gh issue view 383, gh issue view 381) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.2 (TD-57, TD-58, TD-59); the e-mail sender of S5-11 and the account tokens of S5-12a/S5-13a (CODEBASE §2); docs/sprints/sprint_05/S5-11_email_sender.md (P3), S5-12a_registration_server.md (P4), S5-23_share_links_api.md (P1); the client nginx.conf

Owner decisions for this task: O-S6-3 (decided by the owner 2026-11-18): 🔧 `Email:Smtp:Timeout` = 15 s per SMTP operation, validated at startup and applied to the MailKit client.
Additional instructions: Three commits. TD-58: a configurable 🔧 timeout per SMTP operation (connect, auth, send) with the decided value; a test with a stalled fake server. TD-59: a hosted cleanup that deletes expired and used account tokens on a schedule, driven by TimeProvider and tested with FakeTimeProvider; say whether it needs an index (then it adds an IdentityAccess migration). TD-57: the client nginx must not write the request line with a share token to error_log (access_log is already handled); show the log of a 404 share request before and after.

Way of working:
1. Branch sprint06/S6-05-mail-token-hygiene from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-05_mail_token_hygiene.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-06 — Firefox crash after saving; S10 step-1 axe flake

```text
Task S6-06 from docs/sprints/sprint_06/SPRINT_06.md — Firefox crash after saving; S10 step-1 axe flake
Issues: #355, #339
Requirements / invariants: A11Y-01, A11Y-02 stay green
Depends on: nothing; only when no other session runs the E2E stack locally

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 355, gh issue view 339) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.4 (#355, #339); docs/sprints/sprint_05/S5-17b_invitations_client.md and the PR #348 (named regions and the Firefox crash in axe); e2e/configurator/s10-step1*.spec.ts

Owner decisions for this task: none
Additional instructions: Investigation task with a draft PR. #355: reproduce the Firefox page-process crash after saving a list, reduce it to the smallest case (component, axe rule, named region) and record cause and options on the issue; fix it only if the fix is small and certain. #339: make s10-step1 wait for the combobox to be ready before axe runs, without a fixed sleep. Report what remains for M5 screens (Firefox runs every new workspace page).

Way of working:
1. Branch sprint06/S6-06-firefox-crash-s10-flake from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-06_firefox_crash_s10_flake.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-07 — CI smoke: ng serve and the immutable header of lazy chunks

```text
Task S6-07 from docs/sprints/sprint_06/SPRINT_06.md — CI smoke: ng serve and the immutable header of lazy chunks
Issues: #388
Requirements / invariants: none
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 388) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.5 (TD-73); .github/workflows; docs/sprints/sprint_05/S5-01_quick_wins.md (#308) and S5-02_dev_server.md (#264)

Owner decisions for this task: none
Additional instructions: Add a CI step that starts ng serve and checks that the app renders (the #264 regression), and a check against the client image that main, styles and at least one lazy chunk carry Cache-Control immutable (the #308 regression). Keep the added CI time small and report it.

Way of working:
1. Branch sprint06/S6-07-ci-smoke from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-07_ci_smoke.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-08 — Dependabot batch and npm audit fix

```text
Task S6-08 from docs/sprints/sprint_06/SPRINT_06.md — Dependabot batch and npm audit fix
Issues: PR #363, #364, #365, #366
Requirements / invariants: none; every test level stays green
Depends on: S6-02 merged (lock file)

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh pr view 363, gh pr view 364, gh pr view 365, gh pr view 366) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.5 (TD-51, TD-76) and §3.2 (TD-17); CLAUDE §10 (licences); docs/sprints/sprint_05/S5-04_dependabot_majors.md; the Playwright container image tag in the CI workflow and docker-compose.e2e.yml; the material-symbols subset script

Owner decisions for this task: none
Additional instructions: One PR at a time, in this order: #366 (dotnet), #364 (angular), #365 (other: material-symbols needs the subset script run again), #363 (e2e: Playwright 1.65.1 needs the matching container image tag in CI and compose). Rebase each on the previous merge (TD-51). Regenerate THIRD-PARTY-NOTICES.txt where packages change. Then npm audit fix for the dev-only minimatch (TD-17) in its own commit. Do not merge: report per PR what changed and whether I can merge.

Way of working:
1. Branch sprint06/S6-08-dependabot-batch from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-08_dependabot_batch.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-09 — Documentation session: sprint-05 and sprint-06 findings

```text
Task S6-09 from docs/sprints/sprint_06/SPRINT_06.md — Documentation session: sprint-05 and sprint-06 findings
Issues: #389
Requirements / invariants: none
Depends on: every other P0 task merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 389) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.6 (TD-78) and the TECH_DEBT_v5 §6 list of owner documents (TD-79, not yours); every protocol in docs/sprints/sprint_05/ and docs/sprints/sprint_06/ (section 5, open points marked "documentation"); docs/sprints/sprint_05/S5-05_documentation_session.md (how the last session worked)

Owner decisions for this task: none
Additional instructions: Carry the findings of both sprints into TESTING, SPEC (§13 included), ARCHITECTURE, the Design handoff (screens as built), the Figma mapping and the code comments; update the document headers to name current versions. ROADMAP and the SPRINT_06 close-out are done in my control chat; PRD, Kriterienkatalog and UX copy are owner documents (TD-79) — list proposed changes for them in the protocol instead of editing them.

Way of working:
1. Branch sprint06/S6-09-documentation-session from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-09_documentation_session.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-10 — Move the users into IdentityAccess

```text
Task S6-10 from docs/sprints/sprint_06/SPRINT_06.md — Move the users into IdentityAccess
Issues: #387
Requirements / invariants: ARC-01 – ARC-03, OPS-03, SEC-01 – SEC-03, SEC-07 – SEC-11, TEN-01, TEN-02, TEN-05, TEN-11, PRV-07 stay green
Depends on: S6-01 merged; O-S6-1

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 387) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.3 (TD-72, TD-75) and §5 (the users row); ARCHITECTURE ADR-014 (rule 4), §4.3, §5.2; CODEBASE §2 (search S5-07, S5-12a, S5-15, S5-16b, S5-27, S5-28a, S4-25) and §3; docs/sprints/sprint_05/S5-15_organisations_tenancy.md (P5), S5-28a_account_deletion_server.md (R5), S5-14_e2e_users_account_journey.md

Owner decisions for this task: O-S6-1 (decided by the owner 2026-11-18): yes, at the start of M5; (a) the migration moves the Identity tables with their rows, so existing accounts (staging, local) sign in unchanged. No pre-pilot reset.
Additional instructions: This is the first task of the M5 server line and blocks it. Plan the split (a: Identity tables into the IdentityAccess context with the decided data handling and the four foreign keys; b: host endpoints API/Account, API/Auth, API/Organizations and GET /api/me into module endpoints, UserService.GetCurrentUserAsync, the roles command, the E2E seeder, TD-75). Show how the migrations of both contexts stay in order on a fresh database and on an existing one (staging has accounts), and how OPS-03 checks it. Routes and answers of the moved endpoints stay the same unless a SPEC route says otherwise; no client change unless a route changes. Workspace (S6-11) will refer to users only by id through IdentityAccess.Contracts — provide what it needs for display names (API-03 names the competing editor).

Way of working:
1. Branch sprint06/S6-10-users-identityaccess from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-10_users_identityaccess.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-11 — Workspace server: projects

```text
Task S6-11 from docs/sprints/sprint_06/SPRINT_06.md — Workspace server: projects
Issues: none
Requirements / invariants: R-PRJ-01, R-PRJ-05 (server); TEN-01 (new entity), TEN-04 workspace, TEN-05 (project rows)
Depends on: S6-10 merged (without O-S6-1: S6-01)

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-01 – R-PRJ-07, §3 (no deletion), §7.2 (field limits); ARCHITECTURE §4.3, §5.4 (workspace model), §7.1, §7.2, §12 step 5; TESTING §4.5, §4.6, §9 step 5; CODEBASE §2 (the Workspace module skeleton, search S2-06, and the organisation tenancy of S5-15)

Owner decisions for this task: none
Additional instructions: The Workspace module gets its own context and migration history (ADR-014). ProcurementProject is organisation-owned and covered by TEN-01 automatically; the organisation comes from the resource, never from a header (foreign → 404). Endpoints per SPEC/ARCHITECTURE routes: list (active organisation only, sort by name/date), create (name ≤ 120), read, rename, status (Laufend / Abgeschlossen / Archiviert); archived projects are read-only for everyone; no delete endpoint. Leave a seam for the ETag of S6-20 (no If-Match yet). The list answer carries the counts the S30 columns need (number of products, last change). no-store on every answer.

Way of working:
1. Branch sprint06/S6-11-projects-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-11_projects_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-12 — Project copy of a criteria list and S11 in project-copy mode

```text
Task S6-12 from docs/sprints/sprint_06/SPRINT_06.md — Project copy of a criteria list and S11 in project-copy mode
Issues: none
Requirements / invariants: R-PRJ-02 (copy), R-PRJ-04 (assign/replace), R-PRJ-06; WSP-03 active; WSP-02 copy part; TEN-08 project-copy part
Depends on: S6-11 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-02, R-PRJ-04, R-PRJ-06, R-SAV-*; ARCHITECTURE C51, C53, §5.4; TESTING WSP-02, WSP-03, TEN-08; CODEBASE §2 (search S5-19, S5-21a, S5-21b, S5-23); docs/sprints/sprint_05/S5-19_configurator_persistence.md (P5: ProcurementProjectId), S5-23_share_links_api.md (P4), S5-24_share_s15_s16.md (P4)

Owner decisions for this task: none
Additional instructions: Server: PUT /api/v1/projects/{id}/criteria-configuration copies a saved list of the same organisation with ProcurementProjectId set; the source stays unchanged; a project copy is never offered as a source (saved-list list and S31) and a share link for it answers 409. Replacing works while no assessment exists (the lock follows in S6-18 — leave a clear hook). Client: S11 opened from a project shows the copy with criteria.list.projectCopy; the four selection layers are fixed („Auswahl ändern“ hidden), include/exclude, priority and note work; on return to S32 projects.detail.criteriaAdded appears after criteria were added. Say whether the task adds a Configurator migration (a key for ProcurementProjectId) and coordinate with S6-15/S6-18 on the Workspace side.

Way of working:
1. Branch sprint06/S6-12-project-copy from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-12_project_copy.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-13 — Project list S30 and create dialog S31

```text
Task S6-13 from docs/sprints/sprint_06/SPRINT_06.md — Project list S30 and create dialog S31
Issues: none
Requirements / invariants: R-PRJ-01, R-PRJ-02 (UI), R-SHL-02 (Projekte link); A11Y-01, A11Y-09
Depends on: S6-11 merged; S6-24 preferably merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-01, R-PRJ-02, R-SHL-02; Design handoff §5.3 (S14/S30 lists, S12/S15/S31 dialogs); ARCHITECTURE §8.1 (withWorkspace); UX copy projects.*; CODEBASE §2 (search S5-20b for S14 as built, S5-18 for the header)

Owner decisions for this task: none
Additional instructions: S30 and S31 have no Figma frame: build them from Handoff §5.3 like S14 and S12. Add the withWorkspace() store slice per ARCHITECTURE §8.1 with the root-store pattern of S6-24 if merged (otherwise follow the current pattern and note it). Routes under /projekte (German path segments as the other pages); the header link „Projekte“ leads to S30. S30: name, status, number of products, last change; sort by name/date; projects.empty; „Archivierte anzeigen“; results announced. S31: name, optional organisation list (saved lists of the active organisation only, no project copies) with projects.create.criteria.hint; on success open S32 (placeholder until S6-14).

Way of working:
1. Branch sprint06/S6-13-projects-s30-s31 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-13_projects_s30_s31.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-14 — Project overview S32

```text
Task S6-14 from docs/sprints/sprint_06/SPRINT_06.md — Project overview S32
Issues: none
Requirements / invariants: R-PRJ-03, R-PRJ-05 (UI); SIG-13 (UI part); A11Y-01, A11Y-09
Depends on: S6-12, S6-13 and S6-18 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-03, R-PRJ-05, SIG-13; Design handoff §5.1 (S32); Figma mapping §2 (S32: 360 2148:4697, desktop 2148:3877); UX copy projects.detail.*

Owner decisions for this task: none
Additional instructions: Compare with both frames before building and before pushing. Criteria card with the scope row („Geprüft nach …“ named separately, .scope.value) and the link to S11 in project-copy mode; counts row projects.detail.row.counts with seven values, label first; product list where the product name is the only link to S34; add-product menu (create a candidate → S33; „Aus dem Verzeichnis“ is M6 and stays hidden or disabled as the Handoff says — ask if unclear); rename and status change; archived projects read-only.

Way of working:
1. Branch sprint06/S6-14-project-overview-s32 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-14_project_overview_s32.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-15 — Candidates server: candidate, test status and evidence

```text
Task S6-15 from docs/sprints/sprint_06/SPRINT_06.md — Candidates server: candidate, test status and evidence
Issues: none
Requirements / invariants: R-CAN-02, R-CAN-05 (server); WSP-01, SIG-03 (workspace part) active; TEN-01, TEN-05 (candidate rows)
Depends on: S6-11 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-02 – R-CAN-05; ARCHITECTURE §4.4, §5.4, §5.5 (private section, PRV rules of step 6); TESTING WSP-01, SIG-03, PRV-01, PRV-02; CODEBASE §2 (the Workspace module after S6-11)

Owner decisions for this task: none
Additional instructions: CandidateProduct belongs to a project (and through it to the organisation): name, vendor, website, functional summary; the private section (contacts, internal notes) is stored apart so the step-6 redaction (PRV-01/02) can never copy it. Data for the name-match hint (candidate.form.name.match) without linking to the directory yet (step 6). TestStatus tri-state with default Unknown; TestEvidence 0..n, at most one per TestTypeId, rejected while TestStatus ≠ Tested, „Andere Prüfung“ needs a name, a new product version updates the row; setting NotTested or Unknown with evidences → 409 with field error. Workspace migration; one open Workspace migration branch at a time.

Way of working:
1. Branch sprint06/S6-15-candidates-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-15_candidates_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-16 — Candidate form S33 and S34 with tabs and overview

```text
Task S6-16 from docs/sprints/sprint_06/SPRINT_06.md — Candidate form S33 and S34 with tabs and overview
Issues: none
Requirements / invariants: R-CAN-02 (UI), R-CAN-03, R-CAN-04; A11Y-01, A11Y-09
Depends on: S6-15 and S6-13 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-02 – R-CAN-04; Design handoff §5.1 (S34), §5.3 (S33); Figma mapping §2 (S34: 360 2134:3184 Überblick, desktop 2097:2627); UX copy candidate.*

Owner decisions for this task: none
Additional instructions: S33 has no frame: S34a title row and tabs, private section visually separated with candidate.form.privateSection.text (Handoff §5.3). S34: tabs as routes (Überblick default, Prüfnachweis, Einschätzung nach Kriterien) with mat-tab-nav-bar, short labels below 600 px, a page title per tab (pageTitle.candidate). Overview: facts only, no verdict; K1 workspace scope with seven rows; publication card with candidate.overview.publication.none (the published states come in step 6). Prüfnachweis and Einschätzung tabs may show a placeholder until S6-17 and S6-19.

Way of working:
1. Branch sprint06/S6-16-candidate-s33-s34 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-16_candidate_s33_s34.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-17 — Evidence tab S34a

```text
Task S6-17 from docs/sprints/sprint_06/SPRINT_06.md — Evidence tab S34a
Issues: none
Requirements / invariants: R-CAN-05 (UI); WSP-01 stays green; A11Y-01, A11Y-09
Depends on: S6-16 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-05; Design handoff §5.1 (S34a); Figma mapping §2 (S34a: 2022:267, 2094:2525 S34a-J, 2144:3508 S34a-L several evidences); UX copy candidate.evidence.*

Owner decisions for this task: none
Additional instructions: Compare with the frames. Test status radio group (Ja / Nein / Nicht bekannt); evidence cards with H3 and fields in fixed order; „Andere Prüfung“ requires a name; duplicate procedure → candidate.evidence.list.duplicate; switching away from „Ja“ with evidences asks to remove them (the server answers 409 with a field error).

Way of working:
1. Branch sprint06/S6-17-evidence-s34a from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-17_evidence_s34a.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-18 — Assessment server: rows, allowed values, derived rows, counts, lock

```text
Task S6-18 from docs/sprints/sprint_06/SPRINT_06.md — Assessment server: rows, allowed values, derived rows, counts, lock
Issues: none
Requirements / invariants: R-CAN-06 – R-CAN-08 (server), R-PRJ-04 (lock); CAT-15, CAT-17 (workspace), SIG-13, WSP-02 active; WSP-03 stays green
Depends on: S6-15 and S6-12 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-06 – R-CAN-08, R-PRJ-04; ARCHITECTURE C34, C51, C53, §5.4; TESTING CAT-15, CAT-17, SIG-13, WSP-02, WSP-03; CODEBASE §2 (catalog allowed ratings and procedure texts, search S3-20a, S3-20b)

Owner decisions for this task: none
Additional instructions: CandidateAssessment rows are created lazily (no row = Unknown); a value is validated against the union of AllowedRatings of the procedure texts of the project copy's app types (Unknown always allowed); note ≤ 1 000; last-write-wins per row (no If-Match on these rows, R-CAN-11). Procedure criteria never get rows; their answer is derived from the evidences and TestStatus and is never counted. Progress counts requirement criteria only; project and candidate views return exactly seven count properties and never notCovered, pinned to the copy's release (SIG-13). The lock: replacing the project copy returns 409 once any candidate has an assessment ≠ Unknown (WSP-02 complete); excluded criteria keep their assessments and reappear unchanged.

Way of working:
1. Branch sprint06/S6-18-assessment-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-18_assessment_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-19 — Assessment tab S34b

```text
Task S6-19 from docs/sprints/sprint_06/SPRINT_06.md — Assessment tab S34b
Issues: none
Requirements / invariants: R-CAN-06 – R-CAN-08 (UI); A11Y-01, A11Y-02, A11Y-09
Depends on: S6-18 and S6-16 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-06 – R-CAN-08; Design handoff §5.1 (S34b), §4.11 (pa-rating-legend); Figma mapping §2 (S34: 360 2136:3127 Einschätzung, desktop 2022:841; the compact K6 form below 600 px, §2.1); UX copy candidate.assessment.*; CODEBASE §2 (search S5-31 rating legend, S4-14 – S4-18b S11 cards and K5)

Owner decisions for this task: none
Additional instructions: Compare with both frames. K6 per requirement criterion with the allowed values only (radios; compact form below 600 px via BreakpointObserver as the Figma mapping says); autosave with an 800 ms debounce; candidate.assessment.saved at most every 5 s through the announcer; note per criterion; derived „Geprüft nach …“ rows read-only with a link to S34a; progress („36 von 46“); filter „Nur noch nicht eingeschätzte“; the rating legend expanded until the first assessment, its state in withSettings(). K5 „Warum diese Priorität?“ outside a detail panel has no UX key for the hide state yet (TECH_DEBT_v5 §5) — ask before inventing one. Keyboard-only autosave test. Plan a split if needed (a: K6 rows and autosave; b: progress, filter, legend).

Way of working:
1. Branch sprint06/S6-19-assessment-s34b from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-19_assessment_s34b.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-20 — Optimistic locking: ETag / If-Match for workspace resources and saved lists

```text
Task S6-20 from docs/sprints/sprint_06/SPRINT_06.md — Optimistic locking: ETag / If-Match for workspace resources and saved lists
Issues: none
Requirements / invariants: R-PRJ-07, R-CAN-11 (server); API-03, API-04 active
Depends on: S6-18 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-PRJ-07, R-CAN-11, §7.1; ARCHITECTURE §7.1, §9.2 (ETagEpoch, restore safety); TESTING API-03, API-04; docs/sprints/sprint_05/S5-21a_edit_delete_server.md (R1) and S5-21b_edit_delete_client.md (P2)

Owner decisions for this task: none
Additional instructions: ETags carry the configured ETagEpoch; changing it invalidates every issued ETag. Updates to project, project copy, candidate, evidence and saved lists without If-Match → 428 Problem Details; a stale ETag → 409 with the competing editor's display name (from IdentityAccess.Contracts). Assessment rows stay last-write-wins. Include the saved-list race of S5-21a R1 (PATCH …/criteria/{id} racing PUT …/selection answers 500 today) as a test that now gets 409 or 412. Update the client calls that change these resources so they send If-Match (the dialog comes in S6-21). Say whether the concurrency token needs migrations (Workspace, Configurator) — one open branch per context.

Way of working:
1. Branch sprint06/S6-20-etag-concurrency from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-20_etag_concurrency.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-21 — Conflict dialog S37

```text
Task S6-21 from docs/sprints/sprint_06/SPRINT_06.md — Conflict dialog S37
Issues: none
Requirements / invariants: R-CAN-11 (UI); API-03 stays green; A11Y-01
Depends on: S6-20 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-11; Design handoff §5.1 (S37 behaviour); Figma mapping §2 (S37: 2056:2519, shared/components/conflict-dialog/); UX copy conflict.*

Owner decisions for this task: none
Additional instructions: Compare with the frame. A shared component used by the workspace forms (project, copy on S11, candidate, evidence) and the saved-list edits on S11; dialog rules of S13 (initial focus, return focus). E2E with two browser contexts: the second save of the same project gets S37 with the other editor's name, and both choices of the dialog work.

Way of working:
1. Branch sprint06/S6-21-conflict-dialog-s37 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-21_conflict_dialog_s37.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-22 — Remove a candidate, Readers read-only, J2 up to the assessment

```text
Task S6-22 from docs/sprints/sprint_06/SPRINT_06.md — Remove a candidate, Readers read-only, J2 up to the assessment
Issues: none
Requirements / invariants: R-CAN-09, R-CAN-10; TEN-05 complete, WSP-04 (workspace part) active; A11Y-02 (J2)
Depends on: S6-19 and S6-21 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CAN-09, R-CAN-10, journey J2; TESTING TEN-05, WSP-04, §6 (E2E seeder); UX copy candidate.remove.*; CODEBASE §2 (search S5-14 E2E users)

Owner decisions for this task: none
Additional instructions: Server: DELETE /api/v1/candidates/{id} for Owner and Editor deletes evidences and assessments (the published-entry part of WSP-04 is step 6 — record it as deferred). Client: confirmation dialog with candidate.remove.*; Readers see every tab read-only (no inputs, no publish button, no add-product menu). TEN-05: complete the matrix for every workspace endpoint (Owner / Editor / Reader / other org / no org / anonymous). J2 up to the assessment as E2E journey with axe in both appearances on all five browser projects: create project → assign a list → adjust it → add candidate → evidence → assessment → Reader view. This is the M5 exit check.

Way of working:
1. Branch sprint06/S6-22-remove-readonly-j2 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-22_remove_readonly_j2.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-23 — Retire the proposal workflow

```text
Task S6-23 from docs/sprints/sprint_06/SPRINT_06.md — Retire the proposal workflow
Issues: #384, #158 (proposal entries), #82 (check)
Requirements / invariants: ARC-05 active; API-02 stays green (moved first); PRV-07 stays green
Depends on: S6-10 merged (legacy context migration); O-S6-4

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 384, gh issue view 158, gh issue view 82) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §8.1, §12 step 5; TECH_DEBT_v5 §3.2 (TD-61) and §5 (ProposalService, DateTime.UtcNow, API-02 test, LiveAnnouncer, withProposal()); TESTING API-02, ARC-05; CODEBASE §2 (search S4-25 for moderation:review)

Owner decisions for this task: O-S6-4 (decided by the owner 2026-11-18): (a) retire the proposals and drop their tables with M5.6 (pre-pilot data, nothing to export); no export or deletion code for proposals. TD-61 and #384 close by deletion.
Additional instructions: First commit: move the tagged API-02 test from the legacy proposal form to a rebuilt form. Then remove ProposalService, ProposalsController, withProposal(), the proposal pages, routes, forms and the proposal-list LiveAnnouncer, and (with decision (a)) the proposal tables in a legacy-context migration; PlatformAdmin loses moderation:review; Proposal.ApproverId goes with the table. Repoint withProducts() / withFavorites() per ARCHITECTURE §8.1. ARC-05: BaseCrudController only for catalog-admin controllers. The „Kriterium vorschlagen“ link (R-SUG-01) is not part of the proposal workflow and stays. Check which parts of #82 disappear with the removed screens and comment the rest on #82.

Way of working:
1. Branch sprint06/S6-23-retire-proposals from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-23_retire_proposals.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-24 — Root store and the M5 entries of LEGACY_EXCEPTIONS

```text
Task S6-24 from docs/sprints/sprint_06/SPRINT_06.md — Root store and the M5 entries of LEGACY_EXCEPTIONS
Issues: #158 (M5 entries)
Requirements / invariants: none; every client test stays green
Depends on: nothing

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-24 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 158) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §5 (withLoading(), legacy store callers, LEGACY_EXCEPTIONS); ARCHITECTURE §8.1; docs/sprints/sprint_05/S5-02_dev_server.md (P3); eslint.config.js

Owner decisions for this task: none
Additional instructions: withLoading() once in the root store instead of per feature, so dev mode no longer warns "SignalStore members cannot be overridden"; the callers of the fixed legacy store methods catch the rejection; legacy <strong> in mat-error removed. Remove every M5 entry of LEGACY_EXCEPTIONS whose file you change; files that S6-23 deletes stay for S6-23 — list in the protocol which entry goes with which task.

Way of working:
1. Branch sprint06/S6-24-root-store from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-24_root_store.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-25 — Account deletion and data export: projects

```text
Task S6-25 from docs/sprints/sprint_06/SPRINT_06.md — Account deletion and data export: projects
Issues: none
Requirements / invariants: PRV-07 (workspace part) active; TEN-11 stays green
Depends on: S6-15 and S6-10 merged

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-25 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-09, R-AUTH-10, §8.4; TESTING PRV-07, TEN-11; TECH_DEBT_v5 §5 (account deletion parts of later steps); docs/sprints/sprint_05/S5-27_personal_data_export.md (P3), S5-28a_account_deletion_server.md (P3)

Owner decisions for this task: none
Additional instructions: When the last member of an organisation leaves or deletes the account, its projects go with candidates, evidences, assessments and project copies (as the lists and share links do today). GET /api/v1/account/export gains createdByMe.projects per SPEC §8.4 (only projects the user created; no other user's identity). Extend the deletion and export tests; the step-6 parts (contributions, profiles) stay deferred.

Way of working:
1. Branch sprint06/S6-25-account-projects from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-25_account_projects.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-26 — Rate limits for sign-in and organisation creation

```text
Task S6-26 from docs/sprints/sprint_06/SPRINT_06.md — Rate limits for sign-in and organisation creation
Issues: #318
Requirements / invariants: SEC-11 stays green; API-05 stays green
Depends on: S6-10 merged; O-S6-2

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-26 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 318) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC §7.3; TECH_DEBT_v5 §3.2 (#318, TD-60); TESTING §12.5 (rate limits in E2E); CODEBASE §2 (search S5-13a for the reset limit, S4-19 for the export limit); docs/sprints/sprint_05/S5-07_cookie_session_server.md (P4), S5-16a_organisation_server.md (P3)

Owner decisions for this task: O-S6-2 (decided by the owner 2026-11-18), all 🔧 and added to SPEC §7.3: sign-in 30 per 10 minutes per client IP and 10 per hour per e-mail address (not "like the reset": 5 per hour per IP would lock out an office behind one shared IP; the account lockout keeps protecting each account); organisation creation 5 per day per user.
Additional instructions: Sign-in limited per IP and per address like the password reset; organisation creation per user; both 🔧 with the decided values; 429 Problem Details mapped to the existing UX key (ask if none fits). Relax them in the E2E stack like the export limit. Tests on the fake clock. Keep RateLimits:Invitation unchanged.

Way of working:
1. Branch sprint06/S6-26-rate-limits from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-26_rate_limits.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-27 — Reload flows: failed lazy chunk and release-mismatch

```text
Task S6-27 from docs/sprints/sprint_06/SPRINT_06.md — Reload flows: failed lazy chunk and release-mismatch
Issues: #296, #310
Requirements / invariants: none; A11Y-01 stays green
Depends on: O-S6-5

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-27 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 296, gh issue view 310) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v5 §3.1, §9.4 (#296, TD-39); the issues #296 and #310; CODEBASE §2 (search S4-20 for the S13 export dialog and release-mismatch)

Owner decisions for this task: O-S6-5 (decided by the owner 2026-11-18): (1) failed lazy chunk (#296): one automatic full reload per target URL, guarded against loops, never while offline (the offline notice stays); if it fails again, the error page. (2) The in-memory S10 draft may be lost on that reload; no warning (`criteria.list.draftNote` already says a reload loses it). (3) S13 after `release-mismatch` (#310) follows the confirmed UX copy, not #310's proposal to keep per-criterion decisions: drop the pinned release, keep the S10 selection (use case, app types, product parts, test procedures), reload S11 for the active release; per-criterion changes are dropped as `criteria.export.outdated` says; if the selection is now excluded, back to S10 with `criteria.select.error.excluded`. The action in S13 is a new button `criteria.export.outdated.reload` „Kriterien neu laden“ — key and wording confirmed by the owner; add it to a new UX copy version and to de.json.
Additional instructions: When a lazy route chunk fails to load after a deployment, reload once (guarded so it can never loop; no persistent storage for signed-out users — use the URL or the session history as the guard) and show the error page if it fails again. S13: after release-mismatch reload the criteria list as O-S6-5 decides. Unit tests for both guards.

Way of working:
1. Branch sprint06/S6-27-reload-flows from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-27_reload_flows.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-28 — Legal pages from Markdown with the placeholder check

```text
Task S6-28 from docs/sprints/sprint_06/SPRINT_06.md — Legal pages from Markdown with the placeholder check
Issues: none
Requirements / invariants: R-SHL-07; A11Y-01, A11Y-09
Depends on: O-S6-6

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-28 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SHL-07, §11 (release prerequisites); TECH_DEBT_v5 §9.4 (TD-29); scripts/check-release-config.mjs; the footer of S2-15 (CODEBASE §2)

Owner decisions for this task: O-S6-6 (decided by the owner 2026-11-18): (a) three Markdown files in the client at `CLIENT/CLIENT.ProcureAccess/content/legal/*.de.md` (the client image is built from the client folder and cannot reach a root `content/legal/`; correct the CLAUDE.md target layout line), rendered with the existing `ngx-markdown`/`marked` (no new dependency); a placeholder marker shows `legal.placeholder` outside production and fails the production build through `check-release-config.mjs`. Content follows from WERTE.IT before G-P.
Additional instructions: Impressum, Datenschutzerklärung and Erklärung zur Barrierefreiheit rendered from Markdown files at the footer links (headings as real headings, page title and H1 focus like every page). The texts are placeholders until WERTE.IT delivers them: development shows the placeholder, the production build (check-release-config.mjs) fails while any placeholder is left, with a CI test of that failure. No new Markdown dependency without my OK — ngx-markdown is legacy and goes with the legacy filter; propose the smallest option (build-time conversion or an existing dependency).

Way of working:
1. Branch sprint06/S6-28-legal-pages from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-28_legal_pages.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## P2 — stretch

### S6-29 — Nightly backup and restore rehearsal *(stretch)*

```text
Task S6-29 from docs/sprints/sprint_06/SPRINT_06.md — Nightly backup and restore rehearsal
Issues: none
Requirements / invariants: R-ADM-04 (completion)
Depends on: every P0 task

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-29 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-ADM-04; ARCHITECTURE §9.5, §10.2, §7.1 (ETagEpoch after a restore); TESTING API-04; docker-compose.yml

Owner decisions for this task: none
Additional instructions: Stretch. A nightly pg_dump of every schema with retention (🔧), as a compose service or a script the host runs; a restore script and a documented rehearsal (restore into a fresh database, start the API, sign in, open a project; bump ETagEpoch as ARCHITECTURE says). Run the rehearsal locally and record it; the staging run comes with GP.1. Secrets only from the environment.

Way of working:
1. Branch sprint06/S6-29-backup-restore from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-29_backup_restore.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```
### S6-30 — TECHNICAL.md and README *(stretch)*

```text
Task S6-30 from docs/sprints/sprint_06/SPRINT_06.md — TECHNICAL.md and README
Issues: #4
Requirements / invariants: R-MET-02
Depends on: every P0 task; S6-29 if done

Read first: CLAUDE.md and the CODEBASE §2 sections of the areas this task touches; in SPRINT_06.md the row of S6-30 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 4) and their entries in TECH_DEBT_v5.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ai_development_process.md (step 16); SPEC R-MET-02; ARCHITECTURE §9.5, §10; CLAUDE.md §5 and CODEBASE §4 (commands); TECH_DEBT_v5 §9.4

Owner decisions for this task: none
Additional instructions: Stretch. TECHNICAL.md in docs/ for the next developer and for operations: architecture in short with pointers, local setup, configuration and secrets, deployment, backup and restore, logs and health, the documented metric queries of R-MET-02, the catalog and roles commands. A README for the public repository (#4): what ProcureAccess is, status, licence, how to start, where the documents are. English, no internal team context (the repository is public).

Way of working:
1. Branch sprint06/S6-30-technical-readme from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S5-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:WSP-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_06/S6-30_technical_readme.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

Replace `<S>` with the sprint number (e.g. 7), `<SS>` with the two-digit sprint folder number (e.g. 07) and `<v>` with the current tech-debt register version. Name documents without version numbers. Check that new decision IDs are not used yet.

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
5. Write the protocol docs/sprints/sprint_<SS>/S<S>-<NN>_<topic>.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CODEBASE §2 paragraph of the area and §3 layout; CLAUDE.md only for a rule, command or convention that applies across the codebase; ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

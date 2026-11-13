# Sprint 05 — task prompts for Claude Code

> **Status:** v1 (2026-11-13). Companion to `SPRINT_05.md`: one ready-to-paste prompt per task, built from the generic template at the end of `docs/sprints/sprint_04/TASK_PROMPTS.md` (repeated, updated, at the end of this file). Claude Code implements; the owner's control chat for sprint 05 takes the decisions and reviews the reports. The model per task is the owner's choice and is not part of the prompts.
> **How to use:** copy the block of the task into a new Claude Code session (or a worktree session for parallel work). Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>`, `<Name>` or `<date>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_05.md`, `TECH_DEBT_v4.md` and the GitHub issues and pull requests as of 2026-11-13.
> **Changes to the template since sprint 04:** reads `TECH_DEBT_v4.md`; step 2 asks whether the task adds an EF migration and to which module, and names Handoff §5.3 for screens without a Figma frame; step 3 rebases and regenerates a migration after another PR merged one in the same module. Decision IDs `O-S5-n` are this sprint's; the SPEC open question O-S5 (dark mode) keeps its name.

---

## Overview

Prompts are listed in task-ID order; the working order and the parallel lines are in `SPRINT_05.md` (Sequence) and below.

| Task | Title | Issues | Can start |
|---|---|---|---|
| S5-01 | Quick wins: S10 step-line checks, nginx immutable pattern | #309, #308 | ready |
| S5-02 | Local dev server: npm start renders again | #264 | ready |
| S5-03 | Pin the .NET SDK | #306 | after O-S5-1 |
| S5-04 | Review the Dependabot majors MailKit 5 and Swashbuckle 11 | PR #288 (MailKit 5.0.0), PR #289, #291, #292 (Swashbuckle 11) | after O-S5-2 |
| S5-05 | Documentation session: sprint-04 findings | #313 | last (after the M3 document changes) |
| S5-06 | Reflow check with a two-procedure card | #312 | ready |
| S5-07 | Server: Identity cookie session with antiforgery | #87, #88, #89 | ready (first big task) |
| S5-08 | Client: session state, interceptor, JWT removed | none | after S5-07 |
| S5-09 | Sign-in S01, login overlay and sign-out | #170 (L1, login outlet) | after S5-08 |
| S5-10 | Legacy auth cleanup | #158 (auth entries) | after S5-09, S5-12, S5-13 and O-S5-4 |
| S5-11 | IEmailSender with MailKit, Mailpit and real recipients | #302 | after S5-04, S5-07 |
| S5-12 | Registration S02 and the e-mail confirmation gate | #301 | after S5-11 and O-S5-3 |
| S5-13 | Password reset S03 and lockout | none | after S5-11 |
| S5-14 | E2E users and the account journey | none | after S5-12, S5-13 |
| S5-15 | Organisations: domain, query filters, isolation tests | none | after S5-07 (O-S5-5 if needed) |
| S5-16 | Create organisation S20, members page S21, leave | none | after S5-15, S5-12 |
| S5-17 | Invitations by e-mail | none | after S5-15, S5-11 |
| S5-18 | Signed-in header states and organisation switch | none | after S5-16, S5-09 |
| S5-19 | Configurator persistence and PersistedConfigurationSource | none | after S5-15 |
| S5-20 | Save dialog S12, saved lists S14, J1 second part | none | after S5-19, S5-09 |
| S5-21 | Edit, delete and export saved lists; older release | none | after S5-20 |
| S5-22 | Saved-list debt: weight range, too-long note, unsaved state, no-store | none | after S5-20 |
| S5-23 | Share links: model, management API, anonymous read | none | after S5-19 |
| S5-24 | Share dialog S15 and shared list S16 | none | after S5-23 |
| S5-25 | User preferences and the end of the legacy settings store | #234 (TD-25 part) | after S5-07 |
| S5-26 | Settings page S60 | none | after S5-25 |
| S5-27 | Personal data export | none | after S5-15 |
| S5-28 | Account page S61 and account deletion | #234 (TD-26 part) | after S5-16, S5-21, S5-23, S5-27 |
| S5-29 | Session hardening (M4.1) | none | stretch, after S5-09 (S5-28 for deletion) |
| S5-30 | Session warning dialog (M4.2) | none | stretch, after S5-29 |
| S5-31 | Rating legend on S11 | #279 | stretch, ready |
| S5-32 | Design fidelity of S10 and S11 | #278, #281, #282 | stretch, ready |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S5-1 (SDK pin) | S5-03 | owner choice (D1) |
| O-S5-2 (Dependabot majors) | S5-04 | owner choice (D1, before S5-11) |
| O-S5-3 (#301 Member role) | S5-12 | owner choice (before S5-12) |
| O-S5-4 (legacy account pages) | S5-10 | owner choice (before S5-10) |
| O-S5-5 (O-22, contracts → contracts) | S5-15, S5-19 | owner choice, or when S5-15 asks |

## Rules for running tasks

- **S5-07 first among the big tasks:** the server session is the base of every M3 task; it runs alone in the IdentityAccess module.
- **Parallel lines after S5-07:** client (S5-08 → S5-09), accounts (S5-11 → S5-12, S5-13 → S5-14), organisations (S5-15 → S5-16, S5-17 → S5-18), later saved lists (S5-19 → S5-20 → S5-21/S5-22, S5-23 → S5-24), settings (S5-25 → S5-26), account (S5-27 → S5-28). Run them in separate worktrees (`claude --worktree s5-nn`).
- **EF migrations:** only one open branch with a migration per module (IdentityAccess: S5-07 if it adds tables, S5-12, S5-13, S5-15, S5-17, S5-25; Configurator: S5-19, S5-23). The later branch rebases and regenerates.
- **TESTING versions:** almost every task flips an invariant; the later PR rebases and renumbers.
- **Local E2E:** one session at a time (shared host ports); the others leave E2E to CI.
- **Stretch tasks** (S5-29 – S5-32) only after all P0 tasks are done.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_05.md` and `TECH_DEBT_v5.md` (process step 14), prepared in the control chat.

---

## P0 — committed

### S5-01 — Quick wins: S10 step-line checks, nginx immutable pattern

```text
Task S5-01 from docs/sprints/sprint_05/SPRINT_05.md — Quick wins: S10 step-line checks, nginx immutable pattern
Issues: #309, #308
Requirements / invariants: A11Y-01 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 309, gh issue view 308) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.4 (TD-38) and §3.3 (#308); e2e/configurator/s10-steps-2-4.spec.ts; CLIENT nginx.conf; docs/sprints/sprint_04/S4-28_dead_code.md (R2)

Owner decisions for this task: none
Additional instructions: One PR, one commit per issue. #309: scope the step-line checks to #main (or the visible element) so the CDK live-announcer element is never matched; check every E2E spec for the same pattern. #308: widen the nginx immutable rule to every hashed chunk name Angular emits (lowercase, _ and - in the hash) and show the response headers of the client image for main, styles and two lazy chunks in the protocol.

Way of working:
1. Branch sprint05/S5-01-quick-wins from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-01_quick_wins.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-02 — Local dev server: npm start renders again

```text
Task S5-02 from docs/sprints/sprint_05/SPRINT_05.md — Local dev server: npm start renders again
Issues: #264
Requirements / invariants: none
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 264) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.3 (#264); CLIENT app.config.ts and the legacy routes that render Markdown; docs/sprints/sprint_04/S4-22_j1_export_m2_exit.md (P4)

Owner decisions for this task: none
Additional instructions: Make ng serve render the app: provideMarkdown() only on the legacy routes that need it (preferred, it also takes ≈ 75 kB out of the first download), or a stub for the optional marked-katex-extension import. Show npm start working and the size of the initial bundle before and after.

Way of working:
1. Branch sprint05/S5-02-dev-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-02_dev_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-03 — Pin the .NET SDK

```text
Task S5-03 from docs/sprints/sprint_05/SPRINT_05.md — Pin the .NET SDK
Issues: #306
Requirements / invariants: none (CI)
Depends on: O-S5-1

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 306) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.2 (#306); global.json; the API Dockerfile; .github/workflows/ci.yml; docs/sprints/sprint_04/S4-27_nuget_licence_notices.md (P2)

Owner decisions for this task: O-S5-1: <pin global.json and the image tag to the same SDK patch | other>
Additional instructions: Pin per O-S5-1 in global.json, the SDK image tag and the setup-dotnet step of CI; make Dependabot update them together (or note why it can't). Regenerate THIRD-PARTY-NOTICES.txt if the SDK change alters it, and show the notices check green.

Way of working:
1. Branch sprint05/S5-03-sdk-pin from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-03_sdk_pin.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-04 — Review the Dependabot majors MailKit 5 and Swashbuckle 11

```text
Task S5-04 from docs/sprints/sprint_05/SPRINT_05.md — Review the Dependabot majors MailKit 5 and Swashbuckle 11
Issues: PR #288 (MailKit 5.0.0), PR #289, #291, #292 (Swashbuckle 11)
Requirements / invariants: CLAUDE §10 (licences); the notices check
Depends on: O-S5-2; must be settled before S5-11

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the pull requests (gh pr view 288, gh pr view 289, gh pr view 291, gh pr view 292); always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.2 (TD-42) and §6; docs/sprints/sprint_04/S4-06_dependabot_prs.md; docs/sprints/sprint_04/S4-27_nuget_licence_notices.md (P1, EMBEDDED review); THIRD-PARTY-NOTICES.txt

Owner decisions for this task: O-S5-2: <MailKit 5: take | ignore the major>; <Swashbuckle 11: one PR for all four packages | ignore the major>
Additional instructions: This task reviews pull requests: steps 1 and 3 of the way of working apply only to the combined Swashbuckle PR if O-S5-2 asks for one. Per package: licence, changelog for breaking changes, CI, the regenerated THIRD-PARTY-NOTICES.txt, and for SwaggerUI the EMBEDDED review of the bundled swagger-ui-dist. Include any other open Dependabot PRs. Recommend merge or close per PR; do not merge.

Way of working:
1. Branch sprint05/S5-04-dependabot-majors from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-04_dependabot_majors.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-05 — Documentation session: sprint-04 findings

```text
Task S5-05 from docs/sprints/sprint_05/SPRINT_05.md — Documentation session: sprint-04 findings
Issues: #313
Requirements / invariants: none (documents only)
Depends on: best as the last task of the sprint, after the M3 documents changes

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 313) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.6 (TD-50, the full list of points); the open points named there in docs/sprints/sprint_04/

Owner decisions for this task: none
Additional instructions: Carry the TD-50 points into the next versions of SPEC (current-versions line), ARCHITECTURE, TESTING, the Design handoff (S10 no longer mat-stepper, and every other sprint-04 change of a screen) and the Figma mapping, and make every header name the current versions of the others. ROADMAP_v5 and the SPRINT_04 close-out are done; do not touch them. PRD, Kriterienkatalog and UX copy are the owner's documents: list their points in the protocol. Do not change any rule or requirement beyond the listed points.

Way of working:
1. Branch sprint05/S5-05-documentation-session from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-05_documentation_session.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-06 — Reflow check with a two-procedure card

```text
Task S5-06 from docs/sprints/sprint_05/SPRINT_05.md — Reflow check with a two-procedure card
Issues: #312
Requirements / invariants: A11Y-09 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 312) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.4 (TD-54); e2e reflow.spec.ts; docs/sprints/sprint_04/S4-15b_detail_panel_sections.md (P3), docs/sprints/sprint_04/S4-18a_notices_actionbar_titles.md (P6), docs/sprints/sprint_04/S4-22_j1_export_m2_exit.md (P6); the test release T.1

Owner decisions for this task: none
Additional instructions: Extend reflow.spec.ts so it opens a criterion card with two procedures (choose a selection on T.1 that produces one, or add one to T.1 if none exists — say which) at 320 px. If the tab labels do not fit, fix the tab layout with tokens so both labels are reachable and the header follows the selected tab; otherwise only the test changes.

Way of working:
1. Branch sprint05/S5-06-reflow-two-procedures from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-06_reflow_two_procedures.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-07 — Server: Identity cookie session with antiforgery

```text
Task S5-07 from docs/sprints/sprint_05/SPRINT_05.md — Server: Identity cookie session with antiforgery
Issues: #87, #88, #89
Requirements / invariants: SEC-01 (server part), SEC-02, SEC-03 (API part), SEC-09, OPS-04 → active; API-01 stays green
Depends on: nothing; alone in the IdentityAccess module while it adds migrations

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 87, gh issue view 88, gh issue view 89) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §9.1 (session model), §8.3, C60, §12 step 3; SPEC R-AUTH-03, R-AUTH-05, R-AUTH-08; TESTING §4.7 (SEC-*), §4.10 (OPS-04), §5.2, §9 step 3; docs/sprints/sprint_04/S4-26_key_ring_encryption.md (key ring), docs/sprints/sprint_04/S4-03_users_controller_authorize.md; TECH_DEBT_v4 §5 (legacy auth)

Owner decisions for this task: none
Additional instructions: ASP.NET Core Identity cookie authentication: HttpOnly; Secure; SameSite=Strict, 30 min idle / 8 h absolute from configuration (🔧), Data Protection keys from the encrypted store of S4-26. Antiforgery (X-XSRF-TOKEN, cookie XSRF-TOKEN) on every state-changing endpoint except the anonymous export, including criteria-list-shown. Anonymous GET /auth/session returns 200 { user: null } and issues the antiforgery cookie; sign-in and sign-out endpoints; unauthenticated API calls answer 401, never a 302. Build the integration-host options of #87 – #89 (real cookie flow, session probe for anonymous and unknown test users, antiforgery helper carrying the caller's headers). Keep the legacy JWT working in parallel only as long as the client needs it (S5-08 removes it) — say in the plan how both coexist for one PR, or whether you propose to switch in one step. This is the riskiest task of the sprint: list every endpoint whose behaviour changes.

Way of working:
1. Branch sprint05/S5-07-cookie-session-server from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-07_cookie_session_server.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-08 — Client: session state, interceptor, JWT removed

```text
Task S5-08 from docs/sprints/sprint_05/SPRINT_05.md — Client: session state, interceptor, JWT removed
Issues: none
Requirements / invariants: SEC-01 (client part), SEC-03 (overlay part) → active
Depends on: S5-07 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §8.3 (interceptor, login overlay), §8 (store slices, withIdentity()); SPEC R-AUTH-05, R-AUTH-07; TESTING §4.7; CLIENT auth.service.ts and the legacy identity store; CLAUDE.md §9

Owner decisions for this task: none
Additional instructions: withIdentity() holds the session from GET /auth/session (probe at start); an HTTP interceptor sends the antiforgery header on state-changing requests and turns a 401 into the login overlay with a retry of the original request after sign-in (the overlay component itself comes in S5-09 — a minimal one is fine here); signedIn for the usage events comes from the session. Remove the JWT, jwtDecode and every read or write of tokens in localStorage/sessionStorage (test with storage blocked). The legacy screens keep working with the session.

Way of working:
1. Branch sprint05/S5-08-session-client from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-08_session_client.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-09 — Sign-in S01, login overlay and sign-out

```text
Task S5-09 from docs/sprints/sprint_05/SPRINT_05.md — Sign-in S01, login overlay and sign-out
Issues: #170 (L1, login outlet)
Requirements / invariants: R-AUTH-03, R-AUTH-07 (base), R-AUTH-08; A11Y-01, A11Y-08, A11Y-09 stay green
Depends on: S5-08 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 170) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-03, R-AUTH-07, R-AUTH-08; Design handoff §5.3 (S01 form card as in S51, login overlay after 401, error summary §6.4); ux_copy §5.2 (auth.login.*), shell.session.expired.*, shell.nav.logout; ARCHITECTURE C60

Owner decisions for this task: none
Additional instructions: S01 in the named login outlet, as the form card pattern of Handoff §5.3 (no Figma frame: build from the pattern and compare with the S51 frame for the card); auth.login.reasonSave when coming from „Liste speichern“; error summary with focus after a failed submit. The login overlay (MatDialog with the S01 form and shell.session.expired.*) opens without navigation and keeps the page and its form input. Sign-out clears the cookie, fetches a new antiforgery token and clears in-memory state. The login outlet reflows at 320 px (#170 L1); add S01 to reflow.spec.ts. Texts from the UX copy (auth area into AREAS).

Way of working:
1. Branch sprint05/S5-09-login-s01-overlay from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-09_login_s01_overlay.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-10 — Legacy auth cleanup

```text
Task S5-10 from docs/sprints/sprint_05/SPRINT_05.md — Legacy auth cleanup
Issues: #158 (auth entries)
Requirements / invariants: API-01 stays green for the identity endpoints
Depends on: S5-09, S5-12, S5-13 merged; O-S5-4

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 158) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §5 (legacy auth items); CLIENT eslint.config.js LEGACY_EXCEPTIONS; API identity endpoints, UsersController, UserService, UserRepo

Owner decisions for this task: O-S5-4: <S01 – S03, S60, S61 replace the legacy pages at the target routes; legacy pages deleted once replaced; existing users sign in again | other>
Additional instructions: Remove Microsoft.AspNetCore.Identity.UI; turn the { code, message } failures of UsersController and the identity minimal APIs into Problem Details (or delete endpoints the new flows replaced — list them); DateTime.UtcNow in UserService/UserRepo → TimeProvider; ERROR_TRANSLATIONS and other dead legacy client error keys; delete the legacy login, register, reset-password and confirm-email pages and routes per O-S5-4 (profile and settings go with S5-26/S5-28) and their LEGACY_EXCEPTIONS entries. Redirect the old routes to the new ones if they were ever linked from outside.

Way of working:
1. Branch sprint05/S5-10-legacy-auth-cleanup from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-10_legacy_auth_cleanup.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-11 — IEmailSender with MailKit, Mailpit and real recipients

```text
Task S5-11 from docs/sprints/sprint_05/SPRINT_05.md — IEmailSender with MailKit, Mailpit and real recipients
Issues: #302
Requirements / invariants: PRV-11 → active
Depends on: S5-04 settled (MailKit major); S5-07 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 302) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE C71 (e-mail), §9.3; SPEC §7.6, SD13; ux_copy §5.17 (email.*); TESTING §4.3 (PRV-11), §6.3; TECH_DEBT_v4 §3.1 (#302), §3.2 (TD-12); docker-compose.yml, docker-compose.e2e.yml

Owner decisions for this task: none
Additional instructions: IEmailSender in Platform with MailKit over SMTP/TLS (synchronous with one retry), configured only from the environment: Lettermint in staging/production, Mailpit (licence check, my OK) in development and E2E. The message types of C71 with the email.* texts, plain and HTML parts, no tracking pixels, no rewritten links; recipient addresses never logged above Debug. Mails go to the real recipient (#302) — remove the test sink, with a recipient test. Check the E2E compose.log artefact for secrets now that real ones exist (TD-12) and record the result.

Way of working:
1. Branch sprint05/S5-11-email-sender from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-11_email_sender.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-12 — Registration S02 and the e-mail confirmation gate

```text
Task S5-12 from docs/sprints/sprint_05/SPRINT_05.md — Registration S02 and the e-mail confirmation gate
Issues: #301
Requirements / invariants: SEC-10 → active; API-05 covers registration; A11Y-09 stays green
Depends on: S5-11 merged; O-S5-3

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 301) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-01, R-AUTH-02, §7.2, §7.3; ARCHITECTURE C72, §9.1; Design handoff §5.3 (S01–S03 form card, e-mail confirmation pattern); ux_copy §5.2 (auth.register.*, auth.confirm.*); TESTING §4.7 (SEC-10), §4.6 (API-05); TECH_DEBT_v4 §3.1 (#301)

Owner decisions for this task: O-S5-3: <(a) new registration without the legacy Member role | (b) migration that creates Member everywhere>
Additional instructions: S02 with e-mail, password (≥ 12 🔧) and the ⚖️ privacy acknowledgement as written; the confirmation mail with a single-use, hashed link (24 h 🔧) and the result pages „as S80“; the gate: an unconfirmed account can sign in, but saving lists, creating organisations, accepting invitations, creating contributor profiles and publishing answer 403 email-unconfirmed and the client shows auth.confirm.required with „erneut senden“. #301 per O-S5-3. Registration rate limit (API-05). Add S02 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-12-registration-confirmation from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-12_registration_confirmation.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-13 — Password reset S03 and lockout

```text
Task S5-13 from docs/sprints/sprint_05/SPRINT_05.md — Password reset S03 and lockout
Issues: none
Requirements / invariants: SEC-11 → active; API-05 covers password reset
Depends on: S5-11 merged (S5-12 for shared code)

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-04, §7.3; ARCHITECTURE C72; ux_copy §5.2 (auth.reset.*); TESTING §4.7 (SEC-11)

Owner decisions for this task: none
Additional instructions: S03: the same answer for known and unknown addresses; single-use hashed reset link (2 h 🔧); new password ≥ 12 🔧. Lockout after 5 failed sign-ins for 15 minutes 🔧, tested on the fake clock. Password-reset rate limit (API-05). Add S03 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-13-password-reset-lockout from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-13_password_reset_lockout.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-14 — E2E users and the account journey

```text
Task S5-14 from docs/sprints/sprint_05/SPRINT_05.md — E2E users and the account journey
Issues: none
Requirements / invariants: A11Y-01, A11Y-08 stay green; prepares J1 second part
Depends on: S5-12, S5-13 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §6.3 (E2E dataset: tenant users with known test credentials), §5.4; e2e/support/global-setup.ts; the E2E seeder of S4-08

Owner decisions for this task: none
Additional instructions: Extend the E2E seeder with the test users of TESTING §6.3 (known credentials, no real data, idempotent, E2E environment only). Add a journey: register → open the confirmation mail in Mailpit → confirm → sign in → sign out, with axe on every state in both appearances, on all five browser projects.

Way of working:
1. Branch sprint05/S5-14-e2e-users-account-journey from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-14_e2e_users_account_journey.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-15 — Organisations: domain, query filters, isolation tests

```text
Task S5-15 from docs/sprints/sprint_05/SPRINT_05.md — Organisations: domain, query filters, isolation tests
Issues: none
Requirements / invariants: TEN-01, TEN-02, TEN-05 (skeleton) → active
Depends on: S5-07 merged; O-S5-5 if a contracts type needs another module's contracts; one migration branch per module

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §4.3, §5.2, §5.4, §7.2 (authorisation), §9.1, §12 step 3; SPEC R-ORG-05; TESTING §4.5 (TEN-*), §9 step 3; CLAUDE.md §7 (tenant isolation)

Owner decisions for this task: O-S5-5: <may a Contracts project reference another module's Contracts — yes along the §4.3 table without cycles (ADR) | no>
Additional instructions: Organization and OrganizationMembership (Owner / Editor / Reader) in the module §4.3 names, with the migration. EF global query filter on OrganizationId; the organisation is resolved from the resource, never from a client header; foreign resources answer 404. The reflection-driven isolation test over every entity with OrganizationId (TEN-01), the source-scan allow-list for IgnoreQueryFilters() (TEN-02), and the authorisation-matrix skeleton (TEN-05). Existing accounts keep working without a membership (ARCHITECTURE §12 step 3). If O-22 comes up, stop and ask unless O-S5-5 is filled in.

Way of working:
1. Branch sprint05/S5-15-organisations-tenancy from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-15_organisations_tenancy.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-16 — Create organisation S20, members page S21, leave

```text
Task S5-16 from docs/sprints/sprint_05/SPRINT_05.md — Create organisation S20, members page S21, leave
Issues: none
Requirements / invariants: R-ORG-01, R-ORG-03, R-ORG-04, R-ORG-06; TEN-11 (organisation part) → active; A11Y-09 stays green
Depends on: S5-15 merged; S5-12 (confirmation gate)

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-ORG-01, R-ORG-03, R-ORG-04, R-ORG-06; Design handoff §5.3 (S20 form card, S21 members table / list); ux_copy §5.7 (org.*); TESTING §4.5 (TEN-11)

Owner decisions for this task: none
Additional instructions: S20 (name ≤ 120, type public body / company / association; confirmed e-mail required; the creator becomes Owner). S21: members with role; the Owner changes roles, removes members, revokes invitations; org.members.lastOwner in an error notice above the table. Leave for every member. The last Owner cannot be demoted, removed or leave (409). No join requests, no organisation directory. Add S20 and S21 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-16-organisation-s20-s21 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-16_organisation_s20_s21.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-17 — Invitations by e-mail

```text
Task S5-17 from docs/sprints/sprint_05/SPRINT_05.md — Invitations by e-mail
Issues: none
Requirements / invariants: R-ORG-02; SEC-10 (accept part)
Depends on: S5-15, S5-11 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-ORG-02; ARCHITECTURE C72, C80; ux_copy §5.7 (org.invite.*), §5.17 (email.*); TESTING §4.5

Owner decisions for this task: none
Additional instructions: Invitations by e-mail: valid 14 days 🔧, single use, stored hashed, bound to the invited address, role chosen on invite; the accept flow requires a confirmed e-mail and the invited address. Tests on the fake clock for expiry and on reuse.

Way of working:
1. Branch sprint05/S5-17-invitations from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-17_invitations.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-18 — Signed-in header states and organisation switch

```text
Task S5-18 from docs/sprints/sprint_05/SPRINT_05.md — Signed-in header states and organisation switch
Issues: none
Requirements / invariants: R-SHL-02, R-SHL-03; TEN-01 (switch test)
Depends on: S5-16, S5-09 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SHL-02, R-SHL-03; Design handoff §3.1 (header states, compact layout); ux_copy §5.1 (shell.*); the Figma frames of pa-app-header and pa-app-header-compact in the Figma mapping

Owner decisions for this task: none
Additional instructions: pa-header in the states SignedIn and SignedInNoOrg (Anonymous exists): „Projekte“ and the organisation display only with a membership; aria-current on the active item; compact layout below 600 px. Organisation switch for users with more than one membership: lists, projects and the compare select then show only that organisation's data. Compare with the header frames in Figma.

Way of working:
1. Branch sprint05/S5-18-header-states-org-switch from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-18_header_states_org_switch.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-19 — Configurator persistence and PersistedConfigurationSource

```text
Task S5-19 from docs/sprints/sprint_05/SPRINT_05.md — Configurator persistence and PersistedConfigurationSource
Issues: none
Requirements / invariants: CAT-08, EXP-06 → active; TEN-01 covers the new entities; ARC-03 (IConfigurationReader rule)
Depends on: S5-15 merged; O-S5-5; one migration branch per module

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §4.3 (IConfigurationReader, no cycle with Export), §5.3 (configurator entities), §12 step 3, C53, C62; TESTING §4.2 (CAT-08), §4.9 (EXP-06), §4.10 (ARC-03); SPEC §8.2

Owner decisions for this task: O-S5-5: <copy from S5-15>
Additional instructions: CriteriaConfiguration (organisation-owned, pinned to a catalog release), ConfigurationAppType, ConfigurationProductPart, ConfigurationTestType, ConfigurationCriterion (override keeps ResolvedLevel), with the migration. IConfigurationReader declared in Configurator.Contracts and implemented in Configurator; PersistedConfigurationSource in Export reads only through it (ARC-03 rule). EXP-06: for the same selection and decisions both export sources produce identical cell content. CriteriaConfiguration.ProcurementProjectId comes in step 5, not here.

Way of working:
1. Branch sprint05/S5-19-configurator-persistence from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-19_configurator_persistence.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-20 — Save dialog S12, saved lists S14, J1 second part

```text
Task S5-20 from docs/sprints/sprint_05/SPRINT_05.md — Save dialog S12, saved lists S14, J1 second part
Issues: none
Requirements / invariants: R-SAV-01, R-SAV-02; J1 second part (M3 exit); A11Y-02, A11Y-09 stay green
Depends on: S5-19, S5-09 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SAV-01, R-SAV-02, R-AUTH-03 (auth.login.reasonSave); Design handoff §5.3 (S12 dialog as S13, S14 list); ux_copy §5.6 (criteria.save.*); TESTING §5.4 (J1)

Owner decisions for this task: none
Additional instructions: S12 (name ≤ 120, organisation note) per the S13 dialog rules; an anonymous user who presses „Liste speichern“ signs in through the login outlet with auth.login.reasonSave and comes back with the draft intact (in memory, never in storage); an unconfirmed user gets the confirmation notice. S14: organisation lists only, sortable by name/date, results announced. Extend J1: save after sign-in, keyboard only, axe in both appearances. Add S14 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-20-save-s12-s14-j1 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-20_save_s12_s14_j1.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-21 — Edit, delete and export saved lists; older release

```text
Task S5-21 from docs/sprints/sprint_05/SPRINT_05.md — Edit, delete and export saved lists; older release
Issues: none
Requirements / invariants: R-SAV-03, R-SAV-06, R-LST-14; CAT-22 and the persisted parts of EXP-01, EXP-03 → active
Depends on: S5-20 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SAV-03, R-SAV-06, R-LST-14, R-LST-12 (saved export); ARCHITECTURE D6 (copies); TESTING §4.2 (CAT-22), §4.9; ux_copy §5.5, §5.6

Owner decisions for this task: none
Additional instructions: Edit a saved list on S11 (include/exclude, priority, note; „Auswahl ändern“ back to S10 with the saved selection); delete with a confirmation dialog (Owner, Editor); saved export POST /configurations/{id}/exports through PersistedConfigurationSource; the older-release notice and „Neue Liste mit derselben Auswahl“ (copy to the active release, values the new release excludes are dropped and reported, CAT-22). Readers see lists read-only.

Way of working:
1. Branch sprint05/S5-21-edit-delete-saved-export from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-21_edit_delete_saved_export.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-22 — Saved-list debt: weight range, too-long note, unsaved state, no-store

```text
Task S5-22 from docs/sprints/sprint_05/SPRINT_05.md — Saved-list debt: weight range, too-long note, unsaved state, no-store
Issues: none
Requirements / invariants: TEN-04 (workspace part) → active
Depends on: S5-20 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v4 §3.3 (TD-45), §3.1 (TD-40), §9.2; docs/sprints/sprint_04/S4-18a_notices_actionbar_titles.md (O-S4-54 draft note); docs/sprints/sprint_04/S4-19_export_pipeline.md (P7)

Owner decisions for this task: none
Additional instructions: Serve the priority weight range with the saved-list API and drop the duplicated client constant (TD-45); a note over 1 000 characters leads to the note from S13 instead of the generic error (TD-40); criteria.list.unsaved and the draft-note wording for signed-in users per the UX copy; Cache-Control: no-store on every workspace answer (TEN-04 workspace part).

Way of working:
1. Branch sprint05/S5-22-saved-list-debt from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-22_saved_list_debt.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-23 — Share links: model, management API, anonymous read

```text
Task S5-23 from docs/sprints/sprint_05/SPRINT_05.md — Share links: model, management API, anonymous read
Issues: none
Requirements / invariants: TEN-08, TEN-12 → active
Depends on: S5-19 merged; one migration branch per module

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SAV-04, R-SAV-05, SD15; ARCHITECTURE §5.3 (ShareLink), C59, C81; TESTING §4.5 (TEN-08, TEN-09, TEN-12)

Owner decisions for this task: none
Additional instructions: ShareLink (organisation-owned, covered by TEN-01; only a hash of the token stored), expiry required: default today + 90 days, maximum 1 year 🔧, past or missing → 400 with a field error. Management endpoints (create, list, revoke) for Owner and Editor; IShareableResourceProvider declared in Platform, implemented by Configurator; anonymous GET /api/v1/shared/{token} with the redacted list (no notes of the organisation beyond what SPEC allows), 404 for unknown or expired tokens.

Way of working:
1. Branch sprint05/S5-23-share-links-api from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-23_share_links_api.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-24 — Share dialog S15 and shared list S16

```text
Task S5-24 from docs/sprints/sprint_05/SPRINT_05.md — Share dialog S15 and shared list S16
Issues: none
Requirements / invariants: TEN-09 → active; A11Y-09 stays green
Depends on: S5-23 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-24 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SAV-04, R-SAV-05; Design handoff §5.3 (S15 dialog as S13, S16 = S11 read-only); ux_copy §5.6 (criteria.share.*, criteria.shared.*)

Owner decisions for this task: none
Additional instructions: S15 per the S13 dialog rules: required expiry with default, copy link, existing links with revoke. S16 at /geteilt/:token without account: S11 read-only (no checkboxes, no K5 menu, no action bar), header in the anonymous state. Add S16 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-24-share-s15-s16 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-24_share_s15_s16.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-25 — User preferences and the end of the legacy settings store

```text
Task S5-25 from docs/sprints/sprint_05/SPRINT_05.md — User preferences and the end of the legacy settings store
Issues: #234 (TD-25 part)
Requirements / invariants: A11Y-06 and the explicit-setting part of A11Y-08 → active
Depends on: S5-07 merged; one migration branch per module

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-25 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 234) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SET-01, R-SET-02, §7.5; ARCHITECTURE C73, §5.2 (UserPreferences); TESTING §4.11 (A11Y-06, A11Y-08); TECH_DEBT_v4 §3.1 (TD-25)

Owner decisions for this task: none
Additional instructions: UserPreferences (language, appearance, motion, contrast „wie im System“) stored per user, with GET/PUT. The client applies them for signed-in users (explicit choice overrides the system) and keeps following the system for anonymous users, without any browser storage. Remove the legacy settings store with its localStorage writes, its English snackbar and the crash with blocked storage (TD-25). A11Y-08: the axe run with the explicit dark setting and its persistence.

Way of working:
1. Branch sprint05/S5-25-user-preferences from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-25_user_preferences.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-26 — Settings page S60

```text
Task S5-26 from docs/sprints/sprint_05/SPRINT_05.md — Settings page S60
Issues: none
Requirements / invariants: R-SET-01; A11Y-06 stays green; A11Y-09
Depends on: S5-25 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-26 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-SET-01; Design handoff §5.3 (S60 row: fieldsets Sprache, Erscheinungsbild, Bewegung und Animation, Kontrast); ux_copy §5.14 (settings.*)

Owner decisions for this task: none
Additional instructions: S60 per Handoff §5.3: language (only Deutsch selectable, hint), appearance as radio group, motion, contrast; changes apply without reload and are saved; error summary on a failed save. Delete the legacy settings page and its LEGACY_EXCEPTIONS entries. Add S60 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-26-settings-s60 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-26_settings_s60.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-27 — Personal data export

```text
Task S5-27 from docs/sprints/sprint_05/SPRINT_05.md — Personal data export
Issues: none
Requirements / invariants: PRV-07 (account part) → active
Depends on: S5-15 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-27 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-09, §8.4, SD14; ARCHITECTURE C79; TESTING §4.3 (PRV-07)

Owner decisions for this task: none
Additional instructions: GET /api/v1/account/export returns the user's own account data as JSON per SPEC §8.4 (memberships, preferences, saved lists the user created as far as §8.4 says), nothing of other users; attachment headers and no-store.

Way of working:
1. Branch sprint05/S5-27-personal-data-export from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-27_personal_data_export.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-28 — Account page S61 and account deletion

```text
Task S5-28 from docs/sprints/sprint_05/SPRINT_05.md — Account page S61 and account deletion
Issues: #234 (TD-26 part)
Requirements / invariants: R-AUTH-09, R-AUTH-10; TEN-11 (account part) → active; A11Y-09
Depends on: S5-16, S5-21, S5-23, S5-27 merged

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-28 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 234) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-09, R-AUTH-10 ⚖️; ARCHITECTURE C79, §9.3; Design handoff §5.3 (S61 form card); ux_copy §5.14 (account.*); TESTING §4.5 (TEN-11); TECH_DEBT_v4 §3.1 (TD-26)

Owner decisions for this task: none
Additional instructions: S61: download of the export, change e-mail (with re-confirmation) and password (rebuilt from the legacy profile), and account deletion with its rules — the sole Owner of an organisation with other members is refused (409 with the reason), an organisation whose last member deletes the account is deleted with its lists, share links and projects; ⚖️ texts as written; „Löschen“ with aria-disabled and a visible reason where the UX copy has one. Delete the legacy profile page and delete-account dialog (TD-26) and their LEGACY_EXCEPTIONS entries; close #234 if TD-25 is done too. Add S61 to reflow.spec.ts.

Way of working:
1. Branch sprint05/S5-28-account-s61-deletion from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-28_account_s61_deletion.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

## P2 — stretch

### S5-29 — Session hardening (M4.1) *(stretch)*

```text
Task S5-29 from docs/sprints/sprint_05/SPRINT_05.md — Session hardening (M4.1)
Issues: none
Requirements / invariants: SEC-07, SEC-08 → active
Depends on: S5-07 – S5-09 merged; S5-28 for deletion; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-29 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ARCHITECTURE §9.1, C52, C60; SPEC R-AUTH-05; TESTING §4.7 (SEC-07, SEC-08); ROADMAP M4.1

Owner decisions for this task: none
Additional instructions: Stretch task. Absolute limit with the stored sign-in time, sliding renewal on every request, expiry headers, POST /auth/session/extend, security-stamp invalidation on password change and account deletion; all lifetimes 🔧 from configuration; tests on the fake clock.

Way of working:
1. Branch sprint05/S5-29-session-hardening from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-29_session_hardening.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-30 — Session warning dialog (M4.2) *(stretch)*

```text
Task S5-30 from docs/sprints/sprint_05/SPRINT_05.md — Session warning dialog (M4.2)
Issues: none
Requirements / invariants: A11Y-04 → active
Depends on: S5-29 merged; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-30 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-AUTH-06, R-AUTH-07; Design handoff §5.3 (session warning, login overlay); ux_copy shell.session.*; TESTING §4.11 (A11Y-04)

Owner decisions for this task: none
Additional instructions: Stretch task. The warning 5 minutes 🔧 before the earlier of the idle and the absolute limit (alertdialog, focus on „Angemeldet bleiben“; before the absolute limit it asks to save and sign in again); the completed login overlay. Vitest fake timers and E2E with page.clock (E2E clock endpoint if needed).

Way of working:
1. Branch sprint05/S5-30-session-warning from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-30_session_warning.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-31 — Rating legend on S11 *(stretch)*

```text
Task S5-31 from docs/sprints/sprint_05/SPRINT_05.md — Rating legend on S11
Issues: #279
Requirements / invariants: A11Y-01 stays green
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-31 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 279) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: Design handoff §4.11 (pa-rating-legend); ux_copy U24; the Figma component in the Figma mapping

Owner decisions for this task: none
Additional instructions: Stretch task. „Was bedeuten die Bewertungsstufen?“ as a disclosure in „Wie wird bewertet?“ on S11, as pa-rating-legend per Handoff §4.11; compare with its Figma frame.

Way of working:
1. Branch sprint05/S5-31-rating-legend from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-31_rating_legend.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S5-32 — Design fidelity of S10 and S11 *(stretch)*

```text
Task S5-32 from docs/sprints/sprint_05/SPRINT_05.md — Design fidelity of S10 and S11
Issues: #278, #281, #282
Requirements / invariants: A11Y-01, A11Y-09 stay green
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_05.md the row of S5-32 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 278, gh issue view 281, gh issue view 282) and their entries in TECH_DEBT_v4.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: the three issues; Design handoff §5.4, §5.5; the Figma frames of S10 and S11 in the Figma mapping; docs/sprints/sprint_04/S4-21b_export_dialog.md (P3)

Owner decisions for this task: none
Additional instructions: Stretch task. „Auswahl ändern“ next to the H1 from 600 px; the sticky action bar across the viewport; 32 px stepper circles; summary labels and the desktop filter row as in Figma. Compare before and after with the frames; no behaviour change.

Way of working:
1. Branch sprint05/S5-32-design-fidelity from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:SEC-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_05/S5-32_design_fidelity.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

Replace `<S>` with the sprint number (e.g. 6), `<SS>` with the two-digit sprint folder number (e.g. 06) and `<v>` with the current tech-debt register version. Name documents without version numbers. Check that new decision IDs are not used yet.

```text
Task S<S>-<NN> from docs/sprints/sprint_<SS>/SPRINT_<SS>.md — <title of the row>
Issues: <#n, … | none>
Requirements / invariants: <R-… / INV IDs the task activates or must keep green>
Depends on: <tasks, owner decisions | nothing>

Read first: CLAUDE.md; in SPRINT_<SS>.md the row of S<S>-<NN> plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view <n>) and their entries in TECH_DEBT_v<v>.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: <documents and sections without version numbers, and earlier task protocols in docs/sprints/ that the row or an issue points to>

Owner decisions for this task: <none | O-S<S>-n: <decision> (decided <date>)>
Additional instructions: <none | …>

Way of working:
1. Branch sprint<SS>/S<S>-<NN>-<topic> from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests, and whether the task adds an EF migration (and to which module). If the plan shows more than one session, propose a split (as S3-20a/b). For a screen or component: if the Figma MCP connection is available, look at its frame (node ID from the Figma mapping) before building and compare the result before pushing (CLAUDE §2, Figma files); screens without a frame follow Handoff §5.3. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. If another PR merged a migration of the same module first, rebase and regenerate yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_<SS>/S<S>-<NN>_<topic>.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

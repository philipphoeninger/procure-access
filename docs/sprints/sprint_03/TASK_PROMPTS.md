# Sprint 03 — task prompts for Claude Code

> **Status:** v1 (2026-11-05). Companion to `SPRINT_03.md`: one ready-to-paste prompt per task, built from the generic template at the end of `docs/sprints/sprint_02/TASK_PROMPTS.md` (repeated at the end of this file). Claude Code implements; the owner's control chat for sprint 03 takes the decisions and reviews the reports.
> **How to use:** copy the block of the task into a new Claude Code session. Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>` or `<Name>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_03.md`, `TECH_DEBT_v2.md` and the GitHub issues and pull requests as of 2026-11-05.
> **Changes to the template since sprint 02:** reads `TECH_DEBT_v2.md`; step 4 also runs `node scripts/check-i18n.mjs`; step 3 asks for the owner's OK also before adding a project; the protocol header names issues and requirements / invariants, as the sprint-02 protocols did.

---

## Overview

| Task | Title | Issues | Can start |
|---|---|---|---|
| S3-01 | Review Dependabot pull requests #164, #165, #166, #175 | PR #164 (nginx 1.31), PR #165 (Angular group), PR #166 (other group), PR #175 (TypeScript 7 in e2e/) | ready (O-S3-1 only for #175) |
| S3-02 | Legacy client fixes: loading counter, offline notice, icon spans | #168, #171, #137 | ready |
| S3-03 | CodeQL advanced setup | #144 | ready |
| S3-04 | Scripts: CodeQL alert 3 and a permanent self-test of the invariant gate | #182, #185 | ready |
| S3-05 | Documentation session: sprint-02 findings and the reflow invariant | #180, #188 | after O-S3-3 |
| S3-06 | Swagger UI and OpenAPI documents per environment | #183 | after O-S3-2 |
| S3-07 | AutoMapper replacement with the generic CRUD maps and the ARC-06 successor | #31, #90 | ready (run 1); run 2 after O-S3-4 |
| S3-08 | Build warnings and analyzer check on all projects | #118 | after S3-07 |
| S3-09 | Theme foundation: light and dark appearance, focus indicators | #184 | ready |
| S3-10 | A11Y-08: axe in the dark appearance | none | after S3-09 |
| S3-11 | Catalog module: design note on persistence and the existing criteria | none | ready |
| S3-12 | Catalog core entities and first migration | none | after O-S3-5 |
| S3-13 | Catalog filter dimensions, applicability, priorities and procedure texts | none | after S3-12 |
| S3-14 | MiniCatalog fixture and builders for step 2 | none | after S3-13 |
| S3-15 | Release data format and validation | none | after S3-14 |
| S3-16 | CLI catalog import | none | after S3-15 |
| S3-17 | CLI catalog activate, active release and cache | none | after S3-16 |
| S3-18 | The existing criteria in the catalog model | none | after S3-17 (or S3-13, per O-S3-5) |
| S3-19 | GET /api/v1/catalog/filter-options | #153, #176 (milestone per O-S3-6) | after S3-18 and O-S3-6 |
| S3-20 | GET /api/v1/catalog/criteria with resolved priorities | none | after S3-19 |
| S3-21 | Footer link „Kriterium vorschlagen“ | none | ready |
| S3-22 | Snackbar buttons in German | #160 | stretch, after #181 |
| S3-23 | Usage events: fire-and-forget writes | #115 | stretch, ready |
| S3-24 | Persist the Data Protection keys outside the container | #186 | stretch, ready |
| S3-25 | Material Symbols font subsetting | #135 | stretch, after O-S3-7 |
| S3-26 | ESLint for e2e/ and scripts | #161 | stretch (O-S3-8 for the plugin) |
| S3-27 | Release 2026.1, part 1: texts without priorities | none | stretch, after S3-15 |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S3-1 (e2e TypeScript policy) | S3-01 | owner choice (any time) |
| O-S3-2 (Swagger in Production) | S3-06 | owner choice (any time) |
| O-S3-3 (reflow invariant ID) | S3-05 | owner choice (any time) |
| O-S3-4 (AutoMapper replacement, ARC-06 successor) | S3-07 run 2 | the ADR options of S3-07 run 1 |
| O-S3-5 (catalog persistence, path of the existing criteria) | S3-12, S3-13, S3-15, S3-18 | the design note of S3-11 |
| O-S3-6 (endpoint style, #176) | S3-19 | owner choice, best after S3-11 |
| O-S3-7 (font subsetting) | S3-25 | owner choice (options in #135) |
| O-S3-8 (eslint-plugin-playwright) | S3-26 | owner choice |

## Rules for running tasks

- **One new TESTING version at a time.** S3-05, S3-07, S3-10, S3-12, S3-13, S3-16, S3-17, S3-19 and S3-20 flip or replace invariants. If two of them run on parallel branches, both would create the same `TESTING_v<n>.md`. Start such a task only after the previous one is merged. Tasks without an invariant change (S3-01 – S3-04, S3-06, S3-08, S3-09, S3-11, S3-14, S3-15, S3-18, S3-21) can run in between.
- **Order:** as in `SPRINT_03.md` (Sequence). S3-18 comes after S3-17 although it belongs to M2.1. Backend (S3-07, S3-08, S3-11 – S3-20) and frontend (S3-02, S3-09, S3-10, S3-21) may alternate when one side waits.
- **Two-run tasks:** S3-07 and S3-11 stop after their first run (options / design note) for the owner's decision.
- **Stretch tasks** (S3-22 – S3-27) only after all P0 tasks are done.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_03.md` and `TECH_DEBT_v3.md` (process step 14), prepared in the control chat.

---

## P0 — committed

### S3-01 — Review Dependabot pull requests #164, #165, #166, #175

```text
Task S3-01 from docs/sprints/sprint_03/SPRINT_03.md — Review Dependabot pull requests #164, #165, #166, #175
Issues: PR #164 (nginx 1.31), PR #165 (Angular group), PR #166 (other group), PR #175 (TypeScript 7 in e2e/)
Requirements / invariants: CLAUDE §10 (licences); A11Y-01 and CAT-10 stay green
Depends on: nothing; O-S3-1 for #175

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the pull requests (gh pr view 164, gh pr view 165, gh pr view 166, gh pr view 175); always the highest version of each document in docs/.
Also read: docs/sprints/sprint_02/S2-04_dependabot_prs.md (how sprint 02 reviewed Dependabot PRs); TECH_DEBT_v2 §6 (e2e TypeScript policy) and §9.6

Owner decisions for this task: O-S3-1: <e2e/ TypeScript: follow the client's TypeScript | update on its own>
Additional instructions: This task reviews existing pull requests instead of building something: steps 1 and 3 of the way of working do not apply. For every PR check the licence of each new version (CLAUDE §10), the changelog for breaking changes (Angular group: read the update notes; nginx: config compatibility of CLIENT nginx.conf), and the CI result. Apply O-S3-1 to #175 (a Dependabot ignore rule needs a small dependabot.yml change in its own PR). The protocol recommends merge or close per PR with the reason; open a small PR that contains only the protocol and, if needed, the dependabot.yml change. Do not merge any of the Dependabot PRs.

Way of working:
1. Branch sprint03/S3-01-dependabot-prs from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-01_dependabot_prs.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-02 — Legacy client fixes: loading counter, offline notice, icon spans

```text
Task S3-02 from docs/sprints/sprint_03/SPRINT_03.md — Legacy client fixes: loading counter, offline notice, icon spans
Issues: #168, #171, #137
Requirements / invariants: A11Y-01 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 168, gh issue view 171, gh issue view 137) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_02/S2-17_start_and_error_pages.md (open point P1), docs/sprints/sprint_02/S2-18_zoom_and_e2e.md (open point P2), docs/sprints/sprint_02/S2-11_client_error_mapping.md (open point P6: approveProposal)

Owner decisions for this task: none
Additional instructions: One PR, one commit per issue, a unit or component test per issue. #168: decrement the loading counter when a load fails, in every legacy store and in approveProposal. #171: the offline notice counts only answers to /api/ requests. #137: replace the legacy icon <span>s in settings and profile with <mat-icon> and aria-hidden. Do not touch anything else in the legacy screens; they are rebuilt from sprint 04 on.

Way of working:
1. Branch sprint03/S3-02-legacy-client-fixes from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-02_legacy_client_fixes.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-03 — CodeQL advanced setup

```text
Task S3-03 from docs/sprints/sprint_03/SPRINT_03.md — CodeQL advanced setup
Issues: #144
Requirements / invariants: none (CI)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 144) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 §3.2 (#144); .github/workflows/; the ruleset of main (gh api repos/{owner}/{repo}/rulesets)

Owner decisions for this task: none
Additional instructions: Switch CodeQL from default setup to an advanced-setup workflow (C#, JavaScript/TypeScript, Actions) that also runs on Dependabot pull requests; actions pinned by SHA like ci.yml. Show in the protocol that Analyze runs on a normal PR and on a Dependabot PR. The owner switches off the default setup and makes the Analyze checks required again after the merge — list the exact check names for that in your final report.

Way of working:
1. Branch sprint03/S3-03-codeql-advanced from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-03_codeql_advanced.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-04 — Scripts: CodeQL alert 3 and a permanent self-test of the invariant gate

```text
Task S3-04 from docs/sprints/sprint_03/SPRINT_03.md — Scripts: CodeQL alert 3 and a permanent self-test of the invariant gate
Issues: #182, #185
Requirements / invariants: none (CI stage 1)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 182, gh issue view 185) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: scripts/check-i18n.mjs and its test; scripts/check-invariants.mjs; docs/sprints/sprint_02/S2-09_gate_tightening.md (open point P2); TESTING_v23 §8.3

Owner decisions for this task: none
Additional instructions: #182: reject the path segments __proto__, constructor and prototype in setText of check-i18n.mjs, with a self-test case. #185: a node:test self-test for check-invariants.mjs (header drift, test files only, tests that run, .NET trait, protocol files) that runs in CI stage 1 like the Figma and i18n self-tests. No change to what the gate accepts.

Way of working:
1. Branch sprint03/S3-04-scripts-selftests from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-04_scripts_selftests.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-05 — Documentation session: sprint-02 findings and the reflow invariant

```text
Task S3-05 from docs/sprints/sprint_03/SPRINT_03.md — Documentation session: sprint-02 findings and the reflow invariant
Issues: #180, #188
Requirements / invariants: per O-S3-3: reflow invariant (proposal A11Y-09) → active
Depends on: nothing; O-S3-3; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 180, gh issue view 188) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 §3.6 (TD-14, the full list of points) and §6 (TD-23); the open points named there in docs/sprints/sprint_02/; e2e reflow.spec.ts

Owner decisions for this task: O-S3-3: <give reflow.spec.ts an invariant ID (proposal: A11Y-09 "every shell page and every rebuilt screen needs no horizontal scrolling at 320 × 256 CSS px") | keep it without an ID>
Additional instructions: Write ARCHITECTURE v16 (O-22 interim rule, Platform as built with Problem Details, C93 /api/Health removed), SPEC v3 (email-unconfirmed in §8.6, open footer targets in §13) and the next TESTING version (status line, reflow invariant per O-S3-3 with the tag on reflow.spec.ts). Update the headers of Handoff, SPEC, TESTING and the Figma mapping so that each names the current versions of the others. ROADMAP_v3 and the SPRINT_02 close-out are already done; do not touch them. Do not change any rule or requirement beyond the listed points: anything else goes into the open points.

Way of working:
1. Branch sprint03/S3-05-documentation-session from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-05_documentation_session.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-06 — Swagger UI and OpenAPI documents per environment

```text
Task S3-06 from docs/sprints/sprint_03/SPRINT_03.md — Swagger UI and OpenAPI documents per environment
Issues: #183
Requirements / invariants: SEC-06 stays green
Depends on: O-S3-2

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 183) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: docs/sprints/sprint_02/S2-21_api_dockerfile_swagger.md (open point P1); TECH_DEBT_v2 TD-18

Owner decisions for this task: O-S3-2: <Swagger UI and OpenAPI documents only in Development | also in Production (reason: …)>
Additional instructions: Implement O-S3-2 in ConfigureSwaggerExplorer() (or record the reason if Production keeps it) and show the behaviour per environment with a test (WebApplicationFactory in Development and Production).

Way of working:
1. Branch sprint03/S3-06-swagger-exposure from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-06_swagger_exposure.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-07 — AutoMapper replacement with the generic CRUD maps and the ARC-06 successor

```text
Task S3-07 from docs/sprints/sprint_03/SPRINT_03.md — AutoMapper replacement with the generic CRUD maps and the ARC-06 successor
Issues: #31, #90
Requirements / invariants: ARC-06 → replaced by its successor (new TESTING version)
Depends on: nothing for run 1; O-S3-4 for run 2; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 31, gh issue view 90) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 D12 (licence rule) and §14 (ADRs); TESTING_v23 §4.10 (ARC-06); Directory.Build.props (advisory suppression); DAL/MODELS AutoMapper profile; BaseCrudController; TECH_DEBT_v2 §3.1 (#90) and §3.2 (#31)

Owner decisions for this task: O-S3-4: <replacement (e.g. manual mapping | source generator <name>) and the ARC-06 successor — fill in after run 1>
Additional instructions: Run 1 (O-S3-4 not yet decided): options only, then stop — for each option the licence against D12, effort, how it covers the existing profile and the generic CRUD maps of #90, and what replaces ARC-06 as a test; with a recommendation, written as a draft ADR. Run 2 (with O-S3-4): replace every AutoMapper map, fix #90 (create and update work; UpdateOne/DeleteOne answer 404 for unknown ids) with tests first, remove the package and the advisory suppression, ADR in the next ARCHITECTURE version, ARC-06 successor in the next TESTING version. Legacy controllers keep their behaviour otherwise.

Way of working:
1. Branch sprint03/S3-07-automapper-replacement from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-07_automapper_replacement.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-08 — Build warnings and analyzer check on all projects

```text
Task S3-08 from docs/sprints/sprint_03/SPRINT_03.md — Build warnings and analyzer check on all projects
Issues: #118
Requirements / invariants: none (CI stage 1)
Depends on: S3-07 merged

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 118) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 §3.3 (#118: the 55 warning locations); docs/sprints/sprint_02/S2-06_module_skeleton.md (open point P4); docs/sprints/sprint_02/S2-07_platform_module.md (open point P3); .github/workflows/ci.yml (stage 1)

Owner decisions for this task: none
Additional instructions: Fix every warning of a Release build of ProcureAccess.sln (nullable in MODELS entities, VSTHRD200, CS0168, CS0105, CS1572, CS1573); AV0027 stays with #176 — suppress it with a pointer to #176 if needed. Set TreatWarningsAsErrors in src/Directory.Build.props, extend the analyzer check of CI stage 1 to every project, drop the unused Serilog.Settings.Configuration from SERVICES. No behaviour change: nullable fixes must not change what the API returns.

Way of working:
1. Branch sprint03/S3-08-build-warnings from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-08_build_warnings.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-09 — Theme foundation: light and dark appearance, focus indicators

```text
Task S3-09 from docs/sprints/sprint_03/SPRINT_03.md — Theme foundation: light and dark appearance, focus indicators
Issues: #184
Requirements / invariants: R-SET-02, R-SET-03 (development part), SPEC §7.5; A11Y-01 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 184) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: SPEC_v2 §7.5, R-SET-02, R-SET-03; design_handoff_procureaccess_v1.md §1.1, §1.2; CLIENT src/styles/theme.scss; docs/sprints/sprint_02/S2-15_shell_frame.md (open point P5), docs/sprints/sprint_02/S2-18_zoom_and_e2e.md (open point P4)

Owner decisions for this task: none
Additional instructions: html { color-scheme: light dark }, mat.theme with the same palettes in a generated dark variant, the class theme-light / theme-dark on html derived from prefers-color-scheme (no browser storage; the explicit user setting comes with S60 in step 3), prefers-reduced-motion respected. Strong focus indicators per Handoff §1.1 (#184). Remove the Poppins TODO per the Handoff typography. H1 type scale of S04 and S80 per S2-18 P4. Tokens only (--mat-sys-*, --pa-*): no hex values, no px font sizes. The dark values stay generated until the Figma dark mode and its review (O-S5); pin nothing.

Way of working:
1. Branch sprint03/S3-09-theme-foundation from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-09_theme_foundation.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-10 — A11Y-08: axe in the dark appearance

```text
Task S3-10 from docs/sprints/sprint_03/SPRINT_03.md — A11Y-08: axe in the dark appearance
Issues: none
Requirements / invariants: A11Y-08 → active (development part; the persisted setting is deferred to step 3)
Depends on: S3-09 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: TESTING_v23 §4.11 (A11Y-08), §12.4 (suppressions); e2e/support/axe.ts and axe-suppressions.ts; e2e/journeys/

Owner decisions for this task: none
Additional instructions: Repeat the axe run of A11Y-01 with colorScheme 'dark' on every journey state and shell page (Playwright emulation; all five browser projects in CI). Fix contrast findings on shell pages with tokens. Findings on legacy screens (J1 runs on /filters) get one issue and suppressions that name it — the screens are rebuilt from sprint 04. Flip A11Y-08 and record the deferred part (setting persisted in UserPreferences, step 3).

Way of working:
1. Branch sprint03/S3-10-dark-appearance-axe from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-10_dark_appearance_axe.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-11 — Catalog module: design note on persistence and the existing criteria

```text
Task S3-11 from docs/sprints/sprint_03/SPRINT_03.md — Catalog module: design note on persistence and the existing criteria
Issues: none
Requirements / invariants: prepares CAT-05, CAT-07, CAT-10 (stays green), CAT-13, CAT-23
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 §4.3, §5.1, §12 step 2, ADR-014; SPEC_v2 R-ADM-01, SD20; TESTING_v23 §4.2 (CAT-*), §6, §9 step 2; docs/sprints/sprint_01/S1-15_cat10_characterisation.md; the legacy Criteria, CriteriaFilter, FilterType and CriteriaFilterExclusion entities in DAL/MODELS and their controllers

Owner decisions for this task: none (O-S3-5 is taken after this note)
Additional instructions: Write only the design note, as the draft of the protocol, and stop. It answers: CatalogDbContext and schema per ADR-014; which entities and DTOs go into Catalog.Contracts (O-22 interim rule); how the existing criteria and the CriteriaFilter types 1–4 with their exclusions reach the versioned model — (a) a generated legacy release folder imported by the CLI of S3-16, or (b) an EF data migration — and what that means for the order of S3-12 – S3-18; what the legacy endpoints read afterwards and how CAT-10 stays green with unchanged expected sets; dropping seed rows 2 and 3. Options with a recommendation. No code. The PR contains only the note.

Way of working:
1. Branch sprint03/S3-11-catalog-design-note from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-11_catalog_design_note.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-12 — Catalog core entities and first migration

```text
Task S3-12 from docs/sprints/sprint_03/SPRINT_03.md — Catalog core entities and first migration
Issues: none
Requirements / invariants: proposal CAT-05 (unit part) → active; OPS-03 covers the new context
Depends on: S3-11 decided (O-S3-5); no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 §5.1 (catalog), ADR-014; TESTING_v23 §4.2; the protocol of S3-11

Owner decisions for this task: O-S3-5: <copy the decision from S3-11>
Additional instructions: CatalogDbContext with its own migration history, schema catalog. Entities CatalogRelease, Criterion (Key, Kind), CriterionRevision, CatalogReleaseCriterion, StandardReference; Fluent API configurations only. A published release and its revisions cannot be changed in the domain (domain error, not silent). Register through AddCatalogModule; only controllers and the registration are public. Confirm in the plan which invariants this task activates.

Way of working:
1. Branch sprint03/S3-12-catalog-core-entities from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-12_catalog_core_entities.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-13 — Catalog filter dimensions, applicability, priorities and procedure texts

```text
Task S3-13 from docs/sprints/sprint_03/SPRINT_03.md — Catalog filter dimensions, applicability, priorities and procedure texts
Issues: none
Requirements / invariants: proposal CAT-13 → active
Depends on: S3-12 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: ARCHITECTURE_v15 §5.1 (filter layers, exclusions, procedure mapping, priority rules); kriterienkatalog_bit_inklusiv_v1.md (filter layers, exclusions, rating scale); TESTING_v23 §4.2

Owner decisions for this task: O-S3-5: <copy the decision from S3-11>
Additional instructions: AppType, ProductPart (IsAlwaysSelected), UseCaseTemplate (IsActive), TestType, FilterExclusion, CatalogReleaseFilterExclusion, CatalogReleaseAppTypeProcedure, CriterionApplicability, PriorityRecommendation, CriterionProcedureText, with a migration. A FilterExclusion whose target layer is not lower than its source layer cannot be created (CAT-13). No resolution logic yet (S3-20).

Way of working:
1. Branch sprint03/S3-13-catalog-filter-dimensions from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-13_catalog_filter_dimensions.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-14 — MiniCatalog fixture and builders for step 2

```text
Task S3-14 from docs/sprints/sprint_03/SPRINT_03.md — MiniCatalog fixture and builders for step 2
Issues: none
Requirements / invariants: none activated (test data)
Depends on: S3-13 merged

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: TESTING_v23 §6 (builders, canonical fixtures), §9 step 2 (MiniCatalog); tests/ProcureAccess.Tests.Shared; ARCHITECTURE_v15 §5.1

Owner decisions for this task: none
Additional instructions: A MiniCatalog fixture in Tests.Shared: two use cases (one inactive), app types incl. one without requirement criteria (Hardware), product parts incl. Basisfunktionalität (IsAlwaysSelected), two test types, exclusions across layers, priority rules on all four levels incl. a case with several app types, procedure texts with allowed ratings. Builders in the style of TestData.ACriterion(). Use it in at least one test of S3-12/S3-13 and document it in TESTING §6 in the next TESTING version only if another task writes one anyway; otherwise note it as an open point.

Way of working:
1. Branch sprint03/S3-14-mini-catalog from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-14_mini_catalog.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-15 — Release data format and validation

```text
Task S3-15 from docs/sprints/sprint_03/SPRINT_03.md — Release data format and validation
Issues: none
Requirements / invariants: prepares CAT-23
Depends on: S3-12 – S3-14 merged

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-ADM-01, SD20; ARCHITECTURE_v15 C74, §5.1; TESTING_v23 CAT-23

Owner decisions for this task: O-S3-5: <copy the decision from S3-11>
Additional instructions: Folder layout and JSON files of catalog/releases/<version>/ (criteria revisions, procedure texts, applicability, priorities, exclusions, procedure mapping, filter dimensions); a loader and a validator for the whole folder: references, exclusion layers (CAT-13), procedure mapping, allowed ratings. Errors name the file and the JSON path. A small test release folder for the tests (built from the MiniCatalog shape). JSON files need REUSE coverage (REUSE.toml). Document the format for the project team (it is the review artefact of M2.3) in the protocol and in the next ARCHITECTURE version only if another task writes one anyway; otherwise note it.

Way of working:
1. Branch sprint03/S3-15-release-data-format from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-15_release_data_format.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-16 — CLI catalog import

```text
Task S3-16 from docs/sprints/sprint_03/SPRINT_03.md — CLI catalog import
Issues: none
Requirements / invariants: proposal CAT-23 (import part), CAT-05 (integration part), CAT-07 → active
Depends on: S3-15 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-ADM-01; TESTING_v23 CAT-05, CAT-07, CAT-23; CLAUDE.md §5 (how the CLI should be run, add the command there)

Owner decisions for this task: none
Additional instructions: catalog import <version>: validates the whole folder (S3-15) and imports it atomically as an immutable release — any error leaves the database unchanged. Unchanged criteria share their revision with the previous release (CAT-07). Decide in the plan where the CLI lives (e.g. a command mode of the API host or a small console project) and ask before adding a project or a dependency.

Way of working:
1. Branch sprint03/S3-16-catalog-import from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-16_catalog_import.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-17 — CLI catalog activate, active release and cache

```text
Task S3-17 from docs/sprints/sprint_03/SPRINT_03.md — CLI catalog activate, active release and cache
Issues: none
Requirements / invariants: proposal CAT-23 (activate part), CAT-09 → active
Depends on: S3-16 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-ADM-01; TESTING_v23 CAT-09, CAT-23; ARCHITECTURE_v15 §9 (caching)

Owner decisions for this task: none
Additional instructions: catalog activate <version> switches the active release; the next catalog request returns it. Catalog cache keyed by CatalogReleaseId (CAT-09). Add both commands to CLAUDE.md §5.

Way of working:
1. Branch sprint03/S3-17-catalog-activate from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-17_catalog_activate.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-18 — The existing criteria in the catalog model

```text
Task S3-18 from docs/sprints/sprint_03/SPRINT_03.md — The existing criteria in the catalog model
Issues: none
Requirements / invariants: CAT-10 stays green (API and E2E part, expected sets unchanged)
Depends on: S3-17 merged (import path) or S3-13 (data-migration path), per O-S3-5

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: the protocol of S3-11; ARCHITECTURE_v15 §12 step 2; docs/sprints/sprint_01/S1-15_cat10_characterisation.md; tests/…/Catalog/Fixtures/cat-10-criteria-filter.json; e2e/journeys/j1-configurator.spec.ts

Owner decisions for this task: O-S3-5: <copy the decision from S3-11>
Additional instructions: Bring the existing criteria and the CriteriaFilter types 1–4 with their exclusions into the catalog model on the path of O-S3-5; drop seed rows 2 and 3; the legacy filter endpoints keep working against the extended contract. CAT-10's expected sets never change: if they would, stop and report. J1 green on all five browser projects.

Way of working:
1. Branch sprint03/S3-18-existing-criteria-migration from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-18_existing_criteria_migration.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-19 — GET /api/v1/catalog/filter-options

```text
Task S3-19 from docs/sprints/sprint_03/SPRINT_03.md — GET /api/v1/catalog/filter-options
Issues: #153, #176 (milestone per O-S3-6)
Requirements / invariants: proposal CAT-12 (catalog-endpoint part), CAT-18, CAT-19 → active
Depends on: S3-17 and S3-18 merged; O-S3-6; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 153, gh issue view 176) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: SPEC_v2 §8.1 (response shape, C77: excludedBy[] without reason text, appTypesWithoutCriteria[]), §8.6; ARCHITECTURE_v15 §7.1, §7.2; TESTING_v23 CAT-12, CAT-18, CAT-19; TECH_DEBT_v2 §3.3 (#153, #176)

Owner decisions for this task: O-S3-6: <controllers | minimal APIs for /api/v1/catalog/*; milestone of #176>
Additional instructions: The endpoint in the Catalog module per SPEC §8.1, pinned to the active release or a given release id; inactive use cases not offered but still resolvable; exclusions release-pinned. Fix the versioned routes and the duplicate Swagger parameters (#153). Record deferred parts of CAT-12 (configurator, export, directory) in TESTING.

Way of working:
1. Branch sprint03/S3-19-filter-options-endpoint from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-19_filter_options_endpoint.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-20 — GET /api/v1/catalog/criteria with resolved priorities

```text
Task S3-20 from docs/sprints/sprint_03/SPRINT_03.md — GET /api/v1/catalog/criteria with resolved priorities
Issues: none
Requirements / invariants: proposal CAT-01, CAT-02, CAT-06 (release pinning by id), CAT-11, CAT-14, CAT-16 (selection and detail part), CAT-20 (contract part), CAT-21 → active
Depends on: S3-19 merged; no other open branch that creates a TESTING version

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 §8.2 (response shape, matchedRule.kind), §7.1; ARCHITECTURE_v15 §5.1 (priority resolution), C77; TESTING_v23 §4.2; CLAUDE.md §8 (priorities explain themselves)

Owner decisions for this task: none
Additional instructions: Selection validation with field errors (CAT-11). Priority resolution most-specific-first over the four levels, per app type with the strictest result (A > B > Info, then weight), matchedRule naming the deciding app type; every resolved priority with rationale, source „Empfehlung des Projektteams“ and matchedRule (CAT-01, CAT-02, CAT-14) — table-driven tests over MiniCatalog first. Allowed ratings and procedure texts of the selected app types (CAT-16, CAT-21); one procedure criterion per selected test type; empty requirement list plus appTypesWithoutCriteria[] (CAT-20). If the plan shows more than one session, propose a split before coding.

Way of working:
1. Branch sprint03/S3-20-criteria-endpoint from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-20_criteria_endpoint.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-21 — Footer link „Kriterium vorschlagen“

```text
Task S3-21 from docs/sprints/sprint_03/SPRINT_03.md — Footer link „Kriterium vorschlagen“
Issues: none
Requirements / invariants: R-SUG-01 (footer part); A11Y-01 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: SPEC_v2 R-SUG-01, R-LST-13, §13 (O-6); ux_copy_procureaccess_v15.md (suggest.*, shell.footer.*); figma_code_mapping_procureaccess_v1.md; the footer of S2-15

Owner decisions for this task: none
Additional instructions: The footer link opens the configured channel Contact:CriteriaSuggestion: mailto: with subject suggest.mailSubject and the body text of the UX copy, or a URL. The value is a 🔧 placeholder until O-6 is decided; the production build fails while it is the placeholder (the same mechanism GP.3 plans for the legal pages — propose it in the plan). The S11 part comes with M2.6. Texts only from the UX copy (check-i18n --write).

Way of working:
1. Branch sprint03/S3-21-criteria-suggestion-link from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-21_criteria_suggestion_link.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

## P2 — stretch

### S3-22 — Snackbar buttons in German *(stretch)*

```text
Task S3-22 from docs/sprints/sprint_03/SPRINT_03.md — Snackbar buttons in German
Issues: #160
Requirements / invariants: none
Depends on: the UX copy session #181 (wording exists); stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 160) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 §3.1 (#160); ux_copy_procureaccess (the version written in session #181)

Owner decisions for this task: none
Additional instructions: Stretch task. Replace 'Close' / 'Dismiss' with the keys the UX copy now defines (check-i18n --write). If the UX copy has no wording yet, stop.

Way of working:
1. Branch sprint03/S3-22-snackbar-buttons from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-22_snackbar_buttons.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-23 — Usage events: fire-and-forget writes *(stretch)*

```text
Task S3-23 from docs/sprints/sprint_03/SPRINT_03.md — Usage events: fire-and-forget writes
Issues: #115
Requirements / invariants: PRV-05 stays green
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 115) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 §3.3 (#115); docs/sprints/sprint_01/S1-23_usage_event_table.md (open point P4); SPEC_v2 R-MET-01, §9.1

Owner decisions for this task: none
Additional instructions: Stretch task. The writer no longer awaits the insert on the request path: fire-and-forget with a short retry delay, failures logged and swallowed as today. Test it on the fake clock. Nothing calls the writer yet; its first callers come with M2.6/M2.7.

Way of working:
1. Branch sprint03/S3-23-usage-events-fire-and-forget from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-23_usage_events_fire_and_forget.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-24 — Persist the Data Protection keys outside the container *(stretch)*

```text
Task S3-24 from docs/sprints/sprint_03/SPRINT_03.md — Persist the Data Protection keys outside the container
Issues: #186
Requirements / invariants: prepares OPS-04 (step 3)
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-24 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 186) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: TECH_DEBT_v2 TD-21; docs/sprints/sprint_02/S2-21_api_dockerfile_swagger.md (open point P3); ARCHITECTURE_v15 C60; TESTING_v23 OPS-04

Owner decisions for this task: none
Additional instructions: Stretch task. Persist the Data Protection key ring in the database (as ARCHITECTURE C60 plans for the cookie session), in the Platform module with its migration. OPS-04 stays planned; note how its test will work.

Way of working:
1. Branch sprint03/S3-24-data-protection-keys from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-24_data_protection_keys.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-25 — Material Symbols font subsetting *(stretch)*

```text
Task S3-25 from docs/sprints/sprint_03/SPRINT_03.md — Material Symbols font subsetting
Issues: #135
Requirements / invariants: A11Y-01 stays green; CLAUDE §8 (no third-party requests)
Depends on: O-S3-7; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-25 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 135) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: #135 (options); docs/sprints/sprint_02/S2-03_self_hosted_icons.md (open point P1)

Owner decisions for this task: O-S3-7: <option of #135>
Additional instructions: Stretch task. Implement the chosen option; show bundle size and first-visit behaviour before and after in the protocol. Licence check of any new tool (CLAUDE §10) and my OK before adding it.

Way of working:
1. Branch sprint03/S3-25-font-subsetting from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-25_font_subsetting.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-26 — ESLint for e2e/ and scripts *(stretch)*

```text
Task S3-26 from docs/sprints/sprint_03/SPRINT_03.md — ESLint for e2e/ and scripts
Issues: #161
Requirements / invariants: none (CI stage 1)
Depends on: O-S3-8 if eslint-plugin-playwright is wanted; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-26 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 161) and their entries in TECH_DEBT_v2.md; always the highest version of each document in docs/.
Also read: #161; docs/sprints/sprint_02/S2-14_typescript_lint.md (open point P4); CLIENT eslint.config.js

Owner decisions for this task: O-S3-8: <eslint-plugin-playwright: yes | no>
Additional instructions: Stretch task. ESLint with the type-aware TypeScript rules for e2e/ and for scripts/*.mjs, run in CI stage 1; the missing await before a Playwright expect must be reported (with or without the plugin, per O-S3-8). Fix the findings or list them in the protocol.

Way of working:
1. Branch sprint03/S3-26-lint-e2e-scripts from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-26_lint_e2e_scripts.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S3-27 — Release 2026.1, part 1: texts without priorities *(stretch)*

```text
Task S3-27 from docs/sprints/sprint_03/SPRINT_03.md — Release 2026.1, part 1: texts without priorities
Issues: none
Requirements / invariants: CAT-23 validation must pass on the folder
Depends on: S3-15 merged; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_03.md the row of S3-27 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/.
Also read: kriterienkatalog_bit_inklusiv_v1.md, kriterienkatalog_bit_inklusiv_v1_teil_a.md, kriterienkatalog_bit_inklusiv_v1_teil_b.md; SPEC_v2 R-ADM-01, §13 (O-1, K-1); ARCHITECTURE_v15 §12 step 2 (one procedure criterion per test type)

Owner decisions for this task: none
Additional instructions: Stretch task. Build catalog/releases/2026.1/ in the format of S3-15 from the BIT inklusiv texts of Teil A/B, with the filter dimensions and exclusions of the Kriterienkatalog and one procedure criterion per test type. No priorities, rationales or plain-language titles: those come from the project team (O-1, K-1) — leave them out and list what is missing per criterion, so the team can fill it in. Do not invent wording. Validate with S3-15; do not import or activate it.

Way of working:
1. Branch sprint03/S3-27-release-2026-1-texts from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CAT-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_03/S3-27_release_2026_1_texts.md in the structure of sprints 01 and 02 (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

Replace `<S>` with the sprint number (e.g. 4), `<SS>` with the two-digit sprint folder number (e.g. 04) and `<v>` with the current tech-debt register version.

```text
Task S<S>-<NN> from docs/sprints/sprint_<SS>/SPRINT_<SS>.md — <title of the row>
Issues: <#n, … | none>
Requirements / invariants: <R-… / INV IDs the task activates or must keep green>
Depends on: <tasks, owner decisions | nothing>

Read first: CLAUDE.md; in SPRINT_<SS>.md the row of S<S>-<NN> plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view <n>) and their entries in TECH_DEBT_v<v>.md; always the highest version of each document in docs/.
Also read: <documents, sections and earlier task protocols in docs/sprints/ that the row or an issue points to>

Owner decisions for this task: <none | O-S<S>-n: <decision> (decided <date>)>
Additional instructions: <none | …>

Way of working:
1. Branch sprint<SS>/S<S>-<NN>-<topic> from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_<SS>/S<S>-<NN>_<topic>.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

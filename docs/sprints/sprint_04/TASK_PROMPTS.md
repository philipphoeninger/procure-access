# Sprint 04 — task prompts for Claude Code

> **Status:** v1.1 (2026-11-11; v1.1: S4-29 added — Figma light colours, owner decision O-S4-22, first added as O-S4-7, an ID S4-03 already used). v1 (2026-11-10). Companion to `SPRINT_04.md`: one ready-to-paste prompt per task, built from the generic template at the end of `docs/sprints/sprint_03/TASK_PROMPTS.md` (repeated, updated, at the end of this file). Claude Code implements; the owner's control chat for sprint 04 takes the decisions and reviews the reports.
> **How to use:** copy the block of the task into a new Claude Code session (or a worktree session for parallel work). Replace only the placeholders in `<…>` after "Owner decisions for this task" (decisions not yet taken, see "Open placeholders"). `<invariant ID>`, `<NN>`, `<n>`, `<Name>` or `<date>` elsewhere in a prompt are naming patterns for Claude Code, not placeholders for you. Everything else is filled in from `SPRINT_04.md`, `TECH_DEBT_v3.md` and the GitHub issues and pull requests as of 2026-11-10.
> **Changes to the template since sprint 03:** reads `TECH_DEBT_v3.md`; document names in "Also read" carry **no version numbers** (sprint 03 had to correct its prompts when ARCHITECTURE and TESTING moved on, v1.1 of its prompts); step 2 asks for a split when a task needs more than one session; step 3 renumbers a TESTING version after a rebase instead of forbidding parallel flips; step 4 leaves E2E to CI when another session holds the local E2E stack; step 5 also names `LEGACY_EXCEPTIONS`.

---

## Overview

Prompts are listed in task-ID order. The recommended **working order** is: S4-01, S4-02, S4-03, S4-04, S4-05, S4-06, S4-07, S4-08, S4-23, S4-09, S4-10, S4-11, S4-12, S4-13, S4-29, S4-19, S4-20, S4-14, S4-15, S4-16, S4-17, S4-18a, S4-18b, S4-21, S4-22, then the stretch tasks (S4-23 runs early because S11 uses the endpoints; S4-19 and S4-20 can run while S10 is built).

| Task | Title | Issues | Can start |
|---|---|---|---|
| S4-01 | E2E reliability: Playwright install no longer fails main | #203 | after O-S4-1 (first task) |
| S4-02 | UX copy v17: proposals for every gap of #181 | #181 | ready |
| S4-03 | UsersController requires authentication | #205 | ready |
| S4-04 | Scripts and licence: CodeQL alert 3 and the CAT-10 fixture | #233, #242 | ready |
| S4-05 | Validate every catalog release folder in CI | #239 | after O-S4-2 |
| S4-06 | Review Dependabot pull requests #226 – #231 | PR #226 (@playwright/test 1.65 in e2e/), PR #227 (Angular group), PR #228 (Vitest group), PR #229 (other group), PR #230 (jsdom 31, a major), PR #231 (xunit.v3 4.0.3) | ready |
| S4-07 | Documentation session: sprint-03 findings | #243 | ready (best after the other D1 document changes) |
| S4-08 | An active test release in the E2E stack (and in development) | #241 | after O-S4-3 |
| S4-09 | Configurator state: in-memory draft, catalog client, selection rules | none | after S4-08 |
| S4-10 | S10 frame and step 1: stepper, use-case combobox, accessible step names | #52 | after S4-09 |
| S4-11 | S10 steps 2 – 4 with exclusions | none | after S4-10 |
| S4-12 | S10 reset notice, „Bisherige Auswahl“, hand-over to S11, legacy switch | none | after S4-11 and O-S4-3 |
| S4-13 | J1 on S10, keyboard only; CAT-10 after S10 | #91, #92 | after S4-12 and O-S4-5 |
| S4-14 | S11 state: criteria client, list draft, search and filter logic | none | after S4-12 |
| S4-15 | Criterion card, detail panel and K5 | none | after S4-14 and O-S4-4 |
| S4-16 | Priority override, include/exclude and notes | none | after S4-15 |
| S4-17 | Search, priority filter, „Geprüft nach …“, empty state | none | after S4-16 |
| S4-18a | S11 summary, notices, action bar, suggestion link and page titles | none | after S4-14 – S4-17 |
| S4-18b | `CriteriaListShown` endpoint and its client caller | none | after S4-14 – S4-17; independent of S4-18a |
| S4-19 | Export pipeline and POST /api/v1/exports | none | after S4-08 |
| S4-20 | XLSX and CSV renderers | none | after S4-19 (O-S4-6 for one column) |
| S4-21 | Export dialog S13, rate limit and /health/ready | #112 | after S4-19, S4-20, S4-18a |
| S4-22 | J1 complete to the export; M2 exit check; bundle budget | #42 | after S4-13, S4-18a, S4-18b, S4-21 |
| S4-23 | Accept-Language on the catalog list endpoints | none | ready |
| S4-29 | Figma light colours pinned in the theme | #240 (light part) | after S4-13 (O-S4-22 decided; before S4-14) |
| S4-24 | Short description in the catalog model | #237 | stretch, after O-S4-6 |
| S4-25 | M3.4 groundwork: contributor profiles and the Moderator role | none | stretch, ready |
| S4-26 | Encrypt the Data Protection key ring and document it | #235 | stretch, ready |
| S4-27 | Licence notices of the NuGet packages in the API image | #187 | stretch, ready |
| S4-28 | Dead code: dialog service, unused logger, assets location | none | stretch, ready |

## Open placeholders

| Placeholder | Where | Fill in after |
|---|---|---|
| O-S4-1 (E2E flake option) | S4-01 | owner choice (D1, before everything else) |
| O-S4-2 (release-folder validation) | S4-05 | owner choice (any time) |
| O-S4-3 (S10 vs legacy filter while no release is active) | S4-08, S4-12 | owner choice (D1) |
| O-S4-4 (UX copy v17 approved) | S4-15 – S4-18 | the owner's review of the S4-02 PR |
| O-S4-5 (CAT-10 after S10) | S4-13 | owner choice (any time before S4-13) |
| O-S4-6 (short description) | S4-20, S4-24 | owner choice (before S4-20) |
| O-S4-22 (theme colours) | S4-29 | **decided 2026-11-11** — already filled in |

## Rules for running tasks

- **TESTING versions:** S4-10 – S4-13, S4-17, S4-19 – S4-21 and S4-23 flip invariants. Preferably one at a time; if two run in parallel, the later PR rebases and renumbers its TESTING version.
- **Parallel sessions:** frontend (S4-09 – S4-18) and backend (S4-03, S4-05, S4-08, S4-19, S4-20, S4-23) can run in separate worktrees (`claude --worktree s4-nn`). Only one session runs the E2E stack locally at a time (shared host ports); the others leave E2E to CI.
- **S4-01 first:** until the E2E flake is fixed, a red E2E job is probably noise; after it, it is a real failure.
- **S4-02 is a draft PR for the owner:** the wording is the owner's decision; S11 tasks wait for it, S10 does not.
- **Stretch tasks** (S4-24 – S4-28) only after all P0 tasks are done.
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and fills the placeholders of the next task.
- **End of sprint:** close-out table in `SPRINT_04.md` and `TECH_DEBT_v4.md` (process step 14), prepared in the control chat.

---

## P0 — committed

### S4-01 — E2E reliability: Playwright install no longer fails main

```text
Task S4-01 from docs/sprints/sprint_04/SPRINT_04.md — E2E reliability: Playwright install no longer fails main
Issues: #203
Requirements / invariants: none (CI)
Depends on: O-S4-1

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-01 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 203) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: .github/workflows/ci.yml (E2E jobs, Playwright install); TECH_DEBT_v3 §3.5 and §6 (#203, the three options)

Owner decisions for this task: O-S4-1: <(1) pinned Playwright container image | (2) shorter limit plus one retry of the apt step | (3) cache the .deb packages>
Additional instructions: Implement the chosen option in the E2E jobs only. Prove it: list the CI runs (incl. re-runs on a test branch) that passed the install step, and say how long the step takes now. If option (1): licence check of the image (CLAUDE §10) and pin it by digest. This task goes first in the sprint so the other merges are judged on real failures.

Way of working:
1. Branch sprint04/S4-01-e2e-reliability from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-01_e2e_reliability.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-02 — UX copy v17: proposals for every gap of #181

```text
Task S4-02 from docs/sprints/sprint_04/SPRINT_04.md — UX copy v17: proposals for every gap of #181
Issues: #181
Requirements / invariants: CLAUDE §6.5 (UI strings only from the UX copy)
Depends on: nothing; the owner reviews the PR (O-S4-4)

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-02 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 181) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: the highest ux_copy_procureaccess_v*.md (German; §1 principles, §2 glossary, §5, §6, §8 open points); TECH_DEBT_v2 §6 and TECH_DEBT_v3 §6 (the list of #181 gaps); the open points named there: docs/sprints/sprint_02/S2-01_live_bugs.md P3, S2-11_client_error_mapping.md P1, S2-12_pluralisation.md P1 and F2, S2-13_de_json.md P1, S2-15_shell_frame.md P4, S2-16_route_change.md P2, S2-18_zoom_and_e2e.md P3; docs/sprints/sprint_03/S3-20a_priority_resolution.md P3, S3-21_criteria_suggestion_link.md P1; SPEC §8.6 (problem types)

Owner decisions for this task: none (the owner decides the wording in the review of this PR — O-S4-4)
Additional instructions: The owner asked Claude to generate the missing UI texts. Write ux_copy_procureaccess_v17.md (German, like the earlier versions: „Sie“, gender-neutral forms without star or colon, plain language, the glossary terms of §2) with a proposal for every gap of #181: singular and zero forms of the 18 rows of S2-12 F2 and the „/“ forms as .one/.zero keys; the storage rule for dotted keys and the de.json path in §5; page-title variants of pageTitle.criteria.list and pageTitle.publish; UX keys for the problem types without one (also name them for SPEC §8.6); the reason text for the disabled „Löschen“; the soft hyphen in the legal titles; the account button name vs the visible „Konto“ (WCAG 2.5.3); the key and text for selection-excluded; whether suggest.mailBody uses real line breaks; the header naming current document versions. Mark every new or changed text as a proposal (e.g. „Vorschlag (S4-02)“ in the change list) and say in one line why it reads that way. Do not change de.json, code or ⚖️ texts. The protocol lists every gap with its new key. Open the PR as a draft for the owner's review.

Way of working:
1. Branch sprint04/S4-02-ux-copy-v17 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-02_ux_copy_v17.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-03 — UsersController requires authentication

```text
Task S4-03 from docs/sprints/sprint_04/SPRINT_04.md — UsersController requires authentication
Issues: #205
Requirements / invariants: SEC-01 (legacy JWT) unchanged; API-01 stays green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-03 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 205) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: docs/sprints/sprint_03/S3-08_build_warnings.md (open point P2, review finding R2); TECH_DEBT_v3 §3.2 (#205)

Owner decisions for this task: none
Additional instructions: [Authorize] on UsersController, [AllowAnonymous] only on ResetPassword (check the other actions against the legacy client before you decide), integration tests that anonymous calls answer 401, and the null-forgiving ! on the user id replaced by a real check. No other behaviour change; the controller is replaced in step 3.

Way of working:
1. Branch sprint04/S4-03-users-controller-authorize from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-03_users_controller_authorize.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-04 — Scripts and licence: CodeQL alert 3 and the CAT-10 fixture

```text
Task S4-04 from docs/sprints/sprint_04/SPRINT_04.md — Scripts and licence: CodeQL alert 3 and the CAT-10 fixture
Issues: #233, #242
Requirements / invariants: none (CI stage 1)
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-04 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 233, gh issue view 242) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: scripts/check-i18n.mjs and its test; REUSE.toml; docs/sprints/sprint_03/S3-04_scripts_selftests.md (R2, P1); docs/sprints/sprint_03/S3-11_catalog_design_note.md (P3)

Owner decisions for this task: none
Additional instructions: #233: guard the loop variable name inside the loop of setText as well, so the CodeQL analysis on main accepts it; add a self-test case; after the merge the protocol records that alert 3 is closed. #242: annotate tests/…/Catalog/Fixtures/cat-10-criteria-filter.json as BIT inklusiv in REUSE.toml like the seed and the release folders.

Way of working:
1. Branch sprint04/S4-04-scripts-licence from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-04_scripts_licence.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-05 — Validate every catalog release folder in CI

```text
Task S4-05 from docs/sprints/sprint_04/SPRINT_04.md — Validate every catalog release folder in CI
Issues: #239
Requirements / invariants: CAT-23 unchanged
Depends on: O-S4-2

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-05 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 239) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: docs/sprints/sprint_03/S3-16_catalog_import.md (open point P5); the release reader and validator of S3-15; .github/workflows/ci.yml

Owner decisions for this task: O-S4-2: <CI step running the validator on every folder | a catalog validate <version> command used by CI>
Additional instructions: Every folder under catalog/releases/ (incl. 2026.1, which has no priorities yet — validation must accept that, as O-S3-5 Q2 (iii) does for import) is validated in CI; a deliberately broken test folder on a scratch branch fails with the validator's message (file and JSON path). Document in the protocol how the project team runs the same check locally.

Way of working:
1. Branch sprint04/S4-05-release-folder-validation from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-05_release_folder_validation.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-06 — Review Dependabot pull requests #226 – #231

```text
Task S4-06 from docs/sprints/sprint_04/SPRINT_04.md — Review Dependabot pull requests #226 – #231
Issues: PR #226 (@playwright/test 1.65 in e2e/), PR #227 (Angular group), PR #228 (Vitest group), PR #229 (other group), PR #230 (jsdom 31, a major), PR #231 (xunit.v3 4.0.3)
Requirements / invariants: CLAUDE §10 (licences); A11Y-01 and the component tests stay green
Depends on: nothing

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-06 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the pull requests (gh pr view 226, gh pr view 227, gh pr view 228, gh pr view 229, gh pr view 230, gh pr view 231); always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: docs/sprints/sprint_03/S3-01_dependabot_prs.md (how sprint 03 reviewed Dependabot PRs); TECH_DEBT_v3 §7 and §9.6

Owner decisions for this task: none
Additional instructions: This task reviews existing pull requests instead of building something: steps 1 and 3 of the way of working do not apply. For every PR check the licence of each new version (CLAUDE §10), the changelog for breaking changes (jsdom 31 and xunit.v3 4 are majors: run the affected test suites; Playwright 1.65: the container image or install step of S4-01 must match), and the CI result. The protocol recommends merge or close per PR with the reason; open a small PR that contains only the protocol. Do not merge any of the Dependabot PRs.

Way of working:
1. Branch sprint04/S4-06-dependabot-prs from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-06_dependabot_prs.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-07 — Documentation session: sprint-03 findings

```text
Task S4-07 from docs/sprints/sprint_04/SPRINT_04.md — Documentation session: sprint-03 findings
Issues: #243
Requirements / invariants: none (documents only)
Depends on: nothing; best after the other D1 tasks that write documents

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-07 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 243) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v3 §3.6 (TD-37, the full list of points); the open points named there in docs/sprints/sprint_03/

Owner decisions for this task: none
Additional instructions: Carry the TD-37 points into the next versions of ARCHITECTURE (status line, Category in §5.1), TESTING (input line, §8.2 stage 9 and V85 vs the ruleset that blocks on high_or_higher), SPEC (R-SHL-10 names A11Y-09, §11 step-1 invariants incl. A11Y-08/09, the R-SET-03 clause, the current-versions line), the Design handoff (UX copy version, §1.3 subsets, sources line) and the Figma mapping (body and related line). ROADMAP_v4 and the SPRINT_03 close-out are already done; do not touch them. PRD, Kriterienkatalog and UX copy are the owner's documents: list their points in the protocol instead of editing them. Do not change any rule or requirement beyond the listed points.

Way of working:
1. Branch sprint04/S4-07-documentation-session from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-07_documentation_session.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-08 — An active test release in the E2E stack (and in development)

```text
Task S4-08 from docs/sprints/sprint_04/SPRINT_04.md — An active test release in the E2E stack (and in development)
Issues: #241
Requirements / invariants: CAT-23 unchanged; prepares CFG-*, EXP-* E2E parts
Depends on: O-S4-3

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-08 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 241) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §6.3 (E2E dataset), §9 step 2; the test folder T.1 and the CLI of S3-16/S3-17; docker-compose.e2e.yml; e2e/support/global-setup.ts; docs/sprints/sprint_03/S3-17_catalog_activate.md (P4), S3-19_filter_options_endpoint.md (P1), S3-20b_criteria_endpoint.md (P1)

Owner decisions for this task: O-S4-3: <(a) S10 only where a release is active, legacy filter elsewhere | (b) T.1 also active in Development | (c) replace the legacy filter everywhere now — and the combination chosen>
Additional instructions: Build the E2E seeder of TESTING §6.3 for the catalog part: in the E2E environment (only there, and in Development if O-S4-3 says so) it imports and activates the test folder T.1, idempotently. Never in Staging or Production. Show with an E2E smoke test that /api/v1/catalog/filter-options and /criteria answer 200 on the stack. Keep the users and products part of the seeder for later steps (note it as an open point).

Way of working:
1. Branch sprint04/S4-08-e2e-test-release from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-08_e2e_test_release.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-09 — Configurator state: in-memory draft, catalog client, selection rules

```text
Task S4-09 from docs/sprints/sprint_04/SPRINT_04.md — Configurator state: in-memory draft, catalog client, selection rules
Issues: none
Requirements / invariants: R-CON-09; prepares CFG-01 – CFG-04, CAT-12 (configurator part)
Depends on: S4-08 merged

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-09 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CON-01 – R-CON-10, §8.1 (filter-options response, excludedBy[]); ARCHITECTURE §8 (store slices, withConfigurator()); CLAUDE.md §9 (zoneless, signals); Design handoff §5.4 (filter layers, exclusions, reset)

Owner decisions for this task: none
Additional instructions: withConfigurator() holds the four-layer draft in memory only (never browser storage — test it with storage blocked). A typed API client for GET /api/v1/catalog/filter-options. Selection rules as pure functions with tests: at most one use case or „Keiner davon passt“, at least one app type, Basisfunktionalität always selected, exclusions from excludedBy[], changing a higher layer clears every lower layer except Basisfunktionalität and reports which layers were cleared. Lives under features/configurator/ per CLAUDE §4 target layout. No screen yet.

Way of working:
1. Branch sprint04/S4-09-configurator-state from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-09_configurator_state.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-10 — S10 frame and step 1: stepper, use-case combobox, accessible step names

```text
Task S4-10 from docs/sprints/sprint_04/SPRINT_04.md — S10 frame and step 1: stepper, use-case combobox, accessible step names
Issues: #52
Requirements / invariants: R-CON-01, R-CON-02; CFG-03 → active; A11Y-01 (the #52 suppression goes)
Depends on: S4-09 merged; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-10 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 52) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CON-01, R-CON-02; Design handoff §5.1 (S10 row), §5.4, §2.2 (combobox on the on-screen keyboard); ux_copy §5.4 (criteria.select.*); the Figma frames of S10 step 1, S10-M1 and S10-E listed in the Figma mapping; e2e/support/axe-suppressions.ts; the icon list of S3-25

Owner decisions for this task: none
Additional instructions: S10 at /kriterien inside the shell: linear mat-stepper with four steps, compact layout below 600 px, every step with an accessible name (#52; remove its axe suppression), step 1 as single-choice combobox with „Keiner davon passt“ always last also while filtering and the match-count announcement, the error state of S10-E. Add the stepper icons to the icon list and regenerate the subset. Set up vitest-axe (licence check, my OK) and use it in the first component test. Add the criteria.select area to AREAS and take the texts with check-i18n --write. @figma tags per the mapping.

Way of working:
1. Branch sprint04/S4-10-s10-frame-step1 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-10_s10_frame_step1.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-11 — S10 steps 2 – 4 with exclusions

```text
Task S4-11 from docs/sprints/sprint_04/SPRINT_04.md — S10 steps 2 – 4 with exclusions
Issues: none
Requirements / invariants: R-CON-03 – R-CON-06; CFG-02, CFG-04 and the configurator part of CAT-12 → active
Depends on: S4-10 merged; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-11 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CON-03 – R-CON-06, §8.1; Design handoff §5.4; ux_copy §5.4 (criteria.select.excluded.*, explanations); the Figma frames of S10 steps 2 – 4 and S10-M

Owner decisions for this task: none
Additional instructions: Step 2 app types (at least one, hints for offen/geschlossen), step 3 product parts with Basisfunktionalität checked and aria-disabled plus the toggle „Erklärungen anzeigen“, step 4 optional test types with the link; excluded options stay in the DOM, focusable, aria-disabled (not disabled) with their reason linked by aria-describedby, and activating them changes nothing. Component tests by role and name with vitest-axe.

Way of working:
1. Branch sprint04/S4-11-s10-steps-2-4 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-11_s10_steps_2_4.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-12 — S10 reset notice, „Bisherige Auswahl“, hand-over to S11, legacy switch

```text
Task S4-12 from docs/sprints/sprint_04/SPRINT_04.md — S10 reset notice, „Bisherige Auswahl“, hand-over to S11, legacy switch
Issues: none
Requirements / invariants: R-CON-07, R-CON-08, R-CON-10; CFG-01 → active
Depends on: S4-11 merged; O-S4-3; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-12 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-CON-07, R-CON-08, R-CON-10; Design handoff §5.4, §6.2 (empty states, button spinner); ux_copy §5.4 (criteria.select.reset.*, criteria.select.summary), §5.16 (page titles); TECH_DEBT_v3 §5 (legacy LiveAnnouncer calls); the Figma frame S10-R

Owner decisions for this task: O-S4-3: <copy the decision from S4-08>
Additional instructions: Reset notice in a role=status region naming the cleared layers; „Bisherige Auswahl“ from step 2 on with „Ändern“ per layer; „Kriterienliste anzeigen“ navigates to /kriterien/ergebnis with the selection (S11 is built later — a placeholder route is fine) and the server validates again; page title per §5.16; the configurator's legacy LiveAnnouncer calls through A11yAnnouncerService; empty states and the button spinner. Implement the switch of O-S4-3 in one place: start page card and header lead to S10 where a release is active, to the legacy /filters elsewhere. Remove the legacy filter files from LEGACY_EXCEPTIONS only if they are deleted.

Way of working:
1. Branch sprint04/S4-12-s10-reset-summary-switch from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-12_s10_reset_summary_switch.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-13 — J1 on S10, keyboard only; CAT-10 after S10

```text
Task S4-13 from docs/sprints/sprint_04/SPRINT_04.md — J1 on S10, keyboard only; CAT-10 after S10
Issues: #91, #92
Requirements / invariants: A11Y-02 (J1 part) → active; A11Y-01, A11Y-08, A11Y-09 stay green; CAT-10 per O-S4-5
Depends on: S4-12 merged; O-S4-5; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-13 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 91, gh issue view 92) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §4.2 (CAT-10), §4.11, §5.4 (journeys); e2e/journeys/j1-configurator.spec.ts; e2e reflow.spec.ts; docs/sprints/sprint_01/S1-15_cat10_characterisation.md; docs/sprints/sprint_03/S3-11_catalog_design_note.md (F6)

Owner decisions for this task: O-S4-5: <keep the CAT-10 API part while the legacy endpoints exist, retire the E2E part | new characterisation on S10 with T.1 | retire CAT-10 when the legacy filter is unreachable>
Additional instructions: Rewrite J1 on S10 with keyboard only (no pointer events) and axe in both appearances, on all five browser projects in CI, with expected values from the test release T.1. Apply O-S4-5 to CAT-10 and fix #91 and #92 if the API part stays. Add S10 to reflow.spec.ts. J1 ends at the hand-over to S11 for now; S4-22 extends it.

Way of working:
1. Branch sprint04/S4-13-j1-on-s10 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-13_j1_on_s10.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-14 — S11 state: criteria client, list draft, search and filter logic

```text
Task S4-14 from docs/sprints/sprint_04/SPRINT_04.md — S11 state: criteria client, list draft, search and filter logic
Issues: none
Requirements / invariants: prepares R-LST-01 – R-LST-06; CAT-08 stays planned (step 3)
Depends on: S4-12 merged

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-14 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-01 – R-LST-06, §7.2 (note length), §8.2 (criteria response); ARCHITECTURE §8 (withConfigurator()); CLAUDE.md §8 (priorities explain themselves)

Owner decisions for this task: none
Additional instructions: A typed API client for GET /api/v1/catalog/criteria; the list state in the in-memory draft: include/exclude per criterion, an override that keeps the recommended level next to it, a note per criterion (≤ 1 000 characters, server-authoritative limits per §7.2); client-side search with 300 ms debounce over the loaded list and the priority filter, as pure functions with tests. No screen yet.

Way of working:
1. Branch sprint04/S4-14-s11-state from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-14_s11_state.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-15 — Criterion card, detail panel and K5

```text
Task S4-15 from docs/sprints/sprint_04/SPRINT_04.md — Criterion card, detail panel and K5
Issues: none
Requirements / invariants: R-LST-01, R-LST-02; CAT-02 (UI shows rationale, rule and source)
Depends on: S4-14 merged; O-S4-4 for new texts

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-15 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-01, R-LST-02; Design handoff §4.5 (K5 pa-priority-indicator), §5.5; ux_copy v17 §5.5 (criteria.list.*, criteria.item.*, criteria.priority.*); the Figma frames of S11 and the K5 component in the Figma mapping

Owner decisions for this task: O-S4-4: <UX copy v17 approved on <date> | texts of v16 only>
Additional instructions: Criterion cards (title H2, detail sections H3) and the detail panel per Handoff §5.5; K5 with rationale, matched rule (matchedRule.kind appTypeAndUseCase → criteria.priority.rule.full) and source „Empfehlung des Projektteams“, never without them; the Markdown lines of the texts rendered as paragraphs and lists (S3-15 P6), safely (no raw HTML). Component tests by role and name with vitest-axe; @figma tags; criteria.list / criteria.item / criteria.priority areas into AREAS with check-i18n --write.

Way of working:
1. Branch sprint04/S4-15-criterion-card-k5 from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-15_criterion_card_k5.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-16 — Priority override, include/exclude and notes

```text
Task S4-16 from docs/sprints/sprint_04/SPRINT_04.md — Priority override, include/exclude and notes
Issues: none
Requirements / invariants: R-LST-03 – R-LST-05
Depends on: S4-15 merged

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-16 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-03 – R-LST-05, §7.1, §7.2; Design handoff §4.5 (K5 menu), §5.5 (excluded cards); ux_copy v17 §5.5

Owner decisions for this task: O-S4-4: <copy from S4-15>
Additional instructions: Override through the K5 menu with „Zurücksetzen“, the recommendation stays visible; include/exclude with excluded cards kept in the list (surface-container-low, „Nicht in Ihrer Liste“); note with the length error per §7.1. Everything stays in the in-memory draft. Component tests with vitest-axe.

Way of working:
1. Branch sprint04/S4-16-override-include-note from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-16_override_include_note.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-17 — Search, priority filter, „Geprüft nach …“, empty state

```text
Task S4-17 from docs/sprints/sprint_04/SPRINT_04.md — Search, priority filter, „Geprüft nach …“, empty state
Issues: none
Requirements / invariants: R-LST-06 – R-LST-08; CAT-20 (FE part) → active
Depends on: S4-16 merged; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-17 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-06 – R-LST-08; Design handoff §5.5 ("Geprüft nach" items); ux_copy v17 §5.5 (common.results.shown, criteria.item.testedBy, criteria.list.noCriteriaFor)

Owner decisions for this task: O-S4-4: <copy from S4-15>
Additional instructions: Search field and priority filter with the result count announced once (common.results.shown); „Geprüft nach …“ cards for selected test types (priority B, deselectable, short detail panel); the empty state for app types without criteria from appTypesWithoutCriteria[]. Flip the FE part of CAT-20.

Way of working:
1. Branch sprint04/S4-17-search-filter-tested-empty from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-17_search_filter_tested_empty.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-18 — S11 notices, action bar, suggestion link, event and page titles

> **Split 2026-11-11 (O-S4-52 (a)):** S4-18a builds everything below except the event; S4-18b builds the `CriteriaListShown` caller (O-S4-48 (a): `POST /api/v1/configurator/criteria-list-shown` in the Configurator module, called once per new selection; SPEC §8 contract, ARCHITECTURE entry, integration test). Owner decisions O-S4-48 – O-S4-53 in SPRINT_04.md. The prompt below was used for S4-18a; for S4-18b replace the title, the scope and the branch (`sprint04/S4-18b-criteria-list-shown`) and point to `docs/sprints/sprint_04/S4-18a_notices_actionbar_titles.md` for the decisions.

```text
Task S4-18 from docs/sprints/sprint_04/SPRINT_04.md — S11 notices, action bar, suggestion link, event and page titles
Issues: none
Requirements / invariants: R-LST-09, R-LST-10, R-LST-13, R-MET-01 (CriteriaListShown); A11Y-09 stays green
Depends on: S4-14 – S4-17 merged

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-18 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-09, R-LST-10, R-LST-13, §9.1; Design handoff §5.1 (S11 row: notices, sticky action bar); ux_copy v17 §5.5, §5.16; docs/sprints/sprint_03/S3-23_usage_events_fire_and_forget.md (P1); docs/sprints/sprint_03/S3-21_criteria_suggestion_link.md

Owner decisions for this task: O-S4-4: <copy from S4-15>
Additional instructions: The summary of the selection above the list (`criteria.list.summary.*` as a description list, `criteria.list.summary.count`, `criteria.list.counts`; moved here from S4-17 by O-S4-47, see docs/sprints/sprint_04/S4-17_search_filter_tested_empty.md); notices: priority note always, draft note for anonymous users; action bar „Liste speichern“ / „Liste exportieren“, sticky from 600 px width and height and in the flow below (what „Liste speichern“ does for an anonymous user follows the UX copy — ask if it is unclear); „Kriterium vorschlagen“ at the end of the list with the configured channel; the first CriteriaListShown caller (once per shown list); page-title variants of UX copy v17 and shell.pageTitle.error; S11 into reflow.spec.ts. „Liste exportieren“ opens a placeholder until S4-21.

Way of working:
1. Branch sprint04/S4-18-notices-actionbar-titles from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-18_notices_actionbar_titles.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-19 — Export pipeline and POST /api/v1/exports

```text
Task S4-19 from docs/sprints/sprint_04/SPRINT_04.md — Export pipeline and POST /api/v1/exports
Issues: none
Requirements / invariants: R-LST-12; EXP-05, EXP-07, TEN-04 and the export part of CAT-12 → active; ARC-03 (AdHocSelectionSource part)
Depends on: S4-08 merged; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-19 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-12, §8.3, §7.3; ARCHITECTURE §4.3 (Export module, IExportSource), §4.4 Flow A, §9.4; TESTING §4.9 (EXP-*), §4.6, §4.10 (ARC-03 deferred part), §4.5 (TEN-04)

Owner decisions for this task: none
Additional instructions: The export pipeline in the Export module with IExportSource and AdHocSelectionSource (depends on Catalog.Contracts only — write the deferred ARC-03 rule). POST /api/v1/exports re-resolves priorities on the server and rejects client-supplied levels and revisions outside the stated release (EXP-07); the anonymous export persists nothing — no rows except analytics, no files, no request bodies in the logs (EXP-05, with the persists-nothing helper of TESTING §9 step 2); Cache-Control and ETag on the catalog endpoints, no-store on export responses (TEN-04). Renderers come in S4-20: return a minimal file or a stub renderer behind the interface for now.

Way of working:
1. Branch sprint04/S4-19-export-pipeline from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-19_export_pipeline.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-20 — XLSX and CSV renderers

```text
Task S4-20 from docs/sprints/sprint_04/SPRINT_04.md — XLSX and CSV renderers
Issues: none
Requirements / invariants: EXP-01 – EXP-04 → active
Depends on: S4-19 merged; O-S4-6 for the short-description column; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-20 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC §8.3 (columns, order, file names); ARCHITECTURE D12 (ClosedXML, never EPPlus); TESTING §4.9; TECH_DEBT_v3 TD-30

Owner decisions for this task: O-S4-6: <short description: field in CriterionRevision (column written empty until filled) | derived from existing text | column omitted for now>
Additional instructions: ClosedXML for XLSX with a real header row, defined names for the data range, no meaning by colour alone, sheet and column titles in German; CSV in UTF-8 with BOM and a stable column order; the catalog release version in both (EXP-03). Tests re-open the XLSX with ClosedXML and compare it with the selection (EXP-01). The short-description column per O-S4-6.

Way of working:
1. Branch sprint04/S4-20-xlsx-csv-renderers from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-20_xlsx_csv_renderers.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-21 — Export dialog S13, rate limit and /health/ready

```text
Task S4-21 from docs/sprints/sprint_04/SPRINT_04.md — Export dialog S13, rate limit and /health/ready
Issues: #112
Requirements / invariants: R-LST-11; API-05 (anonymous export) → active; R-MET-01 (ExportCreated)
Depends on: S4-19, S4-20 and S4-18 merged; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-21 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 112) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: SPEC R-LST-11, §7.3 (rate limits, 🔧 values), §9.1; Design handoff §5.1 (S13 row: initial focus, notices); ux_copy v17 §5.6 (criteria.export.*); the Figma frames S13, S13-E, S13-M; TECH_DEBT_v3 §3.2 (#112)

Owner decisions for this task: none
Additional instructions: S13 as MatDialog (600 px; full-screen below 600 px per S13-M): file format XLSX (default) or CSV, scope text, release and anonymous notes, the ⚖️ legal note as written, the error state of S13-E; the file download. The anonymous-export rate limit per SPEC §7.3 (configurable, a relaxed value in the E2E environment) answering 429 with Problem Details; put /health/ready behind the rate-limit mechanism and make its timeout configurable (#112). Put `POST /api/v1/configurator/criteria-list-shown` behind the same mechanism with its own SPEC §7.3 row (client IP, 30 per 10 minutes, configurable; owner decision O-S4-56 (b), S4-18b); the client already ignores its 429. Write ExportCreated with format and signed-in dimension.

Way of working:
1. Branch sprint04/S4-21-export-dialog-rate-limit from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-21_export_dialog_rate_limit.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-22 — J1 complete to the export; M2 exit check; bundle budget

```text
Task S4-22 from docs/sprints/sprint_04/SPRINT_04.md — J1 complete to the export; M2 exit check; bundle budget
Issues: #42
Requirements / invariants: A11Y-02 (J1 part), A11Y-01, A11Y-08, A11Y-09 stay green
Depends on: S4-13, S4-18, S4-21 merged

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-22 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 42) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ROADMAP M2 exit criteria; TESTING §5.4, §7.2 (manual screen-reader check of the exported file — the tester's part); angular.json budgets; TECH_DEBT_v3 §3.3 (#42)

Owner decisions for this task: none
Additional instructions: Extend J1: select on S10 → list on S11 (override, exclude, note) → export on S13 with keyboard only and axe in both appearances; check the downloaded file's name and first row. Tighten the bundle budget to what the rebuilt client needs plus a margin (#42). List the M2 exit criteria in the protocol with their state, and name the manual screen-reader check of an exported XLSX as the tester's open item.

Way of working:
1. Branch sprint04/S4-22-j1-export-m2-exit from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-22_j1_export_m2_exit.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-23 — Accept-Language on the catalog list endpoints

```text
Task S4-23 from docs/sprints/sprint_04/SPRINT_04.md — Accept-Language on the catalog list endpoints
Issues: none
Requirements / invariants: API-06 → active
Depends on: nothing; one TESTING version at a time

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-23 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TESTING §4.6 (API-06); SPEC §8.1, §8.2; the catalog endpoints of S3-19 and S3-20b

Owner decisions for this task: none
Additional instructions: The catalog list endpoints honour Accept-Language (de, en); while a release has no English texts, German is the fallback and the response says which language it served (Content-Language). Contract tests for de, en and an unknown language.

Way of working:
1. Branch sprint04/S4-23-accept-language from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-23_accept_language.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-29 — Figma light colours pinned in the theme

```text
Task S4-29 from docs/sprints/sprint_04/SPRINT_04.md — Figma light colours pinned in the theme
Issues: #240 (light part only; the dark part stays open)
Requirements / invariants: R-SET-03 (development part), SPEC §7.5; A11Y-01, A11Y-08 stay green
Depends on: S4-13 (S10 is already merged); merge before S4-14 so S11 is built in the final light colours

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-29 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issue named above (gh issue view 240) and its entry in TECH_DEBT_v3.md (TD-34); always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: Design handoff §1.1, §1.2 (colour tokens: Figma variable, CSS role, value), §1.4 if present (focus ring); SPEC §7.5; CLIENT src/styles/theme.scss; docs/sprints/sprint_03/S3-09_theme_foundation.md (P1, P6) and S3-10_dark_appearance_axe.md (P4)

Owner decisions for this task: O-S4-22: pin the light values of Handoff §1.2 (the Figma variables) now with mat.theme-overrides; the dark appearance stays on the generated palette until the Figma dark mode and its accessibility review (O-S5) (decided 2026-11-11)
Additional instructions: For every --mat-sys-* role listed in Handoff §1.2, set the light appearance to its value there, via mat.theme-overrides in theme.scss (the mechanism SPEC §7.5 names) — never as hex values in components. Roles the Handoff does not list keep the generated value; list them in the protocol. Show the result as a table role | generated value | Figma value | contrast against its background, and with a test or script that reads the computed custom properties in the light appearance. If a Figma value breaks a contrast rule of SPEC §7.5 or fails axe, do not change it: report it as an owner decision. The dark appearance must not change (A11Y-08 green, same values as before). Do not close #240; comment which part is done. Update Handoff §1.2 only if a value there is ambiguous, and the TD-34 note in the protocol.

Way of working:
1. Branch sprint04/S4-29-figma-light-colours from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-29_figma_light_colours.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Refs #240" (the issue stays open for the dark part). Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

## P2 — stretch

### S4-24 — Short description in the catalog model *(stretch)*

```text
Task S4-24 from docs/sprints/sprint_04/SPRINT_04.md — Short description in the catalog model
Issues: #237
Requirements / invariants: CAT-23 validation covers the new field
Depends on: O-S4-6; stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-24 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 237) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v3 TD-30; Kriterienkatalog §6.1; SPEC §8.3/§8.4 (export column); ARCHITECTURE §5.1

Owner decisions for this task: O-S4-6: <copy from S4-20>
Additional instructions: Stretch task. The field per O-S4-6 in CriterionRevision, the release format, reader, validator and migration; an ADR in the next ARCHITECTURE version; the export column of S4-20 reads it. 2026.1 gets the field empty; list it in kriterienkatalog_2026_1_luecken_v1.md only if the owner agrees (owner document).

Way of working:
1. Branch sprint04/S4-24-short-description from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-24_short_description.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-25 — M3.4 groundwork: contributor profiles and the Moderator role *(stretch)*

```text
Task S4-25 from docs/sprints/sprint_04/SPRINT_04.md — M3.4 groundwork: contributor profiles and the Moderator role
Issues: none
Requirements / invariants: prepares TEN-* and PUB-* (step 3/6)
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-25 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: ROADMAP M3.4; ARCHITECTURE §12 step 3, §5.2 (ContributorProfile, UserContributorLink), roles; TESTING §9 step 3

Owner decisions for this task: none
Additional instructions: Stretch task. Entities ContributorProfile and UserContributorLink in the IdentityAccess module with a migration (no endpoints yet); rename the Approver role to Moderator (data migration of existing role rows; the legacy client's checks follow); a CLI command to grant and revoke platform roles. Ask before anything touches the JWT or the legacy account pages.

Way of working:
1. Branch sprint04/S4-25-m3-groundwork from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-25_m3_groundwork.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-26 — Encrypt the Data Protection key ring and document it *(stretch)*

```text
Task S4-26 from docs/sprints/sprint_04/SPRINT_04.md — Encrypt the Data Protection key ring and document it
Issues: #235
Requirements / invariants: OPS-04 stays planned (step 3)
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-26 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 235) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v3 TD-27; docs/sprints/sprint_03/S3-24_data_protection_keys.md (P2); ARCHITECTURE C60

Owner decisions for this task: none
Additional instructions: Stretch task. Encrypt the key ring at rest with a key from the environment (no secret in the repository), and document key store, backup and restore as a section that TECHNICAL.md (#4) can take over. Ask before choosing the protection mechanism if more than one fits.

Way of working:
1. Branch sprint04/S4-26-key-ring-encryption from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-26_key_ring_encryption.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-27 — Licence notices of the NuGet packages in the API image *(stretch)*

```text
Task S4-27 from docs/sprints/sprint_04/SPRINT_04.md — Licence notices of the NuGet packages in the API image
Issues: #187
Requirements / invariants: CLAUDE §10
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-27 plus the sections Dependencies, Owner decisions, Risks and Definition of done; the issues named above (gh issue view 187) and their entries in TECH_DEBT_v3.md; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v3 TD-22; docs/sprints/sprint_02/S2-03_self_hosted_icons.md (P4); the API Dockerfile

Owner decisions for this task: none
Additional instructions: Stretch task. Check which licences of the shipped NuGet packages require a notice, and ship them with the API image (e.g. a generated third-party notices file) the way the client image serves 3rdpartylicenses.txt. Licence check of any tool used (my OK).

Way of working:
1. Branch sprint04/S4-27-nuget-licence-notices from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-27_nuget_licence_notices.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

### S4-28 — Dead code: dialog service, unused logger, assets location *(stretch)*

```text
Task S4-28 from docs/sprints/sprint_04/SPRINT_04.md — Dead code: dialog service, unused logger, assets location
Issues: none
Requirements / invariants: none
Depends on: stretch: only after all P0 tasks

Read first: CLAUDE.md; in SPRINT_04.md the row of S4-28 plus the sections Dependencies, Owner decisions, Risks and Definition of done; always the highest version of each document in docs/ (document names below carry no version on purpose).
Also read: TECH_DEBT_v3 TD-32

Owner decisions for this task: none
Additional instructions: Stretch task. Remove core/services/dialog.service.ts (fully commented out), the unused Logger in BaseCrudController, and the location ^~ /assets/ in nginx.conf after checking nothing references them.

Way of working:
1. Branch sprint04/S4-28-dead-code from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:CFG-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_04/S4-28_dead_code.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Appendix — generic template (for later sprints)

Replace `<S>` with the sprint number (e.g. 5), `<SS>` with the two-digit sprint folder number (e.g. 05) and `<v>` with the current tech-debt register version. Name documents without version numbers.

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
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. If the plan shows more than one session, propose a split (as S3-20a/b). Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code: a test for an invariant carries INV:<invariant ID> in its name (e.g. INV:API-01) and the Invariant trait (TESTING §8.1). Flip invariants to active in a new TESTING version and record deferred parts there; if another PR merged the same TESTING version first, rebase and renumber yours. UI strings only by key. Every new dependency or project needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, node scripts/check-i18n.mjs, reuse lint. If another session runs the E2E stack locally at the same time, leave E2E to CI. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_<SS>/S<S>-<NN>_<topic>.md in the structure of the earlier sprints (header with sprint, task, issues, requirements / invariants, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping, LEGACY_EXCEPTIONS …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

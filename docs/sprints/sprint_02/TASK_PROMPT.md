# Sprint 02 — task prompt for Claude Code

> **Status:** v1 (2026-10-29). Companion to `SPRINT_02.md`. Paste the template below into a new Claude Code session for every task of sprint 02 and fill in the `<…>` placeholders. Claude Code implements; the owner's control chat for sprint 02 takes the decisions and reviews the reports.

---

## Template

```text
Task S2-<NN> from docs/sprints/sprint_02/SPRINT_02.md — <title of the row>

Read first: CLAUDE.md; in SPRINT_02.md the row of S2-<NN> plus the sections Dependencies, Owner decisions, Risks and Definition of done; the documents and sections the row names (always the highest version in docs/); the issues named in the row (gh issue view <n>) and their entries in TECH_DEBT_v1.md; sprint-01 protocols in docs/sprints/sprint_01/ only where the row or an issue points to them.

Owner decisions for this task: <none | O-S2-n: <decision> (decided <date>)>
Additional instructions: <none | …>

Way of working:
1. Branch sprint02/S2-<NN>-<topic> from an up-to-date main. Scope is exactly this row. Anything else you find goes into the protocol's open points (or a proposed issue), not into this PR.
2. Before changing code, give me a short plan: starting point, files, invariants the task activates, tests. Stop and ask if the row, the documents and the code contradict each other, if a UX copy key or its wording is missing, or if the task needs an owner decision that is not given above (CLAUDE §2, §6.5, §6.6). Do not decide for me: give options with a recommendation.
3. Tests come with the code (INV:<ID> in the name plus the Invariant trait). Flip invariants to active in a new TESTING version, as in sprint 01. UI strings only by key. Every new dependency needs a licence check (CLAUDE §10) and my OK. SPDX headers on new files.
4. Before pushing: the checklist of CLAUDE §6.7 (use /engineering:code-review if available), build, every test level the change touches, node scripts/check-invariants.mjs, node scripts/check-figma-links.mjs, reuse lint. Sort review findings per CLAUDE §6.8.
5. Write the protocol docs/sprints/sprint_02/S2-<NN>_<topic>.md in the structure of sprint 01 (header with sprint, task, status, scope, owner decisions; 1 Starting point, 2 Changes, 3 Self-review, 4 Checks, 5 Open points). Update every document the change touches (CLAUDE.md §3/§4, SPEC §13, figma mapping …).
6. Open a pull request against main: task ID, R-/INV- IDs, summary, checks, open points, "Closes #n" for finished issues. Do not merge.

Finish with a short report for my control chat: PR link, protocol path, owner decisions needed, deviations from the plan, open points, and whether the task fitted one session.
```

---

## Additions per task

| Task | Fill in under "Owner decisions" or "Additional instructions" |
|---|---|
| S2-03 | O-S2-5 (icon set replacing Font Awesome) |
| S2-05 | "Spike only, no production change. Report the result and stop." |
| S2-06 | Without O-S2-1: "Write only the design note and stop." Then a second run with O-S2-1 |
| S2-08 | Result of S2-05, or O-S2-2 if the spike failed |
| S2-12 | First run: "Options only, then stop." Second run with O-S2-3 |
| S2-13 | O-S2-3, and the owner's choice for the legacy `en.json` / `es.json` / `fr.json` (keep untouched or remove until the English UI texts come) |
| S2-15 | Wording for #120 (O-S2-4); S2-14 must be merged first |

## Rules for running tasks

- **One new TESTING version at a time.** S2-08, S2-10, S2-11 and S2-16 flip invariants to `active`. If two of them run on parallel branches, both would create the same `TESTING_v16.md`. Start such a task only after the previous one is merged.
- **Can start at once:** S2-01, S2-02, S2-04, S2-05. The others wait for their dependency or owner decision (`SPRINT_02.md`, Dependencies).
- **After each task:** the report and the protocol go to the control chat, which checks them against the row and the Definition of done, settles open decisions and names the additions for the next task.
- **End of sprint:** close-out table in `SPRINT_02.md` and `TECH_DEBT_v2.md` (process step 14), prepared in the control chat.

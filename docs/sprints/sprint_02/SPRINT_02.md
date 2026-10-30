# Sprint 02 — Foundations I: modules, Problem Details, texts and shell

> **Status:** plan (2026-10-29) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v1.md` (milestone M1), `TECH_DEBT_v1.md` (§9.1 sprint 02, §9.2 M1), `TESTING_v15.md` (§4, §9 step 1), `ARCHITECTURE_v14.md` (§4.3, §12 step 1), `SPEC_v2.md` (§11 step 1, R-SHL-*), `CLAUDE.md`, the sprint-01 protocols `docs/sprints/sprint_01/S1-*.md`, and the GitHub issues and pull requests as of 2026-10-29 (38 open issues, all in `TECH_DEBT_v1.md`; Dependabot PRs #129–#131 open).
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.

---

## Sprint 01 close-out (carried in)

The close-out table of `SPRINT_01.md` was not filled in; these are the values this plan works with.

| Item | Value |
|---|---|
| Points done (P0 / stretch) | 28 / 4 — all of S1-01 – S1-23. **Plus, unplanned:** M1.8 (CI browser matrix, 2 points) came with S1-18/S1-20 (all five Playwright projects run in CI); the end-of-sprint review delivered #116, #98, #123, #124 (≈ 3 debt points: dependency PR #126, documentation PR #127, random key PR #128) and the tech-debt register |
| Velocity for forecasting | **≈ 34 roadmap points** (+ ≈ 3 debt points) in one sprint. The number of development days actually used was not recorded → the assumption of ~35 points per sprint stays; this sprint is the second data point |
| Carried over to sprint 02 | Nothing from the backlog. From the register: the live bugs, the icon fonts and the NetArchTest spike (§9.1), which were planned for sprint 02 from the start |
| Issues created | #52 (axe suppression), #82–#124 (review follow-ups and register items); nine pre-process issues closed; milestones M1 … G-R created |
| Changes needed in roadmap or documents | M0 done; M1.2, M1.7, M1.8 done ahead. M1 is larger than its 33 points once the debt that rides with it (TECH_DEBT §9.2: AutoMapper #31, build warnings #118, gate #109/#111, lint #119, Dockerfile #77 …) is counted: **≈ 27 remaining roadmap points + ≈ 11 debt points**. That is more than the 20 % of ROADMAP §6 → `ROADMAP_v2.md` should record it (M1 spans sprints 02 and 03) |

---

## Sprint goal

**The application runs inside the new application shell — landmarks, header, footer, start page, error pages, page titles with focus and announcement, all texts from `de.json` — on top of enforced module boundaries and Problem Details, and the live defects of the register are fixed.**

This is the larger part of roadmap milestone **M1** (M1.1, M1.3, M1.4, M1.6) plus the sprint-02 debt budget. **Not in this sprint:** M1.5 (theme foundation, dark appearance, A11Y-08) and the debt that is bound to M1 but not to these packages (#31 AutoMapper, #118, #77, …). They form sprint 03 together with the start of M2; M1's exit criterion "shell passes axe in both appearances" is therefore reached in sprint 03, not here.

### Success criteria

- [ ] #37, #95 (+ #96), #113, #121 are closed; no icon font or other asset is loaded from a third-party origin, and no Font Awesome asset is in the bundle (#43, #78).
- [ ] Every module of ARCHITECTURE §4.3 exists as `ProcureAccess.Modules.<Name>` with its `.Contracts` project and `Add<Name>Module` registration; the platform code built ahead in sprint 01 (logging, health, usage events) lives in Platform.
- [ ] **ARC-01, ARC-02, ARC-03, API-01, API-02, A11Y-03, A11Y-05** are `active` with tagged, passing tests; the gate also enforces the .NET `Invariant` trait (#109, #111).
- [ ] The new shell (R-SHL-01, -04, -05, -06, -08, -09, -10) replaces the legacy layout; the legacy feature screens still work inside it; J1 and CAT-10 stay green; A11Y-01 passes on every shell page; the legacy header is off the `PENDING` list of the Figma link check.
- [ ] UX-O9 is decided and `de.json` is built from `ux_copy_procureaccess_v14.md` (or its successor); no hard-coded German text in new code.

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | ~35 points | Documentation, WERTE.IT coordination, reviews and the owner decisions below are outside these days |
| **Total** | **8 days** | **~35 points** | |

**Basis:** sprint 01 finished its 32 planned points plus ≈ 5 unplanned ones, so ~35 is not too high. Because the days actually used are unknown, the 80 % rule stays.

**Planned load:** 28.5 points committed (**81 %** of the assumed capacity), of which 6.5 points are debt (**19 %** of capacity: S2-01 – S2-05, S2-09, S2-14; inside the 15–20 % budget of TECH_DEBT §4) + 3.5 points stretch.

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimate, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request. Order = recommended working order.

### P0 — committed (28.5 points)

#### Debt first — 4.5 points (live defects and the spike that gates M1.1)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S2-01 | Live bugs: #37 (missing `await` in `ProductsController.ByCriteriaFilterIds`, plus the spinner that stays, S1-08 N1), #95 with the missing spec cases #96 (cancel from the start, near-miss e-mails), #113 (dev health watcher points at the local API) | 1 | Each bug has a regression test; issues closed |
| S2-02 | #121 `ApplicationName` is always `Unknown` (`AppName` vs `ApplicationName`) | 0.5 | Log rows carry the application name; covered by an existing or new logging test |
| S2-03 | Self-host the icon fonts and settle Font Awesome in one PR (#43, #78): Material icon font served from the client bundle; every Font Awesome usage replaced by an icon with an OSI licence that meets D12 (proposal: Material Symbols, Apache-2.0 — **owner decision O-S2-5**); `LICENSES/` and `REUSE.toml` updated | 2 | No request to a third-party origin in the network log of J1; no Font Awesome package left; `reuse lint` green |
| S2-04 | Dependabot PRs #129–#131: licence check of every new version (CLAUDE §10), changelog check for the major bump `marked` 17 → 18, merge | 0.5 | PRs merged or closed with a reason |
| S2-05 | NetArchTest spike (#117): one real rule against the .NET 10 assemblies | 0.5 | Result in the PR. If the rule works: S2-08 uses NetArchTest. If not: **stop**, owner decides (O-S2-2) before S2-08 |

#### M1.1 Module projects — 8 points (+ 1 point gate debt)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S2-06 | Design note + skeleton: a short note in the PR that maps today's projects (`API`, `DAL`, `MODELS`, `SERVICES`) onto the target layout and states what moves now and what stays legacy until its step (**owner decision O-S2-1** before any code moves); then `src/ProcureAccess.Modules.<Name>` and `.Contracts` for Catalog, Configurator, Workspace, Directory, Moderation, Export, IdentityAccess and Platform, project references exactly per ARCHITECTURE §4.3, `Add<Name>Module` extensions called from `Program.cs`, SPDX headers | 3 | Solution builds; `CLAUDE.md` §4 shows the new layout; the app behaves as before (smoke test) |
| S2-07 | Move the platform code built ahead into Platform: Serilog sink set-up and `LogRetentionJob` (S1-22), health checks, `IUsageEventWriter` and its entity (S1-23 P6), `TimeProvider` registration; only controllers and the registration extension stay public | 2 | OPS-01, OPS-02, PRV-05, SEC-06 still green; no new public type outside `Contracts` |
| S2-08 | **ARC-01, ARC-02, ARC-03** as tagged architecture tests (one rule per row of §4.3; project-reference check plus NetArchTest, or the tool decided in O-S2-2); confirm the architecture test project still runs without `--ignore-exit-code 8` (already removed in S1-23 with PRV-05, S2-05 P2; the TECH_DEBT §5 item is dropped at the sprint close-out) | 3 | The three rules fail on a deliberate wrong reference and pass without it |
| S2-09 | Gate tightening #109 (fail on catalogue header drift, count only test files, require a protocol file) and #111 (count only tests that run, require the `Invariant` trait) | 1 | Gate fails on each of the new cases in a scratch branch, passes on `main` |

#### M1.3 Problem Details — 3 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S2-10 | Problem Details middleware in Platform: RFC 9457 with field-keyed `errors`, problem types with configurable base URI (SPEC §8.6, ARCH C78); the ad-hoc error objects in `BaseCrudController` and `UICustomizationController` go (TECH_DEBT §5); **API-01** as a generic test over all write endpoints | 2 | API-01 active and green |
| S2-11 | Client mapping: problem type → UX key; server field errors bound to the matching form control with `aria-describedby` and announced (**API-02**); one existing form as the example | 1 | API-02 active; component test per CLAUDE §9 convention |

#### M1.6 Texts — 3 points (before the shell, because the shell uses only keys)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S2-12 | UX-O9 pluralisation approach: present the options with licence and bundle size (e.g. ICU MessageFormat compiler for ngx-translate vs. `_one`/`_other` keys with `Intl.PluralRules`) → **owner decision O-S2-3**; set up the chosen approach with one example | 1 | Decision recorded in SPEC §13 / next SPEC version; example key renders singular and plural |
| S2-13 | `de.json` built from the UX copy with all keys of §5 (shell, errors, common) — script or checked conversion, with a check that every key in templates exists; key conflict #101 (`shell.nav.menu` vs `shell.nav.menu.close`) resolved together with the UX copy; legacy `en/es/fr.json` handled per the owner's choice (keep untouched or remove until English UI texts come) | 2 | No missing-key warnings on the shell pages; #101 closed |

#### M1.4 Application shell — 8 points (+ 1 point lint debt)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S2-14 | #119 TypeScript lint rules (`typescript-eslint` recommended) **before** the new client code; legacy findings either fixed if trivial or disabled per file with an issue link, so new code starts clean | 1 | ESLint stage 1 runs the TS rules; no rule disabled for new paths |
| S2-15 | Shell frame: landmarks and skip link (R-SHL-01), `pa-header` in the **Anonymous** state with the compact layout below 600 px (R-SHL-02 anonymous part; SignedIn states come with M3), footer with `shell.footer.*` links (R-SHL-06; legal routes as placeholders until GP.3), #120 English accessible names and `COMMON.ENTER` (needs wording, **O-S2-4**); `@figma` tags and the legacy header removed from the `PENDING` list (S1-21 P4) | 3 | A11Y-01 green on the shell; Figma link check green with the new components |
| S2-16 | Route change: page title from `pageTitle.*` most-specific-first, focus on the H1, announcement via `A11yAnnouncerService` (R-SHL-04, **A11Y-03**); document `lang` follows the UI language (**A11Y-05**) | 2 | A11Y-03 and A11Y-05 active (Vitest + E2E) |
| S2-17 | Start page S04 (R-SHL-05), error pages S80 for 404/403/500 and the offline notice (R-SHL-08), loading/empty/system states (R-SHL-09) | 2 | Component tests by role and name; axe green on each page |
| S2-18 | 320 px / 400 % zoom check of every shell page (R-SHL-10, TESTING §7.2 short protocol); J1 and CAT-10 E2E adjusted to the new shell (locators only, expected sets unchanged) | 1 | Protocol in the PR; J1 green on all five browser projects |

### P2 — stretch (3.5 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S2-19 | #122 E2E readiness on `/health/ready`, remove `/api/Health` (and the `ClockTests` use of it) | 0.5 | TECH_DEBT §9.2 (shell) |
| S2-20 | CI one-liners #104, #105, #106, #107 | 0.5 | TECH_DEBT §9.2 (any CI PR) |
| S2-21 | API Dockerfile hardening #77 (Release build, non-root user, restore caching), #74, #73 | 1 | TECH_DEBT §9.2 (M1.2/ops) |
| S2-22 | Figma link check #110 (map pages, find moved components, validate `PENDING` files) | 1 | TECH_DEBT §9.2 (shell) |
| S2-23 | Client coverage exclusions #97 | 0.5 | TECH_DEBT §9.2 (first new client code) |

Stretch items are pulled in order; if one is started but not finished, it goes back to the M1 backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | S2-01 – S2-05; S2-06 design note; S2-12 options | Live bugs closed; spike result known; **owner decisions O-S2-1 – O-S2-5 taken** |
| First half | D1–D4 | S2-06 – S2-11 (backend), S2-13 | **Mid-sprint check (end of D4):** module boundaries enforced and Problem Details live (≈ 17 points done). If not: drop the stretch and move S2-17 to sprint 03 |
| Second half | D5–D8 | S2-14 – S2-18 | Shell live, step-1 invariants of this sprint active |
| Buffer | remaining | stretch S2-19 – S2-23 | – |

Backend (S2-06 – S2-11) and frontend (S2-12 – S2-18) are independent once S2-05 and the decisions are done; if one is blocked, switch to the other instead of waiting.

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| S2-08 | S2-05 spike; S2-06 skeleton | internal; if the spike fails, O-S2-2 |
| S2-06, S2-07 | **O-S2-1** scope of the module move | Philipp, D1 |
| S2-10 | S2-07 (middleware lives in Platform) | internal |
| S2-13 | **O-S2-3** UX-O9 | Philipp, D1–D2 |
| S2-15 – S2-17 | S2-13 (`de.json`); S2-14 (lint rules) | internal |
| S2-15 | **O-S2-4** wording for #120 in the UX copy | Philipp (UX copy) before D5 |
| S2-03 | **O-S2-5** icon set replacing Font Awesome | Philipp, D1 |
| — | No dependency on the project team or WERTE.IT in this sprint | – |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal |
|---|---|---|---|
| O-S2-1 | Scope of M1.1: are only the module and contracts projects created and the platform code moved, while `API`/`DAL`/`MODELS`/`SERVICES` stay legacy until the step that replaces them — or does existing code move into modules now? | S2-06 | Skeleton + Platform only; legacy code moves with its step (CLAUDE §6.2: no rework on structures a later step replaces) |
| O-S2-2 | Only if the spike fails: fork NetArchTest, switch to another OSI-licensed tool, or project-reference rules only | S2-08 | Decide with an ADR (CLAUDE §11) |
| O-S2-3 | UX-O9 pluralisation approach | S2-13 | Options with licence and size in S2-12 |
| O-S2-4 | German wording for the header icon buttons and `COMMON.ENTER` (#120) | S2-15 | Next UX copy version |
| O-S2-5 | Icon set replacing Font Awesome | S2-03 | Material Symbols (Apache-2.0), self-hosted |
| — | Severity (`blocker`/`major`/`minor`) of the a11y issues #52, #102, #120 (TECH_DEBT §6: "sprint 02 planning") | sprint start | — |
| — | For sprint 03: replacement for AutoMapper (#31) and what replaces ARC-06 in TESTING | before sprint 03 | Prepare the ADR during this sprint |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | M1.1 scope is not written down in ARCHITECTURE (how legacy projects relate to modules) | S2-06 grows or moves code twice | Design note and O-S2-1 on D1 before any code moves |
| R2 | NetArchTest.Rules (no release since 2021) does not work on .NET 10 | S2-08 blocked | S2-05 on D1; project-reference rules of ARC-03 work without it |
| R3 | The new shell breaks legacy screens, J1 or the CAT-10 E2E part | S2-15 – S2-18 grow | Legacy screens only need to keep working inside the shell; adjust locators, never expected sets; screens are rebuilt from M2 on |
| R4 | UX copy lacks keys or wording the shell needs | Shell tasks stop (CLAUDE §6.5) | Collect all gaps in one list and settle them in one UX copy session instead of one by one |
| R5 | `typescript-eslint` recommended reports many findings in legacy code | S2-14 grows | Fix trivial ones; disable per file with an issue link for legacy code that M1–M3 replaces |
| R6 | Ride-along debt makes M1 larger than the roadmap says | M1 needs sprint 03 too | Accepted in this plan; record in `ROADMAP_v2.md` |
| R7 | Velocity basis is thin (days of sprint 01 not recorded) | Over- or under-commitment | 80 % rule plus stretch; record the days used at this close-out |
| R8 | Dependabot major bump `marked` 17 → 18 | Breaks Markdown rendering | Changelog check in S2-04; `marked` matters for the legal pages (GP.3), so a revert is cheap now |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, as in sprint 01.
- [ ] UI strings only by key; new components carry `@figma` tags and pass the Figma link check.
- [ ] New files carry SPDX headers; no secrets committed; licences of new dependencies checked (CLAUDE §10).
- [ ] Documentation touched by the change updated (`CLAUDE.md` §3/§4 for the new layout, TESTING, SPEC §13 for UX-O9); register items closed by the PR named in it.
- [ ] Protocol `docs/sprints/sprint_02/S2-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in sprint 01.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; spike, design note, owner decisions O-S2-1 – O-S2-5, a11y severities |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: success criteria checked, smoke test on the compose stack |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v2.md` |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 03 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** finish M1 — M1.5 theme foundation with A11Y-08 (5), AutoMapper replacement #31 with #90 and the ARC-06 successor, build warnings and analyzer check #118, remaining stretch items — then start M2 with M2.1 (catalog domain model, 13).

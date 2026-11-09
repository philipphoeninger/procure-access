# Sprint 03 — Finish the foundations, build the catalog

> **Status:** plan (2026-11-05) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*. **Amended 2026-11-06** after owner decision O-S3-5 (S3-11, option C): S3-18 reduced to seed rows 2 and 3 (3 → 1 point), S3-27 moved from stretch to P0 (M2.3).
> **Based on:** `ROADMAP_v3.md` (M1 rest, M2.1, M2.2, M2.4, M2.8), `TECH_DEBT_v2.md` (§3, §6, §9.1, §9.2), `TESTING_v23.md` (§4.2, §4.11, §9 step 2), `ARCHITECTURE_v15.md` (§4.3, §5.1, §12 step 2, ADR-014), `SPEC_v2.md` (§7.5, §8.1, §8.2, R-ADM-01, R-SET-02/03, R-SUG-01), `CLAUDE.md`, the sprint-02 close-out and protocols in `docs/sprints/sprint_02/`, and the GitHub issues and pull requests as of 2026-11-05 (35 open issues, all in `TECH_DEBT_v2.md`; Dependabot PRs #164, #165, #166, #175 open).
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.
> **Task prompts:** `TASK_PROMPTS.md` in this folder (one ready-to-paste prompt per task).

---

## Sprint 02 in one paragraph (carried in)

All 23 tasks (28.5 P0 + 3.5 stretch points) were done in about **4 development days** (close-out in `SPRINT_02.md`). Nothing is carried over. M1 is done except the theme (M1.5) and two debt items (AutoMapper #31, build warnings #118), which were planned for this sprint from the start. The register review (`TECH_DEBT_v2.md`) adds a debt budget of ≈ 7.5 points and nine new issues (#180 – #188).

---

## Sprint goal

**M1 is finished — both appearances, no AutoMapper, no build warnings — and the catalog of M2 stands on its own: a versioned model, release import by CLI and the two catalog endpoints, while the existing criteria filter keeps working.**

This sprint builds **no new page** of the target design. Its visible UI work is the dark appearance and the focus indicators on the existing shell pages, three legacy-client fixes and one footer link. The configurator S10 and the criteria list S11 follow in sprint 04 on top of the endpoints built here.

### Success criteria

- [ ] **M1 done:** theme foundation per SPEC §7.5 with the generated dark palette, strong focus indicators (#184); **A11Y-08** `active` (development part; the persisted setting comes with S60 in step 3). AutoMapper is gone, its advisory suppression is out of `Directory.Build.props`, the ARC-06 successor is `active` (#31, #90). A Release build has no warnings in any project, the analyzer check covers all projects, `src/` treats warnings as errors (#118).
- [ ] **Debt of TECH_DEBT_v2 §9.1 closed:** #144 (CodeQL checks required again), #168, #171, #137, #180, #182, #185, #183; the open Dependabot PRs reviewed.
- [ ] **Catalog module (M2.1, M2.2):** own `DbContext` and migration history (ADR-014) with the entities of ARCHITECTURE §5.1; `catalog import` / `catalog activate` work on a release folder; the existing criteria stay on the legacy tables until S10 replaces the legacy filter in sprint 04 (O-S3-5 C) and **CAT-10 stays green**; the BIT inklusiv texts of release 2026.1 — which contain the existing criteria by their EN 301 549 numbers — are imported as a release folder, not activated until O-1 arrives (O-S3-5, Q2 (iii)).
- [ ] **Catalog endpoints (M2.4):** `GET /api/v1/catalog/filter-options` and `/criteria` per SPEC §8.1 / §8.2 under versioned routes (#153); the step-2 invariants of the catalog that need no configurator, export or workspace are `active` (proposal per task below; the deferred parts are recorded in TESTING, as for ARC-03 and API-02 in sprint 02).
- [ ] „Kriterium vorschlagen“ in the footer opens the configured channel (R-SUG-01 footer part).

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | **~55 points** | Documentation, WERTE.IT coordination, reviews, the owner decisions below and the UX copy session #181 are outside these days |
| **Total** | **8 days** | **~55 points** | |

**Basis:** sprint 02 delivered 32 points in about 4 development days (≈ 8 per day). This sprint assumes ≈ 7 per day: it is the first measurement, and M2 brings a new domain model whose tasks are larger and less familiar than the debt and shell tasks of sprint 02.

**Planned load:** **42.5 points committed (77 %** of the assumed capacity; 41.5 before O-S3-5), of which 9.5 points are debt (S3-01 – S3-08, #153 in S3-19; 17 %), + **5.5 points stretch**. The buffer is larger than the usual 80 % because the velocity rests on one data point.

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimates of `ROADMAP_v3.md`, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request. Order = recommended working order. Invariants per task are a **proposal**; Claude Code confirms them against TESTING §4 in its plan.

### P0 — committed (42.5 points)

#### Debt first — 4 points (quick fixes and the documentation drift)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-01 | Review the open Dependabot PRs #164 (nginx 1.31), #165 (Angular group), #166 (other group), #175 (TypeScript 7 in `e2e/`, needs **O-S3-1**): licences (CLAUDE §10), changelogs, CI; recommendation per PR | 0.5 | Each PR merged or closed by the owner with the reason in the protocol |
| S3-02 | Legacy client fixes in one PR: loading counter decremented when a load fails, also in `approveProposal` (#168); offline notice only from `/api/` answers (#171); legacy icon `<span>`s → `<mat-icon>` with `aria-hidden` (#137) | 1 | One unit or component test per issue; issues closed |
| S3-03 | CodeQL advanced setup: workflow file that also runs on Dependabot PRs (#144) | 0.5 | `Analyze` runs on a normal PR and a Dependabot PR; the owner makes the checks required again |
| S3-04 | Scripts PR: guard `setText` in `check-i18n.mjs` against prototype keys with a self-test case (#182, CodeQL alert 3); permanent `node:test` self-test of `check-invariants.mjs` in CI stage 1 (#185) | 0.5 | Alert 3 closed; the gate's self-test runs in CI |
| S3-05 | Documentation session (#180, TD-14): ARCHITECTURE v16 (O-22 interim rule, Platform as built with Problem Details, C93 `/api/Health` removed), SPEC v3 (`email-unconfirmed` in §8.6, open footer targets in §13), TESTING status line, document headers naming current versions; reflow invariant per **O-S3-3** (#188). ROADMAP and SPRINT_02 are already done in the control chat | 1 | #180 and #188 closed; every current document names the current versions of the others |
| S3-06 | Swagger UI and OpenAPI documents per **O-S3-2** (#183): `Development` only, or kept in Production with the reason recorded | 0.5 | #183 closed; a test or the protocol shows the behaviour per environment |

#### M1.1d AutoMapper and build warnings — 4.5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-07 | AutoMapper replacement (#31) with the generic CRUD maps (#90) and the ARC-06 successor. Run 1: ADR options (manual mapping, a source generator with an OSI licence that meets D12, other) incl. what replaces ARC-06 → **O-S3-4**. Run 2: implement, remove the package and the advisory suppression | 3 | No AutoMapper package; Dependabot alerts 1, 8, 9, 10 closed; #90 fixed with tests; ARC-06 replaced in the next TESTING version; ADR in the next ARCHITECTURE version |
| S3-08 | Build warnings and analyzer check on all projects (#118): fix the 55 warning locations of the legacy projects, `TreatWarningsAsErrors` in `src/Directory.Build.props`, analyzer check of CI stage 1 on every project, drop the unused `Serilog.Settings.Configuration` | 1.5 | Release build of `ProcureAccess.sln` with 0 warnings; CI fails on a new warning in `src/` |

#### M1.5 Theme foundation — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-09 | Theme foundation per SPEC §7.5: `color-scheme: light dark`, `mat.theme` in a light and a generated dark variant, class `theme-light` / `theme-dark` on `html` from `prefers-color-scheme` (R-SET-02, no browser storage), `prefers-reduced-motion`; strong focus indicators (#184, Handoff §1.1); the `Poppins` TODO; the H1 type scale of S04/S80 (S2-18 P4) | 3 | Both appearances switch with the system setting without reload; tokens only (no hex, no px font sizes); #184 closed |
| S3-10 | **A11Y-08** (development part): the axe run of A11Y-01 repeated in the dark appearance on every journey state and shell page; contrast findings on shell pages fixed with tokens; findings on legacy screens suppressed with an issue link (TESTING §12.4) | 2 | A11Y-08 `active` in the next TESTING version, the persisted-setting part recorded as deferred to step 3 |

#### M2.1 Catalog domain model — 11 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-11 | Design note for the catalog module (**O-S3-5**): `CatalogDbContext` and schema per ADR-014; how the legacy tables (`Criteria`, `CriteriaFilter` types 1–4, `CriteriaFilterExclusion`, seed rows 2 and 3) reach the new model — a generated legacy release folder imported by CLI, or an EF data migration; what the legacy endpoints read afterwards; how CAT-10 stays green; which entities live in `Catalog.Contracts` | 1 | Owner decision recorded; the note is the draft of the protocol |
| S3-12 | Core entities and first migration: `CatalogRelease`, `Criterion` (with `Key`, `Kind`), `CriterionRevision`, `CatalogReleaseCriterion`, `StandardReference`; immutability of a published release in the domain | 3 | Migration applies (OPS-03 for the new context); proposal **CAT-05** (unit part) active |
| S3-13 | Filter dimensions and rules: `AppType`, `ProductPart`, `UseCaseTemplate` (`IsActive`), `TestType`, `FilterExclusion`, `CatalogReleaseFilterExclusion`, `CatalogReleaseAppTypeProcedure`, `CriterionApplicability`, `PriorityRecommendation`, `CriterionProcedureText` | 4 | Migration applies; proposal **CAT-13** active (exclusion target layer must be lower) |
| S3-14 | Test data for step 2: `MiniCatalog` fixture and builders in `Tests.Shared` (TESTING §6, §9 step 2) with use cases, app types (incl. one without criteria), product parts, test types, exclusions and priority rules on all four levels | 2 | Used by at least one test of S3-12/S3-13; documented in TESTING §6 |
| S3-18 | Seed rows 2 and 3 dropped (`CriteriaFilterExclusion` 17 → 12, 17 → 13; ARCHITECTURE §16.3.2) by a legacy migration; J1 no longer expects the two mobile app types to be excluded by HR-Anwendung. Per O-S3-5 (C) the existing criteria are not copied into the catalog: the legacy endpoints keep reading the legacy tables until S10 (sprint 04) and steps 5/6 retire them (S3-11 §2.4, §2.7) | 1 | **CAT-10 green** (API and E2E part, expected sets unchanged); J1 green; OPS-03 green |

#### M2.2 Release data format and CLI — 8 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-15 | Release data format: folder layout and JSON of `catalog/releases/<version>/` (criteria revisions, procedure texts, applicability, priorities, exclusions, procedure mapping), loader and full validation (references, exclusion layers, procedure mapping, allowed ratings) | 3 | Validation errors name file and path; a test release folder for the tests |
| S3-16 | CLI `catalog import <version>`: validates the whole folder and imports atomically as an immutable release (R-ADM-01) | 3 | Proposal **CAT-23** (import part), **CAT-05** (integration part), **CAT-07** active |
| S3-17 | CLI `catalog activate <version>`; active release; catalog cache keyed by `CatalogReleaseId` | 2 | Proposal **CAT-23** (activate part), **CAT-09** active |

#### M2.3 Release 2026.1 texts — 3 points (P0 since O-S3-5)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-27 | Release 2026.1, part 1: the BIT inklusiv texts of Kriterienkatalog Teil A/B and one procedure criterion per test type as a release folder in the format of S3-15 — **without** priorities and plain-language titles (O-1, K-1 from the project team) | 3 | `catalog import 2026.1` passes the full validation; the release is not activated until O-1 (O-S3-5, Q2 (iii)) |

#### M2.4 Catalog endpoints — 5 points (+ 1 point debt)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-19 | `GET /api/v1/catalog/filter-options` per SPEC §8.1: layers, exclusions with `excludedBy[]` (no reason text), inactive use cases not offered, `appTypesWithoutCriteria[]`; versioned routes and the duplicate Swagger parameters fixed (#153); endpoint style and #176 per **O-S3-6** | 3 | Proposal **CAT-12** (catalog-endpoint part), **CAT-18**, **CAT-19** active; #153 closed |
| S3-20 | `GET /api/v1/catalog/criteria` per SPEC §8.2: selection validation with field errors, resolved priority most-specific-first and strictest per app type with `rationale`, `source`, `matchedRule`, allowed ratings, procedure texts of the selected app types, empty requirement list plus `appTypesWithoutCriteria[]` | 3 | Proposal **CAT-01, CAT-02, CAT-06** (release pinning by id), **CAT-11, CAT-14, CAT-16** (selection and detail part), **CAT-20** (contract part), **CAT-21** active. **Split (owner, 2026-11-09):** S3-20a selection check and priority resolution (CAT-01, CAT-14; unit parts of CAT-02, CAT-11), S3-20b endpoint (the rest) |

#### M2.8 „Kriterium vorschlagen“ — 1 point

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S3-21 | Footer link „Kriterium vorschlagen“ (R-SUG-01): configurable channel `Contact:CriteriaSuggestion` (`mailto:` with `suggest.mailSubject` / body, or URL), 🔧 placeholder until O-6; the production build fails on the placeholder (as GP.3 plans for legal pages). The S11 part follows with M2.6 | 1 | Link in the footer; component test; Figma link check green |

### P2 — stretch (5.5 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S3-22 | Snackbar buttons in German (#160) — only once the UX copy session #181 has the wording | 0.5 | TECH_DEBT_v2 §3.1 |
| S3-23 | Usage events: write fire-and-forget with a retry delay, before the writer gets its first caller in M2.6/M2.7 (#115) | 1 | ROADMAP_v3 M2.7d |
| S3-24 | Persist the Data Protection keys outside the container (#186) | 1 | TECH_DEBT_v2 TD-21 (M3, before the cookie session) |
| S3-25 | Font subsetting of Material Symbols per **O-S3-7** (#135) | 2 | TECH_DEBT_v2 §3.3 |
| S3-26 | ESLint for `e2e/` and `scripts/*.mjs` (#161); `eslint-plugin-playwright` only with **O-S3-8** | 1 | TECH_DEBT_v2 §3.5 |

Stretch items are pulled in order; if one is started but not finished, it goes back to its milestone backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | S3-01 – S3-06; S3-07 run 1 (ADR options); S3-11 (design note) | Debt quick fixes merged; **owner decisions O-S3-1 – O-S3-5 taken** |
| First half | D1–D4 | S3-07 run 2, S3-08 (backend); S3-09, S3-10 (frontend); S3-12 – S3-14 | **Mid-sprint check (end of D4):** M1 finished and the catalog model merged (≈ 24 points done). If not: drop the stretch and move S3-21 and S3-20 to sprint 04 |
| Second half | D5–D8 | S3-15 – S3-17, S3-18, S3-19, S3-20, S3-21, S3-27 | Release imported by CLI; endpoints live; CAT-10 green |
| Buffer | remaining | stretch S3-22 – S3-26 | – |

Backend (S3-07, S3-08, S3-11 – S3-20) and frontend (S3-02, S3-09, S3-10, S3-21) are independent; if one side waits for a decision, switch to the other.

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| S3-01 (#175) | **O-S3-1** | Philipp, D1 |
| S3-05 (reflow invariant) | **O-S3-3** | Philipp, D1 |
| S3-06 | **O-S3-2** | Philipp, D1 |
| S3-07 run 2 | **O-S3-4** (ADR) | Philipp after run 1 |
| S3-08 | S3-07 merged (same projects) | internal |
| S3-10 | S3-09 merged | internal |
| S3-12 – S3-18 | **O-S3-5** (design note S3-11) | **Decided 2026-11-06** |
| S3-13 | S3-12 | internal |
| S3-16 | S3-15; S3-12 – S3-14 | internal |
| S3-17 | S3-16 | internal |
| S3-18 | none (O-S3-5 C: legacy migration and J1 only) | internal |
| S3-19, S3-20 | S3-17; **O-S3-6** for S3-19 | **O-S3-6 decided 2026-11-06** |
| S3-03 | Owner re-adds the `Analyze` checks to the ruleset of `main` after the merge | Philipp |
| S3-22 | UX copy session #181 | Philipp |
| S3-27 | S3-15 (format), S3-16 (import); priorities and titles O-1, K-1 stay with the project team | internal |
| **Invariant flips** | S3-05, S3-07, S3-10, S3-12, S3-13, S3-16, S3-17, S3-19, S3-20 each write a new TESTING version: **one at a time**, the next only after the previous is merged | internal |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal |
|---|---|---|---|
| O-S3-1 | `e2e/` TypeScript: follow the client's TypeScript (Dependabot ignore rule like the client) or update on its own? (#175, TECH_DEBT_v2 §6) | S3-01 | Follow the client: one TypeScript version in the repository is easier to hand over |
| O-S3-2 | Swagger UI and OpenAPI documents in Production: wanted for the public API, or `Development` only? (#183) | S3-06 | `Development` only until the API is meant for third parties; the OpenAPI JSON can be exported as a file if needed |
| O-S3-3 | Reflow invariant: give `reflow.spec.ts` an ID (e.g. A11Y-09) in the next TESTING version? (#188) | S3-05 | Yes — rebuilt screens then must be added to the check, and the gate enforces it |
| O-S3-4 | AutoMapper replacement and the ARC-06 successor (#31), as an ADR (CLAUDE §11) | S3-07 run 2 | Options with licence and effort in run 1 |
| O-S3-5 | Catalog persistence and the path of the existing criteria into the new model (design note S3-11) | S3-12 | **Decided 2026-11-06** (S3-11 §2.9): **C** no copy of the existing criteria, **D1** filter values seeded in the catalog migration, **Q2 (iii)** import accepts a release without priorities, activate refuses it, criterion proposals kept; documents reworded as in S3-11 §2.4 C (ARCHITECTURE with S3-12); `Catalog.Contracts` as in S3-11 §2.2 |
| O-S3-6 | Endpoint style of `/api/v1/catalog/*` (controllers or minimal APIs); milestone of #176 | S3-19 | **Decided 2026-11-06:** minimal APIs, `internal` in the module and mapped by the host on its versioned group; #176 done in S3-19 (milestone M2); only the new endpoints under `/api/v1`, URL segment as the only version reader (#153) (ARCHITECTURE_v22 C107) |
| O-S3-7 | Font subsetting option of #135 | S3-25 (stretch) | Options are in the issue |
| O-S3-8 | `eslint-plugin-playwright` as a new dev dependency (#161) | S3-26 (stretch) | Licence check in the task first |
| — | UX copy session #181 (TD-15): page-title variants for S11/S35, singulars, problem-type texts, snackbar buttons, delete reason, account button | before sprint 04 (S11) | — |
| — | Priorities, rationales and plain-language titles (O-1, K-1) from the project team | before M2.3 is finished | — |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | The path of the existing criteria into the versioned model is not written down (generated legacy release vs. data migration), and CAT-10 must stay green | S3-12 – S3-18 move code twice or break J1 | Design note S3-11 and O-S3-5 on D1; CAT-10 expected sets never change. **Closed 2026-11-06:** O-S3-5 C — no copy, the legacy filter stays on the legacy tables until sprint 04 |
| R2 | AutoMapper is used by every legacy controller and the generic CRUD maps (#90) | S3-07 grows | ADR first; replace only the maps that exist; legacy controllers keep their behaviour (tests of #90 first) |
| R3 | Priority resolution (four levels, several app types, strictest wins) is the most rule-dense code so far | S3-20 grows | Table-driven tests over `MiniCatalog` (CAT-01, CAT-14) before the endpoint; split S3-20 if the plan shows more than one session |
| R4 | Nine tasks write a new TESTING version | Merge conflicts, gate red | One flip at a time (Dependencies); invariant-free tasks can run in between |
| R5 | The velocity rests on one sprint of mostly small tasks | Over-commitment | 75 % load, stretch at the end; record days used at the close-out |
| R6 | The dark appearance shows contrast failures on legacy screens with hex colours (J1 runs on `/filters`) | S3-10 grows | Fix only shell pages; legacy findings suppressed with one issue link — the screens are rebuilt in sprint 04 |
| R7 | O-1 / K-1 content does not arrive | M2.3 cannot finish, the partner preview waits | Endpoints are built and tested on `MiniCatalog` and the test release; S3-27 prepares the texts |
| R8 | A contracts type of the catalog needs another module's contracts (O-22) | S3-12 / S3-19 stop | Stop and ask (CLAUDE §6.6); ARC-03 forbids it until decided |
| R9 | Pressure to start the S10/S11 screens early | Pages built before the endpoints and before the UX copy session | No page in this sprint; sprint 04 starts with S10 on the finished endpoints |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6.7) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`; CodeQL again once S3-03 is merged).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, deferred parts recorded there.
- [ ] UI strings only by key; `node scripts/check-i18n.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-invariants.mjs` and `reuse lint` green.
- [ ] New files carry SPDX headers; no secrets committed; licences of new dependencies checked (CLAUDE §10) and approved by the owner.
- [ ] Documentation touched by the change updated (`CLAUDE.md` §3/§4/§5, ARCHITECTURE for ADRs and the catalog module, SPEC §13); register items closed by the PR named in it.
- [ ] Protocol `docs/sprints/sprint_03/S3-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in sprints 01 and 02.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; quick debt tasks; ADR options (S3-07 run 1), catalog design note (S3-11); owner decisions O-S3-1 – O-S3-5 |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: success criteria checked, smoke test on the compose stack, both appearances on every shell page |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v3.md`; close-out below |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 04 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** configurator S10 (M2.5) with its debt (#52, J1 and CAT-10 rewrite, `vitest-axe`), then the criteria list S11 (M2.6) once the UX copy session #181 is done; release 2026.1 (M2.3) as soon as O-1 / K-1 arrive.

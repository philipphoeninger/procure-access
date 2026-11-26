# ProcureAccess — Roadmap

> **Status:** v7 (2026-11-24) — update after sprint 06, created with the skill *Product Management/roadmap-update*: M5 done except three points (#414, #420, #429); GP.2, GP.3, GP.6 and half of GP.d done; G-P re-estimated with the debt `TECH_DEBT_v6.md` binds to the pilot; M6 re-estimated with its debt rows at its first sprint planning and started in sprint 07; measured velocity of sprint 06 and a new forecast per gate. v6 (2026-11-17) was the update after sprint 05, v5 (2026-11-13) after sprint 04, v4 (2026-11-10) after sprint 03, v3/v3.1 after sprint 02, v2 after sprint 01, v1 (2026-10-19) the output of Phase 3, step 10 ("Create the roadmap") of `ai_development_process.md`. What changed: §8.
> **Based on:** `SPEC_v2.md` (feature areas, requirements `R-…`, mapping §11, phasing §12, open questions §13), the highest ARCHITECTURE (v107; module rules §4.3, §5.5, §8.1, §10, migration path §12, open questions §16.5 O-25 – O-32), the highest TESTING (v114; invariants per step §9), `TECH_DEBT_v6.md` (§3 register, §5 debt per migration step, §6 owner decisions, §9 fix plan), `TECHNICAL.md`, the sprint-06 close-out and protocols in `docs/sprints/sprint_06/`, `docs/sprints/sprint_07/SPRINT_07.md`.
> **Language:** English (owner decision, 2026-10-19).
> **Input for:** sprint planning (step 11). Each sprint takes work packages from the current milestone; a work package is the unit that is sliced into tasks.

---

## How to read this roadmap

- **No dates.** There is no external deadline and no pilot date (SPEC SD18). The roadmap fixes *order* and *relative size*, not time. Forecasts in time can be derived later from the measured velocity (see §6).
- **Milestones follow the migration steps** (`ARCHITECTURE §12`, SPEC §11): M0–M7, plus two **gates** that are not migration steps but release conditions: **G-P** (pilot readiness) and **G-R** (public release). A working application exists at the end of every milestone.
- **Grouping:** *Now* (committed, no open decisions), *Next* (planned, scope clear, some external input needed), *Later* (directional; scope may change, P1 is only outlined in SPEC).
- **Every requirement of SPEC_v2 is assigned to a work package** (§7 checks this). Invariants are not repeated here: the invariants a milestone activates are listed in SPEC §11 and TESTING §9.
- **Debt that rides with a milestone** (`TECH_DEBT_v*.md` §9, column *When*) is counted in the milestone sum from v2 on, as separate rows marked *(debt)*. It is estimated when the milestone is re-estimated at its sprint planning; until then a milestone lists its debt items without points. General debt that belongs to no package (live defects, dependency updates) is not in the roadmap; it comes out of the 15–20 % debt budget of each sprint.

### Effort points

Relative effort on a Fibonacci scale, estimated for **one developer working with Claude Code**, and including everything needed to finish a package: backend, frontend, UI texts, automated tests with their `INV:` tags, accessibility in both appearances, and the documentation the package changes.

| Points | Meaning (reference) |
|---|---|
| 1 | Small, fully understood change; one focused session (e.g. the criteria-suggestion link R-SUG-01) |
| 2–3 | A small feature or a technical change in one layer (e.g. Problem Details middleware with client mapping) |
| 5 | A feature with backend, frontend and tests, no new concepts (e.g. share links S15/S16) |
| 8 | A feature with new concepts or several screens (e.g. export pipeline XLSX/CSV with dialog and rate limit) |
| 13 | A large, coherent package with new domain model and several screens; split into several sprint tasks |
| 21+ | Not used — anything that large is split into packages |

**Confidence** says how reliable a milestone's sum is: *high* (scope fixed, technology known), *medium* (scope fixed, some unknowns), *low* (scope only outlined or depends on open decisions).

---

## 1. Overview

| Milestone | Theme | Group | Points | Confidence | Status | Main dependency |
|---|---|---|---:|---|---|---|
| **M0** | Platform upgrade and test foundation | Done | 28 | – | **Done** (sprint 01) | – |
| **M1** | Foundations: modules, shell, theme, errors | Done | 44 | – | **Done** (sprints 01–03) | – |
| **M2** | Catalog versioning, configurator, anonymous export | Done* | 78 | – | **Done** except M2.3 priorities (73 done, 5 **blocked** on O-1/K-1) | M1 ✓ |
| **M3** | Accounts, organisations, session, saved lists | Done | 67 | – | **Done** (sprint 05; M3.4 in S4-25) | – |
| **M4** | Session warning and hardening | Done | 8 | – | **Done** (sprint 05, stretch S5-29, S5-30) | – |
| **M5** | Workspace: projects, candidates, assessment | Done* | 50 | – | **Done** except 3 points (47 done): the assessments in the data export (#414), assign or replace a project's list on S32 (#420), the path back from S11 of a copy (#429) — **sprint 07** | owner: shape and dialog design (O-S7-1 – O-S7-3) |
| **G-P** | Pilot readiness (gate) | Now | **31.5** (v6: 25) | medium | 10 done (GP.2, GP.3, GP.6, half of GP.d); 21.5 in **sprint 07**, 7 of them only after outside input | staging (TD-28, #236), O-S5, legal page content, O-14, O-16, the tester's pass; 2026.1 active (proposal O-S6-8) |
| **M6** | Directory, publication, contribution | Now | **96** (v6: 86) | medium | 30 in **sprint 07** (core, search, read projections) + 8 stretch; rest sprint 08 | M5 ✓; owner decision on splits (TD-31, O-S7-10) |
| **G-R** | Public release (gate) | Later | 8 | low | Not Started | M6; legal review O-4, O-6 |
| **M7** | Moderation and P1 | Later | 32 | low | Not Started | G-R; SPEC_v3 |
| | **Total P0 (M0–G-R)** | | **410.5** (v6: 394) | | 277 done, **133.5 open** (5 of them blocked: M2.3) | |
| | **Total incl. P1 (M7)** | | **442.5** (v6: 426) | | | |

**Relative weight:** M2, M3 and M6 together are about 60 % of the P0 effort. M6 is the largest single milestone because all three write paths into the directory must land together (`ARCHITECTURE §12`, step 6); it is split into packages, not into milestones.

```
Done:   M0 ✓ ──► M1 ✓ ──► M2 ✓ (except the 2026.1 priorities) ──► M3 ✓ ──► M4 ✓ ──► M5 ✓ (except 3 points)
Now:                                                                                └─► G-P (pilot)
                                                                                    └─► M6 (started beside G-P)
Later:                                                                                    └─► G-R (public release) ──► M7 (P1)
```

The arrows are the only order that is fixed. Two things may be pulled forward: the **staging environment** (G-P) can be set up right after M2 for an early partner preview of the anonymous core flow (SPEC §12: "first pilot-ready milestone"), and **M4** is small enough to be done inside the last sprint of M3.

---

## 2. Now

### M0 — Platform upgrade and test foundation

**Status: done in sprint 01** — all six packages (S1-01 – S1-21), exit criteria met; invariants ARC-06, OPS-03, CAT-10, A11Y-01 active. Kept here for the record.

**Goal:** the existing application runs on .NET 10 and Angular 22, and every test level exists once, so all later code is built and tested on the target platform. No user-visible change.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M0.1 | .NET 8 → 10, EF Core 10, Npgsql 10; build and run unchanged | ARCH C41, ADR-013 | 5 |
| M0.2 | Angular 20 → 22 (`ng update` per major), zoneless, standalone components | ARCH C37 | 8 |
| M0.3 | Backend test foundation: xUnit v3, Shouldly, NSubstitute, Testcontainers (PostgreSQL 17), Respawn, NetArchTest, `FakeTimeProvider`, shared fixtures and antiforgery-aware test client | TESTING §3, §9 step 0 | 5 |
| M0.4 | Frontend test foundation: Karma/Jasmine → Vitest + Angular Testing Library | TESTING §3 | 3 |
| M0.5 | CI pipeline (GitHub Actions): build, tests, axe gate stub, `check-invariants.mjs`, `check-figma-links.mjs`; Playwright stack in docker-compose | TESTING §8, ARCH §10 | 5 |
| M0.6 | Repository hygiene: secrets only from environment, REUSE/SPDX headers, licence, `CLAUDE_v3.md` in the repository root | CLAUDE §10 | 2 |
| | **Sum** | | **28** |

**Exit criteria:** application starts and behaves as before; CI green with the step-0 invariants of SPEC §11 active.
**Risk:** two Angular majors and the zoneless switch in one go may surface breaking changes in third-party packages → confidence *medium*.

### M1 — Foundations

**Status: done** — sprint 02 delivered M1.1, M1.3, M1.4, M1.6 and most of their debt; sprint 03 the AutoMapper replacement (ADR-015), a warning-free build (#118, 104 warning locations, not 55 as v2 said) and the theme foundation with A11Y-08 (development part). Exit criteria met: the shell passes axe in both appearances, ARC-01 – ARC-03 enforce the module boundaries, UX-O9 is decided (SD25).

**Goal:** module structure, application shell, theme (light and dark), error handling and texts are in place, so every feature from M2 on only adds content.

| # | Work package | Requirements / sources | Points | Status |
|---|---|---|---:|---|
| M1.1 | Module projects with `Contracts`, module registration, NetArchTest boundary rules | ARCH §4.3 | 8 | **Done** (S2-06 – S2-08; ADR-014) |
| M1.1d | *(debt)* Invariant gate tightening #109, #111; AutoMapper replacement #31 with #90 and the ARC-06 successor; build warnings and analyzer check on all projects #118 | TECH_DEBT_v2 §9.1 | 6 | **Done** (S2-09, S3-07, S3-08; ADR-015) |
| M1.2 | Operations basics: Serilog in its own schema with retention, `/health` endpoints, `RebuildDatabase` guard | R-ADM-04 (part), ARCH §9.5 | 3 | **Done** (S1-22) |
| M1.2d | *(debt)* API Dockerfile hardening #77; #74; #73 | TECH_DEBT_v1 §9.2 | 1 | **Done** (S2-21) |
| M1.3 | Problem Details middleware with field-keyed `errors`, problem types, client mapping to UX keys | SPEC §7.1, §8.6 | 3 | **Done** (S2-10, S2-11) |
| M1.4 | Application shell: landmarks, anonymous header, footer, start page, error pages, loading/empty/system states, page title and H1 focus with announcement, 320 px / 400 % zoom | R-SHL-01, R-SHL-04, R-SHL-05, R-SHL-06, R-SHL-08, R-SHL-09, R-SHL-10 | 8 | **Done** (S2-15 – S2-18) |
| M1.4d | *(debt)* #120 English accessible names, #101 key conflict, #110 Figma link check, #122 `/api/Health` → `/health/ready` | TECH_DEBT_v1 §9.2 | 2 | **Done** (S2-13, S2-15, S2-19, S2-22) |
| M1.5 | Theme foundation: tokens, `color-scheme: light dark`, appearance without account, reduced motion; dark theme in development with the generated palette; strong focus indicators (#184) | R-SET-02, R-SET-03 (development part), SPEC §7.5 | 5 | **Done** (S3-09, S3-10); light values of Handoff §1.2 pinned in sprint 04 (S4-29, owner 2026-11-11); dark values with O-S5 (#240) |
| M1.6 | ngx-translate with `de.json` from the UX copy; pluralisation approach | UX-O9 | 3 | **Done** (S2-12, S2-13; UX-O9 → SD25) |
| M1.6d | *(debt)* TypeScript lint rules #119; client coverage exclusions #97; CI one-liners #104 – #107 | TECH_DEBT_v1 §9.2 | 2 | **Done** (S2-14, S2-20, S2-23) |
| M1.7 | `UsageEvent` table and writer | R-MET-01 | 1 | **Done** (S1-23) |
| M1.8 | CI browser matrix: Chromium, Firefox, WebKit, mobile viewports | SPEC §7.4 (SD19), TESTING V56 | 2 | **Done** (S1-18, S1-20) |
| | **Sum** | | **44** (packages 33 + debt 11) | **all done** |

**Why the sum grew (+33 %):** v1 counted only the packages. The debt that TECH_DEBT_v1 §9.2 binds to step 1 — above all the AutoMapper replacement (#31, licence and CVE), the build warnings (#118) and the gate (#109, #111) — has to be done while the code moves and was not in any package. M1 therefore takes sprints 02 and 03 instead of one sprint.

**Exit criteria:** shell passes axe in both appearances; architecture tests enforce module boundaries; UX-O9 decided.

---

## 3. Next

### M2 — Catalog versioning, configurator and anonymous export

**Goal:** the primary persona's complete flow without an account: select → prioritised, explained criteria → adjust → export as XLSX/CSV. **First point at which a cooperation partner can try the tool** (SPEC §12).

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| M2.1 | Catalog domain model and migrations: releases, revisions, applicability, priorities, standards, procedure texts, app types, product parts, use cases, test types, exclusions; seed rows 2 and 3 dropped. The existing criteria are not copied: they reach the catalog as part of release 2026.1 (M2.3), and the legacy filter stays on the legacy tables until S10 (O-S3-5) | ARCH §5.1, §12 step 2 | 11 | **Done** (S3-11 – S3-14, S3-18) |
| M2.2 | Release data format and CLI `catalog import` / `catalog activate`, atomic with full validation | R-ADM-01 | 8 | **Done** (S3-15 – S3-17) |
| M2.3 | Release 2026.1 as data folder from Kriterienkatalog Teil A/B, BIT inklusiv texts, one procedure criterion per test type; priorities and plain-language texts from the project team | R-ADM-01, O-1, K-1 | 8 | Texts **done** (S3-27; gap list v3). Open (5): priorities, rationales and titles — **blocked on O-1/K-1** ("unclear", 2026-11-13); until then the legacy filter stays where no release is active (O-S4-3) |
| M2.4 | Catalog endpoints: filter options with exclusions, criteria with resolved priority, rationale, allowed ratings, procedure texts | SPEC §8.1, §8.2 | 5 | **Done** (S3-19, S3-20a, S3-20b) |
| M2.4d | *(debt)* Versioned routes `/api/v1` and the duplicate Swagger parameters #153, #176; `Accept-Language` on the catalog list endpoints (API-06) | TECH_DEBT_v2 §9.2; TESTING §9 step 2 | 2 | **Done** (S3-19, S4-23) |
| M2.5 | Configurator S10: four steps, exclusions with reason, reset of lower layers, „Bisherige Auswahl“, in-memory draft | R-CON-01 – R-CON-10 | 13 | **Done** (S4-09 – S4-12, S4-30) |
| M2.5d | *(debt)* An active test release in the E2E stack (#241); #52 stepper names; J1 rewrite; CAT-10 rewrite incl. #91, #92; the configurator's legacy `LiveAnnouncer` calls; empty states and button spinner; `vitest-axe` | TECH_DEBT_v3 §9.1 | 4 | **Done** (S4-08, S4-10, S4-13, S4-14b); #52 now tied to the legacy filter (TECH_DEBT_v4) |
| M2.6 | Criteria list S11 (anonymous part): cards and detail panel, K5 with rationale, override, include/exclude, notes, search and filter, „Geprüft nach …“, empty state, notices, action bar | R-LST-01 – R-LST-10 | 13 | **Done** (S4-14 – S4-18b) |
| M2.6d | *(debt)* `pageTitle` as an `AREAS` root and `shell.pageTitle.error`; German messages or codes for field errors | TECH_DEBT_v3 §9.2 | 1 | **Done** (S4-18a) |
| M2.7 | Export: pipeline with `AdHocSelectionSource`, XLSX (ClosedXML) and CSV per SPEC §8.3, dialog S13, rate limit; `ExportCreated` and `CriteriaListShown` events | R-LST-11, R-LST-12, SPEC §9.1 | 8 | **Done** (S4-19 – S4-21b) |
| M2.7d | *(debt)* #115; #112 with the first rate limit; anonymous export out of the logs (EXP-05); E2E seeder and relaxed E2E rate limit; ARC-03 rule for `AdHocSelectionSource`; #42 bundle budget | TECH_DEBT_v3 §9.2 | 4 | **Done** (S3-23, S4-19, S4-21a, S4-22) |
| M2.8 | „Kriterium vorschlagen“ link with configurable channel | R-LST-13, R-SUG-01 | 1 | **Done** (S3-21, S4-18a); the channel stays a 🔧 placeholder until O-6 |
| | **Sum** | | **78** (packages 67 + debt 11) | **73 done**; 5 blocked on O-1/K-1 |

**Exit criteria:** journey J1 (anonymous part) passes keyboard-only and with axe; an exported file passes the screen-reader check (TESTING §7.2).
**Dependency:** M2.3 cannot be finished without the priorities and plain-language titles from the project team (O-1, K-1). Development can start with a reduced test catalog; the data folder is the review artefact for the team.
**Status after sprint 04:** done as a development milestone. Exit criteria: J1 (anonymous part) passes keyboard-only and with axe in both appearances (S4-22). Still open: the screen-reader check of an exported XLSX and the manual A11Y-07 pass of step 2 by the accessibility tester (#311), and the 2026.1 priorities (O-1/K-1). Follow-ups outside the sum: the rating legend on S11 (#279), the short description in the criteria answer and on S11 (#299), the reload flow after `release-mismatch` (#310), design fidelity of S10/S11 (#278, #281, #282).
**Pages built:** configurator S10, criteria list S11 and export dialog S13 (sprint 04).

### M3 — Accounts, organisations, session and saved lists

**Status: done in sprint 05** — every package and debt row (S5-07 – S5-28, several split into a/b); exit criteria met: J1 second part (save after sign-in) is green, the tenant-isolation invariants TEN-01, TEN-02, TEN-05 (organisation part), TEN-08, TEN-09, TEN-11, TEN-12 are active. The legacy auth is gone (JWT, `Identity.UI`, `UsersController`, legacy account and settings pages). Left over and moved: the users are still in the legacy context (TD-72, #387 → M5 debt row M5.0d); rate limits for sign-in and organisation creation (#318, TD-60 → GP.d); #368, #375, #380 close in sprint 06.

**Goal:** people can register, work in organisations and save, edit and share criteria lists, on the new cookie session model.

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| M3.1 | Identity cookie authentication with antiforgery replaces the JWT; session probe, login outlet with draft kept, 401 → login overlay, sign-out | R-AUTH-03, R-AUTH-05, R-AUTH-07 (base), R-AUTH-08 | 8 | **Done** (S5-07 – S5-09) |
| M3.1d | *(debt)* Legacy auth goes with the JWT: `Identity.UI`, `{ code, message }` failures, `DateTime.UtcNow` in `UserService`/`UserRepo`, `ERROR_TRANSLATIONS`, antiforgery on `criteria-list-shown`, `signedIn` from the session, the auth entries of `LEGACY_EXCEPTIONS`; test hosts #87 – #89 | TECH_DEBT_v4 §5, §9.1 | 3 | **Done** (S5-07, S5-10) |
| M3.2 | Registration, e-mail confirmation gate, password reset, lockout; `IEmailSender` with Mailpit locally and Lettermint templates (`email.*`) | R-AUTH-01, R-AUTH-02, R-AUTH-04, SPEC §7.6 | 8 | **Done** (S5-11 – S5-13) |
| M3.2d | *(debt)* Mails to the real recipient (#302); registration without the missing `Member` row (#301); Mailpit and real secrets in the E2E stack incl. the `compose.log` check (TD-12); E2E users in the seeder | TECH_DEBT_v4 §3.1, §9.1, §9.2 | 2 | **Done** (S5-11, S5-12, S5-14) |
| M3.3 | Organisations: create, invitations by e-mail, members page, roles, leave, last-Owner rule, authorisation matrix, query filters; header states and organisation switch | R-ORG-01 – R-ORG-06, R-SHL-02, R-SHL-03 | 13 | **Done** (S5-15 – S5-18) |
| M3.4 | Contributor-profile and moderator groundwork: entities `ContributorProfile`, `UserContributorLink`; rename `Approver` → `Moderator`; platform-role CLI | ARCH §12 step 3, R-ADM-03 | 2 | **Done** (S4-25; TEN-06 active for its part) |
| M3.5 | Saved lists: configurator entities, save S12, list S14, edit and „Auswahl ändern“, delete, saved export, older-release notice and „Neue Liste mit derselben Auswahl“ | R-SAV-01, R-SAV-02, R-SAV-03, R-SAV-06, R-LST-14 | 13 | **Done** (S5-19 – S5-21) |
| M3.5d | *(debt)* The weight range served by the server (TD-45); a too-long note leads to the note (TD-40); `criteria.list.unsaved` and the draft-note wording; TEN-04 workspace part | TECH_DEBT_v4 §9.2 | 1.5 | **Done** (S5-22) |
| M3.6 | Share links S15 with required expiry and shared list S16 without account | R-SAV-04, R-SAV-05 | 5 | **Done** (S5-23, S5-24) |
| M3.7 | Settings S60 (language, appearance, motion, contrast „wie im System“), stored per user | R-SET-01 | 5 | **Done** (S5-25, S5-26) |
| M3.7d | *(debt)* The legacy settings store goes (storage for signed-out users, English snackbar, crash on blocked storage — #234 TD-25); A11Y-08 with the explicit, persisted setting | TECH_DEBT_v4 §3.1, §9.2 | 1.5 | **Done** (S5-25) |
| M3.8 | Personal data export (JSON) and account deletion | R-AUTH-09, R-AUTH-10 | 5 | **Done** (S5-27, S5-28) |
| | **Sum** | | **67** (packages 59 + debt 8) | **all done** |

**Re-estimate at the first M3 sprint planning (sprint 05):** the packages keep their sizes; the debt that `TECH_DEBT_v4.md` binds to step 3 is now counted as *(debt)* rows (+8 points, +14 %).
**Pages built:** M3 built S01 – S03 (sign-in, registration, password reset) with the login overlay, S12, S14, S15, S16, S20, S21, S60, S61 and the signed-in header states. None of them has a Figma frame; they follow the patterns of Handoff §5.3 (owner, 2026-11-13). Their frames are an owner item before G-P (TD-80).

**Exit criteria:** J1 second part (save after sign-in) passes; tenant-isolation invariants active.
**Dependency:** Lettermint sending domain with SPF/DKIM for staging (operations; not needed for local work with Mailpit).

### M4 — Session warning and hardening

**Status: done in sprint 05** as stretch (S5-29 session hardening, S5-30 warning dialog and completed login overlay); SEC-07 and SEC-08 active. Follow-up: on `main` the J5 journey sees two warning dialogs on WebKit and fails after „Schließen“ on Chromium (TD-55, #379) — first task of sprint 06.

**Goal:** sessions end safely and predictably; users are warned before expiry.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M4.1 | Absolute limit, sliding renewal, expiry headers, extend endpoint, security stamp on password change and account deletion | R-AUTH-05 (completion), SEC-07, SEC-08 | 5 |
| M4.2 | Session warning dialog; login overlay completed | R-AUTH-06, R-AUTH-07 (completion) | 3 |
| | **Sum** | | **8** (done) |

### M5 — Workspace

**Status: done in sprint 06 except 3 points** — every package and debt row (S6-10 – S6-25, seven of them split into a/b); the exit criterion is met: J2 up to the assessment is green on all five browser projects (S6-22b; a keyboard race fixed in #431), and API-03, API-04, TEN-05, ARC-05, SIG-03 (workspace part), SIG-13, CAT-15/CAT-17 (workspace), WSP-01 – WSP-04 (workspace part) are active. The users live in IdentityAccess (#387), the proposals are retired (#384). **Open (sprint 07):** R-PRJ-04 in the UI — a list can be chosen only when the project is created (#420, part of M5.1, ≈ 1.5); PRV-07 workspace part — the export leaves out the assessments (#414, part of M5.8d, ≈ 1); the path back from S11 of a copy with its reflow case (#429, TD-91, ≈ 0.5); the evidence race answers 500 (#453, with the sprint's quick wins). Before the pilot, as G-P debt (GP.e): the copy of an archived project refused on the server (TD-86, O-30), lock and copy atomic (TD-87, O-31).

**Goal:** organisations run procurement projects: candidate products, test evidence and per-criterion assessment against the project's own copy of a criteria list.

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| M5.0d | *(debt)* The users move into IdentityAccess (ADR-014 rule 4): Identity tables out of the legacy context, host endpoints `API/Account`, `API/Auth`, `API/Organizations` and `GET /api/me` into the module, foreign keys for the four user ids, seeder and `roles` command (TD-72, #387) — if the owner schedules it here (O-S6-1) | TECH_DEBT_v5 §5, §6 | 5 | **Done** (S6-10) |
| M5.1 | Projects S30, S31, S32: list, create, overview, status; project copy of the criteria list with lock; adjusting the copy via S11 | R-PRJ-01 – R-PRJ-06 | 13 | **Done** (S6-11 – S6-14); assign/replace on S32 and the path back open: #420, #429 → S7-07, S7-06 |
| M5.2 | Candidates S33, S34: create/edit with private section, tabs as routes, overview, tri-state test status, evidence list S34a | R-CAN-02 – R-CAN-05 | 8 | **Done** (S6-15 – S6-17) |
| M5.3 | Assessment S34b: K6 with allowed values, autosave, derived „Geprüft nach …“ rows, progress, filter, rating legend | R-CAN-06 – R-CAN-08 | 8 | **Done** (S6-18, S6-19) |
| M5.4 | Concurrency: optimistic locking, conflict dialog S37, last-write-wins for autosaved assessments | R-PRJ-07, R-CAN-11 | 5 | **Done** (S6-20, S6-21) |
| M5.4d | *(debt)* ETag / If-Match for saved lists with API-03 (the 500 of a racing `PATCH`, S5-21a R1) | TECH_DEBT_v5 §5 | 1 | **Done** (S6-20) |
| M5.5 | Remove a candidate; Readers see everything read-only | R-CAN-09, R-CAN-10 | 3 | **Done** (S6-22) |
| M5.6 | Retire the proposal workflow; repoint `withProducts()` / `withFavorites()` | ARCH §8.1, §12 step 5 | 3 | **Done** (S6-23) |
| M5.6d | *(debt)* With the proposals: their personal data (TD-61, #384), `DateTime.UtcNow` in `ProposalService`, the API-02 test moved to a rebuilt form first, the `proposal-list` `LiveAnnouncer`, ARC-05 | TECH_DEBT_v5 §5 | 1 | **Done** (S6-23) |
| M5.7d | *(debt)* Root store: `withLoading()` once instead of per feature (dev-mode warning), callers of the legacy store methods catch rejections, the M5 entries of `LEGACY_EXCEPTIONS` (#158) | TECH_DEBT_v5 §5 | 1.5 | **Done** (S6-24) |
| M5.8d | *(debt)* Account deletion and data export, step-5 parts: projects of an organisation that ends; `createdByMe.projects` (PRV-07 workspace part) | TECH_DEBT_v5 §5 | 1.5 | **Done** (S6-25); assessments in the export open: #414 → S7-05 |
| | **Sum** | | **50** (packages 40 + debt 10) | **47 done**; 3 in sprint 07 |

**Re-estimate at the first M5 sprint planning (sprint 06):** the packages keep their sizes; the debt that `TECH_DEBT_v5.md` binds to step 5 is now counted as *(debt)* rows (+10 points, +25 %), half of it the move of the users (TD-72), which no package covered. Without O-S6-1 the sum is 45.
**Pages built:** S30, S31 (dialog), S32, S33, S34 with its tabs, S34a, S34b, S37 (dialog), the confirmation to remove a candidate, S11 in project-copy mode and the read-only views for Readers. Figma frames exist for S32, S34, S34a, S34b and S37 (with 360 px frames for S32, S34 and S34b); S30, S31 and S33 follow Handoff §5.3. Built in sprint 06; S30, S31, S33, the remove dialog and the read-only tabs still have no Figma frame (TD-80).

**Exit criteria:** J2 up to the assessment passes; workspace invariants active (API-03, API-04, TEN-05 complete, ARC-05, SIG-03 workspace part, PRV-07 workspace part, CAT-15 workspace, CAT-17 workspace, SIG-13, WSP-01 – WSP-04 workspace part; TESTING §9 step 5).
**Note:** „Merken“ from the directory (R-CAN-01) needs the directory and is part of M6.

### G-P — Pilot readiness (gate)

**Goal:** a cooperation partner can use M0–M5 on a stable staging environment (PRD goal 4).

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| GP.1 | Staging environment: separate database, deployment from CI, Lettermint project and API token | ARCH §10.2 | 5 | Sprint 07: repository hardening — ports, public-address settings, health route (#449, #447, #448; S7-09) 2; deployment from CI and the first deployment with the owner's checklist (TD-28, #236; S7-10) 3 |
| GP.2 | Nightly backup and documented restore rehearsal | R-ADM-04 (completion) | 3 | **Done** (S6-29; local rehearsal, runbook in `TECHNICAL.md` §7) |
| GP.2d | *(debt)* Backup gaps: off-host copy, alert on an unhealthy service, check against empty dumps, the staging rehearsal (TD-92, #455) | TECH_DEBT_v6 §3.2 | 1.5 | Sprint 07 (S7-11; the rehearsal after S7-10) |
| GP.3 | Legal pages from Markdown files; production build fails on placeholders | R-SHL-07 | 2 | **Done** (S6-28); the texts are placeholders until WERTE.IT delivers them (TD-29) |
| GP.4 | Dark theme release: pin the reviewed values after the Figma dark mode and the accessibility review | R-SET-03 (release), O-S5 | 3 | Sprint 07 (S7-19), **only after O-S5** (#240, TD-34) |
| GP.5 | Manual accessibility pass (A11Y-07) and fixes | TESTING §7.2 | 5 | Sprint 07: the tester's guide for M2 – M5 (S7-17) 1; the fixes (S7-18) 4, **only after the tester's pass** (TD-49, #311) |
| GP.6 | `TECHNICAL.md` (operations, onboarding) with the documented metric queries; README (#4) | R-MET-02 | 3 | **Done** (S6-30) |
| GP.d | *(debt)* Rate limits for sign-in and organisation creation (#318, TD-60) 1; reload after a failed lazy chunk and after `release-mismatch` (#296, #310) 1; release pipeline running `check-release-config.mjs` with the O-6 channel and the legal pages (TD-29) 1; licence review of the base images' OS packages and the BIT inklusiv permission (TD-44, TD-53) 1 | TECH_DEBT_v5 §9.4 | 4 | 2 **done** (S6-26, S6-27); 2 in sprint 07 (S7-14, S7-15) |
| GP.e | *(debt, new in v7)* Workspace correctness: the copy of an archived project refused on the server, lock and copy atomic (TD-86, TD-87; O-30, O-31) 2; a revoked platform role ends in running sessions (#450) and another user signing in in another tab (TD-63) 1.5; S12 reload after `release-mismatch` (#454) 0.5; one 🔧 time zone for dates (TD-47) and what `.contrast-more` shows until H2 (TD-69) 1 | TECH_DEBT_v6 §9.2 | 5 | Sprint 07 (S7-08, S7-12, S7-13, S7-16) |
| | **Sum** | | **31.5** (packages 21 + debt 10.5) | **10 done**; 21.5 in sprint 07 |

**Gate criteria (SPEC §12):** O-S5 done; legal page content supplied by WERTE.IT; provisional texts of O-14 and O-16 reviewed or knowingly kept; no open issue labelled `a11y` + `blocker`. H2 (high contrast) is a target, not a condition; what `.contrast-more` shows until then is decided in sprint 07 (TD-69, O-S7-9).
**Re-estimate after sprint 06 (+6.5 points, +26 %):** writing the operations guide (S6-30) and the backup (S6-29) showed what a deployment still lacks (#447 – #450, TD-92); the workspace added two correctness items the owner wants decided before a pilot (O-30, O-31).
**What no sprint can finish alone:** the first staging deployment (TD-28 points (1) – (15), owner), the Figma dark mode and its review (O-S5), the legal texts (WERTE.IT), the tester's pass (TD-49), the BIT inklusiv permission (TD-53). The 7 points of S7-18 and S7-19 start only when their input is there; if it is late they move to sprint 08 and the sprint pulls its M6 stretch instead.
**Proposal (O-S6-8, still open):** add "release 2026.1 active" to the gate criteria. Without it a partner works with the legacy filter `/filters`, not with S10/S11, and a project copy can only come from the test release (D1). The preview used for the project meeting of 2026-11-20 (all criteria provisionally „B“, local only) showed the flow works with the 2026.1 texts.

---

## 4. Later

### M6 — Directory, publication and contribution

**Goal:** shared product knowledge: the public directory with both channels, publication from projects (Flow B), experience reports (Flow D) and vendor/auditor test results (Flow E).

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| M6.0d | *(debt)* Criterion splits: several successors instead of one `SupersededByKey`, the carry-over of saved lists follows (TD-31, #238) — before cross-release matching (CAT-03/04) | TECH_DEBT_v6 §3.3 | 2 | Sprint 07 (S7-20), after O-S7-10 |
| M6.1 | Directory core: `SoftwareProduct`, `PublishedEntry` with immutable versions, `PublishedEntryFactory`, immutability interceptor, product-identity resolver (trigram), redaction, cross-release matching | ARCH §5.5, §12 step 6 | 13 | Sprint 07 (S7-21 – S7-23) |
| M6.2 | Publication from a project (Flow B): S35 stepper, blocked states, republish under the same profile, withdraw S36, changed-since hint | R-PUB-01 – R-PUB-09 | 13 | Sprint 08 |
| M6.3 | Contributions (Flow D): contributor profiles S50, experience report S51 with optional assessment, product request S52, my contributions S53, withdrawal with content purge | R-CTR-01 – R-CTR-06 | 13 | Sprint 08 |
| M6.3d | *(debt)* Account deletion and export for step 6: contributor profiles detached, entries withdrawn, the contributor link removed, the export section `contributions`; display names normalised (trim, NFC) | TECH_DEBT_v6 §5 | 2 | Sprint 08 |
| M6.4 | Vendor and auditor publication (Flow E): S55 five-step stepper, consent, preview, republish | R-VEN-01 – R-VEN-07 | 13 | Sprint 08 |
| M6.5 | Directory search S40: search, filters in layer order, URL state, sort, paging, empty state | R-DIR-01 – R-DIR-07 | 8 | Sprint 07 (S7-25, S7-26) |
| M6.5d | *(debt)* Directory entries in the development and E2E seeder, through the factory; an `E2E` environment if the seeder needs it (S5-14 P5) | TECH_DEBT_v6 §5 | 2 | Sprint 07 (S7-24) |
| M6.6 | Product page S41 and entry detail S42: two channels, K1–K4, compare with a list, sort, experience reports, staleness note, context line, copy link | R-ENT-01 – R-ENT-10 | 13 | Sprint 07: read projections (S7-27) 5; S41 and S42 (S7-28, S7-29) 8 as stretch, otherwise sprint 08 |
| M6.6d | *(debt)* The legacy directory goes with its replacements: `withProducts()` / `withFavorites()`, `ProductsController` (the ARC-05 exception), the legacy product screens and their snackbar and `LiveAnnouncer` calls, #170, #82, the legacy TODOs | TECH_DEBT_v6 §5, §9.3 | 3 | Sprint 08 (after S41 replaces the last legacy screen) |
| M6.7 | „Merken“: add a directory product to a project; the name-match hint and „Aus dem Produktverzeichnis übernehmen“ of the workspace | R-CAN-01, R-ENT-11 | 3 | Sprint 08 |
| M6.8 | Product-identity administration S81 for `PlatformAdmin` | R-ADM-02 | 5 | Sprint 08 |
| M6.9 | Contract tests for the two-signal projections and privacy rules | TESTING §4 (SIG-*, PRV-*) | 5 | With each package; the read-side contracts in sprint 07 (S7-27) |
| M6.9d | *(debt)* Deferred invariant parts of step 6: CAT-15/CAT-17 directory parts, TEN-04 directory part, API-05 and API-06 for the directory, TEN-06 for `CatalogEditor` and `PlatformAdmin`, the untagged SIG-08 and PRV-02 client cases | TECH_DEBT_v6 §5 | 1 | With the package that touches each |
| | **Sum** | | **96** (packages 86 + debt 10) | 30 in sprint 07 (+ 8 stretch) |

**Re-estimate at the first M6 sprint planning (sprint 07):** the packages keep their sizes; the debt that `TECH_DEBT_v6.md` §5 binds to step 6 is now counted as *(debt)* rows (+10 points, +12 %).
**Order inside M6:** M6.0d and M6.1 first; then M6.5/M6.6 (read side, shown with seeded entries that go through the factory) in parallel with the three write paths M6.2–M6.4. The milestone is only finished when all three write paths go through the factory (ARCH §12).
**Pages:** S40, S41, S42 (sprint 07 and stretch); S35, S36, S50 – S53, S55, S81 (sprint 08). Figma frames exist for S35, S40, S41, S42, S51, S53 and S55.
**Exit criteria:** journeys J2 (publication), J3 (experience report) and J7 (vendor) pass; directory invariants active.

### G-R — Public release (gate)

**Goal:** the tool is publicly available.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| GR.1 | Incorporate the bundled legal review: ⚖️ texts, purge timing, account deletion rules, export legal note | O-4 | 5 |
| GR.2 | Production environment, accessibility statement, final A11Y-07 pass, real criteria-suggestion channel | O-6, SPEC §11 (release prerequisites) | 3 |
| | **Sum** | | **8** |

Confidence *low*: the size of GR.1 depends on what the legal review requires.

### M7 — Moderation and P1

**Goal:** the P1 features of PRD §9. Detailed in SPEC_v3 at the start of sprint planning for this milestone; the estimate will be redone then.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M7.1 | Report an entry and moderation (restrict, withdraw, reinstate, append-only log) | R-P1-01 | 13 |
| M7.2 | Counter-statements by vendor/auditor profiles | R-P1-02 | 5 |
| M7.3 | Conformance-report upload (VPAT/ACR) in S55, served from a separate origin | R-P1-03, R-VEN-08 | 8 |
| M7.4 | WERTE.IT advice note | R-P1-04 | 1 |
| M7.5 | Links to external product directories | R-P1-05 | 2 |
| M7.6 | „Hoher Kontrast“ setting | R-P1-06, R-SET-04, H2 | 3 |
| | **Sum** | | **32** |

### Beyond M7 (P2, not estimated)

Rating models, tender-text generator, market overview, import, further use cases and standards (PRD §9 P2). The architecture keeps them possible (`ARCHITECTURE §15`); they get a roadmap entry only after the pilot.

---

## 5. Risks and dependencies

| # | Dependency or risk | Affects | Owner | Mitigation |
|---|---|---|---|---|
| D1 | **Priorities, rationales and plain-language titles** per criterion (O-1, K-1) — *still open (2026-11-24); blocks M2.3, the activation of 2026.1 and the removal of the legacy filter (GitHub milestone "Legacy removal (2026.1)": #52, #158); the gap list v3 is the work list. One catalog-default level with a rationale per criterion is enough to activate; deviations per app type or use case are optional. A pilot of the workspace needs it (proposal O-S6-8)* | M2.3, G-P | Project team | Collect the catalog-default priorities first, criteria of the first pilot's use case first; tasks S7-30 and S7-31 are ready for the day they arrive |
| D2 | **Dark mode in Figma and accessibility review** (O-S5) — *not there yet; GP.4 (S7-19) starts only with it* | GP.4 | Philipp, accessibility tester | Generated palette until then; if it is late, GP.4 moves to sprint 08 and does not block M6 |
| D3 | **Legal review** (O-4): consent texts, purge timing, deletion rules, vendor authenticity | G-R; possibly M6.3/M6.4 if requirements change | Legal review | ⚖️ texts as placeholders; purge timing configurable; a change may require SPEC_v3 before M6 is finished |
| D4 | **Legal page content** (imprint, privacy, accessibility statement) | GP.3 | WERTE.IT | Placeholder check in the production build |
| D5 | **Criteria-suggestion channel** (O-6) | G-R | Project team | Configurable placeholder; build check |
| D6 | **Provisional texts** for priority terms (O-14) and rating levels (O-16) | G-P | Project team | Texts live only in the UX copy / catalog data and can change without code |
| D7 | ~~**Platform upgrade** (.NET 10, Angular 22, zoneless)~~ | M0 | Philipp | **Resolved** in sprint 01 |
| D8 | **Lettermint** sending domain (SPF/DKIM) and project per environment | M3 on staging, GP.1 | Philipp | Mailpit for development and E2E |
| D9 | **Single developer** (bus factor, handover after WERTE.IT) | all | Philipp | `CLAUDE.md`, `TECH_DEBT_v*.md`, `TECHNICAL.md` (GP.6), invariant-tagged tests, decisions in ARCHITECTURE §16; handover platform O-7 open |
| D10 | **Size of M6** | M6 | Philipp | Split into packages; read side first with seeded data; factory before any write path |
| D11 | **Debt riding with migration steps** is larger than v1 assumed (M1: +11 points; M2: +10 points) | M1, M2, M3 | Philipp | Counted per milestone as *(debt)* rows, estimated at the milestone's first sprint planning; 15–20 % of each sprint for general debt; register reviewed at the end of every sprint (process step 14) |
| D12 | ~~**Scope of the module move** in M1.1~~ | M1.1 | Philipp | **Resolved:** O-S2-1 (skeleton plus Platform; legacy projects stay until their step), ADR-014 (one `DbContext` and migration history per module), ARCHITECTURE_v15 |
| D13 | ~~**UX copy gaps** (#181)~~ | M2.6 | Philipp | **Resolved:** UX copy v17 ff. with the owner's approval (S4-02) |
| D14 | ~~**AutoMapper replacement** (#31)~~ | M1.1d | Philipp | **Resolved:** ADR-015 (hand-written maps), S3-07 |
| D15 | **No active catalog release outside the tests** until 2026.1 has priorities (O-S3-5 Q2 (iii)); dev, E2E and staging answer 404 on `/api/v1/catalog/*` (#241) | M2.5 – M2.7, staging demos | Philipp | Test release `T.1` in E2E (and development); legacy filter stays reachable until 2026.1 is active (O-S4-3) |
| D16 | ~~**CI reliability** (#203)~~ | every sprint | Philipp | **Resolved:** Playwright container image (S4-01, S4-01b); runner failures outside the workflow remain a watch item (TD-52) |
| D17 | **Staging lags far behind `main`** — *the top risk now (TECH_DEBT_v6 §1): 15 preconditions (TD-28, #236), among them the key-ring certificate, SMTP credentials, never `Development`, the public-address settings (#447), closed ports (#449), the health route (#448), the backup profile (TD-92), and a proxy that does not compress `/api/` (strong ETags)* | GP.1, every demo, the pilot | Philipp | Sprint 07 closes the repository part first (S7-09), then deploys from CI with the owner's checklist (S7-10) and rehearses the restore there (S7-11) |
| D18 | ~~**M3 replaces the authentication** of the whole application in one step (JWT → cookie session)~~ | M3 | Philipp | **Resolved** in sprint 05: server first, then client; legacy auth removed (S5-10) |
| D19 | ~~**The users are still legacy** (TD-72)~~ | M5 | Philipp | **Resolved** in sprint 06 (S6-10a, S6-10b; #387) |
| D20 | **Local E2E unreliable** for lack of disk space (TD-74: 1.1 GB free, 97 % used); three E2E failures on `main` without a named cause (#451, #452) | every sprint | Philipp | Free disk space; S7-01 first; one local E2E stack at a time, the other sessions leave E2E to CI |
| D21 | **Owner decisions pile up:** eight open architecture questions (O-25 – O-32), ten UX copy keys (TD-79), 17 screens without a Figma frame (TD-80) | sprint 07 tasks that wait for a shape, a key or a design (#414, #420, #454, TD-86, TD-87) | Philipp | One decision round at the sprint start (O-S7-1 – O-S7-12, each with a proposal) |
| D22 | **M6 must land all three write paths through one factory** and is the first public, cross-organisation data (two-signal and privacy invariants) | M6, G-R | Philipp | Core and factory first with their invariants (sprint 07); the read side on seeded entries; the write paths together in sprint 08; the strongest model for the factory, redaction and the projections |

---

## 6. Capacity and forecasting

- The points are **relative**. They become a time forecast only once a velocity is known: after the first two or three sprints of M0/M1, divide the completed points by the sprints used and project the remaining sums of the overview table. Record the velocity in the sprint notes (step 11), not in this roadmap.
- As a solo project with ongoing documentation and coordination work, plan sprints with a buffer for unplanned work (e.g. review findings, upgrade issues) rather than filling them completely.
- Re-estimate a milestone at the start of its sprint planning; update this roadmap only when a sum changes by more than about 20 % or the order changes.

### Measured so far

| Sprint | Planned (P0 + stretch) | Done (roadmap points) | Debt and other unplanned work | Notes |
|---|---:|---:|---|---|
| 01 | 28 + 4 | **34** (M0 28, M1.2 3, M1.7 1, M1.8 2) | ≈ 3 points (dependency, documentation and key PRs of the register review) | Development days used were not recorded |
| 02 | 28.5 + 3.5 | **32** (M1 28 incl. its debt rows, general debt 4.5) | small follow-ups #140 – #142 | All 23 tasks incl. stretch in **about 4 development days** (owner, 2026-11-05) → ≈ 8 points per day |
| 03 | 42.5 + 5.5 (after O-S3-5) | **48** (M1 10, M2 29.5, general debt 8.5) | — | All 28 protocols incl. stretch in **about 3 development days** (owner, 2026-11-10) → ≈ 16 points per day |
| 04 | 50 + 6.5 (incl. S4-29) | **≈ 57** (M2 43.5, M3.4 2, general debt ≈ 11) | splits S4-15, S4-18, S4-20, S4-21; S4-14b, S4-30 | All tasks incl. stretch in **about 3 development days** (owner, 2026-11-13) → ≈ 19 points per day |
| 05 | 68.5 + 10 | **78.5** (M3 65, M4 8, M2 follow-ups 2, general debt 3.5) | splits S5-12, S5-13, S5-16, S5-17, S5-20, S5-21, S5-25, S5-28; 40 protocols | All tasks incl. stretch in **about 5 development days** (commits on five calendar days, 2026-11-13 – 2026-11-17; owner to confirm) → ≈ 16 points per day |
| 06 | 60 + 6 | **≈ 64.5** (M5 47, G-P 10, general debt 6, extras ≈ 1.5: #434, the dev-stack mail fix #430, the J2 race #431) | splits S6-10, S6-12, S6-19, S6-20, S6-21, S6-22; 36 protocols; 3 M5 points not done (#414, #420, #429) | All tasks incl. stretch in **about 5 development days** (commits on 2026-11-18 – 20, 23, 24; estimate from the git history, owner to confirm) → ≈ 13 points per day |
| 07 | 58.5 + 8 | | | Plan: `docs/sprints/sprint_07/SPRINT_07.md` (capacity assumption ~90; 7 points wait for outside input) |

**Forecast (six sprints, not a commitment):** sprint 03 gave ≈ 16, sprint 04 ≈ 19, sprint 05 ≈ 16 and sprint 06 ≈ 13 points per development day; sprints 05 and 06 each delivered 65 – 80 points in about five development days. The forecast counts sprints of 60 – 75 delivered points. M5, G-P and M6 are estimated with their debt rows; G-R and M7 still get +15 %:

| Target | Open points after sprint 06 | Done after sprint … |
|---|---:|---|
| **Pilot candidate** (gate G-P) | 24.5 (M5 3, G-P 21.5) | **7** for the development part. The gate itself opens when staging runs, the tester has passed, the dark mode is reviewed and the legal texts are in — sprint 07 if these arrive during it, otherwise early in sprint 08 |
| **Release candidate** (gate G-R) | ≈ 134.5 (+ M2.3 5, M6 96, G-R ≈ 9) | **8 – 9**: sprint 07 takes 30 – 38 points of M6, which leaves ≈ 58 – 66 of M6 plus G-R and M2.3 (≈ 75) — one full sprint if the legal review (O-4) and O-1/K-1 are there, otherwise a ninth |
| **Everything incl. P1** (M7) | ≈ 171.5 (+ M7 ≈ 37) | **9 – 10** (M7 needs SPEC_v3 first) |

The gates wait for inputs that no velocity can replace: for G-P the staging deployment (TD-28), O-S5 (Figma dark mode and its review), the legal page content, the tester's pass (A11Y-07) and — if O-S6-8 is accepted — O-1/K-1 for release 2026.1; for G-R in addition the bundled legal review O-4 and the O-6 channel. Revisit after sprint 07.

---

## 7. Coverage check

All P0 requirements of `SPEC_v2.md` are assigned to a work package above (checked by script against the requirement tables of SPEC_v2). Split across two packages: R-AUTH-05, R-AUTH-07, R-SET-03 and R-ADM-04 (base part and completion part, stated in the package) and R-ADM-01 (CLI in M2.2, release content in M2.3). The P1 requirements R-SET-04, R-VEN-08 and R-P1-01 – R-P1-06 are in M7.

---

## 8. Changes

| Version | Date | Change |
|---|---|---|
| v1 | 2026-10-19 | First version, from SPEC_v2 and ARCHITECTURE_v12 |
| v2 | 2026-10-29 | After sprint 01: M0 done; M1.2, M1.7, M1.8 done ahead; M1 re-estimated 33 → 44 with its debt rows (*(debt)*, TECH_DEBT_v1 §9.2) and a status column; debt riding with M2 listed; D7 resolved, D11, D12 added; measured velocity and tentative forecast in §6; references to ARCHITECTURE_v14, TESTING_v15, SPEC v2.1, CLAUDE.md |
| v3 | 2026-11-05 | After sprint 02: M1.1, M1.3, M1.4, M1.6 and their debt done (34 of 44 points); M1 rest and the start of M2 planned for sprint 03; M2 re-estimated 69 → 79 with *(debt)* rows (TECH_DEBT_v2 §9.2) and a "Planned" column; P0 total 363 → 373; D12 resolved, D13 (UX copy session), D14 (AutoMapper ADR) added; measured velocity of sprint 02 (≈ 8 points per development day) and a new forecast; references to ARCHITECTURE_v15, TESTING_v23, TECH_DEBT_v2 |
| v3.1 | 2026-11-06 | Owner decision O-S3-5 (sprint 03, S3-11): no copy of the existing criteria into the catalog (option C); M2.1 13 → 11, M2 79 → 77; the M2.3 texts (S3-27) are P0 in sprint 03 |
| v4 | 2026-11-10 | After sprint 03: M1 done; M2.1, M2.2, M2.4, M2.4d (#153, #176), M2.8 footer part, the texts of M2.3 and #115 done (29.5 of 78); M2.4d +1 point for API-06; M2.3 priorities marked blocked on O-1/K-1; the rest of M2 planned for sprint 04; overview totals corrected (v3.1 had not carried M2 77 into the P0 total); D1 and D13 updated, D14 resolved, D15 (no active release outside the tests) and D16 (CI reliability) added; sprint-03 velocity (≈ 16 per day) and a forecast per gate; references to SPEC v2.6, ARCHITECTURE_v27, TESTING_v39, TECH_DEBT_v3 |
| v5 | 2026-11-13 | After sprint 04: M2 done except the 2026.1 priorities (73 of 78; status, exit check and follow-ups #279, #299, #310, #278/#281/#282 noted); M3.4 done ahead (S4-25); M3 re-estimated 59 → 67 with *(debt)* rows (TECH_DEBT_v4) and planned in full for sprint 05; P0 total 372 → 380; D13 and D16 resolved, D17 (staging) and D18 (authentication replaced in one step) added; sprint-04 velocity (≈ 19 per day) and a new forecast per gate; references to the highest ARCHITECTURE, TESTING and TECH_DEBT_v4 |
| v6 | 2026-11-17 | After sprint 05: M3 done (67), M4 done as stretch (8); M5 re-estimated 40 → 50 with *(debt)* rows M5.0d (users into IdentityAccess, TD-72), M5.4d, M5.6d, M5.7d, M5.8d (TECH_DEBT_v5 §5) and a "Planned" column (all in sprint 06); G-P 21 → 25 with GP.d, its parts without outside input planned in sprint 06; P0 total 380 → 394, 220 done; D1, D2, D17 updated, D18 resolved, D19 (users still legacy) and D20 (local E2E, `main` red) added; proposal O-S6-8 (2026.1 active as a G-P criterion); sprint-05 velocity (≈ 16 per day) and a new forecast per gate (pilot after sprint 07); references to ARCHITECTURE_v80, TESTING_v88, TECH_DEBT_v5 |
| v7 | 2026-11-24 | After sprint 06: M5 done except 3 points (#414, #420, #429 → sprint 07); GP.2, GP.3, GP.6 and half of GP.d done; G-P re-estimated 25 → 31.5 with GP.2d (backup gaps, TD-92) and GP.e (workspace correctness O-30/O-31, #450, TD-63, #454, TD-47, TD-69) and a "Planned" column; M6 re-estimated 86 → 96 with *(debt)* rows M6.0d, M6.3d, M6.5d, M6.6d, M6.9d (TECH_DEBT_v6 §5) and planned across sprints 07 and 08; P0 total 394 → 410.5, 277 done; D1, D2, D17, D20 updated, D19 resolved, D21 (owner decisions) and D22 (M6 write paths) added; sprint-06 velocity (≈ 13 per day) and a new forecast per gate (pilot development part after sprint 07, release candidate after sprint 08 – 09); references to ARCHITECTURE_v107, TESTING_v114, TECH_DEBT_v6, TECHNICAL.md |

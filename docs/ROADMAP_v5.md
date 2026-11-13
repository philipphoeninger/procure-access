# ProcureAccess — Roadmap

> **Status:** v5 (2026-11-13) — update after sprint 04, created with the skill *Product Management/roadmap-update*: M2 done except the priorities of release 2026.1 (blocked on O-1/K-1); M3.4 done ahead; M3 re-estimated with its debt rows at its first sprint planning and planned in full for sprint 05; measured velocity of sprint 04 and a new forecast per gate. v4 (2026-11-10) was the update after sprint 03, v3/v3.1 after sprint 02, v2 after sprint 01, v1 (2026-10-19) the output of Phase 3, step 10 ("Create the roadmap") of `ai_development_process.md`. What changed: §8.
> **Based on:** `SPEC_v2.md` (feature areas, requirements `R-…`, mapping §11, phasing §12, open questions §13), the highest ARCHITECTURE (v46; module rules §4.3, §5.2, §9.1, migration path §12, ADR-014 – ADR-016), the highest TESTING (v55; invariants per step §9), `TECH_DEBT_v4.md` (debt per migration step §9), the sprint-04 close-out and protocols in `docs/sprints/sprint_04/`, `docs/sprints/sprint_05/SPRINT_05.md`.
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
| **M3** | Accounts, organisations, session, saved lists | Now | **67** (v4: 59) | medium | **Sprint 05** (M3.4 done ahead in S4-25) | M2 ✓; Lettermint sending domain for staging; staging deployment #236 |
| **M4** | Session warning and hardening | Next | 8 | high | Not Started | M3 |
| **M5** | Workspace: projects, candidates, assessment | Next | 40 | medium | Not Started | M3, M4 |
| **G-P** | Pilot readiness (gate) | Next | 21 | medium | Not Started | M5; O-S5, legal page content, O-14, O-16 |
| **M6** | Directory, publication, contribution | Later | 86 | medium | Not Started | M5 |
| **G-R** | Public release (gate) | Later | 8 | low | Not Started | M6; legal review O-4, O-6 |
| **M7** | Moderation and P1 | Later | 32 | low | Not Started | G-R; SPEC_v3 |
| | **Total P0 (M0–G-R)** | | **380** (v4: 372) | | 147 done, **233 open** | |
| | **Total incl. P1 (M7)** | | **412** (v4: 404) | | | |

**Relative weight:** M2, M3 and M6 together are about 60 % of the P0 effort. M6 is the largest single milestone because all three write paths into the directory must land together (`ARCHITECTURE §12`, step 6); it is split into packages, not into milestones.

```
Done:   M0 ✓ ──► M1 ✓ ──► M2 ✓ (except the 2026.1 priorities)
Now:                         └─► M3
Next:                               └─► M4 ──► M5 ──► G-P (pilot)
Later:                                          └─► M6 ──► G-R (public release) ──► M7 (P1)
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

**Goal:** people can register, work in organisations and save, edit and share criteria lists, on the new cookie session model.

| # | Work package | Requirements / sources | Points | Planned |
|---|---|---|---:|---|
| M3.1 | Identity cookie authentication with antiforgery replaces the JWT; session probe, login outlet with draft kept, 401 → login overlay, sign-out | R-AUTH-03, R-AUTH-05, R-AUTH-07 (base), R-AUTH-08 | 8 | Sprint 05 (S5-07 – S5-09) |
| M3.1d | *(debt)* Legacy auth goes with the JWT: `Identity.UI`, `{ code, message }` failures, `DateTime.UtcNow` in `UserService`/`UserRepo`, `ERROR_TRANSLATIONS`, antiforgery on `criteria-list-shown`, `signedIn` from the session, the auth entries of `LEGACY_EXCEPTIONS`; test hosts #87 – #89 | TECH_DEBT_v4 §5, §9.1 | 3 | Sprint 05 (S5-07, S5-10) |
| M3.2 | Registration, e-mail confirmation gate, password reset, lockout; `IEmailSender` with Mailpit locally and Lettermint templates (`email.*`) | R-AUTH-01, R-AUTH-02, R-AUTH-04, SPEC §7.6 | 8 | Sprint 05 (S5-11 – S5-13) |
| M3.2d | *(debt)* Mails to the real recipient (#302); registration without the missing `Member` row (#301); Mailpit and real secrets in the E2E stack incl. the `compose.log` check (TD-12); E2E users in the seeder | TECH_DEBT_v4 §3.1, §9.1, §9.2 | 2 | Sprint 05 (S5-11, S5-12, S5-14) |
| M3.3 | Organisations: create, invitations by e-mail, members page, roles, leave, last-Owner rule, authorisation matrix, query filters; header states and organisation switch | R-ORG-01 – R-ORG-06, R-SHL-02, R-SHL-03 | 13 | Sprint 05 (S5-15 – S5-18) |
| M3.4 | Contributor-profile and moderator groundwork: entities `ContributorProfile`, `UserContributorLink`; rename `Approver` → `Moderator`; platform-role CLI | ARCH §12 step 3, R-ADM-03 | 2 | **Done** (S4-25; TEN-06 active for its part) |
| M3.5 | Saved lists: configurator entities, save S12, list S14, edit and „Auswahl ändern“, delete, saved export, older-release notice and „Neue Liste mit derselben Auswahl“ | R-SAV-01, R-SAV-02, R-SAV-03, R-SAV-06, R-LST-14 | 13 | Sprint 05 (S5-19 – S5-21) |
| M3.5d | *(debt)* The weight range served by the server (TD-45); a too-long note leads to the note (TD-40); `criteria.list.unsaved` and the draft-note wording; TEN-04 workspace part | TECH_DEBT_v4 §9.2 | 1.5 | Sprint 05 (S5-22) |
| M3.6 | Share links S15 with required expiry and shared list S16 without account | R-SAV-04, R-SAV-05 | 5 | Sprint 05 (S5-23, S5-24) |
| M3.7 | Settings S60 (language, appearance, motion, contrast „wie im System“), stored per user | R-SET-01 | 5 | Sprint 05 (S5-25, S5-26) |
| M3.7d | *(debt)* The legacy settings store goes (storage for signed-out users, English snackbar, crash on blocked storage — #234 TD-25); A11Y-08 with the explicit, persisted setting | TECH_DEBT_v4 §3.1, §9.2 | 1.5 | Sprint 05 (S5-25) |
| M3.8 | Personal data export (JSON) and account deletion | R-AUTH-09, R-AUTH-10 | 5 | Sprint 05 (S5-27, S5-28) |
| | **Sum** | | **67** (packages 59 + debt 8) | 2 done; 65 in sprint 05 |

**Re-estimate at the first M3 sprint planning (sprint 05):** the packages keep their sizes; the debt that `TECH_DEBT_v4.md` binds to step 3 is now counted as *(debt)* rows (+8 points, +14 %).
**Pages:** M3 builds S01 – S03 (sign-in, registration, password reset) with the login overlay, S12, S14, S15, S16, S20, S21, S60, S61 and the signed-in header states. None of them has a Figma frame; they follow the patterns of Handoff §5.3 (owner, 2026-11-13).

**Exit criteria:** J1 second part (save after sign-in) passes; tenant-isolation invariants active.
**Dependency:** Lettermint sending domain with SPF/DKIM for staging (operations; not needed for local work with Mailpit).

### M4 — Session warning and hardening

**Goal:** sessions end safely and predictably; users are warned before expiry.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M4.1 | Absolute limit, sliding renewal, expiry headers, extend endpoint, security stamp on password change and account deletion | R-AUTH-05 (completion), SEC-07, SEC-08 | 5 |
| M4.2 | Session warning dialog; login overlay completed | R-AUTH-06, R-AUTH-07 (completion) | 3 |
| | **Sum** | | **8** |

### M5 — Workspace

**Goal:** organisations run procurement projects: candidate products, test evidence and per-criterion assessment against the project's own copy of a criteria list.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M5.1 | Projects S30, S31, S32: list, create, overview, status; project copy of the criteria list with lock; adjusting the copy via S11 | R-PRJ-01 – R-PRJ-06 | 13 |
| M5.2 | Candidates S33, S34: create/edit with private section, tabs as routes, overview, tri-state test status, evidence list S34a | R-CAN-02 – R-CAN-05 | 8 |
| M5.3 | Assessment S34b: K6 with allowed values, autosave, derived „Geprüft nach …“ rows, progress, filter, rating legend | R-CAN-06 – R-CAN-08 | 8 |
| M5.4 | Concurrency: optimistic locking, conflict dialog S37, last-write-wins for autosaved assessments | R-PRJ-07, R-CAN-11 | 5 |
| M5.5 | Remove a candidate; Readers see everything read-only | R-CAN-09, R-CAN-10 | 3 |
| M5.6 | Retire the proposal workflow; repoint `withProducts()` / `withFavorites()` | ARCH §8.1, §12 step 5 | 3 |
| | **Sum** | | **40** |

**Exit criteria:** J2 up to the assessment passes; workspace invariants active.
**Note:** „Merken“ from the directory (R-CAN-01) needs the directory and is part of M6.

### G-P — Pilot readiness (gate)

**Goal:** a cooperation partner can use M0–M5 on a stable staging environment (PRD goal 4).

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| GP.1 | Staging environment: separate database, deployment from CI, Lettermint project and API token | ARCH §10.2 | 5 |
| GP.2 | Nightly backup and documented restore rehearsal | R-ADM-04 (completion) | 3 |
| GP.3 | Legal pages from Markdown files; production build fails on placeholders | R-SHL-07 | 2 |
| GP.4 | Dark theme release: pin the reviewed values after the Figma dark mode and the accessibility review | R-SET-03 (release), O-S5 | 3 |
| GP.5 | Manual accessibility pass (A11Y-07) and fixes | TESTING §7.2 | 5 |
| GP.6 | `TECHNICAL.md` (operations, onboarding) with the documented metric queries | R-MET-02 | 3 |
| | **Sum** | | **21** |

**Gate criteria (SPEC §12):** O-S5 done; legal page content supplied by WERTE.IT; provisional texts of O-14 and O-16 reviewed or knowingly kept; no open issue labelled `a11y` + `blocker`. H2 (high contrast) is a target, not a condition.

---

## 4. Later

### M6 — Directory, publication and contribution

**Goal:** shared product knowledge: the public directory with both channels, publication from projects (Flow B), experience reports (Flow D) and vendor/auditor test results (Flow E).

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M6.1 | Directory core: `SoftwareProduct`, `PublishedEntry` with immutable versions, `PublishedEntryFactory`, immutability interceptor, product-identity resolver (trigram), redaction, cross-release matching | ARCH §5.5, §12 step 6 | 13 |
| M6.2 | Publication from a project (Flow B): S35 stepper, blocked states, republish under the same profile, withdraw S36, changed-since hint | R-PUB-01 – R-PUB-09 | 13 |
| M6.3 | Contributions (Flow D): contributor profiles S50, experience report S51 with optional assessment, product request S52, my contributions S53, withdrawal with content purge | R-CTR-01 – R-CTR-06 | 13 |
| M6.4 | Vendor and auditor publication (Flow E): S55 five-step stepper, consent, preview, republish | R-VEN-01 – R-VEN-07 | 13 |
| M6.5 | Directory search S40: search, filters in layer order, URL state, sort, paging, empty state | R-DIR-01 – R-DIR-07 | 8 |
| M6.6 | Product page S41 and entry detail S42: two channels, K1–K4, compare with a list, sort, experience reports, staleness note, context line, copy link | R-ENT-01 – R-ENT-10 | 13 |
| M6.7 | „Merken“: add a directory product to a project | R-CAN-01, R-ENT-11 | 3 |
| M6.8 | Product-identity administration S81 for `PlatformAdmin` | R-ADM-02 | 5 |
| M6.9 | Contract tests for the two-signal projections and privacy rules | TESTING §4 (SIG-*, PRV-*) | 5 |
| | **Sum** | | **86** |

**Order inside M6:** M6.1 first; then M6.5/M6.6 (read side, can be shown with seeded entries) in parallel with the three write paths M6.2–M6.4. The milestone is only finished when all three write paths go through the factory (ARCH §12).
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
| D1 | **Priorities, rationales and plain-language titles** per criterion (O-1, K-1) — *still open (2026-11-13, owner: "unclear"); blocks M2.3, the activation of 2026.1 and the removal of the legacy filter; the gap list v3 is the work list* | M2.3, early partner preview | Project team | Start M2 with a reduced test catalog; review the data folder in pull requests; prioritise the criteria of the first pilot's use case |
| D2 | **Dark mode in Figma and accessibility review** (O-S5) | GP.4 | Philipp, accessibility tester | Develop with the generated palette from M1; schedule the review before G-P |
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
| D17 | **Staging lags behind `main`** and the next deployment has preconditions (key-ring certificate, plain key rows, log search for the token-less reset, closed API port, environment variable; #236) — M3 adds the cookie session and real e-mail | M3 on staging, every demo | Philipp | Checklist on #236; deploy before M3 is shown on staging |
| D18 | **M3 replaces the authentication** of the whole application in one step (JWT → cookie session) | M3 and everything after it | Philipp | Server first with its invariants (SEC-01/02/03/09, OPS-04), then the client; the legacy account pages go only after the new screens work; the strongest model for these tasks |

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
| 05 | 68.5 + 10 | | | Plan: `docs/sprints/sprint_05/SPRINT_05.md` (capacity assumption ~90) |

**Forecast (four sprints, not a commitment):** sprint 02 gave ≈ 8, sprint 03 ≈ 16 and sprint 04 ≈ 19 points per development day. Sprints end when their scope is done (after 3 – 4 development days so far), so the forecast counts sprints of the planned size. Sprint 05 is planned with **~90 points** of capacity for 8 days (≈ 11 per day, discounted because it replaces the authentication). With 70 – 90 points per sprint and +15 % for the debt that M4 – M7 will add:

| Target | Open points (with +15 % from M4 on) | Done after sprint … |
|---|---:|---|
| **Pilot candidate** (gate G-P) | 139 – 149 | **6 – 7** |
| **Release candidate** (gate G-R) | 233 – 257 | **7 – 8** |
| **Everything incl. P1** (M7) | 265 – 294 | **7 – 9** |

Both gates also wait for inputs that no velocity can replace: O-1/K-1 (priorities of 2026.1), O-S5 (Figma dark mode and its review), the legal page content, and for G-R the bundled legal review O-4. Revisit after sprint 04.

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

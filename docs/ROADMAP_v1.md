# ProcureAccess — Roadmap

> **Status:** v1 (2026-10-19) — output of Phase 3, step 10 ("Create the roadmap") of `ai_development_process.md`, created with the skill *Product Management/roadmap-update*.
> **Based on:** `SPEC_v2.md` (feature areas FA-01–FA-18, requirements `R-…`, mapping §11, phasing §12, open questions §13), `ARCHITECTURE_v12.md` (migration path §12, environments §10), `TESTING_v10.md` (invariants per step §9).
> **Language:** English (owner decision, 2026-10-19).
> **Input for:** sprint planning (step 11). Each sprint takes work packages from the current milestone; a work package is the unit that is sliced into tasks.

---

## How to read this roadmap

- **No dates.** There is no external deadline and no pilot date (SPEC SD18). The roadmap fixes *order* and *relative size*, not time. Forecasts in time can be derived later from the measured velocity (see §6).
- **Milestones follow the migration steps** (`ARCHITECTURE §12`, SPEC §11): M0–M7, plus two **gates** that are not migration steps but release conditions: **G-P** (pilot readiness) and **G-R** (public release). A working application exists at the end of every milestone.
- **Grouping:** *Now* (committed, no open decisions), *Next* (planned, scope clear, some external input needed), *Later* (directional; scope may change, P1 is only outlined in SPEC).
- **Every requirement of SPEC_v2 is assigned to a work package** (§7 checks this). Invariants are not repeated here: the invariants a milestone activates are listed in SPEC §11 and TESTING §9.

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
| **M0** | Platform upgrade and test foundation | Now | 28 | medium | Not Started | – |
| **M1** | Foundations: modules, shell, theme, errors | Now | 33 | high | Not Started | M0 |
| **M2** | Catalog versioning, configurator, anonymous export | Next | 69 | medium | Not Started | M1; catalog content O-1, K-1 |
| **M3** | Accounts, organisations, session, saved lists | Next | 59 | medium | Not Started | M2; Lettermint domain set-up |
| **M4** | Session warning and hardening | Next | 8 | high | Not Started | M3 |
| **M5** | Workspace: projects, candidates, assessment | Next | 40 | medium | Not Started | M3, M4 |
| **G-P** | Pilot readiness (gate) | Next | 21 | medium | Not Started | M5; O-S5, legal page content, O-14, O-16 |
| **M6** | Directory, publication, contribution | Later | 86 | medium | Not Started | M5 |
| **G-R** | Public release (gate) | Later | 8 | low | Not Started | M6; legal review O-4, O-6 |
| **M7** | Moderation and P1 | Later | 32 | low | Not Started | G-R; SPEC_v3 |
| | **Total P0 (M0–G-R)** | | **352** | | | |
| | **Total incl. P1 (M7)** | | **384** | | | |

**Relative weight:** M2, M3 and M6 together are about 60 % of the P0 effort. M6 is the largest single milestone because all three write paths into the directory must land together (`ARCHITECTURE §12`, step 6); it is split into packages, not into milestones.

```
Now:    M0 ──► M1
Next:          └─► M2 ──► M3 ──► M4 ──► M5 ──► G-P (pilot)
Later:                                          └─► M6 ──► G-R (public release) ──► M7 (P1)
```

The arrows are the only order that is fixed. Two things may be pulled forward: the **staging environment** (G-P) can be set up right after M2 for an early partner preview of the anonymous core flow (SPEC §12: "first pilot-ready milestone"), and **M4** is small enough to be done inside the last sprint of M3.

---

## 2. Now

### M0 — Platform upgrade and test foundation

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

**Goal:** module structure, application shell, theme (light and dark), error handling and texts are in place, so every feature from M2 on only adds content.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M1.1 | Module projects with `Contracts`, module registration, NetArchTest boundary rules | ARCH §4.3 | 8 |
| M1.2 | Operations basics: Serilog in its own schema with retention, `/health` endpoints, `RebuildDatabase` guard | R-ADM-04 (part), ARCH §9.5 | 3 |
| M1.3 | Problem Details middleware with field-keyed `errors`, problem types, client mapping to UX keys | SPEC §7.1, §8.6 | 3 |
| M1.4 | Application shell: landmarks, anonymous header, footer, start page, error pages, loading/empty/system states, page title and H1 focus with announcement, 320 px / 400 % zoom | R-SHL-01, R-SHL-04, R-SHL-05, R-SHL-06, R-SHL-08, R-SHL-09, R-SHL-10 | 8 |
| M1.5 | Theme foundation: tokens, `color-scheme: light dark`, appearance without account, reduced motion; dark theme in development with the generated palette | R-SET-02, R-SET-03 (development part), SPEC §7.5 | 5 |
| M1.6 | ngx-translate with `de.json` from the UX copy; pluralisation approach | UX-O9 | 3 |
| M1.7 | `UsageEvent` table and writer | R-MET-01 | 1 |
| M1.8 | CI browser matrix: Chromium, Firefox, WebKit, mobile viewports | SPEC §7.4 (SD19), TESTING V56 | 2 |
| | **Sum** | | **33** |

**Exit criteria:** shell passes axe in both appearances; architecture tests enforce module boundaries; UX-O9 decided.

---

## 3. Next

### M2 — Catalog versioning, configurator and anonymous export

**Goal:** the primary persona's complete flow without an account: select → prioritised, explained criteria → adjust → export as XLSX/CSV. **First point at which a cooperation partner can try the tool** (SPEC §12).

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M2.1 | Catalog domain model and migrations: releases, revisions, applicability, priorities, standards, procedure texts, app types, product parts, use cases, test types, exclusions; migration of existing criteria | ARCH §5.1, §12 step 2 | 13 |
| M2.2 | Release data format and CLI `catalog import` / `catalog activate`, atomic with full validation | R-ADM-01 | 8 |
| M2.3 | Release 2026.1 as data folder from Kriterienkatalog Teil A/B, BIT inklusiv texts, one procedure criterion per test type; priorities and plain-language texts from the project team | R-ADM-01, O-1, K-1 | 8 |
| M2.4 | Catalog endpoints: filter options with exclusions, criteria with resolved priority, rationale, allowed ratings, procedure texts | SPEC §8.1, §8.2 | 5 |
| M2.5 | Configurator S10: four steps, exclusions with reason, reset of lower layers, „Bisherige Auswahl“, in-memory draft | R-CON-01 – R-CON-10 | 13 |
| M2.6 | Criteria list S11 (anonymous part): cards and detail panel, K5 with rationale, override, include/exclude, notes, search and filter, „Geprüft nach …“, empty state, notices, action bar | R-LST-01 – R-LST-10 | 13 |
| M2.7 | Export: pipeline with `AdHocSelectionSource`, XLSX (ClosedXML) and CSV per SPEC §8.3, dialog S13, rate limit; `ExportCreated` and `CriteriaListShown` events | R-LST-11, R-LST-12, SPEC §9.1 | 8 |
| M2.8 | „Kriterium vorschlagen“ link with configurable channel | R-LST-13, R-SUG-01 | 1 |
| | **Sum** | | **69** |

**Exit criteria:** journey J1 (anonymous part) passes keyboard-only and with axe; an exported file passes the screen-reader check (TESTING §7.2).
**Dependency:** M2.3 cannot be finished without the priorities and plain-language titles from the project team (O-1, K-1). Development can start with a reduced test catalog; the data folder is the review artefact for the team.

### M3 — Accounts, organisations, session and saved lists

**Goal:** people can register, work in organisations and save, edit and share criteria lists, on the new cookie session model.

| # | Work package | Requirements / sources | Points |
|---|---|---|---:|
| M3.1 | Identity cookie authentication with antiforgery replaces the JWT; session probe, login outlet with draft kept, 401 → login overlay, sign-out | R-AUTH-03, R-AUTH-05, R-AUTH-07 (base), R-AUTH-08 | 8 |
| M3.2 | Registration, e-mail confirmation gate, password reset, lockout; `IEmailSender` with Mailpit locally and Lettermint templates (`email.*`) | R-AUTH-01, R-AUTH-02, R-AUTH-04, SPEC §7.6 | 8 |
| M3.3 | Organisations: create, invitations by e-mail, members page, roles, leave, last-Owner rule, authorisation matrix, query filters; header states and organisation switch | R-ORG-01 – R-ORG-06, R-SHL-02, R-SHL-03 | 13 |
| M3.4 | Contributor-profile and moderator groundwork: entities `ContributorProfile`, `UserContributorLink`; rename `Approver` → `Moderator`; platform-role CLI | ARCH §12 step 3, R-ADM-03 | 2 |
| M3.5 | Saved lists: configurator entities, save S12, list S14, edit and „Auswahl ändern“, delete, saved export, older-release notice and „Neue Liste mit derselben Auswahl“ | R-SAV-01, R-SAV-02, R-SAV-03, R-SAV-06, R-LST-14 | 13 |
| M3.6 | Share links S15 with required expiry and shared list S16 without account | R-SAV-04, R-SAV-05 | 5 |
| M3.7 | Settings S60 (language, appearance, motion, contrast „wie im System“), stored per user | R-SET-01 | 5 |
| M3.8 | Personal data export (JSON) and account deletion | R-AUTH-09, R-AUTH-10 | 5 |
| | **Sum** | | **59** |

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
| D1 | **Priorities, rationales and plain-language titles** per criterion (O-1, K-1) | M2.3, early partner preview | Project team | Start M2 with a reduced test catalog; review the data folder in pull requests; prioritise the criteria of the first pilot's use case |
| D2 | **Dark mode in Figma and accessibility review** (O-S5) | GP.4 | Philipp, accessibility tester | Develop with the generated palette from M1; schedule the review before G-P |
| D3 | **Legal review** (O-4): consent texts, purge timing, deletion rules, vendor authenticity | G-R; possibly M6.3/M6.4 if requirements change | Legal review | ⚖️ texts as placeholders; purge timing configurable; a change may require SPEC_v3 before M6 is finished |
| D4 | **Legal page content** (imprint, privacy, accessibility statement) | GP.3 | WERTE.IT | Placeholder check in the production build |
| D5 | **Criteria-suggestion channel** (O-6) | G-R | Project team | Configurable placeholder; build check |
| D6 | **Provisional texts** for priority terms (O-14) and rating levels (O-16) | G-P | Project team | Texts live only in the UX copy / catalog data and can change without code |
| D7 | **Platform upgrade** (.NET 10, Angular 22, zoneless) | M0 | Philipp | One major version at a time; tests only after the upgrade |
| D8 | **Lettermint** sending domain (SPF/DKIM) and project per environment | M3 on staging, GP.1 | Philipp | Mailpit for development and E2E |
| D9 | **Single developer** (bus factor, handover after WERTE.IT) | all | Philipp | `CLAUDE_v3.md`, `TECHNICAL.md` (GP.6), invariant-tagged tests, decisions in ARCHITECTURE §16; handover platform O-7 open |
| D10 | **Size of M6** | M6 | Philipp | Split into packages; read side first with seeded data; factory before any write path |

---

## 6. Capacity and forecasting

- The points are **relative**. They become a time forecast only once a velocity is known: after the first two or three sprints of M0/M1, divide the completed points by the sprints used and project the remaining sums of the overview table. Record the velocity in the sprint notes (step 11), not in this roadmap.
- As a solo project with ongoing documentation and coordination work, plan sprints with a buffer for unplanned work (e.g. review findings, upgrade issues) rather than filling them completely.
- Re-estimate a milestone at the start of its sprint planning; update this roadmap only when a sum changes by more than about 20 % or the order changes.

---

## 7. Coverage check

All P0 requirements of `SPEC_v2.md` are assigned to a work package above (checked by script against the requirement tables of SPEC_v2). Split across two packages: R-AUTH-05, R-AUTH-07, R-SET-03 and R-ADM-04 (base part and completion part, stated in the package) and R-ADM-01 (CLI in M2.2, release content in M2.3). The P1 requirements R-SET-04, R-VEN-08 and R-P1-01 – R-P1-06 are in M7.

---

## 8. Changes

| Version | Date | Change |
|---|---|---|
| v1 | 2026-10-19 | First version, from SPEC_v2 and ARCHITECTURE_v12 |

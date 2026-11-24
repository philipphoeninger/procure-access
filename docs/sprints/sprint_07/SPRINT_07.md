# Sprint 07 — Pilot readiness (G-P) and the directory core (M6 start)

> **Status:** plan (2026-11-24) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v7.md` (M5 rest, G-P, M6), `TECH_DEBT_v6.md` (§3, §5, §6, §7, §8, §9.1 – §9.3), `TECHNICAL.md` (§3, §4, §6, §7), the highest versions of TESTING (§4.1 – §4.3, §4.9a, §6, §7.2, §9 step 6), ARCHITECTURE (§4.4, §5.5, §7.1, §7.2, §8.1, §10, §12 step 6, §16.5 O-25 – O-32), SPEC (R-PRJ-04, R-DIR-*, R-ENT-*, R-SET-03, §8.4, §12), the Design handoff (§4.9, §5.1, §5.2, §5.3), the Figma mapping (§2: S40, S41, S42), `CODEBASE_v1.md`, the sprint-06 close-out and protocols in `docs/sprints/sprint_06/`, the open GitHub issues and pull requests as of 2026-11-24.
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.
> **Task prompts:** `TASK_PROMPTS.md` in this folder (one ready-to-paste prompt per task, the running order in waves and the extra prompts for parallel work).
> **IDs:** owner decisions of this sprint are `O-S7-n` (none used before). `O-S6-8` keeps its name. They are not the SPEC open question **O-S5** (Figma dark mode) or the architecture questions `O-25` – `O-32`, which keep their names.

---

## Sprint 06 in one paragraph (carried in)

Every task (60 P0 + 6 stretch points, six of them split into a/b) was done in about **5 development days** (close-out in `SPRINT_06.md`). M5 is done as a development milestone: projects with their own copy of a criteria list, candidates with test status and evidence, the autosaved assessment, optimistic locking with the conflict dialog, Readers read-only, J2 up to the assessment; the users live in IdentityAccess and the proposals are retired. The backup with a rehearsed restore, the legal pages, `TECHNICAL.md` and the README exist. Carried in: two M5 requirements are not met — assigning or replacing a project's list on S32 (#420) and the assessments in the data export (#414) — and S11 of a copy has no path back (#429); three E2E failures on `main` have no named cause (#451, #452, #436); staging lags far behind and a deployment has 15 preconditions (TD-28, #236, #447 – #449); the backup has no off-host copy and no alert (#455); the owner has twelve decisions waiting (O-25 – O-32 and the keys and shapes below); the 2026.1 priorities (O-1/K-1) are still open, so the legacy filter stays (O-S4-3).

---

## Sprint goal

**The pilot candidate is ready on the development side: M5 is complete, a deployment from CI runs on staging with closed ports, correct public addresses, a health route and a rehearsed backup, and the workspace is correct under concurrent use — and the directory has its core: products, immutable published entries through one factory, identity resolution, search (S40) and the two-signal read projections.**

Two kinds of work run side by side. **G-P** is many small, mostly independent tasks, several of which wait for the owner or for people outside (staging server, tester, Figma dark mode, legal texts). **M6** starts with a server chain in one module that carries the product's central promises (two signals, privacy, immutability). The G-P tasks that wait for outside input are committed but **gated**: they start when the input is there and move to sprint 08 if it is not; the sprint then pulls its M6 stretch.

### Success criteria

- [ ] **`main` reliable:** the causes of #451, #452 and #436 named and fixed (or narrowed with a decision); three consecutive green runs.
- [ ] **M5 complete:** assessments in the data export (**PRV-07** workspace part complete, #414); assign and replace a list on S32 (R-PRJ-04 UI, #420); the path back from S11 of a copy, in `reflow.spec.ts` (#429, TD-91); #453 closed; **milestone M5 closed**.
- [ ] **Workspace correct before a pilot:** the copy of an archived project refused on the server (TD-86, O-30); lock and copy atomic (TD-87, O-31).
- [ ] **Deployable (GP.1):** ports on loopback, public-address settings passed, health route reachable (#449, #447, #448); staging deployed from CI on the current `main` with the checklist of #236 ticked; never `Development`.
- [ ] **Backup complete (GP.2d):** empty-dump check, alert, off-host copy, restore rehearsed on staging (#455).
- [ ] **Security and privacy small items:** a role grant or revoke takes effect at once (#450); another user signing in in another tab is handled (TD-63).
- [ ] **Release mechanics:** a release workflow runs `check-release-config.mjs` (TD-29); base-image OS packages in the notices (TD-44); S12 reload (#454); one time zone for dates (TD-47); `.contrast-more` decided (TD-69).
- [ ] **Accessibility pass prepared (GP.5):** the tester has a guide for M2 – M5; *gated:* findings fixed, no open issue labelled `a11y` + `blocker`.
- [ ] *Gated:* **dark theme released (GP.4)** with the reviewed values (O-S5, #240).
- [ ] **Directory core (M6.0d, M6.1):** splits storable (TD-31); `SoftwareProduct`, `PublishedEntry` with immutable versions, the factory with redaction, the identity resolver and cross-release matching, with their invariants `active` for the built parts (PUB-*, PRV-01 – PRV-04, CAT-03/04, SIG-*, ARC-07 — confirmed per task against TESTING §9 step 6).
- [ ] **Directory read side (M6.5, M6.6 server):** seeded entries through the factory in development and E2E; search endpoint and S40 with URL state, sort and paging (**DIR-01 – DIR-03**); read projections for S41 and S42 with their two-signal contract tests.
- [ ] Every new page in `reflow.spec.ts` (A11Y-09) and covered by axe in both appearances.

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | **~90 points** | Outside these days: the staging server and its secrets (TD-28), the decisions below, the tester, Figma (dark mode, missing frames), WERTE.IT (legal texts), disk space (TD-74) |
| **Total** | **8 days** | **~90 points** | |

**Basis:** sprint 04 ≈ 19, sprint 05 ≈ 16, sprint 06 ≈ 13 points per development day; sprints 05 and 06 delivered 65 – 80 points in about five days each. The capacity stays at ≈ 11 per day: many tasks need an owner decision or a step on the server first, and M6.1 is the domain with the strictest invariants.

**Planned load:** **58.5 points committed (65 %** of the assumed capacity): general debt 4 (7 %), M5 rest 3, G-P 21.5 (of which **7 gated** on outside input: S7-18, S7-19), M6 30 (packages 26 + debt rows 4). + **8 points stretch** (S41, S42). Not counted: S7-30 and S7-31, which start only if O-1/K-1 arrive (≈ 8 points; if they do, they replace the stretch).

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimates of `ROADMAP_v7.md`, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request; a task may be split if its plan shows more than one session. Order = recommended working order (Sequence below). Invariants per task are a **proposal**; Claude Code confirms them against TESTING §4 and §9 in its plan.

### P0 — committed (58.5 points)

#### Debt first — 4 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S7-01 | **Name the causes of the E2E failures on `main`:** Firefox loses the H1 focus on S80 after an API 5xx (#451, TD-84); mobile Safari runs axe in dark appearance before the colours switched, and loses the compact menu focus after Esc (#452, TD-85); WebKit types before the dialog's first focus in `account.spec.ts:120` (#436). With it the E2E hygiene of TD-99: one shared module for the keyboard helpers, bounds for the two download waits | 1 | Each failure has a named cause and a fix in the code or the test (no fixed sleep, no blind retry); #451, #452, #436 closed; three consecutive green runs on `main` |
| S7-02 | Quick wins in one PR: two concurrent evidences of one procedure answer 400, not 500 (#453, TD-88); one shared `TimeSpan` reader that requires `hh:mm:ss` (#456, TD-94); the Identity package and usings left in SERVICES and the claims types in MODELS (TD-95); the log sink on an empty database (#437); the page title on a second confirmation link (TD-68) | 1 | #453, #456, #437 closed; a test per item; no setting changes its meaning silently (the reader fails the start on a bare number) |
| S7-03 | Dependabot batch (TD-102): #439 (actions), #443 (dotnet), #441 (vitest), #442 (other), #440 (Angular 22.3), one at a time; #444 Serilog.AspNetCore 11 per **O-S7-11** (its target framework against "stay on .NET 10 LTS") | 0.5 | Each PR merged by the owner or closed with the reason; notices regenerated; `npm audit` and NuGet without findings |
| S7-04 | Documentation session, last: the drift of sprints 06 and 07 (TD-100: ARCHITECTURE §9.1, SPEC §9.2, TESTING, Handoff tabs of S34, Figma mapping); `CODEBASE` split into one file per area per **O-S7-12** (TD-101), with `CLAUDE.md` and the template of the task prompts pointing to the new place | 1.5 | Headers name current versions; `check-figma-links` and `check-invariants` green; if split: no file above 40 000 characters, every task ID still findable |

#### M5 rest — 3 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S7-05 | The assessments of candidates in the personal data export (`createdByMe.projects`), in the shape of **O-S7-1** (#414) | 1 | **PRV-07** workspace part complete in TESTING; sentinel test: no other user's identity; #414 closed |
| S7-06 | S11 of a project's copy gets `pa-breadcrumb` above the H1 as on S32 and S34, the last item per **O-S7-2**; the page joins `reflow.spec.ts` (#429, TD-91) | 0.5 | #429 closed; A11Y-09 covers `/projekte/:id/kriterienliste` at 320 px; axe in both appearances |
| S7-07 | Assign or replace a project's list on S32 (R-PRJ-04 UI, #420) per **O-S7-3**: `projects.detail.noCriteria.cta` when the project has no list, a replace action otherwise, the dialog with S31's select (saved lists of the active organisation, no copies), confirmation before replacing, the lock's 409 shown with its text, `If-Match` and S37 on a conflict | 1.5 | R-PRJ-04 met in the UI; WSP-02 stays green; E2E: assign, replace, replace refused after an assessment; #420 closed; **milestone M5 closed** |

#### G-P — 21.5 points (7 of them gated)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S7-08 | Workspace correctness per **O-S7-4**: Configurator refuses a change to the copy of an archived project by asking Workspace through a contract (TD-86, O-30); lock check and copy become atomic, and an assessment cannot slip between them (TD-87, O-31) | 2 | Integration tests for both races (deterministic, no sleeps); WSP-02, WSP-03 and API-03 stay green; O-30 and O-31 closed in ARCHITECTURE §16.5 |
| S7-09 | Deployment hardening in the repository (GP.1) per **O-S7-5**: API and client ports on loopback in `docker-compose.yml` (#449); `App:BaseUrl`, `Email:From`, `Email:DisplayName`, `Export:ToolUrl`, `ProblemTypes:BaseUri` passed by compose with safe defaults (#447); `/health/live` and `/health/ready` reachable through the client's nginx without exposing details (#448); `TECHNICAL.md` §3, §4, §6 follow | 2 | #449, #447, #448 closed; the dev and E2E stacks still work on `localhost`; a test or CI check for the nginx health route; the API refuses to start outside Development with a default public address |
| S7-10 | Staging from CI (GP.1) per **O-S7-6**: a workflow that deploys `main` to staging (manual dispatch first), a smoke check after the deployment (`/health/ready`, sign-in page, active release), the staging checklist of #236 points (1) – (15) worked through with the owner, staging never `Development`; the pre-pilot reset of old data (O-S5-4) | 3 | Staging runs the current `main`; the checklist on #236 ticked and the issue closed; a registration mail arrives with a staging link; `TECHNICAL.md` §4 describes the pipeline. *The steps on the server and its secrets are the owner's.* |
| S7-11 | Backup completion (GP.2d, #455) per **O-S7-7**: a dump of an emptied database fails the run; an alert when the backup or the API is unhealthy; the off-host copy; the restore rehearsed on staging (after S7-10) and recorded | 1.5 | #455 closed; `TECHNICAL.md` §7 and §6.3 updated; the staging rehearsal protocol in this folder |
| S7-12 | Security small items: `roles grant` and `roles revoke` update the security stamp, so a running session loses a revoked role at its next request (#450); another user signing in in another tab ends this tab's in-memory data like the S5-30 D3 case (TD-63) | 1.5 | #450 closed; tests on the fake clock for the stamp; a unit and an E2E check for the two-tab case; SEC-07, SEC-08 stay green |
| S7-13 | S12 reloads the criteria after `release-mismatch`, like S13, with the key of **O-S7-8** (#454, TD-89) | 0.5 | #454 closed; unit test; must be merged before any second release is activated |
| S7-14 | Release pipeline (TD-29): a workflow (manual dispatch) that builds the production images and runs `check-release-config.mjs`, so a placeholder legal text, the `example.invalid` suggestion channel or a missing public address stops a release; documented in `TECHNICAL.md` §4.2 | 1 | The workflow fails today (placeholders) and names each reason; a test of the script for each reason |
| S7-15 | Licence notices for the base images (TD-44): the OS packages of `aspnet:10.0`, nginx and PostgreSQL in the notices or in a documented statement per CLAUDE §10; the BIT inklusiv permission for the export's source line recorded once the owner has it (TD-53) | 1 | The notices check covers the images; TD-44 closed; TD-53 recorded or still named as the owner's |
| S7-16 | Pre-pilot polish per **O-S7-9**: one 🔧 time zone for the export file names and dates, the share links and the workspace (TD-47); what `.contrast-more` shows until H2 (TD-69); a shared content-link style (TD-67) | 1 | Dates show the German day at 0:30 (fake clock); `.contrast-more` as decided, axe green with forced `prefers-contrast`; one time-zone setting left |
| S7-17 | Guide for the manual accessibility pass (GP.5, TD-49, #311): per screen of M2 – M5 what to check with keyboard and screen reader (TESTING §7.2), the exported XLSX, the announcements of TD-41, #102; how findings become issues (`a11y`, `blocker`); German, for the tester | 1 | The guide is in `docs/` and covers every screen TD-49 lists; the owner has sent it with a staging or local address |
| S7-18 | **Gated — after the tester's pass:** fix the findings (GP.5), blockers first; the announcement class TD-41 and the organisation-switch announcement TD-66 with the owner's keys | 4 | No open issue labelled `a11y` + `blocker`; each fix with a regression test where one is possible; #311 closed |
| S7-19 | **Gated — after O-S5:** dark theme release (GP.4): the reviewed dark values from the Figma variable modes pinned as tokens (#240, TD-34), the development switch removed, axe and the contrast checks in dark on every page | 3 | R-SET-03 released; **A11Y-08** complete; #240 closed |

#### M6 start — 30 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S7-20 | Criterion splits (M6.0d, TD-31, #238) per **O-S7-10**: a criterion can name several successors; the release format, the validator and the importer follow; the carry-over of saved lists (S5-21a) handles a split | 2 | #238 closed; CAT-22 stays green; a MiniCatalog case with a split; TESTING records what CAT-03/04 need from it |
| S7-21 | Directory domain (M6.1): `SoftwareProduct`, `PublishedEntry` with immutable `PublishedEntryVersion`s for the two channels and the origin kinds, in the Directory module with its own context and migration (ADR-014); the immutability interceptor; entities and builders only — no endpoint yet | 5 | ARCHITECTURE §5.5 model as built; immutability invariants `active` (a stored version cannot change or be deleted outside the purge rules); **ARC-07** `active`; OPS-03 green |
| S7-22 | `PublishedEntryFactory` (M6.1): the only way to create an entry or a version, with one input contract for all three paths (workspace publication, contribution, vendor publication), redaction of private fields with sentinel tests, consent records, a definite `WasTested` for test-result entries, allowed ratings by path | 4 | **PRV-01, PRV-02, PRV-03**, **SIG-03** (published side), **PUB-** invariants of the factory `active`; an architecture test that nothing else constructs entries; CAT-15 directory part |
| S7-23 | Product-identity resolver (M6.1): trigram matching on name and vendor (`pg_trgm`), the shared resolver for all paths; cross-release matching of assessments (uses S7-20) | 4 | **CAT-03, CAT-04** `active`; resolver tests on the canonical fixtures (same product, near names, different vendors); a migration for the extension and index |
| S7-24 | Directory data in the development and E2E seeder (M6.5d): products and entries of both channels and all origin kinds, created **through the factory**; an `E2E` environment if needed (S5-14 P5); the canonical fixtures of TESTING §6.2 | 2 | The dev and E2E stacks show a filled directory; no seeder path bypasses the factory; documented in `CODEBASE` and `TECHNICAL.md` §2 |
| S7-25 | Directory search, server (M6.5): `GET /api/v1/directory/products` with name/vendor search, the filters in layer order, sort (name, last published, number of entries — never by matching), offset paging 20; public and cacheable, no organisation data | 4 | **DIR-01, DIR-02, DIR-03**, TEN-04 and API-06 directory parts `active`; the answer holds facts only (no ratio, no fulfilment number) |
| S7-26 | Directory search S40 (M6.5; Figma 360 `2134:2905`, desktop `2053:2051`, `2142:3850`): search field with debounce and cancelled requests, filters, URL state, sort with the order note, `pa-product-result`, paging above 20, empty state; „Produkte“ in the navigation leads to S40 and the legacy list is no longer linked | 4 | R-DIR-01 – R-DIR-07 met; S40 in `reflow.spec.ts`; compared with the frames; results announced (polite, throttled) |
| S7-27 | Read projections (M6.6 server, M6.9): product page and entry detail endpoints — two channel sections that never merge, K1 – K4 data, `wasTested` on every test-result entry, the seven counts with `notCovered`, experience reports apart, staleness, context line; the comparison against a list of the caller's organisation without leaking it (no private id in a public, cacheable URL) | 5 | **SIG-01 – SIG-11** for the built parts, **PRV-04**, PUB-11, PUB-14 `active`; contract tests of the two-signal projections; the IDOR case of the comparison tested |

### P2 — stretch (8 points, only if P0 without the gated tasks is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S7-28 | Product page S41 (Figma `2025:676`, 320 px `2039:2139`): header facts, `directory.product.noVerdict`, two channel sections, entry rows and cards, sort, compare select, experience reports, staleness note, copy link; replaces the legacy product detail | 4 | ROADMAP M6.6 (R-ENT-01 – R-ENT-08, R-ENT-10) |
| S7-29 | Entry detail S42 (Figma 360 `2132:3382`, desktop `2054:2542`): full card, table „Einschätzung je Kriterium“ with filter, rating legend, previous versions | 4 | ROADMAP M6.6 (R-ENT-09) |

Stretch items are pulled in order; if one is started but not finished, it goes back to its backlog as a whole.

### On arrival — not counted (only if O-1/K-1 arrive during the sprint)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S7-30 | Release 2026.1 with the project team's priorities, rationales, titles and short descriptions from the filled gap list: into `catalog/releases/2026.1/` (or `2026.2` if 2026.1 is stored anywhere), validated, imported and activated in development, E2E where intended and staging (M2.3) | ≈ 5 | Every criterion has a catalog-default priority with rationale; the release activates; #299 decided; O-1, K-1, K-6 closed in SPEC §13 |
| S7-31 | Legacy removal (milestone "Legacy removal (2026.1)"): the legacy filter `/filters` and its guard, CAT-10 with its seed and tests, `marked` / `ngx-markdown`, the M2 group of `LEGACY_EXCEPTIONS` (#158), #52; after S7-13 and S7-30 | ≈ 3 | #52 and #158 closed; `LEGACY_EXCEPTIONS` empty; J1 runs on the active release only |

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | **S7-01** first; batch S7-02, S7-06, S7-15; S7-09 | **Owner decisions O-S7-1 – O-S7-12 taken**; disk space freed (TD-74); the tester and the Figma dark mode scheduled |
| First half | D1–D4 | S7-08; S7-05, S7-07, S7-03; **S7-10** with the owner at the server, then S7-11, S7-12; **S7-21 → S7-22**; S7-20; batch S7-13, S7-14, S7-16; S7-17 | **Mid-sprint check (end of D4):** M5 closed; staging runs `main` from CI with the checklist ticked; the factory creates entries of all three kinds in tests (≈ 30 points) |
| Second half | D5–D8 | **S7-23 → S7-25 → S7-27**; S7-24, S7-26; gated S7-18, S7-19 when their input is there; **S7-04 last** | Directory: seeded entries searchable on S40; two-signal contract tests green. Pilot: the gate criteria of ROADMAP G-P checked one by one |
| Buffer | remaining | stretch S7-28, S7-29; S7-30, S7-31 on arrival | – |

**Parallel work:** after S7-01, up to three sessions in separate worktrees (`claude --worktree s7-nn`); the waves are in `TASK_PROMPTS.md`. Limits: **EF migrations** — one open branch per context (Directory: S7-21, S7-23, S7-25 if it adds an index, S7-27 if it adds one; Catalog: S7-20; Workspace/Configurator: S7-08 if the lock needs a column); **one local E2E stack** at a time (TD-74); **files in common** — `docker-compose.yml` and `TECHNICAL.md` (S7-09 → S7-10 → S7-11, in this order), the CI workflows (S7-10, S7-14), `package-lock.json` (S7-03 alone), S32 (S7-07 after S7-06 is merged if both touch the breadcrumb), the main navigation (S7-26; S7-31 after it).

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| every task | S7-01 merged or its causes named (reliable `main`) | internal |
| S7-05 | **O-S7-1** | Philipp, D1 |
| S7-06 | **O-S7-2** | Philipp, D1 |
| S7-07 | **O-S7-3** (dialog design, UX keys, O-28); S7-06 | Philipp |
| S7-08 | **O-S7-4** (O-30, O-31) | Philipp |
| S7-09 | **O-S7-5** (values and choices of #447 – #449) | Philipp, D1 |
| S7-10 | S7-09; **O-S7-6**; the staging server, its secrets, the key-ring certificate, the Lettermint sending domain | Philipp |
| S7-11 | S7-09; **O-S7-7**; the staging rehearsal after S7-10 | Philipp |
| S7-13 | **O-S7-8** (UX key) | Philipp |
| S7-16 | **O-S7-9** | Philipp |
| S7-03 | **O-S7-11** | Philipp |
| S7-17 | nothing; sending it needs staging (S7-10) or a local address | internal / Philipp |
| S7-18 | the tester's pass (TD-49) | tester |
| S7-19 | **O-S5**: the Figma dark mode and its accessibility review (#240) | Philipp, tester |
| S7-20 | **O-S7-10** (#238) | Philipp |
| S7-21 | nothing (first of the Directory line) | internal |
| S7-22 | S7-21 | internal |
| S7-23 | S7-21; S7-20 for cross-release matching | internal |
| S7-24 | S7-22, S7-23 | internal |
| S7-25 | S7-23 | internal |
| S7-26 | S7-25, S7-24 | internal |
| S7-27 | S7-22, S7-23, S7-25 (one Directory migration branch at a time) | internal |
| S7-28, S7-29 | S7-27, S7-26 | internal |
| S7-30 | O-1/K-1 from the project team; S7-13 merged | project team |
| S7-31 | S7-30 active everywhere it should be; S7-26 merged (navigation) | internal |
| S7-04 | every other P0 task | internal |
| **EF migrations** | Directory (S7-21 → S7-23 → S7-25 → S7-27), Catalog (S7-20), Workspace/Configurator (S7-08): one open migration branch per context | internal |
| **Invariant flips** | almost every M6 task: the later PR rebases and renumbers its TESTING version | internal |
| Outside the sprint | The first deployment on the staging server (TD-28); the tester's pass; the Figma dark mode and frames (O-S5, TD-80); legal texts from WERTE.IT and the O-6 channel (TD-29); the BIT inklusiv permission (TD-53); UX copy confirmations (TD-79); O-1/K-1; O-14/O-16; disk space (TD-74) | Philipp, project team, tester, WERTE.IT |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal |
|---|---|---|---|
| O-S7-1 | #414: shape of the assessments in the data export | S7-05 | `assessments [ { number, title, value, note } ]` per candidate, stored rows only (as in the issue) |
| O-S7-2 | #429: text of the last path item on S11 of a copy | S7-06 | (a) the page's H1 |
| O-S7-3 | #420: the dialog to assign or replace a project's list — place on S32, UX keys, whether replacing asks first; O-28: does assigning move the project's „last change“ | S7-07 | Reuse S31's select in a dialog with the S13 rules; a button on the criteria card („Kriterienliste zuweisen“ / „Kriterienliste ersetzen“); confirm before replacing; O-28: yes |
| O-S7-4 | O-30 (TD-86): refuse changes to the copy of an archived project on the server; O-31 (TD-87): how lock and copy become atomic | S7-08 | O-30: yes, Configurator asks Workspace through a contract. O-31: a row lock on the project in both paths |
| O-S7-5 | #449: loopback in the base compose file or a production override; #447: the values for staging and production; #448: health route in `nginx.conf` or in the proxy | S7-09 | Loopback in the base file; variables as in #447 with the `appsettings.json` defaults in Development only; `nginx.conf` |
| O-S7-6 | Staging deployment: how CI triggers it (a Dokploy deploy hook called by a manually dispatched workflow, or SSH), and whether staging is reset before the pilot (O-S5-4) | S7-10 | The Dokploy hook from a manually dispatched workflow, its URL as a repository secret; reset staging once before the pilot |
| O-S7-7 | TD-92: where the off-host copy goes and who gets the alert | S7-11 | An S3-compatible bucket in the EU with its own credentials (or a second host by `rsync`); alert by e-mail through the existing SMTP relay |
| O-S7-8 | #454: the UX key and wording for the reload on S12 | S7-13 | `criteria.save.outdated.reload` with the wording of S13's „Kriterien neu laden“ |
| O-S7-9 | TD-47: one time zone and the German day for all dates; TD-69: what `.contrast-more` shows until H2 | S7-16 | One 🔧 `App:TimeZone` (`Europe/Berlin`) replacing the two; `.contrast-more` shows the reviewed standard theme (the legacy colours go) |
| O-S7-10 | TD-31 (#238): store criterion splits as several successors | S7-20 | Yes: a list of successor keys; a carried-over decision goes to every successor |
| O-S7-11 | TD-51: require up-to-date branches before merging; #444 Serilog.AspNetCore 11 | S7-03 | Turn the setting on; take Serilog 11 only if it targets .NET 10, otherwise ignore the major |
| O-S7-12 | TD-101: split `CODEBASE_v1.md` into one file per area | S7-04 | Yes, `docs/codebase/<area>.md` with a short index |
| O-S6-8 | "Release 2026.1 active" as a G-P gate criterion (carried) | gate check at the sprint review | Yes |
| — | The other rows of TECH_DEBT_v6 §6: O-27/TD-62, O-29/TD-90, O-32, TD-93, TD-98, TD-79, TD-80, TD-77, TD-82, TD-81, #377, S6-16 P3 | when their task or the legal review needs them | see TECH_DEBT_v6 §6 |
| — | Priorities, rationales, titles of 2026.1 (O-1, K-1, K-6); O-14, O-16; the criteria-suggestion channel (O-6); legal texts — project team, WERTE.IT | G-P; O-6 before G-R | — |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | The pilot gate depends on people outside the sprint (server, tester, Figma, legal texts, O-1/K-1) | The development part is done but the gate stays closed | Gated tasks (S7-18, S7-19, S7-30, S7-31); schedule the tester and the dark mode at the sprint start; the gate criteria are checked one by one at the review |
| R2 | The first deployment since M3 meets 15 preconditions at once | Staging down, mails with wrong links, secrets in logs | Repository part first (S7-09); manual dispatch; smoke check; never `Development`; the checklist on #236; the owner at the server |
| R3 | The factory and the projections carry the two-signal and privacy promises | A public leak of private workspace data or a fulfilment score | Factory as the only constructor (architecture test), sentinel tests for redaction, contract tests for the projections before any screen; the strongest model |
| R4 | The comparison with a private list on a public page (the IDOR and caching finding of the architecture review) | Private ids in public URLs | S7-27 builds and tests the comparison as its own authorised request |
| R5 | Twelve owner decisions at the sprint start | Tasks wait | One decision round on D1 with the proposals above; tasks that need none start first |
| R6 | Lock and copy (TD-87) need cross-context consistency | Deadlocks or a wrong 409 | Decide the approach first (O-S7-4); deterministic race tests; Workspace and Configurator touched by one task only |
| R7 | Local E2E unreliable (TD-74); three unnamed E2E failures | Every PR's E2E in doubt | S7-01 first; disk space; one local E2E stack |
| R8 | S40 – S42 replace legacy screens that other code still uses (`withProducts()`, favourites) | Broken legacy pages mid-sprint | The legacy code goes in sprint 08 (M6.6d); S7-26 only moves the navigation |
| R9 | Many invariant flips and Directory migrations | TESTING and snapshot conflicts | One Directory branch at a time; the later PR renumbers |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6.7) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`, incl. CodeQL).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, deferred parts recorded there.
- [ ] UI strings only by key (`node scripts/check-i18n.mjs --write` for areas in `AREAS`); new pages join `reflow.spec.ts` and the axe runs in both appearances; their icons are in the icon list; screens with a Figma frame compared with it.
- [ ] `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` and `reuse lint` green; OPS-03 green for every new migration.
- [ ] New files carry SPDX headers; no secrets committed (deployment values only as repository or platform secrets); licences of new dependencies checked (CLAUDE §10) and approved by the owner.
- [ ] Documentation touched by the change updated: `CODEBASE` (§2 paragraph of the area, §3 layout), `TECHNICAL.md` for anything an operator must know, `CLAUDE.md` only for a rule, command or convention that applies across the codebase, ARCHITECTURE, SPEC §13, Figma mapping; register items closed by the PR named in it.
- [ ] Protocol `docs/sprints/sprint_07/S7-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in the earlier sprints.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; S7-01; the decision round O-S7-1 – O-S7-12; disk space; tester and dark mode scheduled |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: on **staging** — register, save a list, create a project, assign and replace its list, assess a candidate, export the personal data; the directory search on seeded entries; the G-P gate criteria checked one by one |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v7.md`; close-out below |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 08 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** the rest of M6 — S41 and S42 if not done as stretch, the three write paths together (publication S35/S36, contributions S50 – S53, vendor publication S55), „Merken“, product-identity administration S81, the legacy directory removed — then the G-R gate (legal review O-4, production environment, accessibility statement) and whatever of G-P waited for outside input.

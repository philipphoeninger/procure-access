# Sprint 06 — Workspace: projects, candidates and assessment (M5)

> **Status:** plan (2026-11-17; owner decisions O-S6-1 – O-S6-6 filled in 2026-11-18) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v6.md` (M5, G-P), `TECH_DEBT_v5.md` (§3, §5, §6, §8, §9.1, §9.2, §9.4), the highest versions of TESTING (§4.1, §4.5, §4.6, §4.9, §4.10, §6, §9 step 5), ARCHITECTURE (§4.3, §5.4, §7.1, §7.2, §8.1, §9.1, §9.2, §12 step 5, ADR-014 – ADR-016), SPEC (R-PRJ-*, R-CAN-02 – R-CAN-11, R-SHL-07, §7.3, §8.4), the Design handoff (§5.1 workspace screens, §5.3 screens without a frame), the Figma mapping (§2: S32, S34, S34a, S34b, S37), `CODEBASE_v1.md`, the sprint-05 close-out and protocols in `docs/sprints/sprint_05/`, the open GitHub issues and pull requests as of 2026-11-17.
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.
> **Task prompts:** `TASK_PROMPTS.md` in this folder (one ready-to-paste prompt per task).
> **IDs:** owner decisions of this sprint are `O-S6-n` (none used before). They are not the SPEC open question **O-S5** (Figma dark mode), which keeps its name.

---

## Sprint 05 in one paragraph (carried in)

Every task (68.5 P0 + 10 stretch points, eight of them split into a/b) was done in about **5 development days** (close-out in `SPRINT_05.md`). M3 and M4 are done: cookie session with antiforgery, registration, confirmation, reset and lockout, organisations with invitations and roles, saved lists with export and share links, settings S60, account page S61 with export and deletion, session hardening and warning; the legacy auth is gone. Carried in as follow-ups, not as tasks: `main` is red in the new J5 journey (TD-55, #379) and J1 has a mobile-Chrome download flake (TD-56, #380); the users still live in the legacy context (TD-72, #387); sign-in and organisation creation have no rate limit (#318, TD-60); the staging deployment (TD-28, #236) grew with M3; the 2026.1 priorities (O-1/K-1) are still open, so the legacy filter stays where no release is active (O-S4-3).

---

## Sprint goal

**Organisations run procurement projects: a project gets its own copy of a criteria list, candidates carry test status, evidence and an autosaved assessment per criterion, conflicting edits are caught — M5 is finished and J2 passes up to the assessment; `main` is green again and the pilot gate has every part that needs no outside input.**

The workspace is the second multi-tenant area after the saved lists and the first with shared, concurrently edited resources. The server comes first with its invariants (tenancy, project copy and lock, allowed ratings, ETags), then the screens. Five of the M5 screens have Figma frames (S32, S34, S34a, S34b, S37); S30, S31 and S33 follow Handoff §5.3.

### Success criteria

- [ ] **`main` green** before the first M5 merge: J5 on WebKit and Chromium, the J1 download on mobile Chrome (#379, #380 closed).
- [ ] **Users (M5.0d, if O-S6-1):** Identity tables, `API/Account`, `API/Auth`, `API/Organizations` and `GET /api/me` in IdentityAccess; the four user ids with foreign keys; #387 closed.
- [ ] **Projects (M5.1):** S30, S31, S32; project copy of a saved list, the copy adjusted via S11 with fixed selection layers; archived projects read-only; **WSP-02, WSP-03** `active`.
- [ ] **Candidates (M5.2):** S33, S34 with tabs as routes, S34a; tri-state test status with default „Nicht bekannt“; **WSP-01, SIG-03** (workspace part) `active`.
- [ ] **Assessment (M5.3):** S34b with allowed values, autosave, derived „Geprüft nach …“ rows, progress, filter, legend; **CAT-15, CAT-17** (workspace), **SIG-13** `active`.
- [ ] **Concurrency (M5.4):** ETag / If-Match on project, copy, candidate, evidence and saved lists; S37; last-write-wins for assessment rows; **API-03, API-04** `active`.
- [ ] **Remove and read-only (M5.5):** remove a candidate; Readers see every tab read-only; **TEN-05** complete, **WSP-04** (workspace part) `active`; **J2 up to the assessment** green on all five browser projects — M5 exit criterion.
- [ ] **Proposals retired (M5.6):** `ProposalService`, `withProposal()`, the proposal pages and their data gone; `withProducts()` / `withFavorites()` repointed; **ARC-05** `active`; #384 closed; `DateTime.UtcNow` outside tests: **0**.
- [ ] **Step-5 debt:** root store without the dev-mode warning; the M5 group of `LEGACY_EXCEPTIONS` (#158) empty; **PRV-07** (workspace part) `active`.
- [ ] **G-P without outside input:** rate limits for sign-in and organisation creation (#318); reload flows (#296, #310); legal pages from Markdown with the placeholder check (GP.3).
- [ ] **Debt of TECH_DEBT_v5 §9.1 closed:** #375, #378, #350, #385, #386, #368, #381, #382, #383, #388, #389; #355 narrowed down; the Dependabot batch #363 – #366 settled.
- [ ] Every new page in `reflow.spec.ts` (A11Y-09) and covered by axe in both appearances.

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | **~90 points** | Documentation, WERTE.IT coordination, reviews, the owner decisions below, the staging deployment (TD-28, #236) and freeing disk space (TD-74) are outside these days |
| **Total** | **8 days** | **~90 points** | |

**Basis:** sprint 03 ≈ 16, sprint 04 ≈ 19, sprint 05 ≈ 16 points per development day. The capacity stays at ≈ 11 per day: M5 is a new domain with the first concurrently edited resources, eight screens and dialogs with Figma comparison, and the move of the users touches every signed-in path. Local E2E is unreliable until disk space is freed (TD-74).

**Planned load:** **60 points committed (67 %** of the assumed capacity): M5 50 (packages 40 + debt rows 10), G-P 4 (GP.3 2, GP.d 2), general debt 6 (10 %). + **6 points stretch** (GP.2, GP.6). The load is below the usual 70 – 80 % on purpose: the M5 critical path is a chain of server tasks in one module (one open migration branch at a time), and the remaining G-P packages cannot start without outside input.

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimates of `ROADMAP_v6.md`, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request; a task may be split if its plan shows more than one session. Order = recommended working order (Sequence below). Invariants per task are a **proposal**; Claude Code confirms them against TESTING §4 and §9 step 5 in its plan.

### P0 — committed (60 points)

#### Debt first — 6 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-01 | **Make `main` green:** find why J5 sees two session-warning alert dialogs on WebKit (a real double-open or a test problem) and why the count check after „Schließen“ in the login overlay fails on Chromium (TD-55, #379); look at the J1 saved-export download timeout on mobile Chrome (TD-56, #380) | 1 | Three consecutive green CI runs on `main`; a real double-open fixed in the code with a unit test; #379 and #380 closed (or #380 narrowed with a documented retry decision) |
| S6-02 | Quick wins in one PR: S16 list gate for a list of only „Geprüft nach …“ criteria (#375); sticky-bar top edge in forced colours (#378); English `aria-label` on the legacy product list (#350); `credentials: 'omit'` in the health watcher so the dev server stops renewing the session (TD-64, #385); dead CSS and files (TD-71) | 0.5 | #375, #378, #350, #385 closed; axe green in both appearances |
| S6-03 | Return to the target page after the sign-in from a guarded deep link (TD-65, #386): `authGuard` keeps the return URL, only same-origin relative paths are accepted | 0.5 | #386 closed; unit test against open redirects (absolute URL, `//host`, `javascript:`) |
| S6-04 | Remove the legacy UI languages `en`, `es`, `fr` and their language service (#368); the M3 group of `LEGACY_EXCEPTIONS` is empty | 0.5 | #368 closed; `check-i18n` green; no `en/es/fr.json` in the client |
| S6-05 | Mail and token hygiene: 🔧 SMTP timeout per **O-S6-3** (TD-58, #382); a cleanup job for expired and used account tokens on the fake clock (TD-59, #383); the client's nginx `error_log` without share tokens (TD-57, #381) | 1 | #381, #382, #383 closed; cleanup tested with `FakeTimeProvider` |
| S6-06 | Narrow down the Firefox page-process crash after saving a list (#355, draft PR with the findings); check the S10 step-1 axe flake (#339) | 0.5 | Cause or reproducible minimal case recorded on #355; #339 fixed or closed with reason |
| S6-07 | CI smoke for `ng serve` and for the `immutable` header of lazy chunks (TD-73, #388) | 0.5 | #388 closed; CI fails when `ng serve` does not render or a lazy chunk lacks the header |
| S6-08 | Dependabot batch (TD-76): #363 (Playwright 1.65.1 with the container image tag), #364 (Angular), #365 (material-symbols with the subset script), #366 (dotnet); one at a time (TD-51); `npm audit fix` for the dev-only `minimatch` (TD-17) | 0.5 | Each PR merged by the owner or closed with the reason; notices regenerated; Dependabot alerts 12 and 13 closed |
| S6-09 | Documentation session (TD-78, #389): the sprint-05 and sprint-06 findings in TESTING, SPEC, ARCHITECTURE, Handoff, Figma mapping and the code comments; headers name current versions. ROADMAP v7 and the close-out are done in the control chat; the owner documents stay the owner's (TD-79) | 1 | #389 closed; `check-figma-links` and `check-invariants` green |

#### M5.0d Users into IdentityAccess — 5 points (debt row)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-10 | Per **O-S6-1**: the ASP.NET Core Identity tables move from the legacy `ApplicationDBContext` into IdentityAccess's context (data kept by the migration), the host endpoints `API/Account`, `API/Auth`, `API/Organizations` and `GET /api/me` become module endpoints, `UserService.GetCurrentUserAsync`, `User.CreatedAt` and the soft-deleted rows' role, claim and lockout columns follow; the four user ids get foreign keys (`UserContributorLink.UserId`, `Organization.CreatedByUserId`, `OrganizationMembership.UserId`, `AccountToken.UserId`); the E2E seeder `TestUserSeedExtensions` and the `roles` command use the module; the three copies of the legacy-account test helper go (TD-75); the JWT comment in `IdentityAccessModule.cs` is corrected (ADR-014 rule 4, TD-72) | 5 | #387 closed; ARC-01 – ARC-03 and every SEC/TEN invariant green; OPS-03 green for both migrations; existing accounts sign in unchanged. *Split expected (a: tables and keys, b: endpoints and seeder).* **Split as built (owner, 2026-11-18):** S6-10a = the whole move (entity, tables, endpoints, command, seeders — they compile only together), S6-10b = the foreign keys and the clean-up; #387 closes with b; S6-11 may start after a. **Both parts done (2026-11-18):** protocols `S6-10a_users_identityaccess.md`, `S6-10b_user_foreign_keys.md`. |

#### M5.1 Projects — 13 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-11 | Workspace server, projects: `ProcurementProject` (organisation-owned; name ≤ 120; status Laufend / Abgeschlossen / Archiviert; created by, last change) in the Workspace module with its own context and migration (ADR-014), covered by TEN-01; endpoints list (active organisation only), create, read, rename and status; archived projects read-only for everyone; no deletion (SPEC §3); the workspace rows of the authorisation matrix (TEN-05) | 4 | R-PRJ-01, R-PRJ-05 (server) met; TEN-01 covers the new entity; TEN-05 rows for projects green; `no-store` on every answer |
| S6-12 | Project copy of a criteria list: `PUT /api/v1/projects/{id}/criteria-configuration` copies a saved list with `ProcurementProjectId` set (source unchanged; a copy is not offered as a source and gets no share link — TEN-08 "project copy → 409"); S11 in project-copy mode (`criteria.list.projectCopy`: the four selection layers fixed; include, exclude, priority and note allowed; `projects.detail.criteriaAdded` on return to S32) | 4 | R-PRJ-02 (copy), R-PRJ-04 (assign/replace without assessments), R-PRJ-06 met; **WSP-03** `active`; WSP-02 copy part green (the lock follows in S6-18); TEN-08 project-copy part green |
| S6-13 | Client: `withWorkspace()` slice (ARCHITECTURE §8.1), routes under `/projekte`, project list S30 (name, status, products, last change; sort; `projects.empty`; „Archivierte anzeigen“) and create dialog S31 (name, optional organisation list with `projects.create.criteria.hint`); the header link „Projekte“ leads to S30 — Handoff §5.3 (no frame) | 2 | R-PRJ-01, R-PRJ-02 (UI) met; S30 in `reflow.spec.ts`; axe in both appearances |
| S6-14 | Project overview S32 (Figma 360 `2148:4697`, desktop `2148:3877`): criteria card with scope row, counts row `projects.detail.row.counts` (seven values, label first), product list (the name is the only link to S34), add-product menu, rename and status change | 3 | R-PRJ-03, R-PRJ-05 (UI) met; SIG-13 UI part; S32 compared with its frames; in `reflow.spec.ts` |

#### M5.2 Candidates — 8 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-15 | Candidates server: `CandidateProduct` (name, vendor, website, functional summary; private section — contacts, internal notes — stored apart so PRV-01/PRV-02 can hold in step 6), name-match hint data, `TestStatus` tri-state with default `Unknown`, `TestEvidence` 0..n with at most one per test type, „Andere Prüfung“ requires a name, a new product version updates the row; endpoints and TEN-05 rows | 4 | R-CAN-02, R-CAN-05 (server) met; **WSP-01**, **SIG-03** (workspace part) `active` |
| S6-16 | Candidate form S33 (Handoff §5.3: S34a title row and tabs, private section visually separated) and S34 with tabs as routes (Überblick default, Prüfnachweis, Einschätzung nach Kriterien; short labels below 600 px; a page title per tab) and the overview tab (Figma 360 `2134:3184`, desktop `2097:2627`): facts only, K1 workspace scope with seven rows, publication card `candidate.overview.publication.none` | 2.5 | R-CAN-02 (UI), R-CAN-03, R-CAN-04 met (publication states of step 6 excluded); S33 and S34 in `reflow.spec.ts`; compared with the frames |
| S6-17 | Evidence tab S34a (Figma `2022:267`, `2094:2525`, `2144:3508`): test status, evidence cards with H3 and fields in fixed order, duplicate procedure `candidate.evidence.list.duplicate`, switching away from „Ja“ with evidence asks to remove them | 1.5 | R-CAN-05 (UI) met; S34a in `reflow.spec.ts`; compared with the frames |

#### M5.3 Assessment — 8 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-18 | Assessment server: `CandidateAssessment` rows created lazily (no row = „Nicht eingeschätzt“), value validated against the union of allowed ratings of the project's app types, note ≤ 1 000, last-write-wins per row (single-field rows, no If-Match); procedure criteria derived, never stored or counted; progress and the seven counts on project and candidate views, pinned to the copy's release; the list lock — replacing the copy returns 409 once an assessment ≠ „Nicht eingeschätzt“ exists; excluded criteria keep their assessments | 3 | R-CAN-06 – R-CAN-08 (server), R-PRJ-04 (lock) met; **CAT-15**, **CAT-17** (workspace), **SIG-13**, **WSP-02** `active` |
| S6-19 | Assessment tab S34b (Figma 360 `2136:3127`, desktop `2022:841`): K6 per requirement criterion with the allowed values only (compact below 600 px), autosave with 800 ms debounce, `candidate.assessment.saved` at most every 5 s, note, derived „Geprüft nach …“ rows with a link to S34a, progress („36 von 46“), filter „Nur noch nicht eingeschätzte“, the rating legend expanded until the first assessment (state in `withSettings()`) | 5 | R-CAN-06 – R-CAN-08 (UI) met; S34b in `reflow.spec.ts`; keyboard-only autosave tested; compared with the frames. *Split likely (a: K6 rows and autosave, b: progress, filter, legend).* **Split as built (owner, 2026-11-19):** S6-19a = K6 rows, autosave and the derived rows (protocol `S6-19a_assessment_s34b.md`); S6-19b = progress with `candidate.assessment.list` (Q2 (a)), the filter and the legend with its state as a stored user setting (Q1 (b), an IdentityAccess migration; protocol `S6-19b_progress_filter_legend.md`). **Both parts done (2026-11-19).** |

#### M5.4 Concurrency — 6 points (incl. 1 point M5.4d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-20 | Optimistic locking: ETag with the configured `ETagEpoch` on project, project copy, candidate and evidence, and on saved lists (M5.4d: ends the 500 of a racing `PATCH …/criteria/{id}`, S5-21a R1); updates without `If-Match` rejected (428), a stale ETag → 409 with the competing editor's display name; assessment rows stay last-write-wins. *Split by module (owner decision S6-20 D4 (a), 2026-11-18): **S6-20a** — the shared parts (`ETagEpoch`, the tag, 428 and 409) and the Workspace module (project, candidate, evidence, the list assignment) with its client calls; **S6-20b** — saved lists and the project copy in Configurator, the race of S5-21a R1 and the S11 editor* | 3.5 | R-PRJ-07, R-CAN-11 (server) met; **API-03**, **API-04** `active` (S6-20a: workspace part; S6-20b completes API-03); the saved-list race has a test (S6-20b) |
| S6-21 | Conflict dialog S37 (Figma `2056:2519`) as shared component (`shared/components/conflict-dialog/`) with the behaviour of Handoff §5.1, used by the workspace forms and the saved-list edits on S11. **Split as built (owner, 2026-11-19, S6-21 D5 (a)):** S6-21a = the dialog, „Projekt bearbeiten“, S33 and S34a, the E2E with two contexts (protocol `S6-21a_conflict_dialog_s37.md`); S6-21b = the saved-list edits on S11 (a saved list and a project's copy, D3 (a)) and the selection change on S10 (D4 (a)) (protocol `S6-21b_conflict_saved_lists.md`) | 2.5 | R-CAN-11 (UI) met; E2E: two contexts edit the same project, the second gets S37; compared with the frame |

#### M5.5 Remove and read-only — 3 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-22 | Remove a candidate (Owner, Editor; confirmation dialog `candidate.remove.*`; evidences and assessments go); Readers see every tab read-only (no inputs, no publish button); **J2 up to the assessment** as E2E journey (create project → assign list → add candidate → evidence → assessment → Reader view) with axe in both appearances | 3 | R-CAN-09, R-CAN-10 met; **TEN-05** complete, **WSP-04** (workspace part) `active`; J2 green on all five browser projects — **M5 exit** |

#### M5.6 Proposals retired — 4 points (incl. 1 point M5.6d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-23 | Per **O-S6-4**: move the tagged API-02 test to a rebuilt form first; then remove `ProposalService`, `ProposalsController`, `withProposal()`, the proposal pages, routes, forms and their `LiveAnnouncer`, the proposal tables (legacy migration) and `moderation:review` for `PlatformAdmin`; repoint `withProducts()` / `withFavorites()` per ARCHITECTURE §8.1; ARC-05 (`BaseCrudController` only for catalog admin) | 4 | ARCH §12 step 5 proposal part done; **ARC-05** `active`; #384 closed (TD-61 by deletion); `DateTime.UtcNow` outside tests = 0; the proposal entries of `LEGACY_EXCEPTIONS` gone; what #82 still covers recorded on #82 |

#### M5 debt rows — 3 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-24 | Root store: `withLoading()` once in the root store instead of per feature (ends "SignalStore members cannot be overridden"), callers of the fixed legacy store methods catch rejections, legacy `<strong>` in `mat-error`; the remaining M5 entries of `LEGACY_EXCEPTIONS` (#158) | 1.5 | No store warning in dev mode; the M5 group of `LEGACY_EXCEPTIONS` empty (with S6-23) |
| S6-25 | Account deletion and data export for step 5: the projects (with candidates, evidences, assessments, copies) of an organisation whose last member leaves are deleted; `createdByMe.projects` in `GET /api/v1/account/export` (SPEC §8.4) | 1.5 | **PRV-07** (workspace part) `active`; TEN-11 deletion tests cover projects |

#### G-P without outside input — 4 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S6-26 | Rate limits per **O-S6-2**: sign-in per IP and per address, organisation creation per user (#318, TD-60); 429 with Problem Details and the UX key; relaxed in E2E like the export limit (TESTING §12.5) | 1 | #318 closed; limits tested on the fake clock |
| S6-27 | Reload flows per **O-S6-5**: one guarded reload when a lazy route chunk fails after a deployment (#296); the S13 reload after `release-mismatch` (TD-39, #310) | 1 | #296 and #310 closed; unit tests for the guard (no reload loop) |
| S6-28 | Legal pages from Markdown (GP.3, R-SHL-07) per **O-S6-6**: Impressum, Datenschutzerklärung and Erklärung zur Barrierefreiheit rendered from Markdown files under the footer links; the production build fails while a placeholder is left (`check-release-config.mjs`, TD-29 legal-page part); development shows the placeholder | 2 | R-SHL-07 met with placeholder content; the production build fails on a placeholder (CI test); the pages in `reflow.spec.ts` |

### P2 — stretch (6 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S6-29 | Nightly PostgreSQL backup with retention and a documented restore rehearsal (compose service and scripts; the rehearsal run locally, the staging run with GP.1) | 3 | ROADMAP GP.2, R-ADM-04 (completion) |
| S6-30 | `TECHNICAL.md` (operations, onboarding, the documented metric queries of R-MET-02) and a meaningful README (#4) | 3 | ROADMAP GP.6, process step 16 |

Stretch items are pulled in order; if one is started but not finished, it goes back to its backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | **S6-01** first (main green); low-risk batch S6-02, S6-04, S6-07; **S6-10** (users) once O-S6-1 is taken; S6-24, S6-03, S6-27, S6-28 alongside | **Owner decisions O-S6-1 – O-S6-6 taken**; disk space freed (TD-74); `main` green |
| First half | D1–D4 | S6-10 → **S6-11** → S6-15 → S6-18 (server line, Workspace module); S6-12 after S6-11; S6-13 after S6-11; S6-16, S6-17 after S6-15; S6-23 after S6-10 (legacy migrations); S6-05, S6-26 after S6-10; S6-08 after S6-02 | **Mid-sprint check (end of D4):** projects with a copy of a list, candidates with evidence and assessments work through the API; S30, S31 and S33/S34 in the browser (≈ 35 points) |
| Second half | D5–D8 | S6-19, S6-14 after S6-18; S6-20 → S6-21; S6-22 (J2); S6-25 after S6-15; S6-06 when no other session runs E2E locally; **S6-09 last** | M5 exit: J2 up to the assessment green; step-5 invariants active |
| Buffer | remaining | stretch S6-29, S6-30 | – |

**Parallel work:** after S6-01, up to three sessions in separate worktrees (`claude --worktree s6-nn`). Three limits decide what may overlap: **EF migrations** — one open branch per context (Workspace: S6-11, S6-15, S6-18, S6-20 if it adds columns; IdentityAccess: S6-10, S6-05 if the cleanup needs an index; legacy context: S6-10, then S6-23; Configurator: S6-12 if `ProcurementProjectId` gets a key, S6-20); **one local E2E stack** at a time (shared host ports; TD-74 disk space); **files in common** — S6-02 and S6-08 both touch `package-lock.json` (S6-08 after S6-02), S6-10 moves the endpoints S6-05 and S6-26 change (both after S6-10).

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| every M5 task | S6-01 merged (green `main`) | internal |
| S6-10 | **O-S6-1** | Philipp, D1 |
| S6-11 | S6-10 merged (users in the module; Workspace refers to users through IdentityAccess.Contracts). Without O-S6-1: nothing | internal |
| S6-12 | S6-11 | internal |
| S6-13 | S6-11; S6-24 preferably merged (store pattern) | internal |
| S6-14 | S6-12, S6-18 (counts), S6-13 | internal |
| S6-15 | S6-11 | internal |
| S6-16, S6-17 | S6-15, S6-13 | internal |
| S6-18 | S6-15, S6-12 | internal |
| S6-19 | S6-18, S6-16 | internal |
| S6-20 | S6-18 (all workspace resources exist) | internal |
| S6-21 | S6-20 (S6-20a for the workspace forms, S6-20b for the saved-list edits on S11) | internal |
| S6-22 | S6-19, S6-21 | internal |
| S6-23 | S6-10 (legacy context migration); **O-S6-4** | internal / Philipp |
| S6-25 | S6-15, S6-10 | internal |
| S6-05 | S6-10; **O-S6-3** | internal / Philipp |
| S6-26 | S6-10; **O-S6-2** | internal / Philipp |
| S6-27 | **O-S6-5** | Philipp |
| S6-28 | **O-S6-6** | Philipp |
| S6-08 | S6-02 (lock file) | internal |
| S6-09 | every other P0 task | internal |
| **EF migrations** | Workspace (S6-11, S6-15, S6-18, S6-20), IdentityAccess (S6-10, S6-05), legacy (S6-10 → S6-23), Configurator (S6-12, S6-20): one open migration branch per context at a time | internal |
| **Invariant flips** | almost every M5 task: preferably one at a time; otherwise the later PR rebases and renumbers its TESTING version | internal |
| Outside the sprint | Staging deployment per #236 (TD-28) — needed for GP.1 in sprint 07; disk space (TD-74); Figma dark mode and its review (O-S5); legal page content from WERTE.IT; O-14/O-16 texts; O-1/K-1 for 2026.1; the tester's passes for M2 – M5 (#311, TD-49) | Philipp, project team, tester |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal | Owner decision (2026-11-18) |
|---|---|---|---|---|
| O-S6-1 | TD-72: move the users into IdentityAccess at the start of M5 (S6-10)? Data: (a) the migration moves the tables with their rows; (b) a pre-pilot reset (users register again) | S6-10, D1 | Yes, at the start; (a), so staging accounts survive | Yes, at the start; (a) |
| O-S6-2 | Rate limits (#318, TD-60): sign-in per IP and per address, organisation creation per user — values | S6-26 | Sign-in like the reset (per IP and per address, the reset's window); 5 organisations per day per user; both 🔧 | Sign-in 30 per 10 minutes per IP and 10 per hour per address (not like the reset: an office behind one shared IP would be locked out after five sign-ins); 5 organisations per day per user; all 🔧, in SPEC §7.3 |
| O-S6-3 | SMTP timeout (TD-58) | S6-05 | 15 s per operation 🔧 | 15 s per operation 🔧 |
| O-S6-4 | Legacy proposals (TD-61, #384): (a) retire them and drop their tables with M5.6 (pre-pilot data, nothing to export); (b) keep the data and cover it in export and deletion | S6-23 | (a) | (a) |
| O-S6-5 | Reload flows: (a) one automatic reload per failed chunk, guarded against loops, then the error page (#296); the S13 reload after `release-mismatch` as described in #310 (TD-39) | S6-27 | (a) and #310 as written | (a) for #296, the S10 draft may be lost on the reload (no warning); #310 follows the confirmed UX copy (selection kept, per-criterion changes dropped), not #310's proposal; new button `criteria.export.outdated.reload` „Kriterien neu laden“ (owner-confirmed wording) |
| O-S6-6 | Legal pages: (a) three Markdown files in the client with a placeholder marker that fails the production build; (b) served by the API | S6-28 | (a); the content follows from WERTE.IT before G-P | (a), the files at `CLIENT/CLIENT.ProcureAccess/content/legal/*.de.md` (the client image cannot reach a root `content/legal/`); rendered with the existing `ngx-markdown` |
| O-S6-7 | Close milestone **M3** after #368, #375 and #380; move #318 → G-P, #170 → M6, #82 and #355 → M5 (TECH_DEBT_v5 §6, §7) | sprint review | Yes | open |
| O-S6-8 | Add "release 2026.1 active" to the G-P gate criteria (ROADMAP_v6 G-P) | G-P planning (sprint 07) | Yes — otherwise a partner works with the legacy filter, and project copies only come from the test release | open |
| — | Staging deployment (TD-28, #236), disk space (TD-74), TD-69 high contrast, TD-79 UX copy confirmations, TD-80 Figma frames for M3/M4, TD-47 export date, TD-77 Prettier, #377, TD-82 budget (TECH_DEBT_v5 §6) | before G-P; TD-74 now | see TECH_DEBT_v5 §6 | – |
| — | Priorities, rationales, titles of 2026.1 (O-1, K-1, K-6) and the criteria-suggestion channel (O-6) — project team | M2.3; O-6 before G-R | — | – |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | Moving the users touches every signed-in path (S6-10) | Sign-in, organisations or the seeder break | Data-keeping migration with OPS-03; every SEC/TEN invariant must stay green; split a/b; the strongest model; first in the server line, before any workspace endpoint |
| R2 | The workspace is the first area with concurrently edited resources | Lost updates, wrong 409s | API-03/API-04 with tests per resource; assessments stay last-write-wins by design (R-CAN-11); E2E with two browser contexts (S6-21) |
| R3 | Project copy and lock (WSP-02/03) are subtle rules | A replaced list loses assessments | Invariant tests first; the lock in S6-18 together with the assessments it protects |
| R4 | A long chain of server tasks in one module (S6-10 → S6-11 → S6-15 → S6-18 → S6-20) | The critical path decides the sprint length | Client, debt and G-P tasks run beside it; mid-sprint check at D4 |
| R5 | Local E2E unreliable (TD-74) and `main` red (TD-55) | Every PR's E2E in doubt | S6-01 first; free disk space before parallel work; one local E2E stack, the others leave E2E to CI |
| R6 | Screens without a frame (S30, S31, S33) and frame drift (S34b compact form) | Review rounds | Handoff §5.3 patterns; compare the framed screens before pushing; design follow-ups as issues |
| R7 | UX copy keys of M5 missing or worded differently from SPEC (e.g. K5 "hide" state outside S11, TECH_DEBT_v5 §5) | A task stops | Stop and ask (CLAUDE §6.5); the owner confirms new keys in the next UX copy version |
| R8 | Many invariant flips and migrations in parallel | TESTING and snapshot conflicts | The later PR renumbers or regenerates (as in sprint 05) |
| R9 | No active release outside the tests (D1) | A pilot with the workspace is not meaningful | Test release `T.1` in development and E2E; O-S6-8 |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6.7) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`, incl. CodeQL).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, deferred parts recorded there.
- [ ] UI strings only by key (`node scripts/check-i18n.mjs --write` for areas in `AREAS`); new pages join `reflow.spec.ts` and the axe runs in both appearances; their icons are in the icon list; screens with a Figma frame compared with it.
- [ ] `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` and `reuse lint` green; OPS-03 green for every new migration.
- [ ] New files carry SPDX headers; no secrets committed; licences of new dependencies checked (CLAUDE §10) and approved by the owner.
- [ ] Documentation touched by the change updated: `CODEBASE_v*.md` (§2 paragraph of the area, §3 layout), `CLAUDE.md` only for a rule, command or convention that applies across the codebase, ARCHITECTURE, SPEC §13, Figma mapping; register items closed by the PR named in it; replaced legacy files removed from `LEGACY_EXCEPTIONS` (#158).
- [ ] Protocol `docs/sprints/sprint_06/S6-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in the earlier sprints.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; S6-01; owner decisions O-S6-1 – O-S6-6; disk space |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: create a project → assign a list → adjust it on S11 → add a candidate → evidence → assessment → conflict → Reader view, in both appearances; success criteria checked |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v6.md`; close-out below |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 07 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** G-P completion — staging environment from CI (GP.1, after TD-28), dark theme release (GP.4, after O-S5), the manual accessibility pass of M2 – M5 and its fixes (GP.5), GP.2/GP.6 if not done as stretch, the rest of GP.d (TD-29 release pipeline, TD-44/TD-53 licence review) — and, to use the capacity, the start of M6 (M6.1 directory core). Release 2026.1 activated and the legacy filter removed as soon as O-1/K-1 arrive.

# ProcureAccess — Tech-debt register

> **Status:** v2 (2026-11-05) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 02, made with the skill `Engineering/tech-debt` (inventory → categorise → score → plan). Replaces `TECH_DEBT_v1.md`.
> **Sources:** open and closed GitHub issues (2026-11-05), the "Open points" of `docs/sprints/sprint_02/S2-*.md`, `SPRINT_02.md`, `ROADMAP_v2.md`, a scan of the code (TODOs, suppressed warnings, `DateTime.UtcNow`), a Release `dotnet build` of `ProcureAccess.sln`, `dotnet list package --vulnerable/--outdated`, `npm audit` / `npm outdated` (client and `e2e/`), Dependabot and CodeQL alerts, open pull requests and the ruleset of `main`.
> **Next review:** end of sprint 03 → `TECH_DEBT_v3.md`.

---

## 1. Summary

- **Sprint 02 cleared most of v1.** 30 of the 46 active v1 items are done (§4), plus three follow-ups that came up during the sprint (#140 – #142). Every planned debt task (S2-01 – S2-05, S2-09, S2-14) and every stretch task (S2-19 – S2-23) was delivered: about **10 of the sprint's 32 points were debt** (≈ 31 %, budget was 15–20 % plus stretch).
- **35 items are open:** 16 carried over from v1, 11 issues created during sprint 02, and 8 new items from the protocols and this scan (**TD-14 – TD-22**). 33 of them are scored in §3; #158 (lint exceptions) shrinks with the steps (§5), #102 is a question for the manual pass (§6). Two more are decisions for the owner (TD-15, TD-23, §6).
- **No live defect breaks a product rule (CLAUDE §8) any more.** Fonts and icons are self-hosted, Font Awesome is gone. What is left in §3.1 are legacy-client defects that users can see: the progress bar that stays after a failed load (#168), the offline notice that switches itself off (#171), English snackbar buttons (#160), and no strong focus indicator in the theme (TD-19).
- **Security gap in CI:** since 2026-10-30, CodeQL is no longer a required check (it did not run on Dependabot PRs, #144). Dependency updates are merged without a scan. One CodeQL alert is open (TD-16, low risk). The required checks of `main` are now stages 1–7 (stage 8 coverage no longer required, as CLAUDE §5 says).
- **AutoMapper (#31) is still the only High advisory** (4 open Dependabot alerts, suppressed in `Directory.Build.props`). It is committed for sprint 03 and needs the owner's choice of replacement and of the ARC-06 successor first (§6).
- **Documentation is drifting again:** about a dozen "next ARCHITECTURE / SPEC / TESTING / UX copy version" points from sprint 02 have not reached the documents (TD-14, Claude can do it; TD-15, owner's UX copy session).
- **Issue hygiene:** four open issues got their milestone, #176 waits for the owner (§7); nine new items got issues #180 – #188 (§8).

## 2. Method

Unchanged from v1 §2.

**Categories:** Code · Architecture · Test · Dependency · Security/Privacy · Accessibility · CI/Ops · Documentation.

**Scores** (1 = low, 5 = high): *Impact* (hurts users, product rules or development speed today) · *Risk* (security, legal, data, handover if it stays) · *Effort* (1 ≈ under an hour, 3 ≈ one session, 5 ≈ several sessions or a migration step).

**Priority = (Impact + Risk) × (6 − Effort)**, from 2 to 50. Above 25 → fix in the next sprint; 15–25 → schedule with the named step; below 15 → fix when the code is touched anyway.

**When:** *now* (sprint 03), a migration step / milestone (`ARCHITECTURE §12`, `ROADMAP`), *touch* (when the code changes anyway), or *watch* (act only on a trigger, §9.5). Re-scored items are marked ↑ or ↓ against v1.

## 3. Register (open items)

### 3.1 Defects users can see and rule violations

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #168 | Legacy stores never decrement the loading counter when a load fails, so `pa-loading-bar` stays and „Wird geladen …“ never ends (S2-17 P1); `approveProposal` has the same defect (S2-11 P6) | Accessibility | 3 | 2 | 1 | **25** | now |
| TD-19 (#184) | No strong focus indicator on Material components: Handoff §1.1 asks for `mat.strong-focus-indicators`, the theme does not include it (S2-15 P5). The theme also still names `'Poppins'` with a TODO (`styles/theme.scss`) | Accessibility | 3 | 2 | 1 | **25** | now, with M1.5 theme |
| #160 | Snackbar buttons 'Close' / 'Dismiss' are hard-coded English (CLAUDE §6.5, WCAG 3.1.2 — the same pattern as #120). The snackbar stays after M3. **Blocked** on UX copy wording (TD-15) | Accessibility | 3 | 2 | 1 | 25 | as soon as the wording exists |
| #171 | The offline notice switches itself off on the first page load when `de.json` answers after the failed API calls (S2-18 P2). Fix: count only `/api/` answers, plus a unit test | Code | 2 | 2 | 1 | 20 | now (before step 2) |
| #90 | Generic CRUD create and update always fail (no DTO→entity maps); `UpdateOne`/`DeleteOne` answer 200 for unknown ids (S2-10 P4) | Code | 3 | 2 | 2 | 20 | sprint 03, with #31 |
| #52 | Stepper steps on `/filters` have no accessible name (axe suppression) | Accessibility | 3 | 2 | 2 | 20 | step 2 (configurator rebuild) |
| #137 ↓ | Legacy icon `<span>`s in settings (×1) and profile (×2) are not `aria-hidden`; the profile list reads „box“. The header part is done (S2-15). Fix: `<mat-icon>` | Accessibility | 2 | 1 | 1 | 15 | now (one-line fix with #168) or M3 rebuild |
| #170 | Legacy login outlet (411 px) and product list / detail (714 px table, 20 px links, English names and `alt`) do not reflow at 320 px / 400 %. Owner: severity `minor`, no legacy fix (S2-18 P1) | Accessibility | 2 | 1 | 3 | 9 | step 3 (S01), step 6 (S40, S41) |

### 3.2 Security and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #144 | CodeQL runs as default setup and never starts on Dependabot PRs; the owner removed the three `Analyze` checks from the required checks (2026-10-30), so no PR needs a scan before the merge. Fix: advanced setup (workflow file), then require the checks again | Security | 2 | 4 | 2 | **24** | now |
| TD-18 (#183) | Swagger UI and the OpenAPI documents are served in every environment, Production included (`ConfigureSwaggerExplorer()` has no environment check, S2-21 P1). Owner decides whether that is wanted (§6) | Security | 2 | 2 | 1 | 20 | now (decision + small fix) |
| TD-21 (#186) | Data Protection keys live only in the container and are lost on every new container (S2-21 P3). Harmless with JWT today; the cookie session and antiforgery of step 3 depend on them | Security | 1 | 4 | 2 | 20 | step 3, before the cookie session |
| #31 | AutoMapper 14 has GHSA-rvv3-g6hj-g44x (High, suppressed in `Directory.Build.props`, Dependabot alerts 1, 8, 9, 10); v15+ is RPL/commercial (D12) | Dependency | 4 | 4 | 4 | 16 | sprint 03 (M1.1d), owner ADR first |
| #112 | `/health/ready` is public on the API port and opens a database connection per request; timeout is a code constant | Security | 2 | 2 | 2 | 16 | step 2, with the first rate limit |
| TD-12 | The E2E job's `compose.log` artefact could contain per-run secrets; harmless while they are random | Security | 1 | 2 | 1 | 15 | watch at step 3 (Mailpit, real secrets) |
| TD-22 (#187) | Whether the licence notices of the API image's NuGet packages reach users was not checked; the client image already serves `3rdpartylicenses.txt` (S2-03 P4) | Dependency | 1 | 2 | 2 | 12 | next licence review, before G-P |
| TD-17 | `minimatch` 10.2.6 (moderate, GHSA-g3hv-75wg-m249, Dependabot alert 12) inside `eslint` / `typescript-eslint`. Dev tool, not shipped. The fix is minimatch 11, which `eslint` 10 does not accept yet; `npm audit fix` would downgrade ESLint | Dependency | 1 | 1 | 1 | 10 | watch: take the ESLint release that moves to minimatch 11 |

### 3.3 Code and architecture

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #118 ↓ | 55 warning locations in a Release build, all in the four legacy projects: 48 nullable (33 × `CS8618`, 28 of them in `MODELS` entities), 2 `VSTHRD200`, `AV0027` (#176), `CS0168`, `CS0105`, `CS1572`, `CS1573`. The analyzer check in CI covers only `tests/`. The module projects have no warnings, but nothing keeps it so: `src/Directory.Build.props` has no `TreatWarningsAsErrors` (S2-06 P4). Unused `Serilog.Settings.Configuration` in `SERVICES` (S2-07 P3) | Code | 2 | 2 | 2 | 16 | sprint 03 (M1.1d) |
| #115 | Usage events: write fire-and-forget, without retries on the request path (R-MET-01) | Code | 2 | 2 | 2 | 16 | step 2 (first caller) |
| #135 | Full Material Symbols font, 4.0 MB, for about 40 glyphs; `font-display: block` shows blank icons, then ligature names, on a slow first visit. Options in the issue (owner, O-S2-5 follow-up) | Code | 3 | 2 | 3 | 15 | owner decision (§6); M1.5 or before G-P |
| #42 | Bundle budget raised to 2 MB / 2.5 MB; tighten after the rebuild | Code | 2 | 1 | 2 | 12 | end of M2 |
| #153 | Versioned routes `api/v{version}/[controller]` never exist (derived controllers override the base route); Swagger lists `api-version` and `v` twice on every operation (S2-21 P2) | Code | 1 | 2 | 2 | 12 | step 2, with `/api/v1` |
| #176 | Swagger: switch to `DescribeApiVersions` (AV0027) with the first versioned minimal API; no milestone yet (S2-21 P4) | Code | 1 | 1 | 1 | 10 | trigger: first versioned minimal API |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-20 (#185) | The invariant gate (`check-invariants.mjs`) has no permanent self-test; its lexer was tested only with a scratch script in S2-09. The Figma and i18n checks already have `node:test` self-tests (S2-09 P2, S2-20 P3) | Test | 1 | 3 | 2 | 16 | now or any CI PR |
| #87 | Session probe must answer 200 for anonymous and unknown test users | Test | 2 | 3 | 3 | 15 | step 3 |
| #88 | Integration host option for the real Identity cookie flow (SEC-01/02/09) | Test | 2 | 3 | 3 | 15 | step 3 |
| #89 | Test that the antiforgery helper's session probe carries the caller's headers | Test | 1 | 2 | 1 | 15 | step 3 |
| #82 | Regression tests for the S1-10 zoneless fixes (Z1–Z5, R1–R5) | Test | 2 | 2 | 3 | 12 | touch (screens are rebuilt in M2–M3) |
| #92 | CAT-10: `ExistingCatalogSeed` can drift from the migration seed | Test | 1 | 2 | 2 | 12 | step 2 replaces the seed |
| #91 | CAT-10: comparison key ignores JSON value types | Test | 1 | 1 | 1 | 10 | step 2 (CAT-10 rewrite) |

### 3.5 CI and tooling

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #161 | No ESLint for `e2e/` and `scripts/*.mjs` (a missing `await` before a Playwright `expect` goes unnoticed); `eslint-plugin-playwright` would be a new dev dependency (owner) | CI/Ops | 1 | 2 | 2 | 12 | touch; no milestone yet |
| TD-16 (#182) | Open CodeQL alert 3 (`js/prototype-pollution-utility`, `scripts/check-i18n.mjs:170`, `setText`). Input is the repository's own UX copy, so the risk is low, but an open alert hides new ones. Fix: reject the segments `__proto__`, `constructor`, `prototype` with a self-test case | CI/Ops | 1 | 1 | 1 | 10 | now (quick win, with TD-20) |

### 3.6 Documentation

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-14 (#180) | Sprint-02 findings not yet in the documents. **ARCHITECTURE v16:** O-22 interim rule "contracts reference nothing until decided" (S2-08 P1); Platform "as built" with Problem Details (S2-10 P3); C93 `/api/Health` removed (S2-19 P2). **SPEC v3:** `email-unconfirmed` in §8.6 (S2-10 P3); R-SHL-06 open footer targets („Über ProcureAccess“, „Projekt WERTE.IT“, „Kriterium vorschlagen“) as open questions in §13 (S2-15 P2). **TESTING:** status line still starts with „v15 —“ (S2-13 P5). **Headers** of Handoff, SPEC and TESTING name UX copy v14 (S2-16 P6); Figma mapping header names UX copy v12, ARCHITECTURE v10, TESTING v8, Handoff 1.12 (S2-22 P5). **ROADMAP v3:** M1.4d still lists #122 as open (S2-19 P3); sprint-02 velocity. **SPRINT_02:** R5 and S2-14 row say "M1–M3" instead of M1–M5 (S2-14 P6); close-out table empty | Documentation | 3 | 3 | 2 | **24** | now (one documentation session) |
| #4 | README is 18 lines; a successor needs setup, links to `docs/` and the process | Documentation | 2 | 3 | 2 | 20 | step 16 (`TECHNICAL.md`), G-P |

## 4. Done in sprint 02

| Item (v1 ID) | What was done | Task |
|---|---|---|
| #37, #95, #96, #113 | Products filter `await` and spinner; delete dialog gate with its spec cases; dev health watcher on the local API | S2-01 |
| #121 (TD-09) | `ApplicationName` in every log row | S2-02 |
| #43, #78 | Material Symbols self-hosted (Apache-2.0); Font Awesome and the classic Material Icons removed; J1 fails on any third-party request | S2-03 |
| #141 | EasyMDE check-list glyph from Material Symbols | S2-04 follow-up |
| #117 (TD-05) | NetArchTest.Rules 1.3.2 works on .NET 10 (limits L1, L2 in TESTING_v16 §3.1) | S2-05 |
| §5 row "move `UsageEvent` and its writer to Platform" (S1-23 P6) | Platform module with its own `PlatformDbContext` and history (ADR-014) | S2-07 |
| #109, #111 | Gate: header drift, test files only, tests that run, .NET trait, protocol files | S2-09 |
| §5 row "ad-hoc error objects in `BaseCrudController` / `UICustomizationController`" | Problem Details everywhere (API-01) | S2-10 |
| #101 | Dotted property names next to a key with text (`shell.nav.menu` / `menu.close`) | S2-13 |
| #119 (TD-07) | `typescript-eslint` `recommendedTypeChecked` on every `.ts`; legacy findings in `LEGACY_EXCEPTIONS` (#158) | S2-14 |
| #120 (TD-08), S1-21 P4 | New shell header with German names; header off the `PENDING` list; #137 header part | S2-15 |
| #122 (TD-10) | E2E readiness on `/health/ready`; `/api/Health` removed | S2-19 |
| #104 – #107, #140, #142 | CI one-liners; Dependabot ignores `@types/node` majors; J1 tooltip flake | S2-20 |
| #77, #74, #73 | Release build as non-root `app` with a cached restore layer; XML doc file untracked, OpenAPI package removed; `SwaggerDefaultValues` hardened (AV0027 split out to #176) | S2-21 |
| #110 | Figma link check: pages, moved components, `PENDING` validation | S2-22 |
| #97 | Client coverage without `models/` folders | S2-23 |
| TD-02 – TD-04 (#116), #98, #123 (TD-11), #124 (TD-13) | Dependency PR #126, documentation PR #127, random key PR #128 — merged at the sprint-01 review | sprint-01 review |
| TD-01 | Branch protection (done before v1). **Changed since:** the required checks are stages 1–7; CodeQL was removed (→ #144), stage 8 is no longer required | owner |

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design. Do not fix it on its own; replace it with the step. New in v2 are marked *(new)*.

| Debt | Retires with |
|---|---|
| JWT in `localStorage`, `jwtDecode` with `any` in `auth.service.ts` (SEC-01) | step 3 (M3) |
| `DateTime.UtcNow` ×6 (`UserRepo` ×1, `UserService` ×4, `ProposalService` ×1) against CLAUDE §7 | `UserService`/`UserRepo` with step 3, `ProposalService` with step 5 |
| `Microsoft.AspNetCore.Identity.UI` only for the `IEmailSender` interface | step 3 |
| *(new)* `UsersController` and the identity minimal APIs answer business failures with `{ code, message }`; dead `BadRequest(ModelState)`; `signUp` / `reset-password` answer 500 on a missing field (S2-10 P1). The client's `ERROR_TRANSLATIONS` still lists the dead key `UICustomizationNotFound` (S2-10 P7) | step 3 |
| *(new)* Delete-account „Löschen“ uses native `disabled` instead of `aria-disabled` with a visible reason; needs a UX copy text (S2-01 P3) | step 3 (identity rework) |
| *(new)* Legacy login outlet replaces the whole shell; does not reflow (#170 L1) | step 3 (S01) |
| *(new)* `[Produces("application/json")]` on legacy controllers sends Problem Details as `application/json`; the tolerance in `ProblemResponse.AcceptedMediaTypes` goes with them. New controllers must not copy the attribute (S2-10 P2) | each replacing step |
| *(new)* Legacy routes without a page-title key and without an H1: `/filters`, `/products`, `/product/:id`, `/favorites`, `/product-proposal/:id`, `/criterion-proposal/:id`, `/proposals`, `/reset-password`, `/confirm-email` (S2-15 P1, S2-16 P1) | rebuild of each screen |
| *(new)* Three legacy `LiveAnnouncer` calls outside `A11yAnnouncerService` (S2-16 P4) | steps 2 and 5 |
| *(new)* The API sends English validation messages without codes; the client shows them through `errors.field.server` (S2-11 P2) | each step's module validation |
| *(new)* Uppercase legacy roots in `de.json` (`COMMON`, `AUTH`, `ERROR` …) next to the new lowercase ones (S2-11 P7) | rebuild of each screen (CLAUDE §9) |
| *(new)* `LEGACY_EXCEPTIONS` in `eslint.config.js`: 10 files in the groups M2, M3, M5 (#158) | each step deletes its entries; #158 closes when the list is empty |
| *(new)* Legacy product reads ignore `Product.IsDeleted`; `GetByCriteriaFilterIds` lacks the proposal filter of `ReadAll` (S2-01 P1, P2) | step 6 (directory) |
| *(new)* `POST /api/Proposals/approve` without the permission ends in a 500 (S2-10 P6); favourites and proposals have no header link (S2-15 P6, S2-17 P4) | step 5 (proposal retirement, `withFavorites()`) |
| *(new)* The tagged API-02 test sits on the legacy proposal form (S2-11 P5) | move to a rebuilt form before step 5 retires the proposal |
| ~25 TODOs in legacy client features and DAL repos (31 hits today, all in legacy code except the `Poppins` line of TD-19) | features retired or rebuilt in M2–M5 (`ARCHITECTURE §8.1`) |
| `vitest-axe` not installed (S1-16 P5) | first K-component (M2) |
| E2E without `E2E` environment, seeder, Mailpit, clock endpoint, relaxed rate limit (S1-18 P4) | steps 2–4 |
| CAT-10 on `ExistingCatalogSeed` and the old filter (S1-15 P1) | step 2 (catalog release import) |
| `Approver` role, `withProposal()`, `withProducts()`/`withFavorites()` | per `ARCHITECTURE §8.1` |

Not tracked on purpose (unchanged): CI has no NuGet/Docker cache; Serilog stamps with the system clock. *(new)* Other client files without logic (`*.routes.ts`, `app.config.ts`) still count for coverage (S2-23 P1) — few, partly run by tests.

## 6. Decisions waiting for the owner

CLAUDE §6.6: Claude Code must not decide these.

| Decision | Source | Needed by |
|---|---|---|
| **AutoMapper replacement** (#31) and what replaces ARC-06 in TESTING — as an ADR (CLAUDE §11) | SPRINT_02 owner decisions | start of sprint 03 |
| **TD-15 UX copy session** (#181) — one session for all gaps: singular (and zero) forms of the 18 rows of S2-12 F2 and the „/“ forms as `.one`/`.zero` keys (S2-12 P1); the storage rule for dotted keys and the `de.json` path in §5 (S2-13 P1); written-out texts for `pageTitle.notFound` / `.forbidden` / `.server` (S2-13 P3); one key per variant for `pageTitle.criteria.list` and `pageTitle.publish` (S2-16 P2, needed before S11/S35 in step 2); UX keys for eight problem types without one (S2-11 P1, also in SPEC §8.6); snackbar button texts (#160); the reason text for the disabled „Löschen“ (S2-01 P3); soft hyphen in the legal titles (S2-18 P3); account button name vs visible „Konto“, WCAG 2.5.3 (S2-15 P4, before step 3) | S2-01, S2-11 – S2-18 | sprint 03 (#160 and step 2 wait for it) |
| **#135** font subsetting: which option of the issue | S2-03 P1 | M1.5 or before G-P |
| **TD-18** (#183) Swagger UI in Production: wanted for the public API, or `Development` only? | S2-21 P1 | sprint 03 |
| **TD-23** (#188) Reflow invariant: give `reflow.spec.ts` an ID (e.g. „A11Y-09: every shell page and every rebuilt screen needs no horizontal scrolling at 320 × 256 CSS px“) in the next TESTING version? | S2-18 P5 | sprint 03 |
| `e2e/` TypeScript version: follow the client's TypeScript (ignore rule like the client) or update on its own? Dependabot PR #175 (TypeScript 7 in `e2e/`) waits for it | S2-20 P1 | next Dependabot review |
| #161: allow `eslint-plugin-playwright` as a new dev dependency (licence check, CLAUDE §10)? | S2-14 P4 | when #161 is taken |
| #176 milestone: in which step does the first versioned minimal API come? | S2-21 P4 | when M2 endpoints are planned |
| O-22: may a `Contracts` project reference another module's `Contracts`? | S2-08 P4 | first contracts type that needs it (step 2 or 3) |
| Pages without a Figma frame (S04, S80, legal pages); base frame of S35 is the „gesperrt“ state | S2-22 P2, P3 | design, any time |
| Does tapping the product-type field open the autocomplete on phones? | #102 | manual pass (TESTING §7.2) |

## 7. Issue hygiene

- **Milestones set on 2026-11-05:** #135 → M1 (decision with M1.5; move to G-P if subsetting waits), #137 → M1, #161 → M1 ("touch" items carry the current milestone, v1 §7), #102 → G-P (manual pass). #176 stays without a milestone until the owner names the step (§6).
- **Labels:** all open debt issues carry `tech-debt` except #52, #90, #102, #171 (`bug`/`a11y`/`question`), which is correct.
- **Open Dependabot PRs:** #164 (nginx 1.31), #165 (Angular group), #166 (other group), #175 (TypeScript 7 in `e2e/`, waits for §6). Weekly review per §9.6.
- **Stale statements in sprint-02 documents** (SPRINT_02, TASK_PROMPTS) about removing `--ignore-exit-code 8`: resolved since S1-23 (S2-05 P2, S2-08 P3); v1 already corrected its own rows. Nothing left to do except noting it in the SPRINT_02 close-out.

## 8. New issues

Created on 2026-11-05 for the **TD-** items that need one:

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-14 | #180 Carry the sprint-02 findings into ARCHITECTURE, SPEC, TESTING, ROADMAP and the document headers | documentation, tech-debt | M1 |
| TD-15 (§6) | #181 UX copy: one session for the gaps found in sprint 02 (plural forms, page titles, problem types, snackbar, delete reason, account button) | documentation, question | M1 |
| TD-16 | #182 Fix CodeQL alert 3: guard `setText` in `check-i18n.mjs` against prototype keys | tech-debt | M1 |
| TD-18 | #183 Decide: serve the Swagger UI only in Development? | question, tech-debt | M1 |
| TD-19 | #184 a11y: strong focus indicators on Material components (Handoff §1.1) | a11y, tech-debt | M1 |
| TD-20 | #185 Permanent `node:test` self-test of the invariant gate | tech-debt | M1 |
| TD-21 | #186 Persist the Data Protection keys outside the container before the cookie session | .NET, tech-debt | M3 |
| TD-22 | #187 Licence notices of the NuGet packages in the API image | dependencies, tech-debt | G-P |
| TD-23 (§6) | #188 Decide: invariant ID for the 320 px reflow check | question | M1 |

TD-17 is tracked by Dependabot alert 12 and needs no issue.

## 9. Fix plan — when to fix what

### 9.1 Sprint 03 — finish M1, then start M2

Sprint 03 finishes M1 (M1.5 theme, A11Y-08) and starts M2.1. Debt budget **≈ 7.5 points**, of which 4.5 are M1 debt already counted in `ROADMAP_v2` (M1.1d) and 3 are new.

| # | Work | Items | Pts | Note |
|---|---|---|---:|---|
| 1 | AutoMapper replacement with #90 and the ARC-06 successor | #31, #90 | 3 | owner ADR first (§6); clears the only High advisory |
| 2 | Build warnings and analyzer check on all projects; `TreatWarningsAsErrors` in `src/Directory.Build.props`; drop unused `Serilog.Settings.Configuration` | #118 | 1.5 | best right after #31, which touches the same projects |
| 3 | CodeQL advanced setup; require the `Analyze` checks again | #144 | 0.5 | |
| 4 | Legacy client fixes in one PR: loading counter on failure, offline notice only from `/api/` answers, icon spans → `<mat-icon>` | #168, #171, #137 | 1 | each with a unit test |
| 5 | Documentation session; CodeQL alert fix and gate self-test in the same scripts PR | TD-14, TD-16, TD-20 | 1.5 | |
| — | With M1.5 (no extra points): strong focus indicators, `Poppins` TODO, H1 type scale of S04/S80 (S2-18 P4) | TD-19 | 0 | |
| — | Owner: §6 decisions marked "sprint 03", the UX copy session TD-15 (then #160), milestones of §7 | §6, §7 | 0 | |
| — | Quick follow-up after the owner's TD-18 decision | TD-18 | 0.5 | only if "Development only" |

### 9.2 M2 — step 2 (catalog, configurator, export)

| Fix | Items | Why then |
|---|---|---|
| Before the writer gets its first caller | #115 (fire-and-forget, retry delay S1-23 P4) | No pilot data yet |
| With the configurator rebuild | #52 (stepper), J1 rewrite (S1-19 P5), CAT-10 rewrite incl. #91, #92 (S1-15 P1, P3; S1-21 P6); the legacy `LiveAnnouncer` calls of the configurator; empty states and button spinner (S2-17 P5) | The screens and the filter are replaced |
| Before S11 / S35 | `pageTitle` as an `AREAS` root once TD-15 split its variants (S2-16 P2); `shell.pageTitle.error` with the first rebuilt form (S2-16 P5) | Needs the UX copy keys |
| With `/api/v1` | #153 (and the duplicate Swagger parameters); #176 if the first versioned minimal API comes here | Same routes |
| With the export pipeline | ARC-03 rule for `AdHocSelectionSource` (S2-08 P2); no request bodies of the anonymous export in the logs (S1-22 P7, EXP-05); E2E seeder and relaxed rate limit (S1-18 P4) | Needed by the export and its journey |
| With the first rate limit (M2.7) | #112 incl. the health timeout | Same mechanism |
| First K-component | `vitest-axe` (§5) | New components start with it |
| Each step's validation | German messages or codes for field errors (S2-11 P2) | Replaces the English API messages |
| End of M2 | #42 bundle budget (with #135 if subsetting is chosen) | After the screen rebuild |

### 9.3 M3 – M7

| Milestone | Fix | Items |
|---|---|---|
| M3 | Data Protection keys persisted **before** the cookie session | TD-21 |
| M3 | Cookie session replaces the JWT; `Identity.UI`, `DateTime.UtcNow` in `UserService`/`UserRepo`, `{ code, message }` failures, the legacy account pages and `ERROR_TRANSLATIONS` go with it | §5 |
| M3 | Integration host for the real cookie flow; session probe; `TestUsers` as stored rows (S1-13 P5) | #87, #88, #89 |
| M3 | Mailpit and real secrets in the E2E stack: check the `compose.log` artefact; dev and E2E stacks share host ports (S1-18 P6) | TD-12 |
| M3 | Correlation ID and the logging rules of PRV-04 (S1-22 P7); account button WCAG 2.5.3 (TD-15); login outlet reflow; e-mail confirmation pages „as S80“ (S2-17 P8) | #170 L1 |
| M2 – M3 | Regression tests for the zoneless fixes, screen by screen | #82 |
| M4 | First time-dependent tests: shared fake clock only moves forward (S1-13 P4); E2E clock endpoint for J5 (S1-18 P4); S34 child routes in Figma mapping §2.2 and `PAGES` (S2-22 P1) | — |
| M5 | Proposal retirement: `ProposalService`, `withProposal()`, `Approver`, `approve` 500, favourites; move the API-02 test first | §5, #158 M5 group |
| M6 | Directory rebuild: `IsDeleted` and proposal filter on product reads; product list and detail reflow (S40, S41); start-page cards for projects, contributions and vendors (S2-17 P3); TEN-03 (S2-17 P7) | #170 L2, L3 |
| M7 | File-store check in `/health/ready` (S1-22 P3) | — |

### 9.4 Before the pilot (G-P)

| Fix | Items |
|---|---|
| README and `TECHNICAL.md` (process step 16), incl. the indicator queries of R-MET-02 | #4 |
| Licence notices of the API image | TD-22 |
| Swagger exposure settled for Production (if not done in sprint 03) | TD-18 |
| The axe suppression list is empty (TESTING §12.4) | #52 and any later entry |
| Manual accessibility pass (TESTING §7.2, A11Y-07): screen readers on H1 focus plus title announcement (S2-16 P3), offline notice and queued announcements (S2-17 P2), real browser zoom at 320 px / 400 % (S2-18 P6), autocomplete on phones | #102 |

### 9.5 Watch items — act only when the trigger happens

| Trigger | Then | Source |
|---|---|---|
| ESLint / typescript-eslint release accepts minimatch 11 | Take it (Dependabot); close alert 12 | TD-17 |
| A future SDK breaks NetArchTest (the ARC self-tests would show it) | Fork `NetArchTest.eNhancedEdition`, with an ADR | S2-05 P3 |
| One of the gate's known limits bites (runtime skips, CI filters, preprocessor, `[Theory]` counted once, group titles) | Read the result reports instead (option 1B) | S2-09 P1 |
| A setting key equals a host setting (`applicationName`, `environment`, `contentRoot`) | It reads differently under `WebApplicationFactory`; rename it | S2-02 P1 |
| First deployment of a new module schema | The database role needs `CREATE` on the database, otherwise the API stops at startup | S2-07 P5 |
| Dependabot rebases a PR after the CI review | Check the head commit at merge time | S2-04 P5 |
| E2E timeouts show up in CI | Raise `workers` or timeouts, not before | S1-19 P7, S1-20 P6 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |
| A test mixes seed rows with fixed and generated ids in one table | Advance the identity sequence in the test | S1-15 P4 |
| Docker builds fail on the developer machine | Free disk space (E2E images ≈ 2 GB) | S1-13 P1, S1-18 P7 |
| The next version of SPEC, ARCHITECTURE, Handoff or UX copy is written | Update its references to the current versions of the other documents | S1-21 P2, S2-16 P6 |

### 9.6 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`: move done items to "Done", add new findings from the sprint protocols and reviews, re-score, check milestones of new issues.
- **Start of every migration step:** open its GitHub milestone and plan its debt together with the step's invariants (§9.2 – §9.4); delete its `LEGACY_EXCEPTIONS` entries as files are replaced (#158).
- **End of every migration step:** review the axe suppression list (TESTING §12.4); add rebuilt screens to `reflow.spec.ts`.
- **Weekly:** Dependabot pull requests — licence check of new versions before merging (CLAUDE §10); CodeQL alerts.

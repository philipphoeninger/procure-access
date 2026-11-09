# ProcureAccess — Tech-debt register

> **Status:** v3 (2026-11-10) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 03, made with the skill `Engineering/tech-debt` (inventory → categorise → score → plan). Replaces `TECH_DEBT_v2.md`.
> **Sources:** open and closed GitHub issues (2026-11-10), the open points and won't-fix findings of the 28 protocols `docs/sprints/sprint_03/S3-*.md`, `SPRINT_03.md`, `ROADMAP_v3.md`, a scan of the code (TODOs, `DateTime.UtcNow`, suppressed warnings, `eslint-disable`, `LEGACY_EXCEPTIONS`), a Release build of `ProcureAccess.sln` with `-warnaserror` (0 warnings), `dotnet list package --vulnerable --include-transitive` (none) and `--outdated`, `npm audit` / `npm outdated` (client and `e2e/`), Dependabot and CodeQL alerts, the CI runs on `main` since 2026-11-05, open pull requests and the ruleset of `main`.
> **Next review:** end of sprint 04 → `TECH_DEBT_v4.md`.

---

## 1. Summary

- **Sprint 03 cleared the v2 backlog that was due.** 21 issues closed, including the only High advisory (AutoMapper, #31) and every item v2 §9.1 planned for this sprint (§4). About **15 of the sprint's 48 points were debt** (≈ 31 %): 9.5 committed (S3-01 – S3-08, #153 in S3-19) plus all five stretch tasks (S3-22 – S3-26, 5.5).
- **30 items are open and scored** (§3): 14 carried over from v2, 2 issues created during the sprint (#203, #205), and 14 new items from the protocols and this scan (**TD-24 – TD-37**). #158 (lint exceptions) shrinks with the steps (§5), #102 is for the manual pass (§9.4), #181 is the owner's UX copy session (§6).
- **The top item is CI reliability (#203, priority 28).** 13 of 38 CI runs on `main` since 2026-11-05 ended red. In 12 of them an E2E job timed out while installing Playwright's system packages from apt, before any test ran. A red `main` that is "just the flake" hides the first real failure.
- **Nothing breaks a product rule (CLAUDE §8).** The NuGet graph has no known vulnerability, the client and `e2e/` have none in shipped code; the only open Dependabot alerts are the dev-only `minimatch` (TD-17, now in both lock files). The build has 0 warnings.
- **Two security items are cheap and worth doing now:** `UsersController` has no `[Authorize]` (#205; anonymous calls answer 400 and change nothing today), and the deployed staging stack should be rebuilt from `main` and its `ASPNETCORE_ENVIRONMENT` checked (TD-28), because the Swagger guard, SEC-06 and the removal of the anonymous CRUD writes only take effect there after a deployment.
- **CodeQL alert 3 is still open** although #182 is closed (TD-24): the analysis on `main` does not accept the guard S3-04 added before the loop.
- **Documentation drifts again** (TD-37): ARCHITECTURE_v27 still says "v26" in its status line, TESTING names ARCHITECTURE_v25 as input, ROADMAP still shows M1.1d open, and SPEC / Handoff / Figma mapping / PRD / Kriterienkatalog name outdated versions.
- **Sprint 04 (configurator S10) depends on two debt items:** an active catalog release in the E2E stack (TD-35) and the M2.5d rows that ROADMAP_v3 already counts (#52, J1 and CAT-10 rewrite incl. #91, #92, `vitest-axe`).
- **Two corrections to v2:** #118 had 104 warning locations and 22 `VSTHRD200` (v2 said 55 and 2, S3-08 P1); `LEGACY_EXCEPTIONS` holds **32** files (M2 5, M3 16, M5 11), not 10 — the list has not changed since S2-15.

## 2. Method

Unchanged from v1 §2.

**Categories:** Code · Architecture · Test · Dependency · Security/Privacy · Accessibility · CI/Ops · Documentation.

**Scores** (1 = low, 5 = high): *Impact* (hurts users, product rules or development speed today) · *Risk* (security, legal, data, handover if it stays) · *Effort* (1 ≈ under an hour, 3 ≈ one session, 5 ≈ several sessions or a migration step).

**Priority = (Impact + Risk) × (6 − Effort)**, from 2 to 50. Above 25 → fix in the next sprint; 15–25 → schedule with the named step; below 15 → fix when the code is touched anyway.

**When:** *now* (sprint 04), a migration step / milestone (`ARCHITECTURE §12`, `ROADMAP`), *touch* (when the code changes anyway), or *watch* (act only on a trigger, §9.5). Re-scored items are marked ↑ or ↓ against v2.

## 3. Register (open items)

### 3.1 Defects users can see

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-25 (#234) | Legacy settings store: writes the system appearance to `localStorage` for signed-out users (R-SET-02: no browser storage), opens an English snackbar on a `prefers-contrast` change (CLAUDE §6.5), and its init throws when storage is blocked, so products never load; `reloadUICustomization` throws on a `null` answer (S3-09 P3, S3-10 P2, S3-02 R4) | Accessibility | 3 | 2 | 2 | 20 | step 3 (S60) |
| #52 | Stepper steps on `/filters` have no accessible name (the only axe suppression) | Accessibility | 3 | 2 | 2 | 20 | sprint 04 (S10 replaces the screen) |
| TD-26 (#234) | `.text-error` on the legacy delete-account dialog title uses `--mat-sys-error-container` as text colour: low contrast in both appearances (S3-09 P2) | Accessibility | 2 | 1 | 1 | 15 | touch, at the latest step 3 (account screens) |
| #170 | Legacy login outlet and product list / detail do not reflow at 320 px / 400 %. Owner: severity `minor`, no legacy fix | Accessibility | 2 | 1 | 3 | 9 | step 3 (S01), step 6 (S40, S41) |

### 3.2 Security and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-28 (#236) | Deployed instances lag behind `main`: the anonymous generic CRUD writes stay open until #201 is deployed (S3-07 P3); the Swagger guard and SEC-06 rely on `ASPNETCORE_ENVIRONMENT` being `Production`/`Staging` or unset in Dokploy (S3-06 P1); the client image must be built after 2026-10-14 to carry nginx ≥ 1.30.6 (S3-01 P2) | Security | 2 | 3 | 1 | **25** | now (owner: redeploy staging from `main`, check the variable) |
| #205 | `UsersController` has no `[Authorize]`: change-email, change-password, revoke-sessions and `DELETE /api/Users` accept anonymous requests and answer 400 (nothing changes) instead of 401; the user id is silenced with `!` (S3-08 P2, R2) | Security | 2 | 3 | 1 | **25** | now (small fix with tests) or step 3 at the latest |
| TD-27 (#235) | Data Protection keys are stored unencrypted in `identity."DataProtectionKeys"` (owner 3a), so the database and its backups hold them; key store, backup and restore are not documented (S3-24 P2) | Security | 1 | 3 | 2 | 16 | before G-P, with `TECHNICAL.md` (#4) |
| #112 | `/health/ready` is public on the API port and opens a database connection per request; timeout is a code constant | Security | 2 | 2 | 2 | 16 | M2.7, with the first rate limit |
| TD-12 | The E2E job's `compose.log` artefact could contain per-run secrets; harmless while they are random | Security | 1 | 2 | 1 | 15 | watch at step 3 (Mailpit, real secrets) |
| TD-22 (#187) | Licence notices of the NuGet packages in the API image not checked | Dependency | 1 | 2 | 2 | 12 | before G-P |
| TD-29 | 🔧 Criteria-suggestion channel is the placeholder `mailto:kriterium-vorschlagen@example.invalid` until O-6; `check-release-config.mjs` catches it, but no release pipeline runs the check yet (S3-21 P2, P3) | CI/Ops | 1 | 2 | 2 | 12 | with the release pipeline, before G-P; O-6 from the project team |
| TD-17 | `minimatch` GHSA-g3hv-75wg-m249 (moderate) inside `eslint` / `typescript-eslint`, now Dependabot alerts 12 (client) and 13 (`e2e/`); dev tool, not shipped. Dependabot's own fix attempt for `e2e/` failed on 2026-11-09 | Dependency | 1 | 1 | 1 | 10 | watch: take the ESLint release that moves to minimatch 11 |

### 3.3 Code, architecture and design

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-34 (#240) | Theme colours are generated: the light values differ from Handoff §1.2 (e.g. `error-container` `#FFDAD5` vs `#F9DEDC`), the dark values are not reviewed (e.g. `on-surface-variant` ≈ `on-surface`). A clean axe run does not replace the design review (S3-09 P1, P6; S3-10 P4) | Accessibility | 2 | 2 | 2 | 16 | owner decision O-S5 (Figma dark mode), before G-R |
| TD-30 (#237) | No field for the short description (Kurzbeschreibung, Kriterienkatalog §6.1; export column SPEC §8.4) in `CriterionRevision` or the release format; ARCHITECTURE_v27 does not have it (S3-27 P2) | Architecture | 3 | 2 | 3 | 15 | owner decision before the O-1/K-1 texts arrive, at the latest M2.7 (export) |
| TD-31 (#238) | `Criterion.SupersededByKey` holds one key, so a split cannot be stored; ARCHITECTURE §5.1 (matching table) and TESTING (CAT-03/CAT-04 "split, merge and chained" cases) still assume splits (S3-14 P2) | Architecture | 1 | 3 | 3 | 12 | owner decision before step 6 (CAT-03/04) |
| #42 | Bundle budget raised to 2 MB / 2.5 MB; tighten after the rebuild. The icon font no longer counts (S3-25 P4) | Code | 2 | 1 | 2 | 12 | end of M2 |
| TD-32 | Dead code: `core/services/dialog.service.ts` is fully commented out and still holds English literals (S3-22 P1); `Logger` in `BaseCrudController` unused since #201 (S3-07 R4); `nginx.conf` `location ^~ /assets/` serves nothing since S3-09 (R4) | Code | 1 | 1 | 1 | 10 | touch |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-35 (#241) | No stack has an active catalog release: dev, E2E and staging answer 404 on `/api/v1/catalog/*`, and 2026.1 cannot be activated before O-1. S10 needs one in the E2E stack — e.g. the E2E seeder imports and activates the test folder `T.1` (TESTING §6.3) (S3-14 P6, S3-17 P4, S3-19 P1, S3-20b P1, S3-27 P6) | Test | 3 | 2 | 2 | 20 | sprint 04, first task of S10 (M2.5d) |
| #87 | Session probe must answer 200 for anonymous and unknown test users | Test | 2 | 3 | 3 | 15 | step 3 |
| #88 | Integration host option for the real Identity cookie flow (SEC-01/02/09) | Test | 2 | 3 | 3 | 15 | step 3 |
| #89 | Test that the antiforgery helper's session probe carries the caller's headers | Test | 1 | 2 | 1 | 15 | step 3 |
| #82 | Regression tests for the S1-10 zoneless fixes (Z1–Z5, R1–R5) | Test | 2 | 2 | 3 | 12 | touch (screens are rebuilt in M2–M3) |
| #92 | CAT-10: `ExistingCatalogSeed` can drift from the migration seed | Test | 1 | 2 | 2 | 12 | sprint 04 (CAT-10 rewrite) |
| #91 | CAT-10: comparison key ignores JSON value types | Test | 1 | 1 | 1 | 10 | sprint 04 (CAT-10 rewrite) |

### 3.5 CI and tooling

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #203 ↑ | E2E jobs fail at random while installing Playwright's system packages from apt: 12 of 38 CI runs on `main` since 2026-11-05 timed out in that step (one more, the merge of #219, failed in the aggregate check without WebKit results). Re-running clears it, but `main` is red a third of the time. Options in the issue (§6) | CI/Ops | 4 | 3 | 2 | **28** | now (first task of sprint 04) |
| TD-33 (#239) | A release folder is validated in CI only through a test that names it (`Release2026_1Tests`). The project team's first PR for a new version (or for the 2026.1 priorities) gets its validation errors only from a local run. Proposal: a CI step or `catalog validate <version>` / `--dry-run` over every folder under `catalog/releases/` (S3-16 P5) | CI/Ops | 2 | 2 | 2 | 16 | before the project team's first release PR (O-1) |
| TD-24 (#233) | CodeQL alert 3 (`js/prototype-pollution-utility`, `scripts/check-i18n.mjs:174`) is still open on `main` (commit f1ed294) although #182 is closed: the analysis does not count the guard before the loop as a barrier. Fix: guard `name` inside the loop as well (S3-04 R2, P1; S3-05 P6) | CI/Ops | 1 | 2 | 1 | 15 | now (quick win) |

### 3.6 Documentation and licences

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-37 (#243) | Sprint-03 findings not yet in the documents. **ARCHITECTURE:** v27 status line says "v26 —"; `Category` without meaning in §5.1 (S3-12 P5); split vs one `SupersededByKey` (with TD-31). **TESTING:** input names `ARCHITECTURE_v25.md` (S3-23 P3, S3-24 P4); §8.2 stage 9 and V85 say CodeQL findings do not block, but the ruleset now blocks on `high_or_higher` (S3-03 P1). **SPEC:** R-SHL-10 should name A11Y-09 (S3-05 P4); §11 step-1 invariants lack A11Y-08/09 and the R-SET-03 "TESTING follow-up" clause is outdated (S3-10 P3); "current versions" line names ARCHITECTURE v24, TESTING v36, UX copy v15. **ROADMAP:** M1.1d still open and the warning count uncorrected (S3-08 P1); M2.3 "imported, not activated" (S3-27 P5: no kept database imports 2026.1 before O-1); GP.3 "production build fails on placeholders" (S3-21 P4); header names ARCHITECTURE v15, TESTING v23; sprint-03 velocity. **Handoff:** "UX copy v14" in §0, §5.3, §7; §1.3 "latin + latin-ext" (S3-09 P5); sources line. **Figma mapping:** body names `ARCHITECTURE_v10`, `TESTING_v8`; related line. **Owner documents:** PRD v5 and Kriterienkatalog headers; Kriterienkatalog §3.1 names `SampleData.cs` instead of `CriteriaFilterExclusionSeed.cs` (S3-11 P5, S3-18 P1); UX copy header (with #181). **SPRINT_03:** close-out table empty | Documentation | 3 | 3 | 2 | **24** | now (one documentation session; owner documents by the owner) |
| TD-36 (#242) | The CAT-10 fixture `cat-10-criteria-filter.json` quotes BIT inklusiv titles but is covered as MIT by the `**/*.json` block of `REUSE.toml` (S1-15 R3, S3-11 P3) | Dependency (licence) | 1 | 3 | 1 | 20 | now (one `REUSE.toml` block), at the latest with the CAT-10 rewrite |
| #4 | README is 18 lines; a successor needs setup, links to `docs/` and the process. `TECHNICAL.md` must also name the deployment variables (`ASPNETCORE_ENVIRONMENT`, S3-06 P1) and the key store backup (TD-27) | Documentation | 2 | 3 | 2 | 20 | step 16 (`TECHNICAL.md`), G-P |

## 4. Done in sprint 03

| Item (v2 ID) | What was done | Task |
|---|---|---|
| §6 `e2e/` TypeScript (O-S3-1); Dependabot PRs #164 – #166, #175 | `e2e/` follows the client's TypeScript; #165, #166 merged, #164, #175 closed; nginx stable-branch rule D-1 (even minors only) | S3-01 |
| #168, #171, #137 | Loading counter on failure; offline notice only from API answers; icon spans → `<mat-icon>` | S3-02 |
| #144 | CodeQL advanced setup, also on Dependabot PRs; `Analyze` checks and a code-scanning rule required again. **TD-01 note:** the required checks of `main` are stages 1–7 plus the three `Analyze` jobs | S3-03 |
| TD-20 (#185), TD-16 (#182) | Gate self-test in CI stage 1; prototype-key guard in `check-i18n.mjs` — the CodeQL alert itself stays open (→ TD-24) | S3-04 |
| TD-14 (#180), TD-23 (#188) | Sprint-02 findings in ARCHITECTURE, SPEC, TESTING; reflow invariant A11Y-09 `active` | S3-05 |
| TD-18 (#183) | Swagger UI and OpenAPI documents only in `Development` (O-S3-2) | S3-06 |
| #31, #90 | AutoMapper replaced by hand-written maps (ADR-015), ARC-06 reworded; generic CRUD writes removed (they accepted anonymous requests); Dependabot alerts 1, 8, 9, 10 closed | S3-07 |
| #118 | 0 build warnings, analyzer check on every project, `-warnaserror` in CI, `TreatWarningsAsErrors` in `src/` | S3-08 |
| TD-19 (#184), #192 | Theme foundation: light/dark, strong focus indicators, Roboto, reduced motion; `@angular/animations` removed | S3-09 |
| — | A11Y-08 (development part): axe in both appearances | S3-10 |
| #153, #176 | `/api/v1` as URL segment only; Swagger through `DescribeApiVersions` | S3-19 |
| #160 | Snackbar button „Schließen“ (O-S3-9) | S3-22 |
| #115 (incl. S1-23 P4) | Usage events fire-and-forget through a bounded queue | S3-23 |
| TD-21 (#186) | Data Protection keys in `IdentityAccessDbContext` | S3-24 |
| #135 | Material Symbols subset (24 kB instead of 4 MB) with an icon check (O-S3-7) | S3-25 |
| #161 | ESLint for `e2e/` and `scripts/*.mjs`, without `eslint-plugin-playwright` (O-S3-8) | S3-26 |
| §5 row "CAT-10 seed rows 2 and 3" | Seed exclusions HR-Anwendung → Mobile Anwendung removed | S3-18 |

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design. Do not fix it on its own; replace it with the step. New in v3 are marked *(new)*.

| Debt | Retires with |
|---|---|
| JWT in `localStorage`, `jwtDecode` with `any` in `auth.service.ts` (SEC-01) | step 3 (M3) |
| `DateTime.UtcNow` ×6 (`UserRepo` ×1, `UserService` ×4, `ProposalService` ×1) against CLAUDE §7 (unchanged; the hits in `tests/…/Operations/` are wall-clock deadlines of polling loops and the Serilog stamp, which is fine) | `UserService`/`UserRepo` with step 3, `ProposalService` with step 5 |
| `Microsoft.AspNetCore.Identity.UI` only for the `IEmailSender` interface | step 3 |
| `UsersController` and the identity minimal APIs answer business failures with `{ code, message }`; `signUp` / `reset-password` answer 500 on a missing field; dead key `UICustomizationNotFound` in `ERROR_TRANSLATIONS` | step 3 |
| *(new)* The Identity tables move into `IdentityAccessDbContext` by ADR-014 rule 4 (S3-24 P5); OPS-04 two-host cookie test then `active` (S3-24 P1) | step 3 |
| *(new)* Deferred invariant parts: A11Y-08 with the explicit setting and its persistence — a signed-in journey makes the axe helper fail with „did not switch“ (S3-10 P1); CAT-06 and CAT-21 for stored configurations (step 3); CAT-16 deselection and export part (M2.6, M2.7); CAT-20 FE part (S11) (S3-20b P6) | their steps |
| *(new)* `mat-menu` (profile) and `matTooltip` not checked by hand since `@angular/animations` went (S3-09 P4); Material reads `prefers-reduced-motion` once at start, the explicit motion setting comes with S60 (S3-09 R5) | step 3 (first signed-in screen, S60) |
| *(new)* Callers of the fixed legacy store methods do not catch the rejection, so a failed load ends as an unhandled rejection in the console (S3-02 R1) | M3, M5 rebuilds |
| *(new)* Legacy `<strong>` in `mat-error` texts is drawn as synthetic bold (only Roboto 400/500 ship) (S3-09 R1) | step 3 / M5 forms |
| Delete-account „Löschen“ uses native `disabled` instead of `aria-disabled` with a visible reason | step 3 |
| Legacy login outlet replaces the whole shell; does not reflow (#170 L1) | step 3 (S01) |
| `[Produces("application/json")]` on legacy controllers; *(new)* legacy controllers keep `[ApiVersion("1.0")]` (S3-19 P4) | each replacing step |
| *(new)* The snackbar is still the error channel for non-5xx errors, against UX copy §4.1 („nie für Fehler“) (S3-22 P2) | with its last legacy caller, steps 2–6 |
| Legacy routes without a page-title key and without an H1 (`/filters`, `/products`, …) | rebuild of each screen |
| Three legacy `LiveAnnouncer` calls outside `A11yAnnouncerService` | steps 2 and 5 |
| The API sends English validation messages without codes | each step's module validation |
| Uppercase legacy roots in `de.json` | rebuild of each screen (CLAUDE §9) |
| `LEGACY_EXCEPTIONS` in `eslint.config.js`: **32 files** — M2 5, M3 16, M5 11 (#158; v2 said 10) | each step deletes its entries; #158 closes when the list is empty |
| *(new)* Icon list: the stepper entries under `material` stay until the legacy `/products` and `/filters` screens go (S3-25 P2) | S10 / S11 |
| Legacy product reads ignore `Product.IsDeleted`; `GetByCriteriaFilterIds` lacks the proposal filter | step 6 |
| `POST /api/Proposals/approve` without the permission ends in a 500; *(new)* `ProposalService` `dto!.ToEntity()` throws on a null snapshot and `!` on `user` in `ApproveAsync` (S3-07 R3, S3-08 R2) | step 5 (proposal retirement) |
| The tagged API-02 test sits on the legacy proposal form | move to a rebuilt form before step 5 retires the proposal |
| *(new)* No backend test of the legacy seeded exclusions, only J1 (S3-18 R1) | steps 5/6 (legacy tables go) |
| ~25 TODOs in legacy client features and DAL repos (28 hits today, all in legacy code) | features retired or rebuilt in M2–M5 |
| `vitest-axe` not installed | first K-component (sprint 04) |
| E2E without `E2E` environment, seeder, Mailpit, clock endpoint, relaxed rate limit (seeder part → TD-35) | steps 2–4 |
| CAT-10 on `ExistingCatalogSeed` and the old filter | sprint 04 (S10) |
| `Approver` role, `withProposal()`, `withProducts()`/`withFavorites()` | per `ARCHITECTURE §8.1` |
| *(new)* Legacy projects (`API`, `DAL`, `MODELS`, `SERVICES`) fail on a warning only in CI, not locally; they inherit `TreatWarningsAsErrors` when their code moves into `src/` (S3-08 P4) | each migration step |

Not tracked on purpose (unchanged): CI has no NuGet/Docker cache; Serilog stamps with the system clock; files without logic count for coverage. *(new)* Won't-fix findings of the sprint-03 reviews that need no follow-up are listed in their protocols only (e.g. S3-12 R1 cast-back of read-only lists, S3-16 R1 concurrent imports, S3-19 R2 cache without lock, S3-23 R2/R3 queue loss and retries, S3-24 R2–R4 key-ring details, S3-25 R2/R3, S3-26 R1/R2).

## 6. Decisions waiting for the owner

CLAUDE §6.6: Claude Code must not decide these. Decided in sprint 03 and moved to §4: O-S3-1 – O-S3-9 (e2e TypeScript, Swagger, reflow invariant, AutoMapper, catalog persistence, endpoint style and #176, font subsetting, `eslint-plugin-playwright`, snackbar wording).

| Decision | Source | Needed by | Claude's proposal |
|---|---|---|---|
| **#203** E2E flake: (1) run the E2E jobs in the pinned Playwright container image, (2) shorter limit plus one retry of the apt step, or (3) cache the `.deb` packages | #203 | start of sprint 04 | (2) first — smallest change; (1) if `main` still goes red (licence check of the image, CLAUDE §10) |
| **UX copy session #181** (TD-15) — still open; now also the UX key for `selection-excluded` (S3-20a P3), whether `suggest.mailBody` uses real line breaks instead of `%0A%0A` (S3-21 P1), and the UX copy header versions. The snackbar row is done (O-S3-9) | S2-11 – S2-18, S3-20a, S3-21 | before S11 (sprint 04/05); S10 does not wait | — |
| **TD-30** Short description: a field in `CriterionRevision` and the release format, or derived from existing text? | S3-27 P2 | before the O-1/K-1 texts arrive | ADR in the next ARCHITECTURE version |
| **TD-31** Splits: allow several successors (`SupersededBy` as a set) or reword ARCHITECTURE §5.1 and CAT-03/04 to merges and 1:1 only | S3-14 P2 | before step 6 | — |
| **TD-34** Theme colours (O-S5): pin the Handoff light values, update Handoff §1.2 to the generated ones, or wait for the Figma dark mode | S3-09 P1, P6 | before G-R | wait for the Figma dark mode (option c of S3-09) |
| **TD-36** CAT-10 fixture: annotate it as BIT inklusiv in `REUSE.toml` like the seed and the release folders | S3-11 P3 | sprint 04 | yes, one block |
| A release with gaps stays stored; `filter-options?release=<version>` serves it while `/criteria` answers 404. Refuse such a release at import instead? | S3-20b P2 | when 2026.1 is imported anywhere | keep (O-S3-5 Q2 (iii) accepts it); revisit if it confuses the project team |
| **TD-33** CI validation of every release folder, or a `catalog validate` command | S3-16 P5 | before the first release PR of the project team | CI step that runs the validator on every folder |
| Project team: priorities, rationales and plain-language titles (O-1, K-1, K-6) — the gap list `kriterienkatalog_2026_1_luecken_v1.md` is the work list; one product part per criterion (K-4); keep or drop the three „nicht anwendbar“ PDF steps (Z-4); the 10.1.3.1c remark (Z-3); the criteria-suggestion channel (O-6) | S3-27 P1, P4, R2; S3-20a P4; S3-21 P2 | M2.3; O-6 before G-P | — |
| O-22: may a `Contracts` project reference another module's `Contracts`? | S2-08 P4 | first contracts type that needs it | — |
| Pages without a Figma frame (S04, S80, legal pages); base frame of S35 is the „gesperrt“ state | S2-22 P2, P3 | design, any time | — |

## 7. Issue hygiene

- **Milestones set on 2026-11-10:** #203 → M2 (it had none); #181 → M2 (the only open issue of M1; it is needed before S11). M1 then has no open issue — the owner may close the milestone.
- **Labels:** all open debt issues carry `tech-debt` except #4 (`documentation`), #52 (`bug`, `a11y`), #102 and #181 (`question`), which is correct.
- **Closed issue, open alert:** #182 is closed while CodeQL alert 3 stays open (→ TD-24 with its own issue).
- **Open Dependabot PRs** for the weekly review (§9.6): #226 (`@playwright/test` 1.65 in `e2e/`), #227 (Angular group, 12 updates), #228 (Vitest group), #229 (other group), #230 (**jsdom 31, a major**), #231 (dotnet group: xunit.v3 4.0.3).
- **Remote branches** of the merged sprint-03 PRs are deleted (S3-15 P8, S3-16 P6, S3-17 P5, S3-18 P2).

## 8. New issues

Created on 2026-11-10 for the **TD-** items that need one:

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-24 | #233 CodeQL alert 3 still open: guard the loop variable in `setText` | tech-debt | M2 |
| TD-25, TD-26 | #234 Legacy settings store and delete-account dialog: storage, English snackbar, crash on blocked storage, contrast | a11y, tech-debt | M3 |
| TD-27 | #235 Encrypt the Data Protection key ring at rest and document backup and restore | .NET, tech-debt | G-P |
| TD-28 | #236 Staging: redeploy from `main` and check `ASPNETCORE_ENVIRONMENT` | question, tech-debt | M2 |
| TD-30 (§6) | #237 Decide: where the criterion short description lives in the catalog model | question | M2 |
| TD-31 (§6) | #238 Decide: criterion splits vs one `SupersededByKey` | question | M6 |
| TD-33 | #239 Validate every catalog release folder in CI | tech-debt | M2 |
| TD-34 (§6) | #240 Decide: theme colours — generated values vs Handoff §1.2, dark values not reviewed (O-S5) | a11y, question | G-R |
| TD-35 | #241 E2E stack: import and activate a test catalog release (needed by S10) | tech-debt | M2 |
| TD-36 | #242 REUSE: annotate the CAT-10 fixture as BIT inklusiv | tech-debt | M2 |
| TD-37 | #243 Carry the sprint-03 findings into the documents and their headers | documentation, tech-debt | M2 |

TD-12, TD-17, TD-29 (tracked by the release check and O-6) and TD-32 (touch) need no issue.

## 9. Fix plan — when to fix what

### 9.1 Sprint 04 — configurator S10 (M2.5) and its debt

ROADMAP_v3 already counts 4 points of M2.5d debt for this sprint. New debt budget **≈ 3.5 points**.

| # | Work | Items | Pts | Note |
|---|---|---|---:|---|
| 1 | E2E reliability: the option the owner picks | #203 | 0.5–1 | first, so the sprint's merges are judged on real failures |
| 2 | Small security fix: `[Authorize]` on `UsersController`, `[AllowAnonymous]` on `ResetPassword`, 401 tests, `!` removed | #205 | 0.5 | pulls a step-3 item forward; touches one controller |
| 3 | Scripts and licence PR: guard inside the loop of `setText`; CAT-10 fixture block in `REUSE.toml` | TD-24, TD-36 | 0.5 | |
| 4 | Documentation session | TD-37 | 1 | Claude: ARCHITECTURE, TESTING, SPEC, ROADMAP v4, Handoff, Figma mapping, SPRINT_03 close-out; owner: PRD, Kriterienkatalog, UX copy (#181) |
| 5 | Release-folder validation in CI | TD-33 | 0.5 | after the owner's decision (§6) |
| — | With S10 (M2.5d, already counted): an active release in the E2E stack (seeder imports and activates `T.1`); #52; J1 and CAT-10 rewrite incl. #91, #92; `vitest-axe`; the configurator's `LiveAnnouncer` calls; the stepper icons in the icon list | TD-35, #52, #91, #92 | 0 | TD-35 is the first S10 task |
| — | Owner: redeploy staging from `main` and check `ASPNETCORE_ENVIRONMENT`, the nginx version and the anonymous writes; §6 decisions; milestones of §7 | TD-28 | 0 | |
| — | Touch: `dialog.service.ts`, the unused `Logger`, the `/assets/` location | TD-32 | 0 | with any PR in those files |

### 9.2 Rest of M2 — criteria list S11 (M2.6) and export (M2.7)

| Fix | Items | Why then |
|---|---|---|
| With S11 | Render the Markdown lines of the texts as paragraphs and lists (S3-15 P6); map `matchedRule.kind` `appTypeAndUseCase` → `criteria.priority.rule.full` (S3-20b P4); decode the stored `%0A%0A` of `suggest.mailBody` or take real line breaks from #181 (S3-21 P1); first `CriteriaListShown` caller (S3-23 P1); CAT-20 FE part; the `selection-excluded` UX key; if the PDF test's rating examples read badly, a field `ratingExamplesDe` (S3-27 P3); `pageTitle` as an `AREAS` root and `shell.pageTitle.error` (M2.6d) | The screen and its texts are built |
| With the export (M2.7) | Short description column (TD-30, after the decision); CAT-16 export part; `ExportCreated` caller; #112 with the first rate limit; EXP-05 log rule; E2E relaxed rate limit; ARC-03 rule for `AdHocSelectionSource` | Same pipeline |
| Each rebuilt screen | New icons into the icon list and regenerate the subset (S3-25 P1); join `reflow.spec.ts` and name the reflow test in the task prompt (S3-05 P3); delete its `LEGACY_EXCEPTIONS` entries | The gate cannot see these (A11Y-09) |
| End of M2 | #42 bundle budget | After the screen rebuild |

### 9.3 M3 – M7

| Milestone | Fix | Items |
|---|---|---|
| M3 | Legacy settings store replaced by S60: no browser storage, no English snackbar, no crash on blocked storage | TD-25 |
| M3 | Account screens rebuilt: delete-dialog contrast, `aria-disabled` reason, `mat-menu` / `matTooltip` check | TD-26, §5 |
| M3 | Cookie session replaces the JWT; Identity tables into `IdentityAccessDbContext`; OPS-04 two-host test; `Identity.UI`, `DateTime.UtcNow` in `UserService`/`UserRepo`, `{ code, message }` failures; #205 if not done in sprint 04 | §5, #205 |
| M3 | Integration host for the real cookie flow; session probe | #87, #88, #89 |
| M3 | Mailpit and real secrets in the E2E stack: check the `compose.log` artefact | TD-12 |
| M3 | A11Y-08 with the explicit setting; CAT-06 / CAT-21 for stored configurations; login outlet reflow | §5, #170 L1 |
| M2 – M3 | Regression tests for the zoneless fixes, screen by screen | #82 |
| M4 | First time-dependent tests; E2E clock endpoint for J5; S34 child routes in Figma mapping §2.2 and `PAGES` | — |
| M5 | Proposal retirement: `ProposalService`, `withProposal()`, `Approver`, `approve` 500, null snapshot; move the API-02 test first | §5, #158 M5 group |
| M6 | Splits decided before CAT-03/04 (TD-31); directory rebuild: `IsDeleted` and proposal filter; product list and detail reflow | TD-31, #170 L2, L3 |
| M7 | File-store check in `/health/ready` | — |

### 9.4 Before the pilot (G-P) and the public release (G-R)

| Fix | Items |
|---|---|
| README and `TECHNICAL.md` (process step 16): setup, deployment variables, key store backup and restore, the indicator queries of R-MET-02 | #4, TD-27 |
| Encrypt the key ring at rest, or record why not | TD-27 |
| Licence notices of the API image | TD-22 |
| Release pipeline that runs `check-release-config.mjs`; O-6 channel and the legal pages (GP.3) in the check | TD-29 |
| The axe suppression list is empty (TESTING §12.4) | #52 and any later entry |
| Manual accessibility pass (TESTING §7.2, A11Y-07): screen readers on H1 focus and announcements, offline notice, real browser zoom, autocomplete on phones | #102 |
| G-R: theme colours reviewed and pinned with the Figma dark mode | TD-34 |

### 9.5 Watch items — act only when the trigger happens

| Trigger | Then | Source |
|---|---|---|
| ESLint / typescript-eslint release accepts minimatch 11 | Take it (Dependabot); close alerts 12 and 13 | TD-17 |
| A Dependabot PR updates `material-symbols` | It stays red in stage 1 until `scripts/subset-material-symbols.sh` runs on its branch | S3-25 P3 |
| `eslint` / `typescript-eslint` versions drift between `e2e/` and the client and it hurts | One Dependabot group for both | S3-26 P3 |
| The first real C# CodeQL finding, or a missed one | Revisit `build-mode: none` for C# | S3-03 R2 |
| No push to `main` for 60 days | GitHub disables the weekly CodeQL schedule; re-enable it | S3-03 R4 |
| A module needs a map that is tedious by hand | ADR-015 rule 4: revisit Mapperly | S3-07 P4 |
| The API is meant for third parties | Revisit O-S3-2 (Swagger only in `Development`) | S3-06 P3 |
| A hotfix version older than the newest release is imported | „Previous release“ is the one stored last; check `supersedes` by hand | S3-16 P2 |
| A missing setting confuses someone running `catalog …` | Registration errors end with the runtime's exception, not exit 1 | S3-16 P4 |
| A size limit on `IMemoryCache` (D10, scale-out) | Give the catalog entries a size | S3-17 P2 |
| More than one API instance | Migrations, log retention, the usage-event queue and the first Data Protection key assume one instance | S3-23 D3, S3-24 R4 |
| English catalog names exist | `Accept-Language` for catalog names (ARCHITECTURE §7.1) | S3-19 P2 |
| A document defines a range for `PriorityRecommendation.Weight` or `Level`/`Source` | Add the CHECK constraints | S3-13 R6 |
| An icon is chosen by `fontIcon`, `svgIcon` or a CSS class | Extend the icon check | S3-25 R4 |
| A future SDK breaks NetArchTest | Fork `NetArchTest.eNhancedEdition`, with an ADR | S2-05 P3 |
| One of the gate's known limits bites | Read the result reports instead (option 1B) | S2-09 P1 |
| A setting key equals a host setting | Rename it | S2-02 P1 |
| First deployment of a new module schema | The database role needs `CREATE` on the database | S2-07 P5 |
| Dependabot rebases a PR after the CI review | Check the head commit at merge time | S2-04 P5 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |
| A test mixes seed rows with fixed and generated ids in one table | Advance the identity sequence in the test | S1-15 P4 |
| Docker builds fail on the developer machine | Free disk space (E2E images ≈ 2 GB) | S1-13 P1, S1-18 P7 |
| The next version of SPEC, ARCHITECTURE, Handoff or UX copy is written | Update its references to the current versions of the other documents | S1-21 P2, S2-16 P6 |

### 9.6 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`: move done items to "Done", add new findings from the sprint protocols and reviews, re-score, check milestones of new issues; check that every issue closed for a CodeQL or Dependabot alert really closed the alert.
- **Start of every migration step:** open its GitHub milestone and plan its debt together with the step's invariants; delete its `LEGACY_EXCEPTIONS` entries as files are replaced (#158).
- **Every screen task:** new icons into the icon list; the screen joins `reflow.spec.ts` (named in the task prompt).
- **End of every migration step:** review the axe suppression list (TESTING §12.4).
- **Weekly:** Dependabot pull requests — licence check of new versions before merging (CLAUDE §10); nginx: merge even minors (stable branch), close odd ones (S3-01 D-1); CodeQL alerts; the CI runs on `main` (a red run is a finding, not noise).

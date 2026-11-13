# ProcureAccess — Tech-debt register

> **Status:** v4 (2026-11-13) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 04, made with the skill `Engineering/tech-debt` (inventory → categorise → score → plan). Replaces `TECH_DEBT_v3.md`.
> **Sources:** open and closed GitHub issues (2026-11-13), the open points and won't-fix findings of the 31 protocols `docs/sprints/sprint_04/S4-*.md`, `SPRINT_04.md`, `ROADMAP_v4.md`, a scan of the code (TODOs, `DateTime.UtcNow`, suppressed warnings, `eslint-disable`, `LEGACY_EXCEPTIONS`, document versions named in code), a Release build of `ProcureAccess.sln` with `-warnaserror` (0 warnings), `dotnet list package --vulnerable --include-transitive` (none) and `--outdated`, `npm audit` / `npm outdated` (client and `e2e/`), Dependabot and CodeQL alerts, the 46 CI runs on `main` since 2026-11-10, open pull requests and the ruleset of `main`.
> **Next review:** end of sprint 05 → `TECH_DEBT_v5.md`.

---

## 1. Summary

- **Sprint 04 cleared the v3 items it planned, except TD-28 (an owner deployment) and #52 (now tied to the legacy filter).** 15 debt issues were closed: #203, #181, #205, #233, #242, #239, #243, #241, #237, #235, #187, #112, #91, #92 and #42. TD-32 and the light half of TD-34 are also done (§4). About **13 of the sprint's ≈ 57 points went to explicit debt tasks** (≈ 23 %): S4-01 – S4-07, S4-29 and all five stretch tasks S4-24 – S4-28. About **9 more points** were M2 debt rows (M2.4d – M2.7d) carried inside the feature tasks.
- **CI is reliable again.** 39 of the 46 runs on `main` since 2026-11-10 were green. Of the other seven, three were cancelled by newer merges, one has been stuck in `queued` since 2026-11-11, and three failed in E2E. The apt timeouts of #203 are gone. Two of the three failures were test flakes: #269, now fixed, and **a new one (TD-38)**: on 2026-11-12, `s10-steps-2-4.spec.ts:115` matched the CDK live-announcer element as well as the visible line.
- **41 items are open and scored** (§3): 15 carried from v3, 9 rows for the 11 issues created during the sprint, and 17 new items from the protocols and this scan (**TD-38 – TD-54**).
- **The top item is the staging deployment (TD-28, priority 35).** Sprint 04 added three things that only take effect with a deployment the owner has to prepare:
  - The API stops at startup without `DATA_PROTECTION_CERTIFICATE` (S4-26).
  - The token-less password reset was reachable there until #249 was merged. Its traces should be searched in the logs (S4-03 P1).
  - The rate limiter trusts `X-Forwarded-For` from private networks, so the API port must not be reachable from outside (S4-21a P2).
- **Two legacy account bugs affect anyone who registers on staging:**
  - #302: confirmation and reset mails go to Lettermint's test address.
  - #301: registration fails where the `Member` role row is missing.

  Both are cheap and belong at the start of M3.
- **The local dev server is broken (#264).** `npm start` fails on an optional import of the legacy `ngx-markdown`. The fix also takes ≈ 75 kB out of the first download (S4-22 P4).
- **Nothing breaks a product rule (CLAUDE §8).** No known vulnerability is in shipped code:
  - NuGet has none.
  - `npm audit --omit=dev` finds 0 in both packages.
  - CodeQL has no open alert.
  - The only Dependabot alerts are the dev-only `minimatch` (TD-17).

  The build has 0 warnings.
- **The M2 exit is incomplete outside development.** Still open:
  - the screen-reader check of the exported XLSX (X2);
  - the manual A11Y-07 pass of step 2 (X3);
  - the 2026.1 priorities (O-1/K-1, X6).

  TD-49 collects the manual checks; the priorities wait for the project team.
- **Documentation drifts again (TD-50).** The SPEC header lists ARCHITECTURE v28, TESTING v40 and UX copy v16 as current; the current versions are v46, v55 and v26. ROADMAP_v4 shows M2 in progress, and the Handoff still names `mat-stepper` for S10. ROADMAP v5 and the SPRINT_04 close-out are not written yet.
- **Corrections to v3:**
  - Two legacy `LiveAnnouncer` calls remain, not three (S4-12).
  - `LEGACY_EXCEPTIONS` still holds 32 files. The five M2 entries cannot go yet: the legacy `/filters` stays live wherever no release is active (O-S4-3).

## 2. Method

The method is unchanged from v1 §2.

**Categories:**
- Code
- Architecture
- Test
- Dependency
- Security/Privacy
- Accessibility
- CI/Ops
- Documentation

**Scores** (1 = low, 5 = high):

| Score | Means |
|---|---|
| *Impact* | How much it hurts users, product rules or development speed today |
| *Risk* | Security, legal, data or handover risk if it stays |
| *Effort* | 1 ≈ under an hour, 3 ≈ one session, 5 ≈ several sessions or a migration step |

**Priority = (Impact + Risk) × (6 − Effort)**, from 2 to 50.

| Priority | Then |
|---|---|
| above 25 | Fix in the next sprint |
| 15–25 | Schedule with the named step |
| below 15 | Fix when the code is touched anyway |

**When:**
- *now*: sprint 05.
- *a migration step or milestone* (`ARCHITECTURE §12`, `ROADMAP`).
- *legacy removal*: when 2026.1 is activated and the legacy filter goes (needs O-1).
- *touch*: when the code changes anyway.
- *watch*: act only on a trigger (§9.5).

Items re-scored against v3 are marked ↑ or ↓.

## 3. Register (open items)

### 3.1 Defects users can see

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #301 | Registration fails outside Development where the legacy `Member` role row is missing. `UserService.SignUpAsync` adds every user to `Member`, but only the Development rebuild creates that row (`AddMemberRoleAsync`), and nobody has checked staging (S4-25 P1). | Code | 3 | 2 | 1 | **25** | now (check staging first; proposed fix: a migration that creates `Member`) |
| #302 | `SignUpAsync` and `POST /api/forgot-password` send to Lettermint's test sink `ok@testing.lettermint.co` instead of the user's address. New users therefore stay unconfirmed and cannot reset a password (S1-08 K5, S4-03 P4, S4-25 P7). | Code | 3 | 3 | 2 | 24 | now, or the first task of M3.2 |
| TD-25 (#234) | The legacy settings store writes the appearance to `localStorage` for signed-out users. It also opens an English snackbar on a `prefers-contrast` change, and it crashes when storage is blocked. | Accessibility | 3 | 2 | 2 | 20 | step 3 (S60) |
| #296 | A tab left open across a deployment cannot load a lazy chunk it has not fetched yet. The navigation then fails without a message. Proposed fix: `withNavigationErrorHandler` loads the target URL in full (S4-22 R1). | Code | 3 | 2 | 2 | 20 | G-P, at the latest before the first production release |
| #52 ↓ | The legacy `/filters` stepper has unnamed tabs. Its axe suppression is gone (S4-13), but staging still shows the screen and no test checks it any more (O-S4-21). | Accessibility | 2 | 2 | 2 | 16 | legacy removal |
| TD-26 (#234) | `.text-error` on the legacy delete-account dialog title has low contrast in both appearances. | Accessibility | 2 | 1 | 1 | 15 | step 3 (account screens) |
| #279 | S11 „Wie wird bewertet?“ lacks the disclosure „Was bedeuten die Bewertungsstufen?“ (`pa-rating-legend`). It is specified in Handoff §4.11 and UX copy U24. | Accessibility | 2 | 1 | 2 | 12 | M2 follow-up, with #299 |
| TD-39 (#310) | After `release-mismatch`, the export dialog S13 shows only `criteria.export.outdated` with „Schließen“. The client keeps the old release pinned, so the user cannot get a working list without leaving the page. O-S4-72 promised a follow-up issue; none exists. Wanted: unpin the release, reload S11, drop the decisions of criteria that are gone, and go back to S10 if the selection is now excluded. This needs a UX copy key (S4-19 P4, S4-21a P4, S4-21b P2). | Code | 2 | 2 | 3 | 12 | before a second release is activated anywhere (2026.1) |
| TD-40 | A note over 1 000 characters stays too long once its field is closed. „Notiz bearbeiten“ does not say so, and S13 shows only the generic `criteria.export.error`, without leading to the note (S4-16 P2, S4-21b R2). | Accessibility | 2 | 1 | 2 | 12 | touch, at the latest with saved lists (step 3) |
| TD-41 | The loading bar announces `common.loading` each time loading starts. On S10 every option change in steps 2–3 reloads the filter options, so a screen reader hears „Wird geladen“ after each checkbox (S4-09 R2). No protocol decided this. | Accessibility | 2 | 1 | 2 | 12 | check in the manual pass (TD-49); fix only if the tester finds it disturbing |
| #278, #281, #282 | Design fidelity of S10/S11: „Auswahl ändern“ should sit next to the H1 from 600 px. The sticky action bar should span the viewport. The stepper circles are 24 px instead of 32. The summary labels and the desktop filter row also differ slightly. None of these affects behaviour or accessibility. | Code | 1 | 1 | 2 | 8 | touch, or one design-fidelity task |
| #170 | The legacy login outlet and the product list / detail do not reflow at 320 px / 400 %. The owner rated this `minor` and wants no legacy fix. | Accessibility | 2 | 1 | 3 | 9 | step 3 (S01), step 6 (S40, S41) |

### 3.2 Security and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-28 ↑ (#236) | Staging lags behind `main`, and the next deployment has preconditions. **(1)** Create the key-ring certificate and set `DATA_PROTECTION_CERTIFICATE`: without it the API does not start. After the first start, delete the plain-text key rows once (S4-26 P1, O-S4-89). **(2)** Search `logging."SeriLogs"` and the proxy logs for `POST /api/Users/reset-password`, which reset any password from an e-mail address until #249. If real accounts were hit, force a reset (S4-03 P1). **(3)** Make sure the API port is not reachable from outside and Traefik routes only to the client. `docker-compose.yml` publishes `5000:8080`, and a forged `X-Forwarded-For` from a trusted network would choose its own rate-limit counter (S4-21a P2). **(4)** Check `ASPNETCORE_ENVIRONMENT` and the v3 points: anonymous CRUD writes, Swagger, nginx. | Security | 3 | 4 | 1 | **35** | now (owner, before the next staging deployment) |
| #306 | The .NET SDK floats in CI (`10.0.x`), in the image (`sdk:10.0`) and locally. This machine has 10.0.114, CI the newest 10.0.4xx. An SDK patch changes the shipped package set, so the notices check can turn red without a code change (S4-27 P2). | CI/Ops | 2 | 2 | 1 | 20 | now (small; milestone G-P) |
| TD-42 | Four Dependabot majors are open and fail CI: MailKit 5.0.0 (#288: build and E2E), Swashbuckle 11 split across #289, #291 and #292 (build). Each needs a changelog and licence review, the regenerated `THIRD-PARTY-NOTICES.txt`, and for `SwaggerUI` the `EMBEDDED` review of the swagger-ui-dist it bundles (S4-27 P1). Majors of `Swashbuckle.*` and `MailKit` are not in the major-ignore list of `.github/dependabot.yml`. | Dependency | 2 | 2 | 2 | 16 | now (weekly review); owner: take the majors or ignore them like `Microsoft.*` |
| TD-12 | The E2E job's `compose.log` artefact could contain per-run secrets. This is harmless while they are random. | Security | 1 | 2 | 1 | 15 | watch at step 3 (Mailpit, real secrets) |
| TD-43 | Platform-role grants and revokes (`roles grant`/`revoke`) are logged at `Information`. That level reaches only the console, because the database sink keeps `Warning` and above, so no audit trail is stored (S4-25 R8, P4). | Security | 1 | 2 | 2 | 12 | decide with R-ADM-02 (step 7) |
| TD-44 | The licence notices cover npm (client) and NuGet (API), but not the OS packages of the base images: `aspnet:10.0`, nginx, PostgreSQL (`/usr/share/doc/*/copyright`) (S4-27 P4). | Dependency | 1 | 2 | 2 | 12 | licence review before G-P |
| TD-29 | 🔧 The criteria-suggestion channel is the placeholder `mailto:kriterium-vorschlagen@example.invalid` until O-6. `check-release-config.mjs` catches it, but no release pipeline runs the check yet. | CI/Ops | 1 | 2 | 2 | 12 | with the release pipeline, before G-P |
| TD-17 | `minimatch` GHSA-g3hv-75wg-m249 (moderate) inside `eslint` / `typescript-eslint`, Dependabot alerts 12 (client) and 13 (`e2e/`). It is a dev tool, not shipped: 14 and 10 findings with dev dependencies, 0 without. | Dependency | 1 | 1 | 1 | 10 | watch: take the ESLint release that moves to minimatch 11 |

### 3.3 Code, architecture and design

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #264 | `npm start` (`ng serve`) renders nothing. Vite cannot resolve `marked-katex-extension`, an optional import of the legacy `ngx-markdown`. The release build works. Related: the legacy `provideMarkdown()` in `app.config.ts` keeps `marked` and `ngx-markdown` (≈ 75 kB) in the first download (S4-11 P4, S4-15b P5, S4-22 P4). A successor following CLAUDE §5 hits this first. | Code | 3 | 2 | 2 | 20 | now: provide markdown only on the legacy routes, or stub the optional import |
| #308 | The nginx `immutable` rule `-[A-Z0-9]{8}\.(js|css)$` matches only `main` and `styles`. 44 of 45 lazy chunks have lowercase letters, `_` or `-` in their hash, so they are revalidated on every load (S4-28 R2). | Code | 2 | 1 | 1 | 15 | now (quick win) |
| TD-34 ↓ (#240) | Only the dark half is left. The light values of Handoff §1.2 are pinned (S4-29). The dark values are generated and not reviewed, for example `on-surface-variant` ≈ `on-surface`. | Accessibility | 1 | 2 | 2 | 12 | O-S5 (Figma dark mode), before G-R |
| TD-31 (#238) | `Criterion.SupersededByKey` holds one key, so a split cannot be stored. ARCHITECTURE §5.1 and CAT-03/04 still assume splits. | Architecture | 1 | 3 | 3 | 12 | owner decision before step 6 |
| #299 | The short description (S4-24) is in the model and the export, but not in the criteria answer, the S11 card or the S11 search. | Code | 2 | 1 | 2 | 12 | with the 2026.x folder that brings short descriptions (K-1), or M2 follow-up |
| TD-45 | The weight range exists twice, as server `Configurator:PriorityWeight` and client `priorityWeight`. The two are kept in step by hand (S4-19 P7). | Architecture | 1 | 2 | 2 | 12 | step 3 (serve the range with the saved-list API) |
| TD-47 | The export file name and the „Info“ date are the UTC day. Between midnight and 1:00 or 2:00 German time they show the previous day (S4-19 P5, S4-20b R4). | Code | 1 | 1 | 1 | 10 | owner decision before G-P |
| TD-46 | Platform maps 400 to `validation`, but 413 and other non-400 bad requests answer with the RFC default `type`, not a SPEC §8.6 type. S13 shows the generic error for them (S4-19 R4). | Code | 1 | 1 | 2 | 8 | touch (Platform error handling) |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-38 (#309) | **New E2E flake on `main`:** run 43320143678 (merge of #300, Firefox) failed at `e2e/configurator/s10-steps-2-4.spec.ts:115`. `getByText('Schritt 2 von 4: Anwendungstyp')` also matched the CDK live-announcer element while the step announcement was still in it, which is a strict-mode violation. The fix is to scope the check to `#main`. `s10-step1.spec.ts:124` uses the same pattern. No protocol names this flake. | Test | 3 | 2 | 1 | **25** | now (quick win) |
| TD-49 (#311) | **Manual accessibility checks of M2 (exit criteria X2, X3), by the accessibility tester.** They cover the screen-reader check of an exported XLSX (TESTING §7.2; EXP-02 checks only the structure) and the A11Y-07 pass of step 2 on J1. That pass includes the step-2 error and the `aria-disabled` options (S4-11 P2), the reset notice and „Ändern“ (S4-12 P3), the path skip link → card → S10 (S4-13 P6), the S10-R state against Figma (S4-12 P2) and TD-41. Until these are done, M2 is a development milestone, not the first pilot-ready one (SPEC §12, S4-22). #102 (autocomplete on phones) belongs to the same pass. | Accessibility | 3 | 3 | 3 | 18 | before G-P; owner schedules the tester |
| TD-48 | Firefox / Playwright crash class: Playwright's name computation of a `mat-slide-toggle` (V129) or of the S11 include checkbox (V165) crashes the Firefox page once axe has run. The workarounds are role-only or `aria-labelledby` locators. One more „Page crashed“ was seen on S11 (S4-11 P3, S4-16 P5, S4-18a P4, S4-22 P5). | Test | 2 | 2 | 2 | 16 | watch: recheck with every Playwright update; report upstream if a minimal case reproduces it |
| TD-54 (#312) | A card with two procedures shows tab labels that do not fit at 320 px: the second is cut and the header does not follow the selected tab. The reflow check never opens such a card, because J1's selection has one procedure per criterion (S4-15b P3, S4-18a P6, S4-22 P6). | Accessibility | 2 | 2 | 2 | 16 | sprint 05 touch: add a selection with two procedures to `reflow.spec.ts`, then reduce the tab padding below 600 px if needed |
| #87 | The session probe must answer 200 for anonymous and unknown test users. | Test | 2 | 3 | 3 | 15 | step 3 |
| #88 | Integration host option for the real Identity cookie flow (SEC-01/02/09). | Test | 2 | 3 | 3 | 15 | step 3 |
| #89 | Test that the antiforgery helper's session probe carries the caller's headers. | Test | 1 | 2 | 1 | 15 | step 3 |
| #82 | Regression tests for the S1-10 zoneless fixes (Z1–Z5, R1–R5). | Test | 2 | 2 | 3 | 12 | touch (screens are rebuilt in M3–M6) |

### 3.5 CI and tooling

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-51 | PRs that change the same lock file were merged without a rebase in between (#227 – #231), so a broken combination would show only on `main`. The ruleset does not require branches to be up to date (S4-06 P1). | CI/Ops | 1 | 2 | 1 | 15 | owner: merge one at a time, or turn on "Require branches to be up to date" |
| TD-52 | GitHub runner failures outside the workflow (`abandoned` jobs, or jobs `queued` for hours) turn the required check red although no test failed. Run 43098373045, the merge of #275, has been `queued` since 2026-11-11. Re-running is the only fix (S4-01 P1). GitHub refuses to cancel the stuck run (§7). | CI/Ops | 1 | 1 | 1 | 10 | owner: delete the stuck run; watch |

### 3.6 Documentation and licences

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-50 (#313) | Sprint-04 findings are not in the documents yet. **SPEC:** the header's "current versions (v2.7)" line names ARCHITECTURE v28, TESTING v40 and UX copy v16 (now v46, v55, v26). **ROADMAP_v4:** the header names ARCHITECTURE v27 and TESTING v39; M2 is still "In Progress"; the M2 exit status (X2, X3, X6 open; X7 done) is missing; the gap list is `…_luecken_v1` instead of v3 (S4-24 R5); D15 says no release is active outside the tests (S4-08). **Handoff:** §5.1/§2.2 say `mat-stepper` for S10, S35, S55 instead of `pa-stepper` (S4-10 P7); the body names ARCHITECTURE v11, TESTING v9/v10, SPEC_v1. **TESTING:** §4.2a names "UX copy v12"; §6.2 lacks the MiniCatalog's short description (S4-24 R4). **ARCHITECTURE:** the input list uses the old file names of the requirements and brainstorm documents (S4-07 P3). **SPRINT_04:** the close-out table is empty, and the rows O-S4-78 – O-S4-80 are missing (S4-25 P5). **Code:** the `@spec` tag of `criteria-list-summary.ts` names the deleted `ux_copy_procureaccess_v22.md` (S4-20a P5). **Owner documents (#259):** PRD and Kriterienkatalog headers, Kriterienkatalog §3.1; in the UX copy, `criteria.priority.change` is unused and the `changeLabel` hint says "Select" for a menu (S4-16 P4); §5.4 „Bisherige Auswahl“ has one „Ändern“ where R-CON-08 has one per layer (S4-12 F7). Proposal: versionless references in document bodies (S4-07 P3). | Documentation | 3 | 3 | 2 | **24** | now (ROADMAP v5 and the SPRINT_04 close-out in the control chat; one documentation session; owner documents by the owner) |
| #4 | The README is 18 lines. A successor needs setup, links to `docs/` and the process. `TECHNICAL.md` must also cover the deployment variables (`ASPNETCORE_ENVIRONMENT`, `DATA_PROTECTION_CERTIFICATE`); the key store, backup, restore and rotation from protocol S4-26 §6, with one rehearsed restore (S4-26 P2); `CREATE` in schema `public` for the ICU collation (S4-25 P3); the SDK pin (#306); and the switch from `T.1` to a real release on a kept dev database (S4-08 P2). | Documentation | 2 | 3 | 2 | 20 | step 16 (`TECHNICAL.md`), G-P |
| TD-53 | The export's source line quotes BIT inklusiv (U109). Its wording was approved, but the check against the BIT inklusiv permission is not recorded (S4-20a P2). | Dependency (licence) | 1 | 3 | 1 | 20 | owner, before G-P |

## 4. Done in sprint 04

| Item (v3 ID) | What was done | Task |
|---|---|---|
| #203 | The E2E jobs run Playwright in the pinned `mcr.microsoft.com/playwright` image, so no job installs apt packages (O-S4-1 (1)). The apt retry of S4-01 alone did not hold. | S4-01, S4-01b |
| #181 (TD-15) | UX copy v17 with every sprint-02/03 gap, approved unchanged (O-S4-4). Problem-type keys added to SPEC §8.6. | S4-02 |
| #205 | `[Authorize]` on `UsersController`. The token-less `POST /api/Users/reset-password` is removed (O-S4-7 (a)). | S4-03 |
| TD-24 (#233), TD-36 (#242) | CodeQL alert 3 closed on `main`. The CAT-10 fixture is annotated as BIT inklusiv. | S4-04 |
| TD-33 (#239) | Every release folder is validated in CI stage 3 (`CatalogReleaseFoldersTests`). | S4-05 |
| §7 Dependabot #226 – #231, #252 | All merged, jsdom 31 and xunit.v3 4 included. | S4-06 |
| TD-37 (#243) | The sprint-03 findings are in ARCHITECTURE, TESTING, SPEC and the Handoff. The owner part moved to #259. | S4-07 |
| TD-35 (#241) | Test release `T.1` is active in the dev and E2E stacks (O-S4-3). | S4-08 |
| §5 `vitest-axe` | Replaced by the `axe-core` helper `src/app/testing/axe.ts` (O-S4-11). | S4-10 |
| §5 legacy `LiveAnnouncer` (one of three) | The filter's unused call is removed. Two remain (§5). | S4-12 |
| #91, #92, §5 "CAT-10 on the old filter" | CAT-10 is reduced to its API part (O-S4-5 (a)), and the axe suppression list is empty. #52 stays open for the legacy filter (O-S4-21). | S4-13 |
| §9.2 S11 rows | Markdown texts, `matchedRule` texts, CAT-02 and CAT-20 FE parts, the CAT-16 deselection part, `pageTitle` as an `AREAS` root, the first `CriteriaListShown` caller, the `selection-excluded` key and the `suggest.mailBody` line breaks. A `ratingExamplesDe` field was not needed. | S4-15a – S4-18b |
| §9.2 export rows | EXP-05 log rule, the ARC-03 rule for `AdHocSelectionSource`, the CAT-16 export part, the `ExportCreated` caller, the relaxed E2E rate limit. | S4-19 – S4-21a |
| TD-30 (#237) | Short description as `CriterionRevision.ShortDescriptionDe` and `shortDescriptionDe` (ADR-016). The export column is filled; the screen part is #299. | S4-20a, S4-24 |
| #112 | `/health/ready` runs at most 2 checks at a time and has a configurable timeout. The first rate limits are in place (API-05). | S4-21a |
| #42 | Lazy routes. The initial bundle is 762.9 kB (was 1.94 MB), with a budget of 850 kB / 1 MB. | S4-22 |
| M2.4d API-06, §9.5 `Accept-Language` | The catalog endpoints negotiate their language. English texts need an ADR (§9.5). | S4-23 |
| §5 `approve` 500, `Approver` role | The claim policy `ModerationReview` answers 403. The roles are renamed to `Moderator` and `PlatformAdmin`. | S4-25 |
| TD-27 (#235) | The key ring is encrypted with a certificate. Store, backup, restore and rotation are in protocol §6. | S4-26 |
| TD-22 (#187) | `/app/THIRD-PARTY-NOTICES.txt` is generated and checked in CI stage 2. | S4-27 |
| TD-32 | `dialog.service.ts`, the unused `Logger` and the nginx `/assets/` location are removed. | S4-28 |
| TD-34 (light part) | The light values of Handoff §1.2 are pinned, and an E2E test compares them with the table. | S4-29 |
| — | Found and closed within the sprint: #269 (combobox flake), #280 (S10 icons), #283 (Figma frames). | S4-14b, S4-30 |

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design. Do not fix it on its own; replace it with the step. Rows new in v4 are marked *(new)*.

| Debt | Retires with |
|---|---|
| *(new)* **Legacy filter.** These go with the activation of 2026.1 (after O-1): the switch `configuratorReleaseGuard` → `/filters` on 404; `LEGACY_FILTER_PATH` and the main-nav path constants imported from `features/configurator/guards/` (S4-12 R6); nav "current" on `/filters`; #52; CAT-10 with `ExistingCatalogSeed`, `LegacyCatalogReadTests` and `ProductsByCriteriaFilterTests` (S4-13 P3); `provideMarkdown()`, `marked` and `ngx-markdown`, unless #264 moves them earlier; the 5 M2 entries of `LEGACY_EXCEPTIONS`; the stepper icons `create`, `done`, `warning` under `material` in the icon list; the 404 of `/api/v1/catalog/*` on staging | legacy removal |
| JWT in `localStorage`, `jwtDecode` with `any` (SEC-01). *(new)* `revoke-sessions` and the password change renew the security stamp, but the JWT does not check it (S4-03 R4). *(new)* `signedIn` of `CriteriaListShown` counts a user with an expired JWT as anonymous (S4-18b P2). | step 3 (M3.1) |
| `DateTime.UtcNow` ×6 (`UserRepo` ×1, `UserService` ×4, `ProposalService` ×1), against CLAUDE §7. Unchanged. The hits in `tests/` are polling deadlines, the Serilog stamp and test certificates, which is fine. | `UserService`/`UserRepo` with step 3, `ProposalService` with step 5 |
| `Microsoft.AspNetCore.Identity.UI` only for `IEmailSender`. | step 3 |
| `UsersController` and the identity minimal APIs answer with `{ code, message }`. `signUp` / `reset-password` answer 500 on a missing field. Dead key `UICustomizationNotFound`. *(new)* The in-action 401 is `application/json`, the challenge 401 `application/problem+json` (S4-03 R2). | step 3 |
| The Identity tables move into `IdentityAccessDbContext` (ADR-014 rule 4); OPS-04 two-host cookie test (S4-26 P6). *(new)* `roles` command into IdentityAccess (M3.1). *(new)* `UserContributorLink.UserId` without foreign key while users are legacy. | step 3 |
| *(new)* Antiforgery on `POST /api/v1/configurator/criteria-list-shown` (SEC-02; the export stays exempt, S4-18b P3). | step 3 |
| *(new)* S11 without „Liste speichern“ and `criteria.list.unsaved` (O-S4-49). The draft note asks to sign in "um sie zu speichern" before saving exists (O-S4-54). TESTING §5.3 "save the draft in one call" (S4-09 P5). | step 3 (M3.5) |
| Deferred invariant parts: A11Y-08 with the explicit setting (a signed-in journey makes the axe helper fail with „did not switch“); CAT-06, CAT-21 and *(new)* CAT-08 for stored configurations; *(new)* EXP-06 and the persisted source of EXP-01/EXP-03; *(new)* the EXP-05 file-store part; *(new)* the TEN-04 workspace part (step 3) and directory part (step 6); *(new)* TEN-06 for the `CatalogEditor` and `PlatformAdmin` endpoints; *(new)* API-05 for registration, password reset, saved export, publication and contribution (S4-21a P5); *(new)* API-06 for the directory and workspace lists (S4-23 P4, V167); *(new)* PRV-04 (S4-25 P6); *(new)* the A11Y-02 keyboard variant of J3; CAT-17 workspace and directory parts | their steps |
| `mat-menu` (profile) and `matTooltip` not checked by hand since `@angular/animations` went. The explicit motion setting comes with S60. | step 3 (S60) |
| *(new)* The legacy helper classes `.secondary`, `.tertiary`, `.error` re-run `mat.theme` and emit generated colours in their subtree (S4-29 F5). | step 3 (account screens) |
| Callers of the fixed legacy store methods do not catch the rejection. | M3, M5 rebuilds |
| Legacy `<strong>` in `mat-error` is drawn as synthetic bold. | step 3 / M5 forms |
| Delete-account „Löschen“ uses native `disabled`. | step 3 |
| The legacy login outlet replaces the whole shell and does not reflow (#170 L1). | step 3 (S01) |
| `[Produces("application/json")]` and `[ApiVersion("1.0")]` on legacy controllers. *(new)* The host's `UseRequestLocalization` still reads `?culture=` and the culture cookie for them (S4-23 P5). | each replacing step |
| The snackbar is still the error channel for non-5xx errors (UX copy §4.1). | its last legacy caller, steps 3–6 |
| Legacy routes without a page-title key and without an H1 (`/filters`, `/products`, …). | rebuild of each screen |
| **Two** legacy `LiveAnnouncer` calls: `products-list.ts`, `proposal-list.ts` (v3 said three). | steps 5 and 6 |
| The API sends English validation messages without codes. *(new)* S10 shows such a message through `errors.field.server` for a `validation` answer (S4-12 P5). | each step's module validation |
| Uppercase legacy roots in `de.json`. | rebuild of each screen |
| `LEGACY_EXCEPTIONS` in `eslint.config.js`: **32 files**, unchanged (M2 5, M3 16, M5 11; #158). | each step deletes its entries; the M2 group with the legacy removal |
| Legacy product reads ignore `Product.IsDeleted`; `GetByCriteriaFilterIds` lacks the proposal filter. | step 6 |
| `ProposalService` `dto!.ToEntity()` throws on a null snapshot, and `!` on `user` in `ApproveAsync`. *(new)* `PlatformAdmin` keeps `moderation:review` (O-S4-81); `Proposal.ApproverId` keeps its name (O-S4-83). *(new)* `FavoritesController.cs` is commented out as a whole (S4-28 R1). *(new)* The release build warns „Could not parse 1 selector … 50%“ from the CodeMirror `@-moz-keyframes` of EasyMDE (S4-21b F8). | step 5 (proposal retirement, favorites rework) |
| The tagged API-02 test sits on the legacy proposal form. | move it to a rebuilt form before step 5 |
| No backend test of the legacy seeded exclusions, only J1. | steps 5/6 |
| 29 TODOs in legacy client features and DAL repos, plus two in tests. | features retired or rebuilt in M3–M6 |
| E2E: no `E2E` environment, Mailpit or clock endpoint, and *(new)* no users or products in the E2E seeder (S4-08 P1). The catalog and rate-limit parts are done. | steps 3, 4, 6 |
| `withProposal()`, `withProducts()`/`withFavorites()`. | per `ARCHITECTURE §8.1` |
| Legacy projects fail on a warning only in CI, not locally. | each migration step |
| *(new)* K5 „Warum diese Priorität?“ toggle outside a detail panel: no UX key yet for the hide state (S4-15a P3). | first screen with K5 outside S11 (workspace) |
| *(new)* Display names are not normalised (trim, NFC) before the case-insensitive uniqueness check (S4-25 R6). S40 reuses `common.actions.clearInput` (S4-30). | step 6 (S50, S40) |

**Not tracked on purpose** (unchanged): CI has no NuGet or Docker cache; Serilog stamps with the system clock; files without logic count for coverage.

The won't-fix findings of the sprint-04 reviews that need no follow-up are listed only in their protocols. Examples: S4-05 R1/R3/R4, S4-08 R1/R2/R4, S4-14 R2/R5, S4-15a R2/R3, S4-17 R1–R3, S4-19 R3/R5–R7, S4-20a R3/R4, S4-20b R5–R7, S4-21a R2–R4, S4-22 R3, S4-26 R6–R8, S4-27 R1/R2/R4.

## 6. Decisions waiting for the owner

CLAUDE §6.6: Claude Code must not decide these. Decided in sprint 04: O-S4-1 – O-S4-94 (see `SPRINT_04.md` and the protocols); TD-30 (O-S4-6), TD-33 (O-S4-2), TD-36.

| Decision | Source | Needed by | Claude's proposal |
|---|---|---|---|
| **TD-28** staging deployment: certificate, plain key rows, log search for the token-less reset, API port, environment variable | S4-26 P1, S4-03 P1, S4-21a P2 | before the next staging deployment | the checklist is on #236 (comment of 2026-11-13) |
| **#301** registration on staging: a migration that creates `Member`, registration without the role, or wait for M3.3 | S4-25 P1 | start of M3 | the migration, after checking staging |
| **#302** mails: the real address with a recipient test, the sink as debt until step 3, or a redirect on staging only | S4-25 P7 | start of M3 (M3.2) | the real address with a test |
| **#306** pin the SDK in `global.json` and the image tag together | S4-27 P2 | before stage 2 next turns red without a change | pin |
| **TD-42** Dependabot majors: MailKit 5 (#288), Swashbuckle 11 (#289, #291, #292); or add them to the major-ignore list | S4-27 P1 | weekly review | Swashbuckle: one PR for all four packages, with the `EMBEDDED` review; MailKit 5: read the changelog, run the e-mail tests |
| **TD-51** lock-file merges: one at a time, or "Require branches to be up to date" | S4-06 P1 | next Dependabot batch | one at a time (no ruleset change) |
| **TD-39** reload flow after `release-mismatch`: behaviour and UX copy key | O-S4-72, S4-21a P4 | before a second release is activated | as described in §3.1 (#310) |
| **TD-47** date of the export file: UTC day or German day | S4-19 P5 | before G-P | German day (`Europe/Berlin` as a 🔧 setting) |
| **TD-43** stored audit trail of role changes | S4-25 P4 | with R-ADM-02 | — |
| **TD-53** BIT inklusiv permission for the export's source line | S4-20a P2 | before G-P | — |
| K5 menu on „Geprüft nach …“ cards: the Handoff says "priority B, deselectable", the code allows a level change | S4-14 P2 | any time | — |
| **#299** does the S11 search include the short description? | S4-24 P2 | with #299 | — |
| Figma vs code (design owner): S13 differences D1–D5 (S4-21b P3); `error` and notice icons filled in Figma vs Outlined in Handoff §1.8 (S4-30); S11 without the priority filter group (S4-17 P1) and with „Liste speichern“ / „Noch nicht gespeichert“ (S4-18a P3); the „Prüfverfahren …“ line not in code (O-S4-35); pages without a frame (S04, S80, legal) | sprint-04 protocols | design, any time | — |
| **TD-31** splits (#238): several successors, or reword ARCHITECTURE §5.1 and CAT-03/04 | S3-14 P2 | before step 6 | — |
| **TD-34** dark values (O-S5, #240) | S4-29 P3 | before G-R | wait for the Figma dark mode |
| A release with gaps stays stored; `filter-options?release=` serves it while `/criteria` answers 404 | S3-20b P2 | when 2026.1 is imported anywhere | keep |
| O-22: may a `Contracts` project reference another module's `Contracts`? | S2-08 P4 | first contracts type that needs it | — |
| Close milestone **M1** (0 open issues). Close **M2** as a development milestone, with X2, X3 and X6 tracked by TD-49 and O-1 | S4-22 | sprint review | yes to both |
| Project team: priorities, rationales and plain-language titles of 2026.1 (O-1, K-1, K-6; gap list v3); short descriptions; the „#todo#“ in 11.1.3.2 (Z-7, S4-15b P2); app-type hints beyond the UX keys (S4-11 P5); the criteria-suggestion channel (O-6) | S3-27, S4-11, S4-15b, S4-24 | M2.3; O-6 before G-P | — |

## 7. Issue hygiene

- **Milestones set on 2026-11-13** (owner's request): #264 and #308 → M3 (sprint 05 quick wins); #278, #279, #281, #282 → M2 (S11 follow-ups, with #299). No open issue is left without a milestone.
- **M1** has no open issue and can be closed. **M2** has #52 (legacy removal), #236, #259 and #299 open.
- **#52** stays in M2 but closes only with the legacy removal, which needs O-1. **#240** keeps only its dark part; a comment that the light part is done is still missing (S4-29 P4).
- **CI:** run 43098373045 (merge of #275) has been `queued` since 2026-11-11 and has no jobs. GitHub refuses to cancel it: `cancel` answers "completed", `force-cancel` answers "not been queued yet" (409). Deleting the run (`gh run delete`) is the only way to remove it; owner's call (TD-52).
- **Open Dependabot PRs** for the weekly review: #288, #289, #291, #292 (TD-42).
- **Alerts:** CodeQL has no open alert. Dependabot has only 12 and 13 (TD-17). Every issue closed for an alert in sprint 04 (#233) really closed it.
- **Labels:** all open debt issues carry `tech-debt`, except #4 and #259 (`documentation`), #52 (`bug`, `a11y`), #301 and #302 (`bug`), #102 (`question`, `a11y`). This is correct.

## 8. New issues

Created on 2026-11-13 for the **TD-** items that need one. Each holds the problem, the fix and "done when".

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-38 | #309 E2E flake: S10 step-line check also matches the live announcer (strict mode) | tech-debt | M3 |
| TD-39 | #310 S13: reload the criteria list after `release-mismatch` | enhancement, tech-debt | G-P |
| TD-49 | #311 Manual accessibility checks of M2: exported XLSX with a screen reader, A11Y-07 pass of step 2 | a11y, question | G-P |
| TD-54 | #312 Reflow: a criterion with two procedure tabs at 320 px | a11y, tech-debt | M3 |
| TD-50 | #313 Carry the sprint-04 findings into SPEC, ROADMAP, Handoff, TESTING and the document headers | documentation, tech-debt | M3 |
| TD-28 | Comment on #236 (2026-11-13) listing the sprint-04 preconditions of the next deployment (§3.2) | — | M2 |

TD-40 – TD-48 and TD-51 – TD-53 need no issue: they are owner decisions (§6), touch items or watch items.

## 9. Fix plan — when to fix what

### 9.1 Sprint 05 — M3 start (cookie session M3.1, registration M3.2) and its debt

New debt budget **≈ 4 points**.

| # | Work | Items | Pts | Note |
|---|---|---|---:|---|
| 1 | Quick wins in one PR: scope the step-line checks to `#main`; widen the nginx `immutable` pattern and check the headers on the client image | TD-38, #308 | 0.5 | first, so `main` stays green |
| 2 | Local dev server: `provideMarkdown()` only on the legacy routes (≈ 75 kB out of the first download), or a stub for the optional import | #264 | 0.5 | a successor needs `npm start` |
| 3 | Pin the .NET SDK (`global.json` and image tag together) | #306 | 0.5 | after the owner's decision |
| 4 | Account bugs before M3.2 builds on them: `Member` role migration; real recipient with a test | #301, #302 | 1 | pulls two M3 bugs to the start |
| 5 | Documentation session (ROADMAP v5 and the SPRINT_04 close-out in the control chat) | TD-50 | 1 | owner documents by the owner (#259) |
| 6 | Reflow with a two-procedure card | TD-54 | 0.5 | touches `reflow.spec.ts` only, unless the tab padding needs a fix |
| — | Owner: staging deployment checklist; §6 decisions; Dependabot majors; M1 and M2 milestones; the tester's M2 pass | TD-28, TD-42, TD-49 | 0 | |
| — | With M3.1/M3.2 (already counted in the roadmap): JWT → cookie session, OPS-04, #87 – #89, antiforgery on `criteria-list-shown`, `signedIn` from the session, `Identity.UI`, `{ code, message }` failures, `UserService` `DateTime.UtcNow`, the M3 `LEGACY_EXCEPTIONS` entries as files are replaced | §5, #87 – #89 | 0 | |

### 9.2 Rest of M3 — organisations (M3.3), contributor profiles (M3.4), saved lists (M3.5), settings S60

| Fix | Items | Why then |
|---|---|---|
| S60 replaces the legacy settings store; account screens rebuilt (contrast, `aria-disabled` reason, `mat-menu` / `matTooltip`, `.secondary`/`.error` helper classes, login outlet reflow) | TD-25, TD-26, #170 L1, §5 | the screens are rebuilt |
| Saved lists: „Liste speichern“, `criteria.list.unsaved`, draft-note wording, CAT-08, EXP-06 and the persisted EXP-01/EXP-03, the TEN-04 workspace part; serve the weight range from the server; a too-long note leads to the note | §5, TD-45, TD-40 | same pipeline as the saved export |
| Mailpit and real secrets in the E2E stack: check the `compose.log` artefact; E2E users in the seeder | TD-12, §5 | the first mails in E2E |
| A11Y-08 with the explicit setting and its persistence | §5 | S60 |
| Regression tests for the zoneless fixes, screen by screen | #82 | the screens are rebuilt |

### 9.3 M4 – M7

| Milestone | Fix | Items |
|---|---|---|
| M4 | First time-dependent tests; E2E clock endpoint for J5; S34 child routes in Figma mapping §2.2 and `PAGES` | — |
| M5 | Proposal retirement: `ProposalService`, `withProposal()`, `PlatformAdmin`'s `moderation:review`, `ApproverId`, null snapshot, EasyMDE warning, `FavoritesController.cs`; move the API-02 test first; the `products-list` / `proposal-list` `LiveAnnouncer` | §5, #158 M5 group |
| M6 | Splits decided before CAT-03/04; directory rebuild (`IsDeleted`, proposal filter, product list and detail reflow); display-name normalisation; API-06 and TEN-04 directory parts | TD-31, #170 L2/L3, §5 |
| M7 | File-store check in `/health/ready`; EXP-05 file-store part; role-change audit trail with R-ADM-02 | TD-43, §5 |
| Legacy removal (2026.1 activated, needs O-1) | Everything in the first row of §5; #52 closes; the M2 `LEGACY_EXCEPTIONS` entries go; the S13 reload flow must be done before | §5, #52, TD-39 |

### 9.4 Before the pilot (G-P) and the public release (G-R)

| Fix | Items |
|---|---|
| README and `TECHNICAL.md` (process step 16): setup, deployment variables, key ring (S4-26 §6) with one rehearsed restore, collation privilege, SDK pin, indicator queries of R-MET-02 | #4 |
| Manual accessibility checks: exported XLSX, A11Y-07 pass of step 2, autocomplete on phones | TD-49, #102 |
| Reload after a deployment (lazy chunks) | #296 |
| S13 reload flow after `release-mismatch` | TD-39 |
| Licence review: OS packages of the base images, the BIT inklusiv permission of the export source line | TD-44, TD-53 |
| Release pipeline that runs `check-release-config.mjs`; the O-6 channel and the legal pages (GP.3) in the check | TD-29 |
| Export date in the German day, if the owner decides so | TD-47 |
| The axe suppression list stays empty (TESTING §12.4) | any new entry |
| G-R: dark values reviewed and pinned with the Figma dark mode | TD-34 |

### 9.5 Watch items — act only when the trigger happens

| Trigger | Then | Source |
|---|---|---|
| A Playwright update | Recheck the Firefox crash workarounds V129 and V165; report upstream if a minimal case still crashes | TD-48 |
| A Dependabot PR updates `@playwright/test` | Bump the tag and digest in `e2e/playwright-image/Dockerfile` on the same PR | S4-01b F5 |
| MCR is slow or down | Add a retry to the image pull (10-minute limit, no retry today) | S4-01b P4 |
| A Dependabot NuGet group opens in the middle of a .NET servicing release more than once | Add a `cooldown` of 1–2 days for `nuget` | S4-06 P2 |
| The project moves to Node 26 | `.nvmrc` files and the client Dockerfile together (E2E already runs on the image's Node 26) | S4-06 P3 |
| A new version of a package in `EMBEDDED` of `check-api-notices.mjs` | Review what it embeds, update `scripts/api-notices/` and the pinned version | S4-27 |
| A ClosedXML release needs SixLabors.Fonts ≥ 2 | Do not take it (Split License fails D12); `ClosedXmlLicenceTests` guards | S4-20b F6 |
| Anyone calls `AdjustToContents` or measures text in the export | Refuse: the API image has no fonts | S4-20b F4 |
| A new or renamed fixture quotes BIT inklusiv texts | Add its exact path to the BIT block of `REUSE.toml` | S4-04 R4 |
| 2026.x texts are exported | Check that the CSV formula guard does not prefix catalog text starting with `-` | S4-20a R2 |
| English texts are planned (SPEC §3) | ADR for English catalog texts (release format, `CatalogLanguage.OfReleases`), a second `ExportTexts` resource, an `Accept-Language` header with an unreadable entry (S4-23 R3) | S4-23 P2, P3 |
| The project team's first release PR needs the database checks of the import | `catalog validate` or a CI step with a throwaway database | S4-05 P1 |
| 2026.1 gets its priorities | Document starting a kept dev database without `T.1` (fresh volume) | S4-08 P2 |
| A later screen gets a sticky bar | Reuse the S11 `focusin` fallback (Firefox ignores `scroll-padding` on focus moves) | S4-18a P5 |
| Usability tests object to collapsing example groups after a tab switch | Keep the tab content attached | S4-15b P4 |
| A deployment ships a client before its API | An answer without `appliesTo` crashes the card; deploy client and API together | S4-15b P6 |
| A test asserts a closed Material overlay | Wait with `vi.waitFor` (the panel detaches in a `setTimeout`) | S4-20b F7 |
| Material changes when it clears the active option | Recheck `chooseFirstOption` in `e2e/support/combobox.ts` | S4-14b |
| S35 or S55 reuses `pa-stepper` | Give it a test of its own | S4-10 P8 |
| The next change to `Release2026_1Tests` | Read the folder from the repository, not the build output | S4-05 P2 |
| An unhandled startup exception is hard to see | The API container exits with 139, not 134; open an issue | S4-26 R5 |
| ESLint / typescript-eslint release accepts minimatch 11 | Take it; close alerts 12 and 13 | TD-17 |
| A Dependabot PR updates `material-symbols` | It stays red in stage 1 until `scripts/subset-material-symbols.sh` runs on its branch | S3-25 P3 |
| `eslint` / `typescript-eslint` versions drift between `e2e/` and the client | One Dependabot group for both | S3-26 P3 |
| The first real C# CodeQL finding, or a missed one | Revisit `build-mode: none` for C# | S3-03 R2 |
| No push to `main` for 60 days | Re-enable the weekly CodeQL schedule | S3-03 R4 |
| A module needs a map that is tedious by hand | ADR-015 rule 4: revisit Mapperly | S3-07 P4 |
| The API is meant for third parties | Revisit O-S3-2 (Swagger only in `Development`) | S3-06 P3 |
| A hotfix version older than the newest release is imported | „Previous release“ is the one stored last; check `supersedes` by hand | S3-16 P2 |
| A size limit on `IMemoryCache` (D10, scale-out) | Give the catalog entries a size | S3-17 P2 |
| More than one API instance | Migrations, log retention, the usage-event queue, the first Data Protection key and the in-memory rate limiter assume one instance | S3-23 D3, S3-24 R4, S4-21a |
| A document defines a range for `PriorityRecommendation.Weight` or `Level`/`Source` | Add the CHECK constraints | S3-13 R6 |
| An icon is chosen by `fontIcon`, `svgIcon` or a CSS class | Extend the icon check | S3-25 R4 |
| A future SDK breaks NetArchTest | Fork `NetArchTest.eNhancedEdition`, with an ADR | S2-05 P3 |
| One of the gate's known limits bites | Read the result reports instead (option 1B) | S2-09 P1 |
| First deployment of a new module schema | The database role needs `CREATE` on the database, and on `public` for the ICU collation | S2-07 P5, S4-25 P3 |
| Dependabot rebases a PR after the CI review | Check the head commit at merge time | S2-04 P5 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |
| Docker builds fail on the developer machine | Free disk space (E2E images ≈ 2 GB; the root disk was nearly full in S4-18b P4) | S1-13 P1, S4-18b P4 |
| The next version of SPEC, ARCHITECTURE, Handoff or UX copy is written | Update its references to the current versions of the other documents, or make them versionless | S1-21 P2, S4-07 P3 |

### 9.6 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`. Move done items to "Done", add the new findings of the sprint protocols and reviews, and re-score. Check the milestones of new issues and the CI runs on `main` (a failed E2E job is a finding until its cause is named). Check that every issue closed for a CodeQL or Dependabot alert really closed the alert. Fill in the sprint's close-out table.
- **Start of every migration step:** open its GitHub milestone, and plan its debt together with the step's invariants. Delete its `LEGACY_EXCEPTIONS` entries as files are replaced (#158).
- **Every screen task:** new icons go into the icon list; the screen joins `reflow.spec.ts` with each state that changes the layout (tabs, open panels, dialogs), named in the task prompt.
- **End of every migration step:** review the axe suppression list (TESTING §12.4), and plan the manual screen-reader pass of the step with the tester (A11Y-07).
- **Weekly:**
  - Dependabot pull requests: check the licence of new versions before merging (CLAUDE §10), regenerate `THIRD-PARTY-NOTICES.txt` for NuGet updates, merge PRs that share a lock file one at a time, and bump the Playwright image with `@playwright/test`.
  - nginx: merge even minors (stable branch), close odd ones.
  - CodeQL alerts.
  - The CI runs on `main`.

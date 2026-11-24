# ProcureAccess — Tech-debt register

> **Status:** v6 (2026-11-24) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 06, made with the skill `Engineering/tech-debt` (inventory → categorise → score → plan). Replaces `TECH_DEBT_v5.md`.
> **Sources:**
> - open and closed GitHub issues, pull requests and milestones (2026-11-24);
> - the open points and won't-fix findings of the 35 protocols `docs/sprints/sprint_06/S6-*.md` (S6-01 – S6-30), `SPRINT_06.md`, ARCHITECTURE §16.5 (O-25 – O-32);
> - a scan of the code on `main` at f0c6eb19: TODOs, `DateTime.UtcNow`, suppressed warnings, `eslint-disable`, `LEGACY_EXCEPTIONS`, `LiveAnnouncer`, browser storage, document versions named in code, file sizes;
> - `dotnet list package --vulnerable --include-transitive` (none) and `--outdated` on SDK 10.0.404;
> - `npm audit` on both lock files (0 findings, also with dev dependencies);
> - Dependabot and CodeQL alerts (none open), the 47 CI runs on `main` since 2026-11-17, the open pull requests and the ruleset of `main`;
> - free disk space of the development machine.
>
> **Next review:** end of sprint 07 → `TECH_DEBT_v7.md`.

---

## 1. Summary

- **Sprint 06 finished M5 except two requirement gaps and cleared the whole v5 sprint plan (§9.1 of v5).** 27 issues were closed. 22 of them were debt or carried defects: #375, #378, #350, #385, #386, #368, #381 – #383, #379, #380, #355, #339, #387, #384, #318, #296, #310, #388, #389, #434 (created and closed in the sprint) and #4. #278, #279, #281, #282 and #234 closed with sprint-05 work.
  - **Explicit debt was ≈ 16 of the 60 committed points** (≈ 27 %): 6 general (S6-01 – S6-09) and 10 as M5 debt rows (M5.0d users 5, M5.4d 1, M5.6d 1, S6-24 1.5, S6-25 1.5). Both stretch tasks (S6-29 backup, S6-30 `TECHNICAL.md` and README) are done too.
  - Rows closed against v5: **26 of 58** (§4).
- **The big structural debts of v5 are gone.**
  - The users live in IdentityAccess with foreign keys (TD-72, #387).
  - The legacy proposals are retired, with their tables and personal data (TD-61, #384).
  - The root store has `withLoading()` once.
  - `LEGACY_EXCEPTIONS` fell from **16 to 3 files** (only the M2 group, which goes with the legacy filter).
  - `DateTime.UtcNow` outside tests: **0**. TODOs: 24 → **16** (15 in legacy code, 1 deliberate fixture value).
  - The cause of the Firefox crash class TD-48 is known, and every workaround is retired (S6-06, #434).
- **Two M5 requirements are not met in the UI or the export** (§3.1, §3.2):
  - a project's criteria list can be chosen only when the project is created; assigning or replacing it later (R-PRJ-04 UI) waits for a dialog design (#420);
  - the personal data export leaves out the assessments of candidates, so PRV-07's workspace part is incomplete (#414).
- **`main` is green, but three of its five failed runs since 2026-11-17 still have no named cause** (TD-84, TD-85). The other two are a J2 keyboard race, fixed in 98ea8031. 41 of the 47 runs passed, one is still running.
  - The v5 watch trigger "`start-and-error-pages.spec.ts` loses the H1 focus in Firefox again → open an issue" has fired: it failed twice on `main` and twice on pull requests.
  - The mobile-Safari axe flake in dark appearance that S6-01 proposed as an issue happened a second time on `main`. The issue was not created during the sprint; it is #452 now.
- **Deploying is the top risk now, not the code.** Staging (TD-28, priority 40) still lags. S6-30 found four more gaps while writing the operations guide:
  - API and client ports listen on every host interface (#449, priority 30);
  - compose does not pass the public-address settings, so mail links would point to the default address (#447);
  - the health endpoints are not reachable through the public address (#448);
  - a revoked platform role stays in a running session (#450).

  The backup of S6-29 has no off-host copy and no alert yet (TD-92).
- **60 rows are open and scored** (§3):
  - 32 carried from v5;
  - 9 for issues created during the sprint (#414, #420, #429, #436, #437, #447 – #450);
  - 19 for the other new items from the protocols and this scan (**TD-84 – TD-102**); six of them got an issue with this review (#451 – #456, §8).
- **Nothing breaks a product rule (CLAUDE §8).** No known vulnerability:
  - NuGet has none.
  - `npm audit` finds 0 in both packages, also with dev dependencies. The dev-only picomatch advisory S6-08 saw is gone; both trees have 4.0.7.
  - CodeQL and Dependabot have no open alert. Alerts 12 and 13 (`minimatch`, TD-17) were fixed on 2026-11-18.
- **Owner decisions pile up.** ARCHITECTURE §16.5 now holds eight open questions from sprints 05 and 06 (O-25 – O-32). Three of them touch correctness or privacy: O-30, O-31 and O-27. UX copy has ten new open keys or wordings (TD-79), and seven more screens have no Figma frame (TD-80).
- **The development machine is still short of disk space (TD-74):** 1.1 GB free, 97 % used today. Most sprint-06 tasks left E2E to CI because the one local stack was in use or the disk was full.

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

The formula ranks large work low. A requirement that is not met (#420) is marked so in its row, whatever its number.

**When:**
- *now*: sprint 07.
- *a milestone* (`ROADMAP`: G-P pilot, G-R public release, M6, M7) or *a migration step* (`ARCHITECTURE §12`).
- *legacy removal*: when 2026.1 is activated and the legacy filter goes (needs O-1).
- *touch*: when the code changes anyway.
- *watch*: act only on a trigger (§9.5).

Items re-scored against v5 are marked ↑ or ↓.

## 3. Register (open items)

### 3.1 Defects and gaps users can see

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-89 (#454) | S12 shows `criteria.save.outdated` with only „Schließen“ after a `release-mismatch`. S13 got „Kriterien neu laden“ in S6-27; S12 needs the same, with a UX key such as `criteria.save.outdated.reload` (S6-27 P1). | Code | 2 | 2 | 2 | 16 | before a second release is activated anywhere (2026.1); owner key first (TD-79) |
| #52 | The legacy `/filters` stepper has unnamed tabs. No test checks it any more (O-S4-21). | Accessibility | 2 | 2 | 2 | 16 | legacy removal |
| #429 | S11 of a project's copy (`/projekte/:id/kriterienliste`) has no way back to its project, only the browser's „back“. Fix: `pa-breadcrumb` above the H1, as on S32 and S34. Owner: the text of the last item (S6-22b P1). Fix together with TD-91. | Accessibility | 2 | 1 | 1 | 15 | now |
| #420 | **Requirement gap (R-PRJ-04 UI):** a list can be chosen only when the project is created (S31). S32 has neither `projects.detail.noCriteria.cta` nor a replace action. The server part is built (S6-12a, the lock in S6-18, `If-Match` in S6-20a). It needs a dialog design and UX keys (S6-12b P1, S6-14 P1, D2 (a)). | Code | 3 | 1 | 3 | 12 | **M5 is not complete without it:** owner design, then one task in sprint 07. The number understates it. |
| TD-66 | Switching the organisation outside S21 is not announced. The button's name changes and keeps the focus. Needs a UX key such as „Organisation gewechselt: {{name}}“ (S5-18 P3). | Accessibility | 2 | 1 | 2 | 12 | once the owner gives the key (TD-79) |
| TD-63 | When another tab signs in as a **different** user, the warning's probe takes that session into this tab without the S5-30 D3 handling. The previous user's in-memory data stays. This matters on shared devices (S5-30 R7). | Security | 1 | 2 | 2 | 12 | before G-P |
| TD-69 | `.contrast-more` applies the legacy, unreviewed colours to every visitor whose system asks for more contrast. „Hoher Kontrast“ on S60 and its colours wait for H2 (R-SET-04). Until then the API refuses `high` (S5-25b R4, S5-26 P2). | Accessibility | 2 | 2 | 3 | 12 | design H2, before G-P |
| TD-41 | The loading bar announces `common.loading` each time loading starts: on S10 for each option change, and after a sign-in through the overlay right after `shell.session.restored`. **New, same class:** an announcement right after a route change (`candidate.removed`, `projects.created`) is followed at once by the page title (S6-22a P4), and S6-01's limit switch shows two `alertdialog`s for about 75 ms (S6-01 P3). | Accessibility | 2 | 1 | 2 | 12 | in the tester's pass (TD-49) |
| TD-67 | Content links outside the footer, such as S01's „Noch kein Konto? Jetzt registrieren“, use the browser's default link colour. `main.scss` has no shared content-link style (S5-09 P4). | Accessibility | 1 | 1 | 1 | 10 | touch |
| TD-68 | A second confirmation link opened on `/registrieren/bestaetigen` updates H1 and text, but not the page title. `PageTitleStrategy.changePageInPlace` exists for this (S5-12b R4). | Accessibility | 1 | 1 | 1 | 10 | touch |
| TD-81 | Leaving or deleting an organisation as its sole member is not possible (409 `last-owner`), and users cannot change their address. Now ARCHITECTURE O-25 and O-26 (S5-16a P4, S5-28a P5). | Code | 2 | 1 | 3 | 9 | when users need it (owner) |
| TD-90 | Deleting a saved list on S14 after someone changed it meanwhile shows the general error. The UX copy has no text for "deleted although changed" (ARCHITECTURE O-29; S6-21a P2, S6-21b P1). | Code | 1 | 1 | 2 | 8 | owner design (O-29) |
| #170 | The legacy product list and detail do not reflow at 320 px / 400 % (L2/L3). | Accessibility | 2 | 1 | 3 | 9 | step 6 (S40, S41) |
| #377 | A saved list shows its H1 above „Auswahl ändern“ and „Freigabelink erstellen“ on every width. Owner question: should it take S11's title row? (S5-32 P1). | Code | 1 | 1 | 2 | 8 | after the owner decides |

### 3.2 Security, privacy and operations readiness

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-28 (#236) | Staging lags far behind `main`, and the next deployment has preconditions: points (1) – (10) of v5 (key-ring certificate, `ASPNETCORE_ENVIRONMENT`, SMTP credentials and tracking, `App__BaseUrl`, `X-Forwarded-Proto`, never `Development`, Traefik log masking, `JWT_KEY` removable).<br>**Done in sprint 06:** the client's nginx `error_log` no longer writes share tokens (S6-05), and the move of the users needs nothing by hand (S6-10a P8).<br>**New:**<br>**(11)** the public-address settings (#447);<br>**(12)** the published ports (#449);<br>**(13)** the health route (#448);<br>**(14)** the `backup` profile with `BACKUP_DIR` outside the checkout (TD-92);<br>**(15)** a proxy in front of `/api/` must not compress, or every change answers 409 because strong ETags turn weak (S6-20a R6). | Security | 3 | 5 | 1 | **40** | now (owner; GP.1) |
| #449 | `docker-compose.yml` publishes the API (5000) and the client (4200) on every host interface. Docker bypasses host firewalls. On a server the API would answer plain HTTP beside the TLS proxy, and a direct caller could choose its rate-limit counter through `X-Forwarded-For` (`ClientAddress`). Workaround in `TECHNICAL.md` §4.3. Options: bind to `127.0.0.1` in the base file, or a production override (S6-30 P3). | Security | 2 | 4 | 1 | **30** | now, before the next staging deployment |
| #447 | Compose does not pass `App:BaseUrl`, `Email:From`, `Email:DisplayName`, `Export:ToolUrl` and `ProblemTypes:BaseUri`. Staging mail links would point to `http://procureaccess.tech:4200`. Workaround: a host override (`TECHNICAL.md` §3.1). Values are the owner's (S6-30 P1). | CI/Ops | 2 | 3 | 1 | **25** | now, before mail goes out from staging |
| TD-92 (#455) | **Backup gaps** (S6-29 P1 – P3):<br>- the dumps stay on the host, so losing the host loses the backups (D3 (a));<br>- an unhealthy `backup` service shows only in `docker compose ps`; no alert;<br>- a dump of an emptied database counts as a success, and after 14 days the retention removes the good ones;<br>- the staging rehearsal (`COMPOSE_PROFILES`, the mount of `./scripts/backup`, `BACKUP_DIR` across deployments) has not run, so ROADMAP GP.2 stays open. | CI/Ops | 2 | 4 | 2 | 24 | GP.1 (sprint 07) |
| #414 | The personal data export (`createdByMe.projects`) leaves out the assessments of candidates. SPEC §8.4 asks for the projects "with their current content", so PRV-07's workspace part is incomplete. The deletion is done (cascade). The owner decides the shape; the proposal is in the issue (S6-25 P1). | Privacy | 2 | 3 | 2 | 20 | now (M5) |
| #450 | `roles revoke` does not update the security stamp, so a running session keeps the revoked role, e.g. `moderation:review`, for up to 8 hours. A grant applies only from the next sign-in. Fix: update the stamp on grant and revoke; S5-29 checks it on every request (S6-30 P4). | Security | 1 | 3 | 1 | 20 | before step 7 (M7); small enough for now |
| TD-29 ↑ | Placeholders before a release:<br>- the legal pages hold placeholders until WERTE.IT delivers Impressum, Datenschutzerklärung and Erklärung zur Barrierefreiheit (S6-28 P1);<br>- the criteria-suggestion channel is `mailto:kriterium-vorschlagen@example.invalid` until O-6;<br>- `check-release-config.mjs` now fails on a legal-page placeholder (S6-28), but no release pipeline runs it yet (S6-28 P2). | CI/Ops | 1 | 3 | 2 | 16 | release pipeline (sprint 07); content before G-P |
| #448 | The client's nginx forwards only `/api/`, so `/health/live` and `/health/ready` are not reachable for an uptime monitor outside the host. Options: a `location ^~ /health/` in `nginx.conf`, or a proxy route (S6-30 P2). | CI/Ops | 1 | 2 | 1 | 15 | GP.1, with the alert of TD-92 |
| TD-94 (#456) | `TimeSpan` settings anywhere in the API accept a bare number as **days**: `"15"` for `Email:Smtp:Timeout` means 15 days. Proposal: one shared reader that requires `hh:mm:ss` (S6-05 P2). | Code | 1 | 2 | 1 | 15 | now (quick win) |
| TD-62 ↑ | Personal data the export does not cover:<br>- lists of an organisation the user has left (`ICriteriaListDataExport` reads through the organisation filter; S5-19 P4);<br>- *(new)* `ModifiedByUserId` on projects, candidates, evidences and saved lists stays after an account deletion and is not exported beyond the user's own resources (ARCHITECTURE O-27; S6-20a P4, S6-20b P2). | Privacy | 1 | 2 | 2 | 12 | legal review O-4, before G-P |
| TD-93 | ⚖️ An account deleted after a dump comes back when that dump is restored, and nothing records the deletion so it could be repeated. The runbook warns (`TECHNICAL.md` §7; SPEC SD26, v2.68-2; S6-29 P4). | Privacy | 1 | 3 | 3 | 12 | legal review O-4, before G-P |
| TD-86 | The copy of an **archived** project can still be changed through `PATCH /configurations/{copyId}/criteria/{rev}`, because Configurator cannot see the project's status. Only the client shows it read-only (ARCHITECTURE O-30; S6-12a P2, D3 (a)). | Security | 1 | 2 | 2 | 12 | owner decision O-30; before G-P |
| TD-98 | Whoever knows an address can block its sign-in for up to an hour with 10 requests (`SignInAddress`). The lockout already allowed 15 minutes. Options: keep O-S6-2, or count only failed attempts per address (S6-26 P3, C177). | Security | 1 | 2 | 2 | 12 | owner; pilot feedback |
| TD-43 | Platform-role grants and revokes are logged at `Information`, which reaches only the console. No audit trail is stored (S4-25 R8, P4). | Security | 1 | 2 | 2 | 12 | decide with R-ADM-02 (step 7) |
| TD-44 | The licence notices do not cover the OS packages of the base images: `aspnet:10.0`, nginx, PostgreSQL, and Mailpit for development only (S4-27 P4). | Dependency | 1 | 2 | 2 | 12 | licence review before G-P |

### 3.3 Code, architecture and design

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-87 | **Lock and copy are not atomic.** Workspace checks the lock, and Configurator stores the copy in another context. An assessment saved between the two keeps its value, locks the new list and may belong to a criterion the new list lacks. The user then cannot see it and cannot unlock the list. Likewise, a rename landing between the copy and the project's version answers 409 although the copy was replaced (ARCHITECTURE O-31; S6-12a P1, S6-18 P1/R3, S6-20a R2). Options: a row lock on the project in both paths (raw SQL plus an allow-list entry), or a version bump of the project on every assessment. | Architecture | 1 | 3 | 3 | 12 | owner decision O-31, before G-P |
| TD-70 ↑ | Client copies of server values, kept in step by hand:<br>- `passwordMinLength` ↔ `Account:Password:MinLength`;<br>- `lockout` ↔ `Account:Lockout:*`;<br>- `NOTE_MAX_LENGTH`, `LIST_NAME_MAX_LENGTH` ↔ SPEC §7.2;<br>- *(new)* `TEST_PROCEDURES`, the five procedure keys of „Art der Prüfung“ on S34a, because `GET /catalog/filter-options` answers 404 without an active release (S6-17 P1).<br>S5-22 showed the fix: the API serves the value. | Architecture | 1 | 2 | 2 | 12 | with the next settings change; the procedures with 2026.1 |
| TD-34 (#240) | The dark values are generated and not reviewed, for example `on-surface-variant` ≈ `on-surface`. | Accessibility | 1 | 2 | 2 | 12 | O-S5 (Figma dark mode), GP.4 |
| TD-31 (#238) | `Criterion.SupersededByKey` holds one key, so a split cannot be stored. The carry-over of S5-21a follows the same single key. | Architecture | 1 | 3 | 3 | 12 | owner decision before step 6 |
| #299 | The short description is in the model and the export, but not in the criteria answer, the S11 card or the S11 search. | Code | 2 | 1 | 2 | 12 | with the 2026.x folder that brings short descriptions (K-1) |
| TD-47 ↑ | The UTC day is used for the criteria export's file name and „Info“ date and for the account export's file name. Between midnight and 1:00 or 2:00 German time they show the previous day. **There are now two 🔧 time zones:** `ShareLinks:TimeZone` (S5-23) and `Workspace:TimeZone` (S6-15, the „not in the future“ check of the evidence date), both `Europe/Berlin`. | Code | 1 | 1 | 1 | 10 | owner decision before G-P (proposal: one 🔧 time zone for all) |
| TD-88 (#453) | Two concurrent `POST …/test-evidences` for the same procedure: the unique index keeps the data right, but the second answers **500**, not 400 (S6-15 P5/R3). | Code | 1 | 1 | 1 | 10 | now (quick win) |
| TD-95 | Legacy remnants the proposal retirement left:<br>- `SERVICES.ProcureAccess.csproj` still references `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, and its global usings name Identity namespaces (S6-10a P4, S6-23 P3);<br>- `Permissions` and `CustomClaimTypes` stay in MODELS, read only by the API's global usings and the tests (S6-10a P5, S6-23 P2). | Code | 1 | 1 | 1 | 10 | now (quick win) |
| TD-96 | Registration and its confirmation token are stored in two saves, although both are now in one context. One transaction would be simpler; behaviour is unchanged (S6-10a P3/R5, S6-10b P4). | Code | 1 | 1 | 1 | 10 | touch |
| TD-82 | The `initial` bundle budget in `angular.json` is still 850 kB warning / 1 MB error (S5-02 P1). | Code | 1 | 1 | 1 | 10 | owner picks the values; any PR |
| TD-46 | Platform maps 400 to `validation`, but 413 and other non-400 bad requests answer with the RFC default `type` (S4-19 R4). | Code | 1 | 1 | 2 | 8 | touch (Platform error handling) |
| TD-97 | S32 and S34 need four requests for the facts of a project's copy: the whole copy, its criteria and filter values, only to show a name, a release, counts and app-type names. S34 makes six requests in all (S6-14 P4, S6-16 P1). | Code | 1 | 1 | 2 | 8 | touch, with the next change of the workspace answers |
| TD-83 | Large files:<br>- `criteria-select.ts`, 683 lines and growing (630 at v5);<br>- its spec, 1 085 lines;<br>- `ReleaseFolderValidator.cs`, 924 lines;<br>- *(new)* `candidate-evidence.ts`, 598 lines;<br>- *(new)* `CandidateEndpoints.cs`, 590 lines.<br>Not wrong, but hard for a successor to read. | Code | 1 | 1 | 3 | 6 | touch: split by step or rule group when next changed |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-49 ↑ (#311) | **Manual accessibility checks by the accessibility tester.** M2: the screen-reader check of an exported XLSX and the A11Y-07 pass of step 2 (with #102). Steps 3 and 4: S01 – S03, S12 – S16, S20, S21, S60, S61, the session warning and the login overlay. **New, step 5:** S30 – S34b, S37 and the remove dialog, with the announcement after a route change (S6-22a P4) and the double `alertdialog` at a limit switch (S6-01 P3). Until then M2 – M5 are development milestones, not pilot-ready ones. | Accessibility | 3 | 3 | 3 | 18 | GP.5 (sprint 07); owner schedules the tester |
| TD-84 (#451) | **Firefox: S80 loses the H1 focus after an API 5xx.** `e2e/shell/start-and-error-pages.spec.ts:109/110` failed on `main` in runs 44315856184 (merge of #404) and 44801181547 (merge of #430), and on the pull requests of S6-19a (#423) and S6-27 (#399). It passed on the re-run each time. The v5 watch trigger ("open an issue") has fired. | Test | 2 | 2 | 2 | 16 | now (a failed E2E job is a finding until its cause is named) |
| TD-85 (#452) | **Mobile-Safari failures without a named cause:**<br>(a) `identity/preferences.spec.ts:63`: axe found the home cards' text `#1c1b1f` on `#141317` (contrast 1.07) in dark appearance. Axe ran while the text still had its light colour. It failed on `main` twice: runs 44110586418 (merge of #376) and 44232836143 (merge of #397). S6-01 P2 proposed an issue that was not created during the sprint (now #452). Same class as #339; check whether the `mat-card` text colour transitions under reduced motion.<br>(b) `shell/shell.spec.ts:49`: „Menü“ not focused after Esc in the compact menu, run 44380257487 (merge of #410). | Test | 2 | 2 | 2 | 16 | now |
| TD-91 | S11 of a project's copy is not in `e2e/shell/reflow.spec.ts` (A11Y-09; S6-12b P3, S6-14 P6). | Accessibility | 1 | 2 | 1 | 15 | now, with #429 |
| #82 ↓ | Regression tests for the S1-10 zoneless fixes. Only the legacy product screens are left: Z1 (`ProductsList`), Z3/R5 (`ProductDetails`) and R4 (the `withProducts` race). Z2 went with the proposals (S6-23 P1). | Test | 2 | 2 | 3 | 12 | step 6 (the screens are rebuilt) |
| #436 | WebKit: `e2e/identity/account.spec.ts:120` (a sole Owner cannot delete the account) types before the dialog's first focus. It timed out once, in run 45184273467. This breaks the CLAUDE §3 E2E rule of S6-01 (S6-29 P8). | Test | 1 | 1 | 1 | 10 | now (quick win) |
| TD-99 | E2E hygiene:<br>- the keyboard helpers `tabTo`, `arrowTo…` and `settled` are copied in J1, `assessment.spec.ts` and J2 (S6-22b P3);<br>- two download waits have no bound: `account.spec.ts:60` and `s11-list.spec.ts:313` (S6-01 P4). | Test | 1 | 1 | 1 | 10 | touch: with the next journey or the next change of those specs |

### 3.5 CI, tooling and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-74 | **The development machine is short of disk space.** Today 1.1 GB free on a 30 GB root partition (97 %). The E2E build took it to 97 % in S6-01, and S6-06's local multi-browser run stopped for low memory at 331 of 507 tests. Most tasks left E2E to CI, because the one local stack was busy or did not fit. | CI/Ops | 2 | 2 | 1 | 20 | now (owner: grow the partition or move Docker's data root) |
| TD-51 | PRs that change the same lock file can merge without a rebase in between. The ruleset still does not require up-to-date branches (`strict_required_status_checks_policy: false`). S6-08 showed a merge can overtake a requested rebase (#364). | CI/Ops | 1 | 2 | 1 | 15 | owner: turn the setting on, or merge one at a time |
| TD-102 | **The Dependabot batch of 2026-11-23:**<br>- #440: Angular, 13 updates (22.3.0);<br>- #441: vitest;<br>- #442: the "other" group;<br>- #443: the dotnet group;<br>- #444: **Serilog.AspNetCore 11.0.0**, a major version — check its target framework against the rule "stay on .NET 10 LTS" (§9.5);<br>- #439: actions.<br>Merge one at a time (TD-51). | Dependency | 1 | 1 | 1 | 10 | now (weekly review) |
| TD-52 | Two runs of the merge of #275 (43098373045, 43098373165) have been `queued` since 2026-11-11. GitHub refuses to cancel or delete them. They block nothing. | CI/Ops | 1 | 1 | 1 | 10 | watch; GitHub Support only if they start to bother |
| #437 | On an empty database the PostgreSQL log sink writes before the migrations create its schema: `schema "logging" does not exist`, once. Not yet known whether the batch is lost; the console keeps the lines (S6-29 P9). | CI/Ops | 1 | 1 | 2 | 8 | touch (logging), before G-P |
| TD-77 | Prettier is configured, but CI does not run it, so `.prettierrc` is not enforced (S5-32 R4). | CI/Ops | 1 | 1 | 2 | 8 | owner: one formatting PR plus a CI check, or drop the config |

### 3.6 Documentation, design and licences

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-53 | The export's source line quotes BIT inklusiv (U109). The check against the BIT inklusiv permission is not recorded (S4-20a P2). | Dependency (licence) | 1 | 3 | 1 | 20 | owner, before G-P |
| TD-79 ↑ (#259) | **Owner documents.**<br>- **UX copy, from v5:**<br>  - drafts v31 U133 – U137 and v33 U140 – U144 still „Entwurf“;<br>  - U24 names „S11 (Details)“; U130 names S20 for `org.left`;<br>  - no `errors.generic` (the legacy `ERROR.GENERIC` is also used on S32, S6-12b P4);<br>  - `criteria.share.expiry.hint` writes the 🔧 values;<br>  - S16's excluded card „Nicht in Ihrer Liste“;<br>  - the organisation-switch announcement (TD-66);<br>  - `criteria.priority.change` unused, the `changeLabel` hint, §5.4 „Ändern“.<br>- **UX copy, new in sprint 06:**<br>  - §6.3 has no row for `test-status-conflict` (S6-15 P1);<br>  - S32's scope row says „1 Mindestanforderungen (A)“, because one key holds several counts (S6-14 P2);<br>  - German texts for `MatDatepickerIntl` (S6-17 P2);<br>  - no text for a failed autosave (S6-19a P2);<br>  - `assessment.procedure.derived` unused (S6-19a P3);<br>  - confirm `candidate.assessment.list`, U186 (S6-19b P1);<br>  - an hours variant of `errors.rateLimit`, because a day window shows „1440 Minuten“ (S6-26 P2);<br>  - `criteria.save.outdated.reload` (TD-89);<br>  - keys for #420 and O-29 (TD-90).<br>- **PRD v5 and Kriterienkatalog:** headers name old versions; the Kriterienkatalog §3.1, §7 and K-3 still name `SampleData.cs`. | Documentation | 2 | 2 | 2 | 16 | owner, next version of each document |
| TD-101 | `docs/CODEBASE_v1.md` has grown to **375 193 characters**, nine times the size at which Claude Code warns for `CLAUDE.md`. Since v4 of `CLAUDE.md` every task writes there. It is read by section and by search, so it still works, but a successor cannot read it whole. Proposal: one file per area (`docs/codebase/<area>.md`), or a v2 that keeps only the current state and leaves the task history to the protocols. | Documentation | 1 | 2 | 2 | 12 | owner decision; then one documentation session |
| TD-100 | **Documentation drift of sprint 06** (Claude's part, for the next documentation session):<br>- ARCHITECTURE §9.1 still says „until then S4-26 §6“ for the key store, which `TECHNICAL.md` §7.6 now documents;<br>- SPEC §9.2 could point to `TECHNICAL.md` §8, and TESTING could name an operations invariant that `MetricQueryTests` carries;<br>- CODEBASE §2.81 still says the session warning can be checked only on the E2E stack (fixed by S6-02) (S6-30 P5);<br>- Handoff §5.1 says "links with `aria-current`" for the tabs of S34, while ARIA tabs were built because `mat-tab-nav-bar` requires `[tabPanel]` (S6-16 P3);<br>- SPEC R-SHL-07 and SD17 name `content/legal/*.de.md` without the client folder (S6-28 P3);<br>- the S6-13 row of `SPRINT_06.md` does not say the slice rework moved there (S6-23 P5);<br>- `TASK_PROMPTS.md` names `S6-10_users_identityaccess.md` (S6-10b P5);<br>- the headings and status lines S6-09 P4 left as they are. | Documentation | 1 | 1 | 1 | 10 | next documentation session |
| TD-80 ↑ | **Figma.**<br>- **No frame for:** S02, S03, S12, S14, S15, S16, S20, S21, S60, S61, the session warning, the organisation switch, *(new)* S30, S31, S33, the remove dialog and the read-only tabs of S34a/S34b (S6-22a P2).<br>- **Differences to update in Figma:**<br>  - the sticky bar's edge inset and the finished step (S5-32 P2/P3);<br>  - the S11 layer `2034:1196`;<br>  - *(new)* S32: the last path item is a link in the frame but text in S34's frame and in the code, and the example products are in the wrong order (S6-14 P3);<br>  - *(new)* S37 opens over „Projekt bearbeiten“, while frame `2056:2519` shows it over the page (S6-21a);<br>  - *(new)* „Beitragen“ is out of the header until S53, unlike frame `2017:650` (S6-23 P6).<br>- **Not compared side by side in a browser:** S33, S34 and S34a, only by structure (S6-16 P6, S6-17 P5).<br>- From v4: S13 D1 – D5, filled vs outlined icons, S04, S80 and the legal pages. | Documentation (design) | 1 | 2 | 3 | 9 | design owner, before G-P |

## 4. Done in sprint 06

| Item (v5 ID) | What was done | Task |
|---|---|---|
| TD-55 (#379) | WebKit's double session warning was a real defect: `SessionCookieEvents.AbsoluteExpiry` kept the fraction of a second, so the probe and the headers named different instants. The Chromium overlay failure was a test race. New CLAUDE §3 rule: in a dialog, wait for its first focus | S6-01 |
| TD-56 (#380) | J1's download waits bounded at 30 s; the cause was the old `exportThroughS13` race, fixed by S5-25b | S6-01 |
| #375, #378, #350, TD-64 (#385), TD-71 | S16 list gate; forced-colours edge on the sticky bar; English `aria-label` gone; the health watcher sends no cookie; dead CSS, model, 14 `todo.txt` files and commented-out storage lines removed | S6-02 |
| TD-65 (#386) | Return URL after a guarded deep link, with `safeReturnUrl` against open redirects | S6-03 |
| #368 | `en`/`es`/`fr` and `LanguageService` removed; the M3 group of `LEGACY_EXCEPTIONS` is empty | S6-04 |
| TD-58 (#382), TD-59 (#383), TD-57 (#381, repository part) | 🔧 `Email:Smtp:Timeout` 15 s; `AccountTokenCleanupService`; the client's `error_log` at `crit`. The Traefik part stays in TD-28 (9) | S6-05 |
| #355, #339, TD-48 | Firefox `libxul` crash found: reading `ariaLabelledByElements` in two worlds. `keepAxeOffLabelledByElements` in the axe helper; J1 runs on Firefox again (the M3 exit check); S10 waits for its field. The other Firefox workarounds were retired by #434; named sections are allowed again (TESTING V412, V414) | S6-06, #434 |
| TD-73 (#388) | CI smoke for `ng serve` (stage 1) and for the `immutable` header of lazy chunks (stage 6) | S6-07 |
| TD-76, TD-17 | Dependabot batch #363 – #366 merged one at a time; Playwright image pinned by digest; `minimatch` 11.0.9; alerts 12 and 13 fixed on 2026-11-18 | S6-08 |
| TD-78 (#389) | Sprint-05 and sprint-06 findings in ARCHITECTURE, TESTING, SPEC, Handoff and the Figma mapping; eight code comments cite by section; `CLAUDE.md` down to 37 763 characters (state moved to CODEBASE §2.0); ROADMAP v2 – v4 deleted | S6-09 |
| TD-72 (#387), TD-75 | The users and the seven Identity tables in IdentityAccess; endpoints, `GET /api/me`, the `roles` command and the seeders in the module; five foreign keys onto `identity."Users"`; soft-deleted accounts lose their role, claim, login and token rows; `TestAccounts` replaces the copied legacy helpers | S6-10a, S6-10b |
| v5 §5 saved lists without ETag; the 500 of a racing `PATCH` | ETag / If-Match on project, copy, candidate, evidence and saved lists; 428 and 409 `conflict`; S37 for the workspace forms, S11 and S10 | S6-20a, S6-20b, S6-21a, S6-21b |
| TD-61 (#384), v5 §5 proposals row, `DateTime.UtcNow` ×1, the API-02 test, `withProposal()`, one `LiveAnnouncer` | Proposals retired: service, controller, pages, store slice, tables (migration `RetireProposals`) and `moderation:review` of `PlatformAdmin`; EasyMDE removed; the API-02 test on S02 | S6-23 |
| v5 §5 NgRx "members cannot be overridden", callers of the legacy store methods, `<strong>` in `mat-error` | `withLoading()` once in the root store; slices declare their input; `LEGACY_EXCEPTIONS` 15 → 7, then 3 with S6-23 | S6-24 |
| v5 §5 account deletion and export parts of step 5 | Projects of an ending organisation deleted; `createdByMe.projects` in the export. The assessments are #414 | S6-25 |
| #318, TD-60 | Rate limits `SignIn` (30 per 10 min per IP), `SignInAddress` (10 per hour), `OrganizationCreation` (5 per day per user), all 🔧 | S6-26 |
| #296, TD-39 (#310) | One guarded reload when a lazy chunk fails; „Kriterien neu laden“ on S13 after `release-mismatch` | S6-27 |
| TD-29 (legal-page part) | Legal pages from Markdown in the client; the production check fails on a placeholder | S6-28 |
| #4 | Nightly `pg_dump` with 14-day retention and a rehearsed restore (`TECHNICAL.md` §7); `TECHNICAL.md` in nine chapters with the metric queries of R-MET-02, checked by `MetricQueryTests`; a README | S6-29, S6-30 |
| Invariants made `active` | WSP-01, WSP-02, WSP-03, WSP-04 (workspace part), SIG-03 (workspace part), SIG-13, CAT-15, CAT-17 (workspace part), API-03, API-04, TEN-05 (complete for step 5), ARC-05, PRV-07 (workspace part, without the assessments: #414) | S6-11 – S6-25 |
| `LEGACY_EXCEPTIONS` 16 → 3 | M3 1 → 0, M5 10 → 0, M2 5 → 3 | S6-04, S6-23, S6-24 |

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design, or waits for a later step. Do not fix it on its own; replace it with the step. Rows new or changed in v6 are marked *(new)* or *(changed)*.

| Debt | Retires with |
|---|---|
| *(changed)* **Legacy filter.** These go with the activation of 2026.1 (after O-1):<br>- the switch `configuratorReleaseGuard` → `/filters` on 404, `LEGACY_FILTER_PATH`, the main-nav path constants and nav "current" on `/filters`;<br>- #52;<br>- CAT-10 with `ExistingCatalogSeed`, `LegacyCatalogReadTests` and `ProductsByCriteriaFilterTests`;<br>- `marked`, `ngx-markdown` and their provider. Decide the legal-page renderer first (ARCHITECTURE O-32, S6-28 Q1): the pages use the project's own parser, while O-S6-6 named `ngx-markdown`;<br>- the **3** M2 entries of `LEGACY_EXCEPTIONS` (`criteria-list.ts`, `criteria-api.service.ts`, `filters-api.service.ts`), then #158 closes;<br>- the stepper icons `create`, `done`, `warning`;<br>- *(new)* `/filters` as the default path of `scripts/smoke-dev-server.mjs`: name another lazy route with a package import (S6-07 P2);<br>- the 404 of `/api/v1/catalog/*` on staging | legacy removal |
| *(changed)* **Directory rebuild, step 6:**<br>- `withProducts()` / `withFavorites()` and the legacy product screens (S6-13 D3);<br>- `ProductsController`, the ARC-05 exception until then (S6-23 D3);<br>- the commented-out `FavoritesController.cs`;<br>- legacy product reads ignore `Product.IsDeleted`, and `GetByCriteriaFilterIds` lacks the proposal filter;<br>- the `LiveAnnouncer` call in `products-list.ts`, the **last** one;<br>- *(new)* the dead `UpdateProductDto`, `UpdateCriterionDto` and the client's `update-product.dto.ts` (S6-23 P4);<br>- `[Produces]` / `[ApiVersion]` on legacy controllers, and with the last of them `AssumeDefaultVersionWhenUnspecified` and its `#pragma warning disable AV0016`, the only suppression left (S6-10a P7) | step 6 |
| *(new)* **Step 6 parts of the workspace:**<br>- the name-match hint and „Merken“ (R-CAN-01; `SoftwareProductId` stays `null`, S6-15 P3);<br>- „Aus dem Produktverzeichnis übernehmen“ (S6-14 D3);<br>- the publish button and `candidate.publish.cta.hint` with S35 (S6-17 P3);<br>- published assessments and `procedureAnswers` in the directory (S6-18 P6);<br>- the published entry of a removed candidate, the second half of WSP-04 and R-CAN-09 (`candidate.remove.publishedNote` unused; S6-22a P3);<br>- the rest of J2: publication blocked by „Nicht bekannt“, derived rows, publishing, the directory (S6-22b P2);<br>- the `directory` area in `AREAS` of `check-i18n.mjs`, after which the hand-typed `directory.entry.row.*` keys are generated (S6-14 P5);<br>- „Beitragen“ back in the main navigation with S53 (S6-23 P6, D1 (a));<br>- named regions for K1 and K2 (SIG-09); the unnamed sections of V226, V229, V355 and S6-16 D5 get their names when their screens change (TESTING V414);<br>- the metric queries for published against private entries and for the growth of the two channels (S6-30 P8, D2 (a)) | step 6 |
| *(changed)* **Account deletion and export, later steps:**<br>- contributor profiles detached, entries withdrawn, experience reports purged (PRV-06, PRV-10);<br>- deleting an account does not yet remove its contributor link (S6-10b P3);<br>- the export section `contributions`.<br>The step-5 part is done except #414 | steps 6 – 7 |
| *(changed)* **Deferred invariant parts:**<br>- CAT-15 and CAT-17 directory parts;<br>- SIG-08 and PRV-02 client cases, tested but untagged (S6-16);<br>- TEN-04 directory part;<br>- TEN-06 for `CatalogEditor` and `PlatformAdmin`, with their users in the Tenants fixture;<br>- API-05 for publication and contribution;<br>- API-06 for the directory and workspace lists;<br>- PRV-04 identity part, whose endpoint scan must skip `GET /api/v1/account/export`;<br>- the A11Y-02 keyboard variant of J3;<br>- the EXP-05 file-store part;<br>- R-SHL-03's switch test for projects and the compare select | their steps |
| *(new)* **Step 7:**<br>- the file store in the backup and the runbook (S6-29 P5, ARCHITECTURE §10.4);<br>- a policy of its own for `moderation:review` (S6-23 P6);<br>- the role-change audit trail (TD-43);<br>- the revoked role in a running session (#450) at the latest | step 7 |
| The snackbar is still the error channel for non-5xx errors (UX copy §4.1) | its last legacy caller, step 6 |
| *(changed)* Legacy routes without a page-title key and without an H1: `/filters`, `/products`, `/product/:id`, `/favorites`. The proposal routes are gone | rebuild of each screen |
| The API sends English validation messages without codes for legacy endpoints; the invitation endpoint answers a member's address with an English 400 that the client prevents (S5-17b) | each step's module validation |
| Uppercase legacy roots in `de.json`; `ERROR.GENERIC` waits for `errors.generic` (TD-79) | rebuild of each screen |
| No backend test of the legacy seeded exclusions, only J1 | step 6 or legacy removal |
| *(changed)* 16 TODOs: 15 in legacy client features, controllers, repositories and entities, one deliberate fixture value in `e2e/support/axe-helper.spec.ts:184` | features retired or rebuilt in step 6 |
| E2E: no `E2E` environment; the stack runs as `Development` with `RebuildDatabase`. Once it gets one, a soft-deleted seeded user breaks the next start (S5-14 P5). No products in the seeder | step 6 |
| *(new)* The Development rebuild deletes organisations through IdentityAccess only. Criteria lists and share links of a deleted organisation stay in the Configurator and Platform contexts; this affects Development only (S6-10b P2). A rebuild that asks each module through `IOrganizationDataDeletion` would end it | touch (the rebuild) |
| Unused legacy `Member` role rows in existing databases, including staging (S5-12a) | pre-pilot reset (O-S5-4) |
| Legacy projects fail on a warning only in CI, not locally | each migration step |
| K5 „Warum diese Priorität?“ toggle outside a detail panel: no UX key for the hide state. S34b does not need it (S6-19a) | first screen with K5 outside S11 |
| Display names are not normalised (trim, NFC) before the uniqueness check. S40 reuses `common.actions.clearInput` | step 6 (S50, S40) |

**Not tracked on purpose** (unchanged): CI has no NuGet or Docker cache; Serilog stamps with the system clock; files without logic count for coverage; old JWTs stay in the `localStorage` of browsers used before step 3 and are never read; the host ships the guarded `TestClock`. *(new)* The 428/409 helpers exist twice, in `WorkspaceAccess` and `ConfigurationEndpoints`, because `Platform.Contracts` has no ASP.NET reference (S6-20b R3). The export reads `FindOfProjectAsync` once per project (S6-25 R2). `MetricQueryTests` seeds once per query (S6-30 R5).

The won't-fix findings of the sprint-06 reviews that need no follow-up are listed only in their protocols. Examples: S6-01 R1/R2, S6-03 (fragment with `//`), S6-05 (redemption at the expiry), S6-11 R3/R4, S6-12a R1/R2, S6-15 R4/R5, S6-17 (1)–(4), S6-18 R4–R7, S6-19a (1)/(2), S6-19b (1)/(2), S6-20a R3/R8, S6-20b R2/R4, S6-21a (1)/(2), S6-21b (1)–(3), S6-22a (1)–(4), S6-23 R4/R5, S6-25 R3, S6-26 (partitions, address length), S6-27 R3–R5, S6-28 R3–R6, S6-29 R3, S6-30 R6.

## 6. Decisions waiting for the owner

CLAUDE §6.6: Claude Code must not decide these. Decided in sprint 06 (see `SPRINT_06.md` and the protocols): O-S6-1 – O-S6-6, the task decisions of S6-01 – S6-30, and from v5 §6: TD-72, #318/TD-60, TD-58, TD-61, TD-39, the Firefox report to Mozilla (not reported, S6-06). O-S6-7 was carried out with this review on the owner's go-ahead (2026-11-24, §7).

| Decision | Source | Needed by | Claude's proposal |
|---|---|---|---|
| **TD-28** staging deployment: points (1) – (15) of §3.2 | #236, S6-30 | GP.1 | the checklist on #236, extended with points (11) – (15) |
| **#449** published ports: loopback in `docker-compose.yml`, or a production override | S6-30 P3 | before the next staging deployment | loopback in the base file; the local stacks keep working on `localhost` |
| **#447** the values of `App:BaseUrl`, `Email:From`, `Email:DisplayName`, `Export:ToolUrl`, `ProblemTypes:BaseUri` for staging and production | S6-30 P1 | before mail goes out from staging | the variables as in the issue, defaulting to `appsettings.json` |
| **#448** health route: `nginx.conf` or the proxy | S6-30 P2 | GP.1 | `nginx.conf` (no new component) |
| **TD-92** off-host backup copy and the alert | S6-29 D3, P2, P3 | GP.1 | — |
| **#420** the dialog to assign or replace a project's list, its place on S32, its UX keys, and whether replacing asks first | S6-12b P1, S6-14 P1 | sprint 07 (M5) | reuse S31's select; confirm before replacing |
| **#414** shape of the assessments in the export | S6-25 P1 | sprint 07 (M5) | `assessments [ { number, title, value, note } ]`, stored rows only |
| **#429** the text of the last path item on S11 of a copy | S6-22b P1 | with #429 | (a) the page's H1 |
| **O-27 / TD-62** `ModifiedByUserId` and the lists of left organisations in the export and on deletion | S6-20a P4, S5-19 P4 | legal review O-4 | — |
| **O-28** does assigning a list move the project's „last change“ | S6-11 P3, S6-12a P4 | any time | yes, as candidates and evidences do |
| **O-29 / TD-90** deleting a saved list after a conflict | S6-21a P2 | any time | — |
| **O-30 / TD-86** server refusal for the copy of an archived project | S6-12a P2 | before G-P | refuse on the server: Configurator asks Workspace through a contract |
| **O-31 / TD-87** lock and copy atomic | S6-18 P1 | before G-P | a row lock on the project in both paths |
| **O-32** legal-page renderer: keep the project's parser or use `ngx-markdown` | S6-28 Q1 | before the legacy filter goes | keep the project's parser |
| **TD-93** ⚖️ restore and deleted accounts | S6-29 P4 | legal review O-4 | a deletion log the restore replays |
| **TD-98** per-address sign-in limit or failures only | S6-26 P3 | pilot feedback | keep until the pilot |
| **TD-29** legal page content from WERTE.IT; the O-6 channel | S6-28 P1 | before G-P | — |
| **TD-101** split `CODEBASE_v1.md` | this review | next documentation session | one file per area under `docs/codebase/` |
| #158 milestone: G-R, or a new milestone "Legacy removal (2026.1)" | §7 | any time | a new milestone, which #52 joins too |
| **O-S6-8** "release 2026.1 active" in the G-P gate | SPRINT_06 | G-P planning (sprint 07) | yes |
| **M5.5** tick at the sprint review: J2 is green on all five browser projects on `main` since 98ea8031 | S6-22b P4 | sprint review | yes |
| **TD-79** UX copy: the v5 list and the ten new keys of §3.6 | S6-09 P1 and the protocols named there | next UX copy version | — |
| **TD-80** Figma frames and differences | S6-14 P3, S6-21a, S6-22a P2, S6-23 P6 | design, before G-P | — |
| **S6-16 P3** tabs of S34: ARIA tabs as built, or links with `aria-current` as the Handoff says | S6-16 F8 | any time | keep the ARIA tabs and update the Handoff (TD-100) |
| **TD-74** disk space on the development machine | S6-01, S6-06 | now | — |
| **TD-51** require up-to-date branches | S6-08 P4 | next Dependabot batch (TD-102) | turn it on |
| **TD-77** Prettier: enforce or drop | S5-32 R4 | any time | enforce (one PR, then a stage-1 check) |
| **TD-82** initial budget values | S5-02 P1 | any PR | ≈ 800 kB warning / ≈ 950 kB error |
| **TD-47** date of the export files; two 🔧 time zones | S4-19 P5, S5-27, S6-15 | before G-P | one 🔧 time zone for all |
| **TD-69** H2 high contrast, or what `.contrast-more` shows until then | S5-25b R4, S5-26 P2 | before G-P | — |
| **TD-81** (O-25, O-26) leaving or deleting an organisation; changing the address | S5-16a P4, S5-28a P5 | when users need it | — |
| **#377** a saved list takes S11's title row | S5-32 P1 | any time | — |
| **TD-43** stored audit trail of role changes | S4-25 P4 | with R-ADM-02 | — |
| **TD-53** BIT inklusiv permission for the export's source line | S4-20a P2 | before G-P | — |
| K5 menu on „Geprüft nach …“ cards: the Handoff says "priority B, deselectable", the code allows a level change | S4-14 P2 | any time | — |
| **#299** does the S11 search include the short description | S4-24 P2 | with #299 | — |
| **TD-31** splits (#238) | S3-14 P2 | before step 6 | — |
| **TD-34** dark values (O-S5, #240) | S4-29 P3 | GP.4 | wait for the Figma dark mode |
| 🔧 `RateLimits:Invitation`, UX-O11 registration limit behind one NAT address | S5-17a P3, S5-12a R7 | pilot | keep |
| Project team: priorities, rationales and plain-language titles of 2026.1 (O-1, K-1, K-6); short descriptions; the criteria-suggestion channel (O-6) | S3-27, S4-11, S4-15b, S4-24 | M2.3; O-6 before G-P | — |

## 7. Issue hygiene

- **Milestones moved on 2026-11-24** (owner's go-ahead, O-S6-7):
  - #82 and #170 → M6 (both are legacy product screens), #377 → G-P; **M3 closed**.
  - #236 and #259 → G-P (staging is GP.1); #52 and #299 stay in M2 (#52 goes with #158, #299 waits for K-1).
  - #429 → M5 (it belongs to R-PRJ-06), #436 and #437 → G-P. No open issue is left without a milestone.
- **Open milestones:**
  - **M5** has #158, #414, #420, #429 and #453. Close it when #414, #420, #429 and #453 are done. #158 waits for the legacy removal, and no milestone covers that yet (§6).
  - **M2** has #52 and #299.
  - **G-P** has 15 open issues, among them TD-28 (#236), #447 – #449, #451, #452 and #454 – #456.
- **Follow-ups without an issue:** the protocols named S6-01 P2 "proposed issue", S6-27 P7 "an issue if it recurs" (it did), S6-21a P2 "proposed issue", S6-27 P1 "follow-up issue", S6-15 P5 "small follow-up issue" and S6-05 P2 "follow-up issue if wanted", but no issue was created. This register tracks them as TD-85 (a), TD-84, TD-90 (now O-29), TD-89, TD-88 and TD-94; all but TD-90 have an issue now (§8).
- **#240** keeps only its dark part. The comment that the light part is done is on the issue (2026-11-11).
- **#236** got a comment with points (11) – (15) of TD-28 (2026-11-24).
- **CI:** three failed runs on `main` without a named cause, now #451 (TD-84) and #452 (TD-85). Runs 43098373045 and 43098373165 are still `queued` (TD-52).
- **Open Dependabot PRs** for the weekly review: #439 – #444 (TD-102).
- **Alerts:** none open. The alerts closed for TD-17 (12, 13) are really closed (`fixed`, 2026-11-18).
- **Labels:** #436 and #437 have only `bug`; #429 and #420 have no `tech-debt`, which is right for requirement gaps.

## 8. New issues

Created on 2026-11-24 for the **TD-** items that need one (owner's go-ahead). Each holds the problem, the fix and "done when".

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-84 | #451 E2E flake: Firefox loses the H1 focus on S80 after an API 5xx (`start-and-error-pages.spec.ts:109`) | tech-debt | G-P |
| TD-85 | #452 E2E flakes on mobile Safari: axe in dark appearance on the home page, compact menu focus after Esc | tech-debt | G-P |
| TD-88 | #453 Two concurrent test evidences of the same procedure answer 500 instead of 400 | bug, tech-debt | M5 |
| TD-89 | #454 S12: reload the criteria after `release-mismatch`, like S13 (needs a UX key) | tech-debt, question | G-P |
| TD-92 | #455 Backup: off-host copy, alert on an unhealthy service, check against empty dumps, staging rehearsal | tech-debt | G-P |
| TD-94 | #456 `TimeSpan` settings accept a bare number as days | tech-debt | G-P |
| TD-28 | Comment on #236 (2026-11-24) with points (11) – (15) of §3.2 | — | G-P |
| TD-91 | Comment on #429 (2026-11-24): add S11 of a copy to `reflow.spec.ts` in the same PR | — | M5 |

TD-86 and TD-87 got no issue: they are tracked as O-30 and O-31 in ARCHITECTURE §16.5; an issue follows if the owner wants them on GitHub too. The other new TD items need no issue: TD-90, TD-93 and TD-98 are owner decisions (§6). TD-95 – TD-97 and TD-99 are touch or quick-win items. TD-100 and TD-101 are documentation. TD-102 is the weekly review.

## 9. Fix plan — when to fix what

### 9.1 Sprint 07 — G-P and the rest of M5

ROADMAP's preview for sprint 07: staging from CI (GP.1, after TD-28), dark theme (GP.4), the tester's pass (GP.5), the rest of GP.d, and the start of M6. New debt budget **≈ 6 points**.

| # | Work | Items | Pts | Note |
|---|---|---|---:|---|
| 1 | **Name the causes of the E2E failures on `main`:** Firefox S80 focus, mobile-Safari axe in dark and menu focus; the WebKit dialog wait | #451, #452, #436 | 1 | first; a red run on `main` puts every PR's E2E in doubt |
| 2 | **Finish M5:** the assessments in the export; the path back from S11 of a copy with its reflow case; assign and replace a list on S32 | #414, #429, TD-91, #420 | 0 (M5) | M5 points, after the owner's shape and design (§6); about 3 points |
| 3 | **Deployment hardening in the repository:** ports, public-address settings, health route, role revoke with the stamp; the backup alert and empty-dump check | #449, #447, #448, #450, #455 (repository part) | 2 | with GP.1; the values are the owner's |
| 4 | Quick wins in one PR: evidence race 400; `TimeSpan` reader; SERVICES Identity package and the MODELS claims; the log sink on an empty database | #453, #456, TD-95, #437 | 1 | |
| 5 | Dependabot batch, one at a time; check Serilog.AspNetCore 11 against .NET 10 | TD-102 | 0.5 | after TD-51 is decided |
| 6 | Documentation session: drift of sprint 06; split CODEBASE if decided | TD-100, TD-101 | 1 | last |
| 7 | S12 reload after `release-mismatch` | #454 | 0.5 | after the owner's key; before 2026.1 is activated |
| — | Owner: staging (TD-28) and its values; disk space (TD-74); the tester (TD-49, GP.5); O-27 – O-32; legal texts (TD-29); the UX copy list (TD-79) | | 0 | |

### 9.2 Before the pilot (G-P) and the public release (G-R)

| Fix | Items |
|---|---|
| Staging on `main` with every precondition, closed ports, public-address settings, health route | TD-28, #449, #447, #448 |
| Backup with an off-host copy, an alert and the staging rehearsal (GP.2) | TD-92 |
| Manual accessibility checks of M2 – M5 by the tester (GP.5), with the announcement findings | TD-49, #102, TD-41 |
| Privacy: export of assessments, `ModifiedByUserId`, lists of left organisations, restore after deletion, another user in another tab | #414, TD-62, TD-93, TD-63 |
| Correctness of the workspace: the archived copy on the server, lock and copy atomic | TD-86, TD-87 |
| High contrast H2, or a decision on `.contrast-more` | TD-69 |
| Licence review: OS packages of the base images, the BIT inklusiv permission | TD-44, TD-53 |
| Release pipeline that runs `check-release-config.mjs`; the legal texts and the O-6 channel in it | TD-29 |
| Export dates and one time zone, if the owner decides so | TD-47 |
| Figma frames for the M3 – M5 screens | TD-80 |
| The axe suppression list stays empty (TESTING §12.4) | any new entry |
| A role revoke takes effect at once | #450 (step 7 at the latest) |
| G-R: dark values reviewed and pinned with the Figma dark mode; a `SECURITY.md` for private vulnerability reports (S6-30 P6) | TD-34 |

### 9.3 M6 – M7 and the legacy removal

| Milestone | Fix | Items |
|---|---|---|
| M6 | Splits decided before CAT-03/04; directory rebuild (§5 "Directory rebuild", "Step 6 parts of the workspace"); product list and detail reflow; the zoneless regression tests; display-name normalisation; API-06 and TEN-04 directory parts; contributions in the export; account deletion detaches contributor profiles | TD-31, #170, #82, §5 |
| M7 | File store in the backup and `/health/ready`; EXP-05 file-store part; role-change audit trail with R-ADM-02; `moderation:review` policy; experience reports purged on deletion | TD-43, #450, §5 |
| Legacy removal (2026.1 activated, needs O-1) | Everything in the first row of §5; #52 and #158 close; the S12 reload (TD-89) must be done before; the procedure keys served by the API (TD-70) | §5, #52, #158, TD-89, TD-70 |

### 9.4 Watch items — act only when the trigger happens

Rows new in v6 are marked *(new)*. Removed from v5:
- "`start-and-error-pages.spec.ts` loses the H1 focus in Firefox again": it fired, now TD-84.
- "ESLint accepts minimatch 11": done (TD-17).
- The `en`/`es`/`fr` part of "English texts are planned": done (#368).

| Trigger | Then | Source |
|---|---|---|
| *(changed)* A Playwright or Firefox update | Check whether Firefox still crashes when `ariaLabelledByElements` is read in two worlds. If not, `keepAxeOffLabelledByElements` in `e2e/support/axe.ts` can go. Never read that property in a test (TESTING V407) | TD-48, S6-06 P2 |
| A Dependabot PR updates `@playwright/test` | Bump the image in `e2e/playwright-image/Dockerfile` on the same PR, **by digest**: Microsoft re-pushes tags (S6-08) | S4-01b F5, S6-08 |
| A Dependabot PR updates `material-symbols` | Run `scripts/subset-material-symbols.sh` on its branch | S3-25 P3 |
| A Dependabot docker PR bumps `sdk:10.0.x` | Push the matching `global.json` change onto it; run `check-api-notices.mjs --write` if stage 2 fails | S5-03 P1 |
| An update of `Microsoft.Extensions.Identity.Core` | Recheck the two overrides of `ClockedUserManager`, which copy Identity 10.0.15 | S5-13a |
| *(changed)* A .NET 11 package (EF Core, ASP.NET Core, Npgsql 11 and `TimeProvider.Testing` 11 are out; Serilog.AspNetCore 11 is #444) or TypeScript 7 / `@types/node` 26 | Stay on .NET 10 LTS and TypeScript 6; close or ignore until a decision. A package whose new major still targets `net10.0` may be taken after its licence check | scan 2026-11-24 |
| *(changed)* More than one API instance | These assume one instance: migrations, log retention, the usage-event queue, the first Data Protection key, the in-memory rate limiters (now also `SignIn`, `SignInAddress`, `OrganizationCreation`), the `KeyedRateLimiter`, the `PasswordResetMailQueue`, *(new)* `AccountTokenCleanupService` and `InvitationCleanupService` | S3-23, S4-21a, S5-13a, S6-05, S6-26 |
| *(changed)* A new reverse proxy, log sink or log pipeline | It must send `X-Forwarded-Proto` from a trusted address, mask `/geteilt/…` and `/api/v…/shared/…`, *(new)* **not compress `/api/`** (strong ETags turn weak, and every change answers 409), and route `/health/` if monitored (#448) | S5-07, S5-23, S6-20a R6 |
| A new environment name | Check `SessionCookiePolicy` (Secure is relaxed in Development/E2E) and `TestClockGuardTests` | S5-07 3a, S5-30 |
| *(new)* A restore | Raise `Concurrency:ETagEpoch` (`CONCURRENCY_ETAG_EPOCH`) and stop the `backup` service first, as `TECHNICAL.md` §7 says; never start the API as `Development` on a restored database (the rebuild deletes every user) | S6-29 |
| Load shows the per-request work | Raise 🔧 `Session:SecurityStampValidationInterval`; skip the membership query by endpoint metadata; revisit `no-store` on signed-in catalog reads. *(new)* Each autosave resolves the whole list (S6-18 R4) | S5-29, S5-15 R2, S6-18 |
| *(new)* The error log alerts on EF's `DbUpdateException` from a repeated assessment write | It is the expected race of S6-18 R5; lower its level | S6-18 R5 |
| *(changed)* An organisation's lists **or projects** grow large, or S14/S30 get slow | Paging for `GET /configurations` and the project list together (ARCHITECTURE §7.1 says offset-based everywhere) | S5-20a R3, S6-11 P5 |
| Users report the session warning at the wrong time | Send the server's time with the expiry headers and keep the offset on the client | S5-30 R6 |
| A stored release is changed in place, or CAT-05 is relaxed | `PersistedConfigurationSource` diverges from the stored `ResolvedLevel`, and an unresolvable selection answers 500 | S5-19 R4/R5 |
| More callers of `ICriteriaListDeletion` or `IShareLinks.DeleteOfOrganizationsAsync` | They need a membership check of their own | S5-28a R4 |
| The server's sole-owner refusal regresses | The read-only E2E tests would delete the shared seeded Owner | S5-28b R7 |
| `ShareLinks:DefaultExpiryDays` or `MaxExpiryMonths` change | Update `criteria.share.expiry.hint` (or give it placeholders, TD-79) | S5-24 P2 |
| A SPEC §7.2 field limit changes | Change the client constants, or serve them (TD-70) | S5-22 P5 |
| *(changed)* A Material datepicker is needed (S34a, S55) | German `MatDatepickerIntl` texts in the UX copy first, then the provider and `common.date.openCalendar` with `calendar_today` | S5-24, S6-17 P2 |
| Share links are abused | Add a rate limit | S5-23 |
| Owners complain about onboarding time | Revisit 🔧 `RateLimits:Invitation` | S5-17a P3 |
| *(new)* A rate-limit window misbehaves | The passing of a window is not tested for any policy, because the .NET limiters take no `TimeProvider`; write a limiter on `TimeProvider` | S6-26 P5 |
| *(new)* Developers hit 429 on the dev stack | Relax `SignInAddress` in `docker-compose.dev.yml` | S6-26 P4 |
| *(new)* 2026.x is activated | TD-89 must be done first; watch for a selection the new release excludes, which ends on S11's failure state with the generic notice, possibly twice | S6-27 P1/P2 |
| *(new)* A browser changes its chunk-load error message | Add it to `CHUNK_LOAD_MESSAGES` (the E2E on three engines catches it) | S6-27 P3 |
| *(new)* An offline regression of the chunk reload | Add an offline E2E (unit tests only today) | S6-27 P5 |
| *(new)* Users end on the browser's own error page after a deployment | A `/health` probe before the reload (owner decision) | S6-27 P6/R2 |
| *(new)* A third background job | A shared base for the timer loop that `InvitationCleanupService` and `AccountTokenCleanupService` repeat | S6-05 |
| *(new)* A sixth user setting | `UserPreferencesValues` is positional and the `PUT` treats a missing member as "keep": new members need the same optional-with-default handling; migration tests insert settings with SQL in the columns of their time | S6-19b F1 – F3 |
| *(new)* Material renames `.mat-mdc-radio-checked` | The edit dialog's focus on an archived project breaks (unit test and E2E catch it) | S6-14 R5 |
| *(new)* `ubuntu-24.04` drops Google Chrome | The `ng serve` smoke (`channel: 'chrome'`) fails loudly; install a browser in the step | S6-07 R2 |
| *(new)* Vitest's 5 s timeout bites in CI | S12, S13, the saved list, `export-dialog.spec.ts` and `members-page.spec.ts` come close under load; then raise the timeout for those specs | S6-24 P4, S6-20b, S6-27 |
| *(new)* `SPEC_v3.md` is written | About 50 `@spec docs/SPEC_v2.md` tags follow the Figma mapping §3 rule; rename them in the same PR, or change the rule to the pattern of CLAUDE §2 | scan 2026-11-24 |
| *(new)* The legal texts contain `mailto:` links, tables or images | Extend the parser; `pa-catalog-text` links only http(s); give a page with real headings its own axe assertion | S6-28 P1/R2 |
| *(new)* Users report it (pilot) | **Each has a proposal in its protocol:**<br>- a failed autosave that went unnoticed (S6-19a P2);<br>- lost input when leaving S33, S34a or S11 after a conflict (S6-17, S6-21b P2);<br>- the clipboard refused in S37 (S6-21a P3);<br>- Readers missing the S34b filter (S6-22a);<br>- a project's copy confused with its list in the export (S6-25 P2);<br>- the return target lost through the registration (S6-03 P1);<br>- no way to reset the remembered legend state on S60 (S6-19b P2);<br>- swapping two procedures needs two saves (S6-17 P4) | the named protocols |
| The E2E stack gets a real secret | Check the `compose.log` artefact again | S5-11 |
| A pilot office behind one NAT address hits the registration limit | Revisit the 🔧 value (UX-O11) | S5-12a R7 |
| A release has a criterion with three or more procedures, or the font changes | Recheck the tab labels at 320 px; the reflow test flags pagination (`scrollingTabBars` covers only S34) | S5-06, S6-16 P4 |
| A release has a requirement without a procedure text for an app type that selects it | The legend shows six levels and „Wie wird bewertet?“ none; fix the data or the rule | S5-31 R3 |
| A Content-Security-Policy is introduced | The `pixelsAt` E2E helper loads a `data:` URL | S5-32 R3 |
| The Angular/esbuild chunk-hash alphabet or length changes | Recheck the nginx `immutable` pattern (stage 6 checks it since S6-07) | S5-01 |
| A consumer needs OpenAPI 3.1/3.2 | Set `OpenApiVersion` in Swashbuckle 11 | S5-04 P4 |
| A client is served from another origin than the API | No CORS, and `SameSite=Strict` plus the relative XSRF header break: architecture decision first | S5-08 |
| Two open PRs write the same document version | The second to merge renumbers its version and the headers that name it | S5-05 P5, S6-09 P5 |
| MCR is slow or down | Add a retry to the image pull | S4-01b P4 |
| A Dependabot NuGet group opens mid-servicing more than once | Add a `cooldown` of 1–2 days for `nuget` | S4-06 P2 |
| The project moves to Node 26 | `.nvmrc` files and the client Dockerfile together | S4-06 P3 |
| A new version of a package in `EMBEDDED` of `check-api-notices.mjs` | Review what it embeds, update `scripts/api-notices/` and the pinned version | S4-27 |
| A ClosedXML release needs SixLabors.Fonts ≥ 2 | Do not take it (Split License fails D12) | S4-20b F6 |
| Anyone calls `AdjustToContents` or measures text in the export | Refuse: the API image has no fonts | S4-20b F4 |
| A new or renamed fixture quotes BIT inklusiv texts | Add its exact path to the BIT block of `REUSE.toml` | S4-04 R4 |
| 2026.x texts are exported | Check that the CSV formula guard does not prefix catalog text starting with `-` | S4-20a R2 |
| English texts are planned (SPEC §3) | ADR for English catalog texts, a second `ExportTexts` resource, an `Accept-Language` test | S4-23 P2, P3 |
| The project team's first release PR needs the database checks of the import | `catalog validate` or a CI step with a throwaway database | S4-05 P1 |
| 2026.1 gets its priorities | Document starting a kept dev database without `T.1` | S4-08 P2 |
| A later screen gets a sticky bar | Reuse the S11 `focusin` fallback and `pa-criteria-action-bar` | S4-18a P5 |
| Usability tests object to collapsing example groups after a tab switch | Keep the tab content attached | S4-15b P4 |
| A deployment ships a client before its API | Deploy client and API together | S4-15b P6 |
| A test asserts a closed Material overlay | Wait with `vi.waitFor` | S4-20b F7 |
| Material changes when it clears the active option | Recheck `chooseFirstOption` | S4-14b |
| S35 or S55 reuses `pa-stepper` | Give it a test of its own | S4-10 P8 |
| The next change to `Release2026_1Tests` | Read the folder from the repository, not the build output | S4-05 P2 |
| An unhandled startup exception is hard to see | The API container exits with 139, not 134; open an issue | S4-26 R5 |
| `eslint` / `typescript-eslint` versions drift between `e2e/` and the client | One Dependabot group for both | S3-26 P3 |
| The first real C# CodeQL finding, or a missed one | Revisit `build-mode: none` for C# | S3-03 R2 |
| No push to `main` for 60 days | Re-enable the weekly CodeQL schedule | S3-03 R4 |
| A module needs a map that is tedious by hand | ADR-015 rule 4: revisit Mapperly | S3-07 P4 |
| The API is meant for third parties | Revisit O-S3-2 (Swagger only in `Development`) | S3-06 P3 |
| A hotfix version older than the newest release is imported | Check `supersedes` by hand | S3-16 P2 |
| A size limit on `IMemoryCache` | Give the catalog entries a size | S3-17 P2 |
| A document defines a range for `PriorityRecommendation.Weight` or `Level`/`Source` | Add the CHECK constraints | S3-13 R6 |
| An icon is chosen by `fontIcon`, `svgIcon` or a CSS class | Extend the icon check | S3-25 R4 |
| A future SDK breaks NetArchTest | Fork `NetArchTest.eNhancedEdition`, with an ADR | S2-05 P3 |
| One of the gate's known limits bites | Read the result reports instead (option 1B) | S2-09 P1 |
| First deployment of a new module schema | The database role needs `CREATE` on the database, and on `public` for the ICU collation (`TECHNICAL.md` §4.3) | S2-07 P5, S4-25 P3 |
| Dependabot rebases a PR after the CI review | Check the head commit at merge time | S2-04 P5 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |

### 9.5 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`, which deletes the third-newest version.
  - Move done items to "Done", add the new findings of the sprint protocols and reviews, and re-score.
  - Check the milestones of new issues and the CI runs on `main`: a failed E2E job is a finding until its cause is named.
  - Check that every issue closed for a CodeQL or Dependabot alert really closed the alert.
  - Check that every protocol finding marked "follow-up issue" or "proposed issue" has an issue (§7). In sprint 06, six did not.
  - Fill in the sprint's close-out table.
- **Start of every migration step:** open its GitHub milestone, and plan its debt together with the step's invariants. Delete its `LEGACY_EXCEPTIONS` entries as files are replaced (#158).
- **Every screen task:** new icons go into the icon list. The screen joins `reflow.spec.ts` with each state that changes the layout, named in the task prompt (S11 of a copy was missed: TD-91).
- **End of every migration step:** review the axe suppression list (TESTING §12.4), and plan the manual screen-reader pass of the step with the tester (A11Y-07). Steps 3 – 5 have no pass yet (TD-49).
- **Weekly:**
  - Dependabot pull requests: check the licence of new versions before merging (CLAUDE §10), regenerate `THIRD-PARTY-NOTICES.txt` for NuGet updates, merge PRs that share a lock file one at a time, bump the Playwright image by digest with `@playwright/test`, and run the icon subset for `material-symbols`.
  - nginx: merge even minors (stable branch), close odd ones.
  - CodeQL alerts.
  - The CI runs on `main`.

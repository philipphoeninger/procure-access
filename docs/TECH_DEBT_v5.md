# ProcureAccess — Tech-debt register

> **Status:** v5 (2026-11-17) — output of Phase 4, step 14 ("Manage tech debt continuously") of `ai_development_process.md`, at the end of sprint 05, made with the skill `Engineering/tech-debt` (inventory → categorise → score → plan). Replaces `TECH_DEBT_v4.md`.
> **Sources:**
> - open and closed GitHub issues and pull requests (2026-11-17);
> - the open points and won't-fix findings of the 40 protocols `docs/sprints/sprint_05/S5-*.md` (S5-32 from its branch, PR #376 still open), `SPRINT_05.md`, `ROADMAP_v5.md`;
> - a scan of the code: TODOs, `DateTime.UtcNow`, suppressed warnings, `eslint-disable`, `LEGACY_EXCEPTIONS`, `LiveAnnouncer`, browser storage, document versions named in code, file sizes;
> - a Release build of `ProcureAccess.sln` with `-warnaserror` on SDK 10.0.404 (0 warnings);
> - `dotnet list package --vulnerable --include-transitive` (none) and `--outdated`;
> - `npm audit` / `npm outdated` (client and `e2e/`);
> - Dependabot and CodeQL alerts, the 45 CI runs on `main` since 2026-11-13, open pull requests and the ruleset of `main`.
>
> **Next review:** end of sprint 06 → `TECH_DEBT_v6.md`.

---

## 1. Summary

- **Sprint 05 finished M3 and M4 and cleared almost all of the v4 sprint plan (§9.1).** 13 debt issues were closed: #309, #308, #264, #306, #312, #313, #301, #302, #87, #88, #89, #234 and #279. Four more were created and closed within the sprint: #317, #319, #320 and #337. TD-12, TD-25, TD-26, TD-40, TD-42 and TD-45 are done, as are most M3 rows of §5 of v4 (§4). #278, #281 and #282 are done in PR #376, which is not merged yet.
  - About **11.5 of the sprint's ≈ 78.5 planned points went to explicit debt** (≈ 15 %): 3.5 general (S5-01 – S5-06) and 8 as M3 debt rows (M3.1d, M3.2d, M3.5d, M3.7d). Every P0 and stretch task is done; S5-32 is in review.
- **The legacy auth is gone.** JWT, `jwt-decode`, `Identity.UI`, `UsersController`, the `{ code, message }` failures, the legacy account and settings pages, `RefreshTokens` and `UICustomization` are all removed. Other numbers:
  - `LEGACY_EXCEPTIONS` fell from **32 to 16 files** (M2 5, M3 1, M5 10).
  - `DateTime.UtcNow` outside tests fell from 6 to **1** (`ProposalService`, step 5).
  - TODOs fell from 29 to 24 (23 in legacy code).
- **`main` is red (TD-55, top fix).** The last two CI runs on `main` failed in the new J5 session journey of S5-30:
  - WebKit (merge of #373): `j5-session.spec.ts:152` found **two** session-warning alert dialogs at once. This may be a real double-open, not only a test problem.
  - Chromium (merge of #374): `j5-session.spec.ts:198` failed a count check after „Schließen“ in the login overlay.

  Now #379. In total, 37 of the 45 CI runs on `main` since 2026-11-13 were green, one was cancelled and seven failed, all in E2E:
  - one was TD-38, fixed in S5-01;
  - three were the Firefox crash of #337, fixed by #348;
  - one is a new mobile-Chrome timeout of the saved export in J1 (TD-56);
  - two are J5 (TD-55).
- **The staging deployment (TD-28, priority 40) is still the top owner item, and it grew.** Since M3, staging would hold real accounts and send real mail. New preconditions:
  - SMTP credentials (the API no longer starts without them);
  - `App__BaseUrl` as the HTTPS origin;
  - Lettermint tracking off;
  - share tokens out of the nginx error log and the Traefik access log;
  - `X-Forwarded-Proto` from the proxy;
  - and never `ASPNETCORE_ENVIRONMENT=Development`, which relaxes the cookie `Secure` flag and allows the test clock.
- **58 rows are open and scored** (§3):
  - 22 carried from v4;
  - 8 for issues created during the sprint (#318, #339, #350, #355, #368, #375, #377, #378; #318 shares its row with TD-60);
  - 28 for the other new items from the protocols and this scan (**TD-55 – TD-83**).
- **Privacy gaps before the pilot.**
  - The legacy proposals a user made are personal data, but neither the data export nor the account deletion covers them (TD-61).
  - Sign-in has no rate limit (#318), nor has creating organisations (TD-60).
  - Expired account tokens are never deleted (TD-59).
- **The users are still legacy, and no roadmap package moves them (TD-72).** These all wait for "M3.1", which the roadmap marks done:
  - the Identity tables in the legacy context;
  - the host endpoints `API/Account`, `API/Auth`, `API/Organizations`;
  - `GET /api/me`;
  - four user ids without a foreign key.
- **Nothing breaks a product rule (CLAUDE §8).** No known vulnerability is in shipped code:
  - NuGet has none.
  - `npm audit --omit=dev` finds 0 in both packages.
  - CodeQL has no open alert.
  - The only Dependabot alerts are the dev-only `minimatch` (TD-17).

  The build has 0 warnings.
- **Owner material piles up (TD-79, TD-80).** Two UX copy drafts (v31 U133–U137, v33 U140–U144) are unconfirmed. Six more UX keys or wordings are open. Twelve M3/M4 screens have no Figma frame.
- **Local E2E is no longer reliable (TD-74).** In five tasks since S5-21b the machine had under 1–2 GB free. E2E then ran from old images plus `ng serve :4300`, so J5, J8 and S03 failed locally for set-up reasons only.

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

The formula ranks large work low. An item with Effort 5 that needs a roadmap package (TD-72) is marked so in its row, whatever its number.

**When:**
- *now*: sprint 06.
- *a migration step or milestone* (`ARCHITECTURE §12`, `ROADMAP`).
- *legacy removal*: when 2026.1 is activated and the legacy filter goes (needs O-1).
- *touch*: when the code changes anyway.
- *watch*: act only on a trigger (§9.5).

Items re-scored against v4 are marked ↑ or ↓.

## 3. Register (open items)

### 3.1 Defects users can see

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #375 | S16 shows `criteria.list.empty` for a shared list that holds only „Geprüft nach …“ criteria. The list sits inside `counts().total > 0`, and the counts leave those criteria out (O-S4-51). Fix: gate the list by the number of criteria (S5-31 P1). | Code | 2 | 1 | 1 | 15 | now |
| #378 | The sticky action bar has no visible top edge. In forced-colours mode its widening `box-shadow` is dropped too, so content scrolling under it has no boundary. Fix: `border-block-start` in `CanvasText` under `@media (forced-colors: active)` (S5-32 P4). | Accessibility | 2 | 1 | 1 | 15 | now |
| #350 | Signed in, the legacy product list fails axe `label-content-name-mismatch`: `aria-label="Product proposal button"` (English) on a button that shows `PRODUCTS.PROPOSE` (S5-18 P2). | Accessibility | 2 | 1 | 1 | 15 | now (one line), or step 6 (S40) |
| TD-64 (#385) | Under `npm start` the session never ends. `src/dev/dev-reload-health.ts` fetches `/health/live` every 5 s with the session cookie; the dev server forwards `/health`, so each fetch renews the session, and the warning never shows. Fix: `credentials: 'omit'` (S5-30 R4). Not affected: the E2E stack and production. | Code | 2 | 1 | 1 | 15 | now (quick win) |
| TD-65 (#386) | `authGuard` sends a signed-out deep link to S01 over the **start page** and has no return URL. After the sign-in the user stays on `/`. The guard now also protects new screens: saved lists, the organisation, `/konto`, `/einstellungen` (S5-09 P3, S5-28b P3). | Code | 2 | 1 | 2 | 12 | now |
| TD-66 | Switching the organisation outside S21 is not announced. The button's name changes and keeps the focus. Needs a UX key such as „Organisation gewechselt: {{name}}“ (S5-18 P3). | Accessibility | 2 | 1 | 2 | 12 | once the owner gives the key (TD-79) |
| TD-63 | When another tab signs in as a **different** user, the warning's probe takes that session into this tab without the S5-30 D3 handling. The previous user's in-memory data stays. This matters on shared devices (S5-30 R7). | Security | 1 | 2 | 2 | 12 | before G-P |
| TD-69 | `.contrast-more` applies the legacy, unreviewed colours to every visitor whose system asks for more contrast (S5-25 decision 3 (i)). „Hoher Kontrast“ on S60 (`settings.contrast.high`, already in `de.json`) and its colours wait for H2 (R-SET-04). Until then the API refuses `high`, and „Kontrast“ is a radio group with one option (S5-25b R4, S5-26 P2). | Accessibility | 2 | 2 | 3 | 12 | design H2, before G-P |
| TD-41 | The loading bar announces `common.loading` each time loading starts. On S10 every option change reloads the filter options. **New instance:** after a sign-in through the overlay, „Wird geladen …“ follows `shell.session.restored` at once, so a screen reader may cut off the first announcement (S5-30 R5). | Accessibility | 2 | 1 | 2 | 12 | in the tester's pass (TD-49); then no announcement for background reads |
| TD-39 (#310) | After `release-mismatch`, S13 shows only `criteria.export.outdated` with „Schließen“. The client keeps the old release pinned. The `ICatalogReleaseCarryOver` of S5-21a may serve the reload flow. | Code | 2 | 2 | 3 | 12 | before a second release is activated anywhere (2026.1) |
| #296 | A tab left open across a deployment cannot load a lazy chunk it has not fetched yet. The navigation then fails without a message. Proposed fix: `withNavigationErrorHandler` loads the target URL in full (S4-22 R1). | Code | 3 | 2 | 2 | 20 | G-P, at the latest before the first production release |
| #52 | The legacy `/filters` stepper has unnamed tabs. No test checks it any more (O-S4-21). | Accessibility | 2 | 2 | 2 | 16 | legacy removal |
| TD-67 | Content links outside the footer, such as S01's „Noch kein Konto? Jetzt registrieren“ and S11's suggestion link, use the browser's default link colour. `main.scss` has no shared content-link style (S5-09 P4). | Accessibility | 1 | 1 | 1 | 10 | touch |
| TD-68 | A second confirmation link opened on `/registrieren/bestaetigen` updates H1 and text, but not the page title. `PageTitleStrategy.changePageInPlace` exists for this since S5-13b (S5-12b R4). | Accessibility | 1 | 1 | 1 | 10 | touch |
| TD-81 | A sole member is the last Owner, so leaving answers 409 `last-owner`, and deleting an organisation is not designed (S5-16a P4). The only way out is deleting the account. Whether users may change their address is also open (S5-28a P5). | Code | 2 | 1 | 3 | 9 | SPEC questions (§6) |
| #170 ↓ | Only L2/L3 are left: the legacy product list and detail do not reflow at 320 px / 400 %. L1 (login outlet) was done in S5-09. | Accessibility | 2 | 1 | 3 | 9 | step 6 (S40, S41) |
| #377 | A saved list shows its H1 above „Auswahl ändern“ and „Freigabelink erstellen“ on every width. Owner question: should it take S11's title row? (S5-32 P1). | Code | 1 | 1 | 2 | 8 | after the owner decides |

### 3.2 Security, privacy and dependencies

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-28 ↑ (#236) | Staging lags far behind `main`, and the next deployment has preconditions. The v4 points:<br>**(1)** the key-ring certificate (`DATA_PROTECTION_CERTIFICATE`) and deleting the plain key rows once;<br>**(2)** the log search for the token-less reset;<br>**(3)** the API port not reachable from outside;<br>**(4)** `ASPNETCORE_ENVIRONMENT` and the v3 points.<br>**New in sprint 05:**<br>**(5)** `SMTP_USERNAME`/`SMTP_PASSWORD`, because the API stops without them outside Development/E2E (S5-11 P1); Lettermint open and click tracking off (SPEC §7.6); the sending domain with SPF/DKIM;<br>**(6)** `App__BaseUrl` as the HTTPS origin, because `appsettings.json` holds `http://procureaccess.tech:4200` and every mail link is built from it;<br>**(7)** the proxy must send `X-Forwarded-Proto`, otherwise antiforgery answers 500. A forged header from a trusted network also sets the scheme, which adds to (3) (S5-07);<br>**(8)** never `Development`: it relaxes `Secure` on the session and antiforgery cookies (S5-07 3a) and lets `TestClock__Enabled` move the server's clock (S5-30);<br>**(9)** mask share tokens in the Traefik/Dokploy access log (TD-57);<br>**(10)** `JWT_KEY` can be deleted.<br>Existing staging users sign in again and keep unused `Member` role rows until the pre-pilot reset (O-S5-4). | Security | 3 | 5 | 1 | **40** | now (owner, before M3 is shown on staging) |
| #318, TD-60 | Rate limits SPEC §7.3 does not list. `POST /api/v1/auth/login` has none, only the per-account lockout, which does not protect the space of addresses (S5-07 P4). `POST /api/v1/organizations` has none either, so one confirmed account can create any number of organisations (S5-16a P3). | Security | 2 | 3 | 2 | 20 | owner: 🔧 values in SPEC §7.3; then one task, before G-P |
| TD-61 (#384) | The legacy proposals a user made (`Proposals.ProposerId`) are personal data. They are neither in the §8.4 export nor removed by the account deletion (S5-27 P2, S5-28a P4). | Privacy | 2 | 3 | 2 | 20 | M5 (proposal retirement); if the proposals outlive the pilot start, export and deletion must cover them before G-P |
| TD-58 (#382) | `SmtpEmailSender` uses MailKit's default timeout of 2 minutes per operation and tries twice. A stalled SMTP server holds the registration, the confirmation resend or an invitation request for minutes. The reset is not affected, because it uses the background mailer (S5-11 P3). Proposal: 🔧 `Email:Smtp:Timeout` of about 15 s. | Code | 2 | 2 | 1 | 20 | now, after the owner's value |
| TD-57 (#381) | Share tokens travel in the path (`/geteilt/<token>`, `GET /api/v1/shared/{token}`). `ShareTokenMaskingEnricher` and the client's nginx access-log format mask them. The client's nginx `error_log` still writes the request line on upstream failures, and the Traefik access log is outside the repository (S5-23 P1). Fix in the repository: raise `error_log` to `crit` or mask it. The Traefik part is TD-28 (9). | Privacy | 1 | 3 | 2 | 16 | now (repository part); with TD-28 (proxy part) |
| TD-59 (#383) | `identity."AccountTokens"` gains one row per registration, resend and reset request. Nothing deletes expired or used rows; only an account deletion removes a user's tokens (S5-12a P4). Proposed fix: the pattern of `InvitationCleanupService`. | Privacy | 1 | 2 | 2 | 12 | now or touch |
| TD-62 | Lists of an organisation the user has left are not in the personal data export, because `ICriteriaListDataExport` reads through the organisation filter (S5-19 P4). | Privacy | 1 | 2 | 2 | 12 | legal review O-4, before G-P |
| TD-43 | Platform-role grants and revokes are logged at `Information`, which reaches only the console. No audit trail is stored (S4-25 R8, P4). | Security | 1 | 2 | 2 | 12 | decide with R-ADM-02 (step 7) |
| TD-44 | The licence notices do not cover the OS packages of the base images: `aspnet:10.0`, nginx, PostgreSQL, and now Mailpit for development only (S4-27 P4). | Dependency | 1 | 2 | 2 | 12 | licence review before G-P |
| TD-29 | 🔧 The criteria-suggestion channel is the placeholder `mailto:kriterium-vorschlagen@example.invalid` until O-6. No release pipeline runs `check-release-config.mjs` yet. | CI/Ops | 1 | 2 | 2 | 12 | with the release pipeline, before G-P |
| TD-17 | `minimatch` GHSA-g3hv-75wg-m249 (moderate) inside `eslint` / `typescript-eslint`, Dependabot alerts 12 (client) and 13 (`e2e/`). Dev tool, not shipped: 9 findings (client) and 10 (`e2e/`) with dev dependencies, 0 without. The client's findings now have a non-breaking `npm audit fix`. | Dependency | 1 | 1 | 1 | 10 | now: try `npm audit fix` in the client; `e2e/` waits for ESLint |

### 3.3 Code, architecture and design

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-72 (#387) | **The users are still legacy, and no roadmap package moves them.** CLAUDE §3 and the protocols point to "M3.1", but ROADMAP marks M3.1 done (cookie session). Still legacy:<br>- the Identity tables in `ApplicationDBContext` (ADR-014 rule 4);<br>- the endpoints in the host (`API/Account/`, `API/Auth/`, `API/Organizations/`, all through `UserManager<User>`);<br>- `GET /api/me` with `UserService.GetCurrentUserAsync`;<br>- the `roles` command;<br>- the E2E seeder `TestUserSeedExtensions`;<br>- `User.CreatedAt` (S5-27);<br>- the soft-deleted rows, which keep their role, claim and lockout columns (S5-28a R5);<br>- four user ids without a foreign key: `UserContributorLink.UserId`, `Organization.CreatedByUserId`, `OrganizationMembership.UserId`, `AccountToken.UserId` (S5-12a, S5-15 P5).<br>The comment in `IdentityAccessModule.cs:34` still speaks of the JWT. | Architecture | 2 | 3 | 5 | 5 | **needs a roadmap package** (owner): a debt row of M5, or its own package before G-P. The number understates it. |
| TD-70 | Client copies of server values, kept in step by hand:<br>- `passwordMinLength` ↔ `Account:Password:MinLength`;<br>- `lockout` ↔ `Account:Lockout:*`;<br>- `NOTE_MAX_LENGTH` and `LIST_NAME_MAX_LENGTH` ↔ SPEC §7.2.<br>S5-22 showed the fix for the weight range: the API serves the value (S5-12b R6, S5-13b, S5-22 P5). | Architecture | 1 | 2 | 2 | 12 | touch; serve them with the next settings change |
| TD-71 | Dead code:<br>- the legacy helper classes `.secondary`, `.tertiary`, `.error` and `.text-error` in `src/styles/theme.scss:97–112`, which no template uses since S5-28b (their comment still names the legacy account screens);<br>- `core/models/appCustomization.model.ts`, which nothing imports;<br>- 14 placeholder `todo.txt` files (10 under `features/feature-1/`);<br>- the commented-out `localStorage` lines in `criterion-proposal.ts:138` and `product-proposal.ts:155`. | Code | 1 | 1 | 1 | 10 | now (quick win) |
| TD-82 | The `initial` bundle budget in `angular.json` is still 850 kB warning / 1 MB error. The initial bundle is ≈ 763 kB (S5-02 P1). | Code | 1 | 1 | 1 | 10 | owner picks the values; any PR |
| #368 | `en.json`, `es.json`, `fr.json` and `EnLanguage.en/es/fr` can no longer be reached (German only, R-SET-01). `language.service.ts` holds the **last M3 entry** of `LEGACY_EXCEPTIONS` (S5-25b R3). | Code | 1 | 1 | 2 | 8 | now (closes the M3 group of #158) |
| TD-34 (#240) | The dark values are generated and not reviewed, for example `on-surface-variant` ≈ `on-surface`. | Accessibility | 1 | 2 | 2 | 12 | O-S5 (Figma dark mode), before G-R |
| TD-31 (#238) | `Criterion.SupersededByKey` holds one key, so a split cannot be stored. The carry-over of S5-21a (`CatalogReleaseCarryOver`) follows the same single key. | Architecture | 1 | 3 | 3 | 12 | owner decision before step 6 |
| #299 | The short description is in the model and the export, but not in the criteria answer, the S11 card or the S11 search. | Code | 2 | 1 | 2 | 12 | with the 2026.x folder that brings short descriptions (K-1) |
| TD-47 ↑ | The UTC day is used for two things: the criteria export's file name and „Info“ date, and since S5-27 also the account export's file name `ProcureAccess_Daten_<YYYY-MM-DD>.json`. Between midnight and 1:00 or 2:00 German time they show the previous day. Share links already use the German day through 🔧 `ShareLinks:TimeZone` (S5-23). | Code | 1 | 1 | 1 | 10 | owner decision before G-P (proposal: one 🔧 time zone for all three) |
| TD-46 | Platform maps 400 to `validation`, but 413 and other non-400 bad requests answer with the RFC default `type` (S4-19 R4). | Code | 1 | 1 | 2 | 8 | touch (Platform error handling) |
| TD-83 | Large files:<br>- `Catalog/Releases/ReleaseFolderValidator.cs`, 924 lines;<br>- `configurator/pages/criteria-select/criteria-select.ts`, 630 lines and growing (581 at v4);<br>- its spec, 1 020 lines.<br>Not wrong, but hard for a successor to read. | Code | 1 | 1 | 3 | 6 | touch: split by step or rule group when next changed |

### 3.4 Tests

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-55 (#379) | **`main` is red: J5 fails since S5-30.**<br>WebKit, run 44091357421 (merge of #373): `e2e/journeys/j5-session.spec.ts:152` "before the absolute limit …". `getByRole('alertdialog', { name: 'Ihre Sitzung endet bald' })` resolved to **two** elements: the warning was open twice when the page loaded with the warning already due.<br>Chromium, run 44097092697 (merge of #374): `:198` „Schließen“ in the overlay leaves the user signed out — a `toHaveCount` check failed.<br>S5-30 ran J5 90 / 90 with `--repeat-each 5` locally, but not on WebKit. Check first whether `SessionWarningService` can open the warning twice (a user-visible defect), then whether the tests race the clock. | Test | 4 | 3 | 2 | **28** | now (first task of sprint 06) |
| TD-49 ↑ (#311) | **Manual accessibility checks by the accessibility tester.** M2: the screen-reader check of an exported XLSX and the A11Y-07 pass of step 2 (with #102). **New:** the A11Y-07 pass of steps 3 and 4 (S01–S03, S12–S16, S20, S21, S60, S61, the session warning and the login overlay). It includes S5-12b R2 (an empty `aria-describedby` on the consent checkbox), TD-41 and the `mat-menu` „Konto“. Until then M2 – M4 are development milestones, not pilot-ready ones. | Accessibility | 3 | 3 | 3 | 18 | before G-P; owner schedules the tester |
| TD-56 (#380) | Mobile Chrome, run 43937117139 (merge of #359): J1's second part timed out after 5 minutes in `page.waitForEvent('download')` at „S13 of the saved list: the same file as the draft before saving“ (EXP-06, M3 exit check). Seen once; no named cause yet. | Test | 2 | 2 | 2 | 16 | now, with TD-55 (a failed E2E job is a finding until its cause is named) |
| TD-48 | Firefox / Playwright crash class. Known triggers: the name computation of a `mat-slide-toggle` (V129), of the S11 include checkbox (V165) and of S02's consent checkbox (S5-12b R12), and a named `<section aria-labelledby>` (V226, #337; CLAUDE §3 forbids it for now). The workarounds are role-only and CSS locators. | Test | 2 | 2 | 2 | 16 | watch: recheck with every Playwright update |
| #355 | Firefox crashes its page process right after `POST /api/v1/configurations` answers 201, with two or more workers. J1's second test is therefore `test.fixme` on Firefox (V254). Since S5-21b that test also holds the saved-export file comparison, the **M3 exit check, which Firefox never runs**. | Test | 3 | 2 | 3 | 15 | now: narrow it down on a draft PR (steps in the issue) |
| #339 | Mobile Safari: `s10-step1.spec.ts:41` ran axe during the form field's fade-in (contrast 3.89). S5-25b made the axe helper wait for transitions before every run, which may have fixed it. Not verified. | Test | 2 | 1 | 1 | 15 | now: check the CI history, then close or fix |
| #82 | Regression tests for the S1-10 zoneless fixes (Z1–Z5, R1–R5). | Test | 2 | 2 | 3 | 12 | touch (screens are rebuilt in M5–M6) |
| TD-75 | Three integration test files keep their own copy of the legacy-account helper that `tests/…/Infrastructure/LegacyAccounts.cs` offers, among them `OrganizationEndpointsTests` and `InvitationEndpointsTests` (S5-20a R6). | Test | 1 | 1 | 1 | 10 | touch, or with TD-72 |

### 3.5 CI and tooling

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| TD-74 | **Local E2E is unreliable.** In S5-21b, S5-26, S5-28b, S5-31 and S5-32 the machine had under 1–2.1 GB free. E2E either did not run or ran from old images plus `ng serve --port 4300`, so J5, J8 and S03 failed for set-up reasons. Under load, 11 of 270 tests failed on focus and visibility waits and passed alone (S5-28b §4). CI became the only authority. This was the watch row S1-13 P1 / S4-18b P4 of v4; the trigger has happened. | CI/Ops | 2 | 2 | 1 | 20 | now (owner: free disk space; `docker system prune`, move images) |
| TD-73 (#388) | CI never starts `ng serve`, so a broken dev server like #264 can return unnoticed (S5-02 P2). No check that lazy chunks get `immutable` either, so a regression of the nginx rule of #308 would go unnoticed (S5-01 P1). | CI/Ops | 2 | 2 | 2 | 16 | sprint 06 or touch: one smoke on the E2E stack plus one `ng serve` start in stage 1 |
| TD-51 | PRs that change the same lock file can merge without a rebase in between. The ruleset still does not require up-to-date branches (`strict_required_status_checks_policy: false`). #364 and #365 share the client lock file now. | CI/Ops | 1 | 2 | 1 | 15 | owner: merge one at a time, or turn the setting on |
| TD-76 | The Dependabot batch of 2026-11-16:<br>- #363: `@playwright/test` 1.65.1, red in E2E on all browsers until the Playwright image tag and digest are bumped on the same PR (§9.5);<br>- #365: `material-symbols` 0.49.0, red in stage 1 until `scripts/subset-material-symbols.sh` runs on its branch;<br>- #364: Angular 22.2.6/22.2.7 patches;<br>- #366: two .NET packages. | Dependency | 1 | 1 | 1 | 10 | now (weekly review) |
| TD-52 | Run 43098373045 (merge of #275) has been `queued` since 2026-11-11. GitHub refuses to cancel or delete it. It blocks nothing. | CI/Ops | 1 | 1 | 1 | 10 | watch; GitHub Support only if it starts to bother |
| TD-77 | Prettier reports 71 files on `main` (S5-32 R4), and CI does not run it, so the `.prettierrc` is not enforced. | CI/Ops | 1 | 1 | 2 | 8 | owner: one formatting PR plus a CI check, or drop the config |

### 3.6 Documentation and licences

| ID | Item | Category | I | R | E | Prio | When |
|---|---|---|---:|---:|---:|---:|---|
| #4 | The README is 18 lines. `TECHNICAL.md` must cover, from v4:<br>- the deployment variables;<br>- key store, backup, restore and rotation (S4-26 §6) with one rehearsed restore;<br>- `CREATE` on `public` for the ICU collation;<br>- the SDK pin and its install (S5-03 P3, P4);<br>- `T.1` vs a real release.<br>**New:**<br>- the SMTP variables and `App__BaseUrl` (S5-11);<br>- `SAMPLE_USER_PASSWORD` of at least 12 characters, or the Development rebuild stops (S5-12a P3);<br>- the disk space E2E needs (TD-74);<br>- that the session warning can be checked only on the E2E stack (TD-64). | Documentation | 2 | 3 | 2 | 20 | step 16 (`TECHNICAL.md`), G-P |
| TD-53 | The export's source line quotes BIT inklusiv (U109). The check against the BIT inklusiv permission is not recorded (S4-20a P2). | Dependency (licence) | 1 | 3 | 1 | 20 | owner, before G-P |
| TD-78 (#389) | **Documentation drift (Claude's part).**<br>- **Versions:** S5-07 left no v56 clause in TESTING's status line, no "Changes in v2.16" table in SPEC, and ARCHITECTURE C133/C134 under "Changes from v45" (S5-05 P2).<br>- **Stale text:** TESTING §3.3 still says the clock endpoint and the relaxed rate limit do not exist (S5-11 P7). SPEC §7.6 names `ARCHITECTURE_v10`. Handoff §9 names `SPEC_v1`. The Figma mapping §3 example has a wrong path (S5-05 P3). The SPRINT_05 row and task prompt of S5-28 still say „change e-mail and password“ (S5-28a P5).<br>- **Code** naming deleted documents: `e2e/configurator/s11-list.spec.ts:18` (TESTING_v43), `e2e/shell/reflow.spec.ts:27` (TESTING_v26), `MappingCompletenessTests.cs:13` (TESTING_v27), `ProblemDetailsTests.cs:30` (TECH_DEBT_v1), and four "v18"-style citations (`criteria-suggestion.ts:54`, `criterion-card-rules.ts:53`, `criterion-detail-rules.ts:42`, `FilterExclusion.cs:16`). Per CLAUDE §2 they cite by section or pattern; the comment in `IdentityAccessModule.cs:34` (TD-72) too.<br>- **Files:** `ROADMAP_v2.md` and `ROADMAP_v3.md` break the two-versions rule.<br>- **Size:** `CLAUDE.md` is at 38 183 of 40 000 characters: state detail goes to `CODEBASE`, not §3. | Documentation | 2 | 2 | 2 | 16 | now (one documentation session, like S5-05) |
| TD-79 (#259) | **Owner documents.**<br>- **UX copy:**<br>  - drafts v31 U133–U137 (invitations) and v33 U140–U144 (S12, S14, „Kriterienlisten“) are still „Entwurf“ (S5-17b P1, S5-20b P2);<br>  - U24 still names „S11 (Details)“ for the legend (S5-31 P2);<br>  - U130 says `org.left` is announced on S20, while the code announces it on S21 (S5-16b R5);<br>  - a key for the general notice, e.g. `errors.generic`, so that the legacy `ERROR.GENERIC` can go (S5-10 P1);<br>  - `criteria.share.expiry.hint` writes the 🔧 values „90 Tagen“ and „ein Jahr“ into its text (S5-24 P2);<br>  - S16's excluded card says „Nicht in Ihrer Liste“ to a reader who does not own the list (S5-24 P3);<br>  - the organisation-switch announcement (TD-66);<br>  - from v4: `criteria.priority.change` unused, the `changeLabel` hint, §5.4 „Ändern“.<br>- **PRD and Kriterienkatalog:** headers; Kriterienkatalog §3.1, §7 and K-3 still name `SampleData.cs` (S5-05 P1). | Documentation | 2 | 2 | 2 | 16 | owner, next version of each document |
| TD-80 | **Figma.**<br>- No frame for S02, S03, S12, S14, S15, S16, S20, S21, S60, S61, the session warning and the organisation switch; these are built from Handoff §5.3, and the mapping has no rows for S12, S14 and the saved list.<br>- Kept differences to update in Figma: the sticky bar's 32 px edge inset, `label/large` and 12 px padding (S5-32 P2); the number vs `check` in a finished step (S5-32 P3); the S11 layer `2034:1196` „Unsaved status“ can now be shown (S5-22 P1).<br>- From v4: S13 D1–D5, filled vs outlined icons, S04, S80 and the legal pages. | Documentation (design) | 1 | 2 | 3 | 9 | design owner, before G-P |

## 4. Done in sprint 05

| Item (v4 ID) | What was done | Task |
|---|---|---|
| TD-38 (#309) | S10 step-line checks scoped to `#main`, also in `s10-step1.spec.ts` | S5-01 |
| #308 | The nginx `immutable` pattern matches every hashed chunk (47 of 47 files) | S5-01 |
| #264 | `npm start` renders again. `provideMarkdown()` sits on the legacy list only, and the initial bundle dropped 763 → 701 kB | S5-02 |
| #306 | .NET SDK 10.0.404 pinned in `global.json`, the image and CI, with a stage-1 check (O-S5-1) | S5-03 |
| TD-42 (#288, #289, #291, #292) | MailKit 5, `MimeKit.Core`, Swashbuckle 11, `Microsoft.OpenApi` 3.11 with the `EMBEDDED` review; BouncyCastle left the image (O-S5-2) | S5-04 |
| TD-50 (#313) | Sprint-04 findings in SPEC, ARCHITECTURE, TESTING, Handoff and Figma mapping; versionless references in document bodies (D1 (a)). The owner part stays in #259 | S5-05 |
| TD-54 (#312) | Reflow with a two-procedure card; tab padding below 600 px | S5-06 |
| #87, #88, #89 | Session probe, real-cookie test host, antiforgery helper test | S5-07 |
| v4 §5 JWT, antiforgery on `criteria-list-shown`, OPS-04, `signedIn` from the session | Identity cookie session; JWT, `jwt-decode`, CORS and `JWTSettings` removed | S5-07, S5-08 |
| #170 L1, v4 §5 login outlet | The login outlet sits inside the shell and reflows | S5-09 |
| v4 §5 `Identity.UI`, `{ code, message }`, `ERROR_TRANSLATIONS`, `RefreshTokens`, dead key `UICustomizationNotFound` | Legacy auth cleanup (O-S5-4) | S5-10 |
| #302, TD-12 | `IEmailSender` (MailKit, Mailpit), real recipients; `compose.log` checked, no secret | S5-11 |
| #301 | No global `Member` role (O-S5-3 (a)) | S5-12a |
| v4 §5 API-05 registration and reset parts | Rate limits for registration, resend, reset per IP and per address | S5-12a, S5-13a |
| #319, #317, #320 (created and closed in the sprint) | Lockout hint on S01; dev server proxy; typed session contract | S5-08, S5-13b |
| v4 §5 E2E users, Mailpit, clock | Seeded users and organisation; Mailpit; the test clock per browser context | S5-11, S5-14, S5-16b, S5-30 |
| v4 §6 O-22 | Contracts may reference contracts along §4.3 (ADR-017, O-S5-5) | S5-19 |
| v4 §5 TESTING §5.3 "save in one call", S11 „Liste speichern“, `criteria.list.unsaved`, draft-note wording | Saved lists with the shared draft check | S5-20a, S5-20b, S5-22 |
| TD-45, TD-40, v4 §5 TEN-04 workspace part | Weight range served by the API; a too-long note leads to the note; `no-store` default on `/api/v1` | S5-22 |
| TD-25 (#234), v4 §5 A11Y-08 explicit setting, `mat-menu` / motion setting | `withSettings` without storage, snackbar or crash; S60; explicit reduced motion | S5-25b, S5-26 |
| TD-26 (#234), v4 §5 native `disabled` on „Löschen“ | S61 with the rebuilt deletion dialog (`aria-disabled` with a reason); `UsersController`, `UserRepo` and the legacy profile removed | S5-28b |
| v4 §5 security stamp not checked, v4 §9.3 M4 "first time-dependent tests" | Stamp checked on every request; SEC-07, SEC-08 on the fake clock | S5-29 |
| #337 (created and closed in the sprint) | Firefox crash on named sections; no `<section aria-labelledby>` for now | #348 |
| #279 | Rating legend once above the list on S11, the saved list and S16 | S5-31 |
| #278, #281, #282 | Title row, full-width sticky bar, 32 px stepper circles, summary terms. **PR #376 is open**; the issues close when it merges | S5-32 |
| Invariants made `active` | SEC-01, -02, -03, -07, -08, -09, -10, -11; OPS-04; TEN-01, -02, -05 (skeleton), -08, -09, -11, -12; CAT-08, CAT-22; EXP-06 and the persisted parts of EXP-01/EXP-03; PRV-07 (account part), PRV-11; A11Y-04, -06, -08 (complete) | S5-07 – S5-30 |
| `LEGACY_EXCEPTIONS` 32 → 16 | M3 16 → 1, M5 11 → 10 (`app.store.ts`) | S5-08 (−6), S5-12b (−2), S5-13b (−1), S5-25b (−4), S5-28b (−3) |

## 5. Planned debt — retires with a migration step (no extra work)

This code is legacy by design. Do not fix it on its own; replace it with the step. Rows new or changed in v5 are marked *(new)* or *(changed)*.

| Debt | Retires with |
|---|---|
| *(changed)* **Legacy filter.** These go with the activation of 2026.1 (after O-1):<br>- the switch `configuratorReleaseGuard` → `/filters` on 404;<br>- `LEGACY_FILTER_PATH` and the main-nav path constants;<br>- nav "current" on `/filters`;<br>- #52;<br>- CAT-10 with `ExistingCatalogSeed`, `LegacyCatalogReadTests` and `ProductsByCriteriaFilterTests`;<br>- `marked`, `ngx-markdown`, their provider in `features/criteria/list/` and the `externalDependencies` entry of the `development` configuration (S5-02 P4);<br>- the 5 M2 entries of `LEGACY_EXCEPTIONS`;<br>- the stepper icons `create`, `done`, `warning`;<br>- the 404 of `/api/v1/catalog/*` on staging | legacy removal |
| *(new)* **The users** (TD-72): Identity tables, host endpoints, `GET /api/me`, `roles` command, E2E seeder, foreign keys, `Organization.Create`'s optional id for the seeder (S5-16b) | a roadmap package (§6) |
| *(changed)* `DateTime.UtcNow` ×1: `ProposalService.cs:217`. The test hits are polling deadlines, the Serilog stamp and test certificates, which is fine | step 5 |
| *(new)* Saved lists without ETag / If-Match. A `PATCH …/criteria/{id}` racing a `PUT …/selection` that removes the criterion answers 500 (S5-21a R1, S5-21b P2). `ProcurementProjectId` on `CriteriaConfiguration` (S5-19 P5). TEN-08 "a project copy → 409" and its share action (S5-23 P4, S5-24 P4) | step 5 (API-03 with S37) |
| *(new)* Account deletion parts of later steps: the procurement projects of an organisation that ends (step 5); contributor profiles detached, entries withdrawn, experience reports purged (steps 6–7, PRV-06, PRV-10) (S5-28a P3). Export sections `createdByMe.projects` (step 5) and `contributions` (step 6) (S5-27 P3) | steps 5–7 |
| Deferred invariant parts:<br>- CAT-21;<br>- TEN-04 directory part (step 6, probably covered by the `no-store` default);<br>- TEN-06 for `CatalogEditor` and `PlatformAdmin`, with *(new)* their users in the Tenants fixture (S5-15 P4);<br>- API-05 for publication and contribution;<br>- API-06 for the directory and workspace lists;<br>- PRV-04 identity part, *(new)* whose endpoint scan must skip `GET /api/v1/account/export` (S5-27 P1);<br>- the A11Y-02 keyboard variant of J3;<br>- CAT-17 workspace and directory parts;<br>- the EXP-05 file-store part;<br>- *(new)* R-SHL-03's switch test for projects and the compare select (S5-18 P5) | their steps |
| `[Produces("application/json")]` and `[ApiVersion("1.0")]` on legacy controllers; the host's `UseRequestLocalization` reads `?culture=` and the culture cookie for them. `UsersController` is gone | each replacing step |
| The snackbar is still the error channel for non-5xx errors (UX copy §4.1) | its last legacy caller, steps 5–6 |
| Legacy routes without a page-title key and without an H1: `/filters`, `/products`, `/product/:id`, `/proposals`, `/favorites`, the proposal forms. `/settings` and `/profile` are gone | rebuild of each screen |
| **Two** legacy `LiveAnnouncer` calls: `products-list.ts`, `proposal-list.ts` | steps 5 and 6 |
| The API sends English validation messages without codes. *(new)* The invitation endpoint answers a member's address with 400 and an English text; the client prevents it (S5-17b) | each step's module validation |
| Uppercase legacy roots in `de.json` (`AUTH`, `SETTINGS` and most of `ERROR` are gone; `ERROR.GENERIC` waits for TD-79) | rebuild of each screen |
| `LEGACY_EXCEPTIONS` in `eslint.config.js`: **16 files** (M2 5, M3 1 → #368, M5 10; #158) | each step deletes its entries; the M2 group with the legacy removal |
| *(new)* NgRx Signals warns "SignalStore members cannot be overridden" on every page in dev mode, because each feature adds `withLoading()` to the root store (S5-02 P3) | root-store rework (M5) |
| Callers of the fixed legacy store methods do not catch the rejection; legacy `<strong>` in `mat-error` | M5, M6 rebuilds |
| Legacy product reads ignore `Product.IsDeleted`; `GetByCriteriaFilterIds` lacks the proposal filter | step 6 |
| `ProposalService` `dto!.ToEntity()` and `!` on `user`. `PlatformAdmin` keeps `moderation:review`; `Proposal.ApproverId` keeps its name. `FavoritesController.cs` is commented out. The EasyMDE `@-moz-keyframes` warning. *(new)* The proposals as personal data: TD-61 | step 5 |
| The tagged API-02 test sits on the legacy proposal form | move it to a rebuilt form before step 5 |
| No backend test of the legacy seeded exclusions, only J1 | steps 5/6 |
| 24 TODOs: 23 in legacy API, DAL, MODELS and client features, and one deliberate fixture value in `e2e/support/axe-helper.spec.ts:163` | features retired or rebuilt in M5–M6 |
| E2E: no `E2E` environment; the stack runs as `Development` with `RebuildDatabase`. *(new)* Once it gets one, a soft-deleted seeded user breaks the next start with `DuplicateUserName` (S5-14 P5, R2). No products in the seeder | steps 5, 6 |
| *(new)* Unused legacy `Member` role rows in existing databases, including staging (S5-12a) | pre-pilot reset (O-S5-4) |
| `withProposal()`, `withProducts()`/`withFavorites()` | per `ARCHITECTURE §8.1` |
| Legacy projects fail on a warning only in CI, not locally | each migration step |
| K5 „Warum diese Priorität?“ toggle outside a detail panel: no UX key yet for the hide state | first screen with K5 outside S11 (workspace) |
| Display names are not normalised (trim, NFC) before the uniqueness check. S40 reuses `common.actions.clearInput` | step 6 (S50, S40) |

**Not tracked on purpose** (unchanged): CI has no NuGet or Docker cache; Serilog stamps with the system clock; files without logic count for coverage. *(new)* Old JWTs stay in the `localStorage` of browsers used before the switch; they are never read or accepted (S5-08 decision 5 (a)). The host ships the guarded `TestClock` (Development/E2E with a switch; `TestClockGuardTests`).

The won't-fix findings of the sprint-05 reviews that need no follow-up are listed only in their protocols. Examples: S5-12a R2/R3, S5-13a R3–R5, S5-15 R4/R6, S5-16a R3/R4/R7, S5-17a R3/R4, S5-19 R3/R6, S5-20a R2/R4/R7, S5-21a R2/R3/R5, S5-21b R3/R4, S5-23 R2/R3/R6, S5-24 R4, S5-25a R3–R5, S5-25b R2/R6, S5-26 R3/R4, S5-27 R2–R4, S5-28a R2–R4/R6, S5-29 R4/R5, S5-30 R6/R8, S5-31 R1.

## 6. Decisions waiting for the owner

CLAUDE §6.6: Claude Code must not decide these. Decided in sprint 05 (see `SPRINT_05.md` and the protocols):
- O-S5-1 – O-S5-5;
- the task decisions of S5-07 – S5-32;
- from v4 §6: #301, #302, #306, TD-42, O-22, PRV-04 (the export is its exception).

| Decision | Source | Needed by | Claude's proposal |
|---|---|---|---|
| **TD-28** staging deployment: the ten points of §3.2 | #236, S5-07, S5-11 P1, S5-23 P1, S5-30 | before M3 is shown on staging | the checklist is on #236 (comments of 2026-11-13 and 2026-11-17) |
| **TD-72** schedule the move of the users into IdentityAccess | ADR-014 rule 4, S5-12a, S5-15 P5, S5-28a R5 | before G-P | a debt row of M5 (≈ 5 points), at the start of M5 so the workspace endpoints are not built on legacy users |
| **#318, TD-60** 🔧 rate limits for sign-in and organisation creation | S5-07 P4, S5-16a P3 | before G-P | sign-in per IP and per address, like the reset; organisations e.g. 5 per day per user |
| **TD-58** 🔧 SMTP timeout | S5-11 P3 | sprint 06 | 15 s per operation |
| **TD-61** legacy proposals as personal data: cover them in the export and the deletion, or retire them before the pilot | S5-27 P2, S5-28a P4 | before G-P | retire them with M5 before G-P; otherwise cover them |
| **TD-62** lists of a left organisation in the data export | S5-19 P4 | legal review O-4 | — |
| **TD-81** may a sole member leave or delete an organisation; may users change their address | S5-16a P4, S5-28a P5 | when users need it | — |
| **TD-69** H2 high contrast: design and review; until then, should `.contrast-more` keep the legacy colours? | S5-25b R4, S5-26 P2 | before G-P | — |
| **TD-70** serve the SPEC §7.2 field limits and the password and lockout rules from the API | S5-22 P5 | any time | yes, with the next settings change |
| **#377** a saved list takes S11's title row | S5-32 P1 | any time | — |
| **TD-79** UX copy: confirm v31 and v33; U24, U130; `errors.generic`; placeholders in `criteria.share.expiry.hint`; S16 reader wording; the organisation-switch announcement | S5-10 P1, S5-16b R5, S5-17b P1, S5-18 P3, S5-20b P2, S5-24 P2/P3, S5-31 P2 | next UX copy version | — |
| **TD-80** Figma frames for the M3/M4 screens and the three updates | S5-22 P1, S5-32 P2/P3 | design, before G-P | — |
| **TD-74** free disk space on the development machine | S5-21b – S5-32 | now | — |
| **TD-77** Prettier: enforce in CI after one formatting PR, or drop the config | S5-32 R4 | any time | enforce (one PR, then a stage-1 check) |
| **TD-82** initial budget values | S5-02 P1 | any PR | ≈ 800 kB warning / ≈ 950 kB error |
| 🔧 `RateLimits:Invitation` (5 per hour per Owner): keep | S5-17a P3 | pilot feedback | keep |
| UX-O11 registration limit for offices behind one NAT address | S5-12a R7 | pilot | keep until the pilot |
| **TD-39** reload flow after `release-mismatch` | O-S4-72 | before a second release is activated | as in #310 |
| **TD-47** date of the export files: UTC day or German day | S4-19 P5, S5-27 | before G-P | German day through one 🔧 time zone (as `ShareLinks:TimeZone`) |
| **TD-51** lock-file merges | S4-06 P1 | next Dependabot batch | one at a time |
| **TD-43** stored audit trail of role changes | S4-25 P4 | with R-ADM-02 | — |
| **TD-53** BIT inklusiv permission for the export's source line | S4-20a P2 | before G-P | — |
| K5 menu on „Geprüft nach …“ cards: the Handoff says "priority B, deselectable", the code allows a level change | S4-14 P2 | any time | — |
| **#299** does the S11 search include the short description? | S4-24 P2 | with #299 | — |
| **TD-31** splits (#238) | S3-14 P2 | before step 6 | — |
| **TD-34** dark values (O-S5, #240) | S4-29 P3 | before G-R | wait for the Figma dark mode |
| A release with gaps stays stored; `filter-options?release=` serves it while `/criteria` answers 404 | S3-20b P2 | when 2026.1 is imported anywhere | keep |
| Close **M3** after #368, #375 and #380; move #318 → G-P, #170 → M6, #82 and #355 → M5. (M1 and M4 were closed on 2026-11-17.) | §7 | sprint review | yes |
| Project team: priorities, rationales and plain-language titles of 2026.1 (O-1, K-1, K-6); short descriptions; the „#todo#“ in 11.1.3.2; the criteria-suggestion channel (O-6) | S3-27, S4-11, S4-15b, S4-24 | M2.3; O-6 before G-P | — |

## 7. Issue hygiene

- **Milestones set on 2026-11-17** (owner's go-ahead): #355 → M3, because it hides the M3 exit check on Firefox; #339 → M5; #350 → M6, because it is the legacy product list (the one-line fix can still come earlier, §9.1). No open issue is left without a milestone.
- **Milestones closed on 2026-11-17:** **M1** (no open issue) and **M4** (no issue at all; its packages were done as stretch in S5-29 and S5-30).
- **Open milestones:**
  - **M3** has #82, #170, #318, #355, #368, #375, #377 and #380 open. Close it after #368, #375 and #380; move #318 → G-P, #170 → M6, #82 and #355 → M5 (§6).
  - **M2** has #52, #236, #259, #278, #281, #282, #299 and #378. #278, #281 and #282 close with PR #376.
- **Follow-ups without an issue:** the protocols named some findings "follow-up issue", but no issue was created for S5-30 R4, R5, R7, S5-20a R6, S5-02 P2, S5-09 P4, S5-11 P3, S5-12a P4 and S5-23 P1. This register tracks them as TD-64, TD-41, TD-63, TD-75, TD-73, TD-67, TD-58, TD-59 and TD-57; §8 lists the ones that now have an issue.
- **#236** got a comment with the sprint-05 preconditions, points (5) – (10) of TD-28 (2026-11-17).
- **#240** keeps only its dark part. A comment that the light part is done is still missing (S4-29 P4, carried from v4).
- **CI:** two failed runs on `main` without a named cause, now #379 (TD-55) and #380 (TD-56). Run 43098373045 is still `queued` (TD-52).
- **Open Dependabot PRs** for the weekly review: #363, #364, #365, #366 (TD-76).
- **Alerts:** CodeQL has no open alert. Dependabot has only 12 and 13 (TD-17).
- **Labels:** #355 has no label; proposal: `bug`, `tech-debt`.

## 8. New issues

Created on 2026-11-17 for the **TD-** items that need one. Each holds the problem, the fix and "done when". TD-55 went to M5 instead of M4, because M4 was closed (§7).

| Item | Issue | Labels | Milestone |
|---|---|---|---|
| TD-55 | #379 E2E on `main`: J5 shows two session warnings (WebKit) and fails after „Schließen“ in the overlay (Chromium) | bug, tech-debt | M5 |
| TD-56 | #380 E2E flake: J1 second part times out waiting for the saved export download (mobile Chrome) | tech-debt | M3 |
| TD-57 | #381 Share tokens: the client's nginx `error_log` writes the request line with the token | tech-debt | G-P |
| TD-58 | #382 SMTP: a stalled server holds requests for minutes (MailKit default timeout) | tech-debt | G-P |
| TD-59 | #383 Delete expired and used account tokens | tech-debt | M5 |
| TD-61 | #384 Legacy proposals: personal data missing from the export and the account deletion | tech-debt, question | G-P |
| TD-64 | #385 Dev server: the health watcher renews the session every 5 s | tech-debt | M5 |
| TD-65 | #386 Signed-out deep links to guarded pages lose their target after the sign-in | bug | M5 |
| TD-72 | #387 Move the users into IdentityAccess (ADR-014 rule 4): tables, host endpoints, `GET /api/me`, foreign keys | tech-debt | M5 |
| TD-73 | #388 CI: smoke for `ng serve` and for the `immutable` header of lazy chunks | tech-debt | M5 |
| TD-78 | #389 Carry the sprint-05 findings into TESTING, SPEC, ARCHITECTURE, Handoff and the code comments | documentation, tech-debt | M5 |
| TD-28 | Comment on #236 (2026-11-17) with points (5) – (10) of §3.2 | — | M2 |

The other new TD items need no issue: TD-60 is in #318; TD-62, TD-69, TD-70, TD-77, TD-79, TD-80, TD-81 and TD-82 are owner decisions (§6); TD-63, TD-66, TD-67, TD-68, TD-71, TD-74 – TD-76 and TD-83 are touch, quick-win or owner items.

## 9. Fix plan — when to fix what

### 9.1 Sprint 06 — M5 start (workspace) and its debt

New debt budget **≈ 5 points**.

| # | Work | Items | Pts | Note |
|---|---|---|---:|---|
| 1 | **Make `main` green:** find why the warning opens twice and the overlay check fails; look at the J1 download timeout | TD-55, TD-56 | 1 | first; until then every PR's E2E is in doubt |
| 2 | Quick wins in one PR: S16 list gate; sticky-bar edge in forced colours; product-list `aria-label`; `credentials: 'omit'` in the health watcher; dead CSS and files; `npm audit fix` in the client | #375, #378, #350, TD-64, TD-71, TD-17 | 0.5 | |
| 3 | Return to the target page after the sign-in from a guarded deep link | TD-65 | 0.5 | |
| 4 | Remove the legacy UI languages; the M3 group of `LEGACY_EXCEPTIONS` is then empty | #368 | 0.5 | |
| 5 | Mail and token hygiene: 🔧 SMTP timeout; token cleanup job; nginx `error_log` without tokens | TD-58, TD-59, TD-57 | 1 | TD-58 after the owner's value |
| 6 | Documentation session | TD-78 | 1 | owner documents by the owner (TD-79) |
| 7 | Narrow down the Firefox crash after saving (draft PR); check #339 | #355, #339 | 0.5 | |
| — | Owner: staging (TD-28); disk space (TD-74); §6 decisions, above all TD-72 and the rate limits; the tester's pass (TD-49); the Dependabot batch (TD-76) | | 0 | |
| — | With M5 (to be counted in the roadmap): the users' move if scheduled there (TD-72); ETag on saved lists with API-03; the proposal retirement with TD-61; `withLoading()` and the root store; the M5 entries of `LEGACY_EXCEPTIONS` | §5, TD-72, TD-61 | 0 | |

### 9.2 Rest of M5 — workspace (projects, candidates, assessment, concurrency)

| Fix | Items | Why then |
|---|---|---|
| Users in IdentityAccess before the workspace endpoints, if the owner schedules it in M5 | TD-72, TD-75 | the workspace should not grow on legacy users |
| ETag / If-Match for saved lists together with S37 | §5 (S5-21a R1) | same mechanism (API-03) |
| Proposal retirement: `ProposalService`, `withProposal()`, the proposals as personal data, `DateTime.UtcNow`, the API-02 test moved first, the `proposal-list` `LiveAnnouncer` | §5, TD-61, #158 M5 group | the code goes |
| Account deletion and export parts of step 5: projects of an ending organisation, `createdByMe.projects`, TEN-08 project copy | §5 | step 5 |
| Rate limits from SPEC §7.3, if decided | #318, TD-60 | before G-P |
| Regression tests for the zoneless fixes, screen by screen | #82 | the screens are rebuilt |

### 9.3 M6 – M7

| Milestone | Fix | Items |
|---|---|---|
| M6 | Splits decided before CAT-03/04; directory rebuild (`IsDeleted`, proposal filter, product list and detail reflow, `products-list` `LiveAnnouncer`); display-name normalisation; API-06 and TEN-04 directory parts; contributions in the export; account deletion detaches contributor profiles | TD-31, #170, §5 |
| M7 | File-store check in `/health/ready`; EXP-05 file-store part; role-change audit trail with R-ADM-02; experience reports purged on deletion | TD-43, §5 |
| Legacy removal (2026.1 activated, needs O-1) | Everything in the first row of §5; #52 closes; the M2 `LEGACY_EXCEPTIONS` entries go; the S13 reload flow must be done before | §5, #52, TD-39 |

### 9.4 Before the pilot (G-P) and the public release (G-R)

| Fix | Items |
|---|---|
| Staging on `main` with every precondition | TD-28 |
| README and `TECHNICAL.md` (process step 16) | #4 |
| Manual accessibility checks of M2 – M4 by the tester | TD-49, #102, TD-41 |
| Privacy: legacy proposals, left organisations' lists in the export, another user in another tab | TD-61, TD-62, TD-63 |
| Rate limits for sign-in and organisation creation | #318, TD-60 |
| High contrast H2, or a decision on the legacy `.contrast-more` colours | TD-69 |
| Reload after a deployment (lazy chunks); S13 reload flow | #296, TD-39 |
| Licence review: OS packages of the base images, the BIT inklusiv permission | TD-44, TD-53 |
| Release pipeline that runs `check-release-config.mjs`; the O-6 channel and the legal pages in the check | TD-29 |
| Export dates in the German day, if the owner decides so | TD-47 |
| Figma frames for the M3/M4 screens | TD-80 |
| The axe suppression list stays empty (TESTING §12.4) | any new entry |
| G-R: dark values reviewed and pinned with the Figma dark mode | TD-34 |

### 9.5 Watch items — act only when the trigger happens

Rows new in v5 are marked *(new)*. The row "Docker builds fail on the developer machine" of v4 became TD-74. The row "the next version of SPEC … is written: make references versionless" became a standing rule (CLAUDE §2, S5-05 D1).

| Trigger | Then | Source |
|---|---|---|
| A Playwright update | Recheck the Firefox crash workarounds (V129, V165, V226, S5-12b R12, #355); retry `aria-labelledby` on the invite form's section and role locators | TD-48, S5-17b |
| A Dependabot PR updates `@playwright/test` | Bump the tag and digest in `e2e/playwright-image/Dockerfile` on the same PR (now #363) | S4-01b F5 |
| A Dependabot PR updates `material-symbols` | Run `scripts/subset-material-symbols.sh` on its branch (now #365) | S3-25 P3 |
| *(new)* A Dependabot docker PR bumps `sdk:10.0.x` | Push the matching `global.json` change onto it; run `check-api-notices.mjs --write` if stage 2 fails | S5-03 P1 |
| *(new)* An update of `Microsoft.Extensions.Identity.Core` | Recheck the two overrides of `ClockedUserManager` (`IsLockedOutAsync`, `AccessFailedAsync`), which copy Identity 10.0.15 | S5-13a |
| *(new)* A .NET 11 package or TypeScript 7 / `@types/node` 26 shows up | Stay on .NET 10 LTS and the current TypeScript major; ignore or close until a decision | scan 2026-11-17 |
| More than one API instance | Migrations, log retention, the usage-event queue, the first Data Protection key and the in-memory rate limiter assume one instance. *(new)* So do the `PasswordResetMailQueue` (reset mails lost on stop) and the `KeyedRateLimiter` | S3-23 D3, S3-24 R4, S4-21a, S5-13a |
| *(new)* A new reverse proxy, log sink or log pipeline | It must send `X-Forwarded-Proto` from a trusted address, and mask `/geteilt/…` and `/api/v…/shared/…` | S5-07, S5-23 |
| *(new)* A new environment name | Check `SessionCookiePolicy` (Secure is relaxed in Development/E2E) and `TestClockGuardTests` | S5-07 3a, S5-30 |
| *(new)* Load shows the per-request work | Raise 🔧 `Session:SecurityStampValidationInterval` (SEC-08 then holds after the interval); skip the membership query by endpoint metadata; revisit `no-store` on signed-in catalog reads (S5-29 3a) | S5-29 P2/P3, S5-15 R2 |
| *(new)* An organisation's lists grow large or S14 gets slow | Paging for `GET /configurations` and S14 | S5-20a R3 |
| *(new)* Users report the session warning at the wrong time | Send the server's time with the expiry headers and keep the offset on the client | S5-30 R6 |
| *(new)* A stored release is changed in place, or CAT-05 is relaxed | `PersistedConfigurationSource` diverges from the stored `ResolvedLevel`, and an unresolvable selection answers 500 | S5-19 R4/R5 |
| *(new)* More callers of `ICriteriaListDeletion` or `IShareLinks.DeleteOfOrganizationsAsync` | They need a membership check of their own | S5-28a R4 |
| *(new)* The server's sole-owner refusal regresses | The read-only E2E tests would delete the shared seeded Owner | S5-28b R7 |
| *(new)* `ShareLinks:DefaultExpiryDays` or `MaxExpiryMonths` change | Update `criteria.share.expiry.hint` (or give it placeholders, TD-79) | S5-24 P2 |
| *(new)* A SPEC §7.2 field limit changes | Change the client constants, or serve them (TD-70) | S5-22 P5 |
| *(new)* A Material datepicker is needed | Provide a German `MatDatepickerIntl` first | S5-24 B (a) |
| *(new)* Share links are abused | Add a rate limit (S5-23 decision 7 has none) | S5-23 |
| *(new)* Owners complain about onboarding time | Revisit 🔧 `RateLimits:Invitation` | S5-17a P3 |
| *(new)* A release has a criterion with three or more procedures, or the font changes | Recheck the tab labels at 320 px; the reflow test flags pagination | S5-06 |
| *(new)* A release has a requirement without a procedure text for an app type that selects it | The legend shows six levels and „Wie wird bewertet?“ none; fix the data or the rule | S5-31 R3 |
| *(new)* A Content-Security-Policy is introduced | The `pixelsAt` E2E helper loads a `data:` URL; allow it for tests or decode in Node | S5-32 R3 |
| *(new)* The Angular/esbuild chunk-hash alphabet or length changes | Recheck the nginx `immutable` pattern (or TD-73 catches it) | S5-01 |
| *(new)* A consumer needs OpenAPI 3.1/3.2 | Set `OpenApiVersion` in Swashbuckle 11 | S5-04 P4 |
| *(new)* A client is served from another origin than the API | No CORS any more, and `SameSite=Strict` plus the relative XSRF header break: architecture decision first | S5-08 |
| *(new)* Two open PRs write the same document version | The second to merge renumbers its version and the headers that name it | S5-05 P5 |
| *(new)* `start-and-error-pages.spec.ts` loses the H1 focus in Firefox again, or J8 / `invitations.spec.ts` time out in CI | Open an issue | S5-12b R10, S5-24 |
| *(new)* The E2E stack gets a real secret (e.g. sends through Lettermint) | Check the `compose.log` artefact again | S5-11 (TD-12) |
| *(new)* A pilot office behind one NAT address hits the registration limit | Revisit the 🔧 value (UX-O11) | S5-12a R7 |
| MCR is slow or down | Add a retry to the image pull | S4-01b P4 |
| A Dependabot NuGet group opens in the middle of a .NET servicing release more than once | Add a `cooldown` of 1–2 days for `nuget` | S4-06 P2 |
| The project moves to Node 26 | `.nvmrc` files and the client Dockerfile together | S4-06 P3 |
| A new version of a package in `EMBEDDED` of `check-api-notices.mjs` | Review what it embeds, update `scripts/api-notices/` and the pinned version | S4-27 |
| A ClosedXML release needs SixLabors.Fonts ≥ 2 | Do not take it (Split License fails D12) | S4-20b F6 |
| Anyone calls `AdjustToContents` or measures text in the export | Refuse: the API image has no fonts | S4-20b F4 |
| A new or renamed fixture quotes BIT inklusiv texts | Add its exact path to the BIT block of `REUSE.toml` | S4-04 R4 |
| 2026.x texts are exported | Check that the CSV formula guard does not prefix catalog text starting with `-` | S4-20a R2 |
| English texts are planned (SPEC §3) | ADR for English catalog texts, a second `ExportTexts` resource, an `Accept-Language` test (the legacy en/es/fr files go with #368 before then) | S4-23 P2, P3 |
| The project team's first release PR needs the database checks of the import | `catalog validate` or a CI step with a throwaway database | S4-05 P1 |
| 2026.1 gets its priorities | Document starting a kept dev database without `T.1` | S4-08 P2 |
| A later screen gets a sticky bar | Reuse the S11 `focusin` fallback and `pa-criteria-action-bar` | S4-18a P5, S5-32 |
| Usability tests object to collapsing example groups after a tab switch | Keep the tab content attached | S4-15b P4 |
| A deployment ships a client before its API | Deploy client and API together | S4-15b P6 |
| A test asserts a closed Material overlay | Wait with `vi.waitFor` | S4-20b F7 |
| Material changes when it clears the active option | Recheck `chooseFirstOption` | S4-14b |
| S35 or S55 reuses `pa-stepper` | Give it a test of its own | S4-10 P8 |
| The next change to `Release2026_1Tests` | Read the folder from the repository, not the build output | S4-05 P2 |
| An unhandled startup exception is hard to see | The API container exits with 139, not 134; open an issue | S4-26 R5 |
| ESLint / typescript-eslint accepts minimatch 11 | Take it; close alerts 12 and 13 | TD-17 |
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
| First deployment of a new module schema | The database role needs `CREATE` on the database, and on `public` for the ICU collation | S2-07 P5, S4-25 P3 |
| Dependabot rebases a PR after the CI review | Check the head commit at merge time | S2-04 P5 |
| CI run times hurt | NuGet and Docker layer caches | S1-20 P4 |

### 9.6 Recurring

- **End of every sprint:** process step 14 → `TECH_DEBT_v<n+1>.md`, which deletes the third-newest version.
  - Move done items to "Done", add the new findings of the sprint protocols and reviews, and re-score.
  - Check the milestones of new issues and the CI runs on `main`: a failed E2E job is a finding until its cause is named.
  - Check that every issue closed for a CodeQL or Dependabot alert really closed the alert.
  - *(new)* Check that every protocol finding marked "follow-up issue" has an issue (§7).
  - Fill in the sprint's close-out table.
- **Start of every migration step:** open its GitHub milestone, and plan its debt together with the step's invariants. Delete its `LEGACY_EXCEPTIONS` entries as files are replaced (#158).
- **Every screen task:** new icons go into the icon list; the screen joins `reflow.spec.ts` with each state that changes the layout, named in the task prompt.
- **End of every migration step:** review the axe suppression list (TESTING §12.4), and plan the manual screen-reader pass of the step with the tester (A11Y-07). *(new)* Steps 3 and 4 have no pass planned yet (TD-49).
- **Weekly:**
  - Dependabot pull requests: check the licence of new versions before merging (CLAUDE §10), regenerate `THIRD-PARTY-NOTICES.txt` for NuGet updates, merge PRs that share a lock file one at a time, bump the Playwright image with `@playwright/test`, and run the icon subset for `material-symbols`.
  - nginx: merge even minors (stable branch), close odd ones.
  - CodeQL alerts.
  - The CI runs on `main`.

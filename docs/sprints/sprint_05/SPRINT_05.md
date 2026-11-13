# Sprint 05 — Accounts, organisations and saved lists (M3)

> **Status:** plan (2026-11-13) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v5.md` (M3), `TECH_DEBT_v4.md` (§3, §5, §6, §9.1, §9.2), the highest versions of TESTING (§4.5, §4.7, §4.10, §4.11, §6, §9 step 3), ARCHITECTURE (§4.3, §5.2, §5.3, §7.2, §8, §9.1, §12 step 3, ADR-014 – ADR-016), SPEC (§7.1 – §7.3, §7.6, §8.4, R-AUTH-*, R-ORG-*, R-SAV-*, R-SET-01, R-SHL-02/03, R-LST-14), the Design handoff (§5.3 screens without a Figma design), UX copy (§5.2, §5.6, §5.7, §5.14, §5.17), `CLAUDE.md`, the sprint-04 close-out and protocols in `docs/sprints/sprint_04/`, and the GitHub issues and pull requests as of 2026-11-13.
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.
> **Task prompts:** `TASK_PROMPTS.md` in this folder (one ready-to-paste prompt per task).
> **IDs:** owner decisions of this sprint are `O-S5-n`. They are not the SPEC open question **O-S5** (Figma dark mode), which keeps its name.

---

## Sprint 04 in one paragraph (carried in)

Every task (50 P0 + 6.5 stretch points, several split into a/b) was done in about **3 development days** (close-out in `SPRINT_04.md`). M2 is done as a development milestone: S10, S11 and S13 run end to end on the test release, keyboard-only and with axe in both appearances. Carried over as conditions, not as tasks: the 2026.1 priorities (O-1/K-1, still "unclear" on 2026-11-13), so the legacy filter stays where no release is active; the tester's manual checks of the M2 exit (#311); and the staging deployment with its new preconditions (#236), which is the owner's.

---

## Sprint goal

**People can register, confirm their address, sign in with the new cookie session, work in organisations and save, edit, share and export their criteria lists; settings, personal data export and account deletion work — M3 is finished and the JWT is gone.**

This sprint replaces the authentication of the whole application. The server side comes first, with its security invariants, then the client; the legacy account pages go only once their replacements work (O-S5-4). The screens of M3 have **no Figma frame**: they are built from the patterns of Handoff §5.3 (owner, 2026-11-13).

### Success criteria

- [ ] **Session (M3.1):** Identity cookie with antiforgery replaces the JWT; no token in browser storage; 401 opens the login overlay and keeps the page; sign-out; **SEC-01, SEC-02, SEC-03, SEC-09, OPS-04** `active`; #87 – #89 closed.
- [ ] **Accounts (M3.2):** registration with e-mail confirmation (mails to the real recipient, Mailpit in development and E2E), confirmation gate, password reset, lockout; **SEC-10, SEC-11, PRV-11** `active`; #301, #302 closed.
- [ ] **Organisations (M3.3):** create, invite, members and roles, leave, last-Owner rule, organisation switch, signed-in header states; **TEN-01, TEN-02, TEN-05** (skeleton), **TEN-11** (organisation part) `active`.
- [ ] **Saved lists (M3.5, M3.6):** save after sign-in keeps the draft (J1 second part — M3 exit criterion), list, edit, delete, saved export, older-release notice and copy, share links with expiry and the shared list without account; **CAT-08, CAT-22, EXP-06, TEN-08, TEN-09, TEN-12** and the persisted parts of EXP-01/EXP-03 and TEN-04 `active`.
- [ ] **Settings and account (M3.7, M3.8):** S60 replaces the legacy settings store; personal data export; account deletion with its rules; **A11Y-06**, the explicit-setting part of **A11Y-08**, **PRV-07** (account part), **TEN-11** (account part) `active`.
- [ ] Legacy auth gone: JWT, `jwtDecode`, `Identity.UI`, the `{ code, message }` failures, `DateTime.UtcNow` in `UserService`/`UserRepo`, the M3 entries of `LEGACY_EXCEPTIONS` (#158) as files are replaced.
- [ ] **Debt of TECH_DEBT_v4 §9.1 closed:** #309, #308, #264, #306, #312, #313; the Dependabot majors reviewed.
- [ ] Every new page in `reflow.spec.ts` (A11Y-09) and covered by axe in both appearances.

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | **~90 points** | Documentation, WERTE.IT coordination, reviews, the owner decisions below and the staging deployment (#236) are outside these days |
| **Total** | **8 days** | **~90 points** | |

**Basis:** sprint 02 ≈ 8, sprint 03 ≈ 16, sprint 04 ≈ 19 points per development day. This sprint assumes ≈ 11 per day: it replaces the authentication of the whole application, its tasks carry security invariants and EF migrations that must land one at a time per module, and the M3 screens have no Figma frame.

**Planned load:** **68.5 points committed (76 %** of the assumed capacity), of which 3.5 points are general debt (S5-01 – S5-06; 4 %) and 8 points are M3 debt rows (M3.1d 3, M3.2d 2, M3.5d 1.5, M3.7d 1.5), + **10 points stretch** (M4 and two M2 follow-ups).

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimates of `ROADMAP_v5.md`, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request; a task may be split if its plan shows more than one session. Order = recommended working order (Sequence below). Invariants per task are a **proposal**; Claude Code confirms them against TESTING §4 in its plan.

### P0 — committed (68.5 points)

#### Debt first — 3.5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-01 | Quick wins in one PR: scope the step-line checks of the S10 E2E tests to `#main` (#309, TD-38); widen the nginx `immutable` pattern to every hashed chunk and check the headers on the client image (#308) | 0.5 | Both issues closed; the S10 tests no longer match the CDK live-announcer element |
| S5-02 | Local dev server (#264): `provideMarkdown()` only on the legacy routes that need it (≈ 75 kB out of the first download), or a stub for the optional import | 0.5 | `npm start` renders the app; #264 closed |
| S5-03 | Pin the .NET SDK in `global.json` and the image tag together per **O-S5-1** (#306) | 0.5 | Local, CI and image use the same SDK; the notices check is stable; #306 closed |
| S5-04 | Review the Dependabot majors MailKit 5 (#288) and Swashbuckle 11 (#289, #291, #292) per **O-S5-2**, with the regenerated `THIRD-PARTY-NOTICES.txt` and the `EMBEDDED` review of the swagger-ui-dist; plus any new Dependabot PRs | 0.5 | Each PR merged or closed by the owner with the reason in the protocol; **before S5-11** (MailKit) |
| S5-05 | Documentation session (#313, TD-50): the sprint-04 findings in SPEC, ARCHITECTURE, TESTING, Handoff (S10 no longer `mat-stepper`), Figma mapping, and their headers naming current versions. ROADMAP v5 and the SPRINT_04 close-out are done in the control chat; PRD, Kriterienkatalog and UX copy are the owner's | 1 | #313 closed for Claude's part |
| S5-06 | Reflow check with a card that has two procedures (#312, TD-54); fix the tab padding only if needed | 0.5 | `reflow.spec.ts` opens such a card at 320 px; #312 closed |

#### M3.1 Session — 8 points (+ 3 points M3.1d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-07 | Server: ASP.NET Core Identity cookie authentication (`HttpOnly; Secure; SameSite=Strict`, 30 min idle / 8 h absolute per ARCHITECTURE §9.1) with antiforgery (`X-XSRF-TOKEN`) on every state-changing endpoint except the anonymous export; anonymous session probe `GET /auth/session`; sign-in and sign-out endpoints; 401 instead of 302; the test hosts of #87 – #89; antiforgery on `criteria-list-shown` | 4 | **SEC-01** (server part), **SEC-02, SEC-03** (API part), **SEC-09, OPS-04** `active`; #87, #88, #89 closed |
| S5-08 | Client: session state in `withIdentity()` from the probe, the interceptor (antiforgery header, 401 → login overlay and retry), `signedIn` taken from the session; the JWT, `jwtDecode` and every token in browser storage removed | 3 | **SEC-01** (client part), **SEC-03** (overlay part) `active`; no `localStorage`/`sessionStorage` access for auth |
| S5-09 | Sign-in S01 in the named `login` outlet (form card per Handoff §5.3, `auth.login.reasonSave` when coming from „Liste speichern“), the login overlay after 401 keeping the page and its input (C60), sign-out (R-AUTH-08); the login outlet reflows at 320 px (#170 L1) | 2 | R-AUTH-03, R-AUTH-07 (base), R-AUTH-08 met; S01 in `reflow.spec.ts` |
| S5-10 | Legacy auth cleanup (M3.1d) per **O-S5-4**: `Microsoft.AspNetCore.Identity.UI`, the `{ code, message }` failures of `UsersController` and the identity endpoints → Problem Details, `DateTime.UtcNow` in `UserService`/`UserRepo` → `TimeProvider`, `ERROR_TRANSLATIONS`, the legacy login/register/reset pages and their `LEGACY_EXCEPTIONS` entries once S5-09, S5-12, S5-13 work | 2 | No JWT code left; API-01 green for the identity endpoints; #158 smaller by the auth files |

#### M3.2 Accounts — 8 points (+ 2 points M3.2d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-11 | `IEmailSender` with MailKit over SMTP/TLS in Platform (ARCHITECTURE C71): Lettermint in staging/production (credentials only from the environment), Mailpit in development and E2E; message types with the `email.*` texts; open/click tracking off; recipients not logged above `Debug`; mails go to the real recipient (#302); the E2E `compose.log` artefact checked for secrets (TD-12) | 3 | **PRV-11** `active`; #302 closed |
| S5-12 | Registration S02 (`auth.register.*`, password ≥ 12 🔧), e-mail confirmation with single-use hashed link (24 h 🔧), result pages, the confirmation gate (403 `email-unconfirmed`, `auth.confirm.required` notice with resend) for saving, organisations, invitations, contributor profiles and publication; registration without the missing `Member` row per **O-S5-3** (#301); registration rate limit (API-05) | 3.5 | **SEC-10** `active`; #301 closed; S02 in `reflow.spec.ts` |
| S5-13 | Password reset S03 (same answer for known and unknown addresses, single-use hashed link 2 h 🔧) and lockout after 5 failed sign-ins for 15 minutes 🔧 on the fake clock; password-reset rate limit | 2.5 | **SEC-11** `active`; S03 in `reflow.spec.ts` |
| S5-14 | E2E: users in the E2E seeder (TESTING §6.3, known test credentials, no real data); an account journey register → mail in Mailpit → confirm → sign in → sign out, with axe in both appearances | 1 | Journey green on all five browser projects |

#### M3.3 Organisations — 13 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-15 | Organisation domain and tenancy: `Organization`, `OrganizationMembership` (Owner / Editor / Reader) in the module ARCHITECTURE §4.3 names, EF global query filter on `OrganizationId`, resource-based authorisation (organisation from the resource, never from a header; foreign resources → 404), the reflection-driven isolation test, the `IgnoreQueryFilters()` allow-list test, the authorisation-matrix skeleton; contracts rule per **O-S5-5** if a contracts type needs another module's contracts | 4 | **TEN-01, TEN-02, TEN-05** (skeleton) `active` |
| S5-16 | Create organisation S20 (name ≤ 120, type; confirmed e-mail required; creator becomes Owner) and members page S21 (members with role, Owner changes roles and removes members, revokes invitations), leave (R-ORG-04), last-Owner rule (409) | 4 | **TEN-11** (organisation part) `active`; R-ORG-01, -03, -04, -06 met; S20, S21 in `reflow.spec.ts` |
| S5-17 | Invitations by e-mail (`org.invite.*`): 14 days 🔧, single use, stored hashed, bound to the invited address, role chosen on invite; accept flow with the confirmation gate | 3 | R-ORG-02 met; invitation tests on the fake clock |
| S5-18 | Header states SignedIn and SignedInNoOrg with „Projekte“ and the organisation display only with a membership (R-SHL-02), organisation switch for more than one membership (R-SHL-03) that scopes lists to the chosen organisation | 2 | R-SHL-02, R-SHL-03 met; switch test against TEN-01 |

#### M3.5 Saved lists — 13 points (+ 1.5 points M3.5d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-19 | Configurator persistence: `CriteriaConfiguration`, `ConfigurationAppType`, `ConfigurationProductPart`, `ConfigurationTestType`, `ConfigurationCriterion` (organisation-owned, pinned to a release), `IConfigurationReader` in Configurator.Contracts, `PersistedConfigurationSource` in Export; the ARC-03 rule for `IConfigurationReader`; override keeps the recommended level | 4.5 | **CAT-08, EXP-06** `active`; TEN-01 covers the new entities |
| S5-20 | Save dialog S12 (name ≤ 120, organisation note) with the draft kept across sign-in (R-SAV-01, `auth.login.reasonSave`); saved lists S14 (organisation lists only, sortable, results announced); J1 second part: save after sign-in | 4 | J1 second part green (M3 exit criterion); S14 in `reflow.spec.ts` |
| S5-21 | Edit a saved list on S11 (include/exclude, priority, note; „Auswahl ändern“ back to S10), delete with confirmation (R-SAV-06), saved export `POST /configurations/{id}/exports`, older-release notice and „Neue Liste mit derselben Auswahl“ (R-LST-14, copy per D6) | 4.5 | **CAT-22** and the persisted parts of **EXP-01, EXP-03** `active` |
| S5-22 | M3.5d: the weight range served with the saved-list API instead of the duplicated client constant (TD-45); a too-long note leads to the note in S13 (TD-40); `criteria.list.unsaved` and the draft-note wording; `no-store` on workspace answers (TEN-04 workspace part) | 1.5 | TEN-04 workspace part `active` |

#### M3.6 Share links — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-23 | `ShareLink` (organisation-owned, token stored only as hash, expiry required: default today + 90 days, max 1 year 🔧) with management endpoints and `IShareableResourceProvider` implemented by Configurator; anonymous `GET /api/v1/shared/{token}` with the redacted list | 3 | **TEN-08, TEN-12** `active` |
| S5-24 | Share dialog S15 (dialog rules of S13) and the shared list S16 at `/geteilt/:token` without account (S11 read-only: no checkboxes, no K5 menu, no action bar; `criteria.shared.*`) | 2 | **TEN-09** `active`; S16 in `reflow.spec.ts` |

#### M3.7 Settings — 5 points (+ 1.5 points M3.7d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-25 | `UserPreferences` (language, appearance, motion, contrast „wie im System“) stored per user with its API; the client applies them and replaces the legacy settings store (no storage for signed-out users, no English snackbar, no crash with blocked storage — #234 TD-25); anonymous users keep following the system | 3 | **A11Y-06** and the explicit-setting part of **A11Y-08** `active`; TD-25 part of #234 done |
| S5-26 | Settings page S60 per Handoff §5.3 (fieldsets „Sprache“, „Erscheinungsbild“, „Bewegung und Animation“, „Kontrast“); the change applies without reload | 3.5 | R-SET-01 met; S60 in `reflow.spec.ts` |

#### M3.8 Account — 5 points

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S5-27 | Personal data export `GET /api/v1/account/export` as JSON per SPEC §8.4 (only the user's own data) | 2 | **PRV-07** (account part) `active` |
| S5-28 | Account page S61: export download, change e-mail and password (rebuilt from the legacy profile), account deletion with its rules (sole Owner of an organisation with other members is refused; an organisation whose last member leaves is deleted with its lists, share links and projects) ⚖️ texts as written; the legacy delete dialog with its contrast (#234 TD-26) and native `disabled` goes | 3 | **TEN-11** (account part) `active`; R-AUTH-09, R-AUTH-10 met; #234 closed; S61 in `reflow.spec.ts` |

### P2 — stretch (10 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S5-29 | Session hardening (M4.1): absolute limit with the stored sign-in time, sliding renewal, expiry headers, `POST /auth/session/extend`, security-stamp invalidation on password change and account deletion | 5 | ROADMAP M4.1 |
| S5-30 | Session warning dialog and the completed login overlay (M4.2, Handoff §5.3) | 3 | ROADMAP M4.2 |
| S5-31 | „Was bedeuten die Bewertungsstufen?“ on S11 (`pa-rating-legend`, Handoff §4.11, UX copy U24) (#279) | 1 | M2 follow-up |
| S5-32 | Design fidelity of S10/S11: „Auswahl ändern“ next to the H1 from 600 px, the sticky action bar across the viewport, 32 px stepper circles, summary labels, desktop filter row (#278, #281, #282) | 1 | M2 follow-up |

Stretch items are pulled in order; if one is started but not finished, it goes back to its backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | S5-01 – S5-04, S5-06; **S5-07** (the server session, alone in its module) | **Owner decisions O-S5-1 – O-S5-4 taken**; Dependabot majors settled before S5-11 |
| First half | D1–D4 | S5-08 → S5-09 (client); S5-11 → S5-12, S5-13 → S5-14 (accounts); S5-15 → S5-16, S5-17 (organisations); S5-10 once S5-09, S5-12, S5-13 are merged | **Mid-sprint check (end of D4):** sign-in, registration, confirmation, reset and organisations work end to end (≈ 35 points). If not: drop the stretch, then S5-27/S5-28 move to sprint 06 |
| Second half | D5–D8 | S5-18; S5-19 → S5-20 → S5-21 → S5-22; S5-23 → S5-24; S5-25 → S5-26; S5-27 → S5-28; S5-05 last | M3 exit: J1 second part green; tenant-isolation invariants active |
| Buffer | remaining | stretch S5-29 – S5-32 | – |

**Parallel work:** the accounts line (S5-11 – S5-14), the organisations line (S5-15 – S5-18), the client line (S5-08, S5-09) and later the settings line (S5-25, S5-26) can run in separate worktree sessions once S5-07 is merged. Two limits decide what may overlap: **EF migrations of the same module one at a time** (two branches with migrations in one module conflict in the model snapshot), and **one local E2E stack** at a time (shared host ports).

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| S5-03 | **O-S5-1** | Philipp, D1 |
| S5-04 | **O-S5-2** | Philipp, D1 |
| S5-08, S5-11, S5-12, S5-13, S5-15, S5-25 | S5-07 merged | internal |
| S5-09 | S5-08 | internal |
| S5-10 | S5-09, S5-12, S5-13; **O-S5-4** | internal / Philipp |
| S5-11 | S5-04 (MailKit major settled) | internal |
| S5-12, S5-13 | S5-11; **O-S5-3** for S5-12 | internal / Philipp |
| S5-14 | S5-12, S5-13 | internal |
| S5-15 | S5-07; **O-S5-5** if a contracts type needs another module's contracts | internal / Philipp |
| S5-16, S5-17 | S5-15; S5-12 (confirmation gate); S5-11 for S5-17 (mail) | internal |
| S5-18 | S5-16, S5-09 | internal |
| S5-19 | S5-15 | internal |
| S5-20 | S5-19, S5-09 (overlay keeps the draft) | internal |
| S5-21, S5-22 | S5-20 | internal |
| S5-23 | S5-19 | internal |
| S5-24 | S5-23 | internal |
| S5-26 | S5-25 | internal |
| S5-27 | S5-15 | internal |
| S5-28 | S5-16, S5-21, S5-23, S5-27 | internal |
| S5-29, S5-30 | S5-07 – S5-09; S5-28 for the security stamp on deletion | internal |
| **EF migrations** | IdentityAccess (S5-07 if it adds tables, S5-12, S5-13, S5-15, S5-17, S5-25), Configurator (S5-19, S5-23): one open migration branch per module at a time | internal |
| **Invariant flips** | almost every task: preferably one at a time; otherwise the later PR rebases and renumbers its TESTING version | internal |
| Outside the sprint | Staging deployment per the checklist on #236 (key-ring certificate, plain key rows, log search, API port, environment variable); Lettermint sending domain with SPF/DKIM for staging; O-1/K-1 and O-6 from the project team; the tester's M2 pass (#311) | Philipp, project team, tester |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal |
|---|---|---|---|
| O-S5-1 | Pin the .NET SDK in `global.json` and the image tag together (#306)? | S5-03 | Yes, pin both to the same patch; Dependabot updates them together |
| O-S5-2 | Dependabot majors: MailKit 5 (#288), Swashbuckle 11 (#289, #291, #292) — take them, or put them on the major-ignore list | S5-04 | Swashbuckle: one PR for all four packages with the `EMBEDDED` review; MailKit 5: take it before S5-11 if the changelog and the e-mail tests allow |
| O-S5-3 | Registration and the missing `Member` row (#301): (a) the new registration does not use the legacy `Member` role (roles come from memberships), legacy checks of `Member` removed; (b) a migration that creates `Member` everywhere | S5-12 | (a), if nothing outside the replaced legacy code still needs `Member`; otherwise (b) |
| O-S5-4 | Legacy account pages: S01 – S03, S60 and S61 replace the legacy login, register, reset, confirm, profile and settings pages at the target routes; the legacy pages are deleted once their replacement is merged; existing users sign in again after the switch (no data migration; pre-pilot data may be reset, ARCHITECTURE §12 step 3) | S5-10 | Yes |
| O-S5-5 | O-22: may a `Contracts` project reference another module's `Contracts` (e.g. Configurator.Contracts → Catalog.Contracts)? | first contracts type that needs it (S5-15, S5-19) | Yes, only along the dependency table of ARCHITECTURE §4.3 and without cycles, with an ADR and the ARC-03 test adjusted |
| — | Staging deployment (#236) and the Lettermint sending domain for staging | before M3 is shown on staging | — |
| — | Priorities, rationales, titles of 2026.1 (O-1, K-1, K-6) and the criteria-suggestion channel (O-6) — project team | M2.3; O-6 before G-P | — |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | The authentication of the whole application changes in one step | Everything signed in breaks if the client and server disagree | Server first with SEC-01/02/03/09 and OPS-04 (S5-07), then the client (S5-08); the legacy pages go only after their replacements (O-S5-4); the strongest model for these tasks |
| R2 | Parallel branches add EF migrations to the same module | Snapshot conflicts, broken migration order | One open migration branch per module (Dependencies); OPS-03 on every PR |
| R3 | Tenant isolation is the first multi-tenant code | A data leak between organisations | TEN-01 is reflection-driven, so new entities are covered automatically; TEN-02 allow-list; foreign resources → 404 |
| R4 | Real e-mail on staging before the sending domain and the deployment are ready (#236) | Mails bounce or go nowhere; staging shows old code | Mailpit in development and E2E; staging demos of M3 only after #236 |
| R5 | M3 screens have no Figma frame | Visual drift, review rounds | Handoff §5.3 patterns; UX copy keys exist; design follow-ups as issues, not as blockers |
| R6 | ⚖️ texts (privacy acknowledgement, account deletion, export note) are placeholders until the legal review (O-4) | Wording changes later | Implement as written; never change ⚖️ texts |
| R7 | The draft must survive sign-in (J1 second part) across the login outlet and the overlay | M3 exit missed | Covered by S5-20's journey; the draft stays in memory, never in storage |
| R8 | Velocity jumps between sprints (8 → 16 → 19 per day) | Over- or under-commitment | 76 % load; mid-sprint check; record days used at the close-out |
| R9 | Many invariant flips in parallel | TESTING version conflicts | The later PR renumbers (as in sprint 04) |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6.7) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`, incl. CodeQL).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, deferred parts recorded there.
- [ ] UI strings only by key (`node scripts/check-i18n.mjs --write` for areas in `AREAS`); new pages join `reflow.spec.ts` and the axe runs in both appearances; their icons are in the icon list.
- [ ] `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` and `reuse lint` green; OPS-03 green for every new migration.
- [ ] New files carry SPDX headers; no secrets committed; licences of new dependencies checked (CLAUDE §10) and approved by the owner.
- [ ] Documentation touched by the change updated (`CLAUDE.md` §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping); register items closed by the PR named in it; replaced legacy files removed from `LEGACY_EXCEPTIONS` (#158).
- [ ] Protocol `docs/sprints/sprint_05/S5-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in the earlier sprints.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; debt quick wins; S5-07; owner decisions O-S5-1 – O-S5-4 |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: register → confirm → sign in → organisation → save → share → export, in both appearances; success criteria checked |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v5.md`; close-out below |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 06 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** M4 (session warning and hardening, if not done as stretch) and M5 — workspace: projects S30 – S32, candidates S33/S34, assessment S34b, concurrency (S37); release 2026.1 activated and the legacy filter removed as soon as O-1/K-1 arrive.

# ProcureAccess — Feature Specification

> **Status:** v1 (2026-10-15) — output of Phase 3, step 8 ("Write the feature spec") of `ai_development_process.md`. First version.
> **Combines:**
> - `prd_procureaccess_werteit_v5.md` — product scope, personas, P0/P1/P2 (authoritative for *what* and *why*)
> - `ARCHITECTURE_v10.md` — modules, flows A–E, domain model, API (authoritative for *how it is built*)
> - `design_handoff_procureaccess_v1.md` (1.12) — screens, components, behaviour, tokens (authoritative for *how it looks and behaves*)
>
> **Also used:** `ux_copy_procureaccess_v12.md` (all UI strings and keys, screen inventory), `TESTING_v8.md` (invariant catalogue), `kriterienkatalog_bit_inklusiv_v1.md` (1.5), `pruefung_hauptdokumente_v1.md` (decisions D1–D7), `CLAUDE.md` (existing codebase), owner decisions of 2026-10-15 (§0).
>
> **Language:** English (project convention for technical documents). UI terms are quoted in German exactly as defined in the UX copy §2; the UX copy remains the only source for UI wording.
>
> **Purpose:** turn the product requirements into buildable, testable feature requirements with acceptance criteria. This document is the input for `CLAUDE.md` (step 9), `ROADMAP.md` (step 10) and sprint planning (step 11).

---

## How to read this document

- **Single source per statement.** This spec does not repeat what other documents already define. It references them:
  - architecture by section or change number (e.g. `ARCH §4.4`, `ARCH C53`),
  - invariants by ID (e.g. `SIG-08`) — an acceptance criterion that names an invariant is satisfied when that invariant's tagged tests pass,
  - UI strings by key (e.g. `criteria.list.draftNote`),
  - screens by ID (e.g. `S11`) and components by ID (e.g. `K1`) as in the Design Handoff.
- **Precedence on conflict:** PRD (scope) → this spec (behaviour and acceptance) → ARCHITECTURE (technical shape) → Handoff (visual detail) → UX copy (wording only). Report conflicts as an issue; do not guess.
- **IDs used here**
  - `FA-xx` feature area · `US-<AREA>-nn` user story · `R-<AREA>-nn` requirement.
  - Priority: **P0** = MVP, **P1** = fast follow (short spec only, detailed in SPEC_v2), **P2** = future, design must not block it.
  - Acceptance criteria use **Given/When/Then** for stateful flows and **checklists** otherwise.
- **Placeholders** marked ⚖️ depend on the bundled legal review (PRD §11, ARCH O-4); marked 🔧 are configurable values with a proposed default.

### Versioning of this document

`SPEC_v1.md` is versioned like the other documents (new file per version, change table at the top). A new version is due:

1. **At the start of sprint planning for migration step 7** (Moderation and P1): SPEC_v2 details the P1 feature areas (FA-18) that are only outlined here.
2. **Earlier, if** the bundled legal review returns results (O-4), or a migration step forces a deviation from an acceptance criterion here, or the owner changes a P0 decision.

Minor clarifications that do not change behaviour are recorded as "v1.x" in the change table of the same file only when the owner asks for it; otherwise a new version is created.

---

## 0. Decisions for this spec (owner, 2026-10-15)

| # | Topic | Decision |
|---|---|---|
| SD1 | Output | Markdown, `claude/SPEC_v1.md` in the project knowledge, later `docs/SPEC_v1.md` in the repository; English |
| SD2 | Structure | Feature areas grouped by persona; each with user stories, requirements, acceptance criteria and references; mapping to migration steps 0–7 (§11) |
| SD3 | Acceptance criteria | Given/When/Then for stateful flows, checklists otherwise; reference other documents by ID instead of copying |
| SD4 | P1 depth | Short specification only (FA-18); details in SPEC_v2 at the start of sprint planning for migration step 7 |
| SD5 | Success metrics | PRD indicators without target values; targets after the pilot. Final usage-event list in §9 |
| SD6 | Priority terms | „A – Mindestanforderung“, „B – Bewertungskriterium“, „Hinweis“ used as **provisional** (O-14 stays open) |
| SD7 | Staleness note | Threshold **18 months** (closes O-15 / UX-O8) |
| SD8 | Rating-level explanations | Used as **provisional**; review stays open (O-16 / UX-O19 / H13) |
| SD9 | Field lengths | Handoff §8.2 proposals adopted (§7.2) (closes H6) |
| SD10 | API shapes | `excludedBy[]`, `appTypesWithoutCriteria[]` and the filter-options response are fixed in §8.1 (closes O-8) |
| SD11 | Dark theme | **In v1 from the start** (P0). Requires a dark scheme in Figma and an accessibility review before release (§7.5, H3 changed) |
| SD12 | Personal settings | v1: language (UI German only), appearance (system / light / dark, follows S11), reduced motion, contrast „wie im System“. „Hoher Kontrast“ follows when H2 is designed and reviewed (target: before the pilot). No text-size setting (browser zoom) |
| SD13 | E-mail | Existing e-mail service **Lettermint** (EU, transactional) is used; e-mail address confirmation at registration is part of v1 (§7.6) |
| SD14 | Personal data export | JSON file (§8.4) |
| SD15 | Share links | Expiry required, default 90 days, maximum 1 year |
| SD16 | Criteria suggestion contact | Configurable placeholder until the address or form exists (O-6 stays open) |
| SD17 | Legal pages | Imprint, privacy notice, accessibility statement as static pages (P0); content supplied by WERTE.IT, outside this spec |
| SD18 | Pilot date | None yet; phasing without dates (§12) |

**Proposals made in this spec that need owner confirmation** (marked "proposal" where they appear; summarised in §13 as O-S1 … O-S4):
catalog maintenance without an editor UI in v1 (FA-17), minimal administration screens for product identities (FA-17), deletion of candidate products (R-CAN-09), supported browsers (§7.4).

---

## 1. Problem statement

Procuring organisations — public bodies and private companies — still buy software that employees with disabilities cannot use, and those employees then leave. Buyers lack orientation (which criteria apply to my purchase?), understandable criteria (EN 301 549 / BITV are written for specialists), product knowledge (which products meet them?) and a way to carry requirements into a tender (PRD §2). The existing *Standardanforderungskatalog des Bundes* offers filtering and export, but no prioritisation by use case, no plain-language criteria and no shared product knowledge (PRD §3). Without ProcureAccess, accessibility requirements stay generic or missing in tenders, and every organisation evaluates products alone.

## 2. Goals

From PRD §5; the column "Verified by" says how this spec makes each goal checkable.

| # | Goal | Verified by |
|---|---|---|
| G1 | Buyers without prior knowledge find the prioritised criteria for their case within a short time | FA-04/FA-05 without account; J1 completes keyboard-only (A11Y-02); usage events §9; pilot feedback |
| G2 | Tender teams take criteria into the tender documents without significant rework | XLSX export spec §8.3; EXP-01–07; manual check of the export with a screen reader (TESTING §7.2) |
| G3 | A growing base of shared product statements (Kanal 1 + Kanal 2) without a central product database | FA-10, FA-13, FA-14; counted per §9 |
| G4 | At least one pilot with a cooperation partner validates the prioritisation logic | Stored `ResolvedLevel` vs. overrides (`ARCH §5.3`, CAT-08); staging environment (`ARCH §10.2`) |
| G5 | The tool meets the requirements it promotes, also on smartphones (mobile-first) | §7.4, §7.5, FA-03; A11Y-01–07; no open `a11y`+`blocker` issue at release |

## 3. Non-goals

PRD §6 applies unchanged (no audit/certification tool and no overall verdict; no generic vendor tool; no central product database maintained by WERTE.IT; no ISO 9241 in the catalog; no DIN EN 17161). Additional non-goals for v1 of this spec:

| Non-goal | Why |
|---|---|
| English user interface | UX copy §0: English texts follow later; API already honours `Accept-Language` (API-06) so nothing blocks it |
| Text-size setting | Browser zoom up to 400 % is supported (§7.4); a second mechanism adds test effort without benefit |
| Share links for projects | D7; criteria lists only |
| Carrying assessments over to a newer catalog release | O-13; v1: new project (ARCH §5.4) |
| Catalog editor user interface | Proposal (FA-17): releases are prepared as data files in the repository and imported; a handful per year does not justify a UI for a solo maintainer |
| Verification of vendor and auditor profiles | Depends on legal review §16.2.7; hook prepared (`ARCH §16.2.7`) |
| Real-time collaboration, offline mode, public write API | `ARCH §3.3` |
| Deleting procurement projects | Projects are archived via status (R-PRJ-05); deletion follows with the data-retention rules of the legal review |

## 4. Personas and terms

Personas: PRD §4 (Einkäuferin, ausschreibungsverantwortliche Person, Anbieter/Prüferin, Menschen mit Behinderung als Beitragende). Additional operational roles used in this spec: **Organisation roles** Owner/Editor/Reader (UI: Verwalten/Bearbeiten/Lesen), **Platform roles** `PlatformAdmin`, `CatalogEditor`, `Moderator` (`ARCH §9.1`). UI terms: UX copy §2 (binding). Internal terms (Kanal 1/2, Workspace, Konfiguration, Release, Kandidat) never appear in the UI (UX copy G10).

## 5. Scope overview

"Acc." = account requirement: ✗ none · ✓ account · ✓O account and organisation. "Step" = migration step in `ARCH §12` in which the feature is built.

| FA | Feature area | Persona | Screens | Flow | Acc. | Prio | Step |
|---|---|---|---|---|---|---|---|
| FA-01 | App shell, navigation, legal pages, error pages | all | S00, S04, S80, legal pages | – | ✗ | P0 | 1–3 |
| FA-02 | Account, sign-in, session, e-mail | all | S01–S03, S61, login overlay, session warning | – | ✗/✓ | P0 | 3–4 |
| FA-03 | Personal settings and appearance (incl. dark theme) | all | S60 | – | ✓ (theme also ✗) | P0 | 1, 3 |
| FA-04 | Criteria configurator | buyers | S10 | A | ✗ | P0 | 2 |
| FA-05 | Criteria list and export | buyers, tender teams | S11, S13 | A1 | ✗ | P0 | 2 |
| FA-06 | Saved lists, sharing, new list for a newer release | buyers | S12, S14, S15, S16 | A2 | ✓O (S16 ✗) | P0 | 3 |
| FA-07 | Organisations and members | buyers | S20, S21 | – | ✓ | P0 | 3 |
| FA-08 | Procurement projects | buyers | S30, S31, S32 | B | ✓O | P0 | 5 |
| FA-09 | Products in a project: evidence and assessment | buyers | S33, S34, S34a, S34b, S37 | B | ✓O | P0 | 5 |
| FA-10 | Publication from a project | buyers | S35, S36 | B | ✓O | P0 | 6 |
| FA-11 | Product directory search | all | S40 | C | ✗ | P0 | 6 |
| FA-12 | Product page and entry detail | all | S41, S42 | C | ✗ (compare ✓O) | P0 | 6 |
| FA-13 | Contributor profiles, experience reports, my contributions | contributors | S50, S51, S52, S53 | D | ✓ | P0 | 6 |
| FA-14 | Test-result publication by vendors and auditors | vendors, auditors | S55 | E | ✓ | P0 (report upload P1) | 6 |
| FA-15 | Criteria suggestion (external channel) | all | S70 | – | ✗ | P0 | 2 |
| FA-16 | Usage events for the success metrics | – | – | – | – | P0 | 1–6 |
| FA-17 | Operation and administration (catalog releases, product identities) | operators | admin pages (proposal) | – | platform roles | P0 | 2, 6 |
| FA-18 | P1 outline: moderation, conformance-report upload, advice note, external directories | all | S90 | – | ✓ | P1 | 7 |

---

## 6. Feature areas

Every feature area lists user stories, requirements with priority and acceptance criteria, and references. Requirements apply on every viewport width from 320 px (mobile-first, Handoff §2) and in light and dark appearance (§7.5, FA-03) unless stated otherwise.

### FA-01 App shell, navigation, legal and error pages

**User stories**
- US-SHL-01 — As any user, I want to reach every part of the tool by keyboard and screen reader, starting with a skip link, so that I can work without a mouse.
- US-SHL-02 — As any user on a smartphone, I want a compact menu, so that the navigation does not cover the content.
- US-SHL-03 — As a visitor, I want a start page that tells me what I can do without an account, so that I find the right entry point.
- US-SHL-04 — As a member of several organisations, I want to switch the active organisation, so that I see the lists and projects of the right team.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-SHL-01 | P0 | Page structure and landmarks per Handoff §2.1 and §7 | ☐ skip link `shell.skipLink` is the first focusable element and moves focus to `main#main` ☐ one `h1` per page ☐ `nav` with `shell.nav.label` ☐ A11Y-01 passes on every page |
| R-SHL-02 | P0 | Header `pa-header` with three states (Anonymous, SignedIn, SignedInNoOrg) and the compact layout below 600 px (Handoff §3.1) | ☐ „Projekte“ and the organisation display appear only with a membership ☐ active item carries `aria-current="page"` and the 3 px bottom border ☐ compact menu: `aria-expanded`, focus moves to first item, Esc closes and returns focus to „Menü“ |
| R-SHL-03 | P0 | Organisation switch for users with more than one membership (`shell.org.switch`) | **Given** a user with memberships in A and B **When** they switch to B **Then** lists, projects and the compare select show only B's data (TEN-01), the choice persists for the session, and the current page reloads its data |
| R-SHL-04 | P0 | Route change: page title from `pageTitle.*` via `shell.pageTitle`, focus on H1, announcement (A11Y-03) | ☐ title is most-specific-first (UX copy §5.16) ☐ dialogs do not change the title |
| R-SHL-05 | P0 | Start page S04 (Handoff §5.3) | ☐ four cards `home.card.*`, then link `home.card.vendor.text` → S55, then `home.disclaimer` ☐ no card requires an account to open |
| R-SHL-06 | P0 | Footer with `shell.footer.*` links (imprint, privacy, accessibility statement, source code, WERTE.IT, „Kriterium vorschlagen“) | ☐ links wrap on narrow screens ☐ legal pages reachable without account |
| R-SHL-07 | P0 | Legal pages as static routes `/impressum`, `/datenschutz`, `/barrierefreiheit` (SD17) | ☐ content is loaded from versioned Markdown files in the repository (`content/legal/*.de.md`), rendered as accessible HTML with real headings ☐ missing content shows a visible placeholder in non-production environments and fails the production build (so the site never goes live without them) |
| R-SHL-08 | P0 | Error pages S80 for 404, 403, 500 (`errors.*`) and the offline notice | ☐ foreign resources return 404, not 403 (TEN-03) ☐ page title = H1 ☐ `errors.home` button |
| R-SHL-09 | P0 | Loading, empty and system states per Handoff §6.2–§6.5 | ☐ `mat-progress-bar` + one `common.loading` announcement ☐ no skeleton screens ☐ empty states are text + one action |
| R-SHL-10 | P0 | Every page usable at 320 px CSS width and 400 % zoom without horizontal page scrolling (Handoff §2.2) | ☐ manual check per TESTING §7.2 ☐ DOM order = visual order |

### FA-02 Account, sign-in, session and e-mail

**User stories**
- US-AUTH-01 — As a buyer, I want to create an account with e-mail and password, so that I can save lists and work in projects.
- US-AUTH-02 — As a user, I want to confirm my e-mail address once, so that password resets and invitations reach me.
- US-AUTH-03 — As a user who forgot the password, I want to reset it by e-mail without the tool revealing whether an account exists.
- US-AUTH-04 — As a user whose session ended, I want to sign in again without losing what I typed (WCAG 2.2.1).
- US-AUTH-05 — As a user, I want to be warned before my session ends and to extend it.
- US-AUTH-06 — As a user, I want to download my data and delete my account.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-AUTH-01 | P0 | Registration S02 with e-mail, password, privacy acknowledgement (`auth.register.*`) | ☐ password: at least 12 characters 🔧, no composition rules, maximum 128 ☐ duplicate e-mail gives the same success page as a new one (no account enumeration); the existing account owner receives an e-mail „already registered“ instead ☐ validation per §7.1 |
| R-AUTH-02 | P0 | E-mail confirmation (SD13) | **Given** a new account **When** registration succeeds **Then** `auth.register.success` is shown and a confirmation e-mail is sent (§7.6) · **Given** an unconfirmed account **When** the user signs in **Then** sign-in succeeds, but saving, creating organisations, accepting invitations and publishing are blocked with a notice and a „resend“ action until the address is confirmed (SD13) · confirmation link valid 24 h 🔧, single use |
| R-AUTH-03 | P0 | Sign-in S01 in the named `login` outlet; `auth.login.reasonSave` when coming from „Liste speichern“; draft kept (`auth.login.draftKept`) | ☐ `auth.login.error.invalid` does not name the wrong field ☐ lockout after 5 failed attempts for 15 minutes 🔧 (Identity lockout), message reuses `auth.login.error.invalid` plus a lockout hint (new key, §14) ☐ J1 second part passes |
| R-AUTH-04 | P0 | Password reset S03 (`auth.reset.*`) | ☐ same response for known and unknown addresses ☐ link valid 2 h 🔧, single use ☐ success ends all other sessions (SEC-08) |
| R-AUTH-05 | P0 | Session model per `ARCH §9.1` (Identity cookie, antiforgery, 30 min idle, 8 h absolute) | SEC-01, SEC-02, SEC-03, SEC-07, SEC-08, SEC-09, OPS-04 |
| R-AUTH-06 | P0 | Session warning dialog (Handoff §5.3) | **Given** 5 min before the earlier limit **When** the idle limit is next **Then** the alertdialog shows `shell.session.warning.body` with focus on `.stay`; choosing it calls `POST /auth/session/extend` and announces `shell.session.extended` · **When** the absolute limit is next **Then** `shell.session.warning.absolute` without the extend button · A11Y-04 |
| R-AUTH-07 | P0 | Login overlay after 401 (`ARCH §8.3`) | **Given** an expired session and unsaved input **When** the user triggers a request **Then** the overlay opens without navigation, after sign-in the request is retried, the input is still there and `shell.session.restored` is announced · closing the overlay leaves the user signed out on the same page · J5 |
| R-AUTH-08 | P0 | Sign-out (`shell.nav.logout`) | ☐ cookie cleared, new antiforgery token, in-memory stores cleared (except the anonymous configurator draft is also cleared) ☐ focus to H1 of the start page |
| R-AUTH-09 | P0 | Personal data export S61 (SD14) as JSON (§8.4) | ☐ download contains only the user's own data as defined in §8.4 (PRV-07) ☐ generated synchronously, `Cache-Control: no-store` ☐ never contains other users' identities or `UserContributorLink` beyond the user's own profiles |
| R-AUTH-10 | P0 | Account deletion S61 (`account.delete.*`) ⚖️ | **Given** a user who is the only Owner of an organisation with other members **When** they delete the account **Then** deletion is refused with a message naming the organisation (new key, §14) until ownership is transferred · **Given** any other user **When** they confirm with their e-mail address **Then** credentials are removed, memberships removed, contributor profiles detached and anonymised, their visible entries are withdrawn, experience reports are purged (PRV-10), other sessions end (SEC-08) · an organisation whose last member leaves this way is deleted with its lists and projects; published entries from it remain under their (anonymised) profile only if they were not published by this user (PRV-06 stays conditional on the legal review) |

### FA-03 Personal settings and appearance

**User stories**
- US-SET-01 — As a user sensitive to motion, I want to reduce animations, so that the tool is comfortable to use.
- US-SET-02 — As a user who prefers a dark screen, I want the tool in a dark appearance that follows my system or my choice.
- US-SET-03 — As a user, I want my settings on every device.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-SET-01 | P0 | Settings page S60 with: language (only „Deutsch“ selectable in v1), appearance (system / hell / dunkel), motion (system / reduziert), contrast (only „Wie im Betriebssystem eingestellt“ until H2 is done) | ☐ saved in `UserPreferences`, applied on every device after sign-in (A11Y-06) ☐ change takes effect without reload and announces `settings.saved` ☐ no text-size setting (SD12) |
| R-SET-02 | P0 | Appearance without an account | ☐ anonymous users follow `prefers-color-scheme` and `prefers-reduced-motion` ☐ a signed-in user's explicit choice overrides the system preference ☐ no browser storage is required for this (works with storage blocked) |
| R-SET-03 | P0 | Dark theme (SD11) — implementation per §7.5 | ☐ every screen and component of the Handoff renders in both appearances ☐ A11Y-01 runs on each journey state in both appearances (TESTING follow-up, §14) ☐ release blocked until the dark scheme is designed in Figma and has passed an accessibility review (O-S5) |
| R-SET-04 | P1 | „Hoher Kontrast“ option | ☐ enabled only after H2 (design + review); target before the pilot |

### FA-04 Criteria configurator (S10)

Flow A, `ARCH §4.4`, §5.1; Handoff §5.4; Kriterienkatalog §2–§3.

**User stories**
- US-CON-01 — As a buyer without prior knowledge, I want to answer up to four questions about my purchase, so that I get the criteria that matter for it.
- US-CON-02 — As a buyer, I want to see why an option is not available instead of it silently missing.
- US-CON-03 — As a buyer, I want to change an earlier answer and be told what was reset.
- US-CON-04 — As a buyer on a smartphone, I want a searchable list for the use case that stays visible above the keyboard.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-CON-01 | P0 | Four steps in fixed order (use case → app types → product parts → test types), linear stepper; below 600 px the text line `criteria.select.stepLabel` | CFG-03, CFG-04, CAT-11 ☐ step headers are display-only (`aria-current="step"`) ☐ „Weiter“ validates on click (`criteria.select.error.*`) |
| R-CON-02 | P0 | Step 1: searchable single choice from active `UseCaseTemplate`s plus „Keiner davon passt“ always last | CFG-03, CAT-18 ☐ match count announced (`criteria.select.step1.matches`) ☐ combobox behaviour on the on-screen keyboard per Handoff §2.2 |
| R-CON-03 | P0 | Step 2: multi-select of the eight app types with hints for offen/geschlossen | ☐ at least one required (`criteria.select.error.noAppType`) |
| R-CON-04 | P0 | Step 3: multi-select of product parts; Basisfunktionalität checked and `aria-disabled`; toggle „Erklärungen anzeigen“ (default off) shows catalog explanations | CFG-03 |
| R-CON-05 | P0 | Step 4: optional multi-select of the five test procedures with link `testType.link` | CFG-04 ☐ each choice later adds one „Geprüft nach …“ criterion (CAT-16) |
| R-CON-06 | P0 | Exclusions from the catalog stay visible with reason (`criteria.select.excluded.*`), shape of `excludedBy[]` per §8.1 | CFG-02, CAT-12, CAT-19 |
| R-CON-07 | P0 | Changing a higher layer resets all lower layers (except Basisfunktionalität) with the notice `criteria.select.reset.*` | CFG-01 · **Given** steps 1–4 filled **When** the user changes the use case via „Ändern“ **Then** steps 2–4 are empty (step 3 keeps Basisfunktionalität), the attention notice is shown with `role="status"`, focus does not move, and the notice disappears with the next selection |
| R-CON-08 | P0 | „Bisherige Auswahl“ row from step 2 on, with „Ändern“ per layer | ☐ `criteria.select.summary` ☐ „Ändern“ carries the layer name via `aria-describedby` |
| R-CON-09 | P0 | Draft state lives only in `withConfigurator()` in memory (works anonymously; never written to browser storage) | ☐ FE unit test asserts no storage write ☐ reload loses the draft; S11 says so (`criteria.list.draftNote`) |
| R-CON-10 | P0 | „Kriterienliste anzeigen“ loads S11 for the selection; the server validates the selection again | CAT-11, CAT-12 ☐ a `CriteriaListShown` usage event is written once per complete selection shown (§9) |

### FA-05 Criteria list and export (S11, S13)

Flow A1, `ARCH §4.4`, §5.1; Handoff §5.1 S11, S13, §5.5.

**User stories**
- US-LST-01 — As a buyer, I want to see each criterion in plain language with its priority and the reason for it, so that I can judge whether it fits.
- US-LST-02 — As a buyer, I want to remove criteria, change priorities and add notes, so that the list fits my procurement.
- US-LST-03 — As a tender lead, I want to download the list as an Excel file without an account, so that I can put it into the tender documents directly.
- US-LST-04 — As a buyer, I want to read the full BIT inklusiv texts of a criterion, so that I understand what is required and how it is tested.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-LST-01 | P0 | List of criterion cards (compact + detail panel) per Handoff §5.5 | ☐ titles are H2, detail sections H3 ☐ detail texts are verbatim catalog content (`CriterionProcedureText`) with source line ☐ several procedures → tabs inside the panel ☐ each criterion has an anchor `#k-<number>` |
| R-LST-02 | P0 | Priority K5 with rationale, matched rule and source; „Empfehlung des Projektteams“ always present | CAT-01, CAT-02, CAT-14 ☐ in S11, rationale is the first detail section (`showRationaleToggle=false`) |
| R-LST-03 | P0 | Priority override per criterion via K5 menu; recommendation remains visible; reset | CAT-08 ☐ `criteria.priority.changed` announced ☐ B shows weight 🔧 (default from `PriorityRecommendation.Weight`, editable 1–10) |
| R-LST-04 | P0 | Include/exclude per criterion; excluded cards stay in the list (`surface-container-low`, „Nicht in Ihrer Liste“) | ☐ live counter `criteria.list.status.count` ≤ 1 announcement/s ☐ filter „Nur aufgenommene Kriterien anzeigen“ |
| R-LST-05 | P0 | Note per criterion (≤ 1 000 characters, §7.2) | ☐ `criteria.item.note` ☐ length error per §7.1 |
| R-LST-06 | P0 | Search (debounce 300 ms, client-side over the loaded release) and priority filter | ☐ `common.results.shown` announced ☐ `criteria.list.emptyFilter` |
| R-LST-07 | P0 | „Geprüft nach …“ cards for selected test types: priority B, deselectable, exported, short detail panel | CAT-16, CAT-17 (never counted) |
| R-LST-08 | P0 | Empty state for app types without criteria (`criteria.list.noCriteriaFor`) | CAT-20 |
| R-LST-09 | P0 | Notices: `criteria.list.priorityNote` always; `criteria.list.draftNote` when anonymous; `criteria.list.projectCopy` for a project list (no „Auswahl ändern“, no share button) | ☐ notices are not dismissible |
| R-LST-10 | P0 | Action bar: „Liste speichern“, „Liste exportieren“; sticky from 600 px width and height, in flow below | ☐ `scroll-padding-bottom` so focus is never covered (WCAG 2.4.11) |
| R-LST-11 | P0 | Export dialog S13: XLSX (default) or CSV, scope text, release note, anonymous note, legal note ⚖️ | ☐ initial focus on selected radio ☐ errors in the dialog with retry (`criteria.export.error.*`, `errors.rateLimit`, `criteria.export.outdated`) ☐ success announces `criteria.export.success` and closes |
| R-LST-12 | P0 | Anonymous export `POST /api/v1/exports` and saved export `POST /configurations/{id}/exports`; file format per §8.3 | EXP-01–EXP-07 ☐ rate limit per §7.3 |
| R-LST-13 | P0 | „Kriterium vorschlagen“ link at the end of the list (FA-15) | ☐ wraps as a link (`criteria.list.suggest`) |
| R-LST-14 | P0 | Older catalog release notice and „Neue Liste mit derselben Auswahl“ for saved organisation lists (D6) | CAT-22 · **Given** a saved list on release 2026.1 and active release 2026.2 **When** the user chooses `criteria.list.olderRelease.cta` **Then** a new organisation list on 2026.2 is created and opened, the old list is unchanged, and `criteria.list.copy.done` (plus `.dropped` / `.notCarried` where applicable) is shown and announced |

### FA-06 Saved lists, sharing, new list for a newer release (S12, S14, S15, S16)

Flow A2, `ARCH §5.3`, C58, C59.

**User stories**
- US-SAV-01 — As a buyer in an organisation, I want to save a list so that my team finds it again.
- US-SAV-02 — As a buyer, I want to share a list read-only with someone without an account (e.g. the legal department).
- US-SAV-03 — As a buyer, I want to revoke a share link.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-SAV-01 | P0 | Save dialog S12 with name (≤ 120) and organisation note | **Given** an anonymous user with a draft **When** they choose „Liste speichern“ **Then** sign-in opens with `auth.login.reasonSave`, and after sign-in the draft is still there and can be saved in one call · **Given** a user without organisation **Then** `criteria.save.needOrg` leads to S20 · Readers cannot save (403, button hidden) |
| R-SAV-02 | P0 | Saved lists S14: organisation lists only (project copies are not listed), sortable by name and date | ☐ `criteria.saved.empty` ☐ opening a list shows S11 with its name as H1 |
| R-SAV-03 | P0 | Editing a saved organisation list (include/exclude, priority, note) and „Auswahl ändern“ (creates the selection anew on the same list, criteria decisions carried over by key where still applicable) | ☐ shared workspace resources use ETag/If-Match (API-03); conflict → S37 |
| R-SAV-04 | P0 | Share link dialog S15: expiry date required, default today + 90 days, maximum today + 1 year (SD15) | TEN-08 ☐ token shown once, copy button, `criteria.share.copied` ☐ list of active links with revoke (`criteria.share.revoke`) ☐ project copies → no share action (409 from API) |
| R-SAV-05 | P0 | Shared list S16 `/geteilt/:token` without account | TEN-09 ☐ read-only S11 (no checkboxes, no K5 menu, no action bar) ☐ invalid, revoked or expired → `criteria.shared.invalid` (404) ☐ the shared DTO contains no organisation, user or project data |
| R-SAV-06 | P0 | Delete a saved organisation list (Owner, Editor) | ☐ confirmation dialog (new keys, §14) ☐ active share links stop working ☐ project copies are unaffected (they are independent, D1) |

### FA-07 Organisations and members (S20, S21)

**User stories**
- US-ORG-01 — As a buyer, I want to create an organisation for my department, so that my team works on the same lists and projects.
- US-ORG-02 — As an Owner, I want to invite colleagues by e-mail and set their role.
- US-ORG-03 — As an invited person, I want to accept an invitation with a new or an existing account.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-ORG-01 | P0 | Create organisation S20 (name ≤ 120, type: public body / company / association); creator becomes Owner | ☐ `org.start.contributeNote` explains that contributing needs no organisation ☐ requires a confirmed e-mail address |
| R-ORG-02 | P0 | Invitations by e-mail (`org.invite.*`), valid 14 days 🔧, single use, role chosen on invitation | **Given** an Owner invites x@y **Then** an e-mail with an invitation link is sent (§7.6) and S21 shows `org.invite.pending` · **When** the recipient opens the link signed out **Then** they sign in or register and, after confirming the e-mail address, accept · an invitation for an address that differs from the signed-in account's address is rejected with a clear message (new key, §14) |
| R-ORG-03 | P0 | Members page S21: list members with role; Owner changes roles, removes members, revokes pending invitations | ☐ the last Owner cannot be demoted or removed and cannot leave (409, message, new key) ☐ a removed member loses access immediately (next request) |
| R-ORG-04 | P0 | Leave an organisation (member action) | ☐ same last-Owner rule ☐ published entries of the member stay (they belong to the contributor profile, not to the membership) |
| R-ORG-05 | P0 | Authorisation matrix Owner/Editor/Reader for all workspace and configurator endpoints | TEN-01, TEN-02, TEN-05, TEN-06 · Reader: read everything of the organisation, change nothing; Editor: create and edit lists, projects, candidates, assessments, publish and withdraw organisation entries; Owner: Editor + members and organisation settings |
| R-ORG-06 | P0 | No join requests or organisation directory | ☐ joining only by invitation (`org.join.text`) |

### FA-08 Procurement projects (S30, S31, S32)

Flow B context, `ARCH §5.4`, C47, C51, C53; Handoff S32.

**User stories**
- US-PRJ-01 — As a buyer, I want a project per procurement with its own copy of a criteria list, so that changes in one project never affect another.
- US-PRJ-02 — As a buyer, I want an overview of all candidate products with test status, assessment progress and counts, so that I see where I stand without a ranking.
- US-PRJ-03 — As a buyer, I want to adjust the project's list after assessments started, without losing assessments.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-PRJ-01 | P0 | Project list S30: name, status, number of products, last change; sort by name/date; `projects.empty` | ☐ only projects of the active organisation (TEN-01) |
| R-PRJ-02 | P0 | Create project S31: name (≤ 120), optional organisation list → project gets its own copy (`projects.create.criteria.hint`) | WSP-02 ☐ project copies of other projects are not offered |
| R-PRJ-03 | P0 | Project overview S32 per Handoff §5.1 (criteria card, product list, add-product menu) | SIG-13 ☐ counts row `projects.detail.row.counts` with seven values, label first ☐ scope row names „Geprüft nach …“ separately (`.scope.value`) ☐ the product name is the only link to S34 ☐ `projects.detail.orderNote`; no sorting by counts |
| R-PRJ-04 | P0 | Assign or replace the project's list (`PUT …/criteria-configuration`) | WSP-02 · **Given** no candidate has an assessment ≠ „Nicht eingeschätzt“ **When** a list is assigned **Then** the previous copy is replaced · **Given** at least one such assessment **Then** replacing returns 409 and the UI shows `projects.detail.criteriaLocked` instead of a replace action |
| R-PRJ-05 | P0 | Edit project name and status (Laufend / Abgeschlossen / Archiviert) | ☐ archived projects are read-only for everyone and listed under a filter „Archivierte anzeigen“ (new key, §14) ☐ no deletion in v1 (§3) |
| R-PRJ-06 | P0 | Adjust the project copy via S11 (`projects.detail.criteria.adjust`) | WSP-03 ☐ selection layers are fixed (`criteria.list.projectCopy`) ☐ after adding criteria, `projects.detail.criteriaAdded` on return to S32 |
| R-PRJ-07 | P0 | Concurrency on project, copy, candidates | API-03 ☐ conflict → S37 |

### FA-09 Products in a project: evidence and assessment (S33, S34, S34a, S34b, S37)

`ARCH §5.4`, C46, C51, C65; Handoff S34, S34a, S34b, K6, `pa-rating-legend`.

**User stories**
- US-CAN-01 — As a buyer, I want to add a product from the directory or enter a new one, so that I can evaluate it in my project.
- US-CAN-02 — As a buyer, I want to record whether and how a product was tested, including several test evidences, so that „Geprüft nach …“ follows from facts.
- US-CAN-03 — As a buyer, I want to assess each criterion on the BIT inklusiv scale with a reason, saved automatically.
- US-CAN-04 — As a buyer, I want private notes that are never published.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-CAN-01 | P0 | Add from directory („Merken“): search like S40, choose a product → candidate with `SoftwareProductId`, name/vendor/website prefilled | ☐ `POST /projects/{id}/candidates { softwareProductId }` ☐ the same directory product can be added once per project (409 → message, new key) |
| R-CAN-02 | P0 | Create/edit candidate S33: name, vendor, website, functional summary, private section (contacts, internal notes) | PRV-01, PRV-02 ☐ name match hint `candidate.form.name.match` offers linking ☐ private section visually separated with `candidate.form.privateSection.text` |
| R-CAN-03 | P0 | Tabs as routes: Überblick (default), Prüfnachweis, Einschätzung nach Kriterien; short label below 600 px | ☐ each tab has its own page title `pageTitle.candidate` |
| R-CAN-04 | P0 | Overview S34 (Handoff §5.1) | ☐ facts only, no verdict ☐ K1 scope workspace with seven rows (SIG-13) ☐ publication card shows `candidate.published.*` or `candidate.overview.publication.none` ☐ `candidate.published.otherProfile` when the entry belongs to another profile (PUB-15) |
| R-CAN-05 | P0 | Test status tri-state (default „Nicht bekannt“) and evidence list S34a | WSP-01, SIG-03 (workspace part) ☐ evidence cards with H3, fields in fixed order, „Andere Prüfung“ requires a name ☐ duplicate procedure → `candidate.evidence.list.duplicate` ☐ switching away from „Ja“ with evidences asks `candidate.evidence.list.statusChange` ☐ a new product version updates the existing evidence of that procedure |
| R-CAN-06 | P0 | Assessment S34b: K6 per requirement criterion with allowed values only; autosave (debounce 800 ms); note per criterion (≤ 1 000) | CAT-15, CAT-17 ☐ `candidate.assessment.saved` max every 5 s ☐ default „Nicht eingeschätzt“; rows created lazily ☐ compact K6 below 600 px ☐ `candidate.assessment.unionNote` when applicable |
| R-CAN-07 | P0 | Derived „Geprüft nach …“ rows (read-only) with link to S34a | CAT-17 ☐ one row per selected procedure in catalog order ☐ never counted in progress or counts |
| R-CAN-08 | P0 | Progress, filter „Nur noch nicht eingeschätzte“, rating legend expanded until the first assessment | ☐ progress counts requirement criteria only („36 von 46“) ☐ legend state stored in settings (`withSettings()`) |
| R-CAN-09 | P0 (proposal O-S3) | Remove a candidate from a project (Owner, Editor) | ☐ confirmation dialog (new keys, §14) ☐ a published entry from it stays published; its `SourceCandidateProductId` becomes null (PRV-03); withdrawal is then only possible by the publishing profile (TEN-10) |
| R-CAN-10 | P0 | Readers see all tabs read-only (no inputs, no publish button) | TEN-05 |
| R-CAN-11 | P0 | Conflict dialog S37 on 409 for candidate, evidence or list edits; autosaved assessments use last-write-wins per criterion row (single-field rows, no If-Match) | API-03 ☐ S37 behaviour per Handoff §5.1 |

### FA-10 Publication from a project (S35, S36)

Flow B, `ARCH §4.4`, C54, C55, D16; Handoff S35.

**User stories**
- US-PUB-01 — As a buyer, I want to publish my assessment of a product as an independent entry under a pseudonymous profile, so that other organisations benefit without seeing my organisation.
- US-PUB-02 — As a buyer, I want to see exactly what will and will not be published before I confirm.
- US-PUB-03 — As a buyer, I want to update or withdraw my entry later.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-PUB-01 | P0 | Entry point „Im Produktverzeichnis veröffentlichen“ always enabled; hint `candidate.publish.cta.hint` only while test status is „Nicht bekannt“ | ☐ Editors and Owners only |
| R-PUB-02 | P0 | Blocked states instead of the stepper | **Given** test status „Nicht bekannt“ **When** S35 opens **Then** `publish.blocked.testStatus` with „Zum Prüfnachweis“ (focus to the radio group in S34a) · **Given** no criteria list **Then** `publish.blocked.noCriteria` with `projects.detail.noCriteria.cta` · server: SIG-10, PUB-14 (422) |
| R-PUB-03 | P0 | Step 1 „Produkt zuordnen“: trigram matches or new product (shared resolver); skipped when the candidate is already linked | PUB-07 |
| R-PUB-04 | P0 | Step 2 „Inhalt prüfen“: what is / is not published; link to edit assessments with return path | PRV-02 ☐ `publish.backToPublish` returns to step 2 with stepper state kept |
| R-PUB-05 | P0 | Step 3 „Unter welchem Namen?“: choose or create a contributor profile; channel note „Unabhängige Angaben“ | PUB-09 ☐ on update the profile is fixed (D3) |
| R-PUB-06 | P0 | Step 4 „Bestätigen“: confirmation checkbox, snapshot note; submit with `clientRequestId` | PUB-02, PUB-05, PUB-06, PUB-11, PUB-12 ☐ success page `publish.success.*` |
| R-PUB-07 | P0 | Update (republish) only under the original profile; other profile → hint + withdraw | PUB-15 · **Given** an entry published under profile P by user U1 **When** user U2 (Editor, without P) opens S34 **Then** `candidate.published.otherProfile` replaces „Eintrag aktualisieren“, and „Veröffentlichung zurückziehen“ is available |
| R-PUB-08 | P0 | Withdraw dialog S36 | TEN-10, MOD-04, MOD-06 ☐ withdrawn entries are never republished; the next publication creates a new entry ☐ `publish.withdraw.done` announced |
| R-PUB-09 | P0 | Changed-since-publication hint `candidate.published.changed` | ☐ shown when any published field of the candidate changed after `PublishedAt` (assessment, evidence, test status, name/vendor) |

### FA-11 Product directory search (S40)

Flow C, `ARCH §5.5`, §7.2; Handoff S40, `pa-product-result`.

**User stories**
- US-DIR-01 — As a buyer, I want to find products that others assessed for the same use case or app type.
- US-DIR-02 — As a buyer, I want to filter by test procedure and by origin of the statements.
- US-DIR-03 — As a buyer, I want results that are clearly not a ranking.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-DIR-01 | P0 | Search field (name or vendor), debounce 300 ms, previous request cancelled | ☐ `directory.results` visible and announced (polite, throttled) |
| R-DIR-02 | P0 | Filters in layer order: use case (single, searchable), app types (multi), test procedures (multi, incl. „Andere Prüfung“ / „Eigene Prüfung …“), origin (2 checkboxes), „Mit Konformitätsbericht“ | DIR-01, DIR-02 ☐ exclusions as in S10 (`directory.filter.reset` when a higher filter invalidates a lower one) ☐ below 840 px container width inside the disclosure `directory.filter.toggle` |
| R-DIR-03 | P0 | Filters and sort reflected in the URL query (shareable, back button works) | ☐ reload restores the same results |
| R-DIR-04 | P0 | Sort: name (default), last published, number of entries — nothing else | DIR-02 ☐ `directory.results.orderNote` always; with a use case selected combined with `directory.results.useCaseNote` in one notice (S40-V) |
| R-DIR-05 | P0 | Result item `pa-product-result`: facts only | ☐ no tested ratio, no fulfilment number, no badges (Handoff §4.9) |
| R-DIR-06 | P0 | Paging 20 per page with `mat-paginator` only above 20 results | ☐ offset paging (`ARCH §7.1`) |
| R-DIR-07 | P0 | Empty state with `directory.empty.suggest` → S52 (sign-in required there) | |

### FA-12 Product page and entry detail (S41, S42)

Flow C, `ARCH §4.4` (two signals), §7.3; Handoff S41, S42, K1–K4, `pa-entry-row`, `pa-entry-card`.

**User stories**
- US-ENT-01 — As a buyer, I want to see vendor statements and independent statements separately, each with tested status, evidence and fulfilment counts, without an overall verdict.
- US-ENT-02 — As a buyer with a saved list, I want to compare each entry against my own list.
- US-ENT-03 — As a buyer, I want to read individual experience reports with the assistive technology used.
- US-ENT-04 — As a buyer, I want to see the full assessment per criterion of one entry and earlier versions.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-ENT-01 | P0 | Product page S41: header facts, `directory.product.noVerdict`, two channel sections (H2) that never merge and never disappear | SIG-04, SIG-09 ☐ empty section keeps its H2 with the empty text |
| R-ENT-02 | P0 | Entry rows and expandable cards; K4 always for test-result entries; K2 and K1 in separate regions | SIG-03, SIG-07, SIG-08, SIG-09, SIG-11, CAT-04 ☐ notCovered tag always shown, also at 0 |
| R-ENT-03 | P0 | Vendor entry without assessments shows `signals.fulfillment.none` in row and card | PUB-14 |
| R-ENT-04 | P0 | Compare select (signed in with organisation): „Keine“ plus the organisation's saved lists and the project copies of its active projects; project copies labelled with the project name (new key, §14) | TEN-03 ☐ changing it reloads entries with `configurationId`, keeps focus, announces `directory.product.compare.updated` / `.off` ☐ anonymous users see `directory.product.compare.login`; signed-in users without organisation see nothing |
| R-ENT-05 | P0 | Sort of entries: last published (default), name, number — never by matching | DIR-02 rule applied to entries ☐ `directory.sort.updated` |
| R-ENT-06 | P0 | Experience reports only under „Erfahrungsberichte“: report card with `contribute.report.typeNote`, context line (app types only), optional list of assessed criteria — no K1, no K4, no counts | SIG-06, SIG-12, SIG-14 |
| R-ENT-07 | P0 | Staleness note `directory.entry.stale` for entries whose current version is older than **18 months** 🔧 (SD7) | ☐ measured from `PublishedAt` of the current version |
| R-ENT-08 | P0 | Context line `directory.entry.context` with source suffix | PUB-11 |
| R-ENT-09 | P0 | Entry detail S42: full card, table „Einschätzung je Kriterium“ in catalog order with filter, rating legend, previous versions | ☐ public view: projection against the entry's own scope ☐ with a comparison list: table limited to the list's criteria plus „Damals nicht im Katalog“ rows ☐ without assessments (D2): section omitted ☐ previous versions at `/eintraege/:id?fassung=<n>` with a notice that a newer version exists (new key, §14) |
| R-ENT-10 | P0 | „Teilen“ of a product = the public product URL; the page offers a copy-link button (new key, §14) | ☐ no account needed to open the link |
| R-ENT-11 | P0 | „Merken“ of a product = add to a project (FA-09 R-CAN-01); offered on S41 as „Zu einem Projekt hinzufügen“ for Editors/Owners (new key, §14) | ☐ project chooser lists active projects of the active organisation |

### FA-13 Contributor profiles, experience reports, my contributions (S50–S53)

Flow D, `ARCH §4.5`, C48, C56, C57; Handoff S51, S53.

**User stories**
- US-CTR-01 — As a person with a disability, I want to write an experience report under a pseudonym, without naming my disability or my employer.
- US-CTR-02 — As a contributor, I want to state how I used the software and, optionally, assess single criteria.
- US-CTR-03 — As a contributor, I want to withdraw my report at any time, and have my text and assistive-technology details deleted.
- US-CTR-04 — As a contributor, I want to propose a product that is not yet in the directory.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-CTR-01 | P0 | Create contributor profile S50: display name (≤ 60, unique case-insensitively across all profiles), kind (individual / institution / vendor / auditor) | PRV-04 ☐ hint `contribute.profile.name.hint` ☐ a user may hold several profiles; profiles are never linked publicly ☐ requires a confirmed e-mail address; no organisation needed (PUB-04) |
| R-CTR-02 | P0 | Report form S51 (Handoff §5.1) | PUB-03, PUB-16, PRV-09 ☐ narrative required (≤ 5 000) ☐ AT context and version optional (each ≤ 200) ☐ app types optional, always offered ☐ consent checkbox always shown, required, consent-text version sent ⚖️ ☐ validation on submit with error summary (§7.1) ☐ `clientRequestId` ☐ rate limit (§7.3) |
| R-CTR-03 | P0 | Optional single-criteria assessment inside S51: K6 list of the requirement criteria applicable to the stated app types (without a statement: all app types, all six levels), across all product parts, grouped by product part and searchable (no product-part selection on this path, `ARCH §4.5`) | CAT-15 ☐ only assessed criteria are stored ☐ `contribute.report.assessment.note` |
| R-CTR-04 | P0 | Report only for existing products; otherwise product request S52 | PUB-03 ☐ S52: name, vendor, optional website; confirmation `contribute.productRequest.sent` with **5 working days** 🔧 ☐ the requester gets an e-mail when the request is accepted (link to S51 for that product) or rejected (with reason) |
| R-CTR-05 | P0 | My contributions S53: all contributions of the account across its profiles, with status, view, withdraw; „Neues Prüfergebnis veröffentlichen“ for test results of vendor/auditor profiles | TEN-10 ☐ withdrawn items stay with status and date ☐ also lists entries published from projects under the user's profiles (kind „Eintrag aus einem Beschaffungsprojekt“) |
| R-CTR-06 | P0 | Withdrawing a report = withdrawing consent → `ContentPurge` | PRV-10 · **Given** a visible report **When** the author withdraws it with `contribute.mine.withdraw.reportNote` **Then** it disappears from the directory, its narrative, AT context, version and assessment notes are null on all versions in the same transaction 🔧 (timing ⚖️, default immediately), and a tombstone remains |

### FA-14 Test-result publication by vendors and auditors (S55)

Flow E, `ARCH §4.6`, C54, C55, D16; Handoff S55.

**User stories**
- US-VEN-01 — As a vendor or test lab, I want to publish a test result with the commissioning party's consent, without an organisation or project.
- US-VEN-02 — As a vendor, I want to state app types and use cases so that buyers find the product through the filters.
- US-VEN-03 — As a vendor, I want to publish a new test result for the same product later.

**Requirements**

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-VEN-01 | P0 | Five-step stepper (Produkt, Prüfung, Kriterien (optional), Zustimmung, Vorschau und Veröffentlichen); only for profiles of kind vendor or auditor; otherwise profile creation first | PUB-04, PUB-08 ☐ step change focuses the step heading and updates `pageTitle.vendor.step` |
| R-VEN-02 | P0 | Step 1: product name → matches (radio) → vendor, website → use cases (chips, optional) → app types (required, legend in group) → context hint | PUB-07, PUB-11 ☐ exclusions apply like S10 ☐ secondary action „Abbrechen“ |
| R-VEN-03 | P0 | Step 2: tested yes/no; with yes ≥ 1 evidence card (one per procedure), date not in the future | PUB-02 ☐ `vendor.step2.rule` |
| R-VEN-04 | P0 | Step 3 optional: product parts + K6 per applicable criterion; skip = no assessments | PUB-12, PUB-14 · **Given** the user skips step 3 **Then** the published entry has no assessment rows and shows `signals.fulfillment.none` · **Given** at least one assessment **Then** product parts are required and all criteria in scope are stored (`Unknown` where empty) |
| R-VEN-05 | P0 | Step 4 consent ⚖️ (given by, on behalf of, date, how, confirmation) — never prefilled on republish | PUB-05, PUB-08, PRV-08 |
| R-VEN-06 | P0 | Step 5 preview with the real entry card (K1–K4), profile choice, submit with `clientRequestId` | PUB-06 ☐ `vendor.success` |
| R-VEN-07 | P0 | Republish from S53 → S55 in update mode, prefilled except consent; same profile only; withdrawn entries cannot be republished | PUB-05, PUB-15, MOD-06 |
| R-VEN-08 | P1 | Conformance report upload (VPAT/ACR) in step 2 | FA-18 |

### FA-15 Criteria suggestion via external channel (S70)

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-SUG-01 | P0 | „Kriterium vorschlagen“ in the footer and at the end of S11 opens the configured channel (`Contact:CriteriaSuggestion`, 🔧 placeholder until O-6 is decided) | ☐ supports `mailto:` (subject `suggest.mailSubject`, body `suggest.mailBody` with the criterion key when opened from a criterion) or an https URL ☐ hidden suffix `suggest.external` matches the channel type ☐ no data model, no endpoint (`ARCH §16.4.2`) ☐ production build fails if the value is still the placeholder (like R-SHL-07) |

### FA-16 Usage events for the success metrics

See §9 for the event list. Requirement:

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-MET-01 | P0 | Write exactly the events of §9.1 into `analytics.UsageEvent` with date granularity and no personal reference | PRV-05 ☐ no third-party analytics, no client-side tracking script ☐ writing an event never fails the user's request (fire-and-forget with error log) |
| R-MET-02 | P0 | The indicators of §9.2 are reproducible by documented SQL queries in `TECHNICAL.md` | ☐ queries return aggregates only |

### FA-17 Operation and administration (proposal O-S1, O-S2)

The handoff and the UX copy contain no administration screens. Platform roles exist (`ARCH §9.1`). Proposal for v1, chosen for a solo maintainer:

| ID | Prio | Requirement | Acceptance criteria |
|---|---|---|---|
| R-ADM-01 | P0 | **Catalog releases from data files** (O-S1): each release is a folder in the repository (`catalog/releases/<version>/`, JSON) with criteria revisions, procedure texts, applicability, priorities, exclusions, procedure mapping; a CLI command `catalog import <version>` validates and imports it as an immutable release; `catalog activate <version>` sets the active release | CAT-05, CAT-06, CAT-07, CAT-13, CAT-19, CAT-21 ☐ import fails on any validation error (no partial release) ☐ the release folder is the review artefact for the project team (diff in a pull request) ☐ release 2026.1 is produced from Kriterienkatalog Teil A/B plus the priority document (O-1) |
| R-ADM-02 | P0 | **Product identity administration** (O-S2) for `PlatformAdmin`: a minimal list of open product-identity requests (accept → creates the product and notifies the requester; reject with reason) and a merge action for duplicates (`MergedIntoId`) | PUB-07 ☐ built from Angular Material defaults (Handoff §5.3), German strings to be added to the UX copy (§14) ☐ every action is logged with user and timestamp |
| R-ADM-03 | P0 | Role assignment for platform roles via a CLI command (no UI) | TEN-06 |
| R-ADM-04 | P0 | Operations per `ARCH §9.5`, §10: health endpoints, log retention, nightly backup, restore rehearsal documented | OPS-01, OPS-02 ☐ `RebuildDatabase` guard (SEC-06) |

### FA-18 P1 outline (detailed in SPEC_v2)

| ID | Prio | Feature | Outline | Depends on |
|---|---|---|---|---|
| R-P1-01 | P1 | Report an entry and moderation | Any signed-in user reports an entry (reason, description, version shown); `Moderator` reviews: restrict, withdraw, reinstate; append-only log; reports rate-limited | MOD-01–06, ARC-04; moderation policy and legal review (O-4) |
| R-P1-02 | P1 | Counter-statement | Vendor/auditor profiles post a statement shown alongside the entry, never replacing it | MOD-05, TEN-07 |
| R-P1-03 | P1 | Conformance-report upload in S55 | PDF/ODF/OOXML, size cap, served as attachment from a separate origin; K2 shows format, date, age and link | SEC-04, SEC-05, D14 |
| R-P1-04 | P1 | WERTE.IT advice note | `projects.detail.noResults.werteit` and `directory.empty.werteit` with a configurable link | contact details from WERTE.IT |
| R-P1-05 | P1 | Links to external product directories in the workspace flow | List of external directories (configurable), shown in the add-product menu | selection of directories by the project team |
| R-P1-06 | P1 | „Hoher Kontrast“ setting | see R-SET-04 | H2 |

---

## 7. Cross-cutting requirements

### 7.1 Validation and errors

- Validation on submit (format errors such as dates also on blur, silently). Error summary as focus target, field errors identical to the summary lines (UX copy §6.1, E11, E12; Handoff §6.4).
- All server validation errors are Problem Details with an `errors` map keyed by field path (API-01, API-02). Problem `type` URIs are fixed in §8.6.
- Client and server enforce the same limits (§7.2); the server is authoritative.

### 7.2 Field limits (SD9; closes H6)

| Field | Max | Notes |
|---|---|---|
| Display name (contributor profile) | 60 | unique, case-insensitive; counter hint from 50 |
| Experience report narrative | 5 000 | counter from 4 500, announced only on blur |
| AT context, product version used (report) | 200 each | |
| Criterion note, assessment note, evidence fields (tester, version, other procedure name) | 1 000 / 200 / 200 / 120 | note also in the anonymous export (`ARCH §9.2`) |
| Project name, list name, organisation name | 120 | |
| Candidate name, vendor | 200 | |
| Functional summary, internal notes, contact notes | 2 000 each | private, never published |
| URLs (website, evidence, report link) | 2 048, `https://` only | `errors.field.url` |
| Consent fields (Flow E: given by, on behalf of, how) | 200 each | |
| Anonymous export request | criteria array ≤ number of criteria in the release; body ≤ 1 MB | EXP-07 |

### 7.3 Rate limits (🔧 defaults, configurable; UX-O11 stays open until the pilot)

| Policy | Partition | Default |
|---|---|---|
| Anonymous export | client IP (memory only) | 30 per 10 minutes |
| Saved export | user | 60 per 10 minutes |
| Registration | IP | 5 per hour |
| Password reset request | IP and e-mail address | 5 per hour per IP, 3 per hour per address |
| Confirmation e-mail resend, invitation | user | 5 per hour |
| Workspace publication, vendor publication | user | 20 per hour |
| Experience report | user | 5 per hour, 20 per day |
| Product-identity request | user | 5 per day |
| Reports, counter-statements (P1) | user | defined in SPEC_v2 |

All return 429 with Problem Details and a `retryAfterMinutes` extension used by `errors.rateLimit` / `contribute.report.rateLimited` (API-05).

### 7.4 Accessibility, mobile-first and browsers

- Target: WCAG 2.1 AA / EN 301 549 / BITV 2.0 for the tool and its exports (PRD §5 goal 5); WCAG 2.2 AA rules reported, not blocking (TESTING §3.3).
- Mobile-first per Handoff §2; 360 px base, 320 px / 400 % zoom without horizontal page scrolling; touch targets ≥ 48 × 48 px.
- Automated gates A11Y-01–06; manual passes and release gate A11Y-07 (TESTING §7.2).
- **Supported browsers (proposal O-S4):** the two latest major versions of Chrome, Edge, Firefox and Safari on desktop; Safari on iOS and Chrome on Android, current and previous major version. Screen-reader combinations as fixed with the accessibility tester (TESTING §7.2). No support for Internet Explorer or legacy Edge.

### 7.5 Appearance: light and dark theme (SD11)

- Both appearances are part of v1. Handoff §1.1 („do not ship a dark theme in v1“) and H3 are superseded by this decision.
- Implementation: `html { color-scheme: light dark; }`; `mat.theme` with the same palettes in a dark variant; the active scheme is set by a class on `html` (`theme-light`, `theme-dark`) derived from the user setting or `prefers-color-scheme` (R-SET-02). Only `--mat-sys-*` and `--pa-*` tokens are used, never hex values (Handoff §1.2), so components need no per-theme code.
- Rules that apply in both appearances: no product-related colour coding; both channels identical; text that carries a rule ≥ body/medium; focus ring ≥ 3 : 1 against its background; all text ≥ 4.5 : 1.
- Prerequisites before release (O-S5): dark mode for the Figma variables and the screens of the Handoff; an accessibility review of the dark scheme (contrast of every role used in Handoff §1.2, focus ring, `outline-variant` remains decorative only). Development may start with the generated dark palette; values are pinned with `mat.theme-overrides` after the review (like H1).
- Exports (XLSX/CSV) and e-mails are not themed.

### 7.6 E-mail (SD13)

| Aspect | Specification |
|---|---|
| Provider | Lettermint (EU-hosted transactional e-mail), via its **SMTP relay** with TLS; credentials only from environment variables (`Email__Smtp__Host`, `__Port`, `__Username`, `__Password`; Lettermint uses its API token as password). SMTP keeps the code provider-neutral for a successor organisation (`ARCH §2.2` handover) |
| Code | `IEmailSender` in Platform; implementation with MailKit (MIT); one template per message type (plain text + simple accessible HTML, German), rendered on the server |
| Privacy | open and click tracking switched off in the Lettermint project; no tracking pixels, no rewritten links; e-mails contain no data beyond what the recipient needs; recipient addresses are not written to logs above `Debug` |
| Message types (v1) | address confirmation; password reset; „already registered“ notice (R-AUTH-01); organisation invitation; product-identity request accepted / rejected |
| Delivery | sent synchronously within the request with one retry; on failure the user sees a notice with retry (new key, §14) and the action itself (e.g. invitation record) is kept |
| Environments | Development and E2E use a local SMTP catcher (Mailpit, MIT) in docker-compose; Staging and Production use Lettermint with separate API tokens/projects |
| Sender | configurable sender address and name (`Email__From`); domain set up with SPF/DKIM in Lettermint (operations, `TECHNICAL.md`) |
| Texts | e-mail texts are UI texts: new keys `email.*` in the UX copy (§14) |

Architecture follow-up: the e-mail dependency is not yet in `ARCHITECTURE_v10` (§14).

### 7.7 Privacy

`ARCH §9.3` applies. In addition:
- No external fonts, scripts or CDNs (Handoff §1.3, §1.8).
- Personal data export: §8.4. Account deletion: R-AUTH-10.
- No `UsageEvent` carries personal reference (PRV-05).
- E-mail: §7.6.

### 7.8 Security

`ARCH §9.1`, §9.2, §9.7 apply; SEC-01–SEC-09. Additional: password policy and lockout per R-AUTH-01/03; all links in e-mails are single-use tokens with expiry; tokens stored hashed.

---

## 8. API and file contracts decided in this spec

Endpoints not listed here are as in `ARCH §7.2`. JSON uses camelCase; enums are serialised as camelCase strings.

### 8.1 Filter options (closes O-8)

```jsonc
// GET /api/v1/catalog/filter-options?release=2026.1&useCase=hr&appTypeIds[]=web&productPartIds[]=base
{
  "release": "2026.1",
  "layers": {
    "useCases":     [ { "id": "hr", "name": "HR-Anwendung", "hint": null, "excludedBy": [] }, … ],
    "appTypes":     [ { "id": "document", "name": "Dokument", "hint": null,
                        "excludedBy": [ { "layer": "useCase", "id": "hr", "name": "HR-Anwendung" } ] }, … ],
    "productParts": [ { "id": "base", "name": "Basisfunktionalität", "explanation": "…",
                        "alwaysSelected": true, "excludedBy": [] }, … ],
    "testTypes":    [ { "id": "pdfTest", "name": "BIT inklusiv PDF-Test", "url": "https://…",
                        "excludedBy": [ { "layer": "useCase", "id": "hr", "name": "HR-Anwendung" },
                                        { "layer": "appType", "id": "web", "name": "Website oder Webanwendung" } ] }, … ]
  }
}
```

- `excludedBy[]` lists every currently selected higher-layer value that excludes the option (empty = available). `layer` ∈ `useCase | appType | productPart`. No reason text is sent; the client builds the reason from `criteria.select.excluded.*` (one source for wording). The `ReasonDe` column stays for catalog documentation.
- Query parameters are optional; without them every `excludedBy` is empty. Inactive use cases are omitted (CAT-18).

### 8.2 Criteria for a selection

```jsonc
// GET /api/v1/catalog/criteria?release=…&useCase=…&appTypeIds[]=…&productPartIds[]=…&testTypeIds[]=…
{
  "release": "2026.1",
  "criteria": [ { "revisionId": 101, "key": "BITV-SW-11.2.1.1", "kind": "requirement", "number": "11.2.1.1",
                  "title": "…", "plainLanguage": "…", "productPart": "base", "standardReferences": [ … ],
                  "allowedRatings": [ "fulfilled", "restriction", "barrier", "blockade" ],
                  "procedureTexts": [ { "testType": "softwareTest", … } ],
                  "resolvedPriority": { "level": "A", "weight": null, "rationale": "…",
                                        "source": "expertJudgement",
                                        "matchedRule": { "kind": "appTypeAndUseCase", "appType": "web", "useCase": "hr" } } } ],
  "appTypesWithoutCriteria": [ { "id": "hardware", "name": "Hardware" } ]
}
```

- `appTypesWithoutCriteria[]` contains the selected app types for which the release has no applicable requirement criterion (today Hardware, Vermittlungs-/Notrufdienst); it may be non-empty while `criteria` is not empty. `matchedRule.kind` ∈ `appTypeAndUseCase | appType | useCase | default` (maps to `criteria.priority.rule.*`). `allowedRatings` never contains `unknown` (always allowed).

### 8.3 Export file (XLSX and CSV)

- One data sheet „Kriterien“ (defined name `Kriterien` over the data range, header row frozen) and one sheet „Info“.
- **Columns, in this order** (German headers; export language = UI language): Nr. (running) · Kriterium (key) · Nummer (EN 301 549 / BIT inklusiv) · Titel · Originaltitel · Kurzbeschreibung · Funktionsbereich · Priorität (empfohlen) · Priorität (Ihre Wahl) · Gewichtung · Begründung der Empfehlung · Ihre Notiz · Link zum Prüfschritt.
- Only included criteria; „Geprüft nach …“ rows with key `PROC-<testType>`, empty number and original title.
- Priority labels are the provisional terms of SD6 (O-14).
- „Info“ sheet: tool name and URL, generation date, catalog release, the four selection layers, number of criteria per priority, the notes `criteria.list.priorityNote` and `criteria.export.legalNote` ⚖️, and the source line for BIT inklusiv texts.
- CSV: same columns, UTF-8 with BOM, **semicolon** separator (German spreadsheet default), CRLF, RFC 4180 quoting; the „Info“ content is not part of the CSV, except the catalog release in an extra first column „Katalogversion“ (EXP-03).
- File name: `ProcureAccess_Kriterien_<release>_<YYYY-MM-DD>.xlsx|.csv`; saved lists additionally include a slug of the list name.
- Accessibility per EXP-02.

### 8.4 Personal data export (SD14)

`GET /api/v1/account/export` → `application/json`, `Content-Disposition: attachment; filename="ProcureAccess_Daten_<YYYY-MM-DD>.json"`.

```jsonc
{
  "generatedAt": "…", "formatVersion": 1,
  "account": { "email": "…", "createdAt": "…", "emailConfirmed": true, "preferences": { … } },
  "memberships": [ { "organization": "…", "role": "editor", "since": "…" } ],
  "contributorProfiles": [ { "displayName": "…", "kind": "individual", "createdAt": "…" } ],
  "contributions": [ { "product": "…", "kind": "experienceReport", "state": "published",
                       "versions": [ { "versionNumber": 1, "publishedAt": "…", "content": { … } } ] } ],
  "createdByMe": { "criteriaLists": [ … ], "projects": [ { "name": "…", "candidates": [ … ] } ] }
}
```

- `createdByMe` contains lists and projects whose `CreatedByUserId` is the user, with their current content (including private fields of those projects, which the user can read anyway). The UX text `account.export.text` is adjusted accordingly (§14).
- Purged content appears as null. Consent records of Flow E versions published by the user are included (they are the user's own statements).

### 8.5 Shared list

`GET /api/v1/shared/{token}` → `{ name, release, selection { useCase, appTypes[], productParts[], testTypes[] }, criteria [ { …as §8.2, isIncluded, effectiveLevel, recommendedLevel, weight, note } ], expiresAt }` — no organisation, user, project or ids of the source configuration (TEN-08).

### 8.6 Problem types

`https://procureaccess.de/problems/<name>` 🔧 (base URI configurable): `validation`, `antiforgery`, `rate-limit`, `conflict` (409 with `editorDisplayName`), `locked-criteria-list`, `test-status-unknown`, `no-criteria-list`, `profile-mismatch`, `entry-withdrawn`, `selection-excluded`, `release-mismatch`, `not-found`. The client maps them to the UX keys named in the feature areas.

---

## 9. Success metrics and usage events (SD5)

### 9.1 Usage events (final list for v1)

| Event | When | Dimension 1 | Dimension 2 |
|---|---|---|---|
| `CriteriaListShown` | S11 shown for a complete selection (once per selection, not on re-render) | `anonymous` / `signedIn` | – |
| `ExportCreated` | export file generated successfully | `xlsx` / `csv` | `anonymous` / `signedIn` |

Stored with date only (`ARCH §5.7`). No other events in v1.

### 9.2 Indicators (PRD §10) and their source

| Indicator | Type | Source |
|---|---|---|
| Completed criteria configurations | leading | `CriteriaListShown` per month, split by dimension 1 |
| Exports with and without account | leading | `ExportCreated` per month and dimension |
| Saved lists | leading | count of `CriteriaConfiguration` without `ProcurementProjectId` |
| Workspace entries | leading | count of `CandidateProduct` |
| Published vs. private workspace entries | leading | candidates with a visible `WorkspacePublication` entry vs. all candidates |
| Growth of Kanal-1 and Kanal-2 entries | lagging | `PublishedEntry` by channel and origin per month (`FirstPublishedAt`) |
| Pilot procurements | lagging | recorded manually by the project team |
| Expert/pilot feedback on prioritisation | lagging | qualitative; supported by stored `ResolvedLevel` vs. overrides of saved lists (aggregated per criterion) |

No target values in v1; the project team sets them after the first pilot. Queries live in `TECHNICAL.md` (R-MET-02).

---

## 10. Traceability: PRD requirements → feature areas

| PRD §9 requirement | Prio | Feature area / requirement |
|---|---|---|
| Auswahl des Verwendungszwecks | P0 | R-CON-02 |
| Auswahl des Anwendungstyps | P0 | R-CON-03 |
| Auswahl der Funktionsbereiche | P0 | R-CON-04 |
| Auswahl von Prüfverfahren | P0 | R-CON-05, R-LST-07 |
| Ausschlussregeln | P0 | R-CON-06, R-CON-07, R-DIR-02, R-VEN-02 |
| Kriterien in verständlicher Sprache | P0 | R-LST-01, R-ADM-01 (content: O-1) |
| Gewichtungen/Priorisierungen | P0 | R-LST-02, R-LST-03 (content: O-1) |
| Export als Excel-Datei | P0 | R-LST-11, R-LST-12, §8.3 |
| Workspace: Produkte anlegen und einschätzen | P0 | FA-08, FA-09 |
| Optionale Veröffentlichung (Kanal 2) | P0 | FA-10 |
| Direkte Veröffentlichung für Anbieter/Prüfstellen | P0 | FA-14 |
| Basisfeld „Auf Barrierefreiheit geprüft“ | P0 | R-ENT-02, R-CAN-05, R-PUB-02 |
| Getrennte Signale | P0 | R-ENT-02, R-ENT-03 |
| Filterung im Produktverzeichnis | P0 | FA-11 |
| Pseudonyme Teilnahme | P0 | FA-13 |
| Merken und Teilen; Vorschläge zum Katalog | P0 | FA-06 (lists), R-ENT-10, R-ENT-11, R-CAN-01 (products), FA-15 |
| Barrierefreiheit des Tools | P0 | §7.4, FA-01, FA-03 |
| Persönliche Einstellungen | P0 | FA-03 |
| VPAT/ACR als Einreichungsformat | P1 | R-P1-03 |
| Verlinkung externer Produktverzeichnisse | P1 | R-P1-05 |
| Verweis auf WERTE.IT-Beratung | P1 | R-P1-04 |
| Basis-Moderation | P1 | R-P1-01, R-P1-02 |
| P2 items (Bewertungsmodelle, Vergabetext-Generator, Marktlagen-Übersicht, Import, MLBF, BIK Web, weitere Verwendungszwecke) | P2 | not specified; the design keeps them possible (`ARCH §15`) |

Not from PRD §9 but needed: FA-02 (accounts, session, e-mail), FA-07 (organisations), FA-16 (metrics), FA-17 (operation).

## 11. Mapping to migration steps and invariants

| Step (`ARCH §12`) | Feature areas / requirements delivered | Invariants activated (TESTING §9) |
|---|---|---|
| 0 Platform upgrade, test foundation | – (no user-visible change) | ARC-06, OPS-03, CAT-10, A11Y-01 |
| 1 Foundations | R-SHL-01, -04, -08, -09; R-SET-02 (system preferences incl. dark scheme scaffolding); R-MET-01 (table) ; §7.1 | ARC-01–03, API-01/02, OPS-01/02, SEC-06, PRV-05, A11Y-03/05 |
| 2 Catalog versioning and export | FA-04, FA-05 (anonymous part), FA-15, R-ADM-01 | CAT-*, CFG-*, EXP-01–05, -07, TEN-04, API-05/06 |
| 3 Identity, tenancy, session, saved lists | FA-02 (except R-AUTH-06), FA-06, FA-07, R-SET-01, R-SHL-02/03, R-ADM-03, §7.6 | TEN-01/02/05/06/08/09, SEC-01–03, SEC-09, EXP-06, CAT-22, OPS-04, A11Y-06 |
| 4 Session warning and hardening | R-AUTH-06, R-AUTH-07 completion | SEC-07, SEC-08, A11Y-04 |
| 5 Workspace | FA-08, FA-09 | WSP-01–03, SIG-13, API-03/04, ARC-05 |
| 6 Directory, publication, contribution | FA-10, FA-11, FA-12, FA-13, FA-14 (except P1), R-ADM-02 | SIG-*, PUB-*, PRV-*, DIR-*, TEN-03/10, ARC-07 |
| 7 Moderation and P1 | FA-18 (after SPEC_v2) | MOD-*, TEN-07, SEC-04/05, ARC-04 |

Legal pages (R-SHL-07), the dark theme release check (R-SET-03, O-S5) and the accessibility statement content are release prerequisites, not tied to a step.

## 12. Timeline and phasing (SD18)

- No external deadline and no pilot date yet. Phasing follows the migration steps (§11); a working application exists after each step.
- **First pilot-ready milestone:** end of step 2 (anonymous configurator and export) — the earliest point where a cooperation partner can try the core flow. Requires O-1 (priorities) and K-1 (plain-language titles) for at least the criteria of the pilot's use case.
- **Pilot milestone (PRD §5 goal 4):** end of step 5 plus staging environment; requires O-S5 (dark theme review), H2 (target), legal pages, A11Y-07 pass.
- **Public release:** after step 6 and the bundled legal review (O-4).
- SPEC_v2: at the start of sprint planning for step 7 (see "Versioning of this document").

## 13. Open questions

**Blocking** = must be answered before the named step can be completed.

| # | Question | Blocks | Decides | Source |
|---|---|---|---|---|
| O-1 | Priorities and rationales per criterion, use case and app type; plain-language titles and summaries (K-1, K-6) | step 2 (content of release 2026.1) | Project team | ARCH O-1 |
| O-2 | Hardware and Vermittlungs-/Notrufdienst without a procedure | — (empty state exists) | Project team | ARCH O-2 |
| O-4 | Bundled legal review (consent texts ⚖️, purge timing, erasure vs. moderation record, vendor authenticity, export legal note, account deletion) | public release | Legal review | ARCH O-4 |
| O-6 | Contact address or form for „Kriterium vorschlagen“ | production build (R-SUG-01) | Project team | ARCH O-6 |
| O-7 | Handover target platform | — | WERTE.IT | ARCH O-7 |
| O-13 | Carrying assessments over to a newer release | — (later) | Owner | ARCH O-13 |
| O-14 | Priority terms A / B / Hinweis (provisional, SD6) | export column values before the pilot | Project team (accessibility expertise) | ARCH O-14 |
| O-16 | Rating-level explanations (provisional, SD8) | before the pilot | Project team, possibly BIT inklusiv | ARCH O-16 |
| UX-O9 | Pluralisation approach in ngx-translate | step 1 | Implementation | UX copy §8 |
| UX-O11 | Final rate limit of the anonymous export (§7.3 defaults) | pilot | Implementation with pilot feedback | UX copy §8 |
| H2 | High-contrast variant designed and reviewed | „Hoher Kontrast“ setting (P1) | Philipp | Handoff §9 |
| O-S1 | Catalog releases from data files with CLI import, no editor UI (R-ADM-01) — confirm | step 2 | Owner | this spec |
| O-S2 | Minimal admin screens for product-identity requests and merges (R-ADM-02) — confirm | step 6 | Owner | this spec |
| O-S3 | Removing a candidate from a project (R-CAN-09) — confirm | step 5 | Owner | this spec |
| O-S4 | Supported browsers (§7.4) — confirm | step 1 (CI browser matrix) | Owner | this spec |
| O-S5 | Dark theme: Figma dark mode and accessibility review (§7.5) | release (development may start) | Philipp, accessibility tester | SD11 |
| O-S6 | Default values marked 🔧 (password length, token lifetimes, invitation validity, request turnaround 5 working days, staleness 18 months, rate limits) — confirm or adjust | before the pilot | Owner / project team | this spec |

Closed by this spec: O-8 (§8.1), O-15 (SD7), H3 (SD11), H6 (SD9), UX-O8 (SD7).

## 14. Follow-ups for other documents

| Document | Change |
|---|---|
| `ARCHITECTURE_v11.md` | e-mail dependency (`IEmailSender`, Lettermint SMTP, Mailpit; §2.1, §4.3 Platform, §10.1); dark theme (supersedes Handoff note); e-mail confirmation and lockout (§9.1); catalog import CLI (O-S1); admin endpoints for product-identity requests and merge (O-S2); candidate deletion (O-S3); data-export endpoint shape (§8.4); problem types (§8.6); `excludedBy[]` shape (§8.1, O-8 closed); O-15 closed |
| `TESTING_v9.md` | axe runs in both appearances; invariants for e-mail confirmation gate, lockout, last-Owner rule, account deletion refusal, share-link expiry bounds, staleness threshold, candidate deletion (published entry stays), catalog import atomicity; E2E uses Mailpit for J1/J2 sign-up flows |
| `ux_copy_procureaccess_v13.md` | new keys: `email.*` (all message types), appearance setting, lockout hint, unconfirmed-address notice and resend, e-mail send failure, invitation address mismatch, last-Owner messages, account deletion refusal, list deletion dialog, candidate removal dialog, duplicate candidate, archived-projects filter, compare-select label for project copies, copy product link, „Zu einem Projekt hinzufügen“, previous-version notice on S42, admin screens (R-ADM-02); `account.export.text` aligned with §8.4; O8 closed |
| `design_handoff_procureaccess_v1.md` (1.13) | §1.1 dark theme in v1 (SD11), H3 closed, H6 closed; dark mode in Figma (O-S5) |
| `prd_procureaccess_werteit_v6.md` | only if the owner wants SD11–SD13 reflected in the PRD (dark theme, e-mail confirmation); no scope change otherwise |
| `CLAUDE.md` (step 9) | reference this spec; rule: every feature requirement implemented names its `R-` ID in the PR description |

# S5-18 — Signed-in header states and organisation switch

> **Sprint:** 05 · **Task:** S5-18 (2 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-SHL-02, R-SHL-03; TEN-01 and TEN-05 cases of `GET /api/v1/organizations` (TESTING_v71 V230, V231); the switch test against TEN-01 deferred to the lists (V232); A11Y-01, A11Y-08, A11Y-09 for the signed-in header (V233); ARCHITECTURE_v63 C149; SPEC v2.31; UX copy v32 U138–U139
> **Status:** done — pull request open (2026-11-16)
> **Scope:** `pa-header` / `pa-main-nav` in the states SignedIn and SignedInNoOrg with the organisation display and the menu „Konto“; the organisation switch for more than one membership; the list of the user's organisations (server and store); compact layout; component, integration and E2E tests, reflow. No EF migration, no new dependency.
> **Owner decisions (2026-11-16, before the task):** 1a — „Projekte“ comes with the projects screens of the workspace step · 2a — one new entry `shell.nav.organization` „Organisation“ (S21 of the active organisation, S20 without one), drafted as UX copy v32 for review · 3a — the active organisation in memory only; a reload or new sign-in starts from the first by name · 4a — the switch and the active organisation now; the switch test of lists, projects and the compare select with those screens. **Defaults accepted:** `GET /api/v1/organizations` in the host with a SPEC §8.9 contract; „Konto“ becomes the Handoff's `mat-menu` named by `shell.userMenu.label` with the address; switching on S21 opens S21 of the chosen organisation; the switch sits in the drawer below 600 px; the fixed id of the E2E organisation stays. **After the plan:** the switch as a menu button (owner, 2026-11-16). **In review:** UX copy v32 U138–U139 confirmed; issue #350 opened for the axe finding of the legacy product list (owner, 2026-11-16).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `pa-header` and `pa-main-nav` knew only Anonymous; signed in, „Konto“, „Einstellungen“ and „Abmelden“ stood flat in the bar (O-S2-4) | The SignedIn states replace them; „Konto“ becomes the menu of Handoff §3.1 |
| 2 | No endpoint listed a user's organisations; `withIdentity()` knew none | `GET /api/v1/organizations` and `organizations` / `activeOrganization` in the store |
| 3 | No projects screen, no saved-list screen, no compare select exists yet | „Projekte“ waits (1a); the scoping test waits for each list (4a) |
| 4 | Figma `pa-app-header` 2017:650 has the variants Auth × Active (SignedIn: „Organisation: …“ as small text, tonal „Konto“; SignedInNoOrg: „Konto“ only); no frame for more than one membership or for the compact menu's content | The switch follows UX copy §5.1 (`shell.org.active` „bei mehreren Mitgliedschaften als Auswahl“, `shell.org.switch`) and is drafted as U139 |
| 5 | S20 and S21 were reachable only by address (decision S5-16 6a); no UX key for a menu entry | `shell.nav.organization` drafted (2a, U138) |
| 6 | Angular keeps `MembersPage` when only `:id` changes; the resolver runs again but the page showed the old organisation (found by the E2E switch test) | S21 takes the resolver's view on a `paramMap` change |

## 2. Changes

### 2.1 Server

- `IOrganizationMembers.ListOfAsync(userId)` and the record `OrganizationSummary` (`IdentityAccess.Contracts`); `OrganizationMemberService` joins memberships and organisations through the request's filter and sorts by name with German collation, then id.
- `GET /api/v1/organizations` in `API/Organizations/OrganizationEndpoints.cs` (`ListMineAsync`, `OrganizationSummaryResponse`).
- Tests: `OrganizationEndpointsTests` — three organisations by name with roles, own only (also with a foreign `X-Organization-Id` header), empty without membership, gone after leaving (`INV:TEN-01`); `AuthorizationMatrixTests` row `list-mine` (`INV:TEN-05`).

### 2.2 Client

- `OrganizationApiService.mine()` (`BACKGROUND_REQUEST`), model `OrganizationSummary`.
- `withIdentity()`: `organizations`, `activeOrganizationId`, computed `activeOrganization`; `loadOrganizations()`, `organizationJoined(id)`, `switchOrganization(id)`; a new signed-in user reads them next to `GET /api/me`; sign-out and an anonymous probe clear them; an answer after a sign-out is dropped.
- S20 and the invitation page call `organizationJoined` before opening S21; leaving on S21 calls `loadOrganizations`.
- `pa-main-nav`: SignedIn / SignedInNoOrg; the tonal „Konto“ `mat-menu` (Konto, Organisation, Einstellungen, Abmelden; `aria-current` on the current item); the switch as a menu button with `menuitemradio` items; in the drawer the same flat, the switch as a `<ul>` of buttons. Switching on S21 opens S21 of the chosen organisation.
- `MembersPage` follows `paramMap` (finding 6), with a component test.
- `testing/shell-test-doubles.ts`: organisations, session and `switchOrganization` in the store double, `TEST_ORGANIZATIONS`; the spec of `withSettings` provides the organisation API.

### 2.3 Texts

- `docs/ux_copy_procureaccess_v32.md` (v30 deleted): U138 `shell.nav.organization` „Organisation“, U139 the form of the switch (no new text), both confirmed by the owner in review; `check-i18n.mjs --write` added the key to `de.json`.

### 2.4 E2E

- New `e2e/shell/header-states.spec.ts`: SignedInNoOrg (seeded confirmed user), SignedIn (seeded Reader), switch wide and compact (an account with two organisations of its own); axe in both appearances on the start page.
- `reflow.spec.ts`: the signed-in compact menu with two long organisation names at 320 px.
- `e2e/support/session.ts`: `accountMenuButton`, `openAccountMenu`, `signOutThroughHeader`; the login, password-reset and J8 specs use them.

### 2.5 Documents

SPEC v2.31 (§8.9 contract, R-SHL-02/03 as built); ARCHITECTURE_v63 C149 (v61 deleted); TESTING_v71 V230–V234 (v69 deleted); Figma mapping 1.25; `CODEBASE_v1.md` §2.64 and §3; `CLAUDE.md` (state line, E2E sign-out helper).

## 3. Self-review

Review with `/engineering:code-review` on the diff, sorted per CLAUDE §6.8. No correctness or security finding that affects current behaviour.

| # | Finding | Sorting |
|---|---|---|
| R1 | Switching on S21 keeps a half-typed address in the invite form (component reused) | Won't fix: rare, visible, checked against the new members before sending |
| R2 | Switching outside S21 is not announced; the button's name changes and keeps the focus | Follow-up (needs a UX key) — open point 3 |
| R3 | The menu „Konto“ holds an item „Konto“ (legacy `/profile`) | Follow-up for S5-28 — open point 4 |
| R4 | Legacy product list: axe `label-content-name-mismatch` signed in | Follow-up issue #350 — open point 2 |
| R5 | Own role in the stored list stale after demoting oneself | Won't fix: the header shows no role |

Figma comparison: the built bar matches the SignedIn and SignedInNoOrg variants (organisation text small and muted to the left of the tonal „Konto“, „Konto“ alone without membership); the switch and the compact menu content have no frame.

## 4. Checks

- `dotnet build -warnaserror` (with the NU190x exception), `dotnet format --verify-no-changes`: green.
- `dotnet test`: Integration 496 (with the new TEN-01 and TEN-05 cases), Unit 753, Architecture 63 — green.
- `npx ng test`: 69 files, 654 tests green; `npm run lint` (client, e2e) and `npm run typecheck` (e2e): green.
- E2E locally on the E2E stack, Chromium, Firefox and mobile Chrome: 360 passed (WebKit and mobile Safari in CI only).
- `check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs`, `reuse lint`: green.

## 5. Open points

1. ~~**UX copy v32 U138–U139** are drafts for the owner's review.~~ Confirmed (owner, 2026-11-16).
2. **Issue #350:** the legacy product list fails axe when signed in (`label-content-name-mismatch`, `aria-label="Product proposal button"` on a filled button) — until its screen is rebuilt.
3. **Announcement after a switch** outside S21: needs a UX key (e.g. „Organisation gewechselt: {{name}}“), together with the first list that reloads on a switch (S5-21).
4. **„Konto“ in the menu „Konto“:** S5-28 (S61) decides the account page's entry.
5. **The switch test of R-SHL-03** joins with S5-21 (saved lists), the workspace step (projects) and the directory step (compare select) (V232); „Projekte“ with the projects screens (1a).
6. The fixed id of the E2E organisation stays (S5-16b open point 3): the tests still read the seeded organisation by id; the list now also finds it.

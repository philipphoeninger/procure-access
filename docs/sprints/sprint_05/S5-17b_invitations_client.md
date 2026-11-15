# S5-17b — Invitations by e-mail (client)

> **Sprint:** 05 · **Task:** S5-17b (part 2 of S5-17, 3 points together) · **Issues:** none
> **Requirements / invariants:** SPEC R-ORG-02, R-ORG-03 (revoking), client part; SEC-10 E2E accept case (TESTING_v69 V224); A11Y-09 for the invitation screens (V225); the Firefox workarounds (V226, V228) and the E2E account helpers (V227); ARCHITECTURE_v62 C148; SPEC v2.30 §8.9; UX copy v31 U133–U137
> **Status:** done — pull request #344 open (2026-11-15)
> **Scope:** on S21 the open invitations with revoking and the invite form for Owners; the page of the invitation link `/organisation/einladung` with its states; the UX copy drafts; the problem-type mapping of `invitation-mismatch`; component tests, E2E (journey, SEC-10, mismatch, invalid) and the reflow check. No server change (S5-17a), no menu entry (S5-18).
> **Owner decisions (2026-11-15):** D2 (a) of S5-17 — the missing UX copy keys drafted as UX copy v31 for review · S5-17b 1 (a) — no preselected role in the invite form; a choice is required, as the organisation type on S20 · S5-17b 2 — UX copy v30 (U125–U132) confirmed · after the Firefox CI crashes: #337 option 2 (the organisation specs one at a time in Firefox) tried and, since it did not help, replaced by CSS locators on the Owner's S21 (V228)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | The server part is merged (S5-17a, SPEC §8.9 v2.29); S21 and its resolver exist (S5-16b) | The resolver also reads the open invitations for an Owner |
| 2 | UX copy §5.7 had nine `org.invite.*` keys: no H1 or page titles for the link's page, no signed-out hint, no headings for the form and the list, no announcements after revoking and accepting, no „already a member“ | Drafted as UX copy v31 U133–U137 (decision D2 (a)) |
| 3 | No Figma frame for S21 or the link's page | Handoff §5.3: form card as on S20, the link's page like the confirmation pages of S02 |
| 4 | The confirmation page and S03 keep their token in memory and take it out of the address; the link's page also has to survive a sign-in on S01 over it | Same guard; the token stays until accepted or refused, resolvers run on every navigation |
| 5 | The view and title resolvers of one route run side by side, so one cannot read what the other writes | One shared read per navigation (`InvitationPageState.loadFor`, keyed by the navigation id) |
| 6 | The server answers a member's address with 400 and an English text, which `errors.field.server` would show | The client refuses it first against the loaded members (`org.invite.alreadyMember`) |
| 7 | UX copy §3 still named the planned routes `/organisation/neu` (S20) and `/organisation` (S21) | Corrected with U133 to the built routes |

## 2. Changes

### 2.1 Texts

- `docs/ux_copy_procureaccess_v31.md` (v29 deleted): U133–U137 — `org.invite.title`, `.invalid.title`, `.signIn`, `.accepted`, `.form.title`, `.role`, `.alreadyMember`, `.error.title`, `.pendingTitle`, `.revoked`, `pageTitle.org.invite`, `pageTitle.org.invite.invalid`; v30 recorded as confirmed; the routes of S20 and S21 in §3.
- `scripts/check-i18n.mjs`: `org.invite` is an `AREAS` entry; `--write` filled `de.json`.
- `pageTitle.org.invite.invalid` is a row of its own: the table's shorthand `/ .invalid` replaces the last key segment, so a combined row wrote `pageTitle.org.invalid` at first (found by reading the `de.json` diff, fixed before any code used it).

### 2.2 Organisation feature

- `models/organization.ts`: `INVITATION_SEGMENT`, `PendingInvitation`, `InviteRequest`, `CreatedInvitation`, `InvitationDetails`, `AcceptedInvitation`.
- `services/api/organization-api.service.ts`: `invite` (`INVITE_PROBLEMS`), `invitations` and `viewInvitation` (`CALLER_SHOWS_FAILURES`), `revokeInvitation` (`REVOKE_PROBLEMS`), `acceptInvitation` (`ACCEPT_PROBLEMS`).
- `components/invite-form/` (new, `pa-invite-form`): address, role radios with hints (no preselection), checked on submit with the error summary; success announces `org.invite.sent`; 503/429/antiforgery in the error notice; outputs `invited` (also after a failed send, which keeps the invitation) and `errorsShown`.
- `pages/members/`: `loadMembersView` reads members and, for an Owner, the open invitations (resolver and reloads); „Offene Einladungen“ with „Einladung zurücknehmen“; the form below; „Fehler:“ in the page title while the form shows field errors.
- `pages/invitation/` (new): `invitation-page-state.ts` (token, shared read), `invitation.routing.ts` (guard, view and title resolvers, `RedirectCommand` to S21 for a member), `invitation-page.*` (states signed out, invited address, other address, unconfirmed, invalid, server).
- `app.routes.ts`: `/organisation/einladung` as lazy child routes.
- `core/errors/problem-details.ts`: `PROBLEM_TYPE_KEYS['invitation-mismatch']`, `ProblemDetails.invitedEmail`.

### 2.3 Tests

- `components/invite-form/invite-form.spec.ts` (7): roles without preselection, required fields, a member's address in any spelling, success, failed send, rate limit, server field error; axe.
- `pages/members/members-page.spec.ts` (+5): Owner sees the list and the form, Reader neither, no section without invitations, revoking, reading again after an invitation.
- `pages/invitation/invitation.routing.spec.ts` (12): token out of the address, signed out, S01/S02, reading again after a sign-in, accepting, SEC-10 notice with focus, mismatch from the read and from a refused accept, invalid in place (title, focus, announcement), invalid link, reload, server failure, a member's redirect; axe.
- `e2e/support/accounts.ts` (new) and `e2e/organization/invitations.spec.ts` (new, 3 tests): the journey (invite on S21, the link signed out, S01, accept, S21, role change, removal), `INV:SEC-10` (unconfirmed invitee), mismatch and invalid; axe in both appearances. `organization.spec.ts` uses the shared `registerAccount`.
- `e2e/shell/reflow.spec.ts` (+2): S21 with an open invitation and the form's error summary; the link's page signed out and invalid.
- `e2e/organization/organization.spec.ts`: the two S21 Owner tests with axe (S20 → S21, last-Owner refusal) find S21's elements by CSS (`memberRow`, `roleOption`; V228).

### 2.4 Documents

- TESTING_v69 (V224–V228; TESTING_v67 deleted), ARCHITECTURE_v62 (C148; v60 deleted), SPEC v2.30 (§8.9 screens), UX copy v31, CLAUDE.md (version line, §3 paragraph, §4, §5).
- No Figma mapping change (no frame), no `LEGACY_EXCEPTIONS` change, no new dependency.

## 3. Self-review

Checklist of CLAUDE §6.7 over the whole diff; findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Firefox crashes the page in the axe run after the invite form was used through Playwright's label and role lookups (reproducible; `getByLabel` of the address field alone is enough) | **fixed in the test** — CSS locators on S21 until that axe run, recorded as V226 like V129/V165; names and roles stay covered by component tests and axe |
| R2 | In long Firefox runs of the whole suite one S21 test crashed now and then (`organization.spec.ts:101` twice, the journey once in 3 full runs); `organization/`, `identity/` and the reflow spec repeated twice in Firefox: 102 of 102 passed | **fixed in the test** — CI showed it was not load: the Firefox job of pull request #344 crashed in `organization.spec.ts:49` and `:101`, on re-run in `:101` again, and with the organisation specs run one at a time (#337 option 2, commit 9aacac4, reverted) in `:101` alone, at its first axe run after name lookups on S21. These tests never crashed on main, before the invite form; the journey with CSS locators passed every time. The S21 Owner tests with axe now use CSS locators (V228) |
| R3 | The token of the link: memory only, out of the address and the history before any request, sent only in a POST body; the page reads it again after a sign-in but never stores it | — |
| R4 | Accessibility: H1 and page title in every state, focus on the notice after an unconfirmed refusal, invalid in place with title/focus/announcement, revoke and invite announced, the remove button described by the address, no preselected radio, error summary with focus | — |
| R5 | S21 reads the invitations only for an Owner (D4 (a)); two requests per load, no N+1 | — |
| R6 | „Anmelden“ appears in the header and on the link's page | **won't fix** — two regions, the same action; the page's button is the next step the text names |
| R7 | A role lookup in the S21 Owner E2E tests could hit the Firefox crash pattern of R1 | **won't fix** — they passed in every isolated and repeated run; R2 tracks the long-run crashes |

No correctness or security finding that affects current behaviour remains.

## 4. Checks

- Client: `ng build` clean; `npm run lint` clean; Vitest 633 passed.
- E2E, locally against this branch (E2E stack): Chromium, Firefox, mobile Chrome — 344 of 345 passed in the full run; the failure was a Firefox „Page crashed“ (R2), which passed on re-run; the invitation spec 3 × in Firefox (9 of 9). WebKit and mobile Safari only in CI.
- CI of pull request #344: every job green except Firefox E2E, which crashed on the Owner's S21 in three attempts (R2), the third with the organisation specs run one at a time — hence V228.
- `e2e/`: `npm run typecheck`, `npm run lint` clean.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons` passed; `reuse lint` compliant.
- No backend change.

## 5. Open points

1. **UX copy v31 U133–U137** are drafts (decision D2 (a)): the owner confirms or changes the wording in review.
2. **#337** (Firefox page crash): the S21 crashes were the V226 class, handled by CSS locators (V228); the identity specs still crash now and then, and the issue stays open for them.
3. **S5-18:** the header states and the organisation switch link S20, S21 and the link's page into the navigation.
4. From S5-16a, still open: no rate limit on creating organisations, a sole member cannot leave. From S5-17a: 5 invitations per hour 🔧.

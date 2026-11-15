# S5-16b — Create organisation S20, members page S21, leave (client)

> **Sprint:** 05 · **Task:** S5-16b (part 2 of S5-16, 4 points together with S5-16a) · **Issues:** none
> **Requirements / invariants:** SPEC R-ORG-01, R-ORG-03, R-ORG-04, R-ORG-06 (screens); **SEC-10 E2E part done** (TESTING_v67 V215); A11Y-09 with S20 and S21 (V216); TESTING §6.3 E2E organisation (V217); TEN-02 allow-list (V218); ARCHITECTURE_v60 C146; SPEC v2.28 §8.9; UX copy v30 U125–U132 (**draft, for the owner's review**)
> **Status:** done — pull request #342 open (2026-11-15); CI result to be recorded
> **Scope:** S20 and S21 with the role select, removal and leaving behind a shared confirmation dialog, the confirmation notice on S20, the missing UX copy keys as a draft, the organisation of the E2E seeder, the E2E tests and the reflow check. No server endpoint changes (S5-16a), no menu entry (S5-18), no invitations (S5-17).
> **Owner decisions (2026-11-15, before the task):** 1a — S5-16 split; this is the client half · 5a — the missing UX copy keys are drafted as UX copy v30 and confirmed in review; removing a member asks first · 6a — no menu entry before S5-18: S20 at `/organisation`, S21 at `/organisation/:id/mitglieder` · 7a — the E2E seeder gets one organisation with Owner, Editor and Reader, which E2E tests only read. **Defaults from the plan:** SEC-10's E2E part registers a fresh account instead of confirming the shared `e2e-unconfirmed`; the notice shows where the 403 `email-unconfirmed` arrives.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-16a left the four endpoints; no client code used them, and `pa-confirm-required-notice` (S5-12b) had no screen | S20 places the notice (SEC-10 E2E part) |
| 2 | S20 and S21 have no Figma frame | Handoff §5.3: S20 form card as S02; S21 `mat-table` from 600 px, `<ul>` below, a role select per member, errors in a notice above the list |
| 3 | UX copy §5.7 lacked page titles, list headers, the role select's name, „(Sie)“, a deleted account, the announcements, the remove dialog and the title of S21's error notice | Drafted as U125–U132 (decision 5a); the leave dialog reuses `org.leave`; the type error reuses `errors.field.requiredChoice` |
| 4 | S21's title „Mitglieder von {{org}}“ needs the name when the route changes, because the title is announced then (A11Y-03) | S21 loads through a resolver, like the confirmation page of S5-12b |
| 5 | No endpoint lists a user's organisations before S5-18, so an E2E test cannot find the seeded organisation | The seeder gives it a fixed id |
| 6 | The disk had 1.6 GB free — too little to build the E2E stack images | E2E ran locally against `dotnet run` of this branch on a throwaway PostgreSQL container and `ng serve`, with the E2E stack's settings (Chromium, Firefox, mobile Chrome; WebKit only in CI) |

## 2. Changes

### 2.1 Client

- `features/organization/models/organization.ts`: types, `ORGANIZATION_PATH` and `MEMBERS_SEGMENT` (no imports, so `app.routes.ts` names them without loading a page), the UX copy key of a type and a role.
- `features/organization/services/api/organization-api.service.ts`: `create` (`CREATE_PROBLEMS`: `validation`, `antiforgery`, `email-unconfirmed`), `members` (`CALLER_SHOWS_FAILURES`), `changeRole` and `remove` (`MEMBER_CHANGE_PROBLEMS`: `last-owner`, `not-found`, `antiforgery`).
- S20 `pages/organization-start/`: H1, intro, the info notice `org.start.contributeNote`, the form card (name ≤ 120 after trimming; the type as a radio group in a fieldset, `errors.field.requiredChoice`), „Organisation anlegen“ with the button spinner, `org.join.text`. Field errors into the error summary (API-02); 403 `email-unconfirmed` shows `pa-confirm-required-notice` above the form with the focus and keeps the input; success opens S21.
- S21 `pages/members/`: `members.routing.ts` (lazy child routes, `authGuard`, the resolver, route-level `MembersPageLoad`), `members-page-load.ts` (the view and `failedView`: 404/400 → not found, else server), `members-page.ts`. `mat-table` from 600 px, `<ul>` below; for an Owner a `mat-select` per member (named `org.members.role.label`, the hints as second lines, `mat-select-trigger` with the role only) and „Aus der Organisation entfernen“ for the others (described by the address); „Organisation verlassen“ for everyone. A change saves at once and is announced; a refusal shows the error notice `org.members.error.title` and reads the list again (rows rebuilt through a version in the track key, so a select shows the stored role); a lost focus goes to the H1; leaving announces `org.left` and opens S20.
- `shared/components/confirm-dialog/`: `ConfirmDialog.confirm(dialog, data)` — title, text, „Abbrechen“, the action; focus on the title (`first-heading`); stacked below 600 px with the action last.
- `pages/error/`: optional `kind` input, so a page shows S80 in place.
- `app.routes.ts`: both routes; `core/errors/problem-details.ts`: `last-owner` → `org.members.lastOwner`; `scripts/check-i18n.mjs`: areas `org.start`, `org.create`, `org.join`, `org.members`, `org.leave`, `org.left`.
- `de.json` by `check-i18n.mjs --write`.

### 2.2 E2E seeder (server)

- `API/Extensions/TestUserSeedExtensions.cs`: three more confirmed users `e2e-org-owner|editor|reader@example.invalid`, `OrganizationName` „E2E-Organisation“, the fixed `OrganizationId`; after the users it calls `IdentityAccessModule.SeedTestOrganizationAsync`.
- `src/ProcureAccess.Modules.IdentityAccess/Organizations/TestOrganizationSeeder.cs` (`internal`) and the public method on `IdentityAccessModule` (exactly one Owner). An organisation of the same Owner stays; one of another Owner is deleted (memberships by cascade) and created again — see R1.
- `Domain/Organization.cs`: `Create` takes an optional id, for the seeder only.

### 2.3 Tests

- Client: `organization-start-page.spec.ts` (6), `members-page.spec.ts` (8), one case in `error-page.spec.ts`; axe-core on the states.
- Unit: `OrganizationTests.Create_GivenId_KeepsIt`.
- Integration: `TestUserSeedGuardTests` — the organisation with its three members, idempotent on a second start, none outside Development/E2E, and the case of R1 (two starts with a rebuild).
- Architecture: the seeder in the TEN-02 allow-list.
- E2E: `e2e/organization/organization.spec.ts` — S20 with SEC-10's E2E part (`INV:SEC-10 INV:A11Y-01 INV:A11Y-08`), S21 as Owner (selects, last-Owner refusal) and Reader, cancelled dialogs with the focus back, a foreign id; `reflow.spec.ts` — S20 with the error summary, S21 as a list with the notice and the open dialog. `support/session.ts` `signInThroughApi`, `support/users.ts` the organisation users and `E2E_ORGANIZATION`.

### 2.4 Documents

- UX copy v30 (U125–U132, draft); v28 deleted.
- SPEC v2.28 (§8.9 screens, current versions).
- ARCHITECTURE_v60 (C146); v58 deleted. TESTING_v67 (V215–V218: SEC-10 E2E part, A11Y-09, §6.3, TEN-02); v65 deleted.
- CLAUDE.md: version line, §3 (S5-16b paragraph), §4 (client, IdentityAccess, e2e), §5 (E2E stack).
- No Figma mapping change: S20 and S21 have no frame.

## 3. Self-review

Checklist of CLAUDE §6.7 over the whole diff; findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The first local run showed S21 „Seite nicht gefunden“ for every seeded user: an API restart on the same database ran the Development rebuild, which created the users again with new ids, and the seeder skipped the existing organisation, so its memberships named deleted users | **fixed** — an organisation of another Owner is replaced; guard test `Startup_RebuildDatabaseTwice_TheE2EOrganizationNamesTheCurrentUsers` |
| R2 | A failed reload after a change showed „Seite nicht gefunden“ also for a server failure | **fixed** — the same `failedView` as the resolver |
| R3 | `members-page.ts` and `members.routing.ts` imported each other (worked by evaluation order) | **fixed** — shared parts in `members-page-load.ts` |
| R4 | The S20 E2E test expected the focus on the H1 after `page.goto`; the first page load moves no focus (R-SHL-04) | **fixed in the test** — the route change to S21 is where the focus is checked |
| R5 | `org.left` is announced on S21 just before S20 opens, not on S20 itself (UX copy U130 says „auf S20 danach“) | **won't fix** — the announcer queues in call order, so it is read before S20's title; an announcement from S20 would race the title announcement |
| R6 | No busy guard on the role select: two quick changes send two requests | **won't fix** — each is a full PATCH, the server serialises them (S5-16a lock), and the list is read again after each |
| R7 | Security: every request is authorised by the server (S5-16a); the client only hides controls; no new dependency, no secret, no `innerHTML` | — |
| R8 | a11y: the error summary and the notice take the focus, the selects and buttons have names that name the member, the table has column headers, the dialogs return the focus, nothing is `disabled` | — |

No correctness or security finding that affects current behaviour remains.

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1217 passed (unit 727, architecture 63).
- Client: `ng build` clean; `npm run lint` clean; Vitest 609 passed.
- E2E, locally against this branch (see §1, finding 6): Chromium, Firefox, mobile Chrome — 329 of 330 passed; the one failure was `identity/password-reset.spec.ts` in Firefox with „Page crashed“ (flake #337, not touched here), which passed twice on re-run. WebKit and mobile Safari only in CI.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons` passed; `reuse lint` compliant.

## 5. Open points

1. **UX copy v30 U125–U132** are drafts (decision 5a): the owner confirms or changes the wording in review.
2. **S5-17:** invitations, the pending list on S21 and revoking; an E2E test that builds its own organisation can then cover role changes, removal and leaving end to end.
3. **S5-18:** the header states and the organisation switch link S20 and S21; the fixed id of the E2E organisation can stay or give way to the list from the session.
4. **#337** (Firefox page crash in the identity specs) still hits CI now and then; S5-16b adds two Firefox tests with axe.
5. From S5-16a, still open: no rate limit on creating organisations, a sole member cannot leave.

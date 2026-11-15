# S5-16a — Organisation endpoints: create, members, role change, removal, leaving (server)

> **Sprint:** 05 · **Task:** S5-16a (part 1 of S5-16, 4 points together) · **Issues:** none
> **Requirements / invariants:** SPEC R-ORG-01, R-ORG-03, R-ORG-04 (server part), R-ORG-06 (no join or directory endpoint); **TEN-11 `active` for its organisation part** (TESTING_v66 V211); TEN-05 on the real endpoints (V212); SEC-10 on the first real gated action (V213); TEN-02 widened to raw SQL (V214); ARCHITECTURE_v59 C145; SPEC v2.27 §8.6, §8.9
> **Status:** done — pull request #341 open (2026-11-15); CI result to be recorded
> **Scope:** the four organisation endpoints in the host behind the IdentityAccess contract `IOrganizationMembers`, the last-Owner rule under a row lock with the problem type `last-owner`, the TEN-05 matrix and the SEC-10 gate moved off the test-only probes. No screen, text, EF migration or E2E change.
> **Owner decisions (2026-11-15, before the task):** 1a — S5-16 is split: S5-16a server, S5-16b client (S20, S21, leave dialog, notice, E2E, reflow) · 2a — revoking invitations moves to S5-17 with the invitations · 3a — new problem type `last-owner` for the 409 (`conflict` stays API-03's) · 4a — the endpoints live in the host and add the members' addresses from the legacy users; members see each other's addresses · 5a — S5-16b drafts the missing UX copy keys as v30 for review, with a confirmation dialog for removing a member · 6a — no menu entry before S5-18: S20 at `/organisation`, S21 at `/organisation/:id/mitglieder` · 7a — the E2E seeder gets one organisation with Owner, Editor and Reader; E2E only reads it. Decisions 5a–7a are for S5-16b. **Defaults accepted with the plan:** the name is trimmed, a name of spaces counts as missing; no rate limit on creating organisations; a sole member cannot leave (open points 3, 4).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-15 left the tables, the per-request membership filter and the role requirement, and two test-only probes: `OrganizationProbeController` (TEN-05, TEN-01) and `ConfirmedEmailProbeController` (SEC-10) | Both probes go; their tests run on the real endpoints |
| 2 | The S5-16 row includes revoking invitations, but invitations are their own entity (`OrganizationInvitation`, ARCHITECTURE §5.2) built in S5-17 | Decision 2a |
| 3 | SPEC §8.6 had only `conflict` for 409, which means a concurrent edit with `editorDisplayName` and the dialog S37 | Decision 3a: `last-owner` |
| 4 | The members' addresses are on the legacy `User` (MODELS); a module cannot reference legacy projects and ADR-014 forbids joins across contexts | Decision 4a: host endpoints, rules in the module behind a contract |
| 5 | Two Owners who demote each other at once would both see two Owners and leave none | Row lock on the organisation before the count |
| 6 | The lock is the first raw SQL in a module; S5-15 open point 2 asked to widen the TEN-02 scan then | Done here (V214) |
| 7 | No open pull request touched IdentityAccess; no migration is needed | — |

## 2. Changes

### 2.1 Contracts

- `src/ProcureAccess.Modules.IdentityAccess.Contracts/Organizations/IOrganizationMembers.cs`: `CreateAsync`, `GetAsync`, `ChangeRoleAsync`, `RemoveAsync`; records `OrganizationMembers`, `OrganizationMember`; enum `MembershipChangeResult` (`Done`, `NotFound`, `LastOwner`); `OrganizationLimits.NameMaxLength` (120, now also the source of `Organization.NameMaxLength`). The contract never authorises; the caller checks the role first.
- `src/ProcureAccess.Modules.Platform.Contracts/Errors/ProblemTypes.cs`: `LastOwner = "last-owner"`, also in `All`, so Platform writes its URI.

### 2.2 IdentityAccess module

- `Domain/OrganizationMembership.cs`: `ChangeRole(role)` and the rule `LeavesNoOwner(currentRole, newRole?, ownerCount)`.
- `Organizations/OrganizationMemberService.cs` (`internal`): create = organisation + founder membership in one save; members through the filtered context, Owners first. A role change or removal runs in a transaction inside the context's execution strategy: filtered existence check → `SELECT 1 … FOR UPDATE` on the organisation row → load the membership → same role is `Done` → count the Owners → `LastOwner` or change and commit.
- `IdentityAccessModule.cs` registers it.

### 2.3 Host

- `API/API.ProcureAccess/Organizations/OrganizationEndpoints.cs`, mapped in `Program.cs` after the password-reset endpoints (global using added):
  - `POST /api/v1/organizations` (`ConfirmedEmail`): 201 `{ id }` with `Location`; 400 at `name` / `type`.
  - `GET …/{orgId}/members` (Reader): organisation and members with `UserManager` addresses (one query; `null` for a deleted account), Owners first, then by address.
  - `PATCH …/members/{userId}` (Owner): 204, 400 at `role`, 404, 409 `last-owner`.
  - `DELETE …/members/{userId}` (Owner for someone else, Reader for the caller's own id): 204, 404, 409 `last-owner`.
  - Types and roles as camel-case strings; no `:guid` route constraint, so a malformed id is 400 `validation` (API-01's discovery test asked for it).

### 2.4 Tests

- Unit: `IdentityAccess/OrganizationTests.cs` — `ChangeRole`, `LeavesNoOwner` for every role pair (`INV:TEN-11`).
- Integration, new: `Organizations/OrganizationEndpointsTests.cs` (29 cases) — create (trimmed name, each type, 120 accepted, invalid bodies keyed by field, unconfirmed `INV:SEC-10`, anonymous), members with addresses and order, role change and its validation, unknown member, TEN-11 (last Owner demotes or leaves; two Owners; removal of the other Owner; the concurrent demotion with the lock held by the test until `pg_stat_activity` shows both requests waiting — on a connection of its own, because a transaction keeps its first snapshot of the statistics), R-ORG-03 (removed member gets 404 at the next request), leaving.
- Integration, changed: `Tenancy/AuthorizationMatrixTests.cs` on the real endpoints (23 cells + 3 header cases + a member of B named on A's route; TEN-01 cases on `GET members`; probe controller removed); `IdentityAccess/RegistrationTests.cs` checks the gate on `POST /organizations` (probe removed); `Api/ProblemDetailsTests.cs` stores a confirmed account for its Moderator, so the gated endpoint reaches model binding.
- Architecture: `Tenancy/IgnoreQueryFiltersAllowListTests.cs` scans `.FromSql…`, `.SqlQuery…`, `.ExecuteSql…` too; allow-list: the lock (1), `BaseRepo` and `BaseViewRepo` (2 each).
- Checked by hand: with the `FOR UPDATE` line disabled, the concurrency test fails („did not wait for the organisation's lock“).

### 2.5 Documents

- SPEC v2.27: §8.6 `last-owner`, new §8.9 (the contract), current-versions line.
- ARCHITECTURE_v59 (C145): §5.2 membership row, §5.4 raw SQL, §7.1 endpoint list, §9.1, §12 step 3. No version deleted (v58 and v59 are the two kept).
- TESTING_v66 (V211–V214): TEN-11 `active` (organisation part), TEN-05 and SEC-10 notes, TEN-02 wording. No version deleted.
- CLAUDE.md: version line, §3 (S5-16a paragraph; S5-12a and S5-15 sentences about the probes), §4 (host `Organizations/`, IdentityAccess `Organizations/`, Contracts, `Integration/Organizations/`), §5 (curl examples), §7 (raw SQL counts as a bypass).
- No UX copy, `de.json`, Figma mapping or `LEGACY_EXCEPTIONS` change: no screen.

## 3. Self-review

Checklist of CLAUDE §6.7 over the whole diff (`/engineering:code-review`); findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Authorisation and tenant isolation: 401 from the group, the route's organisation checked before the service, the gate on create, antiforgery on every state-changing call, leaving only for the caller's own id from the claim | — |
| R2 | The raw SQL is a parameterised `FormattableString`; the lock comes after the filtered read, so a non-member never locks a foreign row | — |
| R3 | An Owner demoted or removed while their own request is in flight finishes that request with the role it was authorised with; SPEC R-ORG-03 asks for the next request | **won't fix** — a re-check under the lock is a mechanism the spec does not ask for |
| R4 | A retry of the execution strategy after a lost commit acknowledgement answers 404 for a removal that succeeded | **won't fix** — transient failure exactly at commit, no data harm; the client reloads the list |
| R5 | `FOR UPDATE` also waits for the foreign-key share lock of a membership insert for the milliseconds of a change | **won't fix** — one lock order, no deadlock; S5-17 checks its accept flow (open point 2) |
| R6 | N+1: one membership query and one address query per list | — |
| R7 | The lock test polls with real time | **won't fix** — test-only wait for a database state, as `LogRetentionTests` |
| R8 | No new package, secret, migration or UI | — |

No correctness or security finding that affects current behaviour.

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1214 passed (Unit, Architecture, Integration; the organisation tests three times in a row).
- `node scripts/check-invariants.mjs` passed (TESTING_v66: TEN-11 6 tests, TEN-05 4, TEN-02 1, SEC-10 9); `check-figma-links`, `check-i18n` passed; `reuse lint` compliant.
- OPS-03 unchanged (no migration). No E2E run: no client change.

## 5. Open points

1. **S5-16b** (decisions 1a, 5a–7a): S20 at `/organisation`, S21 at `/organisation/:id/mitglieder` with the role select, removal (confirmation dialog) and leaving, `org.members.lastOwner` for `last-owner` in an error notice above the table (client `PROBLEM_TYPE_KEYS`), `pa-confirm-required-notice` on 403 `email-unconfirmed`, UX copy v30 with the missing keys (page titles, list headers, role-select name, „(Sie)“, announcements, remove dialog), the seeded E2E organisation, SEC-10's E2E part with a freshly registered user, both pages in `reflow.spec.ts`.
2. **S5-17:** invitations, the pending list and revoking (decision 2a); check that accepting an invitation does not need to wait on the organisation lock longer than a role change (R5).
3. **No rate limit** on `POST /organizations`: SPEC §7.3 lists none, and 🔧 values come only from the SPEC. A confirmed account could create many organisations — a SPEC question for the owner if it matters before the pilot.
4. **A sole member cannot leave**, because they are the last Owner, and deleting an organisation is not designed (R-ORG-04 and C79 cover only account deletion, S5-28). A SPEC question if users need it.
5. **M3.1:** the endpoints move into IdentityAccess with the users; then the addresses come from the module's own user table.

# S5-28a — Account deletion, server part

> **Sprint:** 05 · **Task:** S5-28a (server part of S5-28, 3 points for S5-28 in all) · **Issues:** #234 (TD-26 part; stays open — the legacy delete dialog goes in S5-28b)
> **Requirements / invariants:** SPEC R-AUTH-10 (server part), §8.6, §8.13 (v2.45); **TEN-11** account part → `active`, the invariant is complete (TESTING_v85 V286 – V288); ARCHITECTURE C163. R-AUTH-09 and A11Y-09 are S5-28b
> **Status:** done — pull request open (2026-11-17)
> **Scope:** `DELETE /api/v1/account` with the organisation rules: the only Owner of an organisation with other members is refused, an organisation ends with the account of its only member, with its criteria lists, share links and invitations. A deleted list deletes its share links. No client change: S61, the download of the export and the removal of the legacy profile page, its dialogs and `DELETE /api/Users` are S5-28b.
> **Owner decisions (2026-11-17, before the task and with the plan):** S5-28 **1a** — S61 gets no change of e-mail address or password: no requirement and no UX copy key asks for it, the legacy dialog never worked, and a password is changed through S03 · **2a** — the account row stays soft-deleted (ARCHITECTURE §9.3); the password hash is cleared, the `UserPreferences` row deleted; the endpoint is `DELETE /api/v1/account` · **3** — the parts of R-AUTH-10 on contributor profiles, entries, experience reports and projects join with their steps and are recorded as deferred · **4** — the other sessions end at the next check of the security stamp; the immediate end is S5-29 · **5a** — the proposal list of the legacy profile goes with the page (S5-28b) · **6a** — 409 with the names of all organisations, a new problem type · **P1** — split into S5-28a (server) and S5-28b (client) · **P2 (a)** — the request has no body; the address typed on S61 is compared in the client · **P3 (a)** — no message after the deletion, as the sign-out

## 1. Starting point

- The only deletion was the legacy `DELETE /api/Users` (`UserRepo.DeleteUserAsync`): it replaced the address by `deleted_<guid>@anonymous.de`, set `IsDeleted` and changed the security stamp. The password hash stayed, and so did the memberships, an organisation without any other member, its lists and share links, the `UserPreferences` row (S5-25a R2) and the account tokens (S5-10 open point 4).
- TEN-11 was `active` for its organisation part only (V211); the account part waited for this task.
- A deleted list left its share-link rows (owner decision S5-23 8; S5-23 R5 named S5-28 for them).
- **Contradiction found before the plan:** the sprint row and the task prompt ask for „change e-mail and password (rebuilt from the legacy profile)“; SPEC, UX copy §5.14 and the code have nothing for it (the endpoints went in S5-10, the legacy dialog ends in a `console.log`). Decided 1a.
- **Found while writing the documents:** ARCHITECTURE §7.2 planned `DELETE /api/v1/account { emailConfirmation }`. The plan's recommendation P2 (a), which the owner took, did not name this line — see open point 1.

## 2. Changes

### 2.1 Server

- **Host** `API/Account/AccountDeletionEndpoints.cs`: `DELETE /api/v1/account` (signed in, antiforgery, no body, no confirmed address, no rate limit). 404 for an account deleted before; 409 `sole-owner` with `organizations`; 204 with the session cookie deleted and a new antiforgery token. Order: `IAccountDeletion.BeginAsync` → lists → share links → `IAccountDeletion.CompleteAsync` → the legacy user (address and user name `deleted-<guid>@deleted.invalid`, unconfirmed, no password hash, `IsDeleted`, `DeletedAt`, new security stamp). Mapped in `Program.cs` after the export.
- **Platform.Contracts** `ProblemTypes.SoleOwner` (`sole-owner`); `IShareLinks.DeleteOfResourceAsync` and `.DeleteOfOrganizationsAsync`, implemented in `ShareLinkService` (hard deletes through the organisation filter).
- **IdentityAccess** `IAccountDeletion` (`Contracts/Accounts`) and `AccountDeletionService`: two steps, each one transaction under the row locks of the user's organisations (in the order of their ids). Begin: the check; when it passes, the invitations of the organisations whose only member the user is are deleted and their ids returned. Complete: the check again; these organisations (memberships and invitations cascade), the user's memberships, `UserPreferences` row and account tokens.
- **Configurator** `ICriteriaListDeletion` (`Contracts/Accounts`) and `CriteriaListDeletionService` (lists of the given organisations; rows cascade); the handler of `DELETE /configurations/{id}` deletes the list's share links after the list.
- **Why two steps:** the module contexts share no transaction (ADR-014). In this order every step can be repeated and none leaves rows nobody can reach: a failure answers 500, the account still signs in, the next attempt finishes. Deleting the invitations first closes the only way someone else could join an organisation while its lists are deleted.
- **No EF migration**, no new dependency, no new setting, no `IgnoreQueryFilters()` or raw SQL call site (the row lock is the allow-listed one).

### 2.2 Client

None (S5-28b). The client's `PROBLEM_TYPE_KEYS` gets `sole-owner` there, with the `account.*` texts.

### 2.3 Tests

- `tests/ProcureAccess.Tests.Integration/IdentityAccess/AccountDeletionTests.cs` (8): four `INV:TEN-11` cases — refusal naming every organisation and changing nothing; an Owner next to another Owner; the only member, with the organisation's lists, links and invitations gone and those of another organisation kept; refused first, then deleted once the other members are gone —, the account row, settings, tokens and the refused sign-in, 404 for a second deletion, `INV:SEC-03` (401) and `INV:SEC-02` (antiforgery).
- `ShareLinkTests.DeleteList_WithLinks_DeletesItsLinksAndKeepsThoseOfOtherLists`.

### 2.4 Documents

TESTING_v85 (V286 – V288; v83 deleted), ARCHITECTURE_v77 (C163, §7.2, §5.3; v75 deleted), SPEC v2.45 (§8.6, new §8.13, §8.9, §8.10, note at R-AUTH-10), CODEBASE §2.78, §3, §4, CLAUDE.md §3, §4 (state of the code, `API/Account/`).

## 3. Self-review

Code review per CLAUDE §6.7 (`/engineering:code-review`), sorted per §6.8:

| # | Finding | Sorting |
|---|---|---|
| R1 | The first run of `Delete_AccountDeletedBefore_Returns404` answered 400 `antiforgery`: the deletion issues an anonymous antiforgery token while the test scheme keeps the user signed in | **fixed** in the test (a new client for the second request); the real cookie session is signed out by the same answer |
| R2 | Between the two steps the user could, in a second session, create a list in an organisation that ends (the list would stay without organisation), or invite someone who accepts at once (the second step then refuses with the lists already deleted) | **won't fix** — only the user's own second session can do either, within the milliseconds of one request; closing it would need a transaction over three contexts |
| R3 | The deletes go through the organisation filter, which holds the memberships of the request's start: a membership the user gains during the request (an invitation accepted in another tab) stays | **won't fix** — same narrow window; the row shows as „Gelöschtes Konto“ on S21 and an Owner can remove it |
| R4 | `ICriteriaListDeletion` and `IShareLinks.DeleteOfOrganizationsAsync` delete for any organisation of the request's user, not only for one that ends; they trust the caller's list | **won't fix** — one caller, which passes what `BeginAsync` returned; both say so in their summary. A check of their own would need the membership counts of another module |
| R5 | The soft-deleted row keeps its legacy role, claim and lockout columns | **follow-up M3.1** — the users move into IdentityAccess there; a deleted account cannot sign in (no password, hidden by the legacy filter) |
| R6 | No test forces a failure between the steps | **won't fix** — it would need fault injection into a module service; the repeatability is covered by the refused-then-deleted case and by each step's own idempotence |
| R7 | The real cookie scheme is not part of these tests (the integration host signs in through the test scheme) | **follow-up S5-28b** — its E2E test deletes an account of its own through S61 and checks that the session is gone and the sign-in refused |
| R8 | Security: the user comes only from the cookie principal; antiforgery tested; the 409 names only organisations the user owns; no user or organisation id in a log or an answer; no filter bypass, no raw SQL beyond the allow-listed row lock | **no finding** |
| R9 | Performance: one lock statement per organisation of the user, then one read and at most four deletes per step; no N+1 over rows | **no finding** |
| R10 | Accessibility: no screen in this part | **n/a** (S5-28b) |

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` **1 661 passed, 0 failed** (unit, architecture, integration).
- `check-invariants` passed; `check-figma-links`, `check-i18n`, `check-icons`, `check-api-notices` 0 errors; `reuse lint` compliant.
- Client and E2E not run: no client file changed; CI is the authority.

## 5. Open points

1. **Owner decision asked: body of the request.** ARCHITECTURE §7.2 had planned `{ emailConfirmation }`; built without a body per P2 (a), which was recommended without naming that line. (a) keep it — C163 and §7.2 record it as built; (b) send and check the address on the server too (400 `validation` at `emailConfirmation` on a mismatch) — a small change that fits S5-28b, where the client sends it.
2. **S5-28b:** S61 at `/konto` (export download, `auth.confirm.required`, deletion section and dialog with `aria-disabled` and its visible reason, `account.delete.soleOwner` per organisation of a 409), `sole-owner` in `PROBLEM_TYPE_KEYS`, the menu item „Konto“; removal of the legacy profile page, the delete-account and update-user dialogs, `identity-api.service.ts`, `deleteAccount` of `with-identity`, their `LEGACY_EXCEPTIONS` entries, `DELETE /api/Users` with `UsersController`, `IUserRepo.DeleteUserAsync` and their tests; E2E with an account of its own, axe in both appearances, S61 in `reflow.spec.ts`; closes #234.
3. **Later steps** (decision 3): contributor profiles detached and anonymised, entries withdrawn, experience reports purged (steps 6 – 7; PRV-06, PRV-10); procurement projects of an organisation that ends (step 5; the last words of TEN-11); the immediate end of the other sessions (SEC-08, S5-29).
4. **Left as they are:** open invitations other organisations sent to the deleted address stay until they expire and the clean-up removes them; `CreatedByUserId` of lists in organisations that live on; the legacy proposals of the user (as in the export, S5-27 3a).
5. **Sprint row and task prompt** of S5-28 still name „change e-mail and password“; decision 1a replaces that. A SPEC question for a later sprint if users need to change their address.

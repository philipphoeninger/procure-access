<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-23 — Retire the proposal workflow

> **Sprint:** 06, task S6-23 (M5.6, 4 points). **Issues:** #384 (TD-61) — closed by deletion; #158 — the four proposal entries of `LEGACY_EXCEPTIONS` (the issue stays open for the three M2 entries); #82 — checked, the rest commented on the issue.
> **Requirements / invariants:** ARCHITECTURE §12 step 5 (proposal part), §8.1, D11. **ARC-05** `planned` → `active` (TESTING_v92 V313). API-02 stays green (its tagged test moved first, V314); API-01 (V315), PRV-07, TEN-06 (V316) and OPS-03 (V317) stay green. R-SUG-01 („Kriterium vorschlagen“) is unchanged.
> **Status:** done by Claude Code (2026-11-18) — pull request open. **Two EF migrations:** legacy `ApplicationDBContext` `RetireProposals` (drops the proposals table) and IdentityAccess `RevokePlatformAdminModerationReview` (data only).
> **Scope:** exactly the row of S6-23 in `SPRINT_06.md`. Also removed because only the proposal code used them: the Markdown editor with its dependency EasyMDE and its icons, the permission guard and the two permission directives, the host policy `ModerationReview`, the create DTOs and their maps. Not done here, see §5: the move of `Permissions` / `CustomClaimTypes` out of MODELS and the Identity package reference of SERVICES (both named for S6-23 in S6-10a's open points).
> **Owner decisions:** **O-S6-4 (a)** (2026-11-18): retire the proposals and drop their tables with M5.6; pre-pilot data, nothing to export; no export or deletion code for proposals. **In the session (2026-11-18, "go ahead with your recommendations"):** **D1 (a)** — the main-navigation item „Beitragen“, which led to the legacy product proposal (owner, S2-15), is left out until S53 of step 6 exists · **D2 (a)** — `withProducts()` / `withFavorites()` only lose their proposal ties here; their rework into `withWorkspace()` moves to S6-13 · **D3 (a)** — ARC-05 allows `BaseCrudController` for the three catalog-admin controllers and names the legacy `ProductsController` as the one exception until step 6.

## 0. Plan (written before the code)

- **Starting point:** `main` at c66f601; no open pull request, so no other branch had a legacy or IdentityAccess migration or a TESTING version open.
- **First commit:** move the tagged API-02 test from `product-proposal.spec.ts` to S02 (`register-page.spec.ts`).
- **Then:** server (controller, service, repo, entity, DTOs, commands, maps, navigations, policy), two migrations, client (slice, pages, routes, forms, buttons, keys, lint entries, guard and directives), ARC-05 test, the tests that used proposal endpoints, E2E specs, documents.
- **Migrations:** legacy `RetireProposals`; IdentityAccess for PlatformAdmin's claim, because the role claims belong to that context since S6-10a (the sprint plan named only a legacy migration).
- **Questions to the owner:** D1 – D3 above, answered with the recommendations.
- **Size:** one session (mostly deletion); fallback split a = API-02 + client, b = server + migrations + ARC-05 was not needed.

## 1. Starting point

| # | Found | Consequence |
|---|---|---|
| F1 | The only `INV:API-02` test sat on the legacy product proposal (V80) | Moved first (commit 1), to S02 Registrieren |
| F2 | `ProposalService.cs:217` held the last `DateTime.UtcNow` outside tests | Gone with the service: 0 left |
| F3 | Products and criteria were read with the filter „no proposal or an approved one“; a product or criterion row was only created by an approval | The filter goes with the navigation; the reads return the same rows |
| F4 | PlatformAdmin's `moderation:review` lives in `identity."RoleClaims"`, which belongs to IdentityAccess since S6-10a | Second migration, in IdentityAccess (no migration crosses two contexts) |
| F5 | The policy `ModerationReview`, `permissionGuard`, `appHasPermission` and `appHasNotPermission` had no user but the proposals | Removed; `withIdentity().hasPermission()` stays |
| F6 | The Markdown editor and EasyMDE (with `codemirror`, `typo-js`, a nested `marked`) served only the two proposal forms; EasyMDE's `@-moz-keyframes` gave the release build its critical-CSS warning (TECH_DEBT_v5 §5, step 5) | Removed with their styles and twelve icons; the build has no warning any more |
| F7 | „Beitragen“ in the main navigation led to `/product-proposal/0`; its real target S53 `/beitragen` comes with step 6 | Owner decision D1 (a) |
| F8 | ARCHITECTURE §12 step 5 repoints `withProducts()` / `withFavorites()` at the new model, but `withWorkspace()` comes with S6-13 | Owner decision D2 (a) |
| F9 | `ProductsController` derives from `BaseCrudController` but belongs to the directory | Owner decision D3 (a) |
| F10 | Tests used proposal endpoints: API-01 field paths (`/api/Proposals/upsert`), the test authentication (`GET /api/Proposals`), TEN-06 (`ModerationPolicyTests`), `GenericCrudWriteTests`, ARC-06 map cases; the E2E „Kein Zugriff“ check went through `/proposals` | Retargeted or removed (§2) |

## 2. Changes

1. **API-02 (commit 1):** `Submit_ServerFieldErrors_BoundToFieldsAndSummaryFocused INV:API-02` in `features/identity/pages/register/register-page.spec.ts` with the same checks as on the proposal form (field error at the field, summary focused, summary link to the field, `$` as a line without a link) and the case that an edit clears the error; the untagged S02 test it replaces is gone.
2. **Server:** deleted `ProposalsController`, `ProposalService`, `IProposalService`, `ProposalRepo`, `IProposalRepo`, `Proposal`, `ProposalStatus`, `ProposalConfiguration`, `ProposalDto`, `MODELS/Requests/*ProposalCommand.cs`, `AccountMappings`, `CreateProductDto`, `CreateCriterionDto` and their `ToEntity` maps, `API/Extensions/AuthorizationExtensions.cs`; `Product.Proposal`, `Criterion.Proposal` and the proposal filter of `ProductService` / `CriterionService` removed; registrations and global usings cleaned; comments of `BaseCrudController`, `ApplicationDBContext`, `WorkspaceModule`, `Permissions` updated.
3. **Migrations:** `DAL/…/Migrations/20261118123648_RetireProposals.cs` (drop; `Down` creates the empty table); `src/ProcureAccess.Modules.IdentityAccess/Persistence/Migrations/20261118123722_RevokePlatformAdminModerationReview.cs` (`internal`, SQL delete; `Down` inserts again). Both contexts: no pending model changes.
4. **ARC-05:** `tests/ProcureAccess.Tests.Architecture/Api/GenericCrudControllerTests.cs`.
5. **Server tests:** new `Persistence/ProposalRetirementMigrationTests.cs` (V316, V317); `IdentityTablesHandOverTests` expects PlatformAdmin without the permission and no proposal table; `ProblemDetailsTests` (API-01 field paths) posts to `POST /api/v1/auth/register`; `TestAuthenticationTests` reads `GET /api/v1/account/preferences`; `GenericCrudWriteTests` and `MapCases` / `MappingCompletenessTests` lose the proposal parts; `ModerationPolicyTests` deleted.
6. **Client:** deleted `features/proposal/`, `features/products/proposal/`, `features/criteria/proposal/`, `features/identity/guards/permission.guard.ts`, `features/identity/directives/`, `shared/components/markdown-editor/`, `products/models/create-product.dto.ts`, `criteria/models/create-criterion.dto.ts`; routes `/proposals`, `/product-proposal/:id`, `/criterion-proposal/:id` removed; `withProposal()` out of the root store with `loadProposals()` after the sign-in and `setProposals([])` in `clearUserData()`; the „vorschlagen“ buttons of `products-list` and `products-container` and their style removed; `easymde` uninstalled (8 packages left the lock file), its two `angular.json` entries and its styles in `main.scss` removed; `de.json` loses `PROPOSALS.*` and 14 legacy keys only the forms used; `LEGACY_EXCEPTIONS` keeps the three M2 entries.
7. **Icons:** `edit`, `checklist`, `format_bold`, `format_italic`, `format_list_bulleted`, `format_list_numbered`, `format_quote`, `fullscreen`, `help`, `image`, `title`, `vertical_split` out of the icon list; subset regenerated (14 icons, 8 288 bytes instead of 15 048); `scripts/check-icons.mjs` and its self-test no longer read the EasyMDE map of `main.scss`.
8. **Navigation (D1 (a)):** „Beitragen“ out of `layout/main-nav/main-nav.html` with a comment; `main-nav.spec.ts`, `testing/shell-test-doubles.ts`, `e2e/shell/reflow.spec.ts` follow; `e2e/shell/start-and-error-pages.spec.ts` opens „Kein Zugriff“ at `/unauthorized` directly.
9. **Documents:** `ARCHITECTURE_v86.md` (C172; §8.1, §9.1, §12 step 5, D11; v84 deleted), `TESTING_v92.md` (V313 – V318, ARC-05 `active`, the API-02 and TEN-06 rows; v90 deleted), `CODEBASE_v1.md` (§2.93, §3 API, DAL and Architecture lines, §1), `CLAUDE.md` (the store-slice note in §4 and the example form of the server field errors in §9 named the proposals). SPEC, UX copy, Handoff and Figma mapping are unchanged: none names the proposals as built (the S53 frame stays the target of „Beitragen“).

## 3. Self-review

Checklist of CLAUDE §6.7 (with `/engineering:code-review` on the branch diff); findings sorted per §6.8.

- **fix (done):** R1 — `CLAUDE.md` §4 and §9 still named `withProposal()` and the proposal form as the example of server field errors. R2 — the new migration test wrote its JSON snapshot through `ExecuteSqlRawAsync`, which reads `{…}` as format placeholders (found by the first integration run); it uses a plain `NpgsqlCommand` now. R3 — the client models `create-product.dto.ts` and `create-criterion.dto.ts` had no importer left.
- **won't fix:** R4 — `products-container.ts` keeps `RouterModule` in its imports though its template has no `routerLink` left; legacy, replaced in step 6, and Angular does not flag an unused NgModule. R5 — a session started before the migration keeps `moderation:review` in its principal until the principal is renewed; no endpoint or screen needs the permission any more.
- **follow-up:** see §5, points 2 – 4.
- **Correctness:** the product and criteria reads lose a filter that was always true for stored rows (F3). The sign-in no longer starts a legacy read, so it cannot report a failure of it either. `startSession` stays as the store's sign-in entry point (`login-form.ts`).
- **Security:** no endpoint left that a permission guarded; the remaining legacy controllers are the read-only generic ones (ARC-05) and `ProductsController`. Antiforgery exemption list unchanged. No secret, no new dependency (one removed).
- **Data:** `RetireProposals` drops rows by decision O-S6-4 (a); products and criteria keep theirs (V317). The role migration touches one claim row per database.
- **Accessibility:** the main navigation has two items; the removed buttons were the only place of #350's mismatch (already closed). No new UI text.

## 4. Checks

- `dotnet build ProcureAccess.sln -warnaserror …`: 0 warnings, 0 errors. `dotnet format --verify-no-changes`: green.
- Unit tests: 793 passed. Architecture tests: 67 passed (with ARC-05). Integration tests (Testcontainers, PostgreSQL 17): first full run 824 of 825 (R2); after the fix the persistence classes 25 of 25 and the full suite again: see the pull request.
- `dotnet ef migrations has-pending-model-changes` for `ApplicationDBContext` and `IdentityAccessDbContext`: none.
- Client: `npm run lint` green; `ng test` 87 files passed; `npm run build:release` without warnings (EasyMDE's critical-CSS warning is gone); `tsc` for app and specs clean.
- `e2e/`: `npm run typecheck` and `npm run lint` green. **E2E not run locally:** another session uses the local E2E stack at the same time (shared ports); E2E is left to CI (`reflow.spec.ts`, `start-and-error-pages.spec.ts` changed).
- `check-invariants` (TESTING_v92, ARC-05 active), `check-figma-links`, `check-i18n`, `check-icons` and every `scripts/*.test.mjs`: green. `reuse lint-file` on the new files: compliant (a full `reuse lint` in this worktree scans build output, because the worktree's `.git` file points outside the container).

## 5. Open points

1. **#82** (commented on the issue): Z2 (`ProposalList`) went with this task; Z4 (`Profile`) and Z5/R1/R2 (`Settings`, legacy `Register`) went with S5-28b, S5-25b and S02; R3 is covered by `with-identity.spec.ts` (`signOut()` leaves `permissions()` empty). Left: Z1 (`ProductsList`), Z3/R5 (`ProductDetails`) and R4 (`withProducts` race) — all on the legacy product screens that the directory rebuild of step 6 replaces.
2. **Follow-up (S6-10a open point 5):** `Permissions` and `CustomClaimTypes` stay in MODELS. Production code no longer reads them (only the API's global usings and the tests do); moving them into IdentityAccess's contracts or dropping them for a constant there is a small change of its own, outside this row.
3. **Follow-up (S6-10a open point 4):** `SERVICES.ProcureAccess.csproj` still references `Microsoft.AspNetCore.Identity.EntityFrameworkCore` and its global usings name Identity namespaces; not touched here (package and notices change of its own).
4. **Dead code found, not from this task:** the server DTOs `UpdateProductDto`, `UpdateCriterionDto` and the client model `products/models/update-product.dto.ts` had no user before S6-23 either; they go with the product rework (step 6).
5. **S6-13:** the rework of `withProducts()` / `withFavorites()` into `withWorkspace()` moves there (D2 (a)); its row in `SPRINT_06.md` does not say so yet — proposed for the owner's next sprint-plan update.
6. **Step 6:** „Beitragen“ returns to the main navigation with S53 (`/beitragen`); until then the header differs from the Figma frame (state Anonymous) by that item. `ProductsController` leaves the ARC-05 exception when the directory replaces it. The moderation of step 7 brings its own policy for `moderation:review`.
7. **TECH_DEBT:** TD-61 and the §5 rows „`ProposalService` …“, „`DateTime.UtcNow` ×1“, „the tagged API-02 test …“, „Two legacy `LiveAnnouncer` calls“ (one left: `products-list.ts`), „`withProposal()` …“ and the EasyMDE warning are done; they are struck in the next TECH_DEBT version (sprint-end task, process step 14).

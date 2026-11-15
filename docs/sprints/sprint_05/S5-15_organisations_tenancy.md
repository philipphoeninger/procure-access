# S5-15 — Organisations: domain, query filters, isolation tests

> **Sprint:** 05 · **Task:** S5-15 (4 points) · **Issues:** none
> **Requirements / invariants:** SPEC R-ORG-05 (the authorisation model; its endpoints come with S5-16 and the workspace steps); **TEN-01, TEN-02 `active`, TEN-05 `active` as the skeleton** (TESTING_v65 V207–V209); ARCHITECTURE_v58 C144, ADR-007 accepted; TESTING §6.2 Tenants fixture in part (V210)
> **Status:** done — pull request #340 open (2026-11-15); CI result to be recorded
> **Scope:** `Organization` and `OrganizationMembership` (Owner / Editor / Reader) in the IdentityAccess module with the migration; the EF global query filter on `OrganizationId`, bound to the memberships of the request; the resource-based role requirement; the reflection-driven isolation test, the `IgnoreQueryFilters()` allow-list test and the authorisation-matrix skeleton. No endpoint, screen or text.
> **Owner decisions (2026-11-15, before the task, all as recommended):** 1 (a) — `IOrganizationContext` is declared in `Platform.Contracts` and implemented by IdentityAccess (the pattern of `IShareableResourceProvider`; Platform's own `ShareLink` is scoped) · 2 (a) — the filter is bound to the set of the signed-in user's memberships, loaded once per request, never to a header or a stored „current organisation“ · 3 (a) — TEN-05 starts on a test-only probe controller until S5-16 adds the real endpoints · 4 — `OrganizationMembership` is itself under the filter; the membership loader is the first allow-listed bypass. **O-S5-5 (O-22) was not needed:** no `Contracts` project references another.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | ARCHITECTURE §5.4 binds the filter to an `IOrganizationContext` „resolved per request“ but does not say where it lives or what it holds; §9.1 says the organisation comes from the resource, never from a header | Decisions 1 and 2: the interface in `Platform.Contracts` with the set of membership ids; for `/configurations/{id}` the organisation is known only after the load, so a per-request set is the only reading that works without a header |
| 2 | ADR-007 (query filters) was still „Proposed“ | Accepted with the rules of C144 instead of a new ADR |
| 3 | No entity anywhere carried `OrganizationId`; the only scoped entities after this task are `OrganizationMembership` and `Organization` itself (scoped by its own id) | The TEN-01 discovery finds both; the behavioural test has one sample per type and fails when a new scoped entity has none |
| 4 | `IdentityAccessDbContext` was constructed directly in the fixture, the design-time factory and six tests | They pass `FixedOrganizationContext.None` (no request, no organisation) |
| 5 | The legacy repositories call `IgnoreQueryFilters()` three times (soft-delete bypass of the legacy context, which has no organisation-scoped entity) | Allow-listed with that reason; the scan looks for the call `.IgnoreQueryFilters(`, because the legacy repositories also have methods named after it |
| 6 | Platform's `ShareLink` (S5-23) will carry `OrganizationId`, and Platform depends on no module | Only a declaration in `Platform.Contracts` lets Platform's context use the filter without changing the dependency table (decision 1) |
| 7 | No open pull request touched IdentityAccess | The migration `AddOrganizations` had a free branch |

## 2. Changes

### 2.1 Contracts

- `src/ProcureAccess.Modules.Platform.Contracts/Tenancy/IOrganizationContext.cs`: `OrganizationIds`, the organisations the request may see. `FixedOrganizationContext.None` / `.Of(ids)` for contexts outside a request and for tests.
- `src/ProcureAccess.Modules.IdentityAccess.Contracts/Organizations/`: `OrganizationRole` (Reader < Editor < Owner), `OrganizationType`, `IOrganizationResource` with `OrganizationReference` (the route's organisation as a resource), `OrganizationRoleRequirement` (`Reader`, `Editor`, `Owner`; `For(role)`), `OrganizationAccess` (failure reasons `NotAMember`, `InsufficientRole`; `StatusOf(result)` → `null`, 404 or 403). The contracts project references the shared framework for `IAuthorizationRequirement` — a `FrameworkReference`, not a project reference, so ARC-03 is unchanged.

### 2.2 IdentityAccess module

- `Domain/Organization.cs` (random `Guid` id, name ≤ 120, type, `CreatedByUserId`, `CreatedAt`; `Create` validates) and `Domain/OrganizationMembership.cs` (key organisation + user, role, `InvitedAt`, `AcceptedAt`; `Create` and `Founder`, the creator's accepted Owner membership).
- `Persistence/Configurations/OrganizationConfiguration.cs`, `OrganizationMembershipConfiguration.cs`: tables `identity."Organizations"`, `"OrganizationMemberships"`; the membership's foreign key cascades, `UserId` has none (ADR-014) and an index for the loader. Migration `AddOrganizations` (made `internal`, summary added).
- `Persistence/IdentityAccessDbContext.cs`: takes `IOrganizationContext`, keeps it in a field (EF parametrises a member chain rooted at the context per query), binds `HasQueryFilter` of `Organization` (own id) and `OrganizationMembership` (`OrganizationId`) to `OrganizationIds.Contains(...)`. The design-time factory passes `FixedOrganizationContext.None`.
- `Tenancy/OrganizationContext.cs`: the request-scoped implementation with the roles by organisation (`RoleIn`, `Load`, `IsLoaded`); registered as itself and as `IOrganizationContext`.
- `Tenancy/OrganizationContextMiddleware.cs`: after authentication, one indexed query of the signed-in user's memberships with `IgnoreQueryFilters()` (the allow-listed call site); an anonymous request gets an empty set without a query. Read on every request, not from a claim (R-ORG-03: a removed member loses access at the next request).
- `Tenancy/OrganizationRoleHandler.cs`: `AuthorizationHandler<OrganizationRoleRequirement, IOrganizationResource>` on the loaded memberships; fails with `NotAMember` or `InsufficientRole`, decides nothing for an anonymous principal.
- `IdentityAccessModule.cs`: the registrations and `UseIdentityAccessOrganizationContext()`; `API/Extensions/IdentityExtensions.cs` calls it inside `AddIdentityAuthMiddlewares` between `UseAuthentication` and `UseAuthorization`.

### 2.3 Tests

- `tests/ProcureAccess.Tests.Shared`: `Auth/TestUsers.cs` gets `OwnerA`, `EditorA`, `ReaderA`, `OwnerB`, `EditorB`, `ReaderB` (no roles or permissions) and documents `Member` as the user without organisation; `Fixtures/Tenants.cs` builds organisations A and B with six memberships (`Entities` for `StoreAsync`). The project references the IdentityAccess module (its internals are visible to Shared).
- Unit: `IdentityAccess/OrganizationTests.cs` (entities), `IdentityAccess/OrganizationRoleHandlerTests.cs` (handler, `StatusOf`, `For`, `OrganizationContext.Load`).
- Architecture: `Tenancy/IgnoreQueryFiltersAllowListTests.cs` (`INV:TEN-02`): scan of `src/`, `API/`, `DAL/`, `SERVICES/` (no `bin`/`obj`, comments skipped) for `.IgnoreQueryFilters(`, compared with the allow-list and its counts; a listed file must exist. `ProjectGraph.FindRepositoryRoot` became `internal` for it.
- Integration: `Tenancy/OrganizationIsolationTests.cs` (`INV:TEN-01`): discovery over the models of the legacy, Platform, Catalog and IdentityAccess contexts (built without a database for the theory data), the declared filter of every scoped type, and the behavioural check through the DI scope's `OrganizationContext` — one row of A per type, read as B (0), A (1), both, none. `Tenancy/AuthorizationMatrixTests.cs` (`INV:TEN-05`, `INV:TEN-01`): an own host with the test-only `OrganizationProbeController` (read = Reader, edit = Editor, owner = Owner, a filtered membership list), the 18-cell matrix, three header cases, the membership list for A, B and the user without organisation.
- The fixture and six tests that construct `IdentityAccessDbContext` directly pass `FixedOrganizationContext.None`.

### 2.4 Documents

- TESTING_v65 (V207–V210): TEN-01, TEN-02 `active`, TEN-05 `active` (skeleton; the real endpoints deferred to S5-16 and the workspace steps), §6.2 Tenants as built; TESTING_v63 deleted.
- ARCHITECTURE_v58 (C144): §4.3 Platform and IdentityAccess rows, §5.2, §5.4, §9.1, §12 step 3, ADR-007 accepted; ARCHITECTURE_v56 deleted.
- SPEC v2.26: the current-versions line and one change row.
- CLAUDE.md: version line, §3 (S5-15 paragraph), §4 (Platform.Contracts, IdentityAccess.Contracts, IdentityAccess, Tests.Shared, Architecture/Tenancy, Integration/Tenancy), §7 (tenant isolation as built).
- No Figma mapping, UX copy, `de.json` or `LEGACY_EXCEPTIONS` change: no screen or text.

## 3. Self-review

Checklist of CLAUDE §6.7 (security, correctness, N+1, edge cases, a11y) over the whole diff; findings sorted per §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Can a request see another organisation's rows? The filter reads the context's field at query time (the behavioural TEN-01 test switches the set between scopes and sees the rows change); the set comes from the database, never from a header (header cases in TEN-05); anonymous requests, command hosts and fixtures hold an empty set, so a forgotten load fails closed | — |
| R2 | Every signed-in request pays one indexed query for the memberships, also a catalog read that needs none | **won't fix** — accepted in C144 (expected load §11.1; the security-stamp check of S5-29 reads per request too); a skip by endpoint metadata is a possible later optimisation |
| R3 | `OrganizationAccess.StatusOf` answers 404 for any failure that is not only `InsufficientRole`, also when a caller combines the requirement with another one in one `AuthorizeAsync` | **won't fix** — documented as the answer for this requirement alone; 404 is the safe default |
| R4 | A client that disconnects while the middleware queries gets an `OperationCanceledException` from `RequestAborted`, as every endpoint's query does | **won't fix** — no response is read by an aborted request |
| R5 | TEN-02 scans only the `IgnoreQueryFilters` call, as the invariant says; a raw SQL query (`FromSql`, `SqlQuery`) could bypass a filter too | **follow-up** (open point 2): extend the scan when a module first uses raw SQL |
| R6 | `OrganizationContext.IsLoaded` is read only by tests | **won't fix** — a one-line diagnostic for the tests of the middleware order |
| R7 | No new NuGet package, no secret, no N+1 (one query per request, none per row), no UI | — |

No correctness or security finding that affects current behaviour.

## 4. Checks

- Backend: build with `-warnaserror` clean; `dotnet format --verify-no-changes` clean; `dotnet test --solution` 1167 passed (60 new, in Unit, Architecture and Integration).
- `node scripts/check-invariants.mjs` (TESTING_v65: TEN-01 5 tests, TEN-02 1, TEN-05 3), `check-figma-links`, `check-i18n` passed; `reuse lint` compliant.
- OPS-03 for IdentityAccess (`IdentityAccessMigrationTests`) green with the new migration; no E2E change (no screen).

## 5. Open points

1. S5-16: the TEN-05 matrix moves onto the real organisation endpoints (`POST /organizations`, members, invitations); the probe controller then goes. S5-18's organisation switch is a list scope, not a security boundary (C144).
2. Extend the TEN-02 scan to raw SQL (`FromSql`, `SqlQuery`) when a module first uses it (R5).
3. S5-19 (configurations) and S5-23 (share links): each scoped entity gets the filter in its context and a sample in `OrganizationIsolationTests.Samples`; the share-link anonymous read is the next allow-list entry. S5-19 needs O-S5-5 for `IConfigurationReader` with Catalog types.
4. The Tenants fixture is partial: contributor profiles of the users without organisation and the `CatalogEditor`/`PlatformAdmin` users come with their steps; E2E organisation users with S5-16.
5. M3.1: when the users move into IdentityAccess, `Organization.CreatedByUserId` and `OrganizationMembership.UserId` can get their foreign keys.

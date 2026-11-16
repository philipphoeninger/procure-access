# S5-23 — Share links: model, management API, anonymous read

> **Sprint:** 05 · **Task:** S5-23 (M3.5, 3 points) · **Issues:** none
> **Requirements / invariants:**
> - **TEN-08 → `active`** except „a project copy → 409“ (TESTING_v80 V269);
> - **TEN-12 → `active`** on the server (V270);
> - TEN-09 server part tested, `active` with S16 in S5-24 (V271); SEC-10 gains creating and revoking a link (V271);
> - SPEC R-SAV-04, R-SAV-05, R-SAV-06, SD15, §8.5, §8.10 (v2.40); ARCHITECTURE C158.
>
> **Status:** done — pull request #360 open (2026-11-16)
> **Scope:**
> - `ShareLink` in Platform with the organisation filter and only the hash of its token;
> - creating, listing and revoking for Editor and Owner;
> - `IShareableResourceProvider` declared in Platform, implemented by Configurator;
> - the anonymous `GET /api/v1/shared/{token}` with the redacted list;
> - the token masked in the log sinks and the client's access log.
>
> **EF migration:** Platform, `AddShareLinks` (table `sharing."ShareLinks"`). Configurator has none.
> **Owner decisions (2026-11-16, before the task):**
> - **1a** — the token stays in the path (`/geteilt/<token>`, `GET /api/v1/shared/{token}`); the nginx access log and the API's log sinks mask it; CLAUDE §3 names share links as the exception to the link-token rule.
> - **2a** — the API takes a calendar date from tomorrow to today + 1 year (Europe/Berlin); the link works until the end of that day, stored as a UTC instant.
> - **3a** — creating and revoking need a confirmed address (SEC-10).
> - **4a** — `GET /api/v1/configurations/{id}/share-links` for Editor and Owner (403 Reader), active links only, never a token.
> - **5** — the migration in Platform (ARCHITECTURE §5.3), not in Configurator as the sprint plan said; the sprint documents corrected.
> - **6** — no `Scope` column.
> - **7** — no rate limit on creating or reading.
> - **8** — a deleted list's links answer 404 through the read; the rows stay until organisation deletion.
> - **9** — „a project copy → 409“ deferred to migration step 5, recorded in TESTING.
> - **a** — the shared list in the shape of `GET /configurations/{id}` without its ids and dates, plus `expiresAt`; SPEC §8.5 amended.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | CLAUDE §3: link tokens travel only in POST bodies and never in logs; SPEC §8.5 and ARCHITECTURE §7.2 put the share token in the path, and the client's nginx `no_query` format logs `$uri` | Owner decision 1a |
| 2 | The UX copy has a date picker („Gültig bis“) and „zwischen morgen und {{maxDate}}“; SPEC says „at most 1 year after creation“ | Owner decision 2a |
| 3 | SEC-10 lists saving, organisations, invitations, contributor profiles and publishing, not share links | Owner decision 3a |
| 4 | ARCHITECTURE §7.2 had create and revoke, but S15 lists the active links | Owner decision 4a: a list endpoint |
| 5 | SPRINT_05 and TASK_PROMPTS planned a Configurator migration; ARCHITECTURE §5.3 and C44 put `ShareLink` in Platform | Owner decision 5 |
| 6 | SPEC §8.5 (written before S5-20a) embeds the catalog texts; the built saved-list read returns ids and decisions and S11 joins the texts | Owner decision a |
| 7 | Platform depends on no module, so it cannot check an organisation role (`IdentityAccess.Contracts`) | The management endpoints are Configurator's; Platform keeps the table, the service and the anonymous read |
| 8 | Platform's context had no organisation filter, and the `catalog` command host registers no IdentityAccess module (so no `IOrganizationContext`) | `PlatformDbContext` takes `IOrganizationContext`; the catalog host registers `FixedOrganizationContext.None` |
| 9 | ASP.NET Core puts the request path into the log scope of every event (`RequestPath`), and the PostgreSQL sink stores all properties | A Serilog enricher masks the token before any sink writes |
| 10 | No project copies exist before migration step 5 | The 409 of TEN-08 is deferred (decision 9) |

## 2. Changes

### 2.1 Platform

- **Contracts** `Platform.Contracts/Sharing/`:
  - `ShareableResourceType` (`Configuration`);
  - `IShareLinks` (create, list active, find, revoke) with `ShareLinkCreation`, `CreatedShareLink`, `ActiveShareLink`, `ShareLinkTarget`;
  - `IShareableResourceProvider` (`ReadSharedAsync(resourceId, expiresAt)`).
- **Model** `Sharing/ShareLink.cs`:
  - `OrganizationId`, `TokenHash` (SHA-256), `ResourceType`, `ResourceId`, `CreatedAt`, `ExpiresAt`, `RevokedAt?`;
  - `IsActiveAt`, and `Revoke`, which keeps the first revocation.
- **Persistence:**
  - table `sharing."ShareLinks"` with a unique index on the hash and an index on type and resource;
  - migration `AddShareLinks` (internal, as the module's other migrations);
  - `PlatformDbContext` takes `IOrganizationContext` and filters `ShareLinks`. The design-time factory and `UsageEventStore` pass `FixedOrganizationContext.None`.
- **Service** `Sharing/ShareLinkService.cs`:
  - 32 random bytes in Base64url;
  - the expiry check in the configured zone, and the next midnight there as `ExpiresAt`;
  - `ReadSharedAsync(token)`: anything that is not 43 characters is refused before decoding; the lookup by hash bypasses the filter (TEN-02 allow-list); revoked or expired give `null`; then the provider of the link's type.
- **Settings** `Sharing/ShareLinkSettings.cs`: 🔧 `ShareLinks:MaxExpiryMonths` (12) and `ShareLinks:TimeZone` (Europe/Berlin) in `appsettings.json`, read once at the start. An impossible value or an unknown zone stops the start. The runtime image has tzdata (checked).
- **Endpoint** `Sharing/SharedEndpoints.cs`:
  - `GET /api/v1/shared/{token}`, anonymous;
  - one 404 `not-found` for every failure;
  - `no-store` from the default;
  - mapped by the new `PlatformModule.MapPlatformApiEndpoints()` on the versioned group.
- **Logs:**
  - `Logging/ShareTokenMaskingEnricher.cs` replaces `/api/v…/shared/<token>` (any case) by `/api/v…/shared/***` in every text property;
  - it runs after the logging provider has added the request scope, so `RequestPath`, the hosting events' `Path` and the rendered message are masked (mutation check: the sink test fails without it).

### 2.2 Configurator

- `Sharing/ShareLinkEndpoints.cs`:
  - `POST /configurations/{id}/share-links` `{ expiresOn }` → 201 `{ id, token, expiresAt }`;
  - `GET /configurations/{id}/share-links` → the active links;
  - `DELETE /share-links/{id}` → 204;
  - the organisation from the list or the link through the filter (404), then Editor (403), `ConfirmedEmail` on POST and DELETE, antiforgery from the pipeline.
- `Sharing/ConfigurationShareProvider.cs`: reads the list past the filter (TEN-02 allow-list) and answers `SharedConfigurationResponse`.
- `ConfigurationService.SelectionOf`/`DecisionsOf` extracted, so the saved-list read and the shared read share one mapping.
- `ConfigurationEndpoints.RefusedAsync`/`RefusedForListAsync` are now `internal`.
- `CreateShareLinkBody` refuses unknown members.

### 2.3 Host and client image

- `Program.cs` maps `MapPlatformApiEndpoints()`.
- `CatalogCommandHost` registers `FixedOrganizationContext.None`.
- `CLIENT/CLIENT.ProcureAccess/nginx.conf`:
  - maps `$log_uri` and `$log_referer` mask the token in `/api/v…/shared/…` and `/geteilt/…`;
  - an empty referrer still logs `-`;
  - checked with `nginx:1.30-alpine`: `/api/v1/shared/***` and `…/geteilt/***?x=1` in the log. The page itself was already logged as `/index.html` after `try_files`.

### 2.4 Tests

- `Configurator/ShareLinkTests.cs` (20 cases):
  - the token once and only its hash stored (TEN-08);
  - the redacted read without any cookie with sentinels (TEN-08, TEN-09);
  - revoked, expired, deleted list, unknown and malformed → 404, and still 200 a second before the end (TEN-08);
  - the expiry with the clock at 23:30 UTC: missing, yesterday, today and one year and a day → 400 at `expiresOn`; tomorrow and one year accepted; `expiresAt` at Berlin midnight (TEN-12);
  - unknown member → 400;
  - only active links of this list (TEN-01);
  - another organisation's link → 404 on revoke (TEN-01);
  - revoking twice → 204 with the first revocation kept;
  - SEC-10 on creating and revoking, with success after confirming.
- `Operations/LogSinkTests`: the path in the scope and in a message property is stored masked (TEN-08).
- `OrganizationIsolationTests`: a `ShareLink` sample (TEN-01).
- `AuthorizationMatrixTests`: rows `create-share-link`, `list-share-links`, `revoke-share-link` (TEN-05, and `no-store` through TEN-04).
- `IgnoreQueryFiltersAllowListTests`: two entries.
- Eight test call sites pass `FixedOrganizationContext.None` to `PlatformDbContext`.

### 2.5 Documents

- TESTING v80 (V269 – V271; v78 deleted).
- ARCHITECTURE v72 (C158; §4.3, §5.3, §7.2, §9.5; v70 deleted).
- SPEC v2.40 (§8.5, §8.10, change rows v2.40-1 – 4).
- CODEBASE §2.73 and §3; CLAUDE.md §3 (state, the share-link exception to the token rule).
- SPRINT_05 and TASK_PROMPTS (the migration's module).

## 3. Self-review

Checklist of CLAUDE §6.7 and `/engineering:code-review` on the staged diff.

| # | Finding | Sorting |
|---|---|---|
| R1 | nginx's error log writes the request line when the upstream fails, and Traefik's access log is outside the repo: during an API outage a share token can land there | **open point** 1 |
| R2 | The enricher masks scalar string properties only; a scope that is not a key/value list becomes a `Scope` sequence | **won't fix**: no ASP.NET Core scope carries the path that way; the sink test covers `RequestPath` and `Path` |
| R3 | `Produces<object>()` on the shared read: Swagger shows no schema, because Platform cannot name Configurator's DTO | **won't fix**: SPEC §8.5 documents the shape |
| R4 | S15 needs the 90-day default and the 12-month maximum on the client | **follow-up**: S5-24 (open point 2) |
| R5 | A deleted list leaves its link rows | **follow-up**: S5-28 with the deletions (decision 8; open point 3) |
| R6 | A malformed JSON body answers 400 before the 404/403 | **won't fix**: as every body endpoint (API-01) |
| R7 | Security | **checked:** <ul><li>the token is never stored or returned again;</li><li>a fixed-size buffer for decoding;</li><li>one 404 for every failure of the read;</li><li>the organisation from the stored resource, never from the client;</li><li>both bypasses allow-listed with their reason;</li><li>antiforgery and the confirmation gate on both changes.</li></ul> |
| R8 | N+1 and indexes | **checked:** the read is one indexed lookup plus one read of the list; the list endpoint uses the index on type and resource |
| R9 | a11y | no UI in this task |

## 4. Checks

- **Backend:**
  - `dotnet format --verify-no-changes` clean;
  - `dotnet build -warnaserror` clean;
  - `dotnet test --solution` **1 634 passed** (unit, architecture, integration);
  - `dotnet ef migrations has-pending-model-changes` clean for Platform and Configurator.
- **Client:** only `nginx.conf` changed, checked in an `nginx:1.30-alpine` container (above). No client code changed, so no `ng` run.
- **E2E:** not run; nothing in the E2E stack's behaviour changed (the log format only). CI runs it.
- **Scripts:**
  - `check-invariants` (TEN-08: 5 tests, TEN-12: 1; a warning for the planned TEN-09 tag);
  - `check-figma-links`;
  - `check-i18n`;
  - `check-icons`;
  - `reuse lint` (through the container).
- **Figma:** no screen in this task.

## 5. Open points

1. **Logs outside the API:** nginx's error log (upstream failures) and the Traefik access log of the deployment can hold a share token. The first needs `error_log` at `crit` or a log pipeline that masks; the second a Dokploy/Traefik setting. Proposed issue for the staging deployment (#236).
2. **S5-24:** S15 needs the default (90 days) and the maximum (12 months) for its date picker and `criteria.share.expiry.error`'s `{{maxDate}}`. Proposal: the settings endpoint serves them, as the weight range (CLAUDE §3), with 🔧 `ShareLinks:DefaultExpiryDays` added then. S16 reads `GET /api/v1/shared/{token}` and joins the texts as S11 does; TEN-09 flips with it.
3. **S5-28:** deleting an organisation or the last member's account should delete its link rows (decision 8).
4. **Migration step 5:** the 409 for a project copy (TEN-08 deferred part, C59).
5. **TECH_DEBT:** none added.

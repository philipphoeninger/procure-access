# ProcureAccess — System Architecture

> **Status:** v14 — output of Phase 2, step 4 ("System design") of `ai_development_process.md`, amended during step 5 ("Architecture diagrams"), step 7 ("UI & Frontend design") and step 8 ("Write the feature spec"). Supersedes v13. v14 (2026-10-29) records how sprint 01 built logging and the usage-event table ahead of step 1 (tasks S1-22, S1-23; `TECH_DEBT_v1.md` TD-11, issue #123). v13 (2026-10-27) records the owner's licence rule for dependencies (C89, D12): shipped dependencies must not use a strong copyleft or reciprocal licence; it follows the AutoMapper finding of sprint 01, task S1-03 (`docs/sprints/sprint_01/S1-03_backend_inventory.md` §4). v12 records the owner's confirmation of the SPEC proposals on 2026-10-19 (`SPEC_v2.md` SD19–SD23): catalog releases from data files without an editor UI, minimal administration of product identities (the owner holds `PlatformAdmin`), removal of candidates, supported browsers and all 🔧 default values; O-17–O-20 are closed. v11 incorporated the follow-ups of `SPEC_v1.md` §14 (2026-10-15): e-mail delivery through Lettermint (SMTP), e-mail confirmation and sign-in lockout, the dark theme in v1, the final shapes of the filter-options and criteria responses (O-8 closed), problem types, the personal data export, organisation and account rules, catalog import from data files, administration of product identities, removal of candidates and the final list of usage events (O-15 closed). v10 incorporated the review of the main documents before `SPEC_v1.md` (`pruefung_hauptdokumente_v1.md`, 2026-10-13): the owner's decisions D1–D7 (project copy of the criteria list, vendor entries without assessments, republication only by the original profile, content purge on consent withdrawal, context of experience reports, re-pinning, share links) and all findings of the consistency audit (A) and of the architecture and testing review (B), among them the session implementation, module projects, idempotency and the version reference in moderation. v9 recorded the session timeouts (O-12: 30 minutes idle, 8 hours absolute). v8 recorded the owner's decision on the session model (O-11: ASP.NET Core Identity cookie authentication with antiforgery) and clarifies what the locked criteria list allows. v7 incorporated the architecture review of 2026-10-12 (`architektur_review_v6.md`) and the owner's decisions on it: .NET 10, stable entries with immutable versions, stored assessment scope, module contracts, release-pinned procedure mapping, several test evidences per product, locked criteria lists, consent for every experience report. v6 incorporated the consistency audit before `SPEC_v1.md` (2026-10-12): the "tested according to" criterion type, the allowed-ratings union, app types and use cases on published entries (variant A), experience reports without counts, API contracts for multi-selection, and Angular 22. v5 added the catalog changes from the design review of 2026-10-06 (filter layers, exclusion rules, test types, BIT inklusiv rating scale, criterion texts per test procedure) and decision E13 (experience reports carry no test status). v4 recorded the owner's decisions on all three open questions of §16.1 and §16.3 and three further decisions that surfaced while writing the UX copy (step 7b).
> **Input documents:**
> - `prd_procureaccess_werteit_v5.md` (authoritative product scope)
> - `brainstorm_validierung_procureaccess_werteit.md` (decision history)
> - `anforderungen_beschaffungstool_procureaccess_werteit_v2.md` (original requirements)
> - `CLAUDE.md` (state of the existing codebase)
> - `ux_copy_procureaccess_v1.md` (step 7b — screen inventory, domain components, UI terminology); current version `ux_copy_procureaccess_v14.md`
> - `design_handoff_procureaccess_v1.md` (v1.14) and `figma_code_mapping_procureaccess_v1.md` (v1.11, step 7f/7h replacement)
> - `kriterienkatalog_bit_inklusiv_v1.md` (1.5, + parts A and B) — criteria catalog based on BIT inklusiv, filter layers, exclusion rules, rating scale (German)
>
> **Diagrams:** FigJam board *ProcureAccess* — 01–09 architecture (component overview and module dependencies, Flows A–D, domain model per schema, migration path), 10–15 decision trees for §15 and §16, 16 persona flow map (PRD §8 user stories → flows and endpoints), 17 Flow E (direct Kanal-1 publication, new in v4). All boards were updated to v4 on 2026-10-02. On 2026-10-12, v7 versions of boards 01–09 and 17 were placed to the right of the originals (titles "NN (v7) …"); the originals stay as history and are marked "superseded". They cover four filter layers, procedure criteria and mapping, entry/version split, full assessment scope, test evidences, context, consent, module contracts, .NET 10 and Angular 22. Boards 10–16 (decision trees, persona flow map) remain at v4; their decisions are recorded in §16.
>
> **Language note:** Written in English per project convention. Domain terms are kept in German where they are the established project vocabulary (Kriterium, Workspace, Kanal 1/2, Vergabeunterlagen), with English identifiers at code level. User-facing terms are defined in the UX copy §2 (current: `ux_copy_procureaccess_v14.md`) and differ deliberately from these (e.g. "Kanal 1" is never shown in the UI).
>
> - `architektur_review_v6.md` (architecture review of v6, German; findings R1–R6, Y1–Y8, G1–G4)
> - `pruefung_hauptdokumente_v1.md` (review of the main documents before `SPEC_v1.md`, German; owner decisions D1–D7, findings A-F1–A-F25 and B-F1–B-F19)
> - `SPEC_v2.md` (feature specification; v1 of 2026-10-15 with decisions SD1–SD18 and follow-ups §14; v2 of 2026-10-19 with decisions SD19–SD23 confirming the proposals O-S1–O-S4 and O-S6)
>
> **Not covered here:** detailed UI/UX design (step 7), test strategy (step 6, `TESTING_v10.md`), feature-level acceptance criteria (step 8, `SPEC_v2.md`). Decisions marked **→ ADR** are candidates for formal decision records.

### Changes from v13

| # | Change | Driver |
|---|---|---|
| C90 | **Logging as built (§9.5):** the sink is the package `Serilog.Sinks.Postgresql.Alternative` (MIT), not a sink of our own; it writes to `logging."SeriLogs"` in batches with a bounded queue. The **migrations own the table** — the sink never creates it — so every environment starts with the same schema. Retention is an **own job** (`LogRetentionJob`) on `TimeProvider`, not the sink's `retentionTime`, which runs on the system clock and could not be tested with `FakeTimeProvider`. Retention 90 days and run interval 1 day are read from `AppLoggingSettings:Retention`; the 90 days are a 🔧 default since SPEC v2.1 (SD24). The job assumes one API instance, as the startup migrations do | Owner decisions D1, D4 of sprint 01, S1-22 (2026-10-25); S1-22 F7 |
| C91 | **Usage events as built (§5.7):** table `analytics."UsageEvents"` with a surrogate key `Id` (EF needs one), a **random UUID v4** set by the database (`gen_random_uuid()`), so the key does not show the order of events within a day (owner, 2026-10-29, #124; never an identity or a time-ordered UUIDv7); `OccurredOn` is the **UTC** day from `TimeProvider`. `IUsageEventWriter` has one typed method per event of SPEC_v2 §9.1. The writer **awaits** the insert in a DI scope and `DbContext` of its own and logs and swallows every failure, so a failure never fails the caller; there is no queue or background service. SPEC R-MET-01 says "fire-and-forget"; whether the request should also not wait (and not retry) is open in #115. | Owner decisions D2, D3 of sprint 01, S1-23 (2026-10-25); key: owner, 2026-10-29 (#124, PR #128); S1-23 P1, P3 |

### Changes from v12

| # | Change | Driver |
|---|---|---|
| C89 | **Licence rule for dependencies** (owner, 2026-10-27): D12 now states the rule for all dependencies, not only Excel. Shipped dependencies: OSI-approved, no commercial-use restriction, no strong copyleft or reciprocal licence; weak, file-level copyleft allowed if used unmodified; a dual licence counts by the option the project would use. Development and test tools that are not shipped: OSI-approved, no commercial-use restriction. Before, the rule only said "OSI-approved, no commercial-use restriction", which does not exclude RPL-1.5 (AutoMapper 15+) | S1-03 inventory §4, review of PR #32; CLAUDE §2 (no silent conflict resolution) |

### Changes from v11

| # | Change | Driver |
|---|---|---|
| C87 | **SPEC proposals confirmed** (owner, 2026-10-19): C74 catalog releases from data files, no editor UI for now (O-17 closed); C75 minimal product-identity administration, `PlatformAdmin` held by the owner in operation (O-18 closed); C76 removing a candidate, published entry stays (O-19 closed); supported browsers per SPEC §7.4 (O-20 closed). *Proposal* markers removed (§7.2); decisions recorded in §16.4.16 | SPEC_v2 SD19–SD22 |
| C88 | **🔧 default values confirmed** (password length, lockout, token and invitation lifetimes, staleness, rate limits §9.2); they stay configurable | SPEC_v2 SD23 |

### Changes from v10

| # | Change | Driver |
|---|---|---|
| C71 | **E-mail delivery.** `IEmailSender` in Platform, implemented with MailKit (MIT) over SMTP with TLS; production provider **Lettermint** (EU-hosted, SMTP relay; credentials only from environment variables); Mailpit (MIT) as SMTP catcher in Development and E2E. Message types: address confirmation, password reset, „already registered“ notice, organisation invitation, product-identity request accepted/rejected. Open and click tracking are switched off in the provider project; recipient addresses are not logged above `Debug`. Sent synchronously with one retry (§2.1, §4.3, §9.1, §9.3, §10.1) | SPEC SD13, §7.6 |
| C72 | **Account security.** E-mail confirmation (Identity `RequireConfirmedEmail` for the gated actions, not for sign-in): saving lists, creating organisations, accepting invitations, creating contributor profiles and all publication paths require a confirmed address (403 Problem Details `email-unconfirmed`). Lockout after 5 failed sign-ins for 15 minutes; password at least 12 characters; confirmation link 24 h, reset link 2 h, invitation 14 days, all single use and stored hashed (§9.1) | SPEC R-AUTH-01–04, R-ORG-02 |
| C73 | **Dark theme in v1.** `UserPreferences.Appearance` (`System` / `Light` / `Dark`); anonymous users follow `prefers-color-scheme`. Pure client concern otherwise (§5.2) | SPEC SD11 |
| C74 | **Catalog releases from data files** (*proposal O-S1*, confirmed in v12, C87): a release is a versioned folder in the repository (`catalog/releases/<version>/`, JSON); CLI commands `catalog import <version>` (validates everything, imports atomically as an immutable release) and `catalog activate <version>`. No catalog-editor UI in v1; `CatalogEditor` stays reserved (§5.1, §12) | SPEC R-ADM-01 |
| C75 | **Product-identity administration** (*proposal O-S2*, confirmed in v12, C87): `PlatformAdmin` endpoints to list, accept and reject `ProductIdentityRequest`s and to merge duplicate `SoftwareProduct`s; minimal admin pages; actions logged with user and time (§7.2) | SPEC R-ADM-02 |
| C76 | **Removing a candidate** (*proposal O-S3*, confirmed in v12, C87): `DELETE /api/v1/candidates/{id}` (Owner, Editor); evidences and assessments are deleted, a published entry stays and its `SourceCandidateProductId` becomes null (Y8) (§7.2) | SPEC R-CAN-09 |
| C77 | **Response shapes fixed** for `GET /catalog/filter-options` and `GET /catalog/criteria` (SPEC §8.1, §8.2): `excludedBy[]` carries `layer`, `id`, `name` and **no reason text** (the client builds it from UX keys); `appTypesWithoutCriteria[]` is a list of `{ id, name }`; `matchedRule.kind` ∈ `appTypeAndUseCase | appType | useCase | default`. O-8 closed (§7.2) | SPEC SD10 |
| C78 | **Problem types** with a configurable base URI (SPEC §8.6) (§7.1) | SPEC §8.6 |
| C79 | **Personal data export** `GET /api/v1/account/export` as JSON (SPEC §8.4) and account deletion rules (sole Owner of an organisation with other members is refused; an organisation whose last member deletes the account is deleted with its lists and projects) (§7.2, §9.3) | SPEC SD14, R-AUTH-09/10 |
| C80 | **Organisation rules:** the last Owner cannot be demoted, removed or leave; invitations are bound to the invited address (§5.2, §7.2) | SPEC R-ORG-02/03 |
| C81 | **Share links:** expiry required, default 90 days, maximum 1 year (§5.3) | SPEC SD15 |
| C82 | **Usage events final:** exactly `CriteriaListShown` and `ExportCreated` (SPEC §9.1); staleness threshold 18 months, evaluated by the client from `publishedAt` (O-15 closed) (§5.7, §16.5) | SPEC SD5, SD7 |
| C83 | **Compare against project copies:** the configuration-scoped directory variant also accepts project copies of the caller's organisation (§7.2) | SPEC R-ENT-04 |
| C84 | **Rate-limit defaults** per SPEC §7.3, all configurable (§9.2) | SPEC §7.3 |
| C85 | **Configured content:** legal pages from versioned Markdown files in the repository; the criteria-suggestion channel from configuration (`Contact:CriteriaSuggestion`); production builds fail while either is a placeholder (§9.7) | SPEC SD16, SD17 |
| C86 | **Open points:** O-8 and O-15 closed; O-17–O-21 added for the SPEC proposals O-S1–O-S5 (§16.5) | SPEC §13 |

### Changes from v9

| # | Change | Driver |
|---|---|---|
| C53 | **Project copy of the criteria list (D1).** Assigning a saved list to a procurement project creates a copy owned by the project (`CriteriaConfiguration.ProcurementProjectId`); the copy is not offered for other projects and cannot be shared by link. After assignment the copy's four selection layers (use case, app types, product parts, test types) are fixed; inclusion, priorities and notes stay editable (C51 now applies to the copy). "An assessment exists" means a `CandidateAssessment` with a value other than `Unknown`; assessment rows are created lazily (no row = `Unknown`). Replacing the list is possible until an assessment exists (§5.3, §5.4, §7.2) | D1; owner 2026-10-13; A-F12 |
| C54 | **Vendor entries without assessments (D2).** A Flow E version without criterion assessments stores no `PublishedAssessment` rows; both directory variants return `fulfillment: null` and the UI shows a note instead of K1. If assessments are given, `productPartIds` is required and the full-scope rule (C43) applies. Flow B always has a scope: publication from a project requires the project's criteria list (§4.4, §4.6, §7.2, §7.3) | D2; B-F13 |
| C55 | **Republication only by the original profile (D3).** Only the contributor profile under which a workspace entry was published may republish it; Editors and Owners of the source organisation may withdraw it and publish a new entry under another profile. Republication keeps the entry's `State`; a withdrawn entry is never republished — the next publication creates a new entry (§4.4, §4.6, §5.5, §7.2) | D3; B-F3 |
| C56 | **Content purge on consent withdrawal (D4).** Withdrawing consent for an experience report withdraws it and purges its personal content: one exempt `ContentPurge` operation sets `NarrativeDe`, `AssistiveTechnologyContext`, `ProductVersionUsed` and the assessment notes (`EvidenceNote`) of all its versions to null and keeps a tombstone (ids, dates, state `Withdrawn`, consent timestamp and text version, moderation record). Timing is set by the legal review; default: immediately. Versions are never updated except by `ContentPurge` (§4.5, §5.5, §9.3, §16.2.8) | D4; B-F1 |
| C57 | **Context of experience reports (D5).** App types are optional and always offered; experience reports carry no use case (`useCaseTemplateIds` removed from Flow D) and never match a set use-case filter; `ContributorDeclared` applies to app types only. Allowed ratings come from the stated app types, all six levels without a statement (§4.5, §5.5, §7.2) | D5; A-F6 |
| C58 | **Re-pinning decided (D6, O-3).** `POST /api/v1/configurations/{id}/copies { release: "active" }` as proposed in §16.3.1; a project that should use a newer release is a new project with a copy (§5.4, §7.2, §16.3.1) | D6 |
| C59 | **Share links (D7, O-5).** Shared criteria lists are readable without an account (`GET /api/v1/shared/{token}` → 200). In v1 only organisation lists can be shared (`ShareLink.ResourceType = Configuration`); there are no project share links. `ShareLink` gains `OrganizationId` (global query filter), stores only a hash of the token and is resolved through `IShareableResourceProvider`, implemented by the owning module (authorisation and redacted read DTO) (§4.3, §5.3, §7.2, §9.1, §16.2.3) | D7; B-F14 |
| C60 | **Session implementation (completes C50, C52).** Sign-in time stored in `AuthenticationProperties.Items`; absolute limit checked in `OnValidatePrincipal`, chained with the security-stamp validator; extend sets `ExpiresUtc` explicitly; `OnCheckSlidingExpiration` renews on every authenticated request except the session probe; expiry times in response headers; anonymous `GET /auth/session` returns 200 and issues the antiforgery token, which rotates on sign-in and sign-out; antiforgery failures return Problem Details; on 401 the shell opens an in-app login overlay and retries the request; Data Protection keys persisted in the database; password change and account deletion end other sessions; lifetimes and warning lead time configurable, time read through `TimeProvider` (§8.3, §9.1, §9.4, §10.4, ADR-006) | B-F7–B-F10, B-F12, B-F17 |
| C61 | **Migration plan:** step 2 adds `CatalogReleaseAppTypeProcedure`; step 3 adds cookie authentication with antiforgery, the Data Protection key store, the Configurator entities and `ShareLink`; step 4 becomes session warning and hardening (§12) | A-F13; B-F11, B-F18 |
| C62 | **Module projects.** One .NET project per module plus one `Contracts` project per module; Platform may be used by every module; the dependency table names `Catalog.Contracts`; outside `Contracts` only controllers and the module registration are public; `PersistedConfigurationSource` moves into Export behind `IConfigurationReader` (Configurator.Contracts), which removes the Export ↔ Configurator cycle (§4.3, §4.4) | A-F19; B-F6 |
| C63 | **Version reference in moderation.** `ContentReport`, `CounterStatement` and `ModerationAction` carry a nullable `PublishedEntryVersionId` (the version that was shown) in addition to the entry (§5.6) | B-F3 |
| C64 | **Idempotency.** `PublishedEntryVersion.ClientRequestId` with a unique index on that column alone, plus `RequestHash`; Flow B carries `clientRequestId`; the same id with a different payload returns 409 (§7.1, §7.2) | A-F8; B-F4 |
| C65 | **Test evidence and product versions.** A new product version updates the existing evidence row of that procedure (`TestedVersion`, date); the history lives in the published versions (§5.4; corrects the wording of C51) | B-F15 |
| C66 | **Enforcement of two invariants.** "`WasTested` is not null for test-result origins" is a factory rule, not a cross-table CHECK; immutability of versions is enforced by an EF Core `SaveChangesInterceptor` (§5.5) | B-F5, B-F16 |
| C67 | **API details:** directory filter `testKinds[]` (multi); `EvidenceUrl` on `PublishedTestEvidence`; derived `procedureAnswers[]` returned by the server; `PATCH …/criteria/{criterionRevisionId}`; Flow D step 2 reads "no `PublicationConsent`" (§4.5, §5.5, §7.2, §7.3) | A-F7, A-F11; B-F19 |
| C68 | **Wording aligned:** consent is no longer described as conditional (§9.3, §16.2.8); §13 uses the WSP-02/03 wording for the project list; the §11.2 projection runs inside Directory on configuration data read through `IConfigurationReader` | A-F9, A-F10; B-F19 |
| C69 | **Usage events for the PRD §10 indicators** listed (§5.7) | Review A (PRD coverage) |
| C70 | **Open points:** O-3 and O-5 closed; O-14–O-16 added from the UX copy (UX-O2, UX-O8, UX-O19); the number O-10 was never assigned; decisions D1–D7 recorded in §16.4.14 | A-F15, A-F18 |

### Changes from v8

| # | Change | Driver |
|---|---|---|
| C52 | **Session timeouts decided:** 30 minutes without activity (sliding), at most 8 hours from sign-in (absolute). The shell warns 5 minutes before either limit; "Angemeldet bleiben" renews only the idle limit and is not offered in the last 5 minutes before the absolute limit (the warning then asks to save and sign in again) (§8.3, §9.1) | O-12; owner 2026-10-13 |

### Changes from v7

| # | Change | Driver |
|---|---|---|
| C50 | **Session model: ASP.NET Core Identity cookie authentication with antiforgery** replaces refresh cookie + in-memory access token (D8 revised, ADR-006). One `HttpOnly; Secure; SameSite=Strict` session cookie with sliding expiration; antiforgery token for state-changing requests; no JWT, no silent-refresh endpoint. The session-expiry warning stays (§8.3, §9.1, §12 step 4) | O-11; owner 2026-10-13 |
| C51 | **Locked criteria list clarified:** only *replacing* the project's configuration (other list or other catalog release) is locked once assessments exist. Adjusting the assigned list within its release (include/exclude criteria, priorities, notes) stays possible; newly included criteria start as `Unknown`, excluded ones keep their assessments hidden. A new version of the product is handled by re-assessing and a new test evidence with `TestedVersion`, not by changing the list (§5.4) | Owner question 2026-10-13 |

### Changes from v6

| # | Change | Driver (review finding) |
|---|---|---|
| C41 | **.NET 10 (LTS until Nov 2028)** replaces .NET 8, which reaches end of support on 2026-11-10; EF Core 10, Npgsql 10. Upgrade in migration step 0, before any test project (§2.1, §10, §12) | R1; owner 2026-10-12 |
| C42 | **Stable entry, immutable versions.** `PublishedEntry` holds identity, channel, origin, contributor, state and `CurrentVersionId`; each publication or republication creates a `PublishedEntryVersion` holding everything that is published. Moderation, counter-statements, reports and URLs attach to the entry. `EntryVersion` is removed (§5.5, §5.6) | R2 |
| C43 | **Assessment scope is stored.** At publication the factory writes one `PublishedAssessment` per in-scope requirement criterion (`Unknown` where nothing was assessed); Flow E's product parts and Flow B's included criteria define the scope. Projection rule for keys outside the scope fixed (§5.1, §5.5, §7.3) | R3 |
| C44 | **Module contracts.** Every module exposes a `Contracts` namespace (interfaces, DTOs); the dependency table lists the permitted directions explicitly, including Workspace → Directory.Contracts and Directory → Configurator.Contracts; withdrawal rights via `IEntrySourceAuthorizer` implemented by Workspace; `ShareLink` moves to Platform (§4.3, §5.3) | R4 |
| C45 | **`CatalogReleaseAppTypeProcedure`** (m:n, release-pinned) replaces `AppType.TextTestTypeId` (§5.1) | R5 |
| C46 | **Several test evidences per product; procedure criteria are derived.** `TestStatus` moves to `CandidateProduct`; `TestEvidence` becomes 0..n rows (one per procedure or other test). The answer for „Geprüft nach …“ is derived from these rows and is no longer entered; `ProcedureAnswer` columns are removed. Published side: `PublishedEntryVersion.WasTested` plus 0..n `PublishedTestEvidence` (§5.4, §5.5, §7.2, §7.3, §16.4.7) | R6; owner 2026-10-12 (decision E-R6 A) |
| C47 | **Criteria list of a project is locked once assessments exist** (v1); re-pinning (§16.3.1) applies to new projects (§5.4) | Y1; owner 2026-10-12 |
| C48 | **Consent for every experience report** with stored consent-text version; withdrawing consent = withdrawing the report (§4.5, §5.5) | Y3; owner 2026-10-12 |
| C49 | Smaller corrections: assessment kind no longer needs a cross-table CHECK (Y2 resolved by C46); "Merken/Teilen von Produkten" defined (Y4); typed FKs on `FilterExclusion` (Y5); context validation and arrays everywhere (Y6); procedure criteria applicable via `testTypeIds` in the export (Y7); `SourceCandidateProductId` never serialised, `ON DELETE SET NULL` (Y8); `UseCaseTemplate.OwnerOrganizationId` removed until P2 (G1); offset paging in the directory and client request ids instead of an idempotency store (G3); step alignment (G4) | Review |

### Changes from v5

| # | Change | Driver |
|---|---|---|
| C30 | **Procedure criteria ("Geprüft nach …").** A criterion kind of its own (`Criterion.Kind = TestProcedure`, one per `TestType`): listed and deselectable, default priority B, exported, assessed as Ja/Nein/Nicht eingeschätzt (`ProcedureAnswer`), **never counted** in the fulfilment breakdown (§4.4, §5.1, §5.4, §7.3, §16.4.7) | Audit E-A; owner 2026-10-12 |
| C31 | **Allowed ratings with several procedures = union.** A criterion's allowed ratings for a selection are the union of the `AllowedRatings` of all procedure texts that apply to the selected app types; `Unknown` is always allowed; without any procedure text all six levels are allowed (§5.1, §16.4.8) | Audit E-B; owner 2026-10-12 |
| C32 | **App types and use cases on the published entry (variant A).** `PublishedEntryAppType` and `PublishedEntryUseCase` with a `ContextSource` (`ProjectSelection` / `VendorDeclared` / `ContributorDeclared`). Flow B copies them from the project's criteria list, Flow E takes the vendor's statement (several allowed), Flow D optional. The product identity stays free of any claim (§4.4–4.6, §5.5, §7.2, §16.4.9) | Audit E-C; owner 2026-10-12 |
| C33 | **Experience reports carry no counts** — only the optional list of assessed criteria (§4.5, §7.3, §16.4.10) | Audit E-D |
| C34 | **K1 in the project scope has seven rows**; `notCovered` exists only in directory projections (§7.3, §16.4.10) | Audit E-E |
| C35 | **Multi-selection in the API contracts:** catalog criteria, ad-hoc export, directory search and Flow E use `appTypeIds[]` / `testTypeIds[]`; Flow E body gains `testKind`, `otherTestTypeName`, `testedVersion`, `useCaseTemplateIds[]`; Flow D body gains the assistive-technology consent; directory search gains `testKind`, `hasReport`, `sort` (§7.2) | Audit E-G, L3 |
| C36 | Catalog details: `UseCaseTemplate.IsActive` (seed „Datenbankgestützte Fachanwendung“ inactive), `AppType.TextTestTypeId` (which procedure texts apply), release pinning of `FilterExclusion` via `CatalogReleaseFilterExclusion`, indicative `excludedBy[]` shape, `appTypesWithoutCriteria[]` for Hardware/Vermittlungsdienst (§5.1, §7.2) | Audit L5, L6, M3, M4 |
| C37 | **Angular 22** replaces Angular 20 (support window; TESTING §12.1 resolved) (§2.1, §4.2, §8.2) | Owner 2026-10-12 |
| C38 | Migration plan: step 2 lists all v5/v6 catalog entities; test-type filtering moves from step 7 to steps 2 and 6 (P0) (§12) | Audit L4 |
| C39 | Re-pinning (§16.3.1) copies the full four-layer selection; consolidated open points in §16.5 | Audit L7, L8 |
| C40 | Code Connect is not available on the Figma plan in use; the Figma link is kept through documentation links, `@figma` JSDoc tags and a CI check (§8.4) | Owner 2026-10-11 |

### Changes from v4

| # | Change | Driver |
|---|---|---|
| C23 | **Four filter layers with a fixed order.** Use case (single choice, optional "none") → app types (multi) → product parts (multi, `Basisfunktionalität` always on) → test types (multi, optional). Configurations and the ad-hoc selection carry **several** app types and optional test types (§4.4, §5.1, §5.3). | Design review 2026-10-06, points 1–4 |
| C24 | **Exclusion rules** as catalog data (`FilterExclusion`), valid across all lower layers. Changing a higher layer clears all lower selections except `Basisfunktionalität`. The server rejects selections that violate a rule (§5.1, §13). | Design review, point 5 |
| C25 | **Rating scale of BIT inklusiv** replaces the VPAT-style values: `Fulfilled`, `SlightRestriction`, `Restriction`, `Barrier`, `Blockade`, `NotApplicable`, plus `Unknown`. The count contract grows from six to **eight** properties (§4.4, §5.4, §7.3, §8.4, §16.4.5). | Design review, point 6; owner decision 2026-10-06 |
| C26 | **Criterion texts per test procedure.** BIT inklusiv texts (applicability, requirement, rationale, allowed ratings, examples, anchor link) are stored per criterion revision and test type (`CriterionProcedureText`). Used with permission (§5.1). | Design review, point 6 |
| C28 | **Experience reports carry no test status (E13).** Entries with `OriginKind = DirectContribution` have no `PublishedTestEvidence`; they are returned in a separate `experienceReports` list, never in `entries`, and the UI never shows "Auf Barrierefreiheit geprüft" for them. F8 applies to test-result entries (Flows B and E) (§4.5, §5.5, §7.3, §13) | Owner decision 2026-10-06 |
| C29 | §16.3.2 decided: HR-Anwendung does **not** exclude Mobile Anwendung; the repository seed rule is removed (§5.1) | Owner decision 2026-10-06 |
| C27 | `TestType` becomes catalog reference data with five procedures, plus two free values for evidence only ("Andere Prüfung" with a name, "Eigene Prüfung ohne festes Verfahren") (§5.1, §5.4). | Design review, point 4 |

### Changes from v3

| # | Change | Driver |
|---|---|---|
| C15 | §16.1 decided: **Option A** — the API returns the six fulfilment counts per entry. The open question is closed; the rationale is kept in §16.4. | Owner decision, 2026-10-01 |
| C16 | §16.3.1 decided: **Option C** — suggestions for the criteria catalog go through an external channel (link to a WERTE.IT contact address or form). No data model, no endpoint. F11 is now fully covered. | Owner decision, 2026-10-01 |
| C17 | §16.3.2 decided: **Option B** — new **Flow E**, a direct Kanal-1 publication path for vendor and auditor contributor profiles, without organisation or procurement project (§4.6). Flow E may create product identities through the same identity-resolution step as Flow B. `PublishedEntryFactory` now has **three** write paths (§4.3, D5, D13, ADR-008). | Owner decision, 2026-10-01 |
| C18 | **Flow B publishes Kanal 2 only.** With Flow E as the single Kanal-1 path, the channel choice is removed from Flow B; `PublicationConsent` is no longer captured in the workspace (§4.4, D16). | UX copy assumption A1, confirmed by owner 2026-10-01 |
| C19 | **Export without an account.** Flow A gains an unauthenticated, non-persisting export of an ad-hoc selection (§4.4, §7.2, D15). Saving, sharing and the workspace still require an account and an organisation. | UX copy finding O1, owner decision 2026-10-01 |
| C20 | **Tri-state test status in the workspace.** `TestEvidence.WasTested` (bool) becomes `TestStatus` (`Tested` / `NotTested` / `Unknown`, default `Unknown`). The published field stays a non-null boolean; publication is rejected while the status is `Unknown` (§5.4, §5.5). | UX copy finding O6, owner decision 2026-10-01 |
| C21 | Two items added to the bundled legal review: authenticity of vendor and auditor claims including self-declared consent (§16.2.7), and explicit consent for assistive-technology context in experience reports (§16.2.8). | UX copy decision E4; Art. 9 GDPR placeholder in UX copy S51 |
| C22 | New §16.3 with an open question raised in step 7: re-pinning a saved configuration to a newer catalog release. | UX copy finding O7 |

### Changes from v2 (retained for history)

| # | Change | Driver |
|---|---|---|
| C11 | New open question §16.3.1: the PRD P0 requirement *"Vorschlagen von Kriterien"* has no path in the architecture since the proposal workflow was retired. | Step 5 flow map (board 16) |
| C12 | New open question §16.3.2: Kanal 1 can only publish through Flow B, so a vendor or auditor needs an organisation and a procurement project although they procure nothing. | Step 5 flow map (board 16) |
| C13 | Cross-references to §16.3 added in §3.1 (F11), §4.4 (Flow B), §8.1 (`withProposal()`) and §12 (step 6). | Follows C11, C12 |
| C14 | Reference to the FigJam diagrams added to the document header. | Step 5 |

### Changes from v1 (retained for history)

| # | Change | Driver |
|---|---|---|
| C1 | The two-signal guarantee is restated honestly: the architecture prevents a *stored* score, not a *displayed* one. Aggregate ratios removed from the API contract; a five-way breakdown including `notCovered` is returned instead. One decision remained open (§16.1, decided in v4). | Review finding 1 |
| C2 | The configuration-scoped directory endpoint is now authenticated and membership-checked; a separate unauthenticated variant exists. | Review finding 2 |
| C3 | **Directory gains a second, direct write path** for experience reports without a workspace. The module dependency rule in §4.3 changed accordingly. | Review finding 3 |
| C4 | Catalog releases reference criterion revisions through a join table instead of owning them, removing the full-clone-per-release burden. | Review finding 4 |
| C5 | Cross-release criterion matching is specified, with `NotCoveredByThisRelease` as a distinct value. | Review finding 5 |
| C6 | The query cost of D3 is stated explicitly, with a pre-filter strategy. | Review finding 6 |
| C7 | Migration no longer auto-creates a personal organisation per existing user. | Review finding 7 |
| C8 | `xmin`-derived ETags documented as restore-unstable. | Review finding 8 |
| C9 | ClamAV sidecar removed from the v1 topology; retained as a documented option. | Review finding 9 |
| C10 | Catalog versioning (ADR-002) and publish-time product identity (ADR-005) confirmed by the project owner; no longer flagged as beyond-PRD proposals. | Owner decision, 2026-09-29 |

---

## 1. Purpose and scope

This document defines the target architecture for ProcureAccess: module boundaries, the domain model, API contracts, cross-cutting concerns, deployment topology, and a migration path from the existing partial implementation.

The architecture must serve two product pillars defined in the PRD:

1. **Kriterien-Konfigurator** — software type + intended use → prioritised, plain-language accessibility criteria → adjust → export. **Usable without an account up to and including the export** (v4).
2. **Workspace with optional publication** — organisation-scoped procurement projects with candidate products, assessed against criteria, optionally published into a community-maintained directory.

It must additionally serve two contribution paths that belong to *neither* pillar — experience reports (§4.5) and vendor/auditor test results (§4.6).

---

## 2. Context and constraints

### 2.1 Fixed technology decisions

| Layer | Technology | Status |
|---|---|---|
| Backend | ASP.NET Core 10 (.NET 10 LTS), C# | locked (raised from 8 in v7, C41) |
| ORM | EF Core 10 with Fluent API (`IEntityTypeConfiguration<T>`, no data annotations) | locked |
| Database | PostgreSQL 17 (Npgsql) | locked |
| Frontend | Angular 22 (zoneless, standalone, signal inputs), TypeScript, NgRx Signals, Angular Material (M3) | locked (version raised from 20 in v6) |
| Packaging | Docker / docker-compose | locked |
| E-mail | SMTP with TLS via MailKit; provider Lettermint (EU); Mailpit locally | locked (new in v11, C71) |
| Distribution | Open source on GitHub | locked (funding requirement) |

### 2.2 Organisational constraints

- **Team size:** effectively one developer, AI-assisted. Every decision must be justifiable against maintenance cost for a single maintainer.
- **Funding horizon:** ProcureAccess is a research deliverable. Ownership after the funding period is an open question (PRD §11). **Handover-ability is therefore a first-class quality attribute**, ranked above performance.
- **Operating model:** one central instance hosted by WERTE.IT. Self-hosting is not a v1 requirement but is not architecturally excluded, since the open-source release invites it.
- **Workspace ownership:** organisation/team scoped. Colleagues in the same procurement department work on the same Beschaffungsvorgang.

### 2.3 State of the existing codebase

The existing implementation covers parts of the criteria filtering pillar.

**Keep and build on:**
- The four-layer pattern (Entity/DTO → Repository → Service → Controller), appropriate for reference data and economical for a solo maintainer.
- `BaseEntity` with `Id`, `CreatedAt` and the PostgreSQL `xmin` concurrency token. Optimistic concurrency matters more now that workspaces are team-shared.
- Serilog, AutoMapper, `Result<T>`, Docker setup, ngx-translate i18n, Angular Material.
- The NgRx Signals store composed from `signalStoreFeature` slices — a good fit, but the slice cut changes (§8.1).

**Structurally outdated (predates the Workspace pivot):**
- `withProducts()`, `withProposal()`, the `Approver` role and the `objects:approve` claim implement *"central product database + proposal workflow with editorial approval"* — rejected in brainstorm §2.1 and §2.4, listed as a Nicht-Ziel in PRD §6.
- No Workspace, no organisation concept, no Kanal 1/Kanal 2 separation, no separation of Dokumentationsgrad and Erfüllungsgrad.
- Global-only roles. Organisation-scoped workspaces need resource-based authorisation.
- Criteria modelled as mutable rows. Exported criteria can become contractually binding parts of a tender (brainstorm §2.6), which requires reproducibility, which requires versioning.
- JWT in `localStorage` — replaced by Identity cookie authentication (D8, C50).
- Generic `BaseCrudController<TEntity, TDto, TController>` is fine for catalog admin, but Workspace and Directory have state transitions and invariants that must not be exposed as generic CRUD.

---

## 3. Requirements summary

### 3.1 Functional drivers (from PRD §9)

| # | Driver | Architectural consequence |
|---|---|---|
| F1 | Filter by app type (incl. closed software), product parts and test procedure (P0 since PRD v3) | Catalog applicability model; four ordered filter layers with exclusion rules |
| F2 | Filter by use case (Verwendungszweck; formerly "Anwendungstyp-Template"), extensible | Data-driven templates, org-owned templates prepared; inactive templates possible |
| F3 | Criteria in plain language, based on BIT inklusiv (Software-, App-, PDF-Test; formerly BITV-Softwaretest) | Criterion text separate from standard references |
| F4 | Recommended priorities/weights per app type + use case | Rule resolution engine with explainable output |
| F5 | Excel export | Pluggable export pipeline, accessible output; **available without an account** for an ad-hoc selection (D15) |
| F6 | Workspace: create and assess candidate products | Org-scoped aggregate, team collaboration |
| F7 | Optional publication, Kanal 1/2 clearly separated | Publication as explicit snapshot, not a visibility flag; **one write path per channel origin** — Kanal 1 only via Flow E, workspace publication only as Kanal 2 (D16) |
| F8 | "Getestet: ja/nein" always visible | Mandatory non-nullable boolean on every published **test-result** entry (Flows B and E), never redactable; experience reports (Flow D) are not tests and carry no test status (C28); tri-state only inside the private workspace (§5.4) |
| F9 | Dokumentationsgrad and Erfüllungsgrad as two separate signals | No combined score in schema or API contract (§4.4); eight counts returned for test-result entries (§16.4.1, §16.4.5); none for experience reports (C33) |
| F10 | Pseudonymous Kanal-2 participation | Contributor identity decoupled from account **and from organisation membership** |
| F11 | Save, share, propose | Share links for criteria lists, readable without an account (C59); criteria suggestions via an external WERTE.IT channel (§16.4.2); product proposals via `ProductIdentityRequest` |
| F12 | Tool is itself accessible | Accessibility as an architectural constraint (§9.4) |

### 3.2 Quality attributes, ranked

1. **Accessibility** — non-negotiable; the tool loses its legitimacy if it fails its own criteria.
2. **Maintainability / handover-ability** — a future maintainer must onboard from the repository alone.
3. **Privacy and legal defensibility** — special-category personal data (§9.3) and takedown risk (PRD §7).
4. **Traceability / reproducibility** — what was exported into a tender must be reconstructible years later.
5. **Performance and scale** — deliberately last. See §11.

### 3.3 Explicit non-requirements

No high availability, no multi-region, no real-time collaboration, no offline mode, no public write API, no microservices, no message broker in v1.

---

## 4. Architecture overview

### 4.1 Style: modular monolith

One deployable ASP.NET Core application with enforced internal module boundaries.

*Rationale:* a single maintainer, low request volume (§11), and a handover requirement all argue against distributed deployment. Module boundaries give most of the comprehension benefits of services without the operational cost. **→ ADR-001**

### 4.2 Component diagram

```
┌────────────────────────────────────────────────────────────────┐
│  Browser — Angular 22 SPA (zoneless, NgRx Signals)             │
│  lazy-loaded features: catalog · configurator · workspace ·    │
│                        directory · contribute · admin · account│
└────────────────────────┬───────────────────────────────────────┘
                         │ HTTPS · JSON · cookie-based session
┌────────────────────────▼───────────────────────────────────────┐
│  Reverse proxy (nginx or Caddy)                                │
│  TLS · static assets · security headers · gzip/brotli          │
└────────────────────────┬───────────────────────────────────────┘
┌────────────────────────▼───────────────────────────────────────┐
│  ASP.NET Core 10 API — single deployable, modular monolith     │
│                                                                │
│  ┌──────────┐ ┌─────────────┐ ┌───────────┐ ┌──────────────┐   │
│  │ Catalog  │ │ Configurator│ │ Workspace │ │  Directory   │   │
│  └──────────┘ └─────────────┘ └───────────┘ └──────────────┘   │
│  ┌──────────┐ ┌─────────────┐ ┌───────────────────────────┐    │
│  │Moderation│ │   Export    │ │  Identity & Access        │    │
│  └──────────┘ └─────────────┘ └───────────────────────────┘    │
│  ──────────────────────────────────────────────────────────    │
│  Platform: AuthZ handlers · caching · ProblemDetails ·         │
│            Serilog · i18n · rate limiting · file storage       │
└──────────┬──────────────────────────────────┬──────────────────┘
           │ EF Core / Npgsql                 │ file I/O
┌──────────▼──────────────────────┐  ┌────────▼───────────────────┐
│  PostgreSQL 17                  │  │  File store (volume, S3-   │
│  schemas: catalog · configurator│  │  compatible later)         │
│  workspace · directory ·        │  │  ACR/PDF uploads, exports  │
│  moderation · identity ·        │  │  served as attachments     │
│  analytics · logging            │  │                            │
└─────────────────────────────────┘  └────────────────────────────┘
```

### 4.3 Module responsibilities and dependency rules

| Module | Owns | May depend on |
|---|---|---|
| **Catalog** | Criteria, revisions, releases, app types, product parts, use-case templates, test types, priority rules, procedure mapping | *no module* (leaf) |
| **Configurator** | Saved criteria configurations (organisation lists and project copies, C53), user overrides, copies to a newer release | Catalog.Contracts, Identity.Contracts |
| **Workspace** | Organisations' procurement projects, candidate products, test evidences, private assessments | Catalog.Contracts, Configurator.Contracts, **Directory.Contracts** (publication, product lookup), Identity.Contracts |
| **Directory** | Software product identities, published entries and versions, ACRs, experience reports, consent records | Catalog.Contracts, **Configurator.Contracts** (configuration-scoped projection via `IConfigurationReader`), Identity.Contracts |
| **Moderation** | Content reports, counter-statements, moderation audit log | Directory.Contracts, Identity.Contracts |
| **Export** | Format renderers (XLSX, CSV, later Vergabetext); export sources `PersistedConfigurationSource` and `AdHocSelectionSource` | Catalog.Contracts, Configurator.Contracts (`IConfigurationReader`) |
| **Identity & Access** | Users, memberships, contributor profiles, roles, Data Protection key store | *no module* |
| **Platform** | Share links (resolved through `IShareableResourceProvider`, C59), file storage, e-mail sending (`IEmailSender`, C71), ProblemDetails, rate limiting, i18n, caching | *no module*; **every module may use Platform** (not repeated in the rows above) |

**Contracts and projects (v7, C44; enforced by project references since v10, C62).** Each module is one .NET project (`ProcureAccess.Modules.<Name>`) with a separate contracts project (`ProcureAccess.Modules.<Name>.Contracts`, interfaces and DTOs). A module project references only the contracts projects listed in the table and Platform, so the compiler rejects any other dependency, and `internal` has an effect because modules are separate assemblies. Inside a module project only the controllers (needed for MVC discovery) and the module's registration extension (`Add<Name>Module`) are public. A module never references another module's entities or `DbContext`. Workspace never references Directory entities: Flow B calls `IPublicationGateway` (implemented in Directory, wraps `PublishedEntryFactory`) and passes a redacted `PublicationRequest` DTO. Withdrawal rights for workspace-origin entries are resolved through `IEntrySourceAuthorizer`, declared in Directory.Contracts and implemented by Workspace (dependency inversion, so Directory never reads workspace tables). Configurations are read by other modules only through `IConfigurationReader` (declared in Configurator.Contracts, implemented in Configurator); this is how `PersistedConfigurationSource` in Export and the configuration-scoped directory projection get their data, so Configurator never depends on Export and there is no cycle (C62). Share links are resolved through `IShareableResourceProvider` (declared in Platform, implemented by Configurator in v1, C59). NetArchTest asserts exactly the dependencies in the table above.

Rules:

```
Catalog is a leaf — it depends on no other module.
Modules reference each other only through *.Contracts (table above);
  every module may use Platform.
Workspace and Directory MUST NOT reference each other's entities.
Directory has exactly THREE write paths into PublishedEntry:
  (a) PublicationService       — Workspace → Directory crossing (§4.4, Flow B)
                                 channel is always Independent (Kanal 2)
  (b) ContributionService      — experience report, no Workspace (§4.5, Flow D)
                                 channel is always Independent (Kanal 2)
  (c) VendorPublicationService — vendor/auditor test result, no Workspace
                                 (§4.6, Flow E); channel is always
                                 VendorOrAuditor (Kanal 1)
All three converge on the same invariants (channel, contributor, consent,
release pinning, test status), enforced in a shared PublishedEntryFactory;
none may bypass it. The factory accepts Channel = VendorOrAuditor only
together with OriginKind = VendorPublication.
Product identity creation is shared by (a) and (c) through one
IProductIdentityResolver; (b) may only link to an existing identity.
Moderation only reads and state-transitions Directory entries;
it never mutates their content.
```

**History:** v1 stated that `PublicationService` was the *only* boundary crossing (wrong, corrected in v2 with Flow D). v4 adds Flow E. The invariant that survives every version is not "one service" but "several paths, one set of invariants, enforced in a shared `PublishedEntryFactory`".

Enforced by namespace conventions plus **NetArchTest** assertions in the test suite, so a violation fails CI rather than being caught in review. This matters disproportionately for a solo project.

### 4.4 Core data flows

**Flow A — Criteria configuration and export**

```
User selects, in this order (each layer may exclude values of the lower ones):
  1 UseCaseTemplate? (single; null = "Keiner davon passt")
  2 AppTypes[]        (multi)
  3 ProductParts[]    (multi; Basisfunktionalität always included)
  4 TestTypes[]       (multi, optional)
      │
      ▼
CatalogQueryService                              (public, no account)
      ├─ validate selection against FilterExclusion rules (400 if violated)
      ├─ applicable criteria   = f(AppTypes, ProductParts) ∪ one procedure criterion per TestType (C30)
      ├─ allowed ratings       = ∪ AllowedRatings of the procedures mapped to the AppTypes
      │                          in this release (C31, C45)
      ├─ priority per criterion = resolve(criterion, appTypes, useCase)
      └─ pinned to the active CatalogRelease
      ▼
Draft selection in the browser (withConfigurator, in memory)
user may include/exclude/re-prioritise/annotate
      │
      ├── A1: export without account ──────────────────────────────┐
      │     POST /api/v1/exports  { selection, format }            │
      │     AdHocSelectionSource: re-resolves priorities on the     │
      │     server, applies overrides, persists NOTHING             │
      │                                                             ▼
      │                                           ExportService → IExportFormatter
      │                                                             ▲
      └── A2: save (account + organisation)                        │
            POST /api/v1/configurations  ──► CriteriaConfiguration │
            POST /api/v1/configurations/{id}/exports ──────────────┘
                 PersistedConfigurationSource (Export, reads the
                 configuration through IConfigurationReader, C62)
```

**Anonymous export (new in v4, D15).** The primary persona should get from "what do I need to demand?" to a file in the Vergabeunterlagen without registering (PRD §5, goal 1). Persistence, sharing and team work keep requiring an account and an organisation, because `CriteriaConfiguration` is organisation-owned and that ownership is what makes it shareable and auditable.

Design points:

- **The server never trusts client-resolved priorities.** The ad-hoc request carries the selection (`releaseVersion`, `useCaseTemplateId?`, `appTypeIds`, `productPartIds`, `testTypeIds`) and per-criterion user decisions (`criterionRevisionId`, `isIncluded`, `overrideLevel?`, `overrideWeight?`, `note?`). The server resolves recommended priorities itself, rejects revision ids that are not part of the stated release or not applicable to the selection (Problem Details, 400; procedure criteria count as applicable through `testTypeIds`, Y7), and renders the same columns as a persisted export — recommended level, user override, weight, note.
- **One formatter, two sources.** `IExportSource` has two implementations, both in Export: `PersistedConfigurationSource` (reads the configuration through `IConfigurationReader`, C62) and `AdHocSelectionSource` (depends on Catalog.Contracts only). Identical input yields identical file content apart from generation metadata. This keeps EXP-01–04 valid for both and avoids a second rendering code path.
- **Nothing is stored.** No configuration row, no file in the file store, no user-derived data in logs. A `UsageEvent` (`ExportCreated`, dimension: format) is the only trace (§5.7).
- **Reproducibility is preserved** because the file states the catalog release version (EXP-03); the release itself is immutable (ADR-002). What is lost for anonymous exports is only the *server-side* record of the user's overrides — acceptable, since the file is the record.
- **Abuse surface:** an unauthenticated POST that does CPU work. Mitigated by a dedicated rate-limit policy, a request-size cap (bounded by the release's criterion count) and length caps on notes (§9.2).
- **Draft carry-over:** the draft lives in the NgRx store. Login and registration use the in-app `login` outlet, so the store survives; after login the draft can be saved as a `CriteriaConfiguration` without being re-entered (§8.1). The draft is not written to browser storage.

**Flow B — Publication from a workspace (Kanal 2 only, changed in v4)**

```
CandidateProduct  (private, organisation-scoped)
      │  user action: "veröffentlichen"
      │  { contributorProfileId, productIdentity, clientRequestId }
      ▼
PublicationService
   0. require: the project has its criteria list (its own copy, C53);
      otherwise 422 — Flow B always has a scope (C54)
   1. resolve product identity → IProductIdentityResolver
      (trigram search, user confirms, or create a new SoftwareProduct)
   2. redact: internal notes, tester qualification note, contacts,
      commercial data are NEVER copied
   3. require: contributor profile; channel is fixed: Independent.
      Candidate already has a visible entry: same profile → new version
      of that entry (State kept); other profile → 409, withdraw first
      (C55). A withdrawn entry is never republished; the next
      publication creates a new entry
   4. require: TestStatus ≠ Unknown (mapped to WasTested true/false);
      copy the candidate's TestEvidence rows (redacted)
   5. scope = requirement criteria included in the project's list;
      one assessment row per criterion in scope (Unknown where the
      candidate has no row or Unknown, C43, C53)
      + CatalogReleaseId + timestamp
   6. context: AppTypeIds and UseCaseTemplateId of the project's
      criteria list (ContextSource = ProjectSelection, C32); no use case
      if the list says "Keiner davon passt"
      ▼
IPublicationGateway ─► PublishedEntryFactory ─► PublishedEntry
                                            └─► PublishedEntryVersion (new, immutable)
```

Publication is a **copy, not a flag**. Consequences, all intentional:

- Editing a workspace entry never silently changes public content; republishing is an explicit act.
- Redaction is structural, not a UI concern — private and public representations are different types.
- Withdrawal removes the public entry without touching the user's own procurement record.

*Trade-off:* published data can go stale relative to the workspace. Mitigated by showing the publication date prominently and prompting the owner to republish after significant changes. **→ ADR-004**

**Why Kanal 2 only (D16).** A procuring organisation is never the vendor of the product it evaluates. Its publication is independent by definition — including when it commissioned an accessibility test itself, since the commissioning party is then the buyer, not the vendor. Offering a channel choice in the workspace would invite exactly the mislabelling the two-channel principle exists to prevent, and would scatter consent capture across two UIs. Kanal 1 has a single entry point: Flow E (§4.6). `PublicationConsent` is therefore no longer part of Flow B.

**Flow C — The two signals**

```
GET /api/v1/directory/products/{id}/entries?configurationId={cfg}   (authenticated)
GET /api/v1/directory/products/{id}/entries                          (public)

PublishedEntry ─┐
                ├─► projection computed per request ─► { documentation, fulfillment }
CriteriaConfig ─┘   (second form: projected against the entry's own criterion scope)
```

**Erfüllungsgrad is relative to the requesting user's criteria selection and therefore cannot be a stored property of a product.** Dokumentationsgrad *is* an attribute of the entry (what evidence exists, how recent) and is stored — in a separate DTO branch that the API contract never merges. **→ ADR-003**

A Flow E version published without criterion assessments has no scope. Both variants return `fulfillment: null` for it, and the UI shows a note instead of K1 (C54). Every other test-result entry has a scope and a full breakdown.

**What this does and does not guarantee.** v1 claimed a combined score was "structurally impossible". That overstated the protection. The accurate statement:

| Layer | Guarantee |
|---|---|
| Persistence | **Hard.** No column, view or index holds an aggregate accessibility score. Nothing can be sorted or filtered by one. |
| API contract | **Hard for ratios, soft for counts.** No percentage, rating or `overallStatus` field is ever serialised; a contract test asserts this. Category counts *are* returned, and a client could divide them. |
| Presentation | **Convention only.** Enforced by the UI rule in §8.4, the domain component `pa-fulfillment-breakdown` (UX copy K1) and a review checklist, not by the architecture. |

The counts are returned as a complete seven-way breakdown (`fulfilled`, `slightRestriction`, `restriction`, `barrier`, `blockade`, `notApplicable`, `unknown`) plus `notCovered` (§5.1), and no total (changed in v5 from five-way, C25). Returning all eight together makes a naive "X %" harder to compute correctly than to compute wrongly — a weak deterrent, and named as such rather than dressed up. **Decided in v4** (Option A, §16.4.1): the stricter per-criterion-only contract was rejected because it would make the directory list and comparison views substantially harder to use.

### 4.5 Flow D — Direct contribution without a workspace (new in v2)

The PRD persona *"Mensch mit Behinderung als Beitragende"* has no procurement project, frequently belongs to no organisation in the tool's sense, and must be able to contribute pseudonymously. Routing this person through a workspace would be wrong three times over: it invents a Beschaffungsvorgang that does not exist, it requires an organisation membership that would tie the contribution to an employer, and it therefore undermines exactly the pseudonymity the PRD demands.

```
Authenticated user with a ContributorProfile
      │  action: "Erfahrungsbericht zu einem Produkt beitragen"
      ▼
ContributionService
   1. select an EXISTING SoftwareProduct (search only —
      this path may not create product identities, see below)
   2. channel is fixed: Independent (Kanal 2); no PublicationConsent
      (that record belongs to Kanal 1 only, C67)
   3. contributor profile required; organisation membership is NOT read
   4. pin CatalogReleaseId (for the optional per-criterion assessment)
   5. optional context: AppTypeIds[] stated by the contributor
      (ContextSource = ContributorDeclared); no use case (C57);
      validated against FilterExclusion
   6. consent required for every report (C48); withdrawal of consent
      = withdrawal + ContentPurge (C56)
      ▼
PublishedEntryFactory ─► PublishedEntry { originKind: DirectContribution }
      └─► ExperienceReport (narrative + assistive-technology context)
          no PublishedTestEvidence — an experience report is not a test
```

Design points:

- **This path cannot create a `SoftwareProduct`.** Identity creation stays with Flows B and E, where a procurement context or the vendor's own knowledge gives the name and vendor some grounding. A contributor wanting to report on an unlisted product submits a product-identity request that a `PlatformAdmin` resolves. This keeps §5.5's identity hygiene intact without blocking the persona.
- **`PublishedEntry` carries no `OrganizationId`.** Provenance is expressed by `ContributorProfileId` and `OriginKind` only. An organisation-derived entry is deliberately not attributable to the organisation; that is the same competitor-visibility concern that motivates §5.4.
- **No test status (E13, changed in v5).** An experience report describes one person's use of the product, not an accessibility test. The factory creates no `PublishedTestEvidence` for this origin, the read API returns these entries only in `experienceReports` (§7.3), and the UI shows the contribution type (`contribute.report.typeNote`) instead of K4. A contributor who has actually tested the product publishes via Flow E (vendor/auditor) or Flow B.
- **Per-criterion assessment is optional on this path, and no counts are derived from it (C33).** A report has no defined criterion scope, so a projection would be undefined. Only assessed criteria are stored (the C43 full-scope rule applies to test-result entries); the read API returns them as a plain list (`assessments`, possibly empty); there is no `fulfillment` branch on experience reports.
- **Consent for every report (v7, C48); purge on withdrawal (v10, C56).** Not only the assistive-technology field but the narrative itself can reveal a disability (Art. 9 GDPR, §9.3, §16.2.8). Every experience report therefore requires consent: `ExperienceReport.ConsentedAt` (not null) and `ConsentTextVersion` (which wording was agreed to). Withdrawing consent (Art. 7(3)) is implemented through the existing withdrawal endpoint: the entry becomes `Withdrawn`, and a `ContentPurge` sets `NarrativeDe`, `AssistiveTechnologyContext`, `ProductVersionUsed` and the `EvidenceNote` of the report's assessments to null on all versions. A tombstone remains: entry and version ids, dates, state `Withdrawn`, consent timestamp and text version, and the moderation record. The legal review decides the consent wording, the legal basis and the timing of the purge (default: immediately, in the same transaction as the withdrawal); none of these needs a schema change.
- **Context (v10, C57).** The contributor may state app types (optional, always offered); experience reports carry no use case. App types decide which criteria and which ratings the optional assessment offers (all six levels without a statement, §5.1).
- Rate limiting and the moderation flow (§5.6) apply identically to all paths.

### 4.6 Flow E — Direct Kanal-1 publication by vendors and auditors (new in v4)

The PRD persona *"Anbieter / Prüferin"* publishes a test result with the consent of whoever commissioned the test, as a visibility and reputation channel. Like the Flow D persona, this person procures nothing; v3 forced them through an organisation and an invented procurement project (§16.4.3). Flow E removes that detour.

```
Authenticated user with a ContributorProfile of Kind = Vendor | Auditor
      │  action: "Prüfergebnis veröffentlichen"
      ▼
VendorPublicationService
   1. resolve product identity → IProductIdentityResolver
      (same step as Flow B: trigram search, user confirms, or create)
   2. channel is fixed: VendorOrAuditor (Kanal 1)
   3. require PublicationConsent { ConsentGivenBy, OnBehalfOf,
      ConsentedAt, EvidenceNote? }
   4. require a definite WasTested (bool) and, if true, ≥ 1 test
      evidence: TestKind (Procedure / Other / Own), TestTypeId or
      OtherTestTypeName, optional TestedVersion, tester, date,
      EvidenceUrl (C46, C67)
   5. context: AppTypeIds[] (≥ 1) and UseCaseTemplateIds[] (0..n),
      ContextSource = VendorDeclared — shown as "vom Anbieter angegeben";
      validated against FilterExclusion
   6. optional per-criterion assessments. With assessments:
      ProductPartIds required; scope = requirement criteria applicable
      to the stated AppTypes + ProductParts in the active release; one
      row per criterion in scope (Unknown if empty, C43). Without
      assessments: no rows, fulfillment = null (C54).
      Pin CatalogReleaseId
   7. optional ConformanceReport (VPAT/ACR, P1)
   organisation membership is NOT read
      ▼
PublishedEntryFactory ─► PublishedEntry { originKind: VendorPublication }
```

Design points:

- **Identity creation is allowed on this path.** A vendor naming its own product is the best-grounded identity creation the system will ever see; queueing it behind `ProductIdentityRequest` (§16.2.6) would penalise the persona the PRD treats as an incentive mechanism. Flow B and Flow E share one `IProductIdentityResolver`, so duplicate detection behaves identically. Duplicates are handled by the existing admin merge (`MergedIntoId`).
- **Republishing creates a new version with renewed consent.** A new test result for the same product is a new `PublishedEntryVersion` of the same `PublishedEntry` (C42), never an in-place edit; `PublicationConsent` belongs to the version and is required again for each one. Reports, counter-statements and the entry URL stay attached to the entry. Only the profile that published the entry may republish it, and a withdrawn entry cannot be republished (C55).
- **Profile kind is self-declared.** Nothing in the system verifies that the user behind a vendor profile works for the vendor, or that the stated consent exists. Flow A of v3 (workspace route) would not have verified this either. The resulting risks — impersonation, publication about a competitor's product, a vendor posting "independent" praise under a second, individual profile — are recorded for the bundled legal review in §16.2.7 and mitigated, not solved, by moderation (§5.6) and rate limiting.
- **Consent records are never public.** `PublicationConsent` is not part of any directory read DTO; it exists as an evidence record for disputes and moderation.

---

## 5. Domain model

Presented per schema. `BaseEntity` (`Id`, `CreatedAt`, `xmin`) applies throughout; audit columns `CreatedByUserId` / `ModifiedAt` are added where relevant.

### 5.1 `catalog`

| Entity | Key fields | Notes |
|---|---|---|
| `CatalogRelease` | `Version`, `PublishedAt`, `IsActive`, `ReleaseNotes` | Immutable once published |
| `CatalogReleaseCriterion` | `CatalogReleaseId`, `CriterionRevisionId` | Join table; see below |
| `Criterion` | `Key` (stable, e.g. `BITV-SW-2.1.1`), `Kind` (`Requirement` / `TestProcedure`), `TestTypeId?` (with `TestProcedure`), `Category`, `SupersededByKey?` | Stable identity across revisions. `Kind` and `TestTypeId` new in v6 (C30) |
| `CriterionRevision` | `CriterionId`, `RevisionNumber`, `TitleDe`, `PlainLanguageDe`, `RationaleDe`, `SortOrder` | Content; not owned by a release |
| `StandardReference` | `Standard` (EN 301 549 / WCAG / BIT inklusiv / ISO 9241), `Clause`, `Url` | m:n to `CriterionRevision` |
| `CriterionProcedureText` | `CriterionRevisionId`, `TestTypeId`, `OriginalNumber`, `OriginalTitleDe`, `ApplicabilityDe`, `RequirementDe`, `RationaleWhyDe`, `AllowedRatings` (flags), `PositiveExamplesDe`, `NegativeExamplesDe`, `SourceUrl` (anchor) | **New in v5.** Verbatim BIT inklusiv texts per procedure (used with permission); 0..n per revision. Testing procedure, delimitation and references are linked, not copied |
| `AppType` | `Key`, `NameDe`, `HintDe?`, `Functionality` (Open / Closed / NotApplicable), `IsInScope`, `SortOrder` | UI term „Anwendungstyp“. Eight values: Website/Webanwendung, Software (offen/geschlossen), Mobile Anwendung (offen/geschlossen), Dokument, Hardware, Vermittlungs-/Notrufdienst |
| `ProductPart` | `Key`, `NameDe`, `ExplanationDe`, `IsAlwaysSelected`, `SortOrder` | UI term „Funktionsbereich“. Seven values; only `Basisfunktionalität` has `IsAlwaysSelected` |
| `UseCaseTemplate` | `Key`, `NameDe`, `Description`, `IsActive` | UI term „Verwendungszweck“. Global only; organisation-owned templates (P2) are added when needed (G1, v7). No row for „Keiner davon passt“ (= no template). Inactive templates are not offered for new selections but stay resolvable for old configurations and entries; seed „Datenbankgestützte Fachanwendung“ inactive (v6) |
| `FilterExclusion` | `SourceUseCaseId?`, `SourceAppTypeId?`, `SourceProductPartId?`, `TargetAppTypeId?`, `TargetProductPartId?`, `TargetTestTypeId?`, `ReasonDe` | **New in v5.** Typed FKs since v7 (Y5): CHECK exactly one source and exactly one target set; target layer lower than source layer. Immutable once part of a published release |
| `CatalogReleaseAppTypeProcedure` | `CatalogReleaseId`, `AppTypeId`, `TestTypeId` | **New in v7 (C45).** Which procedures' texts and allowed ratings apply to an app type in a release; m:n (Website/Webanwendung may later map to the Software-Test *and* BIK BITV-Test Web, Katalog K-5) |
| `CatalogReleaseFilterExclusion` | `CatalogReleaseId`, `FilterExclusionId` | **New in v6.** Release pinning of exclusions, same pattern as `CatalogReleaseCriterion` |
| `CriterionApplicability` | `CriterionRevisionId`, `AppTypeId`, `ProductPartId?` | Which criteria apply where |
| `PriorityRecommendation` | `CriterionRevisionId`, `AppTypeId?`, `UseCaseTemplateId?`, `Level` (A/B/Info), `Weight`, `RationaleDe`, `Source` | `Source` is a non-nullable enum whose only v1 value is `ExpertJudgement` |
| `TestType` | `Key`, `NameDe`, `Url`, `SortOrder` | UI term „Prüfverfahren“. BIT inklusiv Software-Test, App-Test, PDF-Test; BIK BITV-Test (Web), (App). Also filter layer 4 |

**Filter layers and exclusions (new in v5).** The four selection layers have a fixed order: `UseCaseTemplate` (1) → `AppType` (2) → `ProductPart` (3) → `TestType` (4). A `FilterExclusion` row says that a value of a higher layer makes a value of a lower layer unavailable. The initial rule set (Kriterienkatalog §3.1):

| Source | Excludes |
|---|---|
| UseCase HR-Anwendung | AppType Dokument, Hardware, Vermittlungs-/Notrufdienst; TestType BIT inklusiv PDF-Test |
| AppType Website/Webanwendung | TestType BIT inklusiv App-Test, BIT inklusiv PDF-Test, BIK BITV-Test (App) |

Rules: excluded values are returned with the reason(s) (`excludedBy[]`, §7.2) so the UI can show them disabled with an explanation; a selection containing an excluded value is rejected (Problem Details, 400, field-level). When a higher layer changes, the client clears all lower layers except `IsAlwaysSelected` parts — this is a client rule, the server only validates the final selection. The existing repository models the four layers as `CriteriaFilter` with `FilterTypeId` 1–4 and exclusions as `CriteriaFilterExclusion`; migration step 2 maps these onto the entities above. *Decided (§16.3.2):* HR-Anwendung does **not** exclude Mobile Anwendung; remove the two seed rows `CriteriaFilterExclusion` Id 2 and 3 (17 → 12, 17 → 13) in migration step 2.

**Which texts apply.** `CriterionApplicability` decides whether a criterion applies to a selection. `CriterionProcedureText` decides which wording the detail view shows; `CatalogReleaseAppTypeProcedure` names the procedure(s) per release: Software-Test texts for Website/Webanwendung (interim) and Software, App-Test texts for mobile apps, PDF-Test texts for Dokument. Hardware and Vermittlungs-/Notrufdienst have no BIT inklusiv procedure yet (no mapping row, §16.3.3). Because the mapping is release-pinned, a changed mapping never alters the texts or allowed ratings of an older configuration (ADR-002). With several app types the detail view shows one text block per applicable procedure.

**Allowed ratings (v6, C31).** For a criterion and a selection of app types, the allowed ratings are the **union** of `AllowedRatings` over the procedure texts of the procedures mapped to those app types in the pinned release. `Unknown` is always allowed. If no procedure text applies (e.g. only Hardware), all six levels are allowed. The same rule validates `CandidateAssessment` (against the project's configuration), Flow E assessments (against the stated app types) and Flow D assessments (against the stated app types, or all six levels without a statement). Rationale: an assessor must never be unable to record a level that is correct for one of the selected app types.

**Procedure criteria (v6, C30).** For every `TestType` the catalog holds one criterion with `Kind = TestProcedure` (key `PROC-<testTypeKey>`, title „Geprüft nach {{testType}}“), a catalog-default `PriorityRecommendation` of level B, no `CriterionProcedureText` and no `CriterionApplicability` rows. It enters a selection only through layer 4: selecting a test type adds its procedure criterion. It is listed, can be deselected and is exported like any other criterion. It has **no assessment rows**: its answer (Ja / Nein / Nicht angegeben) is **derived** from the product's test evidences (C46, §5.4) — Ja if an evidence with that `TestTypeId` exists, Nein if the test status is `NotTested`, otherwise Nicht angegeben. Every fulfilment projection covers `Kind = Requirement` only — the test status is already shown by K4, counting it again in K1 would weigh it twice.

**Empty selections.** If the selected app types have no applicable requirement criteria (today: only Hardware and/or Vermittlungs-/Notrufdienst), the criteria endpoint returns those app types in `appTypesWithoutCriteria[]`; the UI shows a dedicated empty state instead of an error.

**Release composition.** v1 put `CatalogReleaseId` on `CriterionRevision`, which forced a new revision of every criterion — plus its applicability and priority rows — for every release, even for the ~95 % that did not change. A release is now a **set of revisions**, assembled through `CatalogReleaseCriterion`. Publishing a release that changes three criteria means three new revisions and one new set of join rows; unchanged revisions are referenced by both releases. Reproducibility is identical — a configuration pinned to a release still resolves to exactly the revisions that release contained.

**Priority resolution** — deterministic, most-specific-wins:

```
1. (criterion, appType, useCaseTemplate)   most specific
2. (criterion, appType, null)
3. (criterion, null, useCaseTemplate)
4. (criterion, null, null)                 catalog default
```

**Several app types (v5).** Rules 1 and 2 are evaluated per selected app type; the strictest result wins (A > B > Info, then the higher weight, then the lower `AppType.SortOrder` as a deterministic tie-break), and the matched rule names that app type. With no use case („Keiner davon passt“) only rules 2 and 4 apply.

The API returns the matched rule together with its `RationaleDe` and `Source`, so the UI can answer *"why is this an A criterion?"* — the technical implementation of PRD §7 "Transparenz der Priorisierungslogik". A priority that cannot explain itself is a bug.

**Assessment scope of a published entry (v7, C43).** A test-result entry stores one `PublishedAssessment` per requirement criterion in its scope — `Unknown` where nothing was assessed. The scope is therefore the set of stored rows, reconstructible forever. When projecting against a configuration: key in the configuration and in the entry's scope → normal comparison; key in the entry's release but outside its scope → `Unknown` (the publisher did not assess it); key not in the entry's release → `NotCoveredByThisRelease` (table below).

**Cross-release matching.** A `PublishedEntry` is pinned to the release that was active when it was published; a `CriteriaConfiguration` may be pinned to a newer one. Matching runs on `Criterion.Key`, with these rules:

| Situation | Result for that criterion |
|---|---|
| Key present in both releases | Normal comparison against the assessed value |
| Key added after the entry was published | `NotCoveredByThisRelease` |
| Key removed in the newer release | Excluded from the projection entirely |
| Criterion split or merged | `Criterion.SupersededByKey` maps old → new; if the mapping is not 1:1, `NotCoveredByThisRelease` |

`NotCoveredByThisRelease` is a **distinct value, never folded into `Unknown` or `Blockade`**. Without this distinction an honestly assessed older entry would score worse than a newer one purely because the catalog grew — the same perverse incentive the two-signal principle exists to prevent, one level down. The API surfaces it as its own count (`notCovered`) and the UI renders it as "Damals nicht im Katalog", not as a gap.

**Versioning rationale:** criteria exported into Vergabeunterlagen can become contractual. Mutating criterion text in place would make it impossible to reconstruct what was demanded. Releases are deliberately coarse — expect a handful per year, edited by a `CatalogEditor`. **→ ADR-002** *(confirmed by the project owner, 2026-09-29)*

### 5.2 `identity`

| Entity | Key fields | Notes |
|---|---|---|
| `AppUser` | ASP.NET Core Identity | Login credentials only |
| `Organization` | `Name`, `Type` (public body / private company / association), `CreatedByUserId` | Tenant boundary |
| `OrganizationMembership` | `OrganizationId`, `UserId`, `Role` (Owner/Editor/Reader), `InvitedAt`, `AcceptedAt` | |
| `OrganizationInvitation` | `Email`, `Token`, `ExpiresAt` | |
| `ContributorProfile` | `DisplayName` (pseudonym), `Kind` (Individual / Institution / Vendor / Auditor) | **No account reference, no organisation reference.** `Kind` is self-declared (§4.6, §16.2.7) |
| `UserContributorLink` | `UserId`, `ContributorProfileId` | Restricted table; never exposed through any DTO. One user may hold several profiles |
| `UserPreferences` | UI customisation, language, appearance (`System` / `Light` / `Dark`, C73), contrast/motion settings | Extends existing `UICustomization` |

An authenticated user may exist **without any organisation membership** — this is the normal state for a Flow D or Flow E contributor, not an edge case, and no endpoint outside the workspace and configurator modules may require a membership.

### 5.3 `configurator`

| Entity | Key fields |
|---|---|
| `CriteriaConfiguration` | `OrganizationId`, `Name`, `CatalogReleaseId`, `UseCaseTemplateId?`, `CreatedByUserId`, **`ProcurementProjectId?`** (**new in v10**, C53: set = copy owned by that project; null = list of the organisation) (**`AppTypeId` removed in v5**) |
| `ConfigurationAppType` | `ConfigurationId`, `AppTypeId` (**new in v5**, at least one) |
| `ConfigurationProductPart` | `ConfigurationId`, `ProductPartId` |
| `ConfigurationTestType` | `ConfigurationId`, `TestTypeId` (**new in v5**, optional; each adds its procedure criterion, C30) |
| `ConfigurationCriterion` | `ConfigurationId`, `CriterionRevisionId`, `IsIncluded`, `ResolvedLevel`, `OverrideLevel?`, `OverrideWeight?`, `NoteDe` |

`ShareLink` (`OrganizationId`, `TokenHash`, `ResourceType`, `ResourceId`, `Scope`, `ExpiresAt`, `RevokedAt?`) lives in Platform (v7, C44). `ExpiresAt` is required: default 90 days, at most 1 year after creation (v11, C81). Since v10 (C59) it carries `OrganizationId` and is covered by the global query filter for management (create, list, revoke); the token is shown once and stored only as a SHA-256 hash; `ResourceType` is `Configuration` only in v1 (no project share links, D7). The anonymous read `GET /api/v1/shared/{token}` hashes the token, looks the link up with the filter bypassed at this one documented call site, and asks the owning module's `IShareableResourceProvider` for authorisation (not revoked, not expired, resource still exists) and for a redacted read DTO (no organisation, no user names, no project). A project copy (`ProcurementProjectId` set) cannot be shared.

`ProcurementProjectId` is a plain id without a navigation property: Configurator does not depend on Workspace. Workspace creates and replaces the copy through Configurator.Contracts (C53). The column and its foreign key are added in migration step 5, when `ProcurementProject` exists.

Storing the resolved priority alongside the override preserves the audit trail: *what did the tool recommend* vs. *what did the user decide*. Valuable for the pilot evaluation of the prioritisation logic (PRD §5, goal 4). Anonymous ad-hoc exports (§4.4) do not create rows here; the pilot evaluation therefore covers saved configurations only, which is acceptable because pilot partners work with accounts.

### 5.4 `workspace`

| Entity | Key fields | Notes |
|---|---|---|
| `ProcurementProject` | `OrganizationId`, `Name`, `Status`, `CriteriaConfigurationId?` | The Beschaffungsvorgang. `CriteriaConfigurationId` points to the project's own copy (C53). It can be *replaced* only while no candidate has an assessment (a value other than `Unknown`); editing the copy within its release stays possible (C47, C51) |
| `CandidateProduct` | `ProcurementProjectId`, `SoftwareProductId?`, `Name`, `VendorName`, `WebsiteUrl`, `FunctionalSummary`, **`TestStatus` (Tested / NotTested / Unknown, not null, default Unknown)**, `ContactNotes` 🔒, `InternalNotes` 🔒 | 🔒 never published. `TestStatus` moved here from `TestEvidence` in v7 (C46) |
| `TestEvidence` | `CandidateProductId`, `TestKind` (Procedure / Other / Own), `TestTypeId?` (with Procedure), `OtherTestTypeName?` (with Other, required), `TestedByName?`, `TestDate?`, `TestedVersion?`, `TesterQualificationNote` 🔒, `EvidenceUrl?`, `AttachmentId?` | **0..n per candidate since v7 (C46)**, only while `TestStatus = Tested`; at most one row per `TestTypeId`; a new product version updates the row of that procedure (C65) |
| `CandidateAssessment` | `CandidateProductId`, `CriterionRevisionId`, `Fulfillment` (Fulfilled / SlightRestriction / Restriction / Barrier / Blockade / NotApplicable / **Unknown**), `EvidenceNote` | Requirement criteria only (procedure criteria are derived, C46). Rows are created lazily on the first value; no row means `Unknown` (C53). An *assessment exists* when a row has a value other than `Unknown`. A rating outside the allowed union (C31) is rejected |
| `Attachment` | `FileName`, `ContentType`, `SizeBytes`, `StoragePath` | |

**Test status (changed in v4).** Buyers often do not know yet whether a candidate has been tested. v3 forced a yes/no answer, which in practice would have been recorded as "no" and later published as a false negative. The workspace now records `Unknown` explicitly. `Unknown` is a working state only: it never leaves the workspace. `PublicationService` rejects publication while `TestStatus = Unknown` with a field-level Problem Details error on `testStatus`, and maps `Tested`/`NotTested` to the published boolean. The F8 invariant — every published entry shows "getestet: ja/nein" — is unchanged and still enforced by a non-nullable column on the published side.

`Unknown` as an explicit value (for assessments and for the test status) prevents "not yet known" from being silently read as "fails".

**Several test evidences (v7, C46).** A product can be tested by several procedures (e.g. BIT inklusiv Software-Test and BIK BITV-Test). One row per procedure is the single source of truth: the procedure criteria of the project's list show the derived Ja/Nein, and the directory filter `testTypeIds[]` matches entries whose published evidences contain that procedure. Rejected: one evidence row plus independently entered procedure answers (two sources that can contradict each other).

**Criteria list of a project (v7, C47; clarified in v8, C51; project copy since v10, C53).** Assigning a saved list to a project copies it: the copy is a `CriteriaConfiguration` with `ProcurementProjectId` set, owned by the project, never offered for other projects and never shareable. The organisation's list stays unchanged, so an adjustment in one project never affects another project or a share link. After assignment the copy's four selection layers (use case, app types, product parts, test types) are fixed; its criteria can still be included, excluded, re-prioritised and annotated. Assessments are pinned to the copy's release, which is what makes the seven-count workspace projection valid (C34). v1 therefore forbids *replacing* the copy (by a copy of another list, or of a list on a newer catalog release) once an assessment exists, i.e. a `CandidateAssessment` with a value other than `Unknown` (409 with field error); until then, replacing deletes the old copy. What stays possible:

| Situation | Handling in v1 |
|---|---|
| The buyer's requirements change (criteria added or removed, priorities, notes) | Edit the project's copy within its release. Newly included criteria start as `Unknown` for every candidate; excluded criteria keep their assessments, hidden while excluded |
| A candidate product is developed further (new version) | Re-assess the changed criteria; update the test evidence of that procedure (`TestedVersion`, date, C65); a published entry is updated by republishing under the same contributor profile (new `PublishedEntryVersion`, C55) |
| A newer catalog release should be used | New project with a copy of the re-pinned list (`POST /api/v1/configurations/{id}/copies`, D6, §16.3.1); candidates are added again. Carrying assessments over by `Criterion.Key` (with a report of what could not be carried over) is the planned extension and needs no schema change |

**Tenant isolation:** every organisation-scoped entity carries `OrganizationId` and is protected by an EF Core global query filter bound to an `IOrganizationContext` resolved per request. A forgotten `WHERE` clause then cannot leak one organisation's procurement into another's view. This implements the business argument from the team feedback — a company's accessibility maturity must not become visible to competitors. Admin queries opt out via `IgnoreQueryFilters()` at documented call sites only.

### 5.5 `directory`

| Entity | Key fields | Notes |
|---|---|---|
| `SoftwareProduct` | `Name`, `VendorName`, `WebsiteUrl`, `NormalizedKey`, `MergedIntoId?` | **Identity only — carries no accessibility claim** |
| `ProductIdentityRequest` | `ProposedName`, `ProposedVendor`, `RequestedByContributorProfileId`, `State` | Flow D's path to an unlisted product |
| `PublishedEntry` | `SoftwareProductId`, `Channel` (VendorOrAuditor / Independent), `OriginKind` (WorkspacePublication / DirectContribution / VendorPublication), `ContributorProfileId`, `State`, `CurrentVersionId`, `SourceCandidateProductId?`, `FirstPublishedAt` | **Stable identity of a statement (C42).** **No `OrganizationId`.** `Channel = VendorOrAuditor` ⇔ `OriginKind = VendorPublication`. `SourceCandidateProductId` is never serialised and is `ON DELETE SET NULL` (Y8). Moderation, reports and counter-statements reference this row. Republication keeps `State` (C55) |
| `PublishedEntryVersion` | `PublishedEntryId`, `VersionNumber`, `PublishedAt`, `CatalogReleaseId`, `WasTested?` (bool), `ContextSource?` (ProjectSelection / VendorDeclared / ContributorDeclared), `ClientRequestId` (unique), `RequestHash` | **New in v7 (C42), immutable** (except `ContentPurge`, C56, C66). `ClientRequestId`/`RequestHash` since v10 (C64). `WasTested` is not null for WorkspacePublication and VendorPublication, null for DirectContribution (C28). `ContextSource` set iff the version has app types or use cases |
| `PublishedEntryAppType` | `PublishedEntryVersionId`, `AppTypeId` | C32; 0..n; Flow E ≥ 1 |
| `PublishedEntryUseCase` | `PublishedEntryVersionId`, `UseCaseTemplateId` | C32; Flow B at most 1, Flow E 0..n, Flow D none (C57) |
| `PublishedAssessment` | `PublishedEntryVersionId`, `CriterionRevisionId`, `Fulfillment`, `EvidenceNote` | Requirement criteria only. Test-result versions: one row per criterion in scope (C43); experience reports: only assessed criteria |
| `PublishedTestEvidence` | `PublishedEntryVersionId`, `TestKind`, `TestTypeId?`, `OtherTestTypeName?`, `TestedByName?`, `TestDate?`, `TestedVersion?`, `EvidenceUrl?` (C67) | Redacted copy; 0..n per version, only when `WasTested = true` (C46); none for DirectContribution |
| `ConformanceReport` | `PublishedEntryVersionId`, `Format` (VPAT 2.x EU/INT), `ReportDate`, `AttachmentId?`, `SourceUrl?` | P1, Flow E |
| `ExperienceReport` | `PublishedEntryVersionId`, `NarrativeDe`, `AssistiveTechnologyContext?`, `ProductVersionUsed?`, `ConsentedAt` (not null), `ConsentTextVersion` | Kanal 2; consent required for every report (C48); contributor is on the entry; `NarrativeDe`, `AssistiveTechnologyContext` and `ProductVersionUsed` are set to null by `ContentPurge` when consent is withdrawn (C56) |
| `PublicationConsent` | `PublishedEntryVersionId`, `ConsentGivenBy`, `OnBehalfOf`, `ConsentedAt`, `EvidenceNote` | **Flow E only** (Kanal 1); one per version; never in a read DTO |

**Entry and version (v7, C42).** v6 modelled republication twice (a `Version` column on `PublishedEntry` and a separate `EntryVersion` table). If each republication created a new entry row, reports, counter-statements, the moderation log and every shared URL would stay attached to the outdated row. Now the entry is the stable thing people refer to; its versions are immutable snapshots, and `CurrentVersionId` selects the one the directory shows. Withdrawal changes the entry's `State`, never a version.

`State` (on `PublishedEntry`): `Published → Flagged → UnderReview → (Published | Restricted | Withdrawn)`. Content is never hard-deleted; withdrawal hides it and retains a minimal moderation record. The one exception is the personal content of an experience report, which `ContentPurge` removes when consent is withdrawn (C56). Republication keeps the current `State`; a withdrawn entry is never republished (C55).

**Enforcement (v10, C66).** An EF Core `SaveChangesInterceptor` rejects `Modified` and `Deleted` states for `PublishedEntryVersion` and its children (`PublishedEntryAppType`, `PublishedEntryUseCase`, `PublishedAssessment`, `PublishedTestEvidence`, `ConformanceReport`, `ExperienceReport`, `PublicationConsent`), except inside a `ContentPurge` scope, which may only set the purgeable fields to null. The rule "`WasTested` is not null for `WorkspacePublication` and `VendorPublication`" spans two tables (`OriginKind` is on the entry, `WasTested` on the version) and is therefore enforced by the factory, not by a CHECK constraint.

**Product identity approach:** identities are created *as a by-product of publication* (Flows B and E), never maintained in advance. The user searches existing products (trigram similarity on `NormalizedKey` via `pg_trgm`) and either links or creates. Flow D may only link; unlisted products go through `ProductIdentityRequest`. A `PlatformAdmin` merges duplicates via `MergedIntoId`. This gives aggregation across organisations without reintroducing the rejected central product database: no product exists that nobody has written about, and WERTE.IT never asserts anything about a product itself. **→ ADR-005** *(confirmed by the project owner, 2026-09-29; extended to Flow E in v4)*

**Context on the entry, not on the product (v6, C32; stored per version since v7).** App types and use cases describe the context of one statement, not a property of the product. The same product can be a web application in one procurement and a mobile app in another, and its use case is a property of the procurement. Storing them on `SoftwareProduct` would make the identity carry a claim (against ADR-005) and would need an owner who decides between conflicting statements. On the entry, every statement keeps its own provenance (`ContextSource`) and is withdrawn with the entry. The directory filters "Anwendungstyp" and "Verwendungszweck" therefore mean *"products with at least one visible entry in this context"*; entries without a use case are returned only when the use-case filter is empty. Experience reports count for the app-type filter (they describe use in a context) but never for the test-type filter (they are not tests); they carry no use case and therefore never match a set use-case filter (C57). Contexts from Flows D and E are validated against `FilterExclusion` like any selection. Rejected: variant B (context on the product), variant C (no use-case filter in the directory).

**"Merken" and "Teilen" of products (PRD §9 P0, Y4).** "Merken" means adding a directory product to a procurement project from the project overview (`POST /api/v1/projects/{id}/candidates { softwareProductId }`, prefilled from the identity). "Teilen" means the public product URL; projects are not shared by link in v1 (D7, C59). Users without an organisation have no saving in v1; the directory itself needs no account.

**No aggregate rating exists.** There is deliberately no `AverageRating`, no `AccessibilityScore`, no aggregated "Betroffenen-Meinung". Experience reports are always returned as an ordered list of individual contributions, each carrying its own assistive-technology context — enforcing the participation principle from brainstorm §3.2 at the schema level.

### 5.6 `moderation`

| Entity | Key fields |
|---|---|
| `ContentReport` | `PublishedEntryId`, `PublishedEntryVersionId?`, `ReporterUserId?`, `Reason`, `Description`, `State`, `CreatedAt` |
| `CounterStatement` | `PublishedEntryId`, `PublishedEntryVersionId?`, `AuthorContributorProfileId`, `TextDe`, `PublishedAt` |
| `ModerationAction` | `PublishedEntryId`, `PublishedEntryVersionId?`, `ModeratorUserId`, `Action`, `ReasonDe`, `CreatedAt` |

All three reference `PublishedEntry` (the stable identity, C42) and, since v10 (C63), optionally the `PublishedEntryVersionId` that was shown when the report, statement or action was made, so a moderator can see which wording was meant. `CounterStatement` is a first-class sibling of the entry, rendered alongside it — never replacing it. This is the technical shape of "Recht auf Gegendarstellung statt Löschung" (PRD §7). `ModerationAction` is append-only and forms the legal defensibility record.

### 5.7 `analytics` (privacy-preserving)

| Entity | Fields |
|---|---|
| `UsageEvent` | `EventType`, `OccurredOn` (date, not timestamp), `Dimension1?`, `Dimension2?` |

*As built (C91):* table `analytics."UsageEvents"` with a random UUID v4 key `Id`; `OccurredOn` is the UTC day. PRV-05 allows exactly these columns. Writing goes through `IUsageEventWriter` (one method per event), which awaits the insert in its own scope and never lets a failure reach the caller.

No user ID, no organisation ID, no IP address, no session identifier — date granularity only. Sufficient for the PRD §10 indicators, including anonymous exports (`ExportCreated`).

**Event types (v10, C69).** Events are written only for indicators that cannot be counted from existing rows: `CriteriaListShown` (S11 shown for a complete selection; dimension: with or without account) and `ExportCreated` (dimensions: format; with or without account). Workspace entries, published versus private entries and published entries over time are counted from the existing tables as aggregate numbers. The list is final (SPEC_v2 §9.1, C82): no other events in v1. Anything more granular would require a DSGVO justification the project does not need.

---

## 6. Key design decisions and trade-offs

| ID | Decision | Alternatives rejected | Trade-off accepted |
|---|---|---|---|
| **D1** | Modular monolith, one deployable | Microservices; separate criteria and workspace services | No independent scaling or deployment. Irrelevant at this load; boundaries extractable later. |
| **D2** | Versioned catalog with immutable releases composed from shared revisions | Mutable criteria rows; release-owned revisions (v1) | One extra join table and an editorial release step. Bought: reproducibility of tender documents without a full clone per release. |
| **D3** | Erfüllungsgrad computed per request, never stored | Stored score per product | **Directory cannot be sorted or filtered by fulfilment without computing across all entries** — see §11.2. Bought: no stored score can exist. |
| **D4** | Publication = snapshot copy, not a visibility flag | `IsPublic` boolean on workspace entities | Data duplication, possible staleness. Bought: structural redaction, explicit consent per version, safe withdrawal. |
| **D5** | Product identity created on publish (**Flows B and E**, shared resolver), admin merge, request path for Flow D | Central curated product registry; free-text names only; identity requests for vendors | Some duplicates; an admin step for contributor-requested products. Bought: aggregation without the rejected maintenance burden, and no bottleneck for vendors listing their own product. |
| **D6** | Organisation-scoped tenancy with EF global query filters | Row-level checks in each service | Filter must be bypassed deliberately for admin queries. Bought: defence in depth against cross-organisation leaks. |
| **D7** | Resource-based authorisation for workspace/directory, claims for platform admin | Claim-only (current) | Two authorisation mechanisms to understand. Necessary: permissions depend on membership in a specific organisation. |
| **D8** | **ASP.NET Core Identity cookie authentication** (`HttpOnly; Secure; SameSite=Strict`, sliding expiration) **with antiforgery** for state-changing requests (revised in v8, C50) | JWT in `localStorage` (current); refresh cookie + in-memory access token + silent refresh (v1–v7) | Same-origin deployment only (SPA and API behind one proxy), which is the planned topology (§10.1). Bought: XSS yields no stealable token, and there is no token plumbing to maintain — material for a tool holding disability-related data and for a solo maintainer. |
| **D9** | Synchronous export, no job queue | Hangfire / background worker | Long exports block a request thread. Payloads are small; revisit if p95 export exceeds ~3 s. |
| **D10** | In-process `IMemoryCache` for catalog data | Redis | Cache is per-instance. Fine for a single instance; first thing to change when scaling out. |
| **D11** | Generic CRUD controllers only for catalog admin | Generic CRUD everywhere (current) | More hand-written endpoints. Necessary: workspace and directory invariants would otherwise be bypassable. |
| **D12** | ClosedXML (MIT) for Excel; **licence rule for all dependencies** (extended in v13, C89) | EPPlus (commercial licence since v5); AutoMapper 15+ (RPL-1.5 or commercial) | Fewer features. Required: a publicly funded open-source release must not depend on a commercial licence. **Rule. Shipped dependencies** (API or client image, browser bundle): OSI-approved, no commercial-use restriction, and **no strong copyleft or reciprocal licence** (GPL, AGPL, RPL, OSL …). Weak, file-level copyleft (MPL-2.0, LGPL as a separate library) is allowed if used unmodified. A dual licence counts by the option the project would use. **Development and test tools that are not shipped:** OSI-approved, no commercial-use restriction. |
| **D13** | **Three write paths into Directory, one shared invariant factory** | Single publication path through Workspace (v1); two paths (v2/v3) | The "one crossing" simplification is lost. Necessary: neither the Kanal-2 contributor nor the Kanal-1 vendor has a procurement project, and neither must need an organisation. |
| **D14** | **No virus-scanning sidecar in v1** | ClamAV container (v1 topology) | Accepted residual risk on an authenticated-only upload path. Bought: one less always-on container to maintain; mitigated by type/size allow-list, `Content-Disposition: attachment`, restrictive CSP, and out-of-origin serving. Revisit if uploads open to unauthenticated users. |
| **D15** | **Unauthenticated, non-persisting export of an ad-hoc selection** (new in v4) | Export only from a saved configuration (v3); anonymous configurations stored with a token | One unauthenticated POST that does work, guarded by rate limit and size caps; no server-side record of anonymous overrides. Bought: the core flow of the primary persona works without registration (PRD §5, goal 1). |
| **D16** | **One write path per Kanal-1 origin: Kanal 1 only via Flow E; Flow B always Kanal 2** (new in v4) | Channel choice in Flow B plus Flow E (two Kanal-1 paths) | A vendor that also procures cannot publish Kanal 1 from its workspace and must use Flow E. Bought: no buyer can mislabel an independent entry as a vendor entry; consent capture exists in one place. |

---

## 7. API design

### 7.1 Conventions

- REST over JSON, base path `/api/v1`. Versioning via URL segment; `v1` stable for the funding period.
- Errors as **RFC 9457 Problem Details**, with a machine-readable `errors` map keyed by field path. Not cosmetic: the client must bind server-side validation errors to specific form controls and announce them (WCAG 3.3.1). A generic error string would make the form inaccessible.
- Pagination: offset-based everywhere (`?page=&pageSize=`); at the expected 2,000 entries cursor paging buys nothing and conflicts with `sort=entries` (G3, v7).
- Concurrency: `ETag` derived from `xmin`, `If-Match` required on updates to shared workspace resources; conflicts return `409` with the competing editor's display name. **Caveat:** `xmin` is not preserved across `pg_dump`/restore, so ETags issued before a restore may match incorrectly after one. At RPO 24 h this is a theoretical window; the restore runbook in `TECHNICAL.md` must instruct operators to invalidate client caches (bump an `ETagEpoch` config value included in the ETag) after any restore.
- Idempotency (v10, C64): all three publication paths carry a client-generated `clientRequestId` (UUID per form instance). `PublishedEntryVersion` stores it in `ClientRequestId` with a unique index on that column alone, together with `RequestHash` (SHA-256 of the normalised request body). A replay with the same id and the same hash returns the original result; the same id with a different hash returns 409. There is no separate key store (G3, v7).
- Antiforgery failures return 400 Problem Details with their own `type`, so the client can tell them apart from validation errors (C60).
- All list endpoints honour `Accept-Language` (de/en).
- Problem `type` URIs (v11, C78): `<base>/problems/<name>` with a configurable base; names `validation`, `antiforgery`, `rate-limit` (with `retryAfterMinutes`), `conflict` (with `editorDisplayName`), `locked-criteria-list`, `test-status-unknown`, `no-criteria-list`, `profile-mismatch`, `entry-withdrawn`, `selection-excluded`, `release-mismatch`, `email-unconfirmed`, `not-found` (SPEC_v2 §8.6).

### 7.2 Endpoint map (indicative)

**Catalog (public read, editor write)**

```
GET    /api/v1/catalog/releases/active
GET    /api/v1/catalog/app-types
GET    /api/v1/catalog/product-parts
GET    /api/v1/catalog/use-case-templates
GET    /api/v1/catalog/test-types
GET    /api/v1/catalog/filter-options?release=&useCase=&appTypeIds[]=&productPartIds[]=
         → { release, layers: { useCases[], appTypes[], productParts[], testTypes[] } },
           each item { id, name, …, excludedBy: [ { layer, id, name } ] }
           (final shape: SPEC_v2 §8.1, C77; no reason text — the client builds it)
GET    /api/v1/catalog/criteria?release=&useCase=&appTypeIds[]=&productPartIds[]=&testTypeIds[]=
         → criteria [ { kind, allowedRatings[], procedureTexts[],
                        resolvedPriority { level, weight, rationale, source, matchedRule } } ],
           appTypesWithoutCriteria: [ { id, name } ]       (final shape: SPEC_v2 §8.2, C77)
         400 with field errors if the selection violates a FilterExclusion
```

**Export without account (new in v4)**

```
POST   /api/v1/exports
         { format: "xlsx" | "csv",
           selection: { releaseVersion, useCaseTemplateId?, appTypeIds[], productPartIds[],
                        testTypeIds[] },
           criteria: [ { criterionRevisionId, isIncluded, overrideLevel?,
                         overrideWeight?, note? } ] }
         unauthenticated; rate-limited; request-size capped;
         priorities re-resolved on the server; nothing persisted;
         response: file stream with Content-Disposition: attachment
```

**Configurator (authenticated, org-scoped)**

```
POST   /api/v1/configurations                       accepts an initial criteria[] array,
                                                    so a draft can be saved in one call
GET    /api/v1/configurations/{id}
PATCH  /api/v1/configurations/{id}/criteria/{criterionRevisionId}
         include/exclude, override, note; the four selection layers of a
         project copy cannot be changed (C53)
POST   /api/v1/configurations/{id}/copies           { release: "active" } — re-pinning (D6,
                                                    §16.3.1); creates a new organisation list
POST   /api/v1/configurations/{id}/exports          { format: "xlsx" | "csv" }
POST   /api/v1/configurations/{id}/share-links      organisation lists only; 409 for a
                                                    project copy (C59)
DELETE /api/v1/share-links/{id}                     revoke
GET    /api/v1/shared/{token}                        unauthenticated → 200 with a redacted
                                                    configuration DTO; 404 if unknown, revoked
                                                    or expired (D7, C59)
```

**Session (v8, C50; v10, C60)**

```
GET    /api/v1/auth/session               always 200: { user: null } when anonymous,
                                          { user, expiresAt, absoluteExpiresAt } when signed in;
                                          (re)issues the XSRF-TOKEN cookie; never renews the session
POST   /api/v1/auth/login                 sets the Identity cookie; issues a new XSRF-TOKEN
POST   /api/v1/auth/logout                clears the cookie; issues a new XSRF-TOKEN
POST   /api/v1/auth/session/extend        renews the idle limit ("Angemeldet bleiben")
every authenticated response:             X-Session-Expires-At, X-Session-Absolute-Expires-At
                                          (ISO 8601, UTC)
```

**Workspace (authenticated, membership-checked)**

```
GET    /api/v1/organizations/{orgId}/projects
POST   /api/v1/organizations/{orgId}/projects
GET    /api/v1/projects/{id}
POST   /api/v1/projects/{id}/candidates               { softwareProductId? , name, vendor, … }
                                                    "Merken" from the directory (Y4)
PUT    /api/v1/projects/{id}/criteria-configuration  { sourceConfigurationId } — copies an
                                                    organisation list into the project (C53);
                                                    replacing: 409 once an assessment exists
                                                    (value ≠ Unknown, C47); editing the copy
                                                    uses the configurator endpoints (C51)
PATCH  /api/v1/projects/{id}                          name, status
PATCH  /api/v1/candidates/{id}
DELETE /api/v1/candidates/{id}                       Owner/Editor; a published entry stays,
                                                    SourceCandidateProductId → null (C76, SPEC SD21)
PUT    /api/v1/candidates/{id}/assessments/{criterionRevisionId}
PUT    /api/v1/candidates/{id}/test-status          { testStatus: tested | notTested | unknown }
POST   /api/v1/candidates/{id}/test-evidences       one per procedure / other test (C46)
PUT    /api/v1/test-evidences/{id}
DELETE /api/v1/test-evidences/{id}
POST   /api/v1/candidates/{id}/attachments
POST   /api/v1/candidates/{id}/publication    { contributorProfileId, productIdentity,
                                                clientRequestId }
         channel is NOT part of the request (always independent, D16);
         422 with field error on testStatus if unknown; 422 if the project
         has no criteria list (C54); visible entry under the same profile →
         new version; under another profile → 409, withdraw first (C55)
```

**Directory — read**

```
GET    /api/v1/directory/products?q=&useCase=&appTypeIds[]=&testTypeIds[]=&testKinds[]=
                                  &channel=&hasReport=&sort=name|recent|entries&page=&pageSize=
         filters match products with ≥ 1 visible entry in that context (C32);
         useCase set ⇒ entries without use case are not counted; experience
         reports never match it (C57);
         testTypeIds[] matches published test evidences (C46); experience
         reports count for the app-type filter, never for test filters;
         testKinds[] (other, own; multi, C67) covers evidence outside the five
         procedures; testTypeIds[] and testKinds[] are alternatives (OR);
         never sortable by fulfilment (D3)
GET    /api/v1/directory/products/{id}
GET    /api/v1/directory/products/{id}/entries
         public; fulfillment projected against each entry's own criterion scope;
         cacheable
GET    /api/v1/directory/products/{id}/entries?configurationId={id}
         AUTHENTICATED; the configuration is resolved against the caller's
         organisation membership; 404 (not 403) if it belongs to another
         organisation; Cache-Control: no-store
GET    /api/v1/directory/entries/{id}
GET    /api/v1/directory/products/identity-matches?name=&vendor=
         authenticated; trigram candidates for Flows B and E
```

The configuration-scoped variant was public in v1. Since `CriteriaConfiguration` is an organisation-private object, that allowed enumeration of another organisation's criteria selection through ID guessing, and it made an otherwise cacheable public endpoint uncacheable. Both are fixed by splitting the two variants.

**Directory — write (Flows D and E), withdrawal and moderation**

```
POST   /api/v1/directory/products/{id}/contributions                    Flow D
         { contributorProfileId, clientRequestId, narrative, productVersionUsed?,
           assistiveTechnologyContext?, consent: { given, textVersion },
           context?: { appTypeIds[] },          no use cases (C57)
           assessments?[] }   rate-limited; channel forced to "independent";
         422 without consent (C48)
POST   /api/v1/directory/vendor-publications                            Flow E
         { contributorProfileId, productIdentity: { existingId } | { name, vendor, website },
           clientRequestId, wasTested,
           testEvidences: [ { testKind, testTypeId?, otherTestTypeName?, testedVersion?,
                              testedByName?, testDate?, evidenceUrl? } ]   ≥ 1 if wasTested,
           context: { appTypeIds[] (≥ 1), useCaseTemplateIds[] },
           scope?: { productPartIds[] },        required iff assessments are given (C54)
           assessments?[],
           consent: { givenBy, onBehalfOf, consentedAt, evidenceNote? } }
         rate-limited; requires profile kind vendor | auditor;
         channel forced to "vendorOrAuditor"; without assessments no
         assessment rows are written and reads return fulfillment: null
PUT    /api/v1/directory/vendor-publications/{entryId}                  Flow E republish
         same body; creates a new PublishedEntryVersion of the same entry;
         consent required again; only the publishing profile (C55);
         409 for a withdrawn entry
DELETE /api/v1/directory/entries/{id}                                   withdraw
         CanWithdrawEntry: the user linked to the entry's contributor profile,
         or — for WorkspacePublication — an Editor/Owner of the source
         candidate's organisation; for DirectContribution the withdrawal
         also runs ContentPurge (C56)
POST   /api/v1/directory/product-identity-requests
POST   /api/v1/directory/entries/{id}/reports              rate-limited
POST   /api/v1/directory/entries/{id}/counter-statements   vendor/auditor profiles only
```

**Account and organisation (v11, C72, C79, C80)**

```
POST   /api/v1/auth/register                         same response for new and known addresses
POST   /api/v1/auth/confirm-email     { token }      single use, 24 h
POST   /api/v1/auth/confirm-email/resend             rate-limited
POST   /api/v1/auth/password-reset    { email }      same response for known and unknown addresses
POST   /api/v1/auth/password-reset/complete { token, newPassword }   2 h; ends other sessions
GET    /api/v1/account/export                        JSON, no-store (SPEC_v2 §8.4)
DELETE /api/v1/account                { emailConfirmation }   409 for a sole Owner of an
                                                    organisation with other members
PUT    /api/v1/account/preferences                   language, appearance, motion, contrast
POST   /api/v1/organizations                         creator becomes Owner; confirmed e-mail required
GET    /api/v1/organizations/{orgId}/members
PATCH  /api/v1/organizations/{orgId}/members/{userId}   role; 409 for the last Owner
DELETE /api/v1/organizations/{orgId}/members/{userId}   remove or leave; 409 for the last Owner
POST   /api/v1/organizations/{orgId}/invitations     { email, role }; 14 days
DELETE /api/v1/organizations/{orgId}/invitations/{id}
POST   /api/v1/invitations/{token}/accept            bound to the invited address
```

**Administration (v11, C75, confirmed SPEC SD22; `PlatformAdmin` only)**

```
GET    /api/v1/admin/product-identity-requests?state=open
POST   /api/v1/admin/product-identity-requests/{id}/accept   creates the SoftwareProduct,
                                                            e-mails the requester
POST   /api/v1/admin/product-identity-requests/{id}/reject   { reason }; e-mails the requester
POST   /api/v1/admin/products/{id}/merge     { targetProductId }   sets MergedIntoId
```

The configuration-scoped directory variant accepts any configuration of the caller's organisation, including project copies (C83).

*v3 had `DELETE /api/v1/publications/{entryId}` under the workspace.* It is replaced by the directory-level withdrawal endpoint, because entries from Flows D and E have no workspace to withdraw them from.

### 7.3 The contract that carries the most product weight

```jsonc
// GET /api/v1/directory/products/42/entries?configurationId=17   (authenticated)
{
  "product": { "id": 42, "name": "…", "vendorName": "…" },
  "entries": [
    {
      "id": 901,
      "channel": "vendorOrAuditor",       // never merged with "independent"
      "originKind": "vendorPublication",  // workspacePublication | directContribution | vendorPublication
      "contributor": { "displayName": "…", "kind": "auditor" },
      "version": 2,                       // PublishedEntryVersion.VersionNumber (C42)
      "publishedAt": "2026-05-14",
      "catalogRelease": "2026.1",
      "wasTested": true,                  // always present, always boolean (test-result entries: Flows B and E)

      "documentation": {                  // signal 1 — stored
        "hasConformanceReport": true,
        "reportFormat": "VPAT 2.5 EU",
        "reportDate": "2026-03-01",
        "ageInMonths": 5,
        "testEvidences": [                // 0..n (C46)
          { "testType": "BIT inklusiv Software-Test", "testedBy": "…", "testDate": "2026-02-10",
            "testedVersion": "4.2", "evidenceUrl": "https://…" },
          { "testType": "BIK BITV-Test (Web)", "testedBy": "…", "testDate": "2026-02-20" }
        ]
      },

      "fulfillment": {                    // signal 2 — computed vs. configuration 17
        "configurationId": 17,
        "configurationRelease": "2026.2",
        "fulfilled": 51,
        "slightRestriction": 3,
        "restriction": 12,
        "barrier": 5,
        "blockade": 9,
        "notApplicable": 4,
        "unknown": 6,
        "notCovered": 2                   // criteria added after this entry was published
      },                                  // procedure criteria (C30) are never counted;
                                          // null for a Flow E version without assessments (C54)
      "procedureAnswers": [               // C67; configuration-scoped variant only,
        { "testType": "BIT inklusiv Software-Test", "answer": "yes" }   // yes | no | notStated
      ],
      "context": {                        // C32; omitted when the entry has none
        "source": "vendorDeclared",       // projectSelection | vendorDeclared | contributorDeclared
        "appTypes": [ "Website/Webanwendung" ],
        "useCases": [ "HR-Anwendung" ]
      }
      // no "score", "rating", "percentage", "overallStatus" or total field — by design.
      // no consent data — PublicationConsent is never serialised.
    }
  ],
  "experienceReports": [               // Flow D only (E13): no wasTested, no documentation signal
    { "id": 955, "contributor": { "displayName": "…", "kind": "individual" }, "publishedAt": "2026-08-21",
      "assistiveTechnologyContext": "…", "productVersionUsed": "4.2", "narrative": "…",
      "context": null,
      "assessments": [] }               // optional list of assessed criteria; NO counts (C33)
  ]
}
```

A contract test asserts that no property matching `/score|rating|percent|overall/i` is ever serialised from this endpoint, that exactly the eight count properties are present whenever `fulfillment` is not null (Option A, §16.4.1; eight since v5, §16.4.5), that `fulfillment` is null only for `VendorPublication` versions without assessments (C54), that `wasTested` is always present on every item of `entries`, and that no item of `experienceReports` carries `wasTested`, a documentation branch or a `fulfillment` branch.

**Procedure answers (v10, C67).** `procedureAnswers` is derived on the server from the published test evidences — `yes` if an evidence with that procedure exists, `no` if `wasTested` is false, otherwise `notStated` — and returned only in the configuration-scoped variant, for the procedure criteria of that configuration. The client never derives it.

**Workspace scope (v6, C34).** Inside a procurement project the candidate's assessments and the configuration are pinned to the same release, so `notCovered` cannot occur. `GET /api/v1/projects/{id}` returns the seven counts per candidate without `notCovered`, and K1 renders seven rows (`Scope = Workspace`). It also returns `procedureAnswers[]` per candidate for the procedure criteria of the project's copy (C67). The eight-count rule of the contract test applies to directory projections.

---

## 8. Frontend architecture

### 8.1 Store slice restructuring

| Current slice | Target |
|---|---|
| `withIdentity()` | keep; extend with organisation memberships (possibly empty), active organisation and the user's contributor profiles (kinds only, never linked to other users) |
| `withFilters()`, `withCriteria()` | merge into `withCatalog()` (reference data, cached, release-pinned) |
| — | new `withConfigurator()` — **draft selection** (works without an account, in memory only), active saved configuration, overrides, export state. On login the draft can be saved as a configuration without re-entry |
| `withProducts()`, `withFavorites()` | rework into `withWorkspace()` (projects, candidates, assessments, tri-state test status) |
| — | new `withDirectory()` — search, product detail, entries, contribution (Flow D), vendor publication (Flow E) |
| `withProposal()` | **retire** — the proposal/approval workflow is out of scope. Criteria suggestions are an external link (§16.4.2) |
| `withSettings()`, `withLoading()` | keep |

Feature folders keep the existing layout (`models/`, `services/api/`, `state/`, `pages/`, `list/`, `details/`, `dialogs/`). Routes become lazy-loaded per feature; the workspace bundle must not be downloaded by a Flow D or Flow E contributor who never opens a procurement project.

### 8.2 Consequences of zoneless Angular

`provideZonelessChangeDetection()` is already in place; zoneless is the default for new applications from Angular 21 on, so the upgrade to 22 (C37) changes nothing about the change-detection model. The upgrade runs `ng update` one major version at a time (20 → 21 → 22) in migration step 1, before new feature code. Signals-only state means every async boundary must write into a signal; nothing may rely on Zone.js-triggered change detection. Compatible with the NgRx Signals store, but requires care with third-party widgets and `setTimeout`-based patterns.

### 8.3 Interceptor changes (follows D8, revised in v8)

The browser sends the Identity session cookie automatically (`withCredentials` is not needed on the same origin). `authInterceptor` has three jobs (v10, C60):

1. It attaches the antiforgery request token (read from the `XSRF-TOKEN` cookie that the API issues, sent as `X-XSRF-TOKEN`; Angular's `withXsrfConfiguration` covers this). At application start the shell calls `GET /api/v1/auth/session`, which answers 200 also for anonymous users and issues the first `XSRF-TOKEN`, so sign-in, registration and password reset carry a valid token.
2. It reads `X-Session-Expires-At` and `X-Session-Absolute-Expires-At` from every response and updates the shell's session state.
3. On `401` from any endpoint except the session probe, it opens the in-app login overlay (the sign-in form in a dialog, without navigation), holds the failed request and retries it after a successful sign-in. The page, its form input and the configurator draft therefore survive both limits (WCAG 2.2.1). A `401` from `GET /auth/session` is not expected (the probe answers 200) and is ignored.

There is no token in JavaScript and no refresh endpoint.

The session uses sliding expiration. A session expiring without warning violates WCAG 2.2.1, so the shell shows a visible, screen-reader-announced expiry warning with a "stay signed in" action; that action calls `POST /api/v1/auth/session/extend` (a cheap authenticated request that renews the sliding cookie). The shell knows the remaining time from `GET /api/v1/auth/session` at start and from the expiry headers afterwards (`expiresAt` for the idle limit, `absoluteExpiresAt` for the 8-hour limit) and warns `Session:WarningLead` (default 5 minutes) before the earlier of the two (C52, C60). WCAG 2.2.1 requires at least 20 seconds to respond; 5 minutes leaves room for screen-reader users to finish an input.

The anonymous export endpoint is unauthenticated and therefore exempt from antiforgery validation; it changes no server state.

### 8.4 The one UI rule the architecture cannot enforce

Because the two-signal guarantee stops at the API (§4.4), one rule must be carried by the frontend and by review:

> **No view may display a single number, bar, star rating or traffic light that summarises how accessible a product is.** The fulfilment counts are always rendered together (eight in the directory, seven in the workspace, none for experience reports), with `notCovered` labelled as a catalog-scope difference rather than a deficiency. Dokumentationsgrad and Erfüllungsgrad never share a visual container.

The rule is carried by the domain components defined in step 7 (UX copy §4.2: K1 `pa-fulfillment-breakdown`, K2 `pa-documentation-signal`), by the code-review checklist, and by the accessibility/UX review of the directory screens. Code Connect is not available on the Figma plan in use (C40); components are linked to Figma through documentation links on the Figma components, `@figma` JSDoc tags in code and the CI script `scripts/check-figma-links.mjs` (`figma_code_mapping_procureaccess_v1.md`).

---

## 9. Cross-cutting concerns

### 9.1 Authentication and authorisation

**Roles**

| Scope | Roles |
|---|---|
| Platform | `PlatformAdmin`, `CatalogEditor`, `Moderator` |
| Organisation | `Owner`, `Editor`, `Reader` (rows in `OrganizationMembership`) |
| Contributor | any authenticated user with a `ContributorProfile` — **no organisation required**; Flow E additionally requires a profile of kind Vendor or Auditor |
| Anonymous | catalog reads, public directory reads, ad-hoc export, shared-list reads (D7, C59), session probe |

The existing `Approver` role and `objects:approve` claim are repurposed to `Moderator` / `moderation:review`. `Member` becomes an organisation-level role rather than a global one.

**Implementation:** ASP.NET Core resource-based authorisation (`IAuthorizationHandler<TRequirement, TResource>`) with requirements such as `CanEditProject`, `CanPublish`, `CanContribute`, `CanPublishAsVendor`, `CanWithdrawEntry`, `CanModerate`. Claim-based policies remain for platform endpoints. Every workspace endpoint resolves the organisation from the resource, never from a client-supplied header. `CanContribute` and `CanPublishAsVendor` deliberately check only authentication plus an existing contributor profile (of the required kind); they never read memberships.

**Session (v8, C50; implementation details v10, C60).** ASP.NET Core Identity with cookie authentication (`AddIdentityCookies`), `Cookie.HttpOnly`, `SecurePolicy = Always`, `SameSite = Strict`; API endpoints return `401`/`403` instead of redirecting (`OnRedirectToLogin` and `OnRedirectToAccessDenied` overrides). Limits: **30 minutes idle** and **8 hours absolute** from sign-in (C52), configured as `Session:IdleTimeout`, `Session:AbsoluteTimeout` and `Session:WarningLead`. All time reads go through `TimeProvider`, so tests control the clock.

- *Sign-in time.* At sign-in the sign-in time is stored in `AuthenticationProperties.Items["pa:signedInUtc"]`. `IssuedUtc` is not used for the absolute limit, because the cookie handler rewrites it on every renewal.
- *Validation.* `OnValidatePrincipal` first calls `SecurityStampValidator.ValidatePrincipalAsync` (replacing the event would otherwise switch the security-stamp check off), then rejects the principal and signs out when now ≥ sign-in time + absolute limit.
- *Sliding renewal.* `OnCheckSlidingExpiration` sets `ShouldRenew = true` for every authenticated request except `GET /api/v1/auth/session`. The default renews only after half of the window, which would make the effective idle limit vary between 15 and 30 minutes. Each renewal sets `ExpiresUtc` to min(now + idle limit, sign-in time + absolute limit).
- *Extend.* `POST /api/v1/auth/session/extend` sets `ExpiresUtc` explicitly (same formula) and signs in again with the existing `Items`, so the sign-in time survives.
- *Expiry headers.* Every authenticated response carries `X-Session-Expires-At` and `X-Session-Absolute-Expires-At`; the interceptor reads them (§8.3).
- *Security stamp.* Password change and account deletion update the security stamp. The validation interval is short (default: every request, affordable at the expected load, §11.1), so the user's other sessions end at their next request.
- *Antiforgery.* `AddAntiforgery` with header `X-XSRF-TOKEN`; validation on all non-GET endpoints except the anonymous export. `GET /auth/session` (also anonymous) issues the token cookie; sign-in and sign-out issue a new one, because the token is bound to the user identity. Validation failures return 400 Problem Details.
- *Account security (v11, C72).* `RequireConfirmedEmail` is not used for sign-in; instead a policy `ConfirmedEmail` guards saving lists, creating organisations, accepting invitations, creating contributor profiles and all publication paths (403 Problem Details `email-unconfirmed`). Lockout: 5 failed attempts, 15 minutes. Password: at least 12 characters, at most 128, no composition rules. Token lifetimes: confirmation 24 h, password reset 2 h, invitation 14 days; all single use and stored hashed. Registration answers identically for new and known addresses; a known address receives an „already registered“ e-mail instead.
- *Data Protection keys.* Session cookie and antiforgery tokens are protected with ASP.NET Core Data Protection. The key ring is persisted in the database (`PersistKeysToDbContext`, table `DataProtectionKeys` in the `identity` schema) and included in the backup (§10.4); without this, every container restart would end all sessions. `TECHNICAL.md` documents the key store.

### 9.2 Rate limiting and abuse protection

Built-in ASP.NET Core rate limiter with per-endpoint policies (default values in SPEC_v2 §7.3, all configurable, C84): workspace publication, direct contribution, **vendor publication**, product-identity requests, report submission, counter-statements, registration, password reset, and **anonymous export** (partitioned by client IP at the proxy; the IP is used for limiting only and never stored, §9.3).

Anonymous export additionally: request body size cap, criteria array bounded by the release's criterion count, note length cap.

Uploads (P1, ACR documents): authenticated Flow E submitters only; MIME and extension allow-list (PDF, ODF, OOXML); size cap; stored outside the web root with generated names; served from a separate origin with `Content-Disposition: attachment` and a restrictive CSP. No malware scanner in v1 (D14).

### 9.3 Data protection (DSGVO)

Central hosting makes the operating body the controller. Four architectural consequences:

1. **Special-category data.** An experience report such as *"as a screen reader user I could not complete this workflow"* is health-related data under Art. 9 GDPR, even under a pseudonym. Therefore: contributor identity lives in a separate table (`UserContributorLink`) that no DTO touches; no endpoint returns both the account and the profile; correlation of user IDs with published entry IDs must not appear in logs above `Debug`. Every experience report requires explicit consent with a stored consent-text version (C48); withdrawing consent purges the report's personal content (C56, §16.2.8).
2. **Pseudonymity must survive the organisation model.** This is why `PublishedEntry` has no `OrganizationId` and why `CanContribute` does not read memberships. A contribution must never be traceable to an employer through the data model — see §4.5.
3. **Right to erasure vs. content integrity.** Account deletion detaches and anonymises the contributor profile; published content transitions to `Withdrawn` rather than being erased where a legitimate interest in the moderation record exists. This tension needs the bundled legal review (PRD §11); the architecture keeps both options open by never hard-deleting rows. The personal content of experience reports is purged in both cases (C56).
4. **No third-party tracking.** No external analytics, fonts or CDNs. All assets self-hosted; the `UsageEvent` table (§5.7) covers the PRD metrics.

Additionally: EU hosting, encryption in transit, disk-level encryption at rest, documented retention periods, self-service export of the user's own workspace and contributions. Anonymous exports leave no personal data behind (§4.4); rate-limit partitions are held in memory only.

### 9.4 Accessibility as an architectural constraint

| Requirement | Architectural consequence |
|---|---|
| Errors programmatically associated with fields (WCAG 3.3.1) | Field-level `errors` map in Problem Details (§7.1) |
| No unannounced session expiry (WCAG 2.2.1) | Sliding cookie session + announced warning with "stay signed in" in the shell; after a 401, sign-in in an overlay without losing the page (§8.3) |
| SPA route changes announced | App-level `A11yAnnouncerService`, focus management on navigation |
| Exports themselves accessible | XLSX with real header rows, defined names, no colour-only meaning — identical for anonymous and saved exports |
| Language changes exposed | `lang` driven by i18n state; responses honour `Accept-Language` |
| Regressions caught | axe-core via Playwright in CI as a blocking gate |

Component-level work belongs to step 7; these items are here because they cannot be retrofitted at component level.

### 9.5 Logging and observability

Serilog stays. Two changes:

- The PostgreSQL sink writes to a dedicated `logging` schema with a **retention job** (e.g. 90 days). Unbounded growth of the log table in the primary database is the most likely operational failure for an under-maintained deployment.
  *As built (C90):* `Serilog.Sinks.Postgresql.Alternative` writes to `logging."SeriLogs"`, a table the migrations own (the sink never creates it). `LogRetentionJob` deletes rows older than `AppLoggingSettings:Retention:RetentionDays` (default 90) at startup and every `RunInterval` (default 1 day), with `now` from `TimeProvider`; it assumes a single API instance.
- Structured logging with a correlation ID per request; PII and contributor-profile correlation excluded (§9.3). Request bodies of the anonymous export (which contain free-text notes) are not logged.

Health endpoints `/health/live` and `/health/ready` (DB + file store) for the reverse proxy and uptime monitoring.

### 9.6 Caching

- Catalog reads: `IMemoryCache` keyed by `CatalogReleaseId`, invalidated when a release is published. Releases are immutable, so invalidation is trivial by construction.
- HTTP: `ETag` + `Cache-Control: public, max-age=3600, must-revalidate` on catalog endpoints and on the *public* directory endpoints; `no-store` on everything workspace-scoped, on the configuration-scoped directory variant and on export responses.
- No caching of write paths.

### 9.7 Configuration and secrets

Configuration through `appsettings.json` + environment variables (12-factor). Secrets never in the repository — this matters more than usual for a public GitHub repo. The existing `RebuildDatabase` / `SampleDataInitializer` switch must be hard-disabled outside `Development` (guard on `IHostEnvironment`, not only configuration), otherwise a misconfigured production deployment can wipe data.

---

## 10. Deployment and operations

### 10.1 Topology

```
Internet
   │  TLS (Let's Encrypt)
   ▼
Reverse proxy container ── static SPA bundle
   │
   ▼
API container (ASP.NET Core 10)
   ├── PostgreSQL 17 container (named volume)
   ├── File store volume
   └── outbound SMTP (TLS) → Lettermint (C71); Mailpit container in Development/E2E
```

Single host. `docker compose` in production is acceptable at this scale and keeps the handover story simple — a successor organisation can run this on one VM.

### 10.2 Environments

`Development` (local compose, seeded), `Staging` (pilot with cooperation partners, real data flows, separate database), `Production`.

The pilot (PRD §5, goal 4) needs a stable staging environment with realistic data — worth provisioning before the pilot rather than during it.

### 10.3 CI/CD (GitHub Actions)

Build + unit/integration tests (Testcontainers for PostgreSQL) → Angular build + unit tests → axe-core accessibility gate → NetArchTest boundary assertions → container image build → deploy to staging on `main`, manual promotion to production.

Because the repository is public and publicly funded: Dependabot, CodeQL, and an SPDX/REUSE licence header check belong in the pipeline from day one. Licence compliance is an obligation here, not hygiene.

### 10.4 Backup and recovery

Nightly `pg_dump` plus WAL archiving; file store snapshotted on the same schedule; **restore rehearsed at least once and documented in `TECHNICAL.md`**, including the ETag-epoch bump of §7.1. The `DataProtectionKeys` table is part of the database backup; a restore without it ends all sessions (C60). Targets, stated explicitly because they are modest by choice: RPO 24 h, RTO best-effort within one working day.

### 10.5 Migrations

EF Core migrations auto-applied on startup (existing behaviour) is acceptable for a single instance but unsafe with more than one. Document the constraint; when scaling out, move migrations to a separate deployment step.

---

## 11. Scale and reliability

### 11.1 Load estimation

| Dimension | Expected | Design headroom |
|---|---|---|
| Registered organisations | 10 – 200 | 10,000 |
| Registered users | 50 – 500 | 50,000 |
| Criteria per release | 300 – 1,500 | uncapped |
| Published entries | 50 – 2,000 | 1,000,000 |
| Peak requests | < 5 req/s | ~500 req/s on one container |
| Database size | < 5 GB | 500 GB on one volume |

The system is over-provisioned by two to three orders of magnitude on a single container. **Optimising for throughput would misallocate the project's scarcest resource, which is maintainer attention.** The anonymous export is the only unauthenticated endpoint that does non-trivial work; at one XLSX of at most ~1,500 rows per request it stays well within this headroom under its rate limit.

### 11.2 Known query costs

**The cost of D3.** Because fulfilment is a per-request projection, the directory cannot be indexed, sorted or filtered by "how well does this product meet my criteria". A query such as *"show products that satisfy my A-criteria"* — which PRD §7 describes as part of the core flow — requires computing the projection across all candidate entries.

Mitigation, in order of when it becomes necessary:

1. **Pre-filter on indexable attributes first** (entry app types and use cases, test type, channel, documentation presence, publication recency), then project fulfilment only over the surviving set. At 2,000 entries the surviving set is typically tens of rows.
2. **Push the projection into SQL** as a grouped query inside Directory rather than computing in memory: the configuration's included criterion keys are read through `IConfigurationReader` (C62) and passed as a parameter (an array of keys) to a grouped query over `PublishedAssessment`. A direct join with Configurator tables would cross the module boundary.
3. **Materialised view per (entry, release) pair** holding the per-category counts against the *entry's own* scope, used as a coarse pre-filter only. This is the point to watch: such a view must not acquire a total or ratio column, or D3's guarantee is quietly lost. If this step is ever taken, revisit ADR-003 explicitly rather than treating it as an optimisation detail.

### 11.3 What breaks first, and in what order

1. **Serilog PostgreSQL sink table growth** — mitigated by retention (§9.5). Most likely real incident.
2. **Uploaded file volume growth** — quota per contributor profile, monitoring, object storage if needed.
3. **Directory search** — trigram search degrades in the tens of thousands; add a GIN index, then PostgreSQL full-text search. No external search engine.
4. **Fulfilment projection** — see §11.2.
5. **In-memory cache** — only when running more than one instance.

### 11.4 Failover

Deliberately none. Single instance, accepted downtime, documented recovery. Redundancy would add operational surface that no one is funded to maintain after the project ends.

### 11.5 Scaling path, if ever needed

Stateless API + external session/cache → horizontal scaling behind the proxy → read replica → module extraction (Directory is the natural first candidate: read-heavy, public, depends only on Catalog).

---

## 12. Migration path from the existing codebase

Sequenced so that a working application exists at the end of every step; nothing is rewritten wholesale.

**Step 0 — Platform upgrade and test foundation (v7)**
.NET 8 → 10 with EF Core 10 and Npgsql 10 (C41) and Angular 20 → 22 (`ng update` per major version, C37), both before the first test project is created, so the test setup (TESTING §9 step 0) is built once on the target versions.

**Step 1 — Foundations (no user-visible change)**
Module namespaces and NetArchTest assertions; Serilog to its own schema with retention; guard `RebuildDatabase` on `IHostEnvironment`; `/health` endpoints; Problem Details middleware with the field-level `errors` map.

**Step 2 — Catalog versioning and export**
Add `CatalogRelease`, `CatalogReleaseCriterion`, `CriterionRevision` (with `Criterion.Kind`), `CriterionApplicability`, `PriorityRecommendation`, `StandardReference`, `CriterionProcedureText`, `AppType`, `ProductPart`, `UseCaseTemplate` (with `IsActive`), `TestType`, `FilterExclusion`, `CatalogReleaseFilterExclusion` and `CatalogReleaseAppTypeProcedure` (C45, C61). Map the repository's `CriteriaFilter` (types 1–4) and `CriteriaFilterExclusion` onto them; drop seed rows Id 2 and 3 (§16.3.2). Import the BIT inklusiv texts and create one procedure criterion per test type. Migrate existing criteria into release `2026.1`. *v11 (C74, confirmed SPEC SD20):* release content is prepared as a data folder in the repository (`catalog/releases/2026.1/`) and imported with `catalog import` / `catalog activate`; later releases use the same path. **Prerequisite:** priorities and rationales per criterion (Kriterienkatalog K-6, §16.5). Extend the criteria endpoint to return resolved priority with rationale, allowed ratings and procedure texts, and add the filter-options endpoint with exclusions. The existing filter UI keeps working against the extended contract.

*Added in v4:* the export pipeline with `IExportSource` and **both** sources lands here — `AdHocSelectionSource` behind `POST /api/v1/exports`, and `PersistedConfigurationSource` once configurations exist (step 3). `withConfigurator()` gets its in-memory draft. This makes the primary persona's complete flow (select → adjust → export) available before any account or tenancy work, which is also the earliest point at which a pilot partner can try the tool.

**Step 3 — Identity, tenancy, session and saved lists (changed in v10, C61)**
Add `Organization`, `OrganizationMembership`, `ContributorProfile`, `UserContributorLink`. Rename `Approver` → `Moderator`. Introduce resource-based authorisation and the EF global query filter. Replace the `localStorage` JWT by Identity cookie authentication with antiforgery, the anonymous session probe, the Data Protection key store and the basic interceptor (antiforgery header, 401 → login overlay). Saving a configuration is the first state-changing feature on the new session model, so nothing is built on the JWT that step 4 would have to replace. Add the Configurator entities (`CriteriaConfiguration`, `ConfigurationAppType`, `ConfigurationProductPart`, `ConfigurationTestType`, `ConfigurationCriterion`), `IConfigurationReader` and `PersistedConfigurationSource`, the copy operation (D6) and `ShareLink` with the anonymous read (D7). `CriteriaConfiguration.ProcurementProjectId` follows in step 5. *v11:* `IEmailSender` with Mailpit locally (C71), registration with e-mail confirmation, password reset, lockout (C72), organisation members and invitations (C80), account export and deletion (C79), preferences incl. appearance (C73).

Existing users are **not** auto-assigned a personal organisation. Before the pilot there is little or no real user data, and auto-creation would leave a tail of single-person organisations that later need merging into real team organisations. Existing accounts keep working with no membership; the first workspace or save action prompts the user to create or join an organisation. Any pre-pilot test data is reset rather than migrated.

**Step 4 — Session warning and hardening (D8; changed in v10, C61)**
Absolute limit with the stored sign-in time, sliding renewal on every request, expiry headers, `POST /auth/session/extend`, security-stamp invalidation on password change and account deletion, the expiry warning in the shell and the configurable lifetimes (C52, C60).

**Step 5 — Workspace module**
`ProcurementProject` (with its own list copy and the C47 lock, C53), `CriteriaConfiguration.ProcurementProjectId`, `CandidateProduct` (with tri-state `TestStatus`), `TestEvidence` (0..n), `CandidateAssessment`; "Merken" from the directory once step 6 exists. Retire `withProposal()` and the proposal endpoints. Repoint `withProducts()` / `withFavorites()` at the new model.

**Step 6 — Directory, publication and contribution**
`SoftwareProduct`, `PublishedEntry` and `PublishedEntryVersion` (with app types, use cases, assessments in full scope, test evidences), `IPublicationGateway`, `PublishedEntryFactory`, `IProductIdentityResolver`, **`PublicationService` (Flow B), `ContributionService` (Flow D) and `VendorPublicationService` (Flow E)**, `PublicationConsent`, `ContentPurge` and the version-immutability interceptor (C56, C66), redaction rules, cross-release matching, the two-signal projection endpoints in both variants, directory search with the four context filters (test-type filtering is P0), the directory-level withdrawal endpoint, the product-identity administration (C75, confirmed SPEC SD22), and the contract tests.

All three write paths land in the same step: building the factory around a subset of its paths and retrofitting the rest is how the v1 boundary error happened in the first place.

**Step 7 — Moderation and P1**
Content reports, counter-statements, moderation log (with optional version reference, C63), `ProductIdentityRequest` handling and ACR upload (Flow E). *(v6: test-type filtering moved to steps 2 and 6, since it is P0.)*

The order puts the two decisions with the widest blast radius — catalog versioning and tenancy — before any new feature code is written on top of them.

---

## 13. Testing hooks

Full strategy belongs to step 6 (`TESTING_v10.md`). The architecture assumes:

- Integration tests against real PostgreSQL via Testcontainers (global query filters and `xmin` concurrency cannot be tested on an in-memory provider).
- Contract tests for the invariants that carry product meaning:
  - no `score`/`rating`/`percentage`/`overallStatus` property is ever serialised; directory projections return exactly the eight counts;
  - a selection that violates a `FilterExclusion` rule is rejected (configurator, ad-hoc export, directory filter);
  - an assessment value outside the allowed-ratings union of the selected app types is rejected; `Unknown` is always accepted (C31);
  - procedure criteria have no assessment rows, their answer is derived from test evidences, and they are never counted (C30, C46);
  - a test-result version stores one assessment row per criterion in scope (C43);
  - republication creates a new version of the same entry; reports and counter-statements stay attached (C42);
  - assigning a list copies it into the project; the copy cannot be *replaced* once an assessment with a value other than `Unknown` exists, while it stays editable within its release with fixed selection layers (C47, C51, C53);
  - every experience report has a consent timestamp and text version (C48); withdrawing it purges its personal content and keeps a tombstone (C56);
  - version rows are never modified except by `ContentPurge` (C66);
  - a Flow E version without assessments has no assessment rows and `fulfillment: null` (C54);
  - only the publishing profile republishes; withdrawn entries are never republished (C55);
  - shared configurations are readable without an account; projects have no share links (C59);
  - anonymous `GET /auth/session` returns 200 with an antiforgery token; sessions end at the idle and absolute limits and survive an application restart (C60);
  - modules reference each other only through `Contracts` (C44);
  - experience reports carry no counts; workspace projections carry seven counts without `notCovered` (C33, C34);
  - the directory use-case filter never returns entries without a use case, and never experience reports (C32, C57);
  - `wasTested` is always present and boolean on every test-result entry (WorkspacePublication, VendorPublication); DirectContribution versions have `WasTested = null`, no `PublishedTestEvidence` row, and appear only in `experienceReports`;
  - internal fields (`InternalNotes`, `ContactNotes`, `TesterQualificationNote`) and consent records never appear in any published DTO;
  - cross-organisation reads return empty, and a foreign `configurationId` yields 404;
  - `notCovered` is never merged into `unknown` or `blockade`;
  - Flow D and Flow E succeed for a user with no organisation membership;
  - `Channel = VendorOrAuditor` exists only with `OriginKind = VendorPublication`; Flow B always yields `Independent`;
  - publication with `TestStatus = Unknown` is rejected;
  - the anonymous export persists nothing and produces the same file content as a saved configuration with the same selection.
- Accessibility tests (axe-core) as a blocking CI gate.
- Architecture tests (project references and NetArchTest) enforcing §4.3 (C62), including that only `PublishedEntryFactory` constructs a `PublishedEntry` or `PublishedEntryVersion`.

---

## 14. Proposed ADRs

| ID | Subject | Status |
|---|---|---|
| ADR-001 | Modular monolith over service decomposition, with `Contracts` namespaces as the only cross-module surface | Proposed |
| ADR-002 | Catalog versioning via immutable releases composed from shared revisions | **Accepted** (owner, 2026-09-29) |
| ADR-003 | Erfüllungsgrad as a request-time projection, never a stored score; all category counts returned (Option A; eight since v5) | Proposed; count contract **accepted** (owner, 2026-10-01) |
| ADR-004 | Publication as snapshot copy rather than a visibility flag (versions per entry since v7) | Proposed |
| ADR-005 | Product identity created on publish (Flows B and E), with admin merge and a request path for Flow D | **Accepted** (owner, 2026-09-29; Flow E extension 2026-10-01) |
| ADR-006 | ASP.NET Core Identity cookie session with antiforgery over `localStorage` JWT and over refresh-token plumbing. *Note (v10, C60):* after a `401` the shell re-authenticates in an in-app overlay and retries the request, so no page state is lost at either limit (WCAG 2.2.1); the absolute limit uses a stored sign-in time; Data Protection keys are persisted | **Accepted** (owner, 2026-10-13) |
| ADR-007 | Organisation-scoped tenancy enforced by EF global query filters | Proposed |
| ADR-008 | **Three write paths into Directory with a shared invariant factory; Kanal 1 only via Flow E** | **Accepted** (owner, 2026-10-01) |
| ADR-009 | **Unauthenticated, non-persisting export of an ad-hoc selection** | **Accepted** (owner, 2026-10-01) |
| ADR-010 | **App types and use cases as context of a published entry, never of the product identity** (variant A) | **Accepted** (owner, 2026-10-12) |
| ADR-011 | **Stable published entry with immutable versions** | Proposed (review R2) |
| ADR-012 | **Test evidences as the single source for "tested according to"** | **Accepted** (owner, 2026-10-12) |
| ADR-013 | **.NET 10 LTS and Angular 22 as the locked platform versions** | **Accepted** (owner, 2026-10-12) |

---

## 15. What I would revisit as the system grows

| Trigger | Revisit |
|---|---|
| More than one API instance | In-memory cache (D10), auto-migration on startup (§10.5), rate-limit partitions for the anonymous export |
| More than ~10,000 published entries | Directory search (§11.3) and the fulfilment projection (§11.2) |
| A materialised pre-filter view becomes necessary | ADR-003 explicitly — the guarantee is at stake, not just performance |
| Export p95 above ~3 s | Synchronous export (D9) |
| Abuse of the anonymous export | Tighter limits first; a proof-of-work or challenge step only as a last resort, since it would hurt assistive-technology users |
| Uploads opened beyond authenticated Kanal-1 submitters | D14 — reinstate malware scanning |
| Misuse of self-declared vendor/auditor profiles becomes visible | Verified vendor profiles (e.g. domain e-mail check or confirmation through a cooperation partner) — outcome of §16.2.7 decides |
| Vergabetext-Generator with AI assistance (P2) | New external dependency, latency budget, prompt/response logging under DSGVO, cost model — likely the first genuinely new architectural component |
| A second organisation self-hosting | Configuration surface, seed data, upgrade path, federation of published entries between instances |
| ISO 9241 integration (PRD §6, deferred) | `StandardReference` already accommodates it; `CriterionApplicability` may need a process-level vs. product-level dimension |
| Duplicate product identities becoming visible | Merge tooling, possibly a lightweight curation role — but not a return to a central maintained database |

---

## 16. Open questions and recorded decisions

### 16.1 (closed in v4)

The former decision on aggregate counts is recorded in §16.4.1.

### 16.2 Requiring external input

1. **Erasure vs. moderation record** (§9.3) — needs the bundled legal review from PRD §11 before retention rules are fixed.
2. **Attachment liability** — who is responsible if a vendor uploads an ACR containing third-party material? Same legal bundle.
3. ~~**Share links** — should a shared configuration be readable without an account?~~ **Decided (owner, 2026-10-13, D7):** yes for criteria lists of the organisation; no share links for projects (and for project copies) in v1. Implementation in §5.3 (C59).
4. **Custom use-case templates** (P2) — removed from the v1 model (G1, v7); `UseCaseTemplate` gains an owner when P2 is planned.
5. **Handover target** (PRD §11) — if the successor organisation has a fixed platform (e.g. a Bundes-IT environment), parts of §10 need revisiting. Worth asking early.
6. **`ProductIdentityRequest` turnaround** — a contributor blocked behind an admin step is a poor experience for exactly the persona the project most wants to include. If volume is low this is fine; if not, consider a trusted-contributor capability granted via cooperation partners. *v4 note:* vendors no longer depend on this path (Flow E).
7. **Authenticity of vendor and auditor claims (new in v4, decision E4).** Neither the v3 workspace route nor Flow E verifies that a user behind a vendor or auditor profile acts for that vendor or test lab, or that the stated consent of the commissioning party exists; `PublicationConsent` is a self-declaration. Specific risks: impersonation of a vendor; Kanal-1 entries about a competitor's product; one user holding a vendor profile and an individual profile and publishing "independent" praise under the latter (the profiles are deliberately unlinkable in public). Questions for the bundled legal review: what level of verification is required before Kanal-1 entries may be shown, who is liable for a false consent declaration, and how this interacts with the binding force of VPAT/ACR data in tenders (PRD §7, §11). Architecture hook: a `VerifiedAt` / `VerifiedBy` pair on `ContributorProfile` can be added without migration pain if verification becomes a requirement.
8. **Consent for experience reports (new in v4; widened in v7, C48; purge decided in v10, C56).** The narrative and the assistive-technology context of an experience report can reveal a disability, i.e. special-category data under Art. 9 GDPR. Decided: every report requires explicit consent with a stored consent-text version (`ExperienceReport.ConsentedAt`, `ConsentTextVersion`), and withdrawing consent withdraws the report and purges its personal content (D4). The legal review decides only the consent wording, the legal basis and the timing of the purge (default: immediately); none of these needs a schema change.

### 16.3 Raised in step 7

1. **Re-pinning a configuration to a newer catalog release.** The UX copy offers "Neue Liste mit derselben Auswahl" when a saved configuration is pinned to an older release. The architecture has no operation for this. Proposed shape: `POST /api/v1/configurations/{id}/copies { release: "active" }` creates a *new* configuration on the active release with the same four-layer selection (use case, app types, product parts, test types) — values that the new release excludes or deactivates are dropped and reported; user decisions (`IsIncluded`, overrides, notes) are carried over by `Criterion.Key` where the key still exists (1:1 via `SupersededByKey`), and reported as "not carried over" otherwise. The original configuration stays untouched (ADR-002). **Decided (owner, 2026-10-13, D6):** adopted as proposed (C58). A procurement project that should use the newer release is a new project with a copy of the new list (C53).

2. ~~Exclusion HR-Anwendung → Mobile Anwendung~~ **Decided (owner, 2026-10-06):** no exclusion; the seed rows are removed.
3. **Hardware and Vermittlungs-/Notrufdienst without a procedure (open to-do, accepted for now by the owner on 2026-10-06).** No BIT inklusiv test covers them. Options: map Software-Test criteria of chapters 5 and 6, or mark the app types as "no criteria yet". Until decided, selecting only these app types yields the "tested according to" items and no procedure texts. **Decides:** project team; tracked as Kriterienkatalog K-2.
4. **One product part per criterion.** The repository assigns each criterion to exactly one product part; 11.1.2.1 concerns audio and video. `CriterionApplicability` already allows several rows per revision; the seed should use that. The repository mapping of 11.8.x and chapter 12 looks swapped (Kriterienkatalog §7).

### 16.4 Decisions recorded in v4

The option analyses are kept so that the rationale, including the rejected alternatives, remains traceable.

#### 16.4.1 Aggregate counts in the API — decided: Option A

| Option | API returns | Consequence |
|---|---|---|
| **A — chosen** | Category counts per entry (six at the time; eight since v5) | A list view can show a compact breakdown. A client can still divide two numbers into a percentage. |
| B — strict | Per-criterion values only, no counts | Aggregation requires deliberate client-side work, making an accidental score much less likely. A scannable list or comparison view becomes substantially harder to build, and every client must implement counting itself. |

**Decision (owner, 2026-10-01):** A. B is a stronger reading of the PRD principle but pays for it in directory usability, which is itself a product goal. The residual risk is carried by the presentation rule (§8.4) and the domain component K1.

#### 16.4.2 "Vorschlagen" for criteria — decided: Option C

PRD §9 lists *"Merken, Teilen, Vorschlagen von Kriterien/Produkten/Filtereinstellungen"* as P0. The retired proposal workflow implemented editorial approval into a central product database (PRD §6 Nicht-Ziel); that justification covered products, not criteria.

| Option | What it means | Cost | Fit |
|---|---|---|---|
| A — Clarify the PRD | "Vorschlagen" means sharing a configuration; no catalog suggestions in v1 | PRD wording change | Covered by `ShareLink` |
| B — Lightweight suggestion channel | `CriterionSuggestion` in the `catalog` schema, triaged by a `CatalogEditor` into the next release | One entity, one endpoint, one editor view | Compatible with ADR-002 |
| **C — chosen: external channel** | A link from the UI to a contact address or form at WERTE.IT | Lowest; no data model | Compatible; suggestions are not traceable to the release that adopted them |

**Decision (owner, 2026-10-01):** C. The link appears in the criteria list and in the footer (UX copy S70). Option B remains reachable later without migration pain. The PRD acceptance criterion for "Vorschlagen" should be worded accordingly in `SPEC_v1.md`. Open: contact address or form URL (UX copy O4).

#### 16.4.3 Kanal 1 outside the workspace — decided: Option B (Flow E)

In v3 the only way to create a Kanal-1 entry was Flow B, which forced vendors and test labs to create an organisation and a procurement project for a procurement that does not exist.

| Option | What it means | Consequence |
|---|---|---|
| A — Keep | Vendors and auditors use a workspace | No code change; workspace semantics stretched; role-dependent labels throughout the workspace UI; heavier onboarding for the persona the PRD calls a reputation channel |
| **B — chosen: direct Kanal-1 path** | Flow E for vendor/auditor profiles; consent required; optional conformance report | Third factory path; no organisation or project needed |

**Decision (owner, 2026-10-01):** B, with identity creation allowed on Flow E through the shared resolver (D5), and with Flow B restricted to Kanal 2 (D16). Rationale: Kanal 1 depends on voluntary vendor participation, which PRD §13 already considers weakly supported for the German market; every extra hurdle worsens the bootstrapping problem. Building the path now costs little because migration step 6 has not started. The authenticity of vendor claims, which neither option verifies, is recorded in §16.2.7.

#### 16.4.4 Further decisions from step 7

| Topic | Decision (owner, 2026-10-01) | Where |
|---|---|---|
| Export without an account | Yes: unauthenticated, non-persisting ad-hoc export | §4.4, D15, ADR-009 |
| Workspace publication channel | Flow B always publishes as Kanal 2 | §4.4, D16 |
| Test status while unknown | Tri-state `TestStatus` in the workspace; publication requires a definite answer | §5.4 |
| Consent for assistive-technology context | UX placeholder accepted for now; legal review decides *(superseded: consent for every report, C48; purge on withdrawal, C56)* | §16.2.8 |

#### 16.4.5 Rating scale — decided: BIT inklusiv (v5)

**Decision (owner, 2026-10-06):** assessments use the six levels of BIT inklusiv (Erfüllt, Leichte Einschränkung, Einschränkung, Barriere, Blockade, Nicht anwendbar) plus `Unknown`. BIT inklusiv counts „Leichte Einschränkung“ as conformant; ProcureAccess does **not** derive a conformance verdict from this — that would be the aggregate the two-signal principle forbids. The count contract therefore has eight properties. Rejected: keeping the VPAT-style four values, because criteria and examples are taken from BIT inklusiv and their allowed ratings would no longer match.

#### 16.4.6 Filter layers and exclusions — decided (v5)

**Decision (owner, 2026-10-06):** four layers in the order use case → app type → product part → test type; only the use case is single choice; exclusions apply to all lower layers; changing a higher layer clears all lower selections with a notice asking the user to select again. Excluded values stay visible (`aria-disabled`) with a reason.

#### 16.4.7 "Geprüft nach …" as procedure criteria — decided (v6, refined in v7)

**Decision (owner, 2026-10-12):** a criterion kind of its own (C30). Listed and deselectable, default priority B, exported, shown Ja/Nein, not counted in K1. **v7 (C46, ADR-012):** the Ja/Nein is derived from the product's test evidences instead of being entered separately. Rejected: treating the item as a pure filter (would disappear from the export, which is where buyers need it) and rating it on the six-level scale (a test procedure is either applied or not).

#### 16.4.8 Allowed ratings with several procedures — decided (v6)

**Decision (owner, 2026-10-12):** union of the allowed ratings, `Unknown` always allowed (C31). Rejected: intersection (would hide correct levels for one of the app types) and per-app-type assessments (would multiply the assessment effort).

#### 16.4.9 Context of directory entries — decided: variant A (v6)

**Decision (owner, 2026-10-12):** app types and use cases are stored on the published entry with their source (C32, ADR-010). The directory keeps the use-case filter with the meaning "products with entries from this use context". Entries from projects with „Keiner davon passt“ carry no use case and appear only when the filter is empty. Vendor statements in Flow E are labelled „vom Anbieter angegeben“.

#### 16.4.10 Counts for experience reports and in the workspace — decided (v6)

**Decision (owner, 2026-10-12):** experience reports show no counts, only the optional list of assessed criteria (C33). K1 in the workspace has seven rows without „Damals nicht im Katalog“ (C34).

#### 16.4.11 Angular version — decided (v6)

**Decision (owner, 2026-10-12):** Angular 22 (C37). Resolves the support-window risk recorded in TESTING §12.1.

### 16.5 Consolidated open points (v6, updated in v10)

One list for all documents; the source documents refer here. The number O-10 was never assigned (gap since v7); the numbers stay stable. The open points of the UX copy are cited as UX-O1 … UX-O22 so that they are not confused with the O-numbers here (C70).

| # | Open point | Blocks | Decides | Source |
|---|---|---|---|---|
| O-1 | **Priorities and rationales per criterion, use case and app type; plain-language titles and summaries** (Kriterienkatalog K-6) | Migration step 2 (P0 weightings) | Project team | KAT K-6, PRD §11 |
| O-2 | Hardware and Vermittlungs-/Notrufdienst without a procedure (§16.3.3) | — (empty state exists) | Project team | KAT K-2 |
| ~~O-3~~ | ~~Re-pinning to a newer release (§16.3.1)~~ **Decided (owner, 2026-10-13, D6):** copy operation as proposed (C58) | – | – | – |
| O-4 | Legal review bundle: erasure vs. moderation record, attachment liability, authenticity of vendor claims, consent wording and legal basis for experience reports, timing of the content purge (§16.2.1, .2, .7, .8; C56) | Public launch, not development | Legal review | PRD §11 |
| ~~O-5~~ | ~~Share links readable without an account (§16.2.3)~~ **Decided (owner, 2026-10-13, D7):** yes for criteria lists; no project share links in v1 (C59) | – | – | – |
| O-6 | Contact address or form for criteria suggestions (§16.4.2) | Release | Project team | UX O4 |
| O-7 | Handover target platform (§16.2.5) | — | WERTE.IT | PRD §11 |
| ~~O-8~~ | ~~Final shape of `excludedBy[]` and `appTypesWithoutCriteria[]`~~ **Decided (SPEC_v2 §8.1/§8.2, C77)** | – | – | – |
| ~~O-9~~ | ~~Relationship between procedure-criterion answers and `TestEvidence`~~ **Decided (v7, C46):** answers derived from test evidences | – | – | – |
| ~~O-11~~ | ~~Session model~~ **Decided (owner, 2026-10-13):** ASP.NET Core Identity cookie authentication with antiforgery (C50, ADR-006) | – | – | – |
| ~~O-12~~ | ~~Session timeouts~~ **Decided (owner, 2026-10-13):** 30 minutes idle, 8 hours absolute; warning 5 minutes before (C52) | – | – | – |
| O-13 | Carrying assessments over to a list on a newer catalog release (§5.4, C51) — v1: new project | Later | Owner | C51 |
| O-14 | Priority terms „A – Mindestanforderung“, „B – Bewertungskriterium“, „Hinweis“ (UX copy assumption A2) | Export column headers, `SPEC_v1.md` | Project team (accessibility expertise) | UX-O2 |
| ~~O-15~~ | ~~Threshold for the staleness note~~ **Decided (owner, 2026-10-15, SPEC SD7):** 18 months (C82) | – | – | – |
| O-16 | Review of the rating-level explanations (`pa-rating-legend`); texts used as provisional (SPEC SD8) | Pilot | Project team, possibly with BIT inklusiv | UX-O19, KAT K-8, Handoff H13 |
| ~~O-17~~ | ~~Catalog releases from data files with CLI import, no editor UI (C74)~~ **Decided (owner, 2026-10-19, SPEC SD20):** confirmed, no editor UI for now (C87) | – | – | – |
| ~~O-18~~ | ~~Minimal admin pages for product-identity requests and merges (C75)~~ **Decided (owner, 2026-10-19, SPEC SD22):** confirmed; the owner holds `PlatformAdmin` (C87) | – | – | – |
| ~~O-19~~ | ~~Removing a candidate from a project (C76)~~ **Decided (owner, 2026-10-19, SPEC SD21):** in v1; a published entry stays (C87) | – | – | – |
| ~~O-20~~ | ~~Supported browsers (SPEC §7.4)~~ **Decided (owner, 2026-10-19, SPEC SD19):** as proposed (C87) | – | – | – |
| O-21 | Dark theme: dark mode in Figma and accessibility review (C73) | Release | Philipp, accessibility tester | SPEC O-S5 |

#### 16.4.12 Decisions on the architecture review — v7

| Topic | Decision (owner, 2026-10-12) | Where |
|---|---|---|
| Platform version (R1) | .NET 10 in migration step 0 | C41, ADR-013 |
| Recording tested procedures (R6) | Several test evidences per product; procedure criteria derived | C46, ADR-012 |
| Changing a project's criteria list (Y1) | Locked once assessments exist (v1) | C47 |
| Consent for experience reports (Y3) | Every report, with consent-text version | C48 |

The remaining findings (R2–R5, Y2, Y4–Y8, G1, G3, G4) needed no owner decision and are incorporated as C42–C45 and C49.

#### 16.4.13 Decisions of 2026-10-13 — v8

| Topic | Decision (owner) | Where |
|---|---|---|
| Session model (O-11) | ASP.NET Core Identity cookie authentication with antiforgery | C50, D8, ADR-006 |
| Session timeouts (O-12) | 30 minutes idle, 8 hours absolute, warning 5 minutes before | C52 |
| Locked criteria list | Replacing the list is locked once assessments exist; editing within the release stays possible | C51 |

#### 16.4.14 Decisions of 2026-10-13 — v10 (review of the main documents)

The numbers D1–D7 are those of `pruefung_hauptdokumente_v1.md`; they are unrelated to the design decisions D1–D16 in §6.

| # | Topic | Decision (owner, 2026-10-13) | Where |
|---|---|---|---|
| D1 | Several projects using one criteria list | Each project gets its own copy; the four selection layers are fixed after assignment | C53 |
| D2 | Vendor or auditor entry without assessments | No fulfilment breakdown (`fulfillment: null`), only a note; with assessments, product parts required | C54 |
| D3 | Who may update a workspace entry | Only the profile it was published under; Editors/Owners withdraw and publish a new entry | C55 |
| D4 | Consent withdrawal for experience reports | Withdraw and purge the personal content; a tombstone remains; timing set by the legal review (default: immediately) | C56 |
| D5 | Context of experience reports | App types optional and always offered; no use case | C57 |
| D6 | Re-pinning (O-3) | Adopted as proposed in §16.3.1 | C58 |
| D7 | Share links (O-5) | Criteria lists readable without an account; no project share links in v1 | C59 |

#### 16.4.15 Decisions of 2026-10-15 — v11 (feature specification)

The numbers SD1–SD18 are those of `SPEC_v1.md` §0.

| # | Topic | Decision (owner, 2026-10-15) | Where |
|---|---|---|---|
| SD7 | Staleness note | 18 months | C82, O-15 |
| SD10 | Response shapes | As fixed in SPEC_v2 §8.1/§8.2 | C77, O-8 |
| SD11 | Dark theme | In v1 from the start; release after design and accessibility review | C73, O-21 |
| SD13 | E-mail | Lettermint; e-mail confirmation in v1 | C71, C72 |
| SD14 | Personal data export | JSON | C79 |
| SD15 | Share links | Expiry required, default 90 days, at most 1 year | C81 |
| SD16, SD17 | Suggestion channel, legal pages | From configuration and repository files | C85 |

#### 16.4.16 Decisions of 2026-10-19 — v12 (confirmation of the SPEC proposals)

The numbers SD19–SD23 are those of `SPEC_v2.md` §0.

| # | Topic | Decision (owner, 2026-10-19) | Where |
|---|---|---|---|
| SD19 | Supported browsers | Two latest major versions of Chrome, Edge, Firefox, Safari (desktop); Safari on iOS and Chrome on Android, current and previous | C87, O-20 |
| SD20 | Catalog maintenance | Releases from data files with CLI import; no editor UI for now; `CatalogEditor` stays reserved | C74, C87, O-17 |
| SD21 | Removing candidates | In v1; a published entry stays, `SourceCandidateProductId` → null | C76, C87, O-19 |
| SD22 | Product-identity administration | Minimal admin pages; `PlatformAdmin` held by the owner | C75, C87, O-18 |
| SD23 | 🔧 defaults | All proposed values adopted, configurable | C88 |

#### 16.4.17 Decision of 2026-10-27 — v13 (licence rule for dependencies)

| # | Topic | Decision (owner, 2026-10-27) | Where |
|---|---|---|---|
| – | Licence rule | Shipped dependencies: OSI-approved, no commercial-use restriction, no strong copyleft or reciprocal licence; weak, file-level copyleft allowed if used unmodified; dual licences count by the option used. Development and test tools that are not shipped: OSI-approved, no commercial-use restriction. AutoMapper 15+ is excluded under this rule (issue #31) | D12, C89 |

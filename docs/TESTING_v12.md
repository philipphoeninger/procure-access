# ProcureAccess — Testing Strategy

> **Status:** v12 — output of Phase 2, step 6 ("Define testing strategy") of `ai_development_process.md`, updated during step 7, after the consistency audit, the architecture review of 2026-10-12, the session decisions of 2026-10-13, the review of the main documents of 2026-10-13 (`pruefung_hauptdokumente_v1.md`) and the feature specification of 2026-10-15 (`SPEC_v1.md` §14) to follow `ARCHITECTURE_v11.md`; v10 follows the owner's confirmation of the SPEC proposals of 2026-10-19 (`SPEC_v2.md` SD19–SD23, `ARCHITECTURE_v12.md`); v11 (2026-10-24) activates the step-0 invariants with the invariant gate of sprint 01, task S1-21 (`docs/sprints/S1-21_invariant_gate.md`); v12 (2026-10-25) activates OPS-01, OPS-02 and SEC-06 with the operations basics of sprint 01, task S1-22 (`docs/sprints/S1-22_operations_basics.md`). Supersedes v11.
> **Input documents:**
> - `ARCHITECTURE_v12.md` (authoritative for module boundaries, invariants and migration path)
> - `prd_procureaccess_werteit_v5.md` (product scope, P0/P1 requirements)
> - `ux_copy_procureaccess_v14.md` (domain components K1–K6, UI terminology used in test assertions)
> - `kriterienkatalog_bit_inklusiv_v1.md` (1.5; filter layers, exclusion rules, BIT inklusiv rating scale)
> - `accessibility_review_procureaccess_v2.md` (manual device checks, §7.2) and `figma_code_mapping_procureaccess_v1.md` (1.10; Figma link check, §8.2)
> - `pruefung_hauptdokumente_v1.md` (review of the main documents, German; findings A and B, owner decisions D1–D7)
> - `SPEC_v2.md` (feature specification; requirement IDs `R-…`, limits §7.2, rate limits §7.3, appearance §7.5, e-mail §7.6, contracts §8)
> - `CLAUDE.md` (state of the existing codebase)
>
> **Language note:** Written in English per project convention. Domain terms stay German where they are established project vocabulary (Kriterium, Workspace, Kanal 1/2, Erfüllungsgrad, Dokumentationsgrad). Where a test asserts visible UI text, the German string from the current UX copy (`ux_copy_procureaccess_v13.md`) is the reference.
>
> **Not covered here:** feature-level acceptance criteria (step 8, `SPEC_v2.md`), the test implementation itself (step 18), the operational restore rehearsal (`TECHNICAL.md`).

### Changes from v11

| # | Change | Driver |
|---|---|---|
| V61 | OPS-01, OPS-02 and SEC-06 set to `active` (§4.7, §4.10). They belong to step 1 but were built ahead of it as the sprint-01 stretch task S1-22; the other step-1 invariants (ARC-01–03, API-01/02, API-05, PRV-05, A11Y-03/05, A11Y-08) stay `planned` until their work starts | Sprint 01, S1-22 (roadmap M1.2) |
| V62 | §5.2: tests of startup behaviour and of an unreachable database host the API themselves, on a database or container of their own (SEC-06, OPS-01) | Sprint 01, S1-22 |

### Changes from v10

| # | Change | Driver |
|---|---|---|
| V58 | Step-0 invariants ARC-06, OPS-03, CAT-10 and A11Y-01 set to `active` (§4.2, §4.10, §4.11); the gate enforces them from now on | §9 step 0; sprint 01, S1-21 |
| V59 | The gate reads the catalogue from the **highest** `docs/TESTING_v<n>.md` (numeric comparison, so v10 wins over v9), not from `docs/TESTING.md`; it parses only tables with the header `ID \| Invariant \| Level \| Step \| Status` and fails if it finds no file or no rows (§3.4, §8.3) | Sprint 01, risk R4 |
| V60 | The Figma link check keeps `src/app/layout/header/header.ts` on a "pending" list until the step-1 shell: the file is the legacy header, not the mapped `pa-app-header`; the check prints a notice for it instead of failing (§8.2 stage 1) | Sprint 01, S1-21 (owner decision B1) |

### Changes from v9

| # | Change | Driver (ARCHITECTURE_v12 / SPEC_v2) |
|---|---|---|
| V55 | CAT-23 and WSP-04 no longer conditional: status `planned` for steps 2 and 5/6 (owner confirmed SPEC O-S1 and O-S3) | C87, SD20, SD21 |
| V56 | Browser matrix per the supported browsers: Playwright runs Chromium (Chrome, Edge), Firefox and **WebKit** (Safari approximation) in CI, plus a mobile viewport project (Pixel and iPhone profiles); real iOS Safari and Android Chrome remain part of the manual passes (§7.2) | SD19 |
| V57 | §10 rows for O-S1 and O-S3 moved to "resolved"; rate-limit and token-lifetime tests use the confirmed defaults from configuration, never hard-coded values | SD23 |

### Changes from v8

| # | Change | Driver (ARCHITECTURE_v11 / SPEC_v1) |
|---|---|---|
| V46 | New SEC-10 (e-mail confirmation gate: unconfirmed accounts get 403 `email-unconfirmed` on the gated actions, sign-in itself works) and SEC-11 (lockout after 5 failed sign-ins for 15 minutes; password policy; confirmation, reset and invitation tokens single use, expired and stored hashed; identical responses for known and unknown addresses) | C72 |
| V47 | New PRV-11: e-mails contain no tracking pixel and no rewritten link; recipient addresses never appear in the log sink at `Information` or above | C71 |
| V48 | New TEN-11 (last Owner cannot be demoted, removed or leave; account deletion refused for the sole Owner of an organisation with other members) and TEN-12 (share-link expiry required, at most 1 year) | C79, C80, C81 |
| V49 | New WSP-04 (removing a candidate keeps its published entry visible, `SourceCandidateProductId` null, withdrawal then only by the publishing profile) — conditional on the owner confirming SPEC O-S3 | C76 |
| V50 | New CAT-23 (catalog import from a release folder is atomic and validated; activation switches the active release) — conditional on SPEC O-S1 | C74 |
| V51 | New DIR-03 (staleness note iff the current version is older than the configured threshold, default 18 months) | C82 |
| V52 | New A11Y-08 (axe on every journey state in **both appearances**, light and dark; dark tokens meet the contrast rules) | C73 |
| V53 | PRV-07 restated for the JSON export of SPEC_v1 §8.4; Mailpit added to the E2E stack (sign-up, reset and invitation e-mails read through its API) | C71, C79 |
| V54 | §9 updated with the new invariants; §10 gains rows for the SPEC proposals O-S1 and O-S3; references to ARCHITECTURE_v11 and UX copy v13 | – |

### Changes from v7

| # | Change | Driver (ARCHITECTURE_v10) |
|---|---|---|
| V32 | Workspace invariants in their own section §4.5a; WSP-02/03 restated for the project copy, lazy assessment rows (an assessment exists when its value is not `Unknown`) and fixed selection layers; WSP-01 notes that a new product version updates the existing evidence | C53, C65 |
| V33 | New PUB-14 (Flow E without assessments: no rows, `fulfillment: null`; Flow B needs the project list), PUB-15 (republication only by the publishing profile), PUB-16 (experience reports: app types only, no use case); SIG-07, PUB-11, PUB-12 and DIR-01 adjusted | C54, C55, C57 |
| V34 | New PRV-10 (`ContentPurge` on consent withdrawal); PRV-09 and PUB-13 refer to it | C56 |
| V35 | New CAT-22 (re-pinning copy); §10 rows for re-pinning and share links resolved | C58, C59 |
| V36 | TEN-08 rewritten (organisation, hashed token, revocation, redaction, configurations only) and moved to step 3; TEN-09 is a planned invariant: anonymous read of a shared configuration returns 200 | C59 |
| V37 | SEC-07 rewritten (renewal on every authenticated request, case "request at minute 20, still valid at minute 49", expiry headers); new SEC-08 (password change and account deletion end other sessions), SEC-09 (anonymous session probe, antiforgery token issue and rotation, antiforgery failures as Problem Details), OPS-04 (session survives an application restart); SEC-03, A11Y-04 and J5 cover the login overlay and the retry | C60 |
| V38 | Clock control: `FakeTimeProvider` (`Microsoft.Extensions.TimeProvider.Testing`, MIT) in integration tests; test-only clock endpoint and Playwright `page.clock` in E2E. The shared test `HttpClient` attaches the antiforgery token from step 0 | C60 (review B-F17, B-F11) |
| V39 | §9 aligned with ARCHITECTURE §12: cookie authentication, antiforgery, saved lists and share links in step 3; session warning and hardening in step 4 | C61 |
| V40 | ARC-03 restated for module projects with separate `Contracts` projects (controllers and module registration exempt; Export reads configurations through `IConfigurationReader`) | C62 |
| V41 | PUB-13 enforced by a `SaveChangesInterceptor` (exception: `ContentPurge`); MOD-02 reworded; new MOD-06 (version reference on reports and moderation; withdrawn entries are not republished) | C55, C63, C66 |
| V42 | PUB-06 restated: unique `ClientRequestId` on the version, Flow B included, same id with a different payload → 409 | C64 |
| V43 | SIG-03 without CHECK constraint (factory rule); SIG-08: the notCovered row appears with every scope except Workspace; SIG-10 field error on `testStatus` | C66; review A-F2, A-F4 |
| V44 | DIR-01 uses `testKinds[]` | C67 |
| V45 | Smaller corrections: §5.3 K1 "eight in the directory, seven in the workspace"; §10 wording; reference "UX copy v12 §5.4" (was "v5"); references to the current documents | Review A |

### Changes from v6

| # | Change | Driver (ARCHITECTURE_v9) |
|---|---|---|
| V31 | SEC-07 states the timeouts (30 minutes idle, 8 hours absolute, warning 5 minutes before); A11Y-04 and J5 cover the absolute limit | C52 |

### Changes from v5

| # | Change | Driver (ARCHITECTURE_v8) |
|---|---|---|
| V28 | SEC-01–03 rewritten for ASP.NET Core Identity cookie authentication with antiforgery: no token in browser storage or JavaScript, antiforgery on all non-GET endpoints except the anonymous export, 401 without redirect loop; new SEC-07 (session endpoints) | C50 |
| V29 | WSP-02 restated: only *replacing* the list is locked; new WSP-03 (editing the assigned list keeps assessments, new criteria start as `Unknown`) | C51 |
| V30 | J5 uses short cookie lifetimes and the extend endpoint | C50 |

### Changes from v4

| # | Change | Driver (ARCHITECTURE_v7) |
|---|---|---|
| V22 | .NET 10 test stack (§3.1); upgrade before step 0 test projects | C41 |
| V23 | SIG-03, PUB-02, PUB-05 restated for stable entries with immutable versions and `TestStatus` on the candidate; new PUB-12 (one assessment row per criterion in scope), PUB-13 (moderation attaches to the entry, versions immutable); MOD-02 and PRV-02 reference `PublishedEntryVersion` | C42, C43 |
| V24 | CAT-16/17 rewritten: procedure-criterion answers are derived from test evidences; new CAT-21 (release-pinned procedure mapping) and WSP-01/02 (several test evidences, locked criteria list) | C45, C46, C47 |
| V25 | PRV-09 rewritten (consent for every report, text version); PRV-01/03 cover `SourceCandidateProductId` | C48, Y8 |
| V26 | ARC-03 asserts the `Contracts` rule; PUB-06 uses `clientRequestId`; DIR-01 matches test types via evidences and counts experience reports only for context filters | C44, C49 |
| V27 | J2 records two test evidences and checks the derived „Geprüft nach …“ | C46 |

### Changes from v3

| # | Change | Driver (ARCHITECTURE_v6) |
|---|---|---|
| V14 | PUB-02 restricted to the test-result paths: "definite test status" applies to Flows B and E; Flow D is covered by SIG-12 (audit E-F) | C28 |
| V15 | SIG-08 split by scope: eight categories in the directory, **seven** in the workspace (no „Damals nicht im Katalog“); new SIG-13 (workspace projections carry no `notCovered`) and SIG-14 (experience reports carry no counts) | C33, C34 |
| V16 | CAT-15 rewritten for the **allowed-ratings union**; CAT-16 rewritten for **procedure criteria**; new CAT-17 (procedure criteria never counted, only `ProcedureAnswer`), CAT-18 (inactive use cases), CAT-19 (exclusions pinned to the release), CAT-20 (empty selection for app types without criteria) | C30, C31, C36 |
| V17 | New PUB-11 (entry context and its source per flow) and DIR-01/02 (directory filters on entry context, no fulfilment sort) | C32 |
| V18 | §16.2.8 consent for assistive-technology context moved from conditional to planned: new PRV-09 | §4.5 |
| V19 | §7.2: manual device checks from the accessibility review v2 and mobile-first checks; §8.2 stage 1 runs the Figma link check | a11y review v2; C40 |
| V20 | Angular 22: §3.2 and §12.1 updated, Vitest no longer experimental | C37 |
| V21 | J1, J3 and J7 updated (four layers, consent, vendor context); EXP-07 names the multi-value request fields | C32, C35 |

### Changes from v2

| # | Change | Driver (ARCHITECTURE_v5) |
|---|---|---|
| V9 | SIG-07, SIG-08: **eight** count properties/categories (BIT inklusiv scale: `fulfilled`, `slightRestriction`, `restriction`, `barrier`, `blockade`, `notApplicable`, `unknown`, `notCovered`). CAT-04 now names `Blockade` | C25, §16.4.5 |
| V10 | New CAT-11–CAT-16: filter-layer validation, exclusion rules across all lower layers, always-selected product part, several app types in priority resolution, allowed ratings per criterion, procedure texts per test type | C23, C24, C26 |
| V11 | New FE invariants CFG-01–CFG-04 for the S10 configurator (reset of lower layers, excluded options stay visible with reason, single-choice use case with "none", optional test-type step) | C23, C24; UX copy v5 §5.4 |
| V13 | SIG-03 and SIG-11 restricted to test-result entries; new SIG-12: experience reports (`DirectContribution`) carry no test status in data, API or UI (E13). CAT-12 seed without HR → Mobile exclusion | C28, C29 |
| V12 | EXP-07 extended: the ad-hoc selection carries several app types and test types and is validated against the exclusion rules | C23, C24 |

### Changes from v1

| # | Change | Driver (ARCHITECTURE_v4) |
|---|---|---|
| V1 | SIG-07 is no longer conditional: Option A (six counts, no total) is decided | C15, §16.4.1 |
| V2 | PUB-08 rewritten and planned: Flow E (direct Kanal-1 publication). New PUB-09 (Flow B always Kanal 2) and PUB-10 (Kanal 1 only with origin `VendorPublication`). PUB-02, PUB-05, PUB-06 extended to three paths | C17, C18, D13, D16 |
| V3 | New EXP-05–EXP-07 for the anonymous ad-hoc export; API-05 gains the export and vendor-publication policies; J1 now runs without an account | C19, D15 |
| V4 | SIG-03 restated for the tri-state workspace test status; new SIG-10 (publication rejected while the test status is unknown) | C20 |
| V5 | New PRV-08 (consent records never serialised); PRV-04 extended to Flow E; new TEN-10 (`CanWithdrawEntry`) | C17, §7.2 |
| V6 | New journey J7 (vendor publication, Flow E); J2 reduced to Kanal 2 | C17, C18 |
| V7 | §10 (conditional sections) updated: §16.1, §16.3.1, §16.3.2 resolved; new conditional rows for §16.2.7, §16.2.8 and §16.3 (re-pinning) | §16 |
| V8 | Step 2 of §9 activates the export invariants for both sources; step 6 activates all three write paths at once | §12 |

### Decisions taken in the step-6 session (2026-10-01)

| # | Decision | Consequence in this document |
|---|---|---|
| T1 | Testing starts from zero; no existing backend test projects, no meaningful Angular specs | §9 adds a *step 0* before architecture migration step 1 |
| T2 | Frontend unit tests move from Karma/Jasmine to **Vitest** | §3.2, open question §12.1 |
| T3 | Manual accessibility testing is done by **a member of the project team who uses a screen reader and performs professional accessibility testing** | §7.2 |
| T4 | **Invariant-based gating** instead of percentage coverage thresholds | §4, §8.3 |

---

## 1. Purpose and principles

This document defines how ProcureAccess is tested: which test levels exist, which tools are used, what must be tested, and what blocks a merge. It is written for the current solo maintainer and for a future maintainer who inherits the codebase.

**Principles**

1. **Invariants first, percentages second.** The architecture carries a small number of rules whose violation would damage the product's legitimacy or users' safety: no aggregate accessibility score, no cross-organisation leak, no de-pseudonymisation, no publication of redacted fields, no Kanal-1 entry outside its single path. Each such rule is listed in the **invariant catalogue (§4)** and must have at least one tagged test. The completeness of that catalogue is the CI gate. Line coverage is measured and reported, never gated.
2. **Real PostgreSQL for anything that touches persistence.** Global query filters, `xmin` concurrency, `pg_trgm` and the schema itself cannot be tested on an in-memory provider (ARCHITECTURE §13). No EF Core InMemory provider, no SQLite substitute.
3. **Tests are handover documentation.** A test name states the rule it protects in domain language. A future maintainer should be able to read the invariant tests as an executable summary of ARCHITECTURE §4–§9.
4. **Maintenance cost is a design constraint.** Few E2E tests, no brittle snapshot tests of whole pages, no load tests (ARCHITECTURE §11 deliberately over-provisions). Every test must justify its upkeep against a solo maintainer's time.
5. **Accessibility is tested at every level** (ARCHITECTURE §3.2: quality attribute #1). Automated checks are necessary but cover only part of WCAG; manual screen-reader testing is a planned, scheduled activity, not an afterthought (§7).
6. **Licence hygiene applies to test dependencies.** The repository is public and publicly funded (ARCHITECTURE D12, §10.3). Test tooling must be under an OSI-approved licence with no commercial-use restriction.

---

## 2. Test levels

```
                 ┌───────────────────────┐
                 │  Manual a11y testing  │  scheduled, screen reader, §7.2
                 ├───────────────────────┤
                 │   E2E (Playwright)    │  7 journeys + axe on every state
               ┌─┴───────────────────────┴─┐
               │  API integration / contract│  WebApplicationFactory + Testcontainers
             ┌─┴────────────────────────────┴─┐
             │  Architecture tests (NetArchTest│  module rules, structural invariants
             │  + reflection)                  │
           ┌─┴─────────────────────────────────┴─┐
           │  Unit tests (.NET + Vitest)          │  domain logic, handlers, store slices,
           │                                      │  components
           └──────────────────────────────────────┘
```

| Level | Scope | Runs against | Expected share of tests | Speed budget (CI) |
|---|---|---|---|---|
| **Unit (.NET)** | Domain logic: priority resolution, cross-release matching, `PublishedEntryFactory`, `IProductIdentityResolver`, authorisation handlers, redaction mapping, export sources and formatters | Nothing external | largest | < 1 min |
| **Unit (Angular)** | Store slices (incl. the configurator draft), interceptors, services, components incl. a11y-relevant behaviour and the domain components K1–K6 | jsdom | large | < 1 min |
| **Architecture** | Module dependency rules (§4.3), structural invariants via reflection (e.g. "no published type has a property named `InternalNotes`") | Compiled assemblies | small, high value | < 30 s |
| **Integration / contract** | HTTP endpoints end-to-end through the real pipeline: auth, authorisation, EF filters, serialisation, Problem Details, rate limiting, caching headers | Real PostgreSQL 17 in a container | medium | < 5 min |
| **E2E** | Critical user journeys in a real browser, plus axe-core on every visited state | Full docker-compose stack | few (journeys, not features) | < 10 min |
| **Manual accessibility** | Screen-reader and keyboard usage of the journeys, exports, error and session flows | Staging | scheduled (§7.2) | n/a |

**Contract tests** are integration tests whose assertions target the serialised JSON shape rather than behaviour. They live in the integration project and are tagged as such (§8.1).

---

## 3. Tooling

### 3.1 Backend (.NET 10)

| Purpose | Tool | Licence | Notes |
|---|---|---|---|
| Test framework | **xUnit v3** | Apache-2.0 | Assembly fixtures for the shared PostgreSQL container |
| Assertions | **Shouldly** | BSD-3-Clause | FluentAssertions is excluded: v8 moved to a commercial licence for commercial use, which conflicts with the D12 rationale |
| Clock control | **`Microsoft.Extensions.TimeProvider.Testing`** (`FakeTimeProvider`) | MIT | The API reads time only through `TimeProvider` (ARCHITECTURE C60); session limits (SEC-07, SEC-08) and the log retention are tested by advancing the fake clock, never by waiting |
| Test doubles | **NSubstitute** | BSD-3-Clause | Moq is avoided because of the 2023 SponsorLink incident; mocks are used sparingly (prefer real objects and fakes) |
| HTTP integration | `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) | MIT | One factory per test assembly, configured for the `Testing` environment |
| Database | **Testcontainers for .NET** (`Testcontainers.PostgreSql`, image `postgres:17`) | MIT | Same major version as production; `pg_trgm` enabled through migrations |
| DB reset between tests | **Respawn** | Apache-2.0 | Faster than recreating the database; migrations run once per test run |
| Architecture rules | **NetArchTest.Rules** | MIT | Enforces ARCHITECTURE §4.3 |
| Log assertions | Serilog in-memory sink (`Serilog.Sinks.InMemory`) | Apache-2.0 | Used for privacy invariants on logs (PRV-04, EXP-05) |
| XLSX inspection | ClosedXML (already a production dependency) | MIT | Exports are re-opened and inspected, not byte-compared |
| Coverage (reporting only) | coverlet.collector + ReportGenerator | MIT / Apache-2.0 | Uploaded as CI artefact |
| Mutation testing (optional, periodic) | Stryker.NET | Apache-2.0 | Run manually on the domain core (priority resolution, matching, factory) at the end of a migration step; not in CI |

Test data is built with hand-written **builders** (§6), not with a fake-data library: deterministic values make failures readable and keep the dependency list short.

### 3.2 Frontend (Angular 22, zoneless)

| Purpose | Tool | Licence | Notes |
|---|---|---|---|
| Test runner | **Vitest** via the Angular CLI builder `@angular/build:unit-test` (`runner: "vitest"`), jsdom environment | MIT | Stable in current Angular versions; see §12.1 |
| Component queries | **Angular Testing Library** (`@testing-library/angular`) | MIT | Role- and label-based queries (`getByRole`, `getByLabelText`). A component that cannot be found by its accessible role and name has an accessibility defect, so the query style itself is a lightweight a11y check |
| Unit-level a11y checks | `vitest-axe` (axe-core matcher) on selected components | MIT (axe-core: MPL-2.0) | Supplementary only; jsdom cannot evaluate colour contrast or layout |
| Template linting | `@angular-eslint/template` accessibility rules | MIT | Static, cheap, runs in the lint stage |
| HTTP | `HttpTestingController` (`provideHttpClientTesting`) | MIT (Angular) | |
| Coverage (reporting only) | `@vitest/coverage-v8` | MIT | |

**Zoneless consequences for tests (ARCHITECTURE §8.2):**
- Every `TestBed` configuration includes `provideZonelessChangeDetection()`, so tests run under the same change-detection model as production.
- `fakeAsync`/`tick` depend on Zone.js and are **not used**. Asynchronous tests use `await fixture.whenStable()` and Vitest fake timers (`vi.useFakeTimers()`) where time must be controlled (session-expiry warning, debounced search).
- A component that only updates after a Zone-triggered change detection is a production bug; tests are expected to surface it.

### 3.3 End-to-end and accessibility automation

| Purpose | Tool | Licence | Notes |
|---|---|---|---|
| Browser automation | **Playwright** (`@playwright/test`) | Apache-2.0 | Chromium, Firefox and WebKit in CI, plus mobile viewport profiles (supported browsers, SPEC_v2 §7.4; V56) |
| Accessibility scan | `@axe-core/playwright` | MPL-2.0 | Tags `wcag2a`, `wcag2aa`, `wcag21a`, `wcag21aa` (EN 301 549 / BITV reference WCAG 2.1 AA); `wcag22aa` reported but non-blocking |
| Stack | docker-compose with an `E2E` override file | — | Seeded with the E2E dataset (§6.3), relaxed anonymous-export rate limit, and a `FakeTimeProvider` that a test-only endpoint `POST /e2e/clock/advance` moves forward (registered only in the `E2E` environment) |
| E-mail | **Mailpit** (MIT) container in the `E2E` override; tests read messages through its HTTP API (confirmation, reset, invitation links) | MIT | Development uses the same container; no real e-mail leaves a test run (ARCHITECTURE C71) |
| Browser clock | Playwright `page.clock` | Apache-2.0 | Moves the browser's timers together with the server clock, so session tests (J5) run without real waiting |

### 3.4 Repository layout

```
ProcureAccess/
├── ProcureAccess.sln                     # NEW — root solution incl. test projects (see §12.2)
├── tests/
│   ├── ProcureAccess.Tests.Unit/         # .NET unit tests, no I/O
│   ├── ProcureAccess.Tests.Architecture/ # NetArchTest + reflection-based structural invariants
│   ├── ProcureAccess.Tests.Integration/  # WebApplicationFactory + Testcontainers; contract tests
│   └── ProcureAccess.Tests.Shared/       # builders, fixtures, sentinel values, test auth handler
├── e2e/                                  # Playwright project (own package.json)
│   ├── journeys/                         # J1–J7 (§5.4)
│   └── support/                          # axe helper, login helper, E2E seed client
├── CLIENT/CLIENT.ProcureAccess/src/**/*.spec.ts   # co-located Angular unit tests
├── scripts/check-invariants.mjs          # invariant-catalogue gate (§8.3)
├── scripts/check-figma-links.mjs         # Figma link check (§8.2 stage 1)
└── docs/
    ├── TESTING_v<n>.md                   # this document (the gate reads the highest n)
    └── a11y-reports/                     # manual test protocols (§7.2)
```

---

## 4. Invariant catalogue

This table is the **merge gate** (T4). Every row with status `active` must be referenced by at least one test via the tag `INV:<ID>` in the test name or trait (§8.1). `scripts/check-invariants.mjs` parses this table and fails CI if an active invariant has no tagged test.

**Status values**
- `planned` — becomes `active` when the listed migration step starts (§9). Flipping the status is part of starting the step.
- `active` — enforced by the gate.
- `conditional` — depends on an open decision (§10); activated or rewritten once decided.

**Level abbreviations:** U = unit, A = architecture, I = integration, C = contract (integration), FE = Angular unit, E = E2E, M = manual.

**Origin kinds** referenced below: `WorkspacePublication` (Flow B), `DirectContribution` (Flow D), `VendorPublication` (Flow E).

### 4.1 Two-signal principle and test status (SIG) — ARCHITECTURE §4.4, §5.4, §7.3, ADR-003; PRD §7, §9

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| SIG-01 | No column, view, or index in the `directory`, `workspace` or `configurator` schemas holds an aggregate accessibility score. Checked against `information_schema` for names matching `/score\|rating\|percent\|overall\|average/i` | I | 6 | planned |
| SIG-02 | No property matching `/score\|rating\|percent\|overall\|average/i` is serialised by any directory endpoint, in both the public and the configuration-scoped variant | C | 6 | planned |
| SIG-03 | Published side: `wasTested` is present, non-null and boolean on every item of `entries` in every directory response, for both channels and both test-result origin kinds (`WorkspacePublication`, `VendorPublication`); the factory rejects a test-result version without a definite `WasTested`, and no stored version of a test-result entry has `WasTested = null` (integration query). This is a factory rule, not a CHECK constraint: `OriginKind` is on the entry and `WasTested` on the version (ARCHITECTURE C66). Workspace side: `CandidateProduct.TestStatus` is `NOT NULL` with default `Unknown` | C, I | 5/6 | planned |
| SIG-04 | `documentation` and `fulfillment` are separate JSON branches; no field of one appears in the other | C | 6 | planned |
| SIG-05 | Erfüllungsgrad is never persisted: no entity or column stores a fulfilment result (reflection over all `directory` entities) | A | 6 | planned |
| SIG-06 | Experience reports are returned as an ordered list of individual contributions, each with its own assistive-technology context; no aggregate field exists on product or entry | C | 6 | planned |
| SIG-07 | Exactly the eight count properties are returned per entry whenever `fulfillment` is not null (`fulfilled`, `slightRestriction`, `restriction`, `barrier`, `blockade`, `notApplicable`, `unknown`, `notCovered`) and no total, `relevantCriteria` or `conformant` property (Option A, ARCHITECTURE §16.4.1, §16.4.5) | C | 6 | planned |
| SIG-08 | Frontend: the fulfilment component (`pa-fulfillment-breakdown`, UX K1) always renders all categories of its scope together in fixed order (Erfüllt, Leichte Einschränkung, Einschränkung, Barriere, Blockade, Nicht anwendbar, Nicht eingeschätzt, then — with every scope except `Workspace`, i.e. Public and Configuration — Damals nicht im Katalog): eight rows in the directory, **seven** in the workspace (exception since v4, ARCHITECTURE C34), also when a value is 0, renders no conformant/non-conformant grouping, labels `notCovered` as "Damals nicht im Katalog" with its explanation, and renders no `%` character, star, bar or traffic-light element (ARCHITECTURE §8.4) | FE | 6 | planned |
| SIG-09 | Frontend: Dokumentationsgrad (`pa-documentation-signal`, UX K2) and Erfüllungsgrad (K1) are rendered in separate landmark/region containers with distinct accessible names | FE, E | 6 | planned |
| SIG-10 | Publication via Flow B with `TestStatus = Unknown` is rejected with Problem Details carrying a field error on `testStatus`; `Tested`/`NotTested` map to `true`/`false`. No published row ever results from an `Unknown` status | U, I | 6 | planned |
| SIG-11 | Frontend: `pa-tested-status` (UX K4) always renders "Auf Barrierefreiheit geprüft" with Ja or Nein, never hidden or collapsed, for every test-result entry in both channels | FE | 6 | planned |
| SIG-13 | Workspace projections (`GET /api/v1/projects/{id}` and candidate views) return exactly seven count properties and never `notCovered`; candidate assessments and the configuration are pinned to the same release (ARCHITECTURE C34) | C, U | 5 | planned |
| SIG-14 | Experience reports carry no counts: no item of `experienceReports` has a `fulfillment` branch; assessed criteria are returned as a plain list (possibly empty); the report card renders no K1 (ARCHITECTURE C33) | C, FE | 6 | planned |
| SIG-12 | Experience reports carry no test status: the factory creates no `PublishedTestEvidence` for `DirectContribution`; the read API returns them only in `experienceReports`, without `wasTested` or a documentation branch; the report card renders `contribute.report.typeNote` and never K4 (E13) | U, C, FE | 6 | planned |

### 4.2 Cross-release matching and catalog versioning (CAT) — ARCHITECTURE §5.1, ADR-002

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| CAT-01 | Priority resolution is most-specific-wins across the four levels of §5.1; one test row per level plus fall-through combinations (table-driven) | U | 2 | planned |
| CAT-02 | Every resolved priority carries non-null `rationale`, `source` and `matchedRule`. A priority without explanation fails the test (PRD §7 "Transparenz") | U, C | 2 | planned |
| CAT-03 | Cross-release matching follows the four rows of the §5.1 table: key in both → compared; key added later → `NotCoveredByThisRelease`; key removed → excluded; non-1:1 `SupersededByKey` → `NotCoveredByThisRelease` | U | 6 | planned |
| CAT-04 | `NotCoveredByThisRelease` is never folded into `Unknown` or `Blockade` — neither in the projection nor in the serialised counts | U, C | 6 | planned |
| CAT-05 | A published `CatalogRelease` and the revisions it references cannot be modified; attempts fail with a domain error, not silently | U, I | 2 | planned |
| CAT-06 | A configuration pinned to release *n* resolves to exactly the revisions of release *n* after release *n+1* is published (reproducibility of tender documents) | I | 2 | planned |
| CAT-07 | Publishing a release that changes *k* criteria creates exactly *k* new revisions; unchanged revisions are shared via `CatalogReleaseCriterion` | I | 2 | planned |
| CAT-08 | `ConfigurationCriterion` keeps `ResolvedLevel` unchanged when a user override is set (audit trail "recommended vs. decided") | U, I | 2 | planned |
| CAT-09 | Catalog cache is keyed by `CatalogReleaseId`; publishing a release makes the new release visible on the next request | I | 2 | planned |
| CAT-10 | **Characterisation:** the existing criteria filter returns the same criteria for a fixed seed before and after migration step 2 (subset comparison on stable fields) | I, E | 0→2 | active |
| CAT-11 | Filter layers: a selection has at most one use case (or none), at least one app type, all product parts with `IsAlwaysSelected`, and zero or more test types; anything else yields 400 with field errors | U, C | 2 | planned |
| CAT-12 | Exclusion rules (`FilterExclusion`) apply to every lower layer: for each seeded rule, a selection containing source and target is rejected by the configurator, the ad-hoc export and the directory filter; the catalog endpoint returns excluded values with their reason(s). Table-driven over the seed (HR-Anwendung, Website/Webanwendung) | U, C | 2 | planned |
| CAT-13 | A `FilterExclusion` whose target layer is not lower than its source layer cannot be created (catalog editor validation) | U | 2 | planned |
| CAT-14 | Priority resolution with several app types: per-app-type evaluation, strictest result wins (A > B > Info, then weight); `matchedRule` names the deciding app type. With no use case only rules 2 and 4 apply | U | 2 | planned |
| CAT-15 | **Allowed-ratings union:** an assessment (`CandidateAssessment`, `PublishedAssessment`) is validated against the union of `AllowedRatings` of the procedure texts of the selected app types (workspace: the project's configuration; Flow E: the stated app types; Flow D: the stated app types or all six levels). A value outside the union is rejected; `Unknown` is always allowed; without any procedure text all six levels are allowed. Table-driven, incl. Website + Mobile Anwendung with differing allowed ratings | U, I | 5/6 | planned |
| CAT-16 | Each selected test type adds exactly its one procedure criterion (`Kind = TestProcedure`, default level B) to the selection; it can be deselected and appears in both export sources. Criterion detail returns the `CriterionProcedureText` of every procedure that matches the selected app types (Software-Test for Website and Software, App-Test for mobile, PDF-Test for Dokument), each with a source URL | U, C | 2 | planned |
| CAT-17 | Procedure criteria have no assessment rows (workspace or published); their answer is derived: Ja if a test evidence with that `TestTypeId` exists, Nein if `TestStatus = NotTested`, otherwise Nicht angegeben. Writing an assessment for a procedure criterion is rejected; no fulfilment projection counts a procedure criterion | U, I, C | 2/5/6 | planned |
| CAT-21 | The app-type → procedure mapping is release-pinned (`CatalogReleaseAppTypeProcedure`): texts and allowed ratings of a configuration on release N are unchanged after release N+1 changes the mapping; one app type may map to several procedures | I | 2 | planned |
| CAT-18 | An inactive `UseCaseTemplate` is not offered by the catalog endpoint and is rejected for new selections, but still resolves for existing configurations and published entries | U, I | 2 | planned |
| CAT-19 | Exclusions are release-pinned: a selection validated against release N uses the `FilterExclusion` rows joined to N, also after release N+1 changes the rules | I | 2 | planned |
| CAT-20 | A selection whose app types have no requirement criteria (Hardware, Vermittlungs-/Notrufdienst) returns an empty requirement list plus `appTypesWithoutCriteria[]` — not an error; the UI shows `criteria.list.noCriteriaFor` | C, FE | 2 | planned |
| CAT-23 | `catalog import <version>` validates the whole release folder (references, exclusion layers, procedure mapping, allowed ratings) and imports atomically: any error leaves the database unchanged; an imported release is immutable (CAT-05); `catalog activate` switches the active release, and the next catalog request returns it (CAT-09) (ARCHITECTURE C74, confirmed C87) | I | 2 | planned |
| CAT-22 | Re-pinning (`POST /api/v1/configurations/{id}/copies { release: "active" }`) creates a new organisation list on the active release with the same four-layer selection; values the new release excludes or deactivates are dropped and reported; `IsIncluded`, overrides and notes are carried over by `Criterion.Key` (incl. 1:1 `SupersededByKey`), all others are reported as not carried over; the source configuration is unchanged (ARCHITECTURE C58) | U, I | 3 | planned |

### 4.2a Configurator UI (CFG) — ARCHITECTURE §5.1 (filter layers); UX copy v12 §5.4

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| CFG-01 | Changing a higher layer clears every selection in all lower layers except `Basisfunktionalität`, and shows the reset notice (`criteria.select.reset.*`) in a `role="status"` region naming the cleared layers | FE | 2 | planned |
| CFG-02 | Excluded options stay in the DOM, are focusable, carry `aria-disabled="true"` (not `disabled`) and reference their reason via `aria-describedby`; activating them changes nothing | FE, E | 2 | planned |
| CFG-03 | Step 1 is a single-choice combobox; „Keiner davon passt“ is always the last option, also while filtering; `Basisfunktionalität` is checked and cannot be unchecked | FE | 2 | planned |
| CFG-04 | Step 4 (test types) can be left empty; the submit button is enabled without a selection there | FE | 2 | planned |

### 4.3 Privacy, redaction and pseudonymity (PRV) — ARCHITECTURE §4.4–§4.6, §5.2, §5.5, §9.3

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| PRV-01 | No published entity or DTO type has a property named `InternalNotes`, `ContactNotes`, `TesterQualificationNote` or any commercial-data field (reflection over `directory` types and all DTOs reachable from directory endpoints) | A | 6 | planned |
| PRV-02 | End-to-end redaction: a candidate filled with **sentinel strings** in every private field is published; the sentinel appears nowhere in any directory response, in `PublishedEntryVersion` rows or their children, or in the log sink | I | 6 | planned |
| PRV-03 | `PublishedEntry` has no `OrganizationId` property or column; no directory response contains an organisation identifier or `SourceCandidateProductId`; the FK is `ON DELETE SET NULL` | A, C | 6 | planned |
| PRV-04 | `UserContributorLink` is never mapped to a DTO; no endpoint returns both a user identifier and a contributor profile; no log event at `Information` or above contains both a user ID and a contributor-profile or published-entry ID — checked for Flows B, D and E | A, I | 3/6 | planned |
| PRV-05 | `UsageEvent` has no user, organisation, IP or session field, and stores a date, not a timestamp | A | 1 | planned |
| PRV-06 | **Conditional on ARCHITECTURE §16.2.1** — account deletion detaches and anonymises the contributor profile; published content transitions to `Withdrawn` instead of being hard-deleted | I | 7 | conditional |
| PRV-07 | Self-service export (`GET /api/v1/account/export`, JSON per SPEC_v1 §8.4) returns only the user's own account data, contributor profiles, contributions and the lists and projects the user created; never another user's identity, never `UserContributorLink` rows of other users; purged content appears as null | I | 3/5/6 | planned |
| PRV-09 | Flow D: every experience report requires consent; without it 422 (field error). `ConsentedAt` and `ConsentTextVersion` are stored and never serialised in a read DTO; withdrawing the report is the withdrawal of consent and runs `ContentPurge` (PRV-10) | U, I, C | 6 | planned |
| PRV-10 | `ContentPurge` when an experience report is withdrawn: on all its versions `NarrativeDe`, `AssistiveTechnologyContext`, `ProductVersionUsed` and the `EvidenceNote` of its assessments are null, and their sentinel strings appear in no table, response or log event afterwards; a tombstone remains (entry and version ids, dates, state `Withdrawn`, consent timestamp and text version, moderation record); the purge is the only operation allowed to modify version rows (ARCHITECTURE C56) | U, I | 6 | planned |
| PRV-08 | `PublicationConsent` never appears in any directory read DTO (reflection) and in no directory response (sentinel strings in consent fields, Flow E) | A, C | 6 | planned |
| PRV-11 | E-mails (all message types, captured by Mailpit in E2E and by a fake `IEmailSender` in integration tests) contain no tracking pixel, no externally rewritten link and no data beyond the message type's purpose; recipient addresses never appear in the log sink at `Information` or above (ARCHITECTURE C71) | I, E | 3 | planned |

### 4.4 Publication and contribution paths (PUB) — ARCHITECTURE §4.3–§4.6, D4, D5, D13, D16, ADR-004, ADR-008

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| PUB-01 | Only `PublishedEntryFactory` constructs `PublishedEntry` (non-public constructor + NetArchTest rule) | A | 6 | planned |
| PUB-02 | The factory enforces the shared invariants for **every** write path: channel set, contributor profile present, `CatalogReleaseId` pinned; for the **test-result paths** (Flows B and E) additionally a definite `WasTested` and, for Flow E with `WasTested = true`, ≥ 1 test evidence; `PublicationConsent` required exactly for `VendorPublication`. Tests are parameterised over all three origin kinds; for `DirectContribution` the absence of test evidence is asserted instead (SIG-12) | U | 6 | planned |
| PUB-03 | Flow D: channel is forced to `Independent` regardless of request payload; the path cannot create a `SoftwareProduct` (unknown product → error pointing to `ProductIdentityRequest`) | U, I | 6 | planned |
| PUB-04 | Flow D and Flow E succeed for an authenticated user **with no organisation membership**; `CanContribute` and `CanPublishAsVendor` do not read memberships | I, U | 6 | planned |
| PUB-05 | Publication is a snapshot: editing a candidate after publication changes nothing public; withdrawing an entry leaves the candidate untouched; republishing creates a new immutable `PublishedEntryVersion` of the **same** `PublishedEntry` and moves `CurrentVersionId`. Flow E republication requires a new `PublicationConsent` on the new version | I | 6 | planned |
| PUB-12 | Full scope: a test-result version stores exactly one `PublishedAssessment` per requirement criterion in its scope (Flow B: included criteria of the project's list; Flow E **with assessments**: applicable to stated app types + product parts — Flow E without assessments see PUB-14), `Unknown` where not assessed; projection rule: key in the entry's release but outside its scope → `unknown`, key not in the release → `notCovered` | U, I | 6 | planned |
| PUB-13 | Reports, counter-statements and moderation actions reference `PublishedEntry` (and optionally the version shown, MOD-06); after a republication they are still attached. Version rows and their children are never updated or deleted: an EF Core `SaveChangesInterceptor` rejects `Modified` and `Deleted` entries of these types, except inside a `ContentPurge` scope (PRV-10). Unit test of the interceptor plus one integration attempt per type (ARCHITECTURE C66) | U, I | 6/7 | planned |
| PUB-06 | `clientRequestId` on all three publication paths (Flow B included): `PublishedEntryVersion.ClientRequestId` has a unique index on that column alone; replaying the same id with the same payload returns the original result and creates no second version; the same id with a different payload (different `RequestHash`) returns 409 (ARCHITECTURE C64) | I | 6 | planned |
| PUB-07 | Identity resolution (shared by Flows B and E): a near-duplicate name is offered as a trigram match; both flows return the same candidates for the same input; reads of a product merged via `MergedIntoId` resolve to the target | U, I | 6 | planned |
| PUB-08 | Flow E: channel is forced to `VendorOrAuditor`; a `PublicationConsent` with `givenBy`, `onBehalfOf` and `consentedAt` is required (missing → field errors); profiles of kind `Individual` or `Institution` are rejected (403); the path may create a `SoftwareProduct` through the shared resolver | U, I | 6 | planned |
| PUB-09 | Flow B always yields `Channel = Independent`: a `channel` property in the request is rejected or ignored (decided in implementation, tested either way), and no `PublicationConsent` is created | U, I | 6 | planned |
| PUB-11 | Entry context (ARCHITECTURE C32): Flow B copies app types and the use case from the project's configuration (`ContextSource = ProjectSelection`; none without configuration or with „Keiner davon passt“); Flow E requires ≥ 1 app type and accepts 0..n use cases (`VendorDeclared`); Flow D accepts optional app types only (`ContributorDeclared`, PUB-16). `ContextSource` is set iff a context exists; `SoftwareProduct` has no app-type or use-case column (reflection) | U, I, A | 6 | planned |
| PUB-10 | The factory rejects `Channel = VendorOrAuditor` with any origin kind other than `VendorPublication`, and `VendorPublication` with any channel other than `VendorOrAuditor` | U | 6 | planned |
| PUB-14 | Flow E without assessments writes no `PublishedAssessment` rows; both directory variants return `fulfillment: null` for that entry, and the entry card and entry row render `signals.fulfillment.none` instead of K1. Flow E with assessments but without `productPartIds` → 422 with field error. Flow B for a project without criteria list → 422 (ARCHITECTURE C54) | U, I, C, FE | 6 | planned |
| PUB-15 | Republication of a workspace entry only by the contributor profile it was published under: same profile → new version of the same entry; another profile while the entry is visible → 409; an Editor or Owner of the source organisation can withdraw it, and the next publication creates a **new** entry. Flow E republication (`PUT`) by another profile is rejected (ARCHITECTURE C55) | I | 6 | planned |
| PUB-16 | Experience reports (Flow D): app types are optional (`ContributorDeclared`); a request with use cases is rejected (400); no `PublishedEntryUseCase` row exists for `DirectContribution`; allowed ratings come from the stated app types, all six levels without a statement (CAT-15) (ARCHITECTURE C57) | U, I | 6 | planned |

### 4.5 Tenancy and authorisation (TEN) — ARCHITECTURE §5.4, §7.2, §9.1, D6, D7, ADR-007

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| TEN-01 | Cross-organisation isolation: for **every** entity carrying `OrganizationId` (discovered by reflection, so new entities are covered automatically), data of organisation A is invisible to a member of organisation B | I | 3 | planned |
| TEN-02 | `IgnoreQueryFilters()` appears only at allow-listed call sites (source scan against an explicit allow-list in the test) | A | 3 | planned |
| TEN-03 | Configuration-scoped directory endpoint: unauthenticated → 401; foreign `configurationId` → **404, not 403**; response carries `Cache-Control: no-store` | C | 6 | planned |
| TEN-04 | Public directory and catalog endpoints carry `Cache-Control: public, max-age=3600, must-revalidate` and an `ETag`; workspace endpoints and export responses carry `no-store` | C | 2/6 | planned |
| TEN-05 | Authorisation matrix for workspace endpoints: Owner / Editor / Reader / member of another org / user without org / anonymous × each endpoint, with expected status codes. The organisation is resolved from the resource; a client-supplied organisation header has no effect | I | 3/5 | planned |
| TEN-06 | Platform roles: `CatalogEditor`, `Moderator`, `PlatformAdmin` endpoints reject all other roles; `Approver` / `objects:approve` no longer exist after the rename | I | 3 | planned |
| TEN-07 | Counter-statements may only be posted by contributor profiles of kind vendor or auditor | I | 7 | planned |
| TEN-08 | Share links: `ShareLink` carries `OrganizationId` and is covered by TEN-01 for management; only a hash of the token is stored (no plain token in the table or the log sink); a link grants read-only access to exactly one configuration (`ResourceType = Configuration`; no endpoint creates a project share link; a project copy → 409); the read DTO contains no organisation, user or project data (sentinels); revoked or expired tokens return 404 (ARCHITECTURE C59) | I, C | 3 | planned |
| TEN-09 | Anonymous `GET /api/v1/shared/{token}` for a valid configuration link returns 200 with the redacted list, without any cookie; the S16 page renders it without sign-in (ARCHITECTURE D7, C59) | C, E | 3 | planned |
| TEN-11 | The last Owner of an organisation cannot be demoted, removed or leave (409); account deletion by the sole Owner of an organisation with other members is refused (409); an organisation whose last member deletes the account is deleted with its lists and projects (ARCHITECTURE C79, C80) | I | 3 | planned |
| TEN-12 | Share links require an expiry date; a date more than 1 year after creation, in the past or missing yields 400 with a field error; the default offered by the client is 90 days (ARCHITECTURE C81) | I, FE | 3 | planned |
| TEN-10 | `CanWithdrawEntry`: the user linked to the entry's contributor profile may withdraw (all origin kinds); for `WorkspacePublication` an Editor or Owner of the source candidate's organisation may also withdraw; everyone else gets 404 | I | 6 | planned |

### 4.5a Workspace (WSP) — ARCHITECTURE §5.4, §7.2, C46, C47, C51, C53, C65

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| WSP-01 | Several test evidences: a candidate with `TestStatus = Tested` can hold 0..n `TestEvidence` rows, at most one per `TestTypeId`; evidences are rejected while `TestStatus ≠ Tested`; setting `NotTested` or `Unknown` asks to remove existing evidences (409 with field error); a new product version updates the existing row of that procedure (`TestedVersion`, date) instead of adding one | U, I | 5 | planned |
| WSP-02 | Project copy and lock: assigning a saved list (`PUT /api/v1/projects/{id}/criteria-configuration`) creates a copy with `ProcurementProjectId` set; the source list is unchanged; the copy is not accepted as source for another project and cannot get a share link. Assessment rows are created lazily (no row = `Unknown`). Replacing the project's list returns 409 once any candidate has an assessment with a value other than `Unknown`; before that it succeeds, also when rows with `Unknown` exist (ARCHITECTURE C47, C53) | I | 5 | planned |
| WSP-03 | Editing the project copy after assessments exist: including, excluding, re-prioritising and annotating criteria succeeds; changing one of its four selection layers is rejected; newly included criteria show `Unknown` for every candidate; excluded criteria keep their assessments (hidden) and reappear unchanged when included again (ARCHITECTURE C51, C53) | I | 5 | planned |
| WSP-04 | Removing a candidate (`DELETE /api/v1/candidates/{id}`, Owner/Editor) deletes its evidences and assessments; a published entry from it stays visible, its `SourceCandidateProductId` becomes null, and afterwards only the user linked to the publishing profile can withdraw it (ARCHITECTURE C76, confirmed C87) | I | 5/6 | planned |

### 4.6 API conventions and concurrency (API) — ARCHITECTURE §7.1, §9.2, §9.4

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| API-01 | Every validation failure returns RFC 9457 Problem Details with an `errors` map keyed by field path (generic test over all write endpoints with an invalid payload) | C | 1 | planned |
| API-02 | Frontend: server-side field errors are bound to the matching form control, associated programmatically (`aria-describedby`), and announced (WCAG 3.3.1) | FE, E | 1 | planned |
| API-03 | Updates to shared workspace resources without `If-Match` are rejected; a stale ETag returns 409 including the competing editor's display name | I | 5 | planned |
| API-04 | ETags include the configured `ETagEpoch`; changing the epoch invalidates all previously issued ETags (restore safety, §7.1) | U, I | 5 | planned |
| API-05 | Rate-limit policies are active on workspace publication, contribution, vendor publication, anonymous export, product-identity requests, reports, counter-statements, registration and password reset (one test per policy: limit + 1 → 429 with Problem Details) | I | 1/2/6/7 | planned |
| API-06 | List endpoints honour `Accept-Language` (`de`, `en`) | C | 2 | planned |

### 4.7 Security (SEC) — ARCHITECTURE §8.3, §9.2, §9.7, D8, D14

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| SEC-01 | Session cookie is the ASP.NET Core Identity cookie with `HttpOnly; Secure; SameSite=Strict`; no token is written to `localStorage`, `sessionStorage` or any JavaScript-readable cookie except `XSRF-TOKEN` (FE test + E2E storage inspection) | I, FE, E | 3 | planned |
| SEC-02 | State-changing requests without a valid antiforgery token (`X-XSRF-TOKEN`) are rejected; the anonymous export is exempt because it changes no server state — asserted explicitly | I | 3 | planned |
| SEC-03 | API endpoints answer an unauthenticated request with 401 (never a 302 to a login page); on 401 the interceptor opens the in-app login overlay without navigation, keeps the page state and the in-memory draft, retries the failed request after sign-in and produces no loop; a 401 from the session probe is ignored | I, FE | 3 | planned |
| SEC-04 | Uploads: disallowed MIME types or extensions, and files above the size cap, are rejected; stored files get generated names; downloads are served with `Content-Disposition: attachment` and the restrictive CSP | I | 7 | planned |
| SEC-05 | Uploads are only accepted from authenticated Flow E submitters (D14 residual-risk boundary) | I | 7 | planned |
| SEC-06 | `RebuildDatabase` / `SampleDataInitializer` cannot run outside `Development`, even when the configuration flag is `true` | I | 1 | active |
| SEC-07 | Session limits with a controlled clock (`FakeTimeProvider`): `GET /auth/session` returns `expiresAt` (idle) and `absoluteExpiresAt` and does not renew the cookie; every other authenticated request renews the idle limit (a request at minute 20 keeps the session valid at minute 49); `POST /auth/session/extend` renews only the idle limit and keeps the sign-in time; every authenticated response carries `X-Session-Expires-At` and `X-Session-Absolute-Expires-At`; after 30 minutes without requests and after 8 hours from sign-in (regardless of activity) the next request yields 401 (ARCHITECTURE C52, C60) | I | 4 | planned |
| SEC-08 | Changing the password or deleting the account ends the user's other sessions: their next request yields 401 (security stamp validated; `OnValidatePrincipal` chains the security-stamp validator) | I | 4 | planned |
| SEC-10 | E-mail confirmation gate: an unconfirmed account can sign in, but saving a list, creating an organisation, accepting an invitation, creating a contributor profile and every publication path return 403 Problem Details `email-unconfirmed`; after confirmation they succeed; confirmation tokens are single use and expire after 24 h (ARCHITECTURE C72) | I, E | 3 | planned |
| SEC-11 | Sign-in lockout after 5 failed attempts for 15 minutes (controlled clock); password shorter than 12 characters rejected; registration and password-reset requests answer identically for known and unknown addresses; reset and invitation tokens are single use, expire (2 h / 14 days) and are stored hashed (ARCHITECTURE C72) | I | 3 | planned |
| SEC-09 | Anonymous `GET /auth/session` returns 200 `{ user: null }` and issues an `XSRF-TOKEN` cookie, so sign-in, registration and password reset succeed with that token; sign-in and sign-out issue a new token, and the old one is rejected afterwards; a state-changing request with a missing or invalid antiforgery token returns 400 Problem Details | I, E | 3 | planned |

### 4.8 Moderation (MOD) — ARCHITECTURE §5.5, §5.6

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| MOD-01 | `PublishedEntry.State` allows only the transitions `Published → Flagged → UnderReview → (Published \| Restricted \| Withdrawn)`; all others fail | U | 7 | planned |
| MOD-02 | Each moderation action (restrict, withdraw, reinstate) changes only `PublishedEntry.State` and appends a `ModerationAction`; an integration test runs every action and compares the entry's version rows before and after (the general rule is PUB-13, the API surface ARC-04) | I | 7 | planned |
| MOD-03 | `ModerationAction` is append-only: no update or delete path exists (architecture test + integration attempt) | A, I | 7 | planned |
| MOD-04 | No published content is hard-deleted by any endpoint, including the directory-level withdrawal endpoint | I | 6/7 | planned |
| MOD-05 | A counter-statement is returned alongside its entry, never replacing it | C | 7 | planned |
| MOD-06 | `ContentReport`, `CounterStatement` and `ModerationAction` store the `PublishedEntryVersionId` shown to the user when one is given (nullable); republication keeps the entry's `State`; a `Withdrawn` entry cannot be republished on any path (409) (ARCHITECTURE C55, C63) | I | 6/7 | planned |

### 4.9 Export (EXP) — ARCHITECTURE §4.4 Flow A, §9.4, D9, D12, D15; PRD §9

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| EXP-01 | XLSX export contains the filtered criteria with resolved priority, override and weight; re-opened with ClosedXML and compared against the configuration or ad-hoc selection | U, I | 2 | planned |
| EXP-02 | XLSX accessibility: a real header row, defined names for the data range, no meaning conveyed by colour alone, worksheet and column titles in the export language | U | 2 | planned |
| EXP-03 | The export records the catalog release version, so an exported criteria list can be traced to its release years later — for both export sources | U | 2 | planned |
| EXP-04 | CSV export uses UTF-8 with BOM and a stable column order | U | 2 | planned |
| EXP-05 | Anonymous ad-hoc export (`POST /api/v1/exports`) succeeds without authentication and **persists nothing**: row counts in all schemas except `analytics` are unchanged, no file is written to the file store, and the request's note texts (sentinels) do not appear in the log sink | I | 2 | planned |
| EXP-06 | Source parity: for the same selection and the same user decisions, `AdHocSelectionSource` and `PersistedConfigurationSource` produce identical cell content (generation metadata excluded) | U, I | 2/3 | planned |
| EXP-07 | The ad-hoc export re-resolves priorities on the server: client-supplied resolved levels are not accepted as input; criterion revision ids outside the stated release or not applicable to the selection yield 400 Problem Details with field errors; the selection (`useCaseTemplateId?`, `appTypeIds[]`, `productPartIds[]`, `testTypeIds[]`) is validated as in CAT-11/CAT-12; request size and note length caps are enforced | U, I | 2 | planned |

### 4.9a Directory search (DIR) — ARCHITECTURE §5.5, §7.2, D3, C32

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| DIR-01 | Directory filters work on entry context: a product matches an app-type or use-case filter iff it has ≥ 1 visible entry with that context; with a use-case filter set, entries without a use case do not make a product match; with no use-case filter all entries count. `testTypeIds[]` matches entries whose current version has a published test evidence with that procedure; experience reports count for the app-type filter, never for the use-case filter (they carry no use case, PUB-16) and never for test filters; `testKinds[]` (`other`, `own`, multi) matches evidence outside the five procedures and is combined with `testTypeIds[]` as alternatives; `hasReport` matches entries with a `ConformanceReport` | I, C | 6 | planned |
| DIR-03 | The staleness note `directory.entry.stale` is rendered iff the entry's current version was published more than the configured threshold ago (default 18 months, boundary tested at the day); it never affects order or filters (ARCHITECTURE C82) | FE | 6 | planned |
| DIR-02 | Directory search accepts only `sort` values `name`, `recent`, `entries`; any other value (incl. anything fulfilment-related) yields 400; filter combinations violating a `FilterExclusion` yield 400 as in CAT-12 | C | 6 | planned |

### 4.10 Architecture and operations (ARC, OPS) — ARCHITECTURE §4.3, §9.5, §10

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| ARC-01 | Catalog depends on no other module | A | 1 | planned |
| ARC-02 | Workspace and Directory do not reference each other's types | A | 1 | planned |
| ARC-03 | Each module is its own project with a separate `Contracts` project; a module project references only the `Contracts` projects listed in ARCHITECTURE §4.3 and Platform (one rule per row, checked on the project references and with NetArchTest); outside `Contracts` only controllers and the module registration extension are public; Export reads configurations only through `IConfigurationReader`; `AdHocSelectionSource` depends on Catalog.Contracts only (ARCHITECTURE C62) | A | 1 | planned |
| ARC-04 | Moderation reads and state-transitions Directory entries only through the designated API (no direct content setters reachable) | A | 7 | planned |
| ARC-05 | `BaseCrudController` is only used by catalog-admin controllers (D11) | A | 5 | planned |
| ARC-06 | AutoMapper configuration is valid (`AssertConfigurationIsValid`) | U | 0 | active |
| ARC-07 | Exactly three services call `PublishedEntryFactory` (`PublicationService`, `ContributionService`, `VendorPublicationService`); exactly two call `IProductIdentityResolver.Create` (`PublicationService`, `VendorPublicationService`) | A | 6 | planned |
| OPS-01 | `/health/live` returns healthy without dependencies; `/health/ready` reports unhealthy when the database is unreachable | I | 1 | active |
| OPS-02 | The log-retention job removes rows older than the configured period from the `logging` schema and nothing newer | I | 1 | active |
| OPS-03 | All EF Core migrations apply cleanly to an empty PostgreSQL 17 database, and the model has no pending changes | I | 0 | active |
| OPS-04 | A session survives an application restart: the Data Protection keys are persisted in the database, so a cookie issued before a restart of the API is still accepted afterwards (second `WebApplicationFactory` on the same database) (ARCHITECTURE C60) | I | 3 | planned |

### 4.11 Accessibility of the tool itself (A11Y) — ARCHITECTURE §9.4; PRD §5 goal 5

| ID | Invariant | Level | Step | Status |
|---|---|---|---|---|
| A11Y-01 | axe-core reports no violations tagged WCAG 2.0/2.1 A or AA on every state visited by journeys J1–J7 (blocking) | E | 0 | active |
| A11Y-02 | Every journey can be completed with keyboard only (no pointer events in the test) | E | 2+ | planned |
| A11Y-03 | SPA route changes are announced via `A11yAnnouncerService` and focus moves to the main heading | FE, E | 1 | planned |
| A11Y-04 | The session-expiry warning appears `Session:WarningLead` (default 5 minutes) before the earlier of the idle and the absolute limit (tested with Vitest fake timers and in E2E with `page.clock`), is announced (live region / alert dialog) and offers "Angemeldet bleiben" for the idle limit; choosing it extends the session (WCAG 2.2.1). Before the absolute limit the warning asks to save and sign in again instead. After a 401 the login overlay keeps the page and its form input (WCAG 2.2.1) | FE, E | 4 | planned |
| A11Y-05 | The document `lang` attribute follows the active UI language | FE, E | 1 | planned |
| A11Y-06 | User accessibility preferences (contrast, reduced motion) are applied and persisted | FE, E | 3 | planned |
| A11Y-08 | Both appearances: the axe run of A11Y-01 is repeated with the dark appearance (`prefers-color-scheme: dark` and the explicit setting); contrast violations block in both; the appearance setting persists in `UserPreferences` and applies without reload (ARCHITECTURE C73) | E, FE | 1 | planned |
| A11Y-07 | Manual screen-reader test protocol exists for the current release candidate with no open blocker findings (§7.2) | M | each step with UI changes | planned |

### 4.12 Conditional invariants for open decisions

See §10 for PRV-06 and the invariants that depend on ARCHITECTURE §16.2.7 and §16.2.8 (consent wording and purge timing only).

---

## 5. Strategy per area

### 5.1 Domain core (unit)

The parts of the system whose correctness is pure logic get the densest unit coverage and are the candidates for periodic mutation testing:

- **Priority resolution** (CAT-01/02): table-driven tests (`[Theory]` with a member data table mirroring ARCHITECTURE §5.1). Each row names the expected matched rule.
- **Cross-release matching** (CAT-03/04): one test per row of the §5.1 matching table, plus split, merge and chained `SupersededByKey` cases.
- **`PublishedEntryFactory`** (PUB-02/03/08/09/10/14/15/16, SIG-03, SIG-10): parameterised over the three origin kinds; each shared invariant has a "missing X → rejected" test per path, plus the channel/origin pairing table of PUB-10.
- **`IProductIdentityResolver`** (PUB-07): trigram candidates and normalisation, used identically by Flows B and E.
- **Export sources** (EXP-06/07): the same selection fed into both sources yields identical rows; the ad-hoc source rejects foreign or inapplicable revision ids.
- **Authorisation handlers** (`CanEditProject`, `CanPublish`, `CanContribute`, `CanPublishAsVendor`, `CanWithdrawEntry`, `CanModerate`): tested in isolation with constructed resources and principals; the full matrix is repeated at integration level (TEN-05, TEN-10) because wiring errors are the realistic failure.
- **State machines**: `PublishedEntry.State` (MOD-01), `ProductIdentityRequest.State`, `ProcurementProject.Status`.
- **Export formatters** (EXP-*): generate into a memory stream, re-open, inspect.

Services that are thin orchestration over repositories are tested at integration level instead of with mocked repositories. Mocking `DbContext` or repositories to unit-test them produces tests that pass while the real query is wrong.

### 5.2 Persistence and API (integration)

- **One PostgreSQL container per test run**, started by an xUnit assembly fixture; migrations applied once; Respawn resets data between test classes.
- **`WebApplicationFactory<Program>`** with environment `Testing`. Authentication in most tests uses a **test authentication handler** that issues a principal for a named fixture user. The real Identity cookie and antiforgery flow is tested explicitly (SEC-01/02/09) and in E2E, but other tests do not pay its cost.
- **Antiforgery in the test client (from step 0).** The shared test `HttpClient` calls `GET /api/v1/auth/session` once, keeps the `XSRF-TOKEN` cookie and sends it as `X-XSRF-TOKEN` on every non-GET request. Until the session probe exists (step 3) the helper tolerates its absence, so no POST test has to change when antiforgery validation is switched on.
- **Clock control.** `TimeProvider` is replaced by `FakeTimeProvider` and advanced instead of waiting (SEC-07, SEC-08, OPS-02). The anonymous export is called without any principal.
- **Own host for startup tests (v12).** Tests of what happens at startup or when the database goes away create their own `WebApplicationFactory` (other environment, extra host settings) on a database of their own (`ConnectionStringForNewDatabase()`, SEC-06) or on a container of their own that the test stops (OPS-01), so the shared host and database stay untouched. Every such factory is disposed by the test.
- **Contract tests** assert on the raw JSON (`JsonDocument`), not on deserialised DTOs. Deserialising into the production DTO would silently ignore an extra `score` property — exactly what SIG-02 must catch.
- **Endpoint discovery for generic tests** (API-01, SIG-02): enumerate endpoints from `EndpointDataSource` so that new endpoints are covered without editing the test.
- **"Persists nothing" checks** (EXP-05): snapshot row counts per schema via `information_schema`/`pg_stat_user_tables` before and after the call, so new tables are covered automatically.

### 5.3 Frontend (Vitest)

| Unit | What to test | What not to test |
|---|---|---|
| Store slices (`withCatalog`, `withConfigurator`, `withWorkspace`, `withDirectory`, `withIdentity`) | State transitions, computed signals, effects against `HttpTestingController`; **configurator draft survives the login outlet and can be saved in one call**; draft is never written to browser storage | Internal NgRx Signals mechanics |
| Interceptors | 401 → login overlay and retry without loop (SEC-03), antiforgery header, expiry headers update the session state (SEC-07), language header, error mapping to field errors (API-02) | |
| Domain components K1–K6 (UX copy §4.2) | K1 eight categories in the directory, seven in the workspace (SIG-08, SIG-13), `signals.fulfillment.none` instead of K1 when `fulfillment` is null (PUB-14), K1/K2 separate regions (SIG-09), K3 source label text always present, K4 always rendered with one line per test evidence (SIG-11), K5 rationale reachable without hover and "Empfehlung des Projektteams" present (CAT-02 at UI level), K6 default "Nicht eingeschätzt" | Pure layout |
| A11y behaviour | Announcer (A11Y-03), session warning with fake timers (A11Y-04), form error association (API-02) | Colour contrast (needs a real browser → E2E/manual) |
| Services | Request construction, mapping | Generated or trivial wrappers |

Component tests use Angular Testing Library queries by role and accessible name. `querySelector` by CSS class is reserved for cases where no accessible handle exists, and that absence should be questioned.

### 5.4 E2E journeys

Few journeys, each long enough to cross module boundaries. Every journey calls the axe helper after each significant state change (page load, dialog open, validation error shown, result rendered).

| ID | Journey | Covers | Introduced |
|---|---|---|---|
| J1 | **Configurator without account:** anonymous visitor selects use case, two app types, product parts and one test procedure → an excluded option is shown with its reason → criteria with visible rationale and the procedure criterion „Geprüft nach …“ → change a priority and exclude a criterion → export XLSX → downloaded file is opened and checked. Second part: sign in from the result page → the draft is still there → save it as a configuration → export again → both files have identical content | Flow A1/A2, CAT-02, EXP-01, EXP-05, EXP-06, SEC-10 (registration with the confirmation link from Mailpit before saving) | Step 0 (characterisation of today's filter), extended in step 2 (anonymous part) and step 3 (save part) |
| J2 | **Workspace & Kanal-2 publication:** create/join organisation → project → assign a saved list (the project gets its own copy) → candidate → test status "Nicht bekannt" → publish attempt is blocked with a field error → set test status „Ja“ and record two test evidences (BIT inklusiv Software-Test, BIK BITV-Test Web) → the two procedure criteria show the derived „Ja“ → assess criteria → publish → entry visible in directory as "Unabhängige Angaben", private notes absent | Flow B, PRV-02, SIG-03, SIG-10, PUB-09, PUB-12, WSP-01, WSP-02, CAT-17 | Step 5/6 |
| J3 | **Direct contribution:** user without organisation, with contributor profile, contributes an experience report to an existing product, with assistive-technology context, an app type and consent → report visible without test status and without counts → withdraw it under "Meine Beiträge" → text and assistive-technology context are gone | Flow D, PUB-04, PUB-16, PRV-09, PRV-10, SIG-12, SIG-14 | Step 6 |
| J4 | **Directory read:** anonymous product view (public variant) → sign in → same product against own configuration → eight-count breakdown and separate documentation signal | Flow C, SIG-07/08/09, TEN-03 | Step 6 |
| J5 | **Session:** clock controlled (`page.clock` + `POST /e2e/clock/advance`) → expiry warning announced → "Angemeldet bleiben" → work continues; second run lets the idle limit pass while a form contains input → the next save opens the login overlay → after sign-in the save is retried and the input is still there; third run reaches the absolute limit and shows the save-and-sign-in-again warning | A11Y-04, SEC-03, SEC-07 | Step 4 |
| J6 | **Moderation:** report an entry → moderator restricts it → vendor posts counter-statement → both visible | MOD-01/05 | Step 7 |
| J7 | **Vendor publication (Flow E):** user without organisation creates a vendor profile → "Prüfergebnis veröffentlichen" → new product identity (no trigram match) → app types and use case („vom Anbieter angegeben“) → test evidence with procedure and tested version → consent → publish → product found via the app-type filter → entry visible as "Angaben der Anbieterseite" without a fulfilment breakdown (no criteria assessed), consent data not visible → republish with new consent creates a new version | Flow E, PUB-04, PUB-05, PUB-08, PUB-11, PUB-14, PRV-08, DIR-01 | Step 6 |

Keyboard-only variants (A11Y-02) are added for J1 and J3 first: J1 is the core flow for the primary persona, J3 is the flow for the persona most likely to use assistive technology.

### 5.5 What we deliberately do not test

- Generic CRUD of catalog-admin controllers beyond one smoke test per controller.
- Individual AutoMapper maps (covered by ARC-06 plus the contract tests).
- Framework behaviour (ASP.NET Core routing, EF Core change tracking, Angular Material internals).
- Load, stress and soak testing. ARCHITECTURE §11 sizes the system two to three orders of magnitude above expected load. Exceptions that may be added: a single benchmark of the fulfilment projection query (§11.2) with a 2,000-entry fixture, and a single timing check of the anonymous export at the maximum criterion count of the MiniCatalog's production-sized counterpart — both run manually.
- Visual regression (pixel) testing: high maintenance for a solo maintainer, low value compared to axe plus manual review.
- Real-time collaboration, HA, failover (non-requirements, ARCHITECTURE §3.3).
- Verification of vendor identity: there is none to test until ARCHITECTURE §16.2.7 is decided.

---

## 6. Test data

### 6.1 Builders

`ProcureAccess.Tests.Shared` provides fluent builders with valid defaults (`ACatalogRelease()`, `AnOrganization()`, `ACandidateProduct()`, `APublishedEntry()`, `AVendorPublication()`, `AnAdHocSelection()` …). A test sets only what it is about. Builders persist through the real `DbContext`, never through raw SQL, so that EF configuration is exercised.

### 6.2 Canonical fixtures

| Fixture | Content | Purpose |
|---|---|---|
| **MiniCatalog** | Two releases (`T.1`, `T.2`), ~12 criteria. Between releases: one changed, one added, one removed, one split (non-1:1 `SupersededByKey`), one renamed 1:1. Priority rules at all four specificity levels | Every row of both §5.1 tables is reachable with one fixture |
| **Tenants** | Organisations **A** and **B**; per organisation one Owner, Editor and Reader; one user **without organisation** but with an individual contributor profile; one user **without organisation** with a vendor profile and one with an auditor profile; one `CatalogEditor`, `Moderator`, `PlatformAdmin` | TEN-* matrix, PUB-04, PUB-08, TEN-10 |
| **Sentinels** | Constants such as `"SENTINEL-INTERNAL-NOTES-7f3a"` used in every private field, in consent fields and in ad-hoc export notes | PRV-02, PRV-08, EXP-05: a single string search over responses, version rows and log output proves redaction |

### 6.3 E2E dataset

A dedicated E2E seeder (separate from `SampleDataInitializer`) runs only in the `E2E` environment and creates the MiniCatalog, the tenant users with known test credentials, and a handful of products with entries from all three origin kinds. It is idempotent so that a failed E2E run can be repeated without rebuilding the stack.

### 6.4 Rules

- No real personal data, ever — the repository is public.
- No dependency on `SampleDataInitializer` in any test; it is development convenience, not a test fixture.
- No test depends on another test's data or on execution order.

---

## 7. Accessibility testing

### 7.1 Automated layers

| Layer | Catches | Gate |
|---|---|---|
| Template lint (`@angular-eslint/template` a11y rules) | Missing alt text, invalid ARIA, click handlers without keyboard equivalent | Blocking (lint stage) |
| Component tests with role-based queries (+ `vitest-axe` on selected components, always on K1–K6) | Missing accessible names, broken label associations, unannounced state changes | Blocking (test stage) |
| Playwright + axe-core on every journey state | Rule-detectable WCAG 2.1 A/AA violations in the real rendered page, incl. contrast | Blocking (A11Y-01) |

Automated tools detect only a portion of WCAG failures. They cannot judge whether a label is meaningful, whether the reading order makes sense, whether a live announcement is helpful or noisy, or whether a plain-language criterion is actually understandable when heard through a screen reader. Those questions are answered in §7.2.

### 7.2 Manual testing (T3)

**Tester:** a member of the project team who uses a screen reader and performs professional accessibility testing. The tester's judgement on findings is authoritative; disagreements are resolved by fixing, not by debating severity.

**When**

| Occasion | Scope |
|---|---|
| End of each migration step that changes UI (steps 2–7) | Journeys touched by the step, plus a regression pass over J1 |
| Before the pilot with a cooperation partner (PRD §5 goal 4) | Full pass over J1–J7 and the export files |
| Before each production release with UI changes | Changed screens plus J1 |
| After a dependency upgrade that affects UI (Angular, Angular Material) | J1 and one workspace journey |

**Scope of each pass**

- Journeys completed with the tester's primary screen reader and with keyboard only.
- Reading the **exported XLSX** in a spreadsheet application with a screen reader (exports end up in tender documents and are read by others) — from both the anonymous and the saved export.
- Error flows: server-side validation errors, the "test status unknown" publication error, 409 conflict message, rate-limit message.
- Session-expiry warning (J5).
- Comprehensibility of the two-signal display, of "Damals nicht im Katalog" and of the source labels "Angaben der Anbieterseite" / "Unabhängige Angaben" when heard rather than seen — this is where ARCHITECTURE §8.4 is ultimately verified.
- Settings for contrast and reduced motion.
- **Mobile-first checks (new in v4)** at 360 px and at 320 px / 400 % zoom (reflow, WCAG 1.4.10): DOM order equals visual order, stacked actions with the primary action last, filter disclosure in S40, no horizontal scrolling except tables designated for it.
- **Device checks from `accessibility_review_procureaccess_v2.md`:**

  | Check | Setup |
  |---|---|
  | Combobox listbox while the on-screen keyboard is open | Android/Chrome + TalkBack, iPhone/Safari + VoiceOver, each with the largest system font |
  | Announcement of results in combobox and filters (frequency, no double announcements) | NVDA + Firefox, VoiceOver iOS |
  | Scrolling tab bar at 320 px | Keyboard and swipe gestures |
  | Full-screen dialog: focus trap; Android back gesture closes the dialog | TalkBack |

**Test basis:** EN 301 549 / WCAG 2.1 AA as referenced by BITV 2.0. The choice of screen-reader/browser combinations beyond the tester's primary setup (e.g. NVDA + Firefox, JAWS + Edge/Chrome, VoiceOver + Safari) is made with the tester, recorded in the protocol, and kept stable across passes so results are comparable.

**Output:** a protocol per pass in `docs/a11y-reports/YYYY-MM-DD_<occasion>.md` (date, build/commit, assistive technology and versions, journeys covered, findings with severity). Findings become GitHub issues labelled `a11y` with severity `blocker` / `major` / `minor`.

**Gate:** a release candidate with an open `a11y` + `blocker` issue is not promoted to production (A11Y-07). Major findings must have an issue and a planned fix; they do not block on their own.

### 7.3 Relation to step 7

Step 7f (`Design/accessibility-review`) reviews designs before they are built. The tests here verify what was built. Findings from 7f that cannot be checked automatically should be added to the manual test scope in §7.2.

---

## 8. Conventions and CI

### 8.1 Naming and tagging

- **.NET:** `MethodOrScenario_Condition_ExpectedOutcome`, written in domain language. Invariant tests carry the trait `[Trait("Invariant", "SIG-02")]` **and** include `INV:SIG-02` in the display name, so the tag is visible in test reports and greppable by the gate script.
- **Angular / Playwright:** the `describe` or `it` title contains `INV:<ID>`, e.g. `it('INV:SIG-08 renders all eight fulfilment categories together', …)`.
- Contract tests additionally carry `[Trait("Kind", "Contract")]` so they can be run on their own.
- One invariant may be covered by several tests; one test may cover several invariants (multiple tags).

### 8.2 Pipeline (GitHub Actions)

Extends ARCHITECTURE §10.3. Every pull request to `main` runs all stages; there is no separate nightly suite.

| Stage | Contents | Blocking |
|---|---|---|
| 1. Lint & licences | `dotnet format --verify-no-changes`, ESLint incl. template a11y rules, REUSE/SPDX check, `node scripts/check-figma-links.mjs` (every domain component and page carries a valid `@figma` tag, see `figma_code_mapping_procureaccess_v1.md`; files on the script's `PENDING` list get a notice instead, V60) | yes |
| 2. Build | .NET solution, Angular production build | yes |
| 3. Unit | `ProcureAccess.Tests.Unit`, Vitest | yes |
| 4. Architecture | `ProcureAccess.Tests.Architecture` | yes |
| 5. Integration | `ProcureAccess.Tests.Integration` (Testcontainers on the runner's Docker) | yes |
| 6. E2E + a11y | docker-compose E2E stack, Playwright J1–J7, axe on every state | yes |
| 7. Invariant gate | `node scripts/check-invariants.mjs` | yes |
| 8. Coverage report | coverlet + Vitest coverage merged into one report, uploaded as artefact, summary in the PR | no |
| 9. Security | CodeQL, Dependabot (repository settings) | CodeQL high findings: yes |

Deploy to staging on `main`, manual promotion to production (unchanged from ARCHITECTURE §10.3). Promotion additionally requires A11Y-07 for releases with UI changes.

### 8.3 Invariant gate

`scripts/check-invariants.mjs` (Node, no dependencies):

1. Parse all catalogue tables in the highest `docs/TESTING_v<n>.md` (versions compared as numbers) and collect IDs with status `active`. Catalogue tables are the tables with the header `ID | Invariant | Level | Step | Status`; the gate fails if it finds no such file or no rows.
2. Search `tests/**`, `CLIENT/**/src/**/*.spec.ts` and `e2e/**` for `INV:<ID>` tags.
3. Fail if an active invariant has no tag; warn if a tag references an unknown or non-active ID (typo protection).
4. Print a table of active invariants with the number of tests per invariant.

Manual (`M`) invariants are satisfied by a reference to the protocol file in the status column instead of a test tag, e.g. `active — docs/a11y-reports/2027-02-10_step-5.md`.

### 8.4 Coverage (reported, not gated)

Line and branch coverage are collected for orientation. Expected ranges, as a sanity check rather than a target: domain core (priority resolution, matching, factory, identity resolver, export sources, handlers) close to complete; services and controllers moderate; generated code, migrations and DTOs excluded from measurement. A sharp drop in a domain-core namespace in a PR is a review question, not a build failure.

---

## 9. Introduction along the migration path

Tests are introduced together with the code they protect. Starting a migration step means flipping that step's invariants from `planned` to `active` in §4 — the gate then forces the tests to exist before the step can be merged.

| Step | Test work | Invariants activated |
|---|---|---|
| **0 — Test foundation** (new, before step 1) | Root solution with test projects; Testcontainers fixture; test auth handler; test `HttpClient` with the antiforgery helper and `FakeTimeProvider` registration (§5.2); builders skeleton; switch Angular tests to Vitest and delete obsolete generated Karma specs; Playwright skeleton with axe helper; CI stages 1–8; **characterisation tests** of the existing criteria filter (API + J1 in its current form) | ARC-06, OPS-03, CAT-10, A11Y-01 (on current screens; existing violations are recorded as issues and either fixed or explicitly suppressed with an issue link — suppressions are listed in the axe helper and reviewed at every step) |
| **1 — Foundations** | Module namespaces → NetArchTest rules; Problem Details; health; log retention; `RebuildDatabase` guard; announcer | ARC-01–03, API-01/02, API-05 (registration/password reset), OPS-01/02, SEC-06, PRV-05, A11Y-03/05, A11Y-08 (theme scaffolding). The platform upgrade (.NET 10, Angular 22) is part of ARCHITECTURE §12 step 0 and precedes the step-0 test projects |
| **2 — Catalog versioning and export** | MiniCatalog fixture; priority resolution; release immutability; export pipeline with `AdHocSelectionSource`; "persists nothing" helper; configurator draft in the store; J1 anonymous part | CAT-01, CAT-02, CAT-05–09, EXP-01–05, EXP-07, TEN-04 (catalog part and export responses), API-05 (anonymous export), API-06, A11Y-02 (J1), CAT-11–CAT-14, CAT-16–CAT-21, CFG-01–CFG-04, CAT-23 |
| **3 — Identity, tenancy, session and saved lists** | Tenants fixture (incl. users without organisation and vendor/auditor profiles); reflection-driven isolation test; authorisation matrix skeleton; Identity cookie and antiforgery tests (incl. the explicit exemption of the anonymous export and the anonymous session probe); Data Protection key store; interceptor (antiforgery header, 401 → login overlay); `PersistedConfigurationSource` with organisation-owned configurations and `IConfigurationReader`; copies (re-pinning); share links; J1 save part | TEN-01, TEN-02, TEN-05 (skeleton), TEN-06, TEN-08, TEN-09, PRV-04 (identity part), EXP-06, CAT-22, SEC-01–03, SEC-09, SEC-10, SEC-11, OPS-04, A11Y-06, PRV-07 (account part), PRV-11, TEN-11, TEN-12 |
| **4 — Session warning and hardening** | Absolute limit, sliding renewal, expiry headers, extend endpoint, security-stamp invalidation, session warning; J5 with `page.clock` | SEC-07, SEC-08, A11Y-04 |
| **5 — Workspace** | Concurrency, workspace matrix, tri-state `TestStatus` with default `Unknown`; retire proposal tests; J2 up to assessment | API-03, API-04, TEN-05 (complete), ARC-05, SIG-03 (workspace part), PRV-07 (workspace part), CAT-15 (workspace), CAT-17 (workspace), SIG-13, WSP-01–03, WSP-04 |
| **6 — Directory, publication, contribution** | Factory with **all three paths at once**; shared identity resolver; redaction with sentinels; consent records; two-signal contracts; cross-release matching; withdrawal endpoint; J2 complete, J3, J4, J7 | SIG-01–11 (except SIG-03 workspace part), CAT-03/04, PRV-01–04, PRV-08, PUB-01–10, TEN-03, TEN-04 (directory part), TEN-10, API-05 (publication, contribution, vendor publication), ARC-07, MOD-04, CAT-15 (published), CAT-17 (directory), SIG-12, SIG-14, PUB-11, PUB-12, PUB-13 (versions), PUB-14–16, PRV-09, PRV-10, MOD-06 (republication part), DIR-01–03 |
| **7 — Moderation & P1** | State machine, append-only log, counter-statements, uploads (Flow E); J6 | MOD-01–06, TEN-07, SEC-04/05, ARC-04, API-05 (reports, counter-statements) |

Manual accessibility passes (A11Y-07) follow §7.2 at the end of steps 2 to 7.

---

## 10. Conditional sections for open decisions

These depend on decisions recorded in ARCHITECTURE §16. Until decided, the affected invariants have status `conditional` and are not enforced.

| Open question | If decided … | Test consequence |
|---|---|---|
| **§16.2.1 Erasure vs. moderation record** | Legal review outcome | PRV-06 written according to the decided retention rules |
| **§16.2.7 Authenticity of vendor and auditor claims** | Verification required | New invariant: Flow E is rejected for unverified profiles (or entries stay unpublished until verification); J7 gains a verification step; PUB-08 extended |
|  | No verification | No change; PUB-08 as written |
| **§16.2.8 Consent for experience reports** | Legal review sets wording, legal basis and the timing of the purge | PRV-09 and PRV-10 stay (consent for every report since TESTING_v5, purge on withdrawal since TESTING_v8); only the consent-text version, the purge timing and J3's asserted texts change |

**Resolved in v2 of this document** (ARCHITECTURE §16.4): counts in the API → Option A (SIG-07 planned); "Vorschlagen" for criteria → external channel (no invariants); Kanal 1 outside the workspace → Flow E (PUB-08–10, ARC-07, J7 planned).

**Resolved in v10 of this document** (ARCHITECTURE §16.4.16): catalog releases from data files → CAT-23 planned; removing candidates → WSP-04 planned.

**Resolved in v8 of this document** (ARCHITECTURE §16.4.14): unauthenticated share links → allowed for criteria lists (TEN-08, TEN-09 planned); re-pinning → copy operation adopted (CAT-22 planned).

---

## 11. Responsibilities

| Activity | Responsible |
|---|---|
| Writing and maintaining automated tests | Developer (with Claude Code, guided by this document and `CLAUDE.md`) |
| Keeping the invariant catalogue in sync with `ARCHITECTURE.md` | Developer; any architecture change that adds or removes a guarantee updates §4 in the same PR |
| Keeping UI-text assertions in sync with the UX copy | Developer; tests reference translation keys where possible rather than literal strings |
| Manual accessibility passes and their protocols | Project-team accessibility tester (T3) |
| Severity of accessibility findings | Project-team accessibility tester |
| Decisions in §10 | Projektteam and the bundled legal review, as listed in ARCHITECTURE §16 |

For `CLAUDE.md` (step 9): it should reference this document and state two rules for Claude Code sessions — every change touching a catalogued guarantee adds or updates a tagged test, and new tests follow the naming and tagging conventions of §8.1.

---

## 12. Open questions

### 12.1 Vitest and the Angular version (resolved in v4)

The owner moved the locked stack to **Angular 22** (ARCHITECTURE C37, 2026-10-12). This resolves both parts of the former question: the end of Angular 20 support in late 2026, and the experimental status of the Vitest builder in Angular 20. On current Angular versions Vitest is the default unit-test runner of the CLI builder `@angular/build:unit-test`. Step 0 introduces Vitest directly on Angular 22; please confirm the builder options against the angular.dev documentation of the installed version when setting it up.

### 12.2 Root solution

The codebase currently has two solution files (`DAL/DAL.ProcureAccess.sln`, `SERVICES/SERVICES.ProcureAccess.sln`), and the API references the projects directly. Test projects need a home that builds everything together. Step 0 assumes a new root `ProcureAccess.sln` that includes all production and test projects. The existing solutions can remain for now; whether they are retired is a tidy-up decision for step 1 (module namespaces).

### 12.3 External accessibility audit

The manual passes (§7.2) are internal. Whether an external audit (e.g. a BITV test by an accredited test lab) is commissioned before broad public release is a project decision. It would strengthen the tool's credibility given PRD §5 goal 5, but it is not assumed here.

### 12.4 Characterisation baseline for existing a11y violations

Step 0 will very likely find axe violations in the existing screens. Default policy: blocker-level violations are fixed in step 0; everything else is recorded as an issue and suppressed **by rule and selector** in the axe helper with the issue link, never globally. Suppressions are reviewed at the end of every migration step and must reach zero before the pilot.

### 12.5 Rate limiting of the anonymous export in E2E

J1 exports twice within seconds; production limits for the anonymous export may be tight. The `E2E` environment uses a relaxed policy (§3.3), and API-05 tests the production policy at integration level. If the production limit turns out to hinder normal use (e.g. several exports in a row while refining a list), it is a product tuning question, not a test question.

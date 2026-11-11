# S4-18b — The first `CriteriaListShown` caller

> **Sprint:** 04, P0 task S4-18b (M2.8; 0.5 points, split from S4-18 by O-S4-52). **Issues:** none. **Requirements / invariants:** R-MET-01 (exactly the events of SPEC §9.1, fire-and-forget), R-CON-10 („a `CriteriaListShown` usage event is written once per complete selection shown“); PRV-05 stays green (no new column). No invariant changes status, so no new TESTING version.
> **Status:** done — pull request open.
> **Scope:** `POST /api/v1/configurator/criteria-list-shown` in the Configurator module, the client call once per new selection shown on S11, silent on failure; SPEC §8.7 and the §7.3 row of its rate limit. Not here: the rate limiter itself (S4-21, O-S4-56 (b)), `ExportCreated` (S4-21), antiforgery (step 3, SEC-02).
> **Owner decisions (2026-11-11):** **O-S4-48 (a)** (before S4-18a) — an endpoint of its own in the Configurator module, called once per new selection, `signedIn` from the request's sign-in; **O-S4-55 (a)** — count when the selection shown differs from the last one counted (A → B → A three times, a return without a change none); **O-S4-56 (b)** — a SPEC §7.3 row now (client IP, 30 per 10 minutes 🔧), built with the limiter of S4-21. **Defaults accepted with the plan:** no antiforgery until step 3, which covers this endpoint like every other non-GET one; a failure shows nothing and is not repeated, and the request stays off the loading bar; only a loaded list counts (not `failed`, not `noRelease`), also one without criteria; `signedIn` from the legacy JWT until step 3.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `IUsageEventWriter.CriteriaListShown(bool signedIn)` and its background writer exist since S1-23/S3-23, with no caller | Only the endpoint and the client call are new |
| F2 | The Configurator module was an empty skeleton (`AddConfiguratorModule` only); it already references `Platform.Contracts` (ARCHITECTURE §4.3) | First endpoint there, mapped like `MapCatalogEndpoints()` (ARC-03 allows the registration class's public members) |
| F3 | SPEC §9.1 says „once per selection, not on re-render“ but not what a return to an earlier selection counts | O-S4-55 (a) |
| F4 | SPEC §7.3 had no limit for an anonymous endpoint that writes rows; the bounded event queue drops events beyond `UsageEvents:QueueCapacity`, so a flood could also lose `ExportCreated` events | O-S4-56 (b) |
| F5 | The error interceptor shows the server page (5xx), the offline notice (status 0) or a snackbar for every failed API request; `CALLER_HANDLES_PROBLEMS` covers only problem types | New marker `BACKGROUND_REQUEST` |
| F6 | The authentication default scheme is the legacy JWT, the fallback policy is null; an expired or invalid token on an anonymous endpoint is no 401, the request is simply not authenticated | `signedIn` = `User.Identity.IsAuthenticated`; an expired token counts as `anonymous` (P2) |
| F7 | `UsageEventWriterTests` had private helpers that wait for the background writer's rows | Moved to `Infrastructure/UsageEventRows`, used by both test classes |

## 2. Changes

| File | Change | Requirement / decision |
|---|---|---|
| `src/ProcureAccess.Modules.Configurator/Usage/CriteriaListShownEndpoint.cs` (new) | Minimal API `POST /criteria-list-shown`, `internal`, `AllowAnonymous`, no body, `204`; calls `IUsageEventWriter.CriteriaListShown(signedIn)` | R-MET-01, R-CON-10; O-S4-48 |
| `src/ProcureAccess.Modules.Configurator/ConfiguratorModule.cs` | `MapConfiguratorEndpoints()` (group `/configurator`, tag „Configurator“) | ARC-03 |
| `API/API.ProcureAccess/Program.cs` | Maps it on the versioned group after `MapCatalogEndpoints()`, before the Swagger set-up (#176) | — |
| `tests/ProcureAccess.Tests.Integration/Configurator/CriteriaListShownEndpointTests.cs` (new) | Anonymous and signed in → 204 without body and one event with the dimension; two calls → two events; GET → 405 and nothing stored | R-MET-01 |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/UsageEventRows.cs` (new), `Analytics/UsageEventWriterTests.cs` | Shared helpers: delete the events, wait for the stored rows | F7 |
| `core/errors/problem-details.ts`, `core/interceptors/error.interceptor.ts` (+ spec) | `BACKGROUND_REQUEST`: no snackbar, offline notice or server page for the marked request; an answer still counts for `ConnectionStatus` | F5 |
| `features/configurator/services/api/configurator-api.service.ts` (new, + spec) | `reportCriteriaListShown()`: POST without body, marked `BACKGROUND_REQUEST` | SPEC §8.7 |
| `features/configurator/state/with-configurator.ts` (+ spec) | `configurator.list.countedSelection`; `loadCriteriaList()` reports an applied, loaded list when its selection differs (`sameSelection`), sets it before the request and ignores the failure | O-S4-55 (a) |
| `pages/criteria-list/criteria-list.spec.ts`, `pages/criteria-select/criteria-select.spec.ts` | A `ConfiguratorApiService` double next to the `CatalogApiService` one (these tests provide no `HttpClient`) | — |
| `e2e/configurator/s11-list.spec.ts` | S11 sends one report (204); „Auswahl ändern“ and back without a change sends none | R-CON-10 |
| `docs/SPEC_v2.md` (v2.10) | §8.7 (new), §7.3 row, change rows v2.10-1/-2 | O-S4-48, O-S4-55, O-S4-56 |
| `docs/ARCHITECTURE_v36.md` (new; v34 deleted) | C122; §7.2 endpoint map; §5.7 callers | — |
| `docs/sprints/sprint_04/SPRINT_04.md`, `TASK_PROMPTS.md` | O-S4-55, O-S4-56; S4-21 row and prompt build the rate limit of this endpoint | O-S4-56 (b) |
| `CLAUDE.md` | §3 (S4-18b paragraph, the stale „nothing calls it yet“ of S1-23, header line), §4 (Configurator `Usage/`, the client service), §5 (`curl` of the endpoint) | — |

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, performance, edge cases, a11y) over the diff against `main`, plus an independent review agent, which found no bug in the production code but three tests that could not fail on what they are named for. Sorted per CLAUDE §6.8 (R1–R4 are fixes because a test that cannot tell the owner's rule from the rejected one does not prove the behaviour):

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | „reports A → B → A three times“ ran A → B → C (the use-case change cleared the product part and test type of the first selection), so rule (b) of O-S4-55 passed it too | **Fix** | The test rebuilds the first selection exactly and checks it; a mutation to rule (b) now fails it |
| R2 | „reports only the answer that is applied“ asked twice for the same selection, so `sameSelection` hid a counted stale answer | **Fix** | The selection changes between the two loads, the newer answer comes first; a mutation that counts stale answers now fails it |
| R3 | The E2E negative check after `networkidle` could not fail: the state was already reached on a client-side navigation, and responses were counted, not requests | **Fix** | Counts sent requests and waits a bounded 500 ms |
| R4 | The 405 integration test claimed to prove that nothing was stored, but waited only for any one row | **Fix** | Keeps only the 405 check (a GET cannot reach the POST handler) |
| R5 | A list whose answer arrives after the user has left S11 (e.g. „Auswahl ändern“ during the load) is still counted, because the store applies it | **Won't fix** | The list was requested by S11 for a complete selection and is applied to the state S11 shows; tracking the page's presence adds a mechanism for a rare case of a coarse metric |

Checked without finding: no personal data leaves the client (no body; the server stores only the day and the dimension, PRV-05); no new dependency; no UI text; the endpoint reads no body and writes nothing synchronously, so it cannot fail because of the database; a stale list answer is never counted (`latestListRequest`); a failed report is never repeated and cannot reject unhandled (`.catch`); the marker is set only on this request.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 674 of 674 pass (4 new integration tests; architecture tests 59 of 59, incl. ARC-03 with the new public member); after R4 the two usage-event classes again: 14 of 14 |
| C3 | `npx ng test` (client), `npm run lint` | 475 of 475 pass (464 before; new: 5 store, 1 service, 5 interceptor); lint passes. After R1/R2: two temporary mutations (rule (b) of O-S4-55; counting a stale answer) each fail exactly the matching test |
| C4 | `npm run build:release` | Builds; the one warning („Could not parse 1 selector“) is the known one of `main` |
| C5 | `check-invariants.mjs` (TESTING_v47), `check-figma-links.mjs`, `check-i18n.mjs` (315 keys, UX copy v22), `check-icons.mjs` | Exit 0 each |
| C6 | `reuse lint` | Compliant, 1178 / 1178 files |
| C7 | `e2e/`: `npm run typecheck`, `npm run lint` | Pass |
| C8 | E2E on the local E2E stack (dev stack stopped and started again): `s11-list`, `journeys`, `reflow` in Chromium and Firefox, then the whole suite in Chromium, Firefox and mobile Chrome | 36 of 36, then 207 of 207; after R3 `s11-list` twice in the three browsers (stack started without rebuild, the app code being unchanged): 30 of 30. WebKit and mobile Safari only in CI |
| C9 | Disk | The image build filled the root disk (0 bytes free); `docker image prune` (dangling build images only) freed 1 GB, about 630 MB free afterwards — the cause outside Docker is still unknown (S4-18a) |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | The endpoint has no rate limit yet (SPEC §7.3 row v2.10) | S4-21 (O-S4-56 (b)) |
| P2 | Until step 3 a user whose legacy JWT has expired, but whom the client still shows as signed in, is counted as `anonymous` | Goes with the cookie session of step 3 |
| P3 | No antiforgery on this POST (none exists yet) | Step 3, SEC-02: every non-GET endpoint, this one included |
| P4 | The root disk is nearly full on the development machine (C9) | Owner: find the space outside Docker (probably `/var`) |

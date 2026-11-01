# S2-10 — Problem Details middleware

> **Sprint:** 02, P0 task S2-10 (roadmap M1.3, 2 points; ARCHITECTURE §12 step 1). **Issues:** none; removes the ad-hoc error objects of `TECH_DEBT_v1.md` §5. **Requirements / invariants:** SPEC_v2 §7.1 (Problem Details with field-keyed `errors`), §8.6 (problem types); ARCHITECTURE_v15 §7.1, C78; **API-01** → `active` (`TESTING_v18.md`, V78, V79). **Status:** done by Claude Code (2026-11-01), pull request open.
> **Scope:** RFC 9457 Problem Details in the Platform module for every error response of the API, the problem types of SPEC §8.6 under a configurable base URI, the replacement of the ad-hoc error objects in `BaseCrudController` and `UICustomizationController`, API-01 as one generic test over all write endpoints. Not in scope: the client mapping of problem types to UX keys and API-02 (S2-11), the ad-hoc error objects of `UsersController` and the identity endpoints (replaced in step 3), rate limiting (API-05).
> **Owner decisions 2026-11-01:** **1** the former member `code` is dropped, not kept as an extension member: until S2-11 maps problem types, the legacy client shows its generic error text for these failures. **2** Platform also covers minimal APIs (their bad requests become validation problems), so API-01 needs no exclusions. **3** `ProblemTypes` holds 13 names: the 12 of SPEC §8.6 plus `email-unconfirmed` of ARCHITECTURE §7.1 / C72; SPEC should add it in its next version (P3). **4** The failures of the two legacy controllers stay 400, now as Problem Details without the exception text.

## 1. Starting point

Measured with a throwaway probe test against the real API (not committed): every POST/PUT/PATCH/DELETE endpoint with a malformed and with a wrongly typed body.

| # | Finding | Consequence |
|---|---|---|
| F1 | Controllers already answer binding errors with `ValidationProblemDetails` through `[ApiController]`, but with the framework type `https://tools.ietf.org/html/rfc9110#section-15.5.1` and mixed keys: `$`, `$.id`, `dto`, `NewEmail` | Platform sets the type `validation` and keys errors by field path as the client sends it |
| F2 | The minimal-API identity endpoints (`/api/signIn`, `signUp`, `forgot-password`, `reset-password`, `refresh`) answer a malformed body with an empty 400; `signUp` and `reset-password` throw `ArgumentNullException` on a missing field | Bad requests of minimal APIs become validation problems (decision 2); the missing-field crash stays (P1) |
| F3 | No exception handler: an unhandled exception is an empty 500 (and goes straight through the test server) | `UseExceptionHandler` with Problem Details, without the exception text |
| F4 | `BaseCrudController` and `UICustomizationController` answer every exception with `{ code, message = ex.Message }`, i.e. the exception text reaches the client; `DeleteOne` even returns the bare message string | `Problem(400)`, exception logged (decision 4) |
| F5 | The client's `error.interceptor.ts` maps `code` to a snackbar key (`ERROR_TRANSLATIONS`) | Without `code` these failures show `ERROR.GENERIC` until S2-11 (decision 1) |
| F6 | Every legacy action carries `[Produces("application/json")]`, which overrides the media type `application/problem+json` of RFC 9457 on its Problem Details | Accepted for legacy actions in the test (V79); removing the attribute would also change plain-string responses (P2) |
| F7 | The generic create and update of `BaseCrudController` always fail: `MappingProfile` has no DTO → entity maps (`AutoMapperMappingException`) | Not fixed (scope); found while writing the test, which uses a substituted service instead (P4) |
| F8 | The versioned routes `api/v{version}/[controller]` of `BaseCrudController` produce no endpoint; only `api/[controller]` exists | Tests use `/api/<controller>` (P5) |
| F9 | SPEC_v2 §8.6 lists 12 problem types, ARCHITECTURE_v15 §7.1 and C72 13 (`email-unconfirmed`) | Owner decision 3 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `src/ProcureAccess.Modules.Platform.Contracts/Errors/ProblemTypes.cs` (new) | The 13 type names as constants plus `All` | SPEC §8.6, C78; contract for every module |
| `src/ProcureAccess.Modules.Platform/Errors/ProblemDetailsRegistration.cs` (new, internal) | `AddProblemDetails` with a customisation that runs for every document: a name of `ProblemTypes` becomes `<base>/problems/<name>`; with no type or a framework type, validation problems get `validation` and 404 gets `not-found`; `errors` keys lose the JSON-path root `$.`. Base URI from `ProblemTypes:BaseUri`, startup fails unless it is an absolute http(s) URI (trailing slash trimmed). `ThrowOnBadRequest` for minimal APIs; `SystemTextJsonValidationMetadataProvider`, so validation keys use the JSON names. `UsePlatformProblemDetails()` = `UseExceptionHandler()` + `UseStatusCodePages()` | F1–F3 |
| `src/ProcureAccess.Modules.Platform/Errors/BadRequestExceptionHandler.cs` (new, internal) | `IExceptionHandler` for `BadHttpRequestException`: 400 → validation problem keyed by the JSON path of the reader error, otherwise `$`; other statuses (e.g. 413) → plain Problem Details | F2, decision 2 |
| `src/ProcureAccess.Modules.Platform/PlatformModule.cs` | `AddPlatformModule` registers Problem Details; new public `UsePlatformMiddleware()` | Only the module registration is public (ARC-03) |
| `API/API.ProcureAccess/Program.cs` | `app.UsePlatformMiddleware()` first in the pipeline; module comment | F3 |
| `API/API.ProcureAccess/appsettings.json` | `ProblemTypes:BaseUri` = `https://procureaccess.de` (🔧 default of SPEC §8.6) | CLAUDE §6.6: 🔧 values from configuration |
| `API/API.ProcureAccess/Controllers/Base/BaseCrudController.cs`, `UICustomizationController.cs` | Ad-hoc objects replaced by `Failure(ex, detail)`: logs the exception, returns `Problem(400, detail)`; the duplicate `CustomException` catch blocks and their TODOs removed | F4, decisions 1 and 4, TECH_DEBT_v1 §5 |
| `API/API.ProcureAccess/API.ProcureAccess.xml` | Regenerated by the build (documentation of `Failure`) | Tracked file |
| `tests/…/Api/ProblemDetailsTests.cs` (new) | **INV:API-01** `WriteEndpoints_InvalidPayload_ReturnValidationProblemDetails` (generic, `EndpointDataSource`, 27 requests today) and `ValidationErrors_FieldErrors_KeyedByJsonFieldPath`; unknown route → `not-found`; UI customisation failure without exception text | API-01, F1, F4 |
| `tests/…/Api/ProblemDetailsHostTests.cs` (new) | Host of its own (base URI `https://problems.example.test`, test controller, throwing `IFilterTypeService`): name → URI under the configured base, validation type under it, unhandled exception → 500 without text, CRUD create/update/delete failure → 400 without text, startup fails for a missing or invalid base URI | C78, F3, F4 |
| `tests/…/Api/ProblemResponse.cs` (new) | Raw-JSON reader with the shared RFC 9457 checks (TESTING §5.2) | Contract tests |
| `tests/…/Infrastructure/ProcureAccessApiFactory.cs` | Optional `configureServices` for hosts of their own | Test controller and substitute |
| `docs/TESTING_v18.md` (new) | Copy of v17: status line; V78 (API-01 `active`, how the generic test works), V79 (media-type tolerance for legacy actions); §4.6 API-01 `active` | Prompt: flip in the next TESTING version |
| `CLAUDE.md` | Header; §3 (Problem Details as built); §4 (`Errors/`, `Integration/Api/`) | DoD |

### 2.1 Error keys

`errors` is keyed by the field path as the client sends it: `newEmail`, `items[0].note`. Errors that belong to the body as a whole are keyed `$` (malformed JSON, a missing query parameter of a minimal API). For a body that cannot be bound, controllers also add the parameter name (`"dto": ["The dto field is required."]`); S2-11 can treat any key without a form control as a summary-only error.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; run with the skill *engineering/code-review* on the diff against `main`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: no exception text reaches the client any more (former F4); the 500 of the exception handler carries no detail (tested). Validation messages of the JSON reader carry path, line and byte position, as before in controllers | – |
| R2 | Security (low): for minimal APIs without a JSON reader error, the message of `BadHttpRequestException` names the C# parameter (`Required parameter "string refreshToken" …`) | Won't fix: same level as the controllers' `The dto field is required.`; the affected endpoints are replaced in step 3 |
| R3 | Correctness: the customisation can run twice on one document without harm (a full URI is neither a name nor a framework type; keys are already normalised) | – |
| R4 | Correctness: `UseStatusCodePages` gives empty 401/403/404/405 bodies a Problem Details document. The client reads only the status of 401 (`auth.interceptor.ts`) and falls back to `ERROR.GENERIC` for unknown bodies, as before | – (E2E J1 green) |
| R5 | Edge case: a JSON path rooted in an array (`$[0].name`) keeps its `$` | Won't fix: no endpoint takes a top-level array |
| R6 | Behaviour: the two legacy controllers now log every caught exception at Error, also the always-failing AutoMapper create/update (F7) | – intended: the failures were silent before |
| R7 | Test design: the generic test sends as `approver`; an endpoint that needs another role answers 403 and fails the test, which is the wanted signal | – documented in the test |
| R8 | N+1, a11y: not applicable (no queries, no UI) | – |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | New tests before the code (TDD) | ✅ red for the expected reasons: framework type, `application/json`, empty bodies of minimal APIs and 404, no startup check |
| 2 | `dotnet test --solution ProcureAccess.sln` | ✅ 135 passed (unit 10, architecture 9, integration 116), 0 failed |
| 3 | `dotnet build` | ✅ no new warning; the warnings in touched legacy files are the existing VSTHRD200/CS8600 |
| 4 | `dotnet format` whitespace + style (solution), analyzers per test project | ✅ no changes |
| 5 | `node scripts/check-invariants.mjs` | ✅ passed, catalogue `TESTING_v18.md`, 12 active, API-01 with 2 tests |
| 6 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 7 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 595/595 |
| 8 | E2E stack + `npx playwright test --project=chromium` | ✅ 15 passed (J1 incl. CAT-10/A11Y-01); the other four projects run in CI |
| 9 | Vitest, ESLint | – no client code touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | `UsersController` and the minimal-API identity endpoints still answer business failures with `{ code, message }` (`SignUpFailure`, `ChangePasswordFailure` …, partly with Identity error lists), `BadRequest(ModelState)` is dead code under `[ApiController]`, and `signUp` / `reset-password` crash with a 500 on a missing field. They pass API-01's malformed-body test but are not validation problems for business rules | Step 3 (M3) replaces these endpoints |
| P2 | Legacy actions send Problem Details as `application/json` because of `[Produces("application/json")]` (F6, TESTING_v18 V79); new controllers must not copy the attribute. The tolerance in `ProblemResponse.AcceptedMediaTypes` goes when the legacy controllers do | With the replacing steps; reviewers of new controllers |
| P3 | SPEC_v2 §8.6 lacks `email-unconfirmed` (decision 3); ARCHITECTURE §4.3 "as built" for Platform does not mention Problem Details yet | Next SPEC and ARCHITECTURE versions |
| P4 | The generic create and update of `BaseCrudController` always fail (no DTO → entity maps in `MappingProfile`, F7); `UpdateOne` also returns 200 when the record does not exist; `DeleteOne` returns 200 with `0` for an unknown id | Proposed issue; catalog administration is rebuilt in step 2 (D11) |
| P5 | The versioned routes `api/v{version}/[controller]` of `BaseCrudController` produce no endpoint (F8) | Proposed issue, or with `/api/v1` in step 2 |
| P6 | `POST /api/Proposals/approve` by a user without the permission ends in a 500 (`UnauthorizedAccessException`), now as Problem Details | Proposal retirement (step 5) |
| P7 | Client: `ERROR_TRANSLATIONS` still lists `CreateFailure`, `UpdateFailure`, `DeleteFailure`, `UICustomizationNotFound`, which no response carries any more (decision 1); these failures show `ERROR.GENERIC` | S2-11 (problem type → UX key) |
| P8 | TECH_DEBT_v1 §5 still lists the ad-hoc error objects | Sprint 02 close-out, TECH_DEBT_v2 |

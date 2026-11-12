# S4-21a — Rate limits, client IP and /health/ready

> **Sprint:** 04, P0 task S4-21a (API part of S4-21, M2.7; split by O-S4-73). **Issues:** #112. **Requirements / invariants:** SPEC §7.3 (rate limits), R-MET-01 (`ExportCreated`), R-ADM-04 (health endpoints); API-05 → `active` for the anonymous export and `CriteriaListShown`.
> **Status:** done — pull request #293 open.
> **Scope:** the client IP behind the reverse proxies, the rate limiter of Platform with the policies `AnonymousExport` and `CriteriaListShown`, the limit and the configurable timeout of `/health/ready`, the `ExportCreated` caller, `Content-Disposition` exposed by CORS, the relaxed E2E limits; SPEC v2.12, ARCHITECTURE_v40, TESTING_v51. Not here: the export dialog S13, the download, the client handling of 429 and Problem Details from a blob body, `errors.rateLimit.one` — all S4-21b.
> **Owner decisions (2026-11-12, all (a)):** **O-S4-69** — the API takes the client IP from `X-Forwarded-For`, trusted only from proxies in private and loopback networks, for at most 🔧 2 proxies. **O-S4-70** — `/health/ready`: at most 2 checks at a time over all callers, no queue, 429; the timeout 🔧. **O-S4-71** — UX copy v25 U110 `errors.rateLimit.one` (S4-21b; wording confirmed). **O-S4-72** — `criteria.export.outdated` with „Schließen“ only, the reload flow a follow-up (S4-21b). **O-S4-73** — S4-21 split into S4-21a (API) and S4-21b (client). **O-S4-74** — the client reads Problem Details from a blob error body and gets a request marker for a caller that shows every failure itself (S4-21b).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The API never read `X-Forwarded-For`; behind Traefik and the client's nginx every request came from nginx | A limit „per client IP“ would have been one limit for everyone → O-S4-69 |
| F2 | SPEC §7.3 had no row for `/health/ready`; monitors and healthchecks share few addresses | A limit per IP would not protect the connection pool → O-S4-70 (concurrency, not rate) |
| F3 | `ForwardedHeadersOptions.KnownNetworks` is obsolete in .NET 10 | `KnownIPNetworks` with `System.Net.IPNetwork` |
| F4 | A rejected fixed-window lease reports the whole window as `RetryAfter` (10 minutes), not the time left; checked in `RateLimitingRegistrationTests` | `retryAfterMinutes` is an upper bound, recorded in SPEC v2.12-3 |
| F5 | The rate limiter keeps its own clock, not `TimeProvider` (CLAUDE §7) | Tests check counts, never the end of a window |
| F6 | The in-memory test server gives every request no remote address, so all tests of the shared host share one counter | The shared host raises both limits; the rate-limit tests run a host of their own and set the address through a startup filter |
| F7 | Every E2E test of a run comes from one address | `docker-compose.e2e.yml` raises both limits to 10 000 |
| F8 | The release build calls the API on its own origin, `npm start` on another one; CORS hides `Content-Disposition` from a cross-origin page | The CORS policy exposes it, so S4-21b can read the file name in both set-ups |
| F9 | No other code reads the remote address, and the logs do not carry it | The forwarded-headers middleware changes nothing but the rate-limit partition |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `src/ProcureAccess.Modules.Platform.Contracts/RateLimiting/RateLimitPolicies.cs` (new) | Policy names `AnonymousExport`, `CriteriaListShown`; modules attach them with `RequireRateLimiting` | API-05, ARC-01 – ARC-03 |
| `src/ProcureAccess.Modules.Platform/RateLimiting/ClientAddress.cs` (new) | Forwarded-headers middleware (`X-Forwarded-For` only, trusted networks 10/8, 172.16/12, 192.168/16, 127/8, ::1, fc00::/7, 🔧 `ForwardedHeaders:ForwardLimit` = 2); partition = IPv4 address or IPv6 /64 | O-S4-69 |
| `src/ProcureAccess.Modules.Platform/RateLimiting/RateLimitingRegistration.cs` (new) | Fixed window per client IP for each policy (🔧 `RateLimits:<policy>:PermitLimit` / `:Window`, 30 per 10 minutes; a bad value stops the start); 429 Problem Details `rate-limit` with `retryAfterMinutes` and `Retry-After` | SPEC §7.3, API-05 |
| `src/ProcureAccess.Modules.Platform/Health/HealthChecks.cs`, `HealthSettings.cs` (new) | Database check timeout 🔧 `Health:ReadyTimeout` (5 s); concurrency policy on `/health/ready`, 🔧 `Health:ReadyConcurrentChecks` (2), no queue | #112, O-S4-70 |
| `src/ProcureAccess.Modules.Platform/PlatformModule.cs` | Registers both; `UsePlatformMiddleware` runs the forwarded headers first; new `UsePlatformRateLimiting()` | — |
| `API/API.ProcureAccess/Program.cs` | `app.UsePlatformRateLimiting()` after CORS and authentication | — |
| `API/API.ProcureAccess/appsettings.json` | 🔧 defaults `Health`, `ForwardedHeaders`, `RateLimits` | SD23 |
| `API/API.ProcureAccess/Extensions/AppConfigExtensions.cs` | CORS exposes `Content-Disposition` | R-LST-11 (S4-21b) |
| `src/ProcureAccess.Modules.Export/Http/ExportEndpoint.cs` | Policy `AnonymousExport`; writes `ExportCreated` (format, signed in) after the file is created | API-05, R-MET-01 |
| `src/ProcureAccess.Modules.Configurator/Usage/CriteriaListShownEndpoint.cs` | Policy `CriteriaListShown` | O-S4-56 (b) |
| `docker-compose.e2e.yml` | Both limits 10 000 in the E2E stack | TESTING §3.3 |
| `tests/ProcureAccess.Tests.Integration/Api/RateLimitTests.cs` (new) | `INV:API-05`: limit + 1 → 429 (both endpoints), clients behind the proxies separate, IPv6 /64 shared, untrusted header ignored, IPv4-mapped proxy trusted, nothing read beyond the forward limit; 🔧 defaults in `appsettings.json` | API-05 |
| `tests/ProcureAccess.Tests.Integration/Infrastructure/TestRemoteAddress.cs` (new), `ApiFixture.cs`, `ProcureAccessApiFactory.cs` | Startup filter for the remote address; `ConnectionString` of the shared database; relaxed limits on every test host | — |
| `tests/ProcureAccess.Tests.Integration/Operations/HealthCheckTests.cs` | A third probe while two checks wait → 429 at once; the configured timeout is the database check's | #112 |
| `tests/ProcureAccess.Tests.Integration/Export/ExportCreatedEventTests.cs` (new) | One event per created file with format and audience; none for a refused request | R-MET-01 |
| `tests/ProcureAccess.Tests.Unit/RateLimiting/*.cs`, `Health/HealthSettingsTests.cs` (new) | Partition, trusted networks, rounding, lease metadata, settings validation | — |
| `docs/SPEC_v2.md` (v2.12), `docs/ARCHITECTURE_v40.md` (C126; v38 deleted), `docs/TESTING_v51.md` (V159 – V162; v49 deleted), `CLAUDE.md` §3/§4/§5, `docs/sprints/sprint_04/SPRINT_04.md` (S4-21a/b rows, O-S4-69 – O-S4-74) | Documentation | — |

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases, a11y) over the diff against `main`. Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | `docker-compose.yml` publishes the API port (`5000:8080`) on the host. With Docker's default DNAT an outside caller keeps its own (untrusted) address, so a forged header is ignored; through Docker's userland proxy it would arrive from the gateway (172.x, trusted) and could choose its counter | **Follow-up** | Proposed issue: on staging/production publish no API port (or bind it to 127.0.0.1) and confirm that Traefik does not route straight to the API with an appended untrusted `X-Forwarded-For` (P2) |
| R2 | `retryAfterMinutes` always says 10 for a 10-minute window | **Won't fix** | An upper bound, never too short; SPEC v2.12-3. A sliding window would not report the time left either |
| R3 | `ExportCreated` is written before the response is streamed; a client that aborts the download is still counted | **Won't fix** | SPEC §9.1 counts a generated file |
| R4 | The limiter does not read `TimeProvider` | **Won't fix** | Framework clock; the window is configuration, not domain time (F5) |
| R5 | An IPv6 client is counted by its /64 network, which SPEC §7.3 did not say | **Fix (documented)** | Otherwise one client could use 2⁶⁴ counters; SPEC v2.12-1, C126. Owner may object in the review |

Checked without finding: the 429 is written before the body is read (export) and carries the CORS headers (`UsePlatformRateLimiting` after `UseCors`); partitions live in memory only and idle ones are removed by the framework; the health limiter only covers `/health/ready`, `/health/live` stays unlimited; ARC-01 – ARC-03 green (the modules use only `Platform.Contracts`); no new dependency (the rate limiter and forwarded headers are part of ASP.NET Core).

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror …`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 809 of 809 pass |
| C3 | Mutation: trust every network (`0.0.0.0/0`) | `Post_ForwardedHeaderFromAnUntrustedAddress_DoesNotChooseTheCounter` fails, the others pass; reverted |
| C4 | By hand on Kestrel (`dotnet run` from this branch, throwaway PostgreSQL 17, port 5292; the dev stack untouched) | 30 × `POST /api/v1/exports` from one client → 400, the 31st → 429 `rate-limit`, `retryAfterMinutes` 10, `Retry-After: 600`; 31 requests forwarded through loopback for 31 clients → no 429; `/health/ready` 200 |
| C5 | `check-invariants.mjs` (TESTING_v51), `check-figma-links.mjs`, `check-i18n.mjs` | Exit 0 each |
| C6 | `reuse lint` | Compliant |
| C7 | Client, E2E | No client code change; E2E left to CI (the dev stack was running) — the compose change only raises two limits |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request #293 opened; not merged | Owner |
| P2 | The client IP is only as trustworthy as the deployment: no published API port on staging/production, and no direct Traefik route to the API that appends a forged `X-Forwarded-For` (R1) | Owner: confirm in Dokploy; proposed issue |
| P3 | S13, the download, 429 and blob Problem Details on the client, `errors.rateLimit.one` (U110), `criteria.export.outdated` with „Schließen“ | S4-21b |
| P4 | Reload flow after `release-mismatch` (unpin the release, reload S11, drop per-criterion changes, S10 if the selection is now excluded; needs a UX copy key) | Follow-up issue (O-S4-72) |
| P5 | The other API-05 policies (registration, password reset, saved export, publication, contribution, …) | With their endpoints (steps 2/3/6/7) |
| P6 | TECH_DEBT_v3 lists #112 | The sprint-end version of TECH_DEBT (process step 14) marks it resolved |

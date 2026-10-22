# S1-06 — Docker on .NET 10 and smoke test of the compose stack

> **Sprint:** 01, task S1-06 (milestone M0.1). **Status:** API checks done (2026-10-22); manual browser smoke test: see §4.
> **Scope:** API Docker image on the .NET 10 base images; CORS origins from configuration so the local client can reach the API. The client image is unchanged (S1-11).

## 1. Changes

| File | Change | Why |
|---|---|---|
| `API/API.ProcureAccess/Dockerfile` | `sdk:8.0` / `aspnet:8.0` → `sdk:10.0` / `aspnet:10.0`; copies `Directory.Build.props` | The projects target `net10.0` since S1-04. Without `Directory.Build.props` the image restore misses the AutoMapper advisory suppression (issue #31) |
| `API/API.ProcureAccess/Extensions/AppConfigExtensions.cs`, `Program.cs` | One CORS policy (`frontend`) with origins from `Cors:AllowedOrigins`, applied once before authentication | CORS allowed only `https://procureaccess.tech` (hard-coded twice), so the browser blocked every call from the local client (`http://localhost:4200` → `http://localhost:5000`) |
| `appsettings.json` | `Cors:AllowedOrigins = [ "https://procureaccess.tech" ]` | Same origin as before for every environment |
| `appsettings.Development.json` | `Cors:AllowedOrigins = [ "https://procureaccess.tech", "http://localhost:4200" ]` | Configuration arrays merge by index, so the production origin is listed again. `docker-compose.yml` runs the API as `Development` |

Unchanged on purpose: `-c Debug` in the Dockerfile (existing TODO), `docker-compose.yml` (external network `dokploy-network`), client Dockerfile and nginx config (S1-11).

## 2. Setup

| Item | Version |
|---|---|
| Docker / Compose | 29.9.1 / 5.6.0 |
| API base images | `mcr.microsoft.com/dotnet/sdk:10.0`, `aspnet:10.0` (Ubuntu 24.04.5, ASP.NET Core runtime 10.0.13) |
| Database | `postgres:17-alpine`, fresh compose volume |
| Client image | unchanged (Angular 20, development build, `apiUrl` = `http://localhost:5000/api`) |

## 3. Automated checks (run by Claude Code, 2026-10-22)

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln` | 0 errors; same warning set as `main` (136, no new code) |
| 2 | `docker build` of the API image, also with `--no-cache` | succeeds; no NU19xx audit warning |
| 3 | `docker compose up --build -d` | `db` healthy, `api` and `client` up |
| 4 | Migrations on the empty compose database | 3 migrations in `__EFMigrationsHistory` (ProductVersion 10.0.13); Criteria 127, CriteriaFilters 27, Products 17 (as in S1-05) |
| 5 | `GET /health`, `GET /swagger/index.html` | 200, 200 |
| 6 | `POST /api/signUp` (throwaway address), `POST /api/signIn` correct / wrong password | 200 / 200 with tokens / 400 |
| 7 | CORS preflight `POST /api/signIn` from `http://localhost:4200`, `https://procureaccess.tech`, `http://evil.example` | allowed, allowed, no `Access-Control-Allow-Origin` header |
| 8 | Anonymous GET endpoints, **.NET 8 (commit `c255e4a`) vs. .NET 10**, same database | Same status codes for all 9 endpoints; identical bodies for all 200 responses (CriteriaFilters, FilterTypes, Criteria, Products, Criteria by filter ids ×2) |
| 9 | Client `http://localhost:4200/` and deep link `/filters` | 200, 200 |
| 10 | `reuse lint` | compliant |

## 4. Manual smoke test (browser)

Start the stack (once: `docker network create dokploy-network`):

```bash
docker compose up --build        # or: docker-compose up --build
# client http://localhost:4200 · API http://localhost:5000 · Swagger http://localhost:5000/swagger · health http://localhost:5000/health
docker compose down              # stop; add -v to drop the database volume
```

Note: in `Development` the API clears and re-seeds the database on every start (`RebuildDatabase`), so accounts from earlier runs are gone.

| # | Step | Expected | Result |
|---|---|---|---|
| 1 | `docker compose ps`; `docker compose logs api` | db healthy, api + client up; no exception after startup (see §5 for known log lines) | |
| 2 | Open `http://localhost:4200`, DevTools console open | Home page loads; no CORS error in the console | |
| 3 | Register a new account (any address, e.g. `smoke@example.test`) | Account is created; a mail error may be shown (no SMTP locally, §5) | |
| 4 | Sign in with that account; reload the page; sign out; sign in again | Signed-in state in the header; survives reload; sign-out works | |
| 5 | Sign in with a wrong password | Error message, no sign-in | |
| 6 | Criteria filter (`/filters`): select filters of different types | Criteria list changes with the selection | |
| 7 | Products (`/products`) with a filter selected | As before the upgrade — known 500 from the API, §5 | |
| 8 | Export button on the products page | As before the upgrade — note what is downloaded | |
| 9 | `docker compose restart api`, reload the client | API starts again; the app works | |

## 5. Known behaviour that is not caused by the upgrade

| # | Observation | Evidence | Proposed follow-up |
|---|---|---|---|
| K1 | `GET /api/Products/byCriteriaFilterIds` returns **500** (`System.Text.Json` cannot serialize a `Task`): `ProductsController.ByCriteriaFilterIds` does not `await` the service call | Same 500 on .NET 8 (commit `c255e4a`) | Issue; fix is one `await`. Not changed here because S1-06 must keep behaviour unchanged |
| K2 | Products export link points at `./` and downloads the page, not product data | `products-container.ts`, `products-list.ts` | Replaced by the export pipeline in step 2 |
| K3 | API log: `Cannot load library libgssapi_krb5.so.2` at startup | Library is missing in `aspnet:8.0` as well; Npgsql falls back to no GSS encryption | None needed |
| K4 | API log: `ERR … Failed executing DbCommand … __EFMigrationsHistory` on an empty database | EF probes the history table before creating it (same as S1-05) | None |
| K5 | Sign-up sends the confirmation mail to a fixed test address via Lettermint; without SMTP credentials the mail fails | `UserService.SignUpAsync` | Mailpit and real confirmation in M3 |
| K6 | The API crashes on startup with `ASPNETCORE_ENVIRONMENT=Production` (NullReferenceException in `ConfigureSerilog`, `AppLoggingSettings` missing in `appsettings.Production.json`) | Same on .NET 8 (commit `c255e4a`) and on the new .NET 10 image | Issue; logging is reworked in step 1 (S1-22) |
| K7 | Client container: nginx forwards `/api/` to `api:5000`, but the API listens on 8080. Unused, because the client calls `http://localhost:5000/api` directly | `CLIENT/CLIENT.ProcureAccess/nginx.conf` | S1-11 |
| K8 | `docker-compose.yml` sets `ASPNETCORE_ENVIRONMENT: Development` (and therefore `RebuildDatabase=true`) unconditionally | `docker-compose.yml` | SEC-06 guard in step 1 (S1-22); check the Dokploy deployment |

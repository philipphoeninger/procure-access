<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S7-09 — Deployment hardening: ports, public-address settings, health route

> **Sprint:** 07, task S7-09 (P0; ROADMAP GP.1, repository part). **Issues:** #449, #447, #448 (`TECH_DEBT_v6.md` §3.2; TD-28 points 11 – 13 and 15). **Requirements / invariants:** R-ADM-04; OPS-01, OPS-02 and SEC-06 stay green; no invariant activated, no new TESTING version (as S6-07: operations checks without an invariant).
> **Status:** done by Claude Code (2026-11-26); merged with pull request #464 (2026-11-26), which closed #449, #447 and #448. **E2E left to CI:** another session used the local E2E stack, so it was not started. The nginx change and the new proxy check were run locally against the real `nginx.conf` in `nginx:1.30-alpine` with a stand-in API (§4); their first run on the real stack was CI stage 6, green on all five browser projects (§4). No EF migration, no new dependency, no UI string.
> **Scope:** exactly the sprint row: API and client ports on the loopback (#449); the five public-address settings passed by compose, with a start-up check (#447); `/health/live` and `/health/ready` through the client's nginx (#448); a check that `/api/` answers keep their strong ETags (TD-28 point 15); `TECHNICAL.md` §3, §4, §6 and `.env.example`.
> **Owner decisions (O-S7-5, 2026-11-26):** **#449** — loopback in the base compose file. **#447** — compose passes all five settings; the API refuses the `appsettings.json` default outside `Development` and `E2E` for **`App:BaseUrl` and `Email:From` only**; `Export:ToolUrl` and `ProblemTypes:BaseUri` are not refused, because their default `https://procureaccess.de` is the 🔧 value of SPEC §8.6 and may be a deployment's own. **#448** — `nginx.conf`, exactly the two paths. **Ready limit** — no limit per caller in this pull request; open point 1.

## 1. Starting point

- `docker-compose.yml` published `5000:8080` and `4200:80` without a host address, so on every interface; only the database was on `127.0.0.1`. Docker's port publishing bypasses host firewalls, and `ClientAddress` trusts `X-Forwarded-For` from private networks, which a direct caller behind Docker's userland proxy appears to come from.
- Compose passed none of `App:BaseUrl`, `Email:From`, `Email:DisplayName`, `Export:ToolUrl`, `ProblemTypes:BaseUri`; only the dev, E2E and rehearsal overrides set `App__BaseUrl`. A staging deployment would have mailed links to `http://procureaccess.tech:4200`.
- `App:BaseUrl` was validated only when the first e-mail was built (`ClientLinks.To`); the other three addresses were validated at the start, but only for their form.
- The client's nginx forwarded only `/api/`. The API's health answers already carry nothing but `Healthy` or `Unhealthy`. #112 had kept `/health/ready` off the public origin on purpose; O-S7-5 ends that.
- `/api/` already had `gzip off` with its reason (S6-20a), but nothing checked it, and nothing could check a proxy in front (S6-20a R6).
- The integration host runs as `Testing`, so every start-up rule "outside Development and E2E" also applies to it.

## 2. Changes

| File | Change |
|---|---|
| `docker-compose.yml` | API and client ports as `127.0.0.1:5000:8080` and `127.0.0.1:4200:80`; `api.environment` gains `App__BaseUrl`, `Email__From`, `Email__DisplayName`, `Export__ToolUrl`, `ProblemTypes__BaseUri` from `APP_BASE_URL`, `EMAIL_FROM`, `EMAIL_DISPLAY_NAME`, `EXPORT_TOOL_URL`, `PROBLEM_TYPES_BASE_URI`, each defaulting to the value of `appsettings.json` |
| `docker-compose.e2e.yml` | The client's health check asks its own nginx (`http://127.0.0.1/health/ready`) instead of the API directly, so `--wait` covers the new route; the comment on the ports |
| `.env.example` | The five variables with their meaning, commented out (the local stacks need none) |
| `CLIENT/CLIENT.ProcureAccess/nginx.conf` | `location ~ ^/health/(live\|ready)$`, proxied like `/api/` (same upstream, headers, `gzip off`); nothing else under `/health/` is forwarded |
| `src/…IdentityAccess/Auth/ClientLinks.cs`, `IdentityAccessModule.cs` | `EnsureDeploymentAddress`: `App:BaseUrl` must be an absolute http(s) address (every environment) and, outside `Development` and `E2E`, not `DefaultBaseUrl`; called first in `AddIdentityAccessModule` |
| `src/…Platform/Email/EmailSettings.cs` | Outside `Development` and `E2E` the address of `Email:From` must not be `DefaultSenderAddress` |
| `scripts/check-client-proxy.mjs`, `.test.mjs` | New: health route (both paths `200 Healthy` and nothing else; `/health/details` not forwarded) and ETags (an `/api/` answer of at least 1024 bytes asked with `Accept-Encoding: gzip` stays uncompressed, with a strong ETag equal to the API's) |
| `.github/workflows/ci.yml` | Stage 1 runs the self-test; stage 6 runs the check once on the E2E stack (`http://localhost:4200 http://localhost:5000`) |
| `tests/…Unit/IdentityAccess/ClientLinksTests.cs`, `…Unit/Email/EmailSettingsTests.cs`, `…Unit/Operations/PublicAddressDefaultsTests.cs` | The two rules; and that the defaults of `docker-compose.yml`, `appsettings.json` and the two constants are equal and `.env.example` names each variable |
| `tests/…Integration/Operations/PublicAddressStartupTests.cs` | The real host stops in `Production` and `Staging` with either default and starts in `Development` |
| `tests/…Integration/Infrastructure/ProcureAccessApiFactory.cs`, `RegistrationTests.cs`, `PasswordResetTests.cs` | The test host sets `App:BaseUrl` (`ClientBaseUrl`) and `Email:From`; three link assertions compare with `ClientBaseUrl` instead of the old default |
| `docs/TECHNICAL.md` | §3.1 the five variables and the start-up rule; §4.2 steps 2 and 6; §4.3 ports, the proxy's route to the client, compression and the check; §6.1 the public health route; §6.3 the gap of open point 1 |
| `docs/ARCHITECTURE_v108.md` (v106 deleted) | C196; §9.5, §9.7, §10.1 |
| `docs/CODEBASE_v1.md`, `CLAUDE.md` | §2.0, §2.119, §3, §4; `CLAUDE.md` §3 (🔧 settings: a deployment setting needs its compose line) and §4 |

**Why the defaults stay in three places.** Compose substitutes an empty value for an unset variable, and an empty environment variable overrides `appsettings.json`, so compose needs a default of its own; the API compares with a constant. `PublicAddressDefaultsTests` fails when the three differ.

**The commands.** `roles` registers IdentityAccess and so goes through the check of `App:BaseUrl`; `catalog` does not. Both get the variables from compose (`docker compose run --rm api …`).

**Deviations from the plan:** (1) `App:BaseUrl` must also be http(s): on Linux a value such as `/app` parsed as an absolute `file:` address and would have passed the new check and built `file:` links. (2) One regex location instead of two exact ones in `nginx.conf`: the same two paths, one copy of the proxy lines. (3) No TESTING version: no invariant changes status and the new tests carry none (as S6-07).

## 3. Self-review (CLAUDE §6.7, §6.8; `/engineering:code-review`)

| # | Finding | Sorting |
|---|---|---|
| R1 | `/app` as `App:BaseUrl` passed on Linux (a `file:` address) — found by the new unit test | **Fixed:** http(s) required, as `Export:ToolUrl` and `ProblemTypes:BaseUri` already do |
| R2 | Two doc comments named the wrong test for "keeps it equal to the file" | **Fixed** |
| R3 | Draft of `TECHNICAL.md` §3.1 said both one-off commands go through the checks; only `roles` does | **Fixed** |
| R4 | **`noreply@procureaccess.tech` may be the real sender of staging** (the default names the `.tech` domain). If it is, the API refuses to start there with the right value | **Owner:** open point 2, to settle before S7-10 |
| R5 | The host ports no longer listen on `::1`. A client that resolves `localhost` to IPv6 only would fail; Node (24), curl and the browsers fall back to `127.0.0.1` | **Won't fix now:** the database port is bound the same way; CI stage 6 on all three engines is the proof, and it was green (§4; open point 3 settled) |
| R6 | The public ready check shares its limit of 2 with every caller | **Follow-up:** #469 (open point 1, owner decision) |
| R7 | The proxy lines of `/api/` are repeated for the health location | **Won't fix:** an `include` would add a second file to the image for four lines |
| R8 | `EnsureDeploymentAddress` uses `KeyRingCertificates.MayStoreKeysUnencrypted` for "Development or E2E" | **Won't fix:** one place per module for that rule; a rename is style |
| R9 | `Export:ToolUrl` is not refused at its default, so a staging export links to `https://procureaccess.de` unless set | **As decided** (O-S7-5); `TECHNICAL.md` §3.1 says to check it |

Checked without a finding: no message of a refused start quotes a value; no secret or real address in the repository (`.env.example` uses `procureaccess.example`); the health answers carry only the status word and `Cache-Control: no-store, no-cache`; the rehearsal stack (Development, own port override) and the dev and E2E overrides render as before (`docker compose config`); the regex location does not shadow `/api/` (`^~`) or the hashed-file rule. Mutation check of the proxy check: the same `nginx.conf` with `gzip on` in the proxied locations fails with "Content-Encoding gzip" and "weak ETag".

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` (CI stage 2) | passed, 0 warnings |
| `dotnet format ProcureAccess.sln --verify-no-changes` | passed |
| Unit tests | 904 of 904 passed |
| Architecture tests | 67 of 67 passed |
| Integration tests (Testcontainers, PostgreSQL 17) | 1245 of 1245 passed (whole solution in one run: 2216 of 2216) |
| `node --test scripts/*.test.mjs` | 153 of 153 passed (7 new) |
| `npm run lint`, `npm run typecheck` in `e2e/` (covers `scripts/*.mjs`) | passed |
| `docker compose config` (base; base + E2E; with `APP_BASE_URL` set) | the five settings and `127.0.0.1` for 5000, 4200, 5432 as expected |
| **nginx, locally:** the real `nginx.conf` in `nginx:1.30-alpine` with a stand-in API named `api` on a private Docker network | `nginx -t` ok; `check-client-proxy.mjs` passed; `/health/live`, `/health/ready` forwarded; `/health/`, `/health/details`, `/health/ready/x` answered by the client; fails as expected with `gzip on` |
| Sample answers of the proxy check on `T.1` (measured once with a throwaway integration test, not committed) | `filter-options` 3995 bytes, `criteria` 6373 bytes, strong ETag: above nginx's 1024-byte threshold, so the check proves something on the E2E stack |
| `check-invariants`, `check-figma-links`, `check-i18n` | passed |
| Licence headers | `reuse lint-file` on the new and changed files passed; the full `reuse lint` runs in CI (it cannot read `.gitignore` in a worktree) |
| Vitest, client lint | not run: no client source changed (`nginx.conf` only) |
| **E2E** | **not run locally, left to CI** (another session used the local E2E stack). CI stage 6 is the first run of the loopback ports, the health check through nginx and `check-client-proxy.mjs` on the real stack |
| After the merge of `main` into the branch (S7-02, S7-06, S7-15; conflicts only in `CLAUDE.md`, `CODEBASE_v1.md`, `TECHNICAL.md`) | build and format passed; backend tests 2261 of 2261; `node --test scripts/*.test.mjs` 160 of 160; `check-invariants`, `check-figma-links`, `check-i18n` passed |
| **CI on pull request #464** (run 45842910522 on the merged state; the run before the merge of `main` was green as well) | stages 1 – 8 passed: lint and licences (full `reuse lint`), build, unit (.NET and Angular), architecture, integration, **E2E + a11y on chromium, firefox, webkit, mobile-chrome and mobile-safari**, invariant gate, coverage report; CodeQL (actions, csharp, javascript-typescript) passed |
| **CI stage 6, the new parts** | the E2E stack came up with the ports on `127.0.0.1` and the client's health check through nginx (`--wait`); `check-client-proxy.mjs http://localhost:4200 http://localhost:5000` answered "health route and /api/ ETags of http://localhost:4200 are as required" on the real stack; every engine reached `localhost:4200` |

## 5. Open points

1. **The public `/health/ready` has no limit per caller.** The API runs at most 🔧 2 ready checks at a time for all callers and answers 429 beyond, so somebody who asks often can make a monitor see 429 (not an outage of the tool). Options: (a) an nginx `limit_req` per address for the two paths (a new 🔧 value); (b) the monitor asks `/health/live` from outside and `/health/ready` only from the host; (c) leave it. **Issue #469**, to decide with the alerting (S7-11, TD-92). `TECHNICAL.md` §6.1 tells a monitor to treat 429 as "ask again".
2. **Owner, before S7-10: is `noreply@procureaccess.tech` the real sender of staging?** The check refuses exactly this address outside `Development`. If staging sends from it, either (a) the default in `appsettings.json` and compose becomes a placeholder that is nobody's address (e.g. `noreply@procureaccess.invalid`), so the check keeps its meaning, or (b) `Email:From` leaves the check. Recommendation: (a). The same question for `App:BaseUrl` does not arise: `http://…:4200` is no deployment's address.
3. **Settled by CI (2026-11-26): `localhost` on IPv6-only resolvers.** All five browser projects reached `localhost:4200` in CI stage 6, so nothing is to do. The question was: if an E2E engine in CI cannot reach `localhost:4200` after this change, bind `[::1]` as well or let the E2E override publish as before; not expected (R5).
4. **Staging values (S7-10, owner):** `APP_BASE_URL`, `EMAIL_FROM` (SPF and DKIM at Lettermint for its domain), `EXPORT_TOOL_URL`; whether the Dokploy proxy reaches the client over `dokploy-network` (it must, since port 4200 is no longer public) and does not compress `/api/` — `node scripts/check-client-proxy.mjs https://<staging address>` answers the second.
5. **Documents for the next upkeep:** the headers of SPEC, TESTING, the Handoff and the Figma mapping still name `ARCHITECTURE_v107.md`; `SPRINT_07.md` has no column for the decision O-S7-5 (recorded here and in C196); TECH_DEBT v7 can close #449, #447, #448 and points 11 – 13 of TD-28, and reword point 15 (checked in the image; the proxy in front is checked by the script at the deployment).
6. **E2E environment of its own** (TESTING §3.3): the E2E stack still runs as `Development`, so it never exercises the refusal; the integration tests do.

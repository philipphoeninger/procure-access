<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-30 — `TECHNICAL.md` and README

> **Sprint:** 06, task S6-30 (stretch; ROADMAP GP.6, process step 16). **Issues:** #4. **Requirements / invariants:** R-MET-02; no invariant activated (TESTING has none for R-MET-02), no new TESTING version.
> **Status:** done by Claude Code (2026-11-24); pull request open, rebased on `main` after S6-09 (#445) was merged. **E2E left to CI:** another session used the local E2E stack, so it was not started; this task changes no screen and no E2E spec. No EF migration, no new dependency, no UI string.
> **Scope:** exactly the sprint row: `docs/TECHNICAL.md` for the next developer and for operations (architecture in short with pointers, local setup, configuration and secrets, deployment, backup and restore, logs and health, the metric queries of R-MET-02, the `catalog` and `roles` commands) and a README for the public repository (#4). ARCHITECTURE, SPEC, TESTING and the Handoff are not edited: S6-09 updates them at the same time, so `TECHNICAL.md` points to their sections instead of copying them.
> **Owner decisions (2026-11-24):** **D1 (a)** — one integration test reads the metric queries from `TECHNICAL.md` and runs them against known rows. **D2 (a)** — the two indicators whose tables do not exist yet (published against private workspace entries, growth of Kanal 1 and Kanal 2) are listed as "follows with step 6"; their queries and test cases come with that step. **D3 (a)** — `CLAUDE.md` gets the row `TECHNICAL.md` in its document table, paid for by task IDs dropped from the state paragraph of §3; no rule changes. *After the rebase:* S6-09 had added the row and moved the state paragraph to `CODEBASE §2.0`, so only the row's description changed and nothing was shortened.

## 1. Starting point

- `docs/TECHNICAL.md` existed since S6-29 with only the backup and restore chapter (§1, including the key ring from S4-26 §6). The README had a title and the licence section.
- TECH_DEBT_v5 #4 lists what the document must cover: the deployment variables; key store, backup, restore and rotation (done by S6-29); `CREATE` on `public` for the ICU collation (S4-25 P3); the SDK pin (S5-03 P3, P4); `T.1` against a real release; the SMTP variables and `App__BaseUrl` (S5-11); `SAMPLE_USER_PASSWORD` of at least 12 characters; the disk space E2E needs (TD-74); the session warning under `npm start` (TD-64, fixed by S6-02 since).
- SPEC §9.2 names eight indicators. Five have their tables: `analytics."UsageEvents"` (two indicators), `configurator."CriteriaConfigurations"` and `."ConfigurationCriteria"`, `workspace."CandidateProducts"`. Two need `WorkspacePublication` and `PublishedEntry` (step 6); pilot procurements are counted by hand.
- **Found while checking the facts against the code** (open points 1–4): compose passes neither `App__BaseUrl` nor the other settings that name a deployment's public address; the client's nginx forwards only `/api/`, so the health endpoints are not reachable through the public address; compose publishes the API (5000) and the client (4200) on every host interface; a revoked platform role stays in a running session until it ends.
- `CLAUDE.md` had 39 957 of 40 000 characters when the task started; S6-09 brought it to about 37 500 in parallel.

## 2. Changes

| File | Change |
|---|---|
| `docs/TECHNICAL.md` | Restructured into nine chapters: §1 architecture in short, §2 local setup, §3 configuration and secrets, §4 deployment, §5 catalog releases and platform roles, §6 logs and health, **§7 backup and restore (the S6-29 chapter, moved from §1 unchanged, only renumbered)**, §8 usage indicators with five queries, §9 where to read more |
| `README.md` | What ProcureAccess is, status, quick start, technology, documents, contributing; the licence section unchanged |
| `tests/ProcureAccess.Tests.Integration/Operations/MetricQueryTests.cs` | New (D1 (a)): reads every `sql` block of `TECHNICAL.md` that starts with `-- metric: <name>`, checks the set of names, runs each in a read-only transaction against known rows and compares the result; refuses a `uuid` column |
| `tests/ProcureAccess.Tests.Integration/ProcureAccess.Tests.Integration.csproj` | Links `docs/TECHNICAL.md` into the test output (`Docs/TECHNICAL.md`) |
| `CLAUDE.md` | §2: the description of the row `TECHNICAL.md` (added by S6-09) now names every chapter; 37 537 characters |
| `docs/CODEBASE_v1.md` | §1 change history; §2.0 entry and §2.117 new; §2.26 and §2.115 point to `TECHNICAL.md` §7.6 and §7; §3 layout (`README.md`, `TECHNICAL.md`, the test); §4: the references `§1.4`/`§1.5` → `§7.4`/`§7.5`, and the `roles` line corrected (a change applies at the next sign-in, not "the next token ≤ 30 min", which described the JWT of before step 3) |

**What `TECHNICAL.md` covers of #4:** deployment variables and the settings compose does not pass (§3.1); key ring, backup, restore, rotation and the rehearsal (§7, S6-29); the collation privilege (§4.3); the SDK pin and its install (§2.1); `T.1` against `2026.1` (§5.1); SMTP and `App__BaseUrl` (§3.1); `SAMPLE_USER_PASSWORD` (§2.2, §3.1); disk space (§2.1); the session warning under `npm start` (§2.3). **Not in it, on purpose:** the manual `curl` checks and the full command list stay in `CODEBASE §4` and `CLAUDE.md §5`, to which §2 points.

**The metric queries (§8):** `completed-configurations` and `exports` count the usage events per UTC month and dimension; `saved-lists` counts the lists without `ProcurementProjectId` (a project's copy is not a saved list); `workspace-entries` counts the candidates; `priority-overrides` counts per criterion key in how many saved lists it is, how often its level was changed, a weight set, or it was excluded. They run as the operator through `psql` with `default_transaction_read_only` (the command checked on a throwaway `postgres:17-alpine`: a `CREATE TABLE` in it is refused). While writing the test, a first version compared `OverrideLevel <> ResolvedLevel`; the domain already stores a level equal to the recommendation as none (`ConfigurationCriterion.Decide`), so the query only asks `OverrideLevel IS NOT NULL` and the document says why.

**Deviations from the plan:** `CLAUDE.md` was not shortened in the end. Before the rebase this branch dropped task IDs from the state paragraph to fit the row; S6-09 had moved the whole paragraph to `CODEBASE §2.0` and added a row of its own, so the rebase kept `main`'s version and only updated the row.

## 3. Self-review (CLAUDE §6.7, §6.8; `/engineering:code-review`)

Every statement of `TECHNICAL.md` and the README about behaviour was checked against the code or configuration it describes: compose files, `appsettings*.json`, `.env.example`, the client's `nginx.conf` and `environment.prod.ts`, the API Dockerfile, `SeedUsers`, `DatabaseRebuildExtensions`, `RoleCommandLine`, `HealthChecks`, `LoggingRegistration`, `ClientAddress`, the model snapshots.

| # | Finding | Sorting |
|---|---|---|
| R1 | Draft: "the client's nginx forwards `/api/` and `/health`" — it forwards only `/api/` | **Fixed** (§1, §4.2, §6.1); the gap itself is open point 2 |
| R2 | Draft: the log query named the column `"Timestamp"`; it is `"TimeStamp"` | **Fixed** |
| R3 | Draft: "bind the ports to `127.0.0.1` in an override" — a compose override appends to `ports`, so the public binding would stay | **Fixed:** §4.3 shows `ports: !override [...]`; checked with `docker compose config` |
| R4 | `priority-overrides` compared `OverrideLevel <> ResolvedLevel`, a condition the stored data can never meet (the test passed with it removed) | **Fixed:** simplified, explained in §8.2 |
| R5 | `MetricQueryTests` seeds once per theory row (six times) | **Won't fix:** about 2 s per row; one row per query keeps a failure readable |
| R6 | The test compares rows without order, so a changed `ORDER BY` goes unnoticed | **Won't fix:** the order is presentation; how the database collation sorts criterion keys would make an ordered comparison environment-dependent |

Checked without a finding: the queries return only counts, months, dimension values and criterion keys, never an id, an organisation, a user or a product name; the test runs them in a read-only transaction, so a query that writes fails; the SQL the test runs comes from the repository, not from input; no secret or personal data in the documents (the README keeps the existing licence text). Mutation check of the test: a saved-list query without `IS NULL`, a priority query that counts the project copy, and an extra column in the monthly queries each fail.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` (CI stage 2) | passed, 0 warnings |
| `MetricQueryTests` | 6 of 6 passed; fail as expected on three mutated queries |
| Integration tests (whole project) | 1240 of 1240 passed (Testcontainers, PostgreSQL 17) |
| After the rebase on S6-09 | build (0 warnings), `MetricQueryTests` 6 of 6, `check-invariants`, `check-figma-links`, `check-i18n` and `reuse lint-file` passed again; the whole integration project ran before the rebase (S6-09 ran it in its own pull request) |
| `dotnet format --verify-no-changes` on the new test | passed |
| `check-invariants`, `check-figma-links`, `check-i18n` | passed |
| Licence headers | `reuse lint-file` on the new and changed files passed; the full `reuse lint` runs in CI, because it cannot read `.gitignore` in a worktree |
| Unit, architecture, Vitest, lint | not run: no production code, client or script changed |
| **E2E** | **not run locally, left to CI** (another session used the local E2E stack); nothing E2E covers changed |

## 5. Open points

1. **Compose does not pass the public-address settings** (`App:BaseUrl`, `Email:From`, `Email:DisplayName`, `Export:ToolUrl`, `ProblemTypes:BaseUri`). On staging the e-mail links would point to the default `http://procureaccess.tech:4200`. `TECHNICAL.md §3.1` documents the override; **proposed issue** (with TD-28 / #236, before mail goes out from staging): add them to `api.environment` with variables in `.env.example`, e.g. `APP_BASE_URL`. The values are the owner's.
2. **Health endpoints not public:** the client's nginx forwards only `/api/`, so an uptime monitor outside the host cannot reach `/health/ready`. Either route `/health` in the reverse proxy or in `nginx.conf`. Proposed for GP.1 together with the alert of S6-29 open point 3.
3. **API and client ports on every interface:** `docker-compose.yml` publishes 5000 and 4200 without a host address; Docker bypasses host firewalls such as ufw. On a server the API port would serve plain HTTP and, behind Docker's userland proxy, could let a caller choose its rate-limit counter (`ClientAddress`). `TECHNICAL.md §4.3` says how to close them. **Proposed issue** for GP.1 / TD-28: a production override (or loopback binding in the base file, with the local stacks unaffected).
4. **A revoked platform role stays in the running session** until it ends (at most 8 hours): `roles revoke` does not update the security stamp. Documented in §5.2. **Proposed follow-up** before moderation goes live (step 7): update the stamp on revoke, so the next request ends the session (S5-29 checks the stamp on every request).
5. **For the next documentation session (no edits here; checked against `main` after S6-09):** ARCHITECTURE §9.1 still says „`TECHNICAL.md` documents the key store (until then S4-26 §6)“ — it does, in §7.6; TESTING could name an operations invariant for R-MET-02 that `MetricQueryTests` would carry; SPEC §9.2 could point to `TECHNICAL.md §8`; `CODEBASE §2.81` still says the session warning can be checked only on the E2E stack (fixed by S6-02). S6-09 already dropped the stale „Next: migration step 1“ of `CLAUDE.md`.
6. **README, for the owner to confirm:** it invites bug reports and questions as GitHub issues and promises nothing else about contributions. There is no `SECURITY.md` for reporting vulnerabilities privately; proposed before the public release (G-R).
7. **Sprint close-out:** #4 is done with this pull request (`Closes #4`); TECH_DEBT v6 can mark it resolved. The staging run of the restore rehearsal stays with GP.1 (S6-29 open point 1).
8. **When step 6 is built:** add the queries for published against private entries and for the Kanal growth to `TECHNICAL.md §8.2`, and their names and expected rows to `MetricQueryTests` (D2 (a)).

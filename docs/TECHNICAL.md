# ProcureAccess — Technical documentation

> **For whom:** the next developer and whoever runs a deployment. It explains how the parts fit together, how to run them, how to configure, deploy, back up and restore them, and how to read the usage indicators. It does not repeat the design: every chapter points to the section of the document that decides it.
> **Status:** chapter 7 (backup and restore) since S6-29 (2026-11-23); chapters 1–6, 8 and 9 since S6-30 (2026-11-24; process step 16, ROADMAP GP.6, issue #4). The protocol of the task is `docs/sprints/sprint_06/S6-30_technical_readme.md`.
> **Kept current by:** the task that changes what a chapter describes, in the same pull request — a new setting, a new compose service, a new command, a renamed table of §8 (its test fails otherwise, §8.1). The file has no version in its name, so cite it by chapter (`TECHNICAL.md §7.4`).

---

## Contents

1. Architecture in short
2. Local setup
3. Configuration and secrets
4. Deployment
5. Catalog releases and platform roles
6. Logs and health
7. Backup and restore
8. Usage indicators (R-MET-02)
9. Where to read more

---

## 1. Architecture in short

ProcureAccess is a **modular monolith**: one ASP.NET Core API, one Angular single-page application, one PostgreSQL database. The decisions behind this shape are in `ARCHITECTURE §1`–`§4` and `§6`; the reasons for keeping it simple are the organisational constraints of `ARCHITECTURE §2.2` (one maintainer, handover after the research project).

| Part | Where | What to read |
|---|---|---|
| API host | `API/API.ProcureAccess/` — `Program.cs`, the command modes (`catalog …`, `roles …`), the seeders of the local stacks, the legacy controllers | `CODEBASE §3` |
| Modules | `src/ProcureAccess.Modules.<Name>/` and `.Contracts/`: Platform, Catalog, IdentityAccess, Configurator, Workspace, Export; Directory and Moderation are still empty | `ARCHITECTURE §4.3` (responsibilities and allowed references); `CODEBASE §2` (what each task built) |
| Legacy projects | `DAL/`, `SERVICES/`, `API/…/Controllers` | Retired step by step along `ARCHITECTURE §12` |
| Client | `CLIENT/CLIENT.ProcureAccess/` — Angular, zoneless, NgRx Signals, Angular Material | `ARCHITECTURE §8`; `design_handoff_procureaccess_v1.md` |
| Database | PostgreSQL 17; one schema per module (`platform`, `logging`, `analytics`, `catalog`, `identity`, `configurator`, `workspace`) plus the legacy tables in `public` | `ARCHITECTURE §5` (domain model), `§10.5` (one context and migration history per module, ADR-014) |
| Tests | `tests/` (.NET: unit, architecture, integration with Testcontainers), the client's Vitest specs, `e2e/` (Playwright with axe) | `TESTING §2`, `§3`; the invariant gate `TESTING §4` |
| CI | `.github/workflows/ci.yml` (stages 1–8), `codeql.yml` | `TESTING §8.2` |

**At runtime** three containers serve the application, as `ARCHITECTURE §10.1` draws it: the client image (nginx with the built SPA, which forwards `/api/` to the API), the API image, and PostgreSQL. On a deployment a reverse proxy (Traefik on the current hosting) terminates TLS in front of the client. The API sends mail through an SMTP relay (Lettermint) and writes its logs to the console and to the database. Nothing in the browser calls a third party (`CLAUDE.md §8`).

**The rules that hold the shape together** — module references only through `Contracts`, one `DbContext` per module, tenant filters, the write paths of the directory — are enforced by tests; they are listed in `CLAUDE.md §7` and explained in `ARCHITECTURE §4.3` and `§9`.

---

## 2. Local setup

### 2.1 Prerequisites

| Tool | Version | Why |
|---|---|---|
| Docker with Compose v2 | current | The local stacks, Testcontainers for the integration tests, the backup scripts |
| .NET SDK | **exactly the feature band of `global.json`** (10.0.404, later patches allowed) | `dotnet` refuses another band. Install beside a system SDK: `curl -sSL https://dot.net/v1/dotnet-install.sh \| bash -s -- --version 10.0.404 --install-dir ~/.dotnet`, then `export PATH=$HOME/.dotnet:$PATH DOTNET_ROOT=$HOME/.dotnet`. After an SDK update, regenerate the API's licence notices (`node scripts/check-api-notices.mjs --write`) |
| Node.js | **the version of `.nvmrc`** in the client and in `e2e/` | The Angular CLI refuses older versions; `nvm use` in either folder |
| Optional | `reuse` (`pipx install reuse`), `openssl` | Licence check (`CLAUDE.md §5`); the key-ring certificate (§7.6) and the restore rehearsal (§7.5) |

**Disk space.** A full stack build leaves about 2 GB of image layers behind each time (`docker image prune -f` removes the dangling ones); the Playwright browsers take about 2.5 GB in `~/.cache/ms-playwright`. Plan for **at least 10 GB free** before running the E2E stack: with less, image builds fail half-way and E2E results stop meaning anything (TECH_DEBT TD-74). WebKit needs system libraries most Linux desktops lack; CI runs it (§2.4).

### 2.2 The development stack

```bash
cp .env.example .env                      # once; fill in DB_PASSWORD and SAMPLE_USER_PASSWORD (§3.1)
docker network create dokploy-network     # once; the compose file expects this external network
docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build
```

| Address | What |
|---|---|
| http://localhost:4200 | The client (the image, served by nginx) |
| http://localhost:5000/swagger | API documentation, only in `Development` |
| http://localhost:5000/health/ready | 200 when the API reaches the database |
| http://localhost:8025 | Mailpit: every e-mail the API sends (registration, reset, invitations) |

`docker-compose.dev.yml` adds Mailpit and activates the **test catalog release `T.1`** (§5.1) from `tests/ProcureAccess.Tests.Shared/Fixtures/Releases`. In `Development` the API **rebuilds the sample data at every start** (`RebuildDatabase` in `appsettings.Development.json`): it deletes the users and seeds `sample.user@example.org` with `SAMPLE_USER_PASSWORD`. That password must have at least 12 characters (`Account:Password:MinLength`), or the rebuild stops the start; leave it empty for no sample user. After a change to the `T.1` folder, `docker compose … down -v` drops the volume so it is seeded again.

Without `docker-compose.dev.yml`, no release is active, mail goes to the real relay (and fails without SMTP credentials), and the client falls back to the legacy criteria filter.

### 2.3 Running API and client from the source

For debugging, run the database (and Mailpit) in Docker and the rest from the source: the API from `API/API.ProcureAccess` with `dotnet run`, the client from `CLIENT/CLIENT.ProcureAccess` with `npm install && npm start` (http://localhost:4200, its dev server forwards `/api` to `localhost:5000`). The exact commands, the environment variables `dotnet run` needs (connection string, Mailpit) and the manual API checks with `curl` are in `CLAUDE.md §5` and `CODEBASE §4`.

The session warning (`SessionWarningService`) works under `npm start` too since S6-02 (TD-64); to see it without waiting 25 minutes, shorten `Session:IdleTimeout` for the API you run.

### 2.4 Tests and checks

The commands of every test level and of every CI check are in `CLAUDE.md §5` (everyday) and `CODEBASE §4` (complete); CI runs them on every pull request (`TESTING §8.2`). Three things a newcomer trips over:

- **Integration tests need a running Docker**: Testcontainers starts `postgres:17-alpine`. The EF Core InMemory provider and SQLite are not used, on purpose (`TESTING §3.1`).
- **The E2E stack** (`npm run stack:up` in `e2e/`) uses the same host ports as the development stack: stop one before starting the other. It runs an in-memory database with `T.1`, the E2E users and organisation, Mailpit and relaxed rate limits (`docker-compose.e2e.yml`).
- **Chromium and Firefox run locally; WebKit and mobile Safari in CI** unless the host has WebKit's libraries. CI is the authority for E2E.

---

## 3. Configuration and secrets

The API reads `appsettings.json`, then `appsettings.<Environment>.json`, then environment variables (`Section__Key` for `Section:Key`). The rules — secrets only from the environment, 🔧 values only from configuration, the client without runtime configuration — are `ARCHITECTURE §9.7`; the 🔧 values themselves are confirmed defaults of `SPEC` (SD23 and the rows of the requirements). **The repository is public: no secret is ever committed**; `.env` is git-ignored.

### 3.1 What a deployment sets

`docker-compose.yml` reads `.env` (or the hosting platform's environment) and passes these to the containers:

| Variable (`.env`) | Setting in the API | Required | Meaning |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | – | – | `Production` when unset. `Development` only on a local machine: it rebuilds the sample data (§2.2) and opens Swagger. `RebuildDatabase` never runs outside `Development` (SEC-06) |
| `DB_HOST`, `DB_NAME`, `DB_USER`, `DB_PASSWORD`, `DB_PORT_INNER` | `ConnectionStrings:ProcureAccess` | yes | `DB_PASSWORD` letters and digits only (it is substituted into the connection string); rotating it: `.env.example`. `DB_NAME` also selects a restored database (§7.4) |
| `DB_PORT_OUTER` | – | – | Host port of the database, bound to `127.0.0.1` only |
| `DATA_PROTECTION_CERTIFICATE`, `DATA_PROTECTION_PREVIOUS_CERTIFICATE` | `DataProtection:Certificate`, `:PreviousCertificate` | yes, outside `Development` and `E2E` | Encrypts the key ring; the API does not start without it. Creating, keeping and rotating it: §7.6 |
| `SMTP_USERNAME`, `SMTP_PASSWORD` | `Email:Smtp:Username`, `:Password` | yes, outside `Development` and `E2E` | The relay's API token (Lettermint takes it as both). The API does not start without them |
| `SAMPLE_USER_PASSWORD` | `SampleData:UserPassword` | – | `Development` only, at least 12 characters (§2.2) |
| `CONCURRENCY_ETAG_EPOCH` | `Concurrency:ETagEpoch` | – | 🔧 1. Raised by one after every restore, never lowered (§7.4) |
| `COMPOSE_PROFILES`, `BACKUP_DIR`, `BACKUP_RETENTION_DAYS`, `BACKUP_TIME_UTC` | – (the `backup` service) | on staging and production | §7.2 |

**Settings compose does not pass yet.** Compose hands the API only the variables its `api.environment` lists; a variable set on the platform but not listed there never reaches the container. Four settings of `appsettings.json` name the public address of a deployment and have no entry yet:

| Setting | Default in `appsettings.json` | Used for |
|---|---|---|
| `App:BaseUrl` | `http://procureaccess.tech:4200` | The links in every e-mail (confirmation, reset, invitation). **Must be the deployment's HTTPS origin**, or the links point elsewhere |
| `Email:From`, `Email:DisplayName` | `noreply@procureaccess.tech`, `ProcureAccess` | Sender of the e-mails; the domain needs SPF and DKIM at the relay (`SPEC §7.6`) |
| `Export:ToolUrl` | `https://procureaccess.de` | The link to the tool in exported files |
| `ProblemTypes:BaseUri` | `https://procureaccess.de` | The base of the `type` URI of every error answer (`ARCHITECTURE §7.1`) |

Until compose lists them (issue #447), set them for a deployment in a compose override file next to `docker-compose.yml` (`services: api: environment: App__BaseUrl: https://…`) or in the platform's compose editor. The development and E2E overrides set `App__BaseUrl` to `http://localhost:4200` this way.

### 3.2 Settings that are not secret

Everything else lives in `API/API.ProcureAccess/appsettings.json`, grouped by section and validated at start (a wrong value stops the API with the key in the message): session limits (`Session`), password, token and lockout rules (`Account`), invitations (`Organizations`), rate limits (`RateLimits`, values in `SPEC §7.3`), share links (`ShareLinks`), the weight range (`Configurator`), log level and retention (`AppLoggingSettings`, §6.2), health (`Health`), the proxy hop count (`ForwardedHeaders:ForwardLimit`, §4.3), the SMTP host, port, security and timeout (`Email:Smtp`). A duration — a lifetime, timeout, interval, delay or window, also set through an environment variable such as `Email__Smtp__Timeout` — is written `hh:mm:ss` or `d.hh:mm:ss` (`00:00:15`, `1.00:00:00`); any other form stops the start, also a bare number such as `15`, which .NET would otherwise read as 15 days. A value marked 🔧 in SPEC is a confirmed default: change it in configuration, not in code, and only with the owner's decision (`CLAUDE.md §6.6`).

The client has **no runtime configuration**: its few settings are build-time values in `src/environments/environment*.ts`, the same image everywhere. Some copy a server value and must stay in step with it (`passwordMinLength` ↔ `Account:Password:MinLength`, `lockout` ↔ `Account:Lockout:*`; `CLAUDE.md §3`).

### 3.3 Values that are placeholders until a decision

`node scripts/check-release-config.mjs` refuses a production release while a 🔧 placeholder is set: today the channel of „Kriterium vorschlagen“ in `environment.prod.ts` (open decision O-6) and the legal pages under `CLIENT/CLIENT.ProcureAccess/content/legal/` while they carry their placeholder marker. It is not part of CI; run it before every production release (`ARCHITECTURE §9.7`).

---

## 4. Deployment

### 4.1 Shape

One host, `docker compose`, the three services of §1 plus the nightly `backup` service (§7.2), a reverse proxy with TLS in front — `ARCHITECTURE §10.1` and `§10.2` (environments). Staging is deployed with Dokploy, which runs `docker-compose.yml` and attaches the services to its network `dokploy-network`; that is why the compose file declares the network as external. Another host with Docker Compose works the same way. The current state of the staging deployment and what it still needs is tracked in TECH_DEBT TD-28 (issue #236).

### 4.2 A release step by step

1. CI on `main` is green (`TESTING §8.2`), and for production `node scripts/check-release-config.mjs` passes (§3.3).
2. The `.env` of the host (or the platform's environment) has every required value of §3.1, and the override of §3.1 sets `App__BaseUrl`.
3. `docker compose up -d --build` builds the API and client images from the checkout and starts them. The API image ships the catalog release folders (`catalog/releases/`) and the licence notices (`/app/THIRD-PARTY-NOTICES.txt`); the client image ships `/3rdpartylicenses.txt`. The licences of the base images' OS packages are stated in `docs/BASE_IMAGES.md`; a changed image tag fails CI stage 1 until that file is updated.
4. **The API applies every pending migration at its start**: first the legacy context, then each module (`ARCHITECTURE §10.5`, ADR-014 rule 3). A failing migration stops the API. Before a release that brings a migration, take a backup (`docker compose run --rm backup sh /opt/backup/backup.sh`, §7.3). Migrating at start is safe **only with a single API instance**; the log-retention job (§6.2) and the backup assume one instance as well.
5. If the release brings a new catalog release: import and activate it (§5.1).
6. Check: `/health/ready` answers 200 (§6.1); sign in; the catalog shows the active release.

**Rolling back** a release means starting the previous images. A migration of the new release stays applied: EF migrations are not reverted automatically, and an older API image may not run against a newer schema. When a release with a migration has to be undone, restore the backup taken in step 4 (§7.4).

### 4.3 What the database server and the network must allow

- **The application's database user creates schemas and objects.** The migrations create one schema per module and the ICU collation `public.identity_case_insensitive` (case-insensitive unique contributor names). With the compose stack the user is the server's superuser. On a managed PostgreSQL, the user must own the database or have `CREATE` on it **and** `CREATE` on the schema `public` (`GRANT CREATE ON SCHEMA public TO <user>;` — PostgreSQL 15 and later no longer grant it to everyone). Without it the first migration of IdentityAccess fails.
- **Only the reverse proxy is public.** The database port is bound to `127.0.0.1`. The compose file publishes the API on port 5000 and the client on 4200 on every interface, because the local stacks need them; on a server, keep both closed to the internet (a host firewall does not help: Docker's port publishing bypasses tools such as ufw — bind them to `127.0.0.1` in the host's override, which must replace the list rather than add to it — `ports: !override ["127.0.0.1:5000:8080"]`, as `docker-compose.e2e.yml` does with `!reset` — or block them at the provider; issue #449). The API trusts `X-Forwarded-For` only from private networks and for at most `ForwardedHeaders:ForwardLimit` hops (🔧 2: Traefik → nginx → API); a caller that reaches the API port directly could choose its own rate-limit counter.
- **The proxy must not rewrite `/api/` answers** in a way that weakens their ETags (compression that turns a strong tag weak breaks `If-Match`, `ARCHITECTURE §7.1`).
- **Outbound SMTP** to the relay on port 587 with STARTTLS (`Email:Smtp`).

---

## 5. Catalog releases and platform roles

Both are command modes of the API host: the same binary, started with arguments instead of serving requests. In a compose deployment run them in a one-off container of the API image, which has the configuration of the running API: `docker compose run --rm api <command>`. From the source: `dotnet run -- <command>` in `API/API.ProcureAccess`. Exit codes: **0** done (also when nothing had to change), **1** refused (nothing written, every reason printed), **2** wrong usage. They never migrate the database; start the API once first.

### 5.1 Catalog releases

A catalog release is a reviewed folder `catalog/releases/<version>/` (JSON; format in `SPEC` SD20 and `CODEBASE §2` S3-15). Stored releases never change; saved lists and workspace data are pinned to the release they were made with (`ARCHITECTURE` ADR-002).

```bash
docker compose run --rm api catalog import <version>     # validates the folder and stores it, inactive
docker compose run --rm api catalog activate <version>   # makes it the active release; the running API serves it from its next request
```

- The folders ship **in the API image**: a new or changed folder needs an image rebuild before `import`. From the source, set `Catalog__ReleasesPath=../../catalog/releases`.
- `import` refuses a folder with errors and lists each with file and JSON path. CI validates every folder on each pull request (stage 3).
- `activate` refuses a release with a criterion without a catalog-default priority, or a test type without its procedure criterion.
- **`T.1` is a test release**, not a catalog: a handful of made-up criteria for the local stacks, the E2E tests and the integration tests. It lives under `tests/`, is seeded only in `Development` and `E2E` from a folder the development and E2E overrides mount, and never reaches staging or production (`ARCHITECTURE §9.7`).
- **`2026.1`**, the first real release, has its BIT inklusiv texts but not yet the project team's priorities (open decision O-1, `kriterienkatalog_2026_1_luecken_v*.md`). Import it only into a throwaway database until they are in its folder: `activate` would refuse it, and a stored release cannot be changed afterwards. While no release is active, the client uses the legacy criteria filter.

### 5.2 Platform roles

Three platform roles exist: `PlatformAdmin`, `CatalogEditor`, `Moderator` (`ARCHITECTURE §8.1`, `SPEC` R-ADM-03). There is no screen for them; users are named by e-mail address.

```bash
docker compose run --rm api roles list [<email>]
docker compose run --rm api roles grant <email> <role>
docker compose run --rm api roles revoke <email> <role>
```

A change takes effect at the user's **next sign-in**: a running session keeps the roles it was signed in with until it ends (at the latest after the absolute limit, `Session:AbsoluteTimeout`, 8 hours). Each grant and revoke is logged with the user id and the role, never with the address.

---

## 6. Logs and health

### 6.1 Health

| Endpoint | Answers |
|---|---|
| `/health/live` | 200 while the process serves requests; checks nothing else |
| `/health/ready` | 200 when the API reaches the database, 503 otherwise. At most 🔧 2 checks run at a time (`Health:ReadyConcurrentChecks`; a further call gets 429), each with a 🔧 5 s timeout (`Health:ReadyTimeout`) |

The client's nginx forwards only `/api/`, so the health endpoints are not reachable through the public address: check them on the host (`curl http://127.0.0.1:5000/health/ready`) or inside the compose network (`http://api:8080/health/ready`, as the E2E stack's health check does). An uptime monitor outside the host needs a route to them in the reverse proxy (issue #448). The backup service has its own health state (§7.3). Design: `ARCHITECTURE §9.5`.

### 6.2 Logs

The API logs through Serilog to **the console** (`docker compose logs api`) and to **the table `logging."SeriLogs"`**. The minimum level is `AppLoggingSettings:General:RestrictedToMinimumLevel`, `Warning` by default, so information messages do not appear in either sink; lower it only to investigate, and only for a while. `LogRetentionJob` deletes rows older than 🔧 90 days (`AppLoggingSettings:Retention:RetentionDays`, `SPEC` SD24) at start and once a day.

```sql
-- the newest warnings and errors
SELECT "TimeStamp", "Level", "Message" FROM logging."SeriLogs" ORDER BY "TimeStamp" DESC LIMIT 50;
```

What the logs never contain, by design (`ARCHITECTURE §9.3`, §9.5): passwords, link tokens, the bodies of the anonymous export, and a user id together with a contributor-profile or entry id above `Debug`. The token of a share link is masked in every log property and in the client's nginx access log; nginx writes its error log only at `crit`. Keep it so when a log sink or a proxy log is added.

The log table is part of the backup (§7.1); its rows hold user ids and request paths, so a log export is personal data.

### 6.3 Known gaps

- Nothing alerts anybody yet: an unhealthy API or backup service is visible only to whoever looks (open point of S6-29, planned with GP.1).

Until the migrations have run, the PostgreSQL sink holds its events in memory (at most `AppLoggingSettings:PostgreSQL:QueueLimit`) and writes them afterwards, so the first start on an empty database stores every line of the start and leaves no `schema "logging" does not exist` in the database server's log (issue #437, S7-02). The errors `relation "…__EFMigrationsHistory" does not exist` that PostgreSQL logs at that start are EF Core asking whether a schema has migrations yet; they are expected.

---

## 7. Backup and restore

Requirement `R-ADM-04`; design in `ARCHITECTURE §10.4` and `ARCHITECTURE §7.1`; targets: RPO 24 h, RTO best-effort within one working day. The protocol of the task is `docs/sprints/sprint_06/S6-29_backup_restore.md`.

### 7.1 What is backed up

| What | How | Where it is kept |
|---|---|---|
| The whole database, every schema (with the key ring `identity."DataProtectionKeys"` and the log table) | `pg_dump` in custom format, every night | `BACKUP_DIR` on the host, one file `procureaccess-<UTC time>.dump`, mode 600 |
| Key-ring certificate | once when created, again after each rotation (§7.6) | the project's password manager, **never** next to the dumps |
| File store | **nothing yet:** it is built with migration step 7, and that step must add it here (`ARCHITECTURE §10.4`) | – |

**Not covered yet:** the dumps stay on the host, so **losing the host loses the backups too**, until the off-host copy is set up with GP.1 (owner decision S6-29 D3 (a)). There is no WAL archiving: a restore goes back to the last night (D1 (a)).

The dumps hold personal data (accounts, password hashes, workspace content, logs). Treat each copy like the production database.

### 7.2 Switching it on

The `backup` service of `docker-compose.yml` is behind the compose profile `backup`, so the dev and E2E stacks never start it. On staging and production set, in `.env` or the platform's environment:

| Variable | Default | Meaning |
|---|---|---|
| `COMPOSE_PROFILES` | – | `backup` switches the service on |
| `BACKUP_DIR` | `/var/backups/procureaccess` | Host folder of the dumps. Outside the checkout, because a deployment may replace the checkout |
| `BACKUP_RETENTION_DAYS` | 14 🔧 (SPEC SD26) | A dump older than this is removed after the next successful backup |
| `BACKUP_TIME_UTC` | `02:00` | Time of the nightly run, in UTC |
| `CONCURRENCY_ETAG_EPOCH` | 1 🔧 | Part of every ETag; raised after every restore (§7.4), never lowered |

At its start the service makes a backup when no dump is younger than a day.

### 7.3 Checking it

```bash
docker compose ps backup                    # „healthy“: a dump younger than 25 hours exists
docker compose logs --tail 20 backup        # „Backup written: …“, or „Backup failed …“
docker compose run --rm backup sh /opt/backup/backup.sh   # one backup now
```

A failed run keeps every older dump and leaves no partial file under a dump's name. Nothing alerts anybody yet: an unhealthy service is only visible in these commands until monitoring reads it (open point of S6-29, with GP.1).

### 7.4 Restoring

A restore never writes into an existing database. It creates a new one, and the application moves to it by `DB_NAME`, so the old database stays available until the result has been checked.

> **Never start the API in `Development` on a restored database.** `appsettings.Development.json` sets `RebuildDatabase`, which deletes every user at the start (only `docker-compose.rehearsal.yml` switches it off). Staging and production are protected by SEC-06.

You need the dump **and** the key-ring certificate that belonged to the database at that time.

1. Stop the application and the nightly backup: `docker compose stop api backup`. The application must not write to the old database, and the backup must not dump an empty new server, because that file would then be the newest dump.
2. If the database server itself was lost, start an empty one: `docker compose up -d db`.
3. Pick the dump: `ls -l "$BACKUP_DIR"`. Take the newest one **from before the damage**, and distrust a file much smaller than its neighbours: a dump of an emptied database counts as a successful backup.
4. Restore it into a **new** name (lower-case letters, digits, underscores):

   ```bash
   docker compose run --rm backup sh /opt/backup/restore.sh \
     /backups/procureaccess-<UTC time>.dump procureaccess_restored_<yyyymmdd>
   ```

   The script stops without changing anything when the database exists. If the restore fails, it removes the incomplete new database.
5. In `.env` or the platform's environment:
   - `DB_NAME=procureaccess_restored_<yyyymmdd>`;
   - **`CONCURRENCY_ETAG_EPOCH` raised by one**, and kept there. A restore brings back older versions, so an ETag issued before it could match a restored version and overwrite a change without a conflict. The higher epoch makes every earlier tag stale (`ARCHITECTURE §7.1`, API-04);
   - `DATA_PROTECTION_CERTIFICATE` (and, if one was set at the time, `DATA_PROTECTION_PREVIOUS_CERTIFICATE`) with the values of that database (§7.6).
6. Start with an API image at least as new as the dump: `docker compose up -d`. The API applies any newer migrations at its start.
7. Check: `/health/ready` answers 200; sign in; open a project; the catalog shows the active release.
8. Tell the users what was lost: everything since the time of the dump.
9. Only when the result is confirmed, remove the old database, because it still holds personal data: `docker compose exec db dropdb -U "$DB_USER" <old name>`.

> **Deleted accounts come back (⚖️ open, for the legal review).** A dump holds the accounts as they were that night. An account deleted **after** the dump is in the restored database again, and nothing records the deletion so that it could be repeated. Until the legal review has decided how to handle this, note every restore and check with the person responsible for data protection before the application goes back online.

### 7.5 Rehearsing

```bash
node scripts/rehearse-restore.mjs
```

It needs Docker and openssl, takes a few minutes and is not part of CI. It runs on a throwaway stack (`docker-compose.rehearsal.yml`): own project name and network, no fixed host port, the database in memory, every secret generated for the run; the real `.env` is not read. So it can run beside the dev or E2E stack.

| # | The rehearsal checks |
|---|---|
| 1 | A source stack with an encrypted key ring, a user, an organisation and a project |
| 2 | A backup while the API runs; the retention removes the 15-day dump and keeps the 13-day one |
| 3 | A change after the backup, which must be lost |
| 4 | The database server replaced by an empty one, the dump restored into a new database, every table present with the same row count (the log table is listed, not counted) |
| 5 | A second restore into that database is refused |
| 6 | The API on the restored database, ETag epoch 2, **seeders off**, so what it serves can only come from the dump |
| 7 | The session from before the backup still valid (restored key ring), sign-in with the password from the dump, the project in its state at the backup, the catalog release |
| 8 | ETags from before the restore answer 409; the new tag works |

| Date | Where | Result |
|---|---|---|
| 2026-11-23 | Local, throwaway stack (S6-29) | Passed, 14 of 14 steps |
| – | Staging | **Still to do, with GP.1:** the steps of §7.4 on the real deployment, including whether Dokploy honours the compose profile and keeps `BACKUP_DIR` across deployments |

### 7.6 Key ring and certificate

*Taken over from S4-26 §6.*

ASP.NET Core Data Protection protects the session cookie, the antiforgery tokens and the e-mail links. Its key ring is the table `identity."DataProtectionKeys"` in the application database (IdentityAccess module, application name `ProcureAccess`). A new key is created every 90 days; old keys stay in the table, because payloads made with them must stay readable. Each key is stored as XML, encrypted with the key-ring certificate (`<encryptedSecret>`); the table alone is useless without it.

| Setting | Compose / `.env` | Meaning |
|---|---|---|
| `DataProtection:Certificate` | `DATA_PROTECTION_CERTIFICATE` | PFX with RSA private key, no password, base64 on one line. Encrypts new keys and reads them. Required outside `Development` and `E2E`: the API does not start without it |
| `DataProtection:PreviousCertificate` | `DATA_PROTECTION_PREVIOUS_CERTIFICATE` | Optional, only after a rotation: reads keys encrypted with the old certificate |

Local stacks and CI (`Development`) run without a certificate; their keys are stored in plain text and ASP.NET logs „No XML encryptor configured“.

**Create the certificate** on a trusted machine, not on the server:

```bash
openssl req -x509 -newkey rsa:3072 -sha256 -days 3650 -nodes -subj "/CN=ProcureAccess key ring" \
  -keyout keyring-key.pem -out keyring-cert.pem
openssl pkcs12 -export -inkey keyring-key.pem -in keyring-cert.pem -passout pass: -out keyring.pfx
base64 -w0 keyring.pfx        # the value of DATA_PROTECTION_CERTIFICATE
shred -u keyring-key.pem keyring-cert.pem
```

One certificate per environment (staging and production never share one). It is self-signed; its validity dates are not checked, so it keeps working after `-days`: rotate for other reasons, not because of the date. Store `keyring.pfx` (or the base64 value) in the password manager of the project, **not** next to the database backups, then set the value in the platform and redeploy.

**Backup.** The table is part of the nightly dump. The certificate is backed up once, when it is created, and again after each rotation, apart from the dumps: whoever holds both can read the keys; whoever holds only one cannot. After a rotation, keep the previous certificate as long as it is set in `DATA_PROTECTION_PREVIOUS_CERTIFICATE`.

**Restore without the right certificate.** The API still starts, but cannot read the old keys: it creates a new key, so outstanding e-mail links and all sessions end, and users request a new link or sign in again. No other data is lost. The same happens when the table is missing from the restore.

**Rotation**, when the certificate may have leaked or someone who had it leaves:

1. Create a new certificate.
2. Move the current value to `DATA_PROTECTION_PREVIOUS_CERTIFICATE`, set the new one as `DATA_PROTECTION_CERTIFICATE`, redeploy. New keys are encrypted with the new certificate; the keys of before stay readable through the previous one.
3. Keep the previous certificate for at least 100 days (the 90-day key lifetime plus the lifetime of the payloads made with the last old key), then remove the variable and redeploy.

Removing it earlier only makes data protected before the rotation unreadable (links, sessions); new data keeps working. After a leak, the faster and stricter way is to skip step 3: set only the new certificate, so every old key is unusable at once and everybody signs in again.

**One-off: delete the keys stored in plain text (O-S4-89).** A database that ran before S4-26 holds keys in plain text, and Data Protection keeps reading and using them: its current plain key stays the default key until it expires (up to 90 days), so setting the certificate alone encrypts nothing yet. Once the API runs with the certificate (staging; any kept database that should be protected), delete the plain keys and restart:

```sql
-- the plain keys (no <encryptedSecret>)
SELECT "Id", "FriendlyName" FROM identity."DataProtectionKeys" WHERE position('<encryptedSecret' IN "Xml") = 0;
DELETE FROM identity."DataProtectionKeys" WHERE position('<encryptedSecret' IN "Xml") = 0;
```

Restart the API right after the delete: it caches the key ring, and on the restart it finds no key and creates a new, encrypted one (check with the query above: no row left, one new row with `<encryptedSecret`). E-mail links sent before the deletion stop working once. Backups made before the deletion still hold the plain keys; they go with the backup retention.

---

## 8. Usage indicators (R-MET-02)

`SPEC §9.2` names the indicators of the PRD and their source; `SPEC §9.1` the only two usage events the application records, with a date and no personal reference (`ARCHITECTURE §5.7`, PRV-05). The queries below reproduce the indicators. **They return aggregates only** — counts per month, per dimension value or per criterion — and nothing that names a user, an organisation, a list or a product. They read every organisation's data, so run them as the operator, not through the application, and pass on only their result. No target values exist yet; the project team sets them after the first pilot.

Run them on the host, in a session that cannot write (the single quotes matter: the variables are the database container's own):

```bash
docker compose exec -T db sh -c 'PGOPTIONS="-c default_transaction_read_only=on" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"' < query.sql
```

Without `-T` and `< query.sql` the same command opens an interactive `psql`.

| Indicator (`SPEC §9.2`) | Query |
|---|---|
| Completed criteria configurations | §8.2 `completed-configurations` |
| Exports with and without account | §8.2 `exports` |
| Saved lists | §8.2 `saved-lists` |
| Workspace entries | §8.2 `workspace-entries` |
| Expert and pilot feedback on prioritisation (supporting data) | §8.2 `priority-overrides` |
| Published vs. private workspace entries | **not yet:** follows with migration step 6, which builds the publication of workspace entries (`WorkspacePublication`) |
| Growth of Kanal-1 and Kanal-2 entries | **not yet:** follows with migration step 6, which builds the directory (`PublishedEntry`, `FirstPublishedAt`) |
| Pilot procurements | recorded by hand by the project team; no query |

### 8.1 How the queries stay correct

Each query below is a `sql` block whose first line is `-- metric: <name>`. The integration test `MetricQueryTests` (`tests/ProcureAccess.Tests.Integration/Operations/`) reads exactly these blocks from this file, runs each in a read-only transaction against a database with known rows and compares the result. A renamed table or column therefore fails CI, not the next report. Whoever changes a query, adds one (step 6 adds the two missing ones) or renames a `-- metric:` line changes the test in the same pull request.

### 8.2 The queries

**Completed criteria configurations** — S11 shown for a complete selection, per month, anonymous and signed in (`CriteriaListShown`):

```sql
-- metric: completed-configurations
SELECT date_trunc('month', "OccurredOn")::date AS month,
       "Dimension1"                            AS audience,
       count(*)                                AS configurations
FROM analytics."UsageEvents"
WHERE "EventType" = 'CriteriaListShown'
GROUP BY 1, 2
ORDER BY 1, 2;
```

**Exports with and without account** — per month, by file format and by anonymous or signed in (`ExportCreated`):

```sql
-- metric: exports
SELECT date_trunc('month', "OccurredOn")::date AS month,
       "Dimension1"                            AS format,
       "Dimension2"                            AS audience,
       count(*)                                AS exports
FROM analytics."UsageEvents"
WHERE "EventType" = 'ExportCreated'
GROUP BY 1, 2, 3
ORDER BY 1, 2, 3;
```

The dates of both events are UTC days (`OccurredOn`), so a month is a UTC month.

**Saved lists** — the organisations' saved lists today; a project's own copy of a list is not a saved list and is not counted:

```sql
-- metric: saved-lists
SELECT count(*) AS saved_lists
FROM configurator."CriteriaConfigurations"
WHERE "ProcurementProjectId" IS NULL;
```

**Workspace entries** — candidate products in procurement projects today:

```sql
-- metric: workspace-entries
SELECT count(*) AS candidates
FROM workspace."CandidateProducts";
```

Both are counts at the time of the query: a deleted list or a removed candidate is gone. Run them on the same day each month to follow the trend.

**Prioritisation in saved lists** — per criterion: in how many saved lists it appears, how often the priority level was changed from the recommended one, how often a weight was set, how often it was excluded. It supports the experts' and pilots' feedback on the recommended priorities; it is not a verdict on a criterion or a product:

```sql
-- metric: priority-overrides
SELECT c."Key"                                                    AS criterion,
       count(*)                                                   AS in_saved_lists,
       count(*) FILTER (WHERE cc."OverrideLevel" IS NOT NULL)     AS level_changed,
       count(*) FILTER (WHERE cc."OverrideWeight" IS NOT NULL)    AS weight_set,
       count(*) FILTER (WHERE NOT cc."IsIncluded")                AS excluded
FROM configurator."ConfigurationCriteria" cc
JOIN configurator."CriteriaConfigurations" l ON l."Id" = cc."ConfigurationId"
JOIN catalog."CriterionRevisions" r ON r."Id" = cc."CriterionRevisionId"
JOIN catalog."Criteria" c ON c."Id" = r."CriterionId"
WHERE l."ProcurementProjectId" IS NULL
GROUP BY c."Key"
ORDER BY c."Key";
```

A criterion is counted by its stable key (`Criterion.Key`), so lists of different catalog releases add up (`ARCHITECTURE` ADR-002). `ResolvedLevel` is the level the catalog recommended when the list was saved; `OverrideLevel` is set only when the user chose another one (a choice equal to the recommendation is stored as none). This query joins two module schemas; that is fine for an operator's read, while application code never does it (`CLAUDE.md §7`).

---

## 9. Where to read more

| Question | Document |
|---|---|
| What the product is and why | `docs/prd_procureaccess_werteit_v*.md` (German) |
| What exactly it must do | `docs/SPEC_v*.md` — requirements `R-<AREA>-nn`, limits, rate limits, open questions §13 |
| How it is built, and why | `docs/ARCHITECTURE_v*.md` — modules §4, domain §5, API §7, client §8, cross-cutting §9, operations §10, migration steps §12, ADRs |
| What the tests guarantee | `docs/TESTING_v*.md` — invariant catalogue §4, levels and tooling §2, §3, CI stages §8.2 |
| What each task built, and every command | `docs/CODEBASE_v*.md` — §2 by area, §3 repository layout, §4 commands |
| The rules for changing code (also for AI assistants) | `CLAUDE.md` |
| Known debt and when it is paid | `docs/TECH_DEBT_v*.md` |
| What was decided in a task | `docs/sprints/sprint_NN/` — the sprint plan and one protocol per task |
| UI texts and behaviour | `docs/ux_copy_procureaccess_v*.md` (German), `docs/design_handoff_procureaccess_v1.md` |
| How the project works with AI assistance | `docs/ai_development_process.md` |

Documents carry their version in the file name; always read the highest one.

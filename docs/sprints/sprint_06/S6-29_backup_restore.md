<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-29 — Nightly backup and restore rehearsal

> **Sprint:** 06, task S6-29 (stretch; ROADMAP GP.2). **Issues:** none (#4 stays open for the rest of `TECHNICAL.md`). **Requirements / invariants:** R-ADM-04 (completion); no invariant activated, no new TESTING version; API-04 is active and was exercised by the rehearsal.
> **Status:** done by Claude Code (2026-11-23) — pull request open. The rehearsal ran locally and passed; **the staging run comes with GP.1**. **E2E left to CI:** another session used the local E2E stack, so it was not started. No EF migration, no new dependency.
> **Scope:** exactly the sprint row: a nightly `pg_dump` of every schema with retention as a compose service, a restore script, a documented and locally run rehearsal, the ETag-epoch bump after a restore.
> **Owner decisions (2026-11-23):** D1 (a) nightly `pg_dump` only, no WAL archiving, ARCHITECTURE §10.4 amended. D2 (a) retention 14 days 🔧 (SPEC SD26). D3 (a) dumps in a host folder `BACKUP_DIR`; the off-host copy is set up with GP.1. D4 (a) `docs/TECHNICAL.md` created with only the backup and restore chapter, including S4-26 §6; #4 stays open for the rest.

## 1. Starting point

- No backup existed. ARCHITECTURE §10.4 asked for a nightly `pg_dump` plus WAL archiving and a rehearsal documented in `TECHNICAL.md`, which did not exist (#4); the task row and R-ADM-04 name only a nightly backup (D1).
- No document gave a retention (D2).
- **The epoch could not be raised on a deployment:** `Concurrency:ETagEpoch` is read from configuration, but `docker-compose.yml` did not pass it to the API.
- **Starting the API in `Development` on a restored database deletes every user:** `appsettings.Development.json` sets `RebuildDatabase`. Staging and production are protected by SEC-06; the rehearsal runs in `Development` and has to switch it off.
- The file store is built with step 7, so there are no files to back up yet.

## 2. Changes

| File | Change |
|---|---|
| `docker-compose.yml` | Service `backup` (image of `db`) behind the profile `backup`, with a health check on the age of the newest dump; `Concurrency__ETagEpoch` from `CONCURRENCY_ETAG_EPOCH` |
| `scripts/backup/schedule.sh` | Main process of the service: nightly at `BACKUP_TIME_UTC`, catches up at its start when no dump is younger than a day |
| `scripts/backup/backup.sh` | Dump of every schema under a temporary name, renamed after `pg_restore --list` has read it, mode 600; only then the retention, by the time in the file name |
| `scripts/backup/restore.sh` | Restores only into a new database, refuses an existing name, removes what a failed restore created |
| `scripts/backup-restore.test.mjs` | 32 self-tests of the shell scripts against fake PostgreSQL tools; new step in CI stage 1 |
| `scripts/rehearse-restore.mjs`, `docker-compose.rehearsal.yml` | The rehearsal on a throwaway stack: own project and network, no fixed host port, database in memory, generated secrets; the real `.env` is not read |
| `.env.example` | The backup variables and `CONCURRENCY_ETAG_EPOCH` |
| `docs/TECHNICAL.md` | New: the runbook (backup, check, restore, rehearsal, key ring from S4-26 §6) |
| `docs/ARCHITECTURE_v106.md` | C194, §10.4; v104 deleted (two newest kept) |
| `docs/SPEC_v2.md` | v2.68: R-ADM-04, SD26 |
| `docs/CODEBASE_v1.md` | §2.115, §3 layout, §4 commands |

**Deviation from the plan:** `CLAUDE.md` is unchanged. It has 39 998 characters and must stay under 40 000, so the commands went into `CODEBASE §4` (open point 6).

### Rehearsal, run on 2026-11-23 (local, throwaway stack, API image built from this branch)

```text
 1. ok  source stack started (key ring encrypted with a throwaway certificate), signed in
 2. ok  project created, ETag "1-1"
 3. ok  backup on the running stack: 57 tables, 163765 bytes, mode 600; retention removed the 15-day dump and kept the 13-day one
 4. ok  project changed after the backup, ETag "1-2" (this change must be lost)
 5. ok  database server replaced by an empty one
 6. ok  restored into the new database procureaccess_restored: all 57 tables present, row counts equal (log table not counted)
 7. ok  a second restore into the same database is refused
 8. ok  API started on the restored database, ETag epoch 2, seeders off
 9. ok  session cookie from before the backup still accepted (restored key ring read with the certificate)
10. ok  signed in with the password from the dump
11. ok  project opened with its state at the backup, ETag "2-1"
12. ok  catalog release T.1 served from the restored database
13. ok  tags from before the restore refused with 409 ("1-1", "1-2")
14. ok  change with the new tag accepted, ETag "2-2"
Restore rehearsal passed.
```

The real service was checked separately on an isolated stack with `/backups` in memory: it wrote a dump at its start (mode 600), met its health condition and stopped in 225 ms. The services of the dev and E2E stacks do not include `backup`.

## 3. Self-review (CLAUDE §6.7, §6.8; `/engineering:code-review`)

| # | Finding | Sorting |
|---|---|---|
| R1 | Runbook: after the loss of the database server, a `backup` service left running could dump the new empty database, and that file would be the newest dump | **Fix:** step 1 stops `backup` too; step 3 says how to pick the dump |
| R2 | A dump of a database that was emptied unnoticed counts as a successful backup, and after 14 days the retention has removed the good ones | **Follow-up** with the monitoring of GP.1 (open point 3) |
| R3 | An interrupted rehearsal (Ctrl+C) leaves its stack running | **Won't fix:** throwaway, no fixed ports; `docker compose -p procureaccess-rehearsal down -v` removes it |

Checked without a finding: the database name is validated before it enters SQL; a failed dump costs no older dump; the restore cannot write to an existing database; no secret is in an argument, a log or the repository.

## 4. Checks

| Check | Result |
|---|---|
| Script self-tests (seven suites, Node 24) | 138 of 138 passed, 32 of them new |
| `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons` | passed |
| `npm run typecheck`, `npm run lint` (e2e, scripts) | passed |
| Licence headers | `reuse lint-file` on the new and changed files passed; the full `reuse lint` runs in CI, because it cannot read `.gitignore` in a worktree |
| Restore rehearsal | passed, 14 of 14 steps |
| .NET build and tests | not run: no C# changed. The API image was built from this branch for the rehearsal |
| **E2E** | **not run locally, left to CI** (another session used the local E2E stack). The first CI run of this pull request failed one WebKit test outside this change (open point 8); the checks of the pull request show the run on the final commit |
| POSIX shells | parsed and run in the Alpine image (BusyBox); Ubuntu's `dash` is covered by the self-test in CI |

## 5. Open points

1. **Staging rehearsal with GP.1:** the steps of `TECHNICAL.md` §1.4 on the real deployment, including whether Dokploy honours `COMPOSE_PROFILES`, mounts `./scripts/backup` and keeps `BACKUP_DIR` across deployments.
2. **Off-host copy with GP.1 (D3 (a)):** until then, losing the host loses the backups too.
3. **No alert yet:** an unhealthy `backup` service is visible only in `docker compose ps`, and a dump of an emptied database is not detected (R2). Proposed for GP.1.
4. **⚖️ For the legal review:** an account deleted after a dump comes back with a restore of that dump, and nothing records the deletion so that it could be repeated. The runbook warns; the handling is not decided here (SPEC SD26, v2.68-2).
5. **File store (step 7):** that step must add its files to the backup and the runbook.
6. **`CLAUDE.md` is full (39 998 of 40 000 characters):** the document table does not name `TECHNICAL.md` yet. It needs a shortening first, which is outside this task.
7. **Sprint close-out:** TECH_DEBT v6 should record that the backup part of #4 is done; ROADMAP GP.2 stays open until the staging run.
8. **Flaky E2E test (#436):** `e2e/identity/account.spec.ts:120` timed out once in WebKit (run 45184273467). The trace shows `fill` finished, but the field was empty afterwards, so „Konto endgültig löschen“ stayed `aria-disabled` and the click waited until the timeout. The test types without waiting for the dialog's first focus, which CLAUDE §3 (E2E) requires, so that is the likely cause; it was not reproduced. Not changed here (scope).
9. **Log sink at the first start (#437):** on an empty database the Serilog sink tries to write before the migrations have created the schema, and PostgreSQL logs `schema "logging" does not exist` once (same CI run). Whether the sink drops that batch or writes it again after the migrations was not checked; the console receives the same lines either way. A restored database already has the schema.

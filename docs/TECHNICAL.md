# ProcureAccess — Technical documentation

> **Status:** started with sprint 06, task S6-29 (2026-11-23; owner decision S6-29 D4 (a)). It holds **only** the chapter on backup and restore for now. The rest (deployment variables, SDK pin, `T.1` against a real release …) follows with issue #4, process step 16.

---

## 1. Backup and restore

Requirement `R-ADM-04`; design in `ARCHITECTURE §10.4`, `§7.1`; targets: RPO 24 h, RTO best-effort within one working day. The protocol of the task is `docs/sprints/sprint_06/S6-29_backup_restore.md`.

### 1.1 What is backed up

| What | How | Where it is kept |
|---|---|---|
| The whole database, every schema (with the key ring `identity."DataProtectionKeys"` and the log table) | `pg_dump` in custom format, every night | `BACKUP_DIR` on the host, one file `procureaccess-<UTC time>.dump`, mode 600 |
| Key-ring certificate | once when created, again after each rotation (§1.6) | the project's password manager, **never** next to the dumps |
| File store | **nothing yet:** it is built with migration step 7, and that step must add it here (`ARCHITECTURE §10.4`) | – |

**Not covered yet:** the dumps stay on the host, so **losing the host loses the backups too**, until the off-host copy is set up with GP.1 (owner decision S6-29 D3 (a)). There is no WAL archiving: a restore goes back to the last night (D1 (a)).

The dumps hold personal data (accounts, password hashes, workspace content, logs). Treat each copy like the production database.

### 1.2 Switching it on

The `backup` service of `docker-compose.yml` is behind the compose profile `backup`, so the dev and E2E stacks never start it. On staging and production set, in `.env` or the platform's environment:

| Variable | Default | Meaning |
|---|---|---|
| `COMPOSE_PROFILES` | – | `backup` switches the service on |
| `BACKUP_DIR` | `/var/backups/procureaccess` | Host folder of the dumps. Outside the checkout, because a deployment may replace the checkout |
| `BACKUP_RETENTION_DAYS` | 14 🔧 (SPEC SD26) | A dump older than this is removed after the next successful backup |
| `BACKUP_TIME_UTC` | `02:00` | Time of the nightly run, in UTC |
| `CONCURRENCY_ETAG_EPOCH` | 1 🔧 | Part of every ETag; raised after every restore (§1.4), never lowered |

At its start the service makes a backup when no dump is younger than a day.

### 1.3 Checking it

```bash
docker compose ps backup                    # „healthy“: a dump younger than 25 hours exists
docker compose logs --tail 20 backup        # „Backup written: …“, or „Backup failed …“
docker compose run --rm backup sh /opt/backup/backup.sh   # one backup now
```

A failed run keeps every older dump and leaves no partial file under a dump's name. Nothing alerts anybody yet: an unhealthy service is only visible in these commands until monitoring reads it (open point of S6-29, with GP.1).

### 1.4 Restoring

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
   - `DATA_PROTECTION_CERTIFICATE` (and, if one was set at the time, `DATA_PROTECTION_PREVIOUS_CERTIFICATE`) with the values of that database (§1.6).
6. Start with an API image at least as new as the dump: `docker compose up -d`. The API applies any newer migrations at its start.
7. Check: `/health/ready` answers 200; sign in; open a project; the catalog shows the active release.
8. Tell the users what was lost: everything since the time of the dump.
9. Only when the result is confirmed, remove the old database, because it still holds personal data: `docker compose exec db dropdb -U "$DB_USER" <old name>`.

> **Deleted accounts come back (⚖️ open, for the legal review).** A dump holds the accounts as they were that night. An account deleted **after** the dump is in the restored database again, and nothing records the deletion so that it could be repeated. Until the legal review has decided how to handle this, note every restore and check with the person responsible for data protection before the application goes back online.

### 1.5 Rehearsing

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
| – | Staging | **Still to do, with GP.1:** the steps of §1.4 on the real deployment, including whether Dokploy honours the compose profile and keeps `BACKUP_DIR` across deployments |

### 1.6 Key ring and certificate

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

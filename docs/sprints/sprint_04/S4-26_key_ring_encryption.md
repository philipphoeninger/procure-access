# S4-26 — Encrypt the Data Protection key ring at rest and document it

> **Sprint:** 04, P2 stretch task S4-26 (1 point). **Issues:** #235. **Requirements / invariants:** `TECH_DEBT_v3.md` TD-27; ARCHITECTURE §9.1, §10.4, C60, C112 (3) → C131; **OPS-04 stays `planned`** (cookie session, step 3); no invariant activated, so no new TESTING version and no SPEC change.
> **Status:** done — pull request #303 open.
> **Scope:** the key ring in `identity."DataProtectionKeys"` is encrypted at rest with a key from the environment; key store, backup, restore and rotation documented as a section that `TECHNICAL.md` (#4) can take over (§6). **Not here:** `TECHNICAL.md` itself (#4, process step 16), the OPS-04 test, deleting the plain rows on staging (an owner action, §6.6).
> **Owner decisions (2026-11-12, asked before the task, all (a) as recommended):** **O-S4-87** — the built-in `ProtectKeysWithCertificate` with an X.509 certificate: a base64 PFX from an environment variable, plus an optional previous certificate for a rotation (not a hand-written `IXmlEncryptor`, not „document why not“). **O-S4-88** — the certificate is required outside Development and E2E, where the start fails without it; local stacks and CI stay unencrypted. **O-S4-89** — key rows stored in plain text before are deleted once by hand after the first encrypted start; no code deletes keys. Also accepted with the plan: the documentation lives in this protocol (§6) until `TECHNICAL.md` exists; the certificate is self-signed with `openssl`, its expiry does not matter.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `AddIdentityAccessModule` stored the key ring in plain text (S3-24 decision 3a); ASP.NET logs „No XML encryptor configured“ | #235, TD-27 |
| F2 | Two mechanisms fit (certificate, own `IXmlEncryptor`), plus „record why not“ | Asked: O-S4-87 (a) |
| F3 | The module registration had no `IHostEnvironment`; the environment allow-list of SEC-06 and the test-catalog seeder lives in `API` | `AddIdentityAccessModule(configuration, environment)`; the module keeps its own `E2E` constant (it never references `API`, ARC-01) |
| F4 | `EncryptedXml` (`System.Security.Cryptography.Xml`) is part of the ASP.NET Core shared framework | No new package, no licence check |
| F5 | Secrets come only from environment variables (CLAUDE §10); `docker-compose.yml` lists every variable explicitly | Two new compose variables, documented in `.env.example` |
| F6 | CI writes the E2E `.env` from `.env.example` (`ASPNETCORE_ENVIRONMENT=Development`); the integration tests run as `Testing` | CI needs no secret; the integration host gets a throwaway certificate |
| F7 | The `roles` command host (S4-25) registers IdentityAccess | Passes its environment too; checked in the container (§4) |
| F8 | The last S4-25 commit (`f503fe3`, open point P7 → #302) was pushed after PR #300 was merged and is not on `main` | Not in this PR: control chat (§5 P5) |

## 2. Changes

| Area | Change |
|---|---|
| `src/…IdentityAccess/DataProtection/KeyRingCertificates.cs` (new, `internal`) | Reads `DataProtection:Certificate` and `DataProtection:PreviousCertificate`: base64 → `X509CertificateLoader.LoadPkcs12(…, password: null)` → must hold an RSA private key. Empty = not set. Without a current certificate: allowed only in `Development` and `E2E` (`MayStoreKeysUnencrypted`), otherwise `InvalidOperationException`; a value that is set but unusable, or a previous certificate without a current one, throws in every environment. Messages name the setting, never the value |
| `src/…IdentityAccess/IdentityAccessModule.cs` | Takes `IHostEnvironment`; `ProtectKeysWithCertificate(current)` when set, `UnprotectKeysWithAnyCertificate(previous)` when set |
| `API/…/Program.cs`, `Extensions/RoleCommandHost.cs` | Pass `builder.Environment` |
| `docker-compose.yml` | `DataProtection__Certificate: ${DATA_PROTECTION_CERTIFICATE:-}`, `DataProtection__PreviousCertificate: ${DATA_PROTECTION_PREVIOUS_CERTIFICATE:-}` |
| `.env.example` | Both variables (empty), the `openssl` commands, where to keep the PFX, rotation in two lines |
| `tests/…Shared/DataProtection/TestCertificates.cs` (new) | In-memory self-signed certificates as setting values: RSA with key, RSA without key, ECDSA |
| `tests/…Integration/Infrastructure/ProcureAccessApiFactory.cs` | Every host gets one certificate per test run (`Lazy`), so hosts on one database share it; `settings` overrides it |
| `tests/…Unit/IdentityAccess/KeyRingCertificatesTests.cs` (new, 13 cases) | Missing in Production/Staging/Testing/other → start fails; missing in Development/E2E → unencrypted; whitespace = not set; RSA PFX taken; not base64, not a PFX, no private key, ECDSA → fails also in Development; bad previous; previous without current; the value never in the message |
| `tests/…Integration/Operations/DataProtectionKeyStoreTests.cs` (+4) | The stored row holds `<encryptedSecret`, no `<masterKey`; a host on the same database without the certificate cannot unprotect; after a rotation (new certificate + old as previous) the old payload is read; a plain key stored before the certificate is still read (why O-S4-89 needs the manual delete) |
| `docs/ARCHITECTURE_v45.md` | C131; §4.3 IdentityAccess row, §9.1 key store, §10.4 restore needs the certificate; C112 (3) marked superseded. v43 deleted (two newest kept) |
| `CLAUDE.md` | Version line; §3 key store; §4 module folder; §5 certificate note |
| `docs/sprints/sprint_04/SPRINT_04.md` | Rows O-S4-87 – O-S4-89 |

No migration, no endpoint, no UI text, no new package.

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; `/engineering:code-review` on the diff. N+1 and a11y: not applicable (Data Protection loads the key ring once and refreshes it about every 24 h; no UI). Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | The comment in `AddIdentityAccessModule` said the certificate is read „before anything is registered“ — the context is registered first | Fixed: „while the services are registered, … before the host is built“ |
| R2 | Mutation check: without `ProtectKeysWithCertificate` the encrypted-row and the without-certificate tests fail; without `UnprotectKeysWithAnyCertificate` the rotation test fails | – |
| R3 | Expiry: an expired certificate still encrypts and decrypts (checked with a certificate valid until 2025-11-12), so expiry never stops the API | – documented (§6.2) |
| R4 | Dropping the previous certificate too early: new keys are created and new data works; only data protected before the rotation can no longer be read (checked) | – documented (§6.5): at worst e-mail links and, from step 3, sessions of before the rotation end |
| R5 | The startup exception of the container ends with exit code 139, also for a missing connection string, i.e. before this change | Won't fix here: the process stops either way and compose restarts it (`on-failure`); §5 P4 |
| R6 | `KeyRingCertificates` is a record, so it has value equality over two `X509Certificate2` | Won't fix: one instance for the app's lifetime, never compared; style |
| R7 | The `E2E` environment name now exists twice (`TestCatalogSeedExtensions`, `KeyRingCertificates`) | Won't fix: a module never references `API` (ARC-01); both say they follow TESTING §3.3 |
| R8 | The PFX has no password | Won't fix: O-S4-87; a password in the same environment as the PFX protects nothing |
| R9 | Security: no secret in the repository (test certificates are made in memory per run), the setting value is never logged or put into a message, no endpoint | – |

No correctness or security finding left that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | ✅ 0 warnings |
| 2 | `dotnet format ProcureAccess.sln --verify-no-changes` | ✅ exit 0 |
| 3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | ✅ 912 tests, 0 failed (ARC-01 – ARC-03, OPS-03 included) |
| 4 | API image built and started against a throwaway PostgreSQL on its own network (the running dev stack untouched) | ✅ `Production` without certificate: stops with the `DataProtection:Certificate` message; with an `openssl` certificate (commands of `.env.example`): starts, one key row with `<encryptedSecret` and no `<masterKey`, no „No XML encryptor configured“; restart reads the key (still one row); `roles list` runs with exit 0 |
| 5 | `openssl` 4.1 PFX with empty password (`-passout pass:`) loads with `X509CertificateLoader.LoadPkcs12(…, null)` | ✅ |
| 6 | E2E | Left to CI: the change only adds two empty variables in `Development`; another stack ran locally |
| 7 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v54.md`, unchanged) |
| 8 | `node scripts/check-figma-links.mjs` | ✅ unchanged (6 checked, 20 not built, 0 pending) |
| 9 | `node scripts/check-i18n.mjs` | ✅ 0 errors (no UI text touched) |
| 10 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 11 | CI on the pull request | see the pull request |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | **Before this PR is deployed to staging:** create the certificate (§6.2) and set `DATA_PROTECTION_CERTIFICATE` in Dokploy; without it the staging API (`Production`/`Staging`) does not start. Then delete the plain rows once (§6.6, O-S4-89) | Owner, with the next staging deployment (#236) |
| P2 | Take §6 into `TECHNICAL.md`, rehearse the restore once (ARCHITECTURE §10.4) | #4, before G-P |
| P3 | `TECH_DEBT_v3` (sprint close-out, step 14): TD-27 / #235 resolved | Sprint 04 close-out |
| P4 | The API container ends with exit code 139 (not 134) on any unhandled startup exception (R5) | Watch; issue only if it hides a real crash |
| P5 | Commit `f503fe3` (S4-25 protocol P7, #302) is on the remote branch `sprint04/S4-25-m3-groundwork` but not on `main` | Control chat: cherry-pick or a small PR |
| P6 | OPS-04 (session survives a restart) with the cookie session; its hosts get the test-run certificate from the factory | Migration step 3 |

## 6. Key store, backup and restore (for `TECHNICAL.md`, #4)

### 6.1 What it is

ASP.NET Core Data Protection protects the e-mail confirmation, password-reset and e-mail-change links today and, from migration step 3, the session cookie and antiforgery tokens. Its key ring is the table `identity."DataProtectionKeys"` in the application database (IdentityAccess module, application name `ProcureAccess`). A new key is created every 90 days; old keys stay in the table, because payloads made with them must stay readable. Each key is stored as XML, encrypted with the key-ring certificate (`<encryptedSecret>`); the table alone is useless without it.

| Setting | Compose / `.env` | Meaning |
|---|---|---|
| `DataProtection:Certificate` | `DATA_PROTECTION_CERTIFICATE` | PFX with RSA private key, no password, base64 on one line. Encrypts new keys and reads them. Required outside `Development` and `E2E`: the API does not start without it |
| `DataProtection:PreviousCertificate` | `DATA_PROTECTION_PREVIOUS_CERTIFICATE` | Optional, only after a rotation: reads keys encrypted with the old certificate |

Local stacks and CI (`Development`) run without a certificate; their keys are stored in plain text and ASP.NET logs „No XML encryptor configured“.

### 6.2 Create the certificate

On a trusted machine, not on the server:

```bash
openssl req -x509 -newkey rsa:3072 -sha256 -days 3650 -nodes -subj "/CN=ProcureAccess key ring" \
  -keyout keyring-key.pem -out keyring-cert.pem
openssl pkcs12 -export -inkey keyring-key.pem -in keyring-cert.pem -passout pass: -out keyring.pfx
base64 -w0 keyring.pfx        # the value of DATA_PROTECTION_CERTIFICATE
shred -u keyring-key.pem keyring-cert.pem
```

One certificate per environment (staging and production never share one). It is self-signed; its validity dates are not checked, so it keeps working after `-days` — rotate for other reasons (§6.5), not because of the date. Store `keyring.pfx` (or the base64 value) in the password manager of the project, **not** next to the database backups, then set the value in Dokploy and redeploy.

### 6.3 Backup

- The table is part of the nightly `pg_dump` (ARCHITECTURE §10.4); nothing extra to do for it.
- The certificate is backed up once, when it is created, and again after each rotation — in the password manager, apart from the database backups. Whoever holds both can read the keys; whoever holds only one cannot.
- After a rotation, keep the previous certificate as long as it is set in `DATA_PROTECTION_PREVIOUS_CERTIFICATE`.

### 6.4 Restore

1. Restore the database from the backup as usual (§10.4).
2. Set `DATA_PROTECTION_CERTIFICATE` (and, if one was set at the time of the backup, `DATA_PROTECTION_PREVIOUS_CERTIFICATE`) to the values that belonged to that database.
3. Start the API. It reads the restored keys: links and sessions issued before the backup stay valid.

Without the right certificate the API still starts, but cannot read the old keys: it creates a new key, so outstanding e-mail links and (from step 3) all sessions end — users request a new link or sign in again. No other data is lost. The same happens when the table is missing from the restore.

### 6.5 Rotation

When the certificate may have leaked, or someone who had it leaves:

1. Create a new certificate (§6.2).
2. Move the current value to `DATA_PROTECTION_PREVIOUS_CERTIFICATE`, set the new one as `DATA_PROTECTION_CERTIFICATE`, redeploy. New keys are encrypted with the new certificate; the keys of before stay readable through the previous one.
3. Keep the previous certificate for at least 100 days (the 90-day key lifetime plus the lifetime of the payloads made with the last old key), then remove the variable and redeploy.

Removing it earlier only makes data protected before the rotation unreadable (links, sessions); new data keeps working. After a leak, the faster and stricter way is to skip step 3: set only the new certificate, so every old key is unusable at once and everybody signs in again.

### 6.6 One-off: delete the keys stored in plain text (O-S4-89)

A database that ran before S4-26 holds keys in plain text, and Data Protection keeps reading and using them: its current plain key stays the default key until it expires (up to 90 days), so setting the certificate alone encrypts nothing yet. Once the API runs with the certificate (staging; any kept database that should be protected), delete the plain keys and restart:

```sql
-- the plain keys (no <encryptedSecret>)
SELECT "Id", "FriendlyName" FROM identity."DataProtectionKeys" WHERE position('<encryptedSecret' IN "Xml") = 0;
DELETE FROM identity."DataProtectionKeys" WHERE position('<encryptedSecret' IN "Xml") = 0;
```

Restart the API right after the delete: it caches the key ring, and on the restart it finds no key and creates a new, encrypted one (check with the query above: no row left, one new row with `<encryptedSecret`). E-mail links sent before the deletion stop working once. Backups made before the deletion still hold the plain keys; they go with the backup retention.

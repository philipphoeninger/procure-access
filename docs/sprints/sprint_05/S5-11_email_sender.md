# S5-11 — IEmailSender with MailKit, Mailpit and real recipients

> **Sprint:** 05 · **Task:** S5-11 (3 points) · **Issues:** #302 (closed by this task)
> **Requirements / invariants:** SPEC §7.6, SD13 (base of R-AUTH-01, R-AUTH-02, R-AUTH-04, R-ORG-02); **PRV-11 → `active`** for its integration part (TESTING_v59 V184), E2E part deferred to the first rebuilt screen that sends mail (S5-12 or S5-14); ARC-01 – ARC-03 stay green with the new reference SERVICES → `Platform.Contracts`
> **Status:** done — pull request #332 open (2026-11-14)
> **Scope:** `IEmailSender` in Platform with MailKit over SMTP/TLS (synchronous, one retry), configured only from the environment; the four message types of sprint 05 rendered from the UX copy's `email.*` texts as a plain-text and an HTML part, no tracking pixel, no rewritten link, no recipient in the logs above `Debug`; Mailpit in the dev and E2E stacks; the legacy confirmation and reset mails to the user's own address (#302); the E2E `compose.log` artefact checked for secrets (TD-12). No EF migration.
> **Owner decisions (2026-11-14):** 1 (A) — plain-text **and** HTML part (the prompt's wording; it overrides the first answer (a), so SPEC §7.6 stays as it was) · 2a — the texts live in Platform as the third `check-i18n` target; only the message types with a sender in sprint 05 (confirmation, reset, „already registered“, invitation) · 3a — the legacy mails go through the new `IEmailSender` with the `email.*` texts; the legacy mail service, templates and settings are removed · 4a — strict settings: only Development and E2E may send without credentials or TLS · Mailpit (MIT) approved (B)

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `UserService.SignUpAsync` and `POST /api/forgot-password` passed Lettermint's test address `ok@testing.lettermint.co` instead of the user's; both look the user up by that address first, so no mail was sent at all (#302) | The real address, with a recipient test (decision 3a) |
| 2 | The legacy `EmailService` (MailKit 5, HTML only, STARTTLS) and the file-based `EmailTemplateService` with English HTML templates lived in SERVICES and MODELS; the mails were English | Replaced by Platform's sender and the UX copy texts |
| 3 | The settings were `Email:Host/Port/Username/Password/From/DisplayName`, not SPEC §7.6's `Email:Smtp:*`; without credentials the API started and every send failed at runtime | SPEC's names, `Security`, and the startup rule of decision 4a |
| 4 | No Mailpit anywhere; the E2E override only blanked the credentials | Mailpit in the dev and E2E overrides |
| 5 | UX copy §5.17 says „Nur Text, keine Bilder“, SPEC §7.6 „plain text + simple accessible HTML“ | Asked; decision 1 (A): both parts, the HTML built from the same text without images |
| 6 | The UX copy writes line breaks in a table cell as the two characters `\n`; `check-i18n --write` copies them as they are | The composer turns them into line breaks |
| 7 | The `catalog` and `roles` command hosts call `AddPlatformModule` | E-mail registration separate (`AddPlatformEmail`), web host only — otherwise 4a would make `catalog import` on staging need SMTP settings |
| 8 | The local disk had 296 MB free (1.5 GB after pruning dangling images): too little to build the E2E stack images | Real-app check with `dotnet run`, a throwaway PostgreSQL and Mailpit instead (§4) |

## 2. Changes

### 2.1 Platform: contract, texts, rendering, sending

- **`Platform.Contracts/Email`:** `IEmailSender.SendAsync(EmailMessage, CancellationToken)`; `AddressConfirmationEmail(recipient, link)`, `PasswordResetEmail(recipient, link, validHours)`, `AlreadyRegisteredEmail(recipient, link)`, `OrganisationInvitationEmail(recipient, organisationName, InvitationRole, link, validDays)`; `InvitationRole` (Owner/Editor/Reader → `org.members.role.*`); `EmailDeliveryException` (no constructor takes a cause). The message types are classes, not records: a record's `ToString` would print the recipient and the link's token into any log that formats the object; `ToString` returns the type name.
- **Texts:** `scripts/check-i18n.mjs` has a third target, `EMAIL_AREAS` (`email` and the three role labels; each label brings its `.hint`, which no mail uses, and the `email` area brings the two product-request texts, unused until their step) → `src/ProcureAccess.Modules.Platform/Email/Texts/de.json`, embedded; `EmailTexts` stops the start when a key of `Required` is missing.
- **`EmailComposer`:** fills `{{…}}` (a placeholder its type does not fill, or a value its text does not use, is an error — so a changed UX copy text fails the tests, not a user), appends `email.footer`, refuses values with a line break or another control character (no extra line, paragraph or header), requires absolute http(s) links. HTML part: `<!DOCTYPE html>`, `lang="de"`, the subject as `title`, one `p` per paragraph, `br` between lines, the line that is the link as the only `a` (the address written out as its text); every value HTML-escaped; no image, style or script. MIME: `multipart/alternative` with UTF-8 `text/plain` and `text/html`, `Date` from `TimeProvider`, `Message-Id` on the sender's domain, one `To` without display name.
- **`SmtpEmailSender`:** synchronous, two attempts (one retry), then `EmailDeliveryException`; cancellation is not retried. An address MimeKit cannot parse (Identity accepts e.g. `a b@example.test`) fails the same way without an attempt (review finding R1). Logs: `Information` „E-mail {MessageType} sent“, `Warning`/`Error` with the message type, the exception type and the SMTP status code only; recipient and exception only at `Debug`.
- **`MailKitSmtpTransport`:** one connection per mail; STARTTLS required (not „when available“) unless `Security=None`; signs in only with credentials; MailKit 5's `SendRequest`; no protocol logger.
- **`EmailSettings`:** `Email:Smtp:Host`, `:Port`, `:Username`, `:Password`, `:Security` (`StartTls` default, `SslOnConnect`, `None`), `Email:From`, `Email:DisplayName`. Everywhere: host, port 1–65535, one sender address, username and password together. Outside `Development` and `E2E`: credentials required, `None` refused. Not a record (its `ToString` would print the password); no error message quotes a value.
- **Registration:** `PlatformModule.AddPlatformEmail(configuration, environment)`, called in `Program.cs` after `AddPlatformModule`; MailKit 5.0.0 moved from SERVICES to Platform (same version; the API notices file is unchanged).

### 2.2 Legacy flows (#302, decision 3a)

- `UserService` sends `AddressConfirmationEmail` and `PasswordResetEmail` to `user.Email` (the stored address) through `IEmailSender`. The reset names the real lifetime of the legacy Identity token (`DataProtectionTokenProviderOptions.TokenLifespan`, 24 h); the address in the reset link and the user id in the confirmation link are now URL-escaped. A failed send logs a fixed warning without the address; sign-up still answers 200 and keeps the account, forgot-password answers the same for every address.
- `POST /api/forgot-password` passes `request.Email`.
- Removed: `EmailService`, `IEmailService`, `EmailTemplateService`, `IEmailTemplateService`, `MODELS/Entities/EmailSettings.cs`, `MODELS/EmailTemplates/*.html` (with the csproj `Content` item and the `REUSE.toml` entry), `AppConfigExtensions.AddEmailConfig` (the file held nothing else), the MailKit global usings of SERVICES.
- SERVICES references `ProcureAccess.Modules.Platform.Contracts`: ARC-03 restricts references to module projects only, and the architecture tests pass. `UserService` aliases `IEmailSender` because SERVICES' global usings also import Identity UI's `IEmailSender`.

### 2.3 Configuration and stacks

- `appsettings.json`: `Email:Smtp` (Lettermint, 587, `StartTls`, empty credentials), `Email:From`, `Email:DisplayName`.
- `docker-compose.yml`: `Email__Smtp__Username/Password` from `SMTP_USERNAME`/`SMTP_PASSWORD` (`.env` unchanged; `.env.example` comment rewritten).
- `docker-compose.dev.yml`: Mailpit (`axllent/mailpit:v1.33.1@sha256:aef2aacc…`, MIT) with UI `127.0.0.1:8025` and SMTP `127.0.0.1:1025` (for `dotnet run`); the API sends to `mailpit:1025` with `Security=None`, `App__BaseUrl=http://localhost:4200`.
- `docker-compose.e2e.yml`: the same Mailpit with UI/API on `127.0.0.1:8025` (for S5-14), the API sends there with empty credentials; header comments updated.

### 2.4 Tests

- **Unit** (`tests/ProcureAccess.Tests.Unit/Email/`, 45): `EmailSettingsTests` (environment rule, impossible values, no value in a message), `EmailComposerTests` (each type's text, HTML with only its own link and no image/style/script, escaping, refused control characters and non-http links, placeholder drift, MIME shape, recipient refusal without quoting, `ToString`), `EmailTextsTests`, `SmtpEmailSenderTests` (once, retry, failure after two attempts without cause, no retry on cancellation, unusable recipient without an attempt).
- **Integration:** `Platform/EmailDeliveryTests` (`INV:PRV-11`): a Mailpit Testcontainer (`MailpitContainer`, one per class, mailbox emptied per test, accepts only `@example.test`) receives every message type through the real `SmtpEmailSender`/MailKit; the mail is read back raw and parsed — exactly the rendered texts, two alternatives, only the message's own link, no `<img`/`src=`, no header beyond the message's own (plus Mailpit's `Received`/`Return-Path`), recipient only at `Debug`; a refused recipient gives `EmailDeliveryException` after two attempts and no address above `Debug`. `IdentityAccess/LegacyAccountEmailTests` (#302): sign-up mails the confirmation to the new address; confirm via the captured link, then forgot-password mails the reset to that address (24 h); an unknown address gets nothing and the same 200; a failed send keeps the account.
- **Host:** every `ProcureAccessApiFactory` sets dummy SMTP credentials (`Testing` needs them under 4a) and replaces `IEmailSender` with `CapturingEmailSender` (`Factory.Emails`), so no test reaches an SMTP server.

### 2.5 Documents

`TESTING_v59.md` (PRV-11 `active`, V184–V187, §3.1 SMTP row, §3.3 E-mail row and state paragraph), `ARCHITECTURE_v52.md` (C138; C71 and the §4.3 Platform row point to it; `ARCHITECTURE_v50.md` deleted), `SPEC_v2.md` v2.20 (§7.6 rows, change rows v2.20-1 – -4), `CLAUDE.md` §3/§4/§5. No Figma mapping, Handoff, UX copy or `LEGACY_EXCEPTIONS` change (no client code).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: no secret in code or compose (credentials only from `.env`); header injection impossible (control characters refused, MimeKit encodes headers); HTML injection impossible (every value escaped, only the link becomes an `a`); recipient and SMTP answers only at `Debug`; settings errors quote no value; the exception carries no cause. N+1: none. Accessibility: the HTML part has `lang="de"`, a title, real paragraphs and the link text is the full address (UX copy §5.17).

| # | Finding | Sorting | Reason / fix |
|---|---|---|---|
| R1 | Identity accepts addresses MimeKit cannot parse (`a b@example.test`); the composer threw `ArgumentException`, so the legacy sign-up answered 500 after storing the account (before: 200) | fix | The sender treats it as a failed send (`EmailDeliveryException`, no attempt, warning without the address); test `SendAsync_RecipientSmtpCannotUse_…` |
| R2 | `EmailDeliveryException(string, Exception)` let a caller attach the SMTP error, whose text can quote the address — against its own doc | fix | Constructor removed |
| R3 | MailKit's default timeout is 2 minutes per operation; with the retry a stalled SMTP server can hold a request for several minutes | follow-up | Needs a 🔧 value nobody has confirmed yet (CLAUDE §6.6); open point 3 |
| R4 | `forgot-password` takes longer for a known, confirmed address (SMTP round trip) than for an unknown one: a timing difference behind the same answer | follow-up | Pre-existing in the legacy flow; S5-13 rebuilds the reset (R-AUTH-04 „same response“) — open point 4 |
| R5 | `WebUtility.HtmlEncode` writes umlauts as numeric entities (`&#228;`) | won't fix | Valid HTML with `charset=utf-8`; every mail client renders it; no reader-visible difference |
| R6 | The E2E state paragraph of TESTING §3.3 still lists the relaxed rate limit as missing (it exists since S4-21a) | won't fix here | Not this task's text; open point 7 for the documentation session |

No correctness or security bug that affects current behaviour is left: merge recommended after CI.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror` (SDK 10.0.404) | succeeded, no warning |
| `dotnet format ProcureAccess.sln --verify-no-changes` | clean |
| `dotnet test --solution ProcureAccess.sln` | 1001 passed (unit, architecture, integration), after the review fixes |
| `node scripts/check-invariants.mjs` | passed (TESTING_v59, PRV-11 found) |
| `node scripts/check-i18n.mjs` / `--test` | 0 errors in all three targets (UX copy v27); self-test 15 passed |
| `node scripts/check-figma-links.mjs` | 6 checked, 0 errors |
| `node scripts/check-api-notices.mjs` | 39 packages, 0 errors (MailKit moved, set unchanged) |
| `docker compose … config` (dev and E2E overrides) | valid |
| `reuse lint` (Docker) | compliant |
| Real app (instead of the E2E stack, see §1 #8) | API of this branch with `dotnet run` in `Development` against a throwaway PostgreSQL 17 and Mailpit (dev stack untouched): `POST /api/signUp` 200 → one mail in Mailpit to the registered address, subject „Bitte bestätigen Sie Ihre E-Mail-Adresse“, `multipart/alternative`, link to `http://localhost:4200/confirm-email?…`; the link's API path confirmed the address (200); `forgot-password` for it → „Passwort zurücksetzen“ to the same address, for an unknown address 200 and no mail |
| E2E (Playwright) | not run locally (disk); no client or E2E file changed — CI runs the five projects on the new E2E stack with Mailpit |

**TD-12 — the E2E `compose.log` artefact.** It is uploaded only when an E2E job fails and holds the logs of `db`, `api`, `client` and now `mailpit`. Secrets of the E2E stack: `DB_PASSWORD` (random per run, masked in the job log); no SMTP credentials (Mailpit needs none, the override sets them empty); no key-ring certificate (Development). In the real-app run above none of the logs held the database password, a confirmation or reset token, or the recipient address: the API console shows `Warning` and above, the PostgreSQL log never prints the password, and Mailpit logs only its start-up lines (no recipients, no bodies). Result: **no secret in the artefact; TD-12 can be closed** at the sprint close-out. It must be looked at again only if the E2E stack ever gets a real secret (it never sends through Lettermint).

## 5. Open points

1. **Owner, before mail goes out from staging (#236 / TD-28):** set `SMTP_USERNAME`/`SMTP_PASSWORD` on staging — under 4a the API no longer starts without them; check that open and click tracking are off in the Lettermint project (SPEC §7.6); and check `App:BaseUrl`, which builds the links: `appsettings.json` holds `http://procureaccess.tech:4200`, so staging needs `App__BaseUrl` with its real HTTPS origin.
2. **E2E part of PRV-11** — with the first rebuilt screen that sends mail: registration S02 (S5-12) or the account journey (S5-14), reading Mailpit's API on `127.0.0.1:8025`.
3. **SMTP timeout (R3)** — proposed issue: a 🔧 `Email:Smtp:Timeout` (e.g. 15 s per operation) so a stalled server fails a request in seconds, not minutes; needs the owner's value.
4. **Timing of forgot-password (R4)** — S5-13 should answer before sending (or send in the background with the same delay), so a known address cannot be told from an unknown one by the response time.
5. **„24 Stunden“ in `email.confirm.body`** — the UX copy writes the lifetime as text while SPEC makes it 🔧 (R-AUTH-02); a `{{hours}}` placeholder as in `email.reset.body` would keep them in step. UX copy is the owner's document: point for S5-12.
6. **Address in the legacy reset link** — the legacy reset page needs `email=` in the URL; S5-13's S03 should carry the token only (PRV-11 „no data beyond the purpose“).
7. **TESTING §3.3 state paragraph** still names the relaxed rate limit as missing (R6) — for the next documentation session.
8. **#301** — the legacy sign-up needs the `Member` role, which only the Development rebuild creates; the integration test adds it as `RoleCommandTests` does. S5-12 decides it (O-S5-3).

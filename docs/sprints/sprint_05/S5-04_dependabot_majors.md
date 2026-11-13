# S5-04 — Review the Dependabot majors MailKit 5 and Swashbuckle 11

> **Sprint:** 05 · **Task:** S5-04 · **Pull requests reviewed:** #288 (MailKit 5.0.0), #289, #291, #292 (Swashbuckle 11.0.1); TD-42, S4-27 P1
> **Requirements / invariants:** none activated; CLAUDE §10 and ARCHITECTURE D12 (licence rule); the notices check of CI stage 2 (ARCHITECTURE C132). No TESTING version
> **Status:** pull request open (2026-11-13); the four Dependabot pull requests stay open until it is merged
> **Scope:** per package the licence, the changelog for breaking changes, CI, the regenerated `THIRD-PARTY-NOTICES.txt`, and for `Swashbuckle.AspNetCore.SwaggerUI` the `EMBEDDED` review of the swagger-ui-dist it bundles; any other open Dependabot pull request (there was none). Not here: the legacy `EmailService` beyond what MailKit 5 needs to compile (S5-11 replaces it, §5)
> **Owner decisions:** **O-S5-2 (2026-11-13):** MailKit 5: take; Swashbuckle 11: one pull request for all packages; one combined pull request for both, the Dependabot pull requests closed after its merge. Asked before the prompt, because every one of the four Dependabot pull requests failed CI as opened. Plan stated before the change; fits one session, no EF migration

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Open Dependabot pull requests on 2026-11-13: only #288, #289, #291, #292 (opened 2026-11-12) | Nothing else to include |
| F2 | #288 failed stage 2 and every E2E job: `EmailService.cs(51,34): CS1503 cannot convert from 'MimeKit.MimeMessage' to 'MailKit.ISendRequest'` | MailKit 5 moved `Send(MimeMessage)` to extension methods in the namespace `MailKit` (§1.1) |
| F3 | #289 and #291 failed stage 2: `SwaggerDefaultValuesTests.cs(158,19): CS0266 … Dictionary<string, OpenApiMediaType> to IDictionary<string, IOpenApiMediaType>` | Microsoft.OpenApi 3 types `Content` with the interface (§1.2) |
| F4 | #292 (SwaggerUI alone) shows no failed check, but cannot be merged alone: the five `Swashbuckle.*` packages share one version, and the `EMBEDDED` entry pins 10.3.1 | One change for all of them (O-S5-2) |
| F5 | The meta-package `Swashbuckle.AspNetCore` is referenced too and no Dependabot pull request moves it | Moved with the others to 11.0.1 |
| F6 | All four ran CI before S4-27 (notices check) and S5-03 (SDK pin) were merged | The notices file had to be regenerated in any case (S4-27 P1) |
| F7 | No e-mail test exists (no test references `IEmailService`); TECH_DEBT_v4 §6 asked to „run the e-mail tests“ | The MailKit change is checked by build, the unchanged call and the changelog; tests come with S5-11 on MailKit 5 (§5 P1) |

### 1.1 #288 — MailKit 4.18.2 → 5.0.0

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `MailKit` 5.0.0 | MIT | **Yes** (API image, legacy `EmailService`) | Release notes of 2026-11-10 (`ReleaseNotes.md` of the repository; Dependabot found none). Affects us: the send API is built around `ISendRequest`; `Send(MimeMessage, …)`/`SendAsync` are now extension methods in `MailKit` — compile with `using MailKit;`, same behaviour; they return a `SendResult` instead of a string (the code ignores it). `SmtpClient` throws `SmtpServiceNotAuthenticatedException` (derives from `ServiceNotAuthenticatedException`) on `530`; multi-line SMTP responses are joined with `\r\n` — the code catches `Exception` and logs it, so neither changes anything. Not used here: IMAP/POP3 (`COMPRESS`, 64-bit sizes, quotas, annotations, `Enable`), the capability enums, SASL `Create`, `DeliveryStatusNotificationType` moved, removed obsolete APIs and serialisation constructors, telemetry `error.type` values |
| `MimeKit.Core` 5.0.0 (new, replaces `MimeKit` 4.18.2) | MIT | **Yes** | MailKit 5 depends on `MimeKit.Core` instead of the full `MimeKit`; S/MIME, PGP, DKIM and ARC would need `MimeKit.Cryptography` and `CryptographyModule.Initialize()` — not used. `MimeMessage`, `BodyBuilder`, `MailboxAddress` compile unchanged |
| `BouncyCastle.Cryptography` 2.7.0 (removed) | MIT | was shipped | No longer a dependency; one package and one licence text less in the image |

No GitHub advisory names `MailKit` 5.0.0 or `MimeKit.Core` 5.0.0 (2026-11-13).

### 1.2 #289, #291, #292 — Swashbuckle 10.3.1 → 11.0.1

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `Swashbuckle.AspNetCore`, `.Annotations`, `.Swagger`, `.SwaggerGen`, `.SwaggerUI` 11.0.1 | MIT | **Yes** (served in `Development` only, O-S3-2) | Release notes 11.0.0/11.0.1 and `docs/migrating-to-v11.md`: targets .NET 10 and 11 only (we are on 10); depends on Microsoft.OpenApi 3; still writes **OpenAPI 3.0** unless `OpenApiVersion` opts into 3.1/3.2 (not done); multiple response types per status code change only on .NET 11; Redoc 3 (not used); 11.0.1 adds `SubmitMethod.Query` |
| `Microsoft.OpenApi` 2.7.5 → 3.11.0 (transitive) | MIT | **Yes** | v3: `OpenApiResponse.Content`/`OpenApiRequestBody.Content` are `IDictionary<string, IOpenApiMediaType>`; `OpenApiXml.Attribute`/`Wrapped` replaced; `IOpenApiSchema.Example` obsolete. Our `SwaggerDefaultValues` only reads keys and removes entries — compiles unchanged; one test helper builds the dictionary (F3). CVE-2026-49451 (GHSA-v5pm-xwqc-g5wc) is fixed in 2.7.5 and in 3.5.4, so 3.11.0 is patched |

**Generated document unchanged:** `GET /swagger/v1/swagger.json` in `Development`, dumped once on 10.3.1 and once on 11.0.1 through a temporary, uncommitted line in `SwaggerDocumentTests`, is the same JSON value (`json.load` equal; 59 356 bytes).

### 1.3 `EMBEDDED` review — swagger-ui-dist 5.33.3 → 5.34.1 (S4-27 P1, O-S4-92)

| Check | Result |
|---|---|
| Which version 11.0.1 embeds | `src/Swashbuckle.AspNetCore.SwaggerUI/package.json` and `package-lock.json` at tag `v11.0.1`: `swagger-ui-dist` **5.34.1** (exact pin; 5.33.3 at `v10.3.1`) |
| What it embeds | The manifest resource names of `Swashbuckle.AspNetCore.SwaggerUI.dll` (net10.0) are the same 14 files in 10.3.1 and 11.0.1 (`index.html`, `index.js`, and from swagger-ui-dist `absolute-path.js`, two favicons, `index.css`, `index.js`, `oauth2-redirect.html/.js`, `swagger-initializer.js`, `swagger-ui-bundle.js`, `swagger-ui-standalone-preset.js`, `swagger-ui.css`, `swagger-ui.js`); still no `*.LICENSE.txt` |
| Licence texts of 5.34.1 vs 5.33.3 (npm tarballs) | `LICENSE` (Apache-2.0), `NOTICE` and `swagger-ui-standalone-preset.js.LICENSE.txt` byte-identical; `swagger-ui-bundle.js.LICENSE.txt` differs in one line: DOMPurify 3.4.16 → **3.4.17** (still „Apache license 2.0 and Mozilla Public License 2.0“; the project uses the Apache-2.0 option, CLAUDE §10) |
| Reviewed entry | `scripts/api-notices/swagger-ui-dist.txt`: the version in the header and the bundle's comments replaced by the 5.34.1 file verbatim (before: the 5.33.3 file verbatim, checked); `EMBEDDED` → `version: '11.0.1'`, `swagger-ui-dist 5.34.1` |

### 1.4 Summary

| PR | Recommendation | Reason in one line |
|---|---|---|
| #288 | **Close after this pull request is merged** | Taken here with `using MailKit;`; MIT, MimeKit.Core MIT, BouncyCastle gone |
| #289 | **Close after merge** | Taken here with the other Swashbuckle packages and the test helper fix |
| #291 | **Close after merge** | Same |
| #292 | **Close after merge** | Same, with the `EMBEDDED` review |
| this pull request | **Merge** when CI is green | Two majors with compile-only fixes, licences unchanged (MIT/Apache-2.0), document unchanged, all checks green |

`Closes #n` does not close a pull request, so the four are closed by hand after the merge, with a link to this pull request.

## 2. Changes

| File | Change | Reason |
|---|---|---|
| `SERVICES/SERVICES.ProcureAccess/SERVICES.ProcureAccess.csproj` | `MailKit` 4.18.2 → 5.0.0 | #288 |
| `SERVICES/SERVICES.ProcureAccess/GlobalUsings.cs` | `global using MailKit;` next to the other MailKit usings | F2: the `SendAsync(MimeMessage)` extension lives there |
| `API/API.ProcureAccess/API.ProcureAccess.csproj` | All five `Swashbuckle.*` references → 11.0.1; the comment names Microsoft.OpenApi 3.11.0, the two fixed versions of the CVE and that the documents stay OpenAPI 3.0 | #289, #291, #292, F5 |
| `tests/ProcureAccess.Tests.Unit/Swagger/SwaggerDefaultValuesTests.cs` | `Response()` builds `IDictionary<string, IOpenApiMediaType>` (lambda return type) | F3 |
| `scripts/check-api-notices.mjs` | `EMBEDDED`: version 11.0.1, swagger-ui-dist 5.34.1 | §1.3 |
| `scripts/api-notices/swagger-ui-dist.txt` | Version 5.34.1; DOMPurify line of the 5.34.1 bundle | §1.3 |
| `API/API.ProcureAccess/THIRD-PARTY-NOTICES.txt` | Regenerated (`--write`, SDK 10.0.404): 49 → 48 packages; `BouncyCastle.Cryptography` and its `LICENSE.md` gone; `MimeKit` → `MimeKit.Core` 5.0.0; MailKit 5.0.0, Microsoft.OpenApi 3.11.0, Swashbuckle 11.0.1, swagger-ui-dist 5.34.1; text numbers shift by one. Diff reviewed line by line | S4-27 P1 |
| `CLAUDE.md` | Header line; §3 „the 49 shipped packages“ → 48 since MailKit 5 | Package count |

Not changed: ARCHITECTURE (C132 names no versions), SPEC, TESTING (no invariant), Figma mapping, `LEGACY_EXCEPTIONS`, `.github/dependabot.yml` (both majors taken, nothing to ignore), the API Dockerfile (no new project).

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | `global using MailKit;` brings every type of the namespace into all of `SERVICES` (e.g. `UniqueId`, `MessageFlags`), so a later type of the same name would be ambiguous | **Won't fix** | Builds without a conflict today; the other MailKit usings are global too; S5-11 moves e-mail into IdentityAccess and the legacy project shrinks |
| R2 | The legacy `EmailService.SendEmailAsync` is `async Task<Task>` and returns `Task.CompletedTask` | **Won't fix** (outside the scope) | Older than this change and harmless (callers await the outer task, which completes after the send); S5-11 replaces the class |
| R3 | No automated test sends a mail through MailKit 5 | **Follow-up** (§5 P1) | None existed for 4.x either; S5-11 brings Mailpit and the e-mail tests on MailKit 5 |
| R4 | `swagger-ui.js` (non-bundle) is embedded without a licence-comment file of its own | **Won't fix** | Unchanged since S4-27 (the dist ships none for it); the bundle's file covers its dependencies |

Security: SMTP still connects with `StartTls` and authenticates as before; the new OpenAPI library is past the fix of CVE-2026-49451; Swagger stays `Development` only. Licences: every new or changed shipped package is MIT, swagger-ui-dist Apache-2.0, DOMPurify Apache-2.0/MPL-2.0 dual as before. No correctness or security finding that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` (SDK 10.0.404) | 0 warnings, 0 errors |
| C2 | `dotnet format ProcureAccess.sln --verify-no-changes` | Clean |
| C3 | `dotnet test --solution ProcureAccess.sln` (Testcontainers) | 951 passed, 0 failed, 0 skipped — includes `SwaggerDefaultValuesTests` and `SwaggerDocumentTests` |
| C4 | `swagger.json` in `Development` on 10.3.1 and 11.0.1 (temporary dump, reverted) | Identical |
| C5 | `node scripts/check-api-notices.mjs` after `--write` | 48 packages, 0 errors |
| C6 | `docker build` of the API image; `check-api-notices.mjs --deps` with the image's `deps.json`; `/app` listing | Builds; 48 packages, 0 errors; `MailKit.dll`, `MimeKit.Core.dll`, `Microsoft.OpenApi.dll`, four Swashbuckle DLLs, no BouncyCastle |
| C7 | `node --test` of the six script self-tests (`check-api-notices`, `-invariants`, `-figma-links`, `-i18n`, `-icons`, `-release-config`) | 20, 36, 10, 15, 13, 6 passed |
| C8 | `check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` | Green |
| C9 | `reuse lint` (Docker image) | Compliant |
| C10 | E2E | Not run locally (client untouched; the E2E jobs build the API image) — see CI of the pull request |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | E-mail through MailKit 5 is not covered by a test (R3); S5-11 builds `IEmailSender` on MailKit 5 with Mailpit and recipient tests | S5-11 |
| P2 | Close #288, #289, #291, #292 with a link to this pull request once it is merged | Owner, after the merge |
| P3 | TD-42 is resolved by this pull request; TECH_DEBT is updated at the sprint close-out | Sprint 05 close-out |
| P4 | Swashbuckle 11 can write OpenAPI 3.1/3.2 (`OpenApiVersion`); staying on 3.0 needs no decision now | Only if a consumer of the document needs it |

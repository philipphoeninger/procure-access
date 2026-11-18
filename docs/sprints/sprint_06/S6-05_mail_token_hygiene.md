# S6-05 — Mail and token hygiene: SMTP timeout, token clean-up, nginx error log

> **Sprint:** 06 · **Task:** S6-05 (1 point) · **Issues:** #382 (TECH_DEBT_v5 TD-58; S5-11 P3), #383 (TD-59; S5-12a P4), #381 (TD-57; S5-23 P1)
> **Requirements / invariants:** SPEC §7.6, §8.8 (R-AUTH-02, R-AUTH-04); SEC-10, SEC-11, PRV-11 and TEN-08 stay green; no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-18)
> **Scope:** exactly the row of S6-05. Backend (Platform e-mail, IdentityAccess tokens) and the client image's `nginx.conf`; no EF migration, no new dependency, no UX copy key, no screen. E2E was not run locally, because another session used the local E2E stack at the same time; it runs in CI.
> **Owner decisions:** O-S6-3 (2026-11-18): 🔧 `Email:Smtp:Timeout` = 15 s per SMTP operation, validated at startup and applied to the MailKit client. Task decisions approved in the session (2026-11-18, „Go ahead with your recommendations“): **D1 (a)** a token row, used or not, is deleted at its expiry (like the invitations, S5-17 D7 (a)), no retention setting; **no index** on `ExpiresAt`, so no migration; nginx `error_log` at **`crit`** for the whole `server` block.

## 0. Plan (written before the code)

- **Starting point:** `main` at c66f601 (after S6-03, S6-08). Branch `sprint06/S6-05-mail-token-hygiene`.
- **TD-58:** `EmailSettings.Timeout` from the 🔧 setting, `SmtpClient.Timeout` in `MailKitSmtpTransport`; a fake SMTP server that stalls before the greeting, at the sign-in and at the send.
- **TD-59:** `AccountTokenCleanupService` like `InvitationCleanupService`, `Account:Tokens:CleanupInterval` (1 h, operations setting); tests of the rule and of the hosted service on `FakeTimeProvider`. Open question D1 (when a row may go) and the index question put to the owner.
- **TD-57:** `error_log … crit` in the `server` block; a per-location rule would miss the referrer. Before/after log on `nginx:1.30-alpine` without host ports.
- **Docs:** SPEC v2.49, ARCHITECTURE v86 (C172 – C174), CODEBASE §2.93 and §3. **Invariants:** none activated. **Migration:** none. One session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `MailKitSmtpTransport` created `new SmtpClient()` with MailKit's default `Timeout` of 120 000 ms per network operation; `SmtpEmailSender` tries twice, so a stalled server held the registration, the confirmation resend or an invitation for about four minutes (#382) | Bound every operation by the 🔧 setting |
| F2 | `identity."AccountTokens"` has indexes on `UserId` and `TokenHash`, no query filter; only `AccountDeletionService` deleted rows (#383) | A background clean-up; no TEN-02 allow-list entry needed |
| F3 | `RedeemAsync` answers `AlreadyUsed` for a used row at any age, which the confirmation endpoint turns into 204 for a confirmed account (SPEC §8.8, decision S5-12 4 (b)) | Deleting used rows at once would break the second click → D1 put to the owner |
| F4 | `nginx.conf` masks share tokens in the access log only; the image's main-context `error_log` (`notice`) applies to the server. An upstream error writes `request: "…"` **and** `referrer: "…"` — the shared page's own API calls carry the token in the referrer (§4, before) | A rule for `/api/v*/shared/` alone is not enough; the whole server at `crit` |
| F5 | A plain 404 of the API (unknown share token) is proxied without an error-log line; only upstream failures (502/504) are logged (§4) | The proof shows both the 404 and the 502 case |

## 2. Changes

1. **SMTP timeout (commit 1, TD-58).** `EmailSettings` reads `Email:Smtp:Timeout` and stops the start unless it is a duration of at least 1 ms and at most `int.MaxValue` ms (MailKit's type); `appsettings.json` holds `00:00:15` (O-S6-3). `MailKitSmtpTransport` sets `SmtpClient.Timeout` from it, which bounds connect, greeting, sign-in and each command of the send. The retry is unchanged, so a stalled server ends the request after about 2 × 15 s. The in-memory configurations of `SmtpEmailSenderTests` and `EmailDeliveryTests` set the value. New `SmtpTimeoutTests` (integration project, no container): a loopback `TcpListener` that stalls before the greeting, at `AUTH` or at `MAIL FROM`, a 1 s timeout; each attempt ends within the bound, and `SmtpEmailSender` ends with `EmailDeliveryException` after two connections. `EmailSettingsTests`: the value is read, `appsettings.json` holds 15 s, empty/zero/negative/too large stop the start. SPEC v2.49-1, ARCHITECTURE C172.
2. **Token clean-up (commit 2, TD-59).** `IdentityAccess/Accounts/AccountTokenCleanupService.cs`: at startup and every `Account:Tokens:CleanupInterval` (`01:00:00`) on a `PeriodicTimer` over `TimeProvider`, `DeleteExpiredAsync` runs one `ExecuteDeleteAsync` over `ExpiresAt <= now` (D1 (a)); a failed run is logged and retried, it never stops the host; registered in `IdentityAccessModule` next to the token service. `AccountTokenSettings.CleanupInterval` is read and validated like the lifetimes. Tests: `AccountTokenCleanupTests` — expired unused, expired used and expiring-now rows deleted, valid unused and valid used kept; the hosted service deletes a token that expires within one interval after `Clock.Advance(interval)` and keeps a valid one; `AccountTokenTests` — the interval is read and required. SPEC v2.49-2 (§8.8: the second click on a used confirmation link answers 204 until the clean-up deletes the expired token), ARCHITECTURE C173 and §9.1.
3. **nginx error log (commit 3, TD-57).** `CLIENT/CLIENT.ProcureAccess/nginx.conf`: `error_log /var/log/nginx/error.log crit;` in the `server` block, with the reason in a comment. ARCHITECTURE C174 and §9.5.
4. **Docs.** ARCHITECTURE v86 (v84 deleted), SPEC v2.49, CODEBASE §2.93 and §3, this protocol. TECH_DEBT stays at v5 (new version at the sprint end, S6-09); SPRINT_06 unchanged (finished tasks are not marked there).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security and privacy: no new endpoint, no new log of an address — the sender's existing logs (PRV-11) are unchanged, and the clean-up logs only a count. The clean-up removes token hashes, never adds data. Fewer secrets in logs (nginx). Correctness: the delete rule uses the same boundary as `RedeemAsync` (`ExpiresAt > now` is valid), so a token is never deleted while it can still be redeemed. N+1: none (one statement per run). Performance: one sequential scan per hour of a table that holds one lifetime of tokens. Accessibility: no UI change.

| Finding | Sorting | Reason / fix |
|---|---|---|
| The test value `"15"` for the timeout would parse as 15 days and be accepted | fix (test) | Case removed; the CODEBASE paragraph warns about bare numbers. The same holds for every `TimeSpan` setting of the API, so no special check for this one |
| The doc comment of `MailKitSmtpTransport` names `TimeoutException`; the test only checked that the attempt fails and is not a cancellation | fix (test) | `SmtpTimeoutTests` now expects `TimeoutException`, which MailKit throws in all three cases; the change sits in commit 3, because it came out of the review after commit 1 |
| A redemption in the same millisecond as the token's expiry could find its row deleted between its update and its read, and answer „invalid“ | won't fix | The link expires at that moment anyway; the user requests a new one |
| `AccountTokenCleanupService` repeats the timer loop of `InvitationCleanupService` (about 40 lines) | won't fix | Two copies; a shared base class would be a new mechanism the fix does not need (CLAUDE §6.8). A third job may warrant it |
| MailKit's `Timeout` does not bound the DNS lookup of the SMTP host | won't fix | The system resolver bounds it (seconds); Lettermint's host resolves through the platform's DNS |
| At `crit`, the client log no longer says why a request got 502/504 | won't fix (task decision `crit`) | The status stays in the masked access log, the cause in the API's log; nginx's own failures (`crit`) are still written and may quote a request — rare and accepted |
| Tests that advance their host's clock past a token's lifetime (`RegistrationTests`, `PasswordResetTests`) now race the clean-up | won't fix | Harmless: an expired token answers the same with or without its row; both classes pass |

No correctness or security bug that affects current behaviour is left: merge recommended after CI.

## 4. Checks

| Check | Result |
|---|---|
| `dotnet build ProcureAccess.sln -warnaserror …` | 0 warnings, 0 errors |
| `dotnet format ProcureAccess.sln --verify-no-changes` | no changes |
| `dotnet test --solution ProcureAccess.sln` | 1704 tests passed (unit, architecture, integration with Testcontainers); `SmtpTimeoutTests` again after the review fix: 4 passed |
| `SmtpTimeoutTests` without the timeout (control) | the three stall cases ran past 300 s (MailKit's 120 s each) and were stopped — the test fails without the fix |
| `AccountTokenCleanupTests` without the hosted-service registration (control) | the hosted-service case fails after its 30 s wait, the rule case passes |
| `node scripts/check-invariants.mjs` | Invariant gate passed |
| `node scripts/check-figma-links.mjs` | 7 checked, 19 not built yet, 0 pending |
| `node scripts/check-i18n.mjs` | 0 errors (client, export and e-mail texts; UX copy v40) |
| `node scripts/check-icons.mjs` | 0 errors |
| `reuse lint` (`fsfe/reuse:6.2.0`, the CI version) | no finding in a repository file; the container sees no git (worktree), so it also lists the untracked `.env` and build output, which CI does not have |
| nginx log, manual check | below |
| E2E | not run locally — another session used the local E2E stack; left to CI |

**nginx log before and after (TD-57).** `nginx:1.30-alpine` (the client image's base) with `CLIENT/CLIENT.ProcureAccess/nginx.conf` as `conf.d/default.conf` and a one-line `index.html`, on its own Docker network without host ports; a stub `api` (nginx answering 404 on port 8080) for the 404 case, no `api` for the 502 case. Requests from inside the container with `wget`, the share token `AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_ABCDE` (43 characters, invented), the referrer `https://procureaccess.example/geteilt/<token>`.

Before (`main`):

```
# 404: the API does not know the token — access log only, masked
127.0.0.1 - - [18/Nov/2026:12:47:05 +0000] "GET /api/v1/shared/*** HTTP/1.1" 404 153 "https://procureaccess.example/geteilt/***" "Wget"

# 502: the API is down — the error log writes the token in the request line and the referrer
2026/11/18 12:46:58 [error] 22#22: *2 api could not be resolved (3: Host not found), client: 127.0.0.1, server: localhost, request: "GET /api/v1/shared/AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_ABCDE HTTP/1.1", host: "127.0.0.1", referrer: "https://procureaccess.example/geteilt/AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_ABCDE"
127.0.0.1 - - [18/Nov/2026:12:46:58 +0000] "GET /api/v1/shared/*** HTTP/1.1" 502 157 "https://procureaccess.example/geteilt/***" "Wget"
2026/11/18 12:46:58 [error] 23#23: *4 api could not be resolved (3: Host not found), client: 127.0.0.1, server: localhost, request: "GET /api/v1/configurator/settings HTTP/1.1", host: "127.0.0.1", referrer: "https://procureaccess.example/geteilt/AbCdEfGhIjKlMnOpQrStUvWxYz0123456789-_ABCDE"
127.0.0.1 - - [18/Nov/2026:12:46:58 +0000] "GET /api/v1/configurator/settings HTTP/1.1" 502 157 "https://procureaccess.example/geteilt/***" "Wget"
```

After (this branch):

```
# 404
127.0.0.1 - - [18/Nov/2026:12:47:35 +0000] "GET /api/v1/shared/*** HTTP/1.1" 404 153 "https://procureaccess.example/geteilt/***" "Wget"

# 502 (page, share read, another API call of the page)
127.0.0.1 - - [18/Nov/2026:12:47:46 +0000] "GET /index.html HTTP/1.1" 200 32 "-" "Wget"
127.0.0.1 - - [18/Nov/2026:12:47:46 +0000] "GET /api/v1/shared/*** HTTP/1.1" 502 157 "https://procureaccess.example/geteilt/***" "Wget"
127.0.0.1 - - [18/Nov/2026:12:47:46 +0000] "GET /api/v1/configurator/settings HTTP/1.1" 502 157 "https://procureaccess.example/geteilt/***" "Wget"
```

Lines of the container log since the restart with the new configuration that contain the token: **0**. `nginx -t` passes. The containers and the network were removed afterwards.

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | TD-57, TD-58 and TD-59 are resolved; TECH_DEBT_v6 records it at the sprint end; the Traefik/Dokploy access log stays TD-28 (9) | S6-09 |
| P2 | `TimeSpan` settings accept a bare number as days (`"15"` = 15 days) everywhere in the API, not only for the new timeout | Follow-up issue if wanted: a shared reader that requires the `hh:mm:ss` form |
| P3 | The header of TESTING_v91 still names ARCHITECTURE_v84, which this PR deletes (v85 and v86 remain) | S6-09 (as S6-03 P3) |
| P4 | If another PR merges ARCHITECTURE v86 first, this branch renumbers to v87 and C175 – C177 (and SPEC v2.50 if needed) | at the merge |

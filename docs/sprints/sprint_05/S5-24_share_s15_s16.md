# S5-24 — Share dialog S15 and shared list S16

> **Sprint:** 05 · **Task:** S5-24 (M3.6, 2 points) · **Issues:** none
> **Requirements / invariants:**
> - **TEN-09 → `active`** (TESTING_v81 V272);
> - **TEN-12 complete** with its client part (V273);
> - TEN-08 gains two malformed-token cases (V274); A11Y-09 green with S15 and S16 in the reflow check, SEC-10 gains the client part of creating a link (V275);
> - SPEC R-SAV-04, R-SAV-05, SD15, §8.10 (v2.41); ARCHITECTURE C159; UX copy v36 U150 – U156.
>
> **Status:** done — pull request open (2026-11-16)
> **Scope:**
> - the expiry dates of a new link served by the API with the new 🔧 default;
> - S15 on a saved list: required expiry with default, the link shown once and copied, the active links with revoke;
> - S16 at `/geteilt/:token` without an account, S11 read only;
> - S15 and S16 in the reflow check; TEN-09 in E2E.
>
> **EF migration:** none.
> **Owner decisions (2026-11-16, before the task):**
> - **1a** — the API serves the earliest, default and latest date, worked out in Berlin; new 🔧 `ShareLinks:DefaultExpiryDays` = 90.
> - **2a** — „Freigabelink erstellen“ only on the saved list, for Editor and Owner; Readers do not see it.
> - **3a** — „Link ungültig machen“ revokes at once, announces `criteria.share.revoked`, the focus to the next link or the heading.
> - **4a** — S16's header shows the session as it is; „header in the anonymous state“ of the task row and Handoff §5.3 read as „works without an account“.
> - **5a** — after „Link erstellen“ the dialog stays open with the link field, the hint and „Link kopieren“.
> - **6a** — S16 in the read-only mode of a saved list, without filter row or action bar; „Kriterien zusammenstellen“ links to S10; no expiry date shown.
> - **A (a)** — the dates in the answer of `GET /configurations/{id}/share-links`, loaded each time S15 opens.
> - **B (a)** — „Gültig bis“ is the browser's date field (Material's datepicker has English texts and the UX copy none).
> - **Copy** — the seven proposed keys (UX copy v36 U150 – U155) as proposed.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | S5-23 (PR #360) merged: create, list, revoke and the anonymous read exist; the list answered only `[ { id, createdAt, expiresAt } ]` | Owner decision 1a, A (a): the answer becomes `{ expiry, links }` with `expiresOn` per link |
| 2 | `expiresAt` is the midnight after the chosen day in Berlin; formatted in the browser it names the next day | The server adds `expiresOn`, the chosen last day; the client formats a `yyyy-MM-dd` day in UTC so it never shifts |
| 3 | The UX copy had no texts for the list of links, its empty state, the once-only hint, the dialog's error notice, a refused clipboard, or the H1 and page title of an invalid link | Seven keys proposed before the task and approved (UX copy v36) |
| 4 | Material's datepicker would bring English screen-reader labels; the app has no German `MatDatepickerIntl` | Owner decision B (a) |
| 5 | The task row says „header in the anonymous state“; owner decision 4a says the header shows the session | 4a applies; recorded here and in ARCHITECTURE C159 |
| 6 | No Figma frame for S15 or S16; the Figma mapping has no rows for S12, S14 or a saved list either | Handoff §5.3; S16 carries the S11 frame tag `2020-70` like a saved list; the mapping does not change |
| 7 | The S16 reflow test showed „Etwas ist schiefgelaufen“ for an unknown token of 43 characters: `Base64Url.TryDecodeFromChars` throws `FormatException` for a text that is no Base64url (it returns false only for a short buffer), so `GET /shared/{token}` answered **500** | Fixed here (deviation, §5 item 1): R-SAV-05 needs „Link nicht mehr gültig“ for every malformed link |

## 2. Changes

### 2.1 Server

- `Platform/Sharing/ShareLinkSettings.cs`: 🔧 `DefaultExpiryDays` (90 in `appsettings.json`), positive and at most 28 days per month of `MaxExpiryMonths`, so it always lies inside the window; else the start stops.
- `Platform.Contracts/Sharing/IShareLinks.cs`: `ExpiryWindow()` → `ShareLinkExpiryWindow(Earliest, Default, Latest)`; `ActiveShareLink` adds `ExpiresOn`.
- `Platform/Sharing/ShareLinkService.cs`:
  - `ExpiryWindow()` counts from today in the configured zone; `CreateAsync`'s check uses the same window;
  - `ListActiveAsync` adds each link's last day;
  - `TryHash` catches the `FormatException` of a malformed token (finding 7).
- `Configurator/Sharing/ShareLinkEndpoints.cs`: `GET /configurations/{id}/share-links` answers `ShareLinksResponse(Expiry, Links)`.

### 2.2 Client

- `models/share-links.ts`: the types, `SHARED_PATH` and `sharedListUrl` (import-free for `app.routes.ts`).
- `services/api/configurator-api.service.ts`: `shareLinks`, `createShareLink`, `revokeShareLink`, `shared`, all `CALLER_SHOWS_FAILURES`.
- **S15** `dialogs/share-dialog/`:
  - `ShareDialog.open(dialog, { listId })` with `autoFocus: 'dialog'`; after `afterOpened` and the load the focus goes into „Gültig bis“;
  - the browser's date field with the server's default, `min` and `max`; on submit a missing or out-of-range date shows the error summary and `criteria.share.expiry.error` with the last allowed day; the server's field error at `expiresOn` shows the same text;
  - a created link in a read-only field with `criteria.share.link.hint`, focused; „Link kopieren“ through the clipboard API, or `criteria.share.copyFailed` and the link selected;
  - „Gültige Links“ with one line per link (the revoke button described by it), revoking at once, the focus to the next button or the H3; the shown link disappears when it is revoked;
  - `pa-confirm-required-notice` on `email-unconfirmed`; the error notice `criteria.share.error.*`; a failed load offers „Erneut versuchen“.
- **Saved list** `pages/saved-list/`: „Freigabelink erstellen“ next to „Auswahl ändern“ for Editor and Owner, also on a list of an older release; both buttons wrap at 320 px.
- **S16** `pages/shared-list/`:
  - `shared-list.routing.ts`: the resolver loads the shared list, the criteria and filter values of its release and the configurator settings, and names the page title (`pageTitle.criteria.shared` or `.sharedInvalid`);
  - `shared-list-load.ts`: a 404 is „invalid“, any other failure „server“;
  - `shared-list-page.ts`: H1, the note with „Kriterien zusammenstellen“, summary, priority note, counts and read-only cards;
  - route `geteilt/:token` in `app.routes.ts`, lazy, no guard.
- `saved-list-load.ts`: `selectionOf`/`decisionsOf` take the shared list too.
- `scripts/check-i18n.mjs`: areas `criteria.share` and `criteria.shared`; `de.json` written from UX copy v36.

### 2.3 Tests

- `Integration/Configurator/ShareLinkTests.cs`:
  - `List_ExpiryWindow_CountsFromTheBerlinDayWithTheDefaultOf90Days` (TEN-12): the window at 23:30 UTC, the default accepted;
  - the list test reads the new shape and checks `expiresOn`;
  - `Read_LinkNoLongerValid_Returns404` adds `malformed-full-length` and `not-base64url` (TEN-08).
- `Unit/Sharing/ShareLinkSettingsTests.cs`: the values read; missing, non-positive, too long default, missing or unknown zone stop the start.
- `share-dialog.spec.ts` (11): default, focus and limits (TEN-12); refusal on submit; create, show once, copy; refused clipboard; revoke with the focus; the shown link dropped on revoke; SEC-10; the server's field error; the error notice; „Erneut versuchen“; „Schließen“ returns the focus. axe on the open and the created state.
- `shared-list-page.spec.ts` (6): read only (TEN-09), the detail panel, the invalid link, the server failure, the loader.
- `saved-list-page.spec.ts`: the share button for the Owner on an older release, none for the Reader.
- `configurator-api.service.spec.ts`: the four requests and their marker.
- E2E `configurator/s15-s16-share.spec.ts` (TEN-09): an account, organisation and list of the test's own (`saveListThroughApi` in `support/accounts.ts`); S15 creates a link; a context without cookies reads S16 read only, axe in both appearances; after the revocation „Link nicht mehr gültig“.
- E2E `shell/reflow.spec.ts`: S15 with a created link and two links; S16 with an open detail panel and the invalid link.

### 2.4 Documents

- UX copy v36 (U150 – U156; v34 deleted).
- TESTING v81 (V272 – V275).
- ARCHITECTURE v73 (C159; §7.2).
- SPEC v2.41 (§8.10, SD15, change rows v2.41-1 – 3).
- Handoff 1.27 (§5.3 rows S15 and S16 as built).
- CODEBASE §2.74 and §3; CLAUDE.md §3 (state).

## 3. Self-review

Checklist of CLAUDE §6.7 and `/engineering:code-review` on the staged diff.

| # | Finding | Sorting |
|---|---|---|
| R1 | A token of the right length that is no Base64url answered 500 (S5-23) | **fixed** (finding 7), with two test cases |
| R2 | The ids of the link lines (`pa-share-link-<i>`) started like the link field's id | **fixed**: `pa-share-item-<i>` |
| R3 | `criteria.share.expiry.hint` names „90 Tagen“ and „ein Jahr“ in its text, while both are 🔧 | **open point** 2: the wording is the UX copy's; a changed setting needs a new text |
| R4 | The browser's date picker ignores `min`/`max` on some mobile browsers | **won't fix**: the client checks on submit and the server is authoritative (TEN-12) |
| R5 | S16 shows „Nicht in Ihrer Liste“ on an excluded card, addressed to the reader, not the list's owner | **open point** 3: the card's wording is shared with S11 |
| R6 | Security | **checked:** <ul><li>the token only in the dialog's field and the clipboard, never stored in the browser;</li><li>`strict-origin` keeps it out of the referrer of S16's requests;</li><li>S16 sends no cookie-dependent request beyond the session probe; the server's read stays redacted (TEN-08);</li><li>the dialog builds the link from `location.origin`, never from server data.</li></ul> |
| R7 | N+1 and queries | **checked:** the list answer is the same indexed query plus a date computation per link |
| R8 | a11y | **checked:** focus order and returns, error summary, the described revoke buttons, live announcements through `A11yAnnouncerService`, `aria-disabled` while busy; axe in component tests and E2E in both appearances |

## 4. Checks

- **Backend:**
  - `dotnet format --verify-no-changes` clean;
  - `dotnet build -warnaserror` clean;
  - `dotnet test --solution` **1 645 passed** (unit, architecture, integration).
- **Client:** `ng build` clean; `ng lint` clean; `ng test` **739 passed** (77 files, the new ones included).
- **E2E** (local stack, Chromium, Firefox, mobile Chrome; WebKit only in CI): the full suite 378 passed, 1 skipped, 2 failed on Firefox — J8 and the invitations spec timed out while `dotnet test` ran on the same machine; both passed on their own afterwards. The new spec and the reflow cases passed on all three projects.
- **Scripts:** `check-invariants` (TEN-09: 4 tests, TEN-12: 4), `check-figma-links`, `check-i18n`, `check-icons`, `reuse lint` (container).
- **Figma:** no frame for S15 or S16; S16 follows the saved list, which follows the S11 frame.

## 5. Open points

1. **Deviation:** the malformed-token fix (R1) is server code of S5-23, done here because S16's „Link nicht mehr gültig“ depends on it (R-SAV-05).
2. **UX copy:** `criteria.share.expiry.hint` writes the 🔧 values into its text. If `ShareLinks:DefaultExpiryDays` or `MaxExpiryMonths` ever change, the text needs placeholders; proposal for a later UX copy version.
3. **UX copy:** on S16 an excluded card says „Nicht in Ihrer Liste“ to someone who does not own the list; a reader-neutral variant would need a new key.
4. **Migration step 5:** no share action for a project copy (409, C59) — unchanged from S5-23.
5. **S5-28:** deleting an organisation deletes its link rows — unchanged from S5-23.
6. **TECH_DEBT:** none added.

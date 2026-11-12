# S4-23 — Accept-Language on the catalog list endpoints

> **Sprint:** 04, P0 task S4-23 (1 point, early backend task). **Issues:** none. **Requirements / invariants:** ARCHITECTURE §7.1 („All list endpoints honour `Accept-Language`“), SPEC §8.1, §8.2, §3 (English user interface later); API-06 → `active` for the two catalog endpoints.
> **Status:** done — pull request open, not merged.
> **Scope:** `GET /api/v1/catalog/filter-options` and `GET /api/v1/catalog/criteria` choose their language from `Accept-Language` (`de`, `en`), answer German while a release has German texts only, and name the language in `Content-Language`; `Vary: Accept-Language` and the language in the `ETag`; TESTING_v53, ARCHITECTURE_v42 (C128), SPEC v2.14. Not here: English texts in a release (format and storage need an ADR), the language of the export files, Problem Details texts, the legacy controllers, the client.
> **Owner decisions:** none needed. The defaults stated before the task and accepted by the owner on 2026-11-12: (1) no language in the release format yet; (2) only the two catalog endpoints; (3) every other language, no header and a header that cannot be read get German, never 406; (4) `Vary: Accept-Language`, `Content-Language` also on a 304, the language in the `ETag`; (5) only the header counts, not the host's request localization; (6) API-06 `active` with the other list endpoints deferred, as API-05 does.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Both catalog endpoints answer through `Http/CatalogJsonAnswer` (ETag = hash of the body, `Cache-Control`, 304), which knew nothing of a language | One place for `Content-Language`, `Vary` and the ETag |
| F2 | Every catalog text is German: the filter values have `NameDe`, the release folder format (S3-15) has no language, and the only real release 2026.1 is German | The served language is always `de` today; the choice is still built and unit-tested for a release with English, so it does not have to be rewritten when one comes |
| F3 | The host already runs `UseRequestLocalization` with `en`/`de` (default `de`) for the legacy code; its default providers also read `?culture=` and the `.AspNetCore.Culture` cookie | The Catalog module reads `Accept-Language` itself: an answer chosen by a cookie could not be cached with `Vary: Accept-Language` (§9.6). The host setting stays for the legacy controllers |
| F4 | The client's legacy `languageInterceptor` (`core/interceptors/`) already sends `Accept-Language` = the ngx-translate language (`de`) with every request | No client change; the API never answers in a language other than the UI's as long as the client keeps sending it |
| F5 | `Content-Language` is a CORS-safelisted response header and `Accept-Language` a safelisted request header | No CORS change for `npm start` on another origin |
| F6 | The catalog answers are `Cache-Control: public` (named release: one hour) | Without `Vary: Accept-Language` a shared cache would hand one language's answer to everyone once a second language exists |

## 2. Changes

| File | Change | Requirement / invariant |
|---|---|---|
| `src/ProcureAccess.Modules.Catalog/Http/CatalogLanguage.cs` (new, `internal`) | `Negotiate(Accept-Language, available)`: the client's languages by weight, ties in header order, `q=0` refused, `en-US` → `en`, `*` = any available language the header does not name; German otherwise (also for an unreadable header). `OfReleases` = `[de]`: every release has German texts only. `OpenApiNote` for the endpoint descriptions | API-06 |
| `src/ProcureAccess.Modules.Catalog/Http/CatalogJsonAnswer.cs` | Takes the served language: `Content-Language` and `Vary: Accept-Language` on 200 and 304; the ETag hashes the language with the body (`IncrementalHash`, first 16 bytes of SHA-256 as before) | API-06; ARCHITECTURE §9.6 |
| `src/ProcureAccess.Modules.Catalog/FilterOptions/FilterOptionsEndpoint.cs`, `Criteria/CriteriaEndpoint.cs` | Negotiate before resolving and pass the language; the OpenAPI description names the header | API-06 |
| `tests/ProcureAccess.Tests.Unit/Catalog/CatalogLanguageTests.cs` (new) | `INV:API-06`: German-only release → `de` for `de`, `en`, `en-US,en;q=0.9`, `fr`, `*`, `de;q=0`, empty, unreadable; release with English → weights, ties, regional tags, `q=0`, `*`, several header lines | API-06 |
| `tests/ProcureAccess.Tests.Unit/Catalog/CatalogJsonAnswerTests.cs` (new) | The same body in another language has another ETag and does not revalidate to 304 (not reachable over HTTP while every release is German) | API-06 |
| `tests/ProcureAccess.Tests.Integration/Catalog/CatalogLanguageContract.cs` (new) | Shared contract check: the answer for a given `Accept-Language` equals the `de` answer (body and ETag), carries `Content-Language: de` and `Vary: Accept-Language`, and its 304 carries both too | API-06 |
| `tests/ProcureAccess.Tests.Integration/Catalog/FilterOptionsEndpointTests.cs`, `CriteriaEndpointTests.cs` | `INV:API-06` contract theories over `de`, `en`, `en-US,en;q=0.9`, `fr` and no header, for a named and for the active release | API-06 (C) |
| `docs/TESTING_v53.md` (new; v51 deleted) | API-06 `active` with the deferred parts (V167), input documents (V168) | — |
| `docs/ARCHITECTURE_v42.md` (new; v40 deleted) | C128; §7.1 and §9.6 name `Content-Language`, `Vary` and the ETag | — |
| `docs/SPEC_v2.md` | v2.14: one paragraph at the top of §8 for the catalog answers | — |
| `CLAUDE.md` | §3 state, §4 layout of `Http/`, header log | — |

## 3. Self-review (CLAUDE §6.7)

Pass with the checklist (security, correctness, N+1, edge cases, a11y) over the diff against `main` (skill `engineering:code-review`). Sorted per CLAUDE §6.8.

| # | Finding | Sorting | Reason / change |
|---|---|---|---|
| R1 | Nothing tested that the language is part of the ETag: every answer is German, so the HTTP tests cannot see it | **Fix** | `CatalogJsonAnswerTests` (two cases) |
| R2 | RFC 9110 §15.4.5 says a 304 should not carry representation metadata beyond its list unless it guides cache updates; `Content-Language` is on the 304 | **Won't fix** | The same ETag means the same language, so a cache only gets the value it already stores; the owner accepted it with the defaults (4) |
| R3 | A header with one unreadable entry (`en, ;;;`) is refused as a whole by `TryParseList`, so the request gets German even if `en` were available | **Won't fix** | Not observable while every release is German; German is the stated fallback; browsers send well-formed headers |
| R4 | Could the header put client text into the response? | **No change needed** | `Content-Language` is always a value of `available` (our constants), never the client's text |

Checked without finding: no database access added (the language is chosen per request in memory, linear in the header's entries; Kestrel limits header size); Problem Details answers (400, 404) unchanged and without the two headers; the ETag change makes every client fetch each catalog answer once more after the deployment (no 304 for an old ETag) — harmless; ARC-01 – ARC-03 green (all new types `internal`); no new dependency (`Microsoft.Net.Http.Headers` is part of ASP.NET Core); no UI text, so no UX copy key, no a11y change.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904`, `dotnet format ProcureAccess.sln --verify-no-changes` | Pass |
| C2 | `dotnet test --solution ProcureAccess.sln` | 841 of 841 pass; after R1 the unit project again: 543 of 543 (843 in all) |
| C2a | Mutation: the language left out of the ETag hash | Both `CatalogJsonAnswerTests` fail; reverted |
| C3 | `check-invariants.mjs` (TESTING_v53: API-06 with 4 tagged tests), `check-figma-links.mjs`, `check-i18n.mjs` | Exit 0 each |
| C4 | `reuse lint` | Compliant |
| C5 | Client, E2E | No client change; the client already sends `Accept-Language` (F4). E2E left to CI: the catalog smoke test (`e2e/smoke/catalog.spec.ts`) and J1 run against both endpoints |

## 5. Open points

| # | Point | Next step |
|---|---|---|
| P1 | Pull request opened; not merged | Owner |
| P2 | English catalog texts: where a release stores them (folder format, `CriterionRevision`, filter-value names) and how `CatalogLanguage.OfReleases` becomes a property of the release | ADR when the project team plans English texts (SPEC §3: English UI later) |
| P3 | The export files are German only; SPEC §8.3 says export language = UI language (`ExportTexts` would get a second resource) | With English texts (C124 (1)) |
| P4 | The other list endpoints (directory, workspace) honour `Accept-Language` with their steps; API-06 says so (V167) | Steps 5/6 |
| P5 | The host's `UseRequestLocalization` still reads `?culture=` and a cookie for the legacy controllers | Goes with the legacy controllers; no change in this task |

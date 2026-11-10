# S4-02 — UX copy v17: proposals for every gap of #181

> **Sprint:** 04, P0 task S4-02 (debt TD-15, 1.5 points). **Issues:** #181. **Requirements / invariants:** CLAUDE §6.5 (UI strings only from the UX copy); SPEC SD25; no invariant activated, no new TESTING version. **Status:** draft pull request by Claude Code (2026-11-10), waiting for the owner's review (O-S4-4).
> **Scope:** exactly the sprint row: `docs/ux_copy_procureaccess_v17.md` with a proposal for every gap of #181, the problem-type keys in SPEC §8.6 (v2.7), and the documents that name them. No change to `de.json`, code, tests or ⚖️ texts.
> **Owner decisions** (2026-11-10, before the start):
> 1. **(1a)** The proposals stand in the text tables of §5/§6 and are marked only outside the „Text“ column (change list and „Hinweis“). The draft stays red on `check-i18n.mjs` while it is reviewed. After the owner approves or edits the texts, `node scripts/check-i18n.mjs --write` runs as the last commit of this pull request, so `de.json` changes only with approved texts.
> 2. The #181 row „write out `pageTitle.notFound` / `.forbidden` / `.server`“ (S2-13 P3), missing from the sprint row, is in scope.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | #181 lists nine gaps; the snackbar row is done (O-S3-9, S3-22). TECH_DEBT_v3 §6 adds `selection-excluded` (S3-20a P3), the line breaks of `suggest.mailBody` (S3-21 P1) and the header versions (TD-37) | Ten gaps in scope (§2.1) |
| F2 | `pageTitle.notFound` / `.forbidden` / `.server` have been written out since UX copy v15 (U83, S2-16); #181 was not updated | Owner decision 2 needs no new text; the protocol says so and v17 notes it under the change list |
| F3 | `check-i18n.mjs` (CI stage 1) requires every UX copy key of `common`, `shell`, `errors`, `legal`, `home`, `suggest` in `de.json` with the same text. 10 of the proposed keys are in those areas | The draft fails exactly on those 10 keys (§4); owner decision 1 |
| F4 | `check-i18n.mjs --write` copies a text cell character for character; it does not decode `\n` or `%0A` | `suggest.mailBody` cannot carry a line break in the table (U96) |
| F5 | `shell.userMenu.label` and `directory.product.entryCount` are not used in code (the account button comes with step 3, S41 with step 5) | Both can change without a code change |
| F6 | jsdom (Testing Library) keeps U+00AD in the accessible name: with the approved texts, `legal-page.spec.ts` fails 2 tests („Unable to find … name "Datenschutzhinweise"“); tried once with `--write` and reverted | The approval commit for U94 also updates the tests that name the legal titles (§5 P1) |

### 1.1 The problem types (S2-11 P1, SPEC §8.6)

| Problem type | Where it comes from | Text |
|---|---|---|
| `antiforgery` | any non-GET request with a stale token (ARCH §9.1) | new `errors.antiforgery` |
| `locked-criteria-list` | `PUT …/criteria-configuration` after an assessment (C47, R-PRJ-04) | existing `projects.detail.criteriaLocked` — R-PRJ-04 already names it |
| `test-status-unknown` | publication with status „Nicht bekannt“ (422 on `testStatus`, R-PUB-02) | existing `publish.error.testStatus` (field) / `publish.blocked.testStatus` (no field) |
| `no-criteria-list` | publication without a list (422, C54) | existing `publish.blocked.noCriteria` (R-PUB-02) |
| `profile-mismatch` | publication under another profile while an entry is visible (409, C55) | new `publish.error.profileMismatch` |
| `entry-withdrawn` | Flow E republish of a withdrawn entry (409, C55) | new `vendor.error.withdrawn` |
| `selection-excluded` | catalog endpoints, a value excluded by the release (SPEC §8.2) | new `criteria.select.error.excluded` (field) |
| `release-mismatch` | export with criteria of another release (EXP-07) | existing `criteria.export.outdated`; its „Hinweis“ already said „400 aus EXP-07“ (S2-11 P1 guessed it) |

## 2. Changes

### 2.1 Every gap with its key

| Gap (source) | Key(s) in v17 | Change row |
|---|---|---|
| Singular forms of the 18 rows of S2-12 F2 (S2-12 P1) | `common.loaded.one`, `common.results.shown.one`, `criteria.list.summary.count.one`, `criteria.list.copy.notCarried.one`, `criteria.export.scope.one`, `projects.detail.criteriaAdded.one`, `candidate.overview.directory.linked.one`, `candidate.evidence.list.statusChange.one`, `directory.results.one`, `signals.fulfillment.{fulfilled,slightRestriction,restriction,barrier,blockade,notApplicable,unknown,notCovered}.one` | U86 |
| Zero forms „where wanted“ (S2-12 P1) | `common.loaded.zero`, `common.results.shown.zero`, `directory.results.zero`, `candidate.overview.directory.linked.zero`, `projects.detail.meta.zero` | U87 |
| „/“ and note forms as keys (S2-12 P1) | `directory.result.useCaseLine(.one)`, `.vendorEntries(.one/.zero)`, `.independentEntries(.one/.zero)`, `.reports(.one)`, `projects.detail.meta(.one)`, `projects.detail.row.tested.evidences(.one)` | U88 |
| `directory.product.entryCount` had three counts in one text (found here) | removed; S41 uses the three `directory.result.*` parts of S40 | U89 |
| §7 key rule of SD25, mark O9 done (S2-12 P1); dotted-key storage rule, `de.json` path (S2-13 P1) | §5 header, §7, §8 O9 | U90 |
| `pageTitle.notFound` / `.forbidden` / `.server` (S2-13 P3) | done in v15 (U83); unchanged | — |
| Page-title variants (S2-16 P2) | `pageTitle.criteria.list`, `pageTitle.criteria.list.saved`, `pageTitle.publish`, `pageTitle.publish.step` | U91 |
| UX keys of the problem types (S2-11 P1, S3-20a P3), also in SPEC §8.6 | `errors.antiforgery`, `criteria.select.error.excluded`, `publish.error.profileMismatch`, `vendor.error.withdrawn`; mapping of all 13 types in the new §6.3 and in SPEC §8.6 (v2.7-1) | U92 |
| Reason for the unavailable „Löschen“ (S2-01 P3) | `account.delete.confirm.unavailable` (at the step-3 button `account.delete.confirm.cta`) | U93 |
| Soft hyphen in the legal titles (S2-18 P3) | `legal.privacy.title`, `legal.accessibility.title` (U+00AD); `legal.imprint.title` is short | U94 |
| Account button name vs visible „Konto“ (S2-15 P4) | `shell.userMenu.label` „Konto, angemeldet als {{displayName}}“ | U95 |
| Line breaks of `suggest.mailBody` (S3-21 P1) | `suggest.mailBody` without `%0A%0A`, new `suggest.mailBody.prompt`; the blank line is set by the code | U96 |
| Header naming the current document versions (TD-37) | new header line „Aktuelle Fassungen …“; the old list is „Grundlage (v14)“ | U97 |

Every new or changed text carries „Vorschlag (S4-02, U8x/U9x)“ in its „Hinweis“ cell (the `pageTitle` table has no such column; its rows are marked in the change list). The change list gives one line „Warum so“ per proposal.

### 2.2 Files

| File | Change | Why |
|---|---|---|
| `docs/ux_copy_procureaccess_v17.md` (new) | v16 plus the rows of §2.1, the new §6.3 „Problemtypen und ihre Texte“, change list U86–U97, header | The task |
| `docs/ux_copy_procureaccess_v15.md` | deleted | Only the two newest versions stay (CLAUDE §2) |
| `docs/SPEC_v2.md` | v2.7: change row v2.7-1 and a table in §8.6 with the UX key of each problem type; „proposal until O-S4-4“ | Row: „also name them for SPEC §8.6“ |
| `CLAUDE.md` | header; §3: `pageTitle` variants split in v17, problem-type keys in SPEC §8.6 / UX copy §6.3 | DoD; the §3 sentence „some rows … still hold variants“ was outdated |

Alternatives considered for the proposals the owner may want to change:

| Proposal | Alternatives |
|---|---|
| U95 `shell.userMenu.label` | (a) no `aria-label`: name = visible „Konto“, display name as description `aria-describedby` (needs a new key); (b) visible text shows the display name. The proposal keeps one key and the Handoff §3.1 markup |
| U96 two keys for the mail body | (b) `\n\n` in the text like `email.*` (§5.17), then `check-i18n.mjs` must decode `\n` (script change); (c) keep `%0A%0A` and decode it in S11 before `encodeURIComponent` (S3-21 P1) |
| U94 soft hyphen | leave the titles as they are: `hyphens: auto` keeps breaking inside the word (S2-18 R5); CSS cannot choose a specific break point |
| U87 zero forms | none for K1 counts (SIG-08: all values as numbers), for texts shown only with ≥ 1 (`criteria.list.copy.notCarried`, `projects.detail.criteriaAdded`, `candidate.evidence.list.statusChange`, `directory.result.useCaseLine`) and for `directory.result.reports` (omitted at 0) |

## 3. Self-review (CLAUDE §6.7, `/engineering:code-review`)

| # | Finding | Sorting |
|---|---|---|
| R1 | §6.3 intro said „types without a text show the generic notice“, but every type now has a text | **Fixed:** „until the screen of a type is rebuilt, the client shows the generic notice“ (also in SPEC §8.6) |
| R2 | A `|` inside the U94 change row („Datenschutz\|hinweise“) broke the table | **Fixed:** worded „zwischen „Datenschutz“ und „hinweise““; column count per row checked against v16 |
| R3 | The soft hyphen becomes part of the H1's accessible name (F6) | Won't fix here: headings are not controls, so WCAG 2.5.3 does not apply; screen readers do not speak it. The approval commit updates the tests (P1) |
| R4 | `publish.error.profileMismatch` says it also applies to the S55 republish; C55 says „only the publishing profile“ for Flow E but does not name the problem type | Kept as written; noted as an assumption (P3) |
| R5 | U93 promises an exact match („genau so … wie sie in Ihrem Konto steht“), as the legacy dialog checks today; the step-3 check is not specified | Kept; the owner may soften it in the review (P3) |
| R6 | SPEC's line „Current versions of the related documents (v2.4)“ still names UX copy v15 | Follow-up: TD-37 (#243, documentation session); out of scope |
| — | Security, performance, N+1: not applicable (documents only); `de.json` and code unchanged (`git status`) | — |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `node scripts/check-i18n.mjs` | **Fails as agreed (1a)** on 10 keys: `common.loaded.one/.zero`, `common.results.shown.one/.zero`, `shell.userMenu.label`, `suggest.mailBody`, `suggest.mailBody.prompt`, `legal.privacy.title`, `legal.accessibility.title`, `errors.antiforgery`; no parser problem in v17 |
| C2 | `check-i18n.mjs --write` in the working tree, then `git checkout` of `de.json` (not committed) | 0 errors after the write; the dotted keys land next to their base key (`"shown.one"`, `"loaded.zero"`, `"mailBody.prompt"`) |
| C3 | `npx ng test` of `legal-page.spec.ts`, `footer.spec.ts`, `de-json.spec.ts` with the written texts (reverted afterwards) | `legal-page.spec.ts` 2 of 3 fail (F6); the other two files pass |
| C4 | `node scripts/check-invariants.mjs` | Passed |
| C5 | `node scripts/check-figma-links.mjs` | Passed (2 checked, 24 not built yet, 0 pending) |
| C6 | `reuse lint` | 1064 / 1064 files compliant |
| C7 | Build, backend tests, E2E | Not run: no code change |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | **Approval commit (O-S4-4):** apply the owner's edits to v17, replace „Vorschlag (S4-02, Uxx)“ in the „Hinweis“ cells by „Uxx“, run `node scripts/check-i18n.mjs --write`; if U94 stays, update the tests that name the legal titles (`legal-page.spec.ts`, `e2e/shell/shell.spec.ts`, `e2e/shell/reflow.spec.ts`; `footer.spec.ts` and `route-change.spec.ts` use the footer link and `<title>`, which keep the plain words) | Last commit of this pull request, after the review |
| P2 | S11/S35/S41 use the new keys when they are built: `pageTitle` as an `AREAS` root (M2.6d), `translatePlural` for the count rows, `suggest.mailBody` + blank line + `suggest.mailBody.prompt`, `PROBLEM_TYPE_KEYS` from SPEC §8.6 | With the screens (S4-14 – S4-18 and later steps) |
| P3 | Assumptions the owner may correct in the review: `profile-mismatch` also for the S55 republish (R4); exact e-mail match in U93 (R5); `release-mismatch` = `criteria.export.outdated` | Review of this pull request |
| P4 | #181 can be closed with the merge; TECH_DEBT_v3 TD-15 is then done (next TECH_DEBT version at the sprint close-out) | Merge |

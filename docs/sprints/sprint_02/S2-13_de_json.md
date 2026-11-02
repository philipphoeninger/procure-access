# S2-13 — de.json from the UX copy

> **Sprint:** 02, P0 task S2-13 (roadmap M1.6, 2 points; ARCHITECTURE §12 step 1). **Issues:** #101. **Requirements / invariants:** CLAUDE §6.5 (UI strings only by key); no invariant changes status. **Status:** done by Claude Code (2026-11-02), pull request open.
> **Scope:** `de.json` built from UX copy v14 for the areas `common`, `shell` and `errors` by a script, a check that every key used in templates exists, #101 resolved. Not in scope: the keys of other areas (added with the step that builds their screens), the missing singular texts of the UX copy (S2-12 P1), the wording for #120 (O-S2-4, S2-15), `en/es/fr.json`.
> **Owner decisions 2026-11-02:** **O-S2-3** (from S2-12): option B, plural forms as separate keys chosen with `Intl.PluralRules`; `<key>` = plural, `<key>.one` = singular, `<key>.zero` = optional text for count 0 (SPEC_v2 SD25). **Legacy `en.json` / `es.json` / `fr.json`:** keep untouched. **#101:** option A, the UX copy keys stay; a key that extends a key with text is stored as one property name with dots next to it. **Scope:** only the three areas. **Broken legacy keys (F6):** option A, correct the two template paths.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `public/i18n/de.json` had 156 texts: the legacy uppercase roots and 9 lowercase keys from S1-19, S2-11 and S2-12. The UX copy (§5 header) and the task prompt name `src/assets/i18n/de.json`; the folder is `public/i18n/` since the Angular upgrade | Script uses `public/i18n/`; the path in the UX copy is open point P2 |
| F2 | UX copy v14 has 25 + 35 + 21 rows for `common`, `shell`, `errors` (82 keys: `common.yes` / `common.no` share a row). 8 were in `de.json`, with the UX copy wording | 74 keys to add, none to change |
| F3 | About 70 keys of the whole UX copy also start a longer key (e.g. `candidate.remove` / `candidate.remove.title`, `contribute.mine` with 12 longer keys). In the three areas only `shell.nav.menu` / `shell.nav.menu.close` (#101) and `shell.pageTitle` / `shell.pageTitle.error` | Option A (owner): no key renamed; the same dotted-name rule as SD25 covers all of them |
| F4 | ngx-translate 18 `getValue` walks the key from the left and, where a segment holds a text and is not the last, joins it with the next segment, so `"menu": "…", "menu.close": "…"` resolves both keys (S2-12 F5, pinned by its test) | The script copies this lookup; a Vitest spec runs the real library over every text of `de.json` |
| F5 | The UX copy writes several keys in one cell in two ways: full keys (`` `common.yes` / `common.no` ``) and relative ones (`` `projects.status.open` / `.done` ``); the texts are split on „ / “. Headings inside tables (`**Sitzungswarnung** (…)`) and removed keys (`~~…~~`) are rows too | Parser handles both forms; a cell it cannot read is reported for the key it names, not skipped |
| F6 | Two legacy templates used keys that do not exist and showed the key: `AUTH.CANNOT_UNDO_WARNING` (delete-account dialog; the text is under `COMMON.`) and `COMMON.PRODUCT_PART` (criterion proposal; under `CRITERIA.`) | Paths corrected (owner, option A); the German texts already existed, nothing invented |
| F7 | Six pipes take a key computed at runtime (`pa-notice` prefix `common.notice.${kind}`, the error summary's field label and message keys); a static check cannot follow them | Printed as notices; their keys are covered by the component tests of S2-11 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `scripts/check-i18n.mjs` (new) | Node, no dependencies. Reads the tables of §5–§6 of the highest `docs/ux_copy_procureaccess_v*.md` (column „Text“ of tables headed „Schlüssel“). Checks: (1) every key of `AREAS` (`common`, `shell`, `errors`) is in `de.json` with exactly the UX copy text; (2) every lowercase key of `de.json` is a UX copy key with the same text (uppercase = legacy); (3) every text of `de.json` is reachable under its own key; (4) every literal key before `\| translate` / `\| translatePlural` in `src/**/*.html` and `*.ts` (not specs) resolves. `--write` first copies the texts of `AREAS` into `de.json`, a key that extends a key with text as a dotted property name (F3, F4). A row it cannot read (Markdown in the text, key/text count mismatch, two different texts, a key cell it does not understand, a key row in a table without „Schlüssel“/„Text“) fails only for keys `de.json` needs, so rows of later steps never block it | Row "script or checked conversion, plus a check that every key in templates exists"; #101 |
| `scripts/check-i18n.test.mjs` (new) | 10 self-tests on `node:test` (built into Node): parser rules, lookup, `--write` in any order, and each of the four checks shown to fail | Tests come with the code |
| `CLIENT/…/src/app/testing/de-json.spec.ts` (new) | Vitest: every text of `de.json` comes back from `TranslateService.instant` under its own key; the four keys of F3 side by side, with parameters | Pins the dotted-name lookup of the real library for all of `de.json`; #101 |
| `CLIENT/…/public/i18n/de.json` | 74 keys added by `--write` (§2.1); new keys come after the existing ones of their group | Row |
| `CLIENT/…/delete-account-dialog.html`, `CLIENT/…/criterion-proposal.html` | `COMMON.CANNOT_UNDO_WARNING`, `CRITERIA.PRODUCT_PART` | F6 |
| `.github/workflows/ci.yml` | Stage 1: self-test, then the check | Row "check" runs on every PR |
| `docs/TESTING_v20.md` (new) | V81: §8.2 stage 1 and §3.4 list the check. No invariant changes status | Stage 1 is described there |
| `CLAUDE.md` | Header; §3 (check as built), §4 (scripts), §5 (commands), §6.7 (pre-push), §9 rule "Texts from the UX copy" | DoD |

### 2.1 Keys added

`common`: `actions.save`, `.cancel`, `.back`, `.next`, `.close`, `.edit`, `.showMore`, `.showLess`, `.retry`, `optional`, `loading`, `loaded`, `date.published`, `catalogRelease`, `yes`, `no`, `requiredNote`, `date.hint`, `date.openCalendar`, `results.shown`, `multiSelect.hint`, `newTab`, `stepper.done`.
`shell`: `skipLink`, `appName`, `appClaim`, `nav.label`, `nav.menu.close`, `nav.criteria`, `.projects`, `.directory`, `.contribute`, `.settings`, `.account`, `.login`, `.logout`, `userMenu.label`, `org.active`, `org.switch`, `footer.about`, `.werteit`, `.suggest`, `.accessibility`, `.privacy`, `.imprint`, `.source`, `pageTitle`, `pageTitle.error`, `session.warning.title`, `.body`, `.stay`, `.logout`, `.absolute`, `session.extended`, `session.expired.title`, `.body`, `session.restored`.
`errors`: `field.required`, `.requiredChoice`, `.email`, `.url`, `.maxLength`, `.date`, `.dateFuture`, `notFound.title`, `.body`, `forbidden.title`, `.body`, `server.title`, `.body`, `offline`, `rateLimit`, `home`, `email.sendFailed`.
Already present with the same text: `common.notice.info`, `.attention`, `.error`, `shell.nav.menu`, `errors.summary.title`, `.title.one`, `.item`, `errors.field.server`.

### 2.2 #101

No key changes in the UX copy. `de.json` holds `"nav": { "menu": "Menü", …, "menu.close": "Menü schließen" }` and `"pageTitle": "…", "pageTitle.error": "…"`; the header button keeps its name. The rule is in CLAUDE §9 and the script; the UX copy may state it in its next version (P1).

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review`: no security finding (local script over repository files, no untrusted input, output only to `de.json`; the CI step uses no event data). No dependency added; `node:test` is part of Node.

| # | Finding | Sorting |
|---|---|---|
| R1 | A key row in a table whose header is not „Schlüssel … Text“ was skipped, so a changed table header in a later UX copy would have dropped its keys from check 1 without an error (fail-open) | Fixed: such rows are reported for their keys; self-test added |
| R2 | The script ran only if `process.argv[1]` equalled its module path; Node resolves symlinks for the module but not for `argv[1]`, so started through a symlinked path the check would skip itself and exit 0 | Fixed: both sides compared as real paths; run through a symlinked checkout passes the check |
| R3 | `relative(ROOT, join(ROOT, file))` returned `file` | Fixed (simplified) |
| R4 | A literal after a concatenation (`x + 'SUFFIX' \| translate`) would be checked as the key `SUFFIX` | Won't fix: fails closed (an error, not a silent pass), no such template today |
| R5 | New keys come after the existing ones of their group, not in UX copy order (`nav.label` after `nav.menu`) | Won't fix: lookup does not depend on order; reordering would move the existing keys |
| R6 | Keys used only in TypeScript (`PROBLEM_TYPE_KEYS`, label keys passed to `applyServerErrors`) are not checked by check 4 | Won't fix here: they are covered by the component tests of S2-11; open point P4 |
| R7 | CI prints six notices for the computed keys on every run | Won't fix: they say what the check cannot see (F7) |

Probes (each shows a check can fail): a hidden key planted in `de.json` (`"PROBE.X"` next to `"PROBE": { "X" }`) failed both the script (check 3) and the Vitest spec, then the file was restored byte for byte; before the template fix the script reported both F6 keys with file and line; before `--write` it reported every missing key of the three areas.

## 4. Checks

| Check | Result |
|---|---|
| `node --test scripts/check-i18n.test.mjs` (Node 24.22.1) | 10 passed |
| `node scripts/check-i18n.mjs` | 82 keys of common, shell, errors, 138 client files, 0 errors, 6 keys not checked (F7) |
| `npx ng test` (Vitest, all specs) | 46 tests in 11 files passed (2 new) |
| `npm run lint` | All files pass |
| `npm run build:release` | Passes; the known warning about one unparsable selector in the critical CSS (S2-03) is unrelated |
| `node scripts/check-invariants.mjs` | Passed |
| `node scripts/check-figma-links.mjs` | Passed (1 checked, 11 not built yet, 1 pending); no mapped component touched |
| `reuse lint` (image `fsfe/reuse:latest`) | 618 / 618 files compliant |
| Backend tests, E2E | Not run: no backend change; the two template fixes are in legacy screens no journey visits (J1 is the configurator), the delete-account dialog spec passes |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | Next UX copy version: state the storage rule of §2.2 next to SD25 in §7 (a key that starts with another key's text is stored with a dot in its name), and correct the file path in the §5 header to `public/i18n/de.json` (F1). Together with S2-12 P1 (singular forms, e.g. `common.loaded`, `common.results.shown` „1 Kriterien angezeigt.“, which this PR took as written) | Owner (UX copy) |
| P2 | Add a root to `AREAS` in `scripts/check-i18n.mjs` with the step that builds its screens (`home`, `pageTitle` with S2-16/S2-17, `auth` with step 3 …); `--write` then fills it | Each migration step |
| P3 | `pageTitle.notFound` / `.forbidden` / `.server` say „wie die jeweilige H1 (§6.2)“ instead of a text; the script reports them once `pageTitle` is an area. The UX copy should write the texts out | Owner (UX copy), before S2-16 |
| P4 | Keys used only in TypeScript are outside check 4 (R6); worth a look when more code builds keys (announcements in S2-16) | S2-16 or later |
| P5 | The status line of every TESTING version since v15 starts with „v15 —“; TESTING_v20 keeps it to stay in line | Next TESTING housekeeping |
| P6 | TECH_DEBT_v1 #101 row: closed by this PR; recorded in `TECH_DEBT_v2.md` at the sprint close-out (process step 14) | Sprint 02 close-out |

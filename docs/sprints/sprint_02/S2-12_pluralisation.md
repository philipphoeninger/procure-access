# S2-12 — Pluralisation approach (UX-O9)

> **Sprint:** 02, P0 task S2-12 (roadmap M1.6, 1 point; ARCHITECTURE §12 step 1). **Issues:** none. **Requirements / invariants:** SPEC_v2 §13 UX-O9 (now closed, SD25); UX copy v14 §7 and §8 O9; no invariant changes status. **Status:** done by Claude Code (run 1 and run 2, 2026-11-01/02), pull request open.
> **Scope:** run 1: the options with licence, bundle size, compatibility with ngx-translate 18 and zoneless Angular 22, and how the UX copy writes plural forms, including the `.one` key next to its base key (S2-11 needs it for `errors.summary.title`). Run 2: the chosen approach with one example key and the decision in SPEC. Not in scope: `de.json` from the UX copy and #101 (S2-13), the error summary itself (S2-11), the missing singular texts of the UX copy (P1).
> **Owner decision 2026-11-02 (O-S2-3):** option **B**: plural forms as separate keys chosen with `Intl.PluralRules`; keys `<key>` = plural, `<key>.one` = singular, `<key>.zero` = optional text for count 0. Rejected: option A, ICU MessageFormat with `ngx-translate-messageformat-compiler` (reasons in §1).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | ngx-translate 18 has no plural support; the UX copy (§7) leaves the choice to the implementation (UX-O9, "step 1") | Options in run 1, owner decision O-S2-3 |
| F2 | The UX copy has 37 rows with a count. 12 need no plural form (the number stands alone: `directory.entry.row.*`, „Treffer“, „({{count}} aktiv)“). 7 write the singular, in three styles: a key `.one` next to the base key (only `errors.summary.title`), all forms in one cell with „/“ (`directory.result.*`, incl. a zero form), or a note (`projects.detail.meta`, `projects.detail.row.tested`). 18 lack a singular although the count can be 1 (e.g. `common.results.shown` „1 Kriterien angezeigt.“) | Gaps for the next UX copy version (P1); no wording invented (CLAUDE §6.5) |
| F3 | Option A (`ngx-translate-messageformat-compiler` 7.3.0 + `@messageformat/core` 3.4.1): MIT, dependencies MIT/ISC/BSD-3-Clause/Unicode-DFS-2016 (D12 met); peer dependency allows ngx-translate ^18; measured **+74 KB minified / +21 KB gzip** in the initial bundle; compiles messages at runtime with `new Function`, so the CSP must allow `'unsafe-eval'` (ARCHITECTURE D14 relies on a restrictive CSP); ICU writes `{count}`, so all 180 UX copy rows with `{{…}}` would change or need an opt-out prefix | Rejected (O-S2-3) |
| F4 | Option B: no dependency, `Intl.PluralRules` is built into every supported browser (SPEC §7.4), placeholders stay `{{…}}`, matches the `.one` style the UX copy already uses | Chosen (O-S2-3) |
| F5 | In nested `de.json` a property cannot be a text and an object at once (`errors.summary.title` + `errors.summary.title.one`, like #101). Tested against the installed ngx-translate 18: its lookup (`getValue`) also finds a property name with a dot, so `"title": "…", "title.one": "…"` side by side resolves both keys. The code does this on purpose (a fallback branch), but the README does not promise it | Used for `.one`/`.zero`; pinned by a test (§2); the same works for #101 (P2) |
| F6 | ngx-translate 18 keeps the translations and the current language in signals; its `translate` pipe is impure and reads them | The new pipe reads the same signals, so the view updates when translations load or the language changes (zoneless, CLAUDE §9) |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/…/src/app/shared/pipes/translate-plural.pipe.ts` (new) | `pluralKey(translate, key, count)`: takes the first language (current, then fallback) that has the base text; in it, `<key>.zero` for count 0 if present, else `<key>.<category>` from `Intl.PluralRules` if present, else `<key>`. Pipe `translatePlural` (impure, like `translate`): `{{ key \| translatePlural: count }}`, passes `count` to the text | O-S2-3; only one language decides, so singular and plural never mix languages |
| `CLIENT/…/src/app/shared/pipes/translate-plural.pipe.spec.ts` (new) | Seven tests with the real German texts: singular for 1, plural for 3 and 0, switch when the count signal changes, `.zero`, fallback to the base key without `.one`, same language when the current one falls back, and the pinned dotted-name lookup of ngx-translate | Run 2 "example key renders singular and plural"; F5 |
| `CLIENT/…/public/i18n/de.json` | `errors.summary.title` and `errors.summary.title.one`, verbatim from UX copy v14 §6.1 | Example key; S2-11 uses it for the error summary |
| `docs/SPEC_v2.md` | v2.2: change row v2.2-1, decision **SD25**, UX-O9 removed from §13 and listed as closed. Recorded in this file like v2.1, as the task allowed "SPEC §13" | Run 2 |
| `CLAUDE.md` | Header; §3 (plural pipe as built); §9 rule "Plural forms" | DoD |

## 3. Self-review

Checklist of CLAUDE §6.7 with `/engineering:code-review`: no correctness or security bug. Security: output goes through Angular interpolation (escaped); no dependency, no `eval`. Correctness: `Intl.PluralRules` only receives the app's language codes (`de`, `en`, `es`, `fr`), and a missing language returns the key unchanged. Performance: no N+1 or loops. a11y: no markup.

Both mutations were checked by hand: always returning the base key fails three tests; checking the variant in any language (instead of the language of the base text) fails the language test.

| # | Finding | Sorting |
|---|---|---|
| R1 | The pipe is impure, so it runs on every change-detection pass of its view | Won't fix: same as ngx-translate's own `translate` pipe; two or three object lookups; zoneless with `OnPush` limits the passes |
| R2 | S2-13's check "every key in templates exists" has to treat `title.one` / `title.zero` as keys | Follow-up: S2-13 (P3) |
| R3 | The UX copy gaps of F2 | Open point P1 |
| R4 | `pluralKey()` is exported but nothing in code calls it yet | Won't fix: it is the pipe's core and texts built in code (announcements, A11Y-03 in S2-16) need it |

## 4. Checks

| Check | Result |
|---|---|
| `npx ng test` (Vitest, all specs) | 24 tests in 6 files passed (7 new) |
| `npm run lint` | All files pass |
| `npm run build:release` | Passes; initial total 1.74 MB raw / 357 KB transfer. The existing warning about one unparsable selector in the critical CSS is unrelated (no styles changed) |
| `node scripts/check-invariants.mjs` | Passed |
| `node scripts/check-figma-links.mjs` | Passed (0 checked, 12 not built yet, 1 pending); the pipe is not a mapped component |
| `reuse lint` | 598 / 598 files compliant |
| Backend tests, E2E | Not run: no backend change; no template uses the pipe yet |

## 5. Open points

| # | Point | Where |
|---|---|---|
| P1 | Next UX copy version: mark O9 done and describe the key rule of SD25 in §7; add the singular (and, where wanted, the zero form) for the 18 rows of F2 (`common.loaded`, `common.results.shown`, `criteria.list.summary.count`, `criteria.list.copy.notCarried`, `criteria.export.scope`, `projects.detail.criteriaAdded`, `candidate.overview.directory.linked`, `candidate.evidence.list.statusChange`, `directory.results`, `directory.product.entryCount`, the eight `signals.fulfillment.*`); write the „/“ and note forms of `directory.result.*`, `projects.detail.meta` and `projects.detail.row.tested` as `.one`/`.zero` keys | Owner (UX copy), before S2-13 needs those keys |
| P2 | #101 (`shell.nav.menu` next to `shell.nav.menu.close`) could keep both keys with the same dotted-name lookup (F5) instead of renaming them | Owner decision for S2-13 |
| P3 | S2-13's key check and conversion script must keep `.one`/`.zero` as dotted property names next to the base key (R2); the legacy `en/es/fr.json` have no plural keys and fall back to German | S2-13 |

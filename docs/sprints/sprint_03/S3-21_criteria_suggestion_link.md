# S3-21 — Footer link „Kriterium vorschlagen“

> **Sprint:** 03, P0 task S3-21 (M2.5, 1 point). **Issues:** none. **Requirements / invariants:** SPEC R-SUG-01 (footer part; now v2.5), R-SHL-06; no invariant activated, A11Y-01 stays green. TESTING_v37 adds only V116 (release-check self-test in CI stage 1). **Status:** done by Claude Code (2026-11-09), pull request open.
> **Scope:** exactly the sprint row: the footer link opens the configured channel `Contact:CriteriaSuggestion` (`mailto:` with subject, or a URL), a 🔧 placeholder until O-6, and a check that refuses the placeholder for production. **Not in scope:** the link at the end of S11 with the criterion key in the mail body (R-LST-13, M2.6); the legal pages of GP.3 (R-SHL-07); a production release pipeline.
> **Owner decisions** (2026-11-09, asked before the task, all as recommended):
> 1. **1a — release check instead of a failing build.** `npm run build:release` runs in CI stage 2, in the client image (E2E stack) and in staging; failing it while O-6 is open would block all three. A separate script `scripts/check-release-config.mjs` fails on the placeholder and runs before a production release; GP.3 adds the legal pages to it.
> 2. **2a — build-time client setting.** `criteriaSuggestion` in `environment.ts` / `environment.prod.ts`; one value for every environment (WERTE.IT's channel), so the image stays one image; no endpoint (R-SUG-01, ARCHITECTURE §16.4.2).
> 3. **3a — shown with the placeholder.** Outside production the link is shown and points to `mailto:` on the reserved domain `.invalid`, so tests and E2E exercise the real path.
> 4. **4 — UX copy.** UX copy v16 gives „(öffnet eine externe Seite)“ its own key `suggest.external.url` (wording unchanged, U85); from the footer the mail has the subject `suggest.mailSubject` and no body (the body names a criterion, which the footer has none of). Defaults stated with the decisions and not objected to: the footer uses `shell.footer.suggest` like its other links (`suggest.link` has the same text and stays unused), the link comes first (UX copy order), `suggest` becomes an `AREAS` root of `check-i18n.mjs`.
> 5. **Self-test in CI** (asked with the plan): `check-release-config.test.mjs` runs in CI stage 1 like the other script self-tests, recorded as V116 in TESTING_v37.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The footer (S2-15) links the three legal placeholders and the source code; „Kriterium vorschlagen“ was left out until its channel exists (ARCHITECTURE C99) | New first link, shown with the placeholder (decision 3a) |
| F2 | SPEC R-SUG-01 asks the production build to fail on the placeholder, but the release build runs in CI stage 2, in `CLIENT/…/Dockerfile` (E2E stack, staging) | Decision 1a; R-SUG-01 reworded in SPEC v2.5-2 |
| F3 | `Contact:CriteriaSuggestion` reads like an API setting, but R-SUG-01 forbids an endpoint, and the client has no runtime configuration (one image, `apiUrl` `/api`) | Decision 2a; SPEC v2.5-1, ARCHITECTURE_v25 C110 |
| F4 | UX copy v15 §5.15: `suggest.external` held both texts in one row („bzw. …“ in the notes); `check-i18n.mjs` rejects a `de.json` key that is not in the UX copy | UX copy v16 U85 (decision 4) |
| F5 | `suggest.mailBody` starts with „…betrifft das Kriterium: {{criterionKey}}“ and contains `%0A%0A` (already URL-encoded line breaks) | Not used from the footer (decision 4); the encoding is an open point for S11 (P1) |
| F6 | The footer is not designed (Handoff §5.3) and not in `figma_code_mapping_procureaccess_v1.md` | No `@figma` tag, no mapping change |

## 2. Changes

| Area | Change |
|---|---|
| Client setting | `criteriaSuggestion: 'mailto:kriterium-vorschlagen@example.invalid'` in both environment files, with a comment naming R-SUG-01, O-6 and the release check |
| `core/contact/criteria-suggestion.ts` (new) | Token `CRITERIA_SUGGESTION_CHANNEL` (`providedIn: 'root'`, default from the environment); `parseSuggestionChannel()` accepts `mailto:<address>` (no query) or an https URL and throws otherwise; `suggestionHref()` returns the URL as configured or `mailto:<address>?subject=<encoded>` |
| `layout/footer/` | First link `shell.footer.suggest` with the visually hidden suffix `suggest.external` (mail) or `suggest.external.url` (URL) inside the link; the href follows the language through `TranslateService.stream('suggest.mailSubject')`; same tab, no `target` |
| Texts | `docs/ux_copy_procureaccess_v16.md` (U85); `AREAS` += `suggest`; `node scripts/check-i18n.mjs --write` added the six `suggest.*` keys to `de.json` |
| Release check (new) | `scripts/check-release-config.mjs`: reads `criteriaSuggestion` of `environment.prod.ts`; fails if it is missing or set twice, neither `mailto:<address>` nor an https URL, or on a `.invalid` address or host. Self-test `scripts/check-release-config.test.mjs` (6 tests, `node:test`) |
| CI | Stage 1 step „Release check self-test“; the check itself is not run in CI (decision 1a) |
| Tests | `core/contact/criteria-suggestion.spec.ts` (12 tests: both forms, seven refused values, href, the environment default); `layout/footer/footer.spec.ts` (link order and hrefs, accessible name with each suffix) |
| Documents | SPEC_v2 → v2.5 (v2.5-1 – v2.5-3, R-SUG-01, O-6 row); ARCHITECTURE_v25 (C110; C85, §9.7 and O-6 annotated); TESTING_v37 (V116, §3.4, §8.2 stage 1); UX copy v16; CLAUDE.md §3, §4, §5 |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y; review with `/engineering:code-review` on the diff. N+1: not applicable. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | Security: the href comes from a build-time value, not from user input; the parser refuses `javascript:`, `http:` and anything else, and Angular's URL sanitizer allows `mailto:` and `https:` | – |
| R2 | A wrong value throws when the footer is created, which breaks the shell of that build. Intended (fail fast): both environment values are covered — the dev value by `criteria-suggestion.spec.ts`, the release value by the release check's self-test (CI stage 1), which accepts only the placeholder or a valid value | Won't fix |
| R3 | Client parser and release check use the same rule (same `mailto:` pattern, `https:` through `URL`) in two places; a shared module is not possible between the Angular build and a dependency-free Node script | Won't fix: the self-test reads the real file, so a drift shows on the repository's value |
| R4 | The subject is `''` until the translations are loaded, so the href briefly has an empty subject | Won't fix: `de.json` loads before the first interaction; no visible effect |
| R5 | a11y: the suffix is inside the link, so the accessible name is „Kriterium vorschlagen (öffnet Ihr E-Mail-Programm)“ (tested); the link keeps the footer's 48 px target and focus ring; it wraps at 320 px like the others | – |
| R6 | With the placeholder, staging users who click the link get a mail addressed to `…@example.invalid`, which their mail program cannot deliver | Won't fix: decision 3a; ends with O-6 |
| R7 | `suggest.mailBody` stores `%0A%0A` as text; S11 must decode it before encoding the body, or the mail shows `%0A` | Follow-up: open point P1 |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `npx ng test` (Node 24) | ✅ 33 files, 147 tests |
| 2 | `npm run lint` | ✅ all files pass |
| 3 | `npm run build:release` | ✅ builds with the placeholder (decision 1a); one warning about a CSS selector `50%` in the critical CSS, not touched by this change (no style changed) |
| 4 | `node --test` on the four script self-tests | ✅ i18n 11, Figma 10, invariants 36, release check 6 |
| 5 | `node scripts/check-release-config.mjs` | ✅ fails as intended: exit 1, „… is the placeholder (domain .invalid); set the channel decided in O-6“ |
| 6 | `node scripts/check-i18n.mjs` | ✅ 109 keys of `common, shell, errors, legal, home, suggest`, 0 errors (`docs/ux_copy_procureaccess_v16.md`) |
| 7 | `node scripts/check-figma-links.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending) |
| 8 | `node scripts/check-invariants.mjs` | ✅ passed (`docs/TESTING_v37.md`) |
| 9 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant |
| 10 | E2E on the E2E stack (`npm run stack:up`), Chromium and Firefox: `e2e/shell/` (A11Y-01, A11Y-08 on every shell state incl. the footer; reflow at 320 px), `journeys/` (J1) and `smoke/` | ✅ 60/60 shell, 8/8 J1 and smoke |
| 11 | Backend | – not touched |
| 12 | CI on the pull request (WebKit runs only there) | see the pull request |

## 5. Open points

| # | Point | Who / when |
|---|---|---|
| P1 | S11 (R-LST-13, M2.6): the link at the end of the criteria list with `suggest.linkContext` and the body `suggest.mailBody` with the criterion key. Its text holds `%0A%0A` (already encoded); decode it to line breaks before `encodeURIComponent`, or change the UX copy text to real line breaks | M2.6 |
| P2 | O-6 (project team): the real address or form URL goes into both environment files; then `check-release-config.mjs` passes | Project team, before release |
| P3 | No production release pipeline yet (ARCHITECTURE §10.3: manual promotion), so nothing runs the release check automatically; the release step should call `node scripts/check-release-config.mjs` | When the release pipeline is built |
| P4 | GP.3 (R-SHL-07): add the legal-page placeholders to the release check; ROADMAP GP.3 still says „production build fails“ | GP.3; ROADMAP in the control chat |

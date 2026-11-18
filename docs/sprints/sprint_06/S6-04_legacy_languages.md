<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-04 — Remove the legacy UI languages

> **Sprint:** 06 · **Task:** S6-04 · **Issues:** #368 (closed by this task), #158 (M3 group of `LEGACY_EXCEPTIONS`)
> **Requirements / invariants:** none (SPEC R-SET-01, SD12: German is the only UI language); `check-i18n` stays green; A11Y-05 unchanged
> **Status:** done — pull request open
> **Scope:** `en.json`, `es.json`, `fr.json`, `EnLanguage`, `LanguageService` and every reference go; the M3 group of `LEGACY_EXCEPTIONS` is empty. Nothing else.
> **Owner decisions:** none

## Plan (written before the change)

Starting point: S5-25b removed the settings page with the language switch, so nothing selects another language. Files: the three JSON files, `core/models/language.enum.ts`, `features/settings/services/language.service.ts`, `app.config.ts`, `language.interceptor.ts`, `with-settings.ts` and its spec, `eslint.config.js`, CODEBASE. Invariants activated: none. Tests: `ng test`, lint, `build:release`, `check-i18n`, `check-invariants`, `check-figma-links`, `check-icons`, `reuse lint`. No EF migration, no backend change. One session.

## 1 Starting point

| # | Finding | Consequence |
|---|---|---|
| 1 | `angular.json` and `nginx.conf` name no language file (the loader reads `./i18n/<lang>.json`; nginx serves `/i18n/` as a directory) | Deleting the files needs no configuration change |
| 2 | `languageInterceptor` sent `Accept-Language` from `LanguageService.get()`, which always answered `de`; the catalog endpoint honours `de` and `en` and would answer English for a browser with an English header | The header stays, with the constant `DEFAULT_LANGUAGE` |
| 3 | Specs that call `translate.use('en')` (`translate-plural.pipe.spec`, `page-title.strategy.spec`, `document-language.spec`) register their own test translations | Untouched; they do not read `en.json` |
| 4 | `check-i18n.mjs` reads only `de.json` | Green without change |

## 2 Changes

- Deleted `public/i18n/{en,es,fr}.json`, `core/models/language.enum.ts`, `features/settings/services/language.service.ts`.
- `app.config.ts`: `DEFAULT_LANGUAGE = 'de'` (a string).
- `language.interceptor.ts`: sends `DEFAULT_LANGUAGE`, injects nothing.
- `with-settings.ts`: `onInit` calls `TranslateService.setFallbackLang` and `use` directly; spec provides a `TranslateService` stub.
- `eslint.config.js`: the `language.service.ts` entry and its M3 comment removed. **`LEGACY_EXCEPTIONS` now: M3 0 files, M2 5 files, M5 10 files (15 in all, was 16).**
- `docs/CODEBASE_v1.md`: §2.76 paragraph and §3 layout line corrected, S6-04 paragraph added.

## 3 Self-review

| Finding | Sorting |
|---|---|
| Dropping the `Accept-Language` interceptor would let the browser's header reach the catalog | Kept the header (decision recorded in section 1) |
| `docs/ARCHITECTURE_v80.md` C161 still names `LanguageService` | Won't fix: a change row is history (CLAUDE §2) |
| `docs/TECH_DEBT_v5.md` still lists #368 | Won't fix here: the new TECH_DEBT version is written at sprint end (process step 14) |

## 4 Checks

- `npm run lint`: all files pass; `npx ng test`: 84 files, 822 tests passed; `npm run build:release`: ok.
- `check-i18n`, `check-invariants`, `check-figma-links`, `check-icons`, `reuse lint`: green (run before the push).
- E2E: left to CI (no local stack by instruction).

## 5 Open points

- TECH_DEBT: #368 and the M3 entry of the #158 row to be closed in the sprint-end TECH_DEBT version.
- #158 itself stays open (M2 and M5 groups remain).

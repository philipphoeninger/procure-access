# S4-14b — E2E flake: S10 use-case combobox keeps the typed text

> **Sprint:** 04, unplanned fix found while S4-14 (#268) was in CI (owner decision 2026-11-11: a separate pull request before #268). **Issues:** #269. **Requirements / invariants:** none changed; keeps A11Y-02 (J1 by keyboard only) stable. No TESTING version.
> **Status:** done — pull request #270 open.
> **Scope:** the E2E helpers that pick a use case in the S10 combobox. No app change.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `main` went red after #267 (run 43007944257, chromium, `s10-step1.spec.ts:72`: `HR` instead of `HR-Anwendung`); #268 failed the same way (run 43010111201, mobile-chrome, `s10-reset-submit.spec.ts:61`: `Report` instead of `Reporting-Anwendung`) | A flake in the S10 tests of S4-10 – S4-13, not in the changes of #267 or #268 |
| F2 | `MatAutocompleteTrigger` clears the highlighted option one task after the option list changes (`options.changes` → `delay(0)` → `_resetActiveItem()`, `@angular/material/fesm2022/autocomplete.mjs` l. 639–642) | An ArrowDown just after the last keystroke can be undone before Enter; Enter then selects nothing |
| F3 | `s10-step1` already waited for the final option count and still failed | Waiting for the list cannot close the gap; the helper has to check that the option is highlighted and retry |

## 2. Changes

| File | Change |
|---|---|
| `e2e/support/combobox.ts` (new) | `chooseFirstOption(page, combobox, name)`: waits for the first option's name, presses ArrowDown only while `aria-activedescendant` is empty, checks that it names the first option, presses Enter and repeats until the field shows `name` (`expect.toPass`, 10 s) — keyboard only |
| `e2e/configurator/s10-step1.spec.ts`, `s10-steps-2-4.spec.ts`, `s10-reset-submit.spec.ts`, `e2e/shell/reflow.spec.ts`, `e2e/journeys/j1-configurator.spec.ts` | Use the helper instead of ArrowDown + Enter + `toHaveValue` |
| `CLAUDE.md` | §3/§4 name the helper |

## 3. Self-review

| Area | Result |
|---|---|
| Correctness | The loop presses ArrowDown only when nothing is highlighted, so it never moves past the first option; Enter without a highlighted option leaves the typed text, which the next round fixes |
| Keyboard only | No `click`, `tap` or mouse; J1 keeps A11Y-02 |
| App | Unchanged; a person cannot press two keys within one task, so the race has no user impact |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npm run typecheck`, `npm run lint` in `e2e/` | Pass |
| C2 | Local E2E stack: the five specs with `--repeat-each=5` on chromium, firefox, mobile-chrome | 375 of 375 pass |
| C3 | WebKit projects | Left to CI (no local WebKit) |

## 5. Open points

None.

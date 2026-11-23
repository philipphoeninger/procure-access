# S6-06 — Firefox crash after saving; S10 step-1 axe flake

> **Sprint:** 06 · **Task:** S6-06 (0.5 points) · **Issues:** #355, #339 (TECH_DEBT_v5 §3.4)
> **Requirements / invariants:** A11Y-01 and A11Y-02 stay green; A11Y-02's J1 part and the M3 exit check (EXP-06) run in Firefox again; no invariant flipped
> **Status:** done — draft pull request open (2026-11-23)
> **Scope:** exactly the row of S6-06: E2E helper and specs, documents. No server or client change, no EF migration, no new dependency, no UX key. No Figma comparison, because no screen changed.
> **Owner decisions:** none needed for the task. S5-20b (Firefox skips J1's second part until #355 finds why) has run its course: the cause is found. After the report (2026-11-23): the older Firefox workarounds are retired in follow-up #434; the bug is not reported to Mozilla.

## 0. Plan (written before the code, confirmed by the owner on 2026-11-23)

- **Starting point:** `main` at efaea3a7. Branch `sprint06/S6-06-firefox-crash-s10-flake`. No other session ran E2E locally.
- **#339:** `openPage` of `s10-step1.spec.ts` waits for the field's fade-in, without a fixed sleep.
- **#355:** reproduce, check a normal Firefox, remove one factor at a time, repeat in CI on a throwaway draft pull request, fix only if small and certain.
- **Invariants:** none activated. **Migration:** none. One session.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | J1's second part, `fixme` lifted, Firefox with two workers: 4 of 4 runs crashed. Three crashed in axe on S11 **before** saving, one after it | „After saving“ was one symptom of a wider crash |
| F2 | The kernel log shows every crash as a write to address 0 at the same instruction of Firefox's `libxul`. The core dump shows a function that compares two lists and then sets a flag through a null pointer, called from JavaScript | A browser bug, not an error of the app |
| F3 | Playwright's name lookup and axe both read `element.ariaLabelledByElements`; Playwright in an isolated world, axe in the main world. The client never reads it | Candidate cause, and it fits why every earlier workaround concerned `aria-labelledby` |
| F4 | **Smallest case**, without app and axe: `<h2 id="h">…</h2><section id="s" aria-labelledby="h">`, one read in the main world and one `getByRole('region', { name })`: 8 of 8 crash, in either order. Reads in only one world: 0 of 8 | Cause confirmed |
| F5 | Stock Firefox 160 over WebDriver BiDi, main world and a sandbox: 5 segfaults in 5 tabs. Two reads in one world, in either world: none | Not specific to Playwright's Firefox build |
| F6 | #339 appears once in the failed CI E2E jobs since 2026-11-05 (2026-11-14, mobile Safari), before the helper's general wait of S5-25b. That wait only sees animations already running | The spec still needs its own wait |

## 2. Changes

1. **`e2e/support/axe.ts`.** `keepAxeOffLabelledByElements`, called first in `expectNoAxeViolations`, deletes `ariaLabelledByElements` from `Element.prototype` in the main world, in Firefox only. Axe then resolves `aria-labelledby` from the attribute. Compared with and without the property: identical violations, incomplete results and passes on a fixture with valid, broken, mixed, empty and hidden references and on the start page, S10 and S01.
2. **`e2e/support/axe-helper.spec.ts`.** Regression test on a named region: axe, name lookup, axe. It crashed 3 of 3 times before the change.
3. **`e2e/journeys/j1-configurator.spec.ts`.** The `test.fixme` for Firefox is gone. `tabTo` takes `Tab` or `Shift+Tab`, as in J2. The step to S14 backs up to „Konto“, because past the end of the page Firefox goes on inside `main` and the forward search never reached the header. The `fixme` had hidden this.
4. **`e2e/configurator/s10-step1.spec.ts`.** `openPage` waits for `mat-form-field-animations-enabled` on the page's one field and for the end of its animations. `s10-steps-2-4.spec.ts` needs no change: the stepper renders only the current step, so this field is gone on its axe states.
5. **Docs.** TESTING v112 (V407 – V411; v110 deleted), CODEBASE §2.114, the Firefox sentence of the E2E rule in `CLAUDE.md`. §3 of CODEBASE is unchanged (no new folder or code file). TECH_DEBT stays at v5 (new version at the end of the sprint).

## 3. Self-review (CLAUDE §6.7, §6.8)

Security: test code only; no secret, no request to a third party. Correctness: the property is removed only in Firefox and only in the page under test, and the comparison in §2.1 shows the same axe results. N+1: not applicable. Accessibility: no screen changed; the suppression list stays empty.

| Finding | Sorting | Reason / fix |
|---|---|---|
| The helper changes only the top frame; axe also runs in iframes | won't fix | The client has no iframe |
| The property stays removed until the document is replaced | won't fix | Nothing in the client reads it; only Firefox test pages are affected |
| `settled` in `s10-step1.spec.ts` would wait forever on an endless animation | won't fix | S10's only spinner shows during the submit of step 4, never when the page opens; 400 runs passed |
| The workarounds of V129 – V378 and the rule of V229 are probably no longer needed | follow-up | #434; outside this row |
| `members-page.html` still has a named section, against V229 | follow-up | #434; harmless with the fix |
| The Firefox bug is not reported to Mozilla | won't fix | Owner decision of 2026-11-23 |

## 4. Checks

- **CI experiment**, draft pull request #432 (closed unmerged), run 45176232963:

| Job | Result |
|---|---|
| Firefox, J1 second part, fix switched off | 9 of 10 crashed |
| Firefox, J1 second part, fix on | 40 of 40 passed |
| Firefox, whole suite twice, fix on | 336 passed, 2 skipped (forced colours are Chromium only), no crash |
| WebKit, `s10-step1` × 40 | 200 of 200 passed |
| Mobile Safari, `s10-step1` × 40 | 200 of 200 passed |

- **Locally** (API and client from source, throwaway database): J1's second part in Firefox with two workers, 4 of 4 crashed before and 6 of 6 passed after; the helper's self-test 12 of 12 in Firefox.
- **Not completed locally:** the run of Firefox, Chromium and mobile Chrome was stopped by the system at 331 of 507 tests because memory ran low, with no failure until then. It was not restarted. The CI of the pull request runs all five projects and is the check for the J1 change on Chromium and WebKit.
- **#339** was rare (one failure in the CI history), so 400 passing runs show no recurrence but cannot prove the cause.
- `npm run typecheck`, `npm run lint` (e2e), `check-invariants`, `check-figma-links`, `check-i18n` and `reuse lint` pass. No .NET or client test level is touched.

## 5. Open points

1. **Retire the Firefox workarounds: #434 (owner decision, 2026-11-23).** CSS locators and notes in eleven spec files, J2 without axe on S31 (V341), and the rule that a new `<section>` has no accessible name (V229), with repeated Firefox runs in CI, before step 6, because SIG-09 needs named K1/K2 regions there.
2. **Mozilla: not reported (owner decision, 2026-11-23).** The helper stays for as long as Firefox has the bug; recheck with the next Playwright update.
3. **`members-page.html`** has a `<section aria-labelledby>` from S5-17b, against V229. Harmless now; it goes with #434.
4. **For M5 screens:** Firefox runs every new workspace page. Axe through `expectNoAxeViolations` no longer collides with name lookups, so new specs can use role and name lookups. A test must not read `ariaLabelledByElements` itself in `page.evaluate`. Of the Firefox crashes in CI since 2026-11-16, six were in `workspace/projects.spec.ts`; that spec keeps its workarounds until #434.
5. **Not explained:** why the crash in J1 showed only with two or more workers in S5-20b. The smallest case crashes with one page every time.
6. **O-S6-7** planned to move #355 to M5. When this pull request closes #355, that part of the decision falls away.

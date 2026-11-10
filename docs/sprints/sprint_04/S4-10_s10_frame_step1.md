# S4-10 — S10 frame and step 1: stepper, use-case combobox, accessible step names

> **Sprint:** 04, P0 task S4-10 (M2.5, 4 points). **Issues:** #52 (referenced, not closed: O-S4-9). **Requirements / invariants:** R-CON-01, R-CON-02; **CFG-03 → `active`** for step 1 (`TESTING_v42.md` V122; the Basisfunktionalität part follows with S4-11); A11Y-01 and A11Y-08 on the new page. New versions `TESTING_v42.md` (v40 deleted) and `ARCHITECTURE_v31.md` (C117; v29 deleted); Figma mapping v1.17.
> **Status:** done — pull request #262 open.
> **Scope:** the S10 page at `/kriterien` inside the shell: H1, intro, the linear four-step stepper with the compact layout below 600 px, step 1 as single-choice combobox with „Keiner davon passt“ last, the match-count announcement and the field error of S10-E; steps 2–4 as frames (question and hint only); the stepper icon in the icon list; `axe-core` in the first component test; `criteria.select` as an i18n area. Not in scope: checkbox groups of steps 2–4 and exclusions (S4-11), reset notice, „Bisherige Auswahl“, submit, page title, legacy switch (S4-12), J1 on S10 and the reflow list (S4-13).
> **Owner decisions (2026-11-11):** **O-S4-9 (a)** — the #52 suppression stays until J1 runs on S10 (S4-13); **O-S4-10 (a)** — CFG-03 is activated for step 1 now; **O-S4-11 (a)** — `axe-core` directly through a small helper instead of `vitest-axe`; **O-S4-12 (a)** — the stepper is a shared `pa-stepper` on CDK `CdkStepper`, not `mat-stepper`; **O-S4-13 (a)** — the per-step page title and the „Fehler:“ prefix of S10-E come with the page title of S4-12.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | S4-09 (#261) is merged: `withConfigurator()`, `CatalogApiService`, the selection rules; the dev and E2E stacks serve `T.1` | The page uses the slice as is; its component test runs the real slice with a fake API |
| F2 | #52 is on the **legacy** stepper of `/filters`, which J1 visits until S4-13 and which stays where no release is active (O-S4-3) | Removing the suppression in S4-10 would break J1 → O-S4-9 |
| F3 | SPEC R-CON-01 wants display-only step headers with `aria-current="step"`; the horizontal `mat-stepper` of Material 22 renders `role="tab"` headers that are focusable and clickable, and sets `aria-current` only when vertical | Contradiction between the row („linear mat-stepper“) and SPEC → O-S4-12 |
| F4 | CFG-03 also covers Basisfunktionalität (step 3, S4-11) | → O-S4-10 |
| F5 | `vitest-axe`: last stable 0.1.0 (2022), only a pre-release since; compatibility with Vitest 5 unknown | → O-S4-11 |
| F6 | `PageTitleStrategy` reads one key per route, without parameters; `pageTitle.criteria.step` needs `{{n}}`, S10-E wants the „Fehler:“ prefix | → O-S4-13 |
| F7 | `AREAS` of `check-i18n.mjs` matched key roots only; `criteria` would pull in the S11 texts (some under review in S4-02) | Areas can be prefixes |
| F8 | The icon list already held `create`, `done`, `warning` for the legacy `mat-stepper`; the Handoff names a check icon for finished steps | `check` added, subset regenerated |
| F9 | The API sorts use cases by German name (S3-19), so `T.1` starts with „Dienstreiseanträge“ | E2E asserts presence, not position |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `shared/components/stepper/stepper.{ts,html,scss}` (new) | `pa-stepper`: subclass of `CdkStepper` (`linear`, `[completed]` per `cdk-step`, `next()`/`previous()`); from 600 px an `<ol>` of the step names, current step `aria-current="step"` and bold, finished steps with icon `check` and hidden `common.stepper.done`; below 600 px the text line of `stepLabelKey`; a step change focuses the first heading of the new step and announces the step label | R-CON-01, O-S4-12, ARCHITECTURE C117 |
| `features/configurator/pages/criteria-select/criteria-select.{ts,html,scss}` (new) | S10 page: H1, intro, four `cdk-step`s. Step 1: `mat-autocomplete` combobox (substring filter, case-insensitive), „Keiner davon passt“ last with its second line and a divider, `criteria.select.step1.noMatch` as a disabled option, match count announced with `throttleMs: 1000`, the current answer shown again when typed text chose nothing; below 600 px the field scrolls to the top on focus and the listbox ends at the bottom of `visualViewport`. „Weiter“ without an answer: `mat-error` `criteria.select.error.noUseCase`, `aria-invalid`, focus into the field, list closed (S10-E). Steps 2–4: question (and hint for steps 3, 4), Zurück/Weiter. `@figma` 2134-3174, 2019-407, 2139-3332 (error) | R-CON-01, R-CON-02, CFG-03 |
| `app.routes.ts` | Route `/kriterien` (no page title key yet, O-S4-13; not linked, O-S4-3 switch in S4-12) | Row |
| `core/a11y/a11y-announcer.service.ts` | Option `throttleMs`: per key at most one announcement per interval; a call inside the interval waits for its end and a later call replaces it (the newest text wins) | Handoff §3.4 (first user: the match count) |
| `testing/axe.ts` (new) | `expectNoAxeViolations(context?)`: `axe-core` with the blocking tags of the E2E helper, colour contrast off (jsdom), one line per violating element | O-S4-11, TESTING V123 |
| `package.json`, `package-lock.json` | `axe-core` `^4.14.0` as dev dependency (the version `e2e/` uses). **Licence:** MPL-2.0, test tool only, never in the bundle or an image (CLAUDE §10: development tools need OSI approval and no commercial-use restriction) | O-S4-11 |
| `public/i18n/de.json` | The 38 `criteria.select.*` texts of UX copy v16 §5.4, written by `check-i18n.mjs --write` | Row |
| `scripts/check-i18n.mjs`, `.test.mjs` | `inAreas()`: an area is a key root or a deeper prefix (`criteria.select`); `AREAS` gets `criteria.select`; one new self-test case | F7 |
| `src/assets/fonts/material-symbols-icons.json`, subset `.woff2` + manifest | Icon `check`; subset regenerated (35 icons, 24 164 bytes) | Row, F8 |
| `e2e/configurator/s10-step1.spec.ts` (new) | On `T.1`: step list and combobox (wide), list open, filtered list, field error, a use case chosen by keyboard → step 2, 360 px text line and list; axe in both appearances on each state | A11Y-01, A11Y-08, O-S4-9 |
| `docs/TESTING_v42.md` (new; v40 deleted) | V122 CFG-03 `active` (step-1 part, step-3 part deferred to S4-11); V123 `axe-core` helper instead of `vitest-axe` (§3.2, §7.1); V124 #52 suppression kept until S4-13 | Invariant flip |
| `docs/ARCHITECTURE_v31.md` (new; v29 deleted) | C117 and §8.5 Steppers | O-S4-12 |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.17: S10 page built with three tags | Mapping upkeep |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-9 – O-S4-13 | Decisions recorded |
| `CLAUDE.md` | Header, §3 (S10 page, stepper, throttle, i18n prefix areas, axe helper), §4 (new folders) | Structure changed |

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the staged diff.

| Area | Result |
|---|---|
| Security | No user input leaves the browser except the chosen use-case id through the S4-09 client; typed text is only filtered locally and interpolated by ngx-translate (escaped by Angular); no storage, no secrets |
| Correctness | One answer at a time (the store holds it; a second choice replaces it); typed text that chose nothing is replaced by the current answer when the list closes, so the field never shows an unanswered value as answered; the field error depends on the draft, so typing does not hide it and a choice does. Mutation check: putting „Keiner davon passt“ first fails the three list tests of CFG-03 |
| Edge cases | No match → empty text plus „Keiner davon passt“; options not loaded → empty list (S4-12 handles `noRelease`); step change announces once per change; the throttle keeps the newest count |
| Performance | Filtering is linear over ≤ 10 use cases; one `visualViewport` resize listener per page, removed on destroy |
| a11y | Combobox with label, hint and linked error; step list without controls; focus to the step heading on change; the two lines of „Keiner davon passt“ joined with a space in the accessible name (`&ngsp;`); axe clean in unit tests and in E2E in both appearances |
| UI texts | Only `criteria.select.*`, `common.stepper.done`; no new wording |
| Licence | `axe-core` MPL-2.0, dev only; SPDX headers on all new files; `reuse lint` compliant |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | If a reload no longer lists the chosen use case (a use case set inactive during a session; inactive use cases are accepted but not listed, CAT-18), the field goes empty while the draft keeps the answer | Follow-up P3; not reachable with a pinned release unless the seed changes during a session |
| R2 | The text of „Keiner davon passt“ in the field comes from `translate.instant` and does not follow a language change | Won't fix: German is the only UI language |
| R3 | With `noRelease` or `failed` the combobox has no options and nothing says why | Follow-up P1 (S4-12, together with the switch to the legacy filter) |
| R4 | E2E found a colour-contrast violation on `mat-error` in the first run only: axe ran during Material's enter animation (measured colour `#B4271F` on `#FDF8FD` passes) | Fixed in the test: it waits for the animations as `start-and-error-pages.spec.ts` does |

No correctness or security bug that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test --watch=false` (Vitest) | 213 of 213 pass, 18 new: `criteria-select.spec.ts` (14, four tagged `INV:CFG-03`, axe on step 1 closed, open and in the error state), `a11y-announcer.service.spec.ts` (+4 throttle) |
| C2 | `npm run lint` (client); `npm run typecheck && npm run lint` (`e2e/`) | All pass |
| C3 | `npm run build:release` | Builds without warnings |
| C4 | E2E locally (stack with `T.1`): `configurator/` in Chromium, Firefox, mobile Chrome | 12 of 12 pass; full suite in Chromium 53 of 53 (WebKit and mobile Safari only in CI) |
| C5 | `node scripts/check-invariants.mjs` | Passed; CFG-03 found 4 times (FE) |
| C6 | `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | Exit 0 each (i18n: 149 keys incl. `criteria.select`; Figma: the page is checked — a wrong first node fails) |
| C7 | Self-tests of the five scripts (`node --test`) | 78 pass |
| C8 | `reuse lint` (Docker `fsfe/reuse`) | Compliant, 1099 / 1099 files |
| C9 | Backend | Not touched; left to CI |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | No explanation on the page when the filter options answer 404 or fail (R3); the 404 also opens the generic snackbar (S4-09 P1) | S4-12 with the switch to the legacy filter |
| P2 | `criteria.select.step2.hint` holds both sentences, but its UX copy note says the second one is shown only when something is excluded; S4-10 shows no step-2 hint | S4-11: split the key in the UX copy (owner wording) or show the text only when something is excluded |
| P3 | A chosen use case that a reload no longer lists empties the field (R1) | S4-11/S4-12: keep the shown text when the draft still holds the id |
| P4 | Steps 2 and 3 move on without a check, the „Weiter“ wait for a running reload (S4-09 P2) is open, and step 4 has no submit | S4-11 (checks, wait), S4-12 (submit) |
| P5 | The per-step page title and the „Fehler:“ prefix of S10-E (O-S4-13) | S4-12 |
| P6 | #52 and its suppression (O-S4-9) | S4-13, when J1 runs on S10; S10 joins `reflow.spec.ts` there |
| P7 | The Handoff (§5.1, §2.2) still says `mat-stepper` for S10, S35, S55; ARCHITECTURE C117 reads it as the Material look | Next documentation session: name `pa-stepper` in the Handoff |
| P8 | `pa-stepper` has no test of its own; the S10 page test covers it (list, line, focus, announcement, done suffix) | When S35 or S55 reuse it |

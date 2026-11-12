# Sprint 04 — The anonymous core flow on the new screens

> **Status:** plan (2026-11-10) — output of Phase 3, step 11 ("Sprint planning") of `ai_development_process.md`, created with the skill *Product Management/sprint-planning*.
> **Based on:** `ROADMAP_v4.md` (rest of M2), `TECH_DEBT_v3.md` (§3, §6, §9.1, §9.2), the highest versions of TESTING (§4.2, §4.2a, §4.6, §4.9, §4.11, §6.3, §9 step 2), ARCHITECTURE (§5.1, §7, §12 step 2, ADR-014, ADR-015), SPEC (§7.3, §8.1 – §8.3, §9.1, R-CON-*, R-LST-*), the Design handoff (§4.5 K5, §5.1, §5.4, §5.5), UX copy (§5.4 – §5.6, §5.16), `CLAUDE.md`, the sprint-03 close-out and protocols in `docs/sprints/sprint_03/`, and the GitHub issues and pull requests as of 2026-11-10 (28 open issues, all in `TECH_DEBT_v3.md`; Dependabot PRs #226 – #231 open).
> **Language:** English. **Dates:** none by design — the sprint is described in working days (D1 … D8) and halves, not calendar dates.
> **Task prompts:** `TASK_PROMPTS.md` in this folder (one ready-to-paste prompt per task).
> **Amended 2026-11-11:** S4-18 split into S4-18a (S11 summary, notices, action bar, suggestion link, page titles) and S4-18b (the `CriteriaListShown` event), owner decision O-S4-52. S4-29 added (Figma light colours pinned, owner request, decision O-S4-22; +1 point committed). It came after S10 had been merged (S4-13), so it runs before S11 (S4-14).

---

## Sprint 03 in one paragraph (carried in)

Every task (42.5 P0 + 5.5 stretch points) was done in about **3 development days** (close-out in `SPRINT_03.md`). M1 is finished. M2 has its catalog: versioned model, release import and activation by CLI, both catalog endpoints, and the BIT inklusiv texts of release 2026.1 as a folder. Two things carry over as conditions, not as tasks: release 2026.1 cannot be activated until the project team delivers priorities and titles (O-1/K-1, "later / unclear" on 2026-11-10), so **no stack outside the tests has an active release** (#241); and the UX copy gaps of #181 are still open — the owner asked Claude to generate them (S4-02).

---

## Sprint goal

**The anonymous core flow runs end to end on the new screens — configurator S10, criteria list S11 and export dialog S13 — against the catalog endpoints with the test release, keyboard-only and with axe in both appearances; M2 is finished except the priorities of release 2026.1.**

This is the first sprint that builds screens of the target design. The legacy filter stays reachable wherever no release is active (owner decision O-S4-3), so staging keeps a working filter until 2026.1 has its priorities.

### Success criteria

- [ ] **S10** per R-CON-01 – R-CON-10 at `/kriterien`, on the test release; **CFG-01 – CFG-04** and the configurator part of **CAT-12** `active`; #52 closed; the screen joins the reflow check (A11Y-09).
- [ ] **S11** per R-LST-01 – R-LST-10 and R-LST-13 at `/kriterien/ergebnis`, with the UX copy proposals of S4-02 approved by the owner; the FE part of **CAT-20** `active`; the first `CriteriaListShown` event is written.
- [ ] **Export** `POST /api/v1/exports` with XLSX and CSV per SPEC §8.3 and dialog S13: **EXP-01 – EXP-05, EXP-07**, **TEN-04**, **API-05** (anonymous export) and the export part of **CAT-12** `active`; the ARC-03 rule for `AdHocSelectionSource` written; `ExportCreated` written.
- [ ] **J1** (anonymous part: select → list → export) passes keyboard-only (**A11Y-02**, J1 part) and with axe in both appearances on all five browser projects; CAT-10 follows O-S4-5.
- [ ] **API-06** `active`; #42 bundle budget tightened after the rebuild.
- [ ] **Figma colours:** the light appearance uses the values of Handoff §1.2 (pinned with `mat.theme-overrides`); the dark appearance stays generated until the Figma dark mode and its review (O-S5); A11Y-01 and A11Y-08 stay green (S4-29).
- [ ] **Debt of TECH_DEBT_v3 §9.1 closed:** #203 (E2E reliability), #205, #233, #242, #239, #243; the Dependabot PRs reviewed.
- [ ] M2 exit criteria met except the manual screen-reader check of an exported file (TESTING §7.2), which the accessibility tester does after the sprint.

---

## Capacity

| Person | Available | Assumed capacity | Notes |
|---|---|---|---|
| Philipp | 8 development days (2 weeks × ~4 days) | **~70 points** | Documentation, WERTE.IT coordination, reviews, the owner decisions below and the review of the UX copy proposals (S4-02) are outside these days |
| **Total** | **8 days** | **~70 points** | |

**Basis:** sprint 02 ≈ 8, sprint 03 ≈ 16 points per development day (average ≈ 11). This sprint assumes ≈ 9 per day: it is the first screen sprint, with component tests, axe in two appearances, keyboard journeys and an E2E rewrite, and the spread between the two measurements is large. Sprints have so far ended early; the plan counts what fits into 8 days, not how long it will take.

**Planned load:** **50 points committed (71 %** of the assumed capacity; 49 before S4-29), of which 6.5 points are general debt (S4-01 – S4-07, S4-29; 9 %) and 9 points are M2 debt rows (M2.4d 1, M2.5d 4, M2.6d 1, M2.7d 3), + **6.5 points stretch**.

---

## Sprint backlog

Points use the roadmap scale; task estimates add up to the package estimates of `ROADMAP_v4.md`, debt items use the register's sizes. One task ≈ one Claude Code session and, as a rule, one pull request; a task may be split (like S3-20a/b) if its plan shows more than one session. Order = recommended working order (Sequence below). Invariants per task are a **proposal**; Claude Code confirms them against TESTING §4 in its plan.

### P0 — committed (50 points)

#### Debt and preparation — 6.5 points (S4-29 added 2026-11-11)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S4-01 | E2E reliability per **O-S4-1** (#203): the apt step of the Playwright install no longer fails `main` | 1 | Ten consecutive CI runs (incl. re-runs on a test branch) without an install timeout, or the chosen option's proof in the protocol; #203 closed |
| S4-02 | UX copy v17 with **proposals for every gap of #181** (TD-15): singular and zero forms of S2-12 F2, the storage rule for dotted keys (S2-13 P1), page-title variants of S11/S35 (S2-16 P2), UX keys of the problem types without one (S2-11 P1), reason text for the disabled „Löschen“ (S2-01 P3), soft hyphen in the legal titles (S2-18 P3), account button name (S2-15 P4), `selection-excluded` (S3-20a P3), mail body line breaks (S3-21 P1), header versions. Every proposal marked as such; `de.json` unchanged | 1.5 | **O-S4-4:** the owner approves (or edits) the proposals in the PR; #181 closed with the merge |
| S4-03 | `[Authorize]` on `UsersController`, `[AllowAnonymous]` on `ResetPassword`, 401 tests, the `!` on the user id removed (#205) | 0.5 | Anonymous calls answer 401; #205 closed |
| S4-04 | Scripts and licence PR: guard the loop variable of `setText` in `check-i18n.mjs` (#233, CodeQL alert 3); the CAT-10 fixture annotated as BIT inklusiv in `REUSE.toml` (#242) | 0.5 | Alert 3 closed on `main`; `reuse lint` green; both issues closed |
| S4-05 | Validate every release folder under `catalog/releases/` in CI per **O-S4-2** (#239) | 0.5 | A broken test folder fails CI with the validator's message; #239 closed |
| S4-06 | Review the open Dependabot PRs #226 (`@playwright/test` 1.65), #227 (Angular group), #228 (Vitest group), #229 (other group), #230 (**jsdom 31, a major**), #231 (xunit.v3 4.0.3): licences, changelogs, CI | 0.5 | Each PR merged or closed by the owner with the reason in the protocol |
| S4-07 | Documentation session (#243, TD-37): the sprint-03 findings in ARCHITECTURE, TESTING, SPEC, Handoff and the Figma mapping, and their headers naming current versions. ROADMAP v4 and the SPRINT_03 close-out are already done in the control chat; PRD, Kriterienkatalog and UX copy are the owner's | 1 | #243 closed for Claude's part; owner documents listed in the protocol |
| S4-29 | *(added 2026-11-11, after S10, before S11)* Figma light colours: the light values of Handoff §1.2 (the Figma variables) pinned with `mat.theme-overrides` instead of the generated palette (SPEC §7.5; TD-34, owner decision **O-S4-22**); the dark appearance stays generated until O-S5. A Figma value that breaks a contrast rule is reported, not changed | 1 | Every `--mat-sys-*` role of Handoff §1.2 has its Figma value in the light appearance (test or protocol table); A11Y-01 and A11Y-08 green; #240 keeps only its dark part |
| S4-30 | *(added 2026-11-12, from the Figma review, #280)* S10 step 1: the trailing icons of the Figma frames — the `error` icon while the field shows its error (Handoff §1.8, S10-E) and the clear button „Eingabe löschen“ (`common.actions.clearInput`, UX copy v23 U108) while the field holds text (owner decision **O-S4-61**); clearing empties the text and keeps the answer (protocol S4-30 §2) | 0.5 | Both icons as in Figma `2019:407` / `2139:3332`; clear by keyboard in E2E with axe in both appearances; UX copy v23, Handoff 1.23 |

#### M2.5 Configurator S10 — 13 points (+ 4 points M2.5d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S4-08 | An active test release in the E2E stack (and in development per **O-S4-3**): a seeder for the `E2E` environment imports and activates the test folder `T.1` (TESTING §6.3); `/api/v1/catalog/*` answers 200 there (#241) | 1 | E2E smoke test calls both catalog endpoints; #241 closed |
| S4-09 | Configurator state: `withConfigurator()` with the in-memory draft (R-CON-09, no browser storage), the catalog API client (filter options), the selection model with exclusions and the reset rule of the lower layers | 3 | Store tests for selection, exclusions and reset; nothing written to storage |
| S4-10 | S10 frame at `/kriterien`: linear `mat-stepper` with four steps and the compact layout below 600 px (R-CON-01), step 1 as single-choice combobox with „Keiner davon passt“ last (R-CON-02); accessible step names (#52, the only axe suppression goes); stepper icons in the icon list (subset regenerated); `vitest-axe` set up with the first component test | 4 | **CFG-03** `active`; #52 closed; `vitest-axe` runs in CI |
| S4-11 | Steps 2 – 4: app types with hints (R-CON-03), product parts with Basisfunktionalität checked and `aria-disabled` plus „Erklärungen anzeigen“ (R-CON-04), optional test types with link (R-CON-05); excluded options stay focusable with `aria-disabled` and their reason (R-CON-06) | 4 | **CFG-02, CFG-04** and the configurator part of **CAT-12** `active` |
| S4-12 | Reset notice in a status region naming the cleared layers (R-CON-07), „Bisherige Auswahl“ with „Ändern“ (R-CON-08), „Kriterienliste anzeigen“ → S11 with server-side validation (R-CON-10); page title; the configurator's legacy `LiveAnnouncer` calls through `A11yAnnouncerService`; empty states and button spinner (Handoff §6.2); the switch between S10 and the legacy filter per **O-S4-3** | 3 | **CFG-01** `active`; start page and header lead to S10 where a release is active, to the legacy filter elsewhere |
| S4-13 | J1 rewritten on S10 with keyboard only (A11Y-02, J1 part so far) and axe in both appearances; CAT-10 per **O-S4-5** incl. #91, #92; S10 added to `reflow.spec.ts` (A11Y-09) | 2 | J1 green on all five browser projects; #91, #92 closed or carried by O-S4-5 |

#### M2.4d Accept-Language — 1 point

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S4-23 | *(early, backend)* The catalog list endpoints honour `Accept-Language` (`de`, `en`; German texts as fallback while the release has no English) | 1 | **API-06** `active` |

#### M2.6 Criteria list S11 — 13 points (+ 1 point M2.6d, + 0.5 M2.8)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S4-14 | S11 state: criteria API client, list state in the draft (include/exclude, overrides with the recommendation kept, notes ≤ 1 000 characters), search (debounce 300 ms, client-side) and priority filter as logic | 3 | Store tests; the recommended level stays unchanged next to an override |
| S4-15a | Criterion card (compact) with K5 `pa-priority-indicator` (display; menu with S4-16) and the detail panel with its first section „Warum diese Priorität?“: rationale, matched rule (`appTypeAndUseCase` → `criteria.priority.rule.full`) and source „Empfehlung des Projektteams“ (R-LST-01, R-LST-02, CAT-02 FE part); S11 loads and lists the cards; `@figma` tags (split of S4-15, O-S4-28) | 2 | Component tests by role and name with axe (O-S4-11); Figma link check green |
| S4-15b | Detail panel, catalog sections: the Markdown lines of the texts as paragraphs and lists (S3-15 P6, O-S4-30), tabs for several procedures, examples, „So wird es geprüft“, „Gilt für“ with `appliesTo[]` in the criteria answer (O-S4-29), source line (Handoff §5.5) | 2 | Component tests; SPEC §8.2 and the endpoint test for `appliesTo[]` |
| S4-16 | Priority override via the K5 menu with reset (R-LST-03), include/exclude with excluded cards kept in the list (R-LST-04), note with length error (R-LST-05) | 3 | Component tests; errors per SPEC §7.1 |
| S4-17 | Search and priority filter UI with the result announcement (R-LST-06), „Geprüft nach …“ cards (R-LST-07), empty state for app types without criteria (R-LST-08) | 2.5 | FE part of **CAT-20** `active` |
| S4-18a | Summary of the selection and the counts above the list (`criteria.list.summary.*`, `criteria.list.counts` with one plural key per level, UX copy v22 U106, O-S4-50; moved from S4-17 with O-S4-47), notices (R-LST-09; the draft note at the top, O-S4-53), action bar in the flow and sticky from 600 px width and height (R-LST-10; „Liste exportieren“ opens a placeholder until S4-21, „Liste speichern“ waits for step 3, O-S4-49), „Kriterium vorschlagen“ at the end of the list (R-LST-13, M2.8), `pageTitle` as an `AREAS` root (M2.6d; split of S4-18, O-S4-52) | 1.5 | S11 in `reflow.spec.ts` with summary, notices, an open panel and the action bar; component tests with axe |
| S4-18b | The first `CriteriaListShown` caller (R-MET-01, R-CON-10): `POST /api/v1/configurator/criteria-list-shown` in the Configurator module, called by the client once per shown selection without any system state on failure (O-S4-48) | 0.5 | Event written once per shown list (integration and store tests) |

#### M2.7 Export — 8 points (+ 3 points M2.7d)

| ID | Task | Pts | Done when |
|---|---|---:|---|
| S4-19 | Export pipeline in the Export module with `AdHocSelectionSource` behind `POST /api/v1/exports` (R-LST-12): the server re-resolves priorities (EXP-07), the ad-hoc export persists nothing and keeps request bodies out of the logs (EXP-05), cache headers of catalog and export responses (TEN-04); the ARC-03 rule for `AdHocSelectionSource` | 4 | **EXP-05, EXP-07, TEN-04** and the export part of **CAT-12** `active`; ARC-03's deferred part closed |
| S4-20 | XLSX (ClosedXML) and CSV renderers per SPEC §8.3: header row, defined names, release version, UTF-8 with BOM, stable column order; the short-description column per **O-S4-6** | 3 | **EXP-01 – EXP-04** `active` |
| S4-20a | *(split from S4-20 by O-S4-68, 2026-11-12)* Texts of the export files (UX copy v24 `exportFile.*`, `Texts/de.json` in the Export module, second target of `check-i18n.mjs`) and the CSV renderer: UTF-8 with BOM, semicolon, CRLF, RFC 4180, „Katalogversion“ first, formula guard (O-S4-67) | (of S4-20) | **EXP-04** `active`, **EXP-03** CSV part (TESTING_v49) |
| S4-20b | *(split from S4-20 by O-S4-68)* XLSX renderer with ClosedXML 0.105.1: sheet „Kriterien“ as Excel table `Kriterien` (O-S4-66), sheet „Info“ with 🔧 `Export:ToolUrl` (O-S4-64), Dependabot rule for SixLabors.Fonts ≥ 2 | (of S4-20) | **EXP-01**, **EXP-02** `active`, **EXP-03** complete |
| S4-21 | Export dialog S13 (R-LST-11) incl. the error state and the full-screen dialog below 600 px; anonymous-export rate limit (SPEC §7.3) with the relaxed E2E value; `/health/ready` behind the same mechanism (#112); the same mechanism for `POST /api/v1/configurator/criteria-list-shown` (SPEC v2.10 §7.3, O-S4-56 (b)); `ExportCreated` written | 3 | **API-05** (anonymous export) `active`; #112 closed |
| S4-21a | *(split from S4-21 by O-S4-73, 2026-11-12)* API part: client IP from the forwarded headers of the trusted proxies (O-S4-69), the rate limiter of Platform with the policies anonymous export and `CriteriaListShown` (SPEC §7.3, relaxed E2E value) and `/health/ready` (at most 2 checks at a time, configurable timeout, O-S4-70, #112), 429 as Problem Details with `retryAfterMinutes`; `ExportCreated` written | (of S4-21) | **API-05** `active` for the anonymous export and `CriteriaListShown`; #112 closed |
| S4-21b | *(split from S4-21 by O-S4-73)* Client part: export dialog S13 (R-LST-11) with the error state, full screen below 600 px, the download; Problem Details from a blob error body and the request marker for a caller that shows every failure itself (O-S4-74); `errors.rateLimit.one` (UX copy v25 U110, O-S4-71); `criteria.export.outdated` with „Schließen“ only (O-S4-72) | (of S4-21) | S13 by keyboard with axe in both appearances; S13 in `reflow.spec.ts` |
| S4-22 | J1 complete: select → list → export with keyboard only and axe; M2 exit check (all but the manual screen-reader check of the file); #42 bundle budget tightened | 1 | J1 green; #42 closed; M2 exit criteria listed in the protocol |

### P2 — stretch (6.5 points, only if P0 is done)

| ID | Task | Pts | From |
|---|---|---:|---|
| S4-24 | Short description (Kurzbeschreibung) in the catalog model and the release format per **O-S4-6** (#237), with an ADR | 2 | TECH_DEBT_v3 TD-30 |
| S4-25 | M3.4 groundwork: entities `ContributorProfile`, `UserContributorLink`; rename `Approver` → `Moderator`; platform-role CLI | 2 | ROADMAP M3.4 |
| S4-26 | Encrypt the Data Protection key ring at rest and document key store, backup and restore (#235) | 1 | TECH_DEBT_v3 TD-27 |
| S4-27 | Licence notices of the NuGet packages in the API image (#187) | 1 | TECH_DEBT_v3 TD-22 |
| S4-28 | Dead code: `dialog.service.ts`, the unused `Logger` in `BaseCrudController`, the `/assets/` location in `nginx.conf` | 0.5 | TECH_DEBT_v3 TD-32 |

Stretch items are pulled in order; if one is started but not finished, it goes back to its backlog as a whole.

---

## Sequence

| Phase | Days (approx.) | Tasks | Checkpoint |
|---|---|---|---|
| Start | D1 | S4-01 (first), S4-02, S4-03 – S4-07, S4-08, S4-23 | CI reliable; UX copy proposals in review; **owner decisions O-S4-1 – O-S4-3, O-S4-5 taken** |
| First half | D1–D4 | S4-09 – S4-13 (S10); S4-19, S4-20 (export backend, independent of the screens) | **Mid-sprint check (end of D4):** S10 merged with J1 on it and the export endpoint live (≈ 30 points). If not: drop the stretch, then S4-22 and S4-17 move to sprint 05 |
| Second half | D5–D8 | **O-S4-4 approved**; S4-29 (added, before S11), S4-14 – S4-18 (S11); S4-21, S4-22 | Core flow end to end; M2 exit check |
| Buffer | remaining | stretch S4-24 – S4-28 | – |

Frontend (S4-09 – S4-18, S4-21 UI part) and backend (S4-03, S4-05, S4-08, S4-19 – S4-21 API part, S4-23) can run in parallel sessions (worktrees). Local E2E runs share the host ports: in parallel sessions only one runs E2E locally, the others rely on CI.

---

## Dependencies

| Item | Depends on | Status |
|---|---|---|
| everything | S4-01 (so merges are judged on real failures) | internal, D1 |
| S4-01 | **O-S4-1** | Philipp, D1 |
| S4-05 | **O-S4-2** | Philipp, D1 |
| S4-08, S4-12 | **O-S4-3** | Philipp, D1 |
| S4-09 – S4-13 | S4-08 merged | internal |
| S4-10 | S4-09 | internal |
| S4-11, S4-12 | S4-10 | internal |
| S4-13 | S4-12; **O-S4-5** | internal / Philipp |
| S4-29 | S4-13 merged; **O-S4-22** | internal / Philipp (decided) |
| S4-30 | S4-10 merged; **O-S4-61** | internal / Philipp (decided) |
| S4-14 – S4-18 | S4-29 merged (S11 is built and axe-checked in the final light colours); S4-12 merged (S10 hands the selection over); **O-S4-4** (approved UX copy v17) for the texts | internal / Philipp |
| S4-16, S4-17 | S4-15a | internal |
| S4-18a | S4-14 – S4-17 | internal |
| S4-18b | S4-14 – S4-17 (independent of S4-18a) | internal |
| S4-20 | S4-19; **O-S4-6** for the short-description column only | internal / Philipp (decided: O-S4-6 (a), O-S4-62 – O-S4-68 (a); split into S4-20a and S4-20b) |
| S4-21 | S4-19, S4-20 (API part); S4-18a (the action bar opens the dialog) | internal (decided: O-S4-69 – O-S4-74 (a); split into S4-21a and S4-21b; S4-21b after S4-21a) |
| S4-22 | S4-13, S4-18a, S4-18b, S4-21 | internal |
| S4-24 | **O-S4-6** | Philipp |
| **Invariant flips** | S4-10 – S4-13, S4-17, S4-19 – S4-21, S4-23 write a new TESTING version. Preferably one at a time; if two run in parallel, the later PR rebases and renumbers its TESTING version (as done for SPEC in S3-27) | internal |
| Outside the sprint | Staging redeployed from `main` with `ASPNETCORE_ENVIRONMENT` checked (#236, TD-28); M1 milestone closed; O-1/K-1 and O-6 from the project team; the accessibility tester's screen-reader check of an exported file | Philipp, project team, tester |

### Owner decisions for this sprint

| # | Decision | Needed by | Claude's proposal |
|---|---|---|---|
| O-S4-1 | E2E flake #203: (1) run the E2E jobs in the pinned Playwright container image, (2) a shorter limit plus one retry of the apt step, or (3) cache the `.deb` packages | S4-01 | (2) first — smallest change; (1) if `main` still goes red (licence check of the image) |
| O-S4-2 | Release-folder validation (#239): a CI step that runs the validator on every folder, or a `catalog validate <version>` command used by CI | S4-05 | CI step running the existing validator; a command only if the project team needs it locally |
| O-S4-3 | Until 2026.1 can be activated, where does S10 run, and what happens to the legacy filter? (a) S10 at `/kriterien`; start page and header lead to S10 only where a release is active, otherwise to the legacy `/filters`; (b) also activate the test release `T.1` in Development (staging stays on the legacy filter); (c) replace the legacy filter everywhere now (staging shows an empty configurator until O-1) | S4-08 | **(a) + (b):** staging keeps a working filter, development and E2E show S10 with test data; the legacy filter goes with the activation of 2026.1 |
| O-S4-4 | UX copy v17: approve or edit the proposals for the #181 gaps (review of the S4-02 PR) | S4-14 – S4-18 | — (Claude drafts, the owner decides the wording); **decided: approved unchanged**, owner 2026-11-11, with U98/U99 for S4-11 (#248) |
| O-S4-5 | CAT-10 after S10: keep the API part while the legacy endpoints exist (fix #91, #92) and retire the E2E part with the legacy filter; or move a new characterisation to S10 on `T.1`; or retire CAT-10 as a whole once the legacy filter is unreachable | S4-13 | Keep the API part, retire the E2E part — J1 on S10 takes over the end-to-end check; **decided (a)**, owner 2026-11-11 (TESTING_v45 V136) |
| O-S4-6 | Short description (#237, TD-30): a field in `CriterionRevision` and the release format, or derived from existing text; until then the export column stays empty? | S4-20 (column), S4-24 | A field, with an ADR; the column is written empty until the project team fills it; **decided (a)**, owner 2026-11-12: S4-20 writes the column empty, S4-24 adds the field |
| O-S4-7 | `POST /api/Users/reset-password` sets any user's password from the e-mail address alone (no token; found while planning S4-03): (a) remove the action, (b) put it under `[Authorize]` like the rest, (c) `[AllowAnonymous]` as the S4-03 row says | S4-03 | (a); **decided (a)**, owner 2026-11-10 — the client resets through the token-based `POST /api/reset-password` |
| O-S4-8 | Where the S10 state lives: (a) `withConfigurator()` as a slice of the root store with the draft and the filter options answered for it, `withCatalog()` later with the legacy filter; (b) start `withCatalog()` now with the filter options; (c) a separate `ConfiguratorStore` outside the root store | S4-09 | (a); **decided (a)**, owner 2026-11-10, with the defaults of the S4-09 plan (release pinned by the draft, reload after each change of steps 1–3, reset per CFG-01) |
| O-S4-9 | #52 (the only axe suppression) is on the legacy stepper of `/filters`, which J1 visits until S4-13 and which stays where no release is active (O-S4-3): (a) keep the suppression until J1 runs on S10 (S4-13), S10 is checked without it; (b) name the legacy steps with new UX copy wording and drop the suppression in S4-10 | S4-10 | (a); **decided (a)**, owner 2026-11-11 — #52 closes with S4-13 |
| O-S4-10 | CFG-03 also covers „Basisfunktionalität is checked and cannot be unchecked“, which is step 3 (S4-11): (a) activate CFG-03 in S4-10 for step 1 and record the step-3 part as deferred; (b) activate it only in S4-11 | S4-10 | (a); **decided (a)**, owner 2026-11-11 (TESTING_v42 V122) |
| O-S4-11 | `vitest-axe` (row S4-10): its last stable release is 0.1.0 of 2022: (a) `axe-core` directly with a small helper in `src/app/testing/`; (b) `vitest-axe` 0.1.0 | S4-10 | (a); **decided (a)**, owner 2026-11-11 (TESTING_v42 V123) |
| O-S4-12 | SPEC R-CON-01 wants display-only step headers with `aria-current="step"`; the horizontal `mat-stepper` renders clickable tabs: (a) a shared `pa-stepper` on CDK `CdkStepper` with a list as header; (b) `mat-stepper` with its tabs made inert by CSS and `tabindex` | S4-10 | (a); **decided (a)**, owner 2026-11-11 (ARCHITECTURE_v31 C117) |
| O-S4-13 | S10-E also prefixes the page title with „Fehler:“, and `pageTitle.criteria.step` needs the step number, which `PageTitleStrategy` cannot pass yet: (a) both with the page title of S4-12; (b) both in S4-10 | S4-10 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-14 | S4-11: T.1 has an exclusion by a product part (audio → softwareTest), the UX copy had no reason text for it: (a) a proposal in UX copy v17 (#248), one reason line per layer; (b) S4-11 writes its own UX copy version; (c) leave it out | S4-11 | (a); **decided (a)**, owner 2026-11-11 (UX copy v17 U98) |
| O-S4-15 | S4-11 (S4-10 P2): `criteria.select.step2.hint` held a second sentence meant only for the excluded case: (a) split it into two keys, same wording; (b) always both sentences; (c) the hint only when something is excluded | S4-11 | (a); **decided (a)**, owner 2026-11-11 (UX copy v17 U99) |
| O-S4-16 | S4-11 activates CFG-04, whose submit button comes with S4-12: (a) activate the step-4 part, defer the submit part like O-S4-10; (b) add the button in S4-11; (c) move CFG-04 to S4-12 | S4-11 | (a); **decided (a)**, owner 2026-11-11 (TESTING_v43 V126) |
| O-S4-17 | S4-12, switch of O-S4-3: (a) start page and header always link `/kriterien`, and a guard of S10 opens the legacy `/filters` when the filter options answer 404; (b) the app asks once at start whether a release is active and the links follow (S10 still needs (a) for direct visits) | S4-12 | (a); **decided (a)**, owner 2026-11-11 (ARCHITECTURE_v32 C118) |
| O-S4-18 | S4-12: the UX copy has no text for failed filter options on S10: (a) no new text — 5xx shows the server page, no answer the offline notice, other failures the snackbar; (b) a new UX copy key with „Erneut versuchen“ | S4-12 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-19 | S4-12, hand-over to S11 (R-CON-10): (a) „Kriterienliste anzeigen“ calls `GET /api/v1/catalog/criteria` with button spinner, accepted → placeholder `/kriterien/ergebnis`, refused → error at the step; (b) open S11 at once and leave the check to S4-14 | S4-12 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-20 | S4-12: when the reset notice appears: (a) only when a lower layer held a selection (Basisfunktionalität does not count); (b) on every change of a higher layer | S4-12 | (a); **decided (a)**, owner 2026-11-11, with the defaults of the S4-12 plan (S11 placeholder, „Ändern“ per layer, page title without announcement, no S10 empty states, only the legacy filter's `LiveAnnouncer`) |
| O-S4-21 | S4-13 (O-S4-9 said „#52 closes with S4-13“): the legacy `/filters` stays live where no release is active, and two shell tests still opened it: (a) move those tests to `/kriterien`, drop the suppression, keep #52 open until the legacy filter goes; (b) the same, but close #52 now; (c) keep the suppression and #52 until the legacy filter goes | S4-13 | (a); **decided (a)**, owner 2026-11-11 (TESTING_v45 V137) |
| O-S4-22 | Theme colours (#240, TD-34): pin the light values of Handoff §1.2 now; the dark values wait for the Figma dark mode (O-S5). Added as O-S4-7, which S4-03 already used | S4-29 | **Decided 2026-11-11** (owner: add S4-29 to sprint 04), with the defaults of the S4-29 planning: renumbered to O-S4-22; S4-29 after S4-13 and before S4-14; the pinned values give way to the legacy contrast setting (`.contrast-more`); P0 heading 50 points |
| O-S4-23 | S4-29: the page background of the shell is `--mat-sys-background` (Material paints `mat-sidenav-content` with it), which Handoff §1.2 does not list, so it keeps the generated `#FDF8FD`, while §1.2 says the page background is Surface `#FEF7FF`: (a) pin `background` to the Surface value and add the row Schemes/Background to §1.2; (b) leave it generated until O-S5 | S4-29 | (a): one line in `$figma-light`, invisible difference (1 step per channel), and §1.2 then names every colour the page shows; **decided (a)**, owner 2026-11-11, in the S4-29 pull request (Handoff v1.17) |
| O-S4-24 | S4-14: the user's list decisions (include/exclude, overrides, notes) when „Auswahl ändern“ changes the selection: (a) keep them for the session by criterion key, also for criteria that leave the list; (b) keep them only for criteria still in the new list; (c) reset them on every change of the selection | S4-14 | (a); **decided (a)**, owner 2026-11-11 (ARCHITECTURE_v33 C119) |
| O-S4-25 | S4-14: what the priority filter of S11 compares: (a) the level that applies now (override, else recommendation); (b) always the recommended level | S4-14 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-26 | S4-14: what the S11 search looks at: (a) what the compact card shows — key/number, title, plain-language title, original titles of the procedure texts; (b) also the full BIT inklusiv texts; (c) (a) plus the user's notes | S4-14 | (a); **decided (a)**, owner 2026-11-11: case-insensitive substring, umlauts not folded |
| O-S4-27 | S4-14: the weight of an override to B when the recommendation is A or Info (no recommended weight; R-LST-03 „editable 1–10“ 🔧): (a) empty until the user sets one; (b) a new 🔧 default | S4-14 | (a); **decided (a)**, owner 2026-11-11; the range 1–10 is a client setting in the environment files |
| O-S4-28 | S4-15 is more than one session: (a) S4-15a K5, compact card, S11 list and the priority section of the panel; S4-15b the catalog text sections; (b) one task | S4-15 | (a); **decided (a)**, owner 2026-11-11 (the priority section moved into S4-15a so no card is shown without its rationale, CAT-02) |
| O-S4-29 | „Gilt für“ (Handoff §5.5) has no data in the criteria answer (SPEC §8.2): (a) add `appliesTo[]` (the selected app types the criterion applies to) in S4-15b; (b) leave the section out | S4-15b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-30 | Markdown of the catalog texts: (a) a small renderer into Angular nodes (paragraphs, `- ` lists, `**bold**`, no raw HTML); (b) the legacy `ngx-markdown`/`marked` | S4-15b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-31 | Meta line when product part or EN 301 549 reference is missing: (a) clause from the `en301549` reference, two fragment keys (UX copy v18 U100); (b) the line only with both parts | S4-15a | (a); **decided (a)**, owner 2026-11-11; the wording of U100 confirmed unchanged by the owner in pull request #271 (2026-11-11) |
| O-S4-32 | H2 while `plainLanguage` is null (2026.1 before O-1): (a) the catalog `title`; (b) the source title line | S4-15a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-33 | „Geprüft nach …“ panel: Handoff §5.5 (text and note only) vs. R-LST-02/CAT-02: (a) the panel starts with „Warum diese Priorität?“; (b) as the Handoff | S4-15a | (a); **decided (a)**, owner 2026-11-11 (Handoff 1.18) |
| O-S4-34 | A failed S11 load (S4-14 P1): (a) no new text — the S11 request no longer suppresses the snackbar for `validation`/`selection-excluded`; (b) back to S10 with the error at the step; (c) a new key with „Erneut versuchen“ | S4-15a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-35 | The S11 frame shows „Prüfverfahren · aus Ihrer Auswahl in Schritt 4“ on „Geprüft nach …“ cards without a UX copy key: (a) leave it out; (b) a key in UX copy v18 | S4-15a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-36 | Links in catalog texts (`[url](url)` in 12.1.2 and 12.2.4), outside the subset of O-S4-30: (a) a link, http(s) only, new tab with `common.newTab`; (b) the link text as plain text | S4-15b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-37 | „Alle {{count}} Beispiele anzeigen“: three examples (a) per group („So ist es gut“ / „So ist es nicht gut“), each with its own button and count, notes after the list always visible; (b) three in total | S4-15b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-38 | Tab labels: `criteria.item.detail.tabs` holds the description „Software-Test / App-Test / PDF-Test“: (a) three keys with that wording (UX copy v19 U101); (b) the full names `testType.*` | S4-15b | (a); **decided (a)**, owner 2026-11-11; the wording of U101 confirmed unchanged by the owner in pull request #272 (2026-11-11) |
| O-S4-39 | „#todo#“ in the Software-Test requirement of 11.1.3.2 (2026.1): (a) shown verbatim, listed in the gap list (`kriterienkatalog_2026_1_luecken_v2.md` Z-7); (b) removed from the folder | S4-15b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-40 | S4-16: R-LST-03 makes the weight of B „editable 1–10“, but K5 and the UX copy had no control or label for it: (a) a `mat-select` „Gewichtung“ next to K5 while B applies, new key `criteria.priority.weightLabel` (UX copy v20 U102); (b) the weights as a second radio group in the K5 menu; (c) display only, editing later | S4-16 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-41 | S4-16: a change of only the weight of a recommended B showed no override and no reset: (a) show it with `criteria.priority.overridden.weight` (U103) and „Empfehlung wiederherstellen“; (b) leave it | S4-16 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-42 | S4-16: „Notiz hinzufügen“ had no UX copy key: (a) `criteria.item.note.add` / `.edit` (U104) as a disclosure with the focus into the field, as K6; (b) the note field always visible; (c) the field in the detail panel | S4-16 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-43 | S4-16: when the note's length error shows, as S11 has no submit for it: (a) once the field was left (no `maxlength`, text kept), the export dialog checks again; (b) live while typing; (c) only in S13 | S4-16 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-44 | S4-17: the priority filter of S11 (UX copy has only the label „Priorität“, the Figma filter row has none): (a) a fieldset „Priorität“ with one checkbox per level, none checked = all; (b) a `mat-select` with several choices; (c) a single choice with „Alle“ (new key) | S4-17 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-45 | S4-17: „Filter zurücksetzen“ was the second sentence of `criteria.list.emptyFilter`: (a) split it into the sentence and the button key `criteria.list.emptyFilter.reset` (UX copy v21 U105, same wording); (b) the whole text without a button | S4-17 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-46 | S4-17 (S4-14 P3): search and filter when S11 shows the list of a changed selection: (a) cleared for a changed selection, kept for the same; (b) always kept; (c) always cleared | S4-17 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-47 | S4-17: „Auswahl ändern“ of `criteria.list.noCriteriaFor` leads to S10 step 2 (UX copy), but S10 always opened at step 1: (a) S10 opens at step 2 from the router state, never from the URL; (b) the same link as the title row (step 1) | S4-17 | (a); **decided (a)**, owner 2026-11-11, with the move of the S11 summary (`criteria.list.summary.*`, `criteria.list.counts`), which no task row named, to S4-18 |
| O-S4-48 | S4-18: SPEC names no trigger for `CriteriaListShown` (once per selection shown, no client-side tracking): (a) `POST /api/v1/configurator/criteria-list-shown` (204, no body) in the Configurator module, called by the client once per new selection, `signedIn` from the request's sign-in; (b) the criteria request writes it when S11 marks it (Catalog would depend on Platform; the browser cache of a pinned release hides repeats); (c) wait for step 3 | S4-18b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-49 | S4-18: „Liste speichern“ leads an anonymous user to S01 with `auth.login.reasonSave`, but S01 and saving come with step 3: (a) leave the button and `criteria.list.unsaved` out until step 3; (b) link the legacy login (a dead end: nothing can be saved after it); (c) show it `aria-disabled` (no text for the reason) | S4-18a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-50 | S4-18: `criteria.list.counts` holds three counts in one key without singulars („1 Mindestanforderungen“): (a) one plural key per level with `.one`, the frame keeps only the commas (UX copy v22 U106); (b) keep the key; (c) leave the counts out | S4-18a | (a); **decided (a)**, owner 2026-11-11, wording of U106 approved as proposed |
| O-S4-51 | S4-18: what the counts count: (a) the requirement criteria of the answer by the level that applies now, without „Geprüft nach …“ (Handoff S32), so the levels add up to `criteria.list.summary.count`; inclusion only in the live counter of the action bar; (b) only included criteria | S4-18a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-52 | S4-18 is more than one session: (a) S4-18a frontend (summary, notices, action bar, suggestion link, page titles), S4-18b the event; (b) one task | S4-18 | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-53 | S4-18: the UX copy puts `criteria.list.draftNote` „oberhalb der Aktionen“, Handoff §5.1 and the Figma frame `2020:70` at the top below the priority note: (a) at the top (Handoff over UX copy for layout); UX copy v22 says „oberhalb der Liste“; (b) directly above the action bar | S4-18a | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-54 | S4-18a review: until step 3 `criteria.list.draftNote` asks anonymous users to sign in „um sie zu speichern“, but signing in saves nothing yet: (a) keep the approved wording until step 3; (b) a temporary text in the UX copy | S4-18a | (a); **decided (a)**, owner 2026-11-11 (pull request #275) |
| O-S4-55 | S4-18b: SPEC §9.1 counts `CriteriaListShown` „once per selection“ but not a return to an earlier one: (a) count when the selection shown differs from the last one counted (A → B → A three times; back to S10 and to S11 without a change none); (b) each distinct selection once per session | S4-18b | (a); **decided (a)**, owner 2026-11-11 |
| O-S4-56 | S4-18b: the anonymous `POST /api/v1/configurator/criteria-list-shown` has no rate limit, so a script could inflate the count and fill the bounded event queue, which drops other events too: (a) none, a known limit of the metric; (b) a SPEC §7.3 row (client IP, 30 per 10 minutes 🔧) now, built with the limiter of S4-21; (c) build a limiter in S4-18b | S4-18b, S4-21 | (b); **decided (b)**, owner 2026-11-11 |
| O-S4-57 | S4-19: which criteria the export request lists (ARCHITECTURE §7.2 says nothing about a criterion it leaves out): (a) every criterion of the selection exactly once; a missing, repeated or foreign one is a 400 `validation`; (b) a missing one counts as excluded; (c) a missing one counts as included with the recommendation | S4-19 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-58 | S4-19: the weight range 1–10 (🔧 SD23) existed only as the client setting `priorityWeight`, but the server is authoritative for limits: (a) a server setting `Configurator:PriorityWeight` (Min 1, Max 10) in `appsettings.json`, kept in step with the client by hand; a weight only while B applies; (b) only a positive whole number until step 3 | S4-19 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-59 | S4-19: how EXP-05 proves that no note reaches a log (TESTING named `Serilog.Sinks.InMemory`, not a dependency): (a) a hand-written test logger provider at every level; (b) the package as a test dependency | S4-19 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-60 | S4-19: SPEC §8.6 maps `release-mismatch` to `criteria.export.outdated` („inzwischen gilt eine neue Katalogversion“), against exporting any stored release: (a) only the active release; a stored, inactive one is 400 `release-mismatch`, an unknown one 404; (b) any stored, complete release | S4-19 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-61 | #280 (Figma review 2026-11-12): the clear button of the S10 combobox in Figma `2019:407` / S10-M1 had no UX copy key: (a) build it, wording proposed by Claude; (b) remove it from Figma | S4-30 | (a); **decided (a)**, owner 2026-11-12 („come up with the text yourself“); wording „Eingabe löschen“ (U108) |
| O-S4-62 | S4-20: SPEC §8.3 names the columns, but the UX copy had no keys for the texts of the files („Info“ labels, source line, sheet names, headers; CLAUDE §6.5): (a) Claude drafts UX copy v24 `exportFile.*`, the owner confirms the wording in the S4-20a pull request; (b) the owner gives the wording | S4-20a | (a); **decided (a)**, owner 2026-11-12 (UX copy v24 U109) |
| O-S4-63 | S4-20: where the server keeps those texts: (a) an embedded `de.json` in the Export module, checked by `check-i18n.mjs` as a second target; (b) C# constants with a .NET test against the UX copy | S4-20a | (a); **decided (a)**, owner 2026-11-12 (ARCHITECTURE_v38 C124) |
| O-S4-64 | S4-20: the tool URL on the sheet „Info“ (no public URL decided): (a) a 🔧 server setting `Export:ToolUrl` with `https://procureaccess.de`; (b) a placeholder checked before a release, like O-6 | S4-20b | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-65 | S4-20: what „Priorität (Ihre Wahl)“ holds: (a) the level that applies (the user's choice, else the recommendation); (b) only an override | S4-20a | (a); **decided (a)**, owner 2026-11-12 (SPEC v2.11) |
| O-S4-66 | S4-20: how the data range of „Kriterien“ is marked: (a) an Excel table named `Kriterien` without a colour style; (b) a plain defined name as SPEC §8.3 wrote | S4-20b | (a); **decided (a)**, owner 2026-11-12 (SPEC v2.11); the tester's screen-reader check of an exported file confirms it |
| O-S4-67 | S4-20: CSV formula injection: (a) a leading apostrophe on a text cell starting with `=`, `+`, `-`, `@`, tab or CR (CSV only); (b) write unchanged | S4-20a | (a); **decided (a)**, owner 2026-11-12 (SPEC v2.11) |
| O-S4-68 | S4-20 is more than one session: (a) S4-20a texts and CSV, S4-20b XLSX; (b) one task | S4-20 | (a); **decided (a)**, owner 2026-11-12; ClosedXML 0.105.1 approved (licence check in the S4-20a plan) |
| O-S4-69 | S4-21: the API never read `X-Forwarded-For`, so every request had the nginx container's IP and a limit „per client IP“ would be one limit for everyone: (a) forwarded-headers middleware that trusts only private and loopback networks, with a 🔧 number of proxy hops (default 2: Traefik → nginx → API); (b) nginx sets the client IP (`real_ip`), the API trusts nginx only; (c) trust `X-Forwarded-For` as sent | S4-21a | (a); **decided (a)**, owner 2026-11-12 (ARCHITECTURE_v40 C126) |
| O-S4-70 | S4-21: `/health/ready` (#112) had no SPEC §7.3 row; monitors and healthchecks share few IPs: (a) at most 2 checks at a time over all callers, no queue, 429, timeout 🔧; (b) per IP, e.g. 60 per minute; (c) cache the result a few seconds | S4-21a | (a); **decided (a)**, owner 2026-11-12 (SPEC v2.12 §7.3) |
| O-S4-71 | S4-21: `errors.rateLimit` has no singular („1 Minuten“; SD25): (a) UX copy v25 U110 `errors.rateLimit.one` „… Bitte warten Sie 1 Minute …“; (b) the server reports at least 2 minutes | S4-21b | (a); **decided (a)**, wording confirmed by the owner 2026-11-12 |
| O-S4-72 | S4-21: `criteria.export.outdated` asks to reload the criteria, but the draft lives in memory only and no key names a reload action: (a) S4-21b shows the text with „Schließen“ only, the reload flow is a follow-up issue; (b) build the reload flow in S4-21 with a new key | S4-21b | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-73 | S4-21 is more than one session: (a) S4-21a API, S4-21b client; (b) one task | S4-21 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-74 | S4-21b: S13 shows every failure itself, but the error interceptor shows the 5xx page and snackbars, and cannot read the problem type of a blob error body: (a) the interceptor reads Problem Details from a blob body, and a new request marker turns off snackbar and 5xx page for that request (offline notice stays); (b) mark the export `BACKGROUND_REQUEST`; (c) `fetch` instead of `HttpClient` | S4-21b | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-75 | S4-22, bundle budget (#42): every route was loaded at start (1.94 MB initial): (a) load the feature pages with their route (`loadComponent`; start, error and legal pages stay eager), then set the budget to the measured size plus a margin; (b) only set the budget to today's size plus a margin, lazy loading as a follow-up; (c) split S4-22 into J1 + M2 exit and lazy loading + budget | S4-22 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-76 | S4-22: J1 opens the downloaded XLSX, but e2e/ has no reader for it: (a) XLSX with a small reader on Node's `zlib`, no dependency; (b) an XLSX package as e2e dev dependency; (c) J1 exports CSV | S4-22 | (a); **decided (a)**, owner 2026-11-12, also checking the rows J1 changed (override, excluded criterion, note) |
| O-S4-77 | S4-22 (S4-07 P4): SPEC §11 step 2 lacks A11Y-02, which TESTING §9 lists: (a) add it in the S4-22 pull request (SPEC v2.13); (b) leave it for the owner's next SPEC revision | S4-22 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-81 | S4-25: the legacy role `Admin` also carries `objects:approve`: (a) rename it to `PlatformAdmin` in the same migration, with `moderation:review` until the proposals go; (b) keep `Admin`, change only its permission | S4-25 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-82 | S4-25: role rows exist only after the Development rebuild: (a) the migration makes sure `PlatformAdmin`, `CatalogEditor` and `Moderator` exist everywhere; (b) only `PlatformAdmin` and `Moderator`, `CatalogEditor` with its first endpoint | S4-25 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-83 | S4-25: the legacy column `Proposal.ApproverId`: (a) leave it until the proposals go (step 5); (b) rename it to `ModeratorId` | S4-25 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-84 | S4-25: TEN-06 (step 3): (a) `active` for the rename and the legacy approve endpoint, the endpoints of `CatalogEditor` and `PlatformAdmin` deferred; (b) stays `planned` | S4-25 | (a); **decided (a)**, owner 2026-11-12 (TESTING_v54 V169) |
| O-S4-85 | S4-25: `AuthApproverAttribute` answers 500 for a user without the permission: (a) a claim policy `ModerationReview` (403, anonymous 401); (b) rename only, the 500 as a follow-up issue | S4-25 | (a); **decided (a)**, owner 2026-11-12 |
| O-S4-86 | S4-25 (the task asks before the JWT or the legacy account pages change): the token carries the new role and permission names from its next refresh, and the profile page and three other client checks change `objects:approve` to `moderation:review`: (a) yes; (b) stop and discuss | S4-25 | (a); **decided (a)**, owner 2026-11-12 |
| — | S4-19: TEN-04 asked for `max-age=3600` on every catalog answer, ARCHITECTURE §9.6 (S3-19) for `no-cache` on the active release: reword TEN-04 to §9.6 | S4-19 | Reword; **decided**, owner 2026-11-12 (TESTING_v48 V145) |
| — | Dependabot: majors in #230 (jsdom 31) and #231 (xunit.v3 4) | S4-06 | Decide per the review |
| — | Priorities, rationales, titles of 2026.1 (O-1, K-1, K-6) and the criteria-suggestion channel (O-6) — project team | M2.3; O-6 before G-P | — |

---

## Risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | No active release outside the tests | S10/S11 cannot be shown on staging; a careless switch removes the working legacy filter | O-S4-3: switch by release availability; test release only in development and E2E |
| R2 | The UX copy proposals are not approved in time | S11 waits | S4-02 on D1; S10, the export backend and S13 do not need them; S11 texts that exist in UX copy v16 can be built first |
| R3 | First screen sprint: accessibility details (stepper names, `aria-disabled`, status regions, K5 menu) take longer than estimated | S10/S11 grow | `vitest-axe` and role-based component tests from the first component; split a task when its plan shows more than one session |
| R4 | The E2E flake (#203) keeps `main` red | Merges are judged on noise | S4-01 first |
| R5 | Many invariant flips in parallel branches | TESTING version conflicts | One at a time where possible; otherwise the later PR renumbers |
| R6 | Velocity spread (8 vs. 16 points per day) | Over- or under-commitment | 70 % load; mid-sprint check; record days used at the close-out |
| R7 | The export legal note is a ⚖️ placeholder until the legal review (O-4) | Wording changes later | Implement as written; ⚖️ texts are never changed by Claude Code |
| R8 | The legacy filter and the new screens coexist for a while | Two paths to maintain, confusing for testers | The switch in one place (S4-12); the legacy filter goes with the activation of 2026.1 |
| R9 | `vitest-axe` or the Figma link check need new components in mapping and icon list | Gate red on first use | Add mapping entries and icons in the same PR (TECH_DEBT_v3 §9.2 "each rebuilt screen") |

---

## Definition of done (per task)

- [ ] Code merged into `main` through a pull request; self-review with the code-review checklist (CLAUDE §6.7) done, findings sorted per CLAUDE §6.8.
- [ ] CI pipeline green (required checks on `main`, incl. CodeQL).
- [ ] Tests for invariants carry `INV:<ID>` and the `Invariant` trait (TESTING §8.1); invariants flipped to `active` in the next TESTING version, deferred parts recorded there.
- [ ] UI strings only by key (`node scripts/check-i18n.mjs --write` for areas in `AREAS`); new screens and components carry `@figma` tags, their icons are in the icon list, their pages join `reflow.spec.ts`.
- [ ] `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` and `reuse lint` green.
- [ ] New files carry SPDX headers; no secrets committed; licences of new dependencies checked (CLAUDE §10) and approved by the owner.
- [ ] Documentation touched by the change updated (`CLAUDE.md` §3/§4/§5, ARCHITECTURE, SPEC §13, Figma mapping); register items closed by the PR named in it; legacy files a screen replaces removed from `LEGACY_EXCEPTIONS` (#158).
- [ ] Protocol `docs/sprints/sprint_04/S4-nn_<topic>.md` with starting point, changes, self-review, checks and open points, as in the earlier sprints.

---

## Rhythm (relative, no dates)

| When | What |
|---|---|
| D1 | Sprint start; S4-01 first; UX copy proposals (S4-02) into review; owner decisions O-S4-1 – O-S4-3, O-S4-5 |
| End of D4 | Mid-sprint check against the checkpoint above |
| D8 | Sprint review: J1 end to end on S10 → S11 → S13 in both appearances; success criteria checked |
| D8 | Retro (short) and process step 14: `TECH_DEBT_v4.md`; close-out below |

## Close-out (fill in at the end of the sprint)

| Item | Value |
|---|---|
| Points done (P0 / stretch) | |
| Development days used | |
| Velocity for forecasting | |
| Carried over to sprint 05 (with reason) | |
| Issues created / closed | |
| Changes needed in roadmap or documents | |

**Next sprint (preview):** M3 — cookie session replacing the JWT (M3.1), registration and e-mail confirmation (M3.2), organisations (M3.3), saved lists (M3.5); release 2026.1 activated and the legacy filter removed as soon as O-1/K-1 arrive.

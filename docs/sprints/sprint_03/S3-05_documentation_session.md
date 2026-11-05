# S3-05 — Documentation session: sprint-02 findings and the reflow invariant

> **Sprint:** 03, P0 task S3-05 (documentation, 1 point). **Issues:** #180 (`TECH_DEBT_v2.md` TD-14), #188 (TD-23). **Requirements / invariants:** R-SHL-10 (reflow), R-SHL-06 (footer targets, open questions only); **A11Y-09** new and `active` in `TESTING_v26.md` (V87). **Status:** done by Claude Code (2026-11-05), pull request open.
> **Scope:** the points of #180 that the sprint row names (ARCHITECTURE v16, SPEC, TESTING status line, the headers of Handoff, SPEC, TESTING and the Figma mapping) plus the reflow invariant of #188. ROADMAP_v3 and the SPRINT_02 close-out were already done in the control chat and are not touched. No rule or requirement changes beyond the listed points.
> **Owner decisions** (2026-11-05, before the start):
> 1. **O-S3-3 (A):** the reflow check gets an invariant ID, A11Y-09, tagged on `e2e/shell/reflow.spec.ts`.
> 2. **D12 wording (A):** the hosting-platform sentence of S3-03 P3 goes into ARCHITECTURE v16 in this PR. The task prompt did not list it, but the S3-03 protocol had sent it to this task.
> 3. **SPEC version (A):** the changes are recorded as **v2.3** inside `SPEC_v2.md`, like v2.1 and v2.2. The prompt and #180 said "SPEC v3", but SPEC_v2 "Versioning of this document" reserves SPEC_v3 for the P1 version (start of step 7).
> 4. **No objection** to updating the ARCHITECTURE header to the current versions, or to recording the open footer targets as O-23 and O-24 in ARCHITECTURE §16.5 (the single list of open points) and citing them in SPEC §13.
> 5. CodeQL alert 3 (still open on `main` after S3-04) is a follow-up after this task, not part of it.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Highest versions at the start (`main` at `50ba5a7`): ARCHITECTURE v15, SPEC v2.2 (`SPEC_v2.md`), TESTING v25, UX copy v15, Handoff 1.14, Figma mapping 1.14 | New files `ARCHITECTURE_v16.md`, `TESTING_v26.md`; in-file versions for SPEC (2.3), Handoff and mapping (1.15) |
| F2 | SPEC_v2 reserves "SPEC_v3" for the P1 version; v2.1 and v2.2 were recorded in the same file at the owner's request | Owner decision 3 |
| F3 | `reflow.spec.ts` has no `INV:` tag; A11Y-01 – A11Y-08 are used, A11Y-09 is free | Tag on the `describe` title; the gate counts a tag in a `describe` that is not skipped |
| F4 | „Kriterium vorschlagen“ is already an open point (ARCH O-6, SPEC §13 O-6, UX copy O4); „Über ProcureAccess“ and „Projekt WERTE.IT“ are not | O-6 gets the R-SHL-06 reference; two new points O-23, O-24 |
| F5 | ARCHITECTURE §7.1 already lists 13 problem types including `email-unconfirmed`; only SPEC §8.6 lacks it | SPEC edit only; ARCHITECTURE changes §4.3 "as built" (C96) |
| F6 | C93 says "`/api/Health` is removed with #122"; S2-19 removed it | C97 records it as done; C93 stays as history |
| F7 | The TESTING status line starts with „v15 —“ in every version since v15; the TESTING header names ARCHITECTURE_v12, figma mapping 1.14 and, in the language note, UX copy v13 | V88 |
| F8 | The headers of the UX copy, PRD, criteria catalogue and ROADMAP_v3 also name old versions | Not in the prompt's list (ROADMAP explicitly excluded; UX copy, PRD and catalogue are German documents of the owner / project team): P1 |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `docs/ARCHITECTURE_v16.md` (new, copy of v15) | Status line v16; header names UX copy v15, Handoff 1.15, mapping 1.15, SPEC v2.3, TESTING v26. "Changes from v15": **C95** interim rule for O-22 (§4.3 paragraph "Contracts and projects", §16.5 O-22 row); **C96** Platform as built includes Problem Details (§4.3 table); **C97** `/api/Health` removed; **C98** hosting-platform services in D12 (§6 D12 row, new §16.4.18); **C99** open footer targets: O-6 names R-SHL-06, new O-23 „Über ProcureAccess“ and O-24 „Projekt WERTE.IT“ in §16.5 | #180; decisions 2, 4 |
| `docs/SPEC_v2.md` | Status v2.3; header line "Current versions of the related documents (v2.3)"; "Changes in v2.3" (v2.3-1, v2.3-2); §8.6 adds `email-unconfirmed`; §13 O-6 names the footer link, new rows O-23, O-24 | #180; decision 3 |
| `docs/TESTING_v26.md` (new, copy of v25) | Status line starts with "v26 —", v26 clause added; input documents and language note name the current versions (Handoff added to the list); V87, V88; §4.11 row **A11Y-09** (E, step 1, `active`); §9 step 1 lists A11Y-09; §7.2 mobile-first checks name A11Y-09 as the automated part | #188 (decision 1), #180 |
| `e2e/shell/reflow.spec.ts` | `describe` title `INV:A11Y-09 Reflow at 320 px / 400 % zoom`; doc comment names A11Y-09 and that a rebuilt screen joins the list in its step. No test logic changed | Decision 1 |
| `docs/design_handoff_procureaccess_v1.md` | v1.15: sources name UX copy v15 (noting that the keys were last checked against v14), ARCHITECTURE v16, TESTING v26, and newly SPEC v2.3 and mapping 1.15; language line UX copy v15 | #180 (S2-16 P6) |
| `docs/figma_code_mapping_procureaccess_v1.md` | v1.15: "Related" names Handoff 1.15, UX copy v15, SPEC v2.3, ARCHITECTURE v16, TESTING v26 | #180 (S2-22 P5) |
| `CLAUDE.md` | Version line; §3 reflow paragraph: A11Y-09 instead of "no invariant ID"; §10 licence rule: services of the hosting platform (C98) | DoD |

## 3. Self-review (CLAUDE §6.7)

Checklist: security, correctness, N+1, edge cases, a11y. For this documentation PR it means mostly: does every new statement match the code and the source protocols, and does the PR stay in scope? N+1: not applicable. Sorted per CLAUDE §6.8.

| # | Finding | Sorting |
|---|---|---|
| R1 | C96 first said Platform answers "every error" as Problem Details without the exception of the legacy actions, which still send `application/json` because of `[Produces]` (TESTING_v18 V79, CLAUDE §3) | Fixed: C96 names the exception |
| R2 | The gate makes sure that a test tagged `INV:A11Y-09` runs. It cannot tell whether a rebuilt screen was added to the list. O-S3-3's reason ("the gate enforces it") holds for the test itself; that a new screen joins the list stays a rule in A11Y-09 and §9 that review must check | Won't fix: a check of "every rebuilt screen" would need a list of rebuilt screens the gate could read; P3 |
| R3 | The Handoff and mapping headers now name ARCHITECTURE v16 and TESTING v26, but their content was not checked against those versions (only the header was in scope). The Handoff source line says so for the UX copy keys | Won't fix (scope); P2 |
| R4 | Security: no secrets, no code paths changed; the spec change is a title and a comment | – |
| R5 | Order in §4.11: A11Y-09 comes after A11Y-07, which already comes after A11Y-08 | Won't fix: the gate reads rows, not their order; renumbering would touch unrelated rows |
| R6 | Local E2E ran against the running dev stack (`docker-compose.yml`, client image of 2026-11-04), not a fresh E2E stack: the owner's dev stack was left running. The change is the test title only; CI runs the E2E stack on all five projects | Accepted; §4 check 2 and 7 |

No correctness or security finding that affects behaviour today; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | `node scripts/check-invariants.mjs` | ✅ passed, reads `docs/TESTING_v26.md`, 16 active invariants, A11Y-09 found in 1 test. With the tag removed from `reflow.spec.ts` the gate fails ("A11Y-09 E 0 MISSING"); tag restored |
| 2 | `npx playwright test shell/reflow.spec.ts --project=chromium --project=firefox --project=mobile-chrome` | ✅ 27/27 (against the running dev stack, R6). WebKit and mobile Safari only in CI (host lacks WebKit's libraries) |
| 3 | `npx tsc --noEmit -p .` in `e2e/` | ✅ |
| 4 | `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` | ✅ unchanged (2 checked, 24 not built, 0 pending; 0 errors) |
| 5 | Self-tests `check-invariants` / `check-figma-links` / `check-i18n` | ✅ 36/36, 10/10, 11/11 |
| 6 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 689/689 (`docs/**` covered by `REUSE.toml`) |
| 7 | CI on the pull request | see PR |
| 8 | Build, .NET tests, Vitest, ESLint | – no code of these touched |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The sprint row's "every current document names the current versions of the others" is met only for the documents of the prompt (ARCHITECTURE, SPEC, TESTING, Handoff, mapping). The headers of `ux_copy_procureaccess_v15.md` (ARCHITECTURE v12, SPEC v2, TESTING v10), `prd_procureaccess_werteit_v5.md` (ARCHITECTURE v10, SPEC v1, UX copy v12), `kriterienkatalog_bit_inklusiv_v1.md` (ARCHITECTURE v10, TESTING v8, Handoff 1.12, UX copy v12) and `ROADMAP_v3.md` (ARCHITECTURE v15, TESTING v23) still name older versions | Owner: UX copy with the UX copy session #181; PRD / catalogue / ROADMAP with their next version |
| P2 | References to old versions in the body text, not in the headers: Handoff §0 precedence and the page-title row of §7 name "UX copy v14" (also §5.3: "the strings in UX copy v14"); the mapping names `ARCHITECTURE_v10` (§1, §2.2 store slices) and `TESTING_v8` (§3 `@invariants`); the SPEC "Combines" / "Also used" lines name the versions SPEC v1 was built from (history, left as is) | Next content version of each document |
| P3 | A11Y-09 says every rebuilt screen joins the reflow check; the gate cannot see whether one is missing (R2). Each migration step that rebuilds a screen should name its reflow test in the task prompt | Sprint planning of each step from step 2 on |
| P4 | SPEC R-SHL-10's acceptance criterion still says "☐ manual check per TESTING §7.2". Naming A11Y-09 there would change an acceptance criterion, which this task must not do | Owner, next SPEC version |
| P5 | TD-14 and TD-23 (#180, #188) are closed by this PR; the register records it in its next version | Sprint 03 close-out (TECH_DEBT_v3) |
| P6 | CodeQL alert 3 is still open on `main` after S3-04 (`js/prototype-pollution-utility`, `setText` in `scripts/check-i18n.mjs`); the analysis does not count the guard, which sits before the write loop, as a fix (S3-04 R2) | Follow-up task after S3-05 (owner, 2026-11-05) |
| P7 | Found while pushing: Dependabot alert #12 (`minimatch`, medium, denial of service through glob patterns, `CLIENT/CLIENT.ProcureAccess/package-lock.json`, since 2026-11-04) is open next to the four AutoMapper alerts (#31). It is newer than `TECH_DEBT_v2.md` | Owner: next Dependabot review or a proposed issue |

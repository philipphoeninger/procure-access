<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S6-28 — Legal pages from Markdown with the placeholder check

> **Sprint:** 06, task S6-28 (ROADMAP GP.3, TD-29 legal-page part). **Issues:** none. **Requirements / invariants:** R-SHL-07; A11Y-01, A11Y-09 (both already `active`, unchanged; no invariant flipped).
> **Status:** done by Claude Code (2026-11-18) — pull request open, not merged; E2E left to CI (another session used the local E2E stack). No EF migration, no new dependency.
> **Scope:** exactly the sprint row: Impressum, Datenschutzerklärung and Erklärung zur Barrierefreiheit rendered from Markdown files under the footer links (headings as real headings, page title and H1 focus like every page); development shows the placeholder; the release check `check-release-config.mjs` fails while a placeholder is left, with a CI test of that failure; the pages stay in `reflow.spec.ts`.
> **Owner decisions:** O-S6-6 (a), 2026-11-18 — three Markdown files in the client (`CLIENT/CLIENT.ProcureAccess/content/legal/*.de.md`) with a placeholder marker that fails the production build, not served by the API; the content follows from WERTE.IT before G-P. The owner's task instructions of the same day: no new Markdown dependency without OK, `ngx-markdown` is legacy and leaves with the legacy filter, propose the smallest option; the marker is not fixed by the decision and is chosen and justified here (§2.1). The run could not ask the owner; the plan below was written first and followed without a stop.

## Plan (written before the code)

- **Starting point:** `main` at f5f303e. `pages/legal/legal-page` (S2-15) shows only H1 + `legal.placeholder`, eager in `app.routes.ts`; `check-release-config.mjs` checks only the O-6 channel and says „GP.3 adds the legal pages here“.
- **Smallest option for Markdown, no new dependency:** reuse the project's own Markdown subset parser of the catalog texts (`shared/components/catalog-text/catalog-markdown.ts` and `pa-catalog-text`, S4-15b) and add only a heading/comment splitter for the legal pages. The files get into the bundle at build time through esbuild's built-in `text` loader (an `angular.json` option of the existing `@angular/build`), so no HTTP request, no loading state, no build script. Rejected: `marked`/`ngx-markdown` (legacy, owner), a Node build-time converter to HTML (a second parser, and `innerHTML`), fetching the files from `public/` (loading and error states for three static texts).
- **Files:** `content/legal/{impressum,datenschutz,barrierefreiheit}.de.md`; `angular.json` (loader); `src/markdown.d.ts`; `pages/legal/legal-markdown.ts`, `legal-texts.ts`, `legal-page.{ts,html,scss}` and specs; `app.routes.ts` (legal routes lazy, so the texts do not grow the first download); `scripts/check-release-config.mjs` + self-test; `REUSE.toml`; docs: ARCHITECTURE v81 (C167), TESTING v89 (V302 – V304), Handoff 1.34 (S71 as built), CODEBASE §2.86 and §3, CLAUDE.md §4 target layout line, this protocol.
- **Invariants:** none activated; A11Y-01 (axe on S71 in `shell.spec.ts`) and A11Y-09 (`reflow.spec.ts`) keep covering the pages — the E2E texts do not change while the files are placeholders.
- **Tests:** Vitest for the parser and the page (fixtures with headings, lists, links; the repository's files); `node:test` for the release check (each rule, the whole release on a temporary root, the script on the repository). E2E in CI only.
- **EF migration:** none. **Figma:** S71 has no frame (Figma mapping, UX copy inventory ✗) — Handoff §5.3 applies.
- **One session:** yes.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | SPEC R-SHL-07: content from versioned Markdown files, „rendered as accessible HTML with real headings“; missing content shows a visible placeholder outside production and fails the production build | Parser with headings; placeholder notice; release check |
| F2 | SPEC v2.5-2 / ARCHITECTURE C110 (owner, S3-21 1a): „production build fails“ means the release check `scripts/check-release-config.mjs`, run before a production release, not the image build (which also builds CI, staging and E2E) | The legal pages join that check; the owner's instructions say the same („the production build (check-release-config.mjs)“) |
| F3 | O-S6-6 names the files at `CLIENT/CLIENT.ProcureAccess/content/legal/*.de.md` and says „rendered with the existing `ngx-markdown`“; the owner's task instructions of the same day say no new Markdown dependency and that `ngx-markdown` is legacy and leaves with the legacy filter | Not a contradiction that needs a stop: the later, explicit instruction asks for the smallest option and lists „an existing dependency“ as allowed; the project's own catalog parser needs no dependency at all and does not tie the legal pages to `ngx-markdown` |
| F4 | The project already has a Markdown subset renderer built from Angular nodes (`catalog-markdown.ts`, `pa-catalog-text`: paragraphs, nested lists, bold, http(s) links); headings are deliberately shown as written there | Reuse it for the text between headings; headings are split off before it |
| F5 | UX copy v39 §5.19 has the H1 keys and `legal.placeholder` „Platzhalter: Der Inhalt dieser Seite fehlt noch.“ (Hinweis, nur außerhalb der Produktion); §5.16 the page titles | No new key needed |
| F6 | Handoff §5.3 S71: content column, H1 `legal.*.title`, Markdown with real headings and lists; §1.3: headline/small for H2 section headings | Heading styles from §1.3 |
| F7 | The legal routes were eager (S4-22 kept them in the first download) | With the texts bundled they move to `loadComponent` |
| F8 | E2E: `shell.spec.ts` (footer → each page, `legal.placeholder` visible, axe, tags A11Y-01/A11Y-08), `reflow.spec.ts` (all three pages), `route-change.spec.ts` (starts on `/barrierefreiheit`) | Unchanged assertions stay valid while the files are placeholders |

## 2. Changes

### 2.1 The placeholder marker

The marker is the Markdown/HTML comment **`<!-- PLATZHALTER -->`** (matched in any spacing and case), and each file is that single line until WERTE.IT delivers the text. Why:

1. **Invisible wherever the file is rendered** — a comment is hidden by every Markdown renderer (also the GitHub preview) and by the page's own parser, so the marker can never leak as text; the visible placeholder is the UX copy notice `legal.placeholder`, not wording invented in the file.
2. **Unambiguous and fail-safe** — a fixed token inside a comment never occurs in a real legal text („Platzhalter“ as a word in the text is not the marker; tested). Forgetting to delete it fails the release instead of passing it.
3. **Understandable for the people who insert the text** — the German word matches the notice they see in development; the rule is „replace the line with the text“.
4. **One rule on both sides** — the client (`LEGAL_PLACEHOLDER` in `legal-markdown.ts`) and the release check use the same pattern; additionally a file without content outside comments counts as a placeholder on both sides, so an emptied file cannot pass either. Each side tests against the repository's files, so a change on one side alone shows.

### 2.2 Files

| File | Change |
|---|---|
| `CLIENT/CLIENT.ProcureAccess/content/legal/{impressum,datenschutz,barrierefreiheit}.de.md` | New, each `<!-- PLATZHALTER -->` |
| `CLIENT/CLIENT.ProcureAccess/angular.json` | `build.options.loader: { ".md": "text" }` (the unit-test build inherits the build target's options) |
| `src/markdown.d.ts` | New: types a `*.md` import as a string |
| `src/app/pages/legal/legal-markdown.ts` (+ spec) | New: `parseLegalText` — drops comments, splits at `## ` – `#### ` lines (H2–H4; `# ` stays text, the H1 is the UX copy title), keeps the text between them for `pa-catalog-text`; `placeholder` = marker or no content |
| `src/app/pages/legal/legal-texts.ts` | New: imports the three files into `LEGAL_TEXTS` (root token; tests provide their own); `LegalPageKind` moved here |
| `src/app/pages/legal/legal-page.{ts,html,scss}` (+ spec) | H1 as before; `legal.placeholder` only while the file is a placeholder; then H2/H3/H4 (headline/small, title/medium, title/small, hyphenated) and `pa-catalog-text` blocks |
| `src/app/app.routes.ts` | The three legal routes use `loadComponent` (the texts go into the pages' lazy chunk; checked: the marker text is in a lazy chunk, not in `main`); the comment on the first download updated |
| `scripts/check-release-config.mjs` (+ self-test) | `LEGAL_PAGES`, `checkLegalPage(source)` (missing, marker, no content), `checkRelease(root)`; `main` reports one `::error file=…` line per problem. Self-test: 7 new tests (see §4) |
| `REUSE.toml` | The legal files are covered there, so they hold only the text |
| `docs/ARCHITECTURE_v81.md` | C167, §9.7 sentence; v79 deleted |
| `docs/TESTING_v89.md` | V302 – V304, §3.4 layout comment, §8.2 stage 1 text; v87 deleted |
| `docs/design_handoff_procureaccess_v1.md` | 1.34: „S71 as built“ row in §5.3 |
| `docs/CODEBASE_v1.md` | §2.86 (with how to insert a text), §2.8 and §2.24 pointers, §3 layout |
| `CLAUDE.md` | §4 target layout: the legal files' real path (a structure line; no rule changed) |

Not changed: SPEC (R-SHL-07 is met as written; its path `content/legal/*.de.md` is now the client's folder per O-S6-6, see P3), UX copy, `de.json`, Figma mapping (S71 has no frame), `LEGACY_EXCEPTIONS`, the E2E specs, TECH_DEBT (new version only at sprint end, P2).

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | The release check still runs only before a production release; nothing runs it automatically (TD-29) | **Won't fix here** | By design (C110, owner S3-21 1a): the image build also serves CI, staging and E2E, which must build with placeholders. The release pipeline is TD-29's other part (sprint 07, P2) |
| R2 | `pa-catalog-text` links only http(s); an Impressum usually names an e-mail address | **Follow-up** | Not needed for placeholders; a plain address is legally sufficient. If WERTE.IT wants a `mailto:` link, the parser needs a scheme option (P1) |
| R3 | The marker pattern is duplicated in the client and the script | **Won't fix** | The script is Node without dependencies and cannot import the client's TypeScript; both carry a cross-reference comment and a test against the repository's files |
| R4 | The notice shows in every environment, also staging (a production build) | **Won't fix** | UX copy: „nur außerhalb der Produktion“ — the release check guarantees production never has a placeholder; staging is outside production. No environment switch needed |
| R5 | Markdown inside a heading line (`## **x**`) is shown as written | **Won't fix** | Headings carry no emphasis in legal texts; documented in CODEBASE §2.86 |
| R6 | Legal routes now lazy: offline, a not yet loaded legal page cannot open | **Won't fix** | Like every feature page; the chunk-reload flow of S6-27 applies to it too |
| R7 | Security | **Checked** | No `innerHTML`; text comes only from repository files at build time; links limited to http(s) with `rel="noopener noreferrer"`; no new request, no secret |
| R8 | Accessibility | **Checked** | One H1 (UX copy), file headings start at H2 (a `# ` line is refused by a unit test on the repository's files); notice before the content in DOM order; long headings hyphenate at 320 px; no colour or size below body/medium |

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `npx ng test` (Vitest, whole client) | 85 files, 835 tests pass (14 in `pages/legal/`) |
| C2 | `npm run lint` (client) | All files pass |
| C3 | `npm run build:release` | Builds; initial 756.0 kB (budget 850 kB warning); the legal texts in a lazy chunk of 3.3 kB |
| C4 | `node --test scripts/check-release-config.test.mjs` | 13 tests pass (7 new: content passes with comments; the marker in any spacing/case and next to content; missing, empty, only comments; „Platzhalter“ as a word; ready root passes; each page as placeholder or missing fails naming its file; the script on the repository exits 1 with an `::error` line per placeholder page) |
| C5 | `node scripts/check-release-config.mjs` on the repository | Exit 1: the O-6 channel and the three legal pages |
| C6 | `node scripts/check-invariants.mjs`, `check-i18n.mjs`, `check-figma-links.mjs`, `check-icons.mjs` and their self-tests | Green (TESTING_v89 is the gate's file) |
| C7 | `npm run typecheck`, `npm run lint` in `e2e/` (covers `scripts/*.mjs`) | Green |
| C8 | `reuse lint` (local `fsfe/reuse` image, on a `git archive` of the branch so ignored folders are not scanned) | Compliant, 1592/1592 files. First run found two test fixtures with the text „SPDX-License-Identifier: MIT“ inside a string, read by REUSE as a broken expression — changed to neutral comment text |
| C9 | E2E (`shell.spec.ts`, `reflow.spec.ts`, `route-change.spec.ts`) | **Not run locally** (another session used the E2E stack); CI runs them — see the pull request |
| C10 | Figma comparison | Not applicable: S71 has no frame |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | The texts themselves: WERTE.IT delivers Impressum, Datenschutzerklärung and Erklärung zur Barrierefreiheit (ROADMAP D4). Inserting them: replace the marker line (CODEBASE §2.86). If a text needs `mailto:` links, tables or images, the parser needs a small extension (R2) — check the delivered text first. A page with real headings should then get its own axe assertion in `shell.spec.ts` | Before G-P |
| P2 | TECH_DEBT TD-29: the legal-page part of the check is done; the release pipeline that runs `check-release-config.mjs` is still missing | TECH_DEBT_v6 at sprint end (S6-09); pipeline sprint 07 |
| P3 | SPEC R-SHL-07 and SD17 say `content/legal/*.de.md` without a folder; O-S6-6 placed them in the client. The acceptance criteria are met as written; a SPEC change row could name the folder | Next SPEC change (owner's choice) |
| P4 | Parallel task S6-27 may also write ARCHITECTURE v81 / TESTING v89 or CODEBASE §2.86: the later merge renumbers (C167, V302 – V304, §2.86) | On rebase |

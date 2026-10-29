# S2-04 — Review of the Dependabot pull requests #129 – #131

> **Sprint:** 02, P0 task S2-04 (debt, 0.5 points). **Pull requests:** #129 (e2e group), #130 (client `other` group, closed by Dependabot and replaced by **#138**), #131 (`marked` major). **Requirements / invariants:** none activated; CLAUDE §10 and ARCHITECTURE_v14 D12 (licence rule); A11Y-01 and CAT-10 must stay green. **Status:** review done by Claude Code (2026-10-30); this protocol in its own pull request; **none of the Dependabot PRs merged or closed** (task instruction).
> **Scope:** for each PR: the licence of every new version, the changelog for breaking changes, the CI result, and a recommendation (merge or close, with the reason). Not in scope: changing a Dependabot branch, `dependabot.yml` or client code (→ §5).
> **Owner decisions:** none were given for this task. Decisions needed now: **D-1** (#129, `@types/node`) and **D-2** (#138, toolbar glyph), see §1.4.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | Dependabot opened #129 – #131 on 2026-10-29 at 11:39 UTC, before S2-03 (#136) was merged. S2-03 removed `@fortawesome/fontawesome-free`. #130 then had a merge conflict, and at 23:01 UTC Dependabot closed it ("updatable in another way") and opened **#138** for the same group without Font Awesome | #138 is reviewed in place of #130 (§1.2) |
| F2 | At the same time Dependabot rebased #131 onto the new `main` and raised the target from 18.1.1 to **18.1.2** (title changed). The CI run of the new head failed in E2E on Chromium, on a tooltip unrelated to `marked`; a re-run of the failed job was green | CI in §4 check 1; flake R7 |
| F3 | The sprint row says "merge"; the task instruction says "do not merge any of the Dependabot PRs" | Deviation: the protocol only recommends; the owner merges or closes |
| F4 | None of the new versions is a new package with a new licence, apart from the 20 platform packages of TypeScript 7 (`@typescript/typescript-<os>-<cpu>`, Apache-2.0, Microsoft). Every licence was read from the PR's `package-lock.json` and, for EasyMDE and `marked`, from the published package | Licence check per PR in §1.1 – §1.3 |

### 1.1 #129 — e2e group (`/e2e`): `@types/node` 24.19.2 → 26.6.5, `typescript` 6.0.3 → 7.0.2

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `typescript` 7.0.2 (+ 20 optional platform binaries, one installed per machine) | Apache-2.0 (all 21 packages); OSI-approved, permissive | No, development tool only (`npm run typecheck`, CI stage 1). Playwright transpiles the specs itself and does not load `typescript` | 7.0 is the native (Go) compiler. Changed defaults (`strict`, `module esnext`, `types: []` …) and removed options (`target es5`, `baseUrl`, `moduleResolution node10/classic`, `downlevelIteration`, AMD/UMD/System modules); no programmatic API until 7.1; `tsserver` binary gone. `e2e/tsconfig.json` sets `strict`, `module ESNext`, `moduleResolution bundler`, `target ES2022` and `types: ["node"]` explicitly and uses none of the removed options. No install script (lockfile has no `hasInstallScript`), `engines.node >=16.20` |
| `@types/node` 26.6.5 (+ `undici-types` 7.24.6 → 8.9.0) | MIT, MIT | No (types only) | Type definitions of **Node 26**, while `e2e/.nvmrc` and CI run **Node 24** (24.22.1). The type check would then accept Node-26-only APIs that fail at run time on Node 24. `@types/node` follows the Node major, and the Node version is updated by hand on purpose (`dependabot.yml` ignores `node` for Docker, CLAUDE §3) |

**CI:** all stages green (stage 1 incl. `typecheck` with TypeScript 7, E2E + a11y on all five browser projects); CodeQL neutral.

**Recommendation:** **Do not merge as it is.** Recommended (option B of D-1): keep `@types/node` on the Node major and take TypeScript 7.
- **A — merge as it is:** works today (CI green). Cost: the types are a major ahead of the runtime until Node 26 is adopted.
- **B — recommended:** comment `@dependabot ignore @types/node major version` on #129. Dependabot then rebuilds the group PR with TypeScript only; merge it after green CI. Follow-up: put the same rule into `dependabot.yml` (proposed issue P1), so it is visible to a successor and not only stored by Dependabot.
- **C — close #129 completely** and keep TypeScript 6 in `e2e/` like the client. This avoids two TypeScript majors in the repository (the client stays on 6.0 because Angular 22 sets it). Weaker reason: in `e2e/` TypeScript is only the type check, and its tsconfig sets every changed default explicitly.

### 1.2 #130 → #138 — client `other` group (`/CLIENT/CLIENT.ProcureAccess`)

#130 (closed, F1) had `@fortawesome/fontawesome-free` 7.2.0 → 7.3.1, `easymde` 2.20.0 → 2.21.0, `prettier` 3.6.2 → 3.9.10. Font Awesome is no longer a dependency (S2-03, #78), so #130 needs no action. Its successor **#138** contains:

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `easymde` 2.20.0 → 2.21.0 | MIT (package and `LICENSE`, checked in the published 2.21.0 tarball) | **Yes** (browser bundle; editor of the legacy criterion proposal, `criterion-proposal.ts` → `app-markdown-editor`) | Minor release (2026-05-03): adds a **check-list toolbar button** and fixes a list spacing bug. The new button is `default: true` with the class `fa fa-check-square-o`. Our editor sets no `toolbar` option, so it gets every default button, and **the new one too** (checked in the EasyMDE 2.21.0 source: the default toolbar is built from the `default: true` entries; 2.20.0 has no `check-list`). `styles/main.scss` maps only the twelve classes of 2.20.0 to Material Symbols glyphs (S2-03; its review R5 named exactly this case). Result: a **toolbar button without a visible icon** (its `aria-label` "Check List" stays). Dependencies unchanged (`marked ^4.1.0` of its own, `codemirror ^5`) |
| `prettier` 3.6.2 → 3.9.10 | MIT | No (editor formatting only; no npm script and no CI stage runs Prettier) | Minor releases (3.7 – 3.9: parser upgrades, Angular 21.1 support); formatting output may differ, which affects only files someone formats by hand |
| `typescript-eslint` 8.72.0 → 8.72.1 (and its ten `@typescript-eslint/*` packages) | MIT | No (CI stage 1 lint) | Patch |

**CI:** see §4, check 2.

**Recommendation:** **Merge only together with the glyph fix** (D-2).
- **A — recommended:** a one-line fix PR adds `'fa-check-square-o': 'checklist'` to `$easymde-toolbar-icons` in `src/styles/main.scss` (the `checklist` glyph is in `material-symbols` 0.48.2, `index.d.ts`). Merge it right after #138, or rebase it on #138 and merge both in one go (proposed issue P2).
- **B — merge as it is:** the legacy proposal editor gets one empty button until the fix comes. No test catches it: no component test or E2E journey opens the proposal editor.
- **C — set an explicit `toolbar` list** in `markdown-editor.ts`, so new EasyMDE defaults never show up silently. More than the defect needs (CLAUDE §6.8), and the editor goes away with the legacy proposal (ARCHITECTURE §8.1). Not recommended.

### 1.3 #131 — `marked` 17.0.6 → 18.1.2 (`/CLIENT/CLIENT.ProcureAccess`)

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `marked` 18.1.2 | MIT (lockfile; unchanged since 17) | **Yes** (browser bundle, through `ngx-markdown`) | **18.0.0** (2026-04-07) has two breaking changes: "trim trailing blank lines from block tokens" (changes the `raw` text of block tokens, so custom renderers or extensions that read `token.raw` see fewer newlines) and "update TypeScript to v6" (the type definitions need TypeScript ≥ 6; the client is on 6.0.3). 18.0.1 – 18.1.2 are bug fixes for CommonMark/GFM conformance and several fixes against quadratic or cubic run time (`reflinkSearch`, link destinations), plus a new lexer field `linkParenPossible` (18.1.0) |

Use in the client: no source file imports `marked`. Its only user is `ngx-markdown` 22.1.1 (`provideMarkdown()` in `app.config.ts` without options, `<markdown [data]="criterion.description">` in `criteria-list.html`). `ngx-markdown` 22.1.1 declares the peer range `marked ^17.0.0 || ^18.0.0`, so 18 is supported by the version we use. There is no custom renderer, extension or `walkTokens` hook, so the `raw` trimming has no consumer here. Sanitising stays with `ngx-markdown` (Angular `SecurityContext.HTML`, the default of `provideMarkdown()`) and is not touched by the bump. EasyMDE keeps its own nested `marked` 4.3.0 (`node_modules/easymde/node_modules/marked`); #131 does not change it.

Test coverage: J1 opens one criterion, so `marked` 18 renders one seed description, and axe checks the open panel (A11Y-01). J1 does not compare the rendered HTML, so this is a smoke check.

**CI:** see §4, check 1.

**Recommendation:** **Merge** (CI on the 18.1.2 head green on the second attempt; the first failed on an unrelated J1 flake, R7). Both breaking changes miss this code base, the peer range covers 18, and 18.x brings run-time fixes for untrusted Markdown input. Revert is one commit (sprint risk R8); the legal pages (GP.3) that will depend on `marked` are not built yet.

### 1.4 Summary

| PR | Recommendation | Reason in one line | Decision |
|---|---|---|---|
| #129 | Rebuild without `@types/node` (`@dependabot ignore @types/node major version`), then merge TypeScript 7 | Types should match the Node 24 runtime; TypeScript 7 fits the e2e tsconfig | **D-1** (A / B / C, §1.1) |
| #130 | No action (closed by Dependabot) | Superseded by #138 after S2-03 removed Font Awesome | – |
| #138 | Merge together with the one-line glyph fix | EasyMDE 2.21 adds a default toolbar button that our glyph map lacks | **D-2** (A / B / C, §1.2) |
| #131 | Merge | Breaking changes of `marked` 18 do not touch this code; `ngx-markdown` supports 18 | – |

Merge order: #131 and #138 both change `CLIENT/…/package-lock.json`. After the first merge Dependabot rebases the other (as long as nobody pushed to its branch); wait for its CI before merging. With D-2 = A, a commit pushed onto the #138 branch stops Dependabot from rebasing it, so the fix is better its own PR.

## 2. Changes

| File | Change |
|---|---|
| `docs/sprints/sprint_02/S2-04_dependabot_prs.md` (new) | This protocol |

No Dependabot branch, no `dependabot.yml`, no code changed. The only action on GitHub besides this PR: the failed E2E job of #131 was re-run once (§4, check 1). No document besides this protocol is affected: CLAUDE.md §3 names the client's TypeScript 6.0, which stays; if #129 is merged with TypeScript 7, CLAUDE.md does not name the e2e TypeScript version, so nothing goes stale.

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8. The review subject is three dependency PRs, not code of this PR; the findings are about them.

| # | Finding | Sorting |
|---|---|---|
| R1 | #138: EasyMDE 2.21.0 adds a default toolbar button without a glyph (§1.2). Visible defect in a live (legacy) screen, no a11y name loss | **Fix** before or with the merge of #138 (D-2, proposed issue P2); not in this PR (scope: protocol only) |
| R2 | #129: `@types/node` 26 against the Node 24 runtime (§1.1) | **Follow-up** (D-1; proposed issue P1 for the `dependabot.yml` rule) |
| R3 | Two TypeScript majors in the repository if #129 is merged (client 6.0, e2e 7.0) | Won't fix: e2e uses TypeScript only for the type check; the client follows Angular. Option C of D-1 if the owner prefers one major |
| R4 | No test would catch a broken proposal editor (R1 went through green CI) or wrong Markdown output (J1 only renders and runs axe) | Won't fix now: the proposal editor is legacy (ARCHITECTURE §8.1); a Markdown rendering test belongs to the legal pages (GP.3), where `marked` starts to matter |
| R5 | Security: `marked` output is still sanitised by `ngx-markdown` (Angular `DomSanitizer` with `SecurityContext.HTML` unless configured otherwise; `provideMarkdown()` has no options and no template sets `disableSanitizer`); no changed package declares an install script (`hasInstallScript` absent in all three lockfile diffs; `e2e/` has no `allowScripts` list, the client's is unchanged) | – |
| R6 | Licences: every new version is MIT or Apache-2.0, OSI-approved, permissive; shipped ones (`easymde`, `marked`) meet D12 | – |
| R7 | J1 is flaky on Chromium: axe can catch the Material tooltip of the "Produkte anzeigen" button (`PRODUCTS.SHOW_TOOLTIP`, `products-container.html`) while it fades in and reports `color-contrast` (serious) on `.mat-mdc-tooltip-surface`. The button appears with the criteria list, so it can end up under the pointer of the preceding "Enter" click. CI runs with `retries: 0`, so a flake fails the PR (§4, check 1) | **Follow-up issue** (proposed P6); not caused by any of the Dependabot PRs |

No correctness or security bug in this PR (a document); R1 and R2 are owner decisions on the Dependabot PRs.

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | CI of #131 (head 18.1.2, run 40970227685) | Attempt 1: ❌ E2E + a11y, 1 of 75 tests: J1 main test on **Chromium only**, axe `color-contrast` on the tooltip `.mat-mdc-tooltip-surface` „Produkte anzeigen, die den Filterkriterien entsprechen“ in state "criteria list" (R7); the same test passed on Firefox, WebKit and both mobile projects; every other stage green. Attempt 2 (re-run of the failed job by Claude Code, same head): ✅ all stages green. The earlier head (18.1.1, run 40867093486) was green too. → Flake, not caused by `marked` (the Markdown sits in collapsed panels and changes neither the tooltip nor the layout around it) |
| 2 | CI of #138 (run 40970255793) | ✅ all stages green, incl. E2E + a11y on all five browser projects; CodeQL skipped. Green CI does not cover R1: no test opens the proposal editor |
| 3 | CI of #129 | ✅ all stages green (run of 2026-10-29), CodeQL neutral |
| 4 | Licences from the PR lockfiles (`license` field of every changed package) | ✅ MIT / Apache-2.0 only (§1.1 – §1.3) |
| 5 | Release notes: `marked` v18.0.0 – v18.1.2 (GitHub releases), TypeScript 7.0 announcement, EasyMDE `CHANGELOG.md`, Prettier 3.7 – 3.9 | ✅ read; findings in §1 |
| 6 | EasyMDE 2.21.0 toolbar: source of the published tarball (`npm pack`) against 2.20.0 in `node_modules` | ✅ `check-list` is new and `default: true`, class `fa fa-check-square-o`. Running the package in jsdom to list the rendered buttons was refused by the sandbox (downloaded code); the source reading is unambiguous |
| 7 | `node scripts/check-invariants.mjs` | ✅ passed (no catalogue change) |
| 8 | `node scripts/check-figma-links.mjs` | ✅ unchanged (0 checked, 12 not built, 1 pending) |
| 9 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant (`docs/**` is covered by `REUSE.toml`, so the protocol carries no header, like the other protocols) |
| 10 | Build and test levels | – not touched (document only); each Dependabot PR has its own CI run |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | Proposed issue: `dependabot.yml` — ignore `@types/node` majors in `/e2e` (and in `/CLIENT/CLIENT.ProcureAccess`, should `@types/node` become a direct dependency there), with the comment that it follows the Node version in `.nvmrc` | Owner OK for the issue; any CI/config PR (S2-20 fits) |
| P2 | Proposed issue: EasyMDE check-list glyph (`'fa-check-square-o': 'checklist'` in `styles/main.scss`) — needed for #138 (D-2) | Owner; before #138 is merged |
| P3 | D-1 (#129) and D-2 (#138) | Owner |
| P4 | Merge or close #129, #131, #138 per §1.4; the sprint row's "Done when" (merged or closed with a reason) is met only then | Owner |
| P5 | Dependabot rebased #131 and rebuilt #130 overnight. If another rebase happens before the merge, the CI result in §4 is for an older head | Owner: check the head commit at merge time |
| P6 | Proposed issue: J1 tooltip flake (R7). Minimal fix in the test: move the pointer away (`page.mouse.move(0, 0)`) before the axe check of "criteria list", or wait until no `.mat-mdc-tooltip-surface` is animating. The legacy tooltip itself goes away with the configurator rebuild (step 2); its content is not lost meanwhile, the button has its own visible text | Owner OK for the issue; S2-20 (CI one-liners) or the next J1 change |

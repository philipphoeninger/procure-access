# S3-01 — Review of the Dependabot pull requests #164, #165, #166, #175

> **Sprint:** 03, P0 task S3-01 (debt, 0.5 points). **Pull requests:** #164 (nginx 1.31), #165 (Angular group), #166 (client `other` group), #175 (TypeScript 7 in `e2e/`). **Requirements / invariants:** none activated; CLAUDE §10 and ARCHITECTURE D12 (licence rule); A11Y-01 and CAT-10 must stay green. **Status:** review done by Claude Code (2026-11-05); this protocol and the `dependabot.yml` change in their own pull request; **none of the Dependabot PRs merged or closed** (task instruction).
> **Scope:** for each PR: the licence of every new version, the changelog for breaking changes (Angular: release notes; nginx: compatibility of `CLIENT/CLIENT.ProcureAccess/nginx.conf`), the CI result, and a recommendation (merge or close, with the reason). Applying O-S3-1 to `dependabot.yml`. Not in scope: changing a Dependabot branch, client code or the ruleset (→ §5).
> **Owner decisions:** **O-S3-1** (given with the task): `e2e/` follows the client's TypeScript version. **D-1** (asked during the review, 2026-11-05): the client image stays on the nginx **stable** branch (even minor); #164 is closed, and `dependabot.yml` records the rule for later nginx PRs (§1.1). The first version of the question wrongly said nginx recommends stable for production; the owner confirmed D-1 after the correction (§1.1).

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | All four PRs have green CI on their current head (§4, checks 1–4). #164 was rebased by Dependabot onto S2-21 (`8416d84`); #165, #166 and #175 are based on `530e4ac` and their CI ran on 2026-11-04 20:49 UTC, **before** S2-21 – S2-23 (API Dockerfile, `ci.yml`, `angular.json`, `header.ts`, the scripts) were merged | Their CI result is for an older `main`. The three client PRs were checked together on today's `main` locally (§4, check 5); a `@dependabot rebase` before each merge gives a fresh CI run (§1.5) |
| F2 | nginx numbers its branches by the minor version: even = **stable** (1.30), odd = **mainline** (1.31). #164 is not a security update: 1.30.6 and 1.31.7 (both 2026-10-14) carry the same five CVE fixes, and the floating tag `1.30-alpine` already pulls 1.30.6 | #164 is a choice of branch, not a fix → D-1 |
| F3 | #165 and #166 both change the client's `package.json` and `package-lock.json`. Git merges them without conflict, but they were never tested together in CI | Merge one, rebase the other (§1.5) |
| F4 | Sprint 02 chose TypeScript 7 for `e2e/` (S2-04 D-1 = B), but #129 was never merged; `e2e/` is still on 6.0.3. S2-20 P1 then raised the question again, now answered by O-S3-1 | #175 is closed; the rule in `dependabot.yml` keeps TypeScript minors and majors out of the `e2e` group (§1.4) |
| F5 | The ruleset "Main protection" has `enforcement: disabled` (read with `gh api …/rulesets/26114779`): no required checks, no deletion or force-push protection on `main`. TECH_DEBT_v2 (TD-01, §1) assumes stages 1–7 are required | Out of scope; open point P1. Merges of the PRs below are not held back by a red check |

### 1.1 #164 — nginx `1.30-alpine` → `1.31-alpine` (client `Dockerfile`, final stage)

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `nginx` 1.30.6 → 1.31.8 (Alpine packages `nginx`, `nginx-module-*`) | BSD-2-Clause (apk metadata of both images) | **Yes** (client image) | 1.31.0 – 1.31.8 (May – Oct 2026): no directive removed, no default changed. Behaviour changes: HTTP/2 and HTTP/3 requests with `Connection`, `Upgrade` … headers are rejected (1.31.0); requests to HTTP/1.1 backends always carry `Host` (1.31.4); `$request_id` uses SipHash (1.31.2); relative `working_directory` paths (1.31.8). New: control API, predicate locations, JSON module (1.31.5), MMDB support in the GeoIP modules (1.31.7). None of this touches our `nginx.conf` (HTTP/1.1 on port 80, `gzip`, `try_files`, `add_header`, `log_format no_query`, regex location, `proxy_pass` with a variable and `resolver`; `Host` is set explicitly anyway) |
| `libmaxminddb-libs` 1.13.3 (new in the image) | Apache-2.0 | Yes (image, loaded only by the GeoIP module, which our config does not use) | Pulled in by the MMDB support of 1.31.7 |

Both images are on Alpine 3.24.3; apart from the rows above, their package lists (71 and 72 packages) are identical (`apk info -v`, §4 check 6). `nginx -t` with our `nginx.conf` passes on both (§4 check 6).

**CI:** all stages green on the rebased head (run 41928514818), incl. E2E + a11y on all five browser projects, which run through this nginx.

**Recommendation:** **Close** (D-1 = stay on stable). nginx's documentation recommends mainline for production "unless your organization has strict requirements for stability", and stable for those. The reason here is the floating tag: the Dockerfile names only the minor (`1.30-alpine`), so every image build takes the newest patch of that branch without a PR. On mainline those patches bring new features and behaviour changes (control API, predicate locations and the `Host` change all came in 1.31.x patches); on stable they bring only backported critical and security fixes, which reached both branches on the same day in 2026. Behaviour changes then arrive once a year through a Dependabot PR that runs through CI, which suits a project kept by one maintainer and handed over to a successor. The 1.31 config check above shows that a later move would be easy.
- **Rule for later nginx PRs** (comment in `dependabot.yml`, §2): merge a PR to an even minor (1.32 in spring 2027, 1.34 …), close one to an odd minor. Closing a Dependabot version PR makes it skip that version, and the PR to the next stable minor is the yearly reminder, so 1.30 cannot run out of support unnoticed. A `dependabot.yml` ignore rule was not used: ignoring all minors would also hide the next stable branch.
- **Alternative (not chosen):** merge #164 and follow mainline, nginx's default recommendation (every minor, no rule); features and fixes arrive sooner, behaviour changes come with ordinary rebuilds.

### 1.2 #165 — Angular group: eight `@angular/*` packages 22.2.3 → 22.2.4

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `@angular/animations`, `common`, `compiler`, `core`, `forms`, `platform-browser`, `router` 22.2.4 | MIT | **Yes** (browser bundle) | Patch release, fixes only. Compiler: prototype-named attributes, malformed template literals, escaped characters in comments. Core: i18n/ICU fixes, effect errors in targeted change detection, `resource` no longer reloads on unchanged params. Forms: no crash when `formControlName` has no control. Router: Navigation API fixes, `createUrlTreeFromSnapshot` drops `null` query params, `routerLinkActive` removes old classes. Migrations: optional `ngClass` → `class` schematic (not run by a patch update). No breaking change; nothing of it needs code here |
| `@angular/compiler-cli` 22.2.4 | MIT | No (build) | Requires `@jridgewell/sourcemap-codec` ≥ 1.6.0; the lockfile already has 1.6.0 |

No other package moves (lockfile diff: only the eight). The CLI and Material stay on 22.2.5 (other repositories, no new release in this group). No install script.

**CI:** all stages green (run 41910309647; older `main`, F1).

**Recommendation:** **Merge** after `@dependabot rebase` and a green CI run on today's `main`.

### 1.3 #166 — client `other` group

| Package | Licence | Shipped? | Breaking changes and effect here |
|---|---|---|---|
| `@ngx-translate/core` 18.0.0 → 18.1.0 | MIT | **Yes** | Minor release with many opt-in features (retry policies, several fallback languages, `deleteLanguage`, template slots, interpolation context for the parser). Relevant fix: "don't let a nested key shadow a flat sibling key in `getValue`". The 18.1.0 code (read from the published package) only **adds a fallback**: when the walk through nested keys finds nothing, it tries the whole key as one flat property of the root. Every key that resolved before resolves the same way, so the dotted keys next to a text (#101, `menu` / `menu.close`, SD25 `.one`) are unchanged; `src/app/testing/de-json.spec.ts` checks exactly this with the real library and passes. `scripts/check-i18n.mjs` keeps its stricter copy of the lookup: it can only reject a key the library would accept, never the reverse. Published from GitHub Actions with SLSA provenance (new releaser, Dependabot notes it) |
| `@ngx-translate/http-loader` 18.0.0 → 18.1.0 | MIT | **Yes** | Encodes the language id in the request URL (`de` is unchanged); optional error reporting and resource flattening, both opt-in |
| `tslib` 2.8.1 → 2.9.1 | 0BSD (OSI-approved) | **Yes** (helpers in the bundle) | Changes only the `rewriteRelativeImportExtension` helper, which this code does not use, and the release pipeline (new releaser `microsoft1es`, Microsoft's pipeline account) |
| `eslint` 10.13.0 → 10.14.0 | MIT | No (CI stage 1) | One rule fix counted as a feature (`func-name-matching`), crash fixes, and `minimatch` ≥ 10.2.6 against a vulnerable `brace-expansion` |

No install script.

**CI:** all stages green (run 41910267945; older `main`, F1).

**Recommendation:** **Merge** after #165, with `@dependabot rebase` and a green CI run first (F3).

### 1.4 #175 — `e2e` group: `typescript` 6.0.3 → 7.0.2

Same package set as #129 in sprint 02 (S2-04 §1.1): `typescript` 7.0.2 plus 20 optional platform binaries `@typescript/typescript-<os>-<cpu>`, all Apache-2.0, development only (type check of CI stage 1), no install script. CI green (run 41910203252), so TypeScript 7 would work here.

**Recommendation:** **Close** (O-S3-1: `e2e/` follows the client's TypeScript, which Angular 22 keeps on 6.0). The new ignore rule in `dependabot.yml` (§2) keeps TypeScript minors and majors out of the `e2e` group, as for the client; patches still come. When `ng update` raises the client's TypeScript, the same pull request raises it in `e2e/` (comment in `dependabot.yml`, clause in CLAUDE.md §3). Dependabot may close #175 itself on its next run once the rule is on `main`; otherwise close it by hand with a link to this protocol.

### 1.5 Summary

| PR | Recommendation | Reason in one line | Decision |
|---|---|---|---|
| #164 | Close | Stay on the nginx stable branch; same security fixes, fewer changes | **D-1** |
| #165 | Merge (after `@dependabot rebase` + green CI) | Angular patch release, fixes only | – |
| #166 | Merge after #165 (after `@dependabot rebase` + green CI) | Minor releases, no breaking change; the key lookup change is a fallback only | – |
| #175 | Close (after this PR is merged) | O-S3-1: `e2e/` follows the client's TypeScript 6.0 | **O-S3-1** |

Merge order: #165 first, then comment `@dependabot rebase` on #166 and merge it once its CI is green. A rebase of #165 first is recommended too (F1), since its CI ran before S2-21 – S2-23. Check the head commit at merge time: Dependabot rebases on its own when a PR gets a conflict.

## 2. Changes

| File | Change |
|---|---|
| `.github/dependabot.yml` | `/e2e`: ignore `typescript` minor and major updates, with the comment that it follows the client and moves with `ng update` (O-S3-1). `docker`: comment with the nginx rule of D-1 (no ignore rule) |
| `CLAUDE.md` | §3: `e2e/` uses the client's TypeScript, Dependabot offers only patches there (O-S3-1); the client image's nginx stays on the stable branch (D-1); version line |
| `docs/sprints/sprint_03/S3-01_dependabot_prs.md` (new) | This protocol |

No Dependabot branch and no code changed. Not changed on purpose: `TECH_DEBT_v2.md` §6 still lists the `e2e/` TypeScript question as open; the register is versioned and v3 at the end of the sprint takes it as decided (P4).

## 3. Self-review (CLAUDE §6.7)

Sorted per CLAUDE §6.8. The review subjects are four dependency PRs plus a config comment; the findings are about them.

| # | Finding | Sorting |
|---|---|---|
| R1 | The CI of #165, #166 and #175 ran on the `main` before S2-21 – S2-23 (F1) | **Fix** before the merge: `@dependabot rebase`, then merge on green (§1.5); covered locally for the client part (§4 check 5) |
| R2 | #165 and #166 are not tested together in CI (F3) | **Fix** by the merge order of §1.5 |
| R3 | The production client image gets the nginx October CVE fixes only when it is rebuilt: `1.30-alpine` is a floating tag. CVE-2026-93479 (string expressions with variables, request logging) touches features our config uses (`log_format`, `proxy_pass $api_upstream`) | **Follow-up** P2: check that the deployed client image was built after 2026-10-14 (nginx ≥ 1.30.6) |
| R4 | npm marks `@angular/animations` as deprecated ("use `animate.enter` and `animate.leave`"); the client still calls `provideAnimationsAsync()` in `app.config.ts`. Same message on 22.2.3, so not caused by #165 | **Follow-up issue** #192 (P3) |
| R5 | The nginx rule is a comment, not a rule Dependabot enforces; a maintainer could merge an odd-minor PR by mistake | Won't fix: an ignore rule cannot tell even from odd minors without listing every future mainline version. The rule is written in `dependabot.yml`, next to the Docker updates it governs, and in CLAUDE.md §3; a mistaken merge is one revert commit |
| R6 | Security: no changed npm package declares an install script; ngx-translate 18.1.0 has SLSA provenance; `tslib`'s new releaser is Microsoft's pipeline account; `libmaxminddb-libs` is only loaded with the GeoIP module | – |
| R7 | Licences: every new version is MIT, 0BSD, Apache-2.0 or BSD-2-Clause, OSI-approved and permissive; shipped ones (Angular, ngx-translate, tslib, nginx, libmaxminddb) meet D12 | – |
| R8 | The `main` ruleset is disabled (F5) | **Follow-up** P1, owner (not a repository file) |

The code-review checklist on this PR's own diff (YAML comment and rule, two clauses in CLAUDE.md, this document): no correctness or security finding. The ignore rule copies the client's rule word for word (`dependency-name: typescript`, `update-types` major and minor), which Dependabot has applied to the client since #116 (2026-10-29).

## 4. Checks

| # | Check | Result |
|---|---|---|
| 1 | CI of #164 (head `5c09ce8`, run 41928514818) | ✅ all stages green, incl. E2E + a11y on five browser projects (A11Y-01, CAT-10); CodeQL neutral (#144) |
| 2 | CI of #165 (head `d3b7e74`, run 41910309647) | ✅ all stages green; CodeQL neutral; older `main` (F1) |
| 3 | CI of #166 (head `e6c9ce2`, run 41910267945) | ✅ all stages green; CodeQL neutral; older `main` (F1) |
| 4 | CI of #175 (head `888302e`, run 41910203252) | ✅ all stages green, incl. stage 1 type check of `e2e/` with TypeScript 7; CodeQL neutral |
| 5 | #164 + #165 + #166 merged onto today's `main` (`d9d90b1`) in a scratch worktree; client on Node 24.22.1: `npm ci`, `npm run lint`, `ng test` (single run), `npm run build:release`, `node scripts/check-i18n.mjs` | ✅ merge without conflict; lint clean; Vitest 24 files / 110 tests passed; release build passes with the known critical-CSS selector warning (S2-03 R9); i18n 0 errors. E2E not run locally: the dev stack occupied the same ports, and each PR has a green E2E run in CI |
| 6 | `nginx:1.30-alpine` (1.30.6) and `nginx:1.31-alpine` (1.31.8): `nginx -t` with our `nginx.conf` (`api` mapped to 127.0.0.1), `apk info -v` and licences | ✅ both pass; same Alpine 3.24.3; only difference besides the nginx packages is `libmaxminddb-libs` (Apache-2.0) |
| 7 | Licences from the lockfile diffs (`license` of every changed package) and from the apk database of the images | ✅ §1.1 – §1.4 |
| 8 | Release notes: nginx `CHANGES` (1.31.0 – 1.31.8) and `CHANGES-1.30` (1.30.1 – 1.30.6), Angular 22.2.4, ngx-translate 18.1.0 (and `getValue` in the published 18.0.0 and 18.1.0 packages), tslib 2.9.0/2.9.1, ESLint 10.14.0 | ✅ read; findings in §1 |
| 9 | `dependabot.yml` parses as YAML | ✅ (GitHub validates it on the next Dependabot run, P5) |
| 10 | `node scripts/check-invariants.mjs`, `node scripts/check-figma-links.mjs`, `node scripts/check-i18n.mjs` on this branch | ✅ gate passed; Figma 2 checked, 24 not built, 0 pending; i18n 0 errors |
| 11 | `reuse lint` (image `fsfe/reuse:latest`) | ✅ compliant, 673 / 673 files (`docs/**` is covered by `REUSE.toml`; `dependabot.yml` keeps its header) |
| 12 | Build and test levels | – not touched by this PR (config comment, docs); each Dependabot PR has its own CI run |

## 5. Open points

| # | Point | Owner / when |
|---|---|---|
| P1 | The ruleset "Main protection" is **disabled** (F5): no required checks, no protection against deleting or force-pushing `main`. If it was switched off on purpose (e.g. for the docs PRs #189/#190), switch it back on before merging the PRs of §1.5; S3-03 then adds the CodeQL checks to it (#144) | Owner, now |
| P2 | Check that the deployed client image was built after 2026-10-14, so it runs nginx ≥ 1.30.6 with the October CVE fixes (R3); otherwise rebuild and redeploy | Owner, next deployment |
| P3 | #192: `@angular/animations` is deprecated in Angular 22; Material and CDK 22 no longer depend on it, so remove `provideAnimationsAsync()` and the package (R4). Milestone M1; fits S3-09 (theme, `prefers-reduced-motion`) | Issue created 2026-11-05 (owner) |
| P4 | `TECH_DEBT_v3.md` (end of sprint): move the `e2e/` TypeScript question of §6 to "Done" (O-S3-1), and record the nginx rule (D-1) next to the weekly Dependabot review of §9.6 | End of sprint 03 (process step 14) |
| P5 | After this PR is merged: check that Dependabot accepts the changed `dependabot.yml` (Insights → Dependency graph → Dependabot, no config error) and that #175 is closed (by Dependabot or by hand) | Owner, after the merge |
| P6 | Merge or close #164 – #166, #175 per §1.5; the sprint row's "Done when" is met only then | Owner |

# S1-07 — Frontend pre-check: Node version and third-party packages (Angular 20 → 22)

> **Sprint:** 01, task S1-07 (milestone M0.2). **Status:** done (2026-10-19). **Input for:** S1-08 – S1-11.
> **Scope:** inventory only — no code changed. Versions are the latest on npmjs.org / nodejs.org as of **2026-10-19**; re-check them when S1-08 starts.

## 1. Node, npm and TypeScript

| Item | Current | Angular 21 needs | Angular 22 needs | Target (owner decision) | When |
|---|---|---|---|---|---|
| Node, developer machine | 22.20.0 (`.nvmrc`) | `^20.19 \|\| ^22.12 \|\| >=24` ✅ | `^22.22.3 \|\| ^24.15 \|\| >=26` ❌ | **Node 24 LTS** (24.22.0 today) | Before S1-09; S1-08 runs on 22.20 |
| Node, client `Dockerfile` | `node:22-alpine` (floating tag) | ✅ | ✅ only by chance (tag resolves to 22.23.3 today) | `node:24.<minor>-alpine`, pinned to the same minor as `.nvmrc` | S1-09 (image check in S1-11) |
| npm | 10.9.3 | ✅ | ✅ (`>=8`) | the npm bundled with Node 24 | with Node |
| Global `@angular/cli` in `Dockerfile` | unpinned `npm install -g` | – | – | remove; `npm run` uses the local CLI | S1-11 |
| TypeScript | 5.9.3 | 5.9 | **`>=6.0 <6.1`** | `~6.0.3`; **not** `typescript@latest` (7.0.2 does not satisfy Angular 22) | S1-09 |

**Owner decision (2026-10-19):** target **Node 24 LTS**. Reason: Node 22 leaves maintenance in April 2027, Node 24 is the active LTS line. `.nvmrc` and the `Dockerfile` carry the same exact version.

## 2. Third-party packages

Usage = files in `src/` (plus `angular.json`) that import or reference the package. Licences from the package metadata on npmjs.org. ✅ = meets CLAUDE §10.

### 2.1 Angular-dependent packages

| Package | Installed | Usage | Angular 22 support | Licence | Note |
|---|---|---|---|---|---|
| `@angular/*` (core, common, forms, router, compiler, platform-browser, build, cli, compiler-cli) | 20.3.x | app | 22.2.3 | MIT ✅ | `ng update` per major (S1-08, S1-09) |
| `@angular/material`, `@angular/cdk` | 20.2.14 | app | ✅ 22.2.3 | MIT ✅ | Updated together with core |
| `@ngrx/signals` | 20.0.1 | 13 files (store slices) | ✅ 22.0.1 (21.1.1 for Angular 21) | MIT ✅ | |
| `@ngx-translate/core`, `@ngx-translate/http-loader` | 17.0.0 | 26 files / `app.config.ts` | ✅ v17 peer `>=16`, so it already works on 22; **18.0.0** is available | MIT ✅ | S1-09 says "update ngx-translate": 17 → 18 is a major step; read the v18 migration notes then. If they touch more than `app.config.ts`, staying on 17 for M0 is acceptable |
| `ngx-markdown` | 20.1.0 | 2 files | ✅ 22.1.1 (21.x for Angular 21) | MIT ✅ | v21+ needs the peer `marked ^17 \|\| ^18`; installed `marked` is 16.4.2 → add/upgrade `marked` in S1-08 |
| `ngx-color-picker` | 20.1.1 | 1 file (`features/settings/settings.ts`) | ⚠️ no Angular 21/22 release (last release 2025-08); peer `>=19` allows 22, but it is untested there | MIT ✅ | **Owner decision: keep.** Check the settings screen in the S1-09 and S1-10 smoke tests (zoneless!). If it breaks: open an issue, and decide on replacing it then. *Removed on 2026-10-27: not imported (S1-08), owner decision after the review of PR #39 (S1-08 §7).* |
| `@auth0/angular-jwt` | 5.2.0 | 1 file (`auth.service.ts`, `JwtHelperService`) | ✅ peer `@angular/common >=14` | MIT ✅ | Goes away with cookie auth in M3 |
| `@ngrx/store`, `@ngrx/router-store`, `@ngrx/store-devtools` | 20.0.1 | **0** | (22.0.1 exists) | MIT ✅ | **Unused → remove in S1-08** |
| `@angular/animations`, `@angular/platform-browser-dynamic` | 20.3.4 | **0** | – | MIT ✅ | **Unused → remove in S1-08** |

### 2.2 Other packages

| Package | Installed | Usage | Licence | Note |
|---|---|---|---|---|
| `easymde` | 2.20.0 | markdown editor component, styles | MIT ✅ | Not Angular-bound |
| `jwt-decode` | 4.0.0 | `auth.service.ts` | MIT ✅ | Goes away in M3 |
| `@fortawesome/fontawesome-free` | 7.2.0 | `angular.json` (styles) | CC-BY-4.0 AND OFL-1.1 AND MIT ✅ | Self-hosted (CLAUDE §8) |
| `typedjson` + `reflect-metadata` | 1.8.0 / 0.2.2 | 28 files (models, DTOs) | MIT / Apache-2.0 ✅ | ⚠️ `typedjson` has had no release since 2022 and depends on `experimentalDecorators` + `emitDecoratorMetadata` → see risk 2 |
| `rxjs`, `tslib` | 7.8.x / 2.x | app | Apache-2.0 / 0BSD ✅ | Within Angular 22 peer ranges |
| `flatted`, `jsonpath`, `comment-json` | – | **0** | – | **Unused → remove in S1-08** |
| `prettier` | 3.6.x | formatting | MIT ✅ | |
| Karma, Jasmine (`karma*`, `jasmine-core`, `@types/jasmine`) | – | test runner | – | Replaced by Vitest in S1-16, not in S1-08 |

All packages meet the licence rule. No package lacks a path to Angular 22.

## 3. Blockers and risks for S1-08 – S1-11

| # | Severity | Finding | Affects | What to do |
|---|---|---|---|---|
| 1 | **Blocker** | Node 22.20.0 does not satisfy Angular 22 (`^22.22.3 \|\| ^24.15`) | S1-09 | Install Node 24 LTS locally, update `.nvmrc`, pin the `Dockerfile` base image (S1-09). S1-08 (Angular 21) is not blocked |
| 2 | **Blocker** | Angular 22 requires TypeScript 6.0. TS 6 deprecates `baseUrl` (used in `tsconfig.json` together with `paths`) and changes defaults | S1-09 | `ng update` should raise TypeScript; then remove `baseUrl` and make `paths` relative (`./src/app/*`). Use `"ignoreDeprecations": "6.0"` only as a documented stop-gap. Build and smoke-test the screens that deserialize with `typedjson` (decorator metadata) |
| 3 | Medium | `ngx-color-picker` has no release for Angular 21/22 | S1-09, S1-10 | Kept (owner decision); smoke-test the settings screen on 22 and without Zone.js |
| 4 | Low | `ngx-markdown` 21+ needs `marked ^17` | S1-08 | Update `marked` with `ngx-markdown` |
| 5 | Low | `@ngx-translate` 17 → 18 is a major version | S1-09 | Read the migration notes; staying on 17 is possible (peer `>=16`) |
| 6 | Info | `Dockerfile` builds with `--configuration=development` and installs an unpinned global CLI | S1-11 | Fix as part of the release-build task |

## 4. Note on zoneless (for S1-10)

The app already runs zoneless: `app.config.ts` calls `provideZonelessChangeDetection()`, there is no `zone.js` dependency and no `polyfills` entry in `angular.json` (consistent with ARCHITECTURE §8.2). The S1-10 steps "remove `zone.js` from polyfills" and "no `zone.js` in the bundle" are therefore already met. What remains for S1-10 is the per-screen smoke test, fixing places that rely on change detection triggered by async callbacks, and the standalone migration schematic. The sprint plan is not changed here; the point is to be confirmed at S1-10.

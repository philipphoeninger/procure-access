# S4-28 — Dead code: dialog service, unused logger, assets location

> **Sprint:** 04, P2 stretch task S4-28 (0.5 points). **Issues:** none. **Requirements / invariants:** none; `TECH_DEBT_v3.md` TD-32; no invariant activated, no new TESTING version.
> **Status:** done by Claude Code (2026-11-13) — pull request #307 open.
> **Scope:** exactly the sprint row: delete `core/services/dialog.service.ts`, remove the unused `Logger` of `BaseCrudController` and the `location ^~ /assets/` of the client's `nginx.conf`, each after checking that nothing references it. No behaviour change.
> **Owner decisions:** the task prompt named none. **O-S4-94** (asked with the plan, owner 2026-11-13: (a)) — the type parameter `TController` of `BaseCrudController`, which only typed the logger, goes too.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `CLIENT/CLIENT.ProcureAccess/src/app/core/services/dialog.service.ts` is commented out line by line (S3-22 P1) and still holds English snackbar literals. Nothing imports it: no reference in `src/`, `eslint.config.js`, `REUSE.toml`, `scripts/` or `docs/` apart from TD-32 | Delete the file |
| F2 | `BaseCrudController<TEntity, TDto, TController>` stores `IAppLogging<TController> Logger`; no derived controller reads it since the generic writes went (#201, S3-07 R4). Five controllers (`Criteria`, `CriteriaFilters`, `FilterTypes`, `Products`, `Proposals`) only pass `IAppLogging<…> logger` to `base(logger, service)`; no test constructs them, they are reached over HTTP | Remove field and constructor parameter; the derived constructors take only their service |
| F3 | `TController` (with `where TController : class`) types nothing but the logger | O-S4-94 (a): remove it; the five declarations become `BaseCrudController<TEntity, TDto>` |
| F4 | `IAppLogging<T>` stays in use by `UICustomizationController` and `UsersController` (and `UsersControllerTests`) | Its registration in `SERVICES` (`LoggingConfiguration`) stays; ARCHITECTURE §4 still names it with the legacy controllers |
| F5 | The release build copies only `public/` (`angular.json` `assets`: `i18n/`, favicon, logo, `products.csv`); the Material Symbols subset in `src/assets/fonts/` and the Roboto files are bundled into `media/` (S3-09, S3-25). The build output has no `assets/` folder, so `location ^~ /assets/` serves nothing | Remove the block; the comment above `/i18n/` („Translations and assets are files …“) loses „and assets“ |
| F6 | No E2E test, script or document requests a path under `/assets/` | Nothing else to change |

## 2. Changes

| File | Change | Why |
|---|---|---|
| `CLIENT/CLIENT.ProcureAccess/src/app/core/services/dialog.service.ts` | Deleted | F1 |
| `API/API.ProcureAccess/Controllers/Base/BaseCrudController.cs` | `BaseCrudController<TEntity, TDto>`; field `Logger` and the `logger` parameter removed; constructor takes only `IBaseService<TEntity, TDto>` | F2, F3 |
| `API/API.ProcureAccess/Controllers/{Criteria,CriteriaFilters,FilterTypes,Products,Proposals}Controller.cs` | Base without `TController`; constructor without `IAppLogging<…> logger`, `base(service)` | F2, F3 |
| `CLIENT/CLIENT.ProcureAccess/nginx.conf` | `location ^~ /assets/` removed; comment above `/i18n/` reworded | F5 |
| `docs/sprints/sprint_04/SPRINT_04.md` | O-S4-94 | – |
| `docs/sprints/sprint_04/S4-28_dead_code.md` | This protocol | – |

CLAUDE.md (no path, command or convention changes), ARCHITECTURE, SPEC, TESTING, the Figma mapping and `LEGACY_EXCEPTIONS` (the dialog service had no entry) are not touched. TD-32 is resolved by this pull request; `TECH_DEBT` records it in its next version at the sprint close-out (owner, 2026-11-13).

## 3. Self-review (CLAUDE §6.7)

Checked with `/engineering:code-review` on the diff.

| Area | Result |
|---|---|
| Security | `/assets/…` now falls through to `location /` and answers `index.html` with `no-cache`, like every client route; nothing is exposed (no `assets/` folder in the build, `autoindex` off by default). Checked on `nginx:1.30-alpine` with the release build (C6) |
| Correctness | Dependency injection resolves every controller (all backend tests, including the HTTP tests of the legacy controllers and `SwaggerDocumentTests`); `IAppLogging<>` stays registered (F4); routes `api/[controller]` unchanged; no test or architecture rule names the base's type parameters |
| Edge cases, performance, N+1 | Not applicable (removal only) |
| a11y, UI texts | Not touched; `de.json` unchanged |

**Findings sorted (CLAUDE §6.8):**

| # | Finding | Sorting |
|---|---|---|
| R1 | `API/API.ProcureAccess/Controllers/FavoritesController.cs` is commented out as a whole and still names the old base signature (`BaseCrudController<Favorite, FavoritesController>`, `base(logger, repo)`) | Follow-up: P1 (not in the row) |
| R2 | The `immutable` location `-[A-Z0-9]{8}\.(js\|css)$` of `nginx.conf` matches only `main-*.js` and `styles-*.css`: 44 of the 45 lazy chunks carry lowercase letters, `_` or `-` in their hash (`chunk-6xwBF1Or.js`, `chunk--54nLi_Y.js`), fall into `location /` and are revalidated on every load instead of cached for a year | Follow-up: P2 (not in the row; no wrong file is ever served) |

No correctness or security bug left that affects current behaviour; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,NU1902,NU1903,NU1904` | 0 warnings, 0 errors |
| C2 | `dotnet format ProcureAccess.sln --verify-no-changes` | Exit 0 |
| C3 | `dotnet test --solution ProcureAccess.sln` (Unit, Architecture, Integration with Testcontainers) | 912 of 912 pass |
| C4 | Client `npm run lint`, `npx ng test --watch=false` | Lint clean; 55 files, 504 tests pass |
| C5 | Client `npm run build:release` | Built; initial 762.90 kB (unchanged). The warning „Could not parse 1 selector in styles-….css“ of the critical-CSS step comes from the unchanged styles |
| C6 | Release build served by `nginx:1.30-alpine` with the new `nginx.conf` (`nginx -t` successful) | `/assets/x.png` 200 `index.html`; `/i18n/de.json` 200 JSON; `/i18n/missing.json` 404; `/kriterien` 200 `index.html` |
| C7 | `node scripts/check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs`, `check-icons.mjs` | All exit 0 |
| C8 | `reuse lint` | Compliant (1273 / 1273 files) |
| C9 | E2E | Left to CI (no locally run stack; nothing a journey touches changed) |

## 5. Open points

| # | Point | Proposal |
|---|---|---|
| P1 | `FavoritesController.cs` is commented out as a whole (R1), like the dialog service was | Delete it with the favorites rework of step 5 (`withFavorites()` is legacy), or as a touch item in the next TECH_DEBT version |
| P2 | The hash pattern of the `immutable` nginx location misses the lazy chunks (R2); since S4-22 the lazy routes make them most of the bundle | Proposed issue: widen the pattern to the build's hash alphabet (e.g. `-[A-Za-z0-9_-]{8}\.(js\|css)$`) and check the headers of `main`, `styles` and a chunk on the client image |

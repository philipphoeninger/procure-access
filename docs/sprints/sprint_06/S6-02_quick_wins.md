# S6-02 — Quick wins: S16 gate, forced-colours edge, aria-label, health watcher, dead code

> **Sprint:** 06 · **Task:** S6-02 (0.5 points) · **Issues:** #375, #378, #350, #385 (TD-64); TD-71 (no issue)
> **Requirements / invariants:** R-SAV-05, R-LST-10, R-AUTH-05; A11Y-01, A11Y-09 and TEN-09 stay green; no invariant flipped, no new TESTING version
> **Status:** done — pull request open (2026-11-18)
> **Scope:** exactly the row of S6-02; one commit per item. No EF migration, no backend change, no new dependency, no lock-file change (`npm audit fix` belongs to S6-08).
> **Owner decisions:** none

## 0. Plan (written before the code)

- **Starting point:** `main` at bcca089 (contains S6-01).
- **Files:** `shared-list-page.html` and its spec (#375); `criteria-action-bar.scss`, `e2e/configurator/s11-list.spec.ts`, Handoff §2.2 (#378); `products-list.html` (#350); `src/dev/dev-reload-health.ts` and its spec, CLAUDE §3 (TD-64); the files TD-71 lists.
- **Invariants:** none activated; A11Y-01, A11Y-09, TEN-09 stay green. Tests: a Vitest case for S16, one for the watcher, one Chromium E2E check for forced colours (left to CI).
- **Migration:** none. One session.

## 1. Starting point

Each item is described by its issue. Findings checked in the code before the change: S16 gated cards and legend by `counts().total`, which leaves „Geprüft nach …“ criteria out (O-S4-51 (a)); the action bar had no `forced-colors` rule; `products-list.html:97` carried `aria-label="Product proposal button"`; the watcher fetched with the default credentials. The profile page named in #350 no longer exists (removed in S5-28b), so only the product list is affected. TD-71 items were all found as listed (14 `todo.txt`: 10 under `features/feature-1/`, one each in `core/utils`, `layout/sidebar`, `shared/directives`, `shared/pipes`).

## 2. Changes

1. **#375** — `shared-list-page.html`: the list and the legend sit in `@if (shown.criteria.criteria.length > 0)`, the counts paragraph in `@if (counts().total > 0)` inside it, `criteria.list.empty` is the `@else`. Test: a list of „Geprüft nach …“ only shows its card, no counts paragraph, no empty text.
2. **#378** — `border-block-start: 1px solid CanvasText` under `@media (forced-colors: active)` in the sticky query of `criteria-action-bar.scss`. The bar's height is measured from the element, so the border is part of `scroll-padding-bottom`. E2E (Chromium, `emulateMedia({ forcedColors: 'active' })`): top border > 0 while sticky, 0 in the flow and outside forced colours. Handoff 1.33 §2.2 names the edge. The existing 2.4.11 focus test is untouched.
3. **#350** — the `aria-label` is removed; the name is the translated visible text.
4. **TD-64 / #385** — `credentials: 'omit'` in the watcher's `fetch`; a Vitest case with fake timers checks it. The note in CLAUDE §3 that the warning can be checked only on the E2E stack is replaced. TECH_DEBT_v5 row #4 (TECHNICAL.md: "the session warning can be checked only on the E2E stack") is now obsolete; it is struck in the next TECH_DEBT version (sprint-end task).
5. **TD-71 deletions** (only what the register lists):
   - `theme.scss`: `.tertiary`, `.secondary`, `.error`, `.text-error` and their comment (no template or TS file uses them; checked by search);
   - `core/models/appCustomization.model.ts` (no importer);
   - 14 `todo.txt` files and the then empty `features/feature-1/` folders; their `REUSE.toml` entries (the `**/todo.txt` glob and five path lines);
   - the commented-out `localStorage.setItem` line in `criterion-proposal.ts` and in `product-proposal.ts`.
6. Docs: CODEBASE §2.84, Handoff 1.33, CLAUDE §3 (watcher note only).

## 3. Self-review

- **fix:** none open.
- **won't fix:** the rest of the commented-out login block around the two removed lines in the two proposal files is not in TD-71's list (§5).
- No new colour, no px size; the forced-colours rule sits only inside the sticky query, so nothing else changes.

## 4. Checks

- `ng test` for the two touched specs: green; client `npm run lint`: green; e2e `typecheck` and `lint`: green.
- `check-invariants`, `check-figma-links`, `check-i18n`, `check-icons`: green.
- `reuse lint`: not run locally (`reuse` and Docker are not available here); the change removes files and `REUSE.toml` lines and adds only this protocol (under `docs/**`).
- E2E (forced-colours check, axe in both appearances): left to CI.

## 5. Open points

1. The remaining commented-out login blocks in `criterion-proposal.ts` and `product-proposal.ts` are dead code outside TD-71's list; remove them with the rebuild of those screens.
2. The forced-colours check runs in Chromium only; Playwright has no equivalent emulation elsewhere.

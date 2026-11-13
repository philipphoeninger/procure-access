<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S5-01 — Quick wins: S10 step-line checks, nginx immutable pattern

> **Sprint:** 05, task S5-01. **Issues:** #309 (TD-38), #308. **Requirements / invariants:** A11Y-01 stays green; no invariant activated, no new TESTING version.
> **Status:** done by Claude Code (2026-11-13) — pull request open; E2E left to CI (another session ran the local E2E stack).
> **Scope:** exactly the sprint row: scope the S10 step-line E2E checks to `#main`, widen the nginx immutable rule to the build's hash alphabet. No EF migration.
> **Owner decisions:** none.

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | `s10-steps-2-4.spec.ts:115` and `s10-step1.spec.ts:124` use `page.getByText('Schritt n von 4: …')`. `pa-stepper` announces the same text through CDK `LiveAnnouncer` (`#cdk-live-announcer-0`, outside `#main`), so Playwright's strict mode sees two elements while the announcer holds it (Firefox flake, #309) | Scope both to `page.locator('#main')` |
| F2 | Every other `getByText` in `e2e/` matches a text that is never announced (error text under a field, reset notice, legal placeholder); titles are checked with `toHaveTitle` | No further change |
| F3 | `nginx.conf` immutable rule `-[A-Z0-9]{8}\.(js\|css)$` misses lazy chunks such as `chunk-14tk7xqi.js` or `chunk-CpH5Yic-.js` (#308) | Widen to `-[A-Za-z0-9_-]{8}\.(js\|css)$` |

## 2. Changes

1. `e2e/configurator/s10-steps-2-4.spec.ts`, `e2e/configurator/s10-step1.spec.ts`: `page.locator('#main').getByText(...)` for the step line.
2. `CLIENT/CLIENT.ProcureAccess/nginx.conf`: the hash pattern is `-[A-Za-z0-9_-]{8}\.(js|css)$`. Every `.js`/`.css` file of the release build ends in `-<8 characters of that alphabet>`.

One commit per issue.

## 3. Self-review

- Fix: none open. Won't fix: no E2E/smoke check that a lazy chunk is `immutable` (the issue calls it optional; scope is the row), see P1.
- The scoped locator still finds the visible step line (`<p class="pa-stepper__line">` is inside `main#main`) and no longer the announcer.

## 4. Checks

| Check | Result |
|---|---|
| `npm ci`, `npm run typecheck`, `npm run lint` in `e2e/` | green |
| `npm run build:release` in the client | green |
| nginx response headers: `nginx:1.30-alpine` (the Dockerfile's base tag) with the new `nginx.conf` over the release build's `dist/procure-access/browser`, host port 4400, container removed afterwards; the full client image was not built (1.7 GB free) | table below |
| Playwright E2E | left to CI |

Response headers (`curl -I`):

| File | Cache-Control |
|---|---|
| `index.html` | `no-cache` |
| `i18n/de.json` | `no-cache` |
| `main-KZPMFRWQ.js` | `public, max-age=31536000, immutable` |
| `styles-2IKPWQOD.css` | `public, max-age=31536000, immutable` |
| `chunk-14tk7xqi.js` (lazy, lowercase hash) | `public, max-age=31536000, immutable` |
| `chunk-CpH5Yic-.js` (lazy, mixed case and `-`) | `public, max-age=31536000, immutable` |

All 47 `.js`/`.css` files of the build answered `immutable`.

## 5. Open points

- P1: an automated check that a lazy chunk gets `immutable` (smoke test on the E2E stack, the issue's optional third point) is not built; propose a follow-up if wanted.
- P2: the TD-38 and #308 rows of `TECH_DEBT_v4.md` are updated at the end of the sprint (process step 14), not here.

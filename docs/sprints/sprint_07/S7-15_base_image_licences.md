<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# S7-15 — Licence notices for the base images

> **Sprint:** 07, task S7-15. **Issues:** none (TD-44, TD-53). **Requirements / invariants:** none; no invariant activated, no new TESTING version; the API notices check stays green.
> **Status:** done by Claude Code (2026-11-26); pull request open, not merged. **E2E left to CI:** the task changes no screen and no E2E spec, and the local E2E stack was not started. No EF migration, no new dependency, no UI string. Docker images were not built (disk).
> **Scope:** exactly the sprint row: OS packages of `aspnet:10.0`, nginx and PostgreSQL covered by a documented statement per CLAUDE §10, a check that notices a tag change, and the place for the BIT inklusiv permission (TD-53).
> **Owner decisions:** **O-S7-15 (2026-11-26, given in advance)** — the *documented statement*, not a generated package list per tag (it would drift because the tags float). Details (location, check) were left to the task.

## Plan (step 2; written instead of waiting for approval)

- **Starting point:** `check-api-notices.mjs` covers the NuGet packages of the API image; nothing covers the OS layers of `aspnet:10.0`, `nginx:1.30-alpine`, `postgres:17-alpine`. Dockerfile tags: aspnet floats at `10.0`, sdk is pinned (S5-03), Mailpit and Playwright are pinned with digests.
- **Files:** new `docs/BASE_IMAGES.md` (statement), new `scripts/check-base-images.mjs` + `.test.mjs`, CI stage 1 step, CODEBASE, TECHNICAL, CLAUDE command line.
- **Invariants / tests / migration:** none / node:test self-test / none. One session.

## 1. Starting point

- CLAUDE §10 requires shipped dependencies to be OSI-approved without strong copyleft and every image to ship the notices of what it bundles; S4-27 P4 left the base images' OS packages open (TD-44).
- TD-53: the export's source line quotes BIT inklusiv; the permission is not recorded anywhere.

## 2. Changes

| File | Change |
|---|---|
| `docs/BASE_IMAGES.md` | New. Statement: why no package list (floating tags), two tables — shipped (`aspnet:10.0`, `nginx:1.30-alpine`, `postgres:17-alpine`, each with a link to its licence information) and not shipped (sdk, node, Mailpit, Playwright) — and the empty record of the BIT inklusiv permission (TD-53) for the owner to fill |
| `scripts/check-base-images.mjs` | New, dependency-free. Reads every `FROM` and `image:` in `Dockerfile*` and `docker-compose*.y(a)ml` (digests ignored) and fails on: a tag not in the statement (names the statement's tag when only the tag differs), an unused statement entry, an image without a fixed tag, a shipped image without a link, an image in both tables |
| `scripts/check-base-images.test.mjs` | New self-test, incl. a case that runs the check on the repository |
| `.github/workflows/ci.yml` | Stage 1: step "Base image statement" (self-test and check) |
| `docs/CODEBASE_v1.md`, `docs/TECHNICAL.md`, `CLAUDE.md` | Paragraph of the notices (§2), layout and command lines, deployment step 3, command list |

TD-44 is resolved by this PR; TECH_DEBT_v6 is not edited (a new version comes at the end of the sprint, process step 14). TD-53 stays the owner's: only the place exists.

## 3. Self-review (CLAUDE §6.8)

- *Fix:* none found.
- *Won't fix:* the statement links to the images' documentation but does not verify the links (they would make CI depend on the network).
- *Follow-up:* the aspnet tag floats; a digest pin (as for Mailpit) would make rebuilds reproducible, but the check would then see no change; not part of this row.

## 4. Checks

- `node --test scripts/check-base-images.test.mjs` 7/7; `node scripts/check-base-images.mjs` green (7 images).
- Not run: `e2e` typecheck/lint (no `node_modules` in the worktree; the new files follow the style of `check-api-notices.mjs`, CI stage 1 covers them), .NET build and tests (no .NET change), E2E (CI), `reuse lint` in full (worktree false positives): `reuse lint-file` on the new files instead.

## 5. Open points

1. TD-53: the owner enters the permission in the last section of `docs/BASE_IMAGES.md` (or tells us it is not needed).
2. TECH_DEBT: mark TD-44 as done and TD-53 as "place exists" in the next version.
3. Pinning `aspnet`, `nginx` and `postgres` by digest (follow-up, see self-review).

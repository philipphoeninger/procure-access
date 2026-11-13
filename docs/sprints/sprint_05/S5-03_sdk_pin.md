# S5-03 — Pin the .NET SDK

> **Sprint:** 05 · **Task:** S5-03 · **Issues:** #306 (S4-27 P2)
> **Requirements / invariants:** none (CI and build tooling; no invariant changes, no TESTING version)
> **Status:** pull request open (2026-11-13)
> **Scope:** one .NET SDK version for local builds, CI and the API image, so the shipped NuGet package set and the notices check (CLAUDE §3, C132) no longer depend on which SDK a machine happens to have; a CI check that keeps the image and `global.json` together
> **Owner decisions:** **O-S5-1 (2026-11-13): (a)** pin in `global.json`, CI and the image; **4xx band, version `10.0.404`**; the runtime image stays floating; SDK tag only, no digest. Plan stated before the change; fits one session, no EF migration

## 1. Starting point

| # | Finding | Consequence |
|---|---|---|
| F1 | The SDK floated in three places: CI `DOTNET_VERSION: 10.0.x` (four `setup-dotnet` steps), the image `sdk:10.0`, and local machines (`global.json` had only the test runner). This machine had 10.0.114 | Three possible SDKs for one notices file |
| F2 | The newest `10.0.x` SDK is 10.0.404 (also the newest `sdk:10.0.x` tag and the SDK the committed `THIRD-PARTY-NOTICES.txt` was generated with) | Pinning it changes nothing about CI or the image today |
| F3 | A pinned `sdk.version` with `rollForward: latestPatch` does not roll across feature bands, so the local 10.0.114 can no longer build the repository | Local machines need the 4xx band (owner knew, O-S5-1) |
| F4 | `actions/setup-dotnet` with `global-json-file` installs exactly `sdk.version` (it ignores `rollForward`) | CI is exact, local machines may use a newer patch of the band |
| F5 | Dependabot has a `dotnet-sdk` ecosystem for `global.json` and the `docker` ecosystem for the tag, but each opens its own pull request; pull requests are not grouped across ecosystems | One pull request cannot change both; see §2 and R2 |

## 2. Changes

| File | Change | Reason |
|---|---|---|
| `global.json` | `sdk.version: "10.0.404"`, `rollForward: "latestPatch"`; the test runner stays | O-S5-1 (a), F3, F4 |
| `API/API.ProcureAccess/Dockerfile` | Build stage `FROM mcr.microsoft.com/dotnet/sdk:10.0.404`; the runtime stage `aspnet:10.0` unchanged | O-S5-1: tag only, runtime floating |
| `.github/workflows/ci.yml` | `DOTNET_VERSION` removed; the four `setup-dotnet` steps use `global-json-file: global.json`; new stage 1 step „Check the SDK pin“ fails when the Dockerfile tag differs from `sdk.version` (same pattern as the Playwright image check, S4-01b) | Same SDK in CI and image, mismatch caught |
| `.github/dependabot.yml` | Comment on the docker ecosystem: the SDK tag follows `global.json`; a tag pull request fails stage 1 until `global.json` is changed on it | F5, R2 |
| `scripts/check-api-notices.mjs` | Comments and the regenerate hint name the SDK of `global.json` instead of „the latest 10.0“ | Wording only |
| `docs/ARCHITECTURE_v48.md` (v46 deleted) | C134 (decision, where the version lives, check, update procedure, runtime floating); C132 and the header follow | Issue „done when“ |
| `CLAUDE.md` | Header line; §3 SDK-pin paragraph next to the notices; §4 `global.json` line; §5 install hint and the `--write` comment | Issue „done when“ |

Not changed: `THIRD-PARTY-NOTICES.txt` (the set is identical, §4 C3), SPEC, TESTING (no invariant; the stage 1 step is not yet in §8.2, P2), Figma mapping, `LEGACY_EXCEPTIONS`, the client and e2e images, `codeql.yml` (C# without a build, no .NET step).

## 3. Self-review (CLAUDE §6.7)

| # | Finding | Sorting | Reason |
|---|---|---|---|
| R1 | The pin check reads the Dockerfile with a `sed` pattern that needs `FROM mcr.microsoft.com/dotnet/sdk:<tag> AS build` on one line; another form fails with „found: nothing“ | **Won't fix** | Fails loudly with the expected form in the message; checked with a matching, a mismatching and (by the pattern) an absent line |
| R2 | Dependabot cannot update both places in one pull request: the tag pull request is red until someone pushes the `global.json` change onto it (and `--write` of the notices if needed) | **Won't fix** (documented) | Same working pattern as the Playwright image (S4-01b); using the `dotnet-sdk` ecosystem as well would give two red pull requests per SDK release |
| R3 | `rollForward: latestPatch` lets a developer build with a newer patch than CI | **Won't fix** | Owner-approved default; a changed package set shows up in CI stage 2 as before |
| R4 | The check does not look at `rollForward` or at the runtime image | **Won't fix** | Runtime floating is the decision; `rollForward` has no CI effect |

Security: no secret, no network call added; a floating SDK tag is replaced by a fixed one (the SDK stage is not shipped). Licences: no dependency. No correctness or security finding left; no further review round needed.

## 4. Checks

| # | Check | Result |
|---|---|---|
| C1 | SDK 10.0.404 installed to `~/.dotnet` with `dotnet-install.sh`; `dotnet --version` in the repository | 10.0.404 |
| C2 | The pin check commands of the CI step: matching tag; tag changed to 10.0.405; restored | OK / „MISMATCH (found: …10.0.405)“ / OK |
| C3 | `dotnet build ProcureAccess.sln -warnaserror -warnnotaserror:NU1901,…` and `node scripts/check-api-notices.mjs` on 10.0.404 | 0 warnings, 0 errors; notices: 49 packages, 0 errors — **file unchanged** |
| C4 | `docker build` of the API image (now on `sdk:10.0.404`), then `check-api-notices.mjs --deps` with the image's `deps.json` | Image builds; 49 packages, 0 errors |
| C5 | `dotnet test --project tests/ProcureAccess.Tests.Unit`; `node --test scripts/check-api-notices.test.mjs` | 597 passed; 20 passed |
| C6 | `dotnet format ProcureAccess.sln --verify-no-changes`; `check-invariants.mjs`, `check-figma-links.mjs`, `check-i18n.mjs` | All green |
| C7 | `reuse lint` (Docker image) | Green (1299/1299) |
| C8 | CI of the pull request (`global-json-file`, the new step) | See the pull request; the workflow change is only proven there |
| C9 | E2E | Not run locally; untouched |

## 5. Open points

| # | Point | When |
|---|---|---|
| P1 | An SDK update is now a pull request that must change `global.json` and the Dockerfile tag together (and the notices file if stage 2 turns red); Dependabot proposes only the tag | With each SDK release; owner merges |
| P2 | TESTING §8.2 stage 1 does not yet list the „Check the SDK pin“ step (no invariant) | Next TESTING version |
| P3 | `TECHNICAL.md` (#4) is to cover the SDK pin (TECH_DEBT_v4 #4 already lists it); `TECH_DEBT` #306 is marked resolved at the sprint close-out | Sprint 05 close-out, step 16 |
| P4 | Every other developer machine (and any automation on this one) needs SDK 10.0.404 installed; 10.0.114 no longer builds the repository | Now; install line in CLAUDE §5 |

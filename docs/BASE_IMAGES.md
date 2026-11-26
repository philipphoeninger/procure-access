<!--
SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.

SPDX-License-Identifier: MIT
-->

# Base images and licence records

Statement of the container base images that ProcureAccess ships or uses (S7-15, TD-44, CLAUDE §10). The
licence notices of the API image (`API/API.ProcureAccess/THIRD-PARTY-NOTICES.txt`) and of the client image
(`/3rdpartylicenses.txt`) cover the NuGet and npm packages the project bundles, not the operating-system
packages inside the base images. Those packages come with the image and carry the licences their
maintainers publish. This statement names every image with its tag and points to the place where that
image documents its contents and licences; it does not copy a package list, because the tags float
(`10.0`, `1.30-alpine`, `17-alpine` receive security rebuilds) and a copied list would be wrong after the
next rebuild. The owner chose this option on 2026-11-26 (O-S7-15).

`node scripts/check-base-images.mjs` (CI stage 1) reads every `FROM` line of the Dockerfiles and every
`image:` of the compose files and fails when an image tag is not named here, or when an entry here is no
longer used. A tag change therefore needs a change of this file, and with it a look at the new tag's
licence information. Digests (`@sha256:…`) are ignored.

Licence position: the operating-system layers consist of free software under licences such as GPL, LGPL,
MIT, BSD and MPL. They are distributed unmodified as parts of the official images, not linked into the
project's code. Anyone who deploys an image receives them from the image's publisher under those licences.

## Shipped images

| Image | Used for | Where its contents and licences are documented |
|---|---|---|
| `mcr.microsoft.com/dotnet/aspnet:10.0` | Runtime of the API image (`API/API.ProcureAccess/Dockerfile`), Debian-based | https://github.com/dotnet/dotnet-docker (image documentation; inside the image `/usr/share/doc/<package>/copyright`), .NET licence: https://github.com/dotnet/core/blob/main/license-information.md |
| `nginx:1.30-alpine` | Serves the client (`CLIENT/CLIENT.ProcureAccess/Dockerfile`), Alpine-based | https://hub.docker.com/_/nginx (image documentation), nginx licence: https://nginx.org/LICENSE, Alpine package licences: `apk info -L <package>` and https://pkgs.alpinelinux.org |
| `postgres:17-alpine` | The database of `docker-compose.yml` (API and the restore rehearsal), Alpine-based | https://hub.docker.com/_/postgres (image documentation), PostgreSQL licence: https://www.postgresql.org/about/licence/, Alpine package licences as above |

## Not shipped (build, test and development only)

| Image | Used for | Note |
|---|---|---|
| `mcr.microsoft.com/dotnet/sdk:10.0.404` | Build stage of the API image | Pinned to `global.json`; nothing of it reaches the final image |
| `node:24.22.1-alpine` | Build stage of the client image | Nothing of it reaches the final image |
| `axllent/mailpit:v1.33.1` | Development and E2E mail catcher (`docker-compose.dev.yml`, `docker-compose.e2e.yml`) | Development only (MIT) |
| `mcr.microsoft.com/playwright:v1.65.1-noble` | Browser image of the E2E tests (`e2e/playwright-image/Dockerfile`) | Test tool only |

## BIT inklusiv permission (TD-53)

The place where the permission for the source line of the export (it quotes BIT inklusiv, U109) is recorded.
The check of `node scripts/check-base-images.mjs` does not read this section.

| Field | Entry |
|---|---|
| Permission given by | *to be entered by the owner* |
| Date and form (e-mail, contract) | *to be entered by the owner* |
| Scope (source line of the XLSX and CSV export, texts of the catalog) | *to be entered by the owner* |
| Stored where (the document itself stays out of the public repository) | *to be entered by the owner* |

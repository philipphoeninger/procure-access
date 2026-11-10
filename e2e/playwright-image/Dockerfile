# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# The image the E2E jobs of CI stage 6 run Playwright in (S4-01b, #203, O-S4-1 (1)): it brings
# every browser with its system packages, so no job downloads them from the apt mirror.
# The tag must be the version of @playwright/test in e2e/package-lock.json; CI fails otherwise.
# Update tag and digest together with that package (one pull request):
#   docker buildx imagetools inspect mcr.microsoft.com/playwright:v<version>-noble
# CI only reads this line; nothing builds the file.
FROM mcr.microsoft.com/playwright:v1.64.1-noble@sha256:2f05f8a734509a7b3f26a912639488ee08f58a7b292d14d62271bcff9038768f

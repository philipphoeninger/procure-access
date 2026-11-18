# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# The image the E2E jobs of CI stage 6 run Playwright in (S4-01b, #203, O-S4-1 (1)): it brings
# every browser with its system packages, so no job downloads them from the apt mirror.
# The tag must be the version of @playwright/test in e2e/package-lock.json; CI fails otherwise.
# Update tag and digest together with that package (one pull request):
#   docker buildx imagetools inspect mcr.microsoft.com/playwright:v<version>-noble
# The image brings its own Node (26 from Playwright 1.65 on); e2e/.nvmrc stays the Node of npm ci
# and local runs. e2e/ has no native module, so its node_modules run on either.
# CI only reads this line; nothing builds the file.
FROM mcr.microsoft.com/playwright:v1.65.1-noble@sha256:1507cbfc550a63a6b7396791ca4f00df17beb56342d2447b3f106c306f525579

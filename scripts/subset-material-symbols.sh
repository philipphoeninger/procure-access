#!/bin/sh
# SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# Regenerate the Material Symbols subset of the client (#135, S3-25) after a change to the icon
# list or an update of the material-symbols package:
#   scripts/subset-material-symbols.sh
# Needs Docker and the client's node_modules (npm install in CLIENT/CLIENT.ProcureAccess).
# fontTools and brotli (both MIT) are development tools: installed into the throwaway container
# only, never shipped (CLAUDE §10).
set -eu

FONTTOOLS_VERSION=4.68.0
BROTLI_VERSION=1.2.1

ROOT=$(cd "$(dirname "$0")/.." && pwd)

docker run --rm --user "$(id -u):$(id -g)" \
  -e HOME=/tmp -e PIP_DISABLE_PIP_VERSION_CHECK=1 -e PYTHONPATH=/tmp/py \
  -v "$ROOT":/repo -w /repo \
  python:3.13-slim \
  sh -c "pip install --quiet --root-user-action=ignore --target /tmp/py fonttools==$FONTTOOLS_VERSION brotli==$BROTLI_VERSION \
    && python scripts/subset-material-symbols.py"

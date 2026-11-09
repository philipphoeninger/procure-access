# SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# stage 1: build the Angular client with the project's own CLI (no global install)
FROM node:24.22.1-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . ./
# Release build. It calls the API on the same origin (/api), so the image is the same everywhere.
RUN npm run build:release
# The icon font is a subset in src/assets/fonts (#135, S3-25), not a bundled npm package, so the
# build does not list it: append its entry with the package's licence and the note that it is changed.
RUN { printf '%s\n' \
        'Package: material-symbols (Material Symbols Outlined, cut down by ProcureAccess to the icons it uses)' \
        'License: "Apache-2.0"' ''; \
      cat node_modules/material-symbols/LICENSE; \
      printf '\n%s\n' '--------------------------------------------------------------------------------'; \
    } >> dist/procure-access/3rdpartylicenses.txt

# stage 2: serve with nginx
FROM nginx:1.30-alpine AS final
EXPOSE 80
COPY --from=build /app/dist/procure-access/browser /usr/share/nginx/html
# Licence texts of the bundled packages and of the icon font, served as /3rdpartylicenses.txt
COPY --from=build /app/dist/procure-access/3rdpartylicenses.txt /usr/share/nginx/html/
COPY nginx.conf /etc/nginx/conf.d/default.conf

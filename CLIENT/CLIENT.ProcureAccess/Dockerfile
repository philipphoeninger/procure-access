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

# stage 2: serve with nginx
FROM nginx:1.30-alpine AS final
EXPOSE 80
COPY --from=build /app/dist/procure-access/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

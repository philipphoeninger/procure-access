# SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
#
# SPDX-License-Identifier: MIT

# stage 1: build the Angular client with the project's own CLI (no global install)
FROM node:24.22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . ./
# Angular build configuration(s), set through CLIENT_BUILD_CONFIGURATION in .env:
# "production" calls the production API, "production,local" the local API (localhost:5000).
ARG NG_CONFIGURATION=production
RUN npm run ng -- build --configuration="${NG_CONFIGURATION}"

# stage 2: serve with nginx
FROM nginx:1.30-alpine AS final
EXPOSE 80
COPY --from=build /app/dist/procure-access/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

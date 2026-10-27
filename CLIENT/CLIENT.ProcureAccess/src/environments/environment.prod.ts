// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Release build. The API is called on the same origin: the client's nginx forwards /api/ to
// the API container (nginx.conf), so one image works in the local stack and in Dokploy.
export const environment = {
  production: true,
  apiUrl: '/api'
};

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Release build for the local docker-compose stack (`--configuration=production,local`):
// optimised like production, but calls the local API instead of the production API.
export const environment = {
  production: true,
  apiUrl: 'http://localhost:5000/api'
};

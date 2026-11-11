// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Release build. The API is called on the same origin: the client's nginx forwards /api/ to
// the API container (nginx.conf), so one image works in the local stack and in Dokploy.
export const environment = {
  production: true,
  apiUrl: '/api',
  // Channel of „Kriterium vorschlagen“ (R-SUG-01, SPEC `Contact:CriteriaSuggestion`): `mailto:<address>`
  // or an https URL. 🔧 Placeholder until O-6 is decided: an address on the reserved domain
  // `.invalid`, which scripts/check-release-config.mjs refuses for a production release.
  criteriaSuggestion: 'mailto:kriterium-vorschlagen@example.invalid',
  // Range of a user weight for a B criterion (R-LST-03). 🔧 Confirmed default (SPEC SD23).
  priorityWeight: { min: 1, max: 10 }
};

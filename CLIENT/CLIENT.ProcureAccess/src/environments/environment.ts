// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

export const environment = {
  production: false,
  // Relative in every environment (S5-08, #317): the dev server proxies /api to the API
  // (proxy.conf.json), the release build reaches it through nginx. The session cookie is
  // SameSite=Strict and Angular sends the antiforgery header only to a relative URL.
  apiUrl: '/api',
  // Channel of „Kriterium vorschlagen“ (R-SUG-01, SPEC `Contact:CriteriaSuggestion`): `mailto:<address>`
  // or an https URL. 🔧 Placeholder until O-6 is decided: an address on the reserved domain
  // `.invalid`, which scripts/check-release-config.mjs refuses for a production release.
  criteriaSuggestion: 'mailto:kriterium-vorschlagen@example.invalid',
  // Range of a user weight for a B criterion (R-LST-03). 🔧 Confirmed default (SPEC SD23).
  priorityWeight: { min: 1, max: 10 }
};

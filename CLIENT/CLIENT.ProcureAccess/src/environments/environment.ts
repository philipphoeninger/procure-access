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
  // Shortest password of the registration (R-AUTH-01). 🔧 Confirmed default (SPEC SD23); keep it in
  // step with the server's Account:Password:MinLength, which decides.
  passwordMinLength: 12,
  // Sign-in lockout (R-AUTH-03), for the fixed note under a refused sign-in. 🔧 Confirmed defaults
  // (SPEC SD23); keep them in step with the server's Account:Lockout:MaxFailedAttempts and :Duration.
  lockout: { maxFailedAttempts: 5, minutes: 15 }
};

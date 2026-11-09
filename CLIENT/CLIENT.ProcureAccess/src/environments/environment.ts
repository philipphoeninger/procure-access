// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  // Channel of „Kriterium vorschlagen“ (R-SUG-01, SPEC `Contact:CriteriaSuggestion`): `mailto:<address>`
  // or an https URL. 🔧 Placeholder until O-6 is decided: an address on the reserved domain
  // `.invalid`, which scripts/check-release-config.mjs refuses for a production release.
  criteriaSuggestion: 'mailto:kriterium-vorschlagen@example.invalid'
};

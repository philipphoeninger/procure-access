// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// @ts-check
// ESLint for the Angular client (TESTING §3.2, §8.2 stage 1). Run with `npm run lint`.
//
// Templates (S1-20): Angular's recommended template rules and the template accessibility rules.
// TypeScript (S2-14, #119): typescript-eslint `recommendedTypeChecked` (type-aware, so it finds
// unhandled promises and unsafe `any`) and Angular's `tsRecommended`.
const fs = require('node:fs');
const path = require('node:path');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

// Legacy code that a migration step replaces, reworks or retires keeps the findings that are not
// mechanical to fix; the entries are grouped by that step. Each entry turns off rules for exactly
// one file: no globs, and never a file written since sprint 02 or a file no step replaces — those
// are fixed instead. When a step replaces a file, delete its entry; when the list is empty, close
// the tracking issue #158.
/** @type {Record<string, string[]>} */
const LEGACY_EXCEPTIONS = {
  // M1 — shell (S2-15 – S2-17)
  'src/app/layout/header/header.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
  ],
  // M2 — catalog and configurator (withCatalog replaces withCriteria / withFilters)
  'src/app/features/criteria/list/criteria-list.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/criteria/services/api/criteria-api.service.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/filters/selection/filters-selection.ts': [
    '@typescript-eslint/no-misused-promises',
    '@typescript-eslint/no-unsafe-argument',
    '@typescript-eslint/no-unsafe-assignment',
  ],
  'src/app/features/filters/services/api/filters-api.service.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/filters/state/with-filters.ts': [
    '@typescript-eslint/no-unsafe-enum-comparison',
  ],
  // M3 — cookie authentication, identity screens, settings S60
  'src/app/core/guards/auth.guard.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
  ],
  'src/app/features/identity/directives/has-not-permission.directive.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
  ],
  'src/app/features/identity/directives/has-permission.directive.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
  ],
  'src/app/features/identity/guards/permission.guard.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-argument',
    '@typescript-eslint/no-unsafe-assignment',
  ],
  'src/app/features/identity/login/login.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
  ],
  'src/app/features/identity/pages/email-confirmed/email-confirmation.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
    '@typescript-eslint/no-unsafe-assignment',
  ],
  'src/app/features/identity/profile/profile.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-assignment',
    '@typescript-eslint/no-unsafe-member-access',
  ],
  'src/app/features/identity/register/register.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-argument',
    '@typescript-eslint/unbound-method',
  ],
  'src/app/features/identity/reset-password/reset-password.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-assignment',
  ],
  'src/app/features/identity/services/auth.service.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-assignment',
    '@typescript-eslint/no-unsafe-member-access',
    '@typescript-eslint/no-unsafe-return',
    '@typescript-eslint/only-throw-error',
  ],
  'src/app/features/identity/services/identity-api.service.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-explicit-any',
  ],
  'src/app/features/identity/state/with-identity.ts': [
    '@typescript-eslint/no-unsafe-assignment',
  ],
  'src/app/features/settings/services/language.service.ts': [
    '@typescript-eslint/no-unsafe-assignment',
    '@typescript-eslint/no-unsafe-return',
  ],
  'src/app/features/settings/services/settings-api.service.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/settings/settings.ts': [
    '@typescript-eslint/no-floating-promises',
  ],
  'src/app/features/settings/state/with-settings.ts': [
    '@typescript-eslint/no-unsafe-argument',
  ],
  // M5 — proposal workflow retired, withProducts / withFavorites reworked (M5.6); the store root loads them
  'src/app/core/state/app.store.ts': [
    '@typescript-eslint/no-floating-promises',
  ],
  'src/app/features/criteria/proposal/criterion-proposal.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-misused-promises',
    '@typescript-eslint/no-non-null-asserted-optional-chain',
  ],
  'src/app/features/favorites/list/favorites.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/products/details/product-details.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/products/list/products-list.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-unsafe-argument',
  ],
  'src/app/features/products/pages/products-container/products-container.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/unbound-method',
  ],
  'src/app/features/products/proposal/product-proposal.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
    '@typescript-eslint/no-misused-promises',
    '@typescript-eslint/no-non-null-asserted-optional-chain',
  ],
  'src/app/features/products/services/api/products-api.service.ts': [
    '@angular-eslint/prefer-inject',
  ],
  'src/app/features/products/state/with-products.spec.ts': [
    '@typescript-eslint/prefer-promise-reject-errors',
  ],
  'src/app/features/proposal/list/proposal-list.ts': [
    '@angular-eslint/prefer-inject',
    '@typescript-eslint/no-floating-promises',
  ],
  'src/app/features/proposal/services/api/proposal-api.service.ts': [
    '@angular-eslint/prefer-inject',
  ],
};

for (const file of Object.keys(LEGACY_EXCEPTIONS)) {
  if (!fs.existsSync(path.join(__dirname, file))) {
    throw new Error(`eslint.config.js: legacy exception for a missing file, remove it: ${file}`);
  }
}

module.exports = defineConfig(
  {
    ignores: ['dist/', 'coverage/', '.angular/'],
  },
  {
    files: ['**/*.ts'],
    extends: [tseslint.configs.recommendedTypeChecked, angular.configs.tsRecommended],
    languageOptions: {
      parserOptions: { projectService: true, tsconfigRootDir: __dirname },
    },
    processor: angular.processInlineTemplates,
  },
  ...Object.entries(LEGACY_EXCEPTIONS).map(([file, rules]) => ({
    files: [file],
    rules: Object.fromEntries(rules.map((rule) => [rule, 'off'])),
  })),
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {
      // `x == null` (null or undefined) stays allowed: switching it to `===` would change behaviour.
      '@angular-eslint/template/eqeqeq': ['error', { allowNullOrUndefined: true }],
    },
  },
);

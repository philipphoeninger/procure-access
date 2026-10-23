// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// @ts-check
// ESLint for the Angular client (TESTING §3.2, §8.2 stage 1). Run with `npm run lint`.
//
// Step 0 (S1-20): only the template rules — Angular's recommended template rules and the
// template accessibility rules. TypeScript files are parsed only to lint their inline
// templates; TypeScript rules follow in M1.
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');

module.exports = defineConfig(
  {
    ignores: ['dist/', 'coverage/', '.angular/'],
  },
  {
    files: ['**/*.ts'],
    extends: [tseslint.configs.base],
    processor: angular.processInlineTemplates,
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
    rules: {
      // `x == null` (null or undefined) stays allowed: switching it to `===` would change behaviour.
      '@angular-eslint/template/eqeqeq': ['error', { allowNullOrUndefined: true }],
    },
  },
);

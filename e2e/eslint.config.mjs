// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// ESLint for the Playwright tests in e2e/ and the CI scripts in scripts/ (#161, S3-26; TESTING V118).
// Run with `npm run lint` in e2e/, which starts ESLint in the repository root with this file:
// ESLint lints no file outside the folder it starts from, and scripts/ has no package.json of its own.
// So the `files` patterns below are relative to the repository root.
//
// e2e/: typescript-eslint `recommendedTypeChecked`, as in the client (S2-14). Its type-aware rule
// `no-floating-promises` reports a Playwright `expect(...)` without `await` (owner, O-S3-8: no
// eslint-plugin-playwright).
// scripts/: `@eslint/js` recommended plus the two type-aware promise rules (owner, S3-26 (c)); the
// full type-aware set would mostly report the untyped results of `JSON.parse` in plain JavaScript.
import js from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';

export default defineConfig(
  {
    ignores: ['e2e/node_modules/', 'e2e/test-results/', 'e2e/playwright-report/', 'e2e/blob-report/'],
  },
  {
    files: ['e2e/**/*.ts'],
    extends: [tseslint.configs.recommendedTypeChecked],
    languageOptions: {
      parserOptions: { projectService: true, tsconfigRootDir: import.meta.dirname },
    },
  },
  {
    files: ['scripts/*.mjs', 'e2e/*.mjs'],
    extends: [js.configs.recommended],
    plugins: { '@typescript-eslint': tseslint.plugin },
    languageOptions: {
      parser: tseslint.parser,
      parserOptions: { project: './tsconfig.scripts.json', tsconfigRootDir: import.meta.dirname },
      // The only Node globals the scripts use; `no-undef` reports any other.
      globals: { console: 'readonly', process: 'readonly' },
    },
    rules: {
      '@typescript-eslint/no-floating-promises': [
        'error',
        {
          // node:test runs the promise that `test()` returns itself; the self-tests never await it.
          allowForKnownSafeCalls: [
            { from: 'package', package: 'node:test', name: ['test', 'describe', 'it', 'suite'] },
          ],
        },
      ],
      '@typescript-eslint/no-misused-promises': 'error',
    },
  },
);

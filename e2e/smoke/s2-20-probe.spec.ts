// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';

// S2-20 CI probe (throwaway draft PR, never merged): fails on purpose to show that the
// Playwright browser cache is saved before the tests run (#105).
test('S2-20 probe fails on purpose', () => {
  expect(1).toBe(2);
});

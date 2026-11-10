// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import axe from 'axe-core';

/**
 * axe in component tests (TESTING §5.1, §8.2 test stage; owner decision S4-10: axe-core directly
 * instead of `vitest-axe`). Same blocking tags as the E2E helper `e2e/support/axe.ts`.
 * jsdom has no layout, so colour contrast is off (E2E checks it in both appearances, A11Y-01,
 * A11Y-08). Supplementary only: E2E stays the blocking axe check of pages.
 */
const OPTIONS: axe.RunOptions = {
  runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] },
  rules: { 'color-contrast': { enabled: false } },
  resultTypes: ['violations'],
};

/** Fails with one line per violating element: `<rule>: <selector> — <help>`. */
export async function expectNoAxeViolations(context: Element = document.body): Promise<void> {
  const { violations } = await axe.run(context, OPTIONS);
  const lines = violations.flatMap((violation) =>
    violation.nodes.map((node) => `${violation.id}: ${node.target.join(' ')} — ${violation.help}`),
  );
  expect(lines).toEqual([]);
}

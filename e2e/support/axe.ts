// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import AxeBuilder from '@axe-core/playwright';
import { expect, type Page, type TestInfo } from '@playwright/test';
import type { NodeResult, Result } from 'axe-core';
import { axeSuppressions, validateAxeSuppressions, type AxeSuppression } from './axe-suppressions';

/** WCAG 2.0/2.1 A and AA: the level EN 301 549 and BITV 2.0 refer to. Violations block (A11Y-01). */
export const BLOCKING_TAGS = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'] as const;
/** WCAG 2.2 AA: reported in the test report, never blocking (TESTING §3.3). */
export const REPORTED_TAGS = ['wcag22aa'] as const;

export interface AxeCheckOptions {
  /** Replaces the central list; only for the helper's own tests. */
  readonly suppressions?: readonly AxeSuppression[];
}

interface Finding {
  readonly rule: string;
  readonly impact: string | null | undefined;
  readonly help: string;
  readonly helpUrl: string;
  readonly target: string;
  readonly html: string;
}

interface SuppressedFinding extends Finding {
  readonly issue: string;
}

/**
 * Runs axe on the current state of the page and fails if a WCAG 2.0/2.1 A/AA violation is left
 * after the suppressions (TESTING §5.4: call it after every significant state change).
 *
 * The full result (blocking, suppressed, WCAG 2.2 findings) is attached to the test as
 * `axe-<state>.json`; suppressed and WCAG 2.2 findings are also listed as annotations.
 */
export async function expectNoAxeViolations(
  page: Page,
  testInfo: TestInfo,
  state: string,
  options: AxeCheckOptions = {},
): Promise<void> {
  const suppressions = options.suppressions ?? axeSuppressions;
  validateAxeSuppressions(suppressions);

  const results = await new AxeBuilder({ page }).withTags([...BLOCKING_TAGS, ...REPORTED_TAGS]).analyze();

  const blocking: Finding[] = [];
  const suppressed: SuppressedFinding[] = [];
  const reported: Finding[] = [];

  for (const violation of results.violations) {
    const isBlocking = violation.tags.some((tag) => (BLOCKING_TAGS as readonly string[]).includes(tag));
    for (const node of violation.nodes) {
      const finding = toFinding(violation, node);
      if (!isBlocking) {
        reported.push(finding);
        continue;
      }
      const suppression = await findSuppression(page, violation.id, node, suppressions);
      if (suppression) {
        suppressed.push({ ...finding, issue: suppression.issue });
      } else {
        blocking.push(finding);
      }
    }
  }

  await testInfo.attach(`axe-${slug(state)}.json`, {
    body: JSON.stringify({ state, url: page.url(), blocking, suppressed, reported }, null, 2),
    contentType: 'application/json',
  });
  for (const finding of suppressed) {
    testInfo.annotations.push({
      type: 'axe-suppressed',
      description: `${state}: ${finding.rule} on ${finding.target} (${finding.issue})`,
    });
  }
  for (const finding of reported) {
    testInfo.annotations.push({
      type: 'axe-wcag22',
      description: `${state}: ${finding.rule} on ${finding.target}`,
    });
  }

  expect(blocking, formatFailure(state, page.url(), blocking)).toEqual([]);
}

function toFinding(violation: Result, node: NodeResult): Finding {
  return {
    rule: violation.id,
    impact: violation.impact,
    help: violation.help,
    helpUrl: violation.helpUrl,
    target: JSON.stringify(node.target),
    html: node.html,
  };
}

/**
 * A node is suppressed when an entry has the same rule and the node's element matches the
 * entry's selector or lies inside a match. Nodes inside iframes or shadow roots are never
 * suppressed (their target is not a single document selector).
 */
async function findSuppression(
  page: Page,
  rule: string,
  node: NodeResult,
  suppressions: readonly AxeSuppression[],
): Promise<AxeSuppression | undefined> {
  const candidates = suppressions.filter((entry) => entry.rule === rule);
  const target = node.target;
  if (candidates.length === 0 || target.length !== 1 || typeof target[0] !== 'string') {
    return undefined;
  }

  const matches = await page.evaluate(
    ({ elementSelector, selectors }) => {
      const element = document.querySelector(elementSelector);
      return selectors.map((selector) => element !== null && element.closest(selector) !== null);
    },
    { elementSelector: target[0], selectors: candidates.map((entry) => entry.selector) },
  );
  return candidates.find((_, index) => matches[index]);
}

function formatFailure(state: string, url: string, blocking: readonly Finding[]): string {
  const lines = blocking.map(
    (finding) =>
      `- ${finding.rule} (${finding.impact ?? 'n/a'}): ${finding.help}\n` +
      `    target ${finding.target}\n    html   ${finding.html}\n    ${finding.helpUrl}`,
  );
  return (
    `axe found ${blocking.length} WCAG 2.0/2.1 A/AA violation(s) in state "${state}" (${url}).\n` +
    'Fix them, or suppress by rule and selector with an issue link in e2e/support/axe-suppressions.ts.\n' +
    lines.join('\n')
  );
}

function slug(text: string): string {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'state';
}

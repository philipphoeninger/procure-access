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

/** The two appearances of SPEC §7.5; every axe check runs in both (A11Y-08). */
export type Appearance = 'light' | 'dark';

/**
 * Runs axe on the current state of the page in both appearances and fails if a WCAG 2.0/2.1 A/AA
 * violation is left after the suppressions in either of them (TESTING §5.4: call it after every
 * significant state change; A11Y-01 and A11Y-08).
 *
 * The first run uses the appearance the page shows; then the helper switches the emulated
 * `prefers-color-scheme` to the other one, waits until the page has followed it, runs axe again,
 * and switches back, so the test goes on in the state it was in. The same suppressions apply
 * in both appearances.
 *
 * The full result of each run (blocking, suppressed, WCAG 2.2 findings) is attached to the test
 * as `axe-<state>-<appearance>.json`; suppressed and WCAG 2.2 findings are also listed as
 * annotations.
 */
export async function expectNoAxeViolations(
  page: Page,
  testInfo: TestInfo,
  state: string,
  options: AxeCheckOptions = {},
): Promise<void> {
  const suppressions = options.suppressions ?? axeSuppressions;
  validateAxeSuppressions(suppressions);

  const current = await shownAppearance(page);
  const other: Appearance = current === 'light' ? 'dark' : 'light';

  const failures: string[] = [];
  for (const appearance of [current, other]) {
    if (appearance !== current) await showAppearance(page, appearance);
    const blocking = await checkOneAppearance(page, testInfo, state, appearance, suppressions);
    if (blocking.length > 0) failures.push(formatFailure(state, appearance, page.url(), blocking));
  }
  await showAppearance(page, current);

  expect(failures, failures.join('\n')).toEqual([]);
}

async function checkOneAppearance(
  page: Page,
  testInfo: TestInfo,
  state: string,
  appearance: Appearance,
  suppressions: readonly AxeSuppression[],
): Promise<Finding[]> {
  const results = await new AxeBuilder({ page }).withTags([...BLOCKING_TAGS, ...REPORTED_TAGS]).analyze();

  const blocking: Finding[] = [];
  const suppressed: SuppressedFinding[] = [];
  const reported: Finding[] = [];

  for (const violation of results.violations) {
    const isBlocking = violation.tags.some((tag) => (BLOCKING_TAGS as readonly string[]).includes(tag));
    if (!isBlocking) {
      reported.push(...violation.nodes.map((node) => toFinding(violation, node)));
      continue;
    }
    const entries = suppressions.filter((entry) => entry.rule === violation.id);
    if (entries.length === 0) {
      blocking.push(...violation.nodes.map((node) => toFinding(violation, node)));
      continue;
    }

    // The nodes of the rerun block. The nodes of the first run that the rerun no longer reports
    // are listed as suppressed (for the report only).
    const remaining = await nodesOutsideSuppressions(page, violation.id, entries);
    const remainingTargets = new Set(remaining.map((node) => JSON.stringify(node.target)));
    const issues = entries.map((entry) => entry.issue).join(', ');
    blocking.push(...remaining.map((node) => toFinding(violation, node)));
    for (const node of violation.nodes) {
      if (!remainingTargets.has(JSON.stringify(node.target))) {
        suppressed.push({ ...toFinding(violation, node), issue: issues });
      }
    }
  }

  await testInfo.attach(`axe-${slug(state)}-${appearance}.json`, {
    body: JSON.stringify({ state, appearance, url: page.url(), blocking, suppressed, reported }, null, 2),
    contentType: 'application/json',
  });
  for (const finding of suppressed) {
    testInfo.annotations.push({
      type: 'axe-suppressed',
      description: `${state} (${appearance}): ${finding.rule} on ${finding.target} (${finding.issue})`,
    });
  }
  for (const finding of reported) {
    testInfo.annotations.push({
      type: 'axe-wcag22',
      description: `${state} (${appearance}): ${finding.rule} on ${finding.target}`,
    });
  }

  return blocking;
}

/** The appearance the page shows: its `prefers-color-scheme` as the page sees it. */
async function shownAppearance(page: Page): Promise<Appearance> {
  const dark = await page.evaluate(() => matchMedia('(prefers-color-scheme: dark)').matches);
  return dark ? 'dark' : 'light';
}

/**
 * Emulates the appearance and waits until the page shows it: the app's class on `<html>`
 * (`theme-light`/`theme-dark`, set by `AppearanceService`; pages without it, such as the helper's
 * fixtures, only follow the media query) and the end of the colour transitions it started, so
 * axe never measures a colour halfway.
 */
async function showAppearance(page: Page, appearance: Appearance): Promise<void> {
  await page.emulateMedia({ colorScheme: appearance });
  await expect
    .poll(
      () =>
        page.evaluate((wanted) => {
          const classes = document.documentElement.classList;
          const themed = classes.contains('theme-light') || classes.contains('theme-dark');
          return !themed || classes.contains(`theme-${wanted}`);
        }, appearance),
      { message: `the page did not switch to the ${appearance} appearance` },
    )
    .toBe(true);
  await page.evaluate(async () => {
    await new Promise((resolve) => requestAnimationFrame(() => requestAnimationFrame(resolve)));
    const transitions = document.getAnimations().filter((animation) => animation instanceof CSSTransition);
    await Promise.all(transitions.map((transition) => transition.finished.catch(() => undefined)));
  });
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
 * Runs the one rule again with the entries' selectors excluded, and returns the nodes it still
 * reports. axe resolves the selectors in the same run in which it checks the elements, so a
 * re-render between the runs cannot make the wrong element suppressed. An excluded element's
 * descendants are excluded too ("matches the selector or lies inside a match"). The selectors
 * are resolved in the top document only, so nodes inside iframes are never suppressed.
 */
async function nodesOutsideSuppressions(
  page: Page,
  rule: string,
  entries: readonly AxeSuppression[],
): Promise<NodeResult[]> {
  let builder = new AxeBuilder({ page }).withRules([rule]);
  for (const entry of entries) {
    builder = builder.exclude(entry.selector);
  }
  const results = await builder.analyze();
  return results.violations.find((violation) => violation.id === rule)?.nodes ?? [];
}

function formatFailure(state: string, appearance: Appearance, url: string, blocking: readonly Finding[]): string {
  const lines = blocking.map(
    (finding) =>
      `- ${finding.rule} (${finding.impact ?? 'n/a'}): ${finding.help}\n` +
      `    target ${finding.target}\n    html   ${finding.html}\n    ${finding.helpUrl}`,
  );
  return (
    `axe found ${blocking.length} WCAG 2.0/2.1 A/AA violation(s) in state "${state}", ${appearance} appearance (${url}).\n` +
    'Fix them, or suppress by rule and selector with an issue link in e2e/support/axe-suppressions.ts.\n' +
    lines.join('\n')
  );
}

function slug(text: string): string {
  return text.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '') || 'state';
}

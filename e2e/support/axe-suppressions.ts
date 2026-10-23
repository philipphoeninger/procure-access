// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The central list of suppressed axe violations (TESTING §9 step 0, §12.4).
 *
 * Rules for every entry:
 * - suppress one axe rule on the elements matched by one CSS selector, never a rule globally;
 * - link the GitHub issue that tracks the fix;
 * - say in one sentence why the violation cannot be fixed now.
 *
 * The list is reviewed at the end of every migration step and must be empty before the pilot.
 * `validateAxeSuppressions` enforces the format; the helper self-test runs it on this list.
 */
export interface AxeSuppression {
  /** axe rule id, e.g. "button-name" */
  readonly rule: string;
  /** CSS selector; an element is suppressed if it matches or lies inside a match */
  readonly selector: string;
  /** GitHub issue that tracks the fix */
  readonly issue: string;
  /** why the violation is suppressed instead of fixed */
  readonly reason: string;
}

export const ISSUE_URL_PATTERN = /^https:\/\/github\.com\/philipphoeninger\/procure-access\/issues\/\d+$/;

export const axeSuppressions: readonly AxeSuppression[] = [
  {
    rule: 'aria-tab-name',
    selector: 'pa-products-container mat-step-header',
    issue: 'https://github.com/philipphoeninger/procure-access/issues/52',
    reason:
      'The two stepper steps on /filters have no label, and the UX copy has no wording for them; ' +
      'the criteria selection is rebuilt as S10 in migration step 2.',
  },
];

/** Throws if an entry is incomplete, has no issue link, or repeats another entry. */
export function validateAxeSuppressions(suppressions: readonly AxeSuppression[]): void {
  const problems: string[] = [];
  const seen = new Set<string>();

  suppressions.forEach((entry, index) => {
    const label = `axe suppression #${index + 1} (${entry.rule || '?'} on ${entry.selector || '?'})`;
    if (!entry.rule?.trim()) problems.push(`${label}: rule is missing`);
    if (!entry.selector?.trim()) problems.push(`${label}: selector is missing`);
    if (!entry.reason?.trim()) problems.push(`${label}: reason is missing`);
    if (!ISSUE_URL_PATTERN.test(entry.issue ?? '')) {
      problems.push(`${label}: issue must be a link like https://github.com/philipphoeninger/procure-access/issues/123`);
    }
    const key = `${entry.rule}\u0000${entry.selector}`;
    if (seen.has(key)) problems.push(`${label}: duplicate entry`);
    seen.add(key);
  });

  if (problems.length > 0) {
    throw new Error(`Invalid axe suppression list:\n- ${problems.join('\n- ')}`);
  }
}

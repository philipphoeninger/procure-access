// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, test } from '@playwright/test';
import { expectNoAxeViolations } from './axe';
import { axeSuppressions, validateAxeSuppressions, type AxeSuppression } from './axe-suppressions';

/**
 * Self-test of the axe helper on a small fixed page (no stack needed for the page itself).
 * The page has exactly two WCAG 2.0 A violations: an image without alt text (image-alt)
 * and a button without an accessible name (button-name).
 */
const pageWithTwoViolations = `
  <!doctype html>
  <html lang="de">
    <head><title>Axe helper fixture</title></head>
    <body>
      <main>
        <h1>Fixture</h1>
        <img id="no-alt" src="data:image/gif;base64,R0lGODlhAQABAAAAACw=" width="10" height="10">
        <button id="no-name" type="button"></button>
      </main>
    </body>
  </html>`;

const cleanPage = `
  <!doctype html>
  <html lang="de">
    <head><title>Axe helper fixture</title></head>
    <body><main><h1>Fixture</h1><button type="button">Weiter</button></main></body>
  </html>`;

const issue = 'https://github.com/philipphoeninger/procure-access/issues/1';

function suppression(rule: string, selector: string): AxeSuppression {
  return { rule, selector, issue, reason: 'Fixture for the helper self-test.' };
}

test.describe('axe helper', () => {
  test('passes on a page without violations', async ({ page }, testInfo) => {
    await page.setContent(cleanPage);

    await expectNoAxeViolations(page, testInfo, 'clean page', { suppressions: [] });
  });

  test('fails and names every rule and element when violations are not suppressed', async ({ page }, testInfo) => {
    await page.setContent(pageWithTwoViolations);

    const check = expectNoAxeViolations(page, testInfo, 'two violations', { suppressions: [] });

    await expect(check).rejects.toThrow(/image-alt[\s\S]*#no-alt/);
    await expect(check).rejects.toThrow(/button-name[\s\S]*#no-name/);
  });

  test('suppresses only the rule on the elements the selector matches', async ({ page }, testInfo) => {
    await page.setContent(pageWithTwoViolations);

    const check = expectNoAxeViolations(page, testInfo, 'one suppressed', {
      suppressions: [suppression('image-alt', '#no-alt'), suppression('button-name', '#some-other-element')],
    });

    await expect(check).rejects.toThrow(/button-name/);
    await expect(check).rejects.not.toThrow(/image-alt/);
    expect(testInfo.annotations).toContainEqual(
      expect.objectContaining({ type: 'axe-suppressed', description: expect.stringContaining(issue) }),
    );
  });

  test('suppresses an element inside the matched container, but not another rule there', async ({ page }, testInfo) => {
    await page.setContent(pageWithTwoViolations);

    const check = expectNoAxeViolations(page, testInfo, 'container suppressed', {
      suppressions: [suppression('image-alt', 'main')],
    });

    await expect(check).rejects.toThrow(/button-name/);
    await expect(check).rejects.not.toThrow(/image-alt/);
  });

  test('passes when every violation is suppressed', async ({ page }, testInfo) => {
    await page.setContent(pageWithTwoViolations);

    await expectNoAxeViolations(page, testInfo, 'all suppressed', {
      suppressions: [suppression('image-alt', '#no-alt'), suppression('button-name', '#no-name')],
    });
  });
});

test.describe('axe suppression list', () => {
  test('the central list is valid', () => {
    expect(() => validateAxeSuppressions(axeSuppressions)).not.toThrow();
  });

  test('rejects an entry without a link to an issue of this repository', () => {
    const entries: AxeSuppression[] = [
      { ...suppression('image-alt', '#no-alt'), issue: '' },
      { ...suppression('button-name', '#no-name'), issue: 'https://example.org/issues/1' },
    ];

    expect(() => validateAxeSuppressions(entries)).toThrow(/#1 \(image-alt[\s\S]*issue must be a link/);
    expect(() => validateAxeSuppressions(entries)).toThrow(/#2 \(button-name[\s\S]*issue must be a link/);
  });

  test('rejects an entry without rule, selector or reason, and duplicates', () => {
    expect(() => validateAxeSuppressions([{ ...suppression('', '#a') }])).toThrow(/rule is missing/);
    expect(() => validateAxeSuppressions([{ ...suppression('image-alt', ' ') }])).toThrow(/selector is missing/);
    expect(() => validateAxeSuppressions([{ ...suppression('image-alt', '#a'), reason: '' }])).toThrow(/reason is missing/);
    expect(() =>
      validateAxeSuppressions([suppression('image-alt', '#a'), suppression('image-alt', '#a')]),
    ).toThrow(/duplicate entry/);
  });

  test('the helper refuses to run with an invalid list', async ({ page }, testInfo) => {
    await page.setContent(cleanPage);

    await expect(
      expectNoAxeViolations(page, testInfo, 'invalid list', {
        suppressions: [{ ...suppression('image-alt', '#no-alt'), issue: 'TODO' }],
      }),
    ).rejects.toThrow(/Invalid axe suppression list/);
  });
});

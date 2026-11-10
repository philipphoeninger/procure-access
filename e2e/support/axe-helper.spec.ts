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

/**
 * The button has its own colours: how a native button looks in the dark appearance is up to the
 * browser, and since Playwright 1.65 axe reports WebKit's unstyled button there as a contrast
 * violation. The fixture tests the helper, not the browser's defaults.
 */
const cleanPage = `
  <!doctype html>
  <html lang="de">
    <head>
      <title>Axe helper fixture</title>
      <style>button { color: #000; background: #fff; border: 1px solid #000; }</style>
    </head>
    <body><main><h1>Fixture</h1><button type="button">Weiter</button></main></body>
  </html>`;

/** Readable in light; in dark the text is grey on dark grey (about 1.4:1), a color-contrast violation. */
const pageFailingOnlyInDark = `
  <!doctype html>
  <html lang="de">
    <head>
      <title>Axe helper fixture</title>
      <style>
        p { color: #000; background: #fff; }
        @media (prefers-color-scheme: dark) { p#dark-only { color: #555; background: #333; } }
      </style>
    </head>
    <body><main><h1>Fixture</h1><p id="dark-only">Nur im Dunkelmodus schwer lesbar</p></main></body>
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

  test('INV:A11Y-08 fails on a contrast violation that only the dark appearance shows', async ({ page }, testInfo) => {
    await page.setContent(pageFailingOnlyInDark);

    const check = expectNoAxeViolations(page, testInfo, 'dark only', { suppressions: [] });

    await expect(check).rejects.toThrow(/dark appearance[\s\S]*color-contrast[\s\S]*#dark-only/);
    await expect(check).rejects.not.toThrow(/light appearance/);
  });

  test('INV:A11Y-08 starts in the appearance the page shows and returns to it', async ({ page }, testInfo) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.setContent(cleanPage);

    await expectNoAxeViolations(page, testInfo, 'clean page', { suppressions: [] });

    expect(await page.evaluate(() => matchMedia('(prefers-color-scheme: dark)').matches)).toBe(true);
    expect(testInfo.attachments.map((attachment) => attachment.name)).toEqual([
      'axe-clean-page-dark.json',
      'axe-clean-page-light.json',
    ]);
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

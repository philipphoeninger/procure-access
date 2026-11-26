// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type Locator, type Page } from '@playwright/test';

/**
 * The keyboard and waiting helpers the specs share (TD-99, S7-01): one copy instead of one per spec.
 * Each fails with the target's name when the keyboard does not reach it, so a control that leaves
 * the tab order fails the test instead of a later click.
 */

/** Tab presses before {@link tabTo} gives up: S11 with its cards and S34b with the legend have many stops. */
const MAX_TABS = 80;

/**
 * The longest wait for a requested download (`page.waitForEvent('download', { timeout })`). An
 * export answers in about a second; without a bound, a download that never starts (Enter on the
 * wrong element) held J1 for its whole 5 minutes (#380, S6-01).
 */
export const DOWNLOAD_TIMEOUT = 30_000;

/**
 * Presses Tab (or Shift+Tab) until the target has the focus, as a keyboard user moves through the
 * page. Fails when the target is not reached, i.e. when it is not in the tab order from the focus on.
 */
export async function tabTo(page: Page, target: Locator, key: 'Tab' | 'Shift+Tab' = 'Tab'): Promise<void> {
  for (let presses = 0; presses <= MAX_TABS; presses++) {
    if (await target.evaluate((element) => element === document.activeElement)) {
      return;
    }
    await page.keyboard.press(key);
  }
  throw new Error(`${target.toString()} was not reached with ${MAX_TABS} presses of ${key}`);
}

/** Presses a key until the target has the focus, as in a menu or listbox; fails after `max` presses. */
export async function arrowTo(page: Page, target: Locator, key = 'ArrowDown', max = 10): Promise<void> {
  for (let presses = 0; presses <= max; presses++) {
    if (await target.evaluate((element) => element === document.activeElement)) {
      return;
    }
    await page.keyboard.press(key);
  }
  throw new Error(`${target.toString()} was not reached with ${max} presses of ${key}`);
}

/** The option of the open list of a `mat-select` that the arrow keys have reached. */
export const activeOption = (page: Page) => page.locator('mat-option.mat-mdc-option-active');

/**
 * Presses ArrowDown in the open list of a `mat-select` until the option with the text is active.
 * The select marks its first (or selected) option active when the list opens, but the class is
 * rendered a moment later; a press before that skipped the first option, and the list does not
 * wrap, so the target was never reached. Hence wait for the active option before the first
 * check and for each press to move it before the next one.
 */
export async function arrowToOption(page: Page, text: string): Promise<void> {
  await expect(activeOption(page)).toHaveCount(1);
  for (let presses = 0; presses <= 10; presses++) {
    const current = ((await activeOption(page).textContent()) ?? '').trim();
    if (current.startsWith(text)) {
      return;
    }
    await page.keyboard.press('ArrowDown');
    await expect(activeOption(page), `ArrowDown did not move past „${current}“ towards „${text}“`).not.toHaveText(current);
  }
  throw new Error(`The option „${text}“ was not reached with the arrow keys`);
}

/**
 * Waits until the animations and transitions of an element and its subtree are over (e.g. the
 * clicked control's state layer), so axe sees the settled colours. A transition that is cancelled
 * (the focus moved on) rejects `finished`: it is over too.
 */
export async function settled(locator: Locator): Promise<void> {
  await locator.evaluate((element) =>
    Promise.all(element.getAnimations({ subtree: true }).map((a) => a.finished.catch(() => undefined))),
  );
}

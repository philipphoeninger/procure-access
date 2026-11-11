// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Takes the first option of the open, filtered list of a Material autocomplete by keyboard
 * (ArrowDown, Enter) and waits until the field shows it.
 *
 * Material clears the highlighted option one task after the option list changed
 * (`MatAutocompleteTrigger`: `options.changes` → `delay(0)` → `_resetActiveItem()`). An ArrowDown
 * right after the last keystroke can therefore be undone before Enter, and Enter then selects
 * nothing — the field keeps the typed text. Waiting for the filtered list does not close that gap,
 * so this helper presses ArrowDown only while no option is highlighted and repeats ArrowDown and
 * Enter until the field shows `name`. Keyboard only, as the journeys require (A11Y-02).
 */
export async function chooseFirstOption(page: Page, combobox: Locator, name: string): Promise<void> {
  const first = page.getByRole('listbox').getByRole('option').first();
  await expect(first).toHaveAccessibleName(name);
  await expect(async () => {
    if ((await combobox.inputValue()) !== name) {
      if ((await combobox.getAttribute('aria-activedescendant')) === null) {
        await page.keyboard.press('ArrowDown');
      }
      const firstId = await first.getAttribute('id');
      expect(firstId).not.toBeNull();
      await expect(combobox).toHaveAttribute('aria-activedescendant', firstId ?? '', { timeout: 1_000 });
      await page.keyboard.press('Enter');
    }
    await expect(combobox).toHaveValue(name, { timeout: 1_000 });
  }).toPass({ timeout: 10_000 });
}

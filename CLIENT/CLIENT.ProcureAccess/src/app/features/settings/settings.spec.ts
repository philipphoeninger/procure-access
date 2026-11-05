// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

import { DEFAULT_LANGUAGE } from '@app/app.config';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Settings } from './settings';

// #137: the icon of the contrast button is decoration (Handoff §1.8); the button is named by its
// label alone. Only the store is replaced.
async function renderSettings(highContrastOn: boolean) {
  await render(Settings, {
    providers: [
      provideTestTranslations(),
      {
        provide: ProcureAccessStore,
        useValue: {
          language: signal(DEFAULT_LANGUAGE),
          setLanguage: vi.fn(),
          highContrastOn: signal(highContrastOn),
          toggleContrastMode: vi.fn(),
          darkModeOn: signal(false),
          toggleDarkMode: vi.fn(),
        },
      },
    ],
  });
}

describe('Settings', () => {
  it.each([
    [false, 'contrast'],
    [true, 'contrast_rtl_off'],
  ])('hides the icon of the contrast button from assistive technology (high contrast %s)', async (highContrastOn, glyph) => {
    await renderSettings(highContrastOn);

    const button = screen.getByRole('button', { name: 'Kontrastmodus:' });
    const icon = button.querySelector('mat-icon');
    expect(icon?.getAttribute('aria-hidden')).toBe('true');
    expect(icon?.textContent?.trim()).toBe(glyph);
    expect(button.querySelector('.material-symbols-outlined:not(mat-icon)')).toBeNull();
  });
});

// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { UserPreferences } from '../../models/user-preferences';
import { SettingsLoad } from './settings-load';
import { SettingsPage } from './settings-page';

const DEFAULTS: UserPreferences = { language: 'de', appearance: 'system', motion: 'system', contrast: 'system', assessmentLegend: 'auto' };

describe('SettingsPage (S60, R-SET-01)', () => {
  let preferences: ReturnType<typeof signal<UserPreferences | null>>;
  let changePreferences: ReturnType<typeof vi.fn<(next: UserPreferences) => Promise<boolean>>>;
  let announce: ReturnType<typeof vi.fn>;

  async function openPage(options: { stored?: UserPreferences; failed?: boolean } = {}) {
    preferences = signal<UserPreferences | null>(options.stored ?? DEFAULTS);
    changePreferences = vi.fn<(next: UserPreferences) => Promise<boolean>>((next) => {
      preferences.set(next);
      return Promise.resolve(true);
    });
    announce = vi.fn().mockResolvedValue(undefined);
    const load = new SettingsLoad();
    load.failed = options.failed ?? false;
    return render(SettingsPage, {
      providers: [
        provideTestTranslations(),
        { provide: SettingsLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { preferences, changePreferences } },
        { provide: A11yAnnouncerService, useValue: { announce } },
      ],
    });
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  const checked = (group: string) =>
    Array.from(screen.getByRole('group', { name: group }).querySelectorAll<HTMLInputElement>('input[type="radio"]'))
      .filter((radio) => radio.checked)
      .map((radio) => radio.value);

  it('shows the H1, the intro and the four fieldsets in the order of the Handoff', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Einstellungen' })).toBeTruthy();
    expect(screen.getByText(/gelten auf allen Geräten/)).toBeTruthy();
    expect(screen.getAllByRole('group').map((group) => group.querySelector('legend')?.textContent?.trim())).toEqual([
      'Sprache',
      'Erscheinungsbild',
      'Bewegung und Animationen',
      'Kontrast',
    ]);
    expect(screen.queryByRole('button')).toBeNull(); // no save button: a choice applies at once
    await expectNoAxeViolations();
  });

  it('offers only „Deutsch“, chosen, with its hint (SD12)', async () => {
    await openPage();

    const german = screen.getByRole('radio', { name: 'Deutsch' });
    expect((german as HTMLInputElement).checked).toBe(true);
    expect(german.closest('[role="radiogroup"]')?.getAttribute('aria-describedby')).toBe('pa-settings-language-hint');
    expect(screen.getByText('Die Oberfläche gibt es vorerst nur auf Deutsch.').id).toBe('pa-settings-language-hint');
  });

  it('offers contrast only as the system says until H2 (R-SET-04)', async () => {
    await openPage();

    expect(checked('Kontrast')).toEqual(['system']);
    expect(screen.queryByRole('radio', { name: 'Hoher Kontrast' })).toBeNull();
  });

  it('shows the stored settings as chosen', async () => {
    await openPage({ stored: { ...DEFAULTS, appearance: 'dark', motion: 'reduced' } });

    expect(checked('Erscheinungsbild')).toEqual(['dark']);
    expect(checked('Bewegung und Animationen')).toEqual(['reduced']);
  });

  it('INV:A11Y-06 ChoosingDark_ChangesAllFourAndAnnouncesTheSave', async () => {
    const { fixture } = await openPage({ stored: { ...DEFAULTS, motion: 'reduced' } });

    screen.getByRole('radio', { name: 'Dunkel' }).click();
    await settle(fixture);

    expect(changePreferences).toHaveBeenCalledWith({ ...DEFAULTS, motion: 'reduced', appearance: 'dark' });
    expect(announce).toHaveBeenCalledWith('settings.saved');
    expect(checked('Erscheinungsbild')).toEqual(['dark']);
  });

  it('ChoosingReducedMotion_KeepsTheAppearance', async () => {
    const { fixture } = await openPage({ stored: { ...DEFAULTS, appearance: 'light' } });

    screen.getByRole('radio', { name: 'Bewegungen reduzieren' }).click();
    await settle(fixture);

    expect(changePreferences).toHaveBeenCalledWith({ ...DEFAULTS, appearance: 'light', motion: 'reduced' });
  });

  it('a change that a later one follows announces nothing', async () => {
    const { fixture } = await openPage();
    changePreferences.mockResolvedValueOnce(false);

    screen.getByRole('radio', { name: 'Hell' }).click();
    await settle(fixture);

    expect(announce).not.toHaveBeenCalled();
  });

  it('a failed save announces nothing and keeps the page (D2 (a))', async () => {
    const { fixture } = await openPage();
    changePreferences.mockImplementationOnce((next) => {
      preferences.set(next);
      return Promise.reject(new Error('server error'));
    });

    screen.getByRole('radio', { name: 'Dunkel' }).click();
    await settle(fixture);

    expect(announce).not.toHaveBeenCalled();
    expect(checked('Erscheinungsbild')).toEqual(['dark']);
    expect(screen.getByRole('heading', { level: 1, name: 'Einstellungen' })).toBeTruthy();
  });

  it('a failed read shows the S80 server text in place', async () => {
    await openPage({ failed: true });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
    expect(screen.queryByRole('group')).toBeNull();
  });
});

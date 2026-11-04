// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LoadingBar } from './loading-bar';

describe('LoadingBar (R-SHL-09)', () => {
  const isLoading = signal(false);
  const announcer = { announce: vi.fn().mockResolvedValue(undefined) };

  async function renderBar() {
    isLoading.set(false);
    announcer.announce.mockClear();
    const { fixture } = await render(LoadingBar, {
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { isLoading } },
        { provide: A11yAnnouncerService, useValue: announcer },
      ],
    });
    return fixture;
  }

  it('shows nothing while nothing loads', async () => {
    await renderBar();

    expect(screen.queryByRole('progressbar')).toBeNull();
    expect(announcer.announce).not.toHaveBeenCalled();
  });

  it('shows a named progress bar and announces „Wird geladen …“ once while loading', async () => {
    const fixture = await renderBar();

    isLoading.set(true);
    await fixture.whenStable();

    expect(screen.getByRole('progressbar', { name: 'Wird geladen …' })).toBeTruthy();
    expect(announcer.announce).toHaveBeenCalledTimes(1);
    expect(announcer.announce).toHaveBeenCalledWith('common.loading');

    isLoading.set(false);
    await fixture.whenStable();
    expect(screen.queryByRole('progressbar')).toBeNull();
  });

  it('announces again when the next loading starts', async () => {
    const fixture = await renderBar();

    for (const loading of [true, false, true]) {
      isLoading.set(loading);
      await fixture.whenStable();
    }

    expect(announcer.announce).toHaveBeenCalledTimes(2);
  });
});
